using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>运行时多语言与带前缀日志测试</summary>
/// <remarks>本类会临时改写 <see cref="Localization.Culture"/>，与其它改写全局语言的用例串行执行</remarks>
[Collection("GlobalState")]
public class LocalizationTests
{
    static LocalizationTests()
    {
        // 语言表在模块初始化时注册，测试先显式触发，避免依赖测试执行顺序
        RuntimeHelpers.RunModuleConstructor(typeof(Map).Module.ModuleHandle);
    }

    /// <summary>记录最后一条日志的日志器</summary>
    class CaptureLog : Logger
    {
        /// <summary>最后一条日志内容</summary>
        public String Last { get; private set; }

        /// <summary>最后一条日志级别</summary>
        public LogLevel LastLevel { get; private set; }

        protected override void OnWrite(LogLevel level, String format, params Object[] args)
        {
            LastLevel = level;
            Last = format;
        }
    }

    /// <summary>测试用 Map 子类</summary>
    class TestMap : Map
    {
    }

    [Fact]
    [DisplayName("多语言_地图消息_运行时切换语言")]
    public void Translate_RuntimeCulture()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");

            var map = new TestMap();
            var ex = Assert.Throws<NotSupportedException>(() => map.ConvertAsync([new GeoPoint(1, 2)], "wgs84", "unknown").GetAwaiter().GetResult());
            Assert.Equal("未支持[wgs84]坐标系到[unknown]坐标系的转换", ex.Message);

            Localization.Culture = CultureInfo.GetCultureInfo("en-US");
            ex = Assert.Throws<NotSupportedException>(() => map.ConvertAsync([new GeoPoint(1, 2)], "wgs84", "unknown").GetAwaiter().GetResult());
            Assert.Equal("Coordinate system conversion from [wgs84] to [unknown] is not supported.", ex.Message);
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_带前缀日志_先翻译后拼接")]
    public void WriteLog_TranslatesBeforePrefix()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");

            var log = new CaptureLog();
            var map = new TestMap { Log = log };

            map.WriteLog("No available key.");

            Assert.Equal("[Test]没有可用密钥", log.Last);
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_带前缀错误日志_使用自定义前缀")]
    public void WriteError_UsesCustomPrefix()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");

            var log = new CaptureLog();
            var map = new TestMap { Log = log, LogPrefix = "[Custom]" };

            map.WriteError("No available key.");

            Assert.Equal(LogLevel.Error, log.LastLevel);
            Assert.Equal("[Custom]没有可用密钥", log.Last);
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_缺少译文_回退英文")]
    public void Translate_MissingKey_FallsBackToEnglish()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");

            Assert.Equal("Missing key.", Localization.Translate("Missing key."));
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    /// <summary>组件支持的全部语言（英文为源文与兜底，另有 10 份词表）</summary>
    public static readonly String[] Cultures = ["zh-Hans", "zh-Hant", "es", "de", "ja", "fr", "pt", "ru", "ko", "ar"];

    [Theory]
    [InlineData("zh-Hant", "[Test]沒有可用密鑰")]
    [InlineData("ja", "[Test]利用可能なキーがありません。")]
    [InlineData("de", "[Test]Kein verfügbarer Schlüssel.")]
    [InlineData("ko", "[Test]사용 가능한 키가 없습니다.")]
    [InlineData("ru", "[Test]Нет доступного ключа.")]
    [InlineData("ar", "[Test]لا يوجد مفتاح متاح.")]
    [DisplayName("多语言_带前缀日志_各语言译文")]
    public void WriteLog_LocalizedPerLanguage(String culture, String expected)
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo(culture);

            var log = new CaptureLog();
            var map = new TestMap { Log = log };

            map.WriteLog("No available key.");

            Assert.Equal(expected, log.Last);
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_十种语言词表_均已编译生效")]
    public void Translate_AllCultures_AreCompiled()
    {
        var old = Localization.Culture;
        try
        {
            const String title = "NewLife Map Component Library";
            foreach (var item in Cultures)
            {
                var culture = CultureInfo.GetCultureInfo(item);
                Assert.True(Localization.IsSupported(culture), $"{item} 词表未编译");
                Localization.Culture = culture;

                var text = Localization.Translate(title);
                Assert.False(text.IsNullOrEmpty(), $"{item} 缺少标题译文");
                Assert.NotEqual(title, text);
            }
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_程序集标题与描述_按语言翻译")]
    public void Translate_AssemblyMetadata()
    {
        var old = Localization.Culture;
        try
        {
            var assembly = typeof(Map).Assembly;
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

            Assert.Equal("NewLife Map Component Library", title);
            Assert.StartsWith("A unified map component library", description);

            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");
            Assert.Equal("统一地图组件库", Localization.Translate(title));
            Assert.StartsWith("统一封装百度、高德、腾讯、天地图", Localization.Translate(description));

            Localization.Culture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal(title, Localization.Translate(title));
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_距离单位_按语言格式化")]
    public void Translate_DistanceUnit()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");
            Assert.Equal("16米", Localization.Translate("{0} m", 16));

            Localization.Culture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal("16 m", Localization.Translate("{0} m", 16));
        }
        finally
        {
            Localization.Culture = old;
        }
    }

}
