using Microsoft.Extensions.DependencyInjection;
using NewLife.Data;
using NewLife.Log;
using NewLife.Serialization;
using NewLife.Map;
using XCode.Membership;
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
                Test3();
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

            var ga = await map.GetGeoAsync(addr, null, null, false);

            Assert.NotNull(ga);
            Assert.Equal(121.5119990462553, ga.Location.Longitude);
            Assert.Equal(31.239184684191343, ga.Location.Latitude);
            Assert.Null(ga.Address);

            ga = await map.GetGeoAsync(addr, null, null, true);

            Assert.NotNull(ga);
            Assert.Equal(121.51199904625521, ga.Location.Longitude);
            Assert.Equal(31.239184551783151, ga.Location.Latitude);
            Assert.Equal("上海市浦东新区花园石桥路176号", ga.Address);
            Assert.Equal(310115, ga.Code);
            Assert.Equal(310115005, ga.Towncode);
        }

        static void Test2()
        {
            //Area.FetchAndSave();
            Console.WriteLine("total={0}", Area.Meta.Count);
            if (Area.Meta.Count == 0) Area.Import("http://x.newlifex.com/Area.csv.gz", true, 4, true);

            var ar = Area.FindByIDs(150204101, 150204, 150200, 150000);
            Console.WriteLine(ar?.ToJson(true));

        }

        static async void Test3()
        {
            var services = new ServiceCollection();

            services.AddStardust();
            services.AddSingleton<IMap, NewLifeMap>();

            var provider = services.BuildServiceProvider();

            var map = provider.GetRequiredService<IMap>();
            Assert.NotNull(map);

            for (var i = 0; i < 100; i++)
            {
                var rs = await map.GetReverseGeoAsync(new GeoPoint("109.995837,40.690028"), null);
                Assert.NotNull(rs);

                XTrace.WriteLine(rs.ToJson(true));

                Thread.Sleep(5000);
            }
        }
    }
}