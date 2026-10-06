using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Utility.Performance;
using Steamworks;

namespace FishySteamworks
{
	public abstract class CommonSocket
	{
		private LocalConnectionState _connectionState = LocalConnectionState.Stopped;

		protected bool PeerToPeer;

		protected Transport Transport;

		protected IntPtr[] MessagePointers = new IntPtr[256];

		protected byte[] InboundBuffer;

		protected const int MAX_MESSAGES = 256;

		internal LocalConnectionState GetLocalConnectionState()
		{
			return _connectionState;
		}

		protected virtual void SetLocalConnectionState(LocalConnectionState connectionState, bool server)
		{
			if (connectionState != _connectionState)
			{
				_connectionState = connectionState;
				if (server)
				{
					Transport.HandleServerConnectionState(new ServerConnectionStateArgs(connectionState, Transport.Index));
				}
				else
				{
					Transport.HandleClientConnectionState(new ClientConnectionStateArgs(connectionState, Transport.Index));
				}
			}
		}

		internal virtual void Initialize(Transport t)
		{
			Transport = t;
			int mTU = Transport.GetMTU(0);
			mTU = Math.Max(mTU, Transport.GetMTU(1));
			InboundBuffer = new byte[mTU];
		}

		protected byte[] GetIPBytes(string address)
		{
			if (!string.IsNullOrEmpty(address))
			{
				if (!IPAddress.TryParse(address, out var address2))
				{
					Transport.NetworkManager.LogError("Could not parse address " + address + " to IPAddress.");
					return null;
				}
				return address2.GetAddressBytes();
			}
			return null;
		}

		protected EResult Send(HSteamNetConnection steamConnection, ArraySegment<byte> segment, byte channelId)
		{
			if (segment.Array.Length - 1 <= segment.Offset + segment.Count)
			{
				byte[] array = segment.Array;
				Array.Resize(ref array, array.Length + 1);
				array[^1] = channelId;
			}
			else
			{
				segment.Array[segment.Offset + segment.Count] = channelId;
			}
			segment = new ArraySegment<byte>(segment.Array, segment.Offset, segment.Count + 1);
			GCHandle gCHandle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
			IntPtr pData = gCHandle.AddrOfPinnedObject() + segment.Offset;
			int nSendFlags = ((channelId != 1) ? 8 : 0);
			EResult eResult = SteamNetworkingSockets.SendMessageToConnection(steamConnection, pData, (uint)segment.Count, nSendFlags, out var _);
			if (eResult != EResult.k_EResultOK)
			{
				Transport.NetworkManager.LogWarning($"Send issue: {eResult}");
			}
			gCHandle.Free();
			return eResult;
		}

		internal void ClearQueue(ConcurrentQueue<LocalPacket> queue)
		{
			LocalPacket result;
			while (queue.TryDequeue(out result))
			{
				ByteArrayPool.Store(result.Data);
			}
		}

		internal void ClearQueue(Queue<LocalPacket> queue)
		{
			while (queue.Count > 0)
			{
				ByteArrayPool.Store(queue.Dequeue().Data);
			}
		}

		protected void GetMessage(IntPtr ptr, byte[] buffer, out ArraySegment<byte> segment, out byte channel)
		{
			SteamNetworkingMessage_t steamNetworkingMessage_t = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptr);
			int cbSize = steamNetworkingMessage_t.m_cbSize;
			Marshal.Copy(steamNetworkingMessage_t.m_pData, buffer, 0, cbSize);
			SteamNetworkingMessage_t.Release(ptr);
			channel = buffer[cbSize - 1];
			segment = new ArraySegment<byte>(buffer, 0, cbSize - 1);
		}
	}
}
