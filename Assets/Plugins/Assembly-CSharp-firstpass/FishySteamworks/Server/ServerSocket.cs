using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FishNet.Managing;
using FishNet.Transporting;
using FishySteamworks.Client;
using Steamworks;

namespace FishySteamworks.Server
{
	public class ServerSocket : CommonSocket
	{
		public struct ConnectionChange
		{
			public int ConnectionId;

			public HSteamNetConnection SteamConnection;

			public CSteamID SteamId;

			public bool IsConnect => SteamId.IsValid();

			public ConnectionChange(int id)
			{
				ConnectionId = id;
				SteamId = CSteamID.Nil;
				SteamConnection = default;
			}

			public ConnectionChange(int id, HSteamNetConnection steamConnection, CSteamID steamId)
			{
				ConnectionId = id;
				SteamConnection = steamConnection;
				SteamId = steamId;
			}
		}

		private BidirectionalDictionary<HSteamNetConnection, int> _steamConnections = new BidirectionalDictionary<HSteamNetConnection, int>();

		private BidirectionalDictionary<CSteamID, int> _steamIds = new BidirectionalDictionary<CSteamID, int>();

		private int _maximumClients;

		private int _nextConnectionId;

		private HSteamListenSocket _socket = new HSteamListenSocket(0u);

		private Queue<LocalPacket> _clientHostIncoming = new Queue<LocalPacket>();

		private bool _clientHostStarted;

		private Callback<SteamNetConnectionStatusChangedCallback_t> _onRemoteConnectionStateCallback;

		private Queue<int> _cachedConnectionIds = new Queue<int>();

		private ClientHostSocket _clientHost;

		private bool _iteratingConnections;

		private List<ConnectionChange> _pendingConnectionChanges = new List<ConnectionChange>();

		internal RemoteConnectionState GetConnectionState(int connectionId)
		{
			if (_steamConnections.Second.ContainsKey(connectionId))
			{
				return RemoteConnectionState.Started;
			}
			return RemoteConnectionState.Stopped;
		}

		internal void ResetInvalidSocket()
		{
			if (_socket == HSteamListenSocket.Invalid)
			{
				base.SetLocalConnectionState(LocalConnectionState.Stopped, server: true);
			}
		}

		internal bool StartConnection(string address, ushort port, int maximumClients, bool peerToPeer)
		{
			try
			{
				if (_onRemoteConnectionStateCallback == null)
				{
					_onRemoteConnectionStateCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnRemoteConnectionState);
				}
				PeerToPeer = peerToPeer;
				byte[] array = ((!peerToPeer) ? GetIPBytes(address) : null);
				PeerToPeer = peerToPeer;
				SetMaximumClients(maximumClients);
				_nextConnectionId = 0;
				_cachedConnectionIds.Clear();
				_iteratingConnections = false;
				base.SetLocalConnectionState(LocalConnectionState.Starting, server: true);
				SteamNetworkingConfigValue_t[] array2 = new SteamNetworkingConfigValue_t[0];
				if (PeerToPeer)
				{
					_socket = SteamNetworkingSockets.CreateListenSocketP2P(0, array2.Length, array2);
				}
				else
				{
					SteamNetworkingIPAddr localAddress = default;
					localAddress.Clear();
					if (array != null)
					{
						localAddress.SetIPv6(array, port);
					}
					_socket = SteamNetworkingSockets.CreateListenSocketIP(ref localAddress, 0, array2);
				}
			}
			catch
			{
				base.SetLocalConnectionState(LocalConnectionState.Stopped, server: true);
				return false;
			}
			if (_socket == HSteamListenSocket.Invalid)
			{
				base.SetLocalConnectionState(LocalConnectionState.Stopped, server: true);
				return false;
			}
			base.SetLocalConnectionState(LocalConnectionState.Started, server: true);
			return true;
		}

