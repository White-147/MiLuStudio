# MiLuStudio 总任务阶段安排

更新时间：2026-05-13
工作目录：`D:\code\MiLuStudio`

本文件是 MiLuStudio 的总任务阶段安排。  
它负责“下一步做什么、做到什么程度、怎么验收”。  
修改历史和自检记录写入 `docs\MILUSTUDIO_TASK_RECORD.md`。  
短棒交接写入 `docs\MILUSTUDIO_HANDOFF.md`。

## 1. 每棒先读

```powershell
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Get-Content .\README.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_BUILD_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_PHASE_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_TASK_RECORD.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_HANDOFF.md -Encoding UTF8
```

## 2. 总目标

```text
MiLuStudio 是 Windows 原生 AI 漫剧 Agent 产品。
用户输入故事、小说片段或创作要求。
系统自动生成脚本、角色、风格、分镜、图片、视频、配音、字幕、剪辑、质检和导出。
用户只在关键节点做确认、选择和轻量修改。
```

## 3. 固定执行边界

- 主干参考 `D:\code\XiaoLouAI`。
- 新项目目录 `D:\code\MiLuStudio`。
- 不复制参考项目源码。
- 不把 ArcReel / LumenX / AIComicBuilder / LocalMiniDrama 作为主仓库二开。
- 不恢复旧 MiLuAssistantWeb / MiLuAssistantDesktop 为代码来源。
- 不开放公共 Skills 市场。
- 第一版不做无限画布。
- 第一版不做复杂时间线。
- 第一版不做云端 SaaS。
- 第一版不把 Linux / Docker 作为生产必需。
- 所有文档必须 PowerShell 友好。
- 根 `README.md` 必须使用中文，并采用类似 `D:\code\BookRecommendation\README.md` 的面试展示导向结构。
- 每次修改完成后必须自行检查是否需要同步更新根 `README.md`。
- 总控文档和后续专题文档统一放入 `docs\`，根目录保持简洁。
- 前端、后端和内部 Production Skills 目录优先按路由或功能域聚合。
- 每个大阶段结束后必须联网自检。
- 实际编码必须符合高内聚、低耦合、职责单一、关注点分离和依赖倒置。
- 设计原则不能阻碍功能落地；必要的 MVP 直连必须被限制在单一 adapter / gateway 内，并记录技术债。
- 所有项目依赖、配置、运行数据、缓存、日志、上传素材和生成结果必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录内。
- Python、Node.js、.NET、Electron、Playwright、模型缓存等外部依赖不得主动写入 C 盘。

## 4. 软件设计阶段检查

每个阶段只要发生代码实现，就必须检查以下内容：

```text
1. 本阶段新增模块是否只有一个清晰职责。
2. 是否把模型供应商、FFmpeg、文件存储、队列、Windows 打包等变化点隔离在 adapter / service 边界。
3. UI 是否只通过 Control API 和 DTO 通信，没有直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
4. .NET Control API 是否只做编排、状态、资产索引和 Sidecar 调用，没有混入具体生成细节。
5. Python Skill 是否只通过稳定输入输出协议执行生产能力。
6. 是否存在循环依赖、跨层反向依赖或共享隐式全局状态。
7. 是否存在低内聚的万能 helper / utils。
8. 是否存在为了未来可能需求提前设计的大型抽象。
9. 如果为了 MVP 先跑通功能做了临时直连，是否已经隔离在单一 adapter / gateway，并写入任务记录。
10. 新增目录是否按路由或功能域聚合，没有把同一功能拆散到低信息量目录。
11. 新增依赖或运行文件是否仍然限制在项目目录或 D 盘工具目录内。
```

阶段验收时必须在 `docs\MILUSTUDIO_TASK_RECORD.md` 记录：

```text
Design check:
- cohesion:
- coupling:
- boundaries:
- temporary debt:
Environment check:
- project-local files:
- D drive only:
- C drive risk:
```

## 5. 状态说明

- `pending`：未开始。
- `in_progress`：正在进行。
- `done`：已完成且已本地验证。
- `needs_self_check`：阶段完成但还没有联网自检。
- `blocked`：阻塞，需要用户或外部条件。
- `replanned`：联网自检后发生阶段调整。

## 6. 当前焦点

```text
Current phase: Post Stage 16
Status: pending user confirmation
Goal: 由用户确认 Stage 17 的正式编号与范围。
Next handoff owner: current / next session
```

Stage 16 已完成：

- 应用内授权由 Control API / Auth & Licensing adapter 管理，不由 installer 或 Electron 主进程直接判定。
- Web UI 已新增登录 / 注册 / 激活入口；未登录或未授权时不进入项目工作台。
- 账号、会话、设备绑定和许可证状态已通过 PostgreSQL migration / repository 持久化。
- 受保护项目和生产任务 API 已增加最小授权门禁；未登录返回 401，未授权或设备超额返回 403 和清晰错误 DTO。
- 桌面端继续只承载 Web UI 并管理本地服务，不直接访问 PostgreSQL、业务文件系统、Python 脚本或 FFmpeg。
- 本阶段未接真实模型，未读取真实媒体文件，未触发 FFmpeg，未生成真实 MP4 / WAV / SRT / ZIP。

## 7. Stage 0 项目初始化

Status: done

目标：

- 建立独立 MiLuStudio 仓库。
- 写清项目定位。
- 写清参考项目边界。
- 写清第一版产品范围。

输入：

- `docs\MILUSTUDIO_BUILD_PLAN.md`
- 本文件
- `docs\MILUSTUDIO_TASK_RECORD.md`
- `docs\MILUSTUDIO_HANDOFF.md`
- 参考目录清单

具体任务：

1. 在 `D:\code\MiLuStudio` 初始化 Git。
2. 添加根 `.gitignore`。
3. 添加根 `README.md`。
4. 确认 `docs` 目录存在，四个总控文档位于 `docs\`。
5. 添加 `docs\REFERENCE_PROJECTS.md`。
6. 添加 `docs\PRODUCT_SPEC.md`。
7. 确认 `storage`、`outputs`、`logs`、`.env*`、真实素材、真实生成视频被忽略。
8. 确认文档声明 MiLuStudio 与旧 MiLuAssistantWeb / Desktop 的关系。
9. 确认文档声明参考项目只作参考，不复制源码。
10. 添加或预留 `.tools`、`.cache`、`.config`、`.tmp`、`.venv`、`.nuget`、`runtime`、`storage`、`uploads`、`outputs`、`logs` 等项目内运行目录。
11. 在文档中声明所有依赖、缓存、运行文件必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录。
12. 在后续运行脚本中必须设置 npm、pip、NuGet、Electron、Playwright、模型缓存和临时目录到项目内或 D 盘。
13. 在文档中声明根目录保持简洁，前后端和 Production Skills 按路由或功能域聚合。

推荐 `.gitignore` 要点：

```text
node_modules/
dist/
build/
.env
.env.*
.tools/
.venv/
.cache/
.config/
.tmp/
.nuget/
.dotnet/
.python-userbase/
.ms-playwright/
runtime/
storage/
uploads/
outputs/
logs/
*.mp4
*.mov
*.wav
*.mp3
*.srt
*.zip
*.db
*.sqlite
*.sqlite3
```

验收命令：

```powershell
git status -sb
Get-ChildItem -Force
Get-Content .\README.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_BUILD_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_PHASE_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_TASK_RECORD.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_HANDOFF.md -Encoding UTF8
Get-Content .\docs\REFERENCE_PROJECTS.md -Encoding UTF8
Get-Content .\docs\PRODUCT_SPEC.md -Encoding UTF8
```

不得做：

- 不搭后端。
- 不接模型。
- 不复制参考项目源码。
- 不加入 Docker 作为生产主线。

阶段结束：

- 如果 Stage 0 单独完成，标记 `needs_self_check`。
- 联网自检后更新本文件状态。
- 自检摘要写入 `docs\MILUSTUDIO_TASK_RECORD.md`。
- 下一棒简短状态写入 `docs\MILUSTUDIO_HANDOFF.md`。

## 8. Stage 1 前端 UI 壳

Status: done

目标：

- 建立一个可运行、可演示的 UI 骨架。
- 用 mock 数据体现 MiLuStudio 产品形态。
- 让后续后端和 Worker 能自然接入。

技术：

- Vite
- React
- TypeScript
- CSS Modules 或普通 CSS
- lucide-react 图标

具体任务：

1. 创建 `apps\web`。
2. 初始化 Vite React TypeScript。
3. 添加基础脚本：`dev`、`build`、`preview`。
4. 创建 `src\app` 或 `src\features` 结构。
5. 创建项目列表页。
6. 创建项目详情页。
7. 创建对话输入组件。
8. 创建模式切换组件：极速模式 / 导演模式。
9. 创建任务进度组件。
10. 创建阶段时间线 mock。
11. 创建结果卡片组件。
12. 创建脚本卡 mock。
13. 创建角色卡 mock。
14. 创建分镜卡 mock。
15. 创建图片/视频结果卡 mock。
16. 创建最终交付区 mock。
17. 创建 mock 数据文件。
18. 保持 UI 简单、稳定、工作台感，不做营销页。
19. 按路由或功能域组织前端目录，避免把同一功能拆散到零散文件夹。
20. 如果当前会话可用，调用 `frontend-skill`、`playwright-interactive`、`impeccable`、`taste-skill` / `gpt-taste` 做前端视觉和交互自检。

推荐前端目录：

```text
apps\web\
  package.json
  index.html
  src\
    main.tsx
    app\
      App.tsx
      routes.ts
    features\
      projects\
      production-console\
      settings\
    shared\
      components\
      mock\
      types\
