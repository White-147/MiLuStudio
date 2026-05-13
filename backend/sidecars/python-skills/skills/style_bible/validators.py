from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    episode_writer = unwrap_skill_data(payload, "episode_writer", "episode_writer", "style_bible")
    character_bible = unwrap_skill_data(payload, "character_bible", "character_bible", "style_bible")
    errors: list[str] = []

    required_episode_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "summary",
        "script_text",
        "segments",
        "target_duration_seconds",
        "aspect_ratio",
    ]

    for field in required_episode_fields:
        if field not in episode_writer:
            errors.append(f"episode_writer.data.{field} is required.")

    required_character_fields = ["project_id", "episode_index", "characters", "continuity_rules"]
    for field in required_character_fields:
        if field not in character_bible:
            errors.append(f"character_bible.data.{field} is required.")

    if episode_writer.get("project_id") != character_bible.get("project_id"):
        errors.append("episode_writer.data.project_id must match character_bible.data.project_id.")

    if episode_writer.get("episode_index") != character_bible.get("episode_index"):
        errors.append("episode_writer.data.episode_index must match character_bible.data.episode_index.")

    if not isinstance(character_bible.get("characters"), list) or not character_bible.get("characters"):
        errors.append("character_bible.data.characters must be a non-empty array.")

    if errors:
        raise SkillValidationError("style_bible input validation failed.", errors)

    return {
        "episode_writer": episode_writer,
        "character_bible": character_bible,
    }


def validate_output(data: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []
    required_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "style_name",
        "visual_style",
        "color_palette",
        "lighting",
        "environment_design",
        "camera_language",
        "character_rendering_rules",
        "negative_prompt",
        "reusable_prompt_blocks",
        "image_prompt_guidelines",
        "video_prompt_guidelines",
        "continuity_notes",
        "review",
        "checkpoint",
    ]

    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in style_bible output.")

    palette = data.get("color_palette")
    if not isinstance(palette, list) or len(palette) < 4:
        errors.append("color_palette must contain at least 4 colors.")

    negative_prompt = data.get("negative_prompt")
    if not isinstance(negative_prompt, list) or len(negative_prompt) < 4:
        errors.append("negative_prompt must contain reusable guardrails.")

    reusable_prompt_blocks = data.get("reusable_prompt_blocks")
    if not isinstance(reusable_prompt_blocks, dict):
        errors.append("reusable_prompt_blocks must be an object.")
    else:
        for field in ["base_style", "character_consistency", "environment", "camera", "quality_guardrails"]:
            if not reusable_prompt_blocks.get(field):
                errors.append(f"reusable_prompt_blocks.{field} is required.")

    character_rules = data.get("character_rendering_rules")
    if not isinstance(character_rules, list) or not character_rules:
        errors.append("character_rendering_rules must not be empty.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not True:
        errors.append("style_bible checkpoint.required must be true.")

    if errors:
        raise SkillValidationError("style_bible output validation failed.", errors)

    return data

