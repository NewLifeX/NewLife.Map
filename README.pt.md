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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

> Esta tradução pode estar desatualizada em relação à [versão em inglês](README.md), que é a referência.

NewLife.Map é uma biblioteca de componentes de mapas que unifica Baidu, AMap, Tencent, Tianditu e a plataforma empresarial de API de mapas por trás de uma única interface. Ela oculta as diferenças entre fornecedores e oferece geocodificação, geocodificação reversa, cálculo de distância, conversão de coordenadas e disjuntor de múltiplas chaves.

Origem: https://github.com/NewLifeX/NewLife.Map  
NuGet: `NewLife.Map`  

## Estrutura do repositório
| Projeto | Papel | Descrição |
| ---- | ---- | ---- |
| `NewLife.Map` | Biblioteca de componentes de mapas (pacote NuGet) | Interface unificada `IMap` + adaptadores multifornecedor + conversão de coordenadas + disjuntor de chaves |
| `MapApi` | Plataforma empresarial de API de mapas (serviço web) | Agregação multifornecedor + cache GeoHash multinível + painel administrativo Cube |
| `XUnitTest` | Testes unitários | Testes de geocodificação / geocodificação reversa / distância / conversão de coordenadas por fornecedor (os casos de integração com serviços reais são ignorados por padrão) |

## Documentação
- [Requisitos](Doc/需求文档.md) — visão, objetivos principais, requisitos funcionais, lista de adiamentos
- [Lista de recursos](Doc/功能清单.md) — códigos de módulo/recurso com rastreamento tridimensional (implementação/testes/comentários)
- [Arquitetura](Doc/架构设计.md) — camadas, componentes, fluxos principais e decisões de projeto
- [Análise competitiva](Doc/竞品分析.md) — matriz comparativa e análise de lacunas frente a Geocoding.net / ProjNET / wrappers nacionais de fornecedor único

## Recursos
- Interface unificada `IMap` que oculta as diferenças entre Baidu / AMap / Tencent / Tianditu / auto-hospedado corporativo (NewLifeMap); trocar de fornecedor não exige alterações de código
- Geocodificação (endereço → coordenadas) / geocodificação reversa (coordenadas → endereço), mesclando automaticamente as informações semânticas dos POIs para completar os componentes do endereço
- Distância da rota / tempo de direção, com os parâmetros de estratégia específicos de cada fornecedor abstraídos
- Consultas de divisões administrativas (AMap / Tencent), pesquisa de POI / região (Baidu / Tianditu), localização por IP (Baidu / Tencent / resolução on-line da plataforma)
- Conversão de sistemas de coordenadas: fórmulas off-line integradas para WGS84 / GCJ02 / BD09 + conversão em lote on-line do fornecedor (o mapeamento from/to especial do Baidu é encapsulado)
- Rotação de múltiplas chaves com disjuntor automático + recuperação programada; uma chave limitada ou banida não afeta a disponibilidade geral
- Limitação de taxa por token bucket via `MaxQps` e estatísticas de requisições (total / taxa de sucesso / latência média), evitando que uma chave seja banida por requisições de alta frequência
- Lançamento opcional de exceções via `ThrowException`, facilitando o tratamento unificado nas camadas superiores
- Vários frameworks de destino: net45 / net461 / netstandard2.0 / netstandard2.1 (compatível com projetos legados e .NET moderno)
- Assinado com nome forte, adequado para reflexão e sistemas de plugins
- Integração com o registro Stardust: o provedor `NewLifeMap` descobre automaticamente os endereços dos servidores, permitindo alternância transparente entre múltiplas instâncias / implantações azul-verde

## Instalação
```bash
# Estável
dotnet add package NewLife.Map
# Pré-visualização (baseada em data)
dotnet add package NewLife.Map --prerelease
```

## Início rápido
```csharp
using NewLife.Map;
using NewLife.Data;

// Escolha um fornecedor concreto (BaiduMap / AMap / WeMap / TianDiTu / NewLifeMap)
var map = new BaiduMap { AppKey = "<BAIDU_AK>" }; // Separe várias chaves por vírgulas

// 1. Geocodificação direta: endereço -> coordenadas
var geo = await map.GetGeoAsync("北京市海淀区上地十街10号", city: "北京", coordtype: "bd09ll", formatAddress: true);
Console.WriteLine($"Point: {geo?.Location}  Semantic: {geo?.Title}");

// 2. Geocodificação reversa: coordenadas -> endereço
var addr = await map.GetReverseGeoAsync(new GeoPoint(116.30815, 40.056885), "bd09ll");
Console.WriteLine(addr?.Address);

// 3. Distância (direção)
var d = await map.GetDistanceAsync(new GeoPoint(116.30815, 40.056885), new GeoPoint(116.39745, 39.909187), "bd09ll");
Console.WriteLine($"Distance={d?.Distance}m Duration={d?.Duration}s");

// 4. Conversão de coordenadas (fórmulas off-line)
var list = await map.ConvertAsync(new List<GeoPoint> { new(116.30815, 40.056885) }, from: "wgs84", to: "bd09");
Console.WriteLine(list[0]);
```
> As coordenadas devem usar coordenadas esféricas (longitude/latitude); a saída é um ponto no sistema de coordenadas BD09.

