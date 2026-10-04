using System;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeadPlayer : Item
{
	[Serializable]
	private struct BodyPartSpawnRot
	{
		public Rigidbody BodyPart;

		public Vector3 SpawnRot;
	}

	[Header("Forces")]
	[SerializeField]
	private float _randomAngVelForcesAmount;

	[SerializeField]
	private float _maxVel;

	[SerializeField]
	private BodyPartSpawnRot[] _bodyPartSpawnRots;

	[Header("Mouth transforms")]
	[SerializeField]
	private Transform _mouthUpper;

	[SerializeField]
	private Transform _mouthLower;

	[Header("Skin")]
	[Space]
	[SerializeField]
	private SkinnedMeshRenderer _bodyRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _leftHand;

	[SerializeField]
	private SkinnedMeshRenderer _rightHand;

	[Space]
	[SerializeField]
	private SkinnedMeshRenderer _outfitRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _hatRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _accessoryRenderer;

	[SerializeField]
	private GameObject _oldCharacter;

	[SerializeField]
	private GameObject _oldHead;

	[Header("Resurrection")]
	[SerializeField]
	private float _resurrectSpeed;

	[SerializeField]
	private float _slapHandReturnSpeed = 1f;

	[SerializeField]
	private Animation _slapHandAnim;

	[SerializeField]
	private AudioSequence _slapSounds;

	public readonly SyncVar<Player> _player = new SyncVar<Player>();

	private bool _hasAppliedDeathForce;

	private Vector3 _deathForce;

	private float _resurrectPercent;

	private float _receivedResurrectPercent;

	private bool _isResurrecting;

	private bool _isReturningSlapHand;

	private float _slapHandReturnPercent;

	private Vector3 _slapHandReturnStartPos;

	private Quaternion _slapHandReturnStartRot;

	private bool NetworkInitialize___EarlyDeadPlayerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateDeadPlayerAssembly_002DCSharp_002Edll_Excuted;

	public Transform MouthUpper => _mouthUpper;

	public Transform MouthLower => _mouthLower;

	public Player Player => _player.Value;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_DeadPlayer_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		if (_player.Value.Owner.IsLocalClient)
		{
			InitializeBodyPartsRot();
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		base.TimeManager.OnTick += TickUpdate;
	}

	public override void OnStopClient()
	{
		base.TimeManager.OnTick -= TickUpdate;
		base.OnStopClient();
	}

	protected override void Update()
	{
		base.Update();
		if (base.IsServerInitialized && !base.IsDestroying && (!_player.Value || _player.Value.IsDeinitializing || _player.Value.Vitals.Health > 0))
		{
			DestroyItem(0);
		}
		UpdateResurrection();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.IsServerInitialized && !_hasAppliedDeathForce && !_rig.isKinematic)
		{
			AddDeathForce();
		}
	}

	private void TickUpdate()
	{
		if ((bool)_holder && _holder.Owner.IsLocalClient)
		{
			Server.Instance.UpdateDeadPlayerResurrectPercent(this, _resurrectPercent);
		}
	}

	public void SetPlayer(Player player, Vector3 force)
	{
		_player.Value = player;
		Initialize(force);
	}

	public void SetVoiceChatParent(bool toThis)
	{
		if ((bool)_player.Value)
		{
			Transform voiceTarget = (toThis ? base.transform : _player.Value.CamObject);
			_player.Value.Mouth.SetVoiceTarget(voiceTarget);
		}
	}

	public override void PrimaryInput(InputAction.CallbackContext context)
	{
		if ((bool)_player.Value && (bool)_holder && !_holder.Dying.IsDead && !_isResurrecting)
		{
			_isResurrecting = true;
			StartSlapAnimation();
		}
	}

	public override void PrimaryInputCancel(InputAction.CallbackContext context)
	{
		CancelResurrection();
	}

	public override void OnDrop()
	{
		base.OnDrop();
		CancelResurrection();
	}

	private void CancelResurrection()
	{
		_isResurrecting = false;
		_resurrectPercent = 0f;
		StartSlapHandReturn();
	}

	[ObserversRpc]
	public void ReceiveResurrectPercent(float resurrectPercent, Channel channel = Channel.Unreliable)
	{
		RpcWriter___ReceiveResurrectPercent___2090002017(resurrectPercent, channel);
	}

	private void StartSlapAnimation()
	{
		_isReturningSlapHand = false;
		AudioSequenceManager.CancelAllActiveSequencesFromOwner(base.gameObject);
		if ((bool)_slapHandAnim)
		{
			_slapHandAnim.Play("DeadPlayerSlap");
			_slapHandAnim["DeadPlayerSlap"].speed = 0f;
		}
		if (_slapSounds != null)
		{
			AudioSequenceManager.PlayPlayerSequence(_slapSounds, _holder, base.gameObject);
		}
	}

	private void StartSlapHandReturn()
	{
		AudioSequenceManager.CancelAllActiveSequencesFromOwner(base.gameObject);
		if ((bool)_slapHandAnim)
		{
			Transform transform = _slapHandAnim.transform;
			_slapHandReturnStartPos = transform.localPosition;
			_slapHandReturnStartRot = transform.localRotation;
			_slapHandReturnPercent = 0f;
			_isReturningSlapHand = true;
			_slapHandAnim.Stop();
		}
	}

	private void UpdateResurrection()
	{
		if (!_player.Value)
		{
			return;
		}
		if (_isResurrecting && (!_holder || _holder.Dying.IsDead))
		{
			CancelResurrection();
		}
		else if (_isReturningSlapHand)
		{
			UpdateSlapHandReturn();
		}
		else if ((bool)_holder && !_holder.Owner.IsLocalClient)
		{
			UpdateObservedResurrection();
		}
		else if (_isResurrecting)
		{
			_resurrectPercent = Mathf.MoveTowards(_resurrectPercent, 1f, Time.deltaTime * _resurrectSpeed);
			UpdateSlapHandAnimation();
			if (!(_resurrectPercent < 1f))
			{
				PlayerUI.HideSpecificItemInfo();
				_isResurrecting = false;
				Server.Instance.ResurrectPlayer(_player.Value, this);
			}
		}
	}

	private void UpdateObservedResurrection()
	{
		float t = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
		_resurrectPercent = Mathf.Lerp(_resurrectPercent, _receivedResurrectPercent, t);
		UpdateSlapHandAnimation();
	}

	private void UpdateSlapHandAnimation()
	{
		if ((bool)_slapHandAnim)
		{
			_slapHandAnim["DeadPlayerSlap"].time = _resurrectPercent;
			_slapHandAnim.Sample();
		}
	}

	private void UpdateSlapHandReturn()
	{
		if (!_slapHandAnim)
		{
			_isReturningSlapHand = false;
			return;
		}
		_slapHandReturnPercent = Mathf.MoveTowards(_slapHandReturnPercent, 1f, Time.deltaTime * _slapHandReturnSpeed);
		Transform obj = _slapHandAnim.transform;
		obj.localPosition = Vector3.Lerp(_slapHandReturnStartPos, Vector3.zero, _slapHandReturnPercent);
		obj.localRotation = Quaternion.Lerp(_slapHandReturnStartRot, Quaternion.identity, _slapHandReturnPercent);
		if (_slapHandReturnPercent >= 1f)
		{
			_isReturningSlapHand = false;
		}
	}

	public override void LocalHit(Transform hitTransform, Vector3 point, Vector3 dir, Player player, int damage, bool rangedHit, Vector3 force = default(Vector3), bool fromNpc = false)
	{
		base.LocalHit(hitTransform, point, dir, player, damage, rangedHit, force);
		DazedUtils.PlayDeadPlayerHitEffects(point, dir, damage, player);
	}

	private void ToggleOldModel()
	{
		_accessoryRenderer.gameObject.SetActive(value: false);
		_bodyRenderer.gameObject.SetActive(value: false);
		_hatRenderer.gameObject.SetActive(value: false);
		_outfitRenderer.gameObject.SetActive(value: false);
		_oldCharacter.SetActive(value: true);
		_oldHead.SetActive(value: true);
	}

	private void Initialize(Vector3 force = default(Vector3))
	{
		_player.Value.Dying.SetDeadPlayer(this);
		_player.Value.Skin.SendSkinToDeadPlayer(this);
		if (_player.Value.IsBean)
		{
			ToggleOldModel();
		}
		SetVoiceChatParent(toThis: true);
		SetBodyPartsPosRot();
		InitializeCollider();
		_deathForce = force;
	}

	public override string GetName()
	{
		if ((bool)_player.Value)
		{
			return _player.Value.SteamName;
		}
		return base.GetName();
	}

	private void InitializeCollider()
	{
		_playerWhoDropped = _player.Value;
		_disablePlayerColOnGround = true;
		UpdateLayer();
	}

	private void AddDeathForce()
	{
		_hasAppliedDeathForce = true;
		Vector3 linearVelocity = (_player.Value.Owner.IsLocalClient ? _player.Value.Movement.Velocity : _player.Value.Other.Velocity);
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		foreach (ItemExtraRigidbody obj in extraRigs)
		{
			Vector3 angularVelocity = UnityEngine.Random.insideUnitSphere * _randomAngVelForcesAmount;
			obj.Rig.linearVelocity = linearVelocity;
			obj.Rig.angularVelocity = angularVelocity;
		}
		_rig.linearVelocity += _deathForce;
	}

	private void SetBodyPartsPosRot()
	{
		if (!_player.Value.Owner.IsLocalClient)
		{
			Quaternion[] array = new Quaternion[_player.Value.Dying.BodyParts.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _player.Value.Dying.BodyParts[i].localRotation;
			}
			_rigSync.TeleportToPosRot(_player.Value.Transform.position, _player.Value.Transform.rotation, null, array);
		}
	}

	private void InitializeBodyPartsRot()
	{
		BodyPartSpawnRot[] bodyPartSpawnRots = _bodyPartSpawnRots;
		for (int i = 0; i < bodyPartSpawnRots.Length; i++)
		{
			BodyPartSpawnRot bodyPartSpawnRot = bodyPartSpawnRots[i];
			bodyPartSpawnRot.BodyPart.rotation = Quaternion.Euler(bodyPartSpawnRot.SpawnRot);
		}
	}

	public void SetSkin(Vector3 skinColor, Vector3 hatColor, Vector3 hatColor2, Vector3 hatColor3, Vector3 outfitColor, Vector3 outfitColor2, Vector3 outfitColor3, Vector3 accessoryColor, Vector3 accessoryColor2, Vector3 accessoryColor3, byte hatMeshIndex, byte outfitMeshIndex, byte accessoryMeshIndex)
	{
		if ((bool)_bodyRenderer)
		{
			ShaderManager.SetPlayerColors(_bodyRenderer, skinColor);
		}
		if ((bool)_leftHand)
		{
			ShaderManager.SetPlayerColors(_leftHand, skinColor);
		}
		if ((bool)_rightHand)
		{
			ShaderManager.SetPlayerColors(_rightHand, skinColor);
		}
		if ((bool)_hatRenderer)
		{
			_hatRenderer.sharedMesh = SkinManager.GetHat(hatMeshIndex);
			ShaderManager.SetPlayerColors(_hatRenderer, skinColor, hatColor, hatColor2, hatColor3);
		}
		if ((bool)_accessoryRenderer)
		{
			_accessoryRenderer.sharedMesh = SkinManager.GetAccessory(accessoryMeshIndex);
			ShaderManager.SetPlayerColors(_accessoryRenderer, skinColor, accessoryColor, accessoryColor2, accessoryColor3);
		}
		if ((bool)_outfitRenderer)
		{
			_outfitRenderer.sharedMesh = SkinManager.GetOutfit(outfitMeshIndex);
			ShaderManager.SetPlayerColors(_outfitRenderer, skinColor, outfitColor, outfitColor2, outfitColor3);
		}
	}

	private void OnPlayerChange(Player prev, Player next, bool asServer)
	{
		if (!base.IsServerInitialized)
		{
			Initialize();
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyDeadPlayerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyDeadPlayerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_player.InitializeEarly(this, 9u, isSyncObject: false);
			RegisterObserversRpc(2u, RpcReader___ReceiveResurrectPercent___2090002017);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateDeadPlayerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateDeadPlayerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_player.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ReceiveResurrectPercent___2090002017(float resurrectPercent, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteSingle(resurrectPercent);
		SendObserversRpc(2u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ReceiveResurrectPercent___2090002017(float P_0, Channel P_1)
	{
		if (!_holder || !_holder.Owner.IsLocalClient)
		{
			if (P_0 < _receivedResurrectPercent)
			{
				_resurrectPercent = 0f;
				StartSlapHandReturn();
			}
			else if (P_0 > 0f && _receivedResurrectPercent <= 0f)
			{
				StartSlapAnimation();
			}
			_receivedResurrectPercent = P_0;
		}
	}

	private void RpcReader___ReceiveResurrectPercent___2090002017(PooledReader PooledReader0, Channel channel)
	{
		float num = PooledReader0.ReadSingle();
		if (base.IsClientInitialized)
		{
			RpcLogic___ReceiveResurrectPercent___2090002017(num, channel);
		}
	}

	protected virtual void Awake_UserLogic_DeadPlayer_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_deadPlayer = this;
		_player.OnChange += OnPlayerChange;
		_slapHandAnim["DeadPlayerSlap"].speed = 0f;
		ItemExtraRigidbody[] extraRigs = _extraRigs;
		for (int i = 0; i < extraRigs.Length; i++)
		{
			extraRigs[i].Rig.maxLinearVelocity = _maxVel;
		}
	}
}
