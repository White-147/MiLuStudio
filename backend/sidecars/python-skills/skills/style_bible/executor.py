from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    episode = normalized["episode_writer"]
    character_bible = normalized["character_bible"]
    text = combined_episode_text(episode)
    style_name = build_style_name(text)
    palette = build_color_palette(text)
    character_rules = build_character_rendering_rules(character_bible)

    data = {
        "project_id": episode["project_id"],
        "episode_index": episode["episode_index"],
        "title": episode["title"],
        "language": episode["language"],
        "style_name": style_name,
        "visual_style": build_visual_style(style_name, text),
        "color_palette": palette,
        "lighting": build_lighting(text),
        "environment_design": build_environment_design(text),
        "camera_language": build_camera_language(episode),
        "character_rendering_rules": character_rules,
        "negative_prompt": build_negative_prompt(),
        "reusable_prompt_blocks": build_reusable_prompt_blocks(style_name, palette, character_rules, text),
        "image_prompt_guidelines": build_image_prompt_guidelines(),
        "video_prompt_guidelines": build_video_prompt_guidelines(episode),
        "continuity_notes": build_continuity_notes(episode, character_bible),
        "review": {
            "status": "ready_for_review",
            "editable_fields": [
                "visual_style",
                "color_palette",
                "camera_language",
                "negative_prompt",
                "reusable_prompt_blocks",
            ],
            "notes": [
                "用户可在此阶段锁定画风、色板、镜头语言和负面提示词。",
                "当前结果只产出统一画风结构，不调用图像或视频模型，不写数据库。",
            ],
        },
        "checkpoint": {
            "required": True,
            "policy": "pause_for_style_review",
            "reason": "style_bible output should be reviewed before storyboard and prompt expansion.",
        },
    }

    return validate_output(data)


def build_style_name(text: str) -> str:
    if "雨" in text and any(keyword in text for keyword in ("门", "失踪", "真相")):
        return "雨夜悬疑国漫"

    if any(keyword in text for keyword in ("校园", "夏天", "朋友")):
        return "清透青春短片"

    return "轻写实国漫短视频"


def build_visual_style(style_name: str, text: str) -> dict[str, str]:
    rendering = "2D 国漫短视频，半写实角色，背景保留手绘质感。"
    if "雨" in text:
        rendering = "2D 国漫短视频，半写实角色，雨水和霓虹反光用克制高光处理。"

    return {
        "name": style_name,
        "medium": "short-form animation style guide",
        "rendering": rendering,
        "texture": "背景允许轻微颗粒和旧胶片纹理，角色脸部保持干净。",
        "mood": "悬念先行，情绪克制，不走恐怖血腥方向。",
    }


def build_color_palette(text: str) -> list[dict[str, str]]:
    if "雨" in text or "夜" in text:
        return [
            {"name": "midnight_blue", "hex": "#17213A", "usage": "夜景和主背景阴影。"},
            {"name": "wet_asphalt", "hex": "#2E3438", "usage": "地面、墙面和雨水暗部。"},
            {"name": "paper_crane_glow", "hex": "#D9F7FF", "usage": "核心线索的冷白发光。"},
            {"name": "warning_amber", "hex": "#E5A64B", "usage": "门缝、灯箱或结尾钩子的少量暖光。"},
            {"name": "skin_soft_light", "hex": "#F1D0BE", "usage": "角色脸部受光，避免过灰。"},
        ]

    return [
        {"name": "ink_gray", "hex": "#34383C", "usage": "线稿和深色服装。"},
        {"name": "warm_ivory", "hex": "#F2E8D5", "usage": "主背景亮部。"},
        {"name": "soft_cyan", "hex": "#A9D8E8", "usage": "信息点和空气感。"},
        {"name": "muted_red", "hex": "#B75B52", "usage": "小面积情绪提示。"},
    ]


def build_lighting(text: str) -> dict[str, Any]:
    rules = [
        "主角脸部保留可读高光，环境阴影不压掉表情。",
        "核心线索出现时使用单一方向的提示光，避免满屏发光。",
    ]
    if "门" in text:
        rules.append("门缝光可作为结尾钩子，但不提前照亮全部空间。")

    return {
        "key_light": "低强度侧光或环境反射光。",
        "accent_light": "只服务线索、角色眼神和结尾钩子。",
        "rules": rules,
    }


def build_environment_design(text: str) -> dict[str, Any]:
    motifs = []
    if "照相馆" in text:
        motifs.append("废弃照相馆、暗房、旧照片边框。")
    if "胶片" in text:
        motifs.append("胶片颗粒、冲洗液痕迹、编号贴纸。")
    if "雨" in text:
        motifs.append("雨巷、湿地反光、招牌残光。")
    if "门" in text:
        motifs.append("午夜门形光源，不提前展示门后全貌。")

    return {
        "primary_locations": motifs or ["与脚本线索一致的简洁场景。"],
        "density": "竖屏画面每镜只突出一个环境识别点。",
        "avoid": "背景细节不能抢走角色和线索的可读性。",
    }


