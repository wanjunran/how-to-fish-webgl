using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class OnlineIslandManager : NetworkBehaviour
{
	public static OnlineIslandManager Instance;

	public readonly SyncVar<byte> _curIsland = new SyncVar<byte>();

	public readonly SyncVar<byte> _maxIslandUnlocked = new SyncVar<byte>(2);

	private bool NetworkInitialize___EarlyOnlineIslandManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateOnlineIslandManagerAssembly_002DCSharp_002Edll_Excuted;

	public static bool TeleportPlayers { get; private set; }

	public static float TimeWhenSwappingIsland { get; private set; }

	public static byte MaxIslandUnlocked => Instance._maxIslandUnlocked.Value;

	public static byte CurIsland => Instance._curIsland.Value;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_OnlineIslandManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		if (SaveManager.CurServerSave != null)
		{
			_maxIslandUnlocked.Value = (ClientSettings.CheatsEnabled ? byte.MaxValue : SaveManager.CurServerSave.MaxIsland);
			_curIsland.Value = SaveManager.CurServerSave.SpawnedIsland;
		}
		else if (ClientSettings.CheatsEnabled)
		{
			_maxIslandUnlocked.Value = byte.MaxValue;
			_curIsland.Value = 5;
		}
	}

	public override void OnStartClient()
	{
		if (!base.IsServerInitialized || _curIsland.Value == 0)
		{
			OnIslandChange(_curIsland.Value, _curIsland.Value, asServer: false);
		}
	}

	public override void OnStopClient()
	{
		IslandManager.UnloadIslands();
	}

	private void Update()
	{
		if (base.IsServerInitialized && ClientSettings.CheatsEnabled && (bool)Player.LocalPlayer && !Player.LocalPlayer.BlockInputs && !IslandManager.IsLoading && Input.GetKeyDown(KeyCode.O))
		{
			ToggleTeleportPlayers(to: true);
			NextIslandInBuild();
		}
	}

	public static void ToggleTeleportPlayers(bool to)
	{
		if ((bool)Instance && Instance.IsServerInitialized)
		{
			TeleportPlayers = to;
		}
	}

	public void UnlockIsland(byte islandIndex)
	{
		if (islandIndex > _maxIslandUnlocked.Value)
		{
			_maxIslandUnlocked.Value = islandIndex;
		}
	}

	public static void TpToNextIsland(bool prev = false)
	{
		if ((bool)Instance)
		{
			ToggleTeleportPlayers(to: true);
			Instance.NextIslandInBuild(prev);
		}
	}

	public static void TpToSpecificIsland(byte to)
	{
		if ((bool)Instance)
		{
			ToggleTeleportPlayers(to: true);
			Instance.SpawnIsland(to);
		}
	}

	private void NextIslandInBuild(bool backwards = false)
	{
		byte value = _curIsland.Value;
		value = (backwards ? ((byte)(value - 1)) : ((byte)(value + 1)));
		if (value >= IslandManager.TotalIslands - 1)
		{
			value = (byte)(backwards ? ((byte)(IslandManager.TotalIslands - 2)) : 0);
		}
		SpawnIsland(value);
	}

	public void SpawnIsland(byte islandIndex)
	{
		if (base.IsServerInitialized)
		{
			_curIsland.Value = islandIndex;
		}
	}

	private void OnIslandChange(byte prev, byte next, bool asServer)
	{
		if (!asServer)
		{
			TimeWhenSwappingIsland = Time.time;
			IslandManager.LoadIsland(next);
			IslandManager.OnIslandChange(next);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyOnlineIslandManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyOnlineIslandManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_maxIslandUnlocked.InitializeEarly(this, 1u, isSyncObject: false);
			_curIsland.InitializeEarly(this, 0u, isSyncObject: false);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateOnlineIslandManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateOnlineIslandManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_maxIslandUnlocked.InitializeLate();
			_curIsland.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void Awake_UserLogic_OnlineIslandManager_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
		_curIsland.OnChange += OnIslandChange;
	}
}
