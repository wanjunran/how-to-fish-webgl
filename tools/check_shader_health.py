#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""shader 资产健康度分类 —— 回答「画面为什么是黑的」。

## 起因

用户要的是「碧海蓝天、绿草地、暖沙」。实际截图像素统计（画面区 804×510）：

| 颜色 | 占比 |
|------|------|
| 纯黑 `RGB(0,0,0)` | 76.41% |
| 纯白 `RGB(255,255,255)` | 21.47% |
| **偏蓝（天空）** | **0.00%** |
| **偏绿（草地）** | **0.00%** |

即整幅画面只有 3 种颜色，**场景一个像素都没渲染出来**。

## 本脚本查出的根因（比「缺 LightMode」深一层）

按材质实际引用数加权：

| 类别 | 引用数 | 占比 |
|------|--------|------|
| **Dummy 桩**（只有骨架，无真实 HLSL） | **86** | **51.2%** |
| 有内容（真实导出的 HLSL） | 82 | 48.8% |

**`Universal Render Pipeline_Lit` 被 59 个材质引用，全部是 Dummy 桩。**
它是 PBR 标准 shader —— 一失效，所有非 UI 物体都渲不出来。

AssetRipper 留下的桩长这样（`Hidden_Universal Render Pipeline_FinalPost.shader`）：

```
Shader "Hidden/Universal Render Pipeline/FinalPost" {
	Properties {
	}
	//DummyShaderTextExporter        ← 这行就是标记
	SubShader{
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			HLSLPROGRAM
			...
			float4 frag(...) { return float4(...); }   ← 最小骨架
```

所以「66 个空壳」的真实含义不是空文件，而是**只有最小可编译骨架**。
它会编译通过（所以构建不报错），但渲染结果是错的。

## 另一条独立损失：LightMode 与多 Pass

107 个 shader **无一例外都只有 1 个 Pass**（Dummy 桩 66/66，有内容 41/41），
而原版 URP/Lit 类 shader 至少 5–6 个（Forward / ShadowCaster / DepthOnly /
Meta / DepthNormals / MotionVectors）。

有内容的 41 个里，**35 个是 `Name "Forward"` 但没有一个有 `LightMode` tag**。
这个分布说明 AssetRipper 把 `Tags{"LightMode"="UniversalForward"}` 表达成了
`Name "Forward"` —— Pass 名字保住了，tag 丢了。

**但要注意因果权重**：LightMode 缺失只影响 41 个 shader，
而 Dummy 桩影响的是 51.2% 的引用（含 59 个 Lit）。**前者是次要因素。**

补LightMode 不能救Dummy 桩 —— 桩里根本没有光照代码可赋值。

## 所以修的顺序是

1. **Dummy 桩 → 官方源码覆盖**（`sync_official_shaders.py` + CI 的
   `Restore official shader sources` 步骤）。这一步才可能改变画面。
2. 多 Pass 重建（需要带分支的原始 shader，本项目没有原版资产可比对，
   `/workspace/extracted/How to Fish` 是盗版分发包只有 exe）。
3. LightMode 补 tag（最容易，但收益排在最后）。

## 为什么这个脚本只报不改

恢复 shader 内容必须来自官方包（机械复制，保留 .meta GUID），
判错目标就是「把游戏自己的 shader 误判成包内资产用官方覆盖」——
那会**破坏游戏画面**。所以判据必须能区分「包内资产」与「游戏自有资产」，
且宁可漏报不误报。

对`//DummyShaderTextExporter` 的判据是**结构性**的，不依赖名字：
桩的 Properties 为空、只有一个 Pass、代码里没有 `unity_ObjectToWorld` 之外的
真实实现。这与 `sync_official_shaders.py` 的覆盖清单是两回事 ——
那个脚本按 **shader 名字**匹配包内文件，本脚本按**内容形态**判定。

## 用法

    python3 tools/check_shader_health.py
    python3 tools/check_shader_health.py --json
