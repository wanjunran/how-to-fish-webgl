using UnityEngine;

public class SharpnessPurchasable : Purchasable
{
	[SerializeField]
	private byte _maxSharpnessUpgrade = 1;

	private string attachmentName;

	private string _attachmentDescription;

	public override void Hover()
	{
		Player localPlayer = Player.LocalPlayer;
		if (!localPlayer)
		{
			return;
		}
		string text = "";
		string text2 = "";
		bool flag = true;
		if (!localPlayer.Holding.HeldItem || localPlayer.Holding.HeldItem.SyncedHolder != localPlayer || !localPlayer.Holding.HeldItem.Melee)
		{
			flag = false;
			text2 = "\n(" + LocalizationManager.EquipMeleeWeaponLocalizedLocalized.GetLocalizedString() + ")";
			_customCanBuy = flag;
			text = LocalizationManager.UpgradeSharpnessLocalized.GetLocalizedString() + " " + text2;
			if (text != _hoverString)
			{
				_isHovering = false;
				_hoverString = text;
			}
			base.Hover();
			return;
		}
		Melee melee = localPlayer.Holding.HeldItem.Melee;
		string text3 = "";
		SharpnessUpgrade curSharpness = melee.GetCurSharpness();
		SharpnessUpgrade nextSharpnessUpgrade = melee.GetNextSharpnessUpgrade(_maxSharpnessUpgrade);
		if (nextSharpnessUpgrade != null)
		{
			flag = true;
			_customCost = nextSharpnessUpgrade.Cost;
			text3 = $"\n${_customCost}";
			_attachmentDescription = $"{LocalizationManager.SharpnessDescriptionLocalized.GetLocalizedString()}\n{curSharpness.Damage} -> {nextSharpnessUpgrade.Damage}";
		}
		else
		{
			flag = false;
			text2 = "(" + LocalizationManager.MaxedOnIslandLocalized.GetLocalizedString() + ")";
			_attachmentDescription = LocalizationManager.MaxedOnIslandLocalized.GetLocalizedString() ?? "";
		}
		_customCanBuy = flag;
		text = ((!_isFree) ? (LocalizationManager.UpgradeSharpnessLocalized.GetLocalizedString() + " " + text2 + text3 + "\n" + SpriteManager.GetPickUpInput()) : (LocalizationManager.UpgradeSharpnessLocalized.GetLocalizedString() + "\n" + SpriteManager.GetPickUpInput()));
		if (text != _hoverString)
		{
			_isHovering = false;
			_hoverString = text;
		}
		base.Hover();
	}

	protected override void OnStartHover()
	{
		base.OnStartHover();
		if (_customCanBuy)
		{
			PlayerUI.ShowInspectInfo(this, LocalizationManager.UpgradeSharpnessLocalized.GetLocalizedString() ?? "", _attachmentDescription);
		}
	}

	public override void Interact(Player player)
	{
		if (!Server.Instance)
		{
			return;
		}
		base.Interact(player);
		if (!_customCanBuy || !MoneyManager.CanAfford(_customCost) || !player.Holding.HeldItem || player.Holding.HeldItem.SyncedHolder != player || !player.Holding.HeldItem.Melee)
		{
			CantBuyEffects();
			return;
		}
		Melee melee = player.Holding.HeldItem.Melee;
		if (melee.GetNextSharpnessUpgrade() == null)
		{
			CantBuyEffects();
		}
		else
		{
			Server.Instance.BuySharpnessUpgrade(melee);
		}
	}
}
