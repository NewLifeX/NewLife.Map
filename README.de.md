# NewLife.Map - Einheitliche Kartenbibliothek

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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Diese Übersetzung kann hinter der [englischen Version](README.md) zurückliegen; maßgeblich ist die englische Fassung.

NewLife.Map ist eine Kartenkomponentenbibliothek, die Baidu, AMap, Tencent, Tianditu und die unternehmenseigene Karten-API-Plattform hinter einer einzigen Schnittstelle vereint. Sie verbirgt die Unterschiede der Anbieter und bietet Geokodierung, Reverse-Geokodierung, Entfernungsberechnung, Koordinatentransformation und Multi-Key-Circuit-Breaking.

Quelle: https://github.com/NewLifeX/NewLife.Map  
NuGet: `NewLife.Map`  

## Repository-Aufbau
| Projekt | Rolle | Beschreibung |
| ---- | ---- | ---- |
| `NewLife.Map` | Kartenkomponentenbibliothek (NuGet-Paket) | Einheitliche `IMap`-Schnittstelle + Multi-Provider-Adapter + Koordinatentransformation + Key-Circuit-Breaking |
| `MapApi` | Unternehmens-Karten-API-Plattform (Webdienst) | Multi-Provider-Aggregation + mehrstufiger GeoHash-Cache + Cube-Verwaltungsbackend |
| `XUnitTest` | Unit-Tests | Geokodierungs-/Reverse-Geokodierungs-/Entfernungs-/Koordinatentransformationstests je Anbieter (Integrationstests mit echten Diensten werden standardmäßig übersprungen) |

## Dokumentation
- [Anforderungen](Doc/需求文档.md) — Vision, Kernziele, funktionale Anforderungen, Zurückstellungsliste
- [Funktionsliste](Doc/功能清单.md) — Modul-/Funktionscodes mit dreidimensionaler Statusverfolgung (Implementierung/Tests/Kommentare)
- [Architektur](Doc/架构设计.md) — Schichtung, Komponenten, Kernabläufe und Entwurfsentscheidungen
- [Wettbewerbsanalyse](Doc/竞品分析.md) — Vergleichsmatrix und Gap-Analyse gegenüber Geocoding.net / ProjNET / nationalen Einzelanbieter-Wrappern

## Funktionen
- Einheitliche `IMap`-Schnittstelle, die die Unterschiede zwischen Baidu / AMap / Tencent / Tianditu / unternehmenseigenem Hosting (NewLifeMap) verbirgt; ein Anbieterwechsel erfordert keine Codeänderungen
- Geokodierung (Adresse → Koordinaten) / Reverse-Geokodierung (Koordinaten → Adresse), wobei POI-Semantikinformationen automatisch zusammengeführt werden, um Adressbestandteile zu vervollständigen
- Routenentfernung / Fahrzeit, wobei anbieterspezifische Strategieparameter abstrahiert sind
- Abfragen von Verwaltungsgliederungen (AMap / Tencent), POI-/Regionssuche (Baidu / Tianditu), IP-Standortbestimmung (Baidu / Tencent / Online-Auflösung der Plattform)
- Koordinatensystemtransformation: integrierte Offline-Formeln für WGS84 / GCJ02 / BD09 + Online-Stapelkonvertierung der Anbieter (Baidus spezielles from/to-Mapping ist gekapselt)
- Rotation mehrerer Keys mit automatischem Circuit-Breaking + geplanter Wiederherstellung; ein ratenbegrenzter oder gesperrter Key beeinträchtigt die Gesamtverfügbarkeit nicht
- Token-Bucket-Ratenbegrenzung über `MaxQps` und Anforderungsstatistiken (Gesamt / Erfolgsrate / durchschnittliche Latenz), damit ein einzelner Key nicht durch hochfrequente Anforderungen gesperrt wird
- Optionales Auslösen von Ausnahmen über `ThrowException`, was eine einheitliche Behandlung in höheren Schichten erleichtert
- Mehrere Zielframeworks: net45 / net461 / netstandard2.0 / netstandard2.1 (kompatibel mit Legacy-Projekten und modernem .NET)
- Mit starkem Namen signiert, geeignet für Reflexion und Plug-in-Systeme
- Integrierte Laufzeitlokalisierung: 11 Sprachen mit Englisch als Quelle und Fallback; Ausnahmemeldungen, Protokolle und Assembly-Metadaten werden über Kompilierzeit-Sprachtabellen übersetzt
- Integration in die Stardust-Registrierung: Der Anbieter `NewLifeMap` ermittelt Serveradressen automatisch und ermöglicht so einen nahtlosen Wechsel zwischen mehreren Instanzen / Blue-Green-Deployments

