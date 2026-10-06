Shader "Shader Graphs/SkyboxShader"
{
    Properties
    {


        _SkyPixels ("SkyPixels", Vector) = (1,1,0,0)
        [HDR] _TopDayColor ("TopDayColor", Vector) = (0,0.4660978,1,1)
        [HDR] _TopNightColor ("TopNightColor", Vector) = (0,0.4660978,1,1)
        [HDR] _BottomDayColor ("BottomDayColor", Vector) = (0.2627451,0.5764706,1,1)
        [HDR] _BottomSunriseColor ("BottomSunriseColor", Vector) = (2.996079,1.050501,0,1)
        [HDR] _BottomNightColor ("BottomNightColor", Vector) = (0.01960784,0.01960784,0.01960784,1)
        _SkyNoiseStrength ("SkyNoiseStrength", Range(0, 1)) = 1
        _SkyBands ("SkyBands", Float) = 6
        _SkyNoiseScale ("SkyNoiseScale", Vector) = (10,10,0,0)
        _SunStep ("SunStep", Range(0, 1)) = 0.5
        _SunPixels ("SunPixels", Float) = 4
        [HDR] _SunColor ("SunColor", Vector) = (1,0.9787216,0.6226415,1)
        _SunIntensity ("SunIntensity", Float) = 8
        _MoonRadius ("MoonRadius", Range(0, 100)) = 0
        [HDR] _MoonColor ("MoonColor", Vector) = (0.5660378,0.5660378,0.5660378,1)
        _MoonIntensity ("MoonIntensity", Float) = 1
        [HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl ("_QueueControl", Float) = -1
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Cull Off ZWrite Off
        Pass
        {
            Name "Unlit"


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



            float4x4 unity_MatrixVP;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4 _MainLightPosition;
            float3 _WorldSpaceCameraPos;
            float4 unity_OrthoParams;
            float4x4 unity_MatrixV;
            float4 _TopNightColor;
            float _SkyNoiseStrength;
            float4 _TopDayColor;
            float2 _SkyNoiseScale;
            float _SkyBands;
            float _SunStep;
            float4 _SunColor;
            float _SunIntensity;
            float4 _BottomDayColor;
            float4 _BottomSunriseColor;
            float4 _BottomNightColor;
            float2 _SkyPixels;
            float _SunPixels;

            float3 u_xlat0;
            float4 u_xlat1;
            float u_xlat6;
            float4 u_xlat2;
            int2 u_xlati2;
            uint u_xlatu2;
            float4 u_xlat3;
            int4 u_xlati3;
            uint2 u_xlatu3;
            float4 u_xlat4;
            float3 u_xlat5;
            int2 u_xlati6;
            uint u_xlatu6;
            bool u_xlatb6;
            float u_xlat7;
            bool2 u_xlatb7;
            float u_xlat8;
            int3 u_xlati9;
            float u_xlat12;
            int u_xlati12;
            uint u_xlatu12;
            float2 u_xlat13;
            float2 u_xlat14;
            float2 u_xlat15;
            uint2 u_xlatu15;
            float u_xlat18;
            int u_xlati18;
            uint u_xlatu18;
            bool u_xlatb18;
            float u_xlat19;
            bool u_xlatb19;



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
            float4x4 _tunity_MatrixV = transpose(unity_MatrixV);


    u_xlat0 = input.vs_INTERP0.yxyz + (-_WorldSpaceCameraPos.yxyz);
    u_xlat1.x = dot(u_xlat0.yzw, u_xlat0.yzw);
    u_xlat1.x = _g_inversesqrt(u_xlat1.x);
    u_xlat6.xyz = u_xlat0.yzw * u_xlat1.xxx;
    u_xlat0.x = u_xlat0.x * u_xlat1.x + 1.0;
    u_xlat0.x = u_xlat0.x * 0.5;
    u_xlat0.x = max(u_xlat0.x, 0.5);
    u_xlat0.x = min(u_xlat0.x, 1.0);
    u_xlat0.x = u_xlat0.x + -0.5;
    u_xlat0.x = u_xlat0.x + u_xlat0.x;
    u_xlat1.x = max(abs(u_xlat6.z), abs(u_xlat6.x));
    u_xlat1.x = float(1.0) / u_xlat1.x;
    u_xlat7 = min(abs(u_xlat6.z), abs(u_xlat6.x));
    u_xlat1.x = u_xlat1.x * u_xlat7;
    u_xlat7 = u_xlat1.x * u_xlat1.x;
    u_xlat13.x = u_xlat7 * 0.0208350997 + -0.0851330012;
    u_xlat13.x = u_xlat7 * u_xlat13.x + 0.180141002;
    u_xlat13.x = u_xlat7 * u_xlat13.x + -0.330299497;
    u_xlat7 = u_xlat7 * u_xlat13.x + 0.999866009;
    u_xlat13.x = u_xlat7 * u_xlat1.x;
    u_xlat13.x = u_xlat13.x * -2.0 + 1.57079637;
    u_xlatb19 = abs(u_xlat6.z)<abs(u_xlat6.x);
    u_xlat13.x = u_xlatb19 ? u_xlat13.x : float(0.0);
    u_xlat1.x = u_xlat1.x * u_xlat7 + u_xlat13.x;
    u_xlatb7.xy = _g_lessThan(u_xlat6.zyzz, (-u_xlat6.zyzz)).xy;
    u_xlat7 = u_xlatb7.x ? -3.14159274 : float(0.0);
    u_xlat1.x = u_xlat7 + u_xlat1.x;
    u_xlat7 = min(u_xlat6.z, u_xlat6.x);
    u_xlatb7.x = u_xlat7<(-u_xlat7);
    u_xlat6.x = max(u_xlat6.z, u_xlat6.x);
    u_xlatb6 = u_xlat6.x>=(-u_xlat6.x);
    u_xlatb6 = u_xlatb6 && u_xlatb7.x;
    u_xlat6.x = (u_xlatb6) ? (-u_xlat1.x) : u_xlat1.x;
    u_xlat6.x = u_xlat6.x * _SkyNoiseScale.x;
    u_xlat1.x = u_xlat6.x * _SkyPixels.x;
    u_xlat6.x = abs(u_xlat6.y) * -0.0187292993 + 0.0742610022;
    u_xlat6.x = u_xlat6.x * abs(u_xlat6.y) + -0.212114394;
    u_xlat6.x = u_xlat6.x * abs(u_xlat6.y) + 1.57072878;
    u_xlat12 = -abs(u_xlat6.y) + 1.0;
    u_xlat12 = sqrt(u_xlat12);
    u_xlat18 = u_xlat12 * u_xlat6.x;
    u_xlat18 = u_xlat18 * -2.0 + 3.14159274;
    u_xlat18 = u_xlatb7.y ? u_xlat18 : float(0.0);
    u_xlat6.x = u_xlat6.x * u_xlat12 + u_xlat18;
    u_xlat6.x = (-u_xlat6.x) + 1.57079637;
    u_xlat6.x = u_xlat6.x * _SkyNoiseScale.y;
    u_xlat1.y = u_xlat6.x * _SkyPixels.y;
    u_xlat6.xy = u_xlat1.xy * float2(0.159154952, 0.636619687);
    u_xlat6.xy = floor(u_xlat6.xy);
    u_xlat6.xy = u_xlat6.xy / _SkyPixels.xy;
    u_xlat6.xy = u_xlat6.xy * _SkyNoiseScale.xx;
    u_xlat1.xy = _g_fract(u_xlat6.xy);
    u_xlat6.xy = floor(u_xlat6.xy);
    u_xlat13.xy = u_xlat1.xy * u_xlat1.xy;
    u_xlat13.xy = u_xlat1.xy * u_xlat13.xy;
    u_xlat2.xy = u_xlat1.xy * float2(6.0, 6.0) + float2(-15.0, -15.0);
    u_xlat2.xy = u_xlat1.xy * u_xlat2.xy + float2(10.0, 10.0);
    u_xlat13.xy = u_xlat13.xy * u_xlat2.xy;
    u_xlat2.xy = u_xlat6.xy + float2(1.0, 1.0);
    u_xlati2.xy = int2(u_xlat2.xy);
    u_xlati18 = int(uint(uint(u_xlati2.y) ^ 1103515245u));
    u_xlati2.x = u_xlati18 + u_xlati2.x;
    u_xlatu18 = uint(u_xlati18) * uint(u_xlati2.x);
    u_xlatu2 = uint(u_xlatu18 >> 5u);
    u_xlati18 = int(uint(u_xlatu18 ^ u_xlatu2));
    u_xlatu18 = uint(u_xlati18) * 668265261u;
    u_xlatu18 = uint(u_xlatu18 >> 8u);
    u_xlat18 = float(u_xlatu18);
    u_xlat2.yz = float2(u_xlat18) * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat8 = floor(u_xlat2.y);
    u_xlat2.x = u_xlat18 * 5.96046519e-08 + (-u_xlat8);
    u_xlat18 = dot(u_xlat2.xz, u_xlat2.xz);
    u_xlat18 = _g_inversesqrt(u_xlat18);
    u_xlat2.xy = float2(u_xlat18) * u_xlat2.xz;
    u_xlat14.xy = u_xlat1.xy + float2(-1.0, -1.0);
    u_xlat18 = dot(u_xlat2.xy, u_xlat14.xy);
    u_xlat2 = u_xlat1.xyxy + float4(-0.0, -1.0, -1.0, -0.0);
    u_xlat3 = u_xlat6.xyxy + float4(0.0, 1.0, 1.0, 0.0);
    u_xlati6.xy = int2(u_xlat6.xy);
    u_xlati3 = int4(u_xlat3);
    u_xlati9.xz = int2(uint2(uint(u_xlati3.y) ^ uint(1103515245u), uint(u_xlati3.w) ^ uint(1103515245u)));
    u_xlati3.xz = u_xlati9.xz + u_xlati3.xz;
    u_xlatu3.xy = uint2(u_xlati9.xz) * uint2(u_xlati3.xz);
    u_xlatu15.xy = uint2(u_xlatu3.x >> 5u, u_xlatu3.y >> 5u);
    u_xlati3.xy = int2(uint2(u_xlatu15.x ^ u_xlatu3.x, u_xlatu15.y ^ u_xlatu3.y));
    u_xlatu3.xy = uint2(u_xlati3.xy) * uint2(668265261u, 668265261u);
    u_xlatu3.xy = uint2(u_xlatu3.x >> 8u, u_xlatu3.y >> 8u);
    u_xlat3.xy = float2(u_xlatu3.xy);
    u_xlat4 = u_xlat3.xyxy * float4(5.96046519e-08, 5.96046519e-08, 5.96046519e-08, 5.96046519e-08) + float4(0.5, 0.5, -0.5, -0.5);
    u_xlat15.xy = floor(u_xlat4.xy);
    u_xlat4.xy = u_xlat3.xy * float2(5.96046519e-08, 5.96046519e-08) + (-u_xlat15.xy);
    u_xlat3.x = dot(u_xlat4.yw, u_xlat4.yw);
    u_xlat3.x = _g_inversesqrt(u_xlat3.x);
    u_xlat3.xy = u_xlat3.xx * u_xlat4.yw;
    u_xlat14.x = dot(u_xlat3.xy, u_xlat2.zw);
    u_xlat18 = u_xlat18 + (-u_xlat14.x);
    u_xlat18 = u_xlat13.y * u_xlat18 + u_xlat14.x;
    u_xlat14.x = dot(u_xlat4.xz, u_xlat4.xz);
    u_xlat14.x = _g_inversesqrt(u_xlat14.x);
    u_xlat14.xy = u_xlat14.xx * u_xlat4.xz;
    u_xlat2.x = dot(u_xlat14.xy, u_xlat2.xy);
    u_xlati12 = int(uint(uint(u_xlati6.y) ^ 1103515245u));
    u_xlati6.x = u_xlati12 + u_xlati6.x;
    u_xlatu6 = uint(u_xlati12) * uint(u_xlati6.x);
    u_xlatu12 = uint(u_xlatu6 >> 5u);
    u_xlati6.x = int(uint(u_xlatu12 ^ u_xlatu6));
    u_xlatu6 = uint(u_xlati6.x) * 668265261u;
    u_xlatu6 = uint(u_xlatu6 >> 8u);
    u_xlat6.x = float(u_xlatu6);
    u_xlat3.yz = u_xlat6.xx * float2(5.96046519e-08, 5.96046519e-08) + float2(0.5, -0.5);
    u_xlat12 = floor(u_xlat3.y);
    u_xlat3.x = u_xlat6.x * 5.96046519e-08 + (-u_xlat12);
    u_xlat6.x = dot(u_xlat3.xz, u_xlat3.xz);
    u_xlat6.x = _g_inversesqrt(u_xlat6.x);
    u_xlat6.xy = u_xlat6.xx * u_xlat3.xz;
    u_xlat6.x = dot(u_xlat6.xy, u_xlat1.xy);
    u_xlat12 = (-u_xlat6.x) + u_xlat2.x;
    u_xlat6.x = u_xlat13.y * u_xlat12 + u_xlat6.x;
    u_xlat12 = (-u_xlat6.x) + u_xlat18;
    u_xlat6.x = u_xlat13.x * u_xlat12 + u_xlat6.x;
    u_xlat6.x = u_xlat6.x + 0.5;
    u_xlat0.x = _SkyNoiseStrength * u_xlat6.x + u_xlat0.x;
    u_xlat0.x = u_xlat0.x * _SkyBands;
    u_xlat0.x = floor(u_xlat0.x);
    u_xlat0.x = u_xlat0.x / _SkyBands;
    u_xlat6.x = (-_MainLightPosition.y) + 1.0;
    u_xlat6.xyz = u_xlat6.xxx * float3(0.5, 0.5, 0.5) + float3(-0.300000012, -0.5, -0.400000006);
    u_xlat6.xyz = u_xlat6.xyz * float3(5.00000048, 9.99999809, 4.99999952);
    u_xlat6.xyz = clamp(u_xlat6.xyz, 0.0, 1.0);
    u_xlat1.xyz = u_xlat6.xyz * float3(-2.0, -2.0, -2.0) + float3(3.0, 3.0, 3.0);
    u_xlat6.xyz = u_xlat6.xyz * u_xlat6.xyz;
    u_xlat6.xyz = u_xlat6.xyz * u_xlat1.xyz;
    u_xlat1.xyz = (-_BottomDayColor.xyz) + _BottomSunriseColor.xyz;
    u_xlat1.xyz = u_xlat6.xxx * u_xlat1.xyz + _BottomDayColor.xyz;
    u_xlat2.xyz = (-u_xlat1.xyz) + _BottomNightColor.xyz;
    u_xlat1.xyz = u_xlat6.yyy * u_xlat2.xyz + u_xlat1.xyz;
    u_xlat2.xyz = _TopNightColor.xyz + (-_TopDayColor.xyz);
    u_xlat6.xyz = u_xlat6.zzz * u_xlat2.xyz + _TopDayColor.xyz;
    u_xlat6.xyz = (-u_xlat1.xyz) + u_xlat6.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz + u_xlat1.xyz;
    u_xlat1.xyz = _SunColor.xyz * float3(float3(_SunIntensity, _SunIntensity, _SunIntensity)) + (-u_xlat0.xyz);
    u_xlat2.xyz = _MainLightPosition.zxy * float3(-0.0, -1.0, -0.0);
    u_xlat2.xyz = _MainLightPosition.xyz * float3(-0.0, -0.0, -1.0) + (-u_xlat2.xyz);
    u_xlat18 = dot(u_xlat2.yz, u_xlat2.yz);
    u_xlat18 = _g_inversesqrt(u_xlat18);
    u_xlat2.xyz = float3(u_xlat18) * u_xlat2.xyz;
    u_xlat3.xyz = u_xlat2.xyz * (-_MainLightPosition.zxy);
    u_xlat3.xyz = (-_MainLightPosition.yzx) * u_xlat2.yzx + (-u_xlat3.xyz);
    u_xlat4.xyz = (-input.vs_INTERP0.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat18 = dot(u_xlat4.xyz, u_xlat4.xyz);
    u_xlat18 = _g_inversesqrt(u_xlat18);
    u_xlat4.xyz = float3(u_xlat18) * u_xlat4.xyz;
    u_xlatb18 = unity_OrthoParams.w==0.0;
    u_xlat5.y = (u_xlatb18) ? u_xlat4.y : _tunity_MatrixV[1].z;
    u_xlat5.x = (u_xlatb18) ? u_xlat4.x : _tunity_MatrixV[0].z;
    u_xlat5.z = (u_xlatb18) ? u_xlat4.z : _tunity_MatrixV[2].z;
    u_xlat3.y = dot(u_xlat5.xyz, u_xlat3.xyz);
    u_xlat18 = dot(u_xlat5.xyz, (-_MainLightPosition.xyz));
    u_xlat18 = clamp(u_xlat18, 0.0, 1.0);
    u_xlat3.x = dot(u_xlat5.zx, u_xlat2.yz);
    u_xlat2.xy = u_xlat3.xy * float2(float2(_SunPixels, _SunPixels));
    u_xlat2.xy = floor(u_xlat2.xy);
    u_xlat2.xy = u_xlat2.xy / float2(float2(_SunPixels, _SunPixels));
    u_xlat19 = dot(u_xlat2.xy, u_xlat2.xy);
    u_xlat19 = sqrt(u_xlat19);
    u_xlatb19 = _SunStep>=u_xlat19;
    u_xlat19 = u_xlatb19 ? 1.0 : float(0.0);
    u_xlat18 = u_xlat18 * u_xlat19;
    __SV_Target0.xyz = float3(u_xlat18) * u_xlat1.xyz + u_xlat0.xyz;
    __SV_Target0.w = 1.0;
    return __SV_Target0;

            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Shader Graph/FallbackError"
}
