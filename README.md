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

## NewLife Project Matrix
Each project supports net9.0/netstandard2.1/netstandard2.0/net4.62/net4.5 by default; older versions (2024.0801) support net4.0/net2.0  

| Project | Year | Description |
| :--------------------------------------------------------------: | :---: | ------------------------------------------------------------------------------------------- |
| Basic components | | Supports other middleware and product projects |
| [NewLife.Core](https://github.com/NewLifeX/X) | 2002 | Core library: logging, configuration, caching, networking, serialization, APM performance tracing |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | 2005 | Big-data middleware; single table at 10-billion scale; MySQL/SQLite/SqlServer/Oracle/PostgreSql/DaMeng; automatic sharding; read-write separation |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 2005 | Network library; 10M-level throughput per node (22.66M tps); 1M-level connections per node (4M TCP long connections) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | 2011 | Protocol communication library: C/S application communication framework, HTTP/RPC support, high throughput, low overhead and easy access for IoT devices |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 2010 | Cube rapid development platform with user permissions, SSO login, OAuth server, etc.; verified on 10-billion-scale single-table projects |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | 2008 | Service management component; installs applications as OS daemons — Windows services, Linux systemd |
| [NewLife.Zero](https://github.com/NewLifeX/NewLife.Zero) | 2020 | Zero-generation scaffolding; project templates NewLife.Templates based on the NewLife component ecosystem: Web, WebApi, Service |
| Middleware | | Integration with well-known middleware platforms |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | 2017 | Redis client; microsecond latency, million-level throughput, rich message queues; verified on 10-billion-scale projects |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 2018 | Pure managed RocketMQ client supporting Apache RocketMQ and Alibaba Cloud Message Queue; verified on billion-scale projects |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | 2019 | IoT messaging protocol, MqttClient/MqttServer; the client supports Alibaba Cloud IoT |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | 2022 | IoT standard library defining communication protocol specifications for the IoT domain |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | 2022 | ModbusTcp/ModbusRTU/ModbusASCII based on the IoT standard library; supports ZeroIoT platform and IoTEdge gateway |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | 2022 | Siemens PLC protocol based on the IoT standard library; supports IoT platform and IoTEdge |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 2022 | Map component library wrapping Baidu Maps, AMap, Tencent Maps and Tianditu |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | 2023 | Audio codec library: PCM/ADPCMA/G711A/G722U/WAV/AAC |
| Product platforms | | Product-platform level: ready to compile and deploy, customizable |
| [Stardust](https://github.com/NewLifeX/Stardust) | 2018 | Stardust distributed service platform: node management, APM monitoring center, configuration center, registry, release center |
| [AntJob](https://github.com/NewLifeX/AntJob) | 2019 | Distributed big-data computing platform (real-time/offline), ant-moving sharding concept; verified on trillion-scale projects |
| [NewLife.ERP](https://github.com/NewLifeX/NewLife.ERP) | 2021 | Enterprise ERP: product management, customer management, sales management, supplier management |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 2006 | Developer toolbox: networking, serial ports, encryption/decryption, regular expressions, Modbus, MQTT |
| [EasyIO](https://github.com/NewLifeX/EasyIO) | 2023 | Simple file storage, supporting centralized file storage in distributed systems |
| [XProxy](https://github.com/NewLifeX/XProxy) | 2005 | Product-grade reverse proxy: NAT proxy, HTTP proxy |
| [HttpMeter](https://github.com/NewLifeX/HttpMeter) | 2022 | HTTP stress testing tool |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | 2015 | Git source code management system |
| [SmartOS](https://github.com/NewLifeX/SmartOS) | 2014 | Fully independent embedded operating system supporting ARM Cortex-M chips |
| [SmartA2](https://github.com/NewLifeX/SmartA2) | 2019 | Embedded industrial computer, IoT edge gateway, high-performance .NET8 host for industry, agriculture, transportation and healthcare |
| FIoT IoT Platform | 2020 | End-to-end IoT solution for buildings, environmental protection and agriculture; integrated software, hardware and big-data analytics; verified on 100k-point single-node projects |
| UWB High-Precision Indoor Positioning | 2020 | Centimeter-level (10–20 cm) high-precision indoor positioning; hardware/software integration and interoperability with other systems; verified in large exhibition hall projects |

## NewLife Development Team
![XCode](https://newlifex.com/logo.png)  

NewLife was founded in 2002 and is a provider of IoT-era solutions, offering consulting on software/hardware applications, system architecture planning and development services.  
The team's 80+ open-source projects are widely used across industries, with cumulative NuGet downloads exceeding 4 million.  
The team's big-data middleware NewLife.XCode, AntJob scheduling and computing platform, Stardust distributed platform, NewLife.Redis cache/queue component and FIoT IoT platform have all been successfully applied in power, education, internet, telecom, transportation, logistics, industrial control, healthcare, museums and other industries, providing customers with large numbers of advanced, reliable, secure, high-quality and easily extensible products and system integration services.  

We will keep improving our services to become a long-term trusted partner of our customers, and through continuous innovation and development, become an outstanding IoT service provider in China.  

`The NewLife team started in 2002; some open-source projects have a history of more than 20 years, and the source repositories retain all modification records since 2010`  
Website: https://newlifex.com  
Open source: https://github.com/newlifex  
QQ groups: 1600800/1600838  
WeChat official account:  
![智能大石头](https://newlifex.com/stone.jpg)  