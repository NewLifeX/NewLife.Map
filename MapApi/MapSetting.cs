using System.ComponentModel;
using NewLife.Configuration;
using XCode.Configuration;

namespace IoTWeb;

/// <summary>地址配置</summary>
[Config("Map")]
public class MapSetting : Config<MapSetting>
{
    #region 静态
    static MapSetting() => Provider = new DbConfigProvider { UserId = 0, Category = "Map" };
    #endregion

    #region 属性
    /// <summary>启用地图。默认true</summary>
    [Description("启用地图。默认true")]
    public Boolean Enable { get; set; } = true;

    /// <summary>地图提供者。默认NewLife</summary>
    [Description("地图提供者。默认NewLife")]
    public String MapProvider { get; set; } = "NewLife";

    /// <summary>Js地图密钥。浏览器端密钥</summary>
    [Description("Js地图密钥。浏览器端密钥")]
    public String JsKey { get; set; }

    /// <summary>地图服务密钥。WebService密钥</summary>
    [Description("地图服务密钥。WebService密钥")]
    public String ServiceKey { get; set; }

    /// <summary>地图中心城市。默认西安</summary>
    [Description("地图中心城市。默认西安")]
    public String CenterCity { get; set; } = "西安";

    /// <summary>缩放等级。默认6看全国，17最小</summary>
    [Description("缩放等级。默认6看全国，17最小")]
    public Int32 ZoomLevel { get; set; } = 6;

    /// <summary>卫星图。默认false</summary>
    [Description("卫星图。默认false")]
    public Boolean Earth { get; set; }

    /// <summary>缓存天数。更新数据库记录的时间，默认30天</summary>
    [Description("缓存天数。更新数据库记录的时间，默认30天")]
    public Int32 CacheDays { get; set; } = 30;
    #endregion
}