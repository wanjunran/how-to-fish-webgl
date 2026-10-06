Shader "Shader Graphs/CloudSphereShader"
{
    Properties
    {




_CloudHeigth ("CloudHeigth", Float) = 0
_Pixels ("Pixels", Float) = 1
[HDR] _Color ("Color", Vector) = (0.7490196,0.7490196,0.7490196,1)
[HDR] _SSSColor ("SSSColor", Vector) = (1,0.5607843,0,1)
_NormalInfluence ("NormalInfluence", Float) = 0
_SSSPower ("SSSPower", Float) = 0
_SSSIntensity ("SSSIntensity", Float) = 0
_Thickness ("Thickness", Float) = 0
_CloudScale ("CloudScale", Float) = 0
_DetailScale ("DetailScale", Float) = 10
_CloudSmoothStep ("CloudSmoothStep", Vector) = (0,1,0,0)
_DetailSmoothStep ("DetailSmoothStep", Vector) = (0,0,0,0)
_CombinedSmoothStep ("CombinedSmoothStep", Vector) = (0,0,0,0)
_HeightSmoothStep ("HeightSmoothStep", Vector) = (0,0,0,0)
_HeigthSmoothStep2 ("HeigthSmoothStep2", Vector) = (0,0,0,0)
_CloudSpeed ("CloudSpeed", Vector) = (-0.0005,-0.0025,0,0)
_DetailSpeed ("DetailSpeed", Vector) = (0.015,0.01,0,0)
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
            float4 _MainLightPosition;
            float4 _TimeParameters;
            float3 _WorldSpaceCameraPos;
            float4 unity_OrthoParams;
            float4x4 unity_MatrixV;
            float _NormalInfluence;
            float _SSSPower;
            float _SSSIntensity;
            float _Thickness;
            float4 _Color;
            float4 _SSSColor;
            float2 _CloudSmoothStep;
            float _DetailScale;
            float _CloudScale;
            float2 _HeightSmoothStep;
            float _Pixels;
            float2 _DetailSpeed;
            float2 _CloudSpeed;
            float2 _CombinedSmoothStep;
            float2 _DetailSmoothStep;
            float2 _HeigthSmoothStep2;

            float3 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            int2 u_xlati0;
            uint2 u_xlatu0;
            float4 u_xlat2;
            int4 u_xlati2;
            uint2 u_xlatu2;
            float4 u_xlat3;
            int4 u_xlati3;
            uint2 u_xlatu3;
            float4 u_xlat4;
            int4 u_xlati4;
            uint2 u_xlatu4;
            float4 u_xlat5;
            float u_xlat7;
            float2 u_xlat8;
            int2 u_xlati8;
            uint2 u_xlatu8;
            int3 u_xlati9;
            float2 u_xlat10;
            int3 u_xlati10;
            float3 u_xlat11;
            int3 u_xlati11;
            float2 u_xlat14;
            int2 u_xlati14;
            uint2 u_xlatu14;
            float2 u_xlat15;
            int2 u_xlati15;
            uint2 u_xlatu15;
            uint2 u_xlatu16;
            float2 u_xlat17;
            uint2 u_xlatu17;
            uint2 u_xlatu18;
            float u_xlat21;
            bool u_xlatb21;
            float u_xlat22;
            float u_xlat24;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TEXCOORD0 : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 vs_INTERP0 : TEXCOORD0;
                float3 vs_INTERP1 : TEXCOORD1;
                float3 vs_INTERP2 : TEXCOORD2;
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
    output.vs_INTERP1.xyz = u_xlat0.xyz;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    output.vs_INTERP0 = input.in_TEXCOORD0;
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP2.xyz = float3(u_xlat6) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


    u_xlat0 = input.vs_INTERP0.xyxy * float4(float4(_Pixels, _Pixels, _Pixels, _Pixels));
    u_xlat0 = floor(u_xlat0);
    u_xlat0 = u_xlat0 / float4(float4(_Pixels, _Pixels, _Pixels, _Pixels));
    u_xlat0 = _TimeParameters.xxxx * float4(_DetailSpeed.x, _DetailSpeed.y, _CloudSpeed.x, _CloudSpeed.y) + u_xlat0;
    u_xlat1 = u_xlat0 * float4(_CloudScale, _CloudScale, _DetailScale, _DetailScale);
    u_xlat2 = floor(u_xlat1);
    u_xlat1 = _g_fract(u_xlat1);
    u_xlat3 = u_xlat2 + float4(1.0, 1.0, 1.0, 0.0);
    u_xlati3 = int4(u_xlat3);
    u_xlati0.xy = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati3.xy = u_xlati0.xy + u_xlati3.xz;
    u_xlatu0.xy = uint2(u_xlati0.xy) * uint2(u_xlati3.xy);
    u_xlatu3.xy = uint2(u_xlatu0.x >> 5u, u_xlatu0.y >> 5u);
    u_xlati0.xy = int2(uint2(u_xlatu0.x ^ u_xlatu3.x, u_xlatu0.y ^ u_xlatu3.y));
    u_xlatu0.xy = uint2(u_xlati0.xy) * uint2(668265261u, 668265261u);
    u_xlatu0.xy = uint2(u_xlatu0.x >> 8u, u_xlatu0.y >> 8u);
    u_xlat0.xy = float2(u_xlatu0.xy);
    u_xlat3.yz = u_xlat0.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat10.x = floor(u_xlat3.y);
    u_xlat3.x = u_xlat0.x * 5.96046519e-08 + (-u_xlat10.x);
    u_xlat0.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat3.xy = u_xlat0.xx * u_xlat3.xz;
    u_xlat17.xy = u_xlat1.xy + float2(-1.0, -1.0);
    u_xlat0.x = dot(u_xlat3.xy, u_xlat17.xy);
    u_xlat3 = u_xlat2.xyxy + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati3 = int4(u_xlat3);
    u_xlati10.xz = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati3.xz = u_xlati10.xz + u_xlati3.xz;
    u_xlatu3.xy = uint2(u_xlati10.xz) * uint2(u_xlati3.xz);
    u_xlatu17.xy = uint2(u_xlatu3.x >> 5u, u_xlatu3.y >> 5u);
    u_xlati3.xy = int2(uint2(u_xlatu17.x ^ u_xlatu3.x, u_xlatu17.y ^ u_xlatu3.y));
    u_xlatu3.xy = uint2(u_xlati3.xy) * uint2(668265261u, 668265261u);
    u_xlatu3.xy = uint2(u_xlatu3.x >> 8u, u_xlatu3.y >> 8u);
    u_xlat3.xy = float2(u_xlatu3.xy);
    u_xlat4 = u_xlat3.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat17.xy = floor(u_xlat4.xy);
    u_xlat4.xy = u_xlat3.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat17.xy);
    u_xlat3.x = dot(u_xlat4.yw, u_xlat4.yw);
    u_xlat3.x = _g_inversesqrt(u_xlat3.x);
    u_xlat3.xy = u_xlat3.xx * u_xlat4.yw;
    u_xlat5 = u_xlat1.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat3.x = dot(u_xlat3.xy, u_xlat5.zw);
    u_xlat0.x = u_xlat0.x + (-u_xlat3.x);
    u_xlat10.xy = u_xlat1.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat10.xy = u_xlat1.xy * u_xlat10.xy + float2(10.0, 10.0);
    u_xlat6 = u_xlat1 * u_xlat1;
    u_xlat11.xz = u_xlat1.xy * u_xlat6.xy;
    u_xlat10.xy = u_xlat10.xy * u_xlat11.xz;
    u_xlat0.x = u_xlat10.y * u_xlat0.x + u_xlat3.x;
    u_xlat3.x = dot(u_xlat4.xz, u_xlat4.xz);
    u_xlat3.x = _g_inversesqrt(u_xlat3.x);
    u_xlat3.xw = u_xlat3.xx * u_xlat4.xz;
    u_xlat3.x = dot(u_xlat3.xw, u_xlat5.xy);
    u_xlati4 = int4(u_xlat2);
    u_xlat2 = u_xlat2.zwzw + float4(0.0, 1.0, 1.0, 1.0);
    u_xlati2 = int4(u_xlat2);
    u_xlati11.xz = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati4.xz = u_xlati11.xz + u_xlati4.xz;
    u_xlatu4.xy = uint2(u_xlati11.xz) * uint2(u_xlati4.xz);
    u_xlatu18.xy = uint2(u_xlatu4.x >> 5u, u_xlatu4.y >> 5u);
    u_xlati4.xy = int2(uint2(u_xlatu18.x ^ u_xlatu4.x, u_xlatu18.y ^ u_xlatu4.y));
    u_xlatu4.xy = uint2(u_xlati4.xy) * uint2(668265261u, 668265261u);
    u_xlatu4.xy = uint2(u_xlatu4.x >> 8u, u_xlatu4.y >> 8u);
    u_xlat4.xy = float2(u_xlatu4.xy);
    u_xlat5.yz = u_xlat4.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat24 = floor(u_xlat5.y);
    u_xlat5.x = u_xlat4.x * 5.96046519e-08 + (-u_xlat24);
    u_xlat24 = u_xlat4.y * 5.96046519e-08;
    u_xlat4.x = dot(u_xlat5.xz, u_xlat5.xz);
    u_xlat4.x = _g_inversesqrt(u_xlat4.x);
    u_xlat4.xy = u_xlat4.xx * u_xlat5.xz;
    u_xlat1.x = dot(u_xlat4.xy, u_xlat1.xy);
    u_xlat8.xy = (-u_xlat1.zw) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat8.xy = u_xlat8.xy * u_xlat6.zw;
    u_xlat22 = (-u_xlat1.x) + u_xlat3.x;
    u_xlat1.x = u_xlat10.y * u_xlat22 + u_xlat1.x;
    u_xlat0.x = u_xlat0.x + (-u_xlat1.x);
    u_xlat0.x = u_xlat10.x * u_xlat0.x + u_xlat1.x;
    u_xlat0.x = u_xlat0.x + 0.5;
    u_xlat0.x = u_xlat0.x + (-_CloudSmoothStep.x);
    u_xlat1.x = (-_CloudSmoothStep.x) + _CloudSmoothStep.y;
    u_xlat1.x = float(1.0) / u_xlat1.x;
    u_xlat0.x = u_xlat0.x * u_xlat1.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat1.x = u_xlat0.x * -2.0 + 3.0;
    u_xlat7 = u_xlat0.y * 5.96046519e-08 + (-u_xlat24);
    u_xlat7 = u_xlat8.x * u_xlat7 + u_xlat24;
    u_xlati9.xz = int2(uint2(uint(u_xlati2.y) ^ uint(1103515245u), uint(u_xlati2.w) ^ uint(1103515245u)));
    u_xlati2.xz = u_xlati9.xz + u_xlati2.xz;
    u_xlatu2.xy = uint2(u_xlati9.xz) * uint2(u_xlati2.xz);
    u_xlatu16.xy = uint2(u_xlatu2.x >> 5u, u_xlatu2.y >> 5u);
    u_xlati2.xy = int2(uint2(u_xlatu16.x ^ u_xlatu2.x, u_xlatu16.y ^ u_xlatu2.y));
    u_xlatu2.xy = uint2(u_xlati2.xy) * uint2(668265261u, 668265261u);
    u_xlatu2.xy = uint2(u_xlatu2.x >> 8u, u_xlatu2.y >> 8u);
    u_xlat2.xy = float2(u_xlatu2.xy);
    u_xlat22 = u_xlat2.x * 5.96046519e-08;
    u_xlat2.x = u_xlat2.y * 5.96046519e-08 + (-u_xlat22);
    u_xlat8.x = u_xlat8.x * u_xlat2.x + u_xlat22;
    u_xlat8.x = (-u_xlat7) + u_xlat8.x;
    u_xlat7 = u_xlat8.y * u_xlat8.x + u_xlat7;
    u_xlat2 = float4(float4(_DetailScale, _DetailScale, _DetailScale, _DetailScale)) * float4(0.5, 0.5, 0.25, 0.25);
    u_xlat2 = u_xlat0.zwzw * u_xlat2;
    u_xlat3 = floor(u_xlat2);
    u_xlat2 = _g_fract(u_xlat2);
    u_xlat4 = u_xlat3 + float4(1.0, 1.0, 1.0, 0.0);
    u_xlati4 = int4(u_xlat4);
    u_xlati14.xy = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati8.xy = u_xlati14.xy + u_xlati4.xz;
    u_xlatu14.xy = uint2(u_xlati14.xy) * uint2(u_xlati8.xy);
    u_xlatu8.xy = uint2(u_xlatu14.x >> 5u, u_xlatu14.y >> 5u);
    u_xlati14.xy = int2(uint2(u_xlatu14.x ^ u_xlatu8.x, u_xlatu14.y ^ u_xlatu8.y));
    u_xlatu14.xy = uint2(u_xlati14.xy) * uint2(668265261u, 668265261u);
    u_xlatu14.xy = uint2(u_xlatu14.x >> 8u, u_xlatu14.y >> 8u);
    u_xlat14.xy = float2(u_xlatu14.xy);
    u_xlat4 = u_xlat3.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlati4 = int4(u_xlat4);
    u_xlati8.xy = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati4.xy = u_xlati8.xy + u_xlati4.xz;
    u_xlatu8.xy = uint2(u_xlati8.xy) * uint2(u_xlati4.xy);
    u_xlatu4.xy = uint2(u_xlatu8.x >> 5u, u_xlatu8.y >> 5u);
    u_xlati8.xy = int2(uint2(u_xlatu8.x ^ u_xlatu4.x, u_xlatu8.y ^ u_xlatu4.y));
    u_xlatu8.xy = uint2(u_xlati8.xy) * uint2(668265261u, 668265261u);
    u_xlatu8.xy = uint2(u_xlatu8.x >> 8u, u_xlatu8.y >> 8u);
    u_xlat8.xy = float2(u_xlatu8.xy);
    u_xlat15.x = u_xlat8.y * 5.96046519e-08;
    u_xlat14.x = u_xlat14.x * 5.96046519e-08 + (-u_xlat15.x);
    u_xlat4 = u_xlat2 * u_xlat2;
    u_xlat2 = (-u_xlat2) * float4(2.0, 2.0, 2.0, 2.0) + float4(3.0, 3.0, 3.0, 3.0);
    u_xlat2 = u_xlat2 * u_xlat4;
    u_xlat14.x = u_xlat2.x * u_xlat14.x + u_xlat15.x;
    u_xlati4 = int4(u_xlat3);
    u_xlat3 = u_xlat3.zwzw + float4(0.0, 1.0, 1.0, 1.0);
    u_xlati3 = int4(u_xlat3);
    u_xlati15.xy = int2(uint2(uint(u_xlati4.y) ^ uint(1103515245u), uint(u_xlati4.w) ^ uint(1103515245u)));
    u_xlati4.xy = u_xlati15.xy + u_xlati4.xz;
    u_xlatu15.xy = uint2(u_xlati15.xy) * uint2(u_xlati4.xy);
    u_xlatu4.xy = uint2(u_xlatu15.x >> 5u, u_xlatu15.y >> 5u);
    u_xlati15.xy = int2(uint2(u_xlatu15.x ^ u_xlatu4.x, u_xlatu15.y ^ u_xlatu4.y));
    u_xlatu15.xy = uint2(u_xlati15.xy) * uint2(668265261u, 668265261u);
    u_xlatu15.xy = uint2(u_xlatu15.x >> 8u, u_xlatu15.y >> 8u);
    u_xlat15.xy = float2(u_xlatu15.xy);
    u_xlat15.xy = u_xlat15.xy * float2(5.96046519e-08, 5.96046519e-08);
    u_xlat8.x = u_xlat8.x * 5.96046519e-08 + (-u_xlat15.x);
    u_xlat8.x = u_xlat2.x * u_xlat8.x + u_xlat15.x;
    u_xlat14.x = u_xlat14.x + (-u_xlat8.x);
    u_xlat14.x = u_xlat2.y * u_xlat14.x + u_xlat8.x;
    u_xlat14.x = u_xlat14.x * 0.25;
    u_xlat7 = u_xlat7 * 0.125 + u_xlat14.x;
    u_xlat14.x = u_xlat14.y * 5.96046519e-08 + (-u_xlat15.y);
    u_xlat14.x = u_xlat2.z * u_xlat14.x + u_xlat15.y;
    u_xlati8.xy = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati2.xy = u_xlati8.xy + u_xlati3.xz;
    u_xlatu8.xy = uint2(u_xlati8.xy) * uint2(u_xlati2.xy);
    u_xlatu2.xy = uint2(u_xlatu8.x >> 5u, u_xlatu8.y >> 5u);
    u_xlati8.xy = int2(uint2(u_xlatu8.x ^ u_xlatu2.x, u_xlatu8.y ^ u_xlatu2.y));
    u_xlatu8.xy = uint2(u_xlati8.xy) * uint2(668265261u, 668265261u);
    u_xlatu8.xy = uint2(u_xlatu8.x >> 8u, u_xlatu8.y >> 8u);
    u_xlat8.xy = float2(u_xlatu8.xy);
    u_xlat21 = u_xlat8.x * 5.96046519e-08;
    u_xlat8.x = u_xlat8.y * 5.96046519e-08 + (-u_xlat21);
    u_xlat21 = u_xlat2.z * u_xlat8.x + u_xlat21;
    u_xlat21 = (-u_xlat14.x) + u_xlat21;
    u_xlat14.x = u_xlat2.w * u_xlat21 + u_xlat14.x;
    u_xlat7 = u_xlat14.x * 0.5 + u_xlat7;
    u_xlat7 = u_xlat7 + (-_DetailSmoothStep.xxxy.z);
    u_xlat14.xy = (-float2(_DetailSmoothStep.x, _CombinedSmoothStep.x)) + float2(_DetailSmoothStep.y, _CombinedSmoothStep.y);
    u_xlat14.xy = float2(1.0, 1.0) / u_xlat14.xy;
    u_xlat0.y = u_xlat14.x * u_xlat7;
    u_xlat0.y = clamp(u_xlat0.y, 0.0, 1.0);
    u_xlat14.x = u_xlat0.y * -2.0 + 3.0;
    u_xlat0.xy = u_xlat0.xy * u_xlat0.xy;
    u_xlat7 = u_xlat0.y * u_xlat14.x;
    u_xlat0.x = u_xlat1.x * u_xlat0.x + u_xlat7;
    u_xlat0.x = u_xlat0.x + (-_CombinedSmoothStep.x);
    u_xlat0.x = u_xlat14.y * u_xlat0.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat7 = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = u_xlat0.x * u_xlat7;
    u_xlat7 = input.vs_INTERP1.y * _tunity_WorldToObject[1].z;
    u_xlat7 = _tunity_WorldToObject[0].z * input.vs_INTERP1.x + u_xlat7;
    u_xlat7 = _tunity_WorldToObject[2].z * input.vs_INTERP1.z + u_xlat7;
    u_xlat7 = u_xlat7 + _tunity_WorldToObject[3].z;
    u_xlat14.x = u_xlat7 + (-_HeigthSmoothStep2.x);
    u_xlat7 = u_xlat7 + (-_HeightSmoothStep.x);
    u_xlat21 = (-_HeigthSmoothStep2.x) + _HeigthSmoothStep2.y;
    u_xlat21 = float(1.0) / u_xlat21;
    u_xlat14.x = u_xlat21 * u_xlat14.x;
    u_xlat14.x = clamp(u_xlat14.x, 0.0, 1.0);
    u_xlat21 = u_xlat14.x * -2.0 + 3.0;
    u_xlat14.x = u_xlat14.x * u_xlat14.x;
    u_xlat14.x = u_xlat14.x * u_xlat21;
    u_xlat21 = (-_HeightSmoothStep.x) + _HeightSmoothStep.y;
    u_xlat21 = float(1.0) / u_xlat21;
    u_xlat7 = u_xlat21 * u_xlat7;
    u_xlat7 = clamp(u_xlat7, 0.0, 1.0);
    u_xlat21 = u_xlat7 * -2.0 + 3.0;
    u_xlat7 = u_xlat7 * u_xlat7;
    u_xlat7 = u_xlat7 * u_xlat21;
    u_xlat7 = min(u_xlat7, u_xlat14.x);
    __SV_Target0.w = u_xlat0.x * u_xlat7;
    u_xlat0.x = dot(input.vs_INTERP2.xyz, input.vs_INTERP2.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = float(1.0) / u_xlat0.x;
    u_xlat0.xyz = u_xlat0.xxx * input.vs_INTERP2.xyz;
    u_xlat0.xyz = u_xlat0.xyz * float3(_NormalInfluence) + _MainLightPosition.xyz;
    u_xlat21 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat21 = _g_inversesqrt(u_xlat21);
    u_xlat0.xyz = float3(u_xlat21) * u_xlat0.xyz;
    u_xlat1.xyz = (-input.vs_INTERP1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat21 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat21 = _g_inversesqrt(u_xlat21);
    u_xlat1.xyz = float3(u_xlat21) * u_xlat1.xyz;
    u_xlatb21 = unity_OrthoParams.w==0.0;
    u_xlat2.x = (u_xlatb21) ? u_xlat1.x : _tunity_MatrixV[0].z;
    u_xlat2.y = (u_xlatb21) ? u_xlat1.y : _tunity_MatrixV[1].z;
    u_xlat2.z = (u_xlatb21) ? u_xlat1.z : _tunity_MatrixV[2].z;
    u_xlat0.x = dot((-u_xlat0.xyz), u_xlat2.xyz);
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat0.x = log2(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _SSSPower;
    u_xlat0.x = exp2(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _SSSIntensity;
    u_xlat0.xyz = u_xlat0.xxx * _SSSColor.xyz;
    __SV_Target0.xyz = float3(float3(_Thickness, _Thickness, _Thickness)) * u_xlat0.xyz + _Color.xyz;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
