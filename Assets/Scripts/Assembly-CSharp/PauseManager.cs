using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
	private static PauseManager _instance;

	[SerializeField]
	private GameObject _pauseHolder;

	[SerializeField]
	private GameObject[] _disabledScreensOnPause;

	[SerializeField]
	private GameObject _mainScreen;

	[SerializeField]
	private GameObject _optionsScreen;

	[SerializeField]
	private GameObject _serverSettingsScreen;

	[SerializeField]
	private GameObject _copyLobbyIDButton;

	[SerializeField]
	private GameObject _inviteFriendButton;

	[SerializeField]
	private List<GameObject> _serverButtons;

	[SerializeField]
	private GameObject _mainMenuButton;

	[SerializeField]
	private GameObject _uiDisabledWarning;

	public static bool IsPaused { get; private set; }

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		Server.OnServerStarted += OnSeverChange;
		Server.OnServerStopped += OnSeverChange;
	}

	private void Start()
	{
		BindInputs();
		TogglePause(to: false);
	}

	private void OnDestroy()
	{
		Server.OnServerStarted -= OnSeverChange;
		Server.OnServerStopped -= OnSeverChange;
		UnbindInputs();
	}

	private void OnSeverChange()
	{
		if (IsPaused)
		{
			OnPause();
		}
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["Pause"].performed += PauseInput;
		}
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["Pause"].performed -= PauseInput;
		}
	}

	private void PauseInput(InputAction.CallbackContext context)
	{
		if (!PlayerThinking.IsThinking)
		{
			TogglePause();
		}
	}

	private static void TogglePause()
	{
		TogglePause(!IsPaused);
	}

	public static void TogglePause(bool to)
	{
		IsPaused = to;
		PlayerCamera.ToggleMouse(IsPaused || MainMenuManager.IsInMenu);
		ChatManager.OnPause();
		OnPause();
		ShaderManager.OnPause();
		MainMenuManager.OnPause();
		PlayerUI.OnPause();
	}

	private static void OnPause()
	{
		if (!_instance)
		{
			return;
		}
		if (!IsPaused)
		{
			PlayerPrefs.Save();
		}
		if ((bool)Server.Instance && (bool)Player.LocalPlayer)
		{
			Server.Instance.SetIsAfk(Player.LocalPlayer, IsPaused);
		}
		GameObject[] disabledScreensOnPause = _instance._disabledScreensOnPause;
		for (int i = 0; i < disabledScreensOnPause.Length; i++)
		{
			disabledScreensOnPause[i].SetActive(value: false);
		}
		_instance._pauseHolder.SetActive(IsPaused);
		_instance._mainScreen.SetActive(!MainMenuManager.IsInMenu);
		_instance._optionsScreen.SetActive(MainMenuManager.IsInMenu);
		_instance._copyLobbyIDButton.SetActive(SteamManager.CurrentLobbyID != CSteamID.Nil && SaveManager.CurServerSave != null && SaveManager.CurServerSave.IsPublic);
		_instance._inviteFriendButton.SetActive(SteamManager.CurrentLobbyID != CSteamID.Nil);
		_instance._serverSettingsScreen.SetActive(value: false);
		_instance._uiDisabledWarning.SetActive(PlayerUI.UIDisabled);
		_instance._mainMenuButton.SetActive(Server.Instance);
		foreach (GameObject serverButton in _instance._serverButtons)
		{
			serverButton.SetActive((bool)Server.Instance && Server.Instance.IsServerInitialized);
		}
	}

	public static void MainPauseScreenOrClose()
	{
		if ((bool)_instance)
		{
			_instance._optionsScreen.SetActive(value: false);
			if (MainMenuManager.IsInMenu)
			{
				TogglePause(to: false);
			}
			else
			{
				_instance._mainScreen.SetActive(value: true);
			}
		}
	}
}
