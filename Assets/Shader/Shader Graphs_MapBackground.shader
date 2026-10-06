Shader "Shader Graphs/MapBackground"
{
    Properties
    {


[HideInInspector] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
[HideInInspector] _Stencil ("_Stencil", Float) = 0
[HideInInspector] _StencilComp ("_StencilComp", Float) = 8
[HideInInspector] _StencilOp ("_StencilOp", Float) = 0
[HideInInspector] _StencilWriteMask ("_StencilWriteMask", Float) = 255
[HideInInspector] _StencilReadMask ("_StencilReadMask", Float) = 255
[HideInInspector] _ColorMask ("_ColorMask", Float) = 15
_GridColor ("GridColor", Vector) = (0.01013041,1,0,1)
_Pixels ("Pixels", Float) = 1
_BackgroundColor ("BackgroundColor", Vector) = (0,0,0,1)
_GridThickness ("GridThickness", Float) = 0.12
_PlayerOffsetMultiplier ("PlayerOffsetMultiplier", Float) = 4
_CircleDistanceCheck ("CircleDistanceCheck", Float) = 1
_CircleSmoothstep ("CircleSmoothstep", Vector) = (0.5,1,0,0)
[HideInInspector] White ("Color", Vector) = (1,1,1,1)
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
            float4 _RendererColor;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4 unity_SpriteColor;
            float4 unity_SpriteProps;
            float _MapZoom;
            float2 _PlayerOffset;
            float _PlayerRotation;
            float _GridThickness;
            float4 _GridColor;
            float4 _BackgroundColor;
            float _Pixels;
            float _CircleDistanceCheck;
            float2 _CircleSmoothstep;

            float4 u_xlat0;
            float4 u_xlat1;
            float3 u_xlat2;
            float u_xlat6;
            float2 u_xlat3;
            float3 u_xlat4;
            float2 u_xlat5;
            float u_xlat8;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TEXCOORD0 : TEXCOORD0;
                float4 in_COLOR0 : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 vs_INTERP0 : TEXCOORD0;
                float4 vs_INTERP1 : TEXCOORD1;
                float3 vs_INTERP2 : TEXCOORD2;
                float3 vs_INTERP3 : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0.xy = input.in_POSITION0.xy * unity_SpriteProps.xy;
    u_xlat2.xyz = u_xlat0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[0].xyz * u_xlat0.xxx + u_xlat2.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * input.in_POSITION0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.vs_INTERP2.xyz = u_xlat0.xyz;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    output.vs_INTERP0 = input.in_TEXCOORD0;
    u_xlat0 = _RendererColor * unity_SpriteColor;
    output.vs_INTERP1 = u_xlat0 * input.in_COLOR0;
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP3.xyz = float3(u_xlat6) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {


    u_xlat0.x = _PlayerRotation * 0.0174532924;
    u_xlat1.x = cos(u_xlat0.x);
    u_xlat0.x = sin(u_xlat0.x);
    u_xlat2.x = (-u_xlat0.x);
    u_xlat4.xy = input.vs_INTERP0.xy + float2(-0.5, -0.5);
    u_xlat5.xy = u_xlat4.xy * float2(_MapZoom);
    u_xlat4.x = dot(u_xlat4.xy, u_xlat4.xy);
    u_xlat4.x = sqrt(u_xlat4.x);
    u_xlat4.x = u_xlat4.x * _CircleDistanceCheck;
    u_xlat4.x = clamp(u_xlat4.x, 0.0, 1.0);
    u_xlat4.x = (-u_xlat4.x) + 1.0;
    u_xlat4.x = u_xlat4.x + (-_CircleSmoothstep.xxxy.z);
    u_xlat2.y = u_xlat1.x;
    u_xlat2.z = u_xlat0.x;
    u_xlat3.x = dot(u_xlat5.xy, u_xlat2.yz);
    u_xlat3.y = dot(u_xlat5.xy, u_xlat2.xy);
    u_xlat0.xz = u_xlat3.xy + float2(0.5, 0.5);
    u_xlat0.xz = float2(_PlayerOffset.x, _PlayerOffset.y) * float2(4.5, 4.5) + u_xlat0.xz;
    u_xlat0.xz = u_xlat0.xz * float2(_Pixels);
    u_xlat0.xz = floor(u_xlat0.xz);
    u_xlat0.xz = u_xlat0.xz / float2(_Pixels);
    u_xlat0.xz = _g_fract(u_xlat0.xz);
    u_xlat1.xy = (-u_xlat0.xz) + float2(1.0, 1.0);
    u_xlat0.xz = min(u_xlat0.xz, u_xlat1.xy);
    u_xlat0.xz = float2(1.0, 1.0) / u_xlat0.xz;
    u_xlat0.xz = u_xlat0.xz * float2(_GridThickness);
    u_xlat0.xz = clamp(u_xlat0.xz, 0.0, 1.0);
    u_xlat1.xy = u_xlat0.xz * float2(-2.0, -2.0) + float2(3.0, 3.0);
    u_xlat0.xz = u_xlat0.xz * u_xlat0.xz;
    u_xlat0.xz = u_xlat0.xz * u_xlat1.xy;
    u_xlat0.x = u_xlat0.z + u_xlat0.x;
    u_xlat0.x = min(u_xlat0.x, 1.0);
    u_xlat8 = (-_CircleSmoothstep.xxxy.z) + _CircleSmoothstep.xxxy.w;
    u_xlat8 = float(1.0) / u_xlat8;
    u_xlat4.x = u_xlat8 * u_xlat4.x;
    u_xlat4.x = clamp(u_xlat4.x, 0.0, 1.0);
    u_xlat8 = u_xlat4.x * -2.0 + 3.0;
    u_xlat4.x = u_xlat4.x * u_xlat4.x;
    u_xlat4.x = u_xlat4.x * u_xlat8;
    u_xlat0.x = u_xlat0.x * u_xlat4.x;
    u_xlat4.xyz = _GridColor.xyz + (-_BackgroundColor.xyz);
    u_xlat0.xyz = u_xlat0.xxx * u_xlat4.xyz + _BackgroundColor.xyz;
    u_xlat0.w = 1.0;
    __SV_TARGET0 = u_xlat0 * input.vs_INTERP1;
    return __SV_TARGET0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
