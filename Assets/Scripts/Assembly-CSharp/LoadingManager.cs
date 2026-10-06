using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
	private static LoadingManager _instance;

	[SerializeField]
	private TextMeshProUGUI _loadingText;

	[SerializeField]
	private Image _loadingBackground;

	[SerializeField]
	private float _loadTweenTime = 0.25f;

	[SerializeField]
	private float _unloadBackgroundTweenTime = 2f;

	private static bool _isShowingLoading;

	public static bool IsLoadingIn { get; private set; }

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		_loadingText.gameObject.SetActive(value: false);
	}

	public static void ToggleLoading(bool to, bool instant = false)
	{
		if ((bool)_instance && _isShowingLoading != to)
		{
			_instance._loadingText.gameObject.SetActive(value: true);
			_isShowingLoading = to;
			Vector3 to2 = (to ? Vector3.one : Vector3.zero);
			LeanTweenType ease = (to ? LeanTweenType.easeOutBack : LeanTweenType.easeInBack);
			LeanTween.cancel(_instance._loadingText.gameObject);
			if (to)
			{
				_instance._loadingText.transform.localScale = Vector3.zero;
				_instance._loadingBackground.color = new Color(0f, 0f, 0f, 0f);
			}
			LeanTween.scale(_instance._loadingText.gameObject, to2, _instance._loadTweenTime).setEase(ease).setOnComplete(DisableLoadingText);
			if (!to)
			{
				IsLoadingIn = true;
			}
			float a = ((!to) ? 1 : 0);
			_instance._loadingBackground.color = new Color(0f, 0f, 0f, a);
			float to3 = (to ? 1f : 0f);
			float time = ((to | instant) ? _instance._loadTweenTime : _instance._unloadBackgroundTweenTime);
			LeanTween.cancel(_instance._loadingBackground.gameObject);
			LeanTween.value(_instance._loadingBackground.gameObject, _instance._loadingBackground.color.a, to3, time).setOnUpdate(SetBackgroundAlpha).setOnComplete(LoadingInFinished);
		}
	}

	private static void DisableLoadingText()
	{
		if ((bool)_instance && !_isShowingLoading)
		{
			_instance._loadingText.gameObject.SetActive(value: false);
		}
	}

	private static void SetBackgroundAlpha(float to)
	{
		if ((bool)_instance)
		{
			_instance._loadingBackground.color = new Color(0f, 0f, 0f, to);
		}
	}

	private static void LoadingInFinished()
	{
		IsLoadingIn = false;
	}
}
