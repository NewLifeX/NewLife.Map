using NewLife.Data;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>MapHelper 静态方法测试</summary>
public class MapHelperTests
{
    [Fact]
    public void ParseTest()
    {
        var p = MapHelper.Parse("109.995837,40.690028");
        Assert.Equal(109.995837, p.Longitude);
        Assert.Equal(40.690028, p.Latitude);
    }

    [Fact]
    public void ParseEmptyTest()
    {
        var p = MapHelper.Parse(null);
        Assert.Equal(0, p.Longitude);
        Assert.Equal(0, p.Latitude);
    }

    [Fact]
    public void ParseInvalidTest()
    {
        var p = MapHelper.Parse("abc");
        Assert.Equal(0, p.Longitude);
        Assert.Equal(0, p.Latitude);
    }

    [Fact]
    public void CreateTest()
    {
        var p = MapHelper.Create("121.505366", "31.23351");
        Assert.Equal(121.505366, p.Longitude);
        Assert.Equal(31.23351, p.Latitude);
    }
}
