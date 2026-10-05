#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""按 fileID 精确启停 Unity YAML 组件（局部替换，不重建整个文件）。

用法:
  python3 tools/toggle_comp.py <file> --ids 2707 2716 [--target disable|enable]
  python3 tools/toggle_comp.py <file> --go CloudSphereTop [--comp MeshRenderer] --target disable
默认 dry-run。
"""
import argparse
import re

DOC = re.compile(r"(?m)^--- !u!(\d+) &(\d+)\s*$")

TYPE2NAME = {23: "MeshRenderer", 137: "SkinnedMeshRenderer", 114: "MonoBehaviour",
             20: "Camera", 108: "Light", 199: "ParticleSystem", 33: "MeshFilter"}
NAME2TYPE = {v: k for k, v in TYPE2NAME.items()}


def blocks(src):
    """返回 [(start_of_body, end_of_body, classID, fileID)]"""
    ms = list(DOC.finditer(src))
    out = []
    for i, m in enumerate(ms):
        end = ms[i + 1].start() if i + 1 < len(ms) else len(src)
        out.append((m.end(), end, int(m.group(1)), int(m.group(2))))
    return out


def field(body, key):
    m = re.search(r"(?m)^\s*%s:\s*(.*)$" % re.escape(key), body)
    return m.group(1).strip() if m else None


def resolve_go_ids(src, names, comp_types):
    """根据 GameObject 名字找出其目标组件的 fileID"""
    bs = blocks(src)
    byid = {b[3]: b for b in bs}
    res = []
    for s, e, cid, fid in bs:
        if cid != 1:
            continue
        body = src[s:e]
        nm = field(body, "m_Name")
        if nm not in names:
            continue
        for m in re.finditer(r"(?m)^\s*- component: \{fileID: (\d+)\}", body):
            cf = int(m.group(1))
            if cf in byid and byid[cf][2] in comp_types:
                res.append((nm, cf, byid[cf][2]))
    return res


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("files", nargs="+")
    ap.add_argument("--ids", nargs="*", type=int)
    ap.add_argument("--go", nargs="*")
    ap.add_argument("--comp", nargs="*", default=["MeshRenderer"])
    ap.add_argument("--target", choices=["enable", "disable"], default="disable")
    a = ap.parse_args()

    want = "1" if a.target == "enable" else "0"
    types = {NAME2TYPE[c] for c in a.comp}

    for path in a.files:
        src = open(path, encoding="utf-8", errors="replace").read()
        targets = [(str(i), None) for i in (a.ids or [])]
        if a.go:
            for nm, cf, ct in resolve_go_ids(src, set(a.go), types):
                targets.append((str(cf), nm))
        if not targets:
            print(f"[跳过] {path}: 没找到目标")
            continue

        bs = blocks(src)
        edits = []
        for fid_s, nm in targets:
            for s, e, cid, fid in bs:
                if str(fid) != fid_s:
                    continue
                body = src[s:e]
                cur = field(body, "m_Enabled")
                if cur is None:
                    print(f"   !! &{fid} 无 m_Enabled 字段")
                    continue
                if cur == want:
                    continue
                new = re.sub(r"(?m)^(\s*)m_Enabled:.*$", r"\1m_Enabled: " + want, body, count=1)
                edits.append((s, e, new, f"&{fid}({TYPE2NAME.get(cid,cid)}) {nm or ''} {cur}->{want}"))
                break

        if not edits:
            print(f"[跳过] {path}: 已是目标状态")
            continue
        print(f"[{path}] {len(edits)} 处")
        for _, _, _, d in edits:
            print(f"   {d}")
        out = src
        for s, e, new, _ in sorted(edits, key=lambda x: -x[0]):
            out = out[:s] + new + out[e:]
        open(path, "w", encoding="utf-8").write(out)
        print(f"   >> 已写入 {path}")


if __name__ == "__main__":
    main()
