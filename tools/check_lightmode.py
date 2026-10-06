#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""检查 Pass 的 LightMode tag，并按「shader 是否消费 URP 注入的光照数据」分流。

## 背景：为什么这是个问题

`Assets/Shader/` 下41 个非空壳 shader，**一个 `LightMode` tag 都没有**
（`grep -rl LightMode Assets/Shader/` 零结果）。AssetRipper 导出时把它丢了。

URP 靠 `LightMode` 决定「哪个 Pass 在哪一步渲染」，官方文档的说法是：

> If you do not set the LightMode tag in a Pass, URP uses the SRPDefaultUnlit
> tag value for that Pass.
> —— Unity Manual, ShaderLab Pass tags in URP reference

`SRPDefaultUnlit` 也在 URP 的绘制列表里（DrawObjectsPass 的默认
ShaderTagId 是 SRPDefaultUnlit / UniversalForward / UniversalForwardOnly
三者），所以**Pass 不会凭空消失**——这点常被误读成「不填就完全不渲染」。

真正的后果在别处：**shader 自己声明的那些 uniform 谁来赋值。**

URP 只在 `UniversalForward` Pass 里注入主光与附加光的数据
（`_MainLightPosition`、`_MainLightColor`、`_AdditionalLights*`、
`unity_LightData`…）。落进 `SRPDefaultUnlit` 的 Pass 拿不到这些值 ——
它们不是「默认值」，而是**没有赋值**。于是所有基于这些量的漫反射、
高光、附加光累加全部退化成0，画面表现为**物体只剩环境色/自发光，
一片死黑或纯色**。

实测这批 shader 里有 20 个明确消费了这些量，命中数多为 10：

```
Shader Graphs_DefaultShader.shader    10   (含_AdditionalLightsAttenuation 等)
Shader Graphs_Grass.shader            10
Shader Graphs_CharacterShader.shader  10
Shader Graphs_SkyboxShader.shader      1   (_MainLightPosition 算太阳方向)
...
```

## 判据：不靠猜，靠 shader 自己说话

填`UniversalForward` 还是保持不动，取决于这个 shader 到底用不用 URP 的
光照数据 —— 这是能从源码读出来的事实，不需要猜：

- **消费了 URP 光照 uniform** -> 需要 `LightMode = "UniversalForward"`
- **一个都没用**（UI / 轮廓 / Blur / TMP / Skybox-Procedural 等）
  -> 本来就是 unlit 性质，**不动**。给它们挂 `UniversalForward` 是
  行为改动，不是移植修复。

`Shader Graphs_SkyboxShader`只命中 1 个，但性质与那 10 个相同 ——
它用 `_MainLightPosition` 算太阳方向。所以判据是**有没有消费**，不是
**命中几个**。

## 这个脚本只报，不改

补tag 看起来是机械替换，但它有两个真实风险，所以分开做：

1. **同名 Pass 复用**。`Shader Graphs_UI.shader` 用
   `Pass [_StencilOp]` 做了 stencil 分支，插入位置要按结构判断，
   不能字符串硬替换。
2. **改错了看不出来**。`SRPDefaultUnlit` 和 `UniversalForward` 在某些
   简单材质上画面几乎一样，只有开了多光的场景才暴露。属于「改了
   但没法证明改对了」—— 所以先出判决，动手前确认。

另外要说明的是：**LightMode 与 Pass 数量是两个独立问题。**
本脚本只管 LightMode。Pass 数量（原版5-6 个 vs 现在 1 个）属于
B类，涉及 ShadowCaster / DepthOnly / Meta 等 Pass 的重建，是另一件事。

## 用法

    python3 tools/check_lightmode.py
    python3 tools/check_lightmode.py --json
