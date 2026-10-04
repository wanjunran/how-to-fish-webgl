Shader "Shader Graphs/Grass" {
	Properties {
		_Color ("Color", Vector) = (0.3803922,0.454902,0.282353,1)
		_TopColor ("TopColor", Vector) = (0.5882353,0.6235294,0.2980392,1)
		_Smoothness ("Smoothness", Float) = 0
		[NoScaleOffset] _Texture ("Texture", 2D) = "white" {}
		_WindColorMulti ("WindColorMulti", Float) = 0.5
		_WindSpeed ("WindSpeed", Float) = 3.2
		_WindSpeed2 ("WindSpeed2", Float) = 1
		_WindStrength ("WindStrength", Float) = 0.15
		_WindDensity ("WindDensity", Float) = 0.2
		[NoScaleOffset] _Noise_Map ("Noise Map", 2D) = "white" {}
		_Noise_Map_Scale ("Noise Map Scale", Vector) = (0.04,0.02,0,0)
		_Noise_Remap ("Noise Remap", Vector) = (0.9,1,0,0)
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
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

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}