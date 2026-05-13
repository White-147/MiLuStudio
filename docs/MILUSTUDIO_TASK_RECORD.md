# MiLuStudio 总任务记录

更新时间：2026-05-13
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
当前 Stage 0、Stage 1、Stage 2、Stage 3、Stage 4 和 Stage 5 已完成，可进入 Stage 6。
执行任务看 docs\MILUSTUDIO_PHASE_PLAN.md。
下一棒提示词看 docs\MILUSTUDIO_HANDOFF.md。
本文件只记录阶段完成、自检和修改原因。
```

### 2026-05-12 Stage 3 完成

Date:

- 2026-05-12

Stage:

- Stage 3 任务状态机。

Local verification:

- 已在 `MiLuStudio.Domain` 定义 `ProductionStage`。
- 已在 `MiLuStudio.Application\Production\ProductionStateMachine.cs` 定义合法状态迁移表。
- 已将 `ProductionJobService` 改为通过状态机推进 job/task。
- 已新增 `TaskQueueService`，集中创建初始 task、记录 attempt_count、更新 running/review/completed/failed 状态。
- 已新增 `POST /api/production-jobs/{jobId}/checkpoint`。
- 已让 mock SSE 每轮重新读取 job snapshot，并由状态机决定 `task_started`、`stage_changed`、`checkpoint_required`、`stage_paused`、`task_failed`、`artifact_ready`。
- 已在 SSE DTO 中加入 `jobStatus`，前端不再仅按 progress 推断 job 状态。
- 已在生产控制台增加 pause、resume、checkpoint approve、retry failed task 四个 Control API 操作按钮。
- 已运行 `$env:DOTNET_CLI_HOME='D:\code\MiLuStudio\.dotnet'; $env:NUGET_PACKAGES='D:\code\MiLuStudio\.nuget\packages'; dotnet build backend\control-plane\MiLuStudio.ControlPlane.sln`，通过。
- 已运行 `$env:npm_config_cache='D:\code\MiLuStudio\.cache\npm'; npm run build`，通过。
- 已启动 Control API：`http://127.0.0.1:5268/health`，返回 `stage-3-state-machine-mock`。
- 已通过 REST + `curl.exe --max-time` 验证：启动 job 后 SSE 进入 `checkpoint_required`，job 暂停在 `script`，checkpoint approve 后继续到 `character`。
- 已通过拒绝 character checkpoint 验证 job 进入 `failed_retryable`，character task 变为 `blocked`。
- 已通过 retry 验证只把失败的 character task 重置为 `waiting`，已完成的 story/script 不重跑。
- 已通过 pause/resume smoke 验证 job 状态 `paused -> running`。
- 已用 Edge DevTools Protocol 验证前端项目页能点击“开始生成”、显示真实 `job_...`、在 checkpoint 暂停并启用“确认节点”。
- 前端截图已保存到 `.tmp\screenshots\stage3-console-after-fix.png`；按钮文案溢出问题已修复为两列布局。

Design check:

- cohesion: `ProductionStateMachine` 负责状态迁移，`TaskQueueService` 负责 task 状态和 attempt_count，`ProductionJobService` 负责 repository snapshot 和 DTO/API 编排。
- coupling: UI 只通过 `controlPlaneClient.ts` 调 Control API，不访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: 当前仍只使用 Infrastructure 内的 in-memory repository；PostgreSQL 只保留 SQL migration，未接 EF Core DbContext。
- dependency inversion: API 仍依赖 Application service 和 repository abstraction，不直接依赖具体存储实现。
- temporary debt: durable job recovery、PostgreSQL adapter、EF Core DbContext、Worker 任务领取仍未接入；API 进程重启后会丢失运行期新建 job。

Environment check:

- 后续构建均显式设置 `DOTNET_CLI_HOME=D:\code\MiLuStudio\.dotnet`、`NUGET_PACKAGES=D:\code\MiLuStudio\.nuget\packages`、`npm_config_cache=D:\code\MiLuStudio\.cache\npm`。
- API 日志写入 `D:\code\MiLuStudio\logs`。
- Edge headless/CDP 使用 `D:\code\MiLuStudio\.tmp\edge-stage3-profile` 和 `D:\code\MiLuStudio\.tmp\screenshots`。
- 本棒中最早一次 `dotnet build` 在发现旧 API 进程锁文件前未显式设置 D 盘 env；随后已纠正为 D 盘环境变量重建。后续应优先执行 `scripts\windows\Set-MiLuStudioEnv.ps1` 或等价 env 设置。

Skills check:

- `frontend-skill`：用于约束控制台保持工具型 UI，不做营销页或大视觉重塑。
- `impeccable`：用于按钮可读性和布局稳定性自检；发现四列按钮截断后改为两列。
- `playwright-interactive`：当前没有可调用的 `js_repl` 工具，已记录不可用并用 Edge DevTools Protocol 替代。
- `gpt-taste`：其 AIDA/GSAP/landing page 约束不适合本次生产控制台状态机任务，仅保留按钮对比和不溢出检查思路。

Internet self-check:

- Microsoft Minimal API 文档显示 SSE 可用 `IAsyncEnumerable` 表示事件流；当前手写 SSE 写法与该流式模型一致，后续升级 .NET 版本时可考虑 `TypedResults.ServerSentEvents`。
- Microsoft Hosted Service 文档确认 BackgroundService / Worker Service 是长任务应用的推荐入口；当前 Worker 保持 BackgroundService heartbeat 没有偏离 Windows Worker 主线。
- PostgreSQL 官方文档确认 `SKIP LOCKED` 适合多个消费者访问 queue-like table 时避免锁竞争，但会得到不一致视图；因此当前没有在 in-memory 阶段伪造 PostgreSQL 领取逻辑。
- OpenMontage 文档显示 human checkpoint 会在需要审批的阶段暂停并等待 approve/revise/abort；当前 checkpoint pause/approve/retry 思路只参考协议思想，不复制源码。

