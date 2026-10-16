using System;
using System.ComponentModel;
using System.Threading.Tasks;
using NewLife.Data;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>OSM/Nominatim 开源地图提供者测试。免费无Key，可直接请求真实服务</summary>
public class NominatimMapTests
{
    private readonly NominatimMap _map = new();

    [Fact]
    [Trait("Category", "Integration")]
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
    [Trait("Category", "Integration")]
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

    #region 桩测试。本地 HTTP 桩服务替代真实服务，不依赖外部网络
    [Fact]
    [DisplayName("Nominatim_桩服务_正向地理编码解析")]
    public async Task Geocoder_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/search", """
            [{"lon":"121.511999","lat":"31.239184","display_name":"上海中心大厦, 陆家嘴, 浦东新区, 上海市, 中国","name":"上海中心大厦","address":{"country":"中国","state":"上海市","city":"上海市","county":"浦东新区","road":"银城中路","house_number":"501号"}}]
            """);

        var map = new NominatimMap { Server = stub.Server };
        var ga = await map.GetGeoAsync("上海中心大厦");

        Assert.NotNull(ga);
        Assert.NotNull(ga.Location);
        Assert.Equal(121.511999, ga.Location.Value.Longitude);
        Assert.Equal(31.239184, ga.Location.Value.Latitude);
        Assert.Equal("上海中心大厦, 陆家嘴, 浦东新区, 上海市, 中国", ga.Address);
        Assert.Equal("上海中心大厦", ga.Name);
        Assert.Equal("中国", ga.Country);
        Assert.Equal("上海市", ga.Province);
        Assert.Equal("浦东新区", ga.District);
        Assert.Equal("银城中路", ga.Street);
        Assert.Equal("501号", ga.StreetNumber);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("Nominatim_桩服务_逆地理编码解析")]
    public async Task ReverseGeocoder_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/reverse", """
            {"lon":"121.511999","lat":"31.239184","display_name":"上海中心大厦, 陆家嘴, 浦东新区, 上海市, 中国","name":"上海中心大厦","address":{"country":"中国","state":"上海市","road":"银城中路"}}
            """);

        var map = new NominatimMap { Server = stub.Server };
        var ga = await map.GetReverseGeoAsync(new GeoPoint(121.511999, 31.239184), null);

        Assert.NotNull(ga);
        Assert.Equal("上海中心大厦, 陆家嘴, 浦东新区, 上海市, 中国", ga.Address);
        Assert.Equal("中国", ga.Country);
        Assert.Equal("上海市", ga.Province);
        Assert.Equal("银城中路", ga.Street);
        Assert.Empty(stub.Unmatched);
    }
    #endregion
}
