from __future__ import annotations

from dataclasses import dataclass
from importlib import import_module
from time import perf_counter
from typing import Any, Callable

from .errors import SkillNotFoundError, SkillRuntimeError

SkillExecutor = Callable[[dict[str, Any]], dict[str, Any]]


@dataclass(frozen=True)
class SkillGateway:
    """Single runtime boundary for future .NET Worker subprocess calls."""

    executors: dict[str, str]

    @classmethod
    def default(cls) -> "SkillGateway":
        return cls(
            executors={
                "story_intake": "skills.story_intake.executor:run",
                "plot_adaptation": "skills.plot_adaptation.executor:run",
                "episode_writer": "skills.episode_writer.executor:run",
                "character_bible": "skills.character_bible.executor:run",
                "style_bible": "skills.style_bible.executor:run",
                "storyboard_director": "skills.storyboard_director.executor:run",
                "image_prompt_builder": "skills.image_prompt_builder.executor:run",
                "image_generation": "skills.image_generation.executor:run",
                "video_prompt_builder": "skills.video_prompt_builder.executor:run",
                "video_generation": "skills.video_generation.executor:run",
                "voice_casting": "skills.voice_casting.executor:run",
                "subtitle_generator": "skills.subtitle_generator.executor:run",
                "auto_editor": "skills.auto_editor.executor:run",
            }
        )

    def run(self, skill_name: str, payload: dict[str, Any]) -> dict[str, Any]:
        started = perf_counter()

        try:
            executor = self._load_executor(skill_name)
            data = executor(payload)
            duration_ms = round((perf_counter() - started) * 1000, 2)

            return {
                "ok": True,
                "skill_name": skill_name,
                "schema_version": "1.0",
                "data": data,
                "error": None,
                "runtime": {
                    "duration_ms": duration_ms,
                    "model": "none",
                    "mode": "deterministic-mock",
                    "cost_estimate": 0,
                },
            }
        except SkillRuntimeError as error:
            return self._error_envelope(skill_name, error)
        except Exception as error:  # pragma: no cover - defensive envelope for subprocess boundary.
            return self._error_envelope(
                skill_name,
                SkillRuntimeError(f"Unhandled skill error: {error.__class__.__name__}: {error}"),
            )

    def _load_executor(self, skill_name: str) -> SkillExecutor:
        target = self.executors.get(skill_name)

        if target is None:
            known = ", ".join(sorted(self.executors))
            raise SkillNotFoundError(f"Unknown skill '{skill_name}'. Known skills: {known}.")

        module_name, function_name = target.split(":", maxsplit=1)
        module = import_module(module_name)
        executor = getattr(module, function_name)

        if not callable(executor):
            raise SkillNotFoundError(f"Skill '{skill_name}' executor is not callable.")

        return executor

    @staticmethod
    def _error_envelope(skill_name: str, error: SkillRuntimeError) -> dict[str, Any]:
        return {
            "ok": False,
            "skill_name": skill_name,
            "schema_version": "1.0",
            "data": None,
            "error": {
                "code": error.code,
                "message": error.message,
                "details": error.details,
            },
            "runtime": {
                "duration_ms": 0,
                "model": "none",
                "mode": "deterministic-mock",
                "cost_estimate": 0,
            },
        }
