# Open Narrative Decisions — Endings and Q12

Status: **open, awaiting the user's decisions** · Written 2026-10-23 · No code or story data changed.

Authoritative source: `Documentation/The_Last_Assessment_Complete_Story_Script.md` (the Script).
This file quotes the Script and records only what it does **not** specify. It proposes no rules,
dialogue, or lore. Line numbers refer to the Script as of this date.

## 1. Ending conditions

### What the Script establishes

- The ending depends on "accumulated choices, the evidence they discovered, their decisions about
  the records, and their approach to Eterna—not only their answer to Question 20" (line 1006).
- Q20 has four answers: A Preserve myself · B Preserve the truth · C Change the system ·
  D Refuse the assessment (lines 987–990).
- After Q20: "ASSESSMENT COMPLETE." … "OUTCOME PENDING." (lines 996–1000), then the ending plays.
- Endings stay hidden until reached; no eligibility hints and no ending labels on choices
  (Continuity Rule 10, line 1190).
- Each ending has a one-sentence summary and full dialogue (lines 1008–1166).

### Ending summaries, quoted

| # | Ending | Script summary |
|---|---|---|
| 1 | Continuity Granted | "accepts the system's judgment and prioritises preserving their identity" |
| 2 | The Quiet Erasure | "prioritises safety or accepts the official explanation despite unresolved inconsistencies" |
| 3 | The Escape Protocol | "finds a way to leave the assessment process without fully resolving the mystery"; dialogue uses "the access route they remembered from the corridor" |
| 4 | The New Assessor | "challenges the system from within and accepts a role that grants access to its assessment process"; dialogue opens "Your request has been reviewed." |
| 5 | The Archive Falls | "exposes enough evidence to undermine Eterna's official account" |
| 6 | The Uncounted (true ending) | "rejects the idea that a person's identity can be determined by the system's records or valuation"; dialogue: "chooses to preserve the conflicting records" |

### What the Script does not specify

| ID | Open question |
|---|---|
| E1 | Which Q20 answer or answers can lead to each ending. |
| E2 | Which earlier answers, evidence, or flags each ending requires, and how many count as "enough" (Ending 5). |
| E3 | Priority order when a player qualifies for several endings. |
| E4 | The fallback ending when a player qualifies for none. |
| E5 | How "approach to Eterna" (line 1006) is measured. The Script defines no trait names or scores. |
| E6 | What earns Ending 3's escape route. The corridor door (Q9) and the door glitch (Q17) are seen by every player. |
| E7 | Whether Ending 4's "request" refers to a specific earlier answer (for example Q16's request for access). Not stated. |
| E8 | Whether Ending 6 requires the Q12 recording to be kept, Q18's "preserve both" answer, or neither. Not stated. |

Other sources mention mechanics the Script lacks: hidden traits (Compliance, Empathy, Curiosity,
Defiance, Agency), a preservation protocol, a simulation vulnerability, and an Assessor
relationship. Those sources are the Story Bible PDFs and the Scene Storyline. They are **not**
used unless the user approves them.

The Point-and-Click Redesign (§4) proposes five hidden variables: `evidence`, `trust_eterna`,
`suspicion`, `resistance`, and `continuity_belief`. It labels them "implementation variables,
not new story facts". It does not say which answers change them, by how much, or which values
lead to which ending. So E1–E8 stay open.

### Needed from the user

A table with, for each ending: the allowed Q20 answer(s), the required earlier answers or
evidence, its priority, and which ending is the fallback.

## 2. Q12 — Residual Audio (Script lines 476–521)

### What the Script establishes

- Four choices: A Play the complete available fragment · B Review its metadata before
  listening · C Preserve it and continue the assessment · D Skip the fragment.
- "If the player chooses to listen", the VOICE line plays: "If you're hearing this, they've
  already found my records. I thought I had more time." Then comes a metallic sound and a
  scripted PROTAGONIST/ASSESSOR exchange.
- That exchange ends with "Keep it. I might need it later." and "The recording is added to the
  assessment's available evidence."

### What the Script does not specify

| ID | Open question |
|---|---|
| Q12-1 | Which choice(s) count as "chooses to listen". A, clearly. B says "before listening": does it play the audio after the metadata, or not? |
| Q12-2 | What plays for **B**. No metadata text is written. The follow-up only says "The available metadata is incomplete." |
| Q12-3 | What plays for **C**. It preserves the file, but is it heard? Does the "Keep it" exchange play? |
| Q12-4 | What plays for **D**. No text is written. Is the recording lost, and does a later scene refer to that? |
| Q12-5 | Which choices add the recording to evidence. The Script ties it to the post-listening exchange only. |
| Q12-6 | Whether the recording affects any ending (see E8). |

