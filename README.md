# MiLuStudio

MiLuStudio 是一个 Windows 原生 AI 漫剧生产 Agent 项目。项目面向普通创作者，目标是让用户输入中文故事、小说片段或创作要求后，通过可审阅的生产链路生成脚本、角色设定、画风、分镜、图片提示词、mock 图片资产、视频提示词、mock 视频片段、配音任务、SRT-ready 字幕、粗剪计划、质量报告和最终导出包。

当前仓库采用前后端、Worker、Python Production Skills 和桌面端分离的工程结构。Stage 15 已把 Electron 桌面宿主作为独立交付壳接入：桌面端只负责承载 Web UI、启动本地 Control API / Worker、注入 Control API base URL、展示 preflight 和生成 Windows 安装包，不绑定数据库 schema、migration 或业务文件系统。Stage 16 已补齐本地 deterministic 账号注册、登录、会话和设备绑定；许可证、激活码和付费码相关体验已从当前网页端与桌面端撤下，作为后续商业化大后期内容保留边界。Stage 17 已完成生产控制台可编辑能力。Stage 18 已完成真实 provider adapter 前配置页。Stage 19 已完成桌面发布验收与代码签名前置准备。Stage 20 已把 Web 主入口重构为 Codex 式工作台：左侧历史项目、中央单输入框、右侧进度与结果预览卡、阶段门禁加号上传菜单，以及左下角统一设置入口。Stage 21 已完成新工作台结构化产物编辑增强。Stage 22 已完成 provider adapter 安全前置层：metadata-only secret store、spend guard 和 provider sandbox 均已占位落地，真实 provider 调用仍被阻断。

> 说明：当前项目处于 MVP 工程搭建阶段。Stage 0 到 Stage 22 已完成，已可通过 Electron + electron-builder + NSIS 生成 Windows 桌面安装包，在应用内通过 Control API 完成登录、注册、会话和设备绑定，并在 Codex 式工作台中提交剧本、跟踪生产进度、预览结果、编辑真实分镜产物，以及编辑角色 / 画风 / 图片提示词 / 视频提示词结构化产物。Web 已有 provider 前配置页和 Stage 22 安全前置状态，桌面发布验收脚本会识别当前安装器和主程序仍为 `NotSigned`；真实模型 provider、真实图片 / 视频 / 音频生成、真实媒体质检和 FFmpeg 成片尚未接入。现有 Skills 只输出可审阅 JSON envelope，不生成真实媒体文件。

## 项目功能

- 对中文故事或小说片段做结构化解析。
- 生成短剧改编大纲、集脚本、旁白、对白和字幕候选。
- 生成角色设定、角色音色规划和画风规则。
- 生成可审阅分镜，包含影片概览、分部信息、镜头时长、环境描写、时间切片、景别、运镜、音效和背景音乐等中文分镜稿字段。
- 生成图像提示词和 mock 图片资产结构。
- 生成视频提示词和 mock 视频片段结构。
- 生成配音任务、SRT-ready 字幕结构和粗剪 timeline / render plan。
- 生成质量问题报告、严重级别、可自动重试项和人工确认 checkpoint。
- 通过 Control API 暴露项目、生产任务、暂停、恢复、重试、checkpoint 和 SSE 进度边界。
- 生产控制台点击开始生成或重新生成时，会先保存当前输入，再淘汰同项目未完成旧 job 并创建新 job，确保 Worker 消费的是当前剧本内容。
- 生产控制台已把内部 skill / stage / status 默认转成中文业务文案，并为 checkpoint 补“本步产出、检查项、通过后、拒绝后”和真实产物预览。
- `storyboard_director` 仍保留下游可消费的 `shots` JSON，同时新增 `film_overview`、`storyboard_parts`、`rendered_markdown` 和 `validation_report`，先对齐专业分镜 MD 的展示结构。
- 生产控制台分镜结果卡已支持编辑镜头时长、场景、画面动作、景别、镜头运动、声音、对白和旁白；保存或单镜头重算都只通过 Control API 写回 `storyboard_director` envelope，并重置下游任务等待审核后重算。
- Web 主入口已重构为 Codex 式工作台：左侧展示历史项目，中央新项目默认只有一个剧本 / 要求输入框；左下角加号上传菜单按当前生产阶段启用文本 / 图片 / 视频入口，媒体附件只记录文件名、类型和大小，不读取真实媒体内容；右侧显示当前进度、结果预览和打开入口。
- 模型配置、桌面诊断和账户退出已统一收束到左下角设置入口；前端仍只通过 Control API client 访问项目、任务、provider settings 和分镜编辑 API。
- “模型”设置页已接入 Stage 18 provider 前配置：Text / Image / Video / Audio / Edit adapter 的供应商、默认模型、启用开关、API Key 占位、单项目成本上限、失败重试次数和本地 preflight。
- Stage 19 已新增桌面发布验收脚本，覆盖安装包、`win-unpacked`、运行时资源、快捷方式 / 自启动脚本、Electron 安全边界、签名状态和桌面模式 Control API 边界。
- 已通过 Control API 管理账号、会话和设备绑定，网页端与桌面端当前不展示许可证或激活码流程。
- 已为 PostgreSQL 持久化、真实媒体质检 adapter、真实 provider adapter、桌面安装包和后续商业授权 adapter 预留清晰边界。
- 中文测试剧本 fixture 已放在 `docs\test-fixtures\scripts`，包含 Project Gutenberg《牡丹亭》完整文本和当前输入框可用的 500 字测试样本。

