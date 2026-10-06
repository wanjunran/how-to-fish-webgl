using FishNet.Object;
using FishNet.Object.Synchronizing;

public class ServerSettings : NetworkBehaviour
{
	public static ServerSettings Instance;

	public readonly SyncVar<bool> _useFriendlyFire = new SyncVar<bool>(initialValue: true);

	public readonly SyncVar<bool> _useOneShot = new SyncVar<bool>();

	public readonly SyncVar<byte> _difficulty = new SyncVar<byte>(1);

	private bool NetworkInitialize___EarlyServerSettingsAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateServerSettingsAssembly_002DCSharp_002Edll_Excuted;

	public static bool UseFriendlyFire => Instance._useFriendlyFire.Value;

	public static bool OneShotEnabled => Instance._useOneShot.Value;

	public static Difficulty Difficulty { get; private set; } = Difficulty.Default;

	public static float HealthMultiplier { get; private set; } = 1f;

	public static float DamageMultiplier { get; private set; } = 1f;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ServerSettings_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		_difficulty.Value = (byte)((SaveManager.CurServerSave == null) ? 1 : ((byte)SaveManager.CurServerSave.Difficulty));
	}

	public override void OnStartClient()
	{
		if (!base.IsServerInitialized)
		{
			OnDifficultyChange(_difficulty.Value, _difficulty.Value, asServer: false);
		}
	}

	public void SetDifficulty(Difficulty difficulty)
	{
		if (base.IsServerInitialized)
		{
			_difficulty.Value = (byte)difficulty;
		}
	}

	public void ToggleFriendlyFire(bool to)
	{
		if (base.IsServerInitialized)
		{
			_useFriendlyFire.Value = to;
		}
	}

	public void ToggleOneShot()
	{
		if (base.IsServerInitialized)
		{
			_useOneShot.Value = !_useOneShot.Value;
		}
	}

	private void OnDifficultyChange(byte prev, byte next, bool asServer)
	{
		Difficulty = (Difficulty)next;
		switch (Difficulty)
		{
		case Difficulty.Default:
			HealthMultiplier = 1f;
			DamageMultiplier = 1f;
			break;
		case Difficulty.Easy:
			HealthMultiplier = 0.75f;
			DamageMultiplier = 0.5f;
			break;
		case Difficulty.Hard:
			HealthMultiplier = 1.25f;
			DamageMultiplier = 1.25f;
			break;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyServerSettingsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyServerSettingsAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_difficulty.InitializeEarly(this, 2u, isSyncObject: false);
			_useOneShot.InitializeEarly(this, 1u, isSyncObject: false);
			_useFriendlyFire.InitializeEarly(this, 0u, isSyncObject: false);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateServerSettingsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateServerSettingsAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_difficulty.InitializeLate();
			_useOneShot.InitializeLate();
			_useFriendlyFire.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void Awake_UserLogic_ServerSettings_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
		HealthMultiplier = 1f;
		DamageMultiplier = 1f;
		_difficulty.OnChange += OnDifficultyChange;
	}
}
