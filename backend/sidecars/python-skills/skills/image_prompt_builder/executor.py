from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    storyboard = normalized["storyboard_director"]
    character_bible = normalized["character_bible"]
    style_bible = normalized["style_bible"]
    image_requests = build_image_requests(storyboard, character_bible, style_bible)

    data = {
        "project_id": storyboard["project_id"],
        "episode_index": storyboard["episode_index"],
        "title": storyboard["title"],
        "language": storyboard["language"],
        "aspect_ratio": storyboard["aspect_ratio"],
        "prompt_set_id": f"{storyboard['project_id']}:episode_{storyboard['episode_index']}:image_prompts",
        "image_requests": image_requests,
        "negative_prompt": merge_negative_prompt(style_bible),
        "reference_strategy": build_reference_strategy(character_bible, style_bible),
        "generation_plan": {
            "mode": "mock_ready",
            "provider": "none",
            "model": "none",
            "request_count": len(image_requests),
            "writes_files": False,
            "writes_database": False,
            "notes": [
                "Stage 8 only builds image prompt requests and does not call an image model.",
                "Later image_generation can consume these requests through mock or real provider adapters.",
            ],
        },
        "review": {
            "status": "ready_for_generation_boundary",
            "editable_fields": ["image_requests", "negative_prompt", "reference_strategy"],
            "notes": [
                "Reviewed storyboard data can move into image generation through these requests.",
                "This result writes no database rows, no files, uploads no references, and calls no image model.",
            ],
        },
        "checkpoint": {
            "required": False,
            "policy": "auto_continue_to_mock_image_generation",
            "reason": "image prompts can continue to mock image generation without user review in Stage 8.",
        },
    }

    return validate_output(data)


def build_image_requests(
    storyboard: dict[str, Any],
    character_bible: dict[str, Any],
    style_bible: dict[str, Any],
) -> list[dict[str, Any]]:
    requests: list[dict[str, Any]] = []
    characters = character_bible.get("characters", [])
    prompt_blocks = style_bible.get("reusable_prompt_blocks", {})
    negative_prompt = merge_negative_prompt(style_bible)

    for character in characters:
        requests.append(build_character_reference_request(storyboard, character, prompt_blocks, negative_prompt))

    for shot in storyboard["shots"]:
        for asset_type, frame_role in [
            ("storyboard_image", "storyboard"),
            ("first_frame", "first_frame"),
            ("last_frame", "last_frame"),
        ]:
            requests.append(build_shot_request(storyboard, shot, style_bible, negative_prompt, asset_type, frame_role))

    return requests


def build_character_reference_request(
    storyboard: dict[str, Any],
    character: dict[str, Any],
    prompt_blocks: dict[str, str],
    negative_prompt: list[str],
) -> dict[str, Any]:
    appearance = character.get("appearance", {})
    costume = character.get("costume", {})
    visual_identity = character.get("visual_identity", {})
    must_keep = " / ".join(visual_identity.get("must_keep", [])[:3])
    positive_prompt = " | ".join(
        part
        for part in [
            prompt_blocks.get("base_style", ""),
            prompt_blocks.get("character_consistency", ""),
            f"single character reference sheet for {character['name']} ({character['character_id']})",
            f"role={character.get('role_type', '')}",
            f"appearance={appearance.get('signature_detail', '')}",
            f"costume_lock={costume.get('lock_rule', '')}",
            f"must_keep={must_keep}",
            "clean full-body standing pose, front view, neutral expression, plain background",
        ]
        if part
    )

    return {
        "request_id": f"imgreq_character_{character['character_id']}",
        "asset_type": "character_reference",
        "shot_id": None,
        "character_id": character["character_id"],
        "character_ids": [character["character_id"]],
        "prompt": positive_prompt,
        "negative_prompt": negative_prompt,
        "aspect_ratio": "1:1",
        "style_refs": ["base_style", "character_consistency", "quality_guardrails"],
        "continuity_refs": visual_identity.get("must_keep", []),
        "seed_hint": f"{character['stable_seed']}:character_reference",
        "output_slot": {
            "kind": "image",
            "intended_use": "character_reference",
            "selected": True,
        },
    }


def build_shot_request(
    storyboard: dict[str, Any],
    shot: dict[str, Any],
    style_bible: dict[str, Any],
    negative_prompt: list[str],
    asset_type: str,
    frame_role: str,
) -> dict[str, Any]:
    character_ids = [
        character["character_id"]
        for character in shot.get("characters", [])
        if isinstance(character, dict) and character.get("character_id")
    ]
    frame_instruction = build_frame_instruction(frame_role, shot)
    positive_prompt = " | ".join(
        part
        for part in [
            shot.get("image_prompt_seed", ""),
            f"frame_role={frame_role}",
            f"visual_action={shot.get('visual_action', '')}",
            f"camera_angle={shot.get('camera', {}).get('angle', '')}",
            f"camera_motion_hint={shot.get('camera', {}).get('motion', '')}",
            f"lighting={shot.get('lighting', {}).get('rule', '')}",
            frame_instruction,
            style_bible.get("reusable_prompt_blocks", {}).get("quality_guardrails", ""),
        ]
        if part
    )

    return {
        "request_id": f"imgreq_{shot['shot_id']}_{asset_type}",
        "asset_type": asset_type,
        "shot_id": shot["shot_id"],
        "character_id": None,
        "character_ids": character_ids,
        "prompt": positive_prompt,
        "negative_prompt": negative_prompt,
        "aspect_ratio": storyboard["aspect_ratio"],
        "style_refs": shot.get("style_prompt_block_refs", []),
        "continuity_refs": shot.get("continuity_notes", []),
        "seed_hint": f"{storyboard['project_id']}:episode_{storyboard['episode_index']}:{shot['shot_id']}:{asset_type}",
        "output_slot": {
            "kind": "image",
            "intended_use": asset_type,
            "selected": asset_type in {"storyboard_image", "first_frame"},
        },
    }


def build_frame_instruction(frame_role: str, shot: dict[str, Any]) -> str:
    if frame_role == "first_frame":
        return "choose the earliest readable moment of this shot, before the main movement completes"

    if frame_role == "last_frame":
        return "choose the final readable moment of this shot, preserving continuity for image-to-video"

    return f"single storyboard keyframe for shot timing {shot.get('start_second', 0)}s"


def merge_negative_prompt(style_bible: dict[str, Any]) -> list[str]:
    values = []
    for value in style_bible.get("negative_prompt", []):
        if isinstance(value, str) and value not in values:
            values.append(value)

    for value in [
        "unreadable face",
        "random extra character",
        "wrong aspect ratio",
        "cropped subtitle safe area",
        "inconsistent costume",
    ]:
        if value not in values:
            values.append(value)

    return values


def build_reference_strategy(character_bible: dict[str, Any], style_bible: dict[str, Any]) -> dict[str, Any]:
    return {
        "requires_uploaded_reference": False,
        "character_reference_source": "character_bible",
        "style_reference_source": "style_bible.reusable_prompt_blocks",
        "character_ids": [
            character["character_id"]
            for character in character_bible.get("characters", [])
            if isinstance(character, dict) and character.get("character_id")
        ],
        "style_name": style_bible.get("style_name", ""),
        "notes": [
            "This stage uploads no reference images and only emits request structures for later image stages.",
            "Real reference images, turnarounds, and portrait close-ups stay deferred to asset provider stages.",
        ],
    }
