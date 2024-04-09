using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace MapApi.Locations;

/// <summary>地理数据6位。根据GeoHash索引地理解析数据，6位精度610米</summary>
public partial interface IGeo
{
    #region 属性
    /// <summary>编号</summary>
    Int32 Id { get; set; }

    /// <summary>编码。GeoHash编码</summary>
    String Hash { get; set; }

    /// <summary>经度</summary>
    Double Longitude { get; set; }

    /// <summary>纬度</summary>
    Double Latitude { get; set; }

    /// <summary>bd09编码。GeoHash编码</summary>
    String HashBd09 { get; set; }

    /// <summary>bd09经度。百度坐标</summary>
    Double LongitudeBd09 { get; set; }

    /// <summary>bd09纬度。百度坐标</summary>
    Double LatitudeBd09 { get; set; }

    /// <summary>gcj02编码。GeoHash编码</summary>
    String HashGcj02 { get; set; }

    /// <summary>gcj02经度。国测局坐标</summary>
    Double LongitudeGcj02 { get; set; }

    /// <summary>gcj02纬度。国测局坐标</summary>
    Double LatitudeGcj02 { get; set; }

    /// <summary>区划编码。最高到乡镇级行政区划编码</summary>
    Int32 Code { get; set; }

    /// <summary>省份</summary>
    Int32 ProvinceId { get; set; }

    /// <summary>城市</summary>
    Int32 CityId { get; set; }

    /// <summary>区县</summary>
    Int32 DistrictId { get; set; }

    /// <summary>地址</summary>
    String Address { get; set; }

    /// <summary>标题。POI语义地址</summary>
    String Title { get; set; }

    /// <summary>创建时间</summary>
    DateTime CreateTime { get; set; }

    /// <summary>更新时间</summary>
    DateTime UpdateTime { get; set; }
    #endregion
}
