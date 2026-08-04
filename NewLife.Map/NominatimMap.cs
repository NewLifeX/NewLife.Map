using System.ComponentModel;
using System.Web;
using NewLife.Data;
using NewLife.Map.Models;

namespace NewLife.Map;

/// <summary>OSM/Nominatim 开源地图。免费无Key，覆盖海外与开源数据场景</summary>
/// <remarks>
/// 参考文档 https://nominatim.org/release-docs/latest/api/Search/
/// 使用政策：Nominatim 免费服务要求 ≤1 次/秒，且必须携带有效 UserAgent，请遵守
/// https://operations.osmfoundation.org/policies/nominatim/
/// 数据源坐标为 WGS84，coordtype 参数不适用。
/// </remarks>
[DisplayName("OSM/Nominatim")]
public class NominatimMap : Map, IMap
{
    #region 构造
    /// <summary>OSM/Nominatim 开源地图</summary>
    public NominatimMap()
    {
        Server = "https://nominatim.openstreetmap.org";
        KeyName = "";
    }
    #endregion

    #region 属性
    /// <summary>无需密钥。免费开源数据源</summary>
    public override Boolean NeedKey => false;
    #endregion

    #region 地理编码
    /// <summary>查询地址获取坐标</summary>
    /// <param name="address">地址</param>
    /// <param name="city">城市</param>
    /// <param name="coordtype">坐标系。Nominatim 数据源为 WGS84，该参数不适用</param>
    /// <param name="formatAddress">是否格式化地址</param>
    /// <returns></returns>
    public async Task<GeoAddress?> GetGeoAsync(String address, String? city = null, String? coordtype = null, Boolean formatAddress = false)
    {
        if (address.IsNullOrEmpty()) throw new ArgumentNullException(nameof(address));

        address = HttpUtility.UrlEncode(address);
        city = HttpUtility.UrlEncode(city);

        var url = $"/search?q={address}&limit=1&addressdetails=1";
        if (!city.IsNullOrEmpty()) url += $"&city={city}";

        var list = await InvokeAsync<IList<Object>>(url, null);
        if (list == null || list.Count == 0) return null;

        if (list[0] is not IDictionary<String, Object> rs) return null;

        var geo = new GeoAddress
        {
            Location = MapHelper.Create(rs["lon"], rs["lat"]),
            Address = rs["display_name"] + "",
            Name = rs["name"] + "",
        };

        if (rs["address"] is IDictionary<String, Object> ad) FillAddress(geo, ad);

        if (formatAddress)
        {
            var geo2 = await GetReverseGeoAsync(geo.Location ?? default, coordtype);
            if (geo2 != null) geo = geo2;
        }

        return geo;
    }
    #endregion

    #region 逆地理编码
    /// <summary>根据坐标获取地址</summary>
    /// <param name="point">坐标</param>
    /// <param name="coordtype">坐标系。Nominatim 数据源为 WGS84，该参数不适用</param>
    /// <returns></returns>
    public async Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, String? coordtype)
    {
        if (point.Longitude < 0.1 || point.Latitude < 0.1) throw new ArgumentNullException(nameof(point));

        var url = $"/reverse?lat={point.Latitude}&lon={point.Longitude}&addressdetails=1";

        var rs = await InvokeAsync<IDictionary<String, Object>>(url, null);
        if (rs == null || rs.Count == 0) return null;

        var geo = new GeoAddress
        {
            Location = MapHelper.Create(rs["lon"], rs["lat"]),
            Address = rs["display_name"] + "",
            Name = rs["name"] + "",
        };

        if (rs["address"] is IDictionary<String, Object> ad) FillAddress(geo, ad);

        return geo;
    }
    #endregion

    #region 路径规划
    /// <summary>计算距离和驾车时间。Nominatim 不提供该能力，返回null</summary>
    /// <param name="origin">起始坐标</param>
    /// <param name="destination">目的坐标</param>
    /// <param name="coordtype">坐标系</param>
    /// <param name="type">路径计算方式</param>
    /// <returns></returns>
    public Task<Driving?> GetDistanceAsync(GeoPoint origin, GeoPoint destination, String? coordtype, Int32 type = 0) => Task.FromResult<Driving?>(null);
    #endregion

    #region 辅助
    /// <summary>从 Nominatim address 组件填充地址字段。字段可能缺失，使用 TryGetValue 防异常</summary>
    /// <param name="geo">地理地址</param>
    /// <param name="ad">Nominatim address 字典</param>
    private static void FillAddress(GeoAddress geo, IDictionary<String, Object> ad)
    {
        if (ad.TryGetValue("country", out var v)) geo.Country = v + "";
        if (ad.TryGetValue("state", out v)) geo.Province = v + "";
        if (ad.TryGetValue("city", out v)) geo.City = v + "";
        if (ad.TryGetValue("county", out v)) geo.District = v + "";
        if (ad.TryGetValue("town", out v)) geo.Township = v + "";
        if (ad.TryGetValue("road", out v)) geo.Street = v + "";
        if (ad.TryGetValue("house_number", out v)) geo.StreetNumber = v + "";
    }
    #endregion
}
