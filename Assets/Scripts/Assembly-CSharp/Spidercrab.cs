using FishNet.Object.Synchronizing;
using UnityEngine;

public class Spidercrab : Crab
{
	[Header("Spidercrab Settings (2 = second phase values)")]
	[SerializeField]
	private float _targetHeight = 0.75f;

	[SerializeField]
	private float _towardsHeightForce = 300f;

	[SerializeField]
	private float _heightDamping = 8f;

	[SerializeField]
	private float _groundedDamping = 1f;

	[SerializeField]
	private float _stunnedDamping = 1f;

	[SerializeField]
	private float _turnVel = 25f;

	[SerializeField]
	private float _turnDamping = 6f;

	[Header("Walk Settings")]
	[SerializeField]
	private Vector2 _timeSpentWalkingMinMax;

	[SerializeField]
	private float _stunTime = 4f;

	[SerializeField]
	private float _stunTime2 = 3f;

	[SerializeField]
	private float _horizontalJumpForce = 0.5f;

	[SerializeField]
	private AnimationCurve _attackingRotCurve;

	[SerializeField]
	private AnimationCurve _stunnedRotCurve;

	[Header("Attack Settings")]
	[SerializeField]
	private float _updateTargetDelay = 1f;

	[SerializeField]
	private float _attackMoveSpeed = 0.2f;

	[SerializeField]
	private float _attackTime = 0.75f;

	[SerializeField]
	[Range(0f, 100f)]
	private int _attackChance = 65;

	[Header("On Death")]
	[SerializeField]
	private Item _spawnOnDeath;

	[SerializeField]
	private Item _meat;

	[SerializeField]
	private float _deathAnimTime = 1.5f;

	[SerializeField]
	private float _animUpVel = 1.5f;

	[SerializeField]
	private float _animAngVel = 15f;

	[SerializeField]
	private float _animRandVel = 2f;

	[SerializeField]
	private ParticleSystem _stunnedParticles;

	private Player _target;

	private float _swappedTargetTime;

	private float _curHeight;

	private Vector3 _levelUpNormal;

	private bool _isFacingUp;

	private Quaternion _attackTargetRot;

	private bool _isWalkingRight;

	private bool _isAnimating;

	private float _curTimeInState;

	private float _targetTimeInState;

	private float _curAnimTime;

	private float _curStatePercent;

	private float _jumpTime;

	private bool _isInWater;

	public readonly SyncVar<bool> _inSecondPhase = new SyncVar<bool>();

	public readonly SyncVar<bool> _isStunned = new SyncVar<bool>();

	private bool NetworkInitialize___EarlySpidercrabAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateSpidercrabAssembly_002DCSharp_002Edll_Excuted;

	public CrabState CurState { get; private set; }

	public float TimeOfLastStun { get; private set; }

	public bool IsGrounded => _isGrounded;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Spidercrab_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	private void OnSecondPhaseChange(bool prev, bool next, bool asServer)
	{
	}

	private void OnStunnedChange(bool prev, bool next, bool asServer)
	{
		if (next)
		{
			_stunnedParticles.Play();
		}
		else
		{
			_stunnedParticles.Stop();
		}
	}

	public override void OnStartServer()
	{
		FindTarget();
	}

	private void OnPlayerAmountChange()
	{
		FindTarget();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		OnStunnedChange(prev: false, _isStunned.Value, asServer: false);
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

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		WaterCheck();
	}

	private void WaterCheck()
	{
		Vector3 vector = (_rigSync.IsSimulatedLocal ? _rig.linearVelocity : _fakeWorldVelocity);
		bool flag = base.transform.position.y <= WaterManager.WaterHeight;
		if ((!base.IsDead && !_isInWater) & flag)
		{
			AudioManager.PlayRandomClipAt((vector.y <= -10f) ? "ItemHitWaterHeavy_V" : "ItemHitWaterMedium_V", 1, 3, base.transform.position, variation: true, AudioDistance.Short, 1f, 0.5f);
			ParticleManager.Play("WaterSplash", base.transform.position);
		}
		_isInWater = flag;
	}

	protected override void GroundCheck()
	{
		if (Physics.Raycast(_rig.worldCenterOfMass, Vector3.down, out var hitInfo, _groundCheckDist, GameInfo.CanJumpOnLayers))
		{
			_levelUpNormal = hitInfo.normal;
			_curHeight = _rig.position.y - hitInfo.point.y;
			ToggleGrounded(to: true);
			if (Time.time - _swappedTargetTime > _updateTargetDelay)
			{
				FindTarget();
			}
		}
		else
		{
			_levelUpNormal = Vector3.up;
			ToggleGrounded(to: false);
		}
	}

	protected override void UpdateMovement()
	{
		if (_isAnimating)
		{
			Animate();
		}
		else
		{
			if (base.IsDead)
			{
				return;
			}
			if (_isGrounded)
			{
				_isFacingUp = Vector3.Dot(base.transform.up, Vector3.up) > 0.25f;
				if (_isFacingUp)
				{
					UpdatePos();
				}
			}
			Rotate();
		}
	}

