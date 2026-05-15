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
- 同一项目点击重新生成时会淘汰旧未完成 job 并创建新 job，避免旧剧本输出继续串到当前输入。
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
- `apps\desktop\build\installer.nsh` 当前只保留桌面快捷方式、开始菜单快捷方式和开机自启动复选项；安装前激活码页已在后续 MVP 收敛中撤下。
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

### 2026-05-14 撤下当前许可证和激活码体验

Date:

- 2026-05-14

Trigger:

- 用户确认许可证、激活码和付费码属于后续盈利的大后期内容，当前 Web / Desktop 先取消相关内容。

Action:

- Web `AuthGate` 移除激活码输入、许可证激活页和注册时激活码字段；登录 / 注册成功后直接进入工作台。
- Web sidebar 账号区不再展示许可证 plan，改为展示账号邮箱或设备名。
- Control API 对项目、生产任务和 generation task 的门禁从“登录 + 有效许可证”降为“已登录”；未登录仍返回 401。
- Control API 当前不再映射 `/api/auth/license` 和 `/api/auth/activate`。
- 当前账号状态保留兼容 DTO 字段，但返回 `not_required`，表示当前 MVP 登录后即可使用。
- `apps\desktop\build\installer.nsh` 删除安装前激活码页和安装码 registry 占位，只保留桌面快捷方式、开始菜单快捷方式和开机自启动选项。
- `scripts\windows\Test-MiLuStudioStage16Auth.ps1` 改为账号 / 会话集成测试，不再覆盖激活码、许可证或设备付费上限。
- `scripts\windows\Test-MiLuStudioStage14Integration.ps1` 移除测试激活码 bootstrap。
- README、总参考、阶段计划和短棒交接同步当前 MVP 范围：商业授权、套餐、许可证、激活码和付费码后置。

Boundary:

- 不删除历史 migration 中的 `licenses` 表，避免破坏已应用数据库；当前 Web / Desktop 和受保护 API 不再依赖它。
- 不接真实云端授权服务，不实现离线签名许可证，不做套餐限制，不改 Electron / UI 边界。

Next:

- 重新运行 .NET / Web / Desktop 构建和 Stage 14 / Stage 16 / Desktop API 安全脚本。

Verification result:

- .NET build：0 warning / 0 error。
- Web build：通过。
- Desktop TypeScript build：通过。
- Stage 16 account/session integration：通过；最新测试账号 `stage16_3494570adbc6@example.local`。
- Stage 14 integration：通过；最新完成 job `job_b1b07370aea34c43a2f332c12fe08bbe`。
- Desktop API security：通过；无桌面令牌 403，带桌面令牌后进入应用登录门禁 401，桌面 migration apply 403。

### 2026-05-14 生产控制台中文化与 checkpoint 审核体验补丁

Date:

- 2026-05-14

Trigger:

- 用户指出新项目生产控制台仍暴露 `Style bible`、`story_intake`、`completed`、`local`、字段名等英文内部语义，影响中文用户理解。
- 用户指出 checkpoint 审核虽然有通过 / 拒绝按钮，但用户不清楚每一步在做什么，且点击通过或拒绝后中间进度和右侧结果区缺少明显状态变化。

Action:

- Web 生产控制台新增阶段展示映射，把 Control API / Worker 的内部 skill 名、英文 stage label、任务状态和字段名转成中文业务文案。
- 进度面板新增当前审核说明：本步产出、需要检查什么、通过后进入哪里、拒绝后如何处理。
- 拒绝 checkpoint 前要求填写修改意见，避免用户误点造成任务进入可重试失败状态。
- 同步消息对通过 / 退回使用成功或危险色，并把后端推送消息里的英文 stage / checkpoint 文案本地化。
- 阶段列表对待确认、已确认、已退回使用不同 chip 和背景状态。
- 右侧结果卡突出“当前审核”产物；用户可直接看到当前要审哪张卡，技术细节折叠到“技术详情”中，默认只展示中文摘要、状态、尝试次数、产出和本地测试成本。
- 前端类型补齐 `StageStatus.failed`，避免拒绝 checkpoint 后无法正确表示失败 / 可重试阶段。

Boundary:

- 未修改 Control API / Worker / Python skills 的生产边界。
- 未接真实模型、未读取真实媒体、未触发 FFmpeg、未生成真实 MP4 / WAV / SRT / ZIP。
- 未让 UI 或 Electron 直接访问数据库、业务文件系统、Python 脚本或 FFmpeg。

Verification result:

- Web build：通过，命令为 `npm run build`，工作目录 `D:\code\MiLuStudio\apps\web`。

### 2026-05-14 分镜 MD 格式对齐基础补丁

Date:

- 2026-05-14

Trigger:

- 用户上传 `C:\Users\10045\Desktop\整本拆分1.3.md`，要求分镜审核阶段尽量对齐其专业分镜稿格式。
- 经检查，当前 deterministic `storyboard_director` 只能稳定输出 30-60 秒、6-12 镜头 MVP 结构，不能直接严格实现 120 秒、50-55 镜头、逐句对白保真和 10-14 部分长分镜要求。

Action:

- `storyboard_director` 保留原 `shots` 字段，避免破坏 `image_prompt_builder`、`video_prompt_builder`、配音、字幕和粗剪等下游技能。
- 新增 `format_profile.name=cinematic_md_v1`，明确当前为本地确定性分镜稿结构。
- 新增 `film_overview`、`storyboard_parts`、`rendered_markdown` 和 `validation_report`。
- `storyboard_parts` 按 15 秒窗口组织分部，并为每个镜头生成中文字段：环境描写、时间切片与画面细分、镜头景别、镜头运动与衔接、音效和背景音乐。
- `schema.output.json` 和 `validators.py` 同步增加 MD 分镜结构契约校验。
- Web 生产控制台为 `storyboard_director` 增加专用预览，不再用通用 JSON 摘要显示分镜；审核区和右侧结果卡都展示影片概览、分部和镜头字段。
- README、总参考、阶段计划和短棒交接同步本次分镜格式基础能力和后续严格模式技术债。

Boundary:

- 当前仍不接真实模型，不读取真实媒体，不触发 FFmpeg，不生成真实 MP4 / WAV / SRT / ZIP。
- 当前只做格式、schema、validator 和 UI 预览对齐；不把 120 秒、50-55 镜头、逐句对白保真强行塞进现有 30-60 秒 MVP 生产链路。
- 严格 MD 模式需要后续 TextProvider / 模型 adapter、对白校验器、长文本分镜 validator、UI 分页/折叠和下游 50+ 镜头规模化处理。

Verification result:

- Python Stage 7 storyboard pipeline：通过，命令为 `C:\Users\10045\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe backend\sidecars\python-skills\tests\test_stage7_storyboard_pipeline.py`，并设置 `PYTHONPATH=D:\code\MiLuStudio\backend\sidecars\python-skills`。
- Python Stage 14 skill contract drift：通过，命令为 `C:\Users\10045\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe backend\sidecars\python-skills\tests\test_stage14_skill_contracts.py`。
- Python skills 全量 unittest：通过，30 tests OK，命令为 `C:\Users\10045\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe -m unittest discover -s backend\sidecars\python-skills\tests -v`。
- Web build：通过，命令为 `npm run build`，工作目录 `D:\code\MiLuStudio\apps\web`。

### 2026-05-14 牡丹亭角色抽取与 checkpoint 按钮语义补丁

Date:

- 2026-05-14

Trigger:

- 用户指出《牡丹亭》输入中主角应为杜丽娘，但当前角色设定阶段把“柳枝垂”识别成主角。
- 用户指出 checkpoint 审核态只有“通过”明显可点，暂停 / 恢复 / 拒绝 / 重试都灰掉，难以理解当前能做什么。

Action:

- 修正 `story_intake` 中文角色抽取：先识别明确人物句式，例如“杜丽娘是...小姐”、“侍女春香”、“自称柳梦梅”，再用动作句式兜底。
- 收紧自然物和动作片段过滤，避免把“柳枝垂”、“花影像”、“她倚”当成角色名。
- 为《牡丹亭》500 字 fixture 增加回归测试，断言主角为“杜丽娘”，角色包含“春香”和“柳梦梅”，且不包含误识别片段。
- Web checkpoint 审核态按钮文案调整为“已暂停 / 通过后继续 / 通过并继续 / 退回修改 / 退回后重试”，并增加操作说明。
- “退回修改”按钮不再因空备注直接灰掉；点击时会聚焦备注框并提示填写修改意见，填写后再提交退回。

Boundary:

- 当前仍为 deterministic 本地解析，不接真实模型，不读取真实媒体，不触发 FFmpeg，不生成真实 MP4 / WAV / SRT / ZIP。
- 未实现“备注驱动重新生成”或复杂输入版本 diff；角色源头错误修复后，需要点击“重新生成”创建新 job 才会刷新已有错误输出。
- UI 仍只通过 Control API 调 checkpoint / retry / regenerate，不直接访问 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg。

Verification result:

- Python `story_intake` 定向测试：通过，命令为 `D:\soft\program\Python\Python313\python.exe -m unittest tests.test_story_intake`。
- Python skills 全量 unittest：通过，30 tests OK，命令为 `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests`。
- 手动执行《牡丹亭》fixture：`main_characters` 输出 `杜丽娘 / 春香 / 柳梦梅`，不再输出 `柳枝垂 / 花影像 / 她倚`。
- Web build：通过，命令为 `npm run build`，工作目录 `D:\code\MiLuStudio\apps\web`。

### 2026-05-14 生产任务按当前输入重新生成补丁

Date:

- 2026-05-14

Trigger:

- 用户把项目输入换成《牡丹亭》测试文本后，右侧真实结果仍显示旧的“雨夜纸鹤 / 林溪 / 纸鹤”内容。
- 检查 PostgreSQL 后确认：项目 `story_inputs.original_text` 已是新输入，但生产控制台继续复用了旧 active job 的 `generation_tasks.output_json`。

Action:

- `ProductionJobService.StartAsync` 不再返回同项目已有 active job。
- 点击开始生成或重新生成时，先把同项目 `running / paused / queued` 旧 job 标记为 `failed`，写入“已根据当前输入重新生成，新任务已取代该旧任务。”，再创建新 job。
- Web 生产控制台启动新 job 前清空旧 job、阶段、结果卡、资产和成本状态，按钮文案改为“重新生成”。
- Stage 14 集成脚本从“重复启动应返回同一个 job”改为“重复启动应创建新 job，并确认旧 active job 被淘汰”。
- README、总参考、阶段计划和短棒交接同步当前重新生成语义。

Boundary:

- 不引入复杂输入版本号或 hash 机制；当前按用户确认的简化方案执行：每次重新生成都消费当前已保存输入。
- 不接真实模型，不读取真实媒体，不触发 FFmpeg，不生成真实 MP4 / WAV / SRT / ZIP。
- UI 仍只通过 Control API 启动生产任务和消费 task output，不直接访问 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg。

Verification result:

- .NET build：通过，命令为 `D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage16-build\`。
- Web build：通过，命令为 `npm run build`，工作目录 `D:\code\MiLuStudio\apps\web`。
- API 直连验证：用《牡丹亭》输入启动新 job 后，`story_intake.output_json` 包含“杜丽娘”等新输入内容，且不再包含旧的“雨夜纸鹤”文本。
- Stage 14 integration：通过，最新完成 job `job_5f2798c29f2043cb8b7aaf7b203d9806`。
- Control API health / preflight：通过；当前 provider 为 PostgreSQL，数据库、migration、storage、Python runtime 和 Python skills root 均 healthy。

### 2026-05-14 未开始状态去 mock 与中文剧本 fixture

Date:

- 2026-05-14

Trigger:

- 用户指出项目尚未开始时右侧已经出现脚本卡、角色卡、分镜卡等内容，会被误认为真实生成结果。
- 用户要求真实结果卡应在每一步结束后逐步添加，方便核实和确认。
- 用户确认下载一个中文公版剧本到项目内，方便后续放入输入框测试。

Action:

- Control API 正常连接时，生产控制台初始阶段改为流程预览，全部显示待开始 / 待生成，不再复用 `mockStages` 的已完成、待确认或生成中状态。
- Control API 正常连接且还没有 `task.outputJson` 时，右侧只显示中性占位，说明真实结果会在 Worker 写入后逐步出现。
- `mockResultCards` 只保留给 Control API 不可用的示例模式，不再混入正常 Control API 场景。
- 真实结果卡按 `GenerationTaskRecord.queueIndex` 顺序展示；每个 task 写入 output 后就会逐步追加到右侧结果区，当前审核卡仍通过视觉状态高亮。
- 下载 Project Gutenberg《牡丹亭》完整 UTF-8 文本到 `docs\test-fixtures\scripts\mudan_ting_gutenberg_23849.txt`。
- 新增 `docs\test-fixtures\scripts\mudan_ting_stage_input_zh.txt`，作为当前输入框可直接使用的 500 字中文测试样本。
- 新增 `docs\test-fixtures\scripts\README.md`，记录来源、URL、公版状态和 fixture 用途。

Boundary:

- 测试剧本只作为 fixture，不自动写入项目默认输入，不影响现有 PostgreSQL 数据。
- 当前仍不接真实模型，不读取真实媒体，不触发 FFmpeg，不生成真实 MP4 / WAV / SRT / ZIP。
- UI 仍只通过 Control API 消费真实 task output，不直接访问 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg。

Verification result:

- Web build：通过，命令为 `npm run build`，工作目录 `D:\code\MiLuStudio\apps\web`。
- diff check：通过，仅有 CRLF 提示。

### 2026-05-14 checkpoint 真实产物预览补丁

Date:

- 2026-05-14

Trigger:

- 用户以画风设定阶段举例指出：界面虽然提示“需要确认”，但没有把画风设定的实际内容展示出来，用户无法判断通过或拒绝的对象。

Action:

- 在当前 checkpoint 审核说明区增加真实产物预览，优先展示当前正在审核的 task output。
- 在右侧结果卡增加产物预览区，放在摘要和技术详情之间；技术字段仍折叠到“技术详情”。
- 为 `style_bible` 增加专门预览：画风名、视觉规则、色板、灯光、场景、镜头语言、角色一致性、负面提示词和后续可复用提示词块。
- 为其他 deterministic skills 增加通用结构化预览兜底，优先展示标题、摘要、一句话梗概、风险点、角色、分镜、字幕条目、交付资产和检查项。
- `style_bible` 摘要从剧集标题改为画风摘要，避免右侧卡片只显示“雨夜纸鹤”这类不具备审核价值的文本。

Boundary:

- 当前仍为 deterministic 本地产物预览，不接真实模型，不读取真实媒体，不触发 FFmpeg，不生成真实 MP4 / WAV / SRT / ZIP。
- UI 仍只消费 Control API 返回的 task output JSON，不直接访问 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg。

Verification result:

- Web build：通过，命令为 `npm run build`，工作目录 `D:\code\MiLuStudio\apps\web`。

### 2026-05-14 当前任务安排同步与短棒交接归档

Date:

- 2026-05-14

Trigger:

- 用户要求将当前任务安排同步到项目文档。
- 用户指出 `docs\MILUSTUDIO_HANDOFF.md` 前置任务记录过多，要求检查当前阶段是否需要；如不需要则归档，但保留所有约束。

Action:

- 将原 `docs\MILUSTUDIO_HANDOFF.md` 原样归档到 `docs\archive\MILUSTUDIO_HANDOFF_ARCHIVE_2026-05-14_before_trim.md`。
- 重写 `docs\MILUSTUDIO_HANDOFF.md`，只保留下一棒需要立即接住的内容：当前接棒、已完成摘要、最近补丁、固定约束、当前技术债、最近验证、下一步建议和下一棒提示词。
- 保留并集中整理所有硬约束：不接真实模型、不读真实媒体、不触发 FFmpeg、不生成真实媒体文件、不引入 Linux / Docker / Redis / Celery、不让 UI 或 Electron 绕过 Control API / Worker、不让桌面端执行 migrations 或拥有数据库 schema、D 盘环境约束、README 同步约束等。
- 同步 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md` 和 `docs\MILUSTUDIO_PHASE_PLAN.md`：当前 Post Stage 16 体验收敛已完成，Stage 17 仍待确认，候选方向为生产控制台可编辑能力、真实 provider adapter 前配置页、正式代码签名与干净 Windows 安装验收。

Boundary:

- 本次只整理文档，不改生产代码，不改数据库，不运行迁移，不接真实模型，不读取真实媒体，不触发 FFmpeg。
- 历史细节没有删除，已进入 archive；长期历史仍以 `docs\MILUSTUDIO_TASK_RECORD.md` 为事实来源。

Verification result:

- 已确认 `docs\archive\MILUSTUDIO_HANDOFF_ARCHIVE_2026-05-14_before_trim.md` 存在，归档原 handoff 349 行。
- 已确认新的 `docs\MILUSTUDIO_HANDOFF.md` 缩减为 111 行当前短交接，且保留固定约束。
- 已通过 `Get-Content -Encoding UTF8` 读取 README、总参考、阶段计划、任务记录和短棒交接。

### 2026-05-14 Stage 17 生产控制台可编辑能力完成

Date:

- 2026-05-14

Stage:

- Stage 17：生产控制台可编辑能力。

Trigger:

- 用户要求先做候选 1，并在开始前确认 Stage 17 的正式编号与范围。
- 正式范围收敛为：分镜表编辑、单镜头重新生成、备注驱动重试和审核后重算。

Action:

- 将 Stage 17 正式确认为“生产控制台可编辑能力”。
- 新增 `StoryboardEditingService` 和 DTO，集中处理 `storyboard_director` envelope 的分镜编辑、单镜头备注重算、元数据更新和下游任务 reset。
- 新增 Control API：`PATCH /api/generation-tasks/{taskId}/storyboard`。
- 新增 Control API：`POST /api/generation-tasks/{taskId}/storyboard/shots/{shotId}/regenerate`。
- Web Control API client 和 production types 已补分镜编辑 / 单镜头重算 DTO。
- 生产控制台真实分镜结果卡已加入编辑表单：镜头时长、场景、画面动作、景别、镜头运动、声音、对白和旁白。
- 保存分镜或单镜头重算后，job 回到分镜审核暂停态，storyboard task 回到 review，下游 task 清回 waiting 并清空 output。
- 新增 `scripts\windows\Test-MiLuStudioStage17StoryboardEditing.ps1`，覆盖完整 deterministic job、编辑、下游重置、单镜头重算和 no-provider/no-media 元数据。

Boundary:

- 不接真实模型 provider。
- 不读取真实媒体文件。
- 不触发 FFmpeg。
- 不生成真实 MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery。
- UI 与 Electron 仍只能走 Control API，不直接访问 PostgreSQL、业务文件系统、Python 脚本、模型 SDK 或 FFmpeg。
- 桌面端不执行 migrations、不定义数据库表、不负责数据库初始化。
- 本阶段未新增数据库 migration，复用 `generation_tasks.output_json` 和 `checkpoint_notes`。

