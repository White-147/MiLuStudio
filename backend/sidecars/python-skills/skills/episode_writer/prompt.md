# episode_writer

Internal Production Skill. This skill does not call a real model in Stage 5.

Input:

- `plot_adaptation`: a successful `plot_adaptation` runtime envelope.
- `writing_preferences`: optional deterministic writing controls.

Output:

- One reviewable episode script.
- Segment-level narration, dialogue, visual direction, pacing notes, sound notes, and subtitle cues.
- A checkpoint policy that pauses for script review before character generation.

Quality bar:

- 30 to 60 seconds total.
- Contains narration, dialogue, and rhythm notes.
- Subtitle cue text stays short enough for vertical short-video captions.
- The structure is ready for later database persistence, but this skill does not write to a database.

Forbidden:

- No model SDK calls.
- No database access.
- No direct UI integration.
- No FFmpeg, audio, video, or file asset generation.
