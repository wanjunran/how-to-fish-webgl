using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerInventory : NetworkBehaviour
{
	[FormerlySerializedAs("_onlineStartingSlots")]
	[SerializeField]
	private int _startingSlots = 3;

	[SerializeField]
	private List<Item> _startingItems;

	[SerializeField]
	private List<InventorySlot> _itemSlots;

	[SerializeField]
	private BaitSlot _baitSlot;

	[SerializeField]
	private int[] _extraSlotCosts;

	[SerializeField]
	private Player _player;

	[SerializeField]
	private Camera _inventoryCamera;

	public readonly SyncDictionary<byte, Item> _items = new SyncDictionary<byte, Item>();

	public readonly SyncVar<int> _syncedCurSlot = new SyncVar<int>(-1);

	public readonly SyncVar<byte> _curBait = new SyncVar<byte>();

	public readonly SyncVar<byte> _extraSlots = new SyncVar<byte>();

	public readonly SyncList<int> _ownedBaits = new SyncList<int>();

	private int _localCurSlot = -1;

	private SavedPlayer _toLoadFrom;

	private bool NetworkInitialize___EarlyPlayerInventoryAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerInventoryAssembly_002DCSharp_002Edll_Excuted;

	public byte ExtraSlots => _extraSlots.Value;

	public byte CurBait => _curBait.Value;

	public Item SyncedCurItem { get; private set; }

	private List<InventorySlot> _availableSlots => _itemSlots.GetRange(0, GetTotalSlots());

	public static event Action OnAddedToInventory;

	private int GetTotalSlots()
	{
		return _startingSlots + _extraSlots.Value;
	}

	public int GetExtraSlotCost(byte index)
	{
		return _extraSlotCosts[index - 1];
	}

	public bool HasItemInInventory(Item item)
	{
		if (HasSyncedInventoryItem(item))
		{
			return true;
		}
		if (item.IsInInventory)
		{
			return item.SyncedHolder == _player;
		}
		return false;
	}

	private bool HasSyncedInventoryItem(Item item)
	{
		return _items.Any((KeyValuePair<byte, Item> pair) => pair.Value == item);
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_PlayerInventory_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		for (byte b = 0; b < 9; b++)
		{
			_items[b] = null;
		}
		for (int i = 1; i < GameInfo.AllBaits.Count; i++)
		{
			_ownedBaits.Add(0);
		}
		if (_toLoadFrom != null)
		{
			LoadFromSave();
		}
	}

	public override void OnStopServer()
	{
		if (!base.Owner.IsLocalClient && (bool)Server.Instance && !Server.Instance.IsDeinitializing)
		{
			SaveInventory(andDestroy: true);
		}
	}

	public void SaveInventory(bool andDestroy = false)
	{
		Dictionary<byte, Item> dictionary = new Dictionary<byte, Item>();
		foreach (KeyValuePair<byte, Item> item2 in _items)
		{
			if ((bool)item2.Value)
			{
				dictionary.Add(item2.Key, item2.Value);
			}
		}
		Item item = _player.Holding.HeldItem;
		if ((bool)item && (bool)item.DeadPlayer)
		{
			if (andDestroy)
			{
				item.Drop(calledFromLocal: true, Vector3.zero, Vector3.zero);
			}
			item = null;
		}
		if (!item || item.SyncedHolder != _player || dictionary.ContainsValue(item))
		{
			item = null;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < _ownedBaits.Count; i++)
		{
			list.Add(_ownedBaits[i]);
		}
		SaveManager.SavePlayer(_player, item, dictionary, _extraSlots.Value, list);
		if (!andDestroy)
		{
			return;
		}
		for (int num = _items.Count - 1; num >= 0; num--)
		{
			if ((bool)_items[(byte)num])
			{
				Despawn(_items[(byte)num].gameObject);
			}
		}
		if ((bool)item)
		{
			Despawn(item.gameObject);
		}
	}

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			InitializeLocal();
		}
		StartCoroutine(InitializeInventory());
		_curBait.OnChange += OnCurBaitChange;
	}

	private IEnumerator InitializeInventory()
	{
		foreach (KeyValuePair<byte, Item> item in _items)
		{
			if ((bool)item.Value)
			{
				yield return new WaitUntil(() => item.Value.OnStartClientCalled);
				if (item.Key != _syncedCurSlot.Value || _syncedCurSlot.Value == -1)
				{
					item.Value.PutInInventory();
				}
			}
		}
		UpdateHeldItem(_syncedCurSlot.Value);
	}

	public override void OnStopClient()
	{
		if (base.Owner.IsLocalClient)
		{
			UnbindInputs();
		}
	}

	private IEnumerator SpawnStartingItems()
	{
		yield return new WaitUntil(() => (bool)Server.Instance && (bool)ItemManager.Instance);
		byte b = 0;
		while (b < _startingItems.Count && b <= GetTotalSlots() - 1)
		{
			Item item = UnityEngine.Object.Instantiate(_startingItems[b], _player.CamObject.position + _player.CamObject.forward, Quaternion.identity);
			item.SetSyncedHolder(_player);
			Spawn(item.gameObject);
			_items[b] = item;
			b++;
		}
	}

	private void InitializeLocal()
	{
		BindInputs();
		_inventoryCamera.targetTexture = GameInfo.InventoryTexture;
		int totalSlots = GetTotalSlots();
		for (int i = 0; i < _itemSlots.Count; i++)
		{
			_itemSlots[i].Initialize(i < totalSlots);
		}
		_baitSlot.SetBait(GameInfo.AllBaits[0], -1);
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["Inventory1"].performed += SelectSlotInput;
		input.actions["Inventory2"].performed += SelectSlotInput;
		input.actions["Inventory3"].performed += SelectSlotInput;
		input.actions["Inventory4"].performed += SelectSlotInput;
		input.actions["Inventory5"].performed += SelectSlotInput;
		input.actions["Inventory6"].performed += SelectSlotInput;
		input.actions["Inventory7"].performed += SelectSlotInput;
		input.actions["Inventory8"].performed += SelectSlotInput;
		input.actions["Inventory9"].performed += SelectSlotInput;
		input.actions["InventoryNone"].performed += DeselectSlotInput;
		input.actions["InventoryScroll"].performed += ScrollSlotInput;
		input.actions["ChangeBait"].performed += ChangeBaitInput;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["Inventory1"].performed -= SelectSlotInput;
			input.actions["Inventory2"].performed -= SelectSlotInput;
			input.actions["Inventory3"].performed -= SelectSlotInput;
			input.actions["Inventory4"].performed -= SelectSlotInput;
			input.actions["Inventory5"].performed -= SelectSlotInput;
			input.actions["Inventory6"].performed -= SelectSlotInput;
			input.actions["Inventory7"].performed -= SelectSlotInput;
			input.actions["Inventory8"].performed -= SelectSlotInput;
			input.actions["Inventory9"].performed -= SelectSlotInput;
			input.actions["InventoryNone"].performed -= DeselectSlotInput;
			input.actions["InventoryScroll"].performed -= ScrollSlotInput;
			input.actions["ChangeBait"].performed -= ChangeBaitInput;
		}
	}

	public void OnItemPickup(Item newItem)
	{
		foreach (KeyValuePair<byte, Item> item in _items)
		{
			if (item.Value == newItem)
			{
				return;
			}
		}
		if (_localCurSlot >= 0)
		{
			_itemSlots[_localCurSlot].Deselect();
			_localCurSlot = -1;
		}
	}

	private void DeselectSlotInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && !Boat.IsDrivingLocally && !Boat.WantToDrive)
		{
			LocalTrySelectSlot(-1);
		}
	}

	private void SelectSlotInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && !Boat.IsDrivingLocally && !Boat.WantToDrive)
		{
			int num = (int)context.ReadValue<float>();
			if (num < GetTotalSlots())
			{
				LocalTrySelectSlot(num);
			}
		}
	}

	private void ScrollSlotInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && !Boat.IsDrivingLocally && !Boat.WantToDrive)
		{
			int num = -(int)context.ReadValue<float>();
			int num2 = _localCurSlot + num;
			if (num2 < 0)
			{
				num2 = _availableSlots.Count - 1;
			}
			else if (num2 >= _availableSlots.Count)
			{
				num2 = 0;
			}
			LocalTrySelectSlot(num2);
		}
	}

	private void ChangeBaitInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && !Boat.IsDrivingLocally && !Boat.WantToDrive && (!_player.Holding.HeldItem || !_player.Holding.HeldItem.FishingRod || !_player.Holding.HeldItem.FishingRod.Bait.ItemOnBait))
		{
			byte nextAvailableBait = GetNextAvailableBait();
			if (_curBait.Value != nextAvailableBait)
			{
				Server.Instance.ChangeBait(_player, nextAvailableBait);
			}
		}
	}

	public void AddItem(byte index, Item item)
	{
		if (base.IsServerInitialized && !(item.SyncedHolder != _player) && !_items.Any((KeyValuePair<byte, Item> inventoryItem) => inventoryItem.Value == item))
		{
			_items[index] = item;
		}
	}

	public void RemoveItem(Item item)
	{
		if (!base.IsServerInitialized)
		{
			return;
		}
		foreach (KeyValuePair<byte, Item> item2 in _items)
		{
			if (item2.Value == item)
			{
				_items[item2.Key] = null;
				break;
			}
		}
	}

	private int GetOpenSlot(int selected)
	{
		if (selected != -1 && _items[(byte)selected] == null)
		{
			return selected;
		}
		for (byte b = 0; b < _availableSlots.Count; b++)
		{
			if (_items[b] == null)
			{
				return b;
			}
		}
		return -1;
	}

	public void ResolveHeldItemReplacement(Item item, bool calledFromLocal)
	{
		if (!item || !base.Owner.IsLocalClient)
		{
			return;
		}
		if ((bool)item.DeadPlayer)
		{
			item.Drop();
		}
		else if (HasItemInInventory(item))
		{
			item.PutInInventory();
		}
		else
		{
			if (!calledFromLocal)
			{
				return;
			}
			int openSlot = GetOpenSlot(0);
			if (openSlot != -1)
			{
				if (item.SyncedHolder == _player)
				{
					Server.Instance.PutItemInInventory(_player, item, (byte)openSlot);
				}
				item.PutInInventory();
			}
			else
			{
				item.Drop();
			}
		}
	}

	public bool ServerTryStoreHeldItem(Item item)
	{
		if (!base.IsServerInitialized || !item || item.IsDeinitializing || (bool)item.DeadPlayer || item.SyncedHolder != _player)
		{
			return false;
		}
		if (HasSyncedInventoryItem(item))
		{
			return true;
		}
		int openSlot = GetOpenSlot(0);
		if (openSlot == -1)
		{
			return false;
		}
		AddItem((byte)openSlot, item);
		return HasSyncedInventoryItem(item);
	}

	public void LocalTrySelectSlot(int slot)
	{
		Item uninitializedHeldItem = _player.Holding.UninitializedHeldItem;
		if ((bool)uninitializedHeldItem && !HasItemInInventory(uninitializedHeldItem))
		{
			return;
		}
		Item heldItem = _player.Holding.HeldItem;
		if ((bool)heldItem && (heldItem.Holder != _player || heldItem.SyncedHolder != _player) && ((bool)heldItem.Holder || (bool)heldItem.SyncedHolder))
		{
			return;
		}
		int openSlot = GetOpenSlot(slot);
		Item item = ((slot >= 0) ? _items[(byte)slot] : null);
		if ((bool)heldItem)
		{
			if ((bool)heldItem.DeadPlayer)
			{
				heldItem.Drop(calledFromLocal: true);
			}
			else if (HasItemInInventory(heldItem))
			{
				if (heldItem == item)
				{
					slot = -1;
				}
			}
			else if (openSlot == -1)
			{
				heldItem.Drop(calledFromLocal: true);
			}
			else
			{
				Server.Instance.PutItemInInventory(_player, heldItem, (byte)openSlot);
				if (slot == openSlot)
				{
					slot = -1;
				}
			}
		}
		ApplySlot(slot);
	}

	public void ApplySlot(int slot)
	{
		if (_localCurSlot != -1)
		{
			_availableSlots[_localCurSlot].Deselect();
		}
		if (_localCurSlot == slot)
		{
			_localCurSlot = -1;
		}
		else
		{
			_localCurSlot = slot;
		}
		if (_localCurSlot != -1)
		{
			_availableSlots[_localCurSlot].Select();
		}
		Server.Instance.SelectInvSlot(_player, _localCurSlot);
	}

	public void ServerSetSyncedCurSlot(int slot)
	{
		_syncedCurSlot.Value = slot;
	}

	private void OnItemsChange(SyncDictionaryOperation op, byte index, Item item, bool asServer)
	{
		if (asServer)
		{
			return;
		}
		if (base.Owner.IsLocalClient)
		{
			if (op == SyncDictionaryOperation.Set)
			{
				_itemSlots[index].SetItem(item);
				OnAddedToInventory?.Invoke();
			}
			else if ((bool)item)
			{
				_itemSlots[index].SetItem(item);
				OnAddedToInventory?.Invoke();
			}
			if (index == _localCurSlot || _localCurSlot == -1)
			{
				PlayerUI.StopDropUI();
			}
			UpdateInventoryCamera();
		}
		if (op == SyncDictionaryOperation.Set)
		{
			UpdateHeldItem(_syncedCurSlot.Value);
		}
	}

	private void OnCurSlotChange(int prev, int next, bool asServer)
	{
		if (!base.IsServerInitialized || asServer)
		{
			if (base.Owner.IsLocalClient)
			{
				PlayerUI.StopDropUI();
			}
			if (prev != -1 || next != -1)
			{
				UpdateHeldItem(next);
			}
		}
	}

	private void UpdateHeldItem(int selectedIndex)
	{
		Item item = (SyncedCurItem = HideUnselectedItems(selectedIndex));
		if (base.Owner.IsLocalClient)
		{
			Item heldItem = _player.Holding.HeldItem;
			if ((bool)heldItem && !HasItemInInventory(heldItem) && !heldItem.IsInInventory)
			{
				if (!SyncedCurItem)
				{
					return;
				}
				int openSlot = GetOpenSlot(0);
				if (heldItem.SyncedHolder == _player && openSlot != -1)
				{
					Server.Instance.PutItemInInventory(_player, heldItem, (byte)openSlot);
				}
				else
				{
					heldItem.Drop(calledFromLocal: true);
				}
			}
		}
		if ((bool)item && !item.IsInInventory && item.Holder != _player)
		{
			item = null;
		}
		if ((bool)item)
		{
			item.SpawnFromInventory();
			_player.Holding.PickUpItem(item, base.Owner.IsLocalClient);
			_player.Hands.SpawnItemFromInventory();
			if ((bool)item.Tool)
			{
				_player.ToolMovement.SpawnFromInventory();
			}
			AudioManager.PlayPlayerClip("EquipItem", _player, variation: true, AudioDistance.VeryShort, 0.25f);
		}
		else if ((bool)_player.Holding.HeldItem && _player.Holding.HeldItem.IsInInventory)
		{
			_player.Hands.DropItem(putInInv: true);
			_player.Holding.SetHeldItem(null);
			AudioManager.PlayPlayerClip("UnequipItem", _player, variation: true, AudioDistance.VeryShort, 0.25f);
		}
	}

	private Item HideUnselectedItems(int selectedIndex)
	{
		Item result = null;
		foreach (KeyValuePair<byte, Item> item in _items)
		{
			if ((bool)item.Value)
			{
				if (item.Key == selectedIndex)
				{
					result = item.Value;
				}
				else if (item.Value.Holder == _player && item.Value.SyncedHolder == _player)
				{
					item.Value.PutInInventory();
				}
			}
		}
		return result;
	}

	public void ApplyCooknessInInventory(Item item)
	{
		foreach (InventorySlot availableSlot in _availableSlots)
		{
			if (availableSlot.Item == item)
			{
				availableSlot.ApplyCookness(item);
				UpdateInventoryCamera();
				break;
			}
		}
	}

	public void ApplySkinInInventory(Item item)
	{
		foreach (InventorySlot availableSlot in _availableSlots)
		{
			if (availableSlot.Item == item)
			{
				availableSlot.ApplySkin(item);
				UpdateInventoryCamera();
				break;
			}
		}
	}

	private void UpdateInventoryCamera()
	{
		if ((bool)_inventoryCamera.targetTexture)
		{
			bool fog = RenderSettings.fog;
			RenderSettings.fog = false;
			_inventoryCamera.Render();
			RenderSettings.fog = fog;
		}
	}

	public void UnlockExtraPocket(byte to)
	{
		if (base.IsServerInitialized && _extraSlots.Value < to)
		{
			_extraSlots.Value = to;
		}
	}

	public bool HasPocket(int index)
	{
		return index <= _extraSlots.Value;
	}

	private void OnInventoryAmountChange(byte prev, byte next, bool asServer)
	{
		if (!asServer)
		{
			if (base.Owner.IsLocalClient)
			{
				UpdateInventoryCamera();
				_itemSlots[GetTotalSlots() - 1].Initialize(unlocked: true);
			}
			AudioManager.PlayRandomPlayerClip("PickUp_V", 1, 3, _player, variation: false, AudioDistance.VeryShort, 0.25f);
		}
	}

	public void ServerSetCurBait(byte to)
	{
		if (base.IsServerInitialized)
		{
			_curBait.Value = to;
		}
	}

	public void ServerBoughtBait(byte index)
	{
		_ownedBaits[index - 1]++;
	}

	private void ServerLostBait(byte index)
	{
		_ownedBaits[index - 1]--;
	}

	private void BoughtBait()
	{
		AudioManager.PlayRandomGlobalClip("PickUp_V", 1, 3);
		PlayerUI.OnBaitChange(1, increased: true);
		_baitSlot.UpdateAmount(_ownedBaits[_curBait.Value]);
	}

	private void OnCurBaitChange(byte prev, byte next, bool asServer)
	{
		if (asServer)
		{
			return;
		}
		if (base.Owner.IsLocalClient)
		{
			BaitInfo bait = GameInfo.AllBaits[next];
			if (next == 0 && (bool)_player.Holding.HeldItem && (bool)_player.Holding.HeldItem.FishingRod)
			{
				bait = _player.Holding.HeldItem.FishingRod.GetCurBait();
			}
			_baitSlot.SetBait(bait, (next == 0) ? (-1) : _ownedBaits[next - 1]);
			UpdateInventoryCamera();
		}
		if ((bool)_player.Holding.HeldItem && (bool)_player.Holding.HeldItem.FishingRod)
		{
			ParticleManager.Play("Smoke", _player.Holding.HeldItem.FishingRod.Bait.transform.position);
			AudioManager.PlayClipAt("Poof", _player.Holding.HeldItem.FishingRod.Bait.transform.position, variation: true, AudioDistance.VeryShort, 0.25f);
			_player.Holding.HeldItem.FishingRod.Bait.SetBaitInfo(_player.Holding.HeldItem.FishingRod.GetCurBait());
		}
	}

	public void OnRodPickup(FishingRod rod)
	{
		if (_curBait.Value == 0)
		{
			_baitSlot.SetBait(rod.GetCurBait(), -1);
			UpdateInventoryCamera();
		}
	}

	public void OnRodDrop()
	{
		if (_curBait.Value == 0)
		{
			_baitSlot.SetBait(GameInfo.AllBaits[0], -1);
			UpdateInventoryCamera();
		}
	}

	private byte GetNextAvailableBait(bool prev = false)
	{
		byte b = ((!prev) ? ((byte)(_curBait.Value + 1)) : ((byte)(_curBait.Value - 1)));
		if (!prev)
		{
			for (int i = 0; i < GameInfo.AllBaits.Count; i++)
			{
				if (b >= GameInfo.AllBaits.Count)
				{
					return 0;
				}
				if (_ownedBaits[b - 1] > 0)
				{
					return b;
				}
				b++;
			}
		}
		else
		{
			for (int j = 0; j < GameInfo.AllBaits.Count; j++)
			{
				if (b >= GameInfo.AllBaits.Count)
				{
					b = (byte)(GameInfo.AllBaits.Count - 1);
				}
				if (b == 0)
				{
					return 0;
				}
				if (_ownedBaits[b - 1] > 0)
				{
					return b;
				}
				b--;
			}
		}
		return 0;
	}

	public void ServerOnBaitUsed()
	{
		if (_curBait.Value == 0)
		{
			return;
		}
		int i = _curBait.Value - 1;
		if (!(UnityEngine.Random.Range(0.001f, 100f) <= GameInfo.AllBaits[_curBait.Value].LostOnBaitChance))
		{
			return;
		}
		int num = Mathf.Clamp(_ownedBaits[i] - 1, 0, int.MaxValue);
		_ownedBaits[i] = num;
		if (num <= 0)
		{
			byte nextAvailableBait = GetNextAvailableBait(prev: true);
			if (_curBait.Value != nextAvailableBait)
			{
				ServerSetCurBait(nextAvailableBait);
			}
		}
	}

	public void ServerDropAll(Vector3 pos, Quaternion rot)
	{
		if (!base.IsServerInitialized)
		{
			return;
		}
		Item heldItem = _player.Holding.HeldItem;
		if ((bool)heldItem)
		{
			heldItem.Drop(calledFromLocal: true);
		}
		List<Item> list = new List<Item>();
		foreach (KeyValuePair<byte, Item> item in _items)
		{
			if ((bool)item.Value)
			{
				list.Add(item.Value);
			}
		}
		Vector3 zero = Vector3.zero;
		foreach (Item item2 in list)
		{
			if ((bool)item2)
			{
				zero += Vector3.up * item2.ModelHeight;
				RemoveItem(item2);
				item2.SetSyncedHolder(null);
				if (!item2.RigidbodySync.IsSimulatedLocal)
				{
					item2.RigidbodySync.StartSimulateLocal(pos + zero, rot);
				}
				else
				{
					item2.transform.position = pos + zero;
					item2.Rig.linearVelocity = Vector3.zero;
					item2.Rig.angularVelocity = Vector3.zero;
				}
				zero += Vector3.up * item2.ModelHeight;
			}
		}
	}

	private void OnOwnedBaitChange(SyncListOperation op, int index, int oldAmount, int newAmount, bool asServer)
	{
		if (index != 0 && op != SyncListOperation.Add && oldAmount != newAmount)
		{
			if (newAmount > oldAmount)
			{
				AudioManager.PlayRandomPlayerClip("PickUp_V", 1, 3, _player, variation: true, AudioDistance.Short);
			}
			else
			{
				AudioManager.PlayPlayerClip("FishingRodBaitSnap_WithOutTension", _player, variation: true, AudioDistance.Short);
			}
			if (base.Owner.IsLocalClient && (!base.IsServerInitialized || asServer))
			{
				PlayerUI.OnBaitChange(1, newAmount > oldAmount);
				_baitSlot.UpdateAmount(newAmount);
			}
		}
	}

	public void SetSavedPlayer(SavedPlayer savedPlayer)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			_toLoadFrom = savedPlayer;
		}
	}

	private void LoadFromSave()
	{
		_extraSlots.Value = _toLoadFrom.ExtraSlots;
		foreach (SavedItem inventoryItem in _toLoadFrom.InventoryItems)
		{
			Item spawnable = GameInfo.GetSpawnable(inventoryItem.ItemID);
			if (!spawnable)
			{
				return;
			}
			Item item = UnityEngine.Object.Instantiate(spawnable, SpawnManager.PlayerSpawnPos, Quaternion.identity);
			item.SetSyncedHolder(_player, forced: true);
			item.LoadFromSave(inventoryItem);
			if ((bool)item.Creature)
			{
				item.Creature.ServerKillOnSpawn();
				if (inventoryItem.IsDripCreature)
				{
					item.Creature.SetDrip();
				}
			}
			Spawn(item.gameObject);
			_items[inventoryItem.InventorySlot] = item;
		}
		if (_toLoadFrom.HeldItem.Exists)
		{
			Item spawnable2 = GameInfo.GetSpawnable(_toLoadFrom.HeldItem.ItemID);
			if (!spawnable2)
			{
				return;
			}
			Item item2 = UnityEngine.Object.Instantiate(spawnable2, SpawnManager.PlayerSpawnPos, Quaternion.identity);
			item2.SetSyncedHolder(_player, forced: true);
			item2.LoadFromSave(_toLoadFrom.HeldItem);
			if ((bool)item2.Creature)
			{
				item2.Creature.ServerKillOnSpawn();
				if (_toLoadFrom.HeldItem.IsDripCreature)
				{
					item2.Creature.SetDrip();
				}
			}
			Spawn(item2.gameObject);
		}
		for (int i = 0; i < _toLoadFrom.OwnedBaits.Count; i++)
		{
			_ownedBaits[i] = _toLoadFrom.OwnedBaits[i];
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerInventoryAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerInventoryAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_ownedBaits.InitializeEarly(this, 4u, isSyncObject: true);
			_extraSlots.InitializeEarly(this, 3u, isSyncObject: false);
			_curBait.InitializeEarly(this, 2u, isSyncObject: false);
			_syncedCurSlot.InitializeEarly(this, 1u, isSyncObject: false);
			_items.InitializeEarly(this, 0u, isSyncObject: true);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerInventoryAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerInventoryAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_ownedBaits.InitializeLate();
			_extraSlots.InitializeLate();
			_curBait.InitializeLate();
			_syncedCurSlot.InitializeLate();
			_items.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void Awake_UserLogic_PlayerInventory_Assembly_002DCSharp_002Edll()
	{
		_items.OnChange += OnItemsChange;
		_syncedCurSlot.OnChange += OnCurSlotChange;
		_extraSlots.OnChange += OnInventoryAmountChange;
		_inventoryCamera.clearFlags = CameraClearFlags.Color;
		_inventoryCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		_inventoryCamera.enabled = false;
		_ownedBaits.OnChange += OnOwnedBaitChange;
	}
}
