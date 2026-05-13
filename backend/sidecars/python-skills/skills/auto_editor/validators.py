from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    storyboard = unwrap_skill_data(payload, "storyboard_director", "storyboard_director", "auto_editor")
    video_generation = unwrap_skill_data(payload, "video_generation", "video_generation", "auto_editor")
    voice_casting = unwrap_skill_data(payload, "voice_casting", "voice_casting", "auto_editor")
    subtitle_generator = unwrap_skill_data(payload, "subtitle_generator", "subtitle_generator", "auto_editor")
    errors: list[str] = []

    for field in ["project_id", "episode_index", "title", "language", "aspect_ratio", "shots", "timing_summary"]:
        if field not in storyboard:
            errors.append(f"storyboard_director.data.{field} is required.")

    for field in ["project_id", "episode_index", "clips", "clip_manifest"]:
        if field not in video_generation:
            errors.append(f"video_generation.data.{field} is required.")

    for field in ["project_id", "episode_index", "voice_tasks"]:
        if field not in voice_casting:
            errors.append(f"voice_casting.data.{field} is required.")

    for field in ["project_id", "episode_index", "subtitle_cues", "srt_text"]:
        if field not in subtitle_generator:
            errors.append(f"subtitle_generator.data.{field} is required.")

    for upstream_name, upstream_data in [
        ("video_generation", video_generation),
        ("voice_casting", voice_casting),
        ("subtitle_generator", subtitle_generator),
    ]:
        if storyboard.get("project_id") != upstream_data.get("project_id"):
            errors.append(f"storyboard_director.data.project_id must match {upstream_name}.data.project_id.")
        if storyboard.get("episode_index") != upstream_data.get("episode_index"):
            errors.append(f"storyboard_director.data.episode_index must match {upstream_name}.data.episode_index.")

    clips = video_generation.get("clips")
    if not isinstance(clips, list) or not clips:
        errors.append("video_generation.data.clips must be a non-empty array.")
    else:
        for index, clip in enumerate(clips):
            if not isinstance(clip, dict):
                errors.append(f"video_generation.data.clips[{index}] must be an object.")
                continue
            for field in ["clip_id", "shot_id", "duration_seconds", "asset_uri", "file_written"]:
                if field not in clip:
                    errors.append(f"video_generation.data.clips[{index}].{field} is required.")
            if clip.get("file_written") is not False:
                errors.append(f"video_generation.data.clips[{index}].file_written must be false in Stage 10.")

    if not isinstance(storyboard.get("shots"), list) or not storyboard.get("shots"):
        errors.append("storyboard_director.data.shots must be a non-empty array.")

    if not isinstance(voice_casting.get("voice_tasks"), list) or not voice_casting.get("voice_tasks"):
        errors.append("voice_casting.data.voice_tasks must be a non-empty array.")

    if not isinstance(subtitle_generator.get("subtitle_cues"), list) or not subtitle_generator.get("subtitle_cues"):
        errors.append("subtitle_generator.data.subtitle_cues must be a non-empty array.")

    if errors:
        raise SkillValidationError("auto_editor input validation failed.", errors)

    return {
        "storyboard_director": storyboard,
        "video_generation": video_generation,
        "voice_casting": voice_casting,
        "subtitle_generator": subtitle_generator,
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
        "timeline",
        "render_plan",
        "edit_manifest",
        "cost_estimate",
        "review",
        "checkpoint",
    ]:
        if field not in data:
            errors.append(f"{field} is required in auto_editor output.")

    if data.get("provider") != "none" or data.get("model") != "none":
        errors.append("auto_editor must keep provider=none and model=none in Stage 10.")

    timeline = data.get("timeline")
    if not isinstance(timeline, dict):
        errors.append("timeline must be an object.")
    else:
        tracks = timeline.get("tracks")
        if not isinstance(tracks, dict):
            errors.append("timeline.tracks must be an object.")
        elif not tracks.get("video") or not tracks.get("audio") or not tracks.get("subtitles"):
            errors.append("timeline.tracks must include video, audio, and subtitles.")

    render_plan = data.get("render_plan")
    if not isinstance(render_plan, dict):
        errors.append("render_plan must be an object.")
    else:
        if render_plan.get("engine") != "none":
            errors.append("render_plan.engine must be none in Stage 10.")
        if render_plan.get("uses_ffmpeg") is not False:
            errors.append("render_plan.uses_ffmpeg must be false.")
        if render_plan.get("writes_files") is not False:
            errors.append("render_plan.writes_files must be false.")
        if render_plan.get("writes_database") is not False:
            errors.append("render_plan.writes_database must be false.")

    manifest = data.get("edit_manifest")
    if not isinstance(manifest, dict):
        errors.append("edit_manifest must be an object.")
    else:
        if manifest.get("writes_files") is not False:
            errors.append("edit_manifest.writes_files must be false.")
        if manifest.get("writes_database") is not False:
            errors.append("edit_manifest.writes_database must be false.")
        if manifest.get("uses_ffmpeg") is not False:
            errors.append("edit_manifest.uses_ffmpeg must be false.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not False:
        errors.append("auto_editor checkpoint.required must be false.")

    if errors:
        raise SkillValidationError("auto_editor output validation failed.", errors)

    return data
