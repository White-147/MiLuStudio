# MiLuStudio 短棒交接

更新时间：2026-05-15
工作目录：`D:\code\MiLuStudio`

本文件只保留下一棒需要立刻接住的上下文。
Stage 0-20 完成摘要已归档到 `docs\archive\MILUSTUDIO_COMPLETED_STAGES_ARCHIVE_2026-05-14_stage0-20.md`。
更早的长交接原文已归档到 `docs\archive\MILUSTUDIO_HANDOFF_ARCHIVE_2026-05-14_before_trim.md`。

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
Phase: Stage 23C
Status: pending
Owner: next confirmed implementation session
Stage23A update: provider 设置已改为 OpenAI-compatible baseUrl + API key + 连接测试；FFmpeg 项目内 runtime 已可通过稳定多源脚本安装；工作台已接真实上传、文本 / DOCX / 图片 / 视频技术解析和审核回退。
Stage23B update: 已落地 stage23b asset metadata、文本 / DOCX / PDF chunk manifest、PDF 嵌入文本探测、DOC 结构化降级、图片 OCR runtime 调用路径、图片 preview、视频抽帧、review proxy 策略、后端可恢复分片上传 API 和 asset analysis 解析详情消费接口。
Stage23B-P0 update: 已完成 SQLite 本地持久化与开发稳定化补丁：默认 API / Worker / Desktop runtime 使用 SQLite；移除 Npgsql provider 和 PostgreSQL 初始化脚本；SQLite preflight / migration service / Worker claiming 已收敛到后端边界；Stage 23B 验证脚本改为临时 SQLite 文件并收窄进程清理范围；新增 `/api/system/dependencies` 作为依赖中心后端契约入口。
Stage23B-P1 decision: 已完成检查与切分判断。PostgreSQL 清理 1/2/3/4 和 Web dev 后端启动编排都属于 SQLite 迁移后的稳定化收尾，应合并进入 Stage 23B-P2；原 OCR runtime 固化、PDF rasterizer、DOC/PDF 深度解析、工作台详情和生产链路消费不取消。
Stage23B-P2 update: 已完成 SQLite 迁移后的稳定化收尾：删除旧 PostgreSQL SQL migration / setup 文档，清理 Python skill 示例和当前文档口径，清理旧生成物并重新生成 Desktop runtime，新增 Web dev 本地服务启动 / 停止脚本与 `npm run dev:local`，并将 Control API 不可达展示从普通登录态中拆出。
Stage23B-P3 update: 已完成设置入口收口：删除主侧栏遗留“模型”入口；在左下设置菜单的“模型”下方、“剩余额度”上方新增“依赖”入口；新增 Web 依赖面板，通过 Control API `/api/system/dependencies` 展示 SQLite、storage、uploads、FFmpeg、OCR、Python runtime 和 skills root 状态；后续已把“诊断”归入设置二级菜单第一项，并在左侧栏顶部补齐“麋鹿”品牌栏，项目区置于品牌栏下方；Stage 23C 主线任务继续顺延，不覆盖。
Stage23B-P4 update: 已完成工作台 UI 视觉轻量化补丁：设置菜单、历史项目条目、开始生成按钮、设置弹窗、模型配置、依赖和诊断面板统一降低文本字重、按钮填充、边框、圆角和卡片重量；右侧进度卡片的节奏保持为当前基准。
Stage23B-P5 update: 已完成 composer 与品牌栏细节收尾：左侧“麋鹿”logo / 字样减重；制作要求输入区加高并改为“上传剧本文档后，写下本次制作要求”；移除当前工作台旧 500-2000 字限制、`0/2000` 计数和自动截断；新项目只有添加故事文本附件后才启用生成；上传菜单展示文本 50 MB、图片 50 MB、视频 1 GB 的真实文件大小限制，不新增文本字数硬限制。
Stage23B-P6 update: 已完成右侧进度卡顶部状态去重、左侧项目栏桌面 / 移动端拖拽宽度，以及 Codex 式完全收缩 / 左上边缘展开按钮；宽度和收缩状态写入 localStorage；Stage 23C 主线任务继续顺延，不覆盖。
Stage23C scope: `Stage 23B-*P` 为补丁阶段；下一步恢复到正式 Stage 23C，继续 Tesseract-compatible OCR runtime 固化和正向验证、PDF rasterizer、DOC/PDF 深度解析、工作台详情展示、生产链路实际消费 asset analysis endpoint 和媒体派生策略回归。
```

## 当前已完成主线

- Stage 0-20 已完成，详见归档摘要；Stage 21-22 完成摘要记录在本文件和任务记录中。
- Web / Desktop 已撤下许可证、激活码和付费码体验。
- Control API、Worker、SQLite 本地持久化、Python Skills、Electron 桌面宿主和 Windows 安装验收主边界已就位；PostgreSQL 不再作为后续产品路线保留。
- 生产链路仍是 deterministic JSON envelope，不接真实模型生成；Stage 23A 已允许真实上传文件、技术解析和 FFmpeg 缩略图 / 抽帧派生文件，Stage 23B 已补 chunk manifest、PDF embedded text probe、DOC / OCR 降级、媒体派生 metadata、可恢复分片上传后端 API 和 analysis 解析详情消费接口。
- Web 主入口为 Codex 式工作台：左侧历史项目、中央输入框、右侧固定流程进度与生成结果、左下角设置。
- 工作台已支持编辑 `character_bible`、`style_bible`、`image_prompt_builder` 和 `video_prompt_builder` 的白名单结构化字段。
- Provider adapter 已有 Stage 22 安全前置层：metadata-only secret store、spend guard 和 provider sandbox；真实 provider 调用仍被阻断。

## 最近补丁

- 工作台上传体验已从“上传”按钮改为 Codex 同款加号菜单。
- 点击加号后显示“文本 / 图片 / 视频”三类入口。
- 菜单按当前 job stage 启用或禁用类型，并在禁用项上给出阶段原因。
- 新项目 / 故事解析 / 短剧改编 / 脚本生成阶段只允许文本。
- 角色设定 / 画风设定 / 图片资产阶段允许文本和图片。
- 分镜审核 / 视频提示词 / 视频片段 / 粗剪计划 / 质量检查阶段允许文本、图片和视频。
- 配音任务 / 字幕结构阶段只允许文本。
- 排队中、导出占位、已完成和失败状态禁用新附件。
- 文本附件在故事来源阶段可作为剧本文本；后续阶段的文本、图片、视频只作为参考附件元数据。
- Stage 23A 起，图片 / 视频可通过 Control API 上传到项目资产，后端 Infrastructure adapter 可调用项目内 FFmpeg 做 ffprobe、缩略图和最多 8 张抽帧。
- 已新增已完成阶段归档：`docs\archive\MILUSTUDIO_COMPLETED_STAGES_ARCHIVE_2026-05-14_stage0-20.md`。
- Stage 21 新增 `PATCH /api/generation-tasks/{taskId}/structured-output`。
- Stage 21 保存结构化编辑时只写回 task output JSON envelope，追加 `stage21_edit_summary`，并重置当前任务之后的下游任务。
- 角色 / 画风编辑后回到 review / paused；图片提示词 / 视频提示词编辑后为 completed / running，等待下游重算。
- Web 结果面板已加入角色、画风、图片提示词和视频提示词编辑表单、字段 diff 和保存入口。
- Stage 22 新增 `GET /api/settings/providers/safety` 和 `POST /api/settings/providers/spend-guard/check`。
- Stage 22 provider settings 响应新增 `safety`，preflight 新增 `secret_store`、`spend_guard` 和 `provider_sandbox` 检查。
- Stage 22 新增 `IProviderSecretStore` 和 `FileProviderSecretStore`；当前只保存遮罩、SHA256 指纹和不可调用 secret reference，不保存明文 key。
- Web “模型”设置页已展示 Stage 22 安全前置层和真实 provider 调用阻断状态。
- 新增 `scripts\windows\Test-MiLuStudioStage22ProviderSafety.ps1`。
- Stage 23A 已新增 provider `baseUrl` 配置、OpenAI-compatible 中转供应商、DPAPI 本地加密 secret material 和 `POST /api/settings/providers/{kind}/connection-test`。
- Web 模型设置页已加入 Base URL 输入和“测试连通”按钮；工作台主侧栏遗留“模型”入口已移除，左下设置菜单内的 provider 入口保留为“模型”。
- `scripts\windows\Install-MiLuStudioFfmpeg.ps1` 已加入多源稳定下载，优先 `gyan.dev` 固定 release essentials 链接，BtbN GitHub `/latest/` 作为 fallback。
- 当前本机已安装 FFmpeg 到 `D:\code\MiLuStudio\runtime\ffmpeg`，该目录被 `.gitignore` 排除，Git 只提交安装脚本和运行时发现逻辑。
- Stage 23A 新增 `POST /api/projects/{projectId}/assets/upload`，保存原文件、计算 SHA256、登记 `assets`，并返回技术解析 metadata。
- 文本 / Markdown / JSON / SRT / ASS / VTT / CSV / XML / YAML / RTF 可直接解析文本；DOCX 通过 ZIP + XML 提取正文；DOC / PDF 当前先保存并标记后续解析器 / OCR 待补。
- 图片和视频通过项目内 FFmpeg runtime 执行 ffprobe、生成缩略图；视频额外按上限抽帧到项目 `uploads` 目录。
- 工作台附件卡改为“待上传解析 / 已上传解析 / 上传失败”，新项目输入框作为制作要求；故事正文优先来自上传解析文本。
- 新增 `POST /api/production-jobs/{jobId}/rollback`；最近一个已确认审核步骤可二次确认后回到待审核，并清空下游任务输出。
- Stage 23B 已正式确认，不再需要下一棒先确认编号：本阶段只补上传后解析、切片、分片、压缩和抽帧地基。
- Stage 23B 第一轮已把资产 metadata 升级为 `stage23b_asset_analysis_v1`，记录上传分片契约、no-provider 边界、technical metadata、派生文件和 limits。
- 文本 / DOCX / PDF 嵌入文本解析结果已生成 `contentBlocks` 和 `chunkManifest`；DOCX ZIP entry 已兼容 Windows 反斜杠路径。
- PDF 已有轻量 embedded text probe；扫描版或复杂 PDF 会记录 `ocr_required` 和 warnings，不阻塞上传。
- DOC 当前记录 `parser_unavailable` 和后端 converter runtime 建议；图片 OCR 已接 Tesseract-compatible CLI 调用路径，runtime 可用时会生成 `image_ocr` 文本切片，runtime 缺失时记录 `runtime_not_configured`。
- 图片会由后端 FFmpeg adapter 生成 `thumbnail.jpg` 与 `preview_1280.jpg`；视频会按时长抽帧并尝试生成短 `review_proxy_720p.mp4`，仍不是最终导出。
- 新增 `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1`，验证 txt / DOCX / PDF / DOC / PNG / MP4 上传解析、chunk manifest、结构化降级、媒体派生和 no-provider 边界。
- 新增 `POST /api/projects/{projectId}/assets/upload-sessions`、`GET /api/projects/{projectId}/assets/upload-sessions/{sessionId}`、`PUT /api/projects/{projectId}/assets/upload-sessions/{sessionId}/chunks/{chunkIndex}` 和 `POST /api/projects/{projectId}/assets/upload-sessions/{sessionId}/complete`。
- 分片 session manifest 和 chunks 保存到 `uploads\.upload-sessions`；complete 后复用现有 `ProjectAssetUploadService` 保存、解析和登记 assets，metadata 记录 `upload.mode=control_api_resumable_chunks`。
- 新增 `GET /api/projects/{projectId}/assets/{assetId}/analysis` 和 `ProjectAssetAnalysisService`，只读取 `assets.metadata_json`，向工作台详情或后续生产链路返回 parser、OCR summary、content blocks、chunk manifest、upload policy 和 no-provider 边界。
- analysis endpoint 不读取媒体文件、不执行 FFmpeg、不调用 provider；OCR 候选路径和派生文件本地路径不会作为 UI 可消费契约返回。
- 新增 `scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1`，验证 1MB chunk 乱序上传、resume status、complete、chunk manifest、analysis endpoint 和 no-provider 边界。
- Stage 23B-P0 已把默认开发配置、API / Worker appsettings、EF provider、migration / preflight、Worker durable claiming、桌面 runtime 路径和 Stage 14 / 16 / 17 / 21 / 23B 验证脚本切到 SQLite。
- 已移除运行时 Npgsql package reference、PostgreSQL repository / auth repository / migration / preflight 实现和 `Initialize-MiLuStudioPostgreSql.ps1`。
- 新增 `GET /api/system/dependencies`，返回 SQLite / storage / uploads / FFmpeg / OCR / Python / skills 等依赖检查状态，并标记安装策略为 bundled/offline 优先、在线下载仅辅助。
- Stage 23B-P1 已检查确认：后端用全新临时 SQLite 在 5368 可启动，`/api/auth/me` 可成功返回未登录状态并自动初始化 schema；截图中的 `Control API 未连接` 来自浏览器 Web dev 只启动 Vite、没有同时编排 5368 Control API，不是 SQLite 后端启动失败。
- Stage 23B-P2 已定为收尾稳定化阶段：PostgreSQL 残留清理和 Web dev 本地服务启动编排合并处理，避免旧文档 / 旧 runtime / 单独 Web 入口继续误导。
- Stage 23B-P2 已完成：旧 SQL migration 文件和 `docs\POSTGRESQL_STAGE12_SETUP.md` 已删除；Python skill README / examples 已改为 SQLite / 后端持久化边界口径；`.tmp`、`apps\desktop\runtime`、`outputs\desktop` 旧生成物已清理，Desktop runtime 已重新生成且文本配置不含 Npgsql / PostgreSQL 残留。
- 新增 `scripts\windows\Start-MiLuStudioLocalServices.ps1`、`Stop-MiLuStudioLocalServices.ps1`、`Start-MiLuStudioWebDev.ps1`，以及 `apps\web` 的 `dev:local`、`services:start`、`services:stop`；启动脚本会启动 5368 Control API / Worker 并应用后端初始化，停止脚本只停自己记录且命令行匹配的进程。
- Web `control_api_unavailable` 已改成“本地服务未连接”专用页面，显示 Control API 地址和重试入口；普通未登录仍进入登录 / 注册入口。
- Stage 23B-P3 已完成设置入口收口：左下设置菜单顺序为诊断、个人账户、模型、依赖、剩余额度、退出登录；“依赖”面板只调用 Control API 依赖检测接口，不让 Web / Electron 直接访问 SQLite、文件系统、FFmpeg 或 OCR。
- Stage 23B-P3 后续 UI 收口：工作台顶部命令区移除“新项目 / 搜索 / 诊断”文字按钮；左侧栏顶部展示“麋鹿”品牌栏，其下显示“项目”，项目标题右侧仅保留图标，从右往左第一个是新项目，第二个是搜索。
- Stage 23B-P4 已完成视觉轻量化：共享按钮、左下设置菜单、历史项目条目、设置弹窗、Provider / 依赖 / 诊断设置面板改为更轻的字重、弱边框、弱背景和更紧凑控件；未改变 Control API / Electron 边界。
- Stage 23B-P5 已完成 composer 与品牌栏收尾：新项目必须先添加故事文本附件才启用生成；当前工作台不再显示或执行旧 `0/2000` / `500-2000` 输入限制；上传菜单展示文本 50 MB、图片 50 MB、视频 1 GB 的真实文件大小限制；生成按钮改为 composer 专用轻按钮，品牌栏视觉重量降低。
- Stage 23B-P6 已完成侧栏与进度卡收尾：右侧进度摘要不再重复显示“未开始 / 进行中”等任务状态，阶段状态只由流程项右侧标签表达；左侧项目栏支持拖拽宽度、键盘调整、完全收缩和左上边缘展开。

## 固定约束

- Stage 23 起，真实上传、技术解析、OCR、FFmpeg 缩略图 / 抽帧 / 转码和最终媒体文件生成可以继续推进，但必须通过 Control API / Application service / Infrastructure adapter / Worker / Python Skills Runtime 边界实现。
- Stage 23A 已允许 provider 外网连接测试；真实故事 / 图片 / 视频 / 音频生成调用仍未接入，后续必须先补审计、预算、超时和失败隔离。
- FFmpeg 只允许使用 `D:\code\MiLuStudio\runtime\ffmpeg` 或明确配置的 D 盘工具路径；UI / Electron 不得直接执行 FFmpeg。
- API key 只允许通过本地安全存储读取，Web / Electron 响应中不得回显明文 key。
- 实际编码必须继续符合高内聚、低耦合、职责单一、关注点分离和依赖倒置。
- 新增真实上传、解析、FFmpeg 或 provider 能力时，UI / Electron 仍只能消费 Control API / DTO / SSE，不直接接触文件系统、模型 SDK、Python 脚本或 FFmpeg。
- 业务编排、文件解析、外部工具调用和 provider 调用必须分别落在 Application service、Infrastructure adapter、Worker gateway 或 Python Skills Runtime 的清晰边界内。
- 新增抽象只服务真实变化点、测试隔离或重复逻辑收敛；必要的 MVP 直连必须限制在单一 adapter / gateway 内，并记录技术债。
- 不接真实模型生成 provider。
- UI / Electron 不读取真实媒体文件、不触发 FFmpeg；后端 Infrastructure adapter 可以读取上传文件并调用项目内 FFmpeg 做技术解析。
- 当前不生成最终真实 MP4 / WAV / SRT / ZIP；缩略图和抽帧只作为上传解析派生文件保存。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。
- UI / Electron 只能通过 Control API / DTO / SSE 与业务系统通信。
- 不让 UI 或 Electron 直接访问 SQLite、业务文件系统、模型 SDK、Python 脚本或 FFmpeg。
- Worker 只能通过 Control API / repository / Python Skills Runtime 边界推进。
- Electron 只做桌面宿主、安装器和本地进程管理；不执行 migrations，不定义数据库表，不负责数据库初始化。
- Stage 23B-P0 起数据库路线全面转向 SQLite；InMemory 只作为显式 smoke / 特殊轻量场景保留。
- 安装包尽量自带可控 runtime；设置中的依赖中心负责检测、修复、启用、禁用和导入离线包；在线下载只作为用户明确触发的辅助修复路径，不作为基础可用性前提。
- 继续使用 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束 D 盘环境。
- Python 必须显式使用 D 盘解释器、`$env:MILUSTUDIO_PYTHON` 或项目内 D 盘 venv。
- 每次修改后检查根 `README.md` 是否需要同步。

## 当前技术债

- 真实 Text / Image / Video / Audio / Edit provider adapter 均未实现。
- Provider settings 已开始支持 OpenAI-compatible baseUrl、DPAPI 本地加密 key 和轻量连接测试；真实 provider 生成 adapter、dry-run 审计、预算流水和失败隔离仍未实现。
- 工作台角色 / 画风 / 图片提示词 / 视频提示词基础编辑已完成；批量操作、镜头增删、更细粒度 diff 和高级重算策略仍待推进。
- 真实音频、字幕、导出包文件写入和下载区未实现。
- 真实上传和基础技术解析已实现；Stage 23B 已补稳定 metadata、文本切片、PDF 嵌入文本探测、DOC 降级、图片 OCR runtime 调用路径、图片 preview、视频抽帧 / proxy、可恢复分片上传 endpoint 和 analysis 消费接口；Stage 23C 继续处理 Tesseract runtime 安装 / 正向验证、PDF rasterizer、DOC/PDF 更深解析、工作台详情展示和生产链路实际接入。
- 设置“依赖”只读检测 UI 已实现；离线包导入、修复动作和启用 / 禁用状态管理仍待后续通过 Control API / 后端 adapter 补齐。
- `quality_checker` 不读取真实媒体，不做黑屏、卡顿、水印、音量或字幕烧录检测。
- 正式发布仍需要真实 Authenticode 证书和干净 Windows 虚拟机签名回归。

## 最近验证

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

Push-Location D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run prepare:runtime
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run services:start
D:\soft\program\nodejs\npm.ps1 run services:stop
Pop-Location

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Install-MiLuStudioFfmpeg.ps1

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1 -SkipPrepareRuntime

git diff --check
```

