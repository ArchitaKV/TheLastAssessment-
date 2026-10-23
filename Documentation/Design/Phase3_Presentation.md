# Phase 3: Presentation upgrades (ambience, transitions, hotspots)

These are presentation layers only. The Script, `AssessmentScript2D.json`, `EndingRules2D.json`,
`EndingScreens2D.json`, the ending text and the outcomes are unchanged. No dialogue, clue, evidence or
consequence was added. The scene changes were made additively by `Assessment2DBuilder.AddPhase3ToSavedScene`,
which is safe to re-run.

## Ambience (`Ambience.cs`)

- **Track:** the supplied MP3, copied byte for byte to `Assets/Audio/Ambience/Ambience_Assessment.mp3`.
  It runs 5:07 at 78 BPM, has no vocals, is dark and low-frequency, and starts and ends quietly.
  Import settings: Streaming, Vorbis quality 0.6.
- **Volume:** 0.10, set on the director as `ambienceVolume`. In a focused game window, Unity's measured
  mix level was about −36 to −44 dBFS. That is faint but audible. Nobody has listened to it yet; a person
  should confirm the level.
- **Ducking:** the bed drops to 75% while dialogue or a question is on screen. Wordless beats (act cards,
  view entries, endings' cinematic moments) get the full bed. `Ambience.TextShown` is the hook for voice
  lines if any are added later.
- **Pause:** the bed drops to 40% while the pause menu is open, and returns on Resume or Quit to Title.
- **Ramps:** every change glides (SmoothDamp, 1 s), including the fade-in from silence at launch. Nothing
  is cut abruptly.
- **No duplicates:** one persistent instance survives the scene reloads used by Quit to Title and NEW RUN,
  so the bed never restarts or doubles up.
- **Separation:** no story state, save data or ending logic is involved.
- **QA:** an `AMBIENCE level … dBFS` trace line is logged every 10 s.

## Transitions and camera (`Motion2D.cs`, `StoryDirector2D.Enter`, `StoryView.Drift`)

| Moment | Treatment |
|---|---|
| View → view (related scenes, memory images) | 0.8 s cross-dissolve with a camera push. The incoming view eases from 103.5% to 100%; the outgoing view drifts to 102%. |
| From black (first view, after an act card or a scripted `cut`/`fade`) | The existing fade-in, now with the same push. This is the simple fallback. |
| Act cards | The existing fade in, hold and fade out, with the title settling from 97% to 100%. |
| While a view is on screen | A slow idle drift of 1.5% in and back over 30 s. Art, hotspots and panels move together, so nothing misaligns. |
| Dialogue / question panels | Fade in and rise 14 px over about 0.25 s when they appear. Answers can be selected from the first frame. |
| Endings | Unchanged: the ending screens' cross-dissolve and push. |
| Glitch / flicker / flash | Only where the Script cues them (unchanged). None were added. |

- Every push ends exactly at scale 1 before hotspots go live.
- The stage has a `RectMask2D`, so the push and drift never spill into the black bars on 21:9 or 4:3 screens.
- `-shots` QA runs set `Motion2D.Instant`, so screenshots capture views at rest.
- **Not done:**
  - Parallax: the art is single flattened images, so there are no layers to move.
  - Blur/focus pulls: these need a custom UI shader, and the push gives a similar focus shift.
  - Per-object environmental animation (lights, rain): it would need separated art layers, and it could
    read as story events the Script doesn't contain.

## Hotspots (`Hotspot.cs`, `Scenes2D.json`)

There are three kinds:
- **Advance:** the existing, approved story interactions. They are unchanged.
- **Visual:** new. A visual hotspot shows a hover outline with a faint wash, uses a ring cursor, and pulses
  the outline on click. It has no text and no story effect, and it logs a `LOOK` trace line.
- **Disabled.**

Rules for visual hotspots:
- They are live only while their view is on screen and no question is open, so they can never select an
  answer.
- They are mouse-only extras that don't take keyboard focus, so Enter, Space and the arrow keys behave as
  before.
- A mouse press on one doesn't advance the dialogue.

| View (used in) | Hotspot | Kind | Notes |
|---|---|---|---|
| 0.1 (prologue) | Observation window, desk | Visual (were disabled) | Tablet stays Advance (prologue await) |
| 0.2 (prologue, E1) | Value | Disabled (unchanged) | It's a number display, not an object |
| 1.1 (Q1) | Tablet | Advance (unchanged) | |
| 1.3 (Q3) | Value | Advance (unchanged) | |
| 1.4 (Q4) | none | | Q4's no-replay rule: nothing to click in the audio view |
| 2.3 (Q7, E2) | Desk | Visual (new) | |
| 3.2 (Q10) | Drawers (right cabinet) | Visual (new) | Not the Q19 archive grid (5.3) |
| 3.3 (Q11, Q14) | Monitor | Disabled (new) | The Timeline panel always covers the monitor, so a highlight would land on clue data |
| 3.4 (Q12) | Recorder | Advance (unchanged) | Armed only for Q12-B's approved "click the recorder" |
| 3.4 (Q12) | RecorderLook | Visual (new) | Sits beneath the approved hotspot, which takes clicks while armed |
| 4.1 (Q13) | Paper stack (left) | Visual (new) | Not the paper where the Q13 mark appears |
| 4.2 (Q14) | Desk monitor | Visual (new) | Clear of the History panel |

## Related input fixes made during this phase

- Enter on an ending screen works after a mouse click on empty space.
- Enter on the pause menu's Resume no longer reaches the story underneath. Before, it advanced a line, or
  on the Epilogue it triggered NEW RUN. Fixed with `GameMenus.ClosedFrame`.
- Keyboard focus returns to the last highlighted answer after you close the pause menu during a question.

## Tests

- `Documentation/QA/tools/check_cues.py` (new): the full cue order against the Script data, plus the
  Q4, Q13 and Q19 locks.
- Existing checks: `check_trace.py` (displayed text), `check_endscreens.py`, and the 18-path / ending /
  screenshot runners.
