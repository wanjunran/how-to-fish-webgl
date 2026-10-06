using System.Collections.Generic;
using UnityEngine;

public static class RigidbodyManager
{
	public static readonly HashSet<RigidbodySync> RigSyncs = new HashSet<RigidbodySync>();

	public static readonly Dictionary<Collider, Rigidbody> Debris = new Dictionary<Collider, Rigidbody>();
}
