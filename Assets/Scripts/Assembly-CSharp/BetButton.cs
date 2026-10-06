using System.Collections.Generic;
using UnityEngine;

public class BetButton : Interactable
{
	[SerializeField]
	private BetColor _betColor;

	[SerializeField]
	private int _multiplier = 2;

	private static List<BetButton> _betButtons = new List<BetButton>();

	private string _hoverText;

	protected override void Awake()
	{
		base.Awake();
		_betButtons.Add(this);
	}

	protected override void OnDestroy()
	{
		_betButtons.Remove(this);
	}

	public override void Hover()
	{
		if (!_isHovering)
		{
			string text = "";
			switch (_betColor)
			{
			case BetColor.Black:
				text = LocalizationManager.BlackLocalized.GetLocalizedString();
				break;
			case BetColor.Red:
				text = LocalizationManager.RedLocalized.GetLocalizedString();
				break;
			case BetColor.Green:
				text = LocalizationManager.GreenLocalized.GetLocalizedString();
				break;
			}
			_hoverText = $"{LocalizationManager.BetOnLocalized.GetLocalizedString()} {text}\nx{_multiplier}\n{SpriteManager.GetPickUpInput()}";
			PlayerUI.SetLookAtText(_hoverText, base.TextTarget);
		}
		base.Hover();
	}

	public override void UnHover()
	{
		PlayerUI.HideLookAtText(_hoverText);
		base.UnHover();
	}

	public override void Interact(Player player)
	{
		Server.Instance.PlaceBet((byte)_betColor);
	}

	public static void ToggleSpecific(BetColor betColor)
	{
		foreach (BetButton betButton in _betButtons)
		{
			betButton.gameObject.SetActive(value: true);
			betButton.ToggleIsInteractable(to: false);
			if (betButton._betColor == betColor)
			{
				LeanTween.cancel(betButton.gameObject);
				betButton.transform.localScale = Vector3.zero;
				LeanTween.scale(betButton.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
			}
			else
			{
				LeanTween.cancel(betButton.gameObject);
				LeanTween.scale(betButton.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack).setOnComplete(betButton.Disable);
			}
		}
	}

	private void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public static void ToggleAll(bool to)
	{
		foreach (BetButton betButton in _betButtons)
		{
			betButton.gameObject.SetActive(value: true);
			betButton.ToggleIsInteractable(to);
			LeanTween.cancel(betButton.gameObject);
			if (to)
			{
				LeanTween.scale(betButton.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
			}
			else
			{
				LeanTween.scale(betButton.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack).setOnComplete(betButton.Disable);
			}
		}
	}
}
