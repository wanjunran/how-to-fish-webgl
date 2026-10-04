using System.Collections;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

public class Albatross : Creature
{
	[Header("ALBATROSS")]
	[SerializeField]
	private Animator _anim;

	[Header("Attacking")]
	[SerializeField]
	private int _meleeDamage = 50;

	[SerializeField]
	private float _damageRange;

	[SerializeField]
	private Vector2 _attackIntervalMinMax = new Vector2(3f, 15f);

	[Header("Base Movement")]
	[SerializeField]
	private float _defaultFlySpeed = 1f;

	[SerializeField]
	private float _horizontalSpeed = 3f;

	[SerializeField]
	private float _verticalSpeed = 3f;

	[SerializeField]
	private float _retardation = 0.5f;

	[SerializeField]
	private float _acceleration = 5f;

	[SerializeField]
	private float _minUpDirForFlap = 0.25f;

	[SerializeField]
	private float _homeRadius = 5f;

	[SerializeField]
	private float _homeWeight = 0.25f;

	[Header("Attacking Movement")]
	[SerializeField]
	private float _attackingTargetSpeed = 3f;

	[SerializeField]
	private float _attackingHorizontalSpeed = 5f;

	[SerializeField]
	private float _closeToTargetSpeed = 2f;

	[SerializeField]
	private float _closeToTargetHorizontalSpeed = 2f;

	[SerializeField]
	private float _closeToTargetDist;

	[SerializeField]
	private float _flyHeight = 30f;

	[SerializeField]
	private float _verticalWeight = 0.5f;

	[Header("Pooping")]
	[SerializeField]
	private Transform _poopPoint;

	[SerializeField]
	private ParticleSystem _poopParticles;

	[SerializeField]
	private int _poopCount = 5;

	[SerializeField]
	private float _poopRate = 0.15f;

	[SerializeField]
	private Vector2 _poopTimerMinMax = new Vector2(5f, 10f);

	[SerializeField]
	private float _minDistFromFlyHeightForPooping = 0.5f;

	[Space]
	[SerializeField]
	private WeaponInfo _poopInfo;

	[SerializeField]
	[Range(0f, 1f)]
	private float _poopSpread = 0.25f;

	[SerializeField]
	private string _poopSound;

	[SerializeField]
	private int _randomPoopSounds;

	[SerializeField]
	[Range(0f, 1f)]
	private float _poopVol = 0.5f;

	[Header("Audio")]
	[SerializeField]
	private AudioSource _noiseSource;

	[SerializeField]
	private float _noiseVol = 1f;

	[SerializeField]
	private float _minNoiseDelay = 2.5f;

	[Header("On Death")]
	[SerializeField]
	private Item _spawnOnDeath;

	[SerializeField]
	private float _totalDeathAnimTime = 1.5f;

	[SerializeField]
	private float _animUpVel = 1f;

	[SerializeField]
	private float _animAngVel = 25f;

	[SerializeField]
	private float _animRandVel = 2f;

	[SerializeField]
	private Item _meat;

	private Item _startingTargetItem;

	private Player _targetPlayer;

	public readonly SyncVar<byte> _animState = new SyncVar<byte>();

	private bool _isDying;

	private float _curSpeed;

	private float _foundTargetTime;

	private float _idleTime;

	private float _curAnimTime;

	private float _curFindTargetTime;

	private float _randomizedTargetInterval;

	private float _lastNoiseTime;

	private float _curPoopTimer;

	private float _targetPoopTimer;

	private Vector3 _gizmosRawDir;

	private bool NetworkInitialize___EarlyAlbatrossAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateAlbatrossAssembly_002DCSharp_002Edll_Excuted;

	public Transform Target
	{
		get
		{
			if (!_startingTargetItem)
			{
				if (!_targetPlayer)
				{
					return null;
				}
				return _targetPlayer.Transform;
			}
			return _startingTargetItem.transform;
		}
	}

	public bool IsIdlingRight { get; private set; }

	public float FlyHeight => _flyHeight;

	public float HomeRadius => _homeRadius;

