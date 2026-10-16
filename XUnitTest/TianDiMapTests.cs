using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

public class TianDiMapTests
{
    private readonly TianDiTu _map;
    public TianDiMapTests() => _map = new TianDiTu { AppKey = "3334f7776916effb40f2a11dbae57781" };

    [Fact]
    [Trait("Category", "Integration")]
    public async void Geocoder()
    {
        var addr = "浦东上海中心";
        var map = _map;
        //var rs = await map.GetGeocoderAsync(addr);

        //Assert.NotNull(rs);
        //Assert.True(rs.ContainsKey("location"));

        var ga = await map.GetGeoAsync(addr, null, null, false);

        Assert.NotNull(ga);
        Assert.Equal(121.50053, ga.Location.Value.Longitude);
        Assert.Equal(31.235746, ga.Location.Value.Latitude);
        Assert.Null(ga.Address);
        Assert.True(ga.Confidence > 0);

        ga = await map.GetGeoAsync(addr, null, null, true);

        Assert.NotNull(ga);
        Assert.Equal(121.50053, ga.Location.Value.Longitude);
        Assert.Equal(31.235746, ga.Location.Value.Latitude);
        Assert.StartsWith("银城中路501号", ga.Address);
        Assert.StartsWith("银城中路501号", ga.Title);
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
        var drv = await map.GetDistanceAsync(points[0], points[^1]);

        Assert.NotNull(drv);
        Assert.Equal(853030, drv.Distance);
        Assert.Equal(33118, drv.Duration);
    }

    [Fact]
    [Trait("Category", "Integration")]
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

    #region 桩测试。本地 HTTP 桩服务替代真实厂商，不依赖外部网络与真实密钥
    [Fact]
    [DisplayName("天地图_桩服务_正向地理编码解析")]
    public async Task Geocoder_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/geocoder", """
            {"status":0,"msg":"ok","location":{"keyWord":"浦东上海中心","lon":121.50053,"lat":31.235746,"level":"地名地址","score":100}}
            """, "ds=");

        var map = new TianDiTu { Server = stub.Server, AppKey = "test" };
        var ga = await map.GetGeoAsync("浦东上海中心", null, null, false);

        Assert.NotNull(ga);
        Assert.NotNull(ga.Location);
        Assert.Equal(121.50053, ga.Location.Value.Longitude);
        Assert.Equal(31.235746, ga.Location.Value.Latitude);
        Assert.Equal("地名地址", ga.Level);
        Assert.Equal(100, ga.Confidence);
        Assert.Contains("tk=test", map.LastUrl);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("天地图_桩服务_逆地理编码解析")]
    public async Task GetReverseGeoAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/geocoder", """
            {"status":0,"msg":"ok","result":{"formatted_address":"上海市浦东新区银城中路501号","confidence":99,"location":{"lon":121.505,"lat":31.233},"addressComponent":{"nation":"中国","province":"上海市","city":"上海市","county":"浦东新区","county_code":"156310115","poi":"上海中心大厦","address":"银城中路501号","road":"银城中路","street_number":"501号"}}}
            """, "postStr=");

        var map = new TianDiTu { Server = stub.Server, AppKey = "test" };
        var ga = await map.GetReverseGeoAsync(new GeoPoint(121.505, 31.233), null);

        Assert.NotNull(ga);
        // 天地图响应地址组件中的 address 字段会覆盖 formatted_address，与真实服务行为一致
        Assert.Equal("银城中路501号", ga.Address);
        Assert.Equal(310115, ga.Code);
        Assert.Equal("浦东新区", ga.District);
        Assert.Equal("上海中心大厦", ga.Name);
        Assert.Equal("银城中路501号", ga.Title);
        Assert.Equal("银城中路", ga.Street);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("天地图_桩服务_驾车距离解析")]
    public async Task GetDistanceAsync_Stub()
    {
        using var stub = new StubHttpServer();
        stub.Map("/drive", """
            <response><distance>853.03</distance><duration>33118</duration></response>
            """);

        var map = new TianDiTu { Server = stub.Server, AppKey = "test" };
        var drv = await map.GetDistanceAsync(new GeoPoint(121.511999, 31.239184), new GeoPoint(114.218927, 29.575429));

        Assert.NotNull(drv);
        Assert.Equal(853030, drv.Distance);
        Assert.Equal(33118, drv.Duration);
        Assert.Empty(stub.Unmatched);
    }
    #endregion
}