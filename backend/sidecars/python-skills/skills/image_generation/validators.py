from __future__ import annotations

from typing import Any

from milu_studio_skills.contracts import unwrap_skill_data
from milu_studio_skills.errors import SkillValidationError


def validate_input(payload: dict[str, Any]) -> dict[str, Any]:
    image_prompt_builder = unwrap_skill_data(payload, "image_prompt_builder", "image_prompt_builder", "image_generation")
    errors: list[str] = []

    required_fields = ["project_id", "episode_index", "title", "language", "prompt_set_id", "image_requests"]
    for field in required_fields:
        if field not in image_prompt_builder:
            errors.append(f"image_prompt_builder.data.{field} is required.")

    image_requests = image_prompt_builder.get("image_requests")
    if not isinstance(image_requests, list) or not image_requests:
        errors.append("image_prompt_builder.data.image_requests must be a non-empty array.")
    else:
        for index, request in enumerate(image_requests):
            if not isinstance(request, dict):
                errors.append(f"image_prompt_builder.data.image_requests[{index}] must be an object.")
                continue
            for field in ["request_id", "asset_type", "prompt", "negative_prompt", "aspect_ratio", "seed_hint"]:
                if field not in request:
                    errors.append(f"image_prompt_builder.data.image_requests[{index}].{field} is required.")

    if errors:
        raise SkillValidationError("image_generation input validation failed.", errors)

    return {
        "image_prompt_builder": image_prompt_builder,
    }


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
        "assets",
        "asset_manifest",
        "cost_estimate",
        "review",
        "checkpoint",
    ]
    for field in required_fields:
        if field not in data:
            errors.append(f"{field} is required in image_generation output.")

    if data.get("provider") != "mock" or data.get("model") != "none":
        errors.append("image_generation must use provider=mock and model=none in Stage 8.")

    assets = data.get("assets")
    if not isinstance(assets, list) or not assets:
        errors.append("assets must be a non-empty array.")
    else:
        for index, asset in enumerate(assets):
            if not isinstance(asset, dict):
                errors.append(f"assets[{index}] must be an object.")
                continue
            for field in [
                "asset_id",
                "request_id",
                "asset_type",
                "status",
                "provider",
                "prompt",
                "asset_uri",
                "file_written",
                "storage_intent",
            ]:
                if field not in asset:
                    errors.append(f"assets[{index}].{field} is required.")
            if asset.get("file_written") is not False:
                errors.append(f"assets[{index}].file_written must be false in Stage 8.")

    manifest = data.get("asset_manifest")
    if not isinstance(manifest, dict) or manifest.get("writes_files") is not False or manifest.get("writes_database") is not False:
        errors.append("asset_manifest must explicitly avoid file and database writes.")

    checkpoint = data.get("checkpoint")
    if not isinstance(checkpoint, dict) or checkpoint.get("required") is not True:
        errors.append("image_generation checkpoint.required must be true.")

    if errors:
        raise SkillValidationError("image_generation output validation failed.", errors)

    return data

