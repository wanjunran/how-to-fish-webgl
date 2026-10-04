Shader "Shader Graphs/BoatShader" {
	Properties {
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
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
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

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}