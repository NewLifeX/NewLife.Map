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
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Español](README.es.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Diese Übersetzung kann hinter der chinesischen Version zurückliegen.

NewLife.Map vereinheitlicht die Kartendienste Baidu, AMap, Tencent und Tianditu mit Geokodierung, Reverse-Geokodierung, Routing und Verwaltungsgrenzen.

## Installation

```bash
dotnet add package NewLife.Map
```

## Schnellstart

```csharp
var map = new BaiduMap(accessKey);
var location = await map.GetLocationAsync("北京市海淀区");
```

## Funktionen

- Kompatibel mit gängigen Cloud-Diensten und Managed Middleware
- Breite Abdeckung gängiger Protokoll- und Formatversionen
- Plug-in-fähige, erweiterbare Architektur für eigene Entwicklung

## Dokumentation

> 📖 Vollständige Dokumentation (Chinesisch): [Repository-Dokumente](Doc)

## Lizenz

MIT License © 2002-2026 NewLife-Team

[GitHub](https://github.com/NewLifeX/NewLife.Map) | [Website](https://newlifex.com)