Commands:

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
dotnet build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore

Push-Location D:\code\MiLuStudio\apps\web
npm run build
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage17StoryboardEditing.ps1
```

Verification result:

- .NET build：通过，0 warning / 0 error。
- Web build：通过，Vite 成功输出生产包。
- Stage 17 storyboard editing integration：通过；最新完成 job `job_e1ab06449be24902a372403244ed911f`。
- 集成脚本确认已完成 job 的分镜可编辑、下游任务会被重置、单镜头重算会消费当前备注，并写入 `model_provider=none` / `media_generated=false`。

Doc changes:

- `README.md`：同步 Stage 17 完成状态、能力范围、边界和验证命令。
- `docs\MILUSTUDIO_BUILD_PLAN.md`：将 Post Stage 16 当前安排更新为 Stage 17 落地状态，并记录新增 endpoint / 脚本。
- `docs\MILUSTUDIO_PHASE_PLAN.md`：当前焦点改为 Post Stage 17，新增完整 Stage 17 章节。
- `docs\MILUSTUDIO_HANDOFF.md`：更新短交接、技术债、最近验证、下一步建议和下一棒提示词。
- `docs\MILUSTUDIO_TASK_RECORD.md`：追加本记录。

Next phase:

- Stage 18 尚未正式确认。
- 候选方向：真实 provider adapter 前配置页、正式代码签名与干净 Windows 安装验收，或继续扩展生产控制台角色 / 画风 / 提示词编辑能力。

### 2026-05-14 Stage 18 真实 provider adapter 前配置页完成

Date:

- 2026-05-14

Stage:

- Stage 18：真实 provider adapter 前配置页。

Trigger:

- 用户确认按推荐路径继续。
- Stage 18 正式范围收敛为：先落地真实 provider adapter 前配置页、配置 DTO、本地 preflight 和成本边界；仍不接真实模型、不读取真实媒体、不触发 FFmpeg、不生成真实媒体文件。

Action:

- 新增 `ProviderSettingsService`、provider settings DTO 和 `IProviderSettingsRepository`。
- 新增 `FileProviderSettingsRepository`，默认通过 `ControlPlane:StorageRoot` 下的本地 JSON 保存 provider 前占位配置。
- 新增 `GET /api/settings/providers`、`PATCH /api/settings/providers` 和 `GET /api/settings/providers/preflight`。
- `/api/settings` 已纳入登录门禁；桌面 unsafe 写请求仍受桌面 session token 保护。
- Web 左侧“模型”导航已接入真实 `ProviderSettingsPage`，不再停留在占位页。
- Web provider 配置页支持 Text / Image / Video / Audio / Edit 五类 adapter 的启用、供应商选择、默认模型、API Key 占位输入与清除、单项目成本上限和失败重试次数。
- API Key 明文只进入请求体，服务端只保存遮罩和 SHA256 指纹；API 响应与本地 provider settings 文件不返回、不保存可用于真实调用的明文 key。
- Provider preflight 明确 `adapterMode=placeholder_only`、`externalNetwork=disabled`、`mediaGenerated=false`。
- 新增 `scripts\windows\Test-MiLuStudioStage18ProviderSettings.ps1`，覆盖配置保存、明文 key 不泄漏、preflight 占位边界和 clear key。
- README、总参考、阶段计划和短棒交接同步 Stage 18 完成状态。

Boundary:

- 不接真实模型 provider。
- 不读取真实媒体文件。
- 不触发 FFmpeg。
- 不生成真实 PNG / MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery。
- UI 与 Electron 仍只能走 Control API，不直接访问 PostgreSQL、业务文件系统、Python 脚本、模型 SDK 或 FFmpeg。
- 桌面端不执行 migrations、不定义数据库表、不负责数据库初始化。
- 本阶段未新增数据库 migration；provider 前配置由 Control API / Infrastructure 写入 D 盘本地 JSON。
- 本阶段未实现真实 secure secret store；当前只保存 key 遮罩与指纹，不能用于真实 provider 调用。

Local verification:

```powershell
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage18-build\

Push-Location D:\code\MiLuStudio\apps\web
npm run build
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage18ProviderSettings.ps1
```

Verification result:

- .NET build：通过，0 warning / 0 error。
- Web build：通过，Vite 成功输出生产包。
- Stage 18 provider settings integration：通过；脚本确认 API 响应和本地 provider settings 文件不包含明文测试 key。
- Stage 18 脚本确认启用且配置完整的 text adapter preflight 为 `ok`，且 `externalNetwork=disabled`、`mediaGenerated=false`。

Design check:

- cohesion：provider settings DTO、service、repository 和 Web 页面均围绕“provider 前配置”单一能力组织。
- coupling：Web 只依赖 Control API DTO；provider 配置写入隐藏在 Application service 和 Infrastructure repository 后。
- boundaries：未让 UI / Electron 直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg；未让 Worker 或 Skills 读取真实媒体。
- temporary debt：API Key 当前只保存遮罩和指纹，不能作为真实 adapter secret store；后续如接真实 provider，需要先补 secure secret store、费用硬阈值、provider sandbox 和真实 key 校验边界。

Environment check:

- project-local files：新增脚本、前端页面、后端 service/repository 和文档均在 `D:\code\MiLuStudio` 内。
- D drive only：验收脚本使用 `.tmp\stage18-provider-settings\...` 临时目录和 D 盘本地 API build 输出。
- C drive risk：未新增 C 盘依赖、缓存、模型、媒体或外部服务写入。

Web searches:

- OpenAI API key safety：官方文档要求不要把 API key 暴露在浏览器 / 客户端代码中，建议通过后端和环境变量或 secret management 管理。
- OpenAI production best practices：官方文档强调 API key 应安全保存，并使用 spend limits / usage limits 监控成本。
- Electron security checklist：官方文档强调限制导航、不要向不可信 Web 暴露 Electron API、校验 IPC sender；当前 Stage 18 继续保持 Web 只走 Control API。

Sources:

- https://help.openai.com/en/articles/5112595-best-practices-for-api
- https://platform.openai.com/docs/guides/production-best-practices/model-overview
- https://www.electronjs.org/docs/latest/tutorial/security

Findings:

- Stage 18 的“响应和本地文件不保存明文 key”与官方 key safety 方向一致；真实 provider 接入前仍需要正式 secret store。
- 成本上限和重试次数作为配置 DTO 先落地是合理的，但真实 provider 调用前还需要硬执行 spend guard，而不是只在 UI 展示。
- Electron / Web 继续不直接保存 secret 或调用 provider SDK，符合当前桌面安全边界。

Deviation risk:

- 无方向偏差；本阶段仍是 adapter 前配置，不是 provider 接入。
- 当前 key 只保存遮罩/指纹，后续不能误认为已有可用 provider secret。

README check:

- 已更新根 README，加入 Stage 18 完成状态、Provider 前配置章节、当前边界和下一阶段建议。

Build plan changes:

- 已更新 Stage 18 当前落地状态和 13.2 第一版配置页说明。

Phase plan changes:

- 当前焦点改为 Post Stage 18，新增 Stage 18 完整章节。

Task record changes:

- 已追加本记录。

Handoff changes:

- 已更新短棒交接，下一步改为确认 Stage 19 正式编号与范围。

Next phase:

- Stage 19 尚未正式确认。
- 候选方向：正式代码签名与干净 Windows 安装验收、继续扩展生产控制台角色 / 画风 / 提示词编辑能力，或 provider adapter 后续 secure secret store / spend guard / sandbox 设计。

### 2026-05-14 Stage 19 桌面发布验收与代码签名前置准备完成

Date:

- 2026-05-14

Stage:

- Stage 19：桌面发布验收与代码签名前置准备。

Trigger:

- 用户要求“按照建议方向继续”。
- Stage 19 正式范围收敛为：先补桌面发布验收、签名前置检查和干净 Windows 安装 / 卸载 / 自启动 / 快捷方式清单；仍不接真实 provider、不读取真实媒体、不触发 FFmpeg、不生成真实媒体文件。

Action:

- 新增 `scripts\windows\Test-MiLuStudioStage19DesktopRelease.ps1`，覆盖 desktop package 配置、安装器产物、`win-unpacked`、Web dist、Control API / Worker runtime、Python runtime、Python Skills、最新 migration 和 Electron 安全边界。
- 脚本检查 `installer.nsh` 的快捷方式 / 开机自启动选项，并确认安装器脚本中不恢复许可证、激活码或付费码门槛。
- 脚本检查 Authenticode 状态：默认把 `NotSigned` 记录为本地验收警告；`-RequireSigned` 模式要求安装器和主 exe 均为 `Valid`。
- 脚本检查签名前置配置：支持证书路径 / thumbprint / timestamp URL 检查，证书路径不得位于仓库内。
- 脚本继续调用桌面 API 安全验证，确认桌面模式下 unsafe 写请求、登录门禁和 migration apply 禁止边界没有回退。
- `apps\desktop\package.json` 新增 `verify:release` 和 `verify:release:signed`。
- `.gitignore` 新增 `*.pfx`、`*.p12`、`*.pvk`、`*.spc`、`*.key`，避免证书或私钥容器入仓。
- 新增 `docs\MILUSTUDIO_STAGE19_DESKTOP_RELEASE_CHECKLIST.md`，记录签名前置配置和干净 Windows 安装 / 卸载 / 自启动 / 快捷方式手工验收步骤。
- README、总参考、阶段计划和短棒交接同步 Stage 19 完成状态。

Boundary:

- 不接真实模型 provider。
- 不读取真实媒体文件。
- 不触发 FFmpeg。
- 不生成真实 PNG / MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery。
- UI 与 Electron 仍只能走 Control API，不直接访问 PostgreSQL、业务文件系统、Python 脚本、模型 SDK 或 FFmpeg。
- 桌面端不执行 migrations、不定义数据库表、不负责数据库初始化。
- 本阶段不提交代码签名证书、私钥或真实签名密钥；当前只做签名前置检查。

Local verification:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage19DesktopRelease.ps1 -BuildPackage

Push-Location D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run verify:release
Pop-Location

powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage19DesktopRelease.ps1 -SkipDesktopBuild -SkipApiSecurity -RequireSigned
```

Verification result:

- `-BuildPackage`：通过；重新生成 `outputs\desktop\MiLuStudio-Setup-0.1.0.exe` 和 `win-unpacked`，并确认已包含 `004_stage16_auth_licensing.sql`。
- `verify:release`：通过；脚本确认安装器、blockmap、主 exe、`app.asar`、Web dist、Control API / Worker runtime、Python runtime、Python Skills 和 storyboard director skill 均在发布产物内。
- 桌面 API 安全验证：通过；无桌面 token 的 unsafe 写请求为 403，带 token 后进入账号登录门禁，桌面模式 migration apply 为 403。
- Authenticode：安装器和 `win-unpacked\MiLuStudio.exe` 当前均为 `NotSigned`；本地验收记录为警告，正式发布前阻塞。
- `-RequireSigned` 负向检查：按预期失败，确认当前未签名产物不会被误判为正式可发布。
- Stage 19 重打包前的旧 `outputs\desktop` 产物缺少 `004_stage16_auth_licensing.sql`，已通过本阶段重打包修正。
- `git diff --check`：通过；仅出现工作区 LF/CRLF 提示，无空白错误。

Design check:

- cohesion：新增脚本和清单只围绕“桌面发布验收与签名前置准备”组织，不混入 provider、媒体生成或业务编辑能力。
- coupling：脚本读取 package、源文件和发布产物做验收；业务边界仍由 Control API / Application / Worker 负责。
- boundaries：Electron 继续只承载 Web、启动本地服务和注入 Control API 配置；未让 UI / Electron 直接接触数据库、Python、模型 SDK 或 FFmpeg。
- temporary debt：当前未接入真实签名证书、Azure Trusted Signing 或硬件 EV 证书；正式发布前必须补真实证书和签名流水线，并在干净 Windows 环境回归。

Environment check:

- project-local files：新增脚本、清单、package 配置和文档均在 `D:\code\MiLuStudio` 内。
- D drive only：打包输出仍位于 `D:\code\MiLuStudio\outputs\desktop`；报告输出位于 `D:\code\MiLuStudio\.tmp\stage19-desktop-release-report.json`。
- C drive risk：未新增 C 盘依赖、缓存、模型、媒体或外部服务写入；脚本拒绝使用仓库内证书路径。

Web searches:

- Microsoft SignTool：官方文档说明签名和时间戳需要明确 digest algorithm，推荐 SHA256，并提供 `/tr`、`/td`、`/pa` 等签名 / 验证选项。
- Electron code signing：官方文档强调打包分发的 Electron 应用应进行代码签名；Windows 分发涉及 EV / 云签名等正式证书约束。
- electron-builder Windows signing：官方文档说明 Windows code signing 在配置正确时可由 electron-builder 自动执行，并支持 Azure Trusted Signing 配置。
- Electron security checklist：官方文档强调不要启用 Node integration、启用 context isolation、启用 renderer sandbox、不要关闭 webSecurity。

Sources:

- https://learn.microsoft.com/en-us/windows/win32/seccrypto/signtool
- https://www.electronjs.org/docs/latest/tutorial/code-signing
- https://www.electron.build/code-signing-win.html
- https://www.electronjs.org/docs/latest/tutorial/security

Findings:

- Stage 19 的 `NotSigned` 记录和 `-RequireSigned` 阻断符合 Windows / Electron 桌面发布方向：本地包可验收，但正式分发必须真实签名。
- timestamp URL 作为正式签名前置项是必要的；后续签名流水线应使用 SHA256 digest 与可信时间戳。
- 当前 Electron 主进程的 context isolation、sandbox、Node integration 禁用、webSecurity、导航限制和 IPC sender guard 与官方安全清单方向一致。
- electron-builder 的正式签名配置仍留给后续证书落地；本阶段不伪造证书、不把密钥放入仓库。

Deviation risk:

- 无方向偏差；Stage 19 只做桌面发布验收和签名前置准备，不接真实 provider 或真实媒体链路。
- 正式签名仍是阻塞项，不能把当前 `NotSigned` 本地验收通过误读为可公开发布。

README check:

- 已更新根 README，加入 Stage 19 完成状态、桌面发布验收章节、当前边界、运行命令和下一阶段建议。

Build plan changes:

- 已更新 Stage 15 桌面打包状态和 Stage 19 当前落地状态。

Phase plan changes:

- 当前焦点改为 Post Stage 19，新增完整 Stage 19 章节，下一步改为确认 Stage 20。

Task record changes:

- 已追加本记录。

Handoff changes:

- 已更新短棒交接，下一棒提示词改为 Stage 0 到 Stage 19 已完成，并提示 Stage 20 候选方向。

Next phase:

- Stage 20 尚未正式确认。
- 推荐方向：继续扩展生产控制台角色 / 画风 / 图片提示词 / 视频提示词编辑，或先做 provider adapter 后续 secure secret store / spend guard / sandbox 设计；拿到正式证书后再做 signed release 回归。

### 2026-05-14 Stage 19 联网自检补强

Date:

- 2026-05-14

Trigger:

- 用户要求在进入新阶段前进行联网自检项目，如发现需要修改或完善则优先补齐。

Self-check scope:

- Microsoft SignTool / Authenticode 签名与时间戳。
- Electron code signing 与 Windows 发布签名要求。
- electron-builder Windows signing / Azure Trusted Signing 配置方向。
- Electron security checklist、context isolation、renderer sandbox 和 IPC sender 校验。

Findings:

- Stage 19 已覆盖 Authenticode `NotSigned` / `-RequireSigned`、证书不入仓、桌面 token、migration apply 禁止、contextIsolation、sandbox、nodeIntegration=false、webSecurity、导航限制和 IPC sender guard。
- 官方 Electron security checklist 还强调内容来源限制；当前桌面本地 Web host 已有基础 CSP 与 `X-Content-Type-Options: nosniff`，但 Stage 19 脚本尚未检查，且 CSP 可补 `form-action` 和 `frame-ancestors`。

Action:

- 更新 `apps\desktop\src\webHost.ts`：CSP 增加 `form-action 'self'` 和 `frame-ancestors 'none'`。
- 更新 `scripts\windows\Test-MiLuStudioStage19DesktopRelease.ps1`：把 CSP 与 `X-Content-Type-Options: nosniff` 纳入发布验收，检查 `default-src`、`script-src`、loopback `connect-src`、`object-src`、`base-uri`、`form-action` 和 `frame-ancestors`。
- 同步 README、总构建计划、阶段计划、短棒交接和 Stage 19 桌面发布验收清单。

Sources:

- https://learn.microsoft.com/en-us/windows/win32/seccrypto/signtool
- https://www.electronjs.org/docs/latest/tutorial/code-signing
- https://www.electron.build/code-signing-win.html
- https://www.electronjs.org/docs/latest/tutorial/security
- https://www.electronjs.org/docs/latest/tutorial/context-isolation
- https://www.electronjs.org/docs/latest/tutorial/sandbox

Verification:

- `Test-MiLuStudioStage19DesktopRelease.ps1 -BuildPackage`：通过；重新打包后 Stage 19 脚本已确认 CSP / nosniff 检查通过，并继续确认桌面 API 安全边界。
- `Test-MiLuStudioStage19DesktopRelease.ps1 -SkipDesktopBuild -SkipApiSecurity -RequireSigned`：按预期失败，继续阻断当前 `NotSigned` 安装器。

Next check:

- 无需在 Stage 20 前继续补 Stage 19；下一阶段可按用户要求进入前端重构规划。

### 2026-05-14 Stage 20 Codex 式前端工作台重构完成

Date:

- 2026-05-14

Stage:

- Stage 20：Codex 式前端工作台重构。

Trigger:

- 用户要求把整个前端改成 Codex 风格：左侧历史项目，新项目默认只有一个对话框，支持上传剧本或直接输入要求；进行中时右侧显示当前进度、生成结果预览和打开入口；现有导航与账户相关入口统一收束到左下角设置。

Action:

- 新增 `apps\web\src\features\workspace\StudioWorkspacePage.tsx`，作为登录后的主工作台入口。
- `apps\web\src\app\App.tsx` 改为只负责认证态检查和工作台挂载，不再渲染旧多导航壳。
- `apps\web\src\styles.css` 重写为三栏工作台样式：历史项目侧栏、中央单输入框、右侧进度 / 结果卡、设置弹层。
- 新项目空态不显示顶部页面标题，只保留中央输入框；输入框支持粘贴剧本 / 要求和上传 txt / md 文本。
- 启动生产前仍通过 `createProject` / `updateProject` 保存项目，再调用 `startProductionJob`；SSE 仍通过 `watchProductionJob` 更新进度。
- 右侧结果卡只展示真实 generation task output；“打开”结果会显示结构化预览，`storyboard_director` 结果保留分镜表编辑和单镜头备注重算入口。
- 模型配置、桌面诊断和账户退出统一放入左下角设置菜单；ProviderSettingsPage 和 DesktopDiagnosticsPanel 仍只通过 Control API。
- `AuthGate` 清理为正常中文文案。

