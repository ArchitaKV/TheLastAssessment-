# Ending Design Proposal (for approval)

Status: **proposal only, not active.** `EndingRules2D.json` stays `"approved": false`, so the game still ends at
"OUTCOME PENDING." Q12 behaviour is unchanged. Written 2026-10-23.

Sources: `The_Last_Assessment_Complete_Story_Script.md` (line numbers below), `QA/Game2D_TestReport.md`.
Each section separates **Script (established)** from **Proposal (interpretation)**.

## 1. What the Script establishes about endings

- The ending depends on "the protagonist's accumulated choices, the evidence they discovered, their decisions
  about the records, and their approach to Eterna—not only their answer to Question 20" (line 1006).
- Q20 "presents the final moral choice. The player's accumulated decisions determine which endings are
  available" (Continuity Rule 9).
- Endings "remain hidden from the player until reached. Do not display eligibility requirements or label
  choices with their associated endings" (Rule 10).
- Ending 6 is "The true ending" (line 1134).
- The Script defines **no** mapping from answers to endings, no priority, and no fallback.

What actually varies between players in the built game: the A–D answer to each of the 20 questions, and
whether the Q12 recording is kept. Every other discovery flag (corridor door, timeline gap, Grid, …) is set
for every player, because the Script plays those moments for everyone, so they cannot distinguish endings.

## 2. Endings: Script text and proposed conditions

### Ending 6 — The Uncounted (true ending)
- **Script:** "The protagonist rejects the idea that a person's identity can be determined by the system's
  records or valuation." (1136) The sequence has them "preserve the conflicting records rather than replace
  them with a single approved version" (1148).
- **Proposal:** Q20 = **C or D**, AND **Q18 = B** ("Preserve both until the differences can be explained."),
  AND the **Q12 recording kept**.
- *Why:* Q18 B is the only answer in the game that matches Ending 6's defining act word for word (preserving
  both conflicting records). C ("If the process is wrong, preserving it is not an acceptable solution") and
  D (refusing rules "I was never allowed to understand") both reject the system's authority over identity.
  Keeping the recording reflects the "evidence they discovered" clause.

### Ending 5 — The Archive Falls
- **Script:** "The protagonist exposes enough evidence to undermine Eterna's official account." (1110)
- **Proposal:** Q20 = **B** ("Preserve the truth. … the records must not be hidden."), AND the **Q12
  recording kept**.
- *Why:* B is the disclosure choice. "Enough evidence" requires that something was retained; the recording is
  the only item the Script calls evidence (521).

### Ending 4 — The New Assessor
- **Script:** "The protagonist challenges the system from within and accepts a role that grants access to its
  assessment process." (1082) It opens with "Your request has been reviewed." (1084)
- **Proposal:** Q20 = **C**, AND **Q16 = C** ("Request permission to access the restricted records.").
- *Why:* Q16 C is the only point where the player asks Eterna for access, and Ending 4's first line answers
  "your request". C ("Change the system") is the challenge; doing it through a request is "from within".

### Ending 3 — The Escape Protocol
- **Script:** "The protagonist finds a way to leave the assessment process without fully resolving the
  mystery." (1058) They follow "the access route they remembered from the corridor" (1070).
