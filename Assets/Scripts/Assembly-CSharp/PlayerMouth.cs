using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerMouth : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[Header("Transforms")]
	[SerializeField]
	private Transform _mouthUpper;

	[SerializeField]
	private Transform _mouthLower;

	[Header("Move amount & speed")]
	[FormerlySerializedAs("_mouthMoveDegrees")]
	[SerializeField]
	private float _mouthMoveMaxDegrees;

	[SerializeField]
	private float _mouthMoveSpeed;

	[FormerlySerializedAs("_voiceChat")]
	[FormerlySerializedAs("_voiceChatTrans")]
	[Header("Speaking")]
	[SerializeField]
	private AudioSource _voiceChatSource;

	[SerializeField]
	private float _minVol;

	[SerializeField]
	private float _minDelayToChangeMouth;

	[SerializeField]
	private float _rmsDiffToChangeMouth = 0.02f;

	[Header("Eating")]
	[SerializeField]
	private AnimationCurve _eatMoveCurve;

	private static PlayerMouth _localMouth;

	private Quaternion _origMouthRot;

	private Quaternion _curMouthRot;

	private Vector3 _curSize;

	private float _curVol;

	private float _curTargetSize;

	private float _curTargetRot;

	private float _prevRms;

	private float _timeOfChangeMouth;

	private bool _changeMouth;

	private Transform _voiceTarget;

	private void Awake()
	{
		_origMouthRot = _mouthLower.localRotation;
	}

	public void InitializeLocal()
	{
		BindInputs();
		_voiceChatSource.spatialBlend = 0f;
		_localMouth = this;
	}

	private void Update()
	{
		if (!_player.Owner.IsLocalClient)
		{
			SpeakingMovements();
			MoveVoice();
		}
		else if ((bool)_player.Dying.DeadPlayer)
		{
			AnimateSpeaking();
			ApplyMouth();
		}
	}

	private void SpeakingMovements()
	{
		if (_player.Eating.EatPercent > 0f)
		{
			AnimateEating();
		}
		else
		{
			AnimateSpeaking();
		}
		ApplyMouth();
	}

	private void MoveVoice()
	{
		if ((bool)_voiceTarget)
		{
			_voiceChatSource.transform.position = _voiceTarget.position;
		}
	}

	private void AnimateSpeaking()
	{
		if (_changeMouth && Time.time >= _timeOfChangeMouth + _minDelayToChangeMouth)
		{
			RandomizeMouthTargetSize();
		}
		float num = Mathf.InverseLerp(_minVol, 1f, _curVol);
		if (num <= 0f)
		{
			_curTargetSize = 1f;
		}
		else
		{
			num *= _curTargetSize;
		}
		_curTargetRot = _mouthMoveMaxDegrees * num;
	}

	private void AnimateEating()
	{
		_curTargetSize = 1f;
		_curTargetRot = Mathf.Lerp(0f, _mouthMoveMaxDegrees, _eatMoveCurve.Evaluate(_player.Eating.EatPercent));
	}

	private void ApplyMouth()
	{
		Quaternion b = Quaternion.Euler(_curTargetRot, 0f, 0f);
		_curMouthRot = Quaternion.Slerp(_curMouthRot, b, _mouthMoveSpeed * Time.deltaTime);
		_curSize = Vector3.Lerp(b: new Vector3(_curTargetSize, 1f, 1f), a: _curSize, t: _mouthMoveSpeed * Time.deltaTime);
		if ((bool)_player.Dying.DeadPlayer)
		{
			_player.Dying.DeadPlayer.MouthLower.localRotation = _origMouthRot * _curMouthRot;
			_player.Dying.DeadPlayer.MouthLower.localScale = _curSize;
			_player.Dying.DeadPlayer.MouthUpper.localScale = _curSize;
		}
		else
		{
			_mouthLower.localRotation = _origMouthRot * _curMouthRot;
			_mouthLower.localScale = _curSize;
			_mouthUpper.localScale = _curSize;
		}
	}

	private void OnDestroy()
	{
		if ((bool)_player)
		{
			UnbindInputs();
			AudioManager.TogglePushToTalk(to: false);
		}
	}

	private void RandomizeMouthTargetSize()
	{
		_timeOfChangeMouth = Time.time;
		_curTargetSize = Random.Range(0.2f, 1f);
		_changeMouth = false;
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["PushToTalk"].performed += PushToTalkInput;
		input.actions["PushToTalk"].canceled += PushToTalkInputCanceled;
	}

	private void PushToTalkInput(InputAction.CallbackContext context)
	{
		AudioManager.TogglePushToTalk(to: true);
	}

	private void PushToTalkInputCanceled(InputAction.CallbackContext context)
	{
		AudioManager.TogglePushToTalk(to: false);
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["PushToTalk"].performed -= PushToTalkInput;
			input.actions["PushToTalk"].canceled -= PushToTalkInputCanceled;
		}
	}

	public static void SetLocalSamples(float[] samples)
	{
		if ((bool)_localMouth)
		{
			_localMouth.SetSamples(samples);
		}
	}

	public void SetSamples(float[] samples)
	{
		float decibel = AudioManager.GetDecibel(samples);
		float rms = AudioManager.GetRms(samples);
		if (Mathf.Abs(rms - _prevRms) > _rmsDiffToChangeMouth)
		{
			_changeMouth = true;
		}
		_prevRms = rms;
		_curVol = (decibel + 80f) * 0.0125f;
	}

	public void SetVoiceTarget(Transform target)
	{
		_voiceTarget = target;
	}
}
