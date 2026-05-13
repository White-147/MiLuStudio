from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    plot = normalized["plot_adaptation"]
    preferences = normalized["writing_preferences"]
    characters = plot["main_characters"]
    protagonist = characters[0]["name"] if characters else "主角"
    support = characters[1]["name"] if len(characters) > 1 else "线索"
    segments = build_segments(plot, protagonist, support, preferences)
    subtitle_cues = build_subtitle_cues(segments)

    data = {
        "project_id": plot["project_id"],
        "episode_index": 1,
        "title": plot["title"],
        "language": plot["language"],
        "target_duration_seconds": plot["target_duration_seconds"],
        "aspect_ratio": plot["aspect_ratio"],
        "summary": plot["logline"],
        "script_text": build_script_text(plot, segments),
        "segments": segments,
        "subtitle_cues": subtitle_cues,
        "voice_notes": {
            "narration": "克制、清晰、略带悬疑感；每句不拖长。",
            "dialogue": "对白短促，优先服务画面节奏。",
        },
        "review": {
            "status": "ready_for_review",
            "editable_fields": ["title", "segments", "script_text", "subtitle_cues"],
            "notes": [
                "用户可先改旁白和对白，再进入角色卡阶段。",
                "当前脚本只产出文本结构，不写数据库，不触发配音或字幕生成。",
            ],
        },
        "checkpoint": {
            "required": True,
            "policy": "pause_for_script_review",
            "reason": "episode_writer output is the first user-reviewable script artifact.",
        },
    }

    return validate_output(data)


def build_segments(
    plot: dict[str, Any],
    protagonist: str,
    support: str,
    preferences: dict[str, str],
) -> list[dict[str, Any]]:
    plot_beats = plot["plot_beats"]
    dialogue_density = preferences["dialogue_density"]
    segments = []
    start_second = 0

    for index, beat in enumerate(plot_beats):
        duration = beat["estimated_seconds"]
        dialogue_lines = build_dialogue_lines(index, protagonist, support, dialogue_density)
        narration = build_narration(index, beat, plot, protagonist)
        visual_direction = build_visual_direction(index, beat, plot, protagonist)

        segments.append(
            {
                "segment_id": f"seg_{index + 1:02d}",
                "beat_id": beat["beat_id"],
                "start_second": start_second,
                "duration_seconds": duration,
                "purpose": beat["purpose"],
                "visual_direction": visual_direction,
                "narration": narration,
                "dialogue_lines": dialogue_lines,
                "pacing_note": build_pacing_note(index, duration),
                "sound_note": build_sound_note(index, plot["tone_keywords"]),
            }
        )
        start_second += duration

    return segments


def build_narration(index: int, beat: dict[str, Any], plot: dict[str, Any], protagonist: str) -> str:
    if index == 0:
        return trim_text(f"{protagonist}以为这只是普通的一晚，直到线索把{protagonist}带向不该出现的地方。", 48)

    if index == 1:
        return trim_text(f"越靠近真相，熟悉的记忆越像被人重新摆放过。", 48)

    if index == 2:
        return trim_text(f"证据出现的那一刻，{protagonist}终于明白，危险一直等在前方。", 48)

    return trim_text(plot["ending_hook"], 52)


def build_dialogue_lines(
    index: int,
    protagonist: str,
    support: str,
    dialogue_density: str,
) -> list[dict[str, str]]:
    if dialogue_density == "low" and index in {1, 2}:
        return []

    if index == 0:
        return [
            {
                "speaker": protagonist,
                "line": "你到底想带我去哪？",
                "delivery": "压低声音，带一点试探。",
            }
        ]

    if index == 1:
        return [
            {
                "speaker": protagonist,
                "line": f"{support}的线索，怎么会在这里？",
                "delivery": "困惑后迅速紧张。",
            }
        ]

    if index == 2:
        return [
            {
                "speaker": protagonist,
                "line": "原来他不是失踪，是发现了这扇门。",
                "delivery": "震惊，尾音收住。",
            }
        ]

    return [
        {
            "speaker": protagonist,
            "line": "如果门后是答案，我就进去。",
            "delivery": "坚定，但保留不安。",
        }
    ]


def build_visual_direction(index: int, beat: dict[str, Any], plot: dict[str, Any], protagonist: str) -> str:
    aspect = "竖屏近景" if plot["aspect_ratio"] == "9:16" else "电影感中景"

    if index == 0:
        return f"{aspect}，用异常线索贴近镜头开场，{protagonist}从画面边缘追入。"

    if index == 1:
        return f"{aspect}切到关键地点，环境细节压暗，只突出角色和线索。"

    if index == 2:
        return f"{aspect}推进到证据特写，再切{protagonist}的反应。"

    return f"{aspect}收束在未解画面上，留出 1 秒静默给结尾钩子。"


def build_pacing_note(index: int, duration: int) -> str:
    if index == 0:
        return f"{duration} 秒内完成钩子，不解释背景。"

    if index == 3:
        return f"{duration} 秒先揭示一半真相，再用最后一句对白收尾。"

    return f"{duration} 秒保持一镜一信息点，避免对白堆叠。"


def build_sound_note(index: int, tone_keywords: list[str]) -> str:
    tone = "、".join(tone_keywords[:2]) if tone_keywords else "悬疑"

    if index == 0:
        return f"{tone}低频氛围，线索出现时给轻微提示音。"

    if index == 3:
        return "环境声骤停，结尾对白后留半秒空白。"

    return f"{tone}氛围延续，避免盖住对白。"


def build_subtitle_cues(segments: list[dict[str, Any]]) -> list[dict[str, Any]]:
    cues = []

    for segment in segments:
        start = segment["start_second"]
        duration = segment["duration_seconds"]
        narration_end = start + min(duration, max(3, duration // 2))
        cues.append(
            {
                "cue_id": f"{segment['segment_id']}_narration",
                "start_second": start,
                "end_second": narration_end,
                "text": trim_text(segment["narration"], 30),
                "source": "narration",
            }
        )

        if segment["dialogue_lines"]:
            cues.append(
                {
                    "cue_id": f"{segment['segment_id']}_dialogue",
                    "start_second": narration_end,
                    "end_second": start + duration,
                    "text": trim_text(segment["dialogue_lines"][0]["line"], 30),
                    "source": "dialogue",
                }
            )

    return cues


def build_script_text(plot: dict[str, Any], segments: list[dict[str, Any]]) -> str:
    lines = [
        f"标题：{plot['title']}",
        f"时长：{plot['target_duration_seconds']} 秒",
        f"梗概：{plot['logline']}",
        "",
    ]

    for segment in segments:
        lines.append(f"{segment['segment_id']}｜{segment['start_second']}s-{segment['start_second'] + segment['duration_seconds']}s")
        lines.append(f"画面：{segment['visual_direction']}")
        lines.append(f"旁白：{segment['narration']}")
        for dialogue in segment["dialogue_lines"]:
            lines.append(f"对白（{dialogue['speaker']}）：{dialogue['line']}")
        lines.append(f"节奏：{segment['pacing_note']}")
        lines.append("")

    return "\n".join(lines).strip()


def trim_text(value: str, limit: int) -> str:
    if len(value) <= limit:
        return value

    return value[: limit - 3].rstrip("，,。 ") + "..."
