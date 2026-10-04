Shader "Shader Graphs/LavaShader" {
	Properties {
		_Color_Speed ("Color Speed", Vector) = (0,0,0,0)
		_Color_Speed_2 ("Color Speed 2", Vector) = (0,0,0,0)
		_Noise_Scale_2 ("Noise Scale 2", Float) = 0
		[HDR] _DeepColor ("Color 1", Vector) = (1,0.3686275,0,1)
		[HDR] Color_93e06cd551a5449091bcde90b46765a0 ("Color 2", Vector) = (1,0.3686275,0,1)
		_Pixelate ("Pixelate", Float) = 5
		_Noise_Scale ("Noise Scale", Float) = 1
		_Color_Smoothstep ("Color Smoothstep", Vector) = (0,1,0,0)
		Vector1_6269b1025b26473ca8bc61634f34b537 ("Smoothness", Range(0, 1)) = 0.95
		Vector1_1 ("Metallic", Range(0, 1)) = 0.95
		_Vertex_Speed ("Vertex Speed", Vector) = (0,0,0,0)
		_Vertex_Height ("Vertex Height", Float) = 1
		_Vertex_Noise_Scale ("Vertex Noise Scale", Float) = 0
		[NoScaleOffset] _Normal_Bumps_Map ("Normal Bumps Map", 2D) = "white" {}
		_Normal_Bumps_Strength ("Normal Bumps Strength", Range(0, 1)) = 0.2
		_Normal_Bumps_Scale ("Normal Bumps Scale", Vector) = (1,2,0,0)
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