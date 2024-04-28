using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using NewLife;
using NewLife.Data;
using NewLife.Http;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

public class BaiduMapTests
{
    private readonly BaiduMap _map;
    public BaiduMapTests() => _map = new BaiduMap { AppKey = "C73357a276668f8b0563d3f936475007" };

    [Fact]
    public async void Geocoder()
    {
        var addr = "上海中心";
        var map = _map;
        //var rs = await map.GetGeocoderAsync(addr);

        //Assert.NotNull(rs);
        //Assert.True(rs.ContainsKey("location"));

        var ga = await map.GetGeoAsync(addr, null, null, false);

        Assert.NotNull(ga);
        Assert.Equal(121.5119990462553, ga.Location.Longitude);
        Assert.Equal(31.239184684191343, ga.Location.Latitude);
        Assert.Null(ga.Address);
        Assert.True(ga.Confidence > 0);

        ga = await map.GetGeoAsync(addr, null, null, true);

        Assert.NotNull(ga);
        Assert.Equal(121.51199904625521, ga.Location.Longitude);
        Assert.Equal(31.239184551783151, ga.Location.Latitude);
        Assert.Equal("上海市浦东新区花园石桥路176号", ga.Address);
        Assert.StartsWith("上海中心大厦内", ga.Title);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
    }

    [Fact]
    public async void IpLocation()
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

    [Fact]
    public async void GetDistanceAsync()
    {
        var points = new List<GeoPoint> {
            new(121.51199904625513, 31.239184419374944),
            new(114.21892734521, 29.575429778924)
        };

        var map = _map;
        var drv = await map.GetDistanceAsync(points[0], points[1], "wgs84", 0);

        Assert.NotNull(drv);
        Assert.Equal(814272, drv.Distance);
        Assert.Equal(30533, drv.Duration);
    }

    [Fact]
    public async void ConvertAsync()
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
}