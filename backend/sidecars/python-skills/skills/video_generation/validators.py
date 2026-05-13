from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    video_prompt_builder = unwrap_skill_data(payload, "video_prompt_builder", "video_prompt_builder", "video_generation")
    errors: list[str] = []

    required_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "aspect_ratio",
        "prompt_set_id",
        "video_requests",
    ]
    for field in required_fields:
        if field not in video_prompt_builder:
            errors.append(f"video_prompt_builder.data.{field} is required.")

    video_requests = video_prompt_builder.get("video_requests")
    if not isinstance(video_requests, list) or not video_requests:
        errors.append("video_prompt_builder.data.video_requests must be a non-empty array.")
    else:
        for index, request in enumerate(video_requests):
            if not isinstance(request, dict):
                errors.append(f"video_prompt_builder.data.video_requests[{index}] must be an object.")
                continue
            for field in [
                "request_id",
                "shot_id",
                "shot_index",
                "duration_seconds",
                "generation_mode",
                "prompt",
                "negative_prompt",
                "source_images",
                "aspect_ratio",
                "seed_hint",
            ]:
                if field not in request:
                    errors.append(f"video_prompt_builder.data.video_requests[{index}].{field} is required.")

    if errors:
        raise SkillValidationError("video_generation input validation failed.", errors)

    return {
        "video_prompt_builder": video_prompt_builder,
    }


def validate_output(data: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []
    required_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "provider",
        "model",
        "mode",
        "clips",
        "clip_manifest",
        "cost_estimate",
        "review",
        "checkpoint",
    ]
    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in video_generation output.")

    if data.get("provider") != "mock" or data.get("model") != "none":
        errors.append("video_generation must use provider=mock and model=none in Stage 9.")

    clips = data.get("clips")
    if not isinstance(clips, list) or not clips:
        errors.append("clips must be a non-empty array.")
    else:
        for index, clip in enumerate(clips):
            if not isinstance(clip, dict):
                errors.append(f"clips[{index}] must be an object.")
                continue
            for field in [
                "clip_id",
                "request_id",
                "shot_id",
                "duration_seconds",
                "status",
                "provider",
                "model",
                "generation_mode",
                "prompt",
                "asset_uri",
                "file_written",
                "storage_intent",
            ]:
                if field not in clip:
                    errors.append(f"clips[{index}].{field} is required.")
            if clip.get("file_written") is not False:
                errors.append(f"clips[{index}].file_written must be false in Stage 9.")

    manifest = data.get("clip_manifest")
    if not isinstance(manifest, dict):
        errors.append("clip_manifest must be an object.")
    else:
        if manifest.get("writes_files") is not False:
            errors.append("clip_manifest.writes_files must be false.")
        if manifest.get("writes_database") is not False:
            errors.append("clip_manifest.writes_database must be false.")
        if manifest.get("uses_ffmpeg") is not False:
            errors.append("clip_manifest.uses_ffmpeg must be false.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not True:
        errors.append("video_generation checkpoint.required must be true.")

    if errors:
        raise SkillValidationError("video_generation output validation failed.", errors)

    return data
