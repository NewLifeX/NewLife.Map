# NewLife.Map - Bibliothèque de cartes unifiée

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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Cette traduction peut être en retard sur la [version anglaise](README.md), qui fait référence.

NewLife.Map est une bibliothèque de composants cartographiques qui unifie Baidu, AMap, Tencent, Tianditu et la plateforme d'API cartographique d'entreprise derrière une interface unique. Elle masque les différences entre fournisseurs et fournit le géocodage, le géocodage inverse, le calcul de distance, la conversion de coordonnées et le disjoncteur multi-clés.

Source : https://github.com/NewLifeX/NewLife.Map  
NuGet : `NewLife.Map`  

## Organisation du dépôt
| Projet | Rôle | Description |
| ---- | ---- | ---- |
| `NewLife.Map` | Bibliothèque de composants cartographiques (package NuGet) | Interface `IMap` unifiée + adaptateurs multi-fournisseurs + conversion de coordonnées + disjoncteur de clés |
| `MapApi` | Plateforme d'API cartographique d'entreprise (service web) | Agrégation multi-fournisseurs + cache GeoHash multiniveau + backend d'administration Cube |
| `XUnitTest` | Tests unitaires | Tests de géocodage / géocodage inverse / distance / conversion de coordonnées par fournisseur (les cas d'intégration avec les services réels sont ignorés par défaut) |

## Documentation
- [Exigences](Doc/需求文档.md) — vision, objectifs clés, exigences fonctionnelles, liste des reports
- [Liste des fonctionnalités](Doc/功能清单.md) — codes de module/fonction avec suivi tridimensionnel (implémentation/tests/commentaires)
- [Architecture](Doc/架构设计.md) — couches, composants, flux clés et décisions de conception
- [Analyse concurrentielle](Doc/竞品分析.md) — matrice comparative et analyse des écarts face à Geocoding.net / ProjNET / les wrappers nationaux mono-fournisseur

## Fonctionnalités
- Interface `IMap` unifiée qui masque les différences entre Baidu / AMap / Tencent / Tianditu / l'auto-hébergement d'entreprise (NewLifeMap) ; changer de fournisseur ne nécessite aucune modification de code
- Géocodage (adresse → coordonnées) / géocodage inverse (coordonnées → adresse), avec fusion automatique des informations sémantiques des POI pour compléter les composants d'adresse
- Distance d'itinéraire / temps de conduite, avec les paramètres de stratégie propres à chaque fournisseur abstraits
- Requêtes de divisions administratives (AMap / Tencent), recherche de POI / de zones (Baidu / Tianditu), localisation par IP (Baidu / Tencent / résolution en ligne de la plateforme)
- Conversion de systèmes de coordonnées : formules hors ligne intégrées pour WGS84 / GCJ02 / BD09 + conversion par lots en ligne du fournisseur (le mapping from/to spécifique à Baidu est encapsulé)
- Rotation de plusieurs clés avec disjoncteur automatique + rétablissement programmé ; qu'une clé soit limitée ou bannie n'affecte pas la disponibilité globale
- Limitation de débit par token bucket via `MaxQps` et statistiques de requêtes (total / taux de réussite / latence moyenne), évitant qu'une clé soit bannie par des requêtes à haute fréquence
- Levée d'exception facultative via `ThrowException`, facilitant un traitement unifié dans les couches supérieures
- Plusieurs frameworks cibles : net45 / net461 / netstandard2.0 / netstandard2.1 (compatible avec les projets hérités et le .NET moderne)
- Signé avec un nom fort, adapté à la réflexion et aux systèmes de plugins
- Intégration du registre Stardust : le fournisseur `NewLifeMap` découvre automatiquement les adresses des serveurs, permettant un basculement transparent entre plusieurs instances / déploiements bleu-vert

## Installation
```bash
# Stable
dotnet add package NewLife.Map
# Aperçu (basé sur la date)
dotnet add package NewLife.Map --prerelease
```

## Démarrage rapide
```csharp
using NewLife.Map;
using NewLife.Data;

// Choisissez un fournisseur concret (BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap)
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // Séparez plusieurs clés par des virgules

// 1. Géocodage direct : adresse -> coordonnées
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. Géocodage inverse : coordonnées -> adresse
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. Distance (conduite)
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. Conversion de coordonnées (formules hors ligne)
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> Les coordonnées doivent être des coordonnées sphériques (longitude/latitude) ; la sortie est un point dans le système de coordonnées BD09.

## Création dynamique via la fabrique
```csharp
var m1 = Map.Create("Baidu");           // Correspond automatiquement au suffixe *Map
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// Ou créer via l'énumération
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` est défini après l'instanciation ; plusieurs clés sont séparées par des virgules et alternées en interne.

## Fournisseurs de cartes
| Fournisseur | Classe | Capacités principales | Remarques |
| ------ | ---- | -------- | ---- |
| Cartes Baidu | `BaiduMap` | Géocodage / géocodage inverse / distance / recherche de POI / localisation IP / conversion de coordonnées (en ligne + hors ligne) | `KeyName=ak` |
| AMap | `AMap` | Géocodage / géocodage inverse / distance / divisions administratives | `KeyName=key` |
| Cartes Tencent | `WeMap` | Géocodage / géocodage inverse / distance / localisation IP / divisions administratives | `KeyName=key` |
| Tianditu | `TianDiTu` | Géocodage / géocodage inverse / distance (avec points de passage) / recherche de POI | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | Géocodage / géocodage inverse (open source, gratuit, sans clé, WGS84) | Aucune clé requise |
| Plateforme NewLife Map | `NewLifeMap` | API d'agrégation d'entreprise (se connecte à la plateforme MapApi) | Découverte de services via Stardust |

> Pour ajouter un fournisseur, consultez « Étendre un nouveau fournisseur de cartes » ci-dessous et suivez le modèle AMap/BaiduMap.

## Conversion des systèmes de coordonnées
| Nom | Description | Fournisseurs courants |
| ---- | -------- | ---- |
| WGS84 | Norme GPS internationale | Positionnement brut des appareils |
| GCJ02 | Décalage appliqué par le Bureau national de topographie et de cartographie | AMap / Tencent / la plupart des cartes en ligne chinoises |
| BD09 | Décalage Baidu | Baidu |

```csharp
// Conversion par formule hors ligne, sans consommer de quota fournisseur
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// Conversion par lots en ligne de Baidu, les indices from/to sont déjà encapsulés
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## Rotation des AppKey multiples et disjoncteur
- Passez `AppKey="k1,k2,k3"` ; chaque requête incrémente un index et prend le reste pour alterner
- Lorsqu'une réponse correspond à des mots-clés d'invalidation (tels que `TOO_FREQUENT` / `LIMIT` / `INVALID`), `RemoveKey()` est appelé pour retirer temporairement cette clé
- Les clés retirées entrent dans `_pendingKeys` et sont balayées par un minuteur `TimerX`, puis rétablies automatiquement à échéance
- Politique de temps de rétablissement : actuellement fournie par l'appelant (par exemple 1 heure)

## Plateforme d'agrégation NewLifeMap
`NewLifeMap` pointe vers la plateforme intégrée `MapApi` d'API cartographique d'entreprise. La plateforme agrège plusieurs fournisseurs et met en cache les résultats, afin que les clients n'aient pas à connaître les différences sous-jacentes.

```csharp
// Option 1 : spécifier directement le serveur
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// Option 2 : injection de dépendances + résolution automatique des adresses via le registre Stardust
services.AddSingleton<IMap, NewLifeMap>();
// Le constructeur NewLifeMap(IServiceProvider) essaie : IRegistry -> StarFactory
```

- L'`ApiHttpClient` lié met automatiquement à jour la liste des adresses de serveurs, permettant un basculement transparent lors des déploiements MapApi à plusieurs répliques / bleu-vert
- La plateforme expose les points de terminaison unifiés `GET /Map/Geo`, `/Map/ReverseGeo` (géocodage inverse), `/Map/GeoAddress` (géocodage direct) et `/Map/Distance` (distance) ; un cache GeoHash à quatre niveaux (6–9 caractères) réduit les appels aux fournisseurs

## Gestion des erreurs
- Par défaut : une réponse non réussie ne lève pas d'exception ; la méthode renvoie `null`
- Avec `map.ThrowException = true;`, une `Exception` est levée pour une capture unifiée dans la couche externe
- Informations de débogage de la dernière requête : `LastUrl` / `LastString` / `LastResult` / `LastKey`

## Aperçu de l'API
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
Extensions (spécifiques au fournisseur) :
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // Recherche POI / zone de BaiduMap / Tianditu
Task<IList<GeoArea>> GetAreaAsync(...);       // Divisions administratives d'AMap
Task<IDictionary<string, object?>> IpLocationAsync(...); // Localisation IP de BaiduMap / WeMap
```

## Étendre un nouveau fournisseur de cartes
1. Créez une classe `XXXMap : Map, IMap`
2. Définissez `Server` / `KeyName`
3. Remplacez `InvokeAsync<T>` pour analyser la structure de réponse unifiée et les codes d'erreur du fournisseur
4. Implémentez `GetGeoAsync` / `GetReverseGeoAsync` / les autres capacités requises
5. S'il existe des signaux propres au fournisseur de limitation de débit / d'invalidation, remplacez `IsValidKey` pour retourner true et déclencher le retrait temporaire
6. Enregistrez-le dans le constructeur statique de `MapFactory` avec `Register<XXXMap>(MapKinds.XXX)`
7. Soumettez des tests unitaires couvrant les fonctionnalités principales et les cas limites

## Tests unitaires
Le dépôt contient le projet `XUnitTest` ; la solution est `NewLife.Map.slnx` (nouveau format XML de VS 2026 / .NET 10) :
- Par défaut, il exécute des **tests déterministes** : 84 cas ; un service stub HTTP local couvre la construction d'URL et l'analyse des réponses de chaque fournisseur ; l'exécution prend environ 0,4 à 1 seconde et fonctionne de manière fiable sur tous les RID (y compris la CI)
- Les tests en ligne avec de vrais fournisseurs / OSM / la plateforme sont marqués `Category=Integration` et **ignorés par défaut** via `IntegrationFact` ; « Exécuter tout » dans l'Explorateur de tests VS se comporte comme `dotnet test`
- Pour activer les tests d'intégration : définissez d'abord la variable d'environnement `NEWLIFE_MAP_INTEGRATION=1`, puis exécutez avec `--filter "Category=Integration"`
- Conservez les vraies clés dans des secrets utilisateur locaux ou des variables d'environnement ; ne les validez jamais dans le dépôt

Exécution (Windows PowerShell) :
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # Facultatif : intégration en environnement réel
```

## Compilation
```bash
dotnet build -c Release
```
La sortie (build multi-frameworks) se trouve dans `Bin/`.

## Tests de compatibilité (CI)
Déclenchés à chaque push / PR, couvrant les principaux RID :
- Matrice de tests unitaires : win-x64 / linux-x64 / linux-arm64 / linux-la64 (LoongArch, émulation QEMU + SDK .NET Loongson, voir `test.yml`)
- Matrice de tests de fumée NativeAOT : les mêmes quatre RID (projet `AotSmoke`, 17 scénarios), voir `aot.yml`

## Feuille de route / TODO
> Tout terminé le 2026-08-04 : limitation de débit concurrente (MAP-12), métriques d'échec (MAP-13), comparaison de performances (MAP-14)

## Gestion des versions
Version majeure = changements de compatibilité ; version mineure = nouvelles fonctionnalités ; révision = correctifs / optimisations. `VersionSuffix` contient une date pour identifier facilement la build.

## Contribution
Les issues / PR sont les bienvenues : bogues, optimisations de performances, prise en charge de nouvelles cartes, documentation et tests. Avant de soumettre, veuillez lire `.github/copilot-instructions.md` afin de conserver un style de code cohérent.

## Licence
MIT ; conservez la mention de copyright et vous êtes libre de l'utiliser dans des projets commerciaux / open source.

---

## Carte de l'écosystème

NewLife est un écosystème en couches de projets open source versionnés indépendamment. La carte ci-dessous est reprise à l'identique par tous les dépôts NewLife ; chaque projet renvoie à son dépôt et à son paquet NuGet principal.

| Projet | Rôle | Téléchargements |
| --- | --- | --- |
| **Fondation** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Journalisation, configuration, cache, réseau, sérialisation, ordonnancement et traçage APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Intergiciel de données : tables uniques à l'échelle de dizaines de milliards, partitionnement automatique, séparation lecture/écriture et plus de 10 bases de données. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Bibliothèque réseau hautes performances : 1,77 M pps / 14,5 Gbps sur une machine (TCP 1KB, 2026-10), 4 M connexions TCP persistantes. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Communication protocolaire et framework RPC pour applications client/serveur et appareils IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Installe des applications .NET comme démons système : services Windows et systemd Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Modèles de projet `dotnet new` pour console, web, API, service, données et IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Données et messagerie** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Client Redis avec une latence en microsecondes, des millions d'opérations/s et des files de messages intégrées. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Client RocketMQ entièrement managé pour Apache RocketMQ et Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Client et serveur MQTT, y compris la connectivité Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Base de données hybride en C# : embarquée ou en réseau, relationnelle, séries temporelles, file de messages et KV réunis. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Pilote et couche d'assistance MySql construits sur la pile de données NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, IA et distribué** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Framework rapide de plateforme d'administration : permissions, SSO, serveur OAuth et échafaudage CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Plateforme de services distribués : gestion des nœuds, APM, centre de configuration, registre et centre de publication. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plateforme de tâches distribuées et d'ordonnancement de données en temps réel avec partitionnement, reprises et backfill. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Client unifié pour 46 fournisseurs de LLM, plus ChatAI, une application web de chat prête à l'emploi. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industriel et IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Bibliothèque standard IoT définissant les contrats de protocole et les abstractions d'appareils. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Implémentation Modbus TCP/RTU/ASCII au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Protocole pour automate Siemens au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Protocole pour automate Omron au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Protocole pour automate Mitsubishi Melsec au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Client OPC pour l'accès aux données industrielles. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Pile de protocoles BACnet pour l'automatisation des bâtiments. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Plateforme IoT open source légère avec gestion des passerelles et des appareils. | — |
| **Télématique embarquée** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Protocole de positionnement de véhicules JT/T 808 : plus de 70 corps de message, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Protocole plateforme à plateforme JT/T 809 pour les autorités de transport. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Vidéo embarquée JT/T 1078 : analyse RTP, remuxage FLV/fMP4 et diffusion en direct. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Composants métier** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Un seul paquet pour Excel, Word, PPT, PDF et plus de 15 formats de documents. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Audio multiplateforme : codecs, E/S d'appareils, DSP, conteneurs et streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Codecs vidéo, formats de conteneurs, traitement des images et streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Interface unifiée pour Baidu, AMap, Tencent et Tianditu avec géocodage et conversion de coordonnées. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Géolocalisation IPv4 avec une base de données hors ligne mappée en mémoire et une API en ligne en secours. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Outils et produits** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Boîte à outils du développeur : réseau, port série, cryptographie, regex, Modbus et MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Système de gestion de code source Git auto-hébergé. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Installe automatiquement le runtime .NET correspondant lorsqu'il est lancé depuis un hôte plus ancien. | — |

> Les badges indiquent le paquet principal de chaque projet ; les paquets de thèmes, d'extensions et satellites sont répertoriés sur leurs pages NuGet.Les plus de 30 paquets non listés ici (FTP, LoRa, extensions Office, pilotes de protocoles, etc.) sont disponibles dans l'[organisation NewLifeX](https://github.com/NewLifeX) et sur [newlifex.com](https://newlifex.com).

## Équipe

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

NewLife a été fondée en 2002 et œuvre depuis plus de deux décennies dans l'écosystème .NET et IoT, maintient près de 100 projets open source (dont 34 principaux) et fournit des solutions matérielles et logicielles intégrées aux secteurs de l'énergie, de la logistique, de la santé, du contrôle industriel et d'Internet. Notre devise : « l'apprentissage ne connaît pas l'ancienneté — celui qui maîtrise devient maître ».

Les composants open source ont été téléchargés <!--nuget-total:start-->6.48M<!--nuget-total:end--> fois depuis NuGet (à l'échelle de l'écosystème, en date de 2026-10).

Site web : https://newlifex.com · Organisation open source : https://github.com/NewLifeX

<details>
<summary>Compte officiel WeChat : 智能大石头 (scannez pour suivre)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

