using System;
using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Localization;

public class NPCManager : NetworkBehaviour
{
	public static NPCManager Instance;

	private static Dictionary<byte, NPC> _idToNpc = new Dictionary<byte, NPC>();

	public readonly SyncDictionary<byte, byte> _idToShowingQuestIndex = new SyncDictionary<byte, byte>();

	public readonly SyncVar<bool> _grillUnlocked = new SyncVar<bool>();

	public readonly SyncVar<bool> _finalBossKilled = new SyncVar<bool>();

	private byte _curRandomLine;

	private bool NetworkInitialize___EarlyNPCManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateNPCManagerAssembly_002DCSharp_002Edll_Excuted;

	public bool GrillUnlocked => _grillUnlocked.Value;

	public bool FinalBossKilled => _finalBossKilled.Value;

	public static event Action OnUnlockedGrill;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_NPCManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public static void UnlockGrill()
	{
		if ((bool)Instance && Instance.IsServerInitialized)
		{
			Instance._grillUnlocked.Value = true;
		}
	}

	public static void SetFinalBossKilled()
	{
		if ((bool)Instance && Instance.IsServerInitialized)
		{
			Instance._finalBossKilled.Value = true;
		}
	}

	public override void OnStartServer()
	{
		foreach (KeyValuePair<byte, NPC> item in _idToNpc)
		{
			_idToShowingQuestIndex.Add(item.Key, byte.MaxValue);
		}
		if (SaveManager.CurServerSave != null)
		{
			_grillUnlocked.Value = SaveManager.CurServerSave.UnlockedGrill;
			_finalBossKilled.Value = SaveManager.CurServerSave.FinalBossKilled;
		}
	}

	public override void OnStopClient()
	{
		_idToNpc.Clear();
	}

	public static void AddNpc(NPC npc)
	{
		_idToNpc.TryAdd(npc.ID, npc);
		if ((bool)Instance && Instance.IsServerInitialized)
		{
			Instance._idToShowingQuestIndex[npc.ID] = byte.MaxValue;
		}
	}

	public static void RemoveNpc(NPC npc)
	{
		if (_idToNpc.ContainsKey(npc.ID))
		{
			_idToNpc.Remove(npc.ID);
		}
		if ((bool)Instance && Instance.IsServerInitialized && Instance._idToShowingQuestIndex.ContainsKey(npc.ID))
		{
			Instance._idToShowingQuestIndex.Remove(npc.ID);
		}
	}

	public void ServerOnClientSpokeToNpc(byte npcID)
	{
		NPC nPC = _idToNpc[npcID];
		if ((bool)nPC && nPC.Quests.Count != 0)
		{
			byte questIndex = nPC.GetQuestIndex();
			if ((bool)nPC.Quests[questIndex])
			{
				bool flag = NpcIsHoldingItem(npcID);
				bool flag2 = nPC.Quests[questIndex].Type == QuestType.UnlockGrill && GrillUnlocked;
				LocalizedString[] array = (flag ? nPC.Quests[questIndex].HoldingItemLines : (flag2 ? nPC.Quests[questIndex].AlreadyCompletedLines : nPC.Quests[questIndex].Lines));
				byte lineIndex = nPC.GetLineIndex(array.Length);
				byte serverProgression = nPC.GetServerProgression(questIndex);
				ObserverSpeak(npcID, questIndex, lineIndex, serverProgression, (byte)(flag ? 3 : (flag2 ? 4 : 0)));
			}
		}
	}

	public void ServerSpeak(byte npcID, byte questIndex, byte progression, QuestLineType lineType)
	{
		if (base.IsServerInitialized)
		{
			NPC nPC = _idToNpc[npcID];
			LocalizedString[] array = lineType switch
			{
				QuestLineType.Default => nPC.Quests[questIndex].Lines, 
				QuestLineType.ItemReceived => nPC.Quests[questIndex].OnItemReceivedLines, 
				QuestLineType.OnCompleted => nPC.Quests[questIndex].OnQuestCompletedLines, 
				QuestLineType.HoldingItem => nPC.Quests[questIndex].HoldingItemLines, 
				_ => Array.Empty<LocalizedString>(), 
			};
			byte b = (byte)UnityEngine.Random.Range(0, array.Length);
			if (b == _curRandomLine)
			{
				b++;
			}
			if (b >= array.Length)
			{
				b = 0;
			}
			_curRandomLine = b;
			ObserverSpeak(npcID, questIndex, b, progression, (byte)lineType);
		}
	}

