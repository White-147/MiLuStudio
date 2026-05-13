from __future__ import annotations

import copy
import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway


class Stage11QualityCheckerPipelineTests(unittest.TestCase):
    def test_quality_checker_consumes_stage10_outputs(self) -> None:
        gateway = SkillGateway.default()
        stage = run_stage10_pipeline(gateway)

        quality_result = gateway.run(
            "quality_checker",
            {
                "character_bible": stage["character"],
                "style_bible": stage["style"],
                "storyboard_director": stage["storyboard"],
                "video_generation": stage["video"],
                "voice_casting": stage["voice"],
                "subtitle_generator": stage["subtitles"],
                "auto_editor": stage["edit"],
            },
        )

        self.assertTrue(quality_result["ok"], quality_result.get("error"))
        quality = quality_result["data"]

        self.assertEqual(quality["provider"], "none")
        self.assertEqual(quality["model"], "none")
        self.assertIn(quality["quality_status"], ["passed", "passed_with_warnings", "needs_review"])
        self.assertEqual(quality["generation_plan"]["writes_files"], False)
        self.assertEqual(quality["generation_plan"]["writes_database"], False)
        self.assertEqual(quality["generation_plan"]["reads_media_files"], False)
        self.assertEqual(quality["generation_plan"]["uses_visual_detector"], False)
        self.assertEqual(quality["generation_plan"]["uses_audio_detector"], False)
        self.assertEqual(quality["generation_plan"]["uses_ffmpeg"], False)
        self.assertEqual(quality["quality_manifest"]["writes_files"], False)
        self.assertEqual(quality["quality_manifest"]["writes_database"], False)
        self.assertEqual(quality["quality_manifest"]["reads_media_files"], False)
        self.assertEqual(quality["quality_manifest"]["uses_visual_detector"], False)
        self.assertEqual(quality["quality_manifest"]["uses_audio_detector"], False)
        self.assertEqual(quality["quality_manifest"]["uses_ffmpeg"], False)
        self.assertEqual(quality["quality_manifest"]["blocking_count"], 0)
        self.assertEqual(quality["checkpoint"]["required"], True)
        self.assertTrue(quality["manual_review_items"])

    def test_quality_checker_reports_retryable_subtitle_issue(self) -> None:
        gateway = SkillGateway.default()
        stage = run_stage10_pipeline(gateway)
        subtitles = copy.deepcopy(stage["subtitles"])
        subtitles["data"]["subtitle_cues"][0]["text"] = "This subtitle line is intentionally too long for the mobile safe policy."

        quality_result = gateway.run(
            "quality_checker",
            {
                "character_bible": stage["character"],
                "style_bible": stage["style"],
                "storyboard_director": stage["storyboard"],
                "video_generation": stage["video"],
                "voice_casting": stage["voice"],
                "subtitle_generator": subtitles,
                "auto_editor": stage["edit"],
            },
        )

        self.assertTrue(quality_result["ok"], quality_result.get("error"))
        quality = quality_result["data"]
        subtitle_issues = [issue for issue in quality["issues"] if issue["category"] == "subtitle"]
        subtitle_retries = [item for item in quality["auto_retry_items"] if item["skill_name"] == "subtitle_generator"]

        self.assertGreaterEqual(len(subtitle_issues), 1)
        self.assertGreaterEqual(len(subtitle_retries), 1)
        self.assertEqual(quality["quality_status"], "passed_with_warnings")
        self.assertEqual(quality["quality_manifest"]["warning_count"], 1)

    def test_quality_checker_requires_auto_editor_envelope(self) -> None:
        gateway = SkillGateway.default()
        result = gateway.run(
            "quality_checker",
            {
                "character_bible": {"ok": True},
                "style_bible": {"ok": True},
                "storyboard_director": {"ok": True},
                "video_generation": {"ok": True},
                "voice_casting": {"ok": True},
                "subtitle_generator": {"ok": True},
                "auto_editor": {"ok": True},
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)


def run_stage10_pipeline(gateway: SkillGateway) -> dict[str, dict[str, object]]:
    story_result = gateway.run("story_intake", load_story_input())
    plot_result = gateway.run("plot_adaptation", {"story_intake": story_result})
    script_result = gateway.run("episode_writer", {"plot_adaptation": plot_result})
    character_result = gateway.run("character_bible", {"episode_writer": script_result})
    style_result = gateway.run(
        "style_bible",
        {
            "episode_writer": script_result,
            "character_bible": character_result,
        },
    )
    storyboard_result = gateway.run(
        "storyboard_director",
        {
            "episode_writer": script_result,
            "character_bible": character_result,
            "style_bible": style_result,
        },
    )
    image_prompt_result = gateway.run(
        "image_prompt_builder",
        {
            "storyboard_director": storyboard_result,
            "character_bible": character_result,
            "style_bible": style_result,
        },
    )
    image_result = gateway.run("image_generation", {"image_prompt_builder": image_prompt_result})
    video_prompt_result = gateway.run(
        "video_prompt_builder",
        {
            "storyboard_director": storyboard_result,
            "image_prompt_builder": image_prompt_result,
            "image_generation": image_result,
        },
    )
    video_result = gateway.run("video_generation", {"video_prompt_builder": video_prompt_result})
    voice_result = gateway.run(
        "voice_casting",
        {
            "episode_writer": script_result,
            "storyboard_director": storyboard_result,
        },
    )
    subtitle_result = gateway.run(
        "subtitle_generator",
        {
            "episode_writer": script_result,
            "storyboard_director": storyboard_result,
            "voice_casting": voice_result,
        },
    )
    edit_result = gateway.run(
        "auto_editor",
        {
            "storyboard_director": storyboard_result,
            "video_generation": video_result,
            "voice_casting": voice_result,
            "subtitle_generator": subtitle_result,
        },
    )

    return {
        "script": script_result,
        "character": character_result,
        "style": style_result,
        "storyboard": storyboard_result,
        "video": video_result,
        "voice": voice_result,
        "subtitles": subtitle_result,
        "edit": edit_result,
    }


def load_story_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
