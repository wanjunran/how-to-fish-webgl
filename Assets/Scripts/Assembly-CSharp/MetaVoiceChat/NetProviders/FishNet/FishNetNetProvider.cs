using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using MetaVoiceChat.Utils;
using UnityEngine;

namespace MetaVoiceChat.NetProviders.FishNet
{
	[RequireComponent(typeof(MetaVc))]
	public class FishNetNetProvider : NetworkBehaviour, INetProvider
	{
		private static readonly List<FishNetNetProvider> instances = new List<FishNetNetProvider>();

		private bool _wantsToInitialize;

		private bool NetworkInitialize___EarlyMetaVoiceChat_002ENetProviders_002EFishNet_002EFishNetNetProviderAssembly_002DCSharp_002Edll_Excuted;

		private bool NetworkInitialize___LateMetaVoiceChat_002ENetProviders_002EFishNet_002EFishNetNetProviderAssembly_002DCSharp_002Edll_Excuted;

		public static FishNetNetProvider LocalPlayerInstance { get; private set; }

		public static IReadOnlyList<FishNetNetProvider> Instances => instances;

		bool INetProvider.IsLocalPlayerDeafened
		{
			get
			{
				if (!LocalPlayerInstance)
				{
					return true;
				}
				return LocalPlayerInstance.MetaVc.isDeafened;
			}
		}

		public MetaVc MetaVc { get; private set; }

		public override void OnStartClient()
		{
			if (base.gameObject.activeInHierarchy)
			{
				Initialize();
			}
			else
			{
				_wantsToInitialize = true;
			}
		}

		private void OnEnable()
		{
			if (_wantsToInitialize)
			{
				Initialize();
				_wantsToInitialize = false;
			}
		}

		private void OnDisable()
		{
			_wantsToInitialize = true;
		}

		private string GetParents()
		{
			Transform parent = base.transform.parent;
			string text = "";
			while ((bool)parent)
			{
				if (text != "")
				{
					text += ", ";
				}
				text += parent.name;
				parent = parent.parent;
			}
			return text;
		}

		private void Initialize()
		{
			if (base.Owner.IsLocalClient)
			{
				LocalPlayerInstance = this;
			}
			if (!instances.Contains(this))
			{
				instances.Add(this);
			}
			MetaVc = GetComponent<MetaVc>();
			MetaVc.StartClient(this, base.Owner.IsLocalClient, GetMaxDataBytesPerPacket(base.NetworkManager));
			static int GetMaxDataBytesPerPacket(NetworkManager networkManager)
			{
				return networkManager.TransportManager.GetLowestMTU() - 13 - 4 - 8 - 1 - 2;
			}
		}

		public override void OnStopClient()
		{
			if (base.Owner.IsLocalClient)
			{
				LocalPlayerInstance = null;
			}
			if (instances.Contains(this))
			{
				instances.Remove(this);
			}
			MetaVc.StopClient();
		}

		void INetProvider.RelayFrame(int index, double timestamp, ReadOnlySpan<byte> data)
		{
			byte[] array = FixedLengthArrayPool<byte>.Rent(data.Length);
			data.CopyTo(array);
			float deltaTime = Time.deltaTime;
			FishNetFrame frame = new FishNetFrame(index, timestamp, deltaTime, array);
			if (base.IsServerInitialized)
			{
				ObsReceiveFrame(frame);
			}
			else
			{
				ServerRelayFrame(frame);
			}
			FixedLengthArrayPool<byte>.Return(array);
		}

		[ServerRpc]
		private void ServerRelayFrame(FishNetFrame frame, Channel channel = Channel.Unreliable)
		{
			RpcWriter___ServerRelayFrame___32292061(frame, channel);
		}

		[ObserversRpc(ExcludeOwner = true)]
		private void ObsReceiveFrame(FishNetFrame frame, Channel channel = Channel.Unreliable)
		{
			RpcWriter___ObsReceiveFrame___32292061(frame, channel);
		}

