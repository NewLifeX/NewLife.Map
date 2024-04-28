using System.ComponentModel;
using System.Web;
using NewLife.Data;
using NewLife.Map.Models;
using NewLife.Serialization;

namespace NewLife.Map;

/// <summary>百度地图</summary>
/// <remarks>
/// 参考手册 https://lbsyun.baidu.com/index.php?title=webapi
/// </remarks>
[DisplayName("百度地图")]
public class BaiduMap : Map, IMap
{
    #region 构造
    /// <summary>高德地图</summary>
    public BaiduMap()
    {
        Server = "https://api.map.baidu.com";
        //AppKey = "C73357a276668f8b0563d3f936475007";
        KeyName = "ak";
        //CoordType = "wgs84ll";
        //CoordType = "bd09ll";
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
        if (status != 0)
        {
            var msg = (dic["msg"] ?? dic["message"]) + "";

            // 删除无效密钥
            if (status >= 200 || IsValidKey(msg)) RemoveKey(LastKey, DateTime.Now.AddHours(1));

            return !ThrowException ? default : throw new Exception(msg);
        }

        if (result.IsNullOrEmpty()) return (T)dic;

        return (T)dic[result];
    }
    #endregion

    #region 地理编码
    /// <summary>查询地址的经纬度坐标</summary>
    /// <remarks>
    /// 参考手册 https://lbsyun.baidu.com/index.php?title=webapi/guide/webservice-geocoding
    /// </remarks>
    /// <param name="address"></param>
    /// <param name="city"></param>
    /// <param name="coordtype"></param>
    /// <returns></returns>
    protected async Task<IDictionary<String, Object>> GetGeocoderAsync(String address, String? city = null, String? coordtype = null)
    {
        if (address.IsNullOrEmpty()) throw new ArgumentNullException(nameof(address));

        // 编码
        address = HttpUtility.UrlEncode(address);
        city = HttpUtility.UrlEncode(city);

        var url = $"/geocoding/v3/?address={address}&city={city}&ret_coordtype={coordtype}&extension_analys_level=1&output=json";

        return await InvokeAsync<IDictionary<String, Object>>(url, "result");
    }

    /// <summary>查询地址获取坐标</summary>
    /// <param name="address">地址</param>
    /// <param name="city">城市</param>
    /// <param name="coordtype"></param>
    /// <param name="formatAddress">是否格式化地址</param>
    /// <returns></returns>
    public async Task<GeoAddress?> GetGeoAsync(String address, String? city = null, String? coordtype = null, Boolean formatAddress = false)
    {
        var rs = await GetGeocoderAsync(address, city, coordtype);
        if (rs == null || rs.Count == 0) return null;

        if (rs["location"] is not IDictionary<String, Object> ds || ds.Count < 2) return null;

        var geo = new GeoAddress
        {
            Location = new(ds["lng"], ds["lat"]),
        };

        if (formatAddress)
        {
            var geo2 = await GetReverseGeoAsync(geo.Location, coordtype);
            if (geo2 != null) geo = geo2;
        }

        geo.Precise = rs["precise"].ToBoolean();
        geo.Confidence = rs["confidence"].ToInt();
        geo.Comprehension = rs["comprehension"].ToInt();
        geo.Level = rs["level"] + "";

        return geo;
    }
    #endregion

    #region 逆地理编码
    /// <summary>根据坐标获取地址</summary>
    /// <remarks>
    /// 参考手册 https://lbsyun.baidu.com/index.php?title=webapi/guide/webservice-geocoding-abroad
    /// </remarks>
    /// <param name="point"></param>
    /// <param name="coordtype">坐标系</param>
    /// <returns></returns>
    protected async Task<IDictionary<String, Object>> GetReverseGeocoderAsync(GeoPoint point, String? coordtype)
    {
        if (point == null || point.Longitude == 0 || point.Latitude == 0) throw new ArgumentNullException(nameof(point));

        var url = $"/reverse_geocoding/v3/?location={point.Latitude},{point.Longitude}&extensions_poi=1&extensions_town=true&coordtype={coordtype}&output=json";

        return await InvokeAsync<IDictionary<String, Object>>(url, "result");
    }

