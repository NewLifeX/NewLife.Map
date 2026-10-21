# NewLife.Map - مكتبة خرائط موحدة

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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md)
<!-- lang:end -->

> قد تتأخر هذه الترجمة عن [النسخة الإنجليزية](README.md) التي تُعد المرجع.

NewLife.Map مكتبة مكوّنات خرائط توحّد Baidu وAMap وTencent وTianditu ومنصّة واجهة الخرائط المؤسسية خلف واجهة واحدة. وهي تخفي اختلافات المزوّدين وتوفّر الترميز الجغرافي والترميز العكسي وحساب المسافة وتحويل الإحداثيات وقاطع الدائرة متعدد المفاتيح.

المصدر: https://github.com/NewLifeX/NewLife.Map  
NuGet: `NewLife.Map`  

## بنية المستودع
| المشروع | الدور | الوصف |
| ---- | ---- | ---- |
| `NewLife.Map` | مكتبة مكوّنات الخرائط (حزمة NuGet) | واجهة `IMap` موحّدة + محوّلات متعددة المزوّدين + تحويل الإحداثيات + قاطع دائرة المفاتيح |
| `MapApi` | منصّة واجهة الخرائط المؤسسية (خدمة ويب) | تجميع متعدد المزوّدين + ذاكرة GeoHash متعددة المستويات + لوحة إدارة Cube |
| `XUnitTest` | اختبارات الوحدة | اختبارات الترميز الجغرافي والترميز العكسي والمسافة وتحويل الإحداثيات لكل مزوّد (تُتجاوز حالات التكامل مع الخدمات الحقيقية افتراضيًا) |

## التوثيق
- [المتطلبات](Doc/需求文档.md) — الرؤية والأهداف الأساسية والمتطلبات الوظيفية وقائمة التأجيل
- [قائمة الميزات](Doc/功能清单.md) — رموز الوحدات والميزات مع تتبّع ثلاثي الأبعاد (التنفيذ/الاختبار/التعليقات)
- [البنية المعمارية](Doc/架构设计.md) — الطبقات والمكوّنات والتدفقات الأساسية وقرارات التصميم
- [التحليل التنافسي](Doc/竞品分析.md) — مصفوفة مقارنة وتحليل الفجوات مقابل Geocoding.net / ProjNET / الأغلفة المحلية لمزوّد واحد

## الميزات
- واجهة `IMap` موحّدة تخفي الفروق بين Baidu / AMap / Tencent / Tianditu / الاستضافة الذاتية للمؤسسات (NewLifeMap)؛ تغيير المزوّد لا يتطلّب أي تعديل في الشيفرة
- الترميز الجغرافي (العنوان → الإحداثيات) / الترميز العكسي (الإحداثيات → العنوان)، مع دمج معلومات POI الدلالية تلقائيًا لإكمال مكوّنات العنوان
- مسافة المسار / زمن القيادة، مع تجريد معاملات الاستراتيجية الخاصة بكل مزوّد
- استعلامات التقسيمات الإدارية (AMap / Tencent)، والبحث عن POI / المناطق (Baidu / Tianditu)، وتحديد الموقع عبر IP (Baidu / Tencent / الحل عبر الإنترنت للمنصّة)
- تحويل أنظمة الإحداثيات: صيغ دون اتصال مدمجة لـ WGS84 / GCJ02 / BD09 + تحويل مجمّع عبر الإنترنت لدى المزوّد (مخطط from/to الخاص بـ Baidu مُغلّف)
- تدوير مفاتيح متعددة مع قاطع دائرة تلقائي + استعادة مجدولة؛ تقييد مفتاح واحد أو حظره لا يؤثّر في التوفّر العام
- تحديد معدّل الطلبات عبر token bucket باستخدام `MaxQps` وإحصاءات الطلبات (الإجمالي / معدّل النجاح / متوسط التأخير)، لمنع حظر مفتاح واحد بسبب الطلبات عالية التردّد
- إمكانية إطلاق الاستثناءات عبر `ThrowException`، ما يسهّل المعالجة الموحّدة في الطبقات العليا
- عدة أطر مستهدفة: net45 / net461 / netstandard2.0 / netstandard2.1 (متوافقة مع المشاريع القديمة و.NET الحديثة)
- موقّعة باسم قوي، مناسبة للانعكاس وأنظمة الإضافات
- توطين مدمج في وقت التشغيل: 11 لغة مع الإنجليزية كمصدر واحتياطي؛ وتُترجم رسائل الاستثناءات والسجلات وبيانات تعريف التجميع عبر جداول لغات وقت الترجمة
- تكامل مع سجل Stardust: يكتشف مزوّد `NewLifeMap` عناوين الخوادم تلقائيًا، ما يتيح التبديل السلس بين نسخ متعددة / نشر أزرق-أخضر

