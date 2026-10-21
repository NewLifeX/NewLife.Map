# NewLife.Map - Biblioteca unificada de mapas

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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Esta traducción puede estar desactualizada; la referencia es la [versión en inglés](README.md).

NewLife.Map es una biblioteca de componentes de mapas que unifica Baidu, AMap, Tencent, Tianditu y la plataforma empresarial de API de mapas tras una única interfaz. Oculta las diferencias entre proveedores y ofrece geocodificación, geocodificación inversa, cálculo de distancias, conversión de coordenadas y corte de circuito multi-clave.

Origen: https://github.com/NewLifeX/NewLife.Map  
NuGet: `NewLife.Map`  

## Estructura del repositorio
| Proyecto | Rol | Descripción |
| ---- | ---- | ---- |
| `NewLife.Map` | Biblioteca de componentes de mapas (paquete NuGet) | Interfaz unificada `IMap` + adaptadores multiproveedor + conversión de coordenadas + corte de circuito de claves |
| `MapApi` | Plataforma empresarial de API de mapas (servicio web) | Agregación multiproveedor + caché GeoHash multinivel + panel de administración Cube |
| `XUnitTest` | Pruebas unitarias | Pruebas de geocodificación / geocodificación inversa / distancia / conversión de coordenadas por proveedor (los casos de integración con servicios reales se omiten de forma predeterminada) |

## Documentación
- [Requisitos](Doc/需求文档.md) — visión, objetivos principales, requisitos funcionales, lista de pendientes
- [Lista de funciones](Doc/功能清单.md) — códigos de módulo/función con seguimiento tridimensional (implementación/pruebas/comentarios)
- [Arquitectura](Doc/架构设计.md) — capas, componentes, flujos clave y decisiones de diseño
- [Análisis competitivo](Doc/竞品分析.md) — matriz comparativa y análisis de brechas frente a Geocoding.net / ProjNET / envoltorios nacionales de un solo proveedor

## Características
- Interfaz unificada `IMap` que oculta las diferencias entre Baidu / AMap / Tencent / Tianditu / autoalojado empresarial (NewLifeMap); cambiar de proveedor no requiere cambios de código
- Geocodificación (dirección → coordenadas) / geocodificación inversa (coordenadas → dirección), fusionando automáticamente la información semántica de los POI para completar los componentes de la dirección
- Distancia de ruta / tiempo de conducción, con los parámetros de estrategia propios de cada proveedor abstraídos
- Consultas de divisiones administrativas (AMap / Tencent), búsqueda de POI / regiones (Baidu / Tianditu), ubicación por IP (Baidu / Tencent / resolución en línea de la plataforma)
- Conversión de sistemas de coordenadas: fórmulas sin conexión integradas para WGS84 / GCJ02 / BD09 + conversión por lotes en línea del proveedor (el mapeo especial from/to de Baidu está encapsulado)
- Rotación de múltiples claves con corte de circuito automático + recuperación programada; que una clave sea limitada o bloqueada no afecta a la disponibilidad general
- Limitación de velocidad por token bucket mediante `MaxQps` y estadísticas de solicitudes (total / tasa de éxito / latencia media), evitando que una clave sea bloqueada por solicitudes de alta frecuencia
- Lanzamiento opcional de excepciones mediante `ThrowException`, lo que facilita el tratamiento unificado en capas superiores
- Varios marcos de destino: net45 / net461 / netstandard2.0 / netstandard2.1 (compatible con proyectos heredados y .NET moderno)
- Firmado con nombre seguro, apto para reflexión y sistemas de complementos
- Integración con el registro Stardust: el proveedor `NewLifeMap` descubre automáticamente las direcciones de los servidores, lo que permite cambiar sin fisuras entre múltiples instancias / despliegues azul-verde

## Instalación
```bash
# Estable
dotnet add package NewLife.Map
# Vista previa (basada en fecha)
dotnet add package NewLife.Map --prerelease
```

