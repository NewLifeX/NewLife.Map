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

    /// <summary>是否有效。当前是否有可用key</summary>
    public Boolean Available => _Keys == null ? !AcquireKey().IsNullOrEmpty() : (_Keys.Length > 0);
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

    /// <summary>异步获取字符串</summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public virtual async Task<String> GetStringAsync(String url)
    {
        var key = AcquireKey();
        if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(AppKey), "没有可用密钥");

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

        url += KeyName + "=" + key;

        LastUrl = url;
        LastString = null;
        LastKey = key;

        var rs = await _Client.GetStringAsync(url).ConfigureAwait(false);

        //// 删除无效密钥
        //if (IsValidKey(rs)) RemoveKey(key);

        return LastString = rs;
    }

    /// <summary>远程调用</summary>
    /// <param name="url">目标Url</param>
    /// <param name="result">结果字段</param>
    /// <returns></returns>
    protected virtual async Task<T?> InvokeAsync<T>(String url, String? result) where T : class
    {
        LastResult = null;

        var html = await GetStringAsync(url).ConfigureAwait(false);
        if (html.IsNullOrEmpty()) return default;

        var rs = html[0] == '<' && html[^1] == '>' ? XmlParser.Decode(html) : JsonParser.Decode(html);
        if (rs == null) return default;

        LastResult = rs;

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

    private readonly String[] _KeyWords = ["INVALID", "LIMIT"];
    /// <summary>是否无效Key。可能禁用或超出限制</summary>
    /// <param name="result"></param>
    /// <returns></returns>
    protected virtual Boolean IsValidKey(String result)
    {
        if (result.IsNullOrEmpty()) return false;
        if (!KeyName.IsNullOrEmpty() && result.Contains(KeyName.ToUpper())) return false;

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
        if (from == "wgs84" && to == "bd09")
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
        if (from == "gcj02" && to == "bd09")
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
    /// <param name="provider"></param>
    /// <returns></returns>
    public static IMap? Create(String provider)
    {
        _providers ??= AssemblyX.FindAllPlugins(typeof(IMap), true).ToDictionary(e => e.Name, e => e);

        if (_providers.TryGetValue(provider, out var type) ||
            _providers.TryGetValue(provider + "Map", out type))
            return type.CreateInstance() as IMap;

        type = provider.GetTypeEx();
        if (type != null) return type.CreateInstance() as IMap;

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
