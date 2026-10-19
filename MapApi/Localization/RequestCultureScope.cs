using System.Globalization;
using NewLife;

namespace MapApi;

/// <summary>请求语言作用域。按 Accept-Language 设置当前 UI 文化，释放时还原</summary>
/// <remarks>
/// 对齐 NewLife.Core 的 RequestCultureScope：仅在 <see cref="Localization.Culture"/> 为空时生效，
/// 按 q 权重选择最高权重且存在词表的语言，只修改 <see cref="CultureInfo.CurrentUICulture"/>。
/// Core 当前 beta 尚未包含该类型，先在本项目内实现，后续升级 Core 后可替换为官方实现。
/// </remarks>
internal sealed class RequestCultureScope : IDisposable
{
    private static readonly HashSet<String> _supported = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "zh-Hans", "zh-Hant", "es", "de", "ja", "fr", "pt", "ru", "ko", "ar",
    };

    private readonly CultureInfo _saved;
    private readonly Boolean _changed;

    private RequestCultureScope(CultureInfo saved, Boolean changed)
    {
        _saved = saved;
        _changed = changed;
    }

    /// <summary>按 Accept-Language 头创建请求语言作用域</summary>
    /// <param name="acceptLanguage">Accept-Language 头原始值</param>
    /// <returns>请求语言作用域；未识别到受支持语言时释放不做任何事</returns>
    public static RequestCultureScope Create(String acceptLanguage)
    {
        if (Localization.Culture != null || !TryParseAcceptLanguage(acceptLanguage, out var culture)) return new(null, false);

        var saved = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = culture;

        return new(saved, true);
    }

    /// <summary>解析 Accept-Language 头，返回权重最高且受支持的区域性</summary>
    /// <param name="acceptLanguage">Accept-Language 头原始值</param>
    /// <returns>区域性；没有受支持语言时返回 null</returns>
    public static CultureInfo ParseAcceptLanguage(String acceptLanguage)
    {
        TryParseAcceptLanguage(acceptLanguage, out var culture);

        return culture;
    }

    /// <summary>还原当前 UI 文化</summary>
    public void Dispose()
    {
        if (_changed) CultureInfo.CurrentUICulture = _saved;
    }

    private static Boolean TryParseAcceptLanguage(String acceptLanguage, out CultureInfo culture)
    {
        culture = null;
        if (acceptLanguage.IsNullOrEmpty()) return false;

        var bestQ = 0.0;
        foreach (var item in acceptLanguage.Split(','))
        {
            var parts = item.Split(';');
            var name = parts[0].Trim();
            if (name.IsNullOrEmpty() || name == "*") continue;

            var q = 1.0;
            for (var i = 1; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                if (part.StartsWith("q=", StringComparison.OrdinalIgnoreCase) &&
                    Double.TryParse(part.Substring(2), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
                    q = value;
            }
            if (q <= 0 || q <= bestQ) continue;

            try
            {
                var candidate = CultureInfo.GetCultureInfo(name);
                if (!IsSupported(candidate)) continue;

                culture = candidate;
                bestQ = q;
            }
            catch (ArgumentException) { }
        }

        return culture != null;
    }

    private static Boolean IsSupported(CultureInfo culture) => _supported.Contains(GetCultureKey(culture));

    private static String GetCultureKey(CultureInfo culture)
    {
        var name = culture.Name;
        if (name.IsNullOrEmpty()) return "en";

        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            var isHant = name.IndexOf("Hant", StringComparison.OrdinalIgnoreCase) >= 0
                || name.EndsWith("-TW", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("-HK", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("-MO", StringComparison.OrdinalIgnoreCase);
            return isHant ? "zh-Hant" : "zh-Hans";
        }

        var lang = name;
        var index = lang.IndexOf('-');
        if (index > 0) lang = lang.Substring(0, index);
        return lang.ToLowerInvariant();
    }
}
