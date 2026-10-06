using UnityEngine;

public class QuestInteractable : Interactable
{
	[SerializeField]
	[Tooltip("Mesh to be updated when NPCs give specific bait")]
	private MeshFilter _meshToUpdate;

	private NPC _npc;

	private string _hoverString;

	private NPCQuest _quest;

	public void SetQuest(NPCQuest quest)
	{
		_quest = quest;
		string text = "";
		_meshToUpdate.mesh = quest.InteractableMesh;
		switch (quest.Type)
		{
		case QuestType.GiveBait:
			if ((bool)quest.BaitToReceive)
			{
				text = quest.BaitToReceive.NameLocalized;
				_meshToUpdate.mesh = quest.BaitToReceive.MeshForNpc;
			}
			break;
		case QuestType.UnlockGrill:
			text = LocalizationManager.LighterLocalized.GetLocalizedString();
			break;
		case QuestType.UnlockIsland:
			text = $"{LocalizationManager.UsbForIslandLocalized.GetLocalizedString()} {quest.IslandToUnlock}";
			break;
		case QuestType.UnlockBoat:
			text = LocalizationManager.BoatKeysLocalized.GetLocalizedString();
			break;
		}
		_hoverString = LocalizationManager.TakeLocalized.GetLocalizedString() + " " + text + "\n" + SpriteManager.GetPickUpInput();
	}

	public override void Hover()
	{
		if (!_isHovering)
		{
			PlayerUI.SetLookAtText(_hoverString, base.TextTarget);
		}
		base.Hover();
	}

	public override void UnHover()
	{
		PlayerUI.HideLookAtText(_hoverString);
		base.UnHover();
	}

	public override void Interact(Player player)
	{
		if ((bool)Server.Instance && (bool)_quest)
		{
			base.Interact(player);
			Server.Instance.TakeItemFromNpc(player, _npc.ID);
		}
	}

	public void LinkNpc(NPC npc)
	{
		_npc = npc;
	}
}
