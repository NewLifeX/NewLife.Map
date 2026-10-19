using System;
using System.ComponentModel;
using System.Globalization;
using MapApi;
using NewLife;
using Xunit;

namespace XUnitTest;

/// <summary>请求语言作用域测试</summary>
/// <remarks>本类会临时改写当前 UI 文化与 <see cref="Localization.Culture"/>，与其它改写全局语言的用例串行执行</remarks>
[Collection("GlobalState")]
public class RequestCultureScopeTests
{
    [Fact]
    [DisplayName("请求语言_AcceptLanguage权重_选择最高权重")]
    public void ParseAcceptLanguage_SelectsHighestWeight()
    {
        var culture = RequestCultureScope.ParseAcceptLanguage("en-US;q=0.5, zh-CN;q=1.0");

        Assert.Equal("zh-CN", culture.Name);
    }

    [Fact]
    [DisplayName("请求语言_不支持语言_退守下一个受支持语言")]
    public void ParseAcceptLanguage_SkipsUnsupported()
    {
        var culture = RequestCultureScope.ParseAcceptLanguage("sw-KE;q=1.0, fr-FR;q=0.5");

        Assert.Equal("fr-FR", culture.Name);
    }

    [Fact]
    [DisplayName("请求语言_全部不支持_返回空")]
    public void ParseAcceptLanguage_Unsupported_ReturnsNull()
    {
        Assert.Null(RequestCultureScope.ParseAcceptLanguage("sw-KE"));
        Assert.Null(RequestCultureScope.ParseAcceptLanguage("*"));
        Assert.Null(RequestCultureScope.ParseAcceptLanguage(""));
    }

    [Fact]
    [DisplayName("请求语言_中文区域_按简繁匹配")]
    public void ParseAcceptLanguage_ChineseVariant()
    {
        Assert.Equal("zh-CN", RequestCultureScope.ParseAcceptLanguage("zh-CN").Name);
        Assert.Equal("zh-TW", RequestCultureScope.ParseAcceptLanguage("zh-TW").Name);
    }

    [Fact]
    [DisplayName("请求语言_作用域_设置并还原UI文化")]
    public void Create_SetsAndRestoresCulture()
    {
        var oldCulture = Localization.Culture;
        var oldUI = CultureInfo.CurrentUICulture;
        try
        {
            Localization.Culture = null;

            using (var scope = RequestCultureScope.Create("ja-JP"))
            {
                Assert.Equal("ja-JP", CultureInfo.CurrentUICulture.Name);
            }

            Assert.Equal(oldUI.Name, CultureInfo.CurrentUICulture.Name);
        }
        finally
        {
            Localization.Culture = oldCulture;
            CultureInfo.CurrentUICulture = oldUI;
        }
    }

    [Fact]
    [DisplayName("请求语言_固定语言_不随请求变化")]
    public void Create_FixedCulture_NoChange()
    {
        var oldCulture = Localization.Culture;
        var oldUI = CultureInfo.CurrentUICulture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("en-US");

            using var scope = RequestCultureScope.Create("zh-CN");

            Assert.Equal(oldUI.Name, CultureInfo.CurrentUICulture.Name);
        }
        finally
        {
            Localization.Culture = oldCulture;
            CultureInfo.CurrentUICulture = oldUI;
        }
    }
}
