using UnityEngine;

public class SellBox : MonoBehaviour
{
	private void OnTriggerStay(Collider other)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			Item item = ItemManager.Get(other);
			if ((bool)item && !item.Holder && !item.IsDeinitializing && !item.IsDestroying && (!item.Creature || item.Creature.IsDead) && !item.DeadPlayer)
			{
				SellItem(item);
			}
		}
	}

	private void SellItem(Item item)
	{
		MoneyManager.SellItem(item);
	}
}
