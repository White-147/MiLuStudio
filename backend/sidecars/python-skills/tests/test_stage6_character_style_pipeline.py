from __future__ import annotations

import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway


class Stage6CharacterStylePipelineTests(unittest.TestCase):
    def test_episode_writer_to_character_and_style_bibles_returns_reviewable_structures(self) -> None:
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

        self.assertTrue(character_result["ok"])
        self.assertTrue(style_result["ok"])

        characters = character_result["data"]["characters"]
        style = style_result["data"]

        self.assertEqual(character_result["data"]["project_id"], "demo-episode-01")
        self.assertEqual(style["project_id"], "demo-episode-01")
        self.assertGreaterEqual(len(characters), 1)
        self.assertEqual(characters[0]["role_type"], "main")
        self.assertTrue(characters[0]["appearance"]["signature_detail"])
        self.assertTrue(characters[0]["costume"]["lock_rule"])
        self.assertTrue(characters[0]["voice_profile"]["tone"])
        self.assertEqual(character_result["data"]["checkpoint"]["required"], True)

        self.assertTrue(style["visual_style"]["name"])
        self.assertGreaterEqual(len(style["color_palette"]), 4)
        self.assertTrue(style["camera_language"]["shot_rules"])
        self.assertIn("角色脸型漂移", style["negative_prompt"])
        self.assertTrue(style["reusable_prompt_blocks"]["base_style"])
        self.assertTrue(style["reusable_prompt_blocks"]["character_consistency"])
        self.assertEqual(style["checkpoint"]["required"], True)

    def test_character_bible_requires_episode_writer_envelope(self) -> None:
        result = SkillGateway.default().run("character_bible", {"episode_writer": {"ok": True}})

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)

    def test_style_bible_requires_matching_character_bible_envelope(self) -> None:
        gateway = SkillGateway.default()
        story_result = gateway.run("story_intake", load_story_input())
        plot_result = gateway.run("plot_adaptation", {"story_intake": story_result})
        script_result = gateway.run("episode_writer", {"plot_adaptation": plot_result})
        result = gateway.run(
            "style_bible",
            {
                "episode_writer": script_result,
                "character_bible": {"ok": True, "skill_name": "character_bible", "schema_version": "1.0", "data": {}},
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertTrue(
            any("project_id" in detail for detail in result["error"]["details"])
        )


def load_story_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()

