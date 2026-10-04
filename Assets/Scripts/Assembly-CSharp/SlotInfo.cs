using UnityEngine;

[CreateAssetMenu(fileName = "New Pocket Info", menuName = "How to Fish/Pocket Preset")]
public class SlotInfo : ScriptableObject
{
	[SerializeField]
	private string _name;

	[SerializeField]
	private int _cost;

	[SerializeField]
	private byte _index;

	public string Name => _name;

	public int Cost => _cost;

	public byte Index => _index;
}
