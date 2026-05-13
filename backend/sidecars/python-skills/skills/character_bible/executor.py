from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output

KINSHIP_CANDIDATES = ("哥哥", "姐姐", "弟弟", "妹妹", "父亲", "母亲", "老师", "朋友", "同伴")


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    episode = normalized["episode_writer"]
    preferences = normalized["character_preferences"]
    characters = build_characters(episode, preferences)

    data = {
        "project_id": episode["project_id"],
        "episode_index": episode["episode_index"],
        "title": episode["title"],
        "language": episode["language"],
        "source_summary": episode["summary"],
        "characters": characters,
        "relationship_notes": build_relationship_notes(characters),
        "continuity_rules": build_global_continuity_rules(episode, characters),
        "review": {
            "status": "ready_for_review",
            "editable_fields": [
                "characters",
                "relationship_notes",
                "continuity_rules",
            ],
            "notes": [
                "用户可在此阶段锁定角色外观、服装和声音方向。",
                "当前结果只产出角色设定结构，不写数据库，不上传参考图，不触发生成模型。",
            ],
        },
        "checkpoint": {
            "required": True,
            "policy": "pause_for_character_review",
            "reason": "character_bible output should be reviewed before style and storyboard generation.",
        },
    }

    return validate_output(data)


def build_characters(episode: dict[str, Any], preferences: dict[str, Any]) -> list[dict[str, Any]]:
    names = collect_character_names(episode, preferences)
    text = combined_episode_text(episode)
    anchors = extract_visual_anchors(text)
    characters: list[dict[str, Any]] = []

    for index, name in enumerate(names):
        role_type = "main" if index == 0 else "supporting"
        characters.append(build_character(index, name, role_type, episode, text, anchors))

    return characters


def collect_character_names(episode: dict[str, Any], preferences: dict[str, Any]) -> list[str]:
    names: list[str] = []

    for segment in episode.get("segments", []):
        for dialogue in segment.get("dialogue_lines", []):
            add_unique_name(names, dialogue.get("speaker"))

    for locked_name in preferences.get("locked_character_names", []):
        add_unique_name(names, locked_name)

    text = combined_episode_text(episode)
    for candidate in KINSHIP_CANDIDATES:
        if candidate in text:
            add_unique_name(names, candidate)

    if not names:
        names.append("主角")

    return names[:6]


def add_unique_name(names: list[str], value: Any) -> None:
    if not isinstance(value, str):
        return

    name = value.strip()
    if name and name not in names:
        names.append(name)


def build_character(
    index: int,
    name: str,
    role_type: str,
    episode: dict[str, Any],
    text: str,
    anchors: list[str],
) -> dict[str, Any]:
    is_main = role_type == "main"
    evidence = collect_source_evidence(name, episode)
    signature = anchors[0] if anchors else "清晰可复现的剪影和主色标记"
    supporting_signature = "与主角形成对照的轮廓或道具线索"

    return {
        "character_id": f"char_{index + 1:02d}",
        "name": name,
        "role_type": role_type,
        "stable_seed": f"{episode['project_id']}:episode_{episode['episode_index']}:char_{index + 1:02d}",
        "identity": build_identity(name, is_main),
        "personality": build_personality(name, is_main, text),
        "appearance": build_appearance(name, is_main, signature, supporting_signature, text),
        "costume": build_costume(is_main, text),
        "voice_profile": build_voice_profile(name, is_main),
        "visual_identity": {
            "must_keep": build_must_keep_rules(name, is_main, anchors),
            "may_vary": [
                "表情强度可随剧情变化。",
                "雨水、阴影和镜头距离可按场景调整。",
            ],
            "avoid": [
                "同一角色脸型、发型、服装主色在镜头间漂移。",
                "用临时道具替代已锁定的标志性线索。",
            ],
        },
        "expression_palette": build_expression_palette(is_main),
        "continuity_rules": build_character_continuity_rules(is_main),
        "source_evidence": evidence,
    }


def build_identity(name: str, is_main: bool) -> str:
    if is_main:
        return f"{name}是推动短片行动的主角，负责发现线索、做出选择并承担结尾钩子。"

    return f"{name}是牵动主角行动的关键关系角色，承担线索来源或情绪压力。"


def build_personality(name: str, is_main: bool, text: str) -> list[str]:
    if is_main:
        traits = ["敏锐", "克制", "遇到异常时仍会主动追查"]
        if any(keyword in text for keyword in ("门", "失踪", "真相")):
            traits.append("面对未知时恐惧但不退缩")
        return traits

    if name in KINSHIP_CANDIDATES:
        return ["留白感强", "像谜题核心一样被旁人记住", "与主角有明确情感牵引"]

    return ["信息量集中", "出现时必须服务线索推进", "不抢主角视线"]


def build_appearance(
    name: str,
    is_main: bool,
    signature: str,
    supporting_signature: str,
    text: str,
) -> dict[str, str]:
    gender = infer_gender(name, text)

    if is_main:
        return {
            "age_range": "青年",
            "gender_expression": gender,
            "silhouette": "偏利落的竖屏主角轮廓，便于近景识别。",
            "face_hair": "五官保持清晰，发型不在镜头间大幅变化。",
            "signature_detail": signature,
            "body_language": "紧张时肩颈微收，做决定时视线稳定向前。",
        }

    return {
        "age_range": "未定，按后续剧本补齐",
        "gender_expression": gender,
        "silhouette": "与主角明显区分的轮廓，避免误认。",
        "face_hair": "只锁定必要识别点，保留后续扩展空间。",
        "signature_detail": supporting_signature,
        "body_language": "出现或被提及时应带来情绪压力，而不是普通背景信息。",
    }


