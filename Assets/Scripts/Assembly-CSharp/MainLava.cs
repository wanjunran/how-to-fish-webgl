using System.Collections;
using UnityEngine;

public class MainLava : MonoBehaviour
{
	[SerializeField]
	private Creature _whalePrefab;

	[SerializeField]
	private Creature _mutatedWhalePrefab;

	[SerializeField]
	private Transform _mutatedWhaleSpawnPoint;

	[SerializeField]
	private float _playerUpForce = 5f;

	private bool _isSpawningWhale;

	private float _prevCookTime;

	public static Vector3 LavaPosition { get; private set; }

	private void Awake()
	{
		LavaPosition = base.transform.position;
	}

	private void OnTriggerEnter(Collider col)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			Item item = ItemManager.Get(col);
			if ((bool)item && item.IsClientInitialized && !item.Holder && (!item.Bird || item.Bird.IsDead) && (!item.Creature || item.Creature.BossType == BossType.None || item.Creature.IsDead) && item.ID == _whalePrefab.ID && !_isSpawningWhale && !BossManager.Boss)
			{
				StartCoroutine(SpawnMutatedWhale());
				item.DestroyItem(6);
			}
		}
	}

	private IEnumerator SpawnMutatedWhale()
	{
		_isSpawningWhale = true;
		int num = 3;
		ObserverToLocalAdapter.Instance.EruptVolcano(num);
		yield return new WaitForSeconds(num);
		Creature creature = Object.Instantiate(_mutatedWhalePrefab, _mutatedWhaleSpawnPoint.position, _mutatedWhaleSpawnPoint.rotation);
		Server.Instance.Spawn(creature.gameObject);
		_isSpawningWhale = false;
	}

	private void OnTriggerStay(Collider col)
	{
		TouchLava(col);
	}

	public void TouchLava(Collider col)
	{
		Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(col.transform);
		if ((bool)playerFromBodyPart)
		{
			PlayerTouchedLava(playerFromBodyPart);
			return;
		}
		Item item = ItemManager.Get(col);
		if ((bool)item)
		{
			ItemTouchedLava(item);
		}
	}

	private void PlayerTouchedLava(Player player)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			player.Vitals.ApplyNewFire();
		}
	}

	private void ItemTouchedLava(Item item)
	{
		if ((bool)item.LastHolder && item.ID != _whalePrefab.ID && (!item.Creature || item.Creature.IsDead) && item.RigidbodySync.IsSimulatedLocal && !(Time.time - _prevCookTime < 0.4f))
		{
			AutoJump(item);
			_prevCookTime = Time.time;
			if (!Server.Instance || !Server.Instance.IsServerInitialized)
			{
				Server.Instance.GrillItemInLava(item);
			}
			else
			{
				item.CookWithlava();
			}
		}
	}

	private void AutoJump(Item item)
	{
		Vector3 linearVelocity = item.Rig.linearVelocity;
		linearVelocity.y = _playerUpForce;
		item.Rig.linearVelocity = linearVelocity;
	}
}
