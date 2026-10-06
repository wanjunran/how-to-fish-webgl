using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Melee : Tool
{
	[Header("Melee")]
	[Header("Assign right then left in all arrays")]
	[SerializeField]
	private Transform[] _hands = new Transform[2];

	[SerializeField]
	private bool _useBothHands;

	[SerializeField]
	private float _range;

	[SerializeField]
	private SharpnessUpgrade[] _sharpnessUpgrades;

	[SerializeField]
	private float _hitRadius;

	[SerializeField]
	private float _force;

	[SerializeField]
	[Tooltip("Prevent fist from going inside whatever was hit")]
	private float _hitOffsetDist;

	[FormerlySerializedAs("_moveMeleeSpeed")]
	[SerializeField]
	private float _moveForwardSpeed;

	[FormerlySerializedAs("_moveMeleeCurve")]
	[SerializeField]
	private AnimationCurve _moveForwardCurve;

	[SerializeField]
	private AnimationCurve _moveForwardXCurve;

	[FormerlySerializedAs("_returnMeleeSpeed")]
	[SerializeField]
	private float _returnSpeed;

	[FormerlySerializedAs("_returnMeleeSpeedCurve")]
	[SerializeField]
	private AnimationCurve _returnSpeedCurve;

	[SerializeField]
	private AnimationCurve _returnXCurve;

	[SerializeField]
	private Vector3[] _hitRots;

	[SerializeField]
	private AnimationCurve _rotCurve;

	[SerializeField]
	private Vector3 _noTargetHitPos;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("After this percent of return has happened, a new attack can begin (instantly not queued)")]
	private float _returnPercentForNextAttack = 0.5f;

	[Header("Audio")]
	[SerializeField]
	private string _swingSound = "PunchSwoosh";

	[SerializeField]
	[Tooltip("0 = only one, more than 0 calls PlayRandomPlayerClip()")]
	private int _swingSounds;

	[SerializeField]
	private float _swingVol = 0.25f;

	[SerializeField]
	private string _levelHitSound;

	[SerializeField]
	private float _levelHitSoundVol;

	private int _lastAttackSide = 1;

	private int _attackSwingDirMulti;

	private bool _queuedAttack;

	public readonly SyncVar<byte> _syncedSharpnessIndex = new SyncVar<byte>();

	private readonly bool[] _isAttacking = new bool[2];

	private readonly bool[] _isReturning = new bool[2];

	private readonly bool[] _hitNothing = new bool[2];

	private readonly float[] _curAttackPercent = new float[2];

	private readonly Transform[] _curTarget = new Transform[2];

	private readonly Vector3[] _targetHitPoint = new Vector3[2];

	private readonly Quaternion[] _origRot = new Quaternion[2];

	private readonly Vector3[] _origPos = new Vector3[2];

	private readonly Vector3[] _origPosTP = new Vector3[2];

	private readonly Vector3[] _attackStartPos = new Vector3[2];

	private readonly Quaternion[] _attackStartRot = new Quaternion[2];

	private readonly Vector3[] _hitPos = new Vector3[2];

	private readonly Transform[] _hitTransform = new Transform[2];

	private bool NetworkInitialize___EarlyMeleeAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateMeleeAssembly_002DCSharp_002Edll_Excuted;

	[field: SerializeField]
	public bool UsePunchSoundOnHit { get; private set; }

	public byte SharpnessIndex => _syncedSharpnessIndex.Value;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Melee_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnPickUp()
	{
		base.OnPickUp();
		PlayIdleAnim();
	}

	public override void OnDrop()
	{
		base.OnDrop();
		for (int i = 0; i < 2; i++)
		{
			_isAttacking[i] = false;
			_isReturning[i] = false;
			_curAttackPercent[i] = 0f;
		}
		PlayIdleAnim();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if ((bool)_holder)
		{
			if (_queuedAttack && CanAttack())
			{
				FindAttackTarget();
			}
			int num = ((!_useBothHands) ? 1 : 2);
			for (int i = 0; i < num; i++)
			{
				UpdateHand(i);
			}
		}
	}

	private void UpdateHand(int side)
	{
		if (_isAttacking[side])
		{
			MoveAttack(side);
		}
		else if (_isReturning[side])
		{
			MoveReturn(side);
		}
		if (_isAttacking[side] || _isReturning[side])
		{
			MoveRotation(side);
		}
	}

	private void MoveRotation(int side)
	{
		int num = side;
		if (!_useBothHands)
		{
			num = ((_attackSwingDirMulti != 1) ? 1 : 0);
		}
		Quaternion a = (_isAttacking[side] ? _attackStartRot[side] : _origRot[side]);
		_hands[side].localRotation = Quaternion.Slerp(a, Quaternion.Euler(_hitRots[num]), _rotCurve.Evaluate(_curAttackPercent[side]));
	}

	private void MoveAttack(int side)
	{
		Vector3 a = _holder.CamObject.TransformPoint(_attackStartPos[side]);
		if ((bool)_curTarget[side])
		{
			Vector3 b = _curTarget[side].transform.TransformPoint(_targetHitPoint[side]);
			_hands[side].position = Vector3.Lerp(a, b, _moveForwardCurve.Evaluate(_curAttackPercent[side]));
		}
		else
		{
			Vector3 b2 = _holder.CamObject.TransformPoint(_noTargetHitPos);
			_hands[side].position = Vector3.Lerp(a, b2, _moveForwardCurve.Evaluate(_curAttackPercent[side]));
		}
		_hands[side].position += _holder.CamObject.right * ((float)_attackSwingDirMulti * _moveForwardXCurve.Evaluate(_curAttackPercent[side]));
		_curAttackPercent[side] += _moveForwardSpeed * Time.deltaTime;
		if (_curAttackPercent[side] >= 1f)
		{
			StartReturning(side);
		}
	}

	private void MoveReturn(int side)
	{
		_curAttackPercent[side] -= _returnSpeed * Time.deltaTime;
		if (_curAttackPercent[side] <= 0f)
		{
			_curAttackPercent[side] = 0f;
			_isReturning[side] = false;
		}
		Vector3 a = _hands[side].parent.TransformPoint(_holder.Owner.IsLocalClient ? _origPos[side] : _origPosTP[side]);
		if (_hitNothing[side])
		{
			Vector3 b = _holder.CamObject.TransformPoint(_noTargetHitPos);
			_hands[side].position = Vector3.Lerp(a, b, _returnSpeedCurve.Evaluate(_curAttackPercent[side]));
		}
		else
		{
			if (!_hitTransform[side])
			{
				_hitNothing[side] = true;
				return;
			}
			Vector3 b2 = _hitTransform[side].TransformPoint(_hitPos[side]);
			_hands[side].position = Vector3.Lerp(a, b2, _returnSpeedCurve.Evaluate(_curAttackPercent[side]));
		}
		_hands[side].position += _holder.CamObject.right * ((float)_attackSwingDirMulti * _returnXCurve.Evaluate(_curAttackPercent[side]));
	}

	public override void PrimaryInput(InputAction.CallbackContext context)
	{
		if (CanAttack())
		{
			FindAttackTarget();
		}
		else
		{
			_queuedAttack = true;
		}
	}

	private void FindAttackTarget(bool extraCheckMidAttack = false, int side = 0)
	{
		_queuedAttack = false;
		CheckForItems(out var finalTarget, out var finalHitPoint);
		if (!finalTarget)
		{
			CheckForPlayersAndLevel(out finalTarget, out finalHitPoint);
		}
		if (extraCheckMidAttack)
		{
			_curTarget[side] = finalTarget;
			_targetHitPoint[side] = finalHitPoint;
			StartReturning(side, extraHit: true);
		}
		else
		{
			side = ((_useBothHands && _lastAttackSide == 0) ? 1 : 0);
			StartAttacking(finalTarget, side, calledFromLocal: true, finalHitPoint);
		}
	}

	public override void InspectInput(InputAction.CallbackContext context)
	{
		if (!_isReturning[0] && !_isAttacking[0])
		{
			PlayInspectAnim(calledFromLocal: true);
			string text = $"<b>{_syncedSharpnessIndex.Value}</b> {LocalizationManager.SharpnessLocalized.GetLocalizedString()}";
			text += $"\n<b>{_sharpnessUpgrades[_syncedSharpnessIndex.Value].Damage}</b> {LocalizationManager.DamageLocalized.GetLocalizedString()}";
			PlayerUI.ShowInspectInfo(this, text);
		}
	}

	[ObserversRpc]
	public void ObserverStartAttacking(Transform target, bool right, Vector3 targetHitPoint)
	{
		RpcWriter___ObserverStartAttacking___3363832400(target, right, targetHitPoint);
	}

	private void StartAttacking(Transform target, int side, bool calledFromLocal, Vector3 targetHitPoint)
	{
		if ((bool)_holder && (!_holder.Owner.IsLocalClient || calledFromLocal))
		{
			if (calledFromLocal)
			{
				Server.Instance.MeleeAttack(this, target, side == 0, targetHitPoint);
			}
			_curAttackPercent[side] = 0f;
			_attackSwingDirMulti = ((Random.Range(0, 2) != 0) ? 1 : (-1));
			_attackStartPos[side] = _holder.CamObject.InverseTransformPoint(_hands[side].position);
			_attackStartRot[side] = _hands[side].localRotation;
			_curTarget[side] = target;
			_targetHitPoint[side] = targetHitPoint;
			_isAttacking[side] = true;
			if (_useBothHands)
			{
				_lastAttackSide = side;
			}
			if (_swingSounds == 0)
			{
				AudioManager.PlayPlayerClip(_swingSound, _holder, variation: true, AudioDistance.VeryShort, _swingVol, 0.02f);
			}
			else
			{
				AudioManager.PlayRandomPlayerClip(_swingSound, 1, _swingSounds, _holder, variation: true, AudioDistance.VeryShort, _swingVol, 0.02f);
			}
			if (_anim.isPlaying && !_anim.IsPlaying("Idle"))
			{
				PlayIdleAnim();
			}
		}
	}

	private void StartReturning(int side, bool extraHit = false)
	{
		_isAttacking[side] = false;
		_isReturning[side] = true;
		_hitNothing[side] = !_curTarget[side];
		_queuedAttack = false;
		_hitTransform[side] = _curTarget[side];
		_hitPos[side] = (_hitTransform[side] ? _hitTransform[side].InverseTransformPoint(_hands[side].position) : _hands[side].position);
		if (!_curTarget[side])
		{
			if (!extraHit)
			{
				FindAttackTarget(extraCheckMidAttack: true, side);
			}
		}
		else
		{
			HitTarget(side);
		}
	}

	private void HitTarget(int side)
	{
		int damage = (ServerSettings.OneShotEnabled ? 99999 : _sharpnessUpgrades[_syncedSharpnessIndex.Value].Damage);
		Vector3 forward = _holder.CamObject.forward;
		Vector3 vector = _curTarget[side].transform.TransformPoint(_targetHitPoint[side]);
		Item item = ItemManager.Get(_curTarget[side]);
		if ((bool)item)
		{
			Vector3 force = forward * _force;
			if (force.y < 0f)
			{
				force.y = 0f;
			}
			item.LocalHit(_curTarget[side].transform, _curTarget[side].transform.TransformPoint(_targetHitPoint[side]), forward, _holder, damage, rangedHit: false, force);
			return;
		}
		Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(_curTarget[side]);
		if ((bool)playerFromBodyPart)
		{
			playerFromBodyPart.Vitals.LocalHit(vector, forward, _holder, damage, rangedHit: false, forward * GameInfo.PlayerKillForce);
			return;
		}
		if (_curTarget[side].CompareTag("NPC"))
		{
			DazedUtils.PlayDeadPlayerHitEffects(vector, forward, damage, base.Holder, noDecals: true);
		}
		if (!_curTarget[side].CompareTag("Level") && !_curTarget[side].CompareTag("Boat"))
		{
			return;
		}
		AudioManager.PlayClipAt(_levelHitSound, vector, variation: true, AudioDistance.VeryShort, _levelHitSoundVol);
		if (base.IsServerInitialized && _curTarget[side].CompareTag("Boat"))
		{
			Vector3 force2 = _holder.CamObject.forward * GameInfo.BoatProjectileForce;
			if (force2.y < 0f)
			{
				force2.y = 0f;
			}
			BoatManager.Boat.HiddenPhysicsRig.AddForceAtPosition(force2, vector);
		}
	}

	private void CheckForItems(out Transform finalTarget, out Vector3 finalHitPoint)
	{
		Vector3 position = _holder.CamObject.position;
		Vector3 forward = _holder.CamObject.forward;
		LayerMask layerMask = (int)GameInfo.ItemPartLayer | (int)GameInfo.LevelLayer | (int)GameInfo.PlayerLayers;
		RaycastHit[] array = Physics.SphereCastAll(position, _hitRadius, forward, _range, layerMask);
		Dictionary<Transform, (float, Vector3)> dictionary = new Dictionary<Transform, (float, Vector3)>();
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit hit = array2[i];
			Item item = ItemManager.Get(hit.collider);
			if ((bool)item && item.IsInteractable && !IsBehindWall(hit))
			{
				Vector3 item2 = ((hit.point != Vector3.zero) ? hit.point : hit.collider.ClosestPoint(position + forward * Mathf.Max(hit.distance, 0.01f)));
				if (!dictionary.TryGetValue(hit.transform, out var value) || hit.distance < value.Item1)
				{
					dictionary[hit.transform] = (hit.distance, item2);
				}
			}
		}
		finalTarget = null;
		finalHitPoint = Vector3.zero;
		if (dictionary.Count == 0)
		{
			return;
		}
		float num = float.PositiveInfinity;
		foreach (KeyValuePair<Transform, (float, Vector3)> item3 in dictionary)
		{
			if (!(item3.Value.Item1 > num))
			{
				num = item3.Value.Item1;
				finalTarget = item3.Key;
				finalHitPoint = item3.Value.Item2 - _holder.CamObject.forward * _hitOffsetDist;
				finalHitPoint = finalTarget.InverseTransformPoint(finalHitPoint);
			}
		}
	}

	private void CheckForPlayersAndLevel(out Transform finalTarget, out Vector3 finalHitPoint)
	{
		RaycastHit[] obj = Physics.RaycastAll(_holder.CamObject.position, _holder.CamObject.forward, layerMask: (LayerMask)((int)GameInfo.ItemPartLayer | (int)GameInfo.LevelLayer | (int)GameInfo.PlayerLayers | (int)GameInfo.BoatLayer | (int)GameInfo.NpcLayer), maxDistance: _range);
		(Transform, float, Vector3) tuple = default;
		RaycastHit[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(raycastHit.transform);
			if ((!playerFromBodyPart || !playerFromBodyPart.IsOwner) && (!tuple.Item1 || raycastHit.distance < tuple.Item2))
			{
				tuple = (raycastHit.transform, raycastHit.distance, raycastHit.point);
			}
		}
		finalTarget = null;
		finalHitPoint = Vector3.zero;
		if ((bool)tuple.Item1)
		{
			if (tuple.Item1.CompareTag("Boat"))
			{
				tuple.Item1 = BoatManager.Boat.VisualBoat;
			}
			finalTarget = tuple.Item1;
			finalHitPoint = tuple.Item3 - _holder.CamObject.forward * _hitOffsetDist;
			finalHitPoint = finalTarget.InverseTransformPoint(finalHitPoint);
		}
	}

	private bool IsBehindWall(RaycastHit hit)
	{
		Vector3 position = _holder.CamObject.position;
		RaycastHit[] array = Physics.RaycastAll(_holder.CamObject.position, hit.point - position, _range, GameInfo.LevelLayer);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (!(raycastHit.distance * raycastHit.distance > hit.distance))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanAttack()
	{
		for (int i = 0; i < 2; i++)
		{
			if (_isAttacking[i])
			{
				return false;
			}
		}
		if (!_useBothHands && _isReturning[0] && _curAttackPercent[0] > _returnPercentForNextAttack)
		{
			return false;
		}
		if (_isReturning[1])
		{
			return !_isReturning[0];
		}
		return true;
	}

	public void UpgradeSharpness()
	{
		byte value = _syncedSharpnessIndex.Value;
		value++;
		value = (byte)Mathf.Clamp(value, 0, _sharpnessUpgrades.Length);
		_syncedSharpnessIndex.Value = value;
	}

	public SharpnessUpgrade GetCurSharpness()
	{
		return _sharpnessUpgrades[_syncedSharpnessIndex.Value];
	}

	public SharpnessUpgrade GetNextSharpnessUpgrade(byte max = byte.MaxValue)
	{
		if (_syncedSharpnessIndex.Value + 1 >= _sharpnessUpgrades.Length || max <= _syncedSharpnessIndex.Value)
		{
			return null;
		}
		return _sharpnessUpgrades[_syncedSharpnessIndex.Value + 1];
	}

	public override void LoadFromSave(SavedItem savedItem)
	{
		base.LoadFromSave(savedItem);
		_syncedSharpnessIndex.Value = savedItem.Sharpness;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyMeleeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyMeleeAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_syncedSharpnessIndex.InitializeEarly(this, 9u, isSyncObject: false);
			RegisterObserversRpc(3u, RpcReader___ObserverStartAttacking___3363832400);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateMeleeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateMeleeAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_syncedSharpnessIndex.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverStartAttacking___3363832400(Transform target, bool right, Vector3 targetHitPoint)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteTransform(target);
		pooledWriter.WriteBoolean(right);
		pooledWriter.WriteVector3(targetHitPoint);
		SendObserversRpc(3u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverStartAttacking___3363832400(Transform P_0, bool P_1, Vector3 P_2)
	{
		StartAttacking(P_0, (!P_1) ? 1 : 0, calledFromLocal: false, P_2);
	}

	private void RpcReader___ObserverStartAttacking___3363832400(PooledReader PooledReader0, Channel channel)
	{
		Transform transform = PooledReader0.ReadTransform();
		bool flag = PooledReader0.ReadBoolean();
		Vector3 vector = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverStartAttacking___3363832400(transform, flag, vector);
		}
	}

	protected virtual void Awake_UserLogic_Melee_Assembly_002DCSharp_002Edll()
	{
		_melee = this;
		base.Awake();
		for (int i = 0; i < 2; i++)
		{
			_origRot[i] = ((i == 0) ? base.HandTransformsRight.HandRot : base.HandTransformsLeft.HandRot);
			_origPos[i] = ((i == 0) ? base.HandTransformsRight.HandPos : base.HandTransformsLeft.HandPos);
			_origPosTP[i] = _origPos[i] + Vector3.forward * _thirdPersonOffset;
		}
	}
}