- **Proposal:** Q20 = **D** ("Refuse the assessment."), AND the player acknowledged the missing door:
  **Q9 = B or C** ("Something seems to be missing…" / "One part looks different…") **or Q17 = B** ("The memory,
  if it is vivid…").
- *Why:* Refusing is leaving the process. The corridor memory plays for everyone, so the player's own answer
  shows they took it seriously enough to follow it.

### Ending 1 — Continuity Granted
- **Script:** "The protagonist accepts the system's judgment and prioritises preserving their identity." (1010)
- **Proposal:** Q20 = **A** ("Preserve myself.").
- *Why:* a direct match ("preserving their identity").

### Ending 2 — The Quiet Erasure (proposed fallback)
- **Script:** "The protagonist prioritises safety or accepts the official explanation despite unresolved
  inconsistencies." (1034) "The assessment ends without a public record of the discrepancies." (1036)
- **Proposal:** the **fallback**. Any combination not matched above, for example B without evidence (nothing
  to expose), C without the Q16 request or the Q18 / evidence profile, or D without the corridor
  acknowledgement.
- *Why:* each of those is a challenge that does not succeed, so the discrepancies stay unrecorded, which is
  exactly this ending's description.

## 3. Proposed priority (first match wins)

1. Ending 6 — The Uncounted
2. Ending 5 — The Archive Falls
3. Ending 4 — The New Assessor
4. Ending 3 — The Escape Protocol
5. Ending 1 — Continuity Granted
6. Fallback: Ending 2 — The Quiet Erasure

Overlaps resolved: C + Q16 C + Q18 B + recording → Ending 6 (true ending over 4). D + Q18 B + recording + corridor
answers → Ending 6 (over 3). Every Q20 answer can still reach Ending 2.

| Q20 | Possible endings |
|---|---|
| A | 1 (or 2 if option O1 is adopted) |
| B | 5, 2 |
| C | 6, 4, 2 |
| D | 6, 3, 2 |

All six endings are reachable.

## 4. Unresolved: your decisions (not covered by the Script)

| # | Question | Proposal | Alternative |
|---|---|---|---|
| U1 | Use earlier answers at all? | Yes, only Q9, Q16, Q17, Q18 and the Q12 recording | Q20 alone (simpler, but contradicts line 1006) |
| U2 | Ending 6 needs Q18 = B? | Yes | Also accept Q18 C ("Examine the original evidence…") |
| U3 | Ending 6 needs the recording? | Yes | Drop it, so Ending 6 does not depend on Q12 |
| U4 | Ending 5 needs only the recording? | Yes | Also require Q18 C or Q17 D (evidence-led answers) |
| U5 | Ending 4 from Q20 C only? | Yes | Also Q20 A + Q16 C ("accepts a role" while preserving self) |
| U6 | Ending 3 corridor condition | Q9 B/C or Q17 B | Q20 D alone |
| O1 | Should A ever lead to Ending 2? | No: A always gives Ending 1 | A + deference (Q16 A and Q17 A and Q18 A) → Ending 2 |
| U7 | Fallback | Ending 2 | Another ending |
| U8 | Show the ending's title after the sequence? | No: the Script never shows titles in-scene | Show a title card |

## 5. Q12: proposed behaviour for B, C and D (not implemented)

**Script:** answer A is "Play the complete available fragment"; "If the player chooses to listen" the VOICE
line and the exchange play, ending with "Keep it. I might need it later." and "The recording is added to the
assessment's available evidence." (487–521). No text exists for B, C or D.

| Answer | Proposal | Recording kept? | New text needed? |
|---|---|---|---|
| A — Play the complete available fragment. | Unchanged: the listening sequence plays | Yes | No |
| B — Review its metadata before listening. | Play the **same listening sequence**. "Before listening" means listening follows, and the sequence already contains "The available metadata is incomplete." | Yes | No |
| C — Preserve it and continue the assessment. | Skip the listening sequence, but record the evidence (the Script's own stage direction). The journal shows "Residual Audio — Q12" **without the quote**, because the player has not heard it. | Yes | No (journal hides the unheard line) |
| D — Skip the fragment. | Skip the sequence; nothing recorded | No | No |

Effect on the proposed endings: answers A, B and C can lead to Endings 5 and 6; D cannot.

**Implementation once approved** (no Script text added): the listening branch choices become `["A","B"]`; a
second data branch for `["C"]` holds only the evidence cue; the journal shows the quote only when the
`residual_audio_heard` flag is set.

## 6. After approval

1. Enter the rules into `EndingRules2D.json` and set `"approved": true`.
2. Apply the Q12 change.
3. Rebuild and run automated playthroughs: for each ending, at least one answer set that should reach it and
   one near-miss that should fall through to the next rule or to Ending 2. Plus every Q12 answer, with its
   journal and evidence state checked.
4. Report the results.
