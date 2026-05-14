# MiLuStudio 短棒交接

更新时间：2026-05-14
工作目录：`D:\code\MiLuStudio`

本文件只保留下一棒需要立刻接住的上下文。

长规规划看 `docs\MILUSTUDIO_BUILD_PLAN.md`。
总任务阶段安排看 `docs\MILUSTUDIO_PHASE_PLAN.md`。
修改记录和自检记录看 `docs\MILUSTUDIO_TASK_RECORD.md`。

## 每棒先读

```powershell
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Get-Content .\README.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_BUILD_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_PHASE_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_TASK_RECORD.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_HANDOFF.md -Encoding UTF8
```

## 当前接棒

```text
Phase: Post Stage 16
Status: pending user confirmation
Owner: next session
Goal: 由用户确认 Stage 17 的正式编号与范围。
```

## 已完成

- Stage 0：独立 Git 仓库、根文档、产品规格和参考清单已就位。
- Stage 1：`apps\web` Vite + React + TypeScript 前端壳已就位，使用项目内 `/brand/logo.png`，`npm run build` 通过。
- Stage 2：`.NET Control API` solution 已就位，已有项目 API、production job API、pause、resume、retry 和 SSE mock；PostgreSQL 当前只落 SQL migration，运行期仍是 in-memory repository。
- Stage 3：已有 production 状态机、checkpoint、retry 和 mock SSE 闭环；前端通过 Control API DTO / SSE 展示任务状态。
- Stage 4：已有 `backend\sidecars\python-skills` Runtime、统一 CLI、`SkillGateway` 和 `story_intake`。
- Stage 5：已有 `story_intake -> plot_adaptation -> episode_writer` envelope 链路。
- Stage 6：已有 `episode_writer -> character_bible -> style_bible` envelope 链路。
- Stage 7：已有 `episode_writer + character_bible + style_bible -> storyboard_director` envelope 链路。
- Stage 8：已有 `storyboard_director + character_bible + style_bible -> image_prompt_builder -> image_generation` envelope 链路。
- Stage 9：已有 `storyboard_director + image_prompt_builder + image_generation -> video_prompt_builder -> video_generation` envelope 链路。
- Stage 10：已有 `episode_writer + storyboard_director -> voice_casting -> subtitle_generator`，以及 `storyboard_director + video_generation + voice_casting + subtitle_generator -> auto_editor` envelope 链路。
- Stage 11：已有 `character_bible + style_bible + storyboard_director + video_generation + voice_casting + subtitle_generator + auto_editor -> quality_checker` envelope 链路。
- Stage 12：已有 PostgreSQL / EF Core DbContext adapter、RepositoryProvider 配置切换、API preflight、migration status / apply、Worker durable claiming 和 skill envelope 数据库写回边界。
- Stage 13：真实 `milu` 数据库、Worker 调 Python deterministic skills、API 数据库事实来源和前端真实结果展示已完成。
- Stage 14：打包前补丁与 Stage 13 收敛已完成；真实用户输入保存链路、端口 / CORS / API base URL、checkpoint approve / reject / notes、重新生成淘汰旧未完成 job、PostgreSQL 默认 provider、skill run 保留策略、集成脚本和 Python skill 契约漂移检查已落地。
- Stage 15：Electron + electron-builder + NSIS 桌面打包已完成；`apps\desktop`、本地服务进程管理、Control API base URL / 桌面会话令牌注入、桌面诊断面板、品牌图标、NSIS assisted installer、随包 .NET / Python runtime 和桌面验证脚本已落地；安装器激活码页已按当前 MVP 范围撤下。
- Stage 16：账号注册、登录、会话刷新、退出和设备绑定已完成；许可证、激活码和付费码体验已撤下，商业授权作为后续大后期内容。

## Stage 12 落地内容

