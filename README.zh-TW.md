# NewLife.Map - 統一地圖元件庫

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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> 本翻譯可能落後，以 [英文版](README.md) 為準。

NewLife.Map 是一套地圖元件庫，將百度、高德、騰訊、天地圖與企業地圖 API 平台統一在單一介面之後。它隱藏各家廠商的差異，提供地理編碼、逆地理編碼、距離計算、座標轉換與多 Key 熔斷能力。

原始碼：https://github.com/NewLifeX/NewLife.Map  
NuGet：`NewLife.Map`  

## 倉庫組成
| 專案 | 角色 | 說明 |
| ---- | ---- | ---- |
| `NewLife.Map` | 地圖元件庫（NuGet 套件） | 統一 `IMap` 介面 + 多廠商配接器 + 座標轉換 + 金鑰熔斷 |
| `MapApi` | 企業地圖 API 平台（Web 服務） | 多廠商聚合 + 多層級 GeoHash 快取 + Cube 管理後台 |
| `XUnitTest` | 單元測試 | 各提供者的地理編碼 / 逆地理編碼 / 距離 / 座標轉換測試（真實服務整合案例預設略過） |

## 文件
- [需求文件](Doc/需求文档.md) — 願景、核心目標、功能需求、暫緩清單
- [功能清單](Doc/功能清单.md) — 模組 / 功能編碼與實作、測試、註解三維狀態追蹤
- [架構設計](Doc/架构设计.md) — 分層、元件、關鍵流程與設計決策
- [競品分析](Doc/竞品分析.md) — 與 Geocoding.net / ProjNET / 國內單一廠商封裝的比較矩陣與差距分析

## 功能特性
- 統一 `IMap` 介面，隱藏百度 / 高德 / 騰訊 / 天地圖 / 企業自建（NewLifeMap）之間的差異；切換廠商無需修改程式碼
- 地理編碼（地址 → 座標）/ 逆地理編碼（座標 → 地址），自動合併 POI 語意資訊以補全地址組成
- 路徑距離 / 駕車時間，廠商特定的策略參數已封裝
- 行政區劃查詢（高德 / 騰訊）、POI / 區域搜尋（百度 / 天地圖）、IP 定位（百度 / 騰訊 / 平台線上解析）
- 座標系轉換：內建 WGS84 / GCJ02 / BD09 離線公式 + 廠商線上批次轉換（百度特殊的 from/to 對應已封裝）
- 多 Key 輪詢與自動熔斷 + 定時復原；單一 Key 被限流或封禁不影響整體可用性
- 透過 `MaxQps` 進行權杖桶限流，並提供請求統計（總數 / 成功率 / 平均耗時），避免單一 Key 因高頻請求被封禁
- 可透過 `ThrowException` 選擇是否拋出例外，便於上層統一處理
- 多目標框架：net45 / net461 / netstandard2.0 / netstandard2.1（相容舊專案與現代 .NET）
- 強式名稱簽署，適合反射與外掛系統
- 內建執行階段多語言：11 種語言，英文為來源與後備；例外訊息、日誌與組件中繼資料經編譯期詞表依目前語言翻譯
- 整合星塵註冊中心：`NewLifeMap` 提供者自動探索伺服器位址，在多實例 / 藍綠部署間無縫切換

## 安裝
```bash
# 穩定版
dotnet add package NewLife.Map
# 預覽版（帶日期）
dotnet add package NewLife.Map --prerelease
```

## 快速開始
```csharp
using NewLife.Map;
using NewLife.Data;

// 選擇具體的地圖提供者（BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap）
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // 多個 Key 以逗號分隔

// 1. 正向地理編碼：地址 -> 座標
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. 逆地理編碼：座標 -> 地址
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. 距離（駕車）
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. 座標轉換（離線公式）
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> 座標需使用球面座標（經度 / 緯度）；輸出為 BD09 座標系的點。

## 透過工廠動態建立
```csharp
var m1 = Map.Create("Baidu");           // 自動匹配 *Map 後綴
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// 或依列舉建立
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` 於建立實例後設定；多個金鑰以逗號分隔並在內部輪詢。

## 地圖提供者
| 提供者 | 類別 | 主要能力 | 備註 |
| ------ | ---- | -------- | ---- |
| 百度地圖 | `BaiduMap` | 地理編碼 / 逆地理編碼 / 距離 / POI 搜尋 / IP 定位 / 座標轉換（線上 + 離線） | `KeyName=ak` |
| 高德地圖 | `AMap` | 地理編碼 / 逆地理編碼 / 距離 / 行政區劃 | `KeyName=key` |
| 騰訊地圖 | `WeMap` | 地理編碼 / 逆地理編碼 / 距離 / IP 定位 / 行政區劃 | `KeyName=key` |
| 天地圖 | `TianDiTu` | 地理編碼 / 逆地理編碼 / 距離（支援途經點）/ POI 搜尋 | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | 地理編碼 / 逆地理編碼（開源、免費、免 Key、WGS84） | 無需 Key |
| NewLife 地圖平台 | `NewLifeMap` | 企業聚合 API（連接 MapApi 平台） | 透過星塵進行服務發現 |

> 若要新增提供者，請參閱下方「擴充新的地圖提供者」並遵循 AMap / BaiduMap 的模式。

## 座標系轉換
| 名稱 | 說明 | 常見廠商 |
| ---- | -------- | ---- |
| WGS84 | 國際 GPS 標準 | 裝置原始定位 |
| GCJ02 | 國家測繪局套用的偏移 | 高德 / 騰訊 / 中國多數線上地圖 |
| BD09 | 百度偏移 | 百度 |

```csharp
// 離線公式轉換，不消耗廠商配額
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// 百度線上批次轉換，from/to 索引值已封裝
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## AppKey 多 Key 輪詢與熔斷
- 傳入 `AppKey="k1,k2,k3"`；每次請求遞增索引並取餘數輪詢
- 當回應命中失效關鍵字（例如 `TOO_FREQUENT` / `LIMIT` / `INVALID`），會呼叫 `RemoveKey()` 將該 Key 暫時下線
- 下線的 Key 進入 `_pendingKeys`，由 `TimerX` 定時掃描，到期後自動復原
- 復原時間策略：目前由呼叫端傳入（例如 1 小時）

