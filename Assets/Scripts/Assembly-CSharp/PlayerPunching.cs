using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerPunching : NetworkBehaviour
{
	private const int RightSide = 0;

	private const int LeftSide = 1;

	[SerializeField]
	private Player _player;

	[SerializeField]
	private float _range;

	[SerializeField]
	private int _damage;

	[SerializeField]
	private float _hitRadius;

	[SerializeField]
	private float _force;

	[SerializeField]
	[Tooltip("Prevent fist from going inside whatever was hit")]
	private float _hitOffsetDist;

	[FormerlySerializedAs("_fistPunchSpeed")]
	[SerializeField]
	private float _moveForwardSpeed;

	[FormerlySerializedAs("_fistPunchSpeedCurve")]
	[SerializeField]
	private AnimationCurve _moveForwardCurve;

	[FormerlySerializedAs("_fistReturnSpeed")]
	[SerializeField]
	private float _returnSpeed;

	[FormerlySerializedAs("_fistReturnSpeedCurve")]
	[SerializeField]
	private AnimationCurve _returnSpeedCurve;

	[SerializeField]
	private Vector3[] _hitRots;

	[SerializeField]
	private AnimationCurve _rotCurve;

	[FormerlySerializedAs("_noTargetPunchPos")]
	[SerializeField]
	private Vector3 _noTargetHitPos;

	[SerializeField]
	private string _levelHitSound;

	[SerializeField]
	private float _levelHitSoundVol;

	private bool _queuedPunch;

	private int _lastPunchSide = 1;

	private bool[] _isAttacking = new bool[2];

	private bool[] _isReturning = new bool[2];

	private float[] _curAttackPercent = new float[2];

	private bool[] _hitNothing = new bool[2];

	private Transform[] _curTarget = new Transform[2];

	private Vector3[] _targetHitPoint = new Vector3[2];

	private Vector3[] _fistStartPos = new Vector3[2];

	private Quaternion[] _fistStartRot = new Quaternion[2];

	private Vector3[] _hitPos = new Vector3[2];

	private Transform[] _hitTransform = new Transform[2];

	private float _timeBetweenFistPunch;

	private bool NetworkInitialize___EarlyPlayerPunchingAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerPunchingAssembly_002DCSharp_002Edll_Excuted;

	public bool IsAnimatingRight
	{
		get
		{
			if (!_isAttacking[0])
			{
				return _isReturning[0];
			}
			return true;
		}
	}

	public bool IsAnimatingLeft
	{
		get
		{
			if (!_isAttacking[1])
			{
				return _isReturning[1];
			}
			return true;
		}
	}

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			BindInputs();
		}
		_timeBetweenFistPunch = 1f / _moveForwardSpeed;
	}

	public override void OnStopClient()
	{
		if (base.Owner.IsLocalClient)
		{
			UnbindInputs();
		}
	}

	private void Update()
	{
		if (_queuedPunch && CanPunch())
		{
			FindPunchTarget();
		}
		for (int i = 0; i < 2; i++)
		{
			UpdateFist(i);
		}
	}

	private void BindInputs()
	{
		GameInfo.Input.actions["PlayerLeftClick"].performed += PunchInput;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["PlayerLeftClick"].performed -= PunchInput;
		}
	}

	private Transform GetFist(int side)
	{
		if (side != 0)
		{
			return _player.Hands.HandBoneLeft;
		}
		return _player.Hands.HandBoneRight;
	}

	private void UpdateFist(int side)
	{
		Transform fist = GetFist(side);
		if (_isAttacking[side])
		{
			MoveAttack(side, fist);
		}
		else if (_isReturning[side])
		{
			MoveReturn(side, fist);
		}
		if (_isAttacking[side] || _isReturning[side])
		{
			MoveRotation(side, fist);
		}
	}

	private void MoveAttack(int side, Transform fist)
	{
		Vector3 a = _player.CamObject.TransformPoint(_fistStartPos[side]);
		if ((bool)_curTarget[side])
		{
			Vector3 b = _curTarget[side].transform.TransformPoint(_targetHitPoint[side]);
			fist.position = Vector3.Lerp(a, b, _moveForwardCurve.Evaluate(_curAttackPercent[side]));
		}
		else
		{
			Vector3 b2 = _player.CamObject.TransformPoint(_noTargetHitPos);
			fist.position = Vector3.Lerp(a, b2, _moveForwardCurve.Evaluate(_curAttackPercent[side]));
		}
		_curAttackPercent[side] += _moveForwardSpeed * Time.deltaTime;
		if (_curAttackPercent[side] >= 1f)
		{
			StartReturning(side);
		}
	}

	private void MoveReturn(int side, Transform fist)
	{
		_curAttackPercent[side] -= _returnSpeed * Time.deltaTime;
		if (_curAttackPercent[side] <= 0f)
		{
			_curAttackPercent[side] = 0f;
			_isReturning[side] = false;
		}
		if (_hitNothing[side])
		{
			Vector3 b = _player.CamObject.TransformPoint(_noTargetHitPos);
			fist.position = Vector3.Lerp(_player.Hands.CurDefaultHandPos(side == 0), b, _returnSpeedCurve.Evaluate(_curAttackPercent[side]));
		}
		else if (!_hitTransform[side])
		{
			_hitNothing[side] = true;
		}
		else
		{
			Vector3 b2 = _hitTransform[side].TransformPoint(_hitPos[side]);
			fist.position = Vector3.Lerp(_player.Hands.CurDefaultHandPos(side == 0), b2, _returnSpeedCurve.Evaluate(_curAttackPercent[side]));
		}
	}

	private void MoveRotation(int side, Transform fist)
	{
		Quaternion a = (_isAttacking[side] ? _fistStartRot[side] : (Quaternion.Inverse(fist.parent.rotation) * _player.Hands.CurDefaultHandRot(side == 0)));
		Quaternion quaternion = _player.CamObject.rotation * Quaternion.Euler(_hitRots[side]);
		Quaternion b = (fist.parent ? (Quaternion.Inverse(fist.parent.rotation) * quaternion) : quaternion);
		fist.localRotation = Quaternion.Slerp(a, b, _rotCurve.Evaluate(_curAttackPercent[side]));
	}

	private void PunchInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && !_player.Holding.HeldItem && !Boat.IsDrivingLocally && !Boat.WantToDrive)
		{
			if (CanPunch())
			{
				FindPunchTarget();
			}
			else
			{
				_queuedPunch = true;
			}
		}
	}

	private void FindPunchTarget(bool extraCheckMidPunch = false, int side = 0)
	{
		_queuedPunch = false;
		CheckForItems(out var finalTarget, out var finalHitPoint);
		if (!finalTarget)
		{
			CheckForPlayersAndLevel(out finalTarget, out finalHitPoint);
		}
		if (extraCheckMidPunch)
		{
			_curTarget[side] = finalTarget;
			_targetHitPoint[side] = finalHitPoint;
			StartReturning(side, extraHit: true);
		}
		else
		{
			side = ((_lastPunchSide == 0) ? 1 : 0);
			PlayerSkills.OnAttack(_timeBetweenFistPunch);
			StartPunching(finalTarget, side, calledFromLocal: true, finalHitPoint);
		}
	}

	[ObserversRpc(ExcludeOwner = true)]
	public void ObserverStartPunching(Transform target, bool right, Vector3 targetHitPoint)
	{
		RpcWriter___ObserverStartPunching___3363832400(target, right, targetHitPoint);
	}

	private void StartPunching(Transform target, int side, bool calledFromLocal, Vector3 targetHitPoint)
	{
		if (calledFromLocal)
		{
			Server.Instance.Punch(_player, target, side == 0, targetHitPoint);
		}
		AudioManager.PlayPlayerClip("PunchSwoosh", _player, variation: true, AudioDistance.VeryShort, 0.25f, 0.02f);
		_lastPunchSide = side;
		_curTarget[side] = target;
		_targetHitPoint[side] = targetHitPoint;
		_isAttacking[side] = true;
		Transform fist = GetFist(side);
		_fistStartPos[side] = _player.CamObject.InverseTransformPoint(fist.position);
		_fistStartRot[side] = fist.localRotation;
	}

	private void StartReturning(int side, bool extraHit = false)
	{
		_isAttacking[side] = false;
		_isReturning[side] = true;
		_hitNothing[side] = !_curTarget[side];
		_queuedPunch = false;
		_hitTransform[side] = _curTarget[side];
		_hitPos[side] = (_hitTransform[side] ? _hitTransform[side].InverseTransformPoint(GetFist(side).position) : GetFist(side).position);
		if (!_curTarget[side])
		{
			if (!extraHit)
			{
				FindPunchTarget(extraCheckMidPunch: true, side);
			}
		}
		else
		{
			HitTarget(side);
		}
	}

	private void HitTarget(int side)
	{
		int damage = (ServerSettings.OneShotEnabled ? 99999 : _damage);
		Vector3 forward = _player.CamObject.forward;
		Vector3 vector = _curTarget[side].transform.TransformPoint(_targetHitPoint[side]);
		Item item = ItemManager.Get(_curTarget[side]);
		if ((bool)item)
		{
			Vector3 force = forward * _force;
			if (force.y < 0f)
			{
				force.y = 0f;
			}
			item.LocalHit(_curTarget[side].transform, _curTarget[side].transform.TransformPoint(_targetHitPoint[side]), forward, _player, damage, rangedHit: false, force);
			return;
		}
		Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(_curTarget[side]);
		if ((bool)playerFromBodyPart)
		{
			playerFromBodyPart.Vitals.LocalHit(vector, forward, _player, damage, rangedHit: false, forward * GameInfo.PlayerKillForce);
			return;
		}
		if (_curTarget[side].CompareTag("NPC"))
		{
			DazedUtils.PlayDeadPlayerHitEffects(vector, forward, damage, _player, noDecals: true);
		}
		if (!_curTarget[side].CompareTag("Level") && !_curTarget[side].CompareTag("Boat"))
		{
			return;
		}
		AudioManager.PlayClipAt(_levelHitSound, vector, variation: true, AudioDistance.VeryShort, _levelHitSoundVol);
		if (base.IsServerInitialized && _curTarget[side].CompareTag("Boat"))
		{
			Vector3 force2 = _player.CamObject.forward * GameInfo.BoatProjectileForce;
			if (force2.y < 0f)
			{
				force2.y = 0f;
			}
			BoatManager.Boat.HiddenPhysicsRig.AddForceAtPosition(force2, vector);
		}
	}

	private void CheckForItems(out Transform finalTarget, out Vector3 finalHitPoint)
	{
		Vector3 position = _player.CamObject.position;
		Vector3 forward = _player.CamObject.forward;
		LayerMask itemPartLayer = GameInfo.ItemPartLayer;
		RaycastHit[] array = Physics.SphereCastAll(position, _hitRadius, forward, _range, itemPartLayer);
		Dictionary<Transform, (float, Vector3)> dictionary = new Dictionary<Transform, (float, Vector3)>();
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit hit = array2[i];
			Item item = ItemManager.Get(hit.collider);
			if ((bool)item && item.IsInteractable && !IsBehindWall(hit, position))
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
				finalHitPoint = item3.Value.Item2 - _player.CamObject.forward * _hitOffsetDist;
				finalHitPoint = finalTarget.InverseTransformPoint(finalHitPoint);
			}
		}
	}

	private void CheckForPlayersAndLevel(out Transform finalTarget, out Vector3 finalHitPoint)
	{
		RaycastHit[] obj = Physics.RaycastAll(_player.CamObject.position, _player.CamObject.forward, layerMask: (LayerMask)((int)GameInfo.LevelLayer | (int)GameInfo.PlayerLayers | (int)GameInfo.BoatLayer | (int)GameInfo.NpcLayer), maxDistance: _range);
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
			finalHitPoint = tuple.Item3 - _player.CamObject.forward * _hitOffsetDist;
			finalHitPoint = finalTarget.InverseTransformPoint(finalHitPoint);
		}
	}

	private bool IsBehindWall(RaycastHit hit, Vector3 castOrigin)
	{
		RaycastHit[] array = Physics.RaycastAll(castOrigin, hit.point - castOrigin, _range, GameInfo.LevelLayer);
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

	private bool CanPunch()
	{
		for (int i = 0; i < 2; i++)
		{
			if (_isAttacking[i])
			{
				return false;
			}
		}
		if (_isReturning[0])
		{
			return !_isReturning[1];
		}
		return true;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerPunchingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerPunchingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(0u, RpcReader___ObserverStartPunching___3363832400);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerPunchingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerPunchingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverStartPunching___3363832400(Transform target, bool right, Vector3 targetHitPoint)
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
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: true);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverStartPunching___3363832400(Transform P_0, bool P_1, Vector3 P_2)
	{
		StartPunching(P_0, (!P_1) ? 1 : 0, calledFromLocal: false, P_2);
	}

	private void RpcReader___ObserverStartPunching___3363832400(PooledReader PooledReader0, Channel channel)
	{
		Transform transform = PooledReader0.ReadTransform();
		bool flag = PooledReader0.ReadBoolean();
		Vector3 vector = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverStartPunching___3363832400(transform, flag, vector);
		}
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}
}
