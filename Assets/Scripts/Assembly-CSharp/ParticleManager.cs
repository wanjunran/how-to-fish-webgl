using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
	private class ParticleType
	{
		public ParticleSystem ParticleSystem;

		public readonly List<Matrix4x4> Queue = new List<Matrix4x4>();
	}

	private static readonly Dictionary<string, ParticleType> _particles = new Dictionary<string, ParticleType>();

	[SerializeField]
	private Transform _particleHolder;

	private void Awake()
	{
		_particles.Clear();
		for (int i = 0; i < _particleHolder.childCount; i++)
		{
			_particles.Add(_particleHolder.GetChild(i).name, new ParticleType
			{
				ParticleSystem = _particleHolder.GetChild(i).GetComponent<ParticleSystem>()
			});
		}
	}

	private void Update()
	{
		foreach (KeyValuePair<string, ParticleType> particle in _particles)
		{
			if (particle.Value.Queue.Count > 0)
			{
				RealPlay(particle.Key, particle.Value.Queue[0].GetPosition(), particle.Value.Queue[0].rotation);
				particle.Value.Queue.RemoveAt(0);
			}
		}
	}

	public static void Play(string type, Vector3 position)
	{
		Play(type, position, Vector3.zero);
	}

	public static void Play(string type, Vector3 position, Vector3 direction)
	{
		if (!string.IsNullOrEmpty(type))
		{
			if (!_particles.ContainsKey(type))
			{
				Debug.LogWarning($"Could not find {type} in {_particles}");
			}
			else
			{
				_particles[type].Queue.Add(Matrix4x4.TRS(position, (direction == Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(direction), Vector3.one));
			}
		}
	}

	private static void RealPlay(string type, Vector3 position, Quaternion rotation)
	{
		_particles[type].ParticleSystem.transform.position = position;
		_particles[type].ParticleSystem.transform.rotation = rotation;
		_particles[type].ParticleSystem.Play();
	}
}
