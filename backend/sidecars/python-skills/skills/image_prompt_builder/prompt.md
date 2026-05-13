# image_prompt_builder

Internal Production Skill. This skill does not call a real image model in Stage 8.

Input:

- `storyboard_director`: a successful `storyboard_director` runtime envelope.
- `character_bible`: a successful `character_bible` runtime envelope for the same project and episode.
- `style_bible`: a successful `style_bible` runtime envelope for the same project and episode.

Output:

- Character reference image prompt requests derived from stable character identities.
- Per-shot `storyboard_image`, `first_frame`, and `last_frame` prompt requests.
- A merged negative prompt, reference strategy, and generation plan that explicitly avoids provider calls, file writes, and database writes.

Quality bar:

- Every request must carry an asset type, prompt, negative prompt, aspect ratio, style references, seed hint, and output slot.
- Shot requests must preserve the upstream shot id, character ids, camera hints, lighting hints, and continuity notes.
- Character reference requests must preserve stable character ids and seeds.
- The output must be consumable by the `image_generation` mock boundary without reading a database.

Forbidden:

- No model SDK calls.
- No image provider calls.
- No database access.
- No file writes or uploads.
- No direct UI integration.
- No FFmpeg, video, or audio work.

