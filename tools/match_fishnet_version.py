#!/usr/bin/env python3
"""精确定位 FishNet 版本：拿游戏 DLL 的类型名集合去筛各版本源码。

背景
----
游戏里只有一个编译好的 FishNet.Runtime.dll，版本号被 AssetRipper
剥掉了。换一份匹配的源码重新编译是解决加载崩溃的关键（见
加载崩溃诊断.md），但猜错版本就白搭一轮构建。

之前用"某个类型在不在"来筛，踩了两个坑：
  1. `find -name "ServerRpcAttribute.cs"` 找的是 codegen 的
     Attributes.cs，Runtime 下的同名文件被漏掉——多个同名文件，
     find 命中的是第一个。
  2. `ServerRpc` 是 attribute 名不是类型名，拿它当类型去 grep
     源码当然找不到。

所以改成**集合差**：把各版本 Runtime 源码里声明的公开类型名
抽成集合，与 DLL 的字符串集合求差。DLL 有而某版本源码没有的
类型，说明那个版本比游戏用的旧；某版本有而 DLL 没有的，说明更新。
两边都不吻合的版本直接排除。

用法：
    python3 match_fishnet_version.py <FishNet.Runtime.dll> 4.1.0 4.2.1 ...
"""
import re
import sys
from pathlib import Path

TYPE_RE = re.compile(
    r"^\s*(?:\[[^\]]*\]\s*)*"
    r"(?:public|internal|protected|private)?\s*"
    r"(?:static\s+|sealed\s+|abstract\s+|partial\s+|unsafe\s+|readonly\s+)*"
    r"(class|struct|enum|interface|delegate)\s+"
    r"([A-Za-z_]\w*)",
    re.M,
)

# 名字里带这些的类是 codegen 产物或内部实现，跨版本大改，
# 拿来做指纹噪声太大，单独排除。
NOISE = re.compile(
    r"(__|GeneratedWriters|GeneratedReaders|GeneratedComparers|_Internal$)"
)


def dll_strings(path: Path) -> set[str]:
    d = path.read_bytes()
    return {m.group().decode() for m in re.finditer(rb"[\x20-\x7e]{4,}", d)}


def src_types(root: Path) -> set[str]:
    out: set[str] = set()
    for f in root.rglob("*.cs"):
        try:
            t = f.read_text(encoding="utf-8", errors="replace")
        except Exception:
            continue
        for _, name in TYPE_RE.findall(t):
            if not NOISE.search(name):
                out.add(name)
    return out


def main() -> int:
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    dll = Path(sys.argv[1])
    if not dll.is_file():
        print(f"找不到 {dll}")
        return 2

    ds = dll_strings(dll)
    # DLL 里出现的、看起来像类型名的（首字母大写且不含空格）
    dll_types = {t for t in ds if re.fullmatch(r"[A-Z][A-Za-z0-9_]{3,60}", t)}
    print(f"目标: {dll.name}")
    print(f"  DLL 字符串 {len(ds)} 条，其中疑似类型名 {len(dll_types)} 个")
    print()

    for tag in sys.argv[2:]:
        root = Path(f"/tmp/x-{tag}/FishNet-{tag}/Assets/FishNet/Runtime")
        if not root.is_dir():
            root = Path(f"/tmp/fn/FishNet-{tag}/Assets/FishNet/Runtime")
        if not root.is_dir():
            print(f"{tag:8} 未下载")
            continue

        st = src_types(root)
        if not st:
            print(f"{tag:8} 源码解析为空")
            continue

        # DLL 有、这个版本没有 -> 这个版本太旧
        only_dll = dll_types - st
        # 这个版本有、DLL 没有 -> 这个版本太新
        only_src = st - dll_types
        # 两边都有的是共同类型，作为可信指纹
        both = dll_types & st

        print(f"=== {tag} ===")
        print(f"  源码类型 {len(st)}  共同 {len(both)}  "
              f"DLL独有 {len(only_dll)}  版本独有 {len(only_src)}")
        # 交集要大、独有要小。共同时 <100 的基本不是同一版本。
        if len(both) < 100:
            print("  判定: 共同类型太少，不匹配")
        elif len(only_dll) > len(only_src) * 0.5:
            print("  判定: DLL 独有多-> 这个版本比游戏用的旧")
        elif len(only_src) > len(only_dll) * 0.5:
            print("  判定: 版本独有多 -> 比游戏用的新")
        else:
            print("  判定: **高度吻合**")
        # 打印少量样本供人工确认
        print(f"    DLL 独有样本: {sorted(only_dll)[:6]}")
        print(f"    版本独有样本: {sorted(only_src)[:6]}")
        print()

    return 0


if __name__ == "__main__":
    sys.exit(main())