- 新增 `backend\control-plane\src\MiLuStudio.Infrastructure\Persistence\PostgreSql\MiLuStudioDbContext.cs`。
- 新增 `PostgreSqlControlPlaneRepository`，覆盖 projects、production jobs、generation tasks、assets 和 cost ledger。
- 新增 `ControlPlane:RepositoryProvider=InMemory` / `PostgreSQL` 切换；Stage 13 决策为默认切到 PostgreSQL，InMemory 仅保留为快速 smoke 备选。
- 新增 `/api/system/preflight`、`/api/system/migrations` 和 `/api/system/migrations/apply`。
- 新增 `backend\control-plane\db\migrations\002_stage12_postgresql_claiming.sql`。
- Worker 现在通过 repository 领取任务；PostgreSQL provider 使用 `FOR UPDATE SKIP LOCKED`，并可接管 `locked_until` 过期的 running task。
- 新增 `POST /api/generation-tasks/{taskId}/output`，把 Stage 5-11 skill envelope 写入 `generation_tasks.output_json`，同时建立 `assets` 和可选 `cost_ledger` 记录。
- 新增 `GET /api/projects/{projectId}/assets` 和 `GET /api/projects/{projectId}/cost-ledger`。
- 新增 `docs\POSTGRESQL_STAGE12_SETUP.md`，说明本地连接配置、preflight、migration、Worker claiming 和 Electron 禁止边界。

## Stage 13 落地内容

- 默认 `RepositoryProvider` 已切到 `PostgreSQL`，API / Worker 版本库连接串为 `Host=127.0.0.1;Port=5432;Database=milu;Username=root;Password=root`。
- 已新增 `scripts\windows\Initialize-MiLuStudioPostgreSql.ps1`，幂等创建 `milu`；本机 `root/root` 无 `CREATEDB` 权限时使用 `postgres/root` bootstrap 建库并把 owner 设为 `root`。
- 已通过 Control API `/api/system/migrations/apply` 应用 `001_initial_control_plane` 和 `002_stage12_postgresql_claiming`。
- 已新增 `IProductionSkillRunner` / `PythonProductionSkillRunner`，Worker 通过 Python CLI / `SkillGateway` 调 deterministic skills。
- 已新增 `export_packager` deterministic skill，只输出 MP4 / SRT / JSON / ZIP 占位交付结构，不生成真实文件。
- 已把 `plot_adaptation`、`image_prompt_builder`、`video_prompt_builder`、`export_packager` 纳入完整 queue，Stage 5-13 可端到端写回 PostgreSQL。
- 已将 `SystemClock` 改为 UTC，满足 Npgsql 写入 `timestamptz` 的要求；展示层仍转本地时间。
- 已收敛 PostgreSQL repository 的 job/task 写入顺序和 EF Core ChangeTracker 清理，避免外键顺序与长链路跟踪冲突。
- API SSE 已改为读取数据库快照，不再由 API mock 自动推进生产状态。
- 前端已通过 Control API 读取 job、tasks、assets、cost ledger，并用真实 `outputJson` envelope 构建结果卡和导出区。

## Stage 14 落地内容

- Web UI 编辑区已改为真实项目草稿：故事文本、标题、模式、目标时长、画幅和风格通过 Control API 写入 PostgreSQL，启动生产前会先保存。
- 后端项目更新链路已扩展到 `story_inputs.original_text`、word count 和项目描述；故事文本保持 500 到 2000 字校验。
- Control API 默认端口统一为 `http://127.0.0.1:5368`；前端 API base URL 解析顺序为桌面宿主注入 `window.__MILUSTUDIO_CONTROL_API_BASE__`、`VITE_CONTROL_API_BASE`、默认端口。
- CORS 已允许 loopback 本地源；前端路由支持 hash route，便于 Stage 15 桌面静态或本地 HTTP 承载。
- 已清理 `Stage 1 mock` 文案和没有处理器的 `输出目录`、`锁定`、`重生成` 按钮。
- checkpoint 已支持 approve / reject / notes，notes 持久化到 `generation_tasks.checkpoint_notes`；新增 `003_stage14_checkpoint_notes.sql`。
- 同一项目点击开始生成或重新生成时，会先把 running / paused / queued 旧 job 标记为 failed，并写入“已根据当前输入重新生成，新任务已取代该旧任务。”，再按当前已保存输入创建新 job。
- PostgreSQL 是默认 provider，InMemory 只在显式配置 `RepositoryProvider=InMemory` 时启用；`001_initial_control_plane.sql` 旧注释已修正。
- `.tmp\skill-runs` 默认保留最近 30 次运行，可用 `ControlPlane:SkillRunRetentionCount` 调整。
- 新增 `scripts\windows\Test-MiLuStudioStage14Integration.ps1`，覆盖 migration、项目创建 / 更新、Worker 消费最新故事、API / Worker 重启恢复、lease 过期接管、checkpoint approve / reject notes 和 retry。
- 新增 `backend\sidecars\python-skills\tests\test_stage14_skill_contracts.py`，检查 registry、`skill.yaml`、schema、executor 和 validator 契约漂移。

