#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""核对材质设置的属性与shader 声明是否对得上。

两个方向都要查，缺任何一边材质都绑不上：

  1. 材质里写了值，但 shader 既没在 Properties 里声明、也没在 HLSL 里
     声明成 uniform —— 静默丢失，Unity 不报错。
  2. shader 的 Properties 里有属性，但材质没设值 —— 会用默认值，
     通常无害，但如果默认值是0 而原材质有值，画面就不对。

第2种没法从「材质 vs shader」两方看出来，因为材质文件可能只存了
它用到的那些。这里只报第 1 种，那才是真问题。
"""
import glob
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def shader_props_and_uniforms(path):
    """返回 (Properties 里的属性名, HLSL 里声明的 uniform 名)。

    Properties 的每一行是
        [HDR] _Name ("Display Name", Vector) = (...)
        [ToggleUI] _Name ("x", Float) = 0
        [NoScaleOffset] _Name ("x", 2D) = "white" {}
    所以只认「行首（允许前置属性标记）_Name (」这一种形态 —— 早先用
    宽松的 `\[?\w*\s*\]?\s*"?(\w+)"?\s*\(` 会把 = (0,0,0,0) 里的
    数字、Color_93e06cd5... 里的十六进制片段当成属性名。
    """
    t = open(path, encoding="utf-8", errors="replace").read()
    props = set()
    i = t.find("Properties")
    if i >= 0:
        j = t.find("{", i)
        d, k = 1, j + 1
        while d and k < len(t):
            if t[k] == "{":
                d += 1
            elif t[k] == "}":
                d -= 1
            k += 1
        body = t[j + 1:k - 1]
        for m in re.finditer(
                r'^\s*(?:\[[^\]]*\]\s*)*"?([A-Za-z_]\w*)"?\s*\(',
                body, re.M):
            props.add(m.group(1))
    uni = set(re.findall(r"^\s*(?:cbuffer\s+\w+\s*\{)?\s*[\w]+\s+(\w+)\s*;",
                         t, re.M))
    return props, uni


def material_props(path):
    """从 .mat (YAML) 里读 m_SavedProperties 下设置的属性名。

    AssetRipper 导出的格式是：
        m_SavedProperties:
          m_TexEnvs:
            _BaseMap:                <- 6 空格缩进，无 "- " 前缀
              m_Texture: {...}
          m_Floats:
            _AlphaClip: 0
          m_Colors:
            _BaseColor: {r: 1, ...}

    所以不能按 "  - name:" 找（那是 Variant / Prefab override 的写法）。
    """
    try:
        t = open(path, encoding="utf-8", errors="replace").read()
    except OSError:
        return set(), None
    if "m_Shader:" not in t:
        return set(), None
    guid = re.search(r"m_Shader:\s*\{[^}]*guid:\s*(\w+)", t)

    out = set()
    i = t.find("m_SavedProperties:")
    if i < 0:
        return out, (guid.group(1) if guid else None)
    body = t[i:]
    for sec in ("m_TexEnvs:", "m_Ints:", "m_Floats:", "m_Colors:"):
        j = body.find(sec)
        if j < 0:
            continue
        # 该段到下一个同级别的 key（4 空格缩进）为止
        rest = body[j + len(sec):]
        m = re.search(r"^    \S", rest, re.M)
        seg = rest[:m.start()] if m else rest
        for pm in re.finditer(r"^      ([A-Za-z_]\w*):", seg, re.M):
            out.add(pm.group(1))
    return out, (guid.group(1) if guid else None)


def main():
    mat_dir = sys.argv[1] if len(sys.argv) > 1 else "Assets"
    shader_dir = os.path.join(ROOT, "Assets", "Shader")

    # GUID -> shader 路径
    guid2shader = {}
    for m in glob.glob(os.path.join(shader_dir, "*.shader.meta")):
        g = re.search(r"guid:\s*(\w+)", open(m, encoding="utf-8",
                                            errors="replace").read())
        if g:
            guid2shader[g.group(1)] = m[:-5]

    # Unity 内建 / 引擎属性，材质里出现属正常，不算缺失
    # Unity 内建 /引擎属性，材质里出现属正常，不算缺失。
    # 用集合 + 前缀判定而不是一条大正则：深层嵌套的可选组一旦漏一个
    # 右括号就是 re.error，而这类名单增删频繁，集合更省事。
    ENGINE_EXACT = {
        "MainTex", "Color", "BaseMap", "BaseColor", "Cutoff", "Smoothness",
        "Glossiness", "Metallic", "BumpMap", "NormalMap", "ParallaxMap",
        "EmissionMap", "OcclusionMap", "SpecGloss", "DetailAlbedoMap",
        "Distortion", "Height", "DetailMask", "DetailNormalMap", "Blend",
        "QueueOffset", "AlphaClip", "Shadow", "ReceiveShadows", "Cull",
        "ZWrite", "ZTest", "SrcBlend", "DstBlend", "ColorMask", "Texture",
        "Queue", "RenderType", "Surface", "BlendOp", "Fog", "Tess",
        "Instancing", "Dither", "LOD", "Stencil", "Ref", "ReadMask",
        "WriteMask", "Comp", "CompFail", "ZFail", "FallBack", "Fallback",
        "CustomEditor", "DisableBatching", "IgnoreProjector", "PreviewType",
        "RenderPipeline", "ForceNoShadowCasting", "LightMode", "Pass",
        "Tags", "Name", "Properties", "SubShader", "Shader", "m_Shader",
        "m_Colors", "m_TexEnvs", "m_Floats", "m_Colors", "m_BuildTextureStacks",
    }
    ENGINE_PREFIX = (
        "unity_", "Lightmap", "_MainLight", "_Additional", "Fog",
        "_Shadow", "_Ambient", "_Spec", "_Emission", "_Occlusion",
        "_Detail", "_Parallax", "_Base", "_Metallic", "_Gloss", "_Height",
        "_Mask", "_Noise", "_Bump", "_Normal", "_Cutoff", "_Blend",
        "_Surface", "_ZWrite", "_Cull", "_SrcBlend", "_DstBlend",
        "_ColorMask", "_Queue", "_RenderType", "_ReceiveShadows",
        "_QueueOffset", "_AlphaClip", "_Mode", "_Instancing",
    )

    def is_engine(name):
        return name in ENGINE_EXACT or name.startswith(ENGINE_PREFIX)

    total_mats, problems = 0, []
    for mat in glob.glob(os.path.join(ROOT, mat_dir, "**", "*.mat"),
                         recursive=True):
        mprops, guid = material_props(mat)
        if not guid or not mprops:
            continue
        spath = guid2shader.get(guid)
        if not spath or not os.path.exists(spath):
            continue
        total_mats += 1
        sprops, suni = shader_props_and_uniforms(spath)
        known = sprops | suni
        missing = sorted(p for p in mprops
                         if p not in known and not is_engine(p))
        if missing:
            problems.append((os.path.relpath(mat, ROOT),
                             os.path.basename(spath), missing))

    print(f"检查 {total_mats} 个材质，{len(problems)} 个有对不上的属性")
    for mat, sh, miss in problems[:40]:
        print(f"  {mat}\n      shader={sh}\n      缺: {miss}")
    if len(problems) > 40:
        print(f"  ...另有 {len(problems) - 40} 个")


if __name__ == "__main__":
    main()