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
        Register<WeMap>(MapKinds.WeMap);
        Register<TianDiTu>(MapKinds.TianDiTu);

        Register<NewLifeMap>(MapKinds.NewLife);
    }
    #endregion

    #region 提供者
    private static readonly IDictionary<MapKinds, Type> _dbs = new NullableDictionary<MapKinds, Type>();
    /// <summary>注册数据库提供者</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbType"></param>
    public static void Register<T>(MapKinds dbType) where T : IMap, new() => _dbs[dbType] = typeof(T);

    /// <summary>根据数据库类型创建提供者</summary>
    /// <param name="dbType"></param>
    /// <returns></returns>
    public static IMap? Create(MapKinds dbType) => _dbs[dbType]?.GetType().CreateInstance() as IMap;
    #endregion
}
