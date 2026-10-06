using System.Collections.Generic;
using UnityEngine;

public class ParticleAudio : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Audio clip to play when particle collides with something")]
	private string _clip;

	[SerializeField]
	[Tooltip("Volume multiplier. Used together with velocity of particle to set volume")]
	private float _volMulti;

	[SerializeField]
	private float _maxVol = 1f;

	[SerializeField]
	private int _soundCount;

	private ParticleSystem _particle;

	private readonly List<ParticleCollisionEvent> _cols = new List<ParticleCollisionEvent>();

	private void Awake()
	{
		_particle = GetComponent<ParticleSystem>();
	}

	private void OnParticleCollision(GameObject go)
	{
		int collisionEvents = _particle.GetCollisionEvents(go, _cols);
		for (int i = 0; i < collisionEvents; i++)
		{
			if (go.transform.position.y > WaterManager.WaterHeight)
			{
				AudioManager.PlayRandomClipAt(_clip, 1, _soundCount, go.transform.position, variation: true, AudioDistance.VeryShort, Mathf.Clamp(_cols[i].velocity.sqrMagnitude * _volMulti, 0f, _maxVol));
			}
		}
	}
}