## Criação dinâmica pela fábrica
```csharp
var m1 = Map.Create("Baidu");           // Corresponde automaticamente ao sufixo *Map
var m2 = Map.Create("AMap");            // AMap
var m3 = Map.Create("NewLifeMap");

// Ou criar pela enumeração
var m4 = MapFactory.Create(MapKinds.TianDiTu);
```
> `AppKey` é definido após a instanciação; várias chaves são separadas por vírgulas e rotacionadas internamente.

## Provedores de mapas
| Provedor | Classe | Principais recursos | Observações |
| ------ | ---- | -------- | ---- |
| Mapas Baidu | `BaiduMap` | Geocodificação / geocodificação reversa / distância / pesquisa de POI / localização por IP / conversão de coordenadas (on-line + off-line) | `KeyName=ak` |
| AMap | `AMap` | Geocodificação / geocodificação reversa / distância / divisões administrativas | `KeyName=key` |
| Mapas Tencent | `WeMap` | Geocodificação / geocodificação reversa / distância / localização por IP / divisões administrativas | `KeyName=key` |
| Tianditu | `TianDiTu` | Geocodificação / geocodificação reversa / distância (com pontos de passagem) / pesquisa de POI | `KeyName=tk` |
| OSM/Nominatim | `NominatimMap` | Geocodificação / geocodificação reversa (open source, gratuito, sem chave, WGS84) | Nenhuma chave necessária |
| Plataforma NewLife Map | `NewLifeMap` | API de agregação empresarial (conecta-se à plataforma MapApi) | Descoberta de serviços via Stardust |

> Para adicionar um provedor, consulte «Estender um novo provedor de mapas» abaixo e siga o padrão AMap/BaiduMap.

## Conversão de sistemas de coordenadas
| Nome | Descrição | Provedores comuns |
| ---- | -------- | ---- |
| WGS84 | Padrão GPS internacional | Posicionamento bruto do dispositivo |
| GCJ02 | Deslocamento aplicado pelo Departamento Estatal de Levantamento e Cartografia | AMap / Tencent / a maioria dos mapas on-line chineses |
| BD09 | Deslocamento do Baidu | Baidu |

```csharp
// Conversão por fórmula off-line, sem consumir cota do provedor
var p = await map.ConvertAsync(new GeoPoint(lng, lat), from: "wgs84", to: "gcj02");

// Conversão em lote on-line do Baidu, índices from/to já encapsulados
var list = await map.ConvertAsync(points, from: "wgs84ll", to: "bd09ll");
```

## Rotação de múltiplos AppKey e disjuntor
- Passe `AppKey="k1,k2,k3"`; cada requisição incrementa um índice e usa o resto para rotacionar
- Quando uma resposta corresponde a palavras-chave de invalidação (como `TOO_FREQUENT` / `LIMIT` / `INVALID`), `RemoveKey()` é chamado para retirar temporariamente essa chave
- As chaves retiradas entram em `_pendingKeys` e são verificadas por um temporizador `TimerX`, sendo restauradas automaticamente no vencimento
- Política de tempo de recuperação: atualmente fornecida pelo chamador (por exemplo, 1 hora)

## Plataforma de agregação NewLifeMap
`NewLifeMap` aponta para a plataforma integrada `MapApi` de API de mapas empresarial. A plataforma agrega vários provedores e armazena os resultados em cache, de modo que os clientes não precisam conhecer as diferenças subjacentes.

```csharp
// Opção 1: especificar o servidor diretamente
var map = new NewLifeMap("http://mapapi.company.local") { AppKey = "<KEY>" };

// Opção 2: injeção de dependência + resolução automática de endereços pelo registro Stardust
services.AddSingleton<IMap, NewLifeMap>();
// O construtor NewLifeMap(IServiceProvider) tenta: IRegistry -> StarFactory
```