Boundary:

- 未接真实模型 provider。
- 未读取真实媒体文件。
- 未触发 FFmpeg。
- 未生成真实 MP4 / WAV / SRT / ZIP。
- 未引入 Linux / Docker / Redis / Celery。
- UI / Electron 仍只通过 Control API / DTO / SSE 与业务系统通信，不直接访问数据库、业务文件系统、模型 SDK、Python 脚本或 FFmpeg。
- 桌面端仍不执行 migrations、不定义数据库表、不负责数据库初始化。
- 没有删除或修改 `D:\code\XiaoLouAI`。

Local verification:

```powershell
Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

git diff --check

Invoke-WebRequest -Uri http://127.0.0.1:5174/ -UseBasicParsing
```

Verification result:

- Web build：通过。
- `git diff --check`：通过；仅有工作区 LF/CRLF 提示，无空白错误。
- 本地 Vite 预览服务已启动在 `http://127.0.0.1:5174/`，HTTP 200。

Design check:

- cohesion：新增 `workspace` feature 聚合 Codex 式工作台交互，旧 `production-console` 代码保留但不作为主入口。
- coupling：工作台只调用既有 Control API client；分镜保存和单镜头重算仍走 Stage 17 endpoint。
- boundaries：设置入口只是 UI 收束，不让 Web 或 Electron 直接读写 provider settings 文件或访问底层服务。
- temporary debt：角色 / 画风 / 提示词等更细编辑能力尚未迁入新工作台；下一阶段可继续扩展结果打开面板。

Web searches:

- OpenAI Codex 官方页与文档确认 Codex 是面向真实工程任务的对话式 / 工作区式 coding agent；本阶段只参考交互方向，不下载或引入外部参考项目。

Sources:

- https://openai.com/codex
- https://platform.openai.com/docs/codex/overview

README check:

- 已更新根 `README.md`，加入 Stage 20 当前状态、Web 工作台章节、当前边界和下一阶段建议。

Phase plan changes:

- 已补 Stage 20 章节，下一阶段改为 Stage 21 候选方向。

Task record changes:

- 已追加本记录。

Handoff changes:

- 已更新短棒交接，下一棒提示词改为 Stage 0 到 Stage 20 已完成。

Next phase:

- Stage 21 尚未正式确认。
- 建议候选方向：继续细化新工作台中的角色 / 画风 / 图片提示词 / 视频提示词编辑能力；或做 provider adapter 后续 secure secret store / spend guard / sandbox；或拿到正式证书后做 signed release 回归。

### 2026-05-14 Stage 20 工作台附件阶段门禁与短棒归档

Date:

- 2026-05-14

Stage:

- Post Stage 20 frontend hardening：Codex 式附件菜单、阶段门禁和短棒 handoff 归档。

Trigger:

- 用户确认上传入口应改为 Codex 同款加号，并希望根据当前生产阶段限制文本 / 图片 / 视频上传入口；同时要求把短棒 handoff 重构清楚，将目前已完成阶段归档。

Action:

- `apps\web\src\features\workspace\StudioWorkspacePage.tsx`：把左下角上传按钮改为加号菜单，提供“文本 / 图片 / 视频”三类入口。
- 根据当前 `ProductionJob.currentStage` 和 job / project 状态计算上传权限：
  - 新项目、故事解析、短剧改编、脚本生成、图片提示词、配音任务、字幕结构：只允许文本。
  - 角色设定、画风设定、图片资产：允许文本和图片。
  - 分镜审核、视频提示词、视频片段、粗剪计划、质量检查：允许文本、图片和视频。
  - 排队中、导出占位、已完成和失败状态：禁用新附件。
- 禁用的菜单项不可点击，并通过 title 与菜单内说明解释当前阶段限制。
- 文件 input 的 `accept` 随用户选择的类型动态切换；选择后仍做二次类型校验。
- 文本附件仅在故事来源阶段作为剧本文本读取；后续阶段的文本、图片、视频只作为参考附件元数据。
- 图片 / 视频不读取真实媒体内容、不解析帧、不触发 FFmpeg、不扩后端真实媒体上传链路。
- `apps\web\src\styles.css`：新增 Codex 式加号菜单、上传项和禁用态样式。
- 新增 `docs\archive\MILUSTUDIO_COMPLETED_STAGES_ARCHIVE_2026-05-14_stage0-20.md`，归档 Stage 0-20 完成摘要。
- 重写 `docs\MILUSTUDIO_HANDOFF.md` 为更短的短棒交接，只保留当前状态、最近补丁、固定约束、技术债、验证和下一棒提示。

Boundary:

- 未接真实模型 provider。
- 未读取真实媒体文件。
- 未触发 FFmpeg。
- 未生成真实 MP4 / WAV / SRT / ZIP。
- 未引入 Linux / Docker / Redis / Celery。
- UI / Electron 仍只通过 Control API / DTO / SSE 与业务系统通信。
- 未让前端或 Electron 直接访问数据库、业务文件系统、模型 SDK、Python 脚本或 FFmpeg。

Local verification:

```powershell
Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location
```

Verification result:

- Web build：通过。

Design check:

- cohesion：上传菜单规则聚合在 workspace 组件内，与当前固定生产流程一致。
- coupling：只使用前端已有 job / project DTO 判断阶段，不新增后端上传链路。
- boundaries：图片 / 视频仍只是附件元数据，不进入真实媒体读取或存储。
- accessibility：禁用项不可点击，并保留 title / 小字说明，解释为什么当前阶段不能上传该类型。

README check:

- 已更新根 `README.md`，说明 Stage 20 工作台包含阶段门禁加号上传菜单，媒体附件只记录元数据。

Phase plan changes:

- 已更新 Stage 20 任务、验收、落地状态和阶段结束自检，补入加号上传菜单和阶段门禁。

Task record changes:

- 已追加本记录。

Handoff changes:

- 已将 Stage 0-20 完成摘要归档到 `docs\archive\MILUSTUDIO_COMPLETED_STAGES_ARCHIVE_2026-05-14_stage0-20.md`。
- 已重写短棒 handoff，下一棒提示词加入阶段门禁上传规则。

Next phase:

- Stage 21 尚未正式确认。
- 建议候选方向保持：继续细化新工作台角色 / 画风 / 图片提示词 / 视频提示词编辑能力；或做 provider adapter 后续 secure secret store / spend guard / sandbox；或拿到正式证书后做 signed release 回归。

### 2026-05-14 Stage 21 新工作台结构化产物编辑增强完成

Date:

- 2026-05-14

Stage:

- Stage 21：新工作台结构化产物编辑增强。

Trigger:

- 用户要求“按照建议的方向继续”。
- 正式范围确认为：继续细化新 Codex 式工作台中的角色、画风、图片提示词和视频提示词编辑能力；provider secure secret store / spend guard / sandbox 与 signed release 回归留作后续候选。

Action:

- 新增 `StructuredOutputEditingDtos` 与 `StructuredOutputEditingService`，以 Application service 管理结构化产物编辑、字段白名单、envelope 写回和下游任务重置。
- 新增 `PATCH /api/generation-tasks/{taskId}/structured-output`，支持编辑 `character_bible`、`style_bible`、`image_prompt_builder` 和 `video_prompt_builder` 的顶层白名单字段。
- 保存时只修改当前 task 的 JSON envelope，追加 `stage21_edit_summary`、review metadata 和 no-provider / no-media / no-FFmpeg 标记。
- 角色与画风编辑后回到 `review` / `paused`；图片提示词和视频提示词编辑后保持 `completed` / `running`，等待下游重新计算。
- 保存后重置当前任务之后的所有下游任务为 `waiting`，清空旧 `outputJson`、时间、锁和错误状态。
- Web 工作台结果面板新增角色、画风、图片提示词和视频提示词编辑表单、字段级 diff 和保存动作。
- 新增 `scripts\windows\Test-MiLuStudioStage21StructuredOutputEditing.ps1`，覆盖完整 deterministic 生产链路、四类结构化产物编辑、下游重置和边界标记。
- README、建设方案、阶段计划和短棒交接已同步 Stage 21 状态。

Boundary:

- 未接真实模型 provider。
- 未读取真实媒体文件。
- 未触发 FFmpeg。
- 未生成真实 PNG / MP4 / WAV / SRT / ZIP。
- 未引入 Linux / Docker / Redis / Celery。
- UI / Electron 仍只通过 Control API / DTO / SSE 与业务系统通信。
- 未让 Web 或 Electron 直接访问 PostgreSQL、业务文件系统、Python 脚本、模型 SDK 或 FFmpeg。
- 桌面端仍不执行 migrations、不定义数据库表、不负责数据库初始化。
- 本阶段未新增数据库 migration，未扩展真实媒体上传链路。

Local verification:

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage21-build\

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage21StructuredOutputEditing.ps1

git diff --check
```

Verification result:

- .NET build：通过。
- Web build：通过。
- Stage 21 structured output editing integration：通过；脚本完成生产流后依次编辑视频提示词、图片提示词、画风和角色，并验证下游重置。
- `git diff --check`：通过，仅有工作区 LF / CRLF 提示，无空白错误。

Design check:

- cohesion：后端编辑能力聚合在 `StructuredOutputEditingService`，前端编辑 UI 聚合在工作台结果面板内，围绕“已生成结构化产物编辑”单一职责组织。
- coupling：Web 只调用 Control API client；后端只写 task envelope 和任务状态，不接 provider SDK、Python skill 或 FFmpeg。
- boundaries：媒体附件仍只保留元数据；结构化编辑不读取真实媒体、不生成真实文件、不绕过 Worker / Control API 边界。
- temporary debt：当前只支持白名单顶层字段编辑；提示词批量操作、镜头增删、更细粒度 diff 和高级重算策略留给后续阶段。

Environment check:

- project-local files：新增代码、脚本和文档均位于 `D:\code\MiLuStudio`。
- D drive only：build 输出使用 `D:\code\MiLuStudio\.tmp\stage21-build` 和 `.tmp\stage21-integration-build`。
- C drive risk：未新增 C 盘依赖、缓存、模型、媒体或外部服务写入。

Web searches:

- MDN File API：确认 Web `File` 对象可访问 name / size / type / lastModified 等元数据；真实内容读取需要 `FileReader` 等额外动作。Stage 21 继续只记录媒体附件元数据，不读取真实媒体内容。
- Electron security：继续保持 context isolation、sandbox、nodeIntegration=false、webSecurity 和 IPC 边界方向；Stage 21 未让 Electron 暴露数据库、Python、模型 SDK 或 FFmpeg。
- OpenAI Codex 官方页面 / 文档：Codex 式工作区是工程任务交互参考；本阶段只借鉴工作台组织方式，不接 OpenAI Codex、外部 agent、真实 provider 或远端 sandbox。

Sources:

- https://developer.mozilla.org/en-US/docs/Web/API/File_API
- https://www.electronjs.org/docs/latest/tutorial/security
- https://www.electronjs.org/docs/latest/tutorial/context-isolation
- https://openai.com/codex
- https://platform.openai.com/docs/codex/overview

README check:

- 已更新根 `README.md`，加入 Stage 21 完成状态、工作台结构化产物编辑说明、当前边界和下一阶段建议。

Build plan changes:

- 已更新 `docs\MILUSTUDIO_BUILD_PLAN.md`，补入 Stage 20 / Stage 21 当前落地状态。

Phase plan changes:

- 当前焦点改为 Post Stage 21；新增 Stage 21 完整章节；下一阶段候选改为 Stage 22。

Task record changes:

- 已追加本记录。

Handoff changes:

- 已更新短棒交接，下一棒提示词改为 Stage 0 到 Stage 21 已完成，并建议先确认 Stage 22。

Next phase:

- Stage 22 尚未正式确认。
- 建议优先方向：provider adapter secure secret store / spend guard / sandbox 设计，仍不接真实模型；或继续扩展工作台高级编辑与 signed release 回归。

### 2026-05-14 Stage 22 Provider Adapter 安全前置层设计与占位落地完成

Date:

- 2026-05-14

Stage:

- Stage 22：Provider Adapter 安全前置层设计与占位落地。

Trigger:

- 用户要求“按照推荐的方案继续”。
- 正式范围确认为：先补 provider adapter secure secret store / spend guard / sandbox 安全前置层，仍不接真实模型；工作台批量编辑、镜头增删、更细粒度 diff 和 signed release 回归留作 Stage 23 候选。

Action:

- `ProviderSettingsDtos` 新增 provider safety / adapter safety / secret store / spend guard / sandbox DTO。
- 新增 `IProviderSecretStore` 和 `FileProviderSecretStore`；当前只保存 API Key 遮罩、SHA256 指纹、不可调用 secret reference 和 metadata-only 状态。
- `ProviderSettingsService` 接入 metadata-only secret store，保存 provider key 时不再把明文写入 settings 文件或 API 响应。
- `ProviderSettingsService` 新增 safety 状态构建、preflight 安全检查和 spend guard 判断；预算内仍返回真实 provider 调用阻断，超预算与重试溢出会被拒绝。
- Control API 新增 `GET /api/settings/providers/safety` 和 `POST /api/settings/providers/spend-guard/check`。
- Provider preflight 新增 `secret_store`、`spend_guard` 和 `provider_sandbox` 三个安全前置检查。
- Web provider 设置页显示 Stage 22 安全前置层、metadata-only secret store、spend guard 和 sandbox 状态。
- 新增 `scripts\windows\Test-MiLuStudioStage22ProviderSafety.ps1`，覆盖密钥明文不泄漏、安全状态、preflight、预算阻断、重试阻断和 clear key。
- README、建设方案、阶段计划和短棒交接已同步 Stage 22 状态。

Boundary:

- 未接真实模型 provider。
- 未读取真实媒体文件。
- 未触发 FFmpeg。
- 未生成真实 PNG / MP4 / WAV / SRT / ZIP。
- 未引入 Linux / Docker / Redis / Celery。
- UI / Electron 仍只通过 Control API / DTO / SSE 与业务系统通信。
- 未让 Web 或 Electron 直接访问 PostgreSQL、业务文件系统、Python 脚本、模型 SDK 或 FFmpeg。
- 桌面端仍不执行 migrations、不定义数据库表、不负责数据库初始化。
- 本阶段未新增数据库 migration，未实现真实 provider SDK / HTTP adapter，未保存可用于真实调用的明文 key。

Local verification:

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage22-build\

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage22ProviderSafety.ps1

git diff --check
```

Verification result:

- .NET build：通过。
- Web build：通过。
- Stage 22 provider safety integration：通过；脚本完成 settings / safety / preflight / spend guard / clear key 检查，并验证明文 key 不出现在响应、settings 文件或 secret metadata 文件中。
- `git diff --check`：通过，仅有工作区 LF / CRLF 提示，无空白错误。

Design check:

- cohesion：provider 安全前置状态集中在 `ProviderSettingsService`、provider safety DTO 和 `IProviderSecretStore` 边界内，围绕“真实 provider 接入前安全占位”单一职责组织。
- coupling：Web 只通过 Control API client 读取 / 保存 provider 设置；secret metadata 文件只由 Infrastructure store 写入，UI / Electron 不直接访问。
- boundaries：sandbox 明确阻断 provider calls、external network、media read、FFmpeg 和真实 artifact generation；spend guard 只做前置判断，不触发 provider。
- temporary debt：当前 secret store 是 metadata-only 占位，并非真实 OS secure storage / DPAPI / 硬件密钥方案；真实 provider 接入前仍需补可审计的真实安全存储、运行时解密边界和 dry-run / audit contract。

Environment check:

- project-local files：新增代码、脚本和文档均位于 `D:\code\MiLuStudio`。
- D drive only：build 输出使用 `D:\code\MiLuStudio\.tmp\stage22-build` 和 `.tmp\stage22-provider-safety-build`；测试 secret metadata 位于 `.tmp\stage22-provider-safety\...`。
- C drive risk：未新增 C 盘依赖、缓存、模型、媒体或外部服务写入。

Web searches:

- OWASP Secrets Management Cheat Sheet：确认 secret 管理应覆盖生命周期、访问控制、metadata、审计和限制；Stage 22 先保留 metadata-only，不声称已完成真实生产级 secret manager。
- OpenAI API key safety：确认 API key 不应放在浏览器 / 移动端等客户端环境，不应提交到仓库，生产系统应考虑 secret management 工具。
- OpenAI project key / spend controls：确认协作和生产系统应使用可审计、隔离的 project key、usage visibility、rate / spend controls；Stage 22 的 spend guard 是本地前置占位，不替代供应商侧限制。
- OpenAI rate limit best practices：确认重试应考虑 backoff、token / usage 估算和 limits；Stage 22 只实现重试次数硬边界，不发真实请求。
- Electron security checklist：继续保持 context isolation、sandbox、nodeIntegration=false、webSecurity 和 IPC 边界方向；Stage 22 未让 Electron 暴露 provider secret store、数据库、Python、模型 SDK 或 FFmpeg。

Sources:

- https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html
- https://help.openai.com/en/articles/5112595-best-practices-for-api-key-safety
- https://help.openai.com/en/articles/5008148-can-i-share-my-api-key-with-my-teammate-coworker
- https://help.openai.com/en/articles/6891753-what-are-the-best-practices-for-managing-my-rate-limits-in-the-api
- https://www.electronjs.org/docs/latest/tutorial/security

README check:

- 已更新根 `README.md`，加入 Stage 22 完成状态、provider 安全前置层、当前边界和下一阶段建议。

Build plan changes:

- 已更新 `docs\MILUSTUDIO_BUILD_PLAN.md`，补入 Stage 22 当前落地状态，并更新 provider 配置页与成本安全说明。

Phase plan changes:

- 当前焦点改为 Post Stage 22；新增 Stage 22 完整章节；下一阶段候选改为 Stage 23。

Task record changes:

- 已追加本记录。

Handoff changes:

- 已更新短棒交接，下一棒提示词改为 Stage 0 到 Stage 22 已完成，并建议先确认 Stage 23。

Next phase:

- Stage 23 尚未正式确认。
- 建议候选方向：工作台高级编辑（提示词批量操作、镜头增删、更细粒度 diff 和重算策略）；provider adapter 真实接入前 dry-run / audit contract；拿到正式证书后的 signed release 回归。

### 2026-05-14 Stage 23A Provider 中转配置与 FFmpeg Runtime 基础推进

Date:

- 2026-05-14

Stage:

- Stage 23A：OpenAI-compatible 中转 provider 配置、连接测试、FFmpeg 项目内 runtime 安装和工作台入口修正。

Trigger:

- 用户确认可以添加 `baseUrl + apiKey` 中转站模式、允许真实外网连接测试、允许 DPAPI 本地安全存储、允许下载 FFmpeg 到 D 盘项目 runtime，并解除真实上传 / 技术解析 / FFmpeg 后续推进限制。

Action:

