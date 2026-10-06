using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _npcCanvas;

	[SerializeField]
	private Image _npcBackground;

	[SerializeField]
	private float _npcTweenTime = 0.25f;

	[SerializeField]
	private TextMeshProUGUI _npcText;

	[SerializeField]
	private ContentSizeFitter _npcSizeFitter;

	[SerializeField]
	private float _showNpcTextTime = 5f;

	[SerializeField]
	private float _npcMaxHorizontalSize = 380f;

	[SerializeField]
	private float _npcBackgroundPadding = 10f;

	private Transform _npcTarget;

	public static bool NPCDialougeOpen { get; private set; }

	private void Awake()
	{
		_npcCanvas.alpha = 0f;
	}

	private void LateUpdate()
	{
		UpdateNpcTextPosition();
	}

	public void SetNpcText(string to, Transform target)
	{
		PlayerUI.HideLookAtText(LocalizationManager.TalkLocalized.GetLocalizedString() + " [" + SpriteManager.GetPickUpInput() + "]");
		NPCDialougeOpen = true;
		_npcSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		_npcText.text = to;
		_npcTarget = target;
		LayoutRebuilder.ForceRebuildLayoutImmediate(_npcText.rectTransform);
		if (_npcText.rectTransform.sizeDelta.x >= _npcMaxHorizontalSize)
		{
			_npcSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			_npcText.rectTransform.sizeDelta = new Vector2(_npcMaxHorizontalSize, _npcText.rectTransform.sizeDelta.y);
			LayoutRebuilder.ForceRebuildLayoutImmediate(_npcText.rectTransform);
		}
		LeanTween.cancel(_npcCanvas.gameObject);
		LeanTween.value(_npcCanvas.gameObject, 0f, 1f, _npcTweenTime).setEase(LeanTweenType.easeOutQuart).setOnUpdate(SetNpcCanvasAlpha);
		LeanTween.cancel(_npcBackground.gameObject);
		_npcBackground.rectTransform.localScale = Vector3.one * 0.75f;
		LeanTween.scale(_npcBackground.gameObject, Vector3.one, _npcTweenTime).setEase(LeanTweenType.easeOutQuart);
		LeanTween.value(_npcBackground.gameObject, 0f, 1f, _showNpcTextTime).setOnComplete(HideNpcCanvas);
		_npcBackground.transform.position = GameInfo.CurCamera.WorldToScreenPoint(target.position);
		Vector2 sizeDelta = _npcText.rectTransform.sizeDelta;
		sizeDelta += Vector2.one * _npcBackgroundPadding;
		_npcBackground.rectTransform.sizeDelta = sizeDelta;
	}

	private void UpdateNpcTextPosition()
	{
		if (!NPCDialougeOpen)
		{
			return;
		}
		if (!_npcTarget)
		{
			HideNpcCanvas();
			return;
		}
		Vector3 position = GameInfo.CurCamera.WorldToScreenPoint(_npcTarget.position);
		if (position.z < 0f)
		{
			LeanTween.cancel(_npcCanvas.gameObject);
			LeanTween.cancel(_npcBackground.gameObject);
			_npcCanvas.alpha = 0f;
			NPCDialougeOpen = false;
		}
		else
		{
			_npcBackground.transform.position = position;
		}
	}

	private void HideNpcCanvas()
	{
		LeanTween.cancel(_npcCanvas.gameObject);
		LeanTween.cancel(_npcBackground.gameObject);
		LeanTween.value(_npcCanvas.gameObject, 1f, 0f, _npcTweenTime).setEase(LeanTweenType.easeOutQuart).setOnUpdate(SetNpcCanvasAlpha);
		LeanTween.scale(_npcBackground.gameObject, Vector3.one * 1.25f, _npcTweenTime).setEase(LeanTweenType.easeOutQuart);
		NPCDialougeOpen = false;
	}

	private void SetNpcCanvasAlpha(float to)
	{
		_npcCanvas.alpha = to;
	}
}
