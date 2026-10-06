#!/usr/bin/env python3
"""检查「材质设了 keyword，但 shader 从没声明过它」。

## 问题

Unity 的 `shader_feature` / `multi_compile` 决定了一个 shader 会被编译成
**几个变体**。材质勾上的 keyword 只有在 shader 用对应 pragma 声明过时
才有意义；没声明的话Unity 直接忽略它，**不报错、不影响构建、不影响
exit code**，材质于是被当成没勾任何 keyword 的基础变体来渲染。

实测本项目：36 个由 HLSLcc 恢复的 shader 里，**0 个**声明了
`shader_feature`；而材质实际用了 16 种 keyword，其中直接决定渲染方式的
几个都在被忽略之列：

    _SURFACE_TYPE_TRANSPARENT   14 个材质 -> 透明被当成不透明
    _ALPHAPREMULTIPLY_ON        11 个
    _EMISSION                    6 个-> 自发光失效
    _RECEIVE_SHADOWS_OFF         5 个
    UNDERLAY_ON                  5 个（TMP，声明齐全）
    OUTLINE_ON                   4 个
    _COLORADDSUBDIFF_ON          3 个
    _SKIN_TYPE_GRADIENT_NOISE    3 个

（三个 TextMeshPro shader 是 AssetRipper 的 CGPROGRAM 原生导出，
它们保留了原始 pragma，所以 UNDERLAY_ON / OUTLINE_ON 是有效的。）

## 这不是「少写一行pragma」那么简单

HLSLcc 反编译产物的**文件名本身就编码了变体维度**，例如 DefaultParticle：

    p0_PS_sm40__GlobalMipBias__AlphaToMaskAvailable__Additive
    p0_PS_sm40_unity_WorldToObject_LightShadows__Global
    p1_PS_sm40_base        <- DepthOnly
    p3_PS_sm40_base        <- ShadowCaster

也就是说原版确实是多变体、多 Pass 的，我们目前每个 shader 只还原了
**一个**组合。声明 keyword 只能恢复「材质 keyword 被忽略」这一层；
「多变体本身没还原」是另一件事，需要 pick_variant 重建。

所以本脚本**只判定、不修改**，并且把两类问题分开报：
- **A类**：材质用了 keyword，shader 完全没声明 -> 可以用一行pragma 修
- **B 类**：HLSLcc 产物显示原版有多变体，我们只取了一个 -> 需要重建

## 用法

    python3 tools/check_shader_keywords.py
    python3 tools/check_shader_keywords.py --json
"""
from __future__ import annotations

import argparse
import glob
import json
import os
import re
import sys
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SHADER_DIR = os.path.join(ROOT, "Assets", "Shader")

PRAGMA = re.compile(r"^\s*#pragma\s+(?:shader_feature|multi_compile)\s+(.*)$",
                    re.M)
KEYWORD_LINE = re.compile(r"^\s*-\s*(\S+)\s*$", re.M)
SHADER_NAME = re.compile(r'^\s*Shader\s+"([^"]+)"', re.M)

def guid_of_meta(path: str) -> str | None:
    try:
        with open(path + ".meta", encoding="utf-8", errors="replace") as f:
            m = re.search(r"guid:\s*([0-9a-f]{32})", f.read())
        return m.group(1) if m else None
    except OSError:
        return None


def declared_keywords(raw: str) -> set[str]:
    """取 shader 里 shader_feature / multi_compile 声明过的所有 keyword。

    只认带 `__` 或裸关键字的行，且要把续行（反斜杠结尾）拼起来 ——
    Unity 允许一条 pragma 跨行写多个 keyword。
    """
    out: set[str] = set()
    raw = raw.replace("\\\n", " ")
    for m in PRAGMA.finditer(raw):
        for tok in m.group(1).split():
            tok = tok.strip()
            # 跳过开关标记与行尾注释
            if not tok or tok.startswith("//"):
                continue
            if tok == "__":       # 一个都没勾的开关
                continue
            out.add(tok)
            # `KEYWORD 1 2 3` 形式：后面的数字也是独立 keyword
    return out


