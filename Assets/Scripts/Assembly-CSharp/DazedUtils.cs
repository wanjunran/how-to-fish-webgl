using System;
using System.Collections.Generic;
using UnityEngine;

public static class DazedUtils
{
	private static readonly float MinimumSimulatedDeltaTime = 1f / 30f;

	public static void PlayCreatureHitEffects(Vector3 point, Vector3 dir, int damage, float bloodSizeMultiplier, bool useSmallBloodParticles, bool isDead, int hp, Player playerWhoHit)
	{
		PlayBloodEffects(point, dir, damage, 0.75f, 1.25f, bloodSizeMultiplier, useSmallBloodParticles);
		Item item = (playerWhoHit ? playerWhoHit.Holding.HeldItem : null);
		if (!item || ((bool)item.Melee && item.Melee.UsePunchSoundOnHit))
		{
			AudioManager.PlayClipAt("FishPunchHit", point, variation: true, AudioDistance.VeryShort, 0.4f);
		}
		else
		{
			AudioManager.PlayRandomClipAt("KnifeHit_0", 1, 4, point, variation: true, AudioDistance.VeryShort, 0.5f);
		}
		if (!isDead)
		{
			PlayerUI.AddDamageNumber(point, damage, hp - damage <= 0);
		}
	}

	public static void PlayDeadPlayerHitEffects(Vector3 point, Vector3 dir, int damage, Player playerWhoHit, bool noDecals = false)
	{
		DamageType damageType = DamageType.Penetration;
		if ((bool)playerWhoHit)
		{
			if (!playerWhoHit.Holding.HeldItem || ((bool)playerWhoHit.Holding.HeldItem.Melee && playerWhoHit.Holding.HeldItem.Melee.UsePunchSoundOnHit))
			{
				damageType = DamageType.Generic;
			}
			else if ((bool)playerWhoHit.Holding.HeldItem && (bool)playerWhoHit.Holding.HeldItem.Weapon)
			{
				damageType = DamageType.Penetration;
			}
		}
		bool useSmallBloodParticles = damageType == DamageType.Generic;
		PlayBloodEffects(point, dir, damage, 0.75f, 1.25f, 1f, useSmallBloodParticles, noDecals);
		switch (damageType)
		{
		case DamageType.Generic:
			AudioManager.PlayClipAt("FishPunchHit", point, variation: true, AudioDistance.Short, 0.4f);
			break;
		case DamageType.Penetration:
			AudioManager.PlayRandomClipAt("TakeDamage_0", 1, 5, point, variation: false, AudioDistance.Short, 0.5f);
			break;
		case DamageType.Bite:
			AudioManager.PlayRandomClipAt("FishBite_V", 1, 2, point, variation: true, AudioDistance.Short);
			break;
		}
	}

	public static void PlayPlayerHitEffects(Vector3 point, Vector3 dir, int damage, Player player, Player playerWhoHit, int health, DamageType type = DamageType.Generic)
	{
		if ((bool)playerWhoHit)
		{
			if (!playerWhoHit.Holding.HeldItem || ((bool)playerWhoHit.Holding.HeldItem.Melee && playerWhoHit.Holding.HeldItem.Melee.UsePunchSoundOnHit))
			{
				type = DamageType.Generic;
			}
			else if ((bool)playerWhoHit.Holding.HeldItem && (bool)playerWhoHit.Holding.HeldItem.Weapon)
			{
				type = DamageType.Penetration;
			}
		}
		bool useSmallBloodParticles = type == DamageType.Generic;
		PlayBloodEffects(point, dir, damage, 0.75f, 1.25f, 1f, useSmallBloodParticles);
		switch (type)
		{
		case DamageType.Generic:
			AudioManager.PlayPlayerClip("FishPunchHit", player, variation: true, AudioDistance.Short);
			break;
		case DamageType.Penetration:
			AudioManager.PlayRandomPlayerClip("TakeDamage_0", 1, 5, player, variation: false, AudioDistance.Short);
			break;
		case DamageType.Bite:
			AudioManager.PlayRandomPlayerClip("FishBite_V", 1, 2, player, variation: true, AudioDistance.Short);
			break;
		}
		if (health > 0 && (bool)playerWhoHit && !player.Owner.IsLocalClient)
		{
			PlayerUI.AddDamageNumber(point, damage, health - damage <= 0);
		}
	}

