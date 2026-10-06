using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
	private static EndGameUI _instance;

	[SerializeField]
	private GameObject _endGameCanvas;

	[SerializeField]
	private Image _background;

	[Space]
	[SerializeField]
	private RectTransform _creditsHolder;

	[SerializeField]
	private RectTransform _creditsStartPos;

	[SerializeField]
	private RectTransform _creditsStopPos;

	[SerializeField]
	private float _creditsDelay = 6f;

	[SerializeField]
	private float _creditsTime = 30f;

	[Header("Skip Cutscene Stuff")]
	[SerializeField]
	private float _cutsceneTextTweenTime = 2f;

	[SerializeField]
	private CanvasGroup _skipHolder;

	[SerializeField]
	private TextMeshProUGUI _skipShortcutText;

	[SerializeField]
	private InputActionReference _skipReference;

	[Header("Explosion")]
	[SerializeField]
	private string _explosionSound;

	[SerializeField]
	private float _explosionVol = 1f;

	public static bool IsShowingEndGame { get; private set; }

	private void Awake()
	{
		_instance = this;
		_creditsHolder.localPosition = _creditsStartPos.localPosition;
		_creditsHolder.gameObject.SetActive(value: false);
		_endGameCanvas.SetActive(value: false);
	}

	public static void OnFinishGame()
	{
		if ((bool)_instance)
		{
			IsShowingEndGame = true;
			if ((bool)Server.Instance && (bool)Player.LocalPlayer)
			{
				Server.Instance.SetIsAfk(Player.LocalPlayer, isAfk: true, fromPause: false);
			}
			_instance.StartEnableCreditsAnimation();
		}
	}

	private void StartEnableCreditsAnimation()
	{
		_creditsHolder.localPosition = _creditsStartPos.localPosition;
		LeanTween.cancel(_creditsHolder);
		_endGameCanvas.SetActive(value: true);
		LeanTween.value(_background.gameObject, 0f, 1f, 3f).setEase(LeanTweenType.easeOutExpo).setOnComplete(ShowCredits)
			.setOnUpdate(SetBackgroundAlpha);
	}

	private void ShowCredits()
	{
		foreach (Player player in PlayerManager.Players)
		{
			if (player.Dying.IsDead)
			{
				player.Vitals.Heal(50);
			}
		}
		OnlineIslandManager.TpToSpecificIsland(0);
		EndGameEffects.OnShowCredits();
		_creditsHolder.gameObject.SetActive(value: true);
		PlayerUI.ToggleUIDisabled(to: true);
		CanvasManager.TogglePermaCanvas(to: false);
		LeanTween.cancel(_creditsHolder);
		_creditsHolder.localPosition = _creditsStartPos.localPosition;
		LeanTween.value(_creditsHolder.gameObject, 0f, 1f, _creditsDelay).setOnComplete(StartScrollingCredits);
		SkipManager.OnSkip += StartDisableCreditsAnimation;
		LeanTween.cancel(_background.gameObject);
		LeanTween.value(_background.gameObject, 1f, 0f, 6f).setEase(LeanTweenType.easeInSine).setOnUpdate(SetBackgroundAlpha);
	}

	private void StartScrollingCredits()
	{
		_creditsHolder.localPosition = _creditsStartPos.localPosition;
		Vector2 vector = _creditsStopPos.localPosition;
		vector.y += _creditsHolder.sizeDelta.y;
		LeanTween.cancel(_creditsHolder);
		LeanTween.moveLocal(_creditsHolder.gameObject, vector, _creditsTime).setOnComplete(ShowSkipCutsceneText);
	}

	private void ShowSkipCutsceneText()
	{
		_skipShortcutText.text = SpriteManager.InputToSpriteName(_skipReference);
		LeanTween.cancel(_skipHolder.gameObject);
		LeanTween.value(_skipHolder.gameObject, 0f, 1f, _cutsceneTextTweenTime).setOnUpdate(SetSkipTextAlpha);
	}

	private void HideSkipCutsceneText()
	{
		LeanTween.cancel(_skipHolder.gameObject);
		LeanTween.value(_skipHolder.gameObject, _skipHolder.alpha, 0f, _cutsceneTextTweenTime * 0.5f).setOnUpdate(SetSkipTextAlpha);
	}

	private void SetSkipTextAlpha(float to)
	{
		_skipHolder.alpha = to;
	}

	private void StartDisableCreditsAnimation()
	{
		EndGameEffects.OnCreditsStopping();
		SkipManager.OnSkip -= StartDisableCreditsAnimation;
		HideSkipCutsceneText();
		LeanTween.value(_background.gameObject, 0f, 1f, 5f).setEase(LeanTweenType.easeOutExpo).setOnComplete(HideCredits)
			.setOnUpdate(SetBackgroundAlpha);
	}

	private void HideCredits()
	{
		StartCoroutine(HideCreditsDelayed());
	}

	private IEnumerator HideCreditsDelayed()
	{
		Player.ToggleLocalPlayer(to: true);
		AudioManager.MuteTemporarily();
		if ((bool)Server.Instance && (bool)Player.LocalPlayer)
		{
			Server.Instance.SetIsAfk(Player.LocalPlayer, isAfk: false, fromPause: false);
		}
		IsShowingEndGame = false;
		_creditsHolder.gameObject.SetActive(value: false);
		PlayerUI.ToggleUIDisabled(to: false);
		CanvasManager.TogglePermaCanvas(to: true);
		EndGameEffects.OnHideCredits();
		if ((bool)Server.Instance)
		{
			AudioManager.PlayGlobalClip(_explosionSound, variation: false, _explosionVol, 0.1f, bypassMute: true);
			yield return new WaitForSeconds(3f);
			if ((bool)Server.Instance)
			{
				LeanTween.cancel(_background.gameObject);
				LeanTween.value(_background.gameObject, 1f, 0f, 3f).setEase(LeanTweenType.easeInOutSine).setOnComplete(ResetEndGameStuff)
					.setOnUpdate(SetBackgroundAlpha);
			}
		}
	}

	private void SetBackgroundAlpha(float to)
	{
		_background.color = new Color(0f, 0f, 0f, to);
	}

	public static void ForceDisableEndGameStuff()
	{
		if ((bool)_instance)
		{
			_instance.ResetEndGameStuff();
		}
	}

	private void ResetEndGameStuff()
	{
		SkipManager.OnSkip -= StartDisableCreditsAnimation;
		_background.color = new Color(0f, 0f, 0f, 0f);
		LeanTween.cancel(_background.gameObject);
		_endGameCanvas.SetActive(value: false);
	}
}