最近结果：

- Web build：通过。
- Stage 23B-P3 Web source check：通过，Vite 当前源码中左侧栏顶部展示“麋鹿”品牌栏，项目区位于其下，设置菜单包含诊断和依赖入口。
- Stage 23B-P3 dependencies API check：通过，`/api/system/dependencies` 返回 `repositoryProvider=SQLite` 和 10 个依赖检查项。
- Stage 23B-P4 Web build / Vite CSS check：通过，当前源码包含轻量化后的 settings menu、composer primary button、settings sheet、provider summary 和 preflight panel 样式。
- Stage 23B-P5 Web build / source check：通过，当前源码已移除 workspace composer 旧字数计数 / 截断逻辑，并包含 `composer-submit-button`、真实上传大小提示和减重后的 `workspace-brand` 样式；`git diff --check` 仅输出 LF/CRLF 换行提示。
- Stage 23B-P6 Web build / source check：通过，当前源码包含 `workspace-sidebar-resizer`、`workspace-sidebar-collapse-button`、`workspace-sidebar-expand-button` 和进度摘要去重后的 `project-progress-summary`；`git diff --check` 仅输出 LF/CRLF 换行提示。
- .NET build：通过。
- Python compileall / unittest：通过，30 tests。
- Web services:start / services:stop：通过，5368 `/health` 返回 SQLite provider，停止脚本清理 API / Worker。
- Stage 23B-P0 SQLite smoke：通过，临时 SQLite 文件完成 health、migration apply、账号注册、依赖中心接口和数据库文件创建。
- Stage 14 integration：通过，临时 SQLite API / Worker 完成 deterministic job。
- Stage 16 auth：通过，账号 / 会话集成通过。
- Desktop runtime prepare：通过，重新生成 `apps\desktop\runtime`；runtime 文本配置无 Npgsql / PostgreSQL 残留。
- Desktop TypeScript build：通过。
- Desktop API security：通过，桌面模式 token / migration 边界仍有效。
- Stage 23A provider connection test smoke：通过，临时 InMemory Control API 返回可预期的 HTTP 404 测试结果且未发送 generation payload。
- FFmpeg install：通过，`ffmpeg-release-essentials.zip` SHA256 sidecar 校验通过，已安装 `ffmpeg.exe` / `ffprobe.exe` 到 `D:\code\MiLuStudio\runtime\ffmpeg\bin`。
- Stage 23A upload smoke：通过，临时 InMemory Control API 成功上传 txt，返回 `story_text`、SHA256 和解析正文长度，未发送 generation payload。
- Stage 23B asset parsing：通过，临时 SQLite Control API 成功上传 txt / DOCX / PDF / DOC / PNG / OCR PNG / MP4，验证 chunk manifest、analysis endpoint、DOC parser_unavailable、PDF embedded text、OCR runtime 缺失降级、媒体派生 metadata 和 no-provider 边界。
- Stage 23B chunked upload：通过，临时 SQLite Control API 完成 session 创建、乱序 chunk、resume status、complete、asset 解析、`control_api_resumable_chunks` metadata 和 analysis endpoint 验证。
- `git diff --check`：通过，仅有 CRLF 换行提示。
- Playwright 依赖不在当前 web workspace，截图级自动化检查未执行。

