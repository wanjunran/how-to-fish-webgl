using System.Collections.Generic;
using UnityEngine;

public class AudioSequenceManager : MonoBehaviour
{
	private class ActiveSequence
	{
		public AudioSequence Sequence;

		public int CurrentIndex;

		public float Timer;

		public Player Player;

		public AudioDistance Distance;

		public float Volume;

		public GameObject Caller;
	}

	private static AudioSequenceManager _instance;

	private static readonly Dictionary<int, ActiveSequence> _activeSequences = new Dictionary<int, ActiveSequence>();

	private static int _curId;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
	}

	public static void PlayPlayerSequence(AudioSequence sequence, Player player, GameObject caller)
	{
		if (sequence.Steps.Length != 0)
		{
			ActiveSequence value = new ActiveSequence
			{
				Sequence = sequence,
				CurrentIndex = 0,
				Timer = sequence.Steps[0].Timer,
				Distance = sequence.Distance,
				Player = player,
				Volume = sequence.Volume,
				Caller = caller
			};
			_activeSequences.Add(_curId, value);
			_curId++;
		}
	}

	public static void CancelAllActiveSequencesFromOwner(GameObject owner)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, ActiveSequence> activeSequence in _activeSequences)
		{
			if (activeSequence.Value.Caller == owner)
			{
				list.Add(activeSequence.Key);
			}
		}
		foreach (int item in list)
		{
			_activeSequences.Remove(item);
		}
	}

	private void Update()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, ActiveSequence> activeSequence in _activeSequences)
		{
			activeSequence.Value.Timer -= Time.deltaTime;
			if (!(activeSequence.Value.Timer <= 0f))
			{
				continue;
			}
			if ((bool)activeSequence.Value.Player && (bool)activeSequence.Value.Caller)
			{
				AudioSequenceStep audioSequenceStep = activeSequence.Value.Sequence.Steps[activeSequence.Value.CurrentIndex];
				AudioClip randomClip = audioSequenceStep.GetRandomClip();
				if ((bool)randomClip)
				{
					AudioManager.PlayPlayerClip(randomClip.name, activeSequence.Value.Player, variation: false, activeSequence.Value.Distance, activeSequence.Value.Volume * audioSequenceStep.Volume);
				}
				activeSequence.Value.CurrentIndex++;
				if (activeSequence.Value.CurrentIndex >= activeSequence.Value.Sequence.Steps.Length)
				{
					list.Add(activeSequence.Key);
				}
				else
				{
					activeSequence.Value.Timer = activeSequence.Value.Sequence.Steps[activeSequence.Value.CurrentIndex].Timer;
				}
			}
			else
			{
				list.Add(activeSequence.Key);
			}
		}
		foreach (int item in list)
		{
			_activeSequences.Remove(item);
		}
	}
}
