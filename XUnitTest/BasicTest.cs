using System.Collections.Generic;
using NewLife.Data;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

public class BasicTest
{
    private readonly NewLifeMap _map = new();

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