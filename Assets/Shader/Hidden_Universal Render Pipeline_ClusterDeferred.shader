Shader "Hidden/Universal Render Pipeline/ClusterDeferred" {
	Properties {
		_LitStencilRef ("LitStencilRef", Float) = 0
		_LitStencilReadMask ("LitStencilReadMask", Float) = 0
		_LitStencilWriteMask ("LitStencilWriteMask", Float) = 0
		_SimpleLitStencilRef ("SimpleLitStencilRef", Float) = 0
		_SimpleLitStencilReadMask ("SimpleLitStencilReadMask", Float) = 0
		_SimpleLitStencilWriteMask ("SimpleLitStencilWriteMask", Float) = 0
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
	Fallback "Hidden/Universal Render Pipeline/FallbackError"
}