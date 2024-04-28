using NewLife;
using NewLife.Data;
using XCode;
using XCode.Membership;

namespace MapApi.Locations;

public partial class Geo8 : Entity<Geo8>, IGeo
{
    #region 对象操作
    static Geo8()
    {
        // 累加字段，生成 Update xx Set Count=Count+1234 Where xxx
        //var df = Meta.Factory.AdditionalFields;
        //df.Add(nameof(Code));

        // 过滤器 UserModule、TimeModule、IPModule
        Meta.Modules.Add<TimeModule>();

        var sc = Meta.SingleCache;
        sc.Expire = 20 * 60;
        sc.FindSlaveKeyMethod = k => Find(_.Hash == k);
        sc.GetSlaveKeyMethod = e => e.Hash;
    }

    /// <summary>验证并修补数据，通过抛出异常的方式提示验证失败。</summary>
    /// <param name="isNew">是否插入</param>
    public override void Valid(Boolean isNew)
    {
        // 如果没有脏数据，则不需要进行任何处理
        if (!HasDirty) return;

        // 建议先调用基类方法，基类方法会做一些统一处理
        base.Valid(isNew);

        if (Code == 0)
            Code = DistrictId;
        else if (Code > 0)
        {
            var code = Code;
            while (code > 999999) code /= 10;

            if (ProvinceId == 0) ProvinceId = code / 10000 * 10000;
            if (CityId == 0) CityId = code / 100 * 100;
            if (DistrictId == 0) DistrictId = code;
        }

        if (Hash.IsNullOrEmpty() && Longitude != 0) Hash = GeoHash.Encode(Longitude, Latitude, 8);
        //if (HashBd09.IsNullOrEmpty()) HashBd09 = GeoHash.Encode(LongitudeBd09, LatitudeBd09, 8);

        if (Longitude != 0) Longitude = Math.Round(Longitude, 6);
        if (Latitude != 0) Latitude = Math.Round(Latitude, 6);
        if (LongitudeBd09 != 0) LongitudeBd09 = Math.Round(LongitudeBd09, 6);
        if (LatitudeBd09 != 0) LatitudeBd09 = Math.Round(LatitudeBd09, 6);
    }
    #endregion

    #region 扩展属性
    /// <summary>地区</summary>
    [Map(nameof(Code))]
    public String AreaName => Area.FindByIDs(Code, DistrictId, CityId, ProvinceId)?.Path;
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static Geo8 FindById(Int32 id)
    {
        if (id <= 0) return null;

        //// 实体缓存
        //if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }

    public static Geo8 FindByHash(String hash, Boolean cache)
    {
        if (hash.IsNullOrEmpty()) return null;

        if (cache)
            return Meta.SingleCache.GetItemWithSlaveKey(hash) as Geo8;
        else
            return Find(_.Hash == hash);
    }

    /// <summary>
    /// 根据坐标查询
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Geo8 FindByHash(GeoPoint point)
    {
        if (point == null) return null;

        var hash = GeoHash.Encode(point.Longitude, point.Latitude, 8);
        return Meta.SingleCache.GetItemWithSlaveKey(hash) as Geo8;
    }

    /// <summary>
    /// 根据坐标查询
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Geo8 FindByHashBd09(GeoPoint point)
    {
        if (point == null) return null;

        var hash = GeoHash.Encode(point.Longitude, point.Latitude, 8);
        return Find(_.HashBd09 == hash);
    }

    /// <summary>
    /// 根据坐标查询
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Geo8 FindByHashGcj02(GeoPoint point)
    {
        if (point == null) return null;

        var hash = GeoHash.Encode(point.Longitude, point.Latitude, 8);
        return Find(_.HashGcj02 == hash);
    }

    /// <summary>根据区划编码查找</summary>
    /// <param name="code">区划编码</param>
    /// <returns>实体列表</returns>
    public static IList<Geo8> FindAllByCode(Int32 code)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Code == code);

