using System;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;

public class EndGameManager : NetworkBehaviour
{
	public readonly SyncVar<bool> _hasFinishedGame = new SyncVar<bool>();

	private bool NetworkInitialize___EarlyEndGameManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateEndGameManagerAssembly_002DCSharp_002Edll_Excuted;

	public static EndGameManager Instance { get; private set; }

	public bool HasFinishedGame { get; private set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_EndGameManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		if (SaveManager.CurServerSave != null)
		{
			_hasFinishedGame.Value = SaveManager.CurServerSave.HasFinishedGame;
		}
	}

	public override void OnStartClient()
	{
		HasFinishedGame = _hasFinishedGame.Value;
	}

	public void FinishGameInput()
	{
		if (!_hasFinishedGame.Value)
		{
			if (!base.IsServerInitialized)
			{
				Server.Instance.SendFinishGame();
			}
			else
			{
				ServerFinishGame();
			}
		}
	}

	public static void DisableEndGameStuff()
	{
		EndGameUI.ForceDisableEndGameStuff();
		EndGameEffects.OnHideCredits();
	}

	public void ServerFinishGame()
	{
		_hasFinishedGame.Value = true;
	}

	private void OnHasFinishedGame(bool prev, bool next, bool asServer)
	{
		if (!asServer && !HasFinishedGame && next)
		{
			HasFinishedGame = true;
			FinishGame();
		}
	}

	private void FinishGame()
	{
		AchievementManager.CheckFinishGameAchievement();
		EndGameUI.OnFinishGame();
		if (base.IsServerInitialized && TimeSpan.FromSeconds(SaveManager.GetTotalPlaytime()).Hours < 1)
		{
			ObserverSpeedrunAchievement();
			SpeedrunAchievement();
		}
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverSpeedrunAchievement()
	{
		RpcWriter___ObserverSpeedrunAchievement___2166136261();
	}

	private void SpeedrunAchievement()
	{
		AchievementManager.CheckSpeedrunAchievement();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyEndGameManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyEndGameManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_hasFinishedGame.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterObserversRpc(0u, RpcReader___ObserverSpeedrunAchievement___2166136261);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateEndGameManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateEndGameManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_hasFinishedGame.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverSpeedrunAchievement___2166136261()
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverSpeedrunAchievement___2166136261()
	{
		SpeedrunAchievement();
	}

	private void RpcReader___ObserverSpeedrunAchievement___2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverSpeedrunAchievement___2166136261();
		}
	}

	private void Awake_UserLogic_EndGameManager_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
		_hasFinishedGame.OnChange += OnHasFinishedGame;
	}
}
