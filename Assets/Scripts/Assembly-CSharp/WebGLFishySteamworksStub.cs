// ============================================================
//  WebGL 单人版兼容层 —— FishySteamworks 桩实现
// ============================================================
//  FishySteamworks 是 Steamworks 传输层的封装（FishNet 网络库的
//  Steam 传输实现），底层调用 steam_api64.dll，WebGL 无法加载。
//
//  原实现已随 Steam 相关代码一并移出，但 ConnectionManager 上
//  仍留有一个 [SerializeField] 字段声明：
//
//      [SerializeField]
//      private global::FishySteamworks.FishySteamworks _fishy;
//
//  该字段在 WebGL 单人版中从不被读写（IsUsingSteam 恒为 false，
//  SetTransport 固定走 UnityTransport），保留它只是为了让
//  ConnectionManager 的源码无需改动。
//
//  这里只提供类型本身，不提供任何传输能力。
//
//  若日后要恢复 Steam 联机：删除本文件，放回
//  Assets/Plugins/Assembly-CSharp-firstpass/FishySteamworks/ 即可。
// ============================================================

using UnityEngine;

namespace FishySteamworks
{
	/// <summary>
	/// Steam 传输层占位实现。UnityTransport 才是 WebGL 版实际使用的传输层
	/// —— 它由 Assets/Plugins/FishyUnityTransport.dll 提供，基于
	/// com.unity.transport，并带 WebSocket 支持（浏览器里 UDP 不可用）。
	/// 保留此类型仅为满足 ConnectionManager 的字段声明与场景反序列化。
	/// </summary>
	public class FishySteamworks : MonoBehaviour
	{
		// 原实现继承 FishNet 的 NetworkTransport，并覆写 Initialize /
		// GetCurrentConnectivity 等方法。此处不实现任何传输逻辑，
		// 因为 WebGL 单人版不会把它设为 NetworkManager 的 transport。

		private void Awake()
		{
			Debug.LogWarning(
				"[FishySteamworks] 桩实现被加载。WebGL 单人版不使用 Steam 传输层，" +
				"当前传输方式为 UnityTransport（dll 提供，非源码），此实例不会被使用。");
		}
	}
}
