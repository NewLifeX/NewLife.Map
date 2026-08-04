using NewLife.Map;
using NewLife.Map.Models;
using Xunit;

namespace XUnitTest;

/// <summary>地图工厂与提供者注册测试</summary>
public class MapFactoryTests
{
    [Fact]
    public void Providers_ContainsAll()
    {
        // 内置 6 家提供者：百度/高德/腾讯/天地图/Nominatim/新生命
        Assert.Equal(6, MapFactory.Providers.Count);
    }

    [Fact]
    public void Create_ByKind()
    {
        var map = MapFactory.Create(MapKinds.Baidu);

        Assert.NotNull(map);
        Assert.IsType<BaiduMap>(map);
    }

    [Fact]
    public void Create_ByName()
    {
        var map = Map.Create("Baidu");

        Assert.NotNull(map);
        Assert.IsType<BaiduMap>(map);

        var map2 = Map.Create("AMap");

        Assert.NotNull(map2);
        Assert.IsType<AMap>(map2);
    }

    [Fact]
    public void Create_ByName_NotFound()
    {
        var map = Map.Create("不存在的提供者");

        Assert.Null(map);
    }

    [Fact]
    public void Create_NewLifeMap()
    {
        var map = MapFactory.Create(MapKinds.NewLife);

        Assert.NotNull(map);
        Assert.IsType<NewLifeMap>(map);
    }
}