Deviation reason:

- 阶段计划早期写过 Stage 3 要让 Worker 使用 PostgreSQL `FOR UPDATE SKIP LOCKED` 领取任务；本次用户明确要求当前 PostgreSQL 只落 SQL migration，API 运行期仍是 in-memory repository，真实 PostgreSQL adapter / EF Core DbContext 和 Worker 任务领取边界留给后续阶段。
- 因此已先更新 `MILUSTUDIO_BUILD_PLAN.md` 和 `MILUSTUDIO_PHASE_PLAN.md`：Stage 3 只完成状态机、checkpoint、SSE 和 UI 状态闭环，PostgreSQL task claiming 延后。

Build plan changes:

- `MILUSTUDIO_BUILD_PLAN.md` 的 Stage 3 步骤改为 Worker 保留 heartbeat / 领取边界，`FOR UPDATE SKIP LOCKED` 留到后续持久化 / Worker 收敛阶段。
- Stage 3 验收补充：API 进程存活时刷新可恢复进度；API 重启后的 durable recovery 留给 PostgreSQL adapter / EF Core 阶段。

Phase plan changes:

- Stage 3 标记为 `done`。
- 当前焦点改为 Stage 4 Python Skills Runtime。
- Stage 3 任务清单同步说明 PostgreSQL claiming 延后。

Handoff changes:

- 更新当前接棒状态为 Stage 4 preparation。
- 记录 Stage 3 已完成状态机、checkpoint、retry、SSE 和前端按钮闭环。
- 保留 in-memory runtime / durable recovery / Worker claiming 临时债。

Next phase:

- Stage 4 Python Skills Runtime。
- 创建 `backend\sidecars\python-skills` 和第一个内部 `story_intake` skill。

### 2026-05-13 Stage 4 完成

Date:

- 2026-05-13

Stage:

- Stage 4 Python Skills Runtime。

Local verification:

- 已创建 `backend\sidecars\python-skills`。
- 已创建 `pyproject.toml`，项目目标为 Python `>=3.11,<3.14`。
- 已按用户要求下载 Python 3.13.13 Windows embeddable package 到 `D:\code\MiLuStudio\.cache\python\python-3.13.13-embed-amd64.zip`。
- 已校验 SHA-256：`8766a8775746235e23cf5aee5027ab1060bb981d93110577adcf3508aa0cbd55`。
- 已解压到一致的 D 盘 Python 工具目录：`D:\soft\program\Python\Python313`。
- 已在 `D:\soft\program\Python\Python313\python313._pth` 中加入 `D:\code\MiLuStudio\backend\sidecars\python-skills`，使 embeddable Python 能稳定找到 runtime 包。
- 已创建 `milu_studio_skills` 包。
- 已实现统一 CLI：`python -m milu_studio_skills run --skill story_intake --input input.json --output output.json`。
- 已实现 `milu_studio_skills.gateway.SkillGateway`，作为后续 .NET Worker subprocess 调用 Python skill 的单一边界。
- 已创建 `skills\story_intake`。
- 已添加 `skill.yaml`、`prompt.md`、`schema.input.json`、`schema.output.json`。
- 已添加 `executor.py` 和 `validators.py`。
- 已添加 `examples\input.json` 和 `examples\output.json`。
- 已添加 `tests\test_story_intake.py` 和 `tests\test_cli.py`。
- `story_intake` 当前为 deterministic mock，不接真实模型 provider。
- CLI 成功输出 `ok=true` envelope，包含 `data`、`runtime.model=none`、`runtime.cost_estimate=0`。
- CLI 失败输出 `ok=false` envelope，包含 `error.code`、`error.message` 和 `error.details`。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m compileall -q milu_studio_skills skills tests`，通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -v`，5 个测试通过。
- 已运行 CLI success smoke，输出到 `D:\code\MiLuStudio\.tmp\story_intake.output.json`。
- 已运行 CLI validation failure smoke，输出到 `D:\code\MiLuStudio\.tmp\story_intake.error.json`，退出码为 2。
- 已运行 `dotnet build backend\control-plane\MiLuStudio.ControlPlane.sln`，通过。
- 已运行 `npm run build`，通过。

Design check:

- cohesion: `SkillGateway` 只负责统一 skill 调用边界，CLI 只负责文件输入输出，`story_intake` executor 只负责故事入口解析，validator 只负责输入输出校验。
- coupling: UI 没有变化，仍只通过 Control API；Python skill 不访问数据库、文件系统业务目录、模型 SDK、Python 外部脚本或 FFmpeg。
- boundaries: 后续 .NET Worker 应只调用 `milu_studio_skills` CLI / gateway，不直接 import 或执行单个 skill 文件。
- dependency inversion: 当前 Python runtime 没有外部 provider 依赖，模型供应商和数据库写回仍留给后续 adapter。
- temporary debt: 当前 `story_intake` 是 deterministic mock；真实 LLM 解析、Worker 调用、数据库写回、资产索引、Python 3.11 本机解释器验证仍待后续阶段。

Environment check:

- project-local files: Python runtime 位于 `D:\code\MiLuStudio\backend\sidecars\python-skills`。
- D drive only: 验证使用 `D:\soft\program\Python\Python313\python.exe`，缓存和临时目录通过 `scripts\windows\Set-MiLuStudioEnv.ps1` 指向项目内；脚本现在设置 `MILUSTUDIO_PYTHON=D:\soft\program\Python\Python313\python.exe`。
- C drive risk: `where python` 发现 `C:\Users\10045\AppData\Local\Microsoft\WindowsApps\python.exe` 在 PATH 前面；实际验证没有使用它，后续必须继续显式调用 D 盘 Python 或配置 D 盘 venv。
- Python version note: 已按用户要求补齐 Python 3.13.13 到 `D:\soft\program\Python\Python313`，并在 `pyproject.toml` 中用 `requires-python = ">=3.11,<3.14"` 约束 3.11 到 3.13 兼容项目目标。

