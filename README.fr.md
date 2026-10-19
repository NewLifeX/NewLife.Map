# NewLife.Map - Bibliothèque de cartes unifiée

![GitHub License](https://img.shields.io/github/license/NewLifeX/NewLife.Map?logo=github)
![NuGet Downloads](https://img.shields.io/nuget/dt/NewLife.Map?logo=nuget)
![NuGet](https://img.shields.io/nuget/v/NewLife.Map?logo=nuget)
![NuGet prerelease](https://img.shields.io/nuget/vpre/NewLife.Map?label=dev%20nuget&logo=nuget)
[![test](https://github.com/NewLifeX/NewLife.Map/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/NewLife.Map/actions/workflows/test.yml)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS%20%7C%20Android%20%7C%20iOS-blue)
![arch](https://img.shields.io/badge/arch-x64%20%7C%20arm64%20%7C%20loongarch64-orange)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/NewLife.Map?logo=github)](https://github.com/NewLifeX/NewLife.Map/graphs/contributors)

<!-- lang:start -->
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Cette traduction peut être en retard sur la version chinoise.

NewLife.Map unifie les services Baidu, AMap, Tencent et Tianditu : géocodage, géocodage inverse, itinéraires et limites administratives.

## Installation

```bash
dotnet add package NewLife.Map
```

## Démarrage rapide

```csharp
var map = new BaiduMap(accessKey);
var location = await map.GetLocationAsync("北京市海淀区");
```

## Fonctionnalités

- Compatible avec les services cloud et middlewares managés
- Large couverture des versions de protocoles et formats courants
- Architecture extensible et modulaire pour vos développements

## Documentation

> 📖 Documentation complète (chinois) : [documents du dépôt](Doc)

## Licence

MIT License © 2002-2026 Équipe NewLife

[GitHub](https://github.com/NewLifeX/NewLife.Map) | [Website](https://newlifex.com)
