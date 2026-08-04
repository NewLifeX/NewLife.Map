using BenchmarkDotNet.Attributes;
using NewLife.Data;
using NewLife.Map;

namespace NewLife.Map.Benchmark;

/// <summary>批量坐标转换基准测试。覆盖离线公式直算与 ConvertAsync 封装路径</summary>
[MemoryDiagnoser]
public class ConvertBenchmark
{
    private List<GeoPoint>? _points;
    private Double _sum;
    private IList<GeoPoint>? _result;

    /// <summary>坐标数量维度</summary>
    [Params(100, 1000, 10000)]
    public Int32 Count { get; set; }

    /// <summary>初始化测试数据，与被测路径分离</summary>
    [GlobalSetup]
    public void Setup()
    {
        var list = new List<GeoPoint>(Count);
        for (var i = 0; i < Count; i++)
            list.Add(new GeoPoint(116.3 + i * 0.001, 39.9 + i * 0.001));

        _points = list;
        _sum = 0;
        _result = null;
    }

    /// <summary>WGS84→GCJ02 离线公式直算。结果累积到字段，防止 JIT 死代码消除</summary>
    [Benchmark]
    public void WGS84ToGCJ02_Formula()
    {
        Double sum = 0;
        foreach (var pt in _points!)
        {
            MapHelper.WGS84ToGCJ02(pt.Latitude, pt.Longitude, out var lat, out var lon);
            sum += lat + lon;
        }

        _sum = sum;
    }

    /// <summary>Map.ConvertAsync 封装路径（离线公式）。结果保留到字段</summary>
    [Benchmark]
    public void ConvertAsync_Offline()
    {
        var map = new AMap();

        _result = map.ConvertAsync(_points!, "wgs84", "gcj02").GetAwaiter().GetResult();
    }
}
