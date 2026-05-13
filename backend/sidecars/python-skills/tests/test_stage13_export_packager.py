from __future__ import annotations

import unittest

from milu_studio_skills.gateway import SkillGateway
from tests.test_stage11_quality_checker_pipeline import run_stage10_pipeline


class Stage13ExportPackagerTests(unittest.TestCase):
    def test_export_packager_returns_placeholder_delivery_assets(self) -> None:
        gateway = SkillGateway.default()
        stage = run_stage10_pipeline(gateway)
        quality_result = gateway.run(
            "quality_checker",
            {
                "character_bible": stage["character"],
                "style_bible": stage["style"],
                "storyboard_director": stage["storyboard"],
                "video_generation": stage["video"],
                "voice_casting": stage["voice"],
                "subtitle_generator": stage["subtitles"],
                "auto_editor": stage["edit"],
            },
        )

        result = gateway.run(
            "export_packager",
            {
                "auto_editor": stage["edit"],
                "quality_checker": quality_result,
                "subtitle_generator": stage["subtitles"],
                "video_generation": stage["video"],
            },
        )

        self.assertTrue(result["ok"], result.get("error"))
        package = result["data"]

        self.assertEqual(package["provider"], "none")
        self.assertEqual(package["model"], "none")
        self.assertEqual(package["checkpoint"]["required"], False)
        self.assertEqual(package["export_manifest"]["writes_files"], False)
        self.assertEqual(package["export_manifest"]["writes_database"], False)
        self.assertEqual(package["export_manifest"]["reads_media_files"], False)
        self.assertEqual(package["export_manifest"]["uses_ffmpeg"], False)
        self.assertGreaterEqual(len(package["delivery_assets"]), 4)
        self.assertTrue(all(asset["file_written"] is False for asset in package["delivery_assets"]))

    def test_export_packager_requires_quality_checker_envelope(self) -> None:
        gateway = SkillGateway.default()
        result = gateway.run(
            "export_packager",
            {
                "auto_editor": {"ok": True},
                "quality_checker": {"ok": True},
                "subtitle_generator": {"ok": True},
                "video_generation": {"ok": True},
            },
        )

        self.assertFalse(result["ok"])
        self.assertEqual(result["error"]["code"], "SKILL_VALIDATION_ERROR")


if __name__ == "__main__":
    unittest.main()
