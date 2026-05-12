# MiLuStudio 短棒交接

更新时间：2026-05-12  
工作目录：`D:\code\MiLuStudio`

本文件只保留下一棒需要立刻接住的上下文。  
长规划看 `docs\MILUSTUDIO_BUILD_PLAN.md`。  
总任务阶段安排看 `docs\MILUSTUDIO_PHASE_PLAN.md`。  
修改记录和自检记录看 `docs\MILUSTUDIO_TASK_RECORD.md`。

## 每棒先读

```powershell
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
Get-Content .\docs\MILUSTUDIO_BUILD_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_PHASE_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_TASK_RECORD.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_HANDOFF.md -Encoding UTF8
```

## 文档格式约束

```text
所有项目文档保持 UTF-8 Markdown。
优先短行、普通标题、普通列表和 text 代码块。
避免宽表格、超长单行、隐藏折叠格式和依赖网页渲染的内容。
除根 README 外，长期文档、阶段计划和专题说明优先放在 docs 目录。
所有文档必须便于 PowerShell Get-Content / Select-String 阅读。
```

## 固定路线

```text
1. MiLuStudio 是新项目，不覆盖旧 MiLuAssistantWeb / MiLuAssistantDesktop。
2. XiaoLouAI / MiLu 自有体系是主干。
3. 产品目标是 Windows 原生 AI 漫剧 Agent。
4. 用户只做关键决策，系统执行技术流程。
5. Skills 是内置 Production Skills，不开放为公共插件市场。
6. AIComicBuilder / LocalMiniDrama / LumenX / ArcReel / Toonflow / Huobao Drama / OpenMontage 只作参考。
7. 不直接复制参考项目源码。
8. 不把 Linux / Docker / 云端 SaaS 作为第一版生产主线。
9. 根目录保持简洁，长期文档归入 docs，前后端和 Production Skills 按路由或功能域聚合。
10. 依赖、配置、缓存、运行数据、日志、上传素材和生成结果必须限制在 D:\code\MiLuStudio 或明确的 D 盘工具目录。
11. Python、Node.js、.NET、Electron、Playwright、模型缓存等外部依赖不得主动写入 C 盘。
```

## 编码设计硬约束

```text
1. 必须遵守高内聚、低耦合、职责单一、关注点分离和依赖倒置。
2. UI 不能直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
3. 前端只通过 Control API 和 DTO 通信。
4. .NET Control API 负责编排、状态、资产索引和 Sidecar 调用。
5. Python Sidecar / Skills Runtime 负责具体生产技能执行。
6. 模型、存储、队列、FFmpeg、Windows 打包等变化点必须隔离在 adapter / service 边界。
7. 禁止循环依赖、跨层反向依赖和共享隐式全局状态。
8. 不为了架构感提前堆大型抽象；原则服务功能落地。
9. 目录优先按路由或功能域聚合，不把同一功能拆散到零散文件夹。
10. 前端实现时如当前会话可用，优先调用 frontend-skill、playwright-interactive、impeccable、taste-skill / gpt-taste 做视觉和交互自检。
11. MVP 临时直连必须限制在单一 adapter / gateway 内，并写入任务记录。
```

## 环境封闭硬约束

```text
1. 所有项目依赖、配置、运行文件、缓存、日志、数据库、上传素材和生成结果优先放在 D:\code\MiLuStudio 内。
2. 如果 Python / Node.js / .NET SDK 等基础运行时必须在项目外安装，只能放在 D:\dev 或 D:\tools。
3. 禁止主动写入 C:\Users、C:\ProgramData、C:\Python、C:\nodejs 等 C 盘位置。
4. npm / pnpm / yarn / pip / NuGet / Electron / Playwright / HuggingFace 等缓存必须通过环境变量指向项目内或 D 盘。
5. 不使用全局 npm install -g 或全局 pip 安装作为项目运行前提。
6. 如果工具无法避免写入 C 盘，先停止并记录原因，等待用户确认。
```

## 当前接棒

```text
Phase: Stage 3 preparation
Status: pending
Owner: next session
Goal: 建立任务状态机、合法迁移、pause/resume/retry/checkpoint，并让 mock SSE 由状态机驱动。
```

Done:

