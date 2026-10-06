#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""批量把 hlslcc 产物转成 Unity HLSL .shader。

对每个 shader：
  * 产物目录 /tmp/hlslcc_all/<Name>
  * 原版 dummy .shader（取 Properties / Tags / 渲染状态 / Fallback）
  * 输出到 <out>/<Name>.shader

不含 SSBO 的变体优先（WebGL2 = GLES 3.0 不支持 SSBO）。
"""
import argparse
import json
import os
import subprocess
import sys

TOOLS = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(TOOLS)


def dummy_path(shader_name, shader_dir):
    flat = shader_name.replace("/", "_")
    p = os.path.join(ROOT, shader_dir, flat + ".shader")
    return p if os.path.exists(p) else None


def srcdir_for(src, short):
    """产物目录名：多数是 shader 短名，少数把空格换成了下划线。"""
    for cand in (short, short.replace(" ", "_"),
                 short.replace(" ", "_").replace("(", "_").replace(")", "_")):
        p = os.path.join(src, cand)
        if os.path.isdir(p):
            return p
    return None


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--src", default="/tmp/hlslcc_all")
    ap.add_argument("--shader-dir", default="Assets/Shader")
    ap.add_argument("--out", default=os.path.join(ROOT, "Assets/Shader/Restored"))
    ap.add_argument("--names", default="/tmp/shader_names.txt")
    ap.add_argument("--tex-alias-dir", default=None,
                    help="目录里放 <Name>.json 形式的 tN->属性名映射")
    a = ap.parse_args()

    names = [l.strip() for l in open(a.names, encoding="utf-8")
             if l.strip()]
    os.makedirs(a.out, exist_ok=True)

    ok, skip, fail = [], [], []
    for full in names:
        short = full.split("/")[-1]
        srcdir = srcdir_for(a.src, short)
        if not srcdir:
            skip.append((short, "无反编译产物"))
            continue
        alias = None
        if a.tex_alias_dir:
            ap_path = os.path.join(a.tex_alias_dir, short + ".json")
            if os.path.exists(ap_path):
                alias = ap_path
        cmd = [sys.executable, os.path.join(TOOLS, "glsl_to_unity.py"),
               "--dir", srcdir, "--shader", full,
               "--out", a.out,
               "--out-name", short + ".shader",
               "--from-dummy", dummy_path(full, a.shader_dir) or ""]
        if alias:
            cmd += ["--tex-alias", alias]
        r = subprocess.run(cmd, capture_output=True, text=True)
        if r.returncode == 0:
            warn = "含SSBO!" if "WebGL2 无法编译" in r.stdout else ""
            ok.append((short, warn))
            print(f"  OK   {short} {warn}")
        else:
            fail.append((short, (r.stderr or "").strip().splitlines()[-1:]))
            print(f"  FAIL {short}: {(r.stderr or '').strip().splitlines()[-1:]}")

    print(f"\n成功 {len(ok)}  跳过 {len(skip)}  失败 {len(fail)}")
    for s, w in ok:
        if w:
            print("   注意:", s, w)
    for s, why in skip:
        print("   跳过:", s, why)
    for s, why in fail:
        print("   失败:", s, why)


if __name__ == "__main__":
    main()
