from __future__ import annotations

from typing import Any

from .validators import validate_input, validate_output

SEVERITY_ORDER = {"info": 0, "warning": 1, "error": 2, "blocking": 3}
SCORE_PENALTY = {"info": 1, "warning": 5, "error": 15, "blocking": 30}
MAX_SUBTITLE_CHARS = 32


def run(payload: dict[str, Any]) -> dict[str, Any]:
    normalized = validate_input(payload)
    storyboard = normalized["storyboard_director"]
    issues: list[dict[str, Any]] = []

    context = build_context(normalized)
    check_character_consistency(context, issues)
    check_style_references(context, issues)
    check_storyboard_timing(context, issues)
    check_video_generation(context, issues)
    check_voice_casting(context, issues)
    check_subtitles(context, issues)
    check_auto_editor(context, issues)
    check_boundary_flags(context, issues)

    auto_retry_items = build_auto_retry_items(issues)
    manual_review_items = build_manual_review_items(issues)
    severity_counts = count_severities(issues)

    data = {
        "project_id": storyboard["project_id"],
        "episode_index": storyboard["episode_index"],
        "title": storyboard["title"],
        "language": storyboard["language"],
        "provider": "none",
        "model": "none",
        "mode": "deterministic-quality-boundary",
        "quality_status": derive_quality_status(severity_counts),
        "score": build_score(issues),
        "issues": issues,
        "auto_retry_items": auto_retry_items,
        "manual_review_items": manual_review_items,
        "quality_manifest": build_quality_manifest(context, issues),
        "generation_plan": build_generation_plan(context),
        "review": {
            "status": "ready_for_quality_review",
            "editable_fields": ["issues", "auto_retry_items", "manual_review_items"],
            "notes": [
                "Stage 11 checks structure and boundary flags only; it does not inspect real video or audio files.",
                "Retry items are hints for later Worker orchestration and persistence adapters.",
            ],
        },
        "checkpoint": {
            "required": True,
            "policy": "pause_for_quality_review",
            "reason": "quality_checker is the manual confirmation boundary before retry orchestration or export adapters.",
        },
    }

    return validate_output(data)


def build_context(normalized: dict[str, dict[str, Any]]) -> dict[str, Any]:
    storyboard = normalized["storyboard_director"]
    shots = storyboard["shots"]
    clips = normalized["video_generation"].get("clips", [])
    voice_tasks = normalized["voice_casting"].get("voice_tasks", [])
    subtitle_cues = normalized["subtitle_generator"].get("subtitle_cues", [])
    tracks = normalized["auto_editor"].get("timeline", {}).get("tracks", {})

    return {
        **normalized,
        "shots": shots,
        "shots_by_id": {shot.get("shot_id"): shot for shot in shots if isinstance(shot, dict) and shot.get("shot_id")},
        "clips": clips,
        "clips_by_shot": {clip.get("shot_id"): clip for clip in clips if isinstance(clip, dict) and clip.get("shot_id")},
        "voice_tasks": voice_tasks,
        "voice_tasks_by_id": {
            task.get("task_id"): task for task in voice_tasks if isinstance(task, dict) and task.get("task_id")
        },
        "subtitle_cues": subtitle_cues,
        "subtitle_cues_by_id": {
            cue.get("cue_id"): cue for cue in subtitle_cues if isinstance(cue, dict) and cue.get("cue_id")
        },
        "tracks": tracks,
        "storyboard_total_seconds": int(
            storyboard.get("timing_summary", {}).get(
                "total_shot_seconds",
                sum(int(shot.get("duration_seconds", 0)) for shot in shots if isinstance(shot, dict)),
            )
        ),
        "character_ids": {
            character.get("character_id")
            for character in normalized["character_bible"].get("characters", [])
            if isinstance(character, dict) and character.get("character_id")
        },
        "style_block_keys": set(normalized["style_bible"].get("reusable_prompt_blocks", {}).keys()),
    }


