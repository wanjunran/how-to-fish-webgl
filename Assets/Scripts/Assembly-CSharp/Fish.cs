using System.Collections.Generic;
using UnityEngine;

public class Fish : Creature
{
	[Header("Fish Engine B)")]
	[SerializeField]
	private float _minTime;

	[SerializeField]
	private float _maxTime;

	[SerializeField]
	private float _rotForce;

	[SerializeField]
	private float _motorTime;

	[SerializeField]
	private float _gravityMulti;

	[SerializeField]
	public List<HingeJoint> _joints;

	[SerializeField]
	[Tooltip("Add torque to the main rig for boneless fish")]
	private bool _useTorqueInsteadOfJoints;

	[SerializeField]
	[Tooltip("Add a jumpforce that triggers when the fish flaps (only when grounded)")]
	protected float _jumpForce;

	[SerializeField]
	[Tooltip("Add force away from island to make fish flap towards water")]
	private float _towardsWaterForce = 4f;

	[SerializeField]
	protected bool _jumpOnWater;

	[SerializeField]
	private bool _jumpToLandOnWater;

	private float _timer;

	private float _motorTimer;

	private bool _motorStarted;

	private int _rotMultiplier = 1;

	private bool _oldIsColliding;

	protected bool _useMotor = true;

	protected bool _isUnderwater;

	private bool NetworkInitialize___EarlyFishAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateFishAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Fish_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_rigSync.IsSimulatedLocal && (!base.IsDead || _hasCustomMovement))
		{
			UpdateMovement();
		}
		else if (_motorStarted)
		{
			CancelMovement();
		}
	}

	protected virtual void UpdateMovement()
	{
		if (_oldIsColliding != base.IsColliding)
		{
			OnCollision();
		}
		_isUnderwater = !base.AttachedRod && base.transform.position.y < WaterManager.WaterHeight;
		if (_jumpOnWater && _isUnderwater)
		{
			InitializeMovement();
		}
		if (_timer > 0f)
		{
			_timer -= Time.fixedDeltaTime;
			if (!(_timer > 0f))
			{
				InitializeMovement();
			}
		}
		else if (_motorTimer > 0f)
		{
			_motorTimer -= Time.fixedDeltaTime;
			if (_useTorqueInsteadOfJoints)
			{
				_rig.AddTorque(base.transform.up * ((float)_rotMultiplier * _rotForce));
			}
			if (!(_motorTimer > 0f))
			{
				CancelMovement();
			}
		}
		else
		{
			_motorTimer = _minTime;
		}
	}

	protected virtual void InitializeMovement()
	{
		InitializeMovementBase();
	}

	protected void InitializeMovementBase()
	{
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			Drop(calledFromLocal: true);
		}
		_motorTimer = _motorTime;
		float num = Random.Range(0f - _rotForce, _rotForce);
		for (int i = 0; i < _joints.Count; i++)
		{
			_joints[i].useMotor = _useMotor;
			JointMotor motor = _joints[i].motor;
			motor.targetVelocity = num * (float)_rotMultiplier;
			_joints[i].motor = motor;
		}
		if (base.IsColliding && !_rig.isKinematic)
		{
			_rig.linearVelocity += Vector3.up * _jumpForce;
			if (base.BossType == BossType.None)
			{
				Vector3 vector = base.transform.position - Island.IslandPos;
				vector.y = 0f;
				vector.Normalize();
				_rig.linearVelocity += vector * _towardsWaterForce;
			}
			if (_jumpToLandOnWater && _isUnderwater)
			{
				Vector3 vector2 = Island.IslandPos - base.transform.position;
				vector2.y = 0f;
				vector2.Normalize();
				_rig.linearVelocity += vector2 * _towardsWaterForce;
				return;
			}
		}
		_rotMultiplier *= -1;
		_motorStarted = true;
	}

	protected virtual void CancelMovement()
	{
		_timer = Random.Range(_minTime, _maxTime);
		for (int i = 0; i < _joints.Count; i++)
		{
			_joints[i].useMotor = false;
			JointMotor motor = _joints[i].motor;
			_joints[i].motor = motor;
		}
		_motorStarted = false;
	}

	public override void OnDrop()
	{
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			PlayerUI.HideSpecificItemInfo();
		}
		if (_joints.Count != 0)
		{
			for (int i = 0; i < _joints.Count; i++)
			{
				_joints[i].massScale = 1f;
				_joints[i].connectedMassScale = 1f;
			}
		}
	}

	public void ResetJoint()
	{
		if (_joints.Count != 0)
		{
			for (int i = 0; i < _joints.Count; i++)
			{
				_joints[i].transform.localRotation = Quaternion.identity;
			}
		}
	}

	private void OnCollision()
	{
		_oldIsColliding = base.IsColliding;
		if (base.IsColliding)
		{
			CancelMovement();
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyFishAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyFishAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateFishAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateFishAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	protected virtual void Awake_UserLogic_Fish_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_fish = this;
		_type = ItemType.Fish;
	}
}
