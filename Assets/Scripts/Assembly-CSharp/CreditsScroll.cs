using UnityEngine;

public class CreditsScroll : MonoBehaviour
{
	[SerializeField]
	private RectTransform _rect;

	[SerializeField]
	private Transform _startPos;

	[SerializeField]
	private Transform _endPos;

	[SerializeField]
	private float _creditsTime;

	private void OnEnable()
	{
		ScrollCredits();
	}

	private void ScrollCredits()
	{
		if (base.gameObject.activeInHierarchy)
		{
			LeanTween.cancel(_rect);
			_rect.transform.position = _startPos.position;
			LeanTween.move(_rect, _endPos.position + Vector3.up * _rect.sizeDelta.y, _creditsTime);
			_rect.localPosition = _startPos.localPosition;
			Vector2 vector = _endPos.localPosition;
			vector.y += _rect.sizeDelta.y;
			LeanTween.cancel(_rect);
			LeanTween.moveLocal(_rect.gameObject, vector, _creditsTime).setOnComplete(ScrollCredits);
		}
	}
}
