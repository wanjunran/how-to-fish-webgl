using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DeathUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _deathCanvas;

	[SerializeField]
	private GameObject _friendsHelpText;

	[SerializeField]
	private GameObject _bossUI;

	[SerializeField]
	private GameObject _noBossUI;

	[SerializeField]
	private Image _giveUpMask;

	[SerializeField]
	private LocalizeStringEvent _giveUpEvent;

	[SerializeField]
	private LocalizeStringEvent _giveUpHolderEvent;

	private bool _isOn;

	private bool _bossUIEnabled;

	private void Start()
	{
		GameInfo.Input.onControlsChanged += OnControlsChanged;
		RefreshText();
	}

	private void OnDestroy()
	{
		if ((bool)GameInfo.Input)
		{
			GameInfo.Input.onControlsChanged -= OnControlsChanged;
		}
	}

	private void OnControlsChanged(PlayerInput playerInput)
	{
		RefreshText();
	}

	private void RefreshText()
	{
		object[] arguments = new object[1] { SpriteManager.InputToSpriteName(GameInfo.Input.actions["PlayerLeftClick"]) };
		_giveUpEvent.StringReference = LocalizationManager.RespawnLocalized;
		_giveUpEvent.StringReference.Arguments = arguments;
		_giveUpEvent.RefreshString();
		_giveUpHolderEvent.StringReference = LocalizationManager.RespawnLocalized;
		_giveUpHolderEvent.StringReference.Arguments = arguments;
		_giveUpHolderEvent.RefreshString();
	}

	private void FixedUpdate()
	{
		if (_isOn)
		{
			if ((bool)BossManager.Boss && !_bossUIEnabled)
			{
				ToggleBossUI(to: true);
			}
			else if (!BossManager.Boss && _bossUIEnabled)
			{
				ToggleBossUI(to: false);
			}
		}
	}

	public void ToggleDeathUI(bool to)
	{
		_isOn = to;
		ToggleBossUI(BossManager.Boss);
		_friendsHelpText.SetActive(PlayerManager.Players.Count > 1);
		PlayerUI.ToggleMainCanvas(!to);
		_deathCanvas.alpha = 0f;
		_deathCanvas.transform.localScale = Vector3.one * 2f;
		LeanTween.cancel(_deathCanvas.gameObject);
		if (_isOn)
		{
			LeanTween.scale(_deathCanvas.gameObject, Vector3.one, 2f).setEase(LeanTweenType.easeOutQuart);
			LeanTween.value(_deathCanvas.gameObject, _deathCanvas.alpha, 1f, 2f).setEase(LeanTweenType.easeOutQuart).setOnUpdate(UpdateDeathAlpha);
		}
	}

	private void ToggleBossUI(bool to)
	{
		_bossUIEnabled = to;
		_bossUI.SetActive(to);
		_noBossUI.SetActive(!to);
	}

	private void UpdateDeathAlpha(float to)
	{
		_deathCanvas.alpha = to;
	}

	public void StartGivingUp(float totalTime)
	{
		LeanTween.cancel(_giveUpMask.gameObject);
		LeanTween.value(_giveUpMask.gameObject, _giveUpMask.fillAmount, 1f, totalTime).setOnUpdate(UpdateGiveUpMask);
	}

	private void UpdateGiveUpMask(float to)
	{
		_giveUpMask.fillAmount = to;
	}

	public void StopGivingUp(float totalTime)
	{
		LeanTween.cancel(_giveUpMask.gameObject);
		LeanTween.value(_giveUpMask.gameObject, _giveUpMask.fillAmount, 0f, totalTime).setOnUpdate(UpdateGiveUpMask);
	}
}
