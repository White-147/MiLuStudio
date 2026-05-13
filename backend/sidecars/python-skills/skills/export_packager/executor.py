from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    auto_editor = normalized["auto_editor"]
    quality_checker = normalized["quality_checker"]
    subtitle_generator = normalized["subtitle_generator"]
    video_generation = normalized["video_generation"]

    project_id = auto_editor["project_id"]
    episode_index = auto_editor["episode_index"]
    package_id = f"export_{episode_index:02d}_{project_id}"
    total_seconds = auto_editor.get("timeline", {}).get("total_duration_seconds", 0)

    data = {
        "project_id": project_id,
        "episode_index": episode_index,
        "title": auto_editor["title"],
        "language": auto_editor["language"],
        "provider": "none",
        "model": "none",
        "mode": "deterministic-export-boundary",
        "package_id": package_id,
        "quality_status": quality_checker.get("quality_status", "needs_review"),
        "delivery_assets": build_delivery_assets(project_id, package_id, total_seconds, subtitle_generator, video_generation),
        "export_manifest": {
            "package_id": package_id,
            "source_timeline_id": auto_editor.get("timeline", {}).get("timeline_id"),
            "source_quality_status": quality_checker.get("quality_status"),
            "total_duration_seconds": total_seconds,
            "asset_count": 4,
            "writes_files": False,
            "writes_database": False,
            "reads_media_files": False,
            "uses_ffmpeg": False,
            "notes": [
                "Stage 13 export_packager emits a database-ready placeholder package only.",
                "Real ZIP, MP4, SRT file writing and FFmpeg rendering stay behind later adapters.",
            ],
        },
        "review": {
            "status": "export_placeholder_ready",
            "editable_fields": ["delivery_assets", "export_manifest"],
            "notes": [
                "No files are written in this deterministic mock boundary.",
                "The package structure is intended for later storage and render adapters.",
            ],
        },
        "checkpoint": {
            "required": False,
            "policy": "no_checkpoint_for_placeholder_export",
            "reason": "export_packager only creates a placeholder structure in Stage 13.",
        },
    }

    return validate_output(data)


def build_delivery_assets(
    project_id: str,
    package_id: str,
    total_seconds: int,
    subtitle_generator: dict[str, Any],
    video_generation: dict[str, Any],
) -> list[dict[str, Any]]:
    clip_count = len(video_generation.get("clips", []))
    subtitle_count = len(subtitle_generator.get("subtitle_cues", []))

    return [
        {
            "asset_id": f"{package_id}_mp4",
            "kind": "video",
            "label": "占位成片 MP4",
            "format": "mp4",
            "logical_uri": f"milu://mock-assets/{project_id}/exports/{package_id}.mp4",
            "file_written": False,
            "duration_seconds": total_seconds,
            "source_count": clip_count,
        },
        {
            "asset_id": f"{package_id}_srt",
            "kind": "subtitle",
            "label": "占位字幕 SRT",
            "format": "srt",
            "logical_uri": f"milu://mock-assets/{project_id}/exports/{package_id}.srt",
            "file_written": False,
            "cue_count": subtitle_count,
        },
        {
            "asset_id": f"{package_id}_manifest",
            "kind": "text",
            "label": "导出清单 JSON",
            "format": "json",
            "logical_uri": f"milu://mock-assets/{project_id}/exports/{package_id}.json",
            "file_written": False,
        },
        {
            "asset_id": f"{package_id}_archive",
            "kind": "archive",
            "label": "占位素材包 ZIP",
            "format": "zip",
            "logical_uri": f"milu://mock-assets/{project_id}/exports/{package_id}.zip",
            "file_written": False,
        },
    ]