- Provider settings DTO / state 新增 `baseUrl`。
- Provider catalog 新增 `openai_compatible` 供应商。
- `FileProviderSecretStore` 从 metadata-only 升级为 Windows CurrentUser DPAPI vault：API 响应仍只暴露遮罩和指纹，连接测试可在后端读取解密后的本地 key。
- 新增 `IProviderConnectivityTester` 和 `OpenAiCompatibleProviderConnectivityTester`，只请求 `/models` 与 `/v1/models`，不发送生成 payload。
- Control API 新增 `POST /api/settings/providers/{kind}/connection-test`。
- Web 模型设置页新增 Base URL 输入和“测试连通”按钮；保存时同步 `baseUrl`。
- 修正 Web 网络错误提示，避免裸露浏览器原始 `Failed to fetch`。
- 工作台左上“模型”入口已隐藏，左下设置菜单内 provider 入口改为“模型”。
- 新增 `scripts\windows\Install-MiLuStudioFfmpeg.ps1`，优先使用 `https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip`，BtbN GitHub `/latest/` 作为 fallback。
- 已执行 FFmpeg 安装，当前本机 `D:\code\MiLuStudio\runtime\ffmpeg\bin` 下已有 `ffmpeg.exe` 与 `ffprobe.exe`，runtime 目录仍由 `.gitignore` 排除。

Boundary:

- 真实模型“生成调用”仍未接入；本阶段只允许 provider 连接测试。
- 连接测试会真实访问外网 / 中转站，但不发送故事、图片、视频、音频或正式生成请求。
- FFmpeg 二进制不提交 Git；只提交稳定下载 / 安装脚本。
- UI / Electron 不直接访问 key、文件系统、FFmpeg、Python 或数据库，仍只通过 Control API / DTO / SSE。
- 未引入 Linux / Docker / Redis / Celery。
- 未新增数据库表或 migrations；桌面端仍不执行 migrations、不定义表、不负责数据库初始化。

Local verification:

```powershell
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage23-build\

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Install-MiLuStudioFfmpeg.ps1
```

Verification result:

- .NET build：通过，0 warning / 0 error。
- Web build：通过。
- 临时 InMemory Control API smoke：通过；`POST /api/settings/providers/text/connection-test` 返回预期 `http_404`，证明 endpoint、DPAPI 保存读取和 OpenAI-compatible `/models` 探测链路可用，且 `generationPayloadSent=false`。
- FFmpeg install：通过；`gyan.dev` release essentials 下载成功，SHA256 sidecar 校验通过，安装到项目 runtime。

Remaining work:

- 真实上传后端链路、技术解析、OCR、缩略图、抽帧、转码、最终媒体文件输出仍待实现。
- `.doc` / OCR / PDF 等高级解析依赖和降级提示仍待分阶段补齐。
- 大文件文本切片、图片 / 视频压缩上传、分片上传与模型上下文上限策略仍待设计落地。
- 右侧流程“待确认 / 已确认 / 回退 / 二级确认 / 下游重置”仍待完整实现；本棒只处理了模型入口合并。

### 2026-05-14 Stage 23A 真实上传解析与审核回退补齐

Date:

- 2026-05-14

Stage:

- Stage 23A：真实上传、技术解析、工作台附件上传链路和审核回退。

Action:

- Control API 新增 `POST /api/projects/{projectId}/assets/upload`，通过 multipart/form-data 接收文件。
- Application 层新增 `ProjectAssetUploadService`、`IProjectAssetFileStore`、`IAssetTechnicalAnalyzer`，保持上传编排、文件存储和技术解析职责分离。
- Infrastructure 层新增 `LocalProjectAssetFileStore`，将上传文件保存到 D 盘项目 `uploads` 根内，计算 SHA256，并保护路径不逃逸项目目录。
- Infrastructure 层新增 `FfmpegAssetTechnicalAnalyzer`：
  - 文本 / Markdown / JSON / SRT / ASS / VTT / CSV / XML / YAML / RTF 直接读取正文。
  - DOCX 通过 ZIP + XML 提取正文。
  - DOC / PDF 当前保存并返回 metadata-only，标记后续 Office/PDF/OCR 解析器待补。
  - 图片 / 视频通过 `runtime\ffmpeg\bin` 下的 `ffprobe.exe` / `ffmpeg.exe` 做技术探测、缩略图和最多 8 张视频抽帧。
- `ControlPlaneOptions` 新增 `UploadsRoot`、`FfmpegBinPath`、`AssetParseTimeoutSeconds`、`AssetTranscodeTimeoutSeconds` 和 `AssetVideoFrameLimit`。
- Web 工作台附件卡改为保留真实 `File` 对象，提交时上传解析；输入框文案改为制作要求，不再在浏览器本地读取文本内容作为最终来源。
- 文本上传上限 50MB，图片 50MB，视频 1GB；后续模型真实上限前仍需补文本切片、上传分片和压缩策略。
- 右侧固定流程新增状态文本：当前 / 下一步显示“未开始 / 进行中 / 待确认 / 已确认”；最近一个已确认审核步骤 hover 显示“回退”。
- 新增 `POST /api/production-jobs/{jobId}/rollback` 与前端二级确认弹窗；回退后目标步骤回到 review / paused，下游 task 清空 output / 状态并回到 waiting。

Boundary:

- 真实模型生成 provider 仍未接入；provider 只允许连接测试。
- UI / Electron 不直接读取上传文件、不执行 FFmpeg、不访问业务文件系统；真实上传和 FFmpeg 只在 Control API 后端 adapter 边界内执行。
- 当前不会生成最终真实 MP4 / WAV / SRT / ZIP；缩略图和抽帧只作为上传解析派生文件。
- 未新增数据库 migration；复用现有 `assets` 表。
- 未引入 Linux / Docker / Redis / Celery。

Local verification:

```powershell
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage23-build\

Push-Location D:\code\MiLuStudio\apps\web
npm run build
Pop-Location

git diff --check
```

Verification result:

- .NET build：通过，0 warning / 0 error。
- Web build：通过。
- `git diff --check`：通过，仅有 CRLF 换行提示。
- 临时 InMemory Control API upload smoke：通过；上传 txt 返回 `story_text`、SHA256、解析正文长度，未发送 generation payload。

Remaining work:

- OCR、PDF / DOC 深度解析、文本切片、上传分片、图片 / 视频压缩和更完整媒体抽帧仍待 Stage 23B。
- 真实 provider 生成 adapter、dry-run 审计、预算流水和失败隔离仍待设计落地。
- Playwright / Electron 截图级交互验收未执行。

### 2026-05-14 Stage 23B 编号与稳定范围确认

Date:

- 2026-05-14

Stage:

- Stage 23B：文档 / 媒体深度解析与上传策略加固。

Trigger:

- 用户确认按稳定路线继续推进，并要求把后续安排写入相关文档，避免每次开工前都在候选方向里重新确认。

Action:

- 将 Stage 23B 正式编号和范围写入 `docs\MILUSTUDIO_PHASE_PLAN.md` 当前焦点。
- 在 `docs\MILUSTUDIO_PHASE_PLAN.md` 新增 Stage 23B 章节，明确目标、边界、具体任务、验收和后置阶段。
- 在 `docs\MILUSTUDIO_BUILD_PLAN.md` 记录 Stage 23B 正式安排和后续阶段顺序。
- 更新根 `README.md` 的当前阶段、当前边界和下一阶段说明。
- 更新 `docs\MILUSTUDIO_HANDOFF.md`，下一棒不再需要先确认 Stage 23B，直接推进 OCR、PDF / DOC 深度解析、文本切片、上传分片、图片 / 视频压缩和更完整媒体抽帧。

Scope:

- Stage 23B 优先补上传后处理地基：OCR、PDF / DOC 深度解析、文本切片、上传分片、图片 / 视频压缩和更完整媒体抽帧。
- 解析器或运行时不可用时必须返回结构化降级 metadata，不让上传失败成裸异常。
- 默认复用现有 `assets` 表和 `metadata_json` 记录解析 metadata、切片 manifest、派生文件和降级原因。

Deferred:

- Stage 23D：provider adapter 真实接入前 dry-run / audit contract、审计日志、预算流水、超时和失败隔离。
- Stage 24：工作台高级编辑，包括提示词批量操作、镜头增删、更细粒度 diff 和重算策略。
- 发布回归阶段：拿到正式 Authenticode 证书后再做 `verify:release:signed` 和干净 Windows 虚拟机安装 / 卸载回归。

Boundary:

- 本次只修改文档，不改生产代码。
- 仍不接真实模型生成 provider。
- 仍不发送故事、图片、视频或音频到外部生成接口。
- 仍不生成最终真实 MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery。
- UI / Electron 仍不得直接读取媒体、执行 FFmpeg、访问文件系统、数据库、Python 脚本或模型 SDK。
- 桌面端仍不执行 migrations、不定义数据库表、不负责数据库初始化。

Verification:

- 已用 UTF-8 读取 README、建设方案、阶段计划、任务记录和短棒交接的 Stage 23A / Stage 23B 相关段落。
- `git diff --check -- README.md docs/MILUSTUDIO_BUILD_PLAN.md docs/MILUSTUDIO_PHASE_PLAN.md docs/MILUSTUDIO_TASK_RECORD.md docs/MILUSTUDIO_HANDOFF.md`：通过，仅有既有 LF / CRLF 提示。

Next phase:

- 直接实现 Stage 23B。
- 首先盘点 Stage 23A 的 asset upload / analyzer 边界，再补稳定 metadata schema、PDF / DOC / OCR 降级路径和文本切片 manifest。

### 2026-05-14 Stage 23B 上传解析 Metadata 与切片第一轮落地

Date:

- 2026-05-14

Stage:

- Stage 23B：文档 / 媒体深度解析与上传策略加固。

Trigger:

- 用户要求按稳定路线直接推进 Stage 23B，并允许把后续安排写入文档，减少每次接棒前的重复确认。

Action:

- 继续沿用 Stage 23A 的上传分层：`ProjectAssetUploadService` 只负责编排、分类、大小限制和 assets 登记；`LocalProjectAssetFileStore` 负责保存文件和 SHA256；`FfmpegAssetTechnicalAnalyzer` 负责后端解析、FFmpeg 和派生文件。
- `ProjectAssetUploadService` metadata stage 升级为 `stage23b_document_media_analysis`，新增 `analysisSchemaVersion=stage23b_asset_analysis_v1`。
- 资产 metadata 新增 `upload.chunkingPolicy`，记录 Stage 23B 可恢复分片上传契约、8MB 推荐分片、后端合并边界和 UI / Electron 不直接访问文件系统的约束；本次未新增分片 endpoint。
- `FfmpegAssetTechnicalAnalyzer` 重写为 Stage 23B analyzer：
  - 文本、DOCX、PDF 嵌入文本解析结果生成 `text`、`contentBlocks` 和 `chunkManifest`。
  - DOCX ZIP entry 兼容 Windows `word\document.xml` 与标准 `word/document.xml` 两种路径。
  - PDF 增加轻量嵌入文本探测，支持简单 `Tj` / `TJ` literal string 和 hex string；扫描版、复杂编码或压缩流返回 `ocr_required` 降级。
  - DOC 返回 `parser_unavailable`、后端 converter runtime 建议路径和 unavailable chunk manifest，不引入 UI / Electron Office 自动化。
  - OCR 当前落地为 Tesseract-compatible 后端 runtime 能力检测 metadata，缺少运行时时记录 `runtime_not_configured`，真实 OCR 调用留作后续小步。
  - 图片通过项目内 FFmpeg 生成 `thumbnail.jpg` 与 `preview_1280.jpg`。
  - 视频按时长均匀抽帧，记录 target / actual frame count，并尝试生成短 `review_proxy_720p.mp4`；仍不生成最终成片。
  - ffprobe JSON 被压缩成 `probeSummary`，派生文件写入 `derivativeDetails`。
- 新增 `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1`，启动临时 InMemory Control API，覆盖 txt / DOCX / PDF / DOC / PNG / MP4 上传解析、chunk manifest、结构化降级、媒体派生 metadata 和 no-provider 边界。

Boundary:

- 未接真实 Text / Image / Video / Audio / Edit 生成 provider。
- 未发送生成 payload；metadata 中继续记录 `generationPayloadSent=false` 和 `modelProviderUsed=false`。
- 未新增数据库 migration，继续复用现有 `assets.metadata_json`。
- 未引入 Linux / Docker / Redis / Celery。
- UI / Electron 未改动，仍只能通过 Control API 上传和消费 DTO，不直接读取文件、执行 FFmpeg 或访问业务文件系统。
- 本次生成的图片 preview、视频抽帧和 review proxy 均为上传解析派生文件，不是最终 MP4 / WAV / SRT / ZIP 导出。

Verification:

```powershell
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1

git diff --check
```

Verification result:

- .NET build：通过，0 warning / 0 error。
- Web build：通过；本次未改 Web，但已跑 `tsc -b && vite build` 回归。
- Stage 23B asset parsing：通过；脚本完成 txt / DOCX / PDF / DOC / PNG / MP4 上传，验证 chunk manifest、PDF embedded text、DOC parser_unavailable、图片 / 视频后端派生 metadata、no-provider 边界和 assets 登记。
- `git diff --check`：通过，仅有既有 LF/CRLF 替换提示。

Remaining work:

- 真实 OCR runtime 调用仍待补；当前只记录 runtime 能力检测和结构化降级。
- 可恢复上传分片 endpoint、分片合并状态机、断点续传和错误恢复仍待补；当前只记录契约与大小策略。
- PDF / DOC 深度解析仍可继续增强：复杂 PDF 流、扫描页 OCR、legacy DOC converter runtime 和页码 / 段落级来源信息。
- chunk manifest 尚未接入后续生产链路消费和 UI 展示。
- Playwright / Electron 截图级回归未执行；本次未改 Web。

Next phase:

- 继续 Stage 23B，小步补真实 OCR runtime 调用或可恢复分片上传 endpoint，二者优先选风险更低、验证闭环更短的一项。

### 2026-05-14 Stage 23B 可恢复分片上传 Endpoint 落地

Date:

- 2026-05-14

Stage:

- Stage 23B：文档 / 媒体深度解析与上传策略加固。

Trigger:

- 根据 Stage 23B 第一轮剩余项，选择风险更低、验证闭环更短的“可恢复分片上传 endpoint”继续推进；真实 OCR runtime 调用后置到下一小步。

Action:

- 新增 `IProjectAssetUploadSessionStore` 抽象，负责创建 session、保存 chunk、组装临时文件和标记完成。
- 新增 `LocalProjectAssetUploadSessionStore`，将 session manifest、chunks 和 assembled 临时文件限制在 `uploads\.upload-sessions` 内，并保护路径不逃逸上传根。
- 新增 `ProjectAssetChunkUploadService`，保持 Application 层编排职责：
  - 校验项目存在、文件类型、大小上限和 chunk size。
  - 创建 24 小时有效 session。
  - 支持乱序上传 chunk，并按 `X-MiLuStudio-Chunk-Sha256` 可选校验。
  - complete 时检查 chunk 是否完整，合并后交给既有 `ProjectAssetUploadService` 保存、解析和登记 assets。
- Control API 新增：
  - `POST /api/projects/{projectId}/assets/upload-sessions`
  - `GET /api/projects/{projectId}/assets/upload-sessions/{sessionId}`
  - `PUT /api/projects/{projectId}/assets/upload-sessions/{sessionId}/chunks/{chunkIndex}`
  - `POST /api/projects/{projectId}/assets/upload-sessions/{sessionId}/complete`
- `ProjectAssetUploadRequest` 新增可选 `UploadMode`，分片 complete 后的 metadata 记录 `upload.mode=control_api_resumable_chunks`。
- `ProjectAssetUploadService` 的 chunk policy 从 `contract_recorded` 更新为 `endpoint_available`，并暴露 min / preferred / max chunk bytes 常量供 session service 复用。
- 新增 `scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1`，覆盖 1MB chunk session、乱序上传、resume status、complete、asset 解析、chunk manifest 和 no-provider 边界。

Boundary:

- 未新增数据库 migration；session 状态使用本地 manifest 文件，后续如需跨机器审计再考虑数据库化。
- 未改 UI / Electron；前端后续只能通过新增 Control API 使用分片上传，不得直接访问 chunks 或文件系统。
- complete 后仍复用现有上传解析链路；不复制 analyzer，不接真实模型生成，不发送 generation payload。
- 未引入 Linux / Docker / Redis / Celery。

Verification:

```powershell
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1 -SkipBuild

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1

git diff --check
```

Verification result:

- .NET build：通过，0 warning / 0 error。
- Web build：通过；本次未改 Web，但已跑 `tsc -b && vite build` 回归。
- Stage 23B asset parsing：通过；确认第一轮 txt / DOCX / PDF / DOC / PNG / MP4 上传解析仍兼容。
- Stage 23B chunked upload：通过；临时 InMemory Control API 完成 session 创建、chunk 1/0/2 乱序上传、resume status、complete、asset metadata `control_api_resumable_chunks`、chunk manifest 和 no-provider 边界验证。
- `git diff --check`：通过，仅有既有 LF/CRLF 替换提示。

Remaining work:

- 真实 OCR runtime 调用仍待补。
- DOC/PDF 更深解析、页码 / 段落级来源信息仍待补。
- chunk manifest 尚未接入后续生产链路消费和 UI 展示。
- 分片上传 UI 尚未接入；当前仅后端 API 与脚本验证通过。

Next phase:

- 继续 Stage 23B，优先补真实 OCR runtime 调用；如果 OCR runtime 不可控，则先让 chunk manifest 被工作台详情或后续生产链路稳定消费。
### 2026-05-15 Stage 23B Asset Analysis 消费接口落地

Date:

- 2026-05-15

Stage:

- Stage 23B：文档 / 媒体深度解析与上传策略加固。

Trigger:

- 本机未发现 `D:\code\MiLuStudio\runtime\tesseract\tesseract.exe` 或 `D:\tools\tesseract\tesseract.exe`，`Get-Command tesseract.exe` 也不可用；按 Stage 23B fallback，先让 chunk manifest 被后续链路通过 Control API 稳定消费。

Action:

- 新增 `ProjectAssetAnalysisService`，通过 `IAssetRepository.ListAssetsByProjectAsync` 查找项目资产，并只解析现有 `assets.metadata_json`。
- 新增 `ProjectAssetAnalysisResponse` / `ProjectAssetAnalysisBoundary` / `ProjectAssetChunkManifestSummary` / `ProjectAssetDerivativeSummary` / `ProjectAssetOcrSummary` DTO。
- 新增 `GET /api/projects/{projectId}/assets/{assetId}/analysis`，返回 asset 基本信息、schema version、parse / upload metadata、parser、OCR summary、text summary、content blocks、chunk manifest、limits、derivative summary 和 no-provider / no-UI-file-access 边界。
- OCR summary 不返回 Tesseract 候选本地路径，只返回 engine、status、candidate、runtimeAvailable、checkedPathCount 和边界标记。
- derivative summary 不返回 `uploads/storage` 本地路径，只返回 count、kind 和 `backend_adapter_only` access policy。
- 更新 `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1`，在 txt / DOCX / PDF / DOC / PNG / MP4 上传后断言 analysis endpoint 可读、chunk manifest 可消费、no-provider 边界未破坏且响应不泄漏本地 uploads/storage 路径。
- 更新 `scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1`，在 chunked complete 后断言 analysis endpoint 保留 `control_api_resumable_chunks` mode 并暴露 manifest chunks。
- 同步 README、建设方案、阶段计划和短棒交接，后续不再把“chunk manifest 可通过 Control API 消费”作为未落地项。

