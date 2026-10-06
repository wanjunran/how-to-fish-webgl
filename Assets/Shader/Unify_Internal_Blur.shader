Shader "Unify/Internal/Blur"
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
            // target 级别必须跟目标平台的 GLES 能力对齐，不能照抄 sm50。
            //
            // Unity 的 target 与 GLES 的对应：
            //   3.0 -> GLES 3.0 / SM 4.0
            //   3.5 -> GLES 3.1 / SM 5.0   <- 多compute shader + SSBO
            //
            // 我们的目标是 WebGL2，而 **WebGL2 == GLES 3.0**，没有 SSBO
            // （那是 GLES 3.1 才有的）。原先写 3.5，于是编译器按SM 5.0 的
            // 能力去编译，而 WebGL 后端只能给到 GLES 3.0 —— 整个 shader
            // 编译失败，Unity 把材质渲成洋红，然后**照常打包、照常exit 0**。
            //
            // 实测证据：上一轮CI 抓到的verify_loaded.png 里，**13.19% 的像素
            // 是纯 (254,0,254)**，主菜单一大片元素渲成洋红。这就是本行的
            // 后果 —— 36 个已恢复 shader **全部**是 3.5，无一幸免。
            //
            // 改成 3.0 不改任何算法：只是把「声明需要什么硬件能力」对齐到
            // 目标平台真实提供的能力。
            #pragma target 3.0

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

            TEXTURE2D(_Texture_t0);
            SAMPLER(sampler__Texture_t0);

            float4 _BlitScaleBias;
            float4 _BlitTexture_TexelSize;
            float _BlitMipLevel;
            float _Iteration;
            float4 _BlurParams;

            float3 u_xlat0;
            int u_xlati0;
            uint4 u_xlatu0;
            float4 u_xlat1;
            float4 u_xlat2;
            float4 u_xlat3;
            float u_xlat4;
            uint _g_gl_VertexID : SV_VertexID;



            struct Attributes
            {
                float3 _unused : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 vs_TEXCOORD0 : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;


    u_xlati0 = int(_g_gl_VertexID << 1);
    u_xlatu0.x = uint(uint(u_xlati0) & 2u);
    u_xlatu0.w = uint(uint(_g_gl_VertexID) & 2u);
    u_xlat0.xy = float2(u_xlatu0.xw);
    output.positionCS.xy = u_xlat0.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat0.z = (-u_xlat0.y) + 1.0;
    output.vs_TEXCOORD0.xy = u_xlat0.xz * _BlitScaleBias.xy + _BlitScaleBias.zw;
    output.positionCS.zw = float2(1.0, 1.0);
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {


    u_xlat0.x = float(_g_floatBitsToInt(_Iteration));
    u_xlat4 = float(1.0) / _BlurParams.z;
    u_xlat0.yz = float2(u_xlat4) * _BlitTexture_TexelSize.yx;
    u_xlat0.xyz = u_xlat0.xyz * _BlurParams.wxx;
    u_xlat1.xy = u_xlat0.yz * float2(0.5, 0.5);
    u_xlat0.xy = u_xlat0.yz * u_xlat0.xx + u_xlat1.xy;
    u_xlat1.zw = u_xlat0.xy + input.vs_TEXCOORD0.yx;
    u_xlat1.xy = (-u_xlat0.yx) + input.vs_TEXCOORD0.xy;
    u_xlat0 = _g_textureLod(_Texture_t0, u_xlat1.wz, _BlitMipLevel);
    u_xlat2 = _g_textureLod(_Texture_t0, u_xlat1.xz, _BlitMipLevel);
    u_xlat3 = _g_textureLod(_Texture_t0, u_xlat1.wy, _BlitMipLevel);
    u_xlat1 = _g_textureLod(_Texture_t0, u_xlat1.xy, _BlitMipLevel);
    u_xlat0 = u_xlat0 + u_xlat2;
    u_xlat0 = u_xlat3 + u_xlat0;
    u_xlat0 = u_xlat1 + u_xlat0;
    __SV_Target0 = u_xlat0 * float4(0.25, 0.25, 0.25, 0.25);
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
}
