# NewLife.Map - 統合マップコンポーネントライブラリ

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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> この翻訳は[英語版](README.md)より遅れている場合があります。英語版を正とします。

NewLife.Map は、Baidu・AMap・Tencent・天地図・企業向け地図 API プラットフォームを単一のインターフェースに統合した地図コンポーネントライブラリです。ベンダー間の差異を隠蔽し、ジオコーディング、逆ジオコーディング、距離計算、座標変換、マルチキーサーキットブレーカーを提供します。

ソース: https://github.com/NewLifeX/NewLife.Map  
NuGet: `NewLife.Map`  

## リポジトリ構成
| プロジェクト | 役割 | 説明 |
| ---- | ---- | ---- |
| `NewLife.Map` | 地図コンポーネントライブラリ（NuGet パッケージ） | 統一 `IMap` インターフェース + マルチベンダーアダプター + 座標変換 + キーサーキットブレーカー |
| `MapApi` | 企業向け地図 API プラットフォーム（Web サービス） | マルチベンダー集約 + 多段 GeoHash キャッシュ + Cube 管理バックエンド |
| `XUnitTest` | ユニットテスト | 各プロバイダーのジオコーディング／逆ジオコーディング／距離／座標変換テスト（実サービス連携ケースは既定でスキップ） |

## ドキュメント
- [要件](Doc/需求文档.md) — ビジョン、中核目標、機能要件、保留リスト
- [機能一覧](Doc/功能清单.md) — モジュール／機能コードと、実装・テスト・コメントの三次元状態管理
- [アーキテクチャ](Doc/架构设计.md) — レイヤリング、コンポーネント、主要フロー、設計判断
- [競合分析](Doc/竞品分析.md) — Geocoding.net / ProjNET / 国内単一ベンダーラッパーとの比較マトリクスとギャップ分析

## 機能
- Baidu / AMap / Tencent / 天地図 / 企業セルフホスト（NewLifeMap）の差異を隠蔽する統一 `IMap` インターフェース。ベンダーの切り替えにコード変更は不要
- ジオコーディング（住所 → 座標）／逆ジオコーディング（座標 → 住所）。POI のセマンティック情報を自動統合して住所構成要素を補完
- 経路距離／運転時間。ベンダー固有の戦略パラメーターは抽象化済み
- 行政区画の照会（AMap / Tencent）、POI／地域検索（Baidu / 天地図）、IP 位置情報（Baidu / Tencent / プラットフォームのオンライン解決）
- 座標変換：WGS84 / GCJ02 / BD09 のオフライン公式を内蔵 + ベンダーのオンライン一括変換（Baidu 固有の from/to マッピングはカプセル化済み）
- マルチキー輪番と自動サーキットブレーカー + 定時復旧。単一キーがレート制限や禁止を受けても全体の可用性に影響しない
- `MaxQps` によるトークンバケット式レート制限とリクエスト統計（合計／成功率／平均遅延）。単一キーが高頻度リクエストで禁止されるのを防止
- `ThrowException` で例外送出を切り替え可能。上位層での統合処理が容易
- 複数ターゲットフレームワーク：net45 / net461 / netstandard2.0 / netstandard2.1（レガシープロジェクトと最新 .NET の両方に対応）
- 厳密名署名済み。リフレクションやプラグインシステムに適する
- Stardust レジストリ連携：`NewLifeMap` プロバイダーがサーバーアドレスを自動検出し、マルチインスタンス／ブルーグリーン配備でもシームレスに切り替え

## インストール
```bash
# 安定版
dotnet add package NewLife.Map
# プレビュー版（日付ベース）
dotnet add package NewLife.Map --prerelease
```

