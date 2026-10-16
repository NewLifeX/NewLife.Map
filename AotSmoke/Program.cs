using System.Diagnostics;
using NewLife;
using NewLife.Data;
using NewLife.Map;
using NewLife.Map.Models;
using NewLife.Serialization;

var failed = 0;
var passed = 0;

void Run(String name, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  OK   {name}");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  FAIL {name}: {ex.Message}");
        failed++;
    }
}

async Task RunAsync(String name, Func<Task> action)
{
    try
    {
        await action();
        Console.WriteLine($"  OK   {name}");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  FAIL {name}: {ex.Message}");
        failed++;
    }
}

Console.WriteLine("==== NewLife.Map NativeAOT 冒烟 ====");

Run("MapFactory 提供者注册表 >= 6 家", () =>
{
    if (MapFactory.Providers.Count < 6) throw new Exception($"提供者数量不足：{MapFactory.Providers.Count}");
});

Run("MapFactory.Create 按枚举创建百度", () =>
{
    var map = MapFactory.Create(MapKinds.Baidu);
    if (map is not BaiduMap bd) throw new Exception($"类型不符：{map?.GetType().Name}");
    if (!bd.NeedKey) throw new Exception("NeedKey 应为 true");
    if (map is IDisposable d) d.Dispose();
});

Run("Map.Create 按名称创建", () =>
{
    var map = Map.Create("Baidu");
    if (map == null) throw new Exception("返回 null");
    if (map is IDisposable d) d.Dispose();
});

Run("Map.Create 按类型全名创建", () =>
{
    var map = Map.Create("NewLife.Map.WeMap");
    if (map == null) throw new Exception("返回 null");
    if (map is IDisposable d) d.Dispose();
});

await RunAsync("离线坐标互转往返 wgs84→gcj02→wgs84", async () =>
{
    using var map = new TestMap();
    var p = new GeoPoint(121.511999, 31.239184);
    var gcj = await map.ConvertAsync([p], "wgs84", "gcj02");
    if (gcj == null || gcj.Count != 1) throw new Exception("wgs84→gcj02 失败");
    var back = await map.ConvertAsync([gcj[0]], "gcj02", "wgs84");
    if (back == null) throw new Exception("gcj02→wgs84 失败");
    if (Math.Abs(back[0].Longitude - p.Longitude) > 1e-4) throw new Exception($"往返误差过大：{back[0].Longitude}");
});

await RunAsync("离线坐标互转 gcj02→bd09", async () =>
{
    using var map = new TestMap();
    var gcj = new GeoPoint(121.505366, 31.23351);
    var bd = await map.ConvertAsync([gcj], "gcj02", "bd09");
    if (bd == null || bd.Count != 1) throw new Exception("gcj02→bd09 失败");
    if (bd[0].Longitude <= 0) throw new Exception("结果异常");
});

Run("GeoHash 编解码往返", () =>
{
    var p = new GeoPoint(121.511999, 31.239184);
    var hash = p.Encode(9);
    if (hash.IsNullOrEmpty() || hash.Length != 9) throw new Exception($"编码异常：{hash}");
    var p2 = GeoPoint.Decode(hash);
    if (Math.Abs(p2.Longitude - p.Longitude) > 0.001) throw new Exception("解码误差过大");
});

Run("MapHelper.Parse 坐标字符串解析", () =>
{
    var p = MapHelper.Parse("121.511999,31.239184");
    if (!p.ToString().StartsWith("121.511999")) throw new Exception($"格式化异常：{p}");
});

Run("多 Key 轮询轮转", () =>
{
    using var map = new TestMap { AppKey = "k1,k2,k3" };
    var ks = new List<String>();
    for (var i = 0; i < 6; i++) ks.Add(map.AcquireKey2());
    if (ks.Distinct().Count() != 3) throw new Exception($"轮转异常：{String.Join(",", ks)}");
});

