using System.Collections;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	private static MainMenuManager _instance;

	[SerializeField]
	private Camera _menuCam;

	[SerializeField]
	private SelfDrivingBoat _boat;

	[Header("To toggle")]
	[SerializeField]
	private GameObject _menuStuff;

	[Header("Explosion")]
	[SerializeField]
	private string _explosionSound;

	[SerializeField]
	private float _explosionVol = 0.5f;

	public static bool IsInMenu = true;

	private static bool _isShowingCutscene;

	public static Camera MenuCam => _instance._menuCam;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
	}

	private void Start()
	{
		StartCoroutine(DelayedMainMenu());
	}

	private IEnumerator DelayedMainMenu()
	{
		yield return null;
		ToggleMenu(enabled: true);
	}

	public static void ToggleMenu(bool enabled)
	{
		IsInMenu = enabled;
		if ((bool)_instance)
		{
			if (enabled)
			{
				MusicManager.PlayMusic("MainMenu");
				LeanTween.cancel(_instance.gameObject);
				_instance._boat.ToggleBoat(to: true);
				SkipManager.OnSkip -= InstantCrash;
				SaveManager.LoadAllServers();
				PlayerUI.ToggleUIDisabled(to: false);
				CanvasManager.ToggleBlackscreen(to: false, instant: true);
				SteamManager.LeaveLobby(DisconnectReason.None);
				ShaderManager.ToggleSunset(sunset: false);
				ShaderManager.SetFog(FogState.Default);
				CanvasManager.ToggleUnderwaterImage(to: false);
			}
			else
			{
				MusicManager.StopMusic("MainMenu");
			}
			PlayerCamera.ToggleMouse(PauseManager.IsPaused || IsInMenu);
			_instance._menuStuff.gameObject.SetActive(enabled);
			CanvasManager.ToggleMenuUI(enabled && !PauseManager.IsPaused, instant: true);
		}
	}

	public static void OnPause()
	{
		if (IsInMenu)
		{
			CanvasManager.ToggleMenuUI(!PauseManager.IsPaused, instant: true);
		}
	}

	public static void CrashAnimation()
	{
		if ((bool)_instance)
		{
			_isShowingCutscene = true;
			if (ClientSettings.CheatsEnabled)
			{
				InstantCrash();
				return;
			}
			SkipManager.OnSkip += InstantCrash;
			IsInMenu = false;
			PlayerUI.ToggleUIDisabled(to: true);
			CanvasManager.ToggleMenuUI(enabled: false);
			MusicManager.StopMusic("MainMenu");
			_instance._boat.ToggleCamFollowBoat(to: false);
			CanvasManager.ToggleBlackscreen(to: true, instant: false, 4f);
			LeanTween.cancel(_instance.gameObject);
			LeanTween.value(_instance.gameObject, 0f, 1f, 5f).setEase(LeanTweenType.easeInOutSine).setOnComplete(_instance.HideBoat);
		}
	}

	public static void InstantCrash()
	{
		if ((bool)_instance && _isShowingCutscene)
		{
			SkipManager.OnSkip -= InstantCrash;
			if (!Server.Instance && !IsInMenu)
			{
				ToggleMenu(enabled: true);
				return;
			}
			MusicManager.StopMusic("MainMenu");
			IsInMenu = false;
			_isShowingCutscene = false;
			_instance._menuStuff.gameObject.SetActive(value: false);
			CanvasManager.ToggleMenuUI(enabled: false, instant: true);
			PlayerUI.ToggleUIDisabled(to: false);
			_instance._boat.ToggleBoat(to: false);
			CanvasManager.ToggleBlackscreen(to: false, instant: true);
			LeanTween.cancel(_instance.gameObject);
		}
	}

	private void HideBoat()
	{
		StartCoroutine(HideBoatDealyed());
		SkipManager.OnSkip -= InstantCrash;
		_isShowingCutscene = false;
	}

	private IEnumerator HideBoatDealyed()
	{
		Player.ToggleLocalPlayer(to: true);
		AudioManager.MuteTemporarily();
		_boat.ToggleBoat(to: false);
		if (!Server.Instance)
		{
			yield break;
		}
		ToggleMenu(enabled: false);
		PlayerUI.ToggleUIDisabled(to: false);
		if ((bool)Server.Instance)
		{
			AudioManager.PlayGlobalClip(_explosionSound, variation: false, _explosionVol, 0.1f, bypassMute: true);
			yield return new WaitForSeconds(3f);
			if ((bool)Server.Instance)
			{
				CanvasManager.ToggleBlackscreen(to: false);
			}
		}
	}
}
