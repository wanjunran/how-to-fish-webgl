Shader "Andicraft/Retro Master (NOT Transparent)"
{
    Properties
    {


[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
TEXTURES ("# Textures", Float) = 0
[KeywordEnum(Nearest, Linear, N64)] _TextureFiltering ("Texture Filtering Mode", Float) = 0
_BaseMap ("Color &&", 2D) = "white" {}
_Color ("Tint", Vector) = (1,1,1,1)
[Toggle] _EnableMaskMap ("Mask Map", Float) = 1
_MaskNote1 ("!NOTE Using Color Map Alpha as Smoothness [_EnableMaskMap == 0]", Float) = 0
_MaskMap ("Mask Map & [_EnableMaskMap]", 2D) = "" {}
_MaskNote2 ("!NOTE R: Metal G: Smoothness B: Occlusion", Float) = 0
SMOOTHSLIDER ("-!DRAWER MinMax _SmoothRemap.x _SmoothRemap.y", Float) = 1
_SmoothRemap ("Remap Smoothness", Vector) = (0,1,0,0)
[Normal] _BumpMap ("Normal Map &&", 2D) = "bump" {}
_NormalStrength ("Normal Map Strength", Range(0, 2)) = 1
[Toggle(_EMISSION)] _Emission ("Emission", Float) = 0
_EmissionMap ("-Emission Map && [_EMISSION]", 2D) = "white" {}
[HDR] _EmissionColor ("-Emission Color [_EMISSION]", Vector) = (0,0,0,1)
_LightmapEmissionStr ("-Lightmap Emission Strength", Float) = 1
_TilingOffset ("Tiling/Offset &", Vector) = (1,1,0,0)
_Lighting ("# Lighting", Float) = 0
[KeywordEnum(Pixel, Vertex, Texel)] _LightingType ("Lighting Type", Float) = 2
[KeywordEnum(CookTorrance, Phong, Blinn Phong, Disabled)] _SpecularType ("Specular Type", Float) = 0
[Toggle] _HalfLambert ("Half Lambert", Float) = 0
[Toggle] _Reflections ("Reflections", Float) = 1
[Toggle(_TOONSHADING)] _ToonShading ("Toon Shading", Float) = 0
_DITHERHEADER ("## Dithering", Float) = 0
[Toggle] _DitherDiffuse ("-Dither Diffuse Light", Float) = 0
[Toggle] _DitherSpec ("-Dither Specular Light", Float) = 0
[Toggle] _DitherAmbient ("-Dither Ambient & Lightmaps", Float) = 0
[Toggle] _DoubleSizeDither ("-Double Size Dither", Float) = 0
_Extras ("# Extras", Float) = 0
[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0
[Toggle()] _VColEmissive ("Vertex Color as Emissive", Float) = 0
_VertexEmissiveStr ("- Emissive Strength [_VColEmissive]", Float) = 1
[Toggle(_ALPHATEST_ON)] _AlphaTest ("Alpha Cutoff", Float) = 0
_Cutoff ("-Alpha Cutoff Threshold [_ALPHATEST_ON]", Range(0, 1)) = 0.5
[Toggle(_VERTEXJITTER)] _VertexJitter ("Vertex Snap", Float) = 0
_VertexJitterResX ("- Virtual Screen X Resolution [_VERTEXJITTER]", Float) = 320
_VertexJitterResY ("- Virtual Screen Y Resolution [_VERTEXJITTER]", Float) = 240
[Toggle(_AFFINE_MAPPING)] _AffineMapping ("Affine UV Mapping", Float) = 0
_AffineMapFactor ("-Warp Factor [_AFFINE_MAPPING]", Range(0, 1)) = 1
[Toggle] _MaskTint ("Mask Color Tint", Float) = 0
_TintNote ("!NOTE Using Mask Map A channel to mask color tint value [_MaskTint]", Float) = 0
_ToonShading ("# Toon Shading [_TOONSHADING]", Float) = 0
_ToonRamp ("Diffuse Ramp&", 2D) = "white" {}
_SpecularRamp ("Specular Ramp&", 2D) = "white" {}
RAMPGENERATOR ("!DRAWER Gradient _RampGen", Float) = 0
_RampGen ("Ramp Generator", 2D) = "white" {}
_VertexLighting ("# Vertex Light Settings [_LIGHTINGTYPE_VERTEX]", Float) = 0
_VertexSpecColor ("Specular Color", Vector) = (1,1,1,1)
[Toggle(_VCOLSMOOTHNESS)] _VColSmoothness ("Use Vertex Color A as Smoothness", Float) = 0
SMOOTHSLIDER2 ("-!DRAWER MinMax _SmoothRemapVCol.x _SmoothRemapVCol.y [_VCOLSMOOTHNESS]", Float) = 1
_SmoothRemapVCol ("Remap Smoothness", Vector) = (0,1,0,0)
_VertexLightSmoothness ("Smoothness [!_VCOLSMOOTHNESS]", Range(0, 1)) = 0.5
[HideInInspector] _AlphaClip ("__clip", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Name "Forward"


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

// ---- 以下全部是 GLSL -> HLSL 的等价宏，正文一字未改 ----
#define _g_inversesqrt rsqrt
#define _g_fract frac
#define _g_mix lerp
#define _g_roundEven round
#define _g_trunc trunc
#define _g_floatBitsToUint asuint
#define _g_uintBitsToFloat asfloat
#define _g_floatBitsToInt asint
#define _g_intBitsToFloat asfloat
#define _g_lessThan(a, b) ((a) < (b))
#define _g_greaterThan(a, b) ((a) > (b))
#define _g_greaterThanEqual(a, b) ((a) >= (b))
#define _g_lessThanEqual(a, b) ((a) <= (b))
#define _g_equal(a, b) ((a) == (b))
#define _g_notEqual(a, b) ((a) != (b))
#define _g_bitfieldExtract(v, o, c) (((v) >> (o)) & ((1u << (c)) - 1u))
// 采样：sampler##tex 拼出 sampler_<纹理名>，与下方 SAMPLER 声明一一对应
#define _g_texture(tex, uv) ((tex).Sample(sampler##tex, uv))
#define _g_textureBias(tex, uv, b) ((tex).SampleBias(sampler##tex, uv, b))
#define _g_textureLod(tex, uv, lod) ((tex).SampleLevel(sampler##tex, uv, lod))
#define _g_texelFetch(tex, coord) ((tex).Load(int3(coord)))
// GLSL ES 3.0 的 sampler2DShadow 比较采样 -> HLSL SampleCmpLevelZero
#define _g_zcmpLod(tex, uv, lod) \
    ((tex).SampleCmpLevelZero(sampler##tex, float3(uv, lod)))

            TEXTURECUBE(_Texture_t0);
            SAMPLER(sampler__Texture_t0);
            TEXTURE2D(_Texture_t1);
            SAMPLER(sampler__Texture_t1);
            TEXTURE2D(_Texture_t2);
            SAMPLER(sampler__Texture_t2);
            TEXTURE2D(_Texture_t3);
            SAMPLER(sampler__Texture_t3);
            TEXTURE2D(_Texture_t4);
            SAMPLER(sampler__Texture_t4);

            float4 _pad0;
            float4 _pad16;
            float4 _pad32;
            float4 _pad48;
            float4 _pad64;
            float4 _pad80;
            float4 _pad320;
            float4 _pad336;
            float4 _ProjectionParams;
            float4 _pad368;
            float4 _pad384;
            float4 _pad432;
            float4 _pad448;
            float4 _pad464;
            float4 _pad480;
            float4 _pad496;
            float4 _pad512;
            float4 _pad528;
            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4 _TilingOffset;
            float4 _MainLightColor;
            float4 _AdditionalLightsCount;
            float3 _WorldSpaceCameraPos;
            float4 _pad352;
            float4 _AdditionalLightsColor;
            float4 _pad8192;
            float4 _pad12288;
            float4 unity_LightData;
            float4 unity_LightIndices;
            float4 unity_SpecCube0_HDR;
            float _VColEmissive;
            float _VertexEmissiveStr;
            float4 _Color;
            float _EnableMaskMap;
            float _Reflections;
            float _NormalStrength;
            float2 _SmoothRemap;
            float _DoubleSizeDither;
            float _MaskTint;
            float _DitherAmbient;
            float _DitherDiffuse;
            float _DitherSpec;

            float4 u_xlat0;
            float4 u_xlat1;
            float u_xlat7;
            float4 ImmCB_0_0_0[3];
            float ImmCB_0_3_0[64];
            int u_xlati0;
            uint u_xlatu0;
            int u_xlati1;
            uint2 u_xlatu1;
            bool3 u_xlatb1;
            float3 u_xlat2;
            bool3 u_xlatb2;
            float3 u_xlat3;
            float3 u_xlat4;
            float4 u_xlat5;
            float4 u_xlat6;
            bool4 u_xlatb7;
            float4 u_xlat8;
            float4 u_xlat9;
            bool2 u_xlatb9;
            float3 u_xlat10;
            float3 u_xlat11;
            float3 u_xlat12;
            float3 u_xlat13;
            float u_xlat14;
            bool2 u_xlatb26;
            float u_xlat28;
            int u_xlati29;
            uint2 u_xlatu29;
            float u_xlat42;
            int u_xlati42;
            float u_xlat43;
            float u_xlat44;
            float u_xlat45;
            bool u_xlatb45;
            float u_xlat46;
            bool u_xlatb46;
            float u_xlat48;
            float u_xlat49;
            uint u_xlatu50;
            float u_xlat51;
            int u_xlati51;
            uint u_xlatu51;
            bool u_xlatb51;
            float u_xlat52;
            int u_xlati52;
            float u_xlat53;
            float u_xlat54;



            struct Attributes
            {
                float4 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TANGENT0 : TANGENT;
                float4 in_TEXCOORD0 : TEXCOORD0;
                float4 in_TEXCOORD1 : TEXCOORD1;
                float4 in_COLOR0 : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 vs_COLOR0 : TEXCOORD0;
                float3 vs_TEXCOORD0 : TEXCOORD1;
                float3 vs_TEXCOORD1 : TEXCOORD2;
                float3 vs_TEXCOORD10 : TEXCOORD3;
                float4 vs_TEXCOORD11 : TEXCOORD4;
                float4 vs_TEXCOORD13 : TEXCOORD5;
                float4 vs_TEXCOORD14 : TEXCOORD6;
                float4 vs_TEXCOORD15 : TEXCOORD7;
                float4 vs_TEXCOORD16 : TEXCOORD8;
                float4 vs_TEXCOORD17 : TEXCOORD9;
                float4 vs_TEXCOORD2 : TEXCOORD10;
                float4 vs_TEXCOORD3 : TEXCOORD11;
                float4 vs_TEXCOORD4 : TEXCOORD12;
                float4 vs_TEXCOORD7 : TEXCOORD13;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0.xyz = input.in_POSITION0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[0].xyz * input.in_POSITION0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * input.in_POSITION0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.vs_TEXCOORD0.xyz = u_xlat0.xyz;
    u_xlat0 = u_xlat1 + _tunity_MatrixVP[3];
    output.positionCS = u_xlat0;
    u_xlat1.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat1.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat1.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat7 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat7 = max(u_xlat7, 1.17549435e-38);
    u_xlat7 = _g_inversesqrt(u_xlat7);
    output.vs_TEXCOORD1.xyz = float3(u_xlat7) * u_xlat1.xyz;
    u_xlat1.xyz = input.in_TANGENT0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat1.xyz = _tunity_ObjectToWorld[0].xyz * input.in_TANGENT0.xxx + u_xlat1.xyz;
    u_xlat1.xyz = _tunity_ObjectToWorld[2].xyz * input.in_TANGENT0.zzz + u_xlat1.xyz;
    u_xlat7 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat7 = max(u_xlat7, 1.17549435e-38);
    u_xlat7 = _g_inversesqrt(u_xlat7);
    output.vs_TEXCOORD2.xyz = float3(u_xlat7) * u_xlat1.xyz;
    output.vs_TEXCOORD2.w = input.in_TANGENT0.w;
    output.vs_TEXCOORD3.xy = input.in_TEXCOORD0.xy * _TilingOffset.xy + _TilingOffset.zw;
    output.vs_TEXCOORD3.zw = input.in_TEXCOORD0.zw;
    output.vs_TEXCOORD4 = input.in_TEXCOORD1;
    u_xlat0.y = u_xlat0.y * _ProjectionParams.x;
    u_xlat1.xzw = u_xlat0.xwy * float3(0.5, 0.5, 0.5);
    output.vs_TEXCOORD7.zw = u_xlat0.zw;
    output.vs_TEXCOORD7.xy = u_xlat1.zz + u_xlat1.xw;
    output.vs_COLOR0 = input.in_COLOR0;
    output.vs_TEXCOORD10.xyz = float3(0.0, 0.0, 0.0);
    output.vs_TEXCOORD11 = float4(0.0, 0.0, 0.0, 0.0);
    output.vs_TEXCOORD13 = float4(0.0, 0.0, 0.0, 0.0);
    output.vs_TEXCOORD14 = float4(0.0, 0.0, 0.0, 0.0);
    output.vs_TEXCOORD15 = float4(0.0, 0.0, 0.0, 0.0);
    output.vs_TEXCOORD16 = float4(0.0, 0.0, 0.0, 0.0);
    output.vs_TEXCOORD17 = float4(0.0, 0.0, 0.0, 0.0);
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_3_0[0] = 0.0;
	ImmCB_0_3_0[1] = 4.48415509e-44;
	ImmCB_0_3_0[2] = 1.12103877e-44;
	ImmCB_0_3_0[3] = 5.60519386e-44;
	ImmCB_0_3_0[4] = 2.80259693e-45;
	ImmCB_0_3_0[5] = 4.76441478e-44;
	ImmCB_0_3_0[6] = 1.40129846e-44;
	ImmCB_0_3_0[7] = 5.88545355e-44;
	ImmCB_0_3_0[8] = 6.72623263e-44;
	ImmCB_0_3_0[9] = 2.24207754e-44;
	ImmCB_0_3_0[10] = 7.8472714e-44;
	ImmCB_0_3_0[11] = 3.36311631e-44;
	ImmCB_0_3_0[12] = 7.00649232e-44;
	ImmCB_0_3_0[13] = 2.52233724e-44;
	ImmCB_0_3_0[14] = 8.12753109e-44;
	ImmCB_0_3_0[15] = 3.64337601e-44;
	ImmCB_0_3_0[16] = 1.68155816e-44;
	ImmCB_0_3_0[17] = 6.16571324e-44;
	ImmCB_0_3_0[18] = 5.60519386e-45;
	ImmCB_0_3_0[19] = 5.04467447e-44;
	ImmCB_0_3_0[20] = 1.96181785e-44;
	ImmCB_0_3_0[21] = 6.44597294e-44;
	ImmCB_0_3_0[22] = 8.40779079e-45;
	ImmCB_0_3_0[23] = 5.32493416e-44;
	ImmCB_0_3_0[24] = 8.40779079e-44;
	ImmCB_0_3_0[25] = 3.9236357e-44;
	ImmCB_0_3_0[26] = 7.28675201e-44;
	ImmCB_0_3_0[27] = 2.80259693e-44;
	ImmCB_0_3_0[28] = 8.68805048e-44;
	ImmCB_0_3_0[29] = 4.20389539e-44;
	ImmCB_0_3_0[30] = 7.56701171e-44;
	ImmCB_0_3_0[31] = 3.08285662e-44;
	ImmCB_0_3_0[32] = 4.20389539e-45;
	ImmCB_0_3_0[33] = 4.90454463e-44;
	ImmCB_0_3_0[34] = 1.54142831e-44;
	ImmCB_0_3_0[35] = 6.0255834e-44;
	ImmCB_0_3_0[36] = 1.40129846e-45;
	ImmCB_0_3_0[37] = 4.62428493e-44;
	ImmCB_0_3_0[38] = 1.26116862e-44;
	ImmCB_0_3_0[39] = 5.7453237e-44;
	ImmCB_0_3_0[40] = 7.14662217e-44;
	ImmCB_0_3_0[41] = 2.66246708e-44;
	ImmCB_0_3_0[42] = 8.26766094e-44;
	ImmCB_0_3_0[43] = 3.78350585e-44;
	ImmCB_0_3_0[44] = 6.86636248e-44;
	ImmCB_0_3_0[45] = 2.38220739e-44;
	ImmCB_0_3_0[46] = 7.98740125e-44;
	ImmCB_0_3_0[47] = 3.50324616e-44;
	ImmCB_0_3_0[48] = 2.1019477e-44;
	ImmCB_0_3_0[49] = 6.58610278e-44;
	ImmCB_0_3_0[50] = 9.80908925e-45;
	ImmCB_0_3_0[51] = 5.46506401e-44;
	ImmCB_0_3_0[52] = 1.821688e-44;
	ImmCB_0_3_0[53] = 6.30584309e-44;
	ImmCB_0_3_0[54] = 7.00649232e-45;
	ImmCB_0_3_0[55] = 5.18480432e-44;
	ImmCB_0_3_0[56] = 8.82818033e-44;
	ImmCB_0_3_0[57] = 4.34402524e-44;
	ImmCB_0_3_0[58] = 7.70714155e-44;
	ImmCB_0_3_0[59] = 3.22298647e-44;
	ImmCB_0_3_0[60] = 8.54792063e-44;
	ImmCB_0_3_0[61] = 4.06376555e-44;
	ImmCB_0_3_0[62] = 7.42688186e-44;
	ImmCB_0_3_0[63] = 2.94272678e-44;
float4 hlslcc_FragCoord = float4(input.positionCS.xyz, 1.0/input.positionCS.w);
    u_xlat0.x = dot(input.vs_TEXCOORD1.xyz, input.vs_TEXCOORD1.xyz);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat0.xyz = u_xlat0.xxx * input.vs_TEXCOORD1.xyz;
    u_xlat42 = dot(input.vs_TEXCOORD2.xyz, input.vs_TEXCOORD2.xyz);
    u_xlat42 = _g_inversesqrt(u_xlat42);
    u_xlat1.xyz = float3(u_xlat42) * input.vs_TEXCOORD2.xyz;
    u_xlat2.xyz = input.vs_TEXCOORD1.yzx * input.vs_TEXCOORD2.zxy;
    u_xlat2.xyz = input.vs_TEXCOORD2.yzx * input.vs_TEXCOORD1.zxy + (-u_xlat2.xyz);
    u_xlat2.xyz = u_xlat2.xyz * input.vs_TEXCOORD2.www;
    u_xlat3.xyz = (-input.vs_TEXCOORD0.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat42 = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat42 = _g_inversesqrt(u_xlat42);
    u_xlat4.xyz = float3(u_xlat42) * u_xlat3.xyz;
    u_xlat5 = _g_texture(_Texture_t2, input.vs_TEXCOORD3.xy, _pad64.x).xwyz;
    u_xlat6 = _g_texture(_Texture_t3, input.vs_TEXCOORD3.xy, _pad64.x);
    u_xlat7.xyz = u_xlat5.xzw * _Color.xyz;
    u_xlat8.xyz = u_xlat5.xzw * _Color.xyz + (-u_xlat5.xzw);
    u_xlat8.xyz = u_xlat6.www * u_xlat8.xyz + u_xlat5.xzw;
    u_xlat7.xyz = (_g_floatBitsToInt(float(_MaskTint)) != 0) ? u_xlat8.xyz : u_xlat7.xyz;
    u_xlat8 = _g_texture(_Texture_t4, input.vs_TEXCOORD3.xy, _pad64.x);
    u_xlat8.x = u_xlat8.w * u_xlat8.x;
    u_xlat8.xy = u_xlat8.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat8.xy = u_xlat8.xy * float2(float2(_NormalStrength, _NormalStrength));
    u_xlat43 = dot(u_xlat8.xy, u_xlat8.xy);
    u_xlat43 = min(u_xlat43, 1.0);
    u_xlat43 = (-u_xlat43) + 1.0;
    u_xlat43 = sqrt(u_xlat43);
    u_xlat2.xyz = (-u_xlat2.xyz) * u_xlat8.yyy;
    u_xlat1.xyz = u_xlat8.xxx * u_xlat1.xyz + u_xlat2.xyz;
    u_xlat1.xyz = float3(u_xlat43) * u_xlat0.xyz + u_xlat1.xyz;
    u_xlat5.x = float(0.0);
    u_xlat5.z = float(1.0);
    u_xlat0.xyz = (_g_floatBitsToInt(float(_EnableMaskMap)) != 0) ? u_xlat6.xyz : u_xlat5.xyz;
    u_xlat2.x = (-_SmoothRemap.x) + _SmoothRemap.y;
    u_xlat14 = u_xlat0.y * u_xlat2.x + _SmoothRemap.x;
    u_xlat2.xyz = u_xlat7.xyz + float3(-1.0, -1.0, -1.0);
    u_xlat2.xyz = u_xlat0.xxx * u_xlat2.xyz + float3(1.0, 1.0, 1.0);
    u_xlat44 = u_xlat0.x * 0.959999979 + 0.0399999991;
    u_xlat0.x = (-u_xlat0.x) + 1.0;
    u_xlat5.xzw = u_xlat0.xxx * u_xlat7.xyz;
    u_xlat5.xzw = (_g_floatBitsToInt(float(_Reflections)) != 0) ? u_xlat5.xzw : u_xlat7.xyz;
    if(uint(_g_floatBitsToUint(_Reflections)) != uint(0)) {
        u_xlat0.x = (-u_xlat14) + 1.0;
        u_xlat0.x = u_xlat0.x * u_xlat0.x;
        u_xlat45 = (-u_xlat0.x) * 0.699999988 + 1.70000005;
        u_xlat0.x = u_xlat0.x * u_xlat45;
        u_xlat0.x = u_xlat0.x * 6.0;
        u_xlat45 = dot((-u_xlat4.xyz), u_xlat1.xyz);
        u_xlat45 = u_xlat45 + u_xlat45;
        u_xlat6.xyz = u_xlat1.xyz * (-float3(u_xlat45)) + (-u_xlat4.xyz);
        u_xlat6 = _g_textureLod(_Texture_t0, u_xlat6.xyz, u_xlat0.x);
        u_xlat0.x = u_xlat6.w + -1.0;
        u_xlat0.x = unity_SpecCube0_HDR.w * u_xlat0.x + 1.0;
        u_xlat0.x = max(u_xlat0.x, 0.0);
        u_xlat0.x = log2(u_xlat0.x);
        u_xlat0.x = u_xlat0.x * unity_SpecCube0_HDR.y;
        u_xlat0.x = exp2(u_xlat0.x);
        u_xlat0.x = u_xlat0.x * unity_SpecCube0_HDR.x;
        u_xlat6.xyz = u_xlat6.xyz * u_xlat0.xxx;
    } else {
        u_xlat6.x = float(0.0);
        u_xlat6.y = float(0.0);
        u_xlat6.z = float(0.0);
    }
    u_xlat7.xyz = input.vs_TEXCOORD0.xyz + (-_pad320.xyz);
    u_xlat8.xyz = input.vs_TEXCOORD0.xyz + (-_pad336.xyz);
    u_xlat9.xyz = input.vs_TEXCOORD0.xyz + (-_pad352.xyz);
    u_xlat10.xyz = input.vs_TEXCOORD0.xyz + (-_pad368.xyz);
    u_xlat7.x = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat7.y = dot(u_xlat8.xyz, u_xlat8.xyz);
    u_xlat7.z = dot(u_xlat9.xyz, u_xlat9.xyz);
    u_xlat7.w = dot(u_xlat10.xyz, u_xlat10.xyz);
    u_xlatb7 = _g_lessThan(u_xlat7, _pad384);
    u_xlat8.x = u_xlatb7.x ? float(1.0) : 0.0;
    u_xlat8.y = u_xlatb7.y ? float(1.0) : 0.0;
    u_xlat8.z = u_xlatb7.z ? float(1.0) : 0.0;
    u_xlat8.w = u_xlatb7.w ? float(1.0) : 0.0;
;
    u_xlat7.x = (u_xlatb7.x) ? float(-1.0) : float(-0.0);
    u_xlat7.y = (u_xlatb7.y) ? float(-1.0) : float(-0.0);
    u_xlat7.z = (u_xlatb7.z) ? float(-1.0) : float(-0.0);
    u_xlat7.xyz = u_xlat7.xyz + u_xlat8.yzw;
    u_xlat8.yzw = max(u_xlat7.xyz, float3(0.0, 0.0, 0.0));
    u_xlat0.x = dot(u_xlat8, float4(4.0, 3.0, 2.0, 1.0));
    u_xlat0.x = (-u_xlat0.x) + 4.0;
    u_xlatu0 = uint(u_xlat0.x);
    u_xlati0 = int(int(u_xlatu0) << 2);
    u_xlat7.xyz = input.vs_TEXCOORD0.yyy * _pad16.xyz;
    u_xlat7.xyz = _pad0.xyz * input.vs_TEXCOORD0.xxx + u_xlat7.xyz;
    u_xlat7.xyz = _pad32.xyz * input.vs_TEXCOORD0.zzz + u_xlat7.xyz;
    u_xlat7.xyz = u_xlat7.xyz + _pad48.xyz;
    float3 txVec0 = float3(u_xlat7.xy,u_xlat7.z);
    u_xlat0.x = _g_textureLod(_Texture_t1, txVec0, 0.0);
    u_xlat45 = (-_pad432.x) + 1.0;
    u_xlat0.x = u_xlat0.x * _pad432.x + u_xlat45;
    u_xlatb45 = 0.0>=u_xlat7.z;
    u_xlatb46 = u_xlat7.z>=1.0;
    u_xlatb45 = u_xlatb45 || u_xlatb46;
    u_xlat0.x = (u_xlatb45) ? 1.0 : u_xlat0.x;
    u_xlat7.xyz = input.vs_TEXCOORD0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat45 = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat45 = u_xlat45 * _pad432.z + _pad432.w;
    u_xlat45 = clamp(u_xlat45, 0.0, 1.0);
    u_xlat46 = (-u_xlat0.x) + 1.0;
    u_xlat0.x = u_xlat45 * u_xlat46 + u_xlat0.x;
    u_xlat45 = dot(u_xlat1.xyz, _pad80.xyz);
    u_xlat46 = u_xlat45 * 0.5 + 0.5;
    u_xlat45 = (_g_floatBitsToInt(_pad32) != 0) ? u_xlat46 : u_xlat45;
    u_xlat45 = max(u_xlat45, 0.0);
    u_xlat45 = u_xlat45 * unity_LightData.z;
    u_xlat7.xyz = float3(u_xlat45) * _MainLightColor.xyz;
    u_xlat7.xyz = u_xlat0.xxx * u_xlat7.xyz;
    u_xlat1.w = 1.0;
    u_xlat8.x = dot(_pad432, u_xlat1);
    u_xlat8.y = dot(_pad448, u_xlat1);
    u_xlat8.z = dot(_pad464, u_xlat1);
    u_xlat9 = u_xlat1.yzzx * u_xlat1.xyzz;
    u_xlat10.x = dot(_pad480, u_xlat9);
    u_xlat10.y = dot(_pad496, u_xlat9);
    u_xlat10.z = dot(_pad512, u_xlat9);
    u_xlat43 = u_xlat1.y * u_xlat1.y;
    u_xlat43 = u_xlat1.x * u_xlat1.x + (-u_xlat43);
    u_xlat9.xyz = _pad528.xyz * float3(u_xlat43) + u_xlat10.xyz;
    u_xlat8.xyz = u_xlat8.xyz + u_xlat9.xyz;
    u_xlat28 = min(u_xlat0.z, 1.0);
    u_xlat8.xyz = float3(u_xlat28) * u_xlat8.xyz;
    u_xlat28 = u_xlat14 * 10.0 + 1.0;
    u_xlat28 = exp2(u_xlat28);
    if(uint(_g_floatBitsToUint(_pad48)) == uint(0)) {
        u_xlat43 = (-u_xlat14) + 1.0;
        u_xlat43 = u_xlat43 * u_xlat43;
        u_xlat9.xyz = u_xlat3.xyz * float3(u_xlat42) + _pad80.xyz;
        u_xlat45 = dot(u_xlat9.xyz, u_xlat9.xyz);
        u_xlat45 = max(u_xlat45, 1.17549435e-38);
        u_xlat45 = _g_inversesqrt(u_xlat45);
        u_xlat9.xyz = float3(u_xlat45) * u_xlat9.xyz;
        u_xlat45 = dot(u_xlat1.xyz, u_xlat9.xyz);
        u_xlat45 = clamp(u_xlat45, 0.0, 1.0);
        u_xlat46 = dot(_pad80.xyz, u_xlat9.xyz);
        u_xlat46 = clamp(u_xlat46, 0.0, 1.0);
        u_xlat45 = u_xlat45 * u_xlat45;
        u_xlat48 = u_xlat43 * u_xlat43;
        u_xlat49 = u_xlat43 * u_xlat43 + -1.0;
        u_xlat45 = u_xlat45 * u_xlat49 + 1.00001001;
        u_xlat43 = u_xlat43 * 4.0 + 2.0;
        u_xlat45 = u_xlat45 * u_xlat45;
        u_xlat46 = u_xlat46 * u_xlat46;
        u_xlat46 = max(u_xlat46, 0.100000001);
        u_xlat45 = u_xlat45 * u_xlat46;
        u_xlat43 = u_xlat43 * u_xlat45;
        u_xlat43 = u_xlat48 / u_xlat43;
        u_xlat43 = u_xlat43 * 0.0399999991;
    } else {
        u_xlat45 = dot((-_pad80.xyz), u_xlat1.xyz);
        u_xlat45 = u_xlat45 + u_xlat45;
        u_xlat9.xyz = u_xlat1.xyz * (-float3(u_xlat45)) + (-_pad80.xyz);
        u_xlat45 = dot(u_xlat9.xyz, u_xlat4.xyz);
        u_xlat45 = max(u_xlat45, 0.0);
        u_xlat45 = log2(u_xlat45);
        u_xlat45 = u_xlat28 * u_xlat45;
        u_xlat45 = exp2(u_xlat45);
        u_xlat45 = u_xlat14 * u_xlat45;
        u_xlatb9.xy = _g_equal(_g_floatBitsToInt(float4(_pad48)), int4(1, 2, 0, 0)).xy;
        u_xlat3.xyz = u_xlat3.xyz * float3(u_xlat42) + _pad80.xyz;
        u_xlat42 = dot(u_xlat3.xyz, u_xlat3.xyz);
        u_xlat42 = max(u_xlat42, 1.17549435e-38);
        u_xlat42 = _g_inversesqrt(u_xlat42);
        u_xlat3.xyz = float3(u_xlat42) * u_xlat3.xyz;
        u_xlat42 = dot(u_xlat1.xyz, u_xlat3.xyz);
        u_xlat42 = clamp(u_xlat42, 0.0, 1.0);
        u_xlat42 = log2(u_xlat42);
        u_xlat42 = u_xlat42 * u_xlat28;
        u_xlat42 = exp2(u_xlat42);
        u_xlat42 = u_xlat14 * u_xlat42;
        u_xlat42 = u_xlatb9.y ? u_xlat42 : float(0.0);
        u_xlat43 = (u_xlatb9.x) ? u_xlat45 : u_xlat42;
    }
    u_xlat3.xyz = _MainLightColor.xyz * unity_LightData.zzz;
    u_xlat3.xyz = u_xlat2.xyz * u_xlat3.xyz;
    u_xlat3.xyz = float3(u_xlat43) * u_xlat3.xyz;
    u_xlat3.xyz = u_xlat0.xxx * u_xlat3.xyz;
    u_xlat0.x = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlati0 = int(u_xlat0.x);
    u_xlat42 = (-u_xlat14) + 1.0;
    u_xlat43 = u_xlat42 * u_xlat42;
    u_xlat45 = u_xlat43 * u_xlat43;
    u_xlat46 = u_xlat43 * u_xlat43 + -1.0;
    u_xlat43 = u_xlat43 * 4.0 + 2.0;
    u_xlat9.x = float(0.0);
    u_xlat9.y = float(0.0);
    u_xlat9.z = float(0.0);
    u_xlat10.x = float(0.0);
    u_xlat10.y = float(0.0);
    u_xlat10.z = float(0.0);
    u_xlat48 = _pad32;
    u_xlat49 = _pad48;
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<uint(u_xlati0) ; u_xlatu_loop_1++)
    {
        u_xlatu51 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati52 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat51 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati52]);
        u_xlati51 = int(u_xlat51);
        u_xlat11.xyz = (-input.vs_TEXCOORD0.xyz) * _pad0.www + _pad0.xyz;
        u_xlat52 = dot(u_xlat11.xyz, u_xlat11.xyz);
        u_xlat52 = max(u_xlat52, 6.10351562e-05);
        u_xlat53 = _g_inversesqrt(u_xlat52);
        u_xlat12.xyz = float3(u_xlat53) * u_xlat11.xyz;
        u_xlat54 = float(1.0) / u_xlat52;
        u_xlat52 = u_xlat52 * _pad8192.x;
        u_xlat52 = (-u_xlat52) * u_xlat52 + 1.0;
        u_xlat52 = max(u_xlat52, 0.0);
        u_xlat52 = u_xlat52 * u_xlat52;
        u_xlat52 = u_xlat52 * u_xlat54;
        u_xlat54 = dot(_pad12288.xyz, u_xlat12.xyz);
        u_xlat54 = u_xlat54 * _pad8192.z + _pad8192.w;
        u_xlat54 = clamp(u_xlat54, 0.0, 1.0);
        u_xlat54 = u_xlat54 * u_xlat54;
        u_xlat52 = u_xlat52 * u_xlat54;
        u_xlat54 = dot(u_xlat12.xyz, u_xlat1.xyz);
        u_xlat13.x = u_xlat54 * 0.5 + 0.5;
        u_xlat54 = (_g_floatBitsToInt(u_xlat48) != 0) ? u_xlat13.x : u_xlat54;
        u_xlat54 = max(u_xlat54, 0.0);
        u_xlat54 = u_xlat52 * u_xlat54;
        u_xlat9.xyz = float3(u_xlat54) * _AdditionalLightsColor.xyz + u_xlat9.xyz;
        if(uint(_g_floatBitsToUint(u_xlat49)) == uint(0)) {
            u_xlat13.xyz = u_xlat11.xyz * float3(u_xlat53) + u_xlat4.xyz;
            u_xlat54 = dot(u_xlat13.xyz, u_xlat13.xyz);
            u_xlat54 = max(u_xlat54, 1.17549435e-38);
            u_xlat54 = _g_inversesqrt(u_xlat54);
            u_xlat13.xyz = float3(u_xlat54) * u_xlat13.xyz;
            u_xlat54 = dot(u_xlat1.xyz, u_xlat13.xyz);
            u_xlat54 = clamp(u_xlat54, 0.0, 1.0);
            u_xlat13.x = dot(u_xlat12.xyz, u_xlat13.xyz);
            u_xlat13.x = clamp(u_xlat13.x, 0.0, 1.0);
            u_xlat54 = u_xlat54 * u_xlat54;
            u_xlat54 = u_xlat54 * u_xlat46 + 1.00001001;
            u_xlat54 = u_xlat54 * u_xlat54;
            u_xlat13.x = u_xlat13.x * u_xlat13.x;
            u_xlat13.x = max(u_xlat13.x, 0.100000001);
            u_xlat54 = u_xlat54 * u_xlat13.x;
            u_xlat54 = u_xlat43 * u_xlat54;
            u_xlat54 = u_xlat45 / u_xlat54;
            u_xlat54 = u_xlat54 * 0.0399999991;
        } else {
            u_xlat13.x = dot((-u_xlat12.xyz), u_xlat1.xyz);
            u_xlat13.x = u_xlat13.x + u_xlat13.x;
            u_xlat12.xyz = u_xlat1.xyz * (-u_xlat13.xxx) + (-u_xlat12.xyz);
            u_xlat12.x = dot(u_xlat12.xyz, u_xlat4.xyz);
            u_xlat12.x = max(u_xlat12.x, 0.0);
            u_xlat12.x = log2(u_xlat12.x);
            u_xlat12.x = u_xlat28 * u_xlat12.x;
            u_xlat12.x = exp2(u_xlat12.x);
            u_xlat12.x = u_xlat14 * u_xlat12.x;
            u_xlatb26.xy = _g_equal(_g_floatBitsToInt(float4(u_xlat49)), int4(1, 2, 0, 0)).xy;
            u_xlat11.xyz = u_xlat11.xyz * float3(u_xlat53) + u_xlat4.xyz;
            u_xlat53 = dot(u_xlat11.xyz, u_xlat11.xyz);
            u_xlat53 = max(u_xlat53, 1.17549435e-38);
            u_xlat53 = _g_inversesqrt(u_xlat53);
            u_xlat11.xyz = float3(u_xlat53) * u_xlat11.xyz;
            u_xlat11.x = dot(u_xlat1.xyz, u_xlat11.xyz);
            u_xlat11.x = clamp(u_xlat11.x, 0.0, 1.0);
            u_xlat11.x = log2(u_xlat11.x);
            u_xlat11.x = u_xlat28 * u_xlat11.x;
            u_xlat11.x = exp2(u_xlat11.x);
            u_xlat11.x = u_xlat14 * u_xlat11.x;
            u_xlat11.x = u_xlatb26.y ? u_xlat11.x : float(0.0);
            u_xlat54 = (u_xlatb26.x) ? u_xlat12.x : u_xlat11.x;
        }
        u_xlat52 = u_xlat52 * u_xlat54;
        u_xlat11.xyz = float3(u_xlat52) * _AdditionalLightsColor.xyz;
        u_xlat10.xyz = u_xlat11.xyz * u_xlat2.xyz + u_xlat10.xyz;
    }
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat4.xyz);
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat0.x = (-u_xlat0.x) + 1.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat14 = dot(u_xlat7.xyz, float3(0.298999995, 0.587000012, 0.114));
    u_xlat14 = u_xlat14 * 0.899999976 + 0.100000001;
    u_xlat28 = u_xlat14 * u_xlat44;
    u_xlat1.x = u_xlat28 * 10.0;
    u_xlat1.x = clamp(u_xlat1.x, 0.0, 1.0);
    u_xlat14 = (-u_xlat44) * u_xlat14 + u_xlat1.x;
    u_xlat0.x = u_xlat0.x * u_xlat14 + u_xlat28;
    u_xlat0.xyz = u_xlat2.xyz * u_xlat0.xxx;
    u_xlat0.xyz = u_xlat0.xyz * u_xlat6.xyz;
    u_xlat42 = u_xlat42 * u_xlat42 + 0.5;
    u_xlat0.xyz = u_xlat0.xyz / float3(u_xlat42);
    u_xlat1.xy = hlslcc_FragCoord.yx * float2(0.5, 0.5);
    u_xlat1.xy = (_g_floatBitsToInt(_DoubleSizeDither) != 0) ? u_xlat1.xy : hlslcc_FragCoord.yx;
    if(uint(_g_floatBitsToUint(_DitherDiffuse)) != uint(0)) {
        u_xlatu29.xy = uint2(u_xlat1.xy);
        u_xlati42 = int(int(u_xlatu29.x) << 3);
        u_xlati42 = int(uint(uint(u_xlati42) & 56u));
        u_xlati29 = int(uint(u_xlatu29.y & 7u));
        u_xlati42 = u_xlati42 + u_xlati29;
        u_xlat42 = float(uint(_g_floatBitsToUint(ImmCB_0_3_0[u_xlati42])));
        u_xlat42 = u_xlat42 * 0.015625;
        u_xlatb2.xyz = _g_lessThan(u_xlat7.xyzx, float4(u_xlat42)).xyz;
        u_xlat4.xyz = u_xlat7.xyz * float3(0.25, 0.25, 0.25);
        {
            float4 hlslcc_movcTemp = u_xlat7;
            hlslcc_movcTemp.x = (u_xlatb2.x) ? u_xlat4.x : u_xlat7.x;
            hlslcc_movcTemp.y = (u_xlatb2.y) ? u_xlat4.y : u_xlat7.y;
            hlslcc_movcTemp.z = (u_xlatb2.z) ? u_xlat4.z : u_xlat7.z;
            u_xlat7 = hlslcc_movcTemp;
        }
        u_xlatb2.xyz = _g_lessThan(u_xlat9.xyzx, float4(u_xlat42)).xyz;
        u_xlat4.xyz = u_xlat9.xyz * float3(0.25, 0.25, 0.25);
        {
            float4 hlslcc_movcTemp = u_xlat9;
            hlslcc_movcTemp.x = (u_xlatb2.x) ? u_xlat4.x : u_xlat9.x;
            hlslcc_movcTemp.y = (u_xlatb2.y) ? u_xlat4.y : u_xlat9.y;
            hlslcc_movcTemp.z = (u_xlatb2.z) ? u_xlat4.z : u_xlat9.z;
            u_xlat9 = hlslcc_movcTemp;
        }
    }
    if(uint(_g_floatBitsToUint(_DitherSpec)) != uint(0)) {
        u_xlatu29.xy = uint2(u_xlat1.xy);
        u_xlati42 = int(int(u_xlatu29.x) << 3);
        u_xlati42 = int(uint(uint(u_xlati42) & 56u));
        u_xlati29 = int(uint(u_xlatu29.y & 7u));
        u_xlati42 = u_xlati42 + u_xlati29;
        u_xlat42 = float(uint(_g_floatBitsToUint(ImmCB_0_3_0[u_xlati42])));
        u_xlat42 = u_xlat42 * 0.015625;
        u_xlatb2.xyz = _g_lessThan(u_xlat3.xyzx, float4(u_xlat42)).xyz;
        u_xlat4.xyz = u_xlat3.xyz * float3(0.25, 0.25, 0.25);
        {
            float3 hlslcc_movcTemp = u_xlat3;
            hlslcc_movcTemp.x = (u_xlatb2.x) ? u_xlat4.x : u_xlat3.x;
            hlslcc_movcTemp.y = (u_xlatb2.y) ? u_xlat4.y : u_xlat3.y;
            hlslcc_movcTemp.z = (u_xlatb2.z) ? u_xlat4.z : u_xlat3.z;
            u_xlat3 = hlslcc_movcTemp;
        }
        u_xlatb2.xyz = _g_lessThan(u_xlat10.xyzx, float4(u_xlat42)).xyz;
        u_xlat4.xyz = u_xlat10.xyz * float3(0.25, 0.25, 0.25);
        {
            float3 hlslcc_movcTemp = u_xlat10;
            hlslcc_movcTemp.x = (u_xlatb2.x) ? u_xlat4.x : u_xlat10.x;
            hlslcc_movcTemp.y = (u_xlatb2.y) ? u_xlat4.y : u_xlat10.y;
            hlslcc_movcTemp.z = (u_xlatb2.z) ? u_xlat4.z : u_xlat10.z;
            u_xlat10 = hlslcc_movcTemp;
        }
    }
    if(uint(_g_floatBitsToUint(_DitherAmbient)) != uint(0)) {
        u_xlatu1.xy = uint2(u_xlat1.xy);
        u_xlati42 = int(int(u_xlatu1.x) << 3);
        u_xlati42 = int(uint(uint(u_xlati42) & 56u));
        u_xlati1 = int(uint(u_xlatu1.y & 7u));
        u_xlati42 = u_xlati42 + u_xlati1;
        u_xlat42 = float(uint(_g_floatBitsToUint(ImmCB_0_3_0[u_xlati42])));
        u_xlat42 = u_xlat42 * 0.015625;
        u_xlatb1.xyz = _g_lessThan(u_xlat8.xyzx, float4(u_xlat42)).xyz;
        u_xlat2.xyz = u_xlat8.xyz * float3(0.25, 0.25, 0.25);
        {
            float4 hlslcc_movcTemp = u_xlat8;
            hlslcc_movcTemp.x = (u_xlatb1.x) ? u_xlat2.x : u_xlat8.x;
            hlslcc_movcTemp.y = (u_xlatb1.y) ? u_xlat2.y : u_xlat8.y;
            hlslcc_movcTemp.z = (u_xlatb1.z) ? u_xlat2.z : u_xlat8.z;
            u_xlat8 = hlslcc_movcTemp;
        }
    }
    __SV_Target0.w = u_xlat5.y * _Color.w;
    u_xlat42 = max(_VertexEmissiveStr, 0.0);
    u_xlat1.xyz = input.vs_COLOR0.xyz * float3(u_xlat42) + u_xlat5.xzw;
    u_xlat2.xyz = u_xlat5.xzw * input.vs_COLOR0.xyz;
    u_xlat1.xyz = (_g_floatBitsToInt(_VColEmissive) != 0) ? u_xlat1.xyz : u_xlat2.xyz;
    u_xlat2.xyz = u_xlat1.xyz * u_xlat8.xyz;
    u_xlat4.xyz = u_xlat7.xyz + u_xlat9.xyz;
    u_xlat1.xyz = u_xlat1.xyz * u_xlat4.xyz + u_xlat2.xyz;
    u_xlat42 = dot(u_xlat7.xyz, float3(0.298999995, 0.587000012, 0.114));
    u_xlat42 = clamp(u_xlat42, 0.0, 1.0);
    u_xlat1.xyz = u_xlat3.xyz * float3(u_xlat42) + u_xlat1.xyz;
    u_xlat42 = dot(u_xlat9.xyz, float3(0.298999995, 0.587000012, 0.114));
    u_xlat42 = clamp(u_xlat42, 0.0, 1.0);
    u_xlat1.xyz = u_xlat10.xyz * float3(u_xlat42) + u_xlat1.xyz;
    __SV_Target0.xyz = u_xlat0.xyz + u_xlat1.xyz;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
}
