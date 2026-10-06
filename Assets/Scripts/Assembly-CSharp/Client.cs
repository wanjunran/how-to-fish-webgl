using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class Client : NetworkBehaviour
{
	public static readonly Dictionary<NetworkConnection, Client> Clients = new Dictionary<NetworkConnection, Client>();

	private bool NetworkInitialize___EarlyClientAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateClientAssembly_002DCSharp_002Edll_Excuted;

	public static Client LocalClient { get; private set; }

	public override void OnStartServer()
	{
		if (base.Owner.IsLocalClient)
		{
			InitializeServer();
		}
	}

	public override void OnStartClient()
	{
		Clients.Add(base.Owner, this);
		if (base.Owner.IsLocalClient)
		{
			InitializeLocal();
		}
	}

	public override void OnStopClient()
	{
		Clients.Remove(base.Owner);
	}

	private void InitializeLocal()
	{
		LocalClient = this;
		StartCoroutine(SendSpawnPlayer());
	}

	private IEnumerator SendSpawnPlayer()
	{
		yield return new WaitUntil(() => (bool)Server.Instance && !IslandManager.IsLoading && (bool)Island.CurIsland);
		yield return new WaitForFixedUpdate();
		Server.Instance.SpawnPlayer(LocalSkin.LocalSkinColor, LocalSkin.LocalOutfitColor, LocalSkin.LocalOutfitColor2, LocalSkin.LocalOutfitColor3, LocalSkin.LocalHatColor, LocalSkin.LocalHatColor2, LocalSkin.LocalHatColor3, LocalSkin.LocalAccessoryColor, LocalSkin.LocalAccessoryColor2, LocalSkin.LocalAccessoryColor3, LocalSkin.LocalHatIndex, LocalSkin.LocalOutfitIndex, LocalSkin.LocalAccessoryIndex, LocalSkin.IsBean);
	}

	private void InitializeServer()
	{
		Spawn(Object.Instantiate(GameInfo.ServerPrefab, base.transform), base.Owner);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyClientAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyClientAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateClientAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateClientAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}
}
