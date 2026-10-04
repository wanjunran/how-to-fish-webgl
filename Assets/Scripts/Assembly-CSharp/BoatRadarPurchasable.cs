using UnityEngine;
using UnityEngine.Localization;

public class BoatRadarPurchasable : Purchasable
{
	[Space]
	[SerializeField]
	private LocalizedString _radarNameLocalized;

	public override bool InteractableWhenHoldingItem => true;

	public override void Hover()
	{
		if (!_isHovering)
		{
			_customCanBuy = !BoatManager.Boat.BoatRadarUnlocked;
			_hoverString = (_customCanBuy ? $"{_radarNameLocalized.GetLocalizedString()}\n${_customCost}\n{SpriteManager.GetPickUpInput()}" : $"{_radarNameLocalized.GetLocalizedString()}\n${_customCost}\n{LocalizationManager.AlreadyBoughtLocalized.GetLocalizedString()}");
			PlayerUI.SetLookAtText(_hoverString, base.TextTarget);
		}
		PlayerUI.SetLookAtColor((MoneyManager.CanAfford(_customCost) && _customCanBuy) ? Color.white : GameInfo.RedColor);
		base.Hover();
	}

	public override void UnHover()
	{
		PlayerUI.HideLookAtText(_hoverString);
		base.UnHover();
	}

	public override void Interact(Player player)
	{
		if ((bool)Server.Instance)
		{
			base.Interact(player);
			if (!MoneyManager.CanAfford(_customCost))
			{
				CantBuyEffects();
			}
			else
			{
				Server.Instance.BuyBoatRadar(player, _customCost);
			}
		}
	}
}
