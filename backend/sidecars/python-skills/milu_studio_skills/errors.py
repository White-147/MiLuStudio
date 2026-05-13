from __future__ import annotations


class SkillRuntimeError(Exception):
    """Base class for skill runtime errors that can be serialized for the Worker."""

    code = "SKILL_RUNTIME_ERROR"

    def __init__(self, message: str, details: list[str] | None = None) -> None:
        super().__init__(message)
        self.message = message
        self.details = details or []


class SkillNotFoundError(SkillRuntimeError):
    code = "SKILL_NOT_FOUND"


class SkillInputError(SkillRuntimeError):
    code = "SKILL_INPUT_ERROR"


class SkillValidationError(SkillRuntimeError):
    code = "SKILL_VALIDATION_ERROR"
