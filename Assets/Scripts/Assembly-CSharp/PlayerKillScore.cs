using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerKillScore : MonoBehaviour
{
	[SerializeField]
	private BonusScoreRow _mainBonusScoreRow;

	[SerializeField]
	private float _delayBetweenBonuses = 0.5f;

	[SerializeField]
	private RectTransform _killScoreHolder;

	[SerializeField]
	private float _animateScoreSpeed = 5f;

	[SerializeField]
	private List<BonusScoreRow> _bonusRows;

	[SerializeField]
	private TextMeshProUGUI _multiplierText;

	private int _bonusIndex;

	private float _curScore;

	private int _baseScore;

	private float _multiplier = 1f;

	private float _targetScore;

	private Coroutine _worthRoutine;

	private Queue<KillScore> _killScoreQueue = new Queue<KillScore>();

	private KillScore _curKillScore;

	private void Awake()
	{
		_mainBonusScoreRow.SetLayoutToRebuild(_killScoreHolder);
		foreach (BonusScoreRow bonusRow in _bonusRows)
		{
			bonusRow.SetLayoutToRebuild(_killScoreHolder);
		}
	}

	public void AddKillScore(string killed, int worth, List<Bonus> bonuses)
	{
		if (killed.Contains("(Clone)"))
		{
			killed = killed.Replace("(Clone)", "");
		}
		_killScoreQueue.Enqueue(new KillScore(killed, worth, bonuses));
		if (_killScoreQueue.Count == 1 && _curKillScore == null)
		{
			StartCoroutine(ShowNextKillScore());
		}
	}

	private IEnumerator ShowNextKillScore()
	{
		_curKillScore = _killScoreQueue.Dequeue();
		_bonusIndex = 0;
		_curScore = 0f;
		_baseScore = _curKillScore.Worth;
		_multiplier = 1f;
		_multiplierText.text = $"{_multiplier:F2}x";
		_mainBonusScoreRow.ScoreText.text = "+0";
		_mainBonusScoreRow.AnimateMainScore(LocalizationManager.KilledLocalized.GetLocalizedString() + " " + _curKillScore.Killed, _curKillScore.Worth);
		yield return new WaitForSeconds(_delayBetweenBonuses * 2f);
		for (int i = 0; i < _curKillScore.Bonuses.Count; i++)
		{
			_bonusRows[_bonusIndex].AnimateBonus(_curKillScore.Bonuses[i]);
			_bonusIndex++;
			if (_bonusIndex >= _bonusRows.Count)
			{
				_bonusIndex = 0;
			}
			yield return new WaitForSeconds(_delayBetweenBonuses);
		}
	}

	public void AddScoreFromBonus(float percent, bool isMainRow)
	{
		if (!isMainRow)
		{
			_multiplier *= percent;
		}
		_targetScore = (int)((float)_baseScore * _multiplier);
		if (_worthRoutine != null)
		{
			StopCoroutine(_worthRoutine);
		}
		_worthRoutine = StartCoroutine(ScoreAnimation());
	}

	private IEnumerator ScoreAnimation()
	{
		_multiplierText.text = $"{_multiplier:F2}x";
		LeanTween.cancel(_multiplierText.gameObject);
		_multiplierText.transform.localScale = Vector3.one * 1.25f;
		LeanTween.scale(_multiplierText.gameObject, Vector3.one, _mainBonusScoreRow.TypedScaleTime);
		while (_curScore < _targetScore)
		{
			_curScore = Mathf.Lerp(_curScore, _targetScore, _animateScoreSpeed * Time.deltaTime);
			if (_targetScore - _curScore < 1f)
			{
				_curScore = _targetScore;
			}
			_mainBonusScoreRow.ScoreText.text = $"+{_curScore:0}";
			yield return new WaitForEndOfFrame();
		}
		_curScore = _targetScore;
		yield return new WaitForSeconds(_mainBonusScoreRow.HideDelay);
		if (_killScoreQueue.Count > 0)
		{
			StartCoroutine(ShowNextKillScore());
			yield break;
		}
		_mainBonusScoreRow.HideAnimation();
		_curKillScore = null;
	}
}
