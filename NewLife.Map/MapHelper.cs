using NewLife.Data;

namespace NewLife.Map;

/// <summary>
/// 助手类
/// </summary>
public static class MapHelper
{
    /// <summary>坐标系转换</summary>
    /// <param name="map"></param>
    /// <param name="point">需转换的源坐标</param>
    /// <param name="from">源坐标类型。wgs84ll/gcj02/bd09ll</param>
    /// <param name="to">目标坐标类型。gcj02/bd09ll</param>
    /// <returns></returns>
    public static async Task<GeoPoint?> ConvertAsync(this IMap map, GeoPoint point, String from, String to)
    {
        var list = await map.ConvertAsync([point], from, to);
        return list == null || list.Count == 0 ? null : list[0];
    }

    private const Double xPi = Math.PI * 3000.0 / 180.0;
    private const Double pi = Math.PI;
    private const Double a = 6378245.0;
    private const Double ee = 0.00669342162296594323;

    /// <summary>判断是否在中国境内</summary>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    /// <returns></returns>
    private static Boolean OutOfChina(Double lat, Double lon)
    {
        if (lon < 72.004 || lon > 137.8347)
            return true;
        if (lat < 0.8293 || lat > 55.8271)
            return true;
        return false;
    }

    /// <summary>转换纬度</summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static Double TransformLat(Double x, Double y)
    {
        var ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
        ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
        ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
        return ret;
    }

    /// <summary>转换经度</summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static Double TransformLon(Double x, Double y)
    {
        var ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
        ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
        ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0 * pi)) * 2.0 / 3.0;
        return ret;
    }

    /// <summary>WGS84到WCJ02</summary>
    /// <param name="wgLat"></param>
    /// <param name="wgLon"></param>
    /// <param name="mgLat"></param>
    /// <param name="mgLon"></param>
    public static void WGS84ToGCJ02(Double wgLat, Double wgLon, out Double mgLat, out Double mgLon)
    {
        if (OutOfChina(wgLat, wgLon))
        {
            mgLat = wgLat;
            mgLon = wgLon;
            return;
        }
        var dLat = TransformLat(wgLon - 105.0, wgLat - 35.0);
        var dLon = TransformLon(wgLon - 105.0, wgLat - 35.0);
        var radLat = wgLat / 180.0 * pi;
        var magic = Math.Sin(radLat);
        magic = 1 - ee * magic * magic;
        var sqrtMagic = Math.Sqrt(magic);
        dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
        dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
        mgLat = wgLat + dLat;
        mgLon = wgLon + dLon;
    }

    /// <summary>GCJ02到WGS84</summary>
    /// <param name="mgLat"></param>
    /// <param name="mgLon"></param>
    /// <param name="wgLat"></param>
    /// <param name="wgLon"></param>
    public static void GCJ02ToWGS84(Double mgLat, Double mgLon, out Double wgLat, out Double wgLon)
    {
        Double dLat, dLon;
        WGS84ToGCJ02(mgLat, mgLon, out dLat, out dLon);
        wgLat = mgLat * 2 - dLat;
        wgLon = mgLon * 2 - dLon;
    }

    internal static Double[] WGS84ToBD09(Double lat, Double lon)
    {
        var gcj02 = WGS84ToGCJ02(lat, lon);
        return GCJ02ToBD09(gcj02[0], gcj02[1]);
    }

    internal static Double[] BD09ToWGS84(Double lat, Double lon)
    {
        var gcj02 = BD09ToGCJ02(lat, lon);
        return GCJ02ToWGS84(gcj02[0], gcj02[1]);
    }

    internal static Double[] WGS84ToGCJ02(Double lat, Double lon)
    {
        if (OutOfChina(lat, lon))
        {
            return [lat, lon];
        }
        var dLat = TransformLat(lon - 105.0, lat - 35.0);
        var dLon = TransformLon(lon - 105.0, lat - 35.0);
        var radLat = lat / 180.0 * pi;
        var magic = Math.Sin(radLat);
        magic = 1 - ee * magic * magic;
        var sqrtMagic = Math.Sqrt(magic);
        dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
        dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
        var mgLat = lat + dLat;
        var mgLon = lon + dLon;
        return [mgLat, mgLon];
    }

    internal static Double[] GCJ02ToWGS84(Double lat, Double lon)
    {
        var gcj02 = WGS84ToGCJ02(lat, lon);
        var dLat = gcj02[0] - lat;
        var dLon = gcj02[1] - lon;
        return [lat - dLat, lon - dLon];
    }

    internal static Double[] GCJ02ToBD09(Double lat, Double lon)
    {
        var z = Math.Sqrt(lon * lon + lat * lat) + 0.00002 * Math.Sin(lat * xPi);
        var theta = Math.Atan2(lat, lon) + 0.000003 * Math.Cos(lon * xPi);
        var bdLon = z * Math.Cos(theta) + 0.0065;
        var bdLat = z * Math.Sin(theta) + 0.006;
        return [bdLat, bdLon];
    }

    internal static Double[] BD09ToGCJ02(Double lat, Double lon)
    {
        var x = lon - 0.0065;
        var y = lat - 0.006;
        var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * xPi);
        var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * xPi);
        var gcjLon = z * Math.Cos(theta);
        var gcjLat = z * Math.Sin(theta);
        return [gcjLat, gcjLon];
    }
}
