using System;
using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Creature : Item
{
	[Header("Creature")]
	[SerializeField]
	private bool _excludeFromJournal;

	[SerializeField]
	private bool _skipRandomizedWeight;

	[FormerlySerializedAs("_shinyMesh")]
	[SerializeField]
	private Mesh _dripMesh;

	[SerializeField]
	private GameObject _disableOnShiny;

	[SerializeField]
	private GameObject _enableOnShiny;

	[SerializeField]
	private int _maxHp = 10;

	[SerializeField]
	private float _bossHpMultiplier = 0.5f;

	[SerializeField]
	private int _fullnessToRestore = 20;

	[SerializeField]
	private int _hpToRestore = 15;

	[SerializeField]
	protected float _groundCheckDist = 0.25f;

	[SerializeField]
	private float _headPos;

	[SerializeField]
	private bool _smallBloodParticles;

	[SerializeField]
	private bool _isEndangered;

	[SerializeField]
	private float _bloodSizeMulti = 1f;

	[SerializeField]
	[Tooltip("Should this creature not die by falling into water?")]
	private bool _ignoreDeathByWater;

	[Header("Eating")]
	[SerializeField]
	private Vector3 _eatPos;

	[SerializeField]
	private Vector3 _eatRot;

	[Header("BOSS")]
	[SerializeField]
	private BossType _bossType;

	[SerializeField]
	private float _bossTimeInSeconds = 300f;

	[SerializeField]
	[Tooltip("This can be true for some bosses that animate even after death (like the pufferfish")]
	protected bool _hasCustomMovement;

	public readonly SyncVar<int> _hp = new SyncVar<int>(new SyncTypeSettings(0f, Channel.Reliable));

	public readonly SyncVar<bool> _isDrip = new SyncVar<bool>(new SyncTypeSettings(0f, Channel.Reliable));

	protected int _localHp;

	private bool _localIsDead;

	private static HashSet<Creature> _deadCreatures = new HashSet<Creature>();

	protected bool _isGrounded;

	private bool NetworkInitialize___EarlyCreatureAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateCreatureAssembly_002DCSharp_002Edll_Excuted;

	[field: SerializeField]
	public bool UseDrinkSound { get; private set; }

	public bool ExcludeFromJournal => _excludeFromJournal;

	public bool IsDrip => _isDrip.Value;

	public float HeadPos => _headPos;

	public bool IsEndangered => _isEndangered;

	public bool IgnoreDeathByWater => _ignoreDeathByWater;

	public int LocalHitsTaken { get; private set; }

	public int Hp { get; private set; }

	public float LastTimeDamagedByImpact { get; private set; }

	public int MaxHp => (int)((float)_maxHp * ServerSettings.HealthMultiplier);

	public float BossHpMultiplier => _bossHpMultiplier;

	public bool IsDead => Hp <= 0;

	public int FullnessToRestore => _fullnessToRestore;

	public int HpToRestore => _hpToRestore;

	public Vector3 EatPos => _eatPos;

	public Vector3 EatRot => _eatRot;

	public BossType BossType => _bossType;

	public float BossTimeInSeconds => _bossTimeInSeconds;

	public override Mesh Mesh
	{
		get
		{
			if (!_isDrip.Value || !_dripMesh)
			{
				return _mesh;
			}
			return _dripMesh;
		}
	}

	public override Mesh DripMesh => _dripMesh;

	public static IReadOnlyCollection<Creature> DeadCreatures => _deadCreatures;

	protected bool _spawnedByDynamite { get; private set; }

	public static event Action OnCreatureDeath;

	public event Action<int> OnBossTakeDamage;

	public event Action OnBossDeath;

	public event Action OnBossRemoved;

	public static event Action OnInspectedCreature;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Creature_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (_syncedRandomWeight.Value == 1f && !_skipRandomizedWeight)
		{
			_syncedRandomWeight.Value = CreatureUtils.GetWeightWithStandardDeviation(base.ObjectId);
		}
	}

	public override void OnStartClient()
	{
		if (base.IsServerInitialized && !base.Bird && _hp.Value > 0 && !_spawnedByDynamite)
		{
			CreatureManager.Instance.AddAliveCreature(this);
		}
		base.OnStartClient();
		Hp = _hp.Value;
		_localHp = _hp.Value;
		if (_bossType != BossType.None && _hp.Value > 0)
		{
			BossManager.InitializeBossFight(this);
		}
		_canPickUp = IsDead && _bossType != BossType.Boss;
		if (!base.IsServerInitialized && _bossType != BossType.None)
		{
			_rigSync.SetKinematic(kinematic: true);
		}
		if ((bool)_enableOnShiny && (bool)_disableOnShiny)
		{
			_enableOnShiny.SetActive(_isDrip.Value);
			_disableOnShiny.SetActive(!_isDrip.Value);
		}
		else if (_bossType == BossType.None)
		{
			if (!_enableOnShiny && !_excludeFromJournal)
			{
				MonoBehaviour.print("creature doesn't have shiny version to enable");
			}
			if (!_disableOnShiny && !_excludeFromJournal)
			{
				MonoBehaviour.print("creature doesn't have default version to disable");
			}
		}
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		_deadCreatures.Remove(this);
		if (_bossType != BossType.None && _hp.Value > 0)
		{
			OnBossRemoved?.Invoke();
		}
	}

	public void SetDrip()
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			_isDrip.Value = true;
		}
	}

	public void ServerKillOnSpawn()
	{
		_spawnedByDynamite = true;
		_hp.Value = -1;
	}

	public override void InspectInput(InputAction.CallbackContext context)
	{
		string text = "";
		float num = -1f + _syncedRandomWeight.Value;
		text += $"\n{LocalizationManager.WeightLocalized.GetLocalizedString()}: {_weight * _syncedRandomWeight.Value:0.###} {LocalizationManager.KilogramLocalized.GetLocalizedString()}";
		char c = ((num < 0f) ? '-' : '+');
		if (num != 0f)
		{
			text += $" <color=grey>({c}{Mathf.Abs(num):0.##})%</color>";
		}
		text += $"\n{LocalizationManager.KillscoreLocalized.GetLocalizedString()}: {_killScoreMultiplier.Value:0.##}x";
		if (_isEndangered)
		{
			text = text + "\n\n" + LocalizationManager.EndangeredSpeciesLocalized.GetLocalizedString();
		}
		PlayerUI.ShowInspectInfo(this, text);
		OnInspectedCreature?.Invoke();
	}

	public override void LocalHit(Transform hitTransform, Vector3 point, Vector3 dir, Player player, int damage, bool rangedHit, Vector3 force = default(Vector3), bool fromNpc = false)
	{
		if (_bossType == BossType.None)
		{
			base.LocalHit(hitTransform, point, dir, player, damage, rangedHit, force);
		}
		if (_bossType != BossType.None && BossManager.IsImmortal)
		{
			return;
		}
		bool useSmallBloodParticles = !rangedHit || _smallBloodParticles;
		if (IsDead)
		{
			DazedUtils.PlayCreatureHitEffects(point, dir, damage, _bloodSizeMulti, useSmallBloodParticles, IsDead, Hp, player);
		}
		else
		{
			if (!player.Owner.IsLocalClient)
			{
				return;
			}
			if (rangedHit && base.transform.InverseTransformPoint(point).z > HeadPos)
			{
				damage = Mathf.RoundToInt((float)damage * GameInfo.HeadShotDamageMulti);
			}
			Server.Instance.HitCreature(this, player, damage, point, dir);
			AudioManager.PlayGlobalClip("Hitmarker");
			PlayerUI.AddHitMarker(point, _localHp - damage <= 0);
			if (_localHp - damage <= 0 && !_localIsDead && !fromNpc)
			{
				if (_maxHp != 0)
				{
					OnCreatureDeath?.Invoke();
					AchievementManager.CheckFirstCreatureAchievement();
				}
				_localIsDead = true;
				List<Bonus> bonuses = (rangedHit ? KillScoreCalculator.GetRangedBonuses(this, point, damage) : KillScoreCalculator.GetMeleeBonuses(this));
				AchievementManager.CheckKillscoreMultiplierAchievement(bonuses);
				Server.Instance.SetItemMultiplier(this, KillScoreCalculator.GetMultiplier(bonuses));
				Player.LocalPlayer.KillScore.AddKillScore(GetName(), base.TotalWorth, bonuses);
				PlayerSkills.OnKill();
				AchievementManager.CheckDripCreatureAchievement(_isDrip.Value);
			}
			LocalHitsTaken++;
			_localHp -= damage;
			DazedUtils.PlayCreatureHitEffects(point, dir, damage, _bloodSizeMulti, useSmallBloodParticles, IsDead, Hp, player);
		}
	}

	public void ServerChangeHp(int damage)
	{
		if (base.IsServerInitialized && !IsDead && (_bossType == BossType.None || !BossManager.IsImmortal))
		{
			_hp.Value -= damage;
			if (_hp.Value <= 0)
			{
				_hp.Value = 0;
			}
		}
	}

	[ObserversRpc]
	public void ObserverHit(Player playerWhoHit, Vector3 pos, Vector3 dir, int damage)
	{
		RpcWriter___ObserverHit___701300449(playerWhoHit, pos, dir, damage);
	}

	[ObserversRpc]
	public void ObserverExplosionHit(Player player, Vector3 dir, int damage)
	{
		RpcWriter___ObserverExplosionHit___2627372740(player, dir, damage);
	}

	protected virtual void OnDeath()
	{
		if (!_excludeFromJournal)
		{
			if (SaveManager.IsNewFish(base.ID, to: true, _isDrip.Value || _bossType != BossType.None))
			{
				PlayerUI.OnNewFishCaught(this);
			}
			AchievementManager.CheckAllCreaturesAchievement(_isDrip.Value || _bossType != BossType.None);
		}
		_canPickUp = _bossType != BossType.Boss;
		if (!base.IsServerInitialized && _bossType != BossType.None)
		{
			_rigSync.SetKinematic(kinematic: false);
		}
		SetSleepThreshold(isAbleToSleep: true);
		_deadCreatures.Add(this);
		if (_bossType != BossType.None)
		{
			OnBossDeath?.Invoke();
		}
	}

	public void SetRecentImpactHit()
	{
		LastTimeDamagedByImpact = Time.time;
	}

	protected void SetSleepThreshold(bool isAbleToSleep)
	{
		if (isAbleToSleep)
		{
			ItemExtraRigidbody[] extraRigs = _extraRigs;
			for (int i = 0; i < extraRigs.Length; i++)
			{
				extraRigs[i].Rig.sleepThreshold = 0f;
			}
		}
		else
		{
			ItemExtraRigidbody[] extraRigs = _extraRigs;
			for (int i = 0; i < extraRigs.Length; i++)
			{
				extraRigs[i].Rig.sleepThreshold = 0.005f;
			}
		}
	}

	protected virtual void OnHealthChange(int prev, int next, bool asServer)
	{
		if (asServer)
		{
			return;
		}
		if (_bossType != BossType.None)
		{
			OnBossTakeDamage?.Invoke(next);
			if ((bool)base.AttachedRod && (bool)base.AttachedRod.Holder && base.AttachedRod.Holder.Owner.IsLocalClient && next < Hp)
			{
				base.AttachedRod.ReleaseItem(this);
			}
		}
		Hp = next;
		_localHp = next;
		if (next <= 0)
		{
			OnDeath();
		}
	}

	public void HealBoss(int toHeal)
	{
		if (base.IsServerInitialized)
		{
			_hp.Value += toHeal;
		}
	}

	public override void LoadFromSave(SavedItem savedItem)
	{
		base.LoadFromSave(savedItem);
		_syncedRandomWeight.Value = savedItem.Weight;
		_killScoreMultiplier.Value = savedItem.KillScoreMultiplier;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyCreatureAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyCreatureAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_isDrip.InitializeEarly(this, 10u, isSyncObject: false);
			_hp.InitializeEarly(this, 9u, isSyncObject: false);
			RegisterObserversRpc(2u, RpcReader___ObserverHit___701300449);
			RegisterObserversRpc(3u, RpcReader___ObserverExplosionHit___2627372740);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateCreatureAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateCreatureAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_isDrip.InitializeLate();
			_hp.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverHit___701300449(Player playerWhoHit, Vector3 pos, Vector3 dir, int damage)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, playerWhoHit);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteVector3(dir);
		pooledWriter.WriteInt32(damage);
		SendObserversRpc(2u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverHit___701300449(Player P_0, Vector3 P_1, Vector3 P_2, int P_3)
	{
		if (!P_0.Owner.IsLocalClient)
		{
			bool useSmallBloodParticles = _smallBloodParticles;
			if ((bool)P_0 && (bool)P_0.Holding.HeldItem && !P_0.Holding.HeldItem.Weapon)
			{
				useSmallBloodParticles = true;
			}
			DazedUtils.PlayCreatureHitEffects(P_1, P_2, P_3, _bloodSizeMulti, useSmallBloodParticles, IsDead, Hp, P_0);
		}
	}

	private void RpcReader___ObserverHit___701300449(PooledReader PooledReader0, Channel channel)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3 vector2 = PooledReader0.ReadVector3();
		int num = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverHit___701300449(player, vector, vector2, num);
		}
	}

	private void RpcWriter___ObserverExplosionHit___2627372740(Player player, Vector3 dir, int damage)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteVector3(dir);
		pooledWriter.WriteInt32(damage);
		SendObserversRpc(3u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverExplosionHit___2627372740(Player P_0, Vector3 P_1, int P_2)
	{
		if ((bool)P_0 && P_0.Owner.IsLocalClient)
		{
			AudioManager.PlayGlobalClip("Hitmarker");
			PlayerUI.AddHitMarker(base.transform.position, Hp - P_2 <= 0);
			PlayerUI.AddDamageNumber(base.transform.position, P_2, Hp - P_2 <= 0);
		}
		DazedUtils.PlayCreatureHitEffects(base.transform.position, P_1, P_2, _bloodSizeMulti, _smallBloodParticles, IsDead, Hp, P_0);
	}

	private void RpcReader___ObserverExplosionHit___2627372740(PooledReader PooledReader0, Channel channel)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		int num = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverExplosionHit___2627372740(player, vector, num);
		}
	}

	protected virtual void Awake_UserLogic_Creature_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_hp.OnChange += OnHealthChange;
		_creature = this;
		if (_hp.Value == 0 && !_spawnedByDynamite)
		{
			_hp.Value = ((BossType == BossType.None) ? MaxHp : BossManager.GetBossMaxHp(MaxHp, _bossHpMultiplier));
		}
		_canPickUp = false;
		SetSleepThreshold(IsDead);
	}
}
