using UnityEngine;

public class AttachmentPurchasable : Purchasable
{
	[SerializeField]
	private AttachmentInfo _info;

	private string attachmentName;

	private string _attachmentDescription;

	public override void Hover()
	{
		if (!_info)
		{
			return;
		}
		Player localPlayer = Player.LocalPlayer;
		if (!localPlayer)
		{
			return;
		}
		string text = "";
		string text2 = "";
		bool customCanBuy = true;
		if (!localPlayer.Holding.HeldItem || localPlayer.Holding.HeldItem.SyncedHolder != localPlayer || !localPlayer.Holding.HeldItem.Weapon)
		{
			customCanBuy = false;
			text2 = "\n(" + LocalizationManager.EquipWeaponLocalized.GetLocalizedString() + ")";
			_customCanBuy = customCanBuy;
			text = _info.NameLocalized + " " + text2;
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
		if (!weapon.Attachments.CanAttach(_info))
		{
			customCanBuy = false;
			text2 = "(" + LocalizationManager.CantAttachLocalized.GetLocalizedString() + ")";
			_attachmentDescription = LocalizationManager.CantAttachLocalized.GetLocalizedString() ?? "";
		}
		else if (weapon.Attachments.HasAttachment(_info))
		{
			customCanBuy = false;
			text2 = "(" + LocalizationManager.AlreadyBoughtLocalized.GetLocalizedString() + ")";
			_attachmentDescription = _info.DescriptionLocalized;
		}
		else
		{
			_customCost = weapon.Attachments.GetAttachmentCost(_info);
			text3 = $"\n${_customCost}";
			_attachmentDescription = _info.DescriptionLocalized;
		}
		_customCanBuy = customCanBuy;
		text = ((!_isFree) ? (_info.NameLocalized + " " + text2 + text3 + "\n" + SpriteManager.GetPickUpInput()) : (_info.NameLocalized + "\n" + SpriteManager.GetPickUpInput()));
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
		PlayerUI.ShowInspectInfo(this, _info.NameLocalized, _attachmentDescription);
	}

	public override void Interact(Player player)
	{
		if (!Server.Instance)
		{
			return;
		}
		base.Interact(player);
		if (!_info)
		{
			return;
		}
		if (!MoneyManager.CanAfford(_customCost) || !player.Holding.HeldItem || player.Holding.HeldItem.SyncedHolder != player || !player.Holding.HeldItem.Weapon)
		{
			CantBuyEffects();
			return;
		}
		Weapon weapon = player.Holding.HeldItem.Weapon;
		if (!weapon.Attachments.CanAttach(_info) || weapon.Attachments.HasAttachment(_info))
		{
			CantBuyEffects();
		}
		else
		{
			Server.Instance.BuyAttachment(weapon, GameInfo.GetIndexOfAttachment(_info));
		}
	}
}