Boundary:

- 未接真实模型生成 provider，未发送 generation payload。
- 未新增数据库 migration，继续复用 `assets.metadata_json`。
- 未改 UI / Electron；后续工作台详情只能调用 analysis endpoint，不得直接读取本地路径。
- 未读取媒体文件、未执行 FFmpeg、未调用 OCR runtime；本次接口是 metadata consumption API。
- 未引入 Linux / Docker / Redis / Celery。

Verification:

```powershell
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1

git diff --check
```

Verification result:

- .NET build：通过，0 warning / 0 error。
- Stage 23B asset parsing：通过；analysis endpoint 已覆盖 txt / DOCX / PDF / DOC / PNG / MP4，确认 manifest 与边界可消费。
- Stage 23B chunked upload：通过；complete 后 asset analysis endpoint 可读取 `control_api_resumable_chunks` mode 与 chunk manifest。
- `git diff --check`：通过，仅有既有 LF/CRLF 替换提示。

Remaining work:

- 真实 OCR runtime 调用仍待补；当前环境未安装可控 Tesseract-compatible runtime。
- DOC/PDF 更深解析、页码 / 段落级来源信息仍待补。
- 工作台详情尚未接入 analysis endpoint；生产链路也尚未实际消费 chunk manifest。
- 分片上传 UI 尚未接入；当前仍是后端 API 与脚本验证通过。

Next phase:

- 继续 Stage 23B，优先补真实 OCR runtime 调用；如果 OCR runtime 仍不可控，则推进工作台详情展示或生产链路实际消费 analysis endpoint。
### 2026-05-15 Stage 23B 图片 OCR Runtime 调用路径落地

Date:

- 2026-05-15

Stage:

- Stage 23B：文档 / 媒体深度解析与上传策略加固。

Trigger:

- 用户要求继续 Stage 23B，并优先补真实 OCR runtime；开工前已重新读取 README、建设方案、阶段计划、任务记录和短棒交接。

Action:

- 检查本机 OCR runtime：`D:\code\MiLuStudio\runtime\tesseract\tesseract.exe`、`D:\tools\tesseract\tesseract.exe` 和 PATH 中的 `tesseract.exe` 均不存在。
- `ControlPlaneOptions` 新增 `OcrTesseractPath`、`OcrTessdataPath`、`OcrLanguages` 和 `OcrTimeoutSeconds`；`ServiceCollectionExtensions` 同步读取配置。
- `FfmpegAssetTechnicalAnalyzer` 新增 Tesseract-compatible runtime 探测：优先使用配置路径，其次使用项目 `runtime\tesseract` 和 `D:\tools\tesseract`。
- 图片上传在 Infrastructure adapter 内尝试真实调用 Tesseract CLI：输入为后端保存的上传文件，输出走 `stdout`，默认语言候选为 `chi_sim+eng;eng`，默认 `--psm 6`。
- OCR 成功时写入 `technical.ocr.status=ok`、`invoked=true`、语言、文本长度，并把 OCR 文本写成 `technical.text`、`technical.contentBlocks` 和 `technical.chunkManifest`，sourceType 为 `image_ocr`。
- OCR runtime 缺失、语言包缺失、超时、运行失败或无文本时，上传仍成功并返回结构化 metadata；当前本机验证的是 `runtime_not_configured` 降级路径。
- PDF 仍只做 embedded text probe；扫描 PDF 不直接传给 Tesseract，metadata 明确 `pdfRasterizerRequired=true`，后续需补 PDF rasterizer / page image extraction。
- `ProjectAssetOcrSummary` 增加 `invoked`、`language` 和 `extractedTextLength`，analysis endpoint 可看见 OCR 调用状态但不暴露本地候选路径为 UI 契约。
- `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1` 新增带文字 PNG fixture：runtime 可用时要求 OCR 成功并生成 manifest；runtime 缺失时要求 `runtime_not_configured`、不越界、不失败。
- 同步 README、建设方案、阶段计划和短棒交接。

Boundary:

- 未接真实模型生成 provider，未发送 generation payload。
- 未新增数据库 migration，继续复用 `assets.metadata_json`。
- 未改 UI / Electron；OCR 只在后端 Infrastructure adapter 内读取上传文件和调用本地 runtime。
- 未引入 Linux / Docker / Redis / Celery。
- 当前不生成最终 MP4 / WAV / SRT / ZIP。

Verification:

```powershell
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1

git diff --check
```

Verification result:

- .NET build：通过，0 warning / 0 error。
- Stage 23B asset parsing：通过；txt / DOCX / PDF / DOC / PNG / OCR PNG / MP4 上传解析兼容，OCR PNG 在当前无 runtime 环境下返回 `runtime_not_configured` 降级 metadata，未触发 provider 或 UI/Electron 文件边界。
- Stage 23B chunked upload：通过；确认 OCR DTO 扩展后可恢复分片上传和 analysis endpoint 仍兼容。
- `git diff --check`：通过，仅有既有 LF/CRLF 替换提示。

Remaining work:

- 当前本机仍未安装可控 Tesseract-compatible runtime；OCR 正向路径已实现但尚未在真实 runtime 下验证。
- 需要补 `runtime\tesseract` 安装 / 固化脚本或手工安装说明，并补 eng / chi_sim tessdata 正向 OCR 验证。
- 扫描 PDF 仍需 PDF rasterizer / page image extraction 后才能进入 OCR。
- DOC/PDF 更深解析、页码 / 段落级来源信息仍待补。
- 工作台详情和生产链路仍未实际消费 OCR / chunk manifest。

Next phase:

- 继续 Stage 23B，优先固化 Tesseract-compatible runtime 安装和正向 OCR 验证；如果 runtime 仍不可控，则推进 PDF rasterizer、工作台详情展示或生产链路实际消费 analysis endpoint。

### 2026-05-15 Stage 23B-P0 SQLite 与安装包依赖路线确认

Date:

- 2026-05-15

Stage:

- Stage 23B-P0：SQLite 本地持久化与开发稳定化补丁。

Trigger:

- 用户确认不需要保留 PostgreSQL 相关路线，要求按安装包产品方向把当前项目数据库全面转向 SQLite。
- 用户补充要求同步此前产品判断：安装包尽量自带可控 runtime；设置里的依赖中心负责检测、修复、启用和导入离线包，不把在线下载作为基础体验前提。

Decision:

- SQLite 足以承载当前单机安装包路线的项目、账号、会话、任务、资产索引、JSON envelope、metadata、成本 / 审计流水和 provider 配置指纹。
- 图片、视频、音频、字幕、最终导出、缩略图、抽帧、OCR 中间文件、本地模型权重和模型缓存不进入 SQLite，只由后端 adapter 管理文件路径、hash、大小、格式、派生关系和状态。
- 后续如增加本地大模型，数据库只记录模型配置、启用状态、任务请求、日志、成本估算和产物索引；大模型文件与缓存继续走文件目录和依赖中心。
- 安装包优先自带可控 runtime；无法随主包携带的大体积依赖走离线依赖包导入。在线下载只作为用户明确触发的辅助修复路径。

Action:

- 同步更新 README：当前阶段、技术栈、数据库与持久化说明、桌面打包、当前边界、下一阶段、项目亮点和文档导航口径。
- 同步更新建设方案：主干技术判断、安装包依赖路线、技术栈、架构边界和 Stage 23B-P0 补丁范围。
- 同步更新阶段计划：当前焦点改为 Stage 23B-P0，并说明 OCR / PDF / DOC 深度解析顺延。
- 同步更新短棒交接：下一棒优先执行 SQLite 迁移、依赖中心契约和后端开发稳定化补丁。

Boundary:

- 本次只同步文档，不修改后端、前端、Electron 或脚本实现。
- 不接真实模型生成 provider，不引入 Linux / Docker / Redis / Celery。
- 不让 UI 或 Electron 直接访问 SQLite、业务文件系统、FFmpeg、OCR runtime、Python 脚本或模型 SDK。
- 不把 SQLite schema 初始化、migration 或数据库文件管理交给 Electron；仍由 Control API / Infrastructure 负责。

Next phase:

- 进入 Stage 23B-P0 实现补丁：替换 PostgreSQL provider / Npgsql / SQL migration runner / 默认配置为 SQLite，修复阶段脚本误杀开发后端，并设计依赖中心后端契约。
- Stage 23B 原 OCR runtime 固化、PDF rasterizer、DOC/PDF 深度解析、工作台详情和生产链路消费顺延到补丁后继续。

### 2026-05-15 Stage 23B-P0 SQLite 实现补丁完成

Date:

- 2026-05-15

Stage:

- Stage 23B-P0：SQLite 本地持久化与开发稳定化补丁。

Action:

- 将 Infrastructure 默认 provider 从 PostgreSQL / Npgsql 切换为 EF Core SQLite，运行时默认连接 `storage\milu-control-plane.sqlite3`，InMemory 仅保留为显式 smoke 备选。
- 将原 PostgreSQL DbContext / repository / auth repository / migration / preflight 实现收敛为 SQLite 版本；JSON envelope 字段改为 SQLite `TEXT`，migration service 使用后端 `EnsureCreated` 初始化 schema。
- 将 Worker durable claiming 从 `FOR UPDATE SKIP LOCKED` 改为 SQLite 查询候选任务 + 乐观 `ExecuteUpdate` 领取策略，继续保持 Worker / Control API 共享同一份本地状态。
- 更新 API / Worker appsettings、Desktop runtime 路径和 Web 文案；桌面端只注入 SQLite 连接路径和 Control API 配置，不定义表、不执行 migration、不读取数据库文件。
- 新增 `GET /api/system/dependencies`，基于后端 preflight 返回 SQLite、storage、uploads、FFmpeg、OCR、Python runtime 和 skills root 状态，并明确 bundled / offline runtime 优先、在线下载仅辅助。
- 移除运行时 Npgsql package reference 和 `scripts\windows\Initialize-MiLuStudioPostgreSql.ps1`；Stage14 / Stage16 / Stage17 / Stage21 / Stage23B 验证脚本改为临时 SQLite 数据库。
- 收窄 Stage17 / Stage21 / Stage23B 脚本的 dotnet 进程清理规则，只清理脚本 build output 启动的进程，避免误杀正在 5368 端口运行的开发后端。

Boundary:

- 未接真实模型生成 provider，未引入 Linux / Docker / Redis / Celery。
- SQLite 初始化、preflight、真实上传、OCR、FFmpeg 和依赖检测仍只在 Control API / Infrastructure adapter 边界内执行。
- UI / Electron 仍不得直接访问 SQLite、媒体文件、业务文件系统、FFmpeg、OCR runtime、Python 脚本或模型 SDK。

Verification:

- `D:\soft\program\dotnet\dotnet.exe restore backend\control-plane\MiLuStudio.ControlPlane.sln`：通过。
- `D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore`：通过，0 warning / 0 error。
- SQLite smoke：临时 Control API 通过 `/health`、`/api/system/migrations/apply`、`/api/auth/register`、`/api/system/dependencies`，并创建本地 `.sqlite3` 文件。
- `powershell -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1`：通过，临时 SQLite Control API 完成分片上传、resume、complete 和 analysis endpoint。
- `powershell -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1`：通过，临时 SQLite Control API 完成 txt / DOCX / PDF / DOC / PNG / MP4 解析验证。
- `npm run build` in `apps\web`：通过。
- `npm run build` in `apps\desktop`：通过。

Next phase:

- 回到 Stage 23B 原顺延项：Tesseract-compatible OCR runtime 固化和正向 OCR 验证、PDF rasterizer、DOC/PDF 更深解析、工作台详情展示和生产链路实际消费。
- 分片上传 UI 与依赖中心 UI / 修复动作仍后续推进，但必须只通过 Control API / 后端 adapter 管理。

### 2026-05-15 Stage 23B-P1 检查与阶段切分完成

Date:

- 2026-05-15

Stage:

- Stage 23B-P1：PostgreSQL 残留检查、Web dev 启动问题诊断和 P2 / P3 切分判断。

Decision:

- PostgreSQL 清理 1/2/3/4 与 Web dev 后端启动编排都属于 Stage 23B-P0 SQLite 迁移后的稳定化收尾，应该合并进入 Stage 23B-P2。
- 原 Stage 23B 的 OCR runtime 固化、PDF rasterizer、DOC/PDF 深度解析、工作台详情和生产链路实际消费不取消，补丁结束后恢复为正式 Stage 23C。
- 不把 P2 拆成 PostgreSQL 清理和 Web 启动两轮，原因是两者都会触碰本地开发入口、文档口径、旧生成物和 Desktop runtime；分开做会让旧 runtime / 旧说明继续干扰下一轮验证。

Findings:

- 后端运行时源码已切到 SQLite：API / Worker 默认 provider、EF provider、preflight、migration service、Worker claiming 和桌面 runtime 路径均已走 SQLite；`Npgsql` 运行时 package 已移除。
- 使用全新临时 SQLite 在 5368 启动 Control API 时，`/health` 返回 SQLite，`/api/auth/me` 可返回未登录状态，并且 schema 可自动初始化为 `up_to_date`。
- 截图中的登录页问题不是 SQLite 后端启动失败，而是浏览器 Web dev 入口只启动 Vite，没有同时编排默认 `http://127.0.0.1:5368` Control API；前端请求 `/api/auth/me` 失败后进入 `control_api_unavailable` 降级状态。
- 当前仍存在需要 P2 清理的 PostgreSQL 残留：旧 SQL migration 目录、Stage12 PostgreSQL setup 文档、当前文档里的历史 / 当前路线混杂描述、Python skill README / examples 中的旧描述，以及 `.tmp`、`apps\desktop\runtime`、`outputs\desktop` 等旧生成物中的历史配置。

Planned P2 scope:

- 删除或归档旧 PostgreSQL SQL migration 目录和 Stage12 setup 文档。
- 清理当前非归档文档和 Python skill 示例中的 PostgreSQL 当前路线描述，统一为 SQLite / Control API / repository-neutral 口径；历史归档和必要历史阶段记录可保留事实背景。
- 清理 `.tmp`、`apps\desktop\runtime`、`outputs\desktop` 后重新生成 Desktop runtime，避免旧 runtime 继续携带 PostgreSQL / Npgsql 文本或配置。
- 为浏览器 Web dev 增加本地服务启动 / 停止编排，稳定启动 5368 Control API 与同库 Worker，并只停止脚本自己记录的进程。
- 改善 Web `control_api_unavailable` 展示，把 Control API 不可达与普通登录态区分开。

Boundary:

- 本轮只做判断和文档同步，不执行删除、清理、runtime 重新生成或代码修复。
- 后续 P2 仍不得让 UI / Electron 绕过 Control API / Worker 边界；Electron 不执行 migrations、不定义数据库表、不负责 SQLite 初始化。
- 后续 P2 不接真实模型生成 provider，不引入 Linux / Docker / Redis / Celery。

Documentation updates:

- 更新 `README.md`：同步 P1 判断、P2 / P3 切分、下一阶段和 Web dev 只启动 Vite 的当前说明。
- 更新 `docs\MILUSTUDIO_BUILD_PLAN.md`：新增阶段切分判断、P2 范围和 P2 验收。
- 更新 `docs\MILUSTUDIO_PHASE_PLAN.md`：当前焦点改为 Stage 23B-P2 pending，并保留原 Stage 23B 任务顺延到 P3。
- 更新 `docs\MILUSTUDIO_HANDOFF.md`：下一棒交接改为 P2 收尾稳定化，并更新下一棒提示词。

Next phase:

- 等用户确认后执行 Stage 23B-P2 收尾稳定化。
- Stage 23C 再继续 OCR runtime 固化、PDF rasterizer、DOC/PDF 深度解析、工作台详情和生产链路消费。

### 2026-05-15 Stage 23B-P2 收尾稳定化完成

Date:

- 2026-05-15

Stage:

- Stage 23B-P2：SQLite 迁移后的稳定化收尾。

Trigger:

- 用户要求在 P1 判断基础上继续推进 P2：清理 PostgreSQL 残留、确认运行内容已迁移到 SQLite、稳定后端启动，并修复截图中浏览器 Web 入口只显示 `Control API 未连接` 的问题。

Action:

- 删除旧 SQL migration 文件：`backend\control-plane\db\migrations\001_initial_control_plane.sql`、`002_stage12_postgresql_claiming.sql`、`003_stage14_checkpoint_notes.sql`、`004_stage16_auth_licensing.sql`。
- 删除旧 PostgreSQL setup 文档：`docs\POSTGRESQL_STAGE12_SETUP.md`。
- 清理 Python skill README、`auto_editor` / `style_bible` executor 和 examples 中仍指向 PostgreSQL 的说明，统一改为 SQLite / 后端 EF Core / Control API persistence boundary 口径。
- 清理旧 `.tmp`、`apps\desktop\runtime`、`outputs\desktop`、后端 `bin/obj` 和项目内 `.nuget` 的 Npgsql package cache；停止旧 `.tmp\stage22-build` Worker 和旧 Vite dev 进程后再删除占用日志。
- 新增 `scripts\windows\Start-MiLuStudioLocalServices.ps1`：构建 Control Plane 到 `.tmp\dev-services\build`，启动 5368 Control API，等待 `/health`，调用后端 migration apply，启动同库 Worker，并写入 `services.json` 记录 PID / build output / logs / SQLite path。
- 新增 `scripts\windows\Stop-MiLuStudioLocalServices.ps1`：只停止 `services.json` 记录且命令行同时匹配 `MiLuStudio.Api.dll` / `MiLuStudio.Worker.dll` 与记录 build output 的进程，避免误杀用户其他后端。
- 新增 `scripts\windows\Start-MiLuStudioWebDev.ps1`，并在 `apps\web\package.json` 增加 `dev:local`、`services:start`、`services:stop`；浏览器开发入口可先启动本地 Control API / Worker 再启动 Vite。
- 修复 P1 后端启动稳定性遗漏点：旧逻辑遇到 stale `services.json` 且 API health 不通时会直接退出，导致 Web 继续未连接；P2 改为先清理脚本自己记录的 stale 服务再重启。停止脚本同时避开 PowerShell 内置 `$PID` 变量名冲突。
- Web `App` 新增 `ControlApiUnavailableGate`，把 `control_api_unavailable` 与普通未登录态拆开，展示本地服务未连接、当前 Control API 地址和重试入口；普通未登录仍进入登录 / 注册入口。
- `controlPlaneClient` 暴露 `getControlApiBaseUrl()`，供不可达提示展示当前 API 地址。
- 重新生成 Desktop runtime：`apps\desktop\runtime` 下的 Web dist、Control API runtime、Worker runtime、SQLite metadata、Python skills、Python runtime 和 icon 均重新生成；runtime 文本配置无 Npgsql / PostgreSQL 残留。
- 同步更新 README、建设方案、阶段计划和短棒交接；Stage 23B 原 OCR runtime 固化、PDF rasterizer、DOC/PDF 深度解析、工作台详情和生产链路消费在补丁结束后恢复为正式 Stage 23C。

