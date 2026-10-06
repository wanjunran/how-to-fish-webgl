#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""对生成的 .shader 做静态自查，尽量在提交 CI 之前抓出低级错误。

只做检查，不改文件。检查项：
  1. 括号 / 花括号是否平衡；
  2. 正文里引用的每个标识符是否都有声明（uniform / 全局 / 结构体成员 /
     函数 / 宏 / HLSLcc 辅助函数）；
  3. 采样宏 sampler##tex 拼出的 sampler_<name> 是否都有对应 SAMPLER() 声明；
  4. Varyings 里的插值器是否与 VS 输出、PS 输入一致；
  5. 是否残留 GLSL 特有的语法（layout(、precision、#version、gl_Position、
     vec3/vec4/mat4 之类）。
"""
import argparse
import glob
import os
import re
import sys

KEYWORDS = {
    "if", "else", "for", "while", "do", "return", "break", "continue",
    "discard", "switch", "case", "default", "true", "false", "struct",
    "float", "float2", "float3", "float4", "int", "int2", "int3", "int4",
    "uint", "uint2", "uint3", "uint4", "bool", "bool2", "bool3", "bool4",
    "float2x2", "float3x3", "float4x4", "float2x3", "float3x2",
    "float3x4", "float4x3", "float4x2", "float2x4",
    "sampler_state", "Texture2D", "TextureCube", "Texture3D",
    "SamplerState", "StructuredBuffer", "void", "const", "static",
    "in", "out", "inout", "uniform", "cbuffer", "tbuffer", "typedef",
    "transpose", "mul", "sampler", "max", "min", "abs", "dot", "cross",
    "normalize", "lerp", "rsqrt", "sqrt", "exp", "exp2", "log", "log2",
    "pow", "sin", "cos", "tan", "atan", "asin", "acos", "floor", "ceil",
    "round", "trunc", "frac", "sign", "step", "smoothstep", "clamp",
    "saturate", "ddx", "ddy", "fwidth", "asuint", "asint", "asfloat",
    "any", "all", "isnan", "isinf", "rcp", "mad", "sincos", "modf",
    "TEXTURE2D", "TEXTURECUBE", "TEXTURE3D", "TEXTURE2D_ARRAY",
    "SAMPLER", "SAMPLER_CMP", "TEXTURE2D_SHADOW", "SV_Target", "SV_Position",
    "SV_POSITION", "SV_VertexID", "SV_InstanceID", "UNITY_UNIFORM",
    # 纹理对象的成员方法：HLSL intrinsic，不需要声明
    "Sample", "SampleBias", "SampleLevel", "SampleCmp", "SampleCmpLevelZero",
    "SampleGrad", "Load", "GetDimensions", "Gather", "CalculateLevelOfDetail",
}

GLSL_LEFTOVER = [
    (r"^\s*#version", "#version"),
    (r"^\s*#extension", "#extension"),
    (r"\bprecision\s+(highp|mediump|lowp)\b", "GLSL 精度限定符"),
    (r"\blayout\s*\(", "GLSL layout()"),
    (r"\bgl_Position\b", "gl_Position"),
    (r"\bgl_FragCoord\b", "gl_FragCoord"),
    (r"\bgl_InstanceID\b", "gl_InstanceID"),
    (r"\bgl_VertexID\b", "gl_VertexID"),
    (r"\bgl_FragDepth\b", "gl_FragDepth"),
    (r"\b(in|out)\s+(highp|mediump|lowp)\s", "GLSL in/out"),
    # float3(...) / float4 都是合法 HLSL，只有 GLSL 独有的 vec/mat 才算残留
    (r"\b(vec[234]|ivec[234]|uvec[234]|bvec[234]|mat[234]x?[234]?)\b",
     "GLSL 类型名(vec/mat)"),
]

# 宏形参 / 语义名 / 预处理指令词：出现但不算"未声明的变量"
MISC_OK = {
    "tex", "uv", "lod", "bias", "coord", "o", "a", "b", "c", "d", "v", "i",
    "define", "pragma", "vertex", "fragment", "target", "include", "end",
    "NORMAL", "POSITION", "TANGENT", "COLOR", "BLENDWEIGHT", "BLENDINDICES",
    "SV_Target0", "SV_Target1", "SV_Target2", "SV_Target3",
    "dFdx", "dFdy", "fwidth",
} | {f"TEXCOORD{i}" for i in range(24)}

IDENT = re.compile(r"\b[A-Za-z_]\w*\b")


def strip_comments_and_strings(src):
    src = re.sub(r"/\*.*?\*/", " ", src, flags=re.S)
    src = re.sub(r"//[^\n]*", " ", src)
    src = re.sub(r'"(?:[^"\\]|\\.)*"', '""', src)
    return src


def check(path):
    raw = open(path, encoding="utf-8", errors="replace").read()
    errs, warns = [], []

    # --- 1. 括号平衡（只看 HLSLPROGRAM 段）---
    m = re.search(r"HLSLPROGRAM(.*?)ENDHLSL", raw, re.S)
    if not m:
        errs.append("找不到 HLSLPROGRAM/ENDHLSL 段")
        return errs, warns
    body = m.group(1)

    for ch_open, ch_close in (("{", "}"), ("(", ")"), ("[", "]")):
        d = strip_comments_and_strings(body).count(ch_open) - \
            strip_comments_and_strings(body).count(ch_close)
        if d:
            errs.append(f"{ch_open}{ch_close} 不平衡：差 {d}")

    # --- 5. GLSL 残留 ---
    for pat, label in GLSL_LEFTOVER:
        hits = re.findall(pat, body, re.M)
        if hits:
            errs.append(f"残留 {label} ×{len(hits)}（首个：{hits[0]!r}）")

    src = strip_comments_and_strings(body)

    # --- 收集声明 ---
    declared = set(KEYWORDS)
    declared |= set(re.findall(r"#define\s+(\w+)", src))
    declared |= set(re.findall(r"^\s*(?:const\s+)?[\w]+\s+(\w+)\s*(?:\[[^\]]*\])?\s*;",
                               src, re.M))                     # 全局/局部量
    declared |= set(re.findall(r"^\s*#define\s+(\w+)\(", src, re.M))
    # 结构体成员
    for sm in re.finditer(r"struct\s+(\w+)\s*\{(.*?)\}", src, re.S):
        declared.add(sm.group(1))
        declared |= set(re.findall(r"[\w]+\s+(\w+)\s*(?::\s*\w+)?\s*;",
                                   sm.group(2)))
    # 函数名
    declared |= set(re.findall(r"^\s*[\w]+\s+(\w+)\s*\(", src, re.M))
    # swizzle 会被误当标识符，剔除带 . 的
    src_noswz = re.sub(r"\.\s*[xyzwrgbastpq]+\b", " ", src)
    src_noswz = re.sub(r"\b\w+\s*\.\s*(\w+)", r"\1", src_noswz)  # obj.field

    used = set(IDENT.findall(src_noswz))
    missing = sorted((used - declared) - MISC_OK)
    missing = [u for u in missing if not u.startswith("_g_")]
    if missing:
        warns.append("正文出现但未见声明：" + ", ".join(missing[:25])
                     + (" …" if len(missing) > 25 else ""))

    # --- 3. 采样器配对 ---
    texs = set(re.findall(r"TEXTURE\w*\(\s*(\w+)\s*\)", src))
    samplers = set(re.findall(r"SAMPLER\w*\(\s*(\w+)\s*\)", src))
    for t in texs:
        if f"sampler_{t}" not in samplers and t not in samplers:
            errs.append(f"纹理 {t} 没有对应的 SAMPLER(sampler_{t}) 声明")
    for s in samplers:
        if s.startswith("sampler_") and s[len("sampler_"):] not in texs:
            errs.append(f"采样器 {s} 没有对应的纹理声明")

    # --- 4. 插值器一致性 ---
    vary = None
    mv = re.search(r"struct\s+Varyings\s*\{(.*?)\}", src, re.S)
    if mv:
        vary = set(re.findall(r"[\w]+\s+(\w+)\s*:\s*TEXCOORD", mv.group(1)))
        if "positionCS" in src and "positionCS : SV_POSITION" not in mv.group(1):
            errs.append("Varyings 缺少 positionCS : SV_POSITION")
    for usedv in set(re.findall(r"\binput\.(vs_\w+)", src)):
        if vary is not None and usedv not in vary:
            errs.append(f"frag 用了 input.{usedv}，但 Varyings 里没声明")
    if not re.search(r"output\.\w+\s*=", src):
        warns.append("vert 里没看到 output.xxx 赋值")

    return errs, warns


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("dirs", nargs="+")
    a = ap.parse_args()
    files = []
    for d in a.dirs:
        files += sorted(glob.glob(os.path.join(d, "*.shader"))) \
            if os.path.isdir(d) else [d]
    bad = 0
    for f in files:
        errs, warns = check(f)
        if errs or warns:
            print(f"\n=== {os.path.basename(f)} ===")
            for e in errs:
                print("  错误:", e)
            for w in warns:
                print("  警告:", w)
            bad += bool(errs)
    print(f"\n检查 {len(files)} 个文件，{bad} 个有错误")


if __name__ == "__main__":
    main()
