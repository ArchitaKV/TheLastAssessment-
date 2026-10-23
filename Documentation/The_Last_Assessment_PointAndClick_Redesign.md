# THE LAST ASSESSMENT — Point-and-Click Redesign

> Provided by the user on 2026-10-23. The text is reproduced as given. Only the web-copy debris
> was removed: inline reference-image links and "Checking Favorites status" lines.
> Narrative authority stays with `The_Last_Assessment_Complete_Story_Script.md`. Where this
> document conflicts with the Script, see `Design/OpenNarrativeDecisions.md` §4.

We're keeping your existing story, all 20 questions, five acts, and six endings. The change is how the player experiences the story: instead of walking through a 3D environment, they explore cinematic 2D scenes, click clues, talk through memories, and make decisions that affect the final outcome.

The best fit is a hybrid point-and-click mystery: exploration when there is something meaningful to investigate, dialogue when a character or memory matters, and assessment choices at the heart of the game.

## 1. The core gameplay loop

1. Enter a scene — A detailed, static cinematic background
2. Explore — Click highlighted objects, inspect evidence, and reveal details
3. Investigate or answer — Read dialogue, review memories, and choose responses
4. Record the decision — Update evidence, trust, suspicion, and hidden story flags
5. Progress to the next scene — New scenes and endings depend on what the player has learned and done

Not every scene needs all five steps. Some assessment questions can be answered immediately; others should require finding a clue first.

## 2. Adapt all 20 scenes to point-and-click

### Prologue — The Assessment Begins

**0.1 — Assessment Room** (Interactive scene)

The player sees the assessment room from a fixed viewpoint.

* Click the tablet to begin.
* Click the observation window to inspect the silhouette.
* Click the desk to inspect the workspace.
* Selecting Begin opens the assessment dialogue.

**0.2 — Preliminary Valuation**

The tablet displays `143 CREDITS`.

* Click the value to examine it.
* Wait for the secondary value to flicker and disappear.
* The player can acknowledge the result or question it.

This introduces the valuation system without explaining the mystery too early.

### Act I — The Assessment

Goal: Establish the assessment format and begin introducing unreliable memories.

**1.1 — Familiarity (Q1)** — Click the tablet to display the question. The player chooses a response. The room itself remains still; the tablet and dialogue UI change.

**1.2 — Memory Fragments (Q2)** — Four memory images appear on the tablet. Click each to enlarge it and inspect it. Selecting one highlights it in cyan before the player answers.

**1.3 — The Valuation (Q3)** — The score fills the screen. Clicking it reveals the available response options. The player can accept the valuation or challenge it, according to the question's actual wording.

**1.4 — The Old Voice (Q4)** — Click Play to hear the voice. The player can replay it and inspect the waveform. Playback cuts off abruptly, leaving the player to respond to what they heard.

### Act II — The Past

Goal: Introduce the protagonist's former life and make the memories feel personal.

**2.1 — Former Workplace (Q5)** — Click the workstation, inactive monitor, or work summary panel. Inspecting the summary opens Q5. The office remains empty and ordinary.

**2.2 — The Promise (Q6)** — A memory conversation plays: "You promised you wouldn't let them take it." Click the other person's silhouette to continue the conversation or select a response. Do not add an unexplained recorder or note.

**2.3 — The Empty Desk (Q7)** — Click the dust outline, then inspect the scratch along the desk. Each interaction reveals a short observation. The player decides what the missing workstation might mean.

**2.4 — The Personal Memory (Q8)** — Inspect the closed book, empty chair, and rain-streaked window. These clicks reveal memory details, not a full explanation. The player answers Q8 after exploring.

### Act III — The Missing Records

Goal: Shift from emotional memories to concrete evidence of tampering.

**3.1 — The Familiar Corridor (Q9)** — Click the blank wall at the far end. A closer view reveals faint rectangular marks. The player records the discrepancy and answers Q9.

**3.2 — Restricted Records (Q10)** — Click the authorization symbol and the blue `RESTRICTED` indicator. The game presents an access check. The player can inspect what is visible, but cannot freely open protected records.

**3.3 — Missing Years (Q11)** — Click the timeline to zoom in. Inspect entries before and after the gap. The employment status remains continuous despite the missing `3 YEARS, 4 MONTHS`.

**3.4 — Residual Audio (Q12)** — Click the recorder to play the surviving fragment. Clicking the waveform reveals the incomplete timestamp. The player compares the audio with previously heard material.

### Act IV — The Cover-Up

Goal: Connect the evidence without revealing the full simulation twist prematurely.

**4.1 — Authorisation Mark (Q13)** — Click the marked report and compare its symbol with the restricted archive symbol from Q10. Matching evidence is added to the evidence journal.

