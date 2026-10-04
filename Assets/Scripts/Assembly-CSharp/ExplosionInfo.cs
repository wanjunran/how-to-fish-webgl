using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ExplosionInfo
{
	[SerializeField]
	private int _damage = 100;

	[FormerlySerializedAs("_killRadius")]
	[SerializeField]
	[Tooltip("Radius of deadly explosion")]
	private float damageRadius = 3.5f;

	[SerializeField]
	[Tooltip("Radius of knockback explosion")]
	private float _forceRadius = 2.5f;

	[SerializeField]
	[Tooltip("Force of knockback")]
	private float _itemForce = 1000f;

	[SerializeField]
	[Tooltip("Force of knockback")]
	private float _boatForce = 1000f;

	[SerializeField]
	private float _playerForce = 50f;

	[SerializeField]
	private bool _hasUnderwaterExplosion = true;

	[SerializeField]
	private bool _onlyExplodeOnce = true;

	[SerializeField]
	private Vector2Int _underwaterFishMinMax = new Vector2Int(3, 5);

	[SerializeField]
	[Tooltip("Duration of controller vibration upon explosion")]
	private float _vibrationDuration = 0.3f;

	[SerializeField]
	[Tooltip("Frequency of vibration over time upon explosion")]
	private AnimationCurve _vibrationCurve;

	[SerializeField]
	[Tooltip("Name of particle effect that will play upon explosion")]
	private string _explosionParticleName;

	[SerializeField]
	[Tooltip("Name of sound effect that will play upon explosion")]
	private string _explosionSoundName;

	[SerializeField]
	[Tooltip("If it should play a random clip, how many random clips are there?")]
	private int _explosionSounds;

	[SerializeField]
	private float _explosionSoundVol = 1f;

	[SerializeField]
	private float _screenShakeAmount = 1000f;

	public int Damage => _damage;

	public float DamageRadius => damageRadius;

	public float ForceRadius => _forceRadius;

	public float ItemForce => _itemForce;

	public float BoatForce => _boatForce;

	public float PlayerForce => _playerForce;

	public bool HasUnderwaterExplosion => _hasUnderwaterExplosion;

	public bool OnlyExplodeOnce => _onlyExplodeOnce;

	public Vector2Int UnderWaterFishMinMax => _underwaterFishMinMax;

	public float VibrationDuration => _vibrationDuration;

	public AnimationCurve VibrationCurve => _vibrationCurve;

	public string ExplosionParticleName => _explosionParticleName;

	public string ExplosionSoundName => _explosionSoundName;

	public int ExplosionSounds => _explosionSounds;

	public float ExplosionSoundVol => _explosionSoundVol;

	public float ScreenShakeAmount => _screenShakeAmount;

	public bool HasExploded { get; private set; }

	public void Explode()
	{
		HasExploded = true;
	}
}
