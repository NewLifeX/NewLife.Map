using System;
using NewLife.Data;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>OSM/Nominatim 开源地图提供者测试。免费无Key，可直接请求真实服务</summary>
public class NominatimMapTests
{
    private readonly NominatimMap _map = new();

    [Fact]
    public async void Geocoder()
    {
        var ga = await _map.GetGeoAsync("上海中心");

        Assert.NotNull(ga);
        Assert.NotNull(ga.Location);
        Assert.True(ga.Location.Value.Longitude > 100);
        Assert.True(ga.Location.Value.Latitude > 20);
        Assert.False(String.IsNullOrEmpty(ga.Address));
    }

    [Fact]
    public async void ReverseGeocoder()
    {
        // 上海中心，WGS84 坐标
        var point = new GeoPoint(121.511999, 31.239184);

        var ga = await _map.GetReverseGeoAsync(point, null);

        Assert.NotNull(ga);
        Assert.False(String.IsNullOrEmpty(ga.Address));
        Assert.False(String.IsNullOrEmpty(ga.Country));
    }

    [Fact]
    public async void Geocoder_EmptyAddress_Throws()
    {
        // 空地址触发参数校验，不依赖网络
        await Assert.ThrowsAsync<ArgumentNullException>(() => _map.GetGeoAsync(""));
    }

    [Fact]
    public async void GetDistanceAsync_Null()
    {
        // Nominatim 不提供驾车距离，返回 null
        var d = await _map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), null);

        Assert.Null(d);
    }
}
