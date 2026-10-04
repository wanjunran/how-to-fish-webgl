Shader "Shader Graphs/SlotMachineBackground" {
	Properties {
		[HideInInspector] [NoScaleOffset] _MainTex ("_MainTex", 2D) = "white" {}
		_Emission ("Emission", Float) = 0
		_MainColor ("MainColor", Vector) = (1,1,1,1)
		_SecondColor ("SecondColor", Vector) = (1,1,1,1)
		_Tightness ("Tightness", Float) = 0.75
		_Tiling ("Tiling", Float) = 1
		_TilingSpeed ("TilingSpeed", Vector) = (0.1,0,0,0)
		_WaveSpeed ("WaveSpeed", Vector) = (3,0.1,0,0)
		_WaveAmount ("WaveAmount", Float) = 0
		_WaveOffset ("WaveOffset", Float) = 0
		_NoiseScale ("NoiseScale", Float) = 1
		_NoiseAmount ("NoiseAmount", Float) = 0.25
		_Tightness2 ("Tightness2", Float) = 0.75
		_Tiling2 ("Tiling2", Float) = 1
		_TilingSpeed2 ("TilingSpeed2", Vector) = (0.1,0,0,0)
		_WaveSpeed2 ("WaveSpeed2", Vector) = (2,0.1,0,0)
		_WaveAmount2 ("WaveAmount2", Float) = 0
		_WaveOffset2 ("WaveOffset2", Float) = 0
		_NoiseScale2 ("NoiseScale2", Float) = 1
		_NoiseAmount2 ("NoiseAmount2", Float) = 0.25
		_LineColor1 ("LineColor1", Vector) = (0,0,0,1)
		_LineColor2 ("LineColor2", Vector) = (0,0,0,1)
		_LineColor3 ("LineColor3", Vector) = (0,0,0,1)
		_LineColor4 ("LineColor4", Vector) = (0,0,0,1)
		_Pixels ("Pixels", Float) = 1
		_BackgroundSpeed ("BackgroundSpeed", Float) = 0
		_LineThinness ("LineThinness", Float) = 1
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