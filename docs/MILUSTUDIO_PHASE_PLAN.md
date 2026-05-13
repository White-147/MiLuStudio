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
Current phase: Stage 11
Status: pending
Goal: 基于 Stage 10 粗剪计划继续收敛质量检查边界。
Next handoff owner: next session
```

当前阶段完成标准：

- `D:\code\MiLuStudio` 是独立 Git 仓库。
- 根 `.gitignore` 存在。
- 根 `README.md` 存在。
- 总控文档位于 `docs\`。
- `docs\REFERENCE_PROJECTS.md` 存在。
- `docs\PRODUCT_SPEC.md` 存在。
- `apps\web` 可运行。
- 前端 UI 有项目列表、项目页、对话输入、任务进度 mock、结果卡片 mock。
- 不出现复杂无限画布。
- 不接真实模型。
- 代码边界通过软件设计阶段检查。
- 依赖、缓存、配置和运行文件通过 D 盘封闭环境检查。
- 阶段完成后完成联网自检并更新相关文档。

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
- 当前 API URL：`http://127.0.0.1:5268/health`。

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

Status: pending

目标：

- 降低一键生成失败率。

具体任务：

1. 实现 `quality_checker`。
2. 检查角色一致性。
3. 检查分镜时长。
4. 检查字幕长度。
5. 检查视频黑屏、卡顿、水印、尺寸。
6. 检查文件缺失。
7. 输出可读报告。
8. 可自动修复项自动重试。

验收：

- 用户能看到失败原因。
- 可重试项能从失败镜头继续。
- 质检报告保存到项目资产。

阶段结束自检重点：

- 检查同类产品的一键生成失败点，更新质检项。

## 19. Stage 12 PostgreSQL 持久化与后端收敛

Status: pending

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

## 20. Stage 13 桌面打包

Status: pending

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

- 干净 Windows 环境可安装。
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

阶段结束自检重点：

- 对照 LocalMiniDrama、Toonflow、MiLuAssistantDesktop 经验。
- 检查 Electron / electron-builder / NSIS Windows 打包、自定义安装页、图标、快捷方式、自启动、安装激活码和任务栏固定最新注意事项。

## 21. 每阶段联网自检模板

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
