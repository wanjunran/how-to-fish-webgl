using UnityEngine;

public class SwapOutfitButtons : MonoBehaviour
{
	[SerializeField]
	private RectTransform _hatRect;

	[SerializeField]
	private RectTransform _accessoryRect;

	[SerializeField]
	private RectTransform _outfitRect;

	private void Update()
	{
		_hatRect.position = MainMenuManager.MenuCam.WorldToScreenPoint(LocalSkin.HatTarget.position);
		_accessoryRect.position = MainMenuManager.MenuCam.WorldToScreenPoint(LocalSkin.AccessoryTarget.position);
		_outfitRect.position = MainMenuManager.MenuCam.WorldToScreenPoint(LocalSkin.OutfitTarget.position);
	}
}
