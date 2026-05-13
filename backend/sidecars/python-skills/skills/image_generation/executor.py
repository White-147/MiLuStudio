from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    image_prompt_builder = normalized["image_prompt_builder"]
    assets = build_mock_assets(image_prompt_builder)

    data = {
        "project_id": image_prompt_builder["project_id"],
        "episode_index": image_prompt_builder["episode_index"],
        "title": image_prompt_builder["title"],
        "language": image_prompt_builder["language"],
        "provider": "mock",
        "model": "none",
        "mode": "deterministic-placeholder",
        "assets": assets,
        "asset_manifest": build_asset_manifest(image_prompt_builder, assets),
        "cost_estimate": {
            "provider": "mock",
            "currency": "USD",
            "estimated_cost": 0,
            "actual_cost": 0,
        },
        "review": {
            "status": "ready_for_image_review",
            "editable_fields": ["assets", "asset_manifest"],
            "notes": [
                "Stage 8 emits mock placeholder asset records, not real image files.",
                "Real selection, retry, persistence, and provider work stay behind later Control API and adapter stages.",
            ],
        },
        "checkpoint": {
            "required": True,
            "policy": "pause_for_image_review",
            "reason": "image_generation output represents the image review boundary, even when assets are mock placeholders.",
        },
    }

    return validate_output(data)


def build_mock_assets(image_prompt_builder: dict[str, Any]) -> list[dict[str, Any]]:
    assets = []
    project_id = image_prompt_builder["project_id"]
    episode_index = image_prompt_builder["episode_index"]

    for index, request in enumerate(image_prompt_builder["image_requests"], start=1):
        asset_id = f"mock_image_{episode_index:02d}_{index:03d}"
        assets.append(
            {
                "asset_id": asset_id,
                "request_id": request["request_id"],
                "asset_type": request["asset_type"],
                "shot_id": request.get("shot_id"),
                "character_id": request.get("character_id"),
                "character_ids": request.get("character_ids", []),
                "status": "mock_ready",
                "provider": "mock",
                "model": "none",
                "prompt": request["prompt"],
                "negative_prompt": request["negative_prompt"],
                "aspect_ratio": request["aspect_ratio"],
                "selected": bool(request.get("output_slot", {}).get("selected", False)),
                "file_written": False,
                "asset_uri": f"milu://mock-assets/{project_id}/{asset_id}.png",
                "storage_intent": {
                    "root": "D:\\code\\MiLuStudio\\storage",
                    "relative_path": f"projects/{project_id}/images/{asset_id}.png",
                    "will_write_in_future_adapter": True,
                },
                "metadata": {
                    "seed_hint": request["seed_hint"],
                    "kind": request.get("output_slot", {}).get("kind", "image"),
                    "intended_use": request.get("output_slot", {}).get("intended_use", request["asset_type"]),
                    "source": "deterministic_mock_no_file",
                },
            }
        )

    return assets


def build_asset_manifest(image_prompt_builder: dict[str, Any], assets: list[dict[str, Any]]) -> dict[str, Any]:
    by_shot: dict[str, list[str]] = {}
    character_references: list[str] = []

    for asset in assets:
        shot_id = asset.get("shot_id")
        if shot_id:
            by_shot.setdefault(shot_id, []).append(asset["asset_id"])
        if asset.get("asset_type") == "character_reference":
            character_references.append(asset["asset_id"])

    return {
        "prompt_set_id": image_prompt_builder["prompt_set_id"],
        "asset_count": len(assets),
        "by_shot": by_shot,
        "character_references": character_references,
        "writes_files": False,
        "writes_database": False,
        "notes": [
            "asset_uri is a logical placeholder and does not mean a real file exists.",
            "storage_intent describes a later adapter target and does not write to the file system in this stage.",
        ],
    }