## クイックスタート
```csharp
using NewLife.Map;
using NewLife.Data;

// 具体的な地図プロバイダーを選択（BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap）
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // 複数のキーはカンマで区切る

// 1. ジオコーディング：住所 -> 座標
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. 逆ジオコーディング：座標 -> 住所
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. 距離（運転）
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. 座標変換（オフライン公式）
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> 座標は球面座標（経度／緯度）を使用します。出力は BD09 座標系の点です。

## ファクトリによる動的生成
```csharp
var m1 = Map.Create("Baidu");           // *Map サフィックスを自動的に照合
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// 列挙型で生成することも可能
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` はインスタンス化後に設定します。複数のキーはカンマで区切り、内部で輪番されます。

## 地図プロバイダー
| プロバイダー | クラス | 主な機能 | 備考 |
| ------ | ---- | -------- | ---- |
| Baidu 地図 | `BaiduMap` | ジオコーディング／逆ジオコーディング／距離／POI 検索／IP 位置情報／座標変換（オンライン + オフライン） | `KeyName=ak` |
| AMap（高徳地図） | `AMap` | ジオコーディング／逆ジオコーディング／距離／行政区画 | `KeyName=key` |
| Tencent 地図 | `WeMap` | ジオコーディング／逆ジオコーディング／距離／IP 位置情報／行政区画 | `KeyName=key` |
| 天地図 | `TianDiTu` | ジオコーディング／逆ジオコーディング／距離（経由地対応）／POI 検索 | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | ジオコーディング／逆ジオコーディング（オープンソース、無料、キー不要、WGS84） | キー不要 |
| NewLife 地図プラットフォーム | `NewLifeMap` | 企業向け集約 API（MapApi プラットフォームに接続） | Stardust によるサービスディスカバリ |

> プロバイダーを追加する場合は、後述の「新しい地図プロバイダーの拡張」と AMap／BaiduMap の実装パターンを参照してください。

## 座標系変換
| 名称 | 説明 | 主なベンダー |
| ---- | -------- | ---- |
| WGS84 | 国際的な GPS 標準 | デバイスの生測位 |
| GCJ02 | 国家測絵局によるオフセット | AMap / Tencent / 中国国内の大半のオンライン地図 |
| BD09 | Baidu のオフセット | Baidu |

```csharp
// オフライン公式による変換。ベンダーのクォータを消費しない
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// Baidu のオンライン一括変換。from/to のインデックス値はカプセル化済み
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## AppKey のマルチキー輪番とサーキットブレーカー
- `AppKey="k1,k2,k3"` を渡すと、リクエストごとにインデックスを加算して剰余で輪番します
- 応答が無効キーワード（`TOO_FREQUENT` / `LIMIT` / `INVALID` など）に一致すると、`RemoveKey()` を呼び出してそのキーを一時的に無効化します
- 無効化されたキーは `_pendingKeys` に入り、`TimerX` タイマーが走査して期限到来時に自動復旧します
- 復旧時間の方針：現在は呼び出し側が指定します（例：1 時間）

## NewLifeMap 集約プラットフォーム
`NewLifeMap` は内蔵の `MapApi` 企業向け地図 API プラットフォームを指します。プラットフォーム側で複数ベンダーを集約し結果をキャッシュするため、クライアントは下層の差異を意識する必要がありません。

```csharp
// 方法 1：サーバーを直接指定
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// 方法 2：依存性注入 + Stardust レジストリによるアドレス自動解決
services.AddSingleton<IMap, NewLifeMap>();
// NewLifeMap(IServiceProvider) コンストラクターは IRegistry -> StarFactory の順に試行します
```

- バインドされた `ApiHttpClient` はサーバーアドレス一覧を自動更新するため、MapApi のマルチレプリカ／ブルーグリーン配備でもシームレスに切り替えられます
- プラットフォームは `GET /Map/Geo`、`/Map/ReverseGeo`（逆ジオコーディング）、`/Map/GeoAddress`（ジオコーディング）、`/Map/Distance`（距離）の統一エンドポイントを公開し、6〜9 文字の 4 段階 GeoHash キャッシュでベンダー呼び出しを削減します

## エラー処理
- 既定：失敗応答では例外を送出せず、メソッドは `null` を返します
- `map.ThrowException = true;` を設定すると `Exception` を送出し、上位層で統一的に捕捉できます
- 直近リクエストのデバッグ情報：`LastUrl` / `LastString` / `LastResult` / `LastKey`

