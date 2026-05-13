# plot_adaptation

Internal Production Skill. This skill does not call a real model in Stage 5.

Input:

- `story_intake`: a successful `story_intake` runtime envelope.
- `adaptation_preferences`: optional deterministic preferences.

Output:

- A four-beat short-video plot structure.
- A title, logline, conflict, turning point, ending hook, review points, and continuity notes.
- Estimated beat durations that sum to the requested 30 to 60 second target.

Quality bar:

- Preserve one clear protagonist, one core mystery, and one ending hook.
- Keep the output reviewable by a user before character and storyboard stages.
- Do not include model provider details, database writes, file paths, or downstream FFmpeg assumptions.

Forbidden:

- No real model SDK calls.
- No database access.
- No direct file-system business writes.
- No public plugin marketplace behavior.
