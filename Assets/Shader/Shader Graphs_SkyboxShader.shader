// ============================================================================
//  How to Fish — 原版天空盒 Shader（反编译恢复版）
//  Shader Graphs/SkyboxShader
//  逻辑 100% 来自原版 Windows 版资源内 D3D11 字节码（ps_4_0/vs_4_0），
//  经 RenderDoc wasm 反汇编 + dxbc2sl 翻译 + 常量偏移机械替换得到。
//  属性名/GUID 与 AssetRipper 导出的原版 .shader 完全一致，材质无需改动。
// ============================================================================

Shader "Shader Graphs/SkyboxShader"
{
    Properties
    {
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
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
               "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Cull Off ZWrite Off

        Pass
        {
            Name "Unlit"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            // 原版天空盒不参与雾，也不需要 URP 的 multi_compile 变体：
            // 只依赖 _MainLightPosition（Core.hlsl 已提供），不再 include
            // Lighting.hlsl —— 它会引入一批未声明的 URP keyword 依赖。

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;   // 原版 VS 输出 o1
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 ws = mul(unity_ObjectToWorld, IN.positionOS).xyz;
                OUT.positionWS = ws;
                OUT.positionCS = mul(unity_MatrixVP, float4(ws, 1.0));
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 v1 = float4(IN.positionWS, 1.0);
                float4 o0;
                {
    	float4 r0, r1, r2, r3, r4, r5;
    	r0 = (v1.yxyz + (-_WorldSpaceCameraPos.yxyz));
    	r1.x = dot(r0.yzw, r0.yzw);
    	r1.x = rsqrt(r1.x);
    	r0.yzw = (r0.yzw * r1.xxx);
    	r0.x = mad(r0.x, r1.x, float(1));
    	r0.x = (r0.x * float(0.5));
    	r0.x = max(r0.x, float(0.5));
    	r0.x = min(r0.x, float(1));
    	r0.x = (r0.x + float(-0.5));
    	r0.x = (r0.x + r0.x);
    	r1.x = max(abs(r0.w), abs(r0.y));
    	r1.x = (float4(1, 1, 1, 1).x / r1.x);
    	r1.y = min(abs(r0.w), abs(r0.y));
    	r1.x = (r1.x * r1.y);
    	r1.y = (r1.x * r1.x);
    	r1.z = mad(r1.y, float(0.0208351), float(-0.085133));
    	r1.z = mad(r1.y, r1.z, float(0.180141));
    	r1.z = mad(r1.y, r1.z, float(-0.330299));
    	r1.y = mad(r1.y, r1.z, float(0.999866));
    	r1.z = (r1.y * r1.x);
    	r1.z = mad(r1.z, float(-2), float(1.5708));
    	r1.w = asfloat(abs(r0.w) <  abs(r0.y) ? -1 : 0);
    	r1.z = asfloat(asuint(r1.w) & asuint(r1.z));
    	r1.x = mad(r1.x, r1.y, r1.z);
    	r1.yz = asfloat(r0.wz <  (-r0.wwzw).yz ? -1 : 0);
    	r1.y = asfloat(asuint(r1.y) & asuint(float(-3.14159)));
    	r1.x = (r1.y + r1.x);
    	r1.y = min(r0.w, r0.y);
    	r1.y = asfloat(r1.y <  (-r1.y) ? -1 : 0);
    	r0.y = max(r0.w, r0.y);
    	r0.y = asfloat(r0.y >= (-r0.y) ? -1 : 0);
    	r0.y = asfloat(asuint(r0.y) & asuint(r1.y));
    	r0.y = (asuint(r0.y) ? (-r1.x) : r1.x);
    	r0.y = (r0.y * _SkyNoiseScale.x);
    	r1.x = (r0.y * _SkyPixels.x);
    	r0.y = mad(abs(r0.z), float(-0.0187293), float(0.074261));
    	r0.y = mad(r0.y, abs(r0.z), float(-0.212114));
    	r0.y = mad(r0.y, abs(r0.z), float(1.57073));
    	r0.z = ((-abs(r0.z)) + float(1));
    	r0.z = sqrt(r0.z);
    	r0.w = (r0.z * r0.y);
    	r0.w = mad(r0.w, float(-2), float(3.14159));
    	r0.w = asfloat(asuint(r1.z) & asuint(r0.w));
    	r0.y = mad(r0.y, r0.z, r0.w);
    	r0.y = ((-r0.y) + float(1.5708));
    	r0.y = (r0.y * _SkyNoiseScale.y);
    	r1.y = (r0.y * _SkyPixels.y);
    	r0.yz = (r1.xy * float4(0, 0.159155, 0.63662, 0).yz);
    	r0.yz = floor(r0.yz);
    	r0.yz = (r0.yz / _SkyPixels.xy);
    	r0.yz = (r0.yz * _SkyNoiseScale.xx);
    	r1.xy = frac(r0.yz);
    	r0.yz = floor(r0.yz);
    	r1.zw = (r1.xy * r1.xy);
    	r1.zw = (r1.xy * r1.zw);
    	r2.xy = mad(r1.xy, float4(6, 6, 0, 0).xy, float4(-15, -15, 0, 0).xy);
    	r2.xy = mad(r1.xy, r2.xy, float4(10, 10, 0, 0).xy);
    	r1.zw = (r1.zw * r2.xy);
    	r2.xy = (r0.yz + float4(1, 1, 0, 0).xy);
    	r2.xy = asfloat(int2(r2.xy));
    	r0.w = asfloat(asuint(r2.y) ^ asuint(float(24.7883)));
    	r2.x = asfloat((asint(r0.w) + asint(r2.x)));
    	r0.w = asfloat((asint(r0.w) * asint(r2.x)));
    	r2.x = asfloat(asuint(r0.w) >> asuint(int(5)));
    	r0.w = asfloat(asuint(r0.w) ^ asuint(r2.x));
    	r0.w = asfloat((asint(r0.w) * asint(float(5.90968e-15))));
    	r0.w = asfloat(asuint(r0.w) >> asuint(int(8)));
    	r0.w = float(asuint(r0.w));
    	r2.yz = mad(r0.ww, float4(0, 5.96047e-08, 5.96047e-08, 0).yz, float4(0, 0.5, -0.5, 0).yz);
    	r2.y = floor(r2.y);
    	r2.x = mad(r0.w, float(5.96047e-08), (-r2.y));
    	r0.w = dot(r2.xz, r2.xz);
    	r0.w = rsqrt(r0.w);
    	r2.xy = (r0.ww * r2.xz);
    	r2.zw = (r1.xy + float4(0, 0, -1, -1).zw);
    	r0.w = dot(r2.xy, r2.zw);
    	r2 = (r1.xyxy + float4(-0, -1, -1, -0));
    	r3 = (r0.yzyz + float4(0, 1, 1, 0));
    	r0.yz = asfloat(int2(r0.yz));
    	r3 = asfloat(int4(r3));
    	r3.yw = asfloat(asuint(r3.yw) ^ asuint(float4(0, 24.7883, 0, 24.7883).yw));
    	r3.xz = asfloat((asint(r3.yw) + asint(r3.xz)));
    	r3.xy = asfloat((asint(r3.yw) * asint(r3.xz)));
    	r3.zw = asfloat(asuint(r3.xy) >> asuint(int(5).zw));
    	r3.xy = asfloat(asuint(r3.zw) ^ asuint(r3.xy));
    	r3.xy = asfloat((asint(r3.xy) * asint(float4(5.90968e-15, 5.90968e-15, 0, 0).xy)));
    	r3.xy = asfloat(asuint(r3.xy) >> asuint(int(8).xy));
    	r3.xy = float2(asuint(r3.xy));
    	r4 = mad(r3.xyxy, float4(5.96047e-08, 5.96047e-08, 5.96047e-08, 5.96047e-08), float4(0.5, 0.5, -0.5, -0.5));
    	r3.zw = floor(r4.xy);
    	r4.xy = mad(r3.xy, float4(5.96047e-08, 5.96047e-08, 0, 0).xy, (-r3.zwzz).xy);
    	r3.x = dot(r4.yw, r4.yw);
    	r3.x = rsqrt(r3.x);
    	r3.xy = (r3.xx * r4.yw);
    	r2.z = dot(r3.xy, r2.zw);
    	r0.w = (r0.w + (-r2.z));
    	r0.w = mad(r1.w, r0.w, r2.z);
    	r2.z = dot(r4.xz, r4.xz);
    	r2.z = rsqrt(r2.z);
    	r2.zw = (r2.zz * r4.xz);
    	r2.x = dot(r2.zw, r2.xy);
    	r0.z = asfloat(asuint(r0.z) ^ asuint(float(24.7883)));
    	r0.y = asfloat((asint(r0.z) + asint(r0.y)));
    	r0.y = asfloat((asint(r0.z) * asint(r0.y)));
    	r0.z = asfloat(asuint(r0.y) >> asuint(int(5)));
    	r0.y = asfloat(asuint(r0.z) ^ asuint(r0.y));
    	r0.y = asfloat((asint(r0.y) * asint(float(5.90968e-15))));
    	r0.y = asfloat(asuint(r0.y) >> asuint(int(8)));
    	r0.y = float(asuint(r0.y));
    	r3.yz = mad(r0.yy, float4(0, 5.96047e-08, 5.96047e-08, 0).yz, float4(0, 0.5, -0.5, 0).yz);
    	r0.z = floor(r3.y);
    	r3.x = mad(r0.y, float(5.96047e-08), (-r0.z));
    	r0.y = dot(r3.xz, r3.xz);
    	r0.y = rsqrt(r0.y);
    	r0.yz = (r0.yy * r3.xz);
    	r0.y = dot(r0.yz, r1.xy);
    	r0.z = ((-r0.y) + r2.x);
    	r0.y = mad(r1.w, r0.z, r0.y);
    	r0.z = ((-r0.y) + r0.w);
    	r0.y = mad(r1.z, r0.z, r0.y);
    	r0.y = (r0.y + float(0.5));
    	r0.x = mad(_SkyNoiseStrength, r0.y, r0.x);
    	r0.x = (r0.x * _SkyBands);
    	r0.x = floor(r0.x);
    	r0.x = (r0.x / _SkyBands);
    	r0.y = ((-_MainLightPosition.y) + float(1));
    	r0.yzw = mad(r0.yyy, float4(0, 0.5, 0.5, 0.5).yzw, float4(0, -0.3, -0.5, -0.4).yzw);
    	r0.yzw = saturate((r0.yzw * float4(0, 5, 10, 5).yzw));
    	r1.xyz = mad(r0.yzw, float4(-2, -2, -2, 0).xyz, float4(3, 3, 3, 0).xyz);
    	r0.yzw = (r0.yzw * r0.yzw);
    	r0.yzw = (r0.yzw * r1.xyz);
    	r1.xyz = ((-_BottomDayColor.xyzx).xyz + _BottomSunriseColor.xyz);
    	r1.xyz = mad(r0.yyy, r1.xyz, _BottomDayColor.xyz);
    	r2.xyz = ((-r1.xyzx).xyz + _BottomNightColor.xyz);
    	r1.xyz = mad(r0.zzz, r2.xyz, r1.xyz);
    	r2.xyz = (_TopNightColor.xyz + (-_TopDayColor.xyzx).xyz);
    	r0.yzw = mad(r0.www, r2.xyz, _TopDayColor.xyz);
    	r0.yzw = ((-r1.xxyz).yzw + r0.yzw);
    	r0.xyz = mad(r0.xxx, r0.yzw, r1.xyz);
    	r1.xyz = mad(_SunColor.xyz, _SunIntensity.xxx, (-r0.xyzx).xyz);
    	r2.xyz = (_MainLightPosition.zxy * float4(-0, -1, -0, 0).xyz);
    	r2.xyz = mad(_MainLightPosition.xyz, float4(-0, -0, -1, 0).xyz, (-r2.xyzx).xyz);
    	r0.w = dot(r2.yz, r2.yz);
    	r0.w = rsqrt(r0.w);
    	r2.xyz = (r0.www * r2.xyz);
    	r3.xyz = (r2.xyz * (-_MainLightPosition.zxyz).xyz);
    	r3.xyz = mad((-_MainLightPosition.yzxy).xyz, r2.yzx, (-r3.xyzx).xyz);
    	r4.xyz = ((-v1.xyzx).xyz + _WorldSpaceCameraPos.xyz);
    	r0.w = dot(r4.xyz, r4.xyz);
    	r0.w = rsqrt(r0.w);
    	r4.xyz = (r0.www * r4.xyz);
    	r0.w = asfloat(unity_OrthoParams.w == float(0) ? -1 : 0);
    	r5.y = (asuint(r0.w) ? r4.y : unity_MatrixV[1].z);
    	r5.x = (asuint(r0.w) ? r4.x : unity_MatrixV[0].z);
    	r5.z = (asuint(r0.w) ? r4.z : unity_MatrixV[2].z);
    	r3.y = dot(r5.xyz, r3.xyz);
    	r0.w = saturate(dot(r5.xyz, (-_MainLightPosition.xyzx).xyz));
    	r3.x = dot(r5.zx, r2.yz);
    	r2.xy = (r3.xy * _SunPixels.xx);
    	r2.xy = floor(r2.xy);
    	r2.xy = (r2.xy / _SunPixels.xx);
    	r1.w = dot(r2.xy, r2.xy);
    	r1.w = sqrt(r1.w);
    	r1.w = asfloat(_SunStep >= r1.w ? -1 : 0);
    	r1.w = asfloat(asuint(r1.w) & asuint(float(1)));
    	r0.w = (r0.w * r1.w);
    	o0.xyz = mad(r0.www, r1.xyz, r0.xyz);
    	o0.w = float(1);
    	return;
                }
                return o0;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
    // CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}