- O `ApiHttpClient` vinculado atualiza automaticamente a lista de endereços de servidores, permitindo alternância transparente durante implantações MapApi com múltiplas réplicas / azul-verde
- A plataforma expõe os endpoints unificados `GET /Map/Geo`, `/Map/ReverseGeo` (geocodificação reversa), `/Map/GeoAddress` (geocodificação direta) e `/Map/Distance` (distância); um cache GeoHash de quatro níveis (6–9 caracteres) reduz as chamadas aos provedores

## Tratamento de erros
- Padrão: uma resposta sem sucesso não lança exceção; o método retorna `null`
- Com `map.ThrowException = true;`, uma `Exception` é lançada para captura unificada na camada externa
- Informações de depuração da última requisição: `LastUrl` / `LastString` / `LastResult` / `LastKey`

## Visão geral da API
```csharp
Task<GeoAddress?> GetGeoAsync(string address, string? city=null, string? coordtype=null, bool formatAddress=false);
Task<GeoAddress?> GetReverseGeoAsync(GeoPoint point, string? coordtype);
Task<Driving?>    GetDistanceAsync(GeoPoint origin, GeoPoint destination, string? coordtype, int type=0);
Task<IList<GeoPoint>> ConvertAsync(IList<GeoPoint> points, string from, string to);
```
Extensões (específicas do provedor):
```csharp
Task<GeoAddress?> PlaceSearchAsync(...);      // Pesquisa de POI / região de BaiduMap / Tianditu
Task<IList<GeoArea>> GetAreaAsync(...);       // Divisões administrativas do AMap
Task<IDictionary<string, object?>> IpLocationAsync(...); // Localização por IP de BaiduMap / WeMap
```

## Estender um novo provedor de mapas
1. Crie uma classe `XXXMap : Map, IMap`
2. Defina `Server` / `KeyName`
3. Substitua `InvokeAsync<T>` para analisar a estrutura de resposta unificada e os códigos de erro do provedor
4. Implemente `GetGeoAsync` / `GetReverseGeoAsync` / outros recursos necessários
5. Se houver sinais específicos do provedor de limitação de taxa / invalidação, substitua `IsValidKey` para retornar true e acionar a retirada temporária
6. Registre no construtor estático de `MapFactory` com `Register<XXXMap>(MapKinds.XXX)`
7. Envie testes unitários cobrindo a funcionalidade principal e os casos limite

## Testes unitários
O repositório contém o projeto `XUnitTest`; a solução é `NewLife.Map.slnx` (novo formato XML do VS 2026 / .NET 10):
- Por padrão, executa **testes determinísticos**: 84 casos; um serviço stub HTTP local cobre a construção de URL e a análise de respostas de cada provedor; a execução leva cerca de 0,4–1 segundo e funciona de forma estável em todos os RIDs (incluindo CI)
- Os testes on-line com provedores reais / OSM / plataforma são marcados como `Category=Integration` e **ignorados por padrão** via `IntegrationFact`; «Executar tudo» no Gerenciador de Testes do VS se comporta como `dotnet test`
- Para habilitar os testes de integração: defina primeiro a variável de ambiente `NEWLIFE_MAP_INTEGRATION=1` e execute com `--filter "Category=Integration"`
- Mantenha as chaves reais em segredos de usuário locais ou variáveis de ambiente; nunca as confirme no repositório

Execução (Windows PowerShell):
```powershell
dotnet test -c Release
$env:NEWLIFE_MAP_INTEGRATION = 1
dotnet test -c Release --filter "Category=Integration"   # Opcional: integração em ambiente real
```

## Compilação
```bash
dotnet build -c Release
```
A saída (build multiframework) fica em `Bin/`.

## Testes de compatibilidade (CI)
Acionados em push / PR, cobrindo os principais RIDs:
- Matriz de testes unitários: win-x64 / linux-x64 / linux-arm64 / linux-la64 (LoongArch, emulação QEMU + SDK .NET Loongson, consulte `test.yml`)
- Matriz de smoke NativeAOT: os mesmos quatro RIDs (projeto `AotSmoke`, 17 cenários), consulte `aot.yml`

## Roteiro / TODO
> Tudo concluído em 2026-08-04: limitação de taxa concorrente (MAP-12), métricas de falha (MAP-13), comparação de benchmark (MAP-14)

## Versionamento
Versão principal = mudanças de compatibilidade; versão secundária = novos recursos; revisão = correções / otimizações. `VersionSuffix` carrega uma data para facilitar a identificação da build.

## Contribuição
Issues / PRs são bem-vindos: bugs, otimizações de desempenho, suporte a mais mapas, documentação e testes. Antes de enviar, leia `.github/copilot-instructions.md` para manter o estilo de código consistente.

## Licença
MIT; mantenha o aviso de copyright e você poderá usá-lo livremente em projetos comerciais / open source.

---

## Mapa do ecossistema

