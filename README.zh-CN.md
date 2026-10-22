# NewLife.Map - 统一地图组件库

![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Map?logo=github)


![GitHub License](https://img.shields.io/github/license/NewLifeX/NewLife.Map?logo=github)
![NuGet Downloads](https://img.shields.io/nuget/dt/NewLife.Map?logo=nuget)
![NuGet](https://img.shields.io/nuget/v/NewLife.Map?logo=nuget)
![NuGet prerelease](https://img.shields.io/nuget/vpre/NewLife.Map?label=dev%20nuget&logo=nuget)
[![test](https://github.com/NewLifeX/NewLife.Map/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/NewLife.Map/actions/workflows/test.yml)
[![locales](https://img.shields.io/badge/locales-11-2ea44f?logo=dotnet)](Doc/多语言本地化.md)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS%20%7C%20Android%20%7C%20iOS-blue)
![arch](https://img.shields.io/badge/arch-x64%20%7C%20arm64%20%7C%20loongarch64-orange)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/NewLife.Map?logo=github)](https://github.com/NewLifeX/NewLife.Map/graphs/contributors)

<!-- lang:start -->
🌐 [English](README.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

统一封装百度、高德、腾讯、天地图与企业地图接口平台的地图组件库，以一套接口屏蔽各家差异，提供地理编码、逆地理编码、距离计算、坐标转换与多 Key 熔断能力。  

源码： https://github.com/NewLifeX/NewLife.Map  
Nuget：`NewLife.Map`  

> ℹ️ 本中文版与 [English README](README.md) 同批维护；若两者内容不一致，以英文版为对外口径。

## 仓库组成
| 工程 | 定位 | 说明 |
| ---- | ---- | ---- |
| `NewLife.Map` | 地图组件库（NuGet 包） | 统一接口 `IMap` + 多厂商适配 + 坐标转换 + Key 熔断 |
| `MapApi` | 企业地图接口平台（Web 服务） | 聚合多厂商 + GeoHash 多级缓存 + 魔方后台管理 |
| `XUnitTest` | 单元测试 | 各提供者地理编码 / 逆地理 / 距离 / 坐标转换测试（真实服务集成用例默认跳过） |

## 文档
- [需求文档](Doc/需求文档.md) — 愿景、核心目标、功能需求、暂缓清单
- [功能清单](Doc/功能清单.md) — 模块/功能编码与实现/测试/注释三维状态追踪
- [架构设计](Doc/架构设计.md) — 分层、组件、关键流程与设计决策
- [竞品分析](Doc/竞品分析.md) — 与 Geocoding.net / ProjNET / 国产单厂商封装等对比矩阵与差距分析

## 功能特性
- 统一接口 `IMap`，屏蔽百度 / 高德 / 腾讯 / 天地图 / 企业自建（NewLifeMap）差异，切换厂商零改动
- 地理编码（地址→坐标）/ 逆地理编码（坐标→地址），自动融合 POI 语义信息补全地址组件
- 路径距离 / 驾车时间，不同厂商策略参数差异已抽象
- 行政区划查询（高德 / 腾讯）、POI / 区域检索（百度 / 天地图）、IP 定位（百度 / 腾讯 / 平台在线解析）
- 坐标系互转：WGS84 / GCJ02 / BD09 内置离线公式 + 厂商在线批量转换（百度特殊 from/to 映射已封装）
- 多 Key 轮询与自动熔断 + 定时恢复，单个 Key 超限 / 封禁不影响整体可用
- 令牌桶限速 `MaxQps` 与请求统计（总数 / 成功率 / 平均耗时），避免单 Key 高频请求被封禁
- 可选抛出异常 `ThrowException`，便于上层统一处理
- 多目标框架：net45 / net461 / netstandard2.0 / netstandard2.1（兼容老项目与现代 .NET）
- 强命名签名，适合反射与插件体系
- 内置运行时多语言：11 种语言，英文为源文与兜底；异常消息、日志与程序集元数据经编译期词表按当前语言翻译
- 星尘注册中心集成：`NewLifeMap` 提供者自动发现服务端地址，多实例 / 蓝绿发布无感切换

## 安装
```bash
# 稳定版
 dotnet add package NewLife.Map
# 预览版（带日期）
 dotnet add package NewLife.Map --prerelease
```

## 快速开始
```csharp
using NewLife.Map;
using NewLife.Data;

// 选择一个具体地图提供者（BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap）
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // 多个 key 用逗号分隔

// 1. 正向地理编码：地址 -> 坐标
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  语义:{geo?.Title}");

// 2. 逆地理编码：坐标 -> 地址
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. 距离（驾车）
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. 坐标转换（离线公式）
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> 坐标系需使用球面坐标（经纬度），输出为 BD09 坐标系的点。

## 通过工厂动态创建
```csharp
var m1 = Map.Create("Baidu");           // 自动匹配 *Map 后缀
var m2 = Map.Create("AMap");            // 高德
var m3 = Map.Create("NewLifeMap");

// 或按枚举创建
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` 在实例化后设置；多个 key 以逗号分隔，内部轮询使用。

## 地图提供者
| 提供者 | 类名 | 主要能力 | 备注 |
| ------ | ---- | -------- | ---- |
| 百度地图 | `BaiduMap` | 地理/逆地理/距离/POI检索/IP定位/坐标转换(在线+离线) | `KeyName=ak` |
| 高德地图 | `AMap` | 地理/逆地理/距离/行政区划 | `KeyName=key` |
| 腾讯地图 | `WeMap` | 地理/逆地理/距离/IP定位/行政区划 | `KeyName=key` |
| 天地图 | `TianDiTu` | 地理/逆地理/距离(含途经点)/POI检索 | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | 地理/逆地理（开源免费无 Key，WGS84） | 无需 Key |
| 新生命地图平台 | `NewLifeMap` | 企业聚合接口（对接 MapApi 平台） | 通过 Stardust 服务发现 |

> 新增提供者请参考下文「扩展一个新地图提供者步骤」，按 AMap/BaiduMap 模式实现。

## 坐标系转换
| 名称 | 说明 | 常见厂商 |
| ---- | ---- | -------- |
| WGS84 | 国际通用 GPS | 设备原始定位 |
| GCJ02 | 国测局加偏 | 高德 / 腾讯 / 绝大多数国内在线地图 |
| BD09 | 百度加偏 | 百度 |

```csharp
// 离线公式转换，不消耗厂商配额
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// 百度在线批量转换，from/to 索引值已封装
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## AppKey 多 Key 轮询与熔断
- 传入 `AppKey="k1,k2,k3"`，每次请求递增索引取余轮询
- 响应命中无效关键字（如 `TOO_FREQUENT` / `LIMIT` / `INVALID` 等）时调用 `RemoveKey()` 暂时下线
- 下线 Key 进入 `_pendingKeys`，由 `TimerX` 定时扫描，到期自动恢复
- 恢复时间策略：当前实现为调用处自行传入（例如 1 小时）

## NewLifeMap 聚合平台
`NewLifeMap` 指向仓库内置的 `MapApi` 企业地图接口平台，平台侧聚合多家厂商并缓存结果，客户端无需感知底层差异。

```csharp
// 方式1：直接指定服务端
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// 方式2：依赖注入 + Stardust Registry 自动解析地址
services.AddSingleton<IMap, NewLifeMap>();
// NewLifeMap(IServiceProvider) 构造函数中会尝试：IRegistry -> StarFactory
```

- 绑定的 `ApiHttpClient` 会自动更新服务端地址列表，MapApi 多副本 / 蓝绿发布时无感切换
- 平台侧提供 `GET /Map/Geo`、`/Map/ReverseGeo`（逆地理）、`/Map/GeoAddress`（正向地理）、`/Map/Distance`（距离）统一接口，GeoHash 6~9 位四级缓存降低厂商调用量

## 错误处理
- 默认：接口返回非成功状态不抛异常，方法返回 `null`
- 设置 `map.ThrowException = true;` 后抛出 `Exception`，便于外层统一捕获
- 最近一次请求调试信息：`LastUrl` / `LastString` / `LastResult` / `LastKey`

## 常用 API 一览
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
扩展（部分提供者特有）：
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // BaiduMap / TianDiTu POI / 区域检索
Task<IList<GeoArea>> GetAreaAsync(...);       // AMap 行政区划
Task<IDictionary<string, object?>> IpLocationAsync(...); // BaiduMap / WeMap IP 定位
```

## 扩展一个新地图提供者
1. 新建类 `XXXMap : Map, IMap`
2. 设置 `Server` / `KeyName`
3. 重写 `InvokeAsync<T>` 解析该厂商统一返回结构 & 错误码
4. 实现 `GetGeoAsync` / `GetReverseGeoAsync` / 其它所需能力
5. 若有专属限流/失效特征，重写 `IsValidKey` 返回 true 触发临时下线
6. 在 `MapFactory` 静态构造中 `Register<XXXMap>(MapKinds.XXX)` 注册
7. 提交单元测试覆盖主要功能与边界

## 单元测试
仓库包含 `XUnitTest` 工程，解决方案为 `NewLife.Map.slnx`（VS 2026 / .NET 10 新版 XML 格式）：
- 默认运行**确定性测试**：84 个用例，本地 HTTP 桩服务覆盖各厂商 URL 构造与响应解析，测试执行约 0.4~1 秒，可在全部 RID（含 CI）稳定执行
- 真实厂商 / OSM / 平台在线测试标记 `Category=Integration`，并由 `IntegrationFact` **默认跳过**；VS 测试资源管理器「全部运行」与 `dotnet test` 行为一致
- 启用集成测试：先设置环境变量 `NEWLIFE_MAP_INTEGRATION=1`，再以 `--filter "Category=Integration"` 运行
- 请将真实 Key 放入本地用户机密或环境变量，不要提交到仓库

运行（Windows PowerShell）：
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # 可选：真实环境联调
```

## 构建
```bash
dotnet build -c Release
```
输出（多框架编译）位于 `Bin/`。

## 兼容性测试（CI）
push / PR 触发，覆盖主流 RID：
- 单元测试矩阵：win-x64 / linux-x64 / linux-arm64 / linux-la64（龙芯，QEMU 仿真 + 龙芯 .NET SDK，见 `test.yml`）
- NativeAOT 冒烟矩阵：同上四个 RID（`AotSmoke` 工程，18 个场景），见 `aot.yml`

## Roadmap / TODO
> 2026-08-04 已全部完成：并发限速（MAP-12）、故障统计 Metrics（MAP-13）、Benchmark 基准对比（MAP-14）

## 版本策略
主版本 = 兼容性变更；次版本 = 新功能；修订号 = 修复 / 优化。`VersionSuffix` 带日期，便于快速定位构建。

## 贡献
欢迎 Issue / PR：Bug、性能优化、支持更多地图、补充文档与测试。提交前请阅读 `.github/copilot-instructions.md` 以保持代码风格一致。

## 许可证
MIT，保留版权声明即可自由使用于商业 / 开源项目。

---

## 生态版图

NewLife 生态按依赖分层，各项目独立版本、独立发版。下面的版图在所有 NewLife 仓库中逐字一致；每个项目链接到代码仓库与主 NuGet 包。

| 项目 | 一句话定位 | 下载量 |
| --- | --- | --- |
| **基础底座** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | 日志、配置、缓存、网络、序列化、定时调度与 APM 追踪。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | 数据中间件：单表百亿级、自动分表、读写分离，支持 10 多种数据库。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 高性能网络库：单机 177 万 pps、14.5 Gbps（TCP 1KB，2026-10 实测），400 万 Tcp 长连接。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | 协议通信与 RPC 框架，面向 CS 应用与物联网设备。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | 把 .NET 应用安装为操作系统守护进程：Windows 服务与 Linux systemd。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | `dotnet new` 项目模板：控制台、Web、WebApi、服务、数据与物联网。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **数据存储与消息** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis 客户端：微秒级延迟、百万级吞吐，内置消息队列。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 纯托管 RocketMQ 客户端，支持 Apache RocketMQ 与阿里云消息队列。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT 客户端与服务端，客户端支持阿里云物联网。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | C# 混合数据库：嵌入/网络双模，关系型、时序、消息队列与 KV 于一体。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | MySql 驱动与数据访问辅助层，基于 NewLife 数据栈实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web、AI 与分布式** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 魔方快速开发平台：用户权限、SSO、OAuth 服务端与 CRUD 脚手架。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | 星尘分布式服务平台：节点管理、APM、配置中心、注册中心与发布中心。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | 蚂蚁调度：分布式任务与实时数据平台，支持分片、重试与回溯补算。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | 统一接入 46 家大模型服务商，附带开箱即用的 ChatAI Web 对话应用。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **工业与物联网** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | IoT 标准库，定义物联网通信协议契约与设备抽象。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Modbus TCP/RTU/ASCII，基于 NewLife.IoT 实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | 西门子 PLC 协议，基于 NewLife.IoT 实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | 欧姆龙 PLC 协议，基于 NewLife.IoT 实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | 三菱 Melsec PLC 协议，基于 NewLife.IoT 实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | OPC 客户端，用于工业数据采集。 | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet 楼宇自控协议栈。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | 开源轻量级 IoT 平台，含网关与设备管理。 | — |
| **车联网** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 车辆定位协议：70+ 消息体，覆盖 2011/2013/2019 三个版本。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | JT/T 809 部标平台互联协议，面向运输管理部门。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 车载视频：RTP 解析、FLV/fMP4 转封装与直播推送。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **领域组件** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | 一个包覆盖 Excel、Word、PPT、PDF 等 15+ 文档格式。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | 跨平台音频：编解码、设备 IO、DSP、容器与流式处理。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | 视频编解码、封装格式、帧处理与流媒体。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 百度/高德/腾讯/天地图统一接口，支持地理编码与坐标转换。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 归属地：离线内存映射库 + 在线 API 双源。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **工具与产品** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 码神工具：网络、串口、加解密、正则、Modbus 与 MQTT。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | 自托管 Git 源代码管理系统。 | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | 从低版本宿主启动时，自动安装匹配的 .NET 运行时。 | — |

> 徽章取各项目主包；主题包、扩展包与卫星包见对应 NuGet 页面。未在表中列出的 30 多个包（FTP、LoRa、Office 扩展、各类协议驱动等）见 [NewLifeX 组织](https://github.com/NewLifeX) 与 [newlifex.com](https://newlifex.com)。

## 新生命开发团队

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

新生命团队成立于 2002 年，二十余年专注 .NET 与物联网技术生态，主导开源项目近百个（生态版图收录 34 个主干项目），为电力、物流、医疗、工控、互联网等行业提供软硬件一体化解决方案；以“学无先后达者为师”为信条。

开源组件在 NuGet 累计下载 <!--nuget-total:start-->6.48M<!--nuget-total:end--> 次（全生态口径，截至 2026-10）。

网站：https://newlifex.com · 开源组织：https://github.com/NewLifeX

<details>
<summary>微信公众号：智能大石头（扫码关注）</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>
