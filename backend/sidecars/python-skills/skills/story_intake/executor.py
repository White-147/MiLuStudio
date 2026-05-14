from __future__ import annotations

import re
from typing import Any

from .validators import validate_input, validate_output

SENTENCE_SPLIT_PATTERN = re.compile(r"(?<=[。！？!?；;])\s*")
ACTION_NAME_PATTERN = re.compile(r"(?:^|[，。！？；\s])([\u4e00-\u9fff]{2,4})(?=在|从|向|把|被|捡|发现|进入|来到)")
EXPLICIT_NAME_PATTERNS = (
    re.compile(
        r"(?:^|[，。！？；\s])([\u4e00-\u9fff]{2,4})(?=是[^，。！？；]{0,18}"
        r"(?:小姐|姑娘|女子|少女|少年|书生|公子|侍女|丫鬟|太守|画师|学生|老师|主角|主人公))"
    ),
    re.compile(
        r"(?:侍女|丫鬟|书生|小姐|姑娘|公子|少年|少女|画师|学生|老师)([\u4e00-\u9fff]{2,3})"
        r"(?=悄悄|忽然|却|也|和|与|，|。|、|说|道|走|来|去|把|将|在|从|的|$)"
    ),
    re.compile(r"(?:自称|名叫|叫作|叫做|名为|名字叫|唤作)([\u4e00-\u9fff]{2,4})"),
)
NON_CHARACTER_TERMS = (
    "旧巷",
    "巷口",
    "园中",
    "小池",
    "水面",
    "柳枝",
    "花影",
    "纸鹤",
    "不断",
    "飞向",
    "照相",
    "胶片",
    "午夜",
    "门后",
    "她倚",
)
NON_NAME_PREFIXES = ("她", "他", "它", "这", "那", "的", "了", "从", "在", "向", "把", "被", "对", "和", "与")
NON_NAME_SUFFIXES = ("里", "前", "后", "中", "口", "边", "旁", "上", "下", "内", "外", "时", "处", "间", "像", "垂", "开", "写", "吹", "倚", "悄")
KINSHIP_NAMES = ("哥哥", "姐姐", "妹妹", "弟弟", "父亲", "母亲")


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    story_text = normalized["story_text"]
    sentences = split_sentences(story_text)
    summary = build_summary(sentences)
    genres = infer_genres(story_text)
    tone_keywords = infer_tone_keywords(story_text, normalized["style_preset"])
    characters = infer_characters(story_text)
    target_duration = normalized["target_duration_seconds"]

    data = {
        "project_id": normalized["project_id"],
        "language": normalized["language"],
        "source_word_count": count_story_units(story_text),
        "summary": summary,
        "logline": build_logline(summary, genres),
        "genres": genres,
        "tone_keywords": tone_keywords,
        "style_preset": normalized["style_preset"],
        "mode": normalized["mode"],
        "target_duration_seconds": target_duration,
        "aspect_ratio": normalized["aspect_ratio"],
        "recommended_shot_count": recommend_shot_count(target_duration, normalized["mode"]),
        "main_characters": characters,
        "story_beats": build_story_beats(sentences),
        "risks": build_risks(story_text, characters),
        "checkpoint": {
            "required": False,
            "policy": "auto_continue",
            "reason": "story_intake only normalizes source story and does not need user approval.",
        },
    }

    return validate_output(data)


def split_sentences(story_text: str) -> list[str]:
    sentences = [sentence.strip() for sentence in SENTENCE_SPLIT_PATTERN.split(story_text) if sentence.strip()]

    if sentences:
        return sentences

    return [story_text.strip()]


def build_summary(sentences: list[str]) -> str:
    joined = "".join(sentences[:3])

    if len(joined) <= 140:
        return joined

    return joined[:137].rstrip("，,。 ") + "..."


def build_logline(summary: str, genres: list[str]) -> str:
    genre_text = "、".join(genres[:2]) if genres else "剧情"
    return f"一部{genre_text}短篇：{summary}"


def infer_genres(story_text: str) -> list[str]:
    candidates: list[tuple[str, tuple[str, ...]]] = [
        ("悬疑", ("失踪", "线索", "废弃", "秘密", "追踪", "谜")),
        ("都市", ("城市", "旧巷", "便利店", "照相馆", "雨夜", "街")),
        ("奇幻", ("发光", "壁画", "梦", "纸鹤", "魔法", "异界")),
        ("科幻", ("星", "飞船", "机器人", "宇宙", "星港", "未来")),
        ("古风", ("长安", "画师", "古", "宫", "剑", "仙", "太守", "侍女", "书生", "小姐", "牡丹")),
        ("喜剧", ("误会", "搞笑", "轻喜剧", "笑", "荒唐")),
    ]
    genres = [genre for genre, keywords in candidates if any(keyword in story_text for keyword in keywords)]

    return genres[:3] or ["剧情"]


