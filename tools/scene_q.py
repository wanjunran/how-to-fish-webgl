#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Unity 场景/预制体 YAML 对象查询工具（只读诊断用）。

用法:
  python3 tools/scene_q.py <文件> --names MenuCamera PermaCanvas   # 打印对象的精简块
  python3 tools/scene_q.py <文件> --tree GameObjectName            # 打印层级子树
  python3 tools/scene_q.py <文件> --list 名字正则                  # 列出匹配名字的对象
  python3 tools/scene_q.py <文件> --comp Canvas Image Camera ...   # 按组件类型统计
"""
import argparse
import re
import sys

SPLIT = re.compile(r"(?m)^--- !u!(\d+) &(\d+)\s*$")

# Unity 常见 classID 对照（够用即可）
CLASS = {
    1: "GameObject", 4: "Transform", 20: "Camera", 21: "Material", 23: "MeshRenderer",
    33: "MeshFilter", 43: "Mesh", 45: "Skybox", 54: "Rigidbody", 65: "BoxCollider",
    114: "MonoBehaviour", 223: "Canvas", 224: "RectTransform", 225: "CanvasGroup",
    1142: "CanvasRenderer", 222: "Image", 114: "MonoBehaviour", 199: "ParticleSystem",
    108: "Light", 119: "AudioListener", 81: "AudioListener", 162: "AudioListener",
    328: "VideoPlayer", 212: "SpriteRenderer", 213: "Sprite", 137: "SkinnedMeshRenderer",
    111: "Animation", 95: "Animator", 320: "PlayableDirector",
}


def load(path):
    with open(path, encoding="utf-8", errors="replace") as f:
        src = f.read()
    parts = SPLIT.split(src)
    objs = {}
    for i in range(1, len(parts), 3):
        objs[int(parts[i + 1])] = {"class": int(parts[i]), "body": parts[i + 2]}
    return objs


def field(body, key, default=None):
    m = re.search(r"(?m)^\s*%s:\s*(.*)$" % re.escape(key), body)
    return m.group(1).strip() if m else default


def names(objs):
    out = {}
    for fid, o in objs.items():
        if o["class"] == 1:
            n = field(o["body"], "m_Name")
            if n:
                out[fid] = n
    return out


def cmd_list(objs, args):
    rx = re.compile(args.list, re.I)
    nm = names(objs)
    for fid, n in nm.items():
        if rx.search(n):
            print(f"{fid:>10}  {n}")


def cmd_names(objs, args):
    nm = names(objs)
    inv = {}
    for fid, n in nm.items():
        inv.setdefault(n, []).append(fid)
    for want in args.names:
        rx = re.compile(re.escape(want), re.I) if args.regex else None
        hits = [fid for fid, n in nm.items() if (rx.search(n) if rx else n == want)]
        if not hits:
            print(f"!! 未找到 {want}")
            continue
        for fid in hits:
            dump(objs, fid, nm, args.lines)


def parent_of(objs, fid):
    """给定 GameObject/Transform，返回父 Transform 的 GameObject fileID"""
    b = objs[fid]["body"]
    if objs[fid]["class"] == 1:
        for m in re.finditer(r"(?m)^\s*- component: \{fileID: (\d+)\}", b):
            cid = int(m.group(1))
            if objs.get(cid, {}).get("class") == 4:
                b = objs[cid]["body"]
                break
    father = field(b, "m_Father")
    if not father or father == "0":
        return None
    m = re.search(r"fileID: (\d+)", father)
    if not m:
        return None
    tfid = int(m.group(1))
    goid = int(field(objs[tfid]["body"], "m_GameObject", "0").split("fileID: ")[-1].rstrip("}"))
    return goid


def children_of(objs, fid):
    """返回直接子节点 GameObject fileID 列表"""
    # 先找自己的 Transform
    tfid = None
    if objs[fid]["class"] == 1:
        for m in re.finditer(r"(?m)^\s*- component: \{fileID: (\d+)\}", objs[fid]["body"]):
            cid = int(m.group(1))
            if objs.get(cid, {}).get("class") == 4:
                tfid = cid
                break
    else:
        tfid = fid
    out = []
    for oid, o in objs.items():
        if o["class"] != 4:
            continue
        p = field(o["body"], "m_Father", "0")
        if f"fileID: {tfid}" in p:
            gid = field(o["body"], "m_GameObject", "")
            g = re.search(r"fileID: (\d+)", gid)
            if g:
                out.append(int(g.group(1)))
    return out


def walk(objs, fid, nm, depth=0, maxdepth=99, out=None):
    if out is None:
        out = []
    out.append(("  " * depth) + f"- {nm.get(fid,'?')} (&{fid})")
    if depth >= maxdepth:
        return out
    for c in children_of(objs, fid):
        walk(objs, c, nm, depth + 1, maxdepth, out)
    return out


def cmd_tree(objs, args):
    nm = names(objs)
    inv = {}
    for fid, n in nm.items():
        inv.setdefault(n, []).append(fid)
    for want in args.tree:
        for fid in inv.get(want, []):
            print("\n".join(walk(objs, fid, nm, maxdepth=args.depth)))


def dump(objs, fid, nm, limit):
    o = objs.get(fid)
    if not o:
        print(f"!! 无此 fileID {fid}")
        return
    body = o["body"]
    print("=" * 72)
    print(f"&{fid}  {CLASS.get(o['class'], 'class'+str(o['class']))}  name={nm.get(fid,'-')}")
    lines = body.rstrip().split("\n")
    # 折叠超长的顶点/索引数组
    keep, skipped = [], 0
    for ln in lines[:limit]:
        if len(ln) > 400 or re.match(r"^\s*([-\d.e+]+,?\s*){20,}$", ln):
            skipped += 1
            continue
        keep.append(ln)
    print("\n".join(keep))
    if skipped:
        print(f"... 已跳过 {skipped} 行超长数组 ...")
    # 打印它挂的组件
    if o["class"] == 1:
        comps = [(int(m.group(1)), objs.get(int(m.group(1)), {}).get("class"))
                 for m in re.finditer(r"(?m)^\s*- component: \{fileID: (\d+)\}", body)]
        nice = ", ".join(f"&{c}({CLASS.get(t,t)})" for c, t in comps)
        print(f"[组件] {nice}")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("file")
    ap.add_argument("--names", nargs="*")
    ap.add_argument("--regex", action="store_true")
    ap.add_argument("--tree", nargs="*")
    ap.add_argument("--depth", type=int, default=3)
    ap.add_argument("--list")
    ap.add_argument("--id", nargs="*", type=int)
    ap.add_argument("--lines", type=int, default=45)
    a = ap.parse_args()

    objs = load(a.file)
    nm = names(objs)
    print(f"# {a.file}: {len(objs)} 个对象（{len(nm)} 个 GameObject）")
    if a.list:
        cmd_list(objs, a)
    if a.names:
        cmd_names(objs, a)
    if a.tree:
        cmd_tree(objs, a)
    if a.id:
        for fid in a.id:
            dump(objs, fid, nm, a.lines)


if __name__ == "__main__":
    main()
