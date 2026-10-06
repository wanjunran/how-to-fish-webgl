using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _bossCanvasLerped;

	[SerializeField]
	private TextMeshProUGUI _bossNameText;

	[SerializeField]
	private RectTransform _bossHpToMove;

	[SerializeField]
	private RectTransform _bossHpToRotate;

	[SerializeField]
	private Image _bossHealth;

	[SerializeField]
	private Image _bossHealthLerped;

	[SerializeField]
	private float _bossShakeForce = 750f;

	[SerializeField]
	private float _bossLerpForce = 1200f;

	[SerializeField]
	private float _bossHpDamping = 12f;

	[Space]
	[SerializeField]
	private float _bossShakeAngForce = 10f;

	[SerializeField]
	private float _bossAngLerpForce = 120f;

	[SerializeField]
	private float _bossHpAngDamping = 20f;

	[Header("Time")]
	[SerializeField]
	private Image _timeLeftImage;

	[SerializeField]
	private CanvasGroup _countdownGroup;

	[SerializeField]
	private TextMeshProUGUI _timeCountdownText;

	private Vector3 _bossHpStartPos;

	private Vector3 _bossHpVel;

	private Vector3 _bossHpAngVel;

	private bool _isShowingCountdown;

	private void Awake()
	{
		_bossCanvasLerped.alpha = (BossManager.Boss ? 1 : 0);
		_countdownGroup.alpha = 0f;
		ToggleBossUI(BossManager.Boss);
		_bossHpStartPos = _bossHpToMove.localPosition;
	}

	private void LateUpdate()
	{
		if ((bool)BossManager.Boss && PlayerUI.BossCanvasActive && _bossCanvasLerped.alpha != 0f)
		{
			UpdateBossHpPosition();
			UpdateBossTimer();
		}
	}

	private void UpdateBossHpPosition()
	{
		Vector3 vector = _bossHpStartPos - _bossHpToMove.localPosition;
		vector *= _bossLerpForce;
		_bossHpVel += vector * Time.deltaTime;
		_bossHpVel -= _bossHpVel * (_bossHpDamping * Time.deltaTime);
		_bossHpToMove.localPosition += _bossHpVel * Time.deltaTime;
		Quaternion currentRot = _bossHpToRotate.localRotation;
		DazedUtils.SimulateSpringRotation(ref currentRot, ref _bossHpAngVel, Quaternion.identity, _bossAngLerpForce, _bossHpAngDamping);
		_bossHpToRotate.localRotation = currentRot;
	}

	private void UpdateBossTimer()
	{
		float num = Mathf.InverseLerp(BossManager.BossLeavesTick, BossManager.BossSpawnTick, InstanceFinder.TimeManager.Tick);
		float num2 = Mathf.Lerp(0f, BossManager.BossTotalTimeInTicks, num) / (float)(int)InstanceFinder.TimeManager.TickRate;
		_timeLeftImage.fillAmount = num;
		if (num2 >= 10f)
		{
			_countdownGroup.alpha = 0f;
			return;
		}
		if (!_isShowingCountdown)
		{
			_isShowingCountdown = true;
			LeanTween.cancel(_countdownGroup.gameObject);
			LeanTween.value(_countdownGroup.gameObject, 0f, 1f, 1f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateCountdownAlpha);
		}
		_countdownGroup.gameObject.SetActive(num2 < 10f);
		_timeCountdownText.text = num2.ToString("#0");
	}

	private void UpdateCountdownAlpha(float to)
	{
		_countdownGroup.alpha = to;
	}

	public void UpdateBossHp(int curHp)
	{
		float num = (float)curHp / (float)BossManager.BossMaxHp;
		if (num < _bossHealth.fillAmount)
		{
			float num2 = Mathf.Clamp(_bossHealth.fillAmount - num, 0.025f, 1f);
			Vector3 vector = Random.insideUnitSphere.normalized * num2;
			vector *= (float)((Random.Range(0, 2) == 1) ? 1 : (-1));
			_bossHpVel = vector * _bossShakeForce;
			float num3 = ((Random.Range(0, 2) == 1) ? 1 : (-1));
			_bossHpAngVel += new Vector3(0f, 0f, num3 * num2 * _bossShakeAngForce);
		}
		else
		{
			_bossHpVel = Vector3.zero;
			_bossHpAngVel = Vector3.zero;
			_bossHpToMove.localPosition = _bossHpStartPos;
			_bossHpToRotate.rotation = Quaternion.identity;
		}
		_bossHealth.fillAmount = num;
		LeanTween.cancel(_bossHealthLerped.gameObject);
		LeanTween.value(_bossHealthLerped.gameObject, 0f, 1f, 0.25f).setOnComplete(LerpBossHp);
	}

	private void LerpBossHp()
	{
		LeanTween.cancel(_bossHealthLerped.gameObject);
		LeanTween.value(_bossHealthLerped.gameObject, _bossHealthLerped.fillAmount, _bossHealth.fillAmount, 0.5f).setEase(LeanTweenType.easeOutQuart).setOnUpdate(UpdateBossLerpHp);
	}

	private void UpdateBossLerpHp(float to)
	{
		_bossHealthLerped.fillAmount = to;
	}

	public void ToggleBossUI(bool to)
	{
		if (to)
		{
			_bossNameText.text = ((BossManager.Boss.GetName() == "") ? "Boss" : BossManager.Boss.GetName());
			_bossHealth.color = ((BossManager.Boss.BossType == BossType.Boss) ? GameInfo.RedColor : GameInfo.MiniBossColor);
		}
		LeanTween.cancel(_countdownGroup.gameObject);
		_countdownGroup.alpha = 0f;
		_isShowingCountdown = false;
		float to2 = (to ? 1 : 0);
		LeanTween.cancel(_bossCanvasLerped.gameObject);
		LeanTween.value(_bossCanvasLerped.gameObject, _bossCanvasLerped.alpha, to2, 1f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(LerpBossCanvasAlpha);
	}

	private void LerpBossCanvasAlpha(float to)
	{
		_bossCanvasLerped.alpha = to;
	}
}
