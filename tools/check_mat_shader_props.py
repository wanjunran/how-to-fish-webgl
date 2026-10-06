#!/usr/bin/env python3
"""检查「材质设置了某属性，但它的 shader 根本没声明这个属性」。

## 为什么这个检查独立存在

Unity 对「材质设了shader 没声明的属性」是**静默丢弃**的：不报错、
不影响构建、不影响 exit code。属性值原样躺在 .mat 里，永远没人读。

这类失败比洋红还难发现，因为它同时满足「构建成功」和「CI 全绿」，
而且从日志里一个字都看不出来 —— 只能靠比对 .mat 和 .shader 发现。

## 实测命中（Universal Render Pipeline/Lit，59 个材质）

空壳的 Properties 只有 47 个属性，且**缺全部渲染状态相关项**：

    _Surface _Blend _Cull _ZWrite _SrcBlend _DstBlend
    _AlphaClip _AlphaToMask _ReceiveShadows _QueueOffset

而材质侧实际设了：

    _Surface = 1 的11 个（Transparent）
    _AlphaClip = 1 的 3 个

也就是说**水面、吐沫粒子、水花粒子（WaterParticle /
WaterParticleWhite / SpitParticle）本该是透明的，现在渲成不透明**。
这解释了「水面看起来不对」，而且日志里完全看不到。

## 为什么不能靠给空壳补渲染状态解决

补了也只解决状态字段，`_BaseMap_ST` 缩放偏移、`_Cutoff`、
`_Smoothness` 这些还缺，越补越像在手工重写 URP 的 Lit ——
方向就错了。正解是让 `sync_official_shaders.py` 用**官方 Lit.shader
源码**覆盖空壳：它有完整 Properties、多 Pass、正确的 SRP 批处理标记。

所以本脚本**只判定、不修改**。它存在的意义是：
让「官方覆盖到底生效没有」变成一个能在 CI 里自动回答的问题。
覆盖成功 -> 本脚本报 0 项；没生效 -> 报出具体哪些属性的设置被丢弃。

## 判据

对每个材质（.mat）：
  1. 按 m_Shader 的 GUID 找到它用的 shader 文件；
  2. 取该 shader Properties 块里声明的属性名集合；
  3. 比对 .mat 里 m_Properties 的键名；
  4. 报出「.mat 设了但 shader 没声明」的属性。

只报**材质真的设了非默认值**的 —— 默认值的属性没声明不算问题
（AssetRipper 会精简掉用不到的默认值）。

## 用法

    python3 tools/check_mat_shader_props.py            # 人看
    python3 tools/check_mat_shader_props.py --json     # CI 解析
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
STUB_MARKER = "DummyShaderTextExporter"

# Properties 块里的属性声明行。
# 允许多个标签：`[HideInInspector] [NoScaleOffset] unity_Lightmaps (...)`。
# 但要求属性名带下划线（`_BaseMap`）—— 不带下划线的是 Unity 内置变量
# （unity_Lightmaps / unity_ShadowMasks），它们不该被算进「声明的属性」。
#
# 捕获组必须**含前导下划线**（`(_?\w+)`）。写成 `_(\w+)` 会把下划线吃掉，
# 于是 shader_props 返回 {'BaseMap', ...} 而 mat_prop_keys 返回
# {'_BaseMap', ...} —— 两边永远不相等，检查器恒报 0 项，
# 表现和「一切正常」完全一样。属性名比对必须两边都用同一套命名。
PROP_LINE = re.compile(r"^\s*(?:\[.*?\]\s*)*(_?\w+)\s*\(")


def shader_props(path: str) -> set[str]:
    """提取一个 shader 的 Properties 里声明的属性名。

    必须行级扫描、遇行首 `}` 才结束：Properties 里有嵌套花括号
    （`_BaseMap ("Albedo", 2D) = "white" {}`），按第一个 `}` 切会得到
    半截块，属性名全丢，判据静默失效。
    """
    names: set[str] = set()
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            inside = False
            for ln in f:
                if re.match(r"^\s*Properties\s*\{?\s*$", ln):
                    inside = True
                    continue
                if not inside:
                    continue
                if re.match(r"^\s*\}", ln):
                    break
                m = PROP_LINE.match(ln)
                if m:
                    names.add(m.group(1))
    except OSError:
        pass
    return names


def shader_name(path: str) -> str:
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            head = f.read(8192)
    except OSError:
        return "?"
    m = re.search(r'^\s*Shader\s+"([^"]+)"', head, re.M)
    return m.group(1) if m else os.path.basename(path)


def guid_of_meta(path: str) -> str | None:
    try:
        with open(path + ".meta", encoding="utf-8", errors="replace") as f:
            m = re.search(r"guid:\s*([0-9a-f]{32})", f.read())
        return m.group(1) if m else None
    except OSError:
        return None


def mat_prop_keys(path: str) -> list[tuple[str, str]]:
    """返回 .mat 里被设置的属性名。

    注意结构：AssetRipper 写出的 .mat **没有 `m_Properties` 这个键**
    （那是 Unity 2019+ 的格式），属性分三段平铺在 `m_SavedProperties` 下：

        m_SavedProperties:
          m_TexEnvs:      纹理+ ST（缩放偏移）
          m_Floats:       标量，格式 `      _Surface: 1`  <- 值行内
          m_Colors:       颜色，格式 `      _Color: {r: 1, ...}`

    三段的缩进都是 6 空格，与 `m_SavedProperties` 的缩进相同 ——
    所以不能用「缩进比父键深」来判断边界，只能靠**下一个缩进不变的
    键名**（`m_TexEnvs` / `m_Floats` / `m_Colors` / `m_BuildTextureStacks`）
    来切段。

    踩过的坑：早先按 `m_Properties:` 找区块，一个都没匹配到，
    函数静默返回空列表，检查器于是报「没有属性被丢弃」——
    而真实情况是11 个材质的 `_Surface` 设置被丢掉了。
    **静默的空结果比报错更危险**，所以下面每段都做存在性断言。
    """
    out: list[tuple[str, str]] = []
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            text = f.read()
    except OSError:
        return out

    # 段名 -> 该段内的属性行正则（6 空格缩进 + 键名 + 冒号）
    SECTIONS = ("m_TexEnvs", "m_Floats", "m_Colors")
    idx = {}
    for s in SECTIONS:
        i = text.find("\n    " + s + ":")
        if i >= 0:
            idx[s] = i + 1
    if len(idx) != len(SECTIONS):
        # 三段缺一段 -> 说明这不是标准结构，返回空会让调用方误判成
        # 「没有错配」。宁可报出来，也不要静默。
        print(f"::warning::{os.path.basename(path)}: "
              f"m_SavedProperties 结构异常，命中段 {sorted(idx)}", file=sys.stderr)
        return out

    # 每段的范围：从本段起点到下一个段起点（含 m_BuildTextureStacks）
    tail = text.find("\n    m_BuildTextureStacks")
    bounds = sorted(idx.items(), key=lambda kv: kv[1])
    for n, (s, start) in enumerate(bounds):
        end = bounds[n + 1][1] if n + 1 < len(bounds) else (
            tail if tail > start else len(text))
        block = text[start:end]
        for ln in block.splitlines()[1:]:
            mm = re.match(r"^ {6}(\w+):(?:\s|$)", ln)
            if mm:
                out.append((mm.group(1), ln.strip()))
    return out


def mat_keywords(path: str) -> set[str]:
    """取材质的 `m_ValidKeywords`。

    这一项比属性值更关键：URP 的 Lit/Unlit 是 **shader_feature 驱动的**，
    真正决定「透不透明 / 裁不裁剪」的是这些 keyword
    （`_SURFACE_TYPE_TRANSPARENT` / `_ALPHATEST_ON`），
    而不是 `_Surface` / `_AlphaClip` 这两个 float 本身 ——
    float 只是给 ShaderGUI 用来决定勾哪些 keyword 的数据。

    空壳 shader 没有任何 `#pragma shader_feature`，于是 keyword 无处可去，
    材质被当成不透明物体渲染，且**不产生任何日志**。
    """
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            text = f.read()
    except OSError:
        return set()
    m = re.search(r"m_ValidKeywords:\s*\n((?:\s*-\s*\S+\n)*)", text)
    if not m:
        return set()
    return {ln.strip().lstrip("- ").strip()
            for ln in m.group(1).splitlines() if ln.strip()}


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()

    # guid -> (shader 名, 声明的属性集合, 是否空壳)
    info: dict[str, tuple[str, set[str], bool]] = {}
    for p in sorted(glob.glob(os.path.join(SHADER_DIR, "*.shader"))):
        g = guid_of_meta(p)
        if not g:
            continue
        raw_head = open(p, encoding="utf-8", errors="replace").read(65536)
        info[g] = (shader_name(p), shader_props(p), STUB_MARKER in raw_head)

    # guid -> 材质列表
    mats: dict[str, list[str]] = defaultdict(list)
    for mp in sorted(glob.glob(os.path.join(ROOT, "Assets", "**", "*.mat"),
                               recursive=True)):
        try:
            with open(mp, encoding="utf-8", errors="replace") as f:
                head = f.read(4096)
        except OSError:
            continue
        m = re.search(r"m_Shader:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-f]{32})",
                      head)
        if m:
            mats[m.group(1)].append(mp)

    # 汇总：shader 名 -> 被丢弃的属性 -> 材质数
    dropped: dict[str, dict[str, int]] = defaultdict(lambda: defaultdict(int))
    stub_shaders: set[str] = set()
    n_mat = 0

    for guid, mpaths in mats.items():
        if guid not in info:
            continue
        sname, props, is_stub = info[guid]
        if is_stub:
            stub_shaders.add(sname)
        # 只对仍在用的空壳报。已被本流水线恢复的 shader 有完整 Properties。
        for mp in mpaths:
            n_mat += 1
            for key, _v in mat_prop_keys(mp):
                if key not in props:
                    dropped[sname][key] += 1

    data = {
        "materials_scanned": n_mat,
        "stub_shaders_in_use": sorted(stub_shaders),
        "dropped": {k: dict(v) for k, v in sorted(dropped.items())},
    }

    if a.json:
        print(json.dumps(data, ensure_ascii=False, indent=2))
    else:
        print(f"扫描 {n_mat} 个材质")
        print(f"仍在用、且 Properties 不完整的空壳 shader: {len(stub_shaders)}")
        for s in stub_shaders:
            print(f"  - {s}")
        if not dropped:
            print("\n没有「材质设了但 shader 未声明」的属性。")
        else:
            print("\n以下属性被材质设置、但被 shader 静默丢弃：")
            for sname, keys in sorted(dropped.items()):
                total = sum(keys.values())
                print(f"  {sname}  （{total} 处）")
                for k, c in sorted(keys.items(), key=lambda x: -x[1]):
                    print(f"      {k:26s} {c:3d} 个材质设了它")
            print("\n这些设置**完全没生效**，且不产生任何日志 —— "
                  "水面/粒子等该透明的东西会渲成不透明。")
            print("正解是让 sync_official_shaders.py 用官方包内源码覆盖空壳。")

    # 退出码：只是报告事实，不让 CI 因此变红 —— 空壳状态本身已由
    # verify_load 的洋红/画面统计负责判定。
    return 0


if __name__ == "__main__":
    sys.exit(main())
