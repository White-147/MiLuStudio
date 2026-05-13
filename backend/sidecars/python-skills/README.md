# MiLuStudio Python Skills Runtime

本目录是 MiLuStudio 的内部 Production Skills sidecar。

它只负责通过统一 CLI / `SkillGateway` 接收 JSON 输入并输出 JSON envelope，供后续 .NET Worker 调用。UI 不应直接调用本目录下的 Python 脚本。

## 当前范围

当前已完成到 Stage 13：

- 通过一个 CLI 边界运行内部 Skills。
- 输入和输出都保持为 JSON 文件或 JSON envelope。
- 执行逻辑保持 deterministic，不接真实模型。
- .NET Worker 后续只应通过 CLI / `SkillGateway` 这一层调用。
- 当前链路为 `story_intake -> plot_adaptation -> episode_writer -> character_bible -> style_bible -> storyboard_director -> image_prompt_builder -> image_generation -> video_prompt_builder -> video_generation -> voice_casting -> subtitle_generator -> auto_editor -> quality_checker -> export_packager`。
- 当前输出包括脚本、角色、画风、分镜、图片提示词、mock 图片资产、视频提示词、mock 视频片段、配音任务、SRT-ready 字幕、粗剪计划、质量报告结构和导出包占位结构。
- 当前 skill 本身不写数据库，不写真实媒体文件，不调用 FFmpeg；Stage 13 由 .NET Worker / Control API 边界负责把 envelope 写回 PostgreSQL。

## 示例命令

先进入本目录：

```powershell
cd D:\code\MiLuStudio\backend\sidecars\python-skills
```

运行故事输入示例：

```powershell
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill story_intake --input skills\story_intake\examples\input.json --output D:\code\MiLuStudio\.tmp\story_intake.output.json --pretty
```

运行 Stage 13 导出包占位示例：

```powershell
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill export_packager --input skills\export_packager\examples\input.json --output D:\code\MiLuStudio\.tmp\export_packager.output.json --pretty
```

## Stage 5-13 链路命令

```powershell
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill plot_adaptation --input skills\plot_adaptation\examples\input.json --output D:\code\MiLuStudio\.tmp\plot_adaptation.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill episode_writer --input skills\episode_writer\examples\input.json --output D:\code\MiLuStudio\.tmp\episode_writer.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill character_bible --input skills\character_bible\examples\input.json --output D:\code\MiLuStudio\.tmp\character_bible.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill style_bible --input skills\style_bible\examples\input.json --output D:\code\MiLuStudio\.tmp\style_bible.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill storyboard_director --input skills\storyboard_director\examples\input.json --output D:\code\MiLuStudio\.tmp\storyboard_director.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill image_prompt_builder --input skills\image_prompt_builder\examples\input.json --output D:\code\MiLuStudio\.tmp\image_prompt_builder.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill image_generation --input skills\image_generation\examples\input.json --output D:\code\MiLuStudio\.tmp\image_generation.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill video_prompt_builder --input skills\video_prompt_builder\examples\input.json --output D:\code\MiLuStudio\.tmp\video_prompt_builder.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill video_generation --input skills\video_generation\examples\input.json --output D:\code\MiLuStudio\.tmp\video_generation.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill voice_casting --input skills\voice_casting\examples\input.json --output D:\code\MiLuStudio\.tmp\voice_casting.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill subtitle_generator --input skills\subtitle_generator\examples\input.json --output D:\code\MiLuStudio\.tmp\subtitle_generator.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill auto_editor --input skills\auto_editor\examples\input.json --output D:\code\MiLuStudio\.tmp\auto_editor.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill quality_checker --input skills\quality_checker\examples\input.json --output D:\code\MiLuStudio\.tmp\quality_checker.output.json --pretty
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill export_packager --input skills\export_packager\examples\input.json --output D:\code\MiLuStudio\.tmp\export_packager.output.json --pretty
```

## 本地验证

```powershell
cd D:\code\MiLuStudio\backend\sidecars\python-skills
D:\soft\program\Python\Python313\python.exe -m compileall -q milu_studio_skills skills tests
D:\soft\program\Python\Python313\python.exe -m unittest discover -s tests -v
```

## 边界说明

- UI 必须通过 Control API 与后端通信，不能直接调用本目录。
- Control API / Worker 后续只应通过 CLI / `SkillGateway` 调用 Skills。
- PostgreSQL 写入、EF Core DbContext 和 Worker durable claiming 已由 Stage 12 / Stage 13 在 .NET 后端边界接入；Python skill 仍不直接写数据库。
- Stage 8 mock 图片资产只暴露逻辑 `milu://mock-assets/...` URI。
- Stage 9 mock 视频片段只暴露逻辑 `milu://mock-assets/...` URI。
- Stage 10 配音、字幕和剪辑只暴露逻辑 `milu://mock-assets/...` URI / output intent。
- Stage 11 质量检查只检查 envelope 元数据和 deterministic 结构；真实视觉 / 音频检测、媒体文件读取、FFmpeg validation 和重试执行都留给后续 adapter / Control API 阶段。
- Stage 13 `export_packager` 只输出 MP4 / SRT / JSON / ZIP 占位交付结构，不生成真实文件、不压缩 ZIP、不调用 FFmpeg。