Skills check:

- `frontend-skill`：本阶段没有前端实现，已读取 skill 并作为“不做营销页/前端不变更”的门禁参考。
- `playwright-interactive`：本阶段没有前端交互改动；且当前工具列表没有 `js_repl`，未运行。
- `impeccable`：其说明明确不用于 backend-only 或 non-UI tasks，本阶段未做前端设计编辑。
- `gpt-taste`：其 AIDA/GSAP/landing page 约束不适合 Python runtime，本阶段未应用到代码。

Internet self-check:

- Python.org 显示 Python 3.13.13 于 2026-04-07 发布，是 Python 3.13 的维护版本；当前 `>=3.11,<3.14` 目标没有偏离可维护 Python 主线。
- Python Packaging User Guide 建议新项目使用 `pyproject.toml` 的 `[build-system]` 和 `[project]` 表；当前项目符合。
- Python `argparse` 官方文档说明其用于用户友好的命令行接口并自动生成 help / usage；当前 CLI 使用标准库 `argparse`，没有引入额外依赖。
- JSON Schema 官网显示当前版本为 Draft 2020-12；当前 input/output schema 使用 `https://json-schema.org/draft/2020-12/schema`。
- 当前自检未发现需要引入 Docker、Linux、Redis、Celery、公共 Skill 市场或真实模型 provider 的理由。

Deviation reason:

- Stage 4 原文写过“使用 Python 3.11”和“.NET Worker 能调用 Python skill / 输出能写回数据库”。用户已确认如 Python 版本不符合，可更新到最新 3.13，但文件位置要保持一致；同时用户明确要求真实 PostgreSQL adapter / EF Core DbContext 和 Worker 任务领取边界留给后续阶段。
- 因此已先更新 `MILUSTUDIO_BUILD_PLAN.md` 和 `MILUSTUDIO_PHASE_PLAN.md`：Stage 4 改为目标 Python `>=3.11,<3.14`，本地使用 `D:\soft\program\Python\Python313\python.exe` 验证；验收改为统一 CLI / gateway 和可供后续写回的稳定 envelope，不直接接真实数据库。

Build plan changes:

- Stage 4 的 Python 版本要求改为 `>=3.11,<3.14`。
- Stage 4 验收改为 Python skill 只能通过统一 CLI / gateway 调用。
- Stage 4 验收补充失败输出必须包含错误码和错误消息。

Phase plan changes:

- Stage 4 标记为 `done`。
- 当前焦点改为 Stage 5。
- Stage 4 的 `.NET Worker 调用` 调整为“为后续 .NET Worker 调用留出单一 CLI / gateway 边界”。

Handoff changes:

- 更新当前接棒状态为 Stage 5 preparation。
- 记录 Python runtime 路径、CLI 命令、测试命令和 Python 3.13.13 验证事实。

Next phase:

- Stage 5 故事解析到脚本。
- 在不绕过 Control API 的前提下，实现 `plot_adaptation`、`episode_writer`，并把 story intake 输出接入后续脚本生成边界。

### 2026-05-13 Stage 5 完成

Date:

- 2026-05-13

Stage:

- Stage 5 故事解析到脚本。

Local verification:

- 已新增 `milu_studio_skills.contracts.unwrap_skill_data`，只负责从上游 skill envelope 读取 `data`。
- `SkillGateway.default()` 已注册 `plot_adaptation` 和 `episode_writer`。
- 已新增 `skills\plot_adaptation`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已新增 `skills\episode_writer`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已新增 `tests\test_stage5_script_pipeline.py`，覆盖 `story_intake -> plot_adaptation -> episode_writer` 链路和失败 envelope。
- 已更新 `tests\test_cli.py`，测试临时目录优先使用项目 `.tmp`，避免默认写入 C 盘临时目录。
- 已更新 `backend\sidecars\python-skills\README.md`，记录 Stage 5 三段式 CLI 链路。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m compileall -q milu_studio_skills skills tests`，通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -v`，8 个测试通过。
- 已运行 `plot_adaptation` 和 `episode_writer` examples CLI smoke，均通过。
- 已用 515 个中文单位故事输入跑通完整链路，输出 60 秒、4 段脚本、8 条字幕 cue，`checkpoint.required=true`。
- 已运行 `D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln`，通过。
- 已运行 `npm run build`，通过。

Design check:

- cohesion: `plot_adaptation` 只负责短视频剧情结构，`episode_writer` 只负责可审阅脚本文本和字幕 cue，`unwrap_skill_data` 只负责 envelope 解包。
- coupling: UI 未改动，仍不访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg；Python skill 不访问数据库、不调用模型 provider、不触发媒体生成。
- boundaries: 三段链路只通过统一 CLI / `SkillGateway` 传递 JSON envelope，后续 .NET Worker 可在单一 gateway 边界调用。
- temporary debt: 当前仍是 deterministic mock；真实 LLM provider、脚本数据库写回、前端脚本卡、脚本编辑确认 API、durable Worker claiming 继续留给后续阶段。

Environment check:

- project-local files: 新增源码、schema、examples 和测试均位于 `D:\code\MiLuStudio\backend\sidecars\python-skills`。
- D drive only: Python 使用 `D:\soft\program\Python\Python313\python.exe`；`.NET` 使用 `D:\soft\program\dotnet\dotnet.exe`；npm 使用 `D:\soft\program\nodejs\npm.ps1`；临时文件、pycache、CLI 输出均指向 `D:\code\MiLuStudio\.tmp`。
- C drive risk: 当前 `rg.exe` 来自 Codex app 的 C 盘 WindowsApps 路径且执行被拒，已改用 PowerShell 枚举；本阶段未主动写入 C 盘依赖、缓存、运行数据或生成结果。