	public float HomeWeight => _homeWeight;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Albatross_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public void SetStartingTargetItem(Item item)
	{
		_startingTargetItem = item;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		PlayerManager.OnPlayerAmountChange -= OnPlayerAmountChange;
		if (base.IsDead)
		{
			BossManager.BossExplosion(_rig.worldCenterOfMass);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		StartCoroutine(DelayedNoise());
	}

	private IEnumerator DelayedNoise()
	{
		while (true)
		{
			AudioClip randomClip = AudioManager.GetRandomClip("AlbatrossScreamIdle_0", 1, 2);
			MakeNoise(randomClip);
			yield return new WaitForSeconds(Random.Range(2, 5));
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.IsServerInitialized)
		{
			UpdateMovement();
			if (!base.IsDead)
			{
				FindTarget();
			}
			PoopInIntervals();
		}
	}

	private void PoopInIntervals()
	{
		if (!(_flyHeight - base.transform.position.y > _minDistFromFlyHeightForPooping))
		{
			_curPoopTimer += Time.fixedDeltaTime;
			if (!(_curPoopTimer < _targetPoopTimer))
			{
				_curPoopTimer = 0f;
				_targetPoopTimer = Random.Range(_poopTimerMinMax.x, _poopTimerMinMax.y);
				StartCoroutine(Poop());
			}
		}
	}

	private IEnumerator Poop()
	{
		for (int i = 0; i < _poopCount; i++)
		{
			ObserverPoopEffects();
			PoopEffects();
			foreach (Player alivePlayer in PlayerManager.AlivePlayers)
			{
				Vector3 vector = alivePlayer.Transform.position + Vector3.up * 0.5f - _poopPoint.position;
				vector.Normalize();
				Vector3 vector2 = Random.insideUnitSphere * _poopSpread;
				Vector3 vector3 = vector + vector2;
				ProjectileManager.Instance.AddProjectile(Player.LocalPlayer, _poopInfo, isLocal: true, _poopPoint.position, vector3 * _poopInfo.ProjectileForce, 0u, 0u, fromNpc: true);
			}
			yield return new WaitForSeconds(_poopRate);
		}
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverPoopEffects()
	{
		RpcWriter___ObserverPoopEffects___2166136261();
	}

	private void PoopEffects()
	{
		_poopParticles.Play();
		AudioManager.PlayRandomClipAt(_poopSound, 1, _randomPoopSounds, base.transform.position, variation: false, AudioDistance.Long, _poopVol);
	}

	private void UpdateMovement()
	{
		if (_isDying)
		{
			DeathAnimation();
			return;
		}
		Move();
		_idleTime += Time.fixedDeltaTime;
		if (_idleTime > 3f)
		{
			_idleTime = 0f;
			InvertIdlingRight();
		}
	}

	private void FindTarget()
	{
		_curFindTargetTime += Time.fixedDeltaTime;
		if (_curFindTargetTime < _randomizedTargetInterval)
		{
			return;
		}
		_curFindTargetTime = 0f;
		_randomizedTargetInterval = Random.Range(_attackIntervalMinMax.x, _attackIntervalMinMax.y);
		if (!_targetPlayer && !(Mathf.Abs(base.transform.position.y - _flyHeight) > 5f))
		{
			Player visiblePlayer = PlayerManager.GetVisiblePlayer(base.transform.position);
			if ((bool)visiblePlayer)
			{
				_foundTargetTime = Time.time;
				_targetPlayer = visiblePlayer;
			}
		}
	}

	private void Move()
	{
		float num = 0f;
		bool flag = (bool)_startingTargetItem || (bool)_targetPlayer;
		if (flag)
		{
			num = ((_startingTargetItem ? _startingTargetItem.transform : _targetPlayer.Transform).position + Vector3.up * 0.5f - base.transform.position).sqrMagnitude;
			if (num < _damageRange * _damageRange)
			{
				if ((bool)_startingTargetItem)
				{
					_startingTargetItem.DestroyItem(5);
				}
				else
				{
					int bossDamage = BossManager.GetBossDamage(_meleeDamage);
					Server.Instance.HitPlayer(_targetPlayer, bossDamage, Vector3.zero, _targetPlayer.Transform.position, 1);
				}
				ResetTarget();
			}
		}
		Vector3 vector = (_gizmosRawDir = CreatureUtils.GetFlyDirection(this));
		if ((bool)_targetPlayer && !_startingTargetItem)
		{
			vector += Vector3.up * 0.5f;
			if (_targetPlayer.Dying.IsDead || vector.y > 0f || Physics.Raycast(base.transform.position, vector, vector.magnitude - 0.15f, GameInfo.LevelLayer))
			{
				ResetTarget();
				flag = false;
			}
		}
		float num2 = (flag ? Mathf.Lerp(_attackingHorizontalSpeed, _closeToTargetHorizontalSpeed, Mathf.Clamp(Time.time - 1f - _foundTargetTime, 0f, 1f)) : _horizontalSpeed);
		Vector3 vector2 = Vector3.Lerp(base.transform.forward, vector, Time.fixedDeltaTime * num2);
		float num3 = _flyHeight - base.transform.position.y;
		num3 *= _verticalWeight;
		float magnitude = vector.magnitude;
		num3 = Mathf.Clamp(num3, 0f - magnitude, magnitude);
		float y = Mathf.Lerp(base.transform.forward.y, num3, Time.fixedDeltaTime * _verticalSpeed);
		if (!flag)
		{
			vector2.y = y;
		}
		vector2.Normalize();
		Vector3 vector3 = vector2;
		if ((bool)_startingTargetItem)
		{
			vector3 = (_startingTargetItem.transform.position - base.transform.position).normalized;
		}
		base.transform.forward = vector3;
		float curSpeed = _curSpeed;
		float num4 = ((!flag) ? _defaultFlySpeed : ((num < _closeToTargetDist * _closeToTargetDist) ? _closeToTargetSpeed : _attackingTargetSpeed));
		bool flag2 = num4 <= curSpeed;
		curSpeed = (_curSpeed = Mathf.MoveTowards(curSpeed, num4, flag2 ? (Time.fixedDeltaTime * _retardation) : (Time.fixedDeltaTime * _acceleration)));
		base.transform.position += vector3 * (curSpeed * Time.fixedDeltaTime);
		if (flag)
		{
			SetAnimState((byte)(((num < _closeToTargetDist * _closeToTargetDist) & flag2) ? 3 : 2));
		}
		else
		{
			SetAnimState((byte)((!(vector3.y > _minUpDirForFlap)) ? 1 : 4));
		}
	}

	private void InvertIdlingRight()
	{
		IsIdlingRight = !IsIdlingRight;
	}

	public void ResetTarget()
	{
		_startingTargetItem = null;
		_targetPlayer = null;
	}

	private void SetAnimState(byte to)
	{
		if (base.IsServerInitialized && !base.IsDead)
		{
			_animState.Value = to;
		}
	}

	private void DeathAnimation()
	{
		Vector3 linearVelocity = Vector3.up * _animUpVel;
		linearVelocity += Random.insideUnitSphere.normalized * _animRandVel;
		_rig.linearVelocity = linearVelocity;
		_rig.angularVelocity = base.transform.forward * _animAngVel;
		_curAnimTime += Time.fixedDeltaTime;
		if (_curAnimTime >= _totalDeathAnimTime)
		{
			BossManager.SpawnBossTrophy(_spawnOnDeath, _meat, _rig.worldCenterOfMass);
			Despawn(base.gameObject);
			_isDying = false;
		}
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		_startingTargetItem = null;
		_targetPlayer = null;
		_rigSync.Freeze(frozen: false);
		_isDying = true;
		AudioManager.PlayRandomClipAt("Seagull_V", 1, 9, base.transform.position, variation: false, AudioDistance.Medium, 0.4f);
		AchievementManager.CheckBossAchievement(3);
	}

	private void OnAnimStateChange(byte prev, byte next, bool asServer)
	{
		if (!((base.IsDead || !_anim) | asServer))
		{
			string trigger = "";
			switch (next)
			{
			case 1:
			{
				trigger = "Searching";
				AudioClip randomClip2 = AudioManager.GetRandomClip("AlbatrossScreamIdle_0", 1, 2);
				MakeNoise(randomClip2);
				break;
			}
			case 2:
			{
				trigger = "Diving";
				AudioClip randomClip = AudioManager.GetRandomClip("AlbatrossScreamDive_0", 1, 2);
				MakeNoise(randomClip, overrideDelay: true);
				break;
			}
			case 3:
			{
				trigger = "Flapping";
				AudioClip clip2 = AudioManager.GetClip("AlbatrossScreamTakeDamage");
				MakeNoise(clip2, overrideDelay: true);
				break;
			}
			case 4:
			{
				trigger = "FlappingUp";
				AudioClip clip = AudioManager.GetClip("AlbatrossScreamTakeDamage");
				MakeNoise(clip);
				break;
			}
			}
			_anim.SetTrigger(trigger);
		}
	}

	private void MakeNoise(AudioClip noise, bool overrideDelay = false)
	{
		if ((!(Time.time < _lastNoiseTime + _minNoiseDelay) || overrideDelay) && (bool)noise)
		{
			_lastNoiseTime = Time.time;
			_noiseSource.PlayOneShot(noise, _noiseVol);
		}
	}

	private void OnPlayerAmountChange()
	{
	}

	private void OnDrawGizmos()
	{
		if (!base.IsDead)
		{
			if ((bool)_startingTargetItem)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(_startingTargetItem.transform.position, 1f);
				Vector3 direction = _startingTargetItem.transform.position - base.transform.position;
				Gizmos.DrawRay(base.transform.position, direction);
			}
			else if ((bool)_targetPlayer)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(_targetPlayer.Transform.position, 1f);
				Vector3 direction2 = _targetPlayer.Transform.position - base.transform.position;
				Gizmos.DrawRay(base.transform.position, direction2);
			}
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(base.transform.position, base.transform.forward);
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(base.transform.position, _gizmosRawDir);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyAlbatrossAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyAlbatrossAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_animState.InitializeEarly(this, 11u, isSyncObject: false);
			RegisterObserversRpc(4u, RpcReader___ObserverPoopEffects___2166136261);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateAlbatrossAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateAlbatrossAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_animState.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverPoopEffects___2166136261()
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

	private void RpcLogic___ObserverPoopEffects___2166136261()
	{
		PoopEffects();
	}

	private void RpcReader___ObserverPoopEffects___2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverPoopEffects___2166136261();
		}
	}

	protected virtual void Awake_UserLogic_Albatross_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_animState.OnChange += OnAnimStateChange;
		PlayerManager.OnPlayerAmountChange += OnPlayerAmountChange;
		_targetPoopTimer = Random.Range(_poopTimerMinMax.x, _poopTimerMinMax.y);
	}
}
