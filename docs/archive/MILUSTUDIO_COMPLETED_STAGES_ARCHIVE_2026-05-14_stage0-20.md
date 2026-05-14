# MiLuStudio 已完成阶段归档（Stage 0-20）

归档时间：2026-05-14  
用途：主 `docs\MILUSTUDIO_HANDOFF.md` 只保留短棒交接；已完成阶段摘要沉淀在此文件，详细过程仍以 `docs\MILUSTUDIO_TASK_RECORD.md` 为准。

## Stage 0-1：项目起步

- 建立独立仓库、根 README、总构建计划、阶段计划、任务记录、交接文档和参考项目清单。
- 明确 Windows / D 盘本地优先、Control API / Worker / Python Skills / Electron 分层边界。
- Web 初始 React + Vite 壳和产品规格已就位。

## Stage 2-4：后端与 Skills 基座

- 建成 .NET Control API、生产任务状态机、SSE 事件流和 Worker 边界。
- 建成 Python Production Skills Runtime、统一 CLI 和 `SkillGateway`。
- 首个 deterministic `story_intake` skill 可通过 JSON 输入输出运行。

## Stage 5-11：deterministic 生产链路

- 已打通 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible -> storyboard_director`。
- 已打通 `image_prompt_builder -> image_generation` mock 图片资产结构。
- 已打通 `video_prompt_builder -> video_generation` mock 视频片段结构。
- 已打通 `voice_casting -> subtitle_generator -> auto_editor -> quality_checker -> export_packager`。
- 所有输出仍为可审阅 JSON envelope，不接真实模型，不生成真实媒体文件。

## Stage 12-14：持久化、Worker 和输入真实性

- PostgreSQL / EF Core provider、SQL migration、preflight 和 repository provider 切换已完成。
- Worker durable claiming、真实 task output 写回、assets / cost ledger 写回边界已完成。
- Web 输入保存到 Control API，再由 Worker 消费最新 `story_inputs.original_text`。
- checkpoint approve / reject / notes、重新生成淘汰旧 job 和 skill run 保留策略已完成。

## Stage 15：Windows 桌面宿主

- Electron + electron-builder + NSIS 桌面打包完成。
- 桌面端只承载 Web UI、启动本地 Control API / Worker、注入 API base URL 和桌面令牌。
- 桌面端不执行 migrations，不定义数据库表，不负责数据库初始化。

## Stage 16：本地账号和设备

- deterministic 账号注册、登录、会话刷新、退出和设备绑定完成。
- Web / Desktop 已撤下许可证、激活码、付费码和商业授权体验。
- 历史 licensing 表保留为后续商业化边界，不驱动当前用户体验。

## Stage 17：分镜编辑与审核后重算

- 生产控制台分镜表编辑、单镜头备注驱动本地重算和下游 task reset 完成。
- 保存或重算后，storyboard task 回到 review，job 回到分镜审核暂停态。
- 审核通过后 Worker 才继续消费下游任务。

## Stage 18：provider adapter 前配置页

- Web “模型”页完成。
- Control API provider settings endpoint、本地 provider settings 文件仓储、API Key 遮罩 / 指纹和 placeholder preflight 完成。
- 仍不接真实 provider，不访问外部厂商，不生成媒体。

## Stage 19：桌面发布验收与签名前置

- Windows 安装器、`win-unpacked`、运行时资源、Electron 安全边界、桌面模式 migration 禁止和 Authenticode 状态检查完成。
- `NotSigned` 仅作为本地验收警告，正式发布用 `-RequireSigned` 阻断。
- 已新增干净 Windows 安装 / 卸载 / 自启动 / 快捷方式验收清单。

## Stage 20：Codex 式前端工作台

- Web 主入口重构为 Codex 式工作台：左侧历史项目、中央单输入框、右侧固定流程进度和生成结果、左下角设置。
- 模型配置、桌面诊断和账户退出收束到设置菜单。
- 分镜结果打开后仍可通过 Control API 保存分镜表或按单镜头备注重算。
- 上传体验已补 Codex 式附件卡片和阶段门禁加号菜单：文本、图片、视频入口按当前生产阶段启用或禁用。

## 固定边界

- 不接真实模型 provider。
- 不读取真实媒体文件。
- 不触发 FFmpeg。
- 不生成真实 PNG / MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery。
- UI / Electron 只能通过 Control API / DTO / SSE 与业务系统通信。
- Electron 只做桌面宿主、安装器和本地进程管理。