Findings:

- 运行源码中未发现 Npgsql / UseNpgsql / PostgreSQL 配置入口；残留主要来自历史文档记录、旧生成物和旧 NuGet cache。
- 截图问题的直接原因不是 SQLite 后端启动失败，而是浏览器 Web dev 只启动 Vite；P2 通过 `dev:local` 给出自动编排路径，并通过不可达专用页面避免误导用户继续停在普通登录卡片。
- `npm run services:start` 可稳定拉起 5368 Control API / Worker；`/health` 返回 `repositoryProvider=SQLite`，`/api/system/preflight` 返回 healthy，OCR runtime 当前仍是 warning，属于 P3 待处理。

Design check:

- cohesion: 本地服务编排集中在 `scripts\windows\Start/Stop-MiLuStudioLocalServices.ps1`，Web 只负责显示不可达状态和触发重试。
- coupling: Web 仍只通过 `controlPlaneClient` 调 Control API；Electron、Web、UI 均未直接读 SQLite、媒体文件、FFmpeg、OCR 或业务文件系统。
- boundaries: SQLite 初始化、migration apply、真实上传、OCR、FFmpeg 和依赖检测仍在后端 Control API / Infrastructure adapter 边界内；桌面端不执行 migrations、不定义数据库表。
- temporary debt: `dev:local` 依赖本机 D 盘 dotnet / npm 路径，后续依赖中心 UI 和正式安装包自带 runtime 仍需继续收敛；OCR runtime 缺失仍留到 P3。

Environment check:

- project-local files: 新增脚本位于 `scripts\windows`，Web 修改位于 `apps\web`，Desktop runtime 重新生成到 `apps\desktop\runtime`；生成日志和服务 build 输出位于 `.tmp`。
- D drive only: 构建、SQLite dev 文件、NuGet cache、npm cache、Python cache 和运行时目录继续使用 `D:\code\MiLuStudio` 或明确 D 盘工具路径。
- C drive risk: 未引入新的 C 盘依赖；未安装全局工具；未引入 Linux / Docker / Redis / Celery。

Verification:

```powershell
Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln

Push-Location D:\code\MiLuStudio\backend\sidecars\python-skills
D:\soft\program\Python\Python313\python.exe -m compileall -q milu_studio_skills skills tests
D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -v
Pop-Location

powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Start-MiLuStudioLocalServices.ps1 -ProjectRoot D:\code\MiLuStudio
Invoke-RestMethod -Uri http://127.0.0.1:5368/health
Invoke-RestMethod -Uri http://127.0.0.1:5368/api/system/preflight
powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Stop-MiLuStudioLocalServices.ps1 -ProjectRoot D:\code\MiLuStudio

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run services:start
D:\soft\program\nodejs\npm.ps1 run services:stop
Pop-Location

Push-Location D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run prepare:runtime
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage14Integration.ps1 -ProjectRoot D:\code\MiLuStudio
powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage16Auth.ps1 -ProjectRoot D:\code\MiLuStudio
powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1 -ProjectRoot D:\code\MiLuStudio
powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1 -ProjectRoot D:\code\MiLuStudio
powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1 -ProjectRoot D:\code\MiLuStudio -SkipPrepareRuntime
git diff --check
```

Verification result:

- Web build：通过。
- .NET build：通过，0 warning / 0 error。
- Python compileall / unittest：通过，30 tests。
- 本地服务脚本：通过；5368 `/health` 返回 SQLite provider，preflight healthy，`services:stop` 成功停止脚本记录的 API / Worker。
- Desktop runtime prepare：通过；runtime 文本配置无 Npgsql / PostgreSQL 残留。
- Desktop TypeScript build：通过。
- Stage14 integration：通过。
- Stage16 auth：通过。
- Stage23B asset parsing：通过。
- Stage23B chunked upload：通过。
- Desktop API security：通过。
- `git diff --check`：通过，仅输出 LF/CRLF 换行提示。

Remaining work:

- Stage 23C 继续处理 Tesseract-compatible OCR runtime 安装 / 正向验证、PDF rasterizer、DOC/PDF 深度解析、工作台详情展示、生产链路实际消费 analysis endpoint 和媒体派生策略回归。
- 分片上传 UI 尚未接入，后续仍必须只通过 Control API upload-sessions。
- 设置依赖中心 UI、离线包导入、修复动作和启用 / 禁用状态管理尚未实现。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B 补丁编号修正与本地启动检查

Date:

- 2026-05-15

Stage:

- Stage 23B-P2 后编号修正 / Stage 23C 前置检查。

Trigger:

- 用户指出 `Stage 23B-*P` 应只作为补丁阶段，当前文档里的 `Stage 23B-P3` 应恢复为原正式阶段编号 Stage 23C。
- 用户要求检查当前项目是否已启动；如果未启动，则清理旧启动残留并启动。

Action:

- 检查本地端口和进程：5368 / 5173 / 5174 / 4173 均未监听，`.tmp\dev-services\services.json` 不存在，确认项目未启动。
- 执行 `Stop-MiLuStudioLocalServices.ps1` 清理可能的旧服务状态，然后通过 `apps\web` 的 `npm run services:start` 启动 5368 Control API 与 Worker。
- 通过隐藏 PowerShell 进程启动 Vite Web dev，日志写入 `.tmp\web-dev.out.log` 和 `.tmp\web-dev.err.log`。
- 验证 `http://127.0.0.1:5368/health` 返回 SQLite provider，`http://127.0.0.1:5173` 返回 HTTP 200。
- 修正文档编号：`Stage 23B-P0/P1/P2` 保留为补丁阶段；下一正式阶段改为 Stage 23C，内容为 OCR runtime 固化、PDF rasterizer、DOC/PDF 深度解析、工作台详情和生产链路消费；原 provider dry-run / audit contract 顺延为 Stage 23D。

Verification:

- Web dev URL：`http://127.0.0.1:5173`
- Control API URL：`http://127.0.0.1:5368`
- SQLite dev database：`D:\code\MiLuStudio\storage\milu-control-plane.dev.sqlite3`
- Control API health：通过，`repositoryProvider=SQLite`。
- Web root：通过，HTTP 200。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B-P3 设置入口与依赖入口补丁（开始）

Date:

- 2026-05-15

Stage:

- Stage 23B-P3。

Trigger:

- 用户确认继续推进设置入口补丁：模型入口只保留在左下设置菜单内；新增设置菜单“依赖”入口，位置在“模型”下方、“剩余额度”上方。
- 本补丁只收尾 Stage 23B-P0/P1/P2 后的设置入口和依赖中心入口，不覆盖 Stage 23C 的 OCR runtime、PDF rasterizer、DOC/PDF 深度解析、工作台详情和生产链路消费任务。

Planned action:

- 删除工作台主侧栏遗留“模型”按钮，避免 `hidden` 被按钮 class display 覆盖后继续可见。
- 新增前端 `GET /api/system/dependencies` client / 类型，并在设置面板新增“依赖”页面。
- “依赖”页面只通过 Control API 展示 SQLite、storage、uploads、FFmpeg、OCR、Python runtime 和 skills root 检测状态；Web / Electron 不直接读取文件系统、不执行 FFmpeg / OCR、不访问 SQLite。
- 完成后同步 README、阶段计划、任务记录和短棒交接。

### 2026-05-15 Stage 23B-P3 设置入口与依赖入口补丁（完成）

Date:

- 2026-05-15

Stage:

- Stage 23B-P3。

Action:

- 删除 `StudioWorkspacePage.tsx` 主侧栏遗留“模型”按钮，模型配置只保留在左下设置菜单内。
- 在左下设置菜单新增“依赖”入口，顺序为个人账户、模型、依赖、剩余额度、退出登录。
- 新增 `DependencySettingsPage`，通过 `GET /api/system/dependencies` 展示 SQLite、storage、uploads、FFmpeg、OCR、Python runtime 和 skills root 状态，并显示安装策略与后端建议。
- 新增 Web system dependencies 类型与 `getSystemDependencies` client 方法；依赖面板不直接读取文件系统、不执行 FFmpeg / OCR、不访问 SQLite。
- 同步 README、建设方案、阶段计划、任务记录和短棒交接，明确 Stage 23C 主线任务继续顺延。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Vite 当前源码检查：通过；主侧栏只剩新项目、搜索、诊断，设置菜单包含依赖入口。
- `http://127.0.0.1:5368/api/system/dependencies`：通过，返回 `repositoryProvider=SQLite` 和 10 个依赖检查项。
- `git diff --check`：通过，仅输出 LF/CRLF 换行提示。
- Playwright 不在当前 web workspace，未执行截图级自动化检查。

Remaining work:

- 依赖修复动作、离线包导入、启用 / 禁用状态管理后续仍必须通过 Control API / 后端 adapter 实现。
- Stage 23C 继续处理 Tesseract-compatible OCR runtime 固化、PDF rasterizer、DOC/PDF 深度解析、工作台详情和生产链路消费。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B-P3 项目区快捷按钮收口

Date:

- 2026-05-15

Stage:

- Stage 23B-P3 follow-up UI polish。

Trigger:

- 用户要求参考 Codex 项目标题旁添加按钮的逻辑，把左上角“搜索”和“新项目”统合到“项目”右侧，仅保留图标；从右往左第一个为新项目，第二个为搜索。

Action:

- 从工作台顶部命令列表移除“新项目”和“搜索”文字按钮。
- 在“项目”标题右侧新增两个 icon-only 按钮：左侧为搜索项目，右侧为新项目。
- 保留“诊断”入口不变，避免扩大本次请求范围。
- 同步 README 和短棒交接中的左侧栏说明。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Vite 当前源码检查：通过；顶部命令区不再包含新项目 / 搜索按钮，项目标题右侧存在 `workspace-section-action` 搜索和新项目图标。
- `git diff --check`：通过，仅输出 LF/CRLF 换行提示。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B-P3 诊断入口归拢与项目区顶置

Date:

- 2026-05-15

Stage:

- Stage 23B-P3 follow-up UI polish。

Trigger:

- 用户要求将左上角剩余的“诊断”归拢到设置二级菜单里，位置放在第一个；完成后让左上角“项目”顶置。

Action:

- 从工作台左侧顶部命令区移除“诊断”按钮，左侧栏顶部现在直接显示“项目”标题行。
- 将“诊断”加入左下设置菜单的第一个可点击项，位于“个人账户”上方。
- 保持项目标题右侧图标按钮顺序不变：右侧第一个为新项目，第二个为搜索。
- 同步 README、建设方案、阶段计划、任务记录和短棒交接中的入口描述。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Vite 当前源码检查：通过；左侧栏顶部直接是 `workspace-history` / 项目区，设置菜单中 `openSettingsPanel("diagnostics")` 位于 `openSettingsPanel("account")` 之前。
- `git diff --check`：通过，仅输出 LF/CRLF 换行提示。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B-P3 左侧栏品牌栏补齐

Date:

- 2026-05-15

Stage:

- Stage 23B-P3 follow-up UI polish。

Trigger:

- 用户要求在项目区上方添加类似 XiaoLouAI 的 logo 和项目名称，项目名字为“麋鹿”，logo 位于名称左侧。

Action:

- 在 `StudioWorkspacePage.tsx` 的项目区上方新增 `workspace-brand` 品牌栏。
- 复用 Web public 目录下现有 `/brand/logo.png`，名称显示为“麋鹿”。
- 新增品牌栏样式，保持与当前暗色侧栏、项目标题和左下设置入口的间距层级一致。
- 同步 README、建设方案、阶段计划、任务记录和短棒交接中的左侧栏描述。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Vite 当前源码检查：通过；左侧栏包含 `workspace-brand`、`workspace-brand-logo` 和“麋鹿”，项目区位于其下。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B-P4 工作台 UI 视觉轻量化

Date:

- 2026-05-15

Stage:

- Stage 23B-P4 follow-up UI polish。

Trigger:

- 用户指出右侧进度栏的文本样式接近 Codex 前端，但设置菜单、开始生成、历史项目条目、模型配置、依赖和诊断界面的文本与按钮外框显得过于粗犷，要求先按检查判断后的方案直接修改。

Action:

- 调整共享按钮样式：降低 `primary` / `secondary` / `ghost` 的默认高度、字重、填充和边框存在感。
- 轻量化左下设置菜单：缩小菜单项高度和字重，弱化边框、阴影、hover 背景和分隔线。
- 轻量化历史项目条目：降低行高、左缩进、active 背景和删除按钮重量。
- 单独收敛工作台 `开始生成` 按钮，使其比旧通用主按钮更紧凑。
- 轻量化设置弹窗、模型配置、依赖和诊断面板：降低标题字号、卡片 padding、边框、圆角、状态 pill 和输入框重量。
- 右侧进度卡片当前节奏保持不变，作为本次统一视觉基准。
- 同步 README、建设方案、阶段计划、任务记录和短棒交接。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Vite 当前 CSS 检查：通过；源码中已包含轻量化后的 `workspace-settings-menu`、`workspace-composer-footer .primary-button`、`settings-panel-sheet`、`provider-summary-strip` 和 `provider-adapter-card` 样式。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B-P5 composer 与品牌栏细节收尾

Date:

- 2026-05-15

Stage:

- Stage 23B-P5 follow-up UI polish。

Trigger:

- 用户确认将品牌栏、composer 输入框、生成按钮和上传限制提示作为 P5 收口：缩小并弱化“麋鹿”logo / 字样，不改“项目”标题；输入框加高并更明确提示“上传剧本文档后，写下本次制作要求”；按真实文件大小限制落地，不新增文本字数硬限制；新项目只有上传故事文本附件后才启用生成。

Action:

- 移除当前 Web 工作台 composer 的旧 `500-2000` 字符提交拦截、`0/2000` 计数展示和自动截断逻辑；故事正文继续来自 Control API 上传解析后的文本附件或既有项目故事文本。
- 新项目 submit 启用条件改为必须存在故事文本附件；制作要求输入框只作为制作要求补充，不再单独作为新项目故事来源。
- 上传菜单描述展示真实文件大小限制：文本 50 MB、图片 50 MB、视频 1 GB；不新增文本字数硬限制，避免把旧输入框限制误当上传解析限制。
- 左侧品牌栏减重：缩小并降低 logo 饱和 / 亮度和不透明度，降低“麋鹿”字号与字重；不改“项目”标题样式。
- composer 输入区加高到多行制作要求形态，placeholder 改为“上传剧本文档后，写下本次制作要求”。
- `开始生成 / 更新生成` 改为 composer 专用轻按钮样式，不再复用通用主按钮外观。
- 同步 README、建设方案、阶段计划、任务记录和短棒交接，继续声明 Stage 23C 主线顺延。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Web source check：通过；当前工作台源码已无 `STORY_MIN_LENGTH` / `STORY_MAX_LENGTH` / `count-badge` / `fitStoryTextForProject` / `0/2000` / `500-2000` 残留，保留 `composer-submit-button`、真实上传大小提示和减重后的 `workspace-brand` 样式。
- `git diff --check`：通过，仅输出 LF/CRLF 换行提示。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B-P6 进度卡去重与侧栏宽度控制

Date:

- 2026-05-15

Stage:

- Stage 23B-P6 follow-up UI polish。

Trigger:

- 用户指出右侧进度卡顶部摘要与第一阶段右侧重复显示“未开始”，应只保留下方阶段状态；同时左侧项目栏偏宽，需要在桌面和移动端都支持拖拽调整宽度，并参考 Codex 逻辑增加完全收缩 / 展开侧栏能力。收缩图标放在左侧栏顶部右边缘，展开按钮放在左上边缘，图标 UI 需与当前项目 UI 保持一致。

Action:

- 移除右侧进度卡顶部摘要右侧状态文案，只保留 `completed/total` 和进度条；阶段状态仍由各流程项右侧标签判断。
- 为左侧项目栏新增可拖拽宽度，桌面与移动端都启用，宽度写入 `localStorage`，默认 320px，范围 260px-420px。
- 新增左侧栏完全收缩能力：收缩后不保留图标 rail，主内容扩展；左上边缘显示展开按钮；展开后恢复上次宽度。
- 新增键盘可访问的宽度调整：拖拽柄支持左右方向键、Home 和 End；双击拖拽柄恢复默认宽度。
- 同步 README、建设方案、阶段计划、任务记录和短棒交接，继续声明 Stage 23C 主线顺延。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Web source check：通过；当前源码包含 `workspace-sidebar-resizer`、`workspace-sidebar-collapse-button`、`workspace-sidebar-expand-button`、`sidebar-collapsed` 和进度摘要去重后的 `project-progress-summary`，已无未使用的 `formatJobStatus` / `ProductionJobStatus`。
- `git diff --check`：通过，仅输出 LF/CRLF 换行提示。

Next phase:

- Stage 23C。

### 2026-05-15 Stage 23B-P7 项目搜索与设置页收尾（开始）

Date:

- 2026-05-15

Stage:

- Stage 23B-P7 follow-up UI polish。

Trigger:

- 用户确认搜索入口当前只刷新项目列表，应改为真实搜索；新项目逻辑大体保留，不需要反复点击生成多个空项目，但可以补齐切回空白草稿时的状态清理。模型配置和依赖配置需要继续保证后端对接可用，同时清理过大的弹窗感、开发态文案和与当前整体 UI 不一致的问题。

Planned action:

- 将项目搜索图标改为真实搜索入口，提供查询输入并过滤左侧历史项目列表。
- 新项目入口继续只回到空白草稿态，不立即创建数据库项目；补齐 project / job / tasks / assets / messages / selection 等 UI 状态清理，避免残留。
- 模型配置和依赖配置仍只通过 Control API / DTO 消费后端状态，不让 Web / Electron 触碰 SQLite、文件系统、FFmpeg 或 OCR。
- 清理 Provider / Dependency 设置页中的 Stage、provider、Secret Store、Spend Guard、SQLite Schema 等开发态展示文案，改为用户可理解的模型服务、本地密钥、成本保护、本地数据库、数据库结构等表达。
- 调整设置弹窗和设置页内部布局，优先保证无横向滚动、控件可用、视觉与当前工作台保持统一；深浅色切换和 XiaoLouAI 配色对齐后续改为 Stage 23B-P9 单独推进。

Next phase:

