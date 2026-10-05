using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

public class PlayerVitals : NetworkBehaviour
{
	[SerializeField]
	private Player _player;

	[Header("Testing")]
	[SerializeField]
	private int _testingDamage = 15;

	[SerializeField]
	private int _testingFullnessLost = 15;

	[SerializeField]
	private int _testingHeal = 40;

	[SerializeField]
	private int _testingFullness = 25;

	[Header("Starting Vitals")]
	[SerializeField]
	private int _startHealth = 100;

	[SerializeField]
	private int _startFullness = 100;

	[Space]
	[SerializeField]
	[Tooltip("Damage multiplier for players to take when receiving damage to not get oneshot by strong weapons. 1 = 100 % of damage")]
	private float _playerDamageMultiplier = 0.25f;

	[Header("Losing fullness/health")]
	[SerializeField]
	[Tooltip("How many ticks should player be full after eating, before starting to tick down on fullness again")]
	private uint _ticksOfFullnessAfterEating = 1500u;

	[SerializeField]
	[Tooltip("How many ticks to lose _fullnessLostPerTickInterval amount of fullness when player hasn't eaten in _ticksOfFullnessAfterEating ticks")]
	private uint _loseFullnessTickInterval = 300u;

	[SerializeField]
	[Range(0f, 10f)]
	private uint _fullnessLostPerTickInterval = 1u;

	[SerializeField]
	private uint _loseHealthHungerTickInterval = 150u;

	[Header("Resurrection")]
	[SerializeField]
	private int _healthOnRes = 25;

	[SerializeField]
	private int _fullnessOnRes = 10;

	[SerializeField]
	private int _healthLostPerHungerTick = 5;

