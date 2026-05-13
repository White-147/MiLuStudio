from __future__ import annotations

import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway


class Stage8ImagePipelineTests(unittest.TestCase):
    def test_storyboard_feeds_image_prompts_and_mock_assets(self) -> None:
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
        prompt_result = gateway.run(
            "image_prompt_builder",
            {
                "storyboard_director": storyboard_result,
                "character_bible": character_result,
                "style_bible": style_result,
            },
        )
        image_result = gateway.run("image_generation", {"image_prompt_builder": prompt_result})

        self.assertTrue(prompt_result["ok"], prompt_result.get("error"))
        self.assertTrue(image_result["ok"], image_result.get("error"))

        storyboard = storyboard_result["data"]
        prompts = prompt_result["data"]
        assets = image_result["data"]["assets"]
        manifest = image_result["data"]["asset_manifest"]

        shot_count = len(storyboard["shots"])
        character_count = len(character_result["data"]["characters"])
        expected_min_requests = character_count + shot_count * 3

        self.assertEqual(prompts["project_id"], "demo-episode-01")
        self.assertEqual(prompts["generation_plan"]["provider"], "none")
        self.assertEqual(prompts["generation_plan"]["writes_files"], False)
        self.assertEqual(prompts["generation_plan"]["writes_database"], False)
        self.assertGreaterEqual(len(prompts["image_requests"]), expected_min_requests)
        self.assertEqual(prompts["checkpoint"]["required"], False)
        self.assertIn("wrong aspect ratio", prompts["negative_prompt"])

        request_asset_types = {request["asset_type"] for request in prompts["image_requests"]}
        self.assertIn("character_reference", request_asset_types)
        self.assertIn("storyboard_image", request_asset_types)
        self.assertIn("first_frame", request_asset_types)
        self.assertIn("last_frame", request_asset_types)

        self.assertEqual(len(assets), len(prompts["image_requests"]))
        self.assertEqual(image_result["data"]["provider"], "mock")
        self.assertEqual(image_result["data"]["model"], "none")
        self.assertEqual(image_result["data"]["checkpoint"]["required"], True)
        self.assertEqual(manifest["writes_files"], False)
        self.assertEqual(manifest["writes_database"], False)
        self.assertEqual(manifest["asset_count"], len(assets))
        self.assertTrue(manifest["character_references"])

        for shot in storyboard["shots"]:
            self.assertIn(shot["shot_id"], manifest["by_shot"])
            self.assertGreaterEqual(len(manifest["by_shot"][shot["shot_id"]]), 3)

        for asset in assets:
            self.assertEqual(asset["provider"], "mock")
            self.assertEqual(asset["model"], "none")
            self.assertEqual(asset["file_written"], False)
            self.assertTrue(asset["asset_uri"].startswith("milu://mock-assets/demo-episode-01/"))
            self.assertEqual(asset["storage_intent"]["root"], "D:\\code\\MiLuStudio\\storage")

    def test_image_prompt_builder_requires_storyboard_envelope(self) -> None:
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
        result = gateway.run(
            "image_prompt_builder",
            {
                "storyboard_director": {"ok": True},
                "character_bible": character_result,
                "style_bible": style_result,
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)

    def test_image_generation_requires_prompt_builder_envelope(self) -> None:
        gateway = SkillGateway.default()
        result = gateway.run("image_generation", {"image_prompt_builder": {"ok": True}})

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)


def load_story_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()

