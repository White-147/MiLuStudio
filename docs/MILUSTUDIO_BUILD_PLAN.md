# MiLuStudio 建设方案与下一会话实施手册

更新时间：2026-05-14
定位：给下一次开发会话使用的详细执行依据。  
硬约束：MiLuStudio 是新的 Windows 原生 AI 漫剧 Agent 产品，不继续套壳旧 MiLuAssistantWeb / MiLuAssistantDesktop，也不把 ArcReel、LumenX、AIComicBuilder 等项目整体搬进来。

## 1. 产品结论

MiLuStudio 要做的是：

> 一个面向普通用户的 Windows 原生 AI 漫剧生产 Agent。用户只通过对话框输入故事、小说片段或创作要求，系统自动完成剧本改编、角色设定、分镜、图片生成、视频生成、配音、字幕、剪辑、质检和导出。用户只在关键节点做确认或修改。

不是：

- 不是通用 AI 助手。
- 不是无限画布工作台。
- 不是开放 Skills 市场。
- 不是要求用户理解模型、参数、工作流节点的专业工具。
- 不是 Linux / Docker / Web SaaS 优先项目。
- 不是直接二开 ArcReel / LumenX / AIComicBuilder / LocalMiniDrama。

第一版 MVP：

> 输入 500 到 2000 字中文故事或小说片段，输出 30 到 60 秒竖屏 AI 漫剧视频，并同时提供脚本卡、角色卡、分镜表、图片素材、视频片段、字幕文件和最终 MP4。

## 2. 当前本地目录状态

必须保留的主干与参考项目：

| 本地目录 | 用途 |
| --- | --- |
| `D:\code\XiaoLouAI` | 公司现有 Windows 原生主干，作为 MiLuStudio 的架构和工程风格主参考。 |
| `D:\code\MiLuStudio` | 新项目目录。总控文档统一位于 `D:\code\MiLuStudio\docs`。 |
| `D:\code\AIComicBuilder` | 漫剧生产链路主参考。重点看剧本导入、角色、分镜、首尾帧、视频合成。 |
| `D:\code\LocalMiniDrama` | Windows 本地一键体验参考。重点看桌面交付、项目形态、用户低门槛流程。 |
| `D:\code\lumenx` | 行业 SOP 参考。重点看资产提取、风格定调、分镜图、分镜视频、合成。 |
| `D:\code\ArcReel-main` | Agent / 任务调度 / SSE / 供应商抽象参考。仅参考思路，不做商业主干。 |
| `D:\code\Toonflow-app` | Electron 分发、短剧工作台、Skill 文件化思路参考。 |
| `D:\code\huobao-drama` | 一句话生成短剧、Mastra Agent + skills 参考。注意授权不适合直接商用复制。 |
| `D:\code\OpenMontage` | Skills、pipeline_defs、FFmpeg / Remotion / 质检 / 成本记录参考。注意 AGPL，仅作概念参考。 |

已清理的本地目录：

- `D:\code\MiLuAssistantWeb`
- `D:\code\MiLuAssistantDesktop`
- `D:\code\QwenPaw`

说明：以上删除仅指本地目录清理，不代表删除 GitHub 远端仓库。

## 3. 联网与本地调研结论

### 3.1 推荐主干

主干必须基于 `XiaoLouAI / MiLu` 自有体系：

- React / Vite / TypeScript 前端。
- Windows 原生部署和交付。
- `.NET 8 / ASP.NET Core` Control API。
- PostgreSQL 作为业务事实来源。
- Windows Worker 负责长任务。
- Python Sidecar 负责模型工具、文件处理、FFmpeg、AI provider 适配等。
- Electron / electron-builder / NSIS 负责最终桌面安装包。

原因：

- 老板要的是稳定、简单、可售卖的 Windows 应用。
- 你已经在 XiaoLouAI 中做过相关架构整理、ChatPanel / Agent Canvas、模型/Skills 菜单、媒体附件、视频设置、项目授权、Windows 原生边界等工作。
- 自有体系最适合沉淀到公司产品和你的履历里。

### 3.2 参考项目定位

