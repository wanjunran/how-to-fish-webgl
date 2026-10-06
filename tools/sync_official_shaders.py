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

# URP / core 包自带 shader 的命名前缀。用于离线推断「覆盖目标是谁」。
#
# 这里必须有**存在性断言**：判据漏一个前缀，结论就从「60 个能覆盖」
# 变成「只有 7 个能覆盖」，而表面上只是多打几行名字 —— 判据失效的输出
# 和正常输出长得一模一样，这个坑本项目已经踩过好几次。
#
# 判据只能用来**回答「目标是谁」**，回答「覆盖会不会成功」必须由 CI 里
# 带 PackageCache 的完整路径实测（include 是否齐全那里才知道）。
PACKAGE_PREFIXES = (
    "Universal Render Pipeline/",
    "Shader Graphs/",
    "Hidden/Shader Graph/",
    "Hidden/Core/",
    "Hidden/CoreSRP/",
    "Hidden/Universal Render Pipeline/",
    "Hidden/TerrainEngine/Details/UniversalPipeline/",
    "Hidden/TerrainEngine/",
    "Skybox/",
    "UI/",
)
assert all(p.endswith("/") for p in PACKAGE_PREFIXES), "前缀必须以 / 结尾，否则前缀匹配本身就没有意义"

# 结果落盘路径：CI 会把它并进诊断报告提交回仓库，这样「覆盖到底有没有生效」
# 不再只能靠猜 —— 上一轮就是因为没有这个文件，报告里只有 core 包的路径，
# universal 包缺席这件事完全看不出来。
REPORT = os.path.join(ROOT, "verify-evidence", "official-shader-sync.txt")


def count_passes(text: str) -> int:
    """数真正的 Pass 块。

    两个坑，都是实测撞到的：

    1. 最初写 `^[ \\t]*Pass[ \\t]*(?:\\{|$)` —— 要求 Pass 在行首。
       但 AssetRipper 的空壳里Pass 是**同行**的：
           `\\tSubShader { Pass { HLSLPROGRAM`
       于是计数为 0，看着像「这个 shader 一个 Pass 都没有」。

    2. 放宽之后不能简单改成 `Pass[ \\t]*\\{` —— `GrabPass` 含 "Pass"
       子串，会被数进去；而注释里的 Pass（// Pass 结束后...）同理。

    所以判据三要素齐全才算：前面是行首或紧邻的 `{` / `}`（排掉
    GrabPass，因为它的 Pass 前面是字母）、后面（允许跨空白换行）
    跟着 `{`。注释行先剥掉。
    """
    body = re.sub(r"//[^\n]*", "", text)
    return len(re.findall(r"(?:^|\{|\})[ \t]*Pass[ \t\r\n]*\{", body, re.M))


def _strip_comments(text: str) -> str:
    """剥掉 // 行注释。判据函数共用，避免每个正则各写一遍。"""
    return re.sub(r"//[^\n]*", "", text)


def _tag_present(text: str, key: str) -> bool:
    """Tags 块里是否有这个键。

    ShaderLab 有两种写法，官方 URP 源码两种都出现，且**同一行混用**：

        Tags{"RenderType" = "Opaque" "Queue" = "Geometry" ...}
               ^^^^^^^^^^ 带引号        ^^^^^^ 不带引号

    早期判据只认带引号的 `'"Queue"' in text`，于是 26 个刚覆盖成功的官方
    shader 全被标成「缺 Queue」—— 假警报比没判据更糟，它让人不再相信
    验收这一节。
    """
    if '"' + key + '"' in text:
        return True
    return bool(re.search(rf'(?<!")\b{re.escape(key)}\b(?!")\s*=\s*"', text))


def _state_present(text: str, key: str) -> bool:
    """渲染状态命令是否存在，接受字面量与材质参数两种形式。

    官方 Lit 的标准写法是**方括号材质参数**：

        Blend [_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        Cull  [_Cull]

    早期判据是 `^[ \\t]*Blend[ \\t]+\\w`，要求后面紧跟单词字符，
    于是上面三行全部匹配不上 -> 「缺 Blend 状态 / 缺 ZWrite 状态」。
    """
    return bool(re.search(rf"^[ \t]*{re.escape(key)}[ \t]+(?:\[|\w)", text, re.M))


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

