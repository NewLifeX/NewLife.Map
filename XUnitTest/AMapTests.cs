using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using NewLife.Data;
using NewLife.Map;
using NewLife.Map.Models;
using Xunit;

namespace XUnitTest;

public class AMapTests
{
    private readonly AMap _map;
    public AMapTests() => _map = new AMap { AppKey = "2aada76e462af71e1b67ba1df22d0fa4" };

    [Fact]
    [Trait("Category", "Integration")]
    public async void Geocoder()
    {
        var addr = "上海中心大厦";
        var map = _map;
        //var rs = await map.GetGeocoderAsync(addr);

        //Assert.NotNull(rs);
        //Assert.True(rs.ContainsKey("location"));

        var ga = await map.GetGeoAsync(addr, null, null, false);

        Assert.NotNull(ga);
        Assert.Equal(121.505366, ga.Location.Value.Longitude);
        Assert.Equal(31.23351, ga.Location.Value.Latitude);
        Assert.Equal("上海市浦东新区上海中心大厦", ga.Address);
        //Assert.True(ga.Confidence > 0);

        ga = await map.GetGeoAsync(addr, null, null, true);

        Assert.NotNull(ga);
        Assert.Equal(121.505366, ga.Location.Value.Longitude);
        Assert.Equal(31.23351, ga.Location.Value.Latitude);
        Assert.StartsWith("上海市浦东新区陆家嘴街道上海中心大厦", ga.Address);
        Assert.Equal("上海市浦东新区陆家嘴街道银城中路501号东16米", ga.Title);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async void GetDistanceAsync()
    {
        var points = new List<GeoPoint> {
            new(121.51199904625513, 31.239184419374944),
            new(114.21892734521, 29.575429778924)
        };

        var map = _map;
        var drv = await map.GetDistanceAsync(points[0], points[1], "wgs84", 0);

        Assert.NotNull(drv);
        Assert.Equal(851380, drv.Distance);
        Assert.True(Math.Abs(31569 - drv.Duration) < 600);
    }

    //[Fact]
    //public async void ConvertAsync()
    //{
    //    var points = new List<GeoPoint>
    //    {
    //        new() { Longitude = 121.51199904625513, Latitude = 31.239184419374944 },
    //        new() { Longitude = 114.21892734521, Latitude = 29.575429778924 }
    //    };

    //    var map = _map;
    //    var points2 = await map.ConvertAsync(points, "wgs84", "gcj02");

    //    Assert.NotNull(points2);

    //    Assert.Equal(points.Count, points2.Count);
    //    Assert.True(points2[0].Longitude > 0);
    //    Assert.True(points2[0].Latitude > 0);
    //}

    #region 桩测试。本地 HTTP 桩服务替代真实厂商，不依赖外部网络与真实密钥
    [Fact]
    [DisplayName("高德地图_桩服务_正向地理编码解析")]
    public async Task Geocoder_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/v3/geocode/geo", """
            {"status":"1","info":"OK","infocode":"10000","geocodes":[{"formatted_address":"上海市浦东新区陆家嘴街道上海中心大厦","province":"上海市","city":"上海市","district":"浦东新区","adcode":"310115","location":"121.505366,31.23351","level":"门牌号","township":["陆家嘴街道"],"number":["501号"]}]}
            """);

        var map = new AMap { Server = stub.Server, AppKey = "test" };
        var ga = await map.GetGeoAsync("上海中心大厦", null, null, false);

        Assert.NotNull(ga);
        Assert.NotNull(ga.Location);
        Assert.Equal(121.505366, ga.Location.Value.Longitude);
        Assert.Equal(31.23351, ga.Location.Value.Latitude);
        Assert.Equal("上海市浦东新区陆家嘴街道上海中心大厦", ga.Address);
        Assert.Equal(310115, ga.Code);
        Assert.Equal("陆家嘴街道", ga.Township);
        Assert.Equal("501号", ga.StreetNumber);
        Assert.Contains("key=test", map.LastUrl);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("高德地图_桩服务_逆地理编码解析")]
    public async Task GetReverseGeoAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/v3/geocode/regeo", """
            {"status":"1","info":"OK","regeocode":{"formatted_address":"上海市浦东新区陆家嘴街道银城中路501号","addressComponent":{"province":"上海市","city":"上海市","district":"浦东新区","adcode":"310115","township":"陆家嘴街道","towncode":"310115005000","streetNumber":{"street":["银城中路"],"number":["501号"],"direction":"东","distance":"16.5"}}}}
            """);

        var map = new AMap { Server = stub.Server, AppKey = "test" };
        var ga = await map.GetReverseGeoAsync(new GeoPoint(121.505366, 31.23351), null);

        Assert.NotNull(ga);
        Assert.Equal("上海市浦东新区陆家嘴街道银城中路501号", ga.Address);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
        Assert.Equal("陆家嘴街道", ga.Township);
        Assert.Equal("银城中路", ga.Street);
        Assert.Equal("501号", ga.StreetNumber);
        Assert.Contains("16米", ga.Title);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("高德地图_桩服务_距离解析")]
    public async Task GetDistanceAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/v3/distance", """
            {"status":"1","info":"OK","results":[{"origin_id":"1","dest_id":"1","distance":"851380","duration":"31569"}]}
            """);

        var map = new AMap { Server = stub.Server, AppKey = "test" };
        var drv = await map.GetDistanceAsync(new GeoPoint(121.511999, 31.239184), new GeoPoint(114.218927, 29.575429), "wgs84", 1);

        Assert.NotNull(drv);
        Assert.Equal(851380, drv.Distance);
        Assert.Equal(31569, drv.Duration);
        Assert.Contains("type=1", map.LastUrl);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("高德地图_桩服务_行政区划解析")]
    public async Task GetAreaAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/v3/config/district", """
            {"status":"1","info":"OK","districts":[{"name":"上海市","adcode":"310000","level":"province","districts":[{"name":"浦东新区","adcode":"310115","level":"district","districts":[]}]}]}
            """);

        var map = new AMap { Server = stub.Server, AppKey = "test" };
        var list = await map.GetAreaAsync("上海市");

        Assert.Equal(2, list.Count);
        Assert.Equal(310000, list[0].Code);
        Assert.Equal("上海市", list[0].Name);
        Assert.Equal(310115, list[1].Code);
        Assert.Equal(310000, list[1].ParentCode);
        Assert.Empty(stub.Unmatched);
    }
    #endregion
}