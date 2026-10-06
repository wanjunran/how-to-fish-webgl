Shader "Shader Graphs/URPDecal"
{
    Properties
    {

	_BaseColor ("BaseColor", Vector) = (0,0,0,0)
	[NoScaleOffset] _BaseMap ("BaseMap", 2D) = "white" {}
	_NoiseScale ("NoiseScale", Float) = 1
	[NoScaleOffset] _NoiseMap ("NoiseMap", 2D) = "white" {}
	_NoiseStrength ("NoiseStrength", Range(0, 1)) = 0
	_Alpha ("Alpha", Range(0, 1)) = 1
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
            ZWrite On
            Cull Back
            // RenderType: Opaque


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
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler__BaseMap);
            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler__NoiseMap);
            TEXTURE2D(_Texture_t14);
            SAMPLER(sampler__Texture_t14);
            TEXTURE2D(_Texture_t15);
            SAMPLER(sampler__Texture_t15);

            float4 _pad0;
            float4 _pad16;
            float4 _pad32;
            float4 _pad48;
            float4 _pad64;
            float4 _pad80;
            float4 _pad96;
            float4 _pad112;
            float4 _pad128;
            float4 _pad144;
            float4 _pad432;
            float4 _pad448;
            float4 _pad464;
            float4 _pad480;
            float4 _pad496;
            float4 _pad512;
            float4 _pad528;
            float4 _pad976;
            float4 _pad992;
            float4x4 unity_MatrixVP;
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
            float4 _BaseColor;
            float _NoiseScale;
            float _NoiseStrength;
            float _Alpha;

            float4 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            float4 ImmCB_0_0_0[4];
            int4 u_xlati0;
            uint u_xlatu0;
            bool u_xlatb0;
            uint4 u_xlatu1;
            float4 u_xlat2;
            float4 u_xlat3;
            float4 u_xlat4;
            bool u_xlatb4;
            float4 u_xlat5;
            uint4 u_xlatu5;
            bool u_xlatb5;
            int3 u_xlati6;
            uint4 u_xlatu6;
            bool3 u_xlatb6;
            float4 u_xlat7;
            int4 u_xlati7;
            uint4 u_xlatu7;
            float4 u_xlat8;
            uint3 u_xlatu8;
            float4 u_xlat9;
            uint3 u_xlatu9;
            float4 u_xlat10;
            uint4 u_xlatu10;
            bool2 u_xlatb10;
            float4 u_xlat11;
            float4 u_xlat12;
            int3 u_xlati12;
            uint4 u_xlatu12;
            float4 u_xlat13;
            int4 u_xlati13;
            uint4 u_xlatu13;
            bool2 u_xlatb13;
            float4 u_xlat14;
            uint4 u_xlatu14;
            float4 u_xlat15;
            float4 u_xlat16;
            float4 u_xlat17;
            float3 u_xlat18;
            float3 u_xlat19;
            float3 u_xlat23;
            float3 u_xlat24;
            uint2 u_xlatu24;
            uint3 u_xlatu26;
            float3 u_xlat29;
            uint3 u_xlatu29;
            float2 u_xlat39;
            float u_xlat43;
            int u_xlati43;
            bool u_xlatb43;
            float u_xlat48;
            uint u_xlatu48;
            float u_xlat57;
            uint u_xlatu57;
            float u_xlat58;
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
            bool u_xlatb61;
            float u_xlat62;
            uint u_xlatu62;
            float u_xlat63;
            int u_xlati63;
            uint u_xlatu63;
            float u_xlat67;



            struct Attributes
            {
                float3 in_POSITION0 : POSITION;
                float3 in_NORMAL0 : NORMAL;
                float4 in_TEXCOORD0 : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 vs_INTERP2 : TEXCOORD0;
                float4 vs_INTERP4 : TEXCOORD1;
                float4 vs_INTERP5 : TEXCOORD2;
                float4 vs_INTERP6 : TEXCOORD3;
                float3 vs_INTERP7 : TEXCOORD4;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            float4x4 _tunity_MatrixVP = transpose(unity_MatrixVP);


    u_xlat0.xyz = input.in_POSITION0.yyy * _pad16.xyz;
    u_xlat0.xyz = _pad0.xyz * input.in_POSITION0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = _pad32.xyz * input.in_POSITION0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + _pad48.xyz;
    u_xlat1 = u_xlat0.yyyy * _tunity_MatrixVP[1];
    u_xlat1 = _tunity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = _tunity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    output.positionCS = u_xlat1 + _tunity_MatrixVP[3];
    output.vs_INTERP2.xyz = float3(0.0, 0.0, 0.0);
    u_xlat1.xyz = u_xlat0.yyy * _pad16.xyz;
    u_xlat0.xyw = _pad0.xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat0.xyz = _pad32.xyz * u_xlat0.zzz + u_xlat0.xyw;
    output.vs_INTERP4.xyz = u_xlat0.xyz + _pad48.xyz;
    output.vs_INTERP4.w = 0.0;
    output.vs_INTERP5 = input.in_TEXCOORD0;
    output.vs_INTERP6 = float4(0.0, 0.0, 0.0, 0.0);
    u_xlat0.x = dot(input.in_NORMAL0.xyz, _pad64.xyz);
    u_xlat0.y = dot(input.in_NORMAL0.xyz, _pad80.xyz);
    u_xlat0.z = dot(input.in_NORMAL0.xyz, _pad96.xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = max(u_xlat6, 1.17549435e-38);
    u_xlat6 = _g_inversesqrt(u_xlat6);
    output.vs_INTERP7.xyz = float3(u_xlat6) * u_xlat0.xyz;
    return output;

            }

            float4 frag(Varyings input) : SV_Target0
            {
            float4x4 _t_NormalReconstructionMatrix = transpose(_NormalReconstructionMatrix);
            float4x4 _tunity_MatrixInvV = transpose(unity_MatrixInvV);
            float4x4 _tunity_MatrixInvVP = transpose(unity_MatrixInvVP);
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);


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
    u_xlati0 = _g_floatBitsToInt(_g_texelFetch(_Texture_t14, int2(u_xlatu1.xy), int(u_xlatu1.w)));
    u_xlatu0 = uint(uint(u_xlati0.x) & uint(_g_floatBitsToUint(_RcpPoolDim_XY.x)));
    u_xlat0.x = float(u_xlatu0);
    u_xlat0.x = u_xlat0.x + -0.100000001;
    u_xlatb0 = u_xlat0.x<0.0;
    if(u_xlatb0){discard;}
    u_xlat0 = _g_texelFetch(_Texture_t15, int2(u_xlatu1.xy), int(u_xlatu1.w));
    u_xlat19.xy = hlslcc_FragCoord.xy * _ScreenSize.zw;
    u_xlat1.xy = u_xlat19.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat1.z = 1.0;
    u_xlat2.xyz = u_xlat1.xyz * _ProjectionParams.zzz;
    u_xlat3.xyz = u_xlat2.yyy * _t_NormalReconstructionMatrix[1].yzx;
    u_xlat2.xyw = _t_NormalReconstructionMatrix[0].yzx * u_xlat2.xxx + u_xlat3.xyz;
    u_xlat2.xyw = _t_NormalReconstructionMatrix[2].yzx * u_xlat2.zzz + u_xlat2.xyw;
    u_xlat2.xyz = _t_NormalReconstructionMatrix[3].yzx * u_xlat2.zzz + u_xlat2.xyw;
    u_xlat39.xy = (-_CameraDepthTexture_TexelSize.xy) * float2(0.5, 0.5) + float2(1.0, 1.0);
    u_xlat19.xy = min(u_xlat19.xy, u_xlat39.xy);
    u_xlat19.xy = u_xlat19.xy * _RTHandleScale.xy;
    u_xlat3 = _g_texture(_Texture_t15, u_xlat19.xy, _GlobalMipBias.x);
    u_xlat19.x = _ZBufferParams.x * u_xlat3.x + _ZBufferParams.y;
    u_xlat19.x = float(1.0) / u_xlat19.x;
    u_xlat19.xyz = u_xlat19.xxx * u_xlat2.xyz;
    u_xlat2.xyz = dFdy(u_xlat19.yzx);
    u_xlat19.xyz = dFdx(u_xlat19.xyz);
    u_xlat3.xyz = u_xlat19.xyz * u_xlat2.xyz;
    u_xlat19.xyz = u_xlat2.zxy * u_xlat19.yzx + (-u_xlat3.xyz);
    u_xlat39.x = dot(u_xlat19.xyz, u_xlat19.xyz);
    u_xlat39.x = max(u_xlat39.x, 1.17549435e-38);
    u_xlat39.x = _g_inversesqrt(u_xlat39.x);
    u_xlat19.xyz = u_xlat19.xyz * u_xlat39.xxx;
    u_xlat2 = (-u_xlat1.yyyy) * _tunity_MatrixInvVP[1];
    u_xlat1 = _tunity_MatrixInvVP[0] * u_xlat1.xxxx + u_xlat2;
    u_xlat1 = _tunity_MatrixInvVP[2] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = u_xlat1 + _tunity_MatrixInvVP[3];
    u_xlat1.xyz = u_xlat1.xyz / u_xlat1.www;
    u_xlat2.xyz = u_xlat1.yyy * _pad80.xyz;
    u_xlat2.xyz = _pad64.xyz * u_xlat1.xxx + u_xlat2.xyz;
    u_xlat2.xyz = _pad96.xyz * u_xlat1.zzz + u_xlat2.xyz;
    u_xlat2.xyz = u_xlat2.xyz + _pad112.xyz;
    u_xlat3.xyz = u_xlat2.xyz * float3(1.0, -1.0, 1.0);
    u_xlat0.x = max(abs(u_xlat3.y), abs(u_xlat3.x));
    u_xlat0.x = max(abs(u_xlat3.z), u_xlat0.x);
    u_xlat0.x = (-u_xlat0.x) + 0.5;
    u_xlatb0 = u_xlat0.x<0.0;
    if(u_xlatb0){discard;}
    u_xlat2.xy = u_xlat2.xz * float2(1.0, 1.0) + float2(0.5, 0.5);
    u_xlatb0 = unity_OrthoParams.w==0.0;
    u_xlat3.xyz = (-u_xlat1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat58 = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat58 = _g_inversesqrt(u_xlat58);
    u_xlat3.xyz = float3(u_xlat58) * u_xlat3.xyz;
    u_xlat4.x = _tunity_MatrixV[0].z;
    u_xlat4.y = _tunity_MatrixV[1].z;
    u_xlat4.z = _tunity_MatrixV[2].z;
    u_xlat3.xyz = (bool(u_xlatb0)) ? u_xlat3.xyz : u_xlat4.xyz;
    u_xlat0.x = _PoolDim_MinBrickSize.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat4.x = u_xlat2.x * _Offset_LayerCount.w + _MaxLoadedCellInEntries_RcpIndirectionEntryDim.w;
    u_xlat4.y = u_xlat2.y * _MinLoadedCellInEntries_IndirectionEntryDim.w + _PoolDim_MinBrickSize.w;
    u_xlat2 = _g_texture(_BaseMap, u_xlat4.xy, _GlobalMipBias.x);
    u_xlat2.xyz = u_xlat2.xyz * _BaseColor.xyz;
    u_xlat4.xy = u_xlat4.xy * float2(_NoiseScale);
    u_xlat4 = _g_texture(_NoiseMap, u_xlat4.xy, _GlobalMipBias.x);
    u_xlat4.xyz = _BaseColor.xyz * u_xlat4.xyz + (-u_xlat2.xyz);
    u_xlat2.xyz = float3(_NoiseStrength) * u_xlat4.xyz + u_xlat2.xyz;
    u_xlatb58 = u_xlat2.w>=0.100000001;
    u_xlat58 = u_xlatb58 ? 1.0 : float(0.0);
    u_xlat58 = u_xlat58 * _Alpha;
    __SV_Target0.w = u_xlat0.x * u_xlat58;
    __SV_Target0.w = clamp(__SV_Target0.w, 0.0, 1.0);
    u_xlat0.x = dot(u_xlat19.xyz, u_xlat19.xyz);
    u_xlat0.x = _g_inversesqrt(u_xlat0.x);
    u_xlat0.xyz = u_xlat0.xxx * u_xlat19.xyz;
    u_xlat58 = u_xlat1.y * _tunity_MatrixV[1].z;
    u_xlat58 = _tunity_MatrixV[0].z * u_xlat1.x + u_xlat58;
    u_xlat58 = _tunity_MatrixV[2].z * u_xlat1.z + u_xlat58;
    u_xlat58 = u_xlat58 + _tunity_MatrixV[3].z;
    u_xlat58 = (-u_xlat58) + (-_ProjectionParams.y);
    u_xlat58 = max(u_xlat58, 0.0);
    u_xlat58 = u_xlat58 * _pad976.x;
    if(uint(_g_floatBitsToUint(_EnableProbeVolumes)) != uint(0)) {
        u_xlat59 = _g_trunc(_pad128.x);
        u_xlat4.xy = float2(u_xlat59) * float2(2.08299994, 4.8670001) + hlslcc_FragCoord.xy;
        u_xlat59 = dot(u_xlat4.xy, float2(0.0671105608, 0.00583714992));
        u_xlat59 = _g_fract(u_xlat59);
        u_xlat59 = u_xlat59 * 52.9829178;
        u_xlat59 = _g_fract(u_xlat59);
        u_xlat4.xy = float2(u_xlat59) * float2(100.0, 1000.0);
        u_xlat4.xy = _g_fract(u_xlat4.xy);
        u_xlat4.xy = u_xlat4.xy + float2(-0.5, -0.5);
        u_xlat23.xyz = u_xlat4.yyy * _tunity_MatrixInvV[0].xyz;
        u_xlat4.xyz = _tunity_MatrixInvV[1].xyz * u_xlat4.xxx + u_xlat23.xyz;
        u_xlat4.xyz = u_xlat3.xyz + u_xlat4.xyz;
        u_xlatb60 = 0.0<_pad80.w;
        u_xlat59 = u_xlat59 * _pad80.w;
        u_xlat4.xyz = float3(u_xlat59) * u_xlat4.xyz + u_xlat1.xyz;
        u_xlat4.xyz = (bool(u_xlatb60)) ? u_xlat4.xyz : u_xlat1.xyz;
        u_xlat4.xyz = u_xlat4.xyz + (-_pad0.xyz);
        u_xlat4.xyz = u_xlat0.xyz * _pad112.xxx + u_xlat4.xyz;
        u_xlat4.xyz = u_xlat3.xyz * _pad112.yyy + u_xlat4.xyz;
        u_xlat5.xyz = u_xlat4.xyz * _pad32.www;
        u_xlat5.xyz = floor(u_xlat5.xyz);
        u_xlatb6.xyz = _g_greaterThanEqual(u_xlat5.xyzx, _pad16.xyzx).xyz;
        u_xlatb59 = u_xlatb6.y && u_xlatb6.x;
        u_xlatb59 = u_xlatb6.z && u_xlatb59;
        u_xlatb6.xyz = _g_greaterThanEqual(_pad32.xyzx, u_xlat5.xyzx).xyz;
        u_xlatb60 = u_xlatb6.y && u_xlatb6.x;
        u_xlatb60 = u_xlatb6.z && u_xlatb60;
        u_xlatb59 = u_xlatb59 && u_xlatb60;
        if(u_xlatb59){
            u_xlat6.xyz = u_xlat5.xyz + (-_pad80.xyz);
            u_xlatu6.xyz = uint3(u_xlat6.xyz);
            u_xlati59 = int(u_xlatu6.y) * _g_floatBitsToInt(_pad96.x) + int(u_xlatu6.x);
            u_xlati59 = int(u_xlatu6.z) * _g_floatBitsToInt(_pad96.y) + u_xlati59;
            u_xlatu6.xyz = uint3(_Structured_t3_buf[u_xlati59].value[(0 >> 2) + 0], _Structured_t3_buf[u_xlati59].value[(0 >> 2) + 1], _Structured_t3_buf[u_xlati59].value[(0 >> 2) + 2]);
            u_xlatu7.x = uint(u_xlatu6.x >> 29u);
            u_xlatu26.xz = uint2(u_xlatu6.y >> 10u, u_xlatu6.z >> 10u);
            u_xlatu26.y = uint(u_xlatu6.y >> 20u);
            u_xlat59 = float(u_xlatu7.x);
            u_xlat59 = u_xlat59 * 1.58496249;
            u_xlat59 = exp2(u_xlat59);
            u_xlat59 = _g_roundEven(u_xlat59);
            u_xlatu8.xyz = uint3(u_xlatu6.x & uint(536870911u), u_xlatu6.y & uint(1023u), u_xlatu6.z & uint(1023u));
            u_xlatu7.xyz = uint3(u_xlatu26.x & uint(1023u), u_xlatu26.y & uint(1023u), u_xlatu26.z & uint(1023u));
            u_xlatu60 = uint(u_xlatu6.z >> 20u);
            u_xlatu9.z = uint(u_xlatu60 & 1023u);
            u_xlatb60 = int(u_xlatu6.x)!=int(0xFFFFFFFFu);
            u_xlat5.xyz = (-u_xlat5.xyz) * _pad16.www + u_xlat4.xyz;
            u_xlat59 = u_xlat59 * _pad48.w;
            u_xlat5.xyz = u_xlat5.xyz / float3(u_xlat59);
            u_xlat5.xyz = floor(u_xlat5.xyz);
            u_xlatu5.xyz = uint3(u_xlat5.xyz);
            u_xlatu5.xyz = min(u_xlatu5.xyz, uint3(26u, 26u, 26u));
            u_xlati6.x = 0 - int(u_xlatu8.y);
            u_xlati6.yz = 0 - int2(u_xlatu7.xy);
            u_xlatu5.xyz = u_xlatu5.xyz + uint3(u_xlati6.xyz);
            u_xlatu9.x = u_xlatu8.z;
            u_xlatu9.y = u_xlatu7.z;
            u_xlatb6.xyz = _g_lessThan(u_xlatu5.xyzx, u_xlatu9.xyzx).xyz;
            u_xlatb59 = u_xlatb6.y && u_xlatb6.x;
            u_xlatb59 = u_xlatb6.z && u_xlatb59;
            if(u_xlatb59){
                u_xlati59 = int(u_xlatu7.z) * int(u_xlatu8.z);
                u_xlati61 = int(u_xlatu5.x) * int(u_xlatu7.z) + int(u_xlatu5.y);
                u_xlati59 = int(u_xlatu5.z) * u_xlati59 + u_xlati61;
                u_xlati59 = int(u_xlatu8.x) * 243 + u_xlati59;
                u_xlatu5.x = _Structured_t2_buf[u_xlati59].value[(0 >> 2) + 0];
            } else {
                u_xlatu5.x = 4294967295u;
            }
            u_xlatu59 = (u_xlatb60) ? u_xlatu5.x : 4294967295u;
        } else {
            u_xlatu59 = 4294967295u;
        }
        u_xlati60 = int((int(u_xlatu59)!=int(0xFFFFFFFFu)) ? 0xFFFFFFFFu : uint(0));
        u_xlati61 = op_not(u_xlati60);
        if(u_xlati60 != 0) {
            u_xlatu5.x = uint(u_xlatu59 >> 28u);
            u_xlatu59 = uint(u_xlatu59 & 268435455u);
            u_xlat59 = float(u_xlatu59);
            u_xlat24.x = u_xlat59 * _pad64.w;
            u_xlat6.z = floor(u_xlat24.x);
            u_xlat24.x = _pad48.y * _pad48.x;
            u_xlat59 = (-u_xlat6.z) * u_xlat24.x + u_xlat59;
            u_xlat24.x = u_xlat59 * _pad64.x;
            u_xlat6.y = floor(u_xlat24.x);
            u_xlat59 = (-u_xlat6.y) * _pad48.x + u_xlat59;
            u_xlat6.x = floor(u_xlat59);
            u_xlat59 = float(u_xlatu5.x);
            u_xlat59 = u_xlat59 * 1.58496249;
            u_xlat59 = exp2(u_xlat59);
            u_xlat59 = u_xlat59 * _pad48.w;
            u_xlat4.xyz = u_xlat4.xyz / float3(u_xlat59);
            u_xlat4.xyz = _g_fract(u_xlat4.xyz);
            u_xlat5.xyz = u_xlat6.xyz + float3(0.5, 0.5, 0.5);
            u_xlat4.xyz = u_xlat4.xyz * float3(3.0, 3.0, 3.0) + u_xlat5.xyz;
            u_xlat4.xyz = u_xlat4.xyz * _pad64.xyz;
            u_xlatb59 = _g_floatBitsToInt(_pad96.z)==2;
            if(u_xlatb59){
                u_xlat5.xyz = u_xlat4.xyz * _pad48.xyz + float3(-0.5, -0.5, -0.5);
                u_xlat6.xyz = _g_fract(u_xlat5.xyz);
                u_xlatu5.xyz =  uint3(int3(u_xlat5.xyz));
                u_xlatu5.w = uint(0u);
                u_xlat5 = _g_texelFetch(_Texture_t9, int3(u_xlatu5.xyz), int(u_xlatu5.w));
                u_xlatu59 = uint(_pad0.w);
                u_xlatb59 = int(u_xlatu59)==1;
                u_xlat24.x = u_xlat5.x * 255.0;
                u_xlatu24.x = uint(u_xlat24.x);
                u_xlati43 = int(uint(uint(_g_floatBitsToUint(_pad144.y)) | uint(_g_floatBitsToUint(_pad144.x))));
                u_xlati43 = int(uint(uint(u_xlati43) | uint(_g_floatBitsToUint(_pad144.z))));
                u_xlati43 = int(uint(uint(u_xlati43) | uint(_g_floatBitsToUint(_pad144.w))));
                u_xlati43 = int(uint(uint(u_xlati43) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
                u_xlat43 = (u_xlati43 != 0) ? unity_RenderingLayer.x : _g_intBitsToFloat(int(0xFFFFFFFFu));
                u_xlati7 = int4(uint4(uint(_g_floatBitsToUint(float(u_xlat43))) & uint(_g_floatBitsToUint(_pad144.x)), uint(_g_floatBitsToUint(float(u_xlat43))) & uint(_g_floatBitsToUint(_pad144.y)), uint(_g_floatBitsToUint(float(u_xlat43))) & uint(_g_floatBitsToUint(_pad144.z)), uint(_g_floatBitsToUint(float(u_xlat43))) & uint(_g_floatBitsToUint(_pad144.w))));
                u_xlat43 = (u_xlati7.x != 0) ? u_xlat5.x : 0.0;
                u_xlatu62 = uint(uint(_g_floatBitsToUint(u_xlat5.x)) >> 8u);
                u_xlatu63 = uint(uint(_g_floatBitsToUint(u_xlat5.x)) >> 16u);
                u_xlatu5.x = uint(uint(_g_floatBitsToUint(u_xlat5.x)) >> 24u);
                u_xlat62 = _g_uintBitsToFloat(uint(u_xlatu62 | uint(_g_floatBitsToUint(u_xlat43))));
                u_xlat43 = (u_xlati7.y != 0) ? u_xlat62 : u_xlat43;
                u_xlat62 = _g_uintBitsToFloat(uint(u_xlatu63 | uint(_g_floatBitsToUint(u_xlat43))));
                u_xlat43 = (u_xlati7.z != 0) ? u_xlat62 : u_xlat43;
                u_xlat5.x = _g_uintBitsToFloat(uint(u_xlatu5.x | uint(_g_floatBitsToUint(u_xlat43))));
                u_xlat5.x = (u_xlati7.w != 0) ? u_xlat5.x : u_xlat43;
                u_xlatu5.x = uint(uint(_g_floatBitsToUint(u_xlat5.x)) & 255u);
                u_xlatu59 = (u_xlatb59) ? u_xlatu24.x : u_xlatu5.x;
                u_xlatu5.x = _Structured_t5_buf[u_xlatu59].value[(0 >> 2) + 0];
                u_xlatu7 = uint4(u_xlatu5.x & uint(4u), u_xlatu5.x & uint(2u), u_xlatu5.x & uint(32u), u_xlatu5.x & uint(16u));
                u_xlat7 = float4(u_xlatu7);
                u_xlat7 = min(u_xlat7, float4(1.0, 1.0, 1.0, 1.0));
                u_xlat24.xy = u_xlat7.yw * u_xlat6.xy + (-u_xlat6.xy);
                u_xlat7.xy = u_xlat24.xy + u_xlat7.xz;
                u_xlatu24.xy = uint2(u_xlatu5.x & uint(256u), u_xlatu5.x & uint(128u));
                u_xlat24.xy = float2(u_xlatu24.xy);
                u_xlat24.xy = min(u_xlat24.xy, float2(1.0, 1.0));
                u_xlat43 = u_xlat24.y * u_xlat6.z + (-u_xlat6.z);
                u_xlat7.z = u_xlat43 + u_xlat24.x;
                u_xlat24.xyz = u_xlat7.xyz * _pad64.xyz + u_xlat4.xyz;
                u_xlat7 = _g_textureLod(_Texture_t6, u_xlat24.xyz, 0.0).wxyz;
                u_xlat8 = _g_textureLod(_Texture_t7, u_xlat24.xyz, 0.0);
                u_xlat9 = _g_textureLod(_Texture_t8, u_xlat24.xyz, 0.0);
                u_xlatb10.xy = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), _pad128.zwzz).xy;
                if(u_xlatb10.x){
                    u_xlat11 = _g_textureLod(_Texture_t10, u_xlat24.xyz, 0.0);
                } else {
                    u_xlat11.x = float(0.0);
                    u_xlat11.y = float(0.0);
                    u_xlat11.z = float(0.0);
                    u_xlat11.w = float(0.0);
                }
                if(u_xlatb10.y){
                    u_xlat24.xyz = u_xlat24.xyz * _pad48.xyz + float3(-0.5, -0.5, -0.5);
                    u_xlatu12.xyz =  uint3(int3(u_xlat24.xyz));
                    u_xlatu12.w = uint(0u);
                    u_xlat12 = _g_texelFetch(_Texture_t11, int3(u_xlatu12.xyz), int(u_xlatu12.w));
                    u_xlat24.x = u_xlat12.x * 255.0;
                    u_xlatu24.x = uint(u_xlat24.x);
                    u_xlatb43 = int(u_xlatu24.x)==255;
                    u_xlat12.xyz = float3(_g_uintBitsToFloat(_Structured_t4_buf[u_xlatu24.x].value[(0 >> 2) + 0]), _g_uintBitsToFloat(_Structured_t4_buf[u_xlatu24.x].value[(0 >> 2) + 1]), _g_uintBitsToFloat(_Structured_t4_buf[u_xlatu24.x].value[(0 >> 2) + 2]));
                    u_xlat24.xyz = (bool(u_xlatb43)) ? float3(0.0, 0.0, 0.0) : u_xlat12.xyz;
                } else {
                    u_xlat24.x = float(0.0);
                    u_xlat24.y = float(0.0);
                    u_xlat24.z = float(0.0);
                }
                u_xlatb59 = int(u_xlatu59)!=255;
                if(u_xlatb59){
                    u_xlatu12.x = uint(u_xlatu5.x >> 1u);
                    u_xlatu12.y = uint(u_xlatu5.x >> 4u);
                    u_xlatu12.z = uint(u_xlatu5.x >> 7u);
                    u_xlatu12.w = uint(u_xlatu5.x >> 9u);
                    u_xlatu13 = uint4(u_xlatu12.x & uint(3u), u_xlatu12.y & uint(3u), u_xlatu12.z & uint(3u), u_xlatu12.w & uint(1u));
                    u_xlatu29.xyz = uint3(u_xlatu5.x & uint(1u), u_xlatu5.x & uint(8u), u_xlatu5.x & uint(64u));
                    u_xlat29.xyz = float3(u_xlatu29.xyz);
                    u_xlati12.xyz = int3(u_xlatu13.xyz) + int3(int(0xFFFFFFFFu), int(0xFFFFFFFFu), int(0xFFFFFFFFu));
                    u_xlat12.xyz = float3(u_xlati12.xyz);
                    u_xlat59 = u_xlat12.x * u_xlat6.x + u_xlat29.x;
                    u_xlat29.xy = min(u_xlat29.yz, float2(1.0, 1.0));
                    u_xlat29.xy = u_xlat12.yz * u_xlat6.yz + u_xlat29.xy;
                    u_xlat59 = u_xlat59 * u_xlat29.x;
                    u_xlat12.x = u_xlat29.y * u_xlat59;
                    u_xlatu13.x = uint(u_xlatu12.w >> 1u);
                    u_xlatu13.y = uint(u_xlatu12.w >> 4u);
                    u_xlatu13.z = uint(u_xlatu12.w >> 7u);
                    u_xlati13.xyz = int3(uint3(u_xlatu13.x & uint(3u), u_xlatu13.y & uint(3u), u_xlatu13.z & uint(3u)));
                    u_xlat63 = float(u_xlatu13.w);
                    u_xlati13.xyz = u_xlati13.xyz + int3(int(0xFFFFFFFFu), int(0xFFFFFFFFu), int(0xFFFFFFFFu));
                    u_xlat13.xyz = float3(u_xlati13.xyz);
                    u_xlat63 = u_xlat13.x * u_xlat6.x + u_xlat63;
                    u_xlatu29.xz = uint2(u_xlatu12.w & uint(8u), u_xlatu12.w & uint(64u));
                    u_xlat29.xz = float2(u_xlatu29.xz);
                    u_xlat29.xz = min(u_xlat29.xz, float2(1.0, 1.0));
                    u_xlat29.xz = u_xlat13.yz * u_xlat6.yz + u_xlat29.xz;
                    u_xlat63 = u_xlat63 * u_xlat29.x;
                    u_xlat12.y = u_xlat29.z * u_xlat63;
                    u_xlatu5.x = uint(u_xlatu5.x >> 18u);
                    u_xlatu13.x = uint(u_xlatu5.x >> 1u);
                    u_xlatu13.y = uint(u_xlatu5.x >> 4u);
                    u_xlatu13.z = uint(u_xlatu5.x >> 7u);
                    u_xlati13.xyz = int3(uint3(u_xlatu13.x & uint(3u), u_xlatu13.y & uint(3u), u_xlatu13.z & uint(3u)));
                    u_xlatu14.xyz = uint3(u_xlatu5.x & uint(1u), u_xlatu5.x & uint(8u), u_xlatu5.x & uint(64u));
                    u_xlat14.xyz = float3(u_xlatu14.xyz);
                    u_xlati13.xyz = u_xlati13.xyz + int3(int(0xFFFFFFFFu), int(0xFFFFFFFFu), int(0xFFFFFFFFu));
                    u_xlat13.xyz = float3(u_xlati13.xyz);
                    u_xlat63 = u_xlat13.x * u_xlat6.x + u_xlat14.x;
                    u_xlat29.xz = min(u_xlat14.yz, float2(1.0, 1.0));
                    u_xlat29.xz = u_xlat13.yz * u_xlat6.yz + u_xlat29.xz;
                    u_xlat63 = u_xlat63 * u_xlat29.x;
                    u_xlat12.z = u_xlat29.z * u_xlat63;
                    u_xlat59 = u_xlat59 * u_xlat29.y + u_xlat12.y;
                    u_xlat59 = u_xlat63 * u_xlat29.z + u_xlat59;
                    u_xlat59 = max(u_xlat59, 9.99999975e-05);
                    u_xlat59 = float(1.0) / u_xlat59;
                    u_xlat29.xyz = float3(u_xlat59) * u_xlat12.xyz;
                    u_xlat12.xyz = u_xlat7.yzw * u_xlat29.xxx;
                    u_xlat7.y = u_xlat8.w;
                    u_xlat7.z = u_xlat9.w;
                    u_xlat7.xyz = u_xlat29.xxx * u_xlat7.xyz;
                    u_xlat8.xyz = u_xlat8.xyz * u_xlat29.xxx;
                    u_xlat9.xyz = u_xlat9.xyz * u_xlat29.xxx;
                    u_xlat11 = u_xlat29.xxxx * u_xlat11;
                    u_xlatb13.xy = _g_notEqual(u_xlat29.yzyy, float4(0.0, 0.0, 0.0, 0.0)).xy;
                    if(u_xlatb13.x){
                        u_xlatu14 = uint4(u_xlatu12.w & uint(4u), u_xlatu12.w & uint(2u), u_xlatu12.w & uint(32u), u_xlatu12.w & uint(16u));
                        u_xlat14 = float4(u_xlatu14);
                        u_xlat14 = min(u_xlat14, float4(1.0, 1.0, 1.0, 1.0));
                        u_xlat13.xz = u_xlat14.yw * u_xlat6.xy + (-u_xlat6.xy);
                        u_xlat14.xy = u_xlat13.xz + u_xlat14.xz;
                        u_xlatu13.xz = uint2(u_xlatu12.w & uint(256u), u_xlatu12.w & uint(128u));
                        u_xlat13.xz = float2(u_xlatu13.xz);
                        u_xlat13.xz = min(u_xlat13.xz, float2(1.0, 1.0));
                        u_xlat59 = u_xlat13.z * u_xlat6.z + (-u_xlat6.z);
                        u_xlat14.z = u_xlat59 + u_xlat13.x;
                        u_xlat13.xzw = u_xlat14.xyz * _pad64.xyz + u_xlat4.xyz;
                        u_xlat14 = _g_textureLod(_Texture_t6, u_xlat13.xzw, 0.0);
                        u_xlat15 = _g_textureLod(_Texture_t7, u_xlat13.xzw, 0.0);
                        u_xlat16 = _g_textureLod(_Texture_t8, u_xlat13.xzw, 0.0);
                        if(u_xlatb10.x){
                            u_xlat17 = _g_textureLod(_Texture_t10, u_xlat13.xzw, 0.0);
                        } else {
                            u_xlat17.x = float(0.0);
                            u_xlat17.y = float(0.0);
                            u_xlat17.z = float(0.0);
                            u_xlat17.w = float(0.0);
                        }
                        u_xlat18.x = u_xlat14.w;
                        u_xlat18.y = u_xlat15.w;
                        u_xlat18.z = u_xlat16.w;
                        u_xlat12.xyz = u_xlat14.xyz * u_xlat29.yyy + u_xlat12.xyz;
                        u_xlat7.xyz = u_xlat18.xyz * u_xlat29.yyy + u_xlat7.xyz;
                        u_xlat8.xyz = u_xlat15.xyz * u_xlat29.yyy + u_xlat8.xyz;
                        u_xlat9.xyz = u_xlat16.xyz * u_xlat29.yyy + u_xlat9.xyz;
                        u_xlat11 = u_xlat17 * u_xlat29.yyyy + u_xlat11;
                    }
                    if(u_xlatb13.y){
                        u_xlatu13 = uint4(u_xlatu5.x & uint(4u), u_xlatu5.x & uint(2u), u_xlatu5.x & uint(32u), u_xlatu5.x & uint(16u));
                        u_xlat13 = float4(u_xlatu13);
                        u_xlat13 = min(u_xlat13, float4(1.0, 1.0, 1.0, 1.0));
                        u_xlat6.xy = u_xlat13.yw * u_xlat6.xy + (-u_xlat6.xy);
                        u_xlat13.xy = u_xlat6.xy + u_xlat13.xz;
                        u_xlatu6.xy = uint2(u_xlatu5.x & uint(256u), u_xlatu5.x & uint(128u));
                        u_xlat6.xy = float2(u_xlatu6.xy);
                        u_xlat6.xy = min(u_xlat6.xy, float2(1.0, 1.0));
                        u_xlat59 = u_xlat6.y * u_xlat6.z + (-u_xlat6.z);
                        u_xlat13.z = u_xlat59 + u_xlat6.x;
                        u_xlat6.xyz = u_xlat13.xyz * _pad64.xyz + u_xlat4.xyz;
                        u_xlat13 = _g_textureLod(_Texture_t6, u_xlat6.xyz, 0.0);
                        u_xlat14 = _g_textureLod(_Texture_t7, u_xlat6.xyz, 0.0);
                        u_xlat15 = _g_textureLod(_Texture_t8, u_xlat6.xyz, 0.0);
                        if(u_xlatb10.x){
                            u_xlat6 = _g_textureLod(_Texture_t10, u_xlat6.xyz, 0.0);
                        } else {
                            u_xlat6.x = float(0.0);
                            u_xlat6.y = float(0.0);
                            u_xlat6.z = float(0.0);
                            u_xlat6.w = float(0.0);
                        }
                        u_xlat10.x = u_xlat13.w;
                        u_xlat10.y = u_xlat14.w;
                        u_xlat10.z = u_xlat15.w;
                        u_xlat12.xyz = u_xlat13.xyz * u_xlat29.zzz + u_xlat12.xyz;
                        u_xlat7.xyz = u_xlat10.xyz * u_xlat29.zzz + u_xlat7.xyz;
                        u_xlat8.xyz = u_xlat14.xyz * u_xlat29.zzz + u_xlat8.xyz;
                        u_xlat9.xyz = u_xlat15.xyz * u_xlat29.zzz + u_xlat9.xyz;
                        u_xlat11 = u_xlat6 * u_xlat29.zzzz + u_xlat11;
                    }
                } else {
                    u_xlat12.xyz = u_xlat7.yzw;
                    u_xlat7.y = u_xlat8.w;
                    u_xlat7.z = u_xlat9.w;
                }
            } else {
                u_xlatb59 = _g_floatBitsToInt(_pad96.z)==1;
                if(u_xlatb59){
                    u_xlat6.xyz = u_xlat4.xyz * _pad48.xyz + float3(-0.5, -0.5, -0.5);
                    u_xlatu10.xyz =  uint3(int3(u_xlat6.xyz));
                    u_xlatu10.w = uint(0u);
                    u_xlat10 = _g_texelFetch(_Texture_t9, int3(u_xlatu10.xyz), int(u_xlatu10.w));
                    u_xlatu59 = uint(_pad0.w);
                    u_xlatb59 = int(u_xlatu59)==1;
                    u_xlat5.x = u_xlat10.x * 255.0;
                    u_xlatu5.x = uint(u_xlat5.x);
                    u_xlati63 = int(uint(uint(_g_floatBitsToUint(_pad144.y)) | uint(_g_floatBitsToUint(_pad144.x))));
                    u_xlati63 = int(uint(uint(u_xlati63) | uint(_g_floatBitsToUint(_pad144.z))));
                    u_xlati63 = int(uint(uint(u_xlati63) | uint(_g_floatBitsToUint(_pad144.w))));
                    u_xlati63 = int(uint(uint(u_xlati63) & uint(_g_floatBitsToUint(unity_RenderingLayer.x))));
                    u_xlat63 = (u_xlati63 != 0) ? unity_RenderingLayer.x : _g_intBitsToFloat(int(0xFFFFFFFFu));
                    u_xlati13 = int4(uint4(uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_pad144.x)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_pad144.y)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_pad144.z)), uint(_g_floatBitsToUint(float(u_xlat63))) & uint(_g_floatBitsToUint(_pad144.w))));
                    u_xlat63 = (u_xlati13.x != 0) ? u_xlat10.x : 0.0;
                    u_xlatu29.x = uint(uint(_g_floatBitsToUint(u_xlat10.x)) >> 8u);
                    u_xlatu48 = uint(uint(_g_floatBitsToUint(u_xlat10.x)) >> 16u);
                    u_xlatu10.x = uint(uint(_g_floatBitsToUint(u_xlat10.x)) >> 24u);
                    u_xlat29.x = _g_uintBitsToFloat(uint(uint(_g_floatBitsToUint(u_xlat63)) | u_xlatu29.x));
                    u_xlat63 = (u_xlati13.y != 0) ? u_xlat29.x : u_xlat63;
                    u_xlat29.x = _g_uintBitsToFloat(uint(u_xlatu48 | uint(_g_floatBitsToUint(u_xlat63))));
                    u_xlat63 = (u_xlati13.z != 0) ? u_xlat29.x : u_xlat63;
                    u_xlat10.x = _g_uintBitsToFloat(uint(u_xlatu10.x | uint(_g_floatBitsToUint(u_xlat63))));
                    u_xlat63 = (u_xlati13.w != 0) ? u_xlat10.x : u_xlat63;
                    u_xlatu63 = uint(uint(_g_floatBitsToUint(u_xlat63)) & 255u);
                    u_xlatu59 = (u_xlatb59) ? u_xlatu5.x : u_xlatu63;
                    u_xlatb5 = int(u_xlatu59)!=255;
                    if(u_xlatb5){
                        u_xlat6.xyz = _g_fract(u_xlat6.xyz);
                        u_xlat10.xyz = (-u_xlat6.xyz) + float3(1.0, 1.0, 1.0);
                        u_xlat5.x = u_xlat10.y * u_xlat10.x;
                        u_xlat63 = u_xlat10.z * u_xlat5.x;
                        u_xlatu13 = uint4(uint(u_xlatu59) & uint(1u), uint(u_xlatu59) & uint(2u), uint(u_xlatu59) & uint(4u), uint(u_xlatu59) & uint(8u));
                        u_xlat67 = float(int(u_xlatu13.x));
                        u_xlat14 = u_xlat6.xxyy * u_xlat10.yyxx;
                        u_xlat15 = u_xlat10.zzzz * u_xlat14.yyww;
                        u_xlatu13.xyz = min(u_xlatu13.yzw, uint3(1u, 1u, 1u));
                        u_xlat13.xyz = float3(int3(u_xlatu13.xyz));
                        u_xlat16 = u_xlat13.xxyy * u_xlat15;
                        u_xlat63 = u_xlat63 * u_xlat67 + u_xlat16.y;
                        u_xlat63 = u_xlat15.w * u_xlat13.y + u_xlat63;
                        u_xlat10.x = u_xlat6.y * u_xlat6.x;
                        u_xlat29.x = u_xlat10.z * u_xlat10.x;
                        u_xlat48 = u_xlat13.z * u_xlat29.x;
                        u_xlat63 = u_xlat29.x * u_xlat13.z + u_xlat63;
                        u_xlat5.x = u_xlat6.z * u_xlat5.x;
                        u_xlatu13 = uint4(uint(u_xlatu59) & uint(16u), uint(u_xlatu59) & uint(32u), uint(u_xlatu59) & uint(64u), uint(u_xlatu59) & uint(128u));
                        u_xlatu13 = min(u_xlatu13, uint4(1u, 1u, 1u, 1u));
                        u_xlat13 = float4(int4(u_xlatu13));
                        u_xlat59 = u_xlat5.x * u_xlat13.x;
                        u_xlat5.x = u_xlat5.x * u_xlat13.x + u_xlat63;
                        u_xlat14 = u_xlat6.zzzz * u_xlat14;
                        u_xlat15 = u_xlat13.yyzz * u_xlat14;
                        u_xlat5.x = u_xlat14.y * u_xlat13.y + u_xlat5.x;
                        u_xlat5.x = u_xlat14.w * u_xlat13.z + u_xlat5.x;
                        u_xlat63 = u_xlat6.z * u_xlat10.x;
                        u_xlat10.x = u_xlat13.w * u_xlat63;
                        u_xlat5.x = u_xlat63 * u_xlat13.w + u_xlat5.x;
                        u_xlat5.x = max(u_xlat5.x, 9.99999975e-05);
                        u_xlat5.x = float(1.0) / u_xlat5.x;
                        u_xlat13 = u_xlat5.xxxx * u_xlat16;
                        u_xlat6.xyz = u_xlat13.xyy * float3(1.0, 0.0, 0.0) + (-u_xlat6.xyz);
                        u_xlat6.xyz = u_xlat13.zwz * float3(0.0, 1.0, 0.0) + u_xlat6.xyz;
                        u_xlat63 = u_xlat5.x * u_xlat48;
                        u_xlat6.xyz = float3(u_xlat63) * float3(1.0, 1.0, 0.0) + u_xlat6.xyz;
                        u_xlat59 = u_xlat59 * u_xlat5.x;
                        u_xlat6.xyz = float3(u_xlat59) * float3(0.0, 0.0, 1.0) + u_xlat6.xyz;
                        u_xlat13 = u_xlat5.xxxx * u_xlat15;
                        u_xlat6.xyz = u_xlat13.xyx * float3(1.0, 0.0, 1.0) + u_xlat6.xyz;
                        u_xlat6.xyz = u_xlat13.zww * float3(0.0, 1.0, 1.0) + u_xlat6.xyz;
                        u_xlat6.xyz = u_xlat10.xxx * u_xlat5.xxx + u_xlat6.xyz;
                        u_xlat4.xyz = u_xlat6.xyz * _pad64.xyz + u_xlat4.xyz;
                    }
                }
                u_xlat7 = _g_textureLod(_Texture_t6, u_xlat4.xyz, 0.0).wxyz;
                u_xlat8 = _g_textureLod(_Texture_t7, u_xlat4.xyz, 0.0);
                u_xlat9 = _g_textureLod(_Texture_t8, u_xlat4.xyz, 0.0);
                u_xlatb6.xy = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), _pad128.zwzz).xy;
                if(u_xlatb6.x){
                    u_xlat11 = _g_textureLod(_Texture_t10, u_xlat4.xyz, 0.0);
                } else {
                    u_xlat11.x = float(0.0);
                    u_xlat11.y = float(0.0);
                    u_xlat11.z = float(0.0);
                    u_xlat11.w = float(0.0);
                }
                if(u_xlatb6.y){
                    u_xlat4.xyz = u_xlat4.xyz * _pad48.xyz + float3(-0.5, -0.5, -0.5);
                    u_xlatu6.xyz =  uint3(int3(u_xlat4.xyz));
                    u_xlatu6.w = uint(0u);
                    u_xlat6 = _g_texelFetch(_Texture_t11, int3(u_xlatu6.xyz), int(u_xlatu6.w));
                    u_xlat59 = u_xlat6.x * 255.0;
                    u_xlatu59 = uint(u_xlat59);
                    u_xlatb4 = int(u_xlatu59)==255;
                    u_xlat6.xyz = float3(_g_uintBitsToFloat(_Structured_t4_buf[u_xlatu59].value[(0 >> 2) + 0]), _g_uintBitsToFloat(_Structured_t4_buf[u_xlatu59].value[(0 >> 2) + 1]), _g_uintBitsToFloat(_Structured_t4_buf[u_xlatu59].value[(0 >> 2) + 2]));
                    u_xlat24.xyz = (bool(u_xlatb4)) ? float3(0.0, 0.0, 0.0) : u_xlat6.xyz;
                } else {
                    u_xlat24.x = float(0.0);
                    u_xlat24.y = float(0.0);
                    u_xlat24.z = float(0.0);
                }
                u_xlat12.xyz = u_xlat7.yzw;
                u_xlat7.y = u_xlat8.w;
                u_xlat7.z = u_xlat9.w;
            }
        } else {
            u_xlat11.x = float(0.0);
            u_xlat11.y = float(0.0);
            u_xlat11.z = float(0.0);
            u_xlat11.w = float(0.0);
            u_xlat12.x = float(0.0);
            u_xlat12.y = float(0.0);
            u_xlat12.z = float(0.0);
            u_xlat7.x = float(0.0);
            u_xlat7.y = float(0.0);
            u_xlat7.z = float(0.0);
            u_xlat8.x = float(0.0);
            u_xlat8.y = float(0.0);
            u_xlat8.z = float(0.0);
            u_xlat9.x = float(0.0);
            u_xlat9.y = float(0.0);
            u_xlat9.z = float(0.0);
            u_xlat24.x = float(0.0);
            u_xlat24.y = float(0.0);
            u_xlat24.z = float(0.0);
        }
        u_xlatb59 = u_xlati61!=int(0xFFFFFFFFu);
        if(u_xlatb59){
            u_xlat4.xyz = u_xlat7.xyz + float3(-0.5, -0.5, -0.5);
            u_xlat6.xyz = u_xlat12.xyz * float3(4.0, 4.0, 4.0);
            u_xlat4.xyz = u_xlat4.xyz * u_xlat6.xxx;
            u_xlat10.xyz = u_xlat8.xyz + float3(-0.5, -0.5, -0.5);
            u_xlat6.xyw = u_xlat6.yyy * u_xlat10.xyz;
            u_xlat10.xyz = u_xlat9.xyz + float3(-0.5, -0.5, -0.5);
            u_xlat10.xyz = u_xlat6.zzz * u_xlat10.xyz;
            u_xlat4.xyz = (int(u_xlati60) != 0) ? u_xlat4.xyz : u_xlat7.xyz;
            u_xlat6.xyz = (int(u_xlati60) != 0) ? u_xlat6.xyw : u_xlat8.xyz;
            u_xlat7.xyz = (int(u_xlati60) != 0) ? u_xlat10.xyz : u_xlat9.xyz;
            u_xlat4.x = dot(u_xlat4.xyz, u_xlat0.xyz);
            u_xlat4.y = dot(u_xlat6.xyz, u_xlat0.xyz);
            u_xlat4.z = dot(u_xlat7.xyz, u_xlat0.xyz);
            u_xlat4.xyz = u_xlat12.xyz + u_xlat4.xyz;
            u_xlat6.yzw = u_xlat0.xyz * float3(0.488602519, 0.488602519, 0.488602519);
            u_xlat6.x = 0.282094806;
            u_xlat59 = dot(u_xlat6, u_xlat11);
            u_xlat59 = u_xlat59 * _pad128.z;
            u_xlatb6.xy = _g_lessThan(float4(0.0, 0.0, 0.0, 0.0), _pad128.zwzz).xy;
            u_xlat60 = dot(u_xlat24.xyz, u_xlat24.xyz);
            u_xlatb61 = u_xlat60<0.200000003;
            u_xlat60 = _g_inversesqrt(u_xlat60);
            u_xlat5.xyz = float3(u_xlat60) * u_xlat24.xyz;
            u_xlat5.xyz = (bool(u_xlatb61)) ? u_xlat0.xyz : u_xlat5.xyz;
            u_xlat5.xyz = (u_xlatb6.y) ? u_xlat5.xyz : u_xlat0.xyz;
            u_xlat5.w = 1.0;
            u_xlat7.x = dot(_pad432, u_xlat5);
            u_xlat7.y = dot(_pad448, u_xlat5);
            u_xlat7.z = dot(_pad464, u_xlat5);
            u_xlat8 = u_xlat5.yzzx * u_xlat5.xyzz;
            u_xlat9.x = dot(_pad480, u_xlat8);
            u_xlat9.y = dot(_pad496, u_xlat8);
            u_xlat9.z = dot(_pad512, u_xlat8);
            u_xlat60 = u_xlat5.y * u_xlat5.y;
            u_xlat60 = u_xlat5.x * u_xlat5.x + (-u_xlat60);
            u_xlat5.xyz = _pad528.xyz * float3(u_xlat60) + u_xlat9.xyz;
            u_xlat5.xyz = u_xlat5.xyz + u_xlat7.xyz;
            u_xlat5.xyz = float3(u_xlat59) * u_xlat5.xyz + u_xlat4.xyz;
            u_xlat4.xyz = (u_xlatb6.x) ? u_xlat5.xyz : u_xlat4.xyz;
            u_xlat4.xyz = u_xlat4.xyz * _pad128.yyy;
        } else {
            u_xlat0.w = 1.0;
            u_xlat5.x = dot(_pad432, u_xlat0);
            u_xlat5.y = dot(_pad448, u_xlat0);
            u_xlat5.z = dot(_pad464, u_xlat0);
            u_xlat6 = u_xlat0.yzzx * u_xlat0.xyzz;
            u_xlat7.x = dot(_pad480, u_xlat6);
            u_xlat7.y = dot(_pad496, u_xlat6);
            u_xlat7.z = dot(_pad512, u_xlat6);
            u_xlat59 = u_xlat0.y * u_xlat0.y;
            u_xlat59 = u_xlat0.x * u_xlat0.x + (-u_xlat59);
            u_xlat6.xyz = _pad528.xyz * float3(u_xlat59) + u_xlat7.xyz;
            u_xlat4.xyz = u_xlat5.xyz + u_xlat6.xyz;
        }
    } else {
        u_xlat0.w = 1.0;
        u_xlat5.x = dot(_pad432, u_xlat0);
        u_xlat5.y = dot(_pad448, u_xlat0);
        u_xlat5.z = dot(_pad464, u_xlat0);
        u_xlat6 = u_xlat0.yzzx * u_xlat0.xyzz;
        u_xlat7.x = dot(_pad480, u_xlat6);
        u_xlat7.y = dot(_pad496, u_xlat6);
        u_xlat7.z = dot(_pad512, u_xlat6);
        u_xlat57 = u_xlat0.y * u_xlat0.y;
        u_xlat57 = u_xlat0.x * u_xlat0.x + (-u_xlat57);
        u_xlat6.xyz = _pad528.xyz * float3(u_xlat57) + u_xlat7.xyz;
        u_xlat4.xyz = u_xlat5.xyz + u_xlat6.xyz;
    }
    float3 txVec0 = float3(input.vs_INTERP4.xy,input.vs_INTERP4.z);
    u_xlat57 = _g_textureLod(_Texture_t1, txVec0, 0.0);
    u_xlat59 = (-_pad432.x) + 1.0;
    u_xlat57 = u_xlat57 * _pad432.x + u_xlat59;
    u_xlatb59 = 0.0>=input.vs_INTERP4.z;
    u_xlatb60 = input.vs_INTERP4.z>=1.0;
    u_xlatb59 = u_xlatb59 || u_xlatb60;
    u_xlat57 = (u_xlatb59) ? 1.0 : u_xlat57;
    u_xlat5.xyz = u_xlat1.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat59 = dot(u_xlat5.xyz, u_xlat5.xyz);
    u_xlat59 = u_xlat59 * _pad432.z + _pad432.w;
    u_xlat59 = clamp(u_xlat59, 0.0, 1.0);
    u_xlat60 = (-u_xlat57) + 1.0;
    u_xlat57 = u_xlat59 * u_xlat60 + u_xlat57;
    u_xlat59 = dot((-u_xlat3.xyz), u_xlat0.xyz);
    u_xlat59 = u_xlat59 + u_xlat59;
    u_xlat5.xyz = u_xlat0.xyz * (-float3(u_xlat59)) + (-u_xlat3.xyz);
    u_xlat59 = dot(u_xlat0.xyz, u_xlat3.xyz);
    u_xlat59 = clamp(u_xlat59, 0.0, 1.0);
    u_xlat59 = (-u_xlat59) + 1.0;
    u_xlat59 = u_xlat59 * u_xlat59;
    u_xlat59 = u_xlat59 * u_xlat59;
    u_xlat5 = _g_textureLod(_Texture_t0, u_xlat5.xyz, 6.0);
    u_xlat60 = u_xlat5.w + -1.0;
    u_xlat60 = unity_SpecCube0_HDR.w * u_xlat60 + 1.0;
    u_xlat60 = max(u_xlat60, 0.0);
    u_xlat60 = log2(u_xlat60);
    u_xlat60 = u_xlat60 * unity_SpecCube0_HDR.y;
    u_xlat60 = exp2(u_xlat60);
    u_xlat60 = u_xlat60 * unity_SpecCube0_HDR.x;
    u_xlat5.xyz = u_xlat5.xyz * float3(u_xlat60);
    u_xlat2.w = u_xlat59 * 2.23517418e-08 + 0.0399999991;
    u_xlat2 = u_xlat2 * float4(0.959999979, 0.959999979, 0.959999979, 0.5);
    u_xlat5.xyz = u_xlat2.www * u_xlat5.xyz;
    u_xlat4.xyz = u_xlat4.xyz * u_xlat2.xyz + u_xlat5.xyz;
    u_xlat57 = u_xlat57 * unity_LightData.z;
    u_xlat59 = dot(u_xlat0.xyz, _MainLightPosition.xyz);
    u_xlat59 = clamp(u_xlat59, 0.0, 1.0);
    u_xlat57 = u_xlat57 * u_xlat59;
    u_xlat5.xyz = float3(u_xlat57) * _MainLightColor.xyz;
    u_xlat6.xyz = u_xlat3.xyz + _MainLightPosition.xyz;
    u_xlat57 = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat57 = max(u_xlat57, 1.17549435e-38);
    u_xlat57 = _g_inversesqrt(u_xlat57);
    u_xlat6.xyz = float3(u_xlat57) * u_xlat6.xyz;
    u_xlat57 = dot(_MainLightPosition.xyz, u_xlat6.xyz);
    u_xlat57 = clamp(u_xlat57, 0.0, 1.0);
    u_xlat57 = u_xlat57 * u_xlat57;
    u_xlat57 = max(u_xlat57, 0.100000001);
    u_xlat57 = u_xlat57 * 6.00012016;
    u_xlat57 = float(1.0) / u_xlat57;
    u_xlat6.xyz = float3(u_xlat57) * float3(0.0399999991, 0.0399999991, 0.0399999991) + u_xlat2.xyz;
    u_xlat57 = min(_AdditionalLightsCount.x, unity_LightData.y);
    u_xlatu57 =  uint(int(u_xlat57));
    u_xlat7.x = float(0.0);
    u_xlat7.y = float(0.0);
    u_xlat7.z = float(0.0);
    for(uint u_xlatu_loop_1 = uint(0u) ; u_xlatu_loop_1<u_xlatu57 ; u_xlatu_loop_1++)
    {
        u_xlatu60 = uint(u_xlatu_loop_1 >> 2u);
        u_xlati61 = int(uint(u_xlatu_loop_1 & 3u));
        u_xlat60 = dot(unity_LightIndices, ImmCB_0_0_0[u_xlati61]);
        u_xlati60 = int(u_xlat60);
        u_xlat8.xyz = (-u_xlat1.xyz) * _AdditionalLightsPosition.www + _AdditionalLightsPosition.xyz;
        u_xlat61 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat61 = max(u_xlat61, 6.10351562e-05);
        u_xlat62 = _g_inversesqrt(u_xlat61);
        u_xlat9.xyz = float3(u_xlat62) * u_xlat8.xyz;
        u_xlat63 = float(1.0) / u_xlat61;
        u_xlat61 = u_xlat61 * _AdditionalLightsAttenuation.x;
        u_xlat61 = (-u_xlat61) * u_xlat61 + 1.0;
        u_xlat61 = max(u_xlat61, 0.0);
        u_xlat61 = u_xlat61 * u_xlat61;
        u_xlat61 = u_xlat61 * u_xlat63;
        u_xlat63 = dot(_AdditionalLightsSpotDir.xyz, u_xlat9.xyz);
        u_xlat63 = u_xlat63 * _AdditionalLightsAttenuation.z + _AdditionalLightsAttenuation.w;
        u_xlat63 = clamp(u_xlat63, 0.0, 1.0);
        u_xlat63 = u_xlat63 * u_xlat63;
        u_xlat61 = u_xlat61 * u_xlat63;
        u_xlat63 = dot(u_xlat0.xyz, u_xlat9.xyz);
        u_xlat63 = clamp(u_xlat63, 0.0, 1.0);
        u_xlat61 = u_xlat61 * u_xlat63;
        u_xlat10.xyz = float3(u_xlat61) * _AdditionalLightsColor.xyz;
        u_xlat8.xyz = u_xlat8.xyz * float3(u_xlat62) + u_xlat3.xyz;
        u_xlat60 = dot(u_xlat8.xyz, u_xlat8.xyz);
        u_xlat60 = max(u_xlat60, 1.17549435e-38);
        u_xlat60 = _g_inversesqrt(u_xlat60);
        u_xlat8.xyz = float3(u_xlat60) * u_xlat8.xyz;
        u_xlat60 = dot(u_xlat9.xyz, u_xlat8.xyz);
        u_xlat60 = clamp(u_xlat60, 0.0, 1.0);
        u_xlat60 = u_xlat60 * u_xlat60;
        u_xlat60 = max(u_xlat60, 0.100000001);
        u_xlat60 = u_xlat60 * 6.00012016;
        u_xlat60 = float(1.0) / u_xlat60;
        u_xlat8.xyz = float3(u_xlat60) * float3(0.0399999991, 0.0399999991, 0.0399999991) + u_xlat2.xyz;
        u_xlat7.xyz = u_xlat8.xyz * u_xlat10.xyz + u_xlat7.xyz;
    }
    u_xlat0.xyz = u_xlat6.xyz * u_xlat5.xyz + u_xlat4.xyz;
    u_xlat0.xyz = u_xlat7.xyz + u_xlat0.xyz;
    u_xlat57 = u_xlat58 * (-u_xlat58);
    u_xlat57 = exp2(u_xlat57);
    u_xlat1.x = (-u_xlat57) + 1.0;
    u_xlat1.xyz = u_xlat1.xxx * _pad992.xyz;
    __SV_Target0.xyz = u_xlat0.xyz * float3(u_xlat57) + u_xlat1.xyz;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
