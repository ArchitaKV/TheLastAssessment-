"""A1 — Script fidelity: a story JSON must match the canonical Story Script word for word.

Every dialogue line, question, choice and caption in the JSON (including Q12 branch lines) must
appear as a full line of the Markdown, in order (bold markers ignored). Every dialogue line,
lettered choice and caption of the Markdown between the first and last matched line must appear
in the JSON. Exit code 0 = pass.

Usage: python -I check_script_fidelity.py <project root> [json path relative to root]
"""
import json
import re
import sys
from pathlib import Path

root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
json_rel = sys.argv[2] if len(sys.argv) > 2 else "Assets/Narrative/Data/AssessmentScript.json"
md_lines = [re.sub(r"^\*([^*]+)\*$", r"\1", l.replace("**", "")) for l in  # bold and italic markers
            (root / "Documentation/The_Last_Assessment_Complete_Story_Script.md").read_text(encoding="utf-8").splitlines()]
data = json.loads((root / json_rel).read_text(encoding="utf-8"))


def form(kind, item):
    if kind == "choice":
        return f"{item['id']}. {item['text']}"
    if item.get("caption"):
        return item["caption"]
    return f"{item['speaker']}: {item['text']}"


def displayed(lines):
    return [("line", l) for l in lines or [] if not l.get("visual")]


failures, cursor, first, last, entries = [], 0, None, None, []


def match_in_order(node_id, seq, start):
    """Finds each entry after the previous one; returns the cursor after the last match."""
    global first, last
    pos = start
    for k, item in seq:
        want = form(k, item)
        entries.append((node_id, want))
        try:
            found = md_lines.index(want, pos)
        except ValueError:
            failures.append(f"[{node_id}] not found in order: {want!r}")
            continue
        first = found if first is None else first
        last = max(last or 0, found)
        pos = found + 1
    return pos


for node in data["nodes"]:
    seq = displayed(node.get("pre"))
    if node.get("question") and node["question"].get("text"):
        seq += [("line", node["question"])] + [("choice", c) for c in node.get("choices") or []]
    cursor = match_in_order(node["id"], seq, cursor)
    # Branches are alternatives (one plays per answer): each is checked from the same point.
    ends = [match_in_order(node["id"], displayed(b["lines"]), cursor) for b in node.get("branches") or []]
    cursor = max(ends + [cursor])
    cursor = match_in_order(node["id"], displayed(node.get("post")), cursor)

json_forms = {w for _, w in entries}
shown = re.compile(r"^[A-Z][A-Z ]+: |^[A-D]\. |^\*?[A-Z0-9][A-Z0-9 :,.—-]+\*?$")
missing = [f"line {i + 1}: {l}" for i, l in enumerate(md_lines[first:last + 1], start=first)
           if shown.match(l) and l.strip("*") not in json_forms]

print(f"{json_rel}: {len(entries)} displayed entries checked against Markdown lines {first + 1}-{last + 1}.")
for f in failures + [f"missing from JSON: {m}" for m in missing]:
    print("FAIL", f)
print("PASS" if not failures and not missing else f"{len(failures) + len(missing)} problem(s)")
sys.exit(0 if not failures and not missing else 1)