def check_character_consistency(context: dict[str, Any], issues: list[dict[str, Any]]) -> None:
    character_ids = context["character_ids"]

    for shot in context["shots"]:
        shot_id = str(shot.get("shot_id", "unknown_shot"))
        characters = shot.get("characters", [])
        if not isinstance(characters, list) or not characters:
            add_issue(
                issues,
                severity="warning",
                category="character_consistency",
                message="Storyboard shot has no locked character reference.",
                target={"scope": "shot", "id": shot_id},
                evidence={"shot_id": shot_id},
                retryable=True,
                retry_stage="storyboard",
                skill_name="storyboard_director",
                input_refs=[shot_id],
                recommended_action="Regenerate or edit this shot with an explicit character reference.",
            )
            continue

        for character in characters:
            character_id = character.get("character_id") if isinstance(character, dict) else None
            if character_id not in character_ids:
                add_issue(
                    issues,
                    severity="error",
                    category="character_consistency",
                    message="Storyboard shot references a character not present in character_bible.",
                    target={"scope": "shot", "id": shot_id},
                    evidence={"shot_id": shot_id, "character_id": character_id},
                    retryable=True,
                    retry_stage="storyboard",
                    skill_name="storyboard_director",
                    input_refs=[shot_id, str(character_id)],
                    recommended_action="Refresh storyboard character references from the reviewed character bible.",
                )


def check_style_references(context: dict[str, Any], issues: list[dict[str, Any]]) -> None:
    style_block_keys = context["style_block_keys"]

    for shot in context["shots"]:
        shot_id = str(shot.get("shot_id", "unknown_shot"))
        refs = shot.get("style_prompt_block_refs", [])
        if not isinstance(refs, list) or not refs:
            add_issue(
                issues,
                severity="warning",
                category="style_consistency",
                message="Storyboard shot has no reusable style prompt block references.",
                target={"scope": "shot", "id": shot_id},
                evidence={"shot_id": shot_id},
                retryable=True,
                retry_stage="storyboard",
                skill_name="storyboard_director",
                input_refs=[shot_id],
                recommended_action="Attach reusable style prompt block references before video prompt expansion.",
            )
            continue

        missing_refs = [ref for ref in refs if ref not in style_block_keys]
        if missing_refs:
            add_issue(
                issues,
                severity="error",
                category="style_consistency",
                message="Storyboard shot references style prompt blocks not present in style_bible.",
                target={"scope": "shot", "id": shot_id},
                evidence={"shot_id": shot_id, "missing_refs": missing_refs},
                retryable=True,
                retry_stage="storyboard",
                skill_name="storyboard_director",
                input_refs=[shot_id, *missing_refs],
                recommended_action="Rebuild storyboard style references from the reviewed style bible.",
            )


def check_storyboard_timing(context: dict[str, Any], issues: list[dict[str, Any]]) -> None:
    storyboard = context["storyboard_director"]
    timing_summary = storyboard.get("timing_summary", {})

    if timing_summary.get("within_tolerance") is not True:
        add_issue(
            issues,
            severity="warning",
            category="timing",
            message="Storyboard timing summary is outside the target duration tolerance.",
            target={"scope": "storyboard", "id": "timing_summary"},
            evidence={
                "target_duration_seconds": timing_summary.get("target_duration_seconds"),
                "total_shot_seconds": timing_summary.get("total_shot_seconds"),
            },
            retryable=True,
            retry_stage="storyboard",
            skill_name="storyboard_director",
            input_refs=["timing_summary"],
            recommended_action="Rebalance shot durations before downstream generation.",
        )

    previous_end: int | None = None
    for shot in context["shots"]:
        shot_id = str(shot.get("shot_id", "unknown_shot"))
        start_second = as_int(shot.get("start_second"), 0)
        duration_seconds = as_int(shot.get("duration_seconds"), 0)
        end_second = start_second + duration_seconds

        if duration_seconds <= 0:
            add_issue(
                issues,
                severity="blocking",
                category="timing",
                message="Storyboard shot duration must be positive.",
                target={"scope": "shot", "id": shot_id},
                evidence={"duration_seconds": duration_seconds},
                retryable=True,
                retry_stage="storyboard",
                skill_name="storyboard_director",
                input_refs=[shot_id],
                recommended_action="Repair shot duration before any downstream retry.",
            )
        elif duration_seconds < 2:
            add_issue(
                issues,
                severity="warning",
                category="timing",
                message="Storyboard shot is very short for reviewable video and subtitle timing.",
                target={"scope": "shot", "id": shot_id},
                evidence={"duration_seconds": duration_seconds},
                retryable=True,
                retry_stage="storyboard",
                skill_name="storyboard_director",
                input_refs=[shot_id],
                recommended_action="Consider extending this shot or merging it with a neighboring beat.",
            )
        elif duration_seconds > 12:
            add_issue(
                issues,
                severity="warning",
                category="timing",
                message="Storyboard shot is long and may need another storyboard split.",
                target={"scope": "shot", "id": shot_id},
                evidence={"duration_seconds": duration_seconds},
                retryable=True,
                retry_stage="storyboard",
                skill_name="storyboard_director",
                input_refs=[shot_id],
                recommended_action="Split the long beat into smaller shots before video generation.",
            )

        if previous_end is not None and start_second != previous_end:
            add_issue(
                issues,
                severity="warning",
                category="timing",
                message="Storyboard shot starts with a timing gap or overlap.",
                target={"scope": "shot", "id": shot_id},
                evidence={"expected_start_second": previous_end, "actual_start_second": start_second},
                retryable=True,
                retry_stage="storyboard",
                skill_name="storyboard_director",
                input_refs=[shot_id],
                recommended_action="Normalize storyboard shot start times.",
            )
        previous_end = end_second