## 技术栈

| 模块 | 技术 |
| --- | --- |
| 前端 | React、Vite、TypeScript、CSS、lucide-react |
| Control API | .NET 8、ASP.NET Core、Minimal API |
| 应用层 | ProjectService、ProductionJobService、TaskQueueService、AuthLicensingService |
| Worker | .NET BackgroundService 边界 |
| 当前存储 | PostgreSQL 默认，InMemoryControlPlaneStore 作为快速 smoke 备选 |
| PostgreSQL | EF Core DbContext、Npgsql、SQL migration runner、preflight |
| Production Skills | Python、统一 CLI、SkillGateway、JSON envelope |
| 桌面端 | Electron、electron-builder、NSIS assisted installer、自定义 `installer.nsh` |
| 开发环境 | Windows、PowerShell、D 盘封闭依赖和缓存 |

## 系统架构

```mermaid
flowchart LR
    User["用户"] --> Web["apps/web\nReact + Vite"]
    User --> Desktop["apps/desktop\nElectron + NSIS"]
    Desktop --> Web
    Web --> Api["Control API\nASP.NET Core"]
    Api --> Store["InMemory / PostgreSQL\nRepositoryProvider 配置切换"]
    Api --> Worker["Windows Worker\n任务编排边界"]
    Worker --> Skills["Python Skills Runtime\nSkillGateway + CLI"]
    Skills --> Envelopes["可审阅 JSON envelopes"]
    Envelopes --> Future["后续 provider / storage / export adapters"]
```

架构原则：

- UI 只通过 Control API 和 DTO 通信。
- UI 不直接访问数据库、文件系统、Python 脚本、模型 SDK 或 FFmpeg。
- Python Skills 只负责内部生产能力，输入 JSON，输出 JSON envelope。
- 数据库属于后端基础设施，先在 Control API / Worker / Infrastructure 内完成。
- Electron 只做桌面宿主、安装器和本地进程管理，不定义数据库表，不执行 migrations。

## 目录结构

```text
MiLuStudio/
├── apps/
│   ├── web/                         # React + Vite 前端壳
│   └── desktop/                     # Electron 桌面宿主和 NSIS 打包配置
├── backend/
│   ├── control-plane/               # .NET API / Application / Domain / Infrastructure / Worker
│   └── sidecars/
│       └── python-skills/           # Python Production Skills Runtime
├── docs/                            # 总控规划、阶段计划、任务记录、交接记录
├── scripts/
│   └── windows/                     # Windows / D 盘环境约束脚本
├── README.md
└── .gitignore
```

## 核心生产链路

当前 Python Skills 已打通以下 deterministic envelope 链路，Stage 12 已为这些输出补齐后端持久化写回边界：

```text
story_intake
  -> plot_adaptation
  -> episode_writer
  -> character_bible
  -> style_bible
  -> storyboard_director
  -> image_prompt_builder
  -> image_generation
  -> video_prompt_builder
  -> video_generation
  -> voice_casting
  -> subtitle_generator
  -> auto_editor
  -> quality_checker
  -> export_packager
```

