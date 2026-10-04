using System;
using UnityEngine;

[Serializable]
public class AudioSequenceStep
{
	public AudioClip[] Clips = Array.Empty<AudioClip>();

	public float Timer;

	[Range(0f, 2f)]
	public float Volume = 1f;

	public AudioClip GetRandomClip()
	{
		if (Clips == null)
		{
			return null;
		}
		int num = 0;
		AudioClip[] clips = Clips;
		for (int i = 0; i < clips.Length; i++)
		{
			if ((bool)clips[i])
			{
				num++;
			}
		}
		if (num == 0)
		{
			return null;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		clips = Clips;
		foreach (AudioClip audioClip in clips)
		{
			if ((bool)audioClip)
			{
				if (num2 == 0)
				{
					return audioClip;
				}
				num2--;
			}
		}
		return null;
	}
}
