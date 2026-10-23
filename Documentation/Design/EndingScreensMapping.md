# Ending Screens: Six-Ending Mapping (implemented in code, not yet built or tested)

Source: `Untitled.fig`, decoded locally (18 ending frames, exact positions, fonts and colours). The data file is
`Assets/Narrative/Data/EndingScreens2D.json`; the runtime is `EndingScreens.cs`.

Flow: the ending's Script sequence plays unchanged → press → **01 Reveal → 02 Epilogue** (← back / NEXT →,
NEW RUN on the Epilogue, "0N / 02", 0.9 s cross-dissolve with a slow background push). The .fig's 03 Assessment
screen is **not part of the game** (user decision, 2026-10-23): removed from the scene, code, builder and data. The Script, the
ending rules, and the in-scene ending text are unchanged.

## Mapping

| # | Ending (Figma display title) | Background (.fig image) | 01 Reveal (Figma labels) | 02 Epilogue paragraph = Script description | Status / confirmed outcome (Figma) |
|---|---|---|---|---|---|
| 1 | CONTINUITY GRANTED | room variant `f52406` | "YOUR FINAL ASSESSMENT" + title; tablet panel "CONTINUITY STATUS: APPROVED / 143 CREDITS" | "The protagonist accepts the system's judgment and prioritises preserving their identity." | 143 CREDITS / CONTINUITY STATUS: APPROVED / 143 CREDITS |
| 2 | QUIET ERASURE | `ec7b5b`, veil 0.78 ("fading room") | "ETERNA / CONTINUITY ASSESSMENT", QUIET / ERASURE, "ENDING 02"; foot "ASSESSMENT CLOSED" | "The protagonist prioritises safety or accepts the official explanation despite unresolved inconsistencies." | QUIET ERASURE / ASSESSMENT CLOSED / QUIET ERASURE |
| 3 | ESCAPE PROTOCOL | open doorway `8187c0` | "ENDING 03", title, "CONTINUITY / UNRESOLVED" | "The protagonist finds a way to leave the assessment process without fully resolving the mystery." | ESCAPE PROTOCOL / CONTINUITY / UNRESOLVED / ESCAPE PROTOCOL ¹ |
| 4 | NEW ASSESSOR | reverse desk view `d0a919` | "ETERNA / CONTINUITY REVIEW", title, "CONTINUITY REVIEW — NEW ASSIGNMENT" | "The protagonist challenges the system from within and accepts a role that grants access to its assessment process." | CONTINUITY REVIEW — NEW ASSIGNMENT / ETERNA / CONTINUITY REVIEW / … |
| 5 | ARCHIVE FALLS | archive grid `e256fa` | "CONTINUITY EVALUATION — TERMINAL OUTCOME", title, "ENDING 05 — RECORDS RELEASED / SEQUENCE CONFLICT CONFIRMED" | "The protagonist exposes enough evidence to undermine Eterna's official account." + records panel (headings only ²) | PROFILE / INCOMPLETE · STATUS / PUBLIC / RECORDS RELEASED / … |
| 6 | THE UNCOUNTED | room variant `2afce4` | "ENDING 06", title, "NOT ALL THAT EXISTS CAN BE COUNTED." (Script line) | "The protagonist rejects the idea that a person's identity can be determined by the system's records or valuation." | NOT ALL THAT EXISTS CAN BE COUNTED. / ENDING 06 / THE UNCOUNTED / … |

The Epilogue paragraph is each ending's own one-sentence description from the Script (lines 1010, 1034, 1058,
1082, 1110, 1136), copied by the generator. The Figma "Draft epilogue" text and the "EPILOGUE DRAFT · FINAL
NARRATIVE COPY PENDING" label are **not** used. The second-person rewrites in `EndingScreensProposal.md` stay
unapproved and unused.

¹ The .fig's Ending 3 confirmed line reads "ENDING 03 / ESCAPE PROTOCOL / ESCAPE PROTOCOL" (title repeated).
It is shown using the same pattern as the other endings: status / title.
² The .fig's Ending 5 panel also lists event rows ("01 / CORRIDOR · 02 / ACCESS PANEL"). These are omitted,
as with Q18 (V6): the Script gives no event order.

## Label check against the Script (no contradictions found)

| Ending | Figma label | Script | Verdict |
|---|---|---|---|
| 1 | CONTINUITY STATUS: APPROVED / 143 CREDITS | "Your continuity status has been approved." / "143 credits" | Matches |
| 2 | ASSESSMENT CLOSED | "Your assessment has been concluded." | Same meaning; kept |
| 3 | CONTINUITY / UNRESOLVED | "Your assessment remains incomplete." | Same meaning; kept |
| 4 | CONTINUITY REVIEW — NEW ASSIGNMENT | "A new assignment is available." / "continuity review" | Matches. The in-scene caption "SUBJECT: PENDING CONTINUITY REVIEW." is unchanged |
| 5 | RECORDS RELEASED · STATUS / PUBLIC · SEQUENCE CONFLICT CONFIRMED · TIMELINE / MISMATCH | "Multiple records have been released…", conflicting timelines made public | Matches |
| 5 | PROFILE / INCOMPLETE | Not stated in Ending 5; the Script says memories remain uncertain | Mild addition; kept as Figma (approved 2026-10-23) |
| 6 | NOT ALL THAT EXISTS CAN BE COUNTED. | Same | Matches |

## Assessment chart (removed with the Assessment screen; notes kept for reference)

The .fig specifies **no axes and no data** ("[ AXIS A · PENDING ]", "AXES & PLAYER POSITION AWAIT GAME DATA ·
NO RESULT PLOTTED"). Implemented treatment: the quadrant frame and centre axes exactly as designed (440×440,
#090E11, #344B59), with **no axis labels, quadrant names, plotted point or score**. "YOUR ASSESSMENT", the
outcome headline and the confirmed-outcome line are shown. To plot real data, approve two axes and which answers
move them.

## NEW RUN and saves

- After an ending, the 2D save is marked complete, as before, so Continue stays disabled. This is unchanged.
- NEW RUN deletes only the 2D save (`assessment2d_save.json`) and starts a new game directly at the prologue,
  the same as New Game on the title (no confirmation needed: the save is completed). The 3D save is never touched.
- Escape still opens the pause menu (Quit to Title / Quit Game) on the ending screens.

## Known differences from the .fig

- **Fonts:** Inter / IBM Plex Mono are not installed in the project. LiberationSans is used, including for Light
  weights. Both fonts are free (SIL Open Font License); installing them needs your OK to download.
- The six Reveal frames each have slightly different decoration (authorisation symbol, rules, nav styles). One
  centred title template is used, with per-ending labels and veil, plus the tablet variant for Ending 1.

## Where `PROFILE / INCOMPLETE` appears (Ending 5 only)

| Screen | Element | Exact text |
|---|---|---|
| 02 Epilogue | Ending status line under the paragraph | `PROFILE / INCOMPLETE · STATUS / PUBLIC` |
| 02 Epilogue | Records panel (right), third line | `PROFILE / INCOMPLETE` |

Meaning: in the .fig it labels a released record ("RECORD / SEQUENCE A / B" … "PROFILE / INCOMPLETE",
"STATUS / PUBLIC"). It echoes Q19's Script fact that "Several records are marked incomplete" and the Grid's
"TIMELINE: INCOMPLETE". It does not say whose profile it is or why it is incomplete, so it adds no new event.
It could still be read as a statement about the protagonist, which Ending 5's text does not make. **Decision
(user, 2026-10-23): keep it as designed** in both Epilogue spots.