## 下一步建议

1. Stage 23C：固化 Tesseract-compatible runtime 安装和正向 OCR 验证；继续 PDF rasterizer、DOC/PDF 深度解析、工作台详情展示和生产链路实际消费 analysis endpoint。
2. 分片上传 UI 尚未接入；前端后续必须只调用 Control API upload-sessions，不得直接访问 chunks 或文件系统。
3. 设置“依赖”的修复动作、离线包导入和启用 / 禁用状态管理尚未实现；只能通过 Control API / 后端 adapter 补齐，Electron 不直接扫描业务目录或执行 FFmpeg / OCR。
4. Stage 23D 再做 provider adapter dry-run / audit contract，只做接口契约、审计日志、预算流水和失败隔离，不发送真实生成请求。
5. Stage 24 再细化工作台高级编辑：提示词批量操作、镜头增删、更细粒度 diff 和重算策略。
6. 拿到正式证书后再做 `verify:release:signed` 和干净 Windows 虚拟机安装 / 卸载回归。

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\README.md、D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0 到 Stage 22 已完成，Stage 0-20 完成摘要已归档到 docs\archive\MILUSTUDIO_COMPLETED_STAGES_ARCHIVE_2026-05-14_stage0-20.md；当前 Web / Desktop 已撤下许可证、激活码和付费码体验；Web 主入口是 Codex 式工作台：左侧历史项目、中央制作要求输入框、右侧固定流程进度和生成结果、左下角设置。Stage 23A 已推进 OpenAI-compatible baseUrl + API key、Windows DPAPI 本地加密 key、连接测试、项目内 FFmpeg runtime 安装脚本、真实上传资产保存、文本 / DOCX / 图片 / 视频技术解析和审核回退。Stage 23B 已落地 stage23b asset metadata、文本 / DOCX / PDF chunk manifest、PDF 嵌入文本探测、DOC 结构化降级、图片 OCR runtime 调用路径、图片 preview、视频抽帧、review proxy、后端可恢复分片上传 API 和 asset analysis 解析详情消费接口。Stage 23B-P0 已完成：默认开发 / 桌面数据库全面转向 SQLite 本地文件数据库，不再保留 PostgreSQL 后续路线；API / Worker / Desktop runtime / 验证脚本已切 SQLite；依赖中心已有 Control API 检测契约，安装包路线以自带可控 runtime 和离线包为优先，在线下载只作为辅助；阶段验证脚本已收窄进程清理范围，避免误杀 5368 开发后端。Stage 23B-P1 已完成检查与切分判断：截图中的 Web 登录页问题来自浏览器 Web dev 只启动 Vite、没有编排 5368 Control API，不是 SQLite 后端启动失败。Stage 23B-P2 已完成：旧 PostgreSQL SQL migration / setup 文档删除，Python skill 示例和当前文档口径清理，旧 `.tmp` / Desktop runtime / desktop outputs 清理后重新生成 Desktop runtime；新增 Web dev 本地服务启动 / 停止脚本与 `npm run dev:local`，Web Control API 不可达时显示专用提示页。Stage 23B-P3 已完成设置入口、依赖入口、项目区快捷按钮、诊断归拢和“麋鹿”品牌栏补齐。Stage 23B-P4 已完成工作台 UI 视觉轻量化：设置菜单、历史项目、开始生成按钮、模型配置、依赖和诊断面板统一降低字重、边框、圆角、按钮填充和卡片重量。`Stage 23B-*P` 均为补丁阶段，下一步做正式 Stage 23C：继续 Tesseract-compatible OCR runtime 固化和正向验证、PDF rasterizer、DOC/PDF 深度解析、工作台详情展示、生产链路实际消费 asset analysis endpoint 和媒体派生策略回归。后续仍不得接真实模型生成，不引入 Linux/Docker/Redis/Celery，不让 UI 或 Electron 绕过 Control API / Worker 边界，不让桌面端执行 migrations、定义数据库表或负责数据库初始化；UI / Electron 仍不得直接读取媒体、执行 FFmpeg、执行 OCR 或访问文件系统，真实上传、OCR、FFmpeg、SQLite 初始化和依赖检测只能在后端 adapter / Control API 边界内执行。
```

历史参考（Stage 23B-P0 插入前，勿作为下一步）：

```text
读取 D:\code\MiLuStudio\README.md、D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0 到 Stage 22 已完成，Stage 0-20 完成摘要已归档到 docs\archive\MILUSTUDIO_COMPLETED_STAGES_ARCHIVE_2026-05-14_stage0-20.md；当前 Web / Desktop 已撤下许可证、激活码和付费码体验；Web 主入口是 Codex 式工作台：左侧历史项目、中央制作要求输入框、右侧固定流程进度和生成结果、左下角设置。Stage 23A 已推进：模型配置支持 OpenAI-compatible baseUrl + API key、Windows DPAPI 本地加密 key、连接测试；FFmpeg 可通过 scripts\windows\Install-MiLuStudioFfmpeg.ps1 稳定安装到 D:\code\MiLuStudio\runtime\ffmpeg；工作台附件已通过 Control API 真实上传到项目 assets，文本 / DOCX / 图片 / 视频做基础技术解析，图片 / 视频可生成缩略图，视频可抽帧；审核流程新增最近已确认步骤的回退 API 和二次确认 UI，回退后当前步骤回到待审核并清空下游任务输出。Stage 23B 已正式确认为文档 / 媒体深度解析与上传策略加固，已落地 `stage23b_asset_analysis_v1` metadata、文本 / DOCX / PDF chunk manifest、PDF 嵌入文本探测、DOC 结构化降级、图片 OCR runtime 调用路径、图片 preview、视频抽帧和 review proxy 策略，并新增 scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1；后端可恢复分片上传 API 也已落地：upload-sessions 创建、chunk 乱序上传、resume status、complete 合并并复用 assets 解析链路，新增 scripts\windows\Test-MiLuStudioStage23BChunkedUpload.ps1；asset analysis 解析详情消费接口也已落地：GET /api/projects/{projectId}/assets/{assetId}/analysis 只读 metadataJson，返回 parser、OCR summary、content blocks、chunk manifest、upload policy 和 no-provider 边界，不把 OCR 候选路径或派生文件本地路径作为 UI 契约。当前本机仍未安装可控 Tesseract runtime，下一步 Stage 23C 优先固化 `runtime\tesseract` 安装和正向 OCR 验证；如果 runtime 仍不可控，则推进 PDF rasterizer、工作台详情展示或生产链路实际消费 analysis endpoint。provider dry-run / audit contract 后置 Stage 23D，提示词批量操作、镜头增删和更细粒度 diff 后置 Stage 24，signed release 回归等正式证书到位后再做。后续仍不得接真实模型生成，不引入 Linux/Docker/Redis/Celery，不让 UI 或 Electron 绕过 Control API / Worker 边界，不让桌面端执行 migrations、定义数据库表或负责数据库初始化；UI / Electron 仍不得直接读取媒体、执行 FFmpeg 或访问文件系统，真实上传、OCR 和 FFmpeg 只能在后端 adapter 边界内执行。
```
