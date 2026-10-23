"""Checks the [Slice2D] ENDSCREEN trace of a Player.log: screens shown exactly once each, in order 1/2 (Reveal) → 2/2 (Epilogue),
with the labels from EndingScreens2D.json and the Epilogue text equal to the ending's Script description.
Usage: python -I check_endscreens.py <project root> <player log>"""
import json, re, sys
from pathlib import Path

root, log = Path(sys.argv[1]), Path(sys.argv[2]).read_text(encoding="utf-8", errors="replace")
screens = json.loads((root / "Assets/Narrative/Data/EndingScreens2D.json").read_text(encoding="utf-8"))["endings"]
nodes = {n["id"]: n for n in json.loads((root / "Assets/Narrative/Data/AssessmentScript2D.json").read_text(encoding="utf-8"))["nodes"]}
md = (root / "Documentation/The_Last_Assessment_Complete_Story_Script.md").read_text(encoding="utf-8").splitlines()

lines = re.findall(r"^\[Slice2D\] ENDSCREEN (\w+) (\d)/2 \| (.*)$", log, re.M)
ending = re.search(r"\[Slice2D\] ENDING (\w+)", log).group(1)
d = next(s for s in screens if s["id"] == ending)
title_id = f"ENDING {d['number']}  /  {d['title']}"
summary = nodes[ending]["summary"]
if d["revealStyle"] == "tablet":
    reveal = f"{d['revealOverline']} | {d['revealTitle']} | {d['tabletLabel']} | {d['tabletValue']}"
else:
    reveal = f"{d['revealOverline']} | {d['revealTitle'].replace(chr(10), ' ')} | {d['revealSubline']} | {d['revealFoot']}"
expected = [
    (ending, "1", reveal),
    (ending, "2", f"{title_id} | {d['title']} | {summary} | {d['status']} | records={bool(d['records'])}"),
]
problems = [f"screen sequence/text differs:\n  got      {g}\n  expected {e}" for g, e in zip(lines, expected) if g != e]
if len(lines) != 2: problems.append(f"{len(lines)} ENDSCREEN entries (expected exactly 2, no skips or duplicates)")
if summary not in md: problems.append("Epilogue text is not a line of the canonical Script")
print(f"{ending}: {len(lines)} screens, epilogue = Script line {md.index(summary) + 1 if summary in md else '?'}:", "PASS" if not problems else "FAIL")
for p in problems: print("  ", p)
sys.exit(1 if problems else 0)
