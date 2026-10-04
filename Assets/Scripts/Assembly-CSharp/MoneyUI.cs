using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyUI : MonoBehaviour
{
	[SerializeField]
	private RectTransform _moneyTextHolder;

	[SerializeField]
	private TextMeshProUGUI _moneyText;

	[SerializeField]
	private int _totalMoneyTexts = 10;

	[SerializeField]
	private float _moneyTextMoveTime = 0.25f;

	[SerializeField]
	private float _moneyTextLifeTime = 3f;

	[SerializeField]
	private float _moneyTextDefaultOffset;

	[SerializeField]
	private float _moneyTextRandomOffset = 25f;

	[SerializeField]
	private float _moneyTextUpPos = 0.5f;

	[SerializeField]
	private Vector2 _moneyTextRotMinMax = new Vector2(5f, 45f);

	private List<TextMeshProUGUI> _moneyTexts = new List<TextMeshProUGUI>();

	private int _curMoneyText;

	private int _prevMoney;

	private int _curMoney;

	private int _targetMoney;

	public void InitializeLocal()
	{
		SetMoney(MoneyManager.Money, MoneyManager.Money, gainedMoney: true);
		for (int i = 0; i < _totalMoneyTexts; i++)
		{
			TextMeshProUGUI textMeshProUGUI = Object.Instantiate(PlayerUI.CanvasTextPrefab, PlayerUI.FXCanvasTrans);
			textMeshProUGUI.transform.localScale = Vector3.zero;
			textMeshProUGUI.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - Random.Range(_moneyTextRotMinMax.x, _moneyTextRotMinMax.y));
			textMeshProUGUI.fontSize = 32f;
			_moneyTexts.Add(textMeshProUGUI);
		}
	}

	public void SetMoney(int to, int diff, bool gainedMoney)
	{
		_prevMoney = _curMoney;
		_targetMoney = to;
		LeanTween.cancel(_moneyTextHolder);
		LeanTween.value(_moneyTextHolder.gameObject, 0f, 1f, 1f).setEase(LeanTweenType.easeOutQuart).setOnUpdate(UpdateVisualMoney);
		if (_moneyTexts.Count != 0 && !Player.LocalPlayer.Dying.IsDead)
		{
			TextMeshProUGUI text = _moneyTexts[_curMoneyText];
			float moneyTextRandomOffset = _moneyTextRandomOffset;
			Vector3 vector = new Vector3(Random.Range(0f - moneyTextRandomOffset, moneyTextRandomOffset), Random.Range(0f - moneyTextRandomOffset, moneyTextRandomOffset), 0f);
			Vector3 vector2 = _moneyTextHolder.localPosition + Vector3.right * _moneyTextHolder.sizeDelta.x / 2f + Vector3.up * _moneyTextDefaultOffset + vector;
			text.transform.localPosition = vector2;
			string text2 = $"+${diff}";
			if (!gainedMoney)
			{
				text2 = $"-${diff}";
			}
			text.text = text2;
			text.color = (gainedMoney ? GameInfo.GreenColor : GameInfo.RedColor);
			text.transform.localScale = Vector3.one;
			LeanTween.cancel(text.gameObject);
			LeanTween.moveLocal(text.gameObject, vector2 + text.transform.up * _moneyTextUpPos, _moneyTextMoveTime).setEase(LeanTweenType.easeOutBack);
			LeanTween.value(text.gameObject, 0f, 1f, _moneyTextLifeTime).setOnComplete(() =>
			{
				RemoveMoneyText(text);
			});
			_curMoneyText++;
			if (_curMoneyText >= _moneyTexts.Count)
			{
				_curMoneyText = 0;
			}
		}
	}

	private void UpdateVisualMoney(float t)
	{
		_curMoney = (int)Mathf.LerpUnclamped(_prevMoney, _targetMoney, t);
		_moneyText.text = $"${_curMoney}";
		LayoutRebuilder.ForceRebuildLayoutImmediate(_moneyText.rectTransform);
		Vector2 sizeDelta = _moneyTextHolder.sizeDelta;
		sizeDelta.x = _moneyText.rectTransform.sizeDelta.x + 10f;
		_moneyTextHolder.sizeDelta = sizeDelta;
	}

	private void RemoveMoneyText(TextMeshProUGUI text)
	{
		LeanTween.cancel(text.gameObject);
		LeanTween.scale(text.gameObject, Vector3.zero, _moneyTextMoveTime).setEase(LeanTweenType.easeInBack);
	}
}
