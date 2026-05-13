from __future__ import annotations

import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway


class Stage10AudioSubtitleEditPipelineTests(unittest.TestCase):
    def test_mock_video_clips_feed_voice_subtitles_and_edit_plan(self) -> None:
        gateway = SkillGateway.default()
        stage = run_stage9_pipeline(gateway)

        voice_result = gateway.run(
            "voice_casting",
            {
                "episode_writer": stage["script"],
                "storyboard_director": stage["storyboard"],
            },
        )
        subtitle_result = gateway.run(
            "subtitle_generator",
            {
                "episode_writer": stage["script"],
                "storyboard_director": stage["storyboard"],
                "voice_casting": voice_result,
            },
        )
        edit_result = gateway.run(
            "auto_editor",
            {
                "storyboard_director": stage["storyboard"],
                "video_generation": stage["video"],
                "voice_casting": voice_result,
                "subtitle_generator": subtitle_result,
            },
        )

        self.assertTrue(voice_result["ok"], voice_result.get("error"))
        self.assertTrue(subtitle_result["ok"], subtitle_result.get("error"))
        self.assertTrue(edit_result["ok"], edit_result.get("error"))

        storyboard = stage["storyboard"]["data"]
        video = stage["video"]["data"]
        voice = voice_result["data"]
        subtitles = subtitle_result["data"]
        edit = edit_result["data"]

        self.assertEqual(voice["provider"], "none")
        self.assertEqual(voice["model"], "none")
        self.assertEqual(voice["generation_plan"]["writes_files"], False)
        self.assertEqual(voice["generation_plan"]["writes_database"], False)
        self.assertEqual(voice["voice_manifest"]["writes_files"], False)
        self.assertEqual(voice["checkpoint"]["required"], True)
        self.assertTrue(any(profile["role"] == "narrator" for profile in voice["voice_profiles"]))
        self.assertGreaterEqual(len(voice["voice_tasks"]), len(stage["script"]["data"]["segments"]))

        for task in voice["voice_tasks"]:
            self.assertEqual(task["provider"], "none")
            self.assertEqual(task["model"], "none")
            self.assertEqual(task["output_audio_intent"]["file_written"], False)
            self.assertTrue(task["target_shot_ids"])

        self.assertEqual(subtitles["provider"], "none")
        self.assertEqual(subtitles["model"], "none")
        self.assertEqual(subtitles["generation_plan"]["writes_files"], False)
        self.assertEqual(subtitles["generation_plan"]["writes_database"], False)
        self.assertEqual(subtitles["subtitle_manifest"]["writes_files"], False)
        self.assertEqual(subtitles["checkpoint"]["required"], False)
        self.assertIn("-->", subtitles["srt_text"])
        self.assertEqual(len(subtitles["subtitle_cues"]), len(voice["voice_tasks"]))

        for cue in subtitles["subtitle_cues"]:
            self.assertLessEqual(len(cue["text"]), 32)
            self.assertTrue(cue["target_shot_ids"])

        timeline = edit["timeline"]
        render_plan = edit["render_plan"]
        manifest = edit["edit_manifest"]
        total_duration = sum(shot["duration_seconds"] for shot in storyboard["shots"])

        self.assertEqual(edit["provider"], "none")
        self.assertEqual(edit["model"], "none")
        self.assertEqual(timeline["total_duration_seconds"], total_duration)
        self.assertEqual(len(timeline["tracks"]["video"]), len(video["clips"]))
        self.assertEqual(len(timeline["tracks"]["audio"]), len(voice["voice_tasks"]))
        self.assertEqual(len(timeline["tracks"]["subtitles"]), len(subtitles["subtitle_cues"]))
        self.assertEqual(render_plan["engine"], "none")
        self.assertEqual(render_plan["uses_ffmpeg"], False)
        self.assertEqual(render_plan["writes_files"], False)
        self.assertEqual(render_plan["writes_database"], False)
        self.assertEqual(render_plan["output_intent"]["file_written"], False)
        self.assertEqual(manifest["writes_files"], False)
        self.assertEqual(manifest["writes_database"], False)
        self.assertEqual(manifest["uses_ffmpeg"], False)
        self.assertEqual(edit["checkpoint"]["required"], False)

    def test_voice_casting_requires_storyboard_envelope(self) -> None:
        gateway = SkillGateway.default()
        result = gateway.run(
            "voice_casting",
            {
                "episode_writer": {"ok": True},
                "storyboard_director": {"ok": True},
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)

    def test_subtitle_generator_requires_voice_casting_envelope(self) -> None:
        gateway = SkillGateway.default()
        result = gateway.run(
            "subtitle_generator",
            {
                "episode_writer": {"ok": True},
                "storyboard_director": {"ok": True},
                "voice_casting": {"ok": True},
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)

    def test_auto_editor_requires_video_generation_envelope(self) -> None:
        gateway = SkillGateway.default()
        result = gateway.run(
            "auto_editor",
            {
                "storyboard_director": {"ok": True},
                "video_generation": {"ok": True},
                "voice_casting": {"ok": True},
                "subtitle_generator": {"ok": True},
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)


def run_stage9_pipeline(gateway: SkillGateway) -> dict[str, dict[str, object]]:
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

    return {
        "script": script_result,
        "storyboard": storyboard_result,
        "video": video_result,
    }


def load_story_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
