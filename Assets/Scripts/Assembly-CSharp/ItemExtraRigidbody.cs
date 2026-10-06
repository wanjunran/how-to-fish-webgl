using System.Collections.Generic;
using UnityEngine;

public class ItemExtraRigidbody : MonoBehaviour
{
	[SerializeField]
	private Rigidbody _rig;

	private Item _item;

	private Vector3 _lastVel;

	private Quaternion initialLocalRotation;

	private Vector3 initialLocalPosition;

	private Quaternion localRotationOnDisable;

	private Vector3 localPositionOnDisable;

	private HashSet<Collision> _curCollidingWith = new HashSet<Collision>();

	private HashSet<Collision> _tempCollidingWith = new HashSet<Collision>();

	private int _curBoatColCount;

	private bool hasDisabled;

	public bool IsColliding => _curCollidingWith.Count > 0;

	public float OrigLinearDamping { get; private set; }

	public float OrigAngularDamping { get; private set; }

	public Rigidbody Rig => _rig;

	public void SetItem(Item item)
	{
		_item = item;
	}

	public void ResetColCount()
	{
		_curCollidingWith.Clear();
		_curBoatColCount = 0;
	}

	private void Awake()
	{
		initialLocalRotation = base.transform.localRotation;
		initialLocalPosition = base.transform.localPosition;
		OrigLinearDamping = _rig.linearDamping;
		OrigAngularDamping = _rig.angularDamping;
	}

	private void Update()
	{
		if (hasDisabled)
		{
			hasDisabled = false;
			base.transform.localRotation = localRotationOnDisable;
			base.transform.localPosition = localPositionOnDisable;
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		_item.RigidbodySync.OnCollision(col);
		if (_item.RigidbodySync.IsServerInitialized && _item.RigidbodySync.IsFloating)
		{
			_item.RigidbodySync.StartSimulateLocal();
			return;
		}
		if (_rig.isKinematic || base.transform.position.y < WaterManager.WaterHeight)
		{
			if ((bool)_item.Creature && !_item.Creature.IsDead && _item.Creature.BossType != BossType.None && !_item.AttachedRod && _item.RigidbodySync.IsSimulatedLocal)
			{
				Server.Instance.HandOverItemSimulation(_item);
			}
			return;
		}
		if ((bool)_item.ImpactSoundType)
		{
			AudioImpactManager.PlayLocalImpactSound(_item, col, _lastVel);
		}
		if (!col.transform.CompareTag("Level") || !_item.Creature || Time.time - _item.Creature.LastTimeDamagedByImpact < 0.25f || _item.Creature.IsDead)
		{
			return;
		}
		if (_item.Creature.BossType != BossType.None && !_item.AttachedRod)
		{
			if (_item.RigidbodySync.IsSimulatedLocal)
			{
				Server.Instance.HandOverItemSimulation(_item);
			}
			return;
		}
		Player player = (_item.AttachedRod ? _item.AttachedRod.Holder : (_item.LastHolder ?? null));
		if (!player)
		{
			return;
		}
		float num = Vector3.Dot(-_lastVel, col.contacts[0].normal);
		Vector2 vector = (_item.AttachedRod ? _item.AttachedRod.MinMaxFishVelForDamage : GameInfo.DefaultMinMaxFishVelForDamage);
		float num2 = (_item.AttachedRod ? _item.AttachedRod.VelDamageMulti : GameInfo.DefaultVelDamageMulti);
		if (!(num < vector.x))
		{
			float num3 = num;
			if (num3 > vector.y)
			{
				num3 = vector.y;
			}
			num3 *= num2;
			_item.LocalHit(_item.transform, col.contacts[0].point + Vector3.up * 0.1f, -col.contacts[0].normal, player, (int)num3, rangedHit: false);
			if ((bool)_item.AttachedRod)
			{
				_item.AttachedRod.ReleaseItem(_item);
			}
			_item.Creature.SetRecentImpactHit();
		}
	}

	private void OnCollisionStay(Collision col)
	{
		_item.RigidbodySync.OnCollision(col);
		_tempCollidingWith.Add(col);
		if ((bool)_item.Creature && !_item.Creature.IsDead && _item.Creature.BossType != BossType.None && !_item.AttachedRod && _item.RigidbodySync.IsSimulatedLocal)
		{
			Server.Instance.HandOverItemSimulation(_item);
		}
	}

	private void OnCollisionExit(Collision col)
	{
		_item.RigidbodySync.OnCollision(col);
	}

	private void OnDisable()
	{
		ResetColCount();
		DisableHingeJoint();
	}

	private void DisableHingeJoint()
	{
		if (!(_item.transform == base.transform))
		{
			localRotationOnDisable = base.transform.localRotation;
			base.transform.localRotation = initialLocalRotation;
			localPositionOnDisable = base.transform.localPosition;
			base.transform.localPosition = initialLocalPosition;
			hasDisabled = true;
		}
	}

	private void FixedUpdate()
	{
		_lastVel = _rig.linearVelocity;
		HashSet<Collision> tempCollidingWith = _tempCollidingWith;
		HashSet<Collision> curCollidingWith = _curCollidingWith;
		_curCollidingWith = tempCollidingWith;
		_tempCollidingWith = curCollidingWith;
		_tempCollidingWith.Clear();
	}
}
