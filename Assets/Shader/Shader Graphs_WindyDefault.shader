Shader "Shader Graphs/WindyDefault" {
	Properties {
		[NoScaleOffset] _Texture ("Texture", 2D) = "white" {}
		_Smoothness ("Smoothness", Float) = 0
		_Metallic ("Metallic", Float) = 0
		_WindFromOrigoSmoothstep ("WindFromOrigoSmoothstep", Vector) = (0,1,0,0)
		_WindSpeed ("WindSpeed", Float) = 2.8
		_WindSpeed2 ("WindSpeed2", Float) = 0.5
		_WindScale ("WindScale", Float) = 0.15
		_WindDensity ("WindDensity", Float) = 0.2
		_NormalInfluence ("NormalInfluence", Float) = 2
		_SSSPower ("SSSPower", Float) = 1.5
		_SSSIntensity ("SSSIntensity", Float) = 2
		_Thickness ("Thickness", Float) = 1
		[NoScaleOffset] _Normal_Map ("Normal Map", 2D) = "white" {}
		_Normal_Strength ("Normal Strength", Range(0, 1)) = 0.146
		_Normal_Scale ("Normal Scale", Float) = 1
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