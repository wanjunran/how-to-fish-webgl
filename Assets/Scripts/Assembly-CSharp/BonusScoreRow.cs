using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusScoreRow : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _scoreText;

	[SerializeField]
	private TextMeshProUGUI _descriptionText;

	[SerializeField]
	private float _letterDelay = 0.01f;

	[SerializeField]
	private float _scaleOutTime = 0.25f;

	[SerializeField]
	private Color _typingColor;

	[SerializeField]
	private float _typingVol = 0.25f;

	[SerializeField]
	private float _typingClickVol = 0.5f;

	[Header("Finished Typing")]
	[SerializeField]
	private Color _typedColor = Color.white;

	[SerializeField]
	private float _typedScale = 1.1f;

	[SerializeField]
	private float _typedScaleTime = 0.25f;

	[SerializeField]
	private float _hideDelay = 1f;

	private int _score;

	private RectTransform _holder;

	public float HideDelay => _hideDelay;

	public float TypedScaleTime => _typedScaleTime;

	public TextMeshProUGUI ScoreText => _scoreText;

	public void SetLayoutToRebuild(RectTransform holder)
	{
		_holder = holder;
	}

	public void AnimateBonus(Bonus bonus)
	{
		base.gameObject.SetActive(value: true);
		StartCoroutine(TextAnimation(bonus.Name, bonus.Worth, isMainRow: false));
	}

	public void AnimateMainScore(string text, int worth)
	{
		base.gameObject.SetActive(value: true);
		StartCoroutine(TextAnimation(text, worth, isMainRow: true));
	}

	private IEnumerator TextAnimation(string description, float worth, bool isMainRow)
	{
		if (!isMainRow)
		{
			_scoreText.text = $"{worth}x";
		}
		LeanTween.cancel(base.gameObject);
		base.transform.localScale = Vector3.one;
		_descriptionText.color = _typingColor;
		_descriptionText.text = "";
		foreach (char c in description)
		{
			_descriptionText.text += c;
			AudioManager.PlayRandomGlobalClip("Typing", 1, 3, variation: false, _typingVol);
			yield return new WaitForSeconds(_letterDelay);
		}
		_descriptionText.color = _typedColor;
		_descriptionText.transform.localScale = Vector3.one * _typedScale;
		LeanTween.scale(_descriptionText.gameObject, Vector3.one, _typedScaleTime);
		Player.LocalPlayer.KillScore.AddScoreFromBonus(worth, isMainRow);
		AudioManager.PlayRandomGlobalClip("TypingClick", 1, 3, variation: false, _typingClickVol);
		yield return new WaitForSeconds(_typedScaleTime);
		if (!isMainRow)
		{
			yield return new WaitForSeconds(_hideDelay);
			HideAnimation();
		}
	}

	public void HideAnimation()
	{
		LeanTween.cancel(base.gameObject);
		LeanTween.scale(base.gameObject, Vector3.right, _scaleOutTime).setOnComplete(HideRow).setOnUpdate(UpdateLayoutGroup);
	}

	private void UpdateLayoutGroup(float _)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(_holder);
	}

	private void HideRow()
	{
		base.gameObject.SetActive(value: false);
	}
}
