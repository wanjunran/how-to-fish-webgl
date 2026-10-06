#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""对生成的 .shader 做整体一致性检查（跨 ShaderLab + HLSL 两层）。

与 check_restored_shaders.py 的分工：那个查「未声明标识符」这类HLSL 级
问题；这个查文件结构级的完整性，适合批量跑。

    python3 tools/check_shader_batch.py /tmp/restored7

## 一个曾经踩过的坑

最早的花括号检查只截 `HLSLPROGRAM` 之后的部分，结果 38 个文件**全部**
报「不平衡，差 3」。差的那个 3 是 `ENDHLSL` 之后的三个 `}` ——
分别闭合 Pass、SubShader、Shader。它们对应的 `{` 在 HLSLPROGRAM 之前，
自然数不到。

所以：花括号必须**全文件一起数**。38/38 全绿。
"""
import glob
import os
import re
import sys

# 字符串/注释剔除：复用自查器里的实现，避免两份正则漂移
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import importlib.util

_spec = importlib.util.spec_from_file_location(
    "crs", os.path.join(os.path.dirname(os.path.abspath(__file__)),
                        "check_restored_shaders.py"))
crs = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(crs)
strip = crs.strip_comments_and_strings


def check_one(path):
    name = os.path.basename(path)[:-7]
    raw = open(path, encoding="utf-8", errors="replace").read()
    body = strip(raw)
    issues = []

    m = re.search(r'Shader\s+"([^"]+)"', raw)
    if not m:
        issues.append("无 Shader 声明")
    elif m.group(1).split("/")[-1] != name:
        issues.append(f"shader 名 {m.group(1)!r} 与文件名不符")

    o, c = body.count("{"), body.count("}")
    if o != c:
        issues.append(f"花括号不平衡 {{={o} }}={c}")

    # 生成器用的是 HLSLPROGRAM/ENDHLSL（不是 CGPROGRAM/ENDCG）。
    for lbl in ("HLSLPROGRAM", "ENDHLSL"):
        if lbl not in raw:
            issues.append(f"缺 {lbl}")

    mv = re.search(r"struct Varyings\s*\{(.*?)\}", raw, re.S)
    if mv:
        sems = re.findall(r":\s*(TEXCOORD\d+|SV_\w+)\s*;", mv.group(1))
        dup = sorted({s for s in sems if sems.count(s) > 1})
        if dup:
            issues.append(f"Varyings 语义重复 {dup}")
        else:
            nums = [int(s[8:]) for s in sems if s.startswith("TEXCOORD")]
            if nums and max(nums) > 15:
                issues.append(f"TEXCOORD 索引超WebGL 上限(15): {max(nums)}")

    ma = re.search(r"struct Attributes\s*\{(.*?)\}", raw, re.S)
    if not ma:
        issues.append("无 Attributes 结构")

    if "readonly buffer" in body or "StructuredBuffer" in body:
        issues.append("含 SSBO（WebGL2 = GLES 3.0 不支持）")

    # GLSL 残留
    for pat, lbl in ((r"^\s*#version", "#version"),
                     (r"^\s*layout\(", "layout("),
                     (r"\bvec[234]\s*\(", "vecN("),
                     (r"\bivec[234]\s*\(", "ivecN("),
                     (r"\bmat[234]\b", "matN"),
                     (r"\bgl_Position\b", "gl_Position"),
                     (r"\bgl_FragCoord\b", "gl_FragCoord")):
        if re.search(pat, body, re.M):
            issues.append(f"GLSL 残留 {lbl}")

    return name, issues


def main():
    src = sys.argv[1] if len(sys.argv) > 1 else "/tmp/restored7"
    files = sorted(glob.glob(os.path.join(src, "*.shader")))
    bad = 0
    for f in files:
        name, issues = check_one(f)
        if issues:
            bad += 1
            print(f"=== {name} ===")
            for i in issues:
                print(f"    {i}")
    print()
    print(f"检查 {len(files)} 个文件，{bad} 个有问题")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())