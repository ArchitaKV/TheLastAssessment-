# Ending Rules — Final Review Before Activation

Status: **reviewed, not active.** The live `Assets/Narrative/Data/EndingRules2D.json` still has
`"approved": false`, so the game still ends at "OUTCOME PENDING.". The exact configuration to activate is
`Documentation/Design/EndingRules2D.final-draft.json`. On approval it is copied over the live file unchanged.
Q12 behaviour is also unchanged until approval. Written 2026-10-23.

## 1. Final configuration (first match wins)

| # | Ending | Conditions (all must hold) | Fallback |
|---|---|---|---|
| 1 | ending6 — The Uncounted | Q20 ∈ {C, D} · Q18 = B · evidence `q12_recording` retained | |
| 2 | ending5 — The Archive Falls | Q20 = B · evidence `q12_recording` retained | |
| 3 | ending4 — The New Assessor | Q20 = C · Q16 = C | |
| 4 | ending3 — The Escape Protocol | Q20 = D · and at least one of: Q9 ∈ {B, C} or Q17 = B | |
| 5 | ending1 — Continuity Granted | Q20 = A | |
| — | | no rule matched | **ending2 — The Quiet Erasure** |

No ending name or eligibility hint is shown in the game. The selected ending is written only to the
developer log (`[Slice2D] ENDING …`).

## 2. Referenced answers: exact option text from the Script

| Answer | Script text |
|---|---|
| Q9 B | "Something seems to be missing, though I cannot identify it." |
| Q9 C | "One part looks different from what I remember." |
| Q16 C | "Request permission to access the restricted records." |
| Q17 B | "The memory, if it is vivid and consistent with other recollections." |
| Q18 B | "Preserve both until the differences can be explained." |
| Q20 A | "**Preserve myself.** I need to know who I am before I can change anything else." |
| Q20 B | "**Preserve the truth.** Whatever happens to me, the records must not be hidden." |
| Q20 C | "**Change the system.** If the process is wrong, preserving it is not an acceptable solution." |
| Q20 D | "**Refuse the assessment.** I will not make this decision under rules I was never allowed to understand." |

Automated check: every question ID, answer letter, evidence ID (`q12_recording`) and ending ID in the
configuration exists in `AssessmentScript2D.json`.

## 3. Contradiction and reachability check

All **4,096** combinations of the inputs the rules read were evaluated: Q9, Q16, Q17, Q18, Q20 × 4 answers,
and Q12 × 4 answers, with recording retention as in §4. The evaluation mirrors `EndingRules.Evaluate`.

| Ending | Share of combinations | Reachable from Q20 |
|---|---|---|
| ending1 | 25.0 % | A only (A always gives ending1) |
| ending2 (fallback) | 29.1 % | B, C, D |
| ending3 | 12.7 % | D |
| ending4 | 5.1 % | C |
| ending5 | 18.8 % | B |
| ending6 | 9.4 % | C, D |

- **No unreachable ending and no unsatisfiable condition.**
- Shadowing is intended: Ending 6 takes precedence over 4 (Q20 C) and 3 (Q20 D) when its conditions also hold.
- Q20 A can never give Ending 2. That's by design (option O1 not adopted).
- Answers to Q1–Q8, Q10, Q11, Q13–Q15 and Q19 are not used. Every question must still be answered before Q20,
  so the rules never read a missing answer.

## 4. Q12: heard vs retained

| Flag / state | Set when | Already in the game? |
|---|---|---|
| `residual_audio_heard` (flag) | The listening sequence starts ("If the player chooses to listen…") | Yes |
| `q12_recording` (evidence) + `evidence:q12_recording` (flag) | "The recording is added to the assessment's available evidence." | Yes |

The two are **distinct**, so "heard" and "retained" can differ. The ending rules read only retention.
Today only answer A sets both. The intended behaviour needs this data and code change (no Script text added):

| Answer | Intended | Change needed |
|---|---|---|
| A — "Play the complete available fragment." | Play and retain | None |
| B — "Review its metadata before listening." | Show metadata, then allow the recording to be played and retained | See limitation below |
| C — "Preserve it and continue the assessment." | Retain without playing | New data branch for C with only the evidence cue; the journal hides the quoted line unless `residual_audio_heard` is set |
| D — "Skip the fragment." | Skip; not retained | None |

### Limitation for answer B (decision needed)

The Script writes no text for reviewing metadata, and no prompt for playing afterwards.

- **Metadata:** the only metadata available is Figma p14's incomplete timestamp `--:4-:-- / --.--.----`. It is
  already visible on the recorder for every answer. "Show metadata first" can only be a visual emphasis
  (no new text).
- **"Then allow the recording to be played":** needs a player action the Script doesn't specify. The
  text-free option is to wait for a click on the recorder (a hotspot like the tablet), then run the Script's
  listening sequence.
- **Declining after the metadata** has no outcome or text in the Script, so it can't be offered without
  inventing content. With the option above, B always ends with the recording played and retained, like A.

Choose one:
- **B1 (recommended):** highlight the timestamp, wait for a click on the recorder, then play and retain.
- **B2:** highlight the timestamp, then play automatically.
- **B3:** provide text or an outcome for declining after the metadata.

## 5. Test paths (run after activation)

Format: `-autoplay -default A -answers …`; unlisted questions are answered A. "Expected" comes from the
exhaustive evaluator. Paths marked † need the Q12 change (B / C retention) to be in place.

| Purpose | Answers | Expected |
|---|---|---|
| Ending 6 hit (C) | q18=B, q12=A, q20=C | ending6 |
| Ending 6 hit (D, recording kept unheard) † | q18=B, q12=C, q20=D | ending6 |
| Ending 6 near-miss: recording skipped → next rule | q18=B, q12=D, q16=C, q20=C | ending4 |
| Ending 6 near-miss: Q18 not B | q18=C, q12=A, q20=C | ending2 |
| Ending 5 hit | q12=A, q20=B | ending5 |
| Ending 5 hit (retained unheard) † | q12=C, q20=B | ending5 |
| Ending 5 near-miss: skipped | q12=D, q20=B | ending2 |
| Ending 4 hit | q16=C, q18=A, q20=C | ending4 |
| Ending 4 near-miss: Q16 not C | q16=B, q18=A, q20=C | ending2 |
| Ending 3 hit (Q9 B) | q9=B, q18=A, q20=D | ending3 |
| Ending 3 hit (Q17 B) | q9=A, q17=B, q18=A, q20=D | ending3 |
| Ending 3 near-miss: no corridor answer | q9=A, q17=A, q18=A, q20=D | ending2 |
| Ending 3 shadowed by Ending 6 (priority) | q9=B, q18=B, q12=A, q20=D | ending6 |
| Ending 1 hit, even with a strong challenge profile | q16=C, q18=B, q12=A, q20=A | ending1 |
| Ending 2 via B | q12=D, q20=B | ending2 |
| Ending 2 via C | q16=A, q18=A, q20=C | ending2 |
| Q12 A / B † / C † / D | q12 = each | heard + retained / heard + retained / retained only (journal without quote) / neither |

Each run is also checked with `check_trace.py` (exact Script text, 0 runtime errors).
