// ============================================================
//  WebGL 单人版兼容层 —— Steamworks 桩实现（Stub）
// ============================================================
//  目的：
//  本工程原本依赖 Steamworks.NET（com.rlabrecque.steamworks.net），
//  但该库底层调用 steam_api64.dll（Windows 原生库），
//  WebGL 平台无法加载，且单人版不需要任何 Steam 功能。
//
//  做法：
//  保留原Steamworks.NET 的 API 签名，用桩实现替代真实调用。
//  所有桩方法返回"空/失败"值，不抛异常，保证游戏逻辑可继续运行。
//  这样就无需逐处修改 85 个 Steam API 调用点。
//
//  若日后要恢复 Steam 联机：
//  删除本文件，并把 PackageCache 中的 com.rlabrecque.steamworks.net
//  及 Plugins/steam_api64.dll 恢复即可。
// ============================================================

using System;
using UnityEngine;

namespace Steamworks
{
	/// <summary>
	/// Steam 网络可用性状态（对齐 Steamworks.NET 原枚举，含 CanvasManager 用到的
	/// Retrying —— 早期桩里漏了这个成员，导致 switch 分支编译不过）
	/// </summary>
	public enum ESteamNetworkingAvailability
	{
		k_ESteamNetworkingAvailability_Unknown = 0,
		k_ESteamNetworkingAvailability_Previously = 1,
		k_ESteamNetworkingAvailability_Failed = 2,
		k_ESteamNetworkingAvailability_NeverTried = 3,
		k_ESteamNetworkingAvailability_Waiting = 4,
		k_ESteamNetworkingAvailability_Retrying = 6,
		k_ESteamNetworkingAvailability_Attempting = 7,
		k_ESteamNetworkingAvailability_Current = 8,
		k_ESteamNetworkingAvailability_CannotTry = 9
	}

	/// <summary>
	/// Steam Relay 网络状态回调。字段名必须是 m_eAvail —— Steamworks 原结构如此，
	/// 早期桩写成了 m_eStatus，与 CanvasManager.OnRelayStatus 的读取不匹配。
	/// </summary>
	public struct SteamRelayNetworkStatus_t
	{
		public ESteamNetworkingAvailability m_eAvail;
	}

	/// <summary>Steam 大厅创建回调</summary>
	public struct LobbyCreated_t
	{
		public ulong m_ulSteamIDLobby;
	}

	/// <summary>好友请求加入回调</summary>
	public struct GameLobbyJoinRequested_t
	{
		public ulong m_ulSteamIDLobby;
		public ulong m_ulSteamIDUser;
		public string m_strJoinDisposition;
	}

	/// <summary>大厅聊天更新回调</summary>
	public struct LobbyChatUpdate_t
	{
		public ulong m_ulSteamIDLobby;
		public ulong m_ulSteamIDUser;
	}

	/// <summary>SteamID 占位类型</summary>
	public struct CSteamID
	{
		public static readonly CSteamID Nil = default;

		public ulong m_SteamID;

		public CSteamID(ulong id)
		{
			m_SteamID = id;
		}

		public bool IsValid()
		{
			return m_SteamID != 0;
		}

		public override string ToString()
		{
			return m_SteamID.ToString();
		}

		public static implicit operator ulong(CSteamID id)
		{
			return id.m_SteamID;
		}

		public static explicit operator CSteamID(ulong id)
		{
			return new CSteamID(id);
		}
	}

	/// <summary>Steam 大厅进入回调</summary>
	public struct LobbyEnter_t
	{
		public ulong m_ulSteamIDLobby;
		public ulong m_ulSteamIDUser;
		public bool m_bEStatus;
	}

	/// <summary>Steam 大厅信息</summary>
	public struct Lobby_t
	{
		public ulong m_ulSteamIDLobby;
	}

	public enum ChatSteamIDInstanceFlags
	{
		k_unSteamIDInstanceAll = 0
	}

	public enum FriendFlags
	{
		None = 0,
		Friend = 1,
		RequestRecipient = 2
	}

	public enum FriendGamePlayedState
	{
		None = 0
	}

