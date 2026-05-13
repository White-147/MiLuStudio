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

### 2026-05-13 Stage 10 完成

Date:

- 2026-05-13

Stage:

- Stage 10 音频、字幕、剪辑边界。

Local verification:

- 已新增 `skills\voice_casting`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已新增 `skills\subtitle_generator`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已新增 `skills\auto_editor`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- `SkillGateway.default()` 已注册 `voice_casting`、`subtitle_generator` 和 `auto_editor`。
- `voice_casting` 输入 `episode_writer` 和 `storyboard_director` envelopes，输出 narrator / speaker voice profiles、逐段 `voice_tasks`、逻辑音频 asset intent、零成本估算和 `checkpoint.required=true`。
- `subtitle_generator` 输入 `episode_writer`、`storyboard_director` 和 `voice_casting` envelopes，输出 `subtitle_cues`、SRT 文本结构、逻辑字幕 asset intent 和 review warnings。
- `auto_editor` 输入 `storyboard_director`、`video_generation`、`voice_casting` 和 `subtitle_generator` envelopes，输出 video / audio / subtitle timeline tracks、rough edit `render_plan`、逻辑 MP4 output intent 和 review warnings。
- Stage 10 输出均明确 `provider=none`、`model=none`、`file_written=false`、`writes_files=false`、`writes_database=false`、`uses_ffmpeg=false`。
- 已新增 `tests\test_stage10_audio_subtitle_edit_pipeline.py`，覆盖 Stage 4-10 完整 envelope 链路和三个新 skill 的失败 envelope。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m compileall -q milu_studio_skills skills tests`，通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -v`，23 个测试通过。
- 已运行 `voice_casting`、`subtitle_generator` 和 `auto_editor` examples CLI smoke，均通过并生成 examples output。
- 已运行 `D:\soft\program\nodejs\npm.ps1 run build`，通过。
- 默认 `D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore` 曾因正在运行的 `MiLuStudio.Api (7832)` 锁定 Debug DLL 失败；随后改用临时 D 盘输出目录 `-p:OutputPath=D:\code\MiLuStudio\.codex-tmp\control-plane-build\` 验证通过，0 warning / 0 error，并已清理临时目录。

Design check:

- cohesion: `voice_casting` 只负责把脚本段落和分镜 timing 转成配音任务；`subtitle_generator` 只负责把配音任务转成 subtitle cue / SRT-ready 文本；`auto_editor` 只负责把 mock 视频、配音任务和字幕 cue 转成粗剪 timeline / render plan。
- coupling: Stage 10 仍只通过 `SkillGateway` / CLI envelope 串联；UI 未直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- boundaries: 本阶段不写数据库，不写 WAV / SRT / MP4 文件，不调用真实 TTS / BGM / SFX provider，不触发 FFmpeg，不做前端预览或下载。
- dependency inversion: 真实 `AudioProvider`、BGM/SFX、FFmpeg render adapter、存储 provider、成本记录、重试和资产持久化保留在后续 adapter / Control API / export packaging 边界。
- temporary debt: 音色试听、真实配音、音量标准化、字幕文件写入、BGM/SFX、最终 MP4、下载区 UI 和数据库资产索引均未做，已在阶段计划中延后。

Environment check:

- project-local files: 新增源码、schema、examples、CLI 输出和测试均位于 `D:\code\MiLuStudio\backend\sidecars\python-skills`。
- D drive only: Python 使用 `D:\soft\program\Python\Python313\python.exe`；.NET 使用 `D:\soft\program\dotnet\dotnet.exe`；npm 使用 `D:\soft\program\nodejs\npm.ps1`；构建和缓存继续通过 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束在 D 盘和项目目录。
- C drive risk: 未安装依赖、未下载缓存、未生成项目外产物。临时 .NET 输出目录位于 `D:\code\MiLuStudio\.codex-tmp`，验证后已删除。

Skills check:

- 本阶段没有前端实现，未调用 `frontend-skill`、`playwright-interactive`、`impeccable` 或 `gpt-taste`。

Web searches:

- OpenAI Audio and speech docs: `https://developers.openai.com/api/docs/guides/audio`
- OpenAI Text to speech docs: `https://developers.openai.com/api/docs/guides/text-to-speech`
- FFmpeg official docs: `https://ffmpeg.org/ffmpeg.html`
- Auto-Editor v3 timeline docs: `https://auto-editor.com/docs/v3`
- SRT cue format reference: `https://wiki.x266.mov/docs/subtitles/SRT`

Internet self-check:

- OpenAI Audio docs show audio systems can separate text prompts, text transcripts, audio input and audio output; Stage 10 keeps script text, voice task and subtitle cue boundaries explicit before any real speech generation.
- OpenAI Text-to-speech docs show real speech generation is a provider call that can stream or write audio formats; Stage 10 intentionally outputs `provider=none` and logical audio intents only.
- FFmpeg docs show filtering / muxing is an explicit media-processing boundary; Stage 10 keeps `uses_ffmpeg=false` and only emits a render plan for later adapter work.
- Auto-Editor v3 docs model editing as a JSON timeline with video and audio layers; Stage 10 mirrors that shape with deterministic video / audio / subtitle tracks instead of rendering media.
- SRT format references use ordered cues with start/end timestamps and text; Stage 10 emits ordered cue structures plus SRT-ready text without writing a `.srt` file.

Deviation reason:

- 原 Stage 10 文档把 TTS provider、真实配音、SRT 文件、FFmpeg 拼接、BGM/SFX 和完整 MP4 都放在同一阶段。
- 本轮用户明确要求先基于 `episode_writer`、`storyboard_director` 和 `video_generation` envelopes 收敛 `voice_casting` / `subtitle_generator` / `auto_editor` 内部 Production Skill 边界，后续保存到数据库仍留给 PostgreSQL adapter / EF Core DbContext 阶段。
- 联网自检也确认真实 TTS、字幕文件写入、音频处理、FFmpeg 渲染、最终 MP4 和下载 UI 应位于计划结构之后的 provider / adapter / export 阶段。

Build plan changes:

- Stage 10 目标改为生成后续阶段可消费的配音任务、SRT-ready 字幕结构和粗剪计划结构。
- Stage 10 明确不接真实 TTS / BGM / SFX provider、不写 WAV / SRT / MP4、不写数据库、不调用 FFmpeg、不做前端预览或下载。
- Stage 10 步骤补充 `voice_casting`、`subtitle_generator`、`auto_editor`、gateway 注册、schema、examples、tests 和 CLI smoke。
- Stage 10 延后项补充真实 `AudioProvider` / TTS adapter、音色试听、BGM/SFX、音量标准化、字幕文件落盘、FFmpeg、MP4、资产持久化和 UI 下载。

Phase plan changes:

- Stage 10 标记为 `done`。
- 当前焦点改为 Stage 11。
- Stage 10 落地状态补充三个新 skill、完整 envelope 链路、mock asset intent 不写文件 / 不写数据库 / 不触发 FFmpeg 约束和 deferred 项。

Handoff changes:

- 当前接棒状态更新为 Stage 11 preparation。
- 记录 Stage 10 三个 skill、验证命令、约束、技术债和下一阶段边界。

Next phase:

- Stage 11 质量检查边界。
- 建议基于 Stage 10 的 rough edit timeline、subtitle cues、voice tasks、mock video clips 和上游角色 / 分镜结构实现 `quality_checker` 的内部 Production Skill boundary。
- 继续不接真实视觉 / 音频检测 provider，不读取真实文件，不触发 FFmpeg，不写数据库，不让 UI 绕过 Control API / Worker。

### 2026-05-13 桌面打包和账号授权方案确认

Date:

- 2026-05-13

Decision:

- Stage 12 桌面打包唯一方案收敛为 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。
- 保留现有 Web UI 和 Control API 作为可独立迭代的前后端；Electron 只作为桌面宿主、安装器入口和本地进程管理层。
- 账号系统不阻塞 Stage 12 桌面安装包 MVP，作为桌面 MVP 后下一大阶段；届时对齐 QQ 类体验：应用打开只显示登录 / 注册 / 激活入口，登录后才能进入工作台。
- 安装器可加入付费码 / 激活码输入页，校验通过后才能继续安装；但这只作为安装前门槛，正式授权仍必须由 Control API / Auth & Licensing adapter 在应用内校验。

