using UnityEngine;
using UnityEngine.Serialization;

public class IK : MonoBehaviour
{
	public Transform Target;

	[SerializeField]
	[Tooltip("Offset from target, position in targets local space")]
	private Vector3 _targetOffset;

	[SerializeField]
	[Tooltip("Chain length of bones")]
	private int _chainLength = 2;

	[SerializeField]
	private Transform _pole;

	[FormerlySerializedAs("_shouldStretch")]
	[SerializeField]
	private bool _enableStretch;

	[Header("Solver Parameters")]
	[SerializeField]
	[Tooltip("Solver iterations per update")]
	private int _iterations = 5;

	[SerializeField]
	[Tooltip("Distance when the solver stops")]
	private float _delta = 0.001f;

	[Range(0f, 1f)]
	[SerializeField]
	[Tooltip("Strength of going back to the start position")]
	private float _snapBackStrength = 1f;

	private float[] _bonesLength;

	private float[] _origBonesLength;

	private float _origCompleteLengthSqr;

	private Transform[] _bones;

	private Vector3[] _positions;

	private Vector3[] _startDirectionSucc;

	private Vector3 _lastPos;

	private Quaternion[] _startRotationBone;

	private Transform _root;

	private bool _stopResolving;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		_bones = new Transform[_chainLength + 1];
		_positions = new Vector3[_chainLength + 1];
		_bonesLength = new float[_chainLength];
		_origBonesLength = new float[_chainLength];
		_startDirectionSucc = new Vector3[_chainLength + 1];
		_startRotationBone = new Quaternion[_chainLength + 1];
		_root = base.transform;
		for (int i = 0; i <= _chainLength; i++)
		{
			if (!_root)
			{
				throw new UnityException("The chain value is longer than the ancestor chain!");
			}
			_root = _root.parent;
		}
		if (!Target)
		{
			Target = new GameObject(base.gameObject.name + " Target").transform;
			SetPositionRootSpace(Target, GetPositionRootSpace(base.transform));
		}
		Transform parent = base.transform;
		float num = 0f;
		for (int num2 = _bones.Length - 1; num2 >= 0; num2--)
		{
			_bones[num2] = parent;
			_startRotationBone[num2] = GetRotationRootSpace(parent);
			if (num2 == _bones.Length - 1)
			{
				_startDirectionSucc[num2] = GetPositionRootSpace(Target) - GetPositionRootSpace(parent);
			}
			else
			{
				_startDirectionSucc[num2] = GetPositionRootSpace(_bones[num2 + 1]) - GetPositionRootSpace(parent);
				_bonesLength[num2] = _startDirectionSucc[num2].magnitude;
				_origBonesLength[num2] = _bonesLength[num2];
				num += _bonesLength[num2];
			}
			parent = parent.parent;
		}
		_origCompleteLengthSqr = num * num;
	}

	private void LateUpdate()
	{
		ResolveIK();
	}

	private void ResolveIK(bool hasStretched = false)
	{
		if (!Target)
		{
			return;
		}
		if (_bonesLength.Length != _chainLength)
		{
			Init();
		}
		for (int i = 0; i < _bones.Length; i++)
		{
			_positions[i] = GetPositionRootSpace(_bones[i]);
		}
		Vector3 positionRootSpace = GetPositionRootSpace(Target.position + Target.rotation * _targetOffset);
		if (!hasStretched && Mathf.Approximately((positionRootSpace - _lastPos).sqrMagnitude, 0f))
		{
			if (_stopResolving)
			{
				return;
			}
			_stopResolving = true;
		}
		else
		{
			_stopResolving = false;
		}
		_lastPos = positionRootSpace;
		if ((positionRootSpace - GetPositionRootSpace(_bones[0])).sqrMagnitude >= _origCompleteLengthSqr)
		{
			if (!hasStretched && _enableStretch)
			{
				float num = Vector3.Distance(_positions[1], positionRootSpace) / _origBonesLength[1];
				Vector3 one = Vector3.one;
				one.y = num;
				Vector3 one2 = Vector3.one;
				one2.y = 1f / num;
				_bones[1].localScale = one;
				_bones[2].localScale = one2;
				_bonesLength[1] = _origBonesLength[1] * num;
				ResolveIK(hasStretched: true);
				return;
			}
			Vector3 normalized = (positionRootSpace - _positions[0]).normalized;
			for (int j = 1; j < _positions.Length; j++)
			{
				_positions[j] = _positions[j - 1] + normalized * _bonesLength[j - 1];
			}
		}
		else
		{
			if (_bonesLength[1] != _origBonesLength[1])
			{
				_bones[1].localScale = Vector3.one;
				_bones[2].localScale = Vector3.one;
				_bonesLength[1] = _origBonesLength[1];
			}
			for (int k = 0; k < _positions.Length - 1; k++)
			{
				_positions[k + 1] = Vector3.Lerp(_positions[k + 1], _positions[k] + _startDirectionSucc[k], _snapBackStrength);
			}
			for (int l = 0; l < _iterations; l++)
			{
				for (int num2 = _positions.Length - 1; num2 > 0; num2--)
				{
					if (num2 == _positions.Length - 1)
					{
						_positions[num2] = positionRootSpace;
					}
					else
					{
						_positions[num2] = _positions[num2 + 1] + (_positions[num2] - _positions[num2 + 1]).normalized * _bonesLength[num2];
					}
				}
				for (int m = 1; m < _positions.Length; m++)
				{
					_positions[m] = _positions[m - 1] + (_positions[m] - _positions[m - 1]).normalized * _bonesLength[m - 1];
				}
				if ((_positions[_positions.Length - 1] - positionRootSpace).sqrMagnitude < _delta * _delta)
				{
					break;
				}
			}
		}
		if ((bool)_pole)
		{
			Vector3 positionRootSpace2 = GetPositionRootSpace(_pole);
			for (int n = 1; n < _positions.Length - 1; n++)
			{
				Plane plane = new Plane(_positions[n + 1] - _positions[n - 1], _positions[n - 1]);
				Vector3 vector = plane.ClosestPointOnPlane(positionRootSpace2);
				float angle = Vector3.SignedAngle(plane.ClosestPointOnPlane(_positions[n]) - _positions[n - 1], vector - _positions[n - 1], plane.normal);
				_positions[n] = Quaternion.AngleAxis(angle, plane.normal) * (_positions[n] - _positions[n - 1]) + _positions[n - 1];
			}
		}
		for (int num3 = 0; num3 < _positions.Length; num3++)
		{
			if (num3 != _positions.Length - 1)
			{
				SetRotationRootSpace(_bones[num3], Quaternion.FromToRotation(_startDirectionSucc[num3], _positions[num3 + 1] - _positions[num3]) * Quaternion.Inverse(_startRotationBone[num3]));
			}
			if (num3 != 0)
			{
				SetPositionRootSpace(_bones[num3], _positions[num3]);
			}
		}
	}

	private Vector3 GetPositionRootSpace(Transform current)
	{
		if (!_root)
		{
			return current.position;
		}
		return Quaternion.Inverse(_root.rotation) * (current.position - _root.position);
	}

	private Vector3 GetPositionRootSpace(Vector3 current)
	{
		if (!_root)
		{
			return current;
		}
		return Quaternion.Inverse(_root.rotation) * (current - _root.position);
	}

	private void SetPositionRootSpace(Transform current, Vector3 position)
	{
		if (!_root)
		{
			current.position = position;
		}
		else
		{
			current.position = _root.rotation * position + _root.position;
		}
	}

	private Quaternion GetRotationRootSpace(Transform current)
	{
		if (!_root)
		{
			return current.rotation;
		}
		return Quaternion.Inverse(current.rotation) * _root.rotation;
	}

	private void SetRotationRootSpace(Transform current, Quaternion rotation)
	{
		if (!_root)
		{
			current.rotation = rotation;
		}
		else
		{
			current.rotation = _root.rotation * rotation;
		}
	}

	private void OnDrawGizmos()
	{
	}
}
