# The Last Assessment

A cinematic 2D point-and-click psychological mystery, built in Unity from Figma designs.

You wake up in a sealed assessment room. An Assessor explains that you have died and that your memories
must now be evaluated, one question at a time, before your "continuity" is decided. Over twenty questions
the records stop adding up: a gap in your employment history, a recording you were never meant to hear,
an authorisation mark you have seen before. How you answer decides which of **six endings** you reach.

## Playing

The game is a Windows build. In Unity, build it with `Assessment2DBuilder.BuildPlayer` (see *Building*),
then run `TheLastAssessment.exe`.

| Action | Mouse | Keyboard |
|---|---|---|
| Advance dialogue | Click anywhere | Enter / Space |
| Choose an answer | Click it | ↑ ↓, then Enter |
| Use a story object (tablet, recorder) | Click it when the game waits for it | Any key focuses it, then Enter |
| Look at objects in the scene | Hover / click (outline + ring cursor) | — |
| Pause menu (Resume, Evidence, Quit) | — | Escape |
| Evidence journal | — | J |
| Ending screens | ← / NEXT / NEW RUN buttons | ← → / Enter |
| Toggle full screen | — | Alt + Enter |

Progress autosaves after every question; **Continue** on the title screen resumes it.

## How it works

The game is **data-driven**: the C# code contains no story text. Everything shown comes from JSON
generated from the canonical script, so the game always matches it word for word.

```
Story script (Markdown) ──Tools/story2d──► AssessmentScript2D.json ─┐
Figma frames (PNG + layout) ────────────► Scenes2D.json + Art/ ────┼─► StoryDirector2D (one Unity scene)
Approved ending rules ──────────────────► EndingRules2D.json ──────┤
Figma ending screens ───────────────────► EndingScreens2D.json ────┘
```

- **One scene, many views.** `Assets/Scenes/Assessment2D.unity` holds every Figma frame as a `StoryView`
  (background, panels, hotspots, overlays) on a fixed 1920×1080 stage that letterboxes to any screen.
- **Story engine.** `StoryDirector2D` is a coroutine state machine: each question node plays its lines,
  asks, plays any answer-specific branch, then moves on. Script lines are dialogue, captions or visual cues
  (`view:3.4`, `show:Mark`, `await:Hotspot_Recorder`, `fade:white`, `glitch`, `flag:…`, `evidence:…`).
- **Endings.** `EndingRules` checks the six endings in priority order against the recorded answers and
  flags; the first match wins. After the ending sequence come the Figma **Reveal → Epilogue** screens.
- **Saves.** JSON in the player's `AppData/LocalLow` folder, separate from the legacy 3D prototype's save.
- **Presentation.** Cross-dissolves with a slow camera push, idle camera drift, sliding panels, a looping
  background music track that ducks under dialogue, and hover/click feedback on objects in the scenes.
  Presentation never changes story content: optional object clicks are visual only.

## Project layout

| Path | Contents |
|---|---|
| `Assets/Scripts/PointAndClick/` | 2D game runtime (director, panels, hotspots, menus, endings, music, motion) |
| `Assets/Scripts/Editor/Assessment2DBuilder.cs` | Builds / updates the scene from data and makes player builds |
| `Assets/Narrative/Data/` | Story, scene layout, ending rules and ending-screen data (JSON) |
| `Assets/Art/` | Backgrounds, clue images and ending images exported from Figma |
| `Assets/Audio/` | Background music |
| `Tools/story2d/` | Python generator: canonical script → `AssessmentScript2D.json` |
| `Documentation/` | Story script, story bible, design notes and decisions |
| `Documentation/QA/tools/` | Automated test runners and checkers |
| `Assets/Scenes/AssessmentChamber.unity`, `Prologue.unity` | Legacy 3D prototype (kept, not part of the 2D game) |

## Building

Requires **Unity 6 (6000.6.6f1)** with URP, TextMeshPro and the Input System.

```
Unity.exe -batchmode -quit -projectPath <this folder> -executeMethod Assessment2DBuilder.BuildPlayer
```

The player is written to `D:/game dev/Builds/Game2D/TheLastAssessment.exe` (set by `PlayerPath` in the
builder; change it on another machine) from an explicit scene list, so the project's Build Settings are
not changed. `Assessment2DBuilder.CleanBuildPlayer` does the same without the
build cache.

## Testing

The game has command-line QA hooks (`-autoplay`, `-answers q12=B,…`, `-resume`, `-shots <dir>`, `-newrun`)
and writes every displayed line and cue to `Player.log` as `[Slice2D] …`. Python checkers compare those
logs with the script:

- `check_script_fidelity.py`: the story data matches the canonical script word for word.
- `check_trace.py`: what the game displayed matches the script for the path played.
- `check_cues.py`: visual cues happen in script order, plus the story locks: Q4's fragment plays once and
  can't be replayed, Q13's mark appears only at its scripted line, and Q19's archive reveal needs no
  player action.
- `check_endscreens.py`: each ending shows Reveal → Epilogue with the right text.

PowerShell runners play every ending-rule path (18), all six endings, NEW RUN, and three aspect ratios
(16:9, 21:9, 4:3).

## Known open items

- The fonts are a LiberationSans stand-in for the Figma fonts (Inter and IBM Plex Mono).
- Some UI text contains placeholders from the script that are still to be filled in: `[NOT PROVIDED]`,
  `[DATE]`, `RECORD / [ID]` and `[TIMESTAMP 01–03]`.
