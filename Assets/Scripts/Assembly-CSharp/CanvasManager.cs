using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
	private static CanvasManager _instance;

	[SerializeField]
	private CanvasGroup _mainMenuCanvas;

	[SerializeField]
	private GameObject _howToFishTitle;

	[SerializeField]
	private List<GameObject> _allMainMenuButtons;

	[SerializeField]
	private GameObject _devStuff;

	[SerializeField]
	private GameObject _discordButton;

	[SerializeField]
	private TextMeshProUGUI _steamRelayText;

	[Header("Permanent Canvas")]
	[SerializeField]
	private CanvasGroup _permaCanvas;

	[SerializeField]
	private TextMeshProUGUI _versionText;

	[Header("Underwater Canvas")]
	[SerializeField]
	private Image _underwaterImage;

	[Header("Black Screen")]
	[SerializeField]
	private Image _blackScreen;

	[SerializeField]
	private GameObject _unseenCharacterNotification;

	[Header("Steam Relay Localization")]
	[SerializeField]
	private LocalizedString _steamRelayReady;

	[SerializeField]
	private LocalizedString _waitingForSteam;

	[SerializeField]
	private LocalizedString _connectingToSteam;

	[SerializeField]
	private LocalizedString _reconnectingToSteam;

	[SerializeField]
	private LocalizedString _connectionLost;

	[SerializeField]
	private LocalizedString _connectionFailed;

	[SerializeField]
	private LocalizedString _steamRelayUnavailable;

	[SerializeField]
	private LocalizedString _checkingSteamRelay;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		_versionText.text = Application.version ?? "";
	}

	private void Start()
	{
		_mainMenuCanvas.gameObject.SetActive(value: true);
		_devStuff.SetActive(SteamManager.IsDev);
		_discordButton.SetActive(!SteamManager.IsDev);

#if UNITY_WEBGL && !UNITY_EDITOR
		// WebGL 单人版：整行 Steam Relay 状态都不该出现。
		//
		// 截图里那行红字「Disconnected: Version mismatch」来自
		// CanvasManager.OnRelayStatus 的 k_ESteamNetworkingAvailability_Previously
		// 分支，文案取自 Localization/Locales/Extra Labels_en.asset。
		// 但 Steam 在 WebGL 下压根没启用（SteamManager 是空实现，
		// ButtonManager._useSteam 硬编码 false），这个状态没有任何意义 ——
		// 它不是"版本真的不匹配"，是WebGL 桩的残留状态被渲染了出来。
		//
		// 为什么要在意：它出现在每张 CI 截图的**正中间最显眼处**，
		// 一眼看上去像"游戏连不上服务器"，会让人把排查方向带到联机上去。
		// 事实上单玩家按钮就在下面，只是被这行字盖住注意力。
		//
		// 只隐藏 UI，不动任何逻辑 —— 编辑器与桌面版行为保持原样。
		if (_steamRelayText != null)
		{
			_steamRelayText.gameObject.SetActive(value: false);
		}
		SteamManager.RefreshRelayStatus();
#else
		SteamManager.RefreshRelayStatus();
#endif
	}

	public static void OnRelayStatus(SteamRelayNetworkStatus_t status)
	{
		if ((bool)_instance)
		{
			LeanTween.cancel(_instance._steamRelayText.gameObject);
			_instance._steamRelayText.color = Color.white;
			TextMeshProUGUI steamRelayText = _instance._steamRelayText;
			string localizedString;
			switch (status.m_eAvail)
			{
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current:
				localizedString = _instance._steamRelayReady.GetLocalizedString();
				break;
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_NeverTried:
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Waiting:
				localizedString = _instance._waitingForSteam.GetLocalizedString();
				break;
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Attempting:
				localizedString = _instance._connectingToSteam.GetLocalizedString();
				break;
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Retrying:
				localizedString = _instance._reconnectingToSteam.GetLocalizedString();
				break;
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Previously:
				localizedString = _instance._connectionLost.GetLocalizedString();
				break;
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Failed:
				localizedString = _instance._connectionFailed.GetLocalizedString();
				break;
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_CannotTry:
				localizedString = _instance._steamRelayUnavailable.GetLocalizedString();
				break;
			default:
				localizedString = _instance._checkingSteamRelay.GetLocalizedString();
				break;
			}
			steamRelayText.text = localizedString;
			switch (status.m_eAvail)
			{
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current:
				_instance._steamRelayText.color = GameInfo.GreenColor;
				LeanTween.cancel(_instance._steamRelayText.gameObject);
				LeanTween.value(_instance._steamRelayText.gameObject, 1f, 0f, 3f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate(_instance.UpdateRelayTextAlpha);
				break;
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Failed:
			case ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Previously:
				_instance._steamRelayText.color = GameInfo.RedColor;
				break;
			}
		}
	}

	private void UpdateRelayTextAlpha(float to)
	{
		Color color = _instance._steamRelayText.color;
		color.a = to;
		_instance._steamRelayText.color = color;
	}

	public static void ToggleBlackscreen(bool to, bool instant = false, float duration = 3f)
	{
		if ((bool)_instance)
		{
			LeanTween.cancel(_instance._blackScreen.gameObject);
			if (instant)
			{
				_instance._blackScreen.color = (to ? new Color(0f, 0f, 0f, 1f) : new Color(0f, 0f, 0f, 0f));
				return;
			}
			float num = ((!to) ? 1 : 0);
			float to2 = (to ? 1 : 0);
			LeanTween.value(_instance._blackScreen.gameObject, num, to2, duration).setEase(LeanTweenType.easeInOutSine).setOnUpdate(_instance.SetBlackscreenAlpha);
		}
	}

	private void SetBlackscreenAlpha(float alpha)
	{
		_blackScreen.color = new Color(0f, 0f, 0f, alpha);
	}

	public static void ToggleMenuUI(bool enabled, bool instant = false)
	{
		if (!_instance)
		{
			return;
		}
		_instance._mainMenuCanvas.gameObject.SetActive(value: true);
		LeanTween.cancel(_instance._mainMenuCanvas.gameObject);
		if (enabled)
		{
			_instance._howToFishTitle.SetActive(value: true);
		}
		if (instant)
		{
			_instance._mainMenuCanvas.gameObject.SetActive(enabled);
			_instance._mainMenuCanvas.alpha = (enabled ? 1 : 0);
		}
		else
		{
			LeanTween.value(_instance._mainMenuCanvas.gameObject, _instance._mainMenuCanvas.alpha, enabled ? 1 : 0, 1f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(_instance.UpdateMenuAlpha)
				.setOnComplete(_instance.HideMenuCanvas);
		}
		foreach (GameObject allMainMenuButton in _instance._allMainMenuButtons)
		{
			allMainMenuButton.SetActive(value: false);
		}
		_instance._allMainMenuButtons[0].SetActive(enabled);
		LocalSkin.DisablePreview();
		if (enabled)
		{
			UpdateCharacterNotification();
		}
	}

	public static void UpdateCharacterNotification()
	{
		_instance._unseenCharacterNotification.SetActive(SkinManager.HasUnseenClothing());
	}

	private void UpdateMenuAlpha(float alpha)
	{
		_mainMenuCanvas.alpha = alpha;
	}

	private void HideMenuCanvas()
	{
		if (!MainMenuManager.IsInMenu)
		{
			_mainMenuCanvas.gameObject.SetActive(value: false);
		}
	}

	public static void ToggleUnderwaterImage(bool to)
	{
		if ((bool)_instance)
		{
			_instance._underwaterImage.gameObject.SetActive(to);
		}
	}

	public static void TogglePermaCanvas(bool to)
	{
		if ((bool)_instance)
		{
			_instance._permaCanvas.alpha = (to ? 1 : 0);
		}
	}
}
