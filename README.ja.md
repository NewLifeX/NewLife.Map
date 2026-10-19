# NewLife.Map - 統合マップコンポーネントライブラリ

![GitHub License](https://img.shields.io/github/license/NewLifeX/NewLife.Map?logo=github)
![NuGet Downloads](https://img.shields.io/nuget/dt/NewLife.Map?logo=nuget)
![NuGet](https://img.shields.io/nuget/v/NewLife.Map?logo=nuget)
![NuGet prerelease](https://img.shields.io/nuget/vpre/NewLife.Map?label=dev%20nuget&logo=nuget)
[![test](https://github.com/NewLifeX/NewLife.Map/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/NewLife.Map/actions/workflows/test.yml)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS%20%7C%20Android%20%7C%20iOS-blue)
![arch](https://img.shields.io/badge/arch-x64%20%7C%20arm64%20%7C%20loongarch64-orange)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/NewLife.Map?logo=github)](https://github.com/NewLifeX/NewLife.Map/graphs/contributors)

<!-- lang:start -->
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> この翻訳は中国語版より遅れている場合があります。

NewLife.Map は Baidu、AMap、Tencent、天地図などの地図サービスを統合し、ジオコーディング、逆ジオコーディング、経路探索、行政区画を提供します。

## インストール

```bash
dotnet add package NewLife.Map
```

## クイックスタート

```csharp
var map = new BaiduMap(accessKey);
var location = await map.GetLocationAsync("北京市海淀区");
```

## 主な機能

- 主要クラウドサービスとマネージドミドルウェアに対応
- 主要なプロトコル／フォーマットの各バージョンに対応
- プラグイン可能な拡張アーキテクチャで二次開発が容易

## ドキュメント

> 📖 詳細ドキュメント（中国語）： [リポジトリ内ドキュメント](Doc)

## ライセンス

MIT License © 2002-2026 NewLife チーム

[GitHub](https://github.com/NewLifeX/NewLife.Map) | [Website](https://newlifex.com)
