using System;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

public class MoneyManager : NetworkBehaviour
{
	public static MoneyManager Instance;

	public readonly SyncVar<int> _money = new SyncVar<int>();

	private const int _cheatMoney = 99999;

	private bool NetworkInitialize___EarlyMoneyManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateMoneyManagerAssembly_002DCSharp_002Edll_Excuted;

	public static int Money { get; private set; }

	public static event Action OnItemSold;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_MoneyManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		_money.Value = ((SaveManager.CurServerSave != null) ? SaveManager.CurServerSave.Money : (ClientSettings.CheatsEnabled ? 99999 : 0));
	}

	public override void OnStartClient()
	{
		Money = _money.Value;
	}

	public override void OnStopClient()
	{
		Money = 0;
	}

	private void Update()
	{
		if (ClientSettings.CheatsEnabled && (bool)Player.LocalPlayer && !Player.LocalPlayer.BlockInputs)
		{
			if (Input.GetKeyDown(KeyCode.M))
			{
				AddMoney(99999, Player.LocalPlayer);
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				RemoveMoney(99999, Player.LocalPlayer);
			}
		}
	}

	public static void AddMoney(int amount, Player player)
	{
		if ((bool)Instance && Instance.IsServerInitialized)
		{
			Instance._money.Value += Mathf.Abs(amount);
			Instance.ObserverMoneySound(increase: true, player);
			Instance.MoneySound(increase: true, player);
		}
	}

	public static void SellItem(Item item)
	{
		if ((bool)Instance && Instance.IsServerInitialized)
		{
			Instance._money.Value += item.TotalWorth;
			Instance.ObserverMoneySound(increase: true, item.LastHolder);
			Instance.MoneySound(increase: true, item.LastHolder);
		}
	}

	public static void RemoveMoney(int amount, Player player)
	{
		if (Instance.IsServerInitialized)
		{
			int value = Mathf.Clamp(Instance._money.Value - Mathf.Abs(amount), 0, int.MaxValue);
			Instance._money.Value = value;
			Instance.ObserverMoneySound(increase: false, player);
			Instance.MoneySound(increase: false, player);
		}
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverMoneySound(bool increase, Player player)
	{
		RpcWriter___ObserverMoneySound___4168932475(increase, player);
	}

	private void MoneySound(bool increase, Player player)
	{
		if (increase)
		{
			AudioManager.PlayRandomPlayerClip("Sell_0", 1, 3, player, variation: false, AudioDistance.Long, 0.35f);
		}
		else
		{
			AudioManager.PlayRandomPlayerClip("Buy_0", 1, 3, player, variation: false, AudioDistance.Long, 0.35f);
		}
	}

	public static bool CanAfford(int cost)
	{
		return cost <= Money;
	}

	private void OnChangeMoney(int prev, int next, bool asServer)
	{
		if (!asServer)
		{
			OnItemSold?.Invoke();
			int num = next - Money;
			PlayerUI.SetMoney(next, Mathf.Abs(num), num > 0);
			Money = next;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyMoneyManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyMoneyManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_money.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterObserversRpc(0u, RpcReader___ObserverMoneySound___4168932475);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateMoneyManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateMoneyManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_money.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverMoneySound___4168932475(bool increase, Player player)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteBoolean(increase);
		GeneratedWriters___Internal.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverMoneySound___4168932475(bool P_0, Player P_1)
	{
		MoneySound(P_0, P_1);
	}

	private void RpcReader___ObserverMoneySound___4168932475(PooledReader PooledReader0, Channel channel)
	{
		bool flag = PooledReader0.ReadBoolean();
		Player player = GeneratedReaders___Internal.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverMoneySound___4168932475(flag, player);
		}
	}

	private void Awake_UserLogic_MoneyManager_Assembly_002DCSharp_002Edll()
	{
		Setter.SetSingleInstance(ref Instance, this);
		_money.OnChange += OnChangeMoney;
	}
}
