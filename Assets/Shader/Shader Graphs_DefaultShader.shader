Shader "Shader Graphs/DefaultShader"
{
    Properties
    {



[NoScaleOffset] _Colors ("Colors", 2D) = "white" {}
_Emission ("Emission", Float) = 0
_MetallicAngleSmoothStep ("MetallicAngleSmoothStep", Vector) = (0.4,0.75,0,0)
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
_MetallicMetallicness ("MetallicMetallicness", Range(0, 1)) = 0
_MetallicSmoothness ("MetallicSmoothness", Range(0, 1)) = 0
_PlasticMetallicness ("PlasticMetallicness", Range(0, 1)) = 0
_PlasticSmoothness ("PlasticSmoothness", Range(0, 1)) = 0
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
            float4 _pad80;
            float4 _pad96;
            float4 _pad128;
            float4 _pad176;
            float4 _pad192;
            float4 _pad208;
            float4 _pad224;
            float4 _pad240;
            float4 _pad320;
            float4 _pad336;
            float4 _pad352;
            float4 _pad368;
            float4 _pad384;
            float4 _pad400;
            float4 _pad416;
            float4 _pad432;
            float4 _pad448;
            float4 _pad976;
            float4 _pad992;
            float4x4 unity_MatrixVP;
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
            float _MetallicSmoothness;
            float2 _Color_Offset_2;
            float _PlasticSmoothness;
            float2 _Skin_Smooth_Step;
            float _Use_Skin;
            float _Rainbow_Skin;
            float _Metallic_Cutoff;
            float _MetallicMetallicness;
            float _PlasticMetallicness;
            float _UV_Rotation;

            float4 u_xlat0;
            int u_xlati0;
            float4 u_xlat1;
            int u_xlati2;
            float4 ImmCB_0_0_0[4];
            bool u_xlatb1;
            float3 u_xlat2;
            float4 u_xlat3;
            float3 u_xlat4;
            float4 u_xlat5;
            bool4 u_xlatb5;
            float4 u_xlat6;
            float4 u_xlat7;
            int2 u_xlati7;
            uint2 u_xlatu7;
            float4 u_xlat8;
            bool2 u_xlatb8;
            float4 u_xlat9;
            bool u_xlatb9;
            float4 u_xlat10;
            float4 u_xlat11;
            bool3 u_xlatb11;
            float4 u_xlat12;
            float4 u_xlat13;
            float4 u_xlat14;
            float4 u_xlat15;
            float4 u_xlat16;
            float4 u_xlat17;
            float4 u_xlat18;
            float4 u_xlat19;
            float4 u_xlat20;
            float4 u_xlat21;
            float2 u_xlat22;
            bool u_xlatb22;
            float u_xlat23;
            float u_xlat27;
            bool u_xlatb27;
            float3 u_xlat31;
            bool u_xlatb31;
            float3 u_xlat32;
            float3 u_xlat33;
            float3 u_xlat34;
            float3 u_xlat38;
            float u_xlat44;
            int u_xlati44;
            uint u_xlatu44;
            bool u_xlatb44;
            float2 u_xlat47;
            float2 u_xlat49;
            bool u_xlatb49;
            float2 u_xlat50;
            float2 u_xlat52;
            int u_xlati52;
            bool2 u_xlatb52;
            float u_xlat53;
            bool u_xlatb53;
            float2 u_xlat54;
            float2 u_xlat55;
            float2 u_xlat56;
            float2 u_xlat58;
            float2 u_xlat60;
            float u_xlat66;
            int u_xlati66;
            uint u_xlatu66;
            bool u_xlatb66;
            float u_xlat67;
            int u_xlati67;
            uint u_xlatu67;
            bool u_xlatb67;
            float u_xlat68;
            int u_xlati68;
            bool u_xlatb68;
            float u_xlat69;
            int u_xlati69;
            uint u_xlatu69;
            bool u_xlatb69;
            float u_xlat70;
            int u_xlati70;
            uint u_xlatu70;
            bool u_xlatb70;
            float u_xlat71;
            int u_xlati71;
            bool u_xlatb71;
            float u_xlat72;
            bool u_xlatb72;
            float u_xlat73;
            int u_xlati73;
            uint u_xlatu73;
            bool u_xlatb73;
            float u_xlat74;
            int u_xlati74;
            float u_xlat75;
            float u_xlat76;
            bool u_xlatb76;
            uint _g_gl_InstanceID : SV_InstanceID;



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
                nointerpolation uint vs_CUSTOM_INSTANCE_ID0 : TEXCOORD0;
                float2 vs_INTERP0 : TEXCOORD1;
                float4 vs_INTERP10 : TEXCOORD2;
                float3 vs_INTERP11 : TEXCOORD3;
                float3 vs_INTERP12 : TEXCOORD4;
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


    u_xlati0 = _g_gl_InstanceID + _g_floatBitsToInt(_pad0.x);
    u_xlati2 = u_xlati0 * 9;
    u_xlati0 = int(u_xlati0 << 1);
    output.vs_INTERP0.xy = input.in_TEXCOORD1.xy * _pad0.xy + _pad0.zw;
    u_xlat0.xzw = input.in_POSITION0.yyy * _pad16.xyz;
    u_xlat0.xzw = _pad0.xyz * input.in_POSITION0.xxx + u_xlat0.xzw;
    u_xlat0.xzw = _pad32.xyz * input.in_POSITION0.zzz + u_xlat0.xzw;
    u_xlat0.xzw = u_xlat0.xzw + _pad48.xyz;
    u_xlat1 = u_xlat0.zzzz * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.wwww + u_xlat1;
    output.vs_INTERP11.xyz = u_xlat0.xzw;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    u_xlat0.xzw = input.in_TANGENT0.yyy * _pad16.xyz;
    u_xlat0.xzw = _pad0.xyz * input.in_TANGENT0.xxx + u_xlat0.xzw;
    u_xlat0.xzw = _pad32.xyz * input.in_TANGENT0.zzz + u_xlat0.xzw;
    u_xlat1.x = dot(u_xlat0.xzw, u_xlat0.xzw);
    u_xlat1.x = max(u_xlat1.x, 1.17549435e-38);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    output.vs_INTERP5.xyz = u_xlat0.xzw * u_xlat1.xxx;
    output.vs_INTERP5.w = input.in_TANGENT0.w;
    output.vs_INTERP6 = input.in_TEXCOORD0;
    output.vs_INTERP7 = input.in_TEXCOORD1;
    output.vs_INTERP8 = input.in_TEXCOORD2;
    output.vs_INTERP9 = input.in_TEXCOORD3;
    output.vs_INTERP10 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat1.x = dot(input.in_NORMAL0.xyz, _pad64.xyz);
    u_xlat1.y = dot(input.in_NORMAL0.xyz, _pad80.xyz);
    u_xlat1.z = dot(input.in_NORMAL0.xyz, _pad96.xyz);
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    output.vs_INTERP12.xyz = u_xlat0.xxx * u_xlat1.xyz;
    output.vs_CUSTOM_INSTANCE_ID0 =  uint(_g_gl_InstanceID);
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_0_0[3] = float4(0.0, 0.0, 0.0, 1.0);
    u_xlati0 = int(input.vs_CUSTOM_INSTANCE_ID0) + _g_floatBitsToInt(_pad0.x);
    u_xlat22.x = dot(input.vs_INTERP12.xyz, input.vs_INTERP12.xyz);
    u_xlat22.x = sqrt(u_xlat22.x);
    u_xlat22.x = float(1.0) / u_xlat22.x;
    u_xlat1.xyz = u_xlat22.xxx * input.vs_INTERP12.xyz;
    u_xlat44 = input.vs_INTERP7.y + input.vs_INTERP7.x;
    u_xlatb44 = u_xlat44>=1.79999995;
    u_xlat2.xyz = _g_texture(_Texture_t8, input.vs_INTERP6.xy, _GlobalMipBias.x).xyz;
    u_xlat3.xy = input.vs_INTERP6.xy + float2(_Color_Offset.x, _Color_Offset.y);
    u_xlat4.xyz = _g_texture(_Texture_t8, u_xlat3.xy, _GlobalMipBias.x).xyz;
    u_xlat3.xy = u_xlat3.xy + _Color_Offset_2.xy;
    u_xlat66 = _UV_Rotation * 0.0174532924;
    u_xlat47.xy = input.vs_INTERP9.xy + float2(-0.5, -0.5);
    u_xlat5.x = sin(u_xlat66);
    u_xlat6.x = cos(u_xlat66);
    u_xlat7.x = (-u_xlat5.x);
    u_xlat7.y = u_xlat6.x;
    u_xlat6.y = dot(u_xlat47.xy, u_xlat7.xy);
    u_xlat7.z = u_xlat5.x;
    u_xlat6.x = dot(u_xlat47.xy, u_xlat7.yz);
    u_xlat47.xy = u_xlat6.xy + float2(0.5, 0.5);
    u_xlat47.xy = u_xlat47.xy * _Skin_UV_Scale.xy;
    u_xlat47.xy = u_xlat47.xy * float2(float2(_Skin_Noise_Scale, _Skin_Noise_Scale));
    u_xlat5.xy = floor(u_xlat47.xy);
    u_xlat47.xy = _g_fract(u_xlat47.xy);
    u_xlat49.x = float(0.0);
    u_xlat49.y = float(8.0);
    for(int u_xlati_loop_1 = int(0xFFFFFFFFu) ; u_xlati_loop_1<=1 ; u_xlati_loop_1++)
    {
        u_xlat6.y = float(u_xlati_loop_1);
        u_xlat50.xy = u_xlat49.xy;
        for(int u_xlati_loop_2 = int(0xFFFFFFFFu) ; u_xlati_loop_2<=1 ; u_xlati_loop_2++)
        {
            u_xlat6.x = float(u_xlati_loop_2);
            u_xlat7.xy = u_xlat5.xy + u_xlat6.xy;
            u_xlati7.xy = int2(u_xlat7.xy);
            u_xlati68 = int(uint(uint(u_xlati7.y) ^ 1103515245u));
            u_xlati70 = u_xlati68 + u_xlati7.x;
            u_xlatu70 = uint(u_xlati68) * uint(u_xlati70);
            u_xlatu7.x = uint(u_xlatu70 >> 5u);
            u_xlati70 = int(uint(u_xlatu70 ^ u_xlatu7.x));
            u_xlatu7.y = uint(u_xlati70) * 668265261u;
            u_xlati70 = int(int(u_xlatu7.y) << 3);
            u_xlatu7.x = uint(uint(u_xlati68) ^ uint(u_xlati70));
            u_xlatu7.xy = uint2(u_xlatu7.x >> uint(8u), u_xlatu7.y >> uint(8u));
            u_xlat7.xy = float2(u_xlatu7.xy);
            u_xlat7.xy = u_xlat7.xy * float2(1.19209304e-07, 1.19209304e-07);
            u_xlat8.x = sin(u_xlat7.x);
            u_xlat8.y = cos(u_xlat7.y);
            u_xlat7.xy = u_xlat8.xy * float2(0.5, 0.5) + u_xlat6.xy;
            u_xlat7.xy = (-u_xlat47.xy) + u_xlat7.xy;
            u_xlat7.xy = u_xlat7.xy + float2(0.5, 0.5);
            u_xlat68 = dot(u_xlat7.xy, u_xlat7.xy);
            u_xlat68 = sqrt(u_xlat68);
            u_xlatb70 = u_xlat68<u_xlat50.y;
            u_xlat50.xy = (bool(u_xlatb70)) ? float2(u_xlat68) : u_xlat50.xy;
        }
        u_xlat49.xy = u_xlat50.xy;
    }
    u_xlat66 = (-_Skin_Smooth_Step.x) + _Skin_Smooth_Step.y;
    u_xlat67 = u_xlat49.x + (-_Skin_Smooth_Step.x);
    u_xlat66 = float(1.0) / u_xlat66;
    u_xlat66 = u_xlat66 * u_xlat67;
    u_xlat66 = clamp(u_xlat66, 0.0, 1.0);
    u_xlat67 = u_xlat66 * -2.0 + 3.0;
    u_xlat66 = u_xlat66 * u_xlat66;
    u_xlat66 = u_xlat66 * u_xlat67;
    u_xlatb67 = float4(0.0, 0.0, 0.0, 0.0)!=float4(_Rainbow_Skin);
    u_xlat67 = (u_xlatb67) ? u_xlat66 : 1.0;
    u_xlat3.xy = float2(u_xlat67) * u_xlat3.xy;
    u_xlat3.xyz = _g_texture(_Texture_t8, u_xlat3.xy, _GlobalMipBias.x).xyz;
    u_xlatb67 = float4(0.0, 0.0, 0.0, 0.0)!=float4(_Use_Skin);
    u_xlat67 = u_xlatb67 ? 1.0 : float(0.0);
    u_xlat66 = u_xlat66 * u_xlat67;
    u_xlat3.xyz = (-u_xlat4.xyz) + u_xlat3.xyz;
    u_xlat3.xyz = float3(u_xlat66) * u_xlat3.xyz + u_xlat4.xyz;
    u_xlat2.xyz = (bool(u_xlatb44)) ? u_xlat2.xyz : u_xlat3.xyz;
    u_xlat66 = _Cookness;
    u_xlat66 = clamp(u_xlat66, 0.0, 1.0);
    u_xlat3.xyz = (-u_xlat2.xyz) + _CookColor.xyz;
    u_xlat2.xyz = float3(u_xlat66) * u_xlat3.xyz + u_xlat2.xyz;
    u_xlat67 = _Cookness + -1.0;
    u_xlat67 = clamp(u_xlat67, 0.0, 1.0);
    u_xlat3.xyz = (-u_xlat2.xyz) + _BurntColor.xyz;
    u_xlat2.xyz = float3(u_xlat67) * u_xlat3.xyz + u_xlat2.xyz;
    u_xlat3.xy = input.vs_INTERP9.xy * float2(float2(_Normal_Scale, _Normal_Scale));
    u_xlat3.xyw = _g_texture(_Texture_t9, u_xlat3.xy, _GlobalMipBias.x).xyw;
    u_xlat3.x = u_xlat3.x * u_xlat3.w;
    u_xlat3.xy = u_xlat3.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat67 = dot(u_xlat3.xy, u_xlat3.xy);
    u_xlat67 = min(u_xlat67, 1.0);
    u_xlat67 = (-u_xlat67) + 1.0;
    u_xlat67 = sqrt(u_xlat67);
    u_xlat3.z = max(u_xlat67, 1.00000002e-16);
    u_xlat67 = dot(u_xlat3, u_xlat3);
    u_xlat67 = _g_inversesqrt(u_xlat67);
    u_xlat3.xyz = float3(u_xlat67) * u_xlat3.xyz;
    u_xlat4.xy = u_xlat22.xx * input.vs_INTERP12.xy + u_xlat3.xy;
    u_xlat4.z = u_xlat1.z * u_xlat3.z;
    u_xlat22.x = dot(u_xlat4.xyz, u_xlat4.xyz);
    u_xlat22.x = max(u_xlat22.x, 1.17549435e-38);
    u_xlat22.x = _g_inversesqrt(u_xlat22.x);
    u_xlat67 = (-_Normal_Strength) + _Cooked_Normal_Strength;
    u_xlat66 = u_xlat66 * u_xlat67 + _Normal_Strength;
    u_xlat3.xyz = u_xlat4.xyz * u_xlat22.xxx + (-u_xlat1.xyz);
    u_xlat3.xyz = float3(u_xlat66) * u_xlat3.xyz + u_xlat1.xyz;
    u_xlat22.x = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat22.x = _g_inversesqrt(u_xlat22.x);
    u_xlat3.xyz = u_xlat22.xxx * u_xlat3.xyz;
    u_xlat4.xyz = _g_texture(_Texture_t8, input.vs_INTERP8.xy, _GlobalMipBias.x).xyz;
    u_xlatb22 = input.vs_INTERP7.x>=_Metallic_Cutoff;
    u_xlat22.x = (u_xlatb22) ? _MetallicMetallicness : _PlasticMetallicness;
    u_xlatb66 = 0.00999999978<u_xlat22.x;
    u_xlat5.xyz = input.vs_INTERP11.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat67 = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlat68 = _g_inversesqrt(u_xlat67);
    u_xlat5.xyz = float3(u_xlat68) * u_xlat5.xyz;
    u_xlat1.x = dot(u_xlat1.xyz, u_xlat5.xyz);
    u_xlat1.x = abs(u_xlat1.x) + -0.400000006;
    u_xlat1.x = u_xlat1.x * 4.99999952;
    u_xlat1.x = clamp(u_xlat1.x, 0.0, 1.0);
    u_xlat23 = u_xlat1.x * -2.0 + 3.0;
    u_xlat1.x = u_xlat1.x * u_xlat1.x;
    u_xlat1.x = u_xlat1.x * u_xlat23;
    u_xlat22.x = u_xlat22.x * u_xlat1.x;
    u_xlat22.x = (u_xlatb66) ? u_xlat22.x : input.vs_INTERP7.x;
    u_xlat22.x = u_xlat22.x + (-_Cookness);
    u_xlat22.x = clamp(u_xlat22.x, 0.0, 1.0);
    u_xlat22.x = (u_xlatb44) ? 1.0 : u_xlat22.x;
    u_xlatb66 = input.vs_INTERP7.x>=0.899999976;
    u_xlat66 = (u_xlatb66) ? _MetallicSmoothness : _PlasticSmoothness;
    u_xlatb1 = 0.00999999978<u_xlat66;
    u_xlat23 = max(input.vs_INTERP7.y, 0.0);
    u_xlat23 = min(u_xlat23, 0.699999988);
    u_xlat66 = (u_xlatb1) ? u_xlat66 : u_xlat23;
    u_xlat66 = u_xlat66 + (-_Cookness);
    u_xlat66 = max(u_xlat66, 0.0);
    u_xlat66 = min(u_xlat66, 0.699999988);
    u_xlat44 = (u_xlatb44) ? 0.699999988 : u_xlat66;
    u_xlatb66 = unity_OrthoParams.w==0.0;
    u_xlat1.xyz = (-input.vs_INTERP11.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat68 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat68 = _g_inversesqrt(u_xlat68);
    u_xlat1.xyz = u_xlat1.xyz * float3(u_xlat68);
    u_xlat5.x = _tunity_MatrixV[0].z;
    u_xlat5.y = _tunity_MatrixV[1].z;
    u_xlat5.z = _tunity_MatrixV[2].z;
    u_xlat1.xyz = (bool(u_xlatb66)) ? u_xlat1.xyz : u_xlat5.xyz;
    u_xlat5.xyz = input.vs_INTERP11.xyz + (-_pad320.xyz);
    u_xlat6.xyz = input.vs_INTERP11.xyz + (-_pad336.xyz);
    u_xlat7.xyz = input.vs_INTERP11.xyz + (-_pad352.xyz);
    u_xlat8.xyz = input.vs_INTERP11.xyz + (-_pad368.xyz);
    u_xlat5.x = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlat5.y = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat5.z = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat5.w = dot(u_xlat8.xyz, u_xlat8.xyz);
    u_xlatb5 = _g_lessThan(u_xlat5, _pad384);
    u_xlat6.x = u_xlatb5.x ? float(1.0) : 0.0;
    u_xlat6.y = u_xlatb5.y ? float(1.0) : 0.0;
    u_xlat6.z = u_xlatb5.z ? float(1.0) : 0.0;
    u_xlat6.w = u_xlatb5.w ? float(1.0) : 0.0;
;
    u_xlat5.x = (u_xlatb5.x) ? float(-1.0) : float(-0.0);
    u_xlat5.y = (u_xlatb5.y) ? float(-1.0) : float(-0.0);
    u_xlat5.z = (u_xlatb5.z) ? float(-1.0) : float(-0.0);
    u_xlat5.xyz = u_xlat5.xyz + u_xlat6.yzw;
    u_xlat6.yzw = max(u_xlat5.xyz, float3(0.0, 0.0, 0.0));
    u_xlat66 = dot(u_xlat6, float4(4.0, 3.0, 2.0, 1.0));
    u_xlat66 = (-u_xlat66) + 4.0;
    u_xlatu66 = uint(u_xlat66);
    u_xlati66 = int(int(u_xlatu66) << 2);
    u_xlat5.xyz = input.vs_INTERP11.yyy * _pad16.xyz;
    u_xlat5.xyz = _pad0.xyz * input.vs_INTERP11.xxx + u_xlat5.xyz;
    u_xlat5.xyz = _pad32.xyz * input.vs_INTERP11.zzz + u_xlat5.xyz;
    u_xlat5.xyz = u_xlat5.xyz + _pad48.xyz;
    u_xlat66 = input.vs_INTERP11.y * _tunity_MatrixV[1].z;
    u_xlat66 = _tunity_MatrixV[0].z * input.vs_INTERP11.x + u_xlat66;
    u_xlat66 = _tunity_MatrixV[2].z * input.vs_INTERP11.z + u_xlat66;
    u_xlat66 = u_xlat66 + _tunity_MatrixV[3].z;
    u_xlat66 = (-u_xlat66) + (-_pad352.y);
    u_xlat66 = max(u_xlat66, 0.0);
    u_xlat66 = u_xlat66 * _pad976.x;
    u_xlat6.xyz = _g_texture(_Colors, input.vs_INTERP0.xy, _GlobalMipBias.x).xyz;
    u_xlat7 = _g_texture(_Normal_Map, input.vs_INTERP0.xy, _GlobalMipBias.x);
    u_xlat7.xyz = u_xlat7.xyz + float3(-0.5, -0.5, -0.5);
    u_xlat68 = dot(u_xlat3.xyz, u_xlat7.xyz);
    u_xlat68 = u_xlat68 + 0.5;
    u_xlat6.xyz = float3(u_xlat68) * u_xlat6.xyz;
    u_xlat68 = max(u_xlat7.w, 9.99999975e-05);
    u_xlat6.xyz = u_xlat6.xyz / float3(u_xlat68);
    u_xlat68 = (-u_xlat22.x) * 0.959999979 + 0.959999979;
    u_xlat69 = u_xlat44 + (-u_xlat68);
    u_xlat7.xyz = float3(u_xlat68) * u_xlat2.xyz;
    u_xlat2.xyz = u_xlat2.xyz + float3(-0.0399999991, -0.0399999991, -0.0399999991);
    u_xlat2.xyz = u_xlat22.xxx * u_xlat2.xyz + float3(0.0399999991, 0.0399999991, 0.0399999991);
    u_xlat22.x = (-u_xlat44) + 1.0;
    u_xlat44 = u_xlat22.x * u_xlat22.x;
    u_xlat68 = u_xlat44 * u_xlat44;
    u_xlat69 = u_xlat69 + 1.0;
    u_xlat69 = min(u_xlat69, 1.0);
    u_xlat70 = u_xlat44 * 4.0 + 2.0;
    u_xlati0 = u_xlati0 * 9;
    u_xlatb71 = 0.0<_pad432.y;
    if(u_xlatb71){
        u_xlatb71 = _pad432.y==1.0;
        if(u_xlatb71){
            u_xlat8 = u_xlat5.xyxy + _pad400;
            float3 txVec0 = float3(u_xlat8.xy,u_xlat5.z);
            u_xlat9.x = _g_textureLod(_Texture_t5, txVec0, 0.0);
            float3 txVec1 = float3(u_xlat8.zw,u_xlat5.z);
            u_xlat9.y = _g_textureLod(_Texture_t5, txVec1, 0.0);
            u_xlat8 = u_xlat5.xyxy + _pad416;
            float3 txVec2 = float3(u_xlat8.xy,u_xlat5.z);
            u_xlat9.z = _g_textureLod(_Texture_t5, txVec2, 0.0);
            float3 txVec3 = float3(u_xlat8.zw,u_xlat5.z);
            u_xlat9.w = _g_textureLod(_Texture_t5, txVec3, 0.0);
            u_xlat71 = dot(u_xlat9, float4(0.25, 0.25, 0.25, 0.25));
        } else {
            u_xlatb72 = _pad432.y==2.0;
            if(u_xlatb72){
                u_xlat8.xy = u_xlat5.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat8.xy = floor(u_xlat8.xy);
                u_xlat52.xy = u_xlat5.xy * _pad448.zw + (-u_xlat8.xy);
                u_xlat9 = u_xlat52.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat10 = u_xlat9.xxzz * u_xlat9.xxzz;
                u_xlat9.xz = u_xlat10.yw * float2(0.0799999982, 0.0799999982);
                u_xlat10.xy = u_xlat10.xz * float2(0.5, 0.5) + (-u_xlat52.xy);
                u_xlat54.xy = (-u_xlat52.xy) + float2(1.0, 1.0);
                u_xlat11.xy = min(u_xlat52.xy, float2(0.0, 0.0));
                u_xlat11.xy = (-u_xlat11.xy) * u_xlat11.xy + u_xlat54.xy;
                u_xlat52.xy = max(u_xlat52.xy, float2(0.0, 0.0));
                u_xlat52.xy = (-u_xlat52.xy) * u_xlat52.xy + u_xlat9.yw;
                u_xlat11.xy = u_xlat11.xy + float2(1.0, 1.0);
                u_xlat52.xy = u_xlat52.xy + float2(1.0, 1.0);
                u_xlat12.xy = u_xlat10.xy * float2(0.159999996, 0.159999996);
                u_xlat10.xy = u_xlat54.xy * float2(0.159999996, 0.159999996);
                u_xlat11.xy = u_xlat11.xy * float2(0.159999996, 0.159999996);
                u_xlat13.xy = u_xlat52.xy * float2(0.159999996, 0.159999996);
                u_xlat52.xy = u_xlat9.yw * float2(0.159999996, 0.159999996);
                u_xlat12.z = u_xlat11.x;
                u_xlat12.w = u_xlat52.x;
                u_xlat10.z = u_xlat13.x;
                u_xlat10.w = u_xlat9.x;
                u_xlat14 = u_xlat10.zwxz + u_xlat12.zwxz;
                u_xlat11.z = u_xlat12.y;
                u_xlat11.w = u_xlat52.y;
                u_xlat13.z = u_xlat10.y;
                u_xlat13.w = u_xlat9.z;
                u_xlat9.xyz = u_xlat11.zyw + u_xlat13.zyw;
                u_xlat10.xyz = u_xlat10.xzw / u_xlat14.zwy;
                u_xlat10.xyz = u_xlat10.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat11.xyz = u_xlat13.zyw / u_xlat9.xyz;
                u_xlat11.xyz = u_xlat11.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat10.xyz = u_xlat10.yxz * _pad448.xxx;
                u_xlat11.xyz = u_xlat11.xyz * _pad448.yyy;
                u_xlat10.w = u_xlat11.x;
                u_xlat12 = u_xlat8.xyxy * _pad448.xyxy + u_xlat10.ywxw;
                u_xlat52.xy = u_xlat8.xy * _pad448.xy + u_xlat10.zw;
                u_xlat11.w = u_xlat10.y;
                u_xlat10.yw = u_xlat11.yz;
                u_xlat13 = u_xlat8.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat11 = u_xlat8.xyxy * _pad448.xyxy + u_xlat11.wywz;
                u_xlat10 = u_xlat8.xyxy * _pad448.xyxy + u_xlat10.xwzw;
                u_xlat15 = u_xlat9.xxxy * u_xlat14.zwyz;
                u_xlat16 = u_xlat9.yyzz * u_xlat14;
                u_xlat72 = u_xlat9.z * u_xlat14.y;
                float3 txVec4 = float3(u_xlat12.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec4, 0.0);
                float3 txVec5 = float3(u_xlat12.zw,u_xlat5.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec5, 0.0);
                u_xlat8.x = u_xlat8.x * u_xlat15.y;
                u_xlat73 = u_xlat15.x * u_xlat73 + u_xlat8.x;
                float3 txVec6 = float3(u_xlat52.xy,u_xlat5.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec6, 0.0);
                u_xlat73 = u_xlat15.z * u_xlat8.x + u_xlat73;
                float3 txVec7 = float3(u_xlat11.xy,u_xlat5.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec7, 0.0);
                u_xlat73 = u_xlat15.w * u_xlat8.x + u_xlat73;
                float3 txVec8 = float3(u_xlat13.xy,u_xlat5.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec8, 0.0);
                u_xlat73 = u_xlat16.x * u_xlat8.x + u_xlat73;
                float3 txVec9 = float3(u_xlat13.zw,u_xlat5.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec9, 0.0);
                u_xlat73 = u_xlat16.y * u_xlat8.x + u_xlat73;
                float3 txVec10 = float3(u_xlat11.zw,u_xlat5.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec10, 0.0);
                u_xlat73 = u_xlat16.z * u_xlat8.x + u_xlat73;
                float3 txVec11 = float3(u_xlat10.xy,u_xlat5.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec11, 0.0);
                u_xlat73 = u_xlat16.w * u_xlat8.x + u_xlat73;
                float3 txVec12 = float3(u_xlat10.zw,u_xlat5.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec12, 0.0);
                u_xlat71 = u_xlat72 * u_xlat8.x + u_xlat73;
            } else {
                u_xlat8.xy = u_xlat5.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat8.xy = floor(u_xlat8.xy);
                u_xlat52.xy = u_xlat5.xy * _pad448.zw + (-u_xlat8.xy);
                u_xlat9 = u_xlat52.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat10 = u_xlat9.xxzz * u_xlat9.xxzz;
                u_xlat11.yw = u_xlat10.yw * float2(0.0408160016, 0.0408160016);
                u_xlat9.xz = u_xlat10.xz * float2(0.5, 0.5) + (-u_xlat52.xy);
                u_xlat10.xy = (-u_xlat52.xy) + float2(1.0, 1.0);
                u_xlat54.xy = min(u_xlat52.xy, float2(0.0, 0.0));
                u_xlat10.xy = (-u_xlat54.xy) * u_xlat54.xy + u_xlat10.xy;
                u_xlat54.xy = max(u_xlat52.xy, float2(0.0, 0.0));
                u_xlat31.xz = (-u_xlat54.xy) * u_xlat54.xy + u_xlat9.yw;
                u_xlat10.xy = u_xlat10.xy + float2(2.0, 2.0);
                u_xlat9.yw = u_xlat31.xz + float2(2.0, 2.0);
                u_xlat12.z = u_xlat9.y * 0.0816320032;
                u_xlat13.xyz = u_xlat9.zxw * float3(0.0816320032, 0.0816320032, 0.0816320032);
                u_xlat9.xy = u_xlat10.xy * float2(0.0816320032, 0.0816320032);
                u_xlat12.x = u_xlat13.y;
                u_xlat12.yw = u_xlat52.xx * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat10.xz = u_xlat52.xx * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat10.y = u_xlat9.x;
                u_xlat10.w = u_xlat11.y;
                u_xlat12 = u_xlat10 + u_xlat12;
                u_xlat13.yw = u_xlat52.yy * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat11.xz = u_xlat52.yy * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat11.y = u_xlat9.y;
                u_xlat9 = u_xlat11 + u_xlat13;
                u_xlat10 = u_xlat10 / u_xlat12;
                u_xlat10 = u_xlat10 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat11 = u_xlat11 / u_xlat9;
                u_xlat11 = u_xlat11 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat10 = u_xlat10.wxyz * _pad448.xxxx;
                u_xlat11 = u_xlat11.xwyz * _pad448.yyyy;
                u_xlat13.xzw = u_xlat10.yzw;
                u_xlat13.y = u_xlat11.x;
                u_xlat14 = u_xlat8.xyxy * _pad448.xyxy + u_xlat13.xyzy;
                u_xlat52.xy = u_xlat8.xy * _pad448.xy + u_xlat13.wy;
                u_xlat10.y = u_xlat13.y;
                u_xlat13.y = u_xlat11.z;
                u_xlat15 = u_xlat8.xyxy * _pad448.xyxy + u_xlat13.xyzy;
                u_xlat16.xy = u_xlat8.xy * _pad448.xy + u_xlat13.wy;
                u_xlat10.z = u_xlat13.y;
                u_xlat17 = u_xlat8.xyxy * _pad448.xyxy + u_xlat10.xyxz;
                u_xlat13.y = u_xlat11.w;
                u_xlat18 = u_xlat8.xyxy * _pad448.xyxy + u_xlat13.xyzy;
                u_xlat32.xy = u_xlat8.xy * _pad448.xy + u_xlat13.wy;
                u_xlat10.w = u_xlat13.y;
                u_xlat60.xy = u_xlat8.xy * _pad448.xy + u_xlat10.xw;
                u_xlat11.xzw = u_xlat13.xzw;
                u_xlat13 = u_xlat8.xyxy * _pad448.xyxy + u_xlat11.xyzy;
                u_xlat55.xy = u_xlat8.xy * _pad448.xy + u_xlat11.wy;
                u_xlat11.x = u_xlat10.x;
                u_xlat8.xy = u_xlat8.xy * _pad448.xy + u_xlat11.xy;
                u_xlat19 = u_xlat9.xxxx * u_xlat12;
                u_xlat20 = u_xlat9.yyyy * u_xlat12;
                u_xlat21 = u_xlat9.zzzz * u_xlat12;
                u_xlat9 = u_xlat9.wwww * u_xlat12;
                float3 txVec13 = float3(u_xlat14.xy,u_xlat5.z);
                u_xlat72 = _g_textureLod(_Texture_t5, txVec13, 0.0);
                float3 txVec14 = float3(u_xlat14.zw,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec14, 0.0);
                u_xlat73 = u_xlat73 * u_xlat19.y;
                u_xlat72 = u_xlat19.x * u_xlat72 + u_xlat73;
                float3 txVec15 = float3(u_xlat52.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec15, 0.0);
                u_xlat72 = u_xlat19.z * u_xlat73 + u_xlat72;
                float3 txVec16 = float3(u_xlat17.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec16, 0.0);
                u_xlat72 = u_xlat19.w * u_xlat73 + u_xlat72;
                float3 txVec17 = float3(u_xlat15.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec17, 0.0);
                u_xlat72 = u_xlat20.x * u_xlat73 + u_xlat72;
                float3 txVec18 = float3(u_xlat15.zw,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec18, 0.0);
                u_xlat72 = u_xlat20.y * u_xlat73 + u_xlat72;
                float3 txVec19 = float3(u_xlat16.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec19, 0.0);
                u_xlat72 = u_xlat20.z * u_xlat73 + u_xlat72;
                float3 txVec20 = float3(u_xlat17.zw,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec20, 0.0);
                u_xlat72 = u_xlat20.w * u_xlat73 + u_xlat72;
                float3 txVec21 = float3(u_xlat18.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec21, 0.0);
                u_xlat72 = u_xlat21.x * u_xlat73 + u_xlat72;
                float3 txVec22 = float3(u_xlat18.zw,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec22, 0.0);
                u_xlat72 = u_xlat21.y * u_xlat73 + u_xlat72;
                float3 txVec23 = float3(u_xlat32.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec23, 0.0);
                u_xlat72 = u_xlat21.z * u_xlat73 + u_xlat72;
                float3 txVec24 = float3(u_xlat60.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec24, 0.0);
                u_xlat72 = u_xlat21.w * u_xlat73 + u_xlat72;
                float3 txVec25 = float3(u_xlat13.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec25, 0.0);
                u_xlat72 = u_xlat9.x * u_xlat73 + u_xlat72;
                float3 txVec26 = float3(u_xlat13.zw,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec26, 0.0);
                u_xlat72 = u_xlat9.y * u_xlat73 + u_xlat72;
                float3 txVec27 = float3(u_xlat55.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec27, 0.0);
                u_xlat72 = u_xlat9.z * u_xlat73 + u_xlat72;
                float3 txVec28 = float3(u_xlat8.xy,u_xlat5.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec28, 0.0);
                u_xlat71 = u_xlat9.w * u_xlat73 + u_xlat72;
            }
        }
    } else {
        float3 txVec29 = float3(u_xlat5.xy,u_xlat5.z);
        u_xlat71 = _g_textureLod(_Texture_t5, txVec29, 0.0);
    }
    u_xlat5.x = (-_pad432.x) + 1.0;
    u_xlat5.x = u_xlat71 * _pad432.x + u_xlat5.x;
    u_xlatb27 = 0.0>=u_xlat5.z;
    u_xlatb49 = u_xlat5.z>=1.0;
    u_xlatb27 = u_xlatb49 || u_xlatb27;
    u_xlat5.x = (u_xlatb27) ? 1.0 : u_xlat5.x;
    u_xlat67 = u_xlat67 * _pad432.z + _pad432.w;
    u_xlat67 = clamp(u_xlat67, 0.0, 1.0);
    u_xlat27 = (-u_xlat5.x) + 1.0;
    u_xlat67 = u_xlat67 * u_xlat27 + u_xlat5.x;
    u_xlatb5.x = _pad176.y!=-1.0;
    if(u_xlatb5.x){
        u_xlat5.xy = input.vs_INTERP11.yy * _pad16.xy;
        u_xlat5.xy = _pad0.xy * input.vs_INTERP11.xx + u_xlat5.xy;
        u_xlat5.xy = _pad32.xy * input.vs_INTERP11.zz + u_xlat5.xy;
        u_xlat5.xy = u_xlat5.xy + _pad48.xy;
        u_xlat5.xy = u_xlat5.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
        u_xlat5 = _g_texture(_Texture_t6, u_xlat5.xy, _GlobalMipBias.x);
        u_xlatb8.xy = _g_equal(_pad176.yyyy, float4(0.0, 1.0, 0.0, 0.0)).xy;
        u_xlat71 = (u_xlatb8.y) ? u_xlat5.w : u_xlat5.x;
        u_xlat5.xyz = (u_xlatb8.x) ? u_xlat5.xyz : float3(u_xlat71);
    } else {
        u_xlat5.x = float(1.0);
        u_xlat5.y = float(1.0);
        u_xlat5.z = float(1.0);
    }
    u_xlat5.xyz = u_xlat5.xyz * _MainLightColor.xyz;
    u_xlat71 = dot((-u_xlat1.xyz), u_xlat3.xyz);
    u_xlat71 = u_xlat71 + u_xlat71;
    u_xlat8.xyz = u_xlat3.xyz * (-float3(u_xlat71)) + (-u_xlat1.xyz);
    u_xlat71 = dot(u_xlat3.xyz, u_xlat1.xyz);
    u_xlat71 = clamp(u_xlat71, 0.0, 1.0);
    u_xlat71 = (-u_xlat71) + 1.0;
    u_xlat71 = u_xlat71 * u_xlat71;
    u_xlat71 = u_xlat71 * u_xlat71;
    u_xlat72 = (-u_xlat22.x) * 0.699999988 + 1.70000005;
    u_xlat22.x = u_xlat22.x * u_xlat72;
    u_xlat22.x = u_xlat22.x * 6.0;
    u_xlat9.xyz = unity_SpecCube0_BoxMax.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat10.xyz = u_xlat9.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube0_BoxMin.xyz;
    u_xlat11.xyz = (-u_xlat10.xyz) + input.vs_INTERP11.xyz;
    u_xlat12 = unity_SpecCube0_Rotation.zzxy + unity_SpecCube0_Rotation.zzxy;
    u_xlat13.xyz = u_xlat12.zwy * unity_SpecCube0_Rotation.xyz;
    u_xlat14 = u_xlat12 * unity_SpecCube0_Rotation.xyww;
    u_xlat72 = u_xlat12.y * unity_SpecCube0_Rotation.w;
    u_xlat13.xyz = u_xlat13.zzy + u_xlat13.yxx;
    u_xlat13.xyz = (-u_xlat13.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat15.xy = u_xlat11.xy * u_xlat13.xy;
    u_xlat73 = unity_SpecCube0_Rotation.x * u_xlat12.w + (-u_xlat72);
    u_xlat74 = u_xlat73 * u_xlat11.y + u_xlat15.x;
    u_xlat14.xy = u_xlat14.wz + u_xlat14.xy;
    u_xlat75 = u_xlat11.y * u_xlat14.y;
    u_xlat16.x = u_xlat14.x * u_xlat11.z + u_xlat74;
    u_xlat72 = unity_SpecCube0_Rotation.x * u_xlat12.w + u_xlat72;
    u_xlat74 = u_xlat72 * u_xlat11.x + u_xlat15.y;
    u_xlat33.xz = unity_SpecCube0_Rotation.yx * u_xlat12.yx + (-u_xlat14.zw);
    u_xlat16.y = u_xlat33.x * u_xlat11.z + u_xlat74;
    u_xlat74 = u_xlat33.z * u_xlat11.x + u_xlat75;
    u_xlat16.z = u_xlat13.z * u_xlat11.z + u_xlat74;
    u_xlat10.xyz = u_xlat10.xyz + u_xlat16.xyz;
    u_xlat12.xyz = unity_SpecCube1_BoxMax.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat15.xyz = u_xlat12.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube1_BoxMin.xyz;
    u_xlat16.xyz = (-u_xlat15.xyz) + input.vs_INTERP11.xyz;
    u_xlat17 = unity_SpecCube1_Rotation.zzxy + unity_SpecCube1_Rotation.zzxy;
    u_xlat18.xyz = u_xlat17.zwy * unity_SpecCube1_Rotation.xyz;
    u_xlat19 = u_xlat17 * unity_SpecCube1_Rotation.xyww;
    u_xlat74 = u_xlat17.y * unity_SpecCube1_Rotation.w;
    u_xlat18.xyz = u_xlat18.zzy + u_xlat18.yxx;
    u_xlat18.xyz = (-u_xlat18.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat11.xz = u_xlat16.xy * u_xlat18.xy;
    u_xlat75 = unity_SpecCube1_Rotation.x * u_xlat17.w + (-u_xlat74);
    u_xlat76 = u_xlat75 * u_xlat16.y + u_xlat11.x;
    u_xlat58.xy = u_xlat19.wz + u_xlat19.xy;
    u_xlat11.x = u_xlat16.y * u_xlat58.y;
    u_xlat20.x = u_xlat58.x * u_xlat16.z + u_xlat76;
    u_xlat74 = unity_SpecCube1_Rotation.x * u_xlat17.w + u_xlat74;
    u_xlat76 = u_xlat74 * u_xlat16.x + u_xlat11.z;
    u_xlat38.xz = unity_SpecCube1_Rotation.yx * u_xlat17.yx + (-u_xlat19.zw);
    u_xlat20.y = u_xlat38.x * u_xlat16.z + u_xlat76;
    u_xlat76 = u_xlat38.z * u_xlat16.x + u_xlat11.x;
    u_xlat20.z = u_xlat18.z * u_xlat16.z + u_xlat76;
    u_xlat15.xyz = u_xlat15.xyz + u_xlat20.xyz;
    u_xlat9.x = dot(u_xlat9.xyz, u_xlat9.xyz);
    u_xlat31.x = dot(u_xlat12.xyz, u_xlat12.xyz);
    u_xlat9.x = (-u_xlat31.x) + u_xlat9.x;
    u_xlatb31 = 0.0<unity_SpecCube1_BoxMin.w;
    u_xlatb53 = unity_SpecCube1_BoxMin.w==0.0;
    u_xlatb76 = u_xlat9.x<-9.99999975e-05;
    u_xlatb76 = u_xlatb53 && u_xlatb76;
    u_xlatb31 = u_xlatb31 || u_xlatb76;
    u_xlatb76 = unity_SpecCube1_BoxMin.w<0.0;
    u_xlatb9 = 9.99999975e-05<u_xlat9.x;
    u_xlatb9 = u_xlatb9 && u_xlatb53;
    u_xlatb9 = u_xlatb9 || u_xlatb76;
    u_xlat12.xyz = u_xlat10.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat17.xyz = (-u_xlat10.xyz) + unity_SpecCube0_BoxMax.xyz;
    u_xlat12.xyz = min(u_xlat12.xyz, u_xlat17.xyz);
    u_xlat12.xyz = u_xlat12.xyz / unity_SpecCube0_BoxMax.www;
    u_xlat53 = min(u_xlat12.z, u_xlat12.y);
    u_xlat53 = min(u_xlat53, u_xlat12.x);
    u_xlat53 = clamp(u_xlat53, 0.0, 1.0);
    u_xlat12.xyz = u_xlat15.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat17.xyz = (-u_xlat15.xyz) + unity_SpecCube1_BoxMax.xyz;
    u_xlat12.xyz = min(u_xlat12.xyz, u_xlat17.xyz);
    u_xlat12.xyz = u_xlat12.xyz / unity_SpecCube1_BoxMax.www;
    u_xlat76 = min(u_xlat12.z, u_xlat12.y);
    u_xlat76 = min(u_xlat76, u_xlat12.x);
    u_xlat76 = clamp(u_xlat76, 0.0, 1.0);
    u_xlat11.x = (-u_xlat76) + 1.0;
    u_xlat11.x = min(u_xlat53, u_xlat11.x);
    u_xlat9.x = (u_xlatb9) ? u_xlat11.x : u_xlat53;
    u_xlat53 = (-u_xlat53) + 1.0;
    u_xlat53 = min(u_xlat53, u_xlat76);
    u_xlat9.y = (u_xlatb31) ? u_xlat53 : u_xlat76;
    u_xlat53 = u_xlat9.y + u_xlat9.x;
    u_xlat76 = max(u_xlat53, 1.0);
    u_xlat9.xy = u_xlat9.xy / float2(u_xlat76);
    u_xlatb76 = 0.00999999978<u_xlat9.x;
    if(u_xlatb76){
        u_xlat11.xz = u_xlat8.xy * u_xlat13.xy;
        u_xlat73 = u_xlat73 * u_xlat8.y + u_xlat11.x;
        u_xlat76 = u_xlat8.y * u_xlat14.y;
        u_xlat12.x = u_xlat14.x * u_xlat8.z + u_xlat73;
        u_xlat72 = u_xlat72 * u_xlat8.x + u_xlat11.z;
        u_xlat12.y = u_xlat33.x * u_xlat8.z + u_xlat72;
        u_xlat72 = u_xlat33.z * u_xlat8.x + u_xlat76;
        u_xlat12.z = u_xlat13.z * u_xlat8.z + u_xlat72;
        u_xlatb72 = 0.0<unity_SpecCube0_ProbePosition.w;
        u_xlatb11.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat12.xyzx).xyz;
        u_xlat11.x = (u_xlatb11.x) ? unity_SpecCube0_BoxMax.x : unity_SpecCube0_BoxMin.x;
        u_xlat11.y = (u_xlatb11.y) ? unity_SpecCube0_BoxMax.y : unity_SpecCube0_BoxMin.y;
        u_xlat11.z = (u_xlatb11.z) ? unity_SpecCube0_BoxMax.z : unity_SpecCube0_BoxMin.z;
        u_xlat11.xyz = (-u_xlat10.xyz) + u_xlat11.xyz;
        u_xlat11.xyz = u_xlat11.xyz / u_xlat12.xyz;
        u_xlat73 = min(u_xlat11.y, u_xlat11.x);
        u_xlat73 = min(u_xlat11.z, u_xlat73);
        u_xlat10.xyz = u_xlat10.xyz + (-unity_SpecCube0_ProbePosition.xyz);
        u_xlat10.xyz = u_xlat12.xyz * float3(u_xlat73) + u_xlat10.xyz;
        u_xlat10.xyz = (bool(u_xlatb72)) ? u_xlat10.xyz : u_xlat12.xyz;
        u_xlat11.xyz = unity_SpecCube0_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat12.xyz = u_xlat11.yzx * (-unity_SpecCube0_Rotation.xyz);
        u_xlat13.xyz = u_xlat11.xyz * unity_SpecCube0_Rotation.www;
        u_xlat12.xyz = u_xlat12.zzy + u_xlat12.yxx;
        u_xlat12.xyz = (-u_xlat12.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat33.xz = u_xlat10.xy * u_xlat12.xy;
        u_xlat72 = (-unity_SpecCube0_Rotation.x) * u_xlat11.z + (-u_xlat13.x);
        u_xlat72 = u_xlat72 * u_xlat10.y + u_xlat33.x;
        u_xlat12.xy = (-unity_SpecCube0_Rotation.xy) * u_xlat11.xx + u_xlat13.zy;
        u_xlat73 = u_xlat10.y * u_xlat12.y;
        u_xlat17.x = u_xlat12.x * u_xlat10.z + u_xlat72;
        u_xlat72 = (-unity_SpecCube0_Rotation.x) * u_xlat11.z + u_xlat13.x;
        u_xlat72 = u_xlat72 * u_xlat10.x + u_xlat33.z;
        u_xlat32.xz = (-unity_SpecCube0_Rotation.yx) * u_xlat11.xx + (-u_xlat13.yz);
        u_xlat17.y = u_xlat32.x * u_xlat10.z + u_xlat72;
        u_xlat72 = u_xlat32.z * u_xlat10.x + u_xlat73;
        u_xlat17.z = u_xlat12.z * u_xlat10.z + u_xlat72;
        u_xlat10 = _g_textureLod(_Texture_t1, u_xlat17.xyz, u_xlat22.x);
        u_xlat72 = u_xlat10.w + -1.0;
        u_xlat72 = unity_SpecCube0_HDR.w * u_xlat72 + 1.0;
        u_xlat72 = max(u_xlat72, 0.0);
        u_xlat72 = log2(u_xlat72);
        u_xlat72 = u_xlat72 * unity_SpecCube0_HDR.y;
        u_xlat72 = exp2(u_xlat72);
        u_xlat72 = u_xlat72 * unity_SpecCube0_HDR.x;
        u_xlat10.xyz = u_xlat10.xyz * float3(u_xlat72);
        u_xlat10.xyz = u_xlat9.xxx * u_xlat10.xyz;
    } else {
        u_xlat10.x = float(0.0);
        u_xlat10.y = float(0.0);
        u_xlat10.z = float(0.0);
    }
    u_xlatb72 = 0.00999999978<u_xlat9.y;
    if(u_xlatb72){
        u_xlat11.xy = u_xlat8.xy * u_xlat18.xy;
        u_xlat72 = u_xlat75 * u_xlat8.y + u_xlat11.x;
        u_xlat73 = u_xlat8.y * u_xlat58.y;
        u_xlat12.x = u_xlat58.x * u_xlat8.z + u_xlat72;
        u_xlat72 = u_xlat74 * u_xlat8.x + u_xlat11.y;
        u_xlat12.y = u_xlat38.x * u_xlat8.z + u_xlat72;
        u_xlat72 = u_xlat38.z * u_xlat8.x + u_xlat73;
        u_xlat12.z = u_xlat18.z * u_xlat8.z + u_xlat72;
        u_xlatb72 = 0.0<unity_SpecCube1_ProbePosition.w;
        u_xlatb11.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat12.xyzx).xyz;
        u_xlat11.x = (u_xlatb11.x) ? unity_SpecCube1_BoxMax.x : unity_SpecCube1_BoxMin.x;
        u_xlat11.y = (u_xlatb11.y) ? unity_SpecCube1_BoxMax.y : unity_SpecCube1_BoxMin.y;
        u_xlat11.z = (u_xlatb11.z) ? unity_SpecCube1_BoxMax.z : unity_SpecCube1_BoxMin.z;
        u_xlat11.xyz = (-u_xlat15.xyz) + u_xlat11.xyz;
        u_xlat11.xyz = u_xlat11.xyz / u_xlat12.xyz;
        u_xlat73 = min(u_xlat11.y, u_xlat11.x);
        u_xlat73 = min(u_xlat11.z, u_xlat73);
        u_xlat11.xyz = u_xlat15.xyz + (-unity_SpecCube1_ProbePosition.xyz);
        u_xlat11.xyz = u_xlat12.xyz * float3(u_xlat73) + u_xlat11.xyz;
        u_xlat11.xyz = (bool(u_xlatb72)) ? u_xlat11.xyz : u_xlat12.xyz;
        u_xlat12.xyz = unity_SpecCube1_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat13.xyz = u_xlat12.yzx * (-unity_SpecCube1_Rotation.xyz);
        u_xlat14.xyz = u_xlat12.xyz * unity_SpecCube1_Rotation.www;
        u_xlat13.xyz = u_xlat13.zzy + u_xlat13.yxx;
        u_xlat13.xyz = (-u_xlat13.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat9.xw = u_xlat11.xy * u_xlat13.xy;
        u_xlat72 = (-unity_SpecCube1_Rotation.x) * u_xlat12.z + (-u_xlat14.x);
        u_xlat72 = u_xlat72 * u_xlat11.y + u_xlat9.x;
        u_xlat34.xz = (-unity_SpecCube1_Rotation.xy) * u_xlat12.xx + u_xlat14.zy;
        u_xlat73 = u_xlat11.y * u_xlat34.z;
        u_xlat15.x = u_xlat34.x * u_xlat11.z + u_xlat72;
        u_xlat72 = (-unity_SpecCube1_Rotation.x) * u_xlat12.z + u_xlat14.x;
        u_xlat72 = u_xlat72 * u_xlat11.x + u_xlat9.w;
        u_xlat9.xw = (-unity_SpecCube1_Rotation.yx) * u_xlat12.xx + (-u_xlat14.yz);
        u_xlat15.y = u_xlat9.x * u_xlat11.z + u_xlat72;
        u_xlat72 = u_xlat9.w * u_xlat11.x + u_xlat73;
        u_xlat15.z = u_xlat13.z * u_xlat11.z + u_xlat72;
        u_xlat11 = _g_textureLod(_Texture_t2, u_xlat15.xyz, u_xlat22.x);
        u_xlat72 = u_xlat11.w + -1.0;
        u_xlat72 = unity_SpecCube1_HDR.w * u_xlat72 + 1.0;
        u_xlat72 = max(u_xlat72, 0.0);
        u_xlat72 = log2(u_xlat72);
        u_xlat72 = u_xlat72 * unity_SpecCube1_HDR.y;
        u_xlat72 = exp2(u_xlat72);
        u_xlat72 = u_xlat72 * unity_SpecCube1_HDR.x;
        u_xlat11.xyz = u_xlat11.xyz * float3(u_xlat72);
        u_xlat10.xyz = u_xlat9.yyy * u_xlat11.xyz + u_xlat10.xyz;
    }
    u_xlatb72 = u_xlat53<0.99000001;
    if(u_xlatb72){
        u_xlat8 = _g_textureLod(_Texture_t0, u_xlat8.xyz, u_xlat22.x);
        u_xlat22.x = (-u_xlat53) + 1.0;
        u_xlat72 = u_xlat8.w + -1.0;
        u_xlat72 = _GlossyEnvironmentCubeMap_HDR.w * u_xlat72 + 1.0;
        u_xlat72 = max(u_xlat72, 0.0);
        u_xlat72 = log2(u_xlat72);
        u_xlat72 = u_xlat72 * _GlossyEnvironmentCubeMap_HDR.y;
        u_xlat72 = exp2(u_xlat72);
        u_xlat72 = u_xlat72 * _GlossyEnvironmentCubeMap_HDR.x;
        u_xlat8.xyz = u_xlat8.xyz * float3(u_xlat72);
        u_xlat10.xyz = u_xlat22.xxx * u_xlat8.xyz + u_xlat10.xyz;
    }
    u_xlat22.xy = float2(u_xlat44) * float2(u_xlat44) + float2(-1.0, 1.0);
    u_xlat44 = float(1.0) / u_xlat22.y;
    u_xlat8.xyz = (-u_xlat2.xyz) + float3(u_xlat69);
    u_xlat8.xyz = float3(u_xlat71) * u_xlat8.xyz + u_xlat2.xyz;
    u_xlat8.xyz = float3(u_xlat44) * u_xlat8.xyz;
    u_xlat8.xyz = u_xlat8.xyz * u_xlat10.xyz;
    u_xlat6.xyz = u_xlat6.xyz * u_xlat7.xyz + u_xlat8.xyz;
    u_xlati44 = int(uint(uint(_g_floatBitsToUint(_MainLightLayerMask)) & uint(_g_floatBitsToUint(_pad128.x))));
    u_xlat67 = u_xlat67 * unity_LightData.z;
    u_xlat69 = dot(u_xlat3.xyz, _MainLightPosition.xyz);
    u_xlat69 = clamp(u_xlat69, 0.0, 1.0);
    u_xlat67 = u_xlat67 * u_xlat69;
    u_xlat5.xyz = float3(u_xlat67) * u_xlat5.xyz;
    u_xlat8.xyz = u_xlat1.xyz + _MainLightPosition.xyz;
    u_xlat67 = dot(u_xlat8.xyz, u_xlat8.xyz);
    u_xlat67 = max(u_xlat67, 1.17549435e-38);
    u_xlat67 = _g_inversesqrt(u_xlat67);
    u_xlat8.xyz = float3(u_xlat67) * u_xlat8.xyz;
    u_xlat67 = dot(u_xlat3.xyz, u_xlat8.xyz);
    u_xlat67 = clamp(u_xlat67, 0.0, 1.0);
    u_xlat69 = dot(_MainLightPosition.xyz, u_xlat8.xyz);
    u_xlat69 = clamp(u_xlat69, 0.0, 1.0);
    u_xlat67 = u_xlat67 * u_xlat67;
    u_xlat67 = u_xlat67 * u_xlat22.x + 1.00001001;
    u_xlat69 = u_xlat69 * u_xlat69;
    u_xlat67 = u_xlat67 * u_xlat67;
    u_xlat69 = max(u_xlat69, 0.100000001);
    u_xlat67 = u_xlat67 * u_xlat69;
    u_xlat67 = u_xlat70 * u_xlat67;
    u_xlat67 = u_xlat68 / u_xlat67;
    u_xlat8.xyz = u_xlat2.xyz * float3(u_xlat67) + u_xlat7.xyz;
    u_xlat5.xyz = u_xlat5.xyz * u_xlat8.xyz;
    u_xlat5.xyz = (int(u_xlati44) != 0) ? u_xlat5.xyz : float3(0.0, 0.0, 0.0);
    u_xlat44 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu44 =  uint(int(u_xlat44));
    u_xlatb8.xy = _g_equal(_pad176.zzzz, float4(0.0, 1.0, 0.0, 0.0)).xy;
    u_xlat9.x = float(0.0);
    u_xlat9.y = float(0.0);
    u_xlat9.z = float(0.0);
    for(uint u_xlatu_loop_3 = uint(0u) ; u_xlatu_loop_3<u_xlatu44 ; u_xlatu_loop_3++)
    {
        u_xlatu69 = uint(u_xlatu_loop_3 >> 2u);
        u_xlati71 = int(uint(u_xlatu_loop_3 & 3u));
        u_xlat69 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati71]);
        u_xlatu69 =  uint(int(u_xlat69));
        u_xlat10.xyz = (-input.vs_INTERP11.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat71 = dot(u_xlat10.xyz, u_xlat10.xyz);
        u_xlat71 = max(u_xlat71, 6.10351562e-05);
        u_xlat72 = _g_inversesqrt(u_xlat71);
        u_xlat11.xyz = float3(u_xlat72) * u_xlat10.xyz;
        u_xlat73 = float(1.0) / float(u_xlat71);
        u_xlat71 = u_xlat71 * _AdditionalLightsAttenuation.x;
        u_xlat71 = (-u_xlat71) * u_xlat71 + 1.0;
        u_xlat71 = max(u_xlat71, 0.0);
        u_xlat71 = u_xlat71 * u_xlat71;
        u_xlat71 = u_xlat71 * u_xlat73;
        u_xlat73 = dot(_AdditionalLightsSpotDir.xyz, u_xlat11.xyz);
        u_xlat73 = u_xlat73 * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat73 = clamp(u_xlat73, 0.0, 1.0);
        u_xlat73 = u_xlat73 * u_xlat73;
        u_xlat71 = u_xlat71 * u_xlat73;
        u_xlatu73 = uint(u_xlatu69 >> 5u);
        u_xlati52 = int(1 << int(u_xlatu69));
        u_xlati73 = int(uint(uint(u_xlati52) & uint(_g_floatBitsToUint(_pad64.x))));
        if(u_xlati73 != 0) {
            u_xlati73 = int(_pad20672.x);
            u_xlati52 = (u_xlati73 != 0) ? 0 : 1;
            u_xlati74 = int(int(u_xlatu69) << 2);
            if(u_xlati52 != 0) {
                u_xlat12.xyz = input.vs_INTERP11.yyy * _pad208.xyw;
                u_xlat12.xyz = _pad192.xyw * input.vs_INTERP11.xxx + u_xlat12.xyz;
                u_xlat12.xyz = _pad224.xyw * input.vs_INTERP11.zzz + u_xlat12.xyz;
                u_xlat12.xyz = u_xlat12.xyz + _pad240.xyw;
                u_xlat12.xy = u_xlat12.xy / u_xlat12.zz;
                u_xlat12.xy = u_xlat12.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                u_xlat12.xy = clamp(u_xlat12.xy, 0.0, 1.0);
                u_xlat12.xy = _pad16576.xy * u_xlat12.xy + _pad16576.zw;
            } else {
                u_xlatb73 = u_xlati73==1;
                u_xlati73 = u_xlatb73 ? 1 : int(0);
                if(u_xlati73 != 0) {
                    u_xlat56.xy = input.vs_INTERP11.yy * _pad208.xy;
                    u_xlat56.xy = _pad192.xy * input.vs_INTERP11.xx + u_xlat56.xy;
                    u_xlat56.xy = _pad224.xy * input.vs_INTERP11.zz + u_xlat56.xy;
                    u_xlat56.xy = u_xlat56.xy + _pad240.xy;
                    u_xlat56.xy = u_xlat56.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat56.xy = _g_fract(u_xlat56.xy);
                    u_xlat12.xy = _pad16576.xy * u_xlat56.xy + _pad16576.zw;
                } else {
                    u_xlat13 = input.vs_INTERP11.yyyy * _pad208;
                    u_xlat13 = _pad192 * input.vs_INTERP11.xxxx + u_xlat13;
                    u_xlat13 = _pad224 * input.vs_INTERP11.zzzz + u_xlat13;
                    u_xlat13 = u_xlat13 + _pad240;
                    u_xlat13.xyz = u_xlat13.xyz / u_xlat13.www;
                    u_xlat73 = dot(u_xlat13.xyz, u_xlat13.xyz);
                    u_xlat73 = _g_inversesqrt(u_xlat73);
                    u_xlat13.xyz = float3(u_xlat73) * u_xlat13.xyz;
                    u_xlat73 = dot(abs(u_xlat13.xyz), float3(1.0, 1.0, 1.0));
                    u_xlat73 = max(u_xlat73, 9.99999997e-07);
                    u_xlat73 = float(1.0) / float(u_xlat73);
                    u_xlat14.xyz = float3(u_xlat73) * u_xlat13.zxy;
                    u_xlat14.x = (-u_xlat14.x);
                    u_xlat14.x = clamp(u_xlat14.x, 0.0, 1.0);
                    u_xlatb52.xy = _g_greaterThanEqual(u_xlat14.yzyz, float4(0.0, 0.0, 0.0, 0.0)).xy;
                    u_xlat52.x = (u_xlatb52.x) ? u_xlat14.x : (-u_xlat14.x);
                    u_xlat52.y = (u_xlatb52.y) ? u_xlat14.x : (-u_xlat14.x);
                    u_xlat52.xy = u_xlat13.xy * float2(u_xlat73) + u_xlat52.xy;
                    u_xlat52.xy = u_xlat52.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat52.xy = clamp(u_xlat52.xy, 0.0, 1.0);
                    u_xlat12.xy = _pad16576.xy * u_xlat52.xy + _pad16576.zw;
                }
            }
            u_xlat12 = _g_textureLod(_Texture_t7, u_xlat12.xy, 0.0);
            u_xlat73 = (u_xlatb8.y) ? u_xlat12.w : u_xlat12.x;
            u_xlat12.xyz = (u_xlatb8.x) ? u_xlat12.xyz : float3(u_xlat73);
        } else {
            u_xlat12.x = float(1.0);
            u_xlat12.y = float(1.0);
            u_xlat12.z = float(1.0);
        }
        u_xlat12.xyz = u_xlat12.xyz * _AdditionalLightsColor.xyz;
        u_xlati69 = int(uint(uint(_g_floatBitsToUint(_AdditionalLightsLayerMasks)) & uint(_g_floatBitsToUint(_pad128.x))));
        u_xlat73 = dot(u_xlat3.xyz, u_xlat11.xyz);
        u_xlat73 = clamp(u_xlat73, 0.0, 1.0);
        u_xlat71 = u_xlat71 * u_xlat73;
        u_xlat12.xyz = float3(u_xlat71) * u_xlat12.xyz;
        u_xlat10.xyz = u_xlat10.xyz * float3(u_xlat72) + u_xlat1.xyz;
        u_xlat71 = dot(u_xlat10.xyz, u_xlat10.xyz);
        u_xlat71 = max(u_xlat71, 1.17549435e-38);
        u_xlat71 = _g_inversesqrt(u_xlat71);
        u_xlat10.xyz = float3(u_xlat71) * u_xlat10.xyz;
        u_xlat71 = dot(u_xlat3.xyz, u_xlat10.xyz);
        u_xlat71 = clamp(u_xlat71, 0.0, 1.0);
        u_xlat72 = dot(u_xlat11.xyz, u_xlat10.xyz);
        u_xlat72 = clamp(u_xlat72, 0.0, 1.0);
        u_xlat71 = u_xlat71 * u_xlat71;
        u_xlat71 = u_xlat71 * u_xlat22.x + 1.00001001;
        u_xlat72 = u_xlat72 * u_xlat72;
        u_xlat71 = u_xlat71 * u_xlat71;
        u_xlat72 = max(u_xlat72, 0.100000001);
        u_xlat71 = u_xlat71 * u_xlat72;
        u_xlat71 = u_xlat70 * u_xlat71;
        u_xlat71 = u_xlat68 / u_xlat71;
        u_xlat10.xyz = u_xlat2.xyz * float3(u_xlat71) + u_xlat7.xyz;
        u_xlat10.xyz = u_xlat10.xyz * u_xlat12.xyz + u_xlat9.xyz;
        u_xlat9.xyz = (int(u_xlati69) != 0) ? u_xlat10.xyz : u_xlat9.xyz;
    }
    u_xlat1.xyz = u_xlat5.xyz + u_xlat6.xyz;
    u_xlat1.xyz = u_xlat9.xyz + u_xlat1.xyz;
    u_xlat1.xyz = float3(_Emission) * u_xlat4.xyz + u_xlat1.xyz;
    u_xlat22.x = u_xlat66 * (-u_xlat66);
    u_xlat22.x = exp2(u_xlat22.x);
    u_xlat44 = (-u_xlat22.x) + 1.0;
    u_xlat2.xyz = float3(u_xlat44) * _pad992.xyz;
    __SV_Target0.xyz = u_xlat1.xyz * u_xlat22.xxx + u_xlat2.xyz;
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
