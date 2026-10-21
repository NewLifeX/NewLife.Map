using System.ComponentModel;
using NewLife.Configuration;
using XCode.Configuration;

namespace IoTWeb;

/// <summary>地址配置</summary>
[Config("Map")]
[DisplayName("Map settings")]
public class MapSetting : Config<MapSetting>
{
    #region 静态
    static MapSetting() => Provider = new DbConfigProvider { UserId = 0, Category = "Map" };
    #endregion

    #region 属性
    /// <summary>启用地图。默认true</summary>
    [Description("Enable map. Defaults to true.")]
    public Boolean Enable { get; set; } = true;

    /// <summary>地图提供者。默认NewLife</summary>
    [Description("Map provider. Defaults to NewLife.")]
    public String MapProvider { get; set; } = "NewLife";

    /// <summary>Js地图密钥。浏览器端密钥</summary>
    [Description("Browser-side map key.")]
    public String JsKey { get; set; }

    /// <summary>地图服务密钥。WebService密钥</summary>
    [Description("Web service map key.")]
    public String ServiceKey { get; set; }

    /// <summary>地图中心城市。默认西安</summary>
    [Description("Map center city. Defaults to Xi'an.")]
    public String CenterCity { get; set; } = "西安";

    /// <summary>缩放等级。默认6看全国，17最小</summary>
    [Description("Zoom level. 6 shows the whole country, 17 is the closest.")]
    public Int32 ZoomLevel { get; set; } = 6;

    /// <summary>卫星图。默认false</summary>
    [Description("Satellite imagery. Defaults to false.")]
    public Boolean Earth { get; set; }

    /// <summary>缓存天数。更新数据库记录的时间，默认30天</summary>
    [Description("Cache days. Days to refresh database records, defaults to 30.")]
    public Int32 CacheDays { get; set; } = 30;
    #endregion
}