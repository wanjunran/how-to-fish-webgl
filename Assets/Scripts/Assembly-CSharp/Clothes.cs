using UnityEngine;

[CreateAssetMenu(fileName = "New Clothes", menuName = "How to Fish/Clothes")]
public class Clothes : ScriptableObject
{
	[SerializeField]
	private bool _defaultUnlocked;

	[SerializeField]
	private Mesh _hatMesh;

	[SerializeField]
	private Mesh _accessoryMesh;

	[SerializeField]
	private Mesh outfitMesh;

	public bool DefaultUnlocked => _defaultUnlocked;

	public Mesh HatMesh => _hatMesh;

	public Mesh AccessoryMesh => _accessoryMesh;

	public Mesh OutfitMesh => outfitMesh;
}
