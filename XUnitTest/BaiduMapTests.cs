using System.Net.Http;
using System.Text;
using NewLife;
using NewLife.Http;
using NewLife.Yun;
using Xunit;

namespace XUnitTest.Yun;

public class BaiduMapTests
{
    [Fact]
    public async void Geocoder()
    {
        var addr = "上海中心";
        var map = new BaiduMap();
        var rs = await map.GetGeocoderAsync(addr);

        Assert.NotNull(rs);
        Assert.True(rs.ContainsKey("location"));

        var ga = await map.GetGeoAsync(addr, null, false);

        Assert.NotNull(ga);
        Assert.Equal(121.5119990462553, ga.Location.Longitude);
        Assert.Equal(31.239184684191343, ga.Location.Latitude);
        Assert.Null(ga.Address);

        ga = await map.GetGeoAsync(addr, null, true);

        Assert.NotNull(ga);
        Assert.Equal(121.51199904625521, ga.Location.Longitude);
        Assert.Equal(31.239184551783151, ga.Location.Latitude);
        Assert.Equal("上海市浦东新区花园石桥路176号", ga.Address);
        Assert.Equal(310115, ga.Code);
        Assert.Equal("310115005", ga.Towncode);
    }

    [Fact]
    public async void IpLocation()
    {
        var html = new HttpClient().GetString("http://myip.ipip.net");
        var ip = html?.Substring("IP：", " ");
        Assert.NotEmpty(ip);

        var map = new BaiduMap();
        var rs = await map.IpLocationAsync(ip);

        Assert.NotNull(rs);

        var addrs = (rs["full_address"] + "").Split('|');
        Assert.Equal(7, addrs.Length);
    }
}