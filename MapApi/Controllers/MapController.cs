using MapApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Cube;
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

    [AllowAnonymous]
    [HttpGet(nameof(Geo))]
    public async Task<ActionResult> Geo(Double lng, Double lat, String coordtype)
    {
        using var span = _tracer?.NewSpan(nameof(ReverseGeo), new { lng, lat, coordtype });
        try
        {
            if (lng == 0 || lat == 0) throw new ArgumentNullException(nameof(lng), "坐标为空");

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

    [AllowAnonymous]
    [HttpGet(nameof(ReverseGeo))]
    public async Task<ActionResult> ReverseGeo(Double lng, Double lat, String coordtype)
    {
        using var span = _tracer?.NewSpan(nameof(ReverseGeo), new { lng, lat, coordtype });
        try
        {
            if (lng == 0 || lat == 0) throw new ArgumentNullException(nameof(lng), "坐标为空");

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
}