Skills check:

- 本阶段未做前端实现，未调用 frontend-skill、playwright-interactive、impeccable 或 gpt-taste。
- `playwright-interactive` 当前工具列表仍没有 `js_repl`，且本阶段没有 UI 交互变更。

Web searches:

- AIComicBuilder GitHub: `https://github.com/LingyiChen-AI/AIComicBuilder`
- LumenX GitHub: `https://github.com/alibaba/lumenx`
- JSON Schema Draft 2020-12: `https://json-schema.org/draft/2020-12`

Internet self-check:

- AIComicBuilder GitHub README 仍定位为将脚本转为动画视频，链路包含角色设计、分镜和视频合成；Stage 5 先产出可审阅脚本结构，与后续角色/分镜阶段顺序一致。
- LumenX GitHub README 显示其链路是小说文本转动态视频，并包含剧本分析、角色定制、分镜构造、分镜视频和拼接；Stage 5 只做脚本结构，没有跳到图像、视频或 FFmpeg。
- LumenX 当前仓库包含 Docker、FastAPI、Qwen/Wanx、FFmpeg 等能力，但本项目仍按 Windows 原生、Control API / Worker / Python Sidecar 边界推进，没有把这些作为生产依赖引入。
- JSON Schema 官方站显示当前 released draft 为 2020-12；新增 schema 继续使用 `https://json-schema.org/draft/2020-12/schema`。

Deviation reason:

- 原 Stage 5 文档包含“保存脚本到数据库、前端显示脚本卡、用户可编辑脚本、用户确认后进入角色阶段”。
- 本轮用户明确要求后续保存到数据库仍留给 PostgreSQL adapter / EF Core DbContext 阶段，并要求不要绕过 Control API / Worker 边界。
- 因此先最小更新 `MILUSTUDIO_BUILD_PLAN.md` 和 `MILUSTUDIO_PHASE_PLAN.md`：Stage 5 收敛为 Production Skills 的 `story_intake -> plot_adaptation -> episode_writer` envelope 链路；数据库写回和 UI 审核编辑留后。

Build plan changes:

- Stage 5 目标改为“生成可审阅脚本结构”。
- Stage 5 步骤补充 envelope 串联、`plot_beats`、`segments`、`subtitle_cues` 和 review checkpoint。
- Stage 5 明确本阶段不保存 `episodes.script_text`，不接真实模型，不让 UI 直接调用 Python。

Phase plan changes:

- Stage 5 标记为 `done`。
- 当前焦点改为 Stage 6。
- Stage 5 落地状态补充新增 skill、gateway、测试和 deferred 项。

Task record changes:

- 记录 Stage 5 完成内容、本地验证、设计检查、环境检查、联网自检和偏差原因。

Handoff changes:

- 更新当前接棒状态为 Stage 6 preparation。
- 记录 Stage 5 三段式 skill 链路、测试命令、临时债和下一阶段边界。

Next phase:

- Stage 6 角色和风格。
- 实现 `character_bible` 和 `style_bible`，继续只通过 Python Skills Runtime envelope 产出结构化结果；数据库写回与 UI 编辑仍等待后续 Control API / PostgreSQL adapter 收敛。

### 2026-05-13 Stage 6 完成

Date:

- 2026-05-13

Stage:

- Stage 6 角色和风格。

Local verification:

- 已新增 `skills\character_bible`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已新增 `skills\style_bible`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- `SkillGateway.default()` 已注册 `character_bible` 和 `style_bible`。
- `character_bible` 输入 `episode_writer` envelope，输出角色名、身份、性格、外貌、服装、声音、stable seed、visual identity、continuity rules 和 `checkpoint.required=true`。
- `style_bible` 输入 `episode_writer` + `character_bible` envelopes，输出统一画风、色板、灯光、环境、镜头语言、角色渲染规则、负向提示词、可复用 prompt blocks 和 `checkpoint.required=true`。
- 已新增 `tests\test_stage6_character_style_pipeline.py`，覆盖 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible` 链路和失败 envelope。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m compileall -q backend\sidecars\python-skills\milu_studio_skills backend\sidecars\python-skills\skills backend\sidecars\python-skills\tests`，通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -p test*.py`，11 个测试通过。
- 已运行 `character_bible` 和 `style_bible` examples CLI smoke，均通过。
- 已运行 `D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln --nologo`，通过。
- 已运行 `npm run build`，通过。

Design check:

- cohesion: `character_bible` 只负责角色设定，`style_bible` 只负责统一画风和可复用提示词块，validator 只负责输入输出契约。
- coupling: Stage 6 仍只通过 `SkillGateway` / CLI envelope 串联；UI 未直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: 不写数据库，不触发图片、视频、参考图上传、配音、字幕文件或 FFmpeg；后续保存仍留给 PostgreSQL adapter / EF Core DbContext。
- dependency inversion: 当前 skill 仍为 deterministic mock，真实模型 provider、资产生成和持久化都保留在后续 adapter / gateway 边界。
- temporary debt: 前端角色卡 / 风格卡、用户锁定、重新生成、上传参考图和持久化编辑本阶段未做，已在阶段计划中延后。

Environment check:

- project-local files: 新增源码、schema、examples 和测试均位于 `D:\code\MiLuStudio\backend\sidecars\python-skills`。
- D drive only: Python 使用 `D:\soft\program\Python\Python313\python.exe`；`.NET` 使用 `D:\soft\program\dotnet\dotnet.exe`；npm 使用 `D:\soft\program\nodejs\npm.ps1`；pycache、CLI 输出、构建缓存继续由 `scripts\windows\Set-MiLuStudioEnv.ps1` 指向 D 盘。
- C drive risk: `rg.exe` 仍因 C 盘 WindowsApps 权限不可用，本阶段改用 PowerShell 枚举；未主动写入 C 盘依赖、缓存、运行数据或生成结果。

Skills check:

- 本阶段没有前端实现，未调用 frontend-skill、playwright-interactive、impeccable 或 gpt-taste。
- `playwright-interactive` 当前工具列表没有 `js_repl`，且本阶段无 UI 交互改动。

Web searches:

- LumenX GitHub: `https://github.com/alibaba/lumenx`
- Codeywood: `https://codeywood.com/`
- OpenAI Structured Outputs: `https://developers.openai.com/api/docs/guides/structured-outputs`

