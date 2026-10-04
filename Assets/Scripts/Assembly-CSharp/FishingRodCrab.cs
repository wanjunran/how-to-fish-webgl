using UnityEngine;
using UnityEngine.InputSystem;

public class FishingRodCrab : FishingRod
{
	[SerializeField]
	private float _reelOutSpeed;

	private float _releasePoint;

	private static readonly int ReelingInAnim = Animator.StringToHash("ReelingIn");

	private bool NetworkInitialize___EarlyFishingRodCrabAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateFishingRodCrabAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_FishingRodCrab_Assembly_002DCSharp_002Edll();
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
		base.LateUpdate();
		if ((bool)_holder)
		{
			bool isLocalClient = _holder.Owner.IsLocalClient;
			int curLineLengthMulti = (isLocalClient ? _curLineLengthMulti : _receivedCurLineLengthMulti);
			if (isLocalClient && _isReelingOut)
			{
				ReelOut();
			}
			else
			{
				ReelIn(curLineLengthMulti);
			}
			AnimateHand();
		}
	}

	public override void OnDrop()
	{
		_animator.SetBool(ReelingInAnim, value: true);
		base.OnDrop();
	}

	protected override void ReelInInput(InputAction.CallbackContext context)
	{
		if (!(_curRodRot > 0f))
		{
			if (_isReelingOut)
			{
				StopReelOut();
			}
			_isReelingIn = true;
			_holdReelTimer = 0f;
			if (_lineReelVelRef < -2f)
			{
				_curReelSpeedMulti += _reelMultiIncreaseSpeed;
			}
			DecreaseLineLength(1);
			_animator.SetBool(ReelingInAnim, value: true);
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

	protected override void OnPullBackStarted()
	{
		_animator.SetBool(ReelingInAnim, value: false);
	}

	protected override void OnPullBackReleased(float releasePoint)
	{
		_releasePoint = releasePoint;
	}

	protected override void OnPullBackReset()
	{
		_releasePoint = 0f;
		_animator.SetBool(ReelingInAnim, value: true);
	}

	protected override void UpdateReleasedPullBack()
	{
		if (!_isReelingOut && !(_releasePoint <= 0f) && !(_curRodRot / _releasePoint >= _releaseBaitRotPercent))
		{
			_releasePoint = 0f;
			_isReelingOut = true;
			_holdReelTimer = 0f;
			_animator.SetBool(ReelingInAnim, value: false);
			_bait.ToggleDrag(enable: false);
		}
	}

	private void StopReelOut()
	{
		_isReelingOut = false;
		_lineReelVelRef = 0f;
		_castReelSoundSource.volume = 0f;
		_animator.SetBool(ReelingInAnim, value: false);
	}

	private void ReelOut()
	{
		_bait.ToggleDrag(enable: false);
		float value = _bait.LineLength + _reelOutSpeed * _lineReelStepLength * Time.deltaTime;
		value = Mathf.Clamp(value, _minLineLength, _maxLineLength);
		float b = (value - _bait.LineLength) / Time.deltaTime;
		_lineReelVelRef = Mathf.Lerp(_lineReelVelRef, b, 10f * Time.deltaTime);
		_curLineLengthMulti = Mathf.CeilToInt((value - _minLineLength) / _lineReelStepLength);
		_bait.SetLineLength(value);
		_slowReelSource.volume = 0f;
		_castReelSoundSource.pitch = 1f + Mathf.Abs(_lineReelVelRef * _castReelAudioPitchMulti);
		_castReelSoundSource.volume = ((Time.time - base.TimeOfEquip < 0.25f) ? 0f : Mathf.Abs(_lineReelVelRef * _castReelAudioVolMulti));
		RotateCrank();
	}

	private void ReelIn(int curLineLengthMulti)
	{
		float target = _minLineLength + (float)curLineLengthMulti * _lineReelStepLength;
		_bait.SetLineLength(Mathf.SmoothDamp(_bait.LineLength, target, ref _lineReelVelRef, _lineReelDamping));
		UpdateBaitDrag(_bait.LineLength);
		_castReelSoundSource.volume = 0f;
		RotateCrank();
		if (Time.time - base.TimeOfEquip < 0.25f)
		{
			_slowReelSource.volume = 0f;
			return;
		}
		_slowReelSource.pitch = 1f + Mathf.Abs(_lineReelVelRef * _reelAudioPitchMulti);
		_slowReelSource.volume = Mathf.Abs(_lineReelVelRef * _reelAudioVolMulti);
		_slowReelSource.volume = Mathf.Clamp(_slowReelSource.volume, 0f, _reelAudioMaxVol);
	}

	private void RotateCrank()
	{
		float num = _bait.LineLength * _crankRotMulti;
		_crank.localEulerAngles = new Vector3(num, 90f, 90f);
		_crankHandle.localEulerAngles = new Vector3(0f, num, 0f);
	}

	private void UpdatePlayerUI()
	{
		if ((bool)_holder && !(_holder.Holding.HeldItem != this))
		{
			if ((bool)base.Bait.ItemOnBait)
			{
				PlayerUI.ShowSpecificItemInfo((_curLineLengthMulti < 2) ? _dropInfo : _reelOrDropInfo);
			}
			else if (_isPullingBack)
			{
				PlayerUI.ShowSpecificItemInfo(_throwInfo);
			}
			else
			{
				PlayerUI.ShowSpecificItemInfo((_curLineLengthMulti < 2) ? _pullBackRodInfo : _reelInfo);
			}
		}
	}

	private void AnimateHand()
	{
		_handLeft.localPosition = Vector3.zero;
		_handLeft.localRotation = Quaternion.identity;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyFishingRodCrabAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyFishingRodCrabAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateFishingRodCrabAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateFishingRodCrabAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	protected virtual void Awake_UserLogic_FishingRodCrab_Assembly_002DCSharp_002Edll()
	{
		_isReelingIn = true;
		_handLeft.SetParent(_crankHandleHandHolder);
		base.Awake();
	}
}
