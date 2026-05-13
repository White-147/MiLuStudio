from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    storyboard = normalized["storyboard_director"]
    video_generation = normalized["video_generation"]
    voice_casting = normalized["voice_casting"]
    subtitle_generator = normalized["subtitle_generator"]
    timeline = build_timeline(storyboard, video_generation, voice_casting, subtitle_generator)

    data = {
        "project_id": storyboard["project_id"],
        "episode_index": storyboard["episode_index"],
        "title": storyboard["title"],
        "language": storyboard["language"],
        "provider": "none",
        "model": "none",
        "mode": "deterministic-edit-plan",
        "timeline": timeline,
        "render_plan": build_render_plan(storyboard, timeline),
        "edit_manifest": build_edit_manifest(storyboard, video_generation, voice_casting, subtitle_generator, timeline),
        "cost_estimate": {
            "provider": "none",
            "currency": "USD",
            "unit": "render_second",
            "quantity": timeline["total_duration_seconds"],
            "estimated_cost": 0,
            "actual_cost": 0,
        },
        "review": {
            "status": "ready_for_quality_check_boundary",
            "editable_fields": ["timeline", "render_plan"],
            "notes": [
                "Stage 10 emits a rough edit plan only; no media muxing or rendering is performed.",
                "Real FFmpeg, file writes, preview playback, and persistence stay behind later adapters.",
            ],
        },
        "checkpoint": {
            "required": False,
            "policy": "auto_continue_to_quality_check",
            "reason": "auto_editor produces a deterministic plan that can feed a later quality checker boundary.",
        },
    }

    return validate_output(data)


def build_timeline(
    storyboard: dict[str, Any],
    video_generation: dict[str, Any],
    voice_casting: dict[str, Any],
    subtitle_generator: dict[str, Any],
) -> dict[str, Any]:
    clips_by_shot = {clip["shot_id"]: clip for clip in video_generation["clips"]}
    video_track = []

    for shot in storyboard["shots"]:
        clip = clips_by_shot.get(shot["shot_id"])
        if not clip:
            continue
        start_second = int(shot["start_second"])
        duration_seconds = int(clip["duration_seconds"])
        video_track.append(
            {
                "timeline_item_id": f"video_{shot['shot_id']}",
                "clip_id": clip["clip_id"],
                "shot_id": shot["shot_id"],
                "start_second": start_second,
                "end_second": start_second + duration_seconds,
                "duration_seconds": duration_seconds,
                "asset_uri": clip["asset_uri"],
                "file_written": clip["file_written"],
                "transition_in": "cut",
                "transition_out": "cut",
            }
        )

    audio_track = [
        {
            "timeline_item_id": f"audio_{task['task_id']}",
            "task_id": task["task_id"],
            "source_segment_id": task["source_segment_id"],
            "source_type": task["source_type"],
            "speaker": task["speaker"],
            "start_second": task["start_second"],
            "end_second": task["end_second"],
            "duration_seconds": task["duration_seconds"],
            "asset_uri": task["output_audio_intent"]["asset_uri"],
            "file_written": task["output_audio_intent"]["file_written"],
            "mix_level_db": -3 if task["source_type"] == "dialogue" else -6,
        }
        for task in voice_casting["voice_tasks"]
    ]

    subtitle_track = [
        {
            "timeline_item_id": f"subtitle_{cue['cue_id']}",
            "cue_id": cue["cue_id"],
            "start_second": cue["start_second"],
            "end_second": cue["end_second"],
            "text": cue["text"],
            "target_shot_ids": cue.get("target_shot_ids", []),
            "placement": "lower_third_safe_area",
        }
        for cue in subtitle_generator["subtitle_cues"]
    ]

    total_duration_seconds = storyboard.get("timing_summary", {}).get(
        "total_shot_seconds",
        sum(item["duration_seconds"] for item in video_track),
    )

    return {
        "timeline_id": f"{storyboard['project_id']}:episode_{storyboard['episode_index']}:rough_edit",
        "aspect_ratio": storyboard["aspect_ratio"],
        "fps": 24,
        "total_duration_seconds": total_duration_seconds,
        "tracks": {
            "video": video_track,
            "audio": audio_track,
            "subtitles": subtitle_track,
        },
    }


def build_render_plan(storyboard: dict[str, Any], timeline: dict[str, Any]) -> dict[str, Any]:
    project_id = storyboard["project_id"]
    episode_index = storyboard["episode_index"]

    return {
        "engine": "none",
        "uses_ffmpeg": False,
        "writes_files": False,
        "writes_database": False,
        "output_intent": {
            "kind": "video",
            "intended_use": "rough_cut_mp4",
            "asset_uri": f"milu://mock-assets/{project_id}/rough_cut_episode_{episode_index:02d}.mp4",
            "file_written": False,
            "storage_intent": {
                "root": "D:\\code\\MiLuStudio\\storage",
                "relative_path": f"projects/{project_id}/exports/rough_cut_episode_{episode_index:02d}.mp4",
                "will_write_in_future_adapter": True,
            },
        },
        "inputs": {
            "video_track_count": len(timeline["tracks"]["video"]),
            "audio_track_count": len(timeline["tracks"]["audio"]),
            "subtitle_cue_count": len(timeline["tracks"]["subtitles"]),
        },
        "notes": [
            "This is a render plan only and does not invoke FFmpeg.",
            "Future edit adapters can translate the timeline into FFmpeg or another Windows-local render provider.",
        ],
    }


def build_edit_manifest(
    storyboard: dict[str, Any],
    video_generation: dict[str, Any],
    voice_casting: dict[str, Any],
    subtitle_generator: dict[str, Any],
    timeline: dict[str, Any],
) -> dict[str, Any]:
    return {
        "timeline_id": timeline["timeline_id"],
        "shot_count": len(storyboard["shots"]),
        "video_clip_count": len(video_generation["clips"]),
        "voice_task_count": len(voice_casting["voice_tasks"]),
        "subtitle_cue_count": len(subtitle_generator["subtitle_cues"]),
        "total_duration_seconds": timeline["total_duration_seconds"],
        "writes_files": False,
        "writes_database": False,
        "uses_ffmpeg": False,
        "notes": [
            "All media URIs are logical placeholders until real adapters write files.",
            "The rough edit plan can be stored later by PostgreSQL / EF Core adapters without changing skill outputs.",
        ],
    }
