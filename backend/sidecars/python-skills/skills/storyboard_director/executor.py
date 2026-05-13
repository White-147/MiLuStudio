from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output

SHOT_SIZE_SEQUENCE = ("extreme_close_up", "medium_close_up", "detail", "medium", "close_up", "wide", "medium_close_up", "hook_close_up")
CAMERA_MOTION_SEQUENCE = ("slow_push_in", "steady_follow", "brief_insert_push", "locked_reaction", "slow_pan", "hold", "tracking_in", "still_hook")


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    episode = normalized["episode_writer"]
    character_bible = normalized["character_bible"]
    style_bible = normalized["style_bible"]
    shots = build_shots(episode, character_bible, style_bible)

    data = {
        "project_id": episode["project_id"],
        "episode_index": episode["episode_index"],
        "title": episode["title"],
        "language": episode["language"],
        "target_duration_seconds": episode["target_duration_seconds"],
        "aspect_ratio": episode["aspect_ratio"],
        "storyboard_summary": build_storyboard_summary(episode, shots),
        "shots": shots,
        "timing_summary": build_timing_summary(episode, shots),
        "image_video_readiness": {
            "per_shot_prompt_inputs": [
                "shot.scene",
                "shot.characters",
                "shot.visual_action",
                "shot.camera",
                "shot.lighting",
                "shot.image_prompt_seed",
                "shot.video_prompt_seed",
            ],
            "requires_reference_assets": False,
            "deferred_assets": [
                "storyboard images",
                "first/last frames",
                "video clips",
            ],
            "notes": [
                "当前阶段只生成可审阅镜头列表，不生成图片、视频或文件资产。",
                "后续 image_prompt_builder / video_prompt_builder 应复用每个镜头的 prompt seed。",
            ],
        },
        "review": {
            "status": "ready_for_review",
            "editable_fields": ["shots", "timing_summary", "storyboard_summary"],
            "notes": [
                "用户可在此阶段审核镜头顺序、景别、运镜和时长。",
                "当前结果只产出分镜结构，不写数据库，不触发图像、视频或 FFmpeg。",
            ],
        },
        "checkpoint": {
            "required": True,
            "policy": "pause_for_storyboard_review",
            "reason": "storyboard_director output should be reviewed before image and video prompt expansion.",
        },
    }

    return validate_output(data)


def build_shots(
    episode: dict[str, Any],
    character_bible: dict[str, Any],
    style_bible: dict[str, Any],
) -> list[dict[str, Any]]:
    characters_by_name = {
        character["name"]: character
        for character in character_bible.get("characters", [])
        if isinstance(character, dict) and character.get("name")
    }
    fallback_character = next(iter(characters_by_name.values()), None)
    prompt_blocks = style_bible.get("reusable_prompt_blocks", {})
    style_refs = ["base_style", "character_consistency", "environment", "camera", "quality_guardrails"]
    shots: list[dict[str, Any]] = []

    for segment in episode["segments"]:
        segment_shots = split_segment_into_shots(segment)
        for local_index, shot_plan in enumerate(segment_shots):
            shot_index = len(shots)
            character_refs = resolve_characters(segment, characters_by_name, fallback_character)
            shot = build_shot(
                episode=episode,
                segment=segment,
                character_refs=character_refs,
                style_bible=style_bible,
                prompt_blocks=prompt_blocks,
                style_refs=style_refs,
                shot_index=shot_index,
                local_index=local_index,
                start_second=shot_plan["start_second"],
                duration_seconds=shot_plan["duration_seconds"],
            )
            shots.append(shot)

    return shots