	private static void PlayBloodEffects(Vector3 point, Vector3 dir, int damage, float minDecalSizeMultiplier, float maxDecalSizeMultiplier, float bloodSizeMultiplier = 1f, bool useSmallBloodParticles = false, bool noDecals = false)
	{
		if (!DecalManager.UseBlood)
		{
			return;
		}
		ParticleManager.Play(useSmallBloodParticles ? "BloodSmall" : "Blood", point, Vector3.up);
		if (!noDecals)
		{
			if (Physics.Raycast(point, dir, out var hitInfo, 2f, GameInfo.LevelLayer))
			{
				DecalManager.SpawnDecal("BloodDecal" + UnityEngine.Random.Range(1, 4), hitInfo.point, hitInfo.normal, GameInfo.BloodDecalSizeMulti.Evaluate(damage) * UnityEngine.Random.Range(minDecalSizeMultiplier, maxDecalSizeMultiplier) * bloodSizeMultiplier, 1f);
			}
			else if (Physics.Raycast(point, Vector3.down, out hitInfo, 3f, GameInfo.LevelLayer))
			{
				DecalManager.SpawnDecal("BloodDecal" + UnityEngine.Random.Range(1, 4), hitInfo.point, hitInfo.normal, GameInfo.BloodDecalSizeMulti.Evaluate(damage) * UnityEngine.Random.Range(minDecalSizeMultiplier, maxDecalSizeMultiplier) * bloodSizeMultiplier, 1f);
			}
		}
	}

	public static Vector3 GetAngularVelocityToTarget(Quaternion current, Quaternion target)
	{
		Quaternion quaternion = target * Quaternion.Inverse(current);
		if (quaternion.w < 0f)
		{
			quaternion.x = 0f - quaternion.x;
			quaternion.y = 0f - quaternion.y;
			quaternion.z = 0f - quaternion.z;
			quaternion.w = 0f - quaternion.w;
		}
		quaternion.ToAngleAxis(out var angle, out var axis);
		if (angle > 180f)
		{
			angle -= 360f;
		}
		return axis * (angle * (MathF.PI / 180f));
	}

	public static void SimulateSpringRotation(ref Quaternion currentRot, ref Vector3 angVel, Quaternion targetRot, float speed, float damping)
	{
		float num = Time.deltaTime;
		while (num > 0f)
		{
			float num2 = Mathf.Min(num, MinimumSimulatedDeltaTime);
			num -= num2;
			Vector3 vector = GetAngularVelocityToTarget(currentRot, targetRot) * speed;
			angVel += vector * num2;
			angVel *= Mathf.Exp((0f - damping) * num2);
			currentRot = Quaternion.Euler(angVel * num2) * currentRot;
		}
	}

	public static bool AreVectorsEqual(Vector3 a, Vector3 b, float epsilon = 0.0001f)
	{
		return (a - b).sqrMagnitude < epsilon * epsilon;
	}

	public static bool AreQuaternionsEqual(Quaternion a, Quaternion b, float epsilon = 0.0001f)
	{
		return Mathf.Abs(Quaternion.Dot(a, b)) > 1f - epsilon;
	}

	public static bool CheckIfStationary(Rigidbody rigToCheck)
	{
		bool num = rigToCheck.linearVelocity.sqrMagnitude < 0.01f;
		bool flag = rigToCheck.angularVelocity.sqrMagnitude < 0.01f;
		return num & flag;
	}

	public static bool TryMoveItemFromUnderLevel(Item item)
	{
		Vector3 position = item.transform.position;
		Physics.Raycast(maxDistance: (position.y = 100f) - item.transform.position.y, origin: position, direction: Vector3.down, hitInfo: out var hitInfo, layerMask: GameInfo.LevelLayer, queryTriggerInteraction: QueryTriggerInteraction.Ignore);
		if ((bool)hitInfo.transform && item.RigidbodySync.IsSimulatedLocal)
		{
			Vector3 point = hitInfo.point;
			point.y += item.ModelHeight;
			if (!item.Rig.isKinematic)
			{
				item.Rig.linearVelocity = Vector3.zero;
			}
			item.RigidbodySync.TeleportToPosRot(point, Quaternion.identity);
		}
		return hitInfo.transform;
	}

	public static bool AllPlayersNearbyCheck(Vector3 point, float dist)
	{
		HashSet<Player> hashSet = new HashSet<Player>();
		foreach (Player player in PlayerManager.Players)
		{
			if (((!player.Dying.IsDead) ? (player.Transform.position - point) : (player.Dying.DeadPlayer.transform.position - point)).sqrMagnitude < dist * dist)
			{
				hashSet.Add(player);
			}
		}
		return hashSet.Count == PlayerManager.Players.Count;
	}
}
