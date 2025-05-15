using System.ComponentModel;
using System.Web;
using NewLife.Data;
using NewLife.Map.Models;
using NewLife.Serialization;

namespace NewLife.Map;

/// <summary>高德地图</summary>
/// <remarks>
/// 参考地址 http://lbs.amap.com/api/webservice/guide/api/georegeo/#geo
/// </remarks>
[DisplayName("高德地图")]
public class AMap : Map, IMap
{
    #region 构造
    /// <summary>高德地图</summary>
    public AMap()
    {
        Server = "http://restapi.amap.com";
        //AppKey = "" +
        //    // 六条
        //    "2aada76e462af71e1b67ba1df22d0fa4," +
        //    "038a84bf20e8306fdd2203110739110c," +
        //    "29360e6eeb7b921d644cde3068ddf24f," +
        //    "a8e5e3e7b4068be9c525bd2b7854eb20," +
        //    "9935cf01abd570532ab7a19f83f905d3," +
        //    "ecacc934a6529b39513ea2bfa8a03def," +
        //    "08c70a500587c1006e10e4a096cb6b58," +
        //    "3508dadf3777531cef63bdc061ac020f," +
        //    "331566353c89521faffd84af22cd4f5f," +
        //    "6f19a71c6fd71baf54680eb63c4d5fce," +
        //    "" +
        //    "";
        KeyName = "key";

        //CoordType = "wgs84ll";
    }
    #endregion

    #region 方法
    /// <summary>远程调用</summary>
    /// <param name="url">目标Url</param>
    /// <param name="result">结果字段</param>
    /// <returns></returns>
    protected override async Task<T> InvokeAsync<T>(String url, String? result)
    {
        var dic = await base.InvokeAsync<IDictionary<String, Object>>(url, result);
        if (dic == null || dic.Count == 0) return default;

        var status = dic["status"].ToInt();
        if (status != 1)
        {
            var msg = dic["info"] + "";

            // 删除无效密钥
            if (!LastKey.IsNullOrEmpty() && IsValidKey(msg)) RemoveKey(LastKey, DateTime.Now.AddHours(1));

            return !ThrowException ? default : throw new Exception(msg);
        }

        if (result.IsNullOrEmpty()) return (T)dic;

        return (T)dic[result];
    }
    #endregion

    #region 地理编码
    /// <summary>查询地址的经纬度坐标</summary>
    /// <param name="address"></param>
    /// <param name="city"></param>
    /// <param name="coordtype"></param>
    /// <returns></returns>
    protected async Task<IDictionary<String, Object>?> GetGeocoderAsync(String address, String? city = null, String? coordtype = null)
    {
        if (address.IsNullOrEmpty()) throw new ArgumentNullException(nameof(address));

        // 编码
        address = HttpUtility.UrlEncode(address);
        city = HttpUtility.UrlEncode(city);

        var url = $"http://restapi.amap.com/v3/geocode/geo?address={address}&city={city}&output=json";

        var list = await InvokeAsync<IList<Object>>(url, "geocodes");
        return list?.FirstOrDefault() as IDictionary<String, Object>;
    }

    /// <summary>查询地址获取坐标</summary>
    /// <param name="address">地址</param>
    /// <param name="city">城市</param>
    /// <param name="coordtype"></param>
    /// <param name="formatAddress">是否格式化地址。高德地图默认已经格式化地址</param>
    /// <returns></returns>
    public async Task<GeoAddress?> GetGeoAsync(String address, String? city = null, String? coordtype = null, Boolean formatAddress = false)
    {
        var rs = await GetGeocoderAsync(address, city);
        if (rs == null || rs.Count == 0) return null;

        var geo = new GeoAddress
        {
            Location = new(rs["location"] as String)
        };
        rs.Remove("location");

        var reader = new JsonReader();
        reader.ToObject(rs, null, geo);

        geo.Code = rs["adcode"].ToInt();

        if (rs["township"] is IList<Object> ts && ts.Count > 0) geo.Township = ts[0] + "";
        if (rs["number"] is IList<Object> ns && ns.Count > 0) geo.StreetNumber = ns[0] + "";

        if (formatAddress)
        {
            var geo2 = await GetReverseGeoAsync(geo.Location, "gcj02");
            if (geo2 != null)
            {
                geo = geo2;
                if (geo.Level.IsNullOrEmpty()) geo.Level = rs["level"] + "";
            }
        }

        {
            var addr = rs["formatted_address"] + "";
            if (!addr.IsNullOrEmpty() && (geo.Address.IsNullOrEmpty() || geo.Address.Length < addr.Length))
                geo.Address = addr;
        }

        // 替换竖线
        TrimAddress(geo);

        return geo;
    }

