using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Serialization;

public class Item : NetworkBehaviour
{
	[Header("Ignore this")]
	[SerializeField]
	[Tooltip("Set through Dazed Games/Assign item IDs in top menu (Serialized so it saves)")]
	private byte _id;

	[SerializeField]
	private LocalizedString _nameLocalized;

	[Header("Item Variables")]
	[Header("Bird catching")]
	[SerializeField]
	private bool _ignoredBySeagulls = true;

	[SerializeField]
	private bool _ignoredByMoneyNPC;

	[SerializeField]
	private bool _ignoredByCloseDots;

	[SerializeField]
	[Tooltip("Most items use main rig for birds, so usually doesn't need to be set. Only if you want a specific rig to be attached to the bird when caught")]
	private Rigidbody _otherRigForBird;

	[SerializeField]
	[Tooltip("Change this if rig should have an offset from bird feet")]
	private Vector3 _caughtOffset;

	[SerializeField]
	[Tooltip("All colliders on this item that collide with the world, will be disabled when item is in a spawner")]
	protected Collider[] _worldColliders;

	[FormerlySerializedAs("extraRigs")]
	[FormerlySerializedAs("_extraRigidbodies")]
	[SerializeField]
	[Tooltip("All colliders on this item that collide with the world, will be disabled when item is in a spawner")]
	protected ItemExtraRigidbody[] _extraRigs;

	[SerializeField]
	[Tooltip("Trigger collider used for detecting item pickup with hand, attatched to this object")]
	protected Collider _pickUpCollider;

	[Header("Dimensions")]
	[SerializeField]
	[Tooltip("Used for sniper")]
	protected float _modelForwardLength;

	[SerializeField]
	[Tooltip("Only for items spawned on level to offset y position")]
	protected float _modelHeight;

	[SerializeField]
	[Tooltip("Standard weight when spawned")]
	protected float _weight = 1f;

	[Header("Floating")]
	[SerializeField]
	private float _buoyancy = 1f;

	[SerializeField]
	private FloatInfo[] _floatInfos;

	[Header("Rendering")]
	[SerializeField]
	private List<Renderer> _renderers;

	[SerializeField]
	private List<Renderer> _skinRenderers;

	[FormerlySerializedAs("_attachBaitOn")]
	[SerializeField]
	protected Transform _attachBaitOffset;

	[Header("Purchasable")]
	[SerializeField]
	private int _cost;

	[SerializeField]
	private int _worth;

	[Header("Inventory")]
	[SerializeField]
	protected Mesh _mesh;

	[SerializeField]
	private float _inventoryMeshScale = 1f;

	[Header("Inventory Slot Pos & Rot")]
	[SerializeField]
	private Vector3 _inventoryMeshRot;

	[SerializeField]
	private Vector3 _inventoryMeshPos;

	[Header("Draw Anim")]
	[SerializeField]
	private Vector3 _drawAnimStartPos = new Vector3(0f, -1f, 0.5f);

	[SerializeField]
	private Vector3 _drawAnimStartRot;

	[Header("Hold pos")]
	[SerializeField]
	private Vector3 _heldPos;

	[SerializeField]
	private Vector3 _heldRot;

	[Header("Holding")]
	[SerializeField]
	private bool _overridePlayerVelAmount;

	[SerializeField]
	[Range(0f, 1f)]
	private float _playerVelAmount;

	[SerializeField]
	[Tooltip("Amount of bobbing from camera that should be applied to item when held")]
	private float _holdBobMulti = 1f;

	[Header("Visuals & Effects")]
	[SerializeField]
	private Transform _hoverTextTarget;

	[SerializeField]
	private float _hoverTextOffset = 0.5f;

	[SerializeField]
	[Tooltip("Effects to disable on Awake and enable on client start")]
	private List<GameObject> _trailEffects;

	[Header("Hands")]
	[SerializeField]
	protected Transform _handModelRight;

	[SerializeField]
	protected Transform _handModelLeft;

	[SerializeField]
	private HandTransforms _handTransformsLeft;

	[SerializeField]
	private HandTransforms _handTransformsRight;

	[SerializeField]
	private SkinPreset _skinPreset;

	[SerializeField]
	protected GameObject _outOfHandHolder;

	[SerializeField]
	protected GameObject _inHandHolder;

	[Header("Sounds")]
	[SerializeField]
	private WaterCollisionAudio _waterCollisionAudio;

	[SerializeField]
	private ImpactSoundType _impactSoundType;

	public readonly SyncVar<float> _syncedRandomWeight = new SyncVar<float>(1f);

	public readonly SyncVar<byte> _curSkin = new SyncVar<byte>(new SyncTypeSettings(0f, Channel.Reliable));

	public readonly SyncVar<float> _cookness = new SyncVar<float>();

	public readonly SyncVar<float> _killScoreMultiplier = new SyncVar<float>(1f);

	public readonly SyncVar<FishingRod> _rodAttachedTo = new SyncVar<FishingRod>();

	public readonly SyncVar<Player> _syncedHolder = new SyncVar<Player>(new SyncTypeSettings(0f, Channel.Reliable));

	public readonly SyncVar<bool> _isInteractable = new SyncVar<bool>(initialValue: true);

	public readonly SyncVar<float> _bettingMultiplier = new SyncVar<float>(1f);

	public readonly SyncVar<Bird> _birdHolder = new SyncVar<Bird>(new SyncTypeSettings(0f, Channel.Reliable));

	protected Player _holder;

	protected Rigidbody _rig;

	protected Tool _tool;

	protected Weapon _weapon;

	protected Melee _melee;

	protected FishingRod _fishingRod;

	protected Fish _fish;

