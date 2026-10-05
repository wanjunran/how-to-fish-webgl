#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把 dxbc2sl 生成的寄存器风格 HLSL 机械替换为 Unity uniform 名。

替换表来自 Unity 序列化数据中的 ConstantBufferBindings / VectorParams 偏移，
纯文本替换，不改写任何逻辑 —— 这是"从原版字节码恢复"，不是手写。
"""
import re
import sys

# ===== 片元着色器：cb0 = $Globals, cb1 = UnityPerMaterial =====
PS_CB0 = {
    "cb0[5]":   "_MainLightPosition",
    "cb0[21]":  "_WorldSpaceCameraPos",
    "cb0[25]":  "unity_OrthoParams",
    "cb0[67]":  "unity_MatrixV[0]",
    "cb0[68]":  "unity_MatrixV[1]",
    "cb0[69]":  "unity_MatrixV[2]",
}
PS_CB1 = {
    "cb1[0]":   "_TopNightColor",
    "cb1[2]":   "_TopDayColor",
    "cb1[6]":   "_SunColor",
    "cb1[8]":   "_BottomDayColor",
    "cb1[9]":   "_BottomSunriseColor",
    "cb1[10]":  "_BottomNightColor",
}
# 分量级的：cb1[1].x -> _SkyNoiseStrength, cb1[3].z -> _SkyBands ...
PS_CB1_SWIZZLE = [
    ("cb1[11].xy", "_SkyPixels.xy"),
    ("cb1[11].zz", "_SunPixels.xx"),
    ("cb1[11].z",  "_SunPixels"),
    ("cb1[11]",    "_SkyPixels"),      # 兜底（放后面）
    ("cb1[7].yyy", "_SunIntensity.xxx"),
    ("cb1[7]",     "_SunIntensity_unused"),
    ("cb1[7].y",   "_SunIntensity"),
    ("cb1[3].z",   "_SkyBands"),
    ("cb1[3].xy",  "_SkyNoiseScale.xy"),
    ("cb1[3].y",   "_SkyNoiseScale.y"),
    ("cb1[3].x",   "_SkyNoiseScale.x"),
    ("cb1[4].x",   "_SunStep"),
    ("cb1[1].x",   "_SkyNoiseStrength"),
]

# ===== 顶点着色器：cb0 = $Globals(unity_MatrixVP @79..82), cb1 = UnityPerDraw =====
VS_CB0 = {
    "cb0[79]":  "unity_MatrixVP[0]",
    "cb0[80]":  "unity_MatrixVP[1]",
    "cb0[81]":  "unity_MatrixVP[2]",
    "cb0[82]":  "unity_MatrixVP[3]",
}


def apply(src: str, maps):
    """maps: dict（无序替换）或 (k, v) 元组列表（有序替换）"""
    for m in maps:
        pairs = m.items() if isinstance(m, dict) else m
        for k, v in pairs:
            src = src.replace(k, v)
    return src


def main():
    ps = open("/tmp/sky_ps.hlsl", encoding="utf-8").read()
    vs = open("/tmp/sky_vs.hlsl", encoding="utf-8").read()

    ps = apply(ps, [PS_CB1_SWIZZLE, PS_CB1, PS_CB0])
    vs = apply(vs, [VS_CB0])
    # 顶点里的 cb1 = UnityPerDraw 的 unity_ObjectToWorld 三行 + 平移、unity_WorldToObject 三行
    vs = apply(vs, [[
        ("cb1[0]", "unity_ObjectToWorld[0]"),
        ("cb1[1]", "unity_ObjectToWorld[1]"),
        ("cb1[2]", "unity_ObjectToWorld[2]"),
        ("cb1[3]", "unity_ObjectToWorld[3]"),
        ("cb1[4]", "unity_WorldToObject[0]"),
        ("cb1[5]", "unity_WorldToObject[1]"),
        ("cb1[6]", "unity_WorldToObject[2]"),
    ]])

    open("/tmp/sky_ps_mapped.hlsl", "w").write(ps)
    open("/tmp/sky_vs_mapped.hlsl", "w").write(vs)

    left = sorted(set(re.findall(r"\bcb\d\[\d+\](?:\.\w+)?", ps + vs)))
    print("剩余未映射的 cb 引用:", left if left else "无 ✓")
    print("PS ->", "/tmp/sky_ps_mapped.hlsl", len(ps.splitlines()), "行")
    print("VS ->", "/tmp/sky_vs_mapped.hlsl", len(vs.splitlines()), "行")


if __name__ == "__main__":
    main()
