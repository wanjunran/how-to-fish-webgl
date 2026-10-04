Shader "Andicraft/Retro Master (NOT Transparent)" {
	Properties {
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
		TEXTURES ("# Textures", Float) = 0
		[KeywordEnum(Nearest, Linear, N64)] _TextureFiltering ("Texture Filtering Mode", Float) = 0
		_BaseMap ("Color &&", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[Toggle] _EnableMaskMap ("Mask Map", Float) = 1
		_MaskNote1 ("!NOTE Using Color Map Alpha as Smoothness [_EnableMaskMap == 0]", Float) = 0
		_MaskMap ("Mask Map & [_EnableMaskMap]", 2D) = "" {}
		_MaskNote2 ("!NOTE R: Metal G: Smoothness B: Occlusion", Float) = 0
		SMOOTHSLIDER ("-!DRAWER MinMax _SmoothRemap.x _SmoothRemap.y", Float) = 1
		_SmoothRemap ("Remap Smoothness", Vector) = (0,1,0,0)
		[Normal] _BumpMap ("Normal Map &&", 2D) = "bump" {}
		_NormalStrength ("Normal Map Strength", Range(0, 2)) = 1
		[Toggle(_EMISSION)] _Emission ("Emission", Float) = 0
		_EmissionMap ("-Emission Map && [_EMISSION]", 2D) = "white" {}
		[HDR] _EmissionColor ("-Emission Color [_EMISSION]", Vector) = (0,0,0,1)
		_LightmapEmissionStr ("-Lightmap Emission Strength", Float) = 1
		_TilingOffset ("Tiling/Offset &", Vector) = (1,1,0,0)
		_Lighting ("# Lighting", Float) = 0
		[KeywordEnum(Pixel, Vertex, Texel)] _LightingType ("Lighting Type", Float) = 2
		[KeywordEnum(CookTorrance, Phong, Blinn Phong, Disabled)] _SpecularType ("Specular Type", Float) = 0
		[Toggle] _HalfLambert ("Half Lambert", Float) = 0
		[Toggle] _Reflections ("Reflections", Float) = 1
		[Toggle(_TOONSHADING)] _ToonShading ("Toon Shading", Float) = 0
		_DITHERHEADER ("## Dithering", Float) = 0
		[Toggle] _DitherDiffuse ("-Dither Diffuse Light", Float) = 0
		[Toggle] _DitherSpec ("-Dither Specular Light", Float) = 0
		[Toggle] _DitherAmbient ("-Dither Ambient & Lightmaps", Float) = 0
		[Toggle] _DoubleSizeDither ("-Double Size Dither", Float) = 0
		_Extras ("# Extras", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0
		[Toggle()] _VColEmissive ("Vertex Color as Emissive", Float) = 0
		_VertexEmissiveStr ("- Emissive Strength [_VColEmissive]", Float) = 1
		[Toggle(_ALPHATEST_ON)] _AlphaTest ("Alpha Cutoff", Float) = 0
		_Cutoff ("-Alpha Cutoff Threshold [_ALPHATEST_ON]", Range(0, 1)) = 0.5
		[Toggle(_VERTEXJITTER)] _VertexJitter ("Vertex Snap", Float) = 0
		_VertexJitterResX ("- Virtual Screen X Resolution [_VERTEXJITTER]", Float) = 320
		_VertexJitterResY ("- Virtual Screen Y Resolution [_VERTEXJITTER]", Float) = 240
		[Toggle(_AFFINE_MAPPING)] _AffineMapping ("Affine UV Mapping", Float) = 0
		_AffineMapFactor ("-Warp Factor [_AFFINE_MAPPING]", Range(0, 1)) = 1
		[Toggle] _MaskTint ("Mask Color Tint", Float) = 0
		_TintNote ("!NOTE Using Mask Map A channel to mask color tint value [_MaskTint]", Float) = 0
		_ToonShading ("# Toon Shading [_TOONSHADING]", Float) = 0
		_ToonRamp ("Diffuse Ramp&", 2D) = "white" {}
		_SpecularRamp ("Specular Ramp&", 2D) = "white" {}
		RAMPGENERATOR ("!DRAWER Gradient _RampGen", Float) = 0
		_RampGen ("Ramp Generator", 2D) = "white" {}
		_VertexLighting ("# Vertex Light Settings [_LIGHTINGTYPE_VERTEX]", Float) = 0
		_VertexSpecColor ("Specular Color", Vector) = (1,1,1,1)
		[Toggle(_VCOLSMOOTHNESS)] _VColSmoothness ("Use Vertex Color A as Smoothness", Float) = 0
		SMOOTHSLIDER2 ("-!DRAWER MinMax _SmoothRemapVCol.x _SmoothRemapVCol.y [_VCOLSMOOTHNESS]", Float) = 1
		_SmoothRemapVCol ("Remap Smoothness", Vector) = (0,1,0,0)
		_VertexLightSmoothness ("Smoothness [!_VCOLSMOOTHNESS]", Range(0, 1)) = 0.5
		[HideInInspector] _AlphaClip ("__clip", Float) = 0
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
	//CustomEditor "Needle.MarkdownShaderGUI"
}