using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : NetworkBehaviour
{
	public static Player LocalPlayer;

	[SerializeField]
	private PlayerMovement _movement;

	[SerializeField]
	private PlayerInventory _inventory;

	[SerializeField]
	private PlayerHolding _holding;

	[SerializeField]
	private PlayerUI _ui;

	[FormerlySerializedAs("_quest")]
	[SerializeField]
	private PlayerTutorial tutorial;

	[SerializeField]
	private PlayerCamera _camera;

	[SerializeField]
	private OtherPlayer _other;

	[SerializeField]
	private PlayerToolMovement _toolMovement;

	[SerializeField]
	private PlayerPunching _playerPunching;

	[SerializeField]
	private PlayerHands _playerHands;

	[SerializeField]
	private PlayerVitals _playerVitals;

	[SerializeField]
	private PlayerScreenShake _playerScreenShake;

	[SerializeField]
	private PlayerArms _playerArms;

	[SerializeField]
	private PlayerSkills _playerSkills;

	[SerializeField]
	private PlayerBody _playerBody;

	[SerializeField]
	private PlayerLegs _playerLegs;

	[SerializeField]
	private PlayerMouth _playerMouth;

	[SerializeField]
	private PlayerSkin _playerSkin;

	[SerializeField]
	private PlayerDying _playerDying;

	[SerializeField]
	private PlayerEating _playerEating;

	[SerializeField]
	private PlayerColDetector _playerColDetector;

	[SerializeField]
	private PlayerEffects _effects;

	[SerializeField]
	private PlayerThinking _thinking;

	[SerializeField]
	private PlayerAimAssist _aimAssist;

	[SerializeField]
	private PlayerUnderwater _underwater;

	[SerializeField]
	private Transform _transform;

	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private List<GameObject> _localObjects;

	[SerializeField]
	private List<GameObject> _otherObjects;

	[SerializeField]
	private PlayerKillScore _playerKillScore;

	[SerializeField]
	private PlayerDeathCam _playerDeathCam;

	[Space]
	[SerializeField]
	private MonoBehaviour[] _disableWhenLocal;

	[SerializeField]
	private MonoBehaviour[] _disableWhenOther;

	public readonly SyncVar<ulong> _steamID = new SyncVar<ulong>();

	public readonly SyncVar<bool> _isCrouching = new SyncVar<bool>();

	public readonly SyncVar<bool> _isAfk = new SyncVar<bool>();

	public readonly SyncVar<bool> _isBean = new SyncVar<bool>();

	private bool _sendTeleport;

	private bool NetworkInitialize___EarlyPlayerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerAssembly_002DCSharp_002Edll_Excuted;

	public PlayerMovement Movement => _movement;

	public PlayerInventory Inventory => _inventory;

	public PlayerHolding Holding => _holding;

	public PlayerCamera Camera => _camera;

	public OtherPlayer Other => _other;

	public PlayerToolMovement ToolMovement => _toolMovement;

	public PlayerKillScore KillScore => _playerKillScore;

	public PlayerDeathCam DeathCam => _playerDeathCam;

	public PlayerPunching Punching => _playerPunching;

	public PlayerHands Hands => _playerHands;

	public PlayerVitals Vitals => _playerVitals;

	public PlayerScreenShake ScreenShake => _playerScreenShake;

	public PlayerArms Arms => _playerArms;

	public PlayerBody Body => _playerBody;

	public PlayerLegs Legs => _playerLegs;

	public PlayerMouth Mouth => _playerMouth;

	public PlayerSkin Skin => _playerSkin;

	public PlayerDying Dying => _playerDying;

	public PlayerEating Eating => _playerEating;

	public PlayerColDetector ColDetector => _playerColDetector;

	public PlayerEffects Effects => _effects;

	public PlayerAimAssist AimAssist => _aimAssist;

	public PlayerUnderwater Underwater => _underwater;

	public Rigidbody Rigidbody => _rigidbody;

	public bool BlockInputs
	{
		get
		{
			if (!Dying.IsDead && !PauseManager.IsPaused && !ChatManager.IsTyping && !PlayerThinking.IsThinking)
			{
				return SelfDrivingBoat.IsDriving;
			}
			return true;
		}
	}

	public Transform CamObject { get; private set; }

	public Transform Transform { get; private set; }

	public Quaternion CurPlayerRot { get; private set; }

	public Camera CurCam { get; private set; }

	public static bool LocalPlayerEnabled { get; private set; }

	public string SteamName { get; private set; }

	public bool IsCrouching => _isCrouching.Value;

	public bool IsAfk => _isAfk.Value;

	public bool AfkFromPause { get; private set; } = true;

	public ulong SteamID => _steamID.Value;

	public bool IsBean => _isBean.Value;

	public bool ServerHasFinishedTutorial { get; private set; }

	public static bool IsInside { get; private set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Player_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartClient()
	{
		PlayerManager.AddPlayer(this);
		InitializePlayer();
		if (base.Owner.IsLocalClient)
		{
			base.TimeManager.OnTick += TickUpdate;
		}
		if (base.IsServerInitialized)
		{
			if (SaveManager.PlayerHasFinishedTutorial(this))
			{
				SetFinishedTutorial();
				SkipTutorial(base.Owner);
			}
			if (SaveManager.PlayerHasJoinedPreviously(this))
			{
				SkipIntro(base.Owner);
			}
		}
	}

	public override void OnStopClient()
	{
		PlayerManager.RemovePlayer(this);
		if (base.Owner.IsLocalClient)
		{
			LocalPlayerEnabled = false;
			base.TimeManager.OnTick -= TickUpdate;
		}
	}

	public void SetCurCam(Camera to)
	{
		CurCam = to;
		GameInfo.SetCam(to);
	}

	public void SetSteamID(ulong steamID)
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			_steamID.Value = steamID;
		}
	}

	public void SetIsCrouching(bool isCrouching)
	{
		_isCrouching.Value = isCrouching;
	}

	[TargetRpc]
	public void RPCTeleport(NetworkConnection netCon, Vector3 pos, float rot)
	{
		RpcWriter___RPCTeleport___2734710480(netCon, pos, rot);
	}

	public void LocalTeleport(Vector3 pos, float rot = 0f, bool instant = false)
	{
		_movement.Teleport(pos, instant);
		_camera.SetRot(rot);
		CurPlayerRot = Quaternion.Euler(0f, CamObject.eulerAngles.y, 0f);
		_sendTeleport = true;
	}

	private void LateUpdate()
	{
		CurPlayerRot = Quaternion.Euler(0f, CamObject.eulerAngles.y, 0f);
	}

	private void TickUpdate()
	{
		SendPosRot();
		CheckIsInside();
	}

	private void CheckIsInside()
	{
		IsInside = Physics.Raycast(Transform.position, Vector3.up, 5f, GameInfo.LevelLayer);
	}

	private void SendPosRot()
	{
		if (_sendTeleport)
		{
			_sendTeleport = false;
			if (_movement.OnBoat && (bool)BoatManager.Boat)
			{
				Server.Instance.UpdatePlayerPosRot(this, BoatManager.Boat.VisualBoat.InverseTransformPoint(_transform.position), new Vector2(_camera.CamTransform.localEulerAngles.x, _camera.CamTransform.localEulerAngles.y), onBoat: true, teleport: true);
			}
			else
			{
				Server.Instance.UpdatePlayerPosRot(this, _transform.position, new Vector2(_camera.CamTransform.localEulerAngles.x, _camera.CamTransform.localEulerAngles.y), onBoat: false, teleport: true);
			}
		}
		else if (_movement.OnBoat && (bool)BoatManager.Boat)
		{
			Server.Instance.UpdatePlayerPosRot(this, BoatManager.Boat.VisualBoat.InverseTransformPoint(_transform.position), new Vector2(_camera.CamTransform.localEulerAngles.x, _camera.CamTransform.localEulerAngles.y), onBoat: true);
		}
		else
		{
			Server.Instance.UpdatePlayerPosRot(this, _transform.position, new Vector2(_camera.CamTransform.localEulerAngles.x, _camera.CamTransform.localEulerAngles.y));
		}
	}

	private void InitializePlayer()
	{
		base.transform.SetParent(Client.Clients[base.Owner].transform);
		MonoBehaviour[] disableWhenOther;
		if (base.Owner.IsLocalClient)
		{
			LocalPlayer = this;
			foreach (GameObject localObject in _localObjects)
			{
				localObject.SetActive(value: true);
			}
			foreach (GameObject otherObject in _otherObjects)
			{
				Object.Destroy(otherObject);
			}
			_rigidbody.position = SpawnManager.PlayerSpawnPos;
			disableWhenOther = _disableWhenOther;
			for (int i = 0; i < disableWhenOther.Length; i++)
			{
				disableWhenOther[i].enabled = true;
			}
			_ui.InitializeLocal(this);
			tutorial.InitializeLocal(this);
			_thinking.InitializeLocal(this);
			_playerMouth.InitializeLocal();
			_playerSkills.InitializeLocal();
			Transform = _transform;
			CamObject = _camera.CamTransform;
			SetCurCam(_camera.Cam);
			SteamName = SteamFriends.GetPersonaName();
			Mouth.SetVoiceTarget(CamObject);
			return;
		}
		foreach (GameObject otherObject2 in _otherObjects)
		{
			otherObject2.SetActive(value: true);
		}
		foreach (GameObject localObject2 in _localObjects)
		{
			Object.Destroy(localObject2);
		}
		Other.SetPlayerName(_steamID.Value);
		SteamName = SteamFriends.GetFriendPersonaName(new CSteamID(_steamID.Value));
		Transform = _other.Transform;
		CamObject = _other.CamProxy;
		disableWhenOther = _disableWhenLocal;
		for (int i = 0; i < disableWhenOther.Length; i++)
		{
			disableWhenOther[i].enabled = true;
		}
		Mouth.SetVoiceTarget(CamObject);
		Body.ToggleOldModel(_isBean.Value);
	}

	private void OnSteamIDChange(ulong prev, ulong next, bool asServer)
	{
		if (!asServer && !base.Owner.IsLocalClient)
		{
			Other.SetPlayerName(next);
			if (!base.Owner.IsLocalClient)
			{
				SteamName = SteamFriends.GetFriendPersonaName(new CSteamID(next));
			}
			else
			{
				SteamName = SteamFriends.GetPersonaName();
			}
		}
	}

	public void SetIsAfk(bool isAfk, bool fromPause)
	{
		if (!(!AfkFromPause & fromPause))
		{
			AfkFromPause = fromPause;
			_isAfk.Value = isAfk;
		}
	}

	public static void ToggleLocalPlayer(bool to)
	{
		LocalPlayerEnabled = to;
		if ((bool)LocalPlayer)
		{
			LocalPlayer.Transform.gameObject.SetActive(to);
		}
	}

	public void SetFinishedTutorial()
	{
		ServerHasFinishedTutorial = true;
	}

	[TargetRpc]
	private void SkipTutorial(NetworkConnection netCon)
	{
		RpcWriter___SkipTutorial___328543758(netCon);
	}

	[TargetRpc]
	private void SkipIntro(NetworkConnection netCon)
	{
		RpcWriter___SkipIntro___328543758(netCon);
	}

	public void SetIsBean(bool isBean)
	{
		_isBean.Value = isBean;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_isBean.InitializeEarly(this, 3u, isSyncObject: false);
			_isAfk.InitializeEarly(this, 2u, isSyncObject: false);
			_isCrouching.InitializeEarly(this, 1u, isSyncObject: false);
			_steamID.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterTargetRpc(0u, RpcReader___RPCTeleport___2734710480);
			RegisterTargetRpc(1u, RpcReader___SkipTutorial___328543758);
			RegisterTargetRpc(2u, RpcReader___SkipIntro___328543758);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_isBean.InitializeLate();
			_isAfk.InitializeLate();
			_isCrouching.InitializeLate();
			_steamID.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___RPCTeleport___2734710480(NetworkConnection netCon, Vector3 pos, float rot)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteSingle(rot);
		SendTargetRpc(0u, pooledWriter, channel, DataOrderType.Default, netCon, excludeServer: false);
		pooledWriter.Store();
	}

	public void RpcLogic___RPCTeleport___2734710480(NetworkConnection P_0, Vector3 P_1, float P_2)
	{
		LocalTeleport(P_1, P_2);
	}

	private void RpcReader___RPCTeleport___2734710480(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		float num = PooledReader0.ReadSingle();
		if (base.IsClientInitialized)
		{
			RpcLogic___RPCTeleport___2734710480(base.LocalConnection, vector, num);
		}
	}

	private void RpcWriter___SkipTutorial___328543758(NetworkConnection netCon)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendTargetRpc(1u, pooledWriter, channel, DataOrderType.Default, netCon, excludeServer: false);
		pooledWriter.Store();
	}

	private void RpcLogic___SkipTutorial___328543758(NetworkConnection P_0)
	{
		if ((bool)PlayerTutorial.Instance)
		{
			PlayerTutorial.Instance.SkipTutorial();
		}
	}

	private void RpcReader___SkipTutorial___328543758(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___SkipTutorial___328543758(base.LocalConnection);
		}
	}

	private void RpcWriter___SkipIntro___328543758(NetworkConnection netCon)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendTargetRpc(2u, pooledWriter, channel, DataOrderType.Default, netCon, excludeServer: false);
		pooledWriter.Store();
	}

	private void RpcLogic___SkipIntro___328543758(NetworkConnection P_0)
	{
		if (!ClientSettings.CheatsEnabled)
		{
			MainMenuManager.InstantCrash();
		}
	}

	private void RpcReader___SkipIntro___328543758(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___SkipIntro___328543758(base.LocalConnection);
		}
	}

	private void Awake_UserLogic_Player_Assembly_002DCSharp_002Edll()
	{
		_steamID.OnChange += OnSteamIDChange;
		foreach (GameObject localObject in _localObjects)
		{
			localObject.SetActive(value: false);
		}
		foreach (GameObject otherObject in _otherObjects)
		{
			otherObject.SetActive(value: false);
		}
		CamObject = _camera.CamTransform;
		Transform = _transform;
		MonoBehaviour[] disableWhenLocal = _disableWhenLocal;
		for (int i = 0; i < disableWhenLocal.Length; i++)
		{
			disableWhenLocal[i].enabled = false;
		}
		disableWhenLocal = _disableWhenOther;
		for (int i = 0; i < disableWhenLocal.Length; i++)
		{
			disableWhenLocal[i].enabled = false;
		}
	}
}
