using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class ItemManager : NetworkBehaviour
{
	public static ItemManager Instance;

	[SerializeField]
	private float _itemFloatSmoothness = 2f;

	[SerializeField]
	private float _removeDistOnIslandLoading = 50f;

	private static HashSet<Item> _itemsForBirds = new HashSet<Item>();

	private static bool _allItemsActive = true;

	private static HashSet<Item> _itemsUnderwater = new HashSet<Item>();

	private bool NetworkInitialize___EarlyItemManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateItemManagerAssembly_002DCSharp_002Edll_Excuted;

	public static Dictionary<Transform, Item> Items { get; } = new Dictionary<Transform, Item>();

	private static Dictionary<Collider, Item> _colToItem { get; } = new Dictionary<Collider, Item>();

	private void FixedUpdate()
	{
		CheckUnderwaterItems();
		FloatItems();
	}

	public Item SpawnNewItem(Item toSpawn, Vector3 pos, Quaternion rot)
	{
		Item item = Object.Instantiate(toSpawn, pos, rot, Server.Instance.DynamicObjectsHolder);
		Spawn(item.gameObject);
		return item;
	}

	public override void OnStartClient()
	{
		Setter.SetSingleInstance(ref Instance, this);
	}

	public override void OnStopClient()
	{
		Items.Clear();
	}

	public override void OnStartServer()
	{
		base.TimeManager.OnTick += ServerTickUpdate;
	}

	public override void OnStopServer()
	{
		base.TimeManager.OnTick -= ServerTickUpdate;
	}

	public static void ToggleAllItems(bool to)
	{
		_allItemsActive = to;
		foreach (KeyValuePair<Transform, Item> item in Items)
		{
			if ((bool)item.Value && !item.Value.IsDeinitializing)
			{
				item.Value.gameObject.SetActive(to);
			}
		}
	}

	private void ServerTickUpdate()
	{
		CheckHolderDisconnect();
	}

	private void CheckUnderwaterItems()
	{
		RemoveUnderwaterItems();
		foreach (KeyValuePair<Transform, Item> item in Items)
		{
			if (!item.Key || item.Key.position.y >= WaterManager.WaterHeight + 1f)
			{
				continue;
			}
			bool flag = ((bool)item.Value.Creature && !item.Value.Creature.IsDead) || !item.Value.IsClientInitialized;
			if (((bool)item.Value.Holder | flag) || item.Value.RigidbodySync.OnBoat)
			{
				if (_itemsUnderwater.Contains(item.Value))
				{
					_itemsUnderwater.Remove(item.Value);
				}
				continue;
			}
			FloatInfo[] floatInfos = item.Value.FloatInfos;
			for (int i = 0; i < floatInfos.Length; i++)
			{
				if (WaterManager.GetWaterInfo(floatInfos[i].Pos).Key)
				{
					bool num = DazedUtils.TryMoveItemFromUnderLevel(item.Value);
					bool onBoat = item.Value.RigidbodySync.OnBoat;
					if (!num && !onBoat)
					{
						AddUnderwaterItem(item.Value);
					}
				}
			}
		}
	}

	private void RemoveItems()
	{
		List<Transform> list = new List<Transform>();
		List<Collider> list2 = new List<Collider>();
		foreach (KeyValuePair<Transform, Item> item in Items)
		{
			if (!item.Key)
			{
				list.Add(item.Key);
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (Items.ContainsKey(list[num]))
			{
				Items.Remove(list[num]);
			}
			foreach (KeyValuePair<Collider, Item> item2 in _colToItem)
			{
				if (!item2.Key || item2.Value.transform == list[num])
				{
					list2.Add(item2.Key);
				}
			}
		}
		foreach (Collider item3 in list2)
		{
			_colToItem.Remove(item3);
		}
		_itemsForBirds.RemoveWhere((Item item) => !item);
	}

	private void RemoveUnderwaterItems()
	{
		HashSet<Item> hashSet = new HashSet<Item>();
		foreach (Item item in _itemsUnderwater)
		{
			if (!item || (bool)item.Holder)
			{
				hashSet.Add(item);
			}
		}
		foreach (Item item2 in hashSet)
		{
			_itemsUnderwater.Remove(item2);
		}
	}

	private void AddUnderwaterItem(Item item)
	{
		if (!_itemsUnderwater.Contains(item))
		{
			_itemsUnderwater.Add(item);
			if ((bool)item.LastHolder)
			{
				AudioManager.PlayRandomClipAt(item.WaterCollisionAudio switch
				{
					WaterCollisionAudio.Light => "ItemHitWaterLight_V", 
					WaterCollisionAudio.Medium => "ItemHitWaterMedium_V", 
					WaterCollisionAudio.Heavy => "ItemHitWaterHeavy_V", 
					_ => "ItemHitWaterMedium_V", 
				}, 1, 3, item.transform.position, variation: true);
				VFXManager.Play("WaterSplash", item.transform.position, Vector3.zero);
			}
		}
	}

	private void CheckHolderDisconnect()
	{
		foreach (KeyValuePair<Transform, Item> item in Items)
		{
			if ((bool)item.Value.SyncedHolder && !item.Value.SyncedHolder.Owner.IsActive)
			{
				item.Value.SetSyncedHolder(null);
			}
		}
	}

	private void FloatItems()
	{
		foreach (KeyValuePair<Transform, Item> item in Items)
		{
			item.Value.SetUnderWater(isUnderWater: false);
		}
		foreach (Item item2 in _itemsUnderwater)
		{
			if (!item2 || (bool)item2.Holder)
			{
				continue;
			}
			bool flag = !base.IsServerInitialized && !item2.RigidbodySync.IsSimulatedLocal;
			bool flag2 = base.IsServerInitialized && item2.RigidbodySync.IsSimulatedLocal;
			if (item2.RigidbodySync.IsFloating && (flag | flag2) && item2.Rig.isKinematic)
			{
				FloatKinematicItem(item2);
			}
			else
			{
				if (!item2.RigidbodySync.IsSimulatedLocal)
				{
					continue;
				}
				bool underWater = false;
				float num = 1f / (float)item2.FloatInfos.Length;
				FloatInfo[] floatInfos = item2.FloatInfos;
				foreach (FloatInfo floatInfo in floatInfos)
				{
					KeyValuePair<bool, float> waterInfo = WaterManager.GetWaterInfo(floatInfo.Pos);
					if (!waterInfo.Key || TryStartFloating(item2))
					{
						continue;
					}
					Rigidbody rigidbody = (floatInfo.Rig ? floatInfo.Rig : item2.Rig);
					if (!rigidbody.isKinematic)
					{
						Vector3 linearVelocity = rigidbody.linearVelocity;
						float num2 = Mathf.Clamp(waterInfo.Value, WaterManager.ItemMinForce, float.MaxValue) * WaterManager.ItemBouyancy * item2.Buoyancy * Time.fixedDeltaTime;
						linearVelocity.y = Mathf.MoveTowards(linearVelocity.y, WaterManager.ItemWaterMaxVelocity * item2.Buoyancy, num2 * num);
						rigidbody.linearVelocity = linearVelocity;
						Vector3 toDirection = (item2.Creature ? Vector3.down : Vector3.up);
						Quaternion target = Quaternion.FromToRotation(rigidbody.transform.up, toDirection) * rigidbody.rotation;
						Vector3 angularVelocityToTarget = DazedUtils.GetAngularVelocityToTarget(rigidbody.rotation, target);
						angularVelocityToTarget *= WaterManager.ItemAngularVelocty * num;
						if (!item2.IsColliding)
						{
							rigidbody.AddTorque(angularVelocityToTarget);
						}
						underWater = true;
					}
				}
				item2.SetUnderWater(underWater);
			}
		}
	}

	private void FloatKinematicItem(Item item)
	{
		float num = WaterManager.GetWaterHeight(item.transform.position) - 0.15f;
		Vector3 position = item.Rig.position;
		float num2 = num - position.y;
		position.y = num;
		if (base.IsServerInitialized && num2 < 0f && Physics.Raycast(item.Rig.position, Vector3.down, out var _, item.ModelHeight, GameInfo.LevelLayer))
		{
			item.RigidbodySync.StartSimulateLocal();
			return;
		}
		Vector3 position2 = Vector3.Lerp(item.Rig.position, position, _itemFloatSmoothness * Time.fixedDeltaTime);
		item.Rig.MovePosition(position2);
	}

	private bool TryStartFloating(Item item)
	{
		if (base.IsServerInitialized && item.RigidbodySync.IsSimulatedLocal && !item.RigidbodySync.IsSendingUpdates && item.RigidbodySync.IsStationary && !item.RigidbodySync.RecentlyStartedSimulation && (!item.Creature || item.Creature.IsDead))
		{
			if (!item.RigidbodySync.IsFloating && !item.RigidbodySync.OnBoat)
			{
				item.RigidbodySync.ServerSetIsFloating(to: true);
			}
			return true;
		}
		return false;
	}

	public void ClearDictionary()
	{
		if (base.IsServerStarted)
		{
			foreach (KeyValuePair<Transform, Item> item in Items)
			{
				if ((bool)item.Key && !item.Value.IsDeinitializing)
				{
					Despawn(item.Key.gameObject);
				}
			}
		}
		Items.Clear();
	}

	public static void Add(Item item, Collider pickupCol, Collider[] cols)
	{
		_colToItem.TryAdd(pickupCol, item);
		foreach (Collider collider in cols)
		{
			Items.TryAdd(collider.transform, item);
			_colToItem.TryAdd(collider, item);
		}
		if (!Items.ContainsKey(item.transform))
		{
			Items.TryAdd(item.transform, item);
			if (!_allItemsActive && !InstanceFinder.IsServerStarted)
			{
				item.gameObject.SetActive(value: false);
			}
		}
		if (!item.IgnoredBySeagulls)
		{
			_itemsForBirds.Add(item);
		}
	}

	public static void Remove(Item toRemove, Collider pickupCol, Collider[] cols)
	{
		if (!toRemove || !toRemove.transform)
		{
			return;
		}
		if (_colToItem.ContainsKey(pickupCol))
		{
			_colToItem.Remove(pickupCol);
		}
		foreach (Collider key in cols)
		{
			if (_colToItem.ContainsKey(key))
			{
				_colToItem.Remove(key);
			}
		}
		if (Items.ContainsKey(toRemove.transform))
		{
			Items.Remove(toRemove.transform);
		}
		if (!toRemove.IgnoredBySeagulls && _itemsForBirds.Contains(toRemove))
		{
			_itemsForBirds.Remove(toRemove);
		}
	}

	public static Item Get(Transform tran)
	{
		if (!Contains(tran))
		{
			return null;
		}
		return Items[tran];
	}

	public static Item Get(Collision col)
	{
		if (!col.rigidbody || !col.rigidbody.transform)
		{
			return null;
		}
		if (!Contains(col.rigidbody.transform))
		{
			return null;
		}
		return Items[col.rigidbody.transform];
	}

	public static Item Get(Collider col)
	{
		Item item = _colToItem.GetValueOrDefault(col);
		if (!item && (bool)col.attachedRigidbody && Contains(col.attachedRigidbody.transform))
		{
			item = Items[col.attachedRigidbody.transform];
		}
		return item;
	}

	private static bool Contains(Transform transform)
	{
		return Items.ContainsKey(transform);
	}

	public static Item AvailableItemForBird(Bird bird)
	{
		Item result = null;
		float num = float.MaxValue;
		foreach (Item itemsForBird in _itemsForBirds)
		{
			if ((bool)itemsForBird.Holder || itemsForBird.IsDeinitializing || (bool)itemsForBird.BirdHolder || (bool)itemsForBird.Bird || itemsForBird.IgnoredBySeagulls || ((bool)itemsForBird.Creature && !itemsForBird.Creature.IsDead) || !itemsForBird.IsInteractable)
			{
				continue;
			}
			Vector3 direction = itemsForBird.transform.position - bird.transform.position;
			if (!Physics.Raycast(bird.transform.position, direction, Mathf.Clamp(direction.magnitude - 0.15f, 0f, direction.magnitude), GameInfo.LevelLayer))
			{
				float num2 = Vector3.Distance(bird.transform.position, itemsForBird.transform.position);
				if (num2 < num)
				{
					num = num2;
					result = itemsForBird;
				}
			}
		}
		return result;
	}

	public static void OnIslandUnloaded()
	{
		if (!Instance || !Player.LocalPlayer)
		{
			return;
		}
		Vector3 position = Player.LocalPlayer.Transform.position;
		List<Item> list = new List<Item>();
		float num = Instance._removeDistOnIslandLoading * Instance._removeDistOnIslandLoading;
		foreach (KeyValuePair<Transform, Item> item in Items)
		{
			if ((bool)item.Value && !item.Value.IsDestroying && !item.Value.IsDeinitializing && !item.Value.SyncedHolder && !item.Value.BirdHolder && (!item.Value.Bird || item.Value.Bird.IsDead) && !item.Value.DeadPlayer && (position - item.Key.transform.position).sqrMagnitude > num)
			{
				list.Add(item.Value);
			}
		}
		Instance.StartCoroutine(Instance.RemoveItemsFarAwar(list));
	}

	private IEnumerator RemoveItemsFarAwar(List<Item> toRemove)
	{
		int batchSize = 5;
		int curBatch = batchSize;
		for (int i = toRemove.Count - 1; i >= 0; i--)
		{
			if (curBatch >= batchSize)
			{
				curBatch = 0;
				yield return null;
			}
			if ((bool)toRemove[i] && (bool)toRemove[i].gameObject && !toRemove[i].IsDeinitializing)
			{
				Despawn(toRemove[i].gameObject);
			}
			curBatch++;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyItemManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyItemManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateItemManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateItemManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}
}