## Inicio rápido
```csharp
using NewLife.Map;
using NewLife.Data;

// Elija un proveedor concreto (BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap)
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // Separe varias claves con comas

// 1. Geocodificación directa: dirección -> coordenadas
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. Geocodificación inversa: coordenadas -> dirección
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. Distancia (conducción)
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. Conversión de coordenadas (fórmulas sin conexión)
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> Las coordenadas deben usar coordenadas esféricas (longitud/latitud); la salida es un punto en el sistema de coordenadas BD09.

## Creación dinámica mediante la fábrica
```csharp
var m1 = Map.Create("Baidu");           // Coincide automáticamente con el sufijo *Map
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// O crear mediante enumeración
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` se asigna después de la instanciación; varias claves se separan con comas y rotan internamente.

## Proveedores de mapas
| Proveedor | Clase | Capacidades principales | Notas |
| ------ | ---- | -------- | ---- |
| Mapas Baidu | `BaiduMap` | Geocodificación / geocodificación inversa / distancia / búsqueda de POI / ubicación por IP / conversión de coordenadas (en línea + sin conexión) | `KeyName=ak` |
| AMap | `AMap` | Geocodificación / geocodificación inversa / distancia / divisiones administrativas | `KeyName=key` |
| Mapas Tencent | `WeMap` | Geocodificación / geocodificación inversa / distancia / ubicación por IP / divisiones administrativas | `KeyName=key` |
| Tianditu | `TianDiTu` | Geocodificación / geocodificación inversa / distancia (con puntos de paso) / búsqueda de POI | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | Geocodificación / geocodificación inversa (código abierto, gratuito, sin clave, WGS84) | No requiere clave |
| Plataforma NewLife Map | `NewLifeMap` | API de agregación empresarial (se conecta a la plataforma MapApi) | Descubrimiento de servicios mediante Stardust |

> Para añadir un proveedor, consulte «Ampliar un nuevo proveedor de mapas» más abajo y siga el patrón de AMap/BaiduMap.

## Conversión de sistemas de coordenadas
| Nombre | Descripción | Proveedores habituales |
| ---- | -------- | ---- |
| WGS84 | Estándar GPS internacional | Posicionamiento bruto del dispositivo |
| GCJ02 | Desplazamiento aplicado por la Oficina Estatal de Topografía y Cartografía | AMap / Tencent / la mayoría de los mapas en línea chinos |
| BD09 | Desplazamiento de Baidu | Baidu |

```csharp
// Conversión por fórmula sin conexión, no consume cuota del proveedor
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// Conversión por lotes en línea de Baidu, los índices from/to ya están encapsulados
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## Rotación de múltiples AppKey y corte de circuito
- Pase `AppKey="k1,k2,k3"`; cada solicitud incrementa un índice y toma el resto para rotar
- Cuando una respuesta coincide con palabras clave de invalidación (como `TOO_FREQUENT` / `LIMIT` / `INVALID`), se llama a `RemoveKey()` para retirar temporalmente esa clave
- Las claves retiradas entran en `_pendingKeys` y un temporizador `TimerX` las revisa para restaurarlas automáticamente al vencer
- Política de tiempo de recuperación: actualmente la proporciona quien llama (por ejemplo, 1 hora)

## Plataforma de agregación NewLifeMap
`NewLifeMap` apunta a la plataforma integrada `MapApi` de API de mapas empresarial. La plataforma agrega varios proveedores y almacena en caché los resultados, de modo que los clientes no necesitan conocer las diferencias subyacentes.

```csharp
// Opción 1: especificar el servidor directamente
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// Opción 2: inyección de dependencias + resolución automática de direcciones con el registro Stardust
services.AddSingleton<IMap, NewLifeMap>();
// El constructor NewLifeMap(IServiceProvider) intenta: IRegistry -> StarFactory
```

- El `ApiHttpClient` vinculado actualiza automáticamente la lista de direcciones de servidores, lo que permite cambiar sin fisuras durante despliegues de múltiples réplicas / azul-verde de MapApi
- La plataforma expone los puntos finales unificados `GET /Map/Geo`, `/Map/ReverseGeo` (geocodificación inversa), `/Map/GeoAddress` (geocodificación directa) y `/Map/Distance` (distancia); una caché GeoHash de cuatro niveles (6–9 caracteres) reduce las llamadas a los proveedores

## Tratamiento de errores
- Predeterminado: una respuesta no satisfactoria no lanza excepción; el método devuelve `null`
- Con `map.ThrowException = true;` se lanza una `Exception` para capturarla de forma unificada en la capa externa
- Información de depuración de la última solicitud: `LastUrl` / `LastString` / `LastResult` / `LastKey`

