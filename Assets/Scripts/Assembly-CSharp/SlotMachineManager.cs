using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

public class SlotMachineManager : NetworkBehaviour
{
	private static SlotMachineManager _instance;

	private static Item _cheatItem;

	private static byte _cheatSkinIndex = byte.MaxValue;

	private bool NetworkInitialize___EarlySlotMachineManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateSlotMachineManagerAssembly_002DCSharp_002Edll_Excuted;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_SlotMachineManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public static void SetCheatSkin(Item item, byte skinIndex)
	{
		_cheatItem = item;
		_cheatSkinIndex = skinIndex;
	}

	public static void RollRandom(Player roller)
	{
		if (!_instance || !_instance.IsServerInitialized || SlotMachine.AvailableItems == null)
		{
			return;
		}
		Rarity rarity = Rarity.Common;
		if (_cheatSkinIndex != byte.MaxValue)
		{
			rarity = ((!_cheatItem) ? BoatManager.Boat.SkinPreset.Skins[_cheatSkinIndex].Rarity : _cheatItem.SkinPreset.Skins[_cheatSkinIndex].Rarity);
		}
		List<Item> list = new List<Item>(SlotMachine.AvailableItems);
		if (!SlotMachine.ExcludeBoat)
		{
			list.Add(null);
		}
		List<Item> list2 = new List<Item>();
		List<int> list3 = new List<int>();
		byte[] array = new byte[10];
		byte[] array2 = new byte[10];
		byte rolled = (byte)Random.Range(0, 10);
		for (int i = 0; i < array.Length; i++)
		{
			list2.Add(list[Random.Range(0, list.Count)]);
			list3.Add(i);
		}
		for (int j = 0; j < array2.Length; j++)
		{
			SkinPreset skinPreset = (list2[j] ? list2[j].SkinPreset : BoatManager.Boat.SkinPreset);
			array2[j] = skinPreset.GetRandomSkinIndex(Rarity.Common);
		}
		if (_cheatSkinIndex != byte.MaxValue && rarity == Rarity.Common)
		{
			list2[0] = _cheatItem;
			array2[0] = _cheatSkinIndex;
			list3.Remove(0);
			rolled = 0;
		}
		int num = list3[Random.Range(0, list3.Count)];
		list3.Remove(num);
		SkinPreset skinPreset2 = (list2[num] ? list2[num].SkinPreset : BoatManager.Boat.SkinPreset);
		array2[num] = skinPreset2.GetRandomSkinIndex(Rarity.Rare);
		int num2 = list3[Random.Range(0, list3.Count)];
		list3.Remove(num2);
		SkinPreset skinPreset3 = (list2[num2] ? list2[num2].SkinPreset : BoatManager.Boat.SkinPreset);
		array2[num2] = skinPreset3.GetRandomSkinIndex(Rarity.Rare);
		int num3 = list3[Random.Range(0, list3.Count)];
		list3.Remove(num3);
		SkinPreset skinPreset4 = (list2[num3] ? list2[num3].SkinPreset : BoatManager.Boat.SkinPreset);
		array2[num3] = skinPreset4.GetRandomSkinIndex(Rarity.Legendary);
		if (_cheatSkinIndex != byte.MaxValue)
		{
			switch (rarity)
			{
			case Rarity.Rare:
				list2[num] = _cheatItem;
				array2[num] = _cheatSkinIndex;
				rolled = (byte)num;
				break;
			case Rarity.Legendary:
				list2[num3] = _cheatItem;
				array2[num3] = _cheatSkinIndex;
				rolled = (byte)num3;
				break;
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = (list2[k] ? list2[k].ID : byte.MaxValue);
		}
		_instance.SendRoll(roller, array, array2, rolled);
		SlotMachine.Roll(roller, array, array2, rolled);
	}

	[ObserversRpc(ExcludeServer = true)]
	private void SendRoll(Player roller, byte[] itemIDs, byte[] itemSkins, byte rolled)
	{
		RpcWriter___SendRoll___1775394313(roller, itemIDs, itemSkins, rolled);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlySlotMachineManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlySlotMachineManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(0u, RpcReader___SendRoll___1775394313);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateSlotMachineManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateSlotMachineManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___SendRoll___1775394313(Player roller, byte[] itemIDs, byte[] itemSkins, byte rolled)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GeneratedWriters___Internal.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, roller);
		GeneratedWriters___Internal.GWrite___System_002EByte_005B_005DFishNet_002ESerializing_002EGenerated(pooledWriter, itemIDs);
		GeneratedWriters___Internal.GWrite___System_002EByte_005B_005DFishNet_002ESerializing_002EGenerated(pooledWriter, itemSkins);
		pooledWriter.WriteUInt8Unpacked(rolled);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___SendRoll___1775394313(Player P_0, byte[] P_1, byte[] P_2, byte P_3)
	{
		SlotMachine.Roll(P_0, P_1, P_2, P_3);
	}

	private void RpcReader___SendRoll___1775394313(PooledReader PooledReader0, Channel channel)
	{
		Player player = GeneratedReaders___Internal.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte[] array = PooledReader0.ReadUInt8ArrayAndSizeAllocated();
		byte[] array2 = PooledReader0.ReadUInt8ArrayAndSizeAllocated();
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsClientInitialized)
		{
			RpcLogic___SendRoll___1775394313(player, array, array2, b);
		}
	}

	private void Awake_UserLogic_SlotMachineManager_Assembly_002DCSharp_002Edll()
	{
		_instance = this;
	}
}
