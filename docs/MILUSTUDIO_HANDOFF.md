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
Get-Content .\docs\MILUSTUDIO_BUILD_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_PHASE_PLAN.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_TASK_RECORD.md -Encoding UTF8
Get-Content .\docs\MILUSTUDIO_HANDOFF.md -Encoding UTF8
```

## 当前接棒

```text
Phase: Stage 10 preparation
Status: pending
Owner: next session
Goal: 基于 mock 视频片段继续收敛音频、字幕和粗剪边界。
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

## Stage 9 落地内容

- 新增 `backend\sidecars\python-skills\skills\video_prompt_builder`。
- 新增 `backend\sidecars\python-skills\skills\video_generation`。
- `SkillGateway.default()` 已注册 `video_prompt_builder` 和 `video_generation`。
- `video_prompt_builder` 输入 `storyboard_director`、`image_prompt_builder` 和 `image_generation` envelopes。
- `video_prompt_builder` 每个镜头输出一个结构化 `video_request`，引用 `storyboard_image`、`first_frame`、`last_frame` 和角色参考 mock 资产。
- `video_generation` 输出 deterministic mock 占位视频片段、`clip_manifest`、零成本估算和 `checkpoint.required=true`。
- mock 视频片段只暴露逻辑 `milu://mock-assets/...` URI 和 `storage_intent`；不写文件、不写数据库、不调用真实 provider、不触发 FFmpeg。
- 新增 `backend\sidecars\python-skills\tests\test_stage9_video_pipeline.py`，覆盖完整 Stage 4-9 envelope 链路。

## 验证已通过

```powershell
. .\scripts\windows\Set-MiLuStudioEnv.ps1
& $env:MILUSTUDIO_PYTHON -m compileall backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m unittest discover -s backend\sidecars\python-skills\tests -v
D:\soft\program\dotnet\dotnet.exe build backend\control-plane\MiLuStudio.ControlPlane.sln --no-restore
```

```powershell
. D:\code\MiLuStudio\scripts\windows\Set-MiLuStudioEnv.ps1
Push-Location D:\code\MiLuStudio\apps\web
D:\soft\program\nodejs\npm.ps1 run build
Pop-Location
```

CLI examples:

```powershell
Push-Location backend\sidecars\python-skills
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill video_prompt_builder --input skills\video_prompt_builder\examples\input.json --output skills\video_prompt_builder\examples\output.json --pretty
& $env:MILUSTUDIO_PYTHON -m milu_studio_skills run --skill video_generation --input skills\video_generation\examples\input.json --output skills\video_generation\examples\output.json --pretty
Pop-Location
```

## 固定约束

- 不要接真实模型 provider。
- 不要引入 Linux / Docker / Redis / Celery 作为生产依赖。
- 不要让 UI 直接访问数据库、文件系统、模型 SDK、Python 脚本或 FFmpeg。
- 继续通过 Control API / Worker / Python Skills Runtime 边界推进。
- 真实 PostgreSQL adapter / EF Core DbContext 和 Worker durable claiming 后续再收敛。
- 所有依赖、配置、缓存、运行数据、日志、上传素材和生成结果必须限制在 `D:\code\MiLuStudio` 或明确的 D 盘工具目录。
- 继续使用 `scripts\windows\Set-MiLuStudioEnv.ps1` 约束 D 盘环境。
- 本机 PATH 前面可能有 C 盘 WindowsApps `python.exe`；运行 Python 必须显式使用 `D:\soft\program\Python\Python313\python.exe`、`$env:MILUSTUDIO_PYTHON` 或项目内 D 盘 venv。

## 技术债

- 当前 API 运行期仍是 in-memory repository。
- `storage_intent` 只是 Stage 8 mock asset 的未来 adapter 目标描述，不代表真实文件已生成。
- 真实图片 provider、文件写入、资产持久化、前端选图和重试均未实现。
- `video_generation` 当前只输出 mock clip records，不代表真实 MP4 已生成。
- 真实视频 provider、视频文件写入、资产持久化、前端视频预览、单镜头重试和 FFmpeg 均未实现。
- 本阶段曾误用不存在的 `$env:MILUSTUDIO_DOTNET` / `$env:MILUSTUDIO_NPM` 变量，命令未执行；随后已用 D 盘绝对路径重新验证通过。未安装依赖、未下载缓存、未生成项目外产物。

## 下一步建议

1. 进入 Stage 10 音频、字幕、剪辑边界。
2. 基于 `video_generation` mock clips、`episode_writer` subtitle cues 和 storyboard timing 设计 `voice_casting` / `subtitle_generator` / `auto_editor` 的内部 Production Skill 边界。
3. 先输出可审阅的配音任务、SRT 字幕结构和粗剪计划结构。
4. 不接真实 TTS / BGM / FFmpeg，不写数据库，不生成真实 MP4，不让 UI 绕过 Control API / Worker。
5. 阶段完成后必须联网搜索自检；如发现偏差，先更新 `MILUSTUDIO_BUILD_PLAN.md`，再更新 `MILUSTUDIO_PHASE_PLAN.md`，再把原因写入 `MILUSTUDIO_TASK_RECORD.md`，最后更新本 handoff。

## 下一棒提示词

```text
读取 D:\code\MiLuStudio\docs\MILUSTUDIO_BUILD_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_PHASE_PLAN.md、D:\code\MiLuStudio\docs\MILUSTUDIO_TASK_RECORD.md 和 D:\code\MiLuStudio\docs\MILUSTUDIO_HANDOFF.md。Stage 0、Stage 1、Stage 2、Stage 3、Stage 4、Stage 5、Stage 6、Stage 7、Stage 8 和 Stage 9 已完成。按 docs\MILUSTUDIO_PHASE_PLAN.md 的 Stage 10 开始实现音频、字幕和剪辑边界：基于现有 backend\sidecars\python-skills 的 episode_writer、storyboard_director 和 video_generation envelopes，优先收敛 voice_casting、subtitle_generator 和 auto_editor 的内部 Production Skill 边界，让可审阅脚本、分镜 timing 和 mock 视频片段能生成后续阶段可消费的配音任务、SRT 字幕结构和粗剪计划结构；后续保存到数据库仍留给 PostgreSQL adapter / EF Core DbContext 阶段。不要接真实 TTS/BGM/FFmpeg，不要生成真实 MP4，不要引入 Linux/Docker/Redis/Celery 作为生产依赖，不要让 UI 直接访问数据库、文件系统、Python 脚本或 FFmpeg。Stage 10 不要绕过 Control API / Worker 边界。
```