## Installation
```bash
# Stabil
dotnet add package NewLife.Map
# Vorschau (datumsbasiert)
dotnet add package NewLife.Map --prerelease
```

## Schnellstart
```csharp
using NewLife.Map;
using NewLife.Data;

// Einen konkreten Kartenanbieter wählen (BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap)
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // Mehrere Keys durch Kommas trennen

// 1. Vorwärts-Geokodierung: Adresse -> Koordinaten
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. Reverse-Geokodierung: Koordinaten -> Adresse
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. Entfernung (Fahren)
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. Koordinatentransformation (Offline-Formeln)
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> Koordinaten müssen sphärische Koordinaten (Längengrad/Breitengrad) verwenden; die Ausgabe ist ein Punkt im BD09-Koordinatensystem.

## Dynamische Erstellung über die Factory
```csharp
var m1 = Map.Create("Baidu");           // Entspricht automatisch dem *Map-Suffix
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// Oder über die Enumeration erstellen
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` wird nach der Instanziierung gesetzt; mehrere Keys werden durch Kommas getrennt und intern rotiert.

## Kartenanbieter
| Anbieter | Klasse | Hauptfunktionen | Hinweise |
| ------ | ---- | -------- | ---- |
| Baidu Maps | `BaiduMap` | Geokodierung / Reverse-Geokodierung / Entfernung / POI-Suche / IP-Standort / Koordinatentransformation (online + offline) | `KeyName=ak` |
| AMap | `AMap` | Geokodierung / Reverse-Geokodierung / Entfernung / Verwaltungsgliederungen | `KeyName=key` |
| Tencent Maps | `WeMap` | Geokodierung / Reverse-Geokodierung / Entfernung / IP-Standort / Verwaltungsgliederungen | `KeyName=key` |
| Tianditu | `TianDiTu` | Geokodierung / Reverse-Geokodierung / Entfernung (mit Wegpunkten) / POI-Suche | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | Geokodierung / Reverse-Geokodierung (Open Source, kostenlos, kein Key, WGS84) | Kein Key erforderlich |
| NewLife Map Platform | `NewLifeMap` | Unternehmens-Aggregations-API (verbindet sich mit der MapApi-Plattform) | Service-Discovery über Stardust |

> Um einen Anbieter hinzuzufügen, siehe „Einen neuen Kartenanbieter erweitern“ unten und folge dem Muster von AMap/BaiduMap.

## Koordinatensystemtransformation
| Name | Beschreibung | Übliche Anbieter |
| ---- | -------- | ---- |
| WGS84 | Internationaler GPS-Standard | Rohdaten der Gerätepositionierung |
| GCJ02 | Vom Staatlichen Amt für Vermessung und Kartographie angewendeter Versatz | AMap / Tencent / die meisten chinesischen Online-Karten |
| BD09 | Baidu-Versatz | Baidu |

```csharp
// Offline-Formelkonvertierung, verbraucht kein Anbieterkontingent
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// Online-Stapelkonvertierung von Baidu, from/to-Indexwerte bereits gekapselt
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## AppKey-Rotation und Circuit-Breaking
- `AppKey="k1,k2,k3"` übergeben; jede Anforderung erhöht einen Index und bildet den Rest zur Rotation
- Wenn eine Antwort auf Ungültigkeits-Schlüsselwörter (z. B. `TOO_FREQUENT` / `LIMIT` / `INVALID`) passt, wird `RemoveKey()` aufgerufen, um diesen Key vorübergehend außer Betrieb zu nehmen
- Außer Betrieb genommene Keys landen in `_pendingKeys` und werden von einem `TimerX`-Timer geprüft und bei Fälligkeit automatisch wiederhergestellt
- Wiederherstellungszeit: wird derzeit vom Aufrufer übergeben (z. B. 1 Stunde)

