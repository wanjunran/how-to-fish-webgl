using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

public class BowheadWhale : AttackingFish
{
	[SerializeField]
	private float _constantTowardsPlayerForce = 5f;

	[Header("Noises")]
	[SerializeField]
	private AudioSource _noiseSource;

	[SerializeField]
	private float _noiseVol = 2f;

	[SerializeField]
	private float _noiseDelay = 3f;

	[SerializeField]
	private string _noiseName;

	[SerializeField]
	private int _randomNoises;

	[Header("Fly atttack")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _flyChance = 0.15f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _flyChance2 = 0.33f;

	[SerializeField]
	private float _forceFlyForceTime = 0.75f;

	[SerializeField]
	private float _forceFlyForceTime2 = 0.5f;

	[SerializeField]
	private float _flyForce = 200f;

	[SerializeField]
	private float _flyForce2 = 100f;

	[SerializeField]
	private float _flyForceTowardsPlayer = 28f;

	[SerializeField]
	private float _flyForceTowardsPlayer2 = 18f;

	[SerializeField]
	private float _antiRollOverForce = 3f;

	[SerializeField]
	private float _flyAngDamp = 4f;

	[SerializeField]
	private int _secondPhaseJumps = 3;

	[SerializeField]
	private float _addedFlyingGravity = 28f;

	[SerializeField]
	private float _addedFlyingGravity2 = 32f;

	[Header("Whale Rigs")]
	[SerializeField]
	private Rigidbody _bodyRig;

	[SerializeField]
	private Rigidbody _mouthRig;

	[SerializeField]
	private HingeJoint _bodyJoint;

	[SerializeField]
	private HingeJoint _mouthJoint;

	[Header("Land Explosion")]
	[SerializeField]
	private ExplosionInfo _info;

	[SerializeField]
	private float _landedCooldown = 1f;

	[Header("Lava")]
	[SerializeField]
	private WeaponInfo _lavaInfo;

	[SerializeField]
	[Range(0f, 1f)]
	private float _shootLavaChance = 0.25f;

	[SerializeField]
	private int _lavaCount = 6;

	[SerializeField]
	private float _lavaRecoil = 5f;

	[SerializeField]
	private float _lavaDelay = 0.05f;

	[SerializeField]
	private float _lavaTime = 15f;

	[SerializeField]
	private float _lavaRadius = 1f;

	[SerializeField]
	private Transform _lavaPoint;

	[Header("On Change Phase")]
	[SerializeField]
	private float _totalAnimTime = 3f;

	[SerializeField]
	private float _totalDeathAnimTime = 6f;

	[SerializeField]
	private int _lavaBallsCountInAnimation = 10;

	[SerializeField]
	private float _animUpVel = 1f;

	[SerializeField]
	private float _animAngVel = 3f;

	[SerializeField]
	private float _animRandVel = 2f;

	[Header("On Death")]
	[SerializeField]
	private Item _spawnOnDeath;

	[SerializeField]
	private Item _meat;

	private bool _isFlying;

	private Vector3 _orgRigUp;

	private Quaternion _bodyRotRelativeToRig;

	private Quaternion _mouthRotRelativeToBody;

	private float _bodyHingeTargetAngle;

	private float _mouthHingeTargetAngle;

	private float _curFlyUpForceMultiplier;

	private int _jumpsLeft;

	private bool _isShootingLava;

	private float _curAnimTime;

	private bool _isAnimating;

	private static BowheadWhale _whale;

	private Dictionary<float, Vector3> _lavaPoints = new Dictionary<float, Vector3>();

	public readonly SyncVar<bool> _inSecondPhase = new SyncVar<bool>();

	private bool _justUsedFlyAttack;

	private bool _inInitialFlight;

	private float _prevNoiseSound;

	private float _landedCooldownEndTime = -1f;

	private bool NetworkInitialize___EarlyBowheadWhaleAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateBowheadWhaleAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_BowheadWhale_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (base.IsServerInitialized && base.BossType == BossType.Boss)
		{
			Fly();
			_inInitialFlight = true;
			Collider[] worldColliders = _worldColliders;
			for (int i = 0; i < worldColliders.Length; i++)
			{
				worldColliders[i].isTrigger = true;
			}
		}
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		if (base.IsDead && base.BossType == BossType.Boss)
		{
			BossManager.BossExplosion(base.transform.position, isFinalBoss: true);
		}
	}

