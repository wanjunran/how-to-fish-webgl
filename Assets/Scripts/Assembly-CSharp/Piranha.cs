using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Serialization;

public class Piranha : AttackingFish
{
	[Header("Piranha Stats (2 = second phase stat")]
	[SerializeField]
	private Transform _summonSpawnPos;

	[SerializeField]
	private int _summonsPerSpawn = 3;

	[SerializeField]
	private int _summonsPerSpawn2 = 6;

	[SerializeField]
	private float _summonSpawnDelay = 10f;

	[SerializeField]
	private float _summonSpawnDelay2;

	[SerializeField]
	private float _perSummonSpawnDelay = 0.15f;

	[SerializeField]
	private int _maxSummons = 10;

	[SerializeField]
	private int _maxSummons2 = 20;

	[SerializeField]
	private float _summonSpawnForce = 5f;

	[SerializeField]
	private Creature _summonCreature;

	[FormerlySerializedAs("_totalAnimTime")]
	[Header("Animation")]
	[SerializeField]
	private int _summonsToSpawnInAnimation = 12;

	[SerializeField]
	private float _beforeSummoningAnimTime = 1f;

	[SerializeField]
	private float _totalDeathAnimTime = 1.5f;

	[SerializeField]
	private float _animUpVel = 1f;

	[SerializeField]
	private float _animAngVel = 25f;

	[SerializeField]
	private float _animRandVel = 2f;

	[Header("On Death")]
	[SerializeField]
	private Item _spawnOnDeath;

	[SerializeField]
	private Item _meat;

	private List<Creature> _aliveSummons = new List<Creature>();

	public readonly SyncVar<bool> _inSecondPhase = new SyncVar<bool>();

	private float _curAnimTime;

	private bool _isAnimating;

	private int _animationSummonsSpawned;

	private float _curSummonAnimTime;

	private bool NetworkInitialize___EarlyPiranhaAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePiranhaAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Piranha_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		StartCoroutine(SpawnSummons());
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		if (base.IsDead && base.IsDead)
		{
			BossManager.BossExplosion(_rig.worldCenterOfMass);
		}
	}

	private IEnumerator SpawnSummons()
	{
		while (true)
		{
			yield return new WaitForSeconds(_inSecondPhase.Value ? _summonSpawnDelay2 : _summonSpawnDelay);
			if ((bool)base.AttachedRod || _isAnimating)
			{
				continue;
			}
			if (base.IsDead)
			{
				break;
			}
			for (int num = _aliveSummons.Count - 1; num >= 0; num--)
			{
				if (!_aliveSummons[num] || _aliveSummons[num].IsDead)
				{
					_aliveSummons.RemoveAt(num);
				}
			}
			int num2 = (_inSecondPhase.Value ? _maxSummons2 : _maxSummons);
			num2 += PlayerManager.Players.Count - 1;
			if (_aliveSummons.Count >= num2)
			{
				continue;
			}
			int summons = (_inSecondPhase.Value ? _summonsPerSpawn2 : _summonsPerSpawn);
			if (PlayerManager.Players.Count > 2)
			{
				summons++;
			}
			for (int i = 0; i < summons; i++)
			{
				if (_aliveSummons.Count < _maxSummons)
				{
					SummonCreature();
					yield return new WaitForSeconds(_perSummonSpawnDelay);
				}
			}
		}
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverSummonSpawnEffects()
	{
		RpcWriter___ObserverSummonSpawnEffects___2166136261();
	}

	private void SummonSpawnEffects()
	{
		ParticleManager.Play("BloodDirectional", _summonSpawnPos.position, base.transform.forward);
		AudioManager.PlayRandomClipAt("PiranhaVomit_V", 1, 2, base.transform.position, variation: false, AudioDistance.Short);
	}

	protected override void UpdateMovement()
	{
		if (_isAnimating)
		{
			Animate();
		}
		else
		{
			base.UpdateMovement();
		}
	}

	protected override void InitializeMovement()
	{
		if (!_isAnimating)
		{
			base.InitializeMovement();
		}
	}

	protected override void OnHealthChange(int prev, int next, bool asServer)
	{
		float num = (float)next / (float)BossManager.BossMaxHp;
		base.OnHealthChange(prev, next, asServer);
		if (asServer)
		{
			if (!_inSecondPhase.Value && num <= 0.5f)
			{
				_inSecondPhase.Value = true;
				StartAnimating();
			}
			else if (next <= 0 && !_isAnimating)
			{
				StartAnimating();
			}
		}
	}

	private void StartAnimating()
	{
		_curAnimTime = 0f;
		_isAnimating = true;
	}

	private void Animate()
	{
		Vector3 linearVelocity = Vector3.up * _animUpVel;
		linearVelocity += Random.insideUnitSphere.normalized * _animRandVel;
		_rig.linearVelocity = linearVelocity;
		_rig.angularVelocity = base.transform.forward * _animAngVel;
		_curAnimTime += Time.fixedDeltaTime;
		float num = (base.IsDead ? _totalDeathAnimTime : _beforeSummoningAnimTime);
		if (!(_curAnimTime >= num))
		{
			return;
		}
		if (!base.IsDead)
		{
			if (_animationSummonsSpawned >= _summonsToSpawnInAnimation)
			{
				BossManager.ToggleImmortal(to: false);
				_isAnimating = false;
			}
			_curSummonAnimTime += Time.fixedDeltaTime;
			if (_curSummonAnimTime >= _perSummonSpawnDelay)
			{
				_curSummonAnimTime = 0f;
				SummonCreature();
				_animationSummonsSpawned++;
			}
		}
		else
		{
			BossManager.SpawnBossTrophy(_spawnOnDeath, _meat, _rig.worldCenterOfMass);
			Despawn(base.gameObject);
		}
	}

	private void OnSecondPhaseChange(bool prev, bool next, bool asServer)
	{
		if (asServer)
		{
			_isAnimating = true;
			BossManager.ToggleImmortal(to: true);
		}
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		AchievementManager.CheckBossAchievement(1);
	}

	private void SummonCreature()
	{
		Creature creature = Object.Instantiate(_summonCreature, _summonSpawnPos.position, base.transform.rotation);
		Spawn(creature.gameObject);
		Vector3 force = base.transform.forward * _summonSpawnForce;
		creature.RigidbodySync.StartSimulateLocal();
		creature.Rig.AddForce(force);
		_aliveSummons.Add(creature);
		ObserverSummonSpawnEffects();
		SummonSpawnEffects();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPiranhaAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPiranhaAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_inSecondPhase.InitializeEarly(this, 11u, isSyncObject: false);
			RegisterObserversRpc(4u, RpcReader___ObserverSummonSpawnEffects___2166136261);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePiranhaAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePiranhaAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_inSecondPhase.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverSummonSpawnEffects___2166136261()
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendObserversRpc(4u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverSummonSpawnEffects___2166136261()
	{
		SummonSpawnEffects();
	}

	private void RpcReader___ObserverSummonSpawnEffects___2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverSummonSpawnEffects___2166136261();
		}
	}

	protected virtual void Awake_UserLogic_Piranha_Assembly_002DCSharp_002Edll()
	{
		((Fish)this).Awake();
		_inSecondPhase.OnChange += OnSecondPhaseChange;
	}
}
