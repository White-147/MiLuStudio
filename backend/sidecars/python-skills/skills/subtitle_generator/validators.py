from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    episode = unwrap_skill_data(payload, "episode_writer", "episode_writer", "subtitle_generator")
    storyboard = unwrap_skill_data(payload, "storyboard_director", "storyboard_director", "subtitle_generator")
    voice_casting = unwrap_skill_data(payload, "voice_casting", "voice_casting", "subtitle_generator")
    errors: list[str] = []

    for field in ["project_id", "episode_index", "title", "language", "subtitle_cues", "segments"]:
        if field not in episode:
            errors.append(f"episode_writer.data.{field} is required.")

    for field in ["project_id", "episode_index", "shots"]:
        if field not in storyboard:
            errors.append(f"storyboard_director.data.{field} is required.")

    for field in ["project_id", "episode_index", "voice_tasks"]:
        if field not in voice_casting:
            errors.append(f"voice_casting.data.{field} is required.")

    for upstream_name, upstream_data in [("storyboard_director", storyboard), ("voice_casting", voice_casting)]:
        if episode.get("project_id") != upstream_data.get("project_id"):
            errors.append(f"episode_writer.data.project_id must match {upstream_name}.data.project_id.")
        if episode.get("episode_index") != upstream_data.get("episode_index"):
            errors.append(f"episode_writer.data.episode_index must match {upstream_name}.data.episode_index.")

    voice_tasks = voice_casting.get("voice_tasks")
    if not isinstance(voice_tasks, list) or not voice_tasks:
        errors.append("voice_casting.data.voice_tasks must be a non-empty array.")
    else:
        for index, task in enumerate(voice_tasks):
            if not isinstance(task, dict):
                errors.append(f"voice_casting.data.voice_tasks[{index}] must be an object.")
                continue
            for field in ["task_id", "source_segment_id", "source_type", "text", "start_second", "end_second", "target_shot_ids"]:
                if field not in task:
                    errors.append(f"voice_casting.data.voice_tasks[{index}].{field} is required.")

    if errors:
        raise SkillValidationError("subtitle_generator input validation failed.", errors)

    return {
        "episode_writer": episode,
        "storyboard_director": storyboard,
        "voice_casting": voice_casting,
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
        "subtitle_cues",
        "srt_text",
        "subtitle_manifest",
        "generation_plan",
        "review",
        "checkpoint",
    ]:
        if field not in data:
            errors.append(f"{field} is required in subtitle_generator output.")

    if data.get("provider") != "none" or data.get("model") != "none":
        errors.append("subtitle_generator must keep provider=none and model=none in Stage 10.")

    cues = data.get("subtitle_cues")
    if not isinstance(cues, list) or not cues:
        errors.append("subtitle_cues must be a non-empty array.")
    else:
        for index, cue in enumerate(cues):
            if not isinstance(cue, dict):
                errors.append(f"subtitle_cues[{index}] must be an object.")
                continue
            for field in ["cue_id", "index", "start_second", "end_second", "text", "source_task_id", "target_shot_ids"]:
                if field not in cue:
                    errors.append(f"subtitle_cues[{index}].{field} is required.")
            if len(str(cue.get("text", ""))) > 32:
                errors.append(f"subtitle_cues[{index}].text should stay within 32 characters.")
            if cue.get("end_second", 0) <= cue.get("start_second", 0):
                errors.append(f"subtitle_cues[{index}] end_second must be greater than start_second.")

    if "-->" not in str(data.get("srt_text", "")):
        errors.append("srt_text must contain SRT time separators.")

    manifest = data.get("subtitle_manifest")
    if not isinstance(manifest, dict):
        errors.append("subtitle_manifest must be an object.")
    else:
        if manifest.get("writes_files") is not False:
            errors.append("subtitle_manifest.writes_files must be false.")
        if manifest.get("writes_database") is not False:
            errors.append("subtitle_manifest.writes_database must be false.")

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
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not False:
        errors.append("subtitle_generator checkpoint.required must be false.")

    if errors:
        raise SkillValidationError("subtitle_generator output validation failed.", errors)

    return data
