# NewLife.Map - 地图组件库

![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Map?logo=github)
![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Map?logo=github)
![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Map?logo=nuget)
![Nuget](https://img.shields.io/nuget/v/NewLife.Map?logo=nuget)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Map?label=dev%20nuget&logo=nuget)

地图组件库  

源码： https://github.com/NewLifeX/NewLife.Map  
Nuget：`NewLife.Map`  

## 功能特性
- 统一接口 IMap，屏蔽百度 / 高德 / 腾讯 / 天地图 / 企业自建(NewLifeMap) 差异
- 地理编码（地址→坐标）/ 逆地理编码（坐标→地址）
- 路径距离/驾车时间（不同厂商策略参数差异已抽象）
- 行政区划查询（高德）
- POI / 语义信息融合，自动补充地址组件
- IP 定位（百度）
- 多 Key 轮询与自动熔断 + 定时恢复（内置限流/失效关键字识别）
- 坐标系互转：WGS84 / GCJ02 / BD09（内置常用转换公式）及厂商在线批量转换（百度特殊 from/to 映射）
- 可选抛出异常 `ThrowException`，便于上层统一处理
- 多目标框架：net45 / net461 / netstandard2.0 / netstandard2.1（兼容老项目与现代 .NET）
- 强命名签名，适合反射与插件体系
- Stardust 注册中心集成（`NewLifeMap` 提供者）支持服务发现与多实例地址自动刷新

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

// 选择一个具体地图提供者（BaiduMap / AMap / TencentMap / TianDiMap / NewLifeMap）
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
var m1 = Map.Create("Baidu");     // 自动匹配 *Map 后缀
var m2 = Map.Create("AMap");      // 高德
var m3 = Map.Create("NewLifeMap");
```
> `AppKey` 在实例化后设置；多个 key 以逗号分隔，内部轮询使用。

## 提供者说明
| 提供者 | 类名 | 主要能力 | 备注 |
| ------ | ---- | -------- | ---- |
| 百度地图 | `BaiduMap` | 地理/逆地理/距离/POI检索/IP定位/坐标转换(在线+离线) | `KeyName=ak` |
| 高德地图 | `AMap` | 地理/逆地理/距离/行政区划/坐标转换(离线) | `KeyName=key` |
| 腾讯地图 | `TencentMap` | (若实现) 地理/逆地理/距离 | 同模式 |
| 天地图 | `TianDiMap` | (若实现) 地理/逆地理 | |
| 新生命地图平台 | `NewLifeMap` | 企业聚合接口（后端微服务统一封装） | 通过 Stardust 服务发现 |

`TencentMap.cs` / `TianDiMap.cs` 若为空或未实现，可按 AMap/BaiduMap 模式扩展。

## NewLifeMap & 星尘集成
```csharp
// 方式1：直接指定服务端
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// 方式2：依赖注入 + Stardust Registry 自动解析地址
services.AddSingleton<IMap, NewLifeMap>();
// NewLifeMap(IServiceProvider) 构造函数中会尝试：IRegistry -> StarFactory
```
当后端 MapApi 节点列表变化时，绑定的 `ApiHttpClient` 会自动更新，可在多副本/蓝绿发布中无感切换。

## AppKey 多 Key 轮询与熔断
- 传入 `AppKey="k1,k2,k3"`
- 每次请求递增索引取余
- 若响应命中无效关键字（如：`TOO_FREQUENT`, `LIMIT`, `INVALID` 等），调用 `RemoveKey()` 将其暂时下线
- 下线 key 会放入 `_pendingKeys`，定时器到期后自动恢复
- 恢复时间策略：当前实现为调用处自行传入（例如 1 小时）

## 坐标系说明
| 名称 | 说明 | 常见厂商 |
| ---- | ---- | -------- |
| WGS84 | 国际通用 GPS | 设备原始定位 |
| GCJ02 | 国测局加偏 | 高德 / 腾讯 / 绝大多数国内在线地图 |
| BD09 | 百度加偏 | 百度 |

调用示例：
```csharp
var rs = await map.ConvertAsync(new [] { new GeoPoint(lng, lat) }, from:"wgs84", to:"gcj02");
```
百度在线转换需要 from/to 索引值，本库已封装。

## 常用 API 一览
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
扩展（BaiduMap / AMap 特有）：
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // BaiduMap POI / 区域检索
Task<IList<GeoArea>> GetAreaAsync(...);       // AMap 行政区划
Task<IDictionary<string, object?>> IpLocationAsync(...); // BaiduMap IP 定位
```

## 错误处理
- 默认：接口返回非成功状态不会抛异常，方法返回 null
- 设置 `map.ThrowException = true;` 后抛出 `Exception`，便于外层统一捕获
- 最近一次请求调试信息：`LastUrl` / `LastString` / `LastResult` / `LastKey`

## 扩展一个新地图提供者步骤
1. 新建类 `XXXMap : Map, IMap`
2. 设置 `Server` / `KeyName`
3. 重写 `InvokeAsync<T>` 解析该厂商统一返回结构 & 错误码
4. 实现 `GetGeoAsync` / `GetReverseGeoAsync` / 其它所需能力
5. 若有专属限流/失效特征，重写 `IsValidKey` 返回 true 触发临时下线
6. 提交单元测试覆盖主要功能与边界

## 单元测试
仓库包含 `XUnitTest` 工程：
- `*MapTests`：针对不同提供者的编码/逆编码测试
- 请将真实 Key 放入本地用户机密或环境变量，不要提交到仓库

运行：
```bash
dotnet test -c Release
```

## 构建
```bash
dotnet build -c Release
```
输出（多框架编译）位于 `Bin/`。

## Roadmap / TODO
- [ ] 补全 `TencentMap` / `TianDiMap` 具体实现（若仍为空）
- [ ] 为 `NewLifeMap` 聚合平台补充正向地理编码与距离计算接口
- [ ] 增加并发限速（令牌桶）可选适配，避免密钥整体封禁
- [ ] 添加 Benchmark 基准对比（批量坐标转换）
- [ ] 丰富故障统计 / Metrics（成功率、平均耗时、Key 命中率）

## 版本策略
主版本 = 兼容性变更；次版本 = 新功能；修订号 = 修复 / 优化。`VersionSuffix` 带日期，便于快速定位构建。

## 贡献
欢迎 Issue / PR：Bug、性能优化、支持更多地图、补充文档与测试。提交前请阅读 `.github/copilot-instructions.md` 以保持代码风格一致。

## 许可证
MIT，保留版权声明即可自由使用于商业 / 开源项目。

---

## NewLifeMap
地图提供者NewLifeMap指向内置MapApi地图接口平台，屏蔽各家接口差异，支持自定义修改。  
NewLifeMap 可明文制定服务端地址，也可以借助依赖注入自动从星尘注册中心获取。  

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
