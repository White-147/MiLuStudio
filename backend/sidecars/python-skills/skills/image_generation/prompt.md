# image_generation

Internal Production Skill. This skill does not call a real image model in Stage 8.

Input:

- `image_prompt_builder`: a successful `image_prompt_builder` runtime envelope.

Output:

- Deterministic mock placeholder image asset records for each prompt request.
- An asset manifest grouped by shot and character reference.
- Cost and provider metadata that explicitly remain mock-only.
- A review checkpoint before real provider, retry, persistence, and selection work.

Quality bar:

- Each asset must preserve its source request id, prompt, negative prompt, aspect ratio, selected flag, and seed metadata.
- `file_written` must stay `false`.
- `asset_manifest.writes_files` and `asset_manifest.writes_database` must stay `false`.
- `storage_intent` may describe a future adapter target under `D:\code\MiLuStudio`, but this skill must not write files.

Forbidden:

- No model SDK calls.
- No image provider calls.
- No database access.
- No file writes or uploads.
- No direct UI integration.
- No FFmpeg, video, or audio work.