```

UI 骨架要求：

- 首页直接是项目列表，不做 landing page。
- 项目页优先三栏布局。
- 左栏：对话输入和参数。
- 中栏：任务进度。
- 右栏：中间结果和最终交付。
- 移动端可降级为上下布局。
- 不做复杂画布。
- 不做节点拖拽。

验收命令：

```powershell
cd D:\code\MiLuStudio\apps\web
npm install
npm run build
npm run dev
```

视觉验收：

- 页面不空白。
- 文案不溢出。
- 按钮、卡片、进度区布局稳定。
- 不出现仅营销介绍的首页。
- 第一屏能看出这是 AI 漫剧生成工具。
- 如果前端优化 Skills 可用，记录调用和结论；如果不可用，记录不可见原因和本地替代验证。
- 当前视觉方向已按 XiaoLouAI 的浅色、蓝紫、简洁工作台气质优化，不复制其源码或视觉资产。
- 项目 logo 已内置为 `apps\web\public\brand\logo.png`，Web 中统一引用 `/brand/logo.png`。

阶段结束：

- 完成本地 build。
- 如果启动 dev server，记录 URL。
- 联网搜索自检前端形态是否偏离“简单、稳定、Windows 原生、一键漫剧生产”。
- 如发现偏差，先更新 `docs\MILUSTUDIO_BUILD_PLAN.md` 和本文件。
- 自检摘要写入 `docs\MILUSTUDIO_TASK_RECORD.md`。
- 短记录写入 `docs\MILUSTUDIO_HANDOFF.md`。

## 9. Stage 2 数据模型和 Control API

Status: done

目标：

- 建立后端控制面。
- 能创建项目、启动 production job、查询状态、推送进度。

技术：

- `.NET 8`
- ASP.NET Core
- PostgreSQL
- EF Core 优先
- Dapper 可用于关键队列查询

具体任务：

1. 创建 `backend\control-plane`。
2. 创建 solution。
3. 创建 `MiLuStudio.Api`。
4. 创建 `MiLuStudio.Application`。
5. 创建 `MiLuStudio.Domain`。
6. 创建 `MiLuStudio.Infrastructure`。
7. 创建 `MiLuStudio.Worker`。
8. 建立 PostgreSQL 连接配置。
9. 建立 migrations。
10. 建立 `projects` 表。
11. 建立 `story_inputs` 表。
12. 建立 `characters` 表。
13. 建立 `shots` 表。
14. 建立 `assets` 表。
15. 建立 `production_jobs` 表。
16. 建立 `generation_tasks` 表。
17. 建立 `cost_ledger` 表。
18. 实现项目 API。
19. 实现 production job API。
20. 实现 SSE mock。

验收：

- 可创建项目。
- 可启动 mock 生产任务。
- 前端可看到真实 jobId。
- SSE 能推送阶段变化。

阶段结束自检重点：

- 检查当前 Windows 原生 .NET + PostgreSQL + Worker 方案是否仍稳妥。
- 检查是否误引入 Linux / Docker / Redis / Celery 作为生产必需。

落地状态：

- 已创建 `backend\control-plane\MiLuStudio.ControlPlane.sln`。
- 已创建 `MiLuStudio.Api`、`MiLuStudio.Application`、`MiLuStudio.Domain`、`MiLuStudio.Infrastructure`、`MiLuStudio.Worker`。
- 已建立项目、story input、角色、镜头、资产、production job、generation task、cost ledger 的领域实体。
- 已添加 `backend\control-plane\db\migrations\001_initial_control_plane.sql` 作为 PostgreSQL 初始 schema。
- API 当前使用 in-memory repository 跑通 Stage 2，不要求本机必须已有 PostgreSQL。
- 已实现项目 API、production job API、pause、resume、retry 和 SSE mock。
- 前端已通过 `apps\web\src\shared\api\controlPlaneClient.ts` 接入 Control API DTO 和 SSE。
- 当前 dev URL：`http://127.0.0.1:5173/project/demo-episode-01`。
- 当时 API URL：`http://127.0.0.1:5268/health`；Stage 14 起当前默认 API URL 已统一为 `http://127.0.0.1:5368/health`。

临时债：

- 真实 PostgreSQL adapter / EF Core DbContext 尚未接入。
- 当前 in-memory repository 会在 API 进程重启后丢失新建项目和 job。
- Worker 当前只做 heartbeat，占位真实队列领取；Stage 3 需要接入强状态机和任务领取边界。

## 10. Stage 3 任务状态机

Status: done

目标：

- 让生产过程可控、可暂停、可恢复、可重试。

具体任务：

1. 定义 `ProductionStage`。
2. 定义合法状态迁移表。
3. 实现 `ProductionJobService`。
4. 实现 `TaskQueueService`。
5. Worker 保留 heartbeat 和任务领取边界，不在本阶段接真实 PostgreSQL adapter。
6. `FOR UPDATE SKIP LOCKED` 领取策略保留到后续持久化 / Worker 收敛阶段。
7. 实现 pause。
8. 实现 resume。
9. 实现 retry。
10. 实现 checkpoint。
11. 记录失败原因和 attempt_count。
12. 前端显示阶段状态。

验收：

- 任意阶段失败不会丢状态。
- API 进程存活时，刷新前端后能通过 Control API DTO 恢复进度。
- 用户确认节点能暂停。
- 重试只重跑失败任务，不重跑全流程。
- API 运行期仍是 in-memory repository；API 重启后的 durable recovery 留给 PostgreSQL adapter / EF Core DbContext 阶段。

阶段结束自检重点：

