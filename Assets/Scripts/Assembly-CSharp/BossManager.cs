using System;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

public class BossManager : NetworkBehaviour
{
	private static BossManager _instance;

	[SerializeField]
	private float _bossExplosionScreenShake = 1500f;

	[SerializeField]
	private float _trophyVel = 15f;

	[SerializeField]
	private float _trophyAngVel = 5f;

	[Header("Boss Time")]
	[SerializeField]
	private int _totalBossTimeInSeconds = 15;

	public readonly SyncVar<int> _bossMaxHp = new SyncVar<int>();

	public readonly SyncVar<bool> _isImmortal = new SyncVar<bool>();

	public readonly SyncVar<uint> _bossSpawnTick = new SyncVar<uint>();

	private static float _timeOfStartBoss;

	private bool NetworkInitialize___EarlyBossManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateBossManagerAssembly_002DCSharp_002Edll_Excuted;

	public static Creature Boss { get; private set; }

	public static bool IsImmortal { get; private set; }

	public static int BossMaxHp { get; private set; }

	public static uint BossSpawnTick { get; private set; }

	public static uint BossLeavesTick { get; private set; }

	public static uint BossTotalTimeInTicks { get; private set; }

	public static event Action OnGlobalBossSpawn;

	public static event Action OnGlobalBossDeath;

	public static event Action OnGlobalBossDespawn;

