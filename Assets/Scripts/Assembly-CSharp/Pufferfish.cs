using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

public class Pufferfish : Fish
{
	[Header("Attacks")]
	[SerializeField]
	private int _onHitDamage = 25;

	[SerializeField]
	private float _timeBetweenDamage = 0.5f;

	[SerializeField]
	private float _swapTargetAfterHitChance = 0.3f;

	[SerializeField]
	private float _swapTargetDelay = 6f;

	[SerializeField]
	private float _sqrtPoisonSize = 3f;

	[SerializeField]
	private float _poisonDamageRadius = 0.5f;

	[SerializeField]
	private float _poisonLingerTime = 4f;

	[Header("Movement (2 = second phase)")]
	[SerializeField]
	private float _forceTowardsPlayer = 7f;

	[SerializeField]
	private float _forceTowardsPlayer2 = 7f;

	[Space]
	[SerializeField]
	private float _flyingForceTowardsPlayer = 3.5f;

	[Space]
	[SerializeField]
	private float _torqueTowardPlayer = 10f;

	[SerializeField]
	private float _torqueTowardPlayer2 = 10f;

	[Space]
	[SerializeField]
	private float _maxSqrtAngVel = 400f;

	[SerializeField]
	private float _maxSqrtAngVel2 = 360f;

	[Space]
	[SerializeField]
	private float _velDamping = 3f;

	[SerializeField]
	private float _velDamping2 = 1.75f;

	[Space]
	[SerializeField]
	private float _onHitBackForce = 12f;

	[SerializeField]
	private float _minYDiffForJump = 1.5f;

	[SerializeField]
	private float _bossJumpForce = 5f;

	[SerializeField]
	private float _groundedDist = 0.5f;

	[SerializeField]
	private float _minTimeGroundedForJump = 1f;

	[SerializeField]
	[Tooltip("Jump if the pufferfish stays within this radius while grounded.")]
	private float _stuckJumpRadius = 1f;

	[SerializeField]
	[Tooltip("How long the pufferfish can stay in roughly the same place before jumping.")]
	private float _stuckJumpTime = 2f;

	[Header("Visuals")]
	[SerializeField]
	private float _finalScale = 4f;

	[SerializeField]
	private Transform _boneToScaleUp;

	[SerializeField]
	private Transform _boneToScaleDown;

	[SerializeField]
	private Transform _boneToMove;

	[Header("On Change Phase")]
	[SerializeField]
	private float _totalAnimTime = 3f;

	[SerializeField]
	private float _totalDeathAnimTime = 1.5f;

	[SerializeField]
	private float _animUpVel = 1f;

	[SerializeField]
	private float _animAngVel = 25f;

	[SerializeField]
	private float _animRandVel = 2f;

	[Header("Sound")]
	[SerializeField]
	private float _maxRollVol = 1f;

	[SerializeField]
	private float _volLerpSpeed = 5f;

	[SerializeField]
	private AudioSource _rollSource;

	[Header("On Death")]
	[SerializeField]
	private Item _spawnOnDeath;

	[SerializeField]
	private Item _meat;

	private Player _target;

	private float _curSwapTarget;

	private float _lastDamageTime;

	private float _timeOnGround;

	private float _timeStuckInAir;

	private float _timeStuckInPlace;

	private Vector3 _dir;

	private Vector3 _lastPoisonPos;

	private Vector3 _lastUnstuckPos;

	public readonly SyncVar<bool> _inSecondPhase = new SyncVar<bool>();

	private List<PoisonPoint> _poisonPoints = new List<PoisonPoint>();

	private bool _isAnimating;

	private float _curAnimTime;

	private bool _hasLanded;

	private bool NetworkInitialize___EarlyPufferfishAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePufferfishAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Pufferfish_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	private void OnPlayerAmountChange()
	{
		RandomizeTarget();
	}

	public override void OnStartServer()
	{
		RandomizeTarget();
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		PlayerManager.OnPlayerAmountChange -= OnPlayerAmountChange;
		BossManager.OnBossMaxHpChanged -= OnBossMaxHpChanged;
		if (base.IsDead)
		{
			BossManager.BossExplosion(_rig.worldCenterOfMass);
		}
	}