Web searches:

- electron-builder NSIS docs: `https://www.electron.build/nsis.html`
- NSIS custom page reference: `https://nsis.sourceforge.io/Reference/Page`
- NSIS nsDialogs docs: `https://nsis.sourceforge.io/Docs/nsDialogs/Readme.html`
- NSIS best practices: `https://nsis.sourceforge.io/Best_practices`

Internet self-check:

- electron-builder NSIS 官方文档支持 `include` 自定义 NSIS 脚本，并保留默认 NSIS 安装器能力；适合在 MVP 中作为唯一安装器方案。
- NSIS `Page custom` 和 nsDialogs 支持自定义输入页、输入框和 leave function 校验，因此安装器内付费码 / 激活码门槛技术上可实现。
- electron-builder NSIS 支持 assisted installer 相关配置，例如非 one-click、选择安装目录、安装器图标、卸载器图标、桌面快捷方式和开始菜单快捷方式。
- NSIS best practices 明确不推荐程序化固定任务栏；因此文档将任务栏要求调整为正确 AppUserModelID / 快捷方式 / 用户引导，允许用户自行固定，不做静默强制 pin。

Build plan changes:

- 桌面技术栈改为唯一 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。
- 新增 QQ / Adobe 类安装器要求：自定义图标、安装目录选择、桌面快捷方式、开始菜单快捷方式、开机自启动、安装进度条、安装完成后启动、付费码 / 激活码输入页。
- 新增登录与授权原则：账号系统作为桌面 MVP 后下一大阶段；安装器激活码不是唯一安全边界；正式授权由 Control API / Auth & Licensing adapter 执行。
- 新增账号、会话、设备和许可证数据模型草案，但不作为 Stage 12 阻塞项。
- Stage 12 步骤和验收更新为唯一 NSIS assisted installer 方案。

Phase plan changes:

- Stage 12 目标、任务、验收和阶段自检重点同步更新。

Handoff changes:

- 固定约束和技术债补充桌面打包、登录授权和任务栏 pin 的决策。

### 2026-05-13 数据库与桌面端解耦方案确认

Date:

- 2026-05-13

Decision:

- 数据库能力不和桌面端绑定。
- Stage 11 质量检查完成后，先进入 Stage 12 PostgreSQL 持久化与后端收敛。
- 原桌面打包阶段后移为 Stage 13，并作为独立交付 part，在 Web UI、Control API、Worker、PostgreSQL adapter 和核心生产功能相对稳定后推进。
- Electron 不直接访问 PostgreSQL，不执行 migrations，不定义数据库表，不负责数据库初始化。
- 桌面端只通过 Control API health / preflight 和业务 API 检查、展示和使用后端数据库状态。

Build plan changes:

- 新增“数据库属于后端基础设施，必须先在 Control API / Worker / Infrastructure 内完成，不和桌面安装器绑定”的架构原则。
- 新增 Stage 12：PostgreSQL 持久化与后端收敛。
- 原 Stage 12 桌面打包改为 Stage 13 桌面打包。
- Stage 13 删除“首次启动初始化数据库和 storage”作为 Electron 职责，改为调用 Control API health / preflight；storage 检查也由后端 service 处理。

Phase plan changes:

- 新增 Stage 12 PostgreSQL 持久化与后端收敛，任务包括 EF Core / Npgsql adapter、RepositoryProvider 切换、migration runner、preflight、Worker durable claiming 和数据库集成测试。
- 原 Stage 12 桌面打包改为 Stage 13 桌面打包，明确桌面端不直接访问数据库、文件系统业务目录、Python 脚本或 FFmpeg。

Handoff changes:

- 固定约束补充 Stage 12 数据库先行、Stage 13 桌面后置。
- 技术债补充 PostgreSQL adapter / EF Core DbContext、migration runner、preflight 和 Worker durable claiming 尚未实现。

Next phase order:

- Stage 11 质量检查。
- Stage 12 PostgreSQL 持久化与后端收敛。
- Stage 13 桌面打包。

### 2026-05-13 README 维护规则更新

Date:

- 2026-05-13

Decision:

- 在现有阶段结束联网自检、任务记录和 handoff 更新要求之外，新增长期规则：每次代码或文档修改完成后，都必须自行检查根 `README.md` 是否需要同步更新。
- 根 `README.md` 必须使用中文，必须 PowerShell 友好，并面向后续面试展示。
- README 内容风格参考 `D:\code\BookRecommendation\README.md` 和 `https://github.com/White-147/BookRecommendation`，重点覆盖项目背景、功能、技术栈、系统架构、目录结构、核心链路、运行说明、项目亮点、文档导航和后续改进方向。

Local check:

- 已读取本地 `D:\code\BookRecommendation\README.md`。
- 已打开 GitHub 项目 `White-147/BookRecommendation` 核对 README 展示结构。
- 已用 `Get-Content D:\code\MiLuStudio\README.md -Encoding UTF8` 检查当前 README 可在 PowerShell 中阅读。

Changes:

- `docs\MILUSTUDIO_BUILD_PLAN.md` 新增根 README 维护要求。
- `docs\MILUSTUDIO_PHASE_PLAN.md` 新增每棒先读 README、README check 字段和阶段完成后的 README 检查步骤。
- `docs\MILUSTUDIO_HANDOFF.md` 新增固定约束：每次修改完成后检查 README 是否需要同步更新。
- 根 `README.md` 已改为中文面试展示版，结构对齐 BookRecommendation 风格，同时保留 MiLuStudio 当前阶段、边界、数据库与桌面端解耦路线。

### 2026-05-13 Stage 11 完成

Date:

- 2026-05-13

Stage:

- Stage 11 质量检查边界。

Local verification:

- 已新增 `skills\quality_checker`，包含 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已在 `SkillGateway.default()` 注册 `quality_checker`。
- `quality_checker` 输入消费 `character_bible`、`style_bible`、`storyboard_director`、`video_generation`、`voice_casting`、`subtitle_generator` 和 `auto_editor` 成功 envelope。
- `quality_checker` 输出 `quality_status`、`score`、`issues`、`auto_retry_items`、`manual_review_items`、`quality_manifest`、`generation_plan`、`review` 和 `checkpoint`。
- Stage 11 输出明确 `provider=none`、`model=none`、`writes_files=false`、`writes_database=false`、`reads_media_files=false`、`uses_visual_detector=false`、`uses_audio_detector=false`、`uses_ffmpeg=false`。
- 已新增 `tests\test_stage11_quality_checker_pipeline.py`，覆盖 Stage 5-11 完整 envelope 链路、字幕过长可重试项和失败 envelope。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m py_compile backend\sidecars\python-skills\skills\quality_checker\executor.py backend\sidecars\python-skills\skills\quality_checker\validators.py backend\sidecars\python-skills\milu_studio_skills\gateway.py`，通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill quality_checker --input skills\quality_checker\examples\input.json --output skills\quality_checker\examples\output.json --pretty`，通过并生成 example output。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest tests.test_stage11_quality_checker_pipeline`，3 个测试通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -v`，26 个测试通过。
- 已运行 `D:\soft\program\Python\Python313\python.exe -m compileall -q milu_studio_skills skills tests`，通过。

Design check:

- cohesion: `quality_checker` 只负责结构化质检报告、严重级别、可重试项和人工确认 checkpoint，不混入真实 provider、数据库或媒体处理。
- coupling: 仍只通过 `SkillGateway` / CLI envelope 串联；UI 未直接访问数据库、文件系统、Python 脚本、模型 SDK 或 FFmpeg。
- boundaries: 本阶段不读取真实媒体文件，不接视觉 / 音频检测模型，不触发 FFmpeg，不写 MP4 / WAV / SRT / PNG，不写数据库。
- dependency inversion: 真实黑屏、卡顿、水印、分辨率、音量、字幕烧录检测、报告持久化和 Worker retry claiming 保留给后续 adapter / Control API / PostgreSQL 阶段。
- temporary debt: 当前只检查 envelope 元数据和 deterministic 结构；真实媒体 QA、自动续跑、失败镜头资产落盘、UI 质量报告卡片和数据库资产索引未做。

Environment check:

