from __future__ import annotations

from typing import Any

from milu_studio_skills.errors import SkillValidationError

ALLOWED_ASPECT_RATIOS = {"9:16", "16:9", "1:1"}
ALLOWED_MODES = {"fast", "director"}


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []

    story_text = payload.get("story_text")
    if not isinstance(story_text, str) or not story_text.strip():
        errors.append("story_text is required and must be a non-empty string.")

    target_duration = payload.get("target_duration_seconds", 45)
    if not isinstance(target_duration, int) or target_duration < 30 or target_duration > 60:
        errors.append("target_duration_seconds must be an integer between 30 and 60.")

    aspect_ratio = payload.get("aspect_ratio", "9:16")
    if aspect_ratio not in ALLOWED_ASPECT_RATIOS:
        errors.append("aspect_ratio must be one of 9:16, 16:9, or 1:1.")

    mode = payload.get("mode", "director")
    if mode not in ALLOWED_MODES:
        errors.append("mode must be fast or director.")

    language = payload.get("language", "zh-CN")
    if not isinstance(language, str) or not language.strip():
        errors.append("language must be a non-empty string.")

    style_preset = payload.get("style_preset", "轻写实国漫")
    if not isinstance(style_preset, str) or not style_preset.strip():
        errors.append("style_preset must be a non-empty string.")

    if errors:
        raise SkillValidationError("story_intake input validation failed.", errors)

    return {
        "project_id": string_or_default(payload.get("project_id"), "unknown-project"),
        "story_text": story_text.strip(),
        "language": language.strip(),
        "target_duration_seconds": target_duration,
        "aspect_ratio": aspect_ratio,
        "style_preset": style_preset.strip(),
        "mode": mode,
    }


def validate_output(data: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []

    required_fields = [
        "project_id",
        "summary",
        "logline",
        "genres",
        "tone_keywords",
        "main_characters",
        "story_beats",
        "target_duration_seconds",
        "recommended_shot_count",
        "risks",
        "checkpoint",
    ]

    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in story_intake output.")

    if "story_beats" in data and len(data["story_beats"]) != 3:
        errors.append("story_beats must contain exactly 3 beats.")

    if errors:
        raise SkillValidationError("story_intake output validation failed.", errors)

    return data


def string_or_default(value: Any, default: str) -> str:
    if isinstance(value, str) and value.strip():
        return value.strip()

    return default
