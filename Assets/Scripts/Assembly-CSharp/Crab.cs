using UnityEngine;
using UnityEngine.Serialization;

public class Crab : Creature
{
	[Header("Crab Settings")]
	[SerializeField]
	private Animation _anim;

	[SerializeField]
	private string _animIdleName;

	[SerializeField]
	private string _animWalkName;

	[SerializeField]
	private bool _walkSideways = true;

	[SerializeField]
	protected float _moveAcceleration = 15f;

	[SerializeField]
	protected float _moveSpeed = 1f;

	[SerializeField]
	protected float _jumpVel = 5f;

	[SerializeField]
	protected float _stuckJumpVel = 5f;

	[SerializeField]
	protected float _stuckJumpForwardVel = 5f;

	[SerializeField]
	private float _antiUpsideDownForce = 0.5f;

	[SerializeField]
	[Tooltip("Jump if it stays within this radius for too long while grounded.")]
	private float _stuckJumpRadius = 0.5f;

	[SerializeField]
	[Tooltip("How long it can stay in roughly the same place before jumping.")]
	private float _stuckJumpTime = 1f;

	[SerializeField]
	private float _soundLerpSpeed = 3f;

	[SerializeField]
	private float _maxSoundVol = 0.5f;

	[SerializeField]
	private AudioSource _walkSource;

	[FormerlySerializedAs("_airAngVel")]
	[FormerlySerializedAs("_jumpAngVel")]
	[SerializeField]
	protected float _airTurnVel = 15f;

	private float _timeStuckInPlace;

	private Vector3 _lastUnstuckPos;

	private bool NetworkInitialize___EarlyCrabAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateCrabAssembly_002DCSharp_002Edll_Excuted;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.AttachedRod)
		{
			if (!base.IsDead)
			{
				GroundCheck();
			}
			if (!_rig.isKinematic || (_rigSync.IsSimulatedLocal && (!base.IsDead || _hasCustomMovement)))
			{
				UpdateMovement();
			}
		}
	}

	protected virtual void GroundCheck()
	{
		ToggleGrounded(Physics.Raycast(_rig.worldCenterOfMass, Vector3.down, _groundCheckDist, (int)GameInfo.LevelLayer | (int)GameInfo.BoatLayer));
	}

	protected virtual void UpdateMovement()
	{
		if (_isGrounded)
		{
			if ((double)Vector3.Dot(base.transform.up, Vector3.up) > 0.25)
			{
				Vector3 vector = (_walkSideways ? base.transform.right : base.transform.forward);
				_rig.linearVelocity = Vector3.MoveTowards(_rig.linearVelocity, vector * _moveSpeed, _moveAcceleration * Time.fixedDeltaTime);
				_rig.linearVelocity += Vector3.up * (0.25f * Time.fixedDeltaTime);
				UpdateStuckInPlace();
				if (_timeStuckInPlace >= _stuckJumpTime)
				{
					Jump(stuck: true);
				}
			}
			else
			{
				Jump(stuck: false);
				ResetStuckInPlace();
			}
		}
		else if ((double)Vector3.Dot(_rig.transform.up, Vector3.up) < 0.9)
		{
			Vector3 angularVelocityToTarget = DazedUtils.GetAngularVelocityToTarget(_rig.rotation, Quaternion.Euler(0f, _rig.transform.eulerAngles.y, 0f));
			_rig.AddTorque(angularVelocityToTarget * _antiUpsideDownForce);
		}
	}

	private void Jump(bool stuck)
	{
		Vector3 linearVelocity = _rig.linearVelocity;
		float num = ((!_rigSync.OnBoat) ? 1 : 2);
		linearVelocity.y = (stuck ? _stuckJumpVel : _jumpVel);
		linearVelocity.y *= num;
		if (stuck)
		{
			linearVelocity += (_walkSideways ? base.transform.right : base.transform.forward) * (_stuckJumpForwardVel * num);
			_rig.AddTorque(Vector3.up * linearVelocity.y);
		}
		_rig.linearVelocity = linearVelocity;
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

	protected virtual void ToggleGrounded(bool to)
	{
		if (to && !_isGrounded)
		{
			if ((bool)_anim)
			{
				_anim.Stop();
				_anim.Play(_animWalkName);
			}
		}
		else if (!to && _isGrounded && (bool)_anim)
		{
			_anim.Stop();
			_anim.Play(_animIdleName);
		}
		if (to)
		{
			_walkSource.volume = Mathf.Lerp(_walkSource.volume, _maxSoundVol * Mathf.Clamp01(_fakeWorldVelocity.sqrMagnitude * 0.1f), Time.fixedDeltaTime * _soundLerpSpeed);
		}
		else
		{
			_walkSource.volume = Mathf.Lerp(_walkSource.volume, 0f, Time.fixedDeltaTime * _soundLerpSpeed);
		}
		_isGrounded = to;
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		ToggleGrounded(to: false);
		Object.Destroy(_anim);
		if ((bool)_walkSource)
		{
			_walkSource.volume = 0f;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyCrabAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyCrabAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateCrabAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateCrabAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize___Late();
	}
}
