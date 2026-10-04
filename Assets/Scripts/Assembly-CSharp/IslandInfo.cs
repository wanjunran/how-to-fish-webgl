using System;
using UnityEngine;

[Serializable]
public class IslandInfo
{
	[SerializeField]
	private GameObject _spawnPosition;

	[SerializeField]
	private GameObject _warning;

	public Vector3 IslandPosition => _spawnPosition.transform.position;

	public void Toggle(bool to)
	{
		_spawnPosition.SetActive(to);
		_warning.SetActive(!to);
	}
}
