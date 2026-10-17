﻿using System;
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

public class BaiduMapTests
{
    private readonly BaiduMap _map;
    public BaiduMapTests() => _map = new BaiduMap { AppKey = "C73357a276668f8b0563d3f936475007" };

    [IntegrationFact]
    [Trait("Category", "Integration")]
    public async Task Geocoder()
    {
        var addr = "上海中心";
        var map = _map;
        //var rs = await map.GetGeocoderAsync(addr);

        //Assert.NotNull(rs);
        //Assert.True(rs.ContainsKey("location"));

        var ga = await map.GetGeoAsync(addr, null, null, false);

        Assert.NotNull(ga);
        Assert.True(Math.Abs(121.511937 - ga.Location.Value.Longitude) < 0.000001);
        Assert.True(Math.Abs(31.239212 - ga.Location.Value.Latitude) < 0.000001);
        Assert.Null(ga.Address);
        Assert.True(ga.Confidence > 0);

        ga = await map.GetGeoAsync(addr, null, null, true);

        Assert.NotNull(ga);
        Assert.True(Math.Abs(121.511937 - ga.Location.Value.Longitude) < 0.000001);
        Assert.True(Math.Abs(31.239212 - ga.Location.Value.Latitude) < 0.000001);
        Assert.Equal("上海市浦东新区花园石桥路176号", ga.Address);
        Assert.StartsWith("上海中心大厦内", ga.Title);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
    }

    [IntegrationFact]
    [Trait("Category", "Integration")]
    public async Task IpLocation()
    {
        var html = new HttpClient().GetString("http://myip.ipip.net");
        var ip = html?.Substring("IP：", " ");
        Assert.NotEmpty(ip);

        var map = _map;
        var rs = await map.IpLocationAsync(ip, null);

        Assert.NotNull(rs);

        var addrs = (rs["full_address"] + "").Split('|');
        Assert.Equal(7, addrs.Length);
    }

    [IntegrationFact]
    [Trait("Category", "Integration")]
    public async Task GetDistanceAsync()
    {
        var points = new List<GeoPoint> {
            new(121.51199904625513, 31.239184419374944),
            new(114.21892734521, 29.575429778924)
        };

        var map = _map;
        var drv = await map.GetDistanceAsync(points[0], points[1], "wgs84", 0);

        Assert.NotNull(drv);
        Assert.True(Math.Abs(814563 - drv.Distance) < 100);
        Assert.True(Math.Abs(30544 - drv.Duration) < 600);
    }

    [IntegrationFact]
    [Trait("Category", "Integration")]
    public async Task ConvertAsync()
    {
        var points = new List<GeoPoint> {
            new(121.51199904625513, 31.239184419374944),
            new(114.21892734521, 29.575429778924)
        };

        var map = _map;
        var points2 = await map.ConvertAsync(points, "wgs84", "gcj02");

        Assert.NotNull(points2);

        Assert.Equal(points.Count, points2.Count);
        Assert.True(points2[0].Longitude > 0);
        Assert.True(points2[0].Latitude > 0);
    }

