from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    auto_editor = unwrap_skill_data(payload, "auto_editor", "auto_editor", "export_packager")
    quality_checker = unwrap_skill_data(payload, "quality_checker", "quality_checker", "export_packager")
    subtitle_generator = unwrap_skill_data(payload, "subtitle_generator", "subtitle_generator", "export_packager")
    video_generation = unwrap_skill_data(payload, "video_generation", "video_generation", "export_packager")

    normalized = {
        "auto_editor": auto_editor,
        "quality_checker": quality_checker,
        "subtitle_generator": subtitle_generator,
        "video_generation": video_generation,
    }
    errors: list[str] = []

    for field in ["project_id", "episode_index", "title", "language", "timeline", "render_plan"]:
        if field not in auto_editor:
            errors.append(f"auto_editor.data.{field} is required.")

    for field in ["quality_status", "quality_manifest"]:
        if field not in quality_checker:
            errors.append(f"quality_checker.data.{field} is required.")

    if not isinstance(subtitle_generator.get("subtitle_cues"), list):
        errors.append("subtitle_generator.data.subtitle_cues must be an array.")

    if not isinstance(video_generation.get("clips"), list):
        errors.append("video_generation.data.clips must be an array.")

    project_id = auto_editor.get("project_id")
    episode_index = auto_editor.get("episode_index")
    for upstream_name, upstream_data in normalized.items():
        if upstream_data.get("project_id") != project_id:
            errors.append(f"auto_editor.data.project_id must match {upstream_name}.data.project_id.")
        if upstream_data.get("episode_index") != episode_index:
            errors.append(f"auto_editor.data.episode_index must match {upstream_name}.data.episode_index.")

    if errors:
        raise SkillValidationError("export_packager input validation failed.", errors)

    return normalized


def validate_output(data: dict[str, Any]) -> dict[str, Any]:
    errors: list[str] = []
    for field in [
        "project_id",
        "episode_index",
        "title",
        "language",
        "provider",
        "model",
        "mode",
        "package_id",
        "quality_status",
        "delivery_assets",
        "export_manifest",
        "review",
        "checkpoint",
    ]:
        if field not in data:
            errors.append(f"{field} is required in export_packager output.")

    if data.get("provider") != "none" or data.get("model") != "none":
        errors.append("export_packager must keep provider=none and model=none in Stage 13.")

    assets = data.get("delivery_assets")
    if not isinstance(assets, list) or not assets:
        errors.append("delivery_assets must be a non-empty array.")
    else:
        for index, asset in enumerate(assets):
            if not isinstance(asset, dict):
                errors.append(f"delivery_assets[{index}] must be an object.")
                continue
            for field in ["asset_id", "kind", "label", "format", "logical_uri", "file_written"]:
                if field not in asset:
                    errors.append(f"delivery_assets[{index}].{field} is required.")
            if asset.get("file_written") is not False:
                errors.append(f"delivery_assets[{index}].file_written must be false.")

    manifest = data.get("export_manifest")
    if not isinstance(manifest, dict):
        errors.append("export_manifest must be an object.")
    else:
        for field in ["writes_files", "writes_database", "reads_media_files", "uses_ffmpeg"]:
            if manifest.get(field) is not False:
                errors.append(f"export_manifest.{field} must be false.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not False:
        errors.append("export_packager checkpoint.required must be false.")

    if errors:
        raise SkillValidationError("export_packager output validation failed.", errors)

    return data
