using System;
using UnityEngine;

[Serializable]
public class RadioChannel
{
	[SerializeField]
	private AudioSource _channelSource;

	[SerializeField]
	[Range(88f, 108f)]
	private float _frequency = 98f;

	private bool _initialized;

	public float Frequency => _frequency;

	public bool IsMuted { get; private set; }

	public void ToggleMute(bool to, float time)
	{
		if (IsMuted != to || !_initialized)
		{
			_initialized = true;
			IsMuted = to;
			if (to)
			{
				_channelSource.Stop();
			}
			else if (time != 0f)
			{
				_channelSource.time = time % _channelSource.clip.length;
				_channelSource.Play();
			}
		}
	}

	public void SetVol(float to)
	{
		_channelSource.volume = to;
	}
}