	public struct FriendGamePlayed_t
	{
		public CGameInfo m_gameID;
		public CSteamID m_steamIDLobby;
		public CSteamID m_steamIDUser;
	}

	public struct CGameInfo
	{
		public ulong m_gameID;

		public AppId_t AppID()
		{
			return (AppId_t)m_gameID;
		}
	}

	/// <summary>
	/// Steam AppID。类型名必须是 AppId_t —— Steamworks.NET 就是这个拼写，
	/// ButtonManager.WishlistButton 按 new AppId_t(4001890u) 调用。
	/// </summary>
	public struct AppId_t
	{
		public ulong m_AppId;

		public AppId_t(ulong id)
		{
			m_AppId = id;
		}

		public uint AppID()
		{
			return (uint)m_AppId;
		}

		public static implicit operator AppId_t(ulong value)
		{
			return new AppId_t(value);
		}
	}

	/// <summary>Steam 个人资料数据</summary>
	public struct SteamUserStats_t
	{
		public int m_iNumberOfCurrentAchievements;
	}

	/// <summary>Steam 好友信息</summary>
	public struct FriendsInfo_t
	{
		public CSteamID m_SteamIDFriend;
		public string m_strFriendName;
	}

	/// <summary>Steam 好友游戏状态</summary>
	public struct FriendInfo_t
	{
		public CSteamID m_SteamIDFriend;
	}

	public struct MatchmakingLobby_t
	{
		public ulong m_ulSteamIDLobby;
		public CSteamID m_SteamIDOwner;
	}

	/// <summary>用户统计句柄</summary>
	public struct SteamAPICall_t
	{
	}

	public static class SteamUser
	{
		public static CSteamID GetSteamID()
		{
			return CSteamID.Nil;
		}

		public static string GetPersonaName()
		{
			return "Player";
		}

		public static string GetSteamIDAsString()
		{
			return "0";
		}
	}

	public static class SteamUtils
	{
		public static AppId_t GetAppID()
		{
			return default;
		}

		// ---- WebGL 桩：手柄文字输入与UI 语言 ----

		public static uint GetEnteredGamepadTextLength()
		{
			return 0;
		}

		public static bool GetEnteredGamepadTextInput(out string text)
		{
			text = null;
			return false;
		}

		public static bool ShowGamepadTextInput(
			string description = null,
			string existingText = null,
			bool bAlphabetic = true,
			int maxCharacters = 0,
			string acceptedCharSet = null)
		{
			return false;
		}

		public static string GetSteamUILanguage()
		{
			return "english";
		}
	}

	public static class SteamFriends
	{
		public static string GetPersonaName()
		{
			return "Player";
		}

		public static string GetFriendPersonaName(CSteamID friendID)
		{
			return "Friend";
		}

		public static bool GetFriendGamePlayed(CSteamID friendID, out FriendGamePlayed_t info)
		{
			info = default;
			return false;
		}

		public static int GetNumberOfFriends()
		{
			return 0;
		}

		public static bool GetFriendByIndex(int index, out FriendsInfo_t info)
		{
			info = default;
			return false;
		}

		// ---- WebGL 桩：Steam 覆盖层不可用 ----
		// 签名对齐 Steamworks.NET，否则调用方编译不过：
		//   ActivateGameOverlay(string)            —— JoinFriendButton 传的 "Friends"
		//   ActivateGameOverlayInviteDialog(CSteamID)
		//   ActivateGameOverlayToStore(AppId, EOverlayToStoreFlag)

		public static void ActivateGameOverlay()
		{
		}

		public static void ActivateGameOverlay(string pchOverlayName)
		{
		}

		// Steamworks.NET exposes both a callResult form and a convenience form
		// that takes only the interesting argument; the game uses the latter.
		public static bool ActivateGameOverlayInviteDialog(CSteamID steamIDFriend)
		{
			return false;
		}

		public static bool ActivateGameOverlayInviteDialog(SteamAPICall_t callResult, CSteamID steamIDFriend)
		{
			return false;
		}

