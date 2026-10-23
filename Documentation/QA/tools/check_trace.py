"""A4 — Played-path fidelity: compare a Player.log [Slice2D] trace with AssessmentScript2D.json.

Rebuilds what should have been displayed (lines, captions, questions, options, branch lines for the
answers the trace recorded) from the first to the last node played, and diffs it with the trace.
Speaker label suffixes from the Figma panel style ("/ DIALOGUE") are ignored. Exit 0 = exact match.

Usage: python -I check_trace.py <project root> <player log>
"""
import json
import re
import sys
from pathlib import Path

root, log_path = Path(sys.argv[1]), Path(sys.argv[2])
data = json.loads((root / "Assets/Narrative/Data/AssessmentScript2D.json").read_text(encoding="utf-8"))
nodes = {n["id"]: n for n in data["nodes"]}
log = log_path.read_text(encoding="utf-8", errors="replace")
trace = re.findall(r"^\[Slice2D\] (.*)$", log, re.M)

answers = dict(re.match(r"ANSWER (\w+)=(\w)", t).groups() for t in trace if t.startswith("ANSWER "))
start = re.search(r"START node=(\w+)", log).group(1)
ending = next((t.split()[1] for t in trace if t.startswith("ENDING ")), None)
norm = lambda t: re.sub(r"^LINE ([A-Z ]+?)(?: / DIALOGUE)? \| ", r"LINE \1 | ", t)
shown = [norm(t) for t in trace if t.split(" ", 1)[0] in ("LINE", "CAPTION", "QUESTION", "OPTION", "ANSWER")]


def lines(seq):
    out = []
    for l in seq or []:
        if l.get("visual"):
            continue
        out.append(f"CAPTION {l['caption']}" if l.get("caption") else f"LINE {l['speaker']} | {l['text']}")
    return out


expected, node_id = [], start
while node_id:
    node = nodes[node_id]
    expected += lines(node.get("pre"))
    if node.get("question"):
        expected.append(f"QUESTION {node['question']['text']}")
        expected += [f"OPTION {c['id']} | {c['text']}" for c in node["choices"]]
        a = answers.get(node_id, "?")
        expected.append(f"ANSWER {node_id}={a}")
        for b in node.get("branches") or []:
            if a in b["choices"]:
                expected += lines(b["lines"])
        expected += lines(node.get("post"))
    node_id = node["next"] or (ending if node_id == "q20" and ending not in (None, "pending") else "")

errors = [l for l in log.splitlines() if re.search(r"NullReferenceException|Exception: |StoryDirector2D: |AssessmentScript2D: |Assessment2DBuilder: .*not found", l)]
diffs = [(i, a, b) for i, (a, b) in enumerate(zip(shown, expected)) if a != b]
ok = not diffs and len(shown) == len(expected) and not errors
for i, a, b in diffs[:5]:
    print(f"X #{i}: shown   {a}\n        expected {b}")
for e in errors[:5]:
    print("ERROR", e)
print(f"{log_path.name}: start={start} ending={ending} {len(shown)} shown / {len(expected)} expected, {len(errors)} runtime errors:", "PASS" if ok else "FAIL")
sys.exit(0 if ok else 1)
