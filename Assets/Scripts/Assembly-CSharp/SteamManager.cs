using System;
using FishNet;
using FishNet.Managing;
using FishySteamworks;
using Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
	private static SteamManager _instance;

	private Callback<SteamRelayNetworkStatus_t> _relayStatusCallback;

	[SerializeField]
	private NetworkManager _netMan;

	[SerializeField]
	private global::FishySteamworks.FishySteamworks _fishy;

	private const int MaxPlayers = 8;

	private static DisconnectReason _disconnectReason;

	public static CSteamID CurrentLobbyID { get; private set; } = CSteamID.Nil;

	public static bool IsDev { get; private set; }

	public static ESteamNetworkingAvailability RelayStatus { get; private set; } = ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Unknown;

	public static bool IsSteamRelayReady => RelayStatus == ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Current;

	public static string RelayStatusMessage { get; private set; } = string.Empty;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		Callback<LobbyCreated_t>.Create(OnLobbyCreated);
		Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
		Callback<LobbyEnter_t>.Create(OnLobbyEntered);
		Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
		_relayStatusCallback = Callback<SteamRelayNetworkStatus_t>.Create(OnRelayStatusChanged);
		SteamNetworkingUtils.InitRelayNetworkAccess();
		RefreshRelayStatus();
		ulong steamID = SteamUser.GetSteamID().m_SteamID;
		IsDev = steamID == 76561198105374043L || steamID == 76561198058500353L || steamID == 76561197992778568L || steamID == 76561198080340560L || steamID == 76561199534821387L || steamID == 76561198028651102L;
	}

	private void OnDestroy()
	{
		_relayStatusCallback?.Dispose();
		SteamAPI.Shutdown();
	}

	public static bool RefreshRelayStatus()
	{
		try
		{
			RelayStatus = SteamNetworkingUtils.GetRelayNetworkStatus(out var pDetails);
			RelayStatusMessage = pDetails.m_debugMsg;
			LogRelayStatus(pDetails);
			return IsSteamRelayReady;
		}
		catch (Exception ex)
		{
			RelayStatus = ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_CannotTry;
			RelayStatusMessage = ex.Message;
			MonoBehaviour.print($"Could not query Steam Relay status: {ex}");
			return false;
		}
	}

	private void OnRelayStatusChanged(SteamRelayNetworkStatus_t status)
	{
		RelayStatus = status.m_eAvail;
		RelayStatusMessage = status.m_debugMsg;
		LogRelayStatus(status);
	}

	private static void LogRelayStatus(SteamRelayNetworkStatus_t status)
	{
		CanvasManager.OnRelayStatus(status);
	}

	public static void CreateLobby()
	{
		SteamMatchmaking.CreateLobby((SaveManager.CurServerSave == null || SaveManager.CurServerSave.IsPublic) ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePrivate, 8);
	}

	public static void JoinLobby(ulong lobbyID)
	{
		SteamMatchmaking.JoinLobby(new CSteamID(lobbyID));
	}

	public static void JoinFriend(CSteamID friendID)
	{
		if (SteamFriends.GetFriendGamePlayed(friendID, out var pFriendGameInfo) && pFriendGameInfo.m_gameID.AppID() == SteamUtils.GetAppID() && pFriendGameInfo.m_steamIDLobby != CSteamID.Nil)
		{
			SteamMatchmaking.JoinLobby(pFriendGameInfo.m_steamIDLobby);
		}
	}

	public static void LeaveLobby(DisconnectReason reason)
	{
		if (!(CurrentLobbyID == CSteamID.Nil) || (bool)Server.Instance)
		{
			if (CurrentLobbyID != CSteamID.Nil)
			{
				SteamMatchmaking.LeaveLobby(CurrentLobbyID);
				CurrentLobbyID = CSteamID.Nil;
			}
			_disconnectReason = reason;
			ConnectionManager.Instance.LeaveTransport();
		}
	}

	public static bool IsCurrentLobbyMember(CSteamID user)
	{
		if (CurrentLobbyID == CSteamID.Nil || user == CSteamID.Nil)
		{
			return false;
		}
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyID);
		for (int i = 0; i < numLobbyMembers; i++)
		{
			if (SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobbyID, i) == user)
			{
				return true;
			}
		}
		return false;
	}

	private void OnLobbyChatUpdate(LobbyChatUpdate_t param)
	{
		if (CurrentLobbyID == CSteamID.Nil || param.m_ulSteamIDLobby != CurrentLobbyID.m_SteamID)
		{
			return;
		}
		CSteamID cSteamID = new CSteamID(param.m_ulSteamIDUserChanged);
		if (cSteamID == SteamUser.GetSteamID() && (param.m_rgfChatMemberStateChange & 0x1E) != 0)
		{
			LeaveLobby(DisconnectReason.None);
		}
		else if (InstanceFinder.NetworkManager.IsServerStarted)
		{
			if ((param.m_rgfChatMemberStateChange & 0x1E) != 0)
			{
				ConnectionManager.Instance.KickSteamUserIfConnected(cSteamID);
			}
			switch (param.m_rgfChatMemberStateChange)
			{
			}
		}
	}

	private void OnLobbyEntered(LobbyEnter_t callback)
	{
		if (callback.m_EChatRoomEnterResponse != 1)
		{
			uint eChatRoomEnterResponse = callback.m_EChatRoomEnterResponse;
			if (eChatRoomEnterResponse != 2)
			{
				_ = 3;
			}
			return;
		}
		if (CurrentLobbyID != CSteamID.Nil && !InstanceFinder.NetworkManager.IsServerStarted)
		{
			MonoBehaviour.print("trying to enter lobby");
		}
		CSteamID cSteamID = new CSteamID(callback.m_ulSteamIDLobby);
		if (SteamMatchmaking.GetLobbyData(cSteamID, "version") != Application.version)
		{
			SteamMatchmaking.LeaveLobby(cSteamID);
			NetworkPrompts.ShowVersionMismatch();
		}
		else
		{
			CurrentLobbyID = cSteamID;
			ConnectionManager.Instance.JoinOnlineLobby(CurrentLobbyID);
		}
	}

	private void CloseFailedHostLobby(string error)
	{
		Debug.LogError(error);
		if (CurrentLobbyID != CSteamID.Nil)
		{
			SteamMatchmaking.SetLobbyJoinable(CurrentLobbyID, bLobbyJoinable: false);
		}
		LeaveLobby(DisconnectReason.WithUI);
		MainMenuManager.ToggleMenu(enabled: true);
	}

	private void OnLobbyCreated(LobbyCreated_t callback)
	{
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			return;
		}
		CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
		SteamMatchmaking.SetLobbyJoinable(CurrentLobbyID, bLobbyJoinable: false);
		if (!ConnectionManager.Instance.CreateOnlineLobby(8))
		{
			CloseFailedHostLobby($"Steam lobby {CurrentLobbyID.m_SteamID} was created, but the FishySteamworks server failed to start. Closing the lobby.");
			return;
		}
		SteamMatchmaking.SetLobbyData(CurrentLobbyID, "name", SteamFriends.GetPersonaName() + "'s lobby");
		if (!SteamMatchmaking.SetLobbyData(CurrentLobbyID, "version", Application.version) || !SteamMatchmaking.SetLobbyJoinable(CurrentLobbyID, bLobbyJoinable: true))
		{
			CloseFailedHostLobby($"Steam lobby {CurrentLobbyID.m_SteamID} could not publish its metadata or become joinable. Closing the lobby.");
		}
	}

	private void OnJoinRequested(GameLobbyJoinRequested_t callback)
	{
		if (CurrentLobbyID != CSteamID.Nil)
		{
			if (CurrentLobbyID.m_SteamID == callback.m_steamIDLobby.m_SteamID)
			{
				return;
			}
			LeaveLobby(DisconnectReason.None);
		}
		SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
	}
}