NewLife é um ecossistema em camadas de projetos open source versionados de forma independente. O mapa abaixo é compartilhado literalmente por todos os repositórios NewLife; cada projeto tem links para o seu repositório e para o seu pacote principal no NuGet.

| Projeto | O que faz | Downloads |
| --- | --- | --- |
| **Base** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Log, configuração, cache, rede, serialização, agendamento e rastreamento APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Middleware de dados: tabelas únicas na escala de dezenas de bilhões de linhas, particionamento automático, separação de leitura e escrita, mais de 10 bancos de dados. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Biblioteca de rede de alto desempenho: 1,77M pps / 14,5 Gbps em uma máquina (TCP 1KB, 2026-10), 4M de conexões TCP persistentes. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Framework de comunicação por protocolo e RPC para aplicações cliente/servidor e dispositivos IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Instala aplicações .NET como daemons do sistema operacional: serviços do Windows e systemd do Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Modelos de projeto `dotnet new` para console, web, API, serviço, dados e IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Dados e mensagens** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Cliente Redis com latência em microssegundos, milhões de operações por segundo e filas de mensagens integradas. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Cliente RocketMQ totalmente gerenciado para Apache RocketMQ e Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Cliente e servidor MQTT, incluindo conectividade com o Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Banco de dados híbrido em C#: embarcado ou em rede, relacional, séries temporais, fila de mensagens e KV em um só. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Driver MySql e camada auxiliar construídos sobre a pilha de dados NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, IA e distribuído** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Framework de plataforma administrativa rápida: permissões, SSO, servidor OAuth e scaffolding CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Plataforma de serviços distribuídos: gestão de nós, APM, centro de configuração, registro e centro de publicação. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plataforma distribuída de tarefas e agendamento de dados em tempo real, com particionamento, repetição e reprocessamento. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Cliente unificado para 46 provedores de LLM, além do ChatAI, uma aplicação web de chat pronta para uso. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industrial e IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Biblioteca padrão de IoT que define contratos de protocolo e abstrações de dispositivos. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Implementação Modbus TCP/RTU/ASCII sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Protocolo Siemens PLC sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Protocolo Omron PLC sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Protocolo Mitsubishi Melsec PLC sobre NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Cliente OPC para acesso a dados industriais. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Pilha do protocolo de automação predial BACnet. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Plataforma IoT de código aberto leve, com gestão de gateways e dispositivos. | — |
| **Telemática veicular** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Protocolo de posicionamento veicular JT/T 808: mais de 70 corpos de mensagem, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Protocolo plataforma-a-plataforma JT/T 809 para autoridades de transporte. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Vídeo veicular JT/T 1078: análise RTP, remux para FLV/fMP4 e transmissão ao vivo. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Componentes de domínio** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Um pacote para Excel, Word, PPT, PDF e mais de 15 formatos de documento. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Áudio multiplataforma: codecs, E/S de dispositivos, DSP, contêineres e streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Codecs de vídeo, formatos de contêiner, processamento de frames e streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Interface unificada sobre Baidu, AMap, Tencent e Tianditu, com geocodificação e conversão de coordenadas. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Geolocalização IPv4 com banco de dados offline mapeado em memória e API online como alternativa. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Ferramentas e produtos** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Caixa de ferramentas do desenvolvedor: rede, porta serial, criptografia, regex, Modbus e MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Sistema auto-hospedado de gerenciamento de código-fonte Git. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Instala automaticamente o runtime .NET correspondente quando iniciado em um host mais antigo. | — |

> Os badges mostram o pacote principal de cada projeto; pacotes de tema, extensão e satélites estão listados nas respectivas páginas do NuGet.Os mais de 30 pacotes não listados aqui (FTP, LoRa, extensões do Office, drivers de protocolo e outros) estão disponíveis na [organização NewLifeX](https://github.com/NewLifeX) e em [newlifex.com](https://newlifex.com).

## Equipe

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

Fundada em 2002, a equipe NewLife dedica-se há mais de duas décadas ao ecossistema .NET e de IoT, mantém cerca de 100 projetos de código aberto (34 deles principais) e oferece soluções integradas de software e hardware para os setores de energia, logística, saúde, controle industrial e internet. Seguimos o lema "não há hierarquia no aprendizado — o domínio conquista o respeito".

Os componentes de código aberto já foram baixados <!--nuget-total:start-->6.48M<!--nuget-total:end--> vezes no NuGet (todo o ecossistema, até 2026-10).

Website: https://newlifex.com · Organização de código aberto: https://github.com/NewLifeX

<details>
<summary>Conta oficial no WeChat: 智能大石头 (escaneie para seguir)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

