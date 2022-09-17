using NewLife.Log;
using NewLife.Yun;
using Xunit;

namespace Test
{
    class Program
    {
        static void Main(String[] args)
        {
            XTrace.UseConsole();

            try
            {
                //TestHyperLogLog();
                Test1();
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
            }

            Console.WriteLine("OK!");
            Console.ReadKey();
        }

        static async void Test1()
        {
            var addr = "上海中心";
            var map = new BaiduMap();
            //var rs = await map.GetGeocoderAsync(addr);

            //Assert.NotNull(rs);
            //Assert.True(rs.ContainsKey("location"));

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
    }
}