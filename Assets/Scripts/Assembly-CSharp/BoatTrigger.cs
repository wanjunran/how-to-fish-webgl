using System.Collections.Generic;
using UnityEngine;

public class BoatTrigger : MonoBehaviour
{
	public HashSet<RigidbodySync> ItemsOnBoat = new HashSet<RigidbodySync>();

	public HashSet<Transform> BaitsOnBoat = new HashSet<Transform>();

	private HashSet<RigidbodySync> _tempItemsOnBoat = new HashSet<RigidbodySync>();

	private HashSet<Transform> _tempBaitsOnBoat = new HashSet<Transform>();

	private bool _localPlayerOnBoat;

	private bool _oldLocalPlayerOnBoat;

	private void FixedUpdate()
	{
		TellPlayerTheyAreOnBoat();
		RemoveBoatFromItemsNoLongerInTrigger();
		AddBoatForNewItemsInTrigger();
		HashSet<RigidbodySync> tempItemsOnBoat = _tempItemsOnBoat;
		HashSet<RigidbodySync> itemsOnBoat = ItemsOnBoat;
		ItemsOnBoat = tempItemsOnBoat;
		_tempItemsOnBoat = itemsOnBoat;
		HashSet<Transform> tempBaitsOnBoat = _tempBaitsOnBoat;
		HashSet<Transform> baitsOnBoat = BaitsOnBoat;
		BaitsOnBoat = tempBaitsOnBoat;
		_tempBaitsOnBoat = baitsOnBoat;
		_tempItemsOnBoat.Clear();
		_tempBaitsOnBoat.Clear();
	}

	private void RemoveBoatFromItemsNoLongerInTrigger()
	{
		foreach (RigidbodySync item in ItemsOnBoat)
		{
			if (!_tempItemsOnBoat.Contains(item))
			{
				item.SetBoat(onBoat: false);
			}
		}
	}

	private void AddBoatForNewItemsInTrigger()
	{
		foreach (RigidbodySync item in _tempItemsOnBoat)
		{
			if (!ItemsOnBoat.Contains(item) && !item.OnBoat)
			{
				item.SetBoat(onBoat: true);
			}
		}
	}

	private void TellPlayerTheyAreOnBoat()
	{
		if (_localPlayerOnBoat != _oldLocalPlayerOnBoat)
		{
			Player.LocalPlayer.Movement.SetBoat(_localPlayerOnBoat);
			_oldLocalPlayerOnBoat = _localPlayerOnBoat;
			BoatManager.Boat.SetLocalPlayerOnBoat(_localPlayerOnBoat ? Player.LocalPlayer : null);
		}
		_localPlayerOnBoat = false;
	}

	private void OnTriggerStay(Collider col)
	{
		Transform transform = (col.attachedRigidbody ? col.attachedRigidbody.transform : null);
		if ((bool)col.attachedRigidbody)
		{
			Item item = ItemManager.Get(transform);
			if ((bool)item)
			{
				_tempItemsOnBoat.Add(item.RigidbodySync);
			}
			if (col.gameObject.layer == LayerMask.NameToLayer("LocalPlayer"))
			{
				_localPlayerOnBoat = true;
			}
			if (transform.CompareTag("Bait"))
			{
				_tempBaitsOnBoat.Add(transform);
			}
		}
	}
}
