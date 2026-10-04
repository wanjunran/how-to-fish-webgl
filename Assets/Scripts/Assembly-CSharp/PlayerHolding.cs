using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerHolding : NetworkBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private float _grabRange;

	[SerializeField]
	private float _grabRadius;

	[SerializeField]
	private Vector3 _jointTargetTargetOffset;

	[SerializeField]
	private float _itemMoveToHoldPointSpeed;

	[SerializeField]
	[Range(0f, 1f)]
	private float _playerVelAmount;

	[SerializeField]
	private float _playerVelSmoothing;

	[SerializeField]
	private float _bobSprintMulti;

	[SerializeField]
	private float _bobSprintMultiSpeed;

	[SerializeField]
	private float _maxItemMoveSpeedWhenColliding;

	[SerializeField]
	private float _itemRotateToHoldRotSpeed;

	[SerializeField]
	private float _maxItemRotSpeedWhenColliding;

	[FormerlySerializedAs("_maxItemDistToForceDrop")]
	[SerializeField]
	private float _maxItemDistToUnstuck;

	[FormerlySerializedAs("_timeUntilAllowForceDrop")]
	[SerializeField]
	[Tooltip("Amount of time after picking something up where dropping because of distance is disabled. Prevents accedeltaly dropping things when picking up and flicking")]
	private float _timeUntilAllowUnstuck;

	[SerializeField]
	private float _pickUpSpeed;

	[SerializeField]
	private float _maxDropForce;

	[SerializeField]
	private float _timeUntilMaxDropForce;

	[SerializeField]
	private float _dropForcePosOffset;

	[SerializeField]
	private AnimationCurve _dropForcePosOffsetSpeedCurve;

	[SerializeField]
	private float _startDropForceDelay;

	[SerializeField]
	[Tooltip("Relative to player pos")]
	private Vector3 _itemDrawStartPos;

	[SerializeField]
	private float _itemDrawSpeed;

	private Item _grabbableItem;

	private Interactable _interactable;

	private Item _heldItem;

	private Vector3 _curDrawPos;

	private Quaternion _curDrawRot;

	private Vector3 _curDropForcePosOffset;

	private Vector3 _curJointTargetTargetPos;

	private Vector3 _playerVel;

	private Vector3 _playerVelSmoothDampVelocity;

	private Vector3 _bobOffset;

	private bool _isHoldingDrop;

	private bool _startedDropping;

	private float _curDropForce;

	private float _curHoldPercent;

	private float _curSprintPercent;

	private float _maxItemMoveSpeedWhenCollidingSqr;

	private float _maxItemRotSpeedWhenCollidingSqr;

	private float _maxItemDistToUnstuckSqr;

	private float _timeOfPickUp;

	private bool NetworkInitialize___EarlyPlayerHoldingAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerHoldingAssembly_002DCSharp_002Edll_Excuted;

	public Item UninitializedHeldItem { get; private set; }

	public Item HeldItem => _heldItem;

	public Vector3 ItemDrawStartPos => _itemDrawStartPos;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_PlayerHolding_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			BindInputs();
		}
	}

	public override void OnStopClient()
	{
		if (base.Owner.IsLocalClient)
		{
			UnbindInputs();
		}
	}

	public void SetUninitializedHeldItem(Item item)
	{
		UninitializedHeldItem = item;
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["PlayerPickUp"].performed += PickUpInput;
		input.actions["PlayerDrop"].performed += DropInput;
		input.actions["PlayerDrop"].canceled += DropInputCanceled;
		input.actions["PlayerLeftClick"].performed += PrimaryInput;
		input.actions["PlayerLeftClick"].canceled += PrimaryInputCanceled;
		input.actions["PlayerRightClick"].performed += SecondaryInput;
		input.actions["PlayerRightClick"].canceled += SecondaryInputCanceled;
		input.actions["WeaponReload"].performed += ReloadInput;
		input.actions["WeaponInspect"].performed += InspectInput;
		input.actions["PlayerChangeSkin"].performed += ChangeSkinInput;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["PlayerPickUp"].performed -= PickUpInput;
			input.actions["PlayerDrop"].performed -= DropInput;
			input.actions["PlayerDrop"].canceled -= DropInputCanceled;
			input.actions["PlayerLeftClick"].performed -= PrimaryInput;
			input.actions["PlayerLeftClick"].canceled -= PrimaryInputCanceled;
			input.actions["PlayerRightClick"].performed -= SecondaryInput;
			input.actions["PlayerRightClick"].canceled -= SecondaryInputCanceled;
			input.actions["WeaponReload"].performed -= ReloadInput;
			input.actions["WeaponInspect"].performed -= InspectInput;
			input.actions["PlayerChangeSkin"].performed -= ChangeSkinInput;
		}
	}

	private void PrimaryInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && (bool)_heldItem)
		{
			_heldItem.PrimaryInput(context);
		}
	}

	private void PrimaryInputCanceled(InputAction.CallbackContext context)
	{
		if ((bool)_heldItem)
		{
			_heldItem.PrimaryInputCancel(context);
		}
	}

	private void SecondaryInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && (bool)_heldItem)
		{
			_heldItem.SecondaryInput(context);
		}
	}

	private void SecondaryInputCanceled(InputAction.CallbackContext context)
	{
		if ((bool)_heldItem)
		{
			_heldItem.SecondaryInputCanceled(context);
		}
	}

	private void ReloadInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && (bool)_heldItem)
		{
			_heldItem.ReloadInput(context);
		}
	}

	private void InspectInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && (bool)_heldItem)
		{
			_heldItem.InspectInput(context);
		}
	}

	private void ChangeSkinInput(InputAction.CallbackContext context)
	{
		if ((bool)_heldItem && base.Owner.IsLocalClient && !_player.BlockInputs)
		{
			float num = context.ReadValue<float>();
			byte skin = SaveManager.GetSkin(_heldItem.ID, _heldItem.CurSkin, num > 0f);
			Server.Instance.SetItemSkin(_heldItem, skin);
		}
	}

	private void PickUpInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs)
		{
			if ((bool)_grabbableItem && !_grabbableItem.IsDestroying && !_grabbableItem.IsDeinitializing)
			{
				_grabbableItem.PickUp(_player, calledFromLocal: true, sendToServer: true);
			}
			else if ((bool)_interactable && (!_heldItem || _interactable.InteractableWhenHoldingItem))
			{
				_interactable.Interact(_player);
			}
		}
	}

	private void DropInput(InputAction.CallbackContext context)
	{
		if (!PauseManager.IsPaused && (bool)_heldItem)
		{
			_curDropForce = 0f;
			_curDropForcePosOffset = Vector3.zero;
			_isHoldingDrop = true;
		}
	}

	private void DropInputCanceled(InputAction.CallbackContext context)
	{
		if ((bool)_heldItem && _isHoldingDrop)
		{
			if ((bool)_heldItem.FishingRod && (bool)_heldItem.FishingRod.Bait.ItemOnBait)
			{
				_heldItem.FishingRod.ReleaseItem(_heldItem.FishingRod.Bait.ItemOnBait);
				ResetDropForce();
			}
			else
			{
				_heldItem.Drop(calledFromLocal: true, FinalDropForce(), _heldItem.Tool ? _player.ToolMovement.FakeItemAngVel : Vector3.zero);
			}
		}
	}

	private void FixedUpdate()
	{
		GetPlayerVel();
		MoveJointTargetTarget(usePhysicsPosition: true);
		if ((bool)_heldItem)
		{
			if ((bool)_heldItem.Tool)
			{
				if (!_heldItem.Rig.isKinematic)
				{
					_heldItem.Rig.linearVelocity = Vector3.zero;
					_heldItem.Rig.angularVelocity = Vector3.zero;
				}
			}
			else
			{
				MoveItemToHoldPosRot();
			}
		}
		ScanForGrabbableItem();
		ScanForInteractable();
		if (!_grabbableItem && !_interactable)
		{
			PlayerUI.ForceHideLookAtText();
		}
	}

	private void Update()
	{
		ReconcileHeldTool();
		if (_isHoldingDrop && (bool)_heldItem)
		{
			IncreaseDropForce();
		}
	}

	private void LateUpdate()
	{
		if ((bool)_heldItem)
		{
			GetBobOffset();
		}
	}

	private void ScanForGrabbableItem()
	{
		if (_player.BlockInputs || Boat.IsDrivingLocally || Boat.WantToDrive)
		{
			if ((bool)_grabbableItem)
			{
				_grabbableItem.UnHover();
			}
			_grabbableItem = null;
			return;
		}
		Collider[] array = Physics.OverlapCapsule(_player.Camera.CamTransform.position, _player.Camera.CamTransform.position + _player.Camera.CamTransform.forward * _grabRange, _grabRadius, GameInfo.ItemLayer);
		List<Item> list = new List<Item>();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.CompareTag("Item"))
			{
				Item item = ItemManager.Get(collider);
				if ((bool)item && item.IsInteractable && item.CanPickUp && !item.SyncedHolder && (!Physics.Raycast(_player.CamObject.position, item.Rig.worldCenterOfMass - _player.CamObject.position, out var hitInfo, _grabRange, (int)GameInfo.LevelLayer | (int)GameInfo.ItemLayer) || !hitInfo.transform.CompareTag("Level")))
				{
					list.Add(item);
				}
			}
		}
		Item item2 = null;
		float num = -1f;
		foreach (Item item3 in list)
		{
			float num2 = Vector3.Dot(item3.transform.position - _player.Camera.CamTransform.position, _player.Camera.CamTransform.forward);
			if (!(num2 < num))
			{
				num = num2;
				item2 = item3;
			}
		}
		if (_grabbableItem != item2 && (bool)_grabbableItem)
		{
			_grabbableItem.UnHover();
		}
		if ((bool)item2)
		{
			item2.Hover();
		}
		_grabbableItem = item2;
	}

	private void ScanForInteractable()
	{
		if ((bool)_grabbableItem || _player.BlockInputs || Boat.IsDrivingLocally || Boat.WantToDrive)
		{
			if ((bool)_interactable)
			{
				_interactable.UnHover();
			}
			_interactable = null;
			return;
		}
		Collider[] array = Physics.OverlapCapsule(_player.Camera.CamTransform.position, _player.Camera.CamTransform.position + _player.Camera.CamTransform.forward * _grabRange, _grabRadius, GameInfo.InteractableLayer);
		List<Interactable> list = new List<Interactable>();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.CompareTag("Interactable"))
			{
				Interactable interactable = Interactable.Get(collider);
				if ((bool)interactable && (!Physics.Raycast(_player.CamObject.position, interactable.transform.position - _player.CamObject.position, out var hitInfo, _grabRange, (int)GameInfo.LevelLayer | (int)GameInfo.InteractableLayer) || !hitInfo.transform.CompareTag("Level")) && (bool)interactable)
				{
					list.Add(interactable);
				}
			}
		}
		Interactable interactable2 = null;
		float num = -1f;
		foreach (Interactable item in list)
		{
			float num2 = Vector3.Dot((item.transform.position - _player.Camera.CamTransform.position).normalized, _player.Camera.CamTransform.forward);
			if (!(num2 < num))
			{
				num = num2;
				interactable2 = item;
			}
		}
		if (_interactable != interactable2 && (bool)_interactable)
		{
			_interactable.UnHover();
		}
		if ((bool)interactable2)
		{
			interactable2.Hover();
		}
		_interactable = interactable2;
	}

	public void PickUpItem(Item item, bool calledFromLocal)
	{
		if ((bool)_heldItem && _heldItem != item)
		{
			_player.Inventory.ResolveHeldItemReplacement(_heldItem, calledFromLocal);
		}
		SetHeldItem(item);
		item.OnPickUp();
		_player.Hands.PickUpItem();
		if (calledFromLocal)
		{
			if ((bool)_grabbableItem)
			{
				_grabbableItem.UnHover();
			}
			_timeOfPickUp = Time.time;
			_curHoldPercent = 0f;
			ResetDropForce();
		}
	}

	public void DropItem(bool calledFromLocal, Item droppedItem = null)
	{
		Item item = (droppedItem ? droppedItem : _heldItem);
		if ((bool)item && !(_heldItem != item))
		{
			if (calledFromLocal)
			{
				_player.Inventory.ApplySlot(-1);
				Server.Instance.RemoveItemFromInventory(_player, item);
			}
			SetHeldItem(null);
			ResetDropForce();
		}
	}

	public void RestoreHeldItemIfMissing(Item item)
	{
		if (!_heldItem && (bool)item && !item.IsInInventory && !(item.Holder != _player) && !(item.SyncedHolder != _player))
		{
			SetHeldItem(item);
		}
	}

	public void SetHeldItem(Item item)
	{
		_heldItem = item;
		ReconcileHeldTool();
		if (UninitializedHeldItem == item)
		{
			UninitializedHeldItem = null;
		}
		_timeOfPickUp = Time.time;
	}

	private void ReconcileHeldTool()
	{
		Tool heldTool = (((bool)_heldItem && !_heldItem.IsInInventory && _heldItem.Holder == _player) ? _heldItem.Tool : null);
		_player.ToolMovement.ReconcileHeldTool(heldTool);
	}

	private void MoveItemToHoldPosRot()
	{
		UpdateHoldPercent();
		if (ShouldUnstuckItem())
		{
			UnstuckItem();
			return;
		}
		_heldItem.transform.position -= _bobOffset;
		Vector3 targetPos = CalculateHoldTargetPos();
		Vector3 vel = CalculateLinearVelocity(targetPos);
		vel = CapLinearVelocityWhenColliding(vel);
		Quaternion targetRot = CalculateHoldTargetRot();
		Vector3 angVel = CalculateAngularVelocity(targetRot);
		angVel = CapAngularVelocityWhenColliding(angVel);
		if (!_heldItem.Rig.isKinematic)
		{
			_heldItem.Rig.linearVelocity = vel;
			_heldItem.Rig.angularVelocity = angVel;
			_heldItem.transform.position += _bobOffset;
		}
	}

	private void GetBobOffset()
	{
		float b = (_player.Movement.Sprinting ? _bobSprintMulti : 1f);
		_curSprintPercent = Mathf.Lerp(_curSprintPercent, b, _bobSprintMultiSpeed * Time.deltaTime);
		_bobOffset = _player.CamObject.rotation * _player.Camera.BobPos * (_heldItem.HoldBobMulti * _curSprintPercent);
	}

	private void UpdateHoldPercent()
	{
		if (_curHoldPercent < 1f)
		{
			_curHoldPercent += _pickUpSpeed * Time.fixedDeltaTime;
		}
		else
		{
			_curHoldPercent = 1f;
		}
	}

	private void UnstuckItem()
	{
		_heldItem.Rig.linearVelocity = Vector3.zero;
		_heldItem.Rig.angularVelocity = Vector3.zero;
		_heldItem.Rig.position = CalculateHoldTargetPos();
	}

	private Vector3 CalculateHoldTargetPos()
	{
		Quaternion rotation = _player.CamObject.rotation;
		return _curJointTargetTargetPos + rotation * GetHeldPosOffset() + rotation * _curDropForcePosOffset;
	}

	private Vector3 GetHeldPosOffset()
	{
		if (!_heldItem.Creature)
		{
			return _heldItem.HeldPos;
		}
		return Vector3.Lerp(_heldItem.HeldPos, _heldItem.Creature.EatPos, EatingMovePercent());
	}

	private Vector3 CalculateLinearVelocity(Vector3 targetPos)
	{
		float holdMoveSpeed = GetHoldMoveSpeed();
		if ((bool)BoatManager.Boat && _player.Movement.OnBoat)
		{
			Transform visualBoat = BoatManager.Boat.VisualBoat;
			Vector3 vector = visualBoat.InverseTransformPoint(targetPos);
			Vector3 vector2 = visualBoat.InverseTransformPoint(_heldItem.transform.position);
			Vector3 direction = (vector - vector2) * (holdMoveSpeed * _curHoldPercent);
			direction += visualBoat.InverseTransformDirection(_playerVel) * GetPlayerVelocityAmount();
			return visualBoat.TransformDirection(direction);
		}
		return (targetPos - _heldItem.Rig.position) * (holdMoveSpeed * _curHoldPercent) + _playerVel * GetPlayerVelocityAmount();
	}

	private float GetHoldMoveSpeed()
	{
		if (!_heldItem.Creature)
		{
			return _itemMoveToHoldPointSpeed;
		}
		return Mathf.Lerp(_itemMoveToHoldPointSpeed, _itemMoveToHoldPointSpeed * _player.Eating.HoldForceMulti, EatingMovePercent());
	}

	private float GetPlayerVelocityAmount()
	{
		float num = (_heldItem.OverridePlayerVelAmount ? _heldItem.PlayerVelAmount : _playerVelAmount);
		if ((bool)_heldItem.Creature)
		{
			num = Mathf.Lerp(num, 1f, EatingMovePercent());
		}
		return num;
	}

	private Vector3 CapLinearVelocityWhenColliding(Vector3 vel)
	{
		if (_heldItem.IsColliding && vel.sqrMagnitude >= _maxItemMoveSpeedWhenCollidingSqr)
		{
			return vel.normalized * _maxItemMoveSpeedWhenColliding;
		}
		return vel;
	}

	private bool ShouldUnstuckItem()
	{
		if (Time.time < _timeOfPickUp + _timeUntilAllowUnstuck)
		{
			return false;
		}
		if (!((_curJointTargetTargetPos - _heldItem.transform.position).sqrMagnitude > _maxItemDistToUnstuckSqr))
		{
			return false;
		}
		return true;
	}

	private Quaternion CalculateHoldTargetRot()
	{
		Vector3 heldRotOffset = GetHeldRotOffset();
		return _player.CamObject.rotation * Quaternion.Euler(heldRotOffset);
	}

	private Vector3 GetHeldRotOffset()
	{
		if (!_heldItem.Creature)
		{
			return _heldItem.HeldRot;
		}
		return Quaternion.Slerp(Quaternion.Euler(_heldItem.HeldRot), Quaternion.Euler(_heldItem.Creature.EatRot), EatingMovePercent()).eulerAngles;
	}

	private Vector3 CalculateAngularVelocity(Quaternion targetRot)
	{
		return DazedUtils.GetAngularVelocityToTarget(_heldItem.Rig.rotation, targetRot) * (GetHoldRotSpeed() * _curHoldPercent);
	}

	private float GetHoldRotSpeed()
	{
		if (!_heldItem.Creature)
		{
			return _itemRotateToHoldRotSpeed;
		}
		return Mathf.Lerp(_itemRotateToHoldRotSpeed, _itemRotateToHoldRotSpeed * _player.Eating.HoldForceMulti, EatingMovePercent());
	}

	private Vector3 CapAngularVelocityWhenColliding(Vector3 angVel)
	{
		if (_player.Eating.EatPercent < 0.5f && _heldItem.IsColliding && angVel.sqrMagnitude >= _maxItemRotSpeedWhenCollidingSqr)
		{
			return angVel.normalized * _maxItemRotSpeedWhenColliding;
		}
		return angVel;
	}

	private float EatingMovePercent()
	{
		return _player.Eating.MoveCurve.Evaluate(_player.Eating.EatPercent);
	}

	private void MoveJointTargetTarget(bool usePhysicsPosition = false)
	{
		Vector3 vector = (usePhysicsPosition ? (_player.Rigidbody.position + Vector3.up * _player.Camera.CamHeight) : _player.Camera.CamPosNoEffects);
		_curJointTargetTargetPos = vector + _player.CamObject.rotation * _jointTargetTargetOffset;
	}

	private void IncreaseDropForce()
	{
		if (_curDropForce < 1f)
		{
			_curDropForce += Time.deltaTime / _timeUntilMaxDropForce;
			if (_curDropForce >= _startDropForceDelay && !_startedDropping)
			{
				PlayerUI.StartDropUI(_timeUntilMaxDropForce - _startDropForceDelay);
				_startedDropping = true;
			}
		}
		else
		{
			_curDropForce = 1f;
		}
		_curDropForcePosOffset = Vector3.back * (_dropForcePosOffset * _dropForcePosOffsetSpeedCurve.Evaluate(_curDropForce));
	}

	private void ResetDropForce()
	{
		_isHoldingDrop = false;
		_curDropForce = 0f;
		_curDropForcePosOffset = Vector3.zero;
		_startedDropping = false;
		PlayerUI.StopDropUI();
	}

	private void GetPlayerVel()
	{
		_playerVel = Vector3.SmoothDamp(_playerVel, _player.Movement.Velocity, ref _playerVelSmoothDampVelocity, _playerVelSmoothing);
	}

	private Vector3 FinalDropForce()
	{
		if (_curDropForce < _startDropForceDelay)
		{
			_curDropForce = 0f;
		}
		float num = _curDropForce * _maxDropForce;
		Vector3 result = _player.Camera.CamTransform.forward * num;
		if ((bool)_heldItem.Tool)
		{
			result += _player.ToolMovement.FakeItemVel;
		}
		_player.Hands.SetThrowTimer(_curDropForce);
		return result;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerHoldingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerHoldingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerHoldingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerHoldingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void Awake_UserLogic_PlayerHolding_Assembly_002DCSharp_002Edll()
	{
		_maxItemMoveSpeedWhenCollidingSqr = _maxItemMoveSpeedWhenColliding * _maxItemMoveSpeedWhenColliding;
		_maxItemRotSpeedWhenCollidingSqr = _maxItemRotSpeedWhenColliding * _maxItemRotSpeedWhenColliding;
		_maxItemDistToUnstuckSqr = _maxItemDistToUnstuck * _maxItemDistToUnstuck;
		MoveJointTargetTarget();
	}
}
