from __future__ import annotations

import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway


class Stage5ScriptPipelineTests(unittest.TestCase):
    def test_story_intake_to_episode_writer_pipeline_returns_reviewable_script(self) -> None:
        gateway = SkillGateway.default()
        story_result = gateway.run("story_intake", load_story_input())
        plot_result = gateway.run("plot_adaptation", {"story_intake": story_result})
        script_result = gateway.run("episode_writer", {"plot_adaptation": plot_result})

        self.assertTrue(story_result["ok"])
        self.assertTrue(plot_result["ok"])
        self.assertTrue(script_result["ok"])

        plot = plot_result["data"]
        script = script_result["data"]

        self.assertEqual(plot["project_id"], "demo-episode-01")
        self.assertEqual(len(plot["plot_beats"]), 4)
        self.assertEqual(sum(beat["estimated_seconds"] for beat in plot["plot_beats"]), 45)
        self.assertEqual(script["target_duration_seconds"], 45)
        self.assertEqual(sum(segment["duration_seconds"] for segment in script["segments"]), 45)
        self.assertEqual(script["checkpoint"]["required"], True)
        self.assertIn("旁白", script["script_text"])
        self.assertIn("对白", script["script_text"])
        self.assertGreaterEqual(len(script["subtitle_cues"]), 4)

    def test_plot_adaptation_requires_successful_story_intake_envelope(self) -> None:
        result = SkillGateway.default().run(
            "plot_adaptation",
            {"story_intake": {"ok": False, "skill_name": "story_intake", "schema_version": "1.0", "data": None}},
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertTrue(
            any(detail.startswith("story_intake.ok must be true") for detail in result["error"]["details"])
        )

    def test_episode_writer_requires_plot_adaptation_envelope(self) -> None:
        result = SkillGateway.default().run("episode_writer", {"plot_adaptation": {"ok": True}})

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")
        self.assertGreaterEqual(len(result["error"]["details"]), 1)


def load_story_input() -> dict[str, object]:
    path = Path(__file__).resolve().parents[1] / "skills" / "story_intake" / "examples" / "input.json"
    return json.loads(path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
