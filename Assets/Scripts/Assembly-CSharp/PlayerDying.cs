using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerDying : NetworkBehaviour
{
	[FormerlySerializedAs("_giveUpTime")]
	[SerializeField]
	private float _totalGiveUpTime = 1f;

	[SerializeField]
	private Player _player;

	[SerializeField]
	private Transform[] _bodyParts;

	private DeadPlayer _deadPlayer;

	private Vector3 _lastDeadPlayerPos;

	private float _lastDeadPlayerRot;

	private float _curGiveUpTime;

	private bool _isGivingUp;

	private bool _waitingForTpOnRespawn;

	private bool NetworkInitialize___EarlyPlayerDyingAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerDyingAssembly_002DCSharp_002Edll_Excuted;

	public bool IsDead { get; private set; }

	public DeadPlayer DeadPlayer => _deadPlayer;

	public Transform[] BodyParts => _bodyParts;

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			_player.DeathCam.DisableDeathCam();
			ResurrectEffect(respawned: true);
			BindInputs();
			PlayerManager.OnPlayerAmountChange += OnPlayerAmountChange;
		}
	}

	private void OnPlayerAmountChange()
	{
		if (PlayerManager.AlivePlayers.Count == 0)
		{
			_waitingForTpOnRespawn = true;
			_lastDeadPlayerPos = (_deadPlayer ? _deadPlayer.transform.position : _player.Transform.position);
			_lastDeadPlayerRot = (_deadPlayer ? _deadPlayer.transform.eulerAngles.y : 0f);
		}
	}

	public override void OnStopClient()
	{
		if (base.Owner.IsLocalClient)
		{
			UnbindInputs();
			PlayerManager.OnPlayerAmountChange -= OnPlayerAmountChange;
		}
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["PlayerLeftClick"].performed += MouseClick;
		input.actions["PlayerLeftClick"].canceled += MouseClickCanceled;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["PlayerLeftClick"].performed -= MouseClick;
			input.actions["PlayerLeftClick"].canceled -= MouseClickCanceled;
		}
	}

	private void Update()
	{
		if (!IsDead)
		{
			return;
		}
		if ((bool)_deadPlayer && !_waitingForTpOnRespawn)
		{
			_lastDeadPlayerPos = _deadPlayer.transform.position;
			_lastDeadPlayerRot = _deadPlayer.transform.eulerAngles.y;
		}
		if ((bool)BossManager.Boss)
		{
			if (_isGivingUp)
			{
				StopGivingUp();
			}
		}
		else if (_isGivingUp)
		{
			_curGiveUpTime += Time.deltaTime;
			if (_curGiveUpTime >= _totalGiveUpTime)
			{
				LocalRespawn();
			}
		}
		else if (_curGiveUpTime >= 0f)
		{
			_curGiveUpTime -= Time.deltaTime;
		}
	}

	private void MouseClick(InputAction.CallbackContext context)
	{
		if (!PauseManager.IsPaused && IsDead)
		{
			_isGivingUp = true;
			PlayerUI.StartGivingUp(_totalGiveUpTime - _curGiveUpTime);
		}
	}

	private void MouseClickCanceled(InputAction.CallbackContext context)
	{
		if (IsDead)
		{
			StopGivingUp();
		}
	}

	private void StopGivingUp()
	{
		_isGivingUp = false;
		PlayerUI.StopGivingUp(_curGiveUpTime);
	}

	public void ServerDie(Vector3 force)
	{
		DeadPlayer deadPlayer = Object.Instantiate(GameInfo.DeadPlayerPrefab, _player.Transform.position, _player.CamObject.rotation, Server.Instance.DynamicObjectsHolder);
		deadPlayer.SetPlayer(_player, force);
		Spawn(deadPlayer.gameObject);
		_deadPlayer = deadPlayer;
		if ((bool)_player.Holding.HeldItem)
		{
			_player.Holding.HeldItem.SetSyncedHolder(null);
			_player.Holding.HeldItem.RigidbodySync.StartSimulateLocal();
		}
	}

	public void LocalDie()
	{
		_player.DeathCam.EnableDeathCam();
		_player.Inventory.ApplySlot(-1);
		if ((bool)_player.Holding.HeldItem)
		{
			_player.Holding.HeldItem.Drop();
		}
		if ((bool)BoatManager.Boat && (bool)BoatManager.Boat.Driver && BoatManager.Boat.Driver.Owner.IsLocalClient)
		{
			Server.Instance.SetDriver(null);
		}
		DeathEffects();
	}

	public void DeathEffects()
	{
		IsDead = true;
		if (base.Owner.IsLocalClient)
		{
			PlayerUI.ToggleDeathUI(to: true);
		}
		_player.Transform.gameObject.SetActive(value: false);
		_player.Hands.ToggleHandMeshes(enable: false);
		PlayerManager.OnPlayerDied(_player);
	}

	public void LocalResurrect()
	{
		_player.DeathCam.DisableDeathCam();
		_curGiveUpTime = 0f;
		ResurrectEffect(_waitingForTpOnRespawn);
		if (_waitingForTpOnRespawn)
		{
			_waitingForTpOnRespawn = false;
			RadarUI.SetLastDeathPos(_lastDeadPlayerPos);
		}
	}

	public void ResurrectEffect(bool respawned)
	{
		IsDead = false;
		if (base.Owner.IsLocalClient)
		{
			PlayerUI.ToggleDeathUI(to: false);
		}
		_player.Mouth.SetVoiceTarget(_player.CamObject);
		_player.Hands.ToggleHandMeshes(enable: true);
		Vector3 pos = (respawned ? SpawnManager.PlayerSpawnPos : _lastDeadPlayerPos);
		float num = (respawned ? SpawnManager.PlayerSpawnRot : _lastDeadPlayerRot);
		if (!base.Owner.IsLocalClient)
		{
			_player.Other.Teleport(pos, Vector2.up * num);
			_player.Body.Reset();
			_player.Legs.Reset();
		}
		else
		{
			_player.LocalTeleport(pos, num, instant: true);
		}
		_player.Transform.gameObject.SetActive(!SelfDrivingBoat.IsDriving);
		PlayerManager.OnPlayerResurrected(_player);
	}

	public void SetDeadPlayer(DeadPlayer deadPlayer)
	{
		_deadPlayer = deadPlayer;
	}

	private void LocalRespawn()
	{
		StopGivingUp();
		_waitingForTpOnRespawn = true;
		Transform transform = (_deadPlayer ? _deadPlayer.transform : _player.Transform);
		Server.Instance.RespawnPlayer(_player, transform.position, transform.rotation);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerDyingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerDyingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerDyingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerDyingAssembly_002DCSharp_002Edll_Excuted = true;
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
