# The Last Assessment — Full 2D Game: Build, Test and Decision Report

Date 2026-10-23 · Build `D:\game dev\Builds\Game2D\TheLastAssessment.exe` (Windows x64, 199 MB, launches
straight into the 2D title screen) · Unity 6000.6.6f1 · Nothing committed.
Logs: `D:\game dev\Builds\Game2D_logs\` (`01`–`04_build.log`, `final\*.log`, `final\shots\`).

## What was built

| Area | Implementation |
|---|---|
| Story data | `Assets/Narrative/Data/AssessmentScript2D.json`, **generated** from the Script by `Tools/story2d/build_story2d.py`: prologue, Q1–Q20, Q12 branch, six endings (27 nodes). Visual cues come from `Tools/story2d/cues.json`, anchored to the Script's own stage directions. The 3D `AssessmentScript.json` is untouched. |
| Scenes | `Assets/Narrative/Data/Scenes2D.json`: 20 views from the 18 unique Figma backgrounds, Q2 images, and the Q17 door layer, at Figma coordinates. `Assessment2DBuilder` generates `Assets/Scenes/Assessment2D.unity` from it. |
| Runtime | `StoryDirector2D` (nodes, cues, act cards, Q12 branch, endings, autosave / resume), `StoryView`, `QuestionPanel` (tablet / card / image-tile styles), `DialoguePanel`, `Hotspot`, `CaptionTarget`, `Waveform`, `GameMenus` (title, New / Continue with confirmation, pause, evidence journal), `EndingRules`, `StageFit` (16:9 letterbox). |
| Effects (code) | cross-fade, fade black / white, cut, dim, flash, flicker, shrink-to-corner (Q3), glitch, waveform play / faint / cut (Q4 / Q6, no replay), door flash (Q17), unprompted Grid window (Q19). |
| Endings | All six ending sequences are built and play the Script text. **Selection needs approved rules:** `EndingRules2D.json` ships with `"approved": false` and no rules, so a normal playthrough ends at the Script's "ASSESSMENT COMPLETE. / OUTCOME PENDING." and returns to the title. |
| Q12 | Answer A plays the Script's listening sequence and adds the recording to evidence. B / C / D show no extra content (the Script has none). Which answers listen / retain is one data list (`branches[0].choices`). |
| Saves | Separate `assessment2d_save.json` (3D save untouched); autosave at every node / answer / caption / evidence; a completed or invalid save cannot be continued. |

## Tests actually run

| Test | Result |
|---|---|
| Script fidelity (`check_script_fidelity.py`, 2D data) | **PASS**: 393 displayed entries match Markdown lines 15–1166, in order. 3D data also still passes. |
| Compile + scene build + Windows player (batch) | **PASS**: 0 errors, 4 builds (`01`–`04_build.log`) |
| Full playthroughs, automated (`-autoplay`, final build), trace vs data (`check_trace.py`) | **PASS** ×3: all-A with Q12 = A (377 / 377), all-B (363 / 363), all-C (363 / 363); 0 runtime errors (no missing view, element, or hotspot) |
| Endings 1–6 (forced with the `-ending` test flag, resumed at Q20) | **PASS** ×6: 17–22 elements each, exact; 0 runtime errors |
| Q12 branch | **PASS**: A → recording heard + evidence recorded; B / C → none |
| Visual check (screenshots of every view, question, and act card) | All 20 views and every question panel render with their Figma backgrounds. Found and fixed: invisible act cards, a black Q17 reveal, stale black-view text. |
| Manual (Claude, real keyboard on the build) | Title shows New / Continue / Quit with Continue focused when a save exists; Enter continues at Q13 with its act card; J opens the journal ("Residual Audio Q12" + the Script line; a markup bug was found and fixed); Escape opens pause (Resume / Evidence / Quit to Title / Quit Game) |
| Opening slice (earlier report) | Mouse / keyboard, save / resume, recovery, and 7 resolutions passed on the slice build. The same code paths are used, but **those interactive checks were not repeated on this build**. |
| Protected files | **PASS**: 3D scenes, Build Settings, ProjectSettings, story `.md`, 3D data, 3D director / builders, and 3D save hashes unchanged |

**Not run on this build:** the full mouse-only and keyboard-only playthrough (autoplay covers logic, not input); the
multi-resolution re-check; Quit to Title and Quit Game buttons; start-over confirmation; launching the legacy 3D prototype.

## Unity-generated changes (explained, not reverted)

- `Assets/Settings/PC_RPAsset.asset`, `UniversalRenderPipelineGlobalSettings.asset`,
  `ProjectSettings/GraphicsSettings.asset`: rewritten by Unity during player builds (URP shader-variant
  prefiltering and runtime-settings lists). Harmless; the originals are in `Backups/*_pre2D.zip`.
- `Assets/_Recovery/0.unity`: Unity's auto-recovery copy of the Prologue scene, written by the Editor at 12:18,
  before the 2D work. It can be deleted once you confirm your saved `Prologue.unity` is the version you want.

## Decisions needed from you (the game keeps working while these are pending)

| # | Scene / asset | Missing | Why it matters | What to send |
|---|---|---|---|---|
| D1 | Endings (E1–E8) | Eligibility rules | Players currently always reach "OUTCOME PENDING." | For each ending: required Q20 answer(s) and any earlier answers / flags / evidence, the order, and the fallback. Entered directly into `EndingRules2D.json`. |
| D2 | Q12 (Q12-1–6) | What B (metadata), C (preserve) and D (skip) do; which answers keep the recording | Only A currently listens / keeps it | Which letters listen, which retain; any text for B / D |
| D3 | Fonts (ST-03) | Sans and mono font names / files | LiberationSans is used everywhere | Font names + weights (or TTF / OTF). Set `style.font` in `Scenes2D.json` |
| D4 | p2, p3, p5, p18, p20 (ST-02) | Panels the PDF render dropped | Valuation, Q1, Q3, Q16, and Q18 panel geometry is reconstructed from text positions | 1920×1080 PNG of each frame, full and UI-only (transparent) |
| D5 | Q5 summary (N2), Q11 timeline / Q14 history / Q19 dates (N3) | Colleague name, dates, timestamps | The game shows Figma's own placeholders "[NOT PROVIDED]", "[DATE]", "[TIMESTAMP 01]" | The values, or permission to drop those fields |
| D6 | Q3 second figure (N1) | The flickering value | Shown as "--- CREDITS" (unreadable) | A value, or keep as is |
| D7 | Q6 (V5), Q8 (R5) | Figma p8 (visible face) and p12 (apartment) | Not used: Q6 uses the waveform view, Q8 the room | Approve using p8 / p12, or keep |
| D8 | Q2 labels (V1), Q18 sequence rows (V6), Q14 duplicate row (V4), Q10 icon (V2) | Figma-only content | Omitted: tiles show the Script's choice text; Q18 rows and the duplicate row are left out; Q10 shows the archive without the icon | Approve the omissions or supply wording |
| D9 | Q15 rain (BG-21), city view Q19 / Q20 / Ending 5 (BG-22 / 23) | Artwork | Q15 uses a programmatic dim only; the city lines play over the room | 1920×1080 PNGs (no UI) |
| D10 | 0.1 / 0.2 hotspots (HS-02–04), Q17 report form (N6), hover-state sheet (UI-21), audio (ST-08) | Text, design, sound | Hotspots disabled; report shown as a blank form; interim cyan outline; no audio | As listed in `2D_Asset_Requirements.md` |
| D11 | Build Settings | Switching the editor Build Settings to the 2D scene | The player is built with an explicit 2D scene list, so settings stay untouched | Approval to change `EditorBuildSettings.asset` |

## Final validation with approved rules (2026-10-23, build `07_build.log`)

- Rules: live `EndingRules2D.json` is byte-identical to the approved `EndingRules2D.final-draft.json` (`approved: true`).
- 18 automated paths, rules-driven (no `-ending`): **18 / 18 PASS**, each with an exact trace and 0 runtime errors.
  16 ran on build 06; p16 and p18 timed out there and were rerun on build 07 (see below).
- Real input, endings via title → Continue (save at Q20 holding that path's earlier answers) → Q20 → live rules:
  Endings 1, 3, 5 by keyboard; 2, 4, 6 by mouse. **6 / 6 reached the expected ending**, traces exact.
- Q12 real input: B by keyboard and by mouse (highlight → empty click ignored → recorder hover outline → click →
  listening sequence; heard and kept); C (kept, not heard; journal shows "Residual Audio — Q12" only); D covered by
  automated paths (neither). Pause → Quit Game → relaunch → Continue keeps Q12 evidence; Quit to Title works.
- Pacing holds (timing only): Ending 1 valuation `wait:2.5`, Ending 3 after "They step through it." `wait:2`,
  Ending 4 caption `wait:2.5`. Present in the traces of paths p08, p10 and p14. The real-input ending runs were done
  before these holds were added.
- p16 / p18 timeout cause: the player pauses when its window loses focus (`runInBackground: 0`, unchanged).
  `-autoplay` now sets run-in-background for unattended QA only. Both reruns PASS.
- Resolution: 1600×686 (21:9) and 1024×768 (4:3) runs from Q15 to an ending PASS; screenshots show correct letterbox
  and pillarbox and aligned panels.
- Integrity: Script fidelity PASS (407 entries, Q12 branches included); all protected hashes unchanged; 3D save
  unchanged; Q4 single play / no replay, Q13 mark only at its beat, Q19 Grid opens without input.
- Known presentation issue: Figma placeholders (`[NOT PROVIDED]`, `[DATE]`, `RECORD / [ID]`,
  `[TIMESTAMP 01–03]`) are still shown verbatim; a proposal is in `Design/PlaceholderTreatmentProposal.md`.

## QA hooks (command line, documented)

`-autoplay` (advance automatically), `-default X`, `-answers q1=A,…`, `-resume`, `-ending endingN` (play a
specific ending for testing only; not reachable by players), `-shots <dir>` (screenshots). Trace lines
`[Slice2D] …` in Player.log are checked by `Documentation/QA/tools/check_trace.py`.
