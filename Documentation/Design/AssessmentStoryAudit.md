# Assessment System — Story Mapping (v2)

Status: **draft for approval** · No gameplay code changed yet.

## 1. Narrative authority

1. **Primary:** `Documentation/The_Last_Assessment_Complete_Story_Script.md` (the Complete Story Script, 2026-10-23). All structure, dialogue, choices, act progression, the Q19 reveal, Q20 and the six endings come from here, verbatim.
2. **Secondary, non-contradicting detail only:** `The_Last_Assessment_Story_Bible_Updated.pdf`, `The_Last_Assessment_Story_Bible.pdf`, `The_Last_Assessment_Scene_Storyline_Updated.md`.
3. Where they conflict, the Script wins and the conflict is listed in §4. Nothing from the secondary sources is merged in silently.

## 2. Question map

Every question: Assessor asks → player picks **A–D** → scripted follow-up conversation plays (same for all answers unless noted) → next question. Protagonist lines in the follow-ups are **scripted, not player choices**.

| # | Act | Title | Choices (A / B / C / D, abbreviated) | Script-driven discoveries (all players) | Choice-dependent content in Script | Next |
|---|---|---|---|---|---|---|
| P | — | Prologue — Post-Death Assessment | none | Death "confirmed"; **PRELIMINARY VALUATION: 143 CREDITS**; 20 questions announced | — | Q1 |
| 1 | I | Familiarity | quite often / occasionally / rarely / lately more often | — | none | Q2 |
| 2 | I | The Memory Worth Keeping | clock repair / embrace / diagram / damaged badge | Badge photo feels familiar ("Familiarity has been noted") | none | Q3 |
| 3 | I | The Valuation | not significant / system values some more / record missing or misclassified / need criteria | 143 below average; **a second number flashes and vanishes** | none | Q4 |
| 4 | I | An Old Voice | speaking softly / calling from another room / familiar unidentified / none | Audio fragment **"You promised—"**; replay refused | none | Q5 |
| 5 | II | Professional Judgment | rules / people / errors / consequences | **Protagonist was a memory auditor**; a former colleague's name glimpsed | none | Q6 |
| 6 | II | An Unfinished Promise | did what I could / failed someone / another way / can't recall | Voice returns: **"You promised you wouldn't let them take it."** | none | Q7 |
| 7 | II | The Empty Desk | accept / ask colleagues / remember last talk / look for why it was hidden | Reconstructed office; colleague who tapped the desk | none | Q8 |
| 8 | II | Looking Back | worked harder / people / questioned decisions / understood events | Unarchived experiences can't count toward valuation | none | Q9 |
| 9 | III | The Familiar Corridor | accurate / something missing / one part different / can't compare | **Missing door** at the corridor end (remembered going through it) | none | Q10 |
| 10 | III | A Difficult Decision | rules / harm of withholding / consent / verification | **ARCHIVE REFERENCE: PARTIAL** | none | Q11 |
| 11 | III | A Gap in the Timeline | fewer records / recorded elsewhere / undocumented change / can't judge | **3 years 4 months gap**; Assessor admits access ≠ all memories | none | Q12 |
| 12 | III | Residual Audio | **play / review metadata / preserve & continue / skip** | (if played) **"If you're hearing this, they've already found my records…"** | **Only choice-dependent scene:** listening gates the voice line; the scripted ending "Keep it…" adds the recording to evidence | Q13 |
| 13 | IV | Exceptions | greater harm / justified openly / procedure failing / after evidence | **Authorisation mark** feels familiar (pays off in Q19) | none | Q14 |
| 14 | IV | The Consequences | reasoning / harm / tried to correct / outcome reveals assumptions | Possibly assessed before; links back to the 3y4m gap | none | Q15 |
| 15 | IV | The Last Night | conversation / alarm / felt wrong / nothing | Corridor, rain, red light: **"You need to leave. Now."**; official record says "incident" | none | Q16 |
| 16 | IV | The Limits of the Assessment | complete anyway / mark uncertain / request access / reconsider completion | **Assessor's access restricted; request denied by Eterna's access-control** (restrictions only) | none | Q17 |
| 17 | V | A Small Error | official record / the memory / neither yet / strongest independent evidence | **Door glitch** in the Q9 corridor; **REFERENCE MEMORY: ACCESS RESTRICTED** | none | Q18 |
| 18 | V | Two Versions | strongest verification / preserve both / examine original / reconsider linking | Conflicting final-night records; **CONTINUITY STATUS: UNAUTHORISED**, **SUBJECT INSTANCE: ACTIVE** | none | Q19 |
| 19 | V | The Continuity Grid | **none** | **CONTINUITY GRID** internal record; protagonist connects 143 / gap / restricted records / repeated assessments | none | Q20 |
| 20 | V | Final Judgment | **Preserve myself / Preserve the truth / Change the system / Refuse the assessment** | — | → ending evaluation, then "ASSESSMENT COMPLETE. / OUTCOME PENDING." | Ending |