def build_costume(is_main: bool, text: str) -> dict[str, str]:
    rain_layer = "带轻微雨水反光的外套" if "雨" in text else "适合移动和近景表演的外套"

    if is_main:
        return {
            "base": rain_layer,
            "palette": "低饱和深色为主，保留一个小面积识别色。",
            "lock_rule": "同一集内主色、衣领形状和袖口细节保持一致。",
            "change_policy": "除非用户审核后改设定，否则不在分镜间换装。",
        }

    return {
        "base": "与剧情线索相关的简洁服装或残留物件。",
        "palette": "弱化饱和度，避免压过主角。",
        "lock_rule": "每次出现都保留同一个可识别细节。",
        "change_policy": "可在后续集数扩展，但本集保持统一。",
    }


def build_voice_profile(name: str, is_main: bool) -> dict[str, str]:
    if is_main:
        return {
            "tone": "克制、清晰、紧张时不拖长尾音。",
            "pace": "短句优先，给竖屏字幕留出阅读空间。",
            "delivery": f"{name}的对白要从试探逐步转向坚定。",
        }

    return {
        "tone": "可由旁白或记忆感对白承载，保持距离感。",
        "pace": "信息短促，不展开解释。",
        "delivery": f"{name}的声音方向服务悬念，不替代主角做决定。",
    }


def build_must_keep_rules(name: str, is_main: bool, anchors: list[str]) -> list[str]:
    rules = [
        f"{name}的脸型、发型、服装主色和体态在所有镜头保持一致。",
        "角色入画时优先保证可识别轮廓，再处理环境氛围。",
    ]

    if is_main and anchors:
        rules.append(f"主角与核心线索同框时保留：{anchors[0]}。")

    if is_main and len(anchors) > 1:
        rules.append(f"环境识别点可复用：{anchors[1]}。")

    return rules


def build_expression_palette(is_main: bool) -> list[dict[str, str]]:
    if is_main:
        return [
            {"name": "alert", "description": "眉眼收紧，先观察再行动。"},
            {"name": "doubt", "description": "视线偏移，嘴角收住，不夸张。"},
            {"name": "resolve", "description": "眼神稳定，动作放慢半拍。"},
        ]

    return [
        {"name": "absence", "description": "可通过物件、背影或声音暗示。"},
        {"name": "pressure", "description": "出现时让画面信息更紧，而不是更吵。"},
    ]


def build_character_continuity_rules(is_main: bool) -> list[str]:
    rules = [
        "同一角色跨镜头保持相同的基础比例、发型轮廓和服装结构。",
        "除用户审核修改外，不新增会改变身份识别的装饰。",
    ]

    if is_main:
        rules.append("主角近景、中景、背影都必须能通过服装主色和体态识别。")

    return rules


def build_relationship_notes(characters: list[dict[str, Any]]) -> list[str]:
    if len(characters) == 1:
        return ["当前脚本只显式识别到主角；其他关系角色可在后续审核中补充。"]

    protagonist = characters[0]["name"]
    return [
        f"{protagonist}是观众跟随视角，其余角色必须围绕线索、压力或情绪牵引出现。",
        "关系描述先服务本集短视频节奏，避免提前展开长篇人物史。",
    ]


def build_global_continuity_rules(episode: dict[str, Any], characters: list[dict[str, Any]]) -> list[str]:
    names = "、".join(character["name"] for character in characters)
    return [
        f"本集角色锁定范围：{names}。",
        "进入画风、分镜、图像或视频提示词阶段时，必须引用 character_id 和 stable_seed。",
        f"角色设定来自 episode_writer《{episode['title']}》，后续持久化由数据库 adapter 阶段处理。",
    ]


def extract_visual_anchors(text: str) -> list[str]:
    candidates = [
        ("纸鹤", "发光纸鹤作为核心识别道具"),
        ("雨", "雨水反光与湿润发梢"),
        ("胶片", "旧胶片卷和冲洗痕迹"),
        ("照相馆", "废弃照相馆的暗房质感"),
        ("门", "午夜才出现的门形光源"),
    ]
    anchors = [description for keyword, description in candidates if keyword in text]
    return anchors or ["核心线索道具与角色手部动作同框"]


def infer_gender(name: str, text: str) -> str:
    name_index = text.find(name)
    if name_index >= 0:
        window = text[max(0, name_index - 24) : name_index + 48]
        if "她" in window:
            return "female-presenting"
        if "他" in window:
            return "male-presenting"

    if "她" in text and name not in KINSHIP_CANDIDATES:
        return "female-presenting"

    if name in ("哥哥", "弟弟", "父亲"):
        return "male-presenting"

    if name in ("姐姐", "妹妹", "母亲"):
        return "female-presenting"

    return "unspecified"


def collect_source_evidence(name: str, episode: dict[str, Any]) -> list[str]:
    evidence: list[str] = []
    for segment in episode.get("segments", []):
        for dialogue in segment.get("dialogue_lines", []):
            if dialogue.get("speaker") == name:
                evidence.append(f"{segment['segment_id']} dialogue: {dialogue.get('line', '')}")

        for field in ("narration", "visual_direction"):
            value = segment.get(field, "")
            if isinstance(value, str) and name in value:
                evidence.append(f"{segment['segment_id']} {field}: {value}")

    if evidence:
        return evidence[:3]

    text = combined_episode_text(episode)
    position = text.find(name)
    if position >= 0:
        start = max(0, position - 18)
        end = min(len(text), position + len(name) + 32)
        return [text[start:end].strip()]

    return ["来自 episode_writer 脚本结构，等待用户审核补充。"]


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

