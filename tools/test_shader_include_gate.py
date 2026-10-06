#!/usr/bin/env python3
"""sync_official_shaders 的 include 门禁双向回归测试。

为什么这个测试必须存在
----------------------
门禁的作用是「缺 include 就不覆盖」，因为覆盖一个编译不过的官方 shader
= 洋红，比现在的纯色空壳更显眼。但这个门禁栽过一次，而且**栽得极安静**：

  `Common.hlsl` 里有一整条按平台分发的 API 分支链
      #if defined(SHADER_API_GAMECORE)   -> GameCore.hlsl（主机）
      #elif defined(SHADER_API_XBOXONE)  -> XBoxOne.hlsl
      #elif defined(SHADER_API_GLES3)    -> GLES3.hlsl（WebGL 走这条）
      ...
  早期实现用不带条件的 `INCLUDE.findall(text)` 递归，于是把10 条死分支的
  include 全算成「缺」。结果 45 个官方 shader 全部拒绝覆盖，其中包括被
  59 个材质引用的 `Universal Render Pipeline/Lit` —— 画面黑屏的主角。
  报告里写「已用官方源码覆盖：0」，流程一路绿灯。

修正是让扫描带平台感知。只测「正常情况能覆盖」是不够的：那种情况在
修正前后都是 0 和 26，测不出门禁是否被**放松过头**。所以这里必须双向：

  活分支文件缺失  -> 必须拒（否则洋红）
  死分支文件缺失  -> 必须不拒（否则修复空转）

只测一个方向，都会留下一个能通过测试的坏实现。

跑法：
    python3 tools/test_shader_include_gate.py
需要先有 URP 包缓存，见 --help。
"""
from __future__ import annotations

import argparse
import os
import re
import shutil
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import sync_official_shaders as S   # noqa: E402


def _have_real_pkg() -> str | None:
    """找到一份真实的 URP 包缓存（含 core + universal）。

    优先用环境变量，其次用当前仓库的 Library/PackageCache。
    找不到就返回 None —— 门禁测试没有真包等于没测。
    """
    cands = []
    env = os.environ.get("HTF_TEST_PKG_CACHE")
    if env:
        cands.append(env)
    cands.append(os.path.join(S.ROOT, "Library", "PackageCache"))
    for c in cands:
        if not os.path.isdir(c):
            continue
        names = os.listdir(c)
        has_core = any(n.startswith("com.unity.render-pipelines.core@")
                       for n in names)
        has_univ = any(n.startswith("com.unity.render-pipelines.universal@")
                       for n in names)
        if has_core and has_univ:
            return c
    return None


def _copy_pkg(cache: str, dest: str) -> None:
    os.makedirs(dest, exist_ok=True)
    for n in os.listdir(cache):
        if n.startswith("com.unity.render-pipelines.core@") or \
           n.startswith("com.unity.render-pipelines.universal@"):
            shutil.copytree(os.path.join(cache, n), os.path.join(dest, n))


def _stub_dir(cache: str) -> str | None:
    """找一份带 DummyShaderTextExporter 桩的 Assets/Shader 目录。"""
    for root in (S.ROOT,):
        d = os.path.join(root, "Assets", "Shader")
        if not os.path.isdir(d):
            continue
        names = os.listdir(d)
        if not any(n.endswith(".shader") for n in names):
            continue
        has_stub = False
        for n in names:
            if n.endswith(".shader"):
                try:
                    with open(os.path.join(d, n), encoding="utf-8",
                              errors="replace") as f:
                        if "DummyShaderTextExporter" in f.read(65536):
                            has_stub = True
                            break
                except OSError:
                    pass
        if has_stub:
            return d
    return None


def _print_pkg_identity(cache: str) -> None:
    """打印被测包的**真实身份**（名字 / 版本 / .shader 数）。

    为什么必须打这个：#126 的门禁自检输出是

        [FAIL] 活分支（GLES3）缺失 -> 拒绝更多  3 -> 3

    只看这一行无法判断是「门禁实现坏了」还是「CI 的包和本地不是同一个」。
    而这两种情况的修法完全相反：前者要改代码，后者要改测试的预期基线。
    目录名里的 `@789199009d13` 是**内容哈希**不是版本号 ——
    本地两份不同版本的包可以撞出同一个哈希后缀（实际就撞了：
    10.10.1 的包目录名和 CI 17.x 的完全一样），
    所以必须读 package.json 里的 version 字段。
    """
    import json
    for n in sorted(os.listdir(cache)):
        base = n.split("@", 1)[0]
        if not base.startswith("com.unity.render-pipelines."):
            continue
        pj = os.path.join(cache, n, "package.json")
        ver = "?"
        if os.path.isfile(pj):
            try:
                with open(pj, encoding="utf-8") as f:
                    ver = json.load(f).get("version", "?")
            except (OSError, ValueError):
                ver = "?"
        nsh = 0
        for _, _, files in os.walk(os.path.join(cache, n)):
            nsh += sum(1 for x in files if x.endswith(".shader"))
        print(f"  被测包 {base}  version={ver}  "
              f"{nsh} 个 .shader  ({n})")


