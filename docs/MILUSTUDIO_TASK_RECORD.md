# MiLuStudio 总任务记录

更新时间：2026-05-12  
工作目录：`D:\code\MiLuStudio`

本文件只做记录。  
阶段任务安排看 `docs\MILUSTUDIO_PHASE_PLAN.md`。  
短棒交接看 `docs\MILUSTUDIO_HANDOFF.md`。  
长期总参考看 `docs\MILUSTUDIO_BUILD_PLAN.md`。

## 1. 每棒先读

```powershell
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Get-Content .\docs\MILUSTUDIO_BUILD_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_PHASE_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_TASK_RECORD.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_HANDOFF.md -Encoding UTF8
```

## 2. 文件职责记录

`docs\MILUSTUDIO_BUILD_PLAN.md`

- 总参考。
- 保存长期产品定位、架构约束和验收原则。
- 轻易不要修改。

`docs\MILUSTUDIO_PHASE_PLAN.md`

- 总任务阶段安排。
- 保存阶段任务、状态、验收命令和下一阶段安排。
- 每个大阶段结束后可根据联网自检结果调整。

`docs\MILUSTUDIO_TASK_RECORD.md`

- 修改总记录。
- 保存做过什么、为什么改、怎么验证、联网自检结果是什么。
- 不作为任务板。

`docs\MILUSTUDIO_HANDOFF.md`

- 短棒交接。
- 只保存下一棒立刻需要知道的状态、风险和提示词。
- 不堆长历史。

## 3. 总需求记录

本记录用于确认总参考和阶段安排没有偏离原始意图。

长期需求：

