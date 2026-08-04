using NewLife.Data;

namespace NewLife.Map.Models;

/// <summary>Geo模型</summary>
public class GeoModel
{
    #region 属性
    /// <summary>Geo哈希编码。基于wgs84坐标</summary>
    public String? Hash { get; set; }

    /// <summary>经度。wgw84坐标</summary>
    public Double Longitude { get; set; }

    /// <summary>纬度。wgs84坐标</summary>
    public Double Latitude { get; set; }

    /// <summary>地址。例如XX高速、XX路</summary>
    public String? Address { get; set; }

    /// <summary>标题。POI语义地址</summary>
    public String? Title { get; set; }

    /// <summary>百度经度。bd09坐标</summary>
    public Double LongitudeBd09 { get; set; }

    /// <summary>百度纬度。bd09坐标</summary>
    public Double LatitudeBd09 { get; set; }

    /// <summary>火星经度。gcj02坐标，高德、腾讯</summary>
    public Double LongitudeGcj02 { get; set; }

    /// <summary>火星纬度。gcj02坐标，高德、腾讯</summary>
    public Double LatitudeGcj02 { get; set; }

    /// <summary>区域编码。乡镇四级地址，如321324114</summary>
    public Int32 AreaCode { get; set; }

    /// <summary>区域名称。乡镇四级地址</summary>
    public String? AreaName { get; set; }

    /// <summary>省份编码</summary>
    public Int32 ProvinceId { get; set; }

    /// <summary>省份名称</summary>
    public String? Province { get; set; }

    /// <summary>城市编码</summary>
    public Int32 CityId { get; set; }

    /// <summary>城市名称</summary>
    public String? City { get; set; }

    /// <summary>区县编码</summary>
    public Int32 DistrictId { get; set; }

    /// <summary>区县名称</summary>
    public String? District { get; set; }
    #endregion

    #region 方法
    /// <summary>
    /// 转Geo地址模型
    /// </summary>
    /// <returns></returns>
    public GeoAddress ToGeoAddress()
    {
        var geoAddress = new GeoAddress
        {
            Location = new GeoPoint(Longitude, Latitude),
            Address = Address,
            Title = Title,

            Code = DistrictId,
            Province = Province,
            City = City,
            District = District,
            Towncode = AreaCode,
            Township = AreaName,
        };

        return geoAddress;
    }
    #endregion
}