## NewLifeMap-Aggregationsplattform
`NewLifeMap` verweist auf die integrierte Unternehmens-Karten-API-Plattform `MapApi`. Die Plattform aggregiert mehrere Anbieter und cached Ergebnisse, sodass Clients die zugrunde liegenden Unterschiede nicht kennen müssen.

```csharp
// Option 1: Server direkt angeben
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// Option 2: Dependency Injection + automatische Adressauflösung über die Stardust-Registrierung
services.AddSingleton<IMap, NewLifeMap>();
// Der Konstruktor NewLifeMap(IServiceProvider) versucht: IRegistry -> StarFactory
```

- Der gebundene `ApiHttpClient` aktualisiert die Serveradressliste automatisch und ermöglicht so einen nahtlosen Wechsel bei MapApi-Deployments mit mehreren Replikaten / Blue-Green
- Die Plattform stellt die einheitlichen Endpunkte `GET /Map/Geo`, `/Map/ReverseGeo` (Reverse-Geokodierung), `/Map/GeoAddress` (Vorwärts-Geokodierung) und `/Map/Distance` (Entfernung) bereit; ein vierstufiger GeoHash-Cache (6–9 Zeichen) reduziert Anbieteraufrufe

## Fehlerbehandlung
- Standard: Eine nicht erfolgreiche Antwort löst keine Ausnahme aus; die Methode gibt `null` zurück
- Mit `map.ThrowException = true;` wird eine `Exception` ausgelöst, die in der äußeren Schicht einheitlich abgefangen werden kann
- Debug-Informationen der letzten Anforderung: `LastUrl` / `LastString` / `LastResult` / `LastKey`

## API-Übersicht
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
Erweiterungen (anbieterspezifisch):
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // POI-/Regionssuche von BaiduMap / Tianditu
Task<IList<GeoArea>> GetAreaAsync(...);       // Verwaltungsgliederungen von AMap
Task<IDictionary<string, object?>> IpLocationAsync(...); // IP-Standort von BaiduMap / WeMap
```

## Einen neuen Kartenanbieter erweitern
1. Eine Klasse `XXXMap : Map, IMap` erstellen
2. `Server` / `KeyName` setzen
3. `InvokeAsync<T>` überschreiben, um die einheitliche Antwortstruktur und die Fehlercodes des Anbieters zu analysieren
4. `GetGeoAsync` / `GetReverseGeoAsync` / weitere benötigte Fähigkeiten implementieren
5. Bei anbieterspezifischen Signalen für Ratenbegrenzung / Ungültigkeit `IsValidKey` überschreiben, um true zurückzugeben und die vorübergehende Außerbetriebnahme auszulösen
6. Im statischen Konstruktor von `MapFactory` mit `Register<XXXMap>(MapKinds.XXX)` registrieren
7. Unit-Tests einreichen, die die Hauptfunktionalität und Randfälle abdecken

## Unit-Tests
Das Repository enthält das Projekt `XUnitTest`; die Lösung ist `NewLife.Map.slnx` (neues XML-Format von VS 2026 / .NET 10):
- Standardmäßig werden **deterministische Tests** ausgeführt: 84 Fälle; ein lokaler HTTP-Stub-Dienst deckt die URL-Konstruktion und Antwortanalyse jedes Anbieters ab; die Ausführung dauert etwa 0,4–1 Sekunde und läuft zuverlässig auf allen RIDs (einschließlich CI)
- Online-Tests mit echten Anbietern / OSM / Plattform sind als `Category=Integration` markiert und werden über `IntegrationFact` **standardmäßig übersprungen**; „Alle ausführen“ im VS-Test-Explorer verhält sich wie `dotnet test`
- So aktivieren Sie Integrationstests: zuerst die Umgebungsvariable `NEWLIFE_MAP_INTEGRATION=1` setzen, dann mit `--filter "Category=Integration"` ausführen
- Echte Keys in lokalen Benutzergeheimnissen oder Umgebungsvariablen aufbewahren; niemals im Repository festschreiben

Ausführen (Windows PowerShell):
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # Optional: Integration in Live-Umgebung
```

