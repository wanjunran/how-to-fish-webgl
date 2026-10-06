using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProjectileType
{
	[Header("Behaviour")]
	[Tooltip("Projectiles use Physics.SphereCast to detect hits, this is the radius of that sphere")]
	public float WidthRadius;

	[Tooltip("Replaces bullet travel over time with a single raycast")]
	public bool IsHitScan;

	[Tooltip("Amount of force applied to rigidbodies that are hit")]
	public float PlayerForce;

	[Header("Visuals")]
	[Tooltip("Name of the type of instance in InstanceManager to use for mesh")]
	public string MeshInstance;

	public Vector3 MeshScale;

	[Space]
	[Tooltip("Name of the type of instance in InstanceManager to use for landed mesh (for bullet holes & landed arrows)")]
	public string LandedMeshInstance;

	[Tooltip("Scale of landed mesh")]
	public Vector3 LandedMeshScale;

	[Tooltip("Should landed mesh face normal of hit surface or keep facing last projectile velocity?")]
	public bool LandedMeshFaceNormal;

	[Space]
	[Tooltip("Name of decal to spawn when landing, if any")]
	public string LandedDecal;

	[Space]
	[Tooltip("Size of decal to spawn when landing")]
	public float LandedDecalSize = 0.05f;

	[Space]
	[Tooltip("Hit scan projectiles are visualised by a line that fades with a certain speed")]
	public float HitScanLineFadeSpeed;

	[Space]
	[Tooltip("Name of particle effect in ParticleManager that should play where the projectile hits something")]
	public string HitParticle;

	[Header("Audio")]
	[Tooltip("Name of sound effect to be played when the projectile hits something")]
	public string HitSound;

	[Tooltip("How many random sounds does the HitSound contain? 0 = will not use random sounds")]
	public int RandomSounds;

	[Range(0f, 1f)]
	[Tooltip("Volume of sound effect that plays when projectile hits something")]
	public float HitSoundVolume;

	public readonly List<Projectile> Projectiles = new List<Projectile>();

	public readonly List<Projectile> ProjectilesToRemove = new List<Projectile>();

	public readonly List<Matrix4x4> InstanceMatrices = new List<Matrix4x4>();
}
