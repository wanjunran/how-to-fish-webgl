using System;
using System.Collections.Generic;

[Serializable]
public class SavedPlayer
{
	public ulong SteamID;

	public int Health = 100;

	public int Fullness = 100;

	public SavedItem HeldItem;

	public List<SavedItem> InventoryItems;

	public byte ExtraSlots;

	public bool HasFinishedTutorial;

	public List<int> OwnedBaits;
}
