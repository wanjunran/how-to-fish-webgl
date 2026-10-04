using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Impact Type", menuName = "How to Fish/Audio Impact Type")]
public class ImpactSoundType : ScriptableObject
{
	[SerializeField]
	private AudioClip[] _clips;

	[SerializeField]
	private float[] _minVelToPlayClips;

	[SerializeField]
	private float _velForMaxVol;

	[SerializeField]
	[Range(0f, 1f)]
	private float _maxVol = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _minVol;

	[SerializeField]
	private float _minDelay = 0.15f;

	public AudioClip[] Clips => _clips;

	public float[] MinVelToPlayClips => _minVelToPlayClips;

	public float VelForMaxVol => _velForMaxVol;

	public float MaxVol => _maxVol;

	public float MinVol => _minVol;

	public float MinDelay => _minDelay;
}
