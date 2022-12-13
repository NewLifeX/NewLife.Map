using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using NewLife;
using NewLife.Data;
using NewLife.Http;
using NewLife.Yun;
using Xunit;

namespace XUnitTest;

public class NewLifeMapTests
{
    private readonly NewLifeMap _map;
    public NewLifeMapTests() => _map = new NewLifeMap { Server = "https://localhost:7279" };

    [Fact]
    public async void Geocoder()
    {
        var addr = "上海中心";
        var map = _map;

        var point = new GeoPoint(121.5119990462553, 31.239184684191343);

        var ga = await map.GetGeoModel(point);

        Assert.NotNull(ga);
        Assert.Equal(121.5119990462553, ga.Longitude);
        Assert.Equal(31.239184684191343, ga.Latitude);
        Assert.Null(ga.Address);

        Assert.NotNull(ga);
        Assert.Equal(121.51199904625521, ga.Longitude);
        Assert.Equal(31.239184551783151, ga.Latitude);
        Assert.Equal("上海市浦东新区花园石桥路176号上海中心大厦内", ga.Address);
        //Assert.Equal(310115, ga.Code);
        //Assert.Equal(310115005, ga.Towncode);
    }
}