- MiLuStudio 是新项目。
- 不覆盖旧 MiLuAssistantWeb。
- 不覆盖旧 MiLuAssistantDesktop。
- 旧 MiLuAssistant 项目只作为历史经验。
- XiaoLouAI / MiLu 自有体系作为主干。
- 目标是 Windows 原生 AI 漫剧 Agent 产品。
- 用户只输入故事、小说片段或创作要求。
- 系统自动完成剧本、角色、分镜、图片、视频、配音、字幕、剪辑、质检和导出。
- 用户只在关键节点做确认、选择和轻量修改。
- 产品优先稳定、简单、可售卖。
- 不把 ArcReel、LumenX、AIComicBuilder、LocalMiniDrama 等项目作为主仓库二开。
- 参考项目只参考流程、数据结构、任务调度、Skills 思想、模型接入和桌面交付形态。
- 内置能力叫 Production Skills。
- 不开放公共 Skills 市场。
- 不要求普通用户理解技能、模型、参数和工作流节点。
- 第一版 MVP 目标是 500 到 2000 字故事输入，输出 30 到 60 秒竖屏 AI 漫剧视频。
- 第一版不做无限画布。
- 第一版不做复杂时间线。
- 第一版不做云端 SaaS。
- 第一版不把 Linux / Docker 作为生产必需。
- 项目文档必须便于 PowerShell 阅读。
- 长期文档、阶段计划、专题说明和交接记录优先放入 `docs\`，根目录保持简洁。
- 前端、后端和 Production Skills 目录优先按路由或功能域聚合。
- 前端实现时如当前会话可用，优先调用 frontend-skill、playwright-interactive、impeccable、taste-skill / gpt-taste 做视觉和交互自检。
- 每个大阶段结束后必须联网搜索自检。
- 实际编码必须遵守高内聚、低耦合、职责单一、关注点分离和依赖倒置等软件设计原则。
- 设计原则服务功能落地，不允许为了架构感提前堆大型抽象。
- MVP 临时直连必须限制在单一 adapter / gateway 内，并记录技术债。
- 所有依赖、配置、运行文件、缓存、日志、数据库、上传素材和生成结果必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录。
- Python、Node.js、.NET、Electron、Playwright、模型缓存等外部依赖不得主动写入 C 盘。
- 如果某个工具无法避免写入 C 盘，必须先停止并记录原因，等待用户确认。

覆盖状态：

- 总参考：已覆盖。
- 总任务阶段安排：已覆盖。
- 短棒交接：已覆盖当前阶段。

## 4. 参考项目记录

保留参考：

- `D:\code\XiaoLouAI`
- `D:\code\AIComicBuilder`
- `D:\code\LocalMiniDrama`
- `D:\code\lumenx`
- `D:\code\ArcReel-main`
- `D:\code\Toonflow-app`
- `D:\code\huobao-drama`
- `D:\code\OpenMontage`

已清理本地目录：

- `D:\code\MiLuAssistantWeb`
- `D:\code\MiLuAssistantDesktop`
- `D:\code\QwenPaw`

参考边界：

- `XiaoLouAI` 是工程主干参考。
- `AIComicBuilder` 是漫剧流水线主参考。
- `LocalMiniDrama` 是 Windows 本地一键体验参考。
- `lumenx` 是行业 SOP 参考。
- `ArcReel-main` 是 Agent、SSE、任务队列、成本追踪参考。
- `Toonflow-app` 是 Electron 桌面分发和短剧工厂参考。
- `huobao-drama` 是一句话生成短剧和 Agent Skills 参考，商业授权风险需注意。
- `OpenMontage` 是 Skills、pipeline、FFmpeg、字幕、质检和成本记录参考，AGPL 风险需注意。

## 5. 修改记录

### 2026-05-12

Action:

- 创建 `MILUSTUDIO_BUILD_PLAN.md`。
- 清理本地旧参考目录：MiLuAssistantWeb、MiLuAssistantDesktop、QwenPaw。
- 保留 XiaoLouAI、AIComicBuilder、LocalMiniDrama、lumenx、ArcReel-main、Toonflow-app、huobao-drama、OpenMontage。
- 在总参考中补充第 19 节：文档、任务记录和阶段自检约束。
- 创建 `MILUSTUDIO_TASK_RECORD.md`。
- 创建 `MILUSTUDIO_HANDOFF.md`。

Reason:

- 让新项目与旧 MiLuAssistant 项目分离。
- 给下一会话留下稳定的总参考和短交接。

Verification:

- 已用 PowerShell `Get-Content -Encoding UTF8` 读取文档。
- 已确认旧本地目录不存在。
- 已确认参考项目目录保留。

### 2026-05-12 文档职责重整

Action:

- 新增 `MILUSTUDIO_PHASE_PLAN.md` 作为总任务阶段安排。
- 将 `MILUSTUDIO_TASK_RECORD.md` 收窄为修改总记录和阶段自检记录。
- 更新 `MILUSTUDIO_BUILD_PLAN.md` 第 19 节的文件分工和联网自检更新顺序。
- 更新 `MILUSTUDIO_HANDOFF.md`，让短棒交接直接读取 `MILUSTUDIO_PHASE_PLAN.md`。

Reason:

- 避免“总任务记录”同时承担任务板和历史记录，导致后续阶段修改混乱。
- 让任务安排、修改记录和短交接分工清晰。

Verification:

- 已确认四个总控文档均可通过 `Get-Content -Encoding UTF8` 读取。
- 已确认 `MILUSTUDIO_PHASE_PLAN.md` 存在，并包含 Stage 0 到 Stage 12 的阶段安排。
- 已确认 `MILUSTUDIO_HANDOFF.md` 的每棒先读命令包含 `MILUSTUDIO_PHASE_PLAN.md`。
- 已确认 `MILUSTUDIO_TASK_RECORD.md` 不再承担阶段任务板职责，只保留记录和自检结论。

### 2026-05-12 软件设计硬约束补充

Action:

- 在 `MILUSTUDIO_BUILD_PLAN.md` 增加软件设计硬约束。
- 在 `MILUSTUDIO_PHASE_PLAN.md` 增加每阶段代码设计检查项。
- 在 `MILUSTUDIO_HANDOFF.md` 增加下一棒必须遵守的编码设计硬约束。
- 在本记录的总需求中补充高内聚、低耦合等约束。

Reason:

- 用户明确要求后续实际编码必须符合高内聚、低耦合等软件设计原则。
- 约束需要成为编码验收项，但不能影响核心功能落地。

References:

- Parnas, On the Criteria To Be Used in Decomposing Systems into Modules。
- Robert C. Martin, The Single Responsibility Principle。
- Martin Fowler, YAGNI。

Verification:

- 已确认总参考中包含 `### 19.5 软件设计硬约束`。
- 已确认总任务阶段安排中包含 `## 4. 软件设计阶段检查`。
- 已确认短棒交接中包含 `## 编码设计硬约束`。
- 已确认任务记录保留本次约束追加原因。