	protected override void UpdateMovement()
	{
		if (!base.IsServerInitialized)
		{
			return;
		}
		if (_isAnimating)
		{
			Animate();
		}
		else if (!UpdateLandedCooldown())
		{
			if (!_isFlying && !base.IsDead && (bool)_target)
			{
				Vector3 vector = _target.Transform.position - _rig.worldCenterOfMass;
				vector.Normalize();
				_rig.linearVelocity += vector * (_constantTowardsPlayerForce * Time.fixedDeltaTime);
			}
			base.UpdateMovement();
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_rigSync.IsSimulatedLocal)
		{
			CheckLavaPools();
			if (_isFlying)
			{
				SetFlying();
			}
		}
	}

	private void CheckLavaPools()
	{
		if (base.BossType != BossType.Boss)
		{
			return;
		}
		List<float> list = new List<float>();
		foreach (KeyValuePair<float, Vector3> lavaPoint in _lavaPoints)
		{
			if (Time.time - lavaPoint.Key > _lavaTime)
			{
				list.Add(lavaPoint.Key);
			}
			else
			{
				if (!base.IsServerInitialized)
				{
					continue;
				}
				Collider[] array = Physics.OverlapSphere(lavaPoint.Value, _lavaRadius, GameInfo.PlayerLayers);
				for (int i = 0; i < array.Length; i++)
				{
					Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(array[i].transform);
					if ((bool)playerFromBodyPart)
					{
						playerFromBodyPart.Vitals.ApplyNewFire();
					}
				}
			}
		}
		foreach (float item in list)
		{
			_lavaPoints.Remove(item);
		}
	}

	private void SetFlying()
	{
		if (base.IsDead)
		{
			_isFlying = false;
			return;
		}
		_rig.angularDamping = _flyAngDamp;
		_bodyRig.angularDamping = _flyAngDamp;
		if ((bool)_mouthRig)
		{
			_mouthRig.angularDamping = _flyAngDamp;
		}
		Vector3 angularVelocityToTarget = DazedUtils.GetAngularVelocityToTarget(base.transform.rotation, Quaternion.LookRotation(base.transform.forward, _orgRigUp));
		_rig.angularVelocity = angularVelocityToTarget * _antiRollOverForce;
		if (!_useMotor)
		{
			ResetHingeRigidbody(_bodyRig, _bodyJoint, _rig, _bodyRotRelativeToRig, _bodyHingeTargetAngle);
			if ((bool)_mouthRig)
			{
				ResetHingeRigidbody(_mouthRig, _mouthJoint, _bodyRig, _mouthRotRelativeToBody, _mouthHingeTargetAngle);
			}
		}
		if (_inInitialFlight)
		{
			_curFlyUpForceMultiplier -= Time.fixedDeltaTime * 0.5f;
		}
		else
		{
			_curFlyUpForceMultiplier -= Time.fixedDeltaTime;
		}
		float num = (_inSecondPhase.Value ? _flyForce2 : _flyForce);
		if (_inInitialFlight && _curFlyUpForceMultiplier > 0f)
		{
			num *= 0.5f;
		}
		if (_curFlyUpForceMultiplier > 0f)
		{
			_rig.linearVelocity = Vector3.up * (num * _curFlyUpForceMultiplier);
			return;
		}
		_useMotor = false;
		for (int i = 0; i < _joints.Count; i++)
		{
			_joints[i].useMotor = false;
		}
		Vector3 linearVelocity = _rig.linearVelocity;
		Vector3 vector = _target.Transform.position - _rig.worldCenterOfMass;
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		vector *= (_inSecondPhase.Value ? _flyForceTowardsPlayer2 : _flyForceTowardsPlayer);
		float num2 = (_inSecondPhase.Value ? _addedFlyingGravity2 : _addedFlyingGravity);
		float y = linearVelocity.y - num2 * Time.fixedDeltaTime;
		Vector3 linearVelocity2 = _rig.linearVelocity;
		linearVelocity2.y = y;
		if (linearVelocity2.y < -1f)
		{
			linearVelocity2 = new Vector3(vector.x, y, vector.z);
			if (Physics.Raycast(base.transform.position, Vector3.down, _groundCheckDist * 2f, GameInfo.LevelLayer))
			{
				OnLanded();
				return;
			}
		}
		_rig.linearVelocity = linearVelocity2;
	}

