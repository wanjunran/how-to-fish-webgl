using UnityEngine;
using UnityEngine.Localization;

public class MotorPurchasable : Purchasable
{
	[Space]
	[SerializeField]
	private LocalizedString _motorNameLocalized;

	[SerializeField]
	private byte _motorIndex;

	public override bool InteractableWhenHoldingItem => true;

	public override void Hover()
	{
		bool flag = BoatManager.Boat.MotorIndex < _motorIndex;
		string text = (flag ? $"{_motorNameLocalized.GetLocalizedString()}\n${_customCost}\n{SpriteManager.GetPickUpInput()}" : $"{_motorNameLocalized.GetLocalizedString()}\n${_customCost}\n{LocalizationManager.AlreadyBoughtLocalized.GetLocalizedString()}");
		if (flag != _customCanBuy || text != _hoverString || !_isHovering)
		{
			PlayerUI.SetLookAtText(text, base.TextTarget);
		}
		_customCanBuy = flag;
		_hoverString = text;
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
				Server.Instance.BuyBoatMotor(player, _motorIndex, _customCost);
			}
		}
	}
}