### 2026-05-12 D 盘封闭环境约束补充

Action:

- 在 `MILUSTUDIO_BUILD_PLAN.md` 增加 `### 19.6 D 盘封闭环境约束`。
- 在 `MILUSTUDIO_PHASE_PLAN.md` 增加 D 盘封闭环境检查和 Stage 0 初始化要求。
- 在 `MILUSTUDIO_HANDOFF.md` 增加环境封闭硬约束和下一棒执行要求。
- 在本记录的总需求中补充依赖、配置、运行文件不得污染 C 盘的约束。

Reason:

- 用户明确要求所有项目依赖、配置、运行文件都必须约束在主项目目录内。
- Python、Node.js 等外部依赖如需安装也必须限制在 D 盘，不能影响 C 盘。

Verification:

- 已确认总参考中包含 D 盘目录、项目内缓存目录和常见环境变量约束。
- 已确认总任务阶段安排中包含 `Environment check`。
- 已确认短棒交接中包含 `## 环境封闭硬约束`。
- 已确认 Stage 0 初始化任务包含项目内运行目录和 D 盘限制说明。

### 2026-05-12 总控文档迁移到 docs

Action:

- 创建 `docs\` 目录。
- 将 `MILUSTUDIO_BUILD_PLAN.md`、`MILUSTUDIO_PHASE_PLAN.md`、`MILUSTUDIO_TASK_RECORD.md`、`MILUSTUDIO_HANDOFF.md` 移动到 `docs\`。
- 同步修正四个总控文档中的每棒先读命令和下一棒提示词路径。
- 在总参考、阶段安排和短棒交接中补充根目录简洁、文档归档到 `docs\`、前后端和 Production Skills 按路由或功能域聚合的约束。

Reason:

- 用户希望根目录保持简洁，当前项目尚未开始实现，移动成本低。
- 用户偏好参考 XiaoLouAI 的克制顶层目录风格，并希望后续功能目录按路由或功能进行整合。

Verification:

- 已确认根目录当前只保留 `docs\`。
- 已确认四个总控文档均位于 `docs\`。
- 已确认每棒先读命令使用 `.\docs\...` 路径。
- 已确认文档仍可通过 `Get-Content -Encoding UTF8` 读取。

### 2026-05-12 前端视觉与品牌资产同步

Action:

- 确认当前会话可用 `frontend-skill`、`playwright-interactive`、`impeccable`、`gpt-taste`。
- 按 XiaoLouAI 的浅色、蓝紫、简洁工作台气质优化前端显示效果。
- 主要样式集中在 `apps\web\src\styles.css`，保持 UI 仍是项目列表和生产控制台，不改成营销页。
- 将用户提供的 logo 内置到 `apps\web\public\brand\logo.png`。
- 将前端品牌标识改为项目内 `/brand/logo.png`。
- 将 Web favicon 统一为 `/brand/logo.png`。
- 在 `docs\PRODUCT_SPEC.md` 补充视觉基调和品牌资产约束。
- 在 `docs\MILUSTUDIO_PHASE_PLAN.md` 补充 Stage 1 当前视觉和 logo 落地状态。

Reason:

- 用户要求当前前端整体参考 XiaoLouAI 的界面颜色和气质。
- 用户要求项目 logo 可以全部调用 `C:\Users\10045\Downloads\logo.png`，但不能引用外部文件，必须内置到项目中。
- 文档需要同步实际代码状态，避免下一棒误以为 logo 仍来自下载目录或 UI 只是临时 mock 外观。

Verification:

- 已运行 `npm run build`，通过。
- 已用 Edge headless 检查桌面和移动端页面。
- 截图保存到 `.tmp\milu-project-desktop.png`、`.tmp\milu-list-desktop.png`、`.tmp\milu-project-mobile.png`、`.tmp\milu-logo-project.png`、`.tmp\milu-logo-mobile.png`。
- 已确认 `http://127.0.0.1:5173/project/demo-episode-01` 可访问。
- 已确认前端侧栏和移动端均显示项目内置 logo。
- 已确认代码、HTML、public 目录中不再引用 `C:\Users\10045\Downloads\logo.png` 或 `Downloads/logo`。

