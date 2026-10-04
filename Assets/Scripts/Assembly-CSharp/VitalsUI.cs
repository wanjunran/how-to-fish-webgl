using UnityEngine;
using UnityEngine.UI;

public class VitalsUI : MonoBehaviour
{
	[SerializeField]
	private Image _healthPercent;

	[SerializeField]
	private Image _healthPercentLerped;

	[SerializeField]
	private Image _healthImage;

	[Space]
	[SerializeField]
	private Image _fullnessPercent;

	[SerializeField]
	private Image _fullnessPercentLerped;

	[SerializeField]
	private Image _fullnessImage;

	[Header("Poison")]
	[SerializeField]
	private Image _poisonImage;

	[Space]
	[SerializeField]
	private Image _poisonPercent;

	[SerializeField]
	private Image _poisonPercentLerped;

	[SerializeField]
	private CanvasGroup _poisonGroup;

	[Header("Fire")]
	[SerializeField]
	private Image _fireImage;

	[Space]
	[SerializeField]
	private Image _firePercent;

	[SerializeField]
	private Image _firePercentLerped;

	[SerializeField]
	private CanvasGroup _fireGroup;

	private Material _healthMat;

	private Material _fullnessMat;

	private Material _poisonMat;

	private Material _fireMat;

	private Vector2 _vitalsSmoothstep = new Vector2(0.1f, 0.5f);

	private Color _healthColor;

	private Color _fullnessColor;

	private static readonly int RotMultID = Shader.PropertyToID("_Rotation_Multiplier");

	private void Awake()
	{
		_healthColor = _healthImage.color;
		_fullnessColor = _fullnessImage.color;
		_healthMat = new Material(_healthImage.material);
		_healthImage.material = _healthMat;
		_fullnessMat = new Material(_fullnessImage.material);
		_fullnessImage.material = _fullnessMat;
		_poisonMat = new Material(_poisonImage.material);
		_poisonImage.material = _poisonMat;
		_poisonMat.SetFloat(RotMultID, 1f);
		_poisonGroup.alpha = 0f;
		_fireMat = new Material(_fireImage.material);
		_fireImage.material = _fireMat;
		_fireMat.SetFloat(RotMultID, 1f);
		_fireGroup.alpha = 0f;
	}

	private void OnDestroy()
	{
		Object.Destroy(_healthMat);
		Object.Destroy(_fullnessMat);
		Object.Destroy(_poisonMat);
		Object.Destroy(_fireMat);
	}

	public void SetPlayerHp(float percent)
	{
		LeanTween.cancel(_healthPercentLerped.gameObject);
		if (_healthPercent.fillAmount > percent)
		{
			LeanTween.value(_healthPercentLerped.gameObject, _healthPercentLerped.fillAmount, percent, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(LerpPlayerHp);
		}
		else
		{
			_healthPercentLerped.fillAmount = percent;
		}
		_healthPercent.fillAmount = percent;
		Vector2 vitalsSmoothstep = _vitalsSmoothstep;
		float num = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(vitalsSmoothstep.x, vitalsSmoothstep.y, percent));
		_healthImage.color = Color.Lerp(_healthColor, Color.white, num);
		_healthMat.SetFloat(RotMultID, 1f - num);
	}

	private void LerpPlayerHp(float to)
	{
		_healthPercentLerped.fillAmount = to;
	}

	public void SetPlayerFullness(float percent)
	{
		LeanTween.cancel(_fullnessPercentLerped.gameObject);
		if (_fullnessPercent.fillAmount > percent)
		{
			LeanTween.value(_fullnessPercentLerped.gameObject, _fullnessPercentLerped.fillAmount, percent, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(LerpPlayerFullness);
		}
		else
		{
			_fullnessPercentLerped.fillAmount = percent;
		}
		_fullnessPercent.fillAmount = percent;
		Vector2 vitalsSmoothstep = _vitalsSmoothstep;
		float num = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(vitalsSmoothstep.x, vitalsSmoothstep.y, percent));
		_fullnessImage.color = Color.Lerp(_fullnessColor, Color.white, num);
		_fullnessMat.SetFloat(RotMultID, 1f - num);
	}

	private void LerpPlayerFullness(float to)
	{
		_fullnessPercentLerped.fillAmount = to;
	}

	public void SetPlayerPoison(float percent)
	{
		LeanTween.cancel(_poisonGroup.gameObject);
		LeanTween.value(_poisonGroup.gameObject, _poisonGroup.alpha, (percent != 0f) ? 1 : 0, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(LerpPoisonCanvas);
		LeanTween.cancel(_poisonPercentLerped.gameObject);
		if (_poisonPercent.fillAmount > percent)
		{
			LeanTween.value(_poisonPercentLerped.gameObject, _poisonPercentLerped.fillAmount, percent, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(LerpPlayerPoison);
		}
		else
		{
			_poisonPercentLerped.fillAmount = percent;
		}
		_poisonPercent.fillAmount = percent;
	}

	private void LerpPoisonCanvas(float to)
	{
		_poisonGroup.alpha = to;
	}

	private void LerpPlayerPoison(float to)
	{
		_poisonPercentLerped.fillAmount = to;
	}

	public void SetPlayerFire(float percent)
	{
		LeanTween.cancel(_fireGroup.gameObject);
		LeanTween.value(_fireGroup.gameObject, _fireGroup.alpha, (percent != 0f) ? 1 : 0, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(LerpFireCanvas);
		LeanTween.cancel(_firePercentLerped.gameObject);
		if (_firePercent.fillAmount > percent)
		{
			LeanTween.value(_firePercentLerped.gameObject, _firePercentLerped.fillAmount, percent, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(LerpPlayerFire);
		}
		else
		{
			_firePercentLerped.fillAmount = percent;
		}
		_firePercent.fillAmount = percent;
	}

	private void LerpFireCanvas(float to)
	{
		_fireGroup.alpha = to;
	}

	private void LerpPlayerFire(float to)
	{
		_firePercentLerped.fillAmount = to;
	}
}
