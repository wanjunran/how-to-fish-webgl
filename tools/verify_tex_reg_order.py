"""验证「按 Properties 声明顺序分配寄存器号」这个假设。

思路
----
HLSLcc 拿不到资源名，只能用原始 HLSL 的寄存器号起占位名。若 Unity 在编译
Shader Graph 时是**按 Properties 里的声明顺序**依次分配寄存器号（t0, t1,
t2, ...），那么 tN 与属性之间就是纯序关系，可以机械还原名字。

但寄存器是分「维度」独立编号的（2D / Cube / 3D 各有自己的 t 序列），
而且引擎内建纹理（lightmap / shadowmap / reflection probe）也占号。所以
要验证的是：**排除掉引擎纹理与其它维度之后，剩下的 2D 纹理按序能否对上
Properties 里的 2D 属性**。

判据：如果这个假设成立，多个 shader 都会呈现一致的偏移规律；如果不成立，
不同 shader 的偏移会乱跳。据此可以决定要不要用它生成 alias 表。
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Unity 内建纹理的固定名字。它们出现在 Properties 里时不是材质属性。
ENGINE = {
    "unity_Lightmaps", "unity_LightmapsInd", "unity_ShadowMasks",
    "unity_Lightmap", "unity_ReflectionProbes", "unity_SpecularLightMap",
    "unity_ReflectionBinning", "unity_MainLightShadowmap",
    "unity_AdditionalLightsShadowmap", "unity_AdditionalLightShadowmap",
    "_MainLightTexture", "_AdditionalLightsTexture",
    "_ScreenParams",  # 不是纹理，防误伤
}


def props_textures(dummy):
    """返回 [(属性名, 维度)]，维度为 2D / Cube / 3D / 2DArray。"""
    if not dummy or not os.path.exists(dummy):
        return []
    t = open(dummy, encoding="utf-8", errors="replace").read()
    i = t.find("Properties")
    if i < 0:
        return []
    j = t.find("{", i)
    d, k = 1, j + 1
    while d and k < len(t):
        if t[k] == "{":
            d += 1
        elif t[k] == "}":
            d -= 1
        k += 1
    body = t[j + 1:k - 1]
    out = []
    for m in re.finditer(
            r'"?([A-Za-z_]\w*)"?\s*\(\s*"[^"]*"\s*,\s*'
            r'(2D|Cube|3D|2DArray|Cubemap|RenderTexture)\s*\)', body):
        out.append((m.group(1), m.group(2)))
    return out


def used_regs(shader_txt):
    """{维度: [寄存器号...]}，按声明出现顺序。"""
    out = {}
    for m in re.finditer(r"TEXTURE(\w*)\((_Texture_t\d+)\)", shader_txt):
        macro, name = m.group(1), m.group(2)
        n = int(re.search(r"\d+", name).group())
        # TEXTURE2D / TEXTURE2D_SHADOW -> 2D；TEXTURECUBE -> Cube；
        # TEXTURE3D -> 3D
        dim = {"CUBE": "Cube", "3D": "3D", "2D_ARRAY": "2DArray"}.get(
            macro, "2D")
        out.setdefault(dim, [])
        if n not in out[dim]:
            out[dim].append(n)
    return out


def dummy_for(name):
    for cand in (f"Shader Graphs_{name}.shader", f"{name}.shader",
                 f"{name.replace('/', '_')}.shader"):
        p = os.path.join(ROOT, "Assets", "Shader", cand)
        if os.path.exists(p):
            return p
    return None


def main():
    src = sys.argv[1] if len(sys.argv) > 1 else "/tmp/restored4"
    rows = []
    for f in sorted(glob.glob(os.path.join(src, "*.shader"))):
        name = os.path.basename(f)[:-7]
        txt = open(f, encoding="utf-8", errors="replace").read()
        regs = used_regs(txt)
        props = [p for p in props_textures(dummy_for(name))
                 if p[0] not in ENGINE]
        p2d = [p for p in props if p[1] == "2D"]
        if not p2d or "2D" not in regs:
            continue
        # 只看序号连续的那一段（末尾连续段通常才是真实属性）
        r2d = sorted(regs["2D"])
        rows.append((name, r2d, [p[0] for p in p2d]))

    print(f"{'shader':20} {'2D 寄存器(升序)':34} {'2D 属性(声明序)':40} 连续段")
    print("-" * 110)
    ok_cn = 0
    for name, r2d, p2d in rows:
        # 从最小寄存器号起，找长度 == len(p2d) 的连续段
        run = []
        seg = None
        for x in r2d:
            if run and x == run[-1] + 1:
                run.append(x)
            else:
                run = [x]
            if len(run) == len(p2d):
                seg = list(run)
        tail = seg if seg else []
        mark = "是" if tail else "否"
        if tail:
            ok_cn += 1
        print(f"{name:20} {str(r2d):34} {str(p2d):40} {mark}")
    print()
    print(f"2D 属性数 == 连续寄存器段长度的 shader: {ok_cn}/{len(rows)}")
    print()
    print("解读：若多数为「是」，说明真实属性占一段连续寄存器，")
    print("      按序配对可机械还原名字（--tex-alias 的数据来源）；")
    print("      若多为「否」，说明寄存器分配与 Properties 顺序无关，")
    print("      该思路作废。")


if __name__ == "__main__":
    main()