# MiLuStudio Python Skills Runtime

Internal sidecar for Production Skills.

Current Stage 10 scope:

- Run skills through one CLI boundary.
- Keep input/output as JSON files.
- Keep execution deterministic and model-free.
- Keep the .NET Worker integration point as a single gateway boundary.
- Chain `story_intake` -> `plot_adaptation` -> `episode_writer` -> `character_bible` -> `style_bible` -> `storyboard_director` -> `image_prompt_builder` -> `image_generation` -> `video_prompt_builder` -> `video_generation` -> `voice_casting` -> `subtitle_generator` -> `auto_editor` through runtime envelopes.
- Produce reviewable script, character, style, storyboard, image prompt, mock placeholder image asset, video prompt, mock placeholder video clip, voice task, SRT-ready subtitle, and rough edit plan structures without database or media file writes.

Story intake example:

```powershell
D:\soft\program\Python\Python313\python.exe -m milu_studio_skills run --skill story_intake --input skills\story_intake\examples\input.json --output D:\code\MiLuStudio\.tmp\story_intake.output.json --pretty
```

Stage 5-10 chain:

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
```

Boundary notes:

- UI must not call this runtime directly.
- The Control API / Worker side should invoke only the CLI / `SkillGateway` boundary.
- PostgreSQL writes, EF Core DbContext, and durable Worker task claiming stay outside this sidecar stage.
- Stage 8 mock image assets expose logical `milu://mock-assets/...` URIs only; real provider calls, file writes, selection, retries, and persistence remain deferred to later adapter stages.
- Stage 9 mock video clips expose logical `milu://mock-assets/...` URIs only; real video provider calls, MP4 writes, previews, retries, persistence, and FFmpeg remain deferred to later adapter stages.
- Stage 10 voice, subtitle, and edit outputs expose logical `milu://mock-assets/...` URI intents only; real TTS, BGM/SFX, WAV/SRT/MP4 writes, FFmpeg, previews, downloads, retries, and persistence remain deferred to later adapter/export stages.
