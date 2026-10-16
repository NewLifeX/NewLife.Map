using NewLife.Data;
using NewLife.Map;
using NewLife.Map.Models;
using NewLife.Model;
using Stardust;
using System.ComponentModel;
using System.Threading.Tasks;
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
    [Trait("Category", "Integration")]
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
        Assert.Equal(121.511999, ga.Location.Value.Longitude);
        Assert.Equal(31.239185, ga.Location.Value.Latitude);
        Assert.Equal("上海市浦东新区花园石桥路176号", ga.Address);
        Assert.StartsWith("上海中心大厦内", gmodel.Title);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async void Geo()
    {
        var map = _map;

        var ga = await map.GetGeoAsync("上海中心");

        Assert.NotNull(ga);
        Assert.NotNull(ga.Location);
        Assert.True(ga.Location.Value.Longitude > 100);
        Assert.True(ga.Location.Value.Latitude > 20);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async void Distance()
    {
        var map = _map;

        var d = await map.GetDistanceAsync(new GeoPoint(121.505366, 31.23351), new GeoPoint(121.511999, 31.239184), null);

        Assert.NotNull(d);
        Assert.True(d.Distance > 0);
    }

    #region 桩测试。本地 HTTP 桩服务替代平台服务，不依赖星尘注册中心与 MapApi 在线
    [Fact]
    [DisplayName("新生命地图_桩服务_正向地理编码解析")]
    public async Task Geo_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/Map/GeoAddress", """
            {"code":0,"data":{"longitude":121.505366,"latitude":31.23351,"address":"上海市浦东新区上海中心大厦","title":"上海中心大厦","districtId":310115,"province":"上海市","city":"上海市","district":"浦东新区"}}
            """);

        var map = new NewLifeMap(stub.Server);
        var ga = await map.GetGeoAsync("上海中心");

        Assert.NotNull(ga);
        Assert.NotNull(ga.Location);
        Assert.Equal(121.505366, ga.Location.Value.Longitude);
        Assert.Equal(31.23351, ga.Location.Value.Latitude);
        Assert.Equal("上海市浦东新区上海中心大厦", ga.Address);
        Assert.Equal("上海中心大厦", ga.Title);
        Assert.Equal(310115, ga.Code);
        Assert.Contains("address=", stub.LastRequest);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("新生命地图_桩服务_逆地理编码解析")]
    public async Task ReverseGeo_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/Map/ReverseGeo", """
            {"code":0,"data":{"longitude":121.505366,"latitude":31.23351,"address":"上海市浦东新区陆家嘴街道银城中路501号","title":"上海中心大厦","areaCode":310115005,"areaName":"陆家嘴街道","districtId":310115}}
            """);

        var map = new NewLifeMap(stub.Server);
        var ga = await map.GetReverseGeoAsync(new GeoPoint(121.505366, 31.23351), null);

        Assert.NotNull(ga);
        Assert.Equal("上海市浦东新区陆家嘴街道银城中路501号", ga.Address);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
        Assert.Equal("陆家嘴街道", ga.Township);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("新生命地图_桩服务_驾驶距离解析")]
    public async Task Distance_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/Map/Distance", """
            {"code":0,"data":{"distance":814563,"duration":30544}}
            """);

        var map = new NewLifeMap(stub.Server);
        var drv = await map.GetDistanceAsync(new GeoPoint(121.511999, 31.239184), new GeoPoint(114.218927, 29.575429), null);

        Assert.NotNull(drv);
        Assert.Equal(814563, drv.Distance);
        Assert.Equal(30544, drv.Duration);
        Assert.Empty(stub.Unmatched);
    }
    #endregion
}