## Resumen de la API
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
Extensiones (específicas del proveedor):
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // Búsqueda de POI / regiones de BaiduMap / Tianditu
Task<IList<GeoArea>> GetAreaAsync(...);       // Divisiones administrativas de AMap
Task<IDictionary<string, object?>> IpLocationAsync(...); // Ubicación por IP de BaiduMap / WeMap
```

## Ampliar un nuevo proveedor de mapas
1. Cree una clase `XXXMap : Map, IMap`
2. Configure `Server` / `KeyName`
3. Sobrescriba `InvokeAsync<T>` para analizar la estructura de respuesta unificada y los códigos de error del proveedor
4. Implemente `GetGeoAsync` / `GetReverseGeoAsync` / otras capacidades necesarias
5. Si hay señales propias del proveedor de limitación de velocidad / invalidación, sobrescriba `IsValidKey` para devolver true y activar la retirada temporal
6. Regístrelo en el constructor estático de `MapFactory` con `Register<XXXMap>(MapKinds.XXX)`
7. Envíe pruebas unitarias que cubran la funcionalidad principal y los casos límite

## Pruebas unitarias
El repositorio contiene el proyecto `XUnitTest`; la solución es `NewLife.Map.slnx` (nuevo formato XML de VS 2026 / .NET 10):
- De forma predeterminada se ejecutan **pruebas deterministas**: 84 casos; un servicio stub HTTP local cubre la construcción de URL y el análisis de respuestas de cada proveedor; la ejecución tarda entre 0,4 y 1 segundo y funciona de forma estable en todos los RID (incluida la CI)
- Las pruebas en línea de proveedores reales / OSM / plataforma están marcadas como `Category=Integration` y se **omiten de forma predeterminada** mediante `IntegrationFact`; «Ejecutar todo» en el Explorador de pruebas de VS se comporta igual que `dotnet test`
- Para habilitar las pruebas de integración: primero defina la variable de entorno `NEWLIFE_MAP_INTEGRATION=1` y luego ejecute con `--filter "Category=Integration"`
- Guarde las claves reales en secretos de usuario locales o variables de entorno; nunca las confirme en el repositorio

Ejecución (Windows PowerShell):
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # Opcional: integración en entorno real
```

## Compilación
```bash
dotnet build -c Release
```
La salida (compilación multiframework) se encuentra en `Bin/`.

## Pruebas de compatibilidad (CI)
Se activan en push / PR y cubren los RID principales:
- Matriz de pruebas unitarias: win-x64 / linux-x64 / linux-arm64 / linux-la64 (LoongArch, emulación QEMU + SDK .NET de Loongson, véase `test.yml`)
- Matriz de humo NativeAOT: los mismos cuatro RID (proyecto `AotSmoke`, 17 escenarios), véase `aot.yml`

## Hoja de ruta / TODO
> Todo completado el 2026-08-04: limitación de velocidad concurrente (MAP-12), métricas de fallos (MAP-13), comparación de rendimiento (MAP-14)

## Versionado
Versión mayor = cambios de compatibilidad; versión menor = nuevas funciones; revisión = correcciones / optimizaciones. `VersionSuffix` incluye una fecha para identificar fácilmente la compilación.

## Contribuir
Se aceptan issues / PR: errores, optimizaciones de rendimiento, soporte de más mapas, documentación y pruebas. Antes de enviar, lea `.github/copilot-instructions.md` para mantener un estilo de código coherente.

## Licencia
MIT; conserve el aviso de copyright y podrá usarlo libremente en proyectos comerciales / de código abierto.

---

## Mapa del ecosistema

NewLife es un ecosistema por capas de proyectos de código abierto con versionado independiente. El mapa siguiente se comparte literalmente en todos los repositorios de NewLife; cada proyecto enlaza a su repositorio y a su paquete principal de NuGet.

