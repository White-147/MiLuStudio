from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError

ALLOWED_ENDING_STYLES = {"open_hook", "closed"}
ALLOWED_NARRATIVE_POVS = {"protagonist", "observer"}


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    story_intake = unwrap_skill_data(payload, "story_intake", "story_intake", "plot_adaptation")
    preferences = payload.get("adaptation_preferences", {})
    errors: list[str] = []

    if preferences is None:
        preferences = {}

    if not isinstance(preferences, dict):
        errors.append("adaptation_preferences must be an object when provided.")
        preferences = {}

    narrative_pov = preferences.get("narrative_pov", "protagonist")
    if narrative_pov not in ALLOWED_NARRATIVE_POVS:
        errors.append("adaptation_preferences.narrative_pov must be protagonist or observer.")

    ending_style = preferences.get("ending_style", "open_hook")
    if ending_style not in ALLOWED_ENDING_STYLES:
        errors.append("adaptation_preferences.ending_style must be open_hook or closed.")

    required_intake_fields = [
        "project_id",
        "language",
        "summary",
        "logline",
        "genres",
        "tone_keywords",
        "style_preset",
        "mode",
        "target_duration_seconds",
        "aspect_ratio",
        "recommended_shot_count",
        "main_characters",
        "story_beats",
        "risks",
        "source_word_count",
    ]

    for field in required_intake_fields:
        if field not in story_intake:
            errors.append(f"story_intake.data.{field} is required.")

    if "story_beats" in story_intake and len(story_intake["story_beats"]) != 3:
        errors.append("story_intake.data.story_beats must contain exactly 3 beats.")

    if errors:
        raise SkillValidationError("plot_adaptation input validation failed.", errors)

    return {
        "story_intake": story_intake,
        "adaptation_preferences": {
            "narrative_pov": narrative_pov,
            "ending_style": ending_style,
        },
    }


def validate_output(data: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []
    required_fields = [
        "project_id",
        "language",
        "title",
        "logline",
        "target_duration_seconds",
        "aspect_ratio",
        "mode",
        "style_preset",
        "genre_tags",
        "tone_keywords",
        "main_characters",
        "adaptation_strategy",
        "plot_beats",
        "core_conflict",
        "turning_point",
        "ending_hook",
        "continuity_notes",
        "review_points",
        "risks",
        "checkpoint",
    ]

    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in plot_adaptation output.")

    beats = data.get("plot_beats")
    if not isinstance(beats, list) or len(beats) != 4:
        errors.append("plot_beats must contain exactly 4 beats.")
    else:
        total_seconds = sum(beat.get("estimated_seconds", 0) for beat in beats if isinstance(beat, dict))
        target_seconds = data.get("target_duration_seconds")
        if isinstance(target_seconds, int) and abs(total_seconds - target_seconds) > 2:
            errors.append("plot_beats estimated_seconds must sum close to target_duration_seconds.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or "required" not in checkpoint:
        errors.append("checkpoint.required is required.")

    if errors:
        raise SkillValidationError("plot_adaptation output validation failed.", errors)

    return data
