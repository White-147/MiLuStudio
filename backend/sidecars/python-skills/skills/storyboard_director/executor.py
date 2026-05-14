from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output

SHOT_SIZE_SEQUENCE = ("extreme_close_up", "medium_close_up", "detail", "medium", "close_up", "wide", "medium_close_up", "hook_close_up")
CAMERA_MOTION_SEQUENCE = ("slow_push_in", "steady_follow", "brief_insert_push", "locked_reaction", "slow_pan", "hold", "tracking_in", "still_hook")
SHOT_SIZE_LABELS = {
    "extreme_close_up": "特写",
    "medium_close_up": "中近景",
    "detail": "细节镜头",
    "medium": "中景",
    "close_up": "近景",
    "wide": "全景",
    "hook_close_up": "钩子近景",
}
CAMERA_MOTION_LABELS = {
    "slow_push_in": "缓慢推进",
    "steady_follow": "稳定跟拍",
    "brief_insert_push": "短促插入推进",
    "locked_reaction": "固定反应镜头",
    "slow_pan": "缓慢横摇",
    "hold": "固定停留",
    "tracking_in": "跟随推进",
    "still_hook": "静止钩子镜头",
}


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    episode = normalized["episode_writer"]
    character_bible = normalized["character_bible"]
    style_bible = normalized["style_bible"]
    shots = build_shots(episode, character_bible, style_bible)
    film_overview = build_film_overview(episode, style_bible, shots)
    storyboard_parts = build_storyboard_parts(episode, style_bible, shots)
    rendered_markdown = render_storyboard_markdown(film_overview, storyboard_parts)

    data = {
        "project_id": episode["project_id"],
        "episode_index": episode["episode_index"],
        "title": episode["title"],
        "language": episode["language"],
        "target_duration_seconds": episode["target_duration_seconds"],
        "aspect_ratio": episode["aspect_ratio"],
        "storyboard_summary": build_storyboard_summary(episode, shots),
        "shots": shots,
        "format_profile": {
            "name": "cinematic_md_v1",
            "mode": "mvp_structured",
            "strict_md_ready": False,
            "notes": [
                "当前为本地确定性分镜稿，优先对齐 MD 展示结构和字段。",
                "50-55 镜头、约 120 秒和逐句对白保真需要后续模型适配器与对白校验器。",
            ],
        },
        "film_overview": film_overview,
        "storyboard_parts": storyboard_parts,
        "rendered_markdown": rendered_markdown,
        "validation_report": build_storyboard_validation_report(episode, shots, storyboard_parts),
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


def build_film_overview(episode: dict[str, Any], style_bible: dict[str, Any], shots: list[dict[str, Any]]) -> dict[str, Any]:
    visual_style = style_bible.get("visual_style", {})
    camera_language = style_bible.get("camera_language", {})
    style_parts = [
        str(style_bible.get("style_name", "")).strip(),
        str(visual_style.get("rendering", "")).strip(),
        str(visual_style.get("texture", "")).strip(),
        str(visual_style.get("mood", "")).strip(),
    ]

    return {
        "episode_label": f"第{episode['episode_index']}集",
        "theme": episode["title"],
        "total_duration_seconds": sum(shot["duration_seconds"] for shot in shots),
        "target_duration_seconds": episode["target_duration_seconds"],
        "shot_count": len(shots),
        "style_tone": "，".join(part for part in style_parts if part),
        "camera_setup": build_camera_setup(camera_language, episode["aspect_ratio"]),
    }


def build_storyboard_parts(
    episode: dict[str, Any],
    style_bible: dict[str, Any],
    shots: list[dict[str, Any]],
) -> list[dict[str, Any]]:
    part_seconds = 15
    groups: list[list[dict[str, Any]]] = []

    for shot in shots:
        group_index = max(0, int(shot["start_second"]) // part_seconds)
        while len(groups) <= group_index:
            groups.append([])
        groups[group_index].append(shot)

    return [
        build_storyboard_part(index, episode, style_bible, group)
        for index, group in enumerate(groups, start=1)
        if group
    ]


def build_storyboard_part(
    part_index: int,
    episode: dict[str, Any],
    style_bible: dict[str, Any],
    shots: list[dict[str, Any]],
) -> dict[str, Any]:
    first_shot = shots[0]
    last_shot = shots[-1]
    start_second = int(first_shot["start_second"])
    end_second = int(last_shot["start_second"]) + int(last_shot["duration_seconds"])
    title = build_part_title(part_index, first_shot)
    characters = collect_part_characters(shots)
    formatted_shots = [build_formatted_shot(shot, index) for index, shot in enumerate(shots, start=1)]

    return {
        "part_id": f"part_{part_index:02d}",
        "part_index": part_index,
        "title": title,
        "start_second": start_second,
        "end_second": end_second,
        "duration_seconds": end_second - start_second,
        "time_weather_light": build_time_weather_light(style_bible, first_shot),
        "camera_setup": build_camera_setup(style_bible.get("camera_language", {}), episode["aspect_ratio"]),
        "cast_and_props": build_cast_and_props(characters),
        "absolute_blocking": build_absolute_blocking(characters),
        "style": build_part_style(style_bible),
        "shots": formatted_shots,
    }


def build_formatted_shot(shot: dict[str, Any], local_index: int) -> dict[str, Any]:
    duration = int(shot["duration_seconds"])
    camera = shot.get("camera", {})
    lighting = shot.get("lighting", {})
    dialogue = format_dialogue(shot.get("dialogue", []))
    narration = str(shot.get("narration", "")).strip()
    spoken_text = dialogue or narration or "无对白，依靠画面动作推进。"
    time_slice = f"0.0-{duration:.1f}s：{shot.get('visual_action', '')} {spoken_text}".strip()

    return {
        "shot_label": f"镜头 {shot['shot_index']}",
        "local_shot_index": local_index,
        "source_shot_id": shot["shot_id"],
        "duration_seconds": duration,
        "environment_description": build_environment_description(shot, lighting),
        "time_slice": time_slice,
        "shot_size": localize_shot_size(str(shot.get("shot_size", ""))),
        "camera_movement": build_camera_movement(camera),
        "sound_effect": str(shot.get("sound_note", "")).strip() or "保留环境声和对白清晰度，避免压过关键信息。",
        "background_music": build_background_music(shot),
        "dialogue": dialogue,
        "narration": narration,
    }


def build_part_title(part_index: int, shot: dict[str, Any]) -> str:
    scene = str(shot.get("scene", "")).split("；")
    focus = scene[1] if len(scene) > 1 and scene[1] else "关键行动"
    return f"第{part_index}部分：{focus}"


def collect_part_characters(shots: list[dict[str, Any]]) -> list[str]:
    names: list[str] = []
    for shot in shots:
        for character in shot.get("characters", []):
            name = character.get("name") if isinstance(character, dict) else None
            if isinstance(name, str) and name and name not in names:
                names.append(name)
    return names


def build_time_weather_light(style_bible: dict[str, Any], shot: dict[str, Any]) -> str:
    lighting = style_bible.get("lighting", {})
    location = str(shot.get("scene", "脚本指定场景")).split("；")[0]
    key_light = lighting.get("key_light", "低强度环境光")
    accent_light = lighting.get("accent_light", "线索提示光")
    return f"{location}，按脚本时间推进；主光为{key_light}，辅助光为{accent_light}。"


def build_camera_setup(camera_language: dict[str, Any], aspect_ratio: str) -> str:
    shot_rules = camera_language.get("shot_rules", [])
    movement_rules = camera_language.get("movement_rules", [])
    rules = [
        f"画幅 {aspect_ratio}",
        str(shot_rules[0]) if shot_rules else "镜头优先保证人物表情和关键信息可读。",
        str(movement_rules[0]) if movement_rules else "运动以稳定推进为主，避免无意义晃动。",
    ]
    return "；".join(rule for rule in rules if rule)


def build_cast_and_props(characters: list[str]) -> str:
    if characters:
        return f"出场人物：{'、'.join(characters)}；道具以脚本线索和角色随身物为准。"

    return "本部分以场景、线索或环境动作推进，暂未安排明确出场人物。"


def build_absolute_blocking(characters: list[str]) -> str:
    if not characters:
        return "关键线索位于画面中心偏下，镜头推进时保持字幕安全区。"

    if len(characters) == 1:
        return f"{characters[0]}位于画面中心偏左，视线朝向画面右侧或关键线索，留出字幕安全区。"

    return f"{characters[0]}在画面左侧主导行动，{characters[1]}在画面右侧回应，保持轴线和视线方向稳定。"


def build_part_style(style_bible: dict[str, Any]) -> str:
    visual_style = style_bible.get("visual_style", {})
    style_name = style_bible.get("style_name", "轻写实国漫")
    rendering = visual_style.get("rendering", "写实电影感")
    mood = visual_style.get("mood", "情绪克制")
    return f"{style_name}；{rendering}；{mood}；角色脸部与关键道具必须清晰。"


def build_environment_description(shot: dict[str, Any], lighting: dict[str, Any]) -> str:
    scene = str(shot.get("scene", "")).strip()
    rule = str(lighting.get("rule", "")).strip()
    return "；".join(part for part in [scene, rule] if part)


def build_camera_movement(camera: dict[str, Any]) -> str:
    angle = localize_camera_angle(str(camera.get("angle", "")))
    motion = CAMERA_MOTION_LABELS.get(str(camera.get("motion", "")), str(camera.get("motion", "")))
    composition = str(camera.get("composition", "")).strip()
    return "；".join(part for part in [angle, motion, composition] if part)


def build_background_music(shot: dict[str, Any]) -> str:
    if shot.get("dialogue"):
        return "音乐降低存在感，给对白和表演留出空间。"

    return "低频氛围音乐托底，随镜头推进轻微增强。"


def format_dialogue(dialogue_lines: Any) -> str:
    lines = []
    if not isinstance(dialogue_lines, list):
        return ""

    for dialogue in dialogue_lines:
        if not isinstance(dialogue, dict):
            continue
        speaker = str(dialogue.get("speaker", "")).strip()
        line = str(dialogue.get("line", "")).strip()
        if speaker and line:
            lines.append(f"{speaker}：“{line}”")
        elif line:
            lines.append(f"对白：“{line}”")

    return "；".join(lines)


def localize_shot_size(value: str) -> str:
    return SHOT_SIZE_LABELS.get(value, value or "中景")


def localize_camera_angle(value: str) -> str:
    angle_map = {
        "low detail angle focused on the clue": "低角度细节镜头，聚焦关键线索",
        "eye-level angle that keeps the environment readable": "平视镜头，保留环境关系",
        "slight close reaction angle on the main character": "轻微近景反应角度，突出主角表情",
    }
    return angle_map.get(value, value)


def render_storyboard_markdown(film_overview: dict[str, Any], storyboard_parts: list[dict[str, Any]]) -> str:
    lines = [
        str(film_overview["episode_label"]),
        "",
        "影片概览",
        f"影片主题：{film_overview['theme']}",
        f"总时长：{film_overview['total_duration_seconds']} 秒（目标 {film_overview['target_duration_seconds']} 秒）",
        f"镜头总数：{film_overview['shot_count']} 个",
        f"风格基调：{film_overview['style_tone']}",
        f"核心摄影设备定调：{film_overview['camera_setup']}",
        "",
    ]

    for part in storyboard_parts:
        lines.extend(
            [
                str(part["title"]),
                f"时间/天气/光线：{part['time_weather_light']}",
                f"核心摄影机与参数定调：{part['camera_setup']}",
                f"出场人物与道具：{part['cast_and_props']}",
                f"绝对人物站位：{part['absolute_blocking']}",
                f"风格：{part['style']}",
                "",
            ]
        )

        for shot in part["shots"]:
            lines.extend(
                [
                    str(shot["shot_label"]),
                    f"时长：{shot['duration_seconds']:.1f} 秒",
                    f"环境描写：{shot['environment_description']}",
                    f"时间切片与画面细分：{shot['time_slice']}",
                    f"镜头景别：{shot['shot_size']}",
                    f"镜头运动与衔接：{shot['camera_movement']}",
                    f"音效：{shot['sound_effect']}",
                    f"背景音乐：{shot['background_music']}",
                    "",
                ]
            )

    return "\n".join(lines).strip()


def build_storyboard_validation_report(
    episode: dict[str, Any],
    shots: list[dict[str, Any]],
    storyboard_parts: list[dict[str, Any]],
) -> dict[str, Any]:
    total_seconds = sum(shot["duration_seconds"] for shot in shots)
    return {
        "profile": "cinematic_md_v1",
        "strict_md_ready": False,
        "checks": [
            {
                "name": "分镜稿结构",
                "status": "通过",
                "detail": "已生成影片概览、分部信息和镜头级中文字段。",
            },
            {
                "name": "时长一致性",
                "status": "通过" if abs(total_seconds - episode["target_duration_seconds"]) <= 1 else "需检查",
                "detail": f"镜头总时长 {total_seconds} 秒，目标 {episode['target_duration_seconds']} 秒。",
            },
            {
                "name": "MVP 范围",
                "status": "提示",
                "detail": f"当前本地模式生成 {len(shots)} 个镜头、{len(storyboard_parts)} 个部分；严格 50-55 镜头留给后续模型模式。",
            },
        ],
        "deferred_requirements": [
            "约 120 秒、50-55 镜头的严格电影分镜模式。",
            "原始对白逐句保留和顺序校验。",
            "基于模型/API 的长文本分镜扩写与自动修正。",
        ],
    }


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