- project-local files: 新增文件均位于 `D:\code\MiLuStudio\backend\sidecars\python-skills`、`D:\code\MiLuStudio\docs` 和根 `README.md`。
- D drive only: 验证使用 `D:\soft\program\Python\Python313\python.exe`。
- C drive risk: 未新增 npm / NuGet / pip 依赖，未新增 Docker / Redis / Celery / Linux 生产依赖。

Internet self-check:

- OpenMontage README 显示生产级视频系统会在最终展示前做多点自检，包括 ffprobe validation、frame sampling、audio level analysis、delivery promise verification 和 subtitle checks；Stage 11 先做结构化质量报告，真实 ffprobe / frame / audio 检测留给后续 adapter，方向一致。
- OpenMontage README 还显示其流程含 reviewer skill、checkpoint state 和 human approval；Stage 11 的 `manual_review_items` 与 `checkpoint.required=true` 保留人工确认边界。
- FFmpeg filters 官方文档提供 `blackdetect`、`freezedetect`、`silencedetect` 等真实媒体检测能力；Stage 11 记录这些为后续真实媒体 QA adapter 参考，本阶段不调用 FFmpeg。
- Auto-Editor v1 docs 显示 timeline 可以用 JSON 表达，且要求前后片段无 gap；Stage 11 继续检查 rough edit timeline 与 storyboard timing 的结构一致性，不渲染媒体。

Sources:

- `https://github.com/calesthio/OpenMontage`
- `https://ffmpeg.org/ffmpeg-filters.html`
- `https://auto-editor.com/docs/v1`

Deviation reason:

- 原 Stage 11 文档把黑屏、卡顿、水印、尺寸和文件缺失检查写在同一阶段，容易暗示要读取真实媒体或调用 FFmpeg。
- 本轮用户明确要求 Stage 11 不接真实视觉 / 音频检测模型，不读取真实媒体文件，不触发 FFmpeg，不生成真实 MP4，不写数据库，并且不绕过 Control API / Worker。
- 联网自检也确认真实媒体检测属于渲染或媒体 adapter 后的 QA 阶段；因此 Stage 11 先收敛为 metadata-only quality boundary。

Build plan changes:

- Stage 11 目标改为内部 Production Skill 质量检查边界。
- Stage 11 步骤改为角色 / 风格 / timing / mock clip / 配音 / 字幕 / 粗剪计划结构质检。
- 明确真实黑屏、卡顿、水印、尺寸、音量和字幕烧录检测留给后续真实媒体 QA adapter。

Phase plan changes:

- Stage 11 标记为 `done`。
- 当前焦点改为 Stage 12 PostgreSQL 持久化与后端收敛。
- Stage 11 落地状态补充 `quality_checker`、gateway 注册、schema、examples、tests 和 CLI smoke。

README check:

- 已检查根 `README.md`，需要同步更新。
- 已将当前阶段改为 Stage 0-11 完成。
- 已将核心链路补充到 `auto_editor -> quality_checker`。
- 已补充质量报告、严重级别、可自动重试项和人工确认 checkpoint 说明。
- 已将后续改进方向中的 Stage 11 待办改为 Stage 12 PostgreSQL 和真实媒体 QA adapter。
- 已检查 `backend\sidecars\python-skills\README.md`，需要同步更新；已改为中文、PowerShell 友好的 Stage 11 sidecar 说明。

Handoff changes:

- 当前接棒状态更新为 Stage 12 preparation。
- 记录 Stage 11 新 skill、验证命令、约束、技术债和下一阶段边界。

Next phase:

- Stage 12 PostgreSQL 持久化与后端收敛。
- 将 Stage 5-11 的 Production Skill envelope 输出通过 Control API / Worker 写入数据库。
- 继续保持桌面端与数据库解耦；Electron 不直接访问 PostgreSQL、不执行 migrations、不调用 Python 或 FFmpeg。

### 2026-05-13 Stage 12 完成

Date:

- 2026-05-13

Stage:

- Stage 12 PostgreSQL 持久化与后端收敛。

Local verification:

- 已为 `MiLuStudio.Infrastructure` 引入 `Npgsql.EntityFrameworkCore.PostgreSQL`。
- 已新增 `MiLuStudioDbContext`，映射 projects、story_inputs、production_jobs、generation_tasks、assets、cost_ledger、characters 和 shots。
- 已新增 PostgreSQL repository，覆盖项目、生产任务、生成任务、资产和成本记录。
- 已保留 InMemory provider，通过 `ControlPlane:RepositoryProvider=InMemory` / `PostgreSQL` 切换。
- 已新增 `IControlPlanePreflightService`、`IControlPlaneMigrationService`、`IAssetRepository` 和 `ICostLedgerRepository`。
- 已新增 `/api/system/preflight`、`/api/system/migrations`、`/api/system/migrations/apply`、`/api/projects/{projectId}/assets`、`/api/projects/{projectId}/cost-ledger`、`/api/production-jobs/{jobId}/tasks` 和 `POST /api/generation-tasks/{taskId}/output`。
- 已新增 `002_stage12_postgresql_claiming.sql`，为 `generation_tasks` 增加 `queue_index`、`locked_by`、`locked_until`、`last_heartbeat_at` 和 claiming indexes。
- Worker 现在通过 repository 领取任务；PostgreSQL provider 使用 `FOR UPDATE SKIP LOCKED`，并可接管 lease 过期的 running task。
- Skill envelope 写回会更新 `generation_tasks.output_json`，并建立 `assets` 记录；请求包含成本字段时会写入 `cost_ledger`。
- 已运行 `. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1; D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\control-plane-stage12-build\`，0 warning / 0 error。
- 已运行 Python sidecar `compileall` 和 `unittest discover -s tests -v`，26 个测试通过。
- 已运行 InMemory Control API smoke，覆盖 health、preflight、创建项目、创建 production job、查询 tasks、写入 skill envelope 和查询 assets。
- 已运行 PostgreSQL provider 未配置可用数据库时的 preflight 负向 smoke，`/api/system/preflight` 返回 503 且包含结构化 preflight 诊断。
- 本机存在运行中的 `postgresql-x64-18` 服务，但当前未配置可用连接串和凭据；真实 PostgreSQL migration / API / Worker 共享状态集成测试留给本地数据库配置完成后执行。

Design check:

- cohesion: PostgreSQL schema mapping、repository、migration service、preflight service 和 Worker claiming 各自位于 Infrastructure / Application / Worker 边界内，职责清晰。
- coupling: API 和 Worker 只依赖 Application repository / service 抽象；UI 仍只通过 Control API DTO 通信，不直接访问数据库、文件系统、Python 脚本或 FFmpeg。
- boundaries: 数据库配置、migration、storage 检查和 skill envelope 写回都属于后端；Electron 未接入，也不承担 schema、migration 或初始化职责。
- dependency inversion: InMemory 与 PostgreSQL 通过同一 repository provider 切换；后续真实 Skill 执行、文件存储、FFmpeg 和 provider SDK 仍保留在 adapter / Worker 边界之后。
- temporary debt: 真实 PostgreSQL 集成测试尚需可用本地连接串；InMemory provider 仍是进程内 smoke，不跨 API / Worker 进程共享状态。

Environment check:

- project-local files: 新增源码位于 `D:\code\MiLuStudio\backend\control-plane`，新增 SQL migration 位于 `backend\control-plane\db\migrations`，新增专题文档位于 `docs\POSTGRESQL_STAGE12_SETUP.md`。
- D drive only: .NET 使用 `D:\soft\program\dotnet\dotnet.exe`；NuGet、构建输出、API smoke 日志均位于 `D:\code\MiLuStudio` 下的 `.nuget` / `.tmp`。
- C drive risk: 未安装新全局工具，未引入 Docker / Redis / Celery / Linux 生产依赖；本机已有 PostgreSQL Windows 服务未被本阶段修改。

Internet self-check:

- Npgsql 官方文档确认 PostgreSQL 的 EF Core provider 通过 `Npgsql.EntityFrameworkCore.PostgreSQL` 接入，并遵循通用 EF Core DbContext 模式。
- Microsoft EF Core provider 文档列出 `Npgsql.EntityFrameworkCore.PostgreSQL` 作为 PostgreSQL provider，当前选择符合 .NET 后端主线。
- PostgreSQL 官方 `SELECT` 文档说明 `SKIP LOCKED` 会跳过无法立即加锁的行，适合多个消费者访问 queue-like table 以避免锁竞争；当前 Worker claiming 方案一致。
- Microsoft ASP.NET Core health check 文档说明 DbContext 连接检查默认使用 EF Core `CanConnectAsync`；当前 preflight 的数据库可达性检查方向一致，并额外返回 migration 和 storage 建议。

Sources:

- `https://www.npgsql.org/efcore/?tabs=aspnet`
- `https://learn.microsoft.com/en-us/ef/core/providers/`
- `https://www.postgresql.org/docs/18/sql-select.html`
- `https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0`

