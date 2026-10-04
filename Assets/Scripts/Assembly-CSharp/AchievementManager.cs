using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

public class AchievementManager : MonoBehaviour
{
	[FormerlySerializedAs("_resetAchievements")]
	[Header("Testing")]
	[SerializeField]
	private bool _lockAllAchievements;

	[SerializeField]
	private bool _unlockAllAchievements;

	private const string A01_FirstCreature = "A01_FirstCreature";

	private const string A02_Seagull = "A02_Seagull";

	private const string A03_Boss1 = "A03_Boss1";

	private const string A04_Noob = "A04_Noob";

	private const string A05_DripCreature = "A05_DripCreature";

	private const string A06_FlyingBoat = "A06_FlyingBoat";

	private const string A07_BoatUpgrade = "A07_BoatUpgrade";

	private const string A08_Boss2 = "A08_Boss2";

	private const string A09_BurntCreature = "A09_BurntCreature";

	private const string A10_EatMiniBoss = "A10_EatMiniBoss";

	private const string A11_KillscoreMultiplier = "A11_KillscoreMultiplier";

	private const string A12_360Noscope = "A12_360Noscope";

	private const string A13_Boss3 = "A13_Boss3";

	private const string A14_GrillMaster = "A14_GrillMaster";

	private const string A15_SellWorth = "A15_SellWorth";

	private const string A16_AllCreatures = "A16_AllCreatures";

	private const string A17_Roulette = "A17_Roulette";

	private const string A18_LegendarySkin = "A18_LegendarySkin";

	private const string A19_AllAttachments = "A19_AllAttachments";

	private const string A20_Boss4 = "A20_Boss4";

	private const string A21_Boss5 = "A21_Boss5";

	private const string A22_SeagullDynamite = "A22_SeagullDynamite";

	private const string A23_AllDripCreatures = "A23_AllDripCreatures";

	private const string A24_MaxBoat = "A24_MaxBoat";

	private const string A25_FinishGame = "A25_FinishGame";

	private const string A26_FastBoss = "A26_FastBoss";

	private const string A27_Speedrunner = "A27_Speedrunner";

	private const string A28_Boss5MeleeKills = "A28_Boss5MeleeKill";

	private static bool _firstCreatureUnlocked;

	private static bool _hasFlyingboatAchievement;

	private void Start()
	{
		CheckAllAchievements();
	}

	private void CheckAllAchievements()
	{
		if (HasAchievement("A03_Boss1"))
		{
			SkinManager.UnlockLighthouseKeeper();
		}
		if (HasAchievement("A07_BoatUpgrade"))
		{
			SkinManager.UnlockSwampMan();
		}
		if (HasAchievement("A08_Boss2"))
		{
			SkinManager.UnlockSwampLady();
		}
		if (HasAchievement("A09_BurntCreature"))
		{
			SkinManager.UnlockKioskLady();
		}
		if (HasAchievement("A13_Boss3"))
		{
			SkinManager.UnlockTourist();
		}
		if (HasAchievement("A14_GrillMaster"))
		{
			SkinManager.UnlockGrillmaster();
		}
		if (HasAchievement("A17_Roulette"))
		{
			SkinManager.UnlockAndrei();
		}
		if (HasAchievement("A18_LegendarySkin"))
		{
			SkinManager.UnlockJacob();
		}
		if (HasAchievement("A19_AllAttachments"))
		{
			SkinManager.UnlockGunStoreClerc();
		}
		if (HasAchievement("A20_Boss4"))
		{
			SkinManager.UnlockScaredGuyInShorts();
		}
		if (HasAchievement("A24_MaxBoat"))
		{
			SkinManager.UnlockStoreGradma();
		}
		if (HasAchievement("A21_Boss5"))
		{
			SkinManager.UnlockMilitary();
		}
		if (HasAchievement("A25_FinishGame"))
		{
			SkinManager.UnlockScientist();
		}
		if (HasAchievement("A27_Speedrunner"))
		{
			SkinManager.UnlockBean();
		}
	}

	private static void UnlockAchievement(string achievementName)
	{
		SteamUserStats.GetAchievement(achievementName, out var pbAchieved);
		if (!pbAchieved)
		{
			if (SteamUserStats.SetAchievement(achievementName))
			{
				SteamUserStats.StoreStats();
			}
			else
			{
				Debug.LogWarning("Achievement not found: " + achievementName);
			}
		}
	}

	private static bool HasAchievement(string achievementName)
	{
		SteamUserStats.GetAchievement(achievementName, out var pbAchieved);
		return pbAchieved;
	}

	public static void ToggleAllAchievements(bool unlocked)
	{
		if (unlocked)
		{
			for (uint num = 0u; num < SteamUserStats.GetNumAchievements(); num++)
			{
				UnlockAchievement(SteamUserStats.GetAchievementName(num));
			}
		}
		else
		{
			SteamUserStats.ResetAllStats(bAchievementsToo: true);
		}
		SteamUserStats.StoreStats();
	}

