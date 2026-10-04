using System.Collections.Generic;
using UnityEngine;

public class GrillBox : MonoBehaviour
{
	[SerializeField]
	private float _cookPerTick = 0.025f;

	[SerializeField]
	private AudioSource _sizzlingSource;

	[SerializeField]
	private AudioSource _fireSource;

	[SerializeField]
	private float _sizzlingVol = 0.25f;

	[SerializeField]
	private float _sizzlingVolTweenTime = 0.5f;

	[SerializeField]
	private ParticleSystem _smokeParticles;

	[SerializeField]
	private ParticleSystem _grillParticles;

	private List<Item> _items = new List<Item>();

	private bool _isUnlocked;

	private void Awake()
	{
		NPCManager.OnUnlockedGrill += OnUnlockedGrill;
	}

	private void OnDestroy()
	{
		NPCManager.OnUnlockedGrill -= OnUnlockedGrill;
	}

	private void Start()
	{
		if ((bool)NPCManager.Instance && NPCManager.Instance.GrillUnlocked)
		{
			OnUnlockedGrill();
		}
	}

	private void OnUnlockedGrill()
	{
		_isUnlocked = true;
		_smokeParticles.Play();
		_grillParticles.Play();
		_fireSource.Play();
		AudioManager.PlayClipAt("Ignite", base.transform.position, variation: true, AudioDistance.VeryShort, 0.1f);
	}

	private void FixedUpdate()
	{
		for (int num = _items.Count - 1; num >= 0; num--)
		{
			if (!_items[num] || !_items[num].gameObject.activeInHierarchy || ((bool)_items[num].Creature && (bool)_items[num].Creature.BirdHolder))
			{
				_items.RemoveAt(num);
				if (_items.Count == 0)
				{
					ToggleAudio(enable: false);
				}
			}
		}
		if (!Server.Instance || !Server.Instance.IsServerInitialized)
		{
			return;
		}
		foreach (Item item in _items)
		{
			item.CookItem(_cookPerTick);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (!_isUnlocked)
		{
			return;
		}
		Item item = ItemManager.Get(other);
		if ((bool)item)
		{
			if (!_items.Contains(item))
			{
				_items.Add(item);
			}
			if (_items.Count != 0)
			{
				ToggleAudio(enable: true);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!_isUnlocked)
		{
			return;
		}
		Item item = ItemManager.Get(other);
		if ((bool)item)
		{
			if (_items.Contains(item))
			{
				_items.Remove(item);
			}
			if (_items.Count <= 0)
			{
				ToggleAudio(enable: false);
			}
		}
	}

	private void ToggleAudio(bool enable)
	{
		LeanTween.cancel(_sizzlingSource.gameObject);
		LeanTween.value(_sizzlingSource.gameObject, _sizzlingSource.volume, enable ? _sizzlingVol : 0f, _sizzlingVolTweenTime).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateVolume);
	}

	private void UpdateVolume(float to)
	{
		_sizzlingSource.volume = to;
	}
}
