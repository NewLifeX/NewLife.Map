using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace MapApi.Locations;

/// <summary>地理数据7位。根据GeoHash索引地理解析数据，7位精度76米</summary>
[Serializable]
[DataObject]
[Description("地理数据7位。根据GeoHash索引地理解析数据，7位精度76米")]
[BindIndex("IX_Geo7_Hash", false, "Hash")]
[BindIndex("IX_Geo7_HashBd09", false, "HashBd09")]
[BindIndex("IX_Geo7_HashGcj02", false, "HashGcj02")]
[BindIndex("IX_Geo7_Code", false, "Code")]
[BindIndex("IX_Geo7_ProvinceId_CityId_DistrictId", false, "ProvinceId,CityId,DistrictId")]
[BindTable("Geo7", Description = "地理数据7位。根据GeoHash索引地理解析数据，7位精度76米", ConnName = "Location", DbType = DatabaseType.None)]
public partial class Geo7
{
    #region 属性
    private Int32 _Id;
    /// <summary>编号</summary>
    [DisplayName("编号")]
    [Description("编号")]
    [DataObjectField(true, true, false, 0)]
    [BindColumn("Id", "编号", "")]
    public Int32 Id { get => _Id; set { if (OnPropertyChanging("Id", value)) { _Id = value; OnPropertyChanged("Id"); } } }

    private String _Hash;
    /// <summary>编码。GeoHash编码</summary>
    [DisplayName("编码")]
    [Description("编码。GeoHash编码")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("Hash", "编码。GeoHash编码", "")]
    public String Hash { get => _Hash; set { if (OnPropertyChanging("Hash", value)) { _Hash = value; OnPropertyChanged("Hash"); } } }