Deviation reason:

- 无产品方向偏差。
- Stage 12 按用户要求先完成后端数据库能力，不推进桌面端，不把 PostgreSQL 安装、端口、账号、migration 或 storage 初始化藏进 Electron。
- 真实 PostgreSQL 实库集成测试因缺少已确认的本地数据库连接串和凭据未执行，已记录为下一步配置后验证项。

Build plan changes:

- 增加 Stage 12 当前落地状态，记录 DbContext、PostgreSQL repository、provider switch、preflight、migration API、Worker claiming 和 skill envelope 写回。
- 明确真实 PostgreSQL integration 需要配置好本地连接串后执行。

Phase plan changes:

- Stage 12 标记为 `done`。
- 当前焦点改为 Stage 13 桌面打包。
- Stage 12 落地状态补充 preflight、migration、durable claiming、skill envelope 写回和本地验证边界。

README check:

- 已检查根 `README.md`，需要同步更新。
- 已将项目阶段改为 Stage 0-12 完成。
- 已补充 PostgreSQL adapter、migration、preflight、Worker durable claiming 和 skill envelope 写回说明。
- 已修正“当前边界”中旧的“不写真实数据库”表述，改为默认 InMemory 不要求真实 PostgreSQL，切换 PostgreSQL 后由后端写入业务状态。

Handoff changes:

- 当前接棒状态更新为 Stage 13 preparation。
- 记录 Stage 12 新增后端能力、验证命令、真实 PostgreSQL 集成测试边界和下一阶段桌面打包提示词。

Next phase:

- Stage 13 真实配置、Worker-Skills 与前后端收敛验收。
- 默认 PostgreSQL、真实 `milu` 数据库、Worker 调 Python deterministic skills 和前端真实结果展示。
- Stage 14 再进入桌面打包，唯一方案仍为 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。

### 2026-05-13 Stage 13 文档同步

Date:

- 2026-05-13

Decision:

- 在桌面打包前插入新的 Stage 13：真实配置、Worker-Skills 与前后端收敛验收。
- 原 Stage 13 桌面打包整体后移为 Stage 14。
- 默认 `RepositoryProvider` 后续切到 `PostgreSQL`。
- 本机数据库复用 PostgreSQL 18 Windows 服务，使用 `root/root`，创建 MiLuStudio 专用业务库 `milu`。
- InMemory provider 保留为快速 smoke / 特殊轻量场景，不再作为默认业务事实来源。
- Worker 必须通过内部 adapter 调用 Python deterministic Production Skills，并通过后端 persistence / repository 边界写回 PostgreSQL。
- 前端必须通过 Control API 展示真实任务结果、skill envelope、资产索引和成本记录；不能直接访问数据库、文件系统、Python 脚本或 FFmpeg。

Reason:

- Stage 12 已具备 PostgreSQL adapter、migration、preflight 和 durable claiming 边界，但真实本机数据库配置、Worker 调 skill 和前端真实结果展示尚未收敛。
- 直接进入桌面打包会把仍在变化的前后端和数据库边界过早固化到安装器中，不利于后续功能迭代。
- 用户明确希望数据库不和桌面端绑定，桌面端作为独立 part 在核心前后端功能完善后继续。

Doc changes:

- `docs\MILUSTUDIO_PHASE_PLAN.md`：新增 Stage 13，原桌面打包改为 Stage 14，并更新当前焦点。
- `docs\MILUSTUDIO_BUILD_PLAN.md`：新增阶段 13 真实配置与收敛目标，原桌面打包改为阶段 14。
- `docs\MILUSTUDIO_HANDOFF.md`：更新当前接棒、固定约束、技术债、下一步建议和下一棒提示词。
- `docs\POSTGRESQL_STAGE12_SETUP.md`：同步 Stage 13 默认 PostgreSQL、`milu`、`root/root` 和验收要求。
- 根 `README.md`：同步当前阶段、默认持久化策略和后续 Stage 14 桌面打包位置。

README check:

- 已检查根 `README.md`，需要同步更新。
- README 已从 “Stage 0-12 完成、默认 InMemory” 调整为 “Stage 13 推进中、默认切向 PostgreSQL”。

Next phase:

- 继续 Stage 13 实现。
- 先写入 API / Worker 默认 PostgreSQL 配置和本机数据库初始化脚本。
- 再推进 Worker skill runner adapter、API 状态收敛和前端真实结果展示。

### 2026-05-13 Stage 13 完成

Date:

- 2026-05-13

Stage:

- Stage 13 真实配置、Worker-Skills 与前后端收敛验收。

Local verification:

- 已将 API / Worker 默认 `RepositoryProvider` 切到 `PostgreSQL`。
- 已将版本库连接串统一为 `Host=127.0.0.1;Port=5432;Database=milu;Username=root;Password=root`。
- 已新增 `scripts\windows\Initialize-MiLuStudioPostgreSql.ps1`，幂等创建 `milu`；本机 `root/root` 无 `CREATEDB` 权限时用 `postgres/root` bootstrap 建库并把 owner 设为 `root`。
- 已实际创建数据库 `milu`，并通过 Control API `/api/system/migrations/apply` 应用 `001_initial_control_plane` 和 `002_stage12_postgresql_claiming`。
- 已新增 `IProductionSkillRunner`、`PythonProductionSkillRunner` 和 `ProductionSkillExecutionService`，Worker 通过 Python CLI / `SkillGateway` 调用 deterministic skills。
- 已把 `plot_adaptation`、`image_prompt_builder`、`video_prompt_builder` 和新增 `export_packager` 纳入 production queue。
- 已新增 deterministic `export_packager` skill 和 `tests\test_stage13_export_packager.py`。
- 已将 `SystemClock` 改为 UTC，修复 Npgsql 写入 `timestamptz` 时 `DateTimeOffset +08:00` 不被接受的问题。
- 已修复 PostgreSQL repository 的 job / task 外键写入顺序，并在写入后清理 EF Core ChangeTracker，避免长链路跟踪冲突。
- 已将 Worker 空轮询间隔从 30 秒降为 3 秒，checkpoint 后能更快继续领取下一条 task。
- 已将 API SSE 改为读取数据库快照，不再由 API mock 自动推进生产状态。
- 已让前端通过 Control API 读取 job、tasks、assets、cost ledger，并用真实 `outputJson` envelope 构建结果卡和导出区。

Commands:

```powershell
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Initialize-MiLuStudioPostgreSql.ps1

. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage13-build\

# Control API on http://127.0.0.1:5368
# POST /api/system/migrations/apply
# GET /api/system/preflight
# Full smoke with API + Worker + automatic checkpoint approval

Push-Location D:\code\MiLuStudio\backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m compileall milu_studio_skills skills tests
& $env:MILUSTUDIO_PYTHON -m unittest discover -s tests -v
Pop-Location

cd D:\code\MiLuStudio\apps\web
npm run build
```

Smoke result:

- `job_ba4b02d1cd534e948fe0fda74aaead3c` completed / 100。
- `generation_tasks`: 15 rows, 15 completed, 15 output_json present。
- `cost_ledger`: 15 rows。

Design check:

- cohesion: Worker 调用 Python、输入构建、输出持久化集中在 Application / Infrastructure 边界，前端只消费 API DTO。
- coupling: UI 未接触数据库、文件系统、Python 脚本或 FFmpeg；Python skill 不写数据库；Electron 未提前介入数据库。
- boundaries: PostgreSQL migration、preflight、durable claiming、skill runner 和 envelope persistence 都保持在 Control API / Worker / Infrastructure 边界内。
- temporary debt: 端到端 smoke 已手工脚本化但还不是仓库内自动化集成测试；真实 provider、真实媒体文件、FFmpeg、下载文件和账号授权仍未实现。

