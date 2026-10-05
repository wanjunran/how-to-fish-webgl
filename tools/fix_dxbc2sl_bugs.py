#!/usr/bin/env python3
"""修正 dxbc2sl 翻译器的已知输出缺陷。

背景：dxbc2sl 是面向 GLSL 的翻译器（自带 asuint/asint/floatBitsToInt 等辅助
函数），产物放到 Unity 的 HLSLPROGRAM 里会撞上 HLSL 的语义差异。已确认的
一类致命缺陷：把 DXBC 的立即数向量渲染成对标量取分量，例如

    r3.zw = asfloat(asuint(r3.xy) >> asuint(int(5).zw));   // int 是标量，哪来的 .zw

Unity 会报：'asuint': no matching 1 parameter intrinsic function / invalid
subscript 'zw'，shader 编译失败后退化为 error shader（品红）。

正确写法以 Unity 官方 HLSLcc 的同段产出为准：
    u_xlatu3.xy = uvec2(u_xlatu3.x >> 5u, u_xlatu3.y >> 5u);
即移位量是标量常数。HLSL 里 uint2 >> uint 会按分量展开，等价。

本脚本只做正则层面的机械修正，不改动任何运算语义。

用法:
    python3 tools/fix_dxbc2sl_bugs.py <文件> [<文件> ...]     # 直接改
    python3 tools/fix_dxbc2sl_bugs.py <文件> --dry            # 只看不改
"""
import re
import sys

# (正则, 替换, 说明)
RULES = [
    # asuint(int(5).zw) -> 5u
    (re.compile(r"asuint\(int\((-?\d+)\)\.[xyzw]+\)"), r"\1u",
     "立即数取分量 -> uint 标量"),
    # asint(int(5).zw) -> 5
    (re.compile(r"asint\(int\((-?\d+)\)\.[xyzw]+\)"), r"\1",
     "立即数取分量 -> int 标量"),
    # asfloat(int(5).zw) -> float(5)
    (re.compile(r"asfloat\(int\((-?\d+)\)\.[xyzw]+\)"), r"float(\1)",
     "立即数取分量 -> float 标量"),
    # DXBC 的 ret 被直译成无值 return，会让外层包好的 return o0 变成死代码，
    # Unity 报 "'frag': function must return a value"。
    (re.compile(r"(?m)^(\s*)return;\s*$"), r"\1return o0;",
     "裸 return -> return o0"),
    # float(5).zw -> float2(5, 5)。注意：只处理 2 个及以上分量 —— 单分量的
    # float(5).x 在 HLSL 里本来就合法，改写成 float1(...) 反而会报错。
    (re.compile(r"float\((-?[\d.eE+-]+)\)\.([xyzw]{2,4})"),
     lambda m: f"float{len(m.group(2))}({', '.join([m.group(1)] * len(m.group(2)))})",
     "裸立即数取分量 -> 显式向量"),
]


def fix(src):
    hits = []
    for rx, rep, desc in RULES:
        src, n = rx.subn(rep, src)
        if n:
            hits.append((desc, n))
    return src, hits


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    dry = "--dry" in sys.argv
    if not args:
        print(__doc__)
        return 2
    total = 0
    for path in args:
        src = open(path, encoding="utf-8").read()
        out, hits = fix(src)
        if not hits:
            print(f"{path}: 无需修正")
            continue
        total += sum(n for _, n in hits)
        print(f"{path}: " + ", ".join(f"{d} x{n}" for d, n in hits))
        if not dry:
            open(path, "w", encoding="utf-8").write(out)
    print(f"共修正 {total} 处" + ("（dry run，未写入）" if dry else ""))
    return 0


if __name__ == "__main__":
    sys.exit(main())
