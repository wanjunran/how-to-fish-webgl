using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KillScoreCalculator : MonoBehaviour
{
	private static KillScoreCalculator _instance;

	[SerializeField]
	private bool _useFakeBonuses;

	[SerializeField]
	private List<Bonus> _fakeBonuses;

	private static List<Bonus> _bonuses = new List<Bonus>();

	private static bool _did360;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
	}

	private static void ResetBonuses()
	{
		_bonuses.Clear();
		if (!_instance || !_instance._useFakeBonuses)
		{
			return;
		}
		foreach (Bonus fakeBonuse in _instance._fakeBonuses)
		{
			_bonuses.Add(fakeBonuse);
		}
	}

	public static List<Bonus> GetExplosionBonuses()
	{
		ResetBonuses();
		CheckKillType(KillType.Explosion);
		return _bonuses.OrderByDescending((Bonus x) => x.Worth).ToList();
	}

	public static List<Bonus> GetMeleeBonuses(Creature creature)
	{
		ResetBonuses();
		CheckEndangered(creature);
		CheckMultikill();
		CheckKillType(KillType.Melee);
		CheckKillsteal(creature);
		Check360();
		CheckFinally();
		CheckImpressive();
		return _bonuses.OrderByDescending((Bonus x) => x.Worth).ToList();
	}

	public static List<Bonus> GetMeleePlayerBonuses(Player player)
	{
		ResetBonuses();
		CheckMultikill();
		CheckKillType(KillType.Melee);
		Check360();
		CheckFinally();
		CheckImpressive();
		return _bonuses.OrderByDescending((Bonus x) => x.Worth).ToList();
	}

	public static List<Bonus> GetRangedBonuses(Creature creature, Vector3 point, int damage)
	{
		ResetBonuses();
		CheckEndangered(creature);
		CheckMultikill();
		Check360();
		CheckScope();
		CheckLongshot(creature.transform);
		CheckHeadshot(creature, point);
		CheckAerials(creature.transform);
		CheckOneShotOneKill(creature, damage);
		CheckPointBlank(creature.transform);
		CheckOverkill(creature.Hp, creature.BossType != BossType.None, damage);
		CheckLastBullet();
		CheckFinally();
		CheckKillsteal(creature);
		CheckNoob();
		CheckImpressive();
		return _bonuses.OrderByDescending((Bonus x) => x.Worth).ToList();
	}

	public static List<Bonus> GetRangedPlayerBonuses(Player hitPlayer, int damage)
	{
		ResetBonuses();
		CheckMultikill();
		Check360();
		CheckScope();
		CheckLongshot(hitPlayer.Transform);
		CheckAerials(hitPlayer.Transform);
		CheckPointBlank(hitPlayer.Transform);
		CheckOverkill(hitPlayer.Vitals.Health, isBoss: false, damage);
		CheckLastBullet();
		CheckFinally();
		CheckNoob();
		CheckImpressive();
		return _bonuses.OrderByDescending((Bonus x) => x.Worth).ToList();
	}

	public static List<Bonus> GetAllBonuses(int round)
	{
		ResetBonuses();
		switch (round)
		{
		case 0:
			_bonuses.Add(new Bonus(LocalizationManager.MeleeLocalized.GetLocalizedString(), 1.05f));
			_bonuses.Add(new Bonus(LocalizationManager.ExplosionLocalized.GetLocalizedString(), 1.5f));
			_bonuses.Add(new Bonus(LocalizationManager.EndangeredLocalized.GetLocalizedString(), 1.25f));
			_bonuses.Add(new Bonus(LocalizationManager.DoubleKillLocalized.GetLocalizedString(), Mathf.Lerp(1f, 1.5f, 0.1f)));
			_bonuses.Add(new Bonus(LocalizationManager.TripleKillLocalized.GetLocalizedString(), Mathf.Lerp(1f, 1.5f, 0.2f)));
			_bonuses.Add(new Bonus(LocalizationManager.QuadraKillLocalized.GetLocalizedString(), Mathf.Lerp(1f, 1.5f, 0.3f)));
			_bonuses.Add(new Bonus(LocalizationManager.PentaKillLocalized.GetLocalizedString(), Mathf.Lerp(1f, 1.5f, 0.4f)));
			_bonuses.Add(new Bonus(LocalizationManager.MultikillLocalized.GetLocalizedString(), Mathf.Lerp(1f, 1.5f, 0.5f)));
			break;
		case 1:
			_bonuses.Add(new Bonus(LocalizationManager.ThreeSixtyLocalized.GetLocalizedString(), 1.5f));
			_bonuses.Add(new Bonus(LocalizationManager.NoScopeLocalized.GetLocalizedString(), 1.2f));
			_bonuses.Add(new Bonus(LocalizationManager.QuickScopeLocalized.GetLocalizedString(), 1.1f));
			_bonuses.Add(new Bonus(LocalizationManager.LongshotLocalized.GetLocalizedString(), 1.3f));
			_bonuses.Add(new Bonus(LocalizationManager.HeadshotLocalized.GetLocalizedString(), 1.25f));
			_bonuses.Add(new Bonus(LocalizationManager.DogfightLocalized.GetLocalizedString(), 1.5f));
			_bonuses.Add(new Bonus(LocalizationManager.AerialLocalized.GetLocalizedString(), 1.25f));
			_bonuses.Add(new Bonus(LocalizationManager.FlyFishingLocalized.GetLocalizedString(), 1.25f));
			break;
		case 2:
			_bonuses.Add(new Bonus(LocalizationManager.OneShotOneKillLocalized.GetLocalizedString(), 1.25f));
			_bonuses.Add(new Bonus(LocalizationManager.PointBlankLocalized.GetLocalizedString(), 1.1f));
			_bonuses.Add(new Bonus(LocalizationManager.OverkillLocalized.GetLocalizedString(), 1.25f));
			_bonuses.Add(new Bonus(LocalizationManager.LastBulletLocalized.GetLocalizedString(), 1.25f));
			_bonuses.Add(new Bonus(LocalizationManager.FinallyLocalized.GetLocalizedString(), 1.1f));
			_bonuses.Add(new Bonus(LocalizationManager.KillstealLocalized.GetLocalizedString(), 1.3f));
			_bonuses.Add(new Bonus(LocalizationManager.NoobKillLocalized.GetLocalizedString(), 1.01f));
			_bonuses.Add(new Bonus(LocalizationManager.ImpressiveLocalized.GetLocalizedString(), 2f));
			break;
		}
		return _bonuses.OrderByDescending((Bonus x) => x.Worth).ToList();
	}

	public static float GetMultiplier(List<Bonus> bonuses)
	{
		float num = 1f;
		foreach (Bonus bonuse in bonuses)
		{
			num *= bonuse.Worth;
		}
		return num;
	}

	private static void CheckKillType(KillType killType)
	{
		string text = "";
		switch (killType)
		{
		case KillType.Melee:
			text = LocalizationManager.MeleeLocalized.GetLocalizedString();
			break;
		case KillType.Explosion:
			text = LocalizationManager.ExplosionLocalized.GetLocalizedString();
			break;
		}
		_bonuses.Add(new Bonus(text, killType switch
		{
			KillType.Melee => 1.05f, 
			KillType.Explosion => 1.5f, 
			_ => 1f, 
		}));
	}

	private static void Check360()
	{
		_did360 = false;
		if (PlayerSkills.RecentlyDid360)
		{
			_did360 = true;
			_bonuses.Add(new Bonus(LocalizationManager.ThreeSixtyLocalized.GetLocalizedString(), 1.5f));
		}
	}

	private static void CheckScope()
	{
		if (PlayerSkills.NoScope)
		{
			AchievementManager.Check360NoscopeAchievement(_did360);
			_bonuses.Add(new Bonus(LocalizationManager.NoScopeLocalized.GetLocalizedString(), 1.2f));
		}
		else if (PlayerSkills.QuickScope)
		{
			_bonuses.Add(new Bonus(LocalizationManager.QuickScopeLocalized.GetLocalizedString(), 1.1f));
		}
	}

	private static void CheckLongshot(Transform target)
	{
		if ((bool)target && Vector3.Distance(target.position, Player.LocalPlayer.CamObject.position) >= 25f)
		{
			_bonuses.Add(new Bonus(LocalizationManager.LongshotLocalized.GetLocalizedString(), 1.3f));
		}
	}

	private static void CheckHeadshot(Creature creature, Vector3 hitPoint)
	{
		if ((bool)creature && creature.transform.InverseTransformPoint(hitPoint).z > creature.HeadPos)
		{
			_bonuses.Add(new Bonus(LocalizationManager.HeadshotLocalized.GetLocalizedString(), 1.25f));
		}
	}

	private static void CheckAerials(Transform target)
	{
		bool flag = !Player.LocalPlayer.Movement.Grounded;
		bool flag2 = !Physics.Raycast(target.position, Vector3.down, 1.5f, GameInfo.LevelLayer);
		if (flag & flag2)
		{
			_bonuses.Add(new Bonus(LocalizationManager.DogfightLocalized.GetLocalizedString(), 1.5f));
		}
		else if (flag)
		{
			_bonuses.Add(new Bonus(LocalizationManager.AerialLocalized.GetLocalizedString(), 1.25f));
		}
		else if (flag2)
		{
			_bonuses.Add(new Bonus(LocalizationManager.FlyFishingLocalized.GetLocalizedString(), 1.25f));
		}
	}

	private static void CheckNoob()
	{
		if (_bonuses.Count == 0)
		{
			_bonuses.Add(new Bonus(LocalizationManager.NoobKillLocalized.GetLocalizedString(), 1.01f));
			AchievementManager.CheckNoobAchievement();
		}
	}

	private static void CheckOneShotOneKill(Creature creature, int damage)
	{
		if ((bool)creature && creature.Hp == creature.MaxHp && damage >= creature.Hp)
		{
			_bonuses.Add(new Bonus(LocalizationManager.OneShotOneKillLocalized.GetLocalizedString(), 1.25f));
		}
	}

	private static void CheckMultikill()
	{
		float worth = Mathf.Lerp(1f, 1.5f, (float)PlayerSkills.Multikill * 0.1f);
		string text = "";
		switch (PlayerSkills.Multikill)
		{
		case 0:
			return;
		case 1:
			text = LocalizationManager.DoubleKillLocalized.GetLocalizedString();
			break;
		case 2:
			text = LocalizationManager.TripleKillLocalized.GetLocalizedString();
			break;
		case 3:
			text = LocalizationManager.QuadraKillLocalized.GetLocalizedString();
			break;
		case 4:
			text = LocalizationManager.PentaKillLocalized.GetLocalizedString();
			break;
		default:
			text = LocalizationManager.MultikillLocalized.GetLocalizedString();
			break;
		}
		_bonuses.Add(new Bonus(text, worth));
	}

	private static void CheckFinally()
	{
		if (PlayerSkills.Finally)
		{
			_bonuses.Add(new Bonus(LocalizationManager.FinallyLocalized.GetLocalizedString(), 1.1f));
		}
	}

	private static void CheckEndangered(Creature creature)
	{
		if (creature.IsEndangered)
		{
			_bonuses.Add(new Bonus(LocalizationManager.EndangeredLocalized.GetLocalizedString(), 1.25f));
		}
	}

	private static void CheckKillsteal(Creature creature)
	{
		if (creature.LocalHitsTaken == 0 && !((float)creature.Hp / (float)creature.MaxHp > 0.3f))
		{
			_bonuses.Add(new Bonus(LocalizationManager.KillstealLocalized.GetLocalizedString(), 1.3f));
		}
	}

	private static void CheckPointBlank(Transform target)
	{
		if (Vector3.Distance(target.position, Player.LocalPlayer.Transform.position) < 2f)
		{
			_bonuses.Add(new Bonus(LocalizationManager.PointBlankLocalized.GetLocalizedString(), 1.1f));
		}
	}

	private static void CheckOverkill(int hp, bool isBoss, int damage)
	{
		if (hp - damage < -70 && !isBoss)
		{
			_bonuses.Add(new Bonus(LocalizationManager.OverkillLocalized.GetLocalizedString(), 1.25f));
		}
	}

	private static void CheckLastBullet()
	{
		if ((bool)Player.LocalPlayer.Holding.HeldItem && (bool)Player.LocalPlayer.Holding.HeldItem.Weapon && Player.LocalPlayer.Holding.HeldItem.Weapon.Ammo == 0)
		{
			_bonuses.Add(new Bonus(LocalizationManager.LastBulletLocalized.GetLocalizedString(), 1.25f));
		}
	}

	private static void CheckImpressive()
	{
		if (_bonuses.Count >= 5)
		{
			_bonuses.Add(new Bonus(LocalizationManager.ImpressiveLocalized.GetLocalizedString(), 2f));
		}
	}
}
