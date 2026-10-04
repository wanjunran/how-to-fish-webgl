using System;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceManager : MonoBehaviour
{
	[Serializable]
	public class SurfaceType
	{
		public string StepSoundName;

		public Transform[] Transforms;
	}

	public static SurfaceManager Instance;

	[SerializeField]
	private SurfaceType[] _surfaceTypes;

	private readonly Dictionary<Transform, string> _stepSounds = new Dictionary<Transform, string>();

	private void Awake()
	{
		Instance = this;
		CreateDictionary();
	}

	private void CreateDictionary()
	{
		SurfaceType[] surfaceTypes = _surfaceTypes;
		foreach (SurfaceType surfaceType in surfaceTypes)
		{
			Transform[] transforms = surfaceType.Transforms;
			foreach (Transform transform in transforms)
			{
				if ((bool)transform)
				{
					_stepSounds[transform] = surfaceType.StepSoundName;
				}
			}
		}
	}

	private string GetIslandStepSoundName(Transform surfaceTransform)
	{
		if (!_stepSounds.TryGetValue(surfaceTransform, out var value))
		{
			return GameInfo.DefaultStepSound;
		}
		return value;
	}

	public static string GetStepSound(Transform surfaceTransform, Vector3 position)
	{
		if (!Instance)
		{
			return GameInfo.DefaultStepSound;
		}
		if ((bool)surfaceTransform)
		{
			if (surfaceTransform.CompareTag("Boat"))
			{
				return GameInfo.BoatStepSound;
			}
			if (WaterManager.IsUnderWater(position))
			{
				return GameInfo.WaterStepSound;
			}
			return Instance.GetIslandStepSoundName(surfaceTransform);
		}
		if (WaterManager.IsUnderWater(position))
		{
			return GameInfo.WaterStepSound;
		}
		return GameInfo.DefaultStepSound;
	}
}
