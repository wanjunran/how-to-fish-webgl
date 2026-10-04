using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : NetworkBehaviour
{
	[SerializeField]
	private Player _player;

	[Header("Camera Variables")]
	[SerializeField]
	private float _tiltSpeed = 0.15f;

	[SerializeField]
	private float _walkTiltAmount = 1f;

	[SerializeField]
	private float _fallTiltAmount = 0.25f;

	[SerializeField]
	private float _fallTiltSpeed = 0.08f;

	[SerializeField]
	private float _screenShakeRotMulti = 10f;

	[SerializeField]
	private Camera _cam;

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

	[Header("Head Bobbing")]
	[SerializeField]
	private float _headBobSpeed = 3f;

	[SerializeField]
	private float _headBobMinVelocity = 0.1f;

	[SerializeField]
	private float _headBobMaxVelocity = 7f;

	[SerializeField]
	private float _headBobHorizontalAmount = 0.6f;

	[SerializeField]
	private float _headBobVerticalAmount = 0.6f;

	[SerializeField]
	private float _headBobSmoothTime = 0.08f;

	[SerializeField]
	private float _headBobStepTriggerY = -0.02f;

	[SerializeField]
	private float _headBobDelaySpeed;

	[SerializeField]
	private AnimationCurve _headBobHorizontalCurve = new AnimationCurve();

	[SerializeField]
	private AnimationCurve _headBobVerticalCurve = new AnimationCurve();

	private Vector3 _smoothPos;

	private Vector3 _bobPos;

	private Vector3 _delayedBobPos;

	private Vector3 _moveRot;

	private Vector3 _rot;

	private Vector2 _rawLookInput;

	private Vector2 _controllerLookInput;

	private Vector2 _controllerLookRefVel;

	private Vector2 _refVel;

	private Vector2 _recoilTar;

	private Vector2 _recoilCur;

	private float _refTilt;

	private float _refVelX;

	private float _recoilSpeed = 25f;

	private float _curFov;

	private float _refFovVel;

	private float _headBobTime;

	private bool _headBobStepArmed;

	private static bool _invertX;

	private static bool _invertY;

	private static bool _viewBobbingEnabled = true;

	private static bool _mouseLocked;

	private static float _sensitivity = 0.5f;

	private static float _origFov;

	private bool NetworkInitialize___EarlyPlayerCameraAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerCameraAssembly_002DCSharp_002Edll_Excuted;

	public float HorizontalInput { get; private set; }

	public float VelY { get; private set; }

	public float CurVel { get; private set; }

	public float SensMulti { get; private set; }

	public Vector2 ShakePos { get; private set; }

	public Vector2 LookInput { get; private set; }

	public float CamHeight { get; private set; }

	public Vector3 CamPosNoEffects { get; private set; }

	public float TiltSpeed => _tiltSpeed;

	public Transform CamTransform => _cam.transform;

	public Camera Cam => _cam;

	public Vector3 BobPos => _bobPos;

	public bool MouseLocked => _mouseLocked;

	public float Sensitivity => _sensitivity;

	public static event Action OnPlayerRotated;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_PlayerCamera_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			BindInputs();
			ToggleMouse(unlock: false);
			GameInfo.SetCam(_cam);
		}
	}

	public override void OnStopClient()
	{
		if (base.Owner.IsLocalClient)
		{
			UnbindInputs();
			ToggleMouse(unlock: true);
		}
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["PlayerLook"].performed += MouseInput;
		input.actions["PlayerLook"].canceled += MouseInput;
		input.actions["PlayerLeftClick"].performed += MouseClick;
		input.actions["PlayerRightClick"].performed += MouseClick;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["PlayerLook"].performed -= MouseInput;
			input.actions["PlayerLook"].canceled -= MouseInput;
			input.actions["PlayerLeftClick"].performed -= MouseClick;
			input.actions["PlayerRightClick"].performed -= MouseClick;
		}
	}

	public static void SetFOV(float fov)
	{
		_origFov = fov;
	}

	public static void SetViewBobbing(bool toEnable)
	{
		_viewBobbingEnabled = toEnable;
	}

	public static void SetInvertX(bool toEnable)
	{
		_invertX = toEnable;
	}

	public static void SetInvertY(bool toEnable)
	{
		_invertY = toEnable;
	}

	public static void SetSensitivity(float to)
	{
		_sensitivity = to;
	}

	private void MouseClick(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs)
		{
			ToggleMouse(unlock: false);
		}
	}

	private void MouseInput(InputAction.CallbackContext context)
	{
		if (_player.BlockInputs)
		{
			LookInput = Vector2.zero;
			_rawLookInput = Vector2.zero;
			_controllerLookInput = Vector2.zero;
			return;
		}
		Vector2 rawLookInput = context.ReadValue<Vector2>();
		if (context.control?.device is Gamepad)
		{
			_rawLookInput = rawLookInput;
			if (_invertX)
			{
				_rawLookInput = new Vector2(_rawLookInput.x, 0f - _rawLookInput.y);
			}
			if (_invertY)
			{
				_rawLookInput = new Vector2(0f - _rawLookInput.x, _rawLookInput.y);
			}
			return;
		}
		if (!_mouseLocked)
		{
			LookInput = Vector2.zero;
			return;
		}
		OnPlayerRotated?.Invoke();
		LookInput = new Vector2(0f - rawLookInput.y, rawLookInput.x) * _sensitivity * SensMulti * 0.025f;
		if (_invertX)
		{
			LookInput = new Vector2(LookInput.x, 0f - LookInput.y);
		}
		if (_invertY)
		{
			LookInput = new Vector2(0f - LookInput.x, LookInput.y);
		}
		_rot += (Vector3)LookInput;
	}

	private void Update()
	{
		ControllerRotation();
		ApplyAimAssist();
		ToggleFishCam();
		HeadBobbing();
		HeadRot();
		MouseMovement();
		SetCamPosRot();
		SetFov();
		if (ClientSettings.CheatsEnabled && (bool)Player.LocalPlayer && !Player.LocalPlayer.BlockInputs && Input.GetKeyDown("t"))
		{
			if (Time.timeScale == 1f)
			{
				Time.timeScale = 0.1f;
			}
			else if (Time.timeScale == 0.1f)
			{
				Time.timeScale = 0.01f;
			}
			else
			{
				Time.timeScale = 1f;
			}
		}
	}

	private void ControllerRotation()
	{
		if (GameInfo.Input.currentControlScheme != "Controller")
		{
			return;
		}
		if (_player.BlockInputs || !_mouseLocked)
		{
			LookInput = Vector2.zero;
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
			LookInput = Vector2.zero;
			return;
		}
		OnPlayerRotated?.Invoke();
		LookInput = new Vector2((0f - _controllerLookInput.y) * _controllerLookSpeed.y, _controllerLookInput.x * _controllerLookSpeed.x) * (_sensitivity * SensMulti * Time.deltaTime);
		_rot += (Vector3)LookInput;
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

	private void ApplyAimAssist()
	{
		Vector2 rotationDelta = _player.AimAssist.GetRotationDelta(CamTransform.position, _rot, _controllerLookInput.magnitude);
		_rot += (Vector3)rotationDelta;
	}

	private void ToggleFishCam()
	{
	}

	private void HeadBobbing()
	{
		Vector3 target = Vector3.zero;
		int num;
		if (CurVel > _headBobMinVelocity)
		{
			num = (_player.Movement.Grounded ? 1 : 0);
			if (num != 0)
			{
				float num2 = Mathf.Clamp01(CurVel / Mathf.Max(_headBobMaxVelocity, _headBobMinVelocity + 0.01f));
				float num3 = Mathf.Lerp(_headBobSpeed * 0.5f, _headBobSpeed, num2);
				_headBobTime = Mathf.Repeat(_headBobTime + Time.deltaTime * num3, 1f);
				target = new Vector3(_headBobHorizontalCurve.Evaluate(_headBobTime) * _headBobHorizontalAmount, _headBobVerticalCurve.Evaluate(_headBobTime) * _headBobVerticalAmount, 0f) * num2;
				goto IL_00dd;
			}
		}
		else
		{
			num = 0;
		}
		_headBobTime = 0f;
		_headBobStepArmed = false;
		goto IL_00dd;
		IL_00dd:
		_bobPos = Vector3.SmoothDamp(_bobPos, target, ref _smoothPos, _headBobSmoothTime);
		if (num != 0 && !_player.Dying.IsDead)
		{
			if (_bobPos.y > _headBobStepTriggerY)
			{
				_headBobStepArmed = true;
			}
			if (_headBobStepArmed && _bobPos.y <= _headBobStepTriggerY)
			{
				AudioManager.PlayRandomGlobalClip(SurfaceManager.GetStepSound(_player.Movement.CurGroundTransform, _player.Movement.CurFeetWorldPos), 1, GameInfo.StepSoundCount, variation: true, _player.Movement.Sprinting ? 0.15f : 0.075f);
				_headBobStepArmed = false;
			}
		}
	}

	private void HeadRot()
	{
		_moveRot.x = Mathf.SmoothDamp(_moveRot.x, VelY * _fallTiltAmount, ref _refVelX, _fallTiltSpeed);
		_moveRot.z = Mathf.SmoothDamp(_moveRot.z, (0f - HorizontalInput) * _walkTiltAmount, ref _refTilt, _tiltSpeed);
	}

	private void MouseMovement()
	{
		if (_recoilTar.x != 0f)
		{
			_rot -= new Vector3(0f - _recoilCur.y, _recoilCur.x, 0f);
			_recoilCur = Vector2.Lerp(_recoilCur, _recoilTar, _recoilSpeed * Time.deltaTime);
			_rot += new Vector3(0f - _recoilCur.y, _recoilCur.x, 0f);
		}
		_rot.x = Mathf.Clamp(_rot.x, -90f, 90f);
	}

	private void SetCamPosRot()
	{
		Vector3 vector = new Vector3(_rot.x + _moveRot.x + (0f - ShakePos.y) * _screenShakeRotMulti, _rot.y + ShakePos.x * _screenShakeRotMulti, _moveRot.z);
		vector.x = Mathf.Clamp(vector.x, -90f, 90f);
		Vector3 position = _player.Transform.position;
		position += Vector3.up * CamHeight;
		CamPosNoEffects = position;
		_delayedBobPos = Vector3.Lerp(_delayedBobPos, _bobPos, _headBobDelaySpeed * Time.deltaTime);
		if (_viewBobbingEnabled)
		{
			position += Quaternion.Euler(vector) * (_delayedBobPos + (Vector3)ShakePos);
		}
		CamTransform.position = position;
		CamTransform.eulerAngles = vector;
	}

	public void SetMoveValues(float moveX, float velY, float curVel)
	{
		HorizontalInput = moveX;
		VelY = velY;
		CurVel = curVel;
	}

	public void Recoil(Vector2 recoil)
	{
		_recoilTar.x += UnityEngine.Random.Range(0f - recoil.x, recoil.x);
		_recoilTar.y += recoil.y;
	}

	public static void ToggleMouse(bool unlock)
	{
		if (PauseManager.IsPaused || PlayerThinking.IsThinking || MainMenuManager.IsInMenu)
		{
			unlock = true;
		}
		if (GameInfo.Input.currentControlScheme != "Keyboard")
		{
			unlock = false;
		}
		Cursor.lockState = ((!unlock) ? CursorLockMode.Locked : CursorLockMode.None);
		Cursor.visible = unlock;
		_mouseLocked = !unlock;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!PauseManager.IsPaused && !PlayerThinking.IsThinking && !MainMenuManager.IsInMenu)
		{
			ToggleMouse(unlock: false);
		}
	}

	private void SetFov()
	{
		float target = _origFov;
		float smoothTime = 0.1f;
		if ((bool)_player.Holding.HeldItem && (bool)_player.Holding.HeldItem.Weapon && _player.Holding.HeldItem.Weapon.IsAds)
		{
			target = _player.Holding.HeldItem.Weapon.AdsFov;
			smoothTime = _player.Holding.HeldItem.Weapon.AdsSpeedDamping;
		}
		_curFov = Mathf.SmoothDamp(_curFov, target, ref _refFovVel, smoothTime);
		_cam.fieldOfView = _curFov;
		SensMulti = _curFov / _origFov;
	}

	public void SetRot(float angle)
	{
		_rot.y = angle;
		SetCamPosRot();
	}

	public void SetShakePos(Vector2 shakePos)
	{
		ShakePos = shakePos;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerCameraAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerCameraAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerCameraAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerCameraAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void Awake_UserLogic_PlayerCamera_Assembly_002DCSharp_002Edll()
	{
		_rot = Vector3.up * _player.Transform.eulerAngles.y;
		SensMulti = 1f;
		_curFov = _origFov;
		CamHeight = CamTransform.localPosition.y;
		Cam.fieldOfView = _origFov;
	}
}
