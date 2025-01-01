using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using NewLife;
using NewLife.Data;
using NewLife.Http;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

public class TianDiMapTests
{
    private readonly TianDiTu _map;
    public TianDiMapTests() => _map = new TianDiTu { AppKey = "3334f7776916effb40f2a11dbae57781" };

    [Fact]
    public async void Geocoder()
    {
        var addr = "浦东上海中心";
        var map = _map;
        //var rs = await map.GetGeocoderAsync(addr);

        //Assert.NotNull(rs);
        //Assert.True(rs.ContainsKey("location"));

        var ga = await map.GetGeoAsync(addr, null, null, false);

        Assert.NotNull(ga);
        Assert.Equal(121.50053, ga.Location.Longitude);
        Assert.Equal(31.235746, ga.Location.Latitude);
        Assert.Null(ga.Address);
        Assert.True(ga.Confidence > 0);

        ga = await map.GetGeoAsync(addr, null, null, true);

        Assert.NotNull(ga);
        Assert.Equal(121.50053, ga.Location.Longitude);
        Assert.Equal(31.235746, ga.Location.Latitude);
        Assert.StartsWith("银城中路501号", ga.Address);
        Assert.StartsWith("银城中路501号", ga.Title);
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
        var drv = await map.GetDistanceAsync(points[0], points[^1]);

        Assert.NotNull(drv);
        Assert.Equal(853030, drv.Distance);
        Assert.Equal(33118, drv.Duration);
    }

    [Fact]
    public async void GetDistanceAsync2()
    {
        var points = new List<GeoPoint> {
            new(121.51199904625513, 31.239184419374944),
            new(118.21, 30.57),
            new(116.21, 29.97),
            new(114.21892734521, 29.575429778924)
        };

        var map = _map;
        var drv = await map.GetDistanceAsync(points[0], points[^1], points[1..^1]);

        Assert.NotNull(drv);
        Assert.Equal(958790, drv.Distance);
        Assert.Equal(49091, drv.Duration);
    }
}