# ---------------------------------------------------------------------------
# 平台感知：只展开 WebGL 实际会走的预处理分支
# ---------------------------------------------------------------------------
# 这一节存在的理由是一次**静默失效**：预检把 Common.hlsl 里 10 条死分支的
# include 全算成「缺」，于是 45 个官方 shader 全部被拒覆盖，其中就包括
# 被59 个材质引用的 `Universal Render Pipeline/Lit`。报告里写「已用官方
# 源码覆盖：0」，流程一路绿灯，而画面依旧是黑的。
#
# 官方 Common.hlsl 的真实结构（core 包，已下载核对）：
#
#     // Include language header
#     #if defined (SHADER_API_GAMECORE)
#     #include ".../gamecore/ShaderLibrary/API/GameCore.hlsl"
#     #elif defined(SHADER_API_XBOXONE)
#     #include ".../xboxone/ShaderLibrary/API/XBoxOne.hlsl"
#     #elif defined(SHADER_API_PS4)
#     ...
#     #elif defined(SHADER_API_GLES3)          <-- WebGL 走这条
#     #include ".../core/ShaderLibrary/API/GLES3.hlsl"
#     ...
#     #else
#     #error unsupported shader api
#     #endif
#
# WebGL 构建里 SHADER_API_GAMECORE / XBOXONE / PS4 / PS5 一个都不定义，
# 所以那4 个 include **永远不会被预处理器读到**，也永远不可能导致编译失败。
# 把它们算成「缺」不是保守，是**算错了**：门禁的方向是「宁可别覆盖」，
# 判错就退化成「全都别覆盖」，于是修复全程空转。
#
# 这里不判断 include 的内容，只判断**它所在的分支在 WebGL 下是否可达**。
# 做的仍然是对官方源码的机械解析，没有任何手写 shader 语义。
# WebGL 下「视为已定义」的宏。
#
# 关键：`SHADER_API_GLCORE` **不在**这里。GLCore 是桌面 OpenGL Core
# profile（Standalone Windows/Linux/macOS），Unity 的 WebGL 平台只用
# OpenGL ES 2.0/3.0（WebGL1/WebGL2）。把它算成「可能成立」会让
# `#elif` 链在 GLCore 那支就提前成立，后面的 GLES3 分支被跳过 ——
# 反向验证时正是这么错的：选中了 GLCore.hlsl 而不是 GLES3.hlsl。
# 方向错了的保守比不保守更糟，它会把唯一正确的分支踢掉。
WEBGL_API_MACROS = frozenset({
    "SHADER_API_GLES3", "UNITY_WEBGL", "UNITY_WEBGL_2",
})

# 明确不在 WebGL 下成立的平台宏。出现在 #if defined(X) 里就整块跳过。
DEAD_PLATFORM_MACROS = frozenset({
    # 主机平台（core 包 Common.hlsl 里各占一条 #elif）
    "SHADER_API_GAMECORE", "SHADER_API_GAMECORE_SONY",
    "SHADER_API_XBOXONE", "SHADER_API_XBOXONE_SONY",
    "SHADER_API_PS4", "SHADER_API_PS5",
    "SHADER_API_SWITCH",
    # 桌面 / 移动平台
    "SHADER_API_D3D11", "SHADER_API_D3D12",
    "SHADER_API_METAL", "SHADER_API_VULKAN",
    "SHADER_API_GLES",
    "SHADER_API_GLCORE",
    "UNITY_GAMECORE", "UNITY_GAMECORE_SONY", "UNITY_XBOXONE",
    "UNITY_PS4", "UNITY_PS5", "UNITY_METAL", "UNITY_VULKAN",
    "UNITY_SWITCH", "UNITY_D3D11", "UNITY_D3D12",
    "UNITY_IOS", "UNITY_ANDROID", "UNITY_STANDALONE",
})

# 无路径形式的 include（UnityCG.cginc 之类）。这些是**编辑器安装目录**
# 里的内置CGIncludes，Unity 自己按目标平台注入，不在 PackageCache 里。
# 预检只能查 PackageCache，查不到不代表编译失败 —— 判「缺」就会误杀
# `Hidden/Core/FallbackError` 这种本身只做报错提示的 shader。
BUILTIN_NO_PATH = frozenset({
    "UnityCG.cginc", "UnityShaderVariables.cginc",
    "UnityInstancing.cginc", "UnityInput.cginc",
    "HLSLSupport.cginc", "Lighting.cginc",
    "UnityIndirect.cginc", "UnityGBuffer.cginc",
})