## التثبيت
```bash
# الإصدار المستقر
dotnet add package NewLife.Map
# الإصدار التجريبي (قائم على التاريخ)
dotnet add package NewLife.Map --prerelease
```

## البدء السريع
```csharp
using NewLife.Map;
using NewLife.Data;

// اختر مزوّد خرائط محدّدًا (BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap)
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // افصل المفاتيح المتعددة بفواصل

// 1. الترميز الجغرافي المباشر: العنوان -> الإحداثيات
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. الترميز الجغرافي العكسي: الإحداثيات -> العنوان
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. المسافة (القيادة)
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. تحويل الإحداثيات (صيغ دون اتصال)
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> يجب استخدام الإحداثيات الكروية (خط الطول/خط العرض)؛ والمخرجات نقطة في نظام الإحداثيات BD09.

## الإنشاء الديناميكي عبر المصنع
```csharp
var m1 = Map.Create("Baidu");           // يطابق لاحقة *Map تلقائيًا
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// أو أنشئ عبر التعداد
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> يُضبط `AppKey` بعد إنشاء المثيل؛ وتُفصل المفاتيح المتعددة بفواصل وتُدوّر داخليًا.

## مزوّدو الخرائط
| المزوّد | الفئة | القدرات الرئيسية | ملاحظات |
| ------ | ---- | -------- | ---- |
| خرائط Baidu | `BaiduMap` | الترميز الجغرافي / العكسي / المسافة / بحث POI / تحديد الموقع عبر IP / تحويل الإحداثيات (عبر الإنترنت + دون اتصال) | `KeyName=ak` |
| AMap | `AMap` | الترميز الجغرافي / العكسي / المسافة / التقسيمات الإدارية | `KeyName=key` |
| خرائط Tencent | `WeMap` | الترميز الجغرافي / العكسي / المسافة / تحديد الموقع عبر IP / التقسيمات الإدارية | `KeyName=key` |
| Tianditu | `TianDiTu` | الترميز الجغرافي / العكسي / المسافة (مع نقاط المرور) / بحث POI | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | الترميز الجغرافي / العكسي (مفتوح المصدر، مجاني، بلا مفتاح، WGS84) | لا يتطلّب مفتاحًا |
| منصّة NewLife Map | `NewLifeMap` | واجهة تجميع مؤسسية (تتصل بمنصّة MapApi) | اكتشاف الخدمات عبر Stardust |

> لإضافة مزوّد، راجع «توسيع مزوّد خرائط جديد» أدناه واتبع نمط AMap/BaiduMap.

## تحويل أنظمة الإحداثيات
| الاسم | الوصف | المزوّدون الشائعون |
| ---- | -------- | ---- |
| WGS84 | معيار GPS الدولي | تحديد الموقع الخام للأجهزة |
| GCJ02 | إزاحة تطبّقها الهيئة الوطنية للمساحة والخرائط | AMap / Tencent / معظم الخرائط الصينية عبر الإنترنت |
| BD09 | إزاحة Baidu | Baidu |

