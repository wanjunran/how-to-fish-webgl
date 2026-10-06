using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class AudioSequence : ISerializationCallbackReceiver
{
	public AudioSequenceStep[] Steps = Array.Empty<AudioSequenceStep>();

	public AudioDistance Distance;

	public float Volume = 1f;

	[FormerlySerializedAs("Clips")]
	[SerializeField]
	[HideInInspector]
	private AudioClip[] _legacyClips;

	[FormerlySerializedAs("Timers")]
	[SerializeField]
	[HideInInspector]
	private float[] _legacyTimers;

	[FormerlySerializedAs("Volumes")]
	[SerializeField]
	[HideInInspector]
	private float[] _legacyVolumes;

	public void OnBeforeSerialize()
	{
		MigrateLegacyData();
	}

	public void OnAfterDeserialize()
	{
		MigrateLegacyData();
	}

	private void MigrateLegacyData()
	{
		if (Steps != null && Steps.Length != 0)
		{
			InitializeMissingSteps();
			ClearLegacyData();
			return;
		}
		AudioClip[] legacyClips = _legacyClips;
		int num = ((legacyClips != null) ? legacyClips.Length : 0);
		Steps = new AudioSequenceStep[num];
		for (int i = 0; i < num; i++)
		{
			Steps[i] = new AudioSequenceStep
			{
				Clips = new AudioClip[1] { _legacyClips[i] },
				Timer = ((_legacyTimers != null && i < _legacyTimers.Length) ? _legacyTimers[i] : 0f),
				Volume = ((_legacyVolumes != null && i < _legacyVolumes.Length) ? _legacyVolumes[i] : 1f)
			};
		}
		ClearLegacyData();
	}

	private void InitializeMissingSteps()
	{
		for (int i = 0; i < Steps.Length; i++)
		{
			AudioSequenceStep[] steps = Steps;
			int num = i;
			if (steps[num] == null)
			{
				steps[num] = new AudioSequenceStep();
			}
		}
	}

	private void ClearLegacyData()
	{
		_legacyClips = null;
		_legacyTimers = null;
		_legacyVolumes = null;
	}
}