- 对照 ArcReel / OpenMontage 的任务队列、SSE、checkpoint 思路。
- 只参考思想，不复制 AGPL 代码。

## 11. Stage 4 Python Skills Runtime

Status: done

目标：

- 跑通第一个内部 Production Skill。

具体任务：

1. 创建 `backend\sidecars\python-skills`。
2. 创建目标为 Python `>=3.11,<3.14` 的项目；本地使用 `D:\soft\program\Python\Python313\python.exe` 验证。
3. 创建 `milu_studio_skills` 包。
4. 定义统一 CLI。
5. 创建 `skills\story_intake`。
6. 添加 `skill.yaml`。
7. 添加 `prompt.md`。
8. 添加 `schema.input.json`。
9. 添加 `schema.output.json`。
10. 添加 `executor.py`。
11. 添加 `validators.py`。
12. 添加 examples。
13. 添加单元测试。
14. 为后续 .NET Worker 调用 Python skill 留出单一 CLI / gateway 边界。

验收：

- 输入 JSON。
- 输出 JSON。
- 输出 envelope 结构稳定，后续 Worker / 数据库 adapter 可写回；本阶段不直接接真实数据库。
- 失败有错误码和错误消息。

阶段结束自检重点：

- 检查 Production Skills 是否仍保持内部能力包定位。
- 不开放为公共插件市场。

## 12. Stage 5 故事解析到脚本

Status: done

目标：

- 从故事输入生成可审阅短视频脚本。

具体任务：

1. 实现 `story_intake`。
2. 实现 `plot_adaptation`。
3. 实现 `episode_writer`。
4. 基于 `story_intake` 的稳定 envelope 串联 `plot_adaptation` 和 `episode_writer`。
5. 输出可审阅 `plot_beats`、`segments`、`subtitle_cues` 和脚本 review checkpoint。
6. 保存脚本到数据库留给 PostgreSQL adapter / EF Core DbContext 阶段。
7. 前端脚本卡、脚本编辑和用户确认 API 留给后续 Control API / UI 收敛阶段。

验收：

- 500 到 2000 字输入可生成 30 到 60 秒脚本。
- 脚本含旁白、对白、节奏说明。
- 输出适合字幕和配音。
- Python Sidecar 只通过统一 CLI / gateway 产出 JSON envelope。
- 不接真实模型 provider，不让 UI 直接调用 Python。

落地状态：

- 已在 `backend\sidecars\python-skills` 新增 `plot_adaptation`。
- 已在 `backend\sidecars\python-skills` 新增 `episode_writer`。
- 已新增 `milu_studio_skills.contracts.unwrap_skill_data`，只负责从上游 skill envelope 读取 `data`。
- `SkillGateway.default()` 已注册 `story_intake`、`plot_adaptation` 和 `episode_writer`。
- 已添加两个 skill 的 `skill.yaml`、`prompt.md`、input/output schema、executor、validators 和 examples。
- 已添加 `tests\test_stage5_script_pipeline.py`，覆盖 `story_intake -> plot_adaptation -> episode_writer` 链路。
- `episode_writer` 输出 `script_text`、`segments`、`subtitle_cues`、`voice_notes`、`review` 和 `checkpoint.required=true`。
- 当前不写数据库，不接真实模型，不改 UI，不触发 FFmpeg 或配音生成。

阶段结束自检重点：

- 对照 AIComicBuilder 和 LumenX 的剧本导入、实体提取流程。

## 13. Stage 6 角色和风格

Status: done

目标：

- 在 Python Skills Runtime 内生成稳定角色设定和统一画风结构。

具体任务：

1. 已实现 `character_bible`，输入为 `episode_writer` 成功 envelope。
2. 已实现 `style_bible`，输入为 `episode_writer` 和 `character_bible` 成功 envelope。
3. 已注册 `SkillGateway.default()`。
4. 已补齐 `skill.yaml`、`prompt.md`、input/output schema、examples 和 tests。
5. 已验证 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible` 链路。

延后任务：

- 前端角色卡和风格卡展示。
- 角色锁定、重新生成和参考图上传。
- 数据库保存、用户编辑持久化和下游重算。
- 参考图、三视图、头像特写和图生视频参考素材生成。

验收：

- 主角描述能跨镜头复用。
- 风格提示词能被图片和视频阶段复用。
- 下游 Stage 7 可通过 envelope 读取角色和画风结构。
- Stage 6 不绕过 Control API / Worker 边界，不接真实模型，不写数据库，不触发 FFmpeg。

阶段结束自检重点：

- 对照 AIComicBuilder 角色四视图和 LumenX Art Direction。
- 已联网对照 LumenX、Codeywood 和 OpenAI Structured Outputs；确认角色/风格应先作为可审核结构和质量门禁产出，参考图和真实模型生成应留给后续资产阶段。

## 14. Stage 7 分镜

Status: done

目标：

- 在 Python Skills Runtime 内生成可审阅、可执行镜头列表结构。

具体任务：

1. 已实现 `storyboard_director`，输入为 `episode_writer`、`character_bible` 和 `style_bible` 成功 envelope。
2. 每个镜头包含时长、场景、角色、景别、运镜、构图、灯光、对白、旁白和声音提示。
3. 每个镜头包含角色 / 风格连续性说明、`image_prompt_seed` 和 `video_prompt_seed`。
4. 已对镜头数量和总时长做校验。
5. 已注册 `SkillGateway.default()`。
6. 已补齐 `skill.yaml`、`prompt.md`、input/output schema、examples 和 tests。
7. 已验证 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible -> storyboard_director` 链路。

延后任务：

- 前端分镜表展示。
- 增删改分镜、单镜头重新生成和用户编辑持久化。
- 数据库保存和下游重算。
- 分镜图、首帧、尾帧、视频片段和资产选择。

验收：

- 30 到 60 秒项目默认 6 到 12 个镜头。
- 镜头时长总和接近目标时长。
- 每个镜头具备后续图片和视频阶段可消费的结构化 prompt seed。
- Stage 7 不绕过 Control API / Worker 边界，不接真实模型，不写数据库，不触发 FFmpeg。

阶段结束自检重点：

- 对照 AIComicBuilder shots、LumenX StoryBoard、OpenMontage pipeline_defs。
- 已联网对照 LumenX、Codeywood 和 OpenMontage；确认分镜应先形成结构化 shot list / storyboard gate，再进入分镜图、首尾帧、视频和编辑阶段。

## 15. Stage 8 图片生成

Status: done

目标：

- 在 Python Skills Runtime 内生成后续图像阶段可消费的提示词请求和 mock 占位资产结构。
- 本阶段不接真实图片 provider，不写图片文件，不写数据库，不做前端选图，不做单镜头真实重试。

具体任务：

1. 已实现 `image_prompt_builder`，输入为 `storyboard_director`、`character_bible` 和 `style_bible` 成功 envelope。
2. 已实现 mock `image_generation`，输入为 `image_prompt_builder` 成功 envelope。
3. 已注册 `SkillGateway.default()`。
4. 已补齐 `skill.yaml`、`prompt.md`、input/output schema、examples 和 tests。
5. 已验证 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible -> storyboard_director -> image_prompt_builder -> image_generation` 链路。
6. mock 资产只生成逻辑 `milu://mock-assets/...` URI 和 `storage_intent`；`file_written=false`、`writes_files=false`、`writes_database=false`。

延后任务：

- `ImageProvider` interface 和真实 provider adapter。
- OpenAI / Gemini / Kling / 国产图片模型接入。
- 图片资产文件写入、上传、缓存和持久化。
- `assets` / `shot_assets` 写入和被选中版本追踪。
- 前端选图、单镜头重试和图片预览。