- `docs\MILUSTUDIO_BUILD_PLAN.md` 已作为总参考写入。
- `docs\MILUSTUDIO_PHASE_PLAN.md` 已作为总任务阶段安排写入。
- `docs\MILUSTUDIO_TASK_RECORD.md` 已收窄为修改总记录和阶段自检记录。
- `docs\MILUSTUDIO_HANDOFF.md` 已作为短棒交接写入。
- 四个总控文档已移动到 `docs\`，每棒先读命令已改为 `docs\` 路径。
- Stage 0 已完成：Git、`.gitignore`、`README.md`、`docs\REFERENCE_PROJECTS.md`、`docs\PRODUCT_SPEC.md` 已就位。
- Stage 1 已完成：`apps\web` Vite + React + TypeScript 前端壳已就位。
- 前端已升级到 `vite@^8.0.12` 和 `@vitejs/plugin-react@^6.0.1`。
- 前端视觉已按 XiaoLouAI 的浅色、蓝紫、简洁工作台气质优化，核心样式在 `apps\web\src\styles.css`。
- 项目 logo 已内置为 `apps\web\public\brand\logo.png`。
- 侧栏品牌标识和 Web favicon 已统一引用项目内路径 `/brand/logo.png`。
- 代码、HTML 和运行时配置不得引用 `C:\Users\10045\Downloads\logo.png` 或其他项目外部 logo 路径。
- `npm run build` 已通过。
- dev server 已启动：`http://127.0.0.1:5173/`。
- 项目控制台可访问：`http://127.0.0.1:5173/project/demo-episode-01`。
- Stage 2 已完成：`backend\control-plane` solution 已就位。
- 后端项目已创建：`MiLuStudio.Api`、`MiLuStudio.Application`、`MiLuStudio.Domain`、`MiLuStudio.Infrastructure`、`MiLuStudio.Worker`。
- 已添加 PostgreSQL 初始 migration：`backend\control-plane\db\migrations\001_initial_control_plane.sql`。
- 当前 API 使用 in-memory repository 跑通项目 API、production job API、pause、resume、retry 和 SSE mock。
- Control API 当前可访问：`http://127.0.0.1:5268/health`。
- 前端已接入 Control API DTO 和 SSE，开始生成后能显示真实 `job_...`。
- Stage 2 已通过 `dotnet build backend\control-plane\MiLuStudio.ControlPlane.sln` 和 `npm run build`。
- Stage 2 截图：`.tmp\stage2-project-desktop-v2.png`、`.tmp\stage2-project-mobile-v7.png`。
- 已添加 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束常见缓存到项目内。
- 本地已清理 MiLuAssistantWeb、MiLuAssistantDesktop、QwenPaw。
- 本地保留 XiaoLouAI、AIComicBuilder、LocalMiniDrama、lumenx、ArcReel-main、Toonflow-app、huobao-drama、OpenMontage。

Next:

1. 进入 Stage 3。
2. 定义 production job 的强状态机和合法迁移表。
3. 将 pause、resume、retry、checkpoint 行为改成状态机驱动。
4. 让 mock SSE 基于状态机推进，而不是直接循环 stage catalog。
5. 保持 UI 只通过 Control API DTO 通信。
6. 不接真实模型。
7. 不引入 Docker、Redis、Celery 或 Linux 生产依赖。
8. 真实 PostgreSQL adapter / EF Core DbContext 尚未接入，当前只保留 SQL migration 和 in-memory runtime。
9. Worker 当前只是 heartbeat，占位真实队列领取；Stage 3 可先设计接口，不要过早引入外部队列。
10. 继续使用 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束 D 盘环境。
11. 阶段完成后联网搜索自检并更新 docs。
12. 保留 `/brand/logo.png` 作为 Web 前端统一 logo 路径，后续如需图标变体从项目内品牌资产派生。

## 阶段结束规则

```text
每个大阶段结束后必须联网搜索自检。
如果发现方向偏差，先修改 docs\MILUSTUDIO_BUILD_PLAN.md。
然后修改 docs\MILUSTUDIO_PHASE_PLAN.md。
再把偏差原因和修改摘要写入 docs\MILUSTUDIO_TASK_RECORD.md。
最后只把下一棒必须知道的短记录写入本文件。
```

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0、Stage 1 和 Stage 2 已完成。按 docs\MILUSTUDIO_PHASE_PLAN.md 的 Stage 3 开始实现任务状态机：定义 ProductionStage、合法状态迁移表、ProductionJobService 状态推进、pause、resume、retry、checkpoint，并让当前 mock SSE 由状态机驱动。不要接真实模型，不要引入 Linux/Docker/Redis/Celery 作为生产依赖，不要让 UI 直接访问数据库、文件系统、Python 脚本或 FFmpeg。阶段完成后必须联网搜索自检，如发现偏差，先更新总参考和总任务阶段安排，再把原因写入总任务记录，最后更新 handoff。

编码时必须遵守高内聚、低耦合、职责单一、关注点分离和依赖倒置；不要让 UI 直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg；不要为了架构感提前堆大型抽象。目录保持简洁，前后端和 Production Skills 按路由或功能域聚合。前端实现时如当前会话可用，调用 frontend-skill、playwright-interactive、impeccable、taste-skill / gpt-taste 做视觉和交互自检。MVP 如需临时直连，只能放在单一 adapter / gateway 内，并在任务记录中写清技术债。

所有依赖、配置、缓存、运行数据、日志、上传素材和生成结果必须限制在 D:\code\MiLuStudio 或明确的 D 盘工具目录。Python、Node.js、.NET、Electron、Playwright、模型缓存等不得主动写入 C 盘；如果工具无法避免写入 C 盘，先停止并记录原因，等待用户确认。当前 PostgreSQL 只落了 SQL migration，API 运行期仍是 in-memory repository；真实 PostgreSQL adapter / EF Core DbContext 和 Worker 任务领取边界留给后续阶段收敛。
```
