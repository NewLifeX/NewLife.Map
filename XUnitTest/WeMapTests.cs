using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using NewLife;
using NewLife.Data;
using NewLife.Http;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

public class WeMapTests
{
    private readonly WeMap _map;
    public WeMapTests() => _map = new WeMap { AppKey = "YGEBZ-BDCCX-AJG4X-ZUH6W-MESMV-P2BFF" };

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
        Assert.Equal(121.505406, ga.Location.Longitude);
        Assert.Equal(31.233501, ga.Location.Latitude);
        //Assert.Null(ga.Address);
        Assert.True(ga.Confidence > 0);

        ga = await map.GetGeoAsync(addr, null, null, true);

        Assert.NotNull(ga);
        Assert.Equal(121.505406, ga.Location.Longitude);
        Assert.Equal(31.233501, ga.Location.Latitude);
        Assert.Equal("上海市浦东新区陆家嘴银城中路501号", ga.Address);
        Assert.Equal("上海市浦东新区花园石桥路", ga.Title);
        Assert.Equal(310115, ga.Code);
        //Assert.Equal(310115005, ga.Towncode);
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
        Assert.Equal(851907, drv.Distance);
        Assert.True(Math.Abs(37020 - drv.Duration) < 600);
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
}