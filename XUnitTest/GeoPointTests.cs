using System;
using System.ComponentModel;
using NewLife.Data;
using NewLife.Map.Models;
using Xunit;

namespace XUnitTest;

/// <summary>GeoPoint 坐标点与模型测试</summary>
public class GeoPointTests
{
    [Fact]
    [DisplayName("坐标点_默认构造为零坐标")]
    public void Default_Zero()
    {
        var p = new GeoPoint();

        Assert.Equal(0, p.Longitude);
        Assert.Equal(0, p.Latitude);
        Assert.Equal("0,0", p.ToString());
    }

    [Fact]
    [DisplayName("坐标点_构造与字符串输出")]
    public void Ctor_And_ToString()
    {
        var p = new GeoPoint(121.5, 31);

        Assert.Equal(121.5, p.Longitude);
        Assert.Equal(31, p.Latitude);
        Assert.Equal("121.5,31", p.ToString());
    }

    [Fact]
    [DisplayName("坐标点_字符串解析")]
    public void Parse_Valid()
    {
        var p = GeoPoint.Parse("121.505366,31.23351");

        Assert.Equal(121.505366, p.Longitude);
        Assert.Equal(31.23351, p.Latitude);
    }

    [Fact]
    [DisplayName("坐标点_非法字符串解析抛出格式异常")]
    public void Parse_Invalid_Throws()
    {
        Assert.Throws<FormatException>(() => GeoPoint.Parse(""));
        Assert.Throws<FormatException>(() => GeoPoint.Parse("abc"));

        Assert.False(GeoPoint.TryParse("abc", out _));
        Assert.True(GeoPoint.TryParse("1,2", out var p));
        Assert.Equal(1, p.Longitude);
        Assert.Equal(2, p.Latitude);
    }

    [Fact]
    [DisplayName("坐标点_GeoHash编码解码往返")]
    public void GeoHash_RoundTrip()
    {
        var p = new GeoPoint(121.511999, 31.239184);

        var hash = p.Encode();
        var q = GeoPoint.Decode(hash);

        Assert.Equal(9, hash.Length);
        Assert.InRange(q.Longitude, p.Longitude - 1e-4, p.Longitude + 1e-4);
        Assert.InRange(q.Latitude, p.Latitude - 1e-4, p.Latitude + 1e-4);
        Assert.True(GeoPoint.TryDecode(hash, out _));
    }

    [Fact]
    [DisplayName("地理地址_ToString返回地址")]
    public void GeoAddress_ToString()
    {
        var ga = new GeoAddress { Address = "上海市浦东新区" };

        Assert.Equal("上海市浦东新区", ga.ToString());
    }
}