		internal bool StopConnection()
		{
			if (_socket != HSteamListenSocket.Invalid)
			{
				SteamNetworkingSockets.CloseListenSocket(_socket);
				if (_onRemoteConnectionStateCallback != null)
				{
					_onRemoteConnectionStateCallback.Dispose();
					_onRemoteConnectionStateCallback = null;
				}
				_socket = HSteamListenSocket.Invalid;
			}
			_pendingConnectionChanges.Clear();
			if (GetLocalConnectionState() == LocalConnectionState.Stopped)
			{
				return false;
			}
			base.SetLocalConnectionState(LocalConnectionState.Stopping, server: true);
			base.SetLocalConnectionState(LocalConnectionState.Stopped, server: true);
			return true;
		}

		internal bool StopConnection(int connectionId)
		{
			if (connectionId == 32767)
			{
				if (_clientHost != null)
				{
					_clientHost.StopConnection();
					return true;
				}
				return false;
			}
			if (_steamConnections.Second.TryGetValue(connectionId, out var value))
			{
				return StopConnection(connectionId, value);
			}
			Transport.NetworkManager.LogError($"Steam connection not found for connectionId {connectionId}.");
			return false;
		}

		private bool StopConnection(int connectionId, HSteamNetConnection socket)
		{
			SteamNetworkingSockets.CloseConnection(socket, 0, string.Empty, bEnableLinger: false);
			if (!_iteratingConnections)
			{
				RemoveConnection(connectionId);
			}
			else
			{
				_pendingConnectionChanges.Add(new ConnectionChange(connectionId));
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void OnRemoteConnectionState(SteamNetConnectionStatusChangedCallback_t args)
		{
			ulong steamID = args.m_info.m_identityRemote.GetSteamID64();
			if (args.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
			{
				if (_steamConnections.Count >= GetMaximumClients())
				{
					Transport.NetworkManager.Log($"Incoming connection {steamID} was rejected because would exceed the maximum connection count.");
					SteamNetworkingSockets.CloseConnection(args.m_hConn, 0, "Max Connection Count", bEnableLinger: false);
					return;
				}
				EResult eResult = SteamNetworkingSockets.AcceptConnection(args.m_hConn);
				if (eResult == EResult.k_EResultOK)
				{
					Transport.NetworkManager.Log($"Accepting connection {steamID}");
				}
				else
				{
					Transport.NetworkManager.Log($"Connection {steamID} could not be accepted: {eResult.ToString()}");
				}
			}
			else if (args.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
			{
				int num = ((_cachedConnectionIds.Count > 0) ? _cachedConnectionIds.Dequeue() : _nextConnectionId++);
				if (!_iteratingConnections)
				{
					AddConnection(num, args.m_hConn, args.m_info.m_identityRemote.GetSteamID());
				}
				else
				{
					_pendingConnectionChanges.Add(new ConnectionChange(num, args.m_hConn, args.m_info.m_identityRemote.GetSteamID()));
				}
			}
			else if (args.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer || args.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
			{
				if (_steamConnections.TryGetValue(args.m_hConn, out var value))
				{
					StopConnection(value, args.m_hConn);
				}
			}
			else
			{
				Transport.NetworkManager.Log($"Connection {steamID} state changed: {args.m_info.m_eState.ToString()}");
			}
		}

		private void AddConnection(int connectionId, HSteamNetConnection steamConnection, CSteamID steamId)
		{
			_steamConnections.Add(steamConnection, connectionId);
			_steamIds.Add(steamId, connectionId);
			Transport.NetworkManager.Log($"Client with SteamID {steamId.m_SteamID} connected. Assigning connection id {connectionId}");
			Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Started, connectionId, Transport.Index));
		}

		private void RemoveConnection(int connectionId)
		{
			_steamConnections.Remove(connectionId);
			_steamIds.Remove(connectionId);
			Transport.NetworkManager.Log($"Client with ConnectionID {connectionId} disconnected.");
			Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Stopped, connectionId, Transport.Index));
			_cachedConnectionIds.Enqueue(connectionId);
		}

		internal void IterateOutgoing()
		{
			if (GetLocalConnectionState() != LocalConnectionState.Started)
			{
				return;
			}
			_iteratingConnections = true;
			foreach (HSteamNetConnection firstType in _steamConnections.FirstTypes)
			{
				SteamNetworkingSockets.FlushMessagesOnConnection(firstType);
			}
			_iteratingConnections = false;
			ProcessPendingConnectionChanges();
		}

		internal void IterateIncoming()
		{
			if (GetLocalConnectionState() == LocalConnectionState.Stopped || GetLocalConnectionState() == LocalConnectionState.Stopping)
			{
				return;
			}
			_iteratingConnections = true;
			while (_clientHostIncoming.Count > 0)
			{
				LocalPacket localPacket = _clientHostIncoming.Dequeue();
				ArraySegment<byte> data = new ArraySegment<byte>(localPacket.Data, 0, localPacket.Length);
				Transport.HandleServerReceivedDataArgs(new ServerReceivedDataArgs(data, (Channel)localPacket.Channel, 32767, Transport.Index));
			}
			foreach (KeyValuePair<HSteamNetConnection, int> item in _steamConnections.First)
			{
				HSteamNetConnection key = item.Key;
				int value = item.Value;
				int num = SteamNetworkingSockets.ReceiveMessagesOnConnection(key, MessagePointers, 256);
				if (num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						GetMessage(MessagePointers[i], InboundBuffer, out var segment, out var channel);
						Transport.HandleServerReceivedDataArgs(new ServerReceivedDataArgs(segment, (Channel)channel, value, Transport.Index));
					}
				}
			}
			_iteratingConnections = false;
			ProcessPendingConnectionChanges();
		}

