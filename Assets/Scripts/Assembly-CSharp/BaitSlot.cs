using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaitSlot : InventorySlot
{
	[SerializeField]
	private TextMeshProUGUI _baitAmountText;

	[SerializeField]
	private TextMeshProUGUI _changeBaitShortcutText;

	private BaitInfo _bait;

	private void Start()
	{
		GameInfo.Input.onControlsChanged += OnControlsChanged;
		RefreshChangeBaitShortcut();
	}

	private void OnDestroy()
	{
		if ((bool)GameInfo.Input)
		{
			GameInfo.Input.onControlsChanged -= OnControlsChanged;
		}
	}

	private void OnControlsChanged(PlayerInput playerInput)
	{
		RefreshChangeBaitShortcut();
	}

	private void RefreshChangeBaitShortcut()
	{
		InputAction input = GameInfo.Input.actions["ChangeBait"];
		_changeBaitShortcutText.text = SpriteManager.InputToSpriteName(input);
		_changeBaitShortcutText.fontSize = ((GameInfo.Input.currentControlScheme == "Keyboard") ? 24 : 30);
	}

	public void SetBait(BaitInfo bait, int amount)
	{
		if (!(_bait == bait))
		{
			_bait = bait;
			UpdateAmount(amount);
			if ((bool)bait)
			{
				Mesh mesh = bait.Mesh;
				_filter.mesh = mesh;
				_filter.transform.localPosition = bait.InventoryMeshPos;
				_filter.transform.localEulerAngles = bait.InventoryMeshRot;
				_filter.transform.localScale = Vector3.one * bait.InventoryMeshScale;
				_itemImage.transform.localScale = Vector3.zero;
				LeanTween.cancel(_itemImage.gameObject);
				LeanTween.scale(_itemImage.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
			}
			else
			{
				_itemText.text = "";
				_itemText.color = new Color(1f, 1f, 1f, 0.5f);
				LeanTween.cancel(_itemImage.gameObject);
				LeanTween.scale(_itemImage.gameObject, Vector3.zero, 0.15f).setEase(LeanTweenType.easeInBack).setOnComplete(base.RemoveMesh);
			}
		}
	}

	public void UpdateAmount(int to)
	{
		_baitAmountText.gameObject.SetActive(to >= 0);
		_baitAmountText.text = to.ToString();
	}
}
