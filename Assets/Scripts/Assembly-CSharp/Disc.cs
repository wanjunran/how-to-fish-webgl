using System;
using UnityEngine;

public class Disc : Item
{
	[SerializeField]
	private float _aimUpForce = 1f;

	[SerializeField]
	private float _flyingUpForce = 100f;

	[SerializeField]
	private float _minMagnitude = 1f;

	[SerializeField]
	private float _upForce = 1f;

	[SerializeField]
	private float _spinForce = 1f;

	[SerializeField]
	private int _damage = 10;

	[SerializeField]
	private float _minDamageMagnitude = 3f;

	private bool _hitThisUpdate;

	private bool NetworkInitialize___EarlyDiscAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateDiscAssembly_002DCSharp_002Edll_Excuted;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		_hitThisUpdate = false;
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			AimUpwards(_aimUpForce, withSpin: false);
		}
		else if (_rigSync.IsSimulatedLocal)
		{
			float magnitude = new Vector2(_rig.linearVelocity.x, _rig.linearVelocity.z).magnitude;
			if (magnitude > _minMagnitude)
			{
				AimUpwards(magnitude * _flyingUpForce, withSpin: true);
				UpForce(magnitude);
			}
		}
	}

	private void UpForce(float mag)
	{
		float num = _upForce * mag;
		_rig.linearVelocity += Vector3.up * num;
	}

	private void AimUpwards(float upForce, bool withSpin)
	{
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			Quaternion.FromToRotation(base.transform.up, Vector3.up).ToAngleAxis(out var angle, out var axis);
			if (angle > 180f)
			{
				angle -= 360f;
			}
			Vector3 vector = axis.normalized * (angle * (MathF.PI / 180f) * upForce);
			Vector3 vector2 = (withSpin ? (base.transform.up * _spinForce) : Vector3.zero);
			Vector3 angularVelocity = vector + vector2;
			angularVelocity.y = _rig.angularVelocity.y;
			_rig.angularVelocity = angularVelocity;
		}
	}

	protected override void OnCollisionEnter(Collision other)
	{
		base.OnCollisionEnter(other);
		if (other.contactCount != 0 && !_hitThisUpdate)
		{
			Item item = ItemManager.Get(other);
			if ((bool)item && (bool)item.Creature && !(new Vector2(_rig.linearVelocity.x, _rig.linearVelocity.z).magnitude < _minDamageMagnitude))
			{
				Vector3 point = other.contacts[0].point;
				item.LocalHit(other.transform, point, point - base.transform.position, base.LastHolder, _damage, rangedHit: false);
				_hitThisUpdate = true;
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyDiscAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyDiscAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateDiscAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateDiscAssembly_002DCSharp_002Edll_Excuted = true;
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
