# MiLuStudio 短棒交接

更新时间：2026-05-13
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
Phase: Stage 14
Status: pending
Owner: next session
Goal: 桌面端独立打包与 Electron + electron-builder + NSIS 自定义安装器。
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
- Stage 14 进入桌面打包，唯一方案为 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。
- Electron 只做桌面宿主、安装器和本地进程管理；现有 Web UI 和 Control API 必须继续保持可独立迭代。
- 数据库不和桌面端绑定：Electron 不直接访问 PostgreSQL，不执行 migrations，不定义数据库表，不负责数据库初始化。
- 账号系统不阻塞 Stage 14 桌面安装包 MVP，作为桌面 MVP 后下一大阶段；届时对齐 QQ 类体验：应用打开只显示登录 / 注册 / 激活入口，登录后才进入工作台。
- 安装器可加付费码 / 激活码输入页作为安装门槛，但正式授权必须由 Control API / Auth & Licensing adapter 在应用内校验。
- 任务栏固定不做静默强制 pin；必须通过正确 AppUserModelID、图标、快捷方式和用户引导，让用户自行固定。
- Stage 13 起默认开发配置已切到 PostgreSQL，使用本机 PostgreSQL 18 的 `root/root` 和业务库 `milu`；InMemory 只作为快速 smoke / 特殊轻量场景保留。
- 所有依赖、配置、缓存、运行数据、日志、上传素材和生成结果必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录。
- 继续使用 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束 D 盘环境。
- 本机 PATH 前面可能有 C 盘 WindowsApps `python.exe`；运行 Python 必须显式使用 `D:\soft\program\Python\Python313\python.exe`、`$env:MILUSTUDIO_PYTHON` 或项目内 D 盘 venv。

## 技术债

- 真实 PostgreSQL migration / API / Worker 共享状态 smoke 已完成；后续仍需要补专门的自动化集成测试脚本，覆盖 API / Worker 重启恢复和失败任务 lease 接管。
- InMemory provider 仍是进程内开发 smoke，不跨 API / Worker 进程共享状态；跨进程共享必须使用 PostgreSQL provider。
- `storage_intent` 只是 Stage 8 mock asset 的未来 adapter 目标描述，不代表真实文件已生成。
- 真实图片 provider、文件写入、资产持久化、前端选图和重试均未实现。
- `video_generation` 当前只输出 mock clip records，不代表真实 MP4 已生成。
- 真实视频 provider、视频文件写入、资产持久化、前端视频预览、单镜头重试和 FFmpeg 均未实现。
- `voice_casting` 当前只输出配音任务和逻辑音频 intent，不代表真实 WAV 已生成。
- `subtitle_generator` 当前只输出 SRT-ready 文本结构，不代表真实 `.srt` 文件已落盘。
- `auto_editor` 当前只输出粗剪 timeline / render plan，不代表 FFmpeg 已执行或 MP4 已生成。
- `quality_checker` 当前只输出结构化质量报告和 retry hint，不代表已读取真实媒体、完成黑屏 / 卡顿 / 水印 / 音量检测或保存项目资产。
- `export_packager` 当前只输出导出包占位结构，不代表真实 MP4 / SRT / JSON / ZIP 文件已落盘。
- 真实 TTS provider、音色试听、BGM/SFX、音量标准化、字幕文件落盘、FFmpeg render adapter、最终 MP4 和下载区真实文件下载均未实现。
- 账号注册、登录、会话、设备绑定、许可证和 Auth & Licensing adapter 尚未实现，只已写入规划，且不阻塞 Stage 14 桌面安装包 MVP。
- 安装器激活码输入页尚未实现，但仍属于 Stage 14 安装器能力。
- electron-builder / NSIS 安装器尚未实现；Stage 14 必须验证自定义图标、安装目录选择、桌面快捷方式、开始菜单快捷方式、开机自启动、进度条、安装完成后启动和激活码输入页。

## 下一步建议

1. 开始 Stage 14 桌面打包。
2. 创建 `apps\desktop`，采用 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。
3. 桌面端只负责宿主、安装器和本地进程管理，继续通过 Control API health / preflight 检查 PostgreSQL、storage、Python runtime 和 Worker 状态。
4. 安装器实现自定义图标、安装目录选择、桌面快捷方式、开始菜单快捷方式、开机自启动、安装完成后启动、实时进度条和付费码 / 激活码输入页。
5. 保持数据库和桌面端解耦：Electron 不直接访问 PostgreSQL，不执行 migrations，不定义数据库表，不调用 Python 或 FFmpeg。
6. 阶段完成后必须联网搜索自检；如发现偏差，先更新 `MILUSTUDIO_BUILD_PLAN.md`，再更新 `MILUSTUDIO_PHASE_PLAN.md`，再把原因写入 `MILUSTUDIO_TASK_RECORD.md`，再检查 / 更新根 `README.md`，最后更新本 handoff。

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\README.md、D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0、Stage 1、Stage 2、Stage 3、Stage 4、Stage 5、Stage 6、Stage 7、Stage 8、Stage 9、Stage 10、Stage 11、Stage 12 和 Stage 13 已完成。按 docs\MILUSTUDIO_PHASE_PLAN.md 的 Stage 14 开始实现桌面端独立打包：唯一方案为 Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh；桌面端只作为宿主、安装器和本地进程管理层，继续保留 Web UI、Control API、Worker、PostgreSQL adapter 和 Python Skills 的独立边界；安装器需要支持自定义图标、安装目录选择、桌面快捷方式、开始菜单快捷方式、开机自启动、安装完成后启动、实时安装进度条和付费码 / 激活码输入页。桌面端必须通过 Control API health / preflight 检查 PostgreSQL、storage、Python runtime 和 Worker 状态；如果数据库未就绪，只展示后端 preflight 错误和修复建议。不要让 Electron 直接访问 PostgreSQL、执行 migrations、定义数据库表、初始化业务 storage、调用 Python 脚本或 FFmpeg。不要接真实模型，不要读取真实媒体文件，不要触发 FFmpeg，不要生成真实 MP4/WAV/SRT/ZIP，不要引入 Linux/Docker/Redis/Celery 作为生产依赖。Stage 14 不要绕过 Control API / Worker 边界。
```
