using System;
using UnityEngine;

public class CrabLegs : MonoBehaviour
{
	[Serializable]
	private class Leg
	{
		[SerializeField]
		private Transform _target;

		private Vector3 _localOffset;

		public Transform Target => _target;

		public Vector3 LocalOffset
		{
			get
			{
				return _localOffset;
			}
			set
			{
				_localOffset = value;
			}
		}
	}

	[SerializeField]
	private Spidercrab _spiderCrab;

	[SerializeField]
	private Transform _body;

	[SerializeField]
	private Transform _moveReference;

	[SerializeField]
	private Leg[] _legs;

	[SerializeField]
	private float _raycastDist;

	[SerializeField]
	private float _raycastFromUpDist;

	[SerializeField]
	private float _minStepDist;

	[SerializeField]
	private float _maxLegLengthAllowedStanding;

	[SerializeField]
	private float _maxLegLengthAllowedWalking;

	[Header("Airborne")]
	[SerializeField]
	[Tooltip("How far below the body feet should rest while airborne.")]
	private float _airFootDownOffset = 0.25f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _airSideLocalOffset = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _airForwardLocalOffset = 1f;

	[SerializeField]
	[Tooltip("How far an airborne foot can drift before it snaps back under the body.")]
	private float _airRepositionDist = 0.08f;

	[SerializeField]
	[Tooltip("How quickly airborne feet follow their under-body target.")]
	private float _airFollowSpeed = 14f;

	[Header("Landing Audio")]
	[SerializeField]
	private AudioSequence _landingSequence;

	[SerializeField]
	[Min(0f)]
	[Tooltip("Time with no supporting legs before the next leg landing plays the sequence.")]
	private float _landingSequenceUngroundedTime = 3f;

	[Header("Walking")]
	[SerializeField]
	private float _walkStepForwardDist;

	[SerializeField]
	private float _walkStepDist;

	[SerializeField]
	private float _walkStepDistVelMulti;

	[SerializeField]
	private AnimationCurve _walkStepHeightCurve;

	[SerializeField]
	private AnimationCurve _walkStepMoveCurve;

	[SerializeField]
	private float _walkStepSpeed;

	[SerializeField]
	[Tooltip("Below this velocity, feet will only be lifted a small amount")]
	private float _smallStepVel;

	[SerializeField]
	[Range(0f, 1f)]
	private float _smallStepHeightMulti = 1f;

	private Vector3[] _footPoses;

	private Vector3[] _lastStepPos;

	private Vector3[] _nextStepPos;

	private float[] _curStepPercent;

	private bool _didInitialize;

	private int _activeLegGroup = -1;

	private float _maxLegLengthAllowedStandingSqr;

	private float _maxLegLengthAllowedWalkingSqr;

	private float _curMaxLegLengthAllowedSqr;

	private float _airRepositionDistSqr;

	private float _timeWithNoGroundedLegs;

	private bool _landingSequenceArmed;

	private void Awake()
	{
		TryInitialize();
	}

	private void OnEnable()
	{
		TryInitialize();
		SnapFeetToCurrentTargets();
	}

	private void OnDisable()
	{
		AudioSequenceManager.CancelAllActiveSequencesFromOwner(base.gameObject);
		_timeWithNoGroundedLegs = 0f;
		_landingSequenceArmed = false;
	}

	private void Update()
	{
		TryInitialize();
		if (_legs.Length == 0)
		{
			return;
		}
		UpdateLandingSequence();
		if ((bool)_spiderCrab && (!_spiderCrab.IsGrounded || (bool)_spiderCrab.AttachedRod))
		{
			UpdateAirborneFeet();
			ApplyPosToTargets();
			return;
		}
		SetMaxLegLength();
		bool flag = false;
		for (int i = 0; i < _legs.Length; i++)
		{
			if (_curStepPercent[i] < 1f)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			TryStartNextLegGroupStep();
		}
		for (int j = 0; j < _legs.Length; j++)
		{
			if (_curStepPercent[j] < 1f)
			{
				AnimateStep(j);
			}
			else
			{
				KeepLegGrounded(j);
			}
		}
		ApplyPosToTargets();
	}