## Stage 15 落地内容

- 新增 `apps\desktop`，使用 Electron 承载现有 Web UI 构建产物，并通过本地 HTTP host 提供静态资源、hash route fallback 和 CSP。
- 桌面宿主随机绑定本地端口，启动发布后的 Control API 和 Windows Worker；Python deterministic skills 和随包 `python-runtime` 作为 Worker sidecar runtime 路径注入后端配置。
- preload 只向 Web UI 注入 `window.__MILUSTUDIO_CONTROL_API_BASE__`、`window.__MILUSTUDIO_DESKTOP_TOKEN__` 和受控桌面 IPC，不暴露数据库、业务文件系统、Python 脚本或 FFmpeg。
- Web UI 新增桌面诊断面板，展示 Control API health / preflight、PostgreSQL、storage、Python runtime、Python skills root、Worker 和 Web host 状态。
- 新增 `scripts\windows\Prepare-MiLuStudioDesktopRuntime.ps1`，复制 Web dist、self-contained API / Worker 发布产物、SQL migrations、Python deterministic skills 和 Python runtime 到 `apps\desktop\runtime`。
- 新增 `scripts\windows\Test-MiLuStudioDesktop.ps1`，覆盖 runtime 准备、桌面 TypeScript build、Electron smoke 和桌面 API 安全验证；`scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1` 可单独验证桌面令牌和 migration apply 禁止语义。
- 打包图标、安装器图标、卸载器图标、header 图标、快捷方式图标和托盘图标均来自 `apps\web\public\brand\logo.png` 生成的多尺寸 `apps\desktop\build\icon.ico`。
- electron-builder + NSIS 已配置 `oneClick=false`、`allowToChangeInstallationDirectory=true`、`runAfterFinish=true`、`shortcutName=MiLuStudio`、桌面快捷方式、开始菜单快捷方式、AppUserModelID 和自定义 `installer.nsh`。
- Electron 已升级到 `42.0.1`，主进程限制外部导航、弹窗和 IPC 来源；Electron `userData`、`sessionData` 和 logs 已显式指向 D 盘数据目录，避免默认落到 `C:\Users\...\AppData\Roaming`。
- `apps\desktop\build\installer.nsh` 当前只保留桌面快捷方式、开始菜单快捷方式和开机自启动复选项；安装前激活码页已撤下。
- 已生成 `D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe` 和 `win-unpacked`；打包资源包含 `resources\web`、`resources\control-plane`、`resources\python-skills`、`resources\python-runtime` 和 `resources\build\icon.ico`。
- Control API 桌面模式使用 exact desktop origin + `X-MiLuStudio-Desktop-Token` 保护 unsafe HTTP methods，并拒绝 `/api/system/migrations/apply`。

## Stage 16 落地内容

