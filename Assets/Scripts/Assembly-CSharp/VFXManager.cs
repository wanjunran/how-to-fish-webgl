using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
	private static readonly Dictionary<string, VisualEffect> _allVfx = new Dictionary<string, VisualEffect>();

	[SerializeField]
	private Transform _vfxHolder;

	private void Awake()
	{
		_allVfx.Clear();
		for (int i = 0; i < _vfxHolder.childCount; i++)
		{
			_allVfx.Add(_vfxHolder.GetChild(i).name, _vfxHolder.GetChild(i).GetComponent<VisualEffect>());
		}
	}

	private void Start()
	{
		foreach (KeyValuePair<string, VisualEffect> item in _allVfx)
		{
			Play(item.Key, Vector3.down * 1000f);
		}
	}

	public static void Play(string type, Vector3 position)
	{
		Play(type, position, Vector3.zero);
	}

	public static void Play(string type, Vector3 position, Vector3 direction)
	{
		if (!string.IsNullOrEmpty(type))
		{
			if (!_allVfx.ContainsKey(type))
			{
				Debug.LogWarning($"Could not find {type} in {_allVfx}");
				return;
			}
			_allVfx[type].SetVector3("Position", position);
			_allVfx[type].SetVector3("Direction", direction);
			_allVfx[type].SendEvent("Play");
		}
	}
}
