using System;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private Rigidbody _rig;

	[SerializeField]
	private CapsuleCollider _col;

	[SerializeField]
	private SphereCollider _footCol;

	[Header("Speed")]
	[SerializeField]
	private float _acceleration;

	[SerializeField]
	[Range(0f, 1f)]
	private float _decceleration;

	[SerializeField]
	[Range(0f, 1f)]
	private float _inheritedVelDecceleration;

	[SerializeField]
	private float _walkSpeed;

	[SerializeField]
	private float _sprintSpeed;

	[Header("Speed Transition")]
	[SerializeField]
	private float _speedTransitionSpeed;

	[SerializeField]
	private AnimationCurve _speedTransitionCurve;

	[Header("Ground Check")]
	[SerializeField]
	private float _slipAngle;

	[SerializeField]
	private float _slipForce;

	[SerializeField]
	private float _groundCheckDist;

	[Header("Jumping")]
	[SerializeField]
	private float _jumpAngle;

	[SerializeField]
	private float _extraGravityForce;

	[SerializeField]
	private float _jumpForce;

	[SerializeField]
	private float _coyoteJumpTime;

	[SerializeField]
	[Tooltip("To prevent multiple jumps in a row because you're still grounded slightly after jumping")]
	private float _jumpDelay;

	[Header("Crouching")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _crouchHeightMulti;

	[SerializeField]
	private float _crouchSpeed;

	[SerializeField]
	private float _crouchWalkSpeedMulti;

	[SerializeField]
	private AnimationCurve _crouchSpeedCurve;

	[Header("Crouching")]
	[SerializeField]
	private float _sinkSpeed;

	[Header("Boat")]
	[SerializeField]
	private float _driverMoveToBoatDuration = 0.25f;

	[SerializeField]
	private AnimationCurve _driverMoveToBoatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Header("Underwater")]
	[SerializeField]
	private int _damageFromWater = 10;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Delay in seconds after the player's head goes underwater before damage starts.")]
	private float _underwaterDamageDelay = 2f;

	[SerializeField]
	private int _swimJumpAmountAllowed = 5;

	private Vector2 _moveInput;

	private Vector3 _curVel;

	private Vector3 _slipDir;

	private Vector3 _lastSafePos;

	private Vector3 _boatVel;

	private Vector3 _boatAngLinVel;

	private Vector3 _extraVel;

	private Vector3 _teleportPos;

	private Vector3 _driverMoveStartPos;

	private Vector3 _lastBoatLocalVelSamplePos;

	private bool _waitingToTeleport;

	private float _origColHeight;

	private float _curCrouchPercent;

	private float _curMoveSpeed;

	private float _moveSpeedTransitionPercent = 1f;

	private float _moveSpeedTransitionStart;

	private float _targetMoveSpeed;

	private float _timeOfJump;

	private float _timeOfLastGrounded;

	private float _timeOfLastDamage;

	private float _timeWentUnderWater = -1f;

	private float _driverMoveStartTime;

	private int _swimJumpCount;

	private bool _isBlendingToDriverPos;

	private bool _hasJumped;

	private bool _hasInheritedVel;

	private bool _sprintInput;

	private bool _crouchInput;

	private bool _isCrouching;

	private bool _isSwimming;

	private bool _isTouchingBoat;

	private bool _tempIsTouchingBoat;

	private bool _hasLastBoatLocalVelSample;

	private float _groundAngle;

	private bool NetworkInitialize___EarlyPlayerMovementAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerMovementAssembly_002DCSharp_002Edll_Excuted;

	public bool Grounded { get; private set; }

	public bool Sprinting { get; private set; }

	public float SlipAngle => _slipAngle;

	public Vector3 Velocity { get; private set; }

	public float VelMagSqr { get; private set; }

	public Vector2 Input => _moveInput;

	public bool OnBoat { get; private set; }

	public Transform CurGroundTransform { get; private set; }

	public Vector3 CurFeetWorldPos { get; private set; }

	public float CrouchHeightMulti => _crouchHeightMulti;

	public float CrouchSpeed => _crouchSpeed;

	public AnimationCurve CrouchSpeedCurve => _crouchSpeedCurve;

	public static event Action OnMoved;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_PlayerMovement_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			BindInputs();
			TeleportToLand();
			_player.ColDetector.OnCollision += OnCollision;
		}
	}

	public override void OnStopClient()
	{
		if (base.Owner.IsLocalClient)
		{
			UnbindInputs();
		}
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["PlayerMove"].performed += MoveInput;
		input.actions["PlayerMove"].canceled += MoveInputCanceled;
		input.actions["PlayerSprint"].performed += SprintInput;
		input.actions["PlayerSprint"].canceled += SprintInputCanceled;
		input.actions["PlayerJump"].performed += JumpInput;
		input.actions["PlayerCrouch"].performed += CrouchInput;
		input.actions["PlayerCrouch"].canceled += CrouchInputCanceled;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["PlayerMove"].performed -= MoveInput;
			input.actions["PlayerMove"].canceled -= MoveInputCanceled;
			input.actions["PlayerSprint"].performed -= SprintInput;
			input.actions["PlayerSprint"].canceled -= SprintInputCanceled;
			input.actions["PlayerJump"].performed -= JumpInput;
			input.actions["PlayerCrouch"].performed -= CrouchInput;
			input.actions["PlayerCrouch"].canceled -= CrouchInputCanceled;
		}
	}

	private void MoveInput(InputAction.CallbackContext context)
	{
		_moveInput = (_player.BlockInputs ? Vector2.zero : context.ReadValue<Vector2>());
		ResetControllerSprintIfStopped();
		OnMoved?.Invoke();
	}

	private void MoveInputCanceled(InputAction.CallbackContext context)
	{
		_moveInput = Vector2.zero;
		ResetControllerSprintIfStopped();
	}

	private void SprintInput(InputAction.CallbackContext context)
	{
		if (_player.BlockInputs)
		{
			_sprintInput = false;
		}
		else if (GameInfo.Input.currentControlScheme == "Controller")
		{
			_sprintInput = _moveInput != Vector2.zero && !_sprintInput;
		}
		else
		{
			_sprintInput = true;
		}
	}

	private void SprintInputCanceled(InputAction.CallbackContext context)
	{
		if (GameInfo.Input.currentControlScheme != "Controller")
		{
			_sprintInput = false;
		}
	}

	private void ResetControllerSprintIfStopped()
	{
		if (GameInfo.Input.currentControlScheme == "Controller" && _moveInput.sqrMagnitude < 0.75f)
		{
			_sprintInput = false;
		}
	}

	private void JumpInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && ((Grounded && CanJumpGroundAngle()) || (!_hasJumped && Time.time < _timeOfLastGrounded + _coyoteJumpTime && CanJumpGroundAngle()) || _isSwimming || (!OnBoat && _isTouchingBoat) || PlayerManager.InGodMode))
		{
			Jump();
		}
	}

	private void CrouchInput(InputAction.CallbackContext context)
	{
		if (_player.BlockInputs)
		{
			_crouchInput = false;
		}
		else if (GameInfo.Input.currentControlScheme == "Controller")
		{
			_crouchInput = !_crouchInput;
		}
		else
		{
			_crouchInput = true;
		}
	}

	private void CrouchInputCanceled(InputAction.CallbackContext context)
	{
		if (GameInfo.Input.currentControlScheme != "Controller")
		{
			_crouchInput = false;
		}
	}

	private void FixedUpdate()
	{
		SetVel();
		_isTouchingBoat = _tempIsTouchingBoat;
		_tempIsTouchingBoat = false;
		if (_waitingToTeleport)
		{
			_rig.MovePosition(_teleportPos);
			_waitingToTeleport = false;
		}
		if (!Boat.IsDrivingLocally)
		{
			Crouch();
			GroundCheck();
			GetBoatVel();
			Move();
		}
		else
		{
			_player.Camera.SetMoveValues(0f, 0f, 0f);
		}
		UnderWaterCheck();
		if (_player.BlockInputs)
		{
			_player.Camera.SetMoveValues(0f, 0f, 0f);
		}
		else if ((bool)BoatManager.Boat && BoatManager.Boat.Driver == _player)
		{
			MoveToBoatDriverPos();
		}
	}

	private void LateUpdate()
	{
		if ((bool)BoatManager.Boat && BoatManager.Boat.Driver == _player)
		{
			MoveToBoatDriverPos();
		}
	}

	private void Move()
	{
		Vector2 vector = _moveInput;
		float sqrMagnitude = vector.sqrMagnitude;
		if (_sprintInput && vector != Vector2.zero && (Grounded || (!Grounded && Sprinting)) && (!_player.Holding.HeldItem || !_player.Holding.HeldItem.Weapon || !_player.Holding.HeldItem.Weapon.IsAds) && !_isSwimming)
		{
			Sprinting = true;
		}
		else
		{
			Sprinting = false;
		}
		if (_player.BlockInputs)
		{
			vector = Vector2.zero;
		}
		UpdateMoveSpeed();
		float curMoveSpeed = _curMoveSpeed;
		if (sqrMagnitude > 1f)
		{
			vector = vector.normalized;
		}
		vector *= _acceleration * curMoveSpeed;
		_curVel.y = 0f;
		_curVel *= _decceleration;
		Vector3 vector2 = _player.CurPlayerRot * new Vector3(vector.x, 0f, vector.y);
		_curVel += vector2;
		bool num = _slipDir.sqrMagnitude > 0f;
		_curVel = Vector3.ClampMagnitude(_curVel, curMoveSpeed);
		_curVel += _slipDir * _slipForce;
		_curVel.y = _rig.linearVelocity.y - _extraGravityForce;
		if (_isSwimming && _curVel.y < 0f && !OnBoat)
		{
			_curVel.y = 0f - _sinkSpeed;
		}
		_rig.linearVelocity = _curVel;
		if (_hasInheritedVel)
		{
			if (Grounded)
			{
				_hasInheritedVel = false;
			}
			else
			{
				Vector3 vector3 = _boatVel + _boatAngLinVel;
				vector3.y = 0f;
				_rig.linearVelocity += vector3;
				_boatVel *= _inheritedVelDecceleration;
				_boatAngLinVel *= _inheritedVelDecceleration;
			}
		}
		float magnitude = new Vector2(_curVel.x, _curVel.z).magnitude;
		magnitude /= Mathf.Max(_curMoveSpeed, 0.01f);
		if (num)
		{
			magnitude = 0f;
		}
		_player.Camera.SetMoveValues(_moveInput.x, _rig.linearVelocity.y, magnitude);
	}

	private void UpdateMoveSpeed()
	{
		float num = _walkSpeed;
		if (Sprinting)
		{
			num = _sprintSpeed;
		}
		if (_isCrouching && Grounded)
		{
			num *= _crouchWalkSpeedMulti;
		}
		if (!Mathf.Approximately(_targetMoveSpeed, num))
		{
			_moveSpeedTransitionStart = _curMoveSpeed;
			_targetMoveSpeed = num;
			_moveSpeedTransitionPercent = 0f;
		}
		_moveSpeedTransitionPercent = Mathf.MoveTowards(_moveSpeedTransitionPercent, 1f, _speedTransitionSpeed * Time.fixedDeltaTime);
		float t = _speedTransitionCurve.Evaluate(_moveSpeedTransitionPercent);
		_curMoveSpeed = Mathf.Lerp(_moveSpeedTransitionStart, _targetMoveSpeed, t);
	}

	public void Jump()
	{
		Vector3 linearVelocity = _rig.linearVelocity;
		if (_isSwimming)
		{
			if (_swimJumpCount < _swimJumpAmountAllowed)
			{
				linearVelocity.y = _jumpForce;
			}
			else
			{
				linearVelocity.y += _jumpForce / (float)(_swimJumpCount - _swimJumpAmountAllowed + 1);
			}
		}
		else
		{
			linearVelocity.y = _jumpForce;
		}
		if (_isTouchingBoat && !OnBoat)
		{
			linearVelocity.y = _jumpForce;
		}
		_rig.linearVelocity = linearVelocity;
		_hasJumped = true;
		_timeOfJump = Time.time;
		if (_isSwimming && !Grounded)
		{
			_swimJumpCount++;
			AudioManager.PlayRandomGlobalClip(GameInfo.WaterStepSound, 1, GameInfo.StepSoundCount, variation: true, 0.2f);
		}
	}

	private void GroundCheck()
	{
		CurFeetWorldPos = _rig.position + Vector3.down * (_col.height * 0.5f);
		Physics.SphereCast(CurFeetWorldPos + Vector3.down * (0f - _col.radius - _col.center.y), _col.radius * 0.9f, Vector3.down, out var hitInfo, _groundCheckDist, GameInfo.CanJumpOnLayers);
		CurGroundTransform = hitInfo.transform;
		_groundAngle = Vector3.Angle(Vector3.up, hitInfo.normal);
		if (_groundAngle > _slipAngle)
		{
			_slipDir = hitInfo.normal;
			_slipDir.y = 0f;
		}
		else
		{
			_slipDir = Vector3.zero;
		}
		if (Grounded != (bool)hitInfo.transform)
		{
			Grounded = hitInfo.transform;
			if (_rig.linearVelocity.y < -5f)
			{
				AudioManager.PlayRandomGlobalClip(SurfaceManager.GetStepSound(CurGroundTransform, CurFeetWorldPos), 1, GameInfo.StepSoundCount, variation: true, _player.Movement.Sprinting ? 0.15f : 0.075f);
			}
		}
		if (Grounded && !_isSwimming)
		{
			_swimJumpCount = 0;
		}
		if (Grounded && _groundAngle < 20f && _rig.position.y > WaterManager.GetWaterHeight(_rig.position) && hitInfo.transform.CompareTag("Level"))
		{
			_lastSafePos = _rig.position;
		}
		if (Grounded)
		{
			_timeOfLastGrounded = Time.time;
			if (Time.time >= _timeOfJump + _jumpDelay)
			{
				_hasJumped = false;
			}
		}
	}

	private void GetBoatVel()
	{
		if (OnBoat && (bool)BoatManager.Boat)
		{
			_boatVel = BoatManager.Boat.Velocity;
			if (_boatVel.y > 0f)
			{
				_boatVel.y = 0f;
			}
			Vector3 rhs = _rig.position - BoatManager.Boat.CenterOfMass;
			_boatAngLinVel = Vector3.Cross(BoatManager.Boat.AngularVelocity, rhs);
			if (_boatAngLinVel.y > 0f)
			{
				_boatAngLinVel.y = 0f;
			}
		}
	}

	public void MoveToBoatDriverPos()
	{
		Vector3 vector = BoatManager.Boat.VisualBoat.position + BoatManager.Boat.VisualBoat.rotation * BoatManager.Boat.DriverPos.localPosition;
		if (!_isBlendingToDriverPos)
		{
			_rig.transform.position = vector;
			return;
		}
		float num = Mathf.Max(_driverMoveToBoatDuration, 0.0001f);
		float num2 = Mathf.Clamp01((Time.time - _driverMoveStartTime) / num);
		float t = _driverMoveToBoatCurve.Evaluate(num2);
		Vector3 position = Vector3.Lerp(_driverMoveStartPos, vector, t);
		_rig.transform.position = position;
		if (num2 >= 1f)
		{
			_isBlendingToDriverPos = false;
		}
	}

	private void UnderWaterCheck()
	{
		bool flag = _player.Transform.position.y <= WaterManager.WaterHeight;
		if (_player.CamObject.position.y <= WaterManager.GetWaterHeight(_player.CamObject.position))
		{
			if (_timeWentUnderWater < 0f)
			{
				_timeWentUnderWater = Time.time;
			}
			if (!_waitingToTeleport && Time.time >= _timeWentUnderWater + _underwaterDamageDelay && Time.time >= _timeOfLastDamage + 0.5f)
			{
				_timeOfLastDamage = Time.time;
				Server.Instance.HitPlayer(_player, _damageFromWater, Vector3.zero, Vector3.zero, 0);
			}
		}
		else
		{
			_timeWentUnderWater = -1f;
		}
		if ((!_isSwimming & flag) && !Grounded && !OnBoat)
		{
			ParticleManager.Play("WaterSplash", _rig.position);
			if (_curVel.y <= -10f)
			{
				AudioManager.PlayRandomGlobalClip("ItemHitWaterHeavy_V", 1, 3, variation: true, 1f, 0.5f);
			}
			else
			{
				AudioManager.PlayRandomGlobalClip("ItemHitWaterMedium_V", 1, 3, variation: true, 1f, 0.5f);
			}
		}
		_isSwimming = flag;
	}

	private void TeleportToLand()
	{
		_player.Transform.position = SpawnManager.PlayerSpawnPos;
		_player.Camera.SetRot(SpawnManager.PlayerSpawnRot);
		Vector3 vector = -_curVel;
		vector.y = 0f;
		_curVel = vector;
		_rig.linearVelocity = vector;
	}

	public void Teleport(Vector3 pos, bool instant = false)
	{
		_teleportPos = pos;
		if (instant)
		{
			_waitingToTeleport = false;
			_rig.position = pos;
			_rig.transform.position = pos;
		}
		else
		{
			_waitingToTeleport = true;
			_rig.MovePosition(pos);
		}
		_curVel = Vector3.zero;
		_rig.linearVelocity = Vector3.zero;
	}

	public void SetBoat(bool onBoat)
	{
		if (OnBoat && !onBoat)
		{
			_hasInheritedVel = true;
			OnBoat = false;
			_boatVel = Vector3.zero;
			_boatAngLinVel = Vector3.zero;
			_hasLastBoatLocalVelSample = false;
		}
		if (!OnBoat & onBoat)
		{
			_hasLastBoatLocalVelSample = false;
		}
		OnBoat = onBoat;
	}

	public void SetDriver(Boat boat)
	{
		if ((bool)boat)
		{
			_driverMoveStartPos = _rig.position;
			_driverMoveStartTime = Time.time;
			_isBlendingToDriverPos = true;
		}
		else
		{
			_isBlendingToDriverPos = false;
		}
		_rig.isKinematic = boat;
	}

	public void SetVel(Vector3 vel)
	{
		_curVel = vel;
		_rig.linearVelocity = vel;
	}

	public void AddBoatPos(Vector3 movePos, Vector3 moveRot)
	{
		if (OnBoat && (bool)BoatManager.Boat)
		{
			Vector3 rhs = _rig.position - BoatManager.Boat.CenterOfMass;
			Vector3 vector = Vector3.Cross(moveRot, rhs);
			_rig.MovePosition(_rig.position + movePos + vector);
		}
	}

	private void Crouch()
	{
		bool flag = _crouchInput || (!Grounded && Time.time > _timeOfLastGrounded + 0.1f);
		if (_isCrouching != flag)
		{
			if (base.IsServerInitialized)
			{
				_player.SetIsCrouching(flag);
			}
			else
			{
				Server.Instance.UpdatePlayerCrouching(_player, flag);
			}
		}
		_isCrouching = flag;
		_curCrouchPercent = Mathf.MoveTowards(_curCrouchPercent, _isCrouching ? 1 : 0, _crouchSpeed * Time.fixedDeltaTime);
		float num = Mathf.Lerp(_origColHeight, _origColHeight * _crouchHeightMulti, _crouchSpeedCurve.Evaluate(_curCrouchPercent));
		if (_col.height != num)
		{
			float num2 = (_origColHeight - num) * 0.5f;
			_col.height = num;
			_col.center = Vector3.up * num2;
			_footCol.center = Vector3.down * (_col.height * 0.5f + (0f - _col.radius) - num2 + 0.025f);
		}
	}

	private void OnCollision(Collision col)
	{
		if (1 << col.gameObject.layer == (int)GameInfo.BoatLayer)
		{
			_isTouchingBoat = true;
		}
		if ((((LayerMask)((int)GameInfo.BoatLayer | (int)GameInfo.LevelLayer)).value & (1 << col.gameObject.layer)) != 0)
		{
			_swimJumpCount = 0;
		}
	}

	private void SetVel()
	{
		Vector3 velocity = _rig.linearVelocity;
		if (OnBoat && (bool)BoatManager.Boat)
		{
			Transform visualBoat = BoatManager.Boat.VisualBoat;
			Vector3 vector = visualBoat.InverseTransformPoint(_rig.position);
			if (_hasLastBoatLocalVelSample)
			{
				Vector3 direction = (vector - _lastBoatLocalVelSamplePos) / Time.fixedDeltaTime;
				velocity = visualBoat.TransformDirection(direction);
			}
			else
			{
				velocity = Vector3.zero;
			}
			_lastBoatLocalVelSamplePos = vector;
			_hasLastBoatLocalVelSample = true;
		}
		else
		{
			_hasLastBoatLocalVelSample = false;
		}
		Velocity = velocity;
		VelMagSqr = velocity.sqrMagnitude;
	}

	[TargetRpc]
	public void RPCKnockback(NetworkConnection netCon, Vector3 force)
	{
		RpcWriter___RPCKnockback___4285073289(netCon, force);
	}

	public void Knockback(Vector3 force)
	{
		_curVel += force;
		_rig.linearVelocity += force;
	}

	private bool CanJumpGroundAngle()
	{
		return _groundAngle < _jumpAngle;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerMovementAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerMovementAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterTargetRpc(0u, RpcReader___RPCKnockback___4285073289);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerMovementAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerMovementAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___RPCKnockback___4285073289(NetworkConnection netCon, Vector3 force)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(force);
		SendTargetRpc(0u, pooledWriter, channel, DataOrderType.Default, netCon, excludeServer: false);
		pooledWriter.Store();
	}

	public void RpcLogic___RPCKnockback___4285073289(NetworkConnection P_0, Vector3 P_1)
	{
		Knockback(P_1);
	}

	private void RpcReader___RPCKnockback___4285073289(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___RPCKnockback___4285073289(base.LocalConnection, vector);
		}
	}

	private void Awake_UserLogic_PlayerMovement_Assembly_002DCSharp_002Edll()
	{
		Grounded = true;
		_origColHeight = _col.height;
		_curMoveSpeed = _walkSpeed;
		_moveSpeedTransitionStart = _walkSpeed;
		_targetMoveSpeed = _walkSpeed;
	}
}