## API 一覧
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
拡張メソッド（プロバイダー固有）：
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // BaiduMap / 天地図の POI／地域検索
Task<IList<GeoArea>> GetAreaAsync(...);       // AMap の行政区画
Task<IDictionary<string, object?>> IpLocationAsync(...); // BaiduMap / WeMap の IP 位置情報
```

## 新しい地図プロバイダーの拡張
1. `XXXMap : Map, IMap` クラスを作成する
2. `Server` / `KeyName` を設定する
3. `InvokeAsync<T>` をオーバーライドし、ベンダー統一のレスポンス構造とエラーコードを解析する
4. `GetGeoAsync` / `GetReverseGeoAsync` など必要な機能を実装する
5. ベンダー固有のレート制限／無効化シグナルがある場合は `IsValidKey` をオーバーライドし、true を返して一時無効化をトリガーする
6. `MapFactory` の静的コンストラクターで `Register<XXXMap>(MapKinds.XXX)` を呼び出して登録する
7. 主要機能とエッジケースをカバーするユニットテストを提出する

## ユニットテスト
リポジトリには `XUnitTest` プロジェクトが含まれます。ソリューションは `NewLife.Map.slnx`（VS 2026 / .NET 10 の新しい XML 形式）です：
- 既定では**決定的テスト**を実行します：84 ケース。ローカル HTTP スタブサービスが各ベンダーの URL 構築とレスポンス解析をカバーし、実行時間は約 0.4〜1 秒で、すべての RID（CI を含む）で安定して動作します
- 実際のベンダー／OSM／プラットフォームのオンラインテストは `Category=Integration` としてマークされ、`IntegrationFact` により**既定でスキップ**されます。VS テストエクスプローラーの「すべて実行」も `dotnet test` と同じ動作です
- 統合テストを有効にするには、環境変数 `NEWLIFE_MAP_INTEGRATION=1` を設定してから `--filter "Category=Integration"` を付けて実行します
- 実際のキーはローカルのユーザーシークレットまたは環境変数に保管し、リポジトリにコミットしないでください

実行（Windows PowerShell）：
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # 任意：実環境での統合テスト
```

## ビルド
```bash
dotnet build -c Release
```
出力（マルチフレームワークビルド）は `Bin/` にあります。

## 互換性テスト（CI）
push / PR 時に実行され、主要な RID をカバーします：
- ユニットテストマトリクス：win-x64 / linux-x64 / linux-arm64 / linux-la64（LoongArch、QEMU エミュレーション + 龍芯 .NET SDK。`test.yml` を参照）
- NativeAOT スモークマトリクス：同じ 4 つの RID（`AotSmoke` プロジェクト、18 シナリオ。`aot.yml` を参照）

## ロードマップ / TODO
> 2026-08-04 にすべて完了：並行レート制限（MAP-12）、障害統計（MAP-13）、ベンチマーク比較（MAP-14）

## バージョン方針
メジャーバージョン = 互換性の変更、マイナーバージョン = 新機能、リビジョン = 修正／最適化。`VersionSuffix` に日付を持たせ、ビルドを識別しやすくしています。

## コントリビューション
Issue / PR を歓迎します：バグ、性能最適化、対応地図の追加、ドキュメントとテストなど。送信前に `.github/copilot-instructions.md` を読んで、コードスタイルを統一してください。

## ライセンス
MIT。著作権表示を保持すれば、商用／オープンソースプロジェクトで自由に利用できます。

---

## エコシステムマップ

NewLife は、独立したバージョン管理を行うオープンソースプロジェクトの階層型エコシステムです。以下のマップはすべての NewLife リポジトリで逐語的に同一の内容を共有しており、各プロジェクトはコードリポジトリと主要な NuGet パッケージにリンクしています。