	private void UpdatePos()
	{
		bool num = base.transform.position.y < WaterManager.WaterHeight && _isGrounded;
		bool flag = (bool)BoatManager.Boat && BoatManager.Boat.BoatTrigger.ItemsOnBoat.Contains(_rigSync);
		if (num | flag)
		{
			if (Time.time - _jumpTime > 0.5f)
			{
				Jump();
			}
			return;
		}
		Vector3 vector = Vector3.zero;
		if (CurState == CrabState.Walking)
		{
			vector = (_isWalkingRight ? (base.transform.right * _moveSpeed) : (-base.transform.right * _moveSpeed));
		}
		else if (CurState == CrabState.Attacking)
		{
			vector = base.transform.forward * _attackMoveSpeed;
			if (base.transform.position.y < WaterManager.WaterHeight)
			{
				vector *= 2f;
			}
		}
		Vector3 a = _rig.linearVelocity + vector;
		a.y = 0f;
		float num2 = ((CurState == CrabState.Stunned) ? _stunnedDamping : _groundedDamping);
		vector = Vector3.Lerp(a, Vector3.zero, num2 * Time.fixedDeltaTime);
		float num3 = _targetHeight - _curHeight;
		float num4 = (vector.y = _rig.linearVelocity.y);
		vector.y += num3 * _towardsHeightForce * Time.fixedDeltaTime;
		vector.y -= num4 * _heightDamping * Time.fixedDeltaTime;
		_curTimeInState += Time.fixedDeltaTime;
		_curStatePercent = Mathf.Clamp01(_curTimeInState / _targetTimeInState);
		if (_curTimeInState >= _targetTimeInState)
		{
			ChangeState();
		}
		_rig.linearVelocity = vector;
	}

	private void Jump()
	{
		Vector3 vector = Island.IslandPos - base.transform.position;
		vector.y = 0f;
		vector.Normalize();
		vector *= _horizontalJumpForce;
		vector.y = 1f;
		_rig.linearVelocity = vector * _jumpVel;
		_jumpTime = Time.time;
		ChangeState();
	}

	private void ChangeState()
	{
		CrabState crabState = CrabState.Walking;
		if (base.transform.position.y < WaterManager.WaterHeight)
		{
			crabState = CrabState.Walking;
		}
		else if (CurState == CrabState.Walking)
		{
			crabState = (((float)Random.Range(0, 100) < (float)_attackChance) ? CrabState.Attacking : CrabState.Walking);
		}
		else if (CurState == CrabState.Attacking)
		{
			crabState = CrabState.Stunned;
			FindTarget();
		}
		_curTimeInState = 0f;
		_curStatePercent = 0f;
		_isWalkingRight = ((crabState == CrabState.Walking) ? (!_isWalkingRight) : _isWalkingRight);
		_targetTimeInState = crabState switch
		{
			CrabState.Walking => Random.Range(_timeSpentWalkingMinMax.x, _timeSpentWalkingMinMax.y), 
			CrabState.Stunned => _inSecondPhase.Value ? _stunTime2 : _stunTime, 
			_ => _attackTime, 
		};
		if (crabState == CrabState.Attacking)
		{
			SetAttackTargetRot();
		}
		if (CurState == CrabState.Stunned)
		{
			TimeOfLastStun = Time.time;
		}
		CurState = crabState;
		_isStunned.Value = crabState == CrabState.Stunned;
	}

	private void SetAttackTargetRot()
	{
		if ((bool)_target && (bool)_target.Transform)
		{
			_attackTargetRot = Quaternion.LookRotation(_target.Transform.position - _rig.position, _levelUpNormal);
		}
	}

	private void Rotate()
	{
		if ((bool)_target && (bool)_target.Transform)
		{
			Quaternion quaternion = Quaternion.LookRotation(_target.Transform.position - _rig.position, _levelUpNormal);
			Quaternion target = quaternion;
			if (CurState != CrabState.Walking)
			{
				float t = ((CurState == CrabState.Attacking) ? _attackingRotCurve : _stunnedRotCurve).Evaluate(_curStatePercent);
				target = Quaternion.Slerp(_attackTargetRot, quaternion, t);
			}
			Vector3 angularVelocityToTarget = DazedUtils.GetAngularVelocityToTarget(_rig.rotation, target);
			Vector3 angularVelocity = _rig.angularVelocity;
			_rig.angularVelocity += angularVelocityToTarget * (_turnVel * Time.fixedDeltaTime);
			_rig.angularVelocity -= angularVelocity * (_turnDamping * Time.fixedDeltaTime);
		}
	}

	private void Animate()
	{
		Vector3 linearVelocity = Vector3.up * _animUpVel;
		linearVelocity += Random.insideUnitSphere.normalized * _animRandVel;
		_rig.linearVelocity = linearVelocity;
		_rig.angularVelocity = base.transform.forward * _animAngVel;
		_curAnimTime += Time.fixedDeltaTime;
		if (_curAnimTime >= _deathAnimTime)
		{
			if (base.IsDead)
			{
				BossManager.SpawnBossTrophy(_spawnOnDeath, _meat, _rig.worldCenterOfMass);
				Despawn(base.gameObject);
			}
			_isAnimating = false;
		}
	}

	private void FindTarget()
	{
		_swappedTargetTime = Time.time;
		_target = PlayerManager.GetNearestAlivePlayer(base.transform.position);
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
			}
			if (next <= 0 && !_isAnimating)
			{
				StartAnimating();
			}
		}
	}

	private void StartAnimating()
	{
		MonoBehaviour.print("started animating");
		_curAnimTime = 0f;
		_isAnimating = true;
	}

	protected override void ToggleGrounded(bool to)
	{
		_isGrounded = to;
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		AchievementManager.CheckBossAchievement(0);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlySpidercrabAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlySpidercrabAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_isStunned.InitializeEarly(this, 12u, isSyncObject: false);
			_inSecondPhase.InitializeEarly(this, 11u, isSyncObject: false);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateSpidercrabAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateSpidercrabAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_isStunned.InitializeLate();
			_inSecondPhase.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	protected virtual void Awake_UserLogic_Spidercrab_Assembly_002DCSharp_002Edll()
	{
		((Creature)this).Awake();
		_inSecondPhase.OnChange += OnSecondPhaseChange;
		_isStunned.OnChange += OnStunnedChange;
		PlayerManager.OnPlayerAmountChange += OnPlayerAmountChange;
	}
}
