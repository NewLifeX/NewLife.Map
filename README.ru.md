# NewLife.Map - Унифицированная библиотека карт

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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Перевод может отставать от [английской версии](README.md); при расхождениях верна английская версия.

NewLife.Map — это библиотека компонентов карт, объединяющая Baidu, AMap, Tencent, Tianditu и корпоративную платформу Map API за единым интерфейсом. Она скрывает различия поставщиков и предоставляет геокодирование, обратное геокодирование, расчёт расстояний, преобразование координат и отказоустойчивость по нескольким ключам.

Источник: https://github.com/NewLifeX/NewLife.Map  
NuGet: `NewLife.Map`  

## Структура репозитория
| Проект | Роль | Описание |
| ---- | ---- | ---- |
| `NewLife.Map` | Библиотека компонентов карт (пакет NuGet) | Единый интерфейс `IMap` + адаптеры нескольких поставщиков + преобразование координат + отказоустойчивость ключей |
| `MapApi` | Корпоративная платформа Map API (веб-служба) | Агрегация нескольких поставщиков + многоуровневый кеш GeoHash + административная панель Cube |
| `XUnitTest` | Модульные тесты | Тесты геокодирования / обратного геокодирования / расстояний / преобразования координат для каждого поставщика (интеграционные тесты с реальными сервисами по умолчанию пропускаются) |

## Документация
- [Требования](Doc/需求文档.md) — концепция, ключевые цели, функциональные требования, список отложенного
- [Список функций](Doc/功能清单.md) — коды модулей/функций с трёхмерным отслеживанием статуса (реализация/тесты/комментарии)
- [Архитектура](Doc/架构设计.md) — слои, компоненты, ключевые потоки и проектные решения
- [Конкурентный анализ](Doc/竞品分析.md) — матрица сравнения и анализ пробелов с Geocoding.net / ProjNET / отечественными обёртками одного поставщика

## Возможности
- Единый интерфейс `IMap`, скрывающий различия между Baidu / AMap / Tencent / Tianditu / корпоративным самохостингом (NewLifeMap); смена поставщика не требует изменений кода
- Геокодирование (адрес → координаты) / обратное геокодирование (координаты → адрес) с автоматическим объединением семантической информации POI для дополнения компонентов адреса
- Расстояние по маршруту / время в пути, при этом стратегические параметры поставщиков абстрагированы
- Запросы административных делений (AMap / Tencent), поиск POI / регионов (Baidu / Tianditu), определение местоположения по IP (Baidu / Tencent / онлайн-разрешение платформы)
- Преобразование систем координат: встроенные офлайн-формулы для WGS84 / GCJ02 / BD09 + онлайн-пакетное преобразование поставщика (специфическое сопоставление from/to Baidu инкапсулировано)
- Ротация нескольких ключей с автоматическим отказом + плановое восстановление; ограничение или блокировка одного ключа не влияет на общую доступность
- Ограничение скорости по token bucket через `MaxQps` и статистика запросов (всего / доля успешных / средняя задержка), предотвращающие блокировку одного ключа высокочастотными запросами
- Необязательное выбрасывание исключений через `ThrowException`, упрощающее единую обработку на верхних уровнях
- Несколько целевых платформ: net45 / net461 / netstandard2.0 / netstandard2.1 (совместимо со старыми проектами и современным .NET)
- Подписано строгим именем, подходит для рефлексии и систем плагинов
- Встроенная локализация времени выполнения: 11 языков, английский является исходным и резервным; сообщения исключений, журналы и метаданные сборки переводятся по языковым таблицам времени компиляции
- Интеграция с реестром Stardust: поставщик `NewLifeMap` автоматически обнаруживает адреса серверов, обеспечивая бесшовное переключение между несколькими экземплярами / сине-зелёными развёртываниями

## Установка
```bash
# Стабильная версия
dotnet add package NewLife.Map
# Предварительная версия (на основе даты)
dotnet add package NewLife.Map --prerelease
```

