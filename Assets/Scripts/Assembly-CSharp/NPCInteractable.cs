public class NPCInteractable : Interactable
{
	private string _hoverText;

	public override void Hover()
	{
		if (!_isHovering && !NpcUI.NPCDialougeOpen)
		{
			_hoverText = LocalizationManager.TalkLocalized.GetLocalizedString() + " " + SpriteManager.GetPickUpInput();
			PlayerUI.SetLookAtText(_hoverText, base.TextTarget);
		}
		base.Hover();
	}

	public void OnNPCTalked()
	{
		if (_isHovering)
		{
			UnHover();
		}
	}

	public override void UnHover()
	{
		PlayerUI.HideLookAtText(_hoverText);
		base.UnHover();
	}

	public override void Interact(Player player)
	{
		base.Interact(player);
		PlayerUI.HideLookAtText(_hoverText);
	}
}
