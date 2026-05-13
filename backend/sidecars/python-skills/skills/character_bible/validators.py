from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    episode_writer = unwrap_skill_data(payload, "episode_writer", "episode_writer", "character_bible")
    preferences = payload.get("character_preferences", {})
    errors: list[str] = []

    if preferences is None:
        preferences = {}

    if not isinstance(preferences, dict):
        errors.append("character_preferences must be an object when provided.")
        preferences = {}

    locked_names = preferences.get("locked_character_names", [])
    if locked_names is None:
        locked_names = []
    if not isinstance(locked_names, list) or any(not isinstance(name, str) for name in locked_names):
        errors.append("character_preferences.locked_character_names must be an array of strings when provided.")
        locked_names = []

    required_episode_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "summary",
        "script_text",
        "segments",
    ]

    for field in required_episode_fields:
        if field not in episode_writer:
            errors.append(f"episode_writer.data.{field} is required.")

    segments = episode_writer.get("segments")
    if not isinstance(segments, list) or not segments:
        errors.append("episode_writer.data.segments must be a non-empty array.")

    if errors:
        raise SkillValidationError("character_bible input validation failed.", errors)

    return {
        "episode_writer": episode_writer,
        "character_preferences": {
            "locked_character_names": [name.strip() for name in locked_names if name.strip()],
        },
    }


def validate_output(data: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []
    required_fields = [
        "project_id",
        "episode_index",
        "title",
        "language",
        "source_summary",
        "characters",
        "relationship_notes",
        "continuity_rules",
        "review",
        "checkpoint",
    ]

    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in character_bible output.")

    characters = data.get("characters")
    if not isinstance(characters, list) or not characters:
        errors.append("characters must contain at least one character.")
    else:
        if not any(isinstance(character, dict) and character.get("role_type") == "main" for character in characters):
            errors.append("characters must include one main character.")

        for index, character in enumerate(characters):
            if not isinstance(character, dict):
                errors.append(f"characters[{index}] must be an object.")
                continue

            for field in [
                "character_id",
                "name",
                "role_type",
                "stable_seed",
                "identity",
                "personality",
                "appearance",
                "costume",
                "voice_profile",
                "visual_identity",
                "continuity_rules",
                "source_evidence",
            ]:
                if field not in character:
                    errors.append(f"characters[{index}].{field} is required.")

            if not isinstance(character.get("visual_identity"), dict) or not character.get("visual_identity", {}).get("must_keep"):
                errors.append(f"characters[{index}].visual_identity.must_keep must not be empty.")

    if not isinstance(data.get("continuity_rules"), list) or not data.get("continuity_rules"):
        errors.append("continuity_rules must not be empty.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not True:
        errors.append("character_bible checkpoint.required must be true.")

    if errors:
        raise SkillValidationError("character_bible output validation failed.", errors)

    return data

