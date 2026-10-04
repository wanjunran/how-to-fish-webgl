using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDrivingBoat : MonoBehaviour
{
	private static SelfDrivingBoat _activeBoat;

	[SerializeField]
	private Transform _cam;

	[SerializeField]
	private float _camSpeed = 5f;

	[SerializeField]
	private float _camAngSpeed = 8f;

	[SerializeField]
	private float _camPosRetardation = 0.2f;

	[SerializeField]
	private float _camRotRetardation = 0.5f;

	[SerializeField]
	private float _defaultHeight;

	[Header("Targets")]
	[SerializeField]
	private Transform _defaultCamPos;

	[SerializeField]
	private Transform _defaultLookAt;

	[Space]
	[SerializeField]
	private bool _useSkinCustomizationSettings;

	[SerializeField]
	private Transform _skinCustomizationPos;

	[SerializeField]
	private float _skinCustomizationHeight;

	[SerializeField]
	private Transform _skinCustomizationLookAt;

	[Header("Boat")]
	[SerializeField]
	private Rigidbody _boatRig;

	[SerializeField]
	private BoatMotor _motor;

	[SerializeField]
	private ParticleSystem[] _boatParticles;

	[SerializeField]
	private Transform[] _floatingPoints;

	[SerializeField]
	private float _propellerVisualSpeed;

	[Header("Boat Simulation")]
	[SerializeField]
	private float _turnSmoothing;

	[SerializeField]
	private float _startVel;

	[Space]
	[SerializeField]
	private float _antiRollOverForce = 500f;

	[SerializeField]
	private float _underwaterLinearDamp;

	[SerializeField]
	private float _underwaterAngularDamp;

	[SerializeField]
	private float _linearDampAboveWater = 0.05f;

	[SerializeField]
	private float _angularDampAboveWater = 0.25f;

	[SerializeField]
	private Vector2 _straightTimeMinMax;

	[SerializeField]
	private Vector2 _turnTimeMinMax;

	[Header("Sounds")]
	[SerializeField]
	[Tooltip("Master volume multiplier over normalized time after this boat is enabled (0 = just enabled, 1 = fade duration reached).")]
	private AnimationCurve _volumeOnEnable = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	[Min(0f)]
	[Tooltip("Seconds used to progress through the OnEnable volume curve.")]
	private float _volumeOnEnableDuration = 1f;

	[SerializeField]
	private AudioSource _waterIdleSource;

	[SerializeField]
	private AudioSource _waterMovingSource;

	[SerializeField]
	[Tooltip("Water idle sound volume multiplier by boat velocity magnitude.")]
	private AnimationCurve _waterIdleVolumeByVelocity = AnimationCurve.Linear(0f, 1f, 10f, 0f);

	[SerializeField]
	[Tooltip("Water moving sound volume multiplier by boat velocity magnitude.")]
	private AnimationCurve _waterMovingVolumeByVelocity = AnimationCurve.Linear(0f, 0f, 10f, 1f);

	[SerializeField]
	private float _motorSoundPitchInAirMulti = 1.5f;

	[SerializeField]
	[Min(0f)]
	private float _motorSoundPitchMultiChangeSpeed = 2f;

	[SerializeField]
	private float _motorSoundVolume = 1f;

	[SerializeField]
	private float _boatIdleSoundVolume = 1f;

	[SerializeField]
	[Tooltip("Motor sound volume multiplier by throttle amount (0 = idle, 1 = full throttle).")]
	private AnimationCurve _motorSoundVolumeByThrottle = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	[Tooltip("Motor sound pitch by throttle amount (0 = idle, 1 = full throttle).")]
	private AnimationCurve _motorSoundPitchByThrottle = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	[Tooltip("Motor idle sound volume multiplier by throttle amount (0 = idle, 1 = full throttle).")]
	private AnimationCurve _motorIdleSoundVolumeByThrottle = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	[SerializeField]
	[Tooltip("Motor idle sound pitch by throttle amount (0 = idle, 1 = full throttle).")]
	private AnimationCurve _motorIdleSoundPitchByThrottle = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	private Vector3 _camTargetPos;

	private Vector3 _camLookAtPos;

	private Vector3 _camVel;

	private Vector3 _camAngVel;

	private bool _boatIsUnderwater;

	private int _dirMultiplier = 1;

	private bool _isTurning;

	private bool _camFollowBoat;

	private float _camFollowBoatPosMultiplier;

	private float _camFollowBoatRotMultiplier;

	private bool _hasAppliedStartVel;

	private float _curSteerDirMulti;

	private float _steerVelRef;

	private float _timeLeftInState;

	private Vector3 _startPos;

	private Quaternion _startRot;

	private bool _motorEnabled;

	private bool _motorReadyForThrottle;

	private float _waterIdleStartVolume;

	private float _waterMovingStartVolume;

	private float _throttleAmount;

	private float _motorSoundPitchMulti = 1f;

	private float _volumeOnEnableStartTime;

	private float _volumeOnEnableMultiplier = 1f;

	private Coroutine _motorSoundStartCoroutine;

	public static bool IsDriving { get; private set; }

	private void Awake()
	{
		_startPos = base.transform.position;
		_startRot = base.transform.rotation;
		if ((bool)_waterIdleSource)
		{
			_waterIdleStartVolume = _waterIdleSource.volume;
		}
		if ((bool)_waterMovingSource)
		{
			_waterMovingStartVolume = _waterMovingSource.volume;
		}
	}

	private void OnEnable()
	{
		_activeBoat = this;
		_volumeOnEnableStartTime = Time.time;
		_volumeOnEnableMultiplier = EvaluateOnEnableVolume(0f);
		ToggleSound(soundOn: true);
	}

	private void FixedUpdate()
	{
		if (IsDriving)
		{
			if (!_hasAppliedStartVel)
			{
				_hasAppliedStartVel = true;
				_motorReadyForThrottle = true;
				_boatRig.linearVelocity = _boatRig.transform.forward * _startVel;
			}
			ApplyWaterForce();
			if (!LocalSkin.IsInSkinCustomization && _boatIsUnderwater)
			{
				ApplyEngineForce();
			}
		}
	}

	private void OnDisable()
	{
		Player.ToggleLocalPlayer(to: true);
		if (_activeBoat == this)
		{
			_activeBoat = null;
		}
		_hasAppliedStartVel = false;
	}

	private void Update()
	{
		if (IsDriving)
		{
			float elapsed = Time.time - _volumeOnEnableStartTime;
			_volumeOnEnableMultiplier = EvaluateOnEnableVolume(elapsed);
			MoveCam();
			SetWaterAudio();
			UpdateVolume();
			Transform[] visualPropellers = _motor.VisualPropellers;
			for (int i = 0; i < visualPropellers.Length; i++)
			{
				visualPropellers[i].Rotate(_propellerVisualSpeed * Time.deltaTime, 0f, 0f, Space.Self);
			}
		}
	}

	private void UpdateVolume()
	{
		float b = ((!LocalSkin.IsInSkinCustomization) ? 1 : 0);
		_throttleAmount = Mathf.Lerp(_throttleAmount, b, Time.deltaTime * 2f);
		float target = (_boatIsUnderwater ? 1f : _motorSoundPitchInAirMulti);
		_motorSoundPitchMulti = Mathf.MoveTowards(_motorSoundPitchMulti, target, _motorSoundPitchMultiChangeSpeed * Time.deltaTime);
		float volume = _motorSoundVolumeByThrottle.Evaluate(_throttleAmount) * _motorSoundVolume * _volumeOnEnableMultiplier;
		float pitch = _motorSoundPitchByThrottle.Evaluate(_throttleAmount) * _motorSoundPitchMulti;
		AudioSource[] motorSounds = _motor.MotorSounds;
		foreach (AudioSource audioSource in motorSounds)
		{
			if ((bool)audioSource)
			{
				audioSource.volume = volume;
				audioSource.pitch = pitch;
			}
		}
		float volume2 = _motorIdleSoundVolumeByThrottle.Evaluate(_throttleAmount) * _boatIdleSoundVolume * _volumeOnEnableMultiplier;
		float pitch2 = _motorIdleSoundPitchByThrottle.Evaluate(_throttleAmount);
		motorSounds = _motor.MotorIdleSounds;
		foreach (AudioSource audioSource2 in motorSounds)
		{
			if ((bool)audioSource2)
			{
				audioSource2.volume = volume2;
				audioSource2.pitch = pitch2;
			}
		}
	}

	public void ToggleBoat(bool to)
	{
		IsDriving = to;
		_camFollowBoat = to;
		_camFollowBoatPosMultiplier = 1f;
		_camFollowBoatRotMultiplier = 1f;
		Transform obj = (to ? base.transform : null);
		WaterManager.SetCustomWaterTarget(obj);
		ShaderManager.SetCustomTarget(obj);
		_isTurning = false;
		_timeLeftInState = Random.Range(_straightTimeMinMax.x, _straightTimeMinMax.y);
		if (to)
		{
			Reset();
		}
	}

	private void Reset()
	{
		base.transform.position = _startPos;
		base.transform.rotation = _startRot;
		_boatRig.linearVelocity = Vector3.zero;
		_boatRig.angularVelocity = Vector3.zero;
	}

	public void ToggleCamFollowBoat(bool to)
	{
		_camFollowBoat = to;
	}

	private void ApplyEngineForce()
	{
		if (_motorReadyForThrottle)
		{
			_boatRig.AddForceAtPosition(-_motor.Propeller.right * _motor.Force, _motor.Propeller.position);
			_dirMultiplier = ((!_camFollowBoat) ? (-1) : 0);
			_curSteerDirMulti = Mathf.SmoothDamp(_curSteerDirMulti, _dirMultiplier, ref _steerVelRef, _turnSmoothing);
			_motor.SetRotation(_curSteerDirMulti);
		}
	}

	private void ApplyWaterForce()
	{
		_boatIsUnderwater = false;
		Transform[] floatingPoints = _floatingPoints;
		foreach (Transform transform in floatingPoints)
		{
			KeyValuePair<bool, float> waterInfo = WaterManager.GetWaterInfo(transform.position);
			if (waterInfo.Key)
			{
				_boatIsUnderwater = true;
				_boatRig.AddForceAtPosition(Vector3.up * (WaterManager.BoatWaterForce * Mathf.Abs(waterInfo.Value)), transform.position, ForceMode.Force);
			}
		}
		_boatRig.linearDamping = (_boatIsUnderwater ? _underwaterLinearDamp : _linearDampAboveWater);
		_boatRig.angularDamping = (_boatIsUnderwater ? _underwaterAngularDamp : _angularDampAboveWater);
		if ((double)Vector3.Dot(_boatRig.transform.up, Vector3.up) < 0.9)
		{
			Vector3 angularVelocityToTarget = DazedUtils.GetAngularVelocityToTarget(_boatRig.rotation, Quaternion.Euler(0f, _boatRig.transform.eulerAngles.y, 0f));
			_boatRig.AddTorque(angularVelocityToTarget * _antiRollOverForce);
		}
		ParticleSystem[] boatParticles = _boatParticles;
		foreach (ParticleSystem particleSystem in boatParticles)
		{
			if (WaterManager.IsUnderWater(particleSystem.transform.position))
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

	private void MoveCam()
	{
		if (!_camFollowBoat)
		{
			_camFollowBoatPosMultiplier = Mathf.Clamp01(_camFollowBoatPosMultiplier - Time.deltaTime * _camPosRetardation);
			_camFollowBoatRotMultiplier = Mathf.Clamp01(_camFollowBoatRotMultiplier - Time.deltaTime * _camRotRetardation);
		}
		_camTargetPos = (LocalSkin.IsInSkinCustomization ? _skinCustomizationPos.position : _defaultCamPos.position);
		_camTargetPos.y = (LocalSkin.IsInSkinCustomization ? _skinCustomizationHeight : _defaultHeight);
		_camLookAtPos = (LocalSkin.IsInSkinCustomization ? _skinCustomizationLookAt.position : _defaultLookAt.position);
		Quaternion b = Quaternion.LookRotation(_camLookAtPos - _cam.position, Vector3.up);
		_cam.position = Vector3.Lerp(_cam.position, _camTargetPos, _camSpeed * _camFollowBoatPosMultiplier * Time.deltaTime);
		_cam.rotation = Quaternion.Slerp(_cam.rotation, b, _camAngSpeed * _camFollowBoatRotMultiplier * Time.deltaTime);
	}

	private void ToggleSound(bool soundOn)
	{
		_motorReadyForThrottle = false;
		_motorEnabled = soundOn;
		if (_motorSoundStartCoroutine != null)
		{
			StopCoroutine(_motorSoundStartCoroutine);
			_motorSoundStartCoroutine = null;
		}
		if ((bool)_motor)
		{
			StopMotorAudioSources(_motor);
			if (soundOn)
			{
				_motorSoundStartCoroutine = StartCoroutine(StartMotorSoundsAfterDelay());
			}
		}
	}

	private IEnumerator StartMotorSoundsAfterDelay()
	{
		yield return null;
		if (!_motor)
		{
			yield break;
		}
		if (_motor.MotorStartDelay > 0f)
		{
			yield return new WaitForSeconds(_motor.MotorStartDelay);
		}
		_motorReadyForThrottle = true;
		AudioSource[] motorSounds = _motor.MotorSounds;
		foreach (AudioSource audioSource in motorSounds)
		{
			if ((bool)audioSource)
			{
				audioSource.enabled = true;
				audioSource.Play();
			}
		}
		motorSounds = _motor.MotorIdleSounds;
		foreach (AudioSource audioSource2 in motorSounds)
		{
			if ((bool)audioSource2)
			{
				audioSource2.enabled = true;
				audioSource2.Play();
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

	private void SetWaterAudio()
	{
		float magnitude = _boatRig.linearVelocity.magnitude;
		SetWaterAudioSource(_waterIdleSource, _waterIdleStartVolume, _waterIdleVolumeByVelocity, magnitude);
		SetWaterAudioSource(_waterMovingSource, _waterMovingStartVolume, _waterMovingVolumeByVelocity, magnitude);
	}

	private void SetWaterAudioSource(AudioSource source, float startVolume, AnimationCurve volumeByVelocity, float velocityMagnitude)
	{
		if ((bool)source)
		{
			float b = ((_motor.Propeller.position.y < WaterManager.WaterHeight) ? (volumeByVelocity.Evaluate(velocityMagnitude) * startVolume * _volumeOnEnableMultiplier) : 0f);
			source.volume = Mathf.Lerp(source.volume, b, Time.deltaTime * 5f);
		}
	}

	private float EvaluateOnEnableVolume(float elapsed)
	{
		float time = ((_volumeOnEnableDuration > 0f) ? Mathf.Clamp01(elapsed / _volumeOnEnableDuration) : 1f);
		return Mathf.Max(0f, _volumeOnEnable.Evaluate(time));
	}
}
