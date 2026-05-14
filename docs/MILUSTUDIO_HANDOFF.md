# MiLuStudio 短棒交接

更新时间：2026-05-14  
工作目录：`D:\code\MiLuStudio`

本文件只保留下一棒需要立刻接住的上下文。历史交接明细已归档到 `docs\archive\MILUSTUDIO_HANDOFF_ARCHIVE_2026-05-14_before_trim.md`。  
长期规划看 `docs\MILUSTUDIO_BUILD_PLAN.md`。阶段安排看 `docs\MILUSTUDIO_PHASE_PLAN.md`。修改记录和自检记录看 `docs\MILUSTUDIO_TASK_RECORD.md`。

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
Phase: Post Stage 17
Status: pending Stage 18 confirmation
Owner: current / next session
Goal: Stage 17 已完成生产控制台可编辑能力；下一棒先确认 Stage 18 的正式编号与范围。
```

## 已完成摘要

- Stage 0-1：独立仓库、根文档、产品规格、参考清单和 React / Vite Web 壳已就位。
- Stage 2-4：.NET Control API、生产任务状态机、SSE、Python Skills Runtime、统一 CLI 和 `SkillGateway` 已就位。
- Stage 5-11：`story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible -> storyboard_director -> image / video / voice / subtitle / auto_editor -> quality_checker` deterministic envelope 链路已就位。
- Stage 12-14：PostgreSQL / EF Core provider、migration、Worker durable claiming、真实 task output 写回、真实用户输入保存、checkpoint approve / reject / notes、重新生成淘汰旧 job、skill run 保留策略和集成脚本已完成。
- Stage 15：Electron + electron-builder + NSIS 桌面打包已完成，桌面端只承载 Web UI、启动本地 Control API / Worker、注入 API base URL 和桌面令牌，不拥有数据库 schema 或 migration。
- Stage 16：本地 deterministic 账号注册、登录、会话刷新、退出和设备绑定已完成；许可证、激活码、付费码和商业授权体验已撤下，作为后续大后期内容。
- Stage 17：生产控制台分镜编辑、单镜头备注驱动本地重算、下游任务 reset 和审核后重算边界已完成。

## 当前最近补丁

- 生产控制台已中文化：内部 skill 名、英文 stage label、任务状态和字段名默认不再直接暴露给普通用户。
- checkpoint 审核 UI 已补“本步产出 / 检查什么 / 通过后 / 拒绝后”，退回修改会引导先填写备注。
- 当前审核区和右侧结果卡都会显示真实产物预览；技术字段折叠到“技术详情”。
- Control API 正常连接且项目尚未开始时，不再显示 mock 阶段状态或 mock 结果卡；真实结果卡只在 task output 写入后按队列顺序逐步追加。
- 点击开始生成或重新生成会先保存当前输入，再创建新 job，并把同项目未完成旧 job 标记为被取代，避免旧剧本结果污染新输入。
- `story_intake` 已补中文人物句式优先抽取和自然物过滤；《牡丹亭》fixture 会输出 `杜丽娘 / 春香 / 柳梦梅`，不再把 `柳枝垂 / 花影像 / 她倚` 当角色。
- 中文测试剧本 fixture 位于 `docs\test-fixtures\scripts`，包含 Project Gutenberg《牡丹亭》完整文本和 500 字测试输入样本。
- `storyboard_director` 已新增 `cinematic_md_v1` 分镜稿结构和专用 UI 预览：`film_overview`、`storyboard_parts`、`rendered_markdown`、`validation_report`；下游仍继续消费原 `shots`。
- 真实分镜结果卡已支持编辑镜头时长、场景、画面动作、景别、镜头运动、声音、对白和旁白；保存走 `PATCH /api/generation-tasks/{taskId}/storyboard`。
- 单镜头重算走 `POST /api/generation-tasks/{taskId}/storyboard/shots/{shotId}/regenerate`，必须填写备注；当前只做 deterministic envelope 重写，不接真实模型或媒体。
- 保存或重算后，storyboard task 回到 review，job 回到分镜审核暂停态，分镜后的下游 task 清回 waiting / null，等待审核通过后由 Worker 重新消费。

## 固定约束

- 不接真实模型 provider。
- 不读取真实媒体文件。
- 不触发 FFmpeg。
- 不生成真实 PNG / MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。
- 不让 UI 或 Electron 直接访问 PostgreSQL、业务文件系统、模型 SDK、Python 脚本或 FFmpeg。
- UI / Electron 只能通过 Control API / DTO / SSE 与业务系统通信。
- Worker 只能通过 Control API / repository / Python Skills Runtime 边界推进，不让前端绕过 Worker。
- Electron 只做桌面宿主、安装器和本地进程管理；不执行 migrations，不定义数据库表，不负责数据库初始化。
- Stage 13 起默认开发配置使用 PostgreSQL，连接本机 PostgreSQL 18 的 `root/root` 和业务库 `milu`；InMemory 只作为显式快速 smoke / 特殊轻量场景保留。
- 当前 Web / Desktop 已撤下许可证、付费码和激活码体验；商业授权、套餐、云端授权和离线签名许可证后置。
- 任务栏固定不做静默强制 pin；只能通过正确 AppUserModelID、图标、快捷方式和用户引导，让用户自行固定。
- 所有依赖、配置、缓存、运行数据、日志、上传素材和生成结果必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录。
- 继续使用 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束 D 盘环境。
- 本机 PATH 前面可能有 C 盘 WindowsApps `python.exe`；运行 Python 必须显式使用 `D:\soft\program\Python\Python313\python.exe`、`$env:MILUSTUDIO_PYTHON` 或项目内 D 盘 venv。
- 每次修改完成后必须检查根 `README.md` 是否需要同步更新。
- 根 `README.md` 必须使用中文、PowerShell 友好，并采用面试展示导向结构。

## 当前技术债

- 真实 Text / Image / Video / Audio / Edit provider adapter 均未实现。
- 严格 120 秒、50-55 镜头、逐句对白保真、10-14 部分长分镜模式仍未实现，需要后续 TextProvider / 模型 adapter、对白校验器、长文本分镜 validator 和 UI 分页/折叠。
- 前端分镜表已支持编辑现有镜头和单镜头备注重算；增删镜头、批量 diff、角色 / 画风 / 提示词编辑仍未实现。
- 真实图片文件写入、资产持久化、前端选图和重试未实现。
- 真实视频文件写入、前端视频预览、单镜头重试和 FFmpeg render adapter 未实现。
- 真实 TTS、音色试听、BGM / SFX、音量标准化、字幕文件落盘和最终 MP4 下载区未实现。
- `quality_checker` 当前只检查 deterministic 结构，不读取真实媒体，不做黑屏、卡顿、水印、音量或字幕烧录检测。
- `export_packager` 当前只输出导出包占位结构，不代表真实文件已落盘。
- electron-builder / NSIS 已能生成本机安装包，但正式发布前仍需干净 Windows 安装 / 卸载 / 自启动 / 快捷方式验收和 Authenticode 代码签名。

## 最近验证

```powershell
$env:PYTHONPATH='D:\code\MiLuStudio\backend\sidecars\python-skills'
C:\Users\10045\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe -m unittest discover -s backend\sidecars\python-skills\tests -v

