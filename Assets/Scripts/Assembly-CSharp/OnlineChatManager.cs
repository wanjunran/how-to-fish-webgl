using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using Steamworks;

public class OnlineChatManager : NetworkBehaviour
{
	public static OnlineChatManager Instance;

	private bool NetworkInitialize___EarlyOnlineChatManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateOnlineChatManagerAssembly_002DCSharp_002Edll_Excuted;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_OnlineChatManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	[ObserversRpc]
	public void SendChatMessage(ulong from, string message)
	{
		RpcWriter___SendChatMessage___3264264606(from, message);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyOnlineChatManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyOnlineChatManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(0u, RpcReader___SendChatMessage___3264264606);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateOnlineChatManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateOnlineChatManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___SendChatMessage___3264264606(ulong from, string message)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt64(from);
		pooledWriter.WriteString(message);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___SendChatMessage___3264264606(ulong P_0, string P_1)
	{
		ChatManager.ChatMessage(new CSteamID(P_0), P_1);
	}

	private void RpcReader___SendChatMessage___3264264606(PooledReader PooledReader0, Channel channel)
	{
		ulong num = PooledReader0.ReadUInt64();
		string text = PooledReader0.ReadStringAllocated();
		if (base.IsClientInitialized)
		{
			RpcLogic___SendChatMessage___3264264606(num, text);
		}
	}

	private void Awake_UserLogic_OnlineChatManager_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
	}
}
