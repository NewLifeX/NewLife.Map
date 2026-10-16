using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NewLife;
using NewLife.Data;
using NewLife.Http;
using NewLife.Map;
using NewLife.Map.Models;
using Xunit;

namespace XUnitTest;

public class WeMapTests
{
    private readonly WeMap _map;
    public WeMapTests() => _map = new WeMap { AppKey = "YGEBZ-BDCCX-AJG4X-ZUH6W-MESMV-P2BFF" };

    [Fact]
    [Trait("Category", "Integration")]
    public async void Geocoder()
    {
        var addr = "上海中心";
        var map = _map;
        //var rs = await map.GetGeocoderAsync(addr);

        //Assert.NotNull(rs);
        //Assert.True(rs.ContainsKey("location"));

        var ga = await map.GetGeoAsync(addr, null, null, false);

        Assert.NotNull(ga);
        Assert.Equal(121.505406, ga.Location.Value.Longitude);
        Assert.Equal(31.233501, ga.Location.Value.Latitude);
        //Assert.Null(ga.Address);
        Assert.True(ga.Confidence > 0);

        ga = await map.GetGeoAsync(addr, null, null, true);

        Assert.NotNull(ga);
        Assert.Equal(121.505406, ga.Location.Value.Longitude);
        Assert.Equal(31.233501, ga.Location.Value.Latitude);
        Assert.StartsWith("上海市浦东新区陆家嘴", ga.Address);
        Assert.Equal("上海市浦东新区花园石桥路", ga.Title);
        Assert.Equal(310115, ga.Code);
        //Assert.Equal(310115005, ga.Towncode);
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
        Assert.True(drv.Distance >= 851907);
        Assert.True(Math.Abs(32020 - drv.Duration) < 600);
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

    [Fact]
    [Trait("Category", "Integration")]
    public async void GetArea()
    {
        // 腾讯 district API 行政区划。依赖真实 Key 与网络
        var list = await _map.GetAreaAsync(0);

        Assert.NotNull(list);
        Assert.True(list.Count > 0);
        Assert.True(list[0].Code > 0);
        Assert.False(String.IsNullOrEmpty(list[0].Name));
    }

    #region 桩测试。本地 HTTP 桩服务替代真实厂商，不依赖外部网络与真实密钥
    [Fact]
    [DisplayName("腾讯地图_桩服务_正向地理编码解析")]
    public async Task Geocoder_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/ws/geocoder/v1/", """
            {"status":0,"message":"query ok","result":{"title":"上海中心","location":{"lng":121.505406,"lat":31.233501},"ad_info":{"adcode":310115},"address_components":{"province":"上海市","city":"上海市","district":"浦东新区","street":"陆家嘴街道","street_number":"花园石桥路"},"reliability":7,"level":1}}
            """, "address=");

        var map = new WeMap { Server = stub.Server, AppKey = "test" };
        var ga = await map.GetGeoAsync("上海中心");

        Assert.NotNull(ga);
        Assert.NotNull(ga.Location);
        Assert.Equal(121.505406, ga.Location.Value.Longitude);
        Assert.Equal(31.233501, ga.Location.Value.Latitude);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(70, ga.Confidence);
        Assert.Equal("陆家嘴街道", ga.Street);
        Assert.Contains("浦东新区", ga.Address);
        Assert.Contains("key=test", map.LastUrl);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("腾讯地图_桩服务_逆地理编码解析")]
    public async Task GetReverseGeoAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/ws/geocoder/v1/", """
            {"status":0,"message":"query ok","result":{"address":"上海市浦东新区陆家嘴街道上海中心大厦","formatted_addresses":{"recommend":"上海中心大厦","standard_address":"上海市浦东新区陆家嘴街道银城中路501号"},"address_component":{"nation":"中国","province":"上海市","city":"上海市","district":"浦东新区","street":"陆家嘴街道","street_number":"银城中路501号"},"ad_info":{"adcode":310115}}}
            """, "location=");

        var map = new WeMap { Server = stub.Server, AppKey = "test" };
        var ga = await map.GetReverseGeoAsync(new GeoPoint(121.505406, 31.233501), null);

        Assert.NotNull(ga);
        Assert.Equal("上海市浦东新区陆家嘴街道银城中路501号", ga.Address);
        Assert.Equal("上海市浦东新区陆家嘴街道上海中心大厦", ga.Title);
        Assert.Equal("上海中心大厦", ga.Name);
        Assert.Equal("中国", ga.Country);
        Assert.Equal(310115, ga.Code);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("腾讯地图_桩服务_驾车距离解析")]
    public async Task GetDistanceAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/ws/direction/v1/driving", """
            {"status":0,"result":{"routes":[{"distance":851907,"duration":533}]}}
            """);

        var map = new WeMap { Server = stub.Server, AppKey = "test" };
        var drv = await map.GetDistanceAsync(new GeoPoint(121.511999, 31.239184), new GeoPoint(114.218927, 29.575429), "wgs84");

        Assert.NotNull(drv);
        Assert.Equal(851907, drv.Distance);
        Assert.Equal(31980, drv.Duration);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("腾讯地图_桩服务_行政区划解析")]
    public async Task GetAreaAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/ws/district/v1/list", """
            {"status":0,"data":{"data":[[{"id":"110000","name":"北京市","location":{"lat":39.90469,"lng":116.40717}}],[{"id":"310000","name":"上海市","location":{"lat":31.23037,"lng":121.4737}}],[]]}}
            """);

        var map = new WeMap { Server = stub.Server, AppKey = "test" };
        var list = await map.GetAreaAsync(0);

        Assert.Equal(2, list.Count);
        Assert.Equal(110000, list[0].Code);
        Assert.Equal("北京市", list[0].Name);
        Assert.Equal(310000, list[1].Code);
        Assert.Contains("116.40717", list[0].Center);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("腾讯地图_桩服务_IP定位解析")]
    public async Task IpLocationAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/ws/location/v1/ip", """
            {"status":0,"result":{"ip":"114.247.50.2","location":{"lat":39.9847,"lng":116.30756},"ad_info":{"nation":"中国","province":"北京市","city":"北京市","district":"","adcode":110000}}}
            """);

        var map = new WeMap { Server = stub.Server, AppKey = "test" };
        var rs = await map.IpLocationAsync("114.247.50.2");

        Assert.NotNull(rs);
        Assert.Equal("114.247.50.2", rs["ip"] + "");
        Assert.Equal("北京市", rs["province"] + "");
        Assert.True((rs["lng"] + "").ToDouble() > 116);
        Assert.Empty(stub.Unmatched);
    }
    #endregion
}