- Stage 23C 主线继续顺延；Stage 23B-P8 先处理左侧栏交互补丁，深浅色切换和整体配色对齐顺延到 Stage 23B-P9。

### 2026-05-15 Stage 23B-P7 项目搜索与设置页收尾（完成）

Date:

- 2026-05-15

Stage:

- Stage 23B-P7 follow-up UI polish。

Action:

- 项目区“搜索”图标从刷新项目列表改为真实搜索入口：展开轻量搜索框，按项目标题、描述、状态和模式过滤左侧历史项目列表，Esc 或关闭按钮可清空并退出搜索。
- “新项目”入口继续只切回空白草稿态，不立即创建数据库项目；补齐 project / job / tasks / assets / messages / selectedTask / stream / checkpoint / rollback 等 UI 状态清理，避免从旧项目残留到新草稿。
- 模型配置页清理 Stage / provider / Secret Store / Spend Guard / Sandbox 等开发态文案，改为模型服务、接口、密钥存储、成本保护和调用边界等用户侧表达；真实生成调用仍保持关闭，只允许连接测试。
- 依赖页清理 SQLite Schema / Storage / Uploads 等原始技术标签，改为本地数据库、数据库结构、存储目录、上传目录、Python 运行时和技能目录等展示文案，并映射常见英文后端消息。
- 设置弹窗维持弹窗形态但收敛宽度、改为纵向滚动、禁止横向滚动；模型 / 依赖内容改为单列主布局和弹窗内可换行的辅助面板，视觉继续跟随 Stage 23B-P4/P5/P6 的轻量化工作台风格。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Source check：通过；当前源码包含 `workspace-project-search`、`toggleProjectSearch`、`formatProviderCheckMessage` 和 `formatDependencyMessage`，模型 / 依赖设置源码中已无 `Provider 前配置`、`Secret Store`、`Spend Guard`、`SQLite Schema` 等直接展示文案。
- `git diff --check`：通过，仅输出 LF/CRLF 换行提示。

Next phase:

- Stage 23B-P8：左侧项目行搜索交互、按钮对齐、菜单外部点击取消和设置按钮 UI 残留收口。
- Stage 23B-P9：深浅色切换和 XiaoLouAI 方向的整体配色 / 视觉对齐。
- Stage 23C 主线继续顺延。

### 2026-05-15 Stage 23B-P8 左侧交互与浮层取消收尾（开始）

Date:

- 2026-05-15

Stage:

- Stage 23B-P8 follow-up UI polish。

Trigger:

- 用户指出 P7 搜索点击后的独立搜索框很丑，应改为项目标题行内展开搜索；搜索态新项目按钮需要弱化隐藏；收缩按钮、新项目按钮和搜索按钮需要对齐；项目行操作按钮参考 Codex 逻辑默认隐藏，hover / focus 项目行时显示。同时设置二级菜单和添加文件菜单当前只能原位点击关闭，需要补齐点击外部和 Escape 取消逻辑；设置按钮仍有旧 UI 残留。

Planned action:

- 将项目标题行搜索改为行内展开，复用 P7 的真实过滤逻辑。
- 项目操作按钮默认隐藏，项目行 hover / focus-within 时显示；搜索态隐藏新项目按钮，搜索按钮随输入框右移。
- 统一侧栏收缩、项目搜索和新项目按钮的右侧对齐、尺寸、透明度、hover / focus 状态。
- 为设置菜单、composer 上传菜单和工作台内类似轻浮层补齐 outside click 与 Escape 关闭；设置弹窗本身不做外部点击误关闭。
- 轻量化左下设置按钮残留样式，使其与当前侧栏项目行和图标按钮保持一致。

Next phase:

- Stage 23B-P9：深浅色切换和 XiaoLouAI 方向的整体配色 / 视觉对齐。
- Stage 23C 主线继续顺延。

### 2026-05-15 Stage 23B-P8 左侧交互与浮层取消收尾（完成）

Date:

- 2026-05-15

Stage:

- Stage 23B-P8 follow-up UI polish。

Action:

- 项目标题行搜索改为行内展开：点击搜索后“项目”文本变为搜索输入框，复用 P7 的真实项目过滤逻辑；搜索态新项目按钮弱化隐藏，搜索按钮留在右侧作为关闭 / 清空搜索入口。
- 项目行操作按钮默认隐藏，hover 或 focus 项目标题行时淡入显示，贴近 Codex 项目行的轻量交互。
- 统一侧栏收缩按钮、项目搜索按钮和新项目按钮的尺寸、右侧对齐、透明度与 hover / focus 状态。
- 为左下设置菜单和 composer 加号上传菜单补齐点击外部关闭与 Escape 关闭；设置配置弹窗保持显式关闭，避免误点丢失编辑上下文。
- 左下设置按钮从旧的大块按钮收口为轻量按钮，减少固定底部区域的粗重感。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build`：通过。
- Source check：通过；当前源码包含 `settingsMenuRef`、`uploadMenuRef`、`workspace-project-search-input`、`search-open`、`new-project-action` 和菜单 `pointerdown` / `Escape` 关闭逻辑。
- `git diff --check`：通过，仅输出 LF/CRLF 换行提示。

Next phase:

- Stage 23B-P9：深浅色切换和 XiaoLouAI 方向的整体配色 / 视觉对齐。

### 2026-05-15 Stage 23B-P8 follow-up 对齐与搜索退出修正（开始）

Scope:

- 修正左侧项目区“项目”和“暂无项目”的文本左边缘不一致问题。
- 修正左侧品牌栏 logo / “麋鹿”与收缩按钮纵向不齐的问题。
- 搜索态补齐点击项目行之外区域时自动退出，并保持现有行内搜索交互。
- 左下设置入口恢复为 Codex 式底部行视觉，重点调整“设置”文本与当前 UI 的一致性，不改变设置功能入口。

Decision:

- 本轮是 Stage 23B-P8 的 follow-up，不新开 P9；原 Stage 23B-P9 继续保留给深浅色切换和 XiaoLouAI 方向整体配色 / 视觉对齐。
- 不修改 Control API、Worker、SQLite、上传、OCR 或 FFmpeg 边界。

Status:

- 文档已先行记录，随后执行前端修正和本地构建验证。

### 2026-05-15 Stage 23B-P8 follow-up 对齐与搜索退出修正（完成）

Scope:

- 左侧项目区标题与空状态文本左边缘对齐。
- 左侧品牌栏 logo / “麋鹿”和收缩按钮纵向对齐。
- 搜索态点击项目搜索行之外任意区域会退出搜索，Escape 仍可退出。
- 左下设置入口恢复为 Codex 式底部整行按钮，保留当前设置菜单功能。

Changed:

- `apps\web\src\features\workspace\StudioWorkspacePage.tsx`
- `apps\web\src\styles.css`

Verification:

- `npm run build` passed in `apps\web`。

Design check:

- cohesion: 本轮只调整工作台侧栏交互和侧栏样式，继续沿用当前 Codex 式工作台 UI token。
- coupling: 搜索态退出只在前端状态层处理，不影响项目 API、上传 API 或设置 API。
- boundaries: UI 仍不直接访问 SQLite、文件系统、FFmpeg、OCR 或模型 SDK。
- temporary debt: P9 继续保留给深浅色切换和 XiaoLouAI 方向整体配色 / 视觉对齐。

Environment check:

- project-local files: 改动仅限项目内 Web 源码和文档。
- D drive only: 构建和文档同步均在 `D:\code\MiLuStudio` 内完成。
- C drive risk: 未新增 C 盘路径或用户目录依赖。
- Stage 23C 主线继续顺延。

### 2026-05-15 Stage 23B-P8 follow-up 设置入口与侧栏动效修正（开始）

Scope:

- 继续修正左下设置入口文本仍偏重、偏亮的问题，使其更接近 Codex 式底部行视觉。
- 为左侧项目栏收缩 / 展开补齐平滑过渡，避免当前 `width: 0` / `visibility: hidden` 硬切。

Decision:

- 本轮仍归入 Stage 23B-P8 follow-up，不新开 P9；P9 继续保留给深浅色切换和 XiaoLouAI 方向整体配色 / 视觉对齐。
- 只改 Web 前端样式，不改 Control API、Worker、SQLite、上传、OCR、FFmpeg 或模型边界。

Status:

- 文档已先行记录，随后执行侧栏样式与动效修正。

### 2026-05-15 Stage 23B-P8 follow-up 设置入口与侧栏动效修正（完成）

Scope:

- 左下设置入口文字和图标继续减重，避免底部入口文本仍像旧 UI。
- 左侧项目栏收缩 / 展开加入平滑动效，减少硬切。

Changed:

- `apps\web\src\styles.css`

Verification:

- `npm run build` passed in `apps\web`。

Design check:

- cohesion: 继续沿用当前 Codex 式工作台 token，仅调整侧栏底部入口和侧栏显隐动效。
- coupling: 改动只在 CSS 样式层，不影响搜索、设置菜单、项目状态或后端 API。
- boundaries: UI 仍不直接访问 SQLite、文件系统、FFmpeg、OCR 或模型 SDK。
- temporary debt: P9 继续保留给深浅色切换和 XiaoLouAI 方向整体配色 / 视觉对齐。

Environment check:

- project-local files: 改动仅限项目内 Web 样式和文档。
- D drive only: 构建和文档同步均在 `D:\code\MiLuStudio` 内完成。
- C drive risk: 未新增 C 盘路径或用户目录依赖。

### 2026-05-15 Stage 23B-P9 主题切换与 XiaoLouAI 方向轻度对齐（开始）

Scope:

- 新增 MiLuStudio Web 主题地基：本地存储、启动初始化和 `html` 主题标记。
- 保留当前深色作为默认体验，新增浅色 token，避免破坏已确认的 Codex 式工作台。
- 在左下设置上方添加“切换到浅色 / 切换到深色”入口，使用轻量图标和当前侧栏按钮风格。
- 清理影响浅色主题的少量硬编码深色 / 状态色，转为 CSS token。
- 只做 XiaoLouAI 方向的轻度配色对齐：浅色白 / 浅灰 / 蓝紫，深色保持当前工作台气质并略收强调色。

Decision:

- 不引入 XiaoLouAI 的 Tailwind 主题体系，不复制 XiaoLouAI 的整套导航 / 首页 / 账户卡结构。
- 不改右侧进度卡结构，不触碰 Control API、Worker、SQLite、上传、OCR、FFmpeg 或模型边界。

Status:

- 文档已先行记录，随后执行 Web 主题补丁和构建验证。

### 2026-05-15 Stage 23B-P9 主题切换与 XiaoLouAI 方向轻度对齐（完成）

Implemented:

- 新增 `apps\web\src\shared\theme.ts`，封装 `milu.theme` 本地存储、`html[data-theme]` 初始化、跨组件主题同步和 `useTheme()`。
- `apps\web\src\main.tsx` 在 React render 前初始化主题，减少主题切换入口出现前的默认主题漂移。
- `StudioWorkspacePage` 在左下设置上方新增主题切换入口，深色默认显示“切换到浅色”，浅色显示“切换到深色”；切换时关闭设置菜单和上传菜单，不触碰后端。
- `styles.css` 将主题 token 拆成默认深色和浅色两组：深色保持当前 Codex 式工作台观感，浅色使用白 / 浅灰 / 蓝紫 restrained token 对齐 XiaoLouAI 方向。
- 清理硬编码深色阴影、进度轨道、流程激活色和 provider 测试状态色，改为 CSS token；modal、composer、设置浮层、展开按钮和右侧进度卡均随主题切换。

Verification:

- `npm run build` in `apps\web` passed.
- `git diff --check` passed with only existing LF -> CRLF working-copy warnings.

Boundaries:

- 未引入 Tailwind 主题体系，未复制 XiaoLouAI 源码或视觉资产。
- 未修改 Control API、Worker、SQLite、上传、OCR、FFmpeg 或模型调用边界。
- UI / Electron 仍不得直接访问 SQLite、业务文件系统、FFmpeg、OCR 或真实模型 SDK。

Next phase:

- Stage 23C：Tesseract-compatible OCR runtime 固化和正向验证、PDF rasterizer、DOC/PDF 深度解析、工作台详情展示、生产链路实际消费 asset analysis endpoint 和媒体派生策略回归。

### 2026-05-15 Stage 23C 前置检查与交接口径修正

Check:

- 扫查 README、建设方案、阶段计划、任务记录和短棒交接中的当前阶段描述、`Stage 23B-P9` 后续、`P10`、`正在推进`、`正在做`、`如继续 UI 补丁` 和下一棒提示词。
- 未发现 Stage 23B-P9 之后仍需要在 Stage 23C 前追加的补丁任务。
- 仍出现的“顺延”多数是历史任务记录中的当时状态；当前交接口径已改为下一棒直接进入正式 Stage 23C。

Updated:

- README 下一阶段说明改为：当前未发现 Stage 23C 前仍需追加的补丁任务，下一棒可直接进入正式功能阶段。
- 阶段计划 `Current phase` 仍为 Stage 23C，状态改为 `ready_to_start`。
- 建设方案、任务记录和短棒交接同步去除当前口径中的“继续顺延”表达。

Next phase:

- Stage 23C：固化 Tesseract-compatible OCR runtime 安装和正向 OCR 验证；继续 PDF rasterizer、DOC/PDF 深度解析、工作台详情展示、生产链路实际消费 asset analysis endpoint 和媒体派生策略回归。

### 2026-05-15 Stage 23C-P0 分片上传 UI 与资产解析详情消费（完成）

Implemented:

- `apps\web\src\shared\api\controlPlaneClient.ts` 的 `uploadProjectAsset` 已改为只通过 Control API resumable upload 合同：create session -> PUT chunks -> complete，不再从工作台走旧 multipart 上传入口。
- 浏览器侧按后端返回的 `chunkSize` 切片上传，并在支持 `crypto.subtle` 时为每个 chunk 计算 SHA-256，写入 `X-MiLuStudio-Chunk-Sha256` 供后端校验。
- `apps\web\src\shared\types\production.ts` 新增 upload session / chunk / complete DTO，以及 `ProjectAssetAnalysisResponse` 等解析详情 DTO。
- 工作台生成结果里的资产行不再只提示本地路径，点击后会调用 `GET /api/projects/{projectId}/assets/{assetId}/analysis`，展示基础信息、边界状态、文本切片、OCR summary、派生资产和结构化解析 JSON。
- 撤掉此前暂停时留下的未接入 agent C# 骨架，避免 SDK 自动编译未完成文件导致当前 Stage 23C 主线 build 失败；agent skill / memory 后续作为独立阶段重新接入。

Verification:

- `D:\soft\program\nodejs\npm.ps1 run build` in `apps\web` passed。
- `powershell -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1` passed：.NET build 0 warning / 0 error，并验证 session 创建、乱序 chunk、resume status、complete、asset analysis endpoint 和 no-provider 边界。
- `git diff --check -- apps/web/src/shared/api/controlPlaneClient.ts apps/web/src/shared/types/production.ts apps/web/src/features/workspace/StudioWorkspacePage.tsx apps/web/src/styles.css` passed，仅有现有 LF -> CRLF working-copy 提示。

Boundaries:

- UI / Electron 仍不直接访问 SQLite、业务文件系统、FFmpeg、OCR 或模型 SDK。
- 分片合并、资产保存、解析、OCR / FFmpeg 调用边界仍在 Control API / backend adapter。
- 本次不推进 agent 记忆与 skill 配置，只清理 build-breaking 半成品，保留后续阶段方向。

Next:

- Stage 23C 继续推进 Tesseract-compatible runtime 固化与正向 OCR 验证、PDF rasterizer、DOC/PDF 深度解析，以及生产链路实际消费 asset analysis metadata。

### 2026-05-15 Stage 23C-P1 Tesseract Runtime 固化入口与验证闭环（完成）

Start docs:

- 开工前已阅读 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md`、`docs\MILUSTUDIO_TASK_RECORD.md` 和 `docs\MILUSTUDIO_HANDOFF.md`，确认当前进入 Stage 23C，且 agent skill / memory 大阶段放在当前上传解析主线之后。

Implemented:

- 新增 `scripts\windows\Install-MiLuStudioTesseract.ps1`：支持 `-PackagePath` 离线目录/ZIP 导入，支持用户显式传 `-DownloadUrl` 的辅助下载；`.exe` installer 必须额外传 `-AllowInstaller`；安装写入限制在 `D:\code\MiLuStudio\runtime\tesseract`，并生成 runtime manifest。
- 新增 `scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1`：runtime 缺失时给出可恢复提示；runtime 存在时调用 Stage 23B asset parsing 的 `-RequireOcrRuntime` 强制跑正向图片 OCR。
- `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1` 新增 `-RequireOcrRuntime`，并在临时 Control API 中显式设置 `ControlPlane__OcrTesseractPath` / `ControlPlane__OcrTessdataPath`。
- `ControlPlaneOptions.OcrTesseractPath` 默认指向项目内 `runtime\tesseract\tesseract.exe`；`Set-MiLuStudioEnv.ps1` 同步设置 OCR executable path，并仅在 tessdata 目录存在时设置 tessdata path。
- SQLite / InMemory preflight 均补齐 OCR runtime 细节：`tesseractPath`、`tessdataPath`、`tessdataAvailable`、语言候选和安装脚本；依赖中心前端补充对应详情标签和 OCR 修复建议文案。

Verification:

- PowerShell parser check passed for `Install-MiLuStudioTesseract.ps1`、`Test-MiLuStudioStage23COcrRuntime.ps1`、`Test-MiLuStudioStage23BAssetParsing.ps1` 和 `Set-MiLuStudioEnv.ps1`。
- `D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore` passed，0 warnings / 0 errors。
- `D:\soft\program\nodejs\npm.ps1 run build` in `apps\web` passed。
- `powershell -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1` passed the current no-runtime branch：确认 `runtime\tesseract` 缺失时给出导入提示并跳过正向 OCR。
- `powershell -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1` passed，确认无 Tesseract 时仍保持结构化降级。
- `git diff --check` passed with only existing LF -> CRLF working-copy warnings。

Boundaries:

- 未直接安装外部 OCR 程序；在线 URL 只作为用户显式触发的辅助路径，离线目录/ZIP 导入仍是主路径。
- UI / Electron 仍不直接执行 OCR、不读取业务文件系统、不扫描 `runtime`；依赖状态只通过 Control API preflight / `/api/system/dependencies` 展示。
- 当前本机仍未安装可控 `runtime\tesseract`，因此正向 OCR 尚未在本机完成；导入包含 `eng.traineddata` 的 runtime 后需运行 `Test-MiLuStudioStage23COcrRuntime.ps1 -RequireRuntime`。

Next:

- 导入可控 Tesseract-compatible runtime 离线包后跑 Stage 23C 正向 OCR 验证。
- 继续 Stage 23C：PDF rasterizer / 扫描页 OCR、DOC/PDF 更深解析、生产链路实际消费 analysis metadata 和媒体派生策略回归。

