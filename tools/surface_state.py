#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""从 .mat 里提取 URP 的表面状态，机械映射成 ShaderLab 渲染状态。

为什么需要这个
--------------
AssetRipper 导出的原版 .shader 里，Pass **只保留了一个空壳**
（`Tags { "RenderType" = "Opaque" }` + Name），Blend / ZWrite / Cull
这些渲染状态全部丢失。而这些不是着色逻辑、无法从反编译产物反推 ——
它们是美术在 Inspector 里选出来的。

好消息是：同一份信息在 .mat 里**原样存着**。URP 的 Shader Graph 会往
材质里写一组隐藏字段，AssetRipper 一个不落地导出了：

    _Surface        0 = Opaque, 1 = Transparent
    _SurfaceType    同上（旧字段）
    _Blend          0 = Alpha（透明）, 1 = Premultiply, 2 = Additive, 3 = Multiply
    _SrcBlend       5 = SrcAlpha, 1 = One
    _DstBlend       10 = OneMinusSrcAlpha, 0 = Zero, 2 = One
    _ZWrite         0 = 关, 1 = 开
    _Cull           0 = Off, 2 = Back（Unity 的 Cull 值）
    _CullMode       同上（Shader Graph 的命名）
    _AlphaClip      1 = 开启 alpha clip
    _QueueOffset    渲染队列偏移

数值 -> ShaderLab 关键字的映射直接沿用 Unity 官方 `UnityStandardUtils`：
`_SrcBlend` 用的就是 `BlendMode` 枚举，5=SrcAlpha、1=One；
`_DstBlend` 10=OneMinusSrcAlpha、2=One。所以可以直接查表。

怎么用
------
    python3 tools/surface_state.py                # 汇总成表
    python3 tools/surface_state.py --shader 玻璃   # 只看某个 shader

注意：这一步**只读不写**。把渲染状态写回 .shader 涉及美术判断
（该AlphaTest 还是 Blend、Cull 要不要翻面），不在这里替人决定 ——
但先把事实摆出来，让决定有依据。
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

# ---- 枚举 -> ShaderLab 关键字（沿用 Unity 官方 UnityStandardUtils）----
# 来源：UnityCsReference Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInput.hlsl
# 与 UnityStandardUtils.cginc 的 BlendMode 枚举。
BLEND_ONE = 1
BLEND_ZERO = 0
BLEND_SRC_ALPHA = 5
BLEND_ONE_MINUS_SRC_ALPHA = 10

# _Blend（Shader Graph 自己的高层枚举）-> (SrcBlend, DstBlend, ZWrite)
BLEND_MODE = {
    0: (BLEND_SRC_ALPHA, BLEND_ONE_MINUS_SRC_ALPHA, False),   # Alpha
    1: (BLEND_ONE, BLEND_ONE_MINUS_SRC_ALPHA, False),          # Premultiply
    2: (BLEND_ONE, BLEND_ONE, False),                         # Additive
    3: (BLEND_DST2 - 0 if False else 6, 0, False),            # Multiply
}

# 上表第3 行写得不严谨（Multiply 的 SrcBlend 是 DstColor=6）。
# 直接按官方值写清楚，避免留下歧义：
BLEND_MODE[3] = (6, 0, False)   # Multiply: SrcBlend=DstColor, DstBlend=Zero

SRC_BLEND = {0: "Zero", 1: "One", 2: "DstColor", 3: "SrcColor",
             4: "OneMinusDstColor", 5: "SrcAlpha", 6: "DstAlpha"}
DST_BLEND = {0: "Zero", 1: "One", 2: "SrcColor", 3: "OneMinusSrcColor",
             4: "DstAlpha", 5: "OneMinusSrcAlpha", 9: "SrcAlphaSaturate",
             10: "OneMinusSrcAlpha"}

CULL = {0: "Off", 1: "Front", 2: "Back"}

# _Surface / _SurfaceType 的取值
SURFACE = {0: "Opaque", 1: "Transparent"}


def shader_name_of(path: str) -> str | None:
    with open(path, encoding="utf-8", errors="replace") as f:
        m = re.search(r'^\s*Shader\s+"([^"]+)"', f.read(8192), re.M)
        return m.group(1) if m else None


def guid_of_meta(path: str) -> str | None:
    mp = path + ".meta"
    if not os.path.exists(mp):
        return None
    with open(mp, encoding="utf-8", errors="replace") as f:
        m = re.search(r"guid:\s*([0-9a-f]{32})", f.read())
        return m.group(1) if m else None


def read_mat_props(path: str) -> dict[str, str]:
    """读 .mat 里的标量字段（m_Floats / m_Ints / m_Colors段）。

    AssetRipper 把它们写成 `      _Name: value`（6 空格缩进、无 `- ` 前缀），
    且**不带类型前缀** —— 所以只能靠「缩进恰好 6 空格 + 冒号值」识别。
    之前踩过的坑：按 `"  - name:"` 找属性（那是 Prefab override 的写法），
    结果匹配到 0 个材质 —— 静默全绿比报错更危险。
    """
    out: dict[str, str] = {}
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            for ln in f:
                m = re.match(r"^ {6}(_?\w+):\s*(\S+)\s*$", ln)
                if m:
                    out[m.group(1)] = m.group(2)
    except OSError:
        pass
    return out


