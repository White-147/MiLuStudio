from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    storyboard = normalized["storyboard_director"]
    image_prompt_builder = normalized["image_prompt_builder"]
    image_generation = normalized["image_generation"]
    video_requests = build_video_requests(storyboard, image_prompt_builder, image_generation)

    data = {
        "project_id": storyboard["project_id"],
        "episode_index": storyboard["episode_index"],
        "title": storyboard["title"],
        "language": storyboard["language"],
        "aspect_ratio": storyboard["aspect_ratio"],
        "prompt_set_id": f"{storyboard['project_id']}:episode_{storyboard['episode_index']}:video_prompts",
        "video_requests": video_requests,
        "negative_prompt": merge_negative_prompt(image_prompt_builder),
        "source_asset_manifest": build_source_asset_manifest(image_generation, video_requests),
        "generation_plan": {
            "mode": "mock_ready",
            "provider": "none",
            "model": "none",
            "request_count": len(video_requests),
            "writes_files": False,
            "writes_database": False,
            "uses_ffmpeg": False,
            "notes": [
                "Stage 9 only builds video prompt requests and does not call a video model.",
                "Mock image assets are logical inputs; real files and adapters remain deferred.",
            ],
        },
        "review": {
            "status": "ready_for_video_generation_boundary",
            "editable_fields": ["video_requests", "negative_prompt", "source_asset_manifest"],
            "notes": [
                "Reviewed storyboard and mock image assets can move into mock video generation through these requests.",
                "This result writes no database rows, no files, calls no video model, and does not invoke FFmpeg.",
            ],
        },
        "checkpoint": {
            "required": False,
            "policy": "auto_continue_to_mock_video_generation",
            "reason": "video prompts can continue to mock video generation without user review in Stage 9.",
        },
    }

    return validate_output(data)


def build_video_requests(
    storyboard: dict[str, Any],
    image_prompt_builder: dict[str, Any],
    image_generation: dict[str, Any],
) -> list[dict[str, Any]]:
    assets_by_shot = index_assets_by_shot(image_generation.get("assets", []))
    character_reference_asset_ids = [
        asset["asset_id"]
        for asset in image_generation.get("assets", [])
        if asset.get("asset_type") == "character_reference"
    ]
    negative_prompt = merge_negative_prompt(image_prompt_builder)
    requests: list[dict[str, Any]] = []

    for shot in storyboard["shots"]:
        shot_assets = assets_by_shot.get(shot["shot_id"], {})
        source_images = build_source_images(shot_assets)
        generation_mode = "image_to_video" if has_frame_pair(shot_assets) else "text_to_video_fallback"
        requests.append(
            {
                "request_id": f"vidreq_{shot['shot_id']}_clip",
                "shot_id": shot["shot_id"],
                "shot_index": shot["shot_index"],
                "duration_seconds": shot["duration_seconds"],
                "aspect_ratio": storyboard["aspect_ratio"],
                "generation_mode": generation_mode,
                "prompt": build_prompt(shot, source_images, generation_mode),
                "negative_prompt": negative_prompt,
                "source_images": source_images,
                "character_reference_asset_ids": character_reference_asset_ids,
                "motion_plan": build_motion_plan(shot),
                "continuity_refs": shot.get("continuity_notes", []),
                "seed_hint": f"{storyboard['project_id']}:episode_{storyboard['episode_index']}:{shot['shot_id']}:video_clip",
                "output_slot": {
                    "kind": "video",
                    "intended_use": "video_clip",
                    "selected": True,
                },
            }
        )

    return requests


def index_assets_by_shot(assets: list[dict[str, Any]]) -> dict[str, dict[str, dict[str, Any]]]:
    by_shot: dict[str, dict[str, dict[str, Any]]] = {}

    for asset in assets:
        shot_id = asset.get("shot_id")
        asset_type = asset.get("asset_type")
        if not shot_id or not asset_type:
            continue
        by_shot.setdefault(shot_id, {})[asset_type] = asset

    return by_shot


def has_frame_pair(shot_assets: dict[str, dict[str, Any]]) -> bool:
    return "first_frame" in shot_assets and "last_frame" in shot_assets


def build_source_images(shot_assets: dict[str, dict[str, Any]]) -> list[dict[str, Any]]:
    source_images: list[dict[str, Any]] = []

    for role, asset_type in [
        ("storyboard", "storyboard_image"),
        ("first_frame", "first_frame"),
        ("last_frame", "last_frame"),
    ]:
        asset = shot_assets.get(asset_type)
        if not asset:
            continue
        source_images.append(
            {
                "role": role,
                "asset_id": asset["asset_id"],
                "asset_type": asset["asset_type"],
                "asset_uri": asset["asset_uri"],
                "file_written": asset["file_written"],
                "selected": asset.get("selected", False),
            }
        )

    return source_images


def build_prompt(shot: dict[str, Any], source_images: list[dict[str, Any]], generation_mode: str) -> str:
    source_roles = ", ".join(source_image["role"] for source_image in source_images)
    camera = shot.get("camera", {})

    return " | ".join(
        part
        for part in [
            shot.get("video_prompt_seed", ""),
            f"generation_mode={generation_mode}",
            f"duration={shot.get('duration_seconds', 0)}s",
            f"scene={shot.get('scene', '')}",
            f"visual_action={shot.get('visual_action', '')}",
            f"camera_motion={camera.get('motion', '')}",
            f"camera_angle={camera.get('angle', '')}",
            f"source_images={source_roles}",
            "keep character identity and costume stable across the whole clip",
            "no cuts inside this shot; preserve subtitle-safe lower third",
        ]
        if part
    )


def build_motion_plan(shot: dict[str, Any]) -> dict[str, Any]:
    camera = shot.get("camera", {})

    return {
        "camera_motion": camera.get("motion", "hold"),
        "camera_angle": camera.get("angle", ""),
        "composition": camera.get("composition", ""),
        "subject_motion": shot.get("visual_action", ""),
        "duration_seconds": shot.get("duration_seconds", 0),
        "transition_in": "cut_from_previous_shot",
        "transition_out": "cut_to_next_shot",
    }


def merge_negative_prompt(image_prompt_builder: dict[str, Any]) -> list[str]:
    values = []
    for value in image_prompt_builder.get("negative_prompt", []):
        if isinstance(value, str) and value not in values:
            values.append(value)

    for value in [
        "identity drift",
        "flicker",
        "warped motion",
        "jump cut inside shot",
        "uncontrolled camera shake",
        "burned-in subtitle text",
    ]:
        if value not in values:
            values.append(value)

    return values


def build_source_asset_manifest(
    image_generation: dict[str, Any],
    video_requests: list[dict[str, Any]],
) -> dict[str, Any]:
    by_shot = {
        request["shot_id"]: [source_image["asset_id"] for source_image in request["source_images"]]
        for request in video_requests
    }
    character_references = [
        asset["asset_id"]
        for asset in image_generation.get("assets", [])
        if asset.get("asset_type") == "character_reference"
    ]

    return {
        "image_asset_count": len(image_generation.get("assets", [])),
        "by_shot": by_shot,
        "character_references": character_references,
        "source_manifest_writes_files": bool(image_generation.get("asset_manifest", {}).get("writes_files")),
        "source_manifest_writes_database": bool(image_generation.get("asset_manifest", {}).get("writes_database")),
        "writes_files": False,
        "writes_database": False,
        "notes": [
            "All source image URIs are logical placeholders until a later asset adapter writes real files.",
            "Video prompt requests reference mock image assets but do not dereference or read them.",
        ],
    }
