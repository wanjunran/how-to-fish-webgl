using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponUI : MonoBehaviour
{
	[Header("Sniper")]
	[SerializeField]
	[Tooltip("UI to enable when scoping")]
	private CanvasGroup _sniperUI;

	[SerializeField]
	[Tooltip("Sniper UI to move to target pos")]
	private RectTransform _sniperLinesHolder;

	[SerializeField]
	private RectTransform _horizontalSniperLine;

	[SerializeField]
	private RectTransform _verticalSniperLine;

	[Header("Attachment Info")]
	[SerializeField]
	private RectTransform _weaponInspectHolder;

	[FormerlySerializedAs("_inspectedWeaponText")]
	[SerializeField]
	private TextMeshProUGUI _inspectedNameText;

	[FormerlySerializedAs("_attachmentText")]
	[SerializeField]
	private TextMeshProUGUI _descriptionText;

	private bool _isShowingSniperUI;

	private Item _inspectedItem;

	private Purchasable _inspectedPurchasable;

	private bool _isInspecting;

	private bool _fromPlayerInput;

	private string _extraDescription;

	private bool _isChangingSize;

	public void InitializeLocal()
	{
		ToggleSniperUI(to: false);
	}

	public void ToggleSniperUI(bool to)
	{
		if (_isShowingSniperUI != to)
		{
			ChatManager.ToggleChatCanvas(!to);
			_isShowingSniperUI = to;
			_sniperUI.gameObject.SetActive(value: true);
			LeanTween.cancel(_sniperUI.gameObject);
			PlayerUI.ToggleBossCanvas(!to);
			LeanTween.value(_sniperUI.gameObject, _sniperUI.alpha, to ? 1 : 0, 0.25f).setEase(LeanTweenType.easeOutQuart).setOnUpdate(UpdateSniperUIAlpha);
			PlayerUI.ToggleMainCanvas(!to);
		}
	}

	private void UpdateSniperUIAlpha(float to)
	{
		_sniperUI.alpha = to;
	}

	private void Update()
	{
		if (_isInspecting && (bool)_inspectedItem)
		{
			UpdateInspectedItemText();
		}
	}

	public void SetSniperUI(Vector3 position, float posWeight, float scale)
	{
		Vector3 position2 = GameInfo.CurCamera.WorldToScreenPoint(position);
		_sniperLinesHolder.position = position2;
		_sniperLinesHolder.localPosition = Vector3.Lerp(_sniperLinesHolder.localPosition, Vector3.zero, posWeight);
		Vector2 aimPos = GameInfo.CurCamera.ScreenToViewportPoint(_sniperLinesHolder.position);
		float num = 1080f / (float)Screen.height;
		Vector2 sizeDelta = _horizontalSniperLine.sizeDelta;
		sizeDelta.y = num;
		_horizontalSniperLine.sizeDelta = sizeDelta;
		Vector2 sizeDelta2 = _verticalSniperLine.sizeDelta;
		sizeDelta2.x = num;
		_verticalSniperLine.sizeDelta = sizeDelta2;
		ShaderManager.UpdateSniperUI(aimPos, scale);
	}

	public void ShowInspectText(Item item, string extraDescription)
	{
		_isInspecting = true;
		_inspectedItem = item;
		_inspectedPurchasable = null;
		_extraDescription = extraDescription;
		UpdateInspectedItemText();
		_weaponInspectHolder.gameObject.SetActive(value: true);
		LeanTween.cancel(_weaponInspectHolder);
		LeanTween.value(_weaponInspectHolder.gameObject, 0f, 1f, 0.25f).setEase(LeanTweenType.easeOutBack).setOnUpdate(UpdateInspectSize)
			.setOnComplete(OnSizeComplete);
		LeanTween.value(_weaponInspectHolder.gameObject, 0f, 1f, 4f).setOnComplete(HideInspectInfo);
	}

	public void ShowInspectText(Purchasable purchasable, string purchasableName, string extraDescription)
	{
		_isInspecting = true;
		_inspectedItem = null;
		_inspectedPurchasable = purchasable;
		_inspectedNameText.text = purchasableName;
		_descriptionText.text = extraDescription;
		_weaponInspectHolder.gameObject.SetActive(value: true);
		LeanTween.cancel(_weaponInspectHolder);
		LeanTween.value(_weaponInspectHolder.gameObject, 0f, 1f, 0.25f).setEase(LeanTweenType.easeOutBack).setOnUpdate(UpdateInspectSize)
			.setOnComplete(OnSizeComplete);
	}

	public void HideInspectText(Purchasable purchasable)
	{
		if (_isInspecting && !(_inspectedPurchasable != purchasable))
		{
			HideInspectInfo();
		}
	}

	private void UpdateInspectedItemText()
	{
		_inspectedNameText.text = _inspectedItem.GetName();
		string text = ((!_inspectedItem.IsQuestItem) ? $"{LocalizationManager.WorthLocalized.GetLocalizedString()}: {_inspectedItem.TotalWorth:0.##}" : LocalizationManager.QuestItemLocalized.GetLocalizedString());
		if (_inspectedItem.Cookness != 0f)
		{
			text += $"\n{LocalizationManager.CookingLocalized.GetLocalizedString()}: {GameInfo.CooknessWorthCurve.Evaluate(_inspectedItem.Cookness):0.##}x";
		}
		if (_extraDescription != "")
		{
			text = text + "\n\n" + _extraDescription;
		}
		_descriptionText.text = text;
		if (!_isChangingSize)
		{
			float num = _inspectedNameText.rectTransform.sizeDelta.y + _descriptionText.rectTransform.sizeDelta.y + 10f;
			if (!Mathf.Approximately(_weaponInspectHolder.sizeDelta.y, num))
			{
				LeanTween.cancel(_weaponInspectHolder);
				LeanTween.value(_weaponInspectHolder.gameObject, _weaponInspectHolder.sizeDelta.y / num, 1f, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateInspectSize)
					.setOnComplete(OnSizeComplete);
				LeanTween.value(_weaponInspectHolder.gameObject, 0f, 1f, 4f).setOnComplete(HideInspectInfo);
			}
		}
	}

	private void HideInspectInfo()
	{
		_isInspecting = false;
		_inspectedItem = null;
		_inspectedPurchasable = null;
		LeanTween.cancel(_weaponInspectHolder);
		LeanTween.value(_weaponInspectHolder.gameObject, 1f, 0f, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateInspectSize)
			.setOnComplete(() =>
			{
				_weaponInspectHolder.gameObject.SetActive(value: false);
			})
			.setOnComplete(OnSizeComplete);
	}

	private void UpdateInspectSize(float lerp)
	{
		_isChangingSize = true;
		float b = _inspectedNameText.rectTransform.sizeDelta.y + _descriptionText.rectTransform.sizeDelta.y + 10f;
		float y = Mathf.LerpUnclamped(0f, b, lerp);
		_weaponInspectHolder.sizeDelta = new Vector2(_weaponInspectHolder.sizeDelta.x, y);
	}

	private void OnSizeComplete()
	{
		_isChangingSize = false;
	}
}
