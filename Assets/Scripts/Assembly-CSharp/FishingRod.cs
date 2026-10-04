using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class FishingRod : Tool
{
	[SerializeField]
	protected Bait _bait;

	[SerializeField]
	protected Rigidbody _tipJoint;

	[SerializeField]
	protected Transform[] _rodParts;

	[SerializeField]
	protected float _baitJointForceForMaxBend = 250f;

	[SerializeField]
	protected float _maxRodBendAngle = 35f;

	[SerializeField]
	protected float _rodBendForceSmoothSpeed = 0.1f;

	[SerializeField]
	protected float _rodBendRotSmoothSpeed = 10f;

	[SerializeField]
	protected AnimationCurve _rodBendDistribution = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	protected AnimationCurve _rodBendForceMultiCurve;

	[SerializeField]
	protected Transform _crank;

	[SerializeField]
	protected Transform _crankHandle;

	[SerializeField]
	protected Transform _crankHandleHandHolder;

	[SerializeField]
	protected Transform _handLeft;

	[FormerlySerializedAs("_anim")]
	[SerializeField]
	protected Animator _animator;

	[SerializeField]
	protected LineRenderer _line;

	[SerializeField]
	protected List<Transform> _linePoints = new List<Transform>();

	[SerializeField]
	protected AudioSource _slowReelSource;

	[SerializeField]
	protected AudioSource _castReelSoundSource;

	[SerializeField]
	protected float _reelAudioPitchMulti;

	[SerializeField]
	protected float _reelAudioVolMulti;

	[SerializeField]
	protected float _castReelAudioPitchMulti;

	[SerializeField]
	protected float _castReelAudioVolMulti;

	[SerializeField]
	protected float _reelAudioMaxVol;

	[SerializeField]
	protected float _minLineLength;

	[SerializeField]
	protected float _maxLineLength;

	[SerializeField]
	protected float _lineReelStepLength;

	[SerializeField]
	protected float _lineReelDamping;

	[SerializeField]
	protected float _holdReelSpeed;

	[SerializeField]
	protected float _reelMultiIncreaseSpeed;

	[SerializeField]
	protected float _reelMultiDecreaseSpeed;

	[SerializeField]
	protected float _baitStuckDistToTP;

	[SerializeField]
	protected float _baitStuckTimeToTP;

	[SerializeField]
	protected float _lineThrowSpeed;

	[Tooltip("Line length is the X axis and bait drag is the Y axis. This drag is applied while the line is not reeling out.")]
	[SerializeField]
	protected AnimationCurve _baitDragByLineLength = AnimationCurve.Linear(0f, 25f, 2f, 0f);

	[SerializeField]
	protected float _crankRotMulti;

	[SerializeField]
	protected Vector2 _minMaxFishVelForDamage;

	[SerializeField]
	protected float _velDamageMulti;

	[SerializeField]
	protected HeldInfo _dropInfo;

	[SerializeField]
	protected HeldInfo _pullBackRodInfo;

	[SerializeField]
	protected HeldInfo _throwInfo;

	[SerializeField]
	protected HeldInfo _reelInfo;

	[SerializeField]
	protected HeldInfo _reelOrDropInfo;

	[Header("Pull Back")]
	[SerializeField]
	[Range(0f, 1f)]
	protected float _releaseBaitRotPercent = 0.375f;

	[SerializeField]
	protected AnimationCurve _castingRotCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	protected float _castingRodRotSpeed = 5f;

	[SerializeField]
	protected float _castingRodRotBackSpeed = 1.5f;

	[SerializeField]
	private float _castVol = 1f;

	protected int _curLineLengthMulti;

	protected int _receivedCurLineLengthMulti;

	protected float _lineReelVelRef;

	protected float _holdReelTimer;

	protected float _curReelSpeedMulti = 1f;

	protected bool _isReelingIn;

	protected bool _isReelingOut;

	protected bool _isDisabled;

	protected float _curRodRot;

	protected float _receivedRodRot;

	protected bool _isPullingBack;

	private Vector3 _receivedBaitPos;

	private Quaternion[] _rodPartUnbentLocalRotations = Array.Empty<Quaternion>();

	private Quaternion[] _rodPartCurrentBends = Array.Empty<Quaternion>();

	private float _receivedBaitForce;

	private float _curRodBendForce;

	private float _curRodBendForceRefVel;

	private float _baitStuckDistToTPSqr;

	private float _baitStuckTimer;

	private bool _rodPartsWereBent;

	private bool _receivedRodRotWasDecreasing;

	private bool _receivedRodRotWasIncreasing;

	private bool _hasReceivedRodRot;

	private static readonly int PullBackAnim = Animator.StringToHash("PullBack");

	private bool NetworkInitialize___EarlyFishingRodAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateFishingRodAssembly_002DCSharp_002Edll_Excuted;

	[Header("Fishing Rod")]
	[field: SerializeField]
	public BaitInfo DefaultBait { get; private set; }

	public float TimeOfEquip { get; private set; }

	public Bait Bait => _bait;

	public Vector2 MinMaxFishVelForDamage => _minMaxFishVelForDamage;

	public float VelDamageMulti => _velDamageMulti;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_FishingRod_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_bait.transform.SetParent(Server.Instance.DynamicObjectsHolder);
		base.TimeManager.OnTick += TickUpdate;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		base.TimeManager.OnTick += TickUpdate;
	}

	private void OnDisable()
	{
		RestoreRodPartsBeforeBend();
		ResetRodPartBends();
		_curRodBendForce = 0f;
		_isDisabled = true;
		_bait.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if ((bool)_bait)
		{
			UnityEngine.Object.Destroy(_bait.gameObject);
		}
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)_holder)
		{
			if (!_holder.Owner.IsLocalClient)
			{
				float t = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
				_curRodRot = Mathf.Lerp(_curRodRot, _receivedRodRot, t);
				SetPullBackAnimation(_curRodRot);
			}
			else
			{
				TeleportIfStuck();
				RotateRod();
			}
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if ((bool)_holder)
		{
			RestoreRodPartsBeforeBend();
			if ((bool)_holder && !_holder.Owner.IsLocalClient)
			{
				SetReceivedBaitPos();
			}
			BendRodTowardsBait();
			for (int i = 0; i < _linePoints.Count; i++)
			{
				_line.SetPosition(i, _linePoints[i].position);
			}
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (_isDisabled)
		{
			_isDisabled = false;
			ResetBait();
		}
		if (_isReelingIn)
		{
			_holdReelTimer += _holdReelSpeed * _curReelSpeedMulti * Time.fixedDeltaTime;
			if (_holdReelTimer >= 1f)
			{
				_holdReelTimer = 0f;
				DecreaseLineLength(1);
			}
		}
		_curReelSpeedMulti = Mathf.Lerp(_curReelSpeedMulti, 1f, _reelMultiDecreaseSpeed * Time.fixedDeltaTime);
	}

	public override void OnPickUp()
	{
		base.OnPickUp();
		TimeOfEquip = Time.time;
		_isDisabled = true;
		_bait.SetBaitInfo(GetCurBait());
		_bait.ToggleDrag(enable: true);
		float spatialBlend = ((!_holder || !_holder.Owner.IsLocalClient) ? 1 : 0);
		_castReelSoundSource.spatialBlend = spatialBlend;
		_slowReelSource.spatialBlend = spatialBlend;
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			_holder.Inventory.OnRodPickup(this);
		}
	}

	public override void OnDrop()
	{
		RestoreRodPartsBeforeBend();
		ResetRodPartBends();
		_bait.gameObject.SetActive(value: false);
		_bait.Rig.isKinematic = true;
		_isReelingIn = false;
		_isReelingOut = false;
		_holdReelTimer = 0f;
		_curLineLengthMulti = 0;
		_lineReelVelRef = 0f;
		_curReelSpeedMulti = 1f;
		_curRodBendForce = 0f;
		_castReelSoundSource.volume = 0f;
		_slowReelSource.volume = 0f;
		ResetPullBack();
		_receivedRodRotWasDecreasing = false;
		_receivedRodRotWasIncreasing = false;
		_hasReceivedRodRot = false;
		ReleaseItem(_bait.ItemOnBait);
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			_holder.Inventory.OnRodDrop();
		}
		base.OnDrop();
	}

	public override void PrimaryInput(InputAction.CallbackContext context)
	{
		ReelInInput(context);
	}

	public override void PrimaryInputCancel(InputAction.CallbackContext context)
	{
		ReelInCancelled(context);
	}

	public override void SecondaryInput(InputAction.CallbackContext context)
	{
		ReelOutInput(context);
	}

	public override void SecondaryInputCanceled(InputAction.CallbackContext context)
	{
		ReelOutCancelled(context);
	}

	protected virtual void ReelInInput(InputAction.CallbackContext context)
	{
	}

	protected virtual void ReelInCancelled(InputAction.CallbackContext context)
	{
	}

	protected void UpdateBaitDrag(float lineLength)
	{
		if (!_isReelingOut)
		{
			_bait.SetRodDrag(Mathf.Max(0f, _baitDragByLineLength.Evaluate(lineLength)));
		}
		else
		{
			_bait.ToggleDrag(enable: false);
		}
	}

	protected virtual void ReelOutInput(InputAction.CallbackContext context)
	{
		if (CanStartPullBack())
		{
			if ((bool)_bait.ItemOnBait)
			{
				ReleaseItem(_bait.ItemOnBait);
				return;
			}
			_isReelingIn = false;
			_isPullingBack = true;
			AudioManager.PlayRandomPlayerClip("FishingrodPullBack_V", 1, 2, base.Holder, variation: false, AudioDistance.VeryShort, 0.2f);
			OnPullBackStarted();
		}
	}

	protected virtual void ReelOutCancelled(InputAction.CallbackContext context)
	{
		if (!_bait.ItemOnBait && _isPullingBack)
		{
			_isPullingBack = false;
			OnPullBackReleased(_curRodRot);
			if (_curRodRot > _releaseBaitRotPercent)
			{
				AudioManager.PlayRandomPlayerClip("FishingrodCast_V", 1, 2, base.Holder, variation: false, AudioDistance.VeryShort, _curRodRot * _castVol);
			}
		}
	}

	[ObserversRpc]
	public void ReceiveBaitPosAndLineLength(Vector3 baitPos, int curLineLengthMulti, float curBaitForce, Channel channel = Channel.Unreliable)
	{
		RpcWriter___ReceiveBaitPosAndLineLength___1797574021(baitPos, curLineLengthMulti, curBaitForce, channel);
	}

	[ObserversRpc]
	public void ReceiveRodPullBack(float rodRot, bool additionalState, Channel channel = Channel.Unreliable)
	{
		RpcWriter___ReceiveRodPullBack___982844936(rodRot, additionalState, channel);
	}

	public void ReleaseItem(Item item)
	{
		if ((bool)_bait.ItemOnBait && !(_bait.ItemOnBait != item))
		{
			if (_holder.Owner.IsLocalClient)
			{
				Server.Instance.ReleaseItemFromBait(this);
			}
			if (!base.IsServerInitialized)
			{
				_bait.ItemOnBait.RemoveBaitJointOnItemReleased(this);
			}
		}
	}

	protected virtual void TickUpdate()
	{
		if (!_isDisabled && (bool)_holder && _holder.Owner.IsLocalClient)
		{
			Server.Instance.UpdateBaitPosAndLineLength(this, _bait.transform.position, _curLineLengthMulti, _bait.JointForceMagnitude);
			Server.Instance.UpdateRodPullBack(this, _curRodRot, GetAdditionalRodState());
		}
	}

	protected virtual bool CanStartPullBack()
	{
		return true;
	}

	protected virtual void OnPullBackStarted()
	{
	}

	protected virtual void OnPullBackReleased(float releasePoint)
	{
	}

	protected virtual void OnPullBackReset()
	{
	}

	protected virtual bool GetAdditionalRodState()
	{
		return false;
	}

	protected virtual void ReceiveAdditionalRodState(bool additionalState)
	{
	}

	protected virtual void UpdateReleasedPullBack()
	{
	}

	private void RotateRod()
	{
		if (_isPullingBack && (bool)_bait.ItemOnBait)
		{
			ResetPullBack();
			return;
		}
		_curRodRot += (_isPullingBack ? _castingRodRotBackSpeed : (0f - _castingRodRotSpeed)) * Time.deltaTime;
		_curRodRot = Mathf.Clamp01(_curRodRot);
		SetPullBackAnimation(_curRodRot);
		if (!_isPullingBack)
		{
			UpdateReleasedPullBack();
		}
	}

	private void ResetPullBack()
	{
		_isPullingBack = false;
		_curRodRot = 0f;
		SetPullBackAnimation(0f);
		OnPullBackReset();
	}

	private void SetPullBackAnimation(float rodRot)
	{
		_animator.SetFloat(PullBackAnim, _castingRotCurve.Evaluate(rodRot));
	}

	public void ResetAnimator()
	{
		_animator.enabled = true;
		_animator.Rebind();
		_animator.Update(0f);
	}

	private void SetReceivedBaitPos()
	{
		if ((bool)_bait.ItemOnBait)
		{
			Transform transform = _bait.ItemOnBait.transform;
			Quaternion quaternion = transform.rotation * Quaternion.Euler(90f, 0f, 0f);
			_bait.transform.rotation = quaternion;
			Vector3 vector = quaternion * _bait.Info.HookPoint;
			Vector3 vector2 = (_bait.ItemOnBait.AttachBaitOffset ? _bait.ItemOnBait.AttachBaitOffset.position : transform.position);
			_bait.transform.position = vector2 - vector;
		}
		else
		{
			float t = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
			_bait.transform.position = Vector3.Lerp(_bait.transform.position, _receivedBaitPos, t);
			_bait.transform.up = _tipJoint.position - _bait.transform.position;
		}
	}

	private void TeleportIfStuck()
	{
		if (_isReelingOut || _curLineLengthMulti != 0 || !((_tipJoint.position - _bait.transform.position).sqrMagnitude > _baitStuckDistToTPSqr))
		{
			_baitStuckTimer = 0f;
			return;
		}
		_baitStuckTimer += Time.fixedDeltaTime;
		if (_baitStuckTimer >= _baitStuckTimeToTP)
		{
			Vector3 position = _tipJoint.position;
			position += (_bait.transform.position - _tipJoint.position).normalized * _baitStuckDistToTP;
			_bait.transform.position = position;
		}
	}

	protected void DecreaseLineLength(int amount)
	{
		_curLineLengthMulti -= amount;
		if (_curLineLengthMulti < 0)
		{
			_curLineLengthMulti = 0;
		}
	}

	private void ResetBait()
	{
		if (!_holder)
		{
			return;
		}
		_bait.Rig.isKinematic = !_holder.Owner.IsLocalClient;
		_bait.gameObject.SetActive(value: true);
		_curLineLengthMulti = 0;
		_lineReelVelRef = 0f;
		_isReelingOut = false;
		_isReelingIn = true;
		_bait.SetLineLength(_minLineLength);
		if (_holder.Owner.IsLocalClient)
		{
			if (Physics.Raycast(Player.LocalPlayer.CamObject.position, Player.LocalPlayer.CamObject.forward, out var hitInfo, 2.5f, GameInfo.LevelLayer))
			{
				Vector3 position = Vector3.Lerp(Player.LocalPlayer.CamObject.position, hitInfo.point, 0.9f);
				_bait.transform.position = position;
			}
			else
			{
				_bait.transform.position = Player.LocalPlayer.CamObject.position + Player.LocalPlayer.CamObject.forward * 2f;
			}
			if (!_bait.Rig.isKinematic)
			{
				_bait.Rig.linearVelocity = Vector3.zero;
			}
		}
	}

	public BaitInfo GetCurBait()
	{
		if (_holder.Inventory.CurBait != 0)
		{
			return GameInfo.AllBaits[_holder.Inventory.CurBait];
		}
		return DefaultBait;
	}

	private void InitializeRodPartRots()
	{
		if (_rodParts == null)
		{
			_rodPartUnbentLocalRotations = Array.Empty<Quaternion>();
			_rodPartCurrentBends = Array.Empty<Quaternion>();
			return;
		}
		_rodPartUnbentLocalRotations = new Quaternion[_rodParts.Length];
		_rodPartCurrentBends = new Quaternion[_rodParts.Length];
		for (int i = 0; i < _rodParts.Length; i++)
		{
			_rodPartUnbentLocalRotations[i] = (_rodParts[i] ? _rodParts[i].localRotation : Quaternion.identity);
			_rodPartCurrentBends[i] = Quaternion.identity;
		}
	}

	private void RestoreRodPartsBeforeBend()
	{
		if (!_rodPartsWereBent)
		{
			return;
		}
		for (int i = 0; i < _rodParts.Length; i++)
		{
			if ((bool)_rodParts[i])
			{
				_rodParts[i].localRotation = _rodPartUnbentLocalRotations[i];
			}
		}
		_rodPartsWereBent = false;
	}

	private void ResetRodPartBends()
	{
		for (int i = 0; i < _rodPartCurrentBends.Length; i++)
		{
			_rodPartCurrentBends[i] = Quaternion.identity;
		}
	}

	private void BendRodTowardsBait()
	{
		if (_rodPartCurrentBends.Length != _rodParts.Length || _rodPartUnbentLocalRotations.Length != _rodParts.Length)
		{
			InitializeRodPartRots();
		}
		float value = _receivedBaitForce;
		if (_holder.Owner.IsLocalClient)
		{
			value = _rodBendForceMultiCurve.Evaluate(_bait.JointForceMagnitude);
		}
		float target = Mathf.InverseLerp(0f, _baitJointForceForMaxBend, value);
		_curRodBendForce = Mathf.SmoothDamp(_curRodBendForce, target, ref _curRodBendForceRefVel, _rodBendForceSmoothSpeed);
		Vector3 position = _bait.transform.position;
		float num = _maxRodBendAngle * _curRodBendForce;
		bool flag = false;
		for (int i = 0; i < _rodParts.Length; i++)
		{
			Transform transform = _rodParts[i];
			if (!transform)
			{
				continue;
			}
			_rodPartUnbentLocalRotations[i] = transform.localRotation;
			Vector3 rodPartSegmentDirection = GetRodPartSegmentDirection(i);
			Vector3 vector = position - transform.position;
			if (!(rodPartSegmentDirection.sqrMagnitude < 0.0001f) && !(vector.sqrMagnitude < 0.0001f))
			{
				float time = ((_rodParts.Length == 1) ? 1f : ((float)i / (float)(_rodParts.Length - 1)));
				float num2 = Mathf.Clamp01(_rodBendDistribution.Evaluate(time));
				Quaternion b = Quaternion.identity;
				if (_curRodBendForce > 0.001f && num2 > 0f)
				{
					Vector3 toDirection = Vector3.RotateTowards(rodPartSegmentDirection.normalized, vector.normalized, num * num2 * (MathF.PI / 180f), 0f);
					b = Quaternion.FromToRotation(rodPartSegmentDirection.normalized, toDirection);
				}
				_rodPartCurrentBends[i] = Quaternion.Slerp(_rodPartCurrentBends[i], b, _rodBendRotSmoothSpeed * Time.deltaTime);
				Quaternion quaternion = _rodPartCurrentBends[i] * transform.rotation;
				Quaternion localRotation = (transform.parent ? (Quaternion.Inverse(transform.parent.rotation) * quaternion) : quaternion);
				transform.localRotation = localRotation;
				flag |= Quaternion.Angle(Quaternion.identity, _rodPartCurrentBends[i]) > 0.01f;
			}
		}
		if (!flag)
		{
			ResetRodPartBends();
			for (int j = 0; j < _rodParts.Length; j++)
			{
				if ((bool)_rodParts[j])
				{
					_rodParts[j].localRotation = _rodPartUnbentLocalRotations[j];
				}
			}
		}
		_rodPartsWereBent = flag;
	}

	private Vector3 GetRodPartSegmentDirection(int index)
	{
		Transform transform = _rodParts[index];
		for (int i = index + 1; i < _rodParts.Length; i++)
		{
			if ((bool)_rodParts[i])
			{
				return _rodParts[i].position - transform.position;
			}
		}
		return _tipJoint.position - transform.position;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyFishingRodAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyFishingRodAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(3u, RpcReader___ReceiveBaitPosAndLineLength___1797574021);
			RegisterObserversRpc(4u, RpcReader___ReceiveRodPullBack___982844936);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateFishingRodAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateFishingRodAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ReceiveBaitPosAndLineLength___1797574021(Vector3 baitPos, int curLineLengthMulti, float curBaitForce, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(baitPos);
		pooledWriter.WriteInt32(curLineLengthMulti);
		pooledWriter.WriteSingle(curBaitForce);
		SendObserversRpc(3u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ReceiveBaitPosAndLineLength___1797574021(Vector3 P_0, int P_1, float P_2, Channel P_3)
	{
		_receivedBaitPos = P_0;
		_receivedCurLineLengthMulti = P_1;
		_receivedBaitForce = P_2;
	}

	private void RpcReader___ReceiveBaitPosAndLineLength___1797574021(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		int num = PooledReader0.ReadInt32();
		float num2 = PooledReader0.ReadSingle();
		if (base.IsClientInitialized)
		{
			RpcLogic___ReceiveBaitPosAndLineLength___1797574021(vector, num, num2, channel);
		}
	}

	private void RpcWriter___ReceiveRodPullBack___982844936(float rodRot, bool additionalState, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteSingle(rodRot);
		pooledWriter.WriteBoolean(additionalState);
		SendObserversRpc(4u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ReceiveRodPullBack___982844936(float P_0, bool P_1, Channel P_2)
	{
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			return;
		}
		if (!_hasReceivedRodRot)
		{
			_receivedRodRot = P_0;
			_hasReceivedRodRot = true;
			ReceiveAdditionalRodState(P_1);
			return;
		}
		float num = P_0 - _receivedRodRot;
		bool flag = num < -0.001f;
		bool flag2 = num > 0.001f;
		if (flag && !_receivedRodRotWasDecreasing && (bool)_holder)
		{
			AudioManager.PlayRandomPlayerClip("FishingrodCast_V", 1, 2, base.Holder, variation: false, AudioDistance.VeryShort, _receivedRodRot);
		}
		if (flag2 && !_receivedRodRotWasIncreasing && (bool)_holder)
		{
			AudioManager.PlayRandomPlayerClip("FishingrodPullBack_V", 1, 2, base.Holder, variation: false, AudioDistance.VeryShort, 0.2f);
		}
		if (Mathf.Abs(num) > 0.001f)
		{
			_receivedRodRotWasDecreasing = flag;
			_receivedRodRotWasIncreasing = flag2;
		}
		_receivedRodRot = P_0;
		ReceiveAdditionalRodState(P_1);
	}

	private void RpcReader___ReceiveRodPullBack___982844936(PooledReader PooledReader0, Channel channel)
	{
		float num = PooledReader0.ReadSingle();
		bool flag = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized)
		{
			RpcLogic___ReceiveRodPullBack___982844936(num, flag, channel);
		}
	}

	protected virtual void Awake_UserLogic_FishingRod_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_fishingRod = this;
		_linePoints.Add(_bait.transform);
		_line.positionCount = _linePoints.Count;
		InitializeRodPartRots();
		_baitStuckDistToTPSqr = _baitStuckDistToTP * _baitStuckDistToTP;
		ResetAnimator();
	}
}
