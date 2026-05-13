-- MiLuStudio initial PostgreSQL control-plane schema.
-- Stage 13+ defaults to PostgreSQL as the business state source; InMemory is an explicit smoke-test provider only.

create table if not exists projects (
    id text primary key,
    name text not null,
    description text not null,
    mode text not null check (mode in ('fast', 'director')),
    status text not null check (status in ('draft', 'running', 'paused', 'completed', 'failed')),
    target_duration_seconds integer not null check (target_duration_seconds between 30 and 60),
    aspect_ratio text not null check (aspect_ratio in ('9:16', '16:9', '1:1')),
    style_preset text not null,
    created_at timestamptz not null,
    updated_at timestamptz not null
);

create table if not exists story_inputs (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    source_type text not null check (source_type in ('text', 'file', 'url')),
    original_text text not null,
    file_asset_id text null,
    language text not null,
    word_count integer not null default 0,
    parsed_at timestamptz null
);

create table if not exists characters (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    name text not null,
    role_type text not null,
    gender text null,
    age_range text null,
    personality text null,
    appearance text null,
    costume text null,
    voice_profile text null,
    consistency_notes text null
);

create table if not exists shots (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    episode_id text null,
    shot_index integer not null,
    duration_seconds integer not null,
    scene_summary text not null,
    dialogue text null,
    narration text null,
    characters_json jsonb not null default '[]'::jsonb,
    camera_angle text null,
    camera_motion text null,
    lighting text null,
    composition text null,
    image_prompt text null,
    video_prompt text null,
    status text not null,
    user_locked boolean not null default false
);

create table if not exists assets (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    kind text not null,
    local_path text not null,
    mime_type text not null,
    file_size bigint not null default 0,
    sha256 text null,
    metadata_json jsonb null,
    created_at timestamptz not null
);

create table if not exists production_jobs (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    current_stage text not null,
    status text not null check (status in ('queued', 'running', 'paused', 'completed', 'failed')),
    progress_percent integer not null check (progress_percent between 0 and 100),
    started_at timestamptz not null,
    finished_at timestamptz null,
    error_message text null
);

create table if not exists generation_tasks (
    id text primary key,
    job_id text not null references production_jobs(id) on delete cascade,
    project_id text not null references projects(id) on delete cascade,
    shot_id text null,
    skill_name text not null,
    provider text not null,
    input_json jsonb not null,
    output_json jsonb null,
    status text not null check (status in ('waiting', 'running', 'review', 'completed', 'failed')),
    attempt_count integer not null default 0,
    cost_estimate numeric(12, 4) not null default 0,
    cost_actual numeric(12, 4) null,
    started_at timestamptz null,
    finished_at timestamptz null,
    error_message text null
);

create table if not exists cost_ledger (
    id text primary key,
    project_id text not null references projects(id) on delete cascade,
    task_id text null references generation_tasks(id) on delete set null,
    provider text not null,
    model text not null,
    unit text not null,
    quantity numeric(12, 4) not null,
    estimated_cost numeric(12, 4) not null,
    actual_cost numeric(12, 4) null,
    created_at timestamptz not null
);

create index if not exists ix_story_inputs_project_id on story_inputs(project_id);
create index if not exists ix_characters_project_id on characters(project_id);
create index if not exists ix_shots_project_id on shots(project_id, shot_index);
create index if not exists ix_assets_project_id on assets(project_id, kind);
create index if not exists ix_production_jobs_project_id on production_jobs(project_id, started_at desc);
create index if not exists ix_generation_tasks_job_id on generation_tasks(job_id);
create index if not exists ix_cost_ledger_project_id on cost_ledger(project_id, created_at desc);