验收：

- 每个分镜至少生成 `storyboard_image`、`first_frame` 和 `last_frame` 三类请求。
- 至少生成角色参考请求，供后续真实角色图资产阶段消费。
- mock `image_generation` 资产数量与 prompt 请求数量一致。
- Stage 8 不绕过 Control API / Worker 边界，不接真实模型，不写数据库，不触发 FFmpeg。

阶段结束自检重点：

- 检查主流 image-to-video 工作流对首帧、尾帧、参考图的一致性要求。
- 已联网对照 LumenX、OpenAI Images API 和首帧/尾帧工作流；确认本阶段应先形成图像 prompt request 与 mock asset review gate，真实图片 provider、文件写入、资产选择和重试留给后续 adapter / UI 阶段。

## 16. Stage 9 视频生成边界

Status: done

目标：

- 在 Python Skills Runtime 内生成后续视频阶段可消费的视频提示词请求和 mock 占位视频片段结构。
- 本阶段不接真实视频 provider，不写 MP4 文件，不写数据库，不调用 FFmpeg，不做前端片段预览，不做单镜头真实重试。

具体任务：

1. 已实现 `video_prompt_builder`，输入为 `storyboard_director`、`image_prompt_builder` 和 `image_generation` 成功 envelope。
2. 已实现 mock `video_generation`，输入为 `video_prompt_builder` 成功 envelope。
3. 已在 `SkillGateway.default()` 注册 `video_prompt_builder` 和 `video_generation`。
4. 已补齐 `skill.yaml`、`prompt.md`、input/output schema、executor、validators、examples 和 CLI output。
5. 已新增 `tests\test_stage9_video_pipeline.py`，覆盖 Stage 4-9 完整 envelope 链路和失败 envelope。
6. `video_prompt_builder` 每个镜头输出一个 `video_request`，引用 `first_frame`、`last_frame`、分镜图和角色参考 mock 资产。
7. mock `video_generation` 每个镜头输出一个占位视频片段结构，只生成逻辑 `milu://mock-assets/...` URI 和 `storage_intent`。

延后任务：

- `VideoProvider` interface 和真实视频模型 adapter。
- 真实成本记录、失败单镜头重试和 provider 任务轮询。
- 视频文件写入、`assets` / `shot_assets` 持久化和 PostgreSQL / EF Core DbContext 写回。
- 前端视频片段预览、选择和重试。

验收：

- 每个镜头有独立 `video_request` 和 mock `video_clip` 结构。
- mock `video_generation` 片段数量与分镜镜头数量一致。
- 图生视频请求可消费 Stage 8 的 `first_frame` 和 `last_frame` mock 图片资产。
- 输出明确 `file_written=false`、`writes_files=false`、`writes_database=false`、`uses_ffmpeg=false`。
- Stage 9 不绕过 Control API / Worker 边界，不让 UI 直接访问数据库、文件系统、Python 脚本或 FFmpeg。

阶段结束自检重点：

- 对照 ArcReel / LumenX / LocalMiniDrama 的视频生成链路。
- 已联网对照 LumenX、OpenAI Videos API 和 Kling 首尾帧图生视频链路；确认本阶段先收敛视频 prompt / mock clip boundary，真实 provider、MP4、FFmpeg、持久化和 UI 预览延后。

## 17. Stage 10 音频、字幕、剪辑

Status: done

目标：

- 在 Python Skills Runtime 内生成后续音频、字幕和剪辑阶段可消费的配音任务、SRT-ready 字幕结构和粗剪计划结构。
- 本阶段不接真实 TTS / BGM / SFX provider，不写 WAV / SRT / MP4 文件，不写数据库，不调用 FFmpeg，不做前端预览或下载。

具体任务：

1. 已实现 `voice_casting`，输入为 `episode_writer` 和 `storyboard_director` 成功 envelope。
2. 已实现 `subtitle_generator`，输入为 `episode_writer`、`storyboard_director` 和 `voice_casting` 成功 envelope。
3. 已实现 `auto_editor`，输入为 `storyboard_director`、`video_generation`、`voice_casting` 和 `subtitle_generator` 成功 envelope。
4. 已在 `SkillGateway.default()` 注册 `voice_casting`、`subtitle_generator` 和 `auto_editor`。
5. 已补齐三个 skill 的 `skill.yaml`、`prompt.md`、input/output schema、executor、validators、examples 和 CLI output。
6. 已新增 `tests\test_stage10_audio_subtitle_edit_pipeline.py`，覆盖 Stage 4-10 完整 envelope 链路和失败 envelope。

落地状态：

- `voice_casting` 输出 `voice_profiles`、`voice_tasks`、逻辑音频 asset intent、零成本估算和 `checkpoint.required=true`。
- `subtitle_generator` 输出 `subtitle_cues`、SRT 文本结构、逻辑字幕 asset intent 和 review warnings；只产出文本结构，不写真实 SRT 文件。
- `auto_editor` 输出 video / audio / subtitle timeline tracks、rough edit `render_plan`、逻辑 MP4 output intent 和 review warnings。
- 输出明确 `provider=none`、`model=none`、`file_written=false`、`writes_files=false`、`writes_database=false`、`uses_ffmpeg=false`。

验收：

- 配音任务可消费脚本段落、speaker 信息和分镜 timing。
- 字幕 cue 与配音任务数量和时间轴一致，并可序列化为 SRT 格式文本。
- 粗剪计划可消费 mock 视频片段、配音任务和字幕 cue，形成后续 FFmpeg / export adapter 可读的 timeline。
- Stage 10 不绕过 Control API / Worker 边界，不让 UI 直接访问数据库、文件系统、Python 脚本或 FFmpeg。

阶段结束自检重点：

- 对照 OpenMontage FFmpeg、subtitle-sync、video-stitching。
- 已联网对照 OpenAI Audio / Text-to-speech、FFmpeg、Auto-Editor v3 timeline 和 SRT cue 格式；确认本阶段先收敛配音任务、字幕 cue 和粗剪 timeline boundary，真实 TTS、BGM/SFX、字幕文件、FFmpeg、MP4、持久化和 UI 下载延后。

## 18. Stage 11 质量检查

Status: done

目标：

- 降低一键生成失败率，先把质量检查收敛为内部 Production Skill 边界。

具体任务：

1. 实现 `quality_checker`。
2. 检查角色引用是否存在于 `character_bible`。
3. 检查分镜 style prompt block 是否来自 `style_bible`。
4. 检查分镜时长、mock 视频片段时长和粗剪 timeline 时长是否一致。
5. 检查配音任务、字幕 cue timing、字幕长度和镜头引用。
6. 检查 mock 资产和计划结构是否保持 `file_written=false`、`writes_files=false`、`writes_database=false`、`uses_ffmpeg=false`、`reads_media_files=false`。
7. 输出可读质量报告、严重级别、可自动重试项和人工确认 checkpoint。
8. 真实黑屏、卡顿、水印、尺寸、音量和字幕烧录检测留给后续真实媒体 QA adapter；本阶段不读取真实媒体文件。

验收：

- 用户能看到失败原因。
- 可重试项能指向失败镜头或失败结构，供后续 Worker 从对应 skill 继续。
- 质检报告结构能被后续 PostgreSQL adapter / EF Core DbContext 保存到项目资产。
- Stage 11 不接真实视觉 / 音频检测模型，不触发 FFmpeg，不生成真实 MP4，不写数据库。

阶段结束自检重点：

- 检查同类产品的一键生成失败点，确认 Stage 11 只做结构和边界质检是否仍符合当前阶段。

落地状态：

