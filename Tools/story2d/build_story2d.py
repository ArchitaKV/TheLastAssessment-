"""Generates Assets/Narrative/Data/AssessmentScript2D.json from the canonical Story Script.

Every displayed line, question, choice and caption is copied from the Markdown (bold markers
stripped); nothing is typed by hand. Visual / state cues come from cues.json, anchored to the
Script's own stage directions and lines. The 3D prototype's AssessmentScript.json is not touched.

Usage: python -I build_story2d.py <project root>
"""
import json
import re
import sys
from pathlib import Path

root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
md = (root / "Documentation/The_Last_Assessment_Complete_Story_Script.md").read_text(encoding="utf-8").splitlines()
cue_map = json.loads((Path(__file__).parent / "cues.json").read_text(encoding="utf-8"))

DIALOGUE = re.compile(r"^\*\*([A-Z][A-Z ]+):\*\* (.+)$")
CHOICE = re.compile(r"^([A-D])\. (.+)$")
CAPTION = re.compile(r"^\*\*([A-Z0-9][A-Z0-9 :,.—-]*)\*\*$|^\*([A-Z][A-Z :,.—-]*)\*$")
strip = lambda s: s.replace("**", "")

nodes, node, act, act_title = [], None, "", ""
for raw in md:
    line = raw.strip()
    if m := re.match(r"^# ACT ([IVX]+) — (.+)$", line):
        act, act_title = m.group(1), f"ACT {m.group(1)} — {m.group(2)}"
        continue
    if line.startswith("# PROLOGUE"):
        node = {"id": "prologue", "title": line[2:], "act": "", "actTitle": "", "items": []}
    elif m := re.match(r"^## QUESTION (\d+) — (.+)$", line):
        node = {"id": f"q{m.group(1)}", "number": int(m.group(1)), "title": m.group(2), "act": act, "actTitle": act_title, "items": []}
    elif m := re.match(r"^## ENDING (\d) — (.+)$", line):
        node = {"id": f"ending{m.group(1)}", "title": m.group(2), "act": "", "actTitle": "", "items": []}
    elif line.startswith("# CONTINUITY RULES"):
        node = None
        continue
    elif line.startswith("#") or node is None:
        continue
    else:
        if not line or line == "---":
            continue
        if m := DIALOGUE.match(line):
            node["items"].append(("line", {"speaker": m.group(1), "text": m.group(2)}))
        elif m := CHOICE.match(line):
            node["items"].append(("choice", {"id": m.group(1), "text": strip(m.group(2))}))
        elif m := CAPTION.match(line):
            node["items"].append(("line", {"caption": m.group(1) or m.group(2)}))
        else:
            node["items"].append(("direction", line))
        continue
    nodes.append(node)

errors = []


def cues_for(node_id, kind, text):
    out = []
    for entry in cue_map.get(node_id, []):
        at = entry["at"]
        hit = (kind == at) if at in ("start", "end") else \
            (kind == "direction" and not at.startswith(("before:", "after:")) and text.startswith(at)) or \
            (kind == "before" and at.startswith("before:") and text.startswith(at[7:])) or \
            (kind == "after" and at.startswith("after:") and text.startswith(at[6:]))
        if hit:
            entry["_used"] = True
            out += [{"visual": c} for c in entry["cues"]]
    return out


def build_lines(node_id, items):
    out = []
    for kind, item in items:
        if kind == "direction":
            out += cues_for(node_id, "direction", item)
            continue
        text = item.get("text") or item.get("caption")
        out += cues_for(node_id, "before", text)
        out.append(item)
        out += cues_for(node_id, "after", text)
    return out


result = []
for i, n in enumerate(nodes):
    items = n["items"]
    choice_idx = [k for k, (kind, _) in enumerate(items) if kind == "choice"]
    out = {k: n[k] for k in ("id", "title", "act", "actTitle") if k in n}
    if n["id"].startswith("ending"):
        # The Script's own one-sentence description of the ending (first stage direction of the section).
        out["summary"] = next(t for k, t in items if k == "direction" and t.startswith("The protagonist"))
    if "number" in n:
        out["number"] = n["number"]
    if choice_idx:
        first, last = choice_idx[0], choice_idx[-1]
        q = max(k for k in range(first) if items[k][0] == "line" and items[k][1].get("speaker"))
        out["pre"] = cues_for(n["id"], "start", "") + build_lines(n["id"], items[:q])
        out["question"] = items[q][1]
        out["choices"] = [items[k][1] for k in choice_idx]
        rest = items[last + 1:]
        if n["id"] == "q12":
            # Approved Q12 behaviour (EndingRules_FinalReview.md §4, 2026-10-23); no text is added:
            # A plays and keeps; B highlights the metadata timestamp, waits for a click on the recorder,
            # then plays and keeps; C keeps without playing (only the Script's evidence cue); D skips.
            listening = build_lines(n["id"], rest)
            keep_only = [l for l in listening if l.get("visual", "").startswith("evidence:")]
            out["post"] = []
            out["branches"] = [
                {"choices": ["A"], "lines": listening},
                {"choices": ["B"], "lines": [{"visual": "show:Metadata_Highlight"}, {"visual": "await:Hotspot_Recorder"},
                                             {"visual": "hide:Metadata_Highlight"}] + listening},
                {"choices": ["C"], "lines": keep_only},
            ]
        else:
            out["post"] = build_lines(n["id"], rest)
    else:
        out["pre"] = cues_for(n["id"], "start", "") + build_lines(n["id"], items)
        out["post"] = []
    end_cues = cues_for(n["id"], "end", "")
    if end_cues:
        (out["branches"][0]["lines"] if out.get("branches") else (out["post"] if out.get("question") else out["pre"])).extend(end_cues)
    nxt = nodes[i + 1]["id"] if i + 1 < len(nodes) else ""
    out["next"] = "" if n["id"] == "q20" or n["id"].startswith("ending") else nxt
    result.append(out)

for node_id, entries in cue_map.items():
    if node_id.startswith("_"):
        continue
    for e in entries:
        if not e.get("_used"):
            errors.append(f"cue anchor not found in {node_id}: {e['at']!r}")
    starts = [c for e in entries if e["at"] == "start" for c in e["cues"]]
    if not any(c.startswith("view:") for c in starts):
        errors.append(f"{node_id}: start cues must enter a view (needed for resume)")

data = {
    "source": "Documentation/The_Last_Assessment_Complete_Story_Script.md (generated by Tools/story2d/build_story2d.py)",
    "startNode": "prologue",
    "endings": [n["id"] for n in result if n["id"].startswith("ending")],
    "evidence": [{
        "id": "q12_recording", "title": "Residual Audio", "source": "Q12", "heardFlag": "residual_audio_heard",
        "text": next(l["text"] for l in result if l["id"] == "q12" for b in l["branches"] for l in b["lines"] if l.get("speaker") == "VOICE")
        if False else None}],
    "nodes": result,
}
# Evidence text is the recording's own VOICE line, copied from the Script.
q12 = next(n for n in result if n["id"] == "q12")
data["evidence"][0]["text"] = next(l["text"] for l in q12["branches"][0]["lines"] if l.get("speaker") == "VOICE")

out_path = root / "Assets/Narrative/Data/AssessmentScript2D.json"
out_path.write_text(json.dumps(data, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
print(f"{len(result)} nodes -> {out_path}")
for e in errors:
    print("ERROR", e)
sys.exit(1 if errors else 0)
