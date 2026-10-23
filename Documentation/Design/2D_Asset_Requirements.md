# The Last Assessment — 2D Asset Requirements

Status: **checklist for the user** · Written 2026-10-23 · No artwork designed, nothing committed.

Sources:
- **Script:** `The_Last_Assessment_Complete_Story_Script.md`, the narrative authority. Line numbers refer to it.
- **PDF:** `Downloads/Untitled (1).pdf`, 22 frames at 1920×1080, cited as "p1"–"p22".
- **Bible:** `Downloads/Untitled.pdf`, the scene design bible.
- **Plans:** `VerticalSlice2D_Plan.md`, `Full2D_Architecture.md`, `OpenNarrativeDecisions.md` (E, Q12,
  R items), and the V and N items in `Full2D_Architecture.md` §5.

Priority:
- **P0:** validate or finish the opening slice (0.1 → Q1).
- **P1:** needed before the next scenes (Act I–II, Q2–Q8).
- **P2:** later acts, evidence, and endings.
- **Opt:** polish.

Status:
- **Available:** already in the Unity project.
- **Extractable:** a bitmap inside the PDF; Claude can pull it losslessly.
- **Missing:** not in any export.
- **Awaiting approval:** blocked on a decision.

### What the PDF actually contains

These facts were checked by extracting the PDF's objects.

- Every frame's background is a **1376×768 bitmap**, which Figma scales to 1920×1080.
  **18 backgrounds are unique.** p1 = p20 = p22 (the same room bitmap); p9 = p17 (the same
  corridor).
- Separate bitmap layers exist for the four Q2 images (p4, 1024² each), the **Q17 door-with-figure**
  (p17, 768×1376, placed at x883–1037, y423–699), and **panel drop shadows** (p2, p4, p5, p13, p18).
  The "grain overlay" noted earlier on p2 is actually a drop shadow; the panel that casts it is
  vector and didn't render.
- All text is flattened into a Type 3 font ("FigmaGlyphs"), so font names are lost. Text strings,
  positions, and sizes can be read.
- The Windows PDF renderer drops some vector panels: p2, p5, p18, and p20 render blank, and p3
  loses its panels. These need PNG exports.

---

## Opening slice (0.1 → 0.2 → Q1): what validation needs

### Essential: final sign-off on the slice can't happen without these

| # | Item | Why | Status |
|---|---|---|---|
| S1 | Font names / weights (ST-03) | Text currently uses LiberationSans | Missing |
| S2 | p3 (Q1) PNG: full frame + UI-only (ST-02) | The Q1 question box, options panel and dialogue panel geometry are guessed | Missing |
| S3 | p2 (0.2) PNG: full frame + UI-only (ST-02) | The valuation panel was lost in the PDF; only its shadow survives | Missing |
| S4 | Decision on the 0.1 / 0.2 hotspots HS-02, HS-03, HS-04: text, or drop | They are built but disabled | Awaiting approval |

### Not needed for the slice to work (polish)

| # | Item | Status |
|---|---|---|
| S5 | Palette hex and opacity from Figma styles (ST-04); sampled values are in use | Optional confirmation |
| S6 | Interaction-state sheet (UI-21); interim = Bible cyan border | Optional |
| S7 | Continue-indicator style (UI-07); interim = pulsing cyan dash | Optional |
| S8 | Higher-resolution backgrounds (ST-01); 1376×768 sources are in use | Optional |
| S9 | Panel drop shadows (FX-19); interim = none | Optional |

Not needed for the slice: title / menu, inspection popup, journal, audio. The slice starts
directly in 0.1 and ends after Q1.

## Full-game gaps being tracked

These have no Figma design and are required by the Script or the Redesign: Q15 dimmed room with
rain (BG-21), Q19 / Q20 city view (BG-22), Ending 5 city change (BG-23), Q17 discrepancy report
(UI-22), title / menu (UI-17), start-over confirmation (UI-18), inspection popup (UI-15), Q2
enlarged / selected view (UI-10), evidence journal (UI-16), ending screens (UI-20), interaction
states (UI-21).