Internet self-check:

- LumenX README 显示短漫剧链路按“资产提取 -> 风格定调 -> 资产生成 -> 分镜脚本 -> 分镜图 -> 分镜视频”推进，Art Direction 使用正向/负向提示词限定后续视觉标准；Stage 6 输出 `style_bible` 和 negative prompt，与顺序一致。
- LumenX README 还显示角色一致性依赖全身图、三视图、头像特写等参考资产；本项目 Stage 6 按用户边界只产出角色设定结构，参考图生成留给后续资产阶段。
- Codeywood 强调 self-contained skills、明确输入输出、质量门禁，以及 Story -> Character/Location -> Reference -> Shot -> Video 的顺序；Stage 6 当前停在 Character/Style 结构和 checkpoint，未提前进入参考图或视频。
- OpenAI Structured Outputs 文档说明结构化输出应由 schema 约束，但仍需要处理错误和应用侧校验；当前 deterministic mock + validators + envelope failure 流程符合后续接模型时的边界。

Deviation reason:

- 原 Stage 6 文档把“前端显示角色卡 / 风格卡、角色锁定、上传参考图、重新生成角色”放在同一阶段。
- 本轮用户明确要求基于现有 `episode_writer` envelope 先实现 `character_bible` 和 `style_bible` 的内部 Production Skill 边界，并要求后续保存到数据库留给 PostgreSQL adapter / EF Core DbContext，不让 UI 直接访问 Python、文件系统或数据库。
- 联网自检也确认参考图、三视图、分镜图和视频生成属于角色/风格结构之后的资产阶段；因此已先更新 `MILUSTUDIO_BUILD_PLAN.md` 和 `MILUSTUDIO_PHASE_PLAN.md`，将 Stage 6 收敛为可审阅 JSON 结构，UI/DB/参考图延后。

Build plan changes:

- Stage 6 目标改为“在 Python Skills Runtime 内生成稳定角色设定和统一画风结构”。
- Stage 6 步骤补充 `character_bible`、`style_bible` 的 envelope 输入输出、gateway 注册、schema、examples、tests 和 CLI smoke。
- Stage 6 明确不保存数据库、不接真实模型、不触发图片/视频/FFmpeg、不让 UI 直接调用 Python。
- 前端角色卡 / 风格卡、锁定、重新生成、参考图上传、三视图和头像特写生成已标为延后。

Phase plan changes:

- Stage 6 标记为 `done`。
- 当前焦点改为 Stage 7。
- Stage 6 落地状态补充新增 skill、gateway、测试和 deferred 项。

Handoff changes:

- 更新当前接棒状态为 Stage 7 preparation。
- 记录 Stage 6 两个 skill、测试命令、验证结果、临时债和下一阶段边界。

Next phase:

- Stage 7 分镜。
- 基于 `episode_writer`、`character_bible` 和 `style_bible` envelopes 实现 `storyboard_director`，继续只通过 Python Skills Runtime 输出可审阅镜头列表；数据库写回、前端分镜表和用户编辑仍等待后续 Control API / PostgreSQL adapter 收敛。

### 2026-05-13 Stage 7 完成

Date:

- 2026-05-13

Stage:

- Stage 7 分镜。

Local verification:

- 已新增 `skills\storyboard_director`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- `SkillGateway.default()` 已注册 `storyboard_director`。
- `storyboard_director` 输入 `episode_writer`、`character_bible` 和 `style_bible` envelopes。
- `storyboard_director` 输出 `shots`、`timing_summary`、`image_video_readiness`、`review` 和 `checkpoint.required=true`。
- 每个 shot 输出 `duration_seconds`、`scene`、`characters`、`shot_size`、`camera`、`lighting`、`visual_action`、`dialogue`、`narration`、`image_prompt_seed`、`video_prompt_seed` 和连续性说明。
- 已新增 `tests\test_stage7_storyboard_pipeline.py`，覆盖 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible -> storyboard_director` 链路和失败 envelope。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m compileall -q backend\sidecars\python-skills\milu_studio_skills backend\sidecars\python-skills\skills backend\sidecars\python-skills\tests`，通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -p test*.py`，13 个测试通过。
- 已运行 `storyboard_director` example CLI smoke，输出 8 个镜头、45 秒总时长、`timing_summary.within_tolerance=true`。
- 已运行 `D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln --nologo`，通过。
- 已运行 `npm run build`，通过。

Design check:

- cohesion: `storyboard_director` 只负责把脚本、角色和画风结构转为可审阅镜头列表；validator 只负责 envelope、镜头数量和时长契约。
- coupling: Stage 7 仍只通过 `SkillGateway` / CLI envelope 串联；UI 未直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: 不写数据库，不生成分镜图、首尾帧、视频、配音、字幕文件或 FFmpeg；后续保存仍留给 PostgreSQL adapter / EF Core DbContext。
- dependency inversion: 当前 skill 仍为 deterministic mock，真实模型 provider、资产生成和持久化都保留在后续 adapter / gateway 边界。
- temporary debt: 前端分镜表、用户增删改、单镜头重新生成、持久化编辑和下游重算本阶段未做，已在阶段计划中延后。

Environment check:

- project-local files: 新增源码、schema、examples 和测试均位于 `D:\code\MiLuStudio\backend\sidecars\python-skills`。
- D drive only: Python 使用 `D:\soft\program\Python\Python313\python.exe`；`.NET` 使用 `D:\soft\program\dotnet\dotnet.exe`；npm 使用 `D:\soft\program\nodejs\npm.ps1`；pycache、CLI 输出、构建缓存继续由 `scripts\windows\Set-MiLuStudioEnv.ps1` 指向 D 盘。
- C drive risk: `rg.exe` 仍因 C 盘 WindowsApps 权限不可用，本阶段改用 PowerShell 枚举；未主动写入 C 盘依赖、缓存、运行数据或生成结果。

Skills check:

- 本阶段没有前端实现，未调用 frontend-skill、playwright-interactive、impeccable 或 gpt-taste。
- `playwright-interactive` 当前工具列表没有 `js_repl`，且本阶段无 UI 交互改动。

Web searches:

- LumenX GitHub: `https://github.com/alibaba/lumenx`
- Codeywood: `https://codeywood.com/`
- OpenMontage GitHub: `https://github.com/calesthio/OpenMontage`

