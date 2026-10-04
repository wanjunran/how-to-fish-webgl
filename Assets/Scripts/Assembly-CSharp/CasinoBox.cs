using System.Collections.Generic;
using UnityEngine;

public class CasinoBox : MonoBehaviour
{
	[SerializeField]
	private BoxCollider _col;

	private void FixedUpdate()
	{
		if (!Server.Instance || !Server.Instance.IsServerInitialized || ((bool)CasinoManager.Instance && CasinoManager.IsBetting))
		{
			return;
		}
		Vector3 center = _col.transform.TransformPoint(_col.center);
		Vector3 halfExtents = Vector3.Scale(_col.size * 0.5f, base.transform.localScale);
		Collider[] array = Physics.OverlapBox(center, halfExtents, base.transform.rotation, GameInfo.ItemLayer);
		List<Item> list = new List<Item>();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.transform.CompareTag("Item"))
			{
				Item item = ItemManager.Get(collider);
				if ((bool)item && !list.Contains(item) && !item.DeadPlayer && (!item.Creature || item.Creature.IsDead))
				{
					list.Add(item);
				}
			}
		}
		CasinoManager.SetBetItems(list);
	}
}