	protected Bird _bird;

	protected Explosive _explosive;

	protected Creature _creature;

	protected DeadPlayer _deadPlayer;

	protected string _holdingLayer = "ItemInHand";

	protected string _droppedChildLayer = "ItemPart";

	protected float _timeOfDrop;

	protected ItemType _type;

	protected Player _playerWhoDropped;

	protected int _handsAbleToGrab;

	protected RigidbodySync _rigSync;

	protected float _lastTimeInteractedWith;

	protected Vector3 _fakeWorldVelocity;

	protected Vector3 _lastPos;

	protected float _minVelToHaveCollision;

	protected bool _canPickUp = true;

	protected bool _disablePlayerColOnGround;

	private FixedJoint _jointToBait;

	private Vector3[] _handModelOrigPoses;

	private PhysicsMaterial _origPhysicsMat;

	private bool _hasOutline;

	private bool _isUnderWater;

	private string _hoverString;

	private CollisionDetectionMode _defaultColMode;

	private Vector3 _startEatingPos;

	private bool NetworkInitialize___EarlyItemAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateItemAssembly_002DCSharp_002Edll_Excuted;

	[field: SerializeField]
	public bool IsQuestItem { get; private set; }

	public bool IgnoredBySeagulls => _ignoredBySeagulls;

	public bool IgnoredByMoneyNpc => _ignoredByMoneyNPC;

	public bool IgnoredByCloseDots => _ignoredByCloseDots;

	public float Buoyancy => _buoyancy;

	public FloatInfo[] FloatInfos => _floatInfos;

	public float ModelHeight => _modelHeight;

	public float InventoryMeshScale => _inventoryMeshScale;

	public Vector3 InventoryMeshRot => _inventoryMeshRot;

	public Vector3 InventoryMeshPos => _inventoryMeshPos;

	public Player SyncedHolder => _syncedHolder.Value;

	public int DefaultWorth => _worth;

	public int TotalWorth => (int)((float)(int)((float)_worth * _syncedRandomWeight.Value * GameInfo.CooknessWorthCurve.Evaluate(_cookness.Value) * _bettingMultiplier.Value) * _killScoreMultiplier.Value);

	public HandTransforms HandTransformsRight => _handTransformsRight;

	public HandTransforms HandTransformsLeft => _handTransformsLeft;

	public Transform HandModelRight => _handModelRight;

	public Transform HandModelLeft => _handModelLeft;

	public RigidbodySync RigidbodySync => _rigSync;

	public Rigidbody Rig => _rig;

	public ItemExtraRigidbody[] ExtraRigs => _extraRigs;

	public Player Holder => _holder;

	public Tool Tool => _tool;

	public Fish Fish => _fish;

	public Bird Bird => _bird;

	public Explosive Explosive => _explosive;

	public DeadPlayer DeadPlayer => _deadPlayer;

	public Creature Creature => _creature;

	public Weapon Weapon => _weapon;

	public Melee Melee => _melee;

	public Radio Radio { get; protected set; }

	public FishingRod FishingRod => _fishingRod;

	public FishingRod AttachedRod => _rodAttachedTo.Value;

	public virtual Mesh Mesh => _mesh;

	public virtual Mesh DripMesh => _mesh;

	public bool CanPickUp => _canPickUp;

	public int Cost => _cost;

	public ItemType Type => _type;

	public Vector3 HeldRot => _heldRot;

	public Transform AttachBaitOffset => _attachBaitOffset;

	public Vector3 HeldPos => _heldPos;

	public Vector3 DrawAnimStartPos => _drawAnimStartPos;

	public Vector3 DrawAnimStartRot => _drawAnimStartRot;

	public bool OverridePlayerVelAmount => _overridePlayerVelAmount;

	public float HoldBobMulti => _holdBobMulti;

	public float PlayerVelAmount => _playerVelAmount;

	public SkinPreset SkinPreset => _skinPreset;

	public Vector3 FakeWorldVelocity => _fakeWorldVelocity;

	public WaterCollisionAudio WaterCollisionAudio => _waterCollisionAudio;

	public ImpactSoundType ImpactSoundType => _impactSoundType;

	public byte ID => _id;

	public float RandomizedWeight => _syncedRandomWeight.Value;

	public bool IsDestroying { get; private set; }

	public bool IsInteractable { get; private set; }

	public bool IsColliding { get; private set; }

	public Player LastHolder { get; private set; }

	public bool WaitingForForceDrop { get; private set; }

	public Player LastPlayer { get; protected set; }

	public bool ExtraRigsAreStiff { get; protected set; }

	public bool IsInInventory { get; private set; }

	public float DefaultDamp { get; private set; }

	public float DefaultAngularDamp { get; private set; }

	public byte CurSkin => _curSkin.Value;

	public float Cookness => _cookness.Value;

	public float KillScoreMultiplier => _killScoreMultiplier.Value;

	public float BettingMultiplier => _bettingMultiplier.Value;

	public Bird BirdHolder { get; private set; }

	public static event Action OnHookItem;

	public static event Action OnPickUpItem;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Item_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public virtual string GetName()
	{
		return _nameLocalized.GetLocalizedString();
	}

	public void SetAttachedRod(FishingRod rod)
	{
		_rodAttachedTo.Value = rod;
	}

	public override void OnStartServer()
	{
	}

	public override void OnStopServer()
	{
		PlayerManager.CheckInventoriesForDestroyedItem(this);
	}

