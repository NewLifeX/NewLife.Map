using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>Map 基类限速与统计测试（无网络依赖）</summary>
public class MapRateLimitTests
{
    /// <summary>测试用 Map 子类，暴露限速等待方法</summary>
    class TestMap : Map
    {
        public Task WaitToken() => WaitTokenAsync();
    }

    [Fact]
    public async void MaxQps_Default_NoWait()
    {
        // 默认 0 不限速，立即通过
        var map = new TestMap { MaxQps = 0 };

        var sw = Stopwatch.StartNew();
        await map.WaitToken();
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 100);
    }

    [Fact]
    public async void MaxQps_High_NoWait()
    {
        // 高 QPS 间隔极小，几乎不等待
        var map = new TestMap { MaxQps = 1000 };

        var sw = Stopwatch.StartNew();
        await map.WaitToken();
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 100);
    }

    [Fact]
    public async void MaxQps_1_Throttled()
    {
        // 每秒 1 次：首次通过，第二次需等待约 1 秒
        var map = new TestMap { MaxQps = 1 };

        await map.WaitToken();

        var sw = Stopwatch.StartNew();
        await map.WaitToken();
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds >= 500);
    }

    [Fact]
    public void Metrics_Initial()
    {
        var map = new TestMap();

        Assert.Equal(0, map.Total);
        Assert.Equal(0, map.Success);
        Assert.Equal(0, map.Fail);
        Assert.Equal(0.0, map.SuccessRate);
        Assert.Equal(0.0, map.AvgTime);
        Assert.Equal(0, map.GetKeyCount("k1"));
    }
}