        return FindAll(_.Code == code);
    }

    /// <summary>根据编码查找</summary>
    /// <param name="hash">编码</param>
    /// <returns>实体列表</returns>
    public static IList<Geo8> FindAllByHash(String hash)
    {
        if (hash.IsNullOrEmpty()) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Hash.EqualIgnoreCase(hash));

        return FindAll(_.Hash == hash);
    }

    /// <summary>根据bd09编码查找</summary>
    /// <param name="hashBd09">bd09编码</param>
    /// <returns>实体列表</returns>
    public static IList<Geo8> FindAllByHashBd09(String hashBd09)
    {
        if (hashBd09.IsNullOrEmpty()) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.HashBd09.EqualIgnoreCase(hashBd09));

        return FindAll(_.HashBd09 == hashBd09);
    }

    /// <summary>根据gcj02编码查找</summary>
    /// <param name="hashGcj02">gcj02编码</param>
    /// <returns>实体列表</returns>
    public static IList<Geo8> FindAllByHashGcj02(String hashGcj02)
    {
        if (hashGcj02.IsNullOrEmpty()) return [];

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.HashGcj02.EqualIgnoreCase(hashGcj02));

        return FindAll(_.HashGcj02 == hashGcj02);
    }

    /// <summary>根据省份、城市、区县查找</summary>
    /// <param name="provinceId">省份</param>
    /// <param name="cityId">城市</param>
    /// <param name="districtId">区县</param>
    /// <returns>实体列表</returns>
    public static IList<Geo8> FindAllByProvinceIdAndCityIdAndDistrictId(Int32 provinceId, Int32 cityId, Int32 districtId)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.ProvinceId == provinceId && e.CityId == cityId && e.DistrictId == districtId);

        return FindAll(_.ProvinceId == provinceId & _.CityId == cityId & _.DistrictId == districtId);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="code">区划编码。最高到乡镇级行政区划编码</param>
    /// <param name="provinceId">省份</param>
    /// <param name="cityId">城市</param>
    /// <param name="districtId">区县</param>
    /// <param name="start">更新时间开始</param>
    /// <param name="end">更新时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<Geo8> Search(Int32 code, Int32 provinceId, Int32 cityId, Int32 districtId, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (code >= 0) exp &= _.Code == code;
        if (provinceId >= 0) exp &= _.ProvinceId == provinceId;
        if (cityId >= 0) exp &= _.CityId == cityId;
        if (districtId >= 0) exp &= _.DistrictId == districtId;
        exp &= _.UpdateTime.Between(start, end);
        if (!key.IsNullOrEmpty()) exp &= _.Hash == key | _.HashBd09 == key;

        return FindAll(exp, page);
    }
    #endregion

    #region 业务操作
    public Boolean IsValid() => Address.IsNullOrEmpty() && Longitude != 0 && LongitudeBd09 != 0 && LongitudeGcj02 != 0;

    public static Geo8 Upsert(GeoAddress geoAddress, GeoPoint wgs84, GeoPoint bd09, GeoPoint gcj02, Int32 days)
    {
        Geo8 onCreate(GeoPoint k)
        {
            var gd = new Geo8 { CreateTime = DateTime.Now };
            gd.Fill(geoAddress, wgs84, bd09, gcj02);
            return gd;
        }

        var g = FindByHash(wgs84) ?? FindByHashBd09(bd09) ?? FindByHashGcj02(gcj02);
        if (g == null)
        {
            if (wgs84 != null)
                g = GetOrAdd(wgs84, FindByHash, onCreate);
            else if (bd09 != null)
                g = GetOrAdd(bd09, FindByHashBd09, onCreate);
            else if (gcj02 != null)
                g = GetOrAdd(gcj02, FindByHashGcj02, onCreate);
        }

        if (!g.IsValid() || days > 0 && g.UpdateTime.AddDays(days) < DateTime.Now)
        {
            g.Fill(geoAddress, wgs84, bd09, gcj02);
            g.Update();
        }

        return g;
    }

    public void Fill(GeoAddress geo, GeoPoint wgs84, GeoPoint bd09, GeoPoint gcj02)
    {
        Code = geo.Towncode > 0 ? geo.Towncode : geo.Code;

        Address = geo.Address;
        Title = geo.Title;

        if (wgs84 != null)
        {
            Longitude = Math.Round(wgs84.Longitude, 6);
            Latitude = Math.Round(wgs84.Latitude, 6);
            if (wgs84.Longitude != 0) Hash = GeoHash.Encode(wgs84.Longitude, wgs84.Latitude, 8);
        }

        if (bd09 != null)
        {
            LongitudeBd09 = Math.Round(bd09.Longitude, 6);
            LatitudeBd09 = Math.Round(bd09.Latitude, 6);
            HashBd09 = GeoHash.Encode(bd09.Longitude, bd09.Latitude, 8);
        }

        if (gcj02 != null)
        {
            LongitudeGcj02 = Math.Round(gcj02.Longitude, 6);
            LatitudeGcj02 = Math.Round(gcj02.Latitude, 6);
            HashGcj02 = GeoHash.Encode(gcj02.Longitude, gcj02.Latitude, 8);
        }
    }
    #endregion
}