	public override void OnStartClient()
	{
		IsInteractable = _isInteractable.Value;
		ToggleWorldColliders(toEnabled: true);
		if ((bool)_syncedHolder.Value)
		{
			OnSyncedHolderChange(_syncedHolder.Value, _syncedHolder.Value, asServer: false);
		}
		ToggleTrails(to: false);
		_pickUpCollider.enabled = true;
		ShaderManager.SetItemCookness(_renderers, _cookness.Value);
		if ((bool)_rodAttachedTo.Value)
		{
			InitializeBait(_rodAttachedTo.Value);
		}
		OnBirdHolderChange(_birdHolder.Value, _birdHolder.Value, asServer: false);
	}

	public override void OnStopClient()
	{
		ItemManager.Remove(this, _pickUpCollider, _worldColliders);
	}

	protected virtual void Update()
	{
		if ((bool)_holder && _syncedHolder.Value == _holder && !IsInInventory)
		{
			_holder.Holding.RestoreHeldItemIfMissing(this);
		}
		if ((bool)_playerWhoDropped && !IsDestroying)
		{
			TrySetLayer();
		}
	}

	protected virtual void LateUpdate()
	{
	}

	protected virtual void FixedUpdate()
	{
		_fakeWorldVelocity = base.transform.position - _lastPos;
		_fakeWorldVelocity *= GameInfo.FixedTimeMultiplier;
		_lastPos = base.transform.position;
		_disablePlayerColOnGround = _playerWhoDropped;
		UpdateLayer();
		SetDamp();
		if ((bool)_holder && (bool)_tool && !_rig.isKinematic)
		{
			_rig.linearVelocity = Vector3.zero;
			_rig.angularVelocity = Vector3.zero;
		}
		bool isColliding = false;
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		for (int i = 0; i < extraRigs.Length; i++)
		{
			if (extraRigs[i].IsColliding)
			{
				isColliding = true;
				break;
			}
		}
		IsColliding = isColliding;
		if ((bool)AttachedRod)
		{
			if (_rig.linearVelocity.magnitude > 5f)
			{
				_rig.linearDamping = 10f;
			}
			else
			{
				_rig.linearDamping = 1f;
			}
			if (_rig.angularVelocity.magnitude > 5f)
			{
				_rig.angularDamping = 10f;
			}
			else
			{
				_rig.angularDamping = 1f;
			}
		}
		if (base.IsServerInitialized && (bool)BirdHolder)
		{
			Rigidbody rigidbody = (_otherRigForBird ? _otherRigForBird : _rig);
			if (!rigidbody.isKinematic)
			{
				Vector3 linearVelocity = (BirdHolder.FoodTarget.TransformPoint(_caughtOffset) - rigidbody.position) * 25f;
				rigidbody.linearVelocity = linearVelocity;
				Quaternion rotation = BirdHolder.FoodTarget.rotation;
				rotation.x = 0f;
				Vector3 angularVelocityToTarget = DazedUtils.GetAngularVelocityToTarget(rigidbody.rotation, rotation);
				angularVelocityToTarget *= 25f;
				rigidbody.angularVelocity = angularVelocityToTarget;
			}
		}
	}

	public void RemoveBaitJointOnItemReleased(FishingRod prev)
	{
		if ((bool)prev)
		{
			prev.Bait.OnRemoveBaitJoint(this);
		}
		_rig.collisionDetectionMode = _defaultColMode;
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		for (int i = 0; i < extraRigs.Length; i++)
		{
			extraRigs[i].Rig.collisionDetectionMode = _defaultColMode;
		}
		if ((bool)_jointToBait)
		{
			UnityEngine.Object.Destroy(_jointToBait);
		}
	}

	private void SetDamp()
	{
		float num = 5f;
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		foreach (ItemExtraRigidbody itemExtraRigidbody in extraRigs)
		{
			float num2 = (_isUnderWater ? WaterManager.ItemWaterDrag : itemExtraRigidbody.OrigLinearDamping);
			float num3 = (_isUnderWater ? WaterManager.ItemAngularDrag : itemExtraRigidbody.OrigAngularDamping);
			itemExtraRigidbody.Rig.linearDamping = (_isUnderWater ? num2 : Mathf.Lerp(itemExtraRigidbody.Rig.linearDamping, num2, num * Time.fixedDeltaTime));
			itemExtraRigidbody.Rig.angularDamping = (_isUnderWater ? num3 : Mathf.Lerp(itemExtraRigidbody.Rig.angularDamping, num3, num * Time.fixedDeltaTime));
		}
		float num4 = (_isUnderWater ? WaterManager.ItemWaterDrag : DefaultDamp);
		float num5 = (_isUnderWater ? WaterManager.ItemAngularDrag : DefaultAngularDamp);
		_rig.linearDamping = (_isUnderWater ? num4 : Mathf.Lerp(_rig.linearDamping, num4, num * Time.fixedDeltaTime));
		_rig.angularDamping = (_isUnderWater ? num5 : Mathf.Lerp(_rig.angularDamping, num5, num * Time.fixedDeltaTime));
	}

	public void CaughtByBird(Bird bird)
	{
		if (base.IsServerInitialized)
		{
			_birdHolder.Value = bird;
		}
	}

	public void ReleasedByBird()
	{
		if (base.IsServerInitialized)
		{
			if ((bool)_birdHolder.Value)
			{
				_birdHolder.Value.RemoveFood();
			}
			_birdHolder.Value = null;
			BirdHolder = null;
			MonoBehaviour.print("set bird holder to null");
		}
	}

	public void SetUnderWater(bool isUnderWater)
	{
		_isUnderWater = isUnderWater;
	}

	public void SetID(byte to)
	{
		_id = to;
	}

