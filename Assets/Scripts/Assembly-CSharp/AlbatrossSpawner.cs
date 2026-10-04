using FishNet;
using UnityEngine;

public class AlbatrossSpawner : MonoBehaviour
{
	[SerializeField]
	private Albatross _albatrossPrefab;

	[SerializeField]
	private Creature _albatrossFood;

	[SerializeField]
	private uint _totalTicksWithBossInZone = 100u;

	[SerializeField]
	private Transform _spawnPoint;

	private Creature _bossItem;

	private uint _ticksWithBossInZone;

	private void Start()
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			InstanceFinder.TimeManager.OnTick += TickUpdate;
		}
	}

	private void TickUpdate()
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized && !BossManager.Boss && (bool)_bossItem)
		{
			if ((bool)_bossItem.Holder)
			{
				Reset();
			}
			_ticksWithBossInZone++;
			if (_ticksWithBossInZone > _totalTicksWithBossInZone)
			{
				_ticksWithBossInZone = 0u;
				Albatross albatross = Object.Instantiate(_albatrossPrefab, _spawnPoint.position, _spawnPoint.rotation);
				albatross.SetStartingTargetItem(_bossItem);
				Server.Instance.Spawn(albatross.gameObject);
				_bossItem.ToggleInteractable(to: false);
				_bossItem = null;
			}
		}
	}

	private void OnTriggerStay(Collider col)
	{
		if ((bool)BossManager.Boss)
		{
			return;
		}
		Item item = ItemManager.Get(col);
		if ((bool)item && item.IsInteractable && item.ID == _albatrossFood.ID)
		{
			if (item == _bossItem && (bool)item.Holder)
			{
				Reset();
			}
			else if (!_bossItem)
			{
				_bossItem = item.Creature;
			}
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if ((bool)_bossItem)
		{
			Item item = ItemManager.Get(col);
			if ((bool)item && !item.IsDestroying && !(item != _bossItem))
			{
				Reset();
			}
		}
	}

	private void Reset()
	{
		_bossItem = null;
		_ticksWithBossInZone = 0u;
	}
}
