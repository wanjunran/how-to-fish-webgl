Shader "Shader Graphs/WaterShader"
{
    Properties
    {

	[ToggleUI] _Testing ("Testing", Float) = 0
	_DeepColor ("Deep Color", Vector) = (0.03921569,0.09803922,0.2745098,0.7058824)
	_DeepColor_1 ("Deep Color 2", Vector) = (0.03921569,0.09803922,0.2745098,0.7058824)
	Color_93e06cd551a5449091bcde90b46765a0 ("Shallow Color", Vector) = (0.03804734,0.5490196,0.5490196,0.1960784)
	Color_1 ("Shallow Color 2", Vector) = (0.03804734,0.5490196,0.5490196,0.1960784)
	_Pixels ("Pixels", Float) = 0
	_Color_Noise_Scale ("Color Noise Scale", Float) = 0
	Vector1_6f56a0970372485390c6587863c2374e ("Depth", Float) = -3.5
	Vector1_6c82dffdd68049bcb019d3a9c64c92a0 ("Depth Strenght", Range(0, 2)) = 0.2
	Vector1_6269b1025b26473ca8bc61634f34b537 ("Smoothness", Range(0, 1)) = 0.95
	_Wave_Frequency_1 ("Wave Frequency 1", Float) = 1
	_Gradient_Speed_1 ("Gradient Speed 1", Float) = 1
	_Wave_Direction_1 ("Wave Direction 1", Vector) = (1,0,0,0)
	_Wave_Height_1 ("Wave Height 1", Float) = 0
	_Peak_1 ("Peak 1", Float) = 1
	_Wave_Frequency_2 ("Wave Frequency 2", Float) = 1
	_Gradient_Speed_2 ("Gradient Speed 2", Float) = 1
	_Wave_Direction_2 ("Wave Direction 2", Vector) = (1,0,0,0)
	_Wave_Height_2 ("Wave Height 2", Float) = 0
	_Peak_2 ("Peak 2", Float) = 1
	_Foam_Amount ("Foam Amount", Float) = 40
	_Foam_Threshold ("Foam Threshold", Float) = 50
	_Foam_Speed ("Foam Speed", Float) = 0.05
	_Foam_Scale ("Foam Scale", Float) = 200
	_Foam_Color ("Foam Color", Vector) = (1,1,1,1)
	[NoScaleOffset] _Normal_Bumps_Map ("Normal Bumps Map", 2D) = "white" {}
	_Normal_Bumps_Strength ("Normal Bumps Strength", Range(0, 1)) = 0.2
	_Normal_Bumps_Fade_Speed ("Normal Bumps Fade Speed", Float) = 1
	_Normal_Bumps_Scale ("Normal Bumps Scale", Vector) = (1,2,0,0)
	_Normal_Bumps_2_Offset ("Normal Bumps 2 Offset", Float) = 100
	_Specular_Smoothness ("Specular Smoothness", Range(0, 1)) = 0
	[HDR] _Specular_Color ("Specular Color", Vector) = (0,0,0,1)
	_Specular_SmoothStep ("Specular SmoothStep", Vector) = (0,1,0,0)
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
            TEXTURE2D(_Texture_t3);
            SAMPLER(sampler__Texture_t3);
            TEXTURE2D(_Texture_t4);
            SAMPLER(sampler__Texture_t4);
            TEXTURE2D(_Texture_t5);
            SAMPLER(sampler__Texture_t5);
            TEXTURE2D(_Texture_t6);
            SAMPLER(sampler__Texture_t6);
            TEXTURE2D(_Texture_t7);
            SAMPLER(sampler__Texture_t7);
            TEXTURE2D(_Texture_t8);
            SAMPLER(sampler__Texture_t8);

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
            float4 _pad400;
            float4 _pad976;
            float4 _pad992;
            float4x4 unity_MatrixVP;
            float _Fake_Time;
            float _Wave_Offset;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float _Gradient_Speed_1;
            float _Wave_Frequency_1;
            float3 _Wave_Direction_1;
            float _Wave_Height_1;
            float _Peak_1;
            float _Wave_Frequency_2;
            float _Gradient_Speed_2;
            float3 _Wave_Direction_2;
            float _Wave_Height_2;
            float _Peak_2;
            float _Testing;
            float4 _GlossyEnvironmentCubeMap_HDR;
            float4 _ScaledScreenParams;
            float2 _GlobalMipBias;
            float4 _MainLightPosition;
            float4 _MainLightColor;
            float _MainLightLayerMask;
            float4 _AdditionalLightsCount;
            float3 _WorldSpaceCameraPos;
            float4 _ProjectionParams;
            float4 _ZBufferParams;
            float4 unity_OrthoParams;
            float4 _RTHandleScale;
            float4x4 unity_MatrixV;
            float4x4 unity_MatrixInvVP;
            float4 _CameraDepthTexture_TexelSize;
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
            float4 _DeepColor_1;
            float4 _DeepColor;
            float4 Color_1;
            float4 Color_93e06cd551a5449091bcde90b46765a0;
            float Vector1_6f56a0970372485390c6587863c2374e;
            float Vector1_6c82dffdd68049bcb019d3a9c64c92a0;
            float Vector1_6269b1025b26473ca8bc61634f34b537;
            float _Foam_Amount;
            float _Foam_Threshold;
            float _Foam_Speed;
            float _Foam_Scale;
            float4 _Foam_Color;
            float _Normal_Bumps_Strength;
            float2 _Normal_Bumps_Scale;
            float _Pixels;
            float _Color_Noise_Scale;
            float4 _Specular_Color;
            float2 _Specular_SmoothStep;
            float _Specular_Smoothness;

            float4 u_xlat0;
            float3 u_xlat1;
            float3 u_xlat2;
            float4 u_xlat3;
            float4 u_xlat4;
            float3 u_xlat5;
            float3 u_xlat6;
            bool u_xlatb6;
            float2 u_xlat7;
            float2 u_xlat8;
            float2 u_xlat12;
            float4 ImmCB_0_0_0[4];
            int u_xlati0;
            uint u_xlatu0;
            int2 u_xlati2;
            uint u_xlatu2;
            int2 u_xlati3;
            uint2 u_xlatu3;
            int4 u_xlati4;
            uint3 u_xlatu4;
            int4 u_xlati5;
            uint2 u_xlatu5;
            int4 u_xlati6;
            uint2 u_xlatu6;
            int4 u_xlati7;
            uint2 u_xlatu7;
            bool u_xlatb8;
            float4 u_xlat9;
            float4 u_xlat10;
            int u_xlati10;
            bool4 u_xlatb10;
            float4 u_xlat11;
            float4 u_xlat13;
            float3 u_xlat14;
            float3 u_xlat15;
            float4 u_xlat16;
            float3 u_xlat17;
            float4 u_xlat18;
            float3 u_xlat19;
            float3 u_xlat20;
            bool2 u_xlatb20;
            float3 u_xlat22;
            int u_xlati22;
            uint u_xlatu22;
            float2 u_xlat25;
            int3 u_xlati25;
            uint2 u_xlatu25;
            float u_xlat28;
            bool u_xlatb28;
            float3 u_xlat29;
            float3 u_xlat30;
            float3 u_xlat31;
            float3 u_xlat35;
            float2 u_xlat40;
            float2 u_xlat42;
            int2 u_xlati42;
            uint2 u_xlatu42;
            float2 u_xlat44;
            int2 u_xlati44;
            uint2 u_xlatu44;
            float2 u_xlat45;
            uint2 u_xlatu45;
            bool2 u_xlatb45;
            float u_xlat48;
            bool u_xlatb48;
            float2 u_xlat53;
            float u_xlat60;
            int u_xlati60;
            uint u_xlatu60;
            bool u_xlatb60;
            float u_xlat61;
            int u_xlati61;
            uint u_xlatu61;
            float u_xlat62;
            int u_xlati62;
            uint u_xlatu62;
            float u_xlat63;
            float u_xlat64;
            int u_xlati64;
            uint u_xlatu64;
            bool u_xlatb64;
            float u_xlat65;
            bool u_xlatb65;
            float u_xlat66;
            int u_xlati66;
            float u_xlat67;
            float u_xlat68;
            int u_xlati68;
            uint u_xlatu68;
            bool u_xlatb68;
            float u_xlat69;
            int u_xlati69;
            bool u_xlatb69;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float4 in_TANGENT0 : TANGENT;
                float4 in_TEXCOORD0 : TEXCOORD0;
                float4 in_TEXCOORD1 : TEXCOORD1;
                float4 in_TEXCOORD3 : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 vs_INTERP0 : TEXCOORD0;
                float3 vs_INTERP10 : TEXCOORD10;
                float4 vs_INTERP5 : TEXCOORD5;
                float4 vs_INTERP6 : TEXCOORD6;
                float4 vs_INTERP7 : TEXCOORD7;
                float4 vs_INTERP8 : TEXCOORD8;
                float3 vs_INTERP9 : TEXCOORD9;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0.x = dot(_Wave_Direction_1.xyz, _Wave_Direction_1.xyz);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat0.xyz = u_xlat0.xxx * _Wave_Direction_1.xyz;
    u_xlat1.xyz = input.in_POSITION0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat1.xyz = _tunity_ObjectToWorld[0].xyz * input.in_POSITION0.xxx + u_xlat1.xyz;
    u_xlat1.xyz = _tunity_ObjectToWorld[2].xyz * input.in_POSITION0.zzz + u_xlat1.xyz;
    u_xlat1.xyz = u_xlat1.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat1.xyz);
    u_xlatb6 = float4(0.0, 0.0, 0.0, 0.0)!=float4(_Testing);
    u_xlat6.x = (u_xlatb6) ? _TimeParameters.x : _Fake_Time;
    u_xlat0.y = u_xlat6.x + _Wave_Offset;
    u_xlat12.x = u_xlat0.y * _Gradient_Speed_1;
    u_xlat0.x = u_xlat0.x * _Wave_Frequency_1 + u_xlat12.x;
    u_xlat2.x = cos(u_xlat0.x);
    u_xlat0.x = sin(u_xlat0.x);
    u_xlat3.y = u_xlat0.x * _Wave_Height_1;
    u_xlat0.z = u_xlat2.x * _Wave_Height_1;
    u_xlat0.x = u_xlat0.x * _Wave_Frequency_1;
    u_xlat0.xyw = u_xlat0.xyz * float3(_Peak_1, _Gradient_Speed_2, _Peak_1);
    u_xlat12.x = u_xlat0.z * _Wave_Frequency_1;
    u_xlat8.y = u_xlat12.x * _Wave_Direction_1.z;
    u_xlat4.xz = _Wave_Direction_1.xz;
    u_xlat4.y = 0.0;
    u_xlat3.x = float(0.0);
    u_xlat3.z = float(0.0);
    u_xlat3.xyz = u_xlat4.xyz * u_xlat0.www + u_xlat3.xyz;
    u_xlat12.x = dot(_Wave_Direction_2.xyz, _Wave_Direction_2.xyz);
    u_xlat12.x = _g_inversesqrt(u_xlat12.x);
    u_xlat4.xyz = u_xlat12.xxx * _Wave_Direction_2.xyz;
    u_xlat12.x = dot(u_xlat4.xyz, u_xlat1.xyz);
    u_xlat6.x = u_xlat12.x * _Wave_Frequency_2 + u_xlat0.y;
    u_xlat1.x = sin(u_xlat6.x);
    u_xlat2.x = cos(u_xlat6.x);
    u_xlat4.y = u_xlat1.x * _Wave_Height_2;
    u_xlat6.x = u_xlat2.x * _Wave_Height_2;
    u_xlat6.y = u_xlat1.x * _Wave_Frequency_2;
    u_xlat12.xy = u_xlat6.yx * float2(_Peak_2);
    u_xlat6.x = u_xlat6.x * _Wave_Frequency_2;
    u_xlat7.y = u_xlat6.x * _Wave_Direction_2.z;
    u_xlat5.xz = _Wave_Direction_2.xz;
    u_xlat5.y = 0.0;
    u_xlat4.x = float(0.0);
    u_xlat4.z = float(0.0);
    u_xlat4.xyz = u_xlat5.xyz * u_xlat12.yyy + u_xlat4.xyz;
    u_xlat3.xyz = u_xlat3.xyz + u_xlat4.xyz;
    u_xlat4.xz = input.in_POSITION0.xz;
    u_xlat4.y = 0.0;
    u_xlat3.xyz = u_xlat3.xyz + u_xlat4.xyz;
    u_xlat4.xyz = u_xlat3.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat3.xyw = _tunity_ObjectToWorld[0].xyz * u_xlat3.xxx + u_xlat4.xyz;
    u_xlat3.xyz = _tunity_ObjectToWorld[2].xyz * u_xlat3.zzz + u_xlat3.xyw;
    u_xlat3.xyz = u_xlat3.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat4 = u_xlat3.yyyy * _tunity_MatrixVP[1];
    u_xlat4 = _tunity_MatrixVP[0] * u_xlat3.xxxx + u_xlat4;
    u_xlat4 = _tunity_MatrixVP[2] * u_xlat3.zzzz + u_xlat4;
    output.vs_INTERP9.xyz = u_xlat3.xyz;
    output.positionCS = u_xlat4 + _tunity_MatrixVP[3];
    output.vs_INTERP0.xy = input.in_TEXCOORD1.xy * _pad400.xy + _pad400.zw;
    u_xlat3.xyz = input.in_TANGENT0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat3.xyz = _tunity_ObjectToWorld[0].xyz * input.in_TANGENT0.xxx + u_xlat3.xyz;
    u_xlat3.xyz = _tunity_ObjectToWorld[2].xyz * input.in_TANGENT0.zzz + u_xlat3.xyz;
    u_xlat6.x = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat6.x = max(u_xlat6.x, 1.17549435e-38);
    u_xlat6.x = _g_inversesqrt(u_xlat6.x);
    output.vs_INTERP5.xyz = u_xlat6.xxx * u_xlat3.xyz;
    output.vs_INTERP5.w = input.in_TANGENT0.w;
    output.vs_INTERP6 = input.in_TEXCOORD0;
    output.vs_INTERP7 = input.in_TEXCOORD3;
    output.vs_INTERP8 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat6.xz = _Wave_Direction_2.zx * _Wave_Direction_2.zx;
    u_xlat6.xy = (-u_xlat6.xz) * u_xlat12.xx + float2(1.0, 1.0);
    u_xlat7.x = 1.0;
    u_xlat3.yz = u_xlat7.xy * u_xlat6.xy;
    u_xlat1.x = u_xlat7.y * u_xlat6.x;
    u_xlat1.y = float(0.0);
    u_xlat1.z = float(0.0);
    u_xlat3.x = 0.0;
    u_xlat6.xyz = (-u_xlat1.xyz) + u_xlat3.xyz;
    u_xlat1.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    u_xlat6.xyz = u_xlat6.xyz * u_xlat1.xxx;
    u_xlat1.xy = _Wave_Direction_1.zx * _Wave_Direction_1.zx;
    u_xlat1.xy = (-u_xlat1.xy) * u_xlat0.xx + float2(1.0, 1.0);
    u_xlat8.x = 1.0;
    u_xlat3.yz = u_xlat8.xy * u_xlat1.xy;
    u_xlat1.x = u_xlat8.y * u_xlat1.x;
    u_xlat1.y = float(0.0);
    u_xlat1.z = float(0.0);
    u_xlat3.x = 0.0;
    u_xlat1.xyz = (-u_xlat1.xyz) + u_xlat3.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat2.xy = u_xlat1.xy * u_xlat0.xx + u_xlat6.xy;
    u_xlat0.x = u_xlat0.x * u_xlat1.z;
    u_xlat2.z = u_xlat6.z * u_xlat0.x;
    u_xlat0.x = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat0.xyz = u_xlat0.xxx * u_xlat2.xyz;
    u_xlat1.x = dot(u_xlat0.xyz, _tunity_ObjectToWorld[0].xyz);
    u_xlat1.y = dot(u_xlat0.xyz, _tunity_ObjectToWorld[1].xyz);
    u_xlat1.z = dot(u_xlat0.xyz, _tunity_ObjectToWorld[2].xyz);
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat0.xyz = u_xlat0.xxx * u_xlat1.xyz;
    u_xlat1.x = dot(u_xlat0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat1.y = dot(u_xlat0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat1.z = dot(u_xlat0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    output.vs_INTERP10.xyz = u_xlat0.xxx * u_xlat1.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_MatrixInvVP = transpose(unity_MatrixInvVP);
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_0_0[3] = float4(0.0, 0.0, 0.0, 1.0);
float4 hlslcc_FragCoord = float4(input.positionCS.xyz, 1.0/input.positionCS.w);
    u_xlat0.x = dot(input.vs_INTERP10.xyz, input.vs_INTERP10.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = float(1.0) / u_xlat0.x;
    u_xlatb20.xy = _g_equal(unity_OrthoParams.wwww, float4(0.0, 1.0, 0.0, 0.0)).xy;
    u_xlat1.xyz = (-input.vs_INTERP9.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat60 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat60 = _g_inversesqrt(u_xlat60);
    u_xlat1.xyz = float3(u_xlat60) * u_xlat1.xyz;
    u_xlat2.x = _tunity_MatrixV[0].z;
    u_xlat2.y = _tunity_MatrixV[1].z;
    u_xlat2.z = _tunity_MatrixV[2].z;
    u_xlat1.xyz = (u_xlatb20.x) ? u_xlat1.xyz : u_xlat2.xyz;
    u_xlat20.x = input.vs_INTERP9.y * _tunity_MatrixVP[1].w;
    u_xlat20.x = _tunity_MatrixVP[0].w * input.vs_INTERP9.x + u_xlat20.x;
    u_xlat20.x = _tunity_MatrixVP[2].w * input.vs_INTERP9.z + u_xlat20.x;
    u_xlat20.x = u_xlat20.x + _tunity_MatrixVP[3].w;
    u_xlatb60 = _ProjectionParams.x<0.0;
    u_xlat61 = (-hlslcc_FragCoord.y) + _ScaledScreenParams.y;
    u_xlat2.y = (u_xlatb60) ? u_xlat61 : hlslcc_FragCoord.y;
    u_xlat2.x = hlslcc_FragCoord.x;
    u_xlat2.xy = u_xlat2.xy / _ScaledScreenParams.xy;
    u_xlat3 = input.vs_INTERP6.xyxy * float4(float4(_Pixels, _Pixels, _Pixels, _Pixels));
    u_xlat3 = floor(u_xlat3);
    u_xlat3 = u_xlat3 / float4(float4(_Pixels, _Pixels, _Pixels, _Pixels));
    u_xlat4.xy = u_xlat3.zw * float2(float2(_Color_Noise_Scale, _Color_Noise_Scale));
    u_xlat44.xy = floor(u_xlat4.xy);
    u_xlat4.xy = _g_fract(u_xlat4.xy);
    u_xlat5.xy = u_xlat4.xy * u_xlat4.xy;
    u_xlat4.xy = (-u_xlat4.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat4.xy = u_xlat4.xy * u_xlat5.xy;
    u_xlat5 = u_xlat44.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat6.xy = u_xlat44.xy + float2(1.0, 1.0);
    u_xlati44.xy = int2(u_xlat44.xy);
    u_xlati60 = int(uint(uint(u_xlati44.y) ^ 1103515245u));
    u_xlati61 = u_xlati60 + u_xlati44.x;
    u_xlatu60 = uint(u_xlati60) * uint(u_xlati61);
    u_xlatu61 = uint(u_xlatu60 >> 5u);
    u_xlati60 = int(uint(u_xlatu60 ^ u_xlatu61));
    u_xlatu60 = uint(u_xlati60) * 668265261u;
    u_xlatu60 = uint(u_xlatu60 >> 8u);
    u_xlat60 = float(u_xlatu60);
    u_xlat60 = u_xlat60 * 5.96046519e-08;
    u_xlati5 = int4(u_xlat5);
    u_xlati44.xy = int2(uint2(uint(u_xlati5.y) ^ uint(1103515245u), uint(u_xlati5.w) ^ uint(1103515245u)));
    u_xlati5.xy = u_xlati44.xy + u_xlati5.xz;
    u_xlatu44.xy = uint2(u_xlati44.xy) * uint2(u_xlati5.xy);
    u_xlatu5.xy = uint2(u_xlatu44.x >> uint(5u), u_xlatu44.y >> uint(5u));
    u_xlati44.xy = int2(uint2(u_xlatu44.x ^ u_xlatu5.x, u_xlatu44.y ^ u_xlatu5.y));
    u_xlatu44.xy = uint2(u_xlati44.xy) * uint2(668265261u, 668265261u);
    u_xlatu44.xy = uint2(u_xlatu44.x >> uint(8u), u_xlatu44.y >> uint(8u));
    u_xlat44.xy = float2(u_xlatu44.xy);
    u_xlat61 = u_xlat44.y * 5.96046519e-08;
    u_xlati5.xy = int2(u_xlat6.xy);
    u_xlati62 = int(uint(uint(u_xlati5.y) ^ 1103515245u));
    u_xlati64 = u_xlati62 + u_xlati5.x;
    u_xlatu62 = uint(u_xlati62) * uint(u_xlati64);
    u_xlatu64 = uint(u_xlatu62 >> 5u);
    u_xlati62 = int(uint(u_xlatu62 ^ u_xlatu64));
    u_xlatu62 = uint(u_xlati62) * 668265261u;
    u_xlatu62 = uint(u_xlatu62 >> 8u);
    u_xlat62 = float(u_xlatu62);
    u_xlat44.x = u_xlat44.x * 5.96046519e-08 + (-u_xlat60);
    u_xlat60 = u_xlat4.x * u_xlat44.x + u_xlat60;
    u_xlat62 = u_xlat62 * 5.96046519e-08 + (-u_xlat61);
    u_xlat61 = u_xlat4.x * u_xlat62 + u_xlat61;
    u_xlat61 = (-u_xlat60) + u_xlat61;
    u_xlat60 = u_xlat4.y * u_xlat61 + u_xlat60;
    u_xlat4 = float4(float4(_Color_Noise_Scale, _Color_Noise_Scale, _Color_Noise_Scale, _Color_Noise_Scale)) * float4(0.5, 0.5, 0.25, 0.25);
    u_xlat3 = u_xlat3 * u_xlat4;
    u_xlat4 = floor(u_xlat3);
    u_xlat3 = _g_fract(u_xlat3);
    u_xlat5 = u_xlat3 * u_xlat3;
    u_xlat3 = (-u_xlat3) * float4(2.0, 2.0, 2.0, 2.0) + float4(3.0, 3.0, 3.0, 3.0);
    u_xlat3 = u_xlat3 * u_xlat5;
    u_xlat5 = u_xlat4.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat6 = u_xlat4 + float4(1.0, 1.0, 1.0, 0.0);
    u_xlati7 = int4(u_xlat4);
    u_xlati4.xy = int2(uint2(uint(u_xlati7.y) ^ uint(1103515245u), uint(u_xlati7.w) ^ uint(1103515245u)));
    u_xlati7.xy = u_xlati4.xy + u_xlati7.xz;
    u_xlatu4.xy = uint2(u_xlati4.xy) * uint2(u_xlati7.xy);
    u_xlatu7.xy = uint2(u_xlatu4.x >> uint(5u), u_xlatu4.y >> uint(5u));
    u_xlati4.xy = int2(uint2(u_xlatu4.x ^ u_xlatu7.x, u_xlatu4.y ^ u_xlatu7.y));
    u_xlatu4.xy = uint2(u_xlati4.xy) * uint2(668265261u, 668265261u);
    u_xlatu4.xy = uint2(u_xlatu4.x >> uint(8u), u_xlatu4.y >> uint(8u));
    u_xlat4.xy = float2(u_xlatu4.xy);
    u_xlat4.xy = u_xlat4.xy * float2(5.96046519e-08, 5.96046519e-08);
    u_xlati5 = int4(u_xlat5);
    u_xlati25.xz = int2(uint2(uint(u_xlati5.y) ^ uint(1103515245u), uint(u_xlati5.w) ^ uint(1103515245u)));
    u_xlati5.xz = u_xlati25.xz + u_xlati5.xz;
    u_xlatu5.xy = uint2(u_xlati25.xz) * uint2(u_xlati5.xz);
    u_xlatu45.xy = uint2(u_xlatu5.x >> uint(5u), u_xlatu5.y >> uint(5u));
    u_xlati5.xy = int2(uint2(u_xlatu45.x ^ u_xlatu5.x, u_xlatu45.y ^ u_xlatu5.y));
    u_xlatu5.xy = uint2(u_xlati5.xy) * uint2(668265261u, 668265261u);
    u_xlatu5.xy = uint2(u_xlatu5.x >> uint(8u), u_xlatu5.y >> uint(8u));
    u_xlat5.xy = float2(u_xlatu5.xy);
    u_xlat61 = u_xlat5.y * 5.96046519e-08;
    u_xlati6 = int4(u_xlat6);
    u_xlati25.xy = int2(uint2(uint(u_xlati6.y) ^ uint(1103515245u), uint(u_xlati6.w) ^ uint(1103515245u)));
    u_xlati6.xy = u_xlati25.xy + u_xlati6.xz;
    u_xlatu25.xy = uint2(u_xlati25.xy) * uint2(u_xlati6.xy);
    u_xlatu6.xy = uint2(u_xlatu25.x >> uint(5u), u_xlatu25.y >> uint(5u));
    u_xlati25.xy = int2(uint2(u_xlatu25.x ^ u_xlatu6.x, u_xlatu25.y ^ u_xlatu6.y));
    u_xlatu25.xy = uint2(u_xlati25.xy) * uint2(668265261u, 668265261u);
    u_xlatu25.xy = uint2(u_xlatu25.x >> uint(8u), u_xlatu25.y >> uint(8u));
    u_xlat25.xy = float2(u_xlatu25.xy);
    u_xlat62 = u_xlat5.x * 5.96046519e-08 + (-u_xlat4.x);
    u_xlat62 = u_xlat3.x * u_xlat62 + u_xlat4.x;
    u_xlat4.x = u_xlat25.x * 5.96046519e-08 + (-u_xlat61);
    u_xlat61 = u_xlat3.x * u_xlat4.x + u_xlat61;
    u_xlat61 = (-u_xlat62) + u_xlat61;
    u_xlat61 = u_xlat3.y * u_xlat61 + u_xlat62;
    u_xlat61 = u_xlat61 * 0.25;
    u_xlat60 = u_xlat60 * 0.125 + u_xlat61;
    u_xlat6 = u_xlat4.zwzw + float4(0.0, 1.0, 1.0, 1.0);
    u_xlati6 = int4(u_xlat6);
    u_xlati3.xy = int2(uint2(uint(u_xlati6.y) ^ uint(1103515245u), uint(u_xlati6.w) ^ uint(1103515245u)));
    u_xlati4.xz = u_xlati3.xy + u_xlati6.xz;
    u_xlatu3.xy = uint2(u_xlati3.xy) * uint2(u_xlati4.xz);
    u_xlatu4.xz = uint2(u_xlatu3.x >> uint(5u), u_xlatu3.y >> uint(5u));
    u_xlati3.xy = int2(uint2(u_xlatu3.x ^ u_xlatu4.x, u_xlatu3.y ^ u_xlatu4.z));
    u_xlatu3.xy = uint2(u_xlati3.xy) * uint2(668265261u, 668265261u);
    u_xlatu3.xy = uint2(u_xlatu3.x >> uint(8u), u_xlatu3.y >> uint(8u));
    u_xlat3.xy = float2(u_xlatu3.xy);
    u_xlat61 = u_xlat3.x * 5.96046519e-08;
    u_xlat62 = u_xlat25.y * 5.96046519e-08 + (-u_xlat4.y);
    u_xlat62 = u_xlat3.z * u_xlat62 + u_xlat4.y;
    u_xlat3.x = u_xlat3.y * 5.96046519e-08 + (-u_xlat61);
    u_xlat61 = u_xlat3.z * u_xlat3.x + u_xlat61;
    u_xlat61 = (-u_xlat62) + u_xlat61;
    u_xlat61 = u_xlat3.w * u_xlat61 + u_xlat62;
    u_xlat60 = u_xlat61 * 0.5 + u_xlat60;
    u_xlat3.xyz = Color_1.xyz + (-Color_93e06cd551a5449091bcde90b46765a0.xyz);
    u_xlat3.xyz = float3(u_xlat60) * u_xlat3.xyz + Color_93e06cd551a5449091bcde90b46765a0.xyz;
    u_xlat4.xyz = _DeepColor_1.xyz + (-_DeepColor.xyz);
    u_xlat4.xyz = float3(u_xlat60) * u_xlat4.xyz + _DeepColor.xyz;
    u_xlat5.xy = (-_CameraDepthTexture_TexelSize.xy) * float2(0.5, 0.5) + float2(1.0, 1.0);
    u_xlat2.z = (-u_xlat2.y) + 1.0;
    u_xlat22.xz = min(u_xlat2.xz, u_xlat5.xy);
    u_xlat22.xz = u_xlat22.xz * _RTHandleScale.xy;
    u_xlat60 = _g_texture(_Texture_t7, u_xlat22.xz, _GlobalMipBias.x).x;
    u_xlat61 = _ZBufferParams.x * u_xlat60 + _ZBufferParams.y;
    u_xlat61 = float(1.0) / u_xlat61;
    u_xlat22.x = u_xlat20.x + Vector1_6f56a0970372485390c6587863c2374e;
    u_xlat61 = u_xlat61 * _ProjectionParams.z + (-u_xlat22.x);
    u_xlat61 = u_xlat61 * Vector1_6c82dffdd68049bcb019d3a9c64c92a0;
    u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
    u_xlat4.xyz = (-u_xlat3.xyz) + u_xlat4.xyz;
    u_xlat3.xyz = float3(u_xlat61) * u_xlat4.xyz + u_xlat3.xyz;
    if(u_xlatb20.y){
        u_xlat2.xy = u_xlat2.xz * float2(2.0, 2.0) + float2(-1.0, -1.0);
        u_xlat4 = (-u_xlat2.yyyy) * _tunity_MatrixInvVP[1];
        u_xlat2 = _tunity_MatrixInvVP[0] * u_xlat2.xxxx + u_xlat4;
        u_xlat2 = _tunity_MatrixInvVP[2] * float4(u_xlat60) + u_xlat2;
        u_xlat2 = u_xlat2 + _tunity_MatrixInvVP[3];
        u_xlat2.xyz = u_xlat2.xyz / u_xlat2.www;
        u_xlat40.x = u_xlat2.y * _tunity_MatrixV[1].z;
        u_xlat40.x = _tunity_MatrixV[0].z * u_xlat2.x + u_xlat40.x;
        u_xlat40.x = _tunity_MatrixV[2].z * u_xlat2.z + u_xlat40.x;
        u_xlat40.x = u_xlat40.x + _tunity_MatrixV[3].z;
        u_xlat40.x = abs(u_xlat40.x);
    } else {
        u_xlat60 = _ZBufferParams.z * u_xlat60 + _ZBufferParams.w;
        u_xlat40.x = float(1.0) / u_xlat60;
    }
    u_xlat20.x = (-u_xlat20.x) + u_xlat40.x;
    u_xlat20.x = u_xlat20.x + -0.00999999978;
    u_xlat20.x = u_xlat20.x / _Foam_Amount;
    u_xlat20.x = clamp(u_xlat20.x, 0.0, 1.0);
    u_xlat20.x = u_xlat20.x * _Foam_Threshold;
    u_xlat40.x = _TimeParameters.x * _Foam_Speed;
    u_xlat40.xy = input.vs_INTERP6.xy * float2(float2(_Foam_Scale, _Foam_Scale)) + u_xlat40.xx;
    u_xlat2.xy = floor(u_xlat40.xy);
    u_xlat40.xy = _g_fract(u_xlat40.xy);
    u_xlati42.xy = int2(u_xlat2.xy);
    u_xlati61 = int(uint(uint(u_xlati42.y) ^ 1103515245u));
    u_xlati42.x = u_xlati61 + u_xlati42.x;
    u_xlatu61 = uint(u_xlati61) * uint(u_xlati42.x);
    u_xlatu42.x = uint(u_xlatu61 >> 5u);
    u_xlati61 = int(uint(u_xlatu61 ^ u_xlatu42.x));
    u_xlatu61 = uint(u_xlati61) * 668265261u;
    u_xlatu61 = uint(u_xlatu61 >> 8u);
    u_xlat61 = float(u_xlatu61);
    u_xlat4.yz = float2(u_xlat61) * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat42.x = floor(u_xlat4.y);
    u_xlat4.x = u_xlat61 * 5.96046519e-08 + (-u_xlat42.x);
    u_xlat61 = dot(u_xlat4.xz, u_xlat4.xz);
    u_xlat61 = _g_inversesqrt(u_xlat61);
    u_xlat42.xy = float2(u_xlat61) * u_xlat4.xz;
    u_xlat61 = dot(u_xlat42.xy, u_xlat40.xy);
    u_xlat4 = u_xlat2.xyxy + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati4 = int4(u_xlat4);
    u_xlati42.xy = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati4.xy = u_xlati42.xy + u_xlati4.xz;
    u_xlatu42.xy = uint2(u_xlati42.xy) * uint2(u_xlati4.xy);
    u_xlatu4.xy = uint2(u_xlatu42.x >> uint(5u), u_xlatu42.y >> uint(5u));
    u_xlati42.xy = int2(uint2(u_xlatu42.x ^ u_xlatu4.x, u_xlatu42.y ^ u_xlatu4.y));
    u_xlatu42.xy = uint2(u_xlati42.xy) * uint2(668265261u, 668265261u);
    u_xlatu42.xy = uint2(u_xlatu42.x >> uint(8u), u_xlatu42.y >> uint(8u));
    u_xlat42.xy = float2(u_xlatu42.xy);
    u_xlat4 = u_xlat42.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat5.xy = floor(u_xlat4.xy);
    u_xlat4.xy = u_xlat42.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat5.xy);
    u_xlat42.x = dot(u_xlat4.xz, u_xlat4.xz);
    u_xlat42.x = _g_inversesqrt(u_xlat42.x);
    u_xlat42.xy = u_xlat42.xx * u_xlat4.xz;
    u_xlat5 = u_xlat40.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat42.x = dot(u_xlat42.xy, u_xlat5.xy);
    u_xlat62 = dot(u_xlat4.yw, u_xlat4.yw);
    u_xlat62 = _g_inversesqrt(u_xlat62);
    u_xlat4.xy = float2(u_xlat62) * u_xlat4.yw;
    u_xlat62 = dot(u_xlat4.xy, u_xlat5.zw);
    u_xlat2.xy = u_xlat2.xy + float2(1.0, 1.0);
    u_xlati2.xy = int2(u_xlat2.xy);
    u_xlati22 = int(uint(uint(u_xlati2.y) ^ 1103515245u));
    u_xlati2.x = u_xlati22 + u_xlati2.x;
    u_xlatu2 = uint(u_xlati22) * uint(u_xlati2.x);
    u_xlatu22 = uint(u_xlatu2 >> 5u);
    u_xlati2.x = int(uint(u_xlatu22 ^ u_xlatu2));
    u_xlatu2 = uint(u_xlati2.x) * 668265261u;
    u_xlatu2 = uint(u_xlatu2 >> 8u);
    u_xlat2.x = float(u_xlatu2);
    u_xlat4.yz = u_xlat2.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat22.x = floor(u_xlat4.y);
    u_xlat4.x = u_xlat2.x * 5.96046519e-08 + (-u_xlat22.x);
    u_xlat2.x = dot(u_xlat4.xz, u_xlat4.xz);
    u_xlat2.x = _g_inversesqrt(u_xlat2.x);
    u_xlat2.xy = u_xlat2.xx * u_xlat4.xz;
    u_xlat4.xy = u_xlat40.xy + float2(-1.0, -1.0);
    u_xlat2.x = dot(u_xlat2.xy, u_xlat4.xy);
    u_xlat4.xy = u_xlat40.xy * u_xlat40.xy;
    u_xlat4.xy = u_xlat40.xy * u_xlat4.xy;
    u_xlat44.xy = u_xlat40.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat40.xy = u_xlat40.xy * u_xlat44.xy + float2(10.0, 10.0);
    u_xlat40.xy = u_xlat40.xy * u_xlat4.xy;
    u_xlat22.x = (-u_xlat61) + u_xlat42.x;
    u_xlat61 = u_xlat40.y * u_xlat22.x + u_xlat61;
    u_xlat2.x = (-u_xlat62) + u_xlat2.x;
    u_xlat60 = u_xlat40.y * u_xlat2.x + u_xlat62;
    u_xlat60 = (-u_xlat61) + u_xlat60;
    u_xlat40.x = u_xlat40.x * u_xlat60 + u_xlat61;
    u_xlat40.x = u_xlat40.x + 0.5;
    u_xlatb20.x = u_xlat40.x>=u_xlat20.x;
    u_xlat20.x = u_xlatb20.x ? 1.0 : float(0.0);
    u_xlat2.xyz = (-u_xlat3.xyz) + _Foam_Color.xyz;
    u_xlat20.xyz = u_xlat20.xxx * u_xlat2.xyz + u_xlat3.xyz;
    u_xlat2.xyz = dFdy(input.vs_INTERP9.zxy);
    u_xlat3.xyz = dFdx(input.vs_INTERP9.yzx);
    u_xlat4.xyz = u_xlat2.xyz * u_xlat3.xyz;
    u_xlat2.xyz = u_xlat2.zxy * u_xlat3.yzx + (-u_xlat4.xyz);
    u_xlat61 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat61 = _g_inversesqrt(u_xlat61);
    u_xlat3.xyz = float3(u_xlat61) * u_xlat2.xyz;
    u_xlat42.xy = input.vs_INTERP7.xy * _Normal_Bumps_Scale.xy;
    u_xlat4.xyz = _g_texture(_Texture_t8, u_xlat42.xy, _GlobalMipBias.x).xyw;
    u_xlat4.x = u_xlat4.x * u_xlat4.z;
    u_xlat42.xy = u_xlat4.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat63 = dot(u_xlat42.xy, u_xlat42.xy);
    u_xlat63 = min(u_xlat63, 1.0);
    u_xlat63 = (-u_xlat63) + 1.0;
    u_xlat63 = sqrt(u_xlat63);
    u_xlat63 = max(u_xlat63, 1.00000002e-16);
    u_xlat2.xy = u_xlat2.xy * float2(u_xlat61) + u_xlat42.xy;
    u_xlat2.z = u_xlat63 * u_xlat3.z;
    u_xlat61 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat61 = max(u_xlat61, 1.17549435e-38);
    u_xlat61 = _g_inversesqrt(u_xlat61);
    u_xlat2.xyz = u_xlat2.xyz * float3(u_xlat61) + (-u_xlat3.xyz);
    u_xlat2.xyz = float3(_Normal_Bumps_Strength) * u_xlat2.xyz + u_xlat3.xyz;
    u_xlat61 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat61 = _g_inversesqrt(u_xlat61);
    u_xlat2.xyz = float3(u_xlat61) * u_xlat2.xyz;
    u_xlat3.xyz = u_xlat0.xxx * input.vs_INTERP10.xyz + (-_MainLightPosition.xyz);
    u_xlat0.x = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat3.xyz = u_xlat0.xxx * u_xlat3.xyz;
    u_xlat0.x = dot(u_xlat3.xyz, u_xlat1.xyz);
    u_xlat61 = (-_Specular_SmoothStep.x) + _Specular_SmoothStep.y;
    u_xlat0.x = u_xlat0.x + (-_Specular_SmoothStep.x);
    u_xlat61 = float(1.0) / u_xlat61;
    u_xlat0.x = u_xlat0.x * u_xlat61;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat61 = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = u_xlat0.x * u_xlat61;
    u_xlat3.xyz = u_xlat0.xxx * _Specular_Color.xyz;
    u_xlat61 = (-Vector1_6269b1025b26473ca8bc61634f34b537) + _Specular_Smoothness;
    u_xlat61 = u_xlat0.x * u_xlat61 + Vector1_6269b1025b26473ca8bc61634f34b537;
    u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
    u_xlat62 = input.vs_INTERP9.y * _tunity_MatrixV[1].z;
    u_xlat62 = _tunity_MatrixV[0].z * input.vs_INTERP9.x + u_xlat62;
    u_xlat62 = _tunity_MatrixV[2].z * input.vs_INTERP9.z + u_xlat62;
    u_xlat62 = u_xlat62 + _tunity_MatrixV[3].z;
    u_xlat62 = (-u_xlat62) + (-_ProjectionParams.y);
    u_xlat62 = max(u_xlat62, 0.0);
    u_xlat62 = u_xlat62 * _pad976.x;
    u_xlat4.xyz = _g_texture(_Texture_t3, input.vs_INTERP0.xy, _GlobalMipBias.x).xyz;
    u_xlat5 = _g_texture(_Texture_t4, input.vs_INTERP0.xy, _GlobalMipBias.x);
    u_xlat5.xyz = u_xlat5.xyz + float3(-0.5, -0.5, -0.5);
    u_xlat63 = dot(u_xlat2.xyz, u_xlat5.xyz);
    u_xlat63 = u_xlat63 + 0.5;
    u_xlat4.xyz = float3(u_xlat63) * u_xlat4.xyz;
    u_xlat63 = max(u_xlat5.w, 9.99999975e-05);
    u_xlat4.xyz = u_xlat4.xyz / float3(u_xlat63);
    u_xlat63 = max(u_xlat3.y, u_xlat3.x);
    u_xlat63 = max(u_xlat3.z, u_xlat63);
    u_xlat64 = (-u_xlat63) + 1.0;
    u_xlat20.xyz = u_xlat20.xyz * float3(u_xlat64);
    u_xlat64 = (-u_xlat61) + 1.0;
    u_xlat5.x = u_xlat64 * u_xlat64;
    u_xlat5.x = max(u_xlat5.x, 0.0078125);
    u_xlat61 = u_xlat61 + u_xlat63;
    u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
    u_xlat63 = u_xlat5.x * 4.0 + 2.0;
    u_xlatb45.x = _pad176.y!=-1.0;
    if(u_xlatb45.x){
        u_xlat45.xy = input.vs_INTERP9.yy * _pad16.xy;
        u_xlat45.xy = _pad0.xy * input.vs_INTERP9.xx + u_xlat45.xy;
        u_xlat45.xy = _pad32.xy * input.vs_INTERP9.zz + u_xlat45.xy;
        u_xlat45.xy = u_xlat45.xy + _pad48.xy;
        u_xlat45.xy = u_xlat45.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
        u_xlat6 = _g_texture(_Texture_t5, u_xlat45.xy, _GlobalMipBias.x);
        u_xlatb45.xy = _g_equal(_pad176.yyyy, float4(0.0, 1.0, 0.0, 1.0)).xy;
        u_xlat65 = (u_xlatb45.y) ? u_xlat6.w : u_xlat6.x;
        u_xlat6.xyz = (u_xlatb45.x) ? u_xlat6.xyz : float3(u_xlat65);
    } else {
        u_xlat6.x = float(1.0);
        u_xlat6.y = float(1.0);
        u_xlat6.z = float(1.0);
    }
    u_xlat6.xyz = u_xlat6.xyz * _MainLightColor.xyz;
    u_xlat45.x = dot((-u_xlat1.xyz), u_xlat2.xyz);
    u_xlat45.x = u_xlat45.x + u_xlat45.x;
    u_xlat7.xyz = u_xlat2.xyz * (-u_xlat45.xxx) + (-u_xlat1.xyz);
    u_xlat45.x = dot(u_xlat2.xyz, u_xlat1.xyz);
    u_xlat45.x = clamp(u_xlat45.x, 0.0, 1.0);
    u_xlat5.z = (-u_xlat45.x) + 1.0;
    u_xlat25.xy = u_xlat5.xz * u_xlat5.xz;
    u_xlat45.x = u_xlat25.y * u_xlat25.y;
    u_xlat65 = (-u_xlat64) * 0.699999988 + 1.70000005;
    u_xlat64 = u_xlat64 * u_xlat65;
    u_xlat64 = u_xlat64 * 6.0;
    u_xlat8.xyz = unity_SpecCube0_BoxMax.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat9.xyz = u_xlat8.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube0_BoxMin.xyz;
    u_xlat10.xyz = (-u_xlat9.xyz) + input.vs_INTERP9.xyz;
    u_xlat11 = unity_SpecCube0_Rotation.zzxy + unity_SpecCube0_Rotation.zzxy;
    u_xlat12.xyz = u_xlat11.zwy * unity_SpecCube0_Rotation.xyz;
    u_xlat13 = u_xlat11 * unity_SpecCube0_Rotation.xyww;
    u_xlat65 = u_xlat11.y * unity_SpecCube0_Rotation.w;
    u_xlat12.xyz = u_xlat12.zzy + u_xlat12.yxx;
    u_xlat12.xyz = (-u_xlat12.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat14.xy = u_xlat10.xy * u_xlat12.xy;
    u_xlat66 = unity_SpecCube0_Rotation.x * u_xlat11.w + (-u_xlat65);
    u_xlat67 = u_xlat66 * u_xlat10.y + u_xlat14.x;
    u_xlat13.xy = u_xlat13.wz + u_xlat13.xy;
    u_xlat68 = u_xlat10.y * u_xlat13.y;
    u_xlat15.x = u_xlat13.x * u_xlat10.z + u_xlat67;
    u_xlat65 = unity_SpecCube0_Rotation.x * u_xlat11.w + u_xlat65;
    u_xlat67 = u_xlat65 * u_xlat10.x + u_xlat14.y;
    u_xlat30.xz = unity_SpecCube0_Rotation.yx * u_xlat11.yx + (-u_xlat13.zw);
    u_xlat15.y = u_xlat30.x * u_xlat10.z + u_xlat67;
    u_xlat67 = u_xlat30.z * u_xlat10.x + u_xlat68;
    u_xlat15.z = u_xlat12.z * u_xlat10.z + u_xlat67;
    u_xlat9.xyz = u_xlat9.xyz + u_xlat15.xyz;
    u_xlat11.xyz = unity_SpecCube1_BoxMax.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat14.xyz = u_xlat11.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube1_BoxMin.xyz;
    u_xlat15.xyz = (-u_xlat14.xyz) + input.vs_INTERP9.xyz;
    u_xlat16 = unity_SpecCube1_Rotation.zzxy + unity_SpecCube1_Rotation.zzxy;
    u_xlat17.xyz = u_xlat16.zwy * unity_SpecCube1_Rotation.xyz;
    u_xlat18 = u_xlat16 * unity_SpecCube1_Rotation.xyww;
    u_xlat67 = u_xlat16.y * unity_SpecCube1_Rotation.w;
    u_xlat17.xyz = u_xlat17.zzy + u_xlat17.yxx;
    u_xlat17.xyz = (-u_xlat17.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat10.xz = u_xlat15.xy * u_xlat17.xy;
    u_xlat68 = unity_SpecCube1_Rotation.x * u_xlat16.w + (-u_xlat67);
    u_xlat69 = u_xlat68 * u_xlat15.y + u_xlat10.x;
    u_xlat53.xy = u_xlat18.wz + u_xlat18.xy;
    u_xlat10.x = u_xlat15.y * u_xlat53.y;
    u_xlat19.x = u_xlat53.x * u_xlat15.z + u_xlat69;
    u_xlat67 = unity_SpecCube1_Rotation.x * u_xlat16.w + u_xlat67;
    u_xlat69 = u_xlat67 * u_xlat15.x + u_xlat10.z;
    u_xlat35.xz = unity_SpecCube1_Rotation.yx * u_xlat16.yx + (-u_xlat18.zw);
    u_xlat19.y = u_xlat35.x * u_xlat15.z + u_xlat69;
    u_xlat69 = u_xlat35.z * u_xlat15.x + u_xlat10.x;
    u_xlat19.z = u_xlat17.z * u_xlat15.z + u_xlat69;
    u_xlat14.xyz = u_xlat14.xyz + u_xlat19.xyz;
    u_xlat8.x = dot(u_xlat8.xyz, u_xlat8.xyz);
    u_xlat28 = dot(u_xlat11.xyz, u_xlat11.xyz);
    u_xlat8.x = (-u_xlat28) + u_xlat8.x;
    u_xlatb28 = 0.0<unity_SpecCube1_BoxMin.w;
    u_xlatb48 = unity_SpecCube1_BoxMin.w==0.0;
    u_xlatb69 = u_xlat8.x<-9.99999975e-05;
    u_xlatb69 = u_xlatb48 && u_xlatb69;
    u_xlatb28 = u_xlatb28 || u_xlatb69;
    u_xlatb69 = unity_SpecCube1_BoxMin.w<0.0;
    u_xlatb8 = 9.99999975e-05<u_xlat8.x;
    u_xlatb8 = u_xlatb8 && u_xlatb48;
    u_xlatb8 = u_xlatb8 || u_xlatb69;
    u_xlat11.xyz = u_xlat9.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat16.xyz = (-u_xlat9.xyz) + unity_SpecCube0_BoxMax.xyz;
    u_xlat11.xyz = min(u_xlat11.xyz, u_xlat16.xyz);
    u_xlat11.xyz = u_xlat11.xyz / unity_SpecCube0_BoxMax.www;
    u_xlat48 = min(u_xlat11.z, u_xlat11.y);
    u_xlat48 = min(u_xlat48, u_xlat11.x);
    u_xlat48 = clamp(u_xlat48, 0.0, 1.0);
    u_xlat11.xyz = u_xlat14.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat16.xyz = (-u_xlat14.xyz) + unity_SpecCube1_BoxMax.xyz;
    u_xlat11.xyz = min(u_xlat11.xyz, u_xlat16.xyz);
    u_xlat11.xyz = u_xlat11.xyz / unity_SpecCube1_BoxMax.www;
    u_xlat69 = min(u_xlat11.z, u_xlat11.y);
    u_xlat69 = min(u_xlat69, u_xlat11.x);
    u_xlat69 = clamp(u_xlat69, 0.0, 1.0);
    u_xlat10.x = (-u_xlat69) + 1.0;
    u_xlat10.x = min(u_xlat48, u_xlat10.x);
    u_xlat8.x = (u_xlatb8) ? u_xlat10.x : u_xlat48;
    u_xlat48 = (-u_xlat48) + 1.0;
    u_xlat48 = min(u_xlat48, u_xlat69);
    u_xlat8.y = (u_xlatb28) ? u_xlat48 : u_xlat69;
    u_xlat48 = u_xlat8.y + u_xlat8.x;
    u_xlat69 = max(u_xlat48, 1.0);
    u_xlat8.xy = u_xlat8.xy / float2(u_xlat69);
    u_xlatb69 = 0.00999999978<u_xlat8.x;
    if(u_xlatb69){
        u_xlat10.xz = u_xlat7.xy * u_xlat12.xy;
        u_xlat66 = u_xlat66 * u_xlat7.y + u_xlat10.x;
        u_xlat69 = u_xlat7.y * u_xlat13.y;
        u_xlat11.x = u_xlat13.x * u_xlat7.z + u_xlat66;
        u_xlat65 = u_xlat65 * u_xlat7.x + u_xlat10.z;
        u_xlat11.y = u_xlat30.x * u_xlat7.z + u_xlat65;
        u_xlat65 = u_xlat30.z * u_xlat7.x + u_xlat69;
        u_xlat11.z = u_xlat12.z * u_xlat7.z + u_xlat65;
        u_xlatb65 = 0.0<unity_SpecCube0_ProbePosition.w;
        u_xlatb10.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat11.xyzx).xyz;
        u_xlat10.x = (u_xlatb10.x) ? unity_SpecCube0_BoxMax.x : unity_SpecCube0_BoxMin.x;
        u_xlat10.y = (u_xlatb10.y) ? unity_SpecCube0_BoxMax.y : unity_SpecCube0_BoxMin.y;
        u_xlat10.z = (u_xlatb10.z) ? unity_SpecCube0_BoxMax.z : unity_SpecCube0_BoxMin.z;
        u_xlat10.xyz = (-u_xlat9.xyz) + u_xlat10.xyz;
        u_xlat10.xyz = u_xlat10.xyz / u_xlat11.xyz;
        u_xlat66 = min(u_xlat10.y, u_xlat10.x);
        u_xlat66 = min(u_xlat10.z, u_xlat66);
        u_xlat9.xyz = u_xlat9.xyz + (-unity_SpecCube0_ProbePosition.xyz);
        u_xlat9.xyz = u_xlat11.xyz * float3(u_xlat66) + u_xlat9.xyz;
        u_xlat9.xyz = (bool(u_xlatb65)) ? u_xlat9.xyz : u_xlat11.xyz;
        u_xlat10.xyz = unity_SpecCube0_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat11.xyz = u_xlat10.yzx * (-unity_SpecCube0_Rotation.xyz);
        u_xlat12.xyz = u_xlat10.xyz * unity_SpecCube0_Rotation.www;
        u_xlat11.xyz = u_xlat11.zzy + u_xlat11.yxx;
        u_xlat11.xyz = (-u_xlat11.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat30.xz = u_xlat9.xy * u_xlat11.xy;
        u_xlat65 = (-unity_SpecCube0_Rotation.x) * u_xlat10.z + (-u_xlat12.x);
        u_xlat65 = u_xlat65 * u_xlat9.y + u_xlat30.x;
        u_xlat11.xy = (-unity_SpecCube0_Rotation.xy) * u_xlat10.xx + u_xlat12.zy;
        u_xlat66 = u_xlat9.y * u_xlat11.y;
        u_xlat16.x = u_xlat11.x * u_xlat9.z + u_xlat65;
        u_xlat65 = (-unity_SpecCube0_Rotation.x) * u_xlat10.z + u_xlat12.x;
        u_xlat65 = u_xlat65 * u_xlat9.x + u_xlat30.z;
        u_xlat29.xz = (-unity_SpecCube0_Rotation.yx) * u_xlat10.xx + (-u_xlat12.yz);
        u_xlat16.y = u_xlat29.x * u_xlat9.z + u_xlat65;
        u_xlat65 = u_xlat29.z * u_xlat9.x + u_xlat66;
        u_xlat16.z = u_xlat11.z * u_xlat9.z + u_xlat65;
        u_xlat9 = _g_textureLod(_Texture_t1, u_xlat16.xyz, u_xlat64);
        u_xlat65 = u_xlat9.w + -1.0;
        u_xlat65 = unity_SpecCube0_HDR.w * u_xlat65 + 1.0;
        u_xlat65 = max(u_xlat65, 0.0);
        u_xlat65 = log2(u_xlat65);
        u_xlat65 = u_xlat65 * unity_SpecCube0_HDR.y;
        u_xlat65 = exp2(u_xlat65);
        u_xlat65 = u_xlat65 * unity_SpecCube0_HDR.x;
        u_xlat9.xyz = u_xlat9.xyz * float3(u_xlat65);
        u_xlat9.xyz = u_xlat8.xxx * u_xlat9.xyz;
    } else {
        u_xlat9.x = float(0.0);
        u_xlat9.y = float(0.0);
        u_xlat9.z = float(0.0);
    }
    u_xlatb65 = 0.00999999978<u_xlat8.y;
    if(u_xlatb65){
        u_xlat10.xy = u_xlat7.xy * u_xlat17.xy;
        u_xlat65 = u_xlat68 * u_xlat7.y + u_xlat10.x;
        u_xlat66 = u_xlat7.y * u_xlat53.y;
        u_xlat11.x = u_xlat53.x * u_xlat7.z + u_xlat65;
        u_xlat65 = u_xlat67 * u_xlat7.x + u_xlat10.y;
        u_xlat11.y = u_xlat35.x * u_xlat7.z + u_xlat65;
        u_xlat65 = u_xlat35.z * u_xlat7.x + u_xlat66;
        u_xlat11.z = u_xlat17.z * u_xlat7.z + u_xlat65;
        u_xlatb65 = 0.0<unity_SpecCube1_ProbePosition.w;
        u_xlatb10.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat11.xyzx).xyz;
        u_xlat10.x = (u_xlatb10.x) ? unity_SpecCube1_BoxMax.x : unity_SpecCube1_BoxMin.x;
        u_xlat10.y = (u_xlatb10.y) ? unity_SpecCube1_BoxMax.y : unity_SpecCube1_BoxMin.y;
        u_xlat10.z = (u_xlatb10.z) ? unity_SpecCube1_BoxMax.z : unity_SpecCube1_BoxMin.z;
        u_xlat10.xyz = (-u_xlat14.xyz) + u_xlat10.xyz;
        u_xlat10.xyz = u_xlat10.xyz / u_xlat11.xyz;
        u_xlat66 = min(u_xlat10.y, u_xlat10.x);
        u_xlat66 = min(u_xlat10.z, u_xlat66);
        u_xlat10.xyz = u_xlat14.xyz + (-unity_SpecCube1_ProbePosition.xyz);
        u_xlat10.xyz = u_xlat11.xyz * float3(u_xlat66) + u_xlat10.xyz;
        u_xlat10.xyz = (bool(u_xlatb65)) ? u_xlat10.xyz : u_xlat11.xyz;
        u_xlat11.xyz = unity_SpecCube1_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat12.xyz = u_xlat11.yzx * (-unity_SpecCube1_Rotation.xyz);
        u_xlat13.xyz = u_xlat11.xyz * unity_SpecCube1_Rotation.www;
        u_xlat12.xyz = u_xlat12.zzy + u_xlat12.yxx;
        u_xlat12.xyz = (-u_xlat12.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat8.xw = u_xlat10.xy * u_xlat12.xy;
        u_xlat65 = (-unity_SpecCube1_Rotation.x) * u_xlat11.z + (-u_xlat13.x);
        u_xlat65 = u_xlat65 * u_xlat10.y + u_xlat8.x;
        u_xlat31.xz = (-unity_SpecCube1_Rotation.xy) * u_xlat11.xx + u_xlat13.zy;
        u_xlat66 = u_xlat10.y * u_xlat31.z;
        u_xlat14.x = u_xlat31.x * u_xlat10.z + u_xlat65;
        u_xlat65 = (-unity_SpecCube1_Rotation.x) * u_xlat11.z + u_xlat13.x;
        u_xlat65 = u_xlat65 * u_xlat10.x + u_xlat8.w;
        u_xlat8.xw = (-unity_SpecCube1_Rotation.yx) * u_xlat11.xx + (-u_xlat13.yz);
        u_xlat14.y = u_xlat8.x * u_xlat10.z + u_xlat65;
        u_xlat65 = u_xlat8.w * u_xlat10.x + u_xlat66;
        u_xlat14.z = u_xlat12.z * u_xlat10.z + u_xlat65;
        u_xlat10 = _g_textureLod(_Texture_t2, u_xlat14.xyz, u_xlat64);
        u_xlat65 = u_xlat10.w + -1.0;
        u_xlat65 = unity_SpecCube1_HDR.w * u_xlat65 + 1.0;
        u_xlat65 = max(u_xlat65, 0.0);
        u_xlat65 = log2(u_xlat65);
        u_xlat65 = u_xlat65 * unity_SpecCube1_HDR.y;
        u_xlat65 = exp2(u_xlat65);
        u_xlat65 = u_xlat65 * unity_SpecCube1_HDR.x;
        u_xlat10.xyz = u_xlat10.xyz * float3(u_xlat65);
        u_xlat9.xyz = u_xlat8.yyy * u_xlat10.xyz + u_xlat9.xyz;
    }
    u_xlatb65 = u_xlat48<0.99000001;
    if(u_xlatb65){
        u_xlat7 = _g_textureLod(_Texture_t0, u_xlat7.xyz, u_xlat64);
        u_xlat64 = (-u_xlat48) + 1.0;
        u_xlat65 = u_xlat7.w + -1.0;
        u_xlat65 = _GlossyEnvironmentCubeMap_HDR.w * u_xlat65 + 1.0;
        u_xlat65 = max(u_xlat65, 0.0);
        u_xlat65 = log2(u_xlat65);
        u_xlat65 = u_xlat65 * _GlossyEnvironmentCubeMap_HDR.y;
        u_xlat65 = exp2(u_xlat65);
        u_xlat65 = u_xlat65 * _GlossyEnvironmentCubeMap_HDR.x;
        u_xlat7.xyz = u_xlat7.xyz * float3(u_xlat65);
        u_xlat9.xyz = float3(u_xlat64) * u_xlat7.xyz + u_xlat9.xyz;
    }
    u_xlat5.xw = u_xlat5.xx * u_xlat5.xx + float2(-1.0, 1.0);
    u_xlat64 = float(1.0) / u_xlat5.w;
    u_xlat7.xyz = (-u_xlat0.xxx) * _Specular_Color.xyz + float3(u_xlat61);
    u_xlat7.xyz = u_xlat45.xxx * u_xlat7.xyz + u_xlat3.xyz;
    u_xlat7.xyz = float3(u_xlat64) * u_xlat7.xyz;
    u_xlat7.xyz = u_xlat7.xyz * u_xlat9.xyz;
    u_xlat4.xyz = u_xlat4.xyz * u_xlat20.xyz + u_xlat7.xyz;
    u_xlati0 = int(uint(uint(_g_floatBitsToUint(_MainLightLayerMask)) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
    u_xlat61 = dot(u_xlat2.xyz, _MainLightPosition.xyz);
    u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
    u_xlat61 = u_xlat61 * unity_LightData.z;
    u_xlat6.xyz = float3(u_xlat61) * u_xlat6.xyz;
    u_xlat7.xyz = u_xlat1.xyz + _MainLightPosition.xyz;
    u_xlat61 = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat61 = max(u_xlat61, 1.17549435e-38);
    u_xlat61 = _g_inversesqrt(u_xlat61);
    u_xlat7.xyz = float3(u_xlat61) * u_xlat7.xyz;
    u_xlat61 = dot(u_xlat2.xyz, u_xlat7.xyz);
    u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
    u_xlat64 = dot(_MainLightPosition.xyz, u_xlat7.xyz);
    u_xlat64 = clamp(u_xlat64, 0.0, 1.0);
    u_xlat61 = u_xlat61 * u_xlat61;
    u_xlat61 = u_xlat61 * u_xlat5.x + 1.00001001;
    u_xlat64 = u_xlat64 * u_xlat64;
    u_xlat61 = u_xlat61 * u_xlat61;
    u_xlat64 = max(u_xlat64, 0.100000001);
    u_xlat61 = u_xlat61 * u_xlat64;
    u_xlat61 = u_xlat63 * u_xlat61;
    u_xlat61 = u_xlat25.x / u_xlat61;
    u_xlat7.xyz = u_xlat3.xyz * float3(u_xlat61) + u_xlat20.xyz;
    u_xlat6.xyz = u_xlat6.xyz * u_xlat7.xyz;
    u_xlat6.xyz = (int(u_xlati0) != 0) ? u_xlat6.xyz : float3(0.0, 0.0, 0.0);
    u_xlat0.x = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu0 =  uint(int(u_xlat0.x));
    u_xlatb45.xy = _g_equal(_pad176.zzzz, float4(0.0, 1.0, 0.0, 1.0)).xy;
    u_xlat7.x = float(0.0);
    u_xlat7.y = float(0.0);
    u_xlat7.z = float(0.0);
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<u_xlatu0 ; u_xlatu_loop_1++)
    {
        u_xlatu64 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati66 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat64 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati66]);
        u_xlatu64 =  uint(int(u_xlat64));
        u_xlat8.xyz = (-input.vs_INTERP9.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat66 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat66 = max(u_xlat66, 6.10351562e-05);
        u_xlat67 = _g_inversesqrt(u_xlat66);
        u_xlat9.xyz = float3(u_xlat67) * u_xlat8.xyz;
        u_xlat68 = float(1.0) / float(u_xlat66);
        u_xlat66 = u_xlat66 * _AdditionalLightsAttenuation.x;
        u_xlat66 = (-u_xlat66) * u_xlat66 + 1.0;
        u_xlat66 = max(u_xlat66, 0.0);
        u_xlat66 = u_xlat66 * u_xlat66;
        u_xlat66 = u_xlat66 * u_xlat68;
        u_xlat68 = dot(_AdditionalLightsSpotDir.xyz, u_xlat9.xyz);
        u_xlat68 = u_xlat68 * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat68 = clamp(u_xlat68, 0.0, 1.0);
        u_xlat68 = u_xlat68 * u_xlat68;
        u_xlat66 = u_xlat66 * u_xlat68;
        u_xlatu68 = uint(u_xlatu64 >> 5u);
        u_xlati69 = int(1 << int(u_xlatu64));
        u_xlati68 = int(uint(uint(u_xlati69) & uint(_g_floatBitsToUint(_pad64.x))));
        if(u_xlati68 != 0) {
            u_xlati68 = int(_pad20672.x);
            u_xlati69 = (u_xlati68 != 0) ? 0 : 1;
            u_xlati10 = int(int(u_xlatu64) << 2);
            if(u_xlati69 != 0) {
                u_xlat30.xyz = input.vs_INTERP9.yyy * _pad208.xyw;
                u_xlat30.xyz = _pad192.xyw * input.vs_INTERP9.xxx + u_xlat30.xyz;
                u_xlat30.xyz = _pad224.xyw * input.vs_INTERP9.zzz + u_xlat30.xyz;
                u_xlat30.xyz = u_xlat30.xyz + _pad240.xyw;
                u_xlat30.xy = u_xlat30.xy / u_xlat30.zz;
                u_xlat30.xy = u_xlat30.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                u_xlat30.xy = clamp(u_xlat30.xy, 0.0, 1.0);
                u_xlat30.xy = _pad16576.xy * u_xlat30.xy + _pad16576.zw;
            } else {
                u_xlatb68 = u_xlati68==1;
                u_xlati68 = u_xlatb68 ? 1 : int(0);
                if(u_xlati68 != 0) {
                    u_xlat11.xy = input.vs_INTERP9.yy * _pad208.xy;
                    u_xlat11.xy = _pad192.xy * input.vs_INTERP9.xx + u_xlat11.xy;
                    u_xlat11.xy = _pad224.xy * input.vs_INTERP9.zz + u_xlat11.xy;
                    u_xlat11.xy = u_xlat11.xy + _pad240.xy;
                    u_xlat11.xy = u_xlat11.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat11.xy = _g_fract(u_xlat11.xy);
                    u_xlat30.xy = _pad16576.xy * u_xlat11.xy + _pad16576.zw;
                } else {
                    u_xlat11 = input.vs_INTERP9.yyyy * _pad208;
                    u_xlat11 = _pad192 * input.vs_INTERP9.xxxx + u_xlat11;
                    u_xlat11 = _pad224 * input.vs_INTERP9.zzzz + u_xlat11;
                    u_xlat11 = u_xlat11 + _pad240;
                    u_xlat11.xyz = u_xlat11.xyz / u_xlat11.www;
                    u_xlat68 = dot(u_xlat11.xyz, u_xlat11.xyz);
                    u_xlat68 = _g_inversesqrt(u_xlat68);
                    u_xlat11.xyz = float3(u_xlat68) * u_xlat11.xyz;
                    u_xlat68 = dot(abs(u_xlat11.xyz), float3(1.0, 1.0, 1.0));
                    u_xlat68 = max(u_xlat68, 9.99999997e-07);
                    u_xlat68 = float(1.0) / float(u_xlat68);
                    u_xlat12.xyz = float3(u_xlat68) * u_xlat11.zxy;
                    u_xlat12.x = (-u_xlat12.x);
                    u_xlat12.x = clamp(u_xlat12.x, 0.0, 1.0);
                    u_xlatb10.xw = _g_greaterThanEqual(u_xlat12.yyyz, float4(0.0, 0.0, 0.0, 0.0)).xw;
                    u_xlat10.x = (u_xlatb10.x) ? u_xlat12.x : (-u_xlat12.x);
                    u_xlat10.w = (u_xlatb10.w) ? u_xlat12.x : (-u_xlat12.x);
                    u_xlat10.xw = u_xlat11.xy * float2(u_xlat68) + u_xlat10.xw;
                    u_xlat10.xw = u_xlat10.xw * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat10.xw = clamp(u_xlat10.xw, 0.0, 1.0);
                    u_xlat30.xy = _pad16576.xy * u_xlat10.xw + _pad16576.zw;
                }
            }
            u_xlat10 = _g_textureLod(_Texture_t6, u_xlat30.xy, 0.0);
            u_xlat68 = (u_xlatb45.y) ? u_xlat10.w : u_xlat10.x;
            u_xlat10.xyz = (u_xlatb45.x) ? u_xlat10.xyz : float3(u_xlat68);
        } else {
            u_xlat10.x = float(1.0);
            u_xlat10.y = float(1.0);
            u_xlat10.z = float(1.0);
        }
        u_xlat10.xyz = u_xlat10.xyz * _AdditionalLightsColor.xyz;
        u_xlati64 = int(uint(uint(_g_floatBitsToUint(unity_RenderingLayer.x)) & uint(_g_floatBitsToUint(_AdditionalLightsLayerMasks))));
        u_xlat68 = dot(u_xlat2.xyz, u_xlat9.xyz);
        u_xlat68 = clamp(u_xlat68, 0.0, 1.0);
        u_xlat66 = u_xlat66 * u_xlat68;
        u_xlat10.xyz = float3(u_xlat66) * u_xlat10.xyz;
        u_xlat8.xyz = u_xlat8.xyz * float3(u_xlat67) + u_xlat1.xyz;
        u_xlat66 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat66 = max(u_xlat66, 1.17549435e-38);
        u_xlat66 = _g_inversesqrt(u_xlat66);
        u_xlat8.xyz = float3(u_xlat66) * u_xlat8.xyz;
        u_xlat66 = dot(u_xlat2.xyz, u_xlat8.xyz);
        u_xlat66 = clamp(u_xlat66, 0.0, 1.0);
        u_xlat67 = dot(u_xlat9.xyz, u_xlat8.xyz);
        u_xlat67 = clamp(u_xlat67, 0.0, 1.0);
        u_xlat66 = u_xlat66 * u_xlat66;
        u_xlat66 = u_xlat66 * u_xlat5.x + 1.00001001;
        u_xlat67 = u_xlat67 * u_xlat67;
        u_xlat66 = u_xlat66 * u_xlat66;
        u_xlat67 = max(u_xlat67, 0.100000001);
        u_xlat66 = u_xlat66 * u_xlat67;
        u_xlat66 = u_xlat63 * u_xlat66;
        u_xlat66 = u_xlat25.x / u_xlat66;
        u_xlat8.xyz = u_xlat3.xyz * float3(u_xlat66) + u_xlat20.xyz;
        u_xlat8.xyz = u_xlat8.xyz * u_xlat10.xyz + u_xlat7.xyz;
        u_xlat7.xyz = (int(u_xlati64) != 0) ? u_xlat8.xyz : u_xlat7.xyz;
    }
    u_xlat0.xyz = u_xlat4.xyz + u_xlat6.xyz;
    u_xlat0.xyz = u_xlat7.xyz + u_xlat0.xyz;
    u_xlat60 = u_xlat62 * (-u_xlat62);
    u_xlat60 = exp2(u_xlat60);
    u_xlat1.x = (-u_xlat60) + 1.0;
    u_xlat1.xyz = u_xlat1.xxx * _pad992.xyz;
    __SV_Target0.xyz = u_xlat0.xyz * float3(u_xlat60) + u_xlat1.xyz;
    __SV_Target1 = uint(uint(_g_floatBitsToUint(_pad176.x)) & uint(_g_floatBitsToUint(unity_RenderingLayer.x)));
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
