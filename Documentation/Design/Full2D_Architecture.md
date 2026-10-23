# The Last Assessment — Full 2D Architecture Assessment

Status: **assessment for approval** · Written 2026-10-23 · No refactor started, nothing committed.

Direction: the whole game is 2D point-and-click, using the Figma frames. The 3D chamber
(`Prologue.unity`, `AssessmentChamber.unity`, `ChamberBuilder`, `AssessmentDirector`,
`AssessmentPresentation`) is kept as a recoverable legacy prototype. It is not on the required
gameplay path.

## 0. Current state

### Verified so far

| Item | Result | How |
|---|---|---|
| Unity Editor closed before batch mode | Yes (lockfile gone; only the Hub service running) | process list |
| Backup zip, patch, working-tree report | Present (321 / 321 files); Prologue as saved at 12:49 also copied to `Backups/` | file listing, hashes |
| Compile | **0 errors, 0 warnings** from the new code. 2 pre-existing CS0618 warnings in `ChamberBuilder.cs`. | batch-mode logs `Builds/Slice2D_logs/01_compile.log`, `03_build_player.log` |
| `Assessment2D.unity` | Generated and saved. Contains 1 director, 3 views, 3 dialogue panels, 1 question panel with 4 options, 5 hotspots (2 tablets Advance; window, desk, value Disabled), 3 background references, and the script JSON reference. | batch builder plus YAML inspection |
| Windows player | **Built**: `D:\game dev\Builds\Slice2D\TheLastAssessment.exe`, 140 MB, 0 errors | batch log |
| Protected files | Unchanged: the 3D scenes, `EditorBuildSettings.asset`, `ProjectSettings.asset`, script JSON, story `.md`, 3D director and builders, and the 3D save (`assessment_save.json`, still at q7) | sha256 against `Backups/hashes_pre_verification.txt` |
| **Play tests T3–T11** | **Not run.** Interrupted by the direction change. | — |

### Side effects to note

- The player build re-serialized `Assets/Settings/PC_RPAsset.asset`,
  `UniversalRenderPipelineGlobalSettings.asset`, and `ProjectSettings/GraphicsSettings.asset`.
  Only URP shader-variant prefiltering values changed. Unity regenerates these on any build. They
  are not reverted, per the "no discard" rule; the original values are in the backup zip.
- `Assets/_Recovery/0.unity` (12:18) is Unity's auto-recovery copy of the Prologue scene, made by
  the Editor session. It predates the 2D work and is left untouched.

## 1. Does the current code support an all-2D game?

**No. It supports the opening slice and scenes shaped like it, not the full game.**

What it handles: one background per Script node, an Advance hotspot (click to continue), Script
lines one at a time, one A–D question, a caption that switches scenes (0.1 → 0.2), letterboxing
at 16:9, a separate save file, and resume.

What it lacks:

| Gap | Why the full game needs it |
|---|---|
| Scene data lives in builder code, one hand-written method per frame | 20+ scenes would duplicate the same layout code |
| No **timed beats** inside a node | Q2 images appear and vanish; Q3 number shrinks and a second figure flickers; Q4 waveform cuts off; Q10 PARTIAL notice; Q15 lights dim; Q17 door glitch and message flash; Q18 fields appear, then the documents vanish; Q19 window opens unprompted, pages load |
| No **documents / overlays** | Q5 summary, Q11 timeline, Q14 history, Q16 denial, Q18 comparison, Q19 Grid pages |
| No **memory scene within a question** | Q7, Q9, Q15, and Q17 cut from the chamber to a reconstruction and back |
| No choice-dependent playback | Q12 (rules pending) |
| No evidence store or journal; no inspection popup | Evidence and memory inspection |
| No ending evaluation or ending scenes | Six endings |
| No title screen, New / Continue, or load UI | Save/load from a menu |
| Script data covers only **Prologue–Q8** | Q9–Q20 and the endings are in the `.md` only |
| No audio | Q4, Q6, Q12, Q15 voices, alarms, static |

## 2. Proposed architecture