Design check:

- cohesion: 视觉样式集中在 `apps\web\src\styles.css`，品牌资产集中在 `apps\web\public\brand`。
- coupling: 前端只引用静态 public 资产，不访问用户下载目录、文件系统 API、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: logo 作为品牌资产存在，功能区图标仍由前端组件和 lucide 图标承担。
- temporary debt: 当前小尺寸侧栏使用完整装饰字标，后续桌面打包时可从同源 logo 派生 `.ico` 或更适合任务栏的小图标。

Environment check:

- project-local files: 新增品牌资产位于 `D:\code\MiLuStudio\apps\web\public\brand\logo.png`。
- D drive only: 运行、截图、缓存和项目资产均保留在 `D:\code\MiLuStudio`。
- C drive risk: 只从用户明确给出的本地 logo 源路径复制一次，项目代码和运行时不引用 C 盘路径。

## 6. 联网自检记录

### 2026-05-12 Stage 0 + Stage 1 完成

Date:

- 2026-05-12

Stage:

- Stage 0 项目初始化。
- Stage 1 前端 UI 壳。

Local verification:

- 已初始化 Git 仓库。
- 已添加 `.gitignore`、根 `README.md`、根 `.npmrc`。
- 已添加 `docs\REFERENCE_PROJECTS.md` 和 `docs\PRODUCT_SPEC.md`。
- 已添加 `scripts\windows\Set-MiLuStudioEnv.ps1`，用于约束 npm、pip、NuGet、Electron、Playwright、HuggingFace 等缓存位置到项目内。
- 已创建 `apps\web` Vite + React + TypeScript 前端壳。
- 已实现项目列表、项目详情、对话输入、模式切换、任务进度 mock、结果卡片 mock、最终交付 mock。
- 已运行 `npm install`。
- 已运行 `npm run build`，通过。
- 已启动 dev server，`http://127.0.0.1:5173/project/demo-episode-01` 返回 200。
- 已用 Edge headless 截图检查桌面和移动端布局，截图写入 `.tmp\`。

Design check:

- cohesion: 前端按 `projects`、`production-console`、`settings`、`shared` 聚合，职责清晰。
- coupling: UI 仅使用本地 mock 数据，没有访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: mock 数据在 `shared\mock`，类型在 `shared\types`，页面和生产控制台按功能域隔离。
- temporary debt: 当前路由为轻量 History API mock，后续接入真实路由或应用壳时可替换。
- frontend skills: 当前会话可用 skill 列表未暴露 `frontend-skill`、`playwright-interactive`、`impeccable`、`taste-skill` / `gpt-taste`，已记录约束，并以本地构建、可运行页面和截图人工自检替代。

Environment check:

- project-local files: npm cache 已验证为 `D:\code\MiLuStudio\.cache\npm`。
- D drive only: 项目依赖、构建产物、截图、缓存和临时目录均在 `D:\code\MiLuStudio` 内。
- C drive risk: 使用 Edge 可执行文件进行截图，但通过 `--user-data-dir` 将浏览器 profile 指向 `.tmp\edge-profile*`；未主动写入项目缓存到 C 盘。
- cleanup: 曾误生成根 `package.json`、根 `package-lock.json` 和根 `node_modules`，已确认属于本轮误写并删除，根目录恢复简洁。

Web searches:

- Vite 8 official announcement: `https://vite.dev/blog/announcing-vite8`
- Vite getting started: `https://vite.dev/guide/`
- electron-builder NSIS docs: `https://www.electron.build/nsis.html`
- Vite React plugin repository: `https://github.com/vitejs/vite-plugin-react`

