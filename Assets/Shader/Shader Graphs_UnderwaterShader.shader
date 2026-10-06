Shader "Shader Graphs/UnderwaterShader"
{
    Properties
    {


[HideInInspector] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
_UnderwaterColor ("UnderwaterColor", Vector) = (1,1,1,1)
_Distortion1 ("Distortion1", Range(0, 1)) = 0.1
_Distortion2 ("Distortion2", Range(0, 1)) = 0.1
_WaveScale1 ("WaveScale1", Float) = 1
_WaveScale2 ("WaveScale2", Float) = 1
_WaveSpeed1 ("WaveSpeed1", Vector) = (0,0,0,0)
_WaveSpeed2 ("WaveSpeed2", Vector) = (0,0,0,0)
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler__MainTex);

            float4x4 unity_MatrixVP;
            float4 _RendererColor;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4 unity_SpriteColor;
            float4 unity_SpriteProps;
            float2 _GlobalMipBias;
            float4 _TimeParameters;
            float4 _UnderwaterColor;
            float _Distortion1;
            float _Distortion2;
            float _WaveScale2;
            float _WaveScale1;
            float2 _WaveSpeed1;
            float2 _WaveSpeed2;

            float4 u_xlat0;
            float4 u_xlat1;
            float3 u_xlat2;
            float u_xlat6;
            int2 u_xlati0;
            uint2 u_xlatu0;
            int4 u_xlati1;
            uint2 u_xlatu1;
            int4 u_xlati2;
            uint2 u_xlatu2;
            float4 u_xlat3;
            float4 u_xlat4;
            int4 u_xlati4;
            uint2 u_xlatu4;
            float4 u_xlat5;
            int4 u_xlati6;
            float3 u_xlat7;
            float2 u_xlat9;
            int3 u_xlati9;
            uint2 u_xlatu9;
            float3 u_xlat11;
            int3 u_xlati11;
            uint3 u_xlatu11;
            float u_xlat14;
            float2 u_xlat16;
            int2 u_xlati16;
            uint2 u_xlatu16;
            float2 u_xlat17;
            float u_xlat21;



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


    u_xlat0 = float4(_WaveSpeed1.x, _WaveSpeed1.y, _WaveSpeed2.x, _WaveSpeed2.y) * _TimeParameters.xxxx + input.vs_INTERP0.xyxy;
    u_xlat0 = u_xlat0 * float4(_WaveScale1, _WaveScale1, _WaveScale2, _WaveScale2);
    u_xlat1 = floor(u_xlat0);
    u_xlat0 = _g_fract(u_xlat0);
    u_xlat2 = u_xlat1.zwzw + float4(1.0, 0.0, 1.0, 1.0);
    u_xlati2 = int4(u_xlat2);
    u_xlati9.xz = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati2.xz = u_xlati9.xz + u_xlati2.xz;
    u_xlatu2.xy = uint2(u_xlati9.xz) * uint2(u_xlati2.xz);
    u_xlatu16.xy = uint2(u_xlatu2.x >> 5u, u_xlatu2.y >> 5u);
    u_xlati2.xy = int2(uint2(u_xlatu16.x ^ u_xlatu2.x, u_xlatu16.y ^ u_xlatu2.y));
    u_xlatu2.xy = uint2(u_xlati2.xy) * uint2(668265261u, 668265261u);
    u_xlatu2.xy = uint2(u_xlatu2.x >> 8u, u_xlatu2.y >> 8u);
    u_xlat2.xy = float2(u_xlatu2.xy);
    u_xlat3 = u_xlat2.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat16.xy = floor(u_xlat3.xy);
    u_xlat3.xy = u_xlat2.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat16.xy);
    u_xlat2.x = dot(u_xlat3.yw, u_xlat3.yw);
    u_xlat2.x = _g_inversesqrt(u_xlat2.x);
    u_xlat2.xy = u_xlat2.xx * u_xlat3.yw;
    u_xlat4 = u_xlat0.zwzw + float4(-1.0, -0.0, -1.0, -1.0);
    u_xlat2.x = dot(u_xlat2.xy, u_xlat4.zw);
    u_xlat9.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat9.x = _g_inversesqrt(u_xlat9.x);
    u_xlat9.xy = u_xlat9.xx * u_xlat3.xz;
    u_xlat9.x = dot(u_xlat9.xy, u_xlat4.xy);
    u_xlat2.x = (-u_xlat9.x) + u_xlat2.x;
    u_xlat3 = u_xlat0 * u_xlat0;
    u_xlat3 = u_xlat0 * u_xlat3;
    u_xlat4 = u_xlat0 * float4(6.0, 6.0, 6.0, 6.0) + float4(-15.0, -15.0, -15.0, -15.0);
    u_xlat4 = u_xlat0 * u_xlat4 + float4(10.0, 10.0, 10.0, 10.0);
    u_xlat3 = u_xlat3 * u_xlat4;
    u_xlat2.x = u_xlat3.w * u_xlat2.x + u_xlat9.x;
    u_xlat4 = u_xlat1 + float4(1.0, 1.0, 0.0, 1.0);
    u_xlati4 = int4(u_xlat4);
    u_xlati9.xy = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati4.xy = u_xlati9.xy + u_xlati4.xz;
    u_xlatu9.xy = uint2(u_xlati9.xy) * uint2(u_xlati4.xy);
    u_xlatu4.xy = uint2(u_xlatu9.x >> 5u, u_xlatu9.y >> 5u);
    u_xlati9.xy = int2(uint2(u_xlatu9.x ^ u_xlatu4.x, u_xlatu9.y ^ u_xlatu4.y));
    u_xlatu9.xy = uint2(u_xlati9.xy) * uint2(668265261u, 668265261u);
    u_xlatu9.xy = uint2(u_xlatu9.x >> 8u, u_xlatu9.y >> 8u);
    u_xlat9.xy = float2(u_xlatu9.xy);
    u_xlat4 = u_xlat9.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat5.xy = floor(u_xlat4.xy);
    u_xlat4.xy = u_xlat9.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat5.xy);
    u_xlat9.x = dot(u_xlat4.yw, u_xlat4.yw);
    u_xlat9.x = _g_inversesqrt(u_xlat9.x);
    u_xlat9.xy = u_xlat9.xx * u_xlat4.yw;
    u_xlat5 = u_xlat0 + float4(-1.0, -1.0, -0.0, -1.0);
    u_xlat9.x = dot(u_xlat9.xy, u_xlat5.zw);
    u_xlati6 = int4(u_xlat1);
    u_xlat1 = u_xlat1.xyxy + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati1 = int4(u_xlat1);
    u_xlati16.xy = int2(uint2(uint(u_xlati6.y) ^ uint(1103515245u), uint(u_xlati6.w) ^ uint(1103515245u)));
    u_xlati11.xz = u_xlati16.xy + u_xlati6.xz;
    u_xlatu16.xy = uint2(u_xlati16.xy) * uint2(u_xlati11.xz);
    u_xlatu11.xz = uint2(u_xlatu16.x >> 5u, u_xlatu16.y >> 5u);
    u_xlati16.xy = int2(uint2(u_xlatu16.x ^ u_xlatu11.x, u_xlatu16.y ^ u_xlatu11.z));
    u_xlatu16.xy = uint2(u_xlati16.xy) * uint2(668265261u, 668265261u);
    u_xlatu16.xy = uint2(u_xlatu16.x >> 8u, u_xlatu16.y >> 8u);
    u_xlat16.xy = float2(u_xlatu16.xy);
    u_xlat6 = u_xlat16.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat11.xz = floor(u_xlat6.xy);
    u_xlat6.xy = u_xlat16.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat11.xz);
    u_xlat16.x = dot(u_xlat6.yw, u_xlat6.yw);
    u_xlat16.x = _g_inversesqrt(u_xlat16.x);
    u_xlat16.xy = u_xlat16.xx * u_xlat6.yw;
    u_xlat14 = dot(u_xlat16.xy, u_xlat0.zw);
    u_xlat21 = (-u_xlat14) + u_xlat9.x;
    u_xlat14 = u_xlat3.w * u_xlat21 + u_xlat14;
    u_xlat21 = (-u_xlat14) + u_xlat2.x;
    u_xlat14 = u_xlat3.z * u_xlat21 + u_xlat14;
    u_xlat14 = u_xlat14 * _Distortion2;
    u_xlat21 = dot(u_xlat6.xz, u_xlat6.xz);
    u_xlat21 = _g_inversesqrt(u_xlat21);
    u_xlat2.xy = float2(u_xlat21) * u_xlat6.xz;
    u_xlat21 = dot(u_xlat2.xy, u_xlat0.xy);
    u_xlat2 = u_xlat0.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlati0.xy = int2(uint2(uint(u_xlati1.y) ^ uint(1103515245u), uint(u_xlati1.w) ^ uint(1103515245u)));
    u_xlati1.xy = u_xlati0.xy + u_xlati1.xz;
    u_xlatu0.xy = uint2(u_xlati0.xy) * uint2(u_xlati1.xy);
    u_xlatu1.xy = uint2(u_xlatu0.x >> 5u, u_xlatu0.y >> 5u);
    u_xlati0.xy = int2(uint2(u_xlatu0.x ^ u_xlatu1.x, u_xlatu0.y ^ u_xlatu1.y));
    u_xlatu0.xy = uint2(u_xlati0.xy) * uint2(668265261u, 668265261u);
    u_xlatu0.xy = uint2(u_xlatu0.x >> 8u, u_xlatu0.y >> 8u);
    u_xlat0.xy = float2(u_xlatu0.xy);
    u_xlat1 = u_xlat0.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat17.xy = floor(u_xlat1.xy);
    u_xlat1.xy = u_xlat0.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat17.xy);
    u_xlat0.x = dot(u_xlat1.xz, u_xlat1.xz);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat0.xy = u_xlat0.xx * u_xlat1.xz;
    u_xlat0.x = dot(u_xlat0.xy, u_xlat2.xy);
    u_xlat0.x = (-u_xlat21) + u_xlat0.x;
    u_xlat0.x = u_xlat3.y * u_xlat0.x + u_xlat21;
    u_xlat7.x = dot(u_xlat4.xz, u_xlat4.xz);
    u_xlat7.x = _g_inversesqrt(u_xlat7.x);
    u_xlat7.xz = u_xlat7.xx * u_xlat4.xz;
    u_xlat7.x = dot(u_xlat7.xz, u_xlat5.xy);
    u_xlat21 = dot(u_xlat1.yw, u_xlat1.yw);
    u_xlat21 = _g_inversesqrt(u_xlat21);
    u_xlat1.xy = float2(u_xlat21) * u_xlat1.yw;
    u_xlat21 = dot(u_xlat1.xy, u_xlat2.zw);
    u_xlat7.x = (-u_xlat21) + u_xlat7.x;
    u_xlat7.x = u_xlat3.y * u_xlat7.x + u_xlat21;
    u_xlat7.x = (-u_xlat0.x) + u_xlat7.x;
    u_xlat0.x = u_xlat3.x * u_xlat7.x + u_xlat0.x;
    u_xlat0.x = u_xlat0.x * _Distortion1 + u_xlat14;
    u_xlat7.xy = input.vs_INTERP0.xy + float2(-0.5, -0.5);
    u_xlat7.x = dot(u_xlat7.xy, u_xlat7.xy);
    u_xlat7.x = sqrt(u_xlat7.x);
    u_xlat7.x = u_xlat7.x * 1.41422713;
    u_xlat7.x = min(u_xlat7.x, 1.0);
    u_xlat7.x = (-u_xlat7.x) + 1.0;
    u_xlat0.xy = u_xlat7.xx * u_xlat0.xx + input.vs_INTERP0.xy;
    u_xlat0 = _g_texture(_MainTex, u_xlat0.xy, _GlobalMipBias.x);
    u_xlat0.xyz = u_xlat0.xyz * _UnderwaterColor.xyz;
    u_xlat0.w = 1.0;
    __SV_TARGET0 = u_xlat0 * input.vs_INTERP1;
    return __SV_TARGET0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
