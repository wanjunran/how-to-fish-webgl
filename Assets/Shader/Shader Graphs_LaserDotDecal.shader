Shader "Shader Graphs/LaserDotDecal"
{
    Properties
    {

	_Center ("Center", Vector) = (0.5,0.5,0,0)
	_SmoothStep ("SmoothStep", Vector) = (0,0,0,0)
	[HDR] _Innercolor ("Innercolor", Vector) = (1,1,1,1)
	[HDR] _OuterColor ("OuterColor", Vector) = (1,0,0,1)
	[HideInInspector] _DrawOrder ("Draw Order", Range(-50, 50)) = 0
	[Enum(Default, 0, Depth Bias, 0, View Bias, 1)] [HideInInspector] _DecalMeshBiasType ("DecalMesh BiasType", Float) = 0
	[HideInInspector] _DecalMeshDepthBias ("DecalMesh DepthBias", Float) = 0
	[HideInInspector] _DecalMeshViewBias ("DecalMesh ViewBias", Float) = 0
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

            TEXTURECUBE(_Texture_t0);
            SAMPLER(sampler__Texture_t0);
            TEXTURE2D(_Texture_t1);
            SAMPLER(sampler__Texture_t1);
            TEXTURE3D(_Texture_t6);
            SAMPLER(sampler__Texture_t6);
            TEXTURE3D(_Texture_t7);
            SAMPLER(sampler__Texture_t7);
            TEXTURE3D(_Texture_t8);
            SAMPLER(sampler__Texture_t8);
            TEXTURE3D(_Texture_t9);
            SAMPLER(sampler__Texture_t9);
            TEXTURE3D(_Texture_t10);
            SAMPLER(sampler__Texture_t10);
            TEXTURE3D(_Texture_t11);
            SAMPLER(sampler__Texture_t11);
            TEXTURE2D(_Texture_t12);
            SAMPLER(sampler__Texture_t12);
            TEXTURE2D(_Texture_t13);
            SAMPLER(sampler__Texture_t13);

            float4 _pad0;
            float4 _pad16;
            float4 _pad32;
            float4 _pad48;
            float4 _pad976;
            float4 _pad992;
            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float2 _GlobalMipBias;
            float4 _MainLightPosition;
            float4 _MainLightColor;
            float4 _AdditionalLightsCount;
            float _EnableProbeVolumes;
            float3 _WorldSpaceCameraPos;
            float4 _ProjectionParams;
            float4 _ZBufferParams;
            float4 unity_OrthoParams;
            float4 _ScaleBiasRt;
            float4 _RTHandleScale;
            float4x4 unity_MatrixV;
            float4x4 unity_MatrixInvV;
            float4x4 unity_MatrixInvVP;
            float4 _ScreenSize;
            float4 _CameraDepthTexture_TexelSize;
            float4x4 _NormalReconstructionMatrix;
            float4 _AdditionalLightsPosition;
            float4 _AdditionalLightsColor;
            float4 _AdditionalLightsAttenuation;
            float4 _AdditionalLightsSpotDir;
            float4 unity_RenderingLayer;
            float4 unity_LightData;
            float4 unity_LightIndices;
            float4 unity_SpecCube0_HDR;
            float4 unity_SHAr;
            float4 unity_SHAg;
            float4 unity_SHAb;
            float4 unity_SHBr;
            float4 unity_SHBg;
            float4 unity_SHBb;
            float4 unity_SHC;
            float4 _MainLightShadowParams;
            float4 _Offset_LayerCount;
            float4 _MinLoadedCellInEntries_IndirectionEntryDim;
            float4 _MaxLoadedCellInEntries_RcpIndirectionEntryDim;
            float4 _PoolDim_MinBrickSize;
            float4 _RcpPoolDim_XY;
            float4 _MinEntryPos_Noise;
            float4 _EntryCount_X_XY_LeakReduction;
            float4 _Biases_NormalizationClamp;
            float4 _FrameIndex_Weights;
            float4 _ProbeVolumeLayerMask;
            float4x4 _NormalToWorld;
            float _DecalLayerMaskFromDecal;
            float2 _Center;
            float2 _SmoothStep;
            float4 _OuterColor;
            float4 _Innercolor;

            float3 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            float4 ImmCB_0_0_0[4];
            int4 u_xlati0;
            uint u_xlatu0;
            bool u_xlatb0;
            uint4 u_xlatu1;
            float4 u_xlat2;
            bool u_xlatb2;
            float4 u_xlat3;
            float4 u_xlat4;
            float4 u_xlat5;
            uint4 u_xlatu5;
            uint4 u_xlatu6;
            float4 u_xlat7;
            int3 u_xlati7;
            uint3 u_xlatu7;
            bool3 u_xlatb7;
            float4 u_xlat8;
            int4 u_xlati8;
            uint4 u_xlatu8;
            float4 u_xlat9;
            uint3 u_xlatu9;
            float4 u_xlat10;
            uint3 u_xlatu10;
            float4 u_xlat11;
            uint4 u_xlatu11;
            bool2 u_xlatb11;
            float4 u_xlat12;
            float4 u_xlat13;
            int3 u_xlati13;
            uint4 u_xlatu13;
            float4 u_xlat14;
            int4 u_xlati14;
            uint4 u_xlatu14;
            bool2 u_xlatb14;
            float4 u_xlat15;
            uint4 u_xlatu15;
            float4 u_xlat16;
            float4 u_xlat17;
            float4 u_xlat18;
            float3 u_xlat19;
            float3 u_xlat20;
            float u_xlat22;
            bool u_xlatb22;
            float3 u_xlat25;
            float3 u_xlat26;
            uint2 u_xlatu26;
            uint3 u_xlatu28;
            float3 u_xlat31;
            uint3 u_xlatu31;
            float2 u_xlat40;
            int u_xlati40;
            uint u_xlatu40;
            bool u_xlatb40;
            float2 u_xlat41;
            float u_xlat60;
            int u_xlati60;
            uint u_xlatu60;
            bool u_xlatb60;
            float u_xlat61;
            int u_xlati61;
            uint u_xlatu61;
            bool u_xlatb61;
            float u_xlat62;
            uint u_xlatu62;
            bool u_xlatb62;
            float u_xlat63;
            int u_xlati63;
            uint u_xlatu63;
            bool u_xlatb63;
            float u_xlat64;
            int u_xlati64;
            float u_xlat65;
            uint u_xlatu65;
            float u_xlat66;
            float u_xlat67;
            uint u_xlatu67;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 vs_INTERP2 : TEXCOORD0;
                float4 vs_INTERP4 : TEXCOORD1;
                float4 vs_INTERP5 : TEXCOORD2;
                float3 vs_INTERP6 : TEXCOORD3;
                float3 vs_INTERP7 : TEXCOORD4;
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
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    output.vs_INTERP2.xyz = float3(0.0, 0.0, 0.0);
    u_xlat1.xyz = u_xlat0.yyy * _pad16.xyz;
    u_xlat1.xyz = _pad0.xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat1.xyz = _pad32.xyz * u_xlat0.zzz + u_xlat1.xyz;
    output.vs_INTERP6.xyz = u_xlat0.xyz;
    output.vs_INTERP4.xyz = u_xlat1.xyz + _pad48.xyz;
    output.vs_INTERP4.w = 0.0;
    output.vs_INTERP5 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[0].xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[1].xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _tunity_WorldToObject[2].xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP7.xyz = float3(u_xlat6) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _t_NormalReconstructionMatrix = transpose(_NormalReconstructionMatrix);
            float4x4 _t_NormalToWorld = transpose(_NormalToWorld);
            float4x4 _tunity_MatrixInvV = transpose(unity_MatrixInvV);
            float4x4 _tunity_MatrixInvVP = transpose(unity_MatrixInvVP);
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);
            float4x4 _tunity_WorldToObject = transpose(unity_WorldToObject);


	ImmCB_0_0_0[0] = float4(1.0, 0.0, 0.0, 0.0);
	ImmCB_0_0_0[1] = float4(0.0, 1.0, 0.0, 0.0);
	ImmCB_0_0_0[2] = float4(0.0, 0.0, 1.0, 0.0);
	ImmCB_0_0_0[3] = float4(0.0, 0.0, 0.0, 1.0);
float4 hlslcc_FragCoord = float4(input.positionCS.xyz, 1.0/input.positionCS.w);
    u_xlat0.x = _ScaleBiasRt.y * _ScreenSize.y;
    u_xlat0.x = hlslcc_FragCoord.y * _ScaleBiasRt.x + u_xlat0.x;
    u_xlat0.x = (-u_xlat0.x) + _ScreenSize.y;
    u_xlatu1.x = uint(hlslcc_FragCoord.x);
    u_xlatu1.y = uint(u_xlat0.x);
    u_xlatu1.z = uint(uint(0u));
    u_xlatu1.w = uint(uint(0u));
    u_xlati0 = _g_floatBitsToInt(_g_texelFetch(_Texture_t12, int2(u_xlatu1.xy), int(u_xlatu1.w)));
    u_xlatu0 = uint(uint(u_xlati0.x) & uint(_g_floatBitsToUint(_DecalLayerMaskFromDecal)));
    u_xlat0.x = float(u_xlatu0);
    u_xlat0.x = u_xlat0.x + -0.100000001;
    u_xlatb0 = u_xlat0.x<0.0;
    if(u_xlatb0){discard;}
    u_xlat0 = _g_texelFetch(_Texture_t13, int2(u_xlatu1.xy), int(u_xlatu1.w));
    u_xlat20.xy = hlslcc_FragCoord.xy * _ScreenSize.zw;
    u_xlat1.xy = u_xlat20.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat1.z = 1.0;
    u_xlat2.xyz = u_xlat1.xyz * _ProjectionParams.zzz;
    u_xlat3.xyz = u_xlat2.yyy * _t_NormalReconstructionMatrix[1].yzx;
    u_xlat2.xyw = _t_NormalReconstructionMatrix[0].yzx * u_xlat2.xxx + u_xlat3.xyz;
    u_xlat2.xyw = _t_NormalReconstructionMatrix[2].yzx * u_xlat2.zzz + u_xlat2.xyw;
    u_xlat2.xyz = _t_NormalReconstructionMatrix[3].yzx * u_xlat2.zzz + u_xlat2.xyw;
    u_xlat41.xy = (-_CameraDepthTexture_TexelSize.xy) * float2(0.5, 0.5) + float2(1.0, 1.0);
    u_xlat20.xy = min(u_xlat20.xy, u_xlat41.xy);
    u_xlat20.xy = u_xlat20.xy * _RTHandleScale.xy;
    u_xlat3 = _g_texture(_Texture_t13, u_xlat20.xy, _GlobalMipBias.x);
    u_xlat20.x = _ZBufferParams.x * u_xlat3.x + _ZBufferParams.y;
    u_xlat20.x = float(1.0) / u_xlat20.x;
    u_xlat20.xyz = u_xlat20.xxx * u_xlat2.xyz;
    u_xlat2.xyz = dFdy(u_xlat20.yzx);
    u_xlat20.xyz = dFdx(u_xlat20.xyz);
    u_xlat3.xyz = u_xlat20.xyz * u_xlat2.xyz;
    u_xlat20.xyz = u_xlat2.zxy * u_xlat20.yzx + (-u_xlat3.xyz);
    u_xlat41.x = dot(u_xlat20.xyz, u_xlat20.xyz);
    u_xlat41.x = max(u_xlat41.x, 1.17549435e-38);
    u_xlat41.x = _g_inversesqrt(u_xlat41.x);
    u_xlat20.xyz = u_xlat20.xyz * u_xlat41.xxx;
    u_xlat2 = (-u_xlat1.yyyy) * _tunity_MatrixInvVP[1];
    u_xlat1 = _tunity_MatrixInvVP[0] * u_xlat1.xxxx + u_xlat2;
    u_xlat1 = _tunity_MatrixInvVP[2] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = u_xlat1 + _tunity_MatrixInvVP[3];
    u_xlat1.xyz = u_xlat1.xyz / u_xlat1.www;
    u_xlat2.xyz = u_xlat1.yyy * _tunity_WorldToObject[1].xyz;
    u_xlat2.xyz = _tunity_WorldToObject[0].xyz * u_xlat1.xxx + u_xlat2.xyz;
    u_xlat2.xyz = _tunity_WorldToObject[2].xyz * u_xlat1.zzz + u_xlat2.xyz;
    u_xlat2.xyz = u_xlat2.xyz + _tunity_WorldToObject[3].xyz;
    u_xlat3.xyz = u_xlat2.xyz * float3(1.0, -1.0, 1.0);
    u_xlat0.x = max(abs(u_xlat3.y), abs(u_xlat3.x));
    u_xlat0.x = max(abs(u_xlat3.z), u_xlat0.x);
    u_xlat0.x = (-u_xlat0.x) + 0.5;
    u_xlatb0 = u_xlat0.x<0.0;
    if(u_xlatb0){discard;}
    u_xlatb0 = unity_OrthoParams.w==0.0;
    u_xlat3.xyz = (-u_xlat1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat61 = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat61 = _g_inversesqrt(u_xlat61);
    u_xlat3.xyz = float3(u_xlat61) * u_xlat3.xyz;
    u_xlat4.x = _tunity_MatrixV[0].z;
    u_xlat4.y = _tunity_MatrixV[1].z;
    u_xlat4.z = _tunity_MatrixV[2].z;
    u_xlat3.xyz = (bool(u_xlatb0)) ? u_xlat3.xyz : u_xlat4.xyz;
    u_xlat0.x = _t_NormalToWorld[3].x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat2.xy = u_xlat2.xz + (-_Center.xy);
    u_xlat61 = dot(u_xlat2.xy, u_xlat2.xy);
    u_xlat61 = sqrt(u_xlat61);
    u_xlat2.x = (-_SmoothStep.xxxy.z) + _SmoothStep.xxxy.w;
    u_xlat61 = u_xlat61 + (-_SmoothStep.xxxy.z);
    u_xlat2.x = float(1.0) / u_xlat2.x;
    u_xlat61 = u_xlat61 * u_xlat2.x;
    u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
    u_xlat2.x = u_xlat61 * -2.0 + 3.0;
    u_xlat61 = u_xlat61 * u_xlat61;
    u_xlat61 = (-u_xlat2.x) * u_xlat61 + 1.0;
    u_xlat2.xyz = (-_OuterColor.xyz) + _Innercolor.xyz;
    u_xlat2.xyz = float3(u_xlat61) * u_xlat2.xyz + _OuterColor.xyz;
    u_xlat0.x = u_xlat0.x * u_xlat61;
    u_xlat61 = dot(u_xlat20.xyz, u_xlat20.xyz);
    u_xlat61 = _g_inversesqrt(u_xlat61);
    u_xlat4.xyz = u_xlat20.xyz * float3(u_xlat61);
    u_xlat20.x = u_xlat1.y * _tunity_MatrixV[1].z;
    u_xlat20.x = _tunity_MatrixV[0].z * u_xlat1.x + u_xlat20.x;
    u_xlat20.x = _tunity_MatrixV[2].z * u_xlat1.z + u_xlat20.x;
    u_xlat20.x = u_xlat20.x + _tunity_MatrixV[3].z;
    u_xlat20.x = (-u_xlat20.x) + (-_ProjectionParams.y);
    u_xlat20.x = max(u_xlat20.x, 0.0);
    u_xlat20.x = u_xlat20.x * _pad976.x;
    if(uint(_g_floatBitsToUint(_EnableProbeVolumes)) != uint(0)) {
        u_xlat40.x = _g_trunc(_FrameIndex_Weights.x);
        u_xlat40.xy = u_xlat40.xx * float2(2.08299994, 4.8670001) + hlslcc_FragCoord.xy;
        u_xlat40.x = dot(u_xlat40.xy, float2(0.0671105608, 0.00583714992));
        u_xlat40.x = _g_fract(u_xlat40.x);
        u_xlat40.x = u_xlat40.x * 52.9829178;
        u_xlat40.x = _g_fract(u_xlat40.x);
        u_xlat5.xy = u_xlat40.xx * float2(100.0, 1000.0);
        u_xlat5.xy = _g_fract(u_xlat5.xy);
        u_xlat5.xy = u_xlat5.xy + float2(-0.5, -0.5);
        u_xlat25.xyz = u_xlat5.yyy * _tunity_MatrixInvV[0].xyz;
        u_xlat5.xyz = _tunity_MatrixInvV[1].xyz * u_xlat5.xxx + u_xlat25.xyz;
        u_xlat5.xyz = u_xlat3.xyz + u_xlat5.xyz;
        u_xlatb60 = 0.0<_MinEntryPos_Noise.w;
        u_xlat40.x = u_xlat40.x * _MinEntryPos_Noise.w;
        u_xlat5.xyz = u_xlat40.xxx * u_xlat5.xyz + u_xlat1.xyz;
        u_xlat5.xyz = (bool(u_xlatb60)) ? u_xlat5.xyz : u_xlat1.xyz;
        u_xlat5.xyz = u_xlat5.xyz + (-_Offset_LayerCount.xyz);
        u_xlat5.xyz = u_xlat4.xyz * _Biases_NormalizationClamp.xxx + u_xlat5.xyz;
        u_xlat5.xyz = u_xlat3.xyz * _Biases_NormalizationClamp.yyy + u_xlat5.xyz;
        u_xlat6.xyz = u_xlat5.xyz * _MaxLoadedCellInEntries_RcpIndirectionEntryDim.www;
        u_xlat6.xyz = floor(u_xlat6.xyz);
        u_xlatb7.xyz = _g_greaterThanEqual(u_xlat6.xyzx, _MinLoadedCellInEntries_IndirectionEntryDim.xyzx).xyz;
        u_xlatb40 = u_xlatb7.y && u_xlatb7.x;
        u_xlatb40 = u_xlatb7.z && u_xlatb40;
        u_xlatb7.xyz = _g_greaterThanEqual(_MaxLoadedCellInEntries_RcpIndirectionEntryDim.xyzx, u_xlat6.xyzx).xyz;
        u_xlatb60 = u_xlatb7.y && u_xlatb7.x;
        u_xlatb60 = u_xlatb7.z && u_xlatb60;
        u_xlatb40 = u_xlatb60 && u_xlatb40;
        if(u_xlatb40){
            u_xlat7.xyz = u_xlat6.xyz + (-_MinEntryPos_Noise.xyz);
            u_xlatu7.xyz = uint3(u_xlat7.xyz);
            u_xlati40 = int(u_xlatu7.y) * _g_floatBitsToInt(_EntryCount_X_XY_LeakReduction.x) + int(u_xlatu7.x);
            u_xlati40 = int(u_xlatu7.z) * _g_floatBitsToInt(_EntryCount_X_XY_LeakReduction.y) + u_xlati40;
            u_xlatu7.xyz = uint3(_Structured_t3_buf[u_xlati40].value[(0 >> 2) + 0], _Structured_t3_buf[u_xlati40].value[(0 >> 2) + 1], _Structured_t3_buf[u_xlati40].value[(0 >> 2) + 2]);
            u_xlatu8.x = uint(u_xlatu7.x >> 29u);
            u_xlatu28.xz = uint2(u_xlatu7.y >> 10u, u_xlatu7.z >> 10u);
            u_xlatu28.y = uint(u_xlatu7.y >> 20u);
            u_xlat40.x = float(u_xlatu8.x);
            u_xlat40.x = u_xlat40.x * 1.58496249;
            u_xlat40.x = exp2(u_xlat40.x);
            u_xlat40.x = _g_roundEven(u_xlat40.x);
            u_xlatu9.xyz = uint3(u_xlatu7.x & uint(536870911u), u_xlatu7.y & uint(1023u), u_xlatu7.z & uint(1023u));
            u_xlatu8.xyz = uint3(u_xlatu28.x & uint(1023u), u_xlatu28.y & uint(1023u), u_xlatu28.z & uint(1023u));
            u_xlatu60 = uint(u_xlatu7.z >> 20u);
            u_xlatu10.z = uint(u_xlatu60 & 1023u);
            u_xlatb60 = int(u_xlatu7.x)!=int(0xFFFFFFFFu);
            u_xlat6.xyz = (-u_xlat6.xyz) * _MinLoadedCellInEntries_IndirectionEntryDim.www + u_xlat5.xyz;
            u_xlat40.x = u_xlat40.x * _PoolDim_MinBrickSize.w;
            u_xlat6.xyz = u_xlat6.xyz / u_xlat40.xxx;
            u_xlat6.xyz = floor(u_xlat6.xyz);
            u_xlatu6.xyz = uint3(u_xlat6.xyz);
            u_xlatu6.xyz = min(u_xlatu6.xyz, uint3(26u, 26u, 26u));
            u_xlati7.x = 0 - int(u_xlatu9.y);
            u_xlati7.yz = 0 - int2(u_xlatu8.xy);
            u_xlatu6.xyz = u_xlatu6.xyz + uint3(u_xlati7.xyz);
            u_xlatu10.x = u_xlatu9.z;
            u_xlatu10.y = u_xlatu8.z;
            u_xlatb7.xyz = _g_lessThan(u_xlatu6.xyzx, u_xlatu10.xyzx).xyz;
            u_xlatb40 = u_xlatb7.y && u_xlatb7.x;
            u_xlatb40 = u_xlatb7.z && u_xlatb40;
            if(u_xlatb40){
                u_xlati40 = int(u_xlatu8.z) * int(u_xlatu9.z);
                u_xlati61 = int(u_xlatu6.x) * int(u_xlatu8.z) + int(u_xlatu6.y);
                u_xlati40 = int(u_xlatu6.z) * u_xlati40 + u_xlati61;
                u_xlati40 = int(u_xlatu9.x) * 243 + u_xlati40;
                u_xlatu6.x = _Structured_t2_buf[u_xlati40].value[(0 >> 2) + 0];
            } else {
                u_xlatu6.x = 4294967295u;
            }
            u_xlatu40 = (u_xlatb60) ? u_xlatu6.x : 4294967295u;
        } else {
            u_xlatu40 = 4294967295u;
        }
        u_xlati60 = int((int(u_xlatu40)!=int(0xFFFFFFFFu)) ? 0xFFFFFFFFu : uint(0));
        u_xlati61 = op_not(u_xlati60);
        if(u_xlati60 != 0) {
            u_xlatu62 = uint(u_xlatu40 >> 28u);
            u_xlatu40 = uint(u_xlatu40 & 268435455u);
            u_xlat40.x = float(u_xlatu40);
            u_xlat63 = u_xlat40.x * _RcpPoolDim_XY.w;
            u_xlat6.z = floor(u_xlat63);
            u_xlat63 = _PoolDim_MinBrickSize.y * _PoolDim_MinBrickSize.x;
            u_xlat40.x = (-u_xlat6.z) * u_xlat63 + u_xlat40.x;
            u_xlat63 = u_xlat40.x * _RcpPoolDim_XY.x;
            u_xlat6.y = floor(u_xlat63);
            u_xlat40.x = (-u_xlat6.y) * _PoolDim_MinBrickSize.x + u_xlat40.x;
            u_xlat6.x = floor(u_xlat40.x);
            u_xlat40.x = float(u_xlatu62);
            u_xlat40.x = u_xlat40.x * 1.58496249;
            u_xlat40.x = exp2(u_xlat40.x);
            u_xlat40.x = u_xlat40.x * _PoolDim_MinBrickSize.w;
            u_xlat5.xyz = u_xlat5.xyz / u_xlat40.xxx;
            u_xlat5.xyz = _g_fract(u_xlat5.xyz);
            u_xlat6.xyz = u_xlat6.xyz + float3(0.5, 0.5, 0.5);
            u_xlat5.xyz = u_xlat5.xyz * float3(3.0, 3.0, 3.0) + u_xlat6.xyz;
            u_xlat5.xyz = u_xlat5.xyz * _RcpPoolDim_XY.xyz;
            u_xlatb40 = _g_floatBitsToInt(_EntryCount_X_XY_LeakReduction.z)==2;
            if(u_xlatb40){
                u_xlat6.xyz = u_xlat5.xyz * _PoolDim_MinBrickSize.xyz + float3(-0.5, -0.5, -0.5);
                u_xlat7.xyz = _g_fract(u_xlat6.xyz);
                u_xlatu6.xyz =  uint3(int3(u_xlat6.xyz));
                u_xlatu6.w = uint(0u);
                u_xlat6 = _g_texelFetch(_Texture_t9, int3(u_xlatu6.xyz), int(u_xlatu6.w));
                u_xlatu40 = uint(_Offset_LayerCount.w);
                u_xlatb40 = int(u_xlatu40)==1;
                u_xlat62 = u_xlat6.x * 255.0;
                u_xlatu62 = uint(u_xlat62);
                u_xlati63 = int(uint(uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.y)) | uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.x))));
                u_xlati63 = int(uint(uint(u_xlati63) | uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.z))));
                u_xlati63 = int(uint(uint(u_xlati63) | uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.w))));
                u_xlati63 = int(uint(uint(u_xlati63) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
                u_xlat63 = (u_xlati63 != 0) ? unity_RenderingLayer.x : _g_intBitsToFloat(int(0xFFFFFFFFu));
                u_xlati8 = int4(uint4(uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.x)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.y)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.z)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.w))));
                u_xlat63 = (u_xlati8.x != 0) ? u_xlat6.x : 0.0;
                u_xlatu65 = uint(uint(_g_floatBitsToUint(u_xlat6.x)) >> 8u);
                u_xlatu26.x = uint(uint(_g_floatBitsToUint(u_xlat6.x)) >> 16u);
                u_xlatu6.x = uint(uint(_g_floatBitsToUint(u_xlat6.x)) >> 24u);
                u_xlat65 = _g_uintBitsToFloat(uint(uint(_g_floatBitsToUint(u_xlat63)) | u_xlatu65));
                u_xlat63 = (u_xlati8.y != 0) ? u_xlat65 : u_xlat63;
                u_xlat65 = _g_uintBitsToFloat(uint(u_xlatu26.x | uint(_g_floatBitsToUint(u_xlat63))));
                u_xlat63 = (u_xlati8.z != 0) ? u_xlat65 : u_xlat63;
                u_xlat65 = _g_uintBitsToFloat(uint(u_xlatu6.x | uint(_g_floatBitsToUint(u_xlat63))));
                u_xlat63 = (u_xlati8.w != 0) ? u_xlat65 : u_xlat63;
                u_xlatu63 = uint(uint(_g_floatBitsToUint(u_xlat63)) & 255u);
                u_xlatu40 = (u_xlatb40) ? u_xlatu62 : u_xlatu63;
                u_xlatu6.x = _Structured_t5_buf[u_xlatu40].value[(0 >> 2) + 0];
                u_xlatu8 = uint4(u_xlatu6.x & uint(4u), u_xlatu6.x & uint(2u), u_xlatu6.x & uint(32u), u_xlatu6.x & uint(16u));
                u_xlat8 = float4(u_xlatu8);
                u_xlat8 = min(u_xlat8, float4(1.0, 1.0, 1.0, 1.0));
                u_xlat26.xy = u_xlat8.yw * u_xlat7.xy + (-u_xlat7.xy);
                u_xlat8.xy = u_xlat26.xy + u_xlat8.xz;
                u_xlatu26.xy = uint2(u_xlatu6.x & uint(256u), u_xlatu6.x & uint(128u));
                u_xlat26.xy = float2(u_xlatu26.xy);
                u_xlat26.xy = min(u_xlat26.xy, float2(1.0, 1.0));
                u_xlat62 = u_xlat26.y * u_xlat7.z + (-u_xlat7.z);
                u_xlat8.z = u_xlat62 + u_xlat26.x;
                u_xlat26.xyz = u_xlat8.xyz * _RcpPoolDim_XY.xyz + u_xlat5.xyz;
                u_xlat8 = _g_textureLod(_Texture_t6, u_xlat26.xyz, 0.0).wxyz;
                u_xlat9 = _g_textureLod(_Texture_t7, u_xlat26.xyz, 0.0);
                u_xlat10 = _g_textureLod(_Texture_t8, u_xlat26.xyz, 0.0);
                u_xlatb11.xy = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), _FrameIndex_Weights.zwzz).xy;
                if(u_xlatb11.x){
                    u_xlat12 = _g_textureLod(_Texture_t10, u_xlat26.xyz, 0.0);
                } else {
                    u_xlat12.x = float(0.0);
                    u_xlat12.y = float(0.0);
                    u_xlat12.z = float(0.0);
                    u_xlat12.w = float(0.0);
                }
                if(u_xlatb11.y){
                    u_xlat26.xyz = u_xlat26.xyz * _PoolDim_MinBrickSize.xyz + float3(-0.5, -0.5, -0.5);
                    u_xlatu13.xyz =  uint3(int3(u_xlat26.xyz));
                    u_xlatu13.w = uint(0u);
                    u_xlat13 = _g_texelFetch(_Texture_t11, int3(u_xlatu13.xyz), int(u_xlatu13.w));
                    u_xlat62 = u_xlat13.x * 255.0;
                    u_xlatu62 = uint(u_xlat62);
                    u_xlatb63 = int(u_xlatu62)==255;
                    u_xlat13.xyz = float3(_g_uintBitsToFloat(_Structured_t4_buf[u_xlatu62].value[(0 >> 2) + 0]), _g_uintBitsToFloat(_Structured_t4_buf[u_xlatu62].value[(0 >> 2) + 1]), _g_uintBitsToFloat(_Structured_t4_buf[u_xlatu62].value[(0 >> 2) + 2]));
                    u_xlat26.xyz = (bool(u_xlatb63)) ? float3(0.0, 0.0, 0.0) : u_xlat13.xyz;
                } else {
                    u_xlat26.x = float(0.0);
                    u_xlat26.y = float(0.0);
                    u_xlat26.z = float(0.0);
                }
                u_xlatb40 = int(u_xlatu40)!=255;
                if(u_xlatb40){
                    u_xlatu13.x = uint(u_xlatu6.x >> 1u);
                    u_xlatu13.y = uint(u_xlatu6.x >> 4u);
                    u_xlatu13.z = uint(u_xlatu6.x >> 7u);
                    u_xlatu13.w = uint(u_xlatu6.x >> 9u);
                    u_xlatu14 = uint4(u_xlatu13.x & uint(3u), u_xlatu13.y & uint(3u), u_xlatu13.z & uint(3u), u_xlatu13.w & uint(1u));
                    u_xlatu31.xyz = uint3(u_xlatu6.x & uint(1u), u_xlatu6.x & uint(8u), u_xlatu6.x & uint(64u));
                    u_xlat31.xyz = float3(u_xlatu31.xyz);
                    u_xlati13.xyz = int3(u_xlatu14.xyz) + int3(int(0xFFFFFFFFu), int(0xFFFFFFFFu), int(0xFFFFFFFFu));
                    u_xlat13.xyz = float3(u_xlati13.xyz);
                    u_xlat40.x = u_xlat13.x * u_xlat7.x + u_xlat31.x;
                    u_xlat31.xy = min(u_xlat31.yz, float2(1.0, 1.0));
                    u_xlat31.xy = u_xlat13.yz * u_xlat7.yz + u_xlat31.xy;
                    u_xlat40.x = u_xlat40.x * u_xlat31.x;
                    u_xlat13.x = u_xlat31.y * u_xlat40.x;
                    u_xlatu14.x = uint(u_xlatu13.w >> 1u);
                    u_xlatu14.y = uint(u_xlatu13.w >> 4u);
                    u_xlatu14.z = uint(u_xlatu13.w >> 7u);
                    u_xlati14.xyz = int3(uint3(u_xlatu14.x & uint(3u), u_xlatu14.y & uint(3u), u_xlatu14.z & uint(3u)));
                    u_xlat62 = float(u_xlatu14.w);
                    u_xlati14.xyz = u_xlati14.xyz + int3(int(0xFFFFFFFFu), int(0xFFFFFFFFu), int(0xFFFFFFFFu));
                    u_xlat14.xyz = float3(u_xlati14.xyz);
                    u_xlat62 = u_xlat14.x * u_xlat7.x + u_xlat62;
                    u_xlatu31.xz = uint2(u_xlatu13.w & uint(8u), u_xlatu13.w & uint(64u));
                    u_xlat31.xz = float2(u_xlatu31.xz);
                    u_xlat31.xz = min(u_xlat31.xz, float2(1.0, 1.0));
                    u_xlat31.xz = u_xlat14.yz * u_xlat7.yz + u_xlat31.xz;
                    u_xlat62 = u_xlat62 * u_xlat31.x;
                    u_xlat13.y = u_xlat31.z * u_xlat62;
                    u_xlatu62 = uint(u_xlatu6.x >> 18u);
                    u_xlatu14.x = uint(u_xlatu62 >> 1u);
                    u_xlatu14.y = uint(u_xlatu62 >> 4u);
                    u_xlatu14.z = uint(u_xlatu62 >> 7u);
                    u_xlati14.xyz = int3(uint3(u_xlatu14.x & uint(3u), u_xlatu14.y & uint(3u), u_xlatu14.z & uint(3u)));
                    u_xlatu15.xyz = uint3(uint(u_xlatu62) & uint(1u), uint(u_xlatu62) & uint(8u), uint(u_xlatu62) & uint(64u));
                    u_xlat15.xyz = float3(u_xlatu15.xyz);
                    u_xlati14.xyz = u_xlati14.xyz + int3(int(0xFFFFFFFFu), int(0xFFFFFFFFu), int(0xFFFFFFFFu));
                    u_xlat14.xyz = float3(u_xlati14.xyz);
                    u_xlat63 = u_xlat14.x * u_xlat7.x + u_xlat15.x;
                    u_xlat31.xz = min(u_xlat15.yz, float2(1.0, 1.0));
                    u_xlat31.xz = u_xlat14.yz * u_xlat7.yz + u_xlat31.xz;
                    u_xlat63 = u_xlat63 * u_xlat31.x;
                    u_xlat13.z = u_xlat31.z * u_xlat63;
                    u_xlat40.x = u_xlat40.x * u_xlat31.y + u_xlat13.y;
                    u_xlat40.x = u_xlat63 * u_xlat31.z + u_xlat40.x;
                    u_xlat40.x = max(u_xlat40.x, 9.99999975e-05);
                    u_xlat40.x = float(1.0) / u_xlat40.x;
                    u_xlat31.xyz = u_xlat40.xxx * u_xlat13.xyz;
                    u_xlat13.xyz = u_xlat8.yzw * u_xlat31.xxx;
                    u_xlat8.y = u_xlat9.w;
                    u_xlat8.z = u_xlat10.w;
                    u_xlat8.xyz = u_xlat31.xxx * u_xlat8.xyz;
                    u_xlat9.xyz = u_xlat9.xyz * u_xlat31.xxx;
                    u_xlat10.xyz = u_xlat10.xyz * u_xlat31.xxx;
                    u_xlat12 = u_xlat31.xxxx * u_xlat12;
                    u_xlatb14.xy = _g_notEqual(u_xlat31.yzyy, float4(0.0, 0.0, 0.0, 0.0)).xy;
                    if(u_xlatb14.x){
                        u_xlatu15 = uint4(u_xlatu13.w & uint(4u), u_xlatu13.w & uint(2u), u_xlatu13.w & uint(32u), u_xlatu13.w & uint(16u));
                        u_xlat15 = float4(u_xlatu15);
                        u_xlat15 = min(u_xlat15, float4(1.0, 1.0, 1.0, 1.0));
                        u_xlat14.xz = u_xlat15.yw * u_xlat7.xy + (-u_xlat7.xy);
                        u_xlat15.xy = u_xlat14.xz + u_xlat15.xz;
                        u_xlatu14.xz = uint2(u_xlatu13.w & uint(256u), u_xlatu13.w & uint(128u));
                        u_xlat14.xz = float2(u_xlatu14.xz);
                        u_xlat14.xz = min(u_xlat14.xz, float2(1.0, 1.0));
                        u_xlat40.x = u_xlat14.z * u_xlat7.z + (-u_xlat7.z);
                        u_xlat15.z = u_xlat40.x + u_xlat14.x;
                        u_xlat14.xzw = u_xlat15.xyz * _RcpPoolDim_XY.xyz + u_xlat5.xyz;
                        u_xlat15 = _g_textureLod(_Texture_t6, u_xlat14.xzw, 0.0);
                        u_xlat16 = _g_textureLod(_Texture_t7, u_xlat14.xzw, 0.0);
                        u_xlat17 = _g_textureLod(_Texture_t8, u_xlat14.xzw, 0.0);
                        if(u_xlatb11.x){
                            u_xlat18 = _g_textureLod(_Texture_t10, u_xlat14.xzw, 0.0);
                        } else {
                            u_xlat18.x = float(0.0);
                            u_xlat18.y = float(0.0);
                            u_xlat18.z = float(0.0);
                            u_xlat18.w = float(0.0);
                        }
                        u_xlat19.x = u_xlat15.w;
                        u_xlat19.y = u_xlat16.w;
                        u_xlat19.z = u_xlat17.w;
                        u_xlat13.xyz = u_xlat15.xyz * u_xlat31.yyy + u_xlat13.xyz;
                        u_xlat8.xyz = u_xlat19.xyz * u_xlat31.yyy + u_xlat8.xyz;
                        u_xlat9.xyz = u_xlat16.xyz * u_xlat31.yyy + u_xlat9.xyz;
                        u_xlat10.xyz = u_xlat17.xyz * u_xlat31.yyy + u_xlat10.xyz;
                        u_xlat12 = u_xlat18 * u_xlat31.yyyy + u_xlat12;
                    }
                    if(u_xlatb14.y){
                        u_xlatu14 = uint4(uint(u_xlatu62) & uint(4u), uint(u_xlatu62) & uint(2u), uint(u_xlatu62) & uint(32u), uint(u_xlatu62) & uint(16u));
                        u_xlat14 = float4(u_xlatu14);
                        u_xlat14 = min(u_xlat14, float4(1.0, 1.0, 1.0, 1.0));
                        u_xlat7.xy = u_xlat14.yw * u_xlat7.xy + (-u_xlat7.xy);
                        u_xlat14.xy = u_xlat7.xy + u_xlat14.xz;
                        u_xlatu7.xy = uint2(uint(u_xlatu62) & uint(256u), uint(u_xlatu62) & uint(128u));
                        u_xlat7.xy = float2(u_xlatu7.xy);
                        u_xlat7.xy = min(u_xlat7.xy, float2(1.0, 1.0));
                        u_xlat40.x = u_xlat7.y * u_xlat7.z + (-u_xlat7.z);
                        u_xlat14.z = u_xlat40.x + u_xlat7.x;
                        u_xlat7.xyz = u_xlat14.xyz * _RcpPoolDim_XY.xyz + u_xlat5.xyz;
                        u_xlat14 = _g_textureLod(_Texture_t6, u_xlat7.xyz, 0.0);
                        u_xlat15 = _g_textureLod(_Texture_t7, u_xlat7.xyz, 0.0);
                        u_xlat16 = _g_textureLod(_Texture_t8, u_xlat7.xyz, 0.0);
                        if(u_xlatb11.x){
                            u_xlat7 = _g_textureLod(_Texture_t10, u_xlat7.xyz, 0.0);
                        } else {
                            u_xlat7.x = float(0.0);
                            u_xlat7.y = float(0.0);
                            u_xlat7.z = float(0.0);
                            u_xlat7.w = float(0.0);
                        }
                        u_xlat11.x = u_xlat14.w;
                        u_xlat11.y = u_xlat15.w;
                        u_xlat11.z = u_xlat16.w;
                        u_xlat13.xyz = u_xlat14.xyz * u_xlat31.zzz + u_xlat13.xyz;
                        u_xlat8.xyz = u_xlat11.xyz * u_xlat31.zzz + u_xlat8.xyz;
                        u_xlat9.xyz = u_xlat15.xyz * u_xlat31.zzz + u_xlat9.xyz;
                        u_xlat10.xyz = u_xlat16.xyz * u_xlat31.zzz + u_xlat10.xyz;
                        u_xlat12 = u_xlat7 * u_xlat31.zzzz + u_xlat12;
                    }
                } else {
                    u_xlat13.xyz = u_xlat8.yzw;
                    u_xlat8.y = u_xlat9.w;
                    u_xlat8.z = u_xlat10.w;
                }
            } else {
                u_xlatb40 = _g_floatBitsToInt(_EntryCount_X_XY_LeakReduction.z)==1;
                if(u_xlatb40){
                    u_xlat7.xyz = u_xlat5.xyz * _PoolDim_MinBrickSize.xyz + float3(-0.5, -0.5, -0.5);
                    u_xlatu11.xyz =  uint3(int3(u_xlat7.xyz));
                    u_xlatu11.w = uint(0u);
                    u_xlat11 = _g_texelFetch(_Texture_t9, int3(u_xlatu11.xyz), int(u_xlatu11.w));
                    u_xlatu40 = uint(_Offset_LayerCount.w);
                    u_xlatb40 = int(u_xlatu40)==1;
                    u_xlat62 = u_xlat11.x * 255.0;
                    u_xlatu62 = uint(u_xlat62);
                    u_xlati63 = int(uint(uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.y)) | uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.x))));
                    u_xlati63 = int(uint(uint(u_xlati63) | uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.z))));
                    u_xlati63 = int(uint(uint(u_xlati63) | uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.w))));
                    u_xlati63 = int(uint(uint(u_xlati63) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
                    u_xlat63 = (u_xlati63 != 0) ? unity_RenderingLayer.x : _g_intBitsToFloat(int(0xFFFFFFFFu));
                    u_xlati14 = int4(uint4(uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.x)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.y)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.z)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_ProbeVolumeLayerMask.w))));
                    u_xlat63 = (u_xlati14.x != 0) ? u_xlat11.x : 0.0;
                    u_xlatu65 = uint(uint(_g_floatBitsToUint(u_xlat11.x)) >> 8u);
                    u_xlatu6.x = uint(uint(_g_floatBitsToUint(u_xlat11.x)) >> 16u);
                    u_xlatu67 = uint(uint(_g_floatBitsToUint(u_xlat11.x)) >> 24u);
                    u_xlat65 = _g_uintBitsToFloat(uint(uint(_g_floatBitsToUint(u_xlat63)) | u_xlatu65));
                    u_xlat63 = (u_xlati14.y != 0) ? u_xlat65 : u_xlat63;
                    u_xlat65 = _g_uintBitsToFloat(uint(u_xlatu6.x | uint(_g_floatBitsToUint(u_xlat63))));
                    u_xlat63 = (u_xlati14.z != 0) ? u_xlat65 : u_xlat63;
                    u_xlat65 = _g_uintBitsToFloat(uint(u_xlatu67 | uint(_g_floatBitsToUint(u_xlat63))));
                    u_xlat63 = (u_xlati14.w != 0) ? u_xlat65 : u_xlat63;
                    u_xlatu63 = uint(uint(_g_floatBitsToUint(u_xlat63)) & 255u);
                    u_xlatu40 = (u_xlatb40) ? u_xlatu62 : u_xlatu63;
                    u_xlatb62 = int(u_xlatu40)!=255;
                    if(u_xlatb62){
                        u_xlat7.xyz = _g_fract(u_xlat7.xyz);
                        u_xlat11.xyz = (-u_xlat7.xyz) + float3(1.0, 1.0, 1.0);
                        u_xlat62 = u_xlat11.y * u_xlat11.x;
                        u_xlat63 = u_xlat11.z * u_xlat62;
                        u_xlatu14 = uint4(uint(u_xlatu40) & uint(1u), uint(u_xlatu40) & uint(2u), uint(u_xlatu40) & uint(4u), uint(u_xlatu40) & uint(8u));
                        u_xlat65 = float(int(u_xlatu14.x));
                        u_xlat15 = u_xlat7.xxyy * u_xlat11.yyxx;
                        u_xlat16 = u_xlat11.zzzz * u_xlat15.yyww;
                        u_xlatu11.xyw = min(u_xlatu14.yzw, uint3(1u, 1u, 1u));
                        u_xlat11.xyw = float3(int3(u_xlatu11.xyw));
                        u_xlat14 = u_xlat11.xxyy * u_xlat16;
                        u_xlat63 = u_xlat63 * u_xlat65 + u_xlat14.y;
                        u_xlat63 = u_xlat16.w * u_xlat11.y + u_xlat63;
                        u_xlat65 = u_xlat7.y * u_xlat7.x;
                        u_xlat6.x = u_xlat11.z * u_xlat65;
                        u_xlat67 = u_xlat11.w * u_xlat6.x;
                        u_xlat63 = u_xlat6.x * u_xlat11.w + u_xlat63;
                        u_xlat62 = u_xlat7.z * u_xlat62;
                        u_xlatu11 = uint4(uint(u_xlatu40) & uint(16u), uint(u_xlatu40) & uint(32u), uint(u_xlatu40) & uint(64u), uint(u_xlatu40) & uint(128u));
                        u_xlatu11 = min(u_xlatu11, uint4(1u, 1u, 1u, 1u));
                        u_xlat11 = float4(int4(u_xlatu11));
                        u_xlat40.x = u_xlat62 * u_xlat11.x;
                        u_xlat62 = u_xlat62 * u_xlat11.x + u_xlat63;
                        u_xlat15 = u_xlat7.zzzz * u_xlat15;
                        u_xlat16 = u_xlat11.yyzz * u_xlat15;
                        u_xlat62 = u_xlat15.y * u_xlat11.y + u_xlat62;
                        u_xlat62 = u_xlat15.w * u_xlat11.z + u_xlat62;
                        u_xlat63 = u_xlat7.z * u_xlat65;
                        u_xlat65 = u_xlat11.w * u_xlat63;
                        u_xlat62 = u_xlat63 * u_xlat11.w + u_xlat62;
                        u_xlat62 = max(u_xlat62, 9.99999975e-05);
                        u_xlat62 = float(1.0) / u_xlat62;
                        u_xlat11 = float4(u_xlat62) * u_xlat14;
                        u_xlat7.xyz = u_xlat11.xyy * float3(1.0, 0.0, 0.0) + (-u_xlat7.xyz);
                        u_xlat7.xyz = u_xlat11.zwz * float3(0.0, 1.0, 0.0) + u_xlat7.xyz;
                        u_xlat63 = u_xlat62 * u_xlat67;
                        u_xlat7.xyz = float3(u_xlat63) * float3(1.0, 1.0, 0.0) + u_xlat7.xyz;
                        u_xlat40.x = u_xlat40.x * u_xlat62;
                        u_xlat7.xyz = u_xlat40.xxx * float3(0.0, 0.0, 1.0) + u_xlat7.xyz;
                        u_xlat11 = float4(u_xlat62) * u_xlat16;
                        u_xlat7.xyz = u_xlat11.xyx * float3(1.0, 0.0, 1.0) + u_xlat7.xyz;
                        u_xlat7.xyz = u_xlat11.zww * float3(0.0, 1.0, 1.0) + u_xlat7.xyz;
                        u_xlat7.xyz = float3(u_xlat65) * float3(u_xlat62) + u_xlat7.xyz;
                        u_xlat5.xyz = u_xlat7.xyz * _RcpPoolDim_XY.xyz + u_xlat5.xyz;
                    }
                }
                u_xlat8 = _g_textureLod(_Texture_t6, u_xlat5.xyz, 0.0).wxyz;
                u_xlat9 = _g_textureLod(_Texture_t7, u_xlat5.xyz, 0.0);
                u_xlat10 = _g_textureLod(_Texture_t8, u_xlat5.xyz, 0.0);
                u_xlatb7.xy = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), _FrameIndex_Weights.zwzz).xy;
                if(u_xlatb7.x){
                    u_xlat12 = _g_textureLod(_Texture_t10, u_xlat5.xyz, 0.0);
                } else {
                    u_xlat12.x = float(0.0);
                    u_xlat12.y = float(0.0);
                    u_xlat12.z = float(0.0);
                    u_xlat12.w = float(0.0);
                }
                if(u_xlatb7.y){
                    u_xlat5.xyz = u_xlat5.xyz * _PoolDim_MinBrickSize.xyz + float3(-0.5, -0.5, -0.5);
                    u_xlatu5.xyz =  uint3(int3(u_xlat5.xyz));
                    u_xlatu5.w = uint(0u);
                    u_xlat5 = _g_texelFetch(_Texture_t11, int3(u_xlatu5.xyz), int(u_xlatu5.w));
                    u_xlat40.x = u_xlat5.x * 255.0;
                    u_xlatu40 = uint(u_xlat40.x);
                    u_xlatb62 = int(u_xlatu40)==255;
                    u_xlat5.xyz = float3(_g_uintBitsToFloat(_Structured_t4_buf[u_xlatu40].value[(0 >> 2) + 0]), _g_uintBitsToFloat(_Structured_t4_buf[u_xlatu40].value[(0 >> 2) + 1]), _g_uintBitsToFloat(_Structured_t4_buf[u_xlatu40].value[(0 >> 2) + 2]));
                    u_xlat26.xyz = (bool(u_xlatb62)) ? float3(0.0, 0.0, 0.0) : u_xlat5.xyz;
                } else {
                    u_xlat26.x = float(0.0);
                    u_xlat26.y = float(0.0);
                    u_xlat26.z = float(0.0);
                }
                u_xlat13.xyz = u_xlat8.yzw;
                u_xlat8.y = u_xlat9.w;
                u_xlat8.z = u_xlat10.w;
            }
        } else {
            u_xlat12.x = float(0.0);
            u_xlat12.y = float(0.0);
            u_xlat12.z = float(0.0);
            u_xlat12.w = float(0.0);
            u_xlat13.x = float(0.0);
            u_xlat13.y = float(0.0);
            u_xlat13.z = float(0.0);
            u_xlat8.x = float(0.0);
            u_xlat8.y = float(0.0);
            u_xlat8.z = float(0.0);
            u_xlat9.x = float(0.0);
            u_xlat9.y = float(0.0);
            u_xlat9.z = float(0.0);
            u_xlat10.x = float(0.0);
            u_xlat10.y = float(0.0);
            u_xlat10.z = float(0.0);
            u_xlat26.x = float(0.0);
            u_xlat26.y = float(0.0);
            u_xlat26.z = float(0.0);
        }
        u_xlatb40 = u_xlati61!=int(0xFFFFFFFFu);
        if(u_xlatb40){
            u_xlat5.xyz = u_xlat8.xyz + float3(-0.5, -0.5, -0.5);
            u_xlat7.xyz = u_xlat13.xyz * float3(4.0, 4.0, 4.0);
            u_xlat5.xyz = u_xlat5.xyz * u_xlat7.xxx;
            u_xlat11.xyz = u_xlat9.xyz + float3(-0.5, -0.5, -0.5);
            u_xlat7.xyw = u_xlat7.yyy * u_xlat11.xyz;
            u_xlat11.xyz = u_xlat10.xyz + float3(-0.5, -0.5, -0.5);
            u_xlat11.xyz = u_xlat7.zzz * u_xlat11.xyz;
            u_xlat5.xyz = (int(u_xlati60) != 0) ? u_xlat5.xyz : u_xlat8.xyz;
            u_xlat7.xyz = (int(u_xlati60) != 0) ? u_xlat7.xyw : u_xlat9.xyz;
            u_xlat8.xyz = (int(u_xlati60) != 0) ? u_xlat11.xyz : u_xlat10.xyz;
            u_xlat5.x = dot(u_xlat5.xyz, u_xlat4.xyz);
            u_xlat5.y = dot(u_xlat7.xyz, u_xlat4.xyz);
            u_xlat5.z = dot(u_xlat8.xyz, u_xlat4.xyz);
            u_xlat5.xyz = u_xlat13.xyz + u_xlat5.xyz;
            u_xlat7.yzw = u_xlat4.xyz * float3(0.488602519, 0.488602519, 0.488602519);
            u_xlat7.x = 0.282094806;
            u_xlat40.x = dot(u_xlat7, u_xlat12);
            u_xlat40.x = u_xlat40.x * _FrameIndex_Weights.z;
            u_xlatb7.xy = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), _FrameIndex_Weights.zwzz).xy;
            u_xlat60 = dot(u_xlat26.xyz, u_xlat26.xyz);
            u_xlatb61 = u_xlat60<0.200000003;
            u_xlat60 = _g_inversesqrt(u_xlat60);
            u_xlat6.xyz = float3(u_xlat60) * u_xlat26.xyz;
            u_xlat6.xyz = (bool(u_xlatb61)) ? u_xlat4.xyz : u_xlat6.xyz;
            u_xlat6.xyz = (u_xlatb7.y) ? u_xlat6.xyz : u_xlat4.xyz;
            u_xlat6.w = 1.0;
            u_xlat8.x = dot(unity_SHAr, u_xlat6);
            u_xlat8.y = dot(unity_SHAg, u_xlat6);
            u_xlat8.z = dot(unity_SHAb, u_xlat6);
            u_xlat9 = u_xlat6.yzzx * u_xlat6.xyzz;
            u_xlat10.x = dot(unity_SHBr, u_xlat9);
            u_xlat10.y = dot(unity_SHBg, u_xlat9);
            u_xlat10.z = dot(unity_SHBb, u_xlat9);
            u_xlat60 = u_xlat6.y * u_xlat6.y;
            u_xlat60 = u_xlat6.x * u_xlat6.x + (-u_xlat60);
            u_xlat6.xyz = unity_SHC.xyz * float3(u_xlat60) + u_xlat10.xyz;
            u_xlat6.xyz = u_xlat6.xyz + u_xlat8.xyz;
            u_xlat6.xyz = u_xlat40.xxx * u_xlat6.xyz + u_xlat5.xyz;
            u_xlat5.xyz = (u_xlatb7.x) ? u_xlat6.xyz : u_xlat5.xyz;
            u_xlat5.xyz = u_xlat5.xyz * _FrameIndex_Weights.yyy;
        } else {
            u_xlat4.w = 1.0;
            u_xlat6.x = dot(unity_SHAr, u_xlat4);
            u_xlat6.y = dot(unity_SHAg, u_xlat4);
            u_xlat6.z = dot(unity_SHAb, u_xlat4);
            u_xlat7 = u_xlat4.yzzx * u_xlat4.xyzz;
            u_xlat8.x = dot(unity_SHBr, u_xlat7);
            u_xlat8.y = dot(unity_SHBg, u_xlat7);
            u_xlat8.z = dot(unity_SHBb, u_xlat7);
            u_xlat40.x = u_xlat4.y * u_xlat4.y;
            u_xlat40.x = u_xlat4.x * u_xlat4.x + (-u_xlat40.x);
            u_xlat7.xyz = unity_SHC.xyz * u_xlat40.xxx + u_xlat8.xyz;
            u_xlat5.xyz = u_xlat6.xyz + u_xlat7.xyz;
        }
    } else {
        u_xlat4.w = 1.0;
        u_xlat6.x = dot(unity_SHAr, u_xlat4);
        u_xlat6.y = dot(unity_SHAg, u_xlat4);
        u_xlat6.z = dot(unity_SHAb, u_xlat4);
        u_xlat7 = u_xlat4.yzzx * u_xlat4.xyzz;
        u_xlat8.x = dot(unity_SHBr, u_xlat7);
        u_xlat8.y = dot(unity_SHBg, u_xlat7);
        u_xlat8.z = dot(unity_SHBb, u_xlat7);
        u_xlat40.x = u_xlat4.y * u_xlat4.y;
        u_xlat40.x = u_xlat4.x * u_xlat4.x + (-u_xlat40.x);
        u_xlat7.xyz = unity_SHC.xyz * u_xlat40.xxx + u_xlat8.xyz;
        u_xlat5.xyz = u_xlat6.xyz + u_xlat7.xyz;
    }
    __SV_Target0.w = max(u_xlat0.x, 0.0);
    u_xlat0.xzw = u_xlat2.xyz * float3(0.959999979, 0.959999979, 0.959999979);
    float3 txVec0 = float3(input.vs_INTERP4.xy,input.vs_INTERP4.z);
    u_xlat61 = _g_textureLod(_Texture_t1, txVec0, 0.0);
    u_xlat2.x = (-_MainLightShadowParams.x) + 1.0;
    u_xlat61 = u_xlat61 * _MainLightShadowParams.x + u_xlat2.x;
    u_xlatb2 = 0.0>=input.vs_INTERP4.z;
    u_xlatb22 = input.vs_INTERP4.z>=1.0;
    u_xlatb2 = u_xlatb22 || u_xlatb2;
    u_xlat61 = (u_xlatb2) ? 1.0 : u_xlat61;
    u_xlat2.xyz = u_xlat1.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat2.x = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat2.x = u_xlat2.x * _MainLightShadowParams.z + _MainLightShadowParams.w;
    u_xlat2.x = clamp(u_xlat2.x, 0.0, 1.0);
    u_xlat22 = (-u_xlat61) + 1.0;
    u_xlat61 = u_xlat2.x * u_xlat22 + u_xlat61;
    u_xlat2.x = dot((-u_xlat3.xyz), u_xlat4.xyz);
    u_xlat2.x = u_xlat2.x + u_xlat2.x;
    u_xlat2.xyz = u_xlat4.xyz * (-u_xlat2.xxx) + (-u_xlat3.xyz);
    u_xlat62 = dot(u_xlat4.xyz, u_xlat3.xyz);
    u_xlat62 = clamp(u_xlat62, 0.0, 1.0);
    u_xlat62 = (-u_xlat62) + 1.0;
    u_xlat62 = u_xlat62 * u_xlat62;
    u_xlat62 = u_xlat62 * u_xlat62;
    u_xlat6 = _g_textureLod(_Texture_t0, u_xlat2.xyz, 6.0);
    u_xlat2.x = u_xlat6.w + -1.0;
    u_xlat2.x = unity_SpecCube0_HDR.w * u_xlat2.x + 1.0;
    u_xlat2.x = max(u_xlat2.x, 0.0);
    u_xlat2.x = log2(u_xlat2.x);
    u_xlat2.x = u_xlat2.x * unity_SpecCube0_HDR.y;
    u_xlat2.x = exp2(u_xlat2.x);
    u_xlat2.x = u_xlat2.x * unity_SpecCube0_HDR.x;
    u_xlat2.xyz = u_xlat6.xyz * u_xlat2.xxx;
    u_xlat62 = u_xlat62 * 2.23517418e-08 + 0.0399999991;
    u_xlat62 = u_xlat62 * 0.5;
    u_xlat2.xyz = float3(u_xlat62) * u_xlat2.xyz;
    u_xlat2.xyz = u_xlat5.xyz * u_xlat0.xzw + u_xlat2.xyz;
    u_xlat61 = u_xlat61 * unity_LightData.z;
    u_xlat62 = dot(u_xlat4.xyz, _MainLightPosition.xyz);
    u_xlat62 = clamp(u_xlat62, 0.0, 1.0);
    u_xlat61 = u_xlat61 * u_xlat62;
    u_xlat5.xyz = float3(u_xlat61) * _MainLightColor.xyz;
    u_xlat6.xyz = u_xlat3.xyz + _MainLightPosition.xyz;
    u_xlat61 = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat61 = max(u_xlat61, 1.17549435e-38);
    u_xlat61 = _g_inversesqrt(u_xlat61);
    u_xlat6.xyz = float3(u_xlat61) * u_xlat6.xyz;
    u_xlat61 = dot(_MainLightPosition.xyz, u_xlat6.xyz);
    u_xlat61 = clamp(u_xlat61, 0.0, 1.0);
    u_xlat61 = u_xlat61 * u_xlat61;
    u_xlat61 = max(u_xlat61, 0.100000001);
    u_xlat61 = u_xlat61 * 6.00012016;
    u_xlat61 = float(1.0) / u_xlat61;
    u_xlat6.xyz = float3(u_xlat61) * float3(0.0399999991, 0.0399999991, 0.0399999991) + u_xlat0.xzw;
    u_xlat61 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu61 =  uint(int(u_xlat61));
    u_xlat7.x = float(0.0);
    u_xlat7.y = float(0.0);
    u_xlat7.z = float(0.0);
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<u_xlatu61 ; u_xlatu_loop_1++)
    {
        u_xlatu63 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati64 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat63 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati64]);
        u_xlati63 = int(u_xlat63);
        u_xlat8.xyz = (-u_xlat1.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat64 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat64 = max(u_xlat64, 6.10351562e-05);
        u_xlat65 = _g_inversesqrt(u_xlat64);
        u_xlat9.xyz = float3(u_xlat65) * u_xlat8.xyz;
        u_xlat66 = float(1.0) / u_xlat64;
        u_xlat64 = u_xlat64 * _AdditionalLightsAttenuation.x;
        u_xlat64 = (-u_xlat64) * u_xlat64 + 1.0;
        u_xlat64 = max(u_xlat64, 0.0);
        u_xlat64 = u_xlat64 * u_xlat64;
        u_xlat64 = u_xlat64 * u_xlat66;
        u_xlat66 = dot(_AdditionalLightsSpotDir.xyz, u_xlat9.xyz);
        u_xlat66 = u_xlat66 * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat66 = clamp(u_xlat66, 0.0, 1.0);
        u_xlat66 = u_xlat66 * u_xlat66;
        u_xlat64 = u_xlat64 * u_xlat66;
        u_xlat66 = dot(u_xlat4.xyz, u_xlat9.xyz);
        u_xlat66 = clamp(u_xlat66, 0.0, 1.0);
        u_xlat64 = u_xlat64 * u_xlat66;
        u_xlat10.xyz = float3(u_xlat64) * _AdditionalLightsColor.xyz;
        u_xlat8.xyz = u_xlat8.xyz * float3(u_xlat65) + u_xlat3.xyz;
        u_xlat63 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat63 = max(u_xlat63, 1.17549435e-38);
        u_xlat63 = _g_inversesqrt(u_xlat63);
        u_xlat8.xyz = float3(u_xlat63) * u_xlat8.xyz;
        u_xlat63 = dot(u_xlat9.xyz, u_xlat8.xyz);
        u_xlat63 = clamp(u_xlat63, 0.0, 1.0);
        u_xlat63 = u_xlat63 * u_xlat63;
        u_xlat63 = max(u_xlat63, 0.100000001);
        u_xlat63 = u_xlat63 * 6.00012016;
        u_xlat63 = float(1.0) / u_xlat63;
        u_xlat8.xyz = float3(u_xlat63) * float3(0.0399999991, 0.0399999991, 0.0399999991) + u_xlat0.xzw;
        u_xlat7.xyz = u_xlat8.xyz * u_xlat10.xyz + u_xlat7.xyz;
    }
    u_xlat0.xzw = u_xlat6.xyz * u_xlat5.xyz + u_xlat2.xyz;
    u_xlat0.xzw = u_xlat7.xyz + u_xlat0.xzw;
    u_xlat20.x = u_xlat20.x * (-u_xlat20.x);
    u_xlat20.x = exp2(u_xlat20.x);
    u_xlat1.x = (-u_xlat20.x) + 1.0;
    u_xlat1.xyz = u_xlat1.xxx * _pad992.xyz;
    __SV_Target0.xyz = u_xlat0.xzw * u_xlat20.xxx + u_xlat1.xyz;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
