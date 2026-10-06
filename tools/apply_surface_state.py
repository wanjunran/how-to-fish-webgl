#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把 .mat 里的原版表面状态机械写回 .shader 的 Pass。

这一步做什么
------------
AssetRipper 导出的 .shader 里 Pass 是个空壳：只有
`Tags { "RenderType" = "Opaque" }` + `Name "Forward"`，
Blend / ZWrite / Cull / AlphaToMask 全丢。而这些**不是着色逻辑**，
无法从反编译产物反推 —— 它们是美术在 Inspector 里选的。

但它们在 .mat 里原样存着。URP 的 Shader Graph 往材质写了一组隐藏字段
（_Surface / _Blend / _SrcBlend / _DstBlend / _ZWrite / _Cull /
_AlphaClip / _QueueOffset），AssetRipper 一个不落地导出了。所以这里的
做法是：读材质字段 -> 查表 -> 写 ShaderLab 渲染状态。

**每一条都有出处，没有推测。** 数值沿用 Unity 官方
UnityStandardUtils 的 BlendMode 枚举：
    SrcBlend: 0=Zero 1=One 5=SrcAlpha
    DstBlend: 0=Zero 1=One 2=SrcColor 10=OneMinusSrcAlpha
    Cull:     0=Off 1=Front 2=Back
    _Surface: 0=Opaque 1=Transparent
    _AlphaClip: 1 -> AlphaToMask On

一个 shader 可能被多个材质共用，而不同材质的状态可能不同
（比如同一个 shader，一个材质用 Opaque、另一个用 Transparent）。
那种情况下不做任何改动 —— 一个 .shader 只有一个 Pass，改了就会影响
所有材质。这种冲突会打印出来让人决定，不自己决定。

用法
----
    python3 tools/apply_surface_state.py --dry-run   # 只看会改什么
    python3 tools/apply_surface_state.py--apply