	private void InitializeBait(FishingRod rod)
	{
		if (!rod)
		{
			return;
		}
		rod.Bait.AddItemOnBait(this);
		AudioManager.PlayClipAt("FishCaught_Splash", rod.Bait.transform.position, variation: true, AudioDistance.Long, 0.25f);
		AudioManager.PlayPlayerClip("FishCaught_LineDrag", rod.Holder, variation: true, AudioDistance.Short);
		LastHolder = rod.Holder;
		if (!rod.Holder || !rod.Holder.Owner.IsLocalClient || (bool)_jointToBait)
		{
			return;
		}
		OnHookItem?.Invoke();
		_rigSync.StartSimulateLocal();
		_rigSync.TeleportToPosRot(rod.Bait.transform.position, Quaternion.identity);
		PlayerUI.CaughtFishUI(rod.Bait.transform.position, (bool)_creature && _creature.IsDrip);
		if ((bool)_creature && _creature.IsDrip)
		{
			AudioManager.PlayRandomGlobalClip("ShinyCreatureCaught_0", 1, 3, variation: false, 0.5f);
		}
		if (!rod.Bait.Rig.isKinematic)
		{
			rod.Bait.Rig.linearVelocity = Vector3.zero;
			rod.Bait.Rig.angularVelocity = Vector3.zero;
		}
		base.transform.rotation = rod.Bait.transform.rotation * Quaternion.Euler(-90f, 0f, 0f);
		_jointToBait = base.gameObject.AddComponent<FixedJoint>();
		_jointToBait.autoConfigureConnectedAnchor = false;
		_jointToBait.connectedBody = rod.Bait.Rig;
		_jointToBait.massScale = 0.5f;
		_jointToBait.connectedMassScale = 1f;
		if ((bool)_attachBaitOffset)
		{
			_jointToBait.anchor = _attachBaitOffset.transform.localPosition;
		}
		_rig.collisionDetectionMode = CollisionDetectionMode.Continuous;
		_jointToBait.connectedAnchor = rod.Bait.Info.HookPoint;
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		foreach (ItemExtraRigidbody itemExtraRigidbody in extraRigs)
		{
			itemExtraRigidbody.Rig.collisionDetectionMode = CollisionDetectionMode.Continuous;
			if (!itemExtraRigidbody.Rig.isKinematic)
			{
				itemExtraRigidbody.Rig.linearVelocity = Vector3.zero;
				itemExtraRigidbody.Rig.angularVelocity = Vector3.zero;
			}
		}
	}