def _run_with(cache: str, stubs: str, tmp: str) -> tuple[int, int, str]:
    """在临时工程里跑一次 _run，返回 (覆盖数, 空壳数, 输出)。"""
    import io
    import contextlib
    proj = os.path.join(tmp, "proj")
    shutil.rmtree(proj, ignore_errors=True)
    os.makedirs(proj)
    _copy_pkg(cache, os.path.join(proj, "Library", "PackageCache"))
    shutil.copytree(stubs, os.path.join(proj, "Assets", "Shader"))

    old_root = S.ROOT
    old_assets = S.ASSETS_SHADER
    old_cache = S.PACKAGE_CACHE
    buf = io.StringIO()
    try:
        S.ROOT = proj
        S.ASSETS_SHADER = os.path.join(proj, "Assets", "Shader")
        S.PACKAGE_CACHE = os.path.join(proj, "Library", "PackageCache")
        with contextlib.redirect_stdout(buf):
            S._run()
    finally:
        S.ROOT, S.ASSETS_SHADER, S.PACKAGE_CACHE = \
            old_root, old_assets, old_cache
    out = buf.getvalue()
    m = re.search(r"已用官方源码覆盖：(\d+)", out)
    covered = int(m.group(1)) if m else -1
    return covered, -1, out


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--pkg-cache", default=None,
                    help="真实 URP 包缓存目录（含 core + universal）")
    args = ap.parse_args()

    cache = args.pkg_cache or _have_real_pkg()
    if not cache:
        print("找不到真实 URP 包缓存，本测试不执行任何判断。")
        print("  门禁测试没有真包等于没测 —— 它测的正是「包路径带@版本后缀」")
        print("  和「递归展开」这两件事。")
        print("  用法：--pkg-cache /path/to/Library/PackageCache")
        return 1
    stubs = _stub_dir(cache)
    if not stubs:
        print("找不到带 DummyShaderTextExporter 的 Assets/Shader 目录。")
        return 1

    core = next(n for n in os.listdir(cache)
                if n.startswith("com.unity.render-pipelines.core@"))
    api = os.path.join(cache, core, "ShaderLibrary", "API")
    print(f"包缓存: {cache}")
    print(f"桩目录: {stubs}")
    _print_pkg_identity(cache)
    print()

    failures = []

    def check(label: str, cond: bool, detail: str = "") -> None:
        mark = "OK  " if cond else "FAIL"
        print(f"  [{mark}] {label}" + (f"  {detail}" if detail else ""))
        if not cond:
            failures.append(label)

    with tempfile.TemporaryDirectory() as tmp:
        # --- 基准：包完整，覆盖数应 > 0 ---
        base, _, out = _run_with(cache, stubs, tmp)
        check("包完整时有覆盖发生", base > 0, f"覆盖={base}")

        # --- 方向一：活分支文件缺失，必须拒 ---
        if os.path.isfile(os.path.join(api, "GLES3.hlsl")):
            save = os.path.join(tmp, "GLES3.hlsl")
            shutil.move(os.path.join(api, "GLES3.hlsl"), save)
            try:
                c1, _, _ = _run_with(cache, stubs, tmp)
            finally:
                shutil.move(save, os.path.join(api, "GLES3.hlsl"))
            check("活分支（GLES3）缺失 -> 拒绝更多",
                  c1 < base, f"{base} -> {c1}")

            # --- 方向二：死分支文件缺失，必须不拒 ---
            d3d = os.path.join(api, "D3D11.hlsl")
            if os.path.isfile(d3d):
                save2 = os.path.join(tmp, "D3D11.hlsl")
                shutil.move(d3d, save2)
                try:
                    c2, _, _ = _run_with(cache, stubs, tmp)
                finally:
                    shutil.move(save2, d3d)
                check("死分支（D3D11）缺失 -> 不额外拒绝",
                      c2 == base, f"{base} -> {c2}")
        else:
            print("  [跳过] API/GLES3.hlsl 不存在，两个方向都无法测")

    print()
    if failures:
        print(f"失败 {len(failures)} 项：")
        for f in failures:
            print(f"  - {f}")
        # 失败时必须给出**可操作的信息**，否则这个 FAIL 只能被看着干瞪眼。
        # #126 就是这么废掉的：报告里只有一行「3 -> 3」，
        # 既不知道 3 个候选是谁，也不知道剩下 23 个为什么被拒。
        _diagnose(cache, stubs, tmp)
        return 1
    print("双向验证通过：门禁既能拒真缺，也不会被死分支拖死。")
    print(f"（基线覆盖数 {base}；被测包身份见上方「被测包」行）")
    return 0


