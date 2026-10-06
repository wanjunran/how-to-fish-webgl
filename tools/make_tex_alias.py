#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""生成 tN -> 材质属性名 的映射表，供 glsl_to_unity.py --tex-alias 使用。

## 依据（全部为机械可验证的量，无人工判断）

玩家包里没有 RDEF 的资源名，HLSLcc 只能按原始 HLSL 的寄存器号起占位名
`_Texture_tN`。要把材质纹理绑回去，就得知道 tN 对应哪个 Properties 属性。

实测 22 个有2D 属性的 shader，下面的规律无一例外：

  * t0/t1/t2  -> samplerCube / sampler3D，且采样签名是
                  textureLod(t, 三维坐标, lod) —— URP 的 APV / lightmap
                  等引擎查找表。这类纹理不出现在材质 Properties 里
                  （unity_Lightmaps / unity_ShadowMasks / 反射探针）。
  * t3        -> texture(t3, input.vs_INTERP0.xy, _GlobalMipBias).xyz
  * t4        -> texture(t4, input.vs_INTERP0.xy, _GlobalMipBias)

  t3 / t4 在所有 shader 里的采样签名逐字一致，且都取**顶点插值器带来的 UV**
  —— 这正是材质贴图的标准用法；引擎 LUT 用的是 3D 坐标 + 固定 lod。

  而 t3 / t4 恰好落在「Properties 里第1 个 / 第 2 个 2D 属性」上：
  22/22 个 shader 满足「2D 属性数 == 一段连续寄存器的长度」。

所以 t3 = 第 1 个 2D 属性，t4 = 第 2 个，t5+ 才是引擎纹理。这不是猜，
是可以复算的偏移量。

