from __future__ import annotations

from typing import Any

from .errors import SkillValidationError


def unwrap_skill_data(
    payload: dict[str, Any],
    field_name: str,
    expected_skill_name: str,
    context: str,
) -> dict[str, Any]:
    envelope = payload.get(field_name)
    errors: list[str] = []

    if not isinstance(envelope, dict):
        errors.append(f"{field_name} must be a skill envelope object.")
    else:
        if envelope.get("ok") is not True:
            errors.append(f"{field_name}.ok must be true.")

        if envelope.get("skill_name") != expected_skill_name:
            errors.append(f"{field_name}.skill_name must be {expected_skill_name}.")

        if envelope.get("schema_version") != "1.0":
            errors.append(f"{field_name}.schema_version must be 1.0.")

        data = envelope.get("data")
        if not isinstance(data, dict):
            errors.append(f"{field_name}.data must be an object.")

    if errors:
        raise SkillValidationError(f"{context} input validation failed.", errors)

    return envelope["data"]
