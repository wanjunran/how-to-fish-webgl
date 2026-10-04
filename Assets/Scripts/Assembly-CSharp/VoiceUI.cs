using UnityEngine;

public class VoiceUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _audioInputGroup;

	[SerializeField]
	private RectTransform _audioVolRect;

	private bool _isHidingVoiceVolume;

	public void SetVoiceVolume(float vol)
	{
		if (!ConnectionManager.IsUsingSteam)
		{
			return;
		}
		bool num = vol <= -70f;
		float t = (vol + 80.5f) * 0.0125f;
		_audioVolRect.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, t);
		if (num)
		{
			if (!_isHidingVoiceVolume)
			{
				_isHidingVoiceVolume = true;
				LeanTween.cancel(_audioInputGroup.gameObject);
				LeanTween.value(_audioInputGroup.gameObject, _audioInputGroup.alpha, 0f, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(SetAudioAlpha);
			}
		}
		else if (_isHidingVoiceVolume)
		{
			LeanTween.cancel(_audioInputGroup.gameObject);
			LeanTween.value(_audioInputGroup.gameObject, _audioInputGroup.alpha, 1f, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(SetAudioAlpha);
			_isHidingVoiceVolume = false;
		}
	}

	private void SetAudioAlpha(float to)
	{
		_audioInputGroup.alpha = to;
	}
}