# 条件编译指令。
DIRECTIVE = re.compile(r"^\s*#\s*(if|ifdef|ifndef|elif|else|endif)\b(.*)$")


def _branch_reachable(directive: str, cond: str) -> bool:
    """判断一条预处理分支在 WebGL 下是否可达。

    只处理「`#if defined(X)` / `#ifdef X` / `#if defined(X) && ...`」这类
    **纯平台宏**判定。含未知标识符（例如 ``defined(UNITY_FOO) &&
    !defined(SHADER_QUALITY_LOW)``）时**返回 True**（可达）——
    门禁必须偏向「多查」，宁可多报一个缺，不可放过一个真缺。
    """
    d = directive.lower()
    cond = cond.strip()

    if d == "else":
        return True
    if d == "endif":
        return True
    if d in ("ifdef", "ifndef"):
        m = re.match(r"([A-Za-z_]\w*)", cond)
        if not m:
            return True
        present = m.group(1) in _WEBGL_DEFINED
        return present if d == "ifdef" else not present

    # if / elif：抽出全部 defined(X) 与裸宏
    names = re.findall(r"defined\s*\(\s*([A-Za-z_]\w*)\s*\)|(?<![(])\b([A-Za-z_]\w*)\b",
                       cond)
    flat = [a or b for a, b in names]
    flat = [n for n in flat if n not in
            ("defined", "SHADER_API", "SHADER_TARGET", "UNITY_")]
    # 剔掉 `&&`/`||` 之类噪音后没有任何宏可判 -> 无法判定，按可达处理
    if not flat:
        return True

    # 只要出现任一「明确不成立」的平台宏，且没有出现 WebGL 成立的宏，
    # 则该分支在 WebGL 下不可达。
    has_live = any(n in _WEBGL_DEFINED for n in flat)
    has_dead = any(n in DEAD_PLATFORM_MACROS for n in flat)
    if has_dead and not has_live:
        return False
    return True


# WebGL 下「视为已定义」的宏。只放确定成立的，避免误杀。
_WEBGL_DEFINED = WEBGL_API_MACROS


def active_includes(text: str) -> list[str]:
    """返回在 WebGL 下**真正会被预处理器读到**的 include 列表。

    逐行跟踪 #if / #elif / #else / #endif 的嵌套，遇到不可达的分支就整块
    跳过，不把它里面的 include 算进来。
    """
    out: list[str] = []
    # 栈元素：(父分支是否可达, 本分支是否已判定为可达, 是否已进入 else)
    stack: list[tuple[bool, bool, bool]] = []
    reachable = True

    for line in text.splitlines():
        m = DIRECTIVE.match(line)
        if not m:
            if reachable:
                out.extend(INCLUDE.findall(line))
            continue

        directive, cond = m.group(1).lower(), m.group(2)
        if directive in ("if", "ifdef", "ifndef"):
            parent_ok = reachable
            ok = parent_ok and _branch_reachable(directive, cond)
            # 记录：本分支是否在 WebGL 下成立（用于 #else 取反）
            stack.append([parent_ok, ok, False])
            reachable = ok
        elif directive == "elif":
            if not stack:
                continue
            frame = stack[-1]
            parent_ok, taken, in_else = frame
            if in_else:
                # #else 之后还有 #elif 是非法的，保守当作不可达
                reachable = False
            elif taken:
                # C 预处理器短路：前面已成立，这一支轮不到。
                # 这一条不是可选的 —— 漏掉它会把 GLES3 分支踢掉。
                reachable = False
            else:
                ok = parent_ok and _branch_reachable("if", cond)
                frame[1] = ok
                reachable = ok
        elif directive == "else":
            if not stack:
                continue
            frame = stack[-1]
            parent_ok, taken, in_else = frame
            # 若前面没有任何分支在 WebGL 下成立，else 就是可达的那一支
            ok = parent_ok and not taken
            frame[2] = True
            reachable = ok
        elif directive == "endif":
            if stack:
                stack.pop()
            reachable = stack[-1][0] if stack else True
    return out