def check_video_generation(context: dict[str, Any], issues: list[dict[str, Any]]) -> None:
    shots_by_id = context["shots_by_id"]
    clips_by_shot = context["clips_by_shot"]

    if len(context["clips"]) != len(context["shots"]):
        add_issue(
            issues,
            severity="error",
            category="video_clip",
            message="Mock video clip count does not match storyboard shot count.",
            target={"scope": "project", "id": "video_generation.clips"},
            evidence={"shot_count": len(context["shots"]), "clip_count": len(context["clips"])},
            retryable=True,
            retry_stage="video_generation",
            skill_name="video_generation",
            input_refs=["clips"],
            recommended_action="Regenerate mock video clip records from the current video prompt set.",
        )

    for shot_id, shot in shots_by_id.items():
        clip = clips_by_shot.get(shot_id)
        if not clip:
            add_issue(
                issues,
                severity="error",
                category="video_clip",
                message="Storyboard shot has no matching mock video clip.",
                target={"scope": "shot", "id": str(shot_id)},
                evidence={"shot_id": shot_id},
                retryable=True,
                retry_stage="video_generation",
                skill_name="video_generation",
                input_refs=[str(shot_id)],
                recommended_action="Retry mock video generation for the missing shot.",
            )
            continue

        clip_duration = as_int(clip.get("duration_seconds"), 0)
        shot_duration = as_int(shot.get("duration_seconds"), 0)
        if clip_duration != shot_duration:
            add_issue(
                issues,
                severity="error",
                category="video_clip",
                message="Mock video clip duration does not match storyboard shot duration.",
                target={"scope": "shot", "id": str(shot_id)},
                evidence={"shot_duration_seconds": shot_duration, "clip_duration_seconds": clip_duration},
                retryable=True,
                retry_stage="video_generation",
                skill_name="video_generation",
                input_refs=[str(shot_id), str(clip.get("clip_id", ""))],
                recommended_action="Regenerate the clip record from the current storyboard timing.",
            )

    for clip in context["clips"]:
        clip_id = str(clip.get("clip_id", "unknown_clip"))
        shot_id = clip.get("shot_id")
        if shot_id not in shots_by_id:
            add_issue(
                issues,
                severity="error",
                category="video_clip",
                message="Mock video clip points to an unknown storyboard shot.",
                target={"scope": "clip", "id": clip_id},
                evidence={"clip_id": clip_id, "shot_id": shot_id},
                retryable=True,
                retry_stage="video_generation",
                skill_name="video_generation",
                input_refs=[clip_id],
                recommended_action="Drop or regenerate the orphan mock clip record.",
            )

        if clip.get("provider") != "mock" or clip.get("model") != "none":
            add_issue(
                issues,
                severity="blocking",
                category="asset_boundary",
                message="Stage 11 accepts only mock video_generation clips with model=none.",
                target={"scope": "clip", "id": clip_id},
                evidence={"provider": clip.get("provider"), "model": clip.get("model")},
                retryable=False,
                recommended_action="Return to the mock video_generation boundary; do not connect real models in Stage 11.",
            )

        if clip.get("file_written") is not False:
            add_boundary_issue(
                issues,
                "video_generation.clips.file_written",
                {"clip_id": clip_id, "file_written": clip.get("file_written")},
            )

        if not is_mock_asset_uri(clip.get("asset_uri")):
            add_issue(
                issues,
                severity="warning",
                category="asset_boundary",
                message="Mock video clip asset_uri should be a logical milu://mock-assets URI.",
                target={"scope": "clip", "id": clip_id},
                evidence={"asset_uri": clip.get("asset_uri")},
                retryable=True,
                retry_stage="video_generation",
                skill_name="video_generation",
                input_refs=[clip_id],
                recommended_action="Regenerate placeholder clip metadata with a logical mock asset URI.",
            )