def _diagnose(cache: str, stubs: str, tmp: str) -> None:
    """门禁失败时打印候选清单与被拒理由。

    「门禁坏了」和「包不一样」只能靠这组信息区分：
      - 若被拒理由全是**同一批**平台 API 文件（GameCore/XBoxOne/
        PSSL/D3D11/Metal/Vulkan...），说明门禁没做平台感知；
      - 若被拒理由是**包版本里真的没有**的文件，那是包/门禁的匹配问题，
        修代码方向完全不同。
    """
    import io
    import contextlib
    proj = os.path.join(tmp, "diag")
    shutil.rmtree(proj, ignore_errors=True)
    os.makedirs(proj)
    _copy_pkg(cache, os.path.join(proj, "Library", "PackageCache"))
    shutil.copytree(stubs, os.path.join(proj, "Assets", "Shader"))

    old_root, old_assets, old_cache = S.ROOT, S.ASSETS_SHADER, S.PACKAGE_CACHE
    buf = io.StringIO()
    try:
        S.ROOT = proj
        S.ASSETS_SHADER = os.path.join(proj, "Assets", "Shader")
        S.PACKAGE_CACHE = os.path.join(proj, "Library", "PackageCache")
        with contextlib.redirect_stdout(buf):
            S._run()
    except Exception as e:                      # noqa: BLE001
        print(f"  （诊断跑挂了：{e}）")
        return
    finally:
        S.ROOT, S.ASSETS_SHADER, S.PACKAGE_CACHE = \
            old_root, old_assets, old_cache

    out = buf.getvalue()
    print()
    print("  ---- 失败诊断：候选与被拒理由 ----")
    for line in out.splitlines():
        s = line.strip()
        if s.startswith("已用官方源码覆盖") or \
           s.startswith("有 include 解析不全") or \
           s.startswith("包内无对应"):
            print(f"  {s}")
    blocked = [l.strip()[2:] for l in out.splitlines()
               if l.strip().startswith("! ")]
    if blocked:
        print(f"  被 include 门禁拒绝的候选（{len(blocked)} 个）:")
        for b in blocked[:30]:
            print(f"    ! {b}")
        if len(blocked) > 30:
            print(f"    ... 另 {len(blocked) - 30} 个")
    # 被拒理由按「缺失文件」聚合：同一个缺 10 次说明是包结构问题，
    # 缺 1 次说明是个别 shader 的问题 —— 两者的修法不同。
    reasons: dict[str, int] = {}
    lines = out.splitlines()
    for i, line in enumerate(lines):
        if line.strip().startswith("! "):
            j = i + 1
            while j < len(lines) and lines[j].strip().startswith("缺: "):
                f = lines[j].strip()[3:].strip()
                reasons[f] = reasons.get(f, 0) + 1
                j += 1
    if reasons:
        print("  被拒理由（缺失的 include -> 出现次数）:")
        for f, c in sorted(reasons.items(), key=lambda kv: -kv[1])[:25]:
            print(f"    {c:3d} x  {f}")
    else:
        # 这个措辞很关键。**「没有拒绝」本身就是一种拒绝原因**，
        # 而且是最坏的那种：门禁把活分支当死分支放过，于是缺 include
        # 的官方 shader 照样被复制上去 -> 编译失败 -> 洋红。
        # 只写「不是 include 门禁拒的」会让人以为门禁没参与，
        # 从而查错方向。
        print("  被拒理由: 无 —— 即**没有任何候选被门禁拦下**。")
        print("  这恰好是「删掉活分支文件后覆盖数不变」的成因：")
        print("  门禁把WebGL 会走的分支也当成死分支跳过了，于是它")
        print("  什么都没检查。真实故障是覆盖了编译不过的官方 shader")
        print("  -> 洋红，而不是「没覆盖」。")


if __name__ == "__main__":
    sys.exit(main())