using System;
using UnityEngine;

public class ItemPurchasable : Purchasable
{
	[Space]
	[SerializeField]
	private Item _itemToPurchase;

	public override bool InteractableWhenHoldingItem => true;

	public static event Action OnItemBought;

	protected override void Awake()
	{
		base.Awake();
		if ((bool)_itemToPurchase)
		{
			_hoverString = ((!_isFree) ? $"{_itemToPurchase.GetName()}\n${_itemToPurchase.Cost}\n{SpriteManager.GetPickUpInput()}" : (_itemToPurchase.GetName() + "\n" + SpriteManager.GetPickUpInput()));
			_customCost = ((!_isFree) ? _itemToPurchase.Cost : 0);
		}
	}

	public override void Hover()
	{
		if ((bool)_itemToPurchase)
		{
			_hoverString = ((!_isFree) ? $"{_itemToPurchase.GetName()}\n${_itemToPurchase.Cost}\n{SpriteManager.GetPickUpInput()}" : (_itemToPurchase.GetName() + "\n" + SpriteManager.GetPickUpInput()));
			_customCost = ((!_isFree) ? _itemToPurchase.Cost : 0);
		}
		base.Hover();
	}

	public override void Interact(Player player)
	{
		if (!Server.Instance)
		{
			return;
		}
		base.Interact(player);
		if (!MoneyManager.CanAfford(_customCost))
		{
			CantBuyEffects();
			return;
		}
		if ((bool)_itemToPurchase)
		{
			player.Hands.PrepareForPurchasedItem(_itemToPurchase.ID);
			Server.Instance.BuyItem(_itemToPurchase.ID, player, player.Holding.HeldItem, _modelsToOutline[0].transform.position, base.transform.rotation * Quaternion.Euler(90f, 0f, 0f));
		}
		if (!_isFree)
		{
			OnItemBought?.Invoke();
		}
	}
}