def infer_tone_keywords(story_text: str, style_preset: str) -> list[str]:
    keywords = []
    mapping = [
        ("雨夜", ("雨", "雨夜")),
        ("悬疑", ("悬疑", "失踪", "线索", "秘密")),
        ("温暖", ("便利店", "家", "朋友", "守护")),
        ("奇幻", ("发光", "壁画", "梦", "异界")),
        ("轻快", ("喜剧", "误会", "笑")),
        ("国漫", ("国漫", "古风", "长安")),
    ]

    for tone, terms in mapping:
        if tone in style_preset or any(term in story_text for term in terms):
            keywords.append(tone)

    if style_preset and style_preset not in keywords:
        keywords.append(style_preset)

    return keywords[:4] or ["轻写实"]


def infer_characters(story_text: str) -> list[dict[str, str]]:
    names: list[str] = []

    for pattern in EXPLICIT_NAME_PATTERNS:
        for match in pattern.finditer(story_text):
            add_unique_character_name(names, match.group(1))

    for match in ACTION_NAME_PATTERN.finditer(story_text):
        add_unique_character_name(names, match.group(1))

    for name in KINSHIP_NAMES:
        if name in story_text:
            add_unique_character_name(names, name)

    if not names:
        names.append("主角")

    characters = []
    for index, name in enumerate(names[:3]):
        role = "protagonist" if index == 0 else "supporting"
        characters.append(
            {
                "name": name,
                "role": role,
                "evidence": extract_evidence(story_text, name),
            }
        )

    return characters


def add_unique_character_name(names: list[str], value: str) -> None:
    name = normalize_name(value)
    if not is_likely_character_name(name):
        return

    for existing in names:
        if existing == name or (len(existing) > len(name) and existing.endswith(name)):
            return

    for index, existing in enumerate(names):
        if len(name) > len(existing) and name.endswith(existing):
            names[index] = name
            return

    names.append(name)


def normalize_name(value: str) -> str:
    return value.strip("“”\"'：:，,。！？；;、 \n\t")


def is_likely_character_name(name: str) -> bool:
    if len(name) < 2 or len(name) > 4:
        return False

    if name.startswith(NON_NAME_PREFIXES) or name.endswith(NON_NAME_SUFFIXES):
        return False

    return not any(term in name for term in NON_CHARACTER_TERMS)


def extract_evidence(story_text: str, name: str) -> str:
    if name == "主角":
        return "输入故事中没有稳定角色名，后续脚本阶段需要补齐。"

    index = story_text.find(name)
    start = max(0, index - 12)
    end = min(len(story_text), index + 40)
    return story_text[start:end].strip()


def build_story_beats(sentences: list[str]) -> list[dict[str, Any]]:
    labels = ["开端", "推进", "钩子"]

    if len(sentences) == 1:
        parts = [sentences[0], sentences[0], sentences[0]]
    else:
        parts = [
            sentences[0],
            sentences[min(1, len(sentences) - 1)],
            sentences[-1],
        ]

    return [
        {
            "index": index + 1,
            "label": labels[index],
            "summary": trim_text(parts[index], 96),
        }
        for index in range(3)
    ]


def build_risks(story_text: str, characters: list[dict[str, str]]) -> list[dict[str, str]]:
    risks = []
    unit_count = count_story_units(story_text)

    if unit_count < 500:
        risks.append(
            {
                "code": "SOURCE_TOO_SHORT",
                "severity": "warning",
                "message": "输入少于 500 字，后续脚本阶段可能需要补充情节细节。",
            }
        )

    if unit_count > 2000:
        risks.append(
            {
                "code": "SOURCE_TOO_LONG",
                "severity": "warning",
                "message": "输入超过 2000 字，后续阶段需要先压缩主线。",
            }
        )

    if characters and characters[0]["name"] == "主角":
        risks.append(
            {
                "code": "MISSING_CHARACTER_NAME",
                "severity": "info",
                "message": "未识别到稳定角色名，后续角色卡需要用户或脚本阶段补齐。",
            }
        )

    return risks


def recommend_shot_count(target_duration_seconds: int, mode: str) -> int:
    seconds_per_shot = 5 if mode == "director" else 6
    raw_count = round(target_duration_seconds / seconds_per_shot)
    return max(6, min(12, raw_count))


def count_story_units(story_text: str) -> int:
    return sum(1 for character in story_text if not character.isspace())


def trim_text(value: str, limit: int) -> str:
    if len(value) <= limit:
        return value

    return value[: limit - 3].rstrip("，,。 ") + "..."
