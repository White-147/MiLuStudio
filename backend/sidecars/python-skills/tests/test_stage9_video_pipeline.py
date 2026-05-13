from __future__ import annotations

import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway


class Stage9VideoPipelineTests(unittest.TestCase):
    def test_images_feed_video_prompts_and_mock_clips(self) -> None:
        gateway = SkillGateway.default()
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

        self.assertTrue(video_prompt_result["ok"], video_prompt_result.get("error"))
        self.assertTrue(video_result["ok"], video_result.get("error"))

        storyboard = storyboard_result["data"]
        video_prompts = video_prompt_result["data"]
        video_data = video_result["data"]
        clips = video_data["clips"]
        clip_manifest = video_data["clip_manifest"]

        shot_count = len(storyboard["shots"])
        total_duration = sum(shot["duration_seconds"] for shot in storyboard["shots"])

        self.assertEqual(video_prompts["project_id"], "demo-episode-01")
        self.assertEqual(len(video_prompts["video_requests"]), shot_count)
        self.assertEqual(video_prompts["generation_plan"]["provider"], "none")
        self.assertEqual(video_prompts["generation_plan"]["model"], "none")
        self.assertEqual(video_prompts["generation_plan"]["writes_files"], False)
        self.assertEqual(video_prompts["generation_plan"]["writes_database"], False)
        self.assertEqual(video_prompts["generation_plan"]["uses_ffmpeg"], False)
        self.assertEqual(video_prompts["checkpoint"]["required"], False)
        self.assertIn("identity drift", video_prompts["negative_prompt"])

        for request in video_prompts["video_requests"]:
            self.assertEqual(request["generation_mode"], "image_to_video")
            roles = {source_image["role"] for source_image in request["source_images"]}
            self.assertIn("first_frame", roles)
            self.assertIn("last_frame", roles)
            self.assertTrue(request["character_reference_asset_ids"])

        self.assertEqual(video_data["provider"], "mock")
        self.assertEqual(video_data["model"], "none")
        self.assertEqual(video_data["checkpoint"]["required"], True)
        self.assertEqual(len(clips), shot_count)
        self.assertEqual(clip_manifest["clip_count"], shot_count)
        self.assertEqual(clip_manifest["total_duration_seconds"], total_duration)
        self.assertEqual(clip_manifest["writes_files"], False)
        self.assertEqual(clip_manifest["writes_database"], False)
        self.assertEqual(clip_manifest["uses_ffmpeg"], False)

        for shot in storyboard["shots"]:
            self.assertIn(shot["shot_id"], clip_manifest["by_shot"])

        for clip in clips:
            self.assertEqual(clip["provider"], "mock")
            self.assertEqual(clip["model"], "none")
            self.assertEqual(clip["file_written"], False)
            self.assertTrue(clip["asset_uri"].startswith("milu://mock-assets/demo-episode-01/"))
            self.assertEqual(clip["storage_intent"]["root"], "D:\\code\\MiLuStudio\\storage")
            self.assertGreaterEqual(clip["metadata"]["frame_count_estimate"], 1)
            self.assertTrue(clip["source_images"])

    def test_video_prompt_builder_requires_image_generation_envelope(self) -> None:
        gateway = SkillGateway.default()
        result = gateway.run(
            "video_prompt_builder",
            {
                "storyboard_director": {"ok": True},
                "image_prompt_builder": {"ok": True},
                "image_generation": {"ok": True},
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)

    def test_video_generation_requires_prompt_builder_envelope(self) -> None:
        gateway = SkillGateway.default()
        result = gateway.run("video_generation", {"video_prompt_builder": {"ok": True}})

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)


def load_story_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