    /// <summary>根据坐标获取地址</summary>
    /// <param name="point"></param>
    /// <param name="coordtype"></param>
    /// <returns></returns>
    public async Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, String? coordtype)
    {
        var rs = await GetReverseGeocoderAsync(point, coordtype);
        if (rs == null || rs.Count == 0) return null;

        var addr = new GeoAddress
        {
            Address = rs["formatted_address"] + "",
            Confidence = rs["confidence"].ToInt(),
        };
        if (rs["location"] is IDictionary<String, Object> ds && ds.Count >= 2)
        {
            addr.Location = new(ds["lng"], ds["lat"]);
        }

        if (rs["addressComponent"] is IDictionary<String, Object> component)
        {
            var reader = new JsonReader();
            reader.ToObject(component, null, addr);

            addr.Code = component["adcode"].ToInt();
            addr.Township = component["town"] + "";
            addr.Towncode = component["town_code"].ToInt();
            addr.StreetNumber = component["street_number"] + "";
        }

        // 叠加POI语义描述，让结果地址看起来更精确
        if (rs.TryGetValue("sematic_description", out var sd) && sd is String value && !value.IsNullOrEmpty())
            addr.Title = value;

        return addr;
    }
    #endregion

    #region 路径规划
    /// <summary>计算距离和驾车时间</summary>
    /// <remarks>
    /// https://lbsyun.baidu.com/index.php?title=webapi/route-matrix-api-v2
    /// </remarks>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="coordtype"></param>
    /// <param name="type">路径计算的方式和方法</param>
    /// <returns></returns>
    public async Task<Driving?> GetDistanceAsync(GeoPoint origin, GeoPoint destination, String? coordtype, Int32 type = 13)
    {
        if (origin == null || origin.Longitude < 1 && origin.Latitude < 1) throw new ArgumentNullException(nameof(origin));
        if (destination == null || destination.Longitude < 1 && destination.Latitude < 1) throw new ArgumentNullException(nameof(destination));

        if (type <= 0) type = 13;
        var coord = coordtype;
        if (!coord.IsNullOrEmpty() && coord.Length > 6) coord = coord.TrimEnd("ll");
        var url = $"/routematrix/v2/driving?origins={origin.Latitude},{origin.Longitude}&destinations={destination.Latitude},{destination.Longitude}&tactics={type}&coord_type={coord}&output=json";

        var list = await InvokeAsync<IList<Object>>(url, "result");
        if (list == null || list.Count == 0) return null;

        if (list.FirstOrDefault() is not IDictionary<String, Object> geo) return null;

        var d1 = geo["distance"] as IDictionary<String, Object>;
        var d2 = geo["duration"] as IDictionary<String, Object>;

        var rs = new Driving
        {
            Distance = d1?["value"].ToInt() ?? 0,
            Duration = d2?["value"].ToInt() ?? 0
        };

        return rs;
    }
    #endregion

    #region 地址检索
    /// <summary>行政区划区域检索</summary>
    /// <remarks>
    /// https://lbsyun.baidu.com/index.php?title=webapi/guide/webservice-placeapi
    /// </remarks>
    /// <param name="query"></param>
    /// <param name="tag"></param>
    /// <param name="region"></param>
    /// <param name="coordtype"></param>
    /// <param name="formatAddress"></param>
    /// <returns></returns>
    public async Task<GeoAddress?> PlaceSearchAsync(String query, String tag, String region, String? coordtype = null, Boolean formatAddress = true)
    {
        // 编码
        query = HttpUtility.UrlEncode(query);
        tag = HttpUtility.UrlEncode(tag);
        region = HttpUtility.UrlEncode(region);

        var url = $"/place/v2/search?output=json&query={query}&tag={tag}&region={region}&city_limit=true&ret_coordtype={coordtype}";

        var list = await InvokeAsync<IList<Object>>(url, "results");
        if (list == null || list.Count == 0) return null;

        if (list.FirstOrDefault() is not IDictionary<String, Object> rs) return null;

        var geo = new GeoAddress();

        if (rs["location"] is IDictionary<String, Object> ds && ds.Count >= 2)
        {
            geo.Location = new(ds["lng"], ds["lat"]);
        }
        //else if (rs["num"] is Int32 num && num > 0 && rs["name"] != null)
        //{
        //    // 多个目标城市匹配，重新搜索
        //    return await PlaceSearchAsync(query, tag, rs["name"] + "", formatAddress);
        //}
        else
            return null;

        if (formatAddress && geo.Location != null)
        {
            var geo2 = await GetReverseGeoAsync(geo.Location, coordtype);
            if (geo2 != null) geo = geo2;
        }

        geo.Name = rs["name"] + "";
        var addr = rs["address"] + "";
        if (!addr.IsNullOrEmpty()) geo.Address = addr;

        return geo;
    }
    #endregion

    #region IP定位
    /// <summary>IP定位</summary>
    /// <remarks>
    /// https://lbsyun.baidu.com/index.php?title=webapi/ip-api
    /// </remarks>
    /// <param name="ip"></param>
    /// <param name="coordtype"></param>
    /// <returns></returns>
    public async Task<IDictionary<String, Object?>?> IpLocationAsync(String ip, String coordtype)
    {
        var url = $"/location/ip?ip={ip}&coor={coordtype}";

        var dic = await InvokeAsync<IDictionary<String, Object>>(url, null);
        if (dic == null || dic.Count == 0) return null;

        if (dic["content"] is not IDictionary<String, Object?> rs) return null;

        if (dic.TryGetValue("address", out var fulladdress)) rs["full_address"] = fulladdress;
        if (rs.TryGetValue("address_detail", out var v1))
        {
            if (v1 != null) rs.Merge(v1);
            rs.Remove("address_detail");
        }
        if (rs.TryGetValue("point", out var v2))
        {
            if (v2 != null) rs.Merge(v2);
            rs.Remove("point");
        }

        return rs;
    }
    #endregion

    #region 坐标转换
    private static readonly String[] _coordTypes = ["", "wgs84ll", "sougou", "gcj02ll", "gcj02mc", "bd09ll", "bd09mc"];
    /// <summary>坐标转换</summary>
    /// <remarks>
    /// https://lbsyun.baidu.com/index.php?title=webapi/guide/changeposition
    /// </remarks>
    /// <param name="points">需转换的源坐标</param>
    /// <param name="from">源坐标类型。wgs84ll/gcj02/bd09ll</param>
    /// <param name="to">目标坐标类型。gcj02/bd09ll</param>
    /// <returns></returns>
    public override async Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, String from, String to)
    {
        if (points == null || points.Count == 0) throw new ArgumentNullException(nameof(points));
        //if (from.IsNullOrEmpty()) from = coordtype;
        if (from.IsNullOrEmpty()) throw new ArgumentNullException(nameof(from));
        if (to.IsNullOrEmpty()) throw new ArgumentNullException(nameof(to));

        if (!from.EndsWithIgnoreCase("ll", "mc")) from += "ll";
        if (!to.EndsWithIgnoreCase("ll", "mc")) to += "ll";

        if (from.EqualIgnoreCase(to)) return points;

        var idxFrom = 0;
        var idxTo = 0;
        for (var i = 0; i < _coordTypes.Length; i++)
        {
            if (_coordTypes[i].EqualIgnoreCase(from)) idxFrom = i;
            if (_coordTypes[i].EqualIgnoreCase(to)) idxTo = i;
        }
        if (idxFrom == 0) throw new ArgumentOutOfRangeException(nameof(from));
        if (idxTo == 0) throw new ArgumentOutOfRangeException(nameof(to));

        var url = $"/geoconv/v1/?coords={points.Join(";", e => $"{e.Longitude},{e.Latitude}")}&from={idxFrom}&to={idxTo}&output=json";

        var list = new List<GeoPoint>();
        var rs = await InvokeAsync<IList<Object>>(url, "result");
        if (rs == null || rs.Count == 0) return list;

        foreach (var item in rs.Cast<IDictionary<String, Object>>())
        {
            list.Add(new(item["x"], item["y"]));
        }

        return list;
    }
    #endregion
}