def split_segment_into_shots(segment: dict[str, Any]) -> list[dict[str, int]]:
    start = int(segment["start_second"])
    duration = int(segment["duration_seconds"])

    if duration < 6:
        return [{"start_second": start, "duration_seconds": duration}]

    first_duration = max(3, min(duration - 3, duration // 2))
    second_duration = duration - first_duration

    return [
        {"start_second": start, "duration_seconds": first_duration},
        {"start_second": start + first_duration, "duration_seconds": second_duration},
    ]


def build_shot(
    episode: dict[str, Any],
    segment: dict[str, Any],
    character_refs: list[dict[str, str]],
    style_bible: dict[str, Any],
    prompt_blocks: dict[str, str],
    style_refs: list[str],
    shot_index: int,
    local_index: int,
    start_second: int,
    duration_seconds: int,
) -> dict[str, Any]:
    shot_number = shot_index + 1
    is_first_half = local_index == 0
    dialogue = collect_dialogue(segment, include_first=is_first_half)
    narration = segment.get("narration", "") if is_first_half else ""
    shot_size = SHOT_SIZE_SEQUENCE[shot_index % len(SHOT_SIZE_SEQUENCE)]
    camera_motion = CAMERA_MOTION_SEQUENCE[shot_index % len(CAMERA_MOTION_SEQUENCE)]
    scene = build_scene(segment, style_bible, is_first_half)

    return {
        "shot_id": f"shot_{shot_number:02d}",
        "shot_index": shot_number,
        "source_segment_id": segment["segment_id"],
        "source_beat_id": segment.get("beat_id", ""),
        "start_second": start_second,
        "duration_seconds": duration_seconds,
        "scene": scene,
        "characters": character_refs,
        "shot_size": shot_size,
        "camera": {
            "angle": build_camera_angle(shot_index, is_first_half),
            "motion": camera_motion,
            "composition": build_composition(style_bible, is_first_half),
            "safety": "keep subtitle lower-third clear and preserve character face readability",
        },
        "lighting": build_lighting(style_bible, shot_index),
        "visual_action": build_visual_action(segment, is_first_half),
        "dialogue": dialogue,
        "narration": narration,
        "sound_note": segment.get("sound_note", ""),
        "style_prompt_block_refs": style_refs,
        "continuity_notes": build_continuity_notes(character_refs, style_bible),
        "image_prompt_seed": build_image_prompt_seed(segment, character_refs, shot_size, prompt_blocks, scene),
        "video_prompt_seed": build_video_prompt_seed(segment, camera_motion, duration_seconds, prompt_blocks),
        "review_flags": build_review_flags(segment, duration_seconds, dialogue, narration),
    }


def build_scene(segment: dict[str, Any], style_bible: dict[str, Any], is_first_half: bool) -> str:
    locations = style_bible.get("environment_design", {}).get("primary_locations", [])
    location = locations[0] if locations else "脚本指定场景"
    focus = "线索和空间关系" if is_first_half else "角色反应和下一步行动"
    return f"{location}；{focus}；来源：{segment.get('purpose', '')}"


def resolve_characters(
    segment: dict[str, Any],
    characters_by_name: dict[str, dict[str, Any]],
    fallback_character: dict[str, Any] | None,
) -> list[dict[str, str]]:
    names: list[str] = []

    for dialogue in segment.get("dialogue_lines", []):
        speaker = dialogue.get("speaker")
        if isinstance(speaker, str) and speaker and speaker not in names:
            names.append(speaker)

    for character_name in characters_by_name:
        if character_name in str(segment.get("narration", "")) or character_name in str(segment.get("visual_direction", "")):
            if character_name not in names:
                names.append(character_name)

    if not names and fallback_character:
        names.append(fallback_character["name"])

    refs: list[dict[str, str]] = []
    for name in names[:3]:
        character = characters_by_name.get(name)
        if not character:
            continue
        refs.append(
            {
                "character_id": character["character_id"],
                "name": character["name"],
                "role_type": character.get("role_type", "supporting"),
                "stable_seed": character["stable_seed"],
            }
        )

    return refs


def collect_dialogue(segment: dict[str, Any], include_first: bool) -> list[dict[str, str]]:
    lines = segment.get("dialogue_lines", [])
    if not lines:
        return []

    selected = lines[:1] if include_first else lines[1:2]
    if not selected and not include_first:
        selected = lines[:1]

    return [
        {
            "speaker": str(dialogue.get("speaker", "")),
            "line": str(dialogue.get("line", "")),
            "delivery": str(dialogue.get("delivery", "")),
        }
        for dialogue in selected
    ]


def build_camera_angle(shot_index: int, is_first_half: bool) -> str:
    if shot_index == 0:
        return "low detail angle focused on the clue"

    if is_first_half:
        return "eye-level angle that keeps the environment readable"

    return "slight close reaction angle on the main character"


def build_composition(style_bible: dict[str, Any], is_first_half: bool) -> str:
    rules = style_bible.get("camera_language", {}).get("composition_rules", [])
    base_rule = rules[0] if rules else "竖屏安全区内保留字幕空间。"
    if is_first_half:
        return f"{base_rule} 线索放在画面三分之一交点。"

    return f"{base_rule} 主角眼神方向引导到下一镜。"


def build_lighting(style_bible: dict[str, Any], shot_index: int) -> dict[str, str]:
    lighting = style_bible.get("lighting", {})
    rules = lighting.get("rules", [])
    return {
        "key_light": str(lighting.get("key_light", "低强度环境光。")),
        "accent_light": str(lighting.get("accent_light", "线索提示光。")),
        "rule": str(rules[shot_index % len(rules)] if rules else "角色脸部保持可读。"),
    }


def build_visual_action(segment: dict[str, Any], is_first_half: bool) -> str:
    direction = str(segment.get("visual_direction", ""))
    if is_first_half:
        return f"建立镜头：{direction}"

    dialogue_lines = segment.get("dialogue_lines", [])
    if dialogue_lines:
        line = dialogue_lines[0].get("line", "")
        return f"反应镜头：角色完成对白“{line}”并推进下一动作。"

    return f"推进镜头：延续 {segment.get('purpose', '')}，把信息点收束到下一段。"


def build_continuity_notes(character_refs: list[dict[str, str]], style_bible: dict[str, Any]) -> list[str]:
    notes = [
        "本镜头必须复用 style_bible.reusable_prompt_blocks。",
        "同一角色跨镜头保持 stable_seed、服装主色和脸型一致。",
    ]
    if character_refs:
        ids = "、".join(character["character_id"] for character in character_refs)
        notes.append(f"角色锁定引用：{ids}。")

    style_notes = style_bible.get("continuity_notes", [])
    notes.extend(style_notes[:1])
    return notes


def build_image_prompt_seed(
    segment: dict[str, Any],
    character_refs: list[dict[str, str]],
    shot_size: str,
    prompt_blocks: dict[str, str],
    scene: str,
) -> str:
    character_names = ", ".join(character["name"] for character in character_refs) or "no visible character"
    return " | ".join(
        [
            prompt_blocks.get("base_style", ""),
            prompt_blocks.get("character_consistency", ""),
            f"shot_size={shot_size}",
            f"characters={character_names}",
            f"scene={scene}",
            f"story_purpose={segment.get('purpose', '')}",
        ]
    ).strip(" |")


def build_video_prompt_seed(
    segment: dict[str, Any],
    camera_motion: str,
    duration_seconds: int,
    prompt_blocks: dict[str, str],
) -> str:
    return " | ".join(
        [
            prompt_blocks.get("base_style", ""),
            prompt_blocks.get("camera", ""),
            f"duration_seconds={duration_seconds}",
            f"camera_motion={camera_motion}",
            f"visual_direction={segment.get('visual_direction', '')}",
        ]
    ).strip(" |")


def build_review_flags(
    segment: dict[str, Any],
    duration_seconds: int,
    dialogue: list[dict[str, str]],
    narration: str,
) -> list[str]:
    flags = []
    if duration_seconds < 3:
        flags.append("duration_too_short_for_review")
    if not dialogue and not narration:
        flags.append("silent_visual_beat")
    if not segment.get("visual_direction"):
        flags.append("missing_visual_direction")
    return flags


def build_storyboard_summary(episode: dict[str, Any], shots: list[dict[str, Any]]) -> str:
    return (
        f"《{episode['title']}》拆分为 {len(shots)} 个竖屏分镜，"
        f"总时长 {sum(shot['duration_seconds'] for shot in shots)} 秒，"
        "每镜头保留角色、景别、运镜、灯光、对白/旁白和 prompt seed。"
    )


def build_timing_summary(episode: dict[str, Any], shots: list[dict[str, Any]]) -> dict[str, Any]:
    total_seconds = sum(shot["duration_seconds"] for shot in shots)
    return {
        "target_duration_seconds": episode["target_duration_seconds"],
        "total_shot_seconds": total_seconds,
        "shot_count": len(shots),
        "min_shot_seconds": min(shot["duration_seconds"] for shot in shots),
        "max_shot_seconds": max(shot["duration_seconds"] for shot in shots),
        "within_tolerance": abs(total_seconds - episode["target_duration_seconds"]) <= 1,
    }

