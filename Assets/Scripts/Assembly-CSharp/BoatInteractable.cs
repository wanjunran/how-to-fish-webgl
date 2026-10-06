using UnityEngine;

public class BoatInteractable : Interactable
{
	private string _hoverText;

	public override bool InteractableWhenHoldingItem => false;

	protected override void Awake()
	{
		base.Awake();
		ToggleIsInteractable(to: false);
	}

	public override void Hover()
	{
		string text = (BoatManager.Boat.BoatUnlocked ? (LocalizationManager.DriveLocalized.GetLocalizedString() + "\n" + SpriteManager.GetPickUpInput()) : (LocalizationManager.CantDriveLocalized.GetLocalizedString() + "\n" + LocalizationManager.NoKeysLocalized.GetLocalizedString()));
		if ((bool)Player.LocalPlayer.Holding.HeldItem && !InteractableWhenHoldingItem)
		{
			text = text + "\n(" + LocalizationManager.UnequipItemLocalized.GetLocalizedString() + ")";
		}
		if (!_isHovering)
		{
			PlayerUI.SetLookAtText(text, base.TextTarget);
			PlayerUI.SetLookAtColor(BoatManager.Boat.BoatUnlocked ? Color.white : GameInfo.RedColor);
		}
		else if (text != _hoverText)
		{
			PlayerUI.UpdateLookAtText(text);
		}
		_hoverText = text;
		base.Hover();
	}

	public override void UnHover()
	{
		PlayerUI.HideLookAtText(_hoverText);
		base.UnHover();
	}

	public override void Interact(Player player)
	{
		if (BoatManager.Boat.BoatUnlocked)
		{
			base.Interact(player);
			Boat.ToggleWantToDrive(to: true);
			Server.Instance.SetDriver(player);
		}
	}
}