| Proyecto | Qué hace | Descargas |
| --- | --- | --- |
| **Fundación** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Registro de logs, configuración, caché, red, serialización, programación de tareas y seguimiento APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Middleware de datos: tablas únicas a escala de decenas de miles de millones, particionado automático, separación lectura/escritura y más de 10 bases de datos. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Biblioteca de red de alto rendimiento: 1,77 M pps / 14,5 Gbps en una máquina (TCP 1KB, 2026-10), 4 M conexiones TCP persistentes. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Framework de comunicación por protocolo y RPC para aplicaciones cliente/servidor y dispositivos IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Instala aplicaciones .NET como demonios del sistema operativo: servicios de Windows y systemd de Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Plantillas de proyecto `dotnet new` para consola, web, API, servicio, datos e IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Datos y mensajería** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Cliente Redis con latencia de microsegundos, millones de operaciones por segundo y colas de mensajes integradas. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Cliente RocketMQ totalmente administrado para Apache RocketMQ y Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Cliente y servidor MQTT, incluida la conectividad con Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Base de datos híbrida en C#: embebida o en red, relacional, series temporales, cola de mensajes y KV en una sola. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Controlador y capa de ayuda de MySql construidos sobre la pila de datos de NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, IA y distribuido** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Framework rápido para plataformas de administración: permisos, SSO, servidor OAuth y andamiaje CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Plataforma distribuida de servicios: gestión de nodos, APM, centro de configuración, registro y centro de despliegues. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plataforma de tareas distribuidas y programación de datos en tiempo real con particionado, reintentos y relleno histórico. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Cliente unificado para 46 proveedores de LLM más ChatAI, una aplicación web de chat lista para usar. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industrial e IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Biblioteca estándar de IoT que define contratos de protocolo y abstracciones de dispositivos. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Implementación de Modbus TCP/RTU/ASCII sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Protocolo para PLC Siemens sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Protocolo para PLC Omron sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Protocolo para PLC Mitsubishi Melsec sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Cliente OPC para el acceso a datos industriales. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Pila de protocolo BACnet para la automatización de edificios. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Plataforma IoT de código abierto y ligera con gestión de pasarelas y dispositivos. | — |
| **Telemática vehicular** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Protocolo de posicionamiento vehicular JT/T 808: más de 70 cuerpos de mensaje, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Protocolo plataforma a plataforma JT/T 809 para autoridades de transporte. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Vídeo a bordo JT/T 1078: análisis RTP, remultiplexado FLV/fMP4 y transmisión en directo. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Componentes de dominio** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Un solo paquete para Excel, Word, PPT, PDF y más de 15 formatos de documento. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Audio multiplataforma: códecs, E/S de dispositivos, DSP, contenedores y streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Códecs de vídeo, formatos de contenedor, procesamiento de fotogramas y streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Interfaz unificada sobre Baidu, AMap, Tencent y Tianditu con geocodificación y conversión de coordenadas. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Geolocalización IPv4 con una base de datos offline mapeada en memoria y una API en línea de respaldo. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Herramientas y productos** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Caja de herramientas del desarrollador: red, puerto serie, criptografía, expresiones regulares, Modbus y MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Sistema de gestión de código fuente Git autoalojado. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Instala automáticamente el runtime de .NET correspondiente cuando se ejecuta desde un host más antiguo. | — |

> Las insignias muestran el paquete principal de cada proyecto; los paquetes de temas, extensiones y satélites figuran en sus páginas de NuGet.Los más de 30 paquetes no listados aquí (FTP, LoRa, extensiones de Office, controladores de protocolo, etc.) están disponibles en la [organización NewLifeX](https://github.com/NewLifeX) y en [newlifex.com](https://newlifex.com).

## Equipo

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

NewLife se fundó en 2002 y lleva más de dos décadas dedicado al ecosistema .NET e IoT, mantiene cerca de 100 proyectos de código abierto (34 de ellos principales) y ofrece soluciones integradas de software y hardware para los sectores de energía, logística, salud, control industrial e Internet. Nos guía el lema «en el aprendizaje no hay antigüedad: quien domina es maestro».

Los componentes de código abierto acumulan <!--nuget-total:start-->6.48M<!--nuget-total:end--> descargas en NuGet (todo el ecosistema, a fecha de 2026-10).

Sitio web: https://newlifex.com · Organización de código abierto: https://github.com/NewLifeX

<details>
<summary>Cuenta oficial de WeChat: 智能大石头 (escanea para seguir)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