		private void ProcessPendingConnectionChanges()
		{
			foreach (ConnectionChange pendingConnectionChange in _pendingConnectionChanges)
			{
				if (pendingConnectionChange.IsConnect)
				{
					AddConnection(pendingConnectionChange.ConnectionId, pendingConnectionChange.SteamConnection, pendingConnectionChange.SteamId);
				}
				else
				{
					RemoveConnection(pendingConnectionChange.ConnectionId);
				}
			}
			_pendingConnectionChanges.Clear();
		}

		internal void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
		{
			if (GetLocalConnectionState() != LocalConnectionState.Started)
			{
				return;
			}
			HSteamNetConnection value;
			if (connectionId == 32767)
			{
				if (_clientHost != null)
				{
					LocalPacket packet = new LocalPacket(segment, channelId);
					_clientHost.ReceivedFromLocalServer(packet);
				}
			}
			else if (_steamConnections.TryGetValue(connectionId, out value))
			{
				EResult eResult = Send(value, segment, channelId);
				switch (eResult)
				{
				case EResult.k_EResultNoConnection:
				case EResult.k_EResultInvalidParam:
					Transport.NetworkManager.Log($"Connection to {connectionId} was lost.");
					StopConnection(connectionId, value);
					break;
				default:
					Transport.NetworkManager.LogError("Could not send: " + eResult);
					break;
				case EResult.k_EResultOK:
					break;
				}
			}
			else
			{
				Transport.NetworkManager.LogError($"ConnectionId {connectionId} does not exist, data will not be sent.");
			}
		}

		internal string GetConnectionAddress(int connectionId)
		{
			if (_steamIds.TryGetValue(connectionId, out var value))
			{
				return value.ToString();
			}
			Transport.NetworkManager.LogError($"ConnectionId {connectionId} is invalid; address cannot be returned.");
			return string.Empty;
		}

		internal void SetMaximumClients(int value)
		{
			_maximumClients = Math.Min(value, 32766);
		}

		internal int GetMaximumClients()
		{
			return _maximumClients;
		}

		internal void SetClientHostSocket(ClientHostSocket socket)
		{
			_clientHost = socket;
		}

		internal void OnClientHostState(bool started)
		{
			FishySteamworks fishySteamworks = (FishySteamworks)Transport;
			CSteamID key = new CSteamID(fishySteamworks.LocalUserSteamID);
			if (!started && _clientHostStarted)
			{
				ClearQueue(_clientHostIncoming);
				Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Stopped, 32767, Transport.Index));
				_steamIds.Remove(key);
			}
			else if (started)
			{
				_steamIds[key] = 32767;
				Transport.HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Started, 32767, Transport.Index));
			}
			_clientHostStarted = started;
		}

		internal void ReceivedFromClientHost(LocalPacket packet)
		{
			if (_clientHostStarted)
			{
				_clientHostIncoming.Enqueue(packet);
			}
		}
	}
}