def mat_keywords(path: str) -> set[str]:
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            text = f.read()
    except OSError:
        return set()
    m = re.search(r"m_ValidKeywords:\s*\n((?:\s*-\s*\S+\n)*)", text)
    if not m:
        return set()
    # 第二行开始才是列表（第一行是 m_ValidKeywords: 之后的空）
    return {ln.strip().lstrip("-").strip()
            for ln in m.group(1).splitlines() if ln.strip()}


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()

    # guid -> (shader 名, 声明的 keyword)
    info: dict[str, tuple[str, set[str]]] = {}
    for p in sorted(glob.glob(os.path.join(SHADER_DIR, "*.shader"))):
        g = guid_of_meta(p)
        if not g:
            continue
        raw = open(p, encoding="utf-8", errors="replace").read()
        m = SHADER_NAME.search(raw)
        info[g] = (m.group(1) if m else os.path.basename(p),
                   declared_keywords(raw))

    # A 类：材质 keyword 未被声明
    a_class: dict[str, dict[str, int]] = defaultdict(lambda: defaultdict(int))
    n_mat = 0
    for mp in sorted(glob.glob(os.path.join(ROOT, "Assets", "**", "*.mat"),
                               recursive=True)):
        try:
            with open(mp, encoding="utf-8", errors="replace") as f:
                head = f.read(4096)
        except OSError:
            continue
        m = re.search(r"m_Shader:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-f]{32})",
                      head)
        if not m or m.group(1) not in info:
            continue
        n_mat += 1
        sname, decl = info[m.group(1)]
        for k in mat_keywords(mp):
            if k and k not in decl:
                a_class[sname][k] += 1

    # B 类：HLSLcc 产物显示原版有多个 Pass，而当前只有 1 个
    #
    # **只报 Pass 数，不报 blend 维度**。踩过的坑：文件名里看着有
    # `__Additive` / `__Addit` 这样的 blend 标记，我据此写了BLEND_TOKENS
    # 提取，结果一律 0 —— 查下来是**磁盘上的名字本身就是截断的**
    # （`08_p0_PS_sm40__GlobalMipBias__AlphaToMaskAvailable__Ad.glsl`），
    # 不是 `Additive`。那是当初生成产物时的命名截断，磁盘上就是 `__Ad`。
    #所以从这份产物里提 blend 维度是不可靠的，报出来只会是假结论 ——
    # 不如只报可验证的 Pass 数。
    b_class: dict[str, dict] = {}
    hlslcc = "/tmp/hlslcc_all"
    if os.path.isdir(hlslcc):
        for sdir in sorted(os.listdir(hlslcc)):
            d = os.path.join(hlslcc, sdir)
            if not os.path.isdir(d):
                continue
            passes = set()
            for f in os.listdir(d):
                m = re.match(r"\d+_p(\d+)_[A-Z]+_.*\.(glsl|dxbc)$", f)
                if m:
                    passes.add(m.group(1))
            if len(passes) > 1:
                b_class[sdir] = {"passes": len(passes)}

    data = {
        "materials_scanned": n_mat,
        "undeclared_keywords": {k: dict(v) for k, v in sorted(a_class.items())},
        "multi_variant_originals": b_class,
    }

    if a.json:
        print(json.dumps(data, ensure_ascii=False, indent=2))
    else:
        print(f"扫描 {n_mat} 个材质")
        if not a_class:
            print("\nA 类（材质 keyword 未被 shader 声明）：无")
        else:
            tot = sum(sum(v.values()) for v in a_class.values())
            print(f"\nA 类：材质勾了但 shader 没声明的 keyword（{tot} 处）")
            print("     —— 这些 keyword 被 Unity 静默忽略，材质被当成基础变体渲染")
            for sname, ks in sorted(a_class.items()):
                print(f"  {sname}")
                for k, c in sorted(ks.items(), key=lambda x: -x[1]):
                    print(f"      {k:34s} {c:3d} 个材质勾了它")
        if b_class:
            print(f"\nB 类：原版是多变体 / 多 Pass，当前只还原了一个（"
                  f"{len(b_class)} 个 shader）")
            print("     —— 一行 pragma 补不回这个，需要重建变体")
            for sname, v in sorted(b_class.items(),
                                   key=lambda x: -x[1]["passes"])[:15]:
                print(f"  {sname:36s} 原版 {v['passes']} 个 Pass，"
                      f"当前只有 1 个")
    return 0


if __name__ == "__main__":
    sys.exit(main())
