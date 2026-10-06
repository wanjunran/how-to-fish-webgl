using System.Collections.Generic;
using UnityEngine;

public class AudioImpactManager : MonoBehaviour
{
	private static AudioImpactManager _instance;

	private static readonly Dictionary<Item, float> _timeOfLastSound = new Dictionary<Item, float>();

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
	}

	public static void PlayLocalImpactSound(Item item, Collision col, Vector3 vel)
	{
		Item item2 = ItemManager.Get(col);
		if (!item || item.IsDeinitializing || item2 == item || ((bool)item2 && (bool)item2.Bird) || (_timeOfLastSound.ContainsKey(item) && Time.time < _timeOfLastSound[item] + item.ImpactSoundType.MinDelay))
		{
			return;
		}
		_timeOfLastSound[item] = Time.time;
		float num = Vector3.Dot(-vel, col.contacts[0].normal);
		AudioClip audioClip = null;
		for (int num2 = item.ImpactSoundType.Clips.Length - 1; num2 >= 0; num2--)
		{
			if (num >= item.ImpactSoundType.MinVelToPlayClips[num2])
			{
				audioClip = item.ImpactSoundType.Clips[num2];
				break;
			}
		}
		if ((bool)audioClip)
		{
			float t = Mathf.InverseLerp(item.ImpactSoundType.MinVelToPlayClips[0], item.ImpactSoundType.VelForMaxVol, num);
			t = Mathf.Lerp(item.ImpactSoundType.MinVol, item.ImpactSoundType.MaxVol, t);
			AudioManager.PlayClipAt(audioClip.name, col.contacts[0].point, variation: true, AudioDistance.Short, t, 0.05f);
			byte vel2 = (byte)(Mathf.InverseLerp(0f, item.ImpactSoundType.VelForMaxVol, num) * 255f);
			if (Server.Instance.IsServerInitialized)
			{
				ObserverToLocalAdapter.Instance.PlayImpactSound(item, vel2);
			}
			else
			{
				Server.Instance.PlayImpactSound(item, vel2);
			}
		}
	}

	public static void PlayReceivedImpactSound(Item item, byte vel)
	{
		if (!item || (_timeOfLastSound.ContainsKey(item) && Time.time < _timeOfLastSound[item] + item.ImpactSoundType.MinDelay) || item.RigidbodySync.IsSimulatedLocal)
		{
			return;
		}
		float num = (float)(int)vel / item.ImpactSoundType.VelForMaxVol;
		_timeOfLastSound[item] = Time.time;
		AudioClip audioClip = null;
		bool flag = false;
		for (int num2 = item.ImpactSoundType.Clips.Length - 1; num2 >= 0; num2--)
		{
			if (num >= item.ImpactSoundType.MinVelToPlayClips[num2])
			{
				audioClip = item.ImpactSoundType.Clips[num2];
				if (num2 == item.ImpactSoundType.Clips.Length - 1)
				{
					flag = true;
				}
				break;
			}
		}
		if ((bool)audioClip)
		{
			float t = Mathf.InverseLerp(item.ImpactSoundType.MinVelToPlayClips[0], item.ImpactSoundType.VelForMaxVol, num);
			t = Mathf.Lerp(item.ImpactSoundType.MinVol, item.ImpactSoundType.MaxVol, t);
			AudioManager.PlayClipAt(audioClip.name, item.Rig.worldCenterOfMass, variation: true, (!flag) ? AudioDistance.Short : AudioDistance.Medium, t, 0.05f);
		}
	}

	public static void PlayExtraImpactSound(Item item)
	{
		if ((bool)item.ImpactSoundType)
		{
			float volume = Mathf.Lerp(item.ImpactSoundType.MinVol, item.ImpactSoundType.MaxVol, 0.25f);
			AudioManager.PlayClipAt(item.ImpactSoundType.Clips[0].name, item.Rig.worldCenterOfMass, variation: true, AudioDistance.Short, volume, 0.05f);
		}
	}
}
