# NewLife.Map - 통합 지도 구성 요소 라이브러리

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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [العربية](README.ar.md)
<!-- lang:end -->

> 이 번역은 [영어 버전](README.md)보다 늦을 수 있으며, 영어 버전을 기준으로 합니다.

NewLife.Map은 Baidu, AMap, Tencent, Tianditu 및 기업용 지도 API 플랫폼을 하나의 인터페이스 뒤로 통합한 지도 구성 요소 라이브러리입니다. 공급자 간 차이를 숨기고 지오코딩, 역지오코딩, 거리 계산, 좌표 변환, 다중 키 회로 차단 기능을 제공합니다.

소스: https://github.com/NewLifeX/NewLife.Map  
NuGet: `NewLife.Map`  

## 저장소 구성
| 프로젝트 | 역할 | 설명 |
| ---- | ---- | ---- |
| `NewLife.Map` | 지도 구성 요소 라이브러리(NuGet 패키지) | 통합 `IMap` 인터페이스 + 다중 공급자 어댑터 + 좌표 변환 + 키 회로 차단 |
| `MapApi` | 기업용 지도 API 플랫폼(웹 서비스) | 다중 공급자 집계 + 다단계 GeoHash 캐시 + Cube 관리 백엔드 |
| `XUnitTest` | 단위 테스트 | 공급자별 지오코딩/역지오코딩/거리/좌표 변환 테스트(실제 서비스 연동 사례는 기본적으로 건너뜀) |

## 문서
- [요구 사항](Doc/需求文档.md) — 비전, 핵심 목표, 기능 요구 사항, 보류 목록
- [기능 목록](Doc/功能清单.md) — 구현/테스트/주석 3차원 상태 추적이 포함된 모듈/기능 코드
- [아키텍처](Doc/架构设计.md) — 계층, 구성 요소, 핵심 흐름, 설계 결정
- [경쟁 분석](Doc/竞品分析.md) — Geocoding.net / ProjNET / 국내 단일 공급자 래퍼와의 비교 매트릭스 및 격차 분석

## 기능
- Baidu / AMap / Tencent / Tianditu / 기업 자체 호스팅(NewLifeMap) 간 차이를 숨기는 통합 `IMap` 인터페이스. 공급자를 바꿔도 코드 변경이 필요 없습니다
- 지오코딩(주소 → 좌표) / 역지오코딩(좌표 → 주소). POI 의미 정보를 자동으로 병합하여 주소 구성 요소를 보완합니다
- 경로 거리 / 주행 시간. 공급자별 전략 매개변수는 추상화되어 있습니다
- 행정 구역 조회(AMap / Tencent), POI / 지역 검색(Baidu / Tianditu), IP 위치 조회(Baidu / Tencent / 플랫폼 온라인 해석)
- 좌표계 변환: WGS84 / GCJ02 / BD09 오프라인 공식 내장 + 공급자 온라인 일괄 변환(Baidu 고유의 from/to 매핑은 캡슐화됨)
- 다중 키 순환과 자동 회로 차단 + 예약 복구. 단일 키가 속도 제한이나 차단을 받아도 전체 가용성에는 영향을 주지 않습니다
- `MaxQps`를 통한 토큰 버킷 속도 제한과 요청 통계(전체/성공률/평균 지연). 단일 키가 고빈도 요청으로 차단되는 것을 방지합니다
- `ThrowException`으로 예외 발생을 선택할 수 있어 상위 계층에서 통합 처리하기 쉽습니다
- 다중 대상 프레임워크: net45 / net461 / netstandard2.0 / netstandard2.1(레거시 프로젝트와 최신 .NET 모두 호환)
- 강력한 이름으로 서명되어 리플렉션과 플러그인 시스템에 적합합니다
- 내장 런타임 지역화: 영어를 원문 및 대체로 하는 11개 언어 지원. 예외 메시지, 로그, 어셈블리 메타데이터는 컴파일 타임 언어 표로 번역됩니다
- Stardust 레지스트리 통합: `NewLifeMap` 공급자가 서버 주소를 자동으로 검색하므로 다중 인스턴스 / 블루-그린 배포에서도 원활하게 전환됩니다

## 설치
```bash
# 안정 버전
dotnet add package NewLife.Map
# 미리 보기(날짜 기반)
dotnet add package NewLife.Map --prerelease
```

