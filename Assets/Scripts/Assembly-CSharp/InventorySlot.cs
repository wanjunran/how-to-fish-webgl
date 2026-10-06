using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
	[SerializeField]
	private RectTransform _slot;

	[SerializeField]
	protected TextMeshProUGUI _itemText;

	[SerializeField]
	protected MeshFilter _filter;

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private Image _outline;

	[SerializeField]
	protected RawImage _itemImage;

	public Item Item { get; private set; }

	public void Initialize(bool unlocked)
	{
		base.gameObject.SetActive(unlocked);
	}

	public void SetItem(Item item)
	{
		if (Item == item)
		{
			return;
		}
		Item = item;
		if ((bool)item)
		{
			Mesh mesh = item.Mesh;
			if (!mesh)
			{
				_itemText.text = item.name;
				_itemText.color = Color.white;
				return;
			}
			_filter.mesh = mesh;
			_filter.transform.localPosition = item.InventoryMeshPos;
			_filter.transform.localEulerAngles = item.InventoryMeshRot;
			_filter.transform.localScale = Vector3.one * item.InventoryMeshScale;
			_itemImage.transform.localScale = Vector3.zero;
			ApplyCookness(item);
			ApplySkin(item);
			LeanTween.cancel(_itemImage.gameObject);
			LeanTween.scale(_itemImage.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
		}
		else
		{
			_itemText.text = "";
			_itemText.color = new Color(1f, 1f, 1f, 0.5f);
			LeanTween.cancel(_itemImage.gameObject);
			LeanTween.scale(_itemImage.gameObject, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInBack).setOnComplete(RemoveMesh);
		}
	}

	public void ApplySkin(Item item)
	{
		if ((bool)item)
		{
			if (!item.SkinPreset)
			{
				ShaderManager.ResetItemSkin(_renderer);
			}
			else
			{
				ShaderManager.ApplyItemSkin(item.SkinPreset.Skins[item.CurSkin], _renderer);
			}
		}
	}

	public void ApplyCookness(Item item)
	{
		if ((bool)item)
		{
			ShaderManager.SetItemCookness(_renderer, item.Cookness);
		}
	}

	protected void RemoveMesh()
	{
		_filter.mesh = null;
	}

	public void Select()
	{
		_outline.gameObject.SetActive(value: true);
		LeanTween.cancel(_slot);
		LeanTween.moveLocal(_slot.gameObject, Vector3.up * 20f, 0.25f).setEase(LeanTweenType.easeOutBack);
	}

	public void Deselect()
	{
		_outline.gameObject.SetActive(value: false);
		LeanTween.cancel(_slot);
		LeanTween.moveLocal(_slot.gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeOutQuad);
	}
}
