using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
	[SerializeField]
	private string _musicToPlay;

	private bool _isPlaying;

	private void OnDestroy()
	{
		MusicManager.StopMusic(_musicToPlay);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !BossManager.Boss && !_isPlaying && !(PlayerManager.GetPlayerFromBodyPart(other.transform) != Player.LocalPlayer))
		{
			_isPlaying = true;
			MusicManager.PlayMusic(_musicToPlay);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player") && _isPlaying && !(PlayerManager.GetPlayerFromBodyPart(other.transform) != Player.LocalPlayer))
		{
			_isPlaying = false;
			MusicManager.StopMusic(_musicToPlay);
		}
	}
}