def build_pkg_index(cache_dir: str) -> dict[str, str]:
    """扫描 PackageCache，建 `包名 -> 实际目录` 的映射。

    **这个映射是必须的，不是优化。** Library/PackageCache 里的目录名带
    **版本哈希后缀**：

        com.unity.render-pipelines.core@789199009d13/
        com.unity.render-pipelines.universal@366bb53b7d8b/

    而 shader 里的 include 写的是**不带后缀**的逻辑路径：

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

    所以不能把 `Packages/<pkg>/...` 直接拼到 cache_dir 后面 ——
    少了 `@789199009d13`，于是每个 include 都判成「缺」，45 个官方
    shader 全部被拒绝覆盖，官方源码明明就在那里。

    踩这个坑的代价特别大：预检按设计是为了**阻止**坏覆盖，判错方向却是
    「全都别覆盖」，于是修复全程静默失效 —— 报告里写「已用官方源码覆盖：0」，
    而流程一路绿灯。包在不在、源码能不能用，本该是最容易确认的事。

    同名包多版本共存时选排序后的第一个，并在这里留下记录 ——
    静默挑一个而不说出来，就又是一次「判据失效但看起来正常」。
    """
    idx: dict[str, str] = {}
    try:
        entries = sorted(os.listdir(cache_dir))
    except OSError:
        return idx
    for name in entries:
        full = os.path.join(cache_dir, name)
        if not os.path.isdir(full):
            continue
        base = name.split("@", 1)[0]
        if base not in idx:
            idx[base] = full
        elif os.path.isfile(os.path.join(idx[base], "package.json")):
            pass          # 已有带 package.json 的，更可信
        else:
            idx[base] = full
    return idx


def missing_includes(src: str, cache_dir: str,
                     pkg_index: dict[str, str] | None = None) -> list[str]:
    """列出在包缓存里解析不到的 `#include`（**只看 WebGL 下会读到的**）。

    认三种写法，都是 URP/core 包内 shader 实际在用的：
      - ``Packages/<pkg>/<path>``  绝对逻辑路径（要补版本后缀）
      - ``<同名文件>``               同目录 / 同包内的相对引用
      - ``UnityCG.cginc``           core 包的传统名（无路径形式）

    为什么要逐个解析而不是「文件存在就复制」：官方 shader 依赖同包内的
    .hlsl，这些 .hlsl 又 include 更多的 .hlsl。只检查第一层会漏掉
    「Lit.hlsl 在，但 LitInput.hlsl 不在」这种。所以这里**递归**展开，
    把每一层都查一遍。

    **关键：递归必须带平台感知。** Common.hlsl 里有一整条
    ``#if defined(SHADER_API_GAMECORE) ... #elif defined(SHADER_API_GLES3)``
    的平台 API 分支链，WebGL 只走GLES3 那一支。早期版本用不带条件的
    ``INCLUDE.findall(text)`` 把10 条死分支全算进来，于是
    ``Universal Render Pipeline/Lit`（59 个材质引用）被判「include 不全」，
    45 个官方 shader 全部拒绝覆盖。现在改用 :func:`active_includes`。
    """
    cache_dir = os.path.abspath(cache_dir)
    if pkg_index is None:
        pkg_index = build_pkg_index(cache_dir)
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
        for inc in active_includes(text):
            if inc in seen:
                continue
            seen.add(inc)
            # 编辑器内置 CGIncludes 不在 PackageCache 里，Unity 自己注入。
            # 查不到不代表编译失败，判「缺」会误杀 FallbackError 这类shader。
            if os.path.basename(inc) in BUILTIN_NO_PATH:
                continue
            found = None
            # 1) Packages/<pkg>/<rest>：先查原样（无后缀的老布局），
            #    再按 pkg_index 补版本后缀 —— 真实 PackageCache 用后者。
            if inc.startswith("Packages/"):
                rel = inc[len("Packages/"):]
                head, _, tail = rel.partition("/")
                cands = [os.path.join(cache_dir, rel)]
                if head in pkg_index and tail:
                    cands.append(os.path.join(pkg_index[head], tail))
                found = next((c for c in cands if os.path.isfile(c)), None)
            if found is None:
                # 2) 相对当前目录
                found = os.path.normpath(os.path.join(cur_dir, inc))
                if not os.path.isfile(found):
                    found = None
            if found is None:
                # 3) 无路径形式（UnityCG.cginc 之类）：在包目录里按
                #    文件名找一份存在的。这条只是兜底，找不到就报缺。
                for base in pkg_index.values():
                    for dirpath, _dirs, files in os.walk(base):
                        if os.path.basename(inc) in files:
                            found = os.path.join(dirpath, os.path.basename(inc))
                            break
                    if found:
                        break
            if found:
                queue.append(found)
            else:
                missing.append(f"{os.path.relpath(cur, cache_dir)} -> {inc}")
    return missing


