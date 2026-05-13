from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    video_prompt_builder = normalized["video_prompt_builder"]
    clips = build_mock_clips(video_prompt_builder)

    data = {
        "project_id": video_prompt_builder["project_id"],
        "episode_index": video_prompt_builder["episode_index"],
        "title": video_prompt_builder["title"],
        "language": video_prompt_builder["language"],
        "provider": "mock",
        "model": "none",
        "mode": "deterministic-placeholder",
        "clips": clips,
        "clip_manifest": build_clip_manifest(video_prompt_builder, clips),
        "cost_estimate": {
            "provider": "mock",
            "currency": "USD",
            "unit": "video_second",
            "quantity": sum(clip["duration_seconds"] for clip in clips),
            "estimated_cost": 0,
            "actual_cost": 0,
        },
        "review": {
            "status": "ready_for_video_review",
            "editable_fields": ["clips", "clip_manifest"],
            "notes": [
                "Stage 9 emits mock placeholder video clip records, not real MP4 files.",
                "Real video provider calls, retries, persistence, previews, and FFmpeg work stay behind later adapters.",
            ],
        },
        "checkpoint": {
            "required": True,
            "policy": "pause_for_video_review",
            "reason": "video_generation output represents the video review boundary, even when clips are mock placeholders.",
        },
    }

    return validate_output(data)


def build_mock_clips(video_prompt_builder: dict[str, Any]) -> list[dict[str, Any]]:
    clips = []
    project_id = video_prompt_builder["project_id"]
    episode_index = video_prompt_builder["episode_index"]

    for index, request in enumerate(video_prompt_builder["video_requests"], start=1):
        clip_id = f"mock_video_{episode_index:02d}_{index:03d}"
        clips.append(
            {
                "clip_id": clip_id,
                "request_id": request["request_id"],
                "shot_id": request["shot_id"],
                "shot_index": request["shot_index"],
                "duration_seconds": request["duration_seconds"],
                "status": "mock_ready",
                "provider": "mock",
                "model": "none",
                "generation_mode": request["generation_mode"],
                "prompt": request["prompt"],
                "negative_prompt": request["negative_prompt"],
                "source_images": request["source_images"],
                "aspect_ratio": request["aspect_ratio"],
                "selected": bool(request.get("output_slot", {}).get("selected", True)),
                "file_written": False,
                "asset_uri": f"milu://mock-assets/{project_id}/{clip_id}.mp4",
                "storage_intent": {
                    "root": "D:\\code\\MiLuStudio\\storage",
                    "relative_path": f"projects/{project_id}/videos/{clip_id}.mp4",
                    "will_write_in_future_adapter": True,
                },
                "metadata": {
                    "seed_hint": request["seed_hint"],
                    "kind": request.get("output_slot", {}).get("kind", "video"),
                    "intended_use": request.get("output_slot", {}).get("intended_use", "video_clip"),
                    "source": "deterministic_mock_no_file",
                    "frame_count_estimate": request["duration_seconds"] * 24,
                },
            }
        )

    return clips


def build_clip_manifest(video_prompt_builder: dict[str, Any], clips: list[dict[str, Any]]) -> dict[str, Any]:
    by_shot = {clip["shot_id"]: clip["clip_id"] for clip in clips}
    total_duration_seconds = sum(clip["duration_seconds"] for clip in clips)

    return {
        "prompt_set_id": video_prompt_builder["prompt_set_id"],
        "clip_count": len(clips),
        "total_duration_seconds": total_duration_seconds,
        "by_shot": by_shot,
        "writes_files": False,
        "writes_database": False,
        "uses_ffmpeg": False,
        "notes": [
            "asset_uri is a logical placeholder and does not mean a real MP4 exists.",
            "storage_intent describes a later adapter target and does not write to the file system in this stage.",
        ],
    }
