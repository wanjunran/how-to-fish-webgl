using System.Collections;
using UnityEngine;

public class EndGameToggles : MonoBehaviour
{
	[SerializeField]
	private GameObject _disableInEndGame;

	[SerializeField]
	private GameObject _enableInEndGame;

	private void Start()
	{
		StartCoroutine(DelayedInit());
	}

	private IEnumerator DelayedInit()
	{
		yield return new WaitUntil(() => EndGameManager.Instance);
		if ((bool)_disableInEndGame)
		{
			_disableInEndGame.SetActive(!EndGameManager.Instance.HasFinishedGame);
		}
		if ((bool)_enableInEndGame)
		{
			_enableInEndGame.SetActive(EndGameManager.Instance.HasFinishedGame);
		}
	}
}