def _list_targets_offline() -> int:
    """离线推断覆盖目标：没有 PackageCache 时也能算出「应该覆盖谁」。

    判据是**官方 URP / core 包的 shader 命名规则**——包内 shader 一律以
    这几个前缀开头：
      `Universal Render Pipeline/`、`Universal Render Pipeline/`（Hidden）、
      `Shader Graphs/`、`Hidden/Shader Graph/`、`Hidden/Core/`、
      `Hidden/CoreSRP/`、`Hidden/TerrainEngine/Details/UniversalPipeline/`

    这里踩过一次：`Hidden/` 前缀最初漏在判据外，于是 59 个空壳被判成
    "游戏自研、官方包里不会有"，而实际上
    `Hidden/Universal Render Pipeline/Bloom`、`Hidden/Core/...`
    全都是 URP 包自带 shader。判据漏一个前缀，结论就整个反过来，
    而表面上只多打了几行名字。

    必须说清楚这个模式**不能证明什么**：它只按名字推断候选，没验证包内
    真有对应源码、也没验证 include 是否齐全。所以它用来回答「目标是谁」，
    不用来回答「覆盖会不会成功」—— 后者必须由 CI 里带 PackageCache 的
    完整路径来答。把两件事分开，是为了避免又出现「名字对上了就算成功」。
    """
    if not os.path.isdir(ASSETS_SHADER):
        print(f"::error::找不到 {ASSETS_SHADER}")
        return 1
    print(f"\n空壳总数扫描中：{ASSETS_SHADER}\n")
    candidates, other_stub = [], []
    all_stub = 0
    for f in sorted(os.listdir(ASSETS_SHADER)):
        if not f.endswith(".shader"):
            continue
        p = os.path.join(ASSETS_SHADER, f)
        if not is_stub(p):
            continue
        all_stub += 1
        name = shader_name(p)
        if name and any(name.startswith(pre) for pre in PACKAGE_PREFIXES):
            candidates.append((name, f))
        else:
            other_stub.append(name or f)

    print(f"空壳共 {all_stub} 个，其中按官方命名规则命中候选 {len(candidates)} 个")
    print("\n### 覆盖候选（URP / core 包自带 shader）")
    for name, f in candidates:
        print(f"  {name:52s} <- {f}")
    print(f"\n### 不是候选（游戏自研，官方包里不会有）{len(other_stub)} 个")
    print("     这些只能靠重建带分支的 shader 来修，补 pragma 无效。")
    print("     前 12 个：")
    for n in other_stub[:12]:
        print(f"  - {n}")
    if len(other_stub) > 12:
        print(f"  ... 另 {len(other_stub) - 12} 个")
    print("\n注意：本模式只回答「目标是谁」，**不能**回答「覆盖会不会成功」。")
    print("      include 是否齐全必须由带 PackageCache 的 CI 实测。")
    return 0


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
    # `--list-targets`：只算「哪些空壳能靠官方源码覆盖」，不碰文件。
    #
    # 为什么需要这个模式：没有 Library/PackageCache 时整个脚本原本直接
    # 早退，于是「覆盖目标是谁」这个问题**只有 CI 能回答** —— 而 CI 一轮
    # 好几十分钟，目标清单却从没被人看过一眼。加了这个模式，候选清单能
    # 在本地/评审里先算出来，也能对着已有的 CI 输出交叉核对。
    list_targets = "--list-targets" in sys.argv
    if not os.path.isdir(PACKAGE_CACHE):
        if list_targets:
            print("未找到 PackageCache —— 下面按**官方命名规则**推断覆盖目标。")
            return _list_targets_offline()
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
    # 包名 -> 真实目录（含 @版本后缀）。include 预检全靠它，
    # 少了它会把每个 `Packages/<pkg>/...` 都判成缺 —— 上一轮就是这么
    # 把 45 个官方 shader 全部误判掉、覆盖数变成 0 的。
    pkg_index = build_pkg_index(PACKAGE_CACHE)
    print(f"包内可用 shader：{len(by_name)}")
    print(f"包目录（名字带 @版本，include 要按这个匹配）：{len(pkg_index)}")
    for base in sorted(pkg_index):
        d = os.path.relpath(pkg_index[base], PACKAGE_CACHE)
        n = len(glob.glob(os.path.join(pkg_index[base], "**", "*.shader"),
                          recursive=True))
        mark = "" if n else "  <- 无 .shader 源码"
        print(f"  {base}  ({n} 个 .shader)  -> {d}{mark}")
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
    official_of: dict[str, str] = {}
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
        bad = missing_includes(src, PACKAGE_CACHE, pkg_index)
        if bad:
            blocked.append((name, bad))
            continue
        shutil.copyfile(src, stub)
        # 同时记住官方源路径：验收环节要拿它当基线。只记名字的话，
        # 后面想对比「官方有而覆盖后没了」就得再扫一遍包缓存，
        # 而那正是最容易悄悄拿到不同版本的地方。
        replaced.append(name)
        official_of[name] = src

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

    # ---- 判决行：覆盖到底有没有生效 ----
    #
    # 为什么要单独加这一段：`shutil.copyfile` 成功 **不等于** 覆盖有效。
    # 上面那些计数全部来自「我们打算做什么」，没有一个来自「做完之后
    # 文件真的变了」。这个项目已经被同一个教训反复咬过：报告里
    # `已用官方源码覆盖：N` 那行因为 head -50 被吃掉，于是「覆盖没生效」
    # 和「覆盖生效了」在报告里长得一模一样，只能靠猜。
    #
    # 所以这里做**事后复查**：重新读一遍每个目标文件，看 Stub 标记还在不在。
    # 判据极简且不依赖任何其它脚本的输出 ——
    #   还在  = 没生效（复制了、但写的不是同一件事）
    #   不在  = 生效
    # 把 `replaced` 和「复查仍为空壳」两者同时报出来，两个数不相等就是有鬼。
    still_stub = [n for n in replaced
                  if is_stub(next((p for p in stubs
                                   if shader_name(p) == n), ""))]
    # 三态而不是两态。原先只有「生效 / 未生效」，于是 `目标 0 个` 也报
    # 「生效」—— 而「一个都没覆盖」和「全都覆盖好了」在这套措辞下
    # 长得一模一样。这正是本项目最熟悉的那类失效：判据把「没做事」
    # 判成了「做成了」。所以 0 目标必须单独成态。
    if not replaced and not still_stub:
        verdict = "**未执行**（没有任何候选被覆盖）"
    elif still_stub:
        verdict = "**未生效**"
    else:
        verdict = "生效"
    print(f"\n### 判决：覆盖 {verdict}"
          f"（目标 {len(replaced)} 个，复查后仍为空壳 {len(still_stub)} 个）")
    if still_stub:
        # 这几个是「声称覆盖了、但文件里 Stub 标记还在」——
        # 上一轮 Universal Render Pipeline/Lit 就落在这一类里，
        # 而报告里完全看不出它异常。单独列名，别让它混在计数里。
        print("     以下 shader 声称已覆盖但仍是空壳（需人工查 sync 的复制逻辑）：")
        for n in still_stub:
            print(f"       ! {n}")
    if blocked:
        print(f"     另有 {len(blocked)} 个因 include 不全被拒（见上），"
              f"合计未生效 {len(still_stub) + len(blocked)} 个。")

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
    #
    # 判据本身也栽过一次（第九次静默失效）：早期用 `'"Queue"' in text` 判
    # Queue 存在，而官方真实的写法是**不带引号**的
    #     Tags { "RenderType" = "Opaque" "Queue" = "Geometry" ... }
    #                ^^^^^^ 带引号        ^^^^^ 不带引号
    # 于是 26 个刚覆盖成功的官方 shader 全被标成「缺 Queue / 缺 Blend」，
    # 输出看起来像严重问题，实际全假。假警报比没判据更糟：它让人不再相信
    # 验收这一节。同理 Blend/ZWrite 要认方括号形式 ——
    # `Blend [_SrcBlend][_DstBlend]` 是官方 Lit 的标准写法。
    #
    # 再往前一步：**拿官方源码自己当基线**。这一步的目标是「用官方源码
    # 覆盖」，那么官方源码就是正确性的上限，它没有的东西我们不该报缺。
    # 官方 `Hidden/.../Bloom.shader` 里真的只有
    #     ZTest Always ZWrite Off Cull Off
    # 没有 Blend —— 因为它是全屏 RT blit，本来就不需要混合。报「缺 Blend」
    # 是把官方设计说成缺陷。判据改成「相对官方基线的回退」：
    #   OK        官方有、覆盖后也有
    #   官方即无官方源码本身就没有，不算问题（写明，别让人误判）
    #   回退      官方有、覆盖后没了 —— 这才是真事故
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
            now = _strip_comments(f.read())
        official = official_of.get(name)
        # 基线必须是**包内**的源，不是刚被覆盖写过的桩。
        # 曾经栽在这里：`official_of` 存成了桩路径，于是「基线」和「当前」
        # 读的是同一个文件，判据却报出 26 个「回退」——覆盖是逐字节复制，
        # 字节都相同怎么可能回退？这种自相矛盾的输出一旦出现，就说明基线取错，
        # 必须当场崩掉而不是继续打印。断言比注释管用。
        if official and os.path.abspath(official) == os.path.abspath(stub):
            raise AssertionError(
                f"基线取到了桩文件本身：{official}\n"
                f"覆盖是 shutil.copyfile 的逐字节复制，当前内容与基线必然相同，"
                f"此时报出任何「回退」都是判据错误。"
            )
        base = _strip_comments(
            open(official, encoding="utf-8", errors="replace").read()
        ) if official else None
        # 第二道断言：内容层面自检。逐字节复制意味着两边必须完全相等，
        # 一旦不等就说明「基线」并不是真正被复制的那份文件 —— 而此时判据
        # 报出的任何「回退」都是假的。踩过两次才补上这道：
        # 第一次基线取成桩路径（路径断言能抓），
        # 第二次是别的文件（只能靠内容抓）。
        if base is not None and base != now:
            raise AssertionError(
                f"基线与覆盖结果内容不一致：{name}\n"
                f"  覆盖后：{stub}（{len(now)} 字符）\n"
                f"  基线  ：{official}（{len(base)} 字符）\n"
                f"  两者本应逐字节相同（shutil.copyfile）。判据据此报出的"
                f"「回退」全是假的，先修基线来源，别去改判据。"
            )

        def _t(txt, key):
            return _tag_present(txt, key) if txt is not None else None

        def _s(txt, key):
            return _state_present(txt, key) if txt is not None else None

        # Tags 键：ShaderLab 允许 "Queue" 和 Queue 两种写法，都要认
        has_queue, ref_queue = _t(now, "Queue"), _t(base, "Queue")
        has_rt, ref_rt = _t(now, "RenderType"), _t(base, "RenderType")
        # 渲染状态：字面量（Blend One Zero）与材质参数
        # （Blend [_SrcBlend][_DstBlend] / ZWrite [_ZWrite]）两种都算存在
        has_blend, ref_blend = _s(now, "Blend"), _s(base, "Blend")
        has_zwrite, ref_zwrite = _s(now, "ZWrite"), _s(base, "ZWrite")
        passes = count_passes(now)
        flags = []
        for label, got, ref in (("Queue", has_queue, ref_queue),
                                ("RenderType", has_rt, ref_rt),
                                ("Blend状态", has_blend, ref_blend),
                                ("ZWrite状态", has_zwrite, ref_zwrite)):
            # 只有「官方基线里有、覆盖后没了」才是真回退。
            #
            # 这里栽过一次很典型的错：把 `ref is False`（官方源码本来就没有）
            # 当成「官方有」去报警，于是官方 Lit 报出「官方有 Queue，覆盖后
            # 没了」——而它根本没有 Queue tag，只有 _QueueOffset 属性。
            # 26 个刚覆盖成功的文件全在报警，输出看着像严重事故，全是假的。
            # 覆盖是 shutil.copyfile 逐字节复制，字节相同不可能回退；
            # 出现这种自相矛盾的结论，先怀疑判据，别急着改资产。
            if got or not ref:
                continue
            flags.append(f"回退：官方有 {label}，覆盖后没了")
        mark = "OK" if not flags else "; ".join(flags)
        print(f"  {name}: Queue={'Y' if has_queue else 'N'} "
              f"RenderType={'Y' if has_rt else 'N'} "
              f"Blend={'Y' if has_blend else 'N'} "
              f"ZWrite={'Y' if has_zwrite else 'N'} "
              f"Pass数={passes}  -> {mark}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
