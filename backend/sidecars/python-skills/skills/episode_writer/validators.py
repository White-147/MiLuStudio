from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError

ALLOWED_DIALOGUE_DENSITIES = {"low", "balanced"}


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    plot_adaptation = unwrap_skill_data(payload, "plot_adaptation", "plot_adaptation", "episode_writer")
    preferences = payload.get("writing_preferences", {})
    errors: list[str] = []

    if preferences is None:
        preferences = {}

    if not isinstance(preferences, dict):
        errors.append("writing_preferences must be an object when provided.")
        preferences = {}

    dialogue_density = preferences.get("dialogue_density", "balanced")
    if dialogue_density not in ALLOWED_DIALOGUE_DENSITIES:
        errors.append("writing_preferences.dialogue_density must be low or balanced.")

    required_plot_fields = [
        "project_id",
        "language",
        "title",
        "logline",
        "target_duration_seconds",
        "aspect_ratio",
        "tone_keywords",
        "main_characters",
        "plot_beats",
        "ending_hook",
    ]

    for field in required_plot_fields:
        if field not in plot_adaptation:
            errors.append(f"plot_adaptation.data.{field} is required.")

    if "plot_beats" in plot_adaptation and len(plot_adaptation["plot_beats"]) != 4:
        errors.append("plot_adaptation.data.plot_beats must contain exactly 4 beats.")

    if errors:
        raise SkillValidationError("episode_writer input validation failed.", errors)

    return {
        "plot_adaptation": plot_adaptation,
        "writing_preferences": {
            "dialogue_density": dialogue_density,
        },
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
        "summary",
        "script_text",
        "segments",
        "subtitle_cues",
        "voice_notes",
        "review",
        "checkpoint",
    ]

    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in episode_writer output.")

    segments = data.get("segments")
    if not isinstance(segments, list) or len(segments) < 4:
        errors.append("segments must contain at least 4 script segments.")
    else:
        total_seconds = sum(segment.get("duration_seconds", 0) for segment in segments if isinstance(segment, dict))
        target_seconds = data.get("target_duration_seconds")
        if isinstance(target_seconds, int) and total_seconds != target_seconds:
            errors.append("segments duration_seconds must sum to target_duration_seconds.")

        has_narration = any(isinstance(segment, dict) and segment.get("narration") for segment in segments)
        has_dialogue = any(
            isinstance(segment, dict) and segment.get("dialogue_lines")
            for segment in segments
        )
        if not has_narration:
            errors.append("segments must include narration.")
        if not has_dialogue:
            errors.append("segments must include at least one dialogue line.")

    subtitle_cues = data.get("subtitle_cues")
    if not isinstance(subtitle_cues, list) or len(subtitle_cues) < 4:
        errors.append("subtitle_cues must contain at least 4 cues.")
    elif any(len(str(cue.get("text", ""))) > 32 for cue in subtitle_cues if isinstance(cue, dict)):
        errors.append("subtitle cue text should stay short enough for mobile captions.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not True:
        errors.append("episode_writer checkpoint.required must be true.")

    if errors:
        raise SkillValidationError("episode_writer output validation failed.", errors)

    return data
