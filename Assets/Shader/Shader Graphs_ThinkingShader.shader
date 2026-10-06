Shader "Shader Graphs/ThinkingShader"
{
    Properties
    {



[HideInInspector] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
_OuterColor ("OuterColor", Vector) = (1,1,1,1)
_InnerColor ("InnerColor", Vector) = (1,1,1,1)
_Intensity ("Intensity", Float) = 0
_Center ("Center", Vector) = (0,0,0,0)
_NoiseScale ("NoiseScale", Float) = 0
_NoiseStrength ("NoiseStrength", Float) = 0
_NoiseSpeed ("NoiseSpeed", Vector) = (0,0,0,0)
_NoiseSpeed2 ("NoiseSpeed2", Vector) = (0,0,0,0)
_SmoothStep ("SmoothStep", Vector) = (0,1,0,0)
_AlphaMinMax ("AlphaMinMax", Vector) = (0,1,0,0)
_Steps ("Steps", Float) = 1
_ThinkingAlpha ("ThinkingAlpha", Range(0, 1)) = 1
_Pixels ("Pixels", Float) = 1
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

            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float2 _GlobalMipBias;
            float4 _TimeParameters;
            float4 _ScreenParams;
            float4 _OuterColor;
            float4 _InnerColor;
            float _Intensity;
            float2 _Center;
            float _NoiseScale;
            float _NoiseStrength;
            float2 _NoiseSpeed;
            float2 _SmoothStep;
            float2 _AlphaMinMax;
            float _ThinkingAlpha;
            float _Pixels;
            float2 _NoiseSpeed2;

            float4 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            int2 u_xlati1;
            uint u_xlatu1;
            float4 u_xlat2;
            int4 u_xlati2;
            uint2 u_xlatu2;
            float4 u_xlat3;
            float4 u_xlat4;
            float3 u_xlat5;
            int u_xlati6;
            uint u_xlatu6;
            float2 u_xlat7;
            int3 u_xlati7;
            float3 u_xlat8;
            float2 u_xlat10;
            float2 u_xlat11;
            int2 u_xlati11;
            uint u_xlatu11;
            float2 u_xlat12;
            uint2 u_xlatu12;
            float u_xlat15;
            int u_xlati15;
            uint u_xlatu15;
            float u_xlat16;
            int u_xlati16;
            uint u_xlatu16;



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


    u_xlat0.xy = input.vs_INTERP0.xy + float2(-0.5, -0.5);
    u_xlat0.xy = u_xlat0.xy * _ScreenParams.xy;
    u_xlat0.xy = u_xlat0.xy / _ScreenParams.yy;
    u_xlat0.xy = u_xlat0.xy * float2(float2(_Pixels, _Pixels));
    u_xlat0.xy = floor(u_xlat0.xy);
    u_xlat0.xy = u_xlat0.xy / float2(float2(_Pixels, _Pixels));
    u_xlat0.xy = u_xlat0.xy + float2(0.5, 0.5);
    u_xlat10.xy = float2(_NoiseSpeed.x, _NoiseSpeed.y) * _TimeParameters.xx + u_xlat0.xy;
    u_xlat10.xy = u_xlat10.xy * float2(float2(_NoiseScale, _NoiseScale));
    u_xlat1.xy = floor(u_xlat10.xy);
    u_xlat10.xy = _g_fract(u_xlat10.xy);
    u_xlat11.xy = u_xlat1.xy + float2(1.0, 1.0);
    u_xlati11.xy = int2(u_xlat11.xy);
    u_xlati16 = int(uint(uint(u_xlati11.y) ^ 1103515245u));
    u_xlati11.x = u_xlati16 + u_xlati11.x;
    u_xlatu11 = uint(u_xlati16) * uint(u_xlati11.x);
    u_xlatu16 = uint(u_xlatu11 >> 5u);
    u_xlati11.x = int(uint(u_xlatu16 ^ u_xlatu11));
    u_xlatu11 = uint(u_xlati11.x) * 668265261u;
    u_xlatu11 = uint(u_xlatu11 >> 8u);
    u_xlat11.x = float(u_xlatu11);
    u_xlat2.yz = u_xlat11.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat16 = floor(u_xlat2.y);
    u_xlat2.x = u_xlat11.x * 5.96046519e-08 + (-u_xlat16);
    u_xlat11.x = dot(u_xlat2.xz, u_xlat2.xz);
    u_xlat11.x = _g_inversesqrt(u_xlat11.x);
    u_xlat11.xy = u_xlat11.xx * u_xlat2.xz;
    u_xlat2.xy = u_xlat10.xy + float2(-1.0, -1.0);
    u_xlat11.x = dot(u_xlat11.xy, u_xlat2.xy);
    u_xlat2 = u_xlat1.xyxy + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati1.xy = int2(u_xlat1.xy);
    u_xlati2 = int4(u_xlat2);
    u_xlati7.xz = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati2.xz = u_xlati7.xz + u_xlati2.xz;
    u_xlatu2.xy = uint2(u_xlati7.xz) * uint2(u_xlati2.xz);
    u_xlatu12.xy = uint2(u_xlatu2.x >> 5u, u_xlatu2.y >> 5u);
    u_xlati2.xy = int2(uint2(u_xlatu12.x ^ u_xlatu2.x, u_xlatu12.y ^ u_xlatu2.y));
    u_xlatu2.xy = uint2(u_xlati2.xy) * uint2(668265261u, 668265261u);
    u_xlatu2.xy = uint2(u_xlatu2.x >> 8u, u_xlatu2.y >> 8u);
    u_xlat2.xy = float2(u_xlatu2.xy);
    u_xlat3 = u_xlat2.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat12.xy = floor(u_xlat3.xy);
    u_xlat3.xy = u_xlat2.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat12.xy);
    u_xlat16 = dot(u_xlat3.yw, u_xlat3.yw);
    u_xlat16 = _g_inversesqrt(u_xlat16);
    u_xlat2.xy = float2(u_xlat16) * u_xlat3.yw;
    u_xlat4 = u_xlat10.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat16 = dot(u_xlat2.xy, u_xlat4.zw);
    u_xlat11.x = (-u_xlat16) + u_xlat11.x;
    u_xlat2.xy = u_xlat10.xy * u_xlat10.xy;
    u_xlat2.xy = u_xlat10.xy * u_xlat2.xy;
    u_xlat12.xy = u_xlat10.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat12.xy = u_xlat10.xy * u_xlat12.xy + float2(10.0, 10.0);
    u_xlat2.xy = u_xlat12.xy * u_xlat2.xy;
    u_xlat11.x = u_xlat2.y * u_xlat11.x + u_xlat16;
    u_xlat16 = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat16 = _g_inversesqrt(u_xlat16);
    u_xlat12.xy = float2(u_xlat16) * u_xlat3.xz;
    u_xlat16 = dot(u_xlat12.xy, u_xlat4.xy);
    u_xlati6 = int(uint(uint(u_xlati1.y) ^ 1103515245u));
    u_xlati1.x = u_xlati6 + u_xlati1.x;
    u_xlatu1 = uint(u_xlati6) * uint(u_xlati1.x);
    u_xlatu6 = uint(u_xlatu1 >> 5u);
    u_xlati1.x = int(uint(u_xlatu6 ^ u_xlatu1));
    u_xlatu1 = uint(u_xlati1.x) * 668265261u;
    u_xlatu1 = uint(u_xlatu1 >> 8u);
    u_xlat1.x = float(u_xlatu1);
    u_xlat3.yz = u_xlat1.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat6 = floor(u_xlat3.y);
    u_xlat3.x = u_xlat1.x * 5.96046519e-08 + (-u_xlat6);
    u_xlat1.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    u_xlat1.xy = u_xlat1.xx * u_xlat3.xz;
    u_xlat10.x = dot(u_xlat1.xy, u_xlat10.xy);
    u_xlat15 = (-u_xlat10.x) + u_xlat16;
    u_xlat10.x = u_xlat2.y * u_xlat15 + u_xlat10.x;
    u_xlat15 = (-u_xlat10.x) + u_xlat11.x;
    u_xlat10.x = u_xlat2.x * u_xlat15 + u_xlat10.x;
    u_xlat10.x = u_xlat10.x + 0.5;
    u_xlat1.xy = _NoiseSpeed2.xy * _TimeParameters.xx + u_xlat0.xy;
    u_xlat1.xy = u_xlat1.xy * float2(float2(_NoiseScale, _NoiseScale));
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
    u_xlat2.yz = float2(u_xlat15) * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat7.x = floor(u_xlat2.y);
    u_xlat2.x = u_xlat15 * 5.96046519e-08 + (-u_xlat7.x);
    u_xlat15 = dot(u_xlat2.xz, u_xlat2.xz);
    u_xlat15 = _g_inversesqrt(u_xlat15);
    u_xlat2.xy = float2(u_xlat15) * u_xlat2.xz;
    u_xlat12.xy = u_xlat1.xy + float2(-1.0, -1.0);
    u_xlat15 = dot(u_xlat2.xy, u_xlat12.xy);
    u_xlat2 = u_xlat11.xyxy + float4(0.0, 1.0, 1.0, 0.0);
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
    u_xlat3 = u_xlat2.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat12.xy = floor(u_xlat3.xy);
    u_xlat3.xy = u_xlat2.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat12.xy);
    u_xlat2.x = dot(u_xlat3.yw, u_xlat3.yw);
    u_xlat2.x = _g_inversesqrt(u_xlat2.x);
    u_xlat2.xy = u_xlat2.xx * u_xlat3.yw;
    u_xlat4 = u_xlat1.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat2.x = dot(u_xlat2.xy, u_xlat4.zw);
    u_xlat15 = u_xlat15 + (-u_xlat2.x);
    u_xlat7.xy = u_xlat1.xy * u_xlat1.xy;
    u_xlat7.xy = u_xlat1.xy * u_xlat7.xy;
    u_xlat8.xz = u_xlat1.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat8.xz = u_xlat1.xy * u_xlat8.xz + float2(10.0, 10.0);
    u_xlat7.xy = u_xlat7.xy * u_xlat8.xz;
    u_xlat15 = u_xlat7.y * u_xlat15 + u_xlat2.x;
    u_xlat2.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat2.x = _g_inversesqrt(u_xlat2.x);
    u_xlat2.xw = u_xlat2.xx * u_xlat3.xz;
    u_xlat2.x = dot(u_xlat2.xw, u_xlat4.xy);
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
    u_xlat6 = (-u_xlat1.x) + u_xlat2.x;
    u_xlat1.x = u_xlat7.y * u_xlat6 + u_xlat1.x;
    u_xlat15 = u_xlat15 + (-u_xlat1.x);
    u_xlat15 = u_xlat7.x * u_xlat15 + u_xlat1.x;
    u_xlat10.x = u_xlat10.x + u_xlat15;
    u_xlat10.x = u_xlat10.x + 0.5;
    u_xlat10.x = u_xlat10.x * 0.5 + -0.5;
    u_xlat10.xy = float2(_NoiseStrength) * u_xlat10.xx + float2(_Center.x, _Center.y);
    u_xlat0.xy = (-u_xlat10.xy) + u_xlat0.xy;
    u_xlat0.x = dot(u_xlat0.xy, u_xlat0.xy);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _Intensity;
    u_xlat0.x = u_xlat0.x * _ThinkingAlpha;
    u_xlat0.x = u_xlat0.x * 1.41422713 + (-_SmoothStep.x);
    u_xlat5.xy = (-float2(_SmoothStep.x, _AlphaMinMax.x)) + float2(_SmoothStep.y, _AlphaMinMax.y);
    u_xlat5.x = float(1.0) / u_xlat5.x;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat5.x = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = u_xlat0.x * u_xlat5.x;
    u_xlat5.x = u_xlat0.x * u_xlat5.y + _AlphaMinMax.xxxy.z;
    u_xlat1.w = u_xlat5.x * _ThinkingAlpha;
    u_xlat2 = _g_texture(_Texture_t0, input.vs_INTERP0.xy, _GlobalMipBias.x);
    u_xlat5.xyz = u_xlat2.xyz * _InnerColor.xyz;
    u_xlat2.xyz = (-u_xlat2.xyz) * _InnerColor.xyz + _OuterColor.xyz;
    u_xlat1.xyz = u_xlat0.xxx * u_xlat2.xyz + u_xlat5.xyz;
    u_xlat0.x = input.vs_INTERP2.w * 255.0;
    u_xlat0.x = _g_roundEven(u_xlat0.x);
    u_xlat0.w = u_xlat0.x * 0.00392156886;
    u_xlat0.xyz = input.vs_INTERP2.xyz;
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
