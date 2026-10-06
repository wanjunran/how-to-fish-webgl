#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""对生成的 .shader 做整体一致性检查（跨 ShaderLab + HLSL 两层）。

与 check_restored_shaders.py 的分工：那个查「未声明标识符」这类HLSL 级
问题；这个查文件结构级的完整性，适合批量跑。

    python3 tools/check_shader_batch.py Assets/Shader

默认扫 ``Assets/Shader``（真正要进 CI 的那批），并且**只查已恢复的**
文件 ——判据是「不含 ``DummyShaderTextExporter`` 标记」。剩下的 69 个
空壳由 ``sync_official_shaders.py`` 在 CI 里用URP/包源码覆盖，它们的
源码在 ``Library/PackageCache`` 里、格式与本检查器无关。

## 又一个曾经踩过的坑：查错了目录还以为查出了 bug

这个脚本的默认参数一度是 ``/tmp/restored7``（某次中间批次的临时输出目录）。
于是工作区的文件改了、脚本还在扫旧目录，TEXCOORD 超限的报错看起来像是
回归了。实际工作区那份已经是 ``TEXCOORD0..13`` 连续编号。

教训：**检查永远指向最终交付的那份**，临时目录只在校验生成结果时用。

## 一个曾经踩过的坑（历史）

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
    elif m.group(1).replace("/", "_") != name:
        # AssetRipper 导出时把 Shader 名里的路径分隔符 "/" 转义成文件名里的
        # "_"，例如 Shader "Shader Graphs/Grass" -> Shader Graphs_Grass.shader。
        # 所以比对前必须做同样的转写，否则 43 个文件全部误报。
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

    # SSBO。**三种写法都要查** —— 只 grep "readonly buffer" 会漏掉后两种：
    #   1. HLSLcc 输出的 GLSL 原文 `readonly buffer _Structured_tN_buf...`
    #   2. 转成 HLSL 后正文里残留的 `_Structured_tN_buf[...]` 数组下标访问
    #      （声明那行被 helper 分支吃掉了，但引用还在）
    #   3. 转义名形式 `StructuredBuffer<T> _Structured_tN_buf;`
    # GLES 3.0（= WebGL2）没有 SSBO，那是 GLES 3.1 才有的东西。
    #
    # 这里不能用 \b 前缀：名字是 `_Structured_t3_buf`，`S` 前面是下划线，
    # 而 `_` 和 `S` 同属 \w，之间**不存在词边界**，所以 `\bStructured`
    # 永远匹配不上 —— 检查器会静默漏报（URPDecal / LaserDotDecal 就是这样
    # 被误判成"38/38 全消除"的）。改用 `(?<!\w)` 的显式否定。
    if re.search(r"readonly buffer|StructuredBuffer|"
                 r"(?<!\w)_Structured_t\d+_buf", body):
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

    # fragment shader 必须有且仅有一个 location 0 的输出语义。
    #
    # 没有 SV_Target 语义输出的 frag 什么都不渲染（画面直接空掉），而多于
    # 一个则是 MRT —— GLES 3.0 没有多渲染目标。这两条都是"能编译但画面
    # 错"的类型，光看未声明标识符抓不到。
    sigs = re.findall(r"frag\s*\([^)]*\)\s*(?::\s*(SV_\w+))?", raw)
    if sigs:
        sig = sigs[0]
        if not sig:
            issues.append("frag 没有 SV_Target 语义（不会渲染任何东西）")
        elif sig != "SV_Target0":
            issues.append(f"frag 输出语义是 {sig}，应为 SV_Target0")

    # 括号平衡（花括号在全文件层面查过了，这里补圆/方括号 —— 只截
    # HLSLPROGRAM 段，所以是 HLSL 自己的括号，必须在这一段内平衡）
    hl = re.search(r"HLSLPROGRAM(.*?)ENDHLSL", raw, re.S)
    if hl:
        h = strip(hl.group(1))
        for o, c, lbl in (("(", ")", "圆括号"), ("[", "]", "方括号")):
            if h.count(o) != h.count(c):
                issues.append(
                    f"HLSL 段{lbl}不平衡 {h.count(o)} vs {h.count(c)}")

    return name, issues


def main():
    src = sys.argv[1] if len(sys.argv) > 1 else "Assets/Shader"
    all_files = sorted(glob.glob(os.path.join(src, "*.shader")))

    # 筛出「本流水线恢复的」那批，两个条件缺一不可：
    #   - 含 HLSLPROGRAM：生成器(glsl_to_unity.py) 的产物特征。AssetRipper
    #     能正常反编译的包内 shader(TMP/Skybox-Procedural 等) 用 CGPROGRAM，
    #     源码本来就是对的，不归本检查器管。
    #   - 不含 stub 标记：带标记的是AssetRipper 空壳，CI 里由
    #     sync_official_shaders.py 用包源码覆盖。
    # 只用"非空壳"当判据会把那5 个 CGPROGRAM 文件也捞进来，误报一片。
    files, skipped_cg = [], 0
    for f in all_files:
        raw = open(f, encoding="utf-8", errors="replace").read()
        if crs.STUB_MARKER in raw:
            continue
        if "HLSLPROGRAM" not in raw:
            skipped_cg += 1
            continue
        files.append(f)

    bad = 0
    for f in files:
        name, issues = check_one(f)
        if issues:
            bad += 1
            print(f"=== {name} ===")
            for i in issues:
                print(f"    {i}")
    print()
    print(f"检查 {len(files)} 个已恢复文件"
          f"（跳过 {skipped_cg} 个 CGPROGRAM 原生导出 + "
          f"{len(all_files) - len(files) - skipped_cg} 个空壳），"
          f"{bad} 个有问题")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())