- 已新增 `backend\sidecars\python-skills\skills\quality_checker`。
- 已通过 `SkillGateway.default()` 注册 `quality_checker`。
- 已新增 `tests\test_stage11_quality_checker_pipeline.py`。
- 已覆盖 Stage 5-11 完整 envelope 链路、字幕过长可重试报告和失败 envelope。
- 已生成 `quality_checker` example input / output。

## 19. Stage 12 PostgreSQL 持久化与后端收敛

Status: done

目标：

- 先把数据库、持久化、迁移和 Worker durable claiming 作为后端能力做好，不和桌面端绑定。
- 本阶段不做 Electron，不做安装器，不做桌面端进程管理。
- 桌面端后续只调用 Control API health / preflight 和业务 API。

具体任务：

1. 在 `MiLuStudio.Infrastructure` 中新增 PostgreSQL / EF Core DbContext adapter。
2. 引入必要的 EF Core / Npgsql 依赖，并继续约束 NuGet 缓存到 D 盘项目目录。
3. 将现有 SQL migration 与 EF Core model 对齐，必要时建立 migration runner 或明确的 migration 命令。
4. 实现项目、生产任务、生成任务、资产和成本记录的 PostgreSQL repositories。
5. 通过配置切换 `RepositoryProvider=InMemory` / `RepositoryProvider=PostgreSQL`。
6. 实现 API 启动 preflight，检查连接串、数据库可达性、migration 状态和 storage 路径。
7. 实现 Worker durable claiming，优先使用 PostgreSQL `FOR UPDATE SKIP LOCKED`。
8. 将 Stage 5-11 的 Production Skill envelope 输出通过 Control API / Worker 写入数据库。
9. 补齐数据库集成测试，覆盖 API / Worker 重启后的状态恢复。
10. 明确本地数据库安装方式和连接配置文档；不要把 PostgreSQL 安装、端口、账号或 migrations 藏进 Electron 安装器。

验收：

- API 和 Worker 能在 PostgreSQL provider 下共享同一份项目、任务、资产和成本状态。
- API 重启后仍能恢复项目进度、checkpoint、失败原因和已完成 skill 输出。
- Worker 重启后能继续领取未完成任务，不重复执行已完成任务。
- InMemory provider 仍可用于开发 smoke，不影响 PostgreSQL provider。
- 所有数据库连接、migration、日志和 storage 配置都属于后端配置，不由 Electron 直接管理。
- UI 只通过 Control API 展示数据库状态和错误；不直接访问数据库。

阶段结束自检重点：

- 检查 PostgreSQL Windows 本地部署、EF Core migrations、durable queue 和本地数据目录最新注意事项。
- 确认桌面端不承担数据库 schema、migration 或初始化职责。

落地状态：

- 已在 `MiLuStudio.Infrastructure` 中新增 `MiLuStudioDbContext`，使用 Npgsql / EF Core 映射现有 PostgreSQL schema。
- 已新增 `PostgreSqlControlPlaneRepository`，覆盖项目、生产任务、生成任务、资产和成本记录 repository 边界。
- 已保留 `InMemoryControlPlaneStore`，通过 `ControlPlane:RepositoryProvider=InMemory` / `PostgreSQL` 切换。
- 已新增后端 preflight：检查 provider、connection string、数据库可达性、migration 状态和 storage 根目录。
- 已新增 migration status / apply API，并以 SQL 文件方式维护 `001_initial_control_plane.sql` 和 `002_stage12_postgresql_claiming.sql`。
- 已为 `generation_tasks` 增加 `queue_index`、`locked_by`、`locked_until` 和 `last_heartbeat_at`。
- Worker 已通过 repository 领取任务；PostgreSQL provider 使用 `FOR UPDATE SKIP LOCKED`，并可接管 lease 过期的 running task。
- 已新增 `POST /api/generation-tasks/{taskId}/output`，用于将 skill envelope 写入 `generation_tasks.output_json`，并建立 `assets` 和可选 `cost_ledger` 记录；Stage 13 已把 Stage 5-13 端到端链路接入 Worker 写回。
- 已新增 `GET /api/projects/{projectId}/assets` 和 `GET /api/projects/{projectId}/cost-ledger`。
- 已新增 `docs\POSTGRESQL_STAGE12_SETUP.md`，明确本地 PostgreSQL 配置、preflight、migration、Worker claiming 和 Electron 禁止边界。
- 本机验证已覆盖 .NET build、Python Skills tests、InMemory Control API smoke 和 PostgreSQL provider 未就绪时的 preflight 503。
- Stage 13 已基于本机 `postgresql-x64-18` 服务创建 `milu` 数据库，使用 `root/root` 作为业务连接账号，并完成真实 PostgreSQL migration / API / Worker 共享状态 smoke。

## 20. Stage 13 真实配置、Worker-Skills 与前后端收敛验收

Status: done

目标：

- 把 Stage 12 只落到后端边界的 PostgreSQL 能力切到真实本机配置。
- 本机复用已存在的 PostgreSQL 18 Windows 服务，使用 `root/root`，创建 MiLuStudio 专用业务库 `milu`。
- 默认 `RepositoryProvider` 切到 `PostgreSQL`，后续尽量能持久化的数据都写入 PostgreSQL，InMemory 只作为快速 smoke / 特殊轻量场景保留。
- Worker 真正调用 `backend\sidecars\python-skills` 中的 deterministic Production Skills。
- 通过 Control API / Worker / repository 边界，把 Stage 5-13 deterministic skill envelope 写回数据库。
- 前端通过 Control API 展示真实项目、任务、skill envelope、资产索引和成本记录，减少静态 mock 对主流程的误导。
- 桌面打包继续后移为 Stage 15，数据库不和桌面端绑定；Stage 14 先作为 Stage 13 补丁与打包前收敛阶段。

具体任务：

1. 更新 API / Worker 默认配置：`ControlPlane:RepositoryProvider=PostgreSQL`。
2. 将版本库中的开发连接串统一为 `Host=127.0.0.1;Port=5432;Database=milu;Username=root;Password=root`。
3. 新增或更新 Windows PowerShell 初始化脚本，幂等创建数据库 `milu`，只使用本机 PostgreSQL 18 服务，不修改 `xiaolou` 数据库。
4. 通过 Control API `/api/system/migrations/apply` 或同等后端 migration service 运行 SQL migrations。
5. 保留 InMemory provider，但不再作为默认开发事实来源。
6. 新增 Worker 内部 skill runner adapter，通过 Python CLI / `SkillGateway` 调用 deterministic skills。
7. Worker 构建每个 skill 的稳定输入：首个任务来自 `story_inputs`，后续任务来自前置 task 的成功 envelope。
8. Worker 将 skill output 交给后端 persistence service / repository 写入 `generation_tasks.output_json`、`assets` 和可选 `cost_ledger`。
9. skill envelope `ok=false` 时，任务必须落为 failed，错误信息写入 task / job，不得假装 completed。
10. 收敛 production job 状态推进：由 Worker 和数据库状态驱动，API SSE 只读推送当前状态，不再用 mock 自动推进。
11. 审核节点仍走 checkpoint：前端或 smoke 脚本通过 Control API 提交确认，不能在 Worker 中静默跳过。
12. 如果队列仍包含 `export_packager`，补齐 deterministic mock `export_packager` Production Skill，输出后续导出阶段可消费的占位导出包结构，不生成真实 ZIP / MP4。
13. 增加 Control API 查询能力，让前端能按 job / project 获取 task outputs、assets 和成本记录。
14. 前端生产控制台优先展示真实 API 返回的任务结果、质量报告、mock 资产索引和导出占位结构；demo 静态 mock 只作为 API 不可用时的降级。
15. 补齐本地集成 smoke：创建项目、启动 job、API / Worker 共享 PostgreSQL 状态、checkpoint 推进、重启后恢复。

