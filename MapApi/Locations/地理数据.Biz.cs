using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Threading;
using NewLife.Web;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;
using XCode.Membership;
using XCode.Shards;

namespace MapApi.Locations
{
    public partial class Geo : Entity<Geo>
    {
        #region 对象操作
        static Geo()
        {
            // 累加字段，生成 Update xx Set Count=Count+1234 Where xxx
            //var df = Meta.Factory.AdditionalFields;
            //df.Add(nameof(Code));

            // 过滤器 UserModule、TimeModule、IPModule
            Meta.Modules.Add<TimeModule>();
        }

        /// <summary>验证并修补数据，通过抛出异常的方式提示验证失败。</summary>
        /// <param name="isNew">是否插入</param>
        public override void Valid(Boolean isNew)
        {
            // 如果没有脏数据，则不需要进行任何处理
            if (!HasDirty) return;

            // 建议先调用基类方法，基类方法会做一些统一处理
            base.Valid(isNew);

            // 在新插入数据或者修改了指定字段时进行修正
            //if (isNew && !Dirtys[nameof(CreateTime)]) CreateTime = DateTime.Now;
            //if (!Dirtys[nameof(UpdateTime)]) UpdateTime = DateTime.Now;
        }

        ///// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void InitData()
        //{
        //    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
        //    if (Meta.Session.Count > 0) return;

        //    if (XTrace.Debug) XTrace.WriteLine("开始初始化Geo[地理数据]数据……");

        //    var entity = new Geo();
        //    entity.Hash = "abc";
        //    entity.Longitude = 0.0;
        //    entity.Latitude = 0.0;
        //    entity.HashBd09 = "abc";
        //    entity.LongitudeBd09 = 0.0;
        //    entity.LatitudeBd09 = 0.0;
        //    entity.HashGcj02 = "abc";
        //    entity.LongitudeGcj02 = 0.0;
        //    entity.LatitudeGcj02 = 0.0;
        //    entity.Code = 0;
        //    entity.ProvinceId = 0;
        //    entity.CityId = 0;
        //    entity.DistrictId = 0;
        //    entity.Address = "abc";
        //    entity.Title = "abc";
        //    entity.CreateTime = DateTime.Now;
        //    entity.UpdateTime = DateTime.Now;
        //    entity.Insert();

        //    if (XTrace.Debug) XTrace.WriteLine("完成初始化Geo[地理数据]数据！");
        //}

        ///// <summary>已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert</summary>
        ///// <returns></returns>
        //public override Int32 Insert()
        //{
        //    return base.Insert();
        //}

        ///// <summary>已重载。在事务保护范围内处理业务，位于Valid之后</summary>
        ///// <returns></returns>
        //protected override Int32 OnDelete()
        //{
        //    return base.OnDelete();
        //}
        #endregion

        #region 扩展属性
        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns>实体对象</returns>
        public static Geo FindById(Int32 id)
        {
            if (id <= 0) return null;

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

            // 单对象缓存
            return Meta.SingleCache[id];

            //return Find(_.Id == id);
        }

        /// <summary>根据编码查找</summary>
        /// <param name="hash">编码</param>
        /// <returns>实体列表</returns>
        public static IList<Geo> FindAllByHash(String hash)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Hash.EqualIgnoreCase(hash));

            return FindAll(_.Hash == hash);
        }

        /// <summary>根据bd09编码查找</summary>
        /// <param name="hashBd09">bd09编码</param>
        /// <returns>实体列表</returns>
        public static IList<Geo> FindAllByHashBd09(String hashBd09)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.HashBd09.EqualIgnoreCase(hashBd09));

            return FindAll(_.HashBd09 == hashBd09);
        }

        /// <summary>根据gcj02编码查找</summary>
        /// <param name="hashGcj02">gcj02编码</param>
        /// <returns>实体列表</returns>
        public static IList<Geo> FindAllByHashGcj02(String hashGcj02)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.HashGcj02.EqualIgnoreCase(hashGcj02));

            return FindAll(_.HashGcj02 == hashGcj02);
        }

        /// <summary>根据区划编码查找</summary>
        /// <param name="code">区划编码</param>
        /// <returns>实体列表</returns>
        public static IList<Geo> FindAllByCode(Int32 code)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Code == code);

            return FindAll(_.Code == code);
        }
        #endregion

        #region 高级查询
        /// <summary>高级查询</summary>
        /// <param name="hash">编码。GeoHash编码</param>
        /// <param name="hashBd09">bd09编码。GeoHash编码</param>
        /// <param name="hashGcj02">gcj02编码。GeoHash编码</param>
        /// <param name="code">区划编码。最高到乡镇级行政区划编码</param>
        /// <param name="provinceId">省份</param>
        /// <param name="cityId">城市</param>
        /// <param name="districtId">区县</param>
        /// <param name="start">更新时间开始</param>
        /// <param name="end">更新时间结束</param>
        /// <param name="key">关键字</param>
        /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
        /// <returns>实体列表</returns>
        public static IList<Geo> Search(String hash, String hashBd09, String hashGcj02, Int32 code, Int32 provinceId, Int32 cityId, Int32 districtId, DateTime start, DateTime end, String key, PageParameter page)
        {
            var exp = new WhereExpression();

            if (!hash.IsNullOrEmpty()) exp &= _.Hash == hash;
            if (!hashBd09.IsNullOrEmpty()) exp &= _.HashBd09 == hashBd09;
            if (!hashGcj02.IsNullOrEmpty()) exp &= _.HashGcj02 == hashGcj02;
            if (code >= 0) exp &= _.Code == code;
            if (provinceId >= 0) exp &= _.ProvinceId == provinceId;
            if (cityId >= 0) exp &= _.CityId == cityId;
            if (districtId >= 0) exp &= _.DistrictId == districtId;
            exp &= _.UpdateTime.Between(start, end);
            if (!key.IsNullOrEmpty()) exp &= _.Hash.Contains(key) | _.HashBd09.Contains(key) | _.HashGcj02.Contains(key) | _.Address.Contains(key) | _.Title.Contains(key);

            return FindAll(exp, page);
        }

        // Select Count(Id) as Id,Hash From Geo Where CreateTime>'2020-01-24 00:00:00' Group By Hash Order By Id Desc limit 20
        static readonly FieldCache<Geo> _HashCache = new FieldCache<Geo>(nameof(Hash))
        {
            //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
        };

        /// <summary>获取编码列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
        /// <returns></returns>
        public static IDictionary<String, String> GetHashList() => _HashCache.FindAllName();

        // Select Count(Id) as Id,HashBd09 From Geo Where CreateTime>'2020-01-24 00:00:00' Group By HashBd09 Order By Id Desc limit 20
        static readonly FieldCache<Geo> _HashBd09Cache = new FieldCache<Geo>(nameof(HashBd09))
        {
            //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
        };

        /// <summary>获取bd09编码列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
        /// <returns></returns>
        public static IDictionary<String, String> GetHashBd09List() => _HashBd09Cache.FindAllName();

        // Select Count(Id) as Id,HashGcj02 From Geo Where CreateTime>'2020-01-24 00:00:00' Group By HashGcj02 Order By Id Desc limit 20
        static readonly FieldCache<Geo> _HashGcj02Cache = new FieldCache<Geo>(nameof(HashGcj02))
        {
            //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
        };

        /// <summary>获取gcj02编码列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
        /// <returns></returns>
        public static IDictionary<String, String> GetHashGcj02List() => _HashGcj02Cache.FindAllName();
        #endregion

        #region 业务操作
        #endregion
    }
}