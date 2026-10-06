using UnityEngine;

public class IslandWarning : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !(PlayerManager.GetPlayerFromBodyPart(other.transform) != Player.LocalPlayer))
		{
			PlayerUI.ToggleIslandWarning(to: false);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") && PlayerManager.Players.Count != 1 && !(PlayerManager.GetPlayerFromBodyPart(other.transform) != Player.LocalPlayer) && !DazedUtils.AllPlayersNearbyCheck(other.transform.position, IslandManager.NearPlayerRange))
		{
			PlayerUI.ToggleIslandWarning(to: true);
		}
	}
}