def check_voice_casting(context: dict[str, Any], issues: list[dict[str, Any]]) -> None:
    shot_ids = set(context["shots_by_id"])
    total_seconds = context["storyboard_total_seconds"]

    if not context["voice_tasks"]:
        add_issue(
            issues,
            severity="error",
            category="audio",
            message="Voice casting produced no voice tasks.",
            target={"scope": "project", "id": "voice_casting.voice_tasks"},
            evidence={"voice_task_count": 0},
            retryable=True,
            retry_stage="voice_casting",
            skill_name="voice_casting",
            input_refs=["voice_tasks"],
            recommended_action="Build voice tasks from the reviewed script and storyboard timing.",
        )

    for task in context["voice_tasks"]:
        task_id = str(task.get("task_id", "unknown_voice_task"))
        start_second = as_int(task.get("start_second"), 0)
        end_second = as_int(task.get("end_second"), 0)
        target_shot_ids = task.get("target_shot_ids", [])

        if task.get("provider") != "none" or task.get("model") != "none":
            add_issue(
                issues,
                severity="blocking",
                category="asset_boundary",
                message="Stage 11 accepts only voice task requests with provider=none and model=none.",
                target={"scope": "voice_task", "id": task_id},
                evidence={"provider": task.get("provider"), "model": task.get("model")},
                retryable=False,
                recommended_action="Keep real TTS provider selection behind a later adapter.",
            )

        if end_second <= start_second:
            add_issue(
                issues,
                severity="error",
                category="audio",
                message="Voice task timing must have end_second greater than start_second.",
                target={"scope": "voice_task", "id": task_id},
                evidence={"start_second": start_second, "end_second": end_second},
                retryable=True,
                retry_stage="voice_casting",
                skill_name="voice_casting",
                input_refs=[task_id],
                recommended_action="Regenerate voice timing for this task.",
            )

        if end_second > total_seconds:
            add_issue(
                issues,
                severity="warning",
                category="audio",
                message="Voice task extends beyond storyboard total duration.",
                target={"scope": "voice_task", "id": task_id},
                evidence={"end_second": end_second, "storyboard_total_seconds": total_seconds},
                retryable=True,
                retry_stage="voice_casting",
                skill_name="voice_casting",
                input_refs=[task_id],
                recommended_action="Clamp voice timing to storyboard duration.",
            )

        if not isinstance(target_shot_ids, list) or not target_shot_ids:
            add_issue(
                issues,
                severity="warning",
                category="audio",
                message="Voice task has no target storyboard shot references.",
                target={"scope": "voice_task", "id": task_id},
                evidence={"target_shot_ids": target_shot_ids},
                retryable=True,
                retry_stage="voice_casting",
                skill_name="voice_casting",
                input_refs=[task_id],
                recommended_action="Attach voice task timing to one or more storyboard shots.",
            )
        else:
            missing_shots = [shot_id for shot_id in target_shot_ids if shot_id not in shot_ids]
            if missing_shots:
                add_issue(
                    issues,
                    severity="error",
                    category="audio",
                    message="Voice task references unknown storyboard shots.",
                    target={"scope": "voice_task", "id": task_id},
                    evidence={"missing_shot_ids": missing_shots},
                    retryable=True,
                    retry_stage="voice_casting",
                    skill_name="voice_casting",
                    input_refs=[task_id, *missing_shots],
                    recommended_action="Rebuild voice task shot references from the current storyboard.",
                )

        output_audio_intent = task.get("output_audio_intent", {})
        if isinstance(output_audio_intent, dict) and output_audio_intent.get("file_written") is not False:
            add_boundary_issue(
                issues,
                "voice_casting.output_audio_intent.file_written",
                {"task_id": task_id, "file_written": output_audio_intent.get("file_written")},
            )


