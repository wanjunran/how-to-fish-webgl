using System;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Logging;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using Steamworks;
using Unity.Mathematics;
using UnityEngine;

public class Server : NetworkBehaviour
{
	[SerializeField]
	private Transform _dynamicObjectsHolder;

	private bool NetworkInitialize___EarlyServerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateServerAssembly_002DCSharp_002Edll_Excuted;

	public static Server Instance { get; private set; }

	public int GunsBought { get; private set; }

	public Transform DynamicObjectsHolder => _dynamicObjectsHolder;

	public static event Action OnServerStarted;

	public static event Action OnServerStopped;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Server_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartClient()
	{
		base.TimeManager.OnTick += WaterManager.OnTickUpdate;
		OnServerStarted?.Invoke();
	}

	public override void OnStopClient()
	{
		base.TimeManager.OnTick -= WaterManager.OnTickUpdate;
		EndGameManager.DisableEndGameStuff();
		PauseManager.TogglePause(to: false);
		MainMenuManager.ToggleMenu(enabled: true);
		Instance = null;
		OnServerStopped?.Invoke();
	}

	public override void OnStopServer()
	{
		SaveManager.SaveServer();
	}

	[ServerRpc(RequireOwnership = false)]
	public void SpawnPlayer(Vector3 skinColor, Vector3 outfitColor, Vector3 outfitColor2, Vector3 outfitColor3, Vector3 hatColor, Vector3 hatColor2, Vector3 hatColor3, Vector3 accessoryColor, Vector3 accessoryColor2, Vector3 accessoryColor3, byte hatIndex, byte outfitIndex, byte accessoryIndex, bool isBean, NetworkConnection sender = null)
	{
		RpcWriter___SpawnPlayer___1871804056(skinColor, outfitColor, outfitColor2, outfitColor3, hatColor, hatColor2, hatColor3, accessoryColor, accessoryColor2, accessoryColor3, hatIndex, outfitIndex, accessoryIndex, isBean, sender);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ResurrectPlayer(Player player, DeadPlayer deadPlayer)
	{
		RpcWriter___ResurrectPlayer___2247010027(player, deadPlayer);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateDeadPlayerResurrectPercent(DeadPlayer deadPlayer, float resurrectPercent, Channel channel = Channel.Unreliable)
	{
		RpcWriter___UpdateDeadPlayerResurrectPercent___3918731242(deadPlayer, resurrectPercent, channel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void RespawnPlayer(Player player, Vector3 deathPos, Quaternion deathRot)
	{
		RpcWriter___RespawnPlayer___2210451296(player, deathPos, deathRot);
	}

	[ServerRpc(RequireOwnership = false)]
	public void TeleportPlayer(Player player, Vector3 pos, float rot)
	{
		RpcWriter___TeleportPlayer___3852204532(player, pos, rot);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetItemHolder(Item item, Player player, Item replacedItem)
	{
		RpcWriter___SetItemHolder___3876263354(item, player, replacedItem);
	}

	[TargetRpc]
	private void TargetReconcileRejectedItemPickup(NetworkConnection connection, Item attemptedItem, Item authoritativeHeldItem)
	{
		RpcWriter___TargetReconcileRejectedItemPickup___3695777406(connection, attemptedItem, authoritativeHeldItem);
	}

	[ServerRpc(RequireOwnership = false)]
	public void HandOverItemSimulation(Item item)
	{
		RpcWriter___HandOverItemSimulation___2875874600(item);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateItemPosRot(Item item, NetworkConnection netCon, Vector3 pos, Quaternion rot, bool onBoat, float[] extraHingeAngles = null, Quaternion[] extraHingeRots = null, Channel channel = Channel.Unreliable)
	{
		RpcWriter___UpdateItemPosRot___3673846853(item, netCon, pos, rot, onBoat, extraHingeAngles, extraHingeRots, channel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetSyncedSimulator(Item item, NetworkConnection newSimulator)
	{
		RpcWriter___SetSyncedSimulator___1559681479(item, newSimulator);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateHeldToolPosRot(Tool tool, Vector3 pos, Quaternion rot, Channel channel = Channel.Unreliable)
	{
		RpcWriter___UpdateHeldToolPosRot___3161606994(tool, pos, rot, channel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdatePlayerPosRot(Player player, Vector3 pos, Vector2 rot, bool onBoat = false, bool teleport = false, Channel channel = Channel.Unreliable)
	{
		RpcWriter___UpdatePlayerPosRot___2142082744(player, pos, rot, onBoat, teleport, channel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdatePlayerCrouching(Player player, bool isCrouching, Channel channel = Channel.Unreliable)
	{
		RpcWriter___UpdatePlayerCrouching___1562565462(player, isCrouching, channel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SelectInvSlot(Player player, int slot)
	{
		RpcWriter___SelectInvSlot___3709775007(player, slot);
	}

	[ServerRpc(RequireOwnership = false)]
	public void PutItemInInventory(Player player, Item item, byte slot)
	{
		RpcWriter___PutItemInInventory___3873472972(player, item, slot);
	}

	[ServerRpc(RequireOwnership = false)]
	public void RemoveItemFromInventory(Player player, Item item)
	{
		RpcWriter___RemoveItemFromInventory___241039439(player, item);
	}

	[ServerRpc(RequireOwnership = false)]
	public void DropAllItems(Player player, Vector3 pos, Quaternion rot)
	{
		RpcWriter___DropAllItems___2210451296(player, pos, rot);
	}

	[ServerRpc(RequireOwnership = false)]
	public void PlayImpactSound(Item item, byte vel, Channel channel = Channel.Unreliable)
	{
		RpcWriter___PlayImpactSound___413717140(item, vel, channel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateBaitPosAndLineLength(FishingRod rod, Vector3 baitPos, int curLineLengthMulti, float curBaitForce, Channel channel = Channel.Unreliable)
	{
		RpcWriter___UpdateBaitPosAndLineLength___2349636148(rod, baitPos, curLineLengthMulti, curBaitForce, channel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ReleaseItemFromBait(FishingRod rod)
	{
		RpcWriter___ReleaseItemFromBait___2725923288(rod);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateRodPullBack(FishingRod rod, float rot, bool additionalState, Channel channel = Channel.Unreliable)
	{
		RpcWriter___UpdateRodPullBack___2928850743(rod, rot, additionalState, channel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void HitCreature(Creature creature, Player playerWhoHit, int damage, Vector3 hitPoint, Vector3 dir)
	{
		RpcWriter___HitCreature___215526726(creature, playerWhoHit, damage, hitPoint, dir);
	}

	[ServerRpc(RequireOwnership = false)]
	public void AddProjectile(Player owner, WeaponInfo weapon, uint tick, uint id, Vector3 pos, Vector3 vel)
	{
		RpcWriter___AddProjectile___3746676724(owner, weapon, tick, id, pos, vel);
	}

	[ServerRpc(RequireOwnership = false)]
	public void AddProjectiles(Player owner, WeaponInfo weapon, uint tick, uint id, Vector3 pos, Vector3[] velocities)
	{
		RpcWriter___AddProjectiles___1894401688(owner, weapon, tick, id, pos, velocities);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ProjectileHitDynamic(NetworkConnection netCon, uint id)
	{
		RpcWriter___ProjectileHitDynamic___353006558(netCon, id);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ReloadWeapon(Weapon weapon)
	{
		RpcWriter___ReloadWeapon___1996094309(weapon);
	}

	[ServerRpc(RequireOwnership = false)]
	public void BuyItem(byte itemID, Player player, Item replacedItem, Vector3 pos, Quaternion rot, bool isFree = false)
	{
		RpcWriter___BuyItem___4197152275(itemID, player, replacedItem, pos, rot, isFree);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UnlockPocket(Player player, byte slotIndex)
	{
		RpcWriter___UnlockPocket___1382779195(player, slotIndex);
	}

	[ServerRpc(RequireOwnership = false)]
	public void BuyBait(Player player, byte baitIndex, int cost)
	{
		RpcWriter___BuyBait___4169050770(player, baitIndex, cost);
	}

	[ServerRpc(RequireOwnership = false)]
	public void TakeItemFromNpc(Player player, byte npcID)
	{
		RpcWriter___TakeItemFromNpc___1382779195(player, npcID);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ChangeBait(Player player, byte to)
	{
		RpcWriter___ChangeBait___1382779195(player, to);
	}

	[ServerRpc(RequireOwnership = false)]
	public void PlaceBet(byte betColor)
	{
		RpcWriter___PlaceBet___1246646286(betColor);
	}

	[ServerRpc(RequireOwnership = false)]
	public void UpdateRoulette(Vector3 ballPos, float wheelRot)
	{
		RpcWriter___UpdateRoulette___3661469815(ballPos, wheelRot);
	}

	[ServerRpc(RequireOwnership = false)]
	public void Punch(Player player, Transform target, bool right, Vector3 targetHitPoint)
	{
		RpcWriter___Punch___1201658259(player, target, right, targetHitPoint);
	}

	[ServerRpc(RequireOwnership = false)]
	public void MeleeAttack(Melee weapon, Transform target, bool right, Vector3 targetHitPoint)
	{
		RpcWriter___MeleeAttack___1137909598(weapon, target, right, targetHitPoint);
	}

	[ServerRpc(RequireOwnership = false)]
	public void InspectTool(Tool tool)
	{
		RpcWriter___InspectTool___625650179(tool);
	}

	[ServerRpc(RequireOwnership = false)]
	public void HitPlayer(Player player, int damage, Vector3 force, Vector3 pos = default(Vector3), byte damageType = 1, Player playerWhoHit = null)
	{
		RpcWriter___HitPlayer___2449261505(player, damage, force, pos, damageType, playerWhoHit);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ActivateExplosive(Explosive explosive, uint tick, bool forced = false, bool instant = false, Player playerWhoForced = null)
	{
		RpcWriter___ActivateExplosive___1390675745(explosive, tick, forced, instant, playerWhoForced);
	}

	[ServerRpc(RequireOwnership = false)]
	public void GrillItemInLava(Item item)
	{
		RpcWriter___GrillItemInLava___2875874600(item);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetItemSkin(Item item, byte toIndex)
	{
		RpcWriter___SetItemSkin___2384767125(item, toIndex);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetBoatSkin(byte to)
	{
		RpcWriter___SetBoatSkin___1246646286(to);
	}

	[ServerRpc(RequireOwnership = false)]
	public void FinishEatingCreature(Creature creature, Player player)
	{
		RpcWriter___FinishEatingCreature___1039939981(creature, player);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ToggleEatCreature(Player player, bool isEating)
	{
		RpcWriter___ToggleEatCreature___2104815579(player, isEating);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetItemMultiplier(Item item, float multiplier)
	{
		RpcWriter___SetItemMultiplier___755958659(item, multiplier);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ClientSpokeToNpc(byte npcID)
	{
		RpcWriter___ClientSpokeToNpc___1246646286(npcID);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetRadioFrequency(Radio radio, float frequency)
	{
		RpcWriter___SetRadioFrequency___1599952821(radio, frequency);
	}

	[ServerRpc(RequireOwnership = false)]
	public void BuyAttachment(Weapon weapon, byte attachmentIndex)
	{
		RpcWriter___BuyAttachment___2937188526(weapon, attachmentIndex);
	}

	[ServerRpc(RequireOwnership = false)]
	public void BuyBulletUpgrade(Weapon weapon)
	{
		RpcWriter___BuyBulletUpgrade___1996094309(weapon);
	}

	[ServerRpc(RequireOwnership = false)]
	public void BuySharpnessUpgrade(Melee melee)
	{
		RpcWriter___BuySharpnessUpgrade___680780043(melee);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendChatMessage(ulong from, string message)
	{
		RpcWriter___SendChatMessage___3264264606(from, message);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetDriver(Player driver)
	{
		RpcWriter___SetDriver___3849956746(driver);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendBoatInput(half x, half y)
	{
		RpcWriter___SendBoatInput___1949707525(x, y);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SetIsAfk(Player player, bool isAfk, bool fromPause = true)
	{
		RpcWriter___SetIsAfk___3651916454(player, isAfk, fromPause);
	}

	[ServerRpc(RequireOwnership = false)]
	public void BuyBoatMotor(Player player, byte motorIndex, int cost)
	{
		RpcWriter___BuyBoatMotor___4169050770(player, motorIndex, cost);
	}

	[ServerRpc(RequireOwnership = false)]
	public void BuyBoatRadar(Player player, int cost)
	{
		RpcWriter___BuyBoatRadar___3709775007(player, cost);
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendFinishGame()
	{
		RpcWriter___SendFinishGame___2166136261();
	}

	[ServerRpc(RequireOwnership = false)]
	public void SendFinishedTutorial(Player player)
	{
		RpcWriter___SendFinishedTutorial___3849956746(player);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyServerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyServerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterServerRpc(0u, RpcReader___SpawnPlayer___1871804056);
			RegisterServerRpc(1u, RpcReader___ResurrectPlayer___2247010027);
			RegisterServerRpc(2u, RpcReader___UpdateDeadPlayerResurrectPercent___3918731242);
			RegisterServerRpc(3u, RpcReader___RespawnPlayer___2210451296);
			RegisterServerRpc(4u, RpcReader___TeleportPlayer___3852204532);
			RegisterServerRpc(5u, RpcReader___SetItemHolder___3876263354);
			RegisterTargetRpc(6u, RpcReader___TargetReconcileRejectedItemPickup___3695777406);
			RegisterServerRpc(7u, RpcReader___HandOverItemSimulation___2875874600);
			RegisterServerRpc(8u, RpcReader___UpdateItemPosRot___3673846853);
			RegisterServerRpc(9u, RpcReader___SetSyncedSimulator___1559681479);
			RegisterServerRpc(10u, RpcReader___UpdateHeldToolPosRot___3161606994);
			RegisterServerRpc(11u, RpcReader___UpdatePlayerPosRot___2142082744);
			RegisterServerRpc(12u, RpcReader___UpdatePlayerCrouching___1562565462);
			RegisterServerRpc(13u, RpcReader___SelectInvSlot___3709775007);
			RegisterServerRpc(14u, RpcReader___PutItemInInventory___3873472972);
			RegisterServerRpc(15u, RpcReader___RemoveItemFromInventory___241039439);
			RegisterServerRpc(16u, RpcReader___DropAllItems___2210451296);
			RegisterServerRpc(17u, RpcReader___PlayImpactSound___413717140);
			RegisterServerRpc(18u, RpcReader___UpdateBaitPosAndLineLength___2349636148);
			RegisterServerRpc(19u, RpcReader___ReleaseItemFromBait___2725923288);
			RegisterServerRpc(20u, RpcReader___UpdateRodPullBack___2928850743);
			RegisterServerRpc(21u, RpcReader___HitCreature___215526726);
			RegisterServerRpc(22u, RpcReader___AddProjectile___3746676724);
			RegisterServerRpc(23u, RpcReader___AddProjectiles___1894401688);
			RegisterServerRpc(24u, RpcReader___ProjectileHitDynamic___353006558);
			RegisterServerRpc(25u, RpcReader___ReloadWeapon___1996094309);
			RegisterServerRpc(26u, RpcReader___BuyItem___4197152275);
			RegisterServerRpc(27u, RpcReader___UnlockPocket___1382779195);
			RegisterServerRpc(28u, RpcReader___BuyBait___4169050770);
			RegisterServerRpc(29u, RpcReader___TakeItemFromNpc___1382779195);
			RegisterServerRpc(30u, RpcReader___ChangeBait___1382779195);
			RegisterServerRpc(31u, RpcReader___PlaceBet___1246646286);
			RegisterServerRpc(32u, RpcReader___UpdateRoulette___3661469815);
			RegisterServerRpc(33u, RpcReader___Punch___1201658259);
			RegisterServerRpc(34u, RpcReader___MeleeAttack___1137909598);
			RegisterServerRpc(35u, RpcReader___InspectTool___625650179);
			RegisterServerRpc(36u, RpcReader___HitPlayer___2449261505);
			RegisterServerRpc(37u, RpcReader___ActivateExplosive___1390675745);
			RegisterServerRpc(38u, RpcReader___GrillItemInLava___2875874600);
			RegisterServerRpc(39u, RpcReader___SetItemSkin___2384767125);
			RegisterServerRpc(40u, RpcReader___SetBoatSkin___1246646286);
			RegisterServerRpc(41u, RpcReader___FinishEatingCreature___1039939981);
			RegisterServerRpc(42u, RpcReader___ToggleEatCreature___2104815579);
			RegisterServerRpc(43u, RpcReader___SetItemMultiplier___755958659);
			RegisterServerRpc(44u, RpcReader___ClientSpokeToNpc___1246646286);
			RegisterServerRpc(45u, RpcReader___SetRadioFrequency___1599952821);
			RegisterServerRpc(46u, RpcReader___BuyAttachment___2937188526);
			RegisterServerRpc(47u, RpcReader___BuyBulletUpgrade___1996094309);
			RegisterServerRpc(48u, RpcReader___BuySharpnessUpgrade___680780043);
			RegisterServerRpc(49u, RpcReader___SendChatMessage___3264264606);
			RegisterServerRpc(50u, RpcReader___SetDriver___3849956746);
			RegisterServerRpc(51u, RpcReader___SendBoatInput___1949707525);
			RegisterServerRpc(52u, RpcReader___SetIsAfk___3651916454);
			RegisterServerRpc(53u, RpcReader___BuyBoatMotor___4169050770);
			RegisterServerRpc(54u, RpcReader___BuyBoatRadar___3709775007);
			RegisterServerRpc(55u, RpcReader___SendFinishGame___2166136261);
			RegisterServerRpc(56u, RpcReader___SendFinishedTutorial___3849956746);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateServerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateServerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___SpawnPlayer___1871804056(Vector3 skinColor, Vector3 outfitColor, Vector3 outfitColor2, Vector3 outfitColor3, Vector3 hatColor, Vector3 hatColor2, Vector3 hatColor3, Vector3 accessoryColor, Vector3 accessoryColor2, Vector3 accessoryColor3, byte hatIndex, byte outfitIndex, byte accessoryIndex, bool isBean, NetworkConnection sender = null)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(skinColor);
		pooledWriter.WriteVector3(outfitColor);
		pooledWriter.WriteVector3(outfitColor2);
		pooledWriter.WriteVector3(outfitColor3);
		pooledWriter.WriteVector3(hatColor);
		pooledWriter.WriteVector3(hatColor2);
		pooledWriter.WriteVector3(hatColor3);
		pooledWriter.WriteVector3(accessoryColor);
		pooledWriter.WriteVector3(accessoryColor2);
		pooledWriter.WriteVector3(accessoryColor3);
		pooledWriter.WriteUInt8Unpacked(hatIndex);
		pooledWriter.WriteUInt8Unpacked(outfitIndex);
		pooledWriter.WriteUInt8Unpacked(accessoryIndex);
		pooledWriter.WriteBoolean(isBean);
		SendServerRpc(0u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SpawnPlayer___1871804056(Vector3 P_0, Vector3 P_1, Vector3 P_2, Vector3 P_3, Vector3 P_4, Vector3 P_5, Vector3 P_6, Vector3 P_7, Vector3 P_8, Vector3 P_9, byte P_10, byte P_11, byte P_12, bool P_13, NetworkConnection P_14)
	{
		ulong result;
		if (ConnectionManager.IsUsingSteam)
		{
			if (P_14 == null || !ulong.TryParse(P_14.GetAddress(), out result) || result == 0L || !SteamManager.IsCurrentLobbyMember(new CSteamID(result)))
			{
				MonoBehaviour.print("kicked unauthorized user");
				P_14?.Kick(KickReason.UnusualActivity, LoggingType.Warning, "Player spawn rejected because the sender's Steam identity is not authorized by the current lobby.");
				return;
			}
		}
		else
		{
			result = SteamUser.GetSteamID().m_SteamID;
		}
		Player player = UnityEngine.Object.Instantiate(GameInfo.PlayerPrefab);
		player.SetSteamID(result);
		SavedPlayer savedPlayer = SaveManager.GetSavedPlayer(result);
		player.Inventory.SetSavedPlayer(savedPlayer);
		player.Vitals.SetSavedPlayer(savedPlayer);
		player.Skin.SetServerColors(P_0, P_1, P_2, P_3, P_4, P_5, P_6, P_7, P_8, P_9, P_10, P_11, P_12, P_13);
		player.SetIsBean(P_13);
		Spawn(player.gameObject, P_14);
	}

	private void RpcReader___SpawnPlayer___1871804056(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3 vector2 = PooledReader0.ReadVector3();
		Vector3 vector3 = PooledReader0.ReadVector3();
		Vector3 vector4 = PooledReader0.ReadVector3();
		Vector3 vector5 = PooledReader0.ReadVector3();
		Vector3 vector6 = PooledReader0.ReadVector3();
		Vector3 vector7 = PooledReader0.ReadVector3();
		Vector3 vector8 = PooledReader0.ReadVector3();
		Vector3 vector9 = PooledReader0.ReadVector3();
		Vector3 vector10 = PooledReader0.ReadVector3();
		byte b = PooledReader0.ReadUInt8Unpacked();
		byte b2 = PooledReader0.ReadUInt8Unpacked();
		byte b3 = PooledReader0.ReadUInt8Unpacked();
		bool flag = PooledReader0.ReadBoolean();
		if (base.IsServerInitialized)
		{
			RpcLogic___SpawnPlayer___1871804056(vector, vector2, vector3, vector4, vector5, vector6, vector7, vector8, vector9, vector10, b, b2, b3, flag, conn);
		}
	}

	private void RpcWriter___ResurrectPlayer___2247010027(Player player, DeadPlayer deadPlayer)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		GameTypeSerializers.GWrite___DeadPlayerFishNet_002ESerializing_002EGenerated(pooledWriter, deadPlayer);
		SendServerRpc(1u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___ResurrectPlayer___2247010027(Player P_0, DeadPlayer P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing && P_0.Vitals.Health <= 0 && (bool)P_1 && !P_1.IsDeinitializing)
		{
			P_0.Vitals.OnResurrect();
			P_1.DestroyItem(7);
		}
	}

	private void RpcReader___ResurrectPlayer___2247010027(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		DeadPlayer deadPlayer = GameTypeSerializers.GRead___DeadPlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___ResurrectPlayer___2247010027(player, deadPlayer);
		}
	}

	private void RpcWriter___UpdateDeadPlayerResurrectPercent___3918731242(DeadPlayer deadPlayer, float resurrectPercent, Channel channel = Channel.Unreliable)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___DeadPlayerFishNet_002ESerializing_002EGenerated(pooledWriter, deadPlayer);
		pooledWriter.WriteSingle(resurrectPercent);
		SendServerRpc(2u, pooledWriter, channel2, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdateDeadPlayerResurrectPercent___3918731242(DeadPlayer P_0, float P_1, Channel P_2)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ReceiveResurrectPercent(P_1);
		}
	}

	private void RpcReader___UpdateDeadPlayerResurrectPercent___3918731242(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		DeadPlayer deadPlayer = GameTypeSerializers.GRead___DeadPlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		float num = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___UpdateDeadPlayerResurrectPercent___3918731242(deadPlayer, num, channel);
		}
	}

	private void RpcWriter___RespawnPlayer___2210451296(Player player, Vector3 deathPos, Quaternion deathRot)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteVector3(deathPos);
		pooledWriter.WriteQuaternion32(deathRot);
		SendServerRpc(3u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___RespawnPlayer___2210451296(Player P_0, Vector3 P_1, Quaternion P_2)
	{
		if (!P_0 || P_0.IsDeinitializing || P_0.Vitals.Health > 0)
		{
			return;
		}
		if (PlayerManager.AlivePlayers.Count == 0)
		{
			foreach (Player player in PlayerManager.Players)
			{
				player.Inventory.ServerDropAll(P_1, P_2);
				player.Vitals.ServerResetVitals();
				if ((bool)player.Dying.DeadPlayer)
				{
					player.Dying.DeadPlayer.DestroyItem(7);
				}
			}
			BoatManager.Instance.TryMoveBoat(SpawnManager.BoatSpawnPos, SpawnManager.BoatSpawnRot);
		}
		else
		{
			P_0.Inventory.ServerDropAll(P_1, P_2);
			P_0.Vitals.ServerResetVitals();
			if ((bool)P_0.Dying.DeadPlayer)
			{
				P_0.Dying.DeadPlayer.DestroyItem(7);
			}
		}
	}

	private void RpcReader___RespawnPlayer___2210451296(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		Quaternion quaternion2 = PooledReader0.ReadQuaternion32();
		if (base.IsServerInitialized)
		{
			RpcLogic___RespawnPlayer___2210451296(player, vector, quaternion2);
		}
	}

	private void RpcWriter___TeleportPlayer___3852204532(Player player, Vector3 pos, float rot)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteSingle(rot);
		SendServerRpc(4u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___TeleportPlayer___3852204532(Player P_0, Vector3 P_1, float P_2)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.RPCTeleport(P_0.Owner, P_1, P_2);
		}
	}

	private void RpcReader___TeleportPlayer___3852204532(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		float num = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___TeleportPlayer___3852204532(player, vector, num);
		}
	}

	private void RpcWriter___SetItemHolder___3876263354(Item item, Player player, Item replacedItem)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, replacedItem);
		SendServerRpc(5u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SetItemHolder___3876263354(Item P_0, Player P_1, Item P_2)
	{
		if (!P_0 || P_0.IsDeinitializing)
		{
			return;
		}
		Player syncedHolder = P_0.SyncedHolder;
		if ((bool)P_1)
		{
			Item item = (((bool)P_2 && P_2 != P_0 && P_2.SyncedHolder == P_1) ? P_2 : P_1.Holding.HeldItem);
			if ((bool)syncedHolder && syncedHolder != P_1)
			{
				TargetReconcileRejectedItemPickup(P_1.Owner, P_0, item);
				return;
			}
			Item item2 = item;
			if ((bool)item2 && item2 != P_0)
			{
				if (!P_1.Inventory.ServerTryStoreHeldItem(item2))
				{
					item2.SetSyncedHolder(null);
				}
				P_1.Holding.SetHeldItem(null);
			}
		}
		P_0.SetSyncedHolder(P_1);
		if ((bool)P_1)
		{
			P_1.Holding.SetHeldItem(P_0);
		}
		else if ((bool)syncedHolder && syncedHolder.Holding.HeldItem == P_0)
		{
			syncedHolder.Holding.SetHeldItem(null);
		}
	}

	private void RpcReader___SetItemHolder___3876263354(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Item item2 = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___SetItemHolder___3876263354(item, player, item2);
		}
	}

	private void RpcWriter___TargetReconcileRejectedItemPickup___3695777406(NetworkConnection connection, Item attemptedItem, Item authoritativeHeldItem)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, attemptedItem);
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, authoritativeHeldItem);
		SendTargetRpc(6u, pooledWriter, channel, DataOrderType.Default, connection, excludeServer: false);
		pooledWriter.Store();
	}

	private void RpcLogic___TargetReconcileRejectedItemPickup___3695777406(NetworkConnection P_0, Item P_1, Item P_2)
	{
		if ((bool)P_1)
		{
			P_1.ReconcileLocalHolderWithSyncedHolder();
		}
		if ((bool)P_2 && P_2 != P_1)
		{
			P_2.ReconcileLocalHolderWithSyncedHolder();
		}
	}

	private void RpcReader___TargetReconcileRejectedItemPickup___3695777406(PooledReader PooledReader0, Channel channel)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Item item2 = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___TargetReconcileRejectedItemPickup___3695777406(base.LocalConnection, item, item2);
		}
	}

	private void RpcWriter___HandOverItemSimulation___2875874600(Item item)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		SendServerRpc(7u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___HandOverItemSimulation___2875874600(Item P_0)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.RigidbodySync.StartSimulateLocal();
		}
	}

	private void RpcReader___HandOverItemSimulation___2875874600(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___HandOverItemSimulation___2875874600(item);
		}
	}

	private void RpcWriter___UpdateItemPosRot___3673846853(Item item, NetworkConnection netCon, Vector3 pos, Quaternion rot, bool onBoat, float[] extraHingeAngles = null, Quaternion[] extraHingeRots = null, Channel channel = Channel.Unreliable)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		pooledWriter.WriteNetworkConnection(netCon);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteQuaternion32(rot);
		pooledWriter.WriteBoolean(onBoat);
		GameTypeSerializers.GWrite___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(pooledWriter, extraHingeAngles);
		GameTypeSerializers.GWrite___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerated(pooledWriter, extraHingeRots);
		SendServerRpc(8u, pooledWriter, channel2, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdateItemPosRot___3673846853(Item P_0, NetworkConnection P_1, Vector3 P_2, Quaternion P_3, bool P_4, float[] P_5, Quaternion[] P_6, Channel P_7)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.RigidbodySync.ServerSetPosRot(P_1, P_2, P_3, P_4, P_5, P_6);
		}
	}

	private void RpcReader___UpdateItemPosRot___3673846853(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		NetworkConnection networkConnection = PooledReader0.ReadNetworkConnection();
		Vector3 vector = PooledReader0.ReadVector3();
		Quaternion quaternion2 = PooledReader0.ReadQuaternion32();
		bool flag = PooledReader0.ReadBoolean();
		float[] array = GameTypeSerializers.GRead___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Quaternion[] array2 = GameTypeSerializers.GRead___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___UpdateItemPosRot___3673846853(item, networkConnection, vector, quaternion2, flag, array, array2, channel);
		}
	}

	private void RpcWriter___SetSyncedSimulator___1559681479(Item item, NetworkConnection newSimulator)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		pooledWriter.WriteNetworkConnection(newSimulator);
		SendServerRpc(9u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SetSyncedSimulator___1559681479(Item P_0, NetworkConnection P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.RigidbodySync.ServerSetIsFloating(to: false);
			P_0.RigidbodySync.ServerSetSyncedSimulator(P_1);
		}
	}

	private void RpcReader___SetSyncedSimulator___1559681479(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		NetworkConnection networkConnection = PooledReader0.ReadNetworkConnection();
		if (base.IsServerInitialized)
		{
			RpcLogic___SetSyncedSimulator___1559681479(item, networkConnection);
		}
	}

	private void RpcWriter___UpdateHeldToolPosRot___3161606994(Tool tool, Vector3 pos, Quaternion rot, Channel channel = Channel.Unreliable)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ToolFishNet_002ESerializing_002EGenerated(pooledWriter, tool);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteQuaternion32(rot);
		SendServerRpc(10u, pooledWriter, channel2, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdateHeldToolPosRot___3161606994(Tool P_0, Vector3 P_1, Quaternion P_2, Channel P_3)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.RigidbodySync.ServerSetHeldToolPosRot(P_1, P_2);
		}
	}

	private void RpcReader___UpdateHeldToolPosRot___3161606994(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Tool tool = GameTypeSerializers.GRead___ToolFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		Quaternion quaternion2 = PooledReader0.ReadQuaternion32();
		if (base.IsServerInitialized)
		{
			RpcLogic___UpdateHeldToolPosRot___3161606994(tool, vector, quaternion2, channel);
		}
	}

	private void RpcWriter___UpdatePlayerPosRot___2142082744(Player player, Vector3 pos, Vector2 rot, bool onBoat = false, bool teleport = false, Channel channel = Channel.Unreliable)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteVector2(rot);
		pooledWriter.WriteBoolean(onBoat);
		pooledWriter.WriteBoolean(teleport);
		SendServerRpc(11u, pooledWriter, channel2, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdatePlayerPosRot___2142082744(Player P_0, Vector3 P_1, Vector2 P_2, bool P_3, bool P_4, Channel P_5)
	{
		if (!P_0 || P_0.IsDeinitializing)
		{
			return;
		}
		if (P_3 && (bool)BoatManager.Boat)
		{
			if (P_4)
			{
				P_0.Other.SetReceivedPosRot(P_1, P_2, onBoat: true, teleport: true);
			}
			else
			{
				P_0.Other.SetReceivedPosRot(P_1, P_2, onBoat: true);
			}
		}
		else if (P_4)
		{
			P_0.Other.SetReceivedPosRot(P_1, P_2, onBoat: false, teleport: true);
		}
		else
		{
			P_0.Other.SetReceivedPosRot(P_1, P_2);
		}
	}

	private void RpcReader___UpdatePlayerPosRot___2142082744(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		Vector2 vector2 = PooledReader0.ReadVector2();
		bool flag = PooledReader0.ReadBoolean();
		bool flag2 = PooledReader0.ReadBoolean();
		if (base.IsServerInitialized)
		{
			RpcLogic___UpdatePlayerPosRot___2142082744(player, vector, vector2, flag, flag2, channel);
		}
	}

	private void RpcWriter___UpdatePlayerCrouching___1562565462(Player player, bool isCrouching, Channel channel = Channel.Unreliable)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteBoolean(isCrouching);
		SendServerRpc(12u, pooledWriter, channel2, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdatePlayerCrouching___1562565462(Player P_0, bool P_1, Channel P_2)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.SetIsCrouching(P_1);
		}
	}

	private void RpcReader___UpdatePlayerCrouching___1562565462(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		bool flag = PooledReader0.ReadBoolean();
		if (base.IsServerInitialized)
		{
			RpcLogic___UpdatePlayerCrouching___1562565462(player, flag, channel);
		}
	}

	private void RpcWriter___SelectInvSlot___3709775007(Player player, int slot)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteInt32(slot);
		SendServerRpc(13u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SelectInvSlot___3709775007(Player P_0, int P_1)
	{
		if (!P_0 || P_0.IsDeinitializing || (P_0.Vitals.Health <= 0 && P_1 != -1))
		{
			return;
		}
		if (P_1 != -1)
		{
			Item heldItem = P_0.Holding.HeldItem;
			Item uninitializedHeldItem = P_0.Holding.UninitializedHeldItem;
			bool num = (bool)uninitializedHeldItem && !P_0.Inventory.HasItemInInventory(uninitializedHeldItem);
			bool flag = (bool)heldItem && !P_0.Inventory.HasItemInInventory(heldItem);
			if (num | flag)
			{
				return;
			}
		}
		P_0.Inventory.ServerSetSyncedCurSlot(P_1);
	}

	private void RpcReader___SelectInvSlot___3709775007(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		int num = PooledReader0.ReadInt32();
		if (base.IsServerInitialized)
		{
			RpcLogic___SelectInvSlot___3709775007(player, num);
		}
	}

	private void RpcWriter___PutItemInInventory___3873472972(Player player, Item item, byte slot)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		pooledWriter.WriteUInt8Unpacked(slot);
		SendServerRpc(14u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___PutItemInInventory___3873472972(Player P_0, Item P_1, byte P_2)
	{
		if ((bool)P_1 && !P_1.IsDeinitializing && (bool)P_1.SyncedHolder && !(P_1.SyncedHolder != P_0) && (bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.Inventory.AddItem(P_2, P_1);
		}
	}

	private void RpcReader___PutItemInInventory___3873472972(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___PutItemInInventory___3873472972(player, item, b);
		}
	}

	private void RpcWriter___RemoveItemFromInventory___241039439(Player player, Item item)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		SendServerRpc(15u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___RemoveItemFromInventory___241039439(Player P_0, Item P_1)
	{
		if ((bool)P_1 && !P_1.IsDeinitializing && (bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.Inventory.RemoveItem(P_1);
			P_1.SetSyncedHolder(null);
		}
	}

	private void RpcReader___RemoveItemFromInventory___241039439(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___RemoveItemFromInventory___241039439(player, item);
		}
	}

	private void RpcWriter___DropAllItems___2210451296(Player player, Vector3 pos, Quaternion rot)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteQuaternion32(rot);
		SendServerRpc(16u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___DropAllItems___2210451296(Player P_0, Vector3 P_1, Quaternion P_2)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.Inventory.ServerDropAll(P_1, P_2);
		}
	}

	private void RpcReader___DropAllItems___2210451296(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		Quaternion quaternion2 = PooledReader0.ReadQuaternion32();
		if (base.IsServerInitialized)
		{
			RpcLogic___DropAllItems___2210451296(player, vector, quaternion2);
		}
	}

	private void RpcWriter___PlayImpactSound___413717140(Item item, byte vel, Channel channel = Channel.Unreliable)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		pooledWriter.WriteUInt8Unpacked(vel);
		SendServerRpc(17u, pooledWriter, channel2, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___PlayImpactSound___413717140(Item P_0, byte P_1, Channel P_2)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			ObserverToLocalAdapter.Instance.PlayImpactSound(P_0, P_1);
		}
	}

	private void RpcReader___PlayImpactSound___413717140(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___PlayImpactSound___413717140(item, b, channel);
		}
	}

	private void RpcWriter___UpdateBaitPosAndLineLength___2349636148(FishingRod rod, Vector3 baitPos, int curLineLengthMulti, float curBaitForce, Channel channel = Channel.Unreliable)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___FishingRodFishNet_002ESerializing_002EGenerated(pooledWriter, rod);
		pooledWriter.WriteVector3(baitPos);
		pooledWriter.WriteInt32(curLineLengthMulti);
		pooledWriter.WriteSingle(curBaitForce);
		SendServerRpc(18u, pooledWriter, channel2, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdateBaitPosAndLineLength___2349636148(FishingRod P_0, Vector3 P_1, int P_2, float P_3, Channel P_4)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ReceiveBaitPosAndLineLength(P_1, P_2, P_3);
		}
	}

	private void RpcReader___UpdateBaitPosAndLineLength___2349636148(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		FishingRod fishingRod = GameTypeSerializers.GRead___FishingRodFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		int num = PooledReader0.ReadInt32();
		float num2 = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___UpdateBaitPosAndLineLength___2349636148(fishingRod, vector, num, num2, channel);
		}
	}

	private void RpcWriter___ReleaseItemFromBait___2725923288(FishingRod rod)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___FishingRodFishNet_002ESerializing_002EGenerated(pooledWriter, rod);
		SendServerRpc(19u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___ReleaseItemFromBait___2725923288(FishingRod P_0)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing && (bool)P_0.Bait.ServerItemOnBait)
		{
			P_0.Bait.ServerItemOnBait.SetAttachedRod(null);
			if ((bool)P_0.Holder)
			{
				P_0.Holder.Inventory.ServerOnBaitUsed();
			}
		}
	}

	private void RpcReader___ReleaseItemFromBait___2725923288(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		FishingRod fishingRod = GameTypeSerializers.GRead___FishingRodFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___ReleaseItemFromBait___2725923288(fishingRod);
		}
	}

	private void RpcWriter___UpdateRodPullBack___2928850743(FishingRod rod, float rot, bool additionalState, Channel channel = Channel.Unreliable)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___FishingRodFishNet_002ESerializing_002EGenerated(pooledWriter, rod);
		pooledWriter.WriteSingle(rot);
		pooledWriter.WriteBoolean(additionalState);
		SendServerRpc(20u, pooledWriter, channel2, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdateRodPullBack___2928850743(FishingRod P_0, float P_1, bool P_2, Channel P_3)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ReceiveRodPullBack(P_1, P_2);
		}
	}

	private void RpcReader___UpdateRodPullBack___2928850743(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		FishingRod fishingRod = GameTypeSerializers.GRead___FishingRodFishNet_002ESerializing_002EGenerateds(PooledReader0);
		float num = PooledReader0.ReadSingle();
		bool flag = PooledReader0.ReadBoolean();
		if (base.IsServerInitialized)
		{
			RpcLogic___UpdateRodPullBack___2928850743(fishingRod, num, flag, channel);
		}
	}

	private void RpcWriter___HitCreature___215526726(Creature creature, Player playerWhoHit, int damage, Vector3 hitPoint, Vector3 dir)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___CreatureFishNet_002ESerializing_002EGenerated(pooledWriter, creature);
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, playerWhoHit);
		pooledWriter.WriteInt32(damage);
		pooledWriter.WriteVector3(hitPoint);
		pooledWriter.WriteVector3(dir);
		SendServerRpc(21u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___HitCreature___215526726(Creature P_0, Player P_1, int P_2, Vector3 P_3, Vector3 P_4)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ServerChangeHp(P_2);
			P_0.ObserverHit(P_1, P_3, P_4, P_2);
		}
	}

	private void RpcReader___HitCreature___215526726(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Creature creature = GameTypeSerializers.GRead___CreatureFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		int num = PooledReader0.ReadInt32();
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3 vector2 = PooledReader0.ReadVector3();
		if (base.IsServerInitialized)
		{
			RpcLogic___HitCreature___215526726(creature, player, num, vector, vector2);
		}
	}

	private void RpcWriter___AddProjectile___3746676724(Player owner, WeaponInfo weapon, uint tick, uint id, Vector3 pos, Vector3 vel)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, owner);
		GameTypeSerializers.GWrite___WeaponInfoFishNet_002ESerializing_002EGenerated(pooledWriter, weapon);
		pooledWriter.WriteUInt32(tick);
		pooledWriter.WriteUInt32(id);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteVector3(vel);
		SendServerRpc(22u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___AddProjectile___3746676724(Player P_0, WeaponInfo P_1, uint P_2, uint P_3, Vector3 P_4, Vector3 P_5)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			ProjectileManager.Instance.ObserverAddProjectile(P_0, P_1, P_2, P_3, P_4, P_5);
		}
	}

	private void RpcReader___AddProjectile___3746676724(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		WeaponInfo weaponInfo = GameTypeSerializers.GRead___WeaponInfoFishNet_002ESerializing_002EGenerateds(PooledReader0);
		uint num = PooledReader0.ReadUInt32();
		uint num2 = PooledReader0.ReadUInt32();
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3 vector2 = PooledReader0.ReadVector3();
		if (base.IsServerInitialized)
		{
			RpcLogic___AddProjectile___3746676724(player, weaponInfo, num, num2, vector, vector2);
		}
	}

	private void RpcWriter___AddProjectiles___1894401688(Player owner, WeaponInfo weapon, uint tick, uint id, Vector3 pos, Vector3[] velocities)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, owner);
		GameTypeSerializers.GWrite___WeaponInfoFishNet_002ESerializing_002EGenerated(pooledWriter, weapon);
		pooledWriter.WriteUInt32(tick);
		pooledWriter.WriteUInt32(id);
		pooledWriter.WriteVector3(pos);
		GameTypeSerializers.GWrite___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerated(pooledWriter, velocities);
		SendServerRpc(23u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___AddProjectiles___1894401688(Player P_0, WeaponInfo P_1, uint P_2, uint P_3, Vector3 P_4, Vector3[] P_5)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			ProjectileManager.Instance.ObserverAddProjectiles(P_0, P_1, P_2, P_3, P_4, P_5);
		}
	}

	private void RpcReader___AddProjectiles___1894401688(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		WeaponInfo weaponInfo = GameTypeSerializers.GRead___WeaponInfoFishNet_002ESerializing_002EGenerateds(PooledReader0);
		uint num = PooledReader0.ReadUInt32();
		uint num2 = PooledReader0.ReadUInt32();
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3[] array = GameTypeSerializers.GRead___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___AddProjectiles___1894401688(player, weaponInfo, num, num2, vector, array);
		}
	}

	private void RpcWriter___ProjectileHitDynamic___353006558(NetworkConnection netCon, uint id)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteNetworkConnection(netCon);
		pooledWriter.WriteUInt32(id);
		SendServerRpc(24u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___ProjectileHitDynamic___353006558(NetworkConnection P_0, uint P_1)
	{
		ProjectileManager.Instance.ObserverProjectileHitDynamic(P_0, P_1);
	}

	private void RpcReader___ProjectileHitDynamic___353006558(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkConnection networkConnection = PooledReader0.ReadNetworkConnection();
		uint num = PooledReader0.ReadUInt32();
		if (base.IsServerInitialized)
		{
			RpcLogic___ProjectileHitDynamic___353006558(networkConnection, num);
		}
	}

	private void RpcWriter___ReloadWeapon___1996094309(Weapon weapon)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___WeaponFishNet_002ESerializing_002EGenerated(pooledWriter, weapon);
		SendServerRpc(25u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___ReloadWeapon___1996094309(Weapon P_0)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ObserverReload();
		}
	}

	private void RpcReader___ReloadWeapon___1996094309(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Weapon weapon = GameTypeSerializers.GRead___WeaponFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___ReloadWeapon___1996094309(weapon);
		}
	}

	private void RpcWriter___BuyItem___4197152275(byte itemID, Player player, Item replacedItem, Vector3 pos, Quaternion rot, bool isFree = false)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(itemID);
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, replacedItem);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteQuaternion32(rot);
		pooledWriter.WriteBoolean(isFree);
		SendServerRpc(26u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___BuyItem___4197152275(byte P_0, Player P_1, Item P_2, Vector3 P_3, Quaternion P_4, bool P_5)
	{
		if (P_0 == byte.MaxValue || !P_1 || P_1.IsDeinitializing || (bool)P_1.Holding.UninitializedHeldItem || P_1.Holding.HeldItem != P_2 || ((bool)P_2 && (P_2.IsDeinitializing || P_2.SyncedHolder != P_1)))
		{
			return;
		}
		Item item = GameInfo.IDToItem(P_0);
		if (!item || (!MoneyManager.CanAfford(item.Cost) && !P_5))
		{
			return;
		}
		if ((bool)item.Weapon)
		{
			Instance.GunsBought++;
		}
		if (!P_5)
		{
			MoneyManager.RemoveMoney(item.Cost, P_1);
		}
		if ((bool)P_2)
		{
			if (!P_1.Inventory.ServerTryStoreHeldItem(P_2))
			{
				P_2.SetSyncedHolder(null);
			}
			P_1.Holding.SetHeldItem(null);
		}
		Item item2 = UnityEngine.Object.Instantiate(item, P_3, P_4);
		item2.SetSyncedHolder(P_1);
		Spawn(item2.gameObject);
		P_1.Holding.SetHeldItem(item2);
	}

	private void RpcReader___BuyItem___4197152275(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		Quaternion quaternion2 = PooledReader0.ReadQuaternion32();
		bool flag = PooledReader0.ReadBoolean();
		if (base.IsServerInitialized)
		{
			RpcLogic___BuyItem___4197152275(b, player, item, vector, quaternion2, flag);
		}
	}

	private void RpcWriter___UnlockPocket___1382779195(Player player, byte slotIndex)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteUInt8Unpacked(slotIndex);
		SendServerRpc(27u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UnlockPocket___1382779195(Player P_0, byte P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing && P_1 <= 5)
		{
			int extraSlotCost = P_0.Inventory.GetExtraSlotCost(P_1);
			if (MoneyManager.CanAfford(extraSlotCost))
			{
				MoneyManager.RemoveMoney(extraSlotCost, P_0);
				P_0.Inventory.UnlockExtraPocket(P_1);
			}
		}
	}

	private void RpcReader___UnlockPocket___1382779195(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___UnlockPocket___1382779195(player, b);
		}
	}

	private void RpcWriter___BuyBait___4169050770(Player player, byte baitIndex, int cost)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteUInt8Unpacked(baitIndex);
		pooledWriter.WriteInt32(cost);
		SendServerRpc(28u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___BuyBait___4169050770(Player P_0, byte P_1, int P_2)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing && P_1 < GameInfo.AllBaits.Count && MoneyManager.CanAfford(P_2))
		{
			if (P_2 != 0)
			{
				MoneyManager.RemoveMoney(P_2, P_0);
			}
			P_0.Inventory.ServerBoughtBait(P_1);
			P_0.Inventory.ServerSetCurBait(P_1);
		}
	}

	private void RpcReader___BuyBait___4169050770(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		int num = PooledReader0.ReadInt32();
		if (base.IsServerInitialized)
		{
			RpcLogic___BuyBait___4169050770(player, b, num);
		}
	}

	private void RpcWriter___TakeItemFromNpc___1382779195(Player player, byte npcID)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteUInt8Unpacked(npcID);
		SendServerRpc(29u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___TakeItemFromNpc___1382779195(Player P_0, byte P_1)
	{
		if (!P_0 || P_0.IsDeinitializing || (P_1 != byte.MaxValue && !NPCManager.Instance.NpcIsHoldingItem(P_1)))
		{
			return;
		}
		NPCQuest showingQuest = NPCManager.Instance.GetShowingQuest(P_1);
		if (!showingQuest)
		{
			return;
		}
		switch (showingQuest.Type)
		{
		case QuestType.GiveBait:
			if ((bool)showingQuest.BaitToReceive)
			{
				byte indexOfBait = GameInfo.GetIndexOfBait(showingQuest.BaitToReceive);
				P_0.Inventory.ServerBoughtBait(indexOfBait);
				P_0.Inventory.ServerSetCurBait(indexOfBait);
			}
			break;
		case QuestType.UnlockGrill:
			NPCManager.UnlockGrill();
			break;
		case QuestType.UnlockIsland:
			OnlineIslandManager.Instance.UnlockIsland(showingQuest.IslandToUnlock);
			break;
		case QuestType.UnlockBoat:
			BoatManager.Boat.UnlockBoat();
			break;
		case QuestType.FinalBoss:
			NPCManager.SetFinalBossKilled();
			break;
		}
		NPCManager.Instance.ResetItemHeld(P_1);
	}

	private void RpcReader___TakeItemFromNpc___1382779195(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___TakeItemFromNpc___1382779195(player, b);
		}
	}

	private void RpcWriter___ChangeBait___1382779195(Player player, byte to)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteUInt8Unpacked(to);
		SendServerRpc(30u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___ChangeBait___1382779195(Player P_0, byte P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing && P_1 < GameInfo.AllBaits.Count)
		{
			P_0.Inventory.ServerSetCurBait(P_1);
		}
	}

	private void RpcReader___ChangeBait___1382779195(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___ChangeBait___1382779195(player, b);
		}
	}

	private void RpcWriter___PlaceBet___1246646286(byte betColor)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(betColor);
		SendServerRpc(31u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___PlaceBet___1246646286(byte P_0)
	{
		P_0 = (byte)Mathf.Clamp(P_0, 0, Enum.GetValues(typeof(BetColor)).Length - 1);
		if (CasinoManager.HasPlacedBet && !CasinoManager.IsBetting)
		{
			CasinoManager.Instance.ServerStartBet((BetColor)P_0);
		}
	}

	private void RpcReader___PlaceBet___1246646286(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___PlaceBet___1246646286(b);
		}
	}

	private void RpcWriter___UpdateRoulette___3661469815(Vector3 ballPos, float wheelRot)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(ballPos);
		pooledWriter.WriteSingle(wheelRot);
		SendServerRpc(32u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___UpdateRoulette___3661469815(Vector3 P_0, float P_1)
	{
		CasinoManager.Instance.UpdateGameObjects(P_0, P_1);
	}

	private void RpcReader___UpdateRoulette___3661469815(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		float num = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___UpdateRoulette___3661469815(vector, num);
		}
	}

	private void RpcWriter___Punch___1201658259(Player player, Transform target, bool right, Vector3 targetHitPoint)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteTransform(target);
		pooledWriter.WriteBoolean(right);
		pooledWriter.WriteVector3(targetHitPoint);
		SendServerRpc(33u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___Punch___1201658259(Player P_0, Transform P_1, bool P_2, Vector3 P_3)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.Punching.ObserverStartPunching(P_1, P_2, P_3);
		}
	}

	private void RpcReader___Punch___1201658259(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Transform transform = PooledReader0.ReadTransform();
		bool flag = PooledReader0.ReadBoolean();
		Vector3 vector = PooledReader0.ReadVector3();
		if (base.IsServerInitialized)
		{
			RpcLogic___Punch___1201658259(player, transform, flag, vector);
		}
	}

	private void RpcWriter___MeleeAttack___1137909598(Melee weapon, Transform target, bool right, Vector3 targetHitPoint)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___MeleeFishNet_002ESerializing_002EGenerated(pooledWriter, weapon);
		pooledWriter.WriteTransform(target);
		pooledWriter.WriteBoolean(right);
		pooledWriter.WriteVector3(targetHitPoint);
		SendServerRpc(34u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___MeleeAttack___1137909598(Melee P_0, Transform P_1, bool P_2, Vector3 P_3)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ObserverStartAttacking(P_1, P_2, P_3);
		}
	}

	private void RpcReader___MeleeAttack___1137909598(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Melee melee = GameTypeSerializers.GRead___MeleeFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Transform transform = PooledReader0.ReadTransform();
		bool flag = PooledReader0.ReadBoolean();
		Vector3 vector = PooledReader0.ReadVector3();
		if (base.IsServerInitialized)
		{
			RpcLogic___MeleeAttack___1137909598(melee, transform, flag, vector);
		}
	}

	private void RpcWriter___InspectTool___625650179(Tool tool)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ToolFishNet_002ESerializing_002EGenerated(pooledWriter, tool);
		SendServerRpc(35u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___InspectTool___625650179(Tool P_0)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ObserverPlayInspectAnim();
		}
	}

	private void RpcReader___InspectTool___625650179(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Tool tool = GameTypeSerializers.GRead___ToolFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___InspectTool___625650179(tool);
		}
	}

	private void RpcWriter___HitPlayer___2449261505(Player player, int damage, Vector3 force, Vector3 pos = default(Vector3), byte damageType = 1, Player playerWhoHit = null)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteInt32(damage);
		pooledWriter.WriteVector3(force);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteUInt8Unpacked(damageType);
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, playerWhoHit);
		SendServerRpc(36u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___HitPlayer___2449261505(Player P_0, int P_1, Vector3 P_2, Vector3 P_3, byte P_4, Player P_5)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing && (!P_5 || ServerSettings.UseFriendlyFire) && P_1 != 0)
		{
			P_0.Vitals.TakeDamage(P_1, P_3, P_2, P_5);
			if (P_4 != 0)
			{
				P_0.Vitals.ObserverHit(P_5, P_3, P_2, P_1, (DamageType)P_4);
			}
		}
	}

	private void RpcReader___HitPlayer___2449261505(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		int num = PooledReader0.ReadInt32();
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3 vector2 = PooledReader0.ReadVector3();
		byte b = PooledReader0.ReadUInt8Unpacked();
		Player player2 = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___HitPlayer___2449261505(player, num, vector, vector2, b, player2);
		}
	}

	private void RpcWriter___ActivateExplosive___1390675745(Explosive explosive, uint tick, bool forced = false, bool instant = false, Player playerWhoForced = null)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ExplosiveFishNet_002ESerializing_002EGenerated(pooledWriter, explosive);
		pooledWriter.WriteUInt32(tick);
		pooledWriter.WriteBoolean(forced);
		pooledWriter.WriteBoolean(instant);
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, playerWhoForced);
		SendServerRpc(37u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___ActivateExplosive___1390675745(Explosive P_0, uint P_1, bool P_2, bool P_3, Player P_4)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ObserverActivate(P_1, P_2, P_3, P_4);
		}
	}