	public void SetShowingNpcQuest(byte npcID, byte questIndex)
	{
		if (base.IsServerInitialized)
		{
			_idToShowingQuestIndex[npcID] = questIndex;
		}
	}

	[ObserversRpc]
	private void ObserverSpeak(byte npcID, byte questIndex, byte lineIndex, byte progression, byte lineType)
	{
		RpcWriter___ObserverSpeak___3650919882(npcID, questIndex, lineIndex, progression, lineType);
	}

	public void ResetItemHeld(byte npcID)
	{
		if (base.IsServerInitialized)
		{
			_idToShowingQuestIndex[npcID] = byte.MaxValue;
		}
	}

	private void OnShowingQuestIndexChange(SyncDictionaryOperation op, byte key, byte value, bool asServer)
	{
		if (!asServer && _idToNpc.TryGetValue(key, out var value2) && (bool)value2)
		{
			value2.OnShowingQuestIndexChange(value);
		}
	}

	public bool NpcIsHoldingItem(byte npcID)
	{
		if (_idToShowingQuestIndex.ContainsKey(npcID))
		{
			return _idToShowingQuestIndex[npcID] != byte.MaxValue;
		}
		return false;
	}

	public NPCQuest GetShowingQuest(byte npcID)
	{
		return _idToNpc[npcID].Quests[_idToShowingQuestIndex[npcID]];
	}

	public void SendEatEffects(byte id)
	{
		if (base.IsServerInitialized && (bool)_idToNpc[id])
		{
			ObserverEatEffects(id);
		}
	}

	public static NPC IDToNpc(byte id)
	{
		return _idToNpc[id];
	}

	[ObserversRpc]
	private void ObserverEatEffects(byte id)
	{
		RpcWriter___ObserverEatEffects___1246646286(id);
	}

	private void OnGrillUnlockedChange(bool prev, bool next, bool asServer)
	{
		if (!asServer && next)
		{
			AchievementManager.CheckGrillmasterAchievement();
			OnUnlockedGrill?.Invoke();
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyNPCManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyNPCManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_finalBossKilled.InitializeEarly(this, 2u, isSyncObject: false);
			_grillUnlocked.InitializeEarly(this, 1u, isSyncObject: false);
			_idToShowingQuestIndex.InitializeEarly(this, 0u, isSyncObject: true);
			RegisterObserversRpc(0u, RpcReader___ObserverSpeak___3650919882);
			RegisterObserversRpc(1u, RpcReader___ObserverEatEffects___1246646286);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateNPCManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateNPCManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_finalBossKilled.InitializeLate();
			_grillUnlocked.InitializeLate();
			_idToShowingQuestIndex.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverSpeak___3650919882(byte npcID, byte questIndex, byte lineIndex, byte progression, byte lineType)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(npcID);
		pooledWriter.WriteUInt8Unpacked(questIndex);
		pooledWriter.WriteUInt8Unpacked(lineIndex);
		pooledWriter.WriteUInt8Unpacked(progression);
		pooledWriter.WriteUInt8Unpacked(lineType);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverSpeak___3650919882(byte P_0, byte P_1, byte P_2, byte P_3, byte P_4)
	{
		if (_idToNpc.TryGetValue(P_0, out var value))
		{
			value.SetNpcText(P_1, P_2, P_3, (QuestLineType)P_4);
		}
	}

	private void RpcReader___ObserverSpeak___3650919882(PooledReader PooledReader0, Channel channel)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		byte b2 = PooledReader0.ReadUInt8Unpacked();
		byte b3 = PooledReader0.ReadUInt8Unpacked();
		byte b4 = PooledReader0.ReadUInt8Unpacked();
		byte b5 = PooledReader0.ReadUInt8Unpacked();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverSpeak___3650919882(b, b2, b3, b4, b5);
		}
	}

	private void RpcWriter___ObserverEatEffects___1246646286(byte id)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(id);
		SendObserversRpc(1u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverEatEffects___1246646286(byte P_0)
	{
		NPC nPC = _idToNpc[P_0];
		if ((bool)nPC)
		{
			nPC.StartEatEffects();
		}
	}

	private void RpcReader___ObserverEatEffects___1246646286(PooledReader PooledReader0, Channel channel)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverEatEffects___1246646286(b);
		}
	}

	private void Awake_UserLogic_NPCManager_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
		_grillUnlocked.OnChange += OnGrillUnlockedChange;
		_idToShowingQuestIndex.OnChange += OnShowingQuestIndexChange;
	}
}