- 新增 `Account`、`AuthSession`、`DeviceBinding`、`LicenseGrant` 领域实体和账号 / 许可证枚举。
- 新增 `AuthLicensingService`、`IAuthRepository`、`IAuthTokenService`、`IPasswordHasher`、`IAuthLicensingAdapter`，授权语义集中在 Control API / Application / Infrastructure。
- 新增 `backend\control-plane\db\migrations\004_stage16_auth_licensing.sql`，创建 `accounts`、`auth_sessions`、`devices` 和 `licenses`；Electron 不执行、不拥有 schema。
- PostgreSQL provider 新增 `PostgreSqlAuthRepository`；InMemory provider 新增 `InMemoryAuthRepository`。
- 当前 MVP 已撤下许可证 / 激活码 / 付费码体验；历史 `licenses` 表和 adapter 边界只作为后续商业化预留。
- Control API 当前开放 `/api/auth/register`、`/api/auth/login`、`/api/auth/refresh`、`/api/auth/logout`、`/api/auth/me` 和 `/api/auth/devices/bind`。
- 项目、生产任务和 generation task 类 API 已加最小登录门禁；未登录返回 401。
- Web UI 新增 `apps\web\src\features\auth\AuthGate.tsx`，未登录时只显示登录 / 注册入口，登录后直接进入工作台。
- Web Control API client 保存 access / refresh token 到 localStorage，并只把 Bearer token 发给 Control API；SSE 使用同一 access token query。
- Electron / preload 仍只注入 Control API base URL 和桌面会话令牌，不保存账号密码，不判断许可证。
- 新增 `scripts\windows\Test-MiLuStudioStage16Auth.ps1`。
- `scripts\windows\Test-MiLuStudioStage14Integration.ps1` 已补账号 bootstrap；`Test-MiLuStudioDesktopApiSecurity.ps1` 已确认桌面令牌和应用登录分层。

## Stage 10 落地内容

- 新增 `backend\sidecars\python-skills\skills\voice_casting`。
- 新增 `backend\sidecars\python-skills\skills\subtitle_generator`。
- 新增 `backend\sidecars\python-skills\skills\auto_editor`。
- `SkillGateway.default()` 已注册 `voice_casting`、`subtitle_generator` 和 `auto_editor`。
- `voice_casting` 输出 `voice_profiles`、`voice_tasks`、逻辑音频 asset intent、零成本估算和 `checkpoint.required=true`。
- `subtitle_generator` 输出 `subtitle_cues`、SRT-ready 文本结构、逻辑字幕 asset intent 和 review warnings。
- `auto_editor` 输出 video / audio / subtitle timeline tracks、rough edit `render_plan`、逻辑 MP4 output intent 和 review warnings。
- Stage 10 输出只暴露逻辑 `milu://mock-assets/...` URI / output intent；不写 WAV / SRT / MP4 文件、不写数据库、不调用真实 TTS / BGM / SFX provider、不触发 FFmpeg。
- 新增 `backend\sidecars\python-skills\tests\test_stage10_audio_subtitle_edit_pipeline.py`，覆盖完整 Stage 4-10 envelope 链路。

## Stage 11 落地内容

- 新增 `backend\sidecars\python-skills\skills\quality_checker`。
- `SkillGateway.default()` 已注册 `quality_checker`。
- `quality_checker` 输出质量问题报告、严重级别、可自动重试项、人工确认项、质量 manifest 和 `checkpoint.required=true`。
- Stage 11 只检查 envelope 元数据和 deterministic 结构；不读取真实媒体文件、不接视觉 / 音频检测模型、不触发 FFmpeg、不写数据库、不生成真实 MP4。
- 新增 `backend\sidecars\python-skills\tests\test_stage11_quality_checker_pipeline.py`，覆盖 Stage 5-11 完整 envelope 链路、字幕过长可重试报告和失败 envelope。

## 验证已通过

```powershell
. .\scripts\windows\Set-MiLuStudioEnv.ps1
Push-Location backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m compileall -q milu_studio_skills skills tests
& $env:MILUSTUDIO_PYTHON -m unittest discover -s tests -v
Pop-Location
```

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\control-plane-build\
Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location
```

注意：默认 Debug 输出目录构建曾因正在运行的 `MiLuStudio.Api (7832)` 锁定 DLL 失败；改用上面的临时 D 盘输出目录验证通过，临时目录已清理。

Stage 12 后端验证：

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\control-plane-stage12-build\
```