### Current implementation

`Assets/Narrative/Data/AssessmentScript.json` is authored only up to Q8; Q9–Q20 and the endings
are not in the data. The code has a `setFlags` field on choices, but no Q12 data uses it yet.
Nothing has been implemented for Q12.

## 3. Missing content: hotspot inspection text

`Documentation/The_Last_Assessment_PointAndClick_Redesign.md` (the Redesign) now explains where
the brief's scene names and hotspots come from. It lists which objects are clickable in each
scene, but **neither document contains any inspection text**. That covers what the player reads
on clicking the desk, the silhouette, the dust outline, the scratch, the closed book, the empty
chair, the rain-streaked window, the workstation, the inactive monitor, the access panel, the
distant figure, and so on. Each hotspot needs approved text, or an approved rule that it shows no
text and only changes the visual.

## 4. Redesign vs Script: points needing a decision

The Script wins on dialogue, choices, and reveal order. The Redesign describes how scenes are
played. These items are not just presentation: they add story details or contradict a Script
line. None are implemented until the user decides.

| ID | Scene | Redesign says | Script says (line) | Kind |
|---|---|---|---|---|
| R1 | Setting (all) | Assessment room with a tablet, an **observation window with a silhouette**, and a desk | "A white room… A glass wall overlooks a quiet city" (13). There is no silhouette. | Conflict, likely settled by the Figma art |
| R2 | 0.2 | "The player can acknowledge the result or question it" | The Prologue has no player choice. The protagonist's question is scripted (29). | Adds a choice, with no text |
| R3 | 1.4 / Q4 | "The player can replay it" | "The fragment is unavailable for repeated playback in this section." (173) | **Direct contradiction** |
| R4 | 2.3 / Q7 | Dust outline, plus a "scratch along the desk" | "a faint rectangular mark on its surface" (275). There is no scratch. | Adds a clue |
| R5 | 2.4 / Q8 | Closed book, empty chair, rain-streaked window | Q8 has no scene description. Rain first appears in Q15. | Adds a scene and clues |
| R6 | 3.1 / Q9 | "faint rectangular marks" on the wall; "The player records the discrepancy" | Protagonist: "Not yet. I might be wrong." The discrepancy is not recorded. | Adds a clue; contradicts the dialogue |
| R7 | 3.2 / Q10 | Authorisation symbol and a blue `RESTRICTED` indicator | Q10 shows only *ARCHIVE REFERENCE: PARTIAL.* The mark first appears in Q13. | Adds a clue earlier than the Script |
| R8 | 4.1 / Q13 | Compare the Q13 symbol with Q10's; the "matching evidence" goes in the journal | Q13: the mark "feels familiar, though they cannot remember where they saw it". Q19 is where it is recognised from old employment records. | **Recognises the mark early; could weaken the Q19 payoff** |
| R9 | 4.2 / Q14 | "The player notices a duplicated timestamp" | No timestamp in Q14. Previous evaluation timestamps are revealed in Q19. | Adds a clue; may hint at the Q19 reveal |
| R10 | 3.4 / Q12 | A recorder; clicking the waveform shows an incomplete timestamp | Metadata is "incomplete" (503). The Q12 choice outcomes are still undefined (§2). | Presentation; §2 is still open |
| R11 | 4.3 / Q15 | Memory "ends in a white flash" | "A sharp sound follows. The memory ends." | Additive; probably fine |
| R12 | 5.3 / Q19 | "Click the observation window to reveal the archive chamber" | "a window opens without being requested" (a screen window); the protagonist looks "at the city beyond the glass". | Conflict: player-triggered vs unprompted; a physical chamber vs an on-screen file |
| R13 | 5.4 / Q20 | "the valuation is gone" | Not stated. Ending 6 is where the valuation is removed. | Possible early use of the Ending 6 beat |
| R14 | Ending 2 | `ASSESSMENT CLOSED` text | That text is not in the Script. | Adds on-screen text |
| R15 | Ending 4 | `CONTINUITY REVIEW — NEW ASSIGNMENT` | `SUBJECT: PENDING CONTINUITY REVIEW.` (1102) | Different on-screen text |
| R16 | Ending 1 | `CONTINUITY STATUS: APPROVED` appears | The dialogue says "Your continuity status has been approved." The screen "fades to white". | Adds on-screen text; consistent in meaning |

Technical note: the Redesign's script name `SceneManager` clashes with Unity's
`UnityEngine.SceneManagement.SceneManager`. If the 2D direction is approved, it needs a
different name, such as `SceneFlow`.
