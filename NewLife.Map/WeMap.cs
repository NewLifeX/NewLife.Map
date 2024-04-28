using System.ComponentModel;
using System.Web;
using NewLife.Data;
using NewLife.Map.Models;
using NewLife.Serialization;

namespace NewLife.Map;

/// <summary>腾讯地图</summary>
/// <remarks>
/// 腾讯地图文档地址：  https://lbs.qq.com/service/webService/webServiceGuide/webServiceGcoder
/// https://lbs.qq.com/service/webService/webServiceGuide/webServiceGeocoder
/// </remarks>
[DisplayName("腾讯地图")]
public class WeMap : Map, IMap
{
    #region 构造
    /// <summary>腾讯地图</summary>
    public WeMap()
    {
        Server = "https://apis.map.qq.com";
        //AppKey = "" +
        //    "YGEBZ-BDCCX-AJG4X-ZUH6W-MESMV-P2BFF";//Yann
        KeyName = "key";
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
        var status = dic["status"].ToInt(-1);//0为成功
        if (status != 0)
        {
            var msg = $"{dic["message"]}";

            // 删除无效密钥
            if (IsValidKey(msg)) RemoveKey(LastKey, DateTime.Now.AddHours(1));

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
    /// <remarks>
    /// https://lbs.qq.com/service/webService/webServiceGuide/webServiceGeocoder
    /// 未使用smart_address参数，（智能地址解析作为高级版服务，还可支持地址标准化整理、补全、地址切分及要素识别、提取姓名与手机号的功能。）
    /// </remarks>
    public async Task<IDictionary<String, Object>> GetGeocoderAsync(String address, String? city = null, String? coordtype = null)
    {
        if (address.IsNullOrEmpty()) throw new ArgumentNullException(nameof(address));

        // 编码
        city = HttpUtility.UrlEncode(city);
        address = HttpUtility.UrlEncode(address);

        var url = $"https://apis.map.qq.com/ws/geocoder/v1/?output=json";
        if (!String.IsNullOrWhiteSpace(city)) url += $"&region={city}";//region为城市可选参数，如果address里包含城市信息，可以不带region参数请求
        url += $"&address={city}{address}";
        //url += $"&smart_address={city}{address}";//智能接口，高级服务，你懂的~

        return await InvokeAsync<IDictionary<String, Object>>(url, "result");
    }

    /// <summary>查询地址获取坐标</summary>
    /// <param name="address">地址</param>
    /// <param name="city">城市</param>
    /// <param name="coordtype"></param>
    /// <param name="formatAddress">是否格式化地址。</param>
    /// <returns></returns>
    public async Task<GeoAddress?> GetGeoAsync(String address, String? city = null, String? coordtype = null, Boolean formatAddress = false)
    {
        var rs = await GetGeocoderAsync(address, city, coordtype);
        if (rs == null || rs.Count == 0) return null;

        if (rs["location"] is not IDictionary<String, Object> ds || ds.Count < 2) return null;

        var geo = new GeoAddress
        {
            Location = new(ds["lng"], ds["lat"])
        };

        var reader = new JsonReader();
        reader.ToObject(rs, null, geo);

        if (rs["ad_info"] is IDictionary<String, Object> ad && ad.Count > 0) geo.Code = ad["adcode"].ToInt();

        {
            //省、市、县/乡镇、街道、信息
            if (rs["address_components"] is IDictionary<String, Object> addressComponents)
            {
                geo.Province = $"{addressComponents["province"]}";
                geo.City = $"{addressComponents["city"]}";
                geo.District = $"{addressComponents["district"]}";
                geo.Street = $"{addressComponents["street"]}";
                geo.StreetNumber = $"{addressComponents["street_number"]}";
            }
        }
        geo.Confidence = rs["reliability"].ToInt() * 10;//可信度 
        geo.Level = rs["level"] + "";

        if (formatAddress)
        {
            var geo2 = await GetReverseGeoAsync(geo.Location, coordtype);
            if (geo2 != null)
            {
                geo2.Comprehension = geo.Comprehension;
                geo2.Confidence = geo.Confidence;
                if (geo2.Level.IsNullOrEmpty()) geo2.Level = geo.Level;

                geo = geo2;
            }
        }

        if (geo.Address.IsNullOrEmpty())
        {
            geo.Address = $"{geo.Province}{geo.City}{geo.District}{geo.Street}{rs["title"]}";
        }

        // 替换竖线
        if (!geo.Address.IsNullOrEmpty()) geo.Address = geo.Address.Replace("|", null);

        return geo;
    }
    #endregion

    #region 逆地理编码
    /// <summary>根据坐标获取地址：</summary>
    /// <param name="point"></param>
    /// <param name="coordtype"></param>
    /// <returns></returns>
    /// <remarks> 不会返回周边地点（POI）列表
    /// https://lbs.qq.com/service/webService/webServiceGuide/webServiceGcoder
    /// </remarks>
    public async Task<IDictionary<String, Object>> GetReverseGeocoderAsync(GeoPoint point, String? coordtype)
    {
        if (point.Longitude < 0.1 || point.Latitude < 0.1) throw new ArgumentNullException(nameof(point));

        var url = $"/ws/geocoder/v1/?location={point.Latitude},{point.Longitude}&output=json";//&get_poi=1

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

        var geo = new GeoAddress { Address = rs["address"] + "", Location = point };

        if (rs["formatted_addresses"] is IDictionary<String, Object> formattedAddresses)
        {
            // 推荐使用的地址描述，描述精确性较高
            geo.Name = formattedAddresses["recommend"] + "";

            var addr = formattedAddresses["standard_address"] + "";
            if (!addr.IsNullOrEmpty())
            {
                geo.Title = geo.Address;
                geo.Address = addr;
            }
        }

        if (rs["address_component"] is IDictionary<String, Object> component)
        {
            var reader = new JsonReader();
            reader.ToObject(component, null, geo);
            geo.StreetNumber = $"{component["street_number"]}";
            geo.Country = $"{component["nation"]}";
        }

        if (rs["ad_info"] is IDictionary<String, Object> adInfo)
        {
            geo.Code = adInfo["adcode"].ToInt();
        }

        // 替换竖线
        if (!geo.Address.IsNullOrEmpty()) geo.Address = geo.Address.Replace("|", null);

        return geo;
    }
    #endregion

    #region 路径规划
    /// <summary>计算距离和驾车时间</summary>
    /// <remarks>
    /// https://lbs.qq.com/service/webService/webServiceGuide/webServiceRoute
    /// type:  1：驾车导航距离  3：自行车 2：步行规划距离 
    /// distance    路径距离，单位：米
    /// duration    预计行驶时间，单位：秒
    /// </remarks>
    /// <param name="origin">起始位置坐标</param>
    /// <param name="destination">目的地坐标</param>
    /// <param name="coordtype"></param>
    /// <param name="type">  1：驾车导航距离  3：自行车 2：步行规划距离 </param>
    /// <returns></returns>
    public async Task<Driving?> GetDistanceAsync(GeoPoint origin, GeoPoint destination, String? coordtype, Int32 type = 1)
    {
        if (origin == null || origin.Longitude < 1 && origin.Latitude < 1) throw new ArgumentNullException(nameof(origin));
        if (destination == null || destination.Longitude < 1 && destination.Latitude < 1) throw new ArgumentNullException(nameof(destination));

        if (type <= 0 || type > 3) type = 1;

        var url = $"/ws/direction/v1/{DrivingType(type)}?from={origin.Latitude},{origin.Longitude}&to={destination.Latitude},{destination.Longitude}&output=json";

        var list = await InvokeAsync<IDictionary<String, Object>>(url, "result");
        if (list == null || list.Count == 0) return null;

        if (list["routes"] is not IList<Object> elements) return null;
        if (elements.FirstOrDefault() is not IDictionary<String, Object> geo) return null;

        var rs = new Driving
        {
            Distance = geo["distance"].ToInt(),
            Duration = geo["duration"].ToInt() * 60
        };
        return rs;
    }

    /// <summary>获取参数字符串</summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <remarks> driving：驾车 walking：步行 bicycling：自行车  </remarks>
    private String DrivingType(Int32 type)
    {
        var str = type switch
        {
            2 => "walking",//步行
            3 => "bicycling",//自行车
            _ => "driving",//驾车
        };
        return str;
    }
    #endregion

    #region 行政区划

    #endregion

    #region IP定位
    /// <summary>IP定位</summary>
    /// <remarks>https://lbs.qq.com/service/webService/webServiceGuide/webServiceIp 
    /// </remarks>
    /// <param name="ip">IP</param>
    /// <returns></returns>
    public async Task<IDictionary<String, Object?>?> IpLocationAsync(String ip)
    {
        var url = $"/ws/location/v1/ip?ip={ip}";

        var dic = await InvokeAsync<IDictionary<String, Object>>(url, "result");
        if (dic == null || dic.Count == 0) return null;
        if (dic["ad_info"] is not IDictionary<String, Object?> rs) return null;

        if (dic.TryGetValue("ip", out var ipValue)) rs["ip"] = ipValue;
        if (dic["location"] is IDictionary<String, Object> locationDic)
        {
            rs.Remove("location");
            rs.Merge(locationDic);
        }
        return rs;
    }
    #endregion

    #region 密钥管理
    private readonly String[] _KeyWords = ["TOO_FREQUENT", "LIMIT", "NOMATCH", "RECYCLED", "key"];
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