using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
	private static ButtonManager _instance;

	[Header("Bean Guy")]
	[SerializeField]
	private Toggle _beanToggle;

	[SerializeField]
	private GameObject _beanRow;

	[Header("Create Game")]
	[SerializeField]
	private TMP_InputField _serverNameInputField;

	[FormerlySerializedAs("_createMutliplayerText")]
	[FormerlySerializedAs("_useSteamText")]
	[SerializeField]
	private GameObject _createInviteOnlyText;

	[SerializeField]
	private GameObject _createPublicText;

	[SerializeField]
	private GameObject _createSingleplayerText;

	[SerializeField]
	private GameObject _createEasyDifficultyText;

	[SerializeField]
	private GameObject _createDefaultDifficultyText;

	[SerializeField]
	private GameObject _createHardDifficultyText;

	[SerializeField]
	private Button _createServerButton;

	[SerializeField]
	private Image _createServerOutline;

	[Header("Load Game")]
	[SerializeField]
	private GameObject _selectedSaveInfoHolder;

	[SerializeField]
	private List<GameObject> _savedServerButtons;

	[SerializeField]
	private List<TextMeshProUGUI> _savedServerTexts;

	[SerializeField]
	private TextMeshProUGUI _selectedServerText;

	[SerializeField]
	private TextMeshProUGUI _lastPlayedText;

	[FormerlySerializedAs("_multiplayerText")]
	[FormerlySerializedAs("_sessionTypeText")]
	[SerializeField]
	private GameObject _inviteOnlyText;

	[SerializeField]
	private GameObject _publicText;

	[SerializeField]
	private GameObject _singleplayerText;

	[SerializeField]
	private TextMeshProUGUI _playtimeText;

	[SerializeField]
	private TextMeshProUGUI _islandText;

	[Header("Join Game")]
	[SerializeField]
	private TMP_InputField _lobbyIdInputField;

	[SerializeField]
	private Button _joinByIDButton;

	[Header("Gameplay Settings")]
	[Space]
	[SerializeField]
	private Slider _sensitivitySlider;

	[SerializeField]
	private Toggle _viewBobbingToggle;

	[SerializeField]
	private Toggle _invertXToggle;

	[SerializeField]
	private Toggle _invertYToggle;

	[SerializeField]
	private TMP_InputField _sensitivityInputField;

	[FormerlySerializedAs("_volumeSlider")]
	[Header("Audio Settings")]
	[Space]
	[SerializeField]
	private Slider _masterVolSlider;

	[FormerlySerializedAs("_volumeInputField")]
	[SerializeField]
	private TMP_InputField _masterVolInputField;

	[SerializeField]
	private Slider _musicVolSlider;

	[SerializeField]
	private TMP_InputField _musicVolInputField;

	[SerializeField]
	private Slider _fxVolSlider;

	[SerializeField]
	private TMP_InputField _fxVolInputField;

	[SerializeField]
	private Slider _proxyVolSlider;

	[SerializeField]
	private TMP_InputField _proxyVolInputField;

	[SerializeField]
	private Slider _microphoneGainSlider;

	[SerializeField]
	private TMP_InputField _microphoneGainInputField;

	[Space]
	[SerializeField]
	private GameObject _offMicInputType;

	[SerializeField]
	private GameObject _pushToTalkMicInputType;

	[SerializeField]
	private GameObject _alwaysMicInputType;

	[Space]
	[SerializeField]
	private TextMeshProUGUI _microphoneText;

	[Header("Graphics Settings")]
	[SerializeField]
	private Toggle _itemDotsToggle;

	[SerializeField]
	private TextMeshProUGUI _curResolutionText;

	[SerializeField]
	private GameObject _applyResolutionButton;

	[SerializeField]
	private Slider _fovSlider;

	[SerializeField]
	private TMP_InputField _fovInputField;

	[Space]
	[SerializeField]
	private Toggle _vsyncToggle;

	[Space]
	[SerializeField]
	private Slider _maxFpsSlider;

	[SerializeField]
	private TMP_InputField _maxFpsInputField;

	[Space]
	[SerializeField]
	private GameObject _shadowsOffText;

	[SerializeField]
	private GameObject _shadowsLowText;

	[SerializeField]
	private GameObject _shadowsDefaultText;

	[SerializeField]
	private GameObject _borderlessModeText;

	[SerializeField]
	private GameObject _windowedModeText;

	[SerializeField]
	private GameObject _fullscreenModeText;

	[Space]
	[SerializeField]
	private Toggle _ambientToggle;

	[Space]
	[SerializeField]
	private Toggle _decorationsToggle;

	[Space]
	[SerializeField]
	private Toggle _decalsToggle;

	[Space]
	[SerializeField]
	private Toggle _bloodToggle;

	[Space]
	[SerializeField]
	private Toggle _damageNumbersToggle;

	[Header("Server Settings")]
	[SerializeField]
	private Toggle _friendlyFireToggle;

	[SerializeField]
	private TextMeshProUGUI _cheatText;

	[SerializeField]
	private GameObject _serverInviteOnlyText;

	[SerializeField]
	private GameObject _serverPublicText;

	[SerializeField]
	private GameObject _serverSingleplayerText;

	[SerializeField]
	private GameObject _requiresRestartText;

	[SerializeField]
	private GameObject _serverEasyDifficultyText;

	[SerializeField]
	private GameObject _serverDefaultDifficultyText;

	[SerializeField]
	private GameObject _serverHardDifficultyText;

	private int _shadowQuality;

	private int _audioInputType;

	private int _microphoneIndex;

	private int _fullscreenMode;

	private int _displayedResolutionIndex = -1;

	private int _activeResolutionIndex = -1;

	private bool _useSteam = true;

	private Difficulty _selectedDifficulty = Difficulty.Default;

	private bool _isPublic;

	private Resolution[] _screenResolutions = Array.Empty<Resolution>();

	private ScreenSpaceAmbientOcclusion _ssao;

	private byte _cheatButtonPressedAmount;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
	}

	private void Start()
	{
		SetupUI();
	}

	private void SetupUI()
	{
		_sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 0.5f);
		_sensitivityInputField.text = _sensitivitySlider.value.ToString("F2", CultureInfo.CurrentCulture);
		PlayerCamera.SetSensitivity(_sensitivitySlider.value);
		float value = PlayerPrefs.GetFloat("Master", 0.5f);
		_masterVolSlider.value = value;
		_masterVolInputField.text = value.ToString("F2", CultureInfo.CurrentCulture);
		AudioManager.SetVolume(_masterVolSlider.value, VolumeType.Master);
		float value2 = PlayerPrefs.GetFloat("Music", 0.2f);
		_musicVolSlider.value = value2;
		_musicVolInputField.text = value2.ToString("F2", CultureInfo.CurrentCulture);
		AudioManager.SetVolume(_musicVolSlider.value, VolumeType.Music);
		float value3 = PlayerPrefs.GetFloat("FX", 0.5f);
		_fxVolSlider.value = value3;
		_fxVolInputField.text = value3.ToString("F2", CultureInfo.CurrentCulture);
		AudioManager.SetVolume(_fxVolSlider.value, VolumeType.FX);
		float value4 = PlayerPrefs.GetFloat("Proxy", 0.5f);
		_proxyVolSlider.value = value4;
		_proxyVolInputField.text = value4.ToString("F2", CultureInfo.CurrentCulture);
		AudioManager.SetVolume(_proxyVolSlider.value, VolumeType.Proxy);
		float num = PlayerPrefs.GetFloat("Gain", 1f);
		_microphoneGainSlider.value = num;
		_microphoneGainInputField.text = num.ToString("F2", CultureInfo.CurrentCulture);
		AudioManager.SetVolume(num, VolumeType.Gain);
		_audioInputType = PlayerPrefs.GetInt("AudioInputType", 2);
		ChangeAudioInputType(0);
		string text = PlayerPrefs.GetString("Microphone", "");
		if (Microphone.devices.Length != 0)
		{
			if (text == "")
			{
				text = Microphone.devices[0];
			}
			else if (Microphone.devices.Contains(text))
			{
				_microphoneIndex = Microphone.devices.ToList().IndexOf(text);
			}
			ChangeMicrophone(0);
		}
		_invertXToggle.isOn = PlayerPrefs.GetInt("InvertX", 0) == 1;
		_invertYToggle.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;
		PlayerCamera.SetInvertX(_invertXToggle.isOn);
		PlayerCamera.SetInvertY(_invertYToggle.isOn);
		_viewBobbingToggle.isOn = PlayerPrefs.GetInt("ViewBobbing", 1) == 1;
		_vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 0) == 1;
		QualitySettings.vSyncCount = (_vsyncToggle.isOn ? 1 : 0);
		int num2 = PlayerPrefs.GetInt("MaxFPS", (int)Screen.currentResolution.refreshRateRatio.value);
		_maxFpsSlider.value = num2;
		_maxFpsInputField.text = num2.ToString();
		Application.targetFrameRate = num2;
		int num3 = PlayerPrefs.GetInt("FOV", 74);
		_fovSlider.value = num3;
		_fovInputField.text = num3.ToString(CultureInfo.CurrentCulture);
		PlayerCamera.SetFOV(num3);
		foreach (ScriptableRendererFeature rendererFeature in GameInfo.UrpAsset.rendererDataList[0].rendererFeatures)
		{
			if (rendererFeature is ScreenSpaceAmbientOcclusion ssao)
			{
				_ssao = ssao;
				break;
			}
		}
		_ambientToggle.isOn = PlayerPrefs.GetInt("AmbientOcclusion", 1) == 1;
		_ssao.SetActive(_ambientToggle.isOn);
		_decorationsToggle.isOn = PlayerPrefs.GetInt("Decorations", 1) == 1;
		DecorationManager.ToggleLevelDecorations(_decorationsToggle.isOn);
		_decalsToggle.isOn = PlayerPrefs.GetInt("Decals", 1) == 1;
		DecalManager.ToggleDecals(_decalsToggle.isOn);
		_bloodToggle.isOn = PlayerPrefs.GetInt("Blood", 1) == 1;
		DecalManager.ToggleBlood(_bloodToggle.isOn);
		_damageNumbersToggle.isOn = PlayerPrefs.GetInt("DamageNumbers", 1) == 1;
		DecalManager.ToggleDamageNumbers(_damageNumbersToggle.isOn);
		_itemDotsToggle.isOn = PlayerPrefs.GetInt("ItemDots", 1) == 1;
		CloseItemsUI.ToggleItemDots(_itemDotsToggle.isOn);
		_shadowQuality = PlayerPrefs.GetInt("ShadowQuality", 2);
		ChangeShadowQuality(0);
		switch (Screen.fullScreenMode)
		{
		case FullScreenMode.FullScreenWindow:
			ChangeFullscreenMode(0);
			break;
		case FullScreenMode.Windowed:
			ChangeFullscreenMode(1);
			break;
		case FullScreenMode.ExclusiveFullScreen:
			ChangeFullscreenMode(2);
			break;
		default:
			ChangeFullscreenMode(0);
			break;
		}
		SetupResolution();
		_instance._beanRow.SetActive(SkinManager.HasBean);
		ToggleCreateServerButton(to: false);
		_selectedSaveInfoHolder.SetActive(value: false);
	}

	public void ItemDotsToggle()
	{
		PlayerPrefs.SetInt("ItemDots", _itemDotsToggle.isOn ? 1 : 0);
		CloseItemsUI.ToggleItemDots(_itemDotsToggle.isOn);
	}

	public void ChangeResolution(bool next)
	{
		if (_screenResolutions.Length != 0)
		{
			_displayedResolutionIndex += (next ? 1 : (-1));
			if (_displayedResolutionIndex < 0)
			{
				_displayedResolutionIndex = _screenResolutions.Length - 1;
			}
			else if (_displayedResolutionIndex >= _screenResolutions.Length)
			{
				_displayedResolutionIndex = 0;
			}
			UpdateResolutionUI();
		}
	}

	public void ApplyResolution()
	{
		if (_displayedResolutionIndex >= 0 && _displayedResolutionIndex < _screenResolutions.Length)
		{
			Resolution resolution = _screenResolutions[_displayedResolutionIndex];
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
			_activeResolutionIndex = _displayedResolutionIndex;
			_applyResolutionButton.SetActive(value: false);
		}
	}

	private void SetupResolution()
	{
		_screenResolutions = (from resolution2 in Screen.resolutions
			group resolution2 by (width: resolution2.width, height: resolution2.height) into @group
			select @group.First()).ToArray();
		if (_screenResolutions.Length == 0)
		{
			_curResolutionText.text = $"{Screen.width} x {Screen.height}";
			_applyResolutionButton.SetActive(value: false);
			return;
		}
		for (int num = 0; num < _screenResolutions.Length; num++)
		{
			Resolution resolution = _screenResolutions[num];
			if (resolution.width == Screen.width && resolution.height == Screen.height)
			{
				_activeResolutionIndex = num;
				break;
			}
		}
		if (_activeResolutionIndex < 0)
		{
			_activeResolutionIndex = 0;
		}
		_displayedResolutionIndex = _activeResolutionIndex;
		UpdateResolutionUI();
	}

	private void UpdateResolutionUI()
	{
		Resolution resolution = _screenResolutions[_displayedResolutionIndex];
		_curResolutionText.text = $"{resolution.width} x {resolution.height}";
		_applyResolutionButton.SetActive(_displayedResolutionIndex != _activeResolutionIndex);
	}

	public static void LoadBeanFromSave(bool bean)
	{
		_instance._beanToggle.isOn = bean;
	}

	public static void ShowBeanRow()
	{
		if ((bool)_instance)
		{
			_instance._beanRow.SetActive(value: true);
		}
	}

	public void ChangeLanguageButton(bool prev)
	{
		LocalizationManager.ChangeLanguage(prev);
	}

	public void SaveServerButton()
	{
		SaveManager.SaveServer();
	}

	public void DeleteServerButton()
	{
		SaveManager.DeleteServer();
		SaveManager.LoadAllServers();
	}

	private void ToggleCreateServerButton(bool to)
	{
		_createServerButton.interactable = to;
		_createServerOutline.color = (to ? Color.white : Color.gray);
	}

	public void OnServerNameChange()
	{
		if (string.IsNullOrEmpty(_serverNameInputField.text) || _serverNameInputField.text.ToLower() == "local")
		{
			ToggleCreateServerButton(to: false);
			return;
		}
		bool flag = false;
		foreach (SaveManager.ServerSaveObject serverSafe in SaveManager.ServerSaves)
		{
			if (serverSafe.Name == _serverNameInputField.text)
			{
				flag = true;
				ToggleCreateServerButton(to: false);
			}
		}
		if (!flag)
		{
			ToggleCreateServerButton(to: true);
		}
	}

	public void CreateNewServer()
	{
		_requiresRestartText.SetActive(value: false);
		ToggleCreateServerButton(to: false);
		SaveManager.OnServerLoaded();
		SaveManager.CreateServer(_serverNameInputField.text, _useSteam, _isPublic, _selectedDifficulty);
		if (SaveManager.CurServerSave.UseSteam)
		{
			CreateOnlineLobbyButton();
		}
		else
		{
			CreateLocalLobbyButton();
		}
		_serverNameInputField.text = string.Empty;
	}

	public void LoadServerButton()
	{
		_requiresRestartText.SetActive(value: false);
		if (SaveManager.CurServerSave.UseSteam)
		{
			CreateOnlineLobbyButton();
		}
		else
		{
			CreateLocalLobbyButton();
		}
		SaveManager.OnServerLoaded();
	}

	public static void UpdateSavedServerButtons()
	{
		if (!_instance)
		{
			return;
		}
		List<SaveManager.ServerSaveObject> serverSaves = SaveManager.ServerSaves;
		serverSaves = serverSaves.OrderByDescending((SaveManager.ServerSaveObject x) => x.LastSave).ToList();
		for (int num = 0; num < _instance._savedServerButtons.Count; num++)
		{
			if (serverSaves.Count <= num)
			{
				_instance._savedServerButtons[num].SetActive(value: false);
				continue;
			}
			_instance._savedServerButtons[num].SetActive(value: true);
			_instance._savedServerTexts[num].text = serverSaves[num].Name;
		}
		_instance._selectedSaveInfoHolder.SetActive(value: false);
	}

	public void SelectServer(TextMeshProUGUI text)
	{
		SaveManager.SelectServer(text.text);
		if (SaveManager.CurServerSave != null)
		{
			_selectedSaveInfoHolder.SetActive(value: true);
			_selectedServerText.text = SaveManager.CurServerSave.Name ?? "";
			DateTime dateTime = new DateTime(SaveManager.CurServerSave.LastSave, DateTimeKind.Local);
			string text2 = dateTime.ToString("M", LocalizationSettings.SelectedLocale.Formatter) + " - " + dateTime.ToString("HH:mm", LocalizationSettings.SelectedLocale.Formatter);
			_lastPlayedText.text = text2;
			_inviteOnlyText.SetActive(SaveManager.CurServerSave.UseSteam && !SaveManager.CurServerSave.IsPublic);
			_publicText.SetActive(SaveManager.CurServerSave.UseSteam && SaveManager.CurServerSave.IsPublic);
			_singleplayerText.SetActive(!SaveManager.CurServerSave.UseSteam);
			TimeSpan timeSpan = TimeSpan.FromSeconds(SaveManager.CurServerSave.Playtime);
			string text3 = (LocalizationManager.IsAsianLanguage ? "" : " ");
			_playtimeText.text = $"{timeSpan.Hours}{LocalizationManager.HoursLocalized.GetLocalizedString()}{text3}{timeSpan.Minutes:D2}{LocalizationManager.MinutesLocalized.GetLocalizedString()}{text3}{timeSpan.Seconds:D2}{LocalizationManager.SecondsLocalized.GetLocalizedString()}";
			_islandText.text = $"{SaveManager.CurServerSave.SpawnedIsland + 1}";
			_serverInviteOnlyText.SetActive(SaveManager.CurServerSave.UseSteam && !SaveManager.CurServerSave.IsPublic);
			_serverPublicText.SetActive(SaveManager.CurServerSave.UseSteam && SaveManager.CurServerSave.IsPublic);
			_serverSingleplayerText.SetActive(!SaveManager.CurServerSave.UseSteam);
			_serverEasyDifficultyText.SetActive(SaveManager.CurServerSave.Difficulty == Difficulty.Easy);
			_serverDefaultDifficultyText.SetActive(SaveManager.CurServerSave.Difficulty == Difficulty.Default);
			_serverHardDifficultyText.SetActive(SaveManager.CurServerSave.Difficulty == Difficulty.Hard);
		}
		else
		{
			_selectedSaveInfoHolder.SetActive(value: false);
		}
	}

	public void OnSensitivitySliderChange()
	{
		PlayerPrefs.SetFloat("Sensitivity", _sensitivitySlider.value);
		PlayerCamera.SetSensitivity(_sensitivitySlider.value);
		_sensitivityInputField.text = _sensitivitySlider.value.ToString("F2", CultureInfo.CurrentCulture);
	}

	public void OnSensitivityInputFieldChange()
	{
		if (float.TryParse(_sensitivityInputField.text, out var result))
		{
			result = Mathf.Clamp(result, 0.01f, 10f);
			PlayerPrefs.SetFloat("Sensitivity", result);
			PlayerCamera.SetSensitivity(result);
			_sensitivitySlider.value = result;
		}
		_sensitivityInputField.text = _sensitivitySlider.value.ToString("F2", CultureInfo.CurrentCulture);
	}

	public void InviteFriendButton()
	{
		if (SteamManager.CurrentLobbyID != CSteamID.Nil)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(SteamManager.CurrentLobbyID);
		}
	}

	public void OnMasterSliderChange()
	{
		VolumeSliderChange(VolumeType.Master);
	}

	public void OnMusicSliderChange()
	{
		VolumeSliderChange(VolumeType.Music);
	}

	public void OnFXSliderChange()
	{
		VolumeSliderChange(VolumeType.FX);
	}

	public void OnProxySliderChange()
	{
		VolumeSliderChange(VolumeType.Proxy);
	}

	public void OnGainSliderChange()
	{
		VolumeSliderChange(VolumeType.Gain);
	}

	private void VolumeSliderChange(VolumeType type)
	{
		Slider slider = _masterVolSlider;
		TMP_InputField tMP_InputField = _masterVolInputField;
		switch (type)
		{
		case VolumeType.Music:
			slider = _musicVolSlider;
			tMP_InputField = _musicVolInputField;
			break;
		case VolumeType.FX:
			slider = _fxVolSlider;
			tMP_InputField = _fxVolInputField;
			break;
		case VolumeType.Proxy:
			slider = _proxyVolSlider;
			tMP_InputField = _proxyVolInputField;
			break;
		case VolumeType.Gain:
			slider = _microphoneGainSlider;
			tMP_InputField = _microphoneGainInputField;
			break;
		}
		PlayerPrefs.SetFloat(type.ToString(), slider.value);
		AudioManager.SetVolume(slider.value, type);
		tMP_InputField.text = slider.value.ToString("F2", CultureInfo.CurrentCulture);
	}

	public void OnMasterInputFieldChange()
	{
		VolumeInputFieldChange(VolumeType.Master);
	}

	public void OnMusicInputFieldChange()
	{
		VolumeInputFieldChange(VolumeType.Music);
	}

	public void OnFXInputFieldChange()
	{
		VolumeInputFieldChange(VolumeType.FX);
	}

	public void OnProxyInputFieldChange()
	{
		VolumeInputFieldChange(VolumeType.Proxy);
	}

	public void OnGainInputFieldChange()
	{
		VolumeInputFieldChange(VolumeType.Gain);
	}

	private void VolumeInputFieldChange(VolumeType type)
	{
		Slider slider = _masterVolSlider;
		TMP_InputField tMP_InputField = _masterVolInputField;
		switch (type)
		{
		case VolumeType.Music:
			slider = _musicVolSlider;
			tMP_InputField = _musicVolInputField;
			break;
		case VolumeType.FX:
			slider = _fxVolSlider;
			tMP_InputField = _fxVolInputField;
			break;
		case VolumeType.Proxy:
			slider = _proxyVolSlider;
			tMP_InputField = _proxyVolInputField;
			break;
		case VolumeType.Gain:
			slider = _microphoneGainSlider;
			tMP_InputField = _microphoneGainInputField;
			break;
		}
		if (float.TryParse(tMP_InputField.text, out var result))
		{
			result = Mathf.Clamp(result, slider.minValue, slider.maxValue);
			PlayerPrefs.SetFloat(type.ToString(), result);
			AudioManager.SetVolume(result, type);
			slider.value = result;
		}
		tMP_InputField.text = slider.value.ToString("F2", CultureInfo.CurrentCulture);
	}

	public void CreateOnlineLobbyButton()
	{
		SteamManager.CreateLobby();
	}

	public void CreateLocalLobbyButton()
	{
		ConnectionManager.Instance.CreateOfflineLobby();
	}

	public void JoinLocalLobbyButton()
	{
		ConnectionManager.Instance.JoinOfflineLobby();
	}

	public void ChangeMultiplayerMode(bool toNext)
	{
		if (toNext)
		{
			if (_useSteam && !_isPublic)
			{
				_isPublic = true;
			}
			else if (_useSteam && _isPublic)
			{
				_useSteam = false;
				_isPublic = false;
			}
			else
			{
				_useSteam = true;
				_isPublic = false;
			}
		}
		else if (_useSteam && !_isPublic)
		{
			_useSteam = false;
		}
		else if (_useSteam && _isPublic)
		{
			_isPublic = false;
		}
		else
		{
			_useSteam = true;
			_isPublic = true;
		}
		_createInviteOnlyText.SetActive(_useSteam && !_isPublic);
		_createPublicText.SetActive(_useSteam && _isPublic);
		_createSingleplayerText.SetActive(!_useSteam);
		_serverInviteOnlyText.SetActive(_useSteam && !_isPublic);
		_serverPublicText.SetActive(_useSteam && _isPublic);
		_serverSingleplayerText.SetActive(!_useSteam);
	}

	public void ChangeDifficultyButton(bool toNext)
	{
		int selectedDifficulty = (int)_selectedDifficulty;
		selectedDifficulty += (toNext ? 1 : (-1));
		int num = Enum.GetNames(typeof(Difficulty)).Length;
		if (selectedDifficulty >= num)
		{
			selectedDifficulty = 0;
		}
		if (selectedDifficulty < 0)
		{
			selectedDifficulty = num - 1;
		}
		_selectedDifficulty = (Difficulty)selectedDifficulty;
		_createEasyDifficultyText.SetActive(_selectedDifficulty == Difficulty.Easy);
		_createDefaultDifficultyText.SetActive(_selectedDifficulty == Difficulty.Default);
		_createHardDifficultyText.SetActive(_selectedDifficulty == Difficulty.Hard);
		_serverEasyDifficultyText.SetActive(_selectedDifficulty == Difficulty.Easy);
		_serverDefaultDifficultyText.SetActive(_selectedDifficulty == Difficulty.Default);
		_serverHardDifficultyText.SetActive(_selectedDifficulty == Difficulty.Hard);
	}

	public void ChangeCurServerMultiplayerMode(bool toNext)
	{
		if (toNext)
		{
			if (_useSteam && !_isPublic)
			{
				_isPublic = true;
			}
			else if (_useSteam && _isPublic)
			{
				_useSteam = false;
				_isPublic = false;
			}
			else
			{
				_useSteam = true;
				_isPublic = false;
			}
		}
		else if (_useSteam && !_isPublic)
		{
			_useSteam = false;
		}
		else if (_useSteam && _isPublic)
		{
			_isPublic = false;
		}
		else
		{
			_useSteam = true;
			_isPublic = true;
		}
		_serverInviteOnlyText.SetActive(_useSteam && !_isPublic);
		_serverPublicText.SetActive(_useSteam && _isPublic);
		_serverSingleplayerText.SetActive(!_useSteam);
		if (SaveManager.CurServerSave != null)
		{
			_requiresRestartText.SetActive(value: true);
			SaveManager.CurServerSave.UseSteam = _useSteam;
			SaveManager.CurServerSave.IsPublic = _isPublic;
		}
	}

	public void ChangeCurServerDifficulty(bool toNext)
	{
		int selectedDifficulty = (int)_selectedDifficulty;
		selectedDifficulty += (toNext ? 1 : (-1));
		int num = Enum.GetNames(typeof(Difficulty)).Length;
		if (selectedDifficulty >= num)
		{
			selectedDifficulty = 0;
		}
		if (selectedDifficulty < 0)
		{
			selectedDifficulty = num - 1;
		}
		_selectedDifficulty = (Difficulty)selectedDifficulty;
		_serverEasyDifficultyText.SetActive(_selectedDifficulty == Difficulty.Easy);
		_serverDefaultDifficultyText.SetActive(_selectedDifficulty == Difficulty.Default);
		_serverHardDifficultyText.SetActive(_selectedDifficulty == Difficulty.Hard);
		if (SaveManager.CurServerSave != null)
		{
			SaveManager.CurServerSave.Difficulty = _selectedDifficulty;
			if ((bool)ServerSettings.Instance && ServerSettings.Instance.IsServerInitialized)
			{
				ServerSettings.Instance.SetDifficulty(_selectedDifficulty);
			}
		}
	}

	public void OnLobbyIDInputFieldChange()
	{
		if (ulong.TryParse(_lobbyIdInputField.text, out var _))
		{
			_joinByIDButton.enabled = true;
		}
		else
		{
			_joinByIDButton.enabled = false;
		}
	}

	public void JoinByIDButton()
	{
		if (ulong.TryParse(_lobbyIdInputField.text, out var result))
		{
			SteamManager.JoinLobby(result);
		}
		else
		{
			MonoBehaviour.print("ID not parseable");
		}
	}

	public void DeveloperJoinLobbyButton()
	{
		CSteamID steamID = SteamUser.GetSteamID();
		if (steamID.m_SteamID == 76561198105374043L)
		{
			SteamManager.JoinFriend(new CSteamID(76561198058500353uL));
		}
		else if (steamID.m_SteamID == 76561198058500353L)
		{
			SteamManager.JoinFriend(new CSteamID(76561198105374043uL));
		}
	}

	public void BeanToggle()
	{
		LocalSkin.SetIsBean(_beanToggle.isOn);
	}

	public void JoinFriendButton()
	{
		SteamFriends.ActivateGameOverlay("Friends");
	}

	public void CopyLobbyIDButton()
	{
		TextEditor textEditor = new TextEditor();
		textEditor.Paste();
		if (!(textEditor.text == $"{SteamManager.CurrentLobbyID.m_SteamID}"))
		{
			textEditor.text = $"{SteamManager.CurrentLobbyID.m_SteamID}";
			textEditor.SelectAll();
			textEditor.Copy();
		}
	}

	public void BackOrCloseButton()
	{
		PauseManager.MainPauseScreenOrClose();
	}

	public void MainMenuButton()
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			SaveManager.SaveServer(autoSave: true);
		}
		SaveManager.SaveLocal();
		SteamManager.LeaveLobby(DisconnectReason.WithUI);
	}

	public void QuitButton()
	{
		SaveManager.OnQuit();
		Application.Quit();
	}

	public void JoinDiscordButton()
	{
		Application.OpenURL("https://discord.gg/N9bfGzNP4J");
	}

	public void WishlistButton()
	{
		SteamFriends.ActivateGameOverlayToStore(new AppId_t(4001890u), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
	}

	public void ViewBobbingToggle()
	{
		PlayerPrefs.SetInt("ViewBobbing", _viewBobbingToggle.isOn ? 1 : 0);
		PlayerCamera.SetViewBobbing(_viewBobbingToggle.isOn);
	}

	public void InvertLookAxis(bool xAxis)
	{
		if (xAxis)
		{
			PlayerPrefs.SetInt("InvertX", _invertXToggle.isOn ? 1 : 0);
			PlayerCamera.SetInvertX(_invertXToggle.isOn);
		}
		else
		{
			PlayerPrefs.SetInt("InvertY", _invertYToggle.isOn ? 1 : 0);
			PlayerCamera.SetInvertY(_invertYToggle.isOn);
		}
	}

	public void VSyncToggle()
	{
		PlayerPrefs.SetInt("VSync", _vsyncToggle.isOn ? 1 : 0);
		QualitySettings.vSyncCount = (_vsyncToggle.isOn ? 1 : 0);
	}

	public void FOVSliderChange()
	{
		PlayerPrefs.SetInt("FOV", (int)_fovSlider.value);
		_fovInputField.text = _fovSlider.value.ToString(CultureInfo.CurrentCulture);
		PlayerCamera.SetFOV((int)_fovSlider.value);
	}

	public void FOVInputFieldChange()
	{
		if (int.TryParse(_fovInputField.text, out var result))
		{
			result = (int)Mathf.Clamp(result, _fovSlider.minValue, _fovSlider.maxValue);
			PlayerPrefs.SetInt("FOV", result);
			_fovSlider.value = result;
			PlayerCamera.SetFOV(result);
		}
		_fovInputField.text = _fovSlider.value.ToString(CultureInfo.CurrentCulture);
	}

	public void MaxFPSSliderChange()
	{
		PlayerPrefs.SetInt("MaxFPS", (int)_maxFpsSlider.value);
		Application.targetFrameRate = (int)_maxFpsSlider.value;
		_maxFpsInputField.text = _maxFpsSlider.value.ToString(CultureInfo.CurrentCulture);
	}

	public void MaxFPSInputFieldChange()
	{
		if (int.TryParse(_maxFpsInputField.text, out var result))
		{
			result = Mathf.Clamp(result, (int)_maxFpsSlider.minValue, (int)_maxFpsSlider.maxValue);
			PlayerPrefs.SetInt("MaxFPS", result);
			Application.targetFrameRate = result;
			_maxFpsSlider.value = result;
		}
		_maxFpsInputField.text = _maxFpsSlider.value.ToString(CultureInfo.CurrentCulture);
	}

	public void ChangeShadowQuality(int amount)
	{
		_shadowQuality += amount;
		if (_shadowQuality < 0)
		{
			_shadowQuality = 2;
		}
		else if (_shadowQuality > 2)
		{
			_shadowQuality = 0;
		}
		PlayerPrefs.SetInt("ShadowQuality", _shadowQuality);
		_shadowsOffText.SetActive(value: false);
		_shadowsLowText.SetActive(value: false);
		_shadowsDefaultText.SetActive(value: false);
		switch (_shadowQuality)
		{
		case 0:
			_shadowsOffText.SetActive(value: true);
			QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
			GameInfo.MainLight.shadows = LightShadows.None;
			break;
		case 1:
			_shadowsLowText.SetActive(value: true);
			QualitySettings.shadows = UnityEngine.ShadowQuality.All;
			GameInfo.MainLight.shadows = LightShadows.Soft;
			GameInfo.UrpAsset.mainLightShadowmapResolution = 1024;
			GameInfo.UrpAsset.shadowCascadeCount = 1;
			break;
		case 2:
			_shadowsDefaultText.SetActive(value: true);
			GameInfo.UrpAsset.shadowCascadeCount = 4;
			QualitySettings.shadows = UnityEngine.ShadowQuality.All;
			GameInfo.MainLight.shadows = LightShadows.Soft;
			GameInfo.UrpAsset.mainLightShadowmapResolution = 8192;
			break;
		}
	}

	public void ChangeFullscreenMode(int amount)
	{
		_fullscreenMode += amount;
		if (_fullscreenMode < 0)
		{
			_fullscreenMode = 2;
		}
		else if (_fullscreenMode > 2)
		{
			_fullscreenMode = 0;
		}
		_borderlessModeText.gameObject.SetActive(value: false);
		_windowedModeText.SetActive(value: false);
		_fullscreenModeText.SetActive(value: false);
		switch (_fullscreenMode)
		{
		case 0:
			_borderlessModeText.gameObject.SetActive(value: true);
			if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
			{
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
			}
			break;
		case 1:
			_windowedModeText.SetActive(value: true);
			if (Screen.fullScreenMode != FullScreenMode.Windowed)
			{
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.Windowed);
			}
			break;
		case 2:
			_fullscreenModeText.SetActive(value: true);
			if (Screen.fullScreenMode != FullScreenMode.ExclusiveFullScreen)
			{
				Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.ExclusiveFullScreen);
			}
			break;
		}
	}

	public void AmbientOcclusionToggle()
	{
		if ((bool)_ssao)
		{
			PlayerPrefs.SetInt("AmbientOcclusion", _ambientToggle.isOn ? 1 : 0);
			_ssao.SetActive(_ambientToggle.isOn);
		}
	}

	public void FriendlyFireToggle()
	{
		if ((bool)ServerSettings.Instance)
		{
			ServerSettings.Instance.ToggleFriendlyFire(_friendlyFireToggle.isOn);
		}
	}

	public void CheatsButton()
	{
		_cheatButtonPressedAmount++;
		if (SteamManager.IsDev)
		{
			ClientSettings.ToggleCheats(_cheatButtonPressedAmount % 2 == 1);
		}
		_cheatText.gameObject.SetActive(_cheatButtonPressedAmount % 2 == 0);
		_cheatText.color = ((_cheatButtonPressedAmount % 2 == 1) ? new Color(1f, 1f, 1f, 0f) : Color.white);
	}

	public void DecorationsToggle()
	{
		PlayerPrefs.SetInt("Decorations", _decorationsToggle.isOn ? 1 : 0);
		DecorationManager.ToggleLevelDecorations(_decorationsToggle.isOn);
	}

	public void DecalsToggle()
	{
		PlayerPrefs.SetInt("Decals", _decalsToggle.isOn ? 1 : 0);
		DecalManager.ToggleDecals(_decalsToggle.isOn);
	}

	public void BloodToggle()
	{
		PlayerPrefs.SetInt("Blood", _bloodToggle.isOn ? 1 : 0);
		DecalManager.ToggleBlood(_bloodToggle.isOn);
	}

	public void DamageNumbersToggle()
	{
		PlayerPrefs.SetInt("DamageNumbers", _damageNumbersToggle.isOn ? 1 : 0);
		DecalManager.ToggleDamageNumbers(_damageNumbersToggle.isOn);
	}

	public void ChangeAudioInputType(int amount)
	{
		int num = Enum.GetValues(typeof(VoiceInputType)).Length - 1;
		_audioInputType += amount;
		if (_audioInputType < 0)
		{
			_audioInputType = num;
		}
		else if (_audioInputType > num)
		{
			_audioInputType = 0;
		}
		PlayerPrefs.SetInt("AudioInputType", _audioInputType);
		AudioManager.SetAudioInputType((VoiceInputType)_audioInputType);
		_offMicInputType.SetActive(_audioInputType == 0);
		_pushToTalkMicInputType.SetActive(_audioInputType == 1);
		_alwaysMicInputType.SetActive(_audioInputType == 2);
	}

	public void ChangeMicrophone(int amount)
	{
		int num = Microphone.devices.Length - 1;
		_microphoneIndex += amount;
		if (_microphoneIndex < 0)
		{
			_microphoneIndex = num;
		}
		else if (_microphoneIndex > num)
		{
			_microphoneIndex = 0;
		}
		Micro.Value = Microphone.devices[_microphoneIndex];
		_microphoneText.text = Micro.Value;
		PlayerPrefs.SetString("Microphone", Micro.Value);
	}

	public void SetColoringIndex(int index)
	{
		ColorPicker.SetColorIndex(index);
	}

	public void SetSkinColoringPart()
	{
		ColorPicker.SetColoringPart(ColoringPart.Skin);
	}

	public void SetHatColoringPart()
	{
		ColorPicker.SetColoringPart(ColoringPart.Hat);
	}

	public void SetOutfitColoringPart()
	{
		ColorPicker.SetColoringPart(ColoringPart.Outfit);
	}

	public void SetAccessoryColoringPart()
	{
		ColorPicker.SetColoringPart(ColoringPart.Accessory);
	}

	public void RandomizeSelectedSkin()
	{
		ColorPicker.RandomizeSelected();
	}

	public void ResetSelectedSkin()
	{
		ColorPicker.ResetSelected();
	}

	public void RandomizeSkin()
	{
		ColorPicker.RandomizeSkin();
	}

	public void ResetSkin()
	{
		ColorPicker.ResetSkin();
	}

	public void ToggleCharacterCustomization(bool to)
	{
		if (to)
		{
			LocalSkin.EnablePreview();
		}
		else
		{
			LocalSkin.DisablePreview();
		}
	}

	public void ChangeHatIndex(int amount)
	{
		LocalSkin.ChangeMesh(amount, PlayerSkinType.Hat);
		ColorPicker.UpdateSelectedPicker();
	}

	public void ChangeOutfitIndex(int amount)
	{
		LocalSkin.ChangeMesh(amount, PlayerSkinType.Outfit);
		ColorPicker.UpdateSelectedPicker();
	}

	public void ChangeAccessoryIndex(int amount)
	{
		LocalSkin.ChangeMesh(amount, PlayerSkinType.Accessory);
		ColorPicker.UpdateSelectedPicker();
	}
}
