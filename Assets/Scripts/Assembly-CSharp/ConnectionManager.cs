using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Logging;
using FishNet.Managing.Server;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.UTP;
using FishySteamworks;
using Steamworks;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
	public static ConnectionManager Instance;

	[SerializeField]
	private NetworkManager _netMan;

	[SerializeField]
	private global::FishySteamworks.FishySteamworks _fishy;

	[SerializeField]
	private Multipass _multipass;

	private bool _clientConnectionExpected;

	private bool _showCrashWhenConnected;

	private bool _returnToMenuRequested;

	public static bool IsUsingSteam = true;

	private void Awake()
	{
		Instance = this;
		_netMan.ClientManager.OnClientConnectionState += OnClientConnectionState;
		Callback<LobbyEnter_t>.Create(OnLobbyEntered);
	}

	private void OnDestroy()
	{
		if ((bool)_netMan)
		{
			_netMan.ClientManager.OnClientConnectionState -= OnClientConnectionState;
		}
	}

	private void Update()
	{
		if (_returnToMenuRequested)
		{
			_returnToMenuRequested = false;
			MainMenuManager.ToggleMenu(enabled: true);
		}
	}

	private void OnClientConnectionState(ClientConnectionStateArgs args)
	{
		if (args.ConnectionState == LocalConnectionState.Starting)
		{
			_clientConnectionExpected = true;
		}
		else if (args.ConnectionState == LocalConnectionState.Started)
		{
			if (_showCrashWhenConnected)
			{
				_showCrashWhenConnected = false;
				MainMenuManager.CrashAnimation();
			}
		}
		else if (args.ConnectionState == LocalConnectionState.Stopped && _clientConnectionExpected)
		{
			_clientConnectionExpected = false;
			_showCrashWhenConnected = false;
			_returnToMenuRequested = true;
		}
	}

	private void OnLobbyEntered(LobbyEnter_t param)
	{
		SetTransport(toSteam: true);
		GameInfo.GenerateSeed();
	}

	public void LeaveTransport()
	{
		_clientConnectionExpected = false;
		_showCrashWhenConnected = false;
		_returnToMenuRequested = false;
		_multipass.StopConnection(server: false);
		if (_multipass.NetworkManager.IsServerStarted)
		{
			_multipass.StopConnection(server: true);
		}
	}

	private void SetTransport(bool toSteam)
	{
		IsUsingSteam = toSteam;
		if (toSteam)
		{
			_multipass.SetClientTransport<global::FishySteamworks.FishySteamworks>();
		}
		else
		{
			_multipass.SetClientTransport<UnityTransport>();
		}
	}

	public bool CreateOnlineLobby(int maxPlayers)
	{
		SetTransport(toSteam: true);
		bool flag = _fishy.StartConnection(server: true);
		if (flag)
		{
			_fishy.SetMaximumClients(maxPlayers);
		}
		LocalConnectionState connectionState = _fishy.GetConnectionState(server: true);
		if (!flag || connectionState != LocalConnectionState.Started || !_netMan.IsServerStarted)
		{
			Debug.LogError($"Failed to start Steam host. Start returned {flag}, FishySteamworks state is {connectionState}, FishNet server started is {_netMan.IsServerStarted}.");
			if (connectionState.IsStartedOrStarting())
			{
				_fishy.StopConnection(server: true);
			}
			return false;
		}
		return true;
	}

	public void KickSteamUserIfConnected(CSteamID steamID)
	{
		if (!IsUsingSteam || !_netMan.IsServerStarted)
		{
			return;
		}
		foreach (NetworkConnection value in _netMan.ServerManager.Clients.Values)
		{
			if (ulong.TryParse(value.GetAddress(), out var result) && result == steamID.m_SteamID)
			{
				value.Kick(KickReason.UnexpectedProblem, LoggingType.Common, $"Steam user {steamID.m_SteamID} is no longer authorized by lobby {SteamManager.CurrentLobbyID.m_SteamID}.");
				break;
			}
		}
	}

	public bool JoinOnlineLobby(CSteamID steamID)
	{
		SetTransport(toSteam: true);
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(steamID);
		if (lobbyOwner == CSteamID.Nil)
		{
			Debug.LogError($"Cannot join Steam lobby {steamID.m_SteamID} because Steam did not provide a valid lobby owner.");
			_showCrashWhenConnected = false;
			_returnToMenuRequested = true;
			return false;
		}
		string clientAddress = lobbyOwner.m_SteamID.ToString();
		_fishy.SetClientAddress(clientAddress);
		_clientConnectionExpected = true;
		_showCrashWhenConnected = true;
		if (!_fishy.StartConnection(server: false))
		{
			_showCrashWhenConnected = false;
			_returnToMenuRequested = true;
			return false;
		}
		return true;
	}

	public void CreateOfflineLobby()
	{
		SetTransport(toSteam: false);
		GameInfo.GenerateSeed();
		_multipass.ClientTransport.SetClientAddress("localhost");
		_multipass.ClientTransport.StartConnection(server: true);
		_multipass.ClientTransport.StartConnection(server: false);
		MainMenuManager.CrashAnimation();
	}

	public void JoinOfflineLobby()
	{
		SetTransport(toSteam: false);
		_multipass.ClientTransport.SetClientAddress("localhost");
		_clientConnectionExpected = true;
		if (!_multipass.ClientTransport.StartConnection(server: false))
		{
			_returnToMenuRequested = true;
		}
		MainMenuManager.CrashAnimation();
	}
}
