using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>Map 基类请求链路测试：统计、密钥缺失、多 Key 计数与密钥摘除</summary>
public class MapMetricsTests
{
    class TestMap : Map
    {
        public Task<String> GetAsync(String url) => GetStringAsync(url);

        public void Remove(String key, DateTime reviveTime) => RemoveKey(key, reviveTime);
    }

    [Fact]
    [DisplayName("请求统计_成功请求累加成功次数与密钥计数")]
    public async Task Success_UpdatesMetrics()
    {
        using var stub = new StubHttpServer();
        stub.Map("/ping", """{"ok":true}""");
        using var map = new TestMap { Server = stub.Server, AppKey = "k1" };

        var rs = await map.GetAsync("/ping");

        Assert.Equal("""{"ok":true}""", rs);
        Assert.Equal(1, map.Total);
        Assert.Equal(1, map.Success);
        Assert.Equal(0, map.Fail);
        Assert.Equal(1.0, map.SuccessRate);
        Assert.True(map.AvgTime >= 0);
        Assert.Equal(1, map.GetKeyCount("k1"));
        Assert.Equal("k1", map.LastKey);
        Assert.Contains("key=k1", map.LastUrl);
        Assert.Empty(stub.Unmatched);
    }

    [Fact]
    [DisplayName("请求统计_失败请求累加失败次数")]
    public async Task Failure_UpdatesMetrics()
    {
        using var stub = new StubHttpServer();
        using var map = new TestMap { Server = stub.Server, AppKey = "k1" };

        await Assert.ThrowsAsync<HttpRequestException>(() => map.GetAsync("/missing"));

        Assert.Equal(1, map.Total);
        Assert.Equal(0, map.Success);
        Assert.Equal(1, map.Fail);
        Assert.Equal(0.0, map.SuccessRate);
        Assert.Single(stub.Unmatched);
    }

    [Fact]
    [DisplayName("请求统计_缺少密钥时抛出参数异常且不发起请求")]
    public async Task MissingKey_Throws()
    {
        using var map = new TestMap { Server = "http://127.0.0.1:1" };

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => map.GetAsync("/ping"));

        Assert.Equal("AppKey", ex.ParamName);
        Assert.Equal(0, map.Total);
    }

    [Fact]
    [DisplayName("多Key_两次请求轮询到不同密钥")]
    public async Task MultiKey_RoundRobinCounts()
    {
        using var stub = new StubHttpServer();
        stub.Map("/ping", "{}");
        using var map = new TestMap { Server = stub.Server, AppKey = "k1,k2" };

        await map.GetAsync("/ping");
        await map.GetAsync("/ping");

        Assert.Equal(1, map.GetKeyCount("k1"));
        Assert.Equal(1, map.GetKeyCount("k2"));
        Assert.Equal(2, map.Total);
    }

    [Fact]
    [DisplayName("多Key_全部摘除后不可用")]
    public void RemoveKey_AllKeys_Unavailable()
    {
        using var map = new TestMap { AppKey = "k1,k2" };

        // Available 首次访问会初始化密钥数组
        Assert.True(map.Available);

        map.Remove("k1", DateTime.Now.AddMinutes(1));
        Assert.True(map.Available);

        map.Remove("k2", DateTime.Now.AddMinutes(1));
        Assert.False(map.Available);
        Assert.Equal(0, map.Total);
    }

    [Fact]
    [DisplayName("提供者错误响应_默认返回空且可切换为抛异常")]
    public async Task ProviderError_DefaultNull_ThrowWhenEnabled()
    {
        using var stub = new StubHttpServer();
        stub.Map("/v3/geocode/geo", """
            {"status":"0","info":"INVALID_USER_KEY","infocode":"10001"}
            """);

        var map = new AMap { Server = stub.Server, AppKey = "k1" };
        Assert.Null(await map.GetGeoAsync("上海中心"));

        var map2 = new AMap { Server = stub.Server, AppKey = "k1", ThrowException = true };
        var ex = await Assert.ThrowsAsync<Exception>(() => map2.GetGeoAsync("上海中心"));
        Assert.Contains("INVALID_USER_KEY", ex.Message);
    }
}
