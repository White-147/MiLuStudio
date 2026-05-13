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
Phase: Stage 11 preparation
Status: pending
Owner: next session
Goal: 基于 Stage 10 粗剪计划继续收敛质量检查边界。
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
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.codex-tmp\control-plane-build\
Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location
```

注意：默认 Debug 输出目录构建曾因正在运行的 `MiLuStudio.Api (7832)` 锁定 DLL 失败；改用上面的临时 D 盘输出目录验证通过，临时目录已清理。

CLI examples:

```powershell
Push-Location backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill voice_casting --input skills\voice_casting\examples\input.json --output skills\voice_casting\examples\output.json --pretty
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill subtitle_generator --input skills\subtitle_generator\examples\input.json --output skills\subtitle_generator\examples\output.json --pretty
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill auto_editor --input skills\auto_editor\examples\input.json --output skills\auto_editor\examples\output.json --pretty
Pop-Location
```

## 固定约束

- 不要接真实模型 provider。
- 不要引入 Linux / Docker / Redis / Celery 作为生产依赖。
- 不要让 UI 直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- 继续通过 Control API / Worker / Python Skills Runtime 边界推进。
- 每次修改完成后必须检查根 `README.md` 是否需要同步更新。
- 根 `README.md` 必须使用中文、PowerShell 友好，并采用类似 `D:\code\BookRecommendation\README.md` 的面试展示导向结构。
- Stage 12 调整为 PostgreSQL 持久化与后端收敛，先把数据库、migration、repository adapter、Worker durable claiming 做成后端能力。
- Stage 13 才进入桌面打包，唯一方案为 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。
- Electron 只做桌面宿主、安装器和本地进程管理；现有 Web UI 和 Control API 必须继续保持可独立迭代。
- 数据库不和桌面端绑定：Electron 不直接访问 PostgreSQL，不执行 migrations，不定义数据库表，不负责数据库初始化。
- 账号系统不阻塞 Stage 13 桌面安装包 MVP，作为桌面 MVP 后下一大阶段；届时对齐 QQ 类体验：应用打开只显示登录 / 注册 / 激活入口，登录后才进入工作台。
- 安装器可加付费码 / 激活码输入页作为安装门槛，但正式授权必须由 Control API / Auth & Licensing adapter 在应用内校验。
- 任务栏固定不做静默强制 pin；必须通过正确 AppUserModelID、图标、快捷方式和用户引导，让用户自行固定。
- Stage 12 前 API/Worker 仍可能是 in-memory repository；Stage 12 完成后才要求 PostgreSQL provider 成为真实共享状态来源。
- 所有依赖、配置、缓存、运行数据、日志、上传素材和生成结果必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录。
- 继续使用 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束 D 盘环境。
- 本机 PATH 前面可能有 C 盘 WindowsApps `python.exe`；运行 Python 必须显式使用 `D:\soft\program\Python\Python313\python.exe`、`$env:MILUSTUDIO_PYTHON` 或项目内 D 盘 venv。

## 技术债

- 当前 API 运行期仍是 in-memory repository。
- PostgreSQL adapter / EF Core DbContext、migration runner、preflight 和 Worker durable claiming 尚未实现，是 Stage 12 的核心任务。
- `storage_intent` 只是 Stage 8 mock asset 的未来 adapter 目标描述，不代表真实文件已生成。
- 真实图片 provider、文件写入、资产持久化、前端选图和重试均未实现。
- `video_generation` 当前只输出 mock clip records，不代表真实 MP4 已生成。
- 真实视频 provider、视频文件写入、资产持久化、前端视频预览、单镜头重试和 FFmpeg 均未实现。
- `voice_casting` 当前只输出配音任务和逻辑音频 intent，不代表真实 WAV 已生成。
- `subtitle_generator` 当前只输出 SRT-ready 文本结构，不代表真实 `.srt` 文件已落盘。
- `auto_editor` 当前只输出粗剪 timeline / render plan，不代表 FFmpeg 已执行或 MP4 已生成。
- 真实 TTS provider、音色试听、BGM/SFX、音量标准化、字幕文件落盘、FFmpeg render adapter、最终 MP4、下载区 UI 和资产持久化均未实现。
- 账号注册、登录、会话、设备绑定、许可证和 Auth & Licensing adapter 尚未实现，只已写入规划，且不阻塞 Stage 13 桌面安装包 MVP。
- 安装器激活码输入页尚未实现，但仍属于 Stage 13 安装器能力。
- electron-builder / NSIS 安装器尚未实现；Stage 13 必须验证自定义图标、安装目录选择、桌面快捷方式、开始菜单快捷方式、开机自启动、进度条、安装完成后启动和激活码输入页。

## 下一步建议

1. 进入 Stage 11 质量检查边界。
2. 基于 Stage 10 的 rough edit timeline、subtitle cues、voice tasks、mock video clips 和上游角色 / 分镜结构实现 `quality_checker` 的内部 Production Skill 边界。
3. 先输出可审阅的问题报告、严重级别、可自动重试项和人工确认 checkpoint。
4. 不接真实视觉 / 音频检测 provider，不读取真实媒体文件，不触发 FFmpeg，不写数据库，不让 UI 绕过 Control API / Worker。
5. Stage 11 后优先进入 Stage 12 PostgreSQL 持久化与后端收敛，再进入 Stage 13 桌面打包。
6. 阶段完成后必须联网搜索自检；如发现偏差，先更新 `MILUSTUDIO_BUILD_PLAN.md`，再更新 `MILUSTUDIO_PHASE_PLAN.md`，再把原因写入 `MILUSTUDIO_TASK_RECORD.md`，再检查 / 更新根 `README.md`，最后更新本 handoff。

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0、Stage 1、Stage 2、Stage 3、Stage 4、Stage 5、Stage 6、Stage 7、Stage 8、Stage 9 和 Stage 10 已完成。按 docs\MILUSTUDIO_PHASE_PLAN.md 的 Stage 11 开始实现质量检查边界：基于现有 backend\sidecars\python-skills 的 character_bible、style_bible、storyboard_director、video_generation、voice_casting、subtitle_generator 和 auto_editor envelopes，优先收敛 quality_checker 的内部 Production Skill 边界，让角色/风格/分镜/timing/mock 视频片段/配音任务/SRT 字幕/粗剪计划能生成后续阶段可消费的质量问题报告、严重级别、可自动重试项和人工确认 checkpoint；后续保存到数据库仍留给 PostgreSQL adapter / EF Core DbContext 阶段。不要接真实视觉或音频检测模型，不要读取真实媒体文件，不要触发 FFmpeg，不要生成真实 MP4，不要引入 Linux/Docker/Redis/Celery 作为生产依赖，不要让 UI 直接访问数据库、文件系统、Python 脚本或 FFmpeg。Stage 11 不要绕过 Control API / Worker 边界。
```
