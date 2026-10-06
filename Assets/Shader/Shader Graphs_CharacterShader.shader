Shader "Shader Graphs/CharacterShader"
{
    Properties
    {



[NoScaleOffset] _Colors ("Colors", 2D) = "white" {}
[NoScaleOffset] _Normal_Map ("Normal Map", 2D) = "white" {}
_Normal_Strength ("Normal Strength", Range(0, 1)) = 1
_Normal_Scale ("Normal Scale", Float) = 0.2
_SkinColor ("SkinColor", Vector) = (0,0,0,0)
_PrimaryColor ("PrimaryColor", Vector) = (0,0,0,0)
_SecondColor ("SecondColor", Vector) = (0,0,0,0)
_ThirdColor ("ThirdColor", Vector) = (0,0,0,0)
_NormalInfluence ("NormalInfluence", Float) = 0
_SSSPower ("SSSPower", Float) = 0
_SSSIntensity ("SSSIntensity", Float) = 0
_SSSColor ("SSSColor", Vector) = (0,0,0,1)
_Thickness ("Thickness", Float) = 0
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

            TEXTURECUBE(_Texture_t0);
            SAMPLER(sampler__Texture_t0);
            TEXTURECUBE(_Texture_t1);
            SAMPLER(sampler__Texture_t1);
            TEXTURECUBE(_Texture_t2);
            SAMPLER(sampler__Texture_t2);
            TEXTURE2D(_Texture_t3);
            SAMPLER(sampler__Texture_t3);
            TEXTURE2D(_Texture_t4);
            SAMPLER(sampler__Texture_t4);
            TEXTURE2D(_Texture_t5);
            SAMPLER(sampler__Texture_t5);
            TEXTURE2D(_Texture_t6);
            SAMPLER(sampler__Texture_t6);
            TEXTURE2D(_Texture_t7);
            SAMPLER(sampler__Texture_t7);
            TEXTURE2D(_Texture_t8);
            SAMPLER(sampler__Texture_t8);
            TEXTURE2D(_Texture_t9);
            SAMPLER(sampler__Texture_t9);

            float4 _pad0;
            float4 _pad16;
            float4 _pad32;
            float4 _pad48;
            float4 _pad64;
            float4 _pad176;
            float4 _pad192;
            float4 _pad208;
            float4 _pad224;
            float4 _pad240;
            float4 _pad320;
            float4 _pad336;
            float4 _pad352;
            float4 _pad368;
            float4 _pad384;
            float4 _pad400;
            float4 _pad416;
            float4 _pad448;
            float4 _pad976;
            float4 _pad992;
            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4 _GlossyEnvironmentCubeMap_HDR;
            float2 _GlobalMipBias;
            float4 _MainLightPosition;
            float4 _MainLightColor;
            float _MainLightLayerMask;
            float4 _AdditionalLightsCount;
            float3 _WorldSpaceCameraPos;
            float4 unity_OrthoParams;
            float4x4 unity_MatrixV;
            float4 _AdditionalLightsPosition;
            float4 _AdditionalLightsColor;
            float4 _AdditionalLightsAttenuation;
            float4 _AdditionalLightsSpotDir;
            float4 _pad16576;
            float _AdditionalLightsLayerMasks;
            float4 _pad20672;
            float4 unity_RenderingLayer;
            float4 unity_LightData;
            float4 unity_LightIndices;
            float4 unity_SpecCube0_HDR;
            float4 unity_SpecCube1_HDR;
            float4 unity_SpecCube0_BoxMax;
            float4 unity_SpecCube0_BoxMin;
            float4 unity_SpecCube0_ProbePosition;
            float4 unity_SpecCube0_Rotation;
            float4 unity_SpecCube1_BoxMax;
            float4 unity_SpecCube1_BoxMin;
            float4 unity_SpecCube1_ProbePosition;
            float4 unity_SpecCube1_Rotation;
            float4 _MainLightShadowParams;
            float _Normal_Strength;
            float _Normal_Scale;
            float3 _SkinColor;
            float3 _PrimaryColor;
            float3 _SecondColor;
            float3 _ThirdColor;
            float4 _SSSColor;
            float _Thickness;
            float _SSSPower;
            float _SSSIntensity;
            float _NormalInfluence;

            float3 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            float4 ImmCB_0_0_0[4];
            bool u_xlatb1;
            float3 u_xlat2;
            bool4 u_xlatb2;
            float4 u_xlat3;
            bool4 u_xlatb3;
            float4 u_xlat4;
            float3 u_xlat5;
            float4 u_xlat7;
            float4 u_xlat8;
            float4 u_xlat9;
            bool3 u_xlatb9;
            float4 u_xlat10;
            int u_xlati10;
            bool4 u_xlatb10;
            float4 u_xlat11;
            float4 u_xlat12;
            float4 u_xlat13;
            float4 u_xlat14;
            float4 u_xlat15;
            float4 u_xlat16;
            float4 u_xlat17;
            float4 u_xlat18;
            float4 u_xlat19;
            float4 u_xlat20;
            float3 u_xlat21;
            float3 u_xlat22;
            float3 u_xlat23;
            bool u_xlatb23;
            float2 u_xlat24;
            bool2 u_xlatb24;
            float u_xlat25;
            float3 u_xlat29;
            float3 u_xlat30;
            float3 u_xlat31;
            float3 u_xlat32;
            float2 u_xlat44;
            bool u_xlatb44;
            float2 u_xlat45;
            bool u_xlatb45;
            float2 u_xlat46;
            bool2 u_xlatb46;
            float2 u_xlat49;
            float2 u_xlat51;
            float2 u_xlat52;
            float2 u_xlat56;
            float2 u_xlat57;
            float u_xlat63;
            int u_xlati63;
            uint u_xlatu63;
            float u_xlat64;
            int u_xlati64;
            uint u_xlatu64;
            bool u_xlatb64;
            float u_xlat65;
            uint u_xlatu65;
            bool u_xlatb65;
            float u_xlat66;
            float u_xlat67;
            bool u_xlatb67;
            float u_xlat68;
            int u_xlati68;
            uint u_xlatu68;
            bool u_xlatb68;
            float u_xlat69;
            int u_xlati69;
            bool u_xlatb69;
            float u_xlat70;
            bool u_xlatb70;
            float u_xlat71;
            int u_xlati71;
            uint u_xlatu71;
            bool u_xlatb71;
            int u_xlati72;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TANGENT0 : TANGENT;
                float4 in_TEXCOORD0 : TEXCOORD0;
                float4 in_TEXCOORD1 : TEXCOORD1;
                float4 in_TEXCOORD2 : TEXCOORD2;
                float4 in_TEXCOORD3 : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 vs_INTERP0 : TEXCOORD0;
                float4 vs_INTERP10 : TEXCOORD1;
                float3 vs_INTERP11 : TEXCOORD2;
                float3 vs_INTERP12 : TEXCOORD3;
                float4 vs_INTERP5 : TEXCOORD4;
                float4 vs_INTERP6 : TEXCOORD5;
                float4 vs_INTERP7 : TEXCOORD6;
                float4 vs_INTERP8 : TEXCOORD7;
                float4 vs_INTERP9 : TEXCOORD8;
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
    output.vs_INTERP11.xyz = u_xlat0.xyz;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    output.vs_INTERP0.xy = input.in_TEXCOORD1.xy * _pad400.xy + _pad400.zw;
    u_xlat0.xyz = input.in_TANGENT0.yyy * _tunity_ObjectToWorld[1].xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[0].xyz * input.in_TANGENT0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = _tunity_ObjectToWorld[2].xyz * input.in_TANGENT0.zzz + u_xlat0.xyz;
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP5.xyz = float3(u_xlat6) * u_xlat0.xyz;
    output.vs_INTERP5.w = input.in_TANGENT0.w;
    output.vs_INTERP6 = input.in_TEXCOORD0;
    output.vs_INTERP7 = input.in_TEXCOORD1;
    output.vs_INTERP8 = input.in_TEXCOORD2;
    output.vs_INTERP9 = input.in_TEXCOORD3;
    output.vs_INTERP10 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP12.xyz = float3(u_xlat6) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_0_0[3] = float4(0.0, 0.0, 0.0, 1.0);
    u_xlat0.x = dot(input.vs_INTERP12.xyz, input.vs_INTERP12.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = float(1.0) / u_xlat0.x;
    u_xlat21.xyz = u_xlat0.xxx * input.vs_INTERP12.xyz;
    u_xlatb1 = unity_OrthoParams.w==0.0;
    u_xlat22.xyz = (-input.vs_INTERP11.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat2.x = dot(u_xlat22.xyz, u_xlat22.xyz);
    u_xlat2.x = _g_inversesqrt(u_xlat2.x);
    u_xlat22.xyz = u_xlat22.xyz * u_xlat2.xxx;
    u_xlat2.x = _tunity_MatrixV[0].z;
    u_xlat2.y = _tunity_MatrixV[1].z;
    u_xlat2.z = _tunity_MatrixV[2].z;
    u_xlat1.xyz = (bool(u_xlatb1)) ? u_xlat22.xyz : u_xlat2.xyz;
    u_xlatb2 = _g_greaterThanEqual(input.vs_INTERP8.xxxx, float4(0.150000006, 0.349999994, 0.550000012, 0.75));
    u_xlatb3 = _g_greaterThanEqual(float4(0.25, 0.449999988, 0.649999976, 0.850000024), input.vs_INTERP8.xxxx);
    u_xlatb2.x = u_xlatb2.x && u_xlatb3.x;
    u_xlatb2.y = u_xlatb2.y && u_xlatb3.y;
    u_xlatb2.z = u_xlatb2.z && u_xlatb3.z;
    u_xlatb2.w = u_xlatb2.w && u_xlatb3.w;
    u_xlatb64 = _SkinColor.z>=0.5;
    u_xlatb64 = u_xlatb64 && u_xlatb2.x;
    u_xlatb3.x = _PrimaryColor.z>=0.5;
    u_xlatb23 = u_xlatb2.y && u_xlatb3.x;
    u_xlatb3.x = _SecondColor.z>=0.5;
    u_xlatb44 = u_xlatb2.z && u_xlatb3.x;
    u_xlatb3.x = _ThirdColor.z>=0.5;
    u_xlatb65 = u_xlatb2.w && u_xlatb3.x;
    u_xlat3.xy = (bool(u_xlatb65)) ? _ThirdColor.xy : input.vs_INTERP6.xy;
    u_xlat44.xy = (bool(u_xlatb44)) ? _SecondColor.xy : u_xlat3.xy;
    u_xlat23.xy = (bool(u_xlatb23)) ? _PrimaryColor.xy : u_xlat44.xy;
    u_xlat23.xy = (bool(u_xlatb64)) ? _SkinColor.xy : u_xlat23.xy;
    u_xlat23.xyz = _g_texture(_Texture_t8, u_xlat23.xy, _GlobalMipBias.x).xyz;
    u_xlat3.xyz = u_xlat21.xyz * float3(float3(_NormalInfluence, _NormalInfluence, _NormalInfluence)) + _MainLightPosition.xyz;
    u_xlat64 = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat64 = _g_inversesqrt(u_xlat64);
    u_xlat3.xyz = float3(u_xlat64) * u_xlat3.xyz;
    u_xlat64 = dot((-u_xlat3.xyz), u_xlat1.xyz);
    u_xlat64 = clamp(u_xlat64, 0.0, 1.0);
    u_xlat64 = log2(u_xlat64);
    u_xlat64 = u_xlat64 * _SSSPower;
    u_xlat64 = exp2(u_xlat64);
    u_xlat64 = u_xlat64 * _SSSIntensity;
    u_xlat3.xyz = float3(u_xlat64) * _SSSColor.xyz;
    u_xlat3.xyz = float3(_Thickness) * u_xlat3.xyz + u_xlat23.xyz;
    u_xlat2.xyz = (u_xlatb2.x) ? u_xlat3.xyz : u_xlat23.xyz;
    u_xlat3.xy = input.vs_INTERP9.xy * float2(float2(_Normal_Scale, _Normal_Scale));
    u_xlat3.xyw = _g_texture(_Texture_t9, u_xlat3.xy, _GlobalMipBias.x).xyw;
    u_xlat3.x = u_xlat3.x * u_xlat3.w;
    u_xlat3.xy = u_xlat3.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat64 = dot(u_xlat3.xy, u_xlat3.xy);
    u_xlat64 = min(u_xlat64, 1.0);
    u_xlat64 = (-u_xlat64) + 1.0;
    u_xlat64 = sqrt(u_xlat64);
    u_xlat3.z = max(u_xlat64, 1.00000002e-16);
    u_xlat64 = dot(u_xlat3, u_xlat3);
    u_xlat64 = _g_inversesqrt(u_xlat64);
    u_xlat3.xyz = float3(u_xlat64) * u_xlat3.xyz;
    u_xlat4.xy = u_xlat0.xx * input.vs_INTERP12.xy + u_xlat3.xy;
    u_xlat4.z = u_xlat21.z * u_xlat3.z;
    u_xlat0.x = dot(u_xlat4.xyz, u_xlat4.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat3.xyz = u_xlat4.xyz * u_xlat0.xxx + (-u_xlat21.xyz);
    u_xlat0.xyz = float3(_Normal_Strength) * u_xlat3.xyz + u_xlat21.xyz;
    u_xlat63 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat63 = _g_inversesqrt(u_xlat63);
    u_xlat0.xyz = float3(u_xlat63) * u_xlat0.xyz;
    u_xlat3.xyz = input.vs_INTERP11.xyz + (-_pad320.xyz);
    u_xlat4.xyz = input.vs_INTERP11.xyz + (-_pad336.xyz);
    u_xlat5.xyz = input.vs_INTERP11.xyz + (-_pad352.xyz);
    u_xlat6.xyz = input.vs_INTERP11.xyz + (-_pad368.xyz);
    u_xlat3.x = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat3.y = dot(u_xlat4.xyz, u_xlat4.xyz);
    u_xlat3.z = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlat3.w = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlatb3 = _g_lessThan(u_xlat3, _pad384);
    u_xlat4.x = u_xlatb3.x ? float(1.0) : 0.0;
    u_xlat4.y = u_xlatb3.y ? float(1.0) : 0.0;
    u_xlat4.z = u_xlatb3.z ? float(1.0) : 0.0;
    u_xlat4.w = u_xlatb3.w ? float(1.0) : 0.0;
;
    u_xlat3.x = (u_xlatb3.x) ? float(-1.0) : float(-0.0);
    u_xlat3.y = (u_xlatb3.y) ? float(-1.0) : float(-0.0);
    u_xlat3.z = (u_xlatb3.z) ? float(-1.0) : float(-0.0);
    u_xlat3.xyz = u_xlat3.xyz + u_xlat4.yzw;
    u_xlat4.yzw = max(u_xlat3.xyz, float3(0.0, 0.0, 0.0));
    u_xlat63 = dot(u_xlat4, float4(4.0, 3.0, 2.0, 1.0));
    u_xlat63 = (-u_xlat63) + 4.0;
    u_xlatu63 = uint(u_xlat63);
    u_xlati63 = int(int(u_xlatu63) << 2);
    u_xlat3.xyz = input.vs_INTERP11.yyy * _pad16.xyz;
    u_xlat3.xyz = _pad0.xyz * input.vs_INTERP11.xxx + u_xlat3.xyz;
    u_xlat3.xyz = _pad32.xyz * input.vs_INTERP11.zzz + u_xlat3.xyz;
    u_xlat3.xyz = u_xlat3.xyz + _pad48.xyz;
    u_xlat63 = input.vs_INTERP11.y * _tunity_MatrixV[1].z;
    u_xlat63 = _tunity_MatrixV[0].z * input.vs_INTERP11.x + u_xlat63;
    u_xlat63 = _tunity_MatrixV[2].z * input.vs_INTERP11.z + u_xlat63;
    u_xlat63 = u_xlat63 + _tunity_MatrixV[3].z;
    u_xlat63 = (-u_xlat63) + (-_pad352.y);
    u_xlat63 = max(u_xlat63, 0.0);
    u_xlat63 = u_xlat63 * _pad976.x;
    u_xlat4.xy = input.vs_INTERP7.xy;
    u_xlat4.xy = clamp(u_xlat4.xy, 0.0, 1.0);
    u_xlat5.xyz = _g_texture(_Texture_t3, input.vs_INTERP0.xy, _GlobalMipBias.x).xyz;
    u_xlat6 = _g_texture(_Texture_t4, input.vs_INTERP0.xy, _GlobalMipBias.x);
    u_xlat6.xyz = u_xlat6.xyz + float3(-0.5, -0.5, -0.5);
    u_xlat64 = dot(u_xlat0.xyz, u_xlat6.xyz);
    u_xlat64 = u_xlat64 + 0.5;
    u_xlat5.xyz = float3(u_xlat64) * u_xlat5.xyz;
    u_xlat64 = max(u_xlat6.w, 9.99999975e-05);
    u_xlat5.xyz = u_xlat5.xyz / float3(u_xlat64);
    u_xlat64 = (-u_xlat4.x) * 0.959999979 + 0.959999979;
    u_xlat65 = (-u_xlat64) + u_xlat4.y;
    u_xlat6.xyz = float3(u_xlat64) * u_xlat2.xyz;
    u_xlat2.xyz = u_xlat2.xyz + float3(-0.0399999991, -0.0399999991, -0.0399999991);
    u_xlat2.xyz = u_xlat4.xxx * u_xlat2.xyz + float3(0.0399999991, 0.0399999991, 0.0399999991);
    u_xlat64 = (-u_xlat4.y) + 1.0;
    u_xlat66 = u_xlat64 * u_xlat64;
    u_xlat66 = max(u_xlat66, 0.0078125);
    u_xlat4.x = u_xlat66 * u_xlat66;
    u_xlat65 = u_xlat65 + 1.0;
    u_xlat65 = min(u_xlat65, 1.0);
    u_xlat25 = u_xlat66 * 4.0 + 2.0;
    u_xlatb46.x = 0.0<_MainLightShadowParams.y;
    if(u_xlatb46.x){
        u_xlatb46.x = _MainLightShadowParams.y==1.0;
        if(u_xlatb46.x){
            u_xlat7 = u_xlat3.xyxy + _pad400;
            float3 txVec0 = float3(u_xlat7.xy,u_xlat3.z);
            u_xlat8.x = _g_textureLod(_Texture_t5, txVec0, 0.0);
            float3 txVec1 = float3(u_xlat7.zw,u_xlat3.z);
            u_xlat8.y = _g_textureLod(_Texture_t5, txVec1, 0.0);
            u_xlat7 = u_xlat3.xyxy + _pad416;
            float3 txVec2 = float3(u_xlat7.xy,u_xlat3.z);
            u_xlat8.z = _g_textureLod(_Texture_t5, txVec2, 0.0);
            float3 txVec3 = float3(u_xlat7.zw,u_xlat3.z);
            u_xlat8.w = _g_textureLod(_Texture_t5, txVec3, 0.0);
            u_xlat46.x = dot(u_xlat8, float4(0.25, 0.25, 0.25, 0.25));
        } else {
            u_xlatb67 = _MainLightShadowParams.y==2.0;
            if(u_xlatb67){
                u_xlat7.xy = u_xlat3.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat7.xy = floor(u_xlat7.xy);
                u_xlat49.xy = u_xlat3.xy * _pad448.zw + (-u_xlat7.xy);
                u_xlat8 = u_xlat49.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat9 = u_xlat8.xxzz * u_xlat8.xxzz;
                u_xlat8.xz = u_xlat9.yw * float2(0.0799999982, 0.0799999982);
                u_xlat9.xy = u_xlat9.xz * float2(0.5, 0.5) + (-u_xlat49.xy);
                u_xlat51.xy = (-u_xlat49.xy) + float2(1.0, 1.0);
                u_xlat10.xy = min(u_xlat49.xy, float2(0.0, 0.0));
                u_xlat10.xy = (-u_xlat10.xy) * u_xlat10.xy + u_xlat51.xy;
                u_xlat49.xy = max(u_xlat49.xy, float2(0.0, 0.0));
                u_xlat49.xy = (-u_xlat49.xy) * u_xlat49.xy + u_xlat8.yw;
                u_xlat10.xy = u_xlat10.xy + float2(1.0, 1.0);
                u_xlat49.xy = u_xlat49.xy + float2(1.0, 1.0);
                u_xlat11.xy = u_xlat9.xy * float2(0.159999996, 0.159999996);
                u_xlat9.xy = u_xlat51.xy * float2(0.159999996, 0.159999996);
                u_xlat10.xy = u_xlat10.xy * float2(0.159999996, 0.159999996);
                u_xlat12.xy = u_xlat49.xy * float2(0.159999996, 0.159999996);
                u_xlat49.xy = u_xlat8.yw * float2(0.159999996, 0.159999996);
                u_xlat11.z = u_xlat10.x;
                u_xlat11.w = u_xlat49.x;
                u_xlat9.z = u_xlat12.x;
                u_xlat9.w = u_xlat8.x;
                u_xlat13 = u_xlat9.zwxz + u_xlat11.zwxz;
                u_xlat10.z = u_xlat11.y;
                u_xlat10.w = u_xlat49.y;
                u_xlat12.z = u_xlat9.y;
                u_xlat12.w = u_xlat8.z;
                u_xlat8.xyz = u_xlat10.zyw + u_xlat12.zyw;
                u_xlat9.xyz = u_xlat9.xzw / u_xlat13.zwy;
                u_xlat9.xyz = u_xlat9.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat10.xyz = u_xlat12.zyw / u_xlat8.xyz;
                u_xlat10.xyz = u_xlat10.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat9.xyz = u_xlat9.yxz * _pad448.xxx;
                u_xlat10.xyz = u_xlat10.xyz * _pad448.yyy;
                u_xlat9.w = u_xlat10.x;
                u_xlat11 = u_xlat7.xyxy * _pad448.xyxy + u_xlat9.ywxw;
                u_xlat49.xy = u_xlat7.xy * _pad448.xy + u_xlat9.zw;
                u_xlat10.w = u_xlat9.y;
                u_xlat9.yw = u_xlat10.yz;
                u_xlat12 = u_xlat7.xyxy * _pad448.xyxy + u_xlat9.xyzy;
                u_xlat10 = u_xlat7.xyxy * _pad448.xyxy + u_xlat10.wywz;
                u_xlat9 = u_xlat7.xyxy * _pad448.xyxy + u_xlat9.xwzw;
                u_xlat14 = u_xlat8.xxxy * u_xlat13.zwyz;
                u_xlat15 = u_xlat8.yyzz * u_xlat13;
                u_xlat67 = u_xlat8.z * u_xlat13.y;
                float3 txVec4 = float3(u_xlat11.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec4, 0.0);
                float3 txVec5 = float3(u_xlat11.zw,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec5, 0.0);
                u_xlat69 = u_xlat69 * u_xlat14.y;
                u_xlat68 = u_xlat14.x * u_xlat68 + u_xlat69;
                float3 txVec6 = float3(u_xlat49.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec6, 0.0);
                u_xlat68 = u_xlat14.z * u_xlat69 + u_xlat68;
                float3 txVec7 = float3(u_xlat10.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec7, 0.0);
                u_xlat68 = u_xlat14.w * u_xlat69 + u_xlat68;
                float3 txVec8 = float3(u_xlat12.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec8, 0.0);
                u_xlat68 = u_xlat15.x * u_xlat69 + u_xlat68;
                float3 txVec9 = float3(u_xlat12.zw,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec9, 0.0);
                u_xlat68 = u_xlat15.y * u_xlat69 + u_xlat68;
                float3 txVec10 = float3(u_xlat10.zw,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec10, 0.0);
                u_xlat68 = u_xlat15.z * u_xlat69 + u_xlat68;
                float3 txVec11 = float3(u_xlat9.xy,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec11, 0.0);
                u_xlat68 = u_xlat15.w * u_xlat69 + u_xlat68;
                float3 txVec12 = float3(u_xlat9.zw,u_xlat3.z);
                u_xlat69 = _g_textureLod(_Texture_t5, txVec12, 0.0);
                u_xlat46.x = u_xlat67 * u_xlat69 + u_xlat68;
            } else {
                u_xlat7.xy = u_xlat3.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat7.xy = floor(u_xlat7.xy);
                u_xlat49.xy = u_xlat3.xy * _pad448.zw + (-u_xlat7.xy);
                u_xlat8 = u_xlat49.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat9 = u_xlat8.xxzz * u_xlat8.xxzz;
                u_xlat10.yw = u_xlat9.yw * float2(0.0408160016, 0.0408160016);
                u_xlat8.xz = u_xlat9.xz * float2(0.5, 0.5) + (-u_xlat49.xy);
                u_xlat9.xy = (-u_xlat49.xy) + float2(1.0, 1.0);
                u_xlat51.xy = min(u_xlat49.xy, float2(0.0, 0.0));
                u_xlat9.xy = (-u_xlat51.xy) * u_xlat51.xy + u_xlat9.xy;
                u_xlat51.xy = max(u_xlat49.xy, float2(0.0, 0.0));
                u_xlat29.xz = (-u_xlat51.xy) * u_xlat51.xy + u_xlat8.yw;
                u_xlat9.xy = u_xlat9.xy + float2(2.0, 2.0);
                u_xlat8.yw = u_xlat29.xz + float2(2.0, 2.0);
                u_xlat11.z = u_xlat8.y * 0.0816320032;
                u_xlat12.xyz = u_xlat8.zxw * float3(0.0816320032, 0.0816320032, 0.0816320032);
                u_xlat8.xy = u_xlat9.xy * float2(0.0816320032, 0.0816320032);
                u_xlat11.x = u_xlat12.y;
                u_xlat11.yw = u_xlat49.xx * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat9.xz = u_xlat49.xx * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat9.y = u_xlat8.x;
                u_xlat9.w = u_xlat10.y;
                u_xlat11 = u_xlat9 + u_xlat11;
                u_xlat12.yw = u_xlat49.yy * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat10.xz = u_xlat49.yy * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat10.y = u_xlat8.y;
                u_xlat8 = u_xlat10 + u_xlat12;
                u_xlat9 = u_xlat9 / u_xlat11;
                u_xlat9 = u_xlat9 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat10 = u_xlat10 / u_xlat8;
                u_xlat10 = u_xlat10 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat9 = u_xlat9.wxyz * _pad448.xxxx;
                u_xlat10 = u_xlat10.xwyz * _pad448.yyyy;
                u_xlat12.xzw = u_xlat9.yzw;
                u_xlat12.y = u_xlat10.x;
                u_xlat13 = u_xlat7.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat49.xy = u_xlat7.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.y = u_xlat12.y;
                u_xlat12.y = u_xlat10.z;
                u_xlat14 = u_xlat7.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat15.xy = u_xlat7.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.z = u_xlat12.y;
                u_xlat16 = u_xlat7.xyxy * _pad448.xyxy + u_xlat9.xyxz;
                u_xlat12.y = u_xlat10.w;
                u_xlat17 = u_xlat7.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat30.xy = u_xlat7.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.w = u_xlat12.y;
                u_xlat57.xy = u_xlat7.xy * _pad448.xy + u_xlat9.xw;
                u_xlat10.xzw = u_xlat12.xzw;
                u_xlat12 = u_xlat7.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat52.xy = u_xlat7.xy * _pad448.xy + u_xlat10.wy;
                u_xlat10.x = u_xlat9.x;
                u_xlat7.xy = u_xlat7.xy * _pad448.xy + u_xlat10.xy;
                u_xlat18 = u_xlat8.xxxx * u_xlat11;
                u_xlat19 = u_xlat8.yyyy * u_xlat11;
                u_xlat20 = u_xlat8.zzzz * u_xlat11;
                u_xlat8 = u_xlat8.wwww * u_xlat11;
                float3 txVec13 = float3(u_xlat13.xy,u_xlat3.z);
                u_xlat67 = _g_textureLod(_Texture_t5, txVec13, 0.0);
                float3 txVec14 = float3(u_xlat13.zw,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec14, 0.0);
                u_xlat68 = u_xlat68 * u_xlat18.y;
                u_xlat67 = u_xlat18.x * u_xlat67 + u_xlat68;
                float3 txVec15 = float3(u_xlat49.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec15, 0.0);
                u_xlat67 = u_xlat18.z * u_xlat68 + u_xlat67;
                float3 txVec16 = float3(u_xlat16.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec16, 0.0);
                u_xlat67 = u_xlat18.w * u_xlat68 + u_xlat67;
                float3 txVec17 = float3(u_xlat14.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec17, 0.0);
                u_xlat67 = u_xlat19.x * u_xlat68 + u_xlat67;
                float3 txVec18 = float3(u_xlat14.zw,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec18, 0.0);
                u_xlat67 = u_xlat19.y * u_xlat68 + u_xlat67;
                float3 txVec19 = float3(u_xlat15.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec19, 0.0);
                u_xlat67 = u_xlat19.z * u_xlat68 + u_xlat67;
                float3 txVec20 = float3(u_xlat16.zw,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec20, 0.0);
                u_xlat67 = u_xlat19.w * u_xlat68 + u_xlat67;
                float3 txVec21 = float3(u_xlat17.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec21, 0.0);
                u_xlat67 = u_xlat20.x * u_xlat68 + u_xlat67;
                float3 txVec22 = float3(u_xlat17.zw,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec22, 0.0);
                u_xlat67 = u_xlat20.y * u_xlat68 + u_xlat67;
                float3 txVec23 = float3(u_xlat30.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec23, 0.0);
                u_xlat67 = u_xlat20.z * u_xlat68 + u_xlat67;
                float3 txVec24 = float3(u_xlat57.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec24, 0.0);
                u_xlat67 = u_xlat20.w * u_xlat68 + u_xlat67;
                float3 txVec25 = float3(u_xlat12.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec25, 0.0);
                u_xlat67 = u_xlat8.x * u_xlat68 + u_xlat67;
                float3 txVec26 = float3(u_xlat12.zw,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec26, 0.0);
                u_xlat67 = u_xlat8.y * u_xlat68 + u_xlat67;
                float3 txVec27 = float3(u_xlat52.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec27, 0.0);
                u_xlat67 = u_xlat8.z * u_xlat68 + u_xlat67;
                float3 txVec28 = float3(u_xlat7.xy,u_xlat3.z);
                u_xlat68 = _g_textureLod(_Texture_t5, txVec28, 0.0);
                u_xlat46.x = u_xlat8.w * u_xlat68 + u_xlat67;
            }
        }
    } else {
        float3 txVec29 = float3(u_xlat3.xy,u_xlat3.z);
        u_xlat46.x = _g_textureLod(_Texture_t5, txVec29, 0.0);
    }
    u_xlat3.x = (-_MainLightShadowParams.x) + 1.0;
    u_xlat3.x = u_xlat46.x * _MainLightShadowParams.x + u_xlat3.x;
    u_xlatb24.x = 0.0>=u_xlat3.z;
    u_xlatb45 = u_xlat3.z>=1.0;
    u_xlatb24.x = u_xlatb45 || u_xlatb24.x;
    u_xlat3.x = (u_xlatb24.x) ? 1.0 : u_xlat3.x;
    u_xlat7.xyz = input.vs_INTERP11.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat24.x = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat24.x = u_xlat24.x * _MainLightShadowParams.z + _MainLightShadowParams.w;
    u_xlat24.x = clamp(u_xlat24.x, 0.0, 1.0);
    u_xlat45.x = (-u_xlat3.x) + 1.0;
    u_xlat3.x = u_xlat24.x * u_xlat45.x + u_xlat3.x;
    u_xlatb24.x = _pad176.y!=-1.0;
    if(u_xlatb24.x){
        u_xlat24.xy = input.vs_INTERP11.yy * _pad16.xy;
        u_xlat24.xy = _pad0.xy * input.vs_INTERP11.xx + u_xlat24.xy;
        u_xlat24.xy = _pad32.xy * input.vs_INTERP11.zz + u_xlat24.xy;
        u_xlat24.xy = u_xlat24.xy + _pad48.xy;
        u_xlat24.xy = u_xlat24.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
        u_xlat7 = _g_texture(_Texture_t6, u_xlat24.xy, _GlobalMipBias.x);
        u_xlatb24.xy = _g_equal(_pad176.yyyy, float4(0.0, 1.0, 0.0, 0.0)).xy;
        u_xlat45.x = (u_xlatb24.y) ? u_xlat7.w : u_xlat7.x;
        u_xlat7.xyz = (u_xlatb24.x) ? u_xlat7.xyz : u_xlat45.xxx;
    } else {
        u_xlat7.x = float(1.0);
        u_xlat7.y = float(1.0);
        u_xlat7.z = float(1.0);
    }
    u_xlat7.xyz = u_xlat7.xyz * _MainLightColor.xyz;
    u_xlat24.x = dot((-u_xlat1.xyz), u_xlat0.xyz);
    u_xlat24.x = u_xlat24.x + u_xlat24.x;
    u_xlat8.xyz = u_xlat0.xyz * (-u_xlat24.xxx) + (-u_xlat1.xyz);
    u_xlat24.x = dot(u_xlat0.xyz, u_xlat1.xyz);
    u_xlat24.x = clamp(u_xlat24.x, 0.0, 1.0);
    u_xlat24.x = (-u_xlat24.x) + 1.0;
    u_xlat24.x = u_xlat24.x * u_xlat24.x;
    u_xlat24.x = u_xlat24.x * u_xlat24.x;
    u_xlat45.x = (-u_xlat64) * 0.699999988 + 1.70000005;
    u_xlat64 = u_xlat64 * u_xlat45.x;
    u_xlat64 = u_xlat64 * 6.0;
    u_xlat9.xyz = unity_SpecCube0_BoxMax.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat10.xyz = u_xlat9.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube0_BoxMin.xyz;
    u_xlat11.xyz = (-u_xlat10.xyz) + input.vs_INTERP11.xyz;
    u_xlat12 = unity_SpecCube0_Rotation.zzxy + unity_SpecCube0_Rotation.zzxy;
    u_xlat13.xyz = u_xlat12.zwy * unity_SpecCube0_Rotation.xyz;
    u_xlat14 = u_xlat12 * unity_SpecCube0_Rotation.xyww;
    u_xlat45.x = u_xlat12.y * unity_SpecCube0_Rotation.w;
    u_xlat13.xyz = u_xlat13.zzy + u_xlat13.yxx;
    u_xlat13.xyz = (-u_xlat13.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat46.xy = u_xlat11.xy * u_xlat13.xy;
    u_xlat68 = unity_SpecCube0_Rotation.x * u_xlat12.w + (-u_xlat45.x);
    u_xlat46.x = u_xlat68 * u_xlat11.y + u_xlat46.x;
    u_xlat14.xy = u_xlat14.wz + u_xlat14.xy;
    u_xlat69 = u_xlat11.y * u_xlat14.y;
    u_xlat15.x = u_xlat14.x * u_xlat11.z + u_xlat46.x;
    u_xlat45.x = unity_SpecCube0_Rotation.x * u_xlat12.w + u_xlat45.x;
    u_xlat46.x = u_xlat45.x * u_xlat11.x + u_xlat46.y;
    u_xlat32.xz = unity_SpecCube0_Rotation.yx * u_xlat12.yx + (-u_xlat14.zw);
    u_xlat15.y = u_xlat32.x * u_xlat11.z + u_xlat46.x;
    u_xlat46.x = u_xlat32.z * u_xlat11.x + u_xlat69;
    u_xlat15.z = u_xlat13.z * u_xlat11.z + u_xlat46.x;
    u_xlat10.xyz = u_xlat10.xyz + u_xlat15.xyz;
    u_xlat12.xyz = unity_SpecCube1_BoxMax.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat15.xyz = u_xlat12.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube1_BoxMin.xyz;
    u_xlat16.xyz = (-u_xlat15.xyz) + input.vs_INTERP11.xyz;
    u_xlat17 = unity_SpecCube1_Rotation.zzxy + unity_SpecCube1_Rotation.zzxy;
    u_xlat18.xyz = u_xlat17.zwy * unity_SpecCube1_Rotation.xyz;
    u_xlat19 = u_xlat17 * unity_SpecCube1_Rotation.xyww;
    u_xlat46.x = u_xlat17.y * unity_SpecCube1_Rotation.w;
    u_xlat18.xyz = u_xlat18.zzy + u_xlat18.yxx;
    u_xlat18.xyz = (-u_xlat18.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat11.xz = u_xlat16.xy * u_xlat18.xy;
    u_xlat67 = unity_SpecCube1_Rotation.x * u_xlat17.w + (-u_xlat46.x);
    u_xlat69 = u_xlat67 * u_xlat16.y + u_xlat11.x;
    u_xlat56.xy = u_xlat19.wz + u_xlat19.xy;
    u_xlat70 = u_xlat16.y * u_xlat56.y;
    u_xlat20.x = u_xlat56.x * u_xlat16.z + u_xlat69;
    u_xlat46.x = unity_SpecCube1_Rotation.x * u_xlat17.w + u_xlat46.x;
    u_xlat69 = u_xlat46.x * u_xlat16.x + u_xlat11.z;
    u_xlat11.xz = unity_SpecCube1_Rotation.yx * u_xlat17.yx + (-u_xlat19.zw);
    u_xlat20.y = u_xlat11.x * u_xlat16.z + u_xlat69;
    u_xlat69 = u_xlat11.z * u_xlat16.x + u_xlat70;
    u_xlat20.z = u_xlat18.z * u_xlat16.z + u_xlat69;
    u_xlat15.xyz = u_xlat15.xyz + u_xlat20.xyz;
    u_xlat69 = dot(u_xlat9.xyz, u_xlat9.xyz);
    u_xlat70 = dot(u_xlat12.xyz, u_xlat12.xyz);
    u_xlat69 = u_xlat69 + (-u_xlat70);
    u_xlatb70 = 0.0<unity_SpecCube1_BoxMin.w;
    u_xlatb71 = unity_SpecCube1_BoxMin.w==0.0;
    u_xlatb9.x = u_xlat69<-9.99999975e-05;
    u_xlatb9.x = u_xlatb71 && u_xlatb9.x;
    u_xlatb70 = u_xlatb70 || u_xlatb9.x;
    u_xlatb9.x = unity_SpecCube1_BoxMin.w<0.0;
    u_xlatb69 = 9.99999975e-05<u_xlat69;
    u_xlatb69 = u_xlatb69 && u_xlatb71;
    u_xlatb69 = u_xlatb69 || u_xlatb9.x;
    u_xlat9.xyz = u_xlat10.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat12.xyz = (-u_xlat10.xyz) + unity_SpecCube0_BoxMax.xyz;
    u_xlat9.xyz = min(u_xlat9.xyz, u_xlat12.xyz);
    u_xlat9.xyz = u_xlat9.xyz / unity_SpecCube0_BoxMax.www;
    u_xlat71 = min(u_xlat9.z, u_xlat9.y);
    u_xlat71 = min(u_xlat71, u_xlat9.x);
    u_xlat71 = clamp(u_xlat71, 0.0, 1.0);
    u_xlat9.xyz = u_xlat15.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat12.xyz = (-u_xlat15.xyz) + unity_SpecCube1_BoxMax.xyz;
    u_xlat9.xyz = min(u_xlat9.xyz, u_xlat12.xyz);
    u_xlat9.xyz = u_xlat9.xyz / unity_SpecCube1_BoxMax.www;
    u_xlat30.x = min(u_xlat9.z, u_xlat9.y);
    u_xlat9.x = min(u_xlat30.x, u_xlat9.x);
    u_xlat9.x = clamp(u_xlat9.x, 0.0, 1.0);
    u_xlat30.x = (-u_xlat9.x) + 1.0;
    u_xlat30.x = min(u_xlat71, u_xlat30.x);
    u_xlat69 = (u_xlatb69) ? u_xlat30.x : u_xlat71;
    u_xlat71 = (-u_xlat71) + 1.0;
    u_xlat71 = min(u_xlat71, u_xlat9.x);
    u_xlat70 = (u_xlatb70) ? u_xlat71 : u_xlat9.x;
    u_xlat71 = u_xlat69 + u_xlat70;
    u_xlat9.x = max(u_xlat71, 1.0);
    u_xlat69 = u_xlat69 / u_xlat9.x;
    u_xlat70 = u_xlat70 / u_xlat9.x;
    u_xlatb9.x = 0.00999999978<u_xlat69;
    if(u_xlatb9.x){
        u_xlat9.xy = u_xlat8.xy * u_xlat13.xy;
        u_xlat68 = u_xlat68 * u_xlat8.y + u_xlat9.x;
        u_xlat9.x = u_xlat8.y * u_xlat14.y;
        u_xlat12.x = u_xlat14.x * u_xlat8.z + u_xlat68;
        u_xlat45.x = u_xlat45.x * u_xlat8.x + u_xlat9.y;
        u_xlat12.y = u_xlat32.x * u_xlat8.z + u_xlat45.x;
        u_xlat45.x = u_xlat32.z * u_xlat8.x + u_xlat9.x;
        u_xlat12.z = u_xlat13.z * u_xlat8.z + u_xlat45.x;
        u_xlatb45 = 0.0<unity_SpecCube0_ProbePosition.w;
        u_xlatb9.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat12.xyzx).xyz;
        u_xlat9.x = (u_xlatb9.x) ? unity_SpecCube0_BoxMax.x : unity_SpecCube0_BoxMin.x;
        u_xlat9.y = (u_xlatb9.y) ? unity_SpecCube0_BoxMax.y : unity_SpecCube0_BoxMin.y;
        u_xlat9.z = (u_xlatb9.z) ? unity_SpecCube0_BoxMax.z : unity_SpecCube0_BoxMin.z;
        u_xlat9.xyz = (-u_xlat10.xyz) + u_xlat9.xyz;
        u_xlat9.xyz = u_xlat9.xyz / u_xlat12.xyz;
        u_xlat68 = min(u_xlat9.y, u_xlat9.x);
        u_xlat68 = min(u_xlat9.z, u_xlat68);
        u_xlat9.xyz = u_xlat10.xyz + (-unity_SpecCube0_ProbePosition.xyz);
        u_xlat9.xyz = u_xlat12.xyz * float3(u_xlat68) + u_xlat9.xyz;
        u_xlat9.xyz = (bool(u_xlatb45)) ? u_xlat9.xyz : u_xlat12.xyz;
        u_xlat10.xyz = unity_SpecCube0_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat12.xyz = u_xlat10.yzx * (-unity_SpecCube0_Rotation.xyz);
        u_xlat13.xyz = u_xlat10.xyz * unity_SpecCube0_Rotation.www;
        u_xlat12.xyz = u_xlat12.zzy + u_xlat12.yxx;
        u_xlat12.xyz = (-u_xlat12.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat31.xz = u_xlat9.xy * u_xlat12.xy;
        u_xlat45.x = (-unity_SpecCube0_Rotation.x) * u_xlat10.z + (-u_xlat13.x);
        u_xlat45.x = u_xlat45.x * u_xlat9.y + u_xlat31.x;
        u_xlat32.xz = (-unity_SpecCube0_Rotation.xy) * u_xlat10.xx + u_xlat13.zy;
        u_xlat68 = u_xlat9.y * u_xlat32.z;
        u_xlat16.x = u_xlat32.x * u_xlat9.z + u_xlat45.x;
        u_xlat45.x = (-unity_SpecCube0_Rotation.x) * u_xlat10.z + u_xlat13.x;
        u_xlat45.x = u_xlat45.x * u_xlat9.x + u_xlat31.z;
        u_xlat30.xz = (-unity_SpecCube0_Rotation.yx) * u_xlat10.xx + (-u_xlat13.yz);
        u_xlat16.y = u_xlat30.x * u_xlat9.z + u_xlat45.x;
        u_xlat45.x = u_xlat30.z * u_xlat9.x + u_xlat68;
        u_xlat16.z = u_xlat12.z * u_xlat9.z + u_xlat45.x;
        u_xlat9 = _g_textureLod(_Texture_t1, u_xlat16.xyz, u_xlat64);
        u_xlat45.x = u_xlat9.w + -1.0;
        u_xlat45.x = unity_SpecCube0_HDR.w * u_xlat45.x + 1.0;
        u_xlat45.x = max(u_xlat45.x, 0.0);
        u_xlat45.x = log2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * unity_SpecCube0_HDR.y;
        u_xlat45.x = exp2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * unity_SpecCube0_HDR.x;
        u_xlat9.xyz = u_xlat9.xyz * u_xlat45.xxx;
        u_xlat9.xyz = float3(u_xlat69) * u_xlat9.xyz;
    } else {
        u_xlat9.x = float(0.0);
        u_xlat9.y = float(0.0);
        u_xlat9.z = float(0.0);
    }
    u_xlatb45 = 0.00999999978<u_xlat70;
    if(u_xlatb45){
        u_xlat10.xy = u_xlat8.xy * u_xlat18.xy;
        u_xlat45.x = u_xlat67 * u_xlat8.y + u_xlat10.x;
        u_xlat67 = u_xlat8.y * u_xlat56.y;
        u_xlat12.x = u_xlat56.x * u_xlat8.z + u_xlat45.x;
        u_xlat45.x = u_xlat46.x * u_xlat8.x + u_xlat10.y;
        u_xlat12.y = u_xlat11.x * u_xlat8.z + u_xlat45.x;
        u_xlat45.x = u_xlat11.z * u_xlat8.x + u_xlat67;
        u_xlat12.z = u_xlat18.z * u_xlat8.z + u_xlat45.x;
        u_xlatb45 = 0.0<unity_SpecCube1_ProbePosition.w;
        u_xlatb10.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat12.xyzx).xyz;
        u_xlat10.x = (u_xlatb10.x) ? unity_SpecCube1_BoxMax.x : unity_SpecCube1_BoxMin.x;
        u_xlat10.y = (u_xlatb10.y) ? unity_SpecCube1_BoxMax.y : unity_SpecCube1_BoxMin.y;
        u_xlat10.z = (u_xlatb10.z) ? unity_SpecCube1_BoxMax.z : unity_SpecCube1_BoxMin.z;
        u_xlat10.xyz = (-u_xlat15.xyz) + u_xlat10.xyz;
        u_xlat10.xyz = u_xlat10.xyz / u_xlat12.xyz;
        u_xlat46.x = min(u_xlat10.y, u_xlat10.x);
        u_xlat46.x = min(u_xlat10.z, u_xlat46.x);
        u_xlat10.xyz = u_xlat15.xyz + (-unity_SpecCube1_ProbePosition.xyz);
        u_xlat10.xyz = u_xlat12.xyz * u_xlat46.xxx + u_xlat10.xyz;
        u_xlat10.xyz = (bool(u_xlatb45)) ? u_xlat10.xyz : u_xlat12.xyz;
        u_xlat11.xyz = unity_SpecCube1_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat12.xyz = u_xlat11.yzx * (-unity_SpecCube1_Rotation.xyz);
        u_xlat13.xyz = u_xlat11.xyz * unity_SpecCube1_Rotation.www;
        u_xlat12.xyz = u_xlat12.zzy + u_xlat12.yxx;
        u_xlat12.xyz = (-u_xlat12.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat46.xy = u_xlat10.xy * u_xlat12.xy;
        u_xlat45.x = (-unity_SpecCube1_Rotation.x) * u_xlat11.z + (-u_xlat13.x);
        u_xlat45.x = u_xlat45.x * u_xlat10.y + u_xlat46.x;
        u_xlat32.xz = (-unity_SpecCube1_Rotation.xy) * u_xlat11.xx + u_xlat13.zy;
        u_xlat46.x = u_xlat10.y * u_xlat32.z;
        u_xlat14.x = u_xlat32.x * u_xlat10.z + u_xlat45.x;
        u_xlat45.x = (-unity_SpecCube1_Rotation.x) * u_xlat11.z + u_xlat13.x;
        u_xlat45.x = u_xlat45.x * u_xlat10.x + u_xlat46.y;
        u_xlat31.xz = (-unity_SpecCube1_Rotation.yx) * u_xlat11.xx + (-u_xlat13.yz);
        u_xlat14.y = u_xlat31.x * u_xlat10.z + u_xlat45.x;
        u_xlat45.x = u_xlat31.z * u_xlat10.x + u_xlat46.x;
        u_xlat14.z = u_xlat12.z * u_xlat10.z + u_xlat45.x;
        u_xlat10 = _g_textureLod(_Texture_t2, u_xlat14.xyz, u_xlat64);
        u_xlat45.x = u_xlat10.w + -1.0;
        u_xlat45.x = unity_SpecCube1_HDR.w * u_xlat45.x + 1.0;
        u_xlat45.x = max(u_xlat45.x, 0.0);
        u_xlat45.x = log2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * unity_SpecCube1_HDR.y;
        u_xlat45.x = exp2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * unity_SpecCube1_HDR.x;
        u_xlat10.xyz = u_xlat10.xyz * u_xlat45.xxx;
        u_xlat9.xyz = float3(u_xlat70) * u_xlat10.xyz + u_xlat9.xyz;
    }
    u_xlatb45 = u_xlat71<0.99000001;
    if(u_xlatb45){
        u_xlat10 = _g_textureLod(_Texture_t0, u_xlat8.xyz, u_xlat64);
        u_xlat64 = (-u_xlat71) + 1.0;
        u_xlat45.x = u_xlat10.w + -1.0;
        u_xlat45.x = _GlossyEnvironmentCubeMap_HDR.w * u_xlat45.x + 1.0;
        u_xlat45.x = max(u_xlat45.x, 0.0);
        u_xlat45.x = log2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * _GlossyEnvironmentCubeMap_HDR.y;
        u_xlat45.x = exp2(u_xlat45.x);
        u_xlat45.x = u_xlat45.x * _GlossyEnvironmentCubeMap_HDR.x;
        u_xlat8.xyz = u_xlat10.xyz * u_xlat45.xxx;
        u_xlat9.xyz = float3(u_xlat64) * u_xlat8.xyz + u_xlat9.xyz;
    }
    u_xlat45.xy = float2(u_xlat66) * float2(u_xlat66) + float2(-1.0, 1.0);
    u_xlat64 = float(1.0) / u_xlat45.y;
    u_xlat8.xyz = (-u_xlat2.xyz) + float3(u_xlat65);
    u_xlat8.xyz = u_xlat24.xxx * u_xlat8.xyz + u_xlat2.xyz;
    u_xlat8.xyz = float3(u_xlat64) * u_xlat8.xyz;
    u_xlat8.xyz = u_xlat8.xyz * u_xlat9.xyz;
    u_xlat5.xyz = u_xlat5.xyz * u_xlat6.xyz + u_xlat8.xyz;
    u_xlati64 = int(uint(uint(_g_floatBitsToUint(_MainLightLayerMask)) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
    u_xlat65 = u_xlat3.x * unity_LightData.z;
    u_xlat3.x = dot(u_xlat0.xyz, _MainLightPosition.xyz);
    u_xlat3.x = clamp(u_xlat3.x, 0.0, 1.0);
    u_xlat65 = u_xlat65 * u_xlat3.x;
    u_xlat3.xyw = float3(u_xlat65) * u_xlat7.xyz;
    u_xlat7.xyz = u_xlat1.xyz + _MainLightPosition.xyz;
    u_xlat65 = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat65 = max(u_xlat65, 1.17549435e-38);
    u_xlat65 = _g_inversesqrt(u_xlat65);
    u_xlat7.xyz = float3(u_xlat65) * u_xlat7.xyz;
    u_xlat65 = dot(u_xlat0.xyz, u_xlat7.xyz);
    u_xlat65 = clamp(u_xlat65, 0.0, 1.0);
    u_xlat46.x = dot(_MainLightPosition.xyz, u_xlat7.xyz);
    u_xlat46.x = clamp(u_xlat46.x, 0.0, 1.0);
    u_xlat65 = u_xlat65 * u_xlat65;
    u_xlat65 = u_xlat65 * u_xlat45.x + 1.00001001;
    u_xlat46.x = u_xlat46.x * u_xlat46.x;
    u_xlat65 = u_xlat65 * u_xlat65;
    u_xlat46.x = max(u_xlat46.x, 0.100000001);
    u_xlat65 = u_xlat65 * u_xlat46.x;
    u_xlat65 = u_xlat25 * u_xlat65;
    u_xlat65 = u_xlat4.x / u_xlat65;
    u_xlat7.xyz = u_xlat2.xyz * float3(u_xlat65) + u_xlat6.xyz;
    u_xlat3.xyw = u_xlat3.xyw * u_xlat7.xyz;
    u_xlat3.xyw = (int(u_xlati64) != 0) ? u_xlat3.xyw : float3(0.0, 0.0, 0.0);
    u_xlat64 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu64 =  uint(int(u_xlat64));
    u_xlatb46.xy = _g_equal(_pad176.zzzz, float4(0.0, 1.0, 0.0, 1.0)).xy;
    u_xlat7.x = float(0.0);
    u_xlat7.y = float(0.0);
    u_xlat7.z = float(0.0);
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<u_xlatu64 ; u_xlatu_loop_1++)
    {
        u_xlatu68 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati69 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat68 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati69]);
        u_xlatu68 =  uint(int(u_xlat68));
        u_xlat8.xyz = (-input.vs_INTERP11.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat69 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat69 = max(u_xlat69, 6.10351562e-05);
        u_xlat70 = _g_inversesqrt(u_xlat69);
        u_xlat9.xyz = float3(u_xlat70) * u_xlat8.xyz;
        u_xlat71 = float(1.0) / float(u_xlat69);
        u_xlat69 = u_xlat69 * _AdditionalLightsAttenuation.x;
        u_xlat69 = (-u_xlat69) * u_xlat69 + 1.0;
        u_xlat69 = max(u_xlat69, 0.0);
        u_xlat69 = u_xlat69 * u_xlat69;
        u_xlat69 = u_xlat69 * u_xlat71;
        u_xlat71 = dot(_AdditionalLightsSpotDir.xyz, u_xlat9.xyz);
        u_xlat71 = u_xlat71 * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat71 = clamp(u_xlat71, 0.0, 1.0);
        u_xlat71 = u_xlat71 * u_xlat71;
        u_xlat69 = u_xlat69 * u_xlat71;
        u_xlatu71 = uint(u_xlatu68 >> 5u);
        u_xlati72 = int(1 << int(u_xlatu68));
        u_xlati71 = int(uint(uint(u_xlati72) & uint(_g_floatBitsToUint(_pad64.x))));
        if(u_xlati71 != 0) {
            u_xlati71 = int(_pad20672.x);
            u_xlati72 = (u_xlati71 != 0) ? 0 : 1;
            u_xlati10 = int(int(u_xlatu68) << 2);
            if(u_xlati72 != 0) {
                u_xlat31.xyz = input.vs_INTERP11.yyy * _pad208.xyw;
                u_xlat31.xyz = _pad192.xyw * input.vs_INTERP11.xxx + u_xlat31.xyz;
                u_xlat31.xyz = _pad224.xyw * input.vs_INTERP11.zzz + u_xlat31.xyz;
                u_xlat31.xyz = u_xlat31.xyz + _pad240.xyw;
                u_xlat31.xy = u_xlat31.xy / u_xlat31.zz;
                u_xlat31.xy = u_xlat31.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                u_xlat31.xy = clamp(u_xlat31.xy, 0.0, 1.0);
                u_xlat31.xy = _pad16576.xy * u_xlat31.xy + _pad16576.zw;
            } else {
                u_xlatb71 = u_xlati71==1;
                u_xlati71 = u_xlatb71 ? 1 : int(0);
                if(u_xlati71 != 0) {
                    u_xlat11.xy = input.vs_INTERP11.yy * _pad208.xy;
                    u_xlat11.xy = _pad192.xy * input.vs_INTERP11.xx + u_xlat11.xy;
                    u_xlat11.xy = _pad224.xy * input.vs_INTERP11.zz + u_xlat11.xy;
                    u_xlat11.xy = u_xlat11.xy + _pad240.xy;
                    u_xlat11.xy = u_xlat11.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat11.xy = _g_fract(u_xlat11.xy);
                    u_xlat31.xy = _pad16576.xy * u_xlat11.xy + _pad16576.zw;
                } else {
                    u_xlat11 = input.vs_INTERP11.yyyy * _pad208;
                    u_xlat11 = _pad192 * input.vs_INTERP11.xxxx + u_xlat11;
                    u_xlat11 = _pad224 * input.vs_INTERP11.zzzz + u_xlat11;
                    u_xlat11 = u_xlat11 + _pad240;
                    u_xlat11.xyz = u_xlat11.xyz / u_xlat11.www;
                    u_xlat71 = dot(u_xlat11.xyz, u_xlat11.xyz);
                    u_xlat71 = _g_inversesqrt(u_xlat71);
                    u_xlat11.xyz = float3(u_xlat71) * u_xlat11.xyz;
                    u_xlat71 = dot(abs(u_xlat11.xyz), float3(1.0, 1.0, 1.0));
                    u_xlat71 = max(u_xlat71, 9.99999997e-07);
                    u_xlat71 = float(1.0) / float(u_xlat71);
                    u_xlat12.xyz = float3(u_xlat71) * u_xlat11.zxy;
                    u_xlat12.x = (-u_xlat12.x);
                    u_xlat12.x = clamp(u_xlat12.x, 0.0, 1.0);
                    u_xlatb10.xw = _g_greaterThanEqual(u_xlat12.yyyz, float4(0.0, 0.0, 0.0, 0.0)).xw;
                    u_xlat10.x = (u_xlatb10.x) ? u_xlat12.x : (-u_xlat12.x);
                    u_xlat10.w = (u_xlatb10.w) ? u_xlat12.x : (-u_xlat12.x);
                    u_xlat10.xw = u_xlat11.xy * float2(u_xlat71) + u_xlat10.xw;
                    u_xlat10.xw = u_xlat10.xw * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat10.xw = clamp(u_xlat10.xw, 0.0, 1.0);
                    u_xlat31.xy = _pad16576.xy * u_xlat10.xw + _pad16576.zw;
                }
            }
            u_xlat10 = _g_textureLod(_Texture_t7, u_xlat31.xy, 0.0);
            u_xlat71 = (u_xlatb46.y) ? u_xlat10.w : u_xlat10.x;
            u_xlat10.xyz = (u_xlatb46.x) ? u_xlat10.xyz : float3(u_xlat71);
        } else {
            u_xlat10.x = float(1.0);
            u_xlat10.y = float(1.0);
            u_xlat10.z = float(1.0);
        }
        u_xlat10.xyz = u_xlat10.xyz * _AdditionalLightsColor.xyz;
        u_xlati68 = int(uint(uint(_g_floatBitsToUint(unity_RenderingLayer.x)) & uint(_g_floatBitsToUint(_AdditionalLightsLayerMasks))));
        u_xlat71 = dot(u_xlat0.xyz, u_xlat9.xyz);
        u_xlat71 = clamp(u_xlat71, 0.0, 1.0);
        u_xlat69 = u_xlat69 * u_xlat71;
        u_xlat10.xyz = float3(u_xlat69) * u_xlat10.xyz;
        u_xlat8.xyz = u_xlat8.xyz * float3(u_xlat70) + u_xlat1.xyz;
        u_xlat69 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat69 = max(u_xlat69, 1.17549435e-38);
        u_xlat69 = _g_inversesqrt(u_xlat69);
        u_xlat8.xyz = float3(u_xlat69) * u_xlat8.xyz;
        u_xlat69 = dot(u_xlat0.xyz, u_xlat8.xyz);
        u_xlat69 = clamp(u_xlat69, 0.0, 1.0);
        u_xlat70 = dot(u_xlat9.xyz, u_xlat8.xyz);
        u_xlat70 = clamp(u_xlat70, 0.0, 1.0);
        u_xlat69 = u_xlat69 * u_xlat69;
        u_xlat69 = u_xlat69 * u_xlat45.x + 1.00001001;
        u_xlat70 = u_xlat70 * u_xlat70;
        u_xlat69 = u_xlat69 * u_xlat69;
        u_xlat70 = max(u_xlat70, 0.100000001);
        u_xlat69 = u_xlat69 * u_xlat70;
        u_xlat69 = u_xlat25 * u_xlat69;
        u_xlat69 = u_xlat4.x / u_xlat69;
        u_xlat8.xyz = u_xlat2.xyz * float3(u_xlat69) + u_xlat6.xyz;
        u_xlat8.xyz = u_xlat8.xyz * u_xlat10.xyz + u_xlat7.xyz;
        u_xlat7.xyz = (int(u_xlati68) != 0) ? u_xlat8.xyz : u_xlat7.xyz;
    }
    u_xlat0.xyz = u_xlat3.xyw + u_xlat5.xyz;
    u_xlat0.xyz = u_xlat7.xyz + u_xlat0.xyz;
    u_xlat63 = u_xlat63 * (-u_xlat63);
    u_xlat63 = exp2(u_xlat63);
    u_xlat1.x = (-u_xlat63) + 1.0;
    u_xlat1.xyz = u_xlat1.xxx * _pad992.xyz;
    __SV_Target0.xyz = u_xlat0.xyz * float3(u_xlat63) + u_xlat1.xyz;
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
