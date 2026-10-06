using System;
using System.Collections;
using FishNet.Authenticating;
using FishNet.Connection;
using Steamworks;
using UnityEngine;

public sealed class SteamLobbyAuthenticator : Authenticator
{
	private const float LobbyMembershipTimeout = 2f;

	public override event Action<NetworkConnection, bool> OnAuthenticationResult;

	public override void OnRemoteConnection(NetworkConnection connection)
	{
		if (!ConnectionManager.IsUsingSteam)
		{
			OnAuthenticationResult?.Invoke(connection, arg2: true);
		}
		else
		{
			StartCoroutine(AuthenticateSteamLobbyMember(connection));
		}
	}

	private IEnumerator AuthenticateSteamLobbyMember(NetworkConnection connection)
	{
		if (!ulong.TryParse(connection.GetAddress(), out var steamIDValue) || steamIDValue == 0L)
		{
			Reject(connection, $"Connection {connection.ClientId} did not provide a valid SteamID address.");
			yield break;
		}
		CSteamID steamID = new CSteamID(steamIDValue);
		float timeoutAt = Time.unscaledTime + 2f;
		do
		{
			if (!connection.IsActive)
			{
				yield break;
			}
			if (SteamManager.IsCurrentLobbyMember(steamID))
			{
				OnAuthenticationResult?.Invoke(connection, arg2: true);
				yield break;
			}
			yield return null;
		}
		while (Time.unscaledTime < timeoutAt);
		Reject(connection, $"Steam user {steamIDValue} was rejected because they are not a member of lobby {SteamManager.CurrentLobbyID.m_SteamID}.");
	}

	private void Reject(NetworkConnection connection, string message)
	{
		Debug.LogWarning(message);
		OnAuthenticationResult?.Invoke(connection, arg2: false);
	}
}