```mermaid
sequenceDiagram
    participant U as 用户输入
    participant API as Control API
    participant W as Worker
    participant S as Python Skills
    participant R as Reviewable JSON

    U->>API: 创建项目 / 启动生产任务
    API->>W: 推进生产阶段
    W->>S: 通过 SkillGateway 调用 skill
    S-->>W: 返回 envelope
    W-->>API: 更新任务状态
    API-->>R: 暴露脚本 / 分镜 / mock 资产 / 粗剪计划 / 质量报告
```

## 数据库与持久化说明

Stage 13 起，默认开发运行切到 PostgreSQL：

- 本机 PostgreSQL 18 Windows 服务
- 数据库名：`milu`
- 用户名：`root`
- 密码：`root`
- 连接串：`Host=127.0.0.1;Port=5432;Database=milu;Username=root;Password=root`

PostgreSQL adapter 已在 Stage 12 接入，Stage 13 已将 PostgreSQL 作为默认业务事实来源。仓库中已有 SQL migration：

```text
backend/control-plane/db/migrations/001_initial_control_plane.sql
backend/control-plane/db/migrations/002_stage12_postgresql_claiming.sql
backend/control-plane/db/migrations/003_stage14_checkpoint_notes.sql
backend/control-plane/db/migrations/004_stage16_auth_licensing.sql
```

本机初始化和迁移：
```powershell
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Initialize-MiLuStudioPostgreSql.ps1

. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage14-build\

$env:ASPNETCORE_URLS = "http://127.0.0.1:5368"
$env:ASPNETCORE_ENVIRONMENT = "Development"
D:\soft\program\dotnet\dotnet.exe D:\code\MiLuStudio\.tmp\stage14-build\MiLuStudio.Api.dll

Invoke-RestMethod -Method Post http://127.0.0.1:5368/api/system/migrations/apply
Invoke-RestMethod http://127.0.0.1:5368/api/system/preflight
```

Stage 12 / Stage 13 已完成的数据库能力：

- PostgreSQL / EF Core DbContext adapter。
- `RepositoryProvider=InMemory` / `RepositoryProvider=PostgreSQL` 配置切换。
- 后端 SQL migration status 和 apply endpoint。
- API preflight 检查数据库、migration 和 storage 状态。
- Worker durable claiming，PostgreSQL provider 使用 `FOR UPDATE SKIP LOCKED`。
- Stage 5-13 skill envelope 可通过 Control API / Worker 写入 `generation_tasks.output_json`、`assets` 和 `cost_ledger`。
- Stage 13 已将 Worker 调用 deterministic skills、任务状态推进和前端真实结果展示收敛到 PostgreSQL。
- Stage 16 已新增 `accounts`、`auth_sessions` 和 `devices` 表；历史 migration 中保留的 `licenses` 表暂不驱动当前网页端或桌面端体验。
- Stage 18 provider settings 不新增数据库 migration；Control API / Infrastructure 使用 D 盘 storage 下的本地 JSON 保存占位配置，响应和文件都只保留 API Key 遮罩与指纹，不保存可用于真实调用的明文 key。

相关说明：

- [PostgreSQL 配置说明](./docs/POSTGRESQL_STAGE12_SETUP.md)

数据库不会藏进 Electron 安装器。Stage 15 桌面宿主已通过 Control API health / preflight 展示结果，但不创建数据库、不执行 migrations。

## 打包前补丁

Stage 14 已完成，且未创建 Electron / 安装器。它把 Stage 13 之后发现的打包前问题先修掉：

- 让 Web UI 中用户输入或修改的故事、标题、模式、时长、画幅和风格真正保存到 Control API / PostgreSQL。
- 统一 Control API 默认端口、CORS、API base URL 和后续桌面宿主的配置注入方式。
- 清理过期 mock 文案和无实际处理器按钮，补齐 checkpoint 基本确认语义。
- 加固 PostgreSQL 默认 provider、InMemory 显式启用、skill run 临时目录清理和契约漂移检查。
- 补 API / Worker / PostgreSQL 自动化集成 PowerShell 脚本：`scripts\windows\Test-MiLuStudioStage14Integration.ps1`。
- 补 Python skill registry / `skill.yaml` / schema / validator 契约漂移检查：`backend\sidecars\python-skills\tests\test_stage14_skill_contracts.py`。

