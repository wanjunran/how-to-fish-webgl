using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SlotMachine : MonoBehaviour
{
	private static SlotMachine _instance;

	[SerializeField]
	private Item[] _availableItems;

	[SerializeField]
	private bool _excludeBoat;

	[SerializeField]
	private MachineSlot[] _machineSlots;

	[SerializeField]
	private Transform _slotDestroyTarget;

	[SerializeField]
	private Camera _slotCam;

	[SerializeField]
	private TextMeshProUGUI _rollerText;

	[SerializeField]
	private GameObject _rollerLine;

	[SerializeField]
	private RectTransform[] _showWhenInactive;

	[SerializeField]
	private float _slotSize = 0.275f;

	[SerializeField]
	private TextMeshProUGUI _prizesText;

	[SerializeField]
	private RectTransform _rollHolder;

	[SerializeField]
	private RectTransform _roller;

	[SerializeField]
	private Transform _arm;

	[SerializeField]
	private Vector3 _armPullRot;

	private static Player _playerWhoRolled;

	private static byte _itemIndex;

	private static byte _skinIndex;

	private static byte _rolledIndex;

	private static float _mostRightPos;

	private static float _curMostRightPos;

	private static float _curHolderPosToCheck;

	private static int _slotToMove;

	private static Vector2 _defaultRollHolderSize;

	private static float _inactiveStuffHeight;

	private static float _lastRollPosX;

	private static Vector3 _armOrigRot;

	private static readonly Vector3[] _rectCorners = new Vector3[4];

	private int _lastAudioBoundaryIndex;

	private Vector2 _defaultSlotSize;

	public static Item[] AvailableItems { get; private set; }

	public static bool ExcludeBoat { get; private set; }

	public static Transform SlotDestroyTarget { get; private set; }

	public static bool IsRolling { get; private set; }

	private void Awake()
	{
		_instance = this;
		ExcludeBoat = _excludeBoat;
		AvailableItems = new Item[_availableItems.Length];
		for (int i = 0; i < _availableItems.Length; i++)
		{
			AvailableItems[i] = _availableItems[i];
		}
		SlotDestroyTarget = _slotDestroyTarget;
		_defaultSlotSize = _machineSlots[0].Rect.sizeDelta;
		_mostRightPos = _machineSlots[^1].Rect.localPosition.x;
		_slotCam.clearFlags = CameraClearFlags.Color;
		_slotCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
		_slotCam.enabled = false;
		_slotCam.targetTexture = GameInfo.SlotMachineTexture;
		_rollerText.text = "";
		_defaultRollHolderSize = _rollHolder.sizeDelta;
		_rollHolder.sizeDelta = new Vector2(_defaultRollHolderSize.x, 0f);
		_inactiveStuffHeight = _showWhenInactive[0].sizeDelta.y;
		_armOrigRot = _arm.localEulerAngles;
	}

	private void Start()
	{
		UpdatePrizesText();
	}

	private void OnEnable()
	{
		LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
	}

	private void OnDisable()
	{
		LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
	}

	private void OnSelectedLocaleChanged(Locale locale)
	{
		UpdatePrizesText();
	}

	private void UpdatePrizesText()
	{
		_prizesText.text = "<color=#FFEF4D><b>" + LocalizationManager.PrizesLocalized.GetLocalizedString() + ":</color></b> ";
		if (!_excludeBoat)
		{
			TextMeshProUGUI prizesText = _prizesText;
			prizesText.text = prizesText.text + LocalizationManager.BoatLocalized.GetLocalizedString() + ", ";
		}
		for (int i = 0; i < _availableItems.Length; i++)
		{
			_prizesText.text += _availableItems[i].GetName();
			if (i != _availableItems.Length - 1)
			{
				_prizesText.text += ", ";
			}
		}
	}

	private void OnDestroy()
	{
		IsRolling = false;
	}

	public static void Roll(Player roller, byte[] itemIDs, byte[] itemSkins, byte rolled)
	{
		if ((bool)_instance)
		{
			for (int i = 0; i < _instance._machineSlots.Length; i++)
			{
				_instance._machineSlots[i].SetIndex(itemIDs[i], itemSkins[i]);
			}
			_instance.UpdateSlotMachineCamera();
			_instance._rollerText.text = roller.SteamName + " " + LocalizationManager.IsRollingLocalized.GetLocalizedString();
			_playerWhoRolled = roller;
			_itemIndex = itemIDs[rolled];
			_skinIndex = itemSkins[rolled];
			_rolledIndex = rolled;
			_curMostRightPos = _mostRightPos;
			_curHolderPosToCheck = -1f;
			_slotToMove = 0;
			IsRolling = true;
			_instance._rollerLine.SetActive(value: true);
			RectTransform[] showWhenInactive = _instance._showWhenInactive;
			foreach (RectTransform obj in showWhenInactive)
			{
				LeanTween.cancel(obj);
				LeanTween.size(obj, new Vector2(obj.sizeDelta.x, 0f), 0.25f).setEase(LeanTweenType.easeOutQuad);
			}
			for (int k = 0; k < _instance._machineSlots.Length; k++)
			{
				LeanTween.cancel(_instance._machineSlots[k].Rect);
				_instance._machineSlots[k].MoveTo((float)k * _instance._slotSize);
				_instance._machineSlots[k].Rect.sizeDelta = _instance._defaultSlotSize;
				LeanTween.cancel(_instance._machineSlots[k].ItemImage);
				_instance._machineSlots[k].ItemImage.sizeDelta = _instance._defaultSlotSize;
			}
			float num = _instance._slotSize * 50f;
			float num2 = (float)(int)rolled * _instance._slotSize;
			float num3 = _instance._slotSize * 0.4f;
			float num4 = Mathf.Lerp(0f - num3, num3, (float)((double)(int)rolled * 0.1));
			Vector3 to = Vector3.left * (num + num2 + num4);
			AudioManager.PlayRandomClipAt("CasinoStart_0", 1, 3, _instance._arm.position, variation: false, AudioDistance.Short);
			AudioManager.PlayClipAt("CasinoRoll", _instance.transform.position, variation: false, AudioDistance.Short, 0.75f);
			LeanTween.cancel(_instance._arm.gameObject);
			LeanTween.rotateLocal(_instance._arm.gameObject, _instance._armPullRot, 0.25f).setEaseInOutExpo().setOnComplete(_instance.MoveArmBack);
			LeanTween.cancel(_instance._roller.gameObject);
			_instance._roller.localPosition = Vector3.zero;
			_instance.CaptureAudioBoundary();
			LeanTween.moveLocal(_instance._roller.gameObject, to, 6f).setEase(LeanTweenType.easeOutQuart).setOnUpdate(_instance.MoveSlots)
				.setOnComplete(_instance.OnRollFinished);
			LeanTween.cancel(_instance._rollHolder);
			_instance._rollHolder.sizeDelta = new Vector2(_defaultRollHolderSize.x, 0f);
			LeanTween.size(_instance._rollHolder, _defaultRollHolderSize, 0.25f).setEase(LeanTweenType.easeOutQuad);
		}
	}

	private void MoveSlots(float t)
	{
		if (_roller.localPosition.x < _curHolderPosToCheck)
		{
			_machineSlots[_slotToMove].MoveTo(_curMostRightPos + _slotSize);
			_curMostRightPos += _slotSize;
			_curHolderPosToCheck -= _slotSize;
			_slotToMove++;
			if (_slotToMove > _machineSlots.Length - 1)
			{
				_slotToMove = 0;
			}
		}
		PlaySlotBoundaryAudio();
	}

	private void CaptureAudioBoundary()
	{
		_lastAudioBoundaryIndex = GetAudioBoundaryIndex();
	}

	private void PlaySlotBoundaryAudio()
	{
		int audioBoundaryIndex = GetAudioBoundaryIndex();
		if (audioBoundaryIndex != _lastAudioBoundaryIndex)
		{
			int num = Mathf.Abs(audioBoundaryIndex - _lastAudioBoundaryIndex);
			for (int i = 0; i < num; i++)
			{
				AudioManager.PlayClipAt("TypingClick3", base.transform.position, variation: false, AudioDistance.Short, 0.25f, 0f);
			}
			_lastAudioBoundaryIndex = audioBoundaryIndex;
		}
	}

	private int GetAudioBoundaryIndex()
	{
		return Mathf.FloorToInt((GetRollerLineLocalX() + _slotSize * 0.5f) / _slotSize);
	}

	private float GetRollerLineLocalX()
	{
		if (_rollerLine.transform is RectTransform rectTransform)
		{
			rectTransform.GetWorldCorners(_rectCorners);
			Vector3 position = (_rectCorners[0] + _rectCorners[2]) * 0.5f;
			return _roller.InverseTransformPoint(position).x;
		}
		return _roller.InverseTransformPoint(_rollerLine.transform.position).x;
	}

	private void OnRollFinished()
	{
		IsRolling = false;
		ParticleManager.Play("Confetti", base.transform.position);
		_rollerText.text = "";
		for (int i = 0; i < _machineSlots.Length; i++)
		{
			LeanTween.cancel(_machineSlots[i].gameObject);
			if (i != _rolledIndex)
			{
				LeanTween.size(_machineSlots[i].Rect, Vector3.zero, 0.25f).setEase(LeanTweenType.easeOutQuad);
				continue;
			}
			LeanTween.size(_machineSlots[i].Rect, Vector2.one, 1f).setEase(LeanTweenType.easeOutElastic);
			LeanTween.moveLocal(_machineSlots[i].gameObject, -_roller.localPosition, 1f).setEase(LeanTweenType.easeOutElastic);
			LeanTween.cancel(_machineSlots[i].ItemImage);
			LeanTween.size(_machineSlots[i].ItemImage, _defaultSlotSize * 1.5f, 1f).setEase(LeanTweenType.easeOutElastic);
		}
		_instance._rollerLine.SetActive(value: false);
		LeanTween.cancel(_instance._roller.gameObject);
		LeanTween.value(_instance._roller.gameObject, 0f, 1f, 6f).setOnComplete(ResetText);
		ItemSkin unlockedSkin = ((_itemIndex != byte.MaxValue) ? GameInfo.IDToItem(_itemIndex).SkinPreset.Skins[_skinIndex] : BoatManager.Boat.SkinPreset.Skins[_skinIndex]);
		AudioManager.PlayClipAt((unlockedSkin.Rarity == Rarity.Legendary) ? "CasinoWinEpic" : "CasinoWinGrey", base.transform.position, variation: false, AudioDistance.Short);
		if (_playerWhoRolled.Owner.IsLocalClient)
		{
			SaveManager.UnlockSkin(_itemIndex, _skinIndex);
			if (_itemIndex == byte.MaxValue)
			{
				AchievementManager.CheckSlotMachineAchievement(unlockedSkin);
			}
			else
			{
				AchievementManager.CheckSlotMachineAchievement(unlockedSkin);
			}
		}
	}

	private void ResetText()
	{
		RectTransform[] showWhenInactive = _instance._showWhenInactive;
		foreach (RectTransform obj in showWhenInactive)
		{
			LeanTween.cancel(obj);
			LeanTween.size(obj, new Vector2(obj.sizeDelta.x, _inactiveStuffHeight), 0.25f).setEase(LeanTweenType.easeOutQuad);
		}
		_rollerText.text = "";
		LeanTween.cancel(_instance._rollHolder);
		LeanTween.size(_instance._rollHolder, new Vector2(_defaultRollHolderSize.x, 0f), 0.25f).setEase(LeanTweenType.easeInBack);
	}

	private void UpdateSlotMachineCamera()
	{
		_slotCam.Render();
	}

	private void MoveArmBack()
	{
		LeanTween.cancel(_instance._arm.gameObject);
		LeanTween.rotateLocal(_instance._arm.gameObject, _armOrigRot, 0.5f).setEaseInOutExpo();
	}

	private void OnTriggerStay(Collider other)
	{
		if (!Server.Instance || !Server.Instance.IsServerInitialized || IsRolling)
		{
			return;
		}
		Item item = ItemManager.Get(other);
		if ((bool)item && !item.Holder && (!item.Creature || (item.Creature.IsDead && item.Creature.IsDrip)) && ((bool)item.Creature || item.IsQuestItem))
		{
			Player player = (item.Holder ? item.Holder : item.LastHolder);
			if ((bool)player)
			{
				item.DestroyItem(4);
				SlotMachineManager.RollRandom(player);
			}
		}
	}
}