## Быстрый старт
```csharp
using NewLife.Map;
using NewLife.Data;

// Выберите конкретного поставщика (BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap)
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // Несколько ключей разделяются запятыми

// 1. Прямое геокодирование: адрес -> координаты
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. Обратное геокодирование: координаты -> адрес
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. Расстояние (автомобильный маршрут)
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. Преобразование координат (офлайн-формулы)
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> Координаты должны быть сферическими (долгота/широта); результат — точка в системе координат BD09.

## Динамическое создание через фабрику
```csharp
var m1 = Map.Create("Baidu");           // Автоматически сопоставляет суффикс *Map
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// Или создание по перечислению
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` задаётся после создания экземпляра; несколько ключей разделяются запятыми и чередуются внутри.

## Поставщики карт
| Поставщик | Класс | Основные возможности | Примечания |
| ------ | ---- | -------- | ---- |
| Карты Baidu | `BaiduMap` | Геокодирование / обратное геокодирование / расстояние / поиск POI / определение по IP / преобразование координат (онлайн + офлайн) | `KeyName=ak` |
| AMap | `AMap` | Геокодирование / обратное геокодирование / расстояние / административные деления | `KeyName=key` |
| Карты Tencent | `WeMap` | Геокодирование / обратное геокодирование / расстояние / определение по IP / административные деления | `KeyName=key` |
| Tianditu | `TianDiTu` | Геокодирование / обратное геокодирование / расстояние (с промежуточными точками) / поиск POI | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | Геокодирование / обратное геокодирование (открытый исходный код, бесплатно, без ключа, WGS84) | Ключ не требуется |
| Платформа NewLife Map | `NewLifeMap` | Корпоративный агрегирующий API (подключается к платформе MapApi) | Обнаружение служб через Stardust |

> Чтобы добавить поставщика, см. раздел «Расширение нового поставщика карт» ниже и следуйте шаблону AMap/BaiduMap.

## Преобразование систем координат
| Название | Описание | Распространённые поставщики |
| ---- | -------- | ---- |
| WGS84 | Международный стандарт GPS | Сырые данные позиционирования устройств |
| GCJ02 | Смещение, применяемое Государственным управлением геодезии и картографии | AMap / Tencent / большинство китайских онлайн-карт |
| BD09 | Смещение Baidu | Baidu |

```csharp
// Офлайн-преобразование по формуле, не расходует квоту поставщика
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// Онлайн-пакетное преобразование Baidu, индексы from/to уже инкапсулированы
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## Ротация нескольких AppKey и отказоустойчивость
- Передайте `AppKey="k1,k2,k3"`; каждый запрос увеличивает индекс и берёт остаток для чередования
- Когда ответ соответствует ключевым словам недействительности (например, `TOO_FREQUENT` / `LIMIT` / `INVALID`), вызывается `RemoveKey()` для временного вывода этого ключа из работы
- Выведенные ключи попадают в `_pendingKeys`, проверяются таймером `TimerX` и автоматически восстанавливаются по наступлении срока
- Политика времени восстановления: в настоящее время задаётся вызывающей стороной (например, 1 час)

## Агрегирующая платформа NewLifeMap
`NewLifeMap` указывает на встроенную корпоративную платформу Map API `MapApi`. Платформа агрегирует нескольких поставщиков и кеширует результаты, поэтому клиентам не нужно знать о различиях нижнего уровня.

```csharp
// Вариант 1: указать сервер напрямую
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// Вариант 2: внедрение зависимостей + автоматическое разрешение адресов через реестр Stardust
services.AddSingleton<IMap, NewLifeMap>();
// Конструктор NewLifeMap(IServiceProvider) пытается: IRegistry -> StarFactory
```

- Привязанный `ApiHttpClient` автоматически обновляет список адресов серверов, обеспечивая бесшовное переключение при развёртывании MapApi с несколькими репликами / сине-зелёном развёртывании
- Платформа предоставляет единые конечные точки `GET /Map/Geo`, `/Map/ReverseGeo` (обратное геокодирование), `/Map/GeoAddress` (прямое геокодирование) и `/Map/Distance` (расстояние); четырёхуровневый кеш GeoHash (6–9 символов) сокращает вызовы поставщиков