## 桌面打包

Stage 15 已完成 Electron + electron-builder + NSIS assisted installer：

- 新增 `apps\desktop`，通过本地 HTTP host 承载 `apps\web` 构建产物，避免 `file://` 路由和资源问题。
- 桌面宿主随机绑定本地端口，启动发布后的 Control API 和 Windows Worker，并通过 preload 注入 `window.__MILUSTUDIO_CONTROL_API_BASE__`；写请求额外携带桌面会话令牌，防止其他本地页面直接复用桌面 API。
- Web UI 已新增桌面诊断面板，调用 Electron IPC 获取 Control API health / preflight、数据库、storage、Python runtime、Python skills root、Worker 和 Web host 状态。
- 打包图标、安装器图标、卸载器图标、header 图标和托盘图标均由 `apps\web\public\brand\logo.png` 生成的 `apps\desktop\build\icon.ico` 提供。
- 桌面 runtime 默认打包 self-contained .NET API / Worker 与 `resources\python-runtime\python.exe`，干净 Windows 机器不再依赖外部 `dotnet.exe` 或本机 Python 安装。
- Electron `userData`、`sessionData` 和 logs 已显式指向 D 盘数据目录，避免默认落到 `C:\Users\...\AppData\Roaming`。
- Electron 主进程已限制外部导航、弹窗和 IPC 来源；Control API 桌面模式只允许桌面 Web host origin，并禁止桌面宿主执行 migration apply。
- electron-builder 输出 `D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe`，并保留 `win-unpacked` 供本地 smoke 验证。
- 自定义 `apps\desktop\build\installer.nsh` 当前只保留桌面快捷方式、开始菜单快捷方式和开机自启动复选项；安装前激活码页已撤下。
- Stage 19 新增 `scripts\windows\Test-MiLuStudioStage19DesktopRelease.ps1` 和 `docs\MILUSTUDIO_STAGE19_DESKTOP_RELEASE_CHECKLIST.md`，可重打包并验收安装器、运行时资源、签名前置配置和桌面 API 安全边界。
- 当前本机安装器和 `win-unpacked\MiLuStudio.exe` 的 Authenticode 状态仍为 `NotSigned`；本地验收允许记录为阻塞项，正式发布必须使用 `-RequireSigned` 阻断未签名产物。

## 账号与登录

Stage 16 已完成应用内账号注册、登录、会话和设备绑定：

- 新增 `AuthLicensingService`、`IAuthRepository`、token / password service 和账号会话链路。
- 当前 Control API：`/api/auth/register`、`/api/auth/login`、`/api/auth/refresh`、`/api/auth/logout`、`/api/auth/me`、`/api/auth/devices/bind`。
- Web UI 默认先展示登录 / 注册入口；登录后直接进入项目列表、项目详情和生产控制台。
- 项目、生产任务和 generation task 写回类 API 已加最小登录门禁；未登录返回 401。
- Electron 仍只注入 Control API base URL 和桌面会话令牌；账号密码和设备绑定都不放进 Electron 或安装器脚本。
- 许可证、激活码和付费码相关能力不作为当前 MVP 体验，后续商业化阶段再重新设计。

## Provider 前配置

Stage 18 已完成真实 provider adapter 前配置页：

- Web 左侧“模型”入口打开 `/settings/providers`，仍只通过 Control API 读取和保存配置。
- Control API 新增 `GET /api/settings/providers`、`PATCH /api/settings/providers` 和 `GET /api/settings/providers/preflight`。
- Application 层新增 `ProviderSettingsService`，Infrastructure 层新增本地文件 repository；该能力不走 Electron，不新增数据库 migration。
- 支持 Text / Image / Video / Audio / Edit 五类 adapter 的供应商、默认模型、启用状态、API Key 占位、单项目成本上限和失败重试次数。
- API Key 请求体会被处理成遮罩和 SHA256 指纹；响应与本地文件不返回、不保存明文 key。
- preflight 只检查本地占位配置完整性，明确 `externalNetwork=disabled`、`mediaGenerated=false`，不访问任何真实 provider。
- Stage 22 已补安全前置层：API Key 进入独立 metadata-only secret store，只保存遮罩、指纹和不可调用引用；spend guard 可在后端检查预算和重试边界；provider sandbox 明确阻断真实 provider 调用、外部网络、媒体读取、FFmpeg 和真实产物生成。