Environment check:

- 本机使用 `postgresql-x64-18` Windows 服务。
- 应用业务连接使用 `milu` / `root` / `root`。
- 依赖、临时 skill run、构建输出和 storage 均在 `D:\code\MiLuStudio` 或明确 D 盘工具目录内。
- 未引入 Linux / Docker / Redis / Celery 作为生产依赖。

Web self-check:

- PostgreSQL 18 `SELECT` 文档确认 `FOR UPDATE ... SKIP LOCKED` 仍是可用的非阻塞任务领取能力：https://www.postgresql.org/docs/18/sql-select.html
- Npgsql Date / Time 文档确认 `DateTimeOffset` 写入 `timestamp with time zone` 仅支持 Offset=0，已用 UTC clock 修正：https://www.npgsql.org/doc/types/datetime.html
- Microsoft EF Core tracking 文档确认 DbContext 会跟踪实体实例，长链路重复 attach 需要控制跟踪状态，已在 repository 写入后清理 ChangeTracker：https://learn.microsoft.com/en-us/ef/core/querying/tracking
- electron-builder NSIS 文档确认 Stage 14 继续采用 NSIS assisted installer 和自定义脚本方向：https://www.electron.build/nsis.html

README check:

- 已检查根 `README.md`，需要同步更新。
- 已将 README 改为 Stage 0-13 完成、下一阶段 Stage 14 桌面打包。
- 已补充 `export_packager`、本机 PostgreSQL 初始化 / migration 命令、Stage 5-13 写回数据库和 Stage 13 已完成状态。
- 已检查 `backend\sidecars\python-skills\README.md`，需要同步更新；已补充 Stage 13、`export_packager` 和示例命令。

Doc changes:

- `docs\MILUSTUDIO_PHASE_PLAN.md`：Stage 13 标记为 `done`，当前焦点改为 Stage 14，并补充 Stage 13 落地状态。
- `docs\MILUSTUDIO_BUILD_PLAN.md`：补充 Stage 13 当前落地状态，移除“真实 PostgreSQL 集成测试待配置”的旧表述。
- `docs\POSTGRESQL_STAGE12_SETUP.md`：补充 `milu` 初始化脚本、bootstrap 权限说明和 Stage 13 smoke 结果。
- `docs\MILUSTUDIO_HANDOFF.md`：当前接棒改为 Stage 14，补充 Stage 13 落地内容、验证结果、技术债和下一棒提示词。
- 根 `README.md`：同步 Stage 13 完成状态和 Stage 14 后续方向。
- `backend\sidecars\python-skills\README.md`：同步 Stage 13 sidecar 范围和 `export_packager` 示例。

Next phase:

- Stage 14 桌面端独立打包。
- 唯一方案继续为 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。
- 桌面端不直接访问 PostgreSQL、不执行 migrations、不定义数据库表、不调用 Python 或 FFmpeg，只通过 Control API health / preflight 和业务 API 工作。

### 2026-05-13 Stage 14 补丁阶段文档同步

Date:

- 2026-05-13

Scope:

- 用户要求在进入最后桌面 Stage 前，把全项目只读检查发现的问题统合为 Stage 13 补丁阶段或继续推迟原 Stage 14。
- 本次只做文档同步，不修改业务代码。

Audit findings consolidated:

- 前端故事输入、标题、模式、时长和画幅仍主要停留在展示 / 默认值；启动生产时没有先把用户编辑内容保存到 Control API / PostgreSQL。
- `UpdateProjectRequest` 尚不支持更新 `story_inputs.original_text`，后端项目更新只覆盖部分项目元数据。
- Control API 端口在文档和代码中存在 `5268` / `5368` 双轨；Stage 15 桌面宿主需要明确 API base URL 注入策略。
- API CORS 当前只放行 Vite dev server；桌面本地 HTTP / 随机端口 / 静态承载策略需要先收敛。
- Web 路由依赖 History API pathname，桌面静态或 file URL 环境存在刷新和深链风险。
- UI 仍有 `Stage 1 mock` 文案，以及 `输出目录`、`锁定`、`重生成` 等无实际处理器按钮。
- checkpoint 当前前端默认 approve，缺少 reject / notes 等基本人工确认语义。
- 同一项目可重复启动多个 running production job，后续可能造成队列和展示混乱。
- `ControlPlaneOptions` 默认是 PostgreSQL，但配置节缺失时 infrastructure 会回落 InMemory，和 Stage 13 默认策略存在语义反差。
- `001_initial_control_plane.sql` 注释仍保留运行期使用 in-memory repository 的旧说法。
- `PythonProductionSkillRunner` 会创建 `.tmp\skill-runs`，但没有清理或保留策略。
- Python skills 的 schema / validator / skill.yaml / runtime registry 需要契约漂移检查。
- API / Worker / PostgreSQL 的重启恢复、lease 接管和 checkpoint 恢复还缺仓库内自动化集成测试。

Decision:

- 新增 `Stage 14：打包前补丁与 Stage 13 收敛`。
- 原 `Stage 14：桌面打包` 顺延为 `Stage 15：桌面打包`。
- Stage 14 不创建 `apps\desktop`，不接 Electron，不做安装器，不接真实模型，不读取真实媒体，不触发 FFmpeg。
- Stage 14 结束后再进入 Stage 15 Electron + electron-builder + NSIS assisted installer。

Doc changes:

- `docs\MILUSTUDIO_BUILD_PLAN.md`：新增阶段 14 补丁阶段，原桌面打包改为阶段 15。
- `docs\MILUSTUDIO_PHASE_PLAN.md`：当前阶段改为 Stage 14 补丁与打包前收敛，并补充详细任务、验收和自检重点。
- `docs\MILUSTUDIO_HANDOFF.md`：下一棒任务改为 Stage 14 补丁阶段，桌面打包后移到 Stage 15。
- 根 `README.md`：同步当前阶段和后续方向。

README check:

- 已检查根 `README.md`，需要同步更新。
- README 已改为 Stage 0-13 完成、下一阶段 Stage 14 打包前补丁、桌面打包顺延到 Stage 15。

Next phase:

- Stage 14 打包前补丁与 Stage 13 收敛。
- 完成真实用户输入保存、端口 / CORS / API base URL 收敛、UI mock 残留清理、PostgreSQL 默认语义加固、skill run 临时目录策略和集成测试补齐。

### 2026-05-13 Stage 14 完成

Date:

- 2026-05-13

Stage:

- Stage 14 打包前补丁与 Stage 13 收敛。

Local verification:

- 已打通 Web UI 真实输入保存链路：标题、故事文本、模式、目标时长、画幅和风格通过 `PATCH /api/projects/{projectId}` 写入 PostgreSQL，启动生产前先保存草稿。
- 已扩展 `UpdateProjectRequest`、`ProjectService`、repository 和 PostgreSQL adapter，支持更新 `story_inputs.original_text`、word count 和项目描述。
- 已统一 Control API 默认端口为 `http://127.0.0.1:5368`，并在前端支持 `window.__MILUSTUDIO_CONTROL_API_BASE__`、`VITE_CONTROL_API_BASE` 和默认端口三层解析。
- 已收敛 loopback CORS 和 hash route，为 Stage 15 桌面静态 / 本地 HTTP 承载预留边界。
- 已清理 `Stage 1 mock` 文案和无处理器按钮。
- checkpoint 已支持 approve / reject / notes；新增 `generation_tasks.checkpoint_notes` 与 `003_stage14_checkpoint_notes.sql`。
- 同一项目已有 active production job 时会返回现有 job，避免重复 running / paused job。
- PostgreSQL 是默认 provider；InMemory 只在显式配置 `RepositoryProvider=InMemory` 时启用。
- `.tmp\skill-runs` 默认保留最近 30 次运行，可通过 `ControlPlane:SkillRunRetentionCount` 调整。
- 已新增 `scripts\windows\Test-MiLuStudioStage14Integration.ps1`。
- 已新增 `backend\sidecars\python-skills\tests\test_stage14_skill_contracts.py`。

Commands:

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage14-build\