## Build
```bash
dotnet build -c Release
```
Die Ausgabe (Multiframework-Build) befindet sich in `Bin/`.

## Kompatibilitätstests (CI)
Ausgelöst bei Push / PR und decken die wichtigsten RIDs ab:
- Unit-Test-Matrix: win-x64 / linux-x64 / linux-arm64 / linux-la64 (LoongArch, QEMU-Emulation + Loongson .NET SDK, siehe `test.yml`)
- NativeAOT-Smoke-Matrix: dieselben vier RIDs (Projekt `AotSmoke`, 18 Szenarien), siehe `aot.yml`

## Roadmap / TODO
> Alle am 2026-08-04 abgeschlossen: nebenläufige Ratenbegrenzung (MAP-12), Fehlerstatistiken (MAP-13), Benchmark-Vergleich (MAP-14)

## Versionsschema
Hauptversion = Kompatibilitätsänderungen; Nebenversion = neue Funktionen; Revision = Fehlerbehebungen / Optimierungen. `VersionSuffix` enthält ein Datum zur einfachen Build-Identifikation.

## Mitwirken
Issues / PRs sind willkommen: Fehler, Performance-Optimierungen, Unterstützung weiterer Karten, Dokumentation und Tests. Bitte vor dem Einreichen `.github/copilot-instructions.md` lesen, um den Codestil konsistent zu halten.

## Lizenz
MIT; behalten Sie den Urheberrechtshinweis bei, dann können Sie es frei in kommerziellen / Open-Source-Projekten verwenden.

---

## Ökosystem-Karte

NewLife ist ein geschichtetes Ökosystem unabhängig versionierter Open-Source-Projekte. Die folgende Karte wird wortgleich in allen NewLife-Repositories verwendet; jedes Projekt verlinkt auf sein Repository und sein primäres NuGet-Paket.

