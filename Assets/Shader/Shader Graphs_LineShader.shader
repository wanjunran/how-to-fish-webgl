Shader "Shader Graphs/LineShader"
{
    Properties
    {



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

            float3 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            float4 ImmCB_0_0_0[4];
            float4 u_xlat2;
            bool4 u_xlatb2;
            float4 u_xlat3;
            float4 u_xlat4;
            float4 u_xlat5;
            bool2 u_xlatb5;
            bool u_xlatb6;
            float4 u_xlat7;
            float4 u_xlat8;
            bool3 u_xlatb8;
            float4 u_xlat9;
            float4 u_xlat10;
            float4 u_xlat11;
            float4 u_xlat12;
            float4 u_xlat13;
            float4 u_xlat14;
            float4 u_xlat15;
            float4 u_xlat16;
            float4 u_xlat17;
            float4 u_xlat18;
            float u_xlat21;
            bool u_xlatb21;
            float3 u_xlat25;
            bool u_xlatb25;
            float3 u_xlat26;
            float3 u_xlat27;
            float3 u_xlat28;
            float3 u_xlat32;
            float2 u_xlat43;
            float u_xlat44;
            bool u_xlatb44;
            float2 u_xlat45;
            float2 u_xlat46;
            float2 u_xlat47;
            bool2 u_xlatb47;
            float2 u_xlat49;
            float2 u_xlat51;
            float u_xlat57;
            int u_xlati57;
            uint u_xlatu57;
            bool u_xlatb57;
            float u_xlat58;
            uint u_xlatu58;
            bool u_xlatb58;
            float u_xlat59;
            int u_xlati59;
            uint u_xlatu59;
            bool u_xlatb59;
            float u_xlat60;
            int u_xlati60;
            uint u_xlatu60;
            bool u_xlatb60;
            float u_xlat61;
            int u_xlati61;
            float u_xlat62;
            int u_xlati62;
            uint u_xlatu62;
            bool u_xlatb62;
            float u_xlat63;
            int u_xlati63;
            float u_xlat64;
            int u_xlati64;
            bool u_xlatb64;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TANGENT0 : TANGENT;
                float4 in_TEXCOORD1 : TEXCOORD1;
                float4 in_COLOR0 : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 vs_INTERP0 : TEXCOORD0;
                float4 vs_INTERP5 : TEXCOORD1;
                float4 vs_INTERP6 : TEXCOORD2;
                float4 vs_INTERP7 : TEXCOORD3;
                float3 vs_INTERP8 : TEXCOORD4;
                float3 vs_INTERP9 : TEXCOORD5;
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
    output.vs_INTERP8.xyz = u_xlat0.xyz;
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
    output.vs_INTERP6 = input.in_COLOR0;
    output.vs_INTERP7 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP9.xyz = float3(u_xlat6) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_0_0[3] = float4(0.0, 0.0, 0.0, 1.0);
    u_xlat0.x = dot(input.vs_INTERP9.xyz, input.vs_INTERP9.xyz);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat0.xyz = u_xlat0.xxx * input.vs_INTERP9.xyz;
    u_xlatb57 = unity_OrthoParams.w==0.0;
    u_xlat1.xyz = (-input.vs_INTERP8.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat58 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat58 = _g_inversesqrt(u_xlat58);
    u_xlat1.xyz = float3(u_xlat58) * u_xlat1.xyz;
    u_xlat2.x = _tunity_MatrixV[0].z;
    u_xlat2.y = _tunity_MatrixV[1].z;
    u_xlat2.z = _tunity_MatrixV[2].z;
    u_xlat1.xyz = (bool(u_xlatb57)) ? u_xlat1.xyz : u_xlat2.xyz;
    u_xlat2.xyz = input.vs_INTERP8.xyz + (-_pad320.xyz);
    u_xlat3.xyz = input.vs_INTERP8.xyz + (-_pad336.xyz);
    u_xlat4.xyz = input.vs_INTERP8.xyz + (-_pad352.xyz);
    u_xlat5.xyz = input.vs_INTERP8.xyz + (-_pad368.xyz);
    u_xlat2.x = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat2.y = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat2.z = dot(u_xlat4.xyz, u_xlat4.xyz);
    u_xlat2.w = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlatb2 = _g_lessThan(u_xlat2, _pad384);
    u_xlat3.x = u_xlatb2.x ? float(1.0) : 0.0;
    u_xlat3.y = u_xlatb2.y ? float(1.0) : 0.0;
    u_xlat3.z = u_xlatb2.z ? float(1.0) : 0.0;
    u_xlat3.w = u_xlatb2.w ? float(1.0) : 0.0;
;
    u_xlat2.x = (u_xlatb2.x) ? float(-1.0) : float(-0.0);
    u_xlat2.y = (u_xlatb2.y) ? float(-1.0) : float(-0.0);
    u_xlat2.z = (u_xlatb2.z) ? float(-1.0) : float(-0.0);
    u_xlat2.xyz = u_xlat2.xyz + u_xlat3.yzw;
    u_xlat3.yzw = max(u_xlat2.xyz, float3(0.0, 0.0, 0.0));
    u_xlat57 = dot(u_xlat3, float4(4.0, 3.0, 2.0, 1.0));
    u_xlat57 = (-u_xlat57) + 4.0;
    u_xlatu57 = uint(u_xlat57);
    u_xlati57 = int(int(u_xlatu57) << 2);
    u_xlat2.xyz = input.vs_INTERP8.yyy * _pad16.xyz;
    u_xlat2.xyz = _pad0.xyz * input.vs_INTERP8.xxx + u_xlat2.xyz;
    u_xlat2.xyz = _pad32.xyz * input.vs_INTERP8.zzz + u_xlat2.xyz;
    u_xlat2.xyz = u_xlat2.xyz + _pad48.xyz;
    u_xlat57 = input.vs_INTERP8.y * _tunity_MatrixV[1].z;
    u_xlat57 = _tunity_MatrixV[0].z * input.vs_INTERP8.x + u_xlat57;
    u_xlat57 = _tunity_MatrixV[2].z * input.vs_INTERP8.z + u_xlat57;
    u_xlat57 = u_xlat57 + _tunity_MatrixV[3].z;
    u_xlat57 = (-u_xlat57) + (-_pad352.y);
    u_xlat57 = max(u_xlat57, 0.0);
    u_xlat57 = u_xlat57 * _pad976.x;
    u_xlat3.xyz = _g_texture(_Texture_t3, input.vs_INTERP0.xy, _pad64.x).xyz;
    u_xlat4 = _g_texture(_Texture_t4, input.vs_INTERP0.xy, _pad64.x);
    u_xlat4.xyz = u_xlat4.xyz + float3(-0.5, -0.5, -0.5);
    u_xlat58 = dot(u_xlat0.xyz, u_xlat4.xyz);
    u_xlat58 = u_xlat58 + 0.5;
    u_xlat3.xyz = float3(u_xlat58) * u_xlat3.xyz;
    u_xlat58 = max(u_xlat4.w, 9.99999975e-05);
    u_xlat3.xyz = u_xlat3.xyz / float3(u_xlat58);
    u_xlat4.xyz = input.vs_INTERP6.xyz * float3(0.959999979, 0.959999979, 0.959999979);
    u_xlatb58 = 0.0<_MainLightShadowParams.y;
    if(u_xlatb58){
        u_xlatb58 = _MainLightShadowParams.y==1.0;
        if(u_xlatb58){
            u_xlat5 = u_xlat2.xyxy + _pad400;
            float3 txVec0 = float3(u_xlat5.xy,u_xlat2.z);
            u_xlat6.x = _g_textureLod(_Texture_t5, txVec0, 0.0);
            float3 txVec1 = float3(u_xlat5.zw,u_xlat2.z);
            u_xlat6.y = _g_textureLod(_Texture_t5, txVec1, 0.0);
            u_xlat5 = u_xlat2.xyxy + _pad416;
            float3 txVec2 = float3(u_xlat5.xy,u_xlat2.z);
            u_xlat6.z = _g_textureLod(_Texture_t5, txVec2, 0.0);
            float3 txVec3 = float3(u_xlat5.zw,u_xlat2.z);
            u_xlat6.w = _g_textureLod(_Texture_t5, txVec3, 0.0);
            u_xlat58 = dot(u_xlat6, float4(0.25, 0.25, 0.25, 0.25));
        } else {
            u_xlatb59 = _MainLightShadowParams.y==2.0;
            if(u_xlatb59){
                u_xlat5.xy = u_xlat2.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat5.xy = floor(u_xlat5.xy);
                u_xlat43.xy = u_xlat2.xy * _pad448.zw + (-u_xlat5.xy);
                u_xlat6 = u_xlat43.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat7 = u_xlat6.xxzz * u_xlat6.xxzz;
                u_xlat6.xz = u_xlat7.yw * float2(0.0799999982, 0.0799999982);
                u_xlat7.xy = u_xlat7.xz * float2(0.5, 0.5) + (-u_xlat43.xy);
                u_xlat45.xy = (-u_xlat43.xy) + float2(1.0, 1.0);
                u_xlat8.xy = min(u_xlat43.xy, float2(0.0, 0.0));
                u_xlat8.xy = (-u_xlat8.xy) * u_xlat8.xy + u_xlat45.xy;
                u_xlat43.xy = max(u_xlat43.xy, float2(0.0, 0.0));
                u_xlat43.xy = (-u_xlat43.xy) * u_xlat43.xy + u_xlat6.yw;
                u_xlat8.xy = u_xlat8.xy + float2(1.0, 1.0);
                u_xlat43.xy = u_xlat43.xy + float2(1.0, 1.0);
                u_xlat9.xy = u_xlat7.xy * float2(0.159999996, 0.159999996);
                u_xlat7.xy = u_xlat45.xy * float2(0.159999996, 0.159999996);
                u_xlat8.xy = u_xlat8.xy * float2(0.159999996, 0.159999996);
                u_xlat10.xy = u_xlat43.xy * float2(0.159999996, 0.159999996);
                u_xlat43.xy = u_xlat6.yw * float2(0.159999996, 0.159999996);
                u_xlat9.z = u_xlat8.x;
                u_xlat9.w = u_xlat43.x;
                u_xlat7.z = u_xlat10.x;
                u_xlat7.w = u_xlat6.x;
                u_xlat11 = u_xlat7.zwxz + u_xlat9.zwxz;
                u_xlat8.z = u_xlat9.y;
                u_xlat8.w = u_xlat43.y;
                u_xlat10.z = u_xlat7.y;
                u_xlat10.w = u_xlat6.z;
                u_xlat6.xyz = u_xlat8.zyw + u_xlat10.zyw;
                u_xlat7.xyz = u_xlat7.xzw / u_xlat11.zwy;
                u_xlat7.xyz = u_xlat7.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat8.xyz = u_xlat10.zyw / u_xlat6.xyz;
                u_xlat8.xyz = u_xlat8.xyz + float3(-2.5, -0.5, 1.5);
                u_xlat7.xyz = u_xlat7.yxz * _pad448.xxx;
                u_xlat8.xyz = u_xlat8.xyz * _pad448.yyy;
                u_xlat7.w = u_xlat8.x;
                u_xlat9 = u_xlat5.xyxy * _pad448.xyxy + u_xlat7.ywxw;
                u_xlat43.xy = u_xlat5.xy * _pad448.xy + u_xlat7.zw;
                u_xlat8.w = u_xlat7.y;
                u_xlat7.yw = u_xlat8.yz;
                u_xlat10 = u_xlat5.xyxy * _pad448.xyxy + u_xlat7.xyzy;
                u_xlat8 = u_xlat5.xyxy * _pad448.xyxy + u_xlat8.wywz;
                u_xlat7 = u_xlat5.xyxy * _pad448.xyxy + u_xlat7.xwzw;
                u_xlat12 = u_xlat6.xxxy * u_xlat11.zwyz;
                u_xlat13 = u_xlat6.yyzz * u_xlat11;
                u_xlat59 = u_xlat6.z * u_xlat11.y;
                float3 txVec4 = float3(u_xlat9.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec4, 0.0);
                float3 txVec5 = float3(u_xlat9.zw,u_xlat2.z);
                u_xlat61 = _g_textureLod(_Texture_t5, txVec5, 0.0);
                u_xlat61 = u_xlat61 * u_xlat12.y;
                u_xlat60 = u_xlat12.x * u_xlat60 + u_xlat61;
                float3 txVec6 = float3(u_xlat43.xy,u_xlat2.z);
                u_xlat61 = _g_textureLod(_Texture_t5, txVec6, 0.0);
                u_xlat60 = u_xlat12.z * u_xlat61 + u_xlat60;
                float3 txVec7 = float3(u_xlat8.xy,u_xlat2.z);
                u_xlat61 = _g_textureLod(_Texture_t5, txVec7, 0.0);
                u_xlat60 = u_xlat12.w * u_xlat61 + u_xlat60;
                float3 txVec8 = float3(u_xlat10.xy,u_xlat2.z);
                u_xlat61 = _g_textureLod(_Texture_t5, txVec8, 0.0);
                u_xlat60 = u_xlat13.x * u_xlat61 + u_xlat60;
                float3 txVec9 = float3(u_xlat10.zw,u_xlat2.z);
                u_xlat61 = _g_textureLod(_Texture_t5, txVec9, 0.0);
                u_xlat60 = u_xlat13.y * u_xlat61 + u_xlat60;
                float3 txVec10 = float3(u_xlat8.zw,u_xlat2.z);
                u_xlat61 = _g_textureLod(_Texture_t5, txVec10, 0.0);
                u_xlat60 = u_xlat13.z * u_xlat61 + u_xlat60;
                float3 txVec11 = float3(u_xlat7.xy,u_xlat2.z);
                u_xlat61 = _g_textureLod(_Texture_t5, txVec11, 0.0);
                u_xlat60 = u_xlat13.w * u_xlat61 + u_xlat60;
                float3 txVec12 = float3(u_xlat7.zw,u_xlat2.z);
                u_xlat61 = _g_textureLod(_Texture_t5, txVec12, 0.0);
                u_xlat58 = u_xlat59 * u_xlat61 + u_xlat60;
            } else {
                u_xlat5.xy = u_xlat2.xy * _pad448.zw + float2(0.5, 0.5);
                u_xlat5.xy = floor(u_xlat5.xy);
                u_xlat43.xy = u_xlat2.xy * _pad448.zw + (-u_xlat5.xy);
                u_xlat6 = u_xlat43.xxyy + float4(0.5, 1.0, 0.5, 1.0);
                u_xlat7 = u_xlat6.xxzz * u_xlat6.xxzz;
                u_xlat8.yw = u_xlat7.yw * float2(0.0408160016, 0.0408160016);
                u_xlat6.xz = u_xlat7.xz * float2(0.5, 0.5) + (-u_xlat43.xy);
                u_xlat7.xy = (-u_xlat43.xy) + float2(1.0, 1.0);
                u_xlat45.xy = min(u_xlat43.xy, float2(0.0, 0.0));
                u_xlat7.xy = (-u_xlat45.xy) * u_xlat45.xy + u_xlat7.xy;
                u_xlat45.xy = max(u_xlat43.xy, float2(0.0, 0.0));
                u_xlat25.xz = (-u_xlat45.xy) * u_xlat45.xy + u_xlat6.yw;
                u_xlat7.xy = u_xlat7.xy + float2(2.0, 2.0);
                u_xlat6.yw = u_xlat25.xz + float2(2.0, 2.0);
                u_xlat9.z = u_xlat6.y * 0.0816320032;
                u_xlat10.xyz = u_xlat6.zxw * float3(0.0816320032, 0.0816320032, 0.0816320032);
                u_xlat6.xy = u_xlat7.xy * float2(0.0816320032, 0.0816320032);
                u_xlat9.x = u_xlat10.y;
                u_xlat9.yw = u_xlat43.xx * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat7.xz = u_xlat43.xx * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat7.y = u_xlat6.x;
                u_xlat7.w = u_xlat8.y;
                u_xlat9 = u_xlat7 + u_xlat9;
                u_xlat10.yw = u_xlat43.yy * float2(-0.0816320032, 0.0816320032) + float2(0.163264006, 0.0816320032);
                u_xlat8.xz = u_xlat43.yy * float2(-0.0816320032, 0.0816320032) + float2(0.0816320032, 0.163264006);
                u_xlat8.y = u_xlat6.y;
                u_xlat6 = u_xlat8 + u_xlat10;
                u_xlat7 = u_xlat7 / u_xlat9;
                u_xlat7 = u_xlat7 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat8 = u_xlat8 / u_xlat6;
                u_xlat8 = u_xlat8 + float4(-3.5, -1.5, 0.5, 2.5);
                u_xlat7 = u_xlat7.wxyz * _pad448.xxxx;
                u_xlat8 = u_xlat8.xwyz * _pad448.yyyy;
                u_xlat10.xzw = u_xlat7.yzw;
                u_xlat10.y = u_xlat8.x;
                u_xlat11 = u_xlat5.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat43.xy = u_xlat5.xy * _pad448.xy + u_xlat10.wy;
                u_xlat7.y = u_xlat10.y;
                u_xlat10.y = u_xlat8.z;
                u_xlat12 = u_xlat5.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat13.xy = u_xlat5.xy * _pad448.xy + u_xlat10.wy;
                u_xlat7.z = u_xlat10.y;
                u_xlat14 = u_xlat5.xyxy * _pad448.xyxy + u_xlat7.xyxz;
                u_xlat10.y = u_xlat8.w;
                u_xlat15 = u_xlat5.xyxy * _pad448.xyxy + u_xlat10.xyzy;
                u_xlat26.xy = u_xlat5.xy * _pad448.xy + u_xlat10.wy;
                u_xlat7.w = u_xlat10.y;
                u_xlat51.xy = u_xlat5.xy * _pad448.xy + u_xlat7.xw;
                u_xlat8.xzw = u_xlat10.xzw;
                u_xlat10 = u_xlat5.xyxy * _pad448.xyxy + u_xlat8.xyzy;
                u_xlat46.xy = u_xlat5.xy * _pad448.xy + u_xlat8.wy;
                u_xlat8.x = u_xlat7.x;
                u_xlat5.xy = u_xlat5.xy * _pad448.xy + u_xlat8.xy;
                u_xlat16 = u_xlat6.xxxx * u_xlat9;
                u_xlat17 = u_xlat6.yyyy * u_xlat9;
                u_xlat18 = u_xlat6.zzzz * u_xlat9;
                u_xlat6 = u_xlat6.wwww * u_xlat9;
                float3 txVec13 = float3(u_xlat11.xy,u_xlat2.z);
                u_xlat59 = _g_textureLod(_Texture_t5, txVec13, 0.0);
                float3 txVec14 = float3(u_xlat11.zw,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec14, 0.0);
                u_xlat60 = u_xlat60 * u_xlat16.y;
                u_xlat59 = u_xlat16.x * u_xlat59 + u_xlat60;
                float3 txVec15 = float3(u_xlat43.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec15, 0.0);
                u_xlat59 = u_xlat16.z * u_xlat60 + u_xlat59;
                float3 txVec16 = float3(u_xlat14.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec16, 0.0);
                u_xlat59 = u_xlat16.w * u_xlat60 + u_xlat59;
                float3 txVec17 = float3(u_xlat12.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec17, 0.0);
                u_xlat59 = u_xlat17.x * u_xlat60 + u_xlat59;
                float3 txVec18 = float3(u_xlat12.zw,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec18, 0.0);
                u_xlat59 = u_xlat17.y * u_xlat60 + u_xlat59;
                float3 txVec19 = float3(u_xlat13.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec19, 0.0);
                u_xlat59 = u_xlat17.z * u_xlat60 + u_xlat59;
                float3 txVec20 = float3(u_xlat14.zw,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec20, 0.0);
                u_xlat59 = u_xlat17.w * u_xlat60 + u_xlat59;
                float3 txVec21 = float3(u_xlat15.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec21, 0.0);
                u_xlat59 = u_xlat18.x * u_xlat60 + u_xlat59;
                float3 txVec22 = float3(u_xlat15.zw,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec22, 0.0);
                u_xlat59 = u_xlat18.y * u_xlat60 + u_xlat59;
                float3 txVec23 = float3(u_xlat26.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec23, 0.0);
                u_xlat59 = u_xlat18.z * u_xlat60 + u_xlat59;
                float3 txVec24 = float3(u_xlat51.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec24, 0.0);
                u_xlat59 = u_xlat18.w * u_xlat60 + u_xlat59;
                float3 txVec25 = float3(u_xlat10.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec25, 0.0);
                u_xlat59 = u_xlat6.x * u_xlat60 + u_xlat59;
                float3 txVec26 = float3(u_xlat10.zw,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec26, 0.0);
                u_xlat59 = u_xlat6.y * u_xlat60 + u_xlat59;
                float3 txVec27 = float3(u_xlat46.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec27, 0.0);
                u_xlat59 = u_xlat6.z * u_xlat60 + u_xlat59;
                float3 txVec28 = float3(u_xlat5.xy,u_xlat2.z);
                u_xlat60 = _g_textureLod(_Texture_t5, txVec28, 0.0);
                u_xlat58 = u_xlat6.w * u_xlat60 + u_xlat59;
            }
        }
    } else {
        float3 txVec29 = float3(u_xlat2.xy,u_xlat2.z);
        u_xlat58 = _g_textureLod(_Texture_t5, txVec29, 0.0);
    }
    u_xlat2.x = (-_MainLightShadowParams.x) + 1.0;
    u_xlat58 = u_xlat58 * _MainLightShadowParams.x + u_xlat2.x;
    u_xlatb2.x = 0.0>=u_xlat2.z;
    u_xlatb21 = u_xlat2.z>=1.0;
    u_xlatb2.x = u_xlatb21 || u_xlatb2.x;
    u_xlat58 = (u_xlatb2.x) ? 1.0 : u_xlat58;
    u_xlat2.xyz = input.vs_INTERP8.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat2.x = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat2.x = u_xlat2.x * _MainLightShadowParams.z + _MainLightShadowParams.w;
    u_xlat2.x = clamp(u_xlat2.x, 0.0, 1.0);
    u_xlat21 = (-u_xlat58) + 1.0;
    u_xlat58 = u_xlat2.x * u_xlat21 + u_xlat58;
    u_xlatb2.x = _pad176.y!=-1.0;
    if(u_xlatb2.x){
        u_xlat2.xy = input.vs_INTERP8.yy * _pad16.xy;
        u_xlat2.xy = _pad0.xy * input.vs_INTERP8.xx + u_xlat2.xy;
        u_xlat2.xy = _pad32.xy * input.vs_INTERP8.zz + u_xlat2.xy;
        u_xlat2.xy = u_xlat2.xy + _pad48.xy;
        u_xlat2.xy = u_xlat2.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
        u_xlat2 = _g_texture(_Texture_t6, u_xlat2.xy, _pad64.x);
        u_xlatb5.xy = _g_equal(_pad176.yyyy, float4(0.0, 1.0, 0.0, 0.0)).xy;
        u_xlat59 = (u_xlatb5.y) ? u_xlat2.w : u_xlat2.x;
        u_xlat2.xyz = (u_xlatb5.x) ? u_xlat2.xyz : float3(u_xlat59);
    } else {
        u_xlat2.x = float(1.0);
        u_xlat2.y = float(1.0);
        u_xlat2.z = float(1.0);
    }
    u_xlat2.xyz = u_xlat2.xyz * _MainLightColor.xyz;
    u_xlat59 = dot((-u_xlat1.xyz), u_xlat0.xyz);
    u_xlat59 = u_xlat59 + u_xlat59;
    u_xlat5.xyz = u_xlat0.xyz * (-float3(u_xlat59)) + (-u_xlat1.xyz);
    u_xlat59 = dot(u_xlat0.xyz, u_xlat1.xyz);
    u_xlat59 = clamp(u_xlat59, 0.0, 1.0);
    u_xlat59 = (-u_xlat59) + 1.0;
    u_xlat59 = u_xlat59 * u_xlat59;
    u_xlat59 = u_xlat59 * u_xlat59;
    u_xlat6.xyz = unity_SpecCube0_BoxMax.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat7.xyz = u_xlat6.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube0_BoxMin.xyz;
    u_xlat8.xyz = (-u_xlat7.xyz) + input.vs_INTERP8.xyz;
    u_xlat9 = unity_SpecCube0_Rotation.zzxy + unity_SpecCube0_Rotation.zzxy;
    u_xlat10.xyz = u_xlat9.zwy * unity_SpecCube0_Rotation.xyz;
    u_xlat11 = u_xlat9 * unity_SpecCube0_Rotation.xyww;
    u_xlat60 = u_xlat9.y * unity_SpecCube0_Rotation.w;
    u_xlat10.xyz = u_xlat10.zzy + u_xlat10.yxx;
    u_xlat10.xyz = (-u_xlat10.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat12.xy = u_xlat8.xy * u_xlat10.xy;
    u_xlat61 = unity_SpecCube0_Rotation.x * u_xlat9.w + (-u_xlat60);
    u_xlat62 = u_xlat61 * u_xlat8.y + u_xlat12.x;
    u_xlat11.xy = u_xlat11.wz + u_xlat11.xy;
    u_xlat63 = u_xlat8.y * u_xlat11.y;
    u_xlat13.x = u_xlat11.x * u_xlat8.z + u_xlat62;
    u_xlat60 = unity_SpecCube0_Rotation.x * u_xlat9.w + u_xlat60;
    u_xlat62 = u_xlat60 * u_xlat8.x + u_xlat12.y;
    u_xlat27.xz = unity_SpecCube0_Rotation.yx * u_xlat9.yx + (-u_xlat11.zw);
    u_xlat13.y = u_xlat27.x * u_xlat8.z + u_xlat62;
    u_xlat62 = u_xlat27.z * u_xlat8.x + u_xlat63;
    u_xlat13.z = u_xlat10.z * u_xlat8.z + u_xlat62;
    u_xlat7.xyz = u_xlat7.xyz + u_xlat13.xyz;
    u_xlat9.xyz = unity_SpecCube1_BoxMax.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat12.xyz = u_xlat9.xyz * float3(0.5, 0.5, 0.5) + unity_SpecCube1_BoxMin.xyz;
    u_xlat13.xyz = (-u_xlat12.xyz) + input.vs_INTERP8.xyz;
    u_xlat14 = unity_SpecCube1_Rotation.zzxy + unity_SpecCube1_Rotation.zzxy;
    u_xlat15.xyz = u_xlat14.zwy * unity_SpecCube1_Rotation.xyz;
    u_xlat16 = u_xlat14 * unity_SpecCube1_Rotation.xyww;
    u_xlat62 = u_xlat14.y * unity_SpecCube1_Rotation.w;
    u_xlat15.xyz = u_xlat15.zzy + u_xlat15.yxx;
    u_xlat15.xyz = (-u_xlat15.xyz) + float3(1.0, 1.0, 1.0);
    u_xlat8.xz = u_xlat13.xy * u_xlat15.xy;
    u_xlat63 = unity_SpecCube1_Rotation.x * u_xlat14.w + (-u_xlat62);
    u_xlat64 = u_xlat63 * u_xlat13.y + u_xlat8.x;
    u_xlat49.xy = u_xlat16.wz + u_xlat16.xy;
    u_xlat8.x = u_xlat13.y * u_xlat49.y;
    u_xlat17.x = u_xlat49.x * u_xlat13.z + u_xlat64;
    u_xlat62 = unity_SpecCube1_Rotation.x * u_xlat14.w + u_xlat62;
    u_xlat64 = u_xlat62 * u_xlat13.x + u_xlat8.z;
    u_xlat32.xz = unity_SpecCube1_Rotation.yx * u_xlat14.yx + (-u_xlat16.zw);
    u_xlat17.y = u_xlat32.x * u_xlat13.z + u_xlat64;
    u_xlat64 = u_xlat32.z * u_xlat13.x + u_xlat8.x;
    u_xlat17.z = u_xlat15.z * u_xlat13.z + u_xlat64;
    u_xlat12.xyz = u_xlat12.xyz + u_xlat17.xyz;
    u_xlat6.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat25.x = dot(u_xlat9.xyz, u_xlat9.xyz);
    u_xlat6.x = (-u_xlat25.x) + u_xlat6.x;
    u_xlatb25 = 0.0<unity_SpecCube1_BoxMin.w;
    u_xlatb44 = unity_SpecCube1_BoxMin.w==0.0;
    u_xlatb64 = u_xlat6.x<-9.99999975e-05;
    u_xlatb64 = u_xlatb44 && u_xlatb64;
    u_xlatb25 = u_xlatb25 || u_xlatb64;
    u_xlatb64 = unity_SpecCube1_BoxMin.w<0.0;
    u_xlatb6 = 9.99999975e-05<u_xlat6.x;
    u_xlatb6 = u_xlatb6 && u_xlatb44;
    u_xlatb6 = u_xlatb6 || u_xlatb64;
    u_xlat9.xyz = u_xlat7.xyz + (-unity_SpecCube0_BoxMin.xyz);
    u_xlat14.xyz = (-u_xlat7.xyz) + unity_SpecCube0_BoxMax.xyz;
    u_xlat9.xyz = min(u_xlat9.xyz, u_xlat14.xyz);
    u_xlat9.xyz = u_xlat9.xyz / unity_SpecCube0_BoxMax.www;
    u_xlat44 = min(u_xlat9.z, u_xlat9.y);
    u_xlat44 = min(u_xlat44, u_xlat9.x);
    u_xlat44 = clamp(u_xlat44, 0.0, 1.0);
    u_xlat9.xyz = u_xlat12.xyz + (-unity_SpecCube1_BoxMin.xyz);
    u_xlat14.xyz = (-u_xlat12.xyz) + unity_SpecCube1_BoxMax.xyz;
    u_xlat9.xyz = min(u_xlat9.xyz, u_xlat14.xyz);
    u_xlat9.xyz = u_xlat9.xyz / unity_SpecCube1_BoxMax.www;
    u_xlat64 = min(u_xlat9.z, u_xlat9.y);
    u_xlat64 = min(u_xlat64, u_xlat9.x);
    u_xlat64 = clamp(u_xlat64, 0.0, 1.0);
    u_xlat8.x = (-u_xlat64) + 1.0;
    u_xlat8.x = min(u_xlat44, u_xlat8.x);
    u_xlat6.x = (u_xlatb6) ? u_xlat8.x : u_xlat44;
    u_xlat44 = (-u_xlat44) + 1.0;
    u_xlat44 = min(u_xlat44, u_xlat64);
    u_xlat6.y = (u_xlatb25) ? u_xlat44 : u_xlat64;
    u_xlat44 = u_xlat6.y + u_xlat6.x;
    u_xlat64 = max(u_xlat44, 1.0);
    u_xlat6.xy = u_xlat6.xy / float2(u_xlat64);
    u_xlatb64 = 0.00999999978<u_xlat6.x;
    if(u_xlatb64){
        u_xlat8.xz = u_xlat5.xy * u_xlat10.xy;
        u_xlat61 = u_xlat61 * u_xlat5.y + u_xlat8.x;
        u_xlat64 = u_xlat5.y * u_xlat11.y;
        u_xlat9.x = u_xlat11.x * u_xlat5.z + u_xlat61;
        u_xlat60 = u_xlat60 * u_xlat5.x + u_xlat8.z;
        u_xlat9.y = u_xlat27.x * u_xlat5.z + u_xlat60;
        u_xlat60 = u_xlat27.z * u_xlat5.x + u_xlat64;
        u_xlat9.z = u_xlat10.z * u_xlat5.z + u_xlat60;
        u_xlatb60 = 0.0<unity_SpecCube0_ProbePosition.w;
        u_xlatb8.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat9.xyzx).xyz;
        u_xlat8.x = (u_xlatb8.x) ? unity_SpecCube0_BoxMax.x : unity_SpecCube0_BoxMin.x;
        u_xlat8.y = (u_xlatb8.y) ? unity_SpecCube0_BoxMax.y : unity_SpecCube0_BoxMin.y;
        u_xlat8.z = (u_xlatb8.z) ? unity_SpecCube0_BoxMax.z : unity_SpecCube0_BoxMin.z;
        u_xlat8.xyz = (-u_xlat7.xyz) + u_xlat8.xyz;
        u_xlat8.xyz = u_xlat8.xyz / u_xlat9.xyz;
        u_xlat61 = min(u_xlat8.y, u_xlat8.x);
        u_xlat61 = min(u_xlat8.z, u_xlat61);
        u_xlat7.xyz = u_xlat7.xyz + (-unity_SpecCube0_ProbePosition.xyz);
        u_xlat7.xyz = u_xlat9.xyz * float3(u_xlat61) + u_xlat7.xyz;
        u_xlat7.xyz = (bool(u_xlatb60)) ? u_xlat7.xyz : u_xlat9.xyz;
        u_xlat8.xyz = unity_SpecCube0_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat9.xyz = u_xlat8.yzx * (-unity_SpecCube0_Rotation.xyz);
        u_xlat10.xyz = u_xlat8.xyz * unity_SpecCube0_Rotation.www;
        u_xlat9.xyz = u_xlat9.zzy + u_xlat9.yxx;
        u_xlat9.xyz = (-u_xlat9.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat27.xz = u_xlat7.xy * u_xlat9.xy;
        u_xlat60 = (-unity_SpecCube0_Rotation.x) * u_xlat8.z + (-u_xlat10.x);
        u_xlat60 = u_xlat60 * u_xlat7.y + u_xlat27.x;
        u_xlat9.xy = (-unity_SpecCube0_Rotation.xy) * u_xlat8.xx + u_xlat10.zy;
        u_xlat61 = u_xlat7.y * u_xlat9.y;
        u_xlat14.x = u_xlat9.x * u_xlat7.z + u_xlat60;
        u_xlat60 = (-unity_SpecCube0_Rotation.x) * u_xlat8.z + u_xlat10.x;
        u_xlat60 = u_xlat60 * u_xlat7.x + u_xlat27.z;
        u_xlat26.xz = (-unity_SpecCube0_Rotation.yx) * u_xlat8.xx + (-u_xlat10.yz);
        u_xlat14.y = u_xlat26.x * u_xlat7.z + u_xlat60;
        u_xlat60 = u_xlat26.z * u_xlat7.x + u_xlat61;
        u_xlat14.z = u_xlat9.z * u_xlat7.z + u_xlat60;
        u_xlat7 = _g_textureLod(_Texture_t1, u_xlat14.xyz, 6.0);
        u_xlat60 = u_xlat7.w + -1.0;
        u_xlat60 = unity_SpecCube0_HDR.w * u_xlat60 + 1.0;
        u_xlat60 = max(u_xlat60, 0.0);
        u_xlat60 = log2(u_xlat60);
        u_xlat60 = u_xlat60 * unity_SpecCube0_HDR.y;
        u_xlat60 = exp2(u_xlat60);
        u_xlat60 = u_xlat60 * unity_SpecCube0_HDR.x;
        u_xlat7.xyz = u_xlat7.xyz * float3(u_xlat60);
        u_xlat7.xyz = u_xlat6.xxx * u_xlat7.xyz;
    } else {
        u_xlat7.x = float(0.0);
        u_xlat7.y = float(0.0);
        u_xlat7.z = float(0.0);
    }
    u_xlatb60 = 0.00999999978<u_xlat6.y;
    if(u_xlatb60){
        u_xlat8.xy = u_xlat5.xy * u_xlat15.xy;
        u_xlat60 = u_xlat63 * u_xlat5.y + u_xlat8.x;
        u_xlat61 = u_xlat5.y * u_xlat49.y;
        u_xlat9.x = u_xlat49.x * u_xlat5.z + u_xlat60;
        u_xlat60 = u_xlat62 * u_xlat5.x + u_xlat8.y;
        u_xlat9.y = u_xlat32.x * u_xlat5.z + u_xlat60;
        u_xlat60 = u_xlat32.z * u_xlat5.x + u_xlat61;
        u_xlat9.z = u_xlat15.z * u_xlat5.z + u_xlat60;
        u_xlatb60 = 0.0<unity_SpecCube1_ProbePosition.w;
        u_xlatb8.xyz = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), u_xlat9.xyzx).xyz;
        u_xlat8.x = (u_xlatb8.x) ? unity_SpecCube1_BoxMax.x : unity_SpecCube1_BoxMin.x;
        u_xlat8.y = (u_xlatb8.y) ? unity_SpecCube1_BoxMax.y : unity_SpecCube1_BoxMin.y;
        u_xlat8.z = (u_xlatb8.z) ? unity_SpecCube1_BoxMax.z : unity_SpecCube1_BoxMin.z;
        u_xlat8.xyz = (-u_xlat12.xyz) + u_xlat8.xyz;
        u_xlat8.xyz = u_xlat8.xyz / u_xlat9.xyz;
        u_xlat61 = min(u_xlat8.y, u_xlat8.x);
        u_xlat61 = min(u_xlat8.z, u_xlat61);
        u_xlat8.xyz = u_xlat12.xyz + (-unity_SpecCube1_ProbePosition.xyz);
        u_xlat8.xyz = u_xlat9.xyz * float3(u_xlat61) + u_xlat8.xyz;
        u_xlat8.xyz = (bool(u_xlatb60)) ? u_xlat8.xyz : u_xlat9.xyz;
        u_xlat9.xyz = unity_SpecCube1_Rotation.zxy * float3(-2.0, -2.0, -2.0);
        u_xlat10.xyz = u_xlat9.yzx * (-unity_SpecCube1_Rotation.xyz);
        u_xlat11.xyz = u_xlat9.xyz * unity_SpecCube1_Rotation.www;
        u_xlat10.xyz = u_xlat10.zzy + u_xlat10.yxx;
        u_xlat10.xyz = (-u_xlat10.xyz) + float3(1.0, 1.0, 1.0);
        u_xlat6.xw = u_xlat8.xy * u_xlat10.xy;
        u_xlat60 = (-unity_SpecCube1_Rotation.x) * u_xlat9.z + (-u_xlat11.x);
        u_xlat60 = u_xlat60 * u_xlat8.y + u_xlat6.x;
        u_xlat28.xz = (-unity_SpecCube1_Rotation.xy) * u_xlat9.xx + u_xlat11.zy;
        u_xlat61 = u_xlat8.y * u_xlat28.z;
        u_xlat12.x = u_xlat28.x * u_xlat8.z + u_xlat60;
        u_xlat60 = (-unity_SpecCube1_Rotation.x) * u_xlat9.z + u_xlat11.x;
        u_xlat60 = u_xlat60 * u_xlat8.x + u_xlat6.w;
        u_xlat6.xw = (-unity_SpecCube1_Rotation.yx) * u_xlat9.xx + (-u_xlat11.yz);
        u_xlat12.y = u_xlat6.x * u_xlat8.z + u_xlat60;
        u_xlat60 = u_xlat6.w * u_xlat8.x + u_xlat61;
        u_xlat12.z = u_xlat10.z * u_xlat8.z + u_xlat60;
        u_xlat8 = _g_textureLod(_Texture_t2, u_xlat12.xyz, 6.0);
        u_xlat60 = u_xlat8.w + -1.0;
        u_xlat60 = unity_SpecCube1_HDR.w * u_xlat60 + 1.0;
        u_xlat60 = max(u_xlat60, 0.0);
        u_xlat60 = log2(u_xlat60);
        u_xlat60 = u_xlat60 * unity_SpecCube1_HDR.y;
        u_xlat60 = exp2(u_xlat60);
        u_xlat60 = u_xlat60 * unity_SpecCube1_HDR.x;
        u_xlat8.xyz = u_xlat8.xyz * float3(u_xlat60);
        u_xlat7.xyz = u_xlat6.yyy * u_xlat8.xyz + u_xlat7.xyz;
    }
    u_xlatb60 = u_xlat44<0.99000001;
    if(u_xlatb60){
        u_xlat5 = _g_textureLod(_Texture_t0, u_xlat5.xyz, 6.0);
        u_xlat60 = (-u_xlat44) + 1.0;
        u_xlat61 = u_xlat5.w + -1.0;
        u_xlat61 = _GlossyEnvironmentCubeMap_HDR.w * u_xlat61 + 1.0;
        u_xlat61 = max(u_xlat61, 0.0);
        u_xlat61 = log2(u_xlat61);
        u_xlat61 = u_xlat61 * _GlossyEnvironmentCubeMap_HDR.y;
        u_xlat61 = exp2(u_xlat61);
        u_xlat61 = u_xlat61 * _GlossyEnvironmentCubeMap_HDR.x;
        u_xlat5.xyz = u_xlat5.xyz * float3(u_xlat61);
        u_xlat7.xyz = float3(u_xlat60) * u_xlat5.xyz + u_xlat7.xyz;
    }
    u_xlat59 = u_xlat59 * 2.23517418e-08 + 0.0399999991;
    u_xlat59 = u_xlat59 * 0.5;
    u_xlat5.xyz = float3(u_xlat59) * u_xlat7.xyz;
    u_xlat3.xyz = u_xlat3.xyz * u_xlat4.xyz + u_xlat5.xyz;
    u_xlati59 = int(uint(uint(_g_floatBitsToUint(_MainLightLayerMask)) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
    u_xlat58 = u_xlat58 * unity_LightData.z;
    u_xlat60 = dot(u_xlat0.xyz, _MainLightPosition.xyz);
    u_xlat60 = clamp(u_xlat60, 0.0, 1.0);
    u_xlat58 = u_xlat58 * u_xlat60;
    u_xlat2.xyz = float3(u_xlat58) * u_xlat2.xyz;
    u_xlat5.xyz = u_xlat1.xyz + _MainLightPosition.xyz;
    u_xlat58 = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlat58 = max(u_xlat58, 1.17549435e-38);
    u_xlat58 = _g_inversesqrt(u_xlat58);
    u_xlat5.xyz = float3(u_xlat58) * u_xlat5.xyz;
    u_xlat58 = dot(_MainLightPosition.xyz, u_xlat5.xyz);
    u_xlat58 = clamp(u_xlat58, 0.0, 1.0);
    u_xlat58 = u_xlat58 * u_xlat58;
    u_xlat58 = max(u_xlat58, 0.100000001);
    u_xlat58 = u_xlat58 * 6.00012016;
    u_xlat58 = float(1.0) / u_xlat58;
    u_xlat5.xyz = float3(u_xlat58) * float3(0.0399999991, 0.0399999991, 0.0399999991) + u_xlat4.xyz;
    u_xlat2.xyz = u_xlat2.xyz * u_xlat5.xyz;
    u_xlat2.xyz = (int(u_xlati59) != 0) ? u_xlat2.xyz : float3(0.0, 0.0, 0.0);
    u_xlat58 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu58 =  uint(int(u_xlat58));
    u_xlatb5.xy = _g_equal(_pad176.zzzz, float4(0.0, 1.0, 0.0, 0.0)).xy;
    u_xlat6.x = float(0.0);
    u_xlat6.y = float(0.0);
    u_xlat6.z = float(0.0);
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<u_xlatu58 ; u_xlatu_loop_1++)
    {
        u_xlatu60 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati61 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat60 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati61]);
        u_xlatu60 =  uint(int(u_xlat60));
        u_xlat7.xyz = (-input.vs_INTERP8.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat61 = dot(u_xlat7.xyz, u_xlat7.xyz);
        u_xlat61 = max(u_xlat61, 6.10351562e-05);
        u_xlat43.x = _g_inversesqrt(u_xlat61);
        u_xlat8.xyz = u_xlat43.xxx * u_xlat7.xyz;
        u_xlat62 = float(1.0) / float(u_xlat61);
        u_xlat61 = u_xlat61 * _AdditionalLightsAttenuation.x;
        u_xlat61 = (-u_xlat61) * u_xlat61 + 1.0;
        u_xlat61 = max(u_xlat61, 0.0);
        u_xlat61 = u_xlat61 * u_xlat61;
        u_xlat61 = u_xlat61 * u_xlat62;
        u_xlat62 = dot(_AdditionalLightsSpotDir.xyz, u_xlat8.xyz);
        u_xlat62 = u_xlat62 * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat62 = clamp(u_xlat62, 0.0, 1.0);
        u_xlat62 = u_xlat62 * u_xlat62;
        u_xlat61 = u_xlat61 * u_xlat62;
        u_xlatu62 = uint(u_xlatu60 >> 5u);
        u_xlati63 = int(1 << int(u_xlatu60));
        u_xlati62 = int(uint(uint(u_xlati63) & uint(_g_floatBitsToUint(_pad64.x))));
        if(u_xlati62 != 0) {
            u_xlati62 = int(_pad20672.x);
            u_xlati63 = (u_xlati62 != 0) ? 0 : 1;
            u_xlati64 = int(int(u_xlatu60) << 2);
            if(u_xlati63 != 0) {
                u_xlat9.xyz = input.vs_INTERP8.yyy * _pad208.xyw;
                u_xlat9.xyz = _pad192.xyw * input.vs_INTERP8.xxx + u_xlat9.xyz;
                u_xlat9.xyz = _pad224.xyw * input.vs_INTERP8.zzz + u_xlat9.xyz;
                u_xlat9.xyz = u_xlat9.xyz + _pad240.xyw;
                u_xlat9.xy = u_xlat9.xy / u_xlat9.zz;
                u_xlat9.xy = u_xlat9.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                u_xlat9.xy = clamp(u_xlat9.xy, 0.0, 1.0);
                u_xlat9.xy = _pad16576.xy * u_xlat9.xy + _pad16576.zw;
            } else {
                u_xlatb62 = u_xlati62==1;
                u_xlati62 = u_xlatb62 ? 1 : int(0);
                if(u_xlati62 != 0) {
                    u_xlat47.xy = input.vs_INTERP8.yy * _pad208.xy;
                    u_xlat47.xy = _pad192.xy * input.vs_INTERP8.xx + u_xlat47.xy;
                    u_xlat47.xy = _pad224.xy * input.vs_INTERP8.zz + u_xlat47.xy;
                    u_xlat47.xy = u_xlat47.xy + _pad240.xy;
                    u_xlat47.xy = u_xlat47.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat47.xy = _g_fract(u_xlat47.xy);
                    u_xlat9.xy = _pad16576.xy * u_xlat47.xy + _pad16576.zw;
                } else {
                    u_xlat10 = input.vs_INTERP8.yyyy * _pad208;
                    u_xlat10 = _pad192 * input.vs_INTERP8.xxxx + u_xlat10;
                    u_xlat10 = _pad224 * input.vs_INTERP8.zzzz + u_xlat10;
                    u_xlat10 = u_xlat10 + _pad240;
                    u_xlat10.xyz = u_xlat10.xyz / u_xlat10.www;
                    u_xlat62 = dot(u_xlat10.xyz, u_xlat10.xyz);
                    u_xlat62 = _g_inversesqrt(u_xlat62);
                    u_xlat10.xyz = float3(u_xlat62) * u_xlat10.xyz;
                    u_xlat62 = dot(abs(u_xlat10.xyz), float3(1.0, 1.0, 1.0));
                    u_xlat62 = max(u_xlat62, 9.99999997e-07);
                    u_xlat62 = float(1.0) / float(u_xlat62);
                    u_xlat11.xyz = float3(u_xlat62) * u_xlat10.zxy;
                    u_xlat11.x = (-u_xlat11.x);
                    u_xlat11.x = clamp(u_xlat11.x, 0.0, 1.0);
                    u_xlatb47.xy = _g_greaterThanEqual(u_xlat11.yzyz, float4(0.0, 0.0, 0.0, 0.0)).xy;
                    u_xlat47.x = (u_xlatb47.x) ? u_xlat11.x : (-u_xlat11.x);
                    u_xlat47.y = (u_xlatb47.y) ? u_xlat11.x : (-u_xlat11.x);
                    u_xlat47.xy = u_xlat10.xy * float2(u_xlat62) + u_xlat47.xy;
                    u_xlat47.xy = u_xlat47.xy * float2(0.5, 0.5) + float2(0.5, 0.5);
                    u_xlat47.xy = clamp(u_xlat47.xy, 0.0, 1.0);
                    u_xlat9.xy = _pad16576.xy * u_xlat47.xy + _pad16576.zw;
                }
            }
            u_xlat9 = _g_textureLod(_Texture_t7, u_xlat9.xy, 0.0);
            u_xlat62 = (u_xlatb5.y) ? u_xlat9.w : u_xlat9.x;
            u_xlat9.xyz = (u_xlatb5.x) ? u_xlat9.xyz : float3(u_xlat62);
        } else {
            u_xlat9.x = float(1.0);
            u_xlat9.y = float(1.0);
            u_xlat9.z = float(1.0);
        }
        u_xlat9.xyz = u_xlat9.xyz * _AdditionalLightsColor.xyz;
        u_xlati60 = int(uint(uint(_g_floatBitsToUint(unity_RenderingLayer.x)) & uint(_g_floatBitsToUint(_AdditionalLightsLayerMasks))));
        u_xlat62 = dot(u_xlat0.xyz, u_xlat8.xyz);
        u_xlat62 = clamp(u_xlat62, 0.0, 1.0);
        u_xlat61 = u_xlat61 * u_xlat62;
        u_xlat9.xyz = float3(u_xlat61) * u_xlat9.xyz;
        u_xlat7.xyz = u_xlat7.xyz * u_xlat43.xxx + u_xlat1.xyz;
        u_xlat61 = dot(u_xlat7.xyz, u_xlat7.xyz);
        u_xlat61 = max(u_xlat61, 1.17549435e-38);
        u_xlat61 = _g_inversesqrt(u_xlat61);
        u_xlat7.xyz = float3(u_xlat61) * u_xlat7.xyz;
        u_xlat61 = dot(u_xlat8.xyz, u_xlat7.xyz);
        u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
        u_xlat61 = u_xlat61 * u_xlat61;
        u_xlat61 = max(u_xlat61, 0.100000001);
        u_xlat61 = u_xlat61 * 6.00012016;
        u_xlat61 = float(1.0) / u_xlat61;
        u_xlat7.xyz = float3(u_xlat61) * float3(0.0399999991, 0.0399999991, 0.0399999991) + u_xlat4.xyz;
        u_xlat7.xyz = u_xlat7.xyz * u_xlat9.xyz + u_xlat6.xyz;
        u_xlat6.xyz = (int(u_xlati60) != 0) ? u_xlat7.xyz : u_xlat6.xyz;
    }
    u_xlat0.xyz = u_xlat2.xyz + u_xlat3.xyz;
    u_xlat0.xyz = u_xlat6.xyz + u_xlat0.xyz;
    u_xlat57 = u_xlat57 * (-u_xlat57);
    u_xlat57 = exp2(u_xlat57);
    u_xlat1.x = (-u_xlat57) + 1.0;
    u_xlat1.xyz = u_xlat1.xxx * _pad992.xyz;
    __SV_Target0.xyz = u_xlat0.xyz * float3(u_xlat57) + u_xlat1.xyz;
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
