using UnityEngine;

public abstract class Purchasable : Interactable
{
	[SerializeField]
	protected bool _isFree;

	[SerializeField]
	protected int _customCost;

	protected string _hoverString;

	protected bool _customCanBuy = true;

	private string _finalHoverString;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PlayerUI.HideLookAtText(_finalHoverString);
		PlayerUI.HideInspectInfo(this);
	}

	public override void Hover()
	{
		string text = _hoverString;
		if ((bool)Player.LocalPlayer.Holding.HeldItem && !InteractableWhenHoldingItem)
		{
			text = text + "\n(" + LocalizationManager.UnequipItemLocalized.GetLocalizedString() + ")";
		}
		if (!_isHovering)
		{
			OnStartHover();
			PlayerUI.SetLookAtText(text, base.TextTarget);
		}
		else if (text != _finalHoverString)
		{
			PlayerUI.UpdateLookAtText(text);
		}
		_finalHoverString = text;
		PlayerUI.SetLookAtColor((MoneyManager.CanAfford(_customCost) && _customCanBuy) ? Color.white : GameInfo.RedColor);
		base.Hover();
	}

	protected virtual void OnStartHover()
	{
	}

	public override void UnHover()
	{
		PlayerUI.HideLookAtText(_finalHoverString);
		PlayerUI.HideInspectInfo(this);
		base.UnHover();
	}

	protected void CantBuyEffects()
	{
		MonoBehaviour.print("Cant afford idiot");
		AudioManager.PlayGlobalClip("Error", variation: true, 0.3f);
	}
}