```csharp
// تحويل بصيغة دون اتصال، لا يستهلك حصة المزوّد
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// تحويل مجمّع عبر الإنترنت من Baidu، قيم from/to مُغلّفة مسبقًا
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## تدوير مفاتيح AppKey المتعددة وقاطع الدائرة
- مرّر `AppKey="k1,k2,k3"`؛ يزيد كل طلب الفهرس ويأخذ الباقي للتدوير
- عند مطابقة ردّ ما لكلمات مفتاحية غير صالحة (مثل `TOO_FREQUENT` / `LIMIT` / `INVALID`)، يُستدعى `RemoveKey()` لسحب ذلك المفتاح مؤقتًا
- تدخل المفاتيح المسحوبة في `_pendingKeys` ويفحصها مؤقّت `TimerX` ثم تُستعاد تلقائيًا عند حلول موعدها
- سياسة زمن الاستعادة: يوفّرها المستدعي حاليًا (مثل ساعة واحدة)

## منصّة التجميع NewLifeMap
يشير `NewLifeMap` إلى منصّة `MapApi` المدمجة لواجهة الخرائط المؤسسية. تجمع المنصّة عدة مزوّدين وتخزّن النتائج مؤقتًا، فلا يحتاج العملاء إلى معرفة الفروق الأساسية.

```csharp
// الخيار 1: تحديد الخادم مباشرة
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// الخيار 2: حقن التبعيات + حلّ العناوين تلقائيًا عبر سجل Stardust
services.AddSingleton<IMap, NewLifeMap>();
// يحاول المُنشئ NewLifeMap(IServiceProvider): IRegistry -> StarFactory
```

- يحدّث `ApiHttpClient` المرتبط قائمة عناوين الخوادم تلقائيًا، ما يتيح التبديل السلس أثناء نشر MapApi بنسخ متعددة / نشر أزرق-أخضر
- توفّر المنصّة نقاط النهاية الموحّدة `GET /Map/Geo` و`/Map/ReverseGeo` (الترميز العكسي) و`/Map/GeoAddress` (الترميز المباشر) و`/Map/Distance` (المسافة)؛ وتقلّل ذاكرة GeoHash رباعية المستويات (6–9 أحرف) نداءات المزوّدين

## معالجة الأخطاء
- افتراضيًا: لا يطلق الردّ غير الناجح استثناءً؛ وتعيد الدالة `null`
- مع `map.ThrowException = true;` يُطلق استثناء `Exception` لالتقاطه موحّدًا في الطبقة الخارجية
- معلومات التصحيح لآخر طلب: `LastUrl` / `LastString` / `LastResult` / `LastKey`

## نظرة عامة على الواجهة
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
الامتدادات (خاصة بالمزوّد):
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // بحث POI / المناطق في BaiduMap / Tianditu
Task<IList<GeoArea>> GetAreaAsync(...);       // التقسيمات الإدارية في AMap
Task<IDictionary<string, object?>> IpLocationAsync(...); // تحديد الموقع عبر IP في BaiduMap / WeMap
```

## توسيع مزوّد خرائط جديد
1. أنشئ فئة `XXXMap : Map, IMap`
2. اضبط `Server` / `KeyName`
3. أعد تعريف `InvokeAsync<T>` لتحليل بنية الردّ الموحّدة ورموز الخطأ لدى المزوّد
4. نفّذ `GetGeoAsync` / `GetReverseGeoAsync` / القدرات المطلوبة الأخرى
5. إذا وُجدت إشارات خاصة بالمزوّد لتحديد المعدّل / الإبطال، فأعد تعريف `IsValidKey` لإرجاع true وتشغيل السحب المؤقّت
6. سجّله في المُنشئ الثابت لـ `MapFactory` باستخدام `Register<XXXMap>(MapKinds.XXX)`
7. قدّم اختبارات وحدة تغطّي الوظائف الأساسية والحالات الحدّية

## اختبارات الوحدة
يحتوي المستودع مشروع `XUnitTest`؛ والحلّ هو `NewLife.Map.slnx` (تنسيق XML الجديد في VS 2026 / .NET 10):
- يشغّل افتراضيًا **اختبارات حتمية**: 84 حالة؛ وتغطّي خدمة HTTP محلية وهمية بناء عناوين URL وتحليل الردود لكل مزوّد؛ ويستغرق التنفيذ نحو 0.4–1 ثانية ويعمل بثبات على جميع RID (بما في ذلك CI)
- تُعلَّم اختبارات المزوّدين الحقيقيين / OSM / المنصّة عبر الإنترنت بـ `Category=Integration` و**تُتجاوز افتراضيًا** عبر `IntegrationFact`؛ وسلوك «تشغيل الكل» في مستكشف الاختبارات في VS يطابق `dotnet test`
- لتمكين اختبارات التكامل: اضبط أولًا متغيّر البيئة `NEWLIFE_MAP_INTEGRATION=1` ثم شغّل مع `--filter "Category=Integration"`
- احفظ المفاتيح الحقيقية في أسرار المستخدم المحلية أو متغيّرات البيئة؛ ولا تُثبّتها في المستودع أبدًا

