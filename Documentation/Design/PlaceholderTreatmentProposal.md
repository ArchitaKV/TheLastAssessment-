# Placeholder Presentation Proposal (not implemented)

Status: **proposal for approval.** Nothing changed in the game. Written 2026-10-23.

The Figma frames contain placeholder values that neither the Figma file nor the Script fills in.
The game shows them verbatim today:

| Where | Figma text | What it stands for | Script support |
|---|---|---|---|
| Q5 work summary (view 2.1) | `[NOT PROVIDED]` under RECORD STATE | The record's state is missing | The Script says the summary "closes before the protagonist can examine it further" |
| Q11 / Q14 timeline (view 3.3) | `[DATE]`, `RECORD / [ID]` ×6 | Dated records either side of the gap | The Script gives only "Three years and four months"; no dates |
| Q14 history (view 4.2) | `[TIMESTAMP 01]`, `[TIMESTAMP 02]`, `[TIMESTAMP 03]` | Previous evaluations | "previous performance evaluations"; timestamps are only revealed at Q19 |

Rule kept: no dates, IDs or names are invented, and the Script is not touched. Only how the missing value
*looks* changes, so it reads as a system field with no value, which is what each one means.

## Proposed treatment (recommended)

Use the "unreadable value" convention the Figma design already uses for the Q12 recorder
(`--:4-:-- / --.--.----`) and the Q4 player (`00:-- / --:--`):

| Figma text | Shown as | Style |
|---|---|---|
| `[DATE]` | `--.--.----` | Same position and size, dim grey (#7D8790), tracked mono-style spacing |
| `RECORD / [ID]` | `RECORD / ----` | Keeps Figma's label; only the bracketed value becomes dashes |
| `[TIMESTAMP 01]` / `02` / `03` | `--.--.----  --:--` | Row order and the REVIEWED / COMPLETE status column unchanged |
| `[NOT PROVIDED]` | `NOT PROVIDED` in a thin-outlined status chip | Brackets removed; the wording is the designer's own and is a real system status, so it stays |

Why this treatment:
- It looks deliberate and consistent with frames you already designed.
- It matches the Script's recurring theme that records "are not available in this assessment", without
  saying more than the placeholder did.
- When you supply real values later, they replace the dashes in `Scenes2D.json`; no code changes.

## Alternatives

- **A — Keep Figma text, restyle only:** brackets stay; dim grey with a dashed outline. Most literal, but
  still reads as unfinished.
- **B — Redaction bars:** solid dark blocks. This adds a meaning (deliberate concealment) that these fields
  don't establish, so it is not recommended without your approval.

## Implementation once approved

Edit the eight text entries in `Assets/Narrative/Data/Scenes2D.json`, add a `chip` style for the status
value, rebuild, and re-capture the Q5, Q11 and Q14 screenshots. No story data or Script changes.
