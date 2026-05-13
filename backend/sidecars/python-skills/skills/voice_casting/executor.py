from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    episode = normalized["episode_writer"]
    storyboard = normalized["storyboard_director"]
    voice_profiles = build_voice_profiles(episode)
    voice_tasks = build_voice_tasks(episode, storyboard, voice_profiles)

    data = {
        "project_id": episode["project_id"],
        "episode_index": episode["episode_index"],
        "title": episode["title"],
        "language": episode["language"],
        "provider": "none",
        "model": "none",
        "mode": "deterministic-plan",
        "voice_profiles": voice_profiles,
        "voice_tasks": voice_tasks,
        "voice_manifest": build_voice_manifest(episode, voice_profiles, voice_tasks),
        "generation_plan": {
            "mode": "tts_request_boundary",
            "provider": "none",
            "model": "none",
            "request_count": len(voice_tasks),
            "writes_files": False,
            "writes_database": False,
            "notes": [
                "Stage 10 only builds voice casting and TTS task requests.",
                "Real TTS providers, audio files, retries, and persistence stay behind later adapters.",
            ],
        },
        "cost_estimate": {
            "provider": "none",
            "currency": "USD",
            "unit": "audio_second",
            "quantity": sum(task["duration_seconds"] for task in voice_tasks),
            "estimated_cost": 0,
            "actual_cost": 0,
        },
        "review": {
            "status": "ready_for_voice_review",
            "editable_fields": ["voice_profiles", "voice_tasks"],
            "notes": [
                "Voice profiles and task timing can be reviewed before real TTS is introduced.",
                "This output writes no audio files, no database rows, and calls no TTS provider.",
            ],
        },
        "checkpoint": {
            "required": True,
            "policy": "pause_for_voice_cast_review",
            "reason": "voice_casting chooses narrator and speaker voices before downstream audio tasks.",
        },
    }

    return validate_output(data)


def build_voice_profiles(episode: dict[str, Any]) -> list[dict[str, Any]]:
    profiles = [
        {
            "voice_profile_id": "voice_narrator",
            "speaker": "旁白",
            "role": "narrator",
            "tone": episode.get("voice_notes", {}).get("narration", "清晰、克制。"),
            "gender_hint": "unspecified",
            "age_hint": "adult",
            "provider_voice_id": None,
            "locked": False,
        }
    ]
    speakers = collect_dialogue_speakers(episode)

    for index, speaker in enumerate(speakers, start=1):
        profiles.append(
            {
                "voice_profile_id": f"voice_speaker_{index:02d}",
                "speaker": speaker,
                "role": "dialogue",
                "tone": episode.get("voice_notes", {}).get("dialogue", "对白短促，优先服务画面节奏。"),
                "gender_hint": "unspecified",
                "age_hint": "adult",
                "provider_voice_id": None,
                "locked": False,
            }
        )

    return profiles


def collect_dialogue_speakers(episode: dict[str, Any]) -> list[str]:
    speakers: list[str] = []
    for segment in episode.get("segments", []):
        for dialogue in segment.get("dialogue_lines", []):
            speaker = str(dialogue.get("speaker", "")).strip()
            if speaker and speaker not in speakers:
                speakers.append(speaker)
    return speakers