**4.2 — Previous Evaluations (Q14)** — Click through earlier performance evaluations. The player notices a duplicated timestamp and chooses how to interpret it.

**4.3 — Last Night (Q15)** — Click the access panel or inspect the distant figure. A reconstructed memory plays, then ends in a white flash. Never reveal an explicit cause of death here.

**4.4 — Restricted Access (Q16)** — Click the tablet to request the restricted document. Eterna's access-control system denies the request. The scene reveals the restriction, not the simulation.

### Act V — The Truth

Goal: Build to the reveal, then let the player make an informed final judgment.

**5.1 — Impossible Door (Q17)** — Click the blank wall. As a figure passes, a door appears momentarily and vanishes. The notification reads `REFERENCE MEMORY: ACCESS RESTRICTED`. This is a brief anomaly, not the full reveal.

**5.2 — Conflicting Records (Q18)** — Click each panel to compare the final-night records. The corrupted record briefly displays `SUBJECT INSTANCE: ACTIVE`. The player decides which account seems more credible.

**5.3 — Continuity Grid (Q19)** — Click the observation window to reveal the archive chamber. Inspect the records, repeated profiles, and missing employment years labelled as assessment cycles. The protagonist's record reads `143 CREDITS`. The player finally learns what the assessment environment really is.

**5.4 — Final Judgment (Q20)** — The tablet displays the Continuity Grid; the valuation is gone. The player reviews the evidence journal, then chooses among four final judgments. The ending is determined by the accumulated history plus the final decision.

## 3. Turn the six endings into interactive conclusions

The player should not simply click an ending from a menu. Their accumulated decisions determine which outcomes are available or how the final judgment resolves.

**Ending 1 — Continuity Granted** — The player accepts Eterna's account and approves continuity. The room brightens, `CONTINUITY STATUS: APPROVED` appears, and `143 CREDITS` returns.

**Ending 2 — Quiet Erasure** — The player chooses closure or erasure. The tablet goes blank and `ASSESSMENT CLOSED` gradually disappears.

**Ending 3 — Escape Protocol** — The player discovers and uses the escape route. Clicking the open doorway transitions to the protagonist crossing the threshold, with the destination unknown.

**Ending 4 — New Assessor** — The protagonist accepts a new role. The player clicks the opposite workstation, and the display changes to `CONTINUITY REVIEW — NEW ASSIGNMENT`.

**Ending 5 — Archive Falls** — The player exposes Eterna's records. Clickable displays show the conflicting timelines and released evidence. The ending focuses on disclosure rather than spectacle.

**Ending 6 — The Uncounted** — The player rejects the idea that everything can be measured. The room is left without scores, status labels, or UI. The final text reads: `NOT ALL THAT EXISTS CAN BE COUNTED.`

## 4. The mechanics you actually need

Keep the implementation small. You do not need a complex inventory system or a 3D character controller.

- **Clickable hotspots** — Invisible clickable areas placed over objects in each background. Hovering can reveal a subtle cyan outline or label.
- **Evidence journal** — Stores clues the player has actually discovered. Previously found evidence can be reviewed during the final judgment.
- **Dialogue and choices** — A reusable dialogue panel presents questions, responses, and memory fragments. Avoid building a separate UI from scratch for every scene.
- **Story-state manager** — Tracks decisions and evidence flags. For example, discovering the authorization mark in Q13 can influence how the player interprets the restricted records.
- **Scene transitions** — Use fades, brief zooms, sound, screen flickers, and changes in UI. Your backgrounds can remain static while the experience still feels cinematic.

### How endings should be calculated

Use several hidden variables rather than a single good-versus-evil score.

| Variable            | Tracks                                   |
| ------------------- | ---------------------------------------- |
| `evidence`          | Important clues discovered               |
| `trust_eterna`      | Acceptance of Eterna's claims            |
| `suspicion`         | Willingness to challenge inconsistencies |
| `resistance`        | Actions taken against restrictions       |
| `continuity_belief` | Attitude toward the Continuity Grid      |

These are proposed implementation variables, not new story facts. Each choice updates relevant variables or flags. At Q20, evaluate the full history and final decision to select the ending.

## 5. How to build it in Unity

Scenes: Background image · Clickable hotspots · Dialogue and choice Canvas · Evidence journal panel · Scene transition controller

Core scripts:

- `SceneManager` — loads the next scene
- `Hotspot` — handles clicks on objects
- `DialogueManager` — displays text and choices
- `EvidenceManager` — records discovered clues
- `StoryManager` — stores choices and resolves endings

Recommendation: build one complete vertical slice before converting all 20 questions. Start with `0.1 Assessment Room` → click the tablet → `0.2 Preliminary Valuation` → display `143 CREDITS` → proceed to Q1. Make sure the clicks, dialogue, transitions, and state saving work before creating the rest.
