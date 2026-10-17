using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using NewLife.Data;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>坐标转换测试。覆盖全部支持组合、大小写、同一坐标系、境外坐标与异常参数</summary>
public class MapConvertTests
{
    class TestMap : Map
    {
    }

    private readonly TestMap _map = new();

    /// <summary>上海中心 WGS84 坐标</summary>
    private static readonly GeoPoint _shanghai = new(121.51199904625513, 31.239184419374944);

    private static void AssertClose(Double expected, Double actual, Double tolerance = 1e-9) =>
        Assert.InRange(actual, expected - tolerance, expected + tolerance);

    [Fact]
    [DisplayName("坐标转换_WGS84转GCJ02_已知值")]
    public async Task Wgs84ToGcj02_KnownValue()
    {
        var rs = await _map.ConvertAsync([_shanghai], "wgs84", "gcj02");

        var p = Assert.Single(rs);
        AssertClose(121.51638785294696, p.Longitude);
        AssertClose(31.237133740198594, p.Latitude);
    }

    [Fact]
    [DisplayName("坐标转换_境外坐标_WGS84转GCJ02不变")]
    public async Task Wgs84ToGcj02_OutsideChina_Unchanged()
    {
        var paris = new GeoPoint(2.3522, 48.8566);

        var p = Assert.Single(await _map.ConvertAsync([paris], "wgs84", "gcj02"));

        Assert.Equal(paris.Longitude, p.Longitude);
        Assert.Equal(paris.Latitude, p.Latitude);
    }

    [Fact]
    [DisplayName("坐标转换_GCJ02与BD09_往返误差可忽略")]
    public async Task Gcj02ToBd09_RoundTrip()
    {
        var gcj = new GeoPoint(121.51638785294696, 31.237133740198594);

        var bd = Assert.Single(await _map.ConvertAsync([gcj], "gcj02", "bd09"));
        var back = Assert.Single(await _map.ConvertAsync([bd], "bd09", "gcj02"));

        // BD09 反算采用近似公式，实测往返误差约 2e-7 度（厘米级）
        AssertClose(gcj.Longitude, back.Longitude, 1e-5);
        AssertClose(gcj.Latitude, back.Latitude, 1e-5);
    }

    [Fact]
    [DisplayName("坐标转换_WGS84与BD09_往返误差小于十米")]
    public async Task Wgs84ToBd09_RoundTrip()
    {
        var bd = Assert.Single(await _map.ConvertAsync([_shanghai], "wgs84", "bd09"));
        var back = Assert.Single(await _map.ConvertAsync([bd], "bd09", "wgs84"));

        // 一阶近似反算，实测误差约 1.6e-5 度（约 1.7 米）
        AssertClose(_shanghai.Longitude, back.Longitude, 1e-4);
        AssertClose(_shanghai.Latitude, back.Latitude, 1e-4);
    }

    [Fact]
    [DisplayName("坐标转换_GCJ02转WGS84_往返误差小于十米")]
    public async Task Gcj02ToWgs84_RoundTrip()
    {
        var gcj = new GeoPoint(121.51638785294696, 31.237133740198594);

        var wgs = Assert.Single(await _map.ConvertAsync([gcj], "gcj02", "wgs84"));

        // 一阶近似反算，实测往返误差约 1.6e-5 度（约 1.7 米）
        AssertClose(_shanghai.Longitude, wgs.Longitude, 1e-4);
        AssertClose(_shanghai.Latitude, wgs.Latitude, 1e-4);
    }

    [Theory]
    [DisplayName("坐标转换_坐标系大小写不敏感")]
    [InlineData("WGS84", "GCJ02")]
    [InlineData("wgs84", "gcj02")]
    [InlineData("Wgs84", "Gcj02")]
    public async Task Convert_CaseInsensitive(String from, String to)
    {
        var p = Assert.Single(await _map.ConvertAsync([_shanghai], from, to));

        AssertClose(121.51638785294696, p.Longitude);
    }

    [Fact]
    [DisplayName("坐标转换_同一坐标系返回原列表")]
    public async Task SameCoordinate_ReturnsSameInstance()
    {
        var list = new List<GeoPoint> { _shanghai };

        var rs = await _map.ConvertAsync(list, "gcj02", "GCJ02");

        Assert.Same(list, rs);
    }

    [Fact]
    [DisplayName("坐标转换_空参数抛出参数异常")]
    public async Task InvalidArguments_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _map.ConvertAsync(null, "wgs84", "gcj02"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _map.ConvertAsync([], "wgs84", "gcj02"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _map.ConvertAsync([_shanghai], null, "gcj02"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _map.ConvertAsync([_shanghai], "wgs84", ""));
    }

    [Fact]
    [DisplayName("坐标转换_不支持的组合抛出NotSupportedException")]
    public async Task UnsupportedPair_Throws()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => _map.ConvertAsync([_shanghai], "wgs84", "utm"));
    }

    [Fact]
    [DisplayName("坐标转换_MapHelper公开方法WGS84转GCJ02")]
    public void MapHelper_Wgs84ToGcj02()
    {
        MapHelper.WGS84ToGCJ02(_shanghai.Latitude, _shanghai.Longitude, out var lat, out var lon);

        AssertClose(31.237133740198594, lat);
        AssertClose(121.51638785294696, lon);
    }

    [Fact]
    [DisplayName("坐标转换_MapHelper公开方法GCJ02转WGS84")]
    public void MapHelper_Gcj02ToWgs84()
    {
        MapHelper.GCJ02ToWGS84(31.237133740198594, 121.51638785294696, out var lat, out var lon);

        // 一阶近似反算，实测误差约 1.6e-5 度（约 1.7 米）
        AssertClose(31.239184419374944, lat, 1e-4);
        AssertClose(121.51199904625513, lon, 1e-4);
    }

    [Fact]
    [DisplayName("坐标转换_单点扩展方法")]
    public async Task ConvertPointExtension()
    {
        IMap map = new AMap();

        var p = await map.ConvertAsync(_shanghai, "wgs84", "gcj02");

        Assert.NotNull(p);
        AssertClose(121.51638785294696, p.Value.Longitude);
    }
}
