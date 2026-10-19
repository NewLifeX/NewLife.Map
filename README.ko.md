# NewLife.Map - 통합 지도 구성 요소 라이브러리

![GitHub License](https://img.shields.io/github/license/NewLifeX/NewLife.Map?logo=github)
![NuGet Downloads](https://img.shields.io/nuget/dt/NewLife.Map?logo=nuget)
![NuGet](https://img.shields.io/nuget/v/NewLife.Map?logo=nuget)
![NuGet prerelease](https://img.shields.io/nuget/vpre/NewLife.Map?label=dev%20nuget&logo=nuget)
[![test](https://github.com/NewLifeX/NewLife.Map/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/NewLife.Map/actions/workflows/test.yml)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS%20%7C%20Android%20%7C%20iOS-blue)
![arch](https://img.shields.io/badge/arch-x64%20%7C%20arm64%20%7C%20loongarch64-orange)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/NewLife.Map?logo=github)](https://github.com/NewLifeX/NewLife.Map/graphs/contributors)

<!-- lang:start -->
🌐 [中文](Readme.MD) | [繁體中文](README.zh-TW.md) | [English](README.en.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [العربية](README.ar.md)
<!-- lang:end -->

> 이 번역은 중국어 버전보다 늦을 수 있습니다.

NewLife.Map는 Baidu, AMap, Tencent, Tianditu 지도 서비스를 통합하여 지오코딩, 역지오코딩, 경로 탐색, 행정 구역을 제공합니다.

## 설치

```bash
dotnet add package NewLife.Map
```

## 빠른 시작

```csharp
var map = new BaiduMap(accessKey);
var location = await map.GetLocationAsync("北京市海淀区");
```

## 주요 기능

- 주요 클라우드 서비스와 관리형 미들웨어 지원
- 주요 프로토콜과 형식 버전을 폭넓게 지원
- 플러그인 방식의 확장 가능한 아키텍처

## 문서

> 📖 전체 문서(중국어): [저장소 문서](Doc)

## 라이선스

MIT License © 2002-2026 NewLife 팀

[GitHub](https://github.com/NewLifeX/NewLife.Map) | [Website](https://newlifex.com)
