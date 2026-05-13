# style_bible

Internal Production Skill. This skill does not call a real model in Stage 6.

Input:

- `episode_writer`: a successful `episode_writer` runtime envelope.
- `character_bible`: a successful `character_bible` runtime envelope for the same project and episode.

Output:

- Visual style, palette, lighting, environment design, camera language, character rendering rules, negative prompt, and reusable prompt blocks.
- Guidelines that can be reused by later image prompt and video prompt skills.
- A review checkpoint before storyboard and prompt expansion.

Quality bar:

- Style rules must preserve character consistency from `character_bible`.
- Palette and negative prompt must be reusable across image and video steps.
- Camera language must respect vertical short-video subtitle safety.
- The structure is ready for later database persistence, but this skill does not write to a database.

Forbidden:

- No model SDK calls.
- No database access.
- No direct UI integration.
- No FFmpeg, audio, video, or file asset generation.

