using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
	private static PlayerUI _instance;

	[Header("UI Components")]
	[SerializeField]
	private CloseItemsUI _closeItemsUI;

	[SerializeField]
	private ItemUI _itemUI;

	[SerializeField]
	private OnHitUI _onHitUI;

	[SerializeField]
	private FishingUI fishingUI;

	[SerializeField]
	private WeaponUI weaponUI;

	[SerializeField]
	private VitalsUI _vitalsUI;

	[SerializeField]
	private BossUI _bossUI;

	[SerializeField]
	private NpcUI _npcUI;

	[SerializeField]
	private VoiceUI _voiceUI;

	[SerializeField]
	private MoneyUI _moneyUI;

	[SerializeField]
	private DeathUI _deathUI;

	[SerializeField]
	private IslandUI _islandUI;

	[SerializeField]
	private ThinkingUI _thinkingUI;

	[Header("Canvas Groups")]
	[SerializeField]
	private CanvasGroup _mainCanvas;

	[SerializeField]
	private CanvasGroup _fxCanvas;

	[SerializeField]
	private CanvasGroup _bossCanvas;

	[Header("Prefabs")]
	[SerializeField]
	private TextMeshProUGUI _canvasTextPrefab;

	public static bool BossCanvasActive => _instance._bossCanvas.alpha == 1f;

	public static Transform FXCanvasTrans => _instance._fxCanvas.transform;

	public static TextMeshProUGUI CanvasTextPrefab => _instance._canvasTextPrefab;

	public static bool UIDisabled { get; private set; }

	public void InitializeLocal(Player player)
	{
		Setter.SetSingleInstance(ref _instance, this);
		weaponUI.InitializeLocal();
		_onHitUI.InitializeLocal();
		_moneyUI.InitializeLocal();
		_closeItemsUI.InitializeLocal();
		_thinkingUI.InitializeLocal();
	}

	public static void OnPause()
	{
		if ((bool)_instance)
		{
			ToggleMainCanvas(!PauseManager.IsPaused);
			ToggleFXCanvas(!PauseManager.IsPaused);
			ToggleBossCanvas(!PauseManager.IsPaused);
		}
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			ChatManager.ToggleChatCanvas(to: true);
		}
	}

	private void Update()
	{
		if (SteamManager.IsDev && Input.GetKeyDown(KeyCode.U) && (bool)Player.LocalPlayer && !Player.LocalPlayer.BlockInputs)
		{
			ToggleUIDisabled(!UIDisabled);
		}
	}

	public static void StartGivingUp(float totalTime)
	{
		if ((bool)_instance)
		{
			_instance._deathUI.StartGivingUp(totalTime);
		}
	}

	public static void StopGivingUp(float totalTime)
	{
		if ((bool)_instance)
		{
			_instance._deathUI.StopGivingUp(totalTime);
		}
	}

	public static void ToggleDeathUI(bool to)
	{
		if ((bool)_instance)
		{
			_instance._deathUI.ToggleDeathUI(to);
		}
	}

	public static void SetMoney(int to, int diff, bool gainedMoney)
	{
		if ((bool)_instance)
		{
			_instance._moneyUI.SetMoney(to, diff, gainedMoney);
		}
	}

	public static void StartDropUI(float time)
	{
		if ((bool)_instance)
		{
			_instance._itemUI.StartDropUI(time);
		}
	}

	public static void StopDropUI()
	{
		if ((bool)_instance)
		{
			_instance._itemUI.StopDropUI();
		}
	}

	public static void SetLookAtText(string text, Transform target = null, Vector3 textOffset = default(Vector3), bool dripText = false)
	{
		if ((bool)_instance)
		{
			_instance._itemUI.SetLookAtText(text, target, textOffset, dripText);
		}
	}

	public static void SetLookAtColor(Color color)
	{
		if ((bool)_instance)
		{
			_instance._itemUI.SetLookAtColor(color);
		}
	}

	public static void UpdateLookAtText(string text)
	{
		if ((bool)_instance)
		{
			_instance._itemUI.UpdateLookAtText(text);
		}
	}

	public static void HideLookAtText(string text = "")
	{
		if ((bool)_instance)
		{
			_instance._itemUI.HideLookAtText(text);
		}
	}

	public static void ForceHideLookAtText()
	{
		if ((bool)_instance)
		{
			_instance._itemUI.ForceHideLookAtText();
		}
	}

	public static void ToggleSniperUI(bool to)
	{
		if ((bool)_instance)
		{
			_instance.weaponUI.ToggleSniperUI(to);
		}
	}

	public static void ShowInspectInfo(Item item, string description = "")
	{
		if ((bool)_instance)
		{
			_instance.weaponUI.ShowInspectText(item, description);
		}
	}

	public static void ShowInspectInfo(Purchasable purchasable, string itemName, string description = "")
	{
		if ((bool)_instance)
		{
			_instance.weaponUI.ShowInspectText(purchasable, itemName, description);
		}
	}

	public static void HideInspectInfo(Purchasable purchasable)
	{
		if ((bool)_instance)
		{
			_instance.weaponUI.HideInspectText(purchasable);
		}
	}

	public static void SetSniperUI(Vector3 position, float posWeight, float scale)
	{
		if ((bool)_instance)
		{
			_instance.weaponUI.SetSniperUI(position, posWeight, scale);
		}
	}

	public static void AddHitMarker(Vector3 pos, bool killed)
	{
		if ((bool)_instance)
		{
			_instance._onHitUI.AddHitMarker(pos, killed);
		}
	}

	public static void AddDamageNumber(Vector3 pos, int damage, bool killed)
	{
		if ((bool)_instance)
		{
			_instance._onHitUI.AddDamageNumber(pos, damage, killed);
		}
	}

	public static void CaughtFishUI(Vector3 pos, bool isDrip)
	{
		if ((bool)_instance)
		{
			_instance.fishingUI.CaughtFishUI(pos, isDrip);
		}
	}

	public static void ShowSpecificItemInfo(Item item)
	{
		if ((bool)_instance)
		{
			_instance._itemUI.ShowSpecificItemInfo(item);
		}
	}

	public static void ShowSpecificItemInfo(HeldInfo info)
	{
		if ((bool)_instance)
		{
			_instance._itemUI.ShowSpecificItemInfo(info);
		}
	}

	public static void HideSpecificItemInfo()
	{
		if ((bool)_instance)
		{
			_instance._itemUI.HideSpecificItemInfo();
		}
	}

	public static void SetPlayerHp(float percent)
	{
		if ((bool)_instance)
		{
			_instance._vitalsUI.SetPlayerHp(percent);
		}
	}

	public static void SetPlayerFullness(float percent)
	{
		if ((bool)_instance)
		{
			_instance._vitalsUI.SetPlayerFullness(percent);
		}
	}

	public static void UpdateBossHp(int curHp)
	{
		if ((bool)_instance)
		{
			_instance._bossUI.UpdateBossHp(curHp);
		}
	}

	public static void ToggleBossUI(bool to)
	{
		if ((bool)_instance)
		{
			_instance._bossUI.ToggleBossUI(to);
		}
	}

	public static void SetNpcText(string to, Transform target)
	{
		if ((bool)_instance)
		{
			_instance._npcUI.SetNpcText(to, target);
		}
	}

	public static void OnBaitChange(int amount, bool increased)
	{
		if ((bool)_instance)
		{
			_instance.fishingUI.OnBaitChange(amount, increased);
		}
	}

	public static void SetPlayerPoison(float percent)
	{
		if ((bool)_instance)
		{
			_instance._vitalsUI.SetPlayerPoison(percent);
		}
	}

	public static void SetPlayerFire(float percent)
	{
		if ((bool)_instance)
		{
			_instance._vitalsUI.SetPlayerFire(percent);
		}
	}

	public static void SetVoiceVolume(float vol)
	{
		if ((bool)_instance)
		{
			_instance._voiceUI.SetVoiceVolume(vol);
		}
	}

	public static void ToggleUIDisabled(bool to)
	{
		UIDisabled = to;
		ToggleMainCanvas(!to);
		ToggleFXCanvas(!to);
		ToggleBossCanvas(!to);
		if (!SelfDrivingBoat.IsDriving || EndGameEffects.IsShowingEndGame || !to)
		{
			CanvasManager.TogglePermaCanvas(!to);
		}
	}

	public static void ToggleMainCanvas(bool to)
	{
		if ((bool)_instance)
		{
			_instance._mainCanvas.alpha = ((to && !PauseManager.IsPaused && !PlayerThinking.IsThinking && !UIDisabled && !Player.LocalPlayer.Dying.IsDead) ? 1 : 0);
		}
	}

	public static void ToggleFXCanvas(bool to)
	{
		if ((bool)_instance)
		{
			_instance._fxCanvas.alpha = ((to && !PauseManager.IsPaused && !PlayerThinking.IsThinking && !UIDisabled) ? 1 : 0);
		}
	}

	public static void ToggleBossCanvas(bool to)
	{
		if ((bool)_instance)
		{
			_instance._bossCanvas.alpha = ((to && !PauseManager.IsPaused && !PlayerThinking.IsThinking && !UIDisabled) ? 1 : 0);
		}
	}

	public static void ToggleIslandWarning(bool to)
	{
		if ((bool)_instance)
		{
			_instance._islandUI._islandUIToggleIslandWarning(to);
		}
	}

	public static void OnToggleThinking()
	{
		if ((bool)_instance)
		{
			ToggleMainCanvas(!PlayerThinking.IsThinking);
			ToggleFXCanvas(!PlayerThinking.IsThinking);
			ToggleBossCanvas(!PlayerThinking.IsThinking);
			_instance._thinkingUI.ToggleThinking();
		}
	}

	public static void ScrollThinkingPage(int input)
	{
		if ((bool)_instance)
		{
			_instance._thinkingUI.ScrollThinkingPage(input);
		}
	}

	public static void OnNewFishCaught(Creature creature)
	{
		if ((bool)_instance)
		{
			_instance.fishingUI.OnNewFishCaught(creature);
		}
	}
}
