Shader "Shader Graphs/WindyDefault"
{
    Properties
    {



[NoScaleOffset] _Texture ("Texture", 2D) = "white" {}
_Smoothness ("Smoothness", Float) = 0
_Metallic ("Metallic", Float) = 0
_WindFromOrigoSmoothstep ("WindFromOrigoSmoothstep", Vector) = (0,1,0,0)
_WindSpeed ("WindSpeed", Float) = 2.8
_WindSpeed2 ("WindSpeed2", Float) = 0.5
_WindScale ("WindScale", Float) = 0.15
_WindDensity ("WindDensity", Float) = 0.2
_NormalInfluence ("NormalInfluence", Float) = 2
_SSSPower ("SSSPower", Float) = 1.5
_SSSIntensity ("SSSIntensity", Float) = 2
_Thickness ("Thickness", Float) = 1
[NoScaleOffset] _Normal_Map ("Normal Map", 2D) = "white" {}
_Normal_Strength ("Normal Strength", Range(0, 1)) = 0.146
_Normal_Scale ("Normal Scale", Float) = 1
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
            TEXTURE2D(_Texture);
            SAMPLER(sampler__Texture);
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
            float4 _pad976;
            float4 _pad992;
            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float _WindSpeed;
            float _WindDensity;
            float _WindScale;
            float _WindSpeed2;
            float2 _WindFromOrigoSmoothstep;
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
            float _Smoothness;
            float _NormalInfluence;
            float _SSSPower;
            float _SSSIntensity;
            float _Thickness;
            float _Normal_Strength;
            float _Normal_Scale;
            float _Metallic;

            float4 u_xlat0;
            float4 u_xlat1;
            int2 u_xlati1;
            uint u_xlatu1;
            float4 u_xlat2;
            int4 u_xlati2;
            uint2 u_xlatu2;
            float4 u_xlat3;
            float2 u_xlat4;
            float2 u_xlat5;
            int2 u_xlati5;
            uint2 u_xlatu5;
            float3 u_xlat6;
            float2 u_xlat8;
            int2 u_xlati8;
            uint u_xlatu8;
            float2 u_xlat9;
            int2 u_xlati9;
            uint2 u_xlatu9;
            float2 u_xlat11;
            float u_xlat12;
            int u_xlati12;
            uint u_xlatu12;
            float4 ImmCB_0_0_0[4];
            bool4 u_xlatb3;
            bool2 u_xlatb6;
            float4 u_xlat7;
            bool u_xlatb8;
            float4 u_xlat10;
            int u_xlati10;
            bool4 u_xlatb10;
            float4 u_xlat13;
            float4 u_xlat14;
            float4 u_xlat15;
            float4 u_xlat16;
            float3 u_xlat17;
            float4 u_xlat18;
            float4 u_xlat19;
            float4 u_xlat20;
            float2 u_xlat24;
            bool2 u_xlatb24;
            float2 u_xlat27;
            bool u_xlatb27;
            float3 u_xlat28;
            float2 u_xlat29;
            bool u_xlatb29;
            float3 u_xlat30;
            float3 u_xlat31;
            float3 u_xlat32;
            float3 u_xlat36;
            float2 u_xlat44;
            float u_xlat45;
            bool u_xlatb45;
            float2 u_xlat48;
            int u_xlati48;
            float2 u_xlat49;
            float u_xlat50;
            bool u_xlatb50;
            float2 u_xlat51;
            float2 u_xlat52;
            float2 u_xlat55;
            float u_xlat63;
            int u_xlati63;
            uint u_xlatu63;
            bool u_xlatb63;
            float u_xlat64;
            int u_xlati64;
            uint u_xlatu64;
            float u_xlat65;
            uint u_xlatu65;
            float u_xlat66;
            float u_xlat67;
            int u_xlati67;
            uint u_xlatu67;
            bool u_xlatb67;
            float u_xlat68;
            float u_xlat69;
            float u_xlat70;
            float u_xlat71;
            int u_xlati71;
            uint u_xlatu71;
            bool u_xlatb71;
            float u_xlat72;
            int u_xlati72;
            bool u_xlatb72;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TANGENT0 : TANGENT;
                float4 in_TEXCOORD0 : TEXCOORD0;
                float4 in_TEXCOORD1 : TEXCOORD1;
                float4 in_TEXCOORD2 : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 vs_INTERP0 : TEXCOORD0;
                float3 vs_INTERP10 : TEXCOORD1;
                float3 vs_INTERP11 : TEXCOORD2;
                float4 vs_INTERP5 : TEXCOORD3;
                float4 vs_INTERP6 : TEXCOORD4;
                float4 vs_INTERP7 : TEXCOORD5;
                float4 vs_INTERP8 : TEXCOORD6;
                float4 vs_INTERP9 : TEXCOORD7;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0.xy = float2(float2(_WindSpeed2, _WindSpeed2)) * _TimeParameters.xx + input.in_POSITION0.xy;
    u_xlat0.xy = u_xlat0.xy * float2(float2(_WindDensity, _WindDensity));
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
    u_xlat4.x = (-u_xlat0.x) + u_xlat5.x;
    u_xlat0.x = u_xlat9.y * u_xlat4.x + u_xlat0.x;
    u_xlat4.x = (-u_xlat0.x) + u_xlat1.x;
    u_xlat0.x = u_xlat9.x * u_xlat4.x + u_xlat0.x;
    u_xlat0.x = u_xlat0.x + 0.5;
    u_xlat4.xy = (-_TimeParameters.xx) * float2(_WindSpeed) + input.in_POSITION0.xy;
    u_xlat4.xy = u_xlat4.xy * float2(float2(_WindDensity, _WindDensity));
    u_xlat1.xy = floor(u_xlat4.xy);
    u_xlat4.xy = _g_fract(u_xlat4.xy);
    u_xlat9.xy = u_xlat1.xy + float2(1.0, 1.0);
    u_xlati9.xy = int2(u_xlat9.xy);
    u_xlati12 = int(uint(uint(u_xlati9.y) ^ 1103515245u));
    u_xlati9.x = u_xlati12 + u_xlati9.x;
    u_xlatu12 = uint(u_xlati12) * uint(u_xlati9.x);
    u_xlatu9.x = uint(u_xlatu12 >> 5u);
    u_xlati12 = int(uint(u_xlatu12 ^ u_xlatu9.x));
    u_xlatu12 = uint(u_xlati12) * 668265261u;
    u_xlatu12 = uint(u_xlatu12 >> 8u);
    u_xlat12 = float(u_xlatu12);
    u_xlat2.yz = float2(u_xlat12) * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat9.x = floor(u_xlat2.y);
    u_xlat2.x = u_xlat12 * 5.96046519e-08 + (-u_xlat9.x);
    u_xlat12 = dot(u_xlat2.xz, u_xlat2.xz);
    u_xlat12 = _g_inversesqrt(u_xlat12);
    u_xlat9.xy = float2(u_xlat12) * u_xlat2.xz;
    u_xlat2.xy = u_xlat4.xy + float2(-1.0, -1.0);
    u_xlat12 = dot(u_xlat9.xy, u_xlat2.xy);
    u_xlat2 = u_xlat1.xyxy + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati1.xy = int2(u_xlat1.xy);
    u_xlati2 = int4(u_xlat2);
    u_xlati9.xy = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati2.xy = u_xlati9.xy + u_xlati2.xz;
    u_xlatu9.xy = uint2(u_xlati9.xy) * uint2(u_xlati2.xy);
    u_xlatu2.xy = uint2(u_xlatu9.x >> 5u, u_xlatu9.y >> 5u);
    u_xlati9.xy = int2(uint2(u_xlatu9.x ^ u_xlatu2.x, u_xlatu9.y ^ u_xlatu2.y));
    u_xlatu9.xy = uint2(u_xlati9.xy) * uint2(668265261u, 668265261u);
    u_xlatu9.xy = uint2(u_xlatu9.x >> 8u, u_xlatu9.y >> 8u);
    u_xlat9.xy = float2(u_xlatu9.xy);
    u_xlat2 = u_xlat9.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat3.xy = floor(u_xlat2.xy);
    u_xlat2.xy = u_xlat9.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat3.xy);
    u_xlat9.x = dot(u_xlat2.yw, u_xlat2.yw);
    u_xlat9.x = _g_inversesqrt(u_xlat9.x);
    u_xlat9.xy = u_xlat9.xx * u_xlat2.yw;
    u_xlat3 = u_xlat4.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat9.x = dot(u_xlat9.xy, u_xlat3.zw);
    u_xlat12 = u_xlat12 + (-u_xlat9.x);
    u_xlat6.xz = u_xlat4.xy * u_xlat4.xy;
    u_xlat6.xz = u_xlat4.xy * u_xlat6.xz;
    u_xlat11.xy = u_xlat4.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat11.xy = u_xlat4.xy * u_xlat11.xy + float2(10.0, 10.0);
    u_xlat6.xz = u_xlat6.xz * u_xlat11.xy;
    u_xlat12 = u_xlat6.z * u_xlat12 + u_xlat9.x;
    u_xlat9.x = dot(u_xlat2.xz, u_xlat2.xz);
    u_xlat9.x = _g_inversesqrt(u_xlat9.x);
    u_xlat9.xy = u_xlat9.xx * u_xlat2.xz;
    u_xlat9.x = dot(u_xlat9.xy, u_xlat3.xy);
    u_xlati5.x = int(uint(uint(u_xlati1.y) ^ 1103515245u));
    u_xlati1.x = u_xlati5.x + u_xlati1.x;
    u_xlatu1 = uint(u_xlati5.x) * uint(u_xlati1.x);
    u_xlatu5.x = uint(u_xlatu1 >> 5u);
    u_xlati1.x = int(uint(u_xlatu5.x ^ u_xlatu1));
    u_xlatu1 = uint(u_xlati1.x) * 668265261u;
    u_xlatu1 = uint(u_xlatu1 >> 8u);
    u_xlat1.x = float(u_xlatu1);
    u_xlat3.yz = u_xlat1.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat5.x = floor(u_xlat3.y);
    u_xlat3.x = u_xlat1.x * 5.96046519e-08 + (-u_xlat5.x);
    u_xlat1.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    u_xlat1.xy = u_xlat1.xx * u_xlat3.xz;
    u_xlat4.x = dot(u_xlat1.xy, u_xlat4.xy);
    u_xlat8.x = (-u_xlat4.x) + u_xlat9.x;
    u_xlat4.x = u_xlat6.z * u_xlat8.x + u_xlat4.x;
    u_xlat8.x = (-u_xlat4.x) + u_xlat12;
    u_xlat4.x = u_xlat6.x * u_xlat8.x + u_xlat4.x;
    u_xlat0.x = u_xlat0.x + u_xlat4.x;
    u_xlat0.x = u_xlat0.x + -0.5;
    u_xlat0.x = u_xlat0.x * _WindScale;
    u_xlat4.x = dot(input.in_POSITION0.xy, input.in_POSITION0.xy);
    u_xlat4.x = sqrt(u_xlat4.x);
    u_xlat4.x = u_xlat4.x + (-_WindFromOrigoSmoothstep.xxxy.z);
    u_xlat8.x = (-_WindFromOrigoSmoothstep.xxxy.z) + _WindFromOrigoSmoothstep.xxxy.w;
    u_xlat8.x = float(1.0) / u_xlat8.x;
    u_xlat4.x = u_xlat8.x * u_xlat4.x;
    u_xlat4.x = clamp(u_xlat4.x, 0.0, 1.0);
    u_xlat8.x = u_xlat4.x * -2.0 + 3.0;
    u_xlat4.x = u_xlat4.x * u_xlat4.x;
    u_xlat4.x = u_xlat4.x * u_xlat8.x;
    u_xlat0.xy = u_xlat0.xx * u_xlat4.xx;
    u_xlat0.z = 0.0;
    u_xlat0.xyz = u_xlat0.xyz + input.in_POSITION0.xyz;
    u_xlat1.xyz = u_xlat0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyw = _tunity_ObjectToWorld[0].xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * u_xlat0.zzz + u_xlat0.xyw;
    u_xlat0.xyz = u_xlat0.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.vs_INTERP10.xyz = u_xlat0.xyz;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    output.vs_INTERP0.xy = input.in_TEXCOORD1.xy * _pad400.xy + _pad400.zw;
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
    output.vs_INTERP9 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat12 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat12 = max(u_xlat12, 1.17549435e-38);
    u_xlat12 = _g_inversesqrt(u_xlat12);
    output.vs_INTERP11.xyz = float3(u_xlat12) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_0_0[3] = float4(0.0, 0.0, 0.0, 1.0);
    u_xlat0.x = dot(input.vs_INTERP11.xyz, input.vs_INTERP11.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = float(1.0) / u_xlat0.x;
    u_xlat0.xyz = u_xlat0.xxx * input.vs_INTERP11.xyz;
    u_xlatb63 = unity_OrthoParams.w==0.0;
    u_xlat1.xyz = (-input.vs_INTERP10.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat64 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat64 = _g_inversesqrt(u_xlat64);
    u_xlat1.xyz = float3(u_xlat64) * u_xlat1.xyz;
    u_xlat2.x = _tunity_MatrixV[0].z;
    u_xlat2.y = _tunity_MatrixV[1].z;
    u_xlat2.z = _tunity_MatrixV[2].z;
    u_xlat1.xyz = (bool(u_xlatb63)) ? u_xlat1.xyz : u_xlat2.xyz;
    u_xlat2.xyz = _g_texture(_Texture_t8, input.vs_INTERP6.xy, _GlobalMipBias.x).xyz;
    u_xlat3.xyz = _g_texture(_Texture_t8, input.vs_INTERP8.xy, _GlobalMipBias.x).xyz;
    u_xlat0.xyz = u_xlat0.xyz * float3(float3(_NormalInfluence, _NormalInfluence, _NormalInfluence)) + _MainLightPosition.xyz;
    u_xlat63 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat63 = _g_inversesqrt(u_xlat63);
    u_xlat0.xyz = float3(u_xlat63) * u_xlat0.xyz;
    u_xlat0.x = dot((-u_xlat0.xyz), u_xlat1.xyz);
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat0.x = log2(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _SSSPower;
    u_xlat0.x = exp2(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _SSSIntensity;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat3.xyz;
    u_xlat0.xyz = float3(float3(_Thickness, _Thickness, _Thickness)) * u_xlat0.xyz + u_xlat2.xyz;
    u_xlat2.xyz = dFdy(input.vs_INTERP10.zxy);
    u_xlat3.xyz = dFdx(input.vs_INTERP10.yzx);
    u_xlat4.xyz = u_xlat2.xyz * u_xlat3.xyz;
    u_xlat2.xyz = u_xlat2.zxy * u_xlat3.yzx + (-u_xlat4.xyz);
    u_xlat63 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat63 = _g_inversesqrt(u_xlat63);
    u_xlat3.xyz = float3(u_xlat63) * u_xlat2.xyz;
    u_xlat44.xy = input.vs_INTERP7.xy * float2(float2(_Normal_Scale, _Normal_Scale));
    u_xlat4.xyw = _g_texture(_Texture_t9, u_xlat44.xy, _GlobalMipBias.x).xyw;
    u_xlat4.x = u_xlat4.x * u_xlat4.w;
    u_xlat4.xy = u_xlat4.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat64 = dot(u_xlat4.xy, u_xlat4.xy);
    u_xlat64 = min(u_xlat64, 1.0);
    u_xlat64 = (-u_xlat64) + 1.0;
    u_xlat64 = sqrt(u_xlat64);
    u_xlat4.z = max(u_xlat64, 1.00000002e-16);
    u_xlat64 = dot(u_xlat4, u_xlat4);
    u_xlat64 = _g_inversesqrt(u_xlat64);
    u_xlat4.xyz = float3(u_xlat64) * u_xlat4.xyz;
    u_xlat2.xy = u_xlat2.xy * float2(u_xlat63) + u_xlat4.xy;
    u_xlat2.z = u_xlat3.z * u_xlat4.z;
    u_xlat63 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat63 = max(u_xlat63, 1.17549435e-38);
    u_xlat63 = _g_inversesqrt(u_xlat63);
    u_xlat2.xyz = u_xlat2.xyz * float3(u_xlat63) + (-u_xlat3.xyz);
    u_xlat2.xyz = float3(_Normal_Strength) * u_xlat2.xyz + u_xlat3.xyz;
    u_xlat63 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat63 = _g_inversesqrt(u_xlat63);
    u_xlat2.xyz = float3(u_xlat63) * u_xlat2.xyz;
    u_xlat3.xyz = input.vs_INTERP10.xyz + (-_pad320.xyz);
    u_xlat4.xyz = input.vs_INTERP10.xyz + (-_pad336.xyz);
    u_xlat5.xyz = input.vs_INTERP10.xyz + (-_pad352.xyz);
    u_xlat6.xyz = input.vs_INTERP10.xyz + (-_pad368.xyz);
    u_xlat3.x = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat3.y = dot(u_xlat4.xyz, u_xlat4.xyz);
    u_xlat3.z = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlat3.w = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlatb3 = _g_lessThan(u_xlat3, _pad384);
    u_xlat4.x = u_xlatb3.x ? float(1.0) : 0.0;
    u_xlat4.y = u_xlatb3.y ? float(1.0) : 0.0;
    u_xlat4.z = u_xlatb3.z ? float(1.0) : 0.0;
    u_xlat4.w = u_xlatb3.w ? float(1.0) : 0.0;
;
    u_xlat3.x = (u_xlatb3.x) ? float(-1.0) : float(-0.0);
    u_xlat3.y = (u_xlatb3.y) ? float(-1.0) : float(-0.0);
    u_xlat3.z = (u_xlatb3.z) ? float(-1.0) : float(-0.0);
    u_xlat3.xyz = u_xlat3.xyz + u_xlat4.yzw;
    u_xlat4.yzw = max(u_xlat3.xyz, float3(0.0, 0.0, 0.0));
    u_xlat63 = dot(u_xlat4, float4(4.0, 3.0, 2.0, 1.0));
    u_xlat63 = (-u_xlat63) + 4.0;
    u_xlatu63 = uint(u_xlat63);
    u_xlati63 = int(int(u_xlatu63) << 2);
    u_xlat3.xyz = input.vs_INTERP10.yyy * _pad16.xyz;
    u_xlat3.xyz = _pad0.xyz * input.vs_INTERP10.xxx + u_xlat3.xyz;
    u_xlat3.xyz = _pad32.xyz * input.vs_INTERP10.zzz + u_xlat3.xyz;
    u_xlat3.xyz = u_xlat3.xyz + _pad48.xyz;
    u_xlat63 = input.vs_INTERP10.y * _tunity_MatrixV[1].z;
    u_xlat63 = _tunity_MatrixV[0].z * input.vs_INTERP10.x + u_xlat63;
    u_xlat63 = _tunity_MatrixV[2].z * input.vs_INTERP10.z + u_xlat63;
    u_xlat63 = u_xlat63 + _tunity_MatrixV[3].z;
    u_xlat63 = (-u_xlat63) + (-_pad352.y);
    u_xlat63 = max(u_xlat63, 0.0);
    u_xlat63 = u_xlat63 * _pad976.x;
    u_xlat64 = _Metallic;
    u_xlat64 = clamp(u_xlat64, 0.0, 1.0);
    u_xlat65 = _Smoothness;
    u_xlat65 = clamp(u_xlat65, 0.0, 1.0);
    u_xlat4.xyz = _g_texture(_Texture, input.vs_INTERP0.xy, _GlobalMipBias.x).xyz;
    u_xlat5 = _g_texture(_Normal_Map, input.vs_INTERP0.xy, _GlobalMipBias.x);
    u_xlat5.xyz = u_xlat5.xyz + float3(-0.5, -0.5, -0.5);
    u_xlat66 = dot(u_xlat2.xyz, u_xlat5.xyz);
    u_xlat66 = u_xlat66 + 0.5;
    u_xlat4.xyz = float3(u_xlat66) * u_xlat4.xyz;
    u_xlat66 = max(u_xlat5.w, 9.99999975e-05);
    u_xlat4.xyz = u_xlat4.xyz / float3(u_xlat66);
    u_xlat66 = (-u_xlat64) * 0.959999979 + 0.959999979;
    u_xlat67 = u_xlat65 + (-u_xlat66);
    u_xlat5.xyz = u_xlat0.xyz * float3(u_xlat66);
    u_xlat0.xyz = u_xlat0.xyz + float3(-0.0399999991, -0.0399999991, -0.0399999991);
    u_xlat0.xyz = float3(u_xlat64) * u_xlat0.xyz + float3(0.0399999991, 0.0399999991, 0.0399999991);
    u_xlat64 = (-u_xlat65) + 1.0;
    u_xlat65 = u_xlat64 * u_xlat64;
    u_xlat65 = max(u_xlat65, 0.0078125);
    u_xlat66 = u_xlat65 * u_xlat65;
    u_xlat67 = u_xlat67 + 1.0;
    u_xlat67 = min(u_xlat67, 1.0);
    u_xlat68 = u_xlat65 * 4.0 + 2.0;
    u_xlatb6.x = 0.0<_MainLightShadowParams.y;
    if(u_xlatb6.x){
        u_xlatb6.x = _MainLightShadowParams.y==1.0;
        if(u_xlatb6.x){
            u_xlat6 = u_xlat3.xyxy + _pad400;
            float3 txVec0 = float3(u_xlat6.xy,u_xlat3.z);
            u_xlat7.x = _g_textureLod(_Texture_t5, txVec0, 0.0);
            float3 txVec1 = float3(u_xlat6.zw,u_xlat3.z);
            u_xlat7.y = _g_textureLod(_Texture_t5, txVec1, 0.0);
            u_xlat6 = u_xlat3.xyxy + _pad416;
            float3 txVec2 = float3(u_xlat6.xy,u_xlat3.z);
            u_xlat7.z = _g_textureLod(_Texture_t5, txVec2, 0.0);
            float3 txVec3 = float3(u_xlat6.zw,u_xlat3.z);
            u_xlat7.w = _g_textureLod(_Texture_t5, txVec3, 0.0);
            u_xlat6.x = dot(u_xlat7, float4(0.25, 0.25, 0.25, 0.25));
        } else {
            u_xlatb27 = _MainLightShadowParams.y==2.0;
            if(u_xlatb27){
                u_xlat27.xy = u_xlat3.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat27.xy = floor(u_xlat27.xy);
                u_xlat7.xy = u_xlat3.xy * _pad448.zw + (-u_xlat27.xy);
                u_xlat8 = u_xlat7.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat9 = u_xlat8.xxzz * u_xlat8.xxzz;
                u_xlat49.xy = u_xlat9.yw * float2(0.0799999982, 0.0799999982);
                u_xlat8.xz = u_xlat9.xz * float2(0.5, 0.5) + (-u_xlat7.xy);
                u_xlat9.xy = (-u_xlat7.xy) + float2(1.0, 1.0);
                u_xlat51.xy = min(u_xlat7.xy, float2(0.0, 0.0));
                u_xlat51.xy = (-u_xlat51.xy) * u_xlat51.xy + u_xlat9.xy;
                u_xlat7.xy = max(u_xlat7.xy, float2(0.0, 0.0));
                u_xlat7.xy = (-u_xlat7.xy) * u_xlat7.xy + u_xlat8.yw;
                u_xlat51.xy = u_xlat51.xy + float2(1.0, 1.0);
                u_xlat7.xy = u_xlat7.xy + float2(1.0, 1.0);
                u_xlat10.xy = u_xlat8.xz * float2(0.159999996, 0.159999996);
                u_xlat11.xy = u_xlat9.xy * float2(0.159999996, 0.159999996);
                u_xlat9.xy = u_xlat51.xy * float2(0.159999996, 0.159999996);
                u_xlat12.xy = u_xlat7.xy * float2(0.159999996, 0.159999996);
                u_xlat7.xy = u_xlat8.yw * float2(0.159999996, 0.159999996);
                u_xlat10.z = u_xlat9.x;
                u_xlat10.w = u_xlat7.x;
                u_xlat11.z = u_xlat12.x;
                u_xlat11.w = u_xlat49.x;
                u_xlat8 = u_xlat10.zwxz + u_xlat11.zwxz;
                u_xlat9.z = u_xlat10.y;
                u_xlat9.w = u_xlat7.y;
                u_xlat12.z = u_xlat11.y;
                u_xlat12.w = u_xlat49.y;
                u_xlat7.xyz = u_xlat9.zyw + u_xlat12.zyw;
                u_xlat9.xyz = u_xlat11.xzw / u_xlat8.zwy;
                u_xlat9.xyz = u_xlat9.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat10.xyz = u_xlat12.zyw / u_xlat7.xyz;
                u_xlat10.xyz = u_xlat10.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat9.xyz = u_xlat9.yxz * _pad448.xxx;
                u_xlat10.xyz = u_xlat10.xyz * _pad448.yyy;
                u_xlat9.w = u_xlat10.x;
                u_xlat11 = u_xlat27.xyxy * _pad448.xyxy + u_xlat9.ywxw;
                u_xlat12.xy = u_xlat27.xy * _pad448.xy + u_xlat9.zw;
                u_xlat10.w = u_xlat9.y;
                u_xlat9.yw = u_xlat10.yz;
                u_xlat13 = u_xlat27.xyxy * _pad448.xyxy + u_xlat9.xyzy;
                u_xlat10 = u_xlat27.xyxy * _pad448.xyxy + u_xlat10.wywz;
                u_xlat9 = u_xlat27.xyxy * _pad448.xyxy + u_xlat9.xwzw;
                u_xlat14 = u_xlat7.xxxy * u_xlat8.zwyz;
                u_xlat15 = u_xlat7.yyzz * u_xlat8;
                u_xlat27.x = u_xlat7.z * u_xlat8.y;
                float3 txVec4 = float3(u_xlat11.xy,u_xlat3.z);
                u_xlat48.x = _g_textureLod(_Texture_t5, txVec4, 0.0);
                float3 txVec5 = float3(u_xlat11.zw,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec5, 0.0);
                u_xlat69 = u_xlat69 * u_xlat14.y;
                u_xlat48.x = u_xlat14.x * u_xlat48.x + u_xlat69;
                float3 txVec6 = float3(u_xlat12.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec6, 0.0);
                u_xlat48.x = u_xlat14.z * u_xlat69 + u_xlat48.x;
                float3 txVec7 = float3(u_xlat10.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec7, 0.0);
                u_xlat48.x = u_xlat14.w * u_xlat69 + u_xlat48.x;
                float3 txVec8 = float3(u_xlat13.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec8, 0.0);
                u_xlat48.x = u_xlat15.x * u_xlat69 + u_xlat48.x;
                float3 txVec9 = float3(u_xlat13.zw,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec9, 0.0);
                u_xlat48.x = u_xlat15.y * u_xlat69 + u_xlat48.x;
                float3 txVec10 = float3(u_xlat10.zw,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec10, 0.0);
                u_xlat48.x = u_xlat15.z * u_xlat69 + u_xlat48.x;
                float3 txVec11 = float3(u_xlat9.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec11, 0.0);
                u_xlat48.x = u_xlat15.w * u_xlat69 + u_xlat48.x;
                float3 txVec12 = float3(u_xlat9.zw,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec12, 0.0);
                u_xlat6.x = u_xlat27.x * u_xlat69 + u_xlat48.x;
            } else {
                u_xlat27.xy = u_xlat3.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat27.xy = floor(u_xlat27.xy);
                u_xlat7.xy = u_xlat3.xy * _pad448.zw + (-u_xlat27.xy);
                u_xlat8 = u_xlat7.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat9 = u_xlat8.xxzz * u_xlat8.xxzz;
                u_xlat10.yw = u_xlat9.yw * float2(0.0408160016, 0.0408160016);
                u_xlat49.xy = u_xlat9.xz * float2(0.5, 0.5) + (-u_xlat7.xy);
                u_xlat8.xz = (-u_xlat7.xy) + float2(1.0, 1.0);
                u_xlat9.xy = min(u_xlat7.xy, float2(0.0, 0.0));
                u_xlat8.xz = (-u_xlat9.xy) * u_xlat9.xy + u_xlat8.xz;
                u_xlat9.xy = max(u_xlat7.xy, float2(0.0, 0.0));
                u_xlat8.yw = (-u_xlat9.xy) * u_xlat9.xy + u_xlat8.yw;
                u_xlat8 = u_xlat8 + float4(2.0, 2.0, 2.0, 2.0);
                u_xlat9.z = u_xlat8.y * 0.0816320032;
                u_xlat11.xy = u_xlat49.yx * float2(0.0816320032, 0.0816320032);
                u_xlat49.xy = u_xlat8.xz * float2(0.0816320032, 0.0816320032);
                u_xlat11.z = u_xlat8.w * 0.0816320032;
                u_xlat9.x = u_xlat11.y;
                u_xlat9.yw = u_xlat7.xx * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat8.xz = u_xlat7.xx * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat8.y = u_xlat49.x;
                u_xlat8.w = u_xlat10.y;
                u_xlat9 = u_xlat8 + u_xlat9;
                u_xlat11.yw = u_xlat7.yy * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat10.xz = u_xlat7.yy * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat10.y = u_xlat49.y;
                u_xlat7 = u_xlat10 + u_xlat11;
                u_xlat8 = u_xlat8 / u_xlat9;
                u_xlat8 = u_xlat8 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat10 = u_xlat10 / u_xlat7;
                u_xlat10 = u_xlat10 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat8 = u_xlat8.wxyz * _pad448.xxxx;
                u_xlat10 = u_xlat10.xwyz * _pad448.yyyy;
                u_xlat11.xzw = u_xlat8.yzw;
                u_xlat11.y = u_xlat10.x;
                u_xlat12 = u_xlat27.xyxy * _pad448.xyxy + u_xlat11.xyzy;
                u_xlat13.xy = u_xlat27.xy * _pad448.xy + u_xlat11.wy;
                u_xlat8.y = u_xlat11.y;
                u_xlat11.y = u_xlat10.z;
                u_xlat14 = u_xlat27.xyxy * _pad448.xyxy + u_xlat11.xyzy;
                u_xlat55.xy = u_xlat27.xy * _pad448.xy + u_xlat11.wy;
                u_xlat8.z = u_xlat11.y;
                u_xlat15 = u_xlat27.xyxy * _pad448.xyxy + u_xlat8.xyxz;
                u_xlat11.y = u_xlat10.w;
                u_xlat16 = u_xlat27.xyxy * _pad448.xyxy + u_xlat11.xyzy;
                u_xlat29.xy = u_xlat27.xy * _pad448.xy + u_xlat11.wy;
                u_xlat8.w = u_xlat11.y;
                u_xlat17.xy = u_xlat27.xy * _pad448.xy + u_xlat8.xw;
                u_xlat10.xzw = u_xlat11.xzw;
                u_xlat11 = u_xlat27.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat52.xy = u_xlat27.xy * _pad448.xy + u_xlat10.wy;
                u_xlat10.x = u_xlat8.x;
                u_xlat27.xy = u_xlat27.xy * _pad448.xy + u_xlat10.xy;
                u_xlat18 = u_xlat7.xxxx * u_xlat9;
                u_xlat19 = u_xlat7.yyyy * u_xlat9;
                u_xlat20 = u_xlat7.zzzz * u_xlat9;
                u_xlat7 = u_xlat7.wwww * u_xlat9;
                float3 txVec13 = float3(u_xlat12.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec13, 0.0);
                float3 txVec14 = float3(u_xlat12.zw,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec14, 0.0);
                u_xlat8.x = u_xlat8.x * u_xlat18.y;
                u_xlat69 = u_xlat18.x * u_xlat69 + u_xlat8.x;
                float3 txVec15 = float3(u_xlat13.xy,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec15, 0.0);
                u_xlat69 = u_xlat18.z * u_xlat8.x + u_xlat69;
                float3 txVec16 = float3(u_xlat15.xy,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec16, 0.0);
                u_xlat69 = u_xlat18.w * u_xlat8.x + u_xlat69;
                float3 txVec17 = float3(u_xlat14.xy,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec17, 0.0);
                u_xlat69 = u_xlat19.x * u_xlat8.x + u_xlat69;
                float3 txVec18 = float3(u_xlat14.zw,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec18, 0.0);
                u_xlat69 = u_xlat19.y * u_xlat8.x + u_xlat69;
                float3 txVec19 = float3(u_xlat55.xy,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec19, 0.0);
                u_xlat69 = u_xlat19.z * u_xlat8.x + u_xlat69;
                float3 txVec20 = float3(u_xlat15.zw,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec20, 0.0);
                u_xlat69 = u_xlat19.w * u_xlat8.x + u_xlat69;
                float3 txVec21 = float3(u_xlat16.xy,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec21, 0.0);
                u_xlat69 = u_xlat20.x * u_xlat8.x + u_xlat69;
                float3 txVec22 = float3(u_xlat16.zw,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec22, 0.0);
                u_xlat69 = u_xlat20.y * u_xlat8.x + u_xlat69;
                float3 txVec23 = float3(u_xlat29.xy,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec23, 0.0);
                u_xlat69 = u_xlat20.z * u_xlat8.x + u_xlat69;
                float3 txVec24 = float3(u_xlat17.xy,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec24, 0.0);
                u_xlat69 = u_xlat20.w * u_xlat8.x + u_xlat69;
                float3 txVec25 = float3(u_xlat11.xy,u_xlat3.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec25, 0.0);
                u_xlat69 = u_xlat7.x * u_xlat8.x + u_xlat69;
                float3 txVec26 = float3(u_xlat11.zw,u_xlat3.z);
                u_xlat7.x = _g_textureLod(_Texture_t5, txVec26, 0.0);
                u_xlat69 = u_xlat7.y * u_xlat7.x + u_xlat69;
                float3 txVec27 = float3(u_xlat52.xy,u_xlat3.z);
                u_xlat7.x = _g_textureLod(_Texture_t5, txVec27, 0.0);
                u_xlat69 = u_xlat7.z * u_xlat7.x + u_xlat69;
                float3 txVec28 = float3(u_xlat27.xy,u_xlat3.z);
                u_xlat27.x = _g_textureLod(_Texture_t5, txVec28, 0.0);
                u_xlat6.x = u_xlat7.w * u_xlat27.x + u_xlat69;
            }
        }
    } else {
        float3 txVec29 = float3(u_xlat3.xy,u_xlat3.z);
        u_xlat6.x = _g_textureLod(_Texture_t5, txVec29, 0.0);
    }
    u_xlat3.x = (-_MainLightShadowParams.x) + 1.0;
    u_xlat3.x = u_xlat6.x * _MainLightShadowParams.x + u_xlat3.x;
    u_xlatb24.x = 0.0>=u_xlat3.z;
    u_xlatb45 = u_xlat3.z>=1.0;
    u_xlatb24.x = u_xlatb45 || u_xlatb24.x;
    u_xlat3.x = (u_xlatb24.x) ? 1.0 : u_xlat3.x;
    u_xlat6.xyz = input.vs_INTERP10.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat24.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat24.x = u_xlat24.x * _MainLightShadowParams.z + _MainLightShadowParams.w;
    u_xlat24.x = clamp(u_xlat24.x, 0.0, 1.0);
    u_xlat45 = (-u_xlat3.x) + 1.0;
    u_xlat3.x = u_xlat24.x * u_xlat45 + u_xlat3.x;
    u_xlatb24.x = _pad176.y!=-1.0;
    if(u_xlatb24.x){
        u_xlat24.xy = input.vs_INTERP10.yy * _pad16.xy;
        u_xlat24.xy = _pad0.xy * input.vs_INTERP10.xx + u_xlat24.xy;
        u_xlat24.xy = _pad32.xy * input.vs_INTERP10.zz + u_xlat24.xy;
        u_xlat24.xy = u_xlat24.xy + _pad48.xy;
        u_xlat24.xy = u_xlat24.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
        u_xlat6 = _g_texture(_Texture_t6, u_xlat24.xy, _GlobalMipBias.x);
        u_xlatb24.xy = _g_equal(_pad176.yyyy, float4(0.0, 1.0, 0.0, 0.0)).xy;
        u_xlat45 = (u_xlatb24.y) ? u_xlat6.w : u_xlat6.x;
        u_xlat6.xyz = (u_xlatb24.x) ? u_xlat6.xyz : float3(u_xlat45);
    } else {
        u_xlat6.x = float(1.0);
        u_xlat6.y = float(1.0);
        u_xlat6.z = float(1.0);
    }
    u_xlat6.xyz = u_xlat6.xyz * _MainLightColor.xyz;
    u_xlat24.x = dot((-u_xlat1.xyz), u_xlat2.xyz);
    u_xlat24.x = u_xlat24.x + u_xlat24.x;
    u_xlat7.xyz = u_xlat2.xyz * (-u_xlat24.xxx) + (-u_xlat1.xyz);
    u_xlat24.x = dot(u_xlat2.xyz, u_xlat1.xyz);
    u_xlat24.x = clamp(u_xlat24.x, 0.0, 1.0);
    u_xlat24.x = (-u_xlat24.x) + 1.0;
    u_xlat24.x = u_xlat24.x * u_xlat24.x;
    u_xlat24.x = u_xlat24.x * u_xlat24.x;
    u_xlat45 = (-u_xlat64) * 0.699999988 + 1.70000005;
    u_xlat64 = u_xlat64 * u_xlat45;
    u_xlat64 = u_xlat64 * 6.0;
    u_xlat8.xyz = unity_SpecCube0_BoxMax.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat9.xyz = u_xlat8.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube0_BoxMin.xyz;
    u_xlat10.xyz = (-u_xlat9.xyz) + input.vs_INTERP10.xyz;
    u_xlat11 = unity_SpecCube0_Rotation.zzxy + unity_SpecCube0_Rotation.zzxy;
    u_xlat12.xyz = u_xlat11.zwy * unity_SpecCube0_Rotation.xyz;
    u_xlat13 = u_xlat11 * unity_SpecCube0_Rotation.xyww;
    u_xlat45 = u_xlat11.y * unity_SpecCube0_Rotation.w;
    u_xlat12.xyz = u_xlat12.zzy + u_xlat12.yxx;
    u_xlat12.xyz = (-u_xlat12.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat14.xy = u_xlat10.xy * u_xlat12.xy;
    u_xlat69 = unity_SpecCube0_Rotation.x * u_xlat11.w + (-u_xlat45);
    u_xlat70 = u_xlat69 * u_xlat10.y + u_xlat14.x;
    u_xlat13.xy = u_xlat13.wz + u_xlat13.xy;
    u_xlat71 = u_xlat10.y * u_xlat13.y;
    u_xlat15.x = u_xlat13.x * u_xlat10.z + u_xlat70;
    u_xlat45 = unity_SpecCube0_Rotation.x * u_xlat11.w + u_xlat45;
    u_xlat70 = u_xlat45 * u_xlat10.x + u_xlat14.y;
    u_xlat31.xz = unity_SpecCube0_Rotation.yx * u_xlat11.yx + (-u_xlat13.zw);
    u_xlat15.y = u_xlat31.x * u_xlat10.z + u_xlat70;
    u_xlat70 = u_xlat31.z * u_xlat10.x + u_xlat71;
    u_xlat15.z = u_xlat12.z * u_xlat10.z + u_xlat70;
    u_xlat9.xyz = u_xlat9.xyz + u_xlat15.xyz;
    u_xlat11.xyz = unity_SpecCube1_BoxMax.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat14.xyz = u_xlat11.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube1_BoxMin.xyz;
    u_xlat15.xyz = (-u_xlat14.xyz) + input.vs_INTERP10.xyz;
    u_xlat16 = unity_SpecCube1_Rotation.zzxy + unity_SpecCube1_Rotation.zzxy;
    u_xlat17.xyz = u_xlat16.zwy * unity_SpecCube1_Rotation.xyz;
    u_xlat18 = u_xlat16 * unity_SpecCube1_Rotation.xyww;
    u_xlat70 = u_xlat16.y * unity_SpecCube1_Rotation.w;
    u_xlat17.xyz = u_xlat17.zzy + u_xlat17.yxx;
    u_xlat17.xyz = (-u_xlat17.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat10.xz = u_xlat15.xy * u_xlat17.xy;
    u_xlat71 = unity_SpecCube1_Rotation.x * u_xlat16.w + (-u_xlat70);
    u_xlat72 = u_xlat71 * u_xlat15.y + u_xlat10.x;
    u_xlat55.xy = u_xlat18.wz + u_xlat18.xy;
    u_xlat10.x = u_xlat15.y * u_xlat55.y;
    u_xlat19.x = u_xlat55.x * u_xlat15.z + u_xlat72;
    u_xlat70 = unity_SpecCube1_Rotation.x * u_xlat16.w + u_xlat70;
    u_xlat72 = u_xlat70 * u_xlat15.x + u_xlat10.z;
    u_xlat36.xz = unity_SpecCube1_Rotation.yx * u_xlat16.yx + (-u_xlat18.zw);
    u_xlat19.y = u_xlat36.x * u_xlat15.z + u_xlat72;
    u_xlat72 = u_xlat36.z * u_xlat15.x + u_xlat10.x;
    u_xlat19.z = u_xlat17.z * u_xlat15.z + u_xlat72;
    u_xlat14.xyz = u_xlat14.xyz + u_xlat19.xyz;
    u_xlat8.x = dot(u_xlat8.xyz, u_xlat8.xyz);
    u_xlat29.x = dot(u_xlat11.xyz, u_xlat11.xyz);
    u_xlat8.x = (-u_xlat29.x) + u_xlat8.x;
    u_xlatb29 = 0.0<unity_SpecCube1_BoxMin.w;
    u_xlatb50 = unity_SpecCube1_BoxMin.w==0.0;
    u_xlatb72 = u_xlat8.x<-9.99999975e-05;
    u_xlatb72 = u_xlatb50 && u_xlatb72;
    u_xlatb29 = u_xlatb29 || u_xlatb72;
    u_xlatb72 = unity_SpecCube1_BoxMin.w<0.0;
    u_xlatb8 = 9.99999975e-05<u_xlat8.x;
    u_xlatb8 = u_xlatb8 && u_xlatb50;
    u_xlatb8 = u_xlatb8 || u_xlatb72;
    u_xlat11.xyz = u_xlat9.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat16.xyz = (-u_xlat9.xyz) + unity_SpecCube0_BoxMax.xyz;
    u_xlat11.xyz = min(u_xlat11.xyz, u_xlat16.xyz);
    u_xlat11.xyz = u_xlat11.xyz / unity_SpecCube0_BoxMax.www;
    u_xlat50 = min(u_xlat11.z, u_xlat11.y);
    u_xlat50 = min(u_xlat50, u_xlat11.x);
    u_xlat50 = clamp(u_xlat50, 0.0, 1.0);
    u_xlat11.xyz = u_xlat14.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat16.xyz = (-u_xlat14.xyz) + unity_SpecCube1_BoxMax.xyz;
    u_xlat11.xyz = min(u_xlat11.xyz, u_xlat16.xyz);
    u_xlat11.xyz = u_xlat11.xyz / unity_SpecCube1_BoxMax.www;
    u_xlat72 = min(u_xlat11.z, u_xlat11.y);
    u_xlat72 = min(u_xlat72, u_xlat11.x);
    u_xlat72 = clamp(u_xlat72, 0.0, 1.0);
    u_xlat10.x = (-u_xlat72) + 1.0;
    u_xlat10.x = min(u_xlat50, u_xlat10.x);
    u_xlat8.x = (u_xlatb8) ? u_xlat10.x : u_xlat50;
    u_xlat50 = (-u_xlat50) + 1.0;
    u_xlat50 = min(u_xlat50, u_xlat72);
    u_xlat8.y = (u_xlatb29) ? u_xlat50 : u_xlat72;
    u_xlat50 = u_xlat8.y + u_xlat8.x;
    u_xlat72 = max(u_xlat50, 1.0);
    u_xlat8.xy = u_xlat8.xy / float2(u_xlat72);
    u_xlatb72 = 0.00999999978<u_xlat8.x;
    if(u_xlatb72){
        u_xlat10.xz = u_xlat7.xy * u_xlat12.xy;
        u_xlat69 = u_xlat69 * u_xlat7.y + u_xlat10.x;
        u_xlat72 = u_xlat7.y * u_xlat13.y;
        u_xlat11.x = u_xlat13.x * u_xlat7.z + u_xlat69;
        u_xlat45 = u_xlat45 * u_xlat7.x + u_xlat10.z;
        u_xlat11.y = u_xlat31.x * u_xlat7.z + u_xlat45;
        u_xlat45 = u_xlat31.z * u_xlat7.x + u_xlat72;
        u_xlat11.z = u_xlat12.z * u_xlat7.z + u_xlat45;
        u_xlatb45 = 0.0<unity_SpecCube0_ProbePosition.w;
        u_xlatb10.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat11.xyzx).xyz;
        u_xlat10.x = (u_xlatb10.x) ? unity_SpecCube0_BoxMax.x : unity_SpecCube0_BoxMin.x;
        u_xlat10.y = (u_xlatb10.y) ? unity_SpecCube0_BoxMax.y : unity_SpecCube0_BoxMin.y;
        u_xlat10.z = (u_xlatb10.z) ? unity_SpecCube0_BoxMax.z : unity_SpecCube0_BoxMin.z;
        u_xlat10.xyz = (-u_xlat9.xyz) + u_xlat10.xyz;
        u_xlat10.xyz = u_xlat10.xyz / u_xlat11.xyz;
        u_xlat69 = min(u_xlat10.y, u_xlat10.x);
        u_xlat69 = min(u_xlat10.z, u_xlat69);
        u_xlat9.xyz = u_xlat9.xyz + (-unity_SpecCube0_ProbePosition.xyz);
        u_xlat9.xyz = u_xlat11.xyz * float3(u_xlat69) + u_xlat9.xyz;
        u_xlat9.xyz = (bool(u_xlatb45)) ? u_xlat9.xyz : u_xlat11.xyz;
        u_xlat10.xyz = unity_SpecCube0_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat11.xyz = u_xlat10.yzx * (-unity_SpecCube0_Rotation.xyz);
        u_xlat12.xyz = u_xlat10.xyz * unity_SpecCube0_Rotation.www;
        u_xlat11.xyz = u_xlat11.zzy + u_xlat11.yxx;
        u_xlat11.xyz = (-u_xlat11.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat31.xz = u_xlat9.xy * u_xlat11.xy;
        u_xlat45 = (-unity_SpecCube0_Rotation.x) * u_xlat10.z + (-u_xlat12.x);
        u_xlat45 = u_xlat45 * u_xlat9.y + u_xlat31.x;
        u_xlat11.xy = (-unity_SpecCube0_Rotation.xy) * u_xlat10.xx + u_xlat12.zy;
        u_xlat69 = u_xlat9.y * u_xlat11.y;
        u_xlat16.x = u_xlat11.x * u_xlat9.z + u_xlat45;
        u_xlat45 = (-unity_SpecCube0_Rotation.x) * u_xlat10.z + u_xlat12.x;
        u_xlat45 = u_xlat45 * u_xlat9.x + u_xlat31.z;
        u_xlat30.xz = (-unity_SpecCube0_Rotation.yx) * u_xlat10.xx + (-u_xlat12.yz);
        u_xlat16.y = u_xlat30.x * u_xlat9.z + u_xlat45;
        u_xlat45 = u_xlat30.z * u_xlat9.x + u_xlat69;
        u_xlat16.z = u_xlat11.z * u_xlat9.z + u_xlat45;
        u_xlat9 = _g_textureLod(_Texture_t1, u_xlat16.xyz, u_xlat64);
        u_xlat45 = u_xlat9.w + -1.0;
        u_xlat45 = unity_SpecCube0_HDR.w * u_xlat45 + 1.0;
        u_xlat45 = max(u_xlat45, 0.0);
        u_xlat45 = log2(u_xlat45);
        u_xlat45 = u_xlat45 * unity_SpecCube0_HDR.y;
        u_xlat45 = exp2(u_xlat45);
        u_xlat45 = u_xlat45 * unity_SpecCube0_HDR.x;
        u_xlat9.xyz = u_xlat9.xyz * float3(u_xlat45);
        u_xlat9.xyz = u_xlat8.xxx * u_xlat9.xyz;
    } else {
        u_xlat9.x = float(0.0);
        u_xlat9.y = float(0.0);
        u_xlat9.z = float(0.0);
    }
    u_xlatb45 = 0.00999999978<u_xlat8.y;
    if(u_xlatb45){
        u_xlat10.xy = u_xlat7.xy * u_xlat17.xy;
        u_xlat45 = u_xlat71 * u_xlat7.y + u_xlat10.x;
        u_xlat69 = u_xlat7.y * u_xlat55.y;
        u_xlat11.x = u_xlat55.x * u_xlat7.z + u_xlat45;
        u_xlat45 = u_xlat70 * u_xlat7.x + u_xlat10.y;
        u_xlat11.y = u_xlat36.x * u_xlat7.z + u_xlat45;
        u_xlat45 = u_xlat36.z * u_xlat7.x + u_xlat69;
        u_xlat11.z = u_xlat17.z * u_xlat7.z + u_xlat45;
        u_xlatb45 = 0.0<unity_SpecCube1_ProbePosition.w;
        u_xlatb10.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat11.xyzx).xyz;
        u_xlat10.x = (u_xlatb10.x) ? unity_SpecCube1_BoxMax.x : unity_SpecCube1_BoxMin.x;
        u_xlat10.y = (u_xlatb10.y) ? unity_SpecCube1_BoxMax.y : unity_SpecCube1_BoxMin.y;
        u_xlat10.z = (u_xlatb10.z) ? unity_SpecCube1_BoxMax.z : unity_SpecCube1_BoxMin.z;
        u_xlat10.xyz = (-u_xlat14.xyz) + u_xlat10.xyz;
        u_xlat10.xyz = u_xlat10.xyz / u_xlat11.xyz;
        u_xlat69 = min(u_xlat10.y, u_xlat10.x);
        u_xlat69 = min(u_xlat10.z, u_xlat69);
        u_xlat10.xyz = u_xlat14.xyz + (-unity_SpecCube1_ProbePosition.xyz);
        u_xlat10.xyz = u_xlat11.xyz * float3(u_xlat69) + u_xlat10.xyz;
        u_xlat10.xyz = (bool(u_xlatb45)) ? u_xlat10.xyz : u_xlat11.xyz;
        u_xlat11.xyz = unity_SpecCube1_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat12.xyz = u_xlat11.yzx * (-unity_SpecCube1_Rotation.xyz);
        u_xlat13.xyz = u_xlat11.xyz * unity_SpecCube1_Rotation.www;
        u_xlat12.xyz = u_xlat12.zzy + u_xlat12.yxx;
        u_xlat12.xyz = (-u_xlat12.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat8.xw = u_xlat10.xy * u_xlat12.xy;
        u_xlat45 = (-unity_SpecCube1_Rotation.x) * u_xlat11.z + (-u_xlat13.x);
        u_xlat45 = u_xlat45 * u_xlat10.y + u_xlat8.x;
        u_xlat32.xz = (-unity_SpecCube1_Rotation.xy) * u_xlat11.xx + u_xlat13.zy;
        u_xlat69 = u_xlat10.y * u_xlat32.z;
        u_xlat14.x = u_xlat32.x * u_xlat10.z + u_xlat45;
        u_xlat45 = (-unity_SpecCube1_Rotation.x) * u_xlat11.z + u_xlat13.x;
        u_xlat45 = u_xlat45 * u_xlat10.x + u_xlat8.w;
        u_xlat8.xw = (-unity_SpecCube1_Rotation.yx) * u_xlat11.xx + (-u_xlat13.yz);
        u_xlat14.y = u_xlat8.x * u_xlat10.z + u_xlat45;
        u_xlat45 = u_xlat8.w * u_xlat10.x + u_xlat69;
        u_xlat14.z = u_xlat12.z * u_xlat10.z + u_xlat45;
        u_xlat10 = _g_textureLod(_Texture_t2, u_xlat14.xyz, u_xlat64);
        u_xlat45 = u_xlat10.w + -1.0;
        u_xlat45 = unity_SpecCube1_HDR.w * u_xlat45 + 1.0;
        u_xlat45 = max(u_xlat45, 0.0);
        u_xlat45 = log2(u_xlat45);
        u_xlat45 = u_xlat45 * unity_SpecCube1_HDR.y;
        u_xlat45 = exp2(u_xlat45);
        u_xlat45 = u_xlat45 * unity_SpecCube1_HDR.x;
        u_xlat10.xyz = u_xlat10.xyz * float3(u_xlat45);
        u_xlat9.xyz = u_xlat8.yyy * u_xlat10.xyz + u_xlat9.xyz;
    }
    u_xlatb45 = u_xlat50<0.99000001;
    if(u_xlatb45){
        u_xlat7 = _g_textureLod(_Texture_t0, u_xlat7.xyz, u_xlat64);
        u_xlat64 = (-u_xlat50) + 1.0;
        u_xlat45 = u_xlat7.w + -1.0;
        u_xlat45 = _GlossyEnvironmentCubeMap_HDR.w * u_xlat45 + 1.0;
        u_xlat45 = max(u_xlat45, 0.0);
        u_xlat45 = log2(u_xlat45);
        u_xlat45 = u_xlat45 * _GlossyEnvironmentCubeMap_HDR.y;
        u_xlat45 = exp2(u_xlat45);
        u_xlat45 = u_xlat45 * _GlossyEnvironmentCubeMap_HDR.x;
        u_xlat7.xyz = u_xlat7.xyz * float3(u_xlat45);
        u_xlat9.xyz = float3(u_xlat64) * u_xlat7.xyz + u_xlat9.xyz;
    }
    u_xlat7.xy = float2(u_xlat65) * float2(u_xlat65) + float2(-1.0, 1.0);
    u_xlat64 = float(1.0) / u_xlat7.y;
    u_xlat28.xyz = (-u_xlat0.xyz) + float3(u_xlat67);
    u_xlat28.xyz = u_xlat24.xxx * u_xlat28.xyz + u_xlat0.xyz;
    u_xlat28.xyz = float3(u_xlat64) * u_xlat28.xyz;
    u_xlat28.xyz = u_xlat28.xyz * u_xlat9.xyz;
    u_xlat4.xyz = u_xlat4.xyz * u_xlat5.xyz + u_xlat28.xyz;
    u_xlati64 = int(uint(uint(_g_floatBitsToUint(_MainLightLayerMask)) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
    u_xlat65 = u_xlat3.x * unity_LightData.z;
    u_xlat3.x = dot(u_xlat2.xyz, _MainLightPosition.xyz);
    u_xlat3.x = clamp(u_xlat3.x, 0.0, 1.0);
    u_xlat65 = u_xlat65 * u_xlat3.x;
    u_xlat3.xyz = float3(u_xlat65) * u_xlat6.xyz;
    u_xlat6.xyz = u_xlat1.xyz + _MainLightPosition.xyz;
    u_xlat65 = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat65 = max(u_xlat65, 1.17549435e-38);
    u_xlat65 = _g_inversesqrt(u_xlat65);
    u_xlat6.xyz = float3(u_xlat65) * u_xlat6.xyz;
    u_xlat65 = dot(u_xlat2.xyz, u_xlat6.xyz);
    u_xlat65 = clamp(u_xlat65, 0.0, 1.0);
    u_xlat67 = dot(_MainLightPosition.xyz, u_xlat6.xyz);
    u_xlat67 = clamp(u_xlat67, 0.0, 1.0);
    u_xlat65 = u_xlat65 * u_xlat65;
    u_xlat65 = u_xlat65 * u_xlat7.x + 1.00001001;
    u_xlat67 = u_xlat67 * u_xlat67;
    u_xlat65 = u_xlat65 * u_xlat65;
    u_xlat67 = max(u_xlat67, 0.100000001);
    u_xlat65 = u_xlat65 * u_xlat67;
    u_xlat65 = u_xlat68 * u_xlat65;
    u_xlat65 = u_xlat66 / u_xlat65;
    u_xlat6.xyz = u_xlat0.xyz * float3(u_xlat65) + u_xlat5.xyz;
    u_xlat3.xyz = u_xlat3.xyz * u_xlat6.xyz;
    u_xlat3.xyz = (int(u_xlati64) != 0) ? u_xlat3.xyz : float3(0.0, 0.0, 0.0);
    u_xlat64 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu64 =  uint(int(u_xlat64));
    u_xlatb6.xy = _g_equal(_pad176.zzzz, float4(0.0, 1.0, 0.0, 0.0)).xy;
    u_xlat28.x = float(0.0);
    u_xlat28.y = float(0.0);
    u_xlat28.z = float(0.0);
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<u_xlatu64 ; u_xlatu_loop_1++)
    {
        u_xlatu67 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati48 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat67 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati48]);
        u_xlatu67 =  uint(int(u_xlat67));
        u_xlat8.xyz = (-input.vs_INTERP10.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat48.x = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat48.x = max(u_xlat48.x, 6.10351562e-05);
        u_xlat69 = _g_inversesqrt(u_xlat48.x);
        u_xlat9.xyz = float3(u_xlat69) * u_xlat8.xyz;
        u_xlat71 = float(1.0) / float(u_xlat48.x);
        u_xlat48.x = u_xlat48.x * _AdditionalLightsAttenuation.x;
        u_xlat48.x = (-u_xlat48.x) * u_xlat48.x + 1.0;
        u_xlat48.x = max(u_xlat48.x, 0.0);
        u_xlat48.x = u_xlat48.x * u_xlat48.x;
        u_xlat48.x = u_xlat48.x * u_xlat71;
        u_xlat71 = dot(_AdditionalLightsSpotDir.xyz, u_xlat9.xyz);
        u_xlat71 = u_xlat71 * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat71 = clamp(u_xlat71, 0.0, 1.0);
        u_xlat71 = u_xlat71 * u_xlat71;
        u_xlat48.x = u_xlat48.x * u_xlat71;
        u_xlatu71 = uint(u_xlatu67 >> 5u);
        u_xlati72 = int(1 << int(u_xlatu67));
        u_xlati71 = int(uint(uint(u_xlati72) & uint(_g_floatBitsToUint(_pad64.x))));
        if(u_xlati71 != 0) {
            u_xlati71 = int(_pad20672.x);
            u_xlati72 = (u_xlati71 != 0) ? 0 : 1;
            u_xlati10 = int(int(u_xlatu67) << 2);
            if(u_xlati72 != 0) {
                u_xlat31.xyz = input.vs_INTERP10.yyy * _pad208.xyw;
                u_xlat31.xyz = _pad192.xyw * input.vs_INTERP10.xxx + u_xlat31.xyz;
                u_xlat31.xyz = _pad224.xyw * input.vs_INTERP10.zzz + u_xlat31.xyz;
                u_xlat31.xyz = u_xlat31.xyz + _pad240.xyw;
                u_xlat31.xy = u_xlat31.xy / u_xlat31.zz;
                u_xlat31.xy = u_xlat31.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                u_xlat31.xy = clamp(u_xlat31.xy, 0.0, 1.0);
                u_xlat31.xy = _pad16576.xy * u_xlat31.xy + _pad16576.zw;
            } else {
                u_xlatb71 = u_xlati71==1;
                u_xlati71 = u_xlatb71 ? 1 : int(0);
                if(u_xlati71 != 0) {
                    u_xlat11.xy = input.vs_INTERP10.yy * _pad208.xy;
                    u_xlat11.xy = _pad192.xy * input.vs_INTERP10.xx + u_xlat11.xy;
                    u_xlat11.xy = _pad224.xy * input.vs_INTERP10.zz + u_xlat11.xy;
                    u_xlat11.xy = u_xlat11.xy + _pad240.xy;
                    u_xlat11.xy = u_xlat11.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat11.xy = _g_fract(u_xlat11.xy);
                    u_xlat31.xy = _pad16576.xy * u_xlat11.xy + _pad16576.zw;
                } else {
                    u_xlat11 = input.vs_INTERP10.yyyy * _pad208;
                    u_xlat11 = _pad192 * input.vs_INTERP10.xxxx + u_xlat11;
                    u_xlat11 = _pad224 * input.vs_INTERP10.zzzz + u_xlat11;
                    u_xlat11 = u_xlat11 + _pad240;
                    u_xlat11.xyz = u_xlat11.xyz / u_xlat11.www;
                    u_xlat71 = dot(u_xlat11.xyz, u_xlat11.xyz);
                    u_xlat71 = _g_inversesqrt(u_xlat71);
                    u_xlat11.xyz = float3(u_xlat71) * u_xlat11.xyz;
                    u_xlat71 = dot(abs(u_xlat11.xyz), float3(1.0, 1.0, 1.0));
                    u_xlat71 = max(u_xlat71, 9.99999997e-07);
                    u_xlat71 = float(1.0) / float(u_xlat71);
                    u_xlat12.xyz = float3(u_xlat71) * u_xlat11.zxy;
                    u_xlat12.x = (-u_xlat12.x);
                    u_xlat12.x = clamp(u_xlat12.x, 0.0, 1.0);
                    u_xlatb10.xw = _g_greaterThanEqual(u_xlat12.yyyz, float4(0.0, 0.0, 0.0, 0.0)).xw;
                    u_xlat10.x = (u_xlatb10.x) ? u_xlat12.x : (-u_xlat12.x);
                    u_xlat10.w = (u_xlatb10.w) ? u_xlat12.x : (-u_xlat12.x);
                    u_xlat10.xw = u_xlat11.xy * float2(u_xlat71) + u_xlat10.xw;
                    u_xlat10.xw = u_xlat10.xw * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat10.xw = clamp(u_xlat10.xw, 0.0, 1.0);
                    u_xlat31.xy = _pad16576.xy * u_xlat10.xw + _pad16576.zw;
                }
            }
            u_xlat10 = _g_textureLod(_Texture_t7, u_xlat31.xy, 0.0);
            u_xlat71 = (u_xlatb6.y) ? u_xlat10.w : u_xlat10.x;
            u_xlat10.xyz = (u_xlatb6.x) ? u_xlat10.xyz : float3(u_xlat71);
        } else {
            u_xlat10.x = float(1.0);
            u_xlat10.y = float(1.0);
            u_xlat10.z = float(1.0);
        }
        u_xlat10.xyz = u_xlat10.xyz * _AdditionalLightsColor.xyz;
        u_xlati67 = int(uint(uint(_g_floatBitsToUint(unity_RenderingLayer.x)) & uint(_g_floatBitsToUint(_AdditionalLightsLayerMasks))));
        u_xlat71 = dot(u_xlat2.xyz, u_xlat9.xyz);
        u_xlat71 = clamp(u_xlat71, 0.0, 1.0);
        u_xlat48.x = u_xlat48.x * u_xlat71;
        u_xlat10.xyz = u_xlat48.xxx * u_xlat10.xyz;
        u_xlat8.xyz = u_xlat8.xyz * float3(u_xlat69) + u_xlat1.xyz;
        u_xlat48.x = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat48.x = max(u_xlat48.x, 1.17549435e-38);
        u_xlat48.x = _g_inversesqrt(u_xlat48.x);
        u_xlat8.xyz = u_xlat48.xxx * u_xlat8.xyz;
        u_xlat48.x = dot(u_xlat2.xyz, u_xlat8.xyz);
        u_xlat48.x = clamp(u_xlat48.x, 0.0, 1.0);
        u_xlat48.y = dot(u_xlat9.xyz, u_xlat8.xyz);
        u_xlat48.y = clamp(u_xlat48.y, 0.0, 1.0);
        u_xlat48.xy = u_xlat48.xy * u_xlat48.xy;
        u_xlat48.x = u_xlat48.x * u_xlat7.x + 1.00001001;
        u_xlat48.x = u_xlat48.x * u_xlat48.x;
        u_xlat69 = max(u_xlat48.y, 0.100000001);
        u_xlat48.x = u_xlat69 * u_xlat48.x;
        u_xlat48.x = u_xlat68 * u_xlat48.x;
        u_xlat48.x = u_xlat66 / u_xlat48.x;
        u_xlat8.xyz = u_xlat0.xyz * u_xlat48.xxx + u_xlat5.xyz;
        u_xlat8.xyz = u_xlat8.xyz * u_xlat10.xyz + u_xlat28.xyz;
        u_xlat28.xyz = (int(u_xlati67) != 0) ? u_xlat8.xyz : u_xlat28.xyz;
    }
    u_xlat0.xyz = u_xlat3.xyz + u_xlat4.xyz;
    u_xlat0.xyz = u_xlat28.xyz + u_xlat0.xyz;
    u_xlat63 = u_xlat63 * (-u_xlat63);
    u_xlat63 = exp2(u_xlat63);
    u_xlat1.x = (-u_xlat63) + 1.0;
    u_xlat1.xyz = u_xlat1.xxx * _pad992.xyz;
    __SV_Target0.xyz = u_xlat0.xyz * float3(u_xlat63) + u_xlat1.xyz;
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
