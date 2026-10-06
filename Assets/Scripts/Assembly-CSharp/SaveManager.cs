using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	[Serializable]
	public class ServerSaveObject
	{
		public string Name;

		public long LastSave;

		public float Playtime;

		public byte SpawnedIsland;

		public byte MaxIsland = 2;

		public int Money;

		public bool UnlockedGrill;

		public bool UseSteam;

		public bool IsPublic;

		public bool UnlockedBoat;

		public bool UnlockedBoatRadar;

		public byte MotorIndex;

		public byte BoatSkin;

		public bool FinalBossKilled;

		public bool HasFinishedGame;

		public Difficulty Difficulty = Difficulty.Default;

		public List<SavedPlayer> Players = new List<SavedPlayer>();
	}

	[Serializable]
	public class LocalSaveObject
	{
		public Vector3 LocalSkinColor;

		public Vector3 LocalHatColor;

		public Vector3 LocalHatColor2;

		public Vector3 LocalHatColor3;

		public Vector3 LocalOutfitColor;

		public Vector3 LocalOutfitColor2;

		public Vector3 LocalOutfitColor3;

		public Vector3 LocalAccessoryColor;

		public Vector3 LocalAccessoryColor2;

		public Vector3 LocalAccessoryColor3;

		public byte LocalHatIndex;

		public byte LocalOutfitIndex;

		public byte LocalAccessoryIndex;

		public bool IsBean;

		public bool InvertXInput;

		public bool InvertYInput;

		public List<SavedItemSkin> UnlockedItemSkins = new List<SavedItemSkin>();

		public List<byte> UnlockedBoatSkins = new List<byte>();

		public List<SavedCreature> CreaturesToList = new List<SavedCreature>();

		public Dictionary<byte, SavedCreature> Creatures = new Dictionary<byte, SavedCreature>();

		public bool[] SeenHats;

		public bool[] SeenOutfits;

		public bool[] SeenAccesories;

		public int SavedLanguage = -1;
	}

	private static SaveManager _instance;

	public static List<ServerSaveObject> ServerSaves = new List<ServerSaveObject>();

	private static LocalSaveObject _curLocalSave;

	private static float _timeOfLastLoad;

	public static ServerSaveObject CurServerSave { get; private set; }

	public static bool HasLocalSave => _curLocalSave != null;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		SaveSystem.Init();
	}

	private void Start()
	{
		LoadLocal();
	}

	public static void OnServerLoaded()
	{
		_timeOfLastLoad = Time.time;
	}

	private void OnApplicationQuit()
	{
	}

	public static float GetTotalPlaytime()
	{
		if (CurServerSave == null)
		{
			return 3600f;
		}
		return CurServerSave.Playtime + Time.time - _timeOfLastLoad;
	}

	public static void SelectServer(string name)
	{
		foreach (ServerSaveObject serverSafe in ServerSaves)
		{
			if (serverSafe.Name == name)
			{
				CurServerSave = serverSafe;
			}
		}
	}

	public static void CreateServer(string serverName, bool useSteam, bool isPublic, Difficulty difficulty)
	{
		MonoBehaviour.print($"useSteam: {useSteam}, isPublic: {isPublic}");
		long ticks = DateTime.UtcNow.Ticks;
		ServerSaveObject obj = new ServerSaveObject
		{
			Name = serverName,
			LastSave = ticks,
			UseSteam = useSteam,
			IsPublic = isPublic,
			Difficulty = difficulty
		};
		CurServerSave = obj;
		SaveSystem.SaveServer(saveString: JsonUtility.ToJson(obj), name: obj.Name);
	}

	public static void SaveServer(bool autoSave = false)
	{
		if (CurServerSave == null || !Server.Instance || !Server.Instance.IsServerInitialized)
		{
			return;
		}
		foreach (Player player in PlayerManager.Players)
		{
			player.Inventory.SaveInventory();
		}
		long ticks = DateTime.UtcNow.Ticks;
		float playtime = CurServerSave.Playtime + Time.time - _timeOfLastLoad;
		_timeOfLastLoad = Time.time;
		ServerSaveObject obj = (CurServerSave = new ServerSaveObject
		{
			Name = CurServerSave.Name,
			Money = MoneyManager.Money,
			LastSave = ticks,
			Playtime = playtime,
			SpawnedIsland = OnlineIslandManager.CurIsland,
			MaxIsland = OnlineIslandManager.MaxIslandUnlocked,
			UseSteam = CurServerSave.UseSteam,
			IsPublic = CurServerSave.IsPublic,
			UnlockedGrill = NPCManager.Instance.GrillUnlocked,
			UnlockedBoat = BoatManager.Boat.BoatUnlocked,
			UnlockedBoatRadar = BoatManager.Boat.BoatRadarUnlocked,
			MotorIndex = BoatManager.Boat.MotorIndex,
			BoatSkin = BoatManager.Boat.CurSkin,
			Players = CurServerSave.Players,
			FinalBossKilled = NPCManager.Instance.FinalBossKilled,
			HasFinishedGame = EndGameManager.Instance.HasFinishedGame,
			Difficulty = CurServerSave.Difficulty
		});
		SaveSystem.SaveServer(saveString: JsonUtility.ToJson(obj), name: obj.Name);
		if (!autoSave && (bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			string text = LocalizationManager.SavedLocalized.GetLocalizedString() + " " + CurServerSave.Name;
			DateTime dateTime = new DateTime(ticks, DateTimeKind.Local);
			string text2 = $"{dateTime:M} - {dateTime:HH:mm:ss}";
			ChatManager.ChatMessage("<i>" + text + " " + text2 + "</i>");
		}
	}

	public static void LoadAllServers()
	{
		List<string> list = SaveSystem.LoadAllServers();
		ServerSaves.Clear();
		CurServerSave = null;
		foreach (string item2 in list)
		{
			if (!string.IsNullOrWhiteSpace(item2))
			{
				ServerSaveObject item = JsonUtility.FromJson<ServerSaveObject>(item2);
				ServerSaves.Add(item);
			}
		}
		ButtonManager.UpdateSavedServerButtons();
	}

	public static void SaveLocal()
	{
		if (_curLocalSave == null)
		{
			Debug.LogError("Could not save local data because it has not been initialized.");
			return;
		}
		List<SavedCreature> list = new List<SavedCreature>();
		foreach (KeyValuePair<byte, SavedCreature> creature in _curLocalSave.Creatures)
		{
			list.Add(creature.Value);
		}
		SaveSystem.SaveLocal(JsonUtility.ToJson(new LocalSaveObject
		{
			LocalSkinColor = LocalSkin.LocalSkinColor,
			LocalHatColor = LocalSkin.LocalHatColor,
			LocalHatColor2 = LocalSkin.LocalHatColor2,
			LocalHatColor3 = LocalSkin.LocalHatColor3,
			LocalOutfitColor = LocalSkin.LocalOutfitColor,
			LocalOutfitColor2 = LocalSkin.LocalOutfitColor2,
			LocalOutfitColor3 = LocalSkin.LocalOutfitColor3,
			LocalAccessoryColor = LocalSkin.LocalAccessoryColor,
			LocalAccessoryColor2 = LocalSkin.LocalAccessoryColor2,
			LocalAccessoryColor3 = LocalSkin.LocalAccessoryColor3,
			LocalHatIndex = LocalSkin.LocalHatIndex,
			LocalOutfitIndex = LocalSkin.LocalOutfitIndex,
			LocalAccessoryIndex = LocalSkin.LocalAccessoryIndex,
			IsBean = LocalSkin.IsBean,
			CreaturesToList = list,
			UnlockedBoatSkins = _curLocalSave.UnlockedBoatSkins,
			UnlockedItemSkins = _curLocalSave.UnlockedItemSkins,
			SeenHats = SkinManager.SeenHats,
			SeenOutfits = SkinManager.SeenOutfits,
			SeenAccesories = SkinManager.SeenAccesories,
			SavedLanguage = LocalizationManager.CurLanguage
		}));
	}

	private void LoadLocal()
	{
		string text = SaveSystem.LoadLocal();
		if (text != null)
		{
			LocalSaveObject localSaveObject;
			try
			{
				localSaveObject = JsonUtility.FromJson<LocalSaveObject>(text);
			}
			catch (Exception arg)
			{
				Debug.LogError($"Could not deserialize local save. Using defaults. {arg}");
				localSaveObject = new LocalSaveObject();
			}
			if (localSaveObject == null)
			{
				localSaveObject = new LocalSaveObject();
			}
			LocalSaveObject localSaveObject2 = localSaveObject;
			if (localSaveObject2.CreaturesToList == null)
			{
				localSaveObject2.CreaturesToList = new List<SavedCreature>();
			}
			localSaveObject2 = localSaveObject;
			if (localSaveObject2.UnlockedBoatSkins == null)
			{
				localSaveObject2.UnlockedBoatSkins = new List<byte>();
			}
			localSaveObject2 = localSaveObject;
			if (localSaveObject2.UnlockedItemSkins == null)
			{
				localSaveObject2.UnlockedItemSkins = new List<SavedItemSkin>();
			}
			localSaveObject.UnlockedItemSkins.RemoveAll((SavedItemSkin savedItemSkin) => savedItemSkin == null);
			localSaveObject.Creatures = new Dictionary<byte, SavedCreature>();
			foreach (SavedCreature creaturesTo in localSaveObject.CreaturesToList)
			{
				if (creaturesTo != null)
				{
					localSaveObject.Creatures[creaturesTo.ID] = creaturesTo;
				}
			}
			foreach (SavedItemSkin unlockedItemSkin in localSaveObject.UnlockedItemSkins)
			{
				if (unlockedItemSkin.SkinsUnlocked == null)
				{
					unlockedItemSkin.SkinsUnlocked = new List<byte>();
				}
			}
			LocalSkin.ChangeMesh(localSaveObject.LocalHatIndex, PlayerSkinType.Hat);
			LocalSkin.ChangeMesh(localSaveObject.LocalOutfitIndex, PlayerSkinType.Outfit);
			LocalSkin.ChangeMesh(localSaveObject.LocalAccessoryIndex, PlayerSkinType.Accessory);
			LocalSkin.SetLocalColor(localSaveObject.LocalSkinColor, PlayerSkinType.Skin, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalHatColor, PlayerSkinType.Hat, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalHatColor2, PlayerSkinType.Hat2, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalHatColor3, PlayerSkinType.Hat3, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalOutfitColor, PlayerSkinType.Outfit, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalOutfitColor2, PlayerSkinType.Outfit2, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalOutfitColor3, PlayerSkinType.Outfit3, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalAccessoryColor, PlayerSkinType.Accessory, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalAccessoryColor2, PlayerSkinType.Accessory2, isPreview: false, updatePreview: false);
			LocalSkin.SetLocalColor(localSaveObject.LocalAccessoryColor3, PlayerSkinType.Accessory3);
			LocalSkin.SetIsBean(localSaveObject.IsBean);
			ButtonManager.LoadBeanFromSave(localSaveObject.IsBean);
			SkinManager.SetSeenClothes(localSaveObject.SeenHats, localSaveObject.SeenOutfits, localSaveObject.SeenAccesories);
			if (localSaveObject.SavedLanguage != -1)
			{
				LocalizationManager.SetLanguage(localSaveObject.SavedLanguage);
			}
			_curLocalSave = localSaveObject;
		}
		else
		{
			_curLocalSave = new LocalSaveObject();
		}
	}

	public static void UnlockSkin(byte itemID, byte skinIndex)
	{
		if (_curLocalSave == null)
		{
			MonoBehaviour.print("local save is null, returning");
			return;
		}
		if (itemID == byte.MaxValue)
		{
			if (!_curLocalSave.UnlockedBoatSkins.Contains(skinIndex))
			{
				_curLocalSave.UnlockedBoatSkins.Add(skinIndex);
				_curLocalSave.UnlockedBoatSkins.Sort();
			}
			return;
		}
		for (int i = 0; i < _curLocalSave.UnlockedItemSkins.Count; i++)
		{
			if (_curLocalSave.UnlockedItemSkins[i].ID != itemID)
			{
				continue;
			}
			foreach (byte item in _curLocalSave.UnlockedItemSkins[i].SkinsUnlocked)
			{
				if (item == skinIndex)
				{
					return;
				}
			}
			_curLocalSave.UnlockedItemSkins[i].SkinsUnlocked.Add(skinIndex);
			_curLocalSave.UnlockedItemSkins[i].SkinsUnlocked.Sort();
			return;
		}
		_curLocalSave.UnlockedItemSkins.Add(new SavedItemSkin
		{
			ID = itemID,
			SkinsUnlocked = new List<byte> { skinIndex }
		});
	}

	public static byte GetSkin(byte itemID, byte curSkin, bool right)
	{
		if (_curLocalSave == null)
		{
			return 0;
		}
		if (itemID == byte.MaxValue)
		{
			if (right)
			{
				for (byte b = 0; b < _curLocalSave.UnlockedBoatSkins.Count; b++)
				{
					if (_curLocalSave.UnlockedBoatSkins[b] > curSkin)
					{
						return _curLocalSave.UnlockedBoatSkins[b];
					}
				}
			}
			else
			{
				for (byte b2 = (byte)(_curLocalSave.UnlockedBoatSkins.Count - 1); b2 < byte.MaxValue; b2--)
				{
					if (_curLocalSave.UnlockedBoatSkins[b2] < curSkin)
					{
						return _curLocalSave.UnlockedBoatSkins[b2];
					}
				}
			}
			if (_curLocalSave.UnlockedBoatSkins.Count == 0)
			{
				return 0;
			}
			if (curSkin == 0 && !right)
			{
				List<byte> unlockedBoatSkins = _curLocalSave.UnlockedBoatSkins;
				return unlockedBoatSkins[unlockedBoatSkins.Count - 1];
			}
			List<byte> unlockedBoatSkins2 = _curLocalSave.UnlockedBoatSkins;
			_ = (curSkin == unlockedBoatSkins2[unlockedBoatSkins2.Count - 1]) & right;
			return 0;
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i < _curLocalSave.UnlockedItemSkins.Count; i++)
		{
			if (_curLocalSave.UnlockedItemSkins[i].ID == itemID)
			{
				list = _curLocalSave.UnlockedItemSkins[i].SkinsUnlocked;
				break;
			}
		}
		if (right)
		{
			for (byte b3 = 0; b3 < list.Count; b3++)
			{
				if (list[b3] > curSkin)
				{
					return list[b3];
				}
			}
		}
		else
		{
			for (byte b4 = (byte)(list.Count - 1); b4 < byte.MaxValue; b4--)
			{
				if (list[b4] < curSkin)
				{
					return list[b4];
				}
			}
		}
		if (list.Count == 0)
		{
			return 0;
		}
		if (curSkin == 0 && !right)
		{
			List<byte> list2 = list;
			return list2[list2.Count - 1];
		}
		List<byte> list3 = list;
		_ = (curSkin == list3[list3.Count - 1]) & right;
		return 0;
	}

	public static bool HasSkin(byte itemID)
	{
		if (itemID == byte.MaxValue)
		{
			if (_curLocalSave != null && _curLocalSave.UnlockedBoatSkins.Count > 0)
			{
				return true;
			}
			return false;
		}
		for (int i = 0; i < _curLocalSave.UnlockedItemSkins.Count; i++)
		{
			if (_curLocalSave.UnlockedItemSkins[i].ID == itemID)
			{
				return true;
			}
		}
		return false;
	}

	public static void SavePlayer(Player player, Item heldItem, Dictionary<byte, Item> slotToItem, byte extraSlots, List<int> ownedBaits)
	{
		if (CurServerSave == null)
		{
			return;
		}
		SavedItem heldItem2 = ItemToSavedItem(0, heldItem);
		List<SavedItem> list = new List<SavedItem>();
		foreach (KeyValuePair<byte, Item> item2 in slotToItem)
		{
			SavedItem item = ItemToSavedItem(item2.Key, item2.Value);
			list.Add(item);
		}
		SavedPlayer savedPlayer = new SavedPlayer
		{
			SteamID = player.SteamID,
			HeldItem = heldItem2,
			Health = Mathf.Clamp(player.Vitals.Health, 10, 100),
			Fullness = player.Vitals.Fullness,
			InventoryItems = list,
			ExtraSlots = extraSlots,
			HasFinishedTutorial = player.ServerHasFinishedTutorial,
			OwnedBaits = ownedBaits
		};
		for (int i = 0; i < CurServerSave.Players.Count; i++)
		{
			if (CurServerSave.Players[i].SteamID == player.SteamID)
			{
				CurServerSave.Players[i] = savedPlayer;
				return;
			}
		}
		CurServerSave.Players.Add(savedPlayer);
	}

	private static SavedItem ItemToSavedItem(byte slot, Item item)
	{
		if ((bool)item)
		{
			return new SavedItem
			{
				Exists = true,
				ItemID = item.ID,
				InventorySlot = slot,
				Cookness = item.Cookness,
				BettingMultiplier = item.BettingMultiplier,
				IsDripCreature = ((bool)item.Creature && item.Creature.IsDrip),
				Weight = (item.Creature ? item.Creature.RandomizedWeight : 1f),
				KillScoreMultiplier = item.KillScoreMultiplier,
				SkinIndex = item.CurSkin,
				Sight = (byte)(item.Weapon ? item.Weapon.Attachments.Sight : 0),
				BarrelAttachment = (byte)(item.Weapon ? item.Weapon.Attachments.BarrelAttachment : 0),
				AmmoType = (byte)(item.Weapon ? item.Weapon.Attachments.AmmoType : 0),
				ExtendedMag = ((bool)item.Weapon && item.Weapon.Attachments.ExtendedMag),
				LaserSight = ((bool)item.Weapon && item.Weapon.Attachments.LaserSight),
				Sharpness = (byte)(item.Melee ? item.Melee.SharpnessIndex : 0)
			};
		}
		return new SavedItem
		{
			Exists = false
		};
	}

	public static void DeleteServer()
	{
		if (CurServerSave != null)
		{
			SaveSystem.DeleteServer(CurServerSave.Name);
		}
	}

	public static SavedPlayer GetSavedPlayer(ulong steamID)
	{
		if (CurServerSave == null)
		{
			return null;
		}
		foreach (SavedPlayer player in CurServerSave.Players)
		{
			if (player.SteamID == steamID)
			{
				return player;
			}
		}
		return null;
	}

	public static bool IsNewFish(byte id, bool to, bool drip)
	{
		if (_curLocalSave == null)
		{
			return false;
		}
		if (_curLocalSave.Creatures.TryGetValue(id, out var value))
		{
			if (!to)
			{
				_curLocalSave.Creatures.Remove(id);
			}
			if (drip && !value.KilledDrip)
			{
				value.KilledDrip = true;
				return true;
			}
		}
		else if (to)
		{
			SavedCreature value2 = new SavedCreature
			{
				ID = id,
				BeenKilled = true,
				KilledDrip = drip
			};
			_curLocalSave.Creatures.Add(id, value2);
			return true;
		}
		return false;
	}

	public static bool HasKilledCreature(byte id, bool dripVersion)
	{
		if (_curLocalSave.Creatures.TryGetValue(id, out var value))
		{
			if (!dripVersion)
			{
				return value.BeenKilled;
			}
			return value.KilledDrip;
		}
		return false;
	}

	public static SavedCreature GetSavedCreature(Creature creature)
	{
		SavedCreature value;
		if (_curLocalSave == null)
		{
			MonoBehaviour.print("no local save set");
		}
		else if (_curLocalSave.Creatures.TryGetValue(creature.ID, out value))
		{
			return value;
		}
		return new SavedCreature
		{
			ID = creature.ID
		};
	}

	public static bool PlayerHasFinishedTutorial(Player player)
	{
		if (CurServerSave == null)
		{
			return true;
		}
		foreach (SavedPlayer player2 in CurServerSave.Players)
		{
			if (player2.SteamID == player.SteamID)
			{
				return player2.HasFinishedTutorial;
			}
		}
		return false;
	}

	public static bool PlayerHasJoinedPreviously(Player player)
	{
		if (CurServerSave == null)
		{
			return true;
		}
		foreach (SavedPlayer player2 in CurServerSave.Players)
		{
			if (player2.SteamID == player.SteamID)
			{
				return true;
			}
		}
		return false;
	}

	public static void LockAllSkins()
	{
		if (_curLocalSave != null)
		{
			_curLocalSave.UnlockedBoatSkins = new List<byte>();
			_curLocalSave.UnlockedItemSkins = new List<SavedItemSkin>();
		}
	}

	public static void ToggleInvertedMouse(bool xInput)
	{
		if (_curLocalSave != null)
		{
			if (xInput)
			{
				_curLocalSave.InvertXInput = !_curLocalSave.InvertXInput;
			}
			else
			{
				_curLocalSave.InvertYInput = !_curLocalSave.InvertYInput;
			}
		}
	}

	public static void OnQuit()
	{
		SaveServer(autoSave: true);
		SaveLocal();
	}
}
