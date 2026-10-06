using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
	[SerializeField]
	private bool _isActive = true;

	[SerializeField]
	private Item _itemToSpawn;

	[SerializeField]
	private bool _spawnsDeadCreature;

	[SerializeField]
	[Range(88f, 108f)]
	private int _startFrequencyForRadio = 98;

	[SerializeField]
	private bool _useRandomRotation = true;

	[SerializeField]
	private int _maxItemsSpawned = 10;

	[Space]
	[SerializeField]
	private bool _spawnInstant = true;

	[SerializeField]
	private bool _onlySpawnOnce;

	[SerializeField]
	private bool _spawnInAir;

	[Space]
	[SerializeField]
	private float _spawnDelay = 10f;

	[SerializeField]
	[Range(1f, 25f)]
	private int _spawnCount = 1;

	[SerializeField]
	private BoxCollider _spawnBox;

	private bool _hasSpawned;

	private List<Item> _itemsSpawned = new List<Item>();

	public void Start()
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			StartCoroutine(SpawnItem());
		}
	}

	private IEnumerator SpawnItem()
	{
		if (!_itemToSpawn)
		{
			yield break;
		}
		yield return new WaitUntil(() => ItemManager.Instance);
		while ((!_onlySpawnOnce || !_hasSpawned) && _isActive)
		{
			if (!_spawnInstant || _hasSpawned)
			{
				yield return new WaitForSeconds(_spawnDelay);
			}
			CheckSpawnedItems();
			for (int num = 0; num < _spawnCount; num++)
			{
				if (_itemsSpawned.Count >= _maxItemsSpawned)
				{
					break;
				}
				Vector3 randomSpawnPoint = SpawnUtils.GetRandomSpawnPoint(_spawnBox, _spawnInAir);
				if (!(randomSpawnPoint == Vector3.zero))
				{
					randomSpawnPoint += Vector3.up * _itemToSpawn.ModelHeight;
					Quaternion rot = (_useRandomRotation ? Quaternion.Euler(0f, Random.Range(0, 360), 0f) : base.transform.rotation);
					Item item = ItemManager.Instance.SpawnNewItem(_itemToSpawn, randomSpawnPoint, rot);
					_itemsSpawned.Add(item);
					if ((bool)item.Radio)
					{
						item.Radio.SetStartFrequency(_startFrequencyForRadio);
					}
				}
			}
			_hasSpawned = true;
		}
	}

	private void CheckSpawnedItems()
	{
		for (int num = _itemsSpawned.Count - 1; num >= 0; num--)
		{
			if (!_itemsSpawned[num] || !_itemsSpawned[num].gameObject.activeInHierarchy || (bool)_itemsSpawned[num].Holder || ((bool)_itemsSpawned[num].Creature && _itemsSpawned[num].Creature.IsDead && !_spawnsDeadCreature))
			{
				_itemsSpawned.RemoveAt(num);
			}
		}
	}
}
