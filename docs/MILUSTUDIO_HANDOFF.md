# MiLuStudio 短棒交接

更新时间：2026-05-14  
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
Phase: Stage 23B
Status: in_progress
Owner: current / next session
Stage23A update: provider 设置已改为 OpenAI-compatible baseUrl + API key + 连接测试；FFmpeg 项目内 runtime 已可通过稳定多源脚本安装；工作台已接真实上传、文本 / DOCX / 图片 / 视频技术解析和审核回退。
Stage23B update: 第一轮已落地 stage23b asset metadata、文本 / DOCX / PDF chunk manifest、PDF 嵌入文本探测、DOC / OCR 结构化降级、图片 preview、视频抽帧和 review proxy 策略。
Stage23B next: 继续补真实 OCR runtime 调用或可恢复分片上传 endpoint；provider dry-run / audit contract 后置到 Stage 23C，工作台高级编辑后置到 Stage 24。
```

## 当前已完成主线

- Stage 0-20 已完成，详见归档摘要；Stage 21-22 完成摘要记录在本文件和任务记录中。
- Web / Desktop 已撤下许可证、激活码和付费码体验。
- Control API、Worker、PostgreSQL、Python Skills、Electron 桌面宿主和 Windows 安装验收主边界已就位。
- 生产链路仍是 deterministic JSON envelope，不接真实模型生成；Stage 23A 已允许真实上传文件、技术解析和 FFmpeg 缩略图 / 抽帧派生文件，Stage 23B 第一轮已补 chunk manifest、PDF embedded text probe、DOC / OCR 降级和媒体派生 metadata。
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
- Web 模型设置页已加入 Base URL 输入和“测试连通”按钮；左上“模型”入口已隐藏，左下设置菜单内的 provider 入口改为“模型”。
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
- DOC 当前记录 `parser_unavailable` 和后端 converter runtime 建议；OCR 当前记录 Tesseract-compatible runtime 能力检测 metadata，真实 OCR 调用仍未接。
- 图片会由后端 FFmpeg adapter 生成 `thumbnail.jpg` 与 `preview_1280.jpg`；视频会按时长抽帧并尝试生成短 `review_proxy_720p.mp4`，仍不是最终导出。
- 新增 `scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1`，验证 txt / DOCX / PDF / DOC / PNG / MP4 上传解析、chunk manifest、结构化降级、媒体派生和 no-provider 边界。

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
- 不让 UI 或 Electron 直接访问 PostgreSQL、业务文件系统、模型 SDK、Python 脚本或 FFmpeg。
- Worker 只能通过 Control API / repository / Python Skills Runtime 边界推进。
- Electron 只做桌面宿主、安装器和本地进程管理；不执行 migrations，不定义数据库表，不负责数据库初始化。
- Stage 13 起默认开发配置使用 PostgreSQL；InMemory 只作为显式 smoke / 特殊轻量场景保留。
- 继续使用 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束 D 盘环境。
- Python 必须显式使用 D 盘解释器、`$env:MILUSTUDIO_PYTHON` 或项目内 D 盘 venv。
- 每次修改后检查根 `README.md` 是否需要同步。

## 当前技术债

- 真实 Text / Image / Video / Audio / Edit provider adapter 均未实现。
- Provider settings 已开始支持 OpenAI-compatible baseUrl、DPAPI 本地加密 key 和轻量连接测试；真实 provider 生成 adapter、dry-run 审计、预算流水和失败隔离仍未实现。
- 工作台角色 / 画风 / 图片提示词 / 视频提示词基础编辑已完成；批量操作、镜头增删、更细粒度 diff 和高级重算策略仍待推进。
- 真实音频、字幕、导出包文件写入和下载区未实现。
- 真实上传和基础技术解析已实现；Stage 23B 第一轮已补稳定 metadata、文本切片、PDF 嵌入文本探测、DOC / OCR 降级、图片 preview 和视频抽帧 / proxy；真实 OCR runtime 调用、可恢复分片上传 endpoint、DOC/PDF 更深解析和 chunk manifest 下游消费仍待实现。
- `quality_checker` 不读取真实媒体，不做黑屏、卡顿、水印、音量或字幕烧录检测。
- 正式发布仍需要真实 Authenticode 证书和干净 Windows 虚拟机签名回归。

## 最近验证

```powershell
Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage23-build\

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Install-MiLuStudioFfmpeg.ps1

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1