### 2026-05-15 Stage 23C-P2 生产链路消费上传资产解析文本（完成）

Start docs:

- 开工前已重新阅读 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md`、`docs\MILUSTUDIO_TASK_RECORD.md` 和 `docs\MILUSTUDIO_HANDOFF.md`。
- 当前本机仍无 `runtime\tesseract`，Stage 23C 正向 OCR 需先导入可控 runtime；因此本轮先推进不依赖新增外部包的“生产链路实际消费 analysis metadata”闭环。

Implemented:

- `ProjectAssetUploadService` 在资产 `metadata_json` 中新增 `productionInput`：当上传结果为可用 `story_text` 时记录 `storyTextCandidate`、候选状态和 no-provider / no-generation-payload 边界。
- `ProductionSkillExecutionService` 注入 `IAssetRepository`，构造 `story_intake` payload 时优先读取当前项目最新 `story_text` 资产的 `productionInput.storyTextCandidate`，无可用候选时再回退 `story_inputs.original_text`。
- 保留兼容扩展：若后续 chunk manifest 开始记录 chunk 正文，Worker 也可从 `chunkManifest.chunks[].content/text` 组合故事正文；当前 Stage 23B manifest 仍以摘要 / preview 为主。
- 新增 `scripts\windows\Test-MiLuStudioStage23CAssetConsumption.ps1`，使用临时 SQLite Control API + Worker 创建项目、上传带 marker 的故事文本资产、启动 production job，并断言 `story_intake.inputJson.story_text` 来自上传资产而不是项目 fallback 正文。

Verification:

- PowerShell parser check passed for `Test-MiLuStudioStage23CAssetConsumption.ps1`。
- `D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore` passed，0 warnings / 0 errors。
- `powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23CAssetConsumption.ps1 -ProjectRoot D:\code\MiLuStudio` passed。
- `powershell -NoProfile -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1 -ProjectRoot D:\code\MiLuStudio` passed，确认新增 metadata 字段未破坏 Stage 23B analysis endpoint / no-provider 边界。

Boundaries:

- 未接真实模型生成，未发送 provider payload。
- UI / Electron 仍不直接读取上传文件、SQLite、FFmpeg、OCR 或业务文件系统；生产输入选择发生在后端 Worker / Application 边界。
- 本轮未导入 Tesseract runtime，正向 OCR 仍待 `runtime\tesseract` 可控包到位后执行。

Next:

- 导入可控 Tesseract-compatible runtime 后运行 `scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1 -RequireRuntime`。
- 继续 Stage 23C：PDF rasterizer / 扫描页 OCR、DOC/PDF 更深解析和媒体派生策略回归。

### 2026-05-15 Stage 23C-P3 PDF rasterizer 后端路径与可恢复验证（完成）

Start docs:

- 开工前已重新阅读 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md`、`docs\MILUSTUDIO_TASK_RECORD.md` 和 `docs\MILUSTUDIO_HANDOFF.md`。
- 当前本机仍无 `runtime\tesseract`，也无 `runtime\poppler` / `pdftoppm.exe`；因此本轮先把 PDF rasterizer 和扫描页 OCR 消费路径接入后端 adapter，并验证缺 runtime 可恢复分支。

Implemented:

- `ControlPlaneOptions` 新增 `PdfRasterizerPath`、`PdfRasterizerDpi` 和 `PdfRasterizerPageLimit`，默认优先查找 `D:\code\MiLuStudio\runtime\poppler\Library\bin\pdftoppm.exe`，并保留项目 `runtime\poppler\bin` 与 `D:\tools\poppler` fallback。
- `FfmpegAssetTechnicalAnalyzer` 在 PDF 无 embedded text 时会探测 Poppler `pdftoppm` 与 Tesseract runtime；两者可用时在后端 adapter 内 rasterize 前几页，再复用 OCR wrapper 生成 `pdf_raster_ocr` chunk manifest。
- 缺 Poppler、缺 Tesseract、rasterize 失败或 OCR 无文本时均写入结构化 `pdfRasterizer` / `ocr` metadata 和 unavailable chunk manifest，不让上传失败成裸异常。
- SQLite / InMemory preflight 新增 `pdf_rasterizer_runtime` 检测，返回 `pdftoppmPath`、DPI、页数上限和安装脚本；`Set-MiLuStudioEnv.ps1` 同步设置 `ControlPlane__PdfRasterizerPath`。
- 新增 `scripts\windows\Install-MiLuStudioPdfRasterizer.ps1`：支持离线目录/ZIP 导入、用户显式 `-DownloadUrl` 辅助下载、SHA256 校验和 manifest 写入，所有写入限制在项目 `runtime\poppler` 内。
- 新增 `scripts\windows\Test-MiLuStudioStage23CPdfRasterizer.ps1`：当前无 runtime 时给出导入提示并跳过正向扫描 PDF 验证；导入 Poppler + Tesseract 后可用 `-RequireRuntime` 阻断式验证。

Verification:

- PowerShell parser check passed for `Install-MiLuStudioPdfRasterizer.ps1` 和 `Test-MiLuStudioStage23CPdfRasterizer.ps1`。
- `D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore` passed，0 warnings / 0 errors。
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23CPdfRasterizer.ps1` passed the current no-runtime branch：确认 `runtime\poppler` 缺失时给出导入提示并跳过正向扫描 PDF 验证。
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1` passed，确认 PDF embedded text、DOC 降级、OCR 缺 runtime 降级、media derivatives 和 analysis endpoint 未回归。
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23CAssetConsumption.ps1` passed，确认 Worker 仍优先消费上传故事文本资产解析结果。
- `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1 -SkipBuild` passed the current no-runtime branch。
- `git diff --check` passed with only existing LF -> CRLF working-copy warnings。

Boundaries:

- 未直接下载或安装外部 Poppler / Tesseract 程序；在线 URL 只作为用户显式触发的辅助路径，离线目录/ZIP 导入仍是主路径。
- UI / Electron 仍不直接读取 PDF、执行 OCR、执行 `pdftoppm`、扫描 runtime 或访问业务文件系统；PDF rasterizer、OCR、FFmpeg 和依赖检测都停留在后端 adapter / Control API 边界内。
- 当前本机仍未安装可控 `runtime\tesseract` 和 `runtime\poppler`，因此扫描 PDF OCR 正向验证仍待 runtime 包到位后执行。

Next:

- 导入可控 Tesseract-compatible runtime 后运行 `scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1 -RequireRuntime`。
- 导入可控 Poppler runtime 后运行 `scripts\windows\Test-MiLuStudioStage23CPdfRasterizer.ps1 -RequireRuntime`。
- 继续 Stage 23C：DOC/PDF 深度解析、扫描 PDF 正向 fixture、媒体派生策略回归和参考图 / 视频派生策略消费扩展。

### 2026-05-15 Stage 23C-P4 DOCX 结构化解析与 analysis 暴露（完成）

Start docs:

- 开工前已重新阅读 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md`、`docs\MILUSTUDIO_TASK_RECORD.md` 和 `docs\MILUSTUDIO_HANDOFF.md`。
- 已检查项目内和 `D:\tools`，当前仍无 `runtime\tesseract`、`runtime\poppler`、`tesseract.exe`、`pdftoppm.exe` 或可导入 runtime 包，因此本轮先推进不依赖外部 runtime 的 DOCX 深度解析。

Implemented:

- `FfmpegAssetTechnicalAnalyzer` 的 DOCX 解析从简单抽取 `<w:t>` 升级为 Stage 23C 结构化 ZIP/XML reader。
- DOCX 解析现在会记录正文段落、表格、页眉、页脚、脚注、尾注和批注 block，并生成 `technical.documentStructure`，包含 source parts、block counts、table cell counts、preview blocks、warnings 和 no-provider / no-UI-file-boundary 字段。
- DOCX 文本切片 source type 更新为 `docx_structured`，仍只在后端 adapter 读取上传文件，不让 UI / Electron 触碰文件系统。
- `ProjectAssetAnalysisResponse` 新增 `documentStructure`，`ProjectAssetAnalysisService` 从 `assets.metadata_json` 暴露该结构；工作台资产详情 JSON 同步显示 `documentStructure`。
- `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1` 的 DOCX fixture 增加 heading、table、header 和 footnote，并断言 metadata / analysis endpoint 中的结构字段。

Verification:

- `D:\soft\program\dotnet\dotnet.exe build .\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore` passed，0 warnings / 0 errors。
- `npm run build` in `apps\web` passed。
- `powershell -ExecutionPolicy Bypass -File .\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1` passed，确认 DOCX structure metadata、analysis endpoint、PDF embedded text、DOC 降级、OCR 缺 runtime 降级和媒体派生未回归。
- `powershell -ExecutionPolicy Bypass -File .\scripts\windows\Test-MiLuStudioStage23CAssetConsumption.ps1` passed，确认 Worker 仍优先消费上传故事文本资产解析结果。
- `powershell -ExecutionPolicy Bypass -File .\scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1` passed current no-runtime branch，确认缺 Tesseract 时提示导入并跳过正向 OCR。
- `powershell -ExecutionPolicy Bypass -File .\scripts\windows\Test-MiLuStudioStage23CPdfRasterizer.ps1` passed current no-runtime branch，确认缺 Poppler 时提示导入并跳过正向扫描 PDF 验证。

Design check:

- cohesion: DOCX 结构化读取集中在现有 asset technical analyzer 后端 adapter，analysis response 只负责暴露已有 metadata，不新增跨层文件读取。
- coupling: 前端只新增 DTO 字段和详情 JSON 展示，不理解 DOCX 文件结构；Worker / provider / SQLite schema 不受 DOCX reader 细节影响。
- boundaries: UI / Electron 仍不读取上传文件、不执行 OCR、不执行 PDF rasterizer、不访问 SQLite 或业务文件系统；DOCX 解析发生在 Control API / Infrastructure adapter 边界。
- temporary debt: DOCX reader 仍是轻量 OpenXML ZIP/XML reader，未接完整 Office converter；复杂 DOC、复杂 PDF 和扫描 PDF 正向 OCR 仍依赖后续可控 runtime。

Environment check:

- project-local files: 改动限制在项目源码、脚本和文档内；临时测试文件落在 `.tmp`。
- D drive only: 所有验证在 `D:\code\MiLuStudio` 内执行，runtime 查找只覆盖项目 `runtime` 和 `D:\tools`。
- C drive risk: 未新增 C 盘路径、全局安装或用户目录依赖。

Next:

- 导入可控 Tesseract-compatible runtime 后运行 `scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1 -RequireRuntime`。
- 导入可控 Poppler runtime 后运行 `scripts\windows\Test-MiLuStudioStage23CPdfRasterizer.ps1 -RequireRuntime`。
- 继续 Stage 23C：扫描 PDF 正向 fixture、复杂 PDF 解析补强、媒体派生策略回归和参考图 / 视频派生策略消费扩展。

### 2026-05-15 Stage 23C-P5 PDF Flate embedded text probe 与 runtime 限制解除（完成）

Start docs:

- 开工前已重新阅读 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md`、`docs\MILUSTUDIO_TASK_RECORD.md` 和 `docs\MILUSTUDIO_HANDOFF.md`。
- 用户最新确认：解除 runtime 限制，后续再补充外部需要但缺失的离线包；因此本轮不再让 `runtime\tesseract` / `runtime\poppler` 缺失阻塞 Stage 23C 功能推进。

Implemented:

- `FfmpegAssetTechnicalAnalyzer` 的 PDF parser engine 升级为 `stage23c_pdf_embedded_text_probe`。
- PDF embedded text probe 除原有明文 `Tj/TJ` 探测外，会在后端 adapter 内枚举 PDF stream，针对简单 `/Filter /FlateDecode` 内容尝试 zlib / deflate 解码，再复用现有文字提取逻辑生成 content blocks 和 chunk manifest。
- Parser metadata 新增 `streamCount`、`decodedStreamCount` 和 `failedStreamCount`，让 asset analysis 详情能看到 PDF stream 探测结果。
- `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1` 新增 Stage 23C 压缩 PDF fixture，验证无需 Poppler / OCR runtime 也能解析 Flate stream 里的文本 marker。

Verification:

- `D:\soft\program\dotnet\dotnet.exe build .\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore` passed，0 warnings / 0 errors。
- PowerShell parser check passed for `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1`。
- `powershell -ExecutionPolicy Bypass -File .\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1` passed，确认 PDF Flate stream 文本、parser stream 统计、chunk manifest、analysis endpoint 和 no-provider 边界。
- `powershell -ExecutionPolicy Bypass -File .\scripts\windows\Test-MiLuStudioStage23CAssetConsumption.ps1` passed，确认 Worker 仍优先消费上传故事文本资产解析结果。
- `powershell -ExecutionPolicy Bypass -File .\scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1` passed current no-runtime branch。
- `powershell -ExecutionPolicy Bypass -File .\scripts\windows\Test-MiLuStudioStage23CPdfRasterizer.ps1` passed current no-runtime branch。
- `npm run build` in `apps\web` passed。

Design check:

- cohesion: PDF Flate 解码集中在现有 asset technical analyzer 后端 adapter，属于上传资产技术解析职责。
- coupling: 测试脚本只扩展 PDF fixture 与断言；UI / Electron / Worker / provider 不理解 PDF stream 解码细节。
- boundaries: UI / Electron 仍不读取 PDF、不执行 OCR、不执行 PDF rasterizer、不访问 SQLite 或业务文件系统；PDF stream probe 发生在 Control API / Infrastructure adapter 边界。
- temporary debt: 当前只覆盖简单 Flate 压缩 stream；复杂编码、字体 ToUnicode map、扫描 PDF OCR 正向验证仍待后续 runtime / 离线包和更深解析阶段。

Environment check:

- project-local files: 改动限制在项目源码、脚本和文档内；测试 fixture 由脚本在项目 `.tmp` 下生成。
- D drive only: 所有验证在 `D:\code\MiLuStudio` 内执行；没有引入 C 盘或全局安装路径。
- C drive risk: 未新增 C 盘路径、全局安装或用户目录依赖。

Next:

- 继续 Stage 23C：复杂 PDF 解析补强、扫描 PDF 正向 fixture 预留、媒体派生策略回归和参考图 / 视频派生策略消费扩展。
- 外部 Tesseract / Poppler 离线包补齐后，再运行 `scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1 -RequireRuntime` 与 `scripts\windows\Test-MiLuStudioStage23CPdfRasterizer.ps1 -RequireRuntime`。

### 2026-05-15 Stage 23C-P6 参考图 / 视频 asset analysis 生产链路消费（完成）

Start docs:

- 开工前已重新阅读 `README.md`、`docs\MILUSTUDIO_BUILD_PLAN.md`、`docs\MILUSTUDIO_PHASE_PLAN.md`、`docs\MILUSTUDIO_TASK_RECORD.md` 和 `docs\MILUSTUDIO_HANDOFF.md`。
- 用户最新确认：解除 runtime 限制，外部 OCR / PDF runtime 离线包后续补齐；因此本轮继续推进不依赖 `runtime\tesseract` / `runtime\poppler` 的参考图 / 视频 asset analysis 消费扩展。

Implemented:

- `ProjectAssetUploadService` 的 `productionInput` 已扩展参考图 / 视频候选摘要：记录 reference candidate、派生摘要、OCR / probe / frame extraction 摘要、backend-adapter-only access policy、no-provider 和 no-generation-payload 边界。
- `ProductionSkillExecutionService` 在构造 `image_prompt_builder` 与 `video_prompt_builder` 输入时注入 `asset_analysis`，聚合当前项目上传的 `image_reference` / `video_reference` 资产，只传 asset id、mime、hash、探测摘要、派生类型和边界标记，不暴露本地路径。
- Python `image_prompt_builder` / `video_prompt_builder` 的 input schema、validator 和 executor 已支持可选 `asset_analysis`。
- `image_prompt_builder` 输出 `reference_strategy.uploaded_reference_summary`，`video_prompt_builder` 输出 `source_asset_manifest.uploaded_reference_summary`；两者仍保持 `provider=none`、不写文件、不写数据库、不执行 FFmpeg。
- `tests\test_stage8_image_pipeline.py` 和 `tests\test_stage9_video_pipeline.py` 已覆盖 reference summary。
- `scripts\windows\Test-MiLuStudioStage23CAssetConsumption.ps1` 已扩展：除验证 `story_intake` 消费上传故事文本，还上传参考图并断言 `image_prompt_builder.inputJson.asset_analysis` 包含该 asset id 和 backend-adapter-only 边界。
- 已运行 `apps\desktop` 的 `npm run prepare:runtime`，把更新后的 Python skill 副本同步进 Desktop runtime。

Verification:

- `D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore` passed，0 warnings / 0 errors。
- `D:\soft\program\Python\Python313\python.exe -m compileall -q milu_studio_skills skills tests` passed。
- `D:\soft\program\Python\Python313\python.exe -m unittest tests.test_stage8_image_pipeline tests.test_stage9_video_pipeline -v` passed，6 tests。
- `D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -v` passed，30 tests。
- `powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23CAssetConsumption.ps1` passed，确认 Worker 输入中包含参考图 asset analysis。
- `D:\soft\program\nodejs\npm.ps1 run prepare:runtime` in `apps\desktop` passed，并顺带完成 Web build 与 Release API / Worker publish。
- `git diff --check` passed，仅有现有 LF -> CRLF working-copy warnings。

Design check:

- cohesion: 资产候选摘要生成留在上传 / asset metadata 和生产输入构造边界，Python skill 只消费稳定 JSON 摘要。
- coupling: prompt-builder 不理解本地上传目录、FFmpeg 输出路径、OCR runtime 或文件系统；后续 provider adapter 可从同一 asset analysis 摘要扩展。
- boundaries: UI / Electron 不读取媒体、不执行 FFmpeg / OCR / PDF rasterizer、不访问 SQLite 或业务文件系统；Python skill 不读取真实媒体、不调用 provider。
- temporary debt: 当前只把参考图 / 视频 metadata 接入 prompt builder；真实 provider adapter 对参考图二进制、图生视频源素材和媒体派生文件的安全取用仍留给后续 provider / storage adapter 阶段。

Environment check:

- project-local files: 改动限制在项目源码、脚本、Desktop runtime 生成副本和文档内；验证临时文件落在 `.tmp`。
- D drive only: 验证使用 D 盘 .NET、Python 和 Node；runtime / uploads / sqlite 临时文件均在 `D:\code\MiLuStudio`。
- C drive risk: 未新增 C 盘路径、全局安装或用户目录依赖。

Next:

- 继续 Stage 23C：复杂 PDF 解析补强、扫描 PDF 正向 fixture 预留和媒体派生策略回归。
- 外部 Tesseract / Poppler 离线包补齐后，再运行 `scripts\windows\Test-MiLuStudioStage23COcrRuntime.ps1 -RequireRuntime` 与 `scripts\windows\Test-MiLuStudioStage23CPdfRasterizer.ps1 -RequireRuntime`。
