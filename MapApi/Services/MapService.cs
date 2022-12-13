﻿using MapApi.Locations;
using MapApi.Models;
using Microsoft.Extensions.Options;
using NewLife;
using NewLife.Caching;
using NewLife.Data;
using NewLife.Log;
using NewLife.Yun;

namespace MapApi.Services;

public class MapService
{
    private readonly IMap _map;
    private readonly IOptionsMonitor<MapOptions> _mapOptions;
    private readonly ITracer _tracer;
    private readonly ICache _cache = Cache.Default;

    public MapService(IMap map, IOptionsMonitor<MapOptions> mapOptions, ITracer tracer)
    {
        _map = map;
        _mapOptions = mapOptions;
        _tracer = tracer;

        map.AppKey = mapOptions.CurrentValue.Key;

        // 使用84坐标
        if (map is BaiduMap baidu)
            baidu.CoordType = "wgs84ll";
    }

    public IGeo GetAddress(Double longitude, Double latitude, Int32 days = 0)
    {
        if (longitude == 0 && latitude == 0) return null;

        // 查内存缓存
        var key = $"{longitude},{latitude}";
        if (_cache.TryGetValue<IGeo>(key, out var gd) && gd != null) return gd;

        var opt = _mapOptions.CurrentValue;
        if (days <= 0 && opt.CacheDays > 0) days = opt.CacheDays;

        var point = new GeoPoint(longitude, latitude);
        using var span = _tracer?.NewSpan(nameof(GetAddress), point);
        try
        {
            // 数据库查找，不存在则添加，支持并行执行
            gd = Geo9.FindByHash(point);
            if (gd == null || gd.Address.IsNullOrEmpty() ||
                gd.Longitude == 0 || gd.LongitudeBd09 == 0 || gd.LongitudeGcj02 == 0 ||
                gd.UpdateTime.AddDays(days) < DateTime.Now)
            {
                // 调用接口
                var geoAddress = _map.GetReverseGeoAsync(point).Result;
                if (geoAddress != null)
                {
                    span?.SetTag(geoAddress);

                    // 坐标系转换
                    var bd09 = geoAddress.Location;
                    if (bd09 == null || bd09.Longitude == 0)
                        _map.ConvertAsync(new[] { point }, "wgs84ll", "bd09ll").Result?.FirstOrDefault();

                    var gcj02 = _map.ConvertAsync(new[] { point }, "wgs84ll", "gcj02").Result?.FirstOrDefault();

                    gd = Geo9.Upsert(point, geoAddress, bd09, gcj02, days);

                    Geo6.Upsert(point, geoAddress, bd09, gcj02, days);
                    Geo7.Upsert(point, geoAddress, bd09, gcj02, days);
                    Geo8.Upsert(point, geoAddress, bd09, gcj02, days);
                }
            }

            // 缓存
            _cache.Set(key, gd, 20 * 60);

            return gd;
        }
        catch (Exception ex)
        {
            span?.SetError(ex, null);

            return null;
        }
    }
}