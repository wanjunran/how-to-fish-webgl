Shader "Shader Graphs/MapBackground" {
	Properties {
		[HideInInspector] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
		[HideInInspector] _Stencil ("_Stencil", Float) = 0
		[HideInInspector] _StencilComp ("_StencilComp", Float) = 8
		[HideInInspector] _StencilOp ("_StencilOp", Float) = 0
		[HideInInspector] _StencilWriteMask ("_StencilWriteMask", Float) = 255
		[HideInInspector] _StencilReadMask ("_StencilReadMask", Float) = 255
		[HideInInspector] _ColorMask ("_ColorMask", Float) = 15
		_GridColor ("GridColor", Vector) = (0.01013041,1,0,1)
		_Pixels ("Pixels", Float) = 1
		_BackgroundColor ("BackgroundColor", Vector) = (0,0,0,1)
		_GridThickness ("GridThickness", Float) = 0.12
		_PlayerOffsetMultiplier ("PlayerOffsetMultiplier", Float) = 4
		_CircleDistanceCheck ("CircleDistanceCheck", Float) = 1
		_CircleSmoothstep ("CircleSmoothstep", Vector) = (0.5,1,0,0)
		[HideInInspector] White ("Color", Vector) = (1,1,1,1)
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}