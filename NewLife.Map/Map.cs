using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NewLife.Data;
using NewLife.Log;
using NewLife.Map.Models;
using NewLife.Reflection;
using NewLife.Serialization;
using NewLife.Threading;

#if NET45 || NET461 
using System.Net.Http;
#endif

namespace NewLife.Map;

/// <summary>地图提供者接口</summary>
public interface IMap
{
    #region 属性
    /// <summary>应用密钥。多个key逗号分隔</summary>
    String? AppKey { get; set; }
    #endregion

    #region 方法
    /// <summary>异步获取字符串</summary>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<String> GetStringAsync(String url);
    #endregion

    #region 地理编码
    /// <summary>查询地址获取坐标。将地址转换为地理位置坐标</summary>
    /// <param name="address">地址</param>
    /// <param name="city">城市</param>
    /// <param name="coordtype">所需要的坐标系</param>
    /// <param name="formatAddress">是否格式化地址</param>
    /// <returns></returns>
    Task<GeoAddress?> GetGeoAsync(String address, String? city = null, String? coordtype = null, Boolean formatAddress = false);
    #endregion

    #region 逆地理编码
    /// <summary>根据坐标获取地址。将地理位置坐标转换为直观易懂的地址的过程</summary>
    /// <param name="point">坐标</param>
    /// <param name="coordtype">坐标系</param>
    /// <returns></returns>
    Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, String? coordtype);
    #endregion

    #region 路径规划
    /// <summary>计算距离和驾车时间</summary>
    /// <param name="origin">起始坐标</param>
    /// <param name="destination">目的坐标</param>
    /// <param name="coordtype">坐标系</param>
    /// <param name="type">路径计算的方式和方法</param>
    /// <returns></returns>
    Task<Driving?> GetDistanceAsync(GeoPoint origin, GeoPoint destination, String? coordtype, Int32 type = 0);
    #endregion

    #region 坐标系转换
    /// <summary>坐标系转换</summary>
    /// <param name="points">需转换的源坐标</param>
    /// <param name="from">源坐标类型。wgs84ll/gcj02/bd09ll</param>
    /// <param name="to">目标坐标类型。gcj02/bd09ll</param>
    /// <returns></returns>
    Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, String from, String to);
    #endregion

    #region 日志
    /// <summary>日志</summary>
    ILog Log { get; set; }
    #endregion
}

/// <summary>地图提供者</summary>
public abstract class Map : DisposeBase
{
    #region 属性
    /// <summary>服务地址</summary>
    public String? Server { get; set; }

    /// <summary>应用密钥。多个key逗号分隔</summary>
    public String? AppKey { get; set; }

    /// <summary>应用密码参数名</summary>
    protected String KeyName { get; set; } = "key";

    /// <summary>最后密钥</summary>
    public String? LastKey { get; private set; }

    ///// <summary>坐标系</summary>
    //public String? CoordType { get; set; }

    /// <summary>最后网址</summary>
    public String? LastUrl { get; private set; }

    /// <summary>最后响应</summary>
    public String? LastString { get; private set; }

    /// <summary>最后结果</summary>
    public IDictionary<String, Object?>? LastResult { get; private set; }

    /// <summary>收到异常响应时是否抛出异常</summary>
    public Boolean ThrowException { get; set; }

    /// <summary>是否需要密钥。默认true；免费数据源（如Nominatim）可重写为false</summary>
    public virtual Boolean NeedKey => true;

    /// <summary>是否有效。当前是否有可用key；无需密钥的提供者始终可用</summary>
    public Boolean Available => KeyName.IsNullOrEmpty() || (_Keys == null ? !AcquireKey().IsNullOrEmpty() : _Keys.Length > 0);

    /// <summary>每秒请求上限。0 表示不限速（默认），用于避免单 Key 高频请求被厂商封禁</summary>
    public Int32 MaxQps { get; set; }

    /// <summary>请求总数</summary>
    public Int64 Total => Interlocked.Read(ref _total);

    /// <summary>成功次数</summary>
    public Int64 Success => Interlocked.Read(ref _success);