	public void SetKillscoreMultiplier(float to)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized && _killScoreMultiplier.Value == 1f)
		{
			_killScoreMultiplier.Value = to;
		}
	}

	public void AddBetMultiplier(float multiplier)
	{
		if (base.IsServerInitialized)
		{
			_bettingMultiplier.Value *= multiplier;
		}
	}

	private void CheckForParent()
	{
		if (!base.transform.parent)
		{
			if ((bool)Server.Instance)
			{
				base.transform.SetParent(Server.Instance.DynamicObjectsHolder);
			}
			else
			{
				StartCoroutine(CheckForParentAfterServer());
			}
		}
	}

	private IEnumerator CheckForParentAfterServer()
	{
		yield return new WaitUntil(() => Server.Instance);
		if (!base.transform.parent)
		{
			base.transform.SetParent(Server.Instance.DynamicObjectsHolder);
		}
	}

	private void ToggleTrails(bool to)
	{
		foreach (GameObject trailEffect in _trailEffects)
		{
			trailEffect.SetActive(to);
		}
	}

	public void WasInteractedWith()
	{
		_lastTimeInteractedWith = Time.time;
	}

	public virtual void LocalHit(Transform hitTransform, Vector3 point, Vector3 dir, Player player, int damage, bool rangedHit, Vector3 force = default(Vector3), bool fromNpc = false)
	{
		AudioImpactManager.PlayExtraImpactSound(this);
		if (!player.Owner.IsLocalClient || (bool)BirdHolder)
		{
			return;
		}
		Rigidbody rig = _rig;
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		foreach (ItemExtraRigidbody itemExtraRigidbody in extraRigs)
		{
			if (hitTransform == itemExtraRigidbody.transform)
			{
				rig = itemExtraRigidbody.Rig;
				break;
			}
		}
		_rigSync.StartSimulateLocal();
		if (force != Vector3.zero)
		{
			rig.AddForceAtPosition(force, point);
		}
	}

	public void SetSyncedHolder(Player newHolder, bool forced = false)
	{
		if ((forced || ((!_syncedHolder.Value || !newHolder) && (!newHolder || newHolder.Vitals.Health > 0))) && !BirdHolder)
		{
			if ((bool)_syncedHolder.Value && !newHolder && _syncedHolder.Value.Inventory.HasItemInInventory(this))
			{
				_syncedHolder.Value.Inventory.RemoveItem(this);
			}
			_syncedHolder.Value = newHolder;
			if ((bool)newHolder && !base.IsServerInitialized && (bool)Server.Instance && Server.Instance.IsServerInitialized)
			{
				newHolder.Holding.SetUninitializedHeldItem(this);
			}
			if ((bool)newHolder)
			{
				newHolder.Inventory.ServerSetSyncedCurSlot(-1);
			}
		}
	}

	private void OnSyncedHolderChange(Player prev, Player next, bool asServer)
	{
		if (asServer)
		{
			if ((bool)next && _rigSync.SyncedSimulator != next.Owner)
			{
				_rigSync.ServerSetSyncedSimulator(next.Owner);
			}
		}
		else if (_holder == next)
		{
			if ((bool)next && !IsInInventory)
			{
				next.Holding.RestoreHeldItemIfMissing(this);
			}
		}
		else if ((bool)next)
		{
			if ((bool)_holder)
			{
				Drop();
			}
			PickUp(next);
		}
		else
		{
			SpawnFromInventory();
			Drop();
		}
	}

	public void ReconcileLocalHolderWithSyncedHolder()
	{
		if (base.IsDeinitializing)
		{
			return;
		}
		Player value = _syncedHolder.Value;
		if (!(_holder == value))
		{
			if ((bool)_holder)
			{
				Drop();
			}
			if ((bool)value)
			{
				PickUp(value);
			}
		}
	}

	public void PickUp(Player player, bool calledFromLocal = false, bool sendToServer = false)
	{
		if (calledFromLocal)
		{
			if ((bool)BirdHolder)
			{
				return;
			}
			OnPickUpItem?.Invoke();
			if (sendToServer)
			{
				Server.Instance.SetItemHolder(this, player, player.Holding.HeldItem);
			}
			_rigSync.StartSimulateLocal();
		}
		if (player.Owner.IsLocalClient)
		{
			player.Inventory.OnItemPickup(this);
		}
		if ((bool)_rodAttachedTo.Value)
		{
			_rodAttachedTo.Value.ReleaseItem(this);
		}
		_rigSync.SetKinematic(kinematic: false);
		ToggleWorldColliders(toEnabled: true);
		SetColliderMat(toHolding: true);
		LeanTween.cancel(base.gameObject);
		base.transform.localScale = Vector3.one;
		_holder = player;
		UpdateLayer();
		LastHolder = _holder;
		LastPlayer = player;
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		for (int i = 0; i < extraRigs.Length; i++)
		{
			extraRigs[i].ResetColCount();
		}
		_holder.Holding.PickUpItem(this, calledFromLocal);
		AudioManager.PlayPlayerClip("PickUp", player, variation: false, AudioDistance.VeryShort, 0.2f);
	}

	public void Drop(bool calledFromLocal = false, Vector3 dropForce = default(Vector3), Vector3 dropAngForce = default(Vector3))
	{
		if (!_holder)
		{
			return;
		}
		if (calledFromLocal)
		{
			if (IsInInventory)
			{
				return;
			}
			Server.Instance.SetItemHolder(this, null, null);
			_rigSync.SetKinematic(kinematic: false);
			_rig.linearVelocity += dropForce;
			Rigidbody[] extraHinges = _rigSync.ExtraHinges;
			for (int i = 0; i < extraHinges.Length; i++)
			{
				extraHinges[i].linearVelocity += dropForce * 0.75f;
			}
			_rig.angularVelocity += dropAngForce;
		}
		WasInteractedWith();
		_rigSync.SetKinematic(kinematic: false);
		_playerWhoDropped = _holder;
		_timeOfDrop = Time.time;
		if (_holder.IsOwner & calledFromLocal)
		{
			RigidbodySync.StartSimulateLocal();
		}
		_holder.Hands.DropItem(putInInv: false, this);
		if (_holder.Holding.HeldItem == this)
		{
			_holder.Holding.DropItem(calledFromLocal, this);
		}
		_rigSync.ObserverSetOverride(0f);
		OnDrop();
		SetColliderMat(toHolding: false);
		ToggleTrails(to: true);
		if (!IsDestroying)
		{
			AudioManager.PlayPlayerClip("DropItem", _holder, variation: true, AudioDistance.VeryShort, 0.5f);
		}
		_holder = null;
		UpdateLayer();
	}

	public void PutInInventory()
	{
		if (!IsInInventory)
		{
			IsInInventory = true;
			if ((bool)Fish)
			{
				Fish.ResetJoint();
			}
			base.gameObject.SetActive(value: false);
			OnDrop();
		}
	}

	public void SpawnFromInventory()
	{
		if (IsInInventory)
		{
			IsInInventory = false;
			if (!Tool)
			{
				Vector3 pos = _holder.Transform.position + _holder.CurPlayerRot * (_holder.Holding.ItemDrawStartPos + Vector3.right * _heldPos.x);
				Quaternion rot = _holder.CurPlayerRot * Quaternion.Euler(DrawAnimStartRot);
				_rigSync.TeleportToPosRot(pos, rot);
			}
			base.gameObject.SetActive(value: true);
			OnPickUp();
		}
	}

	public void DestroyItem(byte byteReason = 0, byte npcID = byte.MaxValue)
	{
		if (base.IsServerInitialized)
		{
			IsDestroying = true;
			base.gameObject.layer = 0;
			_isInteractable.Value = false;
			Collider[] worldColliders = _worldColliders;
			for (int i = 0; i < worldColliders.Length; i++)
			{
				worldColliders[i].gameObject.layer = 0;
			}
			ObserverDestroyItem(byteReason, npcID);
		}
	}

	[ObserversRpc]
	private void ObserverDestroyItem(byte byteReason = 0, byte npcID = byte.MaxValue)
	{
		RpcWriter___ObserverDestroyItem___1980385015(byteReason, npcID);
	}

	private void DestroyByWater()
	{
		float waterHeight = WaterManager.GetWaterHeight(_rig.worldCenterOfMass);
		Vector3 worldCenterOfMass = _rig.worldCenterOfMass;
		worldCenterOfMass.y = waterHeight;
		LeanTween.scale(base.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnComplete(DespawnItemOnServer);
		VFXManager.Play("WaterSplash", worldCenterOfMass, Vector3.zero);
		AudioManager.PlayRandomClipAt(WaterCollisionAudio switch
		{
			WaterCollisionAudio.Light => "ItemHitWaterLight_V", 
			WaterCollisionAudio.Medium => "ItemHitWaterMedium_V", 
			WaterCollisionAudio.Heavy => "ItemHitWaterHeavy_V", 
			_ => "ItemHitWaterMedium_V", 
		}, 1, 3, base.transform.position, variation: true);
	}

	private void DestroyByNpc(byte npcID)
	{
		_rigSync.SetKinematic(kinematic: true);
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		for (int i = 0; i < extraRigs.Length; i++)
		{
			extraRigs[i].Rig.isKinematic = true;
		}
		NPC nPC = NPCManager.IDToNpc(npcID);
		AchievementManager.CheckSellWorthAchievement(this);
		Vector3 to = (nPC ? nPC.MouthPosForItems.position : base.transform.position);
		LeanTween.move(base.gameObject, to, 0.25f).setEase(LeanTweenType.easeOutQuad);
		LeanTween.scale(base.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(DespawnItemOnServer);
	}

	private void DestroyByEating()
	{
		_rigSync.SetKinematic(kinematic: true);
		if ((bool)_holder)
		{
			AudioManager.PlayPlayerClip("Swallow", _holder, variation: true, AudioDistance.Short);
			Drop();
			if (_playerWhoDropped.Owner.IsLocalClient)
			{
				AchievementManager.CheckEatingAchievements(this);
				_startEatingPos = _playerWhoDropped.CamObject.InverseTransformPoint(base.transform.position);
			}
			else
			{
				_startEatingPos = _playerWhoDropped.Body.Head.InverseTransformPoint(base.transform.position);
			}
		}
		LeanTween.value(base.gameObject, 0f, 1f, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(OnDestroyByEatingAnimation);
		LeanTween.scale(base.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(DespawnItemOnServer);
	}

	private void DestroyBySlotMachine()
	{
		_rigSync.SetKinematic(kinematic: true);
		LeanTween.value(base.gameObject, 0f, 1f, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(OnDestroyByEatingAnimation);
		LeanTween.scale(base.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(DespawnItemOnServer);
		Transform slotDestroyTarget = SlotMachine.SlotDestroyTarget;
		if ((bool)slotDestroyTarget)
		{
			LeanTween.move(base.gameObject, slotDestroyTarget.position, 0.25f).setEase(LeanTweenType.easeOutQuad);
		}
	}

	private void DetroyByBoss()
	{
		_rigSync.SetKinematic(kinematic: true);
		LeanTween.value(base.gameObject, 0f, 1f, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(OnDestroyByEatingAnimation);
		LeanTween.scale(base.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeOutQuad).setOnComplete(DespawnItemOnServer);
		VFXManager.Play("SmallBloodExplosion", base.transform.position);
		AudioManager.PlayRandomClipAt("TakeDamage_0", 1, 5, base.transform.position);
	}

	private void DestroyByVolcano()
	{
		LeanTween.scale(base.gameObject, Vector3.zero, 2f).setEase(LeanTweenType.easeOutQuad).setOnComplete(DespawnItemOnServer);
		LeanTween.move(base.gameObject, base.transform.position + Vector3.down * 5f, 2f).setEase(LeanTweenType.easeInQuad);
		ParticleManager.Play("LavaPoof", base.transform.position);
		AudioManager.PlayClipAt("BONUS_WhaleInLava_01", base.transform.position, variation: false, AudioDistance.Long);
	}

	private void DestroyImmediate()
	{
		_rigSync.SetKinematic(kinematic: true);
		DespawnItemOnServer();
	}

	private void OnDestroyByEatingAnimation(float lerp)
	{
		if ((bool)_playerWhoDropped)
		{
			Vector3 zero = Vector3.zero;
			Transform transform;
			if (_playerWhoDropped.Owner.IsLocalClient)
			{
				zero = _playerWhoDropped.Eating.MouthPos;
				transform = _playerWhoDropped.CamObject;
			}
			else
			{
				zero = _playerWhoDropped.Eating.ObserverMouthPos;
				transform = _playerWhoDropped.Body.Head;
			}
			base.transform.position = transform.TransformPoint(Vector3.Lerp(_startEatingPos, zero, lerp));
		}
	}

	private void DespawnItemOnServer()
	{
		if (base.IsServerInitialized && !base.IsDeinitializing)
		{
			Despawn(base.gameObject);
		}
	}

	public void Hover()
	{
		if (!IsDestroying && !base.IsDeinitializing)
		{
			bool flag = (bool)_creature && _creature.IsDrip;
			string text = (flag ? (LocalizationManager.DripLocalized.GetLocalizedString() + " ") : "") + GetName() + "\n" + SpriteManager.GetPickUpInput();
			if (!_hasOutline)
			{
				PlayerUI.SetLookAtText(text, _hoverTextTarget ? _hoverTextTarget : base.transform, Vector3.up * _hoverTextOffset, flag);
			}
			else if (text != _hoverString)
			{
				PlayerUI.UpdateLookAtText(text);
			}
			_hoverString = text;
			_hasOutline = true;
			UpdateLayer();
		}
	}

	public void UnHover()
	{
		_hasOutline = false;
		PlayerUI.HideLookAtText(_hoverString);
		UpdateLayer();
	}

	protected void UpdateLayer()
	{
		if (_hasOutline && IsInteractable)
		{
			_droppedChildLayer = (_disablePlayerColOnGround ? "OutlinedObjectNoPlayerCol" : "OutlinedObject");
		}
		else
		{
			_droppedChildLayer = (_disablePlayerColOnGround ? "ItemPartNoPlayerCol" : "ItemPart");
		}
		bool flag = !_holder && !AttachedRod;
		string layerName = (flag ? _droppedChildLayer : _holdingLayer);
		foreach (Renderer renderer in _renderers)
		{
			renderer.gameObject.layer = LayerMask.NameToLayer(layerName);
		}
		Collider[] worldColliders = _worldColliders;
		for (int i = 0; i < worldColliders.Length; i++)
		{
			worldColliders[i].gameObject.layer = LayerMask.NameToLayer(layerName);
		}
		string layerName2 = (flag ? "Item" : "ItemInHand");
		_pickUpCollider.gameObject.layer = LayerMask.NameToLayer(layerName2);
	}

	private void TrySetLayer()
	{
		Vector3 position = _playerWhoDropped.Transform.position;
		position.y = 0f;
		Vector3 position2 = base.transform.position;
		position2.y = 0f;
		if (!(Vector3.Distance(position, position2) < 0.75f) || !(Mathf.Abs(_playerWhoDropped.Transform.position.y - base.transform.position.y) < 1.25f))
		{
			_playerWhoDropped = null;
		}
	}

	protected void ToggleWorldColliders(bool toEnabled)
	{
		Collider[] worldColliders = _worldColliders;
		for (int i = 0; i < worldColliders.Length; i++)
		{
			worldColliders[i].enabled = toEnabled;
		}
	}

	private void SetColliderMat(bool toHolding)
	{
		PhysicsMaterial material = (toHolding ? GameInfo.HoldingPhysicsMat : _origPhysicsMat);
		Collider[] worldColliders = _worldColliders;
		for (int i = 0; i < worldColliders.Length; i++)
		{
			worldColliders[i].material = material;
		}
	}

	public void ToggleInteractable(bool to)
	{
		if (base.IsServerInitialized)
		{
			_isInteractable.Value = to;
			if (!to)
			{
				_rigSync.StartSimulateLocal();
			}
			_rigSync.Freeze(!to);
		}
	}

	public void CookItem(float amount)
	{
		if (base.IsServerInitialized)
		{
			_cookness.Value = Mathf.Clamp(_cookness.Value + amount, 0f, 2f);
		}
	}

	public void CookWithlava()
	{
		if (base.IsServerInitialized)
		{
			if (_cookness.Value < 1f)
			{
				CookItem(1f - _cookness.Value);
			}
			ObserversCookWithLavaEffects();
			CookWithLavaEffects();
		}
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserversCookWithLavaEffects()
	{
		RpcWriter___ObserversCookWithLavaEffects___2166136261();
	}

	private void CookWithLavaEffects()
	{
		ParticleManager.Play("LavaPoof", Rig.worldCenterOfMass);
		AudioManager.PlayRandomClipAt("LavaPoof_V", 1, 6, Rig.worldCenterOfMass, variation: false, AudioDistance.Short, 0.5f);
	}

	private void OnIsInteractableChange(bool prev, bool next, bool asServer)
	{
		if (!asServer)
		{
			IsInteractable = next;
			_rigSync.Freeze(!next);
		}
	}

	private void OnCooknessChange(float prev, float next, bool asServer)
	{
		ShaderManager.SetItemCookness(_renderers, next);
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			_holder.Inventory.ApplyCooknessInInventory(this);
		}
	}

	private void OnCurSkinChange(byte prev, byte next, bool asServer)
	{
		if (!asServer && (bool)_skinPreset && _skinPreset.Skins.Count != 0)
		{
			byte index = (byte)Mathf.Clamp(next, 0, _skinPreset.Skins.Count - 1);
			ApplySkin(_skinPreset.Skins[index]);
		}
	}

	private void ApplySkin(ItemSkin itemSkin)
	{
		ShaderManager.ApplyItemSkin(itemSkin, _skinRenderers);
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			_holder.Inventory.ApplySkinInInventory(this);
		}
	}

	public void ServerSetSkin(byte to)
	{
		if (base.IsServerInitialized && (bool)_skinPreset && _skinPreset.Skins.Count != 0)
		{
			_curSkin.Value = (byte)Mathf.Clamp(to, 0, _skinPreset.Skins.Count - 1);
		}
	}

	private void OnAttachedRodChange(FishingRod prev, FishingRod next, bool asServer)
	{
		if (asServer || !Server.Instance || !Server.Instance.IsServerInitialized)
		{
			if ((bool)next)
			{
				InitializeBait(next);
			}
			else
			{
				RemoveBaitJointOnItemReleased(prev);
			}
		}
	}

	protected virtual void OnCollisionEnter(Collision other)
	{
	}

	public virtual void OnCollision(Collision col)
	{
	}

	public virtual void OnDrop()
	{
		if ((bool)_inHandHolder)
		{
			_inHandHolder.SetActive(value: false);
		}
		if ((bool)_outOfHandHolder)
		{
			_outOfHandHolder.SetActive(value: true);
		}
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			PlayerUI.HideSpecificItemInfo();
		}
	}

	public virtual void OnPickUp()
	{
		if ((bool)_inHandHolder)
		{
			_inHandHolder.SetActive(value: true);
		}
		if ((bool)_outOfHandHolder)
		{
			_outOfHandHolder.SetActive(value: false);
		}
		if ((bool)_birdHolder.Value && base.IsServerInitialized)
		{
			_birdHolder.Value.RemoveFood();
			ReleasedByBird();
		}
		Boat.ToggleWantToDrive(to: false);
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			PlayerUI.ShowSpecificItemInfo(this);
		}
	}

	public virtual ExplosionInfo GetExplosionInfo()
	{
		return null;
	}

	private void OnBirdHolderChange(Bird prev, Bird next, bool asServer)
	{
		if (asServer)
		{
			if ((bool)_holder)
			{
				if ((bool)next)
				{
					next.RemoveFood();
				}
				ReleasedByBird();
				return;
			}
			_rigSync.StartSimulateLocal();
		}
		if (!_holder)
		{
			AchievementManager.CheckPickedUpBySeagullAchievement(next, this);
			BirdHolder = next;
		}
	}

	public virtual void PrimaryInput(InputAction.CallbackContext context)
	{
	}

	public virtual void PrimaryInputCancel(InputAction.CallbackContext context)
	{
	}

	public virtual void SecondaryInput(InputAction.CallbackContext context)
	{
	}

	public virtual void SecondaryInputCanceled(InputAction.CallbackContext context)
	{
	}

	public virtual void ReloadInput(InputAction.CallbackContext context)
	{
	}

	public virtual void InspectInput(InputAction.CallbackContext context)
	{
		PlayerUI.ShowInspectInfo(this);
	}

	public virtual void LoadFromSave(SavedItem savedItem)
	{
		_cookness.Value = savedItem.Cookness;
		_bettingMultiplier.Value = savedItem.BettingMultiplier;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_birdHolder.InitializeEarly(this, 8u, isSyncObject: false);
			_bettingMultiplier.InitializeEarly(this, 7u, isSyncObject: false);
			_isInteractable.InitializeEarly(this, 6u, isSyncObject: false);
			_syncedHolder.InitializeEarly(this, 5u, isSyncObject: false);
			_rodAttachedTo.InitializeEarly(this, 4u, isSyncObject: false);
			_killScoreMultiplier.InitializeEarly(this, 3u, isSyncObject: false);
			_cookness.InitializeEarly(this, 2u, isSyncObject: false);
			_curSkin.InitializeEarly(this, 1u, isSyncObject: false);
			_syncedRandomWeight.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterObserversRpc(0u, RpcReader___ObserverDestroyItem___1980385015);
			RegisterObserversRpc(1u, RpcReader___ObserversCookWithLavaEffects___2166136261);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateItemAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_birdHolder.InitializeLate();
			_bettingMultiplier.InitializeLate();
			_isInteractable.InitializeLate();
			_syncedHolder.InitializeLate();
			_rodAttachedTo.InitializeLate();
			_killScoreMultiplier.InitializeLate();
			_cookness.InitializeLate();
			_curSkin.InitializeLate();
			_syncedRandomWeight.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverDestroyItem___1980385015(byte byteReason = 0, byte npcID = byte.MaxValue)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(byteReason);
		pooledWriter.WriteUInt8Unpacked(npcID);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverDestroyItem___1980385015(byte P_0, byte P_1)
	{
		IsDestroying = true;
		base.gameObject.layer = 0;
		Collider[] worldColliders = _worldColliders;
		foreach (Collider obj in worldColliders)
		{
			obj.gameObject.layer = 0;
			obj.enabled = false;
		}
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			PlayerUI.HideSpecificItemInfo();
		}
		if ((bool)_deadPlayer)
		{
			_deadPlayer.SetVoiceChatParent(toThis: false);
		}
		PlayerUI.HideLookAtText(_hoverString);
		LeanTween.cancel(base.gameObject);
		switch ((DestroyReason)P_0)
		{
		case DestroyReason.Default:
			AudioManager.PlayClipAt("Poof", base.transform.position, variation: true, AudioDistance.VeryShort, 0.25f);
			ParticleManager.Play("Smoke", base.transform.position, Vector3.up);
			DespawnItemOnServer();
			break;
		case DestroyReason.Water:
			DestroyByWater();
			break;
		case DestroyReason.NPC:
			DestroyByNpc(P_1);
			break;
		case DestroyReason.Eating:
			DestroyByEating();
			break;
		case DestroyReason.SlotMachine:
			DestroyBySlotMachine();
			break;
		case DestroyReason.KilledByBoss:
			DetroyByBoss();
			break;
		case DestroyReason.Volcano:
			DestroyByVolcano();
			break;
		case DestroyReason.Immediate:
			DestroyImmediate();
			break;
		}
	}

	private void RpcReader___ObserverDestroyItem___1980385015(PooledReader PooledReader0, Channel channel)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		byte b2 = PooledReader0.ReadUInt8Unpacked();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverDestroyItem___1980385015(b, b2);
		}
	}

	private void RpcWriter___ObserversCookWithLavaEffects___2166136261()
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendObserversRpc(1u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserversCookWithLavaEffects___2166136261()
	{
		CookWithLavaEffects();
	}

	private void RpcReader___ObserversCookWithLavaEffects___2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserversCookWithLavaEffects___2166136261();
		}
	}

	protected virtual void Awake_UserLogic_Item_Assembly_002DCSharp_002Edll()
	{
		_rig = GetComponent<Rigidbody>();
		_rigSync = GetComponent<RigidbodySync>();
		_defaultColMode = _rig.collisionDetectionMode;
		DefaultDamp = _rig.linearDamping;
		DefaultAngularDamp = _rig.angularDamping;
		ItemManager.Add(this, _pickUpCollider, _worldColliders);
		_pickUpCollider.enabled = false;
		_minVelToHaveCollision = GameInfo.MinVelForItemsToHavePlayerColWhenLow;
		if (_worldColliders.Length != 0)
		{
			_origPhysicsMat = _worldColliders[0].material;
		}
		CheckForParent();
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		for (int i = 0; i < extraRigs.Length; i++)
		{
			extraRigs[i].SetItem(this);
		}
		_curSkin.OnChange += OnCurSkinChange;
		_isInteractable.OnChange += OnIsInteractableChange;
		_cookness.OnChange += OnCooknessChange;
		_rodAttachedTo.OnChange += OnAttachedRodChange;
		_syncedHolder.OnChange += OnSyncedHolderChange;
		_birdHolder.OnChange += OnBirdHolderChange;
	}
}
