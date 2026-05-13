# storyboard_director

Internal Production Skill. This skill does not call a real model in Stage 7.

Input:

- `episode_writer`: a successful `episode_writer` runtime envelope.
- `character_bible`: a successful `character_bible` runtime envelope for the same project and episode.
- `style_bible`: a successful `style_bible` runtime envelope for the same project and episode.

Output:

- A reviewable shot list with duration, scene, characters, shot size, camera, lighting, visual action, dialogue, narration, sound note, prompt seed, and continuity notes.
- A timing summary that keeps the shot duration close to the episode target duration.
- A checkpoint before image prompt and video prompt expansion.

Quality bar:

- 30 to 60 second projects should default to 6 to 12 shots.
- Shot durations must sum to the target duration within 1 second.
- Every shot must be usable by later image and video prompt skills without reading a database.
- Character and style continuity must reference upstream `character_bible` and `style_bible` fields.

Forbidden:

- No model SDK calls.
- No database access.
- No direct UI integration.
- No FFmpeg, image, video, audio, or file asset generation.

