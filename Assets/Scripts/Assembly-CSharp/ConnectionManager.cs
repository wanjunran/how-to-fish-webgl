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

	// WebGL 单人版：不使用 Steam 大厅与Steamworks 传输层
	public static bool IsUsingSteam = false;

	private void Awake()
	{
		Instance = this;
		_netMan.ClientManager.OnClientConnectionState += OnClientConnectionState;
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
		// WebGL 单人版：始终使用 UnityTransport，不加载 Steamworks 传输层
		IsUsingSteam = false;
		_multipass.SetClientTransport<UnityTransport>();
	}

	// ---- WebGL 单人版：以下 Steam 大厅相关功能已停用 ----
	// 原因：Steamworks 传输层依赖 steam_api64.dll（Windows 原生库），WebGL 无法加载。
	// 原实现保留于此，供日后恢复 Steam 联机时参考。

	public bool CreateOnlineLobby(int maxPlayers)
	{
		Debug.LogWarning("[WebGL] Steam 联机已停用。");
		return false;
	}

	public void KickSteamUserIfConnected(CSteamID steamID)
	{
		// 单人版无需踢出 Steam 玩家
	}

	public bool JoinOnlineLobby(CSteamID steamID)
	{
		Debug.LogWarning("[WebGL] Steam 联机已停用。");
		return false;
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
