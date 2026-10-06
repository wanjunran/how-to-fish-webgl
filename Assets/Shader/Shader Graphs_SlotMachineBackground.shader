Shader "Shader Graphs/SlotMachineBackground"
{
    Properties
    {


[HideInInspector] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
_Emission ("Emission", Float) = 0
_MainColor ("MainColor", Vector) = (1,1,1,1)
_SecondColor ("SecondColor", Vector) = (1,1,1,1)
_Tightness ("Tightness", Float) = 0.75
_Tiling ("Tiling", Float) = 1
_TilingSpeed ("TilingSpeed", Vector) = (0.1,0,0,0)
_WaveSpeed ("WaveSpeed", Vector) = (3,0.1,0,0)
_WaveAmount ("WaveAmount", Float) = 0
_WaveOffset ("WaveOffset", Float) = 0
_NoiseScale ("NoiseScale", Float) = 1
_NoiseAmount ("NoiseAmount", Float) = 0.25
_Tightness2 ("Tightness2", Float) = 0.75
_Tiling2 ("Tiling2", Float) = 1
_TilingSpeed2 ("TilingSpeed2", Vector) = (0.1,0,0,0)
_WaveSpeed2 ("WaveSpeed2", Vector) = (2,0.1,0,0)
_WaveAmount2 ("WaveAmount2", Float) = 0
_WaveOffset2 ("WaveOffset2", Float) = 0
_NoiseScale2 ("NoiseScale2", Float) = 1
_NoiseAmount2 ("NoiseAmount2", Float) = 0.25
_LineColor1 ("LineColor1", Vector) = (0,0,0,1)
_LineColor2 ("LineColor2", Vector) = (0,0,0,1)
_LineColor3 ("LineColor3", Vector) = (0,0,0,1)
_LineColor4 ("LineColor4", Vector) = (0,0,0,1)
_Pixels ("Pixels", Float) = 1
_BackgroundSpeed ("BackgroundSpeed", Float) = 0
_LineThinness ("LineThinness", Float) = 1
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler__MainTex);

            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float2 _GlobalMipBias;
            float4 _TimeParameters;
            float _Tightness2;
            float4 _LineColor3;
            float4 _LineColor4;
            float _Tiling2;
            float2 _TilingSpeed2;
            float2 _WaveSpeed2;
            float _WaveAmount2;
            float _WaveOffset2;
            float _NoiseScale2;
            float _NoiseAmount2;
            float4 _SecondColor;
            float4 _LineColor2;
            float4 _LineColor1;
            float4 _MainColor;
            float2 _TilingSpeed;
            float _NoiseAmount;
            float _NoiseScale;
            float _Tightness;
            float _WaveAmount;
            float2 _WaveSpeed;
            float _WaveOffset;
            float _Pixels;
            float _BackgroundSpeed;
            float _LineThinness;
            float _Tiling;
            float _Emission;

            float4 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            int2 u_xlati1;
            uint3 u_xlatu1;
            bool3 u_xlatb1;
            float4 u_xlat2;
            int4 u_xlati2;
            uint2 u_xlatu2;
            float4 u_xlat3;
            int4 u_xlati3;
            uint4 u_xlatu3;
            float4 u_xlat4;
            int4 u_xlati4;
            uint2 u_xlatu4;
            float2 u_xlat5;
            int2 u_xlati5;
            uint2 u_xlatu5;
            int2 u_xlati6;
            uint2 u_xlatu6;
            float u_xlat7;
            int3 u_xlati7;
            float2 u_xlat8;
            int3 u_xlati8;
            uint2 u_xlatu8;
            int3 u_xlati9;
            float2 u_xlat10;
            int2 u_xlati10;
            uint2 u_xlatu10;
            float2 u_xlat11;
            int2 u_xlati11;
            uint u_xlatu11;
            float2 u_xlat12;
            uint2 u_xlatu12;
            float u_xlat13;
            int2 u_xlati13;
            uint2 u_xlatu13;
            uint2 u_xlatu14;
            float u_xlat15;
            int u_xlati15;
            uint u_xlatu15;
            float u_xlat16;
            int u_xlati16;
            uint u_xlatu16;
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


    u_xlat0.xy = input.vs_INTERP0.xy * float2(_NoiseScale2);
    u_xlat10.xy = floor(u_xlat0.xy);
    u_xlat0.xy = _g_fract(u_xlat0.xy);
    u_xlat1.xy = u_xlat10.xy + float2(1.0, 1.0);
    u_xlati1.xy = int2(u_xlat1.xy);
    u_xlati6.x = int(uint(uint(u_xlati1.y) ^ 1103515245u));
    u_xlati1.x = u_xlati6.x + u_xlati1.x;
    u_xlatu1.x = uint(u_xlati6.x) * uint(u_xlati1.x);
    u_xlatu6.x = uint(u_xlatu1.x >> 5u);
    u_xlati1.x = int(uint(u_xlatu6.x ^ u_xlatu1.x));
    u_xlatu1.x = uint(u_xlati1.x) * 668265261u;
    u_xlatu1.x = uint(u_xlatu1.x >> 8u);
    u_xlat2 = u_xlat10.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlati10.xy = int2(u_xlat10.xy);
    u_xlati2 = int4(u_xlat2);
    u_xlati6.xy = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati2.xy = u_xlati6.xy + u_xlati2.xz;
    u_xlatu6.xy = uint2(u_xlati6.xy) * uint2(u_xlati2.xy);
    u_xlatu2.xy = uint2(u_xlatu6.x >> 5u, u_xlatu6.y >> 5u);
    u_xlati6.xy = int2(uint2(u_xlatu6.x ^ u_xlatu2.x, u_xlatu6.y ^ u_xlatu2.y));
    u_xlatu6.xy = uint2(u_xlati6.xy) * uint2(668265261u, 668265261u);
    u_xlatu1.yz = uint2(u_xlatu6.x >> 8u, u_xlatu6.y >> 8u);
    u_xlat1.xyz = float3(u_xlatu1.xyz);
    u_xlat11.x = u_xlat1.z * 5.96046519e-08;
    u_xlat1.x = u_xlat1.x * 5.96046519e-08 + (-u_xlat11.x);
    u_xlat2.xy = u_xlat0.xy * u_xlat0.xy;
    u_xlat0.xy = (-u_xlat0.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat0.xy = u_xlat0.xy * u_xlat2.xy;
    u_xlat1.x = u_xlat0.x * u_xlat1.x + u_xlat11.x;
    u_xlati15 = int(uint(uint(u_xlati10.y) ^ 1103515245u));
    u_xlati10.x = u_xlati15 + u_xlati10.x;
    u_xlatu10.x = uint(u_xlati15) * uint(u_xlati10.x);
    u_xlatu15 = uint(u_xlatu10.x >> 5u);
    u_xlati10.x = int(uint(u_xlatu15 ^ u_xlatu10.x));
    u_xlatu10.x = uint(u_xlati10.x) * 668265261u;
    u_xlatu10.x = uint(u_xlatu10.x >> 8u);
    u_xlat10.x = float(u_xlatu10.x);
    u_xlat10.x = u_xlat10.x * 5.96046519e-08;
    u_xlat15 = u_xlat1.y * 5.96046519e-08 + (-u_xlat10.x);
    u_xlat0.x = u_xlat0.x * u_xlat15 + u_xlat10.x;
    u_xlat10.x = (-u_xlat0.x) + u_xlat1.x;
    u_xlat0.x = u_xlat0.y * u_xlat10.x + u_xlat0.x;
    u_xlat1 = float4(_NoiseScale2) * float4(0.5, 0.5, 0.25, 0.25);
    u_xlat1 = u_xlat1 * input.vs_INTERP0.xyxy;
    u_xlat2 = floor(u_xlat1);
    u_xlat1 = _g_fract(u_xlat1);
    u_xlat3 = u_xlat2 + float4(1.0, 1.0, 1.0, 0.0);
    u_xlati3 = int4(u_xlat3);
    u_xlati5.xy = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati3.xy = u_xlati5.xy + u_xlati3.xz;
    u_xlatu5.xy = uint2(u_xlati5.xy) * uint2(u_xlati3.xy);
    u_xlatu3.xy = uint2(u_xlatu5.x >> 5u, u_xlatu5.y >> 5u);
    u_xlati5.xy = int2(uint2(u_xlatu5.x ^ u_xlatu3.x, u_xlatu5.y ^ u_xlatu3.y));
    u_xlatu5.xy = uint2(u_xlati5.xy) * uint2(668265261u, 668265261u);
    u_xlatu5.xy = uint2(u_xlatu5.x >> 8u, u_xlatu5.y >> 8u);
    u_xlat5.xy = float2(u_xlatu5.xy);
    u_xlat3 = u_xlat2.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlati3 = int4(u_xlat3);
    u_xlati8.xz = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati3.xz = u_xlati8.xz + u_xlati3.xz;
    u_xlatu3.xy = uint2(u_xlati8.xz) * uint2(u_xlati3.xz);
    u_xlatu13.xy = uint2(u_xlatu3.x >> 5u, u_xlatu3.y >> 5u);
    u_xlati3.xy = int2(uint2(u_xlatu13.x ^ u_xlatu3.x, u_xlatu13.y ^ u_xlatu3.y));
    u_xlatu3.xy = uint2(u_xlati3.xy) * uint2(668265261u, 668265261u);
    u_xlatu3.xy = uint2(u_xlatu3.x >> 8u, u_xlatu3.y >> 8u);
    u_xlat3.xy = float2(u_xlatu3.xy);
    u_xlat15 = u_xlat3.y * 5.96046519e-08;
    u_xlat5.x = u_xlat5.x * 5.96046519e-08 + (-u_xlat15);
    u_xlat4 = u_xlat1 * u_xlat1;
    u_xlat1 = (-u_xlat1) * float4(2.0, 2.0, 2.0, 2.0) + float4(3.0, 3.0, 3.0, 3.0);
    u_xlat1 = u_xlat1 * u_xlat4;
    u_xlat5.x = u_xlat1.x * u_xlat5.x + u_xlat15;
    u_xlati4 = int4(u_xlat2);
    u_xlat2 = u_xlat2.zwzw + float4(0.0, 1.0, 1.0, 1.0);
    u_xlati2 = int4(u_xlat2);
    u_xlati8.xy = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati4.xy = u_xlati8.xy + u_xlati4.xz;
    u_xlatu8.xy = uint2(u_xlati8.xy) * uint2(u_xlati4.xy);
    u_xlatu4.xy = uint2(u_xlatu8.x >> 5u, u_xlatu8.y >> 5u);
    u_xlati8.xy = int2(uint2(u_xlatu8.x ^ u_xlatu4.x, u_xlatu8.y ^ u_xlatu4.y));
    u_xlatu8.xy = uint2(u_xlati8.xy) * uint2(668265261u, 668265261u);
    u_xlatu8.xy = uint2(u_xlatu8.x >> 8u, u_xlatu8.y >> 8u);
    u_xlat8.xy = float2(u_xlatu8.xy);
    u_xlat8.xy = u_xlat8.xy * float2(5.96046519e-08, 5.96046519e-08);
    u_xlat15 = u_xlat3.x * 5.96046519e-08 + (-u_xlat8.x);
    u_xlat15 = u_xlat1.x * u_xlat15 + u_xlat8.x;
    u_xlat5.x = (-u_xlat15) + u_xlat5.x;
    u_xlat5.x = u_xlat1.y * u_xlat5.x + u_xlat15;
    u_xlat5.x = u_xlat5.x * 0.25;
    u_xlat0.x = u_xlat0.x * 0.125 + u_xlat5.x;
    u_xlat5.x = u_xlat5.y * 5.96046519e-08 + (-u_xlat8.y);
    u_xlat5.x = u_xlat1.z * u_xlat5.x + u_xlat8.y;
    u_xlati10.xy = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati1.xy = u_xlati10.xy + u_xlati2.xz;
    u_xlatu10.xy = uint2(u_xlati10.xy) * uint2(u_xlati1.xy);
    u_xlatu1.xy = uint2(u_xlatu10.x >> 5u, u_xlatu10.y >> 5u);
    u_xlati10.xy = int2(uint2(u_xlatu10.x ^ u_xlatu1.x, u_xlatu10.y ^ u_xlatu1.y));
    u_xlatu10.xy = uint2(u_xlati10.xy) * uint2(668265261u, 668265261u);
    u_xlatu10.xy = uint2(u_xlatu10.x >> 8u, u_xlatu10.y >> 8u);
    u_xlat10.xy = float2(u_xlatu10.xy);
    u_xlat10.x = u_xlat10.x * 5.96046519e-08;
    u_xlat15 = u_xlat10.y * 5.96046519e-08 + (-u_xlat10.x);
    u_xlat10.x = u_xlat1.z * u_xlat15 + u_xlat10.x;
    u_xlat10.x = (-u_xlat5.x) + u_xlat10.x;
    u_xlat5.x = u_xlat1.w * u_xlat10.x + u_xlat5.x;
    u_xlat0.x = u_xlat5.x * 0.5 + u_xlat0.x;
    u_xlat5.x = _WaveOffset2 * input.vs_INTERP0.x + _TimeParameters.x;
    u_xlat5.xy = u_xlat5.xx * _WaveSpeed2.xy;
    u_xlat5.xy = sin(u_xlat5.xy);
    u_xlat5.xy = u_xlat5.xy * float2(float2(_WaveAmount2, _WaveAmount2));
    u_xlat5.xy = float2(_TilingSpeed2.x, _TilingSpeed2.y) * _TimeParameters.xx + u_xlat5.xy;
    u_xlat5.xy = input.vs_INTERP0.xy * float2(_Tiling2) + u_xlat5.xy;
    u_xlat0.xw = (-u_xlat5.xy) + u_xlat0.xx;
    u_xlat0.xy = float2(float2(_NoiseAmount2, _NoiseAmount2)) * u_xlat0.xw + u_xlat5.xy;
    u_xlat0.xy = _g_fract(u_xlat0.xy);
    u_xlat0.xy = u_xlat0.xy / float2(_Tightness2);
    u_xlat0 = _g_texture(_MainTex, u_xlat0.xy, _GlobalMipBias.x);
    u_xlat1.xy = input.vs_INTERP0.xy * float2(float2(_Pixels, _Pixels));
    u_xlat1.xy = floor(u_xlat1.xy);
    u_xlat1.xy = u_xlat1.xy / float2(float2(_Pixels, _Pixels));
    u_xlat1.x = u_xlat1.y + u_xlat1.x;
    u_xlat6 = _TimeParameters.x * _BackgroundSpeed;
    u_xlat1.x = _LineThinness * u_xlat1.x + u_xlat6;
    u_xlat1.x = _g_fract(u_xlat1.x);
    u_xlatb1.xyz = _g_greaterThanEqual(u_xlat1.xxxx, float4(0.75, 0.5, 0.25, 0.0)).xyz;
    u_xlat1.x = u_xlatb1.x ? float(1.0) : 0.0;
    u_xlat1.y = u_xlatb1.y ? float(1.0) : 0.0;
    u_xlat1.z = u_xlatb1.z ? float(1.0) : 0.0;
;
    u_xlat2.xyz = _LineColor2.xyz + (-_LineColor1.xyz);
    u_xlat2.xyz = u_xlat1.xxx * u_xlat2.xyz + _LineColor1.xyz;
    u_xlat2.xyz = u_xlat2.xyz + (-_LineColor3.xyz);
    u_xlat1.xyw = u_xlat1.yyy * u_xlat2.xyz + _LineColor3.xyz;
    u_xlat1.xyw = u_xlat1.xyw + (-_LineColor4.xyz);
    u_xlat1.xyz = u_xlat1.zzz * u_xlat1.xyw + _LineColor4.xyz;
    u_xlat0.xyz = _SecondColor.xyz * u_xlat0.xyz + (-u_xlat1.xyz);
    u_xlat0.xyz = u_xlat0.www * u_xlat0.xyz + u_xlat1.xyz;
    u_xlat1.xy = input.vs_INTERP0.xy * float2(float2(_NoiseScale, _NoiseScale));
    u_xlat11.xy = floor(u_xlat1.xy);
    u_xlat1.xy = _g_fract(u_xlat1.xy);
    u_xlat2.xy = u_xlat11.xy + float2(1.0, 1.0);
    u_xlati2.xy = int2(u_xlat2.xy);
    u_xlati15 = int(uint(uint(u_xlati2.y) ^ 1103515245u));
    u_xlati2.x = u_xlati15 + u_xlati2.x;
    u_xlatu15 = uint(u_xlati15) * uint(u_xlati2.x);
    u_xlatu2.x = uint(u_xlatu15 >> 5u);
    u_xlati15 = int(uint(u_xlatu15 ^ u_xlatu2.x));
    u_xlatu15 = uint(u_xlati15) * 668265261u;
    u_xlatu15 = uint(u_xlatu15 >> 8u);
    u_xlat15 = float(u_xlatu15);
    u_xlat2 = u_xlat11.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlati11.xy = int2(u_xlat11.xy);
    u_xlati2 = int4(u_xlat2);
    u_xlati7.xz = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati2.xz = u_xlati7.xz + u_xlati2.xz;
    u_xlatu2.xy = uint2(u_xlati7.xz) * uint2(u_xlati2.xz);
    u_xlatu12.xy = uint2(u_xlatu2.x >> 5u, u_xlatu2.y >> 5u);
    u_xlati2.xy = int2(uint2(u_xlatu12.x ^ u_xlatu2.x, u_xlatu12.y ^ u_xlatu2.y));
    u_xlatu2.xy = uint2(u_xlati2.xy) * uint2(668265261u, 668265261u);
    u_xlatu2.xy = uint2(u_xlatu2.x >> 8u, u_xlatu2.y >> 8u);
    u_xlat2.xy = float2(u_xlatu2.xy);
    u_xlat7 = u_xlat2.y * 5.96046519e-08;
    u_xlat15 = u_xlat15 * 5.96046519e-08 + (-u_xlat7);
    u_xlat12.xy = u_xlat1.xy * u_xlat1.xy;
    u_xlat1.xy = (-u_xlat1.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat1.xy = u_xlat1.xy * u_xlat12.xy;
    u_xlat15 = u_xlat1.x * u_xlat15 + u_xlat7;
    u_xlati16 = int(uint(uint(u_xlati11.y) ^ 1103515245u));
    u_xlati11.x = u_xlati16 + u_xlati11.x;
    u_xlatu11 = uint(u_xlati16) * uint(u_xlati11.x);
    u_xlatu16 = uint(u_xlatu11 >> 5u);
    u_xlati11.x = int(uint(u_xlatu16 ^ u_xlatu11));
    u_xlatu11 = uint(u_xlati11.x) * 668265261u;
    u_xlatu11 = uint(u_xlatu11 >> 8u);
    u_xlat11.x = float(u_xlatu11);
    u_xlat11.x = u_xlat11.x * 5.96046519e-08;
    u_xlat16 = u_xlat2.x * 5.96046519e-08 + (-u_xlat11.x);
    u_xlat1.x = u_xlat1.x * u_xlat16 + u_xlat11.x;
    u_xlat15 = u_xlat15 + (-u_xlat1.x);
    u_xlat15 = u_xlat1.y * u_xlat15 + u_xlat1.x;
    u_xlat1 = float4(float4(_NoiseScale, _NoiseScale, _NoiseScale, _NoiseScale)) * float4(0.5, 0.5, 0.25, 0.25);
    u_xlat1 = u_xlat1 * input.vs_INTERP0.xyxy;
    u_xlat2 = floor(u_xlat1);
    u_xlat1 = _g_fract(u_xlat1);
    u_xlat3 = u_xlat2 + float4(1.0, 1.0, 1.0, 0.0);
    u_xlati3 = int4(u_xlat3);
    u_xlati8.xz = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati3.xz = u_xlati8.xz + u_xlati3.xz;
    u_xlatu3.xy = uint2(u_xlati8.xz) * uint2(u_xlati3.xz);
    u_xlatu13.xy = uint2(u_xlatu3.x >> 5u, u_xlatu3.y >> 5u);
    u_xlati3.xy = int2(uint2(u_xlatu13.x ^ u_xlatu3.x, u_xlatu13.y ^ u_xlatu3.y));
    u_xlatu3.xy = uint2(u_xlati3.xy) * uint2(668265261u, 668265261u);
    u_xlatu3.xy = uint2(u_xlatu3.x >> 8u, u_xlatu3.y >> 8u);
    u_xlat4 = u_xlat2.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlati4 = int4(u_xlat4);
    u_xlati13.xy = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati4.xy = u_xlati13.xy + u_xlati4.xz;
    u_xlatu13.xy = uint2(u_xlati13.xy) * uint2(u_xlati4.xy);
    u_xlatu4.xy = uint2(u_xlatu13.x >> 5u, u_xlatu13.y >> 5u);
    u_xlati13.xy = int2(uint2(u_xlatu13.x ^ u_xlatu4.x, u_xlatu13.y ^ u_xlatu4.y));
    u_xlatu13.xy = uint2(u_xlati13.xy) * uint2(668265261u, 668265261u);
    u_xlatu3.zw = uint2(u_xlatu13.x >> 8u, u_xlatu13.y >> 8u);
    u_xlat3 = float4(u_xlatu3);
    u_xlat18 = u_xlat3.w * 5.96046519e-08;
    u_xlat3.x = u_xlat3.x * 5.96046519e-08 + (-u_xlat18);
    u_xlat4 = u_xlat1 * u_xlat1;
    u_xlat1 = (-u_xlat1) * float4(2.0, 2.0, 2.0, 2.0) + float4(3.0, 3.0, 3.0, 3.0);
    u_xlat1 = u_xlat1 * u_xlat4;
    u_xlat3.x = u_xlat1.x * u_xlat3.x + u_xlat18;
    u_xlati4 = int4(u_xlat2);
    u_xlat2 = u_xlat2.zwzw + float4(0.0, 1.0, 1.0, 1.0);
    u_xlati2 = int4(u_xlat2);
    u_xlati9.xz = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati4.xz = u_xlati9.xz + u_xlati4.xz;
    u_xlatu4.xy = uint2(u_xlati9.xz) * uint2(u_xlati4.xz);
    u_xlatu14.xy = uint2(u_xlatu4.x >> 5u, u_xlatu4.y >> 5u);
    u_xlati4.xy = int2(uint2(u_xlatu14.x ^ u_xlatu4.x, u_xlatu14.y ^ u_xlatu4.y));
    u_xlatu4.xy = uint2(u_xlati4.xy) * uint2(668265261u, 668265261u);
    u_xlatu4.xy = uint2(u_xlatu4.x >> 8u, u_xlatu4.y >> 8u);
    u_xlat4.xy = float2(u_xlatu4.xy);
    u_xlat4.xy = u_xlat4.xy * float2(5.96046519e-08, 5.96046519e-08);
    u_xlat13 = u_xlat3.z * 5.96046519e-08 + (-u_xlat4.x);
    u_xlat1.x = u_xlat1.x * u_xlat13 + u_xlat4.x;
    u_xlat3.x = (-u_xlat1.x) + u_xlat3.x;
    u_xlat1.x = u_xlat1.y * u_xlat3.x + u_xlat1.x;
    u_xlat1.x = u_xlat1.x * 0.25;
    u_xlat15 = u_xlat15 * 0.125 + u_xlat1.x;
    u_xlat1.x = u_xlat3.y * 5.96046519e-08 + (-u_xlat4.y);
    u_xlat1.x = u_xlat1.z * u_xlat1.x + u_xlat4.y;
    u_xlati7.xz = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati2.xz = u_xlati7.xz + u_xlati2.xz;
    u_xlatu2.xy = uint2(u_xlati7.xz) * uint2(u_xlati2.xz);
    u_xlatu12.xy = uint2(u_xlatu2.x >> 5u, u_xlatu2.y >> 5u);
    u_xlati2.xy = int2(uint2(u_xlatu12.x ^ u_xlatu2.x, u_xlatu12.y ^ u_xlatu2.y));
    u_xlatu2.xy = uint2(u_xlati2.xy) * uint2(668265261u, 668265261u);
    u_xlatu2.xy = uint2(u_xlatu2.x >> 8u, u_xlatu2.y >> 8u);
    u_xlat2.xy = float2(u_xlatu2.xy);
    u_xlat6 = u_xlat2.x * 5.96046519e-08;
    u_xlat2.x = u_xlat2.y * 5.96046519e-08 + (-u_xlat6);
    u_xlat6 = u_xlat1.z * u_xlat2.x + u_xlat6;
    u_xlat6 = (-u_xlat1.x) + u_xlat6;
    u_xlat1.x = u_xlat1.w * u_xlat6 + u_xlat1.x;
    u_xlat15 = u_xlat1.x * 0.5 + u_xlat15;
    u_xlat1.x = input.vs_INTERP0.x * _WaveOffset + _TimeParameters.x;
    u_xlat1.xy = u_xlat1.xx * float2(_WaveSpeed.x, _WaveSpeed.y);
    u_xlat1.xy = sin(u_xlat1.xy);
    u_xlat1.xy = u_xlat1.xy * float2(float2(_WaveAmount, _WaveAmount));
    u_xlat1.xy = _TilingSpeed.xy * _TimeParameters.xx + u_xlat1.xy;
    u_xlat1.xy = input.vs_INTERP0.xy * float2(_Tiling) + u_xlat1.xy;
    u_xlat11.xy = float2(u_xlat15) + (-u_xlat1.xy);
    u_xlat1.xy = float2(float2(_NoiseAmount, _NoiseAmount)) * u_xlat11.xy + u_xlat1.xy;
    u_xlat1.xy = _g_fract(u_xlat1.xy);
    u_xlat1.xy = u_xlat1.xy / float2(_Tightness);
    u_xlat1 = _g_texture(_MainTex, u_xlat1.xy, _GlobalMipBias.x);
    u_xlat1.xyz = _MainColor.xyz * u_xlat1.xyz + (-u_xlat0.xyz);
    u_xlat0.xyz = u_xlat1.www * u_xlat1.xyz + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz * float3(float3(_Emission, _Emission, _Emission)) + u_xlat0.xyz;
    u_xlat1.x = input.vs_INTERP2.w * 255.0;
    u_xlat1.x = _g_roundEven(u_xlat1.x);
    u_xlat1.w = u_xlat1.x * 0.00392156886;
    u_xlat0.w = 1.0;
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
