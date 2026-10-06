using System.Collections;
using UnityEngine;

public class PlayerUnderwater : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private AudioSource _loopSound;

	[SerializeField]
	private string _introSoundName;

	[SerializeField]
	private string _outroSoundName;

	[SerializeField]
	[Range(0f, 1f)]
	private float _introSoundVolume = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _outroSoundVolume = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _loopSoundVolume = 1f;

	[SerializeField]
	private float _loopStartDelay;

	[SerializeField]
	private float _loopStopDelay;

	[SerializeField]
	private float _loopFadeInTime = 1f;

	[SerializeField]
	private float _loopFadeOutTime = 1f;

	[SerializeField]
	private AnimationCurve _loopFadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve _loopFadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private bool _shouldPlaySounds;

	private Coroutine _loopCoroutine;

	public void ToggleSounds()
	{
		SetSoundsPlaying(!_shouldPlaySounds);
	}

	public void SetSoundsPlaying(bool shouldPlay)
	{
		if (_shouldPlaySounds != shouldPlay)
		{
			_shouldPlaySounds = shouldPlay;
			if (_loopCoroutine != null)
			{
				StopCoroutine(_loopCoroutine);
			}
			if (_shouldPlaySounds)
			{
				AudioManager.PlayGlobalClip(_introSoundName, variation: false, _introSoundVolume);
				_loopCoroutine = StartCoroutine(StartLoopSound());
			}
			else
			{
				AudioManager.PlayGlobalClip(_outroSoundName, variation: false, _outroSoundVolume);
				_loopCoroutine = StartCoroutine(StopLoopSound());
			}
		}
	}

	private IEnumerator StartLoopSound()
	{
		if (_loopStartDelay > 0f)
		{
			yield return new WaitForSeconds(_loopStartDelay);
		}
		if (_shouldPlaySounds && (bool)_loopSound)
		{
			if (!_loopSound.isPlaying)
			{
				_loopSound.Play();
			}
			yield return FadeLoopSound(0f, _loopSoundVolume, _loopFadeInTime, _loopFadeInCurve);
			_loopCoroutine = null;
		}
	}

	private IEnumerator StopLoopSound()
	{
		if (_loopStopDelay > 0f)
		{
			yield return new WaitForSeconds(_loopStopDelay);
		}
		if (!_shouldPlaySounds && (bool)_loopSound)
		{
			yield return FadeLoopSound(_loopSound.volume, 0f, _loopFadeOutTime, _loopFadeOutCurve);
			_loopSound.Stop();
			_loopCoroutine = null;
		}
	}

	private IEnumerator FadeLoopSound(float from, float to, float duration, AnimationCurve curve)
	{
		if (!_loopSound)
		{
			yield break;
		}
		if (duration <= 0f)
		{
			_loopSound.volume = to;
			yield break;
		}
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float time = Mathf.Clamp01(timer / duration);
			float t = curve.Evaluate(time);
			_loopSound.volume = Mathf.LerpUnclamped(from, to, t);
			yield return null;
		}
		_loopSound.volume = to;
	}
}
