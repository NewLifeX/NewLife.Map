# NewLife.Map - Unified map component library

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
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> This translation may lag behind the Chinese version.

NewLife.Map unifies Baidu, AMap, Tencent and Tianditu map services, offering geocoding, reverse geocoding, routing and administrative boundaries.

## Installation

```bash
dotnet add package NewLife.Map
```

## Quick start

```csharp
var map = new BaiduMap(accessKey);
var location = await map.GetLocationAsync("北京市海淀区");
```

## Key features

- Works with mainstream cloud services and managed middleware
- Broad coverage of mainstream protocol and format versions
- Pluggable, extensible architecture for custom development

## Documentation

> 📖 Full documentation (Chinese): [repository docs](Doc)

## License

MIT License © 2002-2026 NewLife Team

[GitHub](https://github.com/NewLifeX/NewLife.Map) | [Website](https://newlifex.com)