Push-Location D:\code\MiLuStudio\backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m compileall -q milu_studio_skills skills tests
& $env:MILUSTUDIO_PYTHON -m unittest discover -s tests -v
Pop-Location

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage14Integration.ps1
```

Verification result:

- .NET build：0 warning / 0 error。
- Python compileall + unittest：通过，29 tests。
- Web `npm run build`：通过。
- Stage 14 integration：`Stage 14 integration passed. Completed job: job_ce56423f476d4f1888115f42a2f4b3e0`。
- 集成脚本结束后确认 `5368` 无残留监听。

Design check:

- cohesion：用户输入保存、生产任务状态机、checkpoint 和 Worker skill input 仍分别位于 Web UI、Application、Infrastructure 和 Worker 边界内。
- coupling：UI 只通过 Control API DTO 通信；没有直接访问 PostgreSQL、文件系统、Python 脚本或 FFmpeg。
- boundaries：Electron / installer 未创建；Stage 14 只处理打包前补丁，不固化桌面端。
- recovery：集成脚本覆盖 API 重启、Worker 重启、lease 过期接管、checkpoint approve / reject notes 和 retry。

Environment check:

- 依赖、构建输出、skill run、脚本和测试均位于 `D:\code\MiLuStudio` 或明确 D 盘工具目录。
- 未引入 Linux / Docker / Redis / Celery 生产依赖。
- 未接真实模型 provider，未读取真实媒体文件，未触发 FFmpeg，未生成真实 MP4 / WAV / SRT / ZIP。

Web self-check:

- Electron security 官方文档继续强调 context isolation、sandbox、CSP、限制导航和不向不可信内容暴露 Electron API；Stage 15 的宿主注入方案必须只暴露 Control API base URL，不暴露数据库、文件系统、Python 或 FFmpeg 能力：https://www.electronjs.org/docs/latest/tutorial/security/
- electron-builder NSIS 官方文档确认 NSIS 是 Windows 默认 target，支持 assisted installer 和 `oneClick=false`；Stage 15 继续按 NSIS assisted installer 推进：https://www.electron.build/nsis.html
- PostgreSQL 18 `SELECT` 官方文档确认 `SKIP LOCKED` 适合 queue-like table 的多消费者避锁场景；Stage 14 的 Worker durable claiming 方向保持一致：https://www.postgresql.org/docs/18/sql-select.html
- Vite 官方文档确认 `import.meta.env` 在构建时静态替换，`VITE_*` 会暴露到客户端；因此 Stage 14 额外预留运行时 `window.__MILUSTUDIO_CONTROL_API_BASE__` 注入，适合 Stage 15 桌面宿主动态端口：https://vite.dev/guide/env-and-mode.html

Deviation reason:

- 无产品方向偏差。
- Stage 14 遵守边界：未创建 `apps\desktop`，未接 Electron，未做安装器，未接真实 provider，未绕过 Control API / Worker 边界。

Doc changes:

- `README.md`：同步 Stage 0-14 完成、Stage 15 下一步、003 migration、Stage 14 集成脚本和契约测试。
- `docs\MILUSTUDIO_BUILD_PLAN.md`：Stage 14 当前落地状态改为已完成。
- `docs\MILUSTUDIO_PHASE_PLAN.md`：当前焦点改为 Stage 15，Stage 14 状态改为 done，并补落地状态。
- `docs\MILUSTUDIO_HANDOFF.md`：下一棒提示词改为 Stage 15 桌面打包。
- `docs\MILUSTUDIO_TASK_RECORD.md`：追加本记录。

Next phase:

- Stage 15 Electron + electron-builder + NSIS 桌面打包。
- 创建 `apps\desktop`，承载现有 Web UI，启动 / 停止本地 Control API、Worker 和 Python sidecar，并通过 Control API health / preflight 展示系统状态。

### 2026-05-13 Stage 15 完成

Stage:

- Stage 15 Electron + electron-builder + NSIS 桌面打包。

Local verification:

- 已新增 `apps\desktop`，Electron 主进程承载现有 Web UI 构建产物，并通过本地 HTTP host 提供静态资源、hash route fallback 和 CSP。
- 桌面宿主会随机绑定本地端口，启动发布后的 Control API 与 Windows Worker，并通过 preload 向 Web UI 注入 `window.__MILUSTUDIO_CONTROL_API_BASE__`。
- Web UI 新增桌面诊断面板，通过 Electron IPC 展示 Control API health / preflight、PostgreSQL、storage、Python runtime、Python skills root、Worker 和 Web host 状态。
- 新增 `scripts\windows\Prepare-MiLuStudioDesktopRuntime.ps1`，复制 Web dist、API / Worker 发布产物、SQL migrations 和 Python deterministic skills 到 `apps\desktop\runtime`。
- 新增 `scripts\windows\Test-MiLuStudioDesktop.ps1`，覆盖 runtime 准备、桌面 TypeScript build 和 Electron smoke。
- 打包图标、安装器图标、卸载器图标、header 图标、快捷方式图标和托盘图标均来自 `apps\web\public\brand\logo.png` 生成的多尺寸 `apps\desktop\build\icon.ico`。
- electron-builder + NSIS 已配置 `oneClick=false`、`allowToChangeInstallationDirectory=true`、`runAfterFinish=true`、`shortcutName=MiLuStudio`、桌面快捷方式、开始菜单快捷方式、AppUserModelID 和自定义 `installer.nsh`。
- Electron `userData` 和 logs 已显式指向 D 盘数据目录，避免默认落到 `C:\Users\...\AppData\Roaming`。
- `apps\desktop\build\installer.nsh` 已预留安装前激活码输入页，以及桌面快捷方式、开始菜单快捷方式和开机自启动复选项；正式授权仍留给后续 Control API / Auth & Licensing adapter。
- 已生成 `D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe` 和 `win-unpacked`。

Commands:

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

Push-Location D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run build
D:\soft\program\nodejs\npm.ps1 run smoke
D:\soft\program\nodejs\npm.ps1 run pack:dir
D:\soft\program\nodejs\npm.ps1 run dist:win
Pop-Location

D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktop.ps1 -SkipInstall
& D:\code\MiLuStudio\outputs\desktop\win-unpacked\MiLuStudio.exe --smoke-test
```

Verification result:

- .NET build：0 warning / 0 error；第一次 Debug build 被旧 `MiLuStudio.Api (7832)` 锁定，确认属于本仓库后结束该残留进程并重跑通过。
- Web `npm run build`：通过。
- Desktop TypeScript build：通过。
- Electron smoke：Control API / Worker / Web host 均 running；preflight healthy=true；PostgreSQL reachable；migrations up_to_date；Python runtime 和 Python skills root 均 ok。
- `pack:dir`：生成 `D:\code\MiLuStudio\outputs\desktop\win-unpacked`。
- `dist:win`：生成 `D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe` 和 `.blockmap`。
- packaged smoke：`win-unpacked\MiLuStudio.exe --smoke-test` exit code 0；结束后无 `MiLuStudio.Api.exe` / `MiLuStudio.Worker.exe` 残留。
- packaged Electron `userData` 已落到 `D:\code\MiLuStudio\outputs\desktop\win-unpacked\data\.tmp\electron-user-data`。

Design check:

- cohesion：Electron 只负责桌面宿主、安装器、本地服务进程和受控 IPC；Web UI、Control API、Worker、Infrastructure 和 Python Skills 仍保持原有职责。
- coupling：UI 和 Electron 不直接访问 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg；Control API base URL 是唯一注入给 Web UI 的业务入口配置。
- boundaries：桌面端不执行 migrations、不定义数据库表、不负责数据库初始化；数据库未就绪时只展示 Control API preflight 的错误和建议。
- packaging：安装器激活码页是 Stage 15 安装门槛占位，不作为正式授权安全边界。

Environment check:

- npm、Electron 和 electron-builder 缓存通过 `scripts\windows\Set-MiLuStudioEnv.ps1` / `.npmrc` 指向 `D:\code\MiLuStudio\.cache`；Electron `userData` / logs 指向 D 盘数据目录。
- runtime、安装包、日志、输出目录和临时 skill run 路径均在 `D:\code\MiLuStudio` 或明确 D 盘路径。
- 未引入 Linux / Docker / Redis / Celery 生产依赖。
- 未接真实模型 provider，未读取真实媒体文件，未触发 FFmpeg，未生成真实 MP4 / WAV / SRT / ZIP。

