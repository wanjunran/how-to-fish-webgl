using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineSlot : MonoBehaviour
{
	[SerializeField]
	private RectTransform _selfRect;

	[SerializeField]
	private RectTransform _slot;

	[SerializeField]
	protected TextMeshProUGUI _itemText;

	[SerializeField]
	protected MeshFilter _filter;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	protected RawImage _itemImage;

	[SerializeField]
	private Image _outline;

	private Item _item;

	private byte _skinIndex;

	private bool _useBoat;

	public RectTransform Rect => _selfRect;

	public RectTransform ItemImage => _itemImage.rectTransform;

	public void MoveTo(float x)
	{
		base.transform.localPosition = Vector3.right * x;
	}

	public void SetIndex(byte itemIndex, byte skinIndex)
	{
		_useBoat = itemIndex == byte.MaxValue;
		ItemSkin skin;
		Mesh mesh;
		if (_useBoat)
		{
			skin = BoatManager.Boat.SkinPreset.Skins[skinIndex];
			mesh = BoatManager.Boat.Mesh;
			_filter.transform.localPosition = BoatManager.Boat.SkinMeshPos;
			_filter.transform.localEulerAngles = BoatManager.Boat.SkinMeshRot;
			_filter.transform.localScale = Vector3.one * BoatManager.Boat.SkinMeshScale;
		}
		else
		{
			_item = GameInfo.IDToItem(itemIndex);
			skin = _item.SkinPreset.Skins[skinIndex];
			mesh = _item.Mesh;
			_filter.transform.localPosition = _item.InventoryMeshPos;
			_filter.transform.localEulerAngles = _item.InventoryMeshRot;
			_filter.transform.localScale = Vector3.one * _item.InventoryMeshScale;
		}
		switch (skin.Rarity)
		{
		case Rarity.Rare:
			_outline.color = GameInfo.RareColor;
			break;
		case Rarity.Legendary:
			_outline.color = GameInfo.LegendaryColor;
			break;
		default:
			_outline.color = GameInfo.CommonColor;
			break;
		}
		if ((bool)mesh)
		{
			_filter.mesh = mesh;
			ApplySkin(skin);
		}
		else
		{
			_itemText.text = "";
			_itemText.color = new Color(1f, 1f, 1f, 0.5f);
			RemoveMesh();
		}
	}

	private void ApplySkin(ItemSkin skin)
	{
		ShaderManager.ApplyItemSkin(skin, _renderer);
	}

	private void RemoveMesh()
	{
		_filter.mesh = null;
	}
}
