using System;
using FishNet.Managing;
using FishNet.Transporting;
using FishySteamworks.Client;
using FishySteamworks.Server;
using Steamworks;
using UnityEngine;

namespace FishySteamworks
{
	public class FishySteamworks : Transport
	{
		[NonSerialized]
		public ulong LocalUserSteamID;

		[Tooltip("Address server should bind to.")]
		[SerializeField]
		private string _serverBindAddress = string.Empty;

		[Tooltip("Port to use.")]
		[SerializeField]
		private ushort _port = 7770;

		[Tooltip("Maximum number of players which may be connected at once.")]
		[Range(1f, 65535f)]
		[SerializeField]
		private ushort _maximumClients = 9001;

		[Tooltip("True if using peer to peer socket.")]
		[SerializeField]
		private bool _peerToPeer;

		[Tooltip("Address client should connect to.")]
		[SerializeField]
		private string _clientAddress = string.Empty;

		private int[] _mtus;

		private ClientSocket _client;

		private ClientHostSocket _clientHost;

		private ServerSocket _server;

		private bool _shutdownCalled = true;

		internal const int CLIENT_HOST_ID = 32767;

		public override event Action<ClientConnectionStateArgs> OnClientConnectionState;

		public override event Action<ServerConnectionStateArgs> OnServerConnectionState;

		public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState;

		public override event Action<ClientReceivedDataArgs> OnClientReceivedData;

		public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

		~FishySteamworks()
		{
			Shutdown();
		}

		public override void Initialize(NetworkManager networkManager, int transportIndex)
		{
			base.Initialize(networkManager, transportIndex);
			_client = new ClientSocket();
			_clientHost = new ClientHostSocket();
			_server = new ServerSocket();
			CreateChannelData();
			_client.Initialize(this);
			_clientHost.Initialize(this);
			_server.Initialize(this);
		}

		private void OnDestroy()
		{
			Shutdown();
		}

		private void Update()
		{
			_clientHost.CheckSetStarted();
		}

		private void CreateChannelData()
		{
			_mtus = new int[2] { 1048576, 1200 };
		}

		private bool InitializeRelayNetworkAccess()
		{
			try
			{
				SteamNetworkingUtils.InitRelayNetworkAccess();
				if (IsNetworkAccessAvailable())
				{
					LocalUserSteamID = SteamUser.GetSteamID().m_SteamID;
				}
				_shutdownCalled = false;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool IsNetworkAccessAvailable()
		{
			try
			{
				InteropHelp.TestIfAvailableClient();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public override string GetConnectionAddress(int connectionId)
		{
			return _server.GetConnectionAddress(connectionId);
		}

		public override LocalConnectionState GetConnectionState(bool server)
		{
			if (server)
			{
				return _server.GetLocalConnectionState();
			}
			return _client.GetLocalConnectionState();
		}

		public override RemoteConnectionState GetConnectionState(int connectionId)
		{
			return _server.GetConnectionState(connectionId);
		}

		public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
		{
			OnClientConnectionState?.Invoke(connectionStateArgs);
		}

		public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
		{
			OnServerConnectionState?.Invoke(connectionStateArgs);
		}

		public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
		{
			OnRemoteConnectionState?.Invoke(connectionStateArgs);
		}

		public override void IterateIncoming(bool server)
		{
			if (server)
			{
				_server.IterateIncoming();
				return;
			}
			_client.IterateIncoming();
			_clientHost.IterateIncoming();
		}

		public override void IterateOutgoing(bool server)
		{
			if (server)
			{
				_server.IterateOutgoing();
			}
			else
			{
				_client.IterateOutgoing();
			}
		}

		public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs)
		{
			OnClientReceivedData?.Invoke(receivedDataArgs);
		}

		public override void HandleServerReceivedDataArgs(ServerReceivedDataArgs receivedDataArgs)
		{
			OnServerReceivedData?.Invoke(receivedDataArgs);
		}

		public override void SendToServer(byte channelId, ArraySegment<byte> segment)
		{
			_client.SendToServer(channelId, segment);
			_clientHost.SendToServer(channelId, segment);
		}

		public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
		{
			_server.SendToClient(channelId, segment, connectionId);
		}

		public override int GetMaximumClients()
		{
			return _server.GetMaximumClients();
		}

		public override void SetMaximumClients(int value)
		{
			_server.SetMaximumClients(value);
		}

		public override void SetClientAddress(string address)
		{
			_clientAddress = address;
		}

		public override void SetServerBindAddress(string address, IPAddressType addressType)
		{
			_serverBindAddress = address;
		}

		public override void SetPort(ushort port)
		{
			_port = port;
		}

		public override bool StartConnection(bool server)
		{
			if (server)
			{
				return StartServer();
			}
			return StartClient(_clientAddress);
		}

		public override bool StopConnection(bool server)
		{
			if (server)
			{
				return StopServer();
			}
			return StopClient();
		}

		public override bool StopConnection(int connectionId, bool immediately)
		{
			return StopClient(connectionId, immediately);
		}

		public override void Shutdown()
		{
			if (!_shutdownCalled)
			{
				_shutdownCalled = true;
				StopConnection(server: false);
				StopConnection(server: true);
			}
		}

		private bool StartServer()
		{
			if (!InitializeRelayNetworkAccess())
			{
				base.NetworkManager.LogError("RelayNetworkAccess could not be initialized.");
				return false;
			}
			if (!IsNetworkAccessAvailable())
			{
				base.NetworkManager.LogError("Server network access is not available.");
				return false;
			}
			_server.ResetInvalidSocket();
			if (_server.GetLocalConnectionState() != LocalConnectionState.Stopped)
			{
				base.NetworkManager.LogError("Server is already running.");
				return false;
			}
			bool flag = _client.GetLocalConnectionState() != LocalConnectionState.Stopped;
			if (flag)
			{
				_client.StopConnection();
			}
			bool num = _server.StartConnection(_serverBindAddress, _port, _maximumClients, _peerToPeer);
			if (num & flag)
			{
				StartConnection(server: false);
			}
			return num;
		}

		private bool StopServer()
		{
			if (_server != null)
			{
				return _server.StopConnection();
			}
			return false;
		}

		private bool StartClient(string address)
		{
			if (_server.GetLocalConnectionState() == LocalConnectionState.Stopped)
			{
				if (_client.GetLocalConnectionState() != LocalConnectionState.Stopped)
				{
					base.NetworkManager.LogError("Client is already running.");
					return false;
				}
				if (_clientHost.GetLocalConnectionState() != LocalConnectionState.Stopped)
				{
					_clientHost.StopConnection();
				}
				if (!InitializeRelayNetworkAccess())
				{
					base.NetworkManager.LogError("RelayNetworkAccess could not be initialized.");
					return false;
				}
				if (!IsNetworkAccessAvailable())
				{
					base.NetworkManager.LogError("Client network access is not available.");
					return false;
				}
				_client.StartConnection(address, _port, _peerToPeer);
			}
			else
			{
				_clientHost.StartConnection(_server);
			}
			return true;
		}

		private bool StopClient()
		{
			bool flag = false;
			if (_client != null)
			{
				flag |= _client.StopConnection();
			}
			if (_clientHost != null)
			{
				flag |= _clientHost.StopConnection();
			}
			return flag;
		}

		private bool StopClient(int connectionId, bool immediately)
		{
			return _server.StopConnection(connectionId);
		}

		public override int GetMTU(byte channel)
		{
			if (channel >= _mtus.Length)
			{
				Debug.LogError($"Channel {channel} is out of bounds.");
				return 0;
			}
			return _mtus[channel];
		}
	}
}
