using UnityEngine;

public class IslandUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _warningGroup;

	public void _islandUIToggleIslandWarning(bool to)
	{
		LeanTween.cancel(_warningGroup.gameObject);
		LeanTween.value(_warningGroup.gameObject, _warningGroup.alpha, to ? 1 : 0, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateWarningAlpha);
	}

	private void UpdateWarningAlpha(float to)
	{
		_warningGroup.alpha = to;
	}
}
