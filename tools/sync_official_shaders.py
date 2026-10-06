"""Overwrite decompiler stub shaders with the real package sources.

Why this exists
---------------
AssetRipper writes every shader it cannot decompile as a
``DummyShaderTextExporter`` stub. Most of them, however, are not the game's
own work: they are Unity/URP package shaders (Lit, Unlit, post-processing,
utils...). The real source for those sits in the package cache that Unity
restores from Packages/manifest.json, at exactly the version the project asks
for -- so this script matches them by shader *name* (``Shader "..."`` on the
first line, which the stubs preserve) and copies the real source over the stub.

The .meta files are left untouched, so every material reference (which points
at the stub's GUID) keeps resolving once the stub gains real content.

Game-authored Shader Graph shaders have no source anywhere in the build and
are reported as unmatched; they are not touched.

The preflight check (why copying blindly is dangerous)
-----------------------------------------------------
Official package shaders are not self-contained: ``URP/Lit.shader`` pulls in
a dozen ``#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/
*.hlsl"`` files. If any of those cannot be resolved, the shader fails to compile
-- and a shader that fails to compile is rendered **magenta** while Unity still
exits 0. Copying the official source over a *working* stub would therefore be a
regression, not a fix.

So before copying, every ``#include`` in the candidate file is resolved against
the package cache (both ``Packages/...`` form and cache-relative form). If any
include is missing, that file is **skipped and reported** rather than copied.
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

# 两个路径都允许用环境变量改写，理由是**可测**：
# ROOT 由 __file__ 推导，把脚本复制到临时目录跑就会指向那个临时目录，
# 于是想验证「缺 include 时不覆盖」这条分支时根本没法构造场景
# （唯一能跑出来的分支是「没找到 PackageCache，跳过」——
# 而那条分支根本碰不到新加的预检逻辑，等于没测）。
# 有了覆盖，测试才能真正走到预检。
if os.environ.get("HTF_ROOT"):
    ROOT = os.environ["HTF_ROOT"]
    ASSETS_SHADER = os.path.join(ROOT, "Assets", "Shader")
    PACKAGE_CACHE = os.path.join(ROOT, "Library", "PackageCache")

SHADER_NAME = re.compile(r'^\s*Shader\s+"([^"]+)"', re.MULTILINE)
STUB_MARKER = "DummyShaderTextExporter"

# 结果落盘路径：CI 会把它并进诊断报告提交回仓库，这样「覆盖到底有没有生效」
# 不再只能靠猜 —— 上一轮就是因为没有这个文件，报告里只有 core 包的路径，
# universal 包缺席这件事完全看不出来。
REPORT = os.path.join(ROOT, "verify-evidence", "official-shader-sync.txt")


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


INCLUDE = re.compile(r'^\s*#include\s+"([^"]+)"', re.M)


def missing_includes(src: str, cache_dir: str) -> list[str]:
    """列出在包缓存里解析不到的 `#include`。

    只认包内绝对路径形式（``Packages/<pkg>/...``）与包内相对形式
    （同目录下的 ``Foo.hlsl``）。这两种是 Unity 解析 URP 包内 shader
    时实际用的写法 —— URP 的 Lit.shader 全部用 ``Packages/...`` 绝对形式。

    为什么要逐个解析而不是「文件存在就复制」：官方 shader 依赖同包内的
    .hlsl，这些.hlsl 又 include 更多的 .hlsl。只检查第一层会漏掉
    「Lit.hlsl 在，但 LitInput.hlsl 不在」这种。所以这里**递归**展开，
    把每一层都查一遍。
    """
    cache_dir = os.path.abspath(cache_dir)
    seen: set[str] = set()
    missing: list[str] = []
    queue = [src]
    while queue:
        cur = queue.pop()
        try:
            with open(cur, encoding="utf-8", errors="replace") as f:
                text = f.read()
        except OSError:
            continue
        cur_dir = os.path.dirname(os.path.abspath(cur))
        for inc in INCLUDE.findall(text):
            if inc in seen:
                continue
            seen.add(inc)
            # Packages/... -> Library/PackageCache/...
            cand = os.path.join(cache_dir, inc[len("Packages/"):]) \
                if inc.startswith("Packages/") else None
            if cand is None or not os.path.isfile(cand):
                cand = os.path.normpath(os.path.join(cur_dir, inc))
            if os.path.isfile(cand):
                queue.append(cand)
            else:
                missing.append(f"{os.path.relpath(cur, cache_dir)} -> {inc}")
    return missing


def main() -> int:
    # 所有 print 同时落盘。做法是最后统一写一遍：把 stdout 重定向到
    # 内存缓冲，再既打印又写文件 —— 避免在每个分支里手动写两次，
    # 那种写法漏一处分支就会丢一段证据（而丢的往往正是「没生效」那段）。
    import io
    import contextlib
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        _run()
    text = buf.getvalue()
    print(text, end="")
    try:
        os.makedirs(os.path.dirname(REPORT), exist_ok=True)
        with open(REPORT, "w", encoding="utf-8") as f:
            f.write(text)
    except OSError as e:
        print(f"::warning::结果落盘失败（不影响构建）: {e}")
    return 0


def _run() -> int:
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
    blocked: list[tuple[str, list[str]]] = []
    for stub in stubs:
        name = shader_name(stub)
        if name is None:
            continue
        src = by_name.get(name)
        if src is None:
            missing.append(name)
            continue
        # 预检：include 解析不全就别覆盖。
        # 覆盖一个编译不过的官方 shader =洋红，而洋红比现在的
        # 「纯色空壳」更显眼、更难定位。所以宁可保持原样并报出来。
        bad = missing_includes(src, PACKAGE_CACHE)
        if bad:
            blocked.append((name, bad))
            continue
        shutil.copyfile(src, stub)
        replaced.append(name)

    print(f"\n已用官方源码覆盖：{len(replaced)}")
    for n in replaced:
        print(f"  + {n}")
    if blocked:
        print(f"\n有 include 解析不全、**未覆盖**（覆盖会导致编译失败 -> 洋红）："
              f"{len(blocked)}")
        for n, bad in blocked:
            print(f"  ! {n}")
            for b in bad[:5]:
                print(f"      缺: {b}")
            if len(bad) > 5:
                print(f"      ... 另 {len(bad) - 5} 个")
    print(f"\n包内无对应（游戏自定义，保持原样）：{len(missing)}")
    for n in missing:
        print(f"  - {n}")

    # ---- 覆盖后的验收 ----
    #
    # 「复制成功」不等于「问题解决」。这一步按**具体判据**检查覆盖后的
    # 文件是不是真的具备原版该有的东西，避免脚本一路报成功而画面没变。
    #
    # 判据来自实测：URP/Lit 空壳的 Pass 里没有任何渲染状态块（Blend /
    # ZWrite / Cull 全无），而它的 59 个材质里有 11 个是透明的
    # （_Surface=1 / _ZWrite=0 / _DstBlend=10），水面、WaterParticle、
    # WaterParticleWhite、SpitParticle 全在内 —— 也就是「该透明的地方
    # 渲成不透明」，且不产生一行日志。
    #
    # 官方 Lit.shader 带完整 Tags（Queue / RenderType）和多 Pass，能一并
    # 解决这些。所以覆盖后必须验一遍：Queue 在不在、渲染状态在不在。
    print("\n### 覆盖后验收（关键判据）")
    for name in replaced:
        stub = None
        for p in stubs:
            if shader_name(p) == name:
                stub = p
                break
        if not stub:
            continue
        with open(stub, encoding="utf-8", errors="replace") as f:
            now = f.read()
        has_queue = '"Queue"' in now
        has_rt = '"RenderType"' in now
        has_blend = bool(re.search(r"^[ \t]*Blend[ \t]+\w", now, re.M))
        has_zwrite = bool(re.search(r"^[ \t]*ZWrite[ \t]+\w", now, re.M))
        passes = len(re.findall(r"^[ \t]*Pass[ \t]*(?:\{|$)", now, re.M))
        flags = []
        if not has_queue:
            flags.append("缺 Queue")
        if not has_rt:
            flags.append("缺 RenderType")
        if not has_blend:
            flags.append("缺 Blend 状态")
        if not has_zwrite:
            flags.append("缺 ZWrite 状态")
        mark = "OK" if not flags else "; ".join(flags)
        print(f"  {name}: Queue={'Y' if has_queue else 'N'} "
              f"RenderType={'Y' if has_rt else 'N'} "
              f"Blend={'Y' if has_blend else 'N'} "
              f"ZWrite={'Y' if has_zwrite else 'N'} "
              f"Pass数={passes}  -> {mark}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
