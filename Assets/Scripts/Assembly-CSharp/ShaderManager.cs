using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderManager : MonoBehaviour
{
	private static ShaderManager _instance;

	[SerializeField]
	private Transform _mainLight;

	[SerializeField]
	private Shader _defaultShader;

	[SerializeField]
	private Shader _boatShader;

	[SerializeField]
	private Material _vignetteMaterial;

	[SerializeField]
	private Material _sniperMaterial;

	[SerializeField]
	private Material _thinkingMaterial;

	[SerializeField]
	private Transform _cloudHolder;

	[SerializeField]
	private Vector3 _sunsetRot;

	[Header("Fog Settings")]
	[SerializeField]
	private float _underwaterFog = 0.3f;

	[SerializeField]
	private float _sunsetFog = 0.5f;

	[SerializeField]
	private Color _sunsetColor = Color.yellow;

	[SerializeField]
	private Texture _sunsetReflectionProbe;

	public static readonly int PlayerSkinID = Shader.PropertyToID("_SkinColor");

	public static readonly int PlayerPrimaryColorID = Shader.PropertyToID("_PrimaryColor");

	public static readonly int PlayerSecondColorID = Shader.PropertyToID("_SecondColor");

	public static readonly int PlayerThirdColorID = Shader.PropertyToID("_ThirdColor");

	public static readonly int CooknessID = Shader.PropertyToID("_Cookness");

	public static readonly int UseSkinID = Shader.PropertyToID("_Use_Skin");

	public static readonly int RainbowSkinID = Shader.PropertyToID("_Rainbow_Skin");

	public static LocalKeyword GradientNoiseKW;

	public static LocalKeyword CheckerboardKW;

	public static LocalKeyword VoronoiKW;

	public static LocalKeyword OnlyMetallicKW;

	public static LocalKeyword OnlyMetallicNoiseKW;

	public static LocalKeyword EverythingKW;

	public static LocalKeyword BoatGradientNoiseKW;

	public static LocalKeyword BoatCheckerboardKW;

	public static LocalKeyword BoatVoronoiKW;

	public static LocalKeyword BoatOnlyMetallicKW;

	public static LocalKeyword BoatOnlyMetallicNoiseKW;

	public static LocalKeyword BoatEverythingKW;

	public static readonly int ColorOffsetID = Shader.PropertyToID("_Color_Offset");

	public static readonly int ColorOffset2ID = Shader.PropertyToID("_Color_Offset_2");

	public static readonly int SkinSmoothStepID = Shader.PropertyToID("_Skin_Smooth_Step");

	public static readonly int SkinNoiseScaleID = Shader.PropertyToID("_Skin_Noise_Scale");

	public static readonly int NoiseRotationID = Shader.PropertyToID("_Noise_Rotation");

	public static readonly int UVRotationID = Shader.PropertyToID("_UV_Rotation");

	public static readonly int SkinUVScaleID = Shader.PropertyToID("_Skin_UV_Scale");

	public static readonly int MetallicMetallicnessID = Shader.PropertyToID("_MetallicMetallicness");

	public static readonly int MetallicSmoothness = Shader.PropertyToID("_MetallicSmoothness");

	public static readonly int PlasticMetallicnessID = Shader.PropertyToID("_PlasticMetallicness");

	public static readonly int PlasticSmoothnessID = Shader.PropertyToID("_PlasticSmoothness");

	public static string GradientNoiseName = "_SKIN_TYPE_GRADIENT_NOISE";

	public static string CheckerboardName = "_SKIN_TYPE_CHECKERBOARD";

	public static string VoronoiName = "_SKIN_TYPE_VORONOI";

	public static string OnlyMetallicName = "_SKIN_AFFECTS_ONLY_METALLIC";

	public static string OnlyMetallicNoiseName = "_SKIN_AFFECTS_ONLY_METALLIC_NOISE";

	public static string EverythingName = "_SKIN_AFFECTS_EVERYTHING";

	private static readonly int MousePositionID = Shader.PropertyToID("_MousePosition");

	private static readonly int PausedID = Shader.PropertyToID("_Paused");

	private static readonly int VignetteIntensityID = Shader.PropertyToID("_Intensity");

	private static readonly int VignetteColorID = Shader.PropertyToID("_VignetteColor");

	private static readonly int MapZoomID = Shader.PropertyToID("_MapZoom");

	private static readonly int PlayerOffsetID = Shader.PropertyToID("_PlayerOffset");

	private static readonly int PlayerRotationID = Shader.PropertyToID("_PlayerRotation");

	private static readonly int RadarRotationID = Shader.PropertyToID("_RadarRotation");

	private static readonly int SniperAimPos = Shader.PropertyToID("_SniperAimPos");

	private static readonly int SniperScale = Shader.PropertyToID("_SniperScale");

	private static readonly int ThinkingID = Shader.PropertyToID("_ThinkingAlpha");

	private Transform _customCloudTarget;

	private Vector3 _mainLightOrgRot;

	private static float _defaultFogDensity;

	private static Color _defaultFogColor;

	private static Texture _defaultRefProbe;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		GradientNoiseKW = new LocalKeyword(_defaultShader, "_SKIN_TYPE_GRADIENT_NOISE");
		CheckerboardKW = new LocalKeyword(_defaultShader, "_SKIN_TYPE_CHECKERBOARD");
		VoronoiKW = new LocalKeyword(_defaultShader, "_SKIN_TYPE_VORONOI");
		OnlyMetallicKW = new LocalKeyword(_defaultShader, "_SKIN_AFFECTS_ONLY_METALLIC");
		OnlyMetallicNoiseKW = new LocalKeyword(_defaultShader, "_SKIN_AFFECTS_ONLY_METALLIC_NOISE");
		EverythingKW = new LocalKeyword(_defaultShader, "_SKIN_AFFECTS_EVERYTHING");
		BoatGradientNoiseKW = new LocalKeyword(_boatShader, "_SKIN_TYPE_GRADIENT_NOISE");
		BoatCheckerboardKW = new LocalKeyword(_boatShader, "_SKIN_TYPE_CHECKERBOARD");
		BoatVoronoiKW = new LocalKeyword(_boatShader, "_SKIN_TYPE_VORONOI");
		BoatOnlyMetallicKW = new LocalKeyword(_boatShader, "_SKIN_AFFECTS_ONLY_METALLIC");
		BoatOnlyMetallicNoiseKW = new LocalKeyword(_boatShader, "_SKIN_AFFECTS_ONLY_METALLIC_NOISE");
		BoatEverythingKW = new LocalKeyword(_boatShader, "_SKIN_AFFECTS_EVERYTHING");
		_mainLightOrgRot = _mainLight.eulerAngles;
		_defaultFogDensity = RenderSettings.fogDensity;
		_defaultFogColor = RenderSettings.fogColor;
		_defaultRefProbe = RenderSettings.customReflectionTexture;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Apply()
	{
		if (GraphicsSettings.TryGetRenderPipelineSettings<URPReflectionProbeSettings>(out var settings) && settings.UseReflectionProbeRotation)
		{
			SupportedRenderingFeatures.active.reflectionProbeModes = SupportedRenderingFeatures.ReflectionProbeModes.Rotation;
		}
	}

	private void OnEnable()
	{
		SetPauseFloat(to: false);
	}

	private void OnDestroy()
	{
		SetPauseFloat(to: false);
	}

	private void Update()
	{
		Vector2 vector = Input.mousePosition;
		Shader.SetGlobalVector(value: new Vector2(vector.x / (float)Screen.width, vector.y / (float)Screen.height), nameID: MousePositionID);
		Transform transform = (_customCloudTarget ? _customCloudTarget : (GameInfo.CurCamera ? GameInfo.CurCamera.transform : null));
		if ((bool)transform)
		{
			_cloudHolder.position = transform.position;
		}
		else
		{
			_cloudHolder.position = Vector3.zero;
		}
	}

	public static void SetFog(FogState state)
	{
		float fogDensity = _defaultFogDensity;
		Color fogColor = _defaultFogColor;
		Texture customReflectionTexture = _defaultRefProbe;
		switch (state)
		{
		case FogState.Underwater:
			fogDensity = _instance._underwaterFog;
			break;
		case FogState.Sunset:
			fogDensity = _instance._sunsetFog;
			fogColor = _instance._sunsetColor;
			customReflectionTexture = _instance._sunsetReflectionProbe;
			break;
		}
		RenderSettings.fogDensity = fogDensity;
		RenderSettings.fogColor = fogColor;
		RenderSettings.customReflectionTexture = customReflectionTexture;
	}

	public static void ToggleSunset(bool sunset)
	{
		if ((bool)_instance)
		{
			_instance._mainLight.eulerAngles = (sunset ? _instance._sunsetRot : _instance._mainLightOrgRot);
			SetFog(sunset ? FogState.Sunset : FogState.Default);
		}
	}

	public static void SetCustomTarget(Transform target)
	{
		if ((bool)_instance)
		{
			_instance._customCloudTarget = target;
		}
	}

	public static void OnPause()
	{
		if ((bool)_instance)
		{
			_instance.SetPauseFloat(PauseManager.IsPaused);
		}
	}

	private void SetPauseFloat(bool to)
	{
		Shader.SetGlobalFloat(PausedID, to ? 1 : 0);
	}

	public static void SetPlayerColors(Renderer renderer, Vector3 skinColor, Vector3 primaryColor = default(Vector3), Vector3 secondColor = default(Vector3), Vector3 thirdColor = default(Vector3))
	{
		renderer.material.SetVector(PlayerSkinID, skinColor);
		renderer.material.SetVector(PlayerPrimaryColorID, primaryColor);
		renderer.material.SetVector(PlayerSecondColorID, secondColor);
		renderer.material.SetVector(PlayerThirdColorID, thirdColor);
	}

	public static void SetPlayerColorsEditMode(Renderer renderer, Vector3 skinColor, Vector3 primaryColor = default(Vector3), Vector3 secondColor = default(Vector3), Vector3 thirdColor = default(Vector3))
	{
		renderer.sharedMaterial.SetVector(PlayerSkinID, skinColor);
		renderer.sharedMaterial.SetVector(PlayerPrimaryColorID, primaryColor);
		renderer.sharedMaterial.SetVector(PlayerSecondColorID, secondColor);
		renderer.sharedMaterial.SetVector(PlayerThirdColorID, thirdColor);
	}

	public static void SetItemCookness(List<Renderer> _renderers, float to)
	{
		foreach (Renderer _renderer in _renderers)
		{
			SetItemCookness(_renderer, to);
		}
	}

	public static void SetItemCookness(Renderer rend, float to)
	{
		rend.material.SetFloat(CooknessID, to);
	}

	public static void ApplyItemSkin(ItemSkin itemSkin, List<Renderer> applyTo)
	{
		foreach (Renderer item in applyTo)
		{
			ApplyItemSkin(itemSkin, item);
		}
	}

	public static void ResetItemSkin(Renderer rend)
	{
		ApplyItemSkin(default, rend);
	}

	public static void ApplyItemSkin(ItemSkin itemSkin, Renderer rend, bool forBoat = false)
	{
		rend.material.SetInt(UseSkinID, itemSkin.UseSkin ? 1 : 0);
		rend.material.SetInt(RainbowSkinID, itemSkin.IsRainbowSkin ? 1 : 0);
		LocalKeyword keyword = ((!forBoat) ? GradientNoiseKW : BoatGradientNoiseKW);
		LocalKeyword keyword2 = ((!forBoat) ? CheckerboardKW : BoatCheckerboardKW);
		LocalKeyword keyword3 = ((!forBoat) ? VoronoiKW : BoatVoronoiKW);
		rend.material.SetKeyword(in keyword, value: false);
		rend.material.SetKeyword(in keyword2, value: false);
		rend.material.SetKeyword(in keyword3, value: false);
		rend.material.SetFloat(GradientNoiseName, 0f);
		rend.material.SetFloat(CheckerboardName, 0f);
		rend.material.SetFloat(VoronoiName, 0f);
		switch (itemSkin.NoiseType)
		{
		case SkinNoise.GradientNoise:
			rend.material.SetKeyword(in keyword, value: true);
			rend.material.SetFloat(GradientNoiseName, 1f);
			break;
		case SkinNoise.Checkerboard:
			rend.material.SetKeyword(in keyword2, value: true);
			rend.material.SetFloat(CheckerboardName, 1f);
			break;
		case SkinNoise.Voronoi:
			rend.material.SetKeyword(in keyword3, value: true);
			rend.material.SetFloat(VoronoiName, 1f);
			break;
		}
		LocalKeyword keyword4 = ((!forBoat) ? OnlyMetallicKW : BoatOnlyMetallicKW);
		LocalKeyword keyword5 = ((!forBoat) ? OnlyMetallicNoiseKW : BoatOnlyMetallicNoiseKW);
		LocalKeyword keyword6 = ((!forBoat) ? EverythingKW : BoatEverythingKW);
		rend.material.SetKeyword(in keyword4, value: false);
		rend.material.SetKeyword(in keyword5, value: false);
		rend.material.SetKeyword(in keyword6, value: false);
		rend.material.SetFloat(OnlyMetallicName, 0f);
		rend.material.SetFloat(OnlyMetallicNoiseName, 0f);
		rend.material.SetFloat(EverythingName, 0f);
		switch (itemSkin.SkinAffects)
		{
		case SkinAffects.OnlyMetallic:
			rend.material.SetKeyword(in keyword4, value: true);
			rend.material.SetFloat(OnlyMetallicName, 1f);
			break;
		case SkinAffects.OnlyMetallicNoise:
			rend.material.SetKeyword(in keyword5, value: true);
			rend.material.SetFloat(OnlyMetallicNoiseName, 1f);
			break;
		case SkinAffects.Everything:
			rend.material.SetKeyword(in keyword6, value: true);
			rend.material.SetFloat(EverythingName, 1f);
			break;
		}
		rend.material.SetVector(ColorOffsetID, itemSkin.ColorOffset);
		rend.material.SetVector(ColorOffset2ID, itemSkin.ColorOffset2);
		rend.material.SetVector(SkinSmoothStepID, itemSkin.SkinSmoothStep);
		rend.material.SetFloat(SkinNoiseScaleID, itemSkin.SkinNoiseScale);
		rend.material.SetFloat(NoiseRotationID, itemSkin.NoiseRotation);
		rend.material.SetFloat(UVRotationID, itemSkin.UVRotation);
		rend.material.SetVector(SkinUVScaleID, itemSkin.SkinUVScale);
		rend.material.SetFloat(MetallicMetallicnessID, itemSkin.MetallicMetallicness);
		rend.material.SetFloat(MetallicSmoothness, itemSkin.MetallicSmoothness);
		rend.material.SetFloat(PlasticMetallicnessID, itemSkin.PlasticMetallicness);
		rend.material.SetFloat(PlasticSmoothnessID, itemSkin.PlasticSmoothness);
	}

	public static void UpdateVignette(float intensity, Color color)
	{
		if ((bool)_instance)
		{
			_instance._vignetteMaterial.SetFloat(VignetteIntensityID, intensity);
			_instance._vignetteMaterial.SetColor(VignetteColorID, color);
		}
	}

	public static void UpdateMapZoom(float zoom)
	{
		if ((bool)_instance)
		{
			Shader.SetGlobalFloat(MapZoomID, zoom);
		}
	}

	public static void UpdatePlayerOffset(Vector2 offset)
	{
		if ((bool)_instance)
		{
			Shader.SetGlobalVector(PlayerOffsetID, offset);
		}
	}

	public static void UpdatePlayerRotation(float rot)
	{
		if ((bool)_instance)
		{
			Shader.SetGlobalFloat(PlayerRotationID, rot);
		}
	}

	public static void UpdateRadarRotation(float rot)
	{
		if ((bool)_instance)
		{
			Shader.SetGlobalFloat(RadarRotationID, rot);
		}
	}

	public static void UpdateSniperUI(Vector2 aimPos, float scale)
	{
		if ((bool)_instance)
		{
			_instance._sniperMaterial.SetVector(SniperAimPos, aimPos);
			_instance._sniperMaterial.SetFloat(SniperScale, scale);
		}
	}

	public static void SetThinkingAlpha(float to)
	{
		if ((bool)_instance)
		{
			_instance._thinkingMaterial.SetFloat(ThinkingID, to);
		}
	}
}
