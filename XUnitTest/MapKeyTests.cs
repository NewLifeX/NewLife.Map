using System;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>多Key轮询与熔断测试</summary>
public class MapKeyTests
{
    /// <summary>测试用 Map 子类，暴露受保护方法</summary>
    class TestMap : Map
    {
        public String GetKey() => AcquireKey();

        public void Remove(String key, DateTime reviveTime) => RemoveKey(key, reviveTime);

        public Boolean IsInvalid(String result) => IsValidKey(result);
    }

    [Fact]
    public void AcquireKey_RoundRobin()
    {
        var map = new TestMap { AppKey = "k1,k2,k3" };

        var keys = new[] { map.GetKey(), map.GetKey(), map.GetKey(), map.GetKey() };

        // 递增索引取余轮询：Interlocked.Increment 先加后取，首个返回第二个 key
        Assert.Equal("k2", keys[0]);
        Assert.Equal("k3", keys[1]);
        Assert.Equal("k1", keys[2]);
        Assert.Equal("k2", keys[3]);
    }

    [Fact]
    public void RemoveKey_NotReturned()
    {
        var map = new TestMap { AppKey = "k1,k2" };

        // 先触发密钥数组初始化（RemoveKey 在 _Keys 未初始化时是 no-op）
        map.GetKey();

        map.Remove("k1", DateTime.Now.AddMinutes(1));

        // 移除后轮询不再返回 k1
        Assert.Equal("k2", map.GetKey());
        Assert.Equal("k2", map.GetKey());
    }

    [Fact]
    public void IsValidKey_DetectInvalid()
    {
        var map = new TestMap { AppKey = "k1" };

        // 命中失效关键字（避免包含 KeyName 的文本，如 "INVALID_KEY"）
        Assert.True(map.IsInvalid("INVALID"));
        Assert.True(map.IsInvalid("LIMIT_EXCEEDED"));
        Assert.False(map.IsInvalid("正常结果"));
    }

    [Fact]
    public void IsValidKey_KeyName_NotInvalid()
    {
        var map = new TestMap { AppKey = "k1" };

        // 包含 KeyName 本身不算失效
        Assert.False(map.IsInvalid("key=xxx"));
    }
}
