using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLegs : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[Header("Assign right then left in all arrays")]
	[SerializeField]
	private Transform[] _legTargets;

	[SerializeField]
	private Transform[] _ikPoles;

	[SerializeField]
	private Transform[] _footModels;

	[SerializeField]
	private float _spaceBetweenFeet;

	[SerializeField]
	private float _raycastDistFromCam;

	[SerializeField]
	private float _raycastFromUpDist;

	[SerializeField]
	private float _minStepDist;

	[SerializeField]
	private float _maxLegLengthAllowedStanding;

	[SerializeField]
	private float _maxLegLengthAllowedWalking;

	[Header("Walking")]
	[FormerlySerializedAs("_stepForwardDist")]
	[SerializeField]
	private float _walkStepForwardDist;

	[FormerlySerializedAs("_stepDist")]
	[SerializeField]
	private float _walkStepDist;

	[FormerlySerializedAs("_stepDistVelMulti")]
	[SerializeField]
	private float _walkStepDistVelMulti;

	[FormerlySerializedAs("_stepHeightCurve")]
	[SerializeField]
	private AnimationCurve _walkStepHeightCurve;

	[SerializeField]
	private AnimationCurve _walkStepMoveCurve;

	[FormerlySerializedAs("_stepSpeed")]
	[SerializeField]
	private float _walkStepSpeed;

	[SerializeField]
	[Tooltip("Below this velocity, feet will only be lifted a small amount")]
	private float _smallStepVel;

	[SerializeField]
	[Range(0f, 1f)]
	private float _smallStepHeightMulti;

	[Header("Sprinting")]
	[SerializeField]
	private float _minVelForSprint;

	[SerializeField]
	private float _sprintStepForwardDist;

	[SerializeField]
	private float _sprintStepDist;

	[SerializeField]
	private float _sprintStepDistVelMulti;

	[SerializeField]
	private AnimationCurve _sprintStepHeightCurve;

	[SerializeField]
	private AnimationCurve _sprintStepMoveCurve;

	[SerializeField]
	private float _sprintStepSpeed;

	[SerializeField]
	private float _sprintFootMoveOverlapPercent;

	[SerializeField]
	private float _sprintTransitionSpeed;

	[Header("Jumping")]
	[SerializeField]
	private float _jumpLerpSpeed;

	[SerializeField]
	[Tooltip("Height difference from player center where feet should be during a jump")]
	private Vector3[] _jumpFeetPos;

	[FormerlySerializedAs("_landingLerpSpeed")]
	[SerializeField]
	private float _landingStepSpeed;

	[Header("Foot rotations")]
	[SerializeField]
	private float _footRotSpeed;

	[SerializeField]
	private Vector3[] _footRotOffsets;

	private readonly Quaternion[] _defaultFootRots = new Quaternion[2];

	private readonly Quaternion[] _curGroundRots = new Quaternion[2];

	private readonly Vector3[] _footPoses = new Vector3[2];

	private readonly Vector3[] _lastStepPos = new Vector3[2];

	private readonly Vector3[] _nextStepPos = new Vector3[2];

	private readonly float[] _curStepPercent = new float[2];

	private readonly bool[] _jumpPosIsLast = new bool[2];

	private Vector3 _jumpFeetVelOffset;

	private Transform _positionSpace;

	private bool _isJumping;

	private bool _isSprinting;

	private bool _isLanding;

	private int _latestFootToStartStepIndex;

	private float _curSprintPercent;

	private float _timeOfLastGrounded;

	private float _maxLegLengthAllowedStandingSqr;

	private float _maxLegLengthAllowedWalkingSqr;

	private float _curMaxLegLengthAllowedSqr;

	private bool _grounded => _player.Other.Grounded;

	private void Awake()
	{
		_maxLegLengthAllowedStandingSqr = _maxLegLengthAllowedStanding * _maxLegLengthAllowedStanding;
		_maxLegLengthAllowedWalkingSqr = _maxLegLengthAllowedWalking * _maxLegLengthAllowedWalking;
		for (int i = 0; i < 2; i++)
		{
			_footPoses[i] = _legTargets[i].position;
			_defaultFootRots[i] = _footModels[i].localRotation;
			_curStepPercent[i] = 1f;
		}
		SyncPositionSpace();
	}

	private void Update()
	{
		SyncPositionSpace();
		if (_grounded)
		{
			if (_isJumping)
			{
				Land();
			}
			_timeOfLastGrounded = Time.time;
			SetIsSprinting();
			SetMaxLegLength();
			for (int i = 0; i < 2; i++)
			{
				if (_curStepPercent[i] >= 1f)
				{
					TryGetNextFootPosition(i, out var nextFootPos);
					TryGetNextFootPosition(1 - i, out var nextFootPos2);
					float sqrMagnitude = (_footPoses[i] - nextFootPos).sqrMagnitude;
					float sqrMagnitude2 = (_footPoses[1 - i] - nextFootPos2).sqrMagnitude;
					bool flag = sqrMagnitude > sqrMagnitude2;
					bool num = _curStepPercent[1 - i] >= 1f;
					bool flag2 = _isSprinting && _curStepPercent[1 - i] > _sprintFootMoveOverlapPercent;
					if ((num | flag2) & flag)
					{
						SetNextFootPos(i);
					}
					else
					{
						SetFootToJumpOrLastPos(i);
					}
				}
				else
				{
					AnimateStep(i);
				}
			}
		}
		else
		{
			_isJumping = true;
			for (int j = 0; j < 2; j++)
			{
				_footPoses[j] = GetJumpPos(j);
			}
		}
		ApplyPosToTargets();
		for (int k = 0; k < 2; k++)
		{
			ApplyRotToFoot(k);
		}
	}

	public void Reset()
	{
		SyncPositionSpace();
		for (int i = 0; i < 2; i++)
		{
			if (TryGetNextFootPosition(i, out var nextFootPos))
			{
				_footPoses[i] = nextFootPos;
				_lastStepPos[i] = nextFootPos;
				_nextStepPos[i] = nextFootPos;
			}
			else
			{
				_footPoses[i] = GetJumpPos(i);
				_lastStepPos[i] = _footPoses[i];
				_nextStepPos[i] = _footPoses[i];
			}
			_curStepPercent[i] = 1f;
			SetFootRotToDefault(i);
		}
		ApplyPosToTargets();
		for (int j = 0; j < 2; j++)
		{
			ApplyRotToFoot(j);
		}
	}

	private void SetNextFootPos(int i)
	{
		if (!TryGetNextFootPosition(i, out var nextFootPos))
		{
			SetFootToJumpOrLastPos(i);
			return;
		}
		Vector3 position = ToWorldPosition(nextFootPos);
		Vector3 vector = _legTargets[i].InverseTransformPoint(position);
		float velMag = _player.Other.VelMag;
		velMag *= Mathf.Lerp(_walkStepDistVelMulti * _walkStepDist, _sprintStepDistVelMulti * _sprintStepDist, _curSprintPercent);
		float max = Mathf.Lerp(_walkStepDist, _sprintStepDist, _curSprintPercent);
		velMag = Mathf.Clamp(velMag, _minStepDist, max);
		if ((vector.z < 0f - velMag || vector.z > velMag || vector.x < 0f - velMag || vector.x > velMag) && nextFootPos != _footPoses[i])
		{
			_latestFootToStartStepIndex = i;
			_lastStepPos[i] = _footPoses[i];
			_nextStepPos[i] = nextFootPos;
			_curStepPercent[i] = 0f;
		}
	}

	private void AnimateStep(int i)
	{
		float num = Mathf.Lerp(_walkStepSpeed, _sprintStepSpeed, _curSprintPercent);
		if (_isLanding)
		{
			num = _landingStepSpeed;
		}
		if (_curStepPercent[i] < 1f)
		{
			_curStepPercent[i] += Time.deltaTime * num;
			if (_curStepPercent[i] >= 1f)
			{
				if (_isLanding)
				{
					_isLanding = false;
				}
				_footPoses[i] = _nextStepPos[i];
				Physics.Raycast(_footPoses[i] + Vector3.up * 0.1f, Vector3.down, out var hitInfo, 0.2f, GameInfo.CanJumpOnLayers);
				AudioManager.PlayRandomPlayerClip(SurfaceManager.GetStepSound(hitInfo.transform, _footPoses[i]), 1, GameInfo.StepSoundCount, _player, variation: true, AudioDistance.VeryShort, _isSprinting ? 0.15f : ((_player.Other.VelMag <= _smallStepVel) ? 0.03f : 0.075f));
				return;
			}
		}
		if (!TryGetNextFootPosition(i, out var nextFootPos))
		{
			SetFootToJumpOrLastPos(i);
			return;
		}
		float t = Mathf.Lerp(_walkStepMoveCurve.Evaluate(_curStepPercent[i]), _sprintStepMoveCurve.Evaluate(_curStepPercent[i]), _curSprintPercent);
		float num2 = Mathf.Lerp(_walkStepHeightCurve.Evaluate(_curStepPercent[i]), _sprintStepHeightCurve.Evaluate(_curStepPercent[i]), _curSprintPercent);
		float num3 = Mathf.InverseLerp(_smallStepVel, 0f, _player.Other.VelMag);
		num2 *= 1f - (1f - _smallStepHeightMulti) * num3;
		if (_isLanding)
		{
			num2 = 0f;
		}
		_nextStepPos[i] = nextFootPos;
		_footPoses[i] = Vector3.Lerp(_lastStepPos[i], _nextStepPos[i], t);
		_footPoses[i].y += num2;
	}

	private void ApplyRotToFoot(int i)
	{
		Quaternion b = _curGroundRots[i];
		Quaternion b2 = Quaternion.Slerp(_defaultFootRots[i], b, 1f - _sprintStepHeightCurve.Evaluate(_curStepPercent[i]));
		_footModels[i].localRotation = Quaternion.Slerp(_footModels[i].localRotation, b2, _footRotSpeed * Time.deltaTime);
	}

	private void SetFootRot(int i, Vector3 groundNormal)
	{
		Vector3 forward = Vector3.ProjectOnPlane(_player.Body.LowerBody.forward, groundNormal).normalized;
		if (forward.sqrMagnitude < 0.001f)
		{
			forward = Vector3.Cross(_player.Body.LowerBody.right, groundNormal);
		}
		_curGroundRots[i] = Quaternion.Inverse(_footModels[i].parent.rotation) * Quaternion.LookRotation(forward, groundNormal) * Quaternion.Euler(_footRotOffsets[i]);
	}

	private void SetFootRotToDefault(int i)
	{
		_curGroundRots[i] = _defaultFootRots[i];
	}

	private void SetFootToJumpOrLastPos(int i)
	{
		Vector3 vector = ToWorldPosition(_footPoses[i]);
		if (_jumpPosIsLast[i] || (vector - _player.Body.LowerBody.position).sqrMagnitude > _curMaxLegLengthAllowedSqr)
		{
			_footPoses[i] = GetJumpPos(i);
			SetFootRotToDefault(i);
		}
	}

	private Vector3 GetJumpPos(int i)
	{
		Vector3 b = Vector3.right * ((i == 0) ? _spaceBetweenFeet : (0f - _spaceBetweenFeet));
		b += _jumpFeetPos[(_latestFootToStartStepIndex == 0) ? i : (1 - i)];
		Vector3 position = Vector3.Lerp(_player.Transform.InverseTransformPoint(ToWorldPosition(_footPoses[i])), b, _jumpLerpSpeed * Time.deltaTime);
		_jumpPosIsLast[i] = true;
		Vector3 worldPos = _player.Transform.TransformPoint(position);
		SetFootRotToDefault(i);
		return ToStoragePosition(worldPos);
	}

	private void Land()
	{
		if (Time.time > _timeOfLastGrounded + 0.25f)
		{
			SwitchFoot();
		}
		_isJumping = false;
		_isLanding = true;
		for (int i = 0; i < 2; i++)
		{
			_curStepPercent[i] = 0f;
			if (!TryGetNextFootPosition(i, out var nextFootPos))
			{
				_lastStepPos[i] = GetJumpPos(i);
				_nextStepPos[i] = _lastStepPos[i];
			}
			else
			{
				_lastStepPos[i] = GetJumpPos(i);
				_nextStepPos[i] = nextFootPos;
			}
		}
	}

	public void SwitchFoot()
	{
		_latestFootToStartStepIndex = 1 - _latestFootToStartStepIndex;
	}

	private void ApplyPosToTargets()
	{
		for (int i = 0; i < 2; i++)
		{
			Vector3 vector = ToWorldPosition(_footPoses[i]);
			_legTargets[i].position = vector;
			Vector3 normalized = (vector - _player.Body.LowerBody.position).normalized;
			Vector3 vector2 = Vector3.Lerp(vector, _player.Body.LowerBody.position, 0.5f);
			_ikPoles[i].position = vector2 + Vector3.Cross(normalized, _player.Body.LowerBody.right);
		}
	}

	private bool TryGetNextFootPosition(int i, out Vector3 nextFootPos)
	{
		nextFootPos = _footPoses[i];
		Vector3 normalized = new Vector3(_player.Other.Velocity.x, 0f, _player.Other.Velocity.z).normalized;
		float num = Mathf.Lerp(_walkStepDistVelMulti, _sprintStepDistVelMulti, _curSprintPercent);
		float num2 = Mathf.Lerp(_walkStepForwardDist, _sprintStepForwardDist, _curSprintPercent);
		float num3 = _player.Other.VelMag * num;
		float num4 = num2 * num3;
		Vector3 vector = ((i == 0) ? _player.CamObject.right : (-_player.CamObject.right)) * _spaceBetweenFeet;
		if (Physics.Raycast(_player.Body.LowerBody.position + normalized * num4 + vector + Vector3.up * _raycastFromUpDist, Vector3.down, out var hitInfo, _raycastDistFromCam, GameInfo.LegStepOnLayers, QueryTriggerInteraction.Ignore))
		{
			if ((hitInfo.point - _player.Body.LowerBody.position).sqrMagnitude > _curMaxLegLengthAllowedSqr)
			{
				return false;
			}
			nextFootPos = ToStoragePosition(hitInfo.point);
			SetFootRot(i, hitInfo.normal);
			_jumpPosIsLast[i] = false;
			return true;
		}
		return false;
	}

	private void SyncPositionSpace()
	{
		Transform transform = (_player.Other.OnBoat ? BoatManager.Boat.VisualBoat : null);
		if (!(_positionSpace == transform))
		{
			for (int i = 0; i < 2; i++)
			{
				_footPoses[i] = ConvertPosition(_footPoses[i], _positionSpace, transform);
				_lastStepPos[i] = ConvertPosition(_lastStepPos[i], _positionSpace, transform);
				_nextStepPos[i] = ConvertPosition(_nextStepPos[i], _positionSpace, transform);
			}
			_positionSpace = transform;
		}
	}

	private Vector3 ToStoragePosition(Vector3 worldPos)
	{
		return ConvertPosition(worldPos, null, _positionSpace);
	}

	private Vector3 ToWorldPosition(Vector3 storedPos)
	{
		return ConvertPosition(storedPos, _positionSpace, null);
	}

	private Vector3 ConvertPosition(Vector3 position, Transform fromSpace, Transform toSpace)
	{
		Vector3 vector = (fromSpace ? fromSpace.TransformPoint(position) : position);
		if (!toSpace)
		{
			return vector;
		}
		return toSpace.InverseTransformPoint(vector);
	}

	private void SetIsSprinting()
	{
		_isSprinting = _player.Other.VelMag >= _minVelForSprint;
		_curSprintPercent = Mathf.MoveTowards(_curSprintPercent, _isSprinting ? 1 : 0, _sprintTransitionSpeed * Time.deltaTime);
	}

	private void SetMaxLegLength()
	{
		float t = Mathf.InverseLerp(0f, _smallStepVel, _player.Other.VelMag);
		_curMaxLegLengthAllowedSqr = Mathf.Lerp(_maxLegLengthAllowedStandingSqr, _maxLegLengthAllowedWalkingSqr, t);
	}

	public float BodyBobDownPercent()
	{
		if (_player.Other.VelMag < _smallStepVel)
		{
			return 0f;
		}
		if (_isSprinting)
		{
			return (_curStepPercent[0] >= 1f || _curStepPercent[1] >= 1f) ? 1 : 0;
		}
		float num = 0f;
		for (int i = 0; i < 2; i++)
		{
			float num2 = _curStepPercent[i];
			num2 -= 0.5f;
			num2 = Mathf.Abs(num2);
			num2 *= 2f;
			num += num2;
		}
		return num - 1f;
	}
}