## 桌面发布验收

Stage 19 已完成桌面发布验收与代码签名前置准备：

- `apps\desktop\package.json` 新增 `verify:release` 和 `verify:release:signed`。
- `scripts\windows\Test-MiLuStudioStage19DesktopRelease.ps1` 可检查 package 配置、安装器产物、`win-unpacked`、Web dist、Control API / Worker runtime、Python runtime、Python Skills、最新 migration、Electron 安全设置、安装器快捷方式 / 自启动选项和 Authenticode 状态。
- 桌面本地 Web host 已设置 CSP 与 `X-Content-Type-Options: nosniff`；Stage 19 脚本会检查脚本来源、loopback 连接、object/base/form/frame 限制。
- 脚本默认允许 `NotSigned` 并写入警告；带 `-RequireSigned` 时安装器和主 exe 必须是 `Valid`。
- `.gitignore` 已忽略 `*.pfx`、`*.p12`、`*.pvk`、`*.spc` 和 `*.key`，避免代码签名证书或私钥容器进入仓库。
- 干净 Windows 手工验收步骤记录在 [Stage 19 桌面发布验收清单](./docs/MILUSTUDIO_STAGE19_DESKTOP_RELEASE_CHECKLIST.md)。

## Web 工作台

Stage 20 已完成 Codex 式前端工作台重构：

- `apps\web\src\features\workspace\StudioWorkspacePage.tsx` 作为登录后的主入口，替代旧多导航壳。
- 左侧只保留新项目入口、历史项目列表和左下角设置菜单。
- 新项目空态中央只显示一个输入框，用户可直接粘贴剧本和制作要求，也可上传 txt / md 文本。
- 启动生产前仍先通过 Control API 保存项目输入，再创建 production job，并通过 SSE 更新右侧进度卡。
- 右侧结果区只展示真实 generation task output 和 project assets 占位；打开分镜结果后可继续通过 Control API 保存分镜表或按单镜头备注重算。
- 打开角色、画风、图片提示词或视频提示词结果后，可在工作台内编辑结构化字段；保存时通过 `PATCH /api/generation-tasks/{taskId}/structured-output` 写回 JSON envelope，并重置下游任务等待重新计算。
- 模型配置、桌面诊断和账户信息统一从左下角设置菜单打开，不让 Web 或 Electron 绕过 Control API / Worker 边界。

## 当前边界

- 不接真实文本、图片、视频、音频或质检模型 provider。
- 不生成真实 PNG、MP4、WAV、SRT 或 ZIP。
- 不调用 FFmpeg。
- Stage 17 只保留本地 deterministic 分镜编辑与单镜头备注重算；Stage 18 只保留 provider 前占位配置和本地 preflight；Stage 19 只保留桌面发布验收和签名前置检查；Stage 20 只重构前端工作台交互与设置入口；Stage 21 只编辑已生成的结构化 JSON envelope 并重置下游任务；Stage 22 只落地 provider 安全前置层和占位检查；真实 provider、真实媒体生成、许可证、激活码和付费码后置。
- 默认开发模式使用 PostgreSQL；InMemory provider 只保留为快速 smoke / 特殊轻量场景。
- 不让 UI 直接访问数据库、文件系统、Python 脚本、模型 SDK 或 FFmpeg。
- 不引入 Linux、Docker、Redis、Celery 作为第一版生产依赖。
- 所有依赖、缓存、日志、上传素材和生成结果必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录。

## 下一阶段

Stage 22 已完成 provider adapter 安全前置层设计与占位落地。下一阶段建议候选方向按优先级整理为：

1. Stage 23 继续细化工作台高级编辑：提示词批量操作、镜头增删、差异对比和重算策略。
2. Stage 23 provider adapter 真实接入前设计评审：只做接口契约、审计日志和 dry-run，不发真实请求。
3. Stage 23 发布验收补强：拿到正式证书后做 `verify:release:signed`、干净 Windows 虚拟机安装 / 卸载回归和发布说明。

