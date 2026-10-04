using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

public class CasinoManager : NetworkBehaviour
{
	public static CasinoManager Instance;

	public readonly SyncVar<int> _totalWorth = new SyncVar<int>();

	private static List<Item> _itemsToBet = new List<Item>();

	private static BetColor _curBetColor;

	private bool NetworkInitialize___EarlyCasinoManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateCasinoManagerAssembly_002DCSharp_002Edll_Excuted;

	public int TotalWorth => _totalWorth.Value;

	public static bool HasPlacedBet => Instance._totalWorth.Value > 0;

	public static bool IsBetting { get; private set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_CasinoManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStopClient()
	{
		_itemsToBet.Clear();
	}

	public static void SetBetItems(List<Item> allItems)
	{
		_itemsToBet = allItems;
		CalculateWorth();
	}

	private static void CalculateWorth(bool won = false)
	{
		int num = 0;
		foreach (Item item in _itemsToBet)
		{
			num += item.TotalWorth;
		}
		if (num != Instance._totalWorth.Value)
		{
			Instance._totalWorth.Value = num;
			Instance.UpdateTotalWorth(num, won);
		}
	}

	public void ServerStartBet(BetColor chosenColor)
	{
		if (!base.IsServerInitialized)
		{
			return;
		}
		IsBetting = true;
		foreach (Item item in _itemsToBet)
		{
			item.ToggleInteractable(to: false);
		}
		LocalCasino.Instance.ServerStartRoulette();
		_curBetColor = chosenColor;
		StartBetEffects((byte)chosenColor);
	}

	[ObserversRpc]
	private void StartBetEffects(byte betColorByte)
	{
		RpcWriter___StartBetEffects___1246646286(betColorByte);
	}

	public void ServerRouletteResult(BetColor winColor)
	{
		bool flag = winColor == _curBetColor;
		int num = 0;
		if (flag)
		{
			switch (winColor)
			{
			case BetColor.Black:
				num = 2;
				break;
			case BetColor.Red:
				num = 2;
				break;
			case BetColor.Green:
				num = 35;
				break;
			}
		}
		foreach (Item item in _itemsToBet)
		{
			if (flag)
			{
				item.ToggleInteractable(to: true);
				item.AddBetMultiplier(num);
			}
			else
			{
				item.DestroyItem(0);
			}
		}
		CalculateWorth(flag);
		BetResultEffects((byte)_curBetColor, flag);
		IsBetting = false;
	}

	[ObserversRpc]
	private void BetResultEffects(byte betColor, bool won)
	{
		RpcWriter___BetResultEffects___2774100791(betColor, won);
	}

	[ObserversRpc]
	private void UpdateTotalWorth(int to, bool won)
	{
		RpcWriter___UpdateTotalWorth___3658436649(to, won);
	}

	[ObserversRpc(ExcludeServer = true)]
	public void UpdateGameObjects(Vector3 ballPos, float wheelRot, Channel channel = Channel.Unreliable)
	{
		RpcWriter___UpdateGameObjects___2328420242(ballPos, wheelRot, channel);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyCasinoManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyCasinoManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_totalWorth.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterObserversRpc(0u, RpcReader___StartBetEffects___1246646286);
			RegisterObserversRpc(1u, RpcReader___BetResultEffects___2774100791);
			RegisterObserversRpc(2u, RpcReader___UpdateTotalWorth___3658436649);
			RegisterObserversRpc(3u, RpcReader___UpdateGameObjects___2328420242);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateCasinoManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateCasinoManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_totalWorth.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___StartBetEffects___1246646286(byte betColorByte)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(betColorByte);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___StartBetEffects___1246646286(byte P_0)
	{
		BetButton.ToggleSpecific((BetColor)P_0);
		LocalCasino.Instance.OnRouletteStart((BetColor)P_0);
	}

	private void RpcReader___StartBetEffects___1246646286(PooledReader PooledReader0, Channel channel)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsClientInitialized)
		{
			RpcLogic___StartBetEffects___1246646286(b);
		}
	}

	private void RpcWriter___BetResultEffects___2774100791(byte betColor, bool won)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(betColor);
		pooledWriter.WriteBoolean(won);
		SendObserversRpc(1u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___BetResultEffects___2774100791(byte P_0, bool P_1)
	{
		BetButton.ToggleAll(P_1);
		LocalCasino.Instance.OnRouletteEnd((BetColor)P_0, P_1);
	}

	private void RpcReader___BetResultEffects___2774100791(PooledReader PooledReader0, Channel channel)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		bool flag = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized)
		{
			RpcLogic___BetResultEffects___2774100791(b, flag);
		}
	}

	private void RpcWriter___UpdateTotalWorth___3658436649(int to, bool won)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteInt32(to);
		pooledWriter.WriteBoolean(won);
		SendObserversRpc(2u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___UpdateTotalWorth___3658436649(int P_0, bool P_1)
	{
		LocalCasino.Instance.SetTotalWorthText(P_0, P_1);
	}

	private void RpcReader___UpdateTotalWorth___3658436649(PooledReader PooledReader0, Channel channel)
	{
		int num = PooledReader0.ReadInt32();
		bool flag = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized)
		{
			RpcLogic___UpdateTotalWorth___3658436649(num, flag);
		}
	}

	private void RpcWriter___UpdateGameObjects___2328420242(Vector3 ballPos, float wheelRot, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(ballPos);
		pooledWriter.WriteSingle(wheelRot);
		SendObserversRpc(3u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdateGameObjects___2328420242(Vector3 P_0, float P_1, Channel P_2)
	{
		if ((bool)LocalCasino.Instance)
		{
			LocalCasino.Instance.ObserverUpdateObjects(P_0, P_1);
		}
	}

	private void RpcReader___UpdateGameObjects___2328420242(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		float num = PooledReader0.ReadSingle();
		if (base.IsClientInitialized)
		{
			RpcLogic___UpdateGameObjects___2328420242(vector, num, channel);
		}
	}

	private void Awake_UserLogic_CasinoManager_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
	}
}
