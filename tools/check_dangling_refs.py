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

## 但上面那个分流有个盲区，它已经栽过实打实的一次

把所有 `m_Script` 悬空都归进「包内，正常」，对**包内脚本**是对的。
可 DLL 里的 MonoBehaviour 走的也是 `m_Script` —— 它指向那个 DLL 自己的
.meta guid。**一旦有人在重建或改名 .meta 时换掉了 guid，它就成了悬空
引用，而这条分类会把它当成包内资产放过去。**

本项目的实例（`FishyUnityTransport.dll`）：

| 提交 | meta guid | 场景/prefab 引用的 guid | 状态 |
|------|-----------|------------------------|------|
| `d3eab9a`..`5949eae` | `d8db7e6e…` | `d8db7e6e…` | 一致 |
| `d2ac6c2` | *（文件被删）* | `d8db7e6e…` | 悬空 |
| `d414ac3` | `3f0a1c8e…` | `d8db7e6e…` | **永久悬空** |

`d414ac3` 把这个 DLL 改名回 `FishyUnityTransport`（为了避开与
`com.unity.transport` 的程序集同名冲突），顺手新建了一份 .meta 并给了
**新 guid**。但 Unity 的 .meta 里存的是资产身份：**改文件名不等于改身份，
换 guid 就等于所有既有引用全部作废。** 于是场景和 `NetworkManager.prefab`
里 4 处 `UnityTransport` 组件引用全部悬空，而本脚本当时报的是
「疑似真的丢失: 0 个」。

后果不是"少一个资源"这么轻。`_transports` 列表第二项在Unity 里变成
missing MonoBehaviour（null），`Multipass.Initialize()` 会移除 null 项
并只`LogWarning` —— WebGL 上就只剩走 UDP 的 Tugboat，而浏览器没有 UDP。
接着 `SetClientTransport<UnityTransport>()` 走到 `IndexInRange(-1)`，
`LogError` 之后**静默返回**（`Multipass.cs:597-603`），`ClientTransport`
落回 getter 的自动兜底（`Multipass.cs:66-78`）。整条进场景链路在一个
LogError 之后继续跑，看起来像"随机失败"。

所以补了**判据二**：数一遍所有 DLL 插件 meta 的 guid 引用次数，零引用的
挑出来，再拿 git 历史里该 .meta 曾用过的 guid 逐一比对 —— 只要有悬空
guid 命中历史身份，就是身份漂移的实锤。

零引用本身**不算错**：纯托管库（`Newtonsoft.Json`、
`FishNet.CodeAnalysis*`）只需 `using`，不需要资产引用。所以判据必须两段
才敢报警：零引用 **且** 悬空 guid 命中历史。历史拿不到时（CI 浅克隆）
退化为只报零引用，不硬判 —— 宁可少报也不误报。

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


# ===== 判据二：插件 DLL 的 guid 零引用 = 身份漂移 =====
#
# 上面那个分类有个盲区，代价是本项目实打实栽过一次：
# 它把**所有** m_Script 悬空一律归为"包内，正常"。而DLL 里导出的
# MonoBehaviour，其 m_Script 指向的guid 属于那个 DLL 的 .meta —— 一旦
# 有人在重建/改名 .meta 时换掉了guid，它就成了悬空引用，而分类把它当
# 成包内资产放了过去。
#
# 实测经过：Assets/Plugins/FishyUnityTransport.dll 在 d3eab9a..5949eae
# 期间 meta guid 是 d8db7e6e61692c478fb9733936ad93bb，场景与
# NetworkManager.prefab 里 4 处 UnityTransport 组件引用它。d2ac6c2 删掉
# 该文件，d414ac3 重建时用了新 guid 3f0a1c8e...，于是那 4 处引用全部
# 悬空。分类把它们判成"包内，正常"，报告里"疑似真的丢失"是 0 个。
#
# 后果不是"某个资源加载不出来"这么轻：_transports 列表第二项在Unity 里
# 变成 missing MonoBehaviour（null），Multipass.Initialize() 会移除 null
# 项并只 LogWarning —— 于是在 WebGL 上只剩 UDP 的 Tugboat 可用，
# SetClientTransport<UnityTransport>() 走到 IndexInRange(-1)，LogError
# 之后**静默返回**，ClientTransport 落回 getter 的自动兜底。整条联机/
# 进场景链路在一个 LogError 之后继续跑，看起来像"随机失败"。
#
# 判据：把仓库里所有 .meta 的 guid 数一遍，找出**零引用的 DLL/程序集
# meta**。DLL 被人引用时靠 guid 定位，没被引用就说明要么没人用（正常，
# 比如纯托管库 Newtonsoft），要么引用方拿的是另一个 guid（异常）。
# 再拿 git 历史里该 meta 的历次 blob 逐一核对：只要历史里出现过一个
# 「当前悬空引用正在用」的 guid，就是身份漂移，实锤。
DLL_META_SUFFIX = ".dll.meta"


