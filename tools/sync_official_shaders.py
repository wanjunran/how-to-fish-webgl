"""Overwrite decompiler stub shaders with the real package sources.

Why this exists
---------------
AssetRipper writes every shader it cannot decompile as a
``DummyShaderTextExporter`` stub (a pass that returns solid white). 106 of the
project's shaders are such stubs. Most of them, however, are not the game's
own work: they are Unity/URP package shaders (Lit, Unlit, post-processing,
utils...). The real source for those sits in the package cache that Unity
restores from Packages/manifest.json, at exactly the version the project asks
for -- so this script matches them by shader *name* (``Shader "..."`` on the
first line, which the stubs preserve) and copies the real source over the stub.

The .meta files are left untouched, so every material reference (which points
at the stub's GUID) keeps resolving once the stub gains real content.

Game-authored Shader Graph shaders have no source anywhere in the build and
are reported as unmatched; they are not touched.
"""
from __future__ import annotations

import glob
import os
import re
import shutil
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSETS_SHADER = os.path.join(ROOT, "Assets", "Shader")
PACKAGE_CACHE = os.path.join(ROOT, "Library", "PackageCache")

SHADER_NAME = re.compile(r'^\s*Shader\s+"([^"]+)"', re.MULTILINE)
STUB_MARKER = "DummyShaderTextExporter"


def shader_name(path: str) -> str | None:
    """First `Shader "..."` declaration -- the identity Unity matches on."""
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            head = f.read(8192)
    except OSError:
        return None
    m = SHADER_NAME.search(head)
    return m.group(1) if m else None


def is_stub(path: str) -> bool:
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            return STUB_MARKER in f.read(65536)
    except OSError:
        return False


def main() -> int:
    if not os.path.isdir(PACKAGE_CACHE):
        print(f"跳过：未找到 {PACKAGE_CACHE}（Library 缓存未命中）")
        # 「跳过」和「覆盖成功」在 CI 里都是 exit 0，从外面看一模一样。
        # 而事实上 Unity 装的是**编译后**的 shader 包，官方 .shader 源码
        # 常常根本不在 PackageCache 里 —— 也就是这一步压根没生效，
        # 那一百多个空壳还是空壳。这个事实必须能被人看见，所以打一条
        # warning 注解（匿名 curl 抓 run 页面就能看到）。
        print("::warning::没找到 Library/PackageCache —— "
              "官方 shader 源码覆盖**未执行**，所有 AssetRipper 空壳保持原样。"
              "这不是成功，是没做。")
        return 0

    # Index every .shader shipped by any restored package.
    by_name: dict[str, str] = {}
    for path in glob.glob(os.path.join(PACKAGE_CACHE, "**", "*.shader"), recursive=True):
        name = shader_name(path)
        if name and name not in by_name:
            by_name[name] = path
    print(f"包内可用 shader：{len(by_name)}")
    if not by_name:
        # 同理：包里一个 .shader 都没有，说明这个缓存恢复的是编译产物。
        print("::warning::Library/PackageCache 里没有任何 .shader 源码 —— "
              "覆盖**未执行**。Unity 只装了编译后的 shader 包。")
        print("缓存目录里有什么（前 20 项）:")
        try:
            for n in sorted(os.listdir(PACKAGE_CACHE))[:20]:
                print(f"  {n}")
        except OSError as e:
            print(f"  （读不了：{e}）")

    stubs = sorted(
        os.path.join(ASSETS_SHADER, f)
        for f in os.listdir(ASSETS_SHADER)
        if f.endswith(".shader")
    )
    stubs = [p for p in stubs if is_stub(p)]
    print(f"空壳 shader：{len(stubs)}")

    replaced: list[str] = []
    missing: list[str] = []
    for stub in stubs:
        name = shader_name(stub)
        if name is None:
            continue
        src = by_name.get(name)
        if src is None:
            missing.append(name)
            continue
        shutil.copyfile(src, stub)
        replaced.append(name)

    print(f"\n已用官方源码覆盖：{len(replaced)}")
    for n in replaced:
        print(f"  + {n}")
    print(f"\n包内无对应（游戏自定义，保持原样）：{len(missing)}")
    for n in missing:
        print(f"  - {n}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
