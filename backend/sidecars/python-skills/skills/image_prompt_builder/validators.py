from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    storyboard = unwrap_skill_data(payload, "storyboard_director", "storyboard_director", "image_prompt_builder")
    character_bible = unwrap_skill_data(payload, "character_bible", "character_bible", "image_prompt_builder")
    style_bible = unwrap_skill_data(payload, "style_bible", "style_bible", "image_prompt_builder")
    errors: list[str] = []

    required_storyboard_fields = ["project_id", "episode_index", "title", "language", "aspect_ratio", "shots"]
    for field in required_storyboard_fields:
        if field not in storyboard:
            errors.append(f"storyboard_director.data.{field} is required.")

    required_character_fields = ["project_id", "episode_index", "characters"]
    for field in required_character_fields:
        if field not in character_bible:
            errors.append(f"character_bible.data.{field} is required.")

    required_style_fields = ["project_id", "episode_index", "style_name", "reusable_prompt_blocks", "negative_prompt"]
    for field in required_style_fields:
        if field not in style_bible:
            errors.append(f"style_bible.data.{field} is required.")

    for upstream_name, upstream_data in [("character_bible", character_bible), ("style_bible", style_bible)]:
        if storyboard.get("project_id") != upstream_data.get("project_id"):
            errors.append(f"storyboard_director.data.project_id must match {upstream_name}.data.project_id.")
        if storyboard.get("episode_index") != upstream_data.get("episode_index"):
            errors.append(f"storyboard_director.data.episode_index must match {upstream_name}.data.episode_index.")

    shots = storyboard.get("shots")
    if not isinstance(shots, list) or not shots:
        errors.append("storyboard_director.data.shots must be a non-empty array.")
    else:
        for index, shot in enumerate(shots):
            if not isinstance(shot, dict):
                errors.append(f"storyboard_director.data.shots[{index}] must be an object.")
                continue
            for field in ["shot_id", "scene", "characters", "image_prompt_seed", "camera", "lighting"]:
                if field not in shot:
                    errors.append(f"storyboard_director.data.shots[{index}].{field} is required.")

    if not isinstance(character_bible.get("characters"), list) or not character_bible.get("characters"):
        errors.append("character_bible.data.characters must be a non-empty array.")

    prompt_blocks = style_bible.get("reusable_prompt_blocks")
    if not isinstance(prompt_blocks, dict) or not prompt_blocks.get("base_style"):
        errors.append("style_bible.data.reusable_prompt_blocks.base_style is required.")

    if errors:
        raise SkillValidationError("image_prompt_builder input validation failed.", errors)

    return {
        "storyboard_director": storyboard,
        "character_bible": character_bible,
        "style_bible": style_bible,
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
        "image_requests",
        "negative_prompt",
        "reference_strategy",
        "generation_plan",
        "review",
        "checkpoint",
    ]
    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in image_prompt_builder output.")

    image_requests = data.get("image_requests")
    if not isinstance(image_requests, list) or not image_requests:
        errors.append("image_requests must be a non-empty array.")
    else:
        for index, request in enumerate(image_requests):
            if not isinstance(request, dict):
                errors.append(f"image_requests[{index}] must be an object.")
                continue
            for field in [
                "request_id",
                "asset_type",
                "prompt",
                "negative_prompt",
                "aspect_ratio",
                "style_refs",
                "seed_hint",
                "output_slot",
            ]:
                if field not in request:
                    errors.append(f"image_requests[{index}].{field} is required.")
            if not request.get("prompt"):
                errors.append(f"image_requests[{index}].prompt must not be empty.")

    if not isinstance(data.get("negative_prompt"), list) or not data.get("negative_prompt"):
        errors.append("negative_prompt must be a non-empty array.")

    generation_plan = data.get("generation_plan")
    if not isinstance(generation_plan, dict) or generation_plan.get("provider") != "none":
        errors.append("generation_plan.provider must be none in Stage 8 prompt builder.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not False:
        errors.append("image_prompt_builder checkpoint.required must be false.")

    if errors:
        raise SkillValidationError("image_prompt_builder output validation failed.", errors)

    return data

