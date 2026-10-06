using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Bait Info", menuName = "How to Fish/Bait Info")]
public class BaitInfo : ScriptableObject
{
	[SerializeField]
	private LocalizedString _nameLocalized;

	[SerializeField]
	private LocalizedString _descriptionLocalized;

	[SerializeField]
	private string _name;

	[SerializeField]
	private string _description;

	[SerializeField]
	private Mesh _mesh;

	[SerializeField]
	private Mesh _meshForNpc;

	[SerializeField]
	private int _cost;

	[SerializeField]
	[Range(0f, 100f)]
	private float _lostOnBaitChance;

	[SerializeField]
	private Vector2 _catchTimeMinMax;

	[SerializeField]
	private bool _requireReelingToCatch = true;

	[SerializeField]
	private Vector3 _hookPoint;

	[Header("Inventory Info")]
	[SerializeField]
	private Vector3 inventoryMeshPos = Vector3.zero;

	[SerializeField]
	private Vector3 _inventoryRot = new Vector3(-36f, 40f, -140f);

	[FormerlySerializedAs("_inventoryScale")]
	[SerializeField]
	private float inventoryMeshScale = 2f;

	[Header("Items and weights")]
	[SerializeField]
	private List<ItemInfoWeight> _itemWeights;

	public string NameLocalized => _nameLocalized.GetLocalizedString();

	public string Description => _descriptionLocalized.GetLocalizedString();

	public Mesh Mesh => _mesh;

	public Mesh MeshForNpc
	{
		get
		{
			if (!_meshForNpc)
			{
				return _mesh;
			}
			return _meshForNpc;
		}
	}

	public int Cost => _cost;

	public Vector3 InventoryMeshPos => inventoryMeshPos;

	public Vector3 InventoryMeshRot => _inventoryRot;

	public float InventoryMeshScale => inventoryMeshScale;

	public List<ItemInfoWeight> ItemWeights => _itemWeights;

	public bool RequireReelingToCatch => _requireReelingToCatch;

	public float LostOnBaitChance => _lostOnBaitChance;

	public Vector2 CatchTimeMinMax => _catchTimeMinMax;

	public Vector3 HookPoint => _hookPoint;
}
