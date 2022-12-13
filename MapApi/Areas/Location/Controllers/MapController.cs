using System.ComponentModel;
using IoTWeb;
using MapApi.Areas.Location;
using NewLife.Cube;

namespace MapApi.Areas.Location.Controllers;

/// <summary>地图设置控制器</summary>
[DisplayName("地图设置")]
[LocationArea]
public class MapController : ConfigController<MapSetting>
{
}