def build_camera_language(episode: dict[str, Any]) -> dict[str, Any]:
    aspect = episode.get("aspect_ratio", "9:16")
    return {
        "aspect_ratio": aspect,
        "shot_rules": [
            "开场 2 秒使用线索特写或主角反应近景。",
            "主角做决定时切到稳定中近景，给表情和字幕空间。",
            "结尾保留一个未完全解释的画面钩子。",
        ],
        "movement_rules": [
            "移动镜头以慢推、轻跟拍为主，不做炫技摇晃。",
            "发现证据时允许一次短促推进，但随后稳定住画面。",
        ],
        "composition_rules": [
            "竖屏安全区内保留字幕下沿空间。",
            "核心线索和角色眼神不要同时贴边。",
        ],
        "transition_rules": [
            "段落间用动作或线索方向衔接。",
            "避免无意义闪白、故障风和强转场遮挡信息。",
        ],
    }


def build_character_rendering_rules(character_bible: dict[str, Any]) -> list[dict[str, Any]]:
    rules = []
    for character in character_bible.get("characters", []):
        costume = character.get("costume", {})
        visual_identity = character.get("visual_identity", {})
        rules.append(
            {
                "character_id": character["character_id"],
                "name": character["name"],
                "stable_seed": character["stable_seed"],
                "locked_features": visual_identity.get("must_keep", []),
                "costume_lock": costume.get("lock_rule", ""),
                "voice_reference": character.get("voice_profile", {}).get("tone", ""),
            }
        )

    return rules


def build_negative_prompt() -> list[str]:
    return [
        "角色脸型漂移",
        "发型或服装主色跨镜头变化",
        "过度写实摄影皮肤",
        "血腥恐怖表现",
        "满屏发光粒子遮挡表情",
        "字幕安全区被主体遮挡",
        "随机新增无关角色",
        "低清晰度、手指畸形、眼神失焦",
    ]


def build_reusable_prompt_blocks(
    style_name: str,
    palette: list[dict[str, str]],
    character_rules: list[dict[str, Any]],
    text: str,
) -> dict[str, str]:
    palette_text = ", ".join(f"{color['name']} {color['hex']}" for color in palette[:4])
    character_text = "; ".join(
        f"{rule['name']}({rule['character_id']}): {' / '.join(rule['locked_features'][:2])}"
        for rule in character_rules
    )
    environment = "雨夜旧巷、废弃照相馆、胶片线索" if "照相馆" in text else "脚本指定场景、核心线索道具"

    return {
        "base_style": f"{style_name}, 2D animated short-video frame, controlled suspense, clean character faces, palette: {palette_text}",
        "character_consistency": character_text,
        "environment": environment,
        "camera": "vertical composition, readable face, one story clue per frame, room for subtitles",
        "quality_guardrails": "consistent character identity, no random outfit change, no extra characters, clear focal point",
    }


def build_image_prompt_guidelines() -> list[str]:
    return [
        "单张图像提示词必须引用 base_style 和对应 character_consistency。",
        "每张图只强调一个视觉信息点：角色、线索或环境。",
        "输出给图像模型前再追加 negative_prompt，不在本 skill 调模型。",
    ]


def build_video_prompt_guidelines(episode: dict[str, Any]) -> list[str]:
    duration = episode.get("target_duration_seconds", 45)
    return [
        f"整集目标时长 {duration} 秒，视频提示词按 episode_writer.segments 拆分。",
        "每段视频提示词复用相同 style_name、palette 和角色锁定字段。",
        "镜头运动只描述慢推、跟拍、稳定中近景等可控动作。",
    ]


def build_continuity_notes(episode: dict[str, Any], character_bible: dict[str, Any]) -> list[str]:
    return [
        f"画风结构绑定 episode_writer《{episode['title']}》和 character_bible 的 stable_seed。",
        "后续 storyboard_director、image_prompt 或 video_prompt 阶段必须复用 reusable_prompt_blocks。",
        "当前阶段不保存数据库；持久化由后端 EF Core / SQLite 边界接入。",
        *character_bible.get("continuity_rules", [])[:2],
    ]


def combined_episode_text(episode: dict[str, Any]) -> str:
    pieces = [
        str(episode.get("title", "")),
        str(episode.get("summary", "")),
        str(episode.get("script_text", "")),
    ]

    for segment in episode.get("segments", []):
        pieces.append(str(segment.get("visual_direction", "")))
        pieces.append(str(segment.get("narration", "")))
        pieces.append(str(segment.get("sound_note", "")))
        for dialogue in segment.get("dialogue_lines", []):
            pieces.append(str(dialogue.get("line", "")))

    return "\n".join(piece for piece in pieces if piece)