	public static void CheckBossAchievement(int bossIndex)
	{
		switch (bossIndex)
		{
		case 0:
			UnlockAchievement("A03_Boss1");
			SkinManager.UnlockLighthouseKeeper();
			break;
		case 1:
			UnlockAchievement("A08_Boss2");
			SkinManager.UnlockSwampLady();
			break;
		case 2:
			UnlockAchievement("A13_Boss3");
			SkinManager.UnlockTourist();
			break;
		case 3:
			UnlockAchievement("A20_Boss4");
			SkinManager.UnlockScaredGuyInShorts();
			break;
		case 4:
			UnlockAchievement("A21_Boss5");
			SkinManager.UnlockMilitary();
			break;
		}
	}

	public static void CheckSlotMachineAchievement(ItemSkin unlockedSkin)
	{
		if (unlockedSkin.Rarity == Rarity.Legendary)
		{
			UnlockAchievement("A18_LegendarySkin");
			SkinManager.UnlockJacob();
		}
	}

	public static void CheckRouletteAchievement(BetColor color)
	{
		if (color == BetColor.Green)
		{
			UnlockAchievement("A17_Roulette");
			SkinManager.UnlockAndrei();
		}
	}

	public static void CheckFirstCreatureAchievement()
	{
		if (!_firstCreatureUnlocked)
		{
			_firstCreatureUnlocked = true;
			UnlockAchievement("A01_FirstCreature");
		}
	}

	public static void CheckPickedUpBySeagullAchievement(Bird bird, Item pickedUp)
	{
		if ((bool)bird && (bool)pickedUp && (bool)pickedUp.DeadPlayer && (bool)pickedUp.DeadPlayer.Player && pickedUp.DeadPlayer.Player.Owner.IsLocalClient)
		{
			UnlockAchievement("A02_Seagull");
		}
	}

	public static void CheckNoobAchievement()
	{
		UnlockAchievement("A04_Noob");
	}

	public static void CheckDripCreatureAchievement(bool isDrip)
	{
		if (isDrip)
		{
			UnlockAchievement("A05_DripCreature");
		}
	}

	public static void CheckFlyingBoatAchievement(float yPos)
	{
		if (!(yPos < 10f) && !_hasFlyingboatAchievement)
		{
			UnlockAchievement("A06_FlyingBoat");
		}
	}

	public static void CheckBoatUpgradeAchievement(byte motor)
	{
		if (motor != 0)
		{
			UnlockAchievement("A07_BoatUpgrade");
			if (motor >= 2)
			{
				UnlockAchievement("A24_MaxBoat");
				SkinManager.UnlockStoreGradma();
			}
			SkinManager.UnlockSwampMan();
		}
	}

	public static void CheckEatingAchievements(Item item)
	{
		if ((bool)item && (bool)item.Creature)
		{
			if (item.Cookness >= 1.5f)
			{
				UnlockAchievement("A09_BurntCreature");
				SkinManager.UnlockKioskLady();
			}
			if (item.Creature.BossType == BossType.Mini)
			{
				UnlockAchievement("A10_EatMiniBoss");
			}
		}
	}

	public static void CheckKillscoreMultiplierAchievement(List<Bonus> bonuses)
	{
		float num = 1f;
		foreach (Bonus bonuse in bonuses)
		{
			num *= bonuse.Worth;
		}
		if (!(num < 5f))
		{
			UnlockAchievement("A11_KillscoreMultiplier");
		}
	}

	public static void Check360NoscopeAchievement(bool did360)
	{
		if (did360)
		{
			UnlockAchievement("A12_360Noscope");
		}
	}

	public static void CheckGrillmasterAchievement()
	{
		UnlockAchievement("A14_GrillMaster");
		SkinManager.UnlockGrillmaster();
	}

	public static void CheckSellWorthAchievement(Item item)
	{
		if ((bool)item && item.TotalWorth >= 100000)
		{
			UnlockAchievement("A15_SellWorth");
		}
	}

	public static void CheckAllCreaturesAchievement(bool onlyDrip)
	{
		if (GameInfo.HasKilledAllCreatures(onlyDrip))
		{
			UnlockAchievement("A16_AllCreatures");
			if (onlyDrip)
			{
				UnlockAchievement("A23_AllDripCreatures");
			}
		}
	}

	public static void CheckAttachmentsAchievement(Attachments attachments)
	{
		if ((bool)attachments.Weapon && (bool)attachments.Weapon.Holder && attachments.Weapon.Holder.Owner.IsLocalClient && attachments.Sight != 0 && attachments.AmmoType != 0 && attachments.BarrelAttachment != 0 && attachments.LaserSight && attachments.ExtendedMag)
		{
			UnlockAchievement("A19_AllAttachments");
			SkinManager.UnlockGunStoreClerc();
		}
	}

	public static void CheckFastBossAchievement()
	{
		UnlockAchievement("A26_FastBoss");
	}

	public static void CheckSeagullDynamiteKillAchievement()
	{
		UnlockAchievement("A22_SeagullDynamite");
	}

	public static void CheckFinalBossMeleeKillAchievement(int hp, int damage)
	{
		if (hp <= 0 && damage < 5)
		{
			UnlockAchievement("A28_Boss5MeleeKill");
		}
	}

	public static void CheckFinishGameAchievement()
	{
		UnlockAchievement("A25_FinishGame");
		SkinManager.UnlockScientist();
	}

	public static void CheckSpeedrunAchievement()
	{
		UnlockAchievement("A27_Speedrunner");
		SkinManager.UnlockBean();
	}
}