	[Header("Gaining health")]
	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("1 = needs full fullness, 0 = doesnt need fullness at all for hp regen")]
	private float _minFullnessForHealthRegen = 0.8f;

	[SerializeField]
	private uint _ticksBeforeRegenAfterTakingDamage = 250u;

	[SerializeField]
	private uint _gainHealthTickInterval = 100u;

	[SerializeField]
	private int _healthGainedPerTickInterval = 5;

	[SerializeField]
	private int _fullnessLostPerGainedHealth = 5;

	[Header("Poison")]
	[SerializeField]
	private int _poisonDamagePerTickInterval = 5;

	[SerializeField]
	private uint _poisonTickInterval = 100u;

	[SerializeField]
	private int _poisonLostPerTickInterval = 20;

	[Header("Fire")]
	[SerializeField]
	private int _fireDamagePerTickInterval = 10;

	[SerializeField]
	private uint _fireTickInterval = 50u;

	[SerializeField]
	private int _fireLostPerTickInterval = 25;

	[Header("Invulnerability")]
	[SerializeField]
	private float _invulnerabilityAfterDamage = 0.25f;

	[SerializeField]
	private float _invulnerabilityAfterResurrect = 2f;

	public readonly SyncVar<int> _syncedHealth = new SyncVar<int>();

	public readonly SyncVar<int> _syncedFullness = new SyncVar<int>();

	public readonly SyncVar<int> _syncedPoison = new SyncVar<int>();

	public readonly SyncVar<int> _syncedFire = new SyncVar<int>();

	private int _prevHealth;

	private int _prevFullness;

	private int _prevPoison;

	private int _prevFire;

	private int _localHp;

	private bool _localIsDead;

	private float _invulnerableUntil;

	private const int _maxHealth = 100;

	private const int _maxFullness = 100;

	private const int _maxPoison = 100;

	private const int _maxFire = 100;

	private uint _lastTickEaten;

	private uint _lastTickLosingFullness;

	private uint _lastTickGainedHealth;

	private uint _lastTickTakenDamageFromHunger;

	private uint _lastTickTakenDamage;

	private uint _lastTickPoisoned;

	private uint _lastTickFired;

	private SavedPlayer _toLoadFrom;

	private bool NetworkInitialize___EarlyPlayerVitalsAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerVitalsAssembly_002DCSharp_002Edll_Excuted;

	public int Health => _syncedHealth.Value;

	public int Fullness => _syncedFullness.Value;

	public float HealthPercent => (float)_prevHealth / 100f;

	private float _fullnessPercent => (float)_prevFullness / 100f;

	public float PoisonPercent => (float)_prevPoison / 100f;

	public float FirePercent => (float)_prevFire / 100f;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_PlayerVitals_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		base.TimeManager.OnTick += TickUpdate;
		_lastTickEaten = base.TimeManager.Tick;
		if (_toLoadFrom != null)
		{
			LoadFromSave();
		}
		else
		{
			ServerResetVitals();
		}
	}

	public override void OnStartClient()
	{
		_localHp = _syncedHealth.Value;
	}

	public override void OnStopServer()
	{
		base.TimeManager.OnTick -= TickUpdate;
	}

	private void Update()
	{
		if (ClientSettings.CheatsEnabled && base.IsServerInitialized && (bool)Player.LocalPlayer && !Player.LocalPlayer.BlockInputs)
		{
			if (Input.GetKeyDown(KeyCode.G))
			{
				TakeDamage(_testingDamage);
				LowerFullness(_testingFullnessLost);
			}
			if (Input.GetKeyDown(KeyCode.H))
			{
				Heal(_testingHeal);
				RestoreFullness(_testingFullness);
			}
		}
	}

	private void TickUpdate()
	{
		LowerFullnessTick();
		DamageFromFullness();
		Regenerate();
		PoisonTick();
		FireTick();
	}

	private void DamageFromFullness()
	{
		if (_prevFullness <= 0 && base.TimeManager.Tick - _lastTickTakenDamageFromHunger >= _loseHealthHungerTickInterval)
		{
			_lastTickTakenDamageFromHunger = base.TimeManager.Tick;
			TakeDamage(_healthLostPerHungerTick);
		}
	}

	private void LowerFullnessTick()
	{
		if (base.TimeManager.Tick - _lastTickEaten >= _ticksOfFullnessAfterEating && !_player.IsAfk && base.TimeManager.Tick - _lastTickLosingFullness >= _loseFullnessTickInterval)
		{
			LowerFullness((int)_fullnessLostPerTickInterval);
		}
	}

	private void LowerFullness(int toLose)
	{
		if (_syncedFullness.Value > 0)
		{
			int value = Mathf.Clamp(_syncedFullness.Value - toLose, 0, 100);
			_syncedFullness.Value = value;
			_lastTickLosingFullness = base.TimeManager.Tick;
		}
	}

	private void Regenerate()
	{
		if (!_player.Dying.IsDead && !(_fullnessPercent < _minFullnessForHealthRegen) && base.TimeManager.Tick - _lastTickTakenDamage >= _ticksBeforeRegenAfterTakingDamage && base.TimeManager.Tick - _lastTickGainedHealth >= _gainHealthTickInterval)
		{
			if (_syncedHealth.Value >= 100)
			{
				_lastTickGainedHealth = base.TimeManager.Tick;
				return;
			}
			Heal(_healthGainedPerTickInterval);
			LowerFullness(_fullnessLostPerGainedHealth);
		}
	}

	private void PoisonTick()
	{
		if (_syncedPoison.Value > 0 && base.TimeManager.Tick - _lastTickPoisoned >= _poisonTickInterval)
		{
			_lastTickPoisoned = base.TimeManager.Tick;
			int value = Mathf.Clamp(_syncedPoison.Value - _poisonLostPerTickInterval, 0, int.MaxValue);
			_syncedPoison.Value = value;
			TakeDamage(BossManager.GetBossDamage(_poisonDamagePerTickInterval, 0.1f), Vector3.zero, Vector3.zero, ignoreInvulnerability: true);
		}
	}

	private void FireTick()
	{
		if (_syncedFire.Value > 0 && base.TimeManager.Tick - _lastTickFired >= _fireTickInterval)
		{
			_lastTickFired = base.TimeManager.Tick;
			int value = Mathf.Clamp(_syncedFire.Value - _fireLostPerTickInterval, 0, int.MaxValue);
			_syncedFire.Value = value;
			TakeDamage(BossManager.GetBossDamage(_fireDamagePerTickInterval, 0.1f), Vector3.zero, Vector3.zero, ignoreInvulnerability: true);
		}
	}

	public void ApplyNewPoison()
	{
		if (base.IsServerInitialized)
		{
			_syncedPoison.Value = 100;
		}
	}

	public void ApplyNewFire()
	{
		if (base.IsServerInitialized)
		{
			_syncedFire.Value = 100;
		}
	}

	public void TakeDamage(int amount, Vector3 pos = default(Vector3), Vector3 force = default(Vector3), bool ignoreInvulnerability = false)
	{
		if (base.IsServerInitialized && !PlayerManager.InGodMode && !EndGameEffects.IsShowingEndGame && (!_player.IsAfk || _player.AfkFromPause) && (!(Time.time < _invulnerableUntil) || ignoreInvulnerability))
		{
			_invulnerableUntil = Time.time + _invulnerabilityAfterDamage;
			int value = (int)((float)amount * ServerSettings.DamageMultiplier);
			int value2 = _syncedHealth.Value - Mathf.Abs(value);
			value2 = Mathf.Clamp(value2, 0, 100);
			_lastTickTakenDamage = base.TimeManager.Tick;
			_syncedHealth.Value = value2;
			if (value2 <= 0 && !_player.Dying.DeadPlayer)
			{
				_player.Dying.ServerDie(force);
			}
		}
	}

	public void ServerResetVitals()
	{
		_syncedHealth.Value = Mathf.Clamp(_startHealth, 0, 100);
		_syncedFullness.Value = Mathf.Clamp(_startFullness, 0, 100);
		_syncedPoison.Value = 0;
	}

	public void LocalHit(Vector3 point, Vector3 dir, Player playerWhoHit, int damage, bool rangedHit, Vector3 force, bool fromNpc = false)
	{
		if (!fromNpc)
		{
			damage = (int)((float)damage * _playerDamageMultiplier);
		}
		int num = (int)((float)damage * ServerSettings.DamageMultiplier);
		if (!ServerSettings.UseFriendlyFire && !fromNpc)
		{
			DazedUtils.PlayPlayerHitEffects(point, dir, num, _player, null, Health);
		}
		else if (playerWhoHit.Owner.IsLocalClient)
		{
			Server.Instance.HitPlayer(_player, damage, force, point, 2, playerWhoHit);
			AudioManager.PlayGlobalClip("Hitmarker");
			PlayerUI.AddHitMarker(point, Health - damage <= 0);
			if (_localHp - num <= 0 && !_localIsDead && !fromNpc)
			{
				_localIsDead = true;
				List<Bonus> bonuses = (rangedHit ? KillScoreCalculator.GetRangedPlayerBonuses(_player, damage) : KillScoreCalculator.GetMeleePlayerBonuses(_player));
				Player.LocalPlayer.KillScore.AddKillScore(_player.SteamName, 100, bonuses);
				PlayerSkills.OnKill();
			}
			_localHp -= num;
			DazedUtils.PlayPlayerHitEffects(point, dir, num, _player, playerWhoHit, Health);
		}
	}

	[ObserversRpc]
	public void ObserverHit(Player playerWhoHit, Vector3 pos, Vector3 dir, int damage, DamageType type)
	{
		RpcWriter___ObserverHit___2388800966(playerWhoHit, pos, dir, damage, type);
	}

	public void Heal(int amount)
	{
		if (base.IsServerInitialized)
		{
			int value = _syncedHealth.Value + Mathf.Abs(amount);
			value = Mathf.Clamp(value, 0, 100);
			_lastTickGainedHealth = base.TimeManager.Tick;
			_syncedHealth.Value = value;
		}
	}

	public void OnResurrect()
	{
		if (base.IsServerInitialized)
		{
			_invulnerableUntil = Time.time + _invulnerabilityAfterResurrect;
			Heal(_healthOnRes);
			if (_syncedFullness.Value <= 0)
			{
				RestoreFullness(_fullnessOnRes);
			}
		}
	}

	public void RestoreFullness(int amount)
	{
		if (base.IsServerInitialized)
		{
			int value = _syncedFullness.Value + amount;
			value = Mathf.Clamp(value, 0, 100);
			_syncedFullness.Value = value;
			_lastTickEaten = base.TimeManager.Tick;
		}
	}

	private void OnHealthChange(int prev, int next, bool asServer)
	{
		if (asServer)
		{
			return;
		}
		bool flag = next < _prevHealth;
		bool respawned = next == _startHealth;
		_prevHealth = next;
		_localHp = next;
		_localIsDead = next <= 0;
		if (!_player.Dying.IsDead && _prevHealth <= 0)
		{
			if (base.Owner.IsLocalClient)
			{
				_player.Dying.LocalDie();
			}
			else
			{
				_player.Dying.DeathEffects();
			}
		}
		else if (_player.Dying.IsDead && _prevHealth > 0)
		{
			if (base.Owner.IsLocalClient)
			{
				_player.Dying.LocalResurrect();
			}
			else
			{
				_player.Dying.ResurrectEffect(respawned);
			}
		}
		if (base.Owner.IsLocalClient)
		{
			if (next <= 0)
			{
				_player.Effects.OnDeath();
			}
			if (flag)
			{
				_player.Effects.OnTakeDamage();
			}
			PlayerUI.SetPlayerHp(HealthPercent);
		}
	}

	private void OnFullnessChange(int prev, int next, bool asServer)
	{
		if (asServer)
		{
			return;
		}
		bool flag = next > _prevFullness && _prevFullness >= 0;
		_prevFullness = next;
		if (base.Owner.IsLocalClient)
		{
			if (flag)
			{
				_player.Effects.OnRestoredFullness();
			}
			PlayerUI.SetPlayerFullness(_fullnessPercent);
		}
	}

	private void OnPoisonChange(int prev, int next, bool asServer)
	{
		if (!asServer)
		{
			_prevPoison = next;
			if (base.Owner.IsLocalClient)
			{
				PlayerUI.SetPlayerPoison(PoisonPercent);
			}
		}
	}

	private void OnFireChange(int prev, int next, bool asServer)
	{
		if (asServer)
		{
			return;
		}
		_prevFire = next;
		if (next == 100)
		{
			if (base.IsOwner)
			{
				_player.Movement.Jump();
			}
			AudioManager.PlayRandomPlayerClip("LavaPoof_V", 1, 6, _player, variation: false, AudioDistance.Short);
		}
		if (base.Owner.IsLocalClient)
		{
			PlayerUI.SetPlayerFire(FirePercent);
		}
	}

	public void SetSavedPlayer(SavedPlayer savedPlayer)
	{
		_toLoadFrom = savedPlayer;
	}

	private void LoadFromSave()
	{
		_syncedHealth.Value = Mathf.Clamp(_toLoadFrom.Health, 0, 100);
		_syncedFullness.Value = Mathf.Clamp(_toLoadFrom.Fullness, 0, 100);
		_syncedPoison.Value = 0;
		_syncedFire.Value = 0;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerVitalsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerVitalsAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_syncedFire.InitializeEarly(this, 3u, isSyncObject: false);
			_syncedPoison.InitializeEarly(this, 2u, isSyncObject: false);
			_syncedFullness.InitializeEarly(this, 1u, isSyncObject: false);
			_syncedHealth.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterObserversRpc(0u, RpcReader___ObserverHit___2388800966);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerVitalsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerVitalsAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_syncedFire.InitializeLate();
			_syncedPoison.InitializeLate();
			_syncedFullness.InitializeLate();
			_syncedHealth.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverHit___2388800966(Player playerWhoHit, Vector3 pos, Vector3 dir, int damage, DamageType type)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializersPooled.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, playerWhoHit);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteVector3(dir);
		pooledWriter.WriteInt32(damage);
		GameTypeSerializersPooled.GWrite___DamageTypeFishNet_002ESerializing_002EGenerated(pooledWriter, type);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverHit___2388800966(Player P_0, Vector3 P_1, Vector3 P_2, int P_3, DamageType P_4)
	{
		if (!P_0 || !P_0.Owner.IsLocalClient)
		{
			DazedUtils.PlayPlayerHitEffects(P_1, P_2, (int)((float)P_3 * ServerSettings.DamageMultiplier), _player, P_0, Health, P_4);
		}
	}

	private void RpcReader___ObserverHit___2388800966(PooledReader PooledReader0, Channel channel)
	{
		Player player = GameTypeSerializersPooled.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3 vector2 = PooledReader0.ReadVector3();
		int num = PooledReader0.ReadInt32();
		DamageType damageType = GameTypeSerializersPooled.GRead___DamageTypeFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverHit___2388800966(player, vector, vector2, num, damageType);
		}
	}

	private void Awake_UserLogic_PlayerVitals_Assembly_002DCSharp_002Edll()
	{
		_syncedHealth.OnChange += OnHealthChange;
		_syncedFullness.OnChange += OnFullnessChange;
		_syncedPoison.OnChange += OnPoisonChange;
		_syncedFire.OnChange += OnFireChange;
	}
}
