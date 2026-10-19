using System;
using System.ComponentModel;
using System.Globalization;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Map;
using Xunit;

namespace XUnitTest;

/// <summary>运行时多语言与带前缀日志测试</summary>
/// <remarks>本类会临时改写 <see cref="Localization.Culture"/>，与其它改写全局语言的用例串行执行</remarks>
[Collection("GlobalState")]
public class LocalizationTests
{
    /// <summary>记录最后一条日志的日志器</summary>
    class CaptureLog : Logger
    {
        /// <summary>最后一条日志内容</summary>
        public String Last { get; private set; }

        /// <summary>最后一条日志级别</summary>
        public LogLevel LastLevel { get; private set; }

        protected override void OnWrite(LogLevel level, String format, params Object[] args)
        {
            LastLevel = level;
            Last = format;
        }
    }

    /// <summary>测试用 Map 子类</summary>
    class TestMap : Map
    {
    }

    [Fact]
    [DisplayName("多语言_地图消息_运行时切换语言")]
    public void Translate_RuntimeCulture()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");

            var map = new TestMap();
            var ex = Assert.Throws<NotSupportedException>(() => map.ConvertAsync([new GeoPoint(1, 2)], "wgs84", "unknown").GetAwaiter().GetResult());
            Assert.Equal("未支持[wgs84]坐标系到[unknown]坐标系的转换", ex.Message);

            Localization.Culture = CultureInfo.GetCultureInfo("en-US");
            ex = Assert.Throws<NotSupportedException>(() => map.ConvertAsync([new GeoPoint(1, 2)], "wgs84", "unknown").GetAwaiter().GetResult());
            Assert.Equal("Coordinate system conversion from [wgs84] to [unknown] is not supported.", ex.Message);
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_带前缀日志_先翻译后拼接")]
    public void WriteLog_TranslatesBeforePrefix()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");

            var log = new CaptureLog();
            var map = new TestMap { Log = log };

            map.WriteLog("No available key.");

            Assert.Equal("[Test]没有可用密钥", log.Last);
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_带前缀错误日志_使用自定义前缀")]
    public void WriteError_UsesCustomPrefix()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");

            var log = new CaptureLog();
            var map = new TestMap { Log = log, LogPrefix = "[Custom]" };

            map.WriteError("No available key.");

            Assert.Equal(LogLevel.Error, log.LastLevel);
            Assert.Equal("[Custom]没有可用密钥", log.Last);
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact]
    [DisplayName("多语言_缺少译文_回退英文")]
    public void Translate_MissingKey_FallsBackToEnglish()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");

            Assert.Equal("Missing key.", Localization.Translate("Missing key."));
        }
        finally
        {
            Localization.Culture = old;
        }
    }
}
