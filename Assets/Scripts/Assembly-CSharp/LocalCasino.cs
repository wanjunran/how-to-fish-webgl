using System.Collections;
using FishNet;
using TMPro;
using UnityEngine;

public class LocalCasino : MonoBehaviour
{
	public static LocalCasino Instance;

	[Header("UI")]
	[SerializeField]
	private TextMeshProUGUI _curWorthText;

	[SerializeField]
	private float _worthPercentage = 0.1f;

	[SerializeField]
	private float _worthPercentageOnWin = 0.05f;

	[SerializeField]
	private float _scaleMultiplier = 1.1f;

	[SerializeField]
	private float _worthDelay = 0.15f;

	[SerializeField]
	private float _worthDelayOnWin = 0.3f;

	[SerializeField]
	private TextMeshProUGUI _placeBetsHereText;

	[Header("Effects")]
	[SerializeField]
	private Color _wonColor = Color.yellow;

	[SerializeField]
	private Color _blackColor = Color.black;

	[SerializeField]
	private Color _redColor = Color.red;

	[SerializeField]
	private Color _greenColor = Color.green;

	[SerializeField]
	private Transform _effectsPos;

	[Header("Roulette Settings")]
	[SerializeField]
	private Rigidbody _ball;

	[SerializeField]
	private Transform _ballSpawnPoint;

	[SerializeField]
	private Transform _rouletteTableHolder;

	[SerializeField]
	private Transform _wheel;

	[SerializeField]
	private Transform _ballAngleObject;

	[SerializeField]
	private float _wheelSpeed;

	[SerializeField]
	private float _wheelSlowDownSpeed;

	[SerializeField]
	private float _ballForce;

	[SerializeField]
	private Vector3 _ballSpinForce;

	[SerializeField]
	private AudioSource _ballSource;

	[SerializeField]
	private float _ballVol = 0.1f;

	[SerializeField]
	private float _ballVolRetSpeed = 0.1f;

	[SerializeField]
	private float _ballVolAccSpeed = 0.1f;

	private Color _targetColor;

	private BetColor _curColor;

	private Vector3 _serverBallPos;

	private float _serverWheelRot;

	private float _slotSize = 9.72973f;

	private float _curWheelSpeed;

	private float _timeInSameSlot;

	private bool _isSpinning;

	private bool _isPlaying;

	private int _curBetWorth;

	private int _targetBetWorth;

	private Coroutine _worthRoutine;

	private void Awake()
	{
		_ball.solverIterations = 32;
		_ball.isKinematic = true;
		Setter.SetSingleInstance(ref Instance, this);
		if ((bool)Server.Instance)
		{
			OnStartServer();
		}
		Server.OnServerStopped += OnStopServer;
	}

	private void Start()
	{
		if ((bool)CasinoManager.Instance)
		{
			SetTotalWorthText(CasinoManager.Instance.TotalWorth);
		}
	}

	private void OnDestroy()
	{
		Server.OnServerStopped -= OnStopServer;
		_curWheelSpeed = 0f;
		_isSpinning = false;
		if ((bool)InstanceFinder.TimeManager)
		{
			InstanceFinder.TimeManager.OnTick -= TickUpdate;
		}
	}

	private void OnStartServer()
	{
		InstanceFinder.TimeManager.OnTick += TickUpdate;
	}

	private void OnStopServer()
	{
		_curWheelSpeed = 0f;
		_isSpinning = false;
		InstanceFinder.TimeManager.OnTick -= TickUpdate;
	}

