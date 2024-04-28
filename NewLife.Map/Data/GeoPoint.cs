namespace NewLife.Data;

/// <summary>经纬度坐标</summary>
public class GeoPoint
{
    #region 属性
    /// <summary>经度</summary>
    public Double Longitude { get; }

    /// <summary>纬度</summary>
    public Double Latitude { get; }
    #endregion

    #region 构造
    /// <summary>经纬度坐标</summary>
    public GeoPoint() { }

    /// <summary>实例化经纬度坐标</summary>
    /// <param name="longitude"></param>
    /// <param name="latitude"></param>
    public GeoPoint(Double longitude, Double latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }

    /// <summary>实例化经纬度坐标</summary>
    /// <param name="longitude"></param>
    /// <param name="latitude"></param>
    public GeoPoint(Object longitude, Object latitude)
    {
        Longitude = longitude.ToDouble();
        Latitude = latitude.ToDouble();
    }

    /// <summary>经纬度坐标</summary>
    /// <param name="location"></param>
    public GeoPoint(String? location)
    {
        if (!location.IsNullOrEmpty())
        {
            var ss = location.Split(',');
            if (ss.Length >= 2)
            {
                Longitude = ss[0].ToDouble();
                Latitude = ss[1].ToDouble();
            }
        }
    }
    #endregion

    #region 方法
    /// <summary>编码坐标点为GeoHash字符串</summary>
    /// <param name="level">字符个数。默认9位字符编码，精度2米</param>
    /// <returns></returns>
    public String Encode(Int32 level = 9) => GeoHash.Encode(Longitude, Latitude, level);

    /// <summary>解码GeoHash字符串为坐标点</summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static GeoPoint Decode(String hash)
    {
        var p = GeoHash.Decode(hash);
        return new(p.Longitude, p.Latitude);
    }
    #endregion

    /// <summary>已重载</summary>
    /// <returns></returns>
    public override String ToString() => $"{Longitude},{Latitude}";
}