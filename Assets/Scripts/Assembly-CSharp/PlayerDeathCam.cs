using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerDeathCam : NetworkBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private Camera _deathCam;

	[FormerlySerializedAs("_deathCamRotSpeed")]
	[SerializeField]
	private float _rotSpeed;

	[Header("Controller Look")]
	[SerializeField]
	private Vector2 _controllerLookSpeed = new Vector2(240f, 180f);

	[SerializeField]
	[Range(0f, 0.95f)]
	private float _controllerDeadZone = 0.15f;

	[SerializeField]
	[Min(1f)]
	private float _controllerResponseExponent = 1.6f;

	[SerializeField]
	[Min(0f)]
	private float _controllerLookSmoothTime = 0.04f;

	[Header("Controller Zoom")]
	[SerializeField]
	[Min(0f)]
	private float _controllerZoomSpeed = 3f;

	[FormerlySerializedAs("_deathCamDistance")]
	[SerializeField]
	private float _distance;

	[SerializeField]
	private float _zoomAmountPerScroll = 0.5f;

	[SerializeField]
	private float _zoomSpeed = 10f;

	[SerializeField]
	private float _minZoom = 1.5f;

	[SerializeField]
	private float _maxZoom = 8f;

	[FormerlySerializedAs("_deathCamColOffset")]
	[SerializeField]
	private float _colOffset;

	[FormerlySerializedAs("_deathCamDistSpeed")]
	[SerializeField]
	private float _distSpeed;

	private Vector3 _smoothDeadPlayerPos;

	private Vector3 _targetRot;

	private Vector3 _curRot;

	private Vector2 _lookInput;

	private Vector2 _rawLookInput;

	private Vector2 _controllerLookInput;

	private Vector2 _controllerLookRefVel;

	private float _controllerZoomInput;

	private float _curDist;

	private float _targetDist;

	private bool NetworkInitialize___EarlyPlayerDeathCamAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerDeathCamAssembly_002DCSharp_002Edll_Excuted;

	private void LateUpdate()
	{
		if (base.Owner.IsLocalClient && (bool)_player.Dying.DeadPlayer)
		{
			ControllerRotation();
			ControllerZoom();
			OrbitCamera();
		}
	}

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			BindInputs();
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
		input.actions["PlayerLook"].performed += MouseInput;
		input.actions["PlayerLook"].canceled += MouseInput;
		input.actions["PlayerMove"].performed += ControllerZoomInput;
		input.actions["PlayerMove"].canceled += ControllerZoomInput;
		input.actions["InventoryScroll"].performed += ScrollInput;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["PlayerLook"].performed -= MouseInput;
			input.actions["PlayerLook"].canceled -= MouseInput;
			input.actions["PlayerMove"].performed -= ControllerZoomInput;
			input.actions["PlayerMove"].canceled -= ControllerZoomInput;
			input.actions["InventoryScroll"].performed -= ScrollInput;
		}
	}

	private void MouseInput(InputAction.CallbackContext context)
	{
		if (PauseManager.IsPaused || !_player.Dying.IsDead || !_player.Camera.MouseLocked)
		{
			_lookInput = Vector2.zero;
			_rawLookInput = Vector2.zero;
			_controllerLookInput = Vector2.zero;
			return;
		}
		Vector2 rawLookInput = context.ReadValue<Vector2>();
		if (context.control?.device is Gamepad)
		{
			_rawLookInput = rawLookInput;
			return;
		}
		_lookInput = new Vector2(0f - rawLookInput.y, rawLookInput.x) * _player.Camera.Sensitivity * 0.025f;
		_targetRot += (Vector3)_lookInput;
	}

	private void ControllerRotation()
	{
		if (GameInfo.Input.currentControlScheme != "Controller")
		{
			return;
		}
		if (PauseManager.IsPaused || !_player.Dying.IsDead || !_player.Camera.MouseLocked)
		{
			_lookInput = Vector2.zero;
			_controllerLookInput = Vector2.zero;
			return;
		}
		Vector2 vector = ApplyControllerLookCurve(_rawLookInput);
		if (_controllerLookSmoothTime > 0f)
		{
			_controllerLookInput = Vector2.SmoothDamp(_controllerLookInput, vector, ref _controllerLookRefVel, _controllerLookSmoothTime);
		}
		else
		{
			_controllerLookInput = vector;
		}
		if (_controllerLookInput.sqrMagnitude < 0.0001f)
		{
			_lookInput = Vector2.zero;
			return;
		}
		_lookInput = new Vector2((0f - _controllerLookInput.y) * _controllerLookSpeed.y, _controllerLookInput.x * _controllerLookSpeed.x) * (_player.Camera.Sensitivity * Time.deltaTime);
		_targetRot += (Vector3)_lookInput;
	}

	private Vector2 ApplyControllerLookCurve(Vector2 input)
	{
		float magnitude = input.magnitude;
		if (magnitude <= _controllerDeadZone)
		{
			return Vector2.zero;
		}
		float num = Mathf.Pow(Mathf.InverseLerp(_controllerDeadZone, 1f, Mathf.Clamp01(magnitude)), _controllerResponseExponent);
		return input.normalized * num;
	}

	private void ControllerZoomInput(InputAction.CallbackContext context)
	{
		if (GameInfo.Input.currentControlScheme != "Controller" || PauseManager.IsPaused || !_player.Dying.IsDead)
		{
			_controllerZoomInput = 0f;
		}
		else
		{
			_controllerZoomInput = context.ReadValue<Vector2>().y;
		}
	}

	private void ControllerZoom()
	{
		if (GameInfo.Input.currentControlScheme != "Controller" || PauseManager.IsPaused || !_player.Dying.IsDead)
		{
			_controllerZoomInput = 0f;
		}
		else
		{
			_targetDist = Mathf.Clamp(_targetDist - _controllerZoomInput * _controllerZoomSpeed * Time.deltaTime, _minZoom, _maxZoom);
		}
	}

	private void ScrollInput(InputAction.CallbackContext context)
	{
		if (!PauseManager.IsPaused && _player.Dying.IsDead)
		{
			float num = context.ReadValue<float>();
			if (!Mathf.Approximately(num, 0f))
			{
				_targetDist -= num * _zoomAmountPerScroll;
				_targetDist = Mathf.Clamp(_targetDist, _minZoom, _maxZoom);
			}
		}
	}

	public void EnableDeathCam()
	{
		_deathCam.gameObject.SetActive(value: true);
		_player.SetCurCam(_deathCam);
		_deathCam.transform.position = _player.CamObject.position;
		_deathCam.transform.rotation = _player.CamObject.rotation;
		_targetRot = _player.CamObject.eulerAngles;
		_curRot = _player.CamObject.eulerAngles;
		_curDist = 0f;
		_targetDist = Mathf.Clamp(_distance, _minZoom, _maxZoom);
	}

	public void DisableDeathCam()
	{
		_player.SetCurCam(_player.Camera.Cam);
		_deathCam.gameObject.SetActive(value: false);
	}

	private void OrbitCamera()
	{
		_curDist = Mathf.Lerp(_curDist, _targetDist, _zoomSpeed * Time.deltaTime);
		_targetRot.x = Mathf.Clamp(_targetRot.x, -90f, 90f);
		_curRot = Quaternion.Slerp(Quaternion.Euler(_curRot), Quaternion.Euler(_targetRot), _rotSpeed * Time.deltaTime).eulerAngles;
		Vector3 position = _player.Dying.DeadPlayer.transform.position;
		Quaternion quaternion = Quaternion.Euler(_curRot.x, _curRot.y, 0f);
		Vector3 vector = position + quaternion * new Vector3(0f, 0f, 0f - _curDist);
		Vector3 vector2 = vector - position;
		float magnitude = vector2.magnitude;
		if (Physics.Raycast(position, vector2.normalized, out var hitInfo, magnitude, GameInfo.LevelLayer))
		{
			vector = position + vector2.normalized * (hitInfo.distance - _colOffset);
		}
		_deathCam.transform.position = vector;
		_deathCam.transform.rotation = quaternion;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerDeathCamAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerDeathCamAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerDeathCamAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerDeathCamAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}
}