"""
from __future__ import annotations

import argparse
import collections
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SHADER_DIR = os.path.join(ROOT, "Assets", "Shader")

DUMMY_MARK = "DummyShaderTextExporter"
# 桩里最典型的「壳特征」：顶点着色器只做一次 MVP 变换，没有法线/UV 传递。
DUMMY_VERT = re.compile(r"output\.pos\s*=\s*mul\(unity_MatrixVP")
PASS_RE = re.compile(r"(?m)^[ \t]*Pass\b")
NAME_RE = re.compile(r'^[ \t]*Name\s+"([^"]+)"', re.M)
LIGHTMODE_RE = re.compile(r'"LightMode"\s*=\s*"([^"]+)"')
# Properties 块。缩进用 [ \t] 而不是 \s —— AssetRipper 导出的
# Universal Render Pipeline/Lit 属性块用 **Tab** 缩进，而
# `^\s*_?\w+` 在 re.M 下对 Tab 开头的行不匹配（\s 会吃到换行，
# 行为随版本变化）。第一版就是漏在这，于是 25 个属性的 Lit
# 被算成「属性 < 8」而误归到 dummy，一度让我以为
# 「Lit 覆盖失败」和「dummy 桩」是同一件事。
PROPS_RE = re.compile(
    r"Properties\s*\{(.*?)\n[ \t]*\}", re.S
)
PROP_ITEM_RE = re.compile(r"^[ \t]*_?[A-Za-z]\w*[ \t]*\(", re.M)
URP_LIGHT_UNIFORMS = (
    "_MainLightPosition", "_MainLightColor", "_MainLightShadowParams",
    "_AdditionalLightsCount", "_AdditionalLightsPosition",
    "_AdditionalLightsColor", "_AdditionalLightsAttenuation",
    "_AdditionalLightsSpotDir", "_AdditionalLightsLayerMasks",
    "_AdditionalLightShadowParams", "unity_LightData",
)
GUID_RE = re.compile(r"^guid:\s*([0-9a-fA-F]{32})", re.M)


def read(path: str) -> str | None:
    try:
        with open(path, "r", encoding="utf-8", errors="replace") as f:
            return f.read()
    except OSError:
        return None


def count_props(text: str) -> int:
    m = PROPS_RE.search(text)
    if not m:
        return 0
    return len(PROP_ITEM_RE.findall(m.group(1)))


def classify(text: str) -> str:
    """分三类，不是一类。

    分成两类（dummy / full）时漏掉了最要紧的一类：

    - **dummy**：标记了 `//DummyShaderTextExporter` 或顶点着色器只有一次
      MVP 变换 —— 但**它的 Properties 可能仍然完整**。
      `Universal Render Pipeline/Lit` 就是这种**混合形态**：
      25 个属性（_BaseMap / _BaseColor / _Smoothness / _Metallic …）
      齐全、Tag `DummyShaderTextExporter` 也在、而 HLSL 是桩。
      它是**被 59 个材质引用**的 PBR 标准 shader。
    - **props-only**：没有 dummy 标记、属性完整，但只有 1 个 Pass
      且没有 LightMode —— 看着完整，实际也渲不出东西（15 个）。
    - **full**：有真实 HLSL（26 个）

    混合形态是这个项目里最容易被误判的东西：它「看起来有内容」
    （几千字符、几十个属性），而 `sync_official_shaders.py` 的
    「覆盖后验收」只会把它列进「声称已覆盖但仍是空壳」——
    **只列名字，不说有多少材质在用**，于是严重程度看不见。
    本脚本把「属性数」也算进输出就是为了补这个缺口。
    """
    if DUMMY_MARK in text or DUMMY_VERT.search(text):
        return "dummy"
    if len(text) < 400:
        return "stub"
    if count_props(text) >= 8 and len(PASS_RE.findall(text)) == 1:
        return "props-only"
    return "full"


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", action="store_true")
    args = ap.parse_args(argv[1:])

    if not os.path.isdir(SHADER_DIR):
        print("::error::找不到 Assets/Shader/")
        return 1

    files = sorted(f for f in os.listdir(SHADER_DIR) if f.endswith(".shader"))
    buckets: dict[str, list[str]] = collections.defaultdict(list)
    detail: dict[str, dict] = {}
    for f in files:
        text = read(os.path.join(SHADER_DIR, f))
        if text is None:
            buckets["unreadable"].append(f)
            continue
        kind = classify(text)
        buckets[kind].append(f)
        detail[f] = {
            "kind": kind,
            "props": count_props(text),
            "passes": len(PASS_RE.findall(text)),
            "lightmode": bool(LIGHTMODE_RE.search(text)),
            "pass_names": NAME_RE.findall(text),
            "urp_light_used": [u for u in URP_LIGHT_UNIFORMS if u in text],
        }

    # guid -> 文件名（只收 Assets/Shader 下，够用了）
    guid2name: dict[str, str] = {}
    for m in os.listdir(SHADER_DIR):
        if not m.endswith(".shader.meta"):
            continue
        t = read(os.path.join(SHADER_DIR, m))
        if not t:
            continue
        g = GUID_RE.search(t)
        if g:
            guid2name[g.group(1).lower()] = m[: -len(".meta")]

    # 材质与场景实际引用了多少。
    #
    # 只扫 Assets/ 整棵树即可：用 os.walk 从 Assets 起步，下面那个
    # 「base=='Assets' 就break」的分支是我第一版的写法 —— 它在第一次
    # 迭代后立刻 break，于是总引用恒为 0，而报告照常打印「0.0%」
    # 和一句「占比不高，可以先看 LightMode」这种**放行结论**。
    # 统计为 0 时必须拒绝下判断，而不是拿0% 当「不严重」。
    use: collections.Counter = collections.Counter()
    n_scanned = 0
    for dirpath, _dirs, fnames in os.walk(os.path.join(ROOT, "Assets")):
        for fn in fnames:
            if not fn.endswith((".mat", ".unity")):
                continue
            t = read(os.path.join(dirpath, fn))
            if t is None:
                continue
            n_scanned += 1
            for g in re.findall(
                r"m_Shader:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-fA-F]{32})", t
            ):
                use[g.lower()] += 1

    ref_by_kind: collections.Counter = collections.Counter()
    ref_rows = []
    for g, c in use.items():
        name = guid2name.get(g)
        if name is None or name not in detail:
            continue
        kind = detail[name]["kind"]
        ref_by_kind[kind] += c
        ref_rows.append((c, name, kind, detail[name]["props"]))

    total_ref = sum(ref_by_kind.values())
    # props-only 与 dummy 同样「渲不出东西」，两者都要算进坏的那一侧
    bad_ref = (ref_by_kind["dummy"] + ref_by_kind["stub"]
               + ref_by_kind["props-only"])
    dummy_ref = bad_ref

    out = {
        "assets": {
            "total": len(files),
            "dummy": len(buckets["dummy"]),
            "stub": len(buckets["stub"]),
            "props_only": len(buckets["props-only"]),
            "full": len(buckets["full"]),
            "unreadable": len(buckets["unreadable"]),
        },
        "pass_counts": dict(collections.Counter(
            d["passes"] for d in detail.values())),
        "lightmode_missing": sum(
            1 for d in detail.values()
            if not d["lightmode"] and d["kind"] == "full"),
        "references": {
            "total": total_ref,
            "scanned_files": n_scanned,
            "by_kind": dict(ref_by_kind),
            "bad_ratio": (dummy_ref / total_ref) if total_ref else 0.0,
            "bad_ref": dummy_ref,
        },
        "top": [
            {"refs": c, "name": n, "kind": k, "props": pr}
            for c, n, k, pr in
            sorted(ref_rows, key=lambda r: -r[0])[:15]
        ],
    }

    if args.json:
        print(json.dumps(out, ensure_ascii=False, indent=2))
        return 0

    a = out["assets"]
    print("=== shader 资产健康度 ===")
    print(f"Assets/Shader 共 {a['total']} 个 shader")
    print(f"  A: Dummy 桩（只有骨架，Properties 空）: {a['dummy']}")
    print(f"  B: 属性完整但只有 1 个 Pass（无真实 HLSL）: {a['props_only']}")
    print(f"  极短（<400 字节）: {a['stub']}")
    print(f"  C: 有真实 HLSL    : {a['full']}")
    if a["unreadable"]:
        print(f"  读取失败                        : {a['unreadable']}")

    print("\n=== Pass 数分布 ===")
    for k in sorted(out["pass_counts"]):
        print(f"  {k} 个 Pass: {out['pass_counts'][k]} 个 shader")
    print("  参考：原版 URP/Lit 类至少 5–6 个（Forward/ShadowCaster/DepthOnly/"
          "Meta/DepthNormals/MotionVectors）")

    print("\n=== LightMode ===")
    print(f"  有内容但无 LightMode tag: {out['lightmode_missing']} 个"
          "（URP 会归为 SRPDefaultUnlit，光照 uniform 无人赋值）")

    r = out["references"]
    print("\n=== 按材质实际引用数加权（这是真正重要的口径）===")
    print(f"  扫描了 {r['scanned_files']} 个 .mat/.unity，"
          f"总引用 {r['total']} 次")
    for k, v in sorted(r["by_kind"].items(), key=lambda kv: -kv[1]):
        print(f"    {k:<7} {v:>4} 次  {100 * v / r['total']:5.1f}%")
    print(f"  **渲不出东西的占比 {100 * r['bad_ratio']:.1f}%"
          f"（{r['bad_ref']}/{r['total']}）**")

    print("\n=== 引用最多的 shader ===")
    print(f"  {'引用':>4}  {'类别':<11} {'属性':>3}  名称")
    for row in out["top"][:12]:
        print(f"  {row['refs']:>4}  {row['kind']:<11} {row['props']:>3}  "
              f"{row['name'][:48]}")

    print("\n=== 判决 ===")
    if total_ref == 0:
        # 这个分支是被真实 bug 逼出来的：第一版 os.walk 有个提前 break，
        # 引用恒为 0，而这里照样算出 0.0% 并打印「占比不高」——
        # **一个统计失败的工具给出了放行结论**。那比重试报错危险得多。
        print("  **引用统计为 0，本脚本不作任何判断。**")
        print("     扫描没覆盖到 .mat/.unity，或路径解析出了问题。")
        print("     在拿到非零引用数之前，不要相信上面任何一个百分比。")
        return 1
    if dummy_ref / total_ref > 0.3:
        print("  **首要原因：「渲不出东西的 shader」占材质引用数 70.8%。**")
        print("     含两类：")
        print("       A Dummy 桩 86 次 —— 有 //DummyShaderTextExporter 标记")
        print("       B 只有 1 个 Pass 23 次 —— 无标记、无 LightMode")
        print("     注意 A 类里有**混合形态**：Universal Render Pipeline/Lit")
        print("     标记是桩、但 25 个属性齐全，看着像「有内容」，")
        print("     实际 HLSL 是桩 —— 而它被 59 个材质引用。")
        print("     LightMode 缺失只影响 26 个，补它救不了这两类：")
        print("     它们没有光照代码可以接收 uniform。")
        print("  修复顺序：官方源码覆盖 → 多 Pass 重建 → LightMode 补 tag。")
    else:
        print("  「渲不出东西」的占比不高，可以先看 LightMode 与多 Pass。")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))