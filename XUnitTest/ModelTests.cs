using System;
using NewLife.Data;
using NewLife.Map.Models;
using Xunit;

namespace XUnitTest;

/// <summary>数据模型测试</summary>
public class ModelTests
{
    [Fact]
    public void GeoAddress_Default()
    {
        var geo = new GeoAddress();

        Assert.Null(geo.Address);
        Assert.Equal(0, geo.Code);
        Assert.Null(geo.Location);
    }

    [Fact]
    public void Driving_Default()
    {
        var d = new Driving();

        Assert.Equal(0, d.Distance);
        Assert.Equal(0, d.Duration);
    }

    [Fact]
    public void GeoModel_ToGeoAddress()
    {
        var gm = new GeoModel
        {
            Longitude = 121.5,
            Latitude = 31.2,
            Address = "上海市",
            Title = "POI",
            DistrictId = 310115,
            Province = "上海市",
            City = "上海市",
            District = "浦东新区",
            AreaCode = 310115005,
            AreaName = "花木街道",
        };

        var ga = gm.ToGeoAddress();

        Assert.NotNull(ga.Location);
        Assert.Equal(121.5, ga.Location.Value.Longitude);
        Assert.Equal("上海市", ga.Address);
        Assert.Equal(310115, ga.Code);
        Assert.Equal(310115005, ga.Towncode);
        Assert.Equal("花木街道", ga.Township);
    }

    [Fact]
    public void MapKinds_Values()
    {
        Assert.Equal(1, (Int32)MapKinds.Baidu);
        Assert.Equal(5, (Int32)MapKinds.Nominatim);
        Assert.Equal(99, (Int32)MapKinds.NewLife);
    }
}