		public static bool ActivateGameOverlayInviteDialog(SteamAPICall_t callResult, CSteamID steamIDFriend, string pchConnectionMsg)
		{
			return false;
		}

		public static bool ActivateGameOverlayToStore(AppId_t gameID, EOverlayToStoreFlag flag)
		{
			return false;
		}

		public static bool ActivateGameOverlayToStore(SteamAPICall_t callResult, AppId_t gameID, EOverlayToStoreFlag flag)
		{
			return false;
		}
	}

	/// <summary>商店页跳转标志（SteamFriends.ActivateGameOverlayToStore 用）</summary>
	public enum EOverlayToStoreFlag
	{
		k_EOverlayToStoreFlag_None = 0,
		k_EOverlayToStoreFlag_DepositOnly = 1,
		k_EOverlayToStoreFlag_FinalReleaseOnly = 2
	}

	public static class SteamMatchmaking
	{
		public static void JoinLobby(CSteamID lobbyID)
		{
			Debug.Log("[WebGL桩] SteamMatchmaking.JoinLobby 已忽略。");
		}

		public static void LeaveLobby(CSteamID lobbyID)
		{
		}

		public static string GetLobbyData(CSteamID lobbyID, string key)
		{
			return string.Empty;
		}

		public static bool SetLobbyData(CSteamID lobbyID, string key, string value)
		{
			return true;
		}

		public static bool SetLobbyJoinable(CSteamID lobbyID, bool bLobbyJoinable)
		{
			return true;
		}

		public static CSteamID GetLobbyOwner(CSteamID lobbyID)
		{
			return CSteamID.Nil;
		}

		public static void CreateLobby(SteamAPICall_t callResult, bool bPublic, int maxMembers)
		{
		}
	}

	public static class SteamUserStats
	{
		public static bool GetAchievement(string achievementID, out bool achieved)
		{
			achieved = true;
			return true;
		}

		public static bool GetAchievementAndUnlockTime(string achievementID, out bool achieved, out uint unlockTime)
		{
			achieved = true;
			unlockTime = 0;
			return true;
		}

		public static bool SetAchievement(string achievementID)
		{
			return true;
		}

		public static bool StoreStats()
		{
			return true;
		}

		/// <summary>WebGL 桩：成就数量恒为 0</summary>
		public static int GetNumAchievements()
		{
			return 0;
		}

		/// <summary>WebGL 桩：返回占位成就名</summary>
		public static string GetAchievementName(uint index)
		{
			return "ACH_" + index;
		}

		/// <summary>WebGL 桩：返回占位成就描述</summary>
		public static string GetAchievementDescription(uint index)
		{
			return "";
		}

		/// <summary>WebGL 桩：不重置任何统计</summary>
		// 签名对齐 Steamworks.NET：AchievementManager 用命名参数
		// ResetAllStats(bAchievementsToo: true) 调用。
		public static bool ResetAllStats()
		{
			return true;
		}

		public static bool ResetAllStats(bool bAchievementsToo)
		{
			return true;
		}
	}

	/// <summary>Steamworks 设置（WebGL 桩：恒为已初始化）</summary>
	public static class SteamSettings
	{
		public static bool Initialized { get; } = true;

		public static void Init()
		{
		}

		public static void Shutdown()
		{
		}
	}

	/// <summary>Steam 成就 API 桩</summary>
	public static class SteamAchievements
	{
		public const int k_cchDeveloperNameMaxLength = 128;
		public const int k_cchGameNameMaxLength = 128;
		public const int k_cchGameDescriptionMaxLength = 256;
	}

	/// <summary>Steam 客户端句柄桩</summary>
	public static class SteamClient
	{
		public static bool IsValid()
		{
			return false;
		}

		public static void Init(uint appid, bool asyncCallbacks)
		{
		}

		public static void Shutdown()
		{
		}
	}

	/// <summary>Steam 网络身份占位</summary>
	public struct CSteamNetworkingIdentity
	{
		public ulong m_SteamIDRemote;

		public void SetSteamID(ulong steamID)
		{
			m_SteamIDRemote = steamID;
		}
	}
}