    private Double _Longitude;
    /// <summary>经度</summary>
    [DisplayName("经度")]
    [Description("经度")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Longitude", "经度", "")]
    public Double Longitude { get => _Longitude; set { if (OnPropertyChanging("Longitude", value)) { _Longitude = value; OnPropertyChanged("Longitude"); } } }

    private Double _Latitude;
    /// <summary>纬度</summary>
    [DisplayName("纬度")]
    [Description("纬度")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Latitude", "纬度", "")]
    public Double Latitude { get => _Latitude; set { if (OnPropertyChanging("Latitude", value)) { _Latitude = value; OnPropertyChanged("Latitude"); } } }

    private String _HashBd09;
    /// <summary>bd09编码。GeoHash编码</summary>
    [DisplayName("bd09编码")]
    [Description("bd09编码。GeoHash编码")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("HashBd09", "bd09编码。GeoHash编码", "")]
    public String HashBd09 { get => _HashBd09; set { if (OnPropertyChanging("HashBd09", value)) { _HashBd09 = value; OnPropertyChanged("HashBd09"); } } }

    private Double _LongitudeBd09;
    /// <summary>bd09经度。百度坐标</summary>
    [DisplayName("bd09经度")]
    [Description("bd09经度。百度坐标")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("LongitudeBd09", "bd09经度。百度坐标", "")]
    public Double LongitudeBd09 { get => _LongitudeBd09; set { if (OnPropertyChanging("LongitudeBd09", value)) { _LongitudeBd09 = value; OnPropertyChanged("LongitudeBd09"); } } }

    private Double _LatitudeBd09;
    /// <summary>bd09纬度。百度坐标</summary>
    [DisplayName("bd09纬度")]
    [Description("bd09纬度。百度坐标")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("LatitudeBd09", "bd09纬度。百度坐标", "")]
    public Double LatitudeBd09 { get => _LatitudeBd09; set { if (OnPropertyChanging("LatitudeBd09", value)) { _LatitudeBd09 = value; OnPropertyChanged("LatitudeBd09"); } } }

    private String _HashGcj02;
    /// <summary>gcj02编码。GeoHash编码</summary>
    [DisplayName("gcj02编码")]
    [Description("gcj02编码。GeoHash编码")]
    [DataObjectField(false, false, true, 50)]
    [BindColumn("HashGcj02", "gcj02编码。GeoHash编码", "")]
    public String HashGcj02 { get => _HashGcj02; set { if (OnPropertyChanging("HashGcj02", value)) { _HashGcj02 = value; OnPropertyChanged("HashGcj02"); } } }

    private Double _LongitudeGcj02;
    /// <summary>gcj02经度。国测局坐标</summary>
    [DisplayName("gcj02经度")]
    [Description("gcj02经度。国测局坐标")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("LongitudeGcj02", "gcj02经度。国测局坐标", "")]
    public Double LongitudeGcj02 { get => _LongitudeGcj02; set { if (OnPropertyChanging("LongitudeGcj02", value)) { _LongitudeGcj02 = value; OnPropertyChanged("LongitudeGcj02"); } } }

    private Double _LatitudeGcj02;
    /// <summary>gcj02纬度。国测局坐标</summary>
    [DisplayName("gcj02纬度")]
    [Description("gcj02纬度。国测局坐标")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("LatitudeGcj02", "gcj02纬度。国测局坐标", "")]
    public Double LatitudeGcj02 { get => _LatitudeGcj02; set { if (OnPropertyChanging("LatitudeGcj02", value)) { _LatitudeGcj02 = value; OnPropertyChanged("LatitudeGcj02"); } } }

    private Int32 _Code;
    /// <summary>区划编码。最高到乡镇级行政区划编码</summary>
    [DisplayName("区划编码")]
    [Description("区划编码。最高到乡镇级行政区划编码")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("Code", "区划编码。最高到乡镇级行政区划编码", "")]
    public Int32 Code { get => _Code; set { if (OnPropertyChanging("Code", value)) { _Code = value; OnPropertyChanged("Code"); } } }

    private Int32 _ProvinceId;
    /// <summary>省份</summary>
    [DisplayName("省份")]
    [Description("省份")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("ProvinceId", "省份", "")]
    public Int32 ProvinceId { get => _ProvinceId; set { if (OnPropertyChanging("ProvinceId", value)) { _ProvinceId = value; OnPropertyChanged("ProvinceId"); } } }

    private Int32 _CityId;
    /// <summary>城市</summary>
    [DisplayName("城市")]
    [Description("城市")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("CityId", "城市", "")]
    public Int32 CityId { get => _CityId; set { if (OnPropertyChanging("CityId", value)) { _CityId = value; OnPropertyChanged("CityId"); } } }

    private Int32 _DistrictId;
    /// <summary>区县</summary>
    [DisplayName("区县")]
    [Description("区县")]
    [DataObjectField(false, false, false, 0)]
    [BindColumn("DistrictId", "区县", "")]
    public Int32 DistrictId { get => _DistrictId; set { if (OnPropertyChanging("DistrictId", value)) { _DistrictId = value; OnPropertyChanged("DistrictId"); } } }

    private String _Address;
    /// <summary>地址</summary>
    [DisplayName("地址")]
    [Description("地址")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("Address", "地址", "")]
    public String Address { get => _Address; set { if (OnPropertyChanging("Address", value)) { _Address = value; OnPropertyChanged("Address"); } } }

    private String _Title;
    /// <summary>标题。POI语义地址</summary>
    [DisplayName("标题")]
    [Description("标题。POI语义地址")]
    [DataObjectField(false, false, true, 200)]
    [BindColumn("Title", "标题。POI语义地址", "", Master = true)]
    public String Title { get => _Title; set { if (OnPropertyChanging("Title", value)) { _Title = value; OnPropertyChanged("Title"); } } }

    private DateTime _CreateTime;
    /// <summary>创建时间</summary>
    [DisplayName("创建时间")]
    [Description("创建时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("CreateTime", "创建时间", "")]
    public DateTime CreateTime { get => _CreateTime; set { if (OnPropertyChanging("CreateTime", value)) { _CreateTime = value; OnPropertyChanged("CreateTime"); } } }

    private DateTime _UpdateTime;
    /// <summary>更新时间</summary>
    [DisplayName("更新时间")]
    [Description("更新时间")]
    [DataObjectField(false, false, true, 0)]
    [BindColumn("UpdateTime", "更新时间", "")]
    public DateTime UpdateTime { get => _UpdateTime; set { if (OnPropertyChanging("UpdateTime", value)) { _UpdateTime = value; OnPropertyChanged("UpdateTime"); } } }
    #endregion

    #region 获取/设置 字段值
    /// <summary>获取/设置 字段值</summary>
    /// <param name="name">字段名</param>
    /// <returns></returns>
    public override Object this[String name]
    {
        get => name switch
        {
            "Id" => _Id,
            "Hash" => _Hash,
            "Longitude" => _Longitude,
            "Latitude" => _Latitude,
            "HashBd09" => _HashBd09,
            "LongitudeBd09" => _LongitudeBd09,
            "LatitudeBd09" => _LatitudeBd09,
            "HashGcj02" => _HashGcj02,
            "LongitudeGcj02" => _LongitudeGcj02,
            "LatitudeGcj02" => _LatitudeGcj02,
            "Code" => _Code,
            "ProvinceId" => _ProvinceId,
            "CityId" => _CityId,
            "DistrictId" => _DistrictId,
            "Address" => _Address,
            "Title" => _Title,
            "CreateTime" => _CreateTime,
            "UpdateTime" => _UpdateTime,
            _ => base[name]
        };
        set
        {
            switch (name)
            {
                case "Id": _Id = value.ToInt(); break;
                case "Hash": _Hash = Convert.ToString(value); break;
                case "Longitude": _Longitude = value.ToDouble(); break;
                case "Latitude": _Latitude = value.ToDouble(); break;
                case "HashBd09": _HashBd09 = Convert.ToString(value); break;
                case "LongitudeBd09": _LongitudeBd09 = value.ToDouble(); break;
                case "LatitudeBd09": _LatitudeBd09 = value.ToDouble(); break;
                case "HashGcj02": _HashGcj02 = Convert.ToString(value); break;
                case "LongitudeGcj02": _LongitudeGcj02 = value.ToDouble(); break;
                case "LatitudeGcj02": _LatitudeGcj02 = value.ToDouble(); break;
                case "Code": _Code = value.ToInt(); break;
                case "ProvinceId": _ProvinceId = value.ToInt(); break;
                case "CityId": _CityId = value.ToInt(); break;
                case "DistrictId": _DistrictId = value.ToInt(); break;
                case "Address": _Address = Convert.ToString(value); break;
                case "Title": _Title = Convert.ToString(value); break;
                case "CreateTime": _CreateTime = value.ToDateTime(); break;
                case "UpdateTime": _UpdateTime = value.ToDateTime(); break;
                default: base[name] = value; break;
            }
        }
    }
    #endregion

    #region 关联映射
    #endregion

    #region 字段名
    /// <summary>取得地理数据7位字段信息的快捷方式</summary>
    public partial class _
    {
        /// <summary>编号</summary>
        public static readonly Field Id = FindByName("Id");

        /// <summary>编码。GeoHash编码</summary>
        public static readonly Field Hash = FindByName("Hash");

        /// <summary>经度</summary>
        public static readonly Field Longitude = FindByName("Longitude");

        /// <summary>纬度</summary>
        public static readonly Field Latitude = FindByName("Latitude");

        /// <summary>bd09编码。GeoHash编码</summary>
        public static readonly Field HashBd09 = FindByName("HashBd09");

        /// <summary>bd09经度。百度坐标</summary>
        public static readonly Field LongitudeBd09 = FindByName("LongitudeBd09");

        /// <summary>bd09纬度。百度坐标</summary>
        public static readonly Field LatitudeBd09 = FindByName("LatitudeBd09");

        /// <summary>gcj02编码。GeoHash编码</summary>
        public static readonly Field HashGcj02 = FindByName("HashGcj02");

        /// <summary>gcj02经度。国测局坐标</summary>
        public static readonly Field LongitudeGcj02 = FindByName("LongitudeGcj02");

        /// <summary>gcj02纬度。国测局坐标</summary>
        public static readonly Field LatitudeGcj02 = FindByName("LatitudeGcj02");

        /// <summary>区划编码。最高到乡镇级行政区划编码</summary>
        public static readonly Field Code = FindByName("Code");

        /// <summary>省份</summary>
        public static readonly Field ProvinceId = FindByName("ProvinceId");

        /// <summary>城市</summary>
        public static readonly Field CityId = FindByName("CityId");

        /// <summary>区县</summary>
        public static readonly Field DistrictId = FindByName("DistrictId");

        /// <summary>地址</summary>
        public static readonly Field Address = FindByName("Address");

        /// <summary>标题。POI语义地址</summary>
        public static readonly Field Title = FindByName("Title");

        /// <summary>创建时间</summary>
        public static readonly Field CreateTime = FindByName("CreateTime");

        /// <summary>更新时间</summary>
        public static readonly Field UpdateTime = FindByName("UpdateTime");

        static Field FindByName(String name) => Meta.Table.FindByName(name);
    }

    /// <summary>取得地理数据7位字段名称的快捷方式</summary>
    public partial class __
    {
        /// <summary>编号</summary>
        public const String Id = "Id";

        /// <summary>编码。GeoHash编码</summary>
        public const String Hash = "Hash";

        /// <summary>经度</summary>
        public const String Longitude = "Longitude";

        /// <summary>纬度</summary>
        public const String Latitude = "Latitude";

        /// <summary>bd09编码。GeoHash编码</summary>
        public const String HashBd09 = "HashBd09";

        /// <summary>bd09经度。百度坐标</summary>
        public const String LongitudeBd09 = "LongitudeBd09";

        /// <summary>bd09纬度。百度坐标</summary>
        public const String LatitudeBd09 = "LatitudeBd09";

        /// <summary>gcj02编码。GeoHash编码</summary>
        public const String HashGcj02 = "HashGcj02";

        /// <summary>gcj02经度。国测局坐标</summary>
        public const String LongitudeGcj02 = "LongitudeGcj02";

        /// <summary>gcj02纬度。国测局坐标</summary>
        public const String LatitudeGcj02 = "LatitudeGcj02";

        /// <summary>区划编码。最高到乡镇级行政区划编码</summary>
        public const String Code = "Code";

        /// <summary>省份</summary>
        public const String ProvinceId = "ProvinceId";

        /// <summary>城市</summary>
        public const String CityId = "CityId";

        /// <summary>区县</summary>
        public const String DistrictId = "DistrictId";

        /// <summary>地址</summary>
        public const String Address = "Address";

        /// <summary>标题。POI语义地址</summary>
        public const String Title = "Title";

        /// <summary>创建时间</summary>
        public const String CreateTime = "CreateTime";

        /// <summary>更新时间</summary>
        public const String UpdateTime = "UpdateTime";
    }
    #endregion
}
