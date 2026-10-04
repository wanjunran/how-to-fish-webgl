using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class FishingRodCast : FishingRod
{
	[SerializeField]
	private Transform _rodBaseHandHolder;

	[SerializeField]
	private Transform _reel;

	[FormerlySerializedAs("_guard")]
	[SerializeField]
	private Transform _bail;

	[SerializeField]
	private Transform _releasedLinePointHolder;

	[SerializeField]
	private Transform _releasedLinePoint;

	[FormerlySerializedAs("_reelingLineGuardPoint")]
	[SerializeField]
	private Transform _reelingLineBailPoint;

	[SerializeField]
	private AudioSource _fastReelSource;

	[SerializeField]
	private float _reelRotMulti;

	private Quaternion _curBailRot;

	private float _releasePoint;

	private bool _bailOpen;

	private bool _receivedBailOpen;

	private bool NetworkInitialize___EarlyFishingRodCastAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateFishingRodCastAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_FishingRodCast_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			UpdatePlayerUI();
		}
	}

	protected override void LateUpdate()
	{
		if ((bool)_holder)
		{
			bool isLocalClient = _holder.Owner.IsLocalClient;
			int curLineLengthMulti = (isLocalClient ? _curLineLengthMulti : _receivedCurLineLengthMulti);
			bool flag = (isLocalClient ? _bailOpen : _receivedBailOpen);
			if ((isLocalClient && _isReelingOut) || (!isLocalClient & flag))
			{
				ReelOut(isLocalClient);
			}
			else
			{
				ReelIn(curLineLengthMulti, flag);
			}
			AnimateBail(flag);
			AnimateHand(flag);
		}
		base.LateUpdate();
	}

	public override void OnPickUp()
	{
		base.OnPickUp();
		float spatialBlend = ((!_holder || !_holder.Owner.IsLocalClient) ? 1 : 0);
		_fastReelSource.spatialBlend = spatialBlend;
	}

	public override void OnDrop()
	{
		_bailOpen = false;
		_releasePoint = 0f;
		_fastReelSource.volume = 0f;
		base.OnDrop();
	}

	protected override void ReelInInput(InputAction.CallbackContext context)
	{
		if (!(_curRodRot > 0f))
		{
			if (_isReelingOut)
			{
				StartReel();
			}
			_isReelingIn = true;
			_holdReelTimer = 0f;
			if (_lineReelVelRef < -2f)
			{
				_curReelSpeedMulti += _reelMultiIncreaseSpeed;
			}
			DecreaseLineLength(1);
		}
	}

	protected override void ReelInCancelled(InputAction.CallbackContext context)
	{
		_isReelingIn = false;
	}

	protected override void ReelOutInput(InputAction.CallbackContext context)
	{
		base.ReelOutInput(context);
	}

	protected override void ReelOutCancelled(InputAction.CallbackContext context)
	{
		base.ReelOutCancelled(context);
	}

	protected override bool CanStartPullBack()
	{
		return !_isReelingOut;
	}

	protected override void OnPullBackStarted()
	{
		_bailOpen = true;
		AudioManager.PlayPlayerClip("FishingrodBailClose_V1", base.Holder, variation: true);
	}

	protected override void OnPullBackReleased(float releasePoint)
	{
		_releasePoint = releasePoint;
		_bait.ToggleDrag(enable: false);
	}

	protected override void OnPullBackReset()
	{
		_releasePoint = 0f;
		_bailOpen = false;
	}

	protected override bool GetAdditionalRodState()
	{
		return _bailOpen;
	}

	protected override void ReceiveAdditionalRodState(bool bailOpen)
	{
		_receivedBailOpen = bailOpen;
	}

	protected override void UpdateReleasedPullBack()
	{
		if (!_isReelingOut && _releasePoint > 0f && _curRodRot / _releasePoint < _releaseBaitRotPercent)
		{
			StartCast();
		}
	}

	private void StartReel()
	{
		_curLineLengthMulti = (int)(Vector3.Distance(_bait.transform.position, _tipJoint.position) / _lineReelStepLength);
		_isReelingOut = false;
		_linePoints[1] = _reelingLineBailPoint;
		AudioManager.PlayPlayerClip("FishingrodBailClose_V2", base.Holder, variation: true);
		_bailOpen = false;
		_lineReelVelRef = 0f;
		_curReelSpeedMulti = 1f;
	}

	private void StartCast()
	{
		_releasePoint = 0f;
		_lineReelVelRef = 0f;
		_isReelingOut = true;
		_linePoints[1] = _releasedLinePoint;
	}

	private void ReelOut(bool isLocalHolder)
	{
		_bait.ToggleDrag(enable: false);
		float value = Vector3.Distance(_bait.transform.position, _tipJoint.position) + _lineThrowSpeed;
		value = Mathf.Clamp(value, 0f, _maxLineLength);
		float num = value - _bait.LineLength;
		num /= Time.deltaTime;
		_lineReelVelRef = Mathf.Lerp(_lineReelVelRef, num, 10f * Time.deltaTime);
		_slowReelSource.volume = 0f;
		_fastReelSource.volume = 0f;
		if (isLocalHolder)
		{
			if (Time.time - base.TimeOfEquip < 0.25f)
			{
				_castReelSoundSource.volume = 0f;
			}
			else
			{
				_castReelSoundSource.volume = Mathf.Abs(_lineReelVelRef * _castReelAudioVolMulti);
			}
		}
		else if (_receivedRodRot > 0f)
		{
			_castReelSoundSource.volume = 0f;
		}
		else
		{
			_castReelSoundSource.volume = Mathf.Abs(_lineReelVelRef * _castReelAudioVolMulti);
		}
		_releasedLinePointHolder.localEulerAngles = new Vector3(0f, (0f - _lineReelVelRef) * _reelRotMulti, 0f);
		_bait.SetLineLength(value);
	}

	private void ReelIn(int curLineLengthMulti, bool bailOpen)
	{
		float target = _minLineLength + (float)curLineLengthMulti * _lineReelStepLength;
		_bait.SetLineLength(Mathf.SmoothDamp(_bait.LineLength, target, ref _lineReelVelRef, _lineReelDamping));
		UpdateBaitDrag(_bait.LineLength);
		_crank.localEulerAngles = new Vector3(0f, 90f, (0f - _bait.LineLength) * _crankRotMulti);
		_crankHandle.localEulerAngles = new Vector3(_crank.localEulerAngles.z, 90f, -90f);
		_reel.localEulerAngles = new Vector3(0f, (0f - _bait.LineLength) * _reelRotMulti, 0f);
		_castReelSoundSource.volume = 0f;
		if (Time.time - base.TimeOfEquip < 0.25f)
		{
			_slowReelSource.volume = 0f;
			_fastReelSource.volume = 0f;
		}
		else if (_lineReelVelRef > -3f)
		{
			_slowReelSource.pitch = 1f + Mathf.Abs(_lineReelVelRef * _reelAudioPitchMulti);
			_slowReelSource.volume = Mathf.Abs(_lineReelVelRef * _reelAudioVolMulti);
			_slowReelSource.volume = Mathf.Clamp(_slowReelSource.volume, 0f, _reelAudioMaxVol);
			_fastReelSource.volume = 0f;
		}
		else
		{
			_fastReelSource.pitch = 1f + Mathf.Abs(_lineReelVelRef * _reelAudioPitchMulti);
			_fastReelSource.volume = Mathf.Abs(_lineReelVelRef * _reelAudioVolMulti);
			_fastReelSource.volume = Mathf.Clamp(_fastReelSource.volume, 0f, _reelAudioMaxVol);
			_slowReelSource.volume = 0f;
		}
	}

	private void UpdatePlayerUI()
	{
		if (!_holder || _holder.Holding.HeldItem != this)
		{
			return;
		}
		if (_curLineLengthMulti < 2)
		{
			if ((bool)base.Bait.ItemOnBait)
			{
				PlayerUI.ShowSpecificItemInfo(_isReelingOut ? _reelOrDropInfo : _dropInfo);
			}
			else if (!_isPullingBack && !_bailOpen)
			{
				PlayerUI.ShowSpecificItemInfo(_pullBackRodInfo);
			}
			else if (_isPullingBack && _bailOpen)
			{
				PlayerUI.ShowSpecificItemInfo(_throwInfo);
			}
			else
			{
				PlayerUI.ShowSpecificItemInfo(_isReelingOut ? _reelInfo : _pullBackRodInfo);
			}
		}
		else
		{
			PlayerUI.ShowSpecificItemInfo(base.Bait.ItemOnBait ? _reelOrDropInfo : _reelInfo);
		}
	}

	private void AnimateBail(bool bailOpen)
	{
		float x = (bailOpen ? (-200) : (-90));
		_curBailRot = Quaternion.Slerp(_curBailRot, Quaternion.Euler(x, -67.442f, 90f), 25f * Time.deltaTime);
		_bail.localRotation = _curBailRot;
	}

	private void AnimateHand(bool bailOpen)
	{
		if (bailOpen && _handLeft.parent != _rodBaseHandHolder)
		{
			AudioManager.PlayPlayerClip("FishingrodBailClose_V1", base.Holder, variation: true);
			_handLeft.SetParent(_rodBaseHandHolder);
			_animator.enabled = true;
		}
		else if (!bailOpen && _handLeft.parent != _crankHandleHandHolder)
		{
			AudioManager.PlayPlayerClip("FishingrodBailClose_V2", base.Holder, variation: true);
			_handLeft.SetParent(_crankHandleHandHolder);
			_animator.enabled = false;
		}
		_handLeft.localPosition = Vector3.Lerp(_handLeft.localPosition, Vector3.zero, 10f * Time.deltaTime);
		_handLeft.localRotation = Quaternion.Slerp(_handLeft.localRotation, Quaternion.identity, 10f * Time.deltaTime);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyFishingRodCastAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyFishingRodCastAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateFishingRodCastAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateFishingRodCastAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	protected virtual void Awake_UserLogic_FishingRodCast_Assembly_002DCSharp_002Edll()
	{
		_handLeft.SetParent(_rodBaseHandHolder);
		_curBailRot = _bail.localRotation;
		base.Awake();
	}
}
