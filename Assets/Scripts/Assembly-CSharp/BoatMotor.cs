using System;
using UnityEngine;

public class BoatMotor : MonoBehaviour
{
	[SerializeField]
	private float _maxRotation = 35f;

	[SerializeField]
	private Transform[] _toRotate;

	[field: SerializeField]
	public float Force { get; private set; }

	[field: SerializeField]
	public Transform Propeller { get; private set; }

	[field: SerializeField]
	public Transform[] VisualPropellers { get; private set; }

	[field: SerializeField]
	public AudioSource[] MotorSounds { get; private set; } = Array.Empty<AudioSource>();

	[field: SerializeField]
	public AudioSource[] MotorIdleSounds { get; private set; } = Array.Empty<AudioSource>();

	[field: SerializeField]
	[field: Min(0f)]
	[field: Tooltip("Delay after the engine start sound before throttle and looping motor sounds are enabled.")]
	public float MotorStartDelay { get; private set; }

	[field: SerializeField]
	public string MotorStartSoundName { get; private set; }

	[field: SerializeField]
	public string MotorStopSoundName { get; private set; }

	public void SetRotation(float to)
	{
		Transform[] toRotate = _toRotate;
		for (int i = 0; i < toRotate.Length; i++)
		{
			toRotate[i].localRotation = Quaternion.Euler(-90f, 0f, (0f - to) * _maxRotation);
		}
	}
}
