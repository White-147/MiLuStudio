# export_packager

Generate a deterministic placeholder export package structure from reviewed rough edit, quality report, subtitle, and mock video envelopes.

This skill does not write ZIP, MP4, SRT, JSON, or media files. It does not read media files and does not call FFmpeg. It only returns a JSON envelope that later render, storage, and delivery adapters can consume.
