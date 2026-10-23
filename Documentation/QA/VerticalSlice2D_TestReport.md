# 2D Vertical Slice — Test Report

Date 2026-10-23 · Build `D:\game dev\Builds\Slice2D\TheLastAssessment.exe` (Unity 6000.6.6f1, Windows x64) ·
Display 1920×1080 · Nothing committed.

Logs and screenshots: `D:\game dev\Builds\Slice2D_logs\` (`01_compile.log`, `02_build_scene.log`,
`03_build_player.log`, `runs\*.log|png`). Tools: `Documentation/QA/tools/`.

## Automated (no input)

| Test | Result | Evidence |
|---|---|---|
| A1 script fidelity (`check_script_fidelity.py`) | **PASS**: 126 JSON entries match the `.md` word for word, in order (prologue–Q8). A negative test with one altered word fails as expected. | console |
| T1 / A2 compile | **PASS**: 0 errors; 0 new warnings (2 pre-existing CS0618 in `ChamberBuilder.cs`) | `01`, `03` logs |
| A3 scene structure | **PASS**: 1 director, 3 views, 3 dialogue panels, 1 question panel (4 options), 5 hotspots (2 Advance, 3 Disabled), 3 backgrounds, script reference | YAML inspection |
| T2 data validation at runtime | **PASS**: no `AssessmentScript:` errors in any player log | `runs\*.log` |
| T11 Windows build | **PASS**: 0 errors, 140 MB | `03` log |
| R1–R6 save recovery | **PASS**: no save → prologue; valid q1 → resumes Q1; past-slice q2, unknown node, corrupt JSON, empty file → prologue, never a black screen (screenshots confirm the scene renders) | `r1`–`r6` |
| T8 layout / letterbox | **PASS** at 1280×720, 1600×900, 1280×800 (16:10), 1600×686 (21:9), 1024×768 (4:3), 960×540, 1920×1080: uniform scale, black bars only outside 16:9, layout matches the reference (diff ≤ 4.1 / 255) | `res_*` |
| Figma match at 1920×1080 | Background diff 1.36 / 255; tablet 2.41; HUD in the same position, but font and panel opacity differ | `res_1920x1080_full.png` |
| A5 protected files | **PASS**: 3D scenes, Build Settings, ProjectSettings, script JSON, story `.md`, 3D director / builders, 3D save (`63abb226…`) unchanged | sha256 |

## Interactive (Claude drove the build with mouse and keyboard)

| Test | Result |
|---|---|
| T3 / A4 played text vs Script (`check_trace.py`) | **PASS**: full run 23 / 23 elements; resumed run 12 / 12 |
| T4 flow | **PASS**: 0.1 → tablet → 5 lines → 0.2 at the caption → 5 lines → Q1 → tablet → question + A–D → 6 lines → fade out |
| T5 state | **PASS**: `q1` answer, `valuation_143_seen`, `currentNode=q2`, written to `assessment2d_save.json` |
| T6 resume | **PASS**: quit mid-Q1 follow-up → relaunch resumes at Q1; re-answering replaces the answer (one `q1` entry) |
| T10 input | **PASS**: mouse-only and keyboard-only (Down focuses the tablet, Enter / Space advance, arrows + Enter choose); clicking empty wall does nothing; hover outline aligned on both tablets; Escape quits |
| "…" glyph | **PASS**: renders in LiberationSans |

## Not run, or partial

- **T7 3D gameplay:** files are hash-identical, but the 3D game was not launched.
- **Quit mid-0.2:** not run interactively. By design it resumes at the prologue start (the save
  holds `prologue`).
- **2560×1440 / 2560×1080:** this display is 1920×1080, so the same aspect ratios were tested at
  smaller sizes instead.
- **T9 readability vs Figma:** text is readable at 960×540 and up, but the fonts are not Figma's
  (S1), and the panel fill (α 0.9) lets the ceiling light show through the HUD. Confirm the
  opacity (S5).

## Known behaviour to confirm

- A past-the-slice or unreadable save restarts the prologue and overwrites the 2D save file. The
  3D save is never touched.
- The Q1 question appears only in the question box; the dialogue panel returns for the follow-up
  lines.
- Test-harness note: an early resolution run left 6 game processes open (a pipeline bug in the
  harness, not the game). They were closed and the runs repeated.