def classify(props: dict[str, str]) -> dict:
    """把材质字段归成渲染状态。"""
    def num(*names, default=None):
        for n in names:
            v = props.get(n)
            if v is not None:
                try:
                    return float(v)
                except ValueError:
                    return v
        return default

    surface = num("_Surface", "_SurfaceType", default=0)
    alpha_clip = num("_AlphaClip", default=0)
    zwrite = num("_ZWrite", default=None)
    cull = num("_Cull", "_CullMode", "_CullModeForward", default=None)
    blend = num("_Blend", default=None)
    src = num("_SrcBlend", default=None)
    dst = num("_DstBlend", default=None)
    qo = num("_QueueOffset", default=0)

    is_transparent = int(surface) == 1 if surface is not None else False
    out = {
        "surface": SURFACE.get(int(surface), str(surface)),
        "alpha_clip": bool(alpha_clip),
        "queue_offset": qo,
    }

    # 混合模式：_Blend 是高层枚举，_SrcBlend/_DstBlend 是低层枚举值。
    # 两个都有时以低层为准（更精确）。
    if blend is not None and int(blend) in BLEND_MODE:
        s, d, z = BLEND_MODE[int(blend)]
        out["src"] = SRC_BLEND.get(s, str(s))
        out["dst"] = DST_BLEND.get(d, str(d))
        out["zwrite_from_blend"] = z
    if src is not None:
        out["src_raw"] = int(src)
        out["src"] = SRC_BLEND.get(int(src), str(src))
    if dst is not None:
        out["dst_raw"] = int(dst)
        out["dst"] = DST_BLEND.get(int(dst), str(dst))
    if zwrite is not None:
        out["zwrite"] = bool(zwrite)
    elif "zwrite_from_blend" in out:
        out["zwrite"] = out.pop("zwrite_from_blend")
    if cull is not None:
        out["cull"] = CULL.get(int(cull), str(int(cull)))

    out["transparent"] = is_transparent or out.get("src") not in ("One", None)
    return out


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--shader", default=None, help="只处理名字含此串的")
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()

    shader_dir = os.path.join(ROOT, "Assets", "Shader")
    # shader 名 -> 路径（只用本流水线恢复的：含 HLSLPROGRAM 且非空壳）
    by_guid: dict[str, tuple[str, str]] = {}
    for p in glob.glob(os.path.join(shader_dir, "*.shader")):
        raw = open(p, encoding="utf-8", errors="replace").read()
        if "HLSLPROGRAM" not in raw or "DummyShaderTextExporter" in raw:
            continue
        g = guid_of_meta(p)
        if g:
            by_guid[g] = (shader_name_of(p) or os.path.basename(p), p)

    # guid -> 材质列表
    mats: dict[str, list[str]] = defaultdict(list)
    for mp in glob.glob(os.path.join(ROOT, "Assets", "**", "*.mat"),
                        recursive=True):
        try:
            with open(mp, encoding="utf-8", errors="replace") as f:
                head = f.read(4096)
        except OSError:
            continue
        m = re.search(r"m_Shader:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-f]{32})",
                      head)
        if m:
            mats[m.group(1)].append(mp)

    rows = []
    for guid, (sname, spath) in sorted(by_guid.items()):
        if a.shader and a.shader not in sname:
            continue
        for mp in mats.get(guid, []):
            props = read_mat_props(mp)
            st = classify(props)
            rows.append({
                "shader": sname,
                "material": os.path.relpath(mp, ROOT),
                **st,
            })

    if a.json:
        print(json.dumps(rows, ensure_ascii=False, indent=2))
        return 0

    if not rows:
        print("没有匹配的材质")
        return 0

    # 汇总：按 (transparent, alpha_clip, src, dst, zwrite, cull) 分组
    groups: dict[tuple, list[str]] = defaultdict(list)
    for r in rows:
        key = (r["transparent"], r["alpha_clip"], r.get("src"),
               r.get("dst"), r.get("zwrite"), r.get("cull"))
        groups[key].append(r["material"])

    print(f"共{len(rows)} 个材质引用 {len(by_guid)} 个已恢复 shader\n")
    print(f"{'透明':4} {'clip':5} {'Src':14} {'Dst':16} "
          f"{'ZWrite':7} {'Cull':5} 材质数")
    print("-" * 78)
    for key in sorted(groups, key=lambda k: (not k[0], k[1], str(k[2]))):
        tr, ac, src, dst, zw, cull = key
        print(f"{'是' if tr else '否':4} {str(ac):5} {str(src):14} "
              f"{str(dst):16} {str(zw):7} {str(cull):5} {len(groups[key])}")
        if tr or ac:
            for m in sorted(groups[key])[:6]:
                print(f"        {m}")
            if len(groups[key]) > 6:
                print(f"        ... 另 {len(groups[key]) - 6} 个")
    return 0


if __name__ == "__main__":
    sys.exit(main())