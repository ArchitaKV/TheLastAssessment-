# Ending Screens: Format from `Untitled.fig` + Rewritten Descriptions (proposal)

Status: **proposal, nothing implemented.** Source: `Downloads/Untitled.fig` (decoded locally; 28 frames =
22 story scenes + 6 endings). Ending dialogue and script events are unchanged. These screens come *after*
each ending's Script sequence. Written 2026-10-23.

## Format in the .fig (same for all six endings)

| Screen | Contents in the .fig |
|---|---|
| 01 Reveal | Ending background, a system status label, title treatment "ENDING 0N / TITLE" |
| 02 Epilogue | "ENDING 0N / TITLE", "02 / EPILOGUE", "FINAL ASSESSMENT / OUTCOME", outcome heading, a 2–4 line paragraph marked "EPILOGUE DRAFT — FINAL NARRATIVE COPY PENDING" |
| 03 Assessment | "YOUR ASSESSMENT", outcome headline, a four-quadrant chart ("AXES & PLAYER POSITION AWAIT GAME DATA — NO RESULT PLOTTED"), confirmed outcome line, NEW RUN button |

Fonts named in the file: **Inter** (text), **IBM Plex Mono** (labels). Palette: cyan #6FB9D7, warning red
#A44A4A, memory amber #A48C70.

## Reveal labels in the .fig vs the Script

| Ending | .fig label | Script | Note |
|---|---|---|---|
| 1 | CONTINUITY STATUS: APPROVED / 143 CREDITS | "Your continuity status has been approved." / "143 credits" | Consistent |
| 2 | ASSESSMENT CLOSED | "Your assessment has been concluded." | Paraphrase; OK? |
| 3 | CONTINUITY / UNRESOLVED | "Your assessment remains incomplete." | Paraphrase; OK? |
| 4 | CONTINUITY REVIEW — NEW ASSIGNMENT | **SUBJECT: PENDING CONTINUITY REVIEW.** | Differs (R15); proposal: use the Script line |
| 5 | RECORDS RELEASED · PROFILE / INCOMPLETE · STATUS / PUBLIC + two event sequences | "Multiple records have been released…" | Event rows are not in the Script (as V6); proposal: omit the rows |
| 6 | NOT ALL THAT EXISTS CAN BE COUNTED. | Same | Consistent |

## Epilogue descriptions: .fig drafts and proposed rewrites

Each rewrite uses only facts stated in that ending's Script text (line references in brackets). Nothing new
is added about what happens afterwards.

**Ending 1 — Continuity Granted**
- .fig draft: "Continuity approved. The assessment has concluded. Your final valuation is 143 credits."
- Proposed: "Your continuity status was approved, and your current memory profile will be preserved. The
  records you saw are not part of the approved record. Your valuation stands at 143 credits." [1014–1024]

**Ending 2 — The Quiet Erasure**
- .fig draft: "The assessment is closed. The room falls quiet. What remains is the record of your answers."
- Proposed: "Your assessment was concluded without a public record of the discrepancies. Your memories will
  be handled under the approved restoration process. You know something is being forgotten; you cannot say
  what." [1036–1052]

**Ending 3 — The Escape Protocol**
- .fig draft: "An open doorway. A step beyond the room. The assessment is behind you. What lies ahead
  remains unwritten."
- Proposed: "You left the assessment before it could close, through the door you remembered from the
  corridor. The assessment remains incomplete. Nothing confirms you escaped the system, only that it no
  longer controls what you do next." [1062–1076]

**Ending 4 — The New Assessor**
- .fig draft: "Continuity review. New assignment. The room is familiar. The view from this side of the desk
  is not."
- Proposed: "Your request was reviewed. You accepted an assignment in continuity review and discrepancy
  analysis, and now open other people's cases. The outcome of future reviews cannot be predicted."
  [1084–1104]

**Ending 5 — The Archive Falls**
- .fig draft: "The records are public. The sequences do not agree. The archive offers two accounts. Neither
  makes the contradiction disappear."
- Proposed: "Records were released outside their authorised scope. Conflicting timelines and previously
  inaccessible records are now public. No one knows how many memories will be recovered, or what will
  replace Eterna's control." [1114–1128]

**Ending 6 — The Uncounted**
- .fig draft: "Beyond a valuation, beyond a verdict, the question of what you are remains."
- Proposed: "You preserved the conflicting records instead of a single approved version, and removed the
  valuation from your final report. No score was recorded and no official outcome was granted. Your
  memories remain incomplete, and no longer counted against you." [1148–1162]

## Decisions needed

1. Approve or edit the six rewritten epilogues.
2. Reveal labels for Endings 2, 3, 4 and 5: keep the .fig wording, or use the Script wording.
3. **Assessment chart:** the .fig leaves the axes undefined. The game would need two axes built from the
   player's answers (for example "follows official records ↔ trusts own memory" and "complies ↔ challenges").
   Choose the axes and which answers move them, or drop the chart and keep only the outcome line.
4. Show the ending title ("ENDING 0N / TITLE") after the sequence. The .fig does this; earlier option U8 was
   "no". This needs your confirmation.
5. Export the six ending backgrounds from the .fig as 1920×1080 PNGs. The .fig images are AI-generated
   bitmaps; I can extract them if you prefer.