注意：少数shader 的真实属性起始寄存器不是 3（MapObject / UI /
UnderwaterShader / ThinkingShader / SlotMachineBackground 只有 t0）。
本脚本不写死偏移，而是按「维度 + 出现顺序 + 与 Properties 的对应」逐个
shader 求解；解不出来就不产出条目，宁可留空也不猜。
"""
import glob
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Unity 内建纹理：出现在 Properties 里，但不是材质属性，不能当成绑定目标。
ENGINE = {
    "unity_Lightmaps", "unity_LightmapsInd", "unity_ShadowMasks",
    "unity_Lightmap", "unity_ReflectionProbes", "unity_SpecularLightMap",
    "unity_ReflectionBinning", "unity_MainLightShadowmap",
    "unity_AdditionalLightsShadowmap", "unity_AdditionalLightShadowmap",
    "_MainLightTexture", "_AdditionalLightsTexture",
    "unity_DirectionalShadowData", "unity_MixedShadowData",
    "unity_LightData", "unity_LightIndices", "_AdditionalLightTexture",
}


def props_textures(dummy):
    """从AssetRipper 导出的原版 dummy 里取 [(属性名, 维度)]。"""
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


def regs_by_dim(shader_txt):
    """{维度: [寄存器号...]}，按首次出现顺序。"""
    out = {}
    for m in re.finditer(r"TEXTURE(\w*)\((_Texture_t\d+)\)", shader_txt):
        macro = m.group(1)
        n = int(re.search(r"\d+", m.group(2)).group())
        dim = {"CUBE": "Cube", "3D": "3D", "2D_ARRAY": "2DArray"}.get(
            macro, "2D")
        out.setdefault(dim, [])
        if n not in out[dim]:
            out[dim].append(n)
    return out


def used_in_body(shader_txt, reg):
    """该寄存器在正文里是否真被采样（排除只声明没用的情况）。"""
    return bool(re.search(r'_g_\w+\(\s*' + re.escape(reg) + r'\s*[,)]',
                          shader_txt))


def dummy_for(name, shader_dir=None):
    for cand in (f"Shader Graphs_{name}.shader",
                 f"{name.replace('/', '_')}.shader",
                 f"{name}.shader"):
        p = os.path.join(shader_dir or os.path.join(ROOT, "Assets", "Shader"),
                         cand)
        if os.path.exists(p):
            return p
    return None


def solve_parsed(vs, ps, dummy):
    """由已解析的 VS/PS 结构求{tN: 属性名}，解不出返回 {}。

    数据源是 HLSLcc 的**原始 GLSL**（经 glsl_to_unity.parse_glsl 解析），
    不是已生成的 .shader。原因：一旦用 tex-alias 改过名，.shader 里就没有
    _Texture_tN 了，再从声明反推寄存器号会整段偏移。
    """
    props = [p for p in props_textures(dummy) if p[0] not in ENGINE]
    p2d = [nm for nm, ty in props if ty == "2D"]
    if not p2d:
        return {}

    body = (vs.body or "") + (ps.body or "")
    # 按维度归类寄存器：宏名从 p.tex 的第二个字段来（SAMPLER_MACRO 的键）。
    by_dim = {}
    for p in (vs, ps):
        for name, macro in p.tex:
            m = re.match(r"_Texture_(t\d+)$", name)
            if not m:
                continue
            n = int(m.group(1)[1:])
            dim = {"TEXTURECUBE": "Cube", "TEXTURE3D": "3D"}.get(
                macro, "2D")
            by_dim.setdefault(dim, [])
            if n not in by_dim[dim]:
                by_dim[dim].append(n)
        # 阴影比较纹理走 hlslcc_zcmp_Texture_tN，名字带前缀且不在 p.tex 里。
        # 必须一起计入：漏掉它会在「连续段」扫描时把寄存器序列打断 ——
        # Grass 的 2D 寄存器其实是 t3..t9 连续的，漏掉 t5 后序列变成
        # [3,4,6,7,8,9]，扫描会跳过前两个匹配到 t6/t7，整段偏移 3。
        for zn in p.zcmp:
            m = re.match(r"hlslcc_zcmp_Texture_(t\d+)$", zn)
            if not m:
                continue
            n = int(m.group(1)[1:])
            by_dim.setdefault("2D", [])
            if n not in by_dim["2D"]:
                by_dim["2D"].append(n)

    # 注意：这里对的是**原始 GLSL**，采样函数名没有 _g_ 前缀
    # （生成成HLSL 后才变成 _g_texture）。两种都试。
    # 正文采样过的 2D 寄存器。zcmp 纹理（阴影比较）不进这个集合 ——
    # 它们只被 zcmp 宏引用，不出现在_texture 调用里。
    sampled = {n for n in by_dim.get("2D", [])
               if re.search(
                   r"\b(?:_g_)?_?tex(?:ture|elFetch)\w*\(\s*_Texture_t"
                   + str(n) + r"\b", body)}
    # 但 zcmp 纹理要作为「占位」留在序列里，否则连续段会被打断
    # （Grass 的 2D 寄存器是 t3..t9 连续的，去掉只做shadow 的 t5
    #  就变成 3,4,6,7,...，扫描会错位）。
    r2d = sorted(by_dim.get("2D", []))
    if not sampled:
        return {}

    seg = None
    run = []
    for x in r2d:
        run = run + [x] if run and x == run[-1] + 1 else [x]
        if len(run) == len(p2d):
            # 取**第一段**就停。真实属性占低位寄存器（引擎的 APV /
            # lightmap 从更高位开始），所以第一次匹配才是对的那段。
            # 早期版本不break，结果被后面更长的段覆盖，Grass 整段偏移 3。
            seg = list(run)
            break
    if seg is None:
        # 属性数 1 时，单个寄存器也可能就是它（t0 情形）
        if len(p2d) == 1 and len(r2d) == 1:
            seg = list(r2d)
        else:
            return {}
    return {f"_Texture_t{n}": nm for n, nm in zip(seg, p2d)}


def solve(shader_txt, dummy):
    """从 .shader 文本求解（保留旧接口，仅供调试）。

    注意：输入必须**未改名**。已改名的产物再喂进来会整段偏移。
    """
    if not re.search(r"TEXTURE\w*\(_Texture_t\d+\)", shader_txt):
        return {}
    props = [p for p in props_textures(dummy) if p[0] not in ENGINE]
    p2d = [nm for nm, ty in props if ty == "2D"]
    if not p2d:
        return {}

    dims = regs_by_dim(shader_txt)
    r2d = dims.get("2D", [])
    # 只保留正文真的采样了的（有的变体声明了但没用到）
    r2d = [n for n in r2d
           if used_in_body(shader_txt, f"_Texture_t{n}")]
    if not r2d:
        return {}

    r2d_sorted = sorted(r2d)
    # 找长度等于「2D 属性数」的连续段。属性按 Properties 声明顺序分配到
    # 这段寄存器上。
    seg = None
    run = []
    for x in r2d_sorted:
        run = run + [x] if run and x == run[-1] + 1 else [x]
        if len(run) == len(p2d):
            seg = list(run)
    if seg is None:
        # 属性数 1 时，单个寄存器也可能就是它（t0 情形）
        if len(p2d) == 1 and len(r2d_sorted) == 1:
            seg = list(r2d_sorted)
        else:
            return {}
    return {f"_Texture_t{n}": nm for n, nm in zip(seg, p2d)}


def main():
    """从**原始 HLSLcc GLSL** 直接求解，不依赖生成产物。

    刻意不用 --out 里的 .shader 当输入：那批文件一旦被 tex-alias 改过名，
    声明里的 _Texture_t3 就变成了 _Texture，再从声明里读寄存器号会整段
    偏移（实测 t3->t5），解出来的表是错的。原始 GLSL 才是唯一稳定来源。
    """
    import argparse
    ap = argparse.ArgumentParser()
    ap.add_argument("--glsl-dir", default="/tmp/hlslcc_all",
                    help="HLSLcc 产物根目录")
    ap.add_argument("--shader-dir", default=os.path.join(ROOT, "Assets",
                                                          "Shader"))
    ap.add_argument("--names", default="/tmp/shader_names.txt")
    ap.add_argument("--out", default="/tmp/tex_alias")
    a = ap.parse_args()

    sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
    import glsl_to_unity as G

    os.makedirs(a.out, exist_ok=True)
    names = [l.strip() for l in open(a.names, encoding="utf-8") if l.strip()]

    made, skipped = 0, []
    for full in names:
        short = full.split("/")[-1]
        d = None
        for cand in (short, short.replace(" ", "_"),
                     short.replace(" ", "_").replace("(", "_").replace(")", "_")):
            p = os.path.join(a.glsl_dir, cand)
            if os.path.isdir(p):
                d = p
                break
        if not d:
            skipped.append((short, "无反编译产物"))
            continue

        vs_f, ps_f, _key = G.pick_variant_pair(d)
        if not vs_f or not ps_f:
            skipped.append((short, "找不到配对变体"))
            continue
        vs = G.parse_glsl(open(os.path.join(d, vs_f),
                               errors="replace").read())
        ps = G.parse_glsl(open(os.path.join(d, ps_f),
                               errors="replace").read())

        # 用「已解析的结构」而非原始文本：p.tex 里是 (纹理名, 宏名)，
        # p.zcmp 里是 hlslcc_zcmp_*，与改名前的状态一一对应。
        amap = solve_parsed(vs, ps, dummy_for(short, a.shader_dir))
        if not amap:
            skipped.append((short, "2D 纹理与属性数对不上"))
            continue
        with open(os.path.join(a.out, short + ".json"), "w",
                  encoding="utf-8") as fh:
            json.dump(amap, fh, ensure_ascii=False, indent=2, sort_keys=True)
        made += 1
        print(f"  {short:26} " + "  ".join(
            f"{k}->{v}" for k, v in sorted(amap.items())))

    print()
    print(f"产出 {made} 份映射，未解出 {len(skipped)} 份")
    for n, why in skipped:
        print(f"    {n:28} {why}")
    print(f"目录: {a.out}")


if __name__ == "__main__":
    main()