Reveal pacing is preserved: Q16 restrictions only → Q17 brief glitch → Q18 conflicting records + subject-instance clue → Q19 Grid reveal by the protagonist → Q20.

## 3. Evidence and memory flags

Because every discovery above (except Q12) is scripted for all players, **only two kinds of state actually vary between players:** the A–D answer to each question, and the Q12 audio decision.

| Flag | Set by | Varies? |
|---|---|---|
| `valuation_143_seen` | Prologue / Q3 | no |
| `second_number_glimpsed` | Q3 | no |
| `voice_fragment_heard` | Q4, Q6 | no |
| `auditor_role_known` | Q5 | no |
| `corridor_door_remembered` | Q9 (used by Q17, Ending 3) | no |
| `archive_partial_seen` | Q10 | no |
| `timeline_gap_seen` | Q11 | no |
| `residual_audio_heard` | Q12 A | **yes** |
| `residual_audio_preserved` | Q12 (see decision D2) | **yes** |
| `authorisation_mark_recognised` | Q13 | no |
| `assessor_restrictions_known` | Q16 | no |
| `door_glitch_seen` | Q17 | no |
| `subject_instance_seen` | Q18 | no |
| `continuity_grid_found` | Q19 | no |
| `final_choice` | Q20 | **yes** |

## 4. Conflicts resolved in favour of the Script