	private void UpdateLandingSequence()
	{
		if (!Player.LocalPlayerEnabled)
		{
			return;
		}
		if (!HasSupportingLeg())
		{
			if (!_landingSequenceArmed)
			{
				_timeWithNoGroundedLegs += Time.deltaTime;
				_landingSequenceArmed = _timeWithNoGroundedLegs >= _landingSequenceUngroundedTime;
			}
			return;
		}
		_timeWithNoGroundedLegs = 0f;
		if (_landingSequenceArmed)
		{
			_landingSequenceArmed = false;
			Player nearestAlivePlayer = PlayerManager.GetNearestAlivePlayer(_body.position);
			if ((bool)nearestAlivePlayer && _landingSequence?.Steps != null && _landingSequence.Steps.Length != 0)
			{
				AudioSequenceManager.CancelAllActiveSequencesFromOwner(base.gameObject);
				AudioSequenceManager.PlayPlayerSequence(_landingSequence, nearestAlivePlayer, base.gameObject);
			}
		}
	}

	private bool HasSupportingLeg()
	{
		if (!_spiderCrab || !_spiderCrab.IsGrounded || (bool)_spiderCrab.AttachedRod)
		{
			return false;
		}
		for (int i = 0; i < _legs.Length; i++)
		{
			if (_legs[i] != null && (bool)_legs[i].Target && _curStepPercent[i] >= 1f)
			{
				return true;
			}
		}
		return false;
	}

	public void ResetLegs()
	{
		TryInitialize();
		CaptureLocalOffsetsFromTargets();
		SnapFeetToGround();
		ApplyPosToTargets();
	}

	private void TryInitialize()
	{
		if (!_didInitialize)
		{
			if (!_spiderCrab)
			{
				_spiderCrab = GetComponent<Spidercrab>();
			}
			if (!_body)
			{
				_body = base.transform;
			}
			if (!_moveReference)
			{
				_moveReference = _body;
			}
			if (_legs == null)
			{
				_legs = Array.Empty<Leg>();
			}
			_footPoses = new Vector3[_legs.Length];
			_lastStepPos = new Vector3[_legs.Length];
			_nextStepPos = new Vector3[_legs.Length];
			_curStepPercent = new float[_legs.Length];
			_maxLegLengthAllowedStandingSqr = _maxLegLengthAllowedStanding * _maxLegLengthAllowedStanding;
			_maxLegLengthAllowedWalkingSqr = _maxLegLengthAllowedWalking * _maxLegLengthAllowedWalking;
			_airRepositionDistSqr = _airRepositionDist * _airRepositionDist;
			CaptureLocalOffsetsFromTargets();
			SnapFeetToGround();
			_didInitialize = true;
		}
	}

	private void CaptureLocalOffsetsFromTargets()
	{
		for (int i = 0; i < _legs.Length; i++)
		{
			if (_legs[i] != null && (bool)_legs[i].Target)
			{
				_legs[i].LocalOffset = _body.InverseTransformPoint(_legs[i].Target.position);
			}
		}
	}

	private void SnapFeetToGround()
	{
		for (int i = 0; i < _legs.Length; i++)
		{
			Vector3 vector = ((_legs[i] != null && TryGetNextFootPosition(i, Vector3.zero, out var nextFootPos)) ? nextFootPos : ((_legs[i] != null && (bool)_legs[i].Target) ? _legs[i].Target.position : _body.position));
			_footPoses[i] = vector;
			_lastStepPos[i] = vector;
			_nextStepPos[i] = vector;
			_curStepPercent[i] = 1f;
		}
		_activeLegGroup = -1;
	}

	private void SnapFeetToCurrentTargets()
	{
		if (_footPoses == null)
		{
			return;
		}
		for (int i = 0; i < _legs.Length; i++)
		{
			if (_legs[i] != null && (bool)_legs[i].Target && !(_curStepPercent[i] < 1f))
			{
				_footPoses[i] = _legs[i].Target.position;
				_lastStepPos[i] = _footPoses[i];
				_nextStepPos[i] = _footPoses[i];
			}
		}
	}