## NewLifeMap 聚合平台
`NewLifeMap` 指向內建的 `MapApi` 企業地圖 API 平台。平台聚合多家廠商並快取結果，用戶端無需感知底層差異。

```csharp
// 方式 1：直接指定伺服器
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// 方式 2：相依性注入 + 星塵註冊中心自動解析位址
services.AddSingleton<IMap, NewLifeMap>();
// NewLifeMap(IServiceProvider) 建構函式會依序嘗試：IRegistry -> StarFactory
```

- 綁定的 `ApiHttpClient` 會自動更新伺服器位址清單，在 MapApi 多副本 / 藍綠部署時無縫切換
- 平台對外提供統一的 `GET /Map/Geo`、`/Map/ReverseGeo`（逆地理編碼）、`/Map/GeoAddress`（正向地理編碼）與 `/Map/Distance`（距離）端點；四層級 GeoHash 快取（6–9 字元）降低廠商呼叫量

## 錯誤處理
- 預設：回應不成功時不拋出例外，方法回傳 `null`
- 設定 `map.ThrowException = true;` 後拋出 `Exception`，便於外層統一攔截
- 最近一次請求的除錯資訊：`LastUrl` / `LastString` / `LastResult` / `LastKey`

## 常用 API 一覽
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
擴充方法（提供者特定）：
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // BaiduMap / 天地圖 POI / 區域搜尋
Task<IList<GeoArea>> GetAreaAsync(...);       // 高德行政區劃
Task<IDictionary<string, object?>> IpLocationAsync(...); // BaiduMap / WeMap IP 定位
```

## 擴充新的地圖提供者
1. 建立類別 `XXXMap : Map, IMap`
2. 設定 `Server` / `KeyName`
3. 覆寫 `InvokeAsync<T>` 解析廠商統一的回應結構與錯誤碼
4. 實作 `GetGeoAsync` / `GetReverseGeoAsync` / 其他必要能力
5. 若有廠商特定的限流 / 失效訊號，覆寫 `IsValidKey` 回傳 true 以觸發暫時下線
6. 在 `MapFactory` 靜態建構函式中以 `Register<XXXMap>(MapKinds.XXX)` 註冊
7. 提交涵蓋主要功能與邊界條件的單元測試

## 單元測試
倉庫包含 `XUnitTest` 專案；方案為 `NewLife.Map.slnx`（VS 2026 / .NET 10 新版 XML 格式）：
- 預設執行**確定性測試**：84 個案例；本機 HTTP 樁服務涵蓋各廠商的 URL 建構與回應解析；執行約需 0.4–1 秒，在所有 RID（含 CI）上穩定執行
- 真實廠商 / OSM / 平台的線上測試標記為 `Category=Integration`，並透過 `IntegrationFact` **預設略過**；VS 測試總管的「全部執行」行為與 `dotnet test` 一致
- 啟用整合測試：先設定環境變數 `NEWLIFE_MAP_INTEGRATION=1`，再以 `--filter "Category=Integration"` 執行
- 真實金鑰請保存在本機使用者秘密或環境變數中，切勿提交至倉庫

執行（Windows PowerShell）：
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # 選用：實機環境整合
```

## 建置
```bash
dotnet build -c Release
```
輸出（多框架建置）位於 `Bin/`。

## 相容性測試（CI）
於 push / PR 時觸發，涵蓋主流 RID：
- 單元測試矩陣：win-x64 / linux-x64 / linux-arm64 / linux-la64（龍芯 LoongArch，QEMU 模擬 + 龍芯 .NET SDK，詳見 `test.yml`）
- NativeAOT 冒煙矩陣：同樣四個 RID（`AotSmoke` 專案，18 個場景），詳見 `aot.yml`

