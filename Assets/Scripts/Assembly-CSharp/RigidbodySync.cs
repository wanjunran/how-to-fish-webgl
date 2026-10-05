using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

public class RigidbodySync : NetworkBehaviour
{
	private enum HingeDirection
	{
		X = 0,
		Y = 1,
		Z = 2,
		All = 3
	}

	private struct Sample
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public float Time;
	}

	[SerializeField]
	private bool _startFrozen;

	[SerializeField]
	private Rigidbody[] _extraHinges;

	[SerializeField]
	private HingeDirection _hingeDirection;

	public readonly SyncVar<bool> _isFloating = new SyncVar<bool>(new SyncTypeSettings(Channel.Reliable));

	public readonly SyncVar<NetworkConnection> _syncedSimulator = new SyncVar<NetworkConnection>(new SyncTypeSettings(0f, Channel.Reliable));

	private readonly Queue<Sample> _velSamples = new Queue<Sample>();

	private uint _simulationChangedTick;

	private NetworkConnection _predictedSimulator;

	private Item _item;

	private Rigidbody _rig;

	private Vector3 _serverPos;

	private Quaternion _serverRot;

	private Vector3 _serverHeldToolPos;

	private Vector3 _lastBoatVel;

	private Vector3 _boatLastPos;

	private Quaternion _boatLastRot;

	private Quaternion _serverHeldToolRot;

	private Vector3 _overridePos;

	private Vector3 _fakeVelocity;

	private Vector3 _fakeAngularVelocity;

	private Quaternion[] _serverExtraHingeRots;

	private float[] _serverExtraHingeAngles;

	private bool _isInitialized;

	private bool _isFrozen;

	private bool _isPermaFrozen;

	private bool _hasInitializedServerPos;

	private bool _clientWantedStartSimulating;

	private bool _receivedOnBoat;

	private float _serverToLocalRatio;

	private float _timeOfServerResponse;

	private float _overridePercent;

	private bool NetworkInitialize___EarlyRigidbodySyncAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateRigidbodySyncAssembly_002DCSharp_002Edll_Excuted;

	public bool IsSimulatedLocal { get; private set; }

	public Rigidbody[] ExtraHinges => _extraHinges;

	public NetworkConnection SyncedSimulator => _syncedSimulator.Value;

	public Vector3 FakeVelocity => _fakeVelocity;

	public Vector3 FakeAngularVelocity => _fakeAngularVelocity;

	private bool IsHeldLocal
	{
		get
		{
			if ((bool)_item && (bool)_item.Holder)
			{
				return _item.Holder.Owner.IsLocalClient;
			}
			return false;
		}
	}

	public bool IsFloating => _isFloating.Value;

	public bool OnBoat { get; private set; }

	public bool IsStationary { get; private set; }

	public bool IsSendingUpdates { get; private set; } = true;

	public bool RecentlyStartedSimulation => base.TimeManager.Tick - _simulationChangedTick <= 50;

	public override void OnStartServer()
	{
		ServerInitialize();
	}

	public override void OnStartClient()
	{
		base.TimeManager.OnPostTick += TickUpdate;
		if (!base.IsServerInitialized)
		{
			ClientInitialize();
		}
	}

	public override void OnStopClient()
	{
		base.TimeManager.OnPostTick -= TickUpdate;
		RigidbodyManager.RigSyncs.Remove(this);
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_RigidbodySync_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	private void OnIsFloatingChange(bool prev, bool next, bool asServer)
	{
		SetKinematic(next);
		_simulationChangedTick = base.TimeManager.Tick;
	}

	private void OnCollisionStay(Collision col)
	{
		OnCollision(col);
	}

	private void OnCollisionEnter(Collision col)
	{
		OnCollision(col);
	}

	private void OnCollisionExit(Collision col)
	{
		OnCollision(col);
	}

	private void TickUpdate()
	{
		if (base.IsServerInitialized && !_isFrozen && (_syncedSimulator.Value == null || !_syncedSimulator.Value.IsActive))
		{
			StartSimulateLocal();
		}
		if (IsSimulatedLocal && !base.IsDeinitializing && !_isFrozen && !_item.IsInInventory && (!base.IsServerInitialized || !_isFloating.Value))
		{
			if ((bool)_item.Holder && (bool)_item.Tool)
			{
				SendHeldToolPosRotToObservers();
			}
			else
			{
				SendUpdateIfSimulating();
			}
		}
	}

	private void FixedUpdate()
	{
		if (!IsSimulatedLocal && !_rig.isKinematic)
		{
			ZeroVelocity();
		}
		IsStationary = DazedUtils.CheckIfStationary(_rig);
		if (IsStationary)
		{
			Rigidbody[] extraHinges = _extraHinges;
			for (int i = 0; i < extraHinges.Length; i++)
			{
				if (!DazedUtils.CheckIfStationary(extraHinges[i]))
				{
					IsStationary = false;
					break;
				}
			}
		}
		CalculateFakeVelocity();
	}

	private void Update()
	{
		if (_isInitialized && !IsSimulatedLocal && !_isFrozen && !_isFloating.Value)
		{
			if ((bool)_item.Holder && (bool)_item.Tool)
			{
				ApplyReceivedHeldToolPosRot();
			}
			else
			{
				ApplyReceivedPosRot();
			}
		}
	}

	private void OnSyncedSimulatorChange(NetworkConnection prev, NetworkConnection next, bool asServer)
	{
		if (asServer)
		{
			if ((bool)_item.SyncedHolder && _item.SyncedHolder.Owner != next && !_item.SyncedHolder.IsDeinitializing)
			{
				ServerSetSyncedSimulator(_item.SyncedHolder.Owner);
			}
		}
		else if (base.IsServerInitialized && (bool)_item.BirdHolder && next != InstanceFinder.ClientManager.Connection)
		{
			ServerSetSyncedSimulator(InstanceFinder.ClientManager.Connection);
		}
		else if (next == _predictedSimulator)
		{
			_predictedSimulator = null;
		}
		else
		{
			ToggleSimulation(next.IsLocalClient);
			_predictedSimulator = null;
		}
	}

	public void ServerSetIsFloating(bool to)
	{
		if (base.IsServerInitialized)
		{
			_isFloating.Value = to;
		}
	}

	public void ServerSetPosRot(NetworkConnection netCon, Vector3 pos, Quaternion rot, bool onBoat, float[] extraHingeAngles = null, Quaternion[] extraHingeRots = null)
	{
		if (!(netCon != InstanceFinder.ClientManager.Connection))
		{
			return;
		}
		if (extraHingeRots != null)
		{
			ObserverSetServerPosRot(netCon, pos, rot, onBoat, null, extraHingeRots);
		}
		else if (extraHingeAngles != null)
		{
			ObserverSetServerPosRot(netCon, pos, rot, onBoat, extraHingeAngles);
		}
		else
		{
			ObserverSetServerPosRot(netCon, pos, rot, onBoat);
		}
		if (_receivedOnBoat && !onBoat)
		{
			_boatLastPos = Vector3.zero;
			_boatLastRot = Quaternion.identity;
		}
		_serverPos = pos;
		_serverRot = rot;
		_receivedOnBoat = onBoat;
		if (extraHingeAngles != null)
		{
			for (int i = 0; i < extraHingeAngles.Length; i++)
			{
				_serverExtraHingeAngles[i] = extraHingeAngles[i];
			}
		}
		if (extraHingeRots != null)
		{
			for (int j = 0; j < extraHingeRots.Length; j++)
			{
				_serverExtraHingeRots[j] = extraHingeRots[j];
			}
		}
	}

	public void ServerSetHeldToolPosRot(Vector3 pos, Quaternion rot)
	{
		ObserverSetServerHeldToolPosRot(pos, rot);
		_serverHeldToolPos = pos;
		_serverHeldToolRot = rot;
	}

	public void Freeze(bool frozen, bool permanent = false)
	{
		if (!_isPermaFrozen || frozen)
		{
			if (!_isPermaFrozen)
			{
				_isPermaFrozen = permanent;
			}
			_isFrozen = frozen;
			if (frozen && (!base.IsServerInitialized || !IsSimulatedLocal))
			{
				_rig.Sleep();
			}
			if (_isPermaFrozen)
			{
				SetKinematic(kinematic: true);
			}
			else
			{
				SetKinematic(frozen);
			}
			if (!_rig.isKinematic)
			{
				ZeroVelocity();
			}
		}
	}

	public void TeleportToPosRot(Vector3 pos, Quaternion rot, float[] extraHingeAngles = null, Quaternion[] extraHingeRots = null)
	{
		_serverToLocalRatio = 1f;
		Transform receiveBoatVisual = GetReceiveBoatVisual();
		Vector3 position = (receiveBoatVisual ? receiveBoatVisual.TransformPoint(pos) : pos);
		Quaternion rotation = (receiveBoatVisual ? (receiveBoatVisual.rotation * rot) : rot);
		base.transform.position = position;
		base.transform.rotation = rotation;
		_rig.position = position;
		_rig.rotation = rotation;
		if (extraHingeAngles != null || extraHingeRots != null)
		{
			for (int i = 0; i < _extraHinges.Length; i++)
			{
				Vector3 localEulerAngles = _extraHinges[i].transform.localEulerAngles;
				Quaternion quaternion = Quaternion.identity;
				switch (_hingeDirection)
				{
				case HingeDirection.X:
					localEulerAngles.x = extraHingeAngles[i];
					break;
				case HingeDirection.Y:
					localEulerAngles.y = extraHingeAngles[i];
					break;
				case HingeDirection.Z:
					localEulerAngles.z = extraHingeAngles[i];
					break;
				case HingeDirection.All:
					quaternion = extraHingeRots[i];
					break;
				}
				Quaternion localRotation = ((_hingeDirection == HingeDirection.All) ? quaternion : Quaternion.Euler(localEulerAngles));
				_extraHinges[i].transform.localRotation = localRotation;
			}
		}
		_serverPos = pos;
		_serverRot = rot;
		if (extraHingeAngles != null)
		{
			for (int j = 0; j < extraHingeAngles.Length; j++)
			{
				_serverExtraHingeAngles[j] = extraHingeAngles[j];
			}
		}
		if (extraHingeRots != null)
		{
			for (int k = 0; k < extraHingeRots.Length; k++)
			{
				_serverExtraHingeRots[k] = extraHingeRots[k];
			}
		}
	}

	public void TeleportHeldToolPosRot(Vector3 pos, Quaternion rot)
	{
		_item.Tool.SwayTransform.localPosition = pos;
		_item.Tool.SwayTransform.localRotation = rot;
		_serverHeldToolPos = pos;
		_serverHeldToolRot = rot;
	}

	public void ObserverSetOverride(float percent, Vector3 pos = default(Vector3))
	{
		_overridePercent = percent;
		_overridePos = pos;
	}

	private Transform GetSendBoatVisual()
	{
		if (!OnBoat || !BoatManager.Boat)
		{
			return null;
		}
		return BoatManager.Boat.VisualBoat;
	}

	private Transform GetReceiveBoatVisual()
	{
		if (!_receivedOnBoat || !BoatManager.Boat)
		{
			return null;
		}
		return BoatManager.Boat.VisualBoat;
	}

	private void ApplyReceivedPosRot()
	{
		if (!_hasInitializedServerPos && !base.IsServerInitialized)
		{
			return;
		}
		Transform receiveBoatVisual = GetReceiveBoatVisual();
		Vector3 b = (receiveBoatVisual ? receiveBoatVisual.InverseTransformPoint(_overridePos) : _overridePos);
		Vector3 b2 = Vector3.Lerp(_serverPos, b, _overridePercent);
		Vector3 a = (receiveBoatVisual ? receiveBoatVisual.InverseTransformPoint(_rig.position) : _rig.position);
		Quaternion a2 = (receiveBoatVisual ? (Quaternion.Inverse(receiveBoatVisual.rotation) * _rig.rotation) : _rig.rotation);
		if (_serverToLocalRatio > 0f)
		{
			_serverToLocalRatio = GameInfo.SnapToServerCurve.Evaluate((Time.time - _timeOfServerResponse) / GameInfo.SnapToServerTime);
			if (_serverToLocalRatio <= 0f)
			{
				_serverToLocalRatio = 0f;
			}
		}
		else if (DazedUtils.AreVectorsEqual(a, b2, 0.001f) && DazedUtils.AreQuaternionsEqual(a2, _serverRot, 0.001f))
		{
			return;
		}
		float num = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
		num *= 1f - _serverToLocalRatio;
		if ((bool)receiveBoatVisual)
		{
			Vector3 position = Vector3.Lerp(_boatLastPos, b2, num);
			base.transform.position = receiveBoatVisual.TransformPoint(position);
			Quaternion quaternion = Quaternion.Slerp(_boatLastRot, _serverRot, num);
			base.transform.rotation = receiveBoatVisual.rotation * quaternion;
			_boatLastPos = receiveBoatVisual.InverseTransformPoint(base.transform.position);
			_boatLastRot = Quaternion.Inverse(receiveBoatVisual.rotation) * base.transform.rotation;
		}
		else
		{
			Vector3 position2 = Vector3.Lerp(a, b2, num);
			base.transform.position = position2;
			Quaternion rotation = Quaternion.Slerp(a2, _serverRot, num);
			base.transform.rotation = rotation;
		}
		for (int i = 0; i < _extraHinges.Length; i++)
		{
			Vector3 localEulerAngles = _extraHinges[i].transform.localEulerAngles;
			Quaternion quaternion2 = Quaternion.identity;
			switch (_hingeDirection)
			{
			case HingeDirection.X:
				localEulerAngles.x = _serverExtraHingeAngles[i];
				break;
			case HingeDirection.Y:
				localEulerAngles.y = _serverExtraHingeAngles[i];
				break;
			case HingeDirection.Z:
				localEulerAngles.z = _serverExtraHingeAngles[i];
				break;
			case HingeDirection.All:
				quaternion2 = _serverExtraHingeRots[i];
				break;
			}
			Quaternion b3 = ((_hingeDirection == HingeDirection.All) ? quaternion2 : Quaternion.Euler(localEulerAngles));
			_extraHinges[i].transform.localRotation = Quaternion.Slerp(_extraHinges[i].transform.localRotation, b3, num);
		}
		_rig.Sleep();
	}

	private void ApplyReceivedHeldToolPosRot()
	{
		float t = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
		_item.Tool.SwayTransform.localPosition = Vector3.Lerp(_item.Tool.SwayTransform.localPosition, _serverHeldToolPos, t);
		_item.Tool.SwayTransform.localRotation = Quaternion.Slerp(_item.Tool.SwayTransform.localRotation, _serverHeldToolRot, t);
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverSetServerPosRot(NetworkConnection netCon, Vector3 pos, Quaternion rot, bool onBoat, float[] extraHingeAngles = null, Quaternion[] extraHingeRots = null, Channel channel = Channel.Unreliable)
	{
		RpcWriter___ObserverSetServerPosRot___927077940(netCon, pos, rot, onBoat, extraHingeAngles, extraHingeRots, channel);
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverSetServerHeldToolPosRot(Vector3 pos, Quaternion rot, Channel channel = Channel.Unreliable)
	{
		RpcWriter___ObserverSetServerHeldToolPosRot___3689930256(pos, rot, channel);
	}

	public void ServerSetSyncedSimulator(NetworkConnection newSimulator)
	{
		_syncedSimulator.Value = newSimulator;
	}

	private void SendUpdateIfSimulating()
	{
		if (!IsSendingUpdates)
		{
			if (IsStationary)
			{
				if (!base.IsServerInitialized && !RecentlyStartedSimulation)
				{
					Server.Instance.SetSyncedSimulator(_item, Server.Instance.Owner);
				}
				return;
			}
			IsSendingUpdates = true;
		}
		SendPosRotToObservers();
		if (IsStationary && !IsHeldLocal && !_item.AttachedRod && !OnBoat)
		{
			IsSendingUpdates = false;
		}
	}

	private void ServerInitialize()
	{
		RigidbodyManager.RigSyncs.Add(this);
		_isInitialized = true;
	}

	private void ClientInitialize()
	{
		RigidbodyManager.RigSyncs.Add(this);
		_isInitialized = true;
		if (_clientWantedStartSimulating)
		{
			StartSimulateLocal();
		}
	}

	public void AddBoatPos(Vector3 movePos, Quaternion moveRot)
	{
		if ((bool)BoatManager.Boat && IsSimulatedLocal)
		{
			ItemExtraRigidbody[] extraRigs = _item.ExtraRigs;
			foreach (ItemExtraRigidbody itemExtraRigidbody in extraRigs)
			{
				Vector3 vector = itemExtraRigidbody.Rig.position - BoatManager.Boat.CenterOfMass;
				Vector3 vector2 = moveRot * vector - vector;
				Debug.DrawRay(itemExtraRigidbody.Rig.position, Vector3.up);
				itemExtraRigidbody.Rig.MovePosition(itemExtraRigidbody.Rig.position + movePos + vector2);
				itemExtraRigidbody.Rig.MoveRotation(moveRot * itemExtraRigidbody.Rig.rotation);
			}
		}
	}

	public void SetBoat(bool onBoat)
	{
		OnBoat = onBoat;
	}

	public void OnCollision(Collision col)
	{
		if (IsHeldLocal || _isFrozen || _isPermaFrozen || (bool)_item.Holder || (bool)_item.BirdHolder)
		{
			return;
		}
		_item.OnCollision(col);
		if (base.IsServerInitialized && (((bool)_item.Creature && _item.Creature.BossType != BossType.None && !_item.AttachedRod) || (col.transform.CompareTag("Boat") && !OnBoat)))
		{
			StartSimulateLocal();
		}
		else if (col.transform.CompareTag("Player"))
		{
			Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(col.transform);
			if ((bool)playerFromBodyPart && playerFromBodyPart.Owner.IsLocalClient && playerFromBodyPart.Movement.VelMagSqr > 0.1f)
			{
				StartSimulateLocal();
			}
		}
		else if (col.transform.CompareTag("Item"))
		{
			Item item = ItemManager.Get(col);
			if ((!_item.Bird || _item.Bird.IsDead) && (bool)item && !item == (bool)_item && ((item.RigidbodySync.IsSimulatedLocal && !DazedUtils.CheckIfStationary(item.Rig) && !item.Holder) || item.RigidbodySync.IsHeldLocal))
			{
				StartSimulateLocal();
			}
		}
	}

	public void StartSimulateLocal(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
	{
		if (base.IsServerInitialized && _isFloating.Value)
		{
			ServerSetIsFloating(to: false);
			SetKinematic(kinematic: false);
			IsSendingUpdates = true;
			IsStationary = false;
		}
		bool flag = (bool)_item.Creature && _item.Creature.BossType == BossType.Boss && !base.IsServerInitialized;
		bool flag2 = (bool)_item.Creature && !_item.Creature.IsDead && _item.Creature.BossType == BossType.Mini && !base.IsServerInitialized;
		bool flag3 = (bool)_item.AttachedRod && (bool)_item.AttachedRod.Holder && _item.AttachedRod.Holder.Owner.IsLocalClient;
		bool flag4 = _item.BirdHolder;
		if (!_isInitialized || _isFrozen || _isPermaFrozen || (flag && !flag3) || (flag2 && !flag3) || (flag4 && !base.IsServerInitialized))
		{
			if (!_isInitialized)
			{
				_clientWantedStartSimulating = true;
			}
			return;
		}
		_simulationChangedTick = base.TimeManager.Tick;
		if (!(_syncedSimulator.Value == InstanceFinder.ClientManager.Connection))
		{
			if (pos != Vector3.zero)
			{
				ZeroVelocity();
				base.transform.position = pos;
				base.transform.rotation = rot;
			}
			Server.Instance.SetSyncedSimulator(_item, InstanceFinder.ClientManager.Connection);
			_predictedSimulator = InstanceFinder.ClientManager.Connection;
			ToggleSimulation(isLocal: true);
		}
	}

	private void ToggleSimulation(bool isLocal)
	{
		Rigidbody[] extraHinges;
		if (isLocal)
		{
			IsSimulatedLocal = true;
			IsSendingUpdates = true;
			IsStationary = false;
			if (_isInitialized || !_startFrozen)
			{
				SetKinematic(kinematic: false);
			}
			_rig.interpolation = RigidbodyInterpolation.Interpolate;
			extraHinges = _extraHinges;
			for (int i = 0; i < extraHinges.Length; i++)
			{
				extraHinges[i].interpolation = RigidbodyInterpolation.Interpolate;
			}
			_serverToLocalRatio = 0f;
			InheritFakeVel();
			SendPosRotToObservers();
			return;
		}
		IsSimulatedLocal = false;
		_rig.Sleep();
		_rig.interpolation = RigidbodyInterpolation.None;
		extraHinges = _extraHinges;
		for (int i = 0; i < extraHinges.Length; i++)
		{
			extraHinges[i].interpolation = RigidbodyInterpolation.None;
		}
		_serverToLocalRatio = 1f;
		_timeOfServerResponse = Time.time;
		if (!_rig.isKinematic)
		{
			ZeroVelocity();
		}
		if ((bool)_item && (bool)_item.AttachedRod && _item.AttachedRod.Holder.Owner.IsLocalClient)
		{
			_item.AttachedRod.ReleaseItem(_item);
		}
	}

	private void SendPosRotToObservers()
	{
		float[] array = new float[_extraHinges.Length];
		Quaternion[] array2 = new Quaternion[_extraHinges.Length];
		for (int i = 0; i < _extraHinges.Length; i++)
		{
			switch (_hingeDirection)
			{
			case HingeDirection.X:
				array[i] = _extraHinges[i].transform.localEulerAngles.x;
				break;
			case HingeDirection.Y:
				array[i] = _extraHinges[i].transform.localEulerAngles.y;
				break;
			case HingeDirection.Z:
				array[i] = _extraHinges[i].transform.localEulerAngles.z;
				break;
			case HingeDirection.All:
				array2[i] = _extraHinges[i].transform.localRotation;
				break;
			}
		}
		Transform sendBoatVisual = GetSendBoatVisual();
		Vector3 pos = (sendBoatVisual ? sendBoatVisual.InverseTransformPoint(base.transform.position) : base.transform.position);
		Quaternion rot = (sendBoatVisual ? (Quaternion.Inverse(sendBoatVisual.rotation) * base.transform.rotation) : base.transform.rotation);
		if (!base.IsServerInitialized)
		{
			if (_extraHinges.Length != 0)
			{
				if (_hingeDirection == HingeDirection.All)
				{
					Server.Instance.UpdateItemPosRot(_item, InstanceFinder.ClientManager.Connection, pos, rot, OnBoat, null, array2);
				}
				else
				{
					Server.Instance.UpdateItemPosRot(_item, InstanceFinder.ClientManager.Connection, pos, rot, OnBoat, array);
				}
			}
			else
			{
				Server.Instance.UpdateItemPosRot(_item, InstanceFinder.ClientManager.Connection, pos, rot, OnBoat);
			}
		}
		else if (_extraHinges.Length != 0)
		{
			if (_hingeDirection == HingeDirection.All)
			{
				ObserverSetServerPosRot(_predictedSimulator, pos, rot, OnBoat, null, array2);
			}
			else
			{
				ObserverSetServerPosRot(_predictedSimulator, pos, rot, OnBoat, array);
			}
		}
		else
		{
			ObserverSetServerPosRot(_predictedSimulator, pos, rot, OnBoat);
		}
	}

	private void SendHeldToolPosRotToObservers()
	{
		Vector3 pos = Vector3.zero;
		Quaternion rot = Quaternion.identity;
		if (_item.Tool.Holder.ToolMovement.HoldPercent >= 1f)
		{
			pos = _item.Tool.SwayTransform.localPosition;
			rot = _item.Tool.SwayTransform.localRotation;
		}
		if (!base.IsServerInitialized)
		{
			Server.Instance.UpdateHeldToolPosRot(_item.Tool, pos, rot);
		}
		else
		{
			ObserverSetServerHeldToolPosRot(pos, rot);
		}
	}

	private void InheritFakeVel()
	{
		_rig.linearVelocity = _fakeVelocity;
		_rig.angularVelocity = _fakeAngularVelocity;
		Rigidbody[] extraHinges = _extraHinges;
		foreach (Rigidbody obj in extraHinges)
		{
			obj.linearVelocity = _fakeVelocity;
			obj.angularVelocity = _fakeAngularVelocity;
		}
	}

	private void CalculateFakeVelocity()
	{
		float time = Time.time;
		_velSamples.Enqueue(new Sample
		{
			Position = base.transform.position,
			Rotation = base.transform.rotation,
			Time = time
		});
		while (_velSamples.Count > 2 && time - _velSamples.Peek().Time > 0.1f)
		{
			_velSamples.Dequeue();
		}
		if (_velSamples.Count < 2)
		{
			return;
		}
		Sample sample = _velSamples.Peek();
		Sample sample2 = default;
		foreach (Sample velSample in _velSamples)
		{
			sample2 = velSample;
		}
		float num = sample2.Time - sample.Time;
		if (num > Mathf.Epsilon)
		{
			_fakeVelocity = (sample2.Position - sample.Position) / num;
			_fakeAngularVelocity = DazedUtils.GetAngularVelocityToTarget(sample.Rotation, sample2.Rotation) / num;
		}
	}

	public void SetKinematic(bool kinematic)
	{
		_rig.isKinematic = kinematic;
		Rigidbody[] extraHinges = _extraHinges;
		for (int i = 0; i < extraHinges.Length; i++)
		{
			extraHinges[i].isKinematic = kinematic;
		}
	}

	private void ZeroVelocity()
	{
		_rig.linearVelocity = Vector3.zero;
		_rig.angularVelocity = Vector3.zero;
		Rigidbody[] extraHinges = _extraHinges;
		foreach (Rigidbody obj in extraHinges)
		{
			obj.linearVelocity = Vector3.zero;
			obj.angularVelocity = Vector3.zero;
		}
	}

	public void SetMaxVel(bool useLimit)
	{
		float maxLinearVelocity = (useLimit ? GameInfo.MaxItemVel : 1000f);
		_rig.maxLinearVelocity = maxLinearVelocity;
		Rigidbody[] extraHinges = _extraHinges;
		for (int i = 0; i < extraHinges.Length; i++)
		{
			extraHinges[i].maxLinearVelocity = maxLinearVelocity;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyRigidbodySyncAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyRigidbodySyncAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_syncedSimulator.InitializeEarly(this, 1u, isSyncObject: false);
			_isFloating.InitializeEarly(this, 0u, isSyncObject: false);
			RegisterObserversRpc(0u, RpcReader___ObserverSetServerPosRot___927077940);
			RegisterObserversRpc(1u, RpcReader___ObserverSetServerHeldToolPosRot___3689930256);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateRigidbodySyncAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateRigidbodySyncAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_syncedSimulator.InitializeLate();
			_isFloating.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverSetServerPosRot___927077940(NetworkConnection netCon, Vector3 pos, Quaternion rot, bool onBoat, float[] extraHingeAngles = null, Quaternion[] extraHingeRots = null, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteNetworkConnection(netCon);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteQuaternion32(rot);
		pooledWriter.WriteBoolean(onBoat);
		GameTypeSerializersPooled.GWrite___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(pooledWriter, extraHingeAngles);
		GameTypeSerializersPooled.GWrite___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerated(pooledWriter, extraHingeRots);
		SendObserversRpc(0u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverSetServerPosRot___927077940(NetworkConnection P_0, Vector3 P_1, Quaternion P_2, bool P_3, float[] P_4, Quaternion[] P_5, Channel P_6)
	{
		if (IsSimulatedLocal)
		{
			return;
		}
		_receivedOnBoat = P_3;
		if (!_hasInitializedServerPos)
		{
			_hasInitializedServerPos = true;
			if (P_0 != InstanceFinder.ClientManager.Connection)
			{
				TeleportToPosRot(P_1, P_2, P_4, P_5);
			}
		}
		_serverPos = P_1;
		_serverRot = P_2;
		if (P_4 != null)
		{
			for (int i = 0; i < P_4.Length; i++)
			{
				_serverExtraHingeAngles[i] = P_4[i];
			}
		}
		if (P_5 != null)
		{
			for (int j = 0; j < P_5.Length; j++)
			{
				_serverExtraHingeRots[j] = P_5[j];
			}
		}
	}

	private void RpcReader___ObserverSetServerPosRot___927077940(PooledReader PooledReader0, Channel channel)
	{
		NetworkConnection networkConnection = PooledReader0.ReadNetworkConnection();
		Vector3 vector = PooledReader0.ReadVector3();
		Quaternion quaternion = PooledReader0.ReadQuaternion32();
		bool flag = PooledReader0.ReadBoolean();
		float[] array = GameTypeSerializersPooled.GRead___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
		Quaternion[] array2 = GameTypeSerializersPooled.GRead___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverSetServerPosRot___927077940(networkConnection, vector, quaternion, flag, array, array2, channel);
		}
	}

	private void RpcWriter___ObserverSetServerHeldToolPosRot___3689930256(Vector3 pos, Quaternion rot, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteQuaternion32(rot);
		SendObserversRpc(1u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverSetServerHeldToolPosRot___3689930256(Vector3 P_0, Quaternion P_1, Channel P_2)
	{
		_serverHeldToolPos = P_0;
		_serverHeldToolRot = P_1;
	}

	private void RpcReader___ObserverSetServerHeldToolPosRot___3689930256(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		Quaternion quaternion = PooledReader0.ReadQuaternion32();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverSetServerHeldToolPosRot___3689930256(vector, quaternion, channel);
		}
	}

	private void Awake_UserLogic_RigidbodySync_Assembly_002DCSharp_002Edll()
	{
		_serverPos = base.transform.position;
		_serverRot = base.transform.rotation;
		_item = GetComponent<Item>();
		_rig = GetComponent<Rigidbody>();
		SetKinematic(kinematic: true);
		_serverExtraHingeAngles = new float[_extraHinges.Length];
		_serverExtraHingeRots = new Quaternion[_extraHinges.Length];
		_syncedSimulator.OnChange += OnSyncedSimulatorChange;
		_isFloating.OnChange += OnIsFloatingChange;
	}
}