		public override void NetworkInitialize___Early()
		{
			if (!NetworkInitialize___EarlyMetaVoiceChat_002ENetProviders_002EFishNet_002EFishNetNetProviderAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___EarlyMetaVoiceChat_002ENetProviders_002EFishNet_002EFishNetNetProviderAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Early();
				RegisterServerRpc(0u, RpcReader___ServerRelayFrame___32292061);
				RegisterObserversRpc(1u, RpcReader___ObsReceiveFrame___32292061);
			}
		}

		public override void NetworkInitialize___Late()
		{
			if (!NetworkInitialize___LateMetaVoiceChat_002ENetProviders_002EFishNet_002EFishNetNetProviderAssembly_002DCSharp_002Edll_Excuted)
			{
				NetworkInitialize___LateMetaVoiceChat_002ENetProviders_002EFishNet_002EFishNetNetProviderAssembly_002DCSharp_002Edll_Excuted = true;
				base.NetworkInitialize___Late();
			}
		}

		public override void NetworkInitializeIfDisabled()
		{
			NetworkInitialize___Early();
			NetworkInitialize___Late();
		}

		private void RpcWriter___ServerRelayFrame___32292061(FishNetFrame frame, Channel channel = Channel.Unreliable)
		{
			if (!base.IsClientInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				return;
			}
			if (!base.IsOwner)
			{
				NetworkManager networkManager2 = base.NetworkManager;
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
				return;
			}
			Channel channel2 = channel;
			PooledWriter pooledWriter = WriterPool.Retrieve();
			pooledWriter.WriteFishNetFrame(frame);
			SendServerRpc(0u, pooledWriter, channel2, DataOrderType.Default);
			pooledWriter.Store();
		}

		private void RpcLogic___ServerRelayFrame___32292061(FishNetFrame P_0, Channel P_1)
		{
			float additionalLatency = P_0.additionalLatency + Time.deltaTime;
			P_0 = new FishNetFrame(P_0.index, P_0.timestamp, additionalLatency, P_0.data);
			ObsReceiveFrame(P_0);
		}

		private void RpcReader___ServerRelayFrame___32292061(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
		{
			FishNetFrame fishNetFrame = PooledReader0.ReadFishNetFrame();
			if (base.IsServerInitialized && OwnerMatches(conn))
			{
				RpcLogic___ServerRelayFrame___32292061(fishNetFrame, channel);
			}
		}

		private void RpcWriter___ObsReceiveFrame___32292061(FishNetFrame frame, Channel channel = Channel.Unreliable)
		{
			if (!base.IsServerInitialized)
			{
				NetworkManager networkManager = base.NetworkManager;
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
				return;
			}
			Channel channel2 = channel;
			PooledWriter pooledWriter = WriterPool.Retrieve();
			pooledWriter.WriteFishNetFrame(frame);
			SendObserversRpc(1u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: true);
			pooledWriter.Store();
		}

		private void RpcLogic___ObsReceiveFrame___32292061(FishNetFrame P_0, Channel P_1)
		{
			if (base.IsServerInitialized)
			{
				float additionalLatency = P_0.additionalLatency - Time.deltaTime;
				MetaVc.ReceiveFrame(P_0.index, P_0.timestamp, additionalLatency, P_0.data);
			}
			else
			{
				MetaVc.ReceiveFrame(P_0.index, P_0.timestamp, P_0.additionalLatency, P_0.data);
			}
		}

		private void RpcReader___ObsReceiveFrame___32292061(PooledReader PooledReader0, Channel channel)
		{
			FishNetFrame fishNetFrame = PooledReader0.ReadFishNetFrame();
			if (base.IsClientInitialized)
			{
				RpcLogic___ObsReceiveFrame___32292061(fishNetFrame, channel);
			}
		}

		public virtual void Awake()
		{
			NetworkInitialize___Early();
			NetworkInitialize___Late();
		}
	}
}
