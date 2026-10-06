using UnityEngine;

public class EndGameInteractable : Interactable
{
	private string _hoverText;

	public override bool InteractableWhenHoldingItem => false;

	public override void Hover()
	{
		if (!_isHovering)
		{
			_hoverText = (NPCManager.Instance.FinalBossKilled ? (LocalizationManager.GetBackLocalized.GetLocalizedString() + "\n" + SpriteManager.GetPickUpInput()) : (LocalizationManager.CantDriveLocalized.GetLocalizedString() + "\n" + LocalizationManager.NoKeysLocalized.GetLocalizedString()));
			PlayerUI.SetLookAtText(_hoverText, base.TextTarget);
			PlayerUI.SetLookAtColor(NPCManager.Instance.FinalBossKilled ? Color.white : GameInfo.RedColor);
		}
		base.Hover();
	}

	public override void UnHover()
	{
		PlayerUI.HideLookAtText(_hoverText);
		base.UnHover();
	}

	public override void Interact(Player player)
	{
		if (NPCManager.Instance.FinalBossKilled && !BossManager.Boss)
		{
			base.Interact(player);
			EndGameManager.Instance.FinishGameInput();
		}
	}
}
