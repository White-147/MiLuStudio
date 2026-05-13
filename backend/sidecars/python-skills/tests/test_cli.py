from __future__ import annotations

import json
import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


class SkillCliTests(unittest.TestCase):
    def test_cli_run_writes_success_output_json(self) -> None:
        root = Path(__file__).resolve().parents[1]
        input_path = root / "skills" / "story_intake" / "examples" / "input.json"

        with temporary_directory(root) as temp_dir:
            output_path = Path(temp_dir) / "output.json"
            completed = subprocess.run(
                [
                    sys.executable,
                    "-m",
                    "milu_studio_skills",
                    "run",
                    "--skill",
                    "story_intake",
                    "--input",
                    str(input_path),
                    "--output",
                    str(output_path),
                ],
                cwd=root,
                env=os.environ.copy(),
                text=True,
                capture_output=True,
                check=False,
            )

            self.assertEqual(completed.returncode, 0, completed.stderr)
            result = json.loads(output_path.read_text(encoding="utf-8"))
            self.assertTrue(result["ok"])
            self.assertEqual(result["skill_name"], "story_intake")

    def test_cli_run_writes_error_output_json(self) -> None:
        root = Path(__file__).resolve().parents[1]

        with temporary_directory(root) as temp_dir:
            input_path = Path(temp_dir) / "bad-input.json"
            output_path = Path(temp_dir) / "output.json"
            input_path.write_text('{"target_duration_seconds":120}', encoding="utf-8")

            completed = subprocess.run(
                [
                    sys.executable,
                    "-m",
                    "milu_studio_skills",
                    "run",
                    "--skill",
                    "story_intake",
                    "--input",
                    str(input_path),
                    "--output",
                    str(output_path),
                ],
                cwd=root,
                env=os.environ.copy(),
                text=True,
                capture_output=True,
                check=False,
            )

            self.assertEqual(completed.returncode, 2)
            result = json.loads(output_path.read_text(encoding="utf-8"))
            self.assertFalse(result["ok"])
            self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")


def temporary_directory(root: Path) -> tempfile.TemporaryDirectory[str]:
    temp_base = Path(os.environ.get("TMP") or root.parents[2] / ".tmp")
    temp_base.mkdir(parents=True, exist_ok=True)
    return tempfile.TemporaryDirectory(prefix="milu-skills-test-", dir=temp_base)


if __name__ == "__main__":
    unittest.main()