Internet self-check:

- LumenX README 显示 StoryBoard 阶段基于脚本提取分镜脚本，形成结构化故事版，并在后续为每个分镜场景选择角色、场景、道具参考图生成分镜图；Stage 7 当前只产出结构化 shot list，没有提前生成图像或视频。
- Codeywood 页面显示 Story -> Character/Location -> Reference -> Shot -> Video 的顺序，并包含 `storyboards` 和 `shot-quality-validator`；Stage 7 输出可消费 shot 结构和 review gate，符合先 Shot 后 Video 的顺序。
- OpenMontage README 强调生产流程由 `pipeline_defs/`、stage director skills、schemas、checkpoint 和 review gates 驱动；Stage 7 继续采用 skill 目录、schema、validator 和 checkpoint，不复制其 AGPL 代码和 FFmpeg/Remotion 依赖。

Deviation reason:

- 原 Stage 7 文档把“前端显示分镜表、支持用户增删改分镜、支持单个镜头重新生成”放在同一阶段。
- 本轮用户明确要求基于现有 envelopes 先实现 `storyboard_director` 内部 Production Skill 边界，并要求后续保存到数据库留给 PostgreSQL adapter / EF Core DbContext，不让 UI 直接访问 Python、文件系统或数据库。
- 联网自检也确认分镜图、参考图选择、视频片段和编辑器属于结构化 shot list 之后的资产 / UI 阶段；因此已先更新 `MILUSTUDIO_BUILD_PLAN.md` 和 `MILUSTUDIO_PHASE_PLAN.md`，将 Stage 7 收敛为可审阅 JSON 结构，UI/DB/重生成延后。

Build plan changes:

- Stage 7 目标改为“在 Python Skills Runtime 内生成可审阅、可执行的镜头列表结构”。
- Stage 7 步骤补充 `storyboard_director` 的 envelope 输入输出、gateway 注册、schema、examples、tests 和 CLI smoke。
- Stage 7 明确不保存数据库、不接真实模型、不生成分镜图、不触发图片/视频/FFmpeg、不让 UI 直接调用 Python。
- 前端分镜表、增删改、单镜头重生成、分镜图、首尾帧、视频片段和资产选择已标为延后。

Phase plan changes:

- Stage 7 标记为 `done`。
- 当前焦点改为 Stage 8。
- Stage 7 落地状态补充新增 skill、gateway、测试和 deferred 项。

Handoff changes:

- 更新当前接棒状态为 Stage 8 preparation。
- 记录 Stage 7 `storyboard_director`、测试命令、验证结果、临时债和下一阶段边界。

Next phase:

- Stage 8 图片生成。
- 基于 `storyboard_director`、`character_bible` 和 `style_bible` envelopes 实现 `image_prompt_builder` 和 mock image generation 边界；真实 provider、资产持久化、前端选择和重试仍等待后续 Control API / PostgreSQL adapter 收敛。

### 2026-05-13 Stage 8 完成

Date:

- 2026-05-13

Stage:

- Stage 8 图片生成边界。

Local verification:

- 已新增 `skills\image_prompt_builder`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已新增 `skills\image_generation`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- `SkillGateway.default()` 已注册 `image_prompt_builder` 和 `image_generation`。
- `image_prompt_builder` 输入 `storyboard_director`、`character_bible` 和 `style_bible` envelopes，输出角色参考、分镜图、首帧和尾帧的结构化 `image_requests`。
- `image_generation` 输入 `image_prompt_builder` envelope，输出 mock 占位资产、`asset_manifest`、零成本估算、review 信息和 `checkpoint.required=true`。
- mock 资产只生成逻辑 `milu://mock-assets/...` URI 和 `storage_intent`；`file_written=false`、`writes_files=false`、`writes_database=false`。
- 已新增 `tests\test_stage8_image_pipeline.py`，覆盖 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible -> storyboard_director -> image_prompt_builder -> image_generation` 完整链路和失败 envelope。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m compileall backend\sidecars\python-skills`，通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s backend\sidecars\python-skills\tests -v`，16 个测试通过。
- 已运行 `image_prompt_builder` 和 `image_generation` examples CLI smoke，均通过。
- 已运行 `dotnet build backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore`，通过。
- 已用 `D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1` 重新约束环境后运行 `npm run build`，通过。

Design check:

- cohesion: `image_prompt_builder` 只负责把分镜、角色和画风结构转成图像请求；`image_generation` 只负责把请求转成 deterministic mock asset records。
- coupling: Stage 8 仍只通过 `SkillGateway` / CLI envelope 串联；UI 未直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: 本阶段不写数据库，不写真实图片文件，不上传参考图，不调用真实图片模型，不触发视频、音频、字幕或 FFmpeg。
- dependency inversion: 真实 image provider、存储 provider、成本记录、重试和资产选择保留在后续 adapter / Control API 边界，不塞进 skill executor。
- temporary debt: 前端图片预览、选中最佳图片、单镜头重试、真实 provider adapter 和数据库持久化均未做，已在阶段计划中延后。

Environment check:

- project-local files: 新增源码、schema、examples、CLI 输出和测试均位于 `D:\code\MiLuStudio\backend\sidecars\python-skills`。
- D drive only: Python 使用 `D:\soft\program\Python\Python313\python.exe`；.NET / npm 构建通过 `scripts\windows\Set-MiLuStudioEnv.ps1` 指向 D 盘缓存和本项目目录。
- C drive risk: 第一次前端构建命令在 `apps\web` 下点短路径环境脚本失败，构建仍成功；随后已用绝对路径 `D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1` 重新构建通过。该误触未安装依赖、未下载缓存、未生成项目外产物。

Skills check:

- 本阶段没有前端实现，未调用 `frontend-skill`、`playwright-interactive`、`impeccable` 或 `gpt-taste`。

Web searches:

- LumenX GitHub: `https://github.com/alibaba/lumenx`
- OpenAI Images API reference: `https://developers.openai.com/api/reference/resources/images`
- getimg.ai first and last frame guide: `https://getimg.ai/guides/guide-to-first-and-last-frame-control`
- StoryEnsemble paper / project: `https://sanghosuh.github.io/papers/storyensemble_uist.pdf`