```powershell
# InMemory API smoke 已验证：
# /health
# /api/system/preflight
# POST /api/projects
# POST /api/projects/{projectId}/production-jobs
# GET /api/production-jobs/{jobId}/tasks
# POST /api/generation-tasks/{taskId}/output
# GET /api/projects/{projectId}/assets
```

Stage 13 PostgreSQL / Worker / 前端验证：

```powershell
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Initialize-MiLuStudioPostgreSql.ps1

. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage13-build\

# Control API on http://127.0.0.1:5368
# POST /api/system/migrations/apply -> applied / up_to_date
# GET /api/system/preflight -> healthy=true
# Full smoke -> job_ba4b02d1cd534e948fe0fda74aaead3c completed / 100
# generation_tasks: 15 rows, 15 completed, 15 output_json present
# cost_ledger: 15 rows

cd D:\code\MiLuStudio\apps\web
npm run build
```

Stage 14 打包前补丁验证：

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

集成脚本最近一次通过结果：

- `Stage 14 integration passed. Completed job: job_ce56423f476d4f1888115f42a2f4b3e0`
- 脚本结束后已确认 `5368` 无残留监听。

Stage 15 桌面打包验证：

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
Push-Location D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run build
D:\soft\program\nodejs\npm.ps1 run smoke
D:\soft\program\nodejs\npm.ps1 run pack:dir
D:\soft\program\nodejs\npm.ps1 run dist:win
Pop-Location