	private void TryStartNextLegGroupStep()
	{
		int groupWithLargestStepNeed = GetGroupWithLargestStepNeed();
		if (groupWithLargestStepNeed < 0)
		{
			_activeLegGroup = -1;
		}
		else if (!TryStartGroupStep(groupWithLargestStepNeed))
		{
			int num = 1 - groupWithLargestStepNeed;
			if (!TryStartGroupStep(num))
			{
				_activeLegGroup = -1;
			}
		}
	}

	private int GetGroupWithLargestStepNeed()
	{
		float largestStepNeedForGroup = GetLargestStepNeedForGroup(1);
		float largestStepNeedForGroup2 = GetLargestStepNeedForGroup(0);
		bool flag = largestStepNeedForGroup > 0f;
		bool flag2 = largestStepNeedForGroup2 > 0f;
		if (!flag && !flag2)
		{
			return -1;
		}
		if (!flag)
		{
			return 0;
		}
		if (!flag2)
		{
			return 1;
		}
		if (!(largestStepNeedForGroup > largestStepNeedForGroup2))
		{
			return 0;
		}
		return 1;
	}

	private float GetLargestStepNeedForGroup(int group)
	{
		float num = 0f;
		Vector3 horizontalVelocity = GetHorizontalVelocity();
		for (int i = 0; i < _legs.Length; i++)
		{
			if (i % 2 == group && _legs[i] != null && (bool)_legs[i].Target && TryGetNextFootPosition(i, horizontalVelocity, out var nextFootPos))
			{
				Vector3 vector = _body.InverseTransformVector(nextFootPos - _footPoses[i]);
				float num2 = Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.z));
				float currentStepDistance = GetCurrentStepDistance();
				if (num2 > currentStepDistance && num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private bool TryStartGroupStep(int group)
	{
		bool flag = false;
		Vector3 horizontalVelocity = GetHorizontalVelocity();
		float currentStepDistance = GetCurrentStepDistance();
		for (int i = 0; i < _legs.Length; i++)
		{
			if (i % 2 == group && _legs[i] != null && (bool)_legs[i].Target && TryGetNextFootPosition(i, horizontalVelocity, out var nextFootPos))
			{
				Vector3 vector = _body.InverseTransformVector(nextFootPos - _footPoses[i]);
				if ((vector.z < 0f - currentStepDistance || vector.z > currentStepDistance || vector.x < 0f - currentStepDistance || vector.x > currentStepDistance) && !(nextFootPos == _footPoses[i]))
				{
					_lastStepPos[i] = _footPoses[i];
					_nextStepPos[i] = nextFootPos;
					_curStepPercent[i] = 0f;
					flag = true;
				}
			}
		}
		if (flag)
		{
			_activeLegGroup = group;
		}
		return flag;
	}

	private void AnimateStep(int i)
	{
		_curStepPercent[i] += Time.deltaTime * _walkStepSpeed;
		if (_curStepPercent[i] >= 1f)
		{
			_curStepPercent[i] = 1f;
			_footPoses[i] = _nextStepPos[i];
			float volume = Mathf.Clamp(_spiderCrab.FakeWorldVelocity.sqrMagnitude * 0.025f, 0.1f, 0.6f);
			AudioManager.PlayRandomClipAt("SpidercrabWalk_V", 1, 7, _footPoses[i], variation: true, AudioDistance.Short, volume, 0.05f);
			if (!AnyLegInGroupIsMoving(_activeLegGroup))
			{
				_activeLegGroup = -1;
			}
			return;
		}
		Vector3 horizontalVelocity = GetHorizontalVelocity();
		if (TryGetNextFootPosition(i, horizontalVelocity, out var nextFootPos))
		{
			_nextStepPos[i] = nextFootPos;
		}
		float t = _walkStepMoveCurve.Evaluate(_curStepPercent[i]);
		float num = _walkStepHeightCurve.Evaluate(_curStepPercent[i]);
		float num2 = Mathf.InverseLerp(_smallStepVel, 0f, horizontalVelocity.magnitude);
		num *= 1f - (1f - _smallStepHeightMulti) * num2;
		_footPoses[i] = Vector3.Lerp(_lastStepPos[i], _nextStepPos[i], t);
		_footPoses[i].y += num;
	}

	private void KeepLegGrounded(int i)
	{
		if (_legs[i] != null && (bool)_legs[i].Target && !((_footPoses[i] - _body.position).sqrMagnitude <= _curMaxLegLengthAllowedSqr))
		{
			Vector3 horizontalVelocity = GetHorizontalVelocity();
			if (TryGetNextFootPosition(i, horizontalVelocity, out var nextFootPos))
			{
				_footPoses[i] = nextFootPos;
				_lastStepPos[i] = nextFootPos;
				_nextStepPos[i] = nextFootPos;
			}
		}
	}

	private void ApplyPosToTargets()
	{
		for (int i = 0; i < _legs.Length; i++)
		{
			if (_legs[i] != null && (bool)_legs[i].Target)
			{
				_legs[i].Target.position = _footPoses[i];
			}
		}
	}

	private bool TryGetNextFootPosition(int i, Vector3 velocity, out Vector3 nextFootPos)
	{
		nextFootPos = _footPoses[i];
		if (_legs[i] == null || !_legs[i].Target)
		{
			return false;
		}
		Vector3 localOffset = _legs[i].LocalOffset;
		Vector3 vector = ((velocity.sqrMagnitude > 0.0001f) ? velocity.normalized : Vector3.zero);
		float num = velocity.magnitude * _walkStepDistVelMulti;
		float num2 = _walkStepForwardDist * num;
		if (!Physics.Raycast(_body.position + _moveReference.right * localOffset.x + _moveReference.forward * localOffset.z + vector * num2 + _body.up * _raycastFromUpDist, -_body.up, out var hitInfo, _raycastDist, GameInfo.LevelLayer, QueryTriggerInteraction.Ignore))
		{
			return false;
		}
		if ((hitInfo.point - _body.position).sqrMagnitude > _curMaxLegLengthAllowedSqr)
		{
			return false;
		}
		nextFootPos = hitInfo.point;
		return true;
	}

	private void UpdateAirborneFeet()
	{
		for (int i = 0; i < _legs.Length; i++)
		{
			if (_legs[i] != null && (bool)_legs[i].Target)
			{
				_curStepPercent[i] = 1f;
				Vector3 airborneTargetPos = GetAirborneTargetPos(i);
				if (!((_footPoses[i] - airborneTargetPos).sqrMagnitude <= _airRepositionDistSqr))
				{
					float t = Mathf.Clamp01(Time.deltaTime * _airFollowSpeed);
					Vector3 vector = Vector3.Lerp(_footPoses[i], airborneTargetPos, t);
					_footPoses[i] = vector;
					_lastStepPos[i] = vector;
					_nextStepPos[i] = vector;
				}
			}
		}
		_activeLegGroup = -1;
	}

	private Vector3 GetAirborneTargetPos(int i)
	{
		Vector3 localOffset = _legs[i].LocalOffset;
		return _body.position + _moveReference.right * (localOffset.x * _airSideLocalOffset) + _moveReference.forward * (localOffset.z * _airForwardLocalOffset) - _body.up * _airFootDownOffset;
	}

	private bool AnyLegInGroupIsMoving(int group)
	{
		if (group < 0)
		{
			return false;
		}
		for (int i = 0; i < _legs.Length; i++)
		{
			if (i % 2 == group && _curStepPercent[i] < 1f)
			{
				return true;
			}
		}
		return false;
	}

	private Vector3 GetHorizontalVelocity()
	{
		if (!_spiderCrab)
		{
			return Vector3.zero;
		}
		Vector3 fakeWorldVelocity = _spiderCrab.FakeWorldVelocity;
		fakeWorldVelocity.y = 0f;
		return fakeWorldVelocity;
	}

	private float GetCurrentStepDistance()
	{
		return Mathf.Clamp(GetHorizontalVelocity().magnitude * (_walkStepDistVelMulti * _walkStepDist), _minStepDist, _walkStepDist);
	}

	private void SetMaxLegLength()
	{
		float t = Mathf.InverseLerp(0f, _smallStepVel, GetHorizontalVelocity().magnitude);
		_curMaxLegLengthAllowedSqr = Mathf.Lerp(_maxLegLengthAllowedStandingSqr, _maxLegLengthAllowedWalkingSqr, t);
	}
}
