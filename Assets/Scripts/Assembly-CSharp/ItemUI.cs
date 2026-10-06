using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _lookingAtText;

	[SerializeField]
	private LocalizeStringEvent _itemInfoEvent;

	[SerializeField]
	private TextMeshProUGUI _itemInfoText;

	[SerializeField]
	private Image _dropCircle;

	[SerializeField]
	private DripText _dripText;

	private HeldInfo _curInfo;

	private bool _isShowingLookAtText;

	private bool _appendInputBindings;

	private string _inputBindingSuffix;

	private Transform _lookAtTarget;

	private Vector3 _textOffset;

	private void Start()
	{
		GameInfo.Input.onControlsChanged += OnControlsChanged;
		_itemInfoEvent.OnUpdateString.AddListener(OnItemInfoLocalized);
	}

	private void OnDestroy()
	{
		if ((bool)GameInfo.Input)
		{
			GameInfo.Input.onControlsChanged -= OnControlsChanged;
		}
		_itemInfoEvent.OnUpdateString.RemoveListener(OnItemInfoLocalized);
	}

	private void OnControlsChanged(PlayerInput playerInput)
	{
		if (_curInfo != null)
		{
			RefreshSpecificItemInfo();
		}
	}

	private void LateUpdate()
	{
		if ((bool)GameInfo.CurCamera && (bool)_lookAtTarget)
		{
			Vector3 position = GameInfo.CurCamera.WorldToScreenPoint(_lookAtTarget.position + _textOffset);
			if (position.z > 0f)
			{
				_lookingAtText.transform.position = position;
			}
		}
	}

	public void ShowSpecificItemInfo(Item item)
	{
		if ((bool)item.DeadPlayer)
		{
			ShowSpecificItemInfo(LocalizationManager.HeldDeadPlayerInfo);
		}
		else if ((bool)item.Tool && (bool)item.SkinPreset)
		{
			if (SaveManager.HasSkin(item.ID))
			{
				ShowSpecificItemInfo(LocalizationManager.HeldToolInfo, hideWithDelay: true);
			}
		}
		else if ((bool)item.Creature)
		{
			ShowSpecificItemInfo(LocalizationManager.HeldCreatureInfo, hideWithDelay: true);
		}
	}

	public void ShowSpecificItemInfo(HeldInfo info, bool hideWithDelay = false)
	{
		if (_curInfo != info)
		{
			_curInfo = info;
			RefreshSpecificItemInfo();
			_itemInfoText.transform.localScale = Vector3.zero;
			LeanTween.cancel(_itemInfoText.gameObject);
			LeanTween.scale(_itemInfoText.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
			if (hideWithDelay)
			{
				LeanTween.value(_itemInfoText.gameObject, 0f, 1f, 3f).setOnComplete(HideSpecificItemInfo);
			}
		}
	}

	private void RefreshSpecificItemInfo()
	{
		_itemInfoEvent.StringReference = _curInfo.InfoLocalized;
		_appendInputBindings = false;
		_inputBindingSuffix = null;
		InputActionReference[] inputActions = _curInfo.InputActions;
		if (inputActions != null && inputActions.Length > 0)
		{
			object[] array = new object[_curInfo.InputActions.Length];
			string[] array2 = new string[_curInfo.InputActions.Length];
			for (int i = 0; i < _curInfo.InputActions.Length; i++)
			{
				array2[i] = (string)(array[i] = SpriteManager.InputToSpriteName(_curInfo.InputActions[i].action) ?? "");
			}
			_itemInfoEvent.StringReference.Arguments = array;
			_inputBindingSuffix = string.Join(" ", array2);
			string localizedString = _itemInfoEvent.StringReference.GetLocalizedString();
			_appendInputBindings = !localizedString.Contains(array2[0]);
		}
		else
		{
			_itemInfoEvent.StringReference.Arguments = null;
		}
		if (_itemInfoEvent.StringReference != null)
		{
			_itemInfoEvent.RefreshString();
		}
	}

	private void OnItemInfoLocalized(string localizedText)
	{
		if (_appendInputBindings)
		{
			_itemInfoText.text = localizedText + " " + _inputBindingSuffix;
		}
	}

	public void HideSpecificItemInfo()
	{
		if (_curInfo != null)
		{
			_curInfo = null;
			_itemInfoText.transform.localScale = Vector3.one;
			LeanTween.cancel(_itemInfoText.gameObject);
			LeanTween.scale(_itemInfoText.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack);
		}
	}

	public void HideLookAtText(string text)
	{
		if (!(_lookingAtText.text != text) && _isShowingLookAtText)
		{
			_isShowingLookAtText = false;
			LeanTween.cancel(_lookingAtText.gameObject);
			LeanTween.scale(_lookingAtText.gameObject, Vector3.zero, 0.1f).setEase(LeanTweenType.easeInBack);
		}
	}

	public void ForceHideLookAtText()
	{
		if (_isShowingLookAtText)
		{
			LeanTween.cancel(_lookingAtText.gameObject);
			LeanTween.scale(_lookingAtText.gameObject, Vector3.zero, 0.1f).setEase(LeanTweenType.easeInBack);
		}
	}

	public void SetLookAtText(string text, Transform target, Vector3 textOffset, bool dripText = false)
	{
		_lookingAtText.color = Color.white;
		_lookAtTarget = target;
		_textOffset = textOffset;
		_isShowingLookAtText = true;
		if (!target)
		{
			_lookingAtText.transform.localPosition = Vector3.zero;
		}
		_lookingAtText.ClearMesh();
		_lookingAtText.SetText(text);
		LeanTween.cancel(_lookingAtText.gameObject);
		_lookingAtText.transform.localScale = Vector3.zero;
		LeanTween.scale(_lookingAtText.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
		_lookingAtText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
		_dripText.SetDrip(dripText);
	}

	public void StartDropUI(float time)
	{
		_dropCircle.gameObject.SetActive(value: true);
		LeanTween.cancel(_dropCircle.gameObject);
		LeanTween.value(_dropCircle.gameObject, 0f, 1f, time).setOnUpdate(UpdateDropUI);
	}

	private void UpdateDropUI(float time)
	{
		_dropCircle.fillAmount = time;
	}

	public void StopDropUI()
	{
		_dropCircle.gameObject.SetActive(value: false);
	}

	public void SetLookAtColor(Color color)
	{
		_lookingAtText.color = color;
	}

	public void UpdateLookAtText(string text)
	{
		_lookingAtText.text = text;
	}
}