| プロジェクト | 概要 | ダウンロード数 |
| --- | --- | --- |
| **基盤** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | ログ、構成、キャッシュ、ネットワーク、シリアライズ、スケジューリング、APM トレーシング。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | データミドルウェア：単一テーブルで数百億件級、自動シャーディング、読み書き分離、10 種類以上のデータベースに対応。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 高性能ネットワークライブラリ：単一マシンで 177 万 pps、14.5 Gbps（TCP 1KB、2026-10 実測）、400 万 Tcp 長接続。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | CS アプリケーションと IoT デバイス向けのプロトコル通信と RPC フレームワーク。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | .NET アプリケーションを OS デーモンとしてインストール：Windows サービスと Linux systemd。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | コンソール、Web、API、サービス、データ、IoT 向けの `dotnet new` プロジェクトテンプレート。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **データとメッセージング** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | マイクロ秒級の遅延、数百万 ops/s のスループット、メッセージキュー内蔵の Redis クライアント。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Apache RocketMQ と Alibaba Cloud Message Queue に対応した純マネージド RocketMQ クライアント。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT クライアントとサーバー。Alibaba Cloud IoT への接続を含みます。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | C# 製ハイブリッドデータベース：組み込み/ネットワークの両モード、リレーショナル、時系列、メッセージキュー、KV を 1 つに。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | NewLife データスタック上に構築された MySql ドライバーとヘルパー層。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web、AI、分散** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 高速管理プラットフォームフレームワーク：権限、SSO、OAuth サーバー、CRUD スキャフォールディング。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | 分散サービスプラットフォーム：ノード管理、APM、構成センター、登録センター、リリースセンター。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | シャーディング、リトライ、バックフィルに対応した分散タスクとリアルタイムデータのスケジューリングプラットフォーム。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | 46 社の LLM プロバイダーに統一接続するクライアントと、すぐに使える Web チャットアプリ ChatAI。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **産業と IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | プロトコル契約とデバイス抽象を定義する IoT 標準ライブラリ。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | NewLife.IoT 上に実装された Modbus TCP/RTU/ASCII。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | NewLife.IoT 上に実装されたシーメンス PLC プロトコル。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | NewLife.IoT 上に実装されたオムロン PLC プロトコル。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | NewLife.IoT 上に実装された三菱 Melsec PLC プロトコル。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | 産業データアクセス向けの OPC クライアント。 | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet ビル管理プロトコルスタック。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | ゲートウェイとデバイス管理を備えた軽量なオープンソース IoT プラットフォーム。 | — |
| **車載テレマティクス** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 車両位置情報プロトコル：70 以上のメッセージボディ、2011/2013/2019 に対応。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | 運輸管理部門向けの JT/T 809 プラットフォーム間プロトコル。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 車載ビデオ：RTP 解析、FLV/fMP4 リマックス、ライブ配信。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **ドメインコンポーネント** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Excel、Word、PPT、PDF など 15 以上のドキュメント形式を 1 パッケージで。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | クロスプラットフォーム音声：コーデック、デバイス IO、DSP、コンテナ、ストリーミング。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | ビデオコーデック、コンテナ形式、フレーム処理、ストリーミング。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 百度地図、高徳地図、テンセント地図、天地図を統一インターフェイス化。ジオコーディングと座標変換に対応。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 位置情報：オフラインのメモリマップデータベースとオンライン API のフォールバック。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **ツールと製品** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 開発者向けツールボックス：ネットワーク、シリアルポート、暗号化、正規表現、Modbus、MQTT。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | セルフホスト型 Git ソースコード管理システム。 | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | 古いホストから起動した際に対応する .NET ランタイムを自動インストールします。 | — |

> バッジは各プロジェクトの主要パッケージを示します。テーマ、拡張、サテライトパッケージは各 NuGet ページを参照してください。ここに記載していない 30 以上のパッケージ（FTP、LoRa、Office 拡張、各種プロトコルドライバーなど）は [NewLifeX 組織](https://github.com/NewLifeX) と [newlifex.com](https://newlifex.com) で公開しています。

## 新生命開発チーム

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

新生命チームは 2002 年に設立され、20 年以上にわたり .NET と IoT の技術エコシステムに注力し、100 近いオープンソースプロジェクトを主導し（うち 34 が主要プロジェクト）、電力、物流、医療、産業制御、インターネットなどの業界にソフトウェアとハードウェアを一体化したソリューションを提供しています。「学問に先後なし、達する者が師である」を信条としています。

オープンソースコンポーネントは NuGet で累計 <!--nuget-total:start-->6.48M<!--nuget-total:end--> 回ダウンロードされています（エコシステム全体、2026-10 時点）。

ウェブサイト：https://newlifex.com · オープンソース組織：https://github.com/NewLifeX

<details>
<summary>WeChat 公式アカウント：智能大石头（スキャンしてフォロー）</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

