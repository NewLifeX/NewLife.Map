using NewLife.Data;
using NewLife.Map;
using NewLife.Model;
using Stardust;
using Xunit;

namespace XUnitTest;

public class NewLifeMapTests
{
    private readonly NewLifeMap _map;
    public NewLifeMapTests()
    {
        //_map = new NewLifeMap { Server = "https://localhost:7279" };

        var services = ObjectContainer.Current;
        services.AddStardust();

        var provider = services.BuildServiceProvider();
        //_map = provider.GetService<IMap>() as NewLifeMap;
        //_map = provider.CreateInstance(typeof(NewLifeMap)) as NewLifeMap;
        _map = new NewLifeMap(provider);
    }

    [Fact]
    public async void Geocoder()
    {
        var addr = "上海中心";
        var map = _map;

        var point = new GeoPoint(121.5119990462553, 31.239184684191343);

        var gmodel = await map.GetGeoInfo(point, null);

        Assert.NotNull(gmodel);
        Assert.Equal(121.511999, gmodel.Longitude);
        Assert.Equal(31.239185, gmodel.Latitude);
        Assert.Equal("上海市浦东新区花园石桥路176号", gmodel.Address);
        Assert.StartsWith("上海中心大厦内", gmodel.Title);

        var ga = await map.GetReverseGeoAsync(point, null);

        Assert.NotNull(ga);
        Assert.Equal(121.511999, ga.Location.Longitude);
        Assert.Equal(31.239185, ga.Location.Latitude);
        Assert.Equal("上海市浦东新区花园石桥路176号", ga.Address);
        Assert.StartsWith("上海中心大厦内", gmodel.Title);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
    }
}