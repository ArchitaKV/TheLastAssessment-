# 2D Vertical Slice — Implementation Plan

Status: **awaiting approval** · Written 2026-10-23 · No code or scenes changed.

Slice scope: **0.1 Assessment Room → 0.2 Preliminary Valuation → 1.1 Familiarity (Q1)**.

Authority:
1. Dialogue, choices, and reveal order: `The_Last_Assessment_Complete_Story_Script.md`. Already
   stored verbatim in `Assets/Narrative/Data/AssessmentScript.json`, nodes `prologue` and `q1`.
2. Appearance: the Figma exports (`Downloads/Untitled (1).pdf`, 22 frames;
   `Downloads/Untitled.pdf`, the scene design bible).
3. Interaction ideas: `The_Last_Assessment_PointAndClick_Redesign.md`, used only where the Script
   doesn't conflict.

## 1. What the Figma exports contain

### Frame map

The frames are 1920×1080. PDF page order is **not** scene order: pages 9–12 come in the order
Q9, Q10, Q7, Q8.

| PDF page | Scene | Background bitmap | Figma text / overlays |
|---|---|---|---|
| 1 | 0.1 Assessment Room | room, dark, silhouette behind glass | HUD (SUBJECT A-4172 · ETERNA / CONTINUITY · STATUS ASSESSMENT PENDING), ASSESSOR dialogue panel, 3 choice buttons |
| 2 | 0.2 Preliminary Valuation | tablet close-up | PRELIMINARY VALUATION · 143 CREDITS; a valuation panel (vector; lost in the PDF render, only its drop-shadow bitmap survives — corrected 2026-10-23, it was first noted as a grain overlay) |
| 3 | 1.1 Q1 | room, closer angle on the tablet | HUD (ASSESSMENT IN PROGRESS), INTAKE / QUESTION 01, question box, ASSESSOR / DIALOGUE panel, RESPONSE OPTIONS A–D, SELECT OR SPEAK RESPONSE |
| 4 | 1.2 Q2 | room plus four 1024² memory images | MEMORY RECOGNITION / Q02, 01 MECHANISM · 02 CONTACT · 03 STRUCTURE · 04 IDENTIFICATION |
| 5 | 1.3 Q3 | tablet macro | PRELIMINARY VALUATION · 143 CREDITS |
| 6 | 1.4 Q4 | tablet with waveform | AUDIO / SIGNAL HELD |
| 7 | 2.1 Q5 | open-plan office | WORK SUMMARY / Q05 panel |
| 8 | 2.2 Q6 | office, man at a desk | Q06 / RECOVERED MEMORY quote |
| 9 | 3.1 Q9 | corridor | Q09 / REFERENCE MEMORY |
| 10 | 3.2 Q10 | filing archive | RESTRICTED · ACCESS LOCKED, with an icon |
| 11 | 2.3 Q7 | cleared desk | Q07 / ABSENCE |
| 12 | 2.4 Q8 | apartment, rain window | Q08 / PERSONAL MEMORY |
| 13 | 3.3 Q11 | monitor | EMPLOYMENT RECORD timeline, 3 YEARS, 4 MONTHS |
| 14 | 3.4 Q12 | recorder | --:4-:-- timestamp |
| 15 | 4.1 Q13 | papers on a desk | AUTHORIZATION · RESTRICTED RECORD |
| 16 | 4.2 Q14 | office, city view | Performance history (TIMESTAMP 02 listed twice) |
| 17 | 5.1 Q17 | corridor, door with figure | REFERENCE MEMORY: ACCESS RESTRICTED |
| 18 | 5.2 Q18 | bright room | RECORD COMPARISON panels, SUBJECT INSTANCE: ACTIVE |
| 19 | 4.3 Q15 | corridor, red light, hand | Q15 / MEMORY RECONSTRUCTION |
| 20 | 4.4 Q16 | room | PERMISSION DENIED · SOURCE: ETERNA ACCESS-CONTROL SYSTEM |
| 21 | 5.3 Q19 | archive hall behind glass | 143 CREDITS, 3 YEARS, 4 MONTHS, ASSESSMENT CYCLES, A-4172/73/74 PROFILE REPEATED |
| 22 | 5.4 Q20 | room | CONTINUITY GRID on the tablet, final dialogue panel, 4 options |

