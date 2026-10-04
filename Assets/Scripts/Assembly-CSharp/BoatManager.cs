using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class BoatManager : NetworkBehaviour
{
	public static BoatManager Instance;

	public static readonly Dictionary<Collider, Boat> ColToBoat = new Dictionary<Collider, Boat>();

	private bool NetworkInitialize___EarlyBoatManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateBoatManagerAssembly_002DCSharp_002Edll_Excuted;

	public static Boat Boat { get; private set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_BoatManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public void TrySpawnBoat(Boat boat, Vector3 pos, Quaternion rot)
	{
		if (!Boat)
		{
			Boat boat2 = Object.Instantiate(boat, pos, rot);
			Spawn(boat2.gameObject);
			Boat = boat2;
		}
	}

	public void TryMoveBoat(Vector3 pos, Quaternion rot)
	{
		if ((bool)Boat)
		{
			Boat.HiddenPhysicsRig.linearVelocity = Vector3.zero;
			Boat.HiddenPhysicsRig.angularVelocity = Vector3.zero;
			Boat.HiddenPhysicsRig.MovePosition(pos);
			Boat.HiddenPhysicsRig.MoveRotation(rot);
		}
	}

	public static void SetBoat(Collider[] cols, Boat boat)
	{
		foreach (Collider key in cols)
		{
			if (!ColToBoat.ContainsKey(key))
			{
				ColToBoat.Add(key, boat);
			}
		}
		Boat = boat;
	}

	public static void RemoveBoat()
	{
		ColToBoat.Clear();
		Boat = null;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyBoatManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyBoatManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateBoatManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateBoatManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void Awake_UserLogic_BoatManager_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
	}
}
