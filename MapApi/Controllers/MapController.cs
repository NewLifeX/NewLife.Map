using MapApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
using NewLife.Data;
using NewLife.Log;
using NewLife.Map.Models;
using XCode.Membership;

namespace MapApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MapController : ControllerBaseX
{
    private readonly MapService _mapService;
    private readonly ITracer _tracer;

    public MapController(MapService mapService, ITracer tracer)
    {
        _mapService = mapService;
        _tracer = tracer;
    }

    /// <summary>逆地理编码。坐标转地址</summary>
    /// <param name="lng">经度</param>
    /// <param name="lat">纬度</param>
    /// <param name="coordtype">坐标系。wgs84/gcj02/bd09</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet(nameof(Geo))]
    public async Task<ActionResult> Geo(Double lng, Double lat, String coordtype)
    {
        using var span = _tracer?.NewSpan(nameof(ReverseGeo), new { lng, lat, coordtype });
        try
        {
            if (lng == 0 || lat == 0) throw new ArgumentNullException(nameof(lng), L.T("Coordinates are empty."));

            if (coordtype.IsNullOrEmpty()) coordtype = "wgs84";
            coordtype = coordtype.ToLower();
            if (!coordtype.EqualIgnoreCase("wgs84", "wgs84ll", "bd09", "bd09ll", "gcj02", "gcj02ll")) throw new ArgumentOutOfRangeException(nameof(coordtype));

            var geo = await _mapService.GetAddress(lng, lat, coordtype) ?? throw new Exception("error");
            var gm = new GeoModel
            {
                Hash = geo.Hash,
                Longitude = geo.Longitude,
                Latitude = geo.Latitude,
                Address = geo.Address,
                Title = geo.Title,

                LongitudeBd09 = geo.LongitudeBd09,
                LatitudeBd09 = geo.LatitudeBd09,

                LongitudeGcj02 = geo.LongitudeGcj02,
                LatitudeGcj02 = geo.LatitudeGcj02,

                AreaCode = geo.Code,
                ProvinceId = geo.ProvinceId,
                CityId = geo.CityId,
                DistrictId = geo.DistrictId,

                AreaName = Area.FindByID(geo.Code)?.Path,
                Province = Area.FindByID(geo.ProvinceId)?.Name,
                City = Area.FindByID(geo.CityId)?.Name,
                District = Area.FindByID(geo.DistrictId)?.Name,
            };

            return Json(0, null, gm);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            XTrace.WriteException(ex);

            return Json(500, ex.Message, ex);
        }
    }

    /// <summary>逆地理编码。坐标转地址，别名端点</summary>
    /// <param name="lng">经度</param>
    /// <param name="lat">纬度</param>
    /// <param name="coordtype">坐标系。wgs84/gcj02/bd09</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet(nameof(ReverseGeo))]
    public async Task<ActionResult> ReverseGeo(Double lng, Double lat, String coordtype)
    {
        using var span = _tracer?.NewSpan(nameof(ReverseGeo), new { lng, lat, coordtype });
        try
        {
            if (lng == 0 || lat == 0) throw new ArgumentNullException(nameof(lng), L.T("Coordinates are empty."));

            if (coordtype.IsNullOrEmpty()) coordtype = "wgs84";
            coordtype = coordtype.ToLower();
            if (!coordtype.EqualIgnoreCase("wgs84", "wgs84ll", "bd09", "bd09ll", "gcj02", "gcj02ll")) throw new ArgumentOutOfRangeException(nameof(coordtype));

            var geo = await _mapService.GetAddress(lng, lat, coordtype) ?? throw new Exception("error");
            var gm = new GeoModel
            {
                Hash = geo.Hash,
                Longitude = geo.Longitude,
                Latitude = geo.Latitude,
                Address = geo.Address,
                Title = geo.Title,

                LongitudeBd09 = geo.LongitudeBd09,
                LatitudeBd09 = geo.LatitudeBd09,

                LongitudeGcj02 = geo.LongitudeGcj02,
                LatitudeGcj02 = geo.LatitudeGcj02,

                AreaCode = geo.Code,
                ProvinceId = geo.ProvinceId,
                CityId = geo.CityId,
                DistrictId = geo.DistrictId,

                AreaName = Area.FindByID(geo.Code)?.Path,
                Province = Area.FindByID(geo.ProvinceId)?.Name,
                City = Area.FindByID(geo.CityId)?.Name,
                District = Area.FindByID(geo.DistrictId)?.Name,
            };

            return Json(0, null, gm);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            XTrace.WriteException(ex);

            return Json(500, ex.Message, ex);
        }
    }

    /// <summary>正向地理编码。地址转坐标</summary>
    /// <param name="address">地址</param>
    /// <param name="city">城市</param>
    /// <param name="coordtype">坐标系。wgs84/gcj02/bd09</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet(nameof(GeoAddress))]
    public async Task<ActionResult> GeoAddress(String address, String city = null, String coordtype = null)
    {
        using var span = _tracer?.NewSpan(nameof(GeoAddress), new { address, city, coordtype });
        try
        {
            if (address.IsNullOrEmpty()) throw new ArgumentNullException(nameof(address), L.T("Address is empty."));

            var gm = await _mapService.GetGeo(address, city, coordtype) ?? throw new Exception("error");

            return Json(0, null, gm);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            XTrace.WriteException(ex);

            return Json(500, ex.Message, ex);
        }
    }

    /// <summary>计算距离和驾车时间</summary>
    /// <param name="fromLng">起始经度</param>
    /// <param name="fromLat">起始纬度</param>
    /// <param name="toLng">目的经度</param>
    /// <param name="toLat">目的纬度</param>
    /// <param name="coordtype">坐标系</param>
    /// <param name="type">路径计算方式</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet(nameof(Distance))]
    public async Task<ActionResult> Distance(Double fromLng, Double fromLat, Double toLng, Double toLat, String coordtype = null, Int32 type = 0)
    {
        using var span = _tracer?.NewSpan(nameof(Distance), new { fromLng, fromLat, toLng, toLat, coordtype, type });
        try
        {
            if (fromLng == 0 || fromLat == 0 || toLng == 0 || toLat == 0) throw new ArgumentNullException(nameof(fromLng), L.T("Coordinates are empty."));

            var d = await _mapService.GetDistance(new GeoPoint(fromLng, fromLat), new GeoPoint(toLng, toLat), coordtype, type) ?? throw new Exception("error");

            return Json(0, null, d);
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);
            XTrace.WriteException(ex);

            return Json(500, ex.Message, ex);
        }
    }

    /// <summary>IP 解析。离线 IP 库查询物理地址</summary>
    /// <param name="ip">IP 地址</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet(nameof(Ip))]
    public ActionResult Ip(String ip)
    {
        try
        {
            if (ip.IsNullOrEmpty()) throw new ArgumentNullException(nameof(ip), L.T("IP is empty."));

            var (area, addr) = MapService.IpDb.GetAddress(ip);
            if (area.IsNullOrEmpty()) return Json(1, "未查询到地址，请确认 IP 数据库已就绪", null);

            return Json(0, null, new { ip, area, addr });
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);

            return Json(500, ex.Message, ex);
        }
    }
}