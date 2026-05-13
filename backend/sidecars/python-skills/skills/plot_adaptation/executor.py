from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    intake = normalized["story_intake"]
    preferences = normalized["adaptation_preferences"]
    target_duration = intake["target_duration_seconds"]
    beat_durations = distribute_duration(target_duration, [0.18, 0.27, 0.27, 0.28])
    characters = intake["main_characters"]
    protagonist = characters[0]["name"] if characters else "主角"
    support_names = [character["name"] for character in characters[1:]]
    story_beats = intake["story_beats"]
    title = build_title(intake["summary"], protagonist, intake["tone_keywords"])
    core_mystery = build_core_mystery(intake["summary"], story_beats)

    data = {
        "project_id": intake["project_id"],
        "language": intake["language"],
        "title": title,
        "logline": intake["logline"],
        "target_duration_seconds": target_duration,
        "aspect_ratio": intake["aspect_ratio"],
        "mode": intake["mode"],
        "style_preset": intake["style_preset"],
        "genre_tags": intake["genres"],
        "tone_keywords": intake["tone_keywords"],
        "main_characters": characters,
        "adaptation_strategy": {
            "format": "vertical_short_video",
            "narrative_pov": preferences["narrative_pov"],
            "ending_style": preferences["ending_style"],
            "target_seconds": target_duration,
            "recommended_shot_count": intake["recommended_shot_count"],
            "compression_notes": build_compression_notes(intake),
        },
        "plot_beats": build_plot_beats(
            protagonist=protagonist,
            support_names=support_names,
            story_beats=story_beats,
            summary=intake["summary"],
            core_mystery=core_mystery,
            beat_durations=beat_durations,
        ),
        "core_conflict": f"{protagonist}必须在短时间内确认线索真伪，并决定是否继续追进未知风险。",
        "turning_point": trim_text(story_beats[min(1, len(story_beats) - 1)]["summary"], 96),
        "ending_hook": build_ending_hook(story_beats[-1]["summary"], preferences["ending_style"]),
        "continuity_notes": build_continuity_notes(protagonist, support_names, intake["tone_keywords"]),
        "review_points": [
            "确认短视频主线是否保留原故事最强悬念。",
            "确认结尾钩子是否足够推动用户继续观看。",
            "确认角色称呼是否需要在进入角色卡阶段前改名。",
        ],
        "risks": intake["risks"],
        "checkpoint": {
            "required": False,
            "policy": "optional_review",
            "reason": "plot_adaptation provides structure for episode_writer and can auto-continue in MVP.",
        },
    }

    return validate_output(data)


def distribute_duration(total_seconds: int, weights: list[float]) -> list[int]:
    raw = [max(4, round(total_seconds * weight)) for weight in weights]
    delta = total_seconds - sum(raw)
    raw[-1] += delta

    if raw[-1] < 4:
        borrowed = 4 - raw[-1]
        raw[-1] = 4
        raw[1] = max(4, raw[1] - borrowed)

    return raw


def build_title(summary: str, protagonist: str, tone_keywords: list[str]) -> str:
    if "纸鹤" in summary and ("雨" in summary or "雨夜" in tone_keywords):
        return "雨夜纸鹤"

    if protagonist != "主角":
        keyword = next((tone for tone in tone_keywords if len(tone) <= 4), "秘密")
        return trim_text(f"{protagonist}的{keyword}", 16)

    compact = summary.split("。", maxsplit=1)[0].strip()
    return trim_text(compact, 16) or "短视频脚本"


def build_core_mystery(summary: str, story_beats: list[dict[str, Any]]) -> str:
    if len(story_beats) >= 3:
        return trim_text(story_beats[-1]["summary"], 90)

    return trim_text(summary, 90)


def build_plot_beats(
    protagonist: str,
    support_names: list[str],
    story_beats: list[dict[str, Any]],
    summary: str,
    core_mystery: str,
    beat_durations: list[int],
) -> list[dict[str, Any]]:
    support_text = "、".join(support_names) if support_names else "未知线索"
    source_hook = story_beats[0]["summary"] if story_beats else summary
    source_turn = story_beats[1]["summary"] if len(story_beats) > 1 else summary
    source_reveal = story_beats[-1]["summary"] if story_beats else summary

    return [
        {
            "beat_id": "beat_01_hook",
            "label": "前三秒钩子",
            "purpose": "立刻抛出异常物件和追踪动机。",
            "summary": trim_text(f"{protagonist}被一个异常线索吸引：{source_hook}", 120),
            "estimated_seconds": beat_durations[0],
            "emotional_value": "好奇、紧张",
            "required_characters": [protagonist],
        },
        {
            "beat_id": "beat_02_setup",
            "label": "追踪与信息铺垫",
            "purpose": "交代地点、人物牵挂和必须行动的原因。",
            "summary": trim_text(f"{protagonist}追向关键地点，{support_text}与主线秘密被同时带出。", 120),
            "estimated_seconds": beat_durations[1],
            "emotional_value": "压迫、牵挂",
            "required_characters": [protagonist, *support_names[:1]],
        },
        {
            "beat_id": "beat_03_turn",
            "label": "发现反转",
            "purpose": "让观众看到证据，并把危险推到角色面前。",
            "summary": trim_text(source_turn, 120),
            "estimated_seconds": beat_durations[2],
            "emotional_value": "震惊、犹豫",
            "required_characters": [protagonist],
        },
        {
            "beat_id": "beat_04_hook",
            "label": "结尾钩子",
            "purpose": "用未解谜团或下一步行动收束，留下继续观看理由。",
            "summary": trim_text(source_reveal if core_mystery == source_reveal else f"{core_mystery} {source_reveal}", 120),
            "estimated_seconds": beat_durations[3],
            "emotional_value": "期待、悬念",
            "required_characters": [protagonist],
        },
    ]


def build_compression_notes(intake: dict[str, Any]) -> list[str]:
    notes = [
        "保留一个主角、一个核心线索和一个结尾钩子。",
        "把解释性背景压缩为旁白，不在 Stage 5 展开支线角色。",
    ]

    if intake["source_word_count"] > 1600:
        notes.append("输入较长，脚本阶段只保留最强因果链。")

    if intake["mode"] == "fast":
        notes.append("极速模式下减少对白，把信息放入短旁白和画面动作。")

    return notes


def build_ending_hook(source_reveal: str, ending_style: str) -> str:
    if ending_style == "closed":
        return trim_text(f"谜底被揭开，但最后一个画面保留新的疑问：{source_reveal}", 110)

    return trim_text(f"真相只露出一角，角色即将跨过下一道门槛：{source_reveal}", 110)


def build_continuity_notes(protagonist: str, support_names: list[str], tone_keywords: list[str]) -> list[str]:
    notes = [
        f"后续脚本称呼主角为“{protagonist}”，进入角色卡前不再更换。",
        f"视觉气质保持：{'、'.join(tone_keywords[:3])}。",
    ]

    if support_names:
        notes.append(f"配角或牵挂对象只保留：{'、'.join(support_names[:2])}。")

    return notes


def trim_text(value: str, limit: int) -> str:
    if len(value) <= limit:
        return value

    return value[: limit - 3].rstrip("，,。 ") + "..."