التشغيل (Windows PowerShell):
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # اختياري: تكامل في بيئة حقيقية
```

## البناء
```bash
dotnet build -c Release
```
توجد المخرجات (بناء متعدد الأطر) في `Bin/`.

## اختبارات التوافق (CI)
تُشغَّل عند push / PR وتغطّي RID الرئيسية:
- مصفوفة اختبارات الوحدة: win-x64 / linux-x64 / linux-arm64 / linux-la64 (LoongArch، محاكاة QEMU + .NET SDK من Loongson، انظر `test.yml`)
- مصفوفة اختبارات الدخان NativeAOT: نفس RID الأربعة (مشروع `AotSmoke`، 17 سيناريو)، انظر `aot.yml`

## خارطة الطريق / TODO
> اكتمل كل شيء في 2026-08-04: تحديد المعدّل المتزامن (MAP-12)، إحصاءات الأعطال (MAP-13)، مقارنة الأداء (MAP-14)

## إدارة الإصدارات
الإصدار الرئيسي = تغييرات التوافق؛ الإصدار الفرعي = ميزات جديدة؛ المراجعة = إصلاحات / تحسينات. ويحمل `VersionSuffix` تاريخًا لتسهيل التعرّف على البناء.

## المساهمة
نرحّب بـ issues / PR: الأخطاء وتحسينات الأداء ودعم مزيد من الخرائط والتوثيق والاختبارات. قبل الإرسال، اقرأ `.github/copilot-instructions.md` للحفاظ على اتساق نمط الشيفرة.

## الترخيص
MIT؛ احتفظ بإشعار حقوق النشر ويمكنك استخدامها بحرية في المشاريع التجارية / مفتوحة المصدر.

---

## خريطة المنظومة

NewLife منظومة متعدّدة الطبقات من مشاريع مفتوحة المصدر تُصدَر بإصدارات مستقلة. الخريطة أدناه مشتركة حرفيًا بين جميع مستودعات NewLife؛ وكل مشروع يرتبط بمستودعه وبحزمته الرئيسية على NuGet.

| المشروع | الوظيفة | التنزيلات |
| --- | --- | --- |
| **الأساس** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | التسجيل والتهيئة والتخزين المؤقت والشبكات والتسلسل والجدولة وتتبّع أداء APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | وسيط بيانات: جداول واحدة بمستوى عشرات المليارات من الصفوف، تقسيم تلقائي للجداول، فصل القراءة عن الكتابة، وأكثر من 10 قواعد بيانات. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | مكتبة شبكات عالية الأداء: 1.77 مليون pps / 14.5 Gbps على جهاز واحد (TCP 1KB، 2026-10)، و4 ملايين اتصال TCP طويل الأمد. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | إطار اتصال بروتوكولي وRPC لتطبيقات العميل/الخادم وأجهزة إنترنت الأشياء. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | يثبّت تطبيقات .NET كخدمات حارسة لنظام التشغيل: خدمات Windows وsystemd في Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | قوالب مشاريع `dotnet new` للطرفية وweb وAPI والخدمات والبيانات وإنترنت الأشياء. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **البيانات والمراسلة** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | عميل Redis بزمن استجابة بالميكروثانية، وملايين العمليات في الثانية، وقوائم رسائل مدمجة. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | عميل RocketMQ مُدار بالكامل لـ Apache RocketMQ وخدمة رسائل Alibaba Cloud. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | عميل وخادم MQTT، بما في ذلك الاتصال بـ Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | قاعدة بيانات هجينة بلغة C#: مدمجة أو شبكية، وعلائقية، وسلاسل زمنية، وقائمة رسائل، وKV في واحد. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | برنامج تشغيل MySql وطبقة مساعدة مبنية على مكدّس بيانات NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web والذكاء الاصطناعي والأنظمة الموزّعة** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | إطار منصة إدارة سريعة: الصلاحيات، والدخول الموحّد SSO، وخادم OAuth، وتوليد واجهات CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | منصة خدمات موزّعة: إدارة العقد، وAPM، ومركز الإعدادات، ومركز التسجيل، ومركز النشر. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | منصة موزّعة للمهام وجدولة البيانات الفورية، مع التقسيم وإعادة المحاولة وإعادة التعبئة. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | عميل موحّد لـ 46 مزوّد LLM بالإضافة إلى ChatAI، تطبيق ويب جاهز للمحادثة. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **الصناعة وإنترنت الأشياء** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | مكتبة معايير إنترنت الأشياء التي تعرّف عقود البروتوكولات وتجريدات الأجهزة. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | تنفيذ Modbus TCP/RTU/ASCII فوق NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | بروتوكول Siemens PLC فوق NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | بروتوكول Omron PLC فوق NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | بروتوكول Mitsubishi Melsec PLC فوق NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | عميل OPC للوصول إلى البيانات الصناعية. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | مكدّس بروتوكول BACnet لأتمتة المباني. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | منصة إنترنت أشياء مفتوحة المصدر وخفيفة، مع إدارة البوابات والأجهزة. | — |
| **تليماتيكا المركبات** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | بروتوكول تحديد مواقع المركبات JT/T 808: أكثر من 70 جسم رسالة، 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | بروتوكول JT/T 809 بين المنصات لجهات إدارة النقل. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | فيديو داخل المركبة JT/T 1078: تحليل RTP وإعادة التجميع إلى FLV/fMP4 والبث المباشر. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **مكوّنات المجال** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | حزمة واحدة لـ Excel وWord وPPT وPDF وأكثر من 15 صيغة مستندات. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | صوت عبر المنصات: مرمّزات، وإدخال/إخراج الأجهزة، وDSP، وحاويات، وبث. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | مرمّزات الفيديو وصيغ الحاويات ومعالجة الإطارات والبث. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | واجهة موحّدة فوق Baidu وAMap وTencent وTianditu مع الترميز الجغرافي وتحويل الإحداثيات. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | تحديد الموقع الجغرافي لعناوين IPv4 بقاعدة بيانات غير متصلة مُعيّنة في الذاكرة، وواجهة API عبر الإنترنت كبديل. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **الأدوات والمنتجات** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | صندوق أدوات المطوّر: الشبكات والمنفذ التسلسلي والتشفير والتعبيرات النمطية وModbus وMQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | نظام استضافة ذاتية لإدارة الشيفرة المصدرية Git. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | يثبّت بيئة تشغيل .NET المطابقة تلقائيًا عند التشغيل من مضيف أقدم. | — |

> تعرض الشارات الحزمة الرئيسية لكل مشروع؛ أما حزم السمات والإضافات والحزم التابعة فمذكورة في صفحاتها على NuGet.الحزم التي تتجاوز 30 حزمة وغير المذكورة هنا (FTP وLoRa وإضافات Office وبرامج تشغيل البروتوكولات وغيرها) متاحة في [منظمة NewLifeX](https://github.com/NewLifeX) وعلى [newlifex.com](https://newlifex.com).

## الفريق

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

تأسّست NewLife في عام 2002، وأمضت أكثر من عقدين في منظومة .NET وإنترنت الأشياء، وتدير ما يقارب 100 مشروع مفتوح المصدر (34 منها مشاريع رئيسية)، وتقدّم حلولًا متكاملة للبرمجيات والأجهزة لقطاعات الكهرباء والخدمات اللوجستية والرعاية الصحية والتحكّم الصناعي والإنترنت. ونلتزم بمبدأ "لا أسبقية في التعلّم — الإتقان يكسب الاحترام".

نُزّلت المكوّنات مفتوحة المصدر <!--nuget-total:start-->6.48M<!--nuget-total:end--> مرة من NuGet (على مستوى المنظومة بأكملها، حتى 2026-10).

الموقع: https://newlifex.com · المنظمة مفتوحة المصدر: https://github.com/NewLifeX

<details>
<summary>حساب WeChat الرسمي: 智能大石头 (امسح للاشتراك)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

