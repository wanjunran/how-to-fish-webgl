using UnityEngine;

public class Projectile
{
	public byte TypeId;

	public Player Owner;

	public bool IsLocal;

	public uint CatchingUpToDo;

	public uint Id;

	public Vector3 PreviousPosition;

	public Vector3 SpawnPos;

	public Vector3 Position;

	public Vector3 PreviousVelocity;

	public Vector3 Velocity;

	public int Damage;

	public float Force;

	public float BoatForceOverride;

	public float GravityForce;

	public bool CanHurtSelf;

	public bool FromNpc;
}
