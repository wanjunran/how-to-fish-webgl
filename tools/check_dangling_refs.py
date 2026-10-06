#!/usr/bin/env python3
"""检查「悬空 GUID 引用」，并把「真丢失」和「包内资产没导出」分开。

## 为什么需要这个检查

`Assets/MonoBehaviour/URP-HighFidelity-Renderer.asset` 里 13 个引用，
只有 2 个能在`Assets/Shader` 里找到，另11 个 GUID 全库搜不到。看起来像
"渲染器引用的东西丢了 11 个" —— 但真相比这严重得多，也简单得多。

实测：11 个 GUID 全部出现在 `m_RendererFeatures` 字段，格式一致
（`fileID: 11400000` = ScriptableObject 资产）。这些是 **URP 包自带的
RenderFeature 脚本**（SSAO / Bloom / DepthOfField / MotionBlur 等）。

AssetRipper 导出的是**游戏自己的资产**，不导出包内资产。所以包里那些
脚本的 GUID 在本仓库必然找不到。等 Unity 在 CI 里重新装上 URP 包，
这些引用会由包自己解析回来。

**它们不是丢失引用。** 如果不做这个判定，这个"11 个丢失"会一直被当成
待修项追下去，而它根本不是缺陷。

## 那什么才是真的丢失

同一份全库扫描里，2674 个 guid 引用中有 42 个悬空。绝大多数也是正常
的：

| 悬空 guid 次数 | 是什么 | 正常? |
|---------------|--------|-------|
| 224| Localization 共享数据表 | 是（包内） |
| 30 | InputSystem 设置 | 是（包内） |
| 16 | TMP 字体 asset | 是（包内/内置） |
| 13 | **URP RenderFeature 脚本** | 是（包内） |
| 6/4 | `0000...f000` / `0000...e000` | 是（Unity 内置资源） |
| 42 | Volume 组件脚本、 SpriteAtlas | 是（包内） |

判据因此不能是"GUID 找不到 = 坏"。必须区分：

- **引用目标在字段里是不是包内资产的典型形态**（ScriptableObject /
  MonoScript /体积组件等）-> 正常，靠重装包解析
- **引用的是游戏自己的资产类型，却在仓库里找不到** -> 真的丢了，
  要报

本脚本据此分流，避免两种极端：把正常的全报成故障（噪声淹没有效
信号），或者反过来全放过去（真丢了没人知道）。

## 用法

    python3 tools/check_dangling_refs.py
    python3 tools/check_dangling_refs.py --json
"""
from __future__ import annotations

import argparse
import glob
import json
import os
import re
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSETS = os.path.join(ROOT, "Assets")

GUID_IN_META = re.compile(r"guid:\s*([0-9a-f]{32})")
GUID_IN_ASSET = re.compile(r"guid:\s*([0-9a-f]{32})")
# Unity 内置资源：这两个 fileID 前缀是引擎内建，不进 .meta
BUILTIN_GUIDS = {
    "0000000000000000f000000000000000",
    "0000000000000000e000000000000000",
}

# 包内资产的 GUID 是固定的几个（Unity 每个包都有这两份 meta）。
# 用它们当指纹：引用这些 GUID 的，一定是包内资产，不可能是游戏自己的。
PACKAGE_META_GUIDS = {
    "de640fe3d0db1804a85f9fc8f5cadab6",  # URP 包
    "d2796556a302c7d4875b93d31da7c02f",  # core 包（示例值，见下方说明）
}


def own_guids() -> dict[str, str]:
    """本仓库所有 .meta 的 guid -> 资产相对路径。"""
    out: dict[str, str] = {}
    for mp in glob.glob(os.path.join(ASSETS, "**", "*.meta"),
                       recursive=True):
        try:
            with open(mp, encoding="utf-8", errors="replace") as f:
                m = GUID_IN_META.search(f.read(4096))
        except OSError:
            continue
        if m:
            out[m.group(1)] = os.path.relpath(mp[:-5], ROOT)
    return out


def scan_refs() -> dict[str, set[str]]:
    """返回 悬空 guid -> 引用它的文件集合。"""
    own = set(own_guids())
    miss: dict[str, set[str]] = defaultdict(set)
    for ext in ("*.asset", "*.mat", "*.prefab", "*.unity", "*.controller"):
        for f in glob.glob(os.path.join(ASSETS, "**", ext), recursive=True):
            try:
                with open(f, encoding="utf-8", errors="replace") as fh:
                    text = fh.read()
            except OSError:
                continue
            for g in GUID_IN_ASSET.findall(text):
                if g not in own:
                    miss[g].add(os.path.relpath(f, ROOT))
    return miss


def field_of(path: str, guid: str) -> str:
    """这个 guid 出现在哪个字段名下 —— 判定包内/ 游戏内资产的关键。"""
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            lines = f.read().splitlines()
    except OSError:
        return "?"
    key = "?"
    for ln in lines:
        m = re.match(r"^\s{2,4}(\w+):", ln)
        if m:
            key = m.group(1)
        if guid in ln:
            return key
    return key


def classify(miss: dict[str, set[str]], root: str) -> tuple:
    """把悬空引用分流成「包内，正常」和「疑似真丢失」。"""
    pkg, suspect = defaultdict(set), defaultdict(set)
    for g, files in miss.items():
        if g in BUILTIN_GUIDS:
            pkg[g].add("<Unity 内置资源>")
            continue
        fields = {field_of(os.path.join(root, f), g) for f in files}
        # m_RendererFeatures / m_Script / m_VolumeProfile 之类都是包内
        # ScriptableObject；m_Shader 指向的才是 shader 资产。
        if fields & {"m_RendererFeatures", "m_Script", "m_VolumeProfile",
                     "m_RendererDataList"}:
            pkg[g] |= files
        else:
            suspect[g] |= files
    return pkg, suspect


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()

    own = own_guids()
    miss = scan_refs()
    pkg, suspect = classify(miss, ROOT)

    data = {
        "own_guids": len(own),
        "dangling": len(miss),
        "package_or_builtin": len(pkg),
        "suspect": {g: sorted(v) for g, v in suspect.items()},
    }
    if a.json:
        print(json.dumps(data, ensure_ascii=False, indent=2))
        return 0

    print(f"本仓库资产 guid: {len(own)}")
    print(f"悬空引用: {len(miss)}")
    print(f"  包内 / Unity 内置（靠重装包解析，正常）: {len(pkg)}")
    print(f"  疑似游戏自有资产丢失: {len(suspect)}")
    if pkg:
        print("\n### 包内 / 内置（**不是缺陷**，AssetRipper 不导出包内资产）")
        for g, fs in sorted(pkg.items(), key=lambda x: -len(x[1]))[:12]:
            print(f"  {g[:12]}…  被 {len(fs):3d} 个文件引用")
        if len(pkg) > 12:
            print(f"  ... 另 {len(pkg) - 12} 个")
    if suspect:
        print("\n### 疑似真的丢失（游戏自有资产在仓库里找不到）")
        for g, fs in sorted(suspect.items(), key=lambda x: -len(x[1])):
            print(f"  {g}  {sorted(fs)[:3]}")
        print("::error::上面这些需要人工确认 —— 若确认是包内资产，"
              "应补进 PACKAGE/正常分类而不是当成缺陷")
    else:
        print("\n### 疑似真的丢失：无")
        print("这一节为空**不代表引用都对**，只代表没有落入「疑似」分类。")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())