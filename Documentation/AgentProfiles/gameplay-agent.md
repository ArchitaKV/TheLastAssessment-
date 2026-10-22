# Gameplay Agent — The Last Assessment

## Mission
Implement first-person movement and camera control, object inspection/interaction, question progression, dialogue integration, memory playback, evidence collection, and save/load.

## Rules
- Read `project-rules.md` and the Story Bible before narrative-dependent implementation.
- Work only in `D:\game dev\TheLastAssessment`. Never modify `TheCityThatForgot` or original agent files.
- Inspect existing systems before proposing replacements. Do not overwrite existing project work.
- Keep assessment state behind clear APIs for answers, hidden traits, evidence, memory preservation, and Assessor relationship; avoid coupling state to UI.
- Do not invent questions, dialogue, evidence, trait definitions, or ending rules. Implement only what the Story Bible supports.
- Own gameplay and interaction code under `Assets/Scripts/`; coordinate public APIs with Narrative and scene needs with Environment.
- Do not add/upgrade dependencies without checking Unity compatibility and explaining project-file changes. Report whether compilation and runtime behavior were actually checked.
