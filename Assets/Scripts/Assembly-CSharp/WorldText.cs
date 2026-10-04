using TMPro;
using UnityEngine;

public class WorldText : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private float _upMultiplier = 1f;

	[SerializeField]
	[Range(0.1f, 10f)]
	private float _hoverTime = 3f;

	[SerializeField]
	private float _scaleTime = 0.1f;

	[Header("Damage Animation")]
	[SerializeField]
	private float _damageScaleTime = 0.15f;

	[SerializeField]
	private float _damageUpMultiplier = 0.25f;

	[SerializeField]
	private float _damageRandomRot = 30f;

	private float _inOutMultiplier = 0.1f;

	private float _rotOffset;

	public void ScaleAnimation(bool scaleIn)
	{
		if (scaleIn)
		{
			base.transform.localScale = Vector3.zero;
		}
		Vector3 to = (scaleIn ? Vector3.one : Vector3.zero);
		LeanTweenType ease = (scaleIn ? LeanTweenType.easeOutBack : LeanTweenType.easeInBack);
		LeanTween.cancel(base.gameObject);
		LeanTween.scale(base.gameObject, to, _scaleTime).setEase(ease);
	}

	public void DamageAnimation()
	{
		base.transform.localScale = Vector3.zero;
		base.transform.forward = GameInfo.CurCamera.transform.forward;
		_rotOffset = Random.Range(0f - _damageRandomRot, _damageRandomRot);
		base.transform.Rotate(base.transform.forward, _rotOffset);
		LeanTween.move(base.gameObject, base.transform.position + base.transform.up * _damageUpMultiplier, _damageScaleTime * 2f).setEase(LeanTweenType.easeOutBack);
		LeanTween.scale(base.gameObject, Vector3.one, _scaleTime).setEase(LeanTweenType.easeOutBack).setOnComplete(OutDamageAnimation);
	}

	private void OutDamageAnimation()
	{
		LeanTween.scale(base.gameObject, Vector3.zero, _damageScaleTime).setEase(LeanTweenType.easeInQuad).setDestroyOnComplete(doesDestroy: true);
	}

	public void FloatAndRemoveAnimation()
	{
		base.transform.localScale = Vector3.zero;
		LeanTween.scale(base.gameObject, Vector3.one, _hoverTime * _inOutMultiplier).setEase(LeanTweenType.easeOutBack);
		LeanTween.move(base.gameObject, base.transform.position + Vector3.up * _upMultiplier, _hoverTime).setOnComplete(RemoveAnimation);
	}

	private void RemoveAnimation()
	{
		LeanTween.cancel(base.gameObject);
		LeanTween.scale(base.gameObject, Vector3.zero, _hoverTime * _inOutMultiplier).setEase(LeanTweenType.easeInBack);
		LeanTween.move(base.gameObject, base.transform.position + Vector3.up * (_upMultiplier * _inOutMultiplier), _hoverTime * _inOutMultiplier).setDestroyOnComplete(doesDestroy: true);
	}

	public void SetText(string text, float size = 0.4f)
	{
		_text.text = text;
		_text.fontSize = size;
	}

	private void LateUpdate()
	{
		base.transform.forward = GameInfo.CurCamera.transform.forward;
		base.transform.Rotate(base.transform.forward, _rotOffset);
	}
}