    #region 桩测试。本地 HTTP 桩服务替代真实厂商，不依赖外部网络与真实密钥
    [Fact]
    [DisplayName("百度地图_桩服务_正向地理编码解析")]
    public async Task Geocoder_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/geocoding/v3/", """
            {"status":0,"result":{"location":{"lng":121.511937,"lat":31.239212},"precise":1,"confidence":75,"comprehension":100,"level":"门址"}}
            """);

        var map = new BaiduMap { Server = stub.Server, AppKey = "test" };
        var ga = await map.GetGeoAsync("上海中心", null, null, false);

        Assert.NotNull(ga);
        Assert.NotNull(ga.Location);
        Assert.Equal(121.511937, ga.Location.Value.Longitude);
        Assert.Equal(31.239212, ga.Location.Value.Latitude);
        Assert.True(ga.Precise);
        Assert.Equal(75, ga.Confidence);
        Assert.Equal(100, ga.Comprehension);
        Assert.Equal("门址", ga.Level);
        Assert.Contains("address=", map.LastUrl);
        Assert.Contains("ak=test", map.LastUrl);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("百度地图_桩服务_逆地理编码解析")]
    public async Task GetReverseGeoAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/reverse_geocoding/v3/", """
            {"status":0,"result":{"formatted_address":"上海市浦东新区花园石桥路176号","confidence":80,"location":{"lng":121.511999,"lat":31.239185},"addressComponent":{"country":"中国","province":"上海市","city":"上海市","district":"浦东新区","adcode":310115,"town":"陆家嘴街道","town_code":310115005,"street":"花园石桥路","street_number":"176号"},"sematic_description":"上海中心大厦内"}}
            """);

        var map = new BaiduMap { Server = stub.Server, AppKey = "test" };
        var ga = await map.GetReverseGeoAsync(new GeoPoint(121.511999, 31.239184), null);

        Assert.NotNull(ga);
        Assert.Equal("上海市浦东新区花园石桥路176号", ga.Address);
        Assert.Equal("上海中心大厦内", ga.Title);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
        Assert.Equal("陆家嘴街道", ga.Township);
        Assert.Equal("176号", ga.StreetNumber);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("百度地图_桩服务_驾车距离解析")]
    public async Task GetDistanceAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/routematrix/v2/driving", """
            {"status":0,"result":[{"distance":{"value":814563,"text":"814.6公里"},"duration":{"value":30544,"text":"8.5小时"}}]}
            """);

        var map = new BaiduMap { Server = stub.Server, AppKey = "test" };
        var drv = await map.GetDistanceAsync(new GeoPoint(121.511999, 31.239184), new GeoPoint(114.218927, 29.575429), "wgs84");

        Assert.NotNull(drv);
        Assert.Equal(814563, drv.Distance);
        Assert.Equal(30544, drv.Duration);
        Assert.Contains("coord_type=wgs84", map.LastUrl);
        Assert.Contains("tactics=13", map.LastUrl);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("百度地图_桩服务_IP定位解析")]
    public async Task IpLocationAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/location/ip", """
            {"status":0,"address":"CN|北京市|北京市||100000|CN|110000","content":{"address":"北京市","address_detail":{"province":"北京市","city":"北京市","district":"","street":"","street_number":"","city_code":131},"point":{"x":"116.412","y":"39.916"}}}
            """);

        var map = new BaiduMap { Server = stub.Server, AppKey = "test" };
        var rs = await map.IpLocationAsync("114.247.50.2", null);

        Assert.NotNull(rs);
        Assert.Equal(7, (rs["full_address"] + "").Split('|').Length);
        Assert.Equal("北京市", rs["province"] + "");
        Assert.Equal("116.412", rs["x"] + "");
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("百度地图_桩服务_在线坐标转换解析")]
    public async Task ConvertAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/geoconv/v1/", """
            {"status":0,"result":[{"x":121.516204,"y":31.241021}]}
            """);

        var map = new BaiduMap { Server = stub.Server, AppKey = "test" };
        var points = await map.ConvertAsync([new GeoPoint(121.511999, 31.239184)], "wgs84", "gcj02");

        Assert.NotNull(points);
        var pt = Assert.Single(points);
        Assert.Equal(121.516204, pt.Longitude);
        Assert.Equal(31.241021, pt.Latitude);
        Assert.Contains("from=1&to=3", map.LastUrl);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("百度地图_桩服务_POI检索解析")]
    public async Task PlaceSearch_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/place/v2/search", """
            {"status":0,"results":[{"name":"上海中心大厦","location":{"lng":121.505366,"lat":31.23351},"address":"上海市浦东新区银城中路501号"}]}
            """);

        var map = new BaiduMap { Server = stub.Server, AppKey = "test" };
        var ga = await map.PlaceSearchAsync("上海中心", null, "上海", null, false);

        Assert.NotNull(ga);
        Assert.Equal("上海中心大厦", ga.Name);
        Assert.Equal("上海市浦东新区银城中路501号", ga.Address);
        Assert.NotNull(ga.Location);
        Assert.Equal(121.505366, ga.Location.Value.Longitude);
        Assert.Equal(31.23351, ga.Location.Value.Latitude);

        var raw = Uri.UnescapeDataString(map.LastUrl);
        Assert.Contains("query=上海中心", raw);
        Assert.Contains("region=上海", raw);
        Assert.Contains("ak=test", raw);
        Assert.Empty(stub.Unmatched);
    }
    #endregion
}