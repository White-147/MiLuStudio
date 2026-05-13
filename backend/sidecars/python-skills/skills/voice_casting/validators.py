from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    episode = unwrap_skill_data(payload, "episode_writer", "episode_writer", "voice_casting")
    storyboard = unwrap_skill_data(payload, "storyboard_director", "storyboard_director", "voice_casting")
    errors: list[str] = []

    for field in [
        "project_id",
        "episode_index",
        "title",
        "language",
        "target_duration_seconds",
        "segments",
        "voice_notes",
    ]:
        if field not in episode:
            errors.append(f"episode_writer.data.{field} is required.")

    for field in ["project_id", "episode_index", "shots", "timing_summary"]:
        if field not in storyboard:
            errors.append(f"storyboard_director.data.{field} is required.")

    if episode.get("project_id") != storyboard.get("project_id"):
        errors.append("episode_writer.data.project_id must match storyboard_director.data.project_id.")
    if episode.get("episode_index") != storyboard.get("episode_index"):
        errors.append("episode_writer.data.episode_index must match storyboard_director.data.episode_index.")

    segments = episode.get("segments")
    if not isinstance(segments, list) or not segments:
        errors.append("episode_writer.data.segments must be a non-empty array.")
    else:
        for index, segment in enumerate(segments):
            if not isinstance(segment, dict):
                errors.append(f"episode_writer.data.segments[{index}] must be an object.")
                continue
            for field in ["segment_id", "start_second", "duration_seconds", "narration", "dialogue_lines"]:
                if field not in segment:
                    errors.append(f"episode_writer.data.segments[{index}].{field} is required.")

    shots = storyboard.get("shots")
    if not isinstance(shots, list) or not shots:
        errors.append("storyboard_director.data.shots must be a non-empty array.")
    else:
        for index, shot in enumerate(shots):
            if not isinstance(shot, dict):
                errors.append(f"storyboard_director.data.shots[{index}] must be an object.")
                continue
            for field in ["shot_id", "source_segment_id", "start_second", "duration_seconds"]:
                if field not in shot:
                    errors.append(f"storyboard_director.data.shots[{index}].{field} is required.")

    if errors:
        raise SkillValidationError("voice_casting input validation failed.", errors)

    return {
        "episode_writer": episode,
        "storyboard_director": storyboard,
    }


def validate_output(data: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []

    for field in [
        "project_id",
        "episode_index",
        "title",
        "language",
        "provider",
        "model",
        "mode",
        "voice_profiles",
        "voice_tasks",
        "voice_manifest",
        "generation_plan",
        "cost_estimate",
        "review",
        "checkpoint",
    ]:
        if field not in data:
            errors.append(f"{field} is required in voice_casting output.")

    if data.get("provider") != "none" or data.get("model") != "none":
        errors.append("voice_casting must keep provider=none and model=none in Stage 10.")

    profiles = data.get("voice_profiles")
    if not isinstance(profiles, list) or not profiles:
        errors.append("voice_profiles must be a non-empty array.")

    tasks = data.get("voice_tasks")
    if not isinstance(tasks, list) or not tasks:
        errors.append("voice_tasks must be a non-empty array.")
    else:
        for index, task in enumerate(tasks):
            if not isinstance(task, dict):
                errors.append(f"voice_tasks[{index}] must be an object.")
                continue
            for field in [
                "task_id",
                "source_segment_id",
                "source_type",
                "speaker",
                "voice_profile_id",
                "text",
                "start_second",
                "end_second",
                "duration_seconds",
                "target_shot_ids",
                "output_audio_intent",
            ]:
                if field not in task:
                    errors.append(f"voice_tasks[{index}].{field} is required.")
            if task.get("duration_seconds", 0) <= 0:
                errors.append(f"voice_tasks[{index}].duration_seconds must be positive.")

    manifest = data.get("voice_manifest")
    if not isinstance(manifest, dict):
        errors.append("voice_manifest must be an object.")
    else:
        if manifest.get("writes_files") is not False:
            errors.append("voice_manifest.writes_files must be false.")
        if manifest.get("writes_database") is not False:
            errors.append("voice_manifest.writes_database must be false.")

    generation_plan = data.get("generation_plan")
    if not isinstance(generation_plan, dict):
        errors.append("generation_plan must be an object.")
    else:
        if generation_plan.get("provider") != "none" or generation_plan.get("model") != "none":
            errors.append("generation_plan must keep provider=none and model=none.")
        if generation_plan.get("writes_files") is not False:
            errors.append("generation_plan.writes_files must be false.")
        if generation_plan.get("writes_database") is not False:
            errors.append("generation_plan.writes_database must be false.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not True:
        errors.append("voice_casting checkpoint.required must be true.")

    if errors:
        raise SkillValidationError("voice_casting output validation failed.", errors)

    return data