Internet self-check:

- LumenX README 显示短漫剧链路包含资产提取、风格定调、资产生成、分镜脚本构造、分镜图生成和分镜视频生成；Stage 8 当前先生成 prompt request 和 mock asset gate，未提前引入真实 provider，符合分层推进。
- LumenX Assets / StoryBoard 说明角色一致性依赖角色资产和参考图，分镜图阶段基于角色、场景、道具参考生成；本阶段只生成角色参考请求和分镜图 / 首尾帧请求，把真实参考图生成留后。
- OpenAI Images API 官方文档显示真实图像生成需要 prompt 或 input image 调用 `/images/generations`、`/images/edits` 或 `/images/variations`；本阶段明确 `provider=none` / `provider=mock`，未接真实模型，符合用户约束。
- getimg.ai 首帧 / 尾帧指南说明首帧控制开头、尾帧控制结尾，供 image-to-video 阶段消费；Stage 8 输出 `first_frame` 和 `last_frame` 占位资产结构，能服务后续视频阶段。

Deviation reason:

- 原 Stage 8 文档把 `ImageProvider` interface、真实或 mock provider、保存图片资产、前端选图和失败重试放在同一阶段。
- 本轮用户明确要求先实现 `image_prompt_builder` 和 mock image generation 的内部 Production Skill 边界，后续保存到数据库仍留给 PostgreSQL adapter / EF Core DbContext 阶段，并且不要接真实模型、不要让 UI 直接访问 Python / 文件系统 / 数据库。
- 联网自检也确认真实图片 provider、参考图上传、资产落盘、选图和重试属于 prompt / storyboard gate 之后的 provider / adapter / UI 阶段。
- 因此已先更新 `MILUSTUDIO_BUILD_PLAN.md` 和 `MILUSTUDIO_PHASE_PLAN.md`：Stage 8 收敛为图像 prompt request + mock asset boundary；真实 provider、文件写入、数据库资产和前端选图全部延后。

Build plan changes:

- Stage 8 标题改为“图片生成边界”。
- Stage 8 目标改为生成后续图像阶段可消费的提示词请求和 mock 占位资产结构。
- Stage 8 明确不接真实图片 provider、不写图片文件、不写数据库、不做前端选图、不做单镜头真实重试。
- Stage 8 步骤补充 `image_prompt_builder`、mock `image_generation`、gateway 注册、schema、examples、tests 和 CLI smoke。
- Stage 8 延后项补充真实 provider adapter、资产持久化、`assets` / `shot_assets` 写入、前端选图和重试。

Phase plan changes:

- Stage 8 标记为 `done`。
- 当前焦点改为 Stage 9。
- Stage 8 落地状态补充两个新 skill、完整 envelope 链路、mock asset 不写文件 / 不写数据库约束和 deferred 项。

Handoff changes:

- 当前接棒状态更新为 Stage 9 preparation。
- 记录 Stage 8 两个 skill、验证命令、约束、技术债和下一阶段边界。

Next phase:

- Stage 9 视频生成边界。
- 建议基于 `storyboard_director`、`image_prompt_builder` 和 `image_generation` envelopes 实现 `video_prompt_builder` 与 mock video generation boundary。
- 继续不接真实视频 provider，不触发 FFmpeg，不写数据库，不让 UI 绕过 Control API / Worker。

### 2026-05-13 Stage 9 完成

Date:

- 2026-05-13

Stage:

- Stage 9 视频生成边界。

Local verification:

