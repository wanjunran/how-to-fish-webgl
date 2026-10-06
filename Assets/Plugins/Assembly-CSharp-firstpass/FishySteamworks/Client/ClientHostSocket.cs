using System;
using System.Collections.Generic;
using FishNet.Transporting;
using FishNet.Utility.Performance;
using FishySteamworks.Server;

namespace FishySteamworks.Client
{
	public class ClientHostSocket : CommonSocket
	{
		private ServerSocket _server;

		private Queue<LocalPacket> _incoming = new Queue<LocalPacket>();

		internal void CheckSetStarted()
		{
			if (_server != null && GetLocalConnectionState() == LocalConnectionState.Starting && _server.GetLocalConnectionState() == LocalConnectionState.Started)
			{
				SetLocalConnectionState(LocalConnectionState.Started, server: false);
			}
		}

		internal bool StartConnection(ServerSocket serverSocket)
		{
			_server = serverSocket;
			_server.SetClientHostSocket(this);
			if (_server.GetLocalConnectionState() != LocalConnectionState.Started)
			{
				return false;
			}
			SetLocalConnectionState(LocalConnectionState.Starting, server: false);
			return true;
		}

		protected override void SetLocalConnectionState(LocalConnectionState connectionState, bool server)
		{
			base.SetLocalConnectionState(connectionState, server);
			if (connectionState == LocalConnectionState.Started)
			{
				_server.OnClientHostState(started: true);
			}
			else
			{
				_server.OnClientHostState(started: false);
			}
		}

		internal bool StopConnection()
		{
			if (GetLocalConnectionState() == LocalConnectionState.Stopped || GetLocalConnectionState() == LocalConnectionState.Stopping)
			{
				return false;
			}
			ClearQueue(_incoming);
			SetLocalConnectionState(LocalConnectionState.Stopping, server: false);
			SetLocalConnectionState(LocalConnectionState.Stopped, server: false);
			_server.SetClientHostSocket(null);
			return true;
		}

		internal void IterateIncoming()
		{
			if (GetLocalConnectionState() == LocalConnectionState.Started)
			{
				while (_incoming.Count > 0)
				{
					LocalPacket localPacket = _incoming.Dequeue();
					ArraySegment<byte> data = new ArraySegment<byte>(localPacket.Data, 0, localPacket.Length);
					Transport.HandleClientReceivedDataArgs(new ClientReceivedDataArgs(data, (Channel)localPacket.Channel, Transport.Index));
					ByteArrayPool.Store(localPacket.Data);
				}
			}
		}

		internal void ReceivedFromLocalServer(LocalPacket packet)
		{
			_incoming.Enqueue(packet);
		}

		internal void SendToServer(byte channelId, ArraySegment<byte> segment)
		{
			if (GetLocalConnectionState() == LocalConnectionState.Started && _server.GetLocalConnectionState() == LocalConnectionState.Started)
			{
				LocalPacket packet = new LocalPacket(segment, channelId);
				_server.ReceivedFromClientHost(packet);
			}
		}
	}
}
