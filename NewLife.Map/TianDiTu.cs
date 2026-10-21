using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using NewLife.Collections;
using NewLife.Data;
using NewLife.Map.Models;
using NewLife.Serialization;

namespace NewLife.Map;

/// <summary>天地图</summary>
/// <remarks>
/// 参考手册 http://lbs.tianditu.gov.cn/server/guide.html
/// </remarks>
[DisplayName("Tianditu")]
public class TianDiTu : Map, IMap
{
    #region 构造
    /// <summary>高德地图</summary>
    public TianDiTu()
    {
        Server = "http://api.tianditu.gov.cn";
        //AppKey = "3334f7776916effb40f2a11dbae57781";
        KeyName = "tk";
    }
    #endregion

    #region 方法
    /// <summary>远程调用</summary>
    /// <param name="url">目标Url</param>
    /// <param name="result">结果字段</param>
    /// <returns></returns>
    protected override async Task<T?> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] T>(String url, String? result) where T : class
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
    protected async Task<IDictionary<String, Object>?> GetGeocoderAsync(String address, String? city = null, String? coordtype = null)
    {
        if (address.IsNullOrEmpty()) throw new ArgumentNullException(nameof(address));

        // 编码
        address = HttpUtility.UrlEncode(address);
        //city = HttpUtility.UrlEncode(city);

        var url = $"/geocoder?ds={{\"keyWord\":\"{address}\"}}";

        return await InvokeAsync<IDictionary<String, Object>>(url, "location");
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

        var geo = new GeoAddress
        {
            Location = MapHelper.Create(rs["lon"], rs["lat"]),
        };

        if (formatAddress)
        {
            var geo2 = await GetReverseGeoAsync(geo.Location ?? default, coordtype);
            if (geo2 != null) geo = geo2;
        }

        geo.Level = rs["level"] + "";
        geo.Confidence = rs["score"].ToInt();

        return geo;
    }
    #endregion

    #region 逆地理编码
    /// <summary>根据坐标获取地址</summary>
    /// <remarks>
    /// 参考手册 http://lbs.tianditu.gov.cn/server/geocoding.html
    /// </remarks>
    /// <param name="point"></param>
    /// <param name="coordtype">坐标系</param>
    /// <returns></returns>
    protected async Task<IDictionary<String, Object>?> GetReverseGeocoderAsync(GeoPoint point, String? coordtype)
    {
        if (point == null || point.Longitude == 0 || point.Latitude == 0) throw new ArgumentNullException(nameof(point));

        var url = $"/geocoder?postStr={{'lon':{point.Longitude},'lat':{point.Latitude},'ver':1}}&type=geocode";

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
            addr.Location = MapHelper.Create(ds["lon"], ds["lat"]);
        }

        if (rs["addressComponent"] is IDictionary<String, Object> component)
        {
            var reader = new JsonReader();
            reader.ToObject(component, null, addr);

            addr.Country = component["nation"] + "";
            addr.Province = component["province"] + "";
            addr.City = component["city"] + "";
            addr.District = component["county"] + "";

            addr.Code = (component["county_code"] + "").TrimPrefix("156").ToInt();

            addr.Name = component["poi"] + "";
            addr.Title = component["address"] + "";

            addr.Street = component["road"] + "";
            addr.StreetNumber = component["street_number"] + "";
        }

        return addr;
    }
    #endregion

    #region 路径规划
    /// <summary>计算距离和驾车时间</summary>
    /// <remarks>
    /// http://lbs.tianditu.gov.cn/server/drive.html
    /// </remarks>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="coordtype"></param>
    /// <param name="type">导航路线类型。0：最快路线，1：最短路线，2：避开高速，3：步行</param>
    /// <returns></returns>
    public async Task<Driving?> GetDistanceAsync(GeoPoint origin, GeoPoint destination, String? coordtype = null, Int32 type = 0)
    {
        return await GetDistanceAsync(origin, destination, [], coordtype, type);
    }

    /// <summary>计算距离和驾车时间</summary>
    /// <remarks>
    /// http://lbs.tianditu.gov.cn/server/drive.html
    /// </remarks>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="mids">途经点</param>
    /// <param name="coordtype"></param>
    /// <param name="type">导航路线类型。0：最快路线，1：最短路线，2：避开高速，3：步行</param>
    /// <returns></returns>
    public async Task<Driving?> GetDistanceAsync(GeoPoint origin, GeoPoint destination, IList<GeoPoint> mids, String? coordtype = null, Int32 type = 0)
    {
        if (origin == null || origin.Longitude < 1 && origin.Latitude < 1) throw new ArgumentNullException(nameof(origin));
        if (destination == null || destination.Longitude < 1 && destination.Latitude < 1) throw new ArgumentNullException(nameof(destination));

        var sb = Pool.StringBuilder.Get();
        sb.Append($"/drive?postStr={{\"orig\":\"{origin.Longitude},{origin.Latitude}\",\"dest\":\"{destination.Longitude},{destination.Latitude}\"");
        if (mids != null && mids.Count > 0)
            sb.Append($",\"mid\":\"{mids.Join(";", e => $"{e.Longitude},{e.Latitude}")}\"");
        sb.Append($",\"style\":\"{type}\"}}&type=search");

        var url = sb.Return(true);

        var dic = await InvokeAsync<IDictionary<String, Object>>(url, null);
        if (dic == null || dic.Count == 0) return null;

        var html = LastString;
        if (html.IsNullOrEmpty()) return null;

        var rs = new Driving
        {
            Distance = (Int32)Math.Round(html.Substring("<distance>", "</distance>").ToDouble() * 1000, 0),
            Duration = (Int32)Math.Round(html.Substring("<duration>", "</duration>").ToDouble(), 0),
        };

        return rs;
    }
    #endregion

    #region 地址检索
    /// <summary>行政区划区域检索。检索行政区划范围内的 POI</summary>
    /// <remarks>
    /// 参考手册 http://lbs.tianditu.gov.cn/server/search2.html
    /// 指定 region 时使用行政区划区域搜索（queryType=12，specify 传行政区名称或 9 位国标码）；
    /// 未指定 region 时使用普通搜索（queryType=1），以全球范围与 12 级视野检索。tag 映射到数据分类 dataTypes
    /// </remarks>
    /// <param name="query">检索关键字</param>
    /// <param name="tag">数据分类。可为分类名称或分类编码，多个分类用英文逗号隔开</param>
    /// <param name="region">指定行政区。9 位国标码或行政区名称，如 156110000 或 北京</param>
    /// <param name="coordtype">坐标系。天地图检索返回 WGS84 经纬度，该参数不适用</param>
    /// <param name="formatAddress">是否格式化地址。为真时补充调用逆地理编码</param>
    /// <returns></returns>
    public async Task<GeoAddress?> PlaceSearchAsync(String query, String tag, String region, String? coordtype = null, Boolean formatAddress = true)
    {
        if (query.IsNullOrEmpty()) throw new ArgumentNullException(nameof(query));

        var post = new Dictionary<String, Object>
        {
            ["keyWord"] = query,
            ["queryType"] = region.IsNullOrEmpty() ? 1 : 12,
            ["start"] = 0,
            ["count"] = 10,
        };
        if (!tag.IsNullOrEmpty()) post["dataTypes"] = tag;
        if (!region.IsNullOrEmpty())
            post["specify"] = region;
        else
        {
            post["mapBound"] = "-180,-90,180,90";
            post["level"] = 12;
        }

        var url = $"/v2/search?postStr={HttpUtility.UrlEncode(post.ToJson())}&type=query";

        var rs = await InvokeAsync<IDictionary<String, Object>>(url, null);
        if (rs == null || rs.Count == 0) return null;

        if (rs["pois"] is not IList<Object> pois || pois.Count == 0) return null;
        if (pois[0] is not IDictionary<String, Object> poi) return null;

        var geo = new GeoAddress
        {
            Location = MapHelper.Parse(poi["lonlat"] as String),
        };
        if (poi["province"] is String province) geo.Province = province;
        if (poi["city"] is String city) geo.City = city;
        if (poi["county"] is String county) geo.District = county;
        if (poi["countyCode"] is String code) geo.Code = code.TrimPrefix("156").ToInt();

        if (formatAddress && geo.Location != null)
        {
            var geo2 = await GetReverseGeoAsync(geo.Location.Value, coordtype);
            if (geo2 != null) geo = geo2;
        }

        geo.Name = poi["name"] + "";
        var addr = poi["address"] + "";
        if (!addr.IsNullOrEmpty()) geo.Address = addr;

        return geo;
    }
    #endregion
}