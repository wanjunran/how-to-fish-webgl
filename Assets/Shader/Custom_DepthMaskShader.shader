Shader "Custom/DepthMaskShader"
{
    Properties
    {

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



            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;

            float4 u_xlat0;
            float4 u_xlat1;



            struct Attributes
            {
                float4 in_POSITION0 : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);


    u_xlat0 = input.in_POSITION0.yyyy * _tunity_ObjectToWorld[1];
    u_xlat0 = _tunity_ObjectToWorld[0] * input.in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = _tunity_ObjectToWorld[2] * input.in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + _tunity_ObjectToWorld[3];
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.positionCS = _tunity_MatrixVP[3] * u_xlat0.wwww + u_xlat1;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {


    __SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
}