Web self-check:

- Electron security 官方文档继续要求禁用 Node integration、启用 context isolation，并限制向网页暴露的 Electron API；Stage 15 只暴露 Control API base URL 和受控桌面命令：https://www.electronjs.org/docs/latest/tutorial/security/
- Electron app API 文档保留 `app.setAppUserModelId`；Stage 15 在主进程打开窗口前设置 AppUserModelID：https://www.electronjs.org/docs/latest/api/app/
- electron-builder NSIS 官方文档确认 NSIS 是 Windows 默认 target，并支持 `include` 自定义脚本、assisted installer、`oneClick=false`、`allowToChangeInstallationDirectory`、`runAfterFinish` 和 `shortcutName` 等配置：https://www.electron.build/nsis.html
- NSIS 官方文档确认 custom page 可由用户函数配合 nsDialogs 创建；Stage 15 用 `installer.nsh` 预留安装码页和自启动选项：https://nsis.sourceforge.io/Docs/nsDialogs/Readme.html

Deviation reason:

- 无产品方向偏差。
- Stage 15 遵守边界：未接真实 provider，未读取真实媒体，未调用 FFmpeg，未让 UI / Electron 绕过 Control API / Worker，未让桌面端执行 migrations、定义数据库表或负责数据库初始化。

Doc changes:

- `README.md`：同步 Stage 0-15 完成、桌面打包说明、桌面运行命令和后续方向。
- `docs\MILUSTUDIO_BUILD_PLAN.md`：Stage 15 当前落地状态改为已完成。
- `docs\MILUSTUDIO_PHASE_PLAN.md`：Stage 15 状态改为 done，当前焦点改为 Post Stage 15，由用户确认下一阶段。
- `docs\MILUSTUDIO_HANDOFF.md`：补 Stage 15 落地内容、验证结果、技术债和下一棒提示词。
- `docs\MILUSTUDIO_TASK_RECORD.md`：追加本记录。

Next phase:

- 由用户确认下一阶段正式编号与范围。
- 建议优先推进桌面 MVP 后账号注册、登录、设备绑定和许可证授权系统，正式授权由 Control API / Auth & Licensing adapter 统一处理。

### 2026-05-13 Stage 15 纰漏修复与文档同步

Stage:

- Post Stage 15 desktop hardening。

Action:

- 将 `apps\desktop` 升级到 `electron@42.0.1`，补 Electron 主进程安全边界：限制外部导航、禁止新窗口、校验 IPC sender origin，并把 `sessionData` 与 `userData` / logs 一起落到 D 盘数据目录。
- 桌面宿主启动 Control API / Worker 时改用 `Production` 环境语义，并为桌面 Web 注入 `window.__MILUSTUDIO_DESKTOP_TOKEN__`；Web Control API client 对 fetch 写请求带 `X-MiLuStudio-Desktop-Token`。
- Control API 桌面模式收紧 CORS 为 exact desktop origin，unsafe HTTP methods 要求桌面令牌；`/api/system/migrations/apply` 在桌面模式下返回 403，保持“桌面端不执行 migrations”的边界。
- `Prepare-MiLuStudioDesktopRuntime.ps1` 默认发布 self-contained win-x64 API / Worker，并复制 `python-runtime`；electron-builder extraResources 同步打包 `resources\python-runtime`。
- NSIS 自启动快捷方式补 AppUserModelID；未勾选开机自启动时删除旧 startup link。
- 新增 `scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1`，并接入 `Test-MiLuStudioDesktop.ps1`。
- 同步 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md` 和 `docs\MILUSTUDIO_HANDOFF.md`。

Commands:

```powershell
Push-Location D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 install
D:\soft\program\nodejs\npm.ps1 run build
D:\soft\program\nodejs\npm.ps1 run prepare:runtime
D:\soft\program\nodejs\npm.ps1 run smoke
D:\soft\program\nodejs\npm.ps1 run dist:win
Pop-Location

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln -c Release
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktop.ps1 -SkipInstall -SkipSmoke
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1 -SkipPrepareRuntime
& D:\code\MiLuStudio\outputs\desktop\win-unpacked\MiLuStudio.exe --smoke-test
Get-AuthenticodeSignature D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe
```

Verification result:

- Desktop TypeScript build：通过。
- Web build：通过。
- Control Plane Release build：0 warning / 0 error。
- `prepare:runtime`：生成 self-contained `MiLuStudio.Api.exe` / `MiLuStudio.Worker.exe`，并复制 `apps\desktop\runtime\python-runtime\python.exe`。
- Electron smoke：通过；health mode 为 `stage-15-desktop-packaging`，Control API / Worker / Web host 均 running，preflight healthy=true，Python runtime 指向随包路径。
- Desktop API security：通过；无令牌写请求 403，带令牌写请求进入业务校验，桌面模式 migration apply 403。
- `dist:win`：通过；生成 `D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe`，大小约 177 MB。
- packaged smoke：`D:\code\MiLuStudio\outputs\desktop\win-unpacked\MiLuStudio.exe --smoke-test` exit code 0。
- npm audit：desktop 0 vulnerabilities；desktop outdated 仅剩 `@types/node` 和 TypeScript major dev-only 更新。
- Authenticode：当前安装包为 `NotSigned`，正式发布前仍需代码签名证书和签名流水线。

Design check:

- cohesion：桌面宿主只处理窗口、安全边界、运行时进程和安装器；API 授权语义仍在 Control API 中实现。
- coupling：Web UI 只得到 API base URL 和短生命周期桌面令牌，不得到数据库、文件系统、Python 或 FFmpeg 能力。
- boundaries：桌面端继续不执行 migrations、不定义数据库表、不初始化数据库；API 桌面模式主动阻断 migration apply。
- temporary debt：安装包真实签名和干净 Windows 用户环境的安装 / 卸载 / 自启动 / 快捷方式人工验收仍未完成。

Environment check:

- self-contained .NET、Python runtime、Web dist、Python skills、安装包和日志仍在 `D:\code\MiLuStudio` 或输出目录下。
- Electron `userData`、`sessionData` 和 logs 已显式落到 D 盘数据路径。
- 未引入 Linux / Docker / Redis / Celery 生产依赖。
- 未接真实模型 provider，未读取真实媒体文件，未触发 FFmpeg，未生成真实 MP4 / WAV / SRT / ZIP。

Web self-check:

- Electron security 官方文档要求禁用 Node integration、启用 contextIsolation / sandbox，并限制导航、弹窗和暴露给 renderer 的能力；本次补丁已按这些方向收口：https://www.electronjs.org/docs/latest/tutorial/security/
- Electron release timelines 官方文档提示旧版本有支持周期，`npm outdated` 显示 39.x 已落后，已升级到当前 latest `42.0.1`：https://www.electronjs.org/docs/latest/tutorial/electron-timelines
- Electron app API 官方文档支持 `app.setPath('sessionData')` 与 `app.setPath('userData')`，本次把两者都显式指向 D 盘：https://www.electronjs.org/docs/latest/api/app/
- .NET 官方支持策略显示 .NET 8 是 LTS；桌面 runtime 仍基于 .NET 8，但已改为 self-contained 发布，降低干净机器外部依赖：https://dotnet.microsoft.com/en-us/platform/support/policy
- electron-builder NSIS 官方文档继续支持 assisted installer 与自定义 include；本次只补 NSIS 自启动快捷方式语义，不改变 Stage 15 安装器路线：https://www.electron.build/nsis.html

Doc changes:

- `README.md`：补桌面令牌、self-contained runtime、Python runtime、Electron 安全边界、API 安全测试和未签名安装包说明。
- `docs\MILUSTUDIO_BUILD_PLAN.md`：补 Stage 15 hardening 落地状态和签名技术债。
- `docs\MILUSTUDIO_PHASE_PLAN.md`：补 Stage 15 验收和落地状态。
- `docs\MILUSTUDIO_HANDOFF.md`：补下一棒需要立即接住的桌面 hardening 状态、验证结果和未签名风险。

Next phase:

- 继续建议进入桌面 MVP 后账号注册、登录、设备绑定和许可证授权系统；发布前还需要补正式代码签名与干净 Windows 安装验收。

### 2026-05-13 Stage 16 编号与范围确认

Stage:

- Stage 16：账号、会话、设备绑定和许可证授权。

Action:

- 读取 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md`、`docs\MILUSTUDIO_TASK_RECORD.md` 和 `docs\MILUSTUDIO_HANDOFF.md`，确认当前状态为 Stage 0-15 已完成，下一阶段待用户确认。
- 将下一阶段正式编号确认为 Stage 16，并将范围收敛为桌面 MVP 后账号注册、登录、设备绑定和许可证授权系统。
- 同步 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md` 和 `docs\MILUSTUDIO_HANDOFF.md`，补 Stage 16 的目标、边界、任务和完成标准。
- 明确账号、会话、设备绑定、许可证状态和授权错误统一由 Control API / Auth & Licensing adapter 处理。
- 明确 Electron 与 Web UI 只通过 Control API 展示登录 / 注册 / 激活入口和授权状态。
- 明确安装器激活码页只作为安装前门槛占位，不作为唯一授权边界。

Boundary:

- 不接真实模型。
- 不读取真实媒体文件。
- 不触发 FFmpeg。
- 不生成真实 MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。
- 不让 UI 或 Electron 绕过 Control API / Worker 边界。
- 不让 Electron 或桌面端执行 migrations、定义数据库表或负责数据库初始化。
- 不让 UI 或 Electron 直接访问 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg。

Verification result:

- 文档可通过 PowerShell `Get-Content -Encoding UTF8` 读取。
- 本次为阶段编号与范围确认，尚未开始 Stage 16 代码实现。
- 本次不触发构建、打包或数据库测试。

Doc changes:

- `README.md`：补 Stage 16 下一阶段说明和授权边界。
- `docs\MILUSTUDIO_BUILD_PLAN.md`：补阶段 16 总体计划、任务和验收标准。
- `docs\MILUSTUDIO_PHASE_PLAN.md`：将下一阶段从 Post Stage 15 确认为 Stage 16，并补完整 Stage 16 章节。
- `docs\MILUSTUDIO_HANDOFF.md`：把接棒状态更新为 Stage 16 pending implementation。
- `docs\MILUSTUDIO_TASK_RECORD.md`：记录本次编号和范围确认。

Next phase:

- 按 Stage 16 开始实现账号注册、登录、会话、设备绑定和许可证授权系统。
- 优先使用 deterministic Auth & Licensing adapter、测试激活码和本地 PostgreSQL 集成测试打通授权链路。

### 2026-05-13 Stage 16 完成

Stage:

- Stage 16：账号、会话、设备绑定和许可证授权。

Action:

- 新增账号、会话、设备绑定和许可证领域实体：`Account`、`AuthSession`、`DeviceBinding`、`LicenseGrant`。
- 新增 `IAuthRepository`、`IAuthTokenService`、`IPasswordHasher`、`IAuthLicensingAdapter` 和 `AuthLicensingService`。
- 新增 PostgreSQL migration：`backend\control-plane\db\migrations\004_stage16_auth_licensing.sql`。
- 新增 `PostgreSqlAuthRepository` 和 `InMemoryAuthRepository`，保持 PostgreSQL 默认 provider 语义。
- 新增 deterministic `DeterministicAuthLicensingAdapter`，默认测试激活码为 `MILU-STAGE16-TEST`。
- 新增 Control API auth endpoints：register、login、refresh、logout、me、license、activate、devices/bind。
- 对项目、生产任务和 generation task 类 API 增加最小授权门禁；未登录返回 401，未授权或设备超额返回 403。
- Web UI 新增 `AuthGate` 登录 / 注册 / 激活入口；未登录或未授权时不进入项目工作台。
- Web Control API client 增加 access / refresh token 管理、Bearer header 和 SSE token query。
- 更新 Stage 14 集成脚本，先注册 / 激活测试账号后再跑生产链路。
- 新增 Stage 16 PowerShell 集成测试脚本：`scripts\windows\Test-MiLuStudioStage16Auth.ps1`。
- 更新桌面 API 安全脚本，确认桌面令牌只进入应用授权门禁，不替代正式授权。

Commands:

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage16-build\

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage16Auth.ps1
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage14Integration.ps1
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1

Push-Location D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location
```