	protected override void UpdateMovement()
	{
		if (base.IsServerInitialized && !base.AttachedRod)
		{
			if (_isAnimating)
			{
				Animate();
			}
			if (_inSecondPhase.Value)
			{
				TryPlacePoison();
				CheckPoisonCol();
			}
			FindTarget();
			if ((bool)_target && !_isAnimating)
			{
				FollowPlayer();
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
			if (base.IsDead)
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

	private void CheckPoisonCol()
	{
		List<int> list = new List<int>();
		for (int num = _poisonPoints.Count - 1; num >= 0; num--)
		{
			Collider[] array = Physics.OverlapSphere(_poisonPoints[num].Pos, _poisonDamageRadius, GameInfo.PlayerLayers);
			for (int i = 0; i < array.Length; i++)
			{
				Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(array[i].transform);
				if ((bool)playerFromBodyPart)
				{
					playerFromBodyPart.Vitals.ApplyNewPoison();
				}
			}
			_poisonPoints[num].IncreaseLifetime();
			if (_poisonPoints[num].LifeTime >= _poisonLingerTime)
			{
				list.Add(num);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			_poisonPoints.RemoveAt(list[j]);
		}
		list.Clear();
	}

	protected override void FixedUpdate()
	{
		GroundCheck();
		base.FixedUpdate();
	}

	private void GroundCheck()
	{
		_isGrounded = Physics.Raycast(_rig.worldCenterOfMass, Vector3.down, _groundedDist * _boneToScaleUp.localScale.x, GameInfo.CanJumpOnLayers);
		_rollSource.volume = Mathf.Lerp(_rollSource.volume, _isGrounded ? _maxRollVol : 0f, Time.fixedDeltaTime * _volLerpSpeed);
	}

	private void FindTarget()
	{
		_curSwapTarget += Time.fixedDeltaTime;
		if (_curSwapTarget >= _swapTargetDelay)
		{
			RandomizeTarget();
		}
	}

	private void RandomizeTarget()
	{
		_curSwapTarget = 0f;
		_target = PlayerManager.GetRandomAlivePlayer();
	}

	private void FollowPlayer()
	{
		_dir = (_target.Transform.position - _rig.worldCenterOfMass).normalized;
		Vector3 vector = Vector3.ProjectOnPlane(_rig.linearVelocity, _dir);
		if (!_isGrounded && !_hasLanded)
		{
			return;
		}
		if (_isGrounded)
		{
			_hasLanded = true;
			_timeOnGround += Time.fixedDeltaTime;
			_timeStuckInAir = 0f;
			UpdateStuckInPlace();
		}
		else
		{
			_timeOnGround = 0f;
			if (_rig.linearVelocity.y < 0.1f)
			{
				_timeStuckInAir += Time.fixedDeltaTime;
			}
			else
			{
				_timeStuckInAir = 0f;
			}
			ResetStuckInPlace();
		}
		float num = (_inSecondPhase.Value ? _forceTowardsPlayer2 : _forceTowardsPlayer);
		float num2 = (_inSecondPhase.Value ? _velDamping2 : _velDamping);
		float num3 = (_inSecondPhase.Value ? _maxSqrtAngVel2 : _maxSqrtAngVel);
		float num4 = (_inSecondPhase.Value ? _torqueTowardPlayer2 : _torqueTowardPlayer);
		_rig.AddForce(_isGrounded ? (_dir * num) : (_dir * _flyingForceTowardsPlayer));
		_rig.AddForce(-vector * num2, ForceMode.Acceleration);
		if (_rig.angularVelocity.sqrMagnitude < num3 && _isGrounded)
		{
			_rig.AddTorque(Vector3.Cross(Vector3.up, _dir) * (num4 * _boneToScaleUp.localScale.x));
		}
		bool num5 = _target.Transform.position.y >= _rig.worldCenterOfMass.y + _minYDiffForJump;
		bool flag = _timeStuckInAir >= 5f;
		bool flag2 = _timeStuckInPlace >= _stuckJumpTime;
		if ((num5 || flag || flag2) && _isGrounded && !(_timeOnGround < _minTimeGroundedForJump))
		{
			Vector3 linearVelocity = _rig.linearVelocity;
			linearVelocity.y = _bossJumpForce * Mathf.Max(1f, _target.Transform.position.y - _rig.worldCenterOfMass.y);
			_rig.linearVelocity = linearVelocity;
			ResetStuckInPlace();
		}
	}

	private void UpdateStuckInPlace()
	{
		Vector3 vector = _rig.worldCenterOfMass - _lastUnstuckPos;
		vector.y = 0f;
		if (vector.sqrMagnitude <= _stuckJumpRadius * _stuckJumpRadius)
		{
			_timeStuckInPlace += Time.fixedDeltaTime;
			return;
		}
		_lastUnstuckPos = _rig.worldCenterOfMass;
		_timeStuckInPlace = 0f;
	}

	private void ResetStuckInPlace()
	{
		_lastUnstuckPos = _rig.worldCenterOfMass;
		_timeStuckInPlace = 0f;
	}

	private void TryPlacePoison()
	{
		if (!base.IsDead && !(((Vector2)(_rig.worldCenterOfMass - _lastPoisonPos)).sqrMagnitude < _sqrtPoisonSize))
		{
			ObserverPlacePoison(_rig.worldCenterOfMass);
			PlacePoison(_rig.worldCenterOfMass);
			_lastPoisonPos = _rig.worldCenterOfMass;
		}
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverPlacePoison(Vector3 pos)
	{
		RpcWriter___ObserverPlacePoison___4276783012(pos);
	}

	private void PlacePoison(Vector3 pos)
	{
		VFXManager.Play("Poison", pos);
		_poisonPoints.Add(new PoisonPoint(pos));
	}

	protected override void OnHealthChange(int prev, int next, bool asServer)
	{
		float num = (float)next / (float)BossManager.BossMaxHp;
		SetScale(num);
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

	protected override void OnDeath()
	{
		base.OnDeath();
		_rollSource.volume = 0f;
		AchievementManager.CheckBossAchievement(2);
	}

	private void SetScale(float percent)
	{
		float num = Mathf.Lerp(_finalScale, 1f, percent);
		float num2 = Mathf.Lerp(_finalScale, 0f, percent);
		_boneToScaleUp.localScale = Vector3.one * num;
		_boneToScaleDown.localScale = Vector3.one / num;
		_boneToMove.localPosition = Vector3.right * (num2 * -0.2f);
	}

	private void OnBossMaxHpChanged()
	{
		float scale = (float)_hp.Value / (float)BossManager.BossMaxHp;
		SetScale(scale);
	}

	private void StartAnimating()
	{
		_curAnimTime = 0f;
		_isAnimating = true;
	}

	protected override void OnCollisionEnter(Collision other)
	{
		base.OnCollisionEnter(other);
		if (!_rigSync.IsSimulatedLocal || base.IsDead || Time.time - _lastDamageTime < _timeBetweenDamage || !other.transform.CompareTag("Player"))
		{
			return;
		}
		Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(other.transform);
		if ((bool)playerFromBodyPart)
		{
			Server.Instance.HitPlayer(playerFromBodyPart, BossManager.GetBossDamage(_onHitDamage), _rig.linearVelocity * GameInfo.PlayerDeathForceMultiFromCreature, other.transform.position, 2);
			_lastDamageTime = Time.time;
			Vector3 normalized = (other.transform.position - base.transform.position).normalized;
			_rig.linearVelocity = -normalized * _onHitBackForce + Vector3.up;
			if (Random.Range(0f, 1f) > _swapTargetAfterHitChance)
			{
				RandomizeTarget();
			}
			_rig.AddTorque(Vector3.Cross(Vector3.up, normalized) * (_torqueTowardPlayer * _boneToScaleUp.localScale.x));
		}
	}

	private void OnSecondPhaseChange(bool prev, bool next, bool asServer)
	{
		if (!asServer)
		{
			VFXManager.Play("Angry", _rig.position);
			return;
		}
		_isAnimating = true;
		BossManager.ToggleImmortal(to: true);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPufferfishAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPufferfishAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_inSecondPhase.InitializeEarly(this, 11u, isSyncObject: false);
			RegisterObserversRpc(4u, RpcReader___ObserverPlacePoison___4276783012);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePufferfishAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePufferfishAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_inSecondPhase.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverPlacePoison___4276783012(Vector3 pos)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(pos);
		SendObserversRpc(4u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverPlacePoison___4276783012(Vector3 P_0)
	{
		PlacePoison(P_0);
	}

	private void RpcReader___ObserverPlacePoison___4276783012(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverPlacePoison___4276783012(vector);
		}
	}

	protected virtual void Awake_UserLogic_Pufferfish_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_lastUnstuckPos = _rig.worldCenterOfMass;
		_inSecondPhase.OnChange += OnSecondPhaseChange;
		PlayerManager.OnPlayerAmountChange += OnPlayerAmountChange;
		BossManager.OnBossMaxHpChanged += OnBossMaxHpChanged;
	}
}
