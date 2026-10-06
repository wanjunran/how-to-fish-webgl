Shader "Shader Graphs/BettingBoxShader"
{
    Properties
    {



[HDR] _Base_Color ("Base Color", Vector) = (0,0,0,1)
_Smoothstep ("Smoothstep", Vector) = (0,0,0,0)
_Alpha_Speed ("Alpha Speed", Float) = 5
_AlphaMap ("AlphaMap", Vector) = (0,1,0,0)
_WaveDensity ("WaveDensity", Float) = 1
_Alpha_Clip ("Alpha Clip", Float) = 0
_WidthPixelation ("WidthPixelation", Float) = 10
_HeightPixelation ("HeightPixelation", Float) = 0
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



            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float _AlphaToMaskAvailable;
            float4 _TimeParameters;
            float4 _Base_Color;
            float2 _Smoothstep;
            float _Alpha_Speed;
            float2 _AlphaMap;
            float _WaveDensity;
            float _Alpha_Clip;
            float _WidthPixelation;
            float _HeightPixelation;

            float3 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            bool u_xlatb0;
            float u_xlat2;
            bool u_xlatb3;
            float u_xlat4;
            int u_xlati6;

int op_not(int value) { return -value - 1; }
int2 op_not(int2 a) { a.x = op_not(a.x); a.y = op_not(a.y); return a; }
int3 op_not(int3 a) { a.x = op_not(a.x); a.y = op_not(a.y); a.z = op_not(a.z); return a; }
int4 op_not(int4 a) { a.x = op_not(a.x); a.y = op_not(a.y); a.z = op_not(a.z); a.w = op_not(a.w); return a; }

            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 vs_INTERP0 : TEXCOORD0;
                float3 vs_INTERP1 : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0.xyz = input.in_POSITION0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[0].xyz * input.in_POSITION0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * input.in_POSITION0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.vs_INTERP0.xyz = u_xlat0.xyz;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP1.xyz = float3(u_xlat6) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0.xyz = input.vs_INTERP0.yyy * _tunity_WorldToObject[1].xyz;
    u_xlat0.xyz = _tunity_WorldToObject[0].xyz * input.vs_INTERP0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = _tunity_WorldToObject[2].xyz * input.vs_INTERP0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + _tunity_WorldToObject[3].xyz;
    u_xlat0.x = u_xlat0.z + u_xlat0.x;
    u_xlat2 = u_xlat0.y + 1.0;
    u_xlat2 = u_xlat2 * _HeightPixelation;
    u_xlat2 = u_xlat2 * 0.5;
    u_xlat0.y = floor(u_xlat2);
    u_xlat0.x = u_xlat0.x * _WaveDensity;
    u_xlat0.x = u_xlat0.x * _WidthPixelation;
    u_xlat0.x = floor(u_xlat0.x);
    u_xlat0.xy = u_xlat0.xy / float2(_WidthPixelation, _HeightPixelation);
    u_xlat0.x = _TimeParameters.x * _Alpha_Speed + u_xlat0.x;
    u_xlat0.x = sin(u_xlat0.x);
    u_xlat4 = (-_AlphaMap.x) + _AlphaMap.y;
    u_xlat0.x = u_xlat0.x * u_xlat4 + _AlphaMap.x;
    u_xlat0.x = u_xlat0.y * u_xlat0.x + (-_Smoothstep.x);
    u_xlat2 = (-_Smoothstep.x) + _Smoothstep.y;
    u_xlat2 = float(1.0) / u_xlat2;
    u_xlat0.x = u_xlat2 * u_xlat0.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat2 = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = (-u_xlat2) * u_xlat0.x + 1.0;
    u_xlat2 = dFdx(u_xlat0.x);
    u_xlat4 = dFdy(u_xlat0.x);
    u_xlat2 = abs(u_xlat4) + abs(u_xlat2);
    u_xlat4 = u_xlat0.x + (-_Alpha_Clip);
    u_xlat6 = (-u_xlat2) * 0.5 + u_xlat4;
    u_xlat2 = max(u_xlat2, 9.99999975e-05);
    u_xlat2 = u_xlat6 / u_xlat2;
    u_xlat2 = u_xlat2 + 1.0;
    u_xlat2 = clamp(u_xlat2, 0.0, 1.0);
    u_xlati6 = int((0.0>=_Alpha_Clip) ? 0xFFFFFFFFu : uint(0));
    u_xlat2 = (u_xlati6 != 0) ? 1.0 : u_xlat2;
    u_xlati6 = op_not(u_xlati6);
    u_xlat1 = u_xlat2 + -9.99999975e-05;
    u_xlatb3 = _AlphaToMaskAvailable!=0.0;
    u_xlati6 = u_xlatb3 ? u_xlati6 : int(0);
    __SV_Target0.w = (u_xlatb3) ? u_xlat2 : u_xlat0.x;
    u_xlat0.x = (u_xlati6 != 0) ? u_xlat1 : u_xlat4;
    u_xlatb0 = u_xlat0.x<0.0;
    if(u_xlatb0){discard;}
    __SV_Target0.xyz = _Base_Color.xyz;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
