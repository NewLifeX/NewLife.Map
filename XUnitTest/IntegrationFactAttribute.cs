using System;
using Xunit;

namespace XUnitTest;

/// <summary>集成测试。默认跳过，避免真实网络调用拖慢「全部测试」并引入环境波动</summary>
/// <remarks>
/// 依赖真实厂商 Key / OSM 网络 / 平台在线的测试统一使用本特性，同时保留 <c>Trait("Category", "Integration")</c>。
/// 通过 Skip 而不是过滤器排除，使 dotnet test 与 VS 测试资源管理器行为一致。
/// 运行方式：设置环境变量 <c>NEWLIFE_MAP_INTEGRATION=1</c>，再执行
/// <c>dotnet test --filter "Category=Integration"</c>。
/// </remarks>
public sealed class IntegrationFactAttribute : FactAttribute
{
    /// <summary>是否启用集成测试。由环境变量 NEWLIFE_MAP_INTEGRATION=1 控制</summary>
    public static Boolean Enabled { get; } = Environment.GetEnvironmentVariable("NEWLIFE_MAP_INTEGRATION") == "1";

    /// <summary>实例化。未启用时标记为跳过</summary>
    public IntegrationFactAttribute()
    {
        if (!Enabled) Skip = "集成测试默认跳过；设置环境变量 NEWLIFE_MAP_INTEGRATION=1 后运行";
    }
}