D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktop.ps1 -SkipInstall
D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1 -SkipPrepareRuntime
& D:\code\MiLuStudio\outputs\desktop\win-unpacked\MiLuStudio.exe --smoke-test
```

验证结果：

- desktop TypeScript build：通过。
- Web `npm run build`：通过。
- Control Plane `.sln` build：0 warning / 0 error；首次 Debug build 被旧 `MiLuStudio.Api (7832)` 锁定，确认命令行属于本仓库后结束进程并重跑通过。
- Electron smoke：Control API / Worker / Web host 均 running，preflight healthy=true，PostgreSQL reachable，migrations up_to_date，Python runtime 和 Python skills root 均 ok。
- 桌面 API 安全脚本：无令牌写请求 403、带令牌写请求进入业务校验、桌面模式 migration apply 403。
- `pack:dir` / `dist:win`：生成 `D:\code\MiLuStudio\outputs\desktop\win-unpacked`、`D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe` 和 `.blockmap`，并包含 self-contained .NET runtime 与 `resources\python-runtime\python.exe`。
- packaged smoke：`win-unpacked\MiLuStudio.exe --smoke-test` exit code 0，结束后无 `MiLuStudio.Api.exe` / `MiLuStudio.Worker.exe` 残留。
- packaged Electron `userData` / `sessionData` 已落到 `D:\code\MiLuStudio\outputs\desktop\win-unpacked\data\.tmp\electron-*`。
- Authenticode 检查：`MiLuStudio-Setup-0.1.0.exe` 当前为 `NotSigned`，正式发布前必须补签名。

Stage 16 账号授权验证：

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

验证结果：

- .NET build：0 warning / 0 error。
- Web build：通过。
- Stage 16 account/session integration：通过；覆盖未登录 401、注册、登录、refresh、设备绑定、logout 和重新登录。
- Stage 14 integration：通过；最近完成 job `job_b1b07370aea34c43a2f332c12fe08bbe`。
- Desktop API security：通过；无桌面令牌 403，带桌面令牌后进入应用登录门禁 401，桌面 migration apply 403。
- Desktop TypeScript build：通过。

CLI examples:

```powershell
Push-Location backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill voice_casting --input skills\voice_casting\examples\input.json --output skills\voice_casting\examples\output.json --pretty
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill subtitle_generator --input skills\subtitle_generator\examples\input.json --output skills\subtitle_generator\examples\output.json --pretty
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill auto_editor --input skills\auto_editor\examples\input.json --output skills\auto_editor\examples\output.json --pretty
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill quality_checker --input skills\quality_checker\examples\input.json --output skills\quality_checker\examples\output.json --pretty
Pop-Location
```

## 固定约束

- 不要接真实模型 provider。
- 不要引入 Linux / Docker / Redis / Celery 作为生产依赖。
- 不要让 UI 直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- 继续通过 Control API / Worker / Python Skills Runtime 边界推进。
- 每次修改完成后必须检查根 `README.md` 是否需要同步更新。
- 根 `README.md` 必须使用中文、PowerShell 友好，并采用类似 `D:\code\BookRecommendation\README.md` 的面试展示导向结构。
- Stage 12 已完成 PostgreSQL 持久化与后端收敛的后端边界；数据库、migration、repository adapter、Worker durable claiming 不和桌面端绑定。
- Stage 13 已完成真实配置与端到端收敛：默认 PostgreSQL、`milu` 数据库、Worker 调 Python deterministic skills、前端真实结果展示。
- Stage 14 已完成打包前补丁与 Stage 13 收敛。
- Stage 15 已完成桌面打包，唯一方案为 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。
- Electron 只做桌面宿主、安装器和本地进程管理；现有 Web UI 和 Control API 必须继续保持可独立迭代。
- 数据库不和桌面端绑定：Electron 不直接访问 PostgreSQL，不执行 migrations，不定义数据库表，不负责数据库初始化。
- Stage 16 已完成本地 deterministic 账号系统；应用打开先显示登录 / 注册入口，登录后进入工作台。
- 许可证、付费码和激活码已从当前 Web / Desktop 体验撤下，作为后续商业化大后期内容。
- 任务栏固定不做静默强制 pin；必须通过正确 AppUserModelID、图标、快捷方式和用户引导，让用户自行固定。
- Stage 13 起默认开发配置已切到 PostgreSQL，使用本机 PostgreSQL 18 的 `root/root` 和业务库 `milu`；InMemory 只作为快速 smoke / 特殊轻量场景保留。
- 所有依赖、配置、缓存、运行数据、日志、上传素材和生成结果必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录。
- 继续使用 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束 D 盘环境。
- 本机 PATH 前面可能有 C 盘 WindowsApps `python.exe`；运行 Python 必须显式使用 `D:\soft\program\Python\Python313\python.exe`、`$env:MILUSTUDIO_PYTHON` 或项目内 D 盘 venv。

## 技术债

- 真实 PostgreSQL migration / API / Worker 共享状态 smoke、Stage 14 自动化集成脚本和 Stage 15 桌面宿主 smoke / NSIS 打包验证已完成。
- 生产控制台已补中文化、checkpoint 审核解释和真实产物预览：内部 skill 名、英文 stage label、任务状态和字段名默认不再直接暴露给普通用户；技术字段折叠到“技术详情”。
- checkpoint 审核 UI 已补“本步产出 / 检查什么 / 通过后 / 拒绝后”说明，拒绝前需要填写修改意见；中间审核区和右侧结果卡都会突出当前审核产物。
- `style_bible` 已有专门预览：画风名、视觉规则、色板、灯光、场景、镜头语言、角色一致性、负面提示词和后续可复用提示词块；其他 deterministic skills 先走通用结构化预览兜底。
- `storyboard_director` 已新增 `cinematic_md_v1` 分镜稿结构和专用 UI 预览：`film_overview`、`storyboard_parts`、`rendered_markdown`、`validation_report`；下游仍继续消费原 `shots`。
- Control API 正常连接且项目尚未开始时，生产控制台不再显示 mock 阶段状态或 mock 结果卡；真实结果卡只在 task output 写入后按队列顺序逐步追加。
- 生产控制台重新生成会按当前已保存输入创建新 job，并把同项目未完成旧 job 标记为被取代，避免旧剧本结果继续显示到新输入。
- `story_intake` 已补中文人物句式优先抽取和自然物过滤；《牡丹亭》fixture 会输出 `杜丽娘 / 春香 / 柳梦梅`，不再把 `柳枝垂 / 花影像 / 她倚` 当角色。
- checkpoint 审核态按钮已改成更明确的“已暂停 / 通过后继续 / 通过并继续 / 退回修改 / 退回后重试”，退回修改会引导先填写备注。
- 中文测试剧本 fixture 位于 `docs\test-fixtures\scripts`：完整《牡丹亭》公版文本和 500 字测试输入样本均已准备好。
- InMemory provider 仍是进程内开发 smoke，不跨 API / Worker 进程共享状态；跨进程共享必须使用 PostgreSQL provider。
- `storage_intent` 只是 Stage 8 mock asset 的未来 adapter 目标描述，不代表真实文件已生成。
- 真实图片 provider、文件写入、资产持久化、前端选图和重试均未实现。
- 严格 120 秒、50-55 镜头、逐句对白保真、10-14 部分长分镜模式仍未实现，需要后续 TextProvider / 模型 adapter、对白校验器和长文本分镜 validator。
- `video_generation` 当前只输出 mock clip records，不代表真实 MP4 已生成。
- 真实视频 provider、视频文件写入、资产持久化、前端视频预览、单镜头重试和 FFmpeg 均未实现。
- `voice_casting` 当前只输出配音任务和逻辑音频 intent，不代表真实 WAV 已生成。
- `subtitle_generator` 当前只输出 SRT-ready 文本结构，不代表真实 `.srt` 文件已落盘。
- `auto_editor` 当前只输出粗剪 timeline / render plan，不代表 FFmpeg 已执行或 MP4 已生成。
- `quality_checker` 当前只输出结构化质量报告和 retry hint，不代表已读取真实媒体、完成黑屏 / 卡顿 / 水印 / 音量检测或保存项目资产。
- `export_packager` 当前只输出导出包占位结构，不代表真实 MP4 / SRT / JSON / ZIP 文件已落盘。
- 真实 TTS provider、音色试听、BGM/SFX、音量标准化、字幕文件落盘、FFmpeg render adapter、最终 MP4 和下载区真实文件下载均未实现。
- Stage 16 当前只使用本地 deterministic 账号、会话和设备绑定；真实云端授权服务、离线签名许可证、激活码和套餐计费策略均后置。
- 安装器激活码输入页已撤下；当前安装器只保留快捷方式、开始菜单和开机自启动选项。
- electron-builder / NSIS 安装器已生成本机安装包，且已随包携带 .NET / Python runtime；仍需后续在干净 Windows 用户环境做人工安装 / 卸载 / 自启动 / 快捷方式验收，并补正式 Authenticode 代码签名。

## 下一步建议

1. 先由用户确认 Stage 17 的正式编号与范围。
2. 建议候选方向：真实 provider adapter 前的配置页，或正式代码签名与干净 Windows 安装验收。
3. 继续保持 Electron / Web UI 只通过 Control API 处理账号状态，不接触 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg。
4. 后续发布前仍需补正式代码签名、安装器品牌视觉细化和干净 Windows 安装验收。

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\README.md、D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0、Stage 1、Stage 2、Stage 3、Stage 4、Stage 5、Stage 6、Stage 7、Stage 8、Stage 9、Stage 10、Stage 11、Stage 12、Stage 13、Stage 14、Stage 15 和 Stage 16 已完成。当前 Web / Desktop 已撤下许可证、激活码和付费码体验，商业授权作为后续大后期内容。请先确认下一阶段正式编号与范围；建议候选方向为真实 provider adapter 前的配置页，或正式代码签名与干净 Windows 安装验收。后续阶段仍不得接真实模型、不得读取真实媒体文件、不得触发 FFmpeg、不得生成真实 MP4/WAV/SRT/ZIP，不引入 Linux/Docker/Redis/Celery 作为生产依赖，不让 UI 或 Electron 绕过 Control API / Worker 边界，不让桌面端执行 migrations、定义数据库表或负责数据库初始化。
```