	private void OnLanded()
	{
		ExplosionManager.ServerExplode(this, _info);
		_rig.linearVelocity = Vector3.zero;
		_isFlying = false;
		if (_inInitialFlight)
		{
			Collider[] worldColliders = _worldColliders;
			for (int i = 0; i < worldColliders.Length; i++)
			{
				worldColliders[i].isTrigger = false;
			}
		}
		_inInitialFlight = false;
		_landedCooldownEndTime = Time.time + Mathf.Max(0f, _landedCooldown);
		_useMotor = false;
		for (int j = 0; j < _joints.Count; j++)
		{
			_joints[j].useMotor = false;
		}
		if (_inSecondPhase.Value && _jumpsLeft > 0)
		{
			Fly();
		}
	}

	private bool UpdateLandedCooldown()
	{
		if (_landedCooldownEndTime < 0f)
		{
			return false;
		}
		if (Time.time < _landedCooldownEndTime)
		{
			return true;
		}
		_landedCooldownEndTime = -1f;
		_useMotor = true;
		return false;
	}

	public override void LocalHit(Transform hitTransform, Vector3 point, Vector3 dir, Player player, int damage, bool rangedHit, Vector3 force = default(Vector3), bool fromNpc = false)
	{
		base.LocalHit(hitTransform, point, dir, player, damage, rangedHit, force, false);
		if (base.BossType == BossType.Boss)
		{
			AchievementManager.CheckFinalBossMeleeKillAchievement(_localHp, damage);
		}
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		if (base.BossType == BossType.Boss)
		{
			AudioManager.PlayRandomClipAt("LavaWhaleDeathCry_0", 1, 3, base.transform.position, variation: false, AudioDistance.Long);
			AchievementManager.CheckBossAchievement(4);
		}
	}

	protected override void InitializeMovement()
	{
		if (_isFlying)
		{
			return;
		}
		ObserverMakeNoiseSound();
		MakeNoiseSound();
		InitializeMovementBase();
		float num = Random.Range(0f, 1f);
		float num2 = (_inSecondPhase.Value ? _flyChance2 : _flyChance);
		if (num < num2 && (_isGrounded || base.IsColliding) && !_justUsedFlyAttack)
		{
			Fly();
			return;
		}
		if ((bool)_target && _target.Transform.position.y > base.transform.position.y + 3f && !_justUsedFlyAttack)
		{
			Fly();
			return;
		}
		_justUsedFlyAttack = false;
		AttackTarget();
		if (!_isShootingLava && base.BossType == BossType.Boss && Random.Range(0f, 1f) <= _shootLavaChance)
		{
			StartCoroutine(ShootLava(_lavaCount));
		}
	}