- 已新增 `skills\video_prompt_builder`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已新增 `skills\video_generation`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- `SkillGateway.default()` 已注册 `video_prompt_builder` 和 `video_generation`。
- `video_prompt_builder` 输入 `storyboard_director`、`image_prompt_builder` 和 `image_generation` envelopes，输出每镜头一个结构化 `video_request`。
- `video_prompt_builder` 能引用 Stage 8 的 `storyboard_image`、`first_frame`、`last_frame` 和 `character_reference` mock 资产；首尾帧齐全时输出 `generation_mode=image_to_video`。
- `video_generation` 输入 `video_prompt_builder` envelope，输出每镜头一个 mock 占位视频片段、`clip_manifest`、零成本估算、review 信息和 `checkpoint.required=true`。
- mock 视频片段只生成逻辑 `milu://mock-assets/...` URI 和 `storage_intent`；`file_written=false`、`writes_files=false`、`writes_database=false`、`uses_ffmpeg=false`。
- 已新增 `tests\test_stage9_video_pipeline.py`，覆盖 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible -> storyboard_director -> image_prompt_builder -> image_generation -> video_prompt_builder -> video_generation` 完整链路和失败 envelope。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m compileall -q milu_studio_skills skills tests`，通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -v`，19 个测试通过。
- 已运行 `video_prompt_builder` 和 `video_generation` examples CLI smoke，均通过并生成 examples output。
- 已运行 `D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore`，通过。
- 已运行 `D:\soft\program\nodejs\npm.ps1 run build`，通过。

Design check:

- cohesion: `video_prompt_builder` 只负责把分镜、图像提示词和 mock 图片资产转成视频请求；`video_generation` 只负责把请求转成 deterministic mock video clip records。
- coupling: Stage 9 仍只通过 `SkillGateway` / CLI envelope 串联；UI 未直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: 本阶段不写数据库，不写真实 MP4 文件，不调用真实视频模型，不触发 FFmpeg，不做前端片段预览。
- dependency inversion: 真实 video provider、存储 provider、成本记录、重试、任务轮询和资产选择保留在后续 adapter / Control API 边界，不塞进 skill executor。
- temporary debt: `VideoProvider` interface、真实 provider adapter、单镜头重试、视频资产持久化、前端预览和选择均未做，已在阶段计划中延后。

Environment check:

- project-local files: 新增源码、schema、examples、CLI 输出和测试均位于 `D:\code\MiLuStudio\backend\sidecars\python-skills`。
- D drive only: Python 使用 `D:\soft\program\Python\Python313\python.exe`；.NET 使用 `D:\soft\program\dotnet\dotnet.exe`；npm 使用 `D:\soft\program\nodejs\npm.ps1`；构建和缓存继续通过 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束在 D 盘和项目目录。
- C drive risk: 曾误用不存在的 `$env:MILUSTUDIO_DOTNET` / `$env:MILUSTUDIO_NPM` 变量，命令未执行构建；随后已用 D 盘绝对路径重新验证通过。该误触未安装依赖、未下载缓存、未生成项目外产物。

Skills check:

- 本阶段没有前端实现，未调用 `frontend-skill`、`playwright-interactive`、`impeccable` 或 `gpt-taste`。

Web searches:

- LumenX GitHub: `https://github.com/alibaba/lumenx`
- OpenAI Videos API reference: `https://developers.openai.com/api/reference/resources/videos/methods/create`
- Kling AI Start and End Frames: `https://kling.ai/quickstart/ai-video-start-end-frames`
- Kling API docs: `https://klingapi.com/docs`

Internet self-check:

- LumenX README 显示 Motion 阶段在分镜图之后，按镜头生成分镜视频，并在 Assembly 阶段再审查、选择和拼接；Stage 9 当前先产出视频请求和 mock clip review gate，没有提前进入拼接和 FFmpeg。
- OpenAI Videos API 官方文档显示真实视频生成需要 prompt、可选 image reference、model、seconds、size，并返回异步 video job 状态；本阶段只保留 prompt / source image / duration / aspect ratio 结构，不接真实 API。
- Kling 首尾帧说明显示 image-to-video 可使用开始帧和结束帧控制动态转场，且首尾帧差异过大可能导致镜头切换；Stage 9 明确把 `first_frame` / `last_frame` 作为 source images 并加入 continuity / negative prompt guardrails。
- Kling API 文档显示视频生成通常有 text2video、image2video、任务状态查询、duration、aspect_ratio 和 negative_prompt 等边界；Stage 9 输出这些可映射的结构，但把 provider、轮询、成本和失败重试留给后续 adapter。

Deviation reason:

- 原 Stage 9 文档把 `VideoProvider` interface、真实图生视频 / 文生视频、单镜头重试、成本记录、保存视频片段和前端预览放在同一阶段。
- 本轮用户明确要求先基于现有 `storyboard_director`、`image_prompt_builder` 和 `image_generation` envelopes 实现 `video_prompt_builder` 与 mock `video_generation` 的内部 Production Skill 边界，后续保存到数据库仍留给 PostgreSQL adapter / EF Core DbContext 阶段。
- 联网自检也确认真实视频 provider、任务轮询、MP4 写入、FFmpeg 拼接、片段预览和单镜头重试属于 prompt / mock clip boundary 之后的 provider / adapter / UI 阶段。

Build plan changes:

- Stage 9 标题改为“视频生成边界”。
- Stage 9 目标改为生成后续视频阶段可消费的视频提示词请求和 mock 占位视频片段结构。
- Stage 9 明确不接真实视频 provider、不写 MP4 文件、不写数据库、不调用 FFmpeg、不做前端片段预览、不做单镜头真实重试。
- Stage 9 步骤补充 `video_prompt_builder`、mock `video_generation`、gateway 注册、schema、examples、tests 和 CLI smoke。
- Stage 9 延后项补充真实 `VideoProvider` / adapter、成本记录、任务轮询、资产持久化、前端预览和重试。

Phase plan changes:

- Stage 9 标记为 `done`。
- 当前焦点改为 Stage 10。
- Stage 9 落地状态补充两个新 skill、完整 envelope 链路、mock clip 不写文件 / 不写数据库 / 不触发 FFmpeg 约束和 deferred 项。

Handoff changes:

- 当前接棒状态更新为 Stage 10 preparation。
- 记录 Stage 9 两个 skill、验证命令、约束、技术债和下一阶段边界。

Next phase:

- Stage 10 音频、字幕、剪辑边界。
- 建议基于 `video_generation` mock clips、`episode_writer` subtitle cues 和现有 storyboard timing 先实现 `voice_casting` / `subtitle_generator` / `auto_editor` 的内部边界。
- 继续不接真实 TTS、BGM、FFmpeg 或数据库；真实音视频文件写入和最终 MP4 仍留给后续 provider / adapter / packaging 阶段。