| # | Conflict | Resolution |
|---|---|---|
| C1 | Chamber opening ("Consciousness recovery successful…", three questions) comes from the old storyline/Bible. | Replaced by the Script's **Prologue — Post-Death Assessment** dialogue. |
| C2 | "Who are you?" / "What is this place?" answers are non-canon placeholders; "Where am I?" is not in the Script either. | All three disabled (kept in source, marked temporary, never shown). |
| C3 | All 20 old questions (Dismissal, Engineer's Agreement, Recording, Auditor, etc.), the 2019–2030 gap, 480→143 reduction, and the old recording line. | Script questions only; gap is 3y4m; 143 is shown at the start; Script recording line. |
| C4 | Bible ending scripts (maintenance passage, preservation protocol, "Continuity status unresolved", etc.). | Script ending dialogue only (e.g. Ending 3 uses the remembered corridor door; Ending 6 ends on "NOT ALL THAT EXISTS CAN BE COUNTED."). |
| C5 | Bible-only mechanics: preservation protocol, Eterna's offer, simulation vulnerability, Assessor relationship. | Not implemented unless you approve them. |
| C6 | Bible trait names (Compliance, Empathy, Curiosity, Defiance, Agency). | Not used; the Script only mentions a "behavioural profile". Answers are recorded raw. |
| C7 | Script setting is "a white room… a glass wall overlooks a quiet city" (rain on the glass in Q15, the city in Q19/Q20/Ending 5). The approved chamber is a dark interrogation room with an observation window and silhouette. | Needs your decision (D4). Dialogue is unaffected. |
| C8 | The approved Prologue *title screen* text states the protagonist was an auditor (Script reveals this at Q5) and that their death was "officially recorded as an accident" (Script: "incident"). | Needs your decision (D5); the title screen is not changed without approval. |
| C9 | `AgentProfiles/project-rules.md` and `Design/ProjectOutline.md` still name the original Bible as authoritative. | Update to point at the Script, with your approval (D6). |

## 5. Unresolved ending conditions

The Script's ending summaries, and the Script hook each one could plausibly use:

| Ending | Script description | Hooks available in the Script | Unresolved |
|---|---|---|---|
| 1 Continuity Granted | accepts the system's judgment, prioritises preserving identity | Q20 "Preserve myself" | Does Q20 A alone suffice, or are compliant earlier answers also required? |
| 2 The Quiet Erasure | prioritises safety or accepts the official explanation despite inconsistencies | Q16 A, Q17 A, Q18 A (defers to official records) | Which Q20 answers lead here; is it the fallback? |
| 3 The Escape Protocol | leaves the assessment without resolving the mystery, via the remembered corridor door | Q9 (door), Q17 (door glitch), Q20 "Refuse the assessment" | What earns the escape route (everyone sees the door)? |
| 4 The New Assessor | challenges from within, accepts a role; "Your request has been reviewed" | Q16 C "Request permission to access the restricted records" | Which Q20 answer; is Q16 C required? |
| 5 The Archive Falls | exposes enough evidence to undermine Eterna's account | Q20 "Preserve the truth", Q12 preserved recording | What counts as "enough evidence"? |
| 6 The Uncounted | rejects record-defined identity; preserves the conflicting records | Q18 B "Preserve both…", Q20 "Change the system" / "Refuse…", Q12 preserved | Exact requirements and priority over 3/4/5. |

Also unresolved: evaluation **order/priority** when several endings match, and the **fallback** ending.

## 6. Decisions needed

**Needed before the Q1–Q2 slice (presentation only, no story content):**
- **D1 Slice presentation.** Proposed defaults: the Script Prologue dialogue plays in the chamber after the title screen; one line per press (click / Enter / Space, ▼ shown); the speaker label switches ASSESSOR / PROTAGONIST / VOICE; on-screen system text (e.g. PRELIMINARY VALUATION: 143 CREDITS) appears as an interface caption; prose stage directions are not shown as dialogue; choices appear after the Assessor's question line; answers are recorded with no visible score.

**Needed before Q12 and the ending system:**
- **D2 Q12 evidence.** The Script plays the post-audio conversation ending in "Keep it… added to evidence" after listening. Which answers set `residual_audio_preserved` (A and C only?), and what plays for B (metadata) and D (skip)? The Script gives no text for B/D.
- **D3 Ending rules.** A table of Q20 answer(s) + required earlier answers/flags for each ending, plus priority order and fallback. A short suggestion built only from §5 hooks is in the chat summary for you to accept or edit.

**Needed eventually (not blocking the slice):**
- **D4 Setting.** Keep the approved dark chamber (Script stage directions about the white room and city are not displayed), or plan a later art pass towards the Script's white room / city view?
- **D5 Title screen.** Keep the approved Prologue title text, or revise it later to avoid revealing the auditor role and "accident" before Q5/Q15?
- **D6 Docs.** Update `project-rules.md` / `ProjectOutline.md` to name the Script as primary source.

## 7. Architecture (unchanged in principle)

```
Assets/Narrative/Data/AssessmentScript.json   all Script text: prologue, questions, follow-ups, endings (verbatim)
Assets/Scripts/Assessment/
  AssessmentData.cs       data model + loader (JsonUtility)
  AssessmentState.cs      plain C#: current node, answers, flags
  AssessmentDirector.cs   chamber MonoBehaviour: plays lines, shows choices, records answers, advances
  EndingEvaluator.cs      C# rules table (added only after D3)
  SaveSystem.cs           single-slot JSON autosave per question (persistentDataPath)
Reused: ChoiceButton.cs, chamber dialogue panel / subject panel / fade.
Scene01Dialogue.cs: retired once the director replaces it (placeholders never shown).
```

## 8. Staged plan

1. Checkpoint: tag `96285de` as `checkpoint/pre-assessment-system`; work on branch `feature/assessment-system`.
2. Data model + director; Script Prologue → **Q1** (exact text, four choices, follow-up conversation) → test.
3. Connect **Q2**; verify choice recording, progression and save/resume → test.
4. Stop and report before Q3–Q20 and the ending system.