git diff --check
```

最近结果：

- Web build：通过。
- .NET build：通过。
- Stage 23A provider connection test smoke：通过，临时 InMemory Control API 返回可预期的 HTTP 404 测试结果且未发送 generation payload。
- FFmpeg install：通过，`ffmpeg-release-essentials.zip` SHA256 sidecar 校验通过，已安装 `ffmpeg.exe` / `ffprobe.exe` 到 `D:\code\MiLuStudio\runtime\ffmpeg\bin`。
- Stage 23A upload smoke：通过，临时 InMemory Control API 成功上传 txt，返回 `story_text`、SHA256 和解析正文长度，未发送 generation payload。
- Stage 23B asset parsing：通过，临时 InMemory Control API 成功上传 txt / DOCX / PDF / DOC / PNG / MP4，验证 chunk manifest、DOC parser_unavailable、PDF embedded text、媒体派生 metadata 和 no-provider 边界。
- `git diff --check`：通过，仅有 CRLF 换行提示。
- Playwright 依赖不在当前 web workspace，截图级自动化检查未执行。

## 下一步建议

1. 继续 Stage 23B：优先补真实 OCR runtime 调用或可恢复分片上传 endpoint；二者择一小步推进，保持验证闭环短。
2. 让 chunk manifest 被后续生产链路或工作台详情稳定消费前，继续优先保持 `assets.metadata_json` schema 兼容，不急于新增 migration。
3. Stage 23C 再做 provider adapter dry-run / audit contract，只做接口契约、审计日志、预算流水和失败隔离，不发送真实生成请求。
4. Stage 24 再细化工作台高级编辑：提示词批量操作、镜头增删、更细粒度 diff 和重算策略。
5. 拿到正式证书后再做 `verify:release:signed` 和干净 Windows 虚拟机安装 / 卸载回归。

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\README.md、D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0 到 Stage 22 已完成，Stage 0-20 完成摘要已归档到 docs\archive\MILUSTUDIO_COMPLETED_STAGES_ARCHIVE_2026-05-14_stage0-20.md；当前 Web / Desktop 已撤下许可证、激活码和付费码体验；Web 主入口是 Codex 式工作台：左侧历史项目、中央制作要求输入框、右侧固定流程进度和生成结果、左下角设置。Stage 23A 已推进：模型配置支持 OpenAI-compatible baseUrl + API key、Windows DPAPI 本地加密 key、连接测试；FFmpeg 可通过 scripts\windows\Install-MiLuStudioFfmpeg.ps1 稳定安装到 D:\code\MiLuStudio\runtime\ffmpeg；工作台附件已通过 Control API 真实上传到项目 assets，文本 / DOCX / 图片 / 视频做基础技术解析，图片 / 视频可生成缩略图，视频可抽帧；审核流程新增最近已确认步骤的回退 API 和二次确认 UI，回退后当前步骤回到待审核并清空下游任务输出。Stage 23B 已正式确认为文档 / 媒体深度解析与上传策略加固，第一轮已落地 `stage23b_asset_analysis_v1` metadata、文本 / DOCX / PDF chunk manifest、PDF 嵌入文本探测、DOC / OCR 结构化降级、图片 preview、视频抽帧和 review proxy 策略，并新增 scripts\windows\Test-MiLuStudioStage23BAssetParsing.ps1；下一步继续补真实 OCR runtime 调用或可恢复分片上传 endpoint。provider dry-run / audit contract 后置 Stage 23C，提示词批量操作、镜头增删和更细粒度 diff 后置 Stage 24，signed release 回归等正式证书到位后再做。后续仍不得接真实模型生成，不引入 Linux/Docker/Redis/Celery，不让 UI 或 Electron 绕过 Control API / Worker 边界，不让桌面端执行 migrations、定义数据库表或负责数据库初始化；UI / Electron 仍不得直接读取媒体、执行 FFmpeg 或访问文件系统，真实上传和 FFmpeg 只能在后端 adapter 边界内执行。
```
