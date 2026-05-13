-- MiLuStudio Stage 14 checkpoint notes persistence.
-- Stores user approve/reject notes on the generation task that owns the checkpoint.

alter table generation_tasks
    add column if not exists checkpoint_notes text null;
