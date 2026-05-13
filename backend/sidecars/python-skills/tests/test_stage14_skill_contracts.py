from __future__ import annotations

import importlib
import json
import unittest
from pathlib import Path

from milu_studio_skills.gateway import SkillGateway


class SkillContractDriftTests(unittest.TestCase):
    def test_registry_yaml_schema_and_validators_stay_in_sync(self) -> None:
        root = Path(__file__).resolve().parents[1]
        skills_root = root / "skills"
        skill_names = {
            path.name
            for path in skills_root.iterdir()
            if path.is_dir() and not path.name.startswith("__")
        }
        gateway = SkillGateway.default()

        self.assertEqual(skill_names, set(gateway.executors))

        for skill_name in sorted(skill_names):
            with self.subTest(skill=skill_name):
                skill_dir = skills_root / skill_name
                metadata = parse_simple_skill_yaml(skill_dir / "skill.yaml")

                self.assertEqual(metadata["name"], skill_name)
                self.assertEqual(metadata["input_schema"], "schema.input.json")
                self.assertEqual(metadata["output_schema"], "schema.output.json")
                self.assertTrue((skill_dir / "prompt.md").is_file())
                self.assertTrue((skill_dir / "executor.py").is_file())
                self.assertTrue((skill_dir / "validators.py").is_file())

                json.loads((skill_dir / metadata["input_schema"]).read_text(encoding="utf-8"))
                json.loads((skill_dir / metadata["output_schema"]).read_text(encoding="utf-8"))

                executor_module, executor_name = gateway.executors[skill_name].split(":", maxsplit=1)
                executor = getattr(importlib.import_module(executor_module), executor_name)
                self.assertTrue(callable(executor))

                validators = importlib.import_module(f"skills.{skill_name}.validators")
                self.assertTrue(callable(getattr(validators, "validate_input")))
                self.assertTrue(callable(getattr(validators, "validate_output")))


def parse_simple_skill_yaml(path: Path) -> dict[str, str]:
    metadata: dict[str, str] = {}

    for line in path.read_text(encoding="utf-8").splitlines():
        if not line or line.startswith(" ") or line.startswith("#") or ":" not in line:
            continue

        key, value = line.split(":", maxsplit=1)
        metadata[key.strip()] = value.strip()

    required_keys = {
        "name",
        "display_name",
        "description",
        "stage",
        "input_schema",
        "output_schema",
        "timeout_seconds",
        "max_retries",
    }
    missing = sorted(required_keys - set(metadata))
    if missing:
        raise AssertionError(f"{path} is missing required keys: {', '.join(missing)}")

    return metadata


if __name__ == "__main__":
    unittest.main()