def check_subtitles(context: dict[str, Any], issues: list[dict[str, Any]]) -> None:
    shot_ids = set(context["shots_by_id"])
    total_seconds = context["storyboard_total_seconds"]

    if not context["subtitle_cues"]:
        add_issue(
            issues,
            severity="error",
            category="subtitle",
            message="Subtitle generator produced no subtitle cues.",
            target={"scope": "project", "id": "subtitle_generator.subtitle_cues"},
            evidence={"subtitle_cue_count": 0},
            retryable=True,
            retry_stage="subtitle_generator",
            skill_name="subtitle_generator",
            input_refs=["subtitle_cues"],
            recommended_action="Build subtitle cues from voice task timing.",
        )

    if len(context["subtitle_cues"]) != len(context["voice_tasks"]):
        add_issue(
            issues,
            severity="warning",
            category="subtitle",
            message="Subtitle cue count does not match voice task count.",
            target={"scope": "project", "id": "subtitle_generator.subtitle_cues"},
            evidence={
                "subtitle_cue_count": len(context["subtitle_cues"]),
                "voice_task_count": len(context["voice_tasks"]),
            },
            retryable=True,
            retry_stage="subtitle_generator",
            skill_name="subtitle_generator",
            input_refs=["subtitle_cues", "voice_tasks"],
            recommended_action="Regenerate subtitle cues from the current voice task list.",
        )

    if "-->" not in str(context["subtitle_generator"].get("srt_text", "")):
        add_issue(
            issues,
            severity="error",
            category="subtitle",
            message="SRT preview text is missing timing arrows.",
            target={"scope": "project", "id": "subtitle_generator.srt_text"},
            evidence={"srt_text_present": bool(context["subtitle_generator"].get("srt_text"))},
            retryable=True,
            retry_stage="subtitle_generator",
            skill_name="subtitle_generator",
            input_refs=["srt_text"],
            recommended_action="Regenerate SRT preview text from subtitle cues.",
        )

    for cue in context["subtitle_cues"]:
        cue_id = str(cue.get("cue_id", "unknown_subtitle_cue"))
        start_second = as_int(cue.get("start_second"), 0)
        end_second = as_int(cue.get("end_second"), 0)
        text = str(cue.get("text", ""))
        target_shot_ids = cue.get("target_shot_ids", [])

        if end_second <= start_second:
            add_issue(
                issues,
                severity="error",
                category="subtitle",
                message="Subtitle cue timing must have end_second greater than start_second.",
                target={"scope": "subtitle_cue", "id": cue_id},
                evidence={"start_second": start_second, "end_second": end_second},
                retryable=True,
                retry_stage="subtitle_generator",
                skill_name="subtitle_generator",
                input_refs=[cue_id],
                recommended_action="Regenerate subtitle timing for this cue.",
            )

        if end_second > total_seconds:
            add_issue(
                issues,
                severity="warning",
                category="subtitle",
                message="Subtitle cue extends beyond storyboard total duration.",
                target={"scope": "subtitle_cue", "id": cue_id},
                evidence={"end_second": end_second, "storyboard_total_seconds": total_seconds},
                retryable=True,
                retry_stage="subtitle_generator",
                skill_name="subtitle_generator",
                input_refs=[cue_id],
                recommended_action="Clamp subtitle timing to storyboard duration.",
            )

        if len(text) > MAX_SUBTITLE_CHARS:
            add_issue(
                issues,
                severity="warning",
                category="subtitle",
                message="Subtitle cue text is too long for the current mobile-safe policy.",
                target={"scope": "subtitle_cue", "id": cue_id},
                evidence={"text_length": len(text), "max_subtitle_chars": MAX_SUBTITLE_CHARS},
                retryable=True,
                retry_stage="subtitle_generator",
                skill_name="subtitle_generator",
                input_refs=[cue_id],
                recommended_action="Shorten or split the subtitle cue.",
            )

        if not isinstance(target_shot_ids, list) or not target_shot_ids:
            add_issue(
                issues,
                severity="warning",
                category="subtitle",
                message="Subtitle cue has no target storyboard shot references.",
                target={"scope": "subtitle_cue", "id": cue_id},
                evidence={"target_shot_ids": target_shot_ids},
                retryable=True,
                retry_stage="subtitle_generator",
                skill_name="subtitle_generator",
                input_refs=[cue_id],
                recommended_action="Attach cue timing to one or more storyboard shots.",
            )
        else:
            missing_shots = [shot_id for shot_id in target_shot_ids if shot_id not in shot_ids]
            if missing_shots:
                add_issue(
                    issues,
                    severity="error",
                    category="subtitle",
                    message="Subtitle cue references unknown storyboard shots.",
                    target={"scope": "subtitle_cue", "id": cue_id},
                    evidence={"missing_shot_ids": missing_shots},
                    retryable=True,
                    retry_stage="subtitle_generator",
                    skill_name="subtitle_generator",
                    input_refs=[cue_id, *missing_shots],
                    recommended_action="Rebuild subtitle shot references from the current storyboard.",
                )