## 빠른 시작
```csharp
using NewLife.Map;
using NewLife.Data;

// 구체적인 지도 공급자 선택(BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap)
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // 여러 키는 쉼표로 구분

// 1. 정방향 지오코딩: 주소 -> 좌표
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. 역지오코딩: 좌표 -> 주소
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. 거리(주행)
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. 좌표 변환(오프라인 공식)
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> 좌표는 구면 좌표(경도/위도)를 사용해야 하며, 출력은 BD09 좌표계의 점입니다.

## 팩터리를 통한 동적 생성
```csharp
var m1 = Map.Create("Baidu");           // *Map 접미사를 자동으로 매칭
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// 열거형으로 생성할 수도 있습니다
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey`는 인스턴스 생성 후 설정하며, 여러 키는 쉼표로 구분되어 내부에서 순환됩니다.

## 지도 공급자
| 공급자 | 클래스 | 주요 기능 | 비고 |
| ------ | ---- | -------- | ---- |
| Baidu 지도 | `BaiduMap` | 지오코딩 / 역지오코딩 / 거리 / POI 검색 / IP 위치 / 좌표 변환(온라인 + 오프라인) | `KeyName=ak` |
| AMap | `AMap` | 지오코딩 / 역지오코딩 / 거리 / 행정 구역 | `KeyName=key` |
| Tencent 지도 | `WeMap` | 지오코딩 / 역지오코딩 / 거리 / IP 위치 / 행정 구역 | `KeyName=key` |
| Tianditu | `TianDiTu` | 지오코딩 / 역지오코딩 / 거리(경유지 지원) / POI 검색 | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | 지오코딩 / 역지오코딩(오픈 소스, 무료, 키 불필요, WGS84) | 키 불필요 |
| NewLife Map 플랫폼 | `NewLifeMap` | 기업용 집계 API(MapApi 플랫폼에 연결) | Stardust를 통한 서비스 검색 |

> 공급자를 추가하려면 아래의 "새 지도 공급자 확장"을 참고하고 AMap/BaiduMap 패턴을 따르세요.

## 좌표계 변환
| 이름 | 설명 | 일반적인 공급자 |
| ---- | -------- | ---- |
| WGS84 | 국제 GPS 표준 | 기기 원시 측위 |
| GCJ02 | 국가측량제도국이 적용한 오프셋 | AMap / Tencent / 대부분의 중국 온라인 지도 |
| BD09 | Baidu 오프셋 | Baidu |

```csharp
// 오프라인 공식 변환, 공급자 할당량을 소비하지 않음
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// Baidu 온라인 일괄 변환, from/to 인덱스 값은 이미 캡슐화됨
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## AppKey 다중 키 순환과 회로 차단
- `AppKey="k1,k2,k3"`를 전달하면 요청마다 인덱스를 증가시켜 나머지 연산으로 순환합니다
- 응답이 무효 키워드(`TOO_FREQUENT` / `LIMIT` / `INVALID` 등)와 일치하면 `RemoveKey()`를 호출하여 해당 키를 일시적으로 비활성화합니다
- 비활성화된 키는 `_pendingKeys`에 들어가고 `TimerX` 타이머가 검사하여 만료 시 자동으로 복구합니다
- 복구 시간 정책: 현재 호출자가 지정합니다(예: 1시간)

## NewLifeMap 집계 플랫폼
`NewLifeMap`은 내장된 `MapApi` 기업용 지도 API 플랫폼을 가리킵니다. 플랫폼은 여러 공급자를 집계하고 결과를 캐시하므로 클라이언트는 하위 차이를 알 필요가 없습니다.

```csharp
// 방법 1: 서버 주소를 직접 지정
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// 방법 2: 종속성 주입 + Stardust 레지스트리를 통한 주소 자동 확인
services.AddSingleton<IMap, NewLifeMap>();
// NewLifeMap(IServiceProvider) 생성자는 IRegistry -> StarFactory 순으로 시도합니다
```

- 바인딩된 `ApiHttpClient`가 서버 주소 목록을 자동으로 갱신하므로 MapApi 다중 복제본 / 블루-그린 배포 시 원활하게 전환됩니다
- 플랫폼은 `GET /Map/Geo`, `/Map/ReverseGeo`(역지오코딩), `/Map/GeoAddress`(정방향 지오코딩), `/Map/Distance`(거리) 통합 엔드포인트를 제공하며, 6~9자 4단계 GeoHash 캐시로 공급자 호출을 줄입니다

## 오류 처리
- 기본: 실패 응답은 예외를 발생시키지 않고 메서드가 `null`을 반환합니다
- `map.ThrowException = true;`를 설정하면 `Exception`이 발생하여 상위 계층에서 통합적으로 처리할 수 있습니다
- 마지막 요청의 디버그 정보: `LastUrl` / `LastString` / `LastResult` / `LastKey`

