using FishNet.Managing.Timing;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

public class Radio : Item
{
	[SerializeField]
	private float _radioVol = 0.3f;

	[SerializeField]
	[Tooltip("Channels go between 88 and 108")]
	[Range(88f, 108f)]
	private float _startFrequency = 98f;

	[SerializeField]
	private float _changeFrequencySpeed = 2f;

	[SerializeField]
	private float _frequencySpeedAcc = 3f;

	[SerializeField]
	private AudioSource _noiseSource;

	[SerializeField]
	private RadioChannel[] _channels;

	[SerializeField]
	private Transform _frequencyStick;

	private bool _isLowering;

	private bool _isIncreasing;

	public readonly SyncVar<float> _frequency = new SyncVar<float>();

	private float _localFrequency = 98f;

	private float _prevSentFreqTime;

	private float _heldTime;

	private const float MinTimeToSendFreq = 0.1f;

	private const float MaxStickPos = 0.3f;

	private readonly Vector2 FreqMinMax = new Vector2(88f, 108f);

	private bool _isInitialized;

	private bool NetworkInitialize___EarlyRadioAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateRadioAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Radio_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public void SetStartFrequency(float frequency)
	{
		_frequency.Value = frequency;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		if (_frequency.Value == 0f)
		{
			_frequency.Value = _startFrequency;
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		ToggleChannels(to: true);
		_localFrequency = _frequency.Value;
		ApplyVolume();
		_isInitialized = true;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		BossManager.OnGlobalBossSpawn -= ApplyVolume;
		BossManager.OnGlobalBossDeath -= ApplyVolume;
		BossManager.OnGlobalBossDespawn -= ApplyVolume;
	}

	private void OnEnable()
	{
		if (_isInitialized)
		{
			ToggleChannels(to: true);
		}
	}

	private void OnDisable()
	{
		ToggleChannels(to: false);
	}

	private void ToggleChannels(bool to)
	{
		float time = (float)base.TimeManager.TicksToTime(TickType.Tick);
		RadioChannel[] channels = _channels;
		for (int i = 0; i < channels.Length; i++)
		{
			channels[i].ToggleMute(!to, time);
		}
	}

	public override void OnDrop()
	{
		base.OnDrop();
		_isLowering = false;
		_isIncreasing = false;
	}

	protected override void Update()
	{
		base.Update();
		if (!base.Holder || !base.Holder.Owner.IsLocalClient)
		{
			return;
		}
		if ((!_isLowering && !_isIncreasing) || (_isLowering && _isIncreasing))
		{
			_heldTime = 0f;
			return;
		}
		_heldTime += Time.deltaTime * _frequencySpeedAcc;
		float num = _heldTime * Time.deltaTime;
		_localFrequency += (_isLowering ? ((0f - _changeFrequencySpeed) * num) : (_changeFrequencySpeed * num));
		_localFrequency = Mathf.Clamp(_localFrequency, FreqMinMax.x, FreqMinMax.y);
		if (!Mathf.Approximately(_frequency.Value, _localFrequency))
		{
			ApplyVolume();
			if (!(Time.time - _prevSentFreqTime <= 0.1f))
			{
				_prevSentFreqTime = Time.time;
				Server.Instance.SetRadioFrequency(this, _localFrequency);
			}
		}
	}

	private void ApplyVolume()
	{
		float num = (((bool)base.Holder && base.Holder.Owner.IsLocalClient) ? _localFrequency : _frequency.Value);
		float num2 = 0f;
		RadioChannel[] channels;
		if ((bool)BossManager.Boss)
		{
			channels = _channels;
			for (int i = 0; i < channels.Length; i++)
			{
				channels[i].SetVol(0f);
			}
			_noiseSource.volume = 0f;
			return;
		}
		channels = _channels;
		foreach (RadioChannel radioChannel in channels)
		{
			float num3 = Mathf.Abs(num - radioChannel.Frequency) - 0.5f;
			num3 = Mathf.Clamp01(1f - num3);
			float num4 = 0.05f;
			float num5 = num3 * _radioVol;
			if (radioChannel.IsMuted && num5 > num4)
			{
				float time = (float)base.TimeManager.TicksToTime(TickType.Tick);
				radioChannel.ToggleMute(to: false, time);
			}
			else if (!radioChannel.IsMuted && num5 < num4)
			{
				radioChannel.ToggleMute(to: true, 0f);
			}
			radioChannel.SetVol(num5);
			if (num3 > num2)
			{
				num2 = num3;
			}
		}
		_frequencyStick.localPosition = -Vector3.right * (Mathf.InverseLerp(FreqMinMax.x, FreqMinMax.y, num) * 0.3f);
		_noiseSource.volume = (1f - num2) * 0.075f;
	}

	public override void PrimaryInput(InputAction.CallbackContext context)
	{
		_isLowering = true;
	}

	public override void PrimaryInputCancel(InputAction.CallbackContext context)
	{
		_isLowering = false;
	}

	public override void SecondaryInput(InputAction.CallbackContext context)
	{
		_isIncreasing = true;
	}

	public override void SecondaryInputCanceled(InputAction.CallbackContext context)
	{
		_isIncreasing = false;
	}

	public void SetFrequency(float to)
	{
		_frequency.Value = to;
	}

	private void OnFrequencyChange(float prev, float next, bool asServer)
	{
		if (!asServer && (!base.Holder || !base.Holder.Owner.IsLocalClient))
		{
			ApplyVolume();
			_localFrequency = next;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyRadioAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyRadioAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_frequency.InitializeEarly(this, 9u, isSyncObject: false);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateRadioAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateRadioAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_frequency.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	protected virtual void Awake_UserLogic_Radio_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		base.Radio = this;
		_frequency.OnChange += OnFrequencyChange;
		BossManager.OnGlobalBossSpawn += ApplyVolume;
		BossManager.OnGlobalBossDeath += ApplyVolume;
		BossManager.OnGlobalBossDespawn += ApplyVolume;
	}
}
