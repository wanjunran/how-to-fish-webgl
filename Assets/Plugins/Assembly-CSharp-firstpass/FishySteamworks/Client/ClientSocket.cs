using System;
using System.Diagnostics;
using System.Threading;
using FishNet.Managing;
using FishNet.Transporting;
using Steamworks;
using UnityEngine;

namespace FishySteamworks.Client
{
	public class ClientSocket : CommonSocket
	{
		private Callback<SteamNetConnectionStatusChangedCallback_t> _onLocalConnectionStateCallback;

		private CSteamID _hostSteamID = CSteamID.Nil;

		private HSteamNetConnection _socket;

		private Thread _timeoutThread;

		private float _connectTimeout = -1f;

		private const float CONNECT_TIMEOUT_DURATION = 8000f;

		private void CheckTimeout()
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			do
			{
				if ((float)(stopwatch.ElapsedMilliseconds / 1000) > _connectTimeout)
				{
					StopConnection();
				}
				Thread.Sleep(50);
			}
			while (GetLocalConnectionState() == LocalConnectionState.Starting);
			stopwatch.Stop();
			_timeoutThread.Abort();
		}

		internal bool StartConnection(string address, ushort port, bool peerToPeer)
		{
			try
			{
				if (_onLocalConnectionStateCallback == null)
				{
					_onLocalConnectionStateCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnLocalConnectionState);
				}
				PeerToPeer = peerToPeer;
				byte[] array = ((!peerToPeer) ? GetIPBytes(address) : null);
				if (!peerToPeer && array == null)
				{
					base.SetLocalConnectionState(LocalConnectionState.Stopped, server: false);
					return false;
				}
				base.SetLocalConnectionState(LocalConnectionState.Starting, server: false);
				_connectTimeout = Time.unscaledTime + 8000f;
				_timeoutThread = new Thread(CheckTimeout);
				_timeoutThread.Start();
				_hostSteamID = new CSteamID(ulong.Parse(address));
				SteamNetworkingIdentity identityRemote = default;
				identityRemote.SetSteamID(_hostSteamID);
				SteamNetworkingConfigValue_t[] array2 = new SteamNetworkingConfigValue_t[0];
				if (PeerToPeer)
				{
					_socket = SteamNetworkingSockets.ConnectP2P(ref identityRemote, 0, array2.Length, array2);
				}
				else
				{
					SteamNetworkingIPAddr address2 = default;
					address2.Clear();
					address2.SetIPv6(array, port);
					_socket = SteamNetworkingSockets.ConnectByIPAddress(ref address2, 0, array2);
				}
			}
			catch
			{
				base.SetLocalConnectionState(LocalConnectionState.Stopped, server: false);
				return false;
			}
			return true;
		}

		private void OnLocalConnectionState(SteamNetConnectionStatusChangedCallback_t args)
		{
			if (args.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
			{
				base.SetLocalConnectionState(LocalConnectionState.Started, server: false);
			}
			else if (args.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer || args.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
			{
				UnityEngine.Debug.LogError($"Steam connection ended. State: {args.m_info.m_eState}, reason: {args.m_info.m_eEndReason}, debug: {args.m_info.m_szEndDebug}");
				StopConnection();
			}
			else
			{
				Transport.NetworkManager.Log("Connection state changed: " + args.m_info.m_eState.ToString() + " - " + args.m_info.m_szEndDebug);
			}
		}

		internal bool StopConnection()
		{
			if (_timeoutThread != null && _timeoutThread.IsAlive)
			{
				_timeoutThread.Abort();
			}
			if (_socket != HSteamNetConnection.Invalid)
			{
				if (_onLocalConnectionStateCallback != null)
				{
					_onLocalConnectionStateCallback.Dispose();
					_onLocalConnectionStateCallback = null;
				}
				SteamNetworkingSockets.CloseConnection(_socket, 0, string.Empty, bEnableLinger: false);
				_socket = HSteamNetConnection.Invalid;
			}
			if (GetLocalConnectionState() == LocalConnectionState.Stopped || GetLocalConnectionState() == LocalConnectionState.Stopping)
			{
				return false;
			}
			base.SetLocalConnectionState(LocalConnectionState.Stopping, server: false);
			base.SetLocalConnectionState(LocalConnectionState.Stopped, server: false);
			return true;
		}

		internal void IterateIncoming()
		{
			if (GetLocalConnectionState() != LocalConnectionState.Started)
			{
				return;
			}
			int num = SteamNetworkingSockets.ReceiveMessagesOnConnection(_socket, MessagePointers, 256);
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					GetMessage(MessagePointers[i], InboundBuffer, out var segment, out var channel);
					Transport.HandleClientReceivedDataArgs(new ClientReceivedDataArgs(segment, (Channel)channel, Transport.Index));
				}
			}
		}

		internal void SendToServer(byte channelId, ArraySegment<byte> segment)
		{
			if (GetLocalConnectionState() == LocalConnectionState.Started)
			{
				EResult eResult = Send(_socket, segment, channelId);
				switch (eResult)
				{
				case EResult.k_EResultNoConnection:
				case EResult.k_EResultInvalidParam:
					Transport.NetworkManager.Log("Connection to server was lost.");
					StopConnection();
					break;
				default:
					Transport.NetworkManager.LogError("Could not send: " + eResult);
					break;
				case EResult.k_EResultOK:
					break;
				}
			}
		}

		internal void IterateOutgoing()
		{
			if (GetLocalConnectionState() == LocalConnectionState.Started)
			{
				SteamNetworkingSockets.FlushMessagesOnConnection(_socket);
			}
		}
	}
}
