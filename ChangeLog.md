# NewLife.Map 版本更新记录

## v2.8.2026.1024 (2026-10-24)

### 运行时多语言
- **11 种语言词表**：新增 `Locales/*.yml`，覆盖异常、日志、接口消息、程序集标题/描述与提供者显示名；英文为源文与回退
- **带前缀日志**：`Map.WriteLog/WriteError` 改用 NewLife.Core 的 `InfoPrefixed/ErrorPrefixed`，默认 `[提供者]` 前缀，前缀不参与翻译
- **请求语言切换**：MapApi 按 `Accept-Language` 的 q 权重切换请求语言，支持 `Localization:Culture` / `Localization:LogCulture` 固定界面与日志语言
- **包内 README**：NuGet 包内置 `README.nuget.md`，支持多语言入口

### 地图提供者
- **天地图 POI 检索**：改用官方二代 v2/search，支持行政区划区域搜索与普通搜索，修正原接口与响应解析
- **失效密钥识别**：仅按失效关键词判定，`INVALID_USER_KEY` 等真实失效消息恢复触发熔断
- **Nominatim 解析**：`Map.InvokeAsync` 改用实例 JsonParser，兼容 JSON 数组根，修复 search 恒返回 null
- **镜像部署**：AMap/WeMap 地理编码接口改为相对 Server，支持镜像部署与测试注入

### 平台与工具
- **MapApi 本地化**：运行时消息、配置元数据与接口错误本地化，支持按请求语言输出
- **NativeAOT**：新增 `AotSmoke` 18 个场景，win-x64 / linux-x64 / linux-arm64 原生运行 + linux-loongarch64 JIT 仿真；`Map.Create` 优先匹配工厂注册表，反射扫描仅作兜底
- **移动端冒烟**：新增 Android/iOS 冒烟工程与 CI

### 测试与质量
- **测试分层**：确定性桩测试（默认运行）与真实服务集成测试（`Category=Integration`，默认跳过）分离；确定性用例 98 个
- **多 RID CI**：win-x64 / linux-x64 / linux-arm64 阻断 + linux-la64 观察期非阻塞
- **依赖升级**：NewLife.Core 12.0.2026.1024、Stardust 3.10、XCode 12.2、Cube 6.15

### 发布与工程化
- **发布渠道**：正式版同时推送 GitHub Packages 与 NuGet.org，tag 触发时创建 GitHub Release 并附加 nupkg/snupkg；beta 包同步推送两个源
- **解决方案**：迁移为 `NewLife.Map.slnx`；校验脚本迁入 `tools/`
- **文档**：11 种语言 README，需求/功能清单/架构/竞品文档同步

---

## v2.7.2026.0804 (2026-08-04)

### 地图提供者
- **OSM/Nominatim 开源数据源**：新增无 Key 免费海外地图提供者 `NominatimMap`，支持地理编码与逆地理编码（WGS84），无需申请 Key 即可使用
- **腾讯行政区划**：WeMap 新增 `GetAreaAsync` 行政区划查询，支持省/市/区三级列表与子级区划获取

### 平台能力（MapApi）
- **正向地理编码与驾驶距离**：NewLifeMap 与 MapApi 平台补齐 `GeoAddress` / `Distance` 统一端点，平台侧聚合多厂商实现
- **在线 IP 解析**：MapApi 新增 IP 解析端点，基于 NewLife.IP 离线库，支持物理地址查询

### 性能与可靠性
- **令牌桶限速**：Map 基类新增 `MaxQps` 每秒请求上限，固定间隔限速，避免单 Key 高频请求被厂商封禁
- **故障统计**：Map 基类内置请求总数 / 成功 / 失败 / 成功率 / 平均耗时统计及按 Key 计数，便于监控告警
- **坐标转换基准测试**：新增 `NewLife.Map.Benchmark` 项目，提供坐标转换性能基准测试

### 重构与优化
- **模型重构**：`GeoAddress` / `GeoPoint` 移至 `NewLife.Map.Models` 命名空间，结构更清晰
- **坐标转换健壮性**：修复坐标转换边界场景问题，`GeoPoint` 空值兼容性提升，地图解析优化
- **依赖升级**：升级核心及第三方依赖包至 2026.7 最新版

### 测试与质量
- **测试补充**：新增基类 / 工厂 / 多 Key 熔断 / 模型 / Nominatim / 限速等测试类，累计 41 测试 28 通过
- **文档体系**：重构需求文档 / 功能清单 / 架构设计 / 竞品分析文档体系

### Bug 修复
- **[fix]** 修复 MapApi SQLite 驱动问题
- **[fix]** 修复坐标转换在边界输入下的异常

---

## v2.6.2026.0102 (2026-01-02)

（上一版本，历史记录）
