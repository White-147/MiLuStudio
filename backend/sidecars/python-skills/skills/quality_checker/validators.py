from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError

UPSTREAM_REQUIREMENTS: dict[str, list[str]] = {
    "character_bible": ["project_id", "episode_index", "title", "language", "characters"],
    "style_bible": ["project_id", "episode_index", "title", "language", "reusable_prompt_blocks"],
    "storyboard_director": [
        "project_id",
        "episode_index",
        "title",
        "language",
        "aspect_ratio",
        "shots",
        "timing_summary",
    ],
    "video_generation": ["project_id", "episode_index", "clips", "clip_manifest"],
    "voice_casting": ["project_id", "episode_index", "voice_tasks"],
    "subtitle_generator": ["project_id", "episode_index", "subtitle_cues", "srt_text"],
    "auto_editor": ["project_id", "episode_index", "timeline", "render_plan", "edit_manifest"],
}


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    character_bible = unwrap_skill_data(payload, "character_bible", "character_bible", "quality_checker")
    style_bible = unwrap_skill_data(payload, "style_bible", "style_bible", "quality_checker")
    storyboard = unwrap_skill_data(payload, "storyboard_director", "storyboard_director", "quality_checker")
    video_generation = unwrap_skill_data(payload, "video_generation", "video_generation", "quality_checker")
    voice_casting = unwrap_skill_data(payload, "voice_casting", "voice_casting", "quality_checker")
    subtitle_generator = unwrap_skill_data(payload, "subtitle_generator", "subtitle_generator", "quality_checker")
    auto_editor = unwrap_skill_data(payload, "auto_editor", "auto_editor", "quality_checker")

    normalized = {
        "character_bible": character_bible,
        "style_bible": style_bible,
        "storyboard_director": storyboard,
        "video_generation": video_generation,
        "voice_casting": voice_casting,
        "subtitle_generator": subtitle_generator,
        "auto_editor": auto_editor,
    }
    errors: list[str] = []

    for upstream_name, required_fields in UPSTREAM_REQUIREMENTS.items():
        upstream_data = normalized[upstream_name]
        for field in required_fields:
            if field not in upstream_data:
                errors.append(f"{upstream_name}.data.{field} is required.")

    project_id = storyboard.get("project_id")
    episode_index = storyboard.get("episode_index")
    for upstream_name, upstream_data in normalized.items():
        if upstream_data.get("project_id") != project_id:
            errors.append(f"storyboard_director.data.project_id must match {upstream_name}.data.project_id.")
        if upstream_data.get("episode_index") != episode_index:
            errors.append(f"storyboard_director.data.episode_index must match {upstream_name}.data.episode_index.")

    if not isinstance(character_bible.get("characters"), list) or not character_bible.get("characters"):
        errors.append("character_bible.data.characters must be a non-empty array.")

    if not isinstance(style_bible.get("reusable_prompt_blocks"), dict):
        errors.append("style_bible.data.reusable_prompt_blocks must be an object.")

    if not isinstance(storyboard.get("shots"), list) or not storyboard.get("shots"):
        errors.append("storyboard_director.data.shots must be a non-empty array.")

    if not isinstance(video_generation.get("clips"), list):
        errors.append("video_generation.data.clips must be an array.")

    if not isinstance(voice_casting.get("voice_tasks"), list):
        errors.append("voice_casting.data.voice_tasks must be an array.")

    if not isinstance(subtitle_generator.get("subtitle_cues"), list):
        errors.append("subtitle_generator.data.subtitle_cues must be an array.")

    timeline = auto_editor.get("timeline")
    if not isinstance(timeline, dict):
        errors.append("auto_editor.data.timeline must be an object.")
    elif not isinstance(timeline.get("tracks"), dict):
        errors.append("auto_editor.data.timeline.tracks must be an object.")

    if errors:
        raise SkillValidationError("quality_checker input validation failed.", errors)

    return normalized


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
        "quality_status",
        "score",
        "issues",
        "auto_retry_items",
        "manual_review_items",
        "quality_manifest",
        "generation_plan",
        "review",
        "checkpoint",
    ]
    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in quality_checker output.")

    if data.get("provider") != "none" or data.get("model") != "none":
        errors.append("quality_checker must keep provider=none and model=none in Stage 11.")

    if data.get("quality_status") not in {"passed", "passed_with_warnings", "needs_review", "blocked"}:
        errors.append("quality_status must be passed, passed_with_warnings, needs_review, or blocked.")

    for field in ["issues", "auto_retry_items", "manual_review_items"]:
        if not isinstance(data.get(field), list):
            errors.append(f"{field} must be an array.")

    manifest = data.get("quality_manifest")
    if not isinstance(manifest, dict):
        errors.append("quality_manifest must be an object.")
    else:
        assert_false_flags(
            manifest,
            [
                "writes_files",
                "writes_database",
                "reads_media_files",
                "uses_visual_detector",
                "uses_audio_detector",
                "uses_ffmpeg",
            ],
            "quality_manifest",
            errors,
        )

    generation_plan = data.get("generation_plan")
    if not isinstance(generation_plan, dict):
        errors.append("generation_plan must be an object.")
    else:
        if generation_plan.get("provider") != "none" or generation_plan.get("model") != "none":
            errors.append("generation_plan must keep provider=none and model=none.")
        assert_false_flags(
            generation_plan,
            [
                "writes_files",
                "writes_database",
                "reads_media_files",
                "uses_visual_detector",
                "uses_audio_detector",
                "uses_ffmpeg",
            ],
            "generation_plan",
            errors,
        )

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not True:
        errors.append("quality_checker checkpoint.required must be true.")

    if errors:
        raise SkillValidationError("quality_checker output validation failed.", errors)

    return data


def assert_false_flags(data: dict[str, Any], fields: list[str], context: str, errors: list[str]) -> None:
    for field in fields:
        if data.get(field) is not False:
            errors.append(f"{context}.{field} must be false.")
