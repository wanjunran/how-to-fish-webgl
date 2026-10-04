using UnityEngine;

public class BaitPurchasable : Purchasable
{
	[Space]
	[SerializeField]
	private BaitInfo _bait;

	[SerializeField]
	private bool _skipInspectText;

	protected override void Awake()
	{
		base.Awake();
		if ((bool)_bait)
		{
			_hoverString = ((!_isFree) ? $"{_bait.NameLocalized}\n${_bait.Cost}\n{SpriteManager.GetPickUpInput()}" : (_bait.NameLocalized + "\n" + SpriteManager.GetPickUpInput()));
			_customCost = ((!_isFree) ? _bait.Cost : 0);
		}
	}

	public override void Hover()
	{
		if ((bool)_bait)
		{
			_hoverString = ((!_isFree) ? $"{_bait.NameLocalized}\n${_bait.Cost}\n{SpriteManager.GetPickUpInput()}" : (_bait.NameLocalized + "\n" + SpriteManager.GetPickUpInput()));
		}
		base.Hover();
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
			else if ((bool)_bait)
			{
				Server.Instance.BuyBait(player, GameInfo.GetIndexOfBait(_bait), _customCost);
			}
		}
	}

	protected override void OnStartHover()
	{
		base.OnStartHover();
		if (!_skipInspectText)
		{
			PlayerUI.ShowInspectInfo(this, _bait.NameLocalized, _bait.Description);
		}
	}
}