Push-Location D:\code\MiLuStudio\apps\web
npm run build
Pop-Location

. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
dotnet build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage17StoryboardEditing.ps1
```

最近结果：

- Python skills 全量 unittest：30 tests OK。
- Stage 14 skill contract drift：通过。
- Web build：通过。
- .NET build：通过。
- Stage 17 storyboard editing integration：通过；最新完成 job `job_e1ab06449be24902a372403244ed911f`。
- 当前本机仍有 Web `127.0.0.1:5173` 在监听；Stage 17 验收脚本结束时已清理它启动的 Control API / Worker 进程。

## 下一步建议

1. 先由用户确认 Stage 18 的正式编号与范围。
2. 候选方向一：真实 provider adapter 前配置页，包含供应商、密钥、能力开关、成本边界和 preflight，但仍不接真实模型。
3. 候选方向二：正式代码签名与干净 Windows 安装 / 卸载 / 自启动 / 快捷方式验收。
4. 候选方向三：继续扩展生产控制台编辑能力，包含角色、画风、图片提示词、视频提示词、增删镜头和批量 diff。
5. 暂不把商业授权、激活码、套餐计费放回当前 MVP 主线。

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\README.md、D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0 到 Stage 17 已完成；当前 Web / Desktop 已撤下许可证、激活码和付费码体验；生产控制台已完成中文化、真实产物预览、重新生成消费当前输入、未开始状态去 mock、《牡丹亭》fixture、storyboard_director 的 cinematic_md_v1 分镜稿结构，以及 Stage 17 分镜表编辑、单镜头备注驱动本地重算、下游任务 reset 和审核后重算边界。请先确认 Stage 18 的正式编号与范围。建议候选方向：真实 provider adapter 前配置页、正式代码签名与干净 Windows 安装验收、继续扩展生产控制台角色/画风/提示词编辑能力。后续仍不得接真实模型、不得读取真实媒体文件、不得触发 FFmpeg、不得生成真实 MP4/WAV/SRT/ZIP，不引入 Linux/Docker/Redis/Celery，不让 UI 或 Electron 绕过 Control API / Worker 边界，不让桌面端执行 migrations、定义数据库表或负责数据库初始化。
```