```
Assets/Narrative/Data/AssessmentScript.json   canonical text only (verbatim from the .md), plus cue ids
Assets/Data/Scenes2D.json                     presentation only: one entry per designed scene
Assets/Scenes/Game2D.unity                    the single runtime scene for the 2D game
Assets/Scenes/Title2D.unity                   title / New / Continue (when designed)
Assets/Art/Backgrounds/  Assets/Art/UI/  Assets/Art/Clues/  Assets/Audio/  Assets/Fonts/
```

### Data, in two files

1. **`AssessmentScript.json`, the story:** nodes, lines, questions, choices, captions, and flags,
   all copied word for word from the `.md`. Stage directions that need a visual event become
   `visual` cue lines. The prologue–Q8 data already does this (`valuation_to_corner`,
   `second_figure_flicker`, `waveform_play`, `waveform_cut`, `employment_summary_show`,
   `office_show`, …). Endings become nodes `ending1` … `ending6`. **An automated check (A1, §6)
   proves every line matches the `.md` exactly and in order.**
2. **`Scenes2D.json`, the presentation:** for each Figma scene, a record with id ("0.1"), Figma
   page, background file, entry trigger (node start, caption, or cue), HUD text, hotspot list
   (rect in 1920×1080, action), the documents it can show, and its dialogue / question panel
   style. No story text lives here except Figma's UI labels.

### Runtime (one scene; generalised from the slice)

