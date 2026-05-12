# MiLuStudio

MiLuStudio is a new Windows-native AI comic-drama production Agent.

The product target is simple: a user provides a Chinese story, novel fragment,
or creative request, and MiLuStudio guides a one-click production flow for
script, characters, style, storyboard, images, video clips, subtitles, editing,
quality checks, and final export.

## Current Status

Stage 0 and Stage 1 are in progress:

- Git repository initialized.
- Planning documents live in `docs\`.
- The first frontend shell lives in `apps\web`.
- The current app uses mock data only.
- No backend, model provider, FFmpeg, Python skill, or real user asset pipeline
  is connected yet.

## Product Boundary

MiLuStudio is not a continuation of old MiLuAssistantWeb or
MiLuAssistantDesktop. Those projects are historical experience only.

Reference projects such as AIComicBuilder, LocalMiniDrama, LumenX, ArcReel,
Toonflow, Huobao Drama, and OpenMontage are used only for workflow and product
thinking. Their source code is not copied into this repository.

## Development Rules

- Keep the root directory small.
- Put long-lived documents and handoff records in `docs\`.
- Organize frontend, backend, and Production Skills by route or feature domain.
- Do not make Linux, Docker, or cloud SaaS a first-version production
  requirement.
- Keep dependencies, caches, logs, uploads, and generated outputs inside
  `D:\code\MiLuStudio` or another explicit D drive tools directory.
- Do not commit API keys, real user assets, generated videos, logs, or local
  runtime data.

## Frontend

```powershell
cd D:\code\MiLuStudio\apps\web
. ..\..\scripts\windows\Set-MiLuStudioEnv.ps1
npm install
npm run build
npm run dev
```

The frontend shell is a Vite + React + TypeScript app with a project list,
project production console, conversation input, mode switch, mock stage
progress, result cards, and delivery panel.
