using System.ComponentModel;
using NewLife.Data;
using NewLife.Map.Models;
using NewLife.Remoting;

namespace NewLife.Yun;

/// <summary>新生命地图</summary>
/// <remarks>
/// </remarks>
[DisplayName("新生命地图")]
public class NewLifeMap : Map, IMap
{
    #region 属性
    /// <summary>服务端地址</summary>
    public String Server { get; set; }
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public NewLifeMap() => KeyName = "key";

    /// <summary>使用服务提供者实例化</summary>
    /// <param name="serviceProvider"></param>
    public NewLifeMap(IServiceProvider serviceProvider) : this()
    {
        //todo 借助星尘注册中心，获取服务端地址
    }
    #endregion

    #region 方法
    private ApiHttpClient _client;
    /// <summary>获取通信客户端</summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected ApiHttpClient GetClient()
    {
        if (_client != null && !_client.Disposed) return _client;

        if (Server.IsNullOrEmpty()) throw new Exception("NewLifeMap服务端地址未指定");

        _client = new ApiHttpClient(Server);

        return _client;
    }
    #endregion

    #region 地址编码
    /// <summary>
    /// 地址解析。
    /// </summary>
    /// <param name="address"></param>
    /// <param name="city"></param>
    /// <param name="formatAddress"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<GeoAddress> GetGeoAsync(String address, String city = null, Boolean formatAddress = false) => throw new NotImplementedException();
    #endregion

    #region 逆地址编码
    /// <summary>根据坐标获取地址</summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public async Task<GeoAddress> GetReverseGeoAsync(GeoPoint point)
    {
        var rs = await GetGeoModel(point);
        if (rs == null) return null;

        return rs.ToGeoAddress();
    }

    /// <summary>根据坐标获取地址</summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public async Task<GeoModel> GetGeoModel(GeoPoint point)
    {
        return await GetClient().GetAsync<GeoModel>("/Map/ReverseGeo", new
        {
            lng = point.Longitude,
            lat = point.Latitude,
            coorType = CoordType
        });
    }
    #endregion

    #region 驾驶距离
    /// <summary>
    /// 驾驶距离。（未实现）
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<Driving> GetDistanceAsync(GeoPoint origin, GeoPoint destination, Int32 type = 0) => throw new NotImplementedException();
    #endregion
}