## 1. Story scene backgrounds

"Reuse" means the same bitmap serves several story beats.

| ID | Asset name | Story use (Script) | Figma | Status | Pri | Format |
|---|---|---|---|---|---|---|
| BG-01 | `BG_Room_Main` | Prologue 0.1; Q16 (p20); Q20 (p22); default chamber for Q4–Q6, Q8, Q10–Q14 between memories | p1 = p20 = p22 | **Available** (imported as `BG_0-1_AssessmentRoom`) | P0 | PNG 1366×768, have |
| BG-02 | `BG_Tablet_Closeup` | Prologue 0.2 valuation | p2 | **Available** | P0 | have |
| BG-03 | `BG_Room_Q1Angle` | Q1 | p3 | **Available** | P0 | have |
| BG-04 | `BG_Room_Wide` | Q2 (four images) | p4 | Extractable | P1 | PNG |
| BG-05 | `BG_Tablet_Macro` | Q3 valuation | p5 | Extractable | P1 | PNG |
| BG-06 | `BG_Tablet_Waveform` | Q4 (waveform); Q6 voice returns (Script line 241 is audio only; reuse proposed) | p6 | Extractable (waveform is baked into the bitmap) | P1 | PNG |
| BG-07 | `BG_Office_Workplace` | Q5 employment summary | p7 | Extractable | P1 | PNG |
| BG-08 | `BG_Office_Person` | Q6 | p8 | Extractable · **V5 conflict** (visible face; the Script shows no one) | P1 | PNG, awaiting approval |
| BG-09 | `BG_Office_EmptyDesk` | Q7 reconstructed office (Script 275); **Ending 2** "empty desk … one last time" (1048) | p11 | Extractable | P1 | PNG |
| BG-10 | `BG_Apartment_Rain` | Q8 | p12 | Extractable · **R5 conflict** (Q8 has no scene in the Script) | P1 | PNG, awaiting approval |
| BG-11 | `BG_Corridor_Workplace` | Q9 corridor and blank wall; Q17 replay; **Ending 3** route | p9 = p17 | Extractable | P2 | PNG |
| BG-12 | `BG_Archive_Restricted` | Q10 | p10 | Extractable · **V2 conflict** (icon; PARTIAL notice absent) | P2 | PNG |
| BG-13 | `BG_Monitor_Timeline` | Q11 timeline; Q14 "looks back at the timeline" | p13 | Extractable | P2 | PNG |
| BG-14 | `BG_Recorder` | Q12 | p14 | Extractable | P2 | PNG |
| BG-15 | `BG_Desk_Papers` | Q13 authorisation mark | p15 | Extractable · V3 check | P2 | PNG |
| BG-16 | `BG_Office_CityView` | Q14 performance history | p16 | Extractable · **V4 conflict** (duplicate timestamp, in the vector overlay, not the bitmap) | P2 | PNG |
| BG-17 | `BG_Corridor_RedLight` | Q15 memory: corridor, rain, red light (Script 614) | p19 | Extractable | P2 | PNG |
| BG-18 | `BG_Room_Bright` | Q18 two documents | p18 | Extractable | P2 | PNG |
| BG-19 | `BG_Archive_Grid` | Q19 Continuity Grid | p21 | Extractable · **V7 conflict** (physical hall vs on-screen window) | P2 | PNG, awaiting approval |
| BG-20 | `BG_Corridor_DoorLayer` | Q17 door appears / vanishes (Script 728–732); Ending 3 "door appears again, this time without flickering" (1070) | p17 layer | Extractable (separate 768×1376 layer) | P2 | PNG with alpha |

### Missing scene variations the Script requires