Verification result:

- .NET build：0 warning / 0 error。
- Web build：通过。
- Stage 16 auth integration：通过；覆盖注册、登录、会话刷新、设备绑定、设备上限阻断、许可证状态、未登录 401、未授权 403、无效激活码 403、登出失效和重新登录。
- Stage 14 integration：通过；最近完成 job `job_f7ef2ea3c6e143449ffdce01c8383fc5`，确认授权门禁不破坏 Worker / Python deterministic skills 链路。
- Desktop API security：通过；无桌面令牌写请求 403，带桌面令牌后进入应用授权门禁 401，桌面模式 migration apply 403。
- Desktop TypeScript build：通过。

Design check:

- cohesion：账号、会话、设备和许可证语义集中在 AuthLicensingService / auth repository / adapter，不混入 ProjectService、Worker 或 Electron。
- coupling：Web UI 只通过 Control API auth DTO 和 Bearer token 通信；Electron 只注入 Control API base URL 与桌面会话令牌。
- boundaries：PostgreSQL schema 由后端 migration 管理；Electron 不执行 migrations、不定义表、不读写数据库或授权文件。
- temporary debt：当前只有 deterministic 本地激活码，不含真实云端授权、离线签名许可证、套餐配额计费或 Authenticode 签名。

Environment check:

- project-local files：新增源码、migration、Web auth UI 和测试脚本均位于 `D:\code\MiLuStudio`。
- D drive only：构建输出、desktop runtime、Web dist 和测试临时目录仍在项目目录或 D 盘工具目录。
- C drive risk：未新增需要写入 C 盘的依赖或运行目录。

Web self-check:

- OWASP Authentication Cheat Sheet 建议结合安全会话管理、上下文感知和重新认证；Stage 16 已把账号、设备和会话统一放在后端授权服务边界：https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
- OWASP Session Management Cheat Sheet 强调会话标识需要过期、约束和服务端管理；Stage 16 使用服务端 session 表、过期时间、refresh token hash 和 logout revoke：https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html
- Microsoft ASP.NET Core 文档确认 API 资源常用 `Authorization: Bearer <token>` 访问；Stage 16 Web client 只把 Bearer token 发给 Control API：https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0
- Electron context isolation 文档建议 preload 只暴露有限 API；Stage 16 未扩大 Electron 能力，仍不暴露数据库、文件系统、Python 或 FFmpeg：https://www.electronjs.org/docs/latest/tutorial/context-isolation
- OWASP ASVS secret guidance 强调 secrets 不应进入源代码；Stage 16 只使用本地测试激活码，真实私钥 / 云端授权密钥仍未接入客户端：https://owasp-aasvs4.readthedocs.io/en/latest/requirement-2.29.html
- Keygen offline license docs 提到离线授权常用签名 license 数据；后续如做离线许可证，应走服务端签名 / 客户端公钥验证，而不是可逆激活码算法：https://keygen.sh/docs/choosing-a-licensing-model/offline-licenses/

Deviation reason:

- 无产品方向偏差。
- Stage 16 遵守边界：不接真实模型，不读取真实媒体，不调用 FFmpeg，不生成真实 MP4 / WAV / SRT / ZIP，不引入 Linux / Docker / Redis / Celery，不让 UI 或 Electron 绕过 Control API / Worker。

Doc changes:

- `README.md`：同步 Stage 16 完成状态、004 migration、账号授权能力和测试命令。
- `docs\MILUSTUDIO_BUILD_PLAN.md`：补 Stage 16 当前落地状态。
- `docs\MILUSTUDIO_PHASE_PLAN.md`：Stage 16 标记 done，当前焦点改为 Post Stage 16 pending user confirmation。
- `docs\MILUSTUDIO_HANDOFF.md`：补 Stage 16 落地内容、验证结果、技术债和下一棒提示词。
- `docs\MILUSTUDIO_TASK_RECORD.md`：追加本记录。

Next phase:

- 由用户确认 Stage 17 的正式编号与范围。
- 候选方向：真实 provider adapter 前的配置页 / 套餐限制 / 授权策略细化，或正式代码签名与干净 Windows 安装验收。
