Shader "Shader Graphs/VignetteShader"
{
    Properties
    {


[HideInInspector] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
_VignetteColor ("VignetteColor", Vector) = (1,1,1,1)
_Intensity ("Intensity", Float) = 0
_Center ("Center", Vector) = (0,0,0,0)
_NoiseScale ("NoiseScale", Float) = 0
_NoiseStrength ("NoiseStrength", Float) = 0
_NoiseSpeed ("NoiseSpeed", Vector) = (0,0,0,0)
[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
[HideInInspector] _Stencil ("Stencil ID", Float) = 0
[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
[HideInInspector] _ColorMask ("ColorMask", Float) = 15
[HideInInspector] _ClipRect ("ClipRect", Vector) = (0,0,0,0)
[HideInInspector] _UIMaskSoftnessX ("UIMaskSoftnessX", Float) = 1
[HideInInspector] _UIMaskSoftnessY ("UIMaskSoftnessY", Float) = 1
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



            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4 _TimeParameters;
            float4 _VignetteColor;
            float _Intensity;
            float2 _Center;
            float _NoiseScale;
            float _NoiseStrength;
            float2 _NoiseSpeed;

            float4 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            int2 u_xlati1;
            uint u_xlatu1;
            float4 u_xlat2;
            int4 u_xlati2;
            uint2 u_xlatu2;
            float4 u_xlat3;
            float3 u_xlat4;
            float2 u_xlat5;
            int2 u_xlati5;
            uint2 u_xlatu5;
            float2 u_xlat8;
            int2 u_xlati8;
            uint u_xlatu8;
            float2 u_xlat9;
            float u_xlat12;
            int u_xlati12;
            uint u_xlatu12;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_COLOR0 : COLOR;
                float4 in_TEXCOORD0 : TEXCOORD0;
                float4 in_TEXCOORD1 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 vs_INTERP0 : TEXCOORD0;
                float4 vs_INTERP1 : TEXCOORD1;
                float4 vs_INTERP2 : TEXCOORD2;
                float3 vs_INTERP3 : TEXCOORD3;
                float3 vs_INTERP4 : TEXCOORD4;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0 = input.in_POSITION0.yyyy * _tunity_ObjectToWorld[1];
    u_xlat0 = _tunity_ObjectToWorld[0] * input.in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = _tunity_ObjectToWorld[2] * input.in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + _tunity_ObjectToWorld[3];
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.positionCS = _tunity_MatrixVP[3] * u_xlat0.wwww + u_xlat1;
    output.vs_INTERP0 = input.in_TEXCOORD0;
    output.vs_INTERP1 = input.in_TEXCOORD1;
    output.vs_INTERP2 = input.in_COLOR0;
    u_xlat0.xyz = input.in_POSITION0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[0].xyz * input.in_POSITION0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * input.in_POSITION0.zzz + u_xlat0.xyz;
    output.vs_INTERP3.xyz = u_xlat0.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP4.xyz = float3(u_xlat6) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {


    u_xlat0.xy = float2(_NoiseSpeed.x, _NoiseSpeed.y) * _TimeParameters.xx + input.vs_INTERP0.xy;
    u_xlat0.xy = u_xlat0.xy * float2(float2(_NoiseScale, _NoiseScale));
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
    u_xlat0.xy = float2(_NoiseStrength) * u_xlat0.xx + float2(_Center.x, _Center.y);
    u_xlat0.xy = (-u_xlat0.xy) + input.vs_INTERP0.xy;
    u_xlat0.x = dot(u_xlat0.xy, u_xlat0.xy);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _Intensity;
    u_xlat4.x = input.vs_INTERP2.w * 255.0;
    u_xlat0.y = _g_roundEven(u_xlat4.x);
    u_xlat0.xy = u_xlat0.xy * float2(1.41422713, 0.00392156886);
    u_xlat0.x = u_xlat0.y * u_xlat0.x;
    u_xlat4.xyz = input.vs_INTERP2.xyz;
    u_xlat4.xyz = u_xlat4.xyz * _VignetteColor.xyz;
    __SV_TARGET0.xyz = u_xlat0.xxx * u_xlat4.xyz;
    __SV_TARGET0.w = u_xlat0.x;
    return __SV_TARGET0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