| Component | Role |
|---|---|
| `StoryDirector2D` → generalised into **`SceneFlow`** | Plays nodes, routes cue lines to the presenter, asks questions, records state, autosaves, runs the ending evaluator after Q20 |
| `StoryView` | One designed scene (built from `Scenes2D.json`) |
| **`CueRunner`** (new) | Maps cue ids to small, reusable effects: show / hide document, flicker, shrink-to-corner, fade / dim, glitch-flash, enter / leave a memory view, audio play / cut |
| **`DocumentPanel`** (new, one generic panel plus Figma-styled variants) | Timeline, summary, history, comparison, denial, and Grid pages; text from the Script or Figma UI labels only |
| `Hotspot` | As is; adds an `Inspect` action (approved text only) |
| **`InspectPanel`** (new) | Shows approved inspection text or an enlarged clue image (e.g. Q2 images) |
| `DialoguePanel`, `QuestionPanel`, `ChoiceButton`, `AdvanceInput`, `StageFit` | As is |
| **`EvidenceStore`** (inside `AssessmentState`) and **`EvidenceJournal`** UI (new) | Unique ids, no duplicates, found-in-scene. Only items the Script names, or that you approve. |
| **`EndingEvaluator`** (new) | A rules table from your approved conditions; no rules invented |
| **`TitleScreen2D`** (new, can adapt `PrologueScreen`'s New / Continue logic) | 2D save file; legacy 3D title unchanged |

### Editor and build

- `Assessment2DBuilder` becomes a **data-driven builder**: it reads `Scenes2D.json`, imports
  backgrounds, and generates every view with the same panel and hotspot helpers. Unique document
  layouts get one builder function each.
- **Build Settings:** switching to 2D-only scenes means editing `EditorBuildSettings.asset`, which
  needs your approval. Until then, players are built with an explicit 2D scene list, as now, so
  the 3D settings stay intact.

## 3. Reuse, add, replace

| Keep as is | Generalise | Add | Legacy (kept, not used by 2D) |
|---|---|---|---|
| `AssessmentData`, `AssessmentState` (+ `SaveFileName`), `ChoiceButton`, `AdvanceInput`, `StageFit`, `Hotspot`, `DialoguePanel`, `QuestionPanel`, script JSON prologue–Q8, the 3 imported backgrounds | `StoryDirector2D` → `SceneFlow` (cues, documents, memory views, endings); `StoryView` (built from data); `Assessment2DBuilder` (data-driven) | `Scenes2D.json`, `CueRunner`, `DocumentPanel`, `InspectPanel`, `EvidenceStore` / `EvidenceJournal`, `EndingEvaluator`, `TitleScreen2D`, a script-fidelity checker, a trace checker | `Prologue.unity`, `AssessmentChamber.unity`, `ChamberBuilder`, `PrologueBuilder`, `AssessmentDirector`, `AssessmentPresentation` (its cue names are reused), `PrologueScreen`, `AssessorEyePulse`, chamber materials |

## 4. Figma frames → canonical story

PDF page order is not story order (pages 9–12 = Q9, Q10, Q7, Q8).

| Story | Script beat that needs a visual | Figma page | Coverage |
|---|---|---|---|
| Prologue 0.1 | Room, Assessor opening | 1 | Full |
| Prologue 0.2 | "PRELIMINARY VALUATION: 143 CREDITS" | 2 | Background only renders; needs a PNG |
| Q1 | Question | 3 | Panels need a PNG |
| Q2 | Four images, then the badge image disappears | 4 (4 images included) | Good; labels need approval (V1) |
| Q3 | 143 shown → shrinks to corner → second figure flickers | 5 | Background only renders; corner / flicker states not designed |
| Q4 | Waveform, "You promised—", cut | 6 | Good; audio missing |
| Q5 | Employment summary, a colleague's name | 7 | Good; the name is not in the Script (N2) |
| Q6 | Voice returns | 8 | Conflict V5 (face) |
| Q7 | Reconstructed office, cleared desk with a mark | 11 | Good |
| Q8 | (no scene in the Script) | 12 | Conflict R5 (adds scene and clues) |
| Q9 | Corridor, blank wall where a door should be | 9 | Good; wall close-up not designed |
| Q10 | *ARCHIVE REFERENCE: PARTIAL* notice | 10 | Conflict V2 (icon, "ACCESS LOCKED"); the PARTIAL notice is not in the frame |
| Q11 | Timeline, 3 years 4 months gap | 13 | Good; dates are placeholders (N3) |
| Q12 | Audio file, incomplete metadata | 14 | Good; Q12 rules open |
| Q13 | Authorisation mark on screen | 15 | Check that it shows no comparison (V3) |
| Q14 | Looks back at the timeline | 16 | Conflict V4 (duplicated timestamp) |
| Q15 | Room dims, rain on the glass; memory: corridor, rain, red light | 19 | Memory OK; the chamber dim / rain state is not designed |
| Q16 | Request denied by Eterna's access-control system | 20 | Good (overlay needs a PNG) |
| Q17 | Corridor door appears / vanishes; *REFERENCE MEMORY: ACCESS RESTRICTED* | 17 | Good; the report-filling UI is not designed |
| Q18 | Two documents side by side; *CONTINUITY STATUS: UNAUTHORISED*, *SUBJECT INSTANCE: ACTIVE* | 18 | Labels need approval (V6); renders blank, needs a PNG |
| Q19 | Window opens unprompted: CONTINUITY GRID — INTERNAL RECORD, pages, 143, timestamps | 21 | Conflict V7 (archive hall behind glass) |
| Q20 | Grid open beside the window; final question | 22 | Good |
| Endings 1–6 | Each ending's beats | — | **No frames** |
| Title, journal, inspect, save / load, states | — | — | **No frames** |

## 5. Conflicts and gaps needing approval

### Already logged (OpenNarrativeDecisions.md)

- E1–E8: ending rules.
- Q12-1 to Q12-6: Q12 outcomes.
- R1–R16: redesign vs Script. Your rulings so far: R3 (no Q4 replay), R7 / R8 (no mark
  comparison before Q19), and R12 (Q19 unprompted) follow the Script.
- F2 and F3: approved to keep.

### New visual conflicts (V)

| ID | Frame | Issue |
|---|---|---|
| V1 | Q2 (p4) | Image labels "01 / MECHANISM · 02 / CONTACT · 03 / STRUCTURE · 04 / IDENTIFICATION" and title "MEMORY RECOGNITION" are not in the Script. Keep as UI labels? |
| V2 | Q10 (p10) | The RESTRICTED badge's icon could read as the authorisation mark before Q13 / Q19, and the Script's *ARCHIVE REFERENCE: PARTIAL* notice is absent. Proposed: hide the icon and show the PARTIAL notice in the frame's style. |
| V3 | Q13 (p15) | "AUTHORIZATION · RESTRICTED RECORD" papers: confirm nothing in the art matches the Q19 mark in a way that gives the reveal away. |
| V4 | Q14 (p16) | TIMESTAMP 02 listed twice: an unsupported clue. Proposed: remove the duplicate row. |
| V5 | Q6 (p8) | A clearly visible face. The Script shows no one and keeps the voice unidentified. |
| V6 | Q18 (p18) | Panel labels "ACCESS PANEL SIGNAL / CORRIDOR OBSERVATION / CONFIDENCE HIGH / LOW" are not in the Script. Keep as UI labels, or use Script wording only? |
| V7 | Q19 (p21) | Physical archive hall behind the glass vs the Script's on-screen window that opens unprompted. Proposed: the window opens by itself (no click) and shows this frame's records. |
| V8 | Q19, Q20, Ending 5 | The Script refers to "the city beyond the glass", but the room frames show a dark observation window with a silhouette (no city). |
| V9 | Q15 | The Script's chamber beat (lights dim, rain on the glass wall) has no frame. Only the corridor memory (p19) exists. |

### Narrative gaps (N): text the Script doesn't supply

| ID | Where | Missing |
|---|---|---|
| N1 | Q3 | The value of the "another figure" that flickers |
| N2 | Q5 | The former colleague's name |
| N3 | Q11, Q14, Q19 | Timeline dates, evaluation timestamps, the final-night date |
| N4 | Q19 | The protagonist's name and employment number (the Script says they appear; the Figma frame uses "A-4172") |
| N5 | All | Hotspot inspection text (Plan §4), and evidence-journal titles / descriptions |
| N6 | Q17 | Discrepancy report form wording |

### Assets still needed

- Six ending frames.
- Title, journal, inspect, and save / load screens.
- Hover / selected states.
- PNGs of pages 2, 3, 5, 18, 20 with their UI layers.
- Font names.
- Audio: voice lines (Q4, Q6, Q12, Q15), hum, alarm, static.
- The Q3 corner / flicker state, the Q15 dimmed-chamber state, and the Q9 wall close-up.

## 6. Staged plan and tests

### Automated tests (run each stage)

| ID | Test | Method |
|---|---|---|
| A1 | Script fidelity | Python: every line, choice, and caption in the JSON appears word for word, in order, in the `.md`, and nothing in the JSON is missing from it |
| A2 | Compile | Batch mode; zero `error CS` |
| A3 | Scene build self-check | The builder verifies every node has a view, every sprite exists, every hotspot rect is inside the stage, and no reference is null; non-zero exit on failure |
| A4 | Played-path fidelity | Player.log `[Slice2D]` trace compared with the JSON for each tested path |
| A5 | Protected files | sha256 of the 3D scenes, Build Settings, story `.md`, and 3D save |
| A6 | Player build | Batch build; 0 errors |

### Manual / interactive checks (run on the built player)

| ID | Check |
|---|---|
| M1 | Mouse-only and keyboard-only playthrough of each new group |
| M2 | Screenshots compared with the Figma frames at 1920×1080 |
| M3 | Hotspot alignment at 1280×720, 1920×1080, 2560×1440, 2560×1080, 1280×800 |
| M4 | Save / quit / resume mid-group; past-the-end and corrupt save recovery |

### Stages

| Stage | Scope | Done when |
|---|---|---|
| **1. Finish the slice** (no refactor) | Run T3–T11 on the existing build: trace check, mouse / keyboard, resume, invalid / past-end save, 3D intact, 5 resolutions. Write the QA report. | All tests run and reported |
| **2a. Framework** | `Scenes2D.json`, data-driven builder (rebuild 0.1–Q1 from data with identical results), `SceneFlow` + `CueRunner` + `DocumentPanel`, A1 and A3 checks | The slice still passes, now data-driven |
| **2b. Script data** | Add Q9–Q20 and the endings to the JSON verbatim, with cue ids | A1 passes for the whole Script |
| **2c–2g. Migrate scenes** in groups | Act I (Q2–Q4) → Act II (Q5–Q8) → Act III (Q9–Q11, Q12 without branching) → Act IV (Q13–Q16) → Act V (Q17–Q20). Each needs its V / N items decided first. | A1–A6 and M1–M4 for each group |
| **3. State, evidence, endings** | Evidence store and journal, Q12 branches, ending evaluator, ending scenes, title / New / Continue | Needs E1–E8, Q12, N5, ending frames, journal design |
| **4. Polish** | Fonts, hover states, the valuation panel and drop shadows, audio, transitions, accessibility, the 2D-only Build Settings switch, final Windows build | Needs fonts, audio, and approval to edit Build Settings |