## API 개요
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
확장(공급자별):
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // BaiduMap / Tianditu POI / 지역 검색
Task<IList<GeoArea>> GetAreaAsync(...);       // AMap 행정 구역
Task<IDictionary<string, object?>> IpLocationAsync(...); // BaiduMap / WeMap IP 위치
```

## 새 지도 공급자 확장
1. `XXXMap : Map, IMap` 클래스를 만듭니다
2. `Server` / `KeyName`을 설정합니다
3. `InvokeAsync<T>`를 재정의하여 공급자의 통합 응답 구조와 오류 코드를 분석합니다
4. `GetGeoAsync` / `GetReverseGeoAsync` / 기타 필요한 기능을 구현합니다
5. 공급자 고유의 속도 제한 / 무효화 신호가 있으면 `IsValidKey`를 재정의하여 true를 반환하고 일시 비활성화를 트리거합니다
6. `MapFactory` 정적 생성자에서 `Register<XXXMap>(MapKinds.XXX)`로 등록합니다
7. 주요 기능과 경계 사례를 다루는 단위 테스트를 제출합니다

## 단위 테스트
저장소에는 `XUnitTest` 프로젝트가 포함되어 있으며, 솔루션은 `NewLife.Map.slnx`(VS 2026 / .NET 10의 새로운 XML 형식)입니다:
- 기본적으로 **결정적 테스트**를 실행합니다: 84개 사례. 로컬 HTTP 스텁 서비스가 각 공급자의 URL 구성과 응답 분석을 다루며, 실행 시간은 약 0.4~1초이고 모든 RID(CI 포함)에서 안정적으로 실행됩니다
- 실제 공급자 / OSM / 플랫폼 온라인 테스트는 `Category=Integration`으로 표시되며 `IntegrationFact`를 통해 **기본적으로 건너뜁니다**. VS 테스트 탐색기의 "모두 실행"도 `dotnet test`와 동일하게 동작합니다
- 통합 테스트를 활성화하려면 먼저 환경 변수 `NEWLIFE_MAP_INTEGRATION=1`을 설정한 후 `--filter "Category=Integration"`으로 실행합니다
- 실제 키는 로컬 사용자 비밀이나 환경 변수에 보관하고 절대 저장소에 커밋하지 마세요

실행(Windows PowerShell):
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # 선택 사항: 실제 환경 통합
```

## 빌드
```bash
dotnet build -c Release
```
출력(다중 프레임워크 빌드)은 `Bin/`에 있습니다.

## 호환성 테스트(CI)
push / PR 시 트리거되며 주요 RID를 다룹니다:
- 단위 테스트 매트릭스: win-x64 / linux-x64 / linux-arm64 / linux-la64(LoongArch, QEMU 에뮬레이션 + Loongson .NET SDK, `test.yml` 참조)
- NativeAOT 스모크 매트릭스: 동일한 4개 RID(`AotSmoke` 프로젝트, 18개 시나리오), `aot.yml` 참조

## 로드맵 / TODO
> 2026-08-04에 모두 완료: 동시 속도 제한(MAP-12), 장애 통계(MAP-13), 벤치마크 비교(MAP-14)

## 버전 정책
주 버전 = 호환성 변경, 부 버전 = 새 기능, 수정 버전 = 수정 / 최적화. `VersionSuffix`에 날짜가 포함되어 빌드를 쉽게 식별할 수 있습니다.

## 기여
이슈 / PR을 환영합니다: 버그, 성능 최적화, 더 많은 지도 지원, 문서 및 테스트. 제출 전에 `.github/copilot-instructions.md`를 읽어 코드 스타일을 일관되게 유지해 주세요.

## 라이선스
MIT. 저작권 표시를 유지하면 상업용 / 오픈 소스 프로젝트에서 자유롭게 사용할 수 있습니다.

---

## 생태계 지도

NewLife는 독립적으로 버전이 관리되는 오픈소스 프로젝트의 계층형 생태계입니다. 아래 지도는 모든 NewLife 리포지토리에서 그대로 동일하게 공유되며, 각 프로젝트는 코드 리포지토리와 기본 NuGet 패키지로 연결됩니다.

