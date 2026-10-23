"""Cue-order and story-lock check for a Player.log [Slice2D] trace.

Rebuilds the expected visual-cue sequence (view switches, show/hide, waits, flags, evidence...) from
AssessmentScript2D.json for the nodes the run played, following the answers the trace recorded, and
requires the trace's CUE lines to match it exactly. Then asserts the specific story locks:
  Q4  - the fragment plays once and the view offers nothing to replay it (no hotspot, no second play)
  Q13 - the authorisation mark appears only after its scripted line, never earlier in the run
  Q19 - the archive reveal runs with no player action (no await / hotspot inside Q19)
Optional presentation clicks (LOOK lines) must not change any of this.
Usage: python -I check_cues.py <project root> <player log>
"""
import json
import re
import sys
from pathlib import Path

root, log = Path(sys.argv[1]), Path(sys.argv[2]).read_text(encoding="utf-8", errors="replace")
data = json.loads((root / "Assets/Narrative/Data/AssessmentScript2D.json").read_text(encoding="utf-8"))
nodes = {n["id"]: n for n in data["nodes"]}
trace = re.findall(r"^\[Slice2D\] (.*)$", log, re.M)
looks = sum(t.startswith("LOOK ") for t in trace)
# Presentation log lines (ambience levels, optional looks) are not story events; the locks are checked without them.
trace = [t for t in trace if not t.startswith(("AMBIENCE", "LOOK "))]

start = re.search(r"START node=(\w+)", log).group(1)
answers = dict(re.findall(r"ANSWER (\w+)=(\w)", log))
m = re.search(r"ENDING (\w+)", log)
ending = m.group(1) if m else None  # a hand-test log may stop before the end

def cues(lines):
    return [l["visual"] for l in lines or [] if l.get("visual")]

expected, node_of = [], []
nid = start
while nid:
    n = nodes[nid]
    seq = cues(n.get("pre"))
    if n.get("question"):
        for b in n.get("branches") or []:
            if answers.get(nid) in b["choices"]:
                seq += cues(b["lines"])
        seq += cues(n.get("post"))
    expected += seq
    node_of += [nid] * len(seq)
    nid = n.get("next") or (ending if nid == "q20" else None)

got = [t[4:] for t in trace if t.startswith("CUE ")]
problems = []
if ending is None:
    expected, node_of = expected[:len(got)], node_of[:len(got)]  # partial run: check what was played
if got != expected:
    i = next((k for k, (a, b) in enumerate(zip(got, expected)) if a != b), min(len(got), len(expected)))
    problems.append(f"cue sequence differs at #{i} ({node_of[i] if i < len(node_of) else '?'}): got {got[i:i+3]} expected {expected[i:i+3]}"
                    f" ({len(got)} vs {len(expected)} cues)")

def section(nid):
    """Trace lines from the node's first cue/line to the next node's first cue (by position in expected)."""
    idx = [k for k, x in enumerate(node_of) if x == nid]
    if not idx:
        return None
    cue_positions = [k for k, t in enumerate(trace) if t.startswith("CUE ")]
    lo = cue_positions[idx[0]]
    nxt = idx[-1] + 1
    hi = cue_positions[nxt] if nxt < len(cue_positions) else len(trace)
    return trace[lo:hi]

q4 = section("q4")
if q4 is not None:
    if sum(t == "CUE wave:play" for t in q4) != 1: problems.append("Q4: fragment did not play exactly once")
    if any(t.startswith(("HOTSPOT", "CUE await")) for t in q4): problems.append("Q4: a replay/hotspot interaction occurred")

if "q13" in node_of:
    first_mark = next((k for k, t in enumerate(trace) if t == "CUE show:Mark"), None)
    scripted = next(i for i, l in enumerate(nodes["q13"]["post"]) if l.get("visual") == "show:Mark")
    before = nodes["q13"]["post"][scripted - 1]
    if first_mark is None: problems.append("Q13: mark never shown")
    elif trace[first_mark - 1] != f"LINE {before['speaker']} | {before['text']}":
        problems.append(f"Q13: mark shown after '{trace[first_mark - 1][:60]}', not after its scripted line")

q19 = section("q19")
if q19 is not None and any(t.startswith(("CUE await", "HOTSPOT")) for t in q19):
    problems.append("Q19: the archive reveal waited for a player action")

print(f"{Path(sys.argv[2]).name}: {len(got)} cues from {start} to {ending or '(partial run)'}, {looks} optional looks:", "PASS" if not problems else "FAIL")
for p in problems:
    print("  ", p)
sys.exit(1 if problems else 0)
