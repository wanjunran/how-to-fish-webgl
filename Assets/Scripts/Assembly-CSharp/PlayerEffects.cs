using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerEffects : MonoBehaviour
{
	[Header("General")]
	[SerializeField]
	private AnimationCurve _effectsCurve;

	[Header("SATURATION")]
	[SerializeField]
	private float _saturationLerpSpeed = 10f;

	[SerializeField]
	private float _deathSaturation = -60f;

	[Header("VIGNETTE")]
	[SerializeField]
	private Color _vignetteDamageColor = Color.red;

	[SerializeField]
	private Color _vignettePoisonColor = Color.magenta;

	[SerializeField]
	private Color _vignetteFireColor = Color.darkRed;

	[SerializeField]
	private Color _vignetteRestoreHungerColor = Color.orange;

	[Space]
	[SerializeField]
	[Range(0f, 3f)]
	private float _vignetteOnDamage = 1f;

	[SerializeField]
	[Range(0f, 3f)]
	private float _vignetteOnFullness = 1f;

	[SerializeField]
	[Range(0f, 3f)]
	private float _vignetteOnPoison = 1f;

	[SerializeField]
	private Vector2 _vignettePoisonMinMax = new Vector2(0.5f, 1f);

	[SerializeField]
	[Range(0f, 3f)]
	private float _deathVignette = 0.4f;

	[Header("Speeds")]
	[SerializeField]
	private float _vignetteLerpSpeed = 2.5f;

	[SerializeField]
	private float _vignetteEatEffectLerpSpeed = 1.5f;

	private ColorAdjustments _colorAdjustments;

	private float _lastTimeEaten;

	private float _ticksForEatEffect = 0.5f;

	private float _curVignetteIntensity;

	private float _startSaturation;

	private Color _curVignetteColor;

	private bool _skipRestoreFullnessVignette = true;

	private void Awake()
	{
		GameInfo.GlobalVolume.profile.TryGet<ColorAdjustments>(out _colorAdjustments);
		_startSaturation = _colorAdjustments.saturation.value;
		_curVignetteIntensity = 0f;
		_curVignetteColor = Color.black;
		ShaderManager.UpdateVignette(_curVignetteIntensity, _curVignetteColor);
	}

	private void Update()
	{
		if ((bool)Player.LocalPlayer)
		{
			UpdateEffects();
		}
	}

	private void OnDestroy()
	{
		_colorAdjustments.saturation.value = _startSaturation;
	}

	private void UpdateEffects()
	{
		float b = Mathf.Lerp(_deathSaturation, 0f, _effectsCurve.Evaluate(Player.LocalPlayer.Vitals.HealthPercent));
		if (!Mathf.Approximately(_colorAdjustments.saturation.value, b))
		{
			_colorAdjustments.saturation.value = Mathf.Lerp(_colorAdjustments.saturation.value, b, Time.deltaTime * _saturationLerpSpeed);
		}
		bool flag = Time.time - _lastTimeEaten <= _ticksForEatEffect;
		bool flag2 = Player.LocalPlayer.Vitals.FirePercent > 0.1f;
		bool flag3 = Player.LocalPlayer.Vitals.PoisonPercent > 0.1f;
		float b2 = Mathf.Lerp(_deathVignette, 0f, _effectsCurve.Evaluate(Player.LocalPlayer.Vitals.HealthPercent));
		if (flag3 | flag2)
		{
			b2 = ((Player.LocalPlayer.Vitals.PoisonPercent > 0.9f) ? _vignetteOnPoison : Mathf.Lerp(_vignettePoisonMinMax.x, _vignettePoisonMinMax.y, Player.LocalPlayer.Vitals.PoisonPercent));
		}
		else if (flag)
		{
			b2 = _vignetteOnFullness;
		}
		if (!Mathf.Approximately(_curVignetteIntensity, b2))
		{
			Color b3 = (flag2 ? _vignetteFireColor : (flag3 ? _vignettePoisonColor : (flag ? _vignetteRestoreHungerColor : Color.black)));
			float num = (flag ? _vignetteEatEffectLerpSpeed : _vignetteLerpSpeed);
			_curVignetteIntensity = Mathf.Lerp(_curVignetteIntensity, b2, Time.deltaTime * num);
			_curVignetteColor = Color.Lerp(_curVignetteColor, b3, Time.deltaTime * num);
			ShaderManager.UpdateVignette(_curVignetteIntensity, _curVignetteColor);
		}
	}

	public void OnRestoredFullness()
	{
		if (!_skipRestoreFullnessVignette)
		{
			_lastTimeEaten = Time.time;
		}
		_skipRestoreFullnessVignette = false;
	}

	public void OnDeath()
	{
		_skipRestoreFullnessVignette = true;
	}

	public void OnTakeDamage()
	{
		_curVignetteIntensity = _vignetteOnDamage;
		_curVignetteColor = _vignetteDamageColor;
	}
}