| 프로젝트 | 한 줄 소개 | 다운로드 |
| --- | --- | --- |
| **기반** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | 로깅, 구성, 캐시, 네트워크, 직렬화, 스케줄링, APM 추적. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | 데이터 미들웨어: 단일 테이블 수백억 건, 자동 분할 테이블, 읽기/쓰기 분리, 10여 종 데이터베이스 지원. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 고성능 네트워크 라이브러리: 단일 노드 177만 pps, 14.5 Gbps(TCP 1KB, 2026-10 실측), 400만 Tcp 장기 연결. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | CS 애플리케이션과 IoT 장치를 위한 프로토콜 통신 및 RPC 프레임워크. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | .NET 애플리케이션을 운영체제 데몬으로 설치: Windows 서비스와 Linux systemd. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | 콘솔, Web, API, 서비스, 데이터, IoT용 `dotnet new` 프로젝트 템플릿. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **데이터 및 메시징** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis 클라이언트: 마이크로초 수준 지연, 백만 수준 처리량, 메시지 큐 내장. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Apache RocketMQ와 알리바바 클라우드 메시지 큐를 지원하는 순수 관리형 RocketMQ 클라이언트. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT 클라이언트와 서버, 알리바바 클라우드 IoT 연결 포함. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | C# 하이브리드 데이터베이스: 임베디드/네트워크 두 모드, 관계형, 시계열, 메시지 큐, KV를 하나로. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | NewLife 데이터 스택 위에 구축한 MySql 드라이버와 도우미 계층. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, AI 및 분산** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 빠른 관리 플랫폼 프레임워크: 권한, SSO, OAuth 서버, CRUD 스캐폴딩. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | 분산 서비스 플랫폼: 노드 관리, APM, 구성 센터, 등록 센터, 배포 센터. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | 샤딩, 재시도, 백필을 지원하는 분산 작업 및 실시간 데이터 스케줄링 플랫폼. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | 46개 LLM 공급자를 통합 연결하는 클라이언트와 바로 사용할 수 있는 ChatAI 웹 채팅 애플리케이션. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **산업 및 IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | 프로토콜 계약과 장치 추상을 정의하는 IoT 표준 라이브러리. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | NewLife.IoT 기반으로 구현한 Modbus TCP/RTU/ASCII. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | NewLife.IoT 기반으로 구현한 지멘스 PLC 프로토콜. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | NewLife.IoT 기반으로 구현한 오므론 PLC 프로토콜. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | NewLife.IoT 기반으로 구현한 미쓰비시 Melsec PLC 프로토콜. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | 산업 데이터 수집용 OPC 클라이언트. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet 빌딩 자동화 프로토콜 스택. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | 게이트웨이와 장치 관리를 갖춘 경량 오픈소스 IoT 플랫폼. | — |
| **차량 텔레매틱스** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 차량 위치 프로토콜: 70개 이상 메시지 본문, 2011/2013/2019 지원. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | 운송 관리 부서를 위한 JT/T 809 플랫폼 간 프로토콜. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 차량용 비디오: RTP 파싱, FLV/fMP4 리먹싱, 라이브 스트리밍. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **도메인 컴포넌트** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Excel, Word, PPT, PDF 등 15개 이상 문서 형식을 하나의 패키지로. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | 크로스 플랫폼 오디오: 코덱, 장치 IO, DSP, 컨테이너, 스트리밍. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | 비디오 코덱, 컨테이너 형식, 프레임 처리, 스트리밍. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 바이두/고덕/텐센트/톈디투 지도 통합 인터페이스, 지오코딩과 좌표 변환 지원. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 위치 정보: 오프라인 메모리 매핑 데이터베이스와 온라인 API 이중 소스. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **도구 및 제품** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 개발자 도구 모음: 네트워크, 시리얼 포트, 암호화, 정규식, Modbus, MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | 셀프 호스팅 Git 소스 코드 관리 시스템. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | 구버전 호스트에서 시작할 때 일치하는 .NET 런타임을 자동으로 설치합니다. | — |

> 배지는 각 프로젝트의 기본 패키지를 나타냅니다. 테마, 확장, 위성 패키지는 해당 NuGet 페이지에서 확인하세요.여기에 나열하지 않은 30여 개 패키지(FTP, LoRa, Office 확장, 각종 프로토콜 드라이버 등)는 [NewLifeX 조직](https://github.com/NewLifeX)과 [newlifex.com](https://newlifex.com)에서 확인할 수 있습니다.

## 뉴라이프 개발팀

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

뉴라이프 팀(NewLife)은 2002년에 설립되어 20여 년간 .NET과 IoT 기술 생태계에 집중하며 100여 개의 오픈소스 프로젝트를 주도하고(이 중 34개가 핵심 프로젝트), 전력, 물류, 의료, 산업 제어, 인터넷 등 산업에 소프트웨어와 하드웨어를 아우르는 통합 솔루션을 제공해 왔습니다. "학문에는 선후가 없으며, 이룬 사람이 스승이다"라는 신조를 따릅니다.

오픈소스 컴포넌트는 NuGet에서 누적 <!--nuget-total:start-->6.48M<!--nuget-total:end-->회 다운로드되었습니다(생태계 전체 기준, 2026-10).

웹사이트: https://newlifex.com · 오픈소스 조직: https://github.com/NewLifeX

<details>
<summary>위챗 공식 계정: 智能大石头(스캔하여 팔로우)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

