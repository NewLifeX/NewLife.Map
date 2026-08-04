using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>Map 抽象基类基础能力测试</summary>
public class MapBaseTests
{
    /// <summary>测试用 Map 子类，使用默认密钥行为</summary>
    class TestMap : Map
    {
    }

    [Fact]
    public void NeedKey_DefaultTrue()
    {
        var map = new TestMap();

        // 默认提供者需要密钥
        Assert.True(map.NeedKey);
    }

    [Fact]
    public void NeedKey_Nominatim_False()
    {
        var map = new NominatimMap();

        // 免费开源数据源无需密钥
        Assert.False(map.NeedKey);
    }

    [Fact]
    public void Available_NoKeyName_True()
    {
        // 无需密钥的免费提供者（KeyName 为空）始终可用
        var map = new NominatimMap();

        Assert.True(map.Available);
    }

    [Fact]
    public void Available_NeedKey_NoKey_False()
    {
        // 需要密钥但未配置 AppKey → 不可用
        var map = new TestMap();

        Assert.False(map.Available);
    }

    [Fact]
    public void Available_NeedKey_HasKey_True()
    {
        // 配置 AppKey 后可轮询 → 可用
        var map = new TestMap { AppKey = "k1,k2" };

        Assert.True(map.Available);
    }
}