def build_voice_tasks(
    episode: dict[str, Any],
    storyboard: dict[str, Any],
    voice_profiles: list[dict[str, Any]],
) -> list[dict[str, Any]]:
    profile_by_speaker = {profile["speaker"]: profile for profile in voice_profiles}
    shots_by_segment = index_shots_by_segment(storyboard)
    tasks: list[dict[str, Any]] = []

    for segment in episode["segments"]:
        segment_start = int(segment["start_second"])
        segment_duration = int(segment["duration_seconds"])
        cursor = segment_start
        narration = str(segment.get("narration", "")).strip()
        dialogue_lines = [line for line in segment.get("dialogue_lines", []) if str(line.get("line", "")).strip()]
        target_shot_ids = [shot["shot_id"] for shot in shots_by_segment.get(segment["segment_id"], [])]

        if narration:
            reserved = max(1, segment_duration // 2) if dialogue_lines else segment_duration
            end_second = min(segment_start + segment_duration, cursor + reserved)
            tasks.append(
                build_voice_task(
                    episode=episode,
                    segment=segment,
                    index=len(tasks) + 1,
                    source_type="narration",
                    speaker="旁白",
                    profile=profile_by_speaker["旁白"],
                    text=narration,
                    delivery=episode.get("voice_notes", {}).get("narration", ""),
                    start_second=cursor,
                    end_second=end_second,
                    target_shot_ids=target_shot_ids,
                )
            )
            cursor = end_second

        remaining = max(1, segment_start + segment_duration - cursor)
        per_dialogue = max(1, remaining // max(1, len(dialogue_lines)))
        for dialogue_index, dialogue in enumerate(dialogue_lines):
            speaker = str(dialogue.get("speaker", "")).strip() or "角色"
            profile = profile_by_speaker.get(speaker, profile_by_speaker["旁白"])
            is_last = dialogue_index == len(dialogue_lines) - 1
            end_second = segment_start + segment_duration if is_last else min(segment_start + segment_duration, cursor + per_dialogue)
            tasks.append(
                build_voice_task(
                    episode=episode,
                    segment=segment,
                    index=len(tasks) + 1,
                    source_type="dialogue",
                    speaker=speaker,
                    profile=profile,
                    text=str(dialogue.get("line", "")).strip(),
                    delivery=str(dialogue.get("delivery", "")).strip(),
                    start_second=cursor,
                    end_second=end_second,
                    target_shot_ids=target_shot_ids,
                )
            )
            cursor = end_second

    return tasks


def build_voice_task(
    episode: dict[str, Any],
    segment: dict[str, Any],
    index: int,
    source_type: str,
    speaker: str,
    profile: dict[str, Any],
    text: str,
    delivery: str,
    start_second: int,
    end_second: int,
    target_shot_ids: list[str],
) -> dict[str, Any]:
    duration_seconds = max(1, end_second - start_second)
    task_id = f"voice_task_{index:03d}"

    return {
        "task_id": task_id,
        "source_segment_id": segment["segment_id"],
        "source_type": source_type,
        "speaker": speaker,
        "voice_profile_id": profile["voice_profile_id"],
        "text": text,
        "delivery": delivery,
        "start_second": start_second,
        "end_second": start_second + duration_seconds,
        "duration_seconds": duration_seconds,
        "target_shot_ids": target_shot_ids,
        "provider": "none",
        "model": "none",
        "output_audio_intent": {
            "kind": "audio",
            "intended_use": source_type,
            "asset_uri": f"milu://mock-assets/{episode['project_id']}/{task_id}.wav",
            "file_written": False,
            "storage_intent": {
                "root": "D:\\code\\MiLuStudio\\storage",
                "relative_path": f"projects/{episode['project_id']}/audio/{task_id}.wav",
                "will_write_in_future_adapter": True,
            },
        },
    }


def index_shots_by_segment(storyboard: dict[str, Any]) -> dict[str, list[dict[str, Any]]]:
    by_segment: dict[str, list[dict[str, Any]]] = {}
    for shot in storyboard.get("shots", []):
        segment_id = shot.get("source_segment_id")
        if segment_id:
            by_segment.setdefault(segment_id, []).append(shot)
    return by_segment


def build_voice_manifest(
    episode: dict[str, Any],
    voice_profiles: list[dict[str, Any]],
    voice_tasks: list[dict[str, Any]],
) -> dict[str, Any]:
    return {
        "task_count": len(voice_tasks),
        "profile_count": len(voice_profiles),
        "total_voice_seconds": sum(task["duration_seconds"] for task in voice_tasks),
        "by_segment": group_task_ids_by_segment(voice_tasks),
        "writes_files": False,
        "writes_database": False,
        "notes": [
            "output_audio_intent is a future adapter target and does not mean a WAV exists.",
            "Voice provider mapping and cost accounting stay deferred to the real TTS adapter stage.",
        ],
    }


def group_task_ids_by_segment(voice_tasks: list[dict[str, Any]]) -> dict[str, list[str]]:
    by_segment: dict[str, list[str]] = {}
    for task in voice_tasks:
        by_segment.setdefault(task["source_segment_id"], []).append(task["task_id"])
    return by_segment
