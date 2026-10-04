using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Boat : NetworkBehaviour
{
	private struct Sample
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public float Time;
	}

	[SerializeField]
	private List<BoatMotor> _motors;

	[SerializeField]
	private GameObject _radarHolder;

	[SerializeField]
	private RadarUI _radar;

	[SerializeField]
	private Transform[] _forcePoints;

	[SerializeField]
	private ForceMode _forceMode;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("1 = bouncy like the old buoyancy, 0 = heavily damped and more stable.")]
	private float _boatBounciness = 0.35f;

	[SerializeField]
	private float _linearDampAboveWater = 0.05f;

	[SerializeField]
	private float _angularDampAboveWater = 0.25f;

	[SerializeField]
	private float _antiRollOverForce;

	[SerializeField]
	private float _maxYPosForAntiRollOver;

	[SerializeField]
	[Tooltip("Set a max speed to prevent boat from flying too far away when using dynamite")]
	private float _maxVelocity;

	[Header("Returning to island")]
	[SerializeField]
	private AnimationCurve _returnToIslandForceCurve;

	[SerializeField]
	private float _playerMinDistToReturn;

	[Header("Visuals")]
	[SerializeField]
	private float _propellerVisualSpeed;

	[Header("Audio")]
	[SerializeField]
	private float _motorSoundVolume = 1f;

	[SerializeField]
	[Tooltip("Motor sound volume multiplier by throttle amount (0 = idle, 1 = full throttle).")]
	private AnimationCurve _motorSoundVolumeByThrottle = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	[Tooltip("Motor sound pitch by throttle amount (0 = idle, 1 = full throttle).")]
	private AnimationCurve _motorSoundPitchByThrottle = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float _boatIdleSoundVolume = 1f;

	[SerializeField]
	[Tooltip("Motor idle sound volume multiplier by throttle amount (0 = idle, 1 = full throttle).")]
	private AnimationCurve _motorIdleSoundVolumeByThrottle = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[SerializeField]
	[Tooltip("Motor idle sound pitch by throttle amount (0 = idle, 1 = full throttle).")]
	private AnimationCurve _motorIdleSoundPitchByThrottle = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[SerializeField]
	[Tooltip("Water idle sound volume multiplier by boat velocity magnitude.")]
	private AnimationCurve _waterIdleVolumeByVelocity = AnimationCurve.Linear(0f, 1f, 10f, 0f);

	[SerializeField]
	[Tooltip("Water moving sound volume multiplier by boat velocity magnitude.")]
	private AnimationCurve _waterMovingVolumeByVelocity = AnimationCurve.Linear(0f, 0f, 10f, 1f);

	[SerializeField]
	private float _boatStartStopSoundVolume = 1f;

	[SerializeField]
	private float _motorSoundPitchInAirMulti = 1.5f;

	[SerializeField]
	[Min(0f)]
	private float _motorSoundPitchMultiChangeSpeed = 2f;

	[FormerlySerializedAs("_inWaterSource")]
	[SerializeField]
	private AudioSource _waterIdleSource;

	[SerializeField]
	private AudioSource _waterMovingSource;

	[Header("Particles")]
	[SerializeField]
	private ParticleSystem[] _particlesWhenUnderwater;

	[SerializeField]
	private ParticleSystem[] _particlesWhenPropellerInWater;

	[SerializeField]
	private ParticleSystem[] _motorParticles;

	[SerializeField]
	private float _minVelForSplashParticles;

	[SerializeField]
	[Header("Driving")]
	private float _steerSmoothing;

	[SerializeField]
	private float _maxSteerAngle;

	[SerializeField]
	private float _maxThrottleAngle;

	[SerializeField]
	private Rigidbody _itemColsHolder;

	[SerializeField]
	private Transform _dynamicObjectColsHolder;

	[SerializeField]
	private Collider[] _dynamicObjectCols;

	[SerializeField]
	private Transform _steeringWheel;

	[SerializeField]
	private Transform _throttle;

	[SerializeField]
	private BoatInteractable _boatInteractable;

	[Header("Hands")]
	[SerializeField]
	private HandTransforms _handTransformsLeft;

	[SerializeField]
	private HandTransforms _handTransformsRight;

	[Header("Skins")]
	[SerializeField]
	private float _skinMeshScale = 1f;

	[SerializeField]
	private Vector3 _skinMeshRot;

	[SerializeField]
	private Vector3 _skinMeshPos;

	[SerializeField]
	private SkinPreset _skinPreset;

	[SerializeField]
	private Renderer _skinRenderer;

	[SerializeField]
	private Mesh _mesh;

	public readonly SyncVar<Player> _driver = new SyncVar<Player>();

	public readonly SyncVar<half> _driverXInput = new SyncVar<half>();

	public readonly SyncVar<half> _driverYInput = new SyncVar<half>();

	public readonly SyncVar<byte> _curSkin = new SyncVar<byte>();

	public readonly SyncVar<bool> _boatUnlocked = new SyncVar<bool>();

	public readonly SyncVar<byte> _motorIndex = new SyncVar<byte>();

	public readonly SyncVar<bool> _boatRadarUnlocked = new SyncVar<bool>();

	private readonly Queue<Sample> _velSamples = new Queue<Sample>();

	private Vector3 _serverPos;

	private Vector3 _serverRot;

	private Vector3 _lastFramePos;

	private Quaternion _lastFrameRot;

	private Vector3 _curLerpedPos;

	private Quaternion _curLerpedRot = Quaternion.identity;

	private Quaternion _steeringWheelOrigRot;

	private Quaternion _throttleOrigRot;

	private Vector2 _curSmoothedInput;

	private Vector3 _lastPos;

	private Quaternion _lastRot;

	private float _refSmoothedInputX;

	private float _refSmoothedInputY;

	private float _underwaterLinearDamp;

	private float _underwaterAngularDamp;

	private float _minVelForSplashParticlesSqr;

	private float _playerMinDistToReturnSqr;

	private float _motorSoundPitchMulti = 1f;

	private bool _observerInitialized;

	private bool _propellerInWater;

	private bool _playPropellerInWaterParticles = true;

	private bool _isDrivingForwardInWater = true;

	private bool _controllerForwardPressed;

	private bool _controllerReversePressed;

	private bool _primaryRadarZoomHeld;

	private bool _secondaryRadarZoomHeld;

	private bool _motorReadyForThrottle;

	private float _controllerRadarZoomInput;

	private float _waterIdleStartVolume;

	private float _waterMovingStartVolume;

	private Coroutine _motorSoundStartCoroutine;

	private half2 _localDriveInput;

	private BoatMotor _curMotor;

	private bool NetworkInitialize___EarlyBoatAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateBoatAssembly_002DCSharp_002Edll_Excuted;

	[Header("Assign:")]
	[field: SerializeField]
	public BoatTrigger BoatTrigger { get; private set; }

	[field: SerializeField]
	public Rigidbody HiddenPhysicsRig { get; private set; }

	[field: SerializeField]
	public Rigidbody VisualPhysicsRig { get; private set; }

	[field: SerializeField]
	public Transform VisualBoat { get; private set; }

	[field: SerializeField]
	public Transform DriverPos { get; private set; }

	public static bool IsDrivingLocally { get; private set; }

	public static bool WantToDrive { get; private set; }

	public float SkinMeshScale => _skinMeshScale;

	public Vector3 SkinMeshRot => _skinMeshRot;

	public Vector3 SkinMeshPos => _skinMeshPos;

	public Mesh Mesh => _mesh;

	public SkinPreset SkinPreset => _skinPreset;

	public byte MotorIndex => _motorIndex.Value;

	public bool BoatUnlocked => _boatUnlocked.Value;

	public bool BoatRadarUnlocked => _boatRadarUnlocked.Value;

	public byte CurSkin => _curSkin.Value;

	public HandTransforms HandTransformsRight => _handTransformsRight;

	public HandTransforms HandTransformsLeft => _handTransformsLeft;

	public Player Driver => _driver.Value;

	public Player LocalPlayerOnBoat { get; private set; }

	public Vector3 CenterOfMass { get; private set; }

	public Vector3 Velocity { get; private set; }

	public Vector3 AngularVelocity { get; private set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Boat_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		_boatUnlocked.Value = ((SaveManager.CurServerSave != null) ? SaveManager.CurServerSave.UnlockedBoat : ClientSettings.CheatsEnabled);
		_boatRadarUnlocked.Value = ((SaveManager.CurServerSave != null) ? SaveManager.CurServerSave.UnlockedBoatRadar : ClientSettings.CheatsEnabled);
		_curSkin.Value = (byte)((SaveManager.CurServerSave != null) ? SaveManager.CurServerSave.BoatSkin : 0);
		_motorIndex.Value = (byte)((SaveManager.CurServerSave != null) ? SaveManager.CurServerSave.MotorIndex : 0);
	}

	public override void OnStartClient()
	{
		InstanceFinder.TimeManager.OnTick += TickUpdate;
		IsDrivingLocally = false;
		_radarHolder.SetActive(_boatRadarUnlocked.Value);
		if (!base.IsServerInitialized)
		{
			HiddenPhysicsRig.isKinematic = true;
		}
		BoatManager.SetBoat(_dynamicObjectCols, this);
		OnDriverChange(null, _driver.Value, asServer: false);
	}

	public override void OnStopClient()
	{
		InstanceFinder.TimeManager.OnTick -= TickUpdate;
		BoatManager.RemoveBoat();
	}

	private void BindInputs()
	{
		GameInfo.Input.actions["PlayerMove"].performed += MoveInput;
		GameInfo.Input.actions["PlayerMove"].canceled += MoveInputCanceled;
		GameInfo.Input.actions["PlayerDrop"].performed += StopDrivingInput;
		GameInfo.Input.actions["PlayerPickUp"].performed += StopDrivingInput;
		GameInfo.Input.actions["PlayerLeftClick"].performed += PrimaryInput;
		GameInfo.Input.actions["PlayerLeftClick"].canceled += PrimaryInputCanceled;
		GameInfo.Input.actions["PlayerRightClick"].performed += SecondaryInput;
		GameInfo.Input.actions["PlayerRightClick"].canceled += SecondaryInputCanceled;
		GameInfo.Input.actions["PlayerChangeSkin"].performed += ChangeSkinInput;
		ResetLocalInput();
	}

	private void UnbindInputs()
	{
		GameInfo.Input.actions["PlayerMove"].performed -= MoveInput;
		GameInfo.Input.actions["PlayerMove"].canceled -= MoveInputCanceled;
		GameInfo.Input.actions["PlayerDrop"].performed -= StopDrivingInput;
		GameInfo.Input.actions["PlayerPickUp"].performed -= StopDrivingInput;
		GameInfo.Input.actions["PlayerLeftClick"].performed -= PrimaryInput;
		GameInfo.Input.actions["PlayerLeftClick"].canceled -= PrimaryInputCanceled;
		GameInfo.Input.actions["PlayerRightClick"].performed -= SecondaryInput;
		GameInfo.Input.actions["PlayerRightClick"].canceled -= SecondaryInputCanceled;
		GameInfo.Input.actions["PlayerChangeSkin"].performed -= ChangeSkinInput;
		ResetLocalInput();
	}

	private void PrimaryInput(InputAction.CallbackContext context)
	{
		if (context.control?.device is Gamepad)
		{
			_controllerForwardPressed = true;
			UpdateControllerThrottle();
		}
		else
		{
			_primaryRadarZoomHeld = true;
			UpdateRadarZoom();
		}
	}

	private void PrimaryInputCanceled(InputAction.CallbackContext context)
	{
		if (context.control?.device is Gamepad)
		{
			_controllerForwardPressed = false;
			UpdateControllerThrottle();
		}
		else
		{
			_primaryRadarZoomHeld = false;
			UpdateRadarZoom();
		}
	}

	private void SecondaryInput(InputAction.CallbackContext context)
	{
		if (context.control?.device is Gamepad)
		{
			_controllerReversePressed = true;
			UpdateControllerThrottle();
		}
		else
		{
			_secondaryRadarZoomHeld = true;
			UpdateRadarZoom();
		}
	}

	private void SecondaryInputCanceled(InputAction.CallbackContext context)
	{
		if (context.control?.device is Gamepad)
		{
			_controllerReversePressed = false;
			UpdateControllerThrottle();
		}
		else
		{
			_secondaryRadarZoomHeld = false;
			UpdateRadarZoom();
		}
	}

	private void MoveInput(InputAction.CallbackContext context)
	{
		Vector2 vector = context.ReadValue<Vector2>();
		if (context.control?.device is Gamepad)
		{
			_localDriveInput.x = (half)vector.x;
			if (Mathf.Abs(vector.y) >= 0.9f)
			{
				_controllerRadarZoomInput = vector.y;
				UpdateRadarZoom();
			}
		}
		else
		{
			_localDriveInput = new half2(vector);
		}
	}

	private void MoveInputCanceled(InputAction.CallbackContext context)
	{
		if (context.control?.device is Gamepad)
		{
			_localDriveInput.x = (half)0f;
			_controllerRadarZoomInput = 0f;
			UpdateRadarZoom();
		}
		else
		{
			_localDriveInput = half2.zero;
		}
	}

	private void UpdateControllerThrottle()
	{
		_localDriveInput.y = (half)(_controllerForwardPressed ? 1f : (_controllerReversePressed ? (-1f) : 0f));
	}

	private void UpdateRadarZoom()
	{
		_radar.ToggleIsZoomingIn(_primaryRadarZoomHeld || _controllerRadarZoomInput > 0f);
		_radar.ToggleIsZoomingOut(_secondaryRadarZoomHeld || _controllerRadarZoomInput < 0f);
	}

	private void ResetLocalInput()
	{
		_localDriveInput = half2.zero;
		_controllerForwardPressed = false;
		_controllerReversePressed = false;
		_primaryRadarZoomHeld = false;
		_secondaryRadarZoomHeld = false;
		_controllerRadarZoomInput = 0f;
		UpdateRadarZoom();
	}

	private void StopDrivingInput(InputAction.CallbackContext context)
	{
		Server.Instance.SetDriver(null);
	}

	private void ChangeSkinInput(InputAction.CallbackContext context)
	{
		if ((bool)_driver.Value && _driver.Value.Owner.IsLocalClient && !_driver.Value.BlockInputs)
		{
			float num = context.ReadValue<float>();
			byte skin = SaveManager.GetSkin(byte.MaxValue, _curSkin.Value, num > 0f);
			Server.Instance.SetBoatSkin(skin);
		}
	}

	public void SetLocalPlayerOnBoat(Player player)
	{
		LocalPlayerOnBoat = player;
		_boatInteractable.ToggleIsInteractable(!_driver.Value && (bool)player);
	}

	private void Update()
	{
		if (_observerInitialized)
		{
			UpdateObserverBoatVisuals();
		}
		if (base.IsServerInitialized)
		{
			UpdateServerBoatVisuals();
		}
		_propellerInWater = WaterManager.IsUnderWater(_curMotor.Propeller.position);
		SetWaterAudio();
		UpdateDriverInputVisuals();
		ToggleParticles();
		TogglePropellerParticles();
	}

	private void UpdateDriverInputVisuals()
	{
		Vector2 vector = new Vector2(_driverXInput.Value, _driverYInput.Value);
		_curSmoothedInput.x = Mathf.SmoothDamp(_curSmoothedInput.x, vector.x, ref _refSmoothedInputX, _steerSmoothing);
		_curSmoothedInput.y = Mathf.SmoothDamp(_curSmoothedInput.y, vector.y, ref _refSmoothedInputY, _steerSmoothing);
		if ((bool)_curMotor)
		{
			_curMotor.SetRotation(_curSmoothedInput.x);
			Transform[] visualPropellers = _curMotor.VisualPropellers;
			for (int i = 0; i < visualPropellers.Length; i++)
			{
				visualPropellers[i].Rotate(_curSmoothedInput.y * _propellerVisualSpeed * Time.deltaTime, 0f, 0f, Space.Self);
			}
			float time = Mathf.Abs(_curSmoothedInput.y);
			float target = (_propellerInWater ? 1f : _motorSoundPitchInAirMulti);
			_motorSoundPitchMulti = Mathf.MoveTowards(_motorSoundPitchMulti, target, _motorSoundPitchMultiChangeSpeed * Time.deltaTime);
			float volume = _motorSoundVolumeByThrottle.Evaluate(time) * _motorSoundVolume;
			float pitch = _motorSoundPitchByThrottle.Evaluate(time) * _motorSoundPitchMulti;
			AudioSource[] motorSounds = _curMotor.MotorSounds;
			foreach (AudioSource audioSource in motorSounds)
			{
				if ((bool)audioSource)
				{
					audioSource.volume = volume;
					audioSource.pitch = pitch;
				}
			}
			float volume2 = _motorIdleSoundVolumeByThrottle.Evaluate(time) * _boatIdleSoundVolume;
			float pitch2 = _motorIdleSoundPitchByThrottle.Evaluate(time);
			motorSounds = _curMotor.MotorIdleSounds;
			foreach (AudioSource audioSource2 in motorSounds)
			{
				if ((bool)audioSource2)
				{
					audioSource2.volume = volume2;
					audioSource2.pitch = pitch2;
				}
			}
		}
		_steeringWheel.localRotation = _steeringWheelOrigRot * Quaternion.AngleAxis(_curSmoothedInput.x * _maxSteerAngle, Vector3.forward);
		_throttle.localRotation = _throttleOrigRot * Quaternion.AngleAxis((0f - _curSmoothedInput.y) * _maxThrottleAngle, Vector3.up);
		bool flag = _curSmoothedInput.y > 0f && _playPropellerInWaterParticles;
		if (_isDrivingForwardInWater == flag)
		{
			return;
		}
		_isDrivingForwardInWater = flag;
		if (_isDrivingForwardInWater)
		{
			ParticleSystem[] motorParticles = _motorParticles;
			for (int i = 0; i < motorParticles.Length; i++)
			{
				motorParticles[i].Play();
			}
		}
		else
		{
			ParticleSystem[] motorParticles = _motorParticles;
			for (int i = 0; i < motorParticles.Length; i++)
			{
				motorParticles[i].Stop();
			}
		}
	}

	private void FixedUpdate()
	{
		if (base.IsServerInitialized)
		{
			ApplyReturnForce();
			ApplyWaterForce();
			ApplyInputForce();
			Velocity = HiddenPhysicsRig.linearVelocity;
			AngularVelocity = HiddenPhysicsRig.angularVelocity;
			CenterOfMass = HiddenPhysicsRig.worldCenterOfMass;
		}
		else
		{
			UpdateFakeVelocity();
			CenterOfMass = VisualPhysicsRig.worldCenterOfMass;
		}
		UpdateCollider();
	}

	private void ToggleParticles()
	{
		bool flag = Velocity.sqrMagnitude >= _minVelForSplashParticlesSqr;
		ParticleSystem[] particlesWhenUnderwater = _particlesWhenUnderwater;
		foreach (ParticleSystem particleSystem in particlesWhenUnderwater)
		{
			if (WaterManager.IsUnderWater(particleSystem.transform.position) & flag)
			{
				if (!particleSystem.isPlaying)
				{
					particleSystem.Play();
				}
			}
			else if (particleSystem.isPlaying)
			{
				particleSystem.Stop();
			}
		}
	}

	private void TogglePropellerParticles()
	{
		bool flag = (bool)_driver.Value && _propellerInWater && Velocity.sqrMagnitude >= _minVelForSplashParticlesSqr;
		if (flag != _playPropellerInWaterParticles)
		{
			ParticleSystem[] particlesWhenPropellerInWater = _particlesWhenPropellerInWater;
			foreach (ParticleSystem particleSystem in particlesWhenPropellerInWater)
			{
				if (flag)
				{
					particleSystem.Play();
				}
				else
				{
					particleSystem.Stop();
				}
			}
		}
		_playPropellerInWaterParticles = flag;
	}

	private void SetWaterAudio()
	{
		float magnitude = Velocity.magnitude;
		SetWaterAudioSource(_waterIdleSource, _waterIdleStartVolume, _waterIdleVolumeByVelocity, magnitude);
		SetWaterAudioSource(_waterMovingSource, _waterMovingStartVolume, _waterMovingVolumeByVelocity, magnitude);
	}

	private void SetWaterAudioSource(AudioSource source, float startVolume, AnimationCurve volumeByVelocity, float velocityMagnitude)
	{
		if ((bool)source)
		{
			float b = ((source.transform.position.y < WaterManager.WaterHeight + 0.15f) ? (volumeByVelocity.Evaluate(velocityMagnitude) * startVolume) : 0f);
			source.volume = Mathf.Lerp(source.volume, b, Time.deltaTime * 5f);
		}
	}

	private void UpdateObserverBoatVisuals()
	{
		float t = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
		_curLerpedPos = Vector3.Lerp(_curLerpedPos, _serverPos, t);
		_curLerpedRot = Quaternion.Slerp(_curLerpedRot, Quaternion.Euler(_serverRot), t);
		VisualPhysicsRig.MovePosition(_curLerpedPos);
		VisualPhysicsRig.MoveRotation(_curLerpedRot);
		Vector3 deltaPos = _curLerpedPos - _lastFramePos;
		Vector3 rotDiff = GetRotDiff(_curLerpedRot, _lastFrameRot);
		Quaternion deltaRot = _curLerpedRot * Quaternion.Inverse(_lastFrameRot);
		_lastFramePos = _curLerpedPos;
		_lastFrameRot = _curLerpedRot;
		GiveBoatDeltaToPlayerAndItems(deltaPos, rotDiff, deltaRot);
	}

	private void UpdateServerBoatVisuals()
	{
		VisualPhysicsRig.MovePosition(HiddenPhysicsRig.position);
		VisualPhysicsRig.MoveRotation(HiddenPhysicsRig.rotation);
		Vector3 deltaPos = HiddenPhysicsRig.position - _lastFramePos;
		Vector3 rotDiff = GetRotDiff(HiddenPhysicsRig.rotation, _lastFrameRot);
		Quaternion deltaRot = HiddenPhysicsRig.rotation * Quaternion.Inverse(_lastFrameRot);
		_lastFramePos = HiddenPhysicsRig.position;
		_lastFrameRot = HiddenPhysicsRig.rotation;
		GiveBoatDeltaToPlayerAndItems(deltaPos, rotDiff, deltaRot);
	}

	private void GiveBoatDeltaToPlayerAndItems(Vector3 deltaPos, Vector3 deltaEuler, Quaternion deltaRot)
	{
		Item item = null;
		if ((bool)LocalPlayerOnBoat)
		{
			LocalPlayerOnBoat.Movement.AddBoatPos(deltaPos, deltaEuler);
			item = LocalPlayerOnBoat.Holding.HeldItem;
			if ((bool)item)
			{
				item.RigidbodySync.AddBoatPos(deltaPos, deltaRot);
			}
		}
		foreach (RigidbodySync item2 in BoatTrigger.ItemsOnBoat)
		{
			if (!item || !(item2 == item.RigidbodySync))
			{
				item2.AddBoatPos(deltaPos, deltaRot);
			}
		}
	}

	private void UpdateCollider()
	{
		if (base.IsServerInitialized)
		{
			_dynamicObjectColsHolder.position = HiddenPhysicsRig.position;
			_dynamicObjectColsHolder.rotation = HiddenPhysicsRig.rotation;
			_itemColsHolder.MovePosition(HiddenPhysicsRig.position);
			_itemColsHolder.MoveRotation(HiddenPhysicsRig.rotation);
			_itemColsHolder.linearVelocity = Vector3.zero;
			_itemColsHolder.angularVelocity = Vector3.zero;
		}
		else
		{
			_dynamicObjectColsHolder.position = _curLerpedPos;
			_dynamicObjectColsHolder.rotation = _curLerpedRot;
		}
	}

	private void UpdateFakeVelocity()
	{
		Velocity = (_curLerpedPos - _lastPos) / Time.fixedDeltaTime;
		AngularVelocity = GetRotDiff(_curLerpedRot, _lastRot) / Time.fixedDeltaTime;
		_lastPos = _curLerpedPos;
		_lastRot = _curLerpedRot;
	}

	private void ApplyWaterForce()
	{
		Transform[] forcePoints = _forcePoints;
		foreach (Transform transform in forcePoints)
		{
			KeyValuePair<bool, float> waterInfo = WaterManager.GetWaterInfo(transform.position);
			if (waterInfo.Key)
			{
				float num = WaterManager.BoatWaterForce * Mathf.Abs(waterInfo.Value);
				float y = HiddenPhysicsRig.GetPointVelocity(transform.position).y;
				float num2 = WaterManager.BoatWaterForce * (1f - _boatBounciness);
				float num3 = Mathf.Clamp((0f - y) * num2, 0f - num, WaterManager.BoatWaterForce);
				HiddenPhysicsRig.AddForceAtPosition(Vector3.up * (num + num3), transform.position, _forceMode);
			}
		}
		if (HiddenPhysicsRig.position.y <= _maxYPosForAntiRollOver && (double)Vector3.Dot(HiddenPhysicsRig.transform.up, Vector3.up) < 0.9)
		{
			Vector3 angularVelocityToTarget = DazedUtils.GetAngularVelocityToTarget(HiddenPhysicsRig.rotation, Quaternion.Euler(0f, HiddenPhysicsRig.transform.eulerAngles.y, 0f));
			HiddenPhysicsRig.AddTorque(angularVelocityToTarget * _antiRollOverForce);
		}
		HiddenPhysicsRig.linearDamping = (_propellerInWater ? _underwaterLinearDamp : _linearDampAboveWater);
		HiddenPhysicsRig.angularDamping = (_propellerInWater ? _underwaterAngularDamp : _angularDampAboveWater);
	}

	private void ApplyInputForce()
	{
		if ((bool)_driver.Value && _motorReadyForThrottle && (float)_driverYInput.Value != 0f && _propellerInWater)
		{
			HiddenPhysicsRig.AddForceAtPosition(-_curMotor.Propeller.right * (_curMotor.Force * (float)_driverYInput.Value), _curMotor.Propeller.position);
		}
	}

	private void ApplyReturnForce()
	{
		if (!_driver.Value && !ArePlayersNearBoat() && _propellerInWater && (bool)Island.CurIsland && PlayerManager.AlivePlayers.Count != 0)
		{
			Vector3 vector = Island.IslandPos - HiddenPhysicsRig.position;
			vector = vector.normalized * _returnToIslandForceCurve.Evaluate(vector.sqrMagnitude);
			HiddenPhysicsRig.AddForce(vector);
		}
	}

	private bool ArePlayersNearBoat()
	{
		foreach (Player alivePlayer in PlayerManager.AlivePlayers)
		{
			if ((alivePlayer.Transform.position - VisualBoat.position).sqrMagnitude < _playerMinDistToReturnSqr)
			{
				return true;
			}
		}
		return false;
	}

	private void TickUpdate()
	{
		if (base.IsServerInitialized)
		{
			SendSnapshot(HiddenPhysicsRig.position, HiddenPhysicsRig.rotation.eulerAngles);
		}
		if ((bool)_driver.Value && _driver.Value.Owner.IsLocalClient)
		{
			if (!Player.LocalPlayer || Player.LocalPlayer.BlockInputs)
			{
				Server.Instance.SendBoatInput((half)0f, (half)0f);
			}
			else
			{
				Server.Instance.SendBoatInput(_localDriveInput.x, _localDriveInput.y);
			}
		}
		AchievementManager.CheckFlyingBoatAchievement(VisualBoat.position.y);
	}

	private void ToggleSound(bool soundOn)
	{
		_motorReadyForThrottle = false;
		if (_motorSoundStartCoroutine != null)
		{
			StopCoroutine(_motorSoundStartCoroutine);
			_motorSoundStartCoroutine = null;
		}
		if ((bool)_curMotor)
		{
			StopMotorAudioSources(_curMotor);
			if (soundOn)
			{
				AudioManager.PlayClipAt(_curMotor.MotorStartSoundName, _curMotor.transform.position, variation: false, AudioDistance.Medium, _boatStartStopSoundVolume);
				_motorSoundStartCoroutine = StartCoroutine(StartMotorSoundsAfterDelay(_curMotor));
			}
			else
			{
				AudioManager.PlayClipAt(_curMotor.MotorStopSoundName, _curMotor.transform.position, variation: false, AudioDistance.Medium, _boatStartStopSoundVolume);
			}
		}
	}

	private IEnumerator StartMotorSoundsAfterDelay(BoatMotor motor)
	{
		if (motor.MotorStartDelay > 0f)
		{
			yield return new WaitForSeconds(motor.MotorStartDelay);
		}
		if ((bool)_driver.Value && motor == _curMotor)
		{
			_motorReadyForThrottle = true;
			AudioSource[] motorSounds = motor.MotorSounds;
			foreach (AudioSource audioSource in motorSounds)
			{
				if ((bool)audioSource)
				{
					audioSource.enabled = true;
					audioSource.Play();
				}
			}
			motorSounds = motor.MotorIdleSounds;
			foreach (AudioSource audioSource2 in motorSounds)
			{
				if ((bool)audioSource2)
				{
					audioSource2.enabled = true;
					audioSource2.Play();
				}
			}
		}
		_motorSoundStartCoroutine = null;
	}

	private void StopMotorAudioSources(BoatMotor motor)
	{
		AudioSource[] motorSounds = motor.MotorSounds;
		foreach (AudioSource audioSource in motorSounds)
		{
			if ((bool)audioSource)
			{
				audioSource.Stop();
				audioSource.enabled = false;
			}
		}
		motorSounds = motor.MotorIdleSounds;
		foreach (AudioSource audioSource2 in motorSounds)
		{
			if ((bool)audioSource2)
			{
				audioSource2.Stop();
				audioSource2.enabled = false;
			}
		}
	}

	[ObserversRpc(ExcludeServer = true)]
	private void SendSnapshot(Vector3 pos, Vector3 rot, Channel channel = Channel.Unreliable)
	{
		RpcWriter___SendSnapshot___3148668142(pos, rot, channel);
	}

	private Vector3 GetRotDiff(Quaternion newRot, Quaternion oldRot)
	{
		(newRot * Quaternion.Inverse(oldRot)).ToAngleAxis(out var angle, out var axis);
		if (angle > 180f)
		{
			angle -= 360f;
		}
		float num = angle * (MathF.PI / 180f);
		return axis * num;
	}

	private void OnDriverChange(Player prev, Player next, bool asServer)
	{
		if (asServer && !next)
		{
			_driverXInput.Value = (half)0f;
			_driverYInput.Value = (half)0f;
		}
		if (!base.IsServerInitialized || asServer)
		{
			if (!next || !next.Owner.IsLocalClient)
			{
				_radar.ToggleIsOn(to: false, isInInventory: false);
			}
			else
			{
				_radar.ToggleIsOn(to: true, isInInventory: false);
			}
			ToggleSound(next);
			WantToDrive = false;
			IsDrivingLocally = (bool)next && next.Owner.IsLocalClient;
			_boatInteractable.ToggleIsInteractable(!next && (bool)LocalPlayerOnBoat);
			if (IsDrivingLocally)
			{
				BindInputs();
				Driver.Movement.SetDriver(this);
			}
			else if ((bool)prev && prev.Owner.IsLocalClient)
			{
				UnbindInputs();
				prev.Movement.SetDriver(null);
			}
		}
	}

	public void TrySetDriver(Player newDriver)
	{
		if ((!_driver.Value || !newDriver) && (!newDriver || !newDriver.Holding.HeldItem))
		{
			_driver.Value = newDriver;
		}
	}

	public void ServerSetInput(half x, half y)
	{
		if (base.IsServerInitialized && (bool)_driver.Value)
		{
			_driverXInput.Value = x;
			_driverYInput.Value = (_motorReadyForThrottle ? y : ((half)0f));
		}
	}

	public void ServerSetSkin(byte to)
	{
		_curSkin.Value = (byte)Mathf.Clamp(to, 0, _skinPreset.Skins.Count);
	}

	private void OnSkinChange(byte prev, byte next, bool asServer)
	{
		byte index = (byte)Mathf.Clamp(next, 0, _skinPreset.Skins.Count);
		ShaderManager.ApplyItemSkin(_skinPreset.Skins[index], _skinRenderer, forBoat: true);
	}

	public static void ToggleWantToDrive(bool to)
	{
		WantToDrive = to;
	}

	public void UnlockBoat()
	{
		_boatUnlocked.Value = true;
	}

	public void UnlockBoatRadar()
	{
		_boatRadarUnlocked.Value = true;
	}

	public void SetMotor(byte to)
	{
		if (_motorIndex.Value < to)
		{
			_motorIndex.Value = to;
		}
	}

	private void OnMotorChange(byte prev, byte next, bool asServer)
	{
		if (!base.IsServerInitialized || asServer)
		{
			if ((bool)_curMotor)
			{
				StopMotorAudioSources(_curMotor);
				_curMotor.gameObject.SetActive(value: false);
			}
			AchievementManager.CheckBoatUpgradeAchievement(next);
			_curMotor = _motors[Mathf.Clamp(next, 0, _motors.Count - 1)];
			_curMotor.gameObject.SetActive(value: true);
			if ((bool)_driver.Value)
			{
				ToggleSound(soundOn: true);
			}
		}
	}

	private void OnBoatRadarChange(bool prev, bool next, bool asServer)
	{
		_radarHolder.SetActive(next);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyBoatAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyBoatAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_boatRadarUnlocked.InitializeEarly(this, 6u, isSyncObject: false);
			_motorIndex.InitializeEarly(this, 5u, isSyncObject: false);
			_boatUnlocked.InitializeEarly(this, 4u, isSyncObject: false);
			_curSkin.InitializeEarly(this, 3u, isSyncObject: false);
			_driverYInput.InitializeEarly(this, 2u, isSyncObject: false);
			_driverXInput.InitializeEarly(this, 1u, isSyncObject: false);
			_driver.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterObserversRpc(0u, RpcReader___SendSnapshot___3148668142);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateBoatAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateBoatAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_boatRadarUnlocked.InitializeLate();
			_motorIndex.InitializeLate();
			_boatUnlocked.InitializeLate();
			_curSkin.InitializeLate();
			_driverYInput.InitializeLate();
			_driverXInput.InitializeLate();
			_driver.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___SendSnapshot___3148668142(Vector3 pos, Vector3 rot, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteVector3(rot);
		SendObserversRpc(0u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___SendSnapshot___3148668142(Vector3 P_0, Vector3 P_1, Channel P_2)
	{
		_serverPos = P_0;
		_serverRot = P_1;
		if (!_observerInitialized)
		{
			_observerInitialized = true;
			HiddenPhysicsRig.gameObject.SetActive(value: false);
			_itemColsHolder.gameObject.SetActive(value: false);
		}
	}

	private void RpcReader___SendSnapshot___3148668142(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3 vector2 = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___SendSnapshot___3148668142(vector, vector2, channel);
		}
	}

	private void Awake_UserLogic_Boat_Assembly_002DCSharp_002Edll()
	{
		WantToDrive = false;
		IsDrivingLocally = false;
		_underwaterLinearDamp = HiddenPhysicsRig.linearDamping;
		_underwaterAngularDamp = HiddenPhysicsRig.angularDamping;
		if ((bool)_waterIdleSource)
		{
			_waterIdleStartVolume = _waterIdleSource.volume;
		}
		if ((bool)_waterMovingSource)
		{
			_waterMovingStartVolume = _waterMovingSource.volume;
		}
		_steeringWheelOrigRot = _steeringWheel.localRotation;
		_throttleOrigRot = _throttle.localRotation;
		_minVelForSplashParticlesSqr = _minVelForSplashParticles * _minVelForSplashParticles;
		_playerMinDistToReturnSqr = _playerMinDistToReturn * _playerMinDistToReturn;
		_radar.ToggleIsOn(to: false, isInInventory: false);
		HiddenPhysicsRig.centerOfMass = new Vector3(0f, HiddenPhysicsRig.centerOfMass.y, HiddenPhysicsRig.centerOfMass.z);
		HiddenPhysicsRig.maxLinearVelocity = _maxVelocity;
		_driver.OnChange += OnDriverChange;
		_curMotor = _motors[0];
		_curSkin.OnChange += OnSkinChange;
		_motorIndex.OnChange += OnMotorChange;
		_boatRadarUnlocked.OnChange += OnBoatRadarChange;
	}
}