验收：

- `milu` 数据库存在，`root/root` 可连接，migration 状态为已应用。
- API 和 Worker 默认使用 PostgreSQL provider，并能共享同一份项目、任务、资产和成本状态。
- Worker 能领取 waiting task，调用 Python deterministic skill，并写回 envelope。
- API / Worker 重启后，项目、任务、checkpoint、失败原因和已完成 skill 输出仍可恢复。
- 前端不直接访问数据库、文件系统、Python 脚本或 FFmpeg，只通过 Control API 展示真实结果。
- Stage 13 不接真实模型、不读取真实媒体文件、不调用 FFmpeg、不生成真实 MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。
- 桌面打包、安装器激活码和账号系统不在本阶段实现。

阶段结束自检重点：

- 检查 PostgreSQL 18 Windows 本地服务、EF Core / Npgsql、durable claiming、Worker subprocess、Control API SSE / polling 和前端数据展示是否仍符合 Windows-native 目标。
- 检查是否出现 UI 绕过 Control API、Electron 提前接数据库、Worker 绕过 repository、Python 直接写数据库或 FFmpeg 被提前调用的偏差。

落地状态：

- 已将 API / Worker 默认 `RepositoryProvider` 切到 `PostgreSQL`，连接串写入版本库配置：`Host=127.0.0.1;Port=5432;Database=milu;Username=root;Password=root`。
- 已新增 `scripts\windows\Initialize-MiLuStudioPostgreSql.ps1`，幂等创建 `milu`；当业务账号 `root/root` 无建库权限时，脚本使用本机 `postgres/root` 作为 bootstrap 创建 `milu` 并把 owner 设为 `root`。
- 已通过 Control API `/api/system/migrations/apply` 应用 `001_initial_control_plane` 和 `002_stage12_postgresql_claiming`。
- 已新增 `IProductionSkillRunner` 和 `PythonProductionSkillRunner`，Worker 通过 Python CLI / `SkillGateway` 调用 deterministic skills，不直接调用单个 Python 文件。
- 已将 `plot_adaptation`、`image_prompt_builder`、`video_prompt_builder` 和 `export_packager` 纳入 production task queue，Stage 5-13 链路可完整写回 PostgreSQL。
- 已将 `SystemClock` 改为 UTC，满足 Npgsql 对 `timestamptz` / `DateTimeOffset` 的写入要求；展示层仍转为本地时间。
- 已收敛 PostgreSQL repository 写入顺序和 EF Core ChangeTracker 清理，避免外键顺序和长链路实体跟踪冲突。
- 已将 Worker 轮询间隔收敛为 3 秒，checkpoint 后能更快继续领取下一条 waiting task。
- 已新增 deterministic `export_packager` skill，只输出 MP4 / SRT / JSON / ZIP 的占位交付结构，不生成真实文件。
- 已让 API SSE 只基于数据库快照推送，不再由 API mock 自动推进生产状态。
- 已让前端通过 Control API 读取 production job、tasks、assets 和 cost ledger，并用真实 `outputJson` envelope 构建结果卡和导出区。
- 本地 smoke 已完成：`milu` 中 `job_ba4b02d1cd534e948fe0fda74aaead3c` 达到 `completed / 100`，15 个 task 全部 completed，15 个 task 均有 output envelope，cost ledger 15 行。

## 21. Stage 14 打包前补丁与 Stage 13 收敛

Status: done

目标：

- 在进入桌面端之前，先修复 Stage 13 收敛验收后暴露出的产品链路、运行配置、测试和演示可信度问题。
- 继续保持 Web UI、Control API、Worker、PostgreSQL adapter 和 Python Skills 的独立边界。
- 不接 Electron，不创建安装器，不接真实模型，不读取真实媒体文件，不调用 FFmpeg，不生成真实 MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。
- UI 仍只通过 Control API 通信，不直接访问数据库、文件系统、Python 脚本或 FFmpeg。

具体任务：

1. 打通真实用户输入链路：Web UI 的故事文本、标题、生产模式、目标时长、画幅和风格选择必须通过 Control API 保存到项目，再由 Worker 读取最新 story input 进入 `story_intake`。
2. 扩展后端项目更新 DTO / service / repository 能力，支持更新 `story_inputs.original_text`、word count 和项目描述；避免只保存项目元数据而不保存故事正文。
3. 增加输入校验和错误展示：故事文本至少满足当前产品要求的 500 到 2000 字建议范围，目标时长和画幅校验保持 API 与 UI 一致。
4. 统一端口和 API base URL：对齐 `launchSettings.json`、README、`VITE_CONTROL_API_BASE`、开发脚本和后续桌面进程管理策略；避免 `5268` 与 `5368` 双轨并存。
5. 设计 Stage 15 桌面宿主的 API 地址注入方式：Electron 启动或发现 Control API 后，只把 base URL 作为配置传给 Web UI，不让 UI 访问本地系统能力。
6. 收敛 CORS / 本地承载策略：明确 Vite dev、Control API static hosting 或 Electron 本地 HTTP 承载下的允许源；避免桌面随机端口无法访问 API。
7. 检查前端路由在桌面静态 / 本地 HTTP 环境下的可用性，必要时改为 hash route 或由本地 HTTP host 兜底 History API fallback。
8. 清理演示期 UI 残留：移除或改写 `Stage 1 mock` 文案；无实际功能的 `输出目录`、`锁定`、`重生成` 等按钮要禁用、隐藏或接入 Control API 边界。
9. 增加 checkpoint 的基本人工确认语义：至少区分 approve / reject / notes，避免所有 checkpoint 都被前端硬编码为 approve。
10. 防止同一项目重复启动多个 running production job；若已有 running / paused job，应返回现有 job 或明确要求用户先处理旧 job。
11. 加固 PostgreSQL 默认语义：配置节缺失时不应静默回落到 InMemory；如确需 InMemory，必须显式配置并在 preflight / UI 中清楚标识。
12. 修正过期文档和 schema 注释，例如 `001_initial_control_plane.sql` 中仍称运行期使用 in-memory repository 的旧说明。
13. 为 `PythonProductionSkillRunner` 增加 skill run 临时目录清理或保留策略，避免 `.tmp\skill-runs` 长期堆积。
14. 增加 API / Worker / PostgreSQL 自动化集成测试脚本或测试项目，覆盖 migration status、项目创建 / 更新、生产任务执行、checkpoint、API / Worker 重启恢复、lease 过期接管和失败重试。
15. 检查 Python skill schema、validator 和 `skill.yaml` 是否存在漂移；至少补一个只读契约测试或清单检查，确认 runtime registry、skill.yaml 和 schema 文件一致。
16. 保持 README 中文、PowerShell 友好和面试展示导向；阶段完成后同步说明 Stage 14 补丁完成、Stage 15 桌面打包进入下一步。

验收：

- 用户在 Web UI 输入或修改故事后，启动生产任务时数据库中的 `story_inputs.original_text` 与 UI 输入一致。
- Worker 调用 Python deterministic skills 时消费的是最新项目故事和参数，不再只消费固定示例文案。
- 前端、README、launchSettings 和本地验证命令只保留一套明确的 Control API 默认端口；如使用动态端口，必须由配置注入。
- 桌面打包前的 CORS / API base URL / 路由策略有明确实现和验证记录。
- UI 不再显示过期 `Stage 1 mock` 文案；未实现的动作不会表现成可点击成功的真实功能。
- PostgreSQL provider 是默认业务事实来源；InMemory 只在显式配置时启用，且 UI / preflight 可识别。
- `.tmp\skill-runs` 不会无限增长，保留策略可解释、可测试。
- 自动化测试或脚本覆盖 API / Worker / PostgreSQL 的关键恢复路径。
- 本阶段不创建 Electron 安装包；原 Stage 14 桌面打包继续顺延为 Stage 15。

