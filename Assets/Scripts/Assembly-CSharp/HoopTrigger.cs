using UnityEngine;

public class HoopTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider col)
	{
		Item item = ItemManager.Get(col);
		if ((bool)item && item.name.Contains("Shrimp") && !item.Creature.IsDead)
		{
			VFXManager.Play("LavaBarf", base.transform.position, Vector3.up);
		}
	}
}
