using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MapDot
{
	[SerializeField]
	private Image _dot;

	private float _pingTime;

	private Vector2 _realtimeWorldPos;

	private Vector2 _pingedPos;

	public bool RecentlyPinged => Time.time - _pingTime < 0.5f;

	public Vector2 RealtimeLocalPos { get; private set; }

	public void Hide()
	{
		_dot.color = new Color(_dot.color.r, _dot.color.g, _dot.color.b, 0f);
	}

	public void TriggerPing()
	{
		_pingTime = Time.time;
		_dot.color = new Color(_dot.color.r, _dot.color.g, _dot.color.b, 1f);
		_pingedPos = _realtimeWorldPos;
		_dot.rectTransform.localPosition = RealtimeLocalPos;
		LeanTween.cancel(_dot.gameObject);
		LeanTween.value(_dot.gameObject, 1f, 0f, 3f).setOnUpdate(UpdateAlpha);
	}

	private void UpdateAlpha(float to)
	{
		_dot.color = new Color(_dot.color.r, _dot.color.g, _dot.color.b, to);
	}

	public void Move(Vector2 realtimeWorldPos, Vector2 offset, float mapScale, float zoomMultiplier, float maxPosDist)
	{
		_realtimeWorldPos = realtimeWorldPos;
		RealtimeLocalPos = (realtimeWorldPos - offset) * (mapScale * zoomMultiplier);
		Vector2 vector = (_pingedPos - offset) * (mapScale * zoomMultiplier);
		if (RealtimeLocalPos.sqrMagnitude > maxPosDist * maxPosDist)
		{
			RealtimeLocalPos = RealtimeLocalPos.normalized * maxPosDist;
		}
		if (vector.sqrMagnitude > maxPosDist * maxPosDist)
		{
			vector = vector.normalized * maxPosDist;
		}
		_dot.rectTransform.localPosition = vector;
	}
}
