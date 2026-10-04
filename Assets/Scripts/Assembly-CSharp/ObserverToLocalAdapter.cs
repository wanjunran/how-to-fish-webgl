using System.Collections;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

public class ObserverToLocalAdapter : NetworkBehaviour
{
	private bool NetworkInitialize___EarlyObserverToLocalAdapterAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateObserverToLocalAdapterAssembly_002DCSharp_002Edll_Excuted;

	public static ObserverToLocalAdapter Instance { get; private set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ObserverToLocalAdapter_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	[ObserversRpc]
	public void PlayImpactSound(Item item, byte vel, Channel channel = Channel.Unreliable)
	{
		RpcWriter___PlayImpactSound___413717140(item, vel, channel);
	}

	[ObserversRpc]
	public void EruptVolcano(int delay)
	{
		RpcWriter___EruptVolcano___3316948804(delay);
	}

	private IEnumerator VolcanoEruption(int delay)
	{
		Player.LocalPlayer.ScreenShake.ConstantShake(enabled: true);
		AudioManager.PlayGlobalClip("EarthRumble", variation: false, 0.5f);
		yield return new WaitForSeconds(delay);
		AudioManager.PlayGlobalClip("LavaEruption_01");
		Player.LocalPlayer.ScreenShake.ConstantShake(enabled: false);
		VFXManager.Play("VolcanoEruption", MainLava.LavaPosition);
		Player.LocalPlayer.ScreenShake.ShakeAt(MainLava.LavaPosition, 1, 2500f, 50f, 150f);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyObserverToLocalAdapterAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyObserverToLocalAdapterAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(0u, RpcReader___PlayImpactSound___413717140);
			RegisterObserversRpc(1u, RpcReader___EruptVolcano___3316948804);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateObserverToLocalAdapterAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateObserverToLocalAdapterAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___PlayImpactSound___413717140(Item item, byte vel, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GeneratedWriters___Internal.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		pooledWriter.WriteUInt8Unpacked(vel);
		SendObserversRpc(0u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___PlayImpactSound___413717140(Item P_0, byte P_1, Channel P_2)
	{
		AudioImpactManager.PlayReceivedImpactSound(P_0, P_1);
	}

	private void RpcReader___PlayImpactSound___413717140(PooledReader PooledReader0, Channel channel)
	{
		Item item = GeneratedReaders___Internal.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsClientInitialized)
		{
			RpcLogic___PlayImpactSound___413717140(item, b, channel);
		}
	}

	private void RpcWriter___EruptVolcano___3316948804(int delay)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteInt32(delay);
		SendObserversRpc(1u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___EruptVolcano___3316948804(int P_0)
	{
		StartCoroutine(VolcanoEruption(P_0));
	}

	private void RpcReader___EruptVolcano___3316948804(PooledReader PooledReader0, Channel channel)
	{
		int num = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___EruptVolcano___3316948804(num);
		}
	}

	private void Awake_UserLogic_ObserverToLocalAdapter_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
	}
}
