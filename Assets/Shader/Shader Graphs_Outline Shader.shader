Shader "Shader Graphs/Outline Shader"
{
    Properties
    {



[NoScaleOffset] _Colors ("Colors", 2D) = "white" {}
_Emission ("Emission", Float) = 0
_Outline_Color ("Outline Color", Vector) = (0,0,0,0)
_Outline_Thickness ("Outline Thickness", Float) = 0.1
_Animation_Multiplier ("Animation Multiplier", Float) = 0.5
_Animation_Speed ("Animation Speed", Float) = 1
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



            float4 _TimeParameters;
            float4x4 unity_MatrixV;
            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float _Outline_Thickness;
            float _Animation_Multiplier;
            float _Animation_Speed;
            float4 _Outline_Color;

            float4 u_xlat0;
            float4 u_xlat1;
            float3 u_xlat2;
            float u_xlat9;



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
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);
            float4x4 _tunity_ObjectToWorld = transpose(unity_ObjectToWorld);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0.y = _tunity_MatrixV[0].z;
    u_xlat0.z = _tunity_MatrixV[1].z;
    u_xlat0.x = _tunity_MatrixV[2].z;
    u_xlat1.xyz = u_xlat0.xyz * float3(-0.0, -1.0, -0.0);
    u_xlat1.xyz = u_xlat0.yzx * float3(-0.0, -0.0, -1.0) + (-u_xlat1.xyz);
    u_xlat9 = dot(u_xlat1.yz, u_xlat1.yz);
    u_xlat9 = _g_inversesqrt(u_xlat9);
    u_xlat1.xyz = float3(u_xlat9) * u_xlat1.xyz;
    u_xlat2.xyz = (-u_xlat0.xyz) * u_xlat1.xyz;
    u_xlat0.xyz = (-u_xlat0.zxy) * u_xlat1.yzx + (-u_xlat2.xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = _g_inversesqrt(u_xlat9);
    u_xlat0.xyz = float3(u_xlat9) * u_xlat0.xyz;
    u_xlat9 = dot((-u_xlat0.xyz), (-u_xlat0.xyz));
    u_xlat9 = _g_inversesqrt(u_xlat9);
    u_xlat0.xyz = float3(u_xlat9) * (-u_xlat0.xyz);
    u_xlat2.xyz = u_xlat0.yyy * _tunity_WorldToObject[1].xyz;
    u_xlat0.xyw = _tunity_WorldToObject[0].xyz * u_xlat0.xxx + u_xlat2.xyz;
    u_xlat0.xyz = _tunity_WorldToObject[2].xyz * u_xlat0.zzz + u_xlat0.xyw;
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = _g_inversesqrt(u_xlat9);
    u_xlat0.xyz = float3(u_xlat9) * u_xlat0.xyz;
    u_xlat9 = _TimeParameters.x * _Animation_Speed;
    u_xlat9 = sin(u_xlat9);
    u_xlat9 = u_xlat9 * _Animation_Multiplier + 1.0;
    u_xlat9 = u_xlat9 * _Outline_Thickness;
    u_xlat1.x = dot(_tunity_ObjectToWorld[0].xyz, _tunity_ObjectToWorld[0].xyz);
    u_xlat2.x = sqrt(u_xlat1.x);
    u_xlat1.x = dot(_tunity_ObjectToWorld[1].xyz, _tunity_ObjectToWorld[1].xyz);
    u_xlat2.y = sqrt(u_xlat1.x);
    u_xlat1.x = dot(_tunity_ObjectToWorld[2].xyz, _tunity_ObjectToWorld[2].xyz);
    u_xlat2.z = sqrt(u_xlat1.x);
    u_xlat2.xyz = float3(u_xlat9) / u_xlat2.xyz;
    u_xlat1.xyw = u_xlat1.yyy * _tunity_WorldToObject[2].xyz;
    u_xlat1.xyz = _tunity_WorldToObject[0].xyz * u_xlat1.zzz + u_xlat1.xyw;
    u_xlat9 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat9 = _g_inversesqrt(u_xlat9);
    u_xlat1.xyz = float3(u_xlat9) * u_xlat1.xyz;
    u_xlat1.xyz = u_xlat2.xyz * u_xlat1.xyz;
    u_xlat0.xyz = u_xlat0.xyz * u_xlat2.xyz + u_xlat1.xyz;
    u_xlat0.xyz = u_xlat0.xyz + input.in_POSITION0.xyz;
    u_xlat1.xyz = u_xlat0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyw = _tunity_ObjectToWorld[0].xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * u_xlat0.zzz + u_xlat0.xyw;
    u_xlat0.xyz = u_xlat0.xyz + _tunity_ObjectToWorld[3].xyz;
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.vs_INTERP0.xyz = u_xlat0.xyz;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = max(u_xlat9, 1.17549435e-38);
    u_xlat9 = _g_inversesqrt(u_xlat9);
    output.vs_INTERP1.xyz = float3(u_xlat9) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {


    __SV_Target0.xyz = _Outline_Color.xyz;
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
