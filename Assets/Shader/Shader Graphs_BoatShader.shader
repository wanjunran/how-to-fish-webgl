Shader "Shader Graphs/BoatShader"
{
    Properties
    {




[NoScaleOffset] _Colors ("Colors", 2D) = "white" {}
_Emission ("Emission", Float) = 0
[NoScaleOffset] _Normal_Map ("Normal Map", 2D) = "white" {}
_Normal_Strength ("Normal Strength", Range(0, 1)) = 1
_Normal_Scale ("Normal Scale", Float) = 0.2
[ToggleUI] _Use_Skin ("Use Skin", Float) = 0
[ToggleUI] _Rainbow_Skin ("Rainbow Skin", Float) = 0
[KeywordEnum(Gradient Noise, Checkerboard, Voronoi)] _SKIN_TYPE ("Skin Type", Float) = 0
[KeywordEnum(Only Metallic, Only Metallic Noise, Everything)] _SKIN_AFFECTS ("Skin Affects", Float) = 0
_Metallic_Cutoff ("Metallic Cutoff", Range(0, 1)) = 0.5
_Color_Offset ("Color Offset", Vector) = (0,0,0,0)
_Color_Offset_2 ("Color Offset 2", Vector) = (0,0,0,0)
_Skin_Smooth_Step ("Skin Smooth Step", Vector) = (0.5,0.5,0,0)
_Skin_Noise_Scale ("Skin Noise Scale", Float) = 10
_Noise_Rotation ("Noise Rotation", Float) = 45
_UV_Rotation ("UV Rotation", Float) = 0
_Skin_UV_Scale ("Skin UV Scale", Vector) = (1,1,0,0)
_MetallicMetallicness ("MetallicMetallicness", Range(0, 1)) = 0
_MetallicSmoothness ("MetallicSmoothness", Range(0, 1)) = 0
_PlasticMetallicness ("PlasticMetallicness", Range(0, 1)) = 0
_PlasticSmoothness ("PlasticSmoothness", Range(0, 1)) = 0
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
            ZWrite On
            Cull Back
            // RenderType: Opaque
            ZWrite On Cull Back

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
            float4 _pad80;
            float4 _pad96;
            float4 _pad128;
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
            float4 _pad432;
            float4 _pad448;
            float4 _pad976;
            float4 _pad992;
            float4x4 unity_MatrixVP;
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
            float _Emission;
            float _Normal_Strength;
            float _Normal_Scale;
            float2 _Color_Offset;
            float _Skin_Noise_Scale;
            float2 _Skin_UV_Scale;
            float2 _Color_Offset_2;
            float2 _Skin_Smooth_Step;
            float _Use_Skin;
            float _Rainbow_Skin;
            float _PlasticMetallicness;
            float _PlasticSmoothness;
            float _Metallic_Cutoff;
            float _MetallicMetallicness;
            float _MetallicSmoothness;
            float _UV_Rotation;

            float4 u_xlat0;
            int u_xlati0;
            float4 u_xlat1;
            int u_xlati2;
            float4 ImmCB_0_0_0[4];
            float3 u_xlat2;
            float4 u_xlat3;
            bool2 u_xlatb3;
            float4 u_xlat4;
            bool4 u_xlatb4;
            float4 u_xlat5;
            float4 u_xlat6;
            uint2 u_xlatu6;
            float4 u_xlat7;
            bool2 u_xlatb7;
            float4 u_xlat8;
            float4 u_xlat9;
            bool u_xlatb9;
            float4 u_xlat10;
            float4 u_xlat11;
            int u_xlati11;
            bool4 u_xlatb11;
            float4 u_xlat12;
            float4 u_xlat13;
            float4 u_xlat14;
            float4 u_xlat15;
            float4 u_xlat16;
            float4 u_xlat17;
            float3 u_xlat18;
            float4 u_xlat19;
            float4 u_xlat20;
            float4 u_xlat21;
            float3 u_xlat22;
            float3 u_xlat23;
            float3 u_xlat25;
            float2 u_xlat26;
            bool2 u_xlatb26;
            float2 u_xlat29;
            bool u_xlatb29;
            float3 u_xlat30;
            float2 u_xlat31;
            bool u_xlatb31;
            float3 u_xlat32;
            float3 u_xlat33;
            float3 u_xlat34;
            float3 u_xlat38;
            float2 u_xlat44;
            float2 u_xlat47;
            float2 u_xlat48;
            bool u_xlatb48;
            float2 u_xlat49;
            int2 u_xlati49;
            uint2 u_xlatu49;
            bool u_xlatb49;
            float2 u_xlat51;
            int u_xlati51;
            float2 u_xlat52;
            float u_xlat53;
            bool u_xlatb53;
            float2 u_xlat54;
            float2 u_xlat55;
            float2 u_xlat58;
            float u_xlat67;
            int u_xlati67;
            uint u_xlatu67;
            bool u_xlatb67;
            float u_xlat68;
            int u_xlati68;
            uint u_xlatu68;
            bool u_xlatb68;
            float u_xlat69;
            int u_xlati69;
            uint u_xlatu69;
            float u_xlat70;
            float u_xlat71;
            int u_xlati71;
            uint u_xlatu71;
            bool u_xlatb71;
            float u_xlat72;
            float u_xlat73;
            float u_xlat74;
            float u_xlat75;
            int u_xlati75;
            uint u_xlatu75;
            bool u_xlatb75;
            float u_xlat76;
            int u_xlati76;
            bool u_xlatb76;
            uint _g_gl_InstanceID : SV_InstanceID;



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
                nointerpolation uint vs_CUSTOM_INSTANCE_ID0 : TEXCOORD0;
                float2 vs_INTERP0 : TEXCOORD1;
                float4 vs_INTERP10 : TEXCOORD2;
                float3 vs_INTERP11 : TEXCOORD3;
                float3 vs_INTERP12 : TEXCOORD4;
                float4 vs_INTERP5 : TEXCOORD5;
                float4 vs_INTERP6 : TEXCOORD6;
                float4 vs_INTERP7 : TEXCOORD7;
                float4 vs_INTERP8 : TEXCOORD8;
                float4 vs_INTERP9 : TEXCOORD9;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);


    u_xlati0 = _g_gl_InstanceID + _g_floatBitsToInt(_pad0.x);
    u_xlati2 = u_xlati0 * 9;
    u_xlati0 = int(u_xlati0 << 1);
    output.vs_INTERP0.xy = input.in_TEXCOORD1.xy * _pad0.xy + _pad0.zw;
    u_xlat0.xzw = input.in_POSITION0.yyy * _pad16.xyz;
    u_xlat0.xzw = _pad0.xyz * input.in_POSITION0.xxx + u_xlat0.xzw;
    u_xlat0.xzw = _pad32.xyz * input.in_POSITION0.zzz + u_xlat0.xzw;
    u_xlat0.xzw = u_xlat0.xzw + _pad48.xyz;
    u_xlat1 = u_xlat0.zzzz * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.wwww + u_xlat1;
    output.vs_INTERP11.xyz = u_xlat0.xzw;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    u_xlat0.xzw = input.in_TANGENT0.yyy * _pad16.xyz;
    u_xlat0.xzw = _pad0.xyz * input.in_TANGENT0.xxx + u_xlat0.xzw;
    u_xlat0.xzw = _pad32.xyz * input.in_TANGENT0.zzz + u_xlat0.xzw;
    u_xlat1.x = dot(u_xlat0.xzw, u_xlat0.xzw);
    u_xlat1.x = max(u_xlat1.x, 1.17549435e-38);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    output.vs_INTERP5.xyz = u_xlat0.xzw * u_xlat1.xxx;
    output.vs_INTERP5.w = input.in_TANGENT0.w;
    output.vs_INTERP6 = input.in_TEXCOORD0;
    output.vs_INTERP7 = input.in_TEXCOORD1;
    output.vs_INTERP8 = input.in_TEXCOORD2;
    output.vs_INTERP9 = input.in_TEXCOORD3;
    output.vs_INTERP10 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat1.x = dot(input.in_NORMAL0.xyz, _pad64.xyz);
    u_xlat1.y = dot(input.in_NORMAL0.xyz, _pad80.xyz);
    u_xlat1.z = dot(input.in_NORMAL0.xyz, _pad96.xyz);
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    output.vs_INTERP12.xyz = u_xlat0.xxx * u_xlat1.xyz;
    output.vs_CUSTOM_INSTANCE_ID0 =  uint(_g_gl_InstanceID);
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_0_0[3] = float4(0.0, 0.0, 0.0, 1.0);
    u_xlati0 = int(input.vs_CUSTOM_INSTANCE_ID0) + _g_floatBitsToInt(_pad0.x);
    u_xlat22.x = dot(input.vs_INTERP12.xyz, input.vs_INTERP12.xyz);
    u_xlat22.x = sqrt(u_xlat22.x);
    u_xlat22.x = float(1.0) / u_xlat22.x;
    u_xlat1.xyz = u_xlat22.xxx * input.vs_INTERP12.xyz;
    u_xlat44.xy = input.vs_INTERP6.xy + float2(_Color_Offset.x, _Color_Offset.y);
    u_xlat2.xyz = _g_texture(_Texture_t8, u_xlat44.xy, _GlobalMipBias.x).xyz;
    u_xlat44.xy = u_xlat44.xy + _Color_Offset_2.xy;
    u_xlat67 = _UV_Rotation * 0.0174532924;
    u_xlat3.xy = input.vs_INTERP9.xy + float2(-0.5, -0.5);
    u_xlat4.x = sin(u_xlat67);
    u_xlat5.x = cos(u_xlat67);
    u_xlat6.x = (-u_xlat4.x);
    u_xlat6.y = u_xlat5.x;
    u_xlat5.y = dot(u_xlat3.xy, u_xlat6.xy);
    u_xlat6.z = u_xlat4.x;
    u_xlat5.x = dot(u_xlat3.xy, u_xlat6.yz);
    u_xlat3.xy = u_xlat5.xy + float2(0.5, 0.5);
    u_xlat3.xy = u_xlat3.xy * float2(_Skin_UV_Scale.x, _Skin_UV_Scale.y);
    u_xlat3.xy = u_xlat3.xy * float2(_Skin_Noise_Scale);
    u_xlat47.xy = floor(u_xlat3.xy);
    u_xlat3.xy = _g_fract(u_xlat3.xy);
    u_xlat4.x = float(0.0);
    u_xlat4.y = float(8.0);
    for(int u_xlati_loop_1 = int(0xFFFFFFFFu) ; u_xlati_loop_1<=1 ; u_xlati_loop_1++)
    {
        u_xlat5.y = float(u_xlati_loop_1);
        u_xlat48.xy = u_xlat4.xy;
        for(int u_xlati_loop_2 = int(0xFFFFFFFFu) ; u_xlati_loop_2<=1 ; u_xlati_loop_2++)
        {
            u_xlat5.x = float(u_xlati_loop_2);
            u_xlat49.xy = u_xlat47.xy + u_xlat5.xy;
            u_xlati49.xy = int2(u_xlat49.xy);
            u_xlati71 = int(uint(uint(u_xlati49.y) ^ 1103515245u));
            u_xlati49.x = u_xlati71 + u_xlati49.x;
            u_xlatu49.x = uint(u_xlati71) * uint(u_xlati49.x);
            u_xlatu6.x = uint(u_xlatu49.x >> 5u);
            u_xlati49.x = int(uint(u_xlatu49.x ^ u_xlatu6.x));
            u_xlatu6.y = uint(u_xlati49.x) * 668265261u;
            u_xlati49.x = int(int(u_xlatu6.y) << 3);
            u_xlatu6.x = uint(uint(u_xlati49.x) ^ uint(u_xlati71));
            u_xlatu49.xy = uint2(u_xlatu6.x >> uint(8u), u_xlatu6.y >> uint(8u));
            u_xlat49.xy = float2(u_xlatu49.xy);
            u_xlat49.xy = u_xlat49.xy * float2(1.19209304e-07, 1.19209304e-07);
            u_xlat6.x = sin(u_xlat49.x);
            u_xlat6.y = cos(u_xlat49.y);
            u_xlat5.xz = u_xlat6.xy * float2(0.5, 0.5) + u_xlat5.xy;
            u_xlat5.xz = (-u_xlat3.xy) + u_xlat5.xz;
            u_xlat5.xz = u_xlat5.xz + float2(0.5, 0.5);
            u_xlat5.x = dot(u_xlat5.xz, u_xlat5.xz);
            u_xlat5.x = sqrt(u_xlat5.x);
            u_xlatb49 = u_xlat5.x<u_xlat48.y;
            u_xlat48.xy = (bool(u_xlatb49)) ? u_xlat5.xx : u_xlat48.xy;
        }
        u_xlat4.xy = u_xlat48.xy;
    }
    u_xlat67 = (-_Skin_Smooth_Step.xxxy.z) + _Skin_Smooth_Step.xxxy.w;
    u_xlat68 = u_xlat4.x + (-_Skin_Smooth_Step.xxxy.z);
    u_xlat67 = float(1.0) / u_xlat67;
    u_xlat67 = u_xlat67 * u_xlat68;
    u_xlat67 = clamp(u_xlat67, 0.0, 1.0);
    u_xlat68 = u_xlat67 * -2.0 + 3.0;
    u_xlat67 = u_xlat67 * u_xlat67;
    u_xlat67 = u_xlat67 * u_xlat68;
    u_xlatb3.xy = _g_notEqual(float4(0.0, 0.0, 0.0, 0.0), float4(_Rainbow_Skin, _Use_Skin, _Rainbow_Skin, _Rainbow_Skin)).xy;
    u_xlat68 = (u_xlatb3.x) ? u_xlat67 : 1.0;
    u_xlat44.xy = u_xlat44.xy * float2(u_xlat68);
    u_xlat3.xzw = _g_texture(_Texture_t8, u_xlat44.xy, _GlobalMipBias.x).xyz;
    u_xlat44.x = u_xlatb3.y ? 1.0 : float(0.0);
    u_xlat44.x = u_xlat44.x * u_xlat67;
    u_xlat3.xyz = (-u_xlat2.xyz) + u_xlat3.xzw;
    u_xlat2.xyz = u_xlat44.xxx * u_xlat3.xyz + u_xlat2.xyz;
    u_xlat44.xy = input.vs_INTERP9.xy * float2(float2(_Normal_Scale, _Normal_Scale));
    u_xlat3.xyw = _g_texture(_Texture_t9, u_xlat44.xy, _GlobalMipBias.x).xyw;
    u_xlat3.x = u_xlat3.x * u_xlat3.w;
    u_xlat3.xy = u_xlat3.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat44.x = dot(u_xlat3.xy, u_xlat3.xy);
    u_xlat44.x = min(u_xlat44.x, 1.0);
    u_xlat44.x = (-u_xlat44.x) + 1.0;
    u_xlat44.x = sqrt(u_xlat44.x);
    u_xlat3.z = max(u_xlat44.x, 1.00000002e-16);
    u_xlat44.x = dot(u_xlat3, u_xlat3);
    u_xlat44.x = _g_inversesqrt(u_xlat44.x);
    u_xlat3.xyz = u_xlat44.xxx * u_xlat3.xyz;
    u_xlat4.xy = u_xlat22.xx * input.vs_INTERP12.xy + u_xlat3.xy;
    u_xlat4.z = u_xlat1.z * u_xlat3.z;
    u_xlat22.x = dot(u_xlat4.xyz, u_xlat4.xyz);
    u_xlat22.x = max(u_xlat22.x, 1.17549435e-38);
    u_xlat22.x = _g_inversesqrt(u_xlat22.x);
    u_xlat22.xyz = u_xlat4.xyz * u_xlat22.xxx + (-u_xlat1.xyz);
    u_xlat22.xyz = float3(_Normal_Strength) * u_xlat22.xyz + u_xlat1.xyz;
    u_xlat1.x = dot(u_xlat22.xyz, u_xlat22.xyz);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    u_xlat22.xyz = u_xlat22.xyz * u_xlat1.xxx;
    u_xlat1.xyz = _g_texture(_Texture_t8, input.vs_INTERP8.xy, _GlobalMipBias.x).xyz;
    u_xlatb67 = input.vs_INTERP7.x>=_Metallic_Cutoff;
    u_xlat67 = (u_xlatb67) ? _MetallicMetallicness : _PlasticMetallicness;
    u_xlatb68 = 0.00999999978<u_xlat67;
    u_xlat67 = (u_xlatb68) ? u_xlat67 : input.vs_INTERP7.x;
    u_xlat67 = clamp(u_xlat67, 0.0, 1.0);
    u_xlatb68 = input.vs_INTERP7.x>=0.899999976;
    u_xlat68 = (u_xlatb68) ? _MetallicSmoothness : _PlasticSmoothness;
    u_xlatb3.x = 0.00999999978<u_xlat68;
    u_xlat25.x = max(input.vs_INTERP7.y, 0.0);
    u_xlat25.x = min(u_xlat25.x, 0.800000012);
    u_xlat68 = (u_xlatb3.x) ? u_xlat68 : u_xlat25.x;
    u_xlat68 = clamp(u_xlat68, 0.0, 1.0);
    u_xlatb3.x = unity_OrthoParams.w==0.0;
    u_xlat25.xyz = (-input.vs_INTERP11.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat4.x = dot(u_xlat25.xyz, u_xlat25.xyz);
    u_xlat4.x = _g_inversesqrt(u_xlat4.x);
    u_xlat25.xyz = u_xlat25.xyz * u_xlat4.xxx;
    u_xlat4.x = _tunity_MatrixV[0].z;
    u_xlat4.y = _tunity_MatrixV[1].z;
    u_xlat4.z = _tunity_MatrixV[2].z;
    u_xlat3.xyz = (u_xlatb3.x) ? u_xlat25.xyz : u_xlat4.xyz;
    u_xlat4.xyz = input.vs_INTERP11.xyz + (-_pad320.xyz);
    u_xlat5.xyz = input.vs_INTERP11.xyz + (-_pad336.xyz);
    u_xlat6.xyz = input.vs_INTERP11.xyz + (-_pad352.xyz);
    u_xlat7.xyz = input.vs_INTERP11.xyz + (-_pad368.xyz);
    u_xlat4.x = dot(u_xlat4.xyz, u_xlat4.xyz);
    u_xlat4.y = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlat4.z = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat4.w = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlatb4 = _g_lessThan(u_xlat4, _pad384);
    u_xlat5.x = u_xlatb4.x ? float(1.0) : 0.0;
    u_xlat5.y = u_xlatb4.y ? float(1.0) : 0.0;
    u_xlat5.z = u_xlatb4.z ? float(1.0) : 0.0;
    u_xlat5.w = u_xlatb4.w ? float(1.0) : 0.0;
;
    u_xlat4.x = (u_xlatb4.x) ? float(-1.0) : float(-0.0);
    u_xlat4.y = (u_xlatb4.y) ? float(-1.0) : float(-0.0);
    u_xlat4.z = (u_xlatb4.z) ? float(-1.0) : float(-0.0);
    u_xlat4.xyz = u_xlat4.xyz + u_xlat5.yzw;
    u_xlat5.yzw = max(u_xlat4.xyz, float3(0.0, 0.0, 0.0));
    u_xlat69 = dot(u_xlat5, float4(4.0, 3.0, 2.0, 1.0));
    u_xlat69 = (-u_xlat69) + 4.0;
    u_xlatu69 = uint(u_xlat69);
    u_xlati69 = int(int(u_xlatu69) << 2);
    u_xlat4.xyz = input.vs_INTERP11.yyy * _pad16.xyz;
    u_xlat4.xyz = _pad0.xyz * input.vs_INTERP11.xxx + u_xlat4.xyz;
    u_xlat4.xyz = _pad32.xyz * input.vs_INTERP11.zzz + u_xlat4.xyz;
    u_xlat4.xyz = u_xlat4.xyz + _pad48.xyz;
    u_xlat69 = input.vs_INTERP11.y * _tunity_MatrixV[1].z;
    u_xlat69 = _tunity_MatrixV[0].z * input.vs_INTERP11.x + u_xlat69;
    u_xlat69 = _tunity_MatrixV[2].z * input.vs_INTERP11.z + u_xlat69;
    u_xlat69 = u_xlat69 + _tunity_MatrixV[3].z;
    u_xlat69 = (-u_xlat69) + (-_pad352.y);
    u_xlat69 = max(u_xlat69, 0.0);
    u_xlat69 = u_xlat69 * _pad976.x;
    u_xlat5.xyz = _g_texture(_Texture_t3, input.vs_INTERP0.xy, _GlobalMipBias.x).xyz;
    u_xlat6 = _g_texture(_Texture_t4, input.vs_INTERP0.xy, _GlobalMipBias.x);
    u_xlat6.xyz = u_xlat6.xyz + float3(-0.5, -0.5, -0.5);
    u_xlat70 = dot(u_xlat22.xyz, u_xlat6.xyz);
    u_xlat70 = u_xlat70 + 0.5;
    u_xlat5.xyz = float3(u_xlat70) * u_xlat5.xyz;
    u_xlat70 = max(u_xlat6.w, 9.99999975e-05);
    u_xlat5.xyz = u_xlat5.xyz / float3(u_xlat70);
    u_xlat70 = (-u_xlat67) * 0.959999979 + 0.959999979;
    u_xlat71 = u_xlat68 + (-u_xlat70);
    u_xlat6.xyz = u_xlat2.xyz * float3(u_xlat70);
    u_xlat2.xyz = u_xlat2.xyz + float3(-0.0399999991, -0.0399999991, -0.0399999991);
    u_xlat2.xyz = float3(u_xlat67) * u_xlat2.xyz + float3(0.0399999991, 0.0399999991, 0.0399999991);
    u_xlat67 = (-u_xlat68) + 1.0;
    u_xlat68 = u_xlat67 * u_xlat67;
    u_xlat68 = max(u_xlat68, 0.0078125);
    u_xlat70 = u_xlat68 * u_xlat68;
    u_xlat71 = u_xlat71 + 1.0;
    u_xlat71 = min(u_xlat71, 1.0);
    u_xlat72 = u_xlat68 * 4.0 + 2.0;
    u_xlati0 = u_xlati0 * 9;
    u_xlatb7.x = 0.0<_pad432.y;
    if(u_xlatb7.x){
        u_xlatb7.x = _pad432.y==1.0;
        if(u_xlatb7.x){
            u_xlat7 = u_xlat4.xyxy + _pad400;
            float3 txVec0 = float3(u_xlat7.xy,u_xlat4.z);
            u_xlat8.x = _g_textureLod(_Texture_t5, txVec0, 0.0);
            float3 txVec1 = float3(u_xlat7.zw,u_xlat4.z);
            u_xlat8.y = _g_textureLod(_Texture_t5, txVec1, 0.0);
            u_xlat7 = u_xlat4.xyxy + _pad416;
            float3 txVec2 = float3(u_xlat7.xy,u_xlat4.z);
            u_xlat8.z = _g_textureLod(_Texture_t5, txVec2, 0.0);
            float3 txVec3 = float3(u_xlat7.zw,u_xlat4.z);
            u_xlat8.w = _g_textureLod(_Texture_t5, txVec3, 0.0);
            u_xlat7.x = dot(u_xlat8, float4(0.25, 0.25, 0.25, 0.25));
        } else {
            u_xlatb29 = _pad432.y==2.0;
            if(u_xlatb29){
                u_xlat29.xy = u_xlat4.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat29.xy = floor(u_xlat29.xy);
                u_xlat8.xy = u_xlat4.xy * _pad448.zw + (-u_xlat29.xy);
                u_xlat9 = u_xlat8.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat10 = u_xlat9.xxzz * u_xlat9.xxzz;
                u_xlat52.xy = u_xlat10.yw * float2(0.0799999982, 0.0799999982);
                u_xlat9.xz = u_xlat10.xz * float2(0.5, 0.5) + (-u_xlat8.xy);
                u_xlat10.xy = (-u_xlat8.xy) + float2(1.0, 1.0);
                u_xlat54.xy = min(u_xlat8.xy, float2(0.0, 0.0));
                u_xlat54.xy = (-u_xlat54.xy) * u_xlat54.xy + u_xlat10.xy;
                u_xlat8.xy = max(u_xlat8.xy, float2(0.0, 0.0));
                u_xlat8.xy = (-u_xlat8.xy) * u_xlat8.xy + u_xlat9.yw;
                u_xlat54.xy = u_xlat54.xy + float2(1.0, 1.0);
                u_xlat8.xy = u_xlat8.xy + float2(1.0, 1.0);
                u_xlat11.xy = u_xlat9.xz * float2(0.159999996, 0.159999996);
                u_xlat12.xy = u_xlat10.xy * float2(0.159999996, 0.159999996);
                u_xlat10.xy = u_xlat54.xy * float2(0.159999996, 0.159999996);
                u_xlat13.xy = u_xlat8.xy * float2(0.159999996, 0.159999996);
                u_xlat8.xy = u_xlat9.yw * float2(0.159999996, 0.159999996);
                u_xlat11.z = u_xlat10.x;
                u_xlat11.w = u_xlat8.x;
                u_xlat12.z = u_xlat13.x;
                u_xlat12.w = u_xlat52.x;
                u_xlat9 = u_xlat11.zwxz + u_xlat12.zwxz;
                u_xlat10.z = u_xlat11.y;
                u_xlat10.w = u_xlat8.y;
                u_xlat13.z = u_xlat12.y;
                u_xlat13.w = u_xlat52.y;
                u_xlat8.xyz = u_xlat10.zyw + u_xlat13.zyw;
                u_xlat10.xyz = u_xlat12.xzw / u_xlat9.zwy;
                u_xlat10.xyz = u_xlat10.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat11.xyz = u_xlat13.zyw / u_xlat8.xyz;
                u_xlat11.xyz = u_xlat11.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat10.xyz = u_xlat10.yxz * _pad448.xxx;
                u_xlat11.xyz = u_xlat11.xyz * _pad448.yyy;
                u_xlat10.w = u_xlat11.x;
                u_xlat12 = u_xlat29.xyxy * _pad448.xyxy + u_xlat10.ywxw;
                u_xlat13.xy = u_xlat29.xy * _pad448.xy + u_xlat10.zw;
                u_xlat11.w = u_xlat10.y;
                u_xlat10.yw = u_xlat11.yz;
                u_xlat14 = u_xlat29.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat11 = u_xlat29.xyxy * _pad448.xyxy + u_xlat11.wywz;
                u_xlat10 = u_xlat29.xyxy * _pad448.xyxy + u_xlat10.xwzw;
                u_xlat15 = u_xlat8.xxxy * u_xlat9.zwyz;
                u_xlat16 = u_xlat8.yyzz * u_xlat9;
                u_xlat29.x = u_xlat8.z * u_xlat9.y;
                float3 txVec4 = float3(u_xlat12.xy,u_xlat4.z);
                u_xlat51.x = _g_textureLod(_Texture_t5, txVec4, 0.0);
                float3 txVec5 = float3(u_xlat12.zw,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec5, 0.0);
                u_xlat73 = u_xlat73 * u_xlat15.y;
                u_xlat51.x = u_xlat15.x * u_xlat51.x + u_xlat73;
                float3 txVec6 = float3(u_xlat13.xy,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec6, 0.0);
                u_xlat51.x = u_xlat15.z * u_xlat73 + u_xlat51.x;
                float3 txVec7 = float3(u_xlat11.xy,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec7, 0.0);
                u_xlat51.x = u_xlat15.w * u_xlat73 + u_xlat51.x;
                float3 txVec8 = float3(u_xlat14.xy,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec8, 0.0);
                u_xlat51.x = u_xlat16.x * u_xlat73 + u_xlat51.x;
                float3 txVec9 = float3(u_xlat14.zw,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec9, 0.0);
                u_xlat51.x = u_xlat16.y * u_xlat73 + u_xlat51.x;
                float3 txVec10 = float3(u_xlat11.zw,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec10, 0.0);
                u_xlat51.x = u_xlat16.z * u_xlat73 + u_xlat51.x;
                float3 txVec11 = float3(u_xlat10.xy,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec11, 0.0);
                u_xlat51.x = u_xlat16.w * u_xlat73 + u_xlat51.x;
                float3 txVec12 = float3(u_xlat10.zw,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec12, 0.0);
                u_xlat7.x = u_xlat29.x * u_xlat73 + u_xlat51.x;
            } else {
                u_xlat29.xy = u_xlat4.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat29.xy = floor(u_xlat29.xy);
                u_xlat8.xy = u_xlat4.xy * _pad448.zw + (-u_xlat29.xy);
                u_xlat9 = u_xlat8.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat10 = u_xlat9.xxzz * u_xlat9.xxzz;
                u_xlat11.yw = u_xlat10.yw * float2(0.0408160016, 0.0408160016);
                u_xlat52.xy = u_xlat10.xz * float2(0.5, 0.5) + (-u_xlat8.xy);
                u_xlat9.xz = (-u_xlat8.xy) + float2(1.0, 1.0);
                u_xlat10.xy = min(u_xlat8.xy, float2(0.0, 0.0));
                u_xlat9.xz = (-u_xlat10.xy) * u_xlat10.xy + u_xlat9.xz;
                u_xlat10.xy = max(u_xlat8.xy, float2(0.0, 0.0));
                u_xlat9.yw = (-u_xlat10.xy) * u_xlat10.xy + u_xlat9.yw;
                u_xlat9 = u_xlat9 + float4(2.0, 2.0, 2.0, 2.0);
                u_xlat10.z = u_xlat9.y * 0.0816320032;
                u_xlat12.xy = u_xlat52.yx * float2(0.0816320032, 0.0816320032);
                u_xlat52.xy = u_xlat9.xz * float2(0.0816320032, 0.0816320032);
                u_xlat12.z = u_xlat9.w * 0.0816320032;
                u_xlat10.x = u_xlat12.y;
                u_xlat10.yw = u_xlat8.xx * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat9.xz = u_xlat8.xx * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat9.y = u_xlat52.x;
                u_xlat9.w = u_xlat11.y;
                u_xlat10 = u_xlat9 + u_xlat10;
                u_xlat12.yw = u_xlat8.yy * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat11.xz = u_xlat8.yy * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat11.y = u_xlat52.y;
                u_xlat8 = u_xlat11 + u_xlat12;
                u_xlat9 = u_xlat9 / u_xlat10;
                u_xlat9 = u_xlat9 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat11 = u_xlat11 / u_xlat8;
                u_xlat11 = u_xlat11 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat9 = u_xlat9.wxyz * _pad448.xxxx;
                u_xlat11 = u_xlat11.xwyz * _pad448.yyyy;
                u_xlat12.xzw = u_xlat9.yzw;
                u_xlat12.y = u_xlat11.x;
                u_xlat13 = u_xlat29.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat14.xy = u_xlat29.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.y = u_xlat12.y;
                u_xlat12.y = u_xlat11.z;
                u_xlat15 = u_xlat29.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat58.xy = u_xlat29.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.z = u_xlat12.y;
                u_xlat16 = u_xlat29.xyxy * _pad448.xyxy + u_xlat9.xyxz;
                u_xlat12.y = u_xlat11.w;
                u_xlat17 = u_xlat29.xyxy * _pad448.xyxy + u_xlat12.xyzy;
                u_xlat31.xy = u_xlat29.xy * _pad448.xy + u_xlat12.wy;
                u_xlat9.w = u_xlat12.y;
                u_xlat18.xy = u_xlat29.xy * _pad448.xy + u_xlat9.xw;
                u_xlat11.xzw = u_xlat12.xzw;
                u_xlat12 = u_xlat29.xyxy * _pad448.xyxy + u_xlat11.xyzy;
                u_xlat55.xy = u_xlat29.xy * _pad448.xy + u_xlat11.wy;
                u_xlat11.x = u_xlat9.x;
                u_xlat29.xy = u_xlat29.xy * _pad448.xy + u_xlat11.xy;
                u_xlat19 = u_xlat8.xxxx * u_xlat10;
                u_xlat20 = u_xlat8.yyyy * u_xlat10;
                u_xlat21 = u_xlat8.zzzz * u_xlat10;
                u_xlat8 = u_xlat8.wwww * u_xlat10;
                float3 txVec13 = float3(u_xlat13.xy,u_xlat4.z);
                u_xlat73 = _g_textureLod(_Texture_t5, txVec13, 0.0);
                float3 txVec14 = float3(u_xlat13.zw,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec14, 0.0);
                u_xlat9.x = u_xlat9.x * u_xlat19.y;
                u_xlat73 = u_xlat19.x * u_xlat73 + u_xlat9.x;
                float3 txVec15 = float3(u_xlat14.xy,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec15, 0.0);
                u_xlat73 = u_xlat19.z * u_xlat9.x + u_xlat73;
                float3 txVec16 = float3(u_xlat16.xy,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec16, 0.0);
                u_xlat73 = u_xlat19.w * u_xlat9.x + u_xlat73;
                float3 txVec17 = float3(u_xlat15.xy,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec17, 0.0);
                u_xlat73 = u_xlat20.x * u_xlat9.x + u_xlat73;
                float3 txVec18 = float3(u_xlat15.zw,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec18, 0.0);
                u_xlat73 = u_xlat20.y * u_xlat9.x + u_xlat73;
                float3 txVec19 = float3(u_xlat58.xy,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec19, 0.0);
                u_xlat73 = u_xlat20.z * u_xlat9.x + u_xlat73;
                float3 txVec20 = float3(u_xlat16.zw,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec20, 0.0);
                u_xlat73 = u_xlat20.w * u_xlat9.x + u_xlat73;
                float3 txVec21 = float3(u_xlat17.xy,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec21, 0.0);
                u_xlat73 = u_xlat21.x * u_xlat9.x + u_xlat73;
                float3 txVec22 = float3(u_xlat17.zw,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec22, 0.0);
                u_xlat73 = u_xlat21.y * u_xlat9.x + u_xlat73;
                float3 txVec23 = float3(u_xlat31.xy,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec23, 0.0);
                u_xlat73 = u_xlat21.z * u_xlat9.x + u_xlat73;
                float3 txVec24 = float3(u_xlat18.xy,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec24, 0.0);
                u_xlat73 = u_xlat21.w * u_xlat9.x + u_xlat73;
                float3 txVec25 = float3(u_xlat12.xy,u_xlat4.z);
                u_xlat9.x = _g_textureLod(_Texture_t5, txVec25, 0.0);
                u_xlat73 = u_xlat8.x * u_xlat9.x + u_xlat73;
                float3 txVec26 = float3(u_xlat12.zw,u_xlat4.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec26, 0.0);
                u_xlat73 = u_xlat8.y * u_xlat8.x + u_xlat73;
                float3 txVec27 = float3(u_xlat55.xy,u_xlat4.z);
                u_xlat8.x = _g_textureLod(_Texture_t5, txVec27, 0.0);
                u_xlat73 = u_xlat8.z * u_xlat8.x + u_xlat73;
                float3 txVec28 = float3(u_xlat29.xy,u_xlat4.z);
                u_xlat29.x = _g_textureLod(_Texture_t5, txVec28, 0.0);
                u_xlat7.x = u_xlat8.w * u_xlat29.x + u_xlat73;
            }
        }
    } else {
        float3 txVec29 = float3(u_xlat4.xy,u_xlat4.z);
        u_xlat7.x = _g_textureLod(_Texture_t5, txVec29, 0.0);
    }
    u_xlat4.x = (-_pad432.x) + 1.0;
    u_xlat4.x = u_xlat7.x * _pad432.x + u_xlat4.x;
    u_xlatb26.x = 0.0>=u_xlat4.z;
    u_xlatb48 = u_xlat4.z>=1.0;
    u_xlatb26.x = u_xlatb48 || u_xlatb26.x;
    u_xlat4.x = (u_xlatb26.x) ? 1.0 : u_xlat4.x;
    u_xlat7.xyz = input.vs_INTERP11.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat26.x = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat26.x = u_xlat26.x * _pad432.z + _pad432.w;
    u_xlat26.x = clamp(u_xlat26.x, 0.0, 1.0);
    u_xlat48.x = (-u_xlat4.x) + 1.0;
    u_xlat4.x = u_xlat26.x * u_xlat48.x + u_xlat4.x;
    u_xlatb26.x = _pad176.y!=-1.0;
    if(u_xlatb26.x){
        u_xlat26.xy = input.vs_INTERP11.yy * _pad16.xy;
        u_xlat26.xy = _pad0.xy * input.vs_INTERP11.xx + u_xlat26.xy;
        u_xlat26.xy = _pad32.xy * input.vs_INTERP11.zz + u_xlat26.xy;
        u_xlat26.xy = u_xlat26.xy + _pad48.xy;
        u_xlat26.xy = u_xlat26.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
        u_xlat7 = _g_texture(_Texture_t6, u_xlat26.xy, _GlobalMipBias.x);
        u_xlatb26.xy = _g_equal(_pad176.yyyy, float4(0.0, 1.0, 0.0, 0.0)).xy;
        u_xlat48.x = (u_xlatb26.y) ? u_xlat7.w : u_xlat7.x;
        u_xlat7.xyz = (u_xlatb26.x) ? u_xlat7.xyz : u_xlat48.xxx;
    } else {
        u_xlat7.x = float(1.0);
        u_xlat7.y = float(1.0);
        u_xlat7.z = float(1.0);
    }
    u_xlat7.xyz = u_xlat7.xyz * _MainLightColor.xyz;
    u_xlat26.x = dot((-u_xlat3.xyz), u_xlat22.xyz);
    u_xlat26.x = u_xlat26.x + u_xlat26.x;
    u_xlat8.xyz = u_xlat22.xyz * (-u_xlat26.xxx) + (-u_xlat3.xyz);
    u_xlat26.x = dot(u_xlat22.xyz, u_xlat3.xyz);
    u_xlat26.x = clamp(u_xlat26.x, 0.0, 1.0);
    u_xlat26.x = (-u_xlat26.x) + 1.0;
    u_xlat26.x = u_xlat26.x * u_xlat26.x;
    u_xlat26.x = u_xlat26.x * u_xlat26.x;
    u_xlat48.x = (-u_xlat67) * 0.699999988 + 1.70000005;
    u_xlat67 = u_xlat67 * u_xlat48.x;
    u_xlat67 = u_xlat67 * 6.0;
    u_xlat9.xyz = unity_SpecCube0_BoxMax.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat10.xyz = u_xlat9.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube0_BoxMin.xyz;
    u_xlat11.xyz = (-u_xlat10.xyz) + input.vs_INTERP11.xyz;
    u_xlat12 = unity_SpecCube0_Rotation.zzxy + unity_SpecCube0_Rotation.zzxy;
    u_xlat13.xyz = u_xlat12.zwy * unity_SpecCube0_Rotation.xyz;
    u_xlat14 = u_xlat12 * unity_SpecCube0_Rotation.xyww;
    u_xlat48.x = u_xlat12.y * unity_SpecCube0_Rotation.w;
    u_xlat13.xyz = u_xlat13.zzy + u_xlat13.yxx;
    u_xlat13.xyz = (-u_xlat13.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat15.xy = u_xlat11.xy * u_xlat13.xy;
    u_xlat73 = unity_SpecCube0_Rotation.x * u_xlat12.w + (-u_xlat48.x);
    u_xlat74 = u_xlat73 * u_xlat11.y + u_xlat15.x;
    u_xlat14.xy = u_xlat14.wz + u_xlat14.xy;
    u_xlat75 = u_xlat11.y * u_xlat14.y;
    u_xlat16.x = u_xlat14.x * u_xlat11.z + u_xlat74;
    u_xlat48.x = unity_SpecCube0_Rotation.x * u_xlat12.w + u_xlat48.x;
    u_xlat74 = u_xlat48.x * u_xlat11.x + u_xlat15.y;
    u_xlat33.xz = unity_SpecCube0_Rotation.yx * u_xlat12.yx + (-u_xlat14.zw);
    u_xlat16.y = u_xlat33.x * u_xlat11.z + u_xlat74;
    u_xlat74 = u_xlat33.z * u_xlat11.x + u_xlat75;
    u_xlat16.z = u_xlat13.z * u_xlat11.z + u_xlat74;
    u_xlat10.xyz = u_xlat10.xyz + u_xlat16.xyz;
    u_xlat12.xyz = unity_SpecCube1_BoxMax.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat15.xyz = u_xlat12.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube1_BoxMin.xyz;
    u_xlat16.xyz = (-u_xlat15.xyz) + input.vs_INTERP11.xyz;
    u_xlat17 = unity_SpecCube1_Rotation.zzxy + unity_SpecCube1_Rotation.zzxy;
    u_xlat18.xyz = u_xlat17.zwy * unity_SpecCube1_Rotation.xyz;
    u_xlat19 = u_xlat17 * unity_SpecCube1_Rotation.xyww;
    u_xlat74 = u_xlat17.y * unity_SpecCube1_Rotation.w;
    u_xlat18.xyz = u_xlat18.zzy + u_xlat18.yxx;
    u_xlat18.xyz = (-u_xlat18.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat11.xz = u_xlat16.xy * u_xlat18.xy;
    u_xlat75 = unity_SpecCube1_Rotation.x * u_xlat17.w + (-u_xlat74);
    u_xlat76 = u_xlat75 * u_xlat16.y + u_xlat11.x;
    u_xlat58.xy = u_xlat19.wz + u_xlat19.xy;
    u_xlat11.x = u_xlat16.y * u_xlat58.y;
    u_xlat20.x = u_xlat58.x * u_xlat16.z + u_xlat76;
    u_xlat74 = unity_SpecCube1_Rotation.x * u_xlat17.w + u_xlat74;
    u_xlat76 = u_xlat74 * u_xlat16.x + u_xlat11.z;
    u_xlat38.xz = unity_SpecCube1_Rotation.yx * u_xlat17.yx + (-u_xlat19.zw);
    u_xlat20.y = u_xlat38.x * u_xlat16.z + u_xlat76;
    u_xlat76 = u_xlat38.z * u_xlat16.x + u_xlat11.x;
    u_xlat20.z = u_xlat18.z * u_xlat16.z + u_xlat76;
    u_xlat15.xyz = u_xlat15.xyz + u_xlat20.xyz;
    u_xlat9.x = dot(u_xlat9.xyz, u_xlat9.xyz);
    u_xlat31.x = dot(u_xlat12.xyz, u_xlat12.xyz);
    u_xlat9.x = (-u_xlat31.x) + u_xlat9.x;
    u_xlatb31 = 0.0<unity_SpecCube1_BoxMin.w;
    u_xlatb53 = unity_SpecCube1_BoxMin.w==0.0;
    u_xlatb76 = u_xlat9.x<-9.99999975e-05;
    u_xlatb76 = u_xlatb53 && u_xlatb76;
    u_xlatb31 = u_xlatb31 || u_xlatb76;
    u_xlatb76 = unity_SpecCube1_BoxMin.w<0.0;
    u_xlatb9 = 9.99999975e-05<u_xlat9.x;
    u_xlatb9 = u_xlatb9 && u_xlatb53;
    u_xlatb9 = u_xlatb9 || u_xlatb76;
    u_xlat12.xyz = u_xlat10.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat17.xyz = (-u_xlat10.xyz) + unity_SpecCube0_BoxMax.xyz;
    u_xlat12.xyz = min(u_xlat12.xyz, u_xlat17.xyz);
    u_xlat12.xyz = u_xlat12.xyz / unity_SpecCube0_BoxMax.www;
    u_xlat53 = min(u_xlat12.z, u_xlat12.y);
    u_xlat53 = min(u_xlat53, u_xlat12.x);
    u_xlat53 = clamp(u_xlat53, 0.0, 1.0);
    u_xlat12.xyz = u_xlat15.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat17.xyz = (-u_xlat15.xyz) + unity_SpecCube1_BoxMax.xyz;
    u_xlat12.xyz = min(u_xlat12.xyz, u_xlat17.xyz);
    u_xlat12.xyz = u_xlat12.xyz / unity_SpecCube1_BoxMax.www;
    u_xlat76 = min(u_xlat12.z, u_xlat12.y);
    u_xlat76 = min(u_xlat76, u_xlat12.x);
    u_xlat76 = clamp(u_xlat76, 0.0, 1.0);
    u_xlat11.x = (-u_xlat76) + 1.0;
    u_xlat11.x = min(u_xlat53, u_xlat11.x);
    u_xlat9.x = (u_xlatb9) ? u_xlat11.x : u_xlat53;
    u_xlat53 = (-u_xlat53) + 1.0;
    u_xlat53 = min(u_xlat53, u_xlat76);
    u_xlat9.y = (u_xlatb31) ? u_xlat53 : u_xlat76;
    u_xlat53 = u_xlat9.y + u_xlat9.x;
    u_xlat76 = max(u_xlat53, 1.0);
    u_xlat9.xy = u_xlat9.xy / float2(u_xlat76);
    u_xlatb76 = 0.00999999978<u_xlat9.x;
    if(u_xlatb76){
        u_xlat11.xz = u_xlat8.xy * u_xlat13.xy;
        u_xlat73 = u_xlat73 * u_xlat8.y + u_xlat11.x;
        u_xlat76 = u_xlat8.y * u_xlat14.y;
        u_xlat12.x = u_xlat14.x * u_xlat8.z + u_xlat73;
        u_xlat48.x = u_xlat48.x * u_xlat8.x + u_xlat11.z;
        u_xlat12.y = u_xlat33.x * u_xlat8.z + u_xlat48.x;
        u_xlat48.x = u_xlat33.z * u_xlat8.x + u_xlat76;
        u_xlat12.z = u_xlat13.z * u_xlat8.z + u_xlat48.x;
        u_xlatb48 = 0.0<unity_SpecCube0_ProbePosition.w;
        u_xlatb11.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat12.xyzx).xyz;
        u_xlat11.x = (u_xlatb11.x) ? unity_SpecCube0_BoxMax.x : unity_SpecCube0_BoxMin.x;
        u_xlat11.y = (u_xlatb11.y) ? unity_SpecCube0_BoxMax.y : unity_SpecCube0_BoxMin.y;
        u_xlat11.z = (u_xlatb11.z) ? unity_SpecCube0_BoxMax.z : unity_SpecCube0_BoxMin.z;
        u_xlat11.xyz = (-u_xlat10.xyz) + u_xlat11.xyz;
        u_xlat11.xyz = u_xlat11.xyz / u_xlat12.xyz;
        u_xlat73 = min(u_xlat11.y, u_xlat11.x);
        u_xlat73 = min(u_xlat11.z, u_xlat73);
        u_xlat10.xyz = u_xlat10.xyz + (-unity_SpecCube0_ProbePosition.xyz);
        u_xlat10.xyz = u_xlat12.xyz * float3(u_xlat73) + u_xlat10.xyz;
        u_xlat10.xyz = (bool(u_xlatb48)) ? u_xlat10.xyz : u_xlat12.xyz;
        u_xlat11.xyz = unity_SpecCube0_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat12.xyz = u_xlat11.yzx * (-unity_SpecCube0_Rotation.xyz);
        u_xlat13.xyz = u_xlat11.xyz * unity_SpecCube0_Rotation.www;
        u_xlat12.xyz = u_xlat12.zzy + u_xlat12.yxx;
        u_xlat12.xyz = (-u_xlat12.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat33.xz = u_xlat10.xy * u_xlat12.xy;
        u_xlat48.x = (-unity_SpecCube0_Rotation.x) * u_xlat11.z + (-u_xlat13.x);
        u_xlat48.x = u_xlat48.x * u_xlat10.y + u_xlat33.x;
        u_xlat12.xy = (-unity_SpecCube0_Rotation.xy) * u_xlat11.xx + u_xlat13.zy;
        u_xlat73 = u_xlat10.y * u_xlat12.y;
        u_xlat17.x = u_xlat12.x * u_xlat10.z + u_xlat48.x;
        u_xlat48.x = (-unity_SpecCube0_Rotation.x) * u_xlat11.z + u_xlat13.x;
        u_xlat48.x = u_xlat48.x * u_xlat10.x + u_xlat33.z;
        u_xlat32.xz = (-unity_SpecCube0_Rotation.yx) * u_xlat11.xx + (-u_xlat13.yz);
        u_xlat17.y = u_xlat32.x * u_xlat10.z + u_xlat48.x;
        u_xlat48.x = u_xlat32.z * u_xlat10.x + u_xlat73;
        u_xlat17.z = u_xlat12.z * u_xlat10.z + u_xlat48.x;
        u_xlat10 = _g_textureLod(_Texture_t1, u_xlat17.xyz, u_xlat67);
        u_xlat48.x = u_xlat10.w + -1.0;
        u_xlat48.x = unity_SpecCube0_HDR.w * u_xlat48.x + 1.0;
        u_xlat48.x = max(u_xlat48.x, 0.0);
        u_xlat48.x = log2(u_xlat48.x);
        u_xlat48.x = u_xlat48.x * unity_SpecCube0_HDR.y;
        u_xlat48.x = exp2(u_xlat48.x);
        u_xlat48.x = u_xlat48.x * unity_SpecCube0_HDR.x;
        u_xlat10.xyz = u_xlat10.xyz * u_xlat48.xxx;
        u_xlat10.xyz = u_xlat9.xxx * u_xlat10.xyz;
    } else {
        u_xlat10.x = float(0.0);
        u_xlat10.y = float(0.0);
        u_xlat10.z = float(0.0);
    }
    u_xlatb48 = 0.00999999978<u_xlat9.y;
    if(u_xlatb48){
        u_xlat11.xy = u_xlat8.xy * u_xlat18.xy;
        u_xlat48.x = u_xlat75 * u_xlat8.y + u_xlat11.x;
        u_xlat73 = u_xlat8.y * u_xlat58.y;
        u_xlat12.x = u_xlat58.x * u_xlat8.z + u_xlat48.x;
        u_xlat48.x = u_xlat74 * u_xlat8.x + u_xlat11.y;
        u_xlat12.y = u_xlat38.x * u_xlat8.z + u_xlat48.x;
        u_xlat48.x = u_xlat38.z * u_xlat8.x + u_xlat73;
        u_xlat12.z = u_xlat18.z * u_xlat8.z + u_xlat48.x;
        u_xlatb48 = 0.0<unity_SpecCube1_ProbePosition.w;
        u_xlatb11.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat12.xyzx).xyz;
        u_xlat11.x = (u_xlatb11.x) ? unity_SpecCube1_BoxMax.x : unity_SpecCube1_BoxMin.x;
        u_xlat11.y = (u_xlatb11.y) ? unity_SpecCube1_BoxMax.y : unity_SpecCube1_BoxMin.y;
        u_xlat11.z = (u_xlatb11.z) ? unity_SpecCube1_BoxMax.z : unity_SpecCube1_BoxMin.z;
        u_xlat11.xyz = (-u_xlat15.xyz) + u_xlat11.xyz;
        u_xlat11.xyz = u_xlat11.xyz / u_xlat12.xyz;
        u_xlat73 = min(u_xlat11.y, u_xlat11.x);
        u_xlat73 = min(u_xlat11.z, u_xlat73);
        u_xlat11.xyz = u_xlat15.xyz + (-unity_SpecCube1_ProbePosition.xyz);
        u_xlat11.xyz = u_xlat12.xyz * float3(u_xlat73) + u_xlat11.xyz;
        u_xlat11.xyz = (bool(u_xlatb48)) ? u_xlat11.xyz : u_xlat12.xyz;
        u_xlat12.xyz = unity_SpecCube1_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat13.xyz = u_xlat12.yzx * (-unity_SpecCube1_Rotation.xyz);
        u_xlat14.xyz = u_xlat12.xyz * unity_SpecCube1_Rotation.www;
        u_xlat13.xyz = u_xlat13.zzy + u_xlat13.yxx;
        u_xlat13.xyz = (-u_xlat13.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat9.xw = u_xlat11.xy * u_xlat13.xy;
        u_xlat48.x = (-unity_SpecCube1_Rotation.x) * u_xlat12.z + (-u_xlat14.x);
        u_xlat48.x = u_xlat48.x * u_xlat11.y + u_xlat9.x;
        u_xlat34.xz = (-unity_SpecCube1_Rotation.xy) * u_xlat12.xx + u_xlat14.zy;
        u_xlat73 = u_xlat11.y * u_xlat34.z;
        u_xlat15.x = u_xlat34.x * u_xlat11.z + u_xlat48.x;
        u_xlat48.x = (-unity_SpecCube1_Rotation.x) * u_xlat12.z + u_xlat14.x;
        u_xlat48.x = u_xlat48.x * u_xlat11.x + u_xlat9.w;
        u_xlat9.xw = (-unity_SpecCube1_Rotation.yx) * u_xlat12.xx + (-u_xlat14.yz);
        u_xlat15.y = u_xlat9.x * u_xlat11.z + u_xlat48.x;
        u_xlat48.x = u_xlat9.w * u_xlat11.x + u_xlat73;
        u_xlat15.z = u_xlat13.z * u_xlat11.z + u_xlat48.x;
        u_xlat11 = _g_textureLod(_Texture_t2, u_xlat15.xyz, u_xlat67);
        u_xlat48.x = u_xlat11.w + -1.0;
        u_xlat48.x = unity_SpecCube1_HDR.w * u_xlat48.x + 1.0;
        u_xlat48.x = max(u_xlat48.x, 0.0);
        u_xlat48.x = log2(u_xlat48.x);
        u_xlat48.x = u_xlat48.x * unity_SpecCube1_HDR.y;
        u_xlat48.x = exp2(u_xlat48.x);
        u_xlat48.x = u_xlat48.x * unity_SpecCube1_HDR.x;
        u_xlat11.xyz = u_xlat11.xyz * u_xlat48.xxx;
        u_xlat10.xyz = u_xlat9.yyy * u_xlat11.xyz + u_xlat10.xyz;
    }
    u_xlatb48 = u_xlat53<0.99000001;
    if(u_xlatb48){
        u_xlat8 = _g_textureLod(_Texture_t0, u_xlat8.xyz, u_xlat67);
        u_xlat67 = (-u_xlat53) + 1.0;
        u_xlat48.x = u_xlat8.w + -1.0;
        u_xlat48.x = _GlossyEnvironmentCubeMap_HDR.w * u_xlat48.x + 1.0;
        u_xlat48.x = max(u_xlat48.x, 0.0);
        u_xlat48.x = log2(u_xlat48.x);
        u_xlat48.x = u_xlat48.x * _GlossyEnvironmentCubeMap_HDR.y;
        u_xlat48.x = exp2(u_xlat48.x);
        u_xlat48.x = u_xlat48.x * _GlossyEnvironmentCubeMap_HDR.x;
        u_xlat8.xyz = u_xlat8.xyz * u_xlat48.xxx;
        u_xlat10.xyz = float3(u_xlat67) * u_xlat8.xyz + u_xlat10.xyz;
    }
    u_xlat8.xy = float2(u_xlat68) * float2(u_xlat68) + float2(-1.0, 1.0);
    u_xlat67 = float(1.0) / u_xlat8.y;
    u_xlat30.xyz = (-u_xlat2.xyz) + float3(u_xlat71);
    u_xlat30.xyz = u_xlat26.xxx * u_xlat30.xyz + u_xlat2.xyz;
    u_xlat30.xyz = float3(u_xlat67) * u_xlat30.xyz;
    u_xlat30.xyz = u_xlat30.xyz * u_xlat10.xyz;
    u_xlat5.xyz = u_xlat5.xyz * u_xlat6.xyz + u_xlat30.xyz;
    u_xlati67 = int(uint(uint(_g_floatBitsToUint(_MainLightLayerMask)) & uint(_g_floatBitsToUint(_pad128.x))));
    u_xlat68 = u_xlat4.x * unity_LightData.z;
    u_xlat4.x = dot(u_xlat22.xyz, _MainLightPosition.xyz);
    u_xlat4.x = clamp(u_xlat4.x, 0.0, 1.0);
    u_xlat68 = u_xlat68 * u_xlat4.x;
    u_xlat4.xyz = float3(u_xlat68) * u_xlat7.xyz;
    u_xlat7.xyz = u_xlat3.xyz + _MainLightPosition.xyz;
    u_xlat68 = dot(u_xlat7.xyz, u_xlat7.xyz);
    u_xlat68 = max(u_xlat68, 1.17549435e-38);
    u_xlat68 = _g_inversesqrt(u_xlat68);
    u_xlat7.xyz = float3(u_xlat68) * u_xlat7.xyz;
    u_xlat68 = dot(u_xlat22.xyz, u_xlat7.xyz);
    u_xlat68 = clamp(u_xlat68, 0.0, 1.0);
    u_xlat71 = dot(_MainLightPosition.xyz, u_xlat7.xyz);
    u_xlat71 = clamp(u_xlat71, 0.0, 1.0);
    u_xlat68 = u_xlat68 * u_xlat68;
    u_xlat68 = u_xlat68 * u_xlat8.x + 1.00001001;
    u_xlat71 = u_xlat71 * u_xlat71;
    u_xlat68 = u_xlat68 * u_xlat68;
    u_xlat71 = max(u_xlat71, 0.100000001);
    u_xlat68 = u_xlat68 * u_xlat71;
    u_xlat68 = u_xlat72 * u_xlat68;
    u_xlat68 = u_xlat70 / u_xlat68;
    u_xlat7.xyz = u_xlat2.xyz * float3(u_xlat68) + u_xlat6.xyz;
    u_xlat4.xyz = u_xlat4.xyz * u_xlat7.xyz;
    u_xlat4.xyz = (int(u_xlati67) != 0) ? u_xlat4.xyz : float3(0.0, 0.0, 0.0);
    u_xlat67 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu67 =  uint(int(u_xlat67));
    u_xlatb7.xy = _g_equal(_pad176.zzzz, float4(0.0, 1.0, 0.0, 0.0)).xy;
    u_xlat30.x = float(0.0);
    u_xlat30.y = float(0.0);
    u_xlat30.z = float(0.0);
    for(uint u_xlatu_loop_3 = uint(0u) ; u_xlatu_loop_3<u_xlatu67 ; u_xlatu_loop_3++)
    {
        u_xlatu71 = uint(u_xlatu_loop_3 >> 2u);
        u_xlati51 = int(uint(u_xlatu_loop_3 & 3u));
        u_xlat71 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati51]);
        u_xlatu71 =  uint(int(u_xlat71));
        u_xlat9.xyz = (-input.vs_INTERP11.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat51.x = dot(u_xlat9.xyz, u_xlat9.xyz);
        u_xlat51.x = max(u_xlat51.x, 6.10351562e-05);
        u_xlat73 = _g_inversesqrt(u_xlat51.x);
        u_xlat10.xyz = float3(u_xlat73) * u_xlat9.xyz;
        u_xlat75 = float(1.0) / float(u_xlat51.x);
        u_xlat51.x = u_xlat51.x * _AdditionalLightsAttenuation.x;
        u_xlat51.x = (-u_xlat51.x) * u_xlat51.x + 1.0;
        u_xlat51.x = max(u_xlat51.x, 0.0);
        u_xlat51.x = u_xlat51.x * u_xlat51.x;
        u_xlat51.x = u_xlat51.x * u_xlat75;
        u_xlat75 = dot(_AdditionalLightsSpotDir.xyz, u_xlat10.xyz);
        u_xlat75 = u_xlat75 * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat75 = clamp(u_xlat75, 0.0, 1.0);
        u_xlat75 = u_xlat75 * u_xlat75;
        u_xlat51.x = u_xlat51.x * u_xlat75;
        u_xlatu75 = uint(u_xlatu71 >> 5u);
        u_xlati76 = int(1 << int(u_xlatu71));
        u_xlati75 = int(uint(uint(u_xlati76) & uint(_g_floatBitsToUint(_pad64.x))));
        if(u_xlati75 != 0) {
            u_xlati75 = int(_pad20672.x);
            u_xlati76 = (u_xlati75 != 0) ? 0 : 1;
            u_xlati11 = int(int(u_xlatu71) << 2);
            if(u_xlati76 != 0) {
                u_xlat33.xyz = input.vs_INTERP11.yyy * _pad208.xyw;
                u_xlat33.xyz = _pad192.xyw * input.vs_INTERP11.xxx + u_xlat33.xyz;
                u_xlat33.xyz = _pad224.xyw * input.vs_INTERP11.zzz + u_xlat33.xyz;
                u_xlat33.xyz = u_xlat33.xyz + _pad240.xyw;
                u_xlat33.xy = u_xlat33.xy / u_xlat33.zz;
                u_xlat33.xy = u_xlat33.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                u_xlat33.xy = clamp(u_xlat33.xy, 0.0, 1.0);
                u_xlat33.xy = _pad16576.xy * u_xlat33.xy + _pad16576.zw;
            } else {
                u_xlatb75 = u_xlati75==1;
                u_xlati75 = u_xlatb75 ? 1 : int(0);
                if(u_xlati75 != 0) {
                    u_xlat12.xy = input.vs_INTERP11.yy * _pad208.xy;
                    u_xlat12.xy = _pad192.xy * input.vs_INTERP11.xx + u_xlat12.xy;
                    u_xlat12.xy = _pad224.xy * input.vs_INTERP11.zz + u_xlat12.xy;
                    u_xlat12.xy = u_xlat12.xy + _pad240.xy;
                    u_xlat12.xy = u_xlat12.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat12.xy = _g_fract(u_xlat12.xy);
                    u_xlat33.xy = _pad16576.xy * u_xlat12.xy + _pad16576.zw;
                } else {
                    u_xlat12 = input.vs_INTERP11.yyyy * _pad208;
                    u_xlat12 = _pad192 * input.vs_INTERP11.xxxx + u_xlat12;
                    u_xlat12 = _pad224 * input.vs_INTERP11.zzzz + u_xlat12;
                    u_xlat12 = u_xlat12 + _pad240;
                    u_xlat12.xyz = u_xlat12.xyz / u_xlat12.www;
                    u_xlat75 = dot(u_xlat12.xyz, u_xlat12.xyz);
                    u_xlat75 = _g_inversesqrt(u_xlat75);
                    u_xlat12.xyz = float3(u_xlat75) * u_xlat12.xyz;
                    u_xlat75 = dot(abs(u_xlat12.xyz), float3(1.0, 1.0, 1.0));
                    u_xlat75 = max(u_xlat75, 9.99999997e-07);
                    u_xlat75 = float(1.0) / float(u_xlat75);
                    u_xlat13.xyz = float3(u_xlat75) * u_xlat12.zxy;
                    u_xlat13.x = (-u_xlat13.x);
                    u_xlat13.x = clamp(u_xlat13.x, 0.0, 1.0);
                    u_xlatb11.xw = _g_greaterThanEqual(u_xlat13.yyyz, float4(0.0, 0.0, 0.0, 0.0)).xw;
                    u_xlat11.x = (u_xlatb11.x) ? u_xlat13.x : (-u_xlat13.x);
                    u_xlat11.w = (u_xlatb11.w) ? u_xlat13.x : (-u_xlat13.x);
                    u_xlat11.xw = u_xlat12.xy * float2(u_xlat75) + u_xlat11.xw;
                    u_xlat11.xw = u_xlat11.xw * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat11.xw = clamp(u_xlat11.xw, 0.0, 1.0);
                    u_xlat33.xy = _pad16576.xy * u_xlat11.xw + _pad16576.zw;
                }
            }
            u_xlat11 = _g_textureLod(_Texture_t7, u_xlat33.xy, 0.0);
            u_xlat75 = (u_xlatb7.y) ? u_xlat11.w : u_xlat11.x;
            u_xlat11.xyz = (u_xlatb7.x) ? u_xlat11.xyz : float3(u_xlat75);
        } else {
            u_xlat11.x = float(1.0);
            u_xlat11.y = float(1.0);
            u_xlat11.z = float(1.0);
        }
        u_xlat11.xyz = u_xlat11.xyz * _AdditionalLightsColor.xyz;
        u_xlati71 = int(uint(uint(_g_floatBitsToUint(_AdditionalLightsLayerMasks)) & uint(_g_floatBitsToUint(_pad128.x))));
        u_xlat75 = dot(u_xlat22.xyz, u_xlat10.xyz);
        u_xlat75 = clamp(u_xlat75, 0.0, 1.0);
        u_xlat51.x = u_xlat51.x * u_xlat75;
        u_xlat11.xyz = u_xlat51.xxx * u_xlat11.xyz;
        u_xlat9.xyz = u_xlat9.xyz * float3(u_xlat73) + u_xlat3.xyz;
        u_xlat51.x = dot(u_xlat9.xyz, u_xlat9.xyz);
        u_xlat51.x = max(u_xlat51.x, 1.17549435e-38);
        u_xlat51.x = _g_inversesqrt(u_xlat51.x);
        u_xlat9.xyz = u_xlat51.xxx * u_xlat9.xyz;
        u_xlat51.x = dot(u_xlat22.xyz, u_xlat9.xyz);
        u_xlat51.x = clamp(u_xlat51.x, 0.0, 1.0);
        u_xlat51.y = dot(u_xlat10.xyz, u_xlat9.xyz);
        u_xlat51.y = clamp(u_xlat51.y, 0.0, 1.0);
        u_xlat51.xy = u_xlat51.xy * u_xlat51.xy;
        u_xlat51.x = u_xlat51.x * u_xlat8.x + 1.00001001;
        u_xlat51.x = u_xlat51.x * u_xlat51.x;
        u_xlat73 = max(u_xlat51.y, 0.100000001);
        u_xlat51.x = u_xlat73 * u_xlat51.x;
        u_xlat51.x = u_xlat72 * u_xlat51.x;
        u_xlat51.x = u_xlat70 / u_xlat51.x;
        u_xlat9.xyz = u_xlat2.xyz * u_xlat51.xxx + u_xlat6.xyz;
        u_xlat9.xyz = u_xlat9.xyz * u_xlat11.xyz + u_xlat30.xyz;
        u_xlat30.xyz = (int(u_xlati71) != 0) ? u_xlat9.xyz : u_xlat30.xyz;
    }
    u_xlat22.xyz = u_xlat4.xyz + u_xlat5.xyz;
    u_xlat22.xyz = u_xlat30.xyz + u_xlat22.xyz;
    u_xlat22.xyz = float3(_Emission) * u_xlat1.xyz + u_xlat22.xyz;
    u_xlat1.x = u_xlat69 * (-u_xlat69);
    u_xlat1.x = exp2(u_xlat1.x);
    u_xlat23.x = (-u_xlat1.x) + 1.0;
    u_xlat23.xyz = u_xlat23.xxx * _pad992.xyz;
    __SV_Target0.xyz = u_xlat22.xyz * u_xlat1.xxx + u_xlat23.xyz;
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
