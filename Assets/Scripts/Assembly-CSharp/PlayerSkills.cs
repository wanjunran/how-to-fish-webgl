using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
	private static PlayerSkills _instance;

	private const float _rotWindow = 0.5f;

	private const float _requiredRotation = 320f;

	private const float _timeFor360ToNotCount = 1f;

	private float _accumulatedYaw;

	private float _lastYaw;

	private bool _hasLastYaw;

	private float _timeSinceLastTurn;

	private float _spinDir;

	private static float _timeRotatedFullCircle = float.NegativeInfinity;

	private static float _timeOfAds;

	private static bool _isAiming;

	private static float _timeOfStopAds;

	private const float MinTimeForNoScope = 0.25f;

	private const float MaxTimeForQuickScope = 0.5f;

	private static float _lastKillTime;

	private static int _killCounter;

	private const float MaxTimeForMultikill = 3f;

	public const float LongshotDistance = 25f;

	public const float PointBlankDistance = 2f;

	public const int MinScoresForImpressiveKill = 5;

	public const int OverKillHp = -70;

	private static float _shotsFiredSinceKill;

	private const float MinShotsForFinally = 5f;

	private static float _lastShotFiredTime;

	public static bool RecentlyDid360 => Time.time - _timeRotatedFullCircle <= 1f;

	public static bool NoScope
	{
		get
		{
			if (!_isAiming)
			{
				return Time.time - _timeOfStopAds >= 0.25f;
			}
			return false;
		}
	}

	public static bool QuickScope
	{
		get
		{
			if (_isAiming)
			{
				return Time.time - _timeOfAds <= 0.5f;
			}
			return false;
		}
	}

	public static int Multikill
	{
		get
		{
			if (!(Time.time - _lastKillTime <= 3f))
			{
				return 0;
			}
			return _killCounter;
		}
	}

	public static bool Finally => _shotsFiredSinceKill > 5f;

	public void InitializeLocal()
	{
		Setter.SetSingleInstance(ref _instance, this);
		_timeRotatedFullCircle = float.NegativeInfinity;
		_hasLastYaw = false;
		Reset360();
	}

	public static void OnKill(int toAdd = 1)
	{
		if (Time.time - _lastKillTime > 3f)
		{
			_killCounter = toAdd;
		}
		else
		{
			_killCounter += toAdd;
		}
		_lastKillTime = Time.time;
		_shotsFiredSinceKill = 0f;
	}

	public static void OnADSChange(bool to)
	{
		_isAiming = to;
		if (to)
		{
			_timeOfAds = Time.time;
		}
		else
		{
			_timeOfStopAds = Time.time;
		}
	}

	public static void OnAttack(float timeBetweenShots)
	{
		if (Time.time - _lastShotFiredTime > 30f)
		{
			_shotsFiredSinceKill = 0f;
		}
		_lastShotFiredTime = Time.time;
		_shotsFiredSinceKill += timeBetweenShots;
	}

	private void Update()
	{
		Check360();
	}

	private void Check360()
	{
		if (!Player.LocalPlayer || !Player.LocalPlayer.CamObject)
		{
			_hasLastYaw = false;
			Reset360();
			return;
		}
		float y = Player.LocalPlayer.CamObject.eulerAngles.y;
		if (!_hasLastYaw)
		{
			_lastYaw = y;
			_hasLastYaw = true;
			return;
		}
		float num = Mathf.DeltaAngle(_lastYaw, y);
		_lastYaw = y;
		if (Mathf.Abs(num) > 0.01f)
		{
			if (_spinDir == 0f)
			{
				_spinDir = Mathf.Sign(num);
			}
			_accumulatedYaw += num;
			_timeSinceLastTurn = 0f;
		}
		else
		{
			_timeSinceLastTurn += Time.deltaTime;
		}
		if (_timeSinceLastTurn > 0.5f)
		{
			Reset360();
		}
		if (Mathf.Abs(_accumulatedYaw) >= 320f)
		{
			_timeRotatedFullCircle = Time.time;
			Reset360();
		}
	}

	private void Reset360()
	{
		_spinDir = 0f;
		_accumulatedYaw = 0f;
	}
}
