using UnityEngine;

public class SlotPurchasable : Purchasable
{
	[SerializeField]
	private byte _maxSlot = 1;

	private byte _targetSlotIndex;

	public override void Hover()
	{
		Player localPlayer = Player.LocalPlayer;
		if ((bool)localPlayer)
		{
			string text = "";
			string text2 = "";
			bool customCanBuy = true;
			_targetSlotIndex = (byte)(localPlayer.Inventory.ExtraSlots + 1);
			if (_targetSlotIndex > _maxSlot)
			{
				customCanBuy = false;
				text2 = "\n(" + LocalizationManager.MaxedOnIslandLocalized.GetLocalizedString() + ")";
				text = LocalizationManager.ExtraInventorySlotLocalized.GetLocalizedString() + " " + text2;
			}
			else
			{
				_customCost = localPlayer.Inventory.GetExtraSlotCost(_targetSlotIndex);
				text = ((!_isFree) ? $"{LocalizationManager.ExtraInventorySlotLocalized.GetLocalizedString()} {text2}\n${_customCost}\n{SpriteManager.GetPickUpInput()}" : (LocalizationManager.ExtraInventorySlotLocalized.GetLocalizedString() + "\n" + SpriteManager.GetPickUpInput()));
			}
			_customCanBuy = customCanBuy;
			if (text != _hoverString)
			{
				_isHovering = false;
				_hoverString = text;
			}
			base.Hover();
		}
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
			else if (_targetSlotIndex > _maxSlot)
			{
				CantBuyEffects();
			}
			else
			{
				Server.Instance.UnlockPocket(player, _targetSlotIndex);
			}
		}
	}
}
