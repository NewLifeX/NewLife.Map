# NewLife.Map - Unified Map Component Library

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
🌐 [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

A map component library that unifies Baidu, AMap, Tencent, Tianditu and the enterprise map API platform behind a single interface. It hides vendor differences and provides geocoding, reverse geocoding, distance calculation, coordinate conversion and multi-key circuit breaking.  

Source: https://github.com/NewLifeX/NewLife.Map  
NuGet: `NewLife.Map`  

## Repository layout
| Project | Role | Description |
| ---- | ---- | ---- |
| `NewLife.Map` | Map component library (NuGet package) | Unified `IMap` interface + multi-vendor adapters + coordinate conversion + key circuit breaking |
| `MapApi` | Enterprise map API platform (web service) | Multi-vendor aggregation + multi-level GeoHash cache + Cube admin backend |
| `XUnitTest` | Unit tests | Geocoding / reverse geocoding / distance / coordinate conversion tests for each provider (real-service integration cases are skipped by default) |

## Documentation
- [Requirements](Doc/需求文档.md) — vision, core goals, functional requirements, deferred list
- [Feature list](Doc/功能清单.md) — module/feature codes with three-dimensional status tracking (implementation/test/comments)
- [Architecture](Doc/架构设计.md) — layering, components, key flows and design decisions
- [Competitive analysis](Doc/竞品分析.md) — comparison matrix and gap analysis versus Geocoding.net / ProjNET / domestic single-vendor wrappers

## Features
- Unified `IMap` interface that hides the differences between Baidu / AMap / Tencent / Tianditu / enterprise self-hosted (NewLifeMap); switching vendors requires zero code changes
- Geocoding (address → coordinates) / reverse geocoding (coordinates → address), automatically merging POI semantic information to complete address components
- Route distance / driving time, with vendor-specific strategy parameters abstracted away
- Administrative division queries (AMap / Tencent), POI / region search (Baidu / Tianditu), IP location (Baidu / Tencent / platform online resolution)
- Coordinate system conversion: built-in offline formulas for WGS84 / GCJ02 / BD09 + vendor online batch conversion (Baidu's special from/to mapping is encapsulated)
- Multi-key rotation with automatic circuit breaking + scheduled recovery; a single key being rate-limited or banned does not affect overall availability
- Token-bucket rate limiting via `MaxQps` and request statistics (total / success rate / average latency), preventing a single key from being banned by high-frequency requests
- Optional exception throwing via `ThrowException`, making unified handling easier for upper layers
- Multiple target frameworks: net45 / net461 / netstandard2.0 / netstandard2.1 (compatible with legacy projects and modern .NET)
- Strong-name signed, suitable for reflection and plugin systems
- Built-in runtime localization: 11 languages with English as the source and fallback; exception messages, logs and assembly metadata are translated through compile-time locale tables
- Stardust registry integration: the `NewLifeMap` provider discovers server addresses automatically, enabling seamless switching across multiple instances / blue-green deployments

## Installation
```bash
# Stable
dotnet add package NewLife.Map
# Preview (date-based)
dotnet add package NewLife.Map --prerelease
```

## Quick start
```csharp
using NewLife.Map;
using NewLife.Data;

// Choose a concrete map provider (BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap)
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // Separate multiple keys with commas

// 1. Forward geocoding: address -> coordinates
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. Reverse geocoding: coordinates -> address
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. Distance (driving)
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. Coordinate conversion (offline formulas)
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> Coordinates must use spherical coordinates (longitude/latitude); the output is a point in the BD09 coordinate system.

## Create dynamically through the factory
```csharp
var m1 = Map.Create("Baidu");           // Automatically matches the *Map suffix
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// Or create by enum
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` is set after instantiation; multiple keys are separated by commas and rotated internally.

## Map providers
| Provider | Class | Main capabilities | Notes |
| ------ | ---- | -------- | ---- |
| Baidu Maps | `BaiduMap` | Geocoding / reverse geocoding / distance / POI search / IP location / coordinate conversion (online + offline) | `KeyName=ak` |
| AMap | `AMap` | Geocoding / reverse geocoding / distance / administrative divisions | `KeyName=key` |
| Tencent Maps | `WeMap` | Geocoding / reverse geocoding / distance / IP location / administrative divisions | `KeyName=key` |
| Tianditu | `TianDiTu` | Geocoding / reverse geocoding / distance (with waypoints) / POI search | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | Geocoding / reverse geocoding (open source, free, no key, WGS84) | No key required |
| NewLife Map Platform | `NewLifeMap` | Enterprise aggregation API (connects to the MapApi platform) | Service discovery via Stardust |

> To add a provider, see "Steps to extend a new map provider" below and follow the AMap/BaiduMap pattern.

## Coordinate system conversion
| Name | Description | Common vendors |
| ---- | -------- | ---- |
| WGS84 | International GPS standard | Raw device positioning |
| GCJ02 | Offset applied by the State Bureau of Surveying and Mapping | AMap / Tencent / most Chinese online maps |
| BD09 | Baidu offset | Baidu |

```csharp
// Offline formula conversion, no vendor quota consumed
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// Baidu online batch conversion, from/to index values already encapsulated
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## AppKey multi-key rotation and circuit breaking
- Pass `AppKey="k1,k2,k3"`; each request increments an index and takes the remainder for rotation
- When a response matches invalid keywords (such as `TOO_FREQUENT` / `LIMIT` / `INVALID`), `RemoveKey()` is called to take that key offline temporarily
- Offline keys enter `_pendingKeys` and are scanned by a `TimerX` timer, then restored automatically when due
- Recovery time policy: currently passed in by the caller (for example, 1 hour)

## NewLifeMap aggregation platform
`NewLifeMap` points to the built-in `MapApi` enterprise map API platform. The platform aggregates multiple vendors and caches results, so clients do not need to be aware of the underlying differences.

```csharp
// Option 1: specify the server directly
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// Option 2: dependency injection + Stardust Registry automatic address resolution
services.AddSingleton<IMap, NewLifeMap>();
// The NewLifeMap(IServiceProvider) constructor tries: IRegistry -> StarFactory
```

- The bound `ApiHttpClient` automatically updates the server address list, enabling seamless switching during MapApi multi-replica / blue-green deployments
- The platform exposes unified `GET /Map/Geo`, `/Map/ReverseGeo` (reverse geocoding), `/Map/GeoAddress` (forward geocoding) and `/Map/Distance` (distance) endpoints; a four-level GeoHash cache (6–9 characters) reduces vendor calls

## Error handling
- Default: a non-success response does not throw; the method returns `null`
- With `map.ThrowException = true;`, an `Exception` is thrown for unified capture in the outer layer
- Debug information for the most recent request: `LastUrl` / `LastString` / `LastResult` / `LastKey`

## API overview
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
Extensions (provider-specific):
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // BaiduMap / TianDiTu POI / region search
Task<IList<GeoArea>> GetAreaAsync(...);       // AMap administrative divisions
Task<IDictionary<string, object?>> IpLocationAsync(...); // BaiduMap / WeMap IP location
```

## Extending a new map provider
1. Create a class `XXXMap : Map, IMap`
2. Set `Server` / `KeyName`
3. Override `InvokeAsync<T>` to parse the vendor's unified response structure & error codes
4. Implement `GetGeoAsync` / `GetReverseGeoAsync` / other required capabilities
5. If there are vendor-specific rate-limiting / invalidation signals, override `IsValidKey` to return true and trigger temporary offline
6. Register in the `MapFactory` static constructor with `Register<XXXMap>(MapKinds.XXX)`
7. Submit unit tests covering the main functionality and edge cases

## Unit tests
The repository contains the `XUnitTest` project; the solution is `NewLife.Map.slnx` (VS 2026 / .NET 10 new XML format):
- By default it runs **deterministic tests**: 84 cases; a local HTTP stub service covers URL construction and response parsing for each vendor; execution takes about 0.4–1 second and runs reliably on all RIDs (including CI)
- Real vendor / OSM / platform online tests are marked `Category=Integration` and **skipped by default** via `IntegrationFact`; "Run All" in the VS Test Explorer behaves the same as `dotnet test`
- To enable integration tests: first set the environment variable `NEWLIFE_MAP_INTEGRATION=1`, then run with `--filter "Category=Integration"`
- Keep real keys in local user secrets or environment variables; never commit them to the repository

Run (Windows PowerShell):
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # Optional: live-environment integration
```

## Build
```bash
dotnet build -c Release
```
The output (multi-framework build) is located in `Bin/`.

## Compatibility testing (CI)
Triggered on push / PR, covering mainstream RIDs:
- Unit test matrix: win-x64 / linux-x64 / linux-arm64 / linux-la64 (LoongArch, QEMU emulation + Loongson .NET SDK, see `test.yml`)
- NativeAOT smoke matrix: the same four RIDs (`AotSmoke` project, 17 scenarios), see `aot.yml`

## Roadmap / TODO
> All completed on 2026-08-04: concurrent rate limiting (MAP-12), failure metrics (MAP-13), benchmark comparison (MAP-14)

## Versioning
Major version = compatibility changes; minor version = new features; revision = fixes / optimizations. `VersionSuffix` carries a date for easy build identification.

## Contributing
Issues / PRs are welcome: bugs, performance optimizations, support for more maps, documentation and tests. Before submitting, please read `.github/copilot-instructions.md` to keep the code style consistent.

## License
MIT; keep the copyright notice and you are free to use it in commercial / open-source projects.

---

## Ecosystem Map

NewLife is a layered ecosystem of independently versioned open-source projects. The map below is shared verbatim by every NewLife repository; each project links to its repository and its primary NuGet package.

| Project | What it does | Downloads |
| --- | --- | --- |
| **Foundation** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Logging, configuration, caching, networking, serialization, scheduling and APM tracing. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Data middleware: single tables at the tens-of-billions scale, automatic sharding, read-write splitting, 10+ databases. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | High-performance network library: 1.77M pps / 14.5 Gbps on a single machine (TCP 1KB, 2026-10), 4M long-lived TCP connections. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Protocol communication and RPC framework for client/server applications and IoT devices. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Installs .NET applications as OS daemons: Windows services and Linux systemd. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | `dotnet new` project templates for console, web, API, service, data and IoT projects. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Data & Messaging** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis client with microsecond latency, millions of ops/s and built-in message queues. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Pure managed RocketMQ client for Apache RocketMQ and Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT client and server, including Alibaba Cloud IoT connectivity. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Hybrid database in C#: embedded or networked, relational, time-series, message queue and KV in one. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | MySql driver and helper layer built on the NewLife data stack. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, AI & Distributed** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Rapid admin-platform framework: permissions, SSO, OAuth server and CRUD scaffolding. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Distributed service platform: node management, APM, configuration center, registry and release center. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Distributed task and real-time data scheduling platform with sharding, retries and backfill. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Unified client for 46 LLM providers plus ChatAI, a ready-to-run web chat application. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industrial & IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | IoT standard library defining protocol contracts and device abstractions. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Modbus TCP/RTU/ASCII implementation on top of NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Siemens PLC protocol on top of NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Omron PLC protocol on top of NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Mitsubishi Melsec PLC protocol on top of NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | OPC client for industrial data access. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet building-automation protocol stack. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Lightweight open-source IoT platform with gateway and device management. | — |
| **Vehicle Telematics** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 vehicle positioning protocol: 70+ message bodies, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | JT/T 809 platform-to-platform protocol for transport authorities. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 in-vehicle video: RTP parsing, FLV/fMP4 remuxing and live streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Domain Components** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | One package for Excel, Word, PPT, PDF and 15+ document formats. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Cross-platform audio: codecs, device I/O, DSP, containers and streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Video codecs, container formats, frame processing and streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Unified interface over Baidu, AMap, Tencent and Tianditu with geocoding and coordinate conversion. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 geolocation with an offline memory-mapped database and an online API fallback. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Tools & Products** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Developer toolbox: network, serial port, crypto, regex, Modbus and MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Self-hosted Git source code management system. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Installs the matching .NET runtime automatically when launched from an older host. | — |

> Badges show the primary package of each project; theme, extension and satellite packages are listed on their NuGet pages.The 30+ packages not listed here (FTP, LoRa, Office extensions, protocol drivers and more) are available under the [NewLifeX organization](https://github.com/NewLifeX) and on [newlifex.com](https://newlifex.com).

## Team

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

Founded in 2002, the NewLife team has spent more than two decades on the .NET and IoT ecosystem, maintains nearly 100 open-source projects (34 of them core to this map), and delivers integrated software and hardware solutions for power, logistics, healthcare, industrial control and internet industries. We live by the motto "learning has no seniority — mastery earns respect".

Open-source components have been downloaded <!--nuget-total:start-->6.48M<!--nuget-total:end--> times from NuGet (ecosystem-wide, as of 2026-10).

Website: https://newlifex.com · Open source organization: https://github.com/NewLifeX

<details>
<summary>WeChat Official Account: 智能大石头 (scan to follow)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>