阶段结束自检重点：

- 联网对照 Electron / electron-builder / NSIS、Electron security、Windows taskbar pinning 和同类 AI 视频 / 短剧项目，确认 Stage 15 仍不需要提前引入 Docker / Redis / Celery 或真实 provider。
- 检查根 `README.md` 是否需要同步更新；如需要，继续保持中文、PowerShell 友好、面试展示导向。
- 把本阶段修复内容、验证命令、偏差原因和 Stage 15 下一棒提示词写入 `docs\MILUSTUDIO_TASK_RECORD.md` 和 `docs\MILUSTUDIO_HANDOFF.md`。

落地状态：

- Web UI 保存链路已打通：故事文本、标题、模式、目标时长、画幅和风格会通过 `PATCH /api/projects/{projectId}` 写入 PostgreSQL，启动生产前会先保存草稿。
- Worker / Python deterministic skills 已消费最新 `story_inputs.original_text` 和项目参数，Stage 14 集成脚本会校验 `story_intake` task input 中的更新标记。
- Control API 默认端口统一为 `http://127.0.0.1:5368`；Vite `VITE_CONTROL_API_BASE` 与桌面宿主预留 `window.__MILUSTUDIO_CONTROL_API_BASE__` 注入策略已对齐。
- CORS 已允许 loopback 本地源，Web 路由支持 hash route，便于 Stage 15 静态或本地 HTTP 桌面承载。
- `Stage 1 mock` 文案和无处理器按钮已清理；checkpoint 已区分 approve / reject / notes，notes 会持久化到 `generation_tasks.checkpoint_notes`。
- 同一项目已有 active production job 时会返回现有 job，避免重复创建多个 running / paused job。
- PostgreSQL 是默认 provider；InMemory 只在显式配置 `RepositoryProvider=InMemory` 时启用。
- `.tmp\skill-runs` 已有保留最近 30 次的清理策略，可通过 `ControlPlane:SkillRunRetentionCount` 调整。
- 新增 `scripts\windows\Test-MiLuStudioStage14Integration.ps1`，覆盖 migration、项目创建 / 更新、生产执行、checkpoint、API / Worker 重启恢复、lease 过期接管、reject notes 和 retry。
- 新增 `backend\sidecars\python-skills\tests\test_stage14_skill_contracts.py`，检查 registry、`skill.yaml`、schema、executor 和 validator 契约漂移。

## 22. Stage 15 桌面打包

Status: done

目标：

- 形成可演示、可售卖的 Windows 安装包。
- 唯一桌面打包方案为 `Electron + electron-builder + NSIS assisted installer + 自定义 installer.nsh`。
- 桌面端保留现有 Web UI 和 Control API 作为可持续迭代的前后端，只新增桌面宿主、安装器和本地进程管理边界。
- 桌面端作为独立 part，在 Web UI、Control API、Worker、PostgreSQL adapter 和核心生产功能相对稳定后推进。

具体任务：

1. 创建 `apps\desktop`。
2. Electron 承载 Web UI。
3. 启动本地 Control API。
4. 启动 Windows Worker。
5. 启动 Python Sidecar。
6. 随机端口绑定。
7. 调用 Control API health / preflight 检查数据库、storage、Python runtime 和 Worker 状态；桌面端只展示结果和修复引导。
8. 初始化或检查 storage 目录只能通过 Control API / 后端 service 完成，Electron 不直接写业务目录。
9. 托盘菜单。
10. 打开输出目录。
11. 查看日志。
12. electron-builder + NSIS assisted installer 打包，设置 `oneClick=false`、`allowToChangeInstallationDirectory=true`、`runAfterFinish=true`。
13. 配置应用图标、安装器图标、卸载器图标、快捷方式图标、AppUserModelID 和 `shortcutName`。
14. 增加自定义 `build\installer.nsh`，支持安装前付费码 / 激活码输入页和安装后动作。
15. 安装器提供用户可选项：桌面快捷方式、开始菜单快捷方式、开机自启动、安装完成后启动。
16. 预留登录 / 注册 / 授权入口和前端路由；账号系统不阻塞桌面安装包 MVP，作为下一大阶段推进。
17. 生成 `MiLuStudio-Setup-<version>.exe`。

验收：

- 干净 Windows 环境可安装；桌面包必须尽量随包携带运行所需的 .NET / Python runtime，不能依赖开发机固定路径。
- 用户无需手动启动后端。
- 桌面端只通过 Control API 检查和使用数据库，不直接访问数据库、文件系统业务目录、Python 脚本或 FFmpeg。
- 如果数据库未就绪，桌面端展示 Control API preflight 返回的错误和修复建议，不自行创建或迁移数据库。
- 用户可选择安装目录、桌面快捷方式、开始菜单快捷方式、开机自启动和安装完成后启动。
- 安装过程有可见实时进度条。
- 付费码 / 激活码校验可阻止无效安装继续。
- 安装器激活码门槛不作为唯一授权边界；应用内登录注册和账号授权留给桌面 MVP 后下一大阶段。
- 任务栏固定不做静默强制 pin；通过正确 AppUserModelID / 快捷方式 / 引导让用户自行固定。
- 卸载不默认删除用户生成素材。
- 日志可定位错误。
- 桌面模式下 Control API 写请求有本地会话令牌保护，CORS 不应继续对任意 loopback origin 过宽放行。

落地状态：

- 已完成。新增 `apps\desktop`，Electron 承载 `apps\web` 构建产物，并由本地 HTTP host 提供静态资源、hash route fallback 和 CSP。
- 桌面宿主随机绑定 Web host 与 Control API 端口，启动本地 Control API 和 Windows Worker；Python deterministic skills 作为 Worker sidecar runtime 路径注入后端配置。
- Control API base URL 通过 preload 注入 `window.__MILUSTUDIO_CONTROL_API_BASE__`，桌面会话令牌通过 `window.__MILUSTUDIO_DESKTOP_TOKEN__` 注入并仅用于 Control API 写请求；Web UI 继续只走 Control API DTO / SSE，不碰数据库、文件系统、Python 脚本或 FFmpeg。
- Web UI 已新增桌面诊断面板，展示 Control API health / preflight、PostgreSQL、storage、Python runtime、Python skills root、Worker 和 Web host 状态，并提供重启服务、打开日志、打开输出目录入口。
- `scripts\windows\Prepare-MiLuStudioDesktopRuntime.ps1` 会复制 Web dist、self-contained API / Worker 发布产物、SQL migrations、Python deterministic skills 和 `python-runtime` 到 `apps\desktop\runtime`，并从 `apps\web\public\brand\logo.png` 生成多尺寸 `build\icon.ico`。
- `apps\desktop\package.json` 已配置 electron-builder + NSIS：`oneClick=false`、`allowToChangeInstallationDirectory=true`、`runAfterFinish=true`、`shortcutName=MiLuStudio`、桌面快捷方式、开始菜单快捷方式、AppUserModelID 和自定义 `installer.nsh`。
- Electron 已升级到 `42.0.1`，主进程限制外部导航、弹窗和 IPC 来源；Electron `userData`、`sessionData` 和 logs 已显式指向 D 盘数据目录，避免默认落到 `C:\Users\...\AppData\Roaming`。
- 自定义 `apps\desktop\build\installer.nsh` 已预留安装前激活码输入页，以及桌面快捷方式、开始菜单快捷方式和开机自启动复选项；正式授权仍由后续 Control API / Auth & Licensing adapter 负责。
- 已生成 `D:\code\MiLuStudio\outputs\desktop\MiLuStudio-Setup-0.1.0.exe` 和 `win-unpacked`；打包资源包含 `resources\web`、`resources\control-plane`、`resources\python-skills`、`resources\python-runtime` 和 `resources\build\icon.ico`。
- 桌面端不执行 migrations、不定义数据库表、不初始化数据库；数据库未就绪时只展示 Control API preflight 返回的问题和建议。
- 新增 `scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1`，并接入 `Test-MiLuStudioDesktop.ps1`；验证桌面模式下无令牌写请求为 403、带令牌写请求可进入业务校验、桌面模式 migration apply 为 403。
- 当前本机安装包 Authenticode 状态仍为 `NotSigned`；正式发布前需要补代码签名证书和签名流水线。

