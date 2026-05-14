# MiLuStudio Stage 19 桌面发布验收清单

更新时间：2026-05-14

## 1. 阶段范围

Stage 19 的正式范围是“桌面发布验收与代码签名前置准备”：

- 验收 Windows 安装包、`win-unpacked` 产物、图标、快捷方式、自启动入口和桌面数据目录约束。
- 增加 Authenticode 签名状态检查；本地开发允许 `NotSigned`，正式发布必须使用 `-RequireSigned` 失败即阻断。
- 增加签名前置配置检查，但不提交证书、私钥、PFX/P12/PVK/SPC 或真实密钥。
- 复核 Electron 安全边界：Web 仍只通过 Control API 通信，Electron 只注入 API 地址和桌面会话 token。
- 复核桌面本地 Web host 响应头：必须设置 CSP 和 `X-Content-Type-Options: nosniff`。
- 复核桌面模式下 migrations apply 仍被拒绝，桌面端不执行 migrations、不定义数据库表、不负责数据库初始化。

本阶段不接真实模型、不读取真实媒体文件、不触发 FFmpeg、不生成真实 MP4/WAV/SRT/ZIP，不引入 Linux/Docker/Redis/Celery。

## 2. 自动化验收

本地快速验收：

```powershell
cd D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run verify:release
```

重新生成 Windows 安装产物后验收：

```powershell
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Test-MiLuStudioStage19DesktopRelease.ps1 -BuildPackage
```

正式签名发布验收：

```powershell
cd D:\code\MiLuStudio\apps\desktop
D:\soft\program\nodejs\npm.ps1 run verify:release:signed
```

脚本会生成报告：

```text
D:\code\MiLuStudio\.tmp\stage19-desktop-release-report.json
```

## 3. 签名前置配置

正式发布前至少配置一种证书选择方式：

```powershell
$env:MILUSTUDIO_CODESIGN_CERT_PATH = "D:\secure\certs\MiLuStudio-CodeSigning.pfx"
$env:MILUSTUDIO_CODESIGN_CERT_THUMBPRINT = "<thumbprint>"
$env:MILUSTUDIO_CODESIGN_TIMESTAMP_URL = "https://timestamp.example.com"
```

约束：

- 证书文件不得放在 `D:\code\MiLuStudio` 仓库内。
- 不得提交 `*.pfx`、`*.p12`、`*.pvk`、`*.spc`、`*.key`。
- 没有真实证书时，脚本记录 `NotSigned` 和签名阻塞项；不得伪造签名成功。
- 使用 `-RequireSigned` 时，安装器和 `win-unpacked\MiLuStudio.exe` 必须都是 `Valid`。

## 4. 干净 Windows 手工验收

建议在干净 Windows 虚拟机或快照环境中执行：

1. 运行 `outputs\desktop\MiLuStudio-Setup-0.1.0.exe`。
2. 确认安装器显示 MiLuStudio 品牌图标和 Windows 集成选项。
3. 勾选桌面快捷方式、开始菜单快捷方式和开机自启动后完成安装。
4. 首次启动后确认只进入 Web UI 登录/注册入口，未出现许可证、激活码或付费码体验。
5. 确认应用数据、日志和输出目录位于安装目录旁的 `data\` 下，而不是由桌面端写入数据库初始化逻辑。
6. 确认未登录时受保护 Control API 返回认证错误，桌面 token 只作为桌面安全门，不替代账号登录。
7. 确认 `POST /api/system/migrations/apply` 在桌面模式下返回 403。
8. 退出应用并卸载，确认开始菜单、桌面快捷方式和开机自启动快捷方式被移除。
9. 确认 `deleteAppDataOnUninstall=false`：用户数据默认保留，是否清理由后续正式卸载策略决定。

## 5. 当前阻塞项

- 当前本地产物 Authenticode 状态为 `NotSigned`；这对 Stage 19 本地验收可接受，但正式公开发布前必须配置真实代码签名证书与可信时间戳。
- 本阶段只完成签名前置检查和验收脚本，不引入真实 provider adapter、secure secret store 或 spend guard。
