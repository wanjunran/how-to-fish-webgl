using UnityEngine;

public class BobbleManager : MonoBehaviour
{
	[SerializeField]
	private float _amount;

	[SerializeField]
	private Rigidbody _rig;

	[SerializeField]
	private float _speed = 100f;

	[SerializeField]
	private float _damping = 10f;

	[SerializeField]
	private Transform[] _bobbleObjects;

	private Quaternion[] _restRotations;

	private Quaternion[] _currentRotations;

	private Vector3[] _angularVelocities;

	private Vector3 _previousVelocity;

	private Vector3 _acceleration;

	private void Awake()
	{
		InitializeBobbleObjects();
	}

	private void OnEnable()
	{
		ResetBobbleObjects();
		_previousVelocity = (_rig ? _rig.linearVelocity : Vector3.zero);
		_acceleration = Vector3.zero;
	}

	private void OnDisable()
	{
		ResetBobbleObjects();
	}

	private void FixedUpdate()
	{
		if ((bool)_rig)
		{
			Vector3 linearVelocity = _rig.linearVelocity;
			_acceleration = (linearVelocity - _previousVelocity) / Time.fixedDeltaTime;
			_previousVelocity = linearVelocity;
		}
	}

	private void LateUpdate()
	{
		if (!_rig || _bobbleObjects == null)
		{
			return;
		}
		if (_currentRotations == null || _currentRotations.Length != _bobbleObjects.Length)
		{
			InitializeBobbleObjects();
		}
		for (int i = 0; i < _bobbleObjects.Length; i++)
		{
			Transform transform = _bobbleObjects[i];
			if ((bool)transform)
			{
				Vector3 vector = (transform.parent ? transform.parent.InverseTransformDirection(_acceleration) : _acceleration);
				Quaternion targetRot = Quaternion.Euler(new Vector3(0f - vector.z, 0f, vector.x) * _amount) * _restRotations[i];
				DazedUtils.SimulateSpringRotation(ref _currentRotations[i], ref _angularVelocities[i], targetRot, _speed, _damping);
				transform.localRotation = _currentRotations[i];
			}
		}
	}

	private void InitializeBobbleObjects()
	{
		Transform[] bobbleObjects = _bobbleObjects;
		int num = ((bobbleObjects != null) ? bobbleObjects.Length : 0);
		_restRotations = new Quaternion[num];
		_currentRotations = new Quaternion[num];
		_angularVelocities = new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			Transform transform = _bobbleObjects[i];
			if ((bool)transform)
			{
				_restRotations[i] = transform.localRotation;
				_currentRotations[i] = transform.localRotation;
			}
		}
	}

	private void ResetBobbleObjects()
	{
		if (_bobbleObjects == null || _restRotations == null || _restRotations.Length != _bobbleObjects.Length)
		{
			InitializeBobbleObjects();
			return;
		}
		for (int i = 0; i < _bobbleObjects.Length; i++)
		{
			Transform transform = _bobbleObjects[i];
			if ((bool)transform)
			{
				transform.localRotation = _restRotations[i];
				_currentRotations[i] = _restRotations[i];
				_angularVelocities[i] = Vector3.zero;
			}
		}
	}
}