def check_auto_editor(context: dict[str, Any], issues: list[dict[str, Any]]) -> None:
    auto_editor = context["auto_editor"]
    timeline = auto_editor.get("timeline", {})
    tracks = context["tracks"]
    video_track = tracks.get("video", []) if isinstance(tracks, dict) else []
    audio_track = tracks.get("audio", []) if isinstance(tracks, dict) else []
    subtitle_track = tracks.get("subtitles", []) if isinstance(tracks, dict) else []

    if not isinstance(video_track, list) or len(video_track) != len(context["clips"]):
        add_issue(
            issues,
            severity="error",
            category="edit_plan",
            message="Rough edit video track count does not match mock video clip count.",
            target={"scope": "timeline", "id": str(timeline.get("timeline_id", "rough_edit"))},
            evidence={"video_track_count": len(video_track) if isinstance(video_track, list) else None, "clip_count": len(context["clips"])},
            retryable=True,
            retry_stage="auto_editor",
            skill_name="auto_editor",
            input_refs=["timeline.tracks.video"],
            recommended_action="Rebuild rough edit timeline from the current mock video clips.",
        )

    if not isinstance(audio_track, list) or len(audio_track) != len(context["voice_tasks"]):
        add_issue(
            issues,
            severity="error",
            category="edit_plan",
            message="Rough edit audio track count does not match voice task count.",
            target={"scope": "timeline", "id": str(timeline.get("timeline_id", "rough_edit"))},
            evidence={"audio_track_count": len(audio_track) if isinstance(audio_track, list) else None, "voice_task_count": len(context["voice_tasks"])},
            retryable=True,
            retry_stage="auto_editor",
            skill_name="auto_editor",
            input_refs=["timeline.tracks.audio"],
            recommended_action="Rebuild rough edit timeline from the current voice tasks.",
        )

    if not isinstance(subtitle_track, list) or len(subtitle_track) != len(context["subtitle_cues"]):
        add_issue(
            issues,
            severity="error",
            category="edit_plan",
            message="Rough edit subtitle track count does not match subtitle cue count.",
            target={"scope": "timeline", "id": str(timeline.get("timeline_id", "rough_edit"))},
            evidence={
                "subtitle_track_count": len(subtitle_track) if isinstance(subtitle_track, list) else None,
                "subtitle_cue_count": len(context["subtitle_cues"]),
            },
            retryable=True,
            retry_stage="auto_editor",
            skill_name="auto_editor",
            input_refs=["timeline.tracks.subtitles"],
            recommended_action="Rebuild rough edit timeline from the current subtitle cues.",
        )

    timeline_total = as_int(timeline.get("total_duration_seconds"), 0)
    if abs(timeline_total - context["storyboard_total_seconds"]) > 1:
        add_issue(
            issues,
            severity="error",
            category="edit_plan",
            message="Rough edit timeline duration differs from storyboard timing.",
            target={"scope": "timeline", "id": str(timeline.get("timeline_id", "rough_edit"))},
            evidence={
                "timeline_total_duration_seconds": timeline_total,
                "storyboard_total_seconds": context["storyboard_total_seconds"],
            },
            retryable=True,
            retry_stage="auto_editor",
            skill_name="auto_editor",
            input_refs=["timeline.total_duration_seconds"],
            recommended_action="Rebuild timeline duration from storyboard shot timing.",
        )

    for item in video_track if isinstance(video_track, list) else []:
        if isinstance(item, dict) and item.get("file_written") is not False:
            add_boundary_issue(
                issues,
                "auto_editor.timeline.tracks.video.file_written",
                {"timeline_item_id": item.get("timeline_item_id"), "file_written": item.get("file_written")},
            )

    render_plan = auto_editor.get("render_plan", {})
    if isinstance(render_plan, dict):
        if render_plan.get("engine") != "none":
            add_boundary_issue(issues, "auto_editor.render_plan.engine", {"engine": render_plan.get("engine")})
        output_intent = render_plan.get("output_intent", {})
        if isinstance(output_intent, dict) and output_intent.get("file_written") is not False:
            add_boundary_issue(
                issues,
                "auto_editor.render_plan.output_intent.file_written",
                {"file_written": output_intent.get("file_written")},
            )


