#!/usr/bin/env python3
"""修正 AssetRipper 空壳 shader 的属性名不匹配（纹理 + 颜色）。

## 问题

AssetRipper 无法反编译时写的空壳，正文里用的是**内置 Standard 的
属性名** `_MainTex` 和 `_Color`：

    Texture2D<float4> _MainTex;
    SamplerState sampler_MainTex;
    float4 _Color;
    ...
    return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;

而 URP 的材质存的是 `_BaseMap` / `_BaseColor`。两个名字对不上，
于是正文读到的永远是 Properties 里的默认值（纹理 `"white"`），
**所有物体渲成纯白** —— 地形、水面、场景里的一切都没有贴图。

注意这不是洋红，是比洋红更隐蔽的失败：材质和 shader 都在，
构建成功、CI 全绿，只是画面全白。

## 实测数据（不是推测）

`Universal Render Pipeline/Lit` 被 **59 个材质**引用
（`m_Shader` GUID = `214af55c50256cf489d5d1c7b1c4edaa`）：

| 属性 | 有贴图 | 空引用 | 字段缺失 |
|------|--------|--------|----------|
| `_BaseMap` | 37 | 22 | 0 |
| `_MainTex` | 5 | 52 | 2 |

**关键的安全性判据**：那5 个 `_MainTex` 有贴图的材质，
`_BaseMap` 全都指向**同一个 GUID**（`af977919` / `e9db4f78`）。
也就是说**零个材质存在「`_MainTex` 有贴图但 `_BaseMap` 为空」**
—— 所以改读 `_BaseMap` 不会让任何原本能显示贴图的材质变成纯白，
是严格改进而非取舍。

颜色同理：`_BaseColor` 59 个全设；`_Color` 虽然也 59 个全设，
但其中 **19 个与 `_BaseColor` 取值不同** —— 读错的后果是
**颜色错**（不是全白，更难看出）。

## 为什么只改正文、不动 Properties

Properties 里那两行

    [HideInInspector] _MainTex ("BaseMap", 2D) = "white" {}
    [HideInInspector] _Color ("Base Color", Vector) = (1,1,1,1)

是 AssetRipper 加的**兼容别名**，Unity 靠它把旧名映射到新名，
对编辑器不生效但无害 —— 保留。真正要改的是**声明与采样**，
否则材质设的值根本没人读。`_MainTex_ST` / `sampler_MainTex`
也要跟着改名，否则用的是另一个变量的默认值。

这是纯属性名替换：不改采样次数、不改 UV 计算、不改返回表达式，
只把「读哪个变量」对齐到材质实际设置的那个。

## 判据（三条各自独立，命中任意一条就改）

1. Properties 声明 `_BaseMap` 且正文采样 `_MainTex` -> 改纹理名
2. Properties 声明 `_BaseColor` 且正文用 `_Color` -> 改颜色名
3. 上面两条在 `Universal Render Pipeline/Particles/*` 上只命中第2 条
   （它们不采 `_MainTex`），但同样是 bug —— 判据必须拆开，否则会漏。

实测 5 个 URP 空壳全部命中：

| shader |材质引用 | 命中判据 |
|--------|---------|----------|
| `Universal Render Pipeline/Lit` | 59 | 1 + 2 |
| `Universal Render Pipeline/Unlit` | 1 | 1 + 2 |
| `Universal Render Pipeline/Particles/Lit` | 3 | 2 |
| `Universal Render Pipeline/Particles/Simple Lit` | 0 | 2 |
| `Universal Render Pipeline/Particles/Unlit` | 0 | 2 |

其它空壳（VFX / Hidden/*）是 Built-in 风格，Properties 里没有
`_BaseMap` / `_BaseColor`，两条判据都不命中，不会被碰。

## 为什么不用 `\b` 词边界

踩过：`\b` 在这里恰好漏掉最该改的两个标识符。原因是 Python 的
`\w` **包含下划线**，于是

    sampler_MainTex   前一个字符 'r' 是 \w  -> \b 不成立 -> 漏
    _MainTex_ST       后一个字符 '_' 是 \w  -> \b 不成立 -> 漏
    _Color            独立标识符            -> 命中

也就是说 `\b` 只认得「独立出现的 `_MainTex`」，而**正文里根本没有
独立出现的 `_MainTex`** —— HLSLcc 导出时必然带上 `sampler_` 前缀和
`_ST` 后缀。结果就是改了 4 行、漏了 2 行，shader 照样编译失败。

所以改成**按完整标识符集合精确匹配**：先把正文里所有标识符切出来，
逐个查表决定改名，不做任何基于边界的模糊替换。这样
`_Color` 不会误伤 `_ColorMask`、`_MainTex_ST` 不会被漏，
`_BaseColor`（已是目标名）也不会被二次改成 `_BaseBaseColor`。

## 用法

    python3 tools/fix_stub_basemap.py    # 改写 Assets/Shader 下命中的空壳
    python3 tools/fix_stub_basemap.py --check   # 只报不改（CI 用）
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SHADER_DIR = os.path.join(ROOT, "Assets", "Shader")
STUB_MARKER = "DummyShaderTextExporter"
SHADER_NAME = re.compile(r'^\s*Shader\s+"([^"]+)"', re.M)

# Properties 里的属性声明行：`_BaseMap ("Albedo", 2D) = "white" {}`
# 或带属性标签：`[HideInInspector] _MainTex ("BaseMap", 2D) = "white" {}`
PROP_LINE = re.compile(r"^\s*(?:\[.*?\]\s*)*_(\w+)\s*\(")
# 标识符（含 `sampler_` 前缀，这是 `\b` 漏匹配的根源）
IDENT = re.compile(r"[A-Za-z_]\w*")


def prop_names(raw: str) -> set:
    """提取 Properties 块里声明的属性名。

    必须行级扫描、遇到行首 `}` 才结束 —— 不能用 `split('}', 1)`：
    Properties 里有嵌套括号（`_BaseMap ("Albedo", 2D) = "white" {}`），
    按第一个 `}` 切会得到半截块，属性名全丢，判据静默失效。
    """
    names: set = set()
    inside = False
    for ln in raw.splitlines():
        if re.match(r"^\s*Properties\s*\{?\s*$", ln):
            inside = True
            continue
        if inside:
            if re.match(r"^\s*\}", ln):
                inside = False
                continue
            m = PROP_LINE.match(ln)
            if m:
                names.add(m.group(1))
    return names


def plan(raw: str):
    """算出要改哪些标识符 -> 新名，以及命中的判据编号。返回 None 表示不改。"""
    if STUB_MARKER not in raw:
        return None
    props = prop_names(raw)

    # 正文从 HLSLPROGRAM 起 —— Properties 块里的同名声明不该动。
    # 这一步让「按标识符精确匹配」可以覆盖整个文件而无需再逐行判断。
    m = re.search(r"HLSLPROGRAM(.*?)ENDHLSL", raw, re.S)
    if not m:
        return None
    body = m.group(1)

    rename: dict = {}
    hit: list = []

    # 判据 1：纹理。_MainTex / sampler_MainTex / _MainTex_ST -> _BaseMap 系
    if "BaseMap" in props and re.search(r"MainTex", body):
        for name in ("_MainTex_ST", "sampler_MainTex", "_MainTex"):
            if re.search(rf"(?<![\w]){name}(?![\w])", body):
                rename[name] = name.replace("MainTex", "BaseMap")
        hit.append("1")

    # 判据 2：颜色。_Color -> _BaseColor
    if "BaseColor" in props and re.search(r"(?<![\w])_Color(?![\w])", body):
        rename["_Color"] = "_BaseColor"
        hit.append("2")

    if not rename:
        return None
    return rename, hit


def fix(path: str, check_only: bool = False) -> bool:
    raw = open(path, encoding="utf-8", errors="replace").read()
    got = plan(raw)
    if got is None:
        return False
    rename, hit = got

    m = re.search(r"HLSLPROGRAM(.*?)ENDHLSL", raw, re.S)
    b0, b1 = m.start(1), m.end(1)
    body = raw[b0:b1]

    # 按标识符精确替换：先把标识符切出来逐个查表，再拼回。
    # 不用正则做边界匹配（见文档「为什么不用 \b 词边界」）。
    def sub(mo: re.Match) -> str:
        return rename.get(mo.group(0), mo.group(0))

    new_body = IDENT.sub(sub, body)
    changed = sum(1 for a, b in zip(IDENT.findall(body),
                                   IDENT.findall(new_body)) if a != b)
    if not changed:
        return False

    name = SHADER_NAME.search(raw)
    tag = name.group(1) if name else "?"
    detail = ", ".join(f"{k}->{v}" for k, v in sorted(rename.items()))
    if check_only:
        print(f"  [CHECK] {tag}: 需改 {changed} 处（{detail}）"
              f"  ({os.path.basename(path)})")
        return True
    with open(path, "w", encoding="utf-8") as f:
        f.write(raw[:b0] + new_body + raw[b1:])
    print(f"  {tag}: 改 {changed} 处（判据 {'+'.join(hit)}，{detail}）"
          f"  ({os.path.basename(path)})")
    return True


def main() -> int:
    check_only = "--check" in sys.argv
    if not os.path.isdir(SHADER_DIR):
        print(f"::error::没找到 {SHADER_DIR}")
        return 1
    n = 0
    for f in sorted(glob.glob(os.path.join(SHADER_DIR, "*.shader"))):
        if fix(f, check_only):
            n += 1
    verb = "需修正" if check_only else "修正"
    print(f"\n{verb} {n} 个空壳的属性名（_MainTex -> _BaseMap，_Color -> _BaseColor）")
    if n == 0:
        # 静默 0 个可能意味着判据失效了 —— 而判据失效的表现跟「没问题」
        # 一样，正是最该避免的。所以明确说清这一轮为什么是 0。
        print("（若预期至少 5 个，说明判据与实际文件形态不符，"
              "需要重新看空壳长什么样 —— 别把 0 当成「没问题」。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