## Обработка ошибок
- По умолчанию: неуспешный ответ не вызывает исключение; метод возвращает `null`
- При `map.ThrowException = true;` создаётся исключение `Exception` для единого перехвата на внешнем уровне
- Отладочная информация последнего запроса: `LastUrl` / `LastString` / `LastResult` / `LastKey`

## Обзор API
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
Расширения (зависят от поставщика):
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // Поиск POI / регионов BaiduMap / Tianditu
Task<IList<GeoArea>> GetAreaAsync(...);       // Административные деления AMap
Task<IDictionary<string, object?>> IpLocationAsync(...); // Определение по IP BaiduMap / WeMap
```

## Расширение нового поставщика карт
1. Создайте класс `XXXMap : Map, IMap`
2. Задайте `Server` / `KeyName`
3. Переопределите `InvokeAsync<T>` для разбора единой структуры ответа и кодов ошибок поставщика
4. Реализуйте `GetGeoAsync` / `GetReverseGeoAsync` / другие необходимые возможности
5. Если есть специфичные для поставщика сигналы ограничения скорости / недействительности, переопределите `IsValidKey`, чтобы вернуть true и запустить временный вывод из работы
6. Зарегистрируйте в статическом конструкторе `MapFactory` с помощью `Register<XXXMap>(MapKinds.XXX)`
7. Предоставьте модульные тесты, покрывающие основную функциональность и граничные случаи

## Модульные тесты
Репозиторий содержит проект `XUnitTest`; решение — `NewLife.Map.slnx` (новый XML-формат VS 2026 / .NET 10):
- По умолчанию выполняются **детерминированные тесты**: 84 случая; локальная HTTP-заглушка покрывает построение URL и разбор ответов каждого поставщика; выполнение занимает около 0,4–1 секунды и стабильно работает на всех RID (включая CI)
- Онлайн-тесты с реальными поставщиками / OSM / платформой помечены `Category=Integration` и **по умолчанию пропускаются** через `IntegrationFact`; «Запустить все» в обозревателе тестов VS ведёт себя так же, как `dotnet test`
- Чтобы включить интеграционные тесты: сначала задайте переменную среды `NEWLIFE_MAP_INTEGRATION=1`, затем запустите с `--filter "Category=Integration"`
- Храните реальные ключи в локальных пользовательских секретах или переменных среды; никогда не коммитьте их в репозиторий

Запуск (Windows PowerShell):
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # Необязательно: интеграция в реальной среде
```

## Сборка
```bash
dotnet build -c Release
```
Результат (мультифреймворковая сборка) находится в `Bin/`.

## Тесты совместимости (CI)
Запускаются при push / PR и охватывают основные RID:
- Матрица модульных тестов: win-x64 / linux-x64 / linux-arm64 / linux-la64 (LoongArch, эмуляция QEMU + .NET SDK Loongson, см. `test.yml`)
- Матрица smoke-тестов NativeAOT: те же четыре RID (проект `AotSmoke`, 17 сценариев), см. `aot.yml`

## План развития / TODO
> Всё завершено 2026-08-04: параллельное ограничение скорости (MAP-12), метрики сбоев (MAP-13), сравнение бенчмарков (MAP-14)

## Версионирование
Основная версия = изменения совместимости; дополнительная версия = новые функции; ревизия = исправления / оптимизации. `VersionSuffix` содержит дату для удобной идентификации сборки.

## Участие
Приветствуются issues / PR: ошибки, оптимизация производительности, поддержка новых карт, документация и тесты. Перед отправкой прочитайте `.github/copilot-instructions.md`, чтобы сохранить единый стиль кода.

## Лицензия
MIT; сохраните уведомление об авторских правах, и вы можете свободно использовать её в коммерческих / открытых проектах.

---

## Карта экосистемы

NewLife — это многоуровневая экосистема независимо версионируемых open-source проектов. Карта ниже дословно используется во всех репозиториях NewLife; каждый проект ссылается на свой репозиторий и свой основной пакет NuGet.