## 藍圖 / TODO
> 2026-08-04 已全部完成：並行限流（MAP-12）、故障統計（MAP-13）、基準效能比較（MAP-14）

## 版本策略
主版本 = 相容性變更；次版本 = 新功能；修訂版 = 修正 / 最佳化。`VersionSuffix` 帶有日期，便於識別建置。

## 貢獻
歡迎 issue / PR：錯誤、效能最佳化、支援更多地圖、文件與測試。提交前請先閱讀 `.github/copilot-instructions.md`，保持程式碼風格一致。

## 授權
MIT；保留著作權聲明即可自由用於商業 / 開源專案。

---

## 生態版圖

NewLife 生態系按相依分層，各專案獨立版本、獨立發行。下面的版圖在所有 NewLife 儲存庫中逐字一致；每個專案連結到程式碼儲存庫與主 NuGet 套件。

| 專案 | 一句話定位 | 下載量 |
| --- | --- | --- |
| **基礎底座** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | 日誌、設定、快取、網路、序列化、排程與 APM 追蹤。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | 資料中介層：單表百億級、自動分表、讀寫分離，支援 10 多種資料庫。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 高效能網路庫：單機 177 萬 pps、14.5 Gbps（TCP 1KB，2026-10 實測），400 萬 Tcp 長連線。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | 協定通訊與 RPC 框架，面向 CS 應用程式與物聯網裝置。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | 將 .NET 應用程式安裝為作業系統常駐服務：Windows 服務與 Linux systemd。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | `dotnet new` 專案範本：主控台、Web、API、服務、資料與物聯網。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **資料儲存與訊息** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis 用戶端：微秒級延遲、百萬級吞吐，內建訊息佇列。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 純受控 RocketMQ 用戶端，支援 Apache RocketMQ 與阿里雲訊息佇列。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT 用戶端與服務端，含阿里雲物聯網接入。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | C# 混合式資料庫：內嵌或網路雙模，關係型、時序、訊息佇列與 KV 於一體。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | 建構於 NewLife 資料堆疊之上的 MySql 驅動與輔助層。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web、AI 與分散式** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 快速後台開發框架：權限、SSO、OAuth 服務端與 CRUD 樣板。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | 分散式服務平台：節點管理、APM、設定中心、註冊中心與發佈中心。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | 分散式任務與即時資料排程平台，支援分片、重試與回溯補算。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | 統一接入 46 家大模型服務商，附帶開箱即用的 ChatAI Web 對話應用。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **工業與物聯網** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | 物聯網標準庫，定義通訊協定契約與裝置抽象。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | 建構於 NewLife.IoT 之上的 Modbus TCP/RTU/ASCII 實作。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | 建構於 NewLife.IoT 之上的西門子 PLC 協定。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | 建構於 NewLife.IoT 之上的歐姆龍 PLC 協定。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | 建構於 NewLife.IoT 之上的三菱 Melsec PLC 協定。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | 工業資料存取的 OPC 用戶端。 | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet 樓宇自動化協定堆疊。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | 開源輕量級物聯網平台，含閘道與裝置管理。 | — |
| **車聯網** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 車輛定位協定：70+ 訊息體，涵蓋 2011/2013/2019 三個版本。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | JT/T 809 部標平台互聯協定，面向運輸管理部門。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 車載視訊：RTP 解析、FLV/fMP4 轉封裝與直播推送。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **領域元件** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | 一個套件涵蓋 Excel、Word、PPT、PDF 等 15+ 文件格式。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | 跨平台音訊：編解碼、裝置 IO、DSP、容器與串流處理。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | 視訊編解碼、封裝格式、影格處理與串流。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 百度／高德／騰訊／天地圖統一介面，支援地理編碼與座標轉換。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 歸屬地：離線記憶體映射資料庫 + 線上 API 雙來源。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **工具與產品** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 開發者工具箱：網路、序列埠、加解密、正規表示式、Modbus 與 MQTT。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | 自架 Git 原始碼管理系統。 | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | 從較舊的宿主啟動時，自動安裝相符的 .NET 執行階段。 | — |

> 徽章取各專案主套件；主題套件、擴充套件與衛星套件見對應 NuGet 頁面。未在表中列出的 30 多個套件（FTP、LoRa、Office 擴充、各類協定驅動等）見 [NewLifeX 組織](https://github.com/NewLifeX) 與 [newlifex.com](https://newlifex.com)。

## 新生命開發團隊

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

新生命團隊成立於 2002 年，二十餘年專注 .NET 與物聯網技術生態，主導開源專案近百個（生態版圖收錄 34 個主幹專案），為電力、物流、醫療、工控、網際網路等行業提供軟硬體一體化解決方案；以「學無先後達者為師」為信條。

開源元件在 NuGet 累計下載 <!--nuget-total:start-->6.48M<!--nuget-total:end--> 次（全生態口徑，截至 2026-10）。

網站：https://newlifex.com · 開源組織：https://github.com/NewLifeX

<details>
<summary>微信公眾號：智能大石头（掃碼關注）</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

