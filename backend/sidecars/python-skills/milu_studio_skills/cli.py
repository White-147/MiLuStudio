from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any

from .errors import SkillInputError
from .gateway import SkillGateway


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)

    if args.command == "run":
        return run_skill(args)

    parser.print_help()
    return 1


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="milu-skills",
        description="Run MiLuStudio internal Production Skills through a JSON file contract.",
    )
    subparsers = parser.add_subparsers(dest="command")

    run_parser = subparsers.add_parser("run", help="Run one Production Skill.")
    run_parser.add_argument("--skill", required=True, help="Skill name, for example story_intake.")
    run_parser.add_argument("--input", required=True, help="Input JSON file path.")
    run_parser.add_argument("--output", required=True, help="Output JSON file path.")
    run_parser.add_argument("--pretty", action="store_true", help="Write indented JSON.")

    return parser


def run_skill(args: argparse.Namespace) -> int:
    output_path = Path(args.output)

    try:
        payload = read_json(Path(args.input))
        result = SkillGateway.default().run(args.skill, payload)
    except SkillInputError as error:
        result = {
            "ok": False,
            "skill_name": args.skill,
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

    write_json(output_path, result, pretty=args.pretty)

    if not result["ok"]:
        print(result["error"]["message"], file=sys.stderr)
        return 2

    return 0


def read_json(path: Path) -> dict[str, Any]:
    try:
        with path.open("r", encoding="utf-8-sig") as handle:
            payload = json.load(handle)
    except FileNotFoundError as error:
        raise SkillInputError(f"Input file does not exist: {path}") from error
    except json.JSONDecodeError as error:
        raise SkillInputError(f"Input file is not valid JSON: {error}") from error

    if not isinstance(payload, dict):
        raise SkillInputError("Input JSON root must be an object.")

    return payload


def write_json(path: Path, payload: dict[str, Any], pretty: bool) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    json_text = json.dumps(
        payload,
        ensure_ascii=False,
        indent=2 if pretty else None,
        separators=None if pretty else (",", ":"),
    )
    path.write_text(json_text + "\n", encoding="utf-8")