	private IEnumerator ShootLava(int count)
	{
		_isShootingLava = true;
		for (int i = 0; i < count; i++)
		{
			ProjectileManager.Instance.AddProjectile(Player.LocalPlayer, _lavaInfo, isLocal: true, _lavaPoint.position, _lavaPoint.up * _lavaInfo.ProjectileForce, 0u, 0u, fromNpc: true);
			ObserverBurpEffects();
			_bodyRig.AddForceAtPosition(-_lavaPoint.up * _lavaRecoil, _lavaPoint.position);
			BurpEffects();
			yield return new WaitForSeconds(_lavaDelay);
		}
		_isShootingLava = false;
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverBurpEffects()
	{
		RpcWriter___ObserverBurpEffects___2166136261();
	}

	private void BurpEffects()
	{
		AudioManager.PlayRandomClipAt("WhaleBurp_0", 1, 3, base.transform.position, variation: true, AudioDistance.Long);
	}

	private void Fly()
	{
		if (_jumpsLeft <= 0)
		{
			SetNewTarget(random: true);
			if (_inSecondPhase.Value)
			{
				_jumpsLeft = _secondPhaseJumps;
			}
		}
		_justUsedFlyAttack = true;
		_isFlying = true;
		_useMotor = true;
		for (int i = 0; i < _joints.Count; i++)
		{
			_joints[i].useMotor = true;
		}
		_curFlyUpForceMultiplier = (_inSecondPhase.Value ? _forceFlyForceTime2 : _forceFlyForceTime);
		_rig.linearVelocity = Vector3.up * _flyForce;
		_jumpsLeft--;
		ObserverMakeNoiseSound();
		MakeNoiseSound();
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverMakeNoiseSound()
	{
		RpcWriter___ObserverMakeNoiseSound___2166136261();
	}

	private void MakeNoiseSound(bool overrideDelay = false)
	{
		if (!(Time.time < _prevNoiseSound + _noiseDelay) || overrideDelay)
		{
			_prevNoiseSound = Time.time;
			AudioClip audioClip = ((_randomNoises == 0) ? AudioManager.GetClip(_noiseName) : AudioManager.GetRandomClip(_noiseName, 1, _randomNoises));
			if ((bool)audioClip)
			{
				_noiseSource.PlayOneShot(audioClip, _noiseVol);
			}
		}
	}

	private void ResetHingeRigidbody(Rigidbody rig, HingeJoint joint, Rigidbody connectedBody, Quaternion rotRelativeToConnected, float hingeTargetAngle)
	{
		if ((bool)joint && (bool)connectedBody)
		{
			joint.useMotor = false;
			joint.useSpring = true;
			JointSpring spring = joint.spring;
			spring.targetPosition = hingeTargetAngle;
			joint.spring = spring;
			Quaternion target = connectedBody.rotation * rotRelativeToConnected;
			Vector3 lhs = DazedUtils.GetAngularVelocityToTarget(rig.rotation, target) * _antiRollOverForce;
			Vector3 vector = rig.transform.TransformDirection(joint.axis);
			if (!(vector.sqrMagnitude < 0.0001f))
			{
				vector.Normalize();
				rig.angularVelocity = vector * Vector3.Dot(lhs, vector);
			}
		}
	}

	public override void OnCollision(Collision other)
	{
		if (_isFlying && _rig.linearVelocity.y < -1f)
		{
			OnLanded();
		}
		base.OnCollision(other);
	}

	protected override void DamageOnCollision(Player player, Vector3 pos)
	{
		int num = _onHitDamage;
		if (base.BossType != BossType.None)
		{
			num = BossManager.GetBossDamage(num);
		}
		Server.Instance.HitPlayer(player, num, _rig.linearVelocity * GameInfo.PlayerDeathForceMultiFromCreature, pos, 2);
		_lastDamageTime = Time.time;
	}

	public static void AddLavaPool(Vector3 pos, Vector3 dir)
	{
		if ((bool)_whale && _whale.BossType == BossType.Boss)
		{
			ParticleManager.Play("LavaPool", pos, dir);
			VFXManager.Play("LavaExplosion", pos, dir);
			if (_whale.IsServerInitialized)
			{
				_whale._lavaPoints.TryAdd(Time.time, pos);
			}
		}
	}

	private void Animate()
	{
		Vector3 linearVelocity = Vector3.up * _animUpVel;
		linearVelocity += Random.insideUnitSphere.normalized * _animRandVel;
		_rig.linearVelocity = linearVelocity;
		_rig.angularVelocity = base.transform.forward * _animAngVel;
		_curAnimTime += Time.fixedDeltaTime;
		float num = (base.IsDead ? _totalDeathAnimTime : _totalAnimTime);
		if (_curAnimTime >= num)
		{
			if (base.IsDead && base.BossType == BossType.Boss)
			{
				BossManager.SpawnBossTrophy(_spawnOnDeath, _meat, _rig.worldCenterOfMass);
				Despawn(base.gameObject);
			}
			else
			{
				BossManager.ToggleImmortal(to: false);
			}
			_isAnimating = false;
		}
	}

	private void StartAnimating()
	{
		if (base.BossType == BossType.Boss)
		{
			BossManager.ToggleImmortal(to: true);
			_curAnimTime = 0f;
			_isAnimating = true;
			_rig.linearVelocity = Vector3.zero;
			_bodyRig.linearVelocity = Vector3.zero;
			if ((bool)_mouthRig)
			{
				_mouthRig.linearVelocity = Vector3.zero;
			}
		}
	}

	protected override void OnHealthChange(int prev, int next, bool asServer)
	{
		base.OnHealthChange(prev, next, asServer);
		if (base.BossType != BossType.Boss)
		{
			return;
		}
		float num = (float)next / (float)BossManager.BossMaxHp;
		if (asServer)
		{
			if (!_inSecondPhase.Value && num <= 0.5f)
			{
				_inSecondPhase.Value = true;
				StartCoroutine(ShootLava(_lavaBallsCountInAnimation));
				StartAnimating();
			}
			else if (next <= 0 && !_isAnimating)
			{
				ObserverMakeNoiseSound();
				MakeNoiseSound(overrideDelay: true);
				StartAnimating();
			}
		}
	}

	public override ExplosionInfo GetExplosionInfo()
	{
		return _info;
	}

	private void OnSecondPhaseChange(bool prev, bool next, bool asServer)
	{
		if (!(!next | asServer))
		{
			MakeNoiseSound(overrideDelay: true);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyBowheadWhaleAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyBowheadWhaleAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_inSecondPhase.InitializeEarly(this, 11u, isSyncObject: false);
			RegisterObserversRpc(4u, RpcReader___ObserverBurpEffects___2166136261);
			RegisterObserversRpc(5u, RpcReader___ObserverMakeNoiseSound___2166136261);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateBowheadWhaleAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateBowheadWhaleAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_inSecondPhase.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverBurpEffects___2166136261()
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

	private void RpcLogic___ObserverBurpEffects___2166136261()
	{
		BurpEffects();
	}

	private void RpcReader___ObserverBurpEffects___2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverBurpEffects___2166136261();
		}
	}

	private void RpcWriter___ObserverMakeNoiseSound___2166136261()
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendObserversRpc(5u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverMakeNoiseSound___2166136261()
	{
		MakeNoiseSound();
	}

	private void RpcReader___ObserverMakeNoiseSound___2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverMakeNoiseSound___2166136261();
		}
	}

	protected virtual void Awake_UserLogic_BowheadWhale_Assembly_002DCSharp_002Edll()
	{
		((Fish)this).Awake();
		_inSecondPhase.OnChange += OnSecondPhaseChange;
		_whale = this;
		_orgRigUp = _rig.transform.up;
		_bodyRotRelativeToRig = Quaternion.Inverse(_rig.rotation) * _bodyRig.rotation;
		if ((bool)_mouthRig)
		{
			_mouthRotRelativeToBody = Quaternion.Inverse(_bodyRig.rotation) * _mouthRig.rotation;
		}
		_bodyHingeTargetAngle = (_bodyJoint ? _bodyJoint.spring.targetPosition : 0f);
		_mouthHingeTargetAngle = (_mouthJoint ? _mouthJoint.spring.targetPosition : 0f);
	}
}
