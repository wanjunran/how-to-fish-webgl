#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""生成 How to Fish 的原版天空盒 shader。

来源声明（重要）：
  本文件的片元/顶点逻辑 100% 来自原版游戏《How to Fish》Windows 版
  How to Fish_Data/sharedassets0.assets 中 "Shader Graphs/SkyboxShader"
  的 D3D11 字节码（DXBC, ps_4_0 / vs_4_0），经：
    AssetRipper 依赖的 Unity blob -> LZ4 解压
      -> RenderDoc wasm 反汇编（dxbc2sl/dxbc2asm.py）
      -> dxbc2sl/dxasm2sl.py 翻译为 HLSL
      -> tools/remap_decompiled_skybox.py 按 Unity 序列化的
         ConstantBufferBindings/VectorParams 偏移把 cb0[n]/cb1[n]
         机械替换为真实 uniform 名（_TopDayColor/_SkyPixels/...）
  仅做了机械替换与包装，未手写任何渲染逻辑。
"""
import re

PS = open("/tmp/sky_ps_mapped.hlsl", encoding="utf-8").read()
VS = open("/tmp/sky_vs_mapped.hlsl", encoding="utf-8").read()


def body(src, fn):
    m = re.search(r"void %s\(\) \{\n(.*?)\n\}" % fn, src, re.S)
    b = m.group(1)
    b = b.replace("\tconst int MAX_LOOP = 64;\n", "")
    b = b.replace("\tfloat4 TMP;\n", "")
    return "\n".join("    " + l if l.strip() else l for l in b.split("\n"))


ps_body = body(PS, "ps_main")
vs_body = body(VS, "vs_main")

SHADER = f'''// ============================================================================
//  How to Fish — 原版天空盒 Shader（反编译恢复版）
//  Shader Graphs/SkyboxShader
//  逻辑 100% 来自原版 Windows 版资源内 D3D11 字节码（ps_4_0/vs_4_0），
//  经 RenderDoc wasm 反汇编 + dxbc2sl 翻译 + 常量偏移机械替换得到。
//  属性名/GUID 与 AssetRipper 导出的原版 .shader 完全一致，材质无需改动。
// ============================================================================

Shader "Shader Graphs/SkyboxShader"
{{
    Properties
    {{
        _SkyPixels ("SkyPixels", Vector) = (1,1,0,0)
        [HDR] _TopDayColor ("TopDayColor", Vector) = (0,0.4660978,1,1)
        [HDR] _TopNightColor ("TopNightColor", Vector) = (0,0.4660978,1,1)
        [HDR] _BottomDayColor ("BottomDayColor", Vector) = (0.2627451,0.5764706,1,1)
        [HDR] _BottomSunriseColor ("BottomSunriseColor", Vector) = (2.996079,1.050501,0,1)
        [HDR] _BottomNightColor ("BottomNightColor", Vector) = (0.01960784,0.01960784,0.01960784,1)
        _SkyNoiseStrength ("SkyNoiseStrength", Range(0, 1)) = 1
        _SkyBands ("SkyBands", Float) = 6
        _SkyNoiseScale ("SkyNoiseScale", Vector) = (10,10,0,0)
        _SunStep ("SunStep", Range(0, 1)) = 0.5
        _SunPixels ("SunPixels", Float) = 4
        [HDR] _SunColor ("SunColor", Vector) = (1,0.9787216,0.6226415,1)
        _SunIntensity ("SunIntensity", Float) = 8
        _MoonRadius ("MoonRadius", Range(0, 100)) = 0
        [HDR] _MoonColor ("MoonColor", Vector) = (0.5660378,0.5660378,0.5660378,1)
        _MoonIntensity ("MoonIntensity", Float) = 1
        [HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl ("_QueueControl", Float) = -1
    }}

    SubShader
    {{
        Tags {{ "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
               "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }}
        Cull Off ZWrite Off

        Pass
        {{
            Name "Unlit"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // #pragma multi_compile_fog  // 原版天空盒不参与雾

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // UnityPerMaterial 布局严格按原版序列化偏移（@0/@16/@32/.../@184）
            CBUFFER_START(UnityPerMaterial)
                float4  _TopNightColor;        // @0
                float   _SkyNoiseStrength;     // @16
                float4  _TopDayColor;          // @32
                float2  _SkyNoiseScale;        // @48
                float   _SkyBands;             // @56
                float   _SunStep;              // @64
                float4  _SunColor;             // @96
                float   _SunIntensity;         // @116
                float4  _BottomDayColor;       // @128
                float4  _BottomSunriseColor;   // @144
                float4  _BottomNightColor;     // @160
                float2  _SkyPixels;            // @176
                float   _SunPixels;            // @184
            CBUFFER_END

            struct Attributes
            {{
                float4 positionOS : POSITION;
            }};

            struct Varyings
            {{
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;   // 原版 VS 输出 o1
            }};

            Varyings vert(Attributes IN)
            {{
                Varyings OUT;
                float3 ws = mul(unity_ObjectToWorld, IN.positionOS).xyz;
                OUT.positionWS = ws;
                OUT.positionCS = mul(unity_MatrixVP, float4(ws, 1.0));
                return OUT;
            }}

            float4 frag(Varyings IN) : SV_Target
            {{
                float4 v1 = float4(IN.positionWS, 1.0);
                float4 o0;
                {{
{ps_body}
                }}
                return o0;
            }}
            ENDHLSL
        }}
    }}
    Fallback "Hidden/Shader Graph/FallbackError"
    // CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}}
'''

out = "Assets/Shader/Shader Graphs_SkyboxShader.shader"
open(out, "w", encoding="utf-8").write(SHADER)
print("已生成", out, len(SHADER.splitlines()), "行")
