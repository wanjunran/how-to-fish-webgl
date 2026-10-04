using UnityEngine;

public class MusicManager : MonoBehaviour
{
	private static MusicManager _instance;

	[SerializeField]
	private AudioSource _musicSourceLooping;

	[SerializeField]
	private AudioSource _musicSourceOneShot;

	[Space]
	[SerializeField]
	private float _tweenOutTime = 1f;

	[SerializeField]
	private float _tweenInTime = 3f;

	private static AudioClip _targetMusicClip;

	private static AudioClip _introClip;

	private static AudioClip _outroClip;

	private static bool _replayingSong;

	private static bool _isPlaying;

	private static float _introDelay;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		Server.OnServerStopped += OnServerStopped;
	}

	private void OnServerStopped()
	{
		_ = (bool)_instance;
	}

	private static void FadeOutMusic(bool doubleFadeTime = false)
	{
		_isPlaying = false;
		if ((bool)_instance)
		{
			if ((bool)_outroClip)
			{
				_instance._musicSourceOneShot.PlayOneShot(_outroClip);
			}
			LeanTween.cancel(_instance._musicSourceLooping.gameObject);
			float time = (doubleFadeTime ? (_instance._tweenOutTime * 2f) : _instance._tweenOutTime);
			LeanTween.value(_instance._musicSourceLooping.gameObject, _instance._musicSourceLooping.volume, 0f, time).setEase(LeanTweenType.easeOutQuad).setOnUpdate(_instance.UpdateLoopingVolume)
				.setOnComplete(FadeInMusic);
		}
	}

	public static void PlayMusic(string clip, string intro = "", float introDelay = 5f, bool loop = true)
	{
		if (!_instance)
		{
			MonoBehaviour.print("No MusicManager in scene");
		}
		else
		{
			if (string.IsNullOrEmpty(clip))
			{
				return;
			}
			if ((bool)_instance._musicSourceLooping.clip && _instance._musicSourceLooping.clip.name == clip)
			{
				_replayingSong = true;
				if (_isPlaying)
				{
					return;
				}
			}
			else
			{
				_replayingSong = false;
			}
			_targetMusicClip = AudioManager.GetClip(clip);
			_introClip = AudioManager.GetClip(intro);
			_introDelay = introDelay;
			_instance._musicSourceLooping.loop = loop;
			if (_isPlaying)
			{
				FadeOutMusic();
			}
			else
			{
				FadeInMusic();
			}
		}
	}

	public static void StopMusic(string clip, string outro = "")
	{
		if (_isPlaying && (bool)_targetMusicClip && !(_targetMusicClip.name != clip))
		{
			_targetMusicClip = null;
			_outroClip = AudioManager.GetClip(outro);
			FadeOutMusic(clip == "MainMenu");
		}
	}

	private void UpdateLoopingVolume(float to)
	{
		_instance._musicSourceLooping.volume = to;
	}

	private static void FadeInMusic()
	{
		if ((bool)_targetMusicClip)
		{
			_isPlaying = true;
			_instance._musicSourceLooping.clip = _targetMusicClip;
			if ((bool)_introClip)
			{
				_instance._musicSourceOneShot.PlayOneShot(_introClip);
				LeanTween.cancel(_instance._musicSourceLooping.gameObject);
				LeanTween.value(_instance._musicSourceLooping.gameObject, 0f, 1f, _introDelay).setOnComplete(_instance.PlayMusic);
			}
			else
			{
				_instance.PlayMusic();
			}
		}
	}

	private void PlayMusic()
	{
		LeanTween.cancel(_instance._musicSourceLooping.gameObject);
		LeanTween.value(_instance._musicSourceLooping.gameObject, _instance._musicSourceLooping.volume, 1f, _instance._tweenInTime).setEase(LeanTweenType.easeOutQuad).setOnUpdate(_instance.UpdateLoopingVolume);
		if (!_replayingSong)
		{
			_instance._musicSourceLooping.Play();
		}
	}
}