Run("失效 Key 熔断与恢复判定", () =>
{
    using var map = new TestMap { AppKey = "k1,k2" };
    var first = map.AcquireKey2();
    map.RemoveKey2(first, DateTime.Now.AddMinutes(1));
    for (var i = 0; i < 4; i++)
    {
        if (map.AcquireKey2() == first) throw new Exception("失效 Key 仍被轮询使用");
    }
    if (!map.IsValidKey2("INVALID_REQUEST")) throw new Exception("失效特征未命中");
});

await RunAsync("限速 MaxQps 固定间隔", async () =>
{
    using var map = new TestMap { MaxQps = 20 };
    var sw = Stopwatch.StartNew();
    await map.WaitTokenAsync2();
    await map.WaitTokenAsync2();
    sw.Stop();
    if (sw.ElapsedMilliseconds < 40) throw new Exception($"间隔不足：{sw.ElapsedMilliseconds}ms");
});

Run("故障统计初值", () =>
{
    using var map = new TestMap();
    if (map.Total != 0 || map.Success != 0 || map.Fail != 0) throw new Exception("计数初值非 0");
    if (map.SuccessRate != 0 || map.AvgTime != 0) throw new Exception("比率初值非 0");
});

Run("Nominatim 无 Key 可用", () =>
{
    using var map = new NominatimMap();
    if (map.NeedKey) throw new Exception("NeedKey 应为 false");
    if (!map.Available) throw new Exception("Available 应为 true");
});

Run("模型 JSON 序列化往返", () =>
{
    var geo = new GeoAddress
    {
        Address = "上海市浦东新区银城中路501号",
        Name = "上海中心大厦",
        Code = 310115,
        Location = new GeoPoint(121.505366, 31.23351),
    };
    var json = geo.ToJson();
    var geo2 = json.ToJsonEntity<GeoAddress>();
    if (geo2 == null || geo2.Address != geo.Address || geo2.Code != geo.Code) throw new Exception("往返结果不一致");
    // GeoPoint 为只读结构体属性，反序列化不还原坐标；GeoAddress 属于厂商响应单向 DTO，HTTP 传输使用 GeoModel
});

Run("GeoModel JSON 序列化往返", () =>
{
    var gm = new GeoModel
    {
        Longitude = 121.505366,
        Latitude = 31.23351,
        Address = "银城中路501号",
        DistrictId = 310115,
        AreaCode = 310115005,
    };
    var gm2 = gm.ToJson().ToJsonEntity<GeoModel>();
    if (gm2 == null || Math.Abs(gm2.Longitude - 121.505366) > 1e-9 || gm2.DistrictId != 310115 || gm2.AreaCode != 310115005) throw new Exception("往返结果不一致");
});

Run("GeoModel 转 GeoAddress 映射", () =>
{
    var gm = new GeoModel
    {
        Longitude = 121.505366,
        Latitude = 31.23351,
        Address = "银城中路501号",
        DistrictId = 310115,
        AreaCode = 310115005,
        AreaName = "陆家嘴街道",
    };
    var ga = gm.ToGeoAddress();
    if (ga.Location == null || ga.Code != 310115 || ga.Towncode != 310115005 || ga.Township != "陆家嘴街道") throw new Exception("映射异常");
});

Run("全部提供者可实例化并可释放", () =>
{
    foreach (var kind in new[] { MapKinds.Baidu, MapKinds.AMap, MapKinds.Tencent, MapKinds.TianDiTu, MapKinds.Nominatim })
    {
        var map = MapFactory.Create(kind);
        if (map == null) throw new Exception($"创建失败：{kind}");
        if (map is IDisposable d) d.Dispose();
    }
});

Console.WriteLine($"==== 通过 {passed} / 失败 {failed} ====");

return failed;

/// <summary>测试用地图提供者，暴露基类受保护成员供冒烟场景调用</summary>
internal sealed class TestMap : Map
{
    public String AcquireKey2() => AcquireKey();
    public void RemoveKey2(String key, DateTime reviveTime) => RemoveKey(key, reviveTime);
    public Task WaitTokenAsync2() => WaitTokenAsync();
    public Boolean IsValidKey2(String result) => IsValidKey(result);
}