Findings:

- Vite 8 于 2026-03-12 发布，当前主线使用 Rolldown，并同步发布 `@vitejs/plugin-react` v6。
- Stage 1 前端壳已升级到 `vite@^8.0.12` 和 `@vitejs/plugin-react@^6.0.1`。
- 当前阶段没有发现需要引入 Docker、Linux 生产依赖、云端 SaaS 或复杂工作台的理由。

Deviation risk:

- 无方向偏差。
- 需在下一阶段继续避免让 UI 直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。

Build plan changes:

- 增加前端质量约束：如果会话可用，前端实现前优先调用 `frontend-skill`、`playwright-interactive`、`impeccable`、`taste-skill` / `gpt-taste`。

Phase plan changes:

- Stage 0 标记为 `done`。
- Stage 1 标记为 `done`。
- 当前焦点改为 Stage 2。
- Stage 1 增加前端优化 Skills 自检要求。

Task record changes:

- 记录本阶段完成内容、本地验证、环境检查、联网自检和设计检查。

Handoff changes:

- 更新当前接棒状态为 Stage 2。
- 保留 dev server URL。
- 记录前端优化 Skills 可用时优先调用的要求。

Next phase:

- Stage 2 数据模型和 Control API。
- 建议先创建 `backend\control-plane`，不接真实模型。

### 2026-05-12 Stage 2 完成

Date:

- 2026-05-12

Stage:

- Stage 2 数据模型和 Control API。

Local verification:

- 已创建 `backend\control-plane\MiLuStudio.ControlPlane.sln`。
- 已创建 `MiLuStudio.Api`、`MiLuStudio.Application`、`MiLuStudio.Domain`、`MiLuStudio.Infrastructure`、`MiLuStudio.Worker`。
- 已建立项目、story input、角色、镜头、资产、production job、generation task、cost ledger 的最小领域模型。
- 已添加 PostgreSQL 初始 SQL migration：`backend\control-plane\db\migrations\001_initial_control_plane.sql`。
- 已建立 PostgreSQL connection string 配置，但 Stage 2 运行期使用 in-memory repository，避免本地无数据库时阻塞 API 验收。
- 已实现项目 API、production job API、pause、resume、retry 和 SSE mock。
- 已将前端项目列表和生产控制台接入 Control API DTO。
- 已将前端“开始生成”接入真实 job 创建和 SSE 进度监听。
- 已运行 `dotnet build backend\control-plane\MiLuStudio.ControlPlane.sln`，通过。
- 已运行 `npm run build`，通过。
- 已启动 Control API：`http://127.0.0.1:5268/health`。
- 已启动 Web dev server：`http://127.0.0.1:5173/`。
- 已通过 REST 调用验证 `GET /api/projects`、`POST /api/projects`、`POST /api/projects/{projectId}/production-jobs`。
- 已通过 `curl.exe` 验证 `GET /api/production-jobs/{jobId}/events` 能推送 `checkpoint_required` 和 `artifact_ready`。
- 已通过 Edge DevTools Protocol 点击前端“开始生成”，确认页面显示真实 `job_...` jobId 和 SSE 进度。
- 已用 Edge headless 截图检查桌面和移动端：`.tmp\stage2-project-desktop-v2.png`、`.tmp\stage2-project-mobile-v7.png`。

