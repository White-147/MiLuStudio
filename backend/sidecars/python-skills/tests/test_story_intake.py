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


def load_example_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
