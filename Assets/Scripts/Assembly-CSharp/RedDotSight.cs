using UnityEngine;
using UnityEngine.UI;

public class RedDotSight : Sight
{
	[SerializeField]
	private RectTransform _redDotCanvas;

	[SerializeField]
	private Image _redDot;

	[SerializeField]
	private float _zeroDistance = 5f;

	private const float _lerpSpeed = 16f;

	private void LateUpdate()
	{
		SetRedDotPos();
	}

	private void SetRedDotPos()
	{
		if ((bool)GameInfo.CurCamera)
		{
			Vector3 position = base.transform.position + base.transform.up * _zeroDistance;
			Vector3 vector = GameInfo.CurCamera.WorldToScreenPoint(position);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_redDotCanvas, vector, GameInfo.CurCamera, out var localPoint);
			_redDot.rectTransform.localPosition = Vector3.Lerp(_redDot.rectTransform.localPosition, localPoint, 16f * Time.deltaTime);
		}
	}
}