| 项目 | 参考价值 | 是否作为代码主干 | 授权/风险 |
| --- | --- | --- | --- |
| [AIComicBuilder](https://github.com/LingyiChen-AI/AIComicBuilder) | 剧本导入、角色提取、角色四视图、智能分镜、首尾帧、视频合成，是最接近漫剧流水线的参考。 | 否 | Apache-2.0，可参考结构和流程，但仍建议自研实现。 |
| [LocalMiniDrama](https://github.com/xuanyustudio/LocalMiniDrama) | Windows 本地、下载即用、一键流水线，最贴近老板要的易用性。 | 否 | MIT，可参考产品形态。技术栈不作为主干。 |
| [LumenX](https://github.com/alibaba/lumenx) | 资产提取、风格定调、资产生成、分镜脚本、分镜图、分镜视频、拼接成片 SOP 清晰。 | 否 | MIT，可参考业务流程。UI 不直接照搬。 |
| [ArcReel](https://github.com/ArcReel/ArcReel) | Agent workflow、SSE 进度、供应商抽象、任务队列、成本追踪可参考。 | 否 | AGPL-3.0，不适合作为商业主干或直接复制代码。 |
| [Toonflow](https://github.com/HBAI-Ltd/Toonflow-app) | Electron 桌面端、短剧工厂、可编排 Skill 思路。 | 否 | Apache-2.0，可参考打包和产品叙事。 |
| [Huobao Drama](https://github.com/chatfire-AI/huobao-drama) | 一句话到完整短剧、Mastra Agent + skills。 | 否 | GitHub licenseInfo 为空，README 标注 CC BY-NC-SA 4.0，商业使用风险高，只参考理念。 |
| [OpenMontage](https://github.com/calesthio/OpenMontage) | 500+ skills、pipeline_defs、FFmpeg、字幕、成本追踪、质检、导出包思路。 | 否 | AGPL-3.0，只参考结构和概念。 |

### 3.3 关键判断

MiLuStudio 不应问“继续二开 ArcReel 还是 LumenX”，而应定为：

> XiaoLouAI / MiLu 自研 Windows 原生主干 + AIComicBuilder 漫剧流程 + LocalMiniDrama 一键桌面体验 + LumenX 行业 SOP + ArcReel / Toonflow / OpenMontage 的 Agent / Skills 调度思想。

## 4. 产品形态

### 4.1 普通用户界面只保留四块

用户界面必须极简：

1. 对话输入区
   - 用户输入一句话、故事、小说片段、风格要求。
   - 支持上传 TXT / DOCX / PDF / 图片 / 音频。
   - 支持选择“极速模式”和“导演模式”。

2. 任务进度流
   - 显示当前阶段：分析故事、改编脚本、生成角色、生成分镜、生成图片、生成视频、生成配音、生成字幕、剪辑成片、质检、导出。
   - 使用 SSE 或 WebSocket 实时更新。
   - 每个阶段显示状态、耗时、费用、是否需要用户确认。

3. 中间结果卡片
   - 默认折叠。
   - 高级用户可展开编辑脚本、角色、风格、分镜、图像提示词、视频提示词。
   - 每张卡都能重新生成、锁定、编辑、确认。

4. 最终交付区
   - 下载 MP4。
   - 下载 SRT 字幕。
   - 下载分镜表。
   - 下载角色设定。
   - 下载图片素材包。
   - 下载工程文件。

### 4.2 不做复杂工作台

第一版不要做：

- 无限画布。
- 节点拖拽编排。
- 复杂时间线。
- 公开 Skill 市场。
- 第三方插件安装。
- 多用户协同编辑。
- SaaS 管理后台。

这些能力可以在第二阶段作为高级模式，但不能阻塞 MVP。

## 5. 技术主线

### 5.1 推荐技术栈

前端：

- React
- Vite
- TypeScript
- Zustand 或 TanStack Query
- ECharts 仅用于统计，不用于核心创作 UI
- lucide-react 图标

桌面：

- Electron
- electron-builder
- NSIS assisted installer 是唯一 Windows 安装器方案
- 自定义 NSIS `installer.nsh`
- 本地端口随机分配
- 托盘和后台进程管理

桌面安装器必须按 QQ / Adobe 一类成熟 Windows 桌面软件体验设计：

- 自定义应用图标、安装器图标、卸载器图标和快捷方式图标。
- 用户可选择安装目录。
- 用户可选择创建桌面快捷方式。
- 用户可选择创建开始菜单快捷方式。
- 用户可选择开机自启动。
- 安装过程必须有可见实时进度条。
- 安装完成页可选择立即启动应用。
- 安装器可加入付费码 / 激活码输入页，校验通过后才能继续安装。
- 任务栏固定不能作为静默强制行为实现；Windows pin 列表属于用户控制。必须保证 AppUserModelID、图标和快捷方式正确，让用户能手动固定，并在安装完成页或首次启动时给出用户可控引导。

后端：

- `.NET 8`
- ASP.NET Core Minimal API 或 Controller API
- EF Core / Dapper 二选一，优先 EF Core 建模，关键查询可 Dapper
- PostgreSQL

Worker：

- .NET BackgroundService / Windows Service
- PostgreSQL `FOR UPDATE SKIP LOCKED` 任务领取
- PostgreSQL `LISTEN/NOTIFY` 或轮询退化策略
- 不依赖 Linux、Docker、Celery、Redis 作为核心链路

Python Sidecar：

- Python 3.11
- FastAPI 或 stdio subprocess 协议二选一
- FFmpeg 调用
- 文档解析
- 模型 provider SDK
- 图片/视频/音频处理
- 质量检查

打包：

- 生产目标为 Windows 安装包。
- 不把 Linux / Docker 写成主要部署方式。
- 开发期可保留脚本，但产品 README 必须以 Windows 为主。

### 5.2 架构边界

```text
用户
  ↓
MiLuStudio Desktop Shell
  ↓
React/Vite UI
  ↓
ASP.NET Core Control API
  ↓
PostgreSQL: projects / jobs / tasks / assets / skills / costs
  ↓
Windows Worker
  ↓
Python Sidecar Skills Runtime
  ↓
LLM / Image / Video / TTS / STT / FFmpeg / QC Providers
  ↓
Artifacts: MP4 / SRT / storyboards / character bible / asset pack
```

原则：

- Electron 只是桌面宿主、安装与本地进程管理层，不替代 Web UI 和 Control API。
- Web 前端和 Control API 必须继续保持可独立迭代，桌面端只通过本地 HTTP / DTO 使用 Control API。
- 数据库属于后端基础设施，必须先在 Control API / Worker / Infrastructure 内完成，不和桌面安装器绑定。
- Electron 不直接访问 PostgreSQL，不直接执行 migrations，不定义数据库表，不承担数据库初始化职责。
- UI 只负责展示、确认、编辑，不直接跑模型。
- Control API 负责权限、项目、任务、状态、成本、资产索引。
- Worker 负责编排任务和阶段流转。
- Python Sidecar 负责具体模型调用和文件处理。
- Skills 是内部生产能力，不开放给用户安装。

登录与授权原则：

- 账号系统不阻塞 Stage 15 桌面安装包 MVP，作为桌面端落地后的下一大阶段推进。
- 下一大阶段对齐 QQ 一类体验：应用启动默认只显示登录 / 注册 / 激活入口，未登录用户不能访问项目、素材、生产任务或设置页。
- 登录注册、会话、授权状态、设备绑定和账号套餐必须由 Control API / Auth & Licensing adapter 统一处理，不放进 Electron 主进程或安装器脚本里。
- 安装器里的付费码 / 激活码校验只能作为安装前门槛和商业体验，不作为唯一安全边界；安装完成后应用仍必须在登录态和授权态下访问功能。
- 离线激活码如需支持，必须使用服务端签发的签名许可证或本机授权文件，安装器和客户端不得内置可逆的付费码算法或私钥。

## 6. 核心数据模型

第一版建议数据库表按以下顺序设计。

### 6.0 账号与授权

`accounts`

- `id`
- `phone`
- `email`
- `display_name`
- `password_hash`
- `status`: `active` / `locked` / `deleted`
- `created_at`
- `last_login_at`

`sessions`

- `id`
- `account_id`
- `device_id`
- `refresh_token_hash`
- `expires_at`
- `revoked_at`

`devices`

- `id`
- `account_id`
- `machine_fingerprint_hash`
- `device_name`
- `first_seen_at`
- `last_seen_at`
- `trusted`

`licenses`

- `id`
- `account_id`
- `license_type`: `trial` / `paid` / `offline_signed`
- `plan`
- `activation_code_hash`
- `status`: `active` / `expired` / `revoked`
- `starts_at`
- `expires_at`
- `max_devices`

说明：

- 账号系统作为桌面 MVP 后的下一大阶段：应用打开先登录 / 注册，登录后才进入创作工作台。
- 桌面安装器可做激活码输入页，但真正的授权判断必须落在 Control API / Auth & Licensing adapter。
- 本阶段不设计 SaaS 管理后台；只预留本地 Control API 与未来授权服务的 adapter 边界。

### 6.1 项目与输入

`projects`

- `id`
- `name`
- `description`
- `mode`: `fast` / `director`
- `status`: `draft` / `running` / `paused` / `completed` / `failed`
- `target_duration_seconds`
- `aspect_ratio`: `9:16` / `16:9` / `1:1`
- `style_preset`
- `created_at`
- `updated_at`

`story_inputs`

- `id`
- `project_id`
- `source_type`: `text` / `file` / `url`
- `original_text`
- `file_asset_id`
- `language`
- `word_count`
- `parsed_at`

### 6.2 剧本与角色

`episodes`

- `id`
- `project_id`
- `episode_index`
- `title`
- `summary`
- `script_text`
- `status`

`characters`

- `id`
- `project_id`
- `name`
- `role_type`: `main` / `supporting` / `extra`
- `gender`
- `age_range`
- `personality`
- `appearance`
- `costume`
- `voice_profile`
- `consistency_notes`

`character_assets`

- `id`
- `character_id`
- `asset_type`: `front` / `three_quarter` / `side` / `back` / `portrait` / `reference`
- `asset_id`
- `prompt`
- `is_locked`

`style_bibles`

- `id`
- `project_id`
- `visual_style`
- `color_palette`
- `camera_language`
- `negative_prompt`
- `reference_asset_ids`
- `locked`

### 6.3 分镜与生成

`shots`

- `id`
- `project_id`
- `episode_id`
- `shot_index`
- `duration_seconds`
- `scene_summary`
- `dialogue`
- `narration`
- `characters`
- `camera_angle`
- `camera_motion`
- `lighting`
- `composition`
- `image_prompt`
- `video_prompt`
- `status`
- `user_locked`

`shot_assets`

- `id`
- `shot_id`
- `asset_type`: `storyboard_image` / `first_frame` / `last_frame` / `video_clip` / `audio` / `subtitle`
- `asset_id`
- `provider`
- `generation_task_id`
- `selected`

### 6.4 任务与成本

`production_jobs`

- `id`
- `project_id`
- `current_stage`
- `status`
- `progress_percent`
- `started_at`
- `finished_at`
- `error_message`

`generation_tasks`

- `id`
- `job_id`
- `project_id`
- `shot_id`
- `skill_name`
- `provider`
- `input_json`
- `output_json`
- `status`
- `attempt_count`
- `cost_estimate`
- `cost_actual`
- `started_at`
- `finished_at`
- `error_message`

`cost_ledger`

- `id`
- `project_id`
- `task_id`
- `provider`
- `model`
- `unit`
- `quantity`
- `estimated_cost`
- `actual_cost`
- `created_at`

### 6.5 资产与导出

`assets`

- `id`
- `project_id`
- `kind`: `text` / `image` / `video` / `audio` / `subtitle` / `archive`
- `local_path`
- `mime_type`
- `file_size`
- `sha256`
- `metadata_json`
- `created_at`

`export_packages`

- `id`
- `project_id`
- `mp4_asset_id`
- `srt_asset_id`
- `storyboard_asset_id`
- `character_bible_asset_id`
- `asset_zip_id`
- `created_at`

## 7. 内置 Production Skills 设计

不要叫“插件”，内部叫：

> Production Skills / 生产技能

用户不需要知道这些技能存在，开发者可以维护和版本化它们。

### 7.1 Skill 目录结构

```text
skills/
  story_intake/
    skill.yaml
    prompt.md
    schema.input.json
    schema.output.json
    executor.py
    validators.py
    examples/
  plot_adaptation/
  episode_writer/
  character_bible/
  style_bible/
  storyboard_director/
  image_prompt_builder/
  video_prompt_builder/
  image_generation/
  video_generation/
  voice_casting/
  subtitle_generator/
  music_sfx/
  auto_editor/
  quality_checker/
  export_packager/
```

### 7.2 每个 Skill 必须包含

`skill.yaml`

- `name`
- `display_name`
- `description`
- `stage`
- `input_schema`
- `output_schema`
- `timeout_seconds`
- `max_retries`
- `cost_policy`
- `required_provider_capabilities`
- `checkpoint_policy`

`prompt.md`

- 当前技能的系统提示词。
- 输入字段解释。
- 输出 JSON 要求。
- 质量标准。
- 禁止项。

`schema.input.json`

- 输入 JSON Schema。
- 禁止自由文本透传到下游。

`schema.output.json`

- 输出 JSON Schema。
- 所有输出必须可被后续阶段稳定消费。

`executor.py`

- 只做具体执行。
- 不直接操作数据库。
- 读入 input JSON，返回 output JSON。

`validators.py`

- 检查字段完整性。
- 检查角色一致性。
- 检查分镜数量、时长、字幕长度。

`examples/`

- 放最小输入输出样例。
- 作为单元测试 fixture。

### 7.3 第一版 Skill 清单

| Skill | 输入 | 输出 | 用户是否停顿 |
| --- | --- | --- | --- |
| `story_intake` | 原文、目标时长、风格 | 故事摘要、类型、风险提示 | 否 |
| `plot_adaptation` | 故事摘要、目标时长 | 短视频剧情结构 | 可选 |
| `episode_writer` | 剧情结构 | 单集脚本、旁白、对白 | 是 |
| `character_bible` | 脚本 | 角色卡、外貌、服装、性格 | 是 |
| `style_bible` | 脚本、角色 | 画风、色调、镜头语言 | 是 |
| `storyboard_director` | 脚本、角色、风格 | 分镜列表、景别、运镜、时长 | 是 |
| `image_prompt_builder` | 分镜、角色、风格 | 图像提示词、负向提示词 | 否 |
| `image_generation` | 图像提示词、参考图 | 分镜图/首帧/尾帧 | 可选 |
| `video_prompt_builder` | 分镜图、分镜信息 | 视频提示词 | 否 |
| `video_generation` | 首帧/尾帧、视频提示词 | 视频片段 | 可选 |
| `voice_casting` | 角色、台词 | 配音任务、音色映射 | 是 |
| `subtitle_generator` | 旁白、对白、音频 | SRT 字幕 | 否 |
| `music_sfx` | 风格、剧情节奏 | BGM / SFX 建议或素材 | 可选 |
| `auto_editor` | 视频片段、音频、字幕 | 粗剪成片 | 否 |
| `quality_checker` | 所有中间结果 | 问题报告、可自动重试项 | 是 |
| `export_packager` | 最终成片和素材 | MP4/SRT/ZIP/工程文件 | 否 |

## 8. 工作流状态机

第一版必须做成强状态机，不能让 Agent 随意跳步骤。

```text
CREATED
  -> STORY_INGESTING
  -> PLOT_ADAPTING
  -> SCRIPT_READY_FOR_REVIEW
  -> CHARACTER_BUILDING
  -> CHARACTER_READY_FOR_REVIEW
  -> STYLE_BUILDING
  -> STYLE_READY_FOR_REVIEW
  -> STORYBOARD_BUILDING
  -> STORYBOARD_READY_FOR_REVIEW
  -> IMAGE_PROMPTS_BUILDING
  -> IMAGES_GENERATING
  -> IMAGES_READY_FOR_REVIEW
  -> VIDEO_PROMPTS_BUILDING
  -> VIDEOS_GENERATING
  -> VIDEOS_READY_FOR_REVIEW
  -> AUDIO_GENERATING
  -> SUBTITLES_GENERATING
  -> EDITING
  -> QUALITY_CHECKING
  -> READY_FOR_FINAL_REVIEW
  -> EXPORTING
  -> COMPLETED
```

失败状态：

- `FAILED_RETRYABLE`
- `FAILED_NEEDS_USER`
- `FAILED_FATAL`

暂停状态：

- `PAUSED_BY_USER`
- `PAUSED_FOR_COST_CONFIRMATION`
- `PAUSED_FOR_CONTENT_REVIEW`

## 9. API 设计

### 9.1 项目 API

- `POST /api/projects`
  - 创建项目。
  - 输入：项目名、故事文本、目标时长、比例、模式。
  - 输出：`projectId`。

- `GET /api/projects`
  - 项目列表。

- `GET /api/projects/{projectId}`
  - 项目详情。

- `PATCH /api/projects/{projectId}`
  - 更新项目名称、目标时长、风格、模式。

### 9.2 生产任务 API

- `POST /api/projects/{projectId}/production-jobs`
  - 启动生产。

- `GET /api/production-jobs/{jobId}`
  - 获取当前状态。

- `GET /api/production-jobs/{jobId}/events`
  - SSE 进度流。
  - 事件类型：`stage_changed`、`task_started`、`task_progress`、`task_completed`、`task_failed`、`checkpoint_required`、`cost_updated`、`artifact_ready`。

- `POST /api/production-jobs/{jobId}/pause`

- `POST /api/production-jobs/{jobId}/resume`

- `POST /api/production-jobs/{jobId}/retry`

### 9.3 审核与编辑 API

- `GET /api/projects/{projectId}/script`
- `PATCH /api/projects/{projectId}/script`
- `GET /api/projects/{projectId}/characters`
- `PATCH /api/projects/{projectId}/characters/{characterId}`
- `GET /api/projects/{projectId}/storyboard`
- `PATCH /api/projects/{projectId}/shots/{shotId}`
- `POST /api/projects/{projectId}/shots/{shotId}/regenerate`

### 9.4 资产 API

- `POST /api/assets/upload`
- `GET /api/assets/{assetId}`
- `GET /api/assets/{assetId}/download`
- `GET /api/projects/{projectId}/assets`

### 9.5 导出 API

- `POST /api/projects/{projectId}/export`
- `GET /api/export-packages/{packageId}`
- `GET /api/export-packages/{packageId}/download`

## 10. 前端页面设计

### 10.1 第一版路由

```text
/
  项目列表
/project/:projectId
  对话入口 + 任务进度 + 中间结果 + 最终交付
/settings/providers
  模型供应商配置
/settings/storage
  本地存储路径、缓存、清理
/settings/about
  版本、日志、诊断
```

### 10.2 项目页布局

左栏：对话与输入

- 文本输入框。
- 文件上传按钮。
- 模式切换：极速 / 导演。
- 目标时长选择。
- 比例选择：默认 9:16。
- “开始生成”按钮。

中栏：任务进度

- 阶段时间线。
- 每个阶段状态。
- 当前正在执行的 Skill。
- 成本预估。
- 错误提示和重试按钮。

右栏：结果卡片

- 脚本卡。
- 角色卡。
- 风格卡。
- 分镜卡。
- 图片卡。
- 视频卡。
- 配音/字幕卡。
- 成片卡。

底部或右下：最终交付

- 下载 MP4。
- 下载 SRT。
- 下载素材包。
- 打开输出目录。

### 10.3 用户停顿点

默认只在这些节点停顿：

1. 脚本确认。
2. 角色确认。
3. 风格确认。
4. 分镜确认。
5. 图片结果确认。
6. 视频结果确认。
7. 最终质检确认。

极速模式可以跳过中间确认，但必须在成本明显增加前提示用户。

### 10.4 前端质量约束

如果当前 Codex 会话可用，前端代码和视觉验收前优先调用这些前端优化 Skills：

- `frontend-skill`
- `playwright-interactive`
- `impeccable`
- `taste-skill` / `gpt-taste`

使用目标：

- 检查布局是否稳定、文本是否溢出、控件是否清晰。
- 检查页面是否像可用工作台，而不是营销页。
- 检查移动端和桌面端是否都能正常阅读和操作。
- 检查视觉风格是否克制、专业、符合 Windows 原生 AI 漫剧生产工具定位。

如果这些 Skills 在当前会话不可见，必须至少完成本地构建、可运行页面验证和人工视觉自检。

## 11. 后端目录建议

下一会话如果开始搭建，建议目录：

```text
D:\code\MiLuStudio\
  README.md
  docs\
    MILUSTUDIO_BUILD_PLAN.md
    MILUSTUDIO_PHASE_PLAN.md
    MILUSTUDIO_TASK_RECORD.md
    MILUSTUDIO_HANDOFF.md
    ARCHITECTURE.md
    PRODUCT_SPEC.md
    SKILLS_SPEC.md
    DATA_MODEL.md
    WINDOWS_PACKAGING.md
    REFERENCE_PROJECTS.md
  apps\
    desktop\
      package.json
      electron\
      src\
    web\
      package.json
      src\
  backend\
    control-plane\
      MiLuStudio.ControlPlane.sln
      src\
        MiLuStudio.Api\
        MiLuStudio.Application\
        MiLuStudio.Domain\
        MiLuStudio.Infrastructure\
        MiLuStudio.Worker\
      tests\
    sidecars\
      python-skills\
        pyproject.toml
        milu_studio_skills\
        skills\
  packages\
    shared-types\
    ui\
  scripts\
    windows\
    dev\
  storage\
    .gitkeep
  samples\
    stories\
    outputs\
```

注意：

- `storage/` 只存本地运行数据，默认加入 `.gitignore`。
- `samples/outputs/` 可以保存无敏感样例，真实用户素材必须忽略。
- 后端不要拆得过细，先保证 MVP 贯通。

## 12. 详细实施步骤

### 阶段 0：项目初始化

目标：让 MiLuStudio 成为独立项目，而不是旧项目改名。

步骤：

1. 在 `D:\code\MiLuStudio` 初始化 Git。
2. 添加根 `.gitignore`。
3. 添加 `README.md`，只写项目定位和开发状态。
4. 确认总控文档保留在 `docs/`。
5. 添加 `docs/REFERENCE_PROJECTS.md`，记录参考项目和授权边界。
6. 添加 `docs/PRODUCT_SPEC.md`，记录 MVP 范围。
7. 不复制参考项目源码。
8. 不引入旧 MiLuAssistantWeb / Desktop 代码。

验收：

- `git status` 干净。
- 根目录没有 Node / Python / .NET 混乱依赖。
- 文档能说明 MiLuStudio 与旧 MiLuAssistant 项目的区别。

### 阶段 1：前端壳

目标：先做可运行的 UI 壳，不接模型。

步骤：

1. 创建 `apps/web`。
2. 使用 Vite + React + TypeScript。
3. 建立三栏项目页。
4. 创建静态 mock 数据。
5. 实现任务进度流 mock。
6. 实现结果卡片 mock。
7. 实现“极速模式 / 导演模式”切换。
8. 实现“开始生成”按钮，但只触发 mock 状态变化。
9. 如果可用，调用前端优化 Skills 做视觉和交互自检。

验收：

- `npm run dev` 可启动。
- 首页能进入项目页。
- 项目页能显示对话区、进度区、结果区、交付区。
- 无大段说明性营销文案。
- UI 不出现复杂画布。

参考：

- `D:\code\XiaoLouAI\XIAOLOU-main\src\features\canvas-agent-canvas`
- `D:\code\AIComicBuilder\src\app\[locale]\project\[id]`
- `D:\code\LocalMiniDrama\frontweb`

### 阶段 2：数据模型和 Control API

目标：建立项目、任务、资产、分镜、角色的最小可用后端。

步骤：

1. 创建 `backend/control-plane`。
2. 建立 `.NET 8` solution。
3. 创建 `Api`、`Application`、`Domain`、`Infrastructure`、`Worker` 项目。
4. 配置 PostgreSQL 连接。
5. 添加 migrations。
6. 建立 `projects`、`story_inputs`、`characters`、`shots`、`assets`、`production_jobs`、`generation_tasks` 表。
7. 实现 `POST /api/projects`。
8. 实现 `GET /api/projects/{id}`。
9. 实现 `POST /api/projects/{id}/production-jobs`。
10. 实现 `GET /api/production-jobs/{id}`。
11. 实现 `GET /api/production-jobs/{id}/events` SSE。

验收：

- 可以创建项目。
- 可以启动生产任务。
- 前端可以看到真实 jobId。
- SSE 能推送 mock 进度。

参考：

- `D:\code\XiaoLouAI\backend\dotnet\control-plane`
- `D:\code\ArcReel-main\server`

### 阶段 3：任务状态机

目标：让生产流程可控、可恢复、可暂停。

步骤：

1. 定义 `ProductionStage` enum。
2. 定义合法状态迁移表。
3. 实现 `ProductionJobService`。
4. 实现 `TaskQueueService`。
5. Worker 保留 heartbeat 和任务领取边界，本阶段不接真实 PostgreSQL adapter。
6. 每个 task 失败后记录错误、attempt_count、是否可重试。
7. 实现暂停、恢复、重试。
8. 实现阶段 checkpoint。
9. PostgreSQL `FOR UPDATE SKIP LOCKED` 领取策略留到后续持久化 / Worker 收敛阶段。

验收：

- 任意阶段失败不会导致项目状态丢失。
- API 进程存活时，刷新前端后仍能恢复进度。
- 用户确认后才能继续 checkpoint 阶段。
- API 重启后的 durable recovery 留给真实 PostgreSQL adapter / EF Core DbContext 阶段。

参考：

- ArcReel 的 async task queue / SSE 思路。
- OpenMontage 的 checkpoint protocol 思路。

### 阶段 4：Python Skills Runtime

目标：跑通第一个内部 Skill。

步骤：

1. 创建 `backend/sidecars/python-skills`。
2. 创建目标为 Python `>=3.11,<3.14` 的项目；本地使用 `D:\soft\program\Python\Python313\python.exe` 验证。
3. 创建 `milu_studio_skills` 包。
4. 定义统一 CLI：
   - `python -m milu_studio_skills run --skill story_intake --input input.json --output output.json`
5. 创建 `skills/story_intake`。
6. 添加 `skill.yaml`。
7. 添加 `schema.input.json` 和 `schema.output.json`。
8. 添加 `prompt.md`。
9. 添加 `executor.py`。
10. 添加 `validators.py`。
11. 添加单元测试。

验收：

- Python skill 只能通过统一 CLI / gateway 调用，后续 .NET Worker 不直接调用单个 skill 文件。
- 输入 JSON 输出 JSON。
- 输出 envelope 结构稳定，后续 Worker / 数据库 adapter 可写回；本阶段不直接接真实数据库。
- 失败输出包含错误码和错误消息。

参考：

- `D:\code\huobao-drama\skills`
- `D:\code\OpenMontage\skills`
- `D:\code\ArcReel-main\agent_runtime_profile\.claude\skills`

### 阶段 5：故事解析到脚本

目标：输入故事，生成可审阅脚本结构。

步骤：

1. 实现 `story_intake`。
2. 实现 `plot_adaptation`。
3. 实现 `episode_writer`。
4. 基于 `story_intake` 的稳定 envelope 串联后续 skill。
5. 输出 `plot_beats`、`segments`、`subtitle_cues` 和 review checkpoint。
6. 本阶段不直接保存 `episodes.script_text`。
7. 数据库写回、前端脚本卡、脚本编辑和用户确认 API 留给 PostgreSQL adapter / EF Core DbContext 与后续 Control API 收敛阶段。

验收：

- 输入 500 字故事，能得到 30 到 60 秒脚本。
- 脚本包含旁白和对白。
- 句子长度适合配音和字幕。
- Python Sidecar 只通过统一 CLI / gateway 产出 JSON envelope。
- 不接真实模型 provider，不让 UI 直接调用 Python。

参考：

- AIComicBuilder 的 `src/app/api/projects/[id]/import`
- LumenX 的 Script / entity extraction SOP。

### 阶段 6：角色和风格

目标：在 Python Skills Runtime 内生成稳定的角色设定和统一画风结构。

步骤：

1. 基于 `episode_writer` 成功 envelope 实现 `character_bible`。
2. 输出角色名、身份、性格、外貌、服装、声音、稳定 seed、跨镜头一致性规则和审核 checkpoint。
3. 基于 `episode_writer` 和 `character_bible` 成功 envelope 实现 `style_bible`。
4. 输出画风、色板、灯光、环境设计、镜头语言、角色渲染规则、负向提示词和可复用 prompt blocks。
5. 在 `SkillGateway.default()` 注册两个 skill，并补齐 schema、examples、单元测试和 CLI smoke。
6. 本阶段只产出可审阅 JSON 结构；不保存数据库，不接真实模型，不触发图片/视频/FFmpeg，不让 UI 直接调用 Python。

延后：

- 前端角色卡 / 风格卡展示、锁定、重新生成和参考图上传留给 Control API / PostgreSQL adapter / UI 集成阶段。
- 角色参考图、三视图、头像特写和图生视频参考素材留给图片生成及资产阶段。

验收：

- 主角能保持跨镜头一致性描述。
- 风格提示词能被后续图片和视频阶段复用。
- 下游 `storyboard_director` 能通过 envelope 读取 `character_bible` 和 `style_bible`。
- 用户修改角色后的持久化与下游重算留给后续数据库 / Control API 阶段，不在 Stage 6 直连实现。

参考：

- AIComicBuilder 角色四视图。
- LumenX 资产提取和 Art Direction。
- LocalMiniDrama 角色库和场景库。

### 阶段 7：分镜

目标：在 Python Skills Runtime 内生成可审阅、可执行的镜头列表结构。

步骤：

1. 基于 `episode_writer`、`character_bible` 和 `style_bible` 成功 envelope 实现 `storyboard_director`。
2. 每个镜头包含：时长、场景、角色、景别、运镜、构图、灯光、对白、旁白、声音提示、角色/风格连续性说明。
3. 每个镜头产出后续图像和视频提示词阶段可复用的 `image_prompt_seed` 和 `video_prompt_seed`。
4. 对总时长和镜头数量做校验。
5. 在 `SkillGateway.default()` 注册 skill，并补齐 schema、examples、单元测试和 CLI smoke。
6. 本阶段只产出可审阅 JSON 结构；不保存数据库，不接真实模型，不生成分镜图，不触发图片/视频/FFmpeg，不让 UI 直接调用 Python。
7. 后续补丁已为 `storyboard_director` 增加 `cinematic_md_v1` 展示结构：`film_overview`、`storyboard_parts`、`rendered_markdown` 和 `validation_report`，用于对齐专业分镜 MD 审核格式，同时保留原 `shots` 供下游技能消费。

延后：

- 前端分镜表展示、增删改分镜、单镜头重新生成和用户编辑持久化留给 Control API / PostgreSQL adapter / UI 集成阶段。
- 严格 120 秒、50-55 镜头、逐句对白保真和 10-14 部分长分镜模式留给后续模型/API adapter、对白校验器和长文本分镜 validator。
- 分镜图、首帧、尾帧、视频片段和资产选择留给图片生成、视频生成和资产阶段。

验收：

- 30 到 60 秒项目默认 6 到 12 个镜头。
- 镜头时长总和接近目标时长。
- 每个镜头具备后续图片和视频阶段可消费的结构化 prompt seed。
- 下游 `image_prompt_builder` 和 `video_prompt_builder` 能通过 envelope 读取镜头、角色和画风结构。

参考：

- AIComicBuilder 的 `src/app/api/projects/[id]/shots`
- LumenX StoryBoard。
- OpenMontage pipeline_defs。

### 阶段 8：图片生成边界

目标：在 Python Skills Runtime 内生成后续图像阶段可消费的提示词请求和 mock 占位资产结构。

本阶段不接真实图片 provider，不写图片文件，不写数据库，不做前端选图，不做单镜头真实重试。

步骤：

1. 基于 `storyboard_director`、`character_bible` 和 `style_bible` 成功 envelope 实现 `image_prompt_builder`。
2. 输出角色参考、分镜图、首帧和尾帧的结构化 `image_requests`。
3. 每个请求包含 `request_id`、`asset_type`、`shot_id`、`character_ids`、`prompt`、`negative_prompt`、`aspect_ratio`、`style_refs`、`continuity_refs`、`seed_hint` 和 `output_slot`。
4. 实现 mock `image_generation`，把 `image_requests` 转成可审阅的占位资产结构。
5. mock 资产只暴露逻辑 `milu://mock-assets/...` URI 和 `storage_intent`，`file_written=false`，`writes_files=false`，`writes_database=false`。
6. 在 `SkillGateway.default()` 注册两个 skill，并补齐 schema、examples、单元测试和 CLI smoke。
7. 本阶段继续只产出可审阅 JSON 结构；不保存数据库，不接真实模型，不触发图片/视频/FFmpeg，不让 UI 直接调用 Python。

延后：

- `ImageProvider` interface、OpenAI / Gemini / Kling / 国产图片模型 adapter、真实成本记录和失败重试留给 provider / adapter 阶段。
- 图片资产持久化、`assets` / `shot_assets` 写入和被选中版本追踪留给 PostgreSQL adapter / EF Core DbContext 阶段。
- 前端选图、重试和图片预览留给 Control API / UI 集成阶段。

验收：

- 每个分镜至少生成 `storyboard_image`、`first_frame` 和 `last_frame` 三类请求。
- 至少生成角色参考请求，供后续真实角色图资产阶段消费。
- mock `image_generation` 资产数量与 prompt 请求数量一致。
- mock 输出明确不写文件、不写数据库、不调用 provider。

参考：

- LumenX Assets / StoryBoard / Motion。
- OpenAI Images API。
- 主流 image-to-video 首帧 / 尾帧工作流。

### 阶段 9：视频生成边界

目标：在 Python Skills Runtime 内生成后续视频阶段可消费的视频提示词请求和 mock 占位视频片段结构。

本阶段不接真实视频 provider，不写 MP4 文件，不写数据库，不调用 FFmpeg，不做前端片段预览，不做单镜头真实重试。

步骤：

1. 基于 `storyboard_director`、`image_prompt_builder` 和 `image_generation` 成功 envelope 实现 `video_prompt_builder`。
2. 每个镜头输出一个结构化 `video_requests` 项，包含 `request_id`、`shot_id`、`duration_seconds`、`generation_mode`、`prompt`、`negative_prompt`、`source_images`、`motion_plan`、`continuity_refs`、`seed_hint` 和 `output_slot`。
3. 图生视频优先：当 `first_frame` 和 `last_frame` mock 资产存在时，输出 `generation_mode=image_to_video`。
4. 文生视频退化：当只有分镜图或其他逻辑参考资产时，保留 `generation_mode=text_to_video_fallback`，但本阶段仍不调用真实模型。
5. 实现 mock `video_generation`，把 `video_requests` 转成可审阅的占位视频片段结构。
6. mock 片段只暴露逻辑 `milu://mock-assets/...` URI 和 `storage_intent`，`file_written=false`，`writes_files=false`，`writes_database=false`，`uses_ffmpeg=false`。
7. 在 `SkillGateway.default()` 注册两个 skill，并补齐 schema、examples、单元测试和 CLI smoke。

延后：

- `VideoProvider` interface、OpenAI / Veo / Kling / Wanx 等真实视频模型 adapter、真实成本记录和失败重试留给 provider / adapter 阶段。
- 视频资产持久化、`assets` / `shot_assets` 写入和被选中版本追踪留给 PostgreSQL adapter / EF Core DbContext 阶段。
- 前端片段预览、单镜头重试和视频选择留给 Control API / UI 集成阶段。

验收：

- 每个镜头有独立 `video_request` 和 mock `video_clip` 结构。
- mock 输出数量与分镜镜头数量一致。
- 图生视频请求能引用 `first_frame` 和 `last_frame` mock 图片资产。
- mock 输出明确不写文件、不写数据库、不调用 provider、不触发 FFmpeg。

参考：

- AIComicBuilder 视频生成。
- LumenX Motion。
- ArcReel multi-provider video generation。
- OpenMontage video-gen prompting。

### 阶段 10：音频、字幕、剪辑

目标：在 Python Skills Runtime 内生成后续音频、字幕和剪辑阶段可消费的配音任务、SRT-ready 字幕结构和粗剪计划结构。

本阶段不接真实 TTS / BGM / SFX provider，不写 WAV / SRT / MP4 文件，不写数据库，不调用 FFmpeg，不做前端预览或下载，不让 UI 直接访问 Python、文件系统或 FFmpeg。

步骤：

1. 基于 `episode_writer` 和 `storyboard_director` 成功 envelope 实现 `voice_casting`。
2. 输出 narrator / speaker voice profiles、逐段 `voice_tasks`、逻辑 `milu://mock-assets/...wav` 音频意图、零成本估算和人工确认 checkpoint。
3. 基于 `episode_writer`、`storyboard_director` 和 `voice_casting` 成功 envelope 实现 `subtitle_generator`。
4. 输出 `subtitle_cues`、SRT 文本结构、逻辑 `milu://mock-assets/...srt` 字幕意图和 review warnings；本阶段只生成可消费文本，不写真实 SRT 文件。
5. 基于 `storyboard_director`、`video_generation`、`voice_casting` 和 `subtitle_generator` 成功 envelope 实现 `auto_editor`。
6. 输出 video / audio / subtitle timeline tracks、rough edit `render_plan`、逻辑 `milu://mock-assets/...mp4` 输出意图和 review warnings；本阶段不调用 FFmpeg、不生成真实 MP4。
7. 在 `SkillGateway.default()` 注册三个 skill，并补齐 schema、examples、单元测试和 CLI smoke。

验收：

- `voice_casting` 可从脚本段落和分镜 timing 生成配音任务，输出明确 `provider=none`、`model=none`、`file_written=false`。
- `subtitle_generator` 可从配音任务生成按时间排序的字幕 cue 和 SRT 文本结构，输出明确 `writes_files=false`、`writes_database=false`。
- `auto_editor` 可从 mock 视频片段、配音任务和字幕 cue 生成粗剪 timeline / render plan，输出明确 `uses_ffmpeg=false`、`writes_files=false`、`writes_database=false`。
- Stage 10 不绕过 Control API / Worker / Python Skills Runtime 边界。

延后：

- `AudioProvider` / TTS adapter、音色试听、真实 WAV 写入、BGM / SFX、音量标准化、字幕文件落盘、FFmpeg 拼接、MP4 输出、资产持久化和下载区 UI 留给后续 provider / adapter / export packaging 阶段。

参考：

- OpenMontage `skills/core/ffmpeg.md`
- OpenMontage `skills/core/subtitle-sync.md`
- LumenX Assembly。
- OpenAI Audio / Text-to-speech docs。
- Auto-Editor timeline v3。

### 阶段 11：质量检查

目标：先把质量检查收敛成内部 Production Skill 边界，避免后续“一键生成”变成“一键失败”。

步骤：

1. 实现 `quality_checker`。
2. 基于 `character_bible` 检查分镜角色引用是否一致。
3. 基于 `style_bible` 检查分镜是否引用已审核的画风 prompt blocks。
4. 基于 `storyboard_director`、`video_generation` 和 `auto_editor` 检查 shot / mock clip / timeline 时长是否一致。
5. 基于 `voice_casting` 和 `subtitle_generator` 检查配音任务、SRT cue timing、字幕长度和镜头引用。
6. 检查 mock 资产和计划结构的边界标记：`file_written=false`、`writes_files=false`、`writes_database=false`、`uses_ffmpeg=false`、`reads_media_files=false`。
7. 输出可读质量报告、严重级别、可自动重试项和人工确认 checkpoint。
8. 后续真实黑屏、卡顿、水印、尺寸、音量和字幕烧录检测留给真实媒体 QA adapter；本阶段不读取真实媒体文件。

验收：

- 用户能看到失败原因。
- 可重试项能指向失败镜头或失败结构，供后续 Worker 从对应 skill 继续。
- 报告结构能被后续 Control API / Worker / PostgreSQL adapter 保存为项目资产。
- 本阶段不接真实视觉或音频检测模型，不触发 FFmpeg，不生成真实 MP4，不写数据库。

参考：

- OpenMontage reviewer / quality skills。
- ArcReel checkpoint resume 思路。
- FFmpeg `blackdetect` / `freezedetect` / `silencedetect` 只作为后续真实媒体 adapter 参考，不在 Stage 11 直接调用。

### 阶段 12：PostgreSQL 持久化与后端收敛

目标：先把数据库、持久化、迁移和 Worker durable claiming 作为后端能力做好，不和桌面端绑定。

本阶段不做 Electron，不做安装器，不做桌面端进程管理。桌面端后续只调用 Control API health / preflight 和业务 API。

步骤：

1. 在 `MiLuStudio.Infrastructure` 中新增 PostgreSQL / EF Core DbContext adapter。
2. 引入必要的 EF Core / Npgsql 依赖，并继续约束 NuGet 缓存到 D 盘项目目录。
3. 将现有 SQL migration 与 EF Core model 对齐，必要时建立 migration runner 或明确的 migration 命令。
4. 实现 `ProjectRepository`、`ProductionJobRepository`、`GenerationTaskRepository`、`AssetRepository`、`CostLedgerRepository` 的 PostgreSQL adapter。
5. 通过配置切换 `RepositoryProvider=InMemory` / `RepositoryProvider=PostgreSQL`，开发期可保留 InMemory 作为快速 mock。
6. 实现 API 启动 preflight：检查连接串、数据库可达性、migration 状态和 storage 路径。
7. 实现 Worker durable claiming，优先使用 PostgreSQL `FOR UPDATE SKIP LOCKED`，必要时保留轮询退化。
8. 将 Stage 5-11 的 Production Skill envelope 输出通过 Control API / Worker 写入 `generation_tasks.output_json`、`assets`、`cost_ledger` 等表。
9. 补齐数据库集成测试，覆盖 API 重启后的项目、任务、checkpoint、失败重试和成本记录恢复。
10. 明确本地数据库安装方式和连接配置文档；不要把 PostgreSQL 安装、端口、账号或 migrations 藏进 Electron 安装器。

验收：

- API 和 Worker 能在 PostgreSQL provider 下共享同一份项目、任务、资产和成本状态。
- API 重启后仍能恢复项目进度、checkpoint、失败原因和已完成 skill 输出。
- Worker 重启后能继续领取未完成任务，不重复执行已完成任务。
- InMemory provider 仍可用于开发 smoke，不影响 PostgreSQL provider。
- 所有数据库连接、migration、日志和 storage 配置都属于后端配置，不由 Electron 直接管理。
- UI 只通过 Control API 展示数据库状态和错误；不直接访问数据库。

延后：

- 账号注册、登录、设备绑定、许可证和云端授权服务留给桌面 MVP 后的账号授权阶段。
- Electron 安装器只在 Stage 15 作为交付壳接入已有后端，不承担数据库 schema 或 migration 设计。

参考：

- PostgreSQL `FOR UPDATE SKIP LOCKED` durable queue。
- EF Core DbContext / migrations。
- XiaoLouAI Windows 本地后端配置经验。
- ArcReel task queue / cost ledger 思路。

当前落地状态：

- Stage 12 已在 `backend\control-plane` 中落地为后端 Infrastructure 能力。
- 已新增 `MiLuStudioDbContext`、PostgreSQL repository、provider 配置切换、preflight、migration status / apply API、Worker durable claiming 和 skill envelope 写回 API。
- PostgreSQL provider 使用 SQL migration 文件维护 schema；`002_stage12_postgresql_claiming.sql` 增加 Worker lease / claiming 字段。
- Worker claiming 通过 repository 边界完成，PostgreSQL 查询使用 `FOR UPDATE SKIP LOCKED`，并可接管 lease 过期的 running task。
- 本阶段仍不接 Electron；数据库配置、migration、storage 和日志都属于 Control API / Worker / Infrastructure。
- Stage 13 已将本地真实数据库配置落地到 `milu` / `root` / `root`，Control API preflight 返回 healthy。
- Stage 13 已完成 PostgreSQL migration / API / Worker 共享状态 smoke，验证 15 个 deterministic Production Skill task 全部写回 PostgreSQL。

### 阶段 13：真实配置、Worker-Skills 与前后端收敛验收

目标：把 Stage 12 的后端持久化边界真正切到本机 PostgreSQL，并让 Worker、Python deterministic skills、Control API 和前端展示形成可验收闭环。

本阶段决策：

- 复用本机 PostgreSQL 18 Windows 服务。
- 使用 `root/root` 作为本地开发数据库账号。
- 创建 MiLuStudio 专用业务库 `milu`，不使用 XiaoLouAI 的 `xiaolou` 数据库。
- 默认 `RepositoryProvider` 切换为 `PostgreSQL`。
- InMemory provider 仍保留，但只用于快速 smoke 或特殊轻量场景。
- 能持久化的数据优先写入 PostgreSQL，避免在用户电脑内存较小时堆积过多进程内状态。
- 桌面端仍不参与数据库安装、连接、migration 或 storage 初始化；Stage 14 先处理打包前补丁，桌面打包后移到 Stage 15。

步骤：

1. 将 API / Worker 的版本库配置改为 PostgreSQL 默认 provider。
2. 将连接串统一为 `Host=127.0.0.1;Port=5432;Database=milu;Username=root;Password=root`。
3. 新增或更新 Windows 初始化脚本，幂等创建数据库 `milu` 并保持 D 盘约束。
4. 通过 Control API / Infrastructure migration service 应用 SQL migrations。
5. 增加 Worker 内部 skill runner adapter，通过 Python CLI / `SkillGateway` 调用 deterministic Production Skills。
6. Worker 根据 `story_inputs` 和前置 task output envelope 构建每个 skill 输入。
7. Worker 将 output envelope 通过后端 persistence service / repository 写入 `generation_tasks.output_json`、`assets` 和可选 `cost_ledger`。
8. `ok=false` envelope 必须转为 failed task / failed job，不得写成 completed。
9. API SSE 或轮询只推送数据库中的真实 job / task 状态，不再用 mock 逻辑自动推进。
10. 审核节点继续通过 Control API checkpoint 确认，smoke 可以自动调用 checkpoint，但生产逻辑不能静默跳过人工确认边界。
11. 如果任务队列包含 `export_packager`，补齐 deterministic mock skill，只输出导出包占位结构，不生成真实 ZIP / MP4。
12. 前端通过 Control API 展示真实任务结果、skill envelope 摘要、资产索引、质量报告和成本记录；静态 mock 仅作为 API 不可用时的降级。

验收：

- 本机存在 `milu` 数据库，`root/root` 可连接。
- `/api/system/preflight` 在 PostgreSQL provider 下返回 healthy。
- `/api/system/migrations` 显示所有 SQL migration 已应用。
- API 和 Worker 共享 PostgreSQL 中同一份项目、任务、资产和成本状态。
- Worker 能领取 waiting task，调用 Python deterministic skill，并写回 envelope。
- API / Worker 重启后，项目进度、checkpoint、失败原因和已完成 skill 输出仍能恢复。
- 前端不直接访问数据库、文件系统、Python 脚本或 FFmpeg，只通过 Control API 展示真实结果。
- 不接真实模型、不读取真实媒体、不触发 FFmpeg、不生成真实媒体文件。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。

当前落地状态：

- 已完成。默认开发 provider 已切到 PostgreSQL，InMemory 仅保留为快速 smoke / 特殊轻量场景。
- 已创建 `milu` 数据库并通过 Control API migration service 应用 SQL migrations。
- Worker 已通过 `PythonProductionSkillRunner` 调用 Python CLI / `SkillGateway`，并通过 `SkillEnvelopePersistenceService` 写回 `generation_tasks.output_json`、`assets` 和 `cost_ledger`。
- 已把 `plot_adaptation`、`image_prompt_builder`、`video_prompt_builder`、`export_packager` 纳入完整 queue，端到端链路为 Stage 5-13。
- API SSE 已改为读取数据库快照，不再由 API mock 自动推进生产状态。
- 前端已通过 Control API 展示真实 tasks、skill envelope 摘要、资产索引和成本记录；静态 mock 仅作 API 不可用时降级。
- 本地验证通过：`job_ba4b02d1cd534e948fe0fda74aaead3c` 为 completed / 100，15 个 task 全部 completed 且均有 output envelope，cost ledger 15 行。

### 阶段 14：打包前补丁与 Stage 13 收敛

目标：在进入桌面端之前，先把 Stage 13 收敛验收后暴露出的核心产品链路、运行配置、测试和演示可信度问题修掉，避免把仍有明显纰漏的前后端状态固化进安装包。

本阶段决策：

- 原阶段 14 桌面打包继续顺延为阶段 15。
- 本阶段不创建 `apps/desktop`，不接 Electron，不做安装器，不接真实模型，不读取真实媒体，不触发 FFmpeg。
- 继续保持 UI -> Control API -> Worker -> Python Skills / PostgreSQL 的边界。
- UI 不直接访问数据库、文件系统、Python 脚本或 FFmpeg。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。

步骤：

1. 打通真实用户输入链路：前端故事文本、标题、模式、时长、画幅和风格选择必须通过 Control API 保存，再由 Worker 读取最新数据进入 `story_intake`。
2. 扩展 `UpdateProjectRequest`、`ProjectService` 和 repository，实现 `story_inputs.original_text`、word count 和项目描述更新。
3. 为故事文本、目标时长、画幅和模式增加 API / UI 一致的校验与错误展示；故事文本按当前产品建议限制在 500 到 2000 字范围。
4. 统一 Control API 默认端口和 base URL：对齐 `launchSettings.json`、README、`VITE_CONTROL_API_BASE`、开发脚本和后续桌面进程管理策略，消除 `5268` / `5368` 双轨。
5. 明确 Stage 15 桌面宿主向 Web UI 注入 Control API base URL 的方式；Electron 只传配置，不把数据库、文件系统或 Python 能力暴露给 UI。
6. 收敛 CORS / 本地承载策略：覆盖 Vite dev、Control API static hosting 或 Electron 本地 HTTP 承载，避免随机端口或 file URL 环境下无法访问 API。
7. 检查 Web 路由在桌面静态 / 本地 HTTP 环境下的可用性；必要时采用 hash route 或本地 host 的 History API fallback。
8. 清理演示期 UI 残留：`Stage 1 mock` 文案、无处理器的 `输出目录`、`锁定`、`重生成` 等按钮必须改为禁用、隐藏或接入 Control API。
9. 增加 checkpoint 基本人工确认语义：至少区分 approve / reject / notes，避免前端硬编码全部 approve。
10. 防止同一项目重复启动多个 running / paused production job；当用户点击重新生成时，应先淘汰同项目未完成旧 job，再按当前已保存输入创建新 job。
11. 加固 PostgreSQL 默认 provider 语义：配置缺失时不应静默回落 InMemory；如使用 InMemory，必须显式配置并在 preflight / UI 中标注。
12. 修正过期注释和文档，例如 `001_initial_control_plane.sql` 中仍称 runtime 使用 in-memory repository 的说明。
13. 为 `PythonProductionSkillRunner` 增加 `.tmp\skill-runs` 清理或保留策略，避免长期运行积累临时目录。
14. 补 API / Worker / PostgreSQL 自动化集成测试或 PowerShell 测试脚本，覆盖 migration、项目创建 / 更新、生产任务、checkpoint、API / Worker 重启恢复、lease 过期接管和失败重试。
15. 增加 Python Skills 契约检查，确认 runtime registry、`skill.yaml`、schema 和 validator 不漂移。
16. 阶段完成后检查并同步根 `README.md`，保持中文、PowerShell 友好和面试展示导向。

验收：

- Web UI 修改后的故事和参数能保存到 PostgreSQL，并被后续 Worker / Python deterministic skills 消费。
- 前端、README、launchSettings 和验证命令只有一套明确的 Control API 默认端口；如使用动态端口，必须有配置注入机制。
- CORS / API base URL / 路由策略满足 Stage 15 桌面宿主前置需求。
- UI 不再展示过期 mock 阶段文案；无效按钮不会误导用户。
- PostgreSQL 是默认业务事实来源，InMemory 仅显式启用。
- skill run 临时目录有可解释的清理或保留策略。
- 自动化测试覆盖 Stage 13 端到端链路的关键恢复路径。
- 本阶段不生成桌面安装包，桌面打包进入 Stage 15。

当前落地状态：

- 已完成。Web UI 的故事文本、标题、模式、目标时长、画幅和风格会通过 Control API 保存到 PostgreSQL，启动生产前先保存草稿。
- 已扩展项目更新 API / repository，`story_inputs.original_text`、word count 和项目描述可随用户输入更新。
- Control API 默认端口统一为 `http://127.0.0.1:5368`，前端优先使用桌面宿主注入的 `window.__MILUSTUDIO_CONTROL_API_BASE__`，再回退到 `VITE_CONTROL_API_BASE` 和默认端口。
- CORS 已放行本地 loopback 源，前端支持 hash route，Stage 15 可用本地 HTTP 或静态承载继续推进。
- UI 已清理 `Stage 1 mock` 文案和无处理器按钮；checkpoint 支持 approve / reject / notes，并新增 `generation_tasks.checkpoint_notes` migration。
- 同一项目重新生成时会先将未完成旧 job 标记为已被新输入取代，再创建新 job；避免旧剧本输出继续串到当前输入。
- PostgreSQL 是缺省 provider，InMemory 需要显式配置；`001_initial_control_plane.sql` 旧注释已修正。
- `.tmp\skill-runs` 默认保留最近 30 次运行，可通过 `ControlPlane:SkillRunRetentionCount` 调整。
- 新增 `scripts\windows\Test-MiLuStudioStage14Integration.ps1`，覆盖 API / Worker / PostgreSQL 关键恢复路径。
- 新增 `backend\sidecars\python-skills\tests\test_stage14_skill_contracts.py`，检查 registry、`skill.yaml`、schema、executor 和 validator 契约漂移。

### 阶段 15：桌面打包

目标：形成老板可演示、可售卖的 Windows 安装包。

桌面端采用唯一方案：`Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。不再并列 Inno Setup、MSIX、Squirrel 或 Velopack 作为 MVP 主方案。

桌面端是独立交付 part，必须在 Web UI、Control API、Worker、PostgreSQL adapter 和核心生产功能相对稳定后推进。它不定义数据库，不执行 migrations，不直接访问 PostgreSQL。

步骤：

1. 创建 `apps/desktop`。
2. 使用 Electron 承载 Web UI。
3. 启动本地 Control API。
4. 启动 Windows Worker。
5. 启动 Python Sidecar。
6. 随机端口绑定。
7. 调用 Control API health / preflight 检查数据库、storage、Python runtime 和 Worker 状态；桌面端只展示结果和修复引导。
8. 初始化或检查 storage 目录只能通过 Control API / 后端 service 完成，Electron 不直接写业务目录。
9. 托盘菜单：打开、重启服务、打开输出目录、查看日志、退出。
10. 使用 electron-builder + NSIS 打包，设置 `oneClick=false`、`allowToChangeInstallationDirectory=true`、`runAfterFinish=true`。
11. 配置 `win.icon`、`nsis.installerIcon`、`nsis.uninstallerIcon`、`nsis.installerHeaderIcon`、`shortcutName` 和应用 AppUserModelID。
12. 增加自定义 `build/installer.nsh`，用于付费码 / 激活码输入页、开机自启动选项、安装完成页文案和必要的安装后动作。
13. 安装器必须提供用户可选项：桌面快捷方式、开始菜单快捷方式、开机自启动、安装完成后运行。
14. 安装器必须显示安装进度条。
15. 预留登录 / 注册 / 授权入口和前端路由，但不作为桌面安装包 MVP 的阻塞项。
16. 生成 `MiLuStudio-Setup-<version>.exe`。

验收：

- 干净 Windows 环境安装后可启动。
- 用户无需手动启动后端。
- 桌面端只通过 Control API 检查和使用数据库，不直接访问数据库、文件系统业务目录、Python 脚本或 FFmpeg。
- 桌面包默认携带 self-contained .NET API / Worker 和 Python runtime，避免依赖开发机固定的 `dotnet.exe` 或 Python 路径。
- 如果数据库未就绪，桌面端展示 Control API preflight 返回的错误和修复建议，不自行创建或迁移数据库。
- 用户可选择安装目录、桌面快捷方式、开始菜单快捷方式、开机自启动和安装后启动。
- 安装器图标、卸载器图标、应用图标和快捷方式图标均使用 MiLuStudio 自定义图标。
- 安装器可配置付费码 / 激活码门槛；无效码不能继续安装。
- 激活码门槛不作为唯一授权安全边界；应用内登录注册和账号授权留给桌面 MVP 后下一大阶段。
- 任务栏固定不做静默强制 pin；必须提供正确快捷方式和用户引导，允许用户自行固定。
- 卸载不删除用户生成素材，除非用户选择。
- 日志可定位错误。

当前落地状态：

- 已完成。新增 `apps\desktop`，Electron 主进程承载现有 Web UI 构建产物，并通过本地 HTTP host 提供 hash route fallback、CSP 和静态资源服务。
- 桌面宿主会随机绑定本地端口，启动发布后的 Control API 与 Windows Worker，并通过 preload 向 Web UI 注入 `window.__MILUSTUDIO_CONTROL_API_BASE__`。
- Web UI 已新增桌面诊断面板，通过 Electron IPC 展示 Control API health、preflight、PostgreSQL、storage、Python runtime、Python skills root、Worker 和 Web host 状态；桌面写请求通过 preload 注入的 `window.__MILUSTUDIO_DESKTOP_TOKEN__` 添加会话令牌。
- 桌面端不直接访问 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg；migration、schema、数据库初始化和 storage 检查仍属于 Control API / 后端 service 边界。
- 新增 `scripts\windows\Prepare-MiLuStudioDesktopRuntime.ps1`，将 Web dist、self-contained API / Worker 发布产物、SQL migrations、Python deterministic skills 和 `python-runtime` 复制到 `apps\desktop\runtime`。
- 新增 `scripts\windows\Test-MiLuStudioDesktop.ps1`，覆盖 runtime 准备、桌面 TypeScript build、Electron smoke 和桌面 API 安全验证；`scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1` 可单独验证桌面令牌与 migration apply 禁止语义。
- 打包图标、安装器图标、卸载器图标、header 图标、快捷方式图标和托盘图标均来自 `apps\web\public\brand\logo.png` 生成的多尺寸 `apps\desktop\build\icon.ico`。
- Electron 已升级到 `42.0.1`；主进程限制外部导航、弹窗和 IPC 来源，`userData`、`sessionData` 和 logs 已显式指向 D 盘数据目录，避免默认落到 `C:\Users\...\AppData\Roaming`。
- electron-builder + NSIS 已生成 `D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe`，配置 `oneClick=false`、`allowToChangeInstallationDirectory=true`、`runAfterFinish=true`、`shortcutName=MiLuStudio` 和自定义 `installer.nsh`。
- `installer.nsh` 当前只保留桌面快捷方式、开始菜单快捷方式和开机自启动复选项；安装前激活码页已按当前 MVP 范围撤下。
- Stage 19 已补桌面发布验收脚本和签名前置检查；当前本机生成的 NSIS 安装包 Authenticode 状态仍为 `NotSigned`，正式商业发布前必须配置真实代码签名证书并使用 `-RequireSigned` 阻断未签名产物。

参考：

- XiaoLouAI Windows 原生方向。
- MiLuAssistantDesktop 的经验，但不复制旧项目。
- Toonflow electron-builder 配置。
- LocalMiniDrama 桌面本地体验。
- electron-builder NSIS 官方文档。
- NSIS custom page / nsDialogs 官方文档。
- NSIS best practices 中关于不要程序化固定任务栏的要求。

### 阶段 16：账号、会话、设备绑定和许可证授权

目标：在桌面安装包 MVP 之后补齐应用内注册、登录、设备绑定和许可证授权系统。

本阶段正式范围：

- Control API / Auth & Licensing adapter 统一处理账号、会话、设备绑定、许可证状态和授权错误。
- Electron 与 Web UI 只通过 Control API 展示登录 / 注册 / 激活入口和授权状态。
- 安装器激活码页只作为安装前门槛占位，不作为唯一授权边界。
- 本阶段仍不接真实模型、不读取真实媒体、不触发 FFmpeg、不生成真实 MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。
- 不让 UI 或 Electron 绕过 Control API / Worker 边界。
- 不让桌面端执行 migrations、定义数据库表或负责数据库初始化。

步骤：

1. 建立账号、会话、设备、许可证和授权状态领域模型。
2. 补后端 migration 和 PostgreSQL repository，保持数据库变更只属于 Control API / Infrastructure。
3. 增加 Auth & Licensing service / adapter 边界，先使用本地 deterministic adapter 和测试激活码。
4. 增加注册、登录、退出、当前账号、授权状态、设备绑定、激活码兑换或校验 API。
5. 为 Web UI 增加登录 / 注册 / 激活入口；未登录或未授权时不进入工作台。
6. 为现有项目、生产任务和设置类 API 增加最小授权门禁与清晰错误 DTO。
7. 增加 PowerShell 集成测试，覆盖注册、登录、会话刷新、设备绑定、授权状态和未授权阻断。
8. 同步 README、阶段计划、任务记录和短棒交接。

验收：

- 未登录用户只能看到登录 / 注册 / 激活入口。
- 登录成功后可进入现有项目工作台。
- 未授权或许可证失效时，受保护 API 返回明确授权错误，Web UI 可展示原因和下一步动作。
- 设备绑定由 Control API / Auth & Licensing adapter 管理，Electron 不直接读写数据库或本机授权文件。
- 安装器激活码页与应用内授权状态边界清楚。
- 自动化脚本覆盖主要认证与授权路径。

当前落地状态：

- 已完成。账号、会话和设备绑定已由 Domain / Application / Infrastructure 分层实现。
- 新增 `004_stage16_auth_licensing.sql`，通过后端 migration 创建 `accounts`、`auth_sessions`、`devices` 和 `licenses`，数据库 schema 仍不属于 Electron 或安装器。
- 当前 MVP 已撤下许可证 / 激活码 / 付费码体验，`licenses` 表仅作为后续商业化预留，不驱动当前 Web / Desktop。
- Control API 当前开放 `/api/auth/register`、`/api/auth/login`、`/api/auth/refresh`、`/api/auth/logout`、`/api/auth/me` 和 `/api/auth/devices/bind`。
- Web UI 已新增登录 / 注册门面；登录后直接进入项目列表、项目详情和生产控制台。
- 项目、生产任务和 generation task 写回类 API 已加最小登录门禁；桌面令牌只保护桌面 unsafe HTTP 方法，不替代应用内账号登录。
- Stage 16 PowerShell 集成测试已覆盖注册、登录、会话刷新、设备绑定、未登录阻断、登出失效和重新登录。
- Stage 14 生产链路回归脚本已补账号 bootstrap，确认登录门禁不破坏 Worker / Python deterministic skills 链路。

Stage 17 当前落地状态：

- 已完成生产控制台体验收敛：中文化、checkpoint 审核解释、真实产物预览、未开始状态去 mock、重新生成消费当前输入。
- 已完成中文测试 fixture 与《牡丹亭》人物抽取修复。
- 已完成 `storyboard_director` 的 `cinematic_md_v1` 分镜稿结构基础对齐；下游仍消费原 `shots`，严格 120 秒 / 50-55 镜头 / 逐句对白保真留给后续模型 adapter 和 validator。
- Stage 17 已正式确认为生产控制台可编辑能力，并已完成分镜表编辑、单镜头备注驱动本地确定性重算、保存后重置下游任务、Control API 新接口和 Web 结果卡编辑 UI。
- 新增 `PATCH /api/generation-tasks/{taskId}/storyboard` 与 `POST /api/generation-tasks/{taskId}/storyboard/shots/{shotId}/regenerate`；两个接口只写回 `storyboard_director` JSON envelope，不接真实模型、不读真实媒体、不触发 FFmpeg、不生成真实 MP4 / WAV / SRT / ZIP。
- 新增 `scripts\windows\Test-MiLuStudioStage17StoryboardEditing.ps1`，覆盖完整 deterministic job 完成后编辑分镜、下游任务重置、单镜头重算和 no-provider/no-media 边界。

Stage 18 当前落地状态：

- Stage 18 已正式确认为真实 provider adapter 前配置页，并已完成 Web “模型”设置页、Control API settings endpoint、Application service 和 Infrastructure 本地文件 repository。
- 新增 `GET /api/settings/providers`、`PATCH /api/settings/providers` 和 `GET /api/settings/providers/preflight`；这些接口只管理本地占位配置，不访问真实 provider。
- 支持 Text / Image / Video / Audio / Edit 五类 adapter 的供应商、默认模型、启用开关、API Key 占位、单项目成本上限和失败重试次数。
- API Key 请求体只被转换成遮罩和 SHA256 指纹；API 响应与本地 provider settings 文件不保存、不返回可用于真实调用的明文 key。
- Provider preflight 明确 `adapterMode=placeholder_only`、`externalNetwork=disabled`、`mediaGenerated=false`，不读取真实媒体、不触发 FFmpeg、不生成真实 MP4 / WAV / SRT / ZIP。
- 本阶段不新增数据库 migration；provider 前配置由 Control API / Infrastructure 写入 D 盘 storage 下的本地 JSON，UI / Electron 不直接读写。
- 新增 `scripts\windows\Test-MiLuStudioStage18ProviderSettings.ps1`，覆盖配置保存、明文 key 不泄漏、preflight 占位边界和 clear key。

Stage 19 当前落地状态：

- Stage 19 已正式确认为桌面发布验收与代码签名前置准备，并已完成 `scripts\windows\Test-MiLuStudioStage19DesktopRelease.ps1`。
- `apps\desktop\package.json` 新增 `verify:release` 和 `verify:release:signed`；前者记录未签名阻塞项，后者要求安装器和主 exe Authenticode 状态为 `Valid`。
- Stage 19 脚本可检查 electron-builder / NSIS 配置、安装器与 `win-unpacked` 产物、Web dist、Control API / Worker runtime、Python runtime、Python Skills、最新 backend migration、Electron 安全设置、桌面 Web host CSP / nosniff、安装器快捷方式 / 自启动脚本和桌面 Control API 边界。
- 联网自检后已补强桌面本地 Web host 响应头：CSP 增加 `form-action 'self'` 和 `frame-ancestors 'none'`，并把 CSP / `X-Content-Type-Options: nosniff` 纳入 Stage 19 验收脚本。
- Stage 19 重打包发现旧 `outputs\desktop` 产物缺少 `004_stage16_auth_licensing.sql`，已通过 `-BuildPackage` 重新生成并纳入验收。
- `.gitignore` 已忽略 `*.pfx`、`*.p12`、`*.pvk`、`*.spc` 和 `*.key`，避免代码签名证书或私钥容器进入仓库。
- 新增 `docs\MILUSTUDIO_STAGE19_DESKTOP_RELEASE_CHECKLIST.md`，记录签名前置配置、干净 Windows 安装 / 卸载 / 自启动 / 快捷方式手工验收步骤。
- 当前本机安装器和 `win-unpacked\MiLuStudio.exe` 仍为 `NotSigned`；这是正式公开发布前阻塞项，不伪造签名成功。

Stage 20 当前落地状态：

- Stage 20 已正式确认为 Codex 式前端工作台重构，并已完成登录后主入口替换。
- Web 工作台采用左侧历史项目、中央单输入框、右侧固定流程进度与生成结果、左下角设置入口。
- 工作台已补 Codex 式附件卡片和加号上传菜单，并按当前生产阶段启用文本 / 图片 / 视频入口。
- 媒体附件只记录文件名、类型和大小，不读取真实媒体、不解析帧、不触发 FFmpeg、不扩展后端真实媒体上传链路。

Stage 21 当前落地状态：

- Stage 21 已正式确认为新工作台结构化产物编辑增强，并已完成角色、画风、图片提示词和视频提示词编辑。
- 新增 `PATCH /api/generation-tasks/{taskId}/structured-output`，只允许编辑白名单顶层字段，保存对象仍是 generation task 的 JSON envelope。
- 角色与画风编辑后回到 review / paused；图片提示词与视频提示词编辑后保持 completed / running；所有下游任务会重置为 waiting 并清空旧 output。
- 保存后的 envelope 会记录 `stage21_edit_summary`，明确 `model_provider=none`、`media_read=false`、`media_generated=false`、`ffmpeg_invoked=false`。
- 新增 `scripts\windows\Test-MiLuStudioStage21StructuredOutputEditing.ps1`，覆盖完整 deterministic 生产链路、四类结构化产物编辑和下游重置边界。

Stage 22 当前落地状态：

- Stage 22 已正式确认为 Provider Adapter 安全前置层设计与占位落地。
- Provider settings 响应新增 `safety` 状态，覆盖 metadata-only secret store、spend guard 和 provider sandbox。
- 新增 `IProviderSecretStore` 与 `FileProviderSecretStore`，只在 `provider-secrets.local.json` 保存遮罩、SHA256 指纹和不可调用 secret reference，不保存明文 key，不提供可用于真实 provider 调用的 secret material。
- 新增 `GET /api/settings/providers/safety` 和 `POST /api/settings/providers/spend-guard/check`；spend guard 可以判断预算与重试边界，但在 Stage 22 仍统一返回真实 provider 调用阻断。
- Provider preflight 新增 `secret_store`、`spend_guard` 和 `provider_sandbox` 三个安全前置检查，并继续为各 adapter 标明 `providerCalls=blocked`、`externalNetwork=disabled`、`mediaRead=false`、`ffmpegInvoked=false`。
- Web “模型”设置页已展示 Stage 22 安全前置层状态和真实 provider 调用阻断，不绕过 Control API 读取本地文件。
- 新增 `scripts\windows\Test-MiLuStudioStage22ProviderSafety.ps1`，覆盖密钥明文不泄漏、安全状态、preflight、预算阻断、重试阻断和 clear key。
- 本阶段不新增数据库 migration，不接真实 provider，不读取真实媒体，不触发 FFmpeg，不生成真实 PNG / MP4 / WAV / SRT / ZIP。

## 13. 模型与供应商策略

### 13.1 Provider 分类

`TextProvider`

- 剧本解析。
- 角色抽取。
- 分镜生成。
- 提示词生成。
- 质检。

`ImageProvider`

- 角色图。
- 场景图。
- 分镜图。
- 首帧/尾帧。

`VideoProvider`

- 文生视频。
- 图生视频。
- 首尾帧视频。

`AudioProvider`

- TTS。
- 音色试听。
- BGM / SFX。

`EditProvider`

- FFmpeg。
- Remotion 可选。
- 字幕对齐。

### 13.2 第一版配置页

只暴露：

- API Key。
- 默认文本模型。
- 默认图片模型。
- 默认视频模型。
- 默认配音模型。
- 单项目成本上限。
- 失败重试次数。

Stage 18 已先落地“provider adapter 前配置页”：

- 路由为 `/settings/providers`。
- 后端 endpoint 为 `/api/settings/providers`、`/api/settings/providers/preflight`、`/api/settings/providers/safety` 和 `/api/settings/providers/spend-guard/check`。
- 当前保存的是本地占位配置和 metadata-only secret 描述，不保存明文 API Key，不触发任何真实 provider 请求。
- `TextProvider`、`ImageProvider`、`VideoProvider`、`AudioProvider` 和 `EditProvider` 的真实 SDK / HTTP adapter 仍未实现。
- Stage 22 已把 secret store、spend guard 和 provider sandbox 作为真实 provider 接入前的硬前置层；当前仍处于 placeholder-only / sandbox-blocked 状态。

不要暴露：

- Prompt 工程细节。
- Skill 文件路径。
- 复杂 pipeline 图。
- provider 内部参数全集。

## 14. 成本与安全

必须从第一版开始记录成本。

每个任务记录：

- 使用了哪个 provider。
- 使用了哪个 model。
- 输入 token / 图片张数 / 视频秒数 / 音频秒数。
- 预计费用。
- 实际费用。
- 失败次数。
- 是否重试。

安全要求：

- API Key 只存在本机；Stage 22 当前只保存遮罩、指纹和不可调用引用，真实 secret material 仍不落库、不进响应、不交给 provider。
- 不把用户故事、图片、音频上传到自有服务器。
- 不提交 `.env`、真实素材、输出视频、服务账号。
- 所有本地生成数据放入 `storage/` 或用户数据目录。
- Git 默认忽略 `storage/`、`outputs/`、`logs/`、`.env*`、`*.mp4`、`*.wav` 等。
- 任何后续真实 provider 调用必须先通过 secret store、spend guard 和 sandbox 前置检查，并继续由 Control API / Worker 边界编排。

## 15. 下一会话建议执行顺序

下一会话不要直接大规模写业务逻辑。按这个顺序做：

1. 确认本文件。
2. 初始化 MiLuStudio Git 仓库。
3. 添加根 `.gitignore`。
4. 添加 `README.md`。
5. 添加 `docs/REFERENCE_PROJECTS.md`。
6. 添加 `docs/PRODUCT_SPEC.md`。
7. 搭 `apps/web` 前端壳。
8. 做 mock 流程。
9. 再搭 `.NET Control API`。
10. 再搭 Python Skills Runtime。
11. 再接第一条真实 skill：`story_intake`。

如果时间不够，第一轮只做 1 到 8。

## 16. 下一会话开工提示词

可以直接对 Codex 说：

```text
读取 D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md，按文档阶段 0 和阶段 1 开始搭建 MiLuStudio。先初始化项目、添加 .gitignore、README、docs/REFERENCE_PROJECTS.md、docs/PRODUCT_SPEC.md，然后搭建 apps/web 的 Vite React TypeScript UI 壳。注意不要复制参考项目源码，不要引入 Linux/Docker 作为生产依赖，不要实现复杂无限画布，优先完成可运行的 Windows 原生 AI 漫剧 Agent 产品骨架。
```

## 17. 需要避免的偏离

- 不要把 ArcReel 当主仓库。
- 不要把 LumenX UI 直接搬过来。
- 不要把 AIComicBuilder 的 Next.js 架构作为主干。
- 不要把 LocalMiniDrama 的 Vue / Express 技术栈作为主干。
- 不要开放公共 skill 市场。
- 不要让用户配置复杂模型参数。
- 不要第一版做完整长剧。
- 不要第一版做多用户协作。
- 不要第一版做云端 SaaS。
- 不要把 Docker 写成生产必需。

## 18. MVP 完成标准

技术上：

- Windows 本地启动。
- 前端可用。
- 后端可用。
- Worker 可跑。
- Python Skill 可执行。
- SSE 进度可显示。
- PostgreSQL 数据可恢复。
- 资产可下载。

产品上：

- 用户输入故事。
- 系统生成脚本。
- 系统生成角色。
- 系统生成分镜。
- 系统生成图片。
- 系统生成视频。
- 系统生成字幕。
- 系统合成 MP4。
- 用户可以在关键节点确认和修改。

简历表达上：

> 负责从 0 到 1 规划并搭建 Windows 原生 AI 漫剧 Agent 产品 MiLuStudio，基于 XiaoLouAI 现有工程体系，调研 AIComicBuilder、LumenX、LocalMiniDrama、ArcReel、Toonflow、OpenMontage 等开源项目后，沉淀出内置 Production Skills、任务状态机、SSE 进度、成本追踪和一键成片工作流。

## 19. 文档、任务记录和阶段自检约束

本节是长期协作约束。除非产品方向、架构边界、阶段验收标准或联网自检结果发生变化，否则不要轻易修改本文件。

### 19.1 文件分工

- `docs\MILUSTUDIO_BUILD_PLAN.md` 是总参考。
- `docs\MILUSTUDIO_PHASE_PLAN.md` 是总任务阶段安排。
- `docs\MILUSTUDIO_TASK_RECORD.md` 是修改总记录和阶段自检记录。
- `docs\MILUSTUDIO_HANDOFF.md` 是短棒交接，只记录当前阶段、下一棒提示词和最新风险。

更新顺序：

1. 先读总参考。
2. 再读总任务阶段安排。
3. 再读修改总记录。
4. 最后读短棒交接。
5. 大阶段结束后先更新总任务阶段安排。
6. 再把本地验证、联网自检和修改原因写入总任务记录。
7. 如果联网自检发现方向偏差，先最小修改总参考和总任务阶段安排。
8. 每次修改完成后自行检查根 `README.md` 是否需要同步更新。
9. 最后更新短棒交接。

### 19.2 PowerShell 友好文档格式

项目内所有 Markdown 文档必须便于 PowerShell 阅读。

要求：

- 使用 UTF-8 Markdown。
- 优先使用普通标题、短段落、普通列表和 `text` 代码块。
- 尽量避免宽表格、超长单行、隐藏折叠块和依赖网页渲染的格式。
- 路径使用 Windows 路径，例如 `D:\code\MiLuStudio`。
- 除根 `README.md` 外，长期说明、阶段计划和专题文档优先放入 `docs/`。
- 关键 owner、决策、验证命令、下一步任务尽量一事一行。
- 新增文档后必须能用 `Get-Content -Encoding UTF8` 正常阅读。

根 `README.md` 额外要求：

- 必须使用中文，便于后续面试展示。
- 必须 PowerShell 友好，能用 `Get-Content .\README.md -Encoding UTF8` 顺畅阅读。
- 内容风格参考 `D:\code\BookRecommendation\README.md` 和 `https://github.com/White-147/BookRecommendation`：项目背景、项目功能、技术栈、系统架构、目录结构、核心链路、运行说明、项目亮点、文档导航和后续改进方向。
- README 要面向“面试官快速理解项目价值”，不要只写开发状态流水账。
- 可以使用小型 Markdown 表格和 `mermaid` 代码块，但避免超宽表格、长链接堆砌、HTML 折叠块和只能靠网页渲染理解的内容。
- 每次完成代码或文档修改后，都要判断 README 是否因项目状态、架构路线、运行命令、阶段计划或面试展示内容变化而需要同步更新。
- 如果需要更新 README，优先更新根 `README.md`；如果不需要，也要在最终回复或任务记录中说明已检查。

### 19.3 大阶段结束后的联网自检

每个大阶段结束后必须联网搜索一次，确认下一步没有偏离当前开源生态、模型能力、授权边界和 Windows 原生目标。

自检步骤：

1. 总结本阶段实际完成内容。
2. 联网搜索同类 AI 漫剧、AI 视频 Agent、桌面端一键生成、Production Skills、模型 provider 或 Windows 打包方案的最新变化。
3. 对照本文件的产品定位和技术约束。
4. 如果发现偏差，先更新 `docs\MILUSTUDIO_BUILD_PLAN.md` 的相关总约束。
5. 再更新 `docs\MILUSTUDIO_PHASE_PLAN.md` 的阶段任务、验收和下一步安排。
6. 将本地验证、联网自检、偏差原因和修改摘要写入 `docs\MILUSTUDIO_TASK_RECORD.md`。
7. 最后更新 `docs\MILUSTUDIO_HANDOFF.md`，给下一棒留下短记录。

注意：

- 联网自检不是为了追新功能，而是为了防止方向跑偏。
- 发现 AGPL、非商业授权、云端 SaaS 优先、Linux/Docker 依赖增强等风险时，必须在记录中明确标注。
- 没有偏差也要在总任务记录中留下自检摘要。

### 19.4 总需求覆盖

总需求必须长期保持：

- MiLuStudio 是新的项目，不覆盖旧 MiLuAssistantWeb / MiLuAssistantDesktop。
- 旧 MiLuAssistant 项目只作为历史经验，不作为新项目代码来源。
- XiaoLouAI / MiLu 自有体系是主干。
- 产品优先 Windows 原生、稳定、简单、可售卖。
- 用户角色是决策层，不是技术配置者。
- Skills 是内置 Production Skills，不开放为公共插件市场。
- 参考项目只参考流程、数据结构、调度思想和交付形态。
- 每个阶段都要有明确验收标准、验证记录和下一棒提示词。
- 根目录保持简洁，长期文档统一归入 `docs/`，实现目录优先按路由或功能聚合。
- 依赖、配置、运行文件、缓存、日志、数据库、上传素材和生成结果必须约束在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录内。
- Python、Node.js、.NET、Electron 等外部依赖如需安装或下载缓存，必须限制在 D 盘，不得污染 C 盘。

### 19.5 软件设计硬约束

实际编码必须符合高内聚、低耦合、职责单一、关注点分离和依赖倒置等原则。  
这些原则是编码期硬约束，但不能变成阻碍功能落地的空转抽象。

核心约束：

- 一个模块只围绕一个稳定业务能力组织代码，例如 project、chat、task、skill、asset、provider、packaging。
- 前端、后端和内部 Production Skills 的目录优先按路由或功能域聚合，不按零散文件类型铺开。
- 高频变化点必须隐藏在模块边界后，例如模型供应商、图片/视频生成 API、FFmpeg 调用、文件存储、任务队列、Windows 打包方式。
- UI 不直接访问数据库、文件系统、模型 SDK、FFmpeg 或 Python 脚本。
- 前端只通过 Control API 和清晰 DTO 获取状态、提交操作和展示结果。
- .NET Control API 负责鉴权、任务编排、状态推进、资产索引和 Sidecar 调用，不承载具体 AI 生成细节。
- Python Sidecar / Skills Runtime 只负责具体生产技能执行，通过稳定输入输出协议和主系统通信。
- 模型 provider、存储 provider、队列 provider、视频处理 provider 必须作为 adapter 或 service 边界隔离。
- 禁止跨层反向依赖和循环依赖。
- 禁止为了省事让 UI、Control API、Worker、Python Skill 共享隐式全局状态。
- 新增抽象必须服务于真实变化点、测试隔离或重复逻辑收敛；不要为了“看起来架构完整”提前堆接口。
- 如果 MVP 为了先跑通功能必须临时直连某能力，必须把直连限制在单一 adapter / gateway 内，并在 `docs\MILUSTUDIO_TASK_RECORD.md` 记录技术债和后续收敛点。

编码检查项：

- 这个文件或模块的职责能不能用一句话说清。
- 修改一个 provider 是否不会影响 UI 和业务编排。
- 修改一个页面是否不会影响任务状态机和 Python Skill。
- 修改一个 Skill 是否只需要调整 Skill 输入输出协议和 adapter。
- 是否存在跨层直接 new、直接读写文件、直接调用外部 SDK、直接拼接协议字段的泄漏。
- 是否存在为了复用而把无关业务塞进同一个 helper / utils 的低内聚模块。
- 是否存在功能还没出现就提前设计的大型抽象。

参考原则来源：

- Parnas, On the Criteria To Be Used in Decomposing Systems into Modules: `https://citeseerx.ist.psu.edu/document?doi=5d752e29e29b42cc509417699a98d9dca8212c83&repid=rep1&type=pdf`
- Robert C. Martin, The Single Responsibility Principle: `https://blog.cleancoder.com/uncle-bob/2014/05/08/SingleReponsibilityPrinciple.html`
- Martin Fowler, YAGNI: `https://martinfowler.com/bliki/Yagni.html`

### 19.6 D 盘封闭环境约束

MiLuStudio 的依赖、配置、运行文件和生成数据必须尽量封闭在主项目目录内。  
如果某些基础运行时必须放在项目外，也只能放在 D 盘，例如 `D:\dev` 或 `D:\tools`。  
无论如何不得主动向 C 盘写入项目依赖、缓存、配置、运行数据或生成素材。

项目内优先目录：

```text
D:\code\MiLuStudio\.tools
D:\code\MiLuStudio\.venv
D:\code\MiLuStudio\.cache
D:\code\MiLuStudio\.config
D:\code\MiLuStudio\.tmp
D:\code\MiLuStudio\.nuget
D:\code\MiLuStudio\.ms-playwright
D:\code\MiLuStudio\node_modules
D:\code\MiLuStudio\runtime
D:\code\MiLuStudio\storage
D:\code\MiLuStudio\uploads
D:\code\MiLuStudio\outputs
D:\code\MiLuStudio\logs
```

外部工具目录只允许使用 D 盘：

```text
D:\dev
D:\tools
D:\models
```

运行环境必须显式约束常见缓存位置：

```text
TMP=D:\code\MiLuStudio\.tmp
TEMP=D:\code\MiLuStudio\.tmp
npm_config_cache=D:\code\MiLuStudio\.cache\npm
PNPM_HOME=D:\code\MiLuStudio\.cache\pnpm
COREPACK_HOME=D:\code\MiLuStudio\.cache\corepack
YARN_CACHE_FOLDER=D:\code\MiLuStudio\.cache\yarn
PIP_CACHE_DIR=D:\code\MiLuStudio\.cache\pip
PIP_CONFIG_FILE=D:\code\MiLuStudio\.config\pip\pip.ini
PYTHONUSERBASE=D:\code\MiLuStudio\.python-userbase
NUGET_PACKAGES=D:\code\MiLuStudio\.nuget\packages
DOTNET_CLI_HOME=D:\code\MiLuStudio\.dotnet
ELECTRON_CACHE=D:\code\MiLuStudio\.cache\electron
ELECTRON_BUILDER_CACHE=D:\code\MiLuStudio\.cache\electron-builder
PLAYWRIGHT_BROWSERS_PATH=D:\code\MiLuStudio\.ms-playwright
HF_HOME=D:\code\MiLuStudio\.cache\huggingface
TRANSFORMERS_CACHE=D:\code\MiLuStudio\.cache\huggingface\transformers
```

禁止事项：

- 不使用默认会写入 `C:\Users\...\AppData` 的全局 npm / pip / NuGet / Electron 缓存。
- 不使用全局 `npm install -g`、全局 pip 安装或写入用户目录的工具配置。
- 不把 PostgreSQL、Redis、任务队列、模型缓存、上传素材和生成结果放到 C 盘。
- 不把 Windows 安装包生成过程中的临时目录、下载缓存和构建产物放到 C 盘。
- 不把真实用户数据、素材、模型密钥和运行日志提交到 Git。

如果某个工具无法避免写入 C 盘，必须先停下并记录原因，由用户确认是否继续。

### 19.7 目录组织约束

项目目录应保持简洁，参考 `D:\code\XiaoLouAI` 的顶层克制风格。

根目录只放少量入口文件和一级能力目录：

- `README.md`
- `.gitignore`
- `docs\`
- `apps\`
- `backend\`
- `packages\`
- `scripts\`
- `runtime\`
- `storage\`
- `uploads\`
- `outputs\`
- `logs\`

组织原则：

- 长期文档、阶段计划、专题说明和交接记录统一放在 `docs\`。
- 前端目录按路由、页面或业务 feature 聚合，例如 `projects`、`production-console`、`settings`。
- 后端目录按业务能力和层边界聚合，例如 `project`、`production-job`、`asset`、`provider`、`skill`。
- Production Skills 按技能目录聚合，每个技能保留自己的 schema、prompt、executor、validator 和 examples。
- 不为了“分类整齐”把同一个功能拆散到多个低信息量目录。
- 新增顶层目录前必须确认它代表稳定的一类能力，而不是临时文件收纳箱。
