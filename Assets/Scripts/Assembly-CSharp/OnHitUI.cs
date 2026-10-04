using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnHitUI : MonoBehaviour
{
	[SerializeField]
	private Image _hitMarkerPrefab;

	[SerializeField]
	private int _totalHitMarkers = 10;

	[SerializeField]
	private int _totalDamageTexts = 20;

	[SerializeField]
	private float _damageTextRot = 25f;

	[SerializeField]
	private float _damageTextTime = 0.5f;

	[SerializeField]
	private Vector2 _damageTextUpPos;

	[SerializeField]
	private float _hitMarkerTime = 0.25f;

	private List<TextMeshProUGUI> _damageTexts = new List<TextMeshProUGUI>();

	private List<Vector3> _damageTextPositions = new List<Vector3>();

	private List<float> _damageUps = new List<float>();

	private int _curDamageText;

	private List<Image> _hitMarkers = new List<Image>();

	private List<Vector3> _hitMarkerPositions = new List<Vector3>();

	private int _curHitMarker;

	public void InitializeLocal()
	{
		for (int i = 0; i < _totalHitMarkers; i++)
		{
			Image image = Object.Instantiate(_hitMarkerPrefab, PlayerUI.FXCanvasTrans);
			image.transform.localScale = Vector3.zero;
			_hitMarkers.Add(image);
			_hitMarkerPositions.Add(Vector3.zero);
		}
		for (int j = 0; j < _totalDamageTexts; j++)
		{
			TextMeshProUGUI textMeshProUGUI = Object.Instantiate(PlayerUI.CanvasTextPrefab, PlayerUI.FXCanvasTrans);
			textMeshProUGUI.transform.localScale = Vector3.zero;
			textMeshProUGUI.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f - _damageTextRot, _damageTextRot));
			_damageTexts.Add(textMeshProUGUI);
			_damageTextPositions.Add(Vector3.zero);
			_damageUps.Add(0f);
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _hitMarkers.Count; i++)
		{
			if (_hitMarkers[i].transform.localScale.x > 0.01f)
			{
				if (GameInfo.CurCamera.WorldToScreenPoint(_hitMarkerPositions[i]).z < 0f)
				{
					LeanTween.cancel(_hitMarkers[i].gameObject);
					_hitMarkers[i].transform.localScale = Vector3.zero;
				}
				else
				{
					_hitMarkers[i].transform.position = GameInfo.CurCamera.WorldToScreenPoint(_hitMarkerPositions[i]);
				}
			}
		}
		for (int j = 0; j < _damageTexts.Count; j++)
		{
			if (_damageTexts[j].transform.localScale.x > 0.01f)
			{
				if (GameInfo.CurCamera.WorldToScreenPoint(_damageTextPositions[j]).z < 0f)
				{
					LeanTween.cancel(_damageTexts[j].gameObject);
					_damageTexts[j].transform.localScale = Vector3.zero;
				}
				else
				{
					_damageTexts[j].transform.position = GameInfo.CurCamera.WorldToScreenPoint(_damageTextPositions[j]) + _damageTexts[j].transform.up * _damageUps[j];
				}
			}
		}
	}

	public void AddHitMarker(Vector3 pos, bool killed)
	{
		_curHitMarker++;
		if (_curHitMarker >= _hitMarkers.Count)
		{
			_curHitMarker = 0;
		}
		int curHitMarker = _curHitMarker;
		Image image = _hitMarkers[curHitMarker];
		_hitMarkerPositions[curHitMarker] = pos;
		image.color = (killed ? GameInfo.RedColor : Color.white);
		image.transform.position = GameInfo.CurCamera.WorldToScreenPoint(pos);
		image.transform.localScale = Vector3.zero;
		LeanTween.cancel(image.gameObject);
		LeanTween.scale(image.gameObject, Vector3.one, _hitMarkerTime).setEase(LeanTweenType.easeOutBack).setOnComplete(HideHitMarker, curHitMarker);
	}

	private void HideHitMarker(object indexObj)
	{
		int index = (int)indexObj;
		Image image = _hitMarkers[index];
		if ((bool)image && !LeanTween.isTweening(image.gameObject))
		{
			LeanTween.scale(image.gameObject, Vector3.zero, _hitMarkerTime).setEase(LeanTweenType.easeInQuad);
		}
	}

	public void AddDamageNumber(Vector3 pos, int damage, bool killed)
	{
		if (DecalManager.UseDamageNumbers)
		{
			_curDamageText++;
			if (_curDamageText >= _damageTexts.Count)
			{
				_curDamageText = 0;
			}
			int curDamageText = _curDamageText;
			TextMeshProUGUI textMeshProUGUI = _damageTexts[curDamageText];
			textMeshProUGUI.text = $"{damage}";
			_damageTextPositions[curDamageText] = pos;
			Vector3 vector = GameInfo.CurCamera.WorldToScreenPoint(pos);
			textMeshProUGUI.color = ((vector.y < 0f) ? new Color(1f, 1f, 1f, 0f) : (killed ? GameInfo.RedColor : Color.white));
			textMeshProUGUI.transform.position = vector + textMeshProUGUI.transform.up * _damageTextUpPos.x;
			textMeshProUGUI.transform.localScale = Vector3.zero;
			LeanTween.cancel(textMeshProUGUI.gameObject);
			LeanTween.scale(textMeshProUGUI.gameObject, Vector3.one, _damageTextTime).setEase(LeanTweenType.easeOutBack);
			LeanTween.value(textMeshProUGUI.gameObject, _damageTextUpPos.x, _damageTextUpPos.y, _damageTextTime).setEase(LeanTweenType.easeOutBack).setOnUpdate(UpdateDamageUpPos, curDamageText)
				.setOnComplete(HideDamageText, curDamageText);
		}
	}

	private void HideDamageText(object indexObj)
	{
		int num = (int)indexObj;
		TextMeshProUGUI textMeshProUGUI = _damageTexts[num];
		if ((bool)textMeshProUGUI && !LeanTween.isTweening(textMeshProUGUI.gameObject))
		{
			LeanTween.scale(textMeshProUGUI.gameObject, Vector3.zero, _damageTextTime).setEase(LeanTweenType.easeInQuad);
			LeanTween.value(textMeshProUGUI.gameObject, _damageTextUpPos.y, _damageTextUpPos.y, _damageTextTime).setEase(LeanTweenType.easeInQuad).setOnUpdate(UpdateDamageUpPos, num);
		}
	}

	private void UpdateDamageUpPos(float up, object indexObj)
	{
		int index = (int)indexObj;
		_damageUps[index] = up;
	}
}
