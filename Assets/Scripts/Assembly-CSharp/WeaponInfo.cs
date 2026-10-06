using System;
using UnityEngine;

[Serializable]
public class WeaponInfo
{
	[field: SerializeField]
	[field: Header("Weapon is assigned automatically")]
	public Weapon Weapon { get; set; }

	[field: SerializeField]
	public byte ProjectileType { get; set; }

	[field: SerializeField]
	public int ProjectileDamage { get; set; }

	[field: SerializeField]
	public float ProjectileForce { get; set; }

	[field: SerializeField]
	public float ProjectileGravity { get; set; }

	[field: SerializeField]
	public string ShootVFX { get; set; }

	[field: SerializeField]
	[field: Tooltip("0 = Dont override")]
	public float BoatForceOverride { get; set; }
}
