from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    storyboard = unwrap_skill_data(payload, "storyboard_director", "storyboard_director", "video_prompt_builder")
    image_prompt_builder = unwrap_skill_data(payload, "image_prompt_builder", "image_prompt_builder", "video_prompt_builder")
    image_generation = unwrap_skill_data(payload, "image_generation", "image_generation", "video_prompt_builder")
    asset_analysis = payload.get("asset_analysis", {})
    errors: list[str] = []

    if asset_analysis is None:
        asset_analysis = {}
    elif not isinstance(asset_analysis, dict):
        errors.append("asset_analysis must be an object when provided.")

    required_storyboard_fields = ["project_id", "episode_index", "title", "language", "aspect_ratio", "shots"]
    for field in required_storyboard_fields:
        if field not in storyboard:
            errors.append(f"storyboard_director.data.{field} is required.")

    required_prompt_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "aspect_ratio",
        "prompt_set_id",
        "image_requests",
        "negative_prompt",
    ]
    for field in required_prompt_fields:
        if field not in image_prompt_builder:
            errors.append(f"image_prompt_builder.data.{field} is required.")

    required_image_fields = ["project_id", "episode_index", "title", "language", "provider", "model", "assets", "asset_manifest"]
    for field in required_image_fields:
        if field not in image_generation:
            errors.append(f"image_generation.data.{field} is required.")

    for upstream_name, upstream_data in [
        ("image_prompt_builder", image_prompt_builder),
        ("image_generation", image_generation),
    ]:
        if storyboard.get("project_id") != upstream_data.get("project_id"):
            errors.append(f"storyboard_director.data.project_id must match {upstream_name}.data.project_id.")
        if storyboard.get("episode_index") != upstream_data.get("episode_index"):
            errors.append(f"storyboard_director.data.episode_index must match {upstream_name}.data.episode_index.")

    if image_generation.get("provider") != "mock" or image_generation.get("model") != "none":
        errors.append("image_generation.data must use provider=mock and model=none for Stage 9 prompt building.")

    shots = storyboard.get("shots")
    if not isinstance(shots, list) or not shots:
        errors.append("storyboard_director.data.shots must be a non-empty array.")
    else:
        for index, shot in enumerate(shots):
            if not isinstance(shot, dict):
                errors.append(f"storyboard_director.data.shots[{index}] must be an object.")
                continue
            for field in ["shot_id", "shot_index", "duration_seconds", "scene", "camera", "visual_action", "video_prompt_seed"]:
                if field not in shot:
                    errors.append(f"storyboard_director.data.shots[{index}].{field} is required.")

    image_requests = image_prompt_builder.get("image_requests")
    if not isinstance(image_requests, list) or not image_requests:
        errors.append("image_prompt_builder.data.image_requests must be a non-empty array.")

    assets = image_generation.get("assets")
    if not isinstance(assets, list) or not assets:
        errors.append("image_generation.data.assets must be a non-empty array.")
    else:
        for index, asset in enumerate(assets):
            if not isinstance(asset, dict):
                errors.append(f"image_generation.data.assets[{index}] must be an object.")
                continue
            for field in ["asset_id", "asset_type", "asset_uri", "file_written", "provider", "model"]:
                if field not in asset:
                    errors.append(f"image_generation.data.assets[{index}].{field} is required.")
            if asset.get("file_written") is not False:
                errors.append(f"image_generation.data.assets[{index}].file_written must be false in Stage 9.")

    if errors:
        raise SkillValidationError("video_prompt_builder input validation failed.", errors)

    return {
        "storyboard_director": storyboard,
        "image_prompt_builder": image_prompt_builder,
        "image_generation": image_generation,
        "asset_analysis": asset_analysis,
    }


def validate_output(data: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []
    required_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "aspect_ratio",
        "prompt_set_id",
        "video_requests",
        "negative_prompt",
        "source_asset_manifest",
        "generation_plan",
        "review",
        "checkpoint",
    ]
    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in video_prompt_builder output.")

    video_requests = data.get("video_requests")
    if not isinstance(video_requests, list) or not video_requests:
        errors.append("video_requests must be a non-empty array.")
    else:
        for index, request in enumerate(video_requests):
            if not isinstance(request, dict):
                errors.append(f"video_requests[{index}] must be an object.")
                continue
            for field in [
                "request_id",
                "shot_id",
                "duration_seconds",
                "generation_mode",
                "prompt",
                "negative_prompt",
                "source_images",
                "motion_plan",
                "seed_hint",
                "output_slot",
            ]:
                if field not in request:
                    errors.append(f"video_requests[{index}].{field} is required.")
            if not request.get("prompt"):
                errors.append(f"video_requests[{index}].prompt must not be empty.")
            if not isinstance(request.get("source_images"), list) or not request.get("source_images"):
                errors.append(f"video_requests[{index}].source_images must be a non-empty array.")

    generation_plan = data.get("generation_plan")
    if not isinstance(generation_plan, dict):
        errors.append("generation_plan must be an object.")
    else:
        if generation_plan.get("provider") != "none" or generation_plan.get("model") != "none":
            errors.append("generation_plan must keep provider=none and model=none in Stage 9 prompt builder.")
        if generation_plan.get("writes_files") is not False:
            errors.append("generation_plan.writes_files must be false.")
        if generation_plan.get("writes_database") is not False:
            errors.append("generation_plan.writes_database must be false.")
        if generation_plan.get("uses_ffmpeg") is not False:
            errors.append("generation_plan.uses_ffmpeg must be false.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not False:
        errors.append("video_prompt_builder checkpoint.required must be false.")

    if errors:
        raise SkillValidationError("video_prompt_builder output validation failed.", errors)

    return data
