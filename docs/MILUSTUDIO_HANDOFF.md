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
Phase: Post Stage 22
Status: done
Owner: current / next session
Goal: Stage 22 Provider Adapter 安全前置层设计与占位落地已完成；下一棒优先确认 Stage 23 范围，继续保持不接真实 provider、不读真实媒体、不触发 FFmpeg。
```

## 当前已完成主线

- Stage 0-20 已完成，详见归档摘要；Stage 21-22 完成摘要记录在本文件和任务记录中。
- Web / Desktop 已撤下许可证、激活码和付费码体验。
- Control API、Worker、PostgreSQL、Python Skills、Electron 桌面宿主和 Windows 安装验收主边界已就位。
- 生产链路仍是 deterministic JSON envelope，不接真实模型，不读真实媒体，不生成真实媒体。
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
- 图片 / 视频仍不读取真实媒体内容，不解析帧，不触发 FFmpeg，不上传到后端真实媒体链路。
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

## 固定约束

- 不接真实模型 provider。
- 不读取真实媒体文件。
- 不触发 FFmpeg。
- 不生成真实 PNG / MP4 / WAV / SRT / ZIP。
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
- Provider settings 仍是 placeholder-only；Stage 22 已完成 metadata-only secret store、spend guard 和 sandbox 占位，但真实 OS secure storage / DPAPI / 硬件密钥方案、真实 key 校验、dry-run 审计和真实 provider adapter 均未实现。
- 工作台角色 / 画风 / 图片提示词 / 视频提示词基础编辑已完成；批量操作、镜头增删、更细粒度 diff 和高级重算策略仍待推进。
- 真实图片、视频、音频、字幕、导出包文件写入和下载区未实现。
- `quality_checker` 不读取真实媒体，不做黑屏、卡顿、水印、音量或字幕烧录检测。
- 正式发布仍需要真实 Authenticode 证书和干净 Windows 虚拟机签名回归。

## 最近验证

```powershell
Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location

. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
D:\soft\program\dotnet\dotnet.exe build D:\code\MiLuStudio\backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore -p:OutputPath=D:\code\MiLuStudio\.tmp\stage22-build\

powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage22ProviderSafety.ps1

git diff --check
```

最近结果：

- Web build：通过。
- .NET build：通过。
- Stage 22 provider safety integration：通过。
- `git diff --check`：通过，仅有 LF / CRLF 工作区提示。
- Playwright 依赖不在当前 web workspace，截图级自动化检查未执行。

## 下一步建议

1. 先由用户确认 Stage 23 的正式编号与范围。
2. 推荐方向一：继续细化工作台高级编辑：提示词批量操作、镜头增删、更细粒度 diff 和重算策略。
3. 推荐方向二：provider adapter 真实接入前设计评审，只做接口契约、审计日志和 dry-run，不发送真实请求。
4. 推荐方向三：拿到正式证书后做 `verify:release:signed` 和干净 Windows 虚拟机安装 / 卸载回归。

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\README.md、D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0 到 Stage 22 已完成，Stage 0-20 完成摘要已归档到 docs\archive\MILUSTUDIO_COMPLETED_STAGES_ARCHIVE_2026-05-14_stage0-20.md；当前 Web / Desktop 已撤下许可证、激活码和付费码体验；Web 主入口是 Codex 式工作台：左侧历史项目、中央单输入框、右侧固定流程进度和生成结果、左下角设置。工作台附件只记录文件名、类型和大小，不读取真实媒体、不解析帧、不触发 FFmpeg、不扩后端真实媒体上传链路。Stage 21 已完成新工作台结构化产物编辑增强：角色、画风、图片提示词和视频提示词可通过 Control API 编辑白名单 JSON envelope 字段，保存后追加 stage21_edit_summary，并重置下游任务等待重新计算；角色 / 画风回到 review，图片 / 视频提示词保持 completed。Stage 22 已完成 Provider Adapter 安全前置层设计与占位落地：provider settings 响应包含 safety，新增 metadata-only secret store、spend guard、provider sandbox、safety endpoint、spend guard check endpoint 和 preflight 安全检查；API Key 只保存遮罩、SHA256 指纹和不可调用引用，真实 provider 调用仍被 sandbox 阻断。请先确认 Stage 23 的正式编号与范围。建议候选方向：继续细化工作台提示词批量操作、镜头增删和更细粒度 diff；provider adapter 真实接入前 dry-run / audit contract；拿到正式证书后的 signed release 回归。后续仍不得接真实模型、不得读取真实媒体文件、不得触发 FFmpeg、不得生成真实 MP4/WAV/SRT/ZIP，不引入 Linux/Docker/Redis/Celery，不让 UI 或 Electron 绕过 Control API / Worker 边界，不让桌面端执行 migrations、定义数据库表或负责数据库初始化。
```