	public static event Action OnBossMaxHpChanged;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_BossManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		base.TimeManager.OnTick += TickUpdate;
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		base.TimeManager.OnTick -= TickUpdate;
	}

	private void TickUpdate()
	{
		if ((bool)Boss && BossSpawnTick == _bossSpawnTick.Value && Mathf.InverseLerp(BossLeavesTick, BossSpawnTick, InstanceFinder.TimeManager.Tick) <= 0f)
		{
			Boss.DestroyItem(0);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		IsImmortal = _isImmortal.Value;
		BossMaxHp = _bossMaxHp.Value;
		OnBossSpawnTimeChange(0u, _bossSpawnTick.Value, asServer: false);
	}

	public static void InitializeBossFight(Creature creature)
	{
		if ((bool)_instance && !Boss)
		{
			Boss = creature;
			OnGlobalBossSpawn?.Invoke();
			Boss.OnBossTakeDamage += OnBossTakeDamage;
			Boss.OnBossDeath += OnBossDeath;
			Boss.OnBossRemoved += OnBossRemoved;
			PlayerUI.ToggleBossUI(to: true);
			if (Boss.BossType == BossType.Boss)
			{
				MusicManager.PlayMusic("BossFight1", "BossFightIntro");
			}
			else
			{
				MusicManager.PlayMusic("BossFightMini", "BossFightMini_Intro6s", 6f);
			}
			if (_instance.IsServerInitialized)
			{
				ToggleImmortal(to: false);
				_instance._bossMaxHp.Value = GetBossMaxHp(creature.MaxHp, creature.BossHpMultiplier);
				_instance._bossSpawnTick.Value = InstanceFinder.TimeManager.Tick;
				_timeOfStartBoss = Time.time;
			}
		}
	}

	public static void SpawnBossTrophy(Item trophy, Item meat, Vector3 pos)
	{
		if (!_instance || !trophy)
		{
			return;
		}
		Item item = UnityEngine.Object.Instantiate(trophy, pos, Quaternion.identity);
		_instance.Spawn(item.gameObject);
		item.RigidbodySync.StartSimulateLocal();
		item.Rig.linearVelocity = Vector3.up * _instance._trophyVel;
		item.Rig.angularVelocity = Vector3.one * _instance._trophyAngVel;
		if ((bool)meat)
		{
			for (int i = 0; i < PlayerManager.Players.Count + 2; i++)
			{
				Item item2 = UnityEngine.Object.Instantiate(meat, pos, Quaternion.identity);
				_instance.Spawn(item2.gameObject);
				item2.RigidbodySync.StartSimulateLocal();
				item2.Rig.linearVelocity = Vector3.up * (_instance._trophyVel * 0.5f);
				item2.Rig.angularVelocity = Vector3.one * (_instance._trophyAngVel * 0.5f);
			}
		}
	}

	public static void BossExplosion(Vector3 pos, bool isFinalBoss = false)
	{
		if (!_instance)
		{
			return;
		}
		VFXManager.Play("BossDeath", pos);
		if (isFinalBoss)
		{
			AudioManager.PlayClipAt("LavaWhaleDeathExplosion", pos, variation: false, AudioDistance.Long, 1.75f);
		}
		else
		{
			AudioManager.PlayClipAt("Death", pos, variation: false, AudioDistance.Medium, 1.25f);
			AudioManager.PlayClipAt("Explosion", pos, variation: false, AudioDistance.Long, 0.45f);
		}
		Player.LocalPlayer.ScreenShake.ShakeAt(pos, 1, _instance._bossExplosionScreenShake);
		for (int i = 0; i < 10; i++)
		{
			Vector3 direction = Vector3.down + UnityEngine.Random.insideUnitSphere * 2f;
			if (Physics.Raycast(pos, direction, out var hitInfo, 8f, GameInfo.LevelLayer))
			{
				DecalManager.SpawnDecal("BloodDecal" + UnityEngine.Random.Range(1, 4), hitInfo.point, hitInfo.normal, GameInfo.BloodDecalSizeMulti.Evaluate(50f) * UnityEngine.Random.Range(1f, 1.5f), 1f);
			}
		}
	}

	public static int GetBossMaxHp(int maxHp, float multiplier = 0.5f)
	{
		return maxHp + (int)((float)(maxHp * (PlayerManager.Players.Count - 1)) * multiplier);
	}

	public static int GetBossDamage(int baseDamage, float multiplier = 0.2f)
	{
		return baseDamage + (int)((float)(baseDamage * (PlayerManager.Players.Count - 1)) * multiplier);
	}

	private static void OnBossTakeDamage(int hp)
	{
		if ((bool)Boss)
		{
			PlayerUI.UpdateBossHp(hp);
		}
	}

	private static void OnBossDeath()
	{
		if ((bool)_instance && (bool)Boss)
		{
			if (_instance.IsServerInitialized && Time.time - _timeOfStartBoss <= 10f && Boss.BossType == BossType.Boss)
			{
				_instance.ObserverFastBossAchievement();
			}
			if (_instance.IsServerInitialized)
			{
				_instance._bossSpawnTick.Value = 0u;
			}
			OnGlobalBossDeath?.Invoke();
			if (Boss.BossType == BossType.Boss)
			{
				MusicManager.StopMusic("BossFight1", "BossFightOutro");
			}
			else
			{
				MusicManager.StopMusic("BossFightMini", "BossFightMini_Outro");
			}
			Boss = null;
			PlayerUI.ToggleBossUI(to: false);
		}
	}

	public static void ToggleImmortal(bool to)
	{
		if ((bool)_instance && _instance.IsServerInitialized)
		{
			_instance._isImmortal.Value = to;
		}
	}

	private void UpdateBossMaxHp()
	{
		if ((bool)Boss)
		{
			int bossMaxHp = GetBossMaxHp(Boss.MaxHp, Boss.BossHpMultiplier);
			if (_bossMaxHp.Value <= bossMaxHp)
			{
				int toHeal = bossMaxHp - _bossMaxHp.Value;
				_bossMaxHp.Value = bossMaxHp;
				Boss.HealBoss(toHeal);
			}
		}
	}

	private static void OnBossRemoved()
	{
		if (_instance.IsServerInitialized)
		{
			_instance._bossSpawnTick.Value = 0u;
		}
		MusicManager.StopMusic("BossFight1");
		if (Boss.BossType == BossType.Boss)
		{
			MusicManager.StopMusic("BossFight1");
		}
		else
		{
			MusicManager.StopMusic("BossFightMini");
		}
		PlayerUI.ToggleBossUI(to: false);
		Boss = null;
		OnGlobalBossDespawn?.Invoke();
	}

	private void OnBossMaxHpChange(int prev, int next, bool asServer)
	{
		if (!asServer)
		{
			BossMaxHp = next;
			if ((bool)Boss)
			{
				PlayerUI.UpdateBossHp(Boss.Hp);
			}
			OnBossMaxHpChanged?.Invoke();
		}
	}

	private void OnIsImmportalChange(bool prev, bool next, bool asServer)
	{
		IsImmortal = next;
	}

	private void OnPlayerCountChange()
	{
		if (base.IsServerInitialized && (bool)Boss)
		{
			if (PlayerManager.AlivePlayers.Count <= 0)
			{
				Boss.DestroyItem(0);
			}
			else
			{
				UpdateBossMaxHp();
			}
		}
	}

	private void OnBossSpawnTimeChange(uint prev, uint next, bool asServer)
	{
		if ((bool)Boss)
		{
			BossTotalTimeInTicks = (uint)(Boss.BossTimeInSeconds * (float)(int)base.TimeManager.TickRate);
		}
		else
		{
			BossTotalTimeInTicks = (uint)(_totalBossTimeInSeconds * base.TimeManager.TickRate);
		}
		BossSpawnTick = next;
		BossLeavesTick = next + BossTotalTimeInTicks;
	}

	[ObserversRpc]
	private void ObserverFastBossAchievement()
	{
		RpcWriter___ObserverFastBossAchievement___2166136261();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyBossManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyBossManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_bossSpawnTick.InitializeEarly(this, 2u, isSyncObject: false);
			_isImmortal.InitializeEarly(this, 1u, isSyncObject: false);
			_bossMaxHp.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterObserversRpc(0u, RpcReader___ObserverFastBossAchievement___2166136261);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateBossManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateBossManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_bossSpawnTick.InitializeLate();
			_isImmortal.InitializeLate();
			_bossMaxHp.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverFastBossAchievement___2166136261()
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverFastBossAchievement___2166136261()
	{
		AchievementManager.CheckFastBossAchievement();
	}

	private void RpcReader___ObserverFastBossAchievement___2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverFastBossAchievement___2166136261();
		}
	}

	private void Awake_UserLogic_BossManager_Assembly_002DCSharp_002Edll()
	{
		Setter.SetSingleInstance(ref _instance, this);
		_bossMaxHp.OnChange += OnBossMaxHpChange;
		_isImmortal.OnChange += OnIsImmportalChange;
		_bossSpawnTick.OnChange += OnBossSpawnTimeChange;
		PlayerManager.OnPlayerAmountChange += OnPlayerCountChange;
	}
}