"""
from __future__ import annotations

import argparse
import glob
import json
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SHADERS = os.path.join(ROOT, "Assets", "Shader")

STUB_MARKER = "DummyShaderTextExporter"

# URP 在 UniversalForward Pass 里注入的光照数据。shader 声明并使用了
# 其中任何一个，就说明它依赖 URP 的光照赋值 —— 落到 SRPDefaultUnlit
# 上这些量不会被赋值，光照结果退化为 0。
URP_LIGHT_UNIFORMS = (
    "_MainLightPosition",
    "_MainLightColor",
    "_MainLightShadowParams",
    "_AdditionalLightsCount",
    "_AdditionalLightsPosition",
    "_AdditionalLightsColor",
    "_AdditionalLightsAttenuation",
    "_AdditionalLightsSpotDir",
    "_AdditionalLightsLayerMasks",
    "_AdditionalLightShadowParams",
    "unity_LightData",
)

# URP 前向路径的 tag 值。
FORWARD_TAG = "UniversalForward"

# Pass 头的匹配。ShaderLab 允许同一行多个 tag，所以这里只抓 LightMode。
LIGHTMODE_RE = re.compile(r'"LightMode"\s*=\s*"([^"]+)"')
# `Pass` / `Pass [_StencilOp]` / `Pass "Name"` 都算一个 Pass 头
PASS_HEAD_RE = re.compile(r"(?m)^[ \t]*Pass\b[^\n{]*")


def uniform_hits(raw: str) -> list[str]:
    """shader 正文里出现过的 URP 光照 uniform（去重、保持声明顺序）。"""
    return [u for u in URP_LIGHT_UNIFORMS if u in raw]


def analyse(path: str) -> dict:
    raw = open(path, encoding="utf-8", errors="replace").read()
    name = os.path.basename(path)
    hits = uniform_hits(raw)
    tags = LIGHTMODE_RE.findall(raw)
    passes = PASS_HEAD_RE.findall(raw)
    return {
        "name": name,
        "is_stub": STUB_MARKER in raw,
        "has_lightmode": bool(tags),
        "lightmode": tags,
        "n_pass": len(passes),
        "pass_heads": [p.strip() for p in passes],
        "urp_uniforms": hits,
        "needs_forward": bool(hits) and not tags,
    }


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()

    files = sorted(glob.glob(os.path.join(SHADERS, "*.shader")))
    rows = [analyse(f) for f in files]
    live = [r for r in rows if not r["is_stub"]]
    need = [r for r in live if r["needs_forward"]]
    untagged_unlit = [r for r in live
                      if not r["has_lightmode"] and not r["urp_uniforms"]]
    tagged = [r for r in live if r["has_lightmode"]]

    if a.json:
        print(json.dumps({
            "total": len(rows),
            "stub": len(rows) - len(live),
            "live": len(live),
            "needs_forward": [r["name"] for r in need],
            "untagged_unlit": [r["name"] for r in untagged_unlit],
            "already_tagged": [r["name"] for r in tagged],
        }, ensure_ascii=False, indent=2))
        return 0

    print(f"Assets/Shader 下{len(rows)} 个 shader"
          f"（空壳 {len(rows) - len(live)}，非空壳 {len(live)}）")

    if not live:
        print("无非空壳 shader —— 本节为空**不代表 LightMode 没问题**，"
              "只代表没有可检查的对象。")
        return 0

    print(f"  已有 LightMode tag: {len(tagged)}")
    print(f"  缺 tag 且消费 URP 光照数据（应补 {FORWARD_TAG}）: {len(need)}")
    print(f"  缺 tag 但不消费 URP 光照数据（unlit，应保持不动）: "
          f"{len(untagged_unlit)}")

    if need:
        print(f"\n### 缺 {FORWARD_TAG} —— 落到 SRPDefaultUnlit 则光照 uniform "
              "不被赋值")
        print("    （这些量不是「默认 0」，是**没人赋值**；结果是漫反射/"
              "高光/附加光全退化为 0）")
        for r in sorted(need, key=lambda x: -len(x["urp_uniforms"])):
            print(f"! {r['name']}")
            print(f"    消费 {len(r['urp_uniforms'])} 个: "
                  f"{', '.join(r['urp_uniforms'][:5])}"
                  f"{' …' if len(r['urp_uniforms']) > 5 else ''}"
                  f"   Pass 数 {r['n_pass']}")
        print(f"::warning::以上 {len(need)} 个需要补 LightMode。"
              "**本脚本只报不改** —— 补tag 涉及插入位置判断"
              "（如 `Pass [_StencilOp]` 的 stencil 分支），"
              "且改对改错在简单材质上看不出来。")

    if untagged_unlit:
        print("\n### 不消费 URP 光照数据 —— 保持不动")
        print("    UI / 轮廓 / Blur / TMP 这类本来就是 unlit 性质。"
              "给它们挂 UniversalForward 是行为改动，不是移植修复。")
        for r in untagged_unlit:
            print(f"  {r['name']:52s} Pass 数 {r['n_pass']}")

    # Pass 数量是独立问题，明确划界，避免本节的结论被外推
    multi = [r for r in live if r["n_pass"] > 1]
    print(f"\n### 附：Pass 数量（本脚本不判，只列事实）")
    print(f"  多 Pass 的 shader: {len(multi)} 个"
          + (f" —— {', '.join(r['name'] for r in multi)}" if multi else ""))
    print("  单 Pass 的 shader: "
          f"{len(live) - len(multi)} 个。**单 Pass 不等于错** —— "
          "缺 ShadowCaster / DepthOnly 会影响阴影与深度预通道，")
    print("  但那是 B 类（重建带分支的多 Pass shader），与 LightMode "
          "是两个独立问题，不要用本节结论去推它。")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
