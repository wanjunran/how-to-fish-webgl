using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public sealed class AudioManager : MonoBehaviour
{
	private static AudioManager _instance;

	[SerializeField]
	private AudioSource _audioSourcePrefab;

	[SerializeField]
	private int _globalVariedSourcesAmount = 6;

	[SerializeField]
	private int _worldSourcesAmount = 50;

	[SerializeField]
	private int _variedWorldSourcesAmount = 10;

	[SerializeField]
	[Range(0f, 1f)]
	private float _volume = 0.5f;

	[Header("Source range settings(Default = Medium)")]
	[SerializeField]
	private Vector2 _veryShortMinMax = new Vector2(3f, 10f);

	[SerializeField]
	private byte _veryShortPriority = 220;

	[Space]
	[SerializeField]
	private Vector2 _shortMinMax = new Vector2(5f, 25f);

	[SerializeField]
	private byte _shortPriority = 180;

	[Space]
	[SerializeField]
	private Vector2 _mediumMinMax = new Vector2(15f, 50f);

	[SerializeField]
	private byte _mediumPriority = 128;

	[Space]
	[SerializeField]
	private Vector2 _longMinMax = new Vector2(25f, 150f);

	[SerializeField]
	private byte _longPriority = 8;

	[Header("Mixers")]
	[SerializeField]
	private AudioMixer _mainMixer;

	[SerializeField]
	private AudioMixerGroup _fxGroup;

	[SerializeField]
	private AudioMixerGroup _fxBypassMuteGroup;

	[Header("Ambient")]
	[SerializeField]
	private AudioSource _islandAmbientSource;

	[SerializeField]
	private float _islandAmbientVol = 0.2f;

	[SerializeField]
	private Vector2 _islandDistMinMax = new Vector2(3f, 7f);

	[Space]
	[SerializeField]
	private AudioSource _seaAmbientSource;

	[SerializeField]
	[Range(0f, 1f)]
	private float _seaAmbientVol = 0.5f;

	[FormerlySerializedAs("_oceanSqrtDistMinMax")]
	[SerializeField]
	private Vector2 _seaDistMinMax = new Vector2(3f, 7f);

	[Header("Temporary FX Mute")]
	[SerializeField]
	private AnimationCurve _muteCurve;

	private static readonly Dictionary<string, AudioClip> _realClips = new Dictionary<string, AudioClip>();

	private static readonly Dictionary<string, float> _recentClips = new Dictionary<string, float>();

	private static AudioSource _globalSource;

	private static AudioSource _globalSourceBypassMute;

	private static List<AudioSource> _globalVariedSources = new List<AudioSource>();

	private static List<AudioSource> _worldSources = new List<AudioSource>();

	private static List<AudioSource> _worldVariedSources = new List<AudioSource>();

	private static int _curGlobalVariedIndex;

	private static int _curWorldIndex;

	private static int _curWorldVariedIndex;

	private const int FrameDivisor = 16;

	private static bool _fxMuted;

	private bool _islandAmbientPlaying = true;

	public static VoiceInputType VoiceInputType { get; private set; } = VoiceInputType.Always;

	public static bool IsHoldingPushToTalk { get; private set; } = false;

	public static float MicrophoneGain { get; private set; } = 1f;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		_globalSource = base.gameObject.AddComponent<AudioSource>();
		_globalSource.outputAudioMixerGroup = _fxGroup;
		_globalSource.priority = 0;
		_globalSourceBypassMute = base.gameObject.AddComponent<AudioSource>();
		_globalSourceBypassMute.outputAudioMixerGroup = _fxBypassMuteGroup;
		_globalSourceBypassMute.priority = 0;
		for (int i = 0; i < _globalVariedSourcesAmount; i++)
		{
			float num = i - 2;
			num *= 0.025f;
			num++;
			AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.pitch = num;
			audioSource.outputAudioMixerGroup = _fxGroup;
			audioSource.priority = 0;
			_globalVariedSources.Add(audioSource);
		}
		for (int j = 0; j < _worldSourcesAmount; j++)
		{
			AudioSource item = UnityEngine.Object.Instantiate(_audioSourcePrefab, base.transform);
			_worldSources.Add(item);
		}
		for (int k = 0; k < _variedWorldSourcesAmount; k++)
		{
			float num2 = k - 2;
			num2 *= 0.025f;
			num2++;
			AudioSource audioSource2 = UnityEngine.Object.Instantiate(_audioSourcePrefab, base.transform);
			audioSource2.pitch = num2;
			_worldVariedSources.Add(audioSource2);
		}
		for (int num3 = _globalVariedSources.Count - 1; num3 > 0; num3--)
		{
			int num4 = UnityEngine.Random.Range(0, num3 + 1);
			List<AudioSource> globalVariedSources = _globalVariedSources;
			int index = num3;
			List<AudioSource> globalVariedSources2 = _globalVariedSources;
			int index2 = num4;
			AudioSource audioSource3 = _globalVariedSources[num4];
			AudioSource audioSource4 = _globalVariedSources[num3];
			AudioSource audioSource5 = (globalVariedSources[index] = audioSource3);
			audioSource5 = (globalVariedSources2[index2] = audioSource4);
		}
		for (int num5 = _worldVariedSources.Count - 1; num5 > 0; num5--)
		{
			int num6 = UnityEngine.Random.Range(0, num5 + 1);
			List<AudioSource> worldVariedSources = _worldVariedSources;
			int index2 = num5;
			List<AudioSource> globalVariedSources2 = _worldVariedSources;
			int index = num6;
			AudioSource audioSource4 = _worldVariedSources[num6];
			AudioSource audioSource3 = _worldVariedSources[num5];
			AudioSource audioSource5 = (worldVariedSources[index2] = audioSource4);
			audioSource5 = (globalVariedSources2[index] = audioSource3);
		}
	}

	private void Start()
	{
		_realClips.Clear();
		AudioClip[] clipsInPath = SettingsInstance.Settings.ClipsInPath;
		foreach (AudioClip audioClip in clipsInPath)
		{
			_realClips.Add(audioClip.name, audioClip);
		}
	}

	private void Update()
	{
		SetIslandAmbient();
		SetSeaAmbient();
		RemoveRecentClips();
	}

	public static AudioClip GetClip(string clipName)
	{
		if (_realClips.ContainsKey(clipName))
		{
			return _realClips[clipName];
		}
		return null;
	}

	public static AudioClip GetRandomClip(string clipName, int min, int max)
	{
		string key = clipName + UnityEngine.Random.Range(min, max + 1);
		if (_realClips.ContainsKey(key))
		{
			return _realClips[key];
		}
		return null;
	}

	private void SetIslandAmbient()
	{
		if (!Player.LocalPlayer || !Player.LocalPlayer.Transform || Player.IsInside)
		{
			if (_islandAmbientPlaying)
			{
				_islandAmbientPlaying = false;
				LeanTween.cancel(_islandAmbientSource.gameObject);
				LeanTween.value(_islandAmbientSource.gameObject, _islandAmbientSource.volume, 0f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate(SetIslandVol);
			}
			return;
		}
		if (!_islandAmbientPlaying)
		{
			_islandAmbientPlaying = true;
			LeanTween.cancel(_islandAmbientSource.gameObject);
			LeanTween.value(_islandAmbientSource.gameObject, _islandAmbientSource.volume, _islandAmbientVol, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate(SetIslandVol);
		}
		Vector3 islandPos = Island.IslandPos;
		islandPos.y = 0f;
		Vector3 vector = Player.LocalPlayer.Transform.position - Island.IslandPos;
		vector.y = 0f;
		float sqrMagnitude = vector.sqrMagnitude;
		vector.Normalize();
		vector *= Island.IslandSize + 15f;
		_islandAmbientSource.transform.position = islandPos + vector;
		_islandAmbientSource.spatialBlend = 1f - Mathf.InverseLerp(_islandDistMinMax.x * _islandDistMinMax.x * Island.IslandSize, _islandDistMinMax.y * _islandDistMinMax.y * Island.IslandSize, sqrMagnitude);
	}

	private void SetIslandVol(float to)
	{
		_islandAmbientSource.volume = to;
	}

	private void SetSeaAmbient()
	{
		if (!Player.LocalPlayer || !Player.LocalPlayer.Transform)
		{
			_seaAmbientSource.volume = 0f;
			return;
		}
		Vector2 vector = new Vector2(Player.LocalPlayer.Transform.position.x, Player.LocalPlayer.Transform.position.z);
		Vector2 vector2 = new Vector2(Island.IslandPos.x, Island.IslandPos.z);
		float sqrMagnitude = (vector - vector2).sqrMagnitude;
		_seaAmbientSource.volume = Mathf.InverseLerp(_seaDistMinMax.x * _seaDistMinMax.x * Island.IslandSize, _seaDistMinMax.y * _seaDistMinMax.y * Island.IslandSize, sqrMagnitude) * _seaAmbientVol;
	}

	private void RemoveRecentClips()
	{
		if (_recentClips.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, float> recentClip in _recentClips)
		{
			if (Time.time > recentClip.Value)
			{
				list.Add(recentClip.Key);
			}
		}
		foreach (string item in list)
		{
			_recentClips.Remove(item);
		}
	}

	public static void PlayClipAt(string clip, Vector3 position, bool variation = false, AudioDistance distance = AudioDistance.Medium, float volume = 1f, float minDelay = 0.1f)
	{
		if (Player.LocalPlayerEnabled)
		{
			PlayClip(clip, position, volume, minDelay, variation, distance);
		}
	}

	public static void PlayRandomClipAt(string clip, int min, int max, Vector3 position, bool variation = false, AudioDistance distance = AudioDistance.Medium, float volume = 1f, float minDelay = 0.1f)
	{
		if (Player.LocalPlayerEnabled)
		{
			string cooldownKey = clip;
			clip += UnityEngine.Random.Range(min, max + 1);
			PlayClip(clip, position, volume, minDelay, variation, distance, bypassMute: false, cooldownKey);
		}
	}

	public static void PlayGlobalClip(string clip, bool variation = false, float volume = 1f, float minDelay = 0.1f, bool bypassMute = false)
	{
		PlayClip(clip, Vector3.zero, volume, minDelay, variation, AudioDistance.Long, bypassMute);
	}

	public static void PlayRandomGlobalClip(string clip, int min, int max, bool variation = false, float volume = 1f, float minDelay = 0.1f)
	{
		string cooldownKey = clip;
		clip += UnityEngine.Random.Range(min, max + 1);
		PlayClip(clip, Vector3.zero, volume, minDelay, variation, AudioDistance.Medium, bypassMute: false, cooldownKey);
	}

	public static void PlayPlayerClip(string clip, Player player, bool variation = false, AudioDistance distance = AudioDistance.Medium, float volume = 1f, float minDelay = 0.1f)
	{
		if (Player.LocalPlayerEnabled && (bool)player)
		{
			PlayClip(clip, (!player) ? Vector3.zero : (player.Owner.IsLocalClient ? Vector3.zero : player.Transform.position), volume, minDelay, variation, distance);
		}
	}

	public static void PlayRandomPlayerClip(string clip, int min, int max, Player player, bool variation = false, AudioDistance distance = AudioDistance.Medium, float volume = 1f, float minDelay = 0.1f)
	{
		if (Player.LocalPlayerEnabled && (bool)player)
		{
			string cooldownKey = clip;
			clip += UnityEngine.Random.Range(min, max + 1);
			PlayClip(clip, player.Owner.IsLocalClient ? Vector3.zero : player.Transform.position, volume, minDelay, variation, distance, bypassMute: false, cooldownKey);
		}
	}

	private static void PlayClip(string clip, Vector3 position, float volume, float minDelay, bool variation, AudioDistance distance = AudioDistance.Medium, bool bypassMute = false, string cooldownKey = null)
	{
		if (!_instance)
		{
			MonoBehaviour.print("No AudioManager in scene");
		}
		else
		{
			if (string.IsNullOrEmpty(clip))
			{
				return;
			}
			if (cooldownKey == null)
			{
				cooldownKey = clip;
			}
			if (_recentClips.ContainsKey(cooldownKey))
			{
				return;
			}
			if (_realClips.ContainsKey(clip))
			{
				Vector2 minMaxDistance = GetMinMaxDistance(distance);
				int priority = GetPriority(distance);
				if (position == Vector3.zero)
				{
					if (variation)
					{
						int curIndex = GetCurIndex(isGlobal: true, variation: true);
						_globalVariedSources[curIndex].PlayOneShot(_realClips[clip], volume * _instance._volume);
					}
					else if (!bypassMute)
					{
						_globalSource.PlayOneShot(_realClips[clip], volume * _instance._volume);
					}
					else
					{
						_globalSourceBypassMute.PlayOneShot(_realClips[clip], volume * _instance._volume);
					}
				}
				else if (variation)
				{
					int curIndex2 = GetCurIndex(isGlobal: false, variation: true);
					_worldVariedSources[curIndex2].minDistance = minMaxDistance.x;
					_worldVariedSources[curIndex2].maxDistance = minMaxDistance.y;
					_worldVariedSources[curIndex2].priority = priority;
					_worldVariedSources[curIndex2].transform.position = position;
					_worldVariedSources[curIndex2].PlayOneShot(_realClips[clip], volume * _instance._volume);
				}
				else
				{
					int curIndex3 = GetCurIndex(isGlobal: false, variation: false);
					_worldSources[curIndex3].minDistance = minMaxDistance.x;
					_worldSources[curIndex3].maxDistance = minMaxDistance.y;
					_worldSources[curIndex3].priority = priority;
					_worldSources[curIndex3].transform.position = position;
					_worldSources[curIndex3].PlayOneShot(_realClips[clip], volume * _instance._volume);
				}
			}
			else if (clip != "")
			{
				MonoBehaviour.print("Missing clip: " + clip);
			}
			if (minDelay != 0f)
			{
				_recentClips.Add(cooldownKey, Time.time + minDelay);
			}
		}
	}

	private static int GetCurIndex(bool isGlobal, bool variation)
	{
		if (isGlobal & variation)
		{
			_curGlobalVariedIndex++;
			if (_curGlobalVariedIndex >= _globalVariedSources.Count)
			{
				_curGlobalVariedIndex = 0;
			}
			return _curGlobalVariedIndex;
		}
		if (variation)
		{
			_curWorldVariedIndex++;
			if (_curWorldVariedIndex >= _worldVariedSources.Count)
			{
				_curWorldVariedIndex = 0;
			}
			return _curWorldVariedIndex;
		}
		_curWorldIndex++;
		if (_curWorldIndex >= _worldSources.Count)
		{
			_curWorldIndex = 0;
		}
		return _curWorldIndex;
	}

	public static void SetAudioInputType(VoiceInputType type)
	{
		VoiceInputType = type;
	}

	public static void TogglePushToTalk(bool to)
	{
		IsHoldingPushToTalk = to;
	}

	public static void SetVolume(float to, VolumeType type)
	{
		if (!_instance)
		{
			return;
		}
		if (type == VolumeType.Gain)
		{
			MicrophoneGain = to;
			return;
		}
		float value = Mathf.Log10(Mathf.Clamp(to, 0.0001f, 1f)) * 20f;
		string text = "Master";
		string text2 = "";
		switch (type)
		{
		case VolumeType.Music:
			text = "Music";
			break;
		case VolumeType.FX:
			text = "FX";
			text2 = "FXBypassMute";
			break;
		case VolumeType.Proxy:
			text = "Proxy";
			break;
		}
		if (text != "FX" || !_fxMuted)
		{
			_instance._mainMixer.SetFloat(text, value);
		}
		if (text2 != "")
		{
			_instance._mainMixer.SetFloat(text2, value);
		}
	}

	public static void MuteTemporarily()
	{
		if ((bool)_instance)
		{
			_instance._mainMixer.GetFloat("FXBypassMute", out var value);
			float num = Mathf.Log10(0.0001f) * 20f;
			_instance._mainMixer.SetFloat("FX", num);
			LeanTween.cancel(_instance.gameObject);
			LeanTween.value(_instance.gameObject, num, value, 3f).setEase(_instance._muteCurve).setOnUpdate(_instance.UpdateFXVol);
			_instance._mainMixer.GetFloat("Music", out var value2);
			float num2 = Mathf.Log10(0.0001f) * 20f;
			_instance._mainMixer.SetFloat("FX", num2);
			LeanTween.value(_instance.gameObject, num2, value2, 5f).setEase(_instance._muteCurve).setOnUpdate(_instance.UpdateMusicVol);
		}
	}

	private void UpdateFXVol(float to)
	{
		_mainMixer.SetFloat("FX", to);
	}

	private void UpdateMusicVol(float to)
	{
		_mainMixer.SetFloat("Music", to);
	}

	public static float GetRms(ReadOnlySpan<float> samples)
	{
		if (samples == null || samples.Length == 0)
		{
			return 0f;
		}
		float num = 0f;
		ReadOnlySpan<float> readOnlySpan = samples;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			float num2 = readOnlySpan[i];
			num += num2 * num2;
		}
		return Mathf.Sqrt(num / (float)samples.Length);
	}

	private static float GetMaxRms(float[] samples)
	{
		if (samples == null || samples.Length == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < samples.Length; i += 16)
		{
			float rms = GetRms(samples.AsSpan(i, 16));
			num = Mathf.Max(num, rms);
		}
		return num;
	}

	public static float GetDecibel(float[] samples)
	{
		float maxRms = GetMaxRms(samples);
		return Mathf.Log10(Mathf.Max(0.0001f, maxRms)) * 20f;
	}

	private static Vector2 GetMinMaxDistance(AudioDistance distance)
	{
		if (!_instance)
		{
			return new Vector2(15f, 50f);
		}
		return distance switch
		{
			AudioDistance.VeryShort => _instance._veryShortMinMax, 
			AudioDistance.Short => _instance._shortMinMax, 
			AudioDistance.Medium => _instance._mediumMinMax, 
			AudioDistance.Long => _instance._longMinMax, 
			_ => Vector2.zero, 
		};
	}

	private static int GetPriority(AudioDistance distance)
	{
		if (!_instance)
		{
			return 128;
		}
		return distance switch
		{
			AudioDistance.VeryShort => _instance._veryShortPriority, 
			AudioDistance.Short => _instance._shortPriority, 
			AudioDistance.Medium => _instance._mediumPriority, 
			AudioDistance.Long => _instance._longPriority, 
			_ => _instance._mediumPriority, 
		};
	}
}
