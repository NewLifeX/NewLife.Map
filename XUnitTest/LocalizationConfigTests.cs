using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using NewLife;
using Xunit;

namespace XUnitTest;

/// <summary>MapApi 配置元数据多语言测试</summary>
/// <remarks>配置类 DisplayName/Description 为英文源文，运行时按语言翻译；与其它改写全局语言的用例串行执行</remarks>
[Collection("GlobalState")]
public class LocalizationConfigTests
{
    static LocalizationConfigTests() =>
        // MapApi 的语言表在模块初始化时注册，先显式触发，避免依赖类型加载顺序
        RuntimeHelpers.RunModuleConstructor(typeof(MapApi.Services.MapService).Module.ModuleHandle);

    [Fact]
    [DisplayName("多语言_MapApi配置元数据_十种语言均已编译")]
    public void Translate_MapApiConfig()
    {
        var old = Localization.Culture;
        try
        {
            foreach (var item in LocalizationTests.Cultures)
            {
                var culture = CultureInfo.GetCultureInfo(item);
                Localization.Culture = culture;

                var text = Localization.Translate("Map settings");
                Assert.False(text.IsNullOrEmpty(), $"{item} 缺少配置标题译文");
                Assert.NotEqual("Map settings", text);
            }

            Localization.Culture = CultureInfo.GetCultureInfo("zh-Hans");
            Assert.Equal("地图设置", Localization.Translate("Map settings"));
            Assert.Equal("启用地图。默认true", Localization.Translate("Enable map. Defaults to true."));
        }
        finally
        {
            Localization.Culture = old;
        }
    }
}
