using NewLife.Collections;
using NewLife.Map.Models;
using NewLife.Reflection;

namespace NewLife.Map;

/// <summary>地图工厂</summary>
public static class MapFactory
{
    #region 静态构造
    static MapFactory()
    {
        Register<BaiduMap>(MapKinds.Baidu);
        Register<AMap>(MapKinds.AMap);
        Register<WeMap>(MapKinds.Tencent);
        Register<TianDiTu>(MapKinds.TianDiTu);

        Register<NewLifeMap>(MapKinds.NewLife);
    }
    #endregion

    #region 提供者
    private static readonly IDictionary<MapKinds, Type> _providers = new NullableDictionary<MapKinds, Type>();
    /// <summary>地图提供者</summary>
    public static IDictionary<MapKinds, Type> Providers => _providers;

    /// <summary>注册地图提供者</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbType"></param>
    public static void Register<T>(MapKinds dbType) where T : IMap, new() => _providers[dbType] = typeof(T);

    /// <summary>根据地图类型创建提供者</summary>
    /// <param name="dbType"></param>
    /// <returns></returns>
    public static IMap? Create(MapKinds dbType) => _providers[dbType]?.CreateInstance() as IMap;
    #endregion
}
