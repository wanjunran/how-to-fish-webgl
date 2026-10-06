using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkipManager : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _skipGroup;

	[SerializeField]
	private Image _skipImage;

	[SerializeField]
	private float _skipGrounTime = 0.25f;

	[SerializeField]
	private float _totalSkipTime = 1f;

	private bool _isSkipping;

	private float _curSkipTime;

	public static event Action OnSkip;

	private void Start()
	{
		BindInputs();
	}

	private void OnDestroy()
	{
		UnbindInputs();
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["Skip"].performed += SkipInput;
		input.actions["Skip"].canceled += SkipInputCanceled;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["Skip"].performed -= SkipInput;
			input.actions["Skip"].canceled -= SkipInputCanceled;
		}
	}

	private void SkipInput(InputAction.CallbackContext context)
	{
		StartSkipping();
	}

	private void SkipInputCanceled(InputAction.CallbackContext context)
	{
		CancelSkipping();
	}

	private void StartSkipping()
	{
		if (OnSkip == null)
		{
			CancelSkipping();
			return;
		}
		LeanTween.cancel(_skipGroup.gameObject);
		LeanTween.value(_skipGroup.gameObject, _skipGroup.alpha, 1f, _skipGrounTime - _skipGroup.alpha).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateSkipAlpha);
		LeanTween.cancel(_skipImage.gameObject);
		LeanTween.value(_skipImage.gameObject, _skipImage.fillAmount, 1f, _totalSkipTime - _skipImage.fillAmount).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateSkipFill)
			.setOnComplete(Skipped);
	}

	private void UpdateSkipFill(float alpha)
	{
		_skipImage.fillAmount = alpha;
	}

	private void UpdateSkipAlpha(float alpha)
	{
		_skipGroup.alpha = alpha;
	}

	private void Skipped()
	{
		OnSkip?.Invoke();
		CancelSkipping();
	}

	private void CancelSkipping()
	{
		LeanTween.cancel(_skipGroup.gameObject);
		LeanTween.value(_skipGroup.gameObject, _skipGroup.alpha, 0f, _skipGrounTime - _skipGroup.alpha).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateSkipAlpha);
		LeanTween.cancel(_skipImage.gameObject);
		LeanTween.value(_skipImage.gameObject, _skipImage.fillAmount, 0f, _skipGrounTime - _skipGroup.alpha).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateSkipFill);
	}
}
