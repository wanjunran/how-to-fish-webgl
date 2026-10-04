using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerHands : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private float _handDrawSpeed;

	[SerializeField]
	private float _handDropSpeed;

	[SerializeField]
	private AnimationCurve _handDrawSpeedCurve;

	[SerializeField]
	[Tooltip("Is reversed, starts at 1")]
	private AnimationCurve _handDropSpeedCurve;

	[SerializeField]
	[Tooltip("Amount of time that the hands should stay with an item when it is being thrown")]
	private float _stayWithItemWhenThrowTime;

	[SerializeField]
	private Renderer _handModelRight;

	[SerializeField]
	private Transform _handBoneRight;

	[SerializeField]
	private Renderer _handModelLeft;

	[SerializeField]
	private Transform _handBoneLeft;

	[FormerlySerializedAs("_highDefaultHandAngleCap")]
	[Header("Default hand pos following cam / head")]
	[SerializeField]
	private float _defaultHandAngle;

	[SerializeField]
	private float _followHeadSpeed;

	[FormerlySerializedAs("_followHeadDamper")]
	[SerializeField]
	private float _followHeadDamping;

	[SerializeField]
	private float _tpHandBobbingAmount;

	[SerializeField]
	private float _tpHandBobbingSpeed;

	private Transform[] _fingerTransformsRight;

	private Transform[] _fingerTransformsLeft;

	private HandTransforms _handTransformsOrigRight = new HandTransforms(exists: false);

	private HandTransforms _handTransformsOrigLeft = new HandTransforms(exists: false);

	private HandTransforms _lastHandTransformsRight = new HandTransforms(exists: false);

	private HandTransforms _lastHandTransformsLeft = new HandTransforms(exists: false);

	private HandTransforms _preparedPurchaseHandTransformsRight = new HandTransforms(exists: false);

	private HandTransforms _preparedPurchaseHandTransformsLeft = new HandTransforms(exists: false);

	private Vector3 _localHandSway;

	private Quaternion _curHeadAngle = Quaternion.identity;

	private Vector3 _curHeadAngVel;

	private float _curHandHoldAmountRight;

	private float _curHandHoldAmountCurvedRight;

	private float _curHandHoldAmountLeft;

	private float _curHandHoldAmountCurvedLeft;

	private float _curTpHandBob;

	private float _throwTimer;

	private float _preparedPurchaseTime;

	private Item _thrownItem;

	private Boat _lastDrivenBoat;

	private byte _preparedPurchaseItemID = byte.MaxValue;

	public Transform HandBoneRight => _handBoneRight;

	public Transform HandBoneLeft => _handBoneLeft;

	private Item _heldItem => _player.Holding.HeldItem;

	private Boat _drivenBoat
	{
		get
		{
			if (!BoatManager.Boat || !(BoatManager.Boat.Driver == _player))
			{
				return null;
			}
			return BoatManager.Boat;
		}
	}

	private void Awake()
	{
		InitializeHand(right: true);
		InitializeHand(right: false);
		SetLastItemHandPosRotToCurrent(right: true, relativeToHead: true);
		SetLastItemHandPosRotToCurrent(right: false, relativeToHead: true);
	}

	private void LateUpdate()
	{
		CalculateHeadAngle();
		SetHandBob();
		HandleBoatHandTransitions();
		bool flag = false;
		bool flag2 = false;
		if ((bool)_drivenBoat || (bool)_heldItem)
		{
			if (CurrentHandTransforms(right: true).Exists)
			{
				MoveHandToItem(right: true);
				flag = true;
			}
			else if (_lastHandTransformsRight.Exists && _curHandHoldAmountRight > 0f)
			{
				MoveHandFromItem(right: true);
				flag = true;
			}
			if (CurrentHandTransforms(right: false).Exists)
			{
				MoveHandToItem(right: false);
				flag2 = true;
			}
			else if (_lastHandTransformsLeft.Exists && _curHandHoldAmountLeft > 0f)
			{
				MoveHandFromItem(right: false);
				flag2 = true;
			}
		}
		else if ((bool)_thrownItem)
		{
			if (_thrownItem.HandTransformsRight.Exists)
			{
				MoveHandToThrownItem(right: true);
				flag = true;
			}
			if (_thrownItem.HandTransformsLeft.Exists)
			{
				MoveHandToThrownItem(right: false);
				flag2 = true;
			}
			_throwTimer -= Time.deltaTime;
			if (_throwTimer <= 0f)
			{
				SetLastItemHandPosRot(isThrown: true);
				_thrownItem = null;
			}
		}
		else
		{
			if (_curHandHoldAmountRight > 0f)
			{
				MoveHandFromItem(right: true);
				flag = true;
			}
			if (_curHandHoldAmountLeft > 0f)
			{
				flag2 = true;
				MoveHandFromItem(right: false);
			}
		}
		if (!flag)
		{
			MoveHandToDefault(right: true);
		}
		if (!flag2)
		{
			MoveHandToDefault(right: false);
		}
		SyncHiddenHandsToAnimatedTool();
	}

	private void SyncHiddenHandsToAnimatedTool()
	{
		Item heldItem = _heldItem;
		if ((bool)heldItem && (bool)heldItem.Tool && (bool)heldItem.Tool.HandsMesh && heldItem.Tool.HandsMesh.enabled)
		{
			if ((bool)heldItem.HandModelRight)
			{
				_handBoneRight.SetPositionAndRotation(heldItem.HandModelRight.position, heldItem.HandModelRight.rotation);
			}
			if ((bool)heldItem.HandModelLeft)
			{
				_handBoneLeft.SetPositionAndRotation(heldItem.HandModelLeft.position, heldItem.HandModelLeft.rotation);
			}
		}
	}

	private void HandleBoatHandTransitions()
	{
		if ((bool)_drivenBoat && _drivenBoat != _lastDrivenBoat)
		{
			if (_drivenBoat.HandTransformsRight.Exists)
			{
				_curHandHoldAmountRight = 0f;
				_curHandHoldAmountCurvedRight = 0f;
				SetLastItemHandPosRotToCurrent(right: true, relativeToHead: true);
			}
			if (_drivenBoat.HandTransformsLeft.Exists)
			{
				_curHandHoldAmountLeft = 0f;
				_curHandHoldAmountCurvedLeft = 0f;
				SetLastItemHandPosRotToCurrent(right: false, relativeToHead: true);
			}
		}
		else if (!_drivenBoat && (bool)_lastDrivenBoat)
		{
			if (_lastDrivenBoat.HandTransformsRight.Exists)
			{
				_curHandHoldAmountRight = 1f;
				_curHandHoldAmountCurvedRight = 1f;
				SetLastItemHandPosRotToCurrent(right: true, relativeToHead: false);
			}
			if (_lastDrivenBoat.HandTransformsLeft.Exists)
			{
				_curHandHoldAmountLeft = 1f;
				_curHandHoldAmountCurvedLeft = 1f;
				SetLastItemHandPosRotToCurrent(right: false, relativeToHead: false);
			}
		}
		_lastDrivenBoat = _drivenBoat;
	}

	public void PrepareForPurchasedItem(byte itemID)
	{
		_preparedPurchaseHandTransformsRight = CaptureCurrentHandPosRot(right: true, relativeToHead: true);
		_preparedPurchaseHandTransformsLeft = CaptureCurrentHandPosRot(right: false, relativeToHead: true);
		_preparedPurchaseItemID = itemID;
		_preparedPurchaseTime = Time.unscaledTime;
	}

	public void PickUpItem()
	{
		if (!_heldItem)
		{
			return;
		}
		bool flag = _preparedPurchaseItemID == _heldItem.ID && Time.unscaledTime - _preparedPurchaseTime <= 10f;
		_preparedPurchaseItemID = byte.MaxValue;
		if (flag)
		{
			_lastHandTransformsRight = _preparedPurchaseHandTransformsRight;
			_lastHandTransformsLeft = _preparedPurchaseHandTransformsLeft;
		}
		if (_heldItem.HandTransformsRight.Exists)
		{
			if (!flag || !HasCompleteFingerPose(_preparedPurchaseHandTransformsRight, _fingerTransformsRight))
			{
				SetLastItemHandPosRotToCurrent(right: true, relativeToHead: true);
			}
			_curHandHoldAmountRight = 0f;
			_curHandHoldAmountCurvedRight = 0f;
		}
		if (_heldItem.HandTransformsLeft.Exists)
		{
			if (!flag || !HasCompleteFingerPose(_preparedPurchaseHandTransformsLeft, _fingerTransformsLeft))
			{
				SetLastItemHandPosRotToCurrent(right: false, relativeToHead: true);
			}
			_curHandHoldAmountLeft = 0f;
			_curHandHoldAmountCurvedLeft = 0f;
		}
		ToggleToolHands(enableTool: false);
	}

	public void DropItem(bool putInInv, Item droppedItem = null)
	{
		Item item = (droppedItem ? droppedItem : _heldItem);
		if (!item || ((bool)_heldItem && _heldItem != item))
		{
			return;
		}
		ToggleToolHands(enableTool: false);
		if (item.HandTransformsRight.Exists)
		{
			if (putInInv)
			{
				_curHandHoldAmountRight = 0.1f;
				_curHandHoldAmountCurvedRight = 0.1f;
			}
			else
			{
				_curHandHoldAmountRight = 1f;
				_curHandHoldAmountCurvedRight = 1f;
				MoveHandToItem(right: true, forced: true, item);
				SetLastItemHandPosRotToCurrent(right: true, relativeToHead: false);
			}
		}
		if (item.HandTransformsLeft.Exists)
		{
			if (putInInv)
			{
				_curHandHoldAmountLeft = 0.1f;
				_curHandHoldAmountCurvedLeft = 0.1f;
				return;
			}
			_curHandHoldAmountLeft = 1f;
			_curHandHoldAmountCurvedLeft = 1f;
			MoveHandToItem(right: false, forced: true, item);
			SetLastItemHandPosRotToCurrent(right: false, relativeToHead: false);
		}
	}

	public void SetThrowTimer(float throwPercent)
	{
		if (!(throwPercent <= 0f))
		{
			_thrownItem = _heldItem;
			_throwTimer = _stayWithItemWhenThrowTime * throwPercent;
		}
	}

	private void SetLastItemHandPosRot(bool isThrown)
	{
		Item item = (isThrown ? _thrownItem : _heldItem);
		if (item.HandTransformsRight.Exists)
		{
			HandTransforms lastHandTransformsRight = new HandTransforms(exists: true, _player.Transform.InverseTransformPoint(item.HandTransformsRight.Parent.TransformPoint(item.HandTransformsRight.HandPos)), item.HandTransformsRight.Parent.rotation * item.HandTransformsRight.HandRot, item.HandTransformsRight.FingerRots, null);
			_lastHandTransformsRight = lastHandTransformsRight;
		}
		if (item.HandTransformsLeft.Exists)
		{
			HandTransforms lastHandTransformsLeft = new HandTransforms(exists: true, _player.Transform.InverseTransformPoint(item.HandTransformsLeft.Parent.TransformPoint(item.HandTransformsLeft.HandPos)), item.HandTransformsLeft.Parent.rotation * item.HandTransformsLeft.HandRot, item.HandTransformsLeft.FingerRots, null);
			_lastHandTransformsLeft = lastHandTransformsLeft;
		}
	}

	private void SetLastItemHandPosRotToCurrent(bool right, bool relativeToHead)
	{
		HandTransforms handTransforms = CaptureCurrentHandPosRot(right, relativeToHead);
		if (right)
		{
			_lastHandTransformsRight = handTransforms;
		}
		else
		{
			_lastHandTransformsLeft = handTransforms;
		}
	}

	private HandTransforms CaptureCurrentHandPosRot(bool right, bool relativeToHead)
	{
		Transform transform = (relativeToHead ? _player.CamObject : _player.Transform);
		Transform transform2 = (right ? _handBoneRight : _handBoneLeft);
		Transform[] array = (right ? _fingerTransformsRight : _fingerTransformsLeft);
		if ((bool)_heldItem && (bool)_heldItem.Tool && (bool)_heldItem.Tool.HandsMesh && _heldItem.Tool.HandsMesh.enabled)
		{
			Transform transform3 = (right ? _heldItem.HandModelRight : _heldItem.HandModelLeft);
			if ((bool)transform3)
			{
				transform2 = transform3;
			}
		}
		Quaternion[] array2 = new Quaternion[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = array[i].localRotation;
		}
		return new HandTransforms(exists: true, transform.InverseTransformPoint(transform2.position), Quaternion.Inverse(transform.rotation) * transform2.rotation, array2, null);
	}

	public void SpawnItemFromInventory()
	{
		if ((bool)_heldItem)
		{
			if (_heldItem.HandTransformsRight.Exists)
			{
				_curHandHoldAmountRight = 0.99f;
			}
			if (_heldItem.HandTransformsLeft.Exists)
			{
				_curHandHoldAmountLeft = 0.99f;
			}
			ToggleToolHands(_heldItem.Tool);
		}
	}

	private void MoveHandToItem(bool right, bool forced = false, Item itemOverride = null)
	{
		Item item = (itemOverride ? itemOverride : _heldItem);
		if ((bool)item && (bool)item.Tool && !forced)
		{
			if (item.Tool.HandsMesh.enabled)
			{
				if (_handModelRight.enabled)
				{
					ToggleToolHands(enableTool: true);
				}
				return;
			}
			bool num = !item.HandTransformsRight.Exists || _curHandHoldAmountRight >= 1f;
			bool flag = !item.HandTransformsLeft.Exists || _curHandHoldAmountLeft >= 1f;
			if (num & flag)
			{
				ToggleToolHands(enableTool: true);
				return;
			}
		}
		if (right)
		{
			HandTransforms handTransforms = (itemOverride ? itemOverride.HandTransformsRight : CurrentHandTransforms(right: true));
			Vector3 zero = Vector3.zero;
			Quaternion identity = Quaternion.identity;
			if (!HasCompleteFingerPose(_lastHandTransformsRight, _fingerTransformsRight))
			{
				SetLastItemHandPosRotToCurrent(right: true, relativeToHead: true);
			}
			zero = handTransforms.Parent.TransformPoint(handTransforms.HandPos);
			identity = handTransforms.Parent.rotation * handTransforms.HandRot;
			if ((_curHandHoldAmountRight < 1f) | forced)
			{
				_curHandHoldAmountRight += _handDrawSpeed * Time.deltaTime;
				_curHandHoldAmountCurvedRight = _handDrawSpeedCurve.Evaluate(_curHandHoldAmountRight);
				_handBoneRight.position = Vector3.Lerp(_player.CamObject.TransformPoint(_lastHandTransformsRight.HandPos), zero, _curHandHoldAmountCurvedRight);
				_handBoneRight.rotation = Quaternion.Slerp(_player.CamObject.rotation * _lastHandTransformsRight.HandRot, identity, _curHandHoldAmountCurvedRight);
				for (int i = 0; i < _fingerTransformsRight.Length; i++)
				{
					_fingerTransformsRight[i].localRotation = Quaternion.Slerp(_lastHandTransformsRight.FingerRots[i], handTransforms.FingerRots[i], _curHandHoldAmountCurvedRight);
				}
			}
			else
			{
				_handBoneRight.position = zero;
				_handBoneRight.rotation = identity;
			}
			return;
		}
		HandTransforms handTransforms2 = (itemOverride ? itemOverride.HandTransformsLeft : CurrentHandTransforms(right: false));
		Vector3 zero2 = Vector3.zero;
		Quaternion identity2 = Quaternion.identity;
		if (!HasCompleteFingerPose(_lastHandTransformsLeft, _fingerTransformsLeft))
		{
			SetLastItemHandPosRotToCurrent(right: false, relativeToHead: true);
		}
		zero2 = handTransforms2.Parent.TransformPoint(handTransforms2.HandPos);
		identity2 = handTransforms2.Parent.rotation * handTransforms2.HandRot;
		if ((_curHandHoldAmountLeft < 1f) | forced)
		{
			_curHandHoldAmountLeft += _handDrawSpeed * Time.deltaTime;
			_curHandHoldAmountCurvedLeft = _handDrawSpeedCurve.Evaluate(_curHandHoldAmountLeft);
			_handBoneLeft.position = Vector3.Lerp(_player.CamObject.TransformPoint(_lastHandTransformsLeft.HandPos), zero2, _curHandHoldAmountCurvedLeft);
			_handBoneLeft.rotation = Quaternion.Slerp(_player.CamObject.rotation * _lastHandTransformsLeft.HandRot, identity2, _curHandHoldAmountCurvedLeft);
			for (int j = 0; j < _fingerTransformsLeft.Length; j++)
			{
				_fingerTransformsLeft[j].localRotation = Quaternion.Slerp(_lastHandTransformsLeft.FingerRots[j], handTransforms2.FingerRots[j], _curHandHoldAmountCurvedLeft);
			}
		}
		else
		{
			_handBoneLeft.position = zero2;
			_handBoneLeft.rotation = identity2;
		}
	}

	private void MoveHandToThrownItem(bool right)
	{
		if (right)
		{
			Vector3 zero = Vector3.zero;
			Quaternion identity = Quaternion.identity;
			zero = _thrownItem.HandTransformsRight.Parent.TransformPoint(_thrownItem.HandTransformsRight.HandPos);
			identity = _thrownItem.HandTransformsRight.Parent.rotation * _thrownItem.HandTransformsRight.HandRot;
			_handBoneRight.position = Vector3.Lerp(CurDefaultHandPos(right: true), zero, _curHandHoldAmountCurvedRight);
			_handBoneRight.rotation = Quaternion.Slerp(CurDefaultHandRot(right: true), identity, _curHandHoldAmountCurvedRight);
			for (int i = 0; i < _fingerTransformsRight.Length; i++)
			{
				_fingerTransformsRight[i].localRotation = Quaternion.Slerp(_handTransformsOrigRight.FingerRots[i], _thrownItem.HandTransformsRight.FingerRots[i], _curHandHoldAmountCurvedRight);
			}
		}
		else
		{
			Vector3 zero2 = Vector3.zero;
			Quaternion identity2 = Quaternion.identity;
			zero2 = _thrownItem.HandTransformsLeft.Parent.TransformPoint(_thrownItem.HandTransformsLeft.HandPos);
			identity2 = _thrownItem.HandTransformsLeft.Parent.rotation * _thrownItem.HandTransformsLeft.HandRot;
			_handBoneLeft.position = Vector3.Lerp(CurDefaultHandPos(right: false), zero2, _curHandHoldAmountCurvedLeft);
			_handBoneLeft.rotation = Quaternion.Slerp(CurDefaultHandRot(right: false), identity2, _curHandHoldAmountCurvedLeft);
			for (int j = 0; j < _fingerTransformsLeft.Length; j++)
			{
				_fingerTransformsLeft[j].localRotation = Quaternion.Slerp(_handTransformsOrigLeft.FingerRots[j], _thrownItem.HandTransformsLeft.FingerRots[j], _curHandHoldAmountCurvedLeft);
			}
		}
	}

	private HandTransforms CurrentHandTransforms(bool right)
	{
		if ((bool)_drivenBoat)
		{
			if (!right)
			{
				return _drivenBoat.HandTransformsLeft;
			}
			return _drivenBoat.HandTransformsRight;
		}
		if ((bool)_heldItem)
		{
			if (!right)
			{
				return _heldItem.HandTransformsLeft;
			}
			return _heldItem.HandTransformsRight;
		}
		return new HandTransforms(exists: false);
	}

	private void MoveHandFromItem(bool right)
	{
		if (right)
		{
			if (_curHandHoldAmountRight > 0f)
			{
				_curHandHoldAmountRight -= _handDropSpeed * Time.deltaTime;
				_curHandHoldAmountCurvedRight = _handDropSpeedCurve.Evaluate(_curHandHoldAmountRight);
			}
			if (!_player.Punching.IsAnimatingRight)
			{
				_handBoneRight.position = Vector3.LerpUnclamped(CurDefaultHandPos(right: true), _player.Transform.TransformPoint(_lastHandTransformsRight.HandPos), _curHandHoldAmountCurvedRight);
			}
			_handBoneRight.rotation = Quaternion.SlerpUnclamped(CurDefaultHandRot(right: true), _player.Transform.rotation * _lastHandTransformsRight.HandRot, _curHandHoldAmountCurvedRight);
			for (int i = 0; i < _fingerTransformsRight.Length; i++)
			{
				_fingerTransformsRight[i].localRotation = Quaternion.Slerp(_handTransformsOrigRight.FingerRots[i], _lastHandTransformsRight.FingerRots[i], _curHandHoldAmountCurvedRight);
			}
		}
		else
		{
			if (_curHandHoldAmountLeft > 0f)
			{
				_curHandHoldAmountLeft -= _handDropSpeed * Time.deltaTime;
				_curHandHoldAmountCurvedLeft = _handDropSpeedCurve.Evaluate(_curHandHoldAmountLeft);
			}
			if (!_player.Punching.IsAnimatingLeft)
			{
				_handBoneLeft.position = Vector3.LerpUnclamped(CurDefaultHandPos(right: false), _player.Transform.TransformPoint(_lastHandTransformsLeft.HandPos), _curHandHoldAmountCurvedLeft);
			}
			_handBoneLeft.rotation = Quaternion.SlerpUnclamped(CurDefaultHandRot(right: false), _player.Transform.rotation * _lastHandTransformsLeft.HandRot, _curHandHoldAmountCurvedLeft);
			for (int j = 0; j < _fingerTransformsLeft.Length; j++)
			{
				_fingerTransformsLeft[j].localRotation = Quaternion.Slerp(_handTransformsOrigLeft.FingerRots[j], _lastHandTransformsLeft.FingerRots[j], _curHandHoldAmountCurvedLeft);
			}
		}
	}

	private void MoveHandToDefault(bool right)
	{
		if (right)
		{
			if (!_player.Punching.IsAnimatingRight)
			{
				_handBoneRight.position = CurDefaultHandPos(right: true);
				_handBoneRight.rotation = CurDefaultHandRot(right: true);
			}
		}
		else if (!_player.Punching.IsAnimatingLeft)
		{
			_handBoneLeft.position = CurDefaultHandPos(right: false);
			_handBoneLeft.rotation = CurDefaultHandRot(right: false);
		}
	}

	private void InitializeHand(bool right)
	{
		Transform transform = (right ? _handBoneRight : _handBoneLeft);
		List<Transform> list = new List<Transform>();
		List<Quaternion> list2 = new List<Quaternion>();
		for (int i = 0; i < transform.childCount; i++)
		{
			list.Add(transform.GetChild(i).transform);
			list2.Add(transform.GetChild(i).transform.localRotation);
			list.Add(transform.GetChild(i).GetChild(0).transform);
			list2.Add(transform.GetChild(i).GetChild(0).transform.localRotation);
			list.Add(transform.GetChild(i).GetChild(0).GetChild(0)
				.transform);
				list2.Add(transform.GetChild(i).GetChild(0).GetChild(0)
					.transform.localRotation);
			}
			HandTransforms handTransforms = new HandTransforms(exists: true, (right ? _handBoneRight.position : _handBoneLeft.position) - _player.CamObject.position, right ? _handBoneRight.rotation : _handBoneLeft.rotation, list2.ToArray(), null);
			if (right)
			{
				_fingerTransformsRight = list.ToArray();
				_handTransformsOrigRight = handTransforms;
			}
			else
			{
				_fingerTransformsLeft = list.ToArray();
				_handTransformsOrigLeft = handTransforms;
			}
		}

		private static bool HasCompleteFingerPose(HandTransforms handTransforms, Transform[] fingerTransforms)
		{
			if (handTransforms != null && handTransforms.FingerRots != null && fingerTransforms != null)
			{
				return handTransforms.FingerRots.Length >= fingerTransforms.Length;
			}
			return false;
		}

		private void ToggleToolHands(bool enableTool)
		{
			Item heldItem = _heldItem;
			if (enableTool && (bool)heldItem && (bool)heldItem.Tool && heldItem.Tool.TryActivateAnimatedHands(_player))
			{
				ToggleHandMeshes(enable: false);
				return;
			}
			if (!_player.Dying.IsDead)
			{
				ToggleHandMeshes(enable: true);
			}
			_player.Arms.SetIKTarget(_handBoneRight, _handBoneLeft);
		}

		public void ToggleHandMeshes(bool enable)
		{
			_handModelLeft.enabled = enable;
			_handModelRight.enabled = enable;
		}

		private void CalculateHeadAngle()
		{
			Quaternion targetRot = Quaternion.Euler(_defaultHandAngle, _player.CurPlayerRot.eulerAngles.y, 0f);
			DazedUtils.SimulateSpringRotation(ref _curHeadAngle, ref _curHeadAngVel, targetRot, _followHeadSpeed, _followHeadDamping);
		}

		private void SetHandBob()
		{
			_curTpHandBob = Mathf.Lerp(_curTpHandBob, _player.Legs.BodyBobDownPercent() * _tpHandBobbingAmount, _tpHandBobbingSpeed * Time.deltaTime);
		}

		public Vector3 CurDefaultHandPos(bool right)
		{
			Vector3 vector = _player.Transform.position + Vector3.up * _player.Camera.CamHeight;
			if (right)
			{
				vector += _curHeadAngle * _handTransformsOrigRight.HandPos;
			}
			else
			{
				vector += _curHeadAngle * _handTransformsOrigLeft.HandPos;
			}
			return vector + Vector3.down * _curTpHandBob;
		}

		public Quaternion CurDefaultHandRot(bool right)
		{
			Quaternion curHeadAngle = _curHeadAngle;
			if (right)
			{
				return curHeadAngle * _handTransformsOrigRight.HandRot;
			}
			return curHeadAngle * _handTransformsOrigLeft.HandRot;
		}
	}
