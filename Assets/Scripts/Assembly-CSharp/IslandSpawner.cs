using UnityEngine;

public class IslandSpawner : MonoBehaviour
{
	[SerializeField]
	private byte _islandIndex;

	public void OnTriggerEnter(Collider other)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized && !OnlineIslandManager.TeleportPlayers && !(Time.time - OnlineIslandManager.TimeWhenSwappingIsland < 5f) && OnlineIslandManager.MaxIslandUnlocked - 1 >= _islandIndex && other.CompareTag("Player") && DazedUtils.AllPlayersNearbyCheck(PlayerManager.GetPlayerFromBodyPart(other.transform).Transform.position, IslandManager.NearPlayerRange))
		{
			OnlineIslandManager.Instance.SpawnIsland(_islandIndex);
		}
	}
}