def plugin_meta_guids() -> dict[str, str]:
    """Assets 下所有 DLL 插件的 meta guid -> dll 路径。"""
    out = {}
    for mp in glob.glob(os.path.join(ASSETS, "**", "*" + DLL_META_SUFFIX),
                        recursive=True):
        try:
            with open(mp, encoding="utf-8", errors="replace") as f:
                m = GUID_IN_META.search(f.read(4096))
        except OSError:
            continue
        if m:
            out[m.group(1)] = os.path.relpath(mp[:-len(DLL_META_SUFFIX)], ROOT)
    return out


def guid_refcounts(root: str) -> dict[str, int]:
    """资产文件里每个 guid 被引用多少次。"""
    counts: dict[str, int] = defaultdict(int)
    for ext in ("*.asset", "*.mat", "*.prefab", "*.unity", "*.controller"):
        for f in glob.glob(os.path.join(ASSETS, "**", ext), recursive=True):
            try:
                with open(f, encoding="utf-8", errors="replace") as fh:
                    text = fh.read()
            except OSError:
                continue
            for g in GUID_IN_ASSET.findall(text):
                counts[g] += 1
    return counts


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

    # ---- 判据二：插件 DLL 身份漂移 ----
    counts = guid_refcounts(ROOT)
    dlls = plugin_meta_guids()
    orphans = {g: p for g, p in dlls.items() if counts.get(g, 0) == 0}
    print("\n### 插件 DLL 引用情况")
    if not dlls:
        print("  无 DLL 插件，本节跳过（不代表没有 DLL 被打包进别处）。")
    else:
        for g, p in sorted(dlls.items(), key=lambda x: -counts.get(x[0], 0)):
            n = counts.get(g, 0)
            tag = "  <- 零引用" if n == 0 else ""
            print(f"  {p:48s} {g}被引用 {n:4d} 次{tag}")
        if orphans:
            # 不直接判成缺陷：纯托管库（Newtonsoft.Json）本来就没人引用。
            # 但只要**有一个悬空 guid 出现在这些 DLL 的 git 历史 guid 里**，
            # 就是身份漂移的实锤 —— 场景在找旧身份，DLL 挂在新身份上。
            hist = historical_guids(ROOT, [p + DLL_META_SUFFIX for p in orphans.values()])
            hot = {g: fs for g, fs in miss.items() if g in hist}
            if hot:
                print(f"\n  **身份漂移实锤**：{len(hot)} 个悬空 guid 正是下列 DLL "
                      "历史上用过的身份 —— 场景/prefab 在找旧 guid，"
                      "而 DLL 的 .meta 已经换成新的了。")
                for g, fs in sorted(hot.items(), key=lambda x: -len(x[1])):
                    who = [f for gg, p in hist.items() if gg == g
                           for f in [p]]
                    print(f"! {g}  引用 {len(fs)} 处 "
                          f"{sorted(fs)[:2]}")
                    print(f"    该 guid 历史属于: {who}")
                print("::error::把对应 .meta 的 guid 改回历史值"
                      "（换文件名不等于换身份，Unity 靠 guid 定位）")
            else:
                print("\n  零引用但**未发现身份漂移** —— 多半是纯托管库"
                      "（只需 using，不需要资产引用），属正常。")
                print("  注意：本节只覆盖 Assets/**；打包进 asmdef 或"
                      " 由代码动态加载的 DLL 不在其中。")
    return 0


def historical_guids(root: str, rel_metas: list[str]) -> dict[str, str]:
    """从 git 历史里取这些 .meta 曾用过的 guid -> 现在的路径。

    拿不到 git（CI 里浅克隆、或历史被清）时返回空 dict，判据二退化为
    只报「零引用」，不硬判。宁可少报也不误报。
    """
    import subprocess
    out: dict[str, str] = {}
    for rel in rel_metas:
        try:
            log = subprocess.run(
                ["git", "log", "--all", "--format=%H", "--", rel],
                cwd=root, capture_output=True, text=True, timeout=60)
            if log.returncode != 0:
                return out
            for sha in log.stdout.split():
                ls = subprocess.run(
                    ["git", "ls-tree", sha, "Assets/" + rel if not rel.startswith("Assets/")
                     else rel],
                    cwd=root, capture_output=True, text=True, timeout=60)
                blob = ls.stdout.split("\t")[0].split()[-1] if ls.stdout.strip() else ""
                if not blob:
                    continue
                cat = subprocess.run(["git", "cat-file", "-p", blob],
                                     cwd=root, capture_output=True, text=True,
                                     timeout=60)
                m = GUID_IN_META.search(cat.stdout)
                if m:
                    out.setdefault(m.group(1), rel)
        except (OSError, subprocess.SubprocessError):
            return out
    return out


if __name__ == "__main__":
    raise SystemExit(main())