    static void TrimAddress(GeoAddress geo)
    {
        // 替换竖线
        if (!geo.Address.IsNullOrEmpty()) geo.Address = geo.Address.Replace("|", null);
    }
    #endregion

    #region 逆地理编码
    /// <summary>根据坐标获取地址</summary>
    /// <remarks>
    /// http://lbs.amap.com/api/webservice/guide/api/georegeo/#regeo
    /// </remarks>
    /// <param name="point"></param>
    /// <param name="coordtype"></param>
    /// <returns></returns>
    protected async Task<IDictionary<String, Object>> GetReverseGeocoderAsync(GeoPoint point, String? coordtype)
    {
        if (point.Longitude < 0.1 || point.Latitude < 0.1) throw new ArgumentNullException(nameof(point));

        var url = $"http://restapi.amap.com/v3/geocode/regeo?location={point.Longitude},{point.Latitude}&extensions=base&output=json";

        return await InvokeAsync<IDictionary<String, Object>>(url, "regeocode");
    }

    /// <summary>根据坐标获取地址</summary>
    /// <param name="point"></param>
    /// <param name="coordtype">坐标系</param>
    /// <returns></returns>
    public async Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, String? coordtype)
    {
        var rs = await GetReverseGeocoderAsync(point, coordtype);
        if (rs == null || rs.Count == 0) return null;

        var geo = new GeoAddress
        {
            Address = rs["formatted_address"] + "",
            Location = point
        };
        if (rs["addressComponent"] is IDictionary<String, Object> component)
        {
            var reader = new JsonReader();
            reader.ToObject(component, null, geo);

            if (component.TryGetValue("city", out var obj) && obj is not String)
                geo.City = null;
            if (component.TryGetValue("streetNumber", out obj) && obj is not String)
                geo.StreetNumber = null;

            geo.Code = component["adcode"].ToInt();

            geo.Township = null;
            geo.Towncode = 0;

            var towncode = "";
            if (component["township"] is String ts) geo.Township = ts;
            if (component["towncode"] is String tc) towncode = tc;

            // 去掉乡镇代码后面多余的0
            if (!towncode.IsNullOrEmpty() && towncode.Length > 6 + 3) towncode = towncode.TrimEnd("000");
            geo.Towncode = towncode.ToInt();

            if (component["streetNumber"] is IDictionary<String, Object> sn && sn.Count > 0)
            {
                // 街道地址可能有多个候选项
                if (sn["street"] is IList<Object> ss && ss.Count > 0)
                    geo.Street = ss[0] + "";
                else
                    geo.Street = sn["street"] + "";

                if (sn["number"] is IList<Object> ns && ns.Count > 0)
                    geo.StreetNumber = ns[0] + "";
                else
                    geo.StreetNumber = sn["number"] + "";

                if (geo.Title.IsNullOrEmpty())
                {
                    if (!sn.TryGetValue("direction", out var direction)) direction = "";
                    if (sn.TryGetValue("distance", out var distance)) distance = Math.Round(distance.ToDouble(), 0) + "米";

                    geo.Title = $"{geo.Province}{geo.City}{geo.District}{geo.Township}{geo.Street}{geo.StreetNumber}{direction}{distance}";
                }
            }
        }

        geo.Location = point;
        // 替换竖线
        TrimAddress(geo);

        return geo;
    }
    #endregion

    #region 路径规划
    /// <summary>计算距离和驾车时间</summary>
    /// <remarks>
    /// http://lbs.amap.com/api/webservice/guide/api/direction
    /// 
    /// type:
    /// 0：直线距离
    /// 1：驾车导航距离（仅支持国内坐标）。
    /// 必须指出，当为1时会考虑路况，故在不同时间请求返回结果可能不同。
    /// 此策略和driving接口的 strategy = 4策略一致
    /// 2：公交规划距离（仅支持同城坐标）
    /// 3：步行规划距离（仅支持5km之间的距离）
    /// 
    /// distance    路径距离，单位：米
    /// duration    预计行驶时间，单位：秒
    /// </remarks>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="coordtype"></param>
    /// <param name="type">路径计算的方式和方法</param>
    /// <returns></returns>
    public async Task<Driving?> GetDistanceAsync(GeoPoint origin, GeoPoint destination, String? coordtype, Int32 type = 1)
    {
        if (origin == null || origin.Longitude < 1 && origin.Latitude < 1) throw new ArgumentNullException(nameof(origin));
        if (destination == null || destination.Longitude < 1 && destination.Latitude < 1) throw new ArgumentNullException(nameof(destination));

        if (type <= 0) type = 1;
        var url = $"http://restapi.amap.com/v3/distance?origins={origin.Longitude},{origin.Latitude}&destination={destination.Longitude},{destination.Latitude}&type={type}&output=json";

        var list = await InvokeAsync<IList<Object>>(url, "results");
        if (list == null || list.Count == 0) return null;

        if (list.FirstOrDefault() is not IDictionary<String, Object> geo) return null;

        var rs = new Driving
        {
            Distance = geo["distance"].ToInt(),
            Duration = geo["duration"].ToInt()
        };

        return rs;
    }
    #endregion

    #region 行政区划
    /// <summary>行政区划</summary>
    /// <remarks>
    /// http://lbs.amap.com/api/webservice/guide/api/district
    /// </remarks>
    /// <param name="keywords">查询关键字</param>
    /// <param name="subdistrict">设置显示下级行政区级数</param>
    /// <param name="code">按照指定行政区划进行过滤，填入后则只返回该省/直辖市信息</param>
    /// <returns></returns>
    public async Task<IList<GeoArea>> GetAreaAsync(String keywords, Int32 subdistrict = 1, Int32 code = 0)
    {
        if (keywords.IsNullOrEmpty()) throw new ArgumentNullException(nameof(keywords));

        // 编码
        keywords = HttpUtility.UrlEncode(keywords);

        var url = $"http://restapi.amap.com/v3/config/district?keywords={keywords}&subdistrict={subdistrict}&filter={code}&extensions=base&output=json";

        var list = await InvokeAsync<IList<Object>>(url, "districts");
        if (list == null || list.Count == 0) return [];

        if (list.FirstOrDefault() is not IDictionary<String, Object> geo) return [];

        var addrs = GetArea(geo, 0);

        return addrs;
    }

    private IList<GeoArea> GetArea(IDictionary<String, Object> geo, Int32 parentCode)
    {
        if (geo == null || geo.Count == 0) return [];

        var addrs = new List<GeoArea>();

        var root = new GeoArea();
        new JsonReader().ToObject(geo, null, root);
        root.Code = geo["adcode"].ToInt();
        if (parentCode > 0) root.ParentCode = parentCode;

        addrs.Add(root);

        if (geo["districts"] is IList<Object> childs && childs.Count > 0)
        {
            foreach (var item in childs)
            {
                if (item is IDictionary<String, Object> geo2)
                {
                    var rs = GetArea(geo2, root.Code);
                    if (rs != null && rs.Count > 0) addrs.AddRange(rs);
                }
            }
        }

        return addrs;
    }
    #endregion

    #region 密钥管理
    private readonly String[] _KeyWords = ["TOO_FREQUENT", "LIMIT", "NOMATCH", "RECYCLED"];
    /// <summary>是否无效Key。可能禁用或超出限制</summary>
    /// <param name="result"></param>
    /// <returns></returns>
    protected override Boolean IsValidKey(String result)
    {
        if (result.IsNullOrEmpty()) return false;

        if (_KeyWords.Any(result.Contains)) return true;

        return base.IsValidKey(result);
    }
    #endregion
}