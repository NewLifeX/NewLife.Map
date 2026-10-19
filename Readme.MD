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
🌐 [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

统一封装百度、高德、腾讯、天地图与企业地图接口平台的地图组件库，以一套接口屏蔽各家差异，提供地理编码、逆地理编码、距离计算、坐标转换与多 Key 熔断能力。  

源码： https://github.com/NewLifeX/NewLife.Map  
Nuget：`NewLife.Map`  

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
- NativeAOT 冒烟矩阵：同上四个 RID（`AotSmoke` 工程，17 个场景），见 `aot.yml`

## Roadmap / TODO
> 2026-08-04 已全部完成：并发限速（MAP-12）、故障统计 Metrics（MAP-13）、Benchmark 基准对比（MAP-14）

## 版本策略
主版本 = 兼容性变更；次版本 = 新功能；修订号 = 修复 / 优化。`VersionSuffix` 带日期，便于快速定位构建。

## 贡献
欢迎 Issue / PR：Bug、性能优化、支持更多地图、补充文档与测试。提交前请阅读 `.github/copilot-instructions.md` 以保持代码风格一致。

## 许可证
MIT，保留版权声明即可自由使用于商业 / 开源项目。

---

## 新生命项目矩阵
各项目默认支持net9.0/netstandard2.1/netstandard2.0/net4.62/net4.5，旧版（2024.0801）支持net4.0/net2.0  

|                               项目                               | 年份  | 说明                                                                                        |
| :--------------------------------------------------------------: | :---: | ------------------------------------------------------------------------------------------- |
|                             基础组件                             |       | 支撑其它中间件以及产品项目                                                                  |
|          [NewLife.Core](https://github.com/NewLifeX/X)           | 2002  | 核心库，日志、配置、缓存、网络、序列化、APM性能追踪                                         |
|    [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode)    | 2005  | 大数据中间件，单表百亿级，MySql/SQLite/SqlServer/Oracle/PostgreSql/达梦，自动分表，读写分离 |
|      [NewLife.Net](https://github.com/NewLifeX/NewLife.Net)      | 2005  | 网络库，单机千万级吞吐率（2266万tps），单机百万级连接（400万Tcp长连接）                     |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | 2011  | 协议通信库，提供CS应用通信框架，支持Http/RPC通信框架，高吞吐，物联网设备低开销易接入        |
|     [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube)     | 2010  | 魔方快速开发平台，集成了用户权限、SSO登录、OAuth服务端等，单表100亿级项目验证               |
|    [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent)    | 2008  | 服务管理组件，把应用安装成为操作系统守护进程，Windows服务、Linux的Systemd                   |
|     [NewLife.Zero](https://github.com/NewLifeX/NewLife.Zero)     | 2020  | Zero零代脚手架，基于NewLife组件生态的项目模板NewLife.Templates，Web、WebApi、Service        |
|                              中间件                              |       | 对接知名中间件平台                                                                          |
|    [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis)    | 2017  | Redis客户端，微秒级延迟，百万级吞吐，丰富的消息队列，百亿级数据量项目验证                   |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 2018  | RocketMQ纯托管客户端，支持Apache RocketMQ和阿里云消息队列，十亿级项目验                     |
|     [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT)     | 2019  | 物联网消息协议，MqttClient/MqttServer，客户端支持阿里云物联网                               |
|      [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT)      | 2022  | IoT标准库，定义物联网领域的各种通信协议标准规范                                             |
|   [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus)   | 2022  | ModbusTcp/ModbusRTU/ModbusASCII，基于IoT标准库实现，支持ZeroIoT平台和IoTEdge网关            |
|  [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens)  | 2022  | 西门子PLC协议，基于IoT标准库实现，支持IoT平台和IoTEdge                                      |
|      [NewLife.Map](https://github.com/NewLifeX/NewLife.Map)      | 2022  | 地图组件库，封装百度地图、高德地图、腾讯地图、天地图                                        |
|    [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio)    | 2023  | 音频编解码库，PCM/ADPCMA/G711A/G722U/WAV/AAC                                                |
|                             产品平台                             |       | 产品平台级，编译部署即用，个性化自定义                                                      |
|         [Stardust](https://github.com/NewLifeX/Stardust)         | 2018  | 星尘，分布式服务平台，节点管理、APM监控中心、配置中心、注册中心、发布中心                   |
|           [AntJob](https://github.com/NewLifeX/AntJob)           | 2019  | 蚂蚁调度，分布式大数据计算平台（实时/离线），蚂蚁搬家分片思想，万亿级数据量项目验证         |
|      [NewLife.ERP](https://github.com/NewLifeX/NewLife.ERP)      | 2021  | 企业ERP，产品管理、客户管理、销售管理、供应商管理                                           |
|         [CrazyCoder](https://github.com/NewLifeX/XCoder)         | 2006  | 码神工具，众多开发者工具，网络、串口、加解密、正则表达式、Modbus、MQTT                      |
|           [EasyIO](https://github.com/NewLifeX/EasyIO)           | 2023  | 简易文件存储，支持分布式系统中文件集中存储。                                                |
|           [XProxy](https://github.com/NewLifeX/XProxy)           | 2005  | 产品级反向代理，NAT代理、Http代理                                                           |
|        [HttpMeter](https://github.com/NewLifeX/HttpMeter)        | 2022  | Http压力测试工具                                                                            |
|         [GitCandy](https://github.com/NewLifeX/GitCandy)         | 2015  | Git源代码管理系统                                                                           |
|          [SmartOS](https://github.com/NewLifeX/SmartOS)          | 2014  | 嵌入式操作系统，完全独立自主，支持ARM Cortex-M芯片架构                                      |
|          [SmartA2](https://github.com/NewLifeX/SmartA2)          | 2019  | 嵌入式工业计算机，物联网边缘网关，高性能.NET8主机，应用于工业、农业、交通、医疗             |
|                          FIoT物联网平台                          | 2020  | 物联网整体解决方案，建筑、环保、农业，软硬件及大数据分析一体化，单机十万级点位项目验证      |
|                        UWB高精度室内定位                         | 2020  | 厘米级（10~20cm）高精度室内定位，软硬件一体化，与其它系统联动，大型展厅项目验证             |

## 新生命开发团队
![XCode](https://newlifex.com/logo.png)  

新生命团队（NewLife）成立于2002年，是新时代物联网行业解决方案提供者，致力于提供软硬件应用方案咨询、系统架构规划与开发服务。  
团队主导的80多个开源项目已被广泛应用于各行业，Nuget累计下载量高达400余万次。  
团队开发的大数据中间件NewLife.XCode、蚂蚁调度计算平台AntJob、星尘分布式平台Stardust、缓存队列组件NewLife.Redis以及物联网平台FIoT，均成功应用于电力、高校、互联网、电信、交通、物流、工控、医疗、文博等行业，为客户提供了大量先进、可靠、安全、高质量、易扩展的产品和系统集成服务。  

我们将不断通过服务的持续改进，成为客户长期信赖的合作伙伴，通过不断的创新和发展，成为国内优秀的IoT服务供应商。  

`新生命团队始于2002年，部分开源项目具有20年以上漫长历史，源码库保留有2010年以来所有修改记录`  
网站：https://newlifex.com  
开源：https://github.com/newlifex  
QQ群：1600800/1600838  
微信公众号：  
![智能大石头](https://newlifex.com/stone.jpg)  
