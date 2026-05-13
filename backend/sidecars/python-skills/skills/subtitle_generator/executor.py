from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    episode = normalized["episode_writer"]
    storyboard = normalized["storyboard_director"]
    voice_casting = normalized["voice_casting"]
    subtitle_cues = build_subtitle_cues(voice_casting)

    data = {
        "project_id": episode["project_id"],
        "episode_index": episode["episode_index"],
        "title": episode["title"],
        "language": episode["language"],
        "provider": "none",
        "model": "none",
        "mode": "deterministic-srt-plan",
        "subtitle_cues": subtitle_cues,
        "srt_text": build_srt_text(subtitle_cues),
        "subtitle_manifest": build_subtitle_manifest(episode, storyboard, subtitle_cues),
        "generation_plan": {
            "mode": "srt_structure_boundary",
            "provider": "none",
            "model": "none",
            "cue_count": len(subtitle_cues),
            "writes_files": False,
            "writes_database": False,
            "uses_audio_alignment": False,
            "notes": [
                "Stage 10 builds SRT-ready structures from script and voice timing.",
                "Real subtitle file writes, audio alignment, and persistence stay deferred to later adapters.",
            ],
        },
        "review": {
            "status": "ready_for_edit_plan",
            "editable_fields": ["subtitle_cues", "srt_text"],
            "notes": [
                "Subtitle timing follows voice_casting tasks and can be consumed by auto_editor.",
                "This output writes no SRT file, no database rows, and calls no FFmpeg or alignment service.",
            ],
        },
        "checkpoint": {
            "required": False,
            "policy": "auto_continue_to_edit_plan",
            "reason": "subtitle structures can feed auto_editor without user pause in Stage 10.",
        },
    }

    return validate_output(data)


def build_subtitle_cues(voice_casting: dict[str, Any]) -> list[dict[str, Any]]:
    cues: list[dict[str, Any]] = []
    for index, task in enumerate(voice_casting["voice_tasks"], start=1):
        cues.append(
            {
                "cue_id": f"subtitle_{index:03d}",
                "index": index,
                "start_second": int(task["start_second"]),
                "end_second": int(task["end_second"]),
                "text": trim_subtitle_text(str(task["text"])),
                "source": task["source_type"],
                "source_task_id": task["task_id"],
                "source_segment_id": task["source_segment_id"],
                "speaker": task["speaker"],
                "target_shot_ids": task.get("target_shot_ids", []),
                "line_break_policy": "single_line_mobile_safe",
            }
        )
    return cues


def build_srt_text(cues: list[dict[str, Any]]) -> str:
    blocks = []
    for cue in cues:
        blocks.append(
            "\n".join(
                [
                    str(cue["index"]),
                    f"{format_srt_time(cue['start_second'])} --> {format_srt_time(cue['end_second'])}",
                    cue["text"],
                ]
            )
        )
    return "\n\n".join(blocks)


def format_srt_time(seconds: int) -> str:
    hours = seconds // 3600
    minutes = (seconds % 3600) // 60
    whole_seconds = seconds % 60
    return f"{hours:02d}:{minutes:02d}:{whole_seconds:02d},000"


def trim_subtitle_text(value: str) -> str:
    cleaned = " ".join(value.strip().split())
    if len(cleaned) <= 32:
        return cleaned
    return cleaned[:29].rstrip("，,。 ") + "..."


def build_subtitle_manifest(
    episode: dict[str, Any],
    storyboard: dict[str, Any],
    subtitle_cues: list[dict[str, Any]],
) -> dict[str, Any]:
    target_seconds = storyboard.get("timing_summary", {}).get("total_shot_seconds", episode.get("target_duration_seconds", 0))
    return {
        "cue_count": len(subtitle_cues),
        "target_duration_seconds": target_seconds,
        "coverage_start_second": subtitle_cues[0]["start_second"] if subtitle_cues else 0,
        "coverage_end_second": subtitle_cues[-1]["end_second"] if subtitle_cues else 0,
        "subtitle_uri": f"milu://mock-assets/{episode['project_id']}/subtitles_episode_{episode['episode_index']:02d}.srt",
        "file_written": False,
        "writes_files": False,
        "writes_database": False,
        "notes": [
            "srt_text is a review preview and does not mean an SRT file exists.",
            "Subtitle persistence and download assets stay deferred to storage and database adapters.",
        ],
    }