商业授权、激活码、付费码和套餐计费体系暂不作为近期 MVP 范围。

## 本地运行说明

### 1. 前端

```powershell
cd D:\code\MiLuStudio\apps\web
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\nodejs\npm.ps1 run build
D:\soft\program\nodejs\npm.ps1 run dev
```

### 2. .NET Control Plane

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore
```

如果本机正在运行 `MiLuStudio.Api`，默认 Debug 输出目录可能被锁定。可临时改用 D 盘输出目录验证编译：

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\control-plane-build\
```

### 3. Python Skills

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
cd D:\code\MiLuStudio\backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m compileall -q milu_studio_skills skills tests
& $env:MILUSTUDIO_PYTHON -m unittest discover -s tests -v
```

运行一个 Stage 11 skill 示例：

```powershell
cd D:\code\MiLuStudio\backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill export_packager --input skills\export_packager\examples\input.json --output skills\export_packager\examples\output.json --pretty
```

### 4. 桌面宿主与安装包

```powershell
cd D:\code\MiLuStudio\apps\desktop
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\nodejs\npm.ps1 run smoke
D:\soft\program\nodejs\npm.ps1 run dist:win
D:\soft\program\nodejs\npm.ps1 run verify:release
```

也可以用脚本执行桌面验证：

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktop.ps1 -SkipInstall
```

桌面模式 API 安全验证脚本：

```powershell
D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1 -SkipPrepareRuntime
```

正式签名发布前使用：

```powershell
cd D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run verify:release:signed
```

Stage 16 账号与会话集成验证：

```powershell
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage16Auth.ps1
```

Stage 17 分镜编辑闭环验证：

```powershell
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage17StoryboardEditing.ps1
```

Stage 22 Provider 安全前置层验证：

```powershell
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage22ProviderSafety.ps1
```

## 项目亮点

- 不是单一 demo 页面，而是按真实 AI 漫剧生产链路拆分脚本、角色、风格、分镜、图片、视频、配音、字幕、剪辑和质检边界。
- 使用统一 Python Skills Runtime 和 `SkillGateway`，让内部 Production Skills 可测试、可审阅、可替换。
- 每个阶段都输出结构化 JSON envelope，便于后续写入数据库、展示审核卡片、记录成本、定位质量问题和重试。
- Control API / Worker / Python Sidecar 分层清晰，UI 不直接碰底层系统能力。
- 数据库和桌面端明确解耦，避免 Electron 安装器过早绑定业务持久化。
- Electron 桌面宿主只通过 Control API 与业务系统通信，并可展示 health / preflight / Worker 状态。
- 所有阶段都强调 Windows 原生交付、D 盘环境约束和商业化后置边界。
- 对真实 provider、FFmpeg、PostgreSQL、桌面安装器和账号体系都保留 adapter 边界，方便后续逐步接入。

## 文档导航

- [总构建计划](./docs/MILUSTUDIO_BUILD_PLAN.md)
- [阶段计划](./docs/MILUSTUDIO_PHASE_PLAN.md)
- [任务记录](./docs/MILUSTUDIO_TASK_RECORD.md)
- [短棒交接](./docs/MILUSTUDIO_HANDOFF.md)
- [产品规格](./docs/PRODUCT_SPEC.md)
- [参考项目说明](./docs/REFERENCE_PROJECTS.md)
- [PostgreSQL 配置说明](./docs/POSTGRESQL_STAGE12_SETUP.md)
- [Stage 19 桌面发布验收清单](./docs/MILUSTUDIO_STAGE19_DESKTOP_RELEASE_CHECKLIST.md)

## 后续可改进方向

- 接入真实媒体质量检测 adapter，例如黑屏、卡顿、水印、分辨率、音量和字幕烧录检测。
- 接入真实 Text / Image / Video / Audio / Edit provider adapter。
- 在新 Web 工作台中继续补齐脚本卡、资产卡、质量报告和导出区的编辑与 diff，并扩展角色、画风和提示词的批量操作。
- 拿到正式代码签名证书后补真实 Authenticode 签名流水线和干净 Windows 机器安装回归；Stage 19 已能用 `-RequireSigned` 阻断未签名安装包。
- 后续商业化大后期再推进许可证、付费码、套餐限制和云端授权系统。