阶段结束自检重点：

- 对照 LocalMiniDrama、Toonflow、MiLuAssistantDesktop 经验。
- 检查 Electron / electron-builder / NSIS Windows 打包、自定义安装页、图标、快捷方式、自启动、安装激活码和任务栏固定最新注意事项。

## 23. Stage 16 账号、会话、设备绑定和许可证授权

Status: done

目标：

- 在桌面 MVP 后补齐应用内账号注册、登录、会话、设备绑定和许可证授权系统。
- 正式授权由 Control API / Auth & Licensing adapter 统一处理，不由 Electron 主进程或安装器脚本直接判定。
- Electron 与 Web UI 只通过 Control API 展示登录 / 注册 / 激活入口、授权状态和授权错误。
- 安装器激活码页继续作为安装前门槛占位，不作为唯一授权安全边界。

边界：

- 不接真实模型 provider。
- 不读取真实媒体文件。
- 不触发 FFmpeg。
- 不生成真实 MP4 / WAV / SRT / ZIP。
- 不引入 Linux / Docker / Redis / Celery 作为生产依赖。
- 不让 UI 或 Electron 绕过 Control API / Worker 边界。
- 不让桌面端执行 migrations、定义数据库表或负责数据库初始化。

具体任务：

1. 在 Domain / Application 层定义账号、会话、设备、许可证和授权状态模型。
2. 在 Infrastructure 层补 PostgreSQL repository / adapter，沿用后端 migration 机制，不由 Electron 执行。
3. 增加 Auth & Licensing service 边界，负责注册、登录、退出、刷新会话、设备绑定、许可证校验和授权错误。
4. 增加 Control API 端点：注册、登录、退出、当前账号、授权状态、设备绑定、激活码兑换或校验。
5. 为 Web UI 增加登录 / 注册 / 激活入口；未登录或未授权时只显示认证与授权页面，不进入项目工作台。
6. 桌面宿主继续只注入 Control API base URL 和桌面会话令牌，不保存账号密码，不直接判断许可证。
7. 对现有项目、生产任务和管理操作加最小授权门禁；未授权请求返回清晰错误 DTO。
8. 增加本地开发用 deterministic Auth & Licensing adapter，可使用本地测试账号和测试激活码，不接真实云端授权服务。
9. 增加 PowerShell 集成测试脚本，覆盖注册、登录、会话刷新、设备绑定、许可证状态、未授权阻断和桌面 UI API 边界。
10. 同步 README、总参考、任务记录和短棒交接。

验收：

- 未登录用户打开 Web UI / Electron 时只看到登录、注册或激活入口。
- 登录成功后可进入工作台并继续通过 Control API 使用项目列表、项目详情和生产控制台。
- 未授权或许可证失效时，受保护 API 返回明确授权错误，UI 不表现为静默失败。
- 设备绑定信息由 Control API / Auth & Licensing adapter 管理，Electron 不直接读写数据库。
- 安装器激活码页和应用内授权状态边界清晰，不把安装码当作唯一授权依据。
- 所有新增数据持久化仍通过 PostgreSQL / backend migration / repository 边界完成。
- 自动化脚本覆盖主要认证和授权路径。

落地状态：

- 已新增 `Account`、`AuthSession`、`DeviceBinding`、`LicenseGrant` 领域实体，以及 `AccountStatus`、`LicenseKind`、`LicenseStatus` 枚举。
- 已新增 `IAuthRepository`、`IAuthTokenService`、`IPasswordHasher`、`IAuthLicensingAdapter` 和 `AuthLicensingService`，授权语义集中在 Application / Infrastructure 边界。
- 已新增 `backend\control-plane\db\migrations\004_stage16_auth_licensing.sql`，创建 `accounts`、`auth_sessions`、`devices` 和 `licenses`。
- PostgreSQL provider 已新增 `PostgreSqlAuthRepository`；InMemory provider 已新增 `InMemoryAuthRepository` 作为显式 smoke 备选。
- 已新增 deterministic `DeterministicAuthLicensingAdapter`，默认测试激活码为 `MILU-STAGE16-TEST`，不接真实云端授权服务。
- Control API 已新增注册、登录、刷新、退出、当前账号、许可证状态、激活和设备绑定端点。
- 项目、生产任务和 generation task 类 API 已加授权门禁；SSE 使用同一 access token，通过 query 参数传入，避免 UI 绕过 Control API。
- Web UI 已新增 `features\auth\AuthGate.tsx`，登录 / 注册 / 激活成功后才进入原项目工作台。
- Electron / preload 未保存账号密码、不判断许可证，只继续注入 Control API base URL 和桌面会话令牌。
- 已新增 `scripts\windows\Test-MiLuStudioStage16Auth.ps1`，覆盖注册、登录、会话刷新、设备绑定、设备上限阻断、许可证状态、未登录 401、未授权 403 和登出失效。
- 已更新 `scripts\windows\Test-MiLuStudioStage14Integration.ps1`，先注册并激活测试账号，确保授权门禁不破坏生产链路回归。
- 已更新 `scripts\windows\Test-MiLuStudioDesktopApiSecurity.ps1`，确认桌面令牌只通过桌面安全门，应用授权仍由 Auth & Licensing gate 处理。

验证：

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

阶段结束自检重点：

- 联网对照桌面应用登录、设备绑定、许可证授权和离线激活常见方案，确认不把密钥、私钥或可逆授权算法放入客户端。
- 检查 Electron 安全边界是否仍只暴露受控 IPC 和 Control API 配置。
- 检查 README 是否需要同步更新，保持中文、PowerShell 友好和面试展示导向。

## 24. 每阶段联网自检模板

每个大阶段结束后必须填写到 `docs\MILUSTUDIO_TASK_RECORD.md`。

```text
Date:
Stage:
Local verification:
Design check:
Web searches:
Findings:
Deviation risk:
README check:
Build plan changes:
Phase plan changes:
Task record changes:
Handoff changes:
Next phase:
```

如果发现偏差：

1. 先最小修改 `docs\MILUSTUDIO_BUILD_PLAN.md`。
2. 再修改本文件的阶段任务。
3. 再在 `docs\MILUSTUDIO_TASK_RECORD.md` 记录原因和变更。
4. 检查并按需修改根 `README.md`。
5. 最后修改 `docs\MILUSTUDIO_HANDOFF.md`。

如果没有偏差：

1. 不修改总参考。
2. 只更新本文件阶段状态。
3. 在 `docs\MILUSTUDIO_TASK_RECORD.md` 记录自检摘要。
4. 检查根 `README.md` 是否需要同步更新。
5. 更新 `docs\MILUSTUDIO_HANDOFF.md` 的下一棒任务。