def check_boundary_flags(context: dict[str, Any], issues: list[dict[str, Any]]) -> None:
    flag_groups = [
        ("video_generation.clip_manifest", context["video_generation"].get("clip_manifest", {})),
        ("voice_casting.voice_manifest", context["voice_casting"].get("voice_manifest", {})),
        ("voice_casting.generation_plan", context["voice_casting"].get("generation_plan", {})),
        ("subtitle_generator.subtitle_manifest", context["subtitle_generator"].get("subtitle_manifest", {})),
        ("subtitle_generator.generation_plan", context["subtitle_generator"].get("generation_plan", {})),
        ("auto_editor.render_plan", context["auto_editor"].get("render_plan", {})),
        ("auto_editor.edit_manifest", context["auto_editor"].get("edit_manifest", {})),
    ]

    for group_name, group in flag_groups:
        if not isinstance(group, dict):
            add_issue(
                issues,
                severity="error",
                category="asset_boundary",
                message="Boundary manifest is missing or not an object.",
                target={"scope": "manifest", "id": group_name},
                evidence={"manifest": group_name},
                retryable=False,
                recommended_action="Return to the upstream skill boundary and restore manifest structure.",
            )
            continue

        for field in [
            "writes_files",
            "writes_database",
            "reads_media_files",
            "uses_visual_detector",
            "uses_audio_detector",
            "uses_ffmpeg",
        ]:
            if field in group and group.get(field) is not False:
                add_boundary_issue(issues, f"{group_name}.{field}", {field: group.get(field)})


def add_boundary_issue(issues: list[dict[str, Any]], field_path: str, evidence: dict[str, Any]) -> None:
    add_issue(
        issues,
        severity="blocking",
        category="asset_boundary",
        message=f"Boundary flag {field_path} must remain disabled in Stage 11.",
        target={"scope": "boundary", "id": field_path},
        evidence=evidence,
        retryable=False,
        recommended_action="Keep this stage as metadata-only; move real media, FFmpeg, or persistence work to a later adapter.",
    )


def add_issue(
    issues: list[dict[str, Any]],
    *,
    severity: str,
    category: str,
    message: str,
    target: dict[str, Any],
    evidence: dict[str, Any],
    retryable: bool,
    recommended_action: str,
    retry_stage: str | None = None,
    skill_name: str | None = None,
    input_refs: list[str] | None = None,
) -> str:
    issue_id = f"qc_{len(issues) + 1:03d}"
    auto_retry_hint = None
    if retryable and retry_stage and skill_name:
        auto_retry_hint = {
            "stage": retry_stage,
            "skill_name": skill_name,
            "input_refs": input_refs or [],
            "reason": message,
        }

    issues.append(
        {
            "issue_id": issue_id,
            "severity": severity,
            "category": category,
            "message": message,
            "target": target,
            "evidence": evidence,
            "retryable": retryable,
            "recommended_action": recommended_action,
            "auto_retry_hint": auto_retry_hint,
        }
    )
    return issue_id


def build_auto_retry_items(issues: list[dict[str, Any]]) -> list[dict[str, Any]]:
    retry_items = []
    for index, issue in enumerate((item for item in issues if item.get("retryable") and item.get("auto_retry_hint")), start=1):
        hint = issue["auto_retry_hint"]
        retry_items.append(
            {
                "retry_id": f"retry_{index:03d}",
                "source_issue_id": issue["issue_id"],
                "severity": issue["severity"],
                "stage": hint["stage"],
                "skill_name": hint["skill_name"],
                "target_ids": collect_target_ids(issue),
                "reason": hint["reason"],
                "requires_user_confirmation": issue["severity"] in {"error", "blocking"},
            }
        )
    return retry_items


def build_manual_review_items(issues: list[dict[str, Any]]) -> list[dict[str, Any]]:
    review_items = []
    reviewable_issues = [issue for issue in issues if issue.get("severity") in {"warning", "error", "blocking"}]

    if not reviewable_issues:
        return [
            {
                "checkpoint_id": "manual_confirm_001",
                "source_issue_id": None,
                "severity": "info",
                "category": "quality_checkpoint",
                "target": {"scope": "project", "id": "quality_report"},
                "question": "Confirm the deterministic quality report before downstream persistence or export adapters.",
                "recommended_action": "Approve the report or request another review pass.",
            }
        ]

    for index, issue in enumerate(reviewable_issues, start=1):
        review_items.append(
            {
                "checkpoint_id": f"manual_confirm_{index:03d}",
                "source_issue_id": issue["issue_id"],
                "severity": issue["severity"],
                "category": issue["category"],
                "target": issue["target"],
                "question": "Confirm whether to accept this issue, edit upstream data, or queue the suggested retry.",
                "recommended_action": issue["recommended_action"],
            }
        )
    return review_items