| Проект | Что делает | Загрузки |
| --- | --- | --- |
| **Базовый** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Логирование, конфигурация, кэширование, сеть, сериализация, планирование и APM-трассировка. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Промежуточное ПО данных: отдельные таблицы масштаба десятков миллиардов строк, автоматическое секционирование, разделение чтения и записи, более 10 баз данных. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Высокопроизводительная сетевая библиотека: 1,77M pps / 14,5 Гбит/с на машину (TCP 1KB, 2026-10), 4M долгоживущих TCP-соединений. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Протокольная связь и RPC-фреймворк для клиент-серверных приложений и IoT-устройств. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Устанавливает .NET-приложения как демоны ОС: службы Windows и systemd в Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Шаблоны проектов `dotnet new` для консоли, web, API, службы, данных и IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Данные и обмен сообщениями** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Клиент Redis с микросекундной задержкой, миллионами операций в секунду и встроенными очередями сообщений. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Полностью управляемый клиент RocketMQ для Apache RocketMQ и Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Клиент и сервер MQTT, включая подключение к Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Гибридная база данных на C#: встроенная или сетевая, реляционная, временных рядов, очередь сообщений и KV в одном. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Драйвер MySql и вспомогательный слой на основе стека данных NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, AI и распределённые системы** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Фреймворк быстрой разработки админ-платформ: права, SSO, сервер OAuth и CRUD-скаффолдинг. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Распределённая сервисная платформа: управление узлами, APM, центр конфигурации, реестр и центр публикации. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Распределённая платформа задач и планирования данных в реальном времени с секционированием, повторами и дозаполнением. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Единый клиент для 46 поставщиков LLM плюс ChatAI — готовое веб-приложение для чата. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Промышленность и IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Стандартная библиотека IoT, определяющая контракты протоколов и абстракции устройств. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Реализация Modbus TCP/RTU/ASCII поверх NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Протокол Siemens PLC поверх NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Протокол Omron PLC поверх NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Протокол Mitsubishi Melsec PLC поверх NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Клиент OPC для доступа к промышленным данным. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Стек протокола автоматизации зданий BACnet. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Лёгкая open-source платформа IoT с управлением шлюзами и устройствами. | — |
| **Телематика транспорта** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Протокол позиционирования транспорта JT/T 808: более 70 типов сообщений, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Межплатформенный протокол JT/T 809 для органов управления транспортом. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Автомобильное видео JT/T 1078: разбор RTP, ремультиплексирование в FLV/fMP4 и прямая трансляция. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Прикладные компоненты** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Один пакет для Excel, Word, PPT, PDF и более 15 форматов документов. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Кроссплатформенное аудио: кодеки, ввод-вывод устройств, DSP, контейнеры и потоковая передача. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Видеокодеки, форматы контейнеров, обработка кадров и потоковая передача. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Единый интерфейс к Baidu, AMap, Tencent и Tianditu с геокодированием и преобразованием координат. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Геолокация IPv4 с офлайн-базой, отображаемой в память, и онлайн-API как резервом. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Инструменты и продукты** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Инструменты разработчика: сеть, последовательный порт, шифрование, регулярные выражения, Modbus и MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Самостоятельно размещаемая система управления исходным кодом Git. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Автоматически устанавливает подходящую среду выполнения .NET при запуске на старом хосте. | — |

> Бейджи показывают основной пакет каждого проекта; пакеты тем, расширений и сателлитов перечислены на соответствующих страницах NuGet.Более 30 пакетов, не перечисленных здесь (FTP, LoRa, расширения Office, драйверы протоколов и другие), доступны в [организации NewLifeX](https://github.com/NewLifeX) и на [newlifex.com](https://newlifex.com).

## Команда

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

Команда NewLife основана в 2002 году и более двух десятилетий работает в экосистеме .NET и IoT, поддерживает около 100 проектов с открытым кодом (34 из них — основные) и предлагает интегрированные программно-аппаратные решения для энергетики, логистики, здравоохранения, промышленной автоматизации и интернет-отрасли. Мы следуем девизу «в учении нет старших — мастерство приносит уважение».

Open-source компоненты скачали <!--nuget-total:start-->6.48M<!--nuget-total:end--> раз с NuGet (по всей экосистеме, по состоянию на 2026-10).

Сайт: https://newlifex.com · Организация open source: https://github.com/NewLifeX

<details>
<summary>Официальный аккаунт WeChat: 智能大石头 (отсканируйте, чтобы подписаться)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