Design check:

- cohesion: Domain 只放领域实体和枚举，Application 只放 DTO、接口和业务服务，Infrastructure 只放时钟和 in-memory repository，Api 只映射 HTTP 路由。
- coupling: UI 只通过 `controlPlaneClient.ts` 调用 Control API，不访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: PostgreSQL schema 作为 migration 文件单独存在，运行期 mock repository 被限制在 Infrastructure 边界内。
- temporary debt: Stage 2 尚未接入真实 PostgreSQL adapter / EF Core DbContext；in-memory repository 会随 API 进程重启丢失新建项目和 job；Worker 当前只做 heartbeat。

Environment check:

- project-local files: 新增后端源码位于 `backend\control-plane`，NuGet 包缓存位于 `.nuget\packages`，截图和 API/Web 日志位于 `.tmp`。
- D drive only: .NET SDK 使用 `D:\soft\program\dotnet`，项目依赖、构建产物、NuGet 缓存和截图均保留在 `D:\code\MiLuStudio` 或 D 盘工具目录。
- C drive risk: Edge 可执行文件来自 `C:\Program Files (x86)\Microsoft\Edge`，但浏览器 profile 通过 `--user-data-dir` 指向项目 `.tmp`；未主动把项目缓存、依赖或运行数据写入 C 盘。

Web searches:

- Microsoft .NET releases and support: `https://learn.microsoft.com/en-us/dotnet/core/releases-and-support`
- Microsoft ASP.NET Core APIs overview: `https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis?view=aspnetcore-8.0`
- Npgsql EF Core provider: `https://www.npgsql.org/efcore/`

Findings:

- Microsoft 文档显示 .NET 8 是 LTS，支持到 2026 年 11 月，仍适合当前 Windows 原生产品主线。
- Microsoft 文档推荐新 HTTP API 优先使用 Minimal APIs，当前 Control API 的轻量路由方式没有偏离主线。
- Npgsql 官方文档确认 PostgreSQL 的 EF Core provider 仍是后续真实持久化接入路径。
- 当前阶段没有发现需要引入 Docker、Linux、Redis、Celery 或云端 SaaS 作为生产必需的理由。

Deviation risk:

- 无方向偏差。
- 主要风险是 Stage 2 为了先跑通 API 使用 in-memory repository，Stage 3/后续持久化阶段需要收敛到 PostgreSQL adapter。

Build plan changes:

- 无。

Phase plan changes:

- Stage 2 标记为 `done`。
- 当前焦点改为 Stage 3。
- Stage 2 记录 in-memory runtime、PostgreSQL migration 和临时债。

Task record changes:

- 记录本阶段完成内容、本地验证、设计检查、环境检查和联网自检。

Handoff changes:

- 更新当前接棒状态为 Stage 3。
- 记录 Control API 和 Web dev server URL。
- 记录下一阶段要收敛的 in-memory / Worker 临时债。

Next phase:

- Stage 3 任务状态机。
- 建议先定义合法状态迁移表和 checkpoint，再把 mock SSE 进度改成受状态机驱动。

当前状态：

- Stage 0 已完成。
- Stage 1 已完成。
- Stage 2 已完成。
- 下一步进入 Stage 3。

自检记录模板：

```text
Date:
Stage:
Local verification:
Design check:
Environment check:
Web searches:
Findings:
Deviation risk:
Build plan changes:
Phase plan changes:
Task record changes:
Handoff changes:
Next phase:
```

## 7. 当前短结论

```text
当前 Stage 0、Stage 1 和 Stage 2 已完成，可进入 Stage 3。
执行任务看 docs\MILUSTUDIO_PHASE_PLAN.md。
下一棒提示词看 docs\MILUSTUDIO_HANDOFF.md。
本文件只记录阶段完成、自检和修改原因。
```
