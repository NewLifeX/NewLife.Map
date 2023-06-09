using IoTWeb;
using MapApi.Locations;
using NewLife;
using NewLife.Caching;
using NewLife.Data;
using NewLife.Log;
using NewLife.Map;

namespace MapApi.Services;

public class MapService
{
    private readonly IMap _map;
    private readonly ITracer _tracer;
    private readonly ICache _cache = Cache.Default;

    public MapService(ITracer tracer)
    {
        _tracer = tracer;

        var set = MapSetting.Current;
        if (!set.MapProvider.IsNullOrEmpty())
        {
            _map = Map.Create(set.MapProvider);
            _map.AppKey = set.ServiceKey;
        }
    }

    public async Task<IGeo> GetAddress(Double longitude, Double latitude, String coordtype, Int32 days = 0)
    {
        if (longitude == 0 && latitude == 0) return null;

        // 查内存缓存
        var key = $"{longitude},{latitude},{coordtype}";
        if (_cache.TryGetValue<IGeo>(key, out var gd) && gd != null) return gd;

        var opt = MapSetting.Current;
        if (days <= 0 && opt.CacheDays > 0) days = opt.CacheDays;

        var point = new GeoPoint(longitude, latitude);
        using var span = _tracer?.NewSpan(nameof(GetAddress), point);
        try
        {
            // 数据库查找，不存在则添加，支持并行执行
            gd = coordtype switch
            {
                "wgs84" or "wgs84ll" => Geo9.FindByHash(point),
                "gcj02" or "gcj02ll" => Geo9.FindByHashGcj02(point),
                "bd09" or "bd09ll" => Geo9.FindByHashBd09(point),
                _ => Geo9.FindByHash(point),
            };
            if (gd == null || !gd.IsValid() || gd.UpdateTime.AddDays(days) < DateTime.Now)
            {
                // 调用接口
                var geoAddress = await _map.GetReverseGeoAsync(point, coordtype);
                if (geoAddress != null)
                {
                    span?.SetTag(geoAddress);

                    // 坐标系转换
                    GeoPoint wgs84 = null;
                    GeoPoint bd09 = null;
                    GeoPoint gcj02 = null;

                    if (coordtype.EqualIgnoreCase("wgs84", "wgs84ll"))
                    {
                        wgs84 = point;
                        bd09 = await _map.ConvertAsync(point, "wgs84ll", "bd09ll");
                        gcj02 = await _map.ConvertAsync(point, "wgs84ll", "gcj02");
                    }
                    else if (coordtype.EqualIgnoreCase("bd09", "bd09ll"))
                    {
                        bd09 = point;
                        //bd09 = await _map.ConvertAsync(point, "wgs84ll", "bd09ll");
                        gcj02 = await _map.ConvertAsync(point, "bd09ll", "gcj02");
                    }
                    else if (coordtype.EqualIgnoreCase("gcj02", "gcj02ll"))
                    {
                        bd09 = await _map.ConvertAsync(point, "gcj02ll", "bd09ll");
                        //gcj02 = await _map.ConvertAsync(point, "wgs84ll", "gcj02");
                        gcj02 = point;
                    }

                    gd = Geo9.Upsert(geoAddress, wgs84, bd09, gcj02, days);

                    Geo6.Upsert(geoAddress, wgs84, bd09, gcj02, days);
                    Geo7.Upsert(geoAddress, wgs84, bd09, gcj02, days);
                    Geo8.Upsert(geoAddress, wgs84, bd09, gcj02, days);
                }
            }

            // 缓存
            _cache.Set(key, gd, 10 * 60);

            return gd;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);

            throw;
        }
    }
}