Shader "Shader Graphs/DefaultMoving"
{
    Properties
    {


[NoScaleOffset] _Colors ("Colors", 2D) = "white" {}
_Emission ("Emission", Float) = 0
[NoScaleOffset] _Normal_Map ("Normal Map", 2D) = "white" {}
_Normal_Strength ("Normal Strength", Range(0, 1)) = 1
_Normal_Scale ("Normal Scale", Float) = 0.2
_Cookness ("Cookness", Range(0, 2)) = 0
_CookColor ("CookColor", Vector) = (1,0.4874805,0,1)
_BurntColor ("BurntColor", Vector) = (0,0,0,1)
_Cooked_Normal_Strength ("Cooked Normal Strength", Range(0, 2)) = 0.7
[ToggleUI] _Use_Skin ("Use Skin", Float) = 0
[ToggleUI] _Rainbow_Skin ("Rainbow Skin", Float) = 0
[KeywordEnum(Gradient Noise, Checkerboard, Voronoi)] _SKIN_TYPE ("Skin Type", Float) = 0
[KeywordEnum(Only Metallic, Only Metallic Noise, Everything)] _SKIN_AFFECTS ("Skin Affects", Float) = 0
_Metallic_Cutoff ("Metallic Cutoff", Range(0, 1)) = 0.5
_Color_Offset ("Color Offset", Vector) = (0,0,0,0)
_Color_Offset_2 ("Color Offset 2", Vector) = (0,0,0,0)
_Skin_Smooth_Step ("Skin Smooth Step", Vector) = (0.5,0.5,0,0)
_Skin_Noise_Scale ("Skin Noise Scale", Float) = 10
_Noise_Rotation ("Noise Rotation", Float) = 45
_UV_Rotation ("UV Rotation", Float) = 0
_Skin_UV_Scale ("Skin UV Scale", Vector) = (1,1,0,0)
_PaintThickness ("PaintThickness", Range(0, 1)) = 0
_PaintSmoothness ("PaintSmoothness", Range(0, 1)) = 0
_MoveSpeed ("MoveSpeed", Float) = 0
_MoveStep ("MoveStep", Float) = 0
_MoveAmplitude ("MoveAmplitude", Float) = 0
_MoveNoiseScale ("MoveNoiseScale", Float) = 0
[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Name "Forward"
            ZWrite On
            Cull Back
            // RenderType: Opaque


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
            TEXTURECUBE(_Texture_t1);
            SAMPLER(sampler__Texture_t1);
            TEXTURECUBE(_Texture_t2);
            SAMPLER(sampler__Texture_t2);
            TEXTURE2D(_Colors);
            SAMPLER(sampler__Colors);
            TEXTURE2D(_Normal_Map);
            SAMPLER(sampler__Normal_Map);
            TEXTURE2D(_Texture_t5);
            SAMPLER(sampler__Texture_t5);
            TEXTURE2D(_Texture_t6);
            SAMPLER(sampler__Texture_t6);
            TEXTURE2D(_Texture_t7);
            SAMPLER(sampler__Texture_t7);
            TEXTURE2D(_Texture_t8);
            SAMPLER(sampler__Texture_t8);
            TEXTURE2D(_Texture_t9);
            SAMPLER(sampler__Texture_t9);

            float4 _pad0;
            float4 _pad16;
            float4 _pad32;
            float4 _pad48;
            float4 _pad64;
            float4 _pad176;
            float4 _pad192;
            float4 _pad208;
            float4 _pad224;
            float4 _pad240;
            float4 _TimeParameters;
            float4 _pad320;
            float4 _pad336;
            float4 _pad352;
            float4 _pad368;
            float4 _pad384;
            float4 _pad400;
            float4 _pad416;
            float4 _pad448;
            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float _MoveSpeed;
            float _MoveAmplitude;
            float _MoveNoiseScale;
            float _MoveStep;
            float4 _GlossyEnvironmentCubeMap_HDR;
            float2 _GlobalMipBias;
            float4 _MainLightPosition;
            float4 _MainLightColor;
            float _MainLightLayerMask;
            float4 _AdditionalLightsCount;
            float3 _WorldSpaceCameraPos;
            float4 unity_OrthoParams;
            float4x4 unity_MatrixV;
            float4 _AdditionalLightsPosition;
            float4 _AdditionalLightsColor;
            float4 _AdditionalLightsAttenuation;
            float4 _AdditionalLightsSpotDir;
            float4 _pad16576;
            float _AdditionalLightsLayerMasks;
            float4 _pad20672;
            float4 unity_RenderingLayer;
            float4 unity_LightData;
            float4 unity_LightIndices;
            float4 unity_SpecCube0_HDR;
            float4 unity_SpecCube1_HDR;
            float4 unity_SpecCube0_BoxMax;
            float4 unity_SpecCube0_BoxMin;
            float4 unity_SpecCube0_ProbePosition;
            float4 unity_SpecCube0_Rotation;
            float4 unity_SpecCube1_BoxMax;
            float4 unity_SpecCube1_BoxMin;
            float4 unity_SpecCube1_ProbePosition;
            float4 unity_SpecCube1_Rotation;
            float4 _MainLightShadowParams;
            float _Emission;
            float _Normal_Strength;
            float _Normal_Scale;
            float4 _CookColor;
            float _Cookness;
            float4 _BurntColor;
            float _Cooked_Normal_Strength;
            float2 _Color_Offset;
            float _Skin_Noise_Scale;
            float2 _Skin_UV_Scale;
            float2 _Color_Offset_2;
            float2 _Skin_Smooth_Step;
            float _pad152;
            float _Use_Skin;
            float _Rainbow_Skin;
            float _pad164;
            float _PaintThickness;
            float _PaintSmoothness;
            float _UV_Rotation;

            float4 u_xlat0;
            float4 u_xlat1;
            int2 u_xlati1;
            uint u_xlatu1;
            float4 u_xlat2;
            int4 u_xlati2;
            uint2 u_xlatu2;
            float4 u_xlat3;
            float u_xlat4;
            float2 u_xlat5;
            int2 u_xlati5;
            uint2 u_xlatu5;
            float3 u_xlat6;
            float2 u_xlat8;
            int2 u_xlati8;
            uint u_xlatu8;
            float2 u_xlat9;
            float u_xlat12;
            int u_xlati12;
            uint u_xlatu12;
            bool u_xlatb12;
            float4 ImmCB_0_0_0[4];
            bool u_xlatb1;
            int u_xlati3;
            uint u_xlatu3;
            bool4 u_xlatb3;
            float4 u_xlat7;
            bool2 u_xlatb7;
            bool u_xlatb9;
            float4 u_xlat10;
            float4 u_xlat11;
            bool3 u_xlatb11;
            float4 u_xlat13;
            bool3 u_xlatb13;
            float4 u_xlat14;
            float4 u_xlat15;
            float4 u_xlat16;
            float4 u_xlat17;
            float4 u_xlat18;
            float4 u_xlat19;
            float4 u_xlat20;
            float3 u_xlat21;
            float3 u_xlat22;
            bool u_xlatb22;
            float3 u_xlat23;
            float3 u_xlat24;
            bool2 u_xlatb24;
            float u_xlat28;
            float3 u_xlat29;
            float3 u_xlat30;
            bool u_xlatb30;
            float3 u_xlat32;
            float3 u_xlat33;
            float2 u_xlat43;
            bool u_xlatb43;
            float2 u_xlat45;
            bool u_xlatb45;
            float2 u_xlat49;
            int u_xlati49;
            uint u_xlatu49;
            bool u_xlatb49;
            float2 u_xlat51;
            bool u_xlatb51;
            float2 u_xlat52;
            float2 u_xlat53;
            bool2 u_xlatb53;
            float2 u_xlat56;
            float2 u_xlat57;
            float u_xlat63;
            int u_xlati63;
            uint u_xlatu63;
            float u_xlat64;
            uint u_xlatu64;
            float u_xlat65;
            int u_xlati65;
            uint u_xlatu65;
            bool u_xlatb65;
            float u_xlat66;
            float u_xlat67;
            float u_xlat68;
            int u_xlati68;
            bool u_xlatb68;
            float u_xlat69;
            bool u_xlatb69;
            float u_xlat70;
            int u_xlati70;
            float u_xlat71;
            bool u_xlatb71;
            float u_xlat72;
            int u_xlati72;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TANGENT0 : TANGENT;
                float4 in_TEXCOORD0 : TEXCOORD0;
                float4 in_TEXCOORD1 : TEXCOORD1;
                float4 in_TEXCOORD2 : TEXCOORD2;
                float4 in_TEXCOORD3 : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 vs_INTERP0 : TEXCOORD0;
                float4 vs_INTERP10 : TEXCOORD1;
                float3 vs_INTERP11 : TEXCOORD2;
                float3 vs_INTERP12 : TEXCOORD3;
                float3 vs_INTERP2 : TEXCOORD4;
                float4 vs_INTERP5 : TEXCOORD5;
                float4 vs_INTERP6 : TEXCOORD6;
                float4 vs_INTERP7 : TEXCOORD7;
                float4 vs_INTERP8 : TEXCOORD8;
                float4 vs_INTERP9 : TEXCOORD9;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0.xy = float2(float2(_MoveSpeed, _MoveSpeed)) * _TimeParameters.xx + input.in_POSITION0.xy;
    u_xlat0.xy = u_xlat0.xy * float2(float2(_MoveNoiseScale, _MoveNoiseScale));
    u_xlat0.zw = floor(u_xlat0.xy);
    u_xlat0.xy = _g_fract(u_xlat0.xy);
    u_xlat1 = u_xlat0.zwxy + float4(1.0, 1.0, -1.0, -1.0);
    u_xlati1.xy = int2(u_xlat1.xy);
    u_xlati5.x = int(uint(uint(u_xlati1.y) ^ 1103515245u));
    u_xlati1.x = u_xlati5.x + u_xlati1.x;
    u_xlatu1 = uint(u_xlati5.x) * uint(u_xlati1.x);
    u_xlatu5.x = uint(u_xlatu1 >> 5u);
    u_xlati1.x = int(uint(u_xlatu5.x ^ u_xlatu1));
    u_xlatu1 = uint(u_xlati1.x) * 668265261u;
    u_xlatu1 = uint(u_xlatu1 >> 8u);
    u_xlat1.x = float(u_xlatu1);
    u_xlat2.yz = u_xlat1.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat5.x = floor(u_xlat2.y);
    u_xlat2.x = u_xlat1.x * 5.96046519e-08 + (-u_xlat5.x);
    u_xlat1.x = dot(u_xlat2.xz, u_xlat2.xz);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    u_xlat1.xy = u_xlat1.xx * u_xlat2.xz;
    u_xlat1.x = dot(u_xlat1.xy, u_xlat1.zw);
    u_xlat2 = u_xlat0.zwzw + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati8.xy = int2(u_xlat0.zw);
    u_xlati2 = int4(u_xlat2);
    u_xlati5.xy = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati2.xy = u_xlati5.xy + u_xlati2.xz;
    u_xlatu5.xy = uint2(u_xlati5.xy) * uint2(u_xlati2.xy);
    u_xlatu2.xy = uint2(u_xlatu5.x >> 5u, u_xlatu5.y >> 5u);
    u_xlati5.xy = int2(uint2(u_xlatu5.x ^ u_xlatu2.x, u_xlatu5.y ^ u_xlatu2.y));
    u_xlatu5.xy = uint2(u_xlati5.xy) * uint2(668265261u, 668265261u);
    u_xlatu5.xy = uint2(u_xlatu5.x >> 8u, u_xlatu5.y >> 8u);
    u_xlat5.xy = float2(u_xlatu5.xy);
    u_xlat2 = u_xlat5.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat3.xy = floor(u_xlat2.xy);
    u_xlat2.xy = u_xlat5.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat3.xy);
    u_xlat5.x = dot(u_xlat2.yw, u_xlat2.yw);
    u_xlat5.x = _g_inversesqrt(u_xlat5.x);
    u_xlat5.xy = u_xlat5.xx * u_xlat2.yw;
    u_xlat3 = u_xlat0.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat5.x = dot(u_xlat5.xy, u_xlat3.zw);
    u_xlat1.x = (-u_xlat5.x) + u_xlat1.x;
    u_xlat9.xy = u_xlat0.xy * u_xlat0.xy;
    u_xlat9.xy = u_xlat0.xy * u_xlat9.xy;
    u_xlat6.xz = u_xlat0.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat6.xz = u_xlat0.xy * u_xlat6.xz + float2(10.0, 10.0);
    u_xlat9.xy = u_xlat9.xy * u_xlat6.xz;
    u_xlat1.x = u_xlat9.y * u_xlat1.x + u_xlat5.x;
    u_xlat5.x = dot(u_xlat2.xz, u_xlat2.xz);
    u_xlat5.x = _g_inversesqrt(u_xlat5.x);
    u_xlat2.xy = u_xlat5.xx * u_xlat2.xz;
    u_xlat5.x = dot(u_xlat2.xy, u_xlat3.xy);
    u_xlati12 = int(uint(uint(u_xlati8.y) ^ 1103515245u));
    u_xlati8.x = u_xlati12 + u_xlati8.x;
    u_xlatu8 = uint(u_xlati12) * uint(u_xlati8.x);
    u_xlatu12 = uint(u_xlatu8 >> 5u);
    u_xlati8.x = int(uint(u_xlatu12 ^ u_xlatu8));
    u_xlatu8 = uint(u_xlati8.x) * 668265261u;
    u_xlatu8 = uint(u_xlatu8 >> 8u);
    u_xlat8.x = float(u_xlatu8);
    u_xlat2.yz = u_xlat8.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat12 = floor(u_xlat2.y);
    u_xlat2.x = u_xlat8.x * 5.96046519e-08 + (-u_xlat12);
    u_xlat8.x = dot(u_xlat2.xz, u_xlat2.xz);
    u_xlat8.x = _g_inversesqrt(u_xlat8.x);
    u_xlat8.xy = u_xlat8.xx * u_xlat2.xz;
    u_xlat0.x = dot(u_xlat8.xy, u_xlat0.xy);
    u_xlat4 = (-u_xlat0.x) + u_xlat5.x;
    u_xlat0.x = u_xlat9.y * u_xlat4 + u_xlat0.x;
    u_xlat4 = (-u_xlat0.x) + u_xlat1.x;
    u_xlat0.x = u_xlat9.x * u_xlat4 + u_xlat0.x;
    u_xlat0.x = u_xlat0.x + 0.5;
    u_xlat0.x = u_xlat0.x * _MoveAmplitude;
    u_xlat0.xyz = u_xlat0.xxx * input.in_POSITION0.xyz;
    u_xlat12 = input.in_POSITION0.z + input.in_POSITION0.y;
    u_xlat12 = (-u_xlat12) + 2.0;
    u_xlatb12 = u_xlat12>=_MoveStep;
    u_xlat12 = u_xlatb12 ? 1.0 : float(0.0);
    u_xlat0.xyz = float3(u_xlat12) * u_xlat0.xyz + input.in_POSITION0.xyz;
    u_xlat1.xyz = u_xlat0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyw = _tunity_ObjectToWorld[0].xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * u_xlat0.zzz + u_xlat0.xyw;
    u_xlat0.xyz = u_xlat0.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.vs_INTERP11.xyz = u_xlat0.xyz;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    output.vs_INTERP2.xyz = float3(0.0, 0.0, 0.0);
    u_xlat0.xyz = input.in_TANGENT0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[0].xyz * input.in_TANGENT0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * input.in_TANGENT0.zzz + u_xlat0.xyz;
    u_xlat12 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat12 = max(u_xlat12, 1.17549435e-38);
    u_xlat12 = _g_inversesqrt(u_xlat12);
    output.vs_INTERP5.xyz = float3(u_xlat12) * u_xlat0.xyz;
    output.vs_INTERP5.w = input.in_TANGENT0.w;
    output.vs_INTERP6 = input.in_TEXCOORD0;
    output.vs_INTERP7 = input.in_TEXCOORD1;
    output.vs_INTERP8 = input.in_TEXCOORD2;
    output.vs_INTERP9 = input.in_TEXCOORD3;
    output.vs_INTERP10 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat12 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat12 = max(u_xlat12, 1.17549435e-38);
    u_xlat12 = _g_inversesqrt(u_xlat12);
    output.vs_INTERP12.xyz = float3(u_xlat12) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_0_0[3] = float4(0.0, 0.0, 0.0, 1.0);
    u_xlat0.x = dot(input.vs_INTERP12.xyz, input.vs_INTERP12.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = float(1.0) / u_xlat0.x;
    u_xlat21.xyz = u_xlat0.xxx * input.vs_INTERP12.xyz;
    u_xlatb1 = input.vs_INTERP7.y>=0.899999976;
    u_xlat1.x = (u_xlatb1) ? 0.0 : 1.0;
    u_xlat22.xy = u_xlat1.xx * float2(_Color_Offset.x, _Color_Offset.y) + input.vs_INTERP6.xy;
    u_xlat2.xyz = _g_texture(_Texture_t8, u_xlat22.xy, _GlobalMipBias.x).xyz;
    u_xlat22.xy = u_xlat22.xy + float2(_Color_Offset_2.x, _Color_Offset_2.y);
    u_xlat64 = _UV_Rotation * 0.0174532924;
    u_xlat3.xy = input.vs_INTERP9.xy + float2(-0.5, -0.5);
    u_xlat4.x = sin(u_xlat64);
    u_xlat5.x = cos(u_xlat64);
    u_xlat6.x = (-u_xlat4.x);
    u_xlat6.y = u_xlat5.x;
    u_xlat5.y = dot(u_xlat3.xy, u_xlat6.xy);
    u_xlat6.z = u_xlat4.x;
    u_xlat5.x = dot(u_xlat3.xy, u_xlat6.yz);
    u_xlat3.xy = u_xlat5.xy + float2(0.5, 0.5);
    u_xlat64 = _pad152 * 0.0174532924;
    u_xlat3.xy = u_xlat3.xy * _Skin_UV_Scale.xy + float2(-0.5, -0.5);
    u_xlat4.x = sin(u_xlat64);
    u_xlat5.x = cos(u_xlat64);
    u_xlat6.x = (-u_xlat4.x);
    u_xlat6.y = u_xlat5.x;
    u_xlat45.y = dot(u_xlat3.xy, u_xlat6.xy);
    u_xlat6.z = u_xlat4.x;
    u_xlat45.x = dot(u_xlat3.xy, u_xlat6.yz);
    u_xlat3.xy = u_xlat45.xy + float2(1.0, 1.0);
    u_xlat45.xy = u_xlat3.xy * float2(float2(_Skin_Noise_Scale, _Skin_Noise_Scale));
    u_xlat3.xy = u_xlat3.xy * float2(float2(_Skin_Noise_Scale, _Skin_Noise_Scale)) + float2(0.25, 0.25);
    u_xlat3.xy = _g_fract(u_xlat3.xy);
    u_xlat3.xy = u_xlat3.xy + float2(-0.5, -0.5);
    u_xlat3.xy = abs(u_xlat3.xy) * float2(4.0, 4.0) + float2(-1.0, -1.0);
    u_xlat4.xy = dFdx(u_xlat45.xy);
    u_xlat4.zw = dFdy(u_xlat45.xy);
    u_xlat64 = dot(u_xlat4.xz, u_xlat4.xz);
    u_xlat65 = dot(u_xlat4.yw, u_xlat4.yw);
    u_xlat4.x = sqrt(u_xlat64);
    u_xlat4.y = sqrt(u_xlat65);
    u_xlat45.xy = float2(0.349999994, 0.349999994) / u_xlat4.xy;
    u_xlat64 = max(u_xlat4.y, u_xlat4.x);
    u_xlat64 = (-u_xlat64) + 1.10000002;
    u_xlat64 = clamp(u_xlat64, 0.0, 1.0);
    u_xlat64 = sqrt(u_xlat64);
    u_xlat3.xy = u_xlat45.xy * u_xlat3.xy;
    u_xlat3.xy = max(u_xlat3.xy, float2(-1.0, -1.0));
    u_xlat3.xy = min(u_xlat3.xy, float2(1.0, 1.0));
    u_xlat65 = u_xlat3.x * u_xlat3.y;
    u_xlat64 = u_xlat64 * u_xlat65;
    u_xlat64 = u_xlat64 * 0.5 + 0.5;
    u_xlat64 = (-u_xlat64) + 1.0;
    u_xlat65 = (-_Skin_Smooth_Step.x) + _Skin_Smooth_Step.y;
    u_xlat64 = u_xlat64 + (-_Skin_Smooth_Step.x);
    u_xlat65 = float(1.0) / u_xlat65;
    u_xlat64 = u_xlat64 * u_xlat65;
    u_xlat64 = clamp(u_xlat64, 0.0, 1.0);
    u_xlat65 = u_xlat64 * -2.0 + 3.0;
    u_xlat64 = u_xlat64 * u_xlat64;
    u_xlat43.y = u_xlat64 * u_xlat65;
    u_xlatb65 = float4(0.0, 0.0, 0.0, 0.0)!=float4(_Rainbow_Skin);
    u_xlat65 = (u_xlatb65) ? u_xlat43.y : 1.0;
    u_xlat22.xy = u_xlat22.xy * float2(u_xlat65);
    u_xlat3.xyz = _g_texture(_Texture_t8, u_xlat22.xy, _GlobalMipBias.x).xyz;
    u_xlatb22 = float4(0.0, 0.0, 0.0, 0.0)!=float4(_Use_Skin);
    u_xlat1.y = u_xlatb22 ? 1.0 : float(0.0);
    u_xlatb43 = input.vs_INTERP7.x>=_pad164;
    u_xlat43.x = u_xlatb43 ? 1.0 : float(0.0);
    u_xlat22.xy = u_xlat1.yx * u_xlat43.yx;
    u_xlat22.x = u_xlat22.y * u_xlat22.x;
    u_xlat3.xyz = (-u_xlat2.xyz) + u_xlat3.xyz;
    u_xlat22.xyz = u_xlat22.xxx * u_xlat3.xyz + u_xlat2.xyz;
    u_xlat2.x = _Cookness;
    u_xlat2.x = clamp(u_xlat2.x, 0.0, 1.0);
    u_xlat23.xyz = (-u_xlat22.xyz) + _CookColor.xyz;
    u_xlat22.xyz = u_xlat2.xxx * u_xlat23.xyz + u_xlat22.xyz;
    u_xlat23.x = _Cookness + -1.0;
    u_xlat23.x = clamp(u_xlat23.x, 0.0, 1.0);
    u_xlat3.xyz = (-u_xlat22.xyz) + _BurntColor.xyz;
    u_xlat22.xyz = u_xlat23.xxx * u_xlat3.xyz + u_xlat22.xyz;
    u_xlat23.xy = input.vs_INTERP9.xy * float2(float2(_Normal_Scale, _Normal_Scale));
    u_xlat3.xyw = _g_texture(_Texture_t9, u_xlat23.xy, _GlobalMipBias.x).xyw;
    u_xlat3.x = u_xlat3.x * u_xlat3.w;
    u_xlat3.xy = u_xlat3.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat23.x = dot(u_xlat3.xy, u_xlat3.xy);
    u_xlat23.x = min(u_xlat23.x, 1.0);
    u_xlat23.x = (-u_xlat23.x) + 1.0;
    u_xlat23.x = sqrt(u_xlat23.x);
    u_xlat3.z = max(u_xlat23.x, 1.00000002e-16);
    u_xlat23.x = dot(u_xlat3, u_xlat3);
    u_xlat23.x = _g_inversesqrt(u_xlat23.x);
    u_xlat23.xyz = u_xlat23.xxx * u_xlat3.xyz;
    u_xlat3.xy = u_xlat0.xx * input.vs_INTERP12.xy + u_xlat23.xy;
    u_xlat3.z = u_xlat21.z * u_xlat23.z;
    u_xlat0.x = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat23.x = (-_Normal_Strength) + _Cooked_Normal_Strength;
    u_xlat2.x = u_xlat2.x * u_xlat23.x + _Normal_Strength;
    u_xlat23.xyz = u_xlat3.xyz * u_xlat0.xxx + (-u_xlat21.xyz);
    u_xlat0.xyz = u_xlat2.xxx * u_xlat23.xyz + u_xlat21.xyz;
    u_xlat63 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat63 = _g_inversesqrt(u_xlat63);
    u_xlat0.xyz = float3(u_xlat63) * u_xlat0.xyz;
    u_xlat2.xyz = _g_texture(_Texture_t8, input.vs_INTERP8.xy, _GlobalMipBias.x).xyz;
    u_xlat63 = u_xlat1.x * _PaintThickness;
    u_xlat1.x = (-u_xlat1.x) * _PaintThickness + input.vs_INTERP7.x;
    u_xlat1.x = clamp(u_xlat1.x, 0.0, 1.0);
    u_xlat65 = (-_Cookness) + 1.0;
    u_xlat65 = max(u_xlat65, 0.200000003);
    u_xlat65 = min(u_xlat65, 1.0);
    u_xlat3.x = max(input.vs_INTERP7.y, 0.0);
    u_xlat3.x = min(u_xlat3.x, 0.699999988);
    u_xlat3.x = u_xlat3.x + (-_Cookness);
    u_xlat3.x = max(u_xlat3.x, 0.0);
    u_xlat3.x = min(u_xlat3.x, 0.699999988);
    u_xlat24.x = u_xlat65 * u_xlat3.x;
    u_xlat65 = (-u_xlat65) * u_xlat3.x + _PaintSmoothness;
    u_xlat63 = u_xlat63 * u_xlat65 + u_xlat24.x;
    u_xlat63 = clamp(u_xlat63, 0.0, 1.0);
    u_xlatb65 = unity_OrthoParams.w==0.0;
    u_xlat3.xyz = (-input.vs_INTERP11.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat66 = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat66 = _g_inversesqrt(u_xlat66);
    u_xlat3.xyz = float3(u_xlat66) * u_xlat3.xyz;
    u_xlat4.x = (u_xlatb65) ? u_xlat3.x : _tunity_MatrixV[0].z;
    u_xlat4.y = (u_xlatb65) ? u_xlat3.y : _tunity_MatrixV[1].z;
    u_xlat4.z = (u_xlatb65) ? u_xlat3.z : _tunity_MatrixV[2].z;
    u_xlat3.xyz = input.vs_INTERP11.xyz + (-_pad320.xyz);
    u_xlat5.xyz = input.vs_INTERP11.xyz + (-_pad336.xyz);
    u_xlat6.xyz = input.vs_INTERP11.xyz + (-_pad352.xyz);
    u_xlat7.xyz = input.vs_INTERP11.xyz + (-_pad368.xyz);
    u_xlat3.x = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat3.y = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlat3.z = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.w = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlatb3 = _g_lessThan(u_xlat3, _pad384);
    u_xlat5.x = u_xlatb3.x ? float(1.0) : 0.0;
    u_xlat5.y = u_xlatb3.y ? float(1.0) : 0.0;
    u_xlat5.z = u_xlatb3.z ? float(1.0) : 0.0;
    u_xlat5.w = u_xlatb3.w ? float(1.0) : 0.0;
;
    u_xlat3.x = (u_xlatb3.x) ? float(-1.0) : float(-0.0);
    u_xlat3.y = (u_xlatb3.y) ? float(-1.0) : float(-0.0);
    u_xlat3.z = (u_xlatb3.z) ? float(-1.0) : float(-0.0);
    u_xlat3.xyz = u_xlat3.xyz + u_xlat5.yzw;
    u_xlat5.yzw = max(u_xlat3.xyz, float3(0.0, 0.0, 0.0));
    u_xlat65 = dot(u_xlat5, float4(4.0, 3.0, 2.0, 1.0));
    u_xlat65 = (-u_xlat65) + 4.0;
    u_xlatu65 = uint(u_xlat65);
    u_xlati65 = int(int(u_xlatu65) << 2);
    u_xlat3.xyz = input.vs_INTERP11.yyy * _pad16.xyz;
    u_xlat3.xyz = _pad0.xyz * input.vs_INTERP11.xxx + u_xlat3.xyz;
    u_xlat3.xyz = _pad32.xyz * input.vs_INTERP11.zzz + u_xlat3.xyz;
    u_xlat3.xyz = u_xlat3.xyz + _pad48.xyz;
    u_xlat5.xyz = _g_texture(_Colors, input.vs_INTERP0.xy, _GlobalMipBias.x).xyz;
    u_xlat6 = _g_texture(_Normal_Map, input.vs_INTERP0.xy, _GlobalMipBias.x);
    u_xlat6.xyz = u_xlat6.xyz + float3(-0.5, -0.5, -0.5);
    u_xlat65 = dot(u_xlat0.xyz, u_xlat6.xyz);
    u_xlat65 = u_xlat65 + 0.5;
    u_xlat5.xyz = float3(u_xlat65) * u_xlat5.xyz;
    u_xlat65 = max(u_xlat6.w, 9.99999975e-05);
    u_xlat5.xyz = u_xlat5.xyz / float3(u_xlat65);
    u_xlat65 = (-u_xlat1.x) * 0.959999979 + 0.959999979;
    u_xlat66 = u_xlat63 + (-u_xlat65);
    u_xlat6.xyz = u_xlat22.xyz * float3(u_xlat65);
    u_xlat22.xyz = u_xlat22.xyz + float3(-0.0399999991, -0.0399999991, -0.0399999991);
    u_xlat1.xyz = u_xlat1.xxx * u_xlat22.xyz + float3(0.0399999991, 0.0399999991, 0.0399999991);
    u_xlat63 = (-u_xlat63) + 1.0;
    u_xlat64 = u_xlat63 * u_xlat63;
    u_xlat64 = max(u_xlat64, 0.0078125);
    u_xlat65 = u_xlat64 * u_xlat64;
    u_xlat66 = u_xlat66 + 1.0;
    u_xlat66 = min(u_xlat66, 1.0);
    u_xlat67 = u_xlat64 * 4.0 + 2.0;
    u_xlatb68 = 0.0<_MainLightShadowParams.y;
    if(u_xlatb68){
        u_xlatb68 = _MainLightShadowParams.y==1.0;
        if(u_xlatb68){
            u_xlat7 = u_xlat3.xyxy + _pad400;
            float3 txVec0 = float3(u_xlat7.xy,u_xlat3.z);
            u_xlat8.x = _g_textureLod(_Texture_t5, txVec0, 0.0);
            float3 txVec1 = float3(u_xlat7.zw,u_xlat3.z);
            u_xlat8.y = _g_textureLod(_Texture_t5, txVec1, 0.0);
            u_xlat7 = u_xlat3.xyxy + _pad416;
            float3 txVec2 = float3(u_xlat7.xy,u_xlat3.z);
            u_xlat8.z = _g_textureLod(_Texture_t5, txVec2, 0.0);
            float3 txVec3 = float3(u_xlat7.zw,u_xlat3.z);
            u_xlat8.w = _g_textureLod(_Texture_t5, txVec3, 0.0);
            u_xlat68 = dot(u_xlat8, float4(0.25, 0.25, 0.25, 0.25));
        } else {
            u_xlatb69 = _MainLightShadowParams.y==2.0;
            if(u_xlatb69){
                u_xlat7.xy = u_xlat3.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat7.xy = floor(u_xlat7.xy);
                u_xlat49.xy = u_xlat3.xy * _pad448.zw + (-u_xlat7.xy);
                u_xlat8 = u_xlat49.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat9 = u_xlat8.xxzz * u_xlat8.xxzz;
                u_xlat8.xz = u_xlat9.yw * float2(0.0799999982, 0.0799999982);
                u_xlat9.xy = u_xlat9.xz * float2(0.5, 0.5) + (-u_xlat49.xy);
                u_xlat51.xy = (-u_xlat49.xy) + float2(1.0, 1.0);
                u_xlat10.xy = min(u_xlat49.xy, float2(0.0, 0.0));
                u_xlat10.xy = (-u_xlat10.xy) * u_xlat10.xy + u_xlat51.xy;
                u_xlat49.xy = max(u_xlat49.xy, float2(0.0, 0.0));
                u_xlat49.xy = (-u_xlat49.xy) * u_xlat49.xy + u_xlat8.yw;
                u_xlat10.xy = u_xlat10.xy + float2(1.0, 1.0);
                u_xlat49.xy = u_xlat49.xy + float2(1.0, 1.0);
                u_xlat11.xy = u_xlat9.xy * float2(0.159999996, 0.159999996);
                u_xlat9.xy = u_xlat51.xy * float2(0.159999996, 0.159999996);
                u_xlat10.xy = u_xlat10.xy * float2(0.159999996, 0.159999996);
                u_xlat12.xy = u_xlat49.xy * float2(0.159999996, 0.159999996);
                u_xlat49.xy = u_xlat8.yw * float2(0.159999996, 0.159999996);
                u_xlat11.z = u_xlat10.x;
                u_xlat11.w = u_xlat49.x;
                u_xlat9.z = u_xlat12.x;
                u_xlat9.w = u_xlat8.x;
                u_xlat13 = u_xlat9.zwxz + u_xlat11.zwxz;
                u_xlat10.z = u_xlat11.y;
                u_xlat10.w = u_xlat49.y;
                u_xlat12.z = u_xlat9.y;
                u_xlat12.w = u_xlat8.z;
                u_xlat8.xyz = u_xlat10.zyw + u_xlat12.zyw;
                u_xlat9.xyz = u_xlat9.xzw / u_xlat13.zwy;
                u_xlat9.xyz = u_xlat9.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat10.xyz = u_xlat12.zyw / u_xlat8.xyz;
                u_xlat10.xyz = u_xlat10.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat9.xyz = u_xlat9.yxz * _pad448.xxx;
                u_xlat10.xyz = u_xlat10.xyz * _pad448.yyy;
                u_xlat9.w = u_xlat10.x;
                u_xlat11 = u_xlat7.xyxy * _pad448.xyxy + u_xlat9.ywxw;
                u_xlat49.xy = u_xlat7.xy * _pad448.xy + u_xlat9.zw;
                u_xlat10.w = u_xlat9.y;
                u_xlat9.yw = u_xlat10.yz;
                u_xlat12 = u_xlat7.xyxy * _pad448.xyxy + u_xlat9.xyzy;
                u_xlat10 = u_xlat7.xyxy * _pad448.xyxy + u_xlat10.wywz;
                u_xlat9 = u_xlat7.xyxy * _pad448.xyxy + u_xlat9.xwzw;
                u_xlat14 = u_xlat8.xxxy * u_xlat13.zwyz;
                u_xlat15 = u_xlat8.yyzz * u_xlat13;
                u_xlat69 = u_xlat8.z * u_xlat13.y;
                float3 txVec4 = float3(u_xlat11.xy,u_xlat3.z);
                u_xlat7.x = _g_textureLod(_Texture_t5, txVec4, 0.0);
                float3 txVec5 = float3(u_xlat11.zw,u_xlat3.z);
                u_xlat28 = _g_textureLod(_Texture_t5, txVec5, 0.0);
                u_xlat28 = u_xlat28 * u_xlat14.y;
                u_xlat7.x = u_xlat14.x * u_xlat7.x + u_xlat28;
                float3 txVec6 = float3(u_xlat49.xy,u_xlat3.z);
                u_xlat28 = _g_textureLod(_Texture_t5, txVec6, 0.0);
                u_xlat7.x = u_xlat14.z * u_xlat28 + u_xlat7.x;
                float3 txVec7 = float3(u_xlat10.xy,u_xlat3.z);
                u_xlat28 = _g_textureLod(_Texture_t5, txVec7, 0.0);
                u_xlat7.x = u_xlat14.w * u_xlat28 + u_xlat7.x;
                float3 txVec8 = float3(u_xlat12.xy,u_xlat3.z);
                u_xlat28 = _g_textureLod(_Texture_t5, txVec8, 0.0);
                u_xlat7.x = u_xlat15.x * u_xlat28 + u_xlat7.x;
                float3 txVec9 = float3(u_xlat12.zw,u_xlat3.z);
                u_xlat28 = _g_textureLod(_Texture_t5, txVec9, 0.0);
                u_xlat7.x = u_xlat15.y * u_xlat28 + u_xlat7.x;
                float3 txVec10 = float3(u_xlat10.zw,u_xlat3.z);
                u_xlat28 = _g_textureLod(_Texture_t5, txVec10, 0.0);
                u_xlat7.x = u_xlat15.z * u_xlat28 + u_xlat7.x;
                float3 txVec11 = float3(u_xlat9.xy,u_xlat3.z);
                u_xlat28 = _g_textureLod(_Texture_t5, txVec11, 0.0);
                u_xlat7.x = u_xlat15.w * u_xlat28 + u_xlat7.x;
                float3 txVec12 = float3(u_xlat9.zw,u_xlat3.z);
                u_xlat28 = _g_textureLod(_Texture_t5, txVec12, 0.0);
                u_xlat68 = u_xlat69 * u_xlat28 + u_xlat7.x;
            } else {
                u_xlat7.xy = u_xlat3.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat7.xy = floor(u_xlat7.xy);
                u_xlat49.xy = u_xlat3.xy * _pad448.zw + (-u_xlat7.xy);
                u_xlat8 = u_xlat49.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat9 = u_xlat8.xxzz * u_xlat8.xxzz;
                u_xlat10.yw = u_xlat9.yw * float2(0.0408160016, 0.0408160016);
                u_xlat8.xz = u_xlat9.xz * float2(0.5, 0.5) + (-u_xlat49.xy);
                u_xlat9.xy = (-u_xlat49.xy) + float2(1.0, 1.0);
                u_xlat51.xy = min(u_xlat49.xy, float2(0.0, 0.0));
                u_xlat9.xy = (-u_xlat51.xy) * u_xlat51.xy + u_xlat9.xy;
                u_xlat51.xy = max(u_xlat49.xy, float2(0.0, 0.0));
                u_xlat29.xz = (-u_xlat51.xy) * u_xlat51.xy + u_xlat8.yw;
                u_xlat9.xy = u_xlat9.xy + float2(2.0, 2.0);
                u_xlat8.yw = u_xlat29.xz + float2(2.0, 2.0);
                u_xlat11.z = u_xlat8.y * 0.0816320032;
                u_xlat12.xyz = u_xlat8.zxw * float3(0.0816320032, 0.0816320032, 0.0816320032);
                u_xlat8.xy = u_xlat9.xy * float2(0.0816320032, 0.0816320032);
                u_xlat11.x = u_xlat12.y;
                u_xlat11.yw = u_xlat49.xx * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat9.xz = u_xlat49.xx * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat9.y = u_xlat8.x;
                u_xlat9.w = u_xlat10.y;
                u_xlat11 = u_xlat9 + u_xlat11;
                u_xlat12.yw = u_xlat49.yy * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat10.xz = u_xlat49.yy * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat10.y = u_xlat8.y;
                u_xlat8 = u_xlat10 + u_xlat12;
                u_xlat9 = u_xlat9 / u_xlat11;
                u_xlat9 = u_xlat9 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat10 = u_xlat10 / u_xlat8;
                u_xlat10 = u_xlat10 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat9 = u_xlat9.wxyz * _pad448.xxxx;
                u_xlat10 = u_xlat10.xwyz * _pad448.yyyy;
                u_xlat12.xzw = u_xlat9.yzw;
                u_xlat12.y = u_xlat10.x;
                u_xlat13 = u_xlat7.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat49.xy = u_xlat7.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.y = u_xlat12.y;
                u_xlat12.y = u_xlat10.z;
                u_xlat14 = u_xlat7.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat15.xy = u_xlat7.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.z = u_xlat12.y;
                u_xlat16 = u_xlat7.xyxy * _pad448.xyxy + u_xlat9.xyxz;
                u_xlat12.y = u_xlat10.w;
                u_xlat17 = u_xlat7.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat30.xy = u_xlat7.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.w = u_xlat12.y;
                u_xlat57.xy = u_xlat7.xy * _pad448.xy + u_xlat9.xw;
                u_xlat10.xzw = u_xlat12.xzw;
                u_xlat12 = u_xlat7.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat52.xy = u_xlat7.xy * _pad448.xy + u_xlat10.wy;
                u_xlat10.x = u_xlat9.x;
                u_xlat7.xy = u_xlat7.xy * _pad448.xy + u_xlat10.xy;
                u_xlat18 = u_xlat8.xxxx * u_xlat11;
                u_xlat19 = u_xlat8.yyyy * u_xlat11;
                u_xlat20 = u_xlat8.zzzz * u_xlat11;
                u_xlat8 = u_xlat8.wwww * u_xlat11;
                float3 txVec13 = float3(u_xlat13.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec13, 0.0);
                float3 txVec14 = float3(u_xlat13.zw,u_xlat3.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec14, 0.0);
                u_xlat9.x = u_xlat9.x * u_xlat18.y;
                u_xlat69 = u_xlat18.x * u_xlat69 + u_xlat9.x;
                float3 txVec15 = float3(u_xlat49.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec15, 0.0);
                u_xlat69 = u_xlat18.z * u_xlat49.x + u_xlat69;
                float3 txVec16 = float3(u_xlat16.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec16, 0.0);
                u_xlat69 = u_xlat18.w * u_xlat49.x + u_xlat69;
                float3 txVec17 = float3(u_xlat14.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec17, 0.0);
                u_xlat69 = u_xlat19.x * u_xlat49.x + u_xlat69;
                float3 txVec18 = float3(u_xlat14.zw,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec18, 0.0);
                u_xlat69 = u_xlat19.y * u_xlat49.x + u_xlat69;
                float3 txVec19 = float3(u_xlat15.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec19, 0.0);
                u_xlat69 = u_xlat19.z * u_xlat49.x + u_xlat69;
                float3 txVec20 = float3(u_xlat16.zw,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec20, 0.0);
                u_xlat69 = u_xlat19.w * u_xlat49.x + u_xlat69;
                float3 txVec21 = float3(u_xlat17.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec21, 0.0);
                u_xlat69 = u_xlat20.x * u_xlat49.x + u_xlat69;
                float3 txVec22 = float3(u_xlat17.zw,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec22, 0.0);
                u_xlat69 = u_xlat20.y * u_xlat49.x + u_xlat69;
                float3 txVec23 = float3(u_xlat30.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec23, 0.0);
                u_xlat69 = u_xlat20.z * u_xlat49.x + u_xlat69;
                float3 txVec24 = float3(u_xlat57.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec24, 0.0);
                u_xlat69 = u_xlat20.w * u_xlat49.x + u_xlat69;
                float3 txVec25 = float3(u_xlat12.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec25, 0.0);
                u_xlat69 = u_xlat8.x * u_xlat49.x + u_xlat69;
                float3 txVec26 = float3(u_xlat12.zw,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec26, 0.0);
                u_xlat69 = u_xlat8.y * u_xlat49.x + u_xlat69;
                float3 txVec27 = float3(u_xlat52.xy,u_xlat3.z);
                u_xlat49.x = _g_textureLod(_Texture_t5, txVec27, 0.0);
                u_xlat69 = u_xlat8.z * u_xlat49.x + u_xlat69;
                float3 txVec28 = float3(u_xlat7.xy,u_xlat3.z);
                u_xlat7.x = _g_textureLod(_Texture_t5, txVec28, 0.0);
                u_xlat68 = u_xlat8.w * u_xlat7.x + u_xlat69;
            }
        }
    } else {
        float3 txVec29 = float3(u_xlat3.xy,u_xlat3.z);
        u_xlat68 = _g_textureLod(_Texture_t5, txVec29, 0.0);
    }
    u_xlat3.x = (-_MainLightShadowParams.x) + 1.0;
    u_xlat3.x = u_xlat68 * _MainLightShadowParams.x + u_xlat3.x;
    u_xlatb24.x = 0.0>=u_xlat3.z;
    u_xlatb45 = u_xlat3.z>=1.0;
    u_xlatb24.x = u_xlatb45 || u_xlatb24.x;
    u_xlat3.x = (u_xlatb24.x) ? 1.0 : u_xlat3.x;
    u_xlat7.xyz = input.vs_INTERP11.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat24.x = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat24.x = u_xlat24.x * _MainLightShadowParams.z + _MainLightShadowParams.w;
    u_xlat24.x = clamp(u_xlat24.x, 0.0, 1.0);
    u_xlat45.x = (-u_xlat3.x) + 1.0;
    u_xlat3.x = u_xlat24.x * u_xlat45.x + u_xlat3.x;
    u_xlatb24.x = _pad176.y!=-1.0;
    if(u_xlatb24.x){
        u_xlat24.xy = input.vs_INTERP11.yy * _pad16.xy;
        u_xlat24.xy = _pad0.xy * input.vs_INTERP11.xx + u_xlat24.xy;
        u_xlat24.xy = _pad32.xy * input.vs_INTERP11.zz + u_xlat24.xy;
        u_xlat24.xy = u_xlat24.xy + _pad48.xy;
        u_xlat24.xy = u_xlat24.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
        u_xlat7 = _g_texture(_Texture_t6, u_xlat24.xy, _GlobalMipBias.x);
        u_xlatb24.xy = _g_equal(_pad176.yyyy, float4(0.0, 1.0, 0.0, 0.0)).xy;
        u_xlat45.x = (u_xlatb24.y) ? u_xlat7.w : u_xlat7.x;
        u_xlat7.xyz = (u_xlatb24.x) ? u_xlat7.xyz : u_xlat45.xxx;
    } else {
        u_xlat7.x = float(1.0);
        u_xlat7.y = float(1.0);
        u_xlat7.z = float(1.0);
    }
    u_xlat7.xyz = u_xlat7.xyz * _MainLightColor.xyz;
    u_xlat24.x = dot((-u_xlat4.xyz), u_xlat0.xyz);
    u_xlat24.x = u_xlat24.x + u_xlat24.x;
    u_xlat8.xyz = u_xlat0.xyz * (-u_xlat24.xxx) + (-u_xlat4.xyz);
    u_xlat24.x = dot(u_xlat0.xyz, u_xlat4.xyz);
    u_xlat24.x = clamp(u_xlat24.x, 0.0, 1.0);
    u_xlat24.x = (-u_xlat24.x) + 1.0;
    u_xlat24.x = u_xlat24.x * u_xlat24.x;
    u_xlat24.x = u_xlat24.x * u_xlat24.x;
    u_xlat45.x = (-u_xlat63) * 0.699999988 + 1.70000005;
    u_xlat63 = u_xlat63 * u_xlat45.x;
    u_xlat63 = u_xlat63 * 6.0;
    u_xlat9.xyz = unity_SpecCube0_BoxMax.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat10.xyz = u_xlat9.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube0_BoxMin.xyz;
    u_xlat11.xyz = (-u_xlat10.xyz) + input.vs_INTERP11.xyz;
    u_xlat12 = unity_SpecCube0_Rotation.zzxy + unity_SpecCube0_Rotation.zzxy;
    u_xlat13.xyz = u_xlat12.zwy * unity_SpecCube0_Rotation.xyz;
    u_xlat14 = u_xlat12 * unity_SpecCube0_Rotation.xyww;
    u_xlat45.x = u_xlat12.y * unity_SpecCube0_Rotation.w;
    u_xlat13.xyz = u_xlat13.zzy + u_xlat13.yxx;
    u_xlat13.xyz = (-u_xlat13.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat15.xy = u_xlat11.xy * u_xlat13.xy;
    u_xlat68 = unity_SpecCube0_Rotation.x * u_xlat12.w + (-u_xlat45.x);
    u_xlat69 = u_xlat68 * u_xlat11.y + u_xlat15.x;
    u_xlat14.xy = u_xlat14.wz + u_xlat14.xy;
    u_xlat70 = u_xlat11.y * u_xlat14.y;
    u_xlat16.x = u_xlat14.x * u_xlat11.z + u_xlat69;
    u_xlat45.x = unity_SpecCube0_Rotation.x * u_xlat12.w + u_xlat45.x;
    u_xlat69 = u_xlat45.x * u_xlat11.x + u_xlat15.y;
    u_xlat32.xz = unity_SpecCube0_Rotation.yx * u_xlat12.yx + (-u_xlat14.zw);
    u_xlat16.y = u_xlat32.x * u_xlat11.z + u_xlat69;
    u_xlat69 = u_xlat32.z * u_xlat11.x + u_xlat70;
    u_xlat16.z = u_xlat13.z * u_xlat11.z + u_xlat69;
    u_xlat10.xyz = u_xlat10.xyz + u_xlat16.xyz;
    u_xlat12.xyz = unity_SpecCube1_BoxMax.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat15.xyz = u_xlat12.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube1_BoxMin.xyz;
    u_xlat16.xyz = (-u_xlat15.xyz) + input.vs_INTERP11.xyz;
    u_xlat17 = unity_SpecCube1_Rotation.zzxy + unity_SpecCube1_Rotation.zzxy;
    u_xlat18.xyz = u_xlat17.zwy * unity_SpecCube1_Rotation.xyz;
    u_xlat19 = u_xlat17 * unity_SpecCube1_Rotation.xyww;
    u_xlat69 = u_xlat17.y * unity_SpecCube1_Rotation.w;
    u_xlat18.xyz = u_xlat18.zzy + u_xlat18.yxx;
    u_xlat18.xyz = (-u_xlat18.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat11.xz = u_xlat16.xy * u_xlat18.xy;
    u_xlat70 = unity_SpecCube1_Rotation.x * u_xlat17.w + (-u_xlat69);
    u_xlat71 = u_xlat70 * u_xlat16.y + u_xlat11.x;
    u_xlat56.xy = u_xlat19.wz + u_xlat19.xy;
    u_xlat72 = u_xlat16.y * u_xlat56.y;
    u_xlat20.x = u_xlat56.x * u_xlat16.z + u_xlat71;
    u_xlat69 = unity_SpecCube1_Rotation.x * u_xlat17.w + u_xlat69;
    u_xlat71 = u_xlat69 * u_xlat16.x + u_xlat11.z;
    u_xlat11.xz = unity_SpecCube1_Rotation.yx * u_xlat17.yx + (-u_xlat19.zw);
    u_xlat20.y = u_xlat11.x * u_xlat16.z + u_xlat71;
    u_xlat71 = u_xlat11.z * u_xlat16.x + u_xlat72;
    u_xlat20.z = u_xlat18.z * u_xlat16.z + u_xlat71;
    u_xlat15.xyz = u_xlat15.xyz + u_xlat20.xyz;
    u_xlat71 = dot(u_xlat9.xyz, u_xlat9.xyz);
    u_xlat9.x = dot(u_xlat12.xyz, u_xlat12.xyz);
    u_xlat71 = u_xlat71 + (-u_xlat9.x);
    u_xlatb9 = 0.0<unity_SpecCube1_BoxMin.w;
    u_xlatb30 = unity_SpecCube1_BoxMin.w==0.0;
    u_xlatb51 = u_xlat71<-9.99999975e-05;
    u_xlatb51 = u_xlatb51 && u_xlatb30;
    u_xlatb9 = u_xlatb51 || u_xlatb9;
    u_xlatb51 = unity_SpecCube1_BoxMin.w<0.0;
    u_xlatb71 = 9.99999975e-05<u_xlat71;
    u_xlatb71 = u_xlatb71 && u_xlatb30;
    u_xlatb71 = u_xlatb71 || u_xlatb51;
    u_xlat30.xyz = u_xlat10.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat12.xyz = (-u_xlat10.xyz) + unity_SpecCube0_BoxMax.xyz;
    u_xlat30.xyz = min(u_xlat30.xyz, u_xlat12.xyz);
    u_xlat30.xyz = u_xlat30.xyz / unity_SpecCube0_BoxMax.www;
    u_xlat51.x = min(u_xlat30.z, u_xlat30.y);
    u_xlat30.x = min(u_xlat51.x, u_xlat30.x);
    u_xlat30.x = clamp(u_xlat30.x, 0.0, 1.0);
    u_xlat12.xyz = u_xlat15.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat16.xyz = (-u_xlat15.xyz) + unity_SpecCube1_BoxMax.xyz;
    u_xlat12.xyz = min(u_xlat12.xyz, u_xlat16.xyz);
    u_xlat12.xyz = u_xlat12.xyz / unity_SpecCube1_BoxMax.www;
    u_xlat51.x = min(u_xlat12.z, u_xlat12.y);
    u_xlat51.x = min(u_xlat51.x, u_xlat12.x);
    u_xlat51.x = clamp(u_xlat51.x, 0.0, 1.0);
    u_xlat72 = (-u_xlat51.x) + 1.0;
    u_xlat72 = min(u_xlat72, u_xlat30.x);
    u_xlat71 = (u_xlatb71) ? u_xlat72 : u_xlat30.x;
    u_xlat30.x = (-u_xlat30.x) + 1.0;
    u_xlat30.x = min(u_xlat30.x, u_xlat51.x);
    u_xlat9.x = (u_xlatb9) ? u_xlat30.x : u_xlat51.x;
    u_xlat30.x = u_xlat71 + u_xlat9.x;
    u_xlat51.x = max(u_xlat30.x, 1.0);
    u_xlat71 = u_xlat71 / u_xlat51.x;
    u_xlat9.x = u_xlat9.x / u_xlat51.x;
    u_xlatb51 = 0.00999999978<u_xlat71;
    if(u_xlatb51){
        u_xlat51.xy = u_xlat8.xy * u_xlat13.xy;
        u_xlat68 = u_xlat68 * u_xlat8.y + u_xlat51.x;
        u_xlat51.x = u_xlat8.y * u_xlat14.y;
        u_xlat12.x = u_xlat14.x * u_xlat8.z + u_xlat68;
        u_xlat45.x = u_xlat45.x * u_xlat8.x + u_xlat51.y;
        u_xlat12.y = u_xlat32.x * u_xlat8.z + u_xlat45.x;
        u_xlat45.x = u_xlat32.z * u_xlat8.x + u_xlat51.x;
        u_xlat12.z = u_xlat13.z * u_xlat8.z + u_xlat45.x;
        u_xlatb45 = 0.0<unity_SpecCube0_ProbePosition.w;
        u_xlatb13.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat12.xyzx).xyz;
        u_xlat13.x = (u_xlatb13.x) ? unity_SpecCube0_BoxMax.x : unity_SpecCube0_BoxMin.x;
        u_xlat13.y = (u_xlatb13.y) ? unity_SpecCube0_BoxMax.y : unity_SpecCube0_BoxMin.y;
        u_xlat13.z = (u_xlatb13.z) ? unity_SpecCube0_BoxMax.z : unity_SpecCube0_BoxMin.z;
        u_xlat13.xyz = (-u_xlat10.xyz) + u_xlat13.xyz;
        u_xlat13.xyz = u_xlat13.xyz / u_xlat12.xyz;
        u_xlat68 = min(u_xlat13.y, u_xlat13.x);
        u_xlat68 = min(u_xlat13.z, u_xlat68);
        u_xlat10.xyz = u_xlat10.xyz + (-unity_SpecCube0_ProbePosition.xyz);
        u_xlat10.xyz = u_xlat12.xyz * float3(u_xlat68) + u_xlat10.xyz;
        u_xlat10.xyz = (bool(u_xlatb45)) ? u_xlat10.xyz : u_xlat12.xyz;
        u_xlat12.xyz = unity_SpecCube0_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat13.xyz = u_xlat12.yzx * (-unity_SpecCube0_Rotation.xyz);
        u_xlat16.xyz = u_xlat12.xyz * unity_SpecCube0_Rotation.www;
        u_xlat13.xyz = u_xlat13.zzy + u_xlat13.yxx;
        u_xlat13.xyz = (-u_xlat13.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat51.xy = u_xlat10.xy * u_xlat13.xy;
        u_xlat45.x = (-unity_SpecCube0_Rotation.x) * u_xlat12.z + (-u_xlat16.x);
        u_xlat45.x = u_xlat45.x * u_xlat10.y + u_xlat51.x;
        u_xlat32.xz = (-unity_SpecCube0_Rotation.xy) * u_xlat12.xx + u_xlat16.zy;
        u_xlat68 = u_xlat10.y * u_xlat32.z;
        u_xlat17.x = u_xlat32.x * u_xlat10.z + u_xlat45.x;
        u_xlat45.x = (-unity_SpecCube0_Rotation.x) * u_xlat12.z + u_xlat16.x;
        u_xlat45.x = u_xlat45.x * u_xlat10.x + u_xlat51.y;
        u_xlat51.xy = (-unity_SpecCube0_Rotation.yx) * u_xlat12.xx + (-u_xlat16.yz);
        u_xlat17.y = u_xlat51.x * u_xlat10.z + u_xlat45.x;
        u_xlat45.x = u_xlat51.y * u_xlat10.x + u_xlat68;
        u_xlat17.z = u_xlat13.z * u_xlat10.z + u_xlat45.x;
        u_xlat10 = _g_textureLod(_Texture_t1, u_xlat17.xyz, u_xlat63);
        u_xlat45.x = u_xlat10.w + -1.0;
        u_xlat45.x = unity_SpecCube0_HDR.w * u_xlat45.x + 1.0;
        u_xlat45.x = max(u_xlat45.x, 0.0);
        u_xlat45.x = log2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * unity_SpecCube0_HDR.y;
        u_xlat45.x = exp2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * unity_SpecCube0_HDR.x;
        u_xlat10.xyz = u_xlat10.xyz * u_xlat45.xxx;
        u_xlat10.xyz = float3(u_xlat71) * u_xlat10.xyz;
    } else {
        u_xlat10.x = float(0.0);
        u_xlat10.y = float(0.0);
        u_xlat10.z = float(0.0);
    }
    u_xlatb45 = 0.00999999978<u_xlat9.x;
    if(u_xlatb45){
        u_xlat51.xy = u_xlat8.xy * u_xlat18.xy;
        u_xlat45.x = u_xlat70 * u_xlat8.y + u_xlat51.x;
        u_xlat68 = u_xlat8.y * u_xlat56.y;
        u_xlat12.x = u_xlat56.x * u_xlat8.z + u_xlat45.x;
        u_xlat45.x = u_xlat69 * u_xlat8.x + u_xlat51.y;
        u_xlat12.y = u_xlat11.x * u_xlat8.z + u_xlat45.x;
        u_xlat45.x = u_xlat11.z * u_xlat8.x + u_xlat68;
        u_xlat12.z = u_xlat18.z * u_xlat8.z + u_xlat45.x;
        u_xlatb45 = 0.0<unity_SpecCube1_ProbePosition.w;
        u_xlatb11.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat12.xyzx).xyz;
        u_xlat11.x = (u_xlatb11.x) ? unity_SpecCube1_BoxMax.x : unity_SpecCube1_BoxMin.x;
        u_xlat11.y = (u_xlatb11.y) ? unity_SpecCube1_BoxMax.y : unity_SpecCube1_BoxMin.y;
        u_xlat11.z = (u_xlatb11.z) ? unity_SpecCube1_BoxMax.z : unity_SpecCube1_BoxMin.z;
        u_xlat11.xyz = (-u_xlat15.xyz) + u_xlat11.xyz;
        u_xlat11.xyz = u_xlat11.xyz / u_xlat12.xyz;
        u_xlat68 = min(u_xlat11.y, u_xlat11.x);
        u_xlat68 = min(u_xlat11.z, u_xlat68);
        u_xlat11.xyz = u_xlat15.xyz + (-unity_SpecCube1_ProbePosition.xyz);
        u_xlat11.xyz = u_xlat12.xyz * float3(u_xlat68) + u_xlat11.xyz;
        u_xlat11.xyz = (bool(u_xlatb45)) ? u_xlat11.xyz : u_xlat12.xyz;
        u_xlat12.xyz = unity_SpecCube1_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat13.xyz = u_xlat12.yzx * (-unity_SpecCube1_Rotation.xyz);
        u_xlat14.xyz = u_xlat12.xyz * unity_SpecCube1_Rotation.www;
        u_xlat13.xyz = u_xlat13.zzy + u_xlat13.yxx;
        u_xlat13.xyz = (-u_xlat13.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat51.xy = u_xlat11.xy * u_xlat13.xy;
        u_xlat45.x = (-unity_SpecCube1_Rotation.x) * u_xlat12.z + (-u_xlat14.x);
        u_xlat45.x = u_xlat45.x * u_xlat11.y + u_xlat51.x;
        u_xlat33.xz = (-unity_SpecCube1_Rotation.xy) * u_xlat12.xx + u_xlat14.zy;
        u_xlat68 = u_xlat11.y * u_xlat33.z;
        u_xlat15.x = u_xlat33.x * u_xlat11.z + u_xlat45.x;
        u_xlat45.x = (-unity_SpecCube1_Rotation.x) * u_xlat12.z + u_xlat14.x;
        u_xlat45.x = u_xlat45.x * u_xlat11.x + u_xlat51.y;
        u_xlat51.xy = (-unity_SpecCube1_Rotation.yx) * u_xlat12.xx + (-u_xlat14.yz);
        u_xlat15.y = u_xlat51.x * u_xlat11.z + u_xlat45.x;
        u_xlat45.x = u_xlat51.y * u_xlat11.x + u_xlat68;
        u_xlat15.z = u_xlat13.z * u_xlat11.z + u_xlat45.x;
        u_xlat11 = _g_textureLod(_Texture_t2, u_xlat15.xyz, u_xlat63);
        u_xlat45.x = u_xlat11.w + -1.0;
        u_xlat45.x = unity_SpecCube1_HDR.w * u_xlat45.x + 1.0;
        u_xlat45.x = max(u_xlat45.x, 0.0);
        u_xlat45.x = log2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * unity_SpecCube1_HDR.y;
        u_xlat45.x = exp2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * unity_SpecCube1_HDR.x;
        u_xlat11.xyz = u_xlat11.xyz * u_xlat45.xxx;
        u_xlat10.xyz = u_xlat9.xxx * u_xlat11.xyz + u_xlat10.xyz;
    }
    u_xlatb45 = u_xlat30.x<0.99000001;
    if(u_xlatb45){
        u_xlat8 = _g_textureLod(_Texture_t0, u_xlat8.xyz, u_xlat63);
        u_xlat63 = (-u_xlat30.x) + 1.0;
        u_xlat45.x = u_xlat8.w + -1.0;
        u_xlat45.x = _GlossyEnvironmentCubeMap_HDR.w * u_xlat45.x + 1.0;
        u_xlat45.x = max(u_xlat45.x, 0.0);
        u_xlat45.x = log2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * _GlossyEnvironmentCubeMap_HDR.y;
        u_xlat45.x = exp2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * _GlossyEnvironmentCubeMap_HDR.x;
        u_xlat8.xyz = u_xlat8.xyz * u_xlat45.xxx;
        u_xlat10.xyz = float3(u_xlat63) * u_xlat8.xyz + u_xlat10.xyz;
    }
    u_xlat8.xy = float2(u_xlat64) * float2(u_xlat64) + float2(-1.0, 1.0);
    u_xlat63 = float(1.0) / u_xlat8.y;
    u_xlat29.xyz = (-u_xlat1.xyz) + float3(u_xlat66);
    u_xlat24.xyz = u_xlat24.xxx * u_xlat29.xyz + u_xlat1.xyz;
    u_xlat24.xyz = float3(u_xlat63) * u_xlat24.xyz;
    u_xlat24.xyz = u_xlat24.xyz * u_xlat10.xyz;
    u_xlat24.xyz = u_xlat5.xyz * u_xlat6.xyz + u_xlat24.xyz;
    u_xlati63 = int(uint(uint(_g_floatBitsToUint(_MainLightLayerMask)) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
    u_xlat64 = u_xlat3.x * unity_LightData.z;
    u_xlat3.x = dot(u_xlat0.xyz, _MainLightPosition.xyz);
    u_xlat3.x = clamp(u_xlat3.x, 0.0, 1.0);
    u_xlat64 = u_xlat64 * u_xlat3.x;
    u_xlat5.xyz = float3(u_xlat64) * u_xlat7.xyz;
    u_xlat7.xyz = u_xlat4.xyz + _MainLightPosition.xyz;
    u_xlat64 = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat64 = max(u_xlat64, 1.17549435e-38);
    u_xlat64 = _g_inversesqrt(u_xlat64);
    u_xlat7.xyz = float3(u_xlat64) * u_xlat7.xyz;
    u_xlat64 = dot(u_xlat0.xyz, u_xlat7.xyz);
    u_xlat64 = clamp(u_xlat64, 0.0, 1.0);
    u_xlat3.x = dot(_MainLightPosition.xyz, u_xlat7.xyz);
    u_xlat3.x = clamp(u_xlat3.x, 0.0, 1.0);
    u_xlat64 = u_xlat64 * u_xlat64;
    u_xlat64 = u_xlat64 * u_xlat8.x + 1.00001001;
    u_xlat3.x = u_xlat3.x * u_xlat3.x;
    u_xlat64 = u_xlat64 * u_xlat64;
    u_xlat3.x = max(u_xlat3.x, 0.100000001);
    u_xlat64 = u_xlat64 * u_xlat3.x;
    u_xlat64 = u_xlat67 * u_xlat64;
    u_xlat64 = u_xlat65 / u_xlat64;
    u_xlat7.xyz = u_xlat1.xyz * float3(u_xlat64) + u_xlat6.xyz;
    u_xlat5.xyz = u_xlat5.xyz * u_xlat7.xyz;
    u_xlat5.xyz = (int(u_xlati63) != 0) ? u_xlat5.xyz : float3(0.0, 0.0, 0.0);
    u_xlat63 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu63 =  uint(int(u_xlat63));
    u_xlatb7.xy = _g_equal(_pad176.zzzz, float4(0.0, 1.0, 0.0, 0.0)).xy;
    u_xlat29.x = float(0.0);
    u_xlat29.y = float(0.0);
    u_xlat29.z = float(0.0);
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<u_xlatu63 ; u_xlatu_loop_1++)
    {
        u_xlatu3 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati68 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat3.x = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati68]);
        u_xlatu3 =  uint(int(u_xlat3.x));
        u_xlat9.xyz = (-input.vs_INTERP11.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat68 = dot(u_xlat9.xyz, u_xlat9.xyz);
        u_xlat68 = max(u_xlat68, 6.10351562e-05);
        u_xlat69 = _g_inversesqrt(u_xlat68);
        u_xlat10.xyz = float3(u_xlat69) * u_xlat9.xyz;
        u_xlat49.x = float(1.0) / float(u_xlat68);
        u_xlat68 = u_xlat68 * _AdditionalLightsAttenuation.x;
        u_xlat68 = (-u_xlat68) * u_xlat68 + 1.0;
        u_xlat68 = max(u_xlat68, 0.0);
        u_xlat68 = u_xlat68 * u_xlat68;
        u_xlat68 = u_xlat68 * u_xlat49.x;
        u_xlat49.x = dot(_AdditionalLightsSpotDir.xyz, u_xlat10.xyz);
        u_xlat49.x = u_xlat49.x * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat49.x = clamp(u_xlat49.x, 0.0, 1.0);
        u_xlat49.x = u_xlat49.x * u_xlat49.x;
        u_xlat68 = u_xlat68 * u_xlat49.x;
        u_xlatu49 = uint(u_xlatu3 >> 5u);
        u_xlati70 = int(1 << int(u_xlatu3));
        u_xlati49 = int(uint(uint(u_xlati70) & uint(_g_floatBitsToUint(_pad64.x))));
        if(u_xlati49 != 0) {
            u_xlati49 = int(_pad20672.x);
            u_xlati70 = (u_xlati49 != 0) ? 0 : 1;
            u_xlati72 = int(int(u_xlatu3) << 2);
            if(u_xlati70 != 0) {
                u_xlat11.xyz = input.vs_INTERP11.yyy * _pad208.xyw;
                u_xlat11.xyz = _pad192.xyw * input.vs_INTERP11.xxx + u_xlat11.xyz;
                u_xlat11.xyz = _pad224.xyw * input.vs_INTERP11.zzz + u_xlat11.xyz;
                u_xlat11.xyz = u_xlat11.xyz + _pad240.xyw;
                u_xlat11.xy = u_xlat11.xy / u_xlat11.zz;
                u_xlat11.xy = u_xlat11.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                u_xlat11.xy = clamp(u_xlat11.xy, 0.0, 1.0);
                u_xlat11.xy = _pad16576.xy * u_xlat11.xy + _pad16576.zw;
            } else {
                u_xlatb49 = u_xlati49==1;
                u_xlati49 = u_xlatb49 ? 1 : int(0);
                if(u_xlati49 != 0) {
                    u_xlat49.xy = input.vs_INTERP11.yy * _pad208.xy;
                    u_xlat49.xy = _pad192.xy * input.vs_INTERP11.xx + u_xlat49.xy;
                    u_xlat49.xy = _pad224.xy * input.vs_INTERP11.zz + u_xlat49.xy;
                    u_xlat49.xy = u_xlat49.xy + _pad240.xy;
                    u_xlat49.xy = u_xlat49.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat49.xy = _g_fract(u_xlat49.xy);
                    u_xlat11.xy = _pad16576.xy * u_xlat49.xy + _pad16576.zw;
                } else {
                    u_xlat12 = input.vs_INTERP11.yyyy * _pad208;
                    u_xlat12 = _pad192 * input.vs_INTERP11.xxxx + u_xlat12;
                    u_xlat12 = _pad224 * input.vs_INTERP11.zzzz + u_xlat12;
                    u_xlat12 = u_xlat12 + _pad240;
                    u_xlat12.xyz = u_xlat12.xyz / u_xlat12.www;
                    u_xlat49.x = dot(u_xlat12.xyz, u_xlat12.xyz);
                    u_xlat49.x = _g_inversesqrt(u_xlat49.x);
                    u_xlat12.xyz = u_xlat49.xxx * u_xlat12.xyz;
                    u_xlat49.x = dot(abs(u_xlat12.xyz), float3(1.0, 1.0, 1.0));
                    u_xlat49.x = max(u_xlat49.x, 9.99999997e-07);
                    u_xlat49.x = float(1.0) / float(u_xlat49.x);
                    u_xlat13.xyz = u_xlat49.xxx * u_xlat12.zxy;
                    u_xlat13.x = (-u_xlat13.x);
                    u_xlat13.x = clamp(u_xlat13.x, 0.0, 1.0);
                    u_xlatb53.xy = _g_greaterThanEqual(u_xlat13.yzyz, float4(0.0, 0.0, 0.0, 0.0)).xy;
                    u_xlat53.x = (u_xlatb53.x) ? u_xlat13.x : (-u_xlat13.x);
                    u_xlat53.y = (u_xlatb53.y) ? u_xlat13.x : (-u_xlat13.x);
                    u_xlat49.xy = u_xlat12.xy * u_xlat49.xx + u_xlat53.xy;
                    u_xlat49.xy = u_xlat49.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat49.xy = clamp(u_xlat49.xy, 0.0, 1.0);
                    u_xlat11.xy = _pad16576.xy * u_xlat49.xy + _pad16576.zw;
                }
            }
            u_xlat11 = _g_textureLod(_Texture_t7, u_xlat11.xy, 0.0);
            u_xlat49.x = (u_xlatb7.y) ? u_xlat11.w : u_xlat11.x;
            u_xlat11.xyz = (u_xlatb7.x) ? u_xlat11.xyz : u_xlat49.xxx;
        } else {
            u_xlat11.x = float(1.0);
            u_xlat11.y = float(1.0);
            u_xlat11.z = float(1.0);
        }
        u_xlat11.xyz = u_xlat11.xyz * _AdditionalLightsColor.xyz;
        u_xlati3 = int(uint(uint(_g_floatBitsToUint(unity_RenderingLayer.x)) & uint(_g_floatBitsToUint(_AdditionalLightsLayerMasks))));
        u_xlat49.x = dot(u_xlat0.xyz, u_xlat10.xyz);
        u_xlat49.x = clamp(u_xlat49.x, 0.0, 1.0);
        u_xlat68 = u_xlat68 * u_xlat49.x;
        u_xlat11.xyz = float3(u_xlat68) * u_xlat11.xyz;
        u_xlat9.xyz = u_xlat9.xyz * float3(u_xlat69) + u_xlat4.xyz;
        u_xlat68 = dot(u_xlat9.xyz, u_xlat9.xyz);
        u_xlat68 = max(u_xlat68, 1.17549435e-38);
        u_xlat68 = _g_inversesqrt(u_xlat68);
        u_xlat9.xyz = float3(u_xlat68) * u_xlat9.xyz;
        u_xlat68 = dot(u_xlat0.xyz, u_xlat9.xyz);
        u_xlat68 = clamp(u_xlat68, 0.0, 1.0);
        u_xlat69 = dot(u_xlat10.xyz, u_xlat9.xyz);
        u_xlat69 = clamp(u_xlat69, 0.0, 1.0);
        u_xlat68 = u_xlat68 * u_xlat68;
        u_xlat68 = u_xlat68 * u_xlat8.x + 1.00001001;
        u_xlat69 = u_xlat69 * u_xlat69;
        u_xlat68 = u_xlat68 * u_xlat68;
        u_xlat69 = max(u_xlat69, 0.100000001);
        u_xlat68 = u_xlat68 * u_xlat69;
        u_xlat68 = u_xlat67 * u_xlat68;
        u_xlat68 = u_xlat65 / u_xlat68;
        u_xlat9.xyz = u_xlat1.xyz * float3(u_xlat68) + u_xlat6.xyz;
        u_xlat9.xyz = u_xlat9.xyz * u_xlat11.xyz + u_xlat29.xyz;
        u_xlat29.xyz = (int(u_xlati3) != 0) ? u_xlat9.xyz : u_xlat29.xyz;
    }
    u_xlat0.xyz = u_xlat24.xyz + u_xlat5.xyz;
    u_xlat0.xyz = u_xlat29.xyz + u_xlat0.xyz;
    __SV_Target0.xyz = float3(_Emission) * u_xlat2.xyz + u_xlat0.xyz;
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
