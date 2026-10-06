using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
	public static readonly List<Player> Players = new List<Player>();

	public static readonly List<Player> AlivePlayers = new List<Player>();

	public static readonly List<Player> OtherPlayers = new List<Player>();

	private static readonly Dictionary<Transform, Player> TransToPlayer = new Dictionary<Transform, Player>();

	public static bool InGodMode { get; private set; }

	public static event Action OnPlayerAmountChange;

	public static Player GetPlayerFromBodyPart(Transform part)
	{
		if (TransToPlayer.TryGetValue(part, out var value))
		{
			return value;
		}
		return null;
	}

	public static void OnPlayerResurrected(Player player)
	{
		if (!AlivePlayers.Contains(player))
		{
			AlivePlayers.Add(player);
		}
		OnPlayerAmountChange?.Invoke();
	}

	public static void OnPlayerDied(Player player)
	{
		if (AlivePlayers.Contains(player))
		{
			AlivePlayers.Remove(player);
		}
		OnPlayerAmountChange?.Invoke();
	}

	public static void AddPlayer(Player player)
	{
		Players.Add(player);
		if (!player.Owner.IsLocalClient)
		{
			OtherPlayers.Add(player);
		}
		if (!AlivePlayers.Contains(player))
		{
			AlivePlayers.Add(player);
		}
		TransToPlayer.Add(player.Transform, player);
		TransToPlayer.Add(player.Other.Transform, player);
		OnPlayerAmountChange?.Invoke();
	}

	public static void RemovePlayer(Player player)
	{
		Players.Remove(player);
		if (!player.Owner.IsLocalClient)
		{
			OtherPlayers.Remove(player);
		}
		if (AlivePlayers.Contains(player))
		{
			AlivePlayers.Remove(player);
		}
		TransToPlayer.Remove(player.Transform);
		TransToPlayer.Remove(player.Other.Transform);
		OnPlayerAmountChange?.Invoke();
	}

	public static void CheckInventoriesForDestroyedItem(Item item)
	{
		foreach (Player player in Players)
		{
			if (player.Inventory.HasItemInInventory(item))
			{
				player.Inventory.RemoveItem(item);
				break;
			}
		}
	}

	public static Player GetRandomAlivePlayer()
	{
		if (AlivePlayers.Count != 0)
		{
			return AlivePlayers[UnityEngine.Random.Range(0, AlivePlayers.Count)];
		}
		return null;
	}

	public static Player GetNearestAlivePlayer(Vector3 pos)
	{
		if (AlivePlayers.Count == 0)
		{
			return null;
		}
		float num = float.MaxValue;
		Player result = Players[0];
		foreach (Player alivePlayer in AlivePlayers)
		{
			float sqrMagnitude = (alivePlayer.Transform.position - pos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = alivePlayer;
			}
		}
		return result;
	}

	public static void ToggleGodMode()
	{
		InGodMode = !InGodMode;
	}

	public static Player GetVisiblePlayer(Vector3 from)
	{
		Player player = null;
		foreach (Player alivePlayer in AlivePlayers)
		{
			Vector3 direction = alivePlayer.Transform.position - from;
			if (!Physics.Raycast(from, direction, direction.magnitude, GameInfo.LevelLayer) && (!player || (float)UnityEngine.Random.Range(0, 1) < 0.5f))
			{
				player = alivePlayer;
			}
		}
		return player;
	}

	public static Player GetPlayerInFrontOf(Transform inFrontOf)
	{
		Player player = null;
		foreach (Player alivePlayer in AlivePlayers)
		{
			Vector3 vector = alivePlayer.Transform.position - inFrontOf.position;
			if (Vector3.Dot(inFrontOf.forward, vector.normalized) > 0.6f && (!player || (float)UnityEngine.Random.Range(0, 1) < 0.5f))
			{
				player = alivePlayer;
			}
		}
		return player;
	}
}
