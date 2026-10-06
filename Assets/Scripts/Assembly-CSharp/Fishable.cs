using UnityEngine;

[CreateAssetMenu(fileName = "New Item Info", menuName = "How to Fish/Item Info")]
public class Fishable : ScriptableObject
{
	[SerializeField]
	private Item _itemToSpawn;

	public Item ItemToSpawn => _itemToSpawn;
}
