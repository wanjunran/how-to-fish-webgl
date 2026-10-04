using UnityEngine;

public class NetworkPrompts : MonoBehaviour
{
	private static NetworkPrompts _instance;

	[Header("Networking prompts")]
	[SerializeField]
	private RectTransform _networkPromptBackground;

	private void Awake()
	{
		_instance = this;
	}

	public static void ShowVersionMismatch()
	{
		if ((bool)_instance)
		{
			LeanTween.cancel(_instance._networkPromptBackground.gameObject);
			LeanTween.value(_instance._networkPromptBackground.gameObject, 0f, 60f, 0.5f).setEase(LeanTweenType.easeInOutBack).setOnUpdate(_instance.SetNetworkBackgroundSize)
				.setOnComplete(_instance.ResetNetworkBackgroundDelayed);
		}
	}

	private void SetNetworkBackgroundSize(float to)
	{
		_networkPromptBackground.sizeDelta = new Vector2(_instance._networkPromptBackground.sizeDelta.x, to);
	}

	private void ResetNetworkBackgroundDelayed()
	{
		LeanTween.value(_instance._networkPromptBackground.gameObject, 0f, 1f, 4f).setOnComplete(RemoveNetworkBackground);
	}

	private void RemoveNetworkBackground()
	{
		LeanTween.value(_instance._networkPromptBackground.gameObject, _instance._networkPromptBackground.sizeDelta.y, 0f, 0.5f).setEase(LeanTweenType.easeInBack).setOnUpdate(_instance.SetNetworkBackgroundSize)
			.setOnComplete(_instance.ResetNetworkBackgroundDelayed);
	}
}
