using IoTWeb;
using MapApi.Locations;
using NewLife;
using NewLife.Caching;
using NewLife.Data;
using NewLife.Log;
using NewLife.Map;
using NewLife.Map.Models;

namespace MapApi.Services;

public class MapService
{
    private readonly ITracer _tracer;
    private readonly ICache _cache = Cache.Default;

    public MapService(ITracer tracer)
    {
        _tracer = tracer;
    }

    private IList<IMap> _maps;
    private Int32 _mapIndex;
    private IMap GetMap()
    {
        if (_maps == null)
        {
            var ms = new List<IMap>();
            var list = MapProvider.FindAll();
            foreach (var item in list)
            {
                if (!item.Enable) continue;

                // 创建地图实现
                var map = MapFactory.Create(item.Kind);
                if (map == null) continue;

                // 需要 Key 的普通厂商必须配置 Key；无 Key 厂商（如 Nominatim）与聚合平台（NewLife）不要求
                if (item.Kind < MapKinds.NewLife && map is Map { NeedKey: true } && item.AppKey.IsNullOrEmpty()) continue;

                map.AppKey = item.AppKey;
                if (!item.Server.IsNullOrEmpty() && map is Map mp) mp.Server = item.Server;

                ms.Add(map);
            }

            _maps = ms;
        }

        if (_maps.Count == 0) throw new InvalidOperationException("未找到可用地图服务提供者");

        var idx = Interlocked.Increment(ref _mapIndex);
        return _maps[idx % _maps.Count];
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
                var map = GetMap();
                var geoAddress = await map.GetReverseGeoAsync(point, coordtype);
                if (geoAddress != null)
                {
                    span?.SetTag(geoAddress);

                    // 坐标系转换
                    GeoPoint? wgs84 = null;
                    GeoPoint? bd09 = null;
                    GeoPoint? gcj02 = null;

                    if (coordtype.EqualIgnoreCase("wgs84", "wgs84ll"))
                    {
                        wgs84 = point;
                        bd09 = await map.ConvertAsync(point, "wgs84ll", "bd09ll");
                        gcj02 = await map.ConvertAsync(point, "wgs84ll", "gcj02");
                    }
                    else if (coordtype.EqualIgnoreCase("bd09", "bd09ll"))
                    {
                        bd09 = point;
                        //bd09 = await map.ConvertAsync(point, "wgs84ll", "bd09ll");
                        gcj02 = await map.ConvertAsync(point, "bd09ll", "gcj02");
                    }
                    else if (coordtype.EqualIgnoreCase("gcj02", "gcj02ll"))
                    {
                        bd09 = await map.ConvertAsync(point, "gcj02ll", "bd09ll");
                        //gcj02 = await map.ConvertAsync(point, "wgs84ll", "gcj02");
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

    /// <summary>正向地理编码。地址转坐标</summary>
    /// <param name="address">地址</param>
    /// <param name="city">城市</param>
    /// <param name="coordtype">坐标系。wgs84/gcj02/bd09</param>
    /// <returns>地理模型；地址为空或厂商无结果时返回 null</returns>
    public async Task<GeoModel?> GetGeo(String address, String? city = null, String? coordtype = null)
    {
        if (address.IsNullOrEmpty()) return null;

        // 内存缓存
        var key = $"geo:{address},{city},{coordtype}";
        if (_cache.TryGetValue<GeoModel>(key, out var gm) && gm != null) return gm;

        using var span = _tracer?.NewSpan(nameof(GetGeo), new { address, city, coordtype });
        try
        {
            // 调用厂商正向地理编码
            var map = GetMap();
            var geoAddress = await map.GetGeoAsync(address, city, coordtype);
            if (geoAddress == null) return null;

            gm = new GeoModel
            {
                Longitude = geoAddress.Location?.Longitude ?? 0,
                Latitude = geoAddress.Location?.Latitude ?? 0,
                Address = geoAddress.Address,
                Title = geoAddress.Title,
                AreaCode = geoAddress.Code,
                Province = geoAddress.Province,
                City = geoAddress.City,
                District = geoAddress.District,
            };

            _cache.Set(key, gm, 10 * 60);

            return gm;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);

            throw;
        }
    }

    /// <summary>计算距离和驾车时间</summary>
    /// <param name="origin">起始坐标</param>
    /// <param name="destination">目的坐标</param>
    /// <param name="coordtype">坐标系</param>
    /// <param name="type">路径计算方式</param>
    /// <returns>驾驶距离</returns>
    public async Task<Driving?> GetDistance(GeoPoint origin, GeoPoint destination, String? coordtype = null, Int32 type = 0)
    {
        var map = GetMap();

        return await map.GetDistanceAsync(origin, destination, coordtype, type);
    }
}