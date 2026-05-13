from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    episode_writer = unwrap_skill_data(payload, "episode_writer", "episode_writer", "storyboard_director")
    character_bible = unwrap_skill_data(payload, "character_bible", "character_bible", "storyboard_director")
    style_bible = unwrap_skill_data(payload, "style_bible", "style_bible", "storyboard_director")
    errors: list[str] = []

    required_episode_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "target_duration_seconds",
        "aspect_ratio",
        "segments",
    ]
    for field in required_episode_fields:
        if field not in episode_writer:
            errors.append(f"episode_writer.data.{field} is required.")

    required_character_fields = ["project_id", "episode_index", "characters", "continuity_rules"]
    for field in required_character_fields:
        if field not in character_bible:
            errors.append(f"character_bible.data.{field} is required.")

    required_style_fields = [
        "project_id",
        "episode_index",
        "style_name",
        "lighting",
        "environment_design",
        "camera_language",
        "reusable_prompt_blocks",
        "negative_prompt",
    ]
    for field in required_style_fields:
        if field not in style_bible:
            errors.append(f"style_bible.data.{field} is required.")

    for upstream_name, upstream_data in [("character_bible", character_bible), ("style_bible", style_bible)]:
        if episode_writer.get("project_id") != upstream_data.get("project_id"):
            errors.append(f"episode_writer.data.project_id must match {upstream_name}.data.project_id.")
        if episode_writer.get("episode_index") != upstream_data.get("episode_index"):
            errors.append(f"episode_writer.data.episode_index must match {upstream_name}.data.episode_index.")

    segments = episode_writer.get("segments")
    if not isinstance(segments, list) or not segments:
        errors.append("episode_writer.data.segments must be a non-empty array.")
    else:
        for index, segment in enumerate(segments):
            if not isinstance(segment, dict):
                errors.append(f"episode_writer.data.segments[{index}] must be an object.")
                continue
            for field in ["segment_id", "start_second", "duration_seconds", "visual_direction", "dialogue_lines"]:
                if field not in segment:
                    errors.append(f"episode_writer.data.segments[{index}].{field} is required.")

    if not isinstance(character_bible.get("characters"), list) or not character_bible.get("characters"):
        errors.append("character_bible.data.characters must be a non-empty array.")

    reusable_prompt_blocks = style_bible.get("reusable_prompt_blocks")
    if not isinstance(reusable_prompt_blocks, dict) or not reusable_prompt_blocks.get("base_style"):
        errors.append("style_bible.data.reusable_prompt_blocks.base_style is required.")

    if errors:
        raise SkillValidationError("storyboard_director input validation failed.", errors)

    return {
        "episode_writer": episode_writer,
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
        "target_duration_seconds",
        "aspect_ratio",
        "storyboard_summary",
        "shots",
        "timing_summary",
        "image_video_readiness",
        "review",
        "checkpoint",
    ]
    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in storyboard_director output.")

    shots = data.get("shots")
    if not isinstance(shots, list) or not 6 <= len(shots) <= 12:
        errors.append("shots must contain 6 to 12 shots.")
    else:
        total_seconds = sum(shot.get("duration_seconds", 0) for shot in shots if isinstance(shot, dict))
        target_seconds = data.get("target_duration_seconds")
        if isinstance(target_seconds, int) and abs(total_seconds - target_seconds) > 1:
            errors.append("shots duration_seconds must stay within 1 second of target_duration_seconds.")

        for index, shot in enumerate(shots):
            if not isinstance(shot, dict):
                errors.append(f"shots[{index}] must be an object.")
                continue

            for field in [
                "shot_id",
                "shot_index",
                "source_segment_id",
                "start_second",
                "duration_seconds",
                "scene",
                "characters",
                "shot_size",
                "camera",
                "lighting",
                "visual_action",
                "dialogue",
                "narration",
                "style_prompt_block_refs",
                "image_prompt_seed",
                "video_prompt_seed",
            ]:
                if field not in shot:
                    errors.append(f"shots[{index}].{field} is required.")

            if not shot.get("dialogue") and not shot.get("narration") and not shot.get("visual_action"):
                errors.append(f"shots[{index}] must include dialogue, narration, or visual_action.")

    timing_summary = data.get("timing_summary")
    if not isinstance(timing_summary, dict) or timing_summary.get("within_tolerance") is not True:
        errors.append("timing_summary.within_tolerance must be true.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not True:
        errors.append("storyboard_director checkpoint.required must be true.")

    if errors:
        raise SkillValidationError("storyboard_director output validation failed.", errors)

    return data

