Shader "Shader Graphs/CloudSphereShader" {
	Properties {
		_CloudHeigth ("CloudHeigth", Float) = 0
		_Pixels ("Pixels", Float) = 1
		[HDR] _Color ("Color", Vector) = (0.7490196,0.7490196,0.7490196,1)
		[HDR] _SSSColor ("SSSColor", Vector) = (1,0.5607843,0,1)
		_NormalInfluence ("NormalInfluence", Float) = 0
		_SSSPower ("SSSPower", Float) = 0
		_SSSIntensity ("SSSIntensity", Float) = 0
		_Thickness ("Thickness", Float) = 0
		_CloudScale ("CloudScale", Float) = 0
		_DetailScale ("DetailScale", Float) = 10
		_CloudSmoothStep ("CloudSmoothStep", Vector) = (0,1,0,0)
		_DetailSmoothStep ("DetailSmoothStep", Vector) = (0,0,0,0)
		_CombinedSmoothStep ("CombinedSmoothStep", Vector) = (0,0,0,0)
		_HeightSmoothStep ("HeightSmoothStep", Vector) = (0,0,0,0)
		_HeigthSmoothStep2 ("HeigthSmoothStep2", Vector) = (0,0,0,0)
		_CloudSpeed ("CloudSpeed", Vector) = (-0.0005,-0.0025,0,0)
		_DetailSpeed ("DetailSpeed", Vector) = (0.015,0.01,0,0)
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