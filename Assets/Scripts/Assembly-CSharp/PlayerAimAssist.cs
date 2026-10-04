using System;
using UnityEngine;

public class PlayerAimAssist : MonoBehaviour
{
	[Header("Target Acquisition")]
	[SerializeField]
	[Min(0f)]
	private float _maxTargetDistance = 60f;

	[SerializeField]
	[Range(0f, 90f)]
	private float _adsAcquireAngle = 18f;

	[SerializeField]
	[Range(0f, 180f)]
	private float _trackingBreakAngle = 40f;

	[SerializeField]
	[Min(0.01f)]
	private float _targetScanInterval = 0.1f;

	[Header("Camera Tracking")]
	[SerializeField]
	[Min(0f)]
	private float _maxRotationSpeed = 120f;

	[SerializeField]
	[Min(0f)]
	private float _trackingSharpness = 10f;

	private Player _player;

	private Creature _target;

	private float _nextTargetScanTime;

	private float _maxSqrTargetDistance;

	private float _minAcquireAlignment;

	private float _acquireAlignmentRange;

	private float _minTrackingAlignment;

	private bool _wasAds;

	private void Awake()
	{
		_player = GetComponent<Player>();
		CacheSettings();
	}

	private void OnValidate()
	{
		CacheSettings();
	}

	public Vector2 GetRotationDelta(Vector3 cameraPosition, Vector3 cameraEuler, float manualLookAmount)
	{
		if (!CanUseAimAssist() || !IsAimingDownSights())
		{
			ResetAimAssist();
			return Vector2.zero;
		}
		if (!_wasAds)
		{
			_target = FindBestTarget(cameraPosition, cameraEuler);
			_nextTargetScanTime = Time.time + _targetScanInterval;
		}
		_wasAds = true;
		if ((bool)_target && !CanTrackTarget(_target, cameraPosition, cameraEuler))
		{
			ClearTarget();
		}
		if (!_target && Time.time >= _nextTargetScanTime)
		{
			_target = FindBestTarget(cameraPosition, cameraEuler);
			_nextTargetScanTime = Time.time + _targetScanInterval;
		}
		if (!_target)
		{
			return Vector2.zero;
		}
		Vector3 eulerAngles = Quaternion.LookRotation(GetTargetPosition(_target) - cameraPosition, Vector3.up).eulerAngles;
		Vector2 vector = new Vector2(Mathf.DeltaAngle(cameraEuler.x, eulerAngles.x), Mathf.DeltaAngle(cameraEuler.y, eulerAngles.y));
		float num = 1f - Mathf.Clamp01(manualLookAmount);
		float num2 = 1f - Mathf.Exp((0f - _trackingSharpness) * Time.deltaTime);
		return Vector2.ClampMagnitude(vector * (num2 * num), _maxRotationSpeed * Time.deltaTime);
	}

	private bool CanUseAimAssist()
	{
		if ((bool)_player && !_player.BlockInputs && _player.Camera.MouseLocked && (bool)GameInfo.Input)
		{
			return GameInfo.Input.currentControlScheme == "Controller";
		}
		return false;
	}

	private bool IsAimingDownSights()
	{
		Item heldItem = _player.Holding.HeldItem;
		if ((bool)heldItem && (bool)heldItem.Weapon)
		{
			return heldItem.Weapon.IsAds;
		}
		return false;
	}

	private Creature FindBestTarget(Vector3 cameraPosition, Vector3 cameraEuler)
	{
		Creature result = null;
		float num = float.MaxValue;
		Vector3 lhs = Quaternion.Euler(cameraEuler) * Vector3.forward;
		foreach (Item value in ItemManager.Items.Values)
		{
			Creature creature = (value ? value.Creature : null);
			if (!IsAliveCreature(creature))
			{
				continue;
			}
			Vector3 targetPosition = GetTargetPosition(creature);
			Vector3 rhs = targetPosition - cameraPosition;
			float sqrMagnitude = rhs.sqrMagnitude;
			if (sqrMagnitude <= Mathf.Epsilon || sqrMagnitude > _maxSqrTargetDistance)
			{
				continue;
			}
			float num2 = Vector3.Dot(lhs, rhs) / Mathf.Sqrt(sqrMagnitude);
			if (!(num2 < _minAcquireAlignment) && !IsTargetObstructed(cameraPosition, targetPosition))
			{
				float num3 = (1f - num2) / _acquireAlignmentRange;
				float num4 = sqrMagnitude / _maxSqrTargetDistance;
				float num5 = num3 + num4 * 0.1f;
				if (!(num5 >= num))
				{
					num = num5;
					result = creature;
				}
			}
		}
		return result;
	}

	private bool CanTrackTarget(Creature creature, Vector3 cameraPosition, Vector3 cameraEuler)
	{
		if (!IsAliveCreature(creature))
		{
			return false;
		}
		Vector3 targetPosition = GetTargetPosition(creature);
		Vector3 rhs = targetPosition - cameraPosition;
		float sqrMagnitude = rhs.sqrMagnitude;
		if (sqrMagnitude <= Mathf.Epsilon || sqrMagnitude > _maxSqrTargetDistance)
		{
			return false;
		}
		if (Vector3.Dot(Quaternion.Euler(cameraEuler) * Vector3.forward, rhs) / Mathf.Sqrt(sqrMagnitude) < _minTrackingAlignment)
		{
			return false;
		}
		return !IsTargetObstructed(cameraPosition, targetPosition);
	}

	private static bool IsAliveCreature(Creature creature)
	{
		if ((bool)creature && creature.isActiveAndEnabled && !creature.IsDeinitializing)
		{
			return !creature.IsDead;
		}
		return false;
	}

	private static Vector3 GetTargetPosition(Creature creature)
	{
		if (!creature.Rig)
		{
			return creature.transform.position;
		}
		return creature.Rig.worldCenterOfMass;
	}

	private static bool IsTargetObstructed(Vector3 cameraPosition, Vector3 targetPosition)
	{
		return Physics.Linecast(cameraPosition, targetPosition, (int)GameInfo.LevelLayer | (int)GameInfo.BoatLayer, QueryTriggerInteraction.Ignore);
	}

	private void ClearTarget()
	{
		_target = null;
	}

	private void ResetAimAssist()
	{
		ClearTarget();
		_wasAds = false;
		_nextTargetScanTime = 0f;
	}

	private void CacheSettings()
	{
		_maxSqrTargetDistance = Mathf.Max(_maxTargetDistance * _maxTargetDistance, Mathf.Epsilon);
		_minAcquireAlignment = Mathf.Cos(_adsAcquireAngle * (MathF.PI / 180f));
		_acquireAlignmentRange = Mathf.Max(1f - _minAcquireAlignment, Mathf.Epsilon);
		_minTrackingAlignment = Mathf.Cos(_trackingBreakAngle * (MathF.PI / 180f));
	}
}
