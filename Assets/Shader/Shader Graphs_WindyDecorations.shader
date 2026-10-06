Shader "Shader Graphs/WindyDecorations"
{
    Properties
    {



_Color ("Color", Vector) = (0.3803922,0.454902,0.282353,1)
_TopColor ("TopColor", Vector) = (0.5882353,0.6235294,0.2980392,1)
_Smoothness ("Smoothness", Float) = 0
[NoScaleOffset] _Texture ("Texture", 2D) = "white" {}
_WindColorMulti ("WindColorMulti", Float) = 0.5
[HideInInspector] _WindSpeed ("WindSpeed", Float) = 2.8
[HideInInspector] _WindSpeed2 ("WindSpeed2", Float) = 0.5
[HideInInspector] _WindScale ("WindScale", Float) = 0.15
[HideInInspector] _WindDensity ("WindDensity", Float) = 0.2
[NoScaleOffset] _Noise_Map ("Noise Map", 2D) = "white" {}
_Noise_Map_Scale ("Noise Map Scale", Vector) = (0.04,0.02,0,0)
_Noise_Remap ("Noise Remap", Vector) = (0.9,1,0,0)
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
            TEXTURE2D(_Noise_Map);
            SAMPLER(sampler__Noise_Map);
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
            float4 _pad80;
            float4 _pad96;
            float4 _pad112;
            float4 _pad128;
            float4 _pad176;
            float4 _pad192;
            float4 _pad208;
            float4 _pad224;
            float4 _pad240;
            float4 _TimeParameters;
            float4 _pad352;
            float4 _pad400;
            float4 _pad416;
            float4 _pad432;
            float4 _pad448;
            float4 _pad976;
            float4 _pad992;
            float4x4 unity_MatrixVP;
            float _WindSpeed;
            float _WindDensity;
            float _WindScale;
            float _WindSpeed2;
            float4 _GlossyEnvironmentCubeMap_HDR;
            float2 _GlobalMipBias;
            float _AlphaToMaskAvailable;
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
            float _Smoothness;

            float4 u_xlat0;
            int u_xlati0;
            float4 u_xlat1;
            float4 u_xlat2;
            int2 u_xlati2;
            uint u_xlatu2;
            float4 u_xlat3;
            int4 u_xlati3;
            uint2 u_xlatu3;
            float4 u_xlat4;
            int u_xlati5;
            float2 u_xlat6;
            float2 u_xlat7;
            int2 u_xlati7;
            uint2 u_xlatu7;
            float3 u_xlat8;
            float2 u_xlat11;
            int2 u_xlati11;
            uint u_xlatu11;
            float2 u_xlat12;
            int2 u_xlati12;
            uint2 u_xlatu12;
            float2 u_xlat14;
            float u_xlat16;
            int u_xlati16;
            uint u_xlatu16;
            float4 ImmCB_0_0_0[4];
            bool u_xlatb2;
            float4 u_xlat5;
            bool2 u_xlatb6;
            bool u_xlatb8;
            float4 u_xlat9;
            float4 u_xlat10;
            int u_xlati10;
            bool4 u_xlatb10;
            float4 u_xlat13;
            float4 u_xlat15;
            float4 u_xlat17;
            float4 u_xlat18;
            float4 u_xlat19;
            float3 u_xlat20;
            float u_xlat21;
            float3 u_xlat22;
            bool u_xlatb22;
            float u_xlat23;
            int u_xlati23;
            uint u_xlatu23;
            float3 u_xlat25;
            bool u_xlatb25;
            float u_xlat26;
            float2 u_xlat27;
            float u_xlat28;
            bool u_xlatb28;
            float3 u_xlat29;
            float3 u_xlat30;
            float3 u_xlat31;
            float3 u_xlat35;
            float u_xlat42;
            float2 u_xlat43;
            float2 u_xlat45;
            bool2 u_xlatb45;
            float2 u_xlat46;
            int u_xlati46;
            uint u_xlatu46;
            bool u_xlatb46;
            float2 u_xlat48;
            bool u_xlatb48;
            float2 u_xlat49;
            float2 u_xlat52;
            float2 u_xlat53;
            float u_xlat61;
            float u_xlat62;
            float u_xlat63;
            uint u_xlatu63;
            float u_xlat64;
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
            uint _g_gl_InstanceID : SV_InstanceID;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TANGENT0 : TANGENT;
                float4 in_TEXCOORD0 : TEXCOORD0;
                float4 in_TEXCOORD1 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                nointerpolation uint vs_CUSTOM_INSTANCE_ID0 : TEXCOORD0;
                float2 vs_INTERP0 : TEXCOORD1;
                float4 vs_INTERP4 : TEXCOORD2;
                float4 vs_INTERP5 : TEXCOORD3;
                float4 vs_INTERP6 : TEXCOORD4;
                float4 vs_INTERP7 : TEXCOORD5;
                float3 vs_INTERP8 : TEXCOORD6;
                float3 vs_INTERP9 : TEXCOORD7;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);


    u_xlati0 = _g_gl_InstanceID + _g_floatBitsToInt(_pad0.x);
    u_xlati5 = u_xlati0 * 9;
    u_xlati0 = int(u_xlati0 << 1);
    output.vs_INTERP0.xy = input.in_TEXCOORD1.xy * _pad0.xy + _pad0.zw;
    u_xlat0.xzw = input.in_POSITION0.yyy * _pad16.xyz;
    u_xlat0.xzw = _pad0.xyz * input.in_POSITION0.xxx + u_xlat0.xzw;
    u_xlat0.xzw = _pad32.xyz * input.in_POSITION0.zzz + u_xlat0.xzw;
    u_xlat0.xzw = u_xlat0.xzw + _pad48.xyz;
    u_xlat1.xy = float2(float2(_WindSpeed2, _WindSpeed2)) * _TimeParameters.xx + u_xlat0.xw;
    u_xlat1.xy = u_xlat1.xy * float2(_WindDensity);
    u_xlat1.zw = floor(u_xlat1.xy);
    u_xlat1.xy = _g_fract(u_xlat1.xy);
    u_xlat2 = u_xlat1.zwxy + float4(1.0, 1.0, -1.0, -1.0);
    u_xlati2.xy = int2(u_xlat2.xy);
    u_xlati7.x = int(uint(uint(u_xlati2.y) ^ 1103515245u));
    u_xlati2.x = u_xlati7.x + u_xlati2.x;
    u_xlatu2 = uint(u_xlati7.x) * uint(u_xlati2.x);
    u_xlatu7.x = uint(u_xlatu2 >> 5u);
    u_xlati2.x = int(uint(u_xlatu7.x ^ u_xlatu2));
    u_xlatu2 = uint(u_xlati2.x) * 668265261u;
    u_xlatu2 = uint(u_xlatu2 >> 8u);
    u_xlat2.x = float(u_xlatu2);
    u_xlat3.yz = u_xlat2.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat7.x = floor(u_xlat3.y);
    u_xlat3.x = u_xlat2.x * 5.96046519e-08 + (-u_xlat7.x);
    u_xlat2.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat2.x = _g_inversesqrt(u_xlat2.x);
    u_xlat2.xy = u_xlat2.xx * u_xlat3.xz;
    u_xlat2.x = dot(u_xlat2.xy, u_xlat2.zw);
    u_xlat3 = u_xlat1.zwzw + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati11.xy = int2(u_xlat1.zw);
    u_xlati3 = int4(u_xlat3);
    u_xlati7.xy = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati3.xy = u_xlati7.xy + u_xlati3.xz;
    u_xlatu7.xy = uint2(u_xlati7.xy) * uint2(u_xlati3.xy);
    u_xlatu3.xy = uint2(u_xlatu7.x >> 5u, u_xlatu7.y >> 5u);
    u_xlati7.xy = int2(uint2(u_xlatu7.x ^ u_xlatu3.x, u_xlatu7.y ^ u_xlatu3.y));
    u_xlatu7.xy = uint2(u_xlati7.xy) * uint2(668265261u, 668265261u);
    u_xlatu7.xy = uint2(u_xlatu7.x >> 8u, u_xlatu7.y >> 8u);
    u_xlat7.xy = float2(u_xlatu7.xy);
    u_xlat3 = u_xlat7.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat4.xy = floor(u_xlat3.xy);
    u_xlat3.xy = u_xlat7.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat4.xy);
    u_xlat7.x = dot(u_xlat3.yw, u_xlat3.yw);
    u_xlat7.x = _g_inversesqrt(u_xlat7.x);
    u_xlat7.xy = u_xlat7.xx * u_xlat3.yw;
    u_xlat4 = u_xlat1.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat7.x = dot(u_xlat7.xy, u_xlat4.zw);
    u_xlat2.x = (-u_xlat7.x) + u_xlat2.x;
    u_xlat12.xy = u_xlat1.xy * u_xlat1.xy;
    u_xlat12.xy = u_xlat1.xy * u_xlat12.xy;
    u_xlat8.xz = u_xlat1.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat8.xz = u_xlat1.xy * u_xlat8.xz + float2(10.0, 10.0);
    u_xlat12.xy = u_xlat12.xy * u_xlat8.xz;
    u_xlat2.x = u_xlat12.y * u_xlat2.x + u_xlat7.x;
    u_xlat7.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat7.x = _g_inversesqrt(u_xlat7.x);
    u_xlat3.xy = u_xlat7.xx * u_xlat3.xz;
    u_xlat7.x = dot(u_xlat3.xy, u_xlat4.xy);
    u_xlati16 = int(uint(uint(u_xlati11.y) ^ 1103515245u));
    u_xlati11.x = u_xlati16 + u_xlati11.x;
    u_xlatu11 = uint(u_xlati16) * uint(u_xlati11.x);
    u_xlatu16 = uint(u_xlatu11 >> 5u);
    u_xlati11.x = int(uint(u_xlatu16 ^ u_xlatu11));
    u_xlatu11 = uint(u_xlati11.x) * 668265261u;
    u_xlatu11 = uint(u_xlatu11 >> 8u);
    u_xlat11.x = float(u_xlatu11);
    u_xlat3.yz = u_xlat11.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat16 = floor(u_xlat3.y);
    u_xlat3.x = u_xlat11.x * 5.96046519e-08 + (-u_xlat16);
    u_xlat11.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat11.x = _g_inversesqrt(u_xlat11.x);
    u_xlat11.xy = u_xlat11.xx * u_xlat3.xz;
    u_xlat1.x = dot(u_xlat11.xy, u_xlat1.xy);
    u_xlat6.x = (-u_xlat1.x) + u_xlat7.x;
    u_xlat1.x = u_xlat12.y * u_xlat6.x + u_xlat1.x;
    u_xlat6.x = (-u_xlat1.x) + u_xlat2.x;
    u_xlat1.x = u_xlat12.x * u_xlat6.x + u_xlat1.x;
    u_xlat1.x = u_xlat1.x + 0.5;
    u_xlat6.xy = (-_TimeParameters.xx) * float2(_WindSpeed) + u_xlat0.xw;
    u_xlat6.xy = u_xlat6.xy * float2(_WindDensity);
    u_xlat2.xy = floor(u_xlat6.xy);
    u_xlat6.xy = _g_fract(u_xlat6.xy);
    u_xlat12.xy = u_xlat2.xy + float2(1.0, 1.0);
    u_xlati12.xy = int2(u_xlat12.xy);
    u_xlati16 = int(uint(uint(u_xlati12.y) ^ 1103515245u));
    u_xlati12.x = u_xlati16 + u_xlati12.x;
    u_xlatu16 = uint(u_xlati16) * uint(u_xlati12.x);
    u_xlatu12.x = uint(u_xlatu16 >> 5u);
    u_xlati16 = int(uint(u_xlatu16 ^ u_xlatu12.x));
    u_xlatu16 = uint(u_xlati16) * 668265261u;
    u_xlatu16 = uint(u_xlatu16 >> 8u);
    u_xlat16 = float(u_xlatu16);
    u_xlat3.yz = float2(u_xlat16) * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat12.x = floor(u_xlat3.y);
    u_xlat3.x = u_xlat16 * 5.96046519e-08 + (-u_xlat12.x);
    u_xlat16 = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat16 = _g_inversesqrt(u_xlat16);
    u_xlat12.xy = float2(u_xlat16) * u_xlat3.xz;
    u_xlat3.xy = u_xlat6.xy + float2(-1.0, -1.0);
    u_xlat16 = dot(u_xlat12.xy, u_xlat3.xy);
    u_xlat3 = u_xlat2.xyxy + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati2.xy = int2(u_xlat2.xy);
    u_xlati3 = int4(u_xlat3);
    u_xlati12.xy = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati3.xy = u_xlati12.xy + u_xlati3.xz;
    u_xlatu12.xy = uint2(u_xlati12.xy) * uint2(u_xlati3.xy);
    u_xlatu3.xy = uint2(u_xlatu12.x >> 5u, u_xlatu12.y >> 5u);
    u_xlati12.xy = int2(uint2(u_xlatu12.x ^ u_xlatu3.x, u_xlatu12.y ^ u_xlatu3.y));
    u_xlatu12.xy = uint2(u_xlati12.xy) * uint2(668265261u, 668265261u);
    u_xlatu12.xy = uint2(u_xlatu12.x >> 8u, u_xlatu12.y >> 8u);
    u_xlat12.xy = float2(u_xlatu12.xy);
    u_xlat3 = u_xlat12.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat4.xy = floor(u_xlat3.xy);
    u_xlat3.xy = u_xlat12.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat4.xy);
    u_xlat12.x = dot(u_xlat3.yw, u_xlat3.yw);
    u_xlat12.x = _g_inversesqrt(u_xlat12.x);
    u_xlat12.xy = u_xlat12.xx * u_xlat3.yw;
    u_xlat4 = u_xlat6.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat12.x = dot(u_xlat12.xy, u_xlat4.zw);
    u_xlat16 = u_xlat16 + (-u_xlat12.x);
    u_xlat8.xz = u_xlat6.xy * u_xlat6.xy;
    u_xlat8.xz = u_xlat6.xy * u_xlat8.xz;
    u_xlat14.xy = u_xlat6.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat14.xy = u_xlat6.xy * u_xlat14.xy + float2(10.0, 10.0);
    u_xlat8.xz = u_xlat8.xz * u_xlat14.xy;
    u_xlat16 = u_xlat8.z * u_xlat16 + u_xlat12.x;
    u_xlat12.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat12.x = _g_inversesqrt(u_xlat12.x);
    u_xlat12.xy = u_xlat12.xx * u_xlat3.xz;
    u_xlat12.x = dot(u_xlat12.xy, u_xlat4.xy);
    u_xlati7.x = int(uint(uint(u_xlati2.y) ^ 1103515245u));
    u_xlati2.x = u_xlati7.x + u_xlati2.x;
    u_xlatu2 = uint(u_xlati7.x) * uint(u_xlati2.x);
    u_xlatu7.x = uint(u_xlatu2 >> 5u);
    u_xlati2.x = int(uint(u_xlatu7.x ^ u_xlatu2));
    u_xlatu2 = uint(u_xlati2.x) * 668265261u;
    u_xlatu2 = uint(u_xlatu2 >> 8u);
    u_xlat2.x = float(u_xlatu2);
    u_xlat4.yz = u_xlat2.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat7.x = floor(u_xlat4.y);
    u_xlat4.x = u_xlat2.x * 5.96046519e-08 + (-u_xlat7.x);
    u_xlat2.x = dot(u_xlat4.xz, u_xlat4.xz);
    u_xlat2.x = _g_inversesqrt(u_xlat2.x);
    u_xlat2.xy = u_xlat2.xx * u_xlat4.xz;
    u_xlat6.x = dot(u_xlat2.xy, u_xlat6.xy);
    u_xlat11.x = (-u_xlat6.x) + u_xlat12.x;
    u_xlat6.x = u_xlat8.z * u_xlat11.x + u_xlat6.x;
    u_xlat11.x = (-u_xlat6.x) + u_xlat16;
    u_xlat6.x = u_xlat8.x * u_xlat11.x + u_xlat6.x;
    u_xlat1.x = u_xlat1.x + u_xlat6.x;
    u_xlat1.xz = u_xlat1.xx + float2(-0.5, -0.5);
    u_xlat1.y = _WindScale;
    u_xlat1.xyz = u_xlat1.xyz * input.in_POSITION0.zzz;
    u_xlat2.xz = float2(_WindScale);
    u_xlat2.y = 0.0;
    u_xlat0.xzw = u_xlat2.xyz * u_xlat1.xyz + u_xlat0.xzw;
    u_xlat1.xyz = u_xlat0.zzz * _pad80.xyz;
    u_xlat1.xyz = _pad64.xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat0.xzw = _pad96.xyz * u_xlat0.www + u_xlat1.xyz;
    u_xlat0.xzw = u_xlat0.xzw + _pad112.xyz;
    u_xlat1.xyz = u_xlat0.zzz * _pad16.xyz;
    u_xlat1.xyz = _pad0.xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat0.xzw = _pad32.xyz * u_xlat0.www + u_xlat1.xyz;
    u_xlat0.xzw = u_xlat0.xzw + _pad48.xyz;
    u_xlat1 = u_xlat0.zzzz * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.wwww + u_xlat1;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    u_xlat1.xyz = u_xlat0.zzz * _pad16.xyz;
    u_xlat1.xyz = _pad0.xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat1.xyz = _pad32.xyz * u_xlat0.www + u_xlat1.xyz;
    output.vs_INTERP8.xyz = u_xlat0.xzw;
    output.vs_INTERP4.xyz = u_xlat1.xyz + _pad48.xyz;
    output.vs_INTERP4.w = 0.0;
    u_xlat0.xzw = input.in_TANGENT0.yyy * _pad16.xyz;
    u_xlat0.xzw = _pad0.xyz * input.in_TANGENT0.xxx + u_xlat0.xzw;
    u_xlat0.xzw = _pad32.xyz * input.in_TANGENT0.zzz + u_xlat0.xzw;
    u_xlat1.x = dot(u_xlat0.xzw, u_xlat0.xzw);
    u_xlat1.x = max(u_xlat1.x, 1.17549435e-38);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    output.vs_INTERP5.xyz = u_xlat0.xzw * u_xlat1.xxx;
    output.vs_INTERP5.w = input.in_TANGENT0.w;
    output.vs_INTERP6 = input.in_TEXCOORD0;
    output.vs_INTERP7 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat1.x = dot(input.in_NORMAL0.xyz, _pad64.xyz);
    u_xlat1.y = dot(input.in_NORMAL0.xyz, _pad80.xyz);
    u_xlat1.z = dot(input.in_NORMAL0.xyz, _pad96.xyz);
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    output.vs_INTERP9.xyz = u_xlat0.xxx * u_xlat1.xyz;
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
    u_xlat1 = _g_texture(_Texture_t8, input.vs_INTERP6.xy, _GlobalMipBias.x);
    u_xlat20.xyz = dFdy(input.vs_INTERP8.zxy);
    u_xlat2.xyz = dFdx(input.vs_INTERP8.yzx);
    u_xlat3.xyz = u_xlat20.xyz * u_xlat2.xyz;
    u_xlat20.xyz = u_xlat20.zxy * u_xlat2.yzx + (-u_xlat3.xyz);
    u_xlat2.x = dot(u_xlat20.xyz, u_xlat20.xyz);
    u_xlat2.x = _g_inversesqrt(u_xlat2.x);
    u_xlat20.xyz = u_xlat20.xyz * u_xlat2.xxx;
    u_xlatb2 = _AlphaToMaskAvailable!=0.0;
    u_xlat22.x = dFdx(u_xlat1.w);
    u_xlat42 = dFdy(u_xlat1.w);
    u_xlat22.x = abs(u_xlat42) + abs(u_xlat22.x);
    u_xlat42 = u_xlat1.w + -0.5;
    u_xlat62 = (-u_xlat22.x) * 0.5 + u_xlat42;
    u_xlat22.x = max(u_xlat22.x, 9.99999975e-05);
    u_xlat22.x = u_xlat62 / u_xlat22.x;
    u_xlat22.x = u_xlat22.x + 1.0;
    u_xlat22.x = clamp(u_xlat22.x, 0.0, 1.0);
    u_xlat62 = u_xlat22.x + -9.99999975e-05;
    u_xlat42 = (u_xlatb2) ? u_xlat62 : u_xlat42;
    u_xlat61 = (u_xlatb2) ? u_xlat22.x : u_xlat1.w;
    u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
    u_xlatb22 = u_xlat42<0.0;
    if(u_xlatb22){discard;}
    u_xlatb22 = unity_OrthoParams.w==0.0;
    u_xlat3.xyz = (-input.vs_INTERP8.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat42 = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat42 = _g_inversesqrt(u_xlat42);
    u_xlat3.xyz = float3(u_xlat42) * u_xlat3.xyz;
    u_xlat4.x = _tunity_MatrixV[0].z;
    u_xlat4.y = _tunity_MatrixV[1].z;
    u_xlat4.z = _tunity_MatrixV[2].z;
    u_xlat22.xyz = (bool(u_xlatb22)) ? u_xlat3.xyz : u_xlat4.xyz;
    u_xlat3.x = input.vs_INTERP8.y * _tunity_MatrixV[1].z;
    u_xlat3.x = _tunity_MatrixV[0].z * input.vs_INTERP8.x + u_xlat3.x;
    u_xlat3.x = _tunity_MatrixV[2].z * input.vs_INTERP8.z + u_xlat3.x;
    u_xlat3.x = u_xlat3.x + _tunity_MatrixV[3].z;
    u_xlat3.x = (-u_xlat3.x) + (-_pad352.y);
    u_xlat3.x = max(u_xlat3.x, 0.0);
    u_xlat3.x = u_xlat3.x * _pad976.x;
    u_xlat23 = _Smoothness;
    u_xlat23 = clamp(u_xlat23, 0.0, 1.0);
    u_xlat4.xyz = _g_texture(_Texture, input.vs_INTERP0.xy, _GlobalMipBias.x).xyz;
    u_xlat5 = _g_texture(_Noise_Map, input.vs_INTERP0.xy, _GlobalMipBias.x);
    u_xlat5.xyz = u_xlat5.xyz + float3(-0.5, -0.5, -0.5);
    u_xlat43.x = dot(u_xlat20.xyz, u_xlat5.xyz);
    u_xlat43.x = u_xlat43.x + 0.5;
    u_xlat4.xyz = u_xlat43.xxx * u_xlat4.xyz;
    u_xlat43.x = max(u_xlat5.w, 9.99999975e-05);
    u_xlat4.xyz = u_xlat4.xyz / u_xlat43.xxx;
    u_xlat1.xyz = u_xlat1.xyz * float3(0.959999979, 0.959999979, 0.959999979);
    u_xlat43.x = (-u_xlat23) + 1.0;
    u_xlat63 = u_xlat43.x * u_xlat43.x;
    u_xlat63 = max(u_xlat63, 0.0078125);
    u_xlat64 = u_xlat63 * u_xlat63;
    u_xlat23 = u_xlat23 + 0.0400000215;
    u_xlat23 = min(u_xlat23, 1.0);
    u_xlat5.x = u_xlat63 * 4.0 + 2.0;
    u_xlati0 = u_xlati0 * 9;
    u_xlatb25 = 0.0<_pad432.y;
    if(u_xlatb25){
        u_xlatb25 = _pad432.y==1.0;
        if(u_xlatb25){
            u_xlat6 = input.vs_INTERP4.xyxy + _pad400;
            float3 txVec0 = float3(u_xlat6.xy,input.vs_INTERP4.z);
            u_xlat7.x = _g_textureLod(_Texture_t5, txVec0, 0.0);
            float3 txVec1 = float3(u_xlat6.zw,input.vs_INTERP4.z);
            u_xlat7.y = _g_textureLod(_Texture_t5, txVec1, 0.0);
            u_xlat6 = input.vs_INTERP4.xyxy + _pad416;
            float3 txVec2 = float3(u_xlat6.xy,input.vs_INTERP4.z);
            u_xlat7.z = _g_textureLod(_Texture_t5, txVec2, 0.0);
            float3 txVec3 = float3(u_xlat6.zw,input.vs_INTERP4.z);
            u_xlat7.w = _g_textureLod(_Texture_t5, txVec3, 0.0);
            u_xlat25.x = dot(u_xlat7, float4(0.25, 0.25, 0.25, 0.25));
        } else {
            u_xlatb45.x = _pad432.y==2.0;
            if(u_xlatb45.x){
                u_xlat45.xy = input.vs_INTERP4.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat45.xy = floor(u_xlat45.xy);
                u_xlat6.xy = input.vs_INTERP4.xy * _pad448.zw + (-u_xlat45.xy);
                u_xlat7 = u_xlat6.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat8 = u_xlat7.xxzz * u_xlat7.xxzz;
                u_xlat46.xy = u_xlat8.yw * float2(0.0799999982, 0.0799999982);
                u_xlat7.xz = u_xlat8.xz * float2(0.5, 0.5) + (-u_xlat6.xy);
                u_xlat8.xy = (-u_xlat6.xy) + float2(1.0, 1.0);
                u_xlat48.xy = min(u_xlat6.xy, float2(0.0, 0.0));
                u_xlat48.xy = (-u_xlat48.xy) * u_xlat48.xy + u_xlat8.xy;
                u_xlat6.xy = max(u_xlat6.xy, float2(0.0, 0.0));
                u_xlat6.xy = (-u_xlat6.xy) * u_xlat6.xy + u_xlat7.yw;
                u_xlat48.xy = u_xlat48.xy + float2(1.0, 1.0);
                u_xlat6.xy = u_xlat6.xy + float2(1.0, 1.0);
                u_xlat9.xy = u_xlat7.xz * float2(0.159999996, 0.159999996);
                u_xlat10.xy = u_xlat8.xy * float2(0.159999996, 0.159999996);
                u_xlat8.xy = u_xlat48.xy * float2(0.159999996, 0.159999996);
                u_xlat11.xy = u_xlat6.xy * float2(0.159999996, 0.159999996);
                u_xlat6.xy = u_xlat7.yw * float2(0.159999996, 0.159999996);
                u_xlat9.z = u_xlat8.x;
                u_xlat9.w = u_xlat6.x;
                u_xlat10.z = u_xlat11.x;
                u_xlat10.w = u_xlat46.x;
                u_xlat7 = u_xlat9.zwxz + u_xlat10.zwxz;
                u_xlat8.z = u_xlat9.y;
                u_xlat8.w = u_xlat6.y;
                u_xlat11.z = u_xlat10.y;
                u_xlat11.w = u_xlat46.y;
                u_xlat6.xyz = u_xlat8.zyw + u_xlat11.zyw;
                u_xlat8.xyz = u_xlat10.xzw / u_xlat7.zwy;
                u_xlat8.xyz = u_xlat8.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat9.xyz = u_xlat11.zyw / u_xlat6.xyz;
                u_xlat9.xyz = u_xlat9.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat8.xyz = u_xlat8.yxz * _pad448.xxx;
                u_xlat9.xyz = u_xlat9.xyz * _pad448.yyy;
                u_xlat8.w = u_xlat9.x;
                u_xlat10 = u_xlat45.xyxy * _pad448.xyxy + u_xlat8.ywxw;
                u_xlat11.xy = u_xlat45.xy * _pad448.xy + u_xlat8.zw;
                u_xlat9.w = u_xlat8.y;
                u_xlat8.yw = u_xlat9.yz;
                u_xlat12 = u_xlat45.xyxy * _pad448.xyxy + u_xlat8.xyzy;
                u_xlat9 = u_xlat45.xyxy * _pad448.xyxy + u_xlat9.wywz;
                u_xlat8 = u_xlat45.xyxy * _pad448.xyxy + u_xlat8.xwzw;
                u_xlat13 = u_xlat6.xxxy * u_xlat7.zwyz;
                u_xlat14 = u_xlat6.yyzz * u_xlat7;
                u_xlat45.x = u_xlat6.z * u_xlat7.y;
                float3 txVec4 = float3(u_xlat10.xy,input.vs_INTERP4.z);
                u_xlat65 = _g_textureLod(_Texture_t5, txVec4, 0.0);
                float3 txVec5 = float3(u_xlat10.zw,input.vs_INTERP4.z);
                u_xlat6.x = _g_textureLod(_Texture_t5, txVec5, 0.0);
                u_xlat6.x = u_xlat6.x * u_xlat13.y;
                u_xlat65 = u_xlat13.x * u_xlat65 + u_xlat6.x;
                float3 txVec6 = float3(u_xlat11.xy,input.vs_INTERP4.z);
                u_xlat6.x = _g_textureLod(_Texture_t5, txVec6, 0.0);
                u_xlat65 = u_xlat13.z * u_xlat6.x + u_xlat65;
                float3 txVec7 = float3(u_xlat9.xy,input.vs_INTERP4.z);
                u_xlat6.x = _g_textureLod(_Texture_t5, txVec7, 0.0);
                u_xlat65 = u_xlat13.w * u_xlat6.x + u_xlat65;
                float3 txVec8 = float3(u_xlat12.xy,input.vs_INTERP4.z);
                u_xlat6.x = _g_textureLod(_Texture_t5, txVec8, 0.0);
                u_xlat65 = u_xlat14.x * u_xlat6.x + u_xlat65;
                float3 txVec9 = float3(u_xlat12.zw,input.vs_INTERP4.z);
                u_xlat6.x = _g_textureLod(_Texture_t5, txVec9, 0.0);
                u_xlat65 = u_xlat14.y * u_xlat6.x + u_xlat65;
                float3 txVec10 = float3(u_xlat9.zw,input.vs_INTERP4.z);
                u_xlat6.x = _g_textureLod(_Texture_t5, txVec10, 0.0);
                u_xlat65 = u_xlat14.z * u_xlat6.x + u_xlat65;
                float3 txVec11 = float3(u_xlat8.xy,input.vs_INTERP4.z);
                u_xlat6.x = _g_textureLod(_Texture_t5, txVec11, 0.0);
                u_xlat65 = u_xlat14.w * u_xlat6.x + u_xlat65;
                float3 txVec12 = float3(u_xlat8.zw,input.vs_INTERP4.z);
                u_xlat6.x = _g_textureLod(_Texture_t5, txVec12, 0.0);
                u_xlat25.x = u_xlat45.x * u_xlat6.x + u_xlat65;
            } else {
                u_xlat45.xy = input.vs_INTERP4.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat45.xy = floor(u_xlat45.xy);
                u_xlat6.xy = input.vs_INTERP4.xy * _pad448.zw + (-u_xlat45.xy);
                u_xlat7 = u_xlat6.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat8 = u_xlat7.xxzz * u_xlat7.xxzz;
                u_xlat9.yw = u_xlat8.yw * float2(0.0408160016, 0.0408160016);
                u_xlat46.xy = u_xlat8.xz * float2(0.5, 0.5) + (-u_xlat6.xy);
                u_xlat7.xz = (-u_xlat6.xy) + float2(1.0, 1.0);
                u_xlat8.xy = min(u_xlat6.xy, float2(0.0, 0.0));
                u_xlat7.xz = (-u_xlat8.xy) * u_xlat8.xy + u_xlat7.xz;
                u_xlat8.xy = max(u_xlat6.xy, float2(0.0, 0.0));
                u_xlat7.yw = (-u_xlat8.xy) * u_xlat8.xy + u_xlat7.yw;
                u_xlat7 = u_xlat7 + float4(2.0, 2.0, 2.0, 2.0);
                u_xlat8.z = u_xlat7.y * 0.0816320032;
                u_xlat10.xy = u_xlat46.yx * float2(0.0816320032, 0.0816320032);
                u_xlat46.xy = u_xlat7.xz * float2(0.0816320032, 0.0816320032);
                u_xlat10.z = u_xlat7.w * 0.0816320032;
                u_xlat8.x = u_xlat10.y;
                u_xlat8.yw = u_xlat6.xx * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat7.xz = u_xlat6.xx * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat7.y = u_xlat46.x;
                u_xlat7.w = u_xlat9.y;
                u_xlat8 = u_xlat7 + u_xlat8;
                u_xlat10.yw = u_xlat6.yy * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat9.xz = u_xlat6.yy * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat9.y = u_xlat46.y;
                u_xlat6 = u_xlat9 + u_xlat10;
                u_xlat7 = u_xlat7 / u_xlat8;
                u_xlat7 = u_xlat7 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat9 = u_xlat9 / u_xlat6;
                u_xlat9 = u_xlat9 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat7 = u_xlat7.wxyz * _pad448.xxxx;
                u_xlat9 = u_xlat9.xwyz * _pad448.yyyy;
                u_xlat10.xzw = u_xlat7.yzw;
                u_xlat10.y = u_xlat9.x;
                u_xlat11 = u_xlat45.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat12.xy = u_xlat45.xy * _pad448.xy + u_xlat10.wy;
                u_xlat7.y = u_xlat10.y;
                u_xlat10.y = u_xlat9.z;
                u_xlat13 = u_xlat45.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat52.xy = u_xlat45.xy * _pad448.xy + u_xlat10.wy;
                u_xlat7.z = u_xlat10.y;
                u_xlat14 = u_xlat45.xyxy * _pad448.xyxy + u_xlat7.xyxz;
                u_xlat10.y = u_xlat9.w;
                u_xlat15 = u_xlat45.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat27.xy = u_xlat45.xy * _pad448.xy + u_xlat10.wy;
                u_xlat7.w = u_xlat10.y;
                u_xlat16.xy = u_xlat45.xy * _pad448.xy + u_xlat7.xw;
                u_xlat9.xzw = u_xlat10.xzw;
                u_xlat10 = u_xlat45.xyxy * _pad448.xyxy + u_xlat9.xyzy;
                u_xlat49.xy = u_xlat45.xy * _pad448.xy + u_xlat9.wy;
                u_xlat9.x = u_xlat7.x;
                u_xlat45.xy = u_xlat45.xy * _pad448.xy + u_xlat9.xy;
                u_xlat17 = u_xlat6.xxxx * u_xlat8;
                u_xlat18 = u_xlat6.yyyy * u_xlat8;
                u_xlat19 = u_xlat6.zzzz * u_xlat8;
                u_xlat6 = u_xlat6.wwww * u_xlat8;
                float3 txVec13 = float3(u_xlat11.xy,input.vs_INTERP4.z);
                u_xlat7.x = _g_textureLod(_Texture_t5, txVec13, 0.0);
                float3 txVec14 = float3(u_xlat11.zw,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec14, 0.0);
                u_xlat67 = u_xlat67 * u_xlat17.y;
                u_xlat7.x = u_xlat17.x * u_xlat7.x + u_xlat67;
                float3 txVec15 = float3(u_xlat12.xy,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec15, 0.0);
                u_xlat7.x = u_xlat17.z * u_xlat67 + u_xlat7.x;
                float3 txVec16 = float3(u_xlat14.xy,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec16, 0.0);
                u_xlat7.x = u_xlat17.w * u_xlat67 + u_xlat7.x;
                float3 txVec17 = float3(u_xlat13.xy,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec17, 0.0);
                u_xlat7.x = u_xlat18.x * u_xlat67 + u_xlat7.x;
                float3 txVec18 = float3(u_xlat13.zw,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec18, 0.0);
                u_xlat7.x = u_xlat18.y * u_xlat67 + u_xlat7.x;
                float3 txVec19 = float3(u_xlat52.xy,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec19, 0.0);
                u_xlat7.x = u_xlat18.z * u_xlat67 + u_xlat7.x;
                float3 txVec20 = float3(u_xlat14.zw,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec20, 0.0);
                u_xlat7.x = u_xlat18.w * u_xlat67 + u_xlat7.x;
                float3 txVec21 = float3(u_xlat15.xy,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec21, 0.0);
                u_xlat7.x = u_xlat19.x * u_xlat67 + u_xlat7.x;
                float3 txVec22 = float3(u_xlat15.zw,input.vs_INTERP4.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec22, 0.0);
                u_xlat7.x = u_xlat19.y * u_xlat67 + u_xlat7.x;
                float3 txVec23 = float3(u_xlat27.xy,input.vs_INTERP4.z);
                u_xlat27.x = _g_textureLod(_Texture_t5, txVec23, 0.0);
                u_xlat7.x = u_xlat19.z * u_xlat27.x + u_xlat7.x;
                float3 txVec24 = float3(u_xlat16.xy,input.vs_INTERP4.z);
                u_xlat27.x = _g_textureLod(_Texture_t5, txVec24, 0.0);
                u_xlat7.x = u_xlat19.w * u_xlat27.x + u_xlat7.x;
                float3 txVec25 = float3(u_xlat10.xy,input.vs_INTERP4.z);
                u_xlat27.x = _g_textureLod(_Texture_t5, txVec25, 0.0);
                u_xlat6.x = u_xlat6.x * u_xlat27.x + u_xlat7.x;
                float3 txVec26 = float3(u_xlat10.zw,input.vs_INTERP4.z);
                u_xlat7.x = _g_textureLod(_Texture_t5, txVec26, 0.0);
                u_xlat6.x = u_xlat6.y * u_xlat7.x + u_xlat6.x;
                float3 txVec27 = float3(u_xlat49.xy,input.vs_INTERP4.z);
                u_xlat26 = _g_textureLod(_Texture_t5, txVec27, 0.0);
                u_xlat6.x = u_xlat6.z * u_xlat26 + u_xlat6.x;
                float3 txVec28 = float3(u_xlat45.xy,input.vs_INTERP4.z);
                u_xlat45.x = _g_textureLod(_Texture_t5, txVec28, 0.0);
                u_xlat25.x = u_xlat6.w * u_xlat45.x + u_xlat6.x;
            }
        }
    } else {
        float3 txVec29 = float3(input.vs_INTERP4.xy,input.vs_INTERP4.z);
        u_xlat25.x = _g_textureLod(_Texture_t5, txVec29, 0.0);
    }
    u_xlat45.x = (-_pad432.x) + 1.0;
    u_xlat25.x = u_xlat25.x * _pad432.x + u_xlat45.x;
    u_xlatb45.x = 0.0>=input.vs_INTERP4.z;
    u_xlatb65 = input.vs_INTERP4.z>=1.0;
    u_xlatb45.x = u_xlatb65 || u_xlatb45.x;
    u_xlat25.x = (u_xlatb45.x) ? 1.0 : u_xlat25.x;
    u_xlat6.xyz = input.vs_INTERP8.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat45.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat45.x = u_xlat45.x * _pad432.z + _pad432.w;
    u_xlat45.x = clamp(u_xlat45.x, 0.0, 1.0);
    u_xlat65 = (-u_xlat25.x) + 1.0;
    u_xlat25.x = u_xlat45.x * u_xlat65 + u_xlat25.x;
    u_xlatb45.x = _pad176.y!=-1.0;
    if(u_xlatb45.x){
        u_xlat45.xy = input.vs_INTERP8.yy * _pad16.xy;
        u_xlat45.xy = _pad0.xy * input.vs_INTERP8.xx + u_xlat45.xy;
        u_xlat45.xy = _pad32.xy * input.vs_INTERP8.zz + u_xlat45.xy;
        u_xlat45.xy = u_xlat45.xy + _pad48.xy;
        u_xlat45.xy = u_xlat45.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
        u_xlat6 = _g_texture(_Texture_t6, u_xlat45.xy, _GlobalMipBias.x);
        u_xlatb45.xy = _g_equal(_pad176.yyyy, float4(0.0, 1.0, 0.0, 1.0)).xy;
        u_xlat65 = (u_xlatb45.y) ? u_xlat6.w : u_xlat6.x;
        u_xlat6.xyz = (u_xlatb45.x) ? u_xlat6.xyz : float3(u_xlat65);
    } else {
        u_xlat6.x = float(1.0);
        u_xlat6.y = float(1.0);
        u_xlat6.z = float(1.0);
    }
    u_xlat6.xyz = u_xlat6.xyz * _MainLightColor.xyz;
    u_xlat45.x = dot((-u_xlat22.xyz), u_xlat20.xyz);
    u_xlat45.x = u_xlat45.x + u_xlat45.x;
    u_xlat7.xyz = u_xlat20.xyz * (-u_xlat45.xxx) + (-u_xlat22.xyz);
    u_xlat45.x = dot(u_xlat20.xyz, u_xlat22.xyz);
    u_xlat45.x = clamp(u_xlat45.x, 0.0, 1.0);
    u_xlat45.x = (-u_xlat45.x) + 1.0;
    u_xlat45.x = u_xlat45.x * u_xlat45.x;
    u_xlat45.x = u_xlat45.x * u_xlat45.x;
    u_xlat65 = (-u_xlat43.x) * 0.699999988 + 1.70000005;
    u_xlat43.x = u_xlat43.x * u_xlat65;
    u_xlat43.x = u_xlat43.x * 6.0;
    u_xlat8.xyz = unity_SpecCube0_BoxMax.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat9.xyz = u_xlat8.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube0_BoxMin.xyz;
    u_xlat10.xyz = (-u_xlat9.xyz) + input.vs_INTERP8.xyz;
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
    u_xlat15.xyz = (-u_xlat14.xyz) + input.vs_INTERP8.xyz;
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
    u_xlat48.x = min(u_xlat11.z, u_xlat11.y);
    u_xlat48.x = min(u_xlat48.x, u_xlat11.x);
    u_xlat48.x = clamp(u_xlat48.x, 0.0, 1.0);
    u_xlat11.xyz = u_xlat14.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat16.xyz = (-u_xlat14.xyz) + unity_SpecCube1_BoxMax.xyz;
    u_xlat11.xyz = min(u_xlat11.xyz, u_xlat16.xyz);
    u_xlat11.xyz = u_xlat11.xyz / unity_SpecCube1_BoxMax.www;
    u_xlat69 = min(u_xlat11.z, u_xlat11.y);
    u_xlat69 = min(u_xlat69, u_xlat11.x);
    u_xlat69 = clamp(u_xlat69, 0.0, 1.0);
    u_xlat10.x = (-u_xlat69) + 1.0;
    u_xlat10.x = min(u_xlat48.x, u_xlat10.x);
    u_xlat8.x = (u_xlatb8) ? u_xlat10.x : u_xlat48.x;
    u_xlat48.x = (-u_xlat48.x) + 1.0;
    u_xlat48.x = min(u_xlat48.x, u_xlat69);
    u_xlat8.y = (u_xlatb28) ? u_xlat48.x : u_xlat69;
    u_xlat48.x = u_xlat8.y + u_xlat8.x;
    u_xlat69 = max(u_xlat48.x, 1.0);
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
        u_xlat9 = _g_textureLod(_Texture_t1, u_xlat16.xyz, u_xlat43.x);
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
        u_xlat10 = _g_textureLod(_Texture_t2, u_xlat14.xyz, u_xlat43.x);
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
    u_xlatb65 = u_xlat48.x<0.99000001;
    if(u_xlatb65){
        u_xlat7 = _g_textureLod(_Texture_t0, u_xlat7.xyz, u_xlat43.x);
        u_xlat43.x = (-u_xlat48.x) + 1.0;
        u_xlat65 = u_xlat7.w + -1.0;
        u_xlat65 = _GlossyEnvironmentCubeMap_HDR.w * u_xlat65 + 1.0;
        u_xlat65 = max(u_xlat65, 0.0);
        u_xlat65 = log2(u_xlat65);
        u_xlat65 = u_xlat65 * _GlossyEnvironmentCubeMap_HDR.y;
        u_xlat65 = exp2(u_xlat65);
        u_xlat65 = u_xlat65 * _GlossyEnvironmentCubeMap_HDR.x;
        u_xlat7.xyz = u_xlat7.xyz * float3(u_xlat65);
        u_xlat9.xyz = u_xlat43.xxx * u_xlat7.xyz + u_xlat9.xyz;
    }
    u_xlat43.xy = float2(u_xlat63) * float2(u_xlat63) + float2(-1.0, 1.0);
    u_xlat63 = float(1.0) / u_xlat43.y;
    u_xlat23 = u_xlat23 + -0.0399999991;
    u_xlat23 = u_xlat45.x * u_xlat23 + 0.0399999991;
    u_xlat23 = u_xlat23 * u_xlat63;
    u_xlat7.xyz = float3(u_xlat23) * u_xlat9.xyz;
    u_xlat4.xyz = u_xlat4.xyz * u_xlat1.xyz + u_xlat7.xyz;
    u_xlati23 = int(uint(uint(_g_floatBitsToUint(_MainLightLayerMask)) & uint(_g_floatBitsToUint(_pad128.x))));
    u_xlat63 = u_xlat25.x * unity_LightData.z;
    u_xlat25.x = dot(u_xlat20.xyz, _MainLightPosition.xyz);
    u_xlat25.x = clamp(u_xlat25.x, 0.0, 1.0);
    u_xlat63 = u_xlat63 * u_xlat25.x;
    u_xlat25.xyz = float3(u_xlat63) * u_xlat6.xyz;
    u_xlat6.xyz = u_xlat22.xyz + _MainLightPosition.xyz;
    u_xlat63 = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat63 = max(u_xlat63, 1.17549435e-38);
    u_xlat63 = _g_inversesqrt(u_xlat63);
    u_xlat6.xyz = float3(u_xlat63) * u_xlat6.xyz;
    u_xlat63 = dot(u_xlat20.xyz, u_xlat6.xyz);
    u_xlat63 = clamp(u_xlat63, 0.0, 1.0);
    u_xlat6.x = dot(_MainLightPosition.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat63 = u_xlat63 * u_xlat63;
    u_xlat63 = u_xlat63 * u_xlat43.x + 1.00001001;
    u_xlat6.x = u_xlat6.x * u_xlat6.x;
    u_xlat63 = u_xlat63 * u_xlat63;
    u_xlat6.x = max(u_xlat6.x, 0.100000001);
    u_xlat63 = u_xlat63 * u_xlat6.x;
    u_xlat63 = u_xlat5.x * u_xlat63;
    u_xlat63 = u_xlat64 / u_xlat63;
    u_xlat6.xyz = float3(u_xlat63) * float3(0.0399999991, 0.0399999991, 0.0399999991) + u_xlat1.xyz;
    u_xlat25.xyz = u_xlat25.xyz * u_xlat6.xyz;
    u_xlat25.xyz = (int(u_xlati23) != 0) ? u_xlat25.xyz : float3(0.0, 0.0, 0.0);
    u_xlat23 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu23 =  uint(int(u_xlat23));
    u_xlatb6.xy = _g_equal(_pad176.zzzz, float4(0.0, 1.0, 0.0, 0.0)).xy;
    u_xlat7.x = float(0.0);
    u_xlat7.y = float(0.0);
    u_xlat7.z = float(0.0);
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<u_xlatu23 ; u_xlatu_loop_1++)
    {
        u_xlatu46 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati66 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat46.x = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati66]);
        u_xlatu46 =  uint(int(u_xlat46.x));
        u_xlat8.xyz = (-input.vs_INTERP8.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
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
        u_xlatu68 = uint(u_xlatu46 >> 5u);
        u_xlati69 = int(1 << int(u_xlatu46));
        u_xlati68 = int(uint(uint(u_xlati69) & uint(_g_floatBitsToUint(_pad64.x))));
        if(u_xlati68 != 0) {
            u_xlati68 = int(_pad20672.x);
            u_xlati69 = (u_xlati68 != 0) ? 0 : 1;
            u_xlati10 = int(int(u_xlatu46) << 2);
            if(u_xlati69 != 0) {
                u_xlat30.xyz = input.vs_INTERP8.yyy * _pad208.xyw;
                u_xlat30.xyz = _pad192.xyw * input.vs_INTERP8.xxx + u_xlat30.xyz;
                u_xlat30.xyz = _pad224.xyw * input.vs_INTERP8.zzz + u_xlat30.xyz;
                u_xlat30.xyz = u_xlat30.xyz + _pad240.xyw;
                u_xlat30.xy = u_xlat30.xy / u_xlat30.zz;
                u_xlat30.xy = u_xlat30.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                u_xlat30.xy = clamp(u_xlat30.xy, 0.0, 1.0);
                u_xlat30.xy = _pad16576.xy * u_xlat30.xy + _pad16576.zw;
            } else {
                u_xlatb68 = u_xlati68==1;
                u_xlati68 = u_xlatb68 ? 1 : int(0);
                if(u_xlati68 != 0) {
                    u_xlat11.xy = input.vs_INTERP8.yy * _pad208.xy;
                    u_xlat11.xy = _pad192.xy * input.vs_INTERP8.xx + u_xlat11.xy;
                    u_xlat11.xy = _pad224.xy * input.vs_INTERP8.zz + u_xlat11.xy;
                    u_xlat11.xy = u_xlat11.xy + _pad240.xy;
                    u_xlat11.xy = u_xlat11.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat11.xy = _g_fract(u_xlat11.xy);
                    u_xlat30.xy = _pad16576.xy * u_xlat11.xy + _pad16576.zw;
                } else {
                    u_xlat11 = input.vs_INTERP8.yyyy * _pad208;
                    u_xlat11 = _pad192 * input.vs_INTERP8.xxxx + u_xlat11;
                    u_xlat11 = _pad224 * input.vs_INTERP8.zzzz + u_xlat11;
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
            u_xlat10 = _g_textureLod(_Texture_t7, u_xlat30.xy, 0.0);
            u_xlat68 = (u_xlatb6.y) ? u_xlat10.w : u_xlat10.x;
            u_xlat10.xyz = (u_xlatb6.x) ? u_xlat10.xyz : float3(u_xlat68);
        } else {
            u_xlat10.x = float(1.0);
            u_xlat10.y = float(1.0);
            u_xlat10.z = float(1.0);
        }
        u_xlat10.xyz = u_xlat10.xyz * _AdditionalLightsColor.xyz;
        u_xlati46 = int(uint(uint(_g_floatBitsToUint(_AdditionalLightsLayerMasks)) & uint(_g_floatBitsToUint(_pad128.x))));
        u_xlat68 = dot(u_xlat20.xyz, u_xlat9.xyz);
        u_xlat68 = clamp(u_xlat68, 0.0, 1.0);
        u_xlat66 = u_xlat66 * u_xlat68;
        u_xlat10.xyz = float3(u_xlat66) * u_xlat10.xyz;
        u_xlat8.xyz = u_xlat8.xyz * float3(u_xlat67) + u_xlat22.xyz;
        u_xlat66 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat66 = max(u_xlat66, 1.17549435e-38);
        u_xlat66 = _g_inversesqrt(u_xlat66);
        u_xlat8.xyz = float3(u_xlat66) * u_xlat8.xyz;
        u_xlat66 = dot(u_xlat20.xyz, u_xlat8.xyz);
        u_xlat66 = clamp(u_xlat66, 0.0, 1.0);
        u_xlat67 = dot(u_xlat9.xyz, u_xlat8.xyz);
        u_xlat67 = clamp(u_xlat67, 0.0, 1.0);
        u_xlat66 = u_xlat66 * u_xlat66;
        u_xlat66 = u_xlat66 * u_xlat43.x + 1.00001001;
        u_xlat67 = u_xlat67 * u_xlat67;
        u_xlat66 = u_xlat66 * u_xlat66;
        u_xlat67 = max(u_xlat67, 0.100000001);
        u_xlat66 = u_xlat66 * u_xlat67;
        u_xlat66 = u_xlat5.x * u_xlat66;
        u_xlat66 = u_xlat64 / u_xlat66;
        u_xlat8.xyz = float3(u_xlat66) * float3(0.0399999991, 0.0399999991, 0.0399999991) + u_xlat1.xyz;
        u_xlat8.xyz = u_xlat8.xyz * u_xlat10.xyz + u_xlat7.xyz;
        u_xlat7.xyz = (int(u_xlati46) != 0) ? u_xlat8.xyz : u_xlat7.xyz;
    }
    u_xlat20.xyz = u_xlat4.xyz + u_xlat25.xyz;
    u_xlat20.xyz = u_xlat7.xyz + u_xlat20.xyz;
    u_xlat1.x = u_xlat3.x * (-u_xlat3.x);
    u_xlat1.x = exp2(u_xlat1.x);
    u_xlat21 = (-u_xlat1.x) + 1.0;
    u_xlat22.xyz = float3(u_xlat21) * _pad992.xyz;
    __SV_Target0.xyz = u_xlat20.xyz * u_xlat1.xxx + u_xlat22.xyz;
    __SV_Target0.w = (u_xlatb2) ? u_xlat61 : 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
