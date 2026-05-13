# MiLuStudio PostgreSQL 配置说明

更新时间：2026-05-13

本文记录 Stage 12 / Stage 13 的本地 PostgreSQL 配置和后端检查方式。  
数据库能力属于 Control API / Worker / Infrastructure，不属于 Electron 安装器。

## 当前默认

Stage 13 起，默认开发配置切到 PostgreSQL：

```json
"ControlPlane": {
  "RepositoryProvider": "PostgreSQL"
}
```

本机开发连接串写入版本库配置：

```powershell
Host=127.0.0.1;Port=5432;Database=milu;Username=root;Password=root
```

说明：

- 复用本机已存在的 PostgreSQL 18 Windows 服务。
- 使用数据库账号 `root`，密码 `root`。
- MiLuStudio 使用独立数据库 `milu`，不使用 XiaoLouAI 的 `xiaolou` 数据库。
- InMemory provider 保留，但只作为快速 smoke / 特殊轻量场景。
- 当前本机 `root/root` 可连接但没有 `CREATEDB` 权限；初始化脚本会在需要创建库时使用本机 `postgres/root` bootstrap 创建 `milu`，并把 owner 设为 `root`。应用运行仍只使用 `root/root`。

幂等初始化：

```powershell
powershell -ExecutionPolicy Bypass -File D:\code\MiLuStudio\scripts\windows\Initialize-MiLuStudioPostgreSql.ps1
```

## 后端 preflight

启动 Control API 后检查：

```powershell
Invoke-RestMethod http://127.0.0.1:5268/api/system/preflight
```

preflight 会检查：

- 当前 `RepositoryProvider`。
- PostgreSQL connection string 是否存在。
- 数据库是否可达。
- SQL migration 是否已全部应用。
- `storage` 根目录是否存在。

如果 PostgreSQL 未就绪，Control API 返回 503 和结构化修复建议。  
桌面端后续只能展示这个结果，不能直接连接数据库。

## Migration 状态

查看 migration：

```powershell
Invoke-RestMethod http://127.0.0.1:5268/api/system/migrations
```

应用 pending migration：

```powershell
Invoke-RestMethod -Method Post http://127.0.0.1:5268/api/system/migrations/apply
```

当前 SQL 文件：

```text
D:\code\MiLuStudio\backend\control-plane\db\migrations\001_initial_control_plane.sql
D:\code\MiLuStudio\backend\control-plane\db\migrations\002_stage12_postgresql_claiming.sql
```

`002_stage12_postgresql_claiming.sql` 为 `generation_tasks` 增加：

- `queue_index`
- `locked_by`
- `locked_until`
- `last_heartbeat_at`
- durable claiming indexes

## Worker durable claiming

Worker 通过后端 repository 调用领取任务。  
PostgreSQL provider 使用：

```sql
for update skip locked
```

这保证多个 Worker 进程不会重复领取同一个 waiting task。  
如果 Worker 崩溃，`locked_until` 过期后的 running task 也可被后续 Worker 接管。  
Stage 12 只完成 durable claiming 边界。  
Stage 13 已让 Worker 通过内部 adapter 调用 Python deterministic skills，但仍不接真实模型、不读取真实媒体、不触发 FFmpeg。

## Skill 输出写入

Stage 5-13 的 Production Skill envelope 通过 Control API / Worker 写入：

```powershell
Invoke-RestMethod `
  -Method Post `
  -ContentType "application/json" `
  -Uri http://127.0.0.1:5268/api/generation-tasks/{taskId}/output `
  -Body '{"outputJson":"{\"ok\":true}","assetKind":"skill_envelope","provider":"none","model":"none","unit":"skill_envelope","quantity":1,"estimatedCost":0,"actualCost":0,"requiresReview":false}'
```

写入后：

- `generation_tasks.output_json` 保存完整 envelope。
- `assets` 增加 `db://generation_tasks/{taskId}/output_json` 索引记录。
- 如请求包含成本字段，`cost_ledger` 增加成本记录。
- Stage 13 已收敛为 Worker 自动调用 deterministic skill 后写回，API 只保留边界和调试入口。

## Stage 13 真实配置验收

Stage 13 已验证：

- `milu` 数据库已创建。
- `/api/system/preflight` 返回 healthy。
- `/api/system/migrations` 显示 SQL migration 已全部应用。
- API 和 Worker 在 PostgreSQL provider 下共享同一份任务状态。
- Worker 调用 Python `SkillGateway` 后，output envelope 写入 PostgreSQL。
- 前端只通过 Control API 展示真实任务结果和资产索引。
- 本地 smoke job `job_ba4b02d1cd534e948fe0fda74aaead3c` 达到 `completed / 100`，15 个 task 全部 completed 且均有 output envelope，`cost_ledger` 15 行。

## 明确禁止

- Electron 不直接访问 PostgreSQL。
- Electron 不执行 migrations。
- Electron 不创建数据库表。
- Electron 不初始化业务 storage。
- UI 不直接访问数据库、文件系统、Python 脚本或 FFmpeg。
- Stage 12 / Stage 13 不引入 Docker / Redis / Celery 作为生产依赖。
