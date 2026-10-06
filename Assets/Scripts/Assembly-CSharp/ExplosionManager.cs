using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

public class ExplosionManager : NetworkBehaviour
{
	public static ExplosionManager Instance;

	[SerializeField]
	private List<ItemInfoWeight> _explodabaleItemWeights = new List<ItemInfoWeight>();

	private bool NetworkInitialize___EarlyExplosionManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateExplosionManagerAssembly_002DCSharp_002Edll_Excuted;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ExplosionManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public static void ServerExplode(Item item, ExplosionInfo info)
	{
		if (!item || (info.HasExploded && info.OnlyExplodeOnce) || !Instance)
		{
			return;
		}
		info.Explode();
		float waterHeight = WaterManager.GetWaterHeight(item.transform.position);
		bool flag = waterHeight > item.transform.position.y && info.HasUnderwaterExplosion;
		List<Creature> list = new List<Creature>();
		List<Item> list2 = new List<Item>();
		List<Creature> list3 = new List<Creature>();
		Player player = item.LastPlayer;
		if ((bool)item.Explosive && (bool)item.Explosive.PlayerWhoForcedExplosion)
		{
			player = item.Explosive.PlayerWhoForcedExplosion;
		}
		if (flag)
		{
			Vector3 position = item.transform.position;
			position.y = waterHeight - 0.1f;
			int num = Random.Range(info.UnderWaterFishMinMax.x, info.UnderWaterFishMinMax.y);
			for (int i = 0; i < num; i++)
			{
				Fishable randomItem = CreatureManager.Instance.GetRandomItem(position, Instance._explodabaleItemWeights);
				Vector3 vector = Random.insideUnitSphere * info.ForceRadius / 2f;
				vector.y = 0f;
				Vector3 vector2 = position + vector;
				Item item2 = Object.Instantiate(randomItem.ItemToSpawn, vector2, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
				if ((bool)item2.Creature)
				{
					item2.Creature.ServerKillOnSpawn();
					list.Add(item2.Creature);
					list3.Add(item2.Creature);
				}
				Instance.Spawn(item2.gameObject);
				Vector3 force = vector2 - position;
				force.Normalize();
				force *= info.ItemForce * 0.15f;
				float num2 = Random.Range(0.8f, 1.2f);
				force.y = Mathf.Abs(info.ItemForce) * num2;
				item2.RigidbodySync.StartSimulateLocal();
				item2.Rig.AddForce(force);
			}
		}
		Collider[] array = CreateExplosion(item, info);
		List<Player> list4 = new List<Player>();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.transform == item.transform)
			{
				continue;
			}
			Vector3 vector3 = item.Rig.worldCenterOfMass - collider.transform.position;
			Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(collider.transform);
			if ((bool)playerFromBodyPart && !playerFromBodyPart.IsDeinitializing && vector3.sqrMagnitude <= info.DamageRadius * info.DamageRadius && !flag)
			{
				if (!list4.Contains(playerFromBodyPart))
				{
					list4.Add(playerFromBodyPart);
					Vector3 force2 = -vector3.normalized;
					force2 *= info.PlayerForce;
					Server.Instance.HitPlayer(playerFromBodyPart, info.Damage, force2, collider.transform.position, 0, player);
					if ((bool)player && playerFromBodyPart != player && playerFromBodyPart.Vitals.Health > 0 && playerFromBodyPart.Vitals.Health - info.Damage <= 0 && ServerSettings.UseFriendlyFire)
					{
						Instance.SendExplosionKills(player.Owner, playerFromBodyPart.SteamName, 100);
					}
				}
				continue;
			}
			Item item3 = ItemManager.Get(collider);
			if ((bool)item3 && !list2.Contains(item3) && item3 != item)
			{
				list2.Add(item3);
				if ((bool)item3.Creature)
				{
					if (list.Contains(item3.Creature))
					{
						continue;
					}
					if (!item3.Creature.IsDead && vector3.sqrMagnitude <= info.DamageRadius * info.DamageRadius)
					{
						int damageOnCreature = Instance.GetDamageOnCreature(info.Damage, item3.Creature);
						item3.Creature.ServerChangeHp(damageOnCreature);
						if (item3.Creature.Hp - damageOnCreature <= 0)
						{
							list3.Add(item3.Creature);
							if ((bool)player && (bool)item3.Creature.Bird)
							{
								Instance.SendSeagullDynamiteKillAchievement(player.Owner);
							}
						}
						list.Add(item3.Creature);
					}
				}
				item3.RigidbodySync.StartSimulateLocal();
				Vector3 force3 = item3.transform.position - item.transform.position;
				force3.y += 2f;
				force3.Normalize();
				force3 *= info.ItemForce;
				if (!item3.Explosive && (!item3.Creature || item3.Creature.BossType == BossType.None))
				{
					item3.Rig.AddForce(force3);
				}
				item3.WasInteractedWith();
				if ((bool)item3.Explosive)
				{
					item3.Explosive.ForceExplode(player, instant: false);
				}
			}
			if (BoatManager.ColToBoat.TryGetValue(collider, out var value))
			{
				value.HiddenPhysicsRig.AddExplosionForce(info.BoatForce, item.transform.position, info.DamageRadius, 2f);
			}
		}
		foreach (Creature item4 in list)
		{
			Vector3 dir = item4.transform.position - item.transform.position;
			int damageOnCreature2 = Instance.GetDamageOnCreature(info.Damage, item4);
			item4.ObserverExplosionHit(player, dir, damageOnCreature2);
		}
		string text = "";
		int num3 = 0;
		Dictionary<string, Vector2Int> dictionary = new Dictionary<string, Vector2Int>();
		foreach (Creature item5 in list3)
		{
			if (!dictionary.TryAdd(item5.GetName(), new Vector2Int(1, item5.TotalWorth)))
			{
				dictionary[item5.GetName()] += new Vector2Int(1, item5.TotalWorth);
			}
		}
		foreach (KeyValuePair<string, Vector2Int> item6 in dictionary)
		{
			if (text != "")
			{
				text += ", ";
			}
			text = ((item6.Value.x != 1) ? (text + $"{item6.Value.x}x {item6.Key}") : (text + item6.Key));
			num3 += item6.Value.y;
		}
		if (list3.Count > 0 && (bool)player)
		{
			Instance.SendExplosionKills(player.Owner, text, num3);
		}
		foreach (Creature item7 in list3)
		{
			item7.SetKillscoreMultiplier(1.25f);
		}
		ExplodeEffects(item.transform.position, info);
		Instance.ObserverExplode(item, item.transform.position);
		if (info.OnlyExplodeOnce)
		{
			item.Despawn();
		}
	}

	private int GetDamageOnCreature(int baseDamage, Creature creature)
	{
		return baseDamage + (int)((float)creature.MaxHp * 0.05f);
	}

	[ObserversRpc]
	private void ObserverExplode(Item item, Vector3 pos)
	{
		RpcWriter___ObserverExplode___4259294983(item, pos);
	}

	[TargetRpc]
	private void SendExplosionKills(NetworkConnection netCon, string creatureNames, int worth)
	{
		RpcWriter___SendExplosionKills___3905681115(netCon, creatureNames, worth);
	}

	[TargetRpc]
	private void SendSeagullDynamiteKillAchievement(NetworkConnection netCon)
	{
		RpcWriter___SendSeagullDynamiteKillAchievement___328543758(netCon);
	}

	private static void ExplodeEffects(Vector3 pos, ExplosionInfo info)
	{
		float waterHeight = WaterManager.GetWaterHeight(pos);
		if (waterHeight > pos.y && info.HasUnderwaterExplosion)
		{
			VFXManager.Play("WaterExplosion", new Vector3(pos.x, waterHeight + 0.15f, pos.z), Vector3.zero);
			AudioManager.PlayRandomClipAt("ExplosionUnderwater_V", 1, 3, pos, variation: false, AudioDistance.Long, info.ExplosionSoundVol, 0.2f);
		}
		else
		{
			ParticleManager.Play(info.ExplosionParticleName, pos, Vector3.zero);
			if (Physics.Raycast(pos, Vector3.down, 1f, GameInfo.LevelLayer))
			{
				ParticleManager.Play("Ashes", pos, Vector3.up);
			}
			if (info.ExplosionSounds == 0)
			{
				AudioManager.PlayClipAt(info.ExplosionSoundName, pos, variation: true, AudioDistance.Long, info.ExplosionSoundVol, 0.2f);
			}
			else
			{
				AudioManager.PlayRandomClipAt(info.ExplosionSoundName, 1, info.ExplosionSounds, pos, variation: false, AudioDistance.Long, info.ExplosionSoundVol, 0.2f);
			}
		}
		Player.LocalPlayer.ScreenShake.ShakeAt(pos, 1, info.ScreenShakeAmount);
	}

	private static Collider[] CreateExplosion(Item item, ExplosionInfo info)
	{
		Collider[] array = Physics.OverlapSphere(item.transform.position, info.ForceRadius, GameInfo.AffectedByExplosionLayer);
		Collider[] array2 = array;
		foreach (Collider key in array2)
		{
			if (RigidbodyManager.Debris.ContainsKey(key))
			{
				RigidbodyManager.Debris[key].AddExplosionForce(info.ItemForce, item.transform.position + Vector3.down, info.ForceRadius);
			}
		}
		return array;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyExplosionManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyExplosionManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(0u, RpcReader___ObserverExplode___4259294983);
			RegisterTargetRpc(1u, RpcReader___SendExplosionKills___3905681115);
			RegisterTargetRpc(2u, RpcReader___SendSeagullDynamiteKillAchievement___328543758);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateExplosionManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateExplosionManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverExplode___4259294983(Item item, Vector3 pos)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GeneratedWriters___Internal.GWrite___ItemFishNet_002ESerializing_002EGenerated(pooledWriter, item);
		pooledWriter.WriteVector3(pos);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverExplode___4259294983(Item P_0, Vector3 P_1)
	{
		if ((bool)P_0)
		{
			ExplosionInfo explosionInfo = P_0.GetExplosionInfo();
			if (explosionInfo != null && (!explosionInfo.HasExploded || !explosionInfo.OnlyExplodeOnce))
			{
				P_0.transform.position = P_1;
				explosionInfo.Explode();
				CreateExplosion(P_0, explosionInfo);
				ExplodeEffects(P_1, explosionInfo);
			}
		}
	}

	private void RpcReader___ObserverExplode___4259294983(PooledReader PooledReader0, Channel channel)
	{
		Item item = GeneratedReaders___Internal.GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Vector3 vector = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverExplode___4259294983(item, vector);
		}
	}

	private void RpcWriter___SendExplosionKills___3905681115(NetworkConnection netCon, string creatureNames, int worth)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteString(creatureNames);
		pooledWriter.WriteInt32(worth);
		SendTargetRpc(1u, pooledWriter, channel, DataOrderType.Default, netCon, excludeServer: false);
		pooledWriter.Store();
	}

	private void RpcLogic___SendExplosionKills___3905681115(NetworkConnection P_0, string P_1, int P_2)
	{
		Player.LocalPlayer.KillScore.AddKillScore(P_1, P_2, KillScoreCalculator.GetExplosionBonuses());
	}

	private void RpcReader___SendExplosionKills___3905681115(PooledReader PooledReader0, Channel channel)
	{
		string text = PooledReader0.ReadStringAllocated();
		int num = PooledReader0.ReadInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___SendExplosionKills___3905681115(base.LocalConnection, text, num);
		}
	}

	private void RpcWriter___SendSeagullDynamiteKillAchievement___328543758(NetworkConnection netCon)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendTargetRpc(2u, pooledWriter, channel, DataOrderType.Default, netCon, excludeServer: false);
		pooledWriter.Store();
	}

	private void RpcLogic___SendSeagullDynamiteKillAchievement___328543758(NetworkConnection P_0)
	{
		AchievementManager.CheckSeagullDynamiteKillAchievement();
	}

	private void RpcReader___SendSeagullDynamiteKillAchievement___328543758(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___SendSeagullDynamiteKillAchievement___328543758(base.LocalConnection);
		}
	}

	private void Awake_UserLogic_ExplosionManager_Assembly_002DCSharp_002Edll()
	{
		Setter.SetSingleInstance(ref Instance, this);
	}
}
