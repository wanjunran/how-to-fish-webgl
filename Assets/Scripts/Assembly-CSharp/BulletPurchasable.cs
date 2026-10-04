using UnityEngine;

public class BulletPurchasable : Purchasable
{
	[SerializeField]
	private byte _maxBulletUpgrade = 1;

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
		if (!localPlayer.Holding.HeldItem || localPlayer.Holding.HeldItem.SyncedHolder != localPlayer || !localPlayer.Holding.HeldItem.Weapon)
		{
			flag = false;
			text2 = "\n(" + LocalizationManager.EquipWeaponLocalized.GetLocalizedString() + ")";
			_customCanBuy = flag;
			text = LocalizationManager.UpgradeBulletsLocalized.GetLocalizedString() + " " + text2;
			if (text != _hoverString)
			{
				_isHovering = false;
				_hoverString = text;
			}
			base.Hover();
			return;
		}
		Weapon weapon = localPlayer.Holding.HeldItem.Weapon;
		string text3 = "";
		BulletUpgrade curBulletUpgrade = weapon.Attachments.GetCurBulletUpgrade();
		BulletUpgrade nextBulletUpgrade = weapon.Attachments.GetNextBulletUpgrade(_maxBulletUpgrade);
		if (nextBulletUpgrade != null)
		{
			flag = true;
			_customCost = nextBulletUpgrade.Cost;
			text3 = $"\n${_customCost}";
			_attachmentDescription = $"{LocalizationManager.BulletDescriptionLocalized.GetLocalizedString()}\n{curBulletUpgrade.Damage} -> {nextBulletUpgrade.Damage}";
		}
		else
		{
			flag = false;
			text2 = "(" + LocalizationManager.MaxedOnIslandLocalized.GetLocalizedString() + ")";
			_attachmentDescription = LocalizationManager.MaxedOnIslandLocalized.GetLocalizedString() ?? "";
		}
		_customCanBuy = flag;
		text = ((!_isFree) ? (LocalizationManager.UpgradeBulletsLocalized.GetLocalizedString() + " " + text2 + text3 + "\n" + SpriteManager.GetPickUpInput()) : (LocalizationManager.UpgradeBulletsLocalized.GetLocalizedString() + "\n" + SpriteManager.GetPickUpInput()));
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
		PlayerUI.ShowInspectInfo(this, LocalizationManager.UpgradeBulletsLocalized.GetLocalizedString() ?? "", _attachmentDescription);
	}

	public override void Interact(Player player)
	{
		if (!Server.Instance)
		{
			return;
		}
		base.Interact(player);
		if (!_customCanBuy || !MoneyManager.CanAfford(_customCost) || !player.Holding.HeldItem || player.Holding.HeldItem.SyncedHolder != player || !player.Holding.HeldItem.Weapon)
		{
			CantBuyEffects();
			return;
		}
		Weapon weapon = player.Holding.HeldItem.Weapon;
		if (weapon.Attachments.GetNextBulletUpgrade() == null)
		{
			CantBuyEffects();
		}
		else
		{
			Server.Instance.BuyBulletUpgrade(weapon);
		}
	}
}
