using System.ComponentModel;
using NewLife.Data;
using NewLife.Log;
using NewLife.Map.Models;
using NewLife.Model;
using NewLife.Remoting;
using Stardust;
using Stardust.Registry;

namespace NewLife.Map;

/// <summary>新生命地图</summary>
/// <remarks>
/// </remarks>
[DisplayName("新生命地图")]
public class NewLifeMap : Map, IMap
{
    #region 属性
    #endregion

    #region 构造
    /// <summary>实例化</summary>
    public NewLifeMap() => KeyName = "key";

    /// <summary>通过制定服务端地址来实例化</summary>
    /// <param name="server"></param>
    public NewLifeMap(String server) : this() => Server = server;

    /// <summary>使用服务提供者实例化</summary>
    /// <param name="serviceProvider"></param>
    public NewLifeMap(IServiceProvider serviceProvider) : this() => Task.Run(() => InitAsync(serviceProvider)).Wait();

    /// <summary>使用服务提供者初始化，借助星尘注册中心能力</summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public async Task InitAsync(IServiceProvider serviceProvider)
    {
        // 借助星尘注册中心，获取服务端地址
        var registry = serviceProvider.GetService<IRegistry>();
        if (registry != null)
        {
            // 星尘创建客户端，绑定注册中心，可自动更新服务端地址
            _client = await registry.CreateForServiceAsync("NewLife.Map") as ApiHttpClient;

            if (_client != null)
            {
                Server = _client.Services.Join(",", e => e.Address);
                XTrace.WriteLine("由星尘注册中心绑定NewLife.Map地址：{0}", Server);
            }
        }
        else
        {
            var star = serviceProvider.GetService<StarFactory>();
            if (star != null)
            {
                // 单次获取服务端地址，后续不再改变
                var models = await star.ResolveAddressAsync("NewLife.Map");
                Server = models.Join(",");
                if (!Server.IsNullOrEmpty()) XTrace.WriteLine("由星尘注册中心取得NewLife.Map地址：{0}", Server);
            }
        }
    }
    #endregion

    #region 方法
    private ApiHttpClient? _client;
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

    #region 地理编码
    /// <summary>
    /// 地址解析。
    /// </summary>
    /// <param name="address"></param>
    /// <param name="city"></param>
    /// <param name="coordtype"></param>
    /// <param name="formatAddress"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<GeoAddress?> GetGeoAsync(String address, String? city = null, String? coordtype = null, Boolean formatAddress = false) => throw new NotImplementedException();
    #endregion

    #region 逆地理编码
    /// <summary>根据坐标获取地址</summary>
    /// <param name="point"></param>
    /// <param name="coordtype"></param>
    /// <returns></returns>
    public async Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, String? coordtype)
    {
        var rs = await GetGeoInfo(point, coordtype);
        if (rs == null) return null;

        return rs.ToGeoAddress();
    }

    /// <summary>根据坐标获取地理编码信息</summary>
    /// <param name="point"></param>
    /// <param name="coordtype"></param>
    /// <returns></returns>
    public async Task<GeoModel?> GetGeoInfo(GeoPoint point, String? coordtype)
    {
        return await GetClient().GetAsync<GeoModel>("/Map/ReverseGeo", new
        {
            lng = point.Longitude,
            lat = point.Latitude,
            coortype = coordtype
        });
    }
    #endregion

    #region 驾驶距离
    /// <summary>
    /// 驾驶距离。（未实现）
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="coordtype"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<Driving?> GetDistanceAsync(GeoPoint origin, GeoPoint destination, String? coordtype, Int32 type = 0) => throw new NotImplementedException();
    #endregion
}