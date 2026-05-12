# MiLuStudio Reference Projects

更新时间：2026-05-12

本文件记录参考项目的用途和边界。参考项目只用于理解流程、产品形态、目录组织、任务调度和授权风险，不复制源码。

## 主干参考

`D:\code\XiaoLouAI`

- 用途：公司现有 Windows 原生工程体系主参考。
- 参考点：React / Vite / TypeScript 前端风格、Windows 原生边界、后端控制面、工程目录克制风格。
- 边界：只参考工程经验，不把 XiaoLouAI 业务代码直接搬入 MiLuStudio。

## 漫剧流水线参考

`D:\code\AIComicBuilder`

- 用途：漫剧生产链路主参考。
- 参考点：剧本导入、角色提取、角色四视图、智能分镜、首尾帧、视频合成。
- 授权：Apache-2.0。
- 边界：可参考结构和流程，MiLuStudio 仍自研实现。

`D:\code\lumenx`

- 用途：行业 SOP 参考。
- 参考点：资产提取、风格定调、分镜图、分镜视频、合成成片。
- 授权：MIT。
- 边界：可参考业务流程，不直接搬 UI。

## Windows 本地体验参考

`D:\code\LocalMiniDrama`

- 用途：Windows 本地、一键短剧体验参考。
- 参考点：下载即用、低门槛流程、桌面交付形态。
- 授权：MIT。
- 边界：技术栈不作为 MiLuStudio 主干。

`D:\code\Toonflow-app`

- 用途：Electron 桌面分发和短剧工作台参考。
- 参考点：Electron 打包、桌面产品叙事、Skill 文件化思路。
- 授权：Apache-2.0。
- 边界：只参考打包和产品形态。

## Agent 和 Skills 参考

`D:\code\ArcReel-main`

- 用途：Agent、SSE、任务队列、供应商抽象、成本追踪参考。
- 授权：AGPL-3.0。
- 风险：不适合作为商业主干或直接复制代码。
- 边界：只参考思想。

`D:\code\OpenMontage`

- 用途：Skills、pipeline、FFmpeg、字幕、质检、成本记录参考。
- 授权：AGPL-3.0。
- 风险：不适合直接复制代码。
- 边界：只参考结构和概念。

`D:\code\huobao-drama`

- 用途：一句话生成短剧、Mastra Agent + skills 参考。
- 授权：GitHub licenseInfo 为空，README 标注 CC BY-NC-SA 4.0。
- 风险：商业使用风险高。
- 边界：只参考理念。

## 固定边界

- 不把任何参考项目作为主仓库二开。
- 不直接复制参考项目源码。
- 不恢复旧 MiLuAssistantWeb / MiLuAssistantDesktop 为代码来源。
- 不引入 Linux / Docker 作为第一版生产必需。
- 不开放公共 Skills 市场。
