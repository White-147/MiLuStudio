from __future__ import annotations

import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway


class Stage7StoryboardPipelineTests(unittest.TestCase):
    def test_character_and_style_bibles_feed_reviewable_storyboard(self) -> None:
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

        self.assertTrue(storyboard_result["ok"])

        storyboard = storyboard_result["data"]
        shots = storyboard["shots"]

        self.assertEqual(storyboard["project_id"], "demo-episode-01")
        self.assertEqual(storyboard["target_duration_seconds"], 45)
        self.assertGreaterEqual(len(shots), 6)
        self.assertLessEqual(len(shots), 12)
        self.assertEqual(sum(shot["duration_seconds"] for shot in shots), 45)
        self.assertEqual(storyboard["timing_summary"]["within_tolerance"], True)
        self.assertEqual(storyboard["checkpoint"]["required"], True)

        first_shot = shots[0]
        self.assertTrue(first_shot["scene"])
        self.assertTrue(first_shot["characters"])
        self.assertTrue(first_shot["camera"]["motion"])
        self.assertTrue(first_shot["lighting"]["key_light"])
        self.assertTrue(first_shot["image_prompt_seed"])
        self.assertTrue(first_shot["video_prompt_seed"])
        self.assertIn("base_style", first_shot["style_prompt_block_refs"])

    def test_storyboard_director_requires_style_bible_envelope(self) -> None:
        gateway = SkillGateway.default()
        story_result = gateway.run("story_intake", load_story_input())
        plot_result = gateway.run("plot_adaptation", {"story_intake": story_result})
        script_result = gateway.run("episode_writer", {"plot_adaptation": plot_result})
        character_result = gateway.run("character_bible", {"episode_writer": script_result})
        result = gateway.run(
            "storyboard_director",
            {
                "episode_writer": script_result,
                "character_bible": character_result,
                "style_bible": {"ok": True},
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)


def load_story_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()