### Technical findings

- **Backgrounds are 1376×768 bitmaps**, which Figma scales to fill 1920×1080 (×1.406, cropping
  about 7 px on each side). They can be pulled losslessly from the PDF. At 1080p they will look
  slightly soft; better source files would be sharper, if they exist.
- **Text is flattened** into a Type 3 font called "FigmaGlyphs". The real font names are lost.
  Visually the sans looks like Inter and the mono like IBM Plex Mono or JetBrains Mono. This is a
  guess and needs confirming.
- **Some frames don't render** with the Windows PDF renderer (pages 2, 5, 18, 20 come out blank;
  page 3 loses its panels). Text positions and sizes can still be read from the PDF.
- **Sampled colours (to confirm):** panel fill ≈ `#060D10` (translucent black); cyan hairline
  border ≈ `#6EB8D6`; dim cyan labels ≈ `#355967`; body text white. This matches the bible:
  "thin cyan-blue outlines, white text, translucent black dialogue panels".
- Type sizes at 1080p from the PDF: HUD 16 / 13 px mono; speaker label 16 px; dialogue body
  26 px (frame 1) or 22 px (frame 3); choices 23 / 20 px; Q1 question 20 px; valuation 94 px.

### Missing from the exports

1. All **six ending** frames.
2. Title / main menu, **evidence journal**, **inspection popup**, pause / save / load.
3. Interaction states: hotspot hover outline, button hover / selected / disabled, the dialogue
   "continue" indicator (the bible's reference shows ▼ and `>` chevrons), Q2's enlarged image
   view, and Q2's cyan selected state.
4. A Q9 wall close-up, plus the four frames that render blank (2, 5, 18, 20) as PNGs.

## 2. Figma vs Script — new conflicts (Script wins)

| ID | Frame | Issue | Handling |
|---|---|---|---|
| F1 | 1 (0.1) | Choices "Where am I? / Who are you? / What is this place?" are not in the Script (earlier audit item C2). | Not shown. The prologue has no player choice. |
| F2 | 3 (Q1) | "SELECT OR SPEAK RESPONSE" implies voice input, which doesn't exist. | **Needs decision:** keep as is, or show "SELECT RESPONSE". |
| F3 | 1, 3, 21 | Subject IDs A-4172 / A-4173 / A-4174 are not in the Script. | **Needs decision:** keep as Figma UI text (proposed), or remove. |
| F4 | 10 (Q10) | An icon next to RESTRICTED may read as the authorisation mark before Q13 / Q19 (R7 / R8). | Out of slice. **Needs decision** before Q10. |
| F5 | 16 (Q14) | TIMESTAMP 02 appears twice, a clue the Script doesn't have (R9). | Out of slice. **Needs decision.** |
| F6 | 21 (Q19) | The Grid is shown as a physical archive behind the glass; in the Script it's an on-screen window that opens unprompted (R12). | Out of slice. Proposed: show the frame as the result of the unprompted event, with no player click. **Needs decision.** |
| F7 | 8 (Q6) | A man's face is clearly visible. The Script shows no person in Q6, and the voice stays unidentified. | Out of slice. **Needs decision.** |
| F8 | 2 (0.2) | The Redesign has the secondary value flicker in 0.2; in the Script that flicker is in **Q3** (`second_number_glimpsed`). | Not in 0.2. |

## 3. Slice flow (Script text only)

```
[fade in] 0.1 ROOM VIEW (frame 1 background + HUD "STATUS ASSESSMENT PENDING")
   hotspot: TABLET (functional, no text) ── click ──►
   prologue lines 1–5 in the dialogue panel (frame 1 layout), one line per click / Enter / Space
   line 6 = caption "PRELIMINARY VALUATION: 143 CREDITS" ──► cross-fade to
0.2 VALUATION VIEW (frame 2: tablet close-up, PRELIMINARY VALUATION / 143 CREDITS)
   prologue lines 7–11 continue in the dialogue panel   [flag valuation_143_seen; autosave]
   ──► cross-fade to
1.1 Q1 VIEW (frame 3 background, HUD "ASSESSMENT IN PROGRESS")
   hotspot: TABLET (functional) ── click ──► INTAKE / QUESTION 01 + question text + options A–D
   choice ──► record answer q1=A..D, autosave ──► Q1 follow-up lines (6) ──► end of slice
   end of slice: save points at q2; the screen fades out and the session ends.
```

- No player choice in 0.1 or 0.2 (Script). No Q3 flicker in 0.2 (F8).
- Hotspots that only start Script dialogue need no new text. They are the only active hotspots
  in the slice.
- The Prologue node is one block of lines in the data. The switch from 0.1 to 0.2 is triggered by
  its existing caption line, so **the data file needs no change**.

## 4. Hotspots that need approved text (rule 6)

None of these is written; they stay disabled until text, or a "visual only" ruling, is approved.

| Scene | Hotspot | Source of the hotspot | What's needed |
|---|---|---|---|
| 0.1 | Observation window / silhouette | Redesign: "inspect the silhouette" | Inspection text, or "no text" |
| 0.1 | Desk / workspace | Redesign: "inspect the workspace" | Inspection text, or "no text" |
| 0.2 | 143 CREDITS value | Redesign: "click the value to examine it" | Inspection text, or drop the hotspot |
| 0.1 | Lamp, camera, chair | In the artwork only | Not planned as hotspots unless you ask |

## 5. Architecture

### Unity structure

- **One new scene, `Assets/Scenes/Assessment2D.unity`.** Each scene in the story becomes a
  *view* (background plus hotspots) inside this one Unity scene, with cross-fades. That avoids 26
  near-identical Unity scenes and keeps the UI and state in one place.
- The existing `Prologue.unity`, `AssessmentChamber.unity`, `SampleScene.unity`, and the build
  settings are **untouched**. The slice is opened and played directly from `Assessment2D.unity`.
  Hooking it to a title screen comes later.
- Canvas: Screen Space – Overlay, Canvas Scaler "scale with screen size", reference 1920×1080,
  match 0.5.
- **Stage rect:** a 16:9 container (AspectRatioFitter, *Fit In Parent*) holds the background and
  hotspots. Other aspect ratios get black bars, so artwork is never stretched or cropped and
  hotspots always line up. The dialogue panel and HUD sit outside the stage, anchored to the
  screen.

### Components

| Component | New / reused | Responsibility |
|---|---|---|
| `AssessmentData` | **reused as is** | Parses the Script JSON and validates it |
| `AssessmentState` | **reused, one small change** | Answers, flags, current node, JSON save. The change is a configurable save file name, so the 2D slice (`assessment2d_save.json`) can't overwrite the 3D save. The 3D default is unchanged. |
| `ChoiceButton`, `AdvanceInput` | **reused as is** | Choice row and the click / Enter / Space advance |
| `StoryView` (ScriptableObject) | new | View id, background sprite, HUD status text, and hotspot list (id, rect in 1920×1080 stage pixels, action: `Advance` / `Inspect` / disabled) |
| `Hotspot` | new | Invisible UI Button over the stage. Cyan hairline outline on hover / focus, using `#6EB8D6` until Figma provides a hover state. Keyboard focusable. |
| `ViewPresenter` | new | Shows a view, cross-fades the background, shows the HUD, enables the view's hotspots |
| `DialoguePanel` | new (built to Figma frames 1 and 3) | Speaker label plus body text, a continue indicator, and the question box with options A–D |
| `StoryDirector2D` | new | Plays Script nodes as the existing `AssessmentDirector` does, and switches views on caption lines or node changes. It doesn't change `AssessmentDirector`; the 3D chamber keeps working. About 100 lines overlap with the 3D director, accepted so the 3D implementation stays untouched. |

Deferred until the slice works (Stage 3): the evidence journal (no Figma design yet; the slice
finds no evidence), the inspection popup (no design, no approved text), hidden variables
(rule 5), endings, and audio.

**Unity name clash:** the Redesign's `SceneManager` would clash with
`UnityEngine.SceneManagement.SceneManager`, so the plan uses `ViewPresenter` / `StoryDirector2D`.

### Assets

| Destination | Contents | Import settings |
|---|---|---|
| `Assets/Art/Backgrounds/` | `BG_0-1_AssessmentRoom.png`, `BG_0-2_Valuation.png`, `BG_1-1_Q1.png`, taken from the PDF, or your PNG exports | Sprite (Single), no mipmaps, Clamp, Bilinear, compression None (or High Quality), max size 2048 |
| `Assets/Art/UI/` | 9-slice panel and border sprites (or plain Images with an Outline) | Sprite, 9-slice borders set |
| `Assets/Fonts/` | The confirmed sans and mono fonts, plus TMP SDF assets | TMP Font Asset Creator, dynamic |

## 6. Ready now vs blocked

### Can be built as soon as you approve the plan

- Backup (§7), the `AssessmentState` save-name change, and the new scripts in §5.
- The `Assessment2D` scene with stage, HUD, dialogue panel, question box, and options, laid out
  to frames 1 and 3 using the PDF's sizes and positions and the sampled colours.
- The slice backgrounds pulled from the PDF (Figma's own bitmaps, not stock or placeholders).
- The tablet hotspots in 0.1 and Q1, the full slice flow, autosave and resume.
- Text uses the project's existing LiberationSans until the fonts are confirmed. **That is a
  known visual gap, not final.**

### Blocked on Figma

- Font family names and weights (or font files): needed for the final look.
- PNG exports of frame 2 (0.2) and frame 3 (Q1) **with UI layers visible**, so the panels the
  renderer dropped can be matched exactly. Also wanted: Figma's exact colour and opacity values
  for panels and borders.
- Hover, selected, and continue-indicator states (otherwise the bible's chevron / ▼ treatment is
  used).
- Higher-resolution backgrounds, if they exist (optional).

### Blocked on narrative approval

- Hotspot text in §4 (window / silhouette, desk, value).
- F2 (SELECT OR SPEAK) and F3 (subject IDs) on-screen text.

## 7. Backup before editing

Nothing is committed (rule 2). Before the first edit:

1. `git status --porcelain` is recorded to `Documentation/QA/` with the date.
2. `git diff > D:\game dev\Backups\TheLastAssessment_<date>_pre2D.patch` captures all tracked
   modifications.
3. A zip of `Assets/`, `Documentation/`, `Packages/`, and `ProjectSettings/` (excluding
   `Library/` and `Temp/`) goes to `D:\game dev\Backups\TheLastAssessment_<date>_pre2D.zip`. This
   covers untracked files, which a patch misses.
4. The zip is checked by listing its contents and comparing file counts.

## 8. Tests

Unity's Test Framework package isn't installed, and adding it changes dependencies. The plan
uses the checks below. Package-based EditMode tests can be added if you approve them.

| # | Check | How | Pass criterion |
|---|---|---|---|
| T1 | Compiles | Unity batch mode `-quit -logFile`, then search the log for `error CS` | 0 compile errors; existing 3D scripts still compile |
| T2 | Data valid | `AssessmentData.Validate` runs on Start | No errors logged |
| T3 | Script fidelity | Editor play-through, comparing every displayed line and option with the Script | Word-for-word match; no extra lines or choices; no Q3 flicker in 0.2 |
| T4 | Flow | Play the slice | Room → tablet → 5 lines → 0.2 view at the caption → 5 lines → Q1 view → tablet → question + A–D → follow-up → end |
| T5 | State | Pick each of A–D in separate runs, then inspect `assessment2d_save.json` | `q1` answer recorded; `valuation_143_seen` set; `currentNode` = `q2` |
| T6 | Resume | Quit mid-0.2 and mid-Q1, then replay | Resumes at the saved node; no duplicate answers |
| T7 | 3D intact | Open `Prologue.unity` → play | 3D chamber flow behaves as before; its save file is untouched |
| T8 | Hotspot alignment | Game view at 1920×1080, 2560×1440, 1280×720, 2560×1080, 1280×800, with debug outlines on | Tablet outline covers the tablet at every size; letterbox, never stretched |
| T9 | Readability | Screenshots at 1280×720 and 1920×1080 | Text size and positions match frames 1 and 3; nothing clipped |
| T10 | Input | Mouse only; keyboard only (Tab / Enter / Space / arrows) | The whole slice is completable both ways |
| T11 | Windows build | Build `Assessment2D` to a temp folder, run, play the slice | Launches and completes the slice |

Screenshots go to `Assets/Screenshots/slice2d_*.png`, the existing project convention, and
results go to `Documentation/QA/VerticalSlice2D_TestReport.md`.
