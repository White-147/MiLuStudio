from __future__ import annotations

import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway
from skills.story_intake.executor import run


class StoryIntakeTests(unittest.TestCase):
    def test_executor_returns_stable_story_facts(self) -> None:
        payload = load_example_input()

        data = run(payload)

        self.assertEqual(data["project_id"], "demo-episode-01")
        self.assertIn("悬疑", data["genres"])
        self.assertEqual(data["checkpoint"]["required"], False)
        self.assertEqual(len(data["story_beats"]), 3)
        self.assertGreaterEqual(data["recommended_shot_count"], 6)

    def test_gateway_wraps_success_in_worker_envelope(self) -> None:
        result = SkillGateway.default().run("story_intake", load_example_input())

        self.assertTrue(result["ok"])
        self.assertEqual(result["skill_name"], "story_intake")
        self.assertIsNone(result["error"])
        self.assertEqual(result["runtime"]["model"], "none")

    def test_gateway_returns_structured_validation_error(self) -> None:
        result = SkillGateway.default().run("story_intake", {"target_duration_seconds": 120})

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertTrue(result["error"]["message"])
        self.assertGreaterEqual(len(result["error"]["details"]), 1)

    def test_mudan_ting_fixture_extracts_real_characters(self) -> None:
        payload = {
            "project_id": "mudan-ting-fixture",
            "story_text": load_mudan_ting_fixture(),
            "language": "zh-CN",
            "target_duration_seconds": 45,
            "aspect_ratio": "9:16",
            "style_preset": "轻写实国漫",
            "mode": "director",
        }

        data = run(payload)
        names = [character["name"] for character in data["main_characters"]]

        self.assertGreaterEqual(len(names), 3)
        self.assertEqual(names[0], "杜丽娘")
        self.assertIn("春香", names)
        self.assertIn("柳梦梅", names)
        self.assertNotIn("柳枝垂", names)
        self.assertNotIn("花影像", names)
        self.assertNotIn("她倚", names)


def load_example_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


def load_mudan_ting_fixture() -> str:
    path = Path(__file__).resolve().parents[4] / "docs" / "test-fixtures" / "scripts" / "mudan_ting_stage_input_zh.txt"
    return path.read_text(encoding="utf-8")


if __name__ == "__main__":
    unittest.main()
