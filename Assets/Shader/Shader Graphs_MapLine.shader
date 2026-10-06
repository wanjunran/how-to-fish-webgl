Shader "Shader Graphs/MapLine"
{
    Properties
    {


[HideInInspector] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
_Thinness ("Thinness", Float) = 31.76
_Angle ("Angle", Float) = 0
_RightMaskOffset ("RightMaskOffset", Float) = -0.25
_ButtomMaskOffset ("ButtomMaskOffset", Float) = 0
_Speed ("Speed", Float) = 0
_Distance_Step ("Distance Step", Float) = 1
_CircleSmoothstep ("CircleSmoothstep", Vector) = (0,0,0,0)
_LineSmoothstep ("LineSmoothstep", Vector) = (0,0,0,0)
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
            float _PlayerRotation;
            float _RadarRotation;
            float2 _CircleSmoothstep;
            float _ButtomMaskOffset;
            float _Thinness;
            float _RightMaskOffset;

            float4 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            bool2 u_xlatb0;
            float3 u_xlat2;
            float u_xlat3;
            float u_xlat4;
            float3 u_xlat5;
            float2 u_xlat12;
            float2 u_xlat13;
            float u_xlat18;



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


    u_xlat0.x = _RadarRotation + _ButtomMaskOffset;
    u_xlat0.x = u_xlat0.x * 6.28318596;
    u_xlat1.x = cos(u_xlat0.x);
    u_xlat0.x = sin(u_xlat0.x);
    u_xlat0.y = u_xlat1.x;
    u_xlat12.xy = float2(_PlayerRotation, _RadarRotation) * float2(0.0174532924, 6.28318596);
    u_xlat1.x = sin(u_xlat12.x);
    u_xlat2.x = cos(u_xlat12.x);
    u_xlat3 = sin(u_xlat12.y);
    u_xlat4 = cos(u_xlat12.y);
    u_xlat5.x = (-u_xlat1.x);
    u_xlat5.y = u_xlat2.x;
    u_xlat5.z = u_xlat1.x;
    u_xlat12.xy = input.vs_INTERP0.xy + float2(-0.5, -0.5);
    u_xlat1.y = dot(u_xlat12.xy, u_xlat5.xy);
    u_xlat1.x = dot(u_xlat12.xy, u_xlat5.yz);
    u_xlat12.x = dot(u_xlat12.xy, u_xlat12.xy);
    u_xlat12.x = sqrt(u_xlat12.x);
    u_xlat12.x = min(u_xlat12.x, 1.0);
    u_xlat12.x = (-u_xlat12.x) + 1.0;
    u_xlat12.x = u_xlat12.x + (-_CircleSmoothstep.xxyx.y);
    u_xlat18 = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat18 = _g_inversesqrt(u_xlat18);
    u_xlat13.xy = float2(u_xlat18) * u_xlat1.xy;
    u_xlat0.x = dot(u_xlat13.xy, u_xlat0.xy);
    u_xlatb0.x = u_xlat0.x>=0.0;
    u_xlat6 = _RadarRotation + _RightMaskOffset;
    u_xlat6 = u_xlat6 * 6.28318596;
    u_xlat2.x = sin(u_xlat6);
    u_xlat5.x = cos(u_xlat6);
    u_xlat2.y = u_xlat5.x;
    u_xlat6 = dot(u_xlat13.xy, u_xlat2.xy);
    u_xlatb0.y = u_xlat6>=0.0;
    u_xlat0.x = u_xlatb0.x ? float(1.0) : 0.0;
    u_xlat0.y = u_xlatb0.y ? float(1.0) : 0.0;
;
    u_xlat0.x = u_xlat0.x * u_xlat0.y;
    u_xlat2.x = (-u_xlat4);
    u_xlat2.y = u_xlat3;
    u_xlat2.z = u_xlat4;
    u_xlat18 = dot(u_xlat13.xy, u_xlat2.yz);
    u_xlat1.x = dot(u_xlat1.xy, u_xlat2.xy);
    u_xlat1.x = (-_Thinness) * abs(u_xlat1.x) + 1.0;
    u_xlat0.x = u_xlat0.x * u_xlat1.x;
    u_xlat18 = u_xlat18 + 1.21000004;
    u_xlat18 = u_xlat18 * 0.120627262;
    u_xlat18 = clamp(u_xlat18, 0.0, 1.0);
    u_xlat1.x = u_xlat18 * -2.0 + 3.0;
    u_xlat18 = u_xlat18 * u_xlat18;
    u_xlat18 = u_xlat18 * u_xlat1.x;
    u_xlat6 = u_xlat0.y * u_xlat18;
    u_xlat0.x = max(u_xlat6, u_xlat0.x);
    u_xlat0.x = min(u_xlat0.x, 1.0);
    u_xlat6 = (-_CircleSmoothstep.xxyx.y) + _CircleSmoothstep.xxyx.z;
    u_xlat6 = float(1.0) / u_xlat6;
    u_xlat6 = u_xlat6 * u_xlat12.x;
    u_xlat6 = clamp(u_xlat6, 0.0, 1.0);
    u_xlat12.x = u_xlat6 * -2.0 + 3.0;
    u_xlat6 = u_xlat6 * u_xlat6;
    u_xlat6 = u_xlat6 * u_xlat12.x;
    u_xlat0.w = u_xlat0.x * u_xlat6;
    u_xlat1.x = input.vs_INTERP2.w * 255.0;
    u_xlat1.x = _g_roundEven(u_xlat1.x);
    u_xlat1.w = u_xlat1.x * 0.00392156886;
    u_xlat0.x = float(1.0);
    u_xlat0.y = float(1.0);
    u_xlat0.z = float(1.0);
    u_xlat1.xyz = input.vs_INTERP2.xyz;
    u_xlat0 = u_xlat0 * u_xlat1;
    __SV_TARGET0.xyz = u_xlat0.www * u_xlat0.xyz;
    __SV_TARGET0.w = u_xlat0.w;
    return __SV_TARGET0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
