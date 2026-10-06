using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DecalManager : MonoBehaviour
{
	private static Dictionary<string, List<DecalProjector>> _decals;

	private static Dictionary<string, int> _decalIndexes;

	[SerializeField]
	private Transform _decalHolder;

	[SerializeField]
	private DecalInfo[] _prefabs;

	private static bool _hideDecals;

	public static bool UseBlood { get; private set; } = true;

	public static bool UseDamageNumbers { get; private set; } = true;

	private void Awake()
	{
		_decals = new Dictionary<string, List<DecalProjector>>();
		_decalIndexes = new Dictionary<string, int>();
		DecalInfo[] prefabs = _prefabs;
		foreach (DecalInfo decalInfo in prefabs)
		{
			List<DecalProjector> list = new List<DecalProjector>();
			for (int j = 0; j < decalInfo.Amount; j++)
			{
				GameObject gameObject = Object.Instantiate(decalInfo.DecalPrefab, Vector3.up * 10000f, Quaternion.identity, _decalHolder);
				gameObject.SetActive(value: false);
				list.Add(gameObject.GetComponent<DecalProjector>());
			}
			_decals.Add(decalInfo.DecalPrefab.name, list);
			_decalIndexes.Add(decalInfo.DecalPrefab.name, 0);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Period))
		{
			ToggleDecals(!_hideDecals);
		}
	}

	public static void ClearDecals()
	{
		foreach (KeyValuePair<string, List<DecalProjector>> decal in _decals)
		{
			foreach (DecalProjector item in decal.Value)
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	public static void ToggleDecals(bool to)
	{
		_hideDecals = !to;
		if (_hideDecals)
		{
			ClearDecals();
		}
	}

	public static void SpawnDecal(string decalName, Vector3 pos, Vector3 normal, float scale = 1f, float scaleTime = 0.1f)
	{
		if (!_hideDecals && _decals.ContainsKey(decalName))
		{
			DecalProjector decalProjector = _decals[decalName][_decalIndexes[decalName]];
			Transform transform = decalProjector.transform;
			_decalIndexes[decalName]++;
			if (_decalIndexes[decalName] >= _decals[decalName].Count)
			{
				_decalIndexes[decalName] = 0;
			}
			transform.rotation = Quaternion.Euler(Quaternion.LookRotation(normal).eulerAngles);
			transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), normal) * transform.rotation;
			transform.position = pos;
			transform.localScale = Vector3.zero;
			decalProjector.gameObject.SetActive(value: true);
			decalProjector.size = new Vector3(scale, scale, decalProjector.size.z);
			LeanTween.cancel(transform.gameObject);
			LeanTween.scale(transform.gameObject, Vector3.one, scaleTime).setEaseOutQuint();
		}
	}

	public static void ToggleBlood(bool to)
	{
		UseBlood = to;
		if (!to)
		{
			ClearDecals();
		}
	}

	public static void ToggleDamageNumbers(bool to)
	{
		UseDamageNumbers = to;
	}
}
