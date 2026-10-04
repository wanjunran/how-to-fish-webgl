// ============================================================
//  WebGL 单人版替代实现 —— SteamManager
// ============================================================
//  原 SteamManager 负责：Steam 大厅创建/加入、好友邀请、Relay 状态、
//  Steam 身份鉴权。全部依赖 Steamworks + FishySteamworks（Windows 原生库），
//  WebGL 平台无法加载。
//
//  单人版策略：
//  保留同名类与完全相同的公开 API（MainMenuManager、ChatManager、
//  Server、PlayerUI 等 20+ 个文件都在调用它们），实现改为安全的空实现，
//  上层代码无需改动即可编译通过。
//
//  签名依据：原文件公开成员清单
//    CurrentLobbyID / IsDev / RelayStatus / IsSteamRelayReady
//    RelayStatusMessage / RefreshRelayStatus / CreateLobby / JoinLobby
//    JoinFriend / LeaveLobby(DisconnectReason) / IsCurrentLobbyMember
//
//  若日后恢复 Steam 联机：删除本文件，放回原 SteamManager.cs 即可。
// ============================================================

using System;
using Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
	private static SteamManager _instance;

	// ---- 保留原 API 签名 ----

	/// <summary>WebGL 版：永远无 Steam 大厅</summary>
	public static CSteamID CurrentLobbyID { get; private set; } = CSteamID.Nil;

	/// <summary>WebGL 版：非开发构建</summary>
	public static bool IsDev { get; private set; } = false;

	/// <summary>WebGL 版：Relay 恒为不可用</summary>
	public static ESteamNetworkingAvailability RelayStatus { get; private set; } =
		ESteamNetworkingAvailability.k_ESteamNetworkingAvailability_Unknown;

	public static bool IsSteamRelayReady => false;

	public static string RelayStatusMessage { get; private set; } = string.Empty;

	private void Awake()
	{
		_instance = this;
		Debug.Log("[WebGL] SteamManager 已加载为单人版空实现（Steam 联机不可用）。");
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}

	// ---- 以下为原 API 的空实现，签名与原文件保持一致 ----

	/// <summary>WebGL 版：无法刷新 Relay 状态</summary>
	public static bool RefreshRelayStatus()
	{
		return false;
	}

	/// <summary>WebGL 版：Steam 大厅不可用</summary>
	public static void CreateLobby()
	{
		Debug.LogWarning("[WebGL] Steam 大厅功能已禁用。");
	}

	/// <summary>WebGL 版：Steam 大厅不可用</summary>
	public static void JoinLobby(ulong lobbyID)
	{
		Debug.LogWarning("[WebGL] Steam 大厅功能已禁用。");
	}

	/// <summary>WebGL 版：好友邀请不可用</summary>
	public static void JoinFriend(CSteamID friendID)
	{
		Debug.LogWarning("[WebGL] Steam 好友邀请已禁用。");
	}

	/// <summary>WebGL 版：直接清空大厅状态</summary>
	public static void LeaveLobby(DisconnectReason reason)
	{
		CurrentLobbyID = CSteamID.Nil;
	}

	/// <summary>WebGL 版：单人模式不校验大厅成员</summary>
	public static bool IsCurrentLobbyMember(CSteamID user)
	{
		return true;
	}
}