| ID | Variation | Script reference | Status | Pri | Notes |
|---|---|---|---|---|---|
| BG-21 | Chamber with **lights dimmed, rain on the glass wall** | Q15 (603), "looks at the rain on the window" (652) | Missing (**V9**) | P2 | Could be done in code (darken BG-01 plus a rain overlay) only if you accept that; otherwise draw it |
| BG-22 | Chamber with a **city beyond the glass** | Q19 (947), Q20 (983), Ending 5 (1122) | Missing (**V8**); the room frames show a silhouette, not a city | P2 | Decision first: redraw the window, or reinterpret the Script lines visually |
| BG-23 | **Ending 5 city changing**: public displays show errors, conflicting timelines | Ending 5 (1122) | Missing | P2 | Depends on BG-22 |
| BG-24 | Q9 blank wall close-up (Redesign 3.1) | Not in the Script (R6) | Missing; probably **not needed** (Script: "approaches the blank wall") | Opt | Only if R6 is approved |
| BG-25 | **Ending 1** room returning to the approved state, then a fade to white | 1012, 1028 | Can reuse BG-01; white fade done in code | P2 | No new art unless you want a distinct state |
| BG-26 | **Ending 4** "empty chair opposite the desk" | 1094 | Can reuse BG-01; confirm the chair reads as "opposite" | P2 | Decision |
| BG-27 | **Ending 6** Grid, then dark screen with no score | 1138–1166 | Reuse BG-19 / BG-01, plus code | P2 | — |
| BG-28 | Ending 3 "display goes dark" after the door | 1074 | Reuse BG-11 + BG-20, plus code | P2 | — |

---

## 2. UI screens and panels

| ID | Screen / panel | Figma | Reuse / modify / new | Status | Pri | Export |
|---|---|---|---|---|---|---|
| UI-01 | HUD status panel (SUBJECT A-4172 · ETERNA / CONTINUITY · status) | p1, p3 | Reuse (two variants) | Available (rebuilt in Unity) | P0 | PNG of p3 with UI to confirm sizes |
| UI-02 | Dialogue panel, room style (speaker plus line) | p1 | Reuse | Available | P0 | — |
| UI-03 | Dialogue panel, question style ("ASSESSOR / DIALOGUE") | p3 | Reuse | Rebuilt from text positions; **panel geometry guessed** | P0 | **PNG of p3 with UI layers** |
| UI-04 | Question box ("INTAKE / QUESTION 01" + question) | p3 | Reuse | Geometry guessed | P0 | PNG of p3 |
| UI-05 | Response options A–D + "SELECT OR SPEAK RESPONSE" | p3 | Reuse | Geometry guessed | P0 | PNG of p3 |
| UI-06 | Valuation display (PRELIMINARY VALUATION / 143 CREDITS) and its panel | p2 | Reuse | **Panel missing** in the render (only its shadow survives) | P0 | **PNG of p2 with UI layers** |
| UI-07 | Continue indicator | Bible (▼ in the reference; "—" in the specimen) | Modify (choose one) | Using a pulsing cyan dash | P0 | Decision or a small PNG |
| UI-08 | Final-question layout (four options) | p22 | Reuse | Extractable text / positions | P2 | PNG of p22 |
| UI-09 | Q2 image grid + labels | p4 | Reuse; labels **V1** | Extractable | P1 | PNG of p4 |
| UI-10 | Q2 **enlarged image view** and **cyan selected state** (Redesign 1.2) | — | **New** | Missing | P1 | 1920×1080 frame |
| UI-11 | Waveform player (AUDIO / SIGNAL HELD) | p6 | Reuse | Extractable | P1 | — |
| UI-12 | Document panel styles (summary, timeline, history, comparison, denial, Grid) | p7, p13, p16, p18, p20, p21 | Reuse | p7, p13, p16, p21 render; **p18 and p20 need PNGs** | P1–P2 | PNGs of p18 and p20 |
| UI-13 | Memory caption tag ("Q06 / RECOVERED MEMORY", "Q15 / MEMORY RECONSTRUCTION") | p8, p19 | Reuse | Extractable | P1 | — |
| UI-14 | Notice / flash bar (REFERENCE MEMORY: ACCESS RESTRICTED, ARCHIVE REFERENCE: PARTIAL) | p17 | Reuse | Extractable | P2 | — |
| UI-15 | **Inspection popup** (text or close-up image) | — | **New** | Missing | P1 | 1920×1080 frame |
| UI-16 | **Evidence journal**: list, detail, "found in" scene, compare view | — | **New** | Missing | P2 | 1920×1080 frames (list, detail, compare) |
| UI-17 | **Title screen / main menu** (New, Continue, Quit) | — | **New** | Missing. The legacy 3D title's text has open issue D5 (reveals the auditor role early). | P1 | 1920×1080 |
| UI-18 | **Save / continue** confirmation ("start over?") | — | **New** (the legacy 3D version exists, unstyled for 2D) | Missing | P1 | 1920×1080 |
| UI-19 | **Pause / settings** (resume, settings, quit to title) | — | **New** | Missing | Opt / P2 | 1920×1080 |
| UI-20 | **Ending screens**: closing text (ASSESSMENT COMPLETE / OUTCOME PENDING, final lines) | — | **New** | Missing | P2 | 1920×1080 |
| UI-21 | **Interaction states**: hover, selected, pressed, disabled, keyboard focus, for options, menu buttons, and hotspots | — | **New** | Missing. Interim: the Bible's cyan border on the selected option. | P1 | One sheet of all states |
| UI-22 | Q17 **discrepancy report form** ("begins filling in the report") | — | **New** | Missing · **N6** (wording) | P2 | 1920×1080 |
| UI-23 | End-of-session / quit-to-title confirmation | — | New | Missing | Opt | — |