    /// <summary>失败次数</summary>
    public Int64 Fail => Interlocked.Read(ref _fail);

    /// <summary>成功率。0~1</summary>
    public Double SuccessRate => Total == 0 ? 0 : (Double)Success / Total;

    /// <summary>平均耗时。单位毫秒</summary>
    public Double AvgTime => Total == 0 ? 0 : (Double)Interlocked.Read(ref _totalTime) / Total;
    #endregion

    #region 构造
    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _Client?.TryDispose();
    }
    #endregion

    #region 方法

    private HttpClient? _Client;

    // 限速与统计
    private DateTime _lastRequest;
    private Int64 _total;
    private Int64 _success;
    private Int64 _fail;
    private Int64 _totalTime;
    private readonly ConcurrentDictionary<String, Int64> _keyStats = new();

    /// <summary>按 MaxQps 限速等待。固定间隔控制请求频率，默认不限速</summary>
    /// <returns></returns>
    protected async Task WaitTokenAsync()
    {
        if (MaxQps <= 0) return;

        var interval = 1000 / MaxQps;
        var wait = 0;

        lock (this)
        {
            var now = DateTime.Now;
            var elapsed = (Int32)(now - _lastRequest).TotalMilliseconds;
            if (elapsed >= interval)
            {
                _lastRequest = now;
                return;
            }

            wait = interval - elapsed;
        }

        if (wait > 0) await Task.Delay(wait).ConfigureAwait(false);

        lock (this)
        {
            _lastRequest = DateTime.Now;
        }
    }

    /// <summary>获取指定 Key 的请求次数</summary>
    /// <param name="key">应用密钥</param>
    /// <returns></returns>
    public Int64 GetKeyCount(String key) => _keyStats.TryGetValue(key, out var n) ? n : 0;

    /// <summary>异步获取字符串</summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public virtual async Task<String> GetStringAsync(String url)
    {
        // 限速：MaxQps 大于 0 时按固定间隔控制请求频率，避免单 Key 高频请求被厂商封禁
        await WaitTokenAsync().ConfigureAwait(false);

        var key = AcquireKey();

        // 无需密钥的免费数据源（如 Nominatim）跳过密钥校验；否则必须配置 AppKey
        if (!KeyName.IsNullOrEmpty() && key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(AppKey), "没有可用密钥");

        if (_Client == null)
        {
            var client = DefaultTracer.Instance.CreateHttpClient();
            if (!Server.IsNullOrEmpty()) client.BaseAddress = new Uri(Server);

            _Client = client;
        }

        if (url.Contains('?'))
            url += "&";
        else
            url += "?";

        if (!KeyName.IsNullOrEmpty()) url += KeyName + "=" + key;

        LastUrl = url;
        LastString = null;
        LastKey = key;

        if (!key.IsNullOrEmpty()) _keyStats.AddOrUpdate(key, 1, (_, n) => n + 1);

        var sw = Stopwatch.StartNew();
        try
        {
            var rs = await _Client.GetStringAsync(url).ConfigureAwait(false);

            Interlocked.Increment(ref _success);

            //// 删除无效密钥
            //if (IsValidKey(rs)) RemoveKey(key);

            return LastString = rs;
        }
        catch
        {
            Interlocked.Increment(ref _fail);

            throw;
        }
        finally
        {
            Interlocked.Increment(ref _total);
            Interlocked.Add(ref _totalTime, sw.ElapsedMilliseconds);
        }
    }

    /// <summary>远程调用</summary>
    /// <param name="url">目标Url</param>
    /// <param name="result">结果字段</param>
    /// <returns></returns>
    protected virtual async Task<T?> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>(String url, String? result) where T : class
    {
        LastResult = null;

        var html = await GetStringAsync(url).ConfigureAwait(false);
        if (html.IsNullOrEmpty()) return default;

        // 使用实例解析器，兼容 JSON 对象与数组根（Nominatim search 返回数组）
        var rs = html[0] == '<' && html[^1] == '>' ? XmlParser.Decode(html) : new JsonParser(html).Decode();
        if (rs == null) return default;

        LastResult = rs as IDictionary<String, Object?>;

        return JsonHelper.Convert<T>(rs);
    }
    #endregion

    #region 密钥管理
    private String[]? _Keys;
    private Int32 _KeyIndex;
    private SortedList<DateTime, List<String>> _pendingKeys = [];
    private TimerX? _timer;

    /// <summary>申请密钥</summary>
    /// <returns></returns>
    protected String AcquireKey()
    {
        if (AppKey.IsNullOrEmpty()) return String.Empty;

        var ks = _Keys;
        ks ??= _Keys = AppKey?.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (ks == null) return String.Empty;
        if (ks.Length == 0) return String.Empty;

        var idx = Interlocked.Increment(ref _KeyIndex);
        return ks[idx % ks.Length];
    }

    /// <summary>移除暂时不可用密钥</summary>
    /// <param name="key"></param>
    /// <param name="reviveTime">复苏时间。达到该时间时，重新启用该key</param>
    protected void RemoveKey(String? key, DateTime reviveTime)
    {
        if (key.IsNullOrEmpty()) return;

        lock (this)
        {
            // 使用本地变量保存数据，避免多线程冲突
            var ks = _Keys;
            if (ks == null || ks.Length == 0) return;

            var list = new List<String>(ks);
            if (list.Contains(key))
            {
                list.Remove(key);

                _timer ??= new TimerX(CheckPending, null, 5_000, 60_000)
                {
                    Async = true,
                    //CanExecute = () => _pendingKeys.Any()
                };
            }

            // 加入挂起列表
            if (!_pendingKeys.TryGetValue(reviveTime, out var keys))
                _pendingKeys.Add(reviveTime, keys = []);

            keys.Add(key);

            _Keys = list.ToArray();
        }
    }

    /// <summary>检查挂起密钥，到期后恢复可用。由 TimerX 定时调用</summary>
    /// <param name="state">定时器状态</param>
    void CheckPending(Object state)
    {
        var now = DateTime.Now;
        while (_pendingKeys.Count > 0)
        {
            var dt = _pendingKeys.Keys[0];
            if (dt > now) break;

            // 回归山林
            var ks = new List<String>(_Keys);
            var keys = _pendingKeys.Values[0];
            ks.AddRange(keys);
            _Keys = ks.Distinct().ToArray();

            _pendingKeys.RemoveAt(0);
        }

        if (_pendingKeys.Count > 0)
        {
            // 对齐时间，使其更精确
            var retain = _pendingKeys.Keys[0] - now;
            if (retain.TotalSeconds < 60) _timer?.SetNext((Int32)retain.TotalMilliseconds);
        }
    }

    /// <summary>失效关键字。命中即视为密钥失效（禁用或超限）</summary>
    private readonly String[] _KeyWords = ["INVALID", "LIMIT"];
    /// <summary>是否无效Key。可能禁用或超出限制</summary>
    /// <param name="result"></param>
    /// <returns></returns>
    protected virtual Boolean IsValidKey(String result)
    {
        if (result.IsNullOrEmpty()) return false;

        // 仅在错误消息中命中失效关键词（INVALID/LIMIT 等）时判定失效；
        // 消息回显密钥参数名（key/ak/tk）本身不改变判定，例如 "INVALID_USER_KEY" 属于失效消息，须能被识别
        return _KeyWords.Any(result.Contains);
    }
    #endregion

    #region 坐标系转换
    /// <summary>坐标系转换。公式来源于网络，未经考证</summary>
    /// <param name="points">需转换的源坐标</param>
    /// <param name="from">源坐标类型。wgs84ll/gcj02/bd09ll</param>
    /// <param name="to">目标坐标类型。gcj02/bd09ll</param>
    /// <returns></returns>
    public virtual Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, String from, String to)
    {
        if (points == null || points.Count == 0) throw new ArgumentNullException(nameof(points));
        if (from.IsNullOrEmpty()) throw new ArgumentNullException(nameof(from));
        if (to.IsNullOrEmpty()) throw new ArgumentNullException(nameof(to));

        //if (!from.EndsWithIgnoreCase("ll", "mc")) from += "ll";
        //if (!to.EndsWithIgnoreCase("ll", "mc")) to += "ll";
        from = from.ToLower();
        to = to.ToLower();

        if (from.EqualIgnoreCase(to)) return Task.FromResult(points);

        // 转换坐标
        var rs = new List<GeoPoint>();
        if (from == "wgs84" && to == "gcj02")
        {
            foreach (var pt in points)
            {
                var ps = MapHelper.WGS84ToGCJ02(pt.Latitude, pt.Longitude);
                rs.Add(new(ps[1], ps[0]));
            }
        }
        else if (from == "gcj02" && to == "wgs84")
        {
            foreach (var pt in points)
            {
                var ps = MapHelper.GCJ02ToWGS84(pt.Latitude, pt.Longitude);
                rs.Add(new(ps[1], ps[0]));
            }
        }
        else if (from == "wgs84" && to == "bd09")
        {
            foreach (var pt in points)
            {
                var ps = MapHelper.WGS84ToBD09(pt.Latitude, pt.Longitude);
                rs.Add(new(ps[1], ps[0]));
            }
        }
        else if (from == "bd09" && to == "wgs84")
        {
            foreach (var pt in points)
            {
                var ps = MapHelper.BD09ToWGS84(pt.Latitude, pt.Longitude);
                rs.Add(new(ps[1], ps[0])); 
            }
        }
        else if (from == "gcj02" && to == "bd09")
        {
            foreach (var pt in points)
            {
                var ps = MapHelper.GCJ02ToBD09(pt.Latitude, pt.Longitude);
                rs.Add(new(ps[1], ps[0]));
            }
        }
        else if (from == "bd09" && to == "gcj02")
        {
            foreach (var pt in points)
            {
                var ps = MapHelper.BD09ToGCJ02(pt.Latitude, pt.Longitude);
                rs.Add(new(ps[1], ps[0]));
            }
        }
        else
            throw new NotSupportedException($"未支持[{from}]坐标系到[{to}]坐标系的转换");

        return Task.FromResult(rs as IList<GeoPoint>);
    }
    #endregion

    #region 静态
    private static Dictionary<String, Type>? _providers;
    /// <summary>创建指定提供者的实例</summary>
    /// <param name="provider">提供者名称。支持类型名、类型全名，或省略 Map 后缀，如 Baidu、BaiduMap、NewLife.Map.BaiduMap</param>
    /// <remarks>优先匹配工厂注册表（可用 MapFactory.Register 注册自定义提供者，NativeAOT 下同样生效）；未命中时回退程序集反射扫描，该回退在部分裁剪/AOT 环境不可用</remarks>
    /// <returns></returns>
    public static IMap? Create(String provider)
    {
        if (provider.IsNullOrEmpty()) return null;

        // 优先匹配工厂已注册的提供者。NativeAOT/裁剪环境下程序集反射扫描不可用，注册表是唯一可靠路径
        foreach (var item in MapFactory.Providers)
        {
            var type = item.Value;
            if (type.Name.EqualIgnoreCase(provider) ||
                type.Name.EqualIgnoreCase(provider + "Map") ||
                type.FullName.EqualIgnoreCase(provider))
                return type.CreateInstance() as IMap;
        }

        // 反射扫描程序集，发现未注册的自定义提供者；部分裁剪/AOT 环境不支持程序集类型扫描
        try
        {
            _providers ??= AssemblyX.FindAllPlugins(typeof(IMap), true).ToDictionary(e => e.Name, e => e);

            if (_providers.TryGetValue(provider, out var type2) ||
                _providers.TryGetValue(provider + "Map", out type2))
                return type2.CreateInstance() as IMap;

            type2 = provider.GetTypeEx();
            if (type2 != null) return type2.CreateInstance() as IMap;
        }
        catch (NotSupportedException)
        {
            // NativeAOT 等环境不支持程序集类型扫描，按找不到处理
        }

        return null;
    }
    #endregion

    #region 日志
    /// <summary>日志</summary>
    public ILog Log { get; set; } = Logger.Null;

    /// <summary>写日志</summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WriteLog(String format, params Object[] args) => Log?.Info(format, args);
    #endregion
}