	private void TickUpdate()
	{
		if ((bool)Instance && _isSpinning && (bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			Server.Instance.UpdateRoulette(_ball.position, _wheel.eulerAngles.y);
		}
	}

	private void FixedUpdate()
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			if (_isSpinning)
			{
				ServerUpdateRouletteSpinning();
			}
			if (_isPlaying)
			{
				ServerUpdateRouletteGame();
			}
		}
	}

	private void LateUpdate()
	{
		if ((bool)Server.Instance && !Server.Instance.IsServerInitialized)
		{
			ObserverUpdateGame();
		}
	}

	private void ServerUpdateRouletteSpinning()
	{
		_wheel.localEulerAngles += new Vector3(0f, _curWheelSpeed, 0f);
		if (_curWheelSpeed > 0f)
		{
			_curWheelSpeed -= _wheelSlowDownSpeed * Time.fixedDeltaTime;
			return;
		}
		_curWheelSpeed = 0f;
		_isSpinning = false;
	}

	private void ServerUpdateRouletteGame()
	{
		BetColor rouletteColorFromBall = GetRouletteColorFromBall();
		float num = 0f;
		if (rouletteColorFromBall == _curColor)
		{
			_timeInSameSlot += Time.fixedDeltaTime;
		}
		else
		{
			num = _ballVol;
			_timeInSameSlot = 0f;
			_curColor = rouletteColorFromBall;
		}
		_ballSource.volume = Mathf.MoveTowards(_ballSource.volume, num, (_ballSource.volume > num) ? (_ballVolRetSpeed * Time.fixedDeltaTime) : (_ballVolAccSpeed * Time.fixedDeltaTime));
		if (_timeInSameSlot > 2f)
		{
			EndRoulette();
		}
	}

	private void ObserverUpdateGame()
	{
		_ball.isKinematic = true;
		float t = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
		_ball.transform.position = Vector3.Lerp(_ball.transform.position, _serverBallPos, t);
		_wheel.rotation = Quaternion.Slerp(_wheel.rotation, Quaternion.Euler(0f, _serverWheelRot, 0f), t);
		BetColor rouletteColorFromBall = GetRouletteColorFromBall();
		float num = 0f;
		if (rouletteColorFromBall != _curColor)
		{
			num = _ballVol;
			_timeInSameSlot = 0f;
			_curColor = rouletteColorFromBall;
		}
		_ballSource.volume = Mathf.MoveTowards(_ballSource.volume, num, (_ballSource.volume > num) ? (_ballVolRetSpeed * Time.deltaTime) : (_ballVolAccSpeed * Time.deltaTime));
	}

	public void SetTotalWorthText(int worth, bool won = false)
	{
		_targetBetWorth = worth;
		if (_worthRoutine != null)
		{
			StopCoroutine(_worthRoutine);
		}
		_worthRoutine = StartCoroutine(AnimateTotalWorthText(won));
		BetButton.ToggleAll(worth > 0);
	}

	private IEnumerator AnimateTotalWorthText(bool won)
	{
		float percentage = (won ? _worthPercentageOnWin : _worthPercentage);
		_curWorthText.color = (won ? _wonColor : Color.white);
		float delay = (won ? _worthDelayOnWin : _worthDelay);
		if (!won)
		{
			LeanTween.cancel(_curWorthText.gameObject);
			LeanTween.scale(_curWorthText.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutQuad);
		}
		while (_curBetWorth != _targetBetWorth)
		{
			int num = ((_targetBetWorth >= _curBetWorth) ? Mathf.CeilToInt(Mathf.Lerp(_curBetWorth, _targetBetWorth, percentage)) : Mathf.FloorToInt(Mathf.Lerp(_curBetWorth, _targetBetWorth, percentage)));
			int num2 = Mathf.Abs(_targetBetWorth - num);
			_curBetWorth = num;
			_curWorthText.text = $"${_curBetWorth}";
			if (won)
			{
				AudioManager.PlayClipAt("UIHover", _curWorthText.transform.position, variation: true, AudioDistance.VeryShort, 1f, 0.02f);
			}
			yield return new WaitForSeconds(delay / (float)num2);
			if (won)
			{
				LeanTween.cancel(_curWorthText.gameObject);
				_curWorthText.transform.localScale += Vector3.one * _scaleMultiplier;
				LeanTween.scale(_curWorthText.gameObject, Vector3.one, 0.4f).setEase(LeanTweenType.easeOutQuad);
			}
		}
	}

	public void ServerStartRoulette()
	{
		_ball.transform.SetParent(_rouletteTableHolder);
		_ball.isKinematic = false;
		_ball.position = _ballSpawnPoint.position;
		_ball.linearVelocity = _ballSpawnPoint.forward * (_ballForce * Random.Range(0.8f, 1.2f));
		_ball.angularVelocity = _ballSpinForce;
		_curWheelSpeed = _wheelSpeed;
		_timeInSameSlot = 0f;
		_isSpinning = true;
		_isPlaying = true;
	}

	public void OnRouletteStart(BetColor color)
	{
		string text = "";
		string text2 = "";
		switch (color)
		{
		case BetColor.Black:
			text = ColorUtility.ToHtmlStringRGB(_blackColor);
			text2 = LocalizationManager.BlackLocalized.GetLocalizedString();
			break;
		case BetColor.Red:
			text = ColorUtility.ToHtmlStringRGB(_redColor);
			text2 = LocalizationManager.RedLocalized.GetLocalizedString();
			break;
		case BetColor.Green:
			text = ColorUtility.ToHtmlStringRGB(_greenColor);
			text2 = LocalizationManager.GreenLocalized.GetLocalizedString();
			break;
		}
		_placeBetsHereText.text = "<color=#" + text + ">" + text2 + "</color>";
		LeanTween.cancel(_placeBetsHereText.gameObject);
		_placeBetsHereText.transform.localScale = Vector3.zero;
		LeanTween.scale(_placeBetsHereText.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
		_ballSource.volume = _ballVol;
	}

	public void OnRouletteEnd(BetColor color, bool won)
	{
		_placeBetsHereText.text = LocalizationManager.PlaceItemsLocalized.GetLocalizedString();
		LeanTween.cancel(_placeBetsHereText.gameObject);
		_placeBetsHereText.transform.localScale = Vector3.zero;
		LeanTween.scale(_placeBetsHereText.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
		_ballSource.volume = 0f;
		if (won)
		{
			string type = color.ToString() + "Confetti";
			AchievementManager.CheckRouletteAchievement(color);
			AudioManager.PlayClipAt("Confetti", _effectsPos.position);
			AudioManager.PlayClipAt("Win", _effectsPos.position);
			ParticleManager.Play(type, _effectsPos.position, Vector3.up);
		}
		else
		{
			AudioManager.PlayClipAt("Error", base.transform.position, variation: false, AudioDistance.Short, 0.5f);
			WorldText worldText = Object.Instantiate(GameInfo.WorldTextPrefab, _effectsPos.position + Vector3.up * 0.2f, Quaternion.identity);
			worldText.SetText(LocalizationManager.YouLostLocalized.GetLocalizedString() ?? "", 0.2f);
			worldText.FloatAndRemoveAnimation();
		}
	}

	public void ObserverUpdateObjects(Vector3 ballPos, float wheelRot)
	{
		_serverBallPos = ballPos;
		_serverWheelRot = wheelRot;
	}

	private void EndRoulette()
	{
		_ball.transform.SetParent(_wheel);
		_ball.isKinematic = true;
		CasinoManager.Instance.ServerRouletteResult(GetRouletteColorFromBall());
		_isPlaying = false;
	}

	private BetColor GetRouletteColorFromBall()
	{
		_ballAngleObject.transform.LookAt(_wheel);
		float y = _ballAngleObject.transform.eulerAngles.y;
		float y2 = _wheel.transform.eulerAngles.y;
		int num = Mathf.FloorToInt((y - y2 + 360f) % 360f / _slotSize);
		if (num == 0)
		{
			return BetColor.Green;
		}
		if (num % 2 > 0)
		{
			return BetColor.Black;
		}
		return BetColor.Red;
	}
}