---

## 3. Interactive scene elements

Kind: **Click** (player clicks), **Auto** (triggered by the Script), **Deco** (art only).
"Text" says whether the Script supplies the interaction text.

| ID | Object | Scene | Kind | Visual need | Text | Pri |
|---|---|---|---|---|---|---|
| HS-01 | Tablet | 0.1 | Click → starts the prologue (Redesign) | Hover outline | None needed | P0 (done) |
| HS-02 | Observation window / silhouette | 0.1 | Click (disabled) | Hover outline; inspect popup | **Awaiting approval** | P0 decision |
| HS-03 | Desk / workspace | 0.1 | Click (disabled) | Hover outline; inspect popup | **Awaiting approval** | P0 decision |
| HS-04 | 143 CREDITS value | 0.2 | Click (disabled) | — | **Awaiting approval** (or drop) | P0 decision |
| HS-05 | Tablet | Q1 | Click → shows the question (Redesign) | Hover outline | None needed | P0 (done) |
| HS-06 | Four memory images | Q2 | Click → enlarge (Redesign); the answer is the A–D choice | Enlarged view; cyan selected state; the badge image "disappears" (Script 103) | Script: none for the enlargement | P1 |
| HS-07 | Valuation number | Q3 | Auto: shrinks to the corner, a second figure flickers (Script 136) | Corner state; flicker | **N1** (second figure's value) | P1 |
| HS-08 | Waveform / Play | Q4 | Redesign: click Play. Script: plays once; **no replay** (173) | Cut-off state | — | P1 |
| HS-09 | Employment summary | Q5 | Auto: appears, then closes (Script 216–226) | Panel with the colleague's name | **N2** (name) | P1 |
| HS-10 | Workstation, inactive monitor | Q5 | Redesign: click | — | Awaiting approval | P1 |
| HS-11 | Other person's silhouette | Q6 | Redesign: click to continue | — | **V5** / awaiting approval | P1 |
| HS-12 | Cleared desk, rectangular mark | Q7 | Script: "The protagonist examines the desk" (284) | — | Desk examination is covered by Script dialogue; the extra "scratch" is not (R4) | P1 |
| HS-13 | Closed book, empty chair, rain window | Q8 | Redesign only | — | **R5**, awaiting approval | P1 |
| HS-14 | Blank wall at the corridor's end | Q9 | Script: "approaches the blank wall" (368) | — | Script dialogue covers it; "faint marks" is not in the Script (R6) | P2 |
| HS-15 | RESTRICTED indicator / authorisation symbol | Q10 | Redesign: click | — | **V2 / R7**, awaiting approval; must not reveal the mark | P2 |
| HS-16 | ARCHIVE REFERENCE: PARTIAL notice | Q10 | Auto (Script 411) | Notice bar | Script | P2 |
| HS-17 | Timeline entries | Q11 | Redesign: click to zoom | Zoomed view | **N3** (dates) | P2 |
| HS-18 | Recorder / audio file | Q12 | Choice A–D (Script) | Playback state | **Q12-1 to Q12-6** | P2 |
| HS-19 | Authorisation mark on screen | Q13 | Auto (Script 554) | Mark graphic | Script; **no comparison** (R8) | P2 |
| HS-20 | Performance evaluations list | Q14 | Redesign: click through | — | **V4 / R9** | P2 |
| HS-21 | Access panel, distant figure | Q15 | Redesign: click | Memory plays (Auto in Script) | Awaiting approval | P2 |
| HS-22 | Restricted-document request | Q16 | Redesign: click the tablet; Script: the Assessor requests | Denial panel | Script text | P2 |
| HS-23 | Door / figure glitch | Q17 | **Auto** (Script) | Door layer flash, freeze, vanish | Script | P2 |
| HS-24 | Two record panels | Q18 | Redesign: click to compare; Script: "moves the documents closer" | Fields appear; flicker; documents vanish | Script for fields; **V6** for labels | P2 |
| HS-25 | Continuity Grid window | Q19 | **Auto**: opens unprompted (Script 853) | Window opening; pages scroll | Script; **V7** | P2 |
| HS-26 | Grid pages / records | Q19 | Script: "The protagonist scrolls" (867, 889) | Page states | Script text; **N3, N4** | P2 |
| HS-27 | Door (escape) | Ending 3 | Redesign: click the open doorway | Door without flicker | Script | P2 |
| HS-28 | Opposite workstation / case file | Ending 4 | Redesign: click | "SUBJECT: PENDING CONTINUITY REVIEW." | Script (R15: Redesign wording rejected) | P2 |
| HS-29 | Released-records displays | Ending 5 | Redesign: clickable | — | Awaiting approval (no text in the Script) | P2 |
| HS-30 | Lamp, camera, chair (0.1) | 0.1 | **Deco** | — | — | — |

---

## 4. Documents and evidence artwork

| ID | Document | Scene | Script supplies | Unresolved | Figma | Pri |
|---|---|---|---|---|---|---|
| DOC-01 | Preliminary valuation | Prologue, Q3 | "PRELIMINARY VALUATION: 143 CREDITS", "143 CREDITS" | Q3 second figure (**N1**) | p2, p5 | P0 / P1 |
| DOC-02 | Q2 memory images ×4 | Q2 | Descriptions (Script 76) | Labels (**V1**) | p4 layers (extractable) | P1 |
| DOC-03 | Employment summary | Q5 | That it exists; a name beside a project entry | Name (**N2**), project text; Figma shows "[NOT PROVIDED]" | p7 | P1 |
| DOC-04 | Archive notice | Q10 | "ARCHIVE REFERENCE: PARTIAL." | Placement / style | — | P2 |
| DOC-05 | Employment timeline | Q11 (and Q14) | Gap "Three years and four months"; position and salary unchanged; dense, then sparse entries | All dates and IDs (**N3**); Figma shows "[DATE]", "RECORD / [ID]" | p13 | P2 |
| DOC-06 | Residual audio file | Q12 | VOICE line; "metadata is incomplete" | Q12 rules; the timestamp "--:4-:--" is Figma-only | p14 | P2 |
| DOC-07 | Authorisation mark | Q13, Q19 | That it is the same mark, recognised at Q19 | **Mark design** (no standalone asset); V3 | p15 | P2 |
| DOC-08 | Performance history | Q14 | "previous performance evaluations" | Timestamps (**N3**); duplicate row (**V4**) | p16 | P2 |
| DOC-09 | Access denial | Q16 | "The request has been denied" / "Eterna's access-control system" | — | p20 (PNG needed) | P2 |
| DOC-10 | Discrepancy report | Q17 | "begins filling in the report" | Form wording (**N6**) | — | P2 |
| DOC-11 | Two final-night records | Q18 | Official: leaves before the incident. Fragment: credentials used after. Dates and location match. Fields: CONTINUITY STATUS: UNAUTHORISED, SUBJECT INSTANCE: ACTIVE | Times / dates (**N3**); Figma labels (**V6**) | p18 (PNG needed) | P2 |
| DOC-12 | Continuity Grid pages | Q19 | "CONTINUITY GRID — INTERNAL RECORD"; diagrams; numbered subjects; "143 CREDITS."; final-night date; "CONTINUITY GRID: SUBJECT RECONSTRUCTION AND MEMORY-STABILITY FRAMEWORK."; name and employment number; diagram of reconstruction; "ASSESSMENT CYCLE: ACTIVE."; previous evaluation timestamps matching the gaps | Name, employment number (**N4**); dates (**N3**); diagram art; Figma's A-4173 / A-4174 rows | p21 | P2 |
| DOC-13 | Q20 Grid beside the window | Q20 | "remains open beside the assessment window" | — | p22 | P2 |
| DOC-14 | Ending 4 case file | Ending 4 | "SUBJECT: PENDING CONTINUITY REVIEW." | Layout | — | P2 |
| DOC-15 | Ending 5 released records | Ending 5 | "conflicting records", "error messages, conflicting timelines" | Content | — | P2 |
| DOC-16 | Ending 6 final report without the valuation | Ending 6 | "removes the valuation from the final report. The number disappears" | Report layout | — | P2 |
| EV-01 | **Evidence: the Q12 recording** | Q12 | The only item the Script explicitly adds to evidence (521) | Which choices add it (Q12-5) | — | P2 |
| EV-xx | Any other journal entries (flags such as the corridor door, timeline gap, PARTIAL notice) | — | Not named as evidence by the Script | **N5**: you approve the list, titles, and descriptions | — | P2 |

---

## 5. Visual effects and transitions

**Code** means Claude implements it programmatically from existing art. **Art** means you draw or
export something.

| ID | Effect | Where | Code / Art | Pri |
|---|---|---|---|---|
| FX-01 | Fade from black at start; cross-fade between scenes | All | Code (done for the slice) | P0 |
| FX-02 | Continue-indicator pulse; hotspot hover outline | All | Code (done); Art optional (UI-21) | P0 |
| FX-03 | Q2 images appear; badge image disappears | Q2 | Code | P1 |
| FX-04 | Q3 number shrinks to the corner; a second figure flickers and vanishes | Q3 | Code; Art only if you want a designed corner state | P1 |
| FX-05 | Q4 waveform plays and cuts off; Q6 faint replay | Q4, Q6 | Code (waveform animation) + **audio** | P1 |
| FX-06 | Summary / document open and close | Q5, Q11, Q14, Q16 | Code | P1 |
| FX-07 | Memory transition into a reconstruction and back (fade or blur) | Q7, Q9, Q15, Q17 | Code; Art optional (a "reconstruction" texture) | P1 |
| FX-08 | Q15 lights dim, rain on the glass | Q15 | Code (dim) + **Art** (rain overlay or BG-21), per V9 | P2 |
| FX-09 | Q15 memory "sharp sound", memory ends | Q15 | Audio + code cut | P2 |
| FX-10 | Q16 "The screen dims" | Q16 | Code | P2 |
| FX-11 | Q17 door appears for a fraction of a second, image freezes, door vanishes; message flashes | Q17 | Code using BG-20 (extractable) | P2 |
| FX-12 | Q18 fields appear; screen flickers; documents disappear | Q18 | Code | P2 |
| FX-13 | Q19 window opens **unprompted**; pages load and scroll | Q19 | Code; Art = DOC-12 pages | P2 |
| FX-14 | Q20 "screen goes dark", ASSESSMENT COMPLETE / OUTCOME PENDING | Q20 | Code | P2 |
| FX-15 | Ending 1 fade to white | E1 | Code | P2 |
| FX-16 | Ending 3 door without flicker; display goes dark | E3 | Code (BG-20) | P2 |
| FX-17 | Ending 5 city displays change | E5 | **Art** (BG-23) + code | P2 |
| FX-18 | Ending 6 valuation disappears; final line | E6 | Code | P2 |
| FX-19 | Panel drop shadows | All panels | Code (soft shadow) or the extractable shadow bitmaps | Opt |
| FX-20 | Atmospheric haze (Bible: "subtle atmospheric haze"; no film grain appears in any frame) | All | Code or a small tileable PNG | Opt |

---

## 6. Style guide and source files

| ID | Spec | Current value | Status | Pri |
|---|---|---|---|---|
| ST-01 | Background size | 16:9; source bitmaps are 1376×768 (soft at 1080p). Preferred: **1920×1080 or 3840×2160 PNG, no UI**. | Extractable now; higher-res Missing | P1 (Opt if 1376 is acceptable) |
| ST-02 | Frame PNGs **with UI** for pages the PDF can't render | p2, p3, p5, p18, p20 | Missing | **P0** (p2, p3), P1 (p5), P2 (p18, p20) |
| ST-03 | Fonts | Sans looks like Inter; mono looks like IBM Plex Mono or JetBrains Mono. **Unconfirmed.** | Missing | **P0** |
| ST-04 | Palette | Cyan `#6EB8D6`, white `#EDF2F4`, grey `#A5B3BD`, border `#344B59`, panel `#060D10` ≈ 90 %. Q1 HUD: `#799AAC`, `#A5C3CD`, `#1D495E`, `#1D3A49`. Sampled from renders. | Awaiting confirmation (Figma hex and opacity) | P1 |
| ST-05 | Type sizes (1080p) | HUD 16 / 13 / 12; speaker 16 / 10; dialogue 26 / 22; options 23 / 20; question 20; labels 10–11; valuation 94 | From the PDF | Available |
| ST-06 | Margins / panels | HUD at 64, 48; dialogue panel 0, 793, 1267×287 (p1); options panel ≈ 1334, 656, 522 wide (p3, guessed) | Partly guessed | P0 (confirm via the p3 PNG) |
| ST-07 | Icons | Authorisation mark (DOC-07), RESTRICTED icon (V2), chevrons, ▼ | Missing as standalone SVG / PNG | P2 |
| ST-08 | Audio | Hum; Assessor? (voice acting not specified); VOICE "You promised—" (Q4), Q6 line, Q12 recording, Q15 voices; static; metallic sound (Q12); alarm / sharp sound (Q15); UI clicks | Missing | P1 (Q4) / P2 |
| ST-09 | App icon / splash | — | Missing | Opt |

---

## 7. Checklists by who acts

### A. Assets Claude can extract from the existing PDFs (no action from you)

- Backgrounds BG-04 to BG-19 (16 bitmaps).
- BG-20, the Q17 door layer.
- DOC-02, the four Q2 images.
- Drop-shadow bitmaps.
- All text strings, positions, and sizes for all 22 frames.

Status: **Extractable**, as PNG. They will be extracted only when their scene group starts.

### B. Designs you need to create or export from Figma

| Order | Item | Format |
|---|---|---|
| 1 | Font names and weights (or files) | text / TTF / OTF |
| 2 | **p3 (Q1)** and **p2 (0.2)** as PNG, *full frame with UI*, plus *UI-only* (background hidden) | 1920×1080 PNG, transparent for UI-only |
| 3 | Interaction-state sheet (UI-21) | 1920×1080 PNG |
| 4 | Palette hex and opacity from Figma styles (ST-04) | text |
| 5 | Title / main menu (UI-17) and the start-over confirmation (UI-18) | 1920×1080 PNG + UI-only |
| 6 | Inspection popup (UI-15); Q2 enlarged image and selected state (UI-10) | 1920×1080 |
| 7 | **p5** PNG with UI (Q3) | 1920×1080 |
| 8 | **p18, p20** PNGs with UI | 1920×1080 |
| 9 | Evidence journal: list, detail, compare (UI-16) | 1920×1080 ×3 |
| 10 | Authorisation mark (DOC-07) and the RESTRICTED icon if kept | SVG or 512² PNG |
| 11 | Scene variations after decisions: BG-21 (Q15 dim / rain), BG-22 (city view), BG-23 (Ending 5) | 1920×1080 PNG, no UI |
| 12 | Ending screens (UI-20) and any ending-specific states | 1920×1080 |
| 13 | Q17 report form (UI-22); pause / settings (UI-19) | 1920×1080 |
| 14 | Higher-resolution background sources, if they exist (ST-01) | 1920×1080 or 4K PNG |
| 15 | Audio (ST-08) | WAV 48 kHz |

### C. Decisions you need to make before designs can be final

| Group | Items |
|---|---|
| P0 | HS-02 / HS-03 / HS-04 text, or drop them; UI-07 indicator style |
| P1 | **V1** (Q2 labels); **V5** (Q6 face); **R5** (Q8 scene); **N1** (Q3 second figure); **N2** (Q5 colleague name); Redesign hotspots HS-10, HS-11, HS-13 |
| P2 | **V2, V3, V4, V6, V7, V8, V9**; **N3** (dates / timestamps); **N4** (name and employment number); **N5** (evidence list); **N6** (report form); **Q12-1 to Q12-6**; **E1–E8** (ending rules); whether an ending title card is shown (the Script only says endings stay hidden until reached); switching Build Settings to 2D only |

### D. Items Claude can implement in Unity without more artwork

- Run the slice play tests T3–T11.
- Done: the docs now describe the p2 bitmap as a panel drop shadow. One editor-only code comment (`Assessment2DBuilder.cs`, line 112) still says "grain overlay"; it is left unchanged and will be corrected at the next builder edit.
- Data-driven `Scenes2D.json` builder.
- `CueRunner` effects FX-01 to FX-07 and FX-10 to FX-16, FX-18 (code-only items).
- `DocumentPanel` built from the existing frames' styles.
- Copy Q9–Q20 and the endings into the JSON word for word, with an automated fidelity check.
- Evidence store logic (no journal UI); keyboard navigation; letterboxing; the save / resume /
  recovery logic.
- Placeholder-free interim states using the Bible's cyan-border treatment.

---

## Your prioritised to-do list

1. **Send the font names** (sans and mono, with the weights used).
2. **Export p3 (Q1) and p2 (0.2)** at 1920×1080 PNG, twice each: the full frame, and the UI
   layers only on a transparent background. This fixes the guessed panel geometry and the missing
   valuation panel.
3. **Copy the palette** from Figma's colour styles, as hex plus opacity for the panels.
4. **Decide the P0 hotspots** (silhouette, desk, value): give text, or say "drop them".
5. **Design an interaction-state sheet**: option, menu button, and hotspot in idle, hover,
   selected, pressed, disabled, and focused states, on one 1920×1080 frame.
6. **Design the title / main menu** and the start-over confirmation (1920×1080, plus UI-only).
7. **Answer the Act I–II decisions:** V1, V5, R5, N1, N2.
8. **Design the inspection popup** and the Q2 enlarged / selected view. Then export p5 (Q3) with
   UI.
9. Later: p18 and p20 PNGs; the evidence journal (three frames); the authorisation mark as SVG;
   the city-view and Q15 rain variations once V8 / V9 are decided; the ending screens once E1–E8
   are settled; audio.
