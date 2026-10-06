using System.Collections;
using UnityEngine;

public class AutoSaver : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Interval for saving in minutes")]
	private float _intervalInMinutes = 5f;

	private void Start()
	{
		StartCoroutine(AutoSave());
	}

	private IEnumerator AutoSave()
	{
		while (true)
		{
			yield return new WaitForSeconds(Mathf.Clamp(_intervalInMinutes, 1f, 60f) * 60f);
			if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
			{
				SaveManager.SaveServer(autoSave: true);
			}
			SaveManager.SaveLocal();
		}
	}
}