def count_severities(issues: list[dict[str, Any]]) -> dict[str, int]:
    counts = {"info": 0, "warning": 0, "error": 0, "blocking": 0}
    for issue in issues:
        severity = str(issue.get("severity", "info"))
        if severity in counts:
            counts[severity] += 1
    return counts


def derive_quality_status(severity_counts: dict[str, int]) -> str:
    if severity_counts["blocking"]:
        return "blocked"
    if severity_counts["error"]:
        return "needs_review"
    if severity_counts["warning"]:
        return "passed_with_warnings"
    return "passed"


def build_score(issues: list[dict[str, Any]]) -> dict[str, Any]:
    category_penalties: dict[str, int] = {}
    for issue in issues:
        category = str(issue.get("category", "unknown"))
        category_penalties[category] = category_penalties.get(category, 0) + SCORE_PENALTY.get(str(issue.get("severity")), 0)

    bands = {
        category: max(0, 100 - penalty)
        for category, penalty in sorted(category_penalties.items())
    }
    for category in [
        "character_consistency",
        "style_consistency",
        "timing",
        "video_clip",
        "audio",
        "subtitle",
        "edit_plan",
        "asset_boundary",
    ]:
        bands.setdefault(category, 100)

    total_penalty = sum(SCORE_PENALTY.get(str(issue.get("severity")), 0) for issue in issues)
    return {
        "overall": max(0, 100 - total_penalty),
        "max": 100,
        "bands": bands,
    }


def build_quality_manifest(context: dict[str, Any], issues: list[dict[str, Any]]) -> dict[str, Any]:
    severity_counts = count_severities(issues)
    manual_issue_count = len([issue for issue in issues if issue.get("severity") in {"warning", "error", "blocking"}])
    return {
        "checks_run": [
            "character_reference_consistency",
            "style_prompt_block_references",
            "storyboard_timing",
            "mock_video_clip_alignment",
            "voice_task_coverage",
            "subtitle_cue_coverage",
            "rough_edit_timeline_alignment",
            "metadata_only_boundary_flags",
        ],
        "issue_count": len(issues),
        "blocking_count": severity_counts["blocking"],
        "error_count": severity_counts["error"],
        "warning_count": severity_counts["warning"],
        "info_count": severity_counts["info"],
        "auto_retry_count": len([issue for issue in issues if issue.get("retryable")]),
        "manual_review_count": max(1, manual_issue_count),
        "shot_count": len(context["shots"]),
        "clip_count": len(context["clips"]),
        "voice_task_count": len(context["voice_tasks"]),
        "subtitle_cue_count": len(context["subtitle_cues"]),
        "writes_files": False,
        "writes_database": False,
        "reads_media_files": False,
        "uses_visual_detector": False,
        "uses_audio_detector": False,
        "uses_ffmpeg": False,
    }


def build_generation_plan(context: dict[str, Any]) -> dict[str, Any]:
    return {
        "mode": "quality_report_boundary",
        "provider": "none",
        "model": "none",
        "input_skills": [
            "character_bible",
            "style_bible",
            "storyboard_director",
            "video_generation",
            "voice_casting",
            "subtitle_generator",
            "auto_editor",
        ],
        "input_counts": {
            "characters": len(context["character_bible"].get("characters", [])),
            "shots": len(context["shots"]),
            "mock_video_clips": len(context["clips"]),
            "voice_tasks": len(context["voice_tasks"]),
            "subtitle_cues": len(context["subtitle_cues"]),
        },
        "writes_files": False,
        "writes_database": False,
        "reads_media_files": False,
        "uses_visual_detector": False,
        "uses_audio_detector": False,
        "uses_ffmpeg": False,
        "notes": [
            "This skill inspects envelope metadata and deterministic structures only.",
            "Real media QA, OCR, watermark detection, black-frame detection, audio analysis, and persistence stay behind later adapters.",
        ],
    }


def collect_target_ids(issue: dict[str, Any]) -> list[str]:
    target = issue.get("target", {})
    ids = []
    if isinstance(target, dict) and target.get("id"):
        ids.append(str(target["id"]))
    hint = issue.get("auto_retry_hint") or {}
    for value in hint.get("input_refs", []):
        text = str(value)
        if text not in ids:
            ids.append(text)
    return ids


def as_int(value: Any, default: int) -> int:
    try:
        return int(value)
    except (TypeError, ValueError):
        return default


def is_mock_asset_uri(value: Any) -> bool:
    return isinstance(value, str) and value.startswith("milu://mock-assets/")