	private void RpcReader___ActivateExplosive___1390675745(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Explosive explosive = GameTypeSerializers.GRead___ExplosiveFishNet_002ESerializing_002EGenerateds(PooledReader0);
		uint num = PooledReader0.ReadUInt32();
		bool flag = PooledReader0.ReadBoolean();
		bool flag2 = PooledReader0.ReadBoolean();
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___ActivateExplosive___1390675745(explosive, num, flag, flag2, player);
		}
	}

	private void RpcWriter___GrillItemInLava___2875874600(Item item)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		SendServerRpc(38u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___GrillItemInLava___2875874600(Item P_0)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing && !(P_0.Cookness > 1f))
		{
			P_0.CookWithlava();
		}
	}

	private void RpcReader___GrillItemInLava___2875874600(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___GrillItemInLava___2875874600(item);
		}
	}

	private void RpcWriter___SetItemSkin___2384767125(Item item, byte toIndex)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		pooledWriter.WriteUInt8Unpacked(toIndex);
		SendServerRpc(39u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SetItemSkin___2384767125(Item P_0, byte P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.ServerSetSkin(P_1);
		}
	}

	private void RpcReader___SetItemSkin___2384767125(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___SetItemSkin___2384767125(item, b);
		}
	}

	private void RpcWriter___SetBoatSkin___1246646286(byte to)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(to);
		SendServerRpc(40u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SetBoatSkin___1246646286(byte P_0)
	{
		if ((bool)BoatManager.Boat && !BoatManager.Boat.IsDeinitializing)
		{
			BoatManager.Boat.ServerSetSkin(P_0);
		}
	}

	private void RpcReader___SetBoatSkin___1246646286(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___SetBoatSkin___1246646286(b);
		}
	}

	private void RpcWriter___FinishEatingCreature___1039939981(Creature creature, Player player)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___CreatureFishNet_002ESerializing_002EGenerated(pooledWriter, creature);
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		SendServerRpc(41u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___FinishEatingCreature___1039939981(Creature P_0, Player P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing && (bool)P_1 && !P_1.IsDeinitializing)
		{
			P_1.Vitals.RestoreFullness((int)((float)P_0.FullnessToRestore * GameInfo.CooknessEatCurve.Evaluate(P_0.Cookness)));
			P_1.Vitals.Heal((int)((float)P_0.HpToRestore * GameInfo.CooknessEatCurve.Evaluate(P_0.Cookness)));
			P_0.DestroyItem(3);
		}
	}

	private void RpcReader___FinishEatingCreature___1039939981(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Creature creature = GameTypeSerializers.GRead___CreatureFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___FinishEatingCreature___1039939981(creature, player);
		}
	}

	private void RpcWriter___ToggleEatCreature___2104815579(Player player, bool isEating)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteBoolean(isEating);
		SendServerRpc(42u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___ToggleEatCreature___2104815579(Player P_0, bool P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.Eating.ServerSetEating(P_1);
		}
	}

	private void RpcReader___ToggleEatCreature___2104815579(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		bool flag = PooledReader0.ReadBoolean();
		if (base.IsServerInitialized)
		{
			RpcLogic___ToggleEatCreature___2104815579(player, flag);
		}
	}

	private void RpcWriter___SetItemMultiplier___755958659(Item item, float multiplier)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		pooledWriter.WriteSingle(multiplier);
		SendServerRpc(43u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SetItemMultiplier___755958659(Item P_0, float P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.SetKillscoreMultiplier(P_1);
		}
	}

	private void RpcReader___SetItemMultiplier___755958659(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Item item = GameTypeSerializers.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		float num = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___SetItemMultiplier___755958659(item, num);
		}
	}

	private void RpcWriter___ClientSpokeToNpc___1246646286(byte npcID)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt8Unpacked(npcID);
		SendServerRpc(44u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___ClientSpokeToNpc___1246646286(byte P_0)
	{
		NPCManager.Instance.ServerOnClientSpokeToNpc(P_0);
	}

	private void RpcReader___ClientSpokeToNpc___1246646286(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___ClientSpokeToNpc___1246646286(b);
		}
	}

	private void RpcWriter___SetRadioFrequency___1599952821(Radio radio, float frequency)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___RadioFishNet_002ESerializing_002EGenerated(pooledWriter, radio);
		pooledWriter.WriteSingle(frequency);
		SendServerRpc(45u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SetRadioFrequency___1599952821(Radio P_0, float P_1)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.SetFrequency(P_1);
		}
	}

	private void RpcReader___SetRadioFrequency___1599952821(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Radio radio = GameTypeSerializers.GRead___RadioFishNet_002ESerializing_002EGenerateds(PooledReader0);
		float num = PooledReader0.ReadSingle();
		if (base.IsServerInitialized)
		{
			RpcLogic___SetRadioFrequency___1599952821(radio, num);
		}
	}

	private void RpcWriter___BuyAttachment___2937188526(Weapon weapon, byte attachmentIndex)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___WeaponFishNet_002ESerializing_002EGenerated(pooledWriter, weapon);
		pooledWriter.WriteUInt8Unpacked(attachmentIndex);
		SendServerRpc(46u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___BuyAttachment___2937188526(Weapon P_0, byte P_1)
	{
		if (!P_0 || P_0.IsDeinitializing || P_1 >= GameInfo.AllAttachments.Count)
		{
			return;
		}
		AttachmentInfo attachmentInfo = GameInfo.AllAttachments[P_1];
		if ((bool)attachmentInfo)
		{
			int attachmentCost = P_0.Attachments.GetAttachmentCost(attachmentInfo);
			if (MoneyManager.CanAfford(attachmentCost))
			{
				MoneyManager.RemoveMoney(attachmentCost, P_0.Holder);
				P_0.Attachments.SetAttachment(attachmentInfo);
			}
		}
	}

	private void RpcReader___BuyAttachment___2937188526(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Weapon weapon = GameTypeSerializers.GRead___WeaponFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsServerInitialized)
		{
			RpcLogic___BuyAttachment___2937188526(weapon, b);
		}
	}

	private void RpcWriter___BuyBulletUpgrade___1996094309(Weapon weapon)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___WeaponFishNet_002ESerializing_002EGenerated(pooledWriter, weapon);
		SendServerRpc(47u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___BuyBulletUpgrade___1996094309(Weapon P_0)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			BulletUpgrade nextBulletUpgrade = P_0.Attachments.GetNextBulletUpgrade();
			if (nextBulletUpgrade != null && MoneyManager.CanAfford(nextBulletUpgrade.Cost))
			{
				MoneyManager.RemoveMoney(nextBulletUpgrade.Cost, P_0.Holder);
				P_0.Attachments.UpgradeBullets();
			}
		}
	}

	private void RpcReader___BuyBulletUpgrade___1996094309(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Weapon weapon = GameTypeSerializers.GRead___WeaponFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___BuyBulletUpgrade___1996094309(weapon);
		}
	}

	private void RpcWriter___BuySharpnessUpgrade___680780043(Melee melee)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___MeleeFishNet_002ESerializing_002EGenerated(pooledWriter, melee);
		SendServerRpc(48u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___BuySharpnessUpgrade___680780043(Melee P_0)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			SharpnessUpgrade nextSharpnessUpgrade = P_0.GetNextSharpnessUpgrade();
			if (nextSharpnessUpgrade != null && MoneyManager.CanAfford(nextSharpnessUpgrade.Cost))
			{
				MoneyManager.RemoveMoney(nextSharpnessUpgrade.Cost, P_0.Holder);
				P_0.UpgradeSharpness();
			}
		}
	}

	private void RpcReader___BuySharpnessUpgrade___680780043(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Melee melee = GameTypeSerializers.GRead___MeleeFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___BuySharpnessUpgrade___680780043(melee);
		}
	}

	private void RpcWriter___SendChatMessage___3264264606(ulong from, string message)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt64(from);
		pooledWriter.WriteString(message);
		SendServerRpc(49u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SendChatMessage___3264264606(ulong P_0, string P_1)
	{
		OnlineChatManager.Instance.SendChatMessage(P_0, P_1);
	}

	private void RpcReader___SendChatMessage___3264264606(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		ulong num = PooledReader0.ReadUInt64();
		string text = PooledReader0.ReadStringAllocated();
		if (base.IsServerInitialized)
		{
			RpcLogic___SendChatMessage___3264264606(num, text);
		}
	}

	private void RpcWriter___SetDriver___3849956746(Player driver)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, driver);
		SendServerRpc(50u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SetDriver___3849956746(Player P_0)
	{
		if ((bool)BoatManager.Boat)
		{
			BoatManager.Boat.TrySetDriver(P_0);
		}
	}

	private void RpcReader___SetDriver___3849956746(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___SetDriver___3849956746(player);
		}
	}

	private void RpcWriter___SendBoatInput___1949707525(half x, half y)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerated(pooledWriter, x);
		GameTypeSerializers.GWrite___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerated(pooledWriter, y);
		SendServerRpc(51u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SendBoatInput___1949707525(half P_0, half P_1)
	{
		if ((bool)BoatManager.Boat)
		{
			BoatManager.Boat.ServerSetInput(P_0, P_1);
		}
	}

	private void RpcReader___SendBoatInput___1949707525(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		half half5 = GameTypeSerializers.GRead___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerateds(PooledReader0);
		half half6 = GameTypeSerializers.GRead___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___SendBoatInput___1949707525(half5, half6);
		}
	}

	private void RpcWriter___SetIsAfk___3651916454(Player player, bool isAfk, bool fromPause = true)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteBoolean(isAfk);
		pooledWriter.WriteBoolean(fromPause);
		SendServerRpc(52u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SetIsAfk___3651916454(Player P_0, bool P_1, bool P_2)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.SetIsAfk(P_1, P_2);
		}
	}

	private void RpcReader___SetIsAfk___3651916454(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		bool flag = PooledReader0.ReadBoolean();
		bool flag2 = PooledReader0.ReadBoolean();
		if (base.IsServerInitialized)
		{
			RpcLogic___SetIsAfk___3651916454(player, flag, flag2);
		}
	}

	private void RpcWriter___BuyBoatMotor___4169050770(Player player, byte motorIndex, int cost)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteUInt8Unpacked(motorIndex);
		pooledWriter.WriteInt32(cost);
		SendServerRpc(53u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___BuyBoatMotor___4169050770(Player P_0, byte P_1, int P_2)
	{
		if ((bool)BoatManager.Boat && BoatManager.Boat.MotorIndex < P_1 && MoneyManager.CanAfford(P_2))
		{
			MoneyManager.RemoveMoney(P_2, P_0);
			BoatManager.Boat.SetMotor(P_1);
		}
	}

	private void RpcReader___BuyBoatMotor___4169050770(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		byte b = PooledReader0.ReadUInt8Unpacked();
		int num = PooledReader0.ReadInt32();
		if (base.IsServerInitialized)
		{
			RpcLogic___BuyBoatMotor___4169050770(player, b, num);
		}
	}

	private void RpcWriter___BuyBoatRadar___3709775007(Player player, int cost)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		pooledWriter.WriteInt32(cost);
		SendServerRpc(54u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___BuyBoatRadar___3709775007(Player P_0, int P_1)
	{
		if ((bool)BoatManager.Boat && !BoatManager.Boat.BoatRadarUnlocked && MoneyManager.CanAfford(P_1))
		{
			MoneyManager.RemoveMoney(P_1, P_0);
			BoatManager.Boat.UnlockBoatRadar();
		}
	}

	private void RpcReader___BuyBoatRadar___3709775007(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		int num = PooledReader0.ReadInt32();
		if (base.IsServerInitialized)
		{
			RpcLogic___BuyBoatRadar___3709775007(player, num);
		}
	}

	private void RpcWriter___SendFinishGame___2166136261()
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendServerRpc(55u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SendFinishGame___2166136261()
	{
		if (OnlineIslandManager.CurIsland == 4)
		{
			EndGameManager.Instance.ServerFinishGame();
		}
	}

	private void RpcReader___SendFinishGame___2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (base.IsServerInitialized)
		{
			RpcLogic___SendFinishGame___2166136261();
		}
	}

	private void RpcWriter___SendFinishedTutorial___3849956746(Player player)
	{
		if (!base.IsClientInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GameTypeSerializers.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		SendServerRpc(56u, pooledWriter, channel, DataOrderType.Default);
		pooledWriter.Store();
	}

	public void RpcLogic___SendFinishedTutorial___3849956746(Player P_0)
	{
		if ((bool)P_0 && !P_0.IsDeinitializing)
		{
			P_0.SetFinishedTutorial();
		}
	}

	private void RpcReader___SendFinishedTutorial___3849956746(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		Player player = GameTypeSerializers.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsServerInitialized)
		{
			RpcLogic___SendFinishedTutorial___3849956746(player);
		}
	}

	private void Awake_UserLogic_Server_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
	}
}
