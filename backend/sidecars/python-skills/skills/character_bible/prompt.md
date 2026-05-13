# character_bible

Internal Production Skill. This skill does not call a real model in Stage 6.

Input:

- `episode_writer`: a successful `episode_writer` runtime envelope.
- `character_preferences`: optional deterministic controls for locked names.

Output:

- Stable character cards with name, identity, personality, appearance, costume, voice profile, visual identity, and continuity rules.
- A review checkpoint before style, storyboard, image, or video prompt expansion.

Quality bar:

- At least one main character.
- Main character must be recognizable across close-up, medium shot, and back view.
- Character identity should reference `character_id` and `stable_seed` for later prompt reuse.
- The structure is ready for later database persistence, but this skill does not write to a database.

Forbidden:

- No model SDK calls.
- No database access.
- No direct UI integration.
- No FFmpeg, audio, video, or file asset generation.

