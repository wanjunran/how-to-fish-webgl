using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DazedCommands : MonoBehaviour
{
	private const string SpawnCommand = "spawn";

	private const string SpawnDeadCommand = "spawndead";

	private const string SpawnDripCommand = "spawndrip";

	private const string SpawnDeadDripCommand = "spawndripdead";

	private const string GrillCommand = "grill";

	private const string BoatCommand = "boat";

	private const string GiveCommand = "addmoney";

	private const string RemoveCommand = "removemoney";

	private const string NextIslandCommand = "nextisland";

	private const string PrevIslandCommand = "previsland";

	private const string GodModeCommand = "godmode";

	private const string OneShotCommand = "oneshot";

	private const string KillAllCreaturesCommand = "killallcreatures";

	private const string KillAllDripCreaturesCommand = "killalldripcreatures";

	private const string ResetAllCreaturesCommand = "resetallcreatures";

	private const string ResetAllDripCreaturesCommand = "resetalldripcreatures";

	private const string SlotsCommand = "slots";

	private const string KillBossCommand = "killboss";

	private const string UnlockAllSkinsCommand = "allskins";

	private const string LockAllSkinsCommand = "noskins";

	private const string ShowKillscoresCommand = "showkillscores";

	private const string FinishGameCommand = "finishgame";

	private const string UnlockAchievementsCommand = "unlockachievements";

	private const string LockAchievementsCommand = "lockachievements";

	public static bool IsServerCommand(string fullCommand)
	{
		if (string.IsNullOrEmpty(fullCommand))
		{
			return false;
		}
		if (!fullCommand.StartsWith("/"))
		{
			return false;
		}
		if (!ClientSettings.CheatsEnabled)
		{
			ChatManager.ChatMessage("Only dev is allowed to type commands");
			return true;
		}
		if (!Server.Instance || !Server.Instance.IsServerInitialized)
		{
			ChatManager.ChatMessage("Only host is allowed to type commands");
			return true;
		}
		string[] array = fullCommand.Split(' ');
		string text = array[0];
		string text2 = "";
		for (int i = 1; i < array.Length; i++)
		{
			text2 += array[i];
		}
		text = text.Remove(0, 1).ToLower();
		string[] array2 = array;
		if (array2.Length > 1)
		{
			array2 = array2.Skip(1).ToArray();
		}
		if (text == "spawn" && !string.IsNullOrEmpty(text2))
		{
			UseSpawnCommand(text2);
		}
		else if (text == "spawndead" && !string.IsNullOrEmpty(text2))
		{
			UseSpawnCommand(text2, asDead: true);
		}
		else if (text == "spawndrip" && !string.IsNullOrEmpty(text2))
		{
			UseSpawnDripCommand(text2);
		}
		else if (text == "spawndripdead" && !string.IsNullOrEmpty(text2))
		{
			UseSpawnDripCommand(text2, asDead: true);
		}
		else
		{
			switch (text)
			{
			case "grill":
				UseGrillCommand();
				break;
			case "boat":
				UseBoatCommand();
				break;
			case "addmoney":
				UseAddMoneyCommand();
				break;
			case "removemoney":
				UseRemoveMoneyCommand();
				break;
			case "nextisland":
				UseNextIslandCommand();
				break;
			case "previsland":
				UseNextIslandCommand(prev: true);
				break;
			case "godmode":
				UseGodModeCommand();
				break;
			case "oneshot":
				UseOneShotCommand();
				break;
			case "killallcreatures":
				UseToggleAllCreaturesKilledCommand(to: true, drip: false);
				break;
			case "killalldripcreatures":
				UseToggleAllCreaturesKilledCommand(to: true, drip: true);
				break;
			case "resetallcreatures":
				UseToggleAllCreaturesKilledCommand(to: false, drip: false);
				break;
			case "resetalldripcreatures":
				UseToggleAllCreaturesKilledCommand(to: false, drip: true);
				break;
			case "slots":
				UseSlotsCommand(array2);
				break;
			case "killboss":
				UseKillBossCommand();
				break;
			case "allskins":
				UseUnlockAllSkinsCommand();
				break;
			case "noskins":
				UseLockAllSkinsCommand();
				break;
			case "showkillscores":
				UseShowKillScoresCommand();
				break;
			case "finishgame":
				UseFinishGameCommand();
				break;
			case "unlockachievements":
				ToggleAchievements(unlock: true);
				break;
			case "lockachievements":
				ToggleAchievements(unlock: false);
				break;
			default:
				ChatManager.ChatMessage("Couldn't find command called <b>" + fullCommand + "</b>");
				break;
			}
		}
		return true;
	}

	private static void ToggleAchievements(bool unlock)
	{
		AchievementManager.ToggleAllAchievements(unlock);
	}

	private static void UseFinishGameCommand()
	{
		if ((bool)EndGameManager.Instance)
		{
			EndGameManager.Instance.FinishGameInput();
		}
	}

	private static void UseShowKillScoresCommand()
	{
		for (int i = 0; i < 3; i++)
		{
			List<Bonus> allBonuses = KillScoreCalculator.GetAllBonuses(i);
			Player.LocalPlayer.KillScore.AddKillScore($"All killscores {i}", 100, allBonuses);
		}
	}

	private static void UseUnlockAllSkinsCommand()
	{
		SaveManager.LockAllSkins();
		Item[] itemWithSkinsforCommands = GameInfo.ItemWithSkinsforCommands;
		foreach (Item item in itemWithSkinsforCommands)
		{
			if ((bool)item.SkinPreset)
			{
				for (int j = 0; j < item.SkinPreset.Skins.Count; j++)
				{
					SaveManager.UnlockSkin(item.ID, (byte)j);
				}
			}
		}
		for (int k = 0; k < BoatManager.Boat.SkinPreset.Skins.Count; k++)
		{
			SaveManager.UnlockSkin(byte.MaxValue, (byte)k);
		}
	}

	private static void UseLockAllSkinsCommand()
	{
		SaveManager.LockAllSkins();
	}

	private static void UseKillBossCommand()
	{
		if ((bool)BossManager.Boss)
		{
			BossManager.Boss.LocalHit(BossManager.Boss.transform, BossManager.Boss.transform.position, Vector3.up, Player.LocalPlayer, 999999, rangedHit: false, Vector3.zero);
		}
	}

	private static void UseSlotsCommand(string[] seperatedSubCommands)
	{
		if (seperatedSubCommands.Length >= 2)
		{
			Item spawnable = GameInfo.GetSpawnable(seperatedSubCommands[0]);
			byte b = (byte)(spawnable ? spawnable.SkinPreset : BoatManager.Boat.SkinPreset).Skins.Count;
			if (byte.TryParse(seperatedSubCommands[1], out var result) && result < b)
			{
				SlotMachineManager.SetCheatSkin(spawnable, result);
			}
		}
	}

	private static void UseToggleAllCreaturesKilledCommand(bool to, bool drip)
	{
		GameInfo.ToggleAllCreaturesKilled(to, drip);
	}

	private static void UseSpawnCommand(string subCommands, bool asDead = false)
	{
		Item spawnable = GameInfo.GetSpawnable(subCommands.Replace(" ", "").ToLower());
		if ((bool)spawnable)
		{
			Vector3 position = GameInfo.CurCamera.transform.position + GameInfo.CurCamera.transform.forward * 2f;
			Item item = Object.Instantiate(spawnable, position, Quaternion.identity);
			if (asDead)
			{
				item.Creature.ServerKillOnSpawn();
			}
			Server.Instance.Spawn(item.gameObject);
		}
	}

	private static void UseSpawnDripCommand(string subCommands, bool asDead = false)
	{
		Creature component = GameInfo.GetSpawnable(subCommands.Replace(" ", "").ToLower()).GetComponent<Creature>();
		if ((bool)component)
		{
			Vector3 position = GameInfo.CurCamera.transform.position + GameInfo.CurCamera.transform.forward * 2f;
			Creature creature = Object.Instantiate(component, position, Quaternion.identity);
			creature.SetDrip();
			if (asDead)
			{
				creature.Creature.ServerKillOnSpawn();
			}
			Server.Instance.Spawn(creature.gameObject);
		}
	}

	private static void UseGrillCommand()
	{
		NPCManager.UnlockGrill();
	}

	private static void UseBoatCommand()
	{
		BoatManager.Boat.UnlockBoat();
	}

	private static void UseAddMoneyCommand()
	{
		MoneyManager.AddMoney(9999, Player.LocalPlayer);
	}

	private static void UseRemoveMoneyCommand()
	{
		MoneyManager.RemoveMoney(9999, Player.LocalPlayer);
	}

	private static void UseNextIslandCommand(bool prev = false)
	{
		OnlineIslandManager.TpToNextIsland(prev);
	}

	private static void UseGodModeCommand()
	{
		PlayerManager.ToggleGodMode();
	}

	private static void UseOneShotCommand()
	{
		ServerSettings.Instance.ToggleOneShot();
	}
}