| Projekt | Funktion | Downloads |
| --- | --- | --- |
| **Grundlage** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Protokollierung, Konfiguration, Caching, Netzwerk, Serialisierung, Zeitplanung und APM-Tracing. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Daten-Middleware: einzelne Tabellen in der Größenordnung von zig Milliarden Zeilen, automatisches Sharding, Lese-/Schreibtrennung, über 10 Datenbanken. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Hochperformante Netzwerkbibliothek: 1,77 Mio. pps / 14,5 Gbps auf einer Maschine (TCP 1KB, 2026-10), 4 Mio. langlebige TCP-Verbindungen. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Protokollkommunikation und RPC-Framework für Client/Server-Anwendungen und IoT-Geräte. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Installiert .NET-Anwendungen als Betriebssystem-Dienste: Windows-Dienste und Linux-systemd. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | `dotnet new`-Projektvorlagen für Konsole, Web, API, Dienst, Daten und IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Daten & Messaging** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis-Client mit Mikrosekunden-Latenz, Millionen Operationen pro Sekunde und integrierten Nachrichtenwarteschlangen. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Vollständig verwalteter RocketMQ-Client für Apache RocketMQ und Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT-Client und -Server, einschließlich Alibaba Cloud IoT-Anbindung. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Hybriddatenbank in C#: eingebettet oder im Netzwerk, relational, Zeitreihen, Nachrichtenwarteschlange und KV in einem. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | MySql-Treiber und Hilfsschicht auf Basis des NewLife-Datenstacks. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, KI & verteilt** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Schnelles Framework für Administrationsplattformen: Berechtigungen, SSO, OAuth-Server und CRUD-Gerüst. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Verteilte Dienstplattform: Knotenverwaltung, APM, Konfigurationszentrum, Registry und Release-Center. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plattform für verteilte Aufgaben und Echtzeit-Datenplanung mit Sharding, Wiederholungen und Backfill. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Einheitlicher Client für 46 LLM-Anbieter plus ChatAI, eine sofort einsatzbereite Web-Chat-Anwendung. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industrie & IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | IoT-Standardbibliothek mit Protokollverträgen und Geräteabstraktionen. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Modbus-TCP/RTU/ASCII-Implementierung auf Basis von NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Siemens-SPS-Protokoll auf Basis von NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Omron-SPS-Protokoll auf Basis von NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Mitsubishi-Melsec-SPS-Protokoll auf Basis von NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | OPC-Client für industriellen Datenzugriff. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet-Protokollstack für die Gebäudeautomation. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Leichtgewichtige Open-Source-IoT-Plattform mit Gateway- und Geräteverwaltung. | — |
| **Fahrzeugtelematik** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T-808-Fahrzeugortungsprotokoll: über 70 Nachrichtenkörper, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | JT/T-809-Plattform-zu-Plattform-Protokoll für Verkehrsbehörden. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T-1078-Fahrzeugvideo: RTP-Parsing, FLV/fMP4-Remuxing und Live-Streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Domänenkomponenten** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Ein Paket für Excel, Word, PPT, PDF und über 15 Dokumentformate. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Plattformübergreifendes Audio: Codecs, Geräte-I/O, DSP, Container und Streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Videocodecs, Containerformate, Frame-Verarbeitung und Streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Einheitliche Schnittstelle für Baidu, AMap, Tencent und Tianditu mit Geokodierung und Koordinatenumrechnung. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4-Geolokalisierung mit einer offline speichergemappten Datenbank und einer Online-API als Fallback. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Werkzeuge & Produkte** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Entwickler-Toolbox: Netzwerk, serielle Schnittstelle, Krypto, Regex, Modbus und MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Selbst gehostetes Git-Quellcode-Verwaltungssystem. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Installiert automatisch die passende .NET-Laufzeit, wenn es von einem älteren Host gestartet wird. | — |

> Die Badges zeigen das primäre Paket jedes Projekts; Theme-, Erweiterungs- und Satellitenpakete sind auf den jeweiligen NuGet-Seiten aufgeführt.Die hier nicht aufgeführten über 30 Pakete (FTP, LoRa, Office-Erweiterungen, Protokolltreiber und mehr) sind in der [NewLifeX-Organisation](https://github.com/NewLifeX) und auf [newlifex.com](https://newlifex.com) verfügbar.

## Team

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

NewLife wurde 2002 gegründet und widmet sich seit mehr als zwei Jahrzehnten dem .NET- und IoT-Ökosystem, betreut fast 100 Open-Source-Projekte (34 davon Hauptprojekte) und liefert integrierte Hard- und Softwarelösungen für die Bereiche Energie, Logistik, Gesundheitswesen, Industrieautomation und Internet. Unser Leitspruch lautet „Lernen kennt kein Dienstalter – wer es beherrscht, wird zum Lehrmeister“.

Die Open-Source-Komponenten wurden <!--nuget-total:start-->6.48M<!--nuget-total:end--> Mal von NuGet heruntergeladen (ökosystemweit, Stand 2026-10).

Website: https://newlifex.com · Open-Source-Organisation: https://github.com/NewLifeX

<details>
<summary>WeChat-Offizieller Account: 智能大石头 (zum Folgen scannen)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