"""
from __future__ import annotations

import argparse
import glob
import os
import re
import sys
from collections import defaultdict

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from surface_state import (ROOT, guid_of_meta, read_mat_props,  # noqa: E402
                           shader_name_of, SRC_BLEND, DST_BLEND, CULL)

SHADER_DIR = os.path.join(ROOT, "Assets", "Shader")


def state_of(props: dict[str, str]) -> dict:
    """材质字段 -> ShaderLab 渲染状态字典。缺字段就不给那一项。"""
    def num(*names):
        for n in names:
            v = props.get(n)
            if v is None:
                continue
            try:
                return int(float(v))
            except ValueError:
                return None
        return None

    out = {}
    src, dst = num("_SrcBlend"), num("_DstBlend")
    if src is not None:
        out["src"] = SRC_BLEND.get(src, str(src))
    if dst is not None:
        out["dst"] = DST_BLEND.get(dst, str(dst))
    zw = num("_ZWrite")
    if zw is not None:
        out["zwrite"] = zw
    cull = num("_Cull", "_CullMode", "_CullModeForward")
    if cull is not None:
        out["cull"] = CULL.get(cull, str(cull))
    if num("_AlphaClip") == 1:
        out["alphatomasks"] = True

    surface = num("_Surface", "_SurfaceType")
    if surface is not None:
        out["surface"] = "Transparent" if surface == 1 else "Opaque"
    qo = num("_QueueOffset")
    if qo:
        out["queue_offset"] = qo
    return out


def render_lines(st: dict) -> list[str]:
    """状态字典 -> ShaderLab 渲染状态行。"""
    ls = []
    if "src" in st and "dst" in st and (st["src"], st["dst"]) != ("One", "Zero"):
        ls.append(f'            Blend {st["src"]} {st["dst"]}')
    if st.get("alphatomasks"):
        ls.append("            AlphaToMask On")
    if "zwrite" in st:
        ls.append(f'            ZWrite {"On" if st["zwrite"] else "Off"}')
    if "cull" in st:
        ls.append(f'            Cull {st["cull"]}')
    if "surface" in st:
        ls.append('            // RenderType: ' + st["surface"])
    return ls


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--apply", action="store_true", help="真正写文件")
    ap.add_argument("--dry-run", action="store_true")
    a = ap.parse_args()

    # guid -> (shader 名, 路径)
    #
    # 这里**不能**排除 AssetRipper 空壳。原实现写着
    # 「含 HLSLPROGRAM 且非空壳」，于是 Universal Render Pipeline/Lit
    # 空壳从来没被处理 —— 而它被 59 个材质引用，其中 11 个是透明的
    # （水面 / WaterParticle / WaterParticleWhite / SpitParticle），
    # 空壳的 Pass 里没有任何 Blend/ZWrite/Cull 块，
    # 于是「该透明的东西渲成不透明」，且不产生一行日志。
    #
    # 空壳有完整的 Properties（含 _SrcBlend/_DstBlend/_ZWrite/_Cull），
    # 缺的只是 Pass 里的渲染状态块 —— 那正是本脚本要补的东西。
    by_guid = {}
    for p in glob.glob(os.path.join(SHADER_DIR, "*.shader")):
        raw = open(p, encoding="utf-8", errors="replace").read()
        if "HLSLPROGRAM" not in raw:
            continue
        g = guid_of_meta(p)
        if g:
            by_guid[g] = (shader_name_of(p) or os.path.basename(p), p)

    # guid -> 材质
    mats = defaultdict(list)
    for mp in glob.glob(os.path.join(ROOT, "Assets", "**", "*.mat"),
                        recursive=True):
        try:
            head = open(mp, encoding="utf-8",
                        errors="replace").read(4096)
        except OSError:
            continue
        m = re.search(r"m_Shader:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-f]{32})",
                      head)
        if m and m.group(1) in by_guid:
            mats[m.group(1)].append(mp)

    changed, conflicts, skipped = 0, [], 0
    for guid, (sname, spath) in sorted(by_guid.items()):
        paths = mats.get(guid)
        if not paths:
            continue
        states = {}
        for mp in paths:
            st = state_of(read_mat_props(mp))
            # 「没有需要改的东西」= 全默认，归一化成一个状态
            key = render_lines(st)
            states.setdefault(tuple(key), []).append(
                os.path.relpath(mp, ROOT))

        default = tuple(render_lines(
            state_of({})))  # 空 props -> 不产生任何行
        real = {k: v for k, v in states.items() if k != default}

        if not real:
            skipped += 1
            continue
        if len(real) > 1:
            conflicts.append((sname, real))
            continue

        lines = next(iter(real))
        if not lines:
            skipped += 1
            continue

        raw = open(spath, encoding="utf-8", errors="replace").read()
        # 插入锚点：渲染状态块必须排在 Pass 的 `{` 之后、HLSLPROGRAM 之前。
        #
        # 两种形态都要认：
        #   本流水线恢复的产物 -> `    Name "Forward"\n{`
        #   AssetRipper 空壳   -> `Pass\n{`（**没有 Name 标记**）
        # 之前只认 Name 那种，于是空壳全部报「找不到锚点，跳过」——
        # 又是一个静默跳过（输出里只是多一行提示，容易被当成正常）。
        m = re.search(r'(Name "Forward"\n)', raw) or \
            re.search(r'(^[ \t]*Pass[ \t]*\n[ \t]*\{\n)', raw, re.M)
        if not m:
            print(f"!! {sname}: 找不到 Pass 锚点，跳过")
            skipped += 1
            continue
        block = "\n".join(lines) + "\n"
        if block.strip() and block in raw:
            print(f"== {sname}: 已含该渲染状态，无需改")
            skipped += 1
            continue
        new = raw[:m.end(1)] + block + raw[m.end(1):]
        mats_list = sorted(next(iter(real.values())))

        print(f"== {sname}")
        print(f"   依据材质: {', '.join(mats_list)}")
        for l in lines:
            print(f"   + {l.strip()}")
        changed += 1
        if a.apply and not a.dry_run:
            open(spath, "w", encoding="utf-8").write(new)

    print()
    print(f"需改动 {changed} 个，跳过（已是默认/已含）{skipped} 个，"
          f"冲突 {len(conflicts)} 个")
    if conflicts:
        print("\n冲突（同一 shader 被多个材质要求不同状态，未改动，需人工决定）:")
        for sname, real in conflicts:
            print(f"  {sname}")
            for k, ms in real.items():
                print(f"    {sorted(ms)} -> {list(k)}")
    if changed and not a.apply:
        print("\n（这是 dry-run，加 --apply 才会写文件）")
    return 0


if __name__ == "__main__":
    sys.exit(main())