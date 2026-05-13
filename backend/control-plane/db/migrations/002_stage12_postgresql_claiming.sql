-- MiLuStudio Stage 12 PostgreSQL provider additions.
-- These columns support durable Worker claiming without Redis or Celery.

alter table generation_tasks
    add column if not exists queue_index integer not null default 0,
    add column if not exists locked_by text null,
    add column if not exists locked_until timestamptz null,
    add column if not exists last_heartbeat_at timestamptz null;

with ordered_tasks as (
    select
        id,
        row_number() over (partition by job_id order by skill_name, id) - 1 as calculated_queue_index
    from generation_tasks
)
update generation_tasks
set queue_index = ordered_tasks.calculated_queue_index
from ordered_tasks
where generation_tasks.id = ordered_tasks.id
  and generation_tasks.queue_index = 0;

create index if not exists ix_generation_tasks_claiming
    on generation_tasks(job_id, queue_index, status, locked_until);

create index if not exists ix_generation_tasks_skill
    on generation_tasks(project_id, skill_name);
