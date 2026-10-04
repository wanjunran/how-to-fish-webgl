Shader "Shader Graphs/SkyboxShader" {
	Properties {
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