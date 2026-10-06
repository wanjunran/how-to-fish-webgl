using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using Steamworks;
using TMPro;
using UnityEngine;

public class OtherPlayer : NetworkBehaviour
{
	private struct Sample
	{
		public Vector3 Position;

		public float Time;
	}

	[SerializeField]
	private Player _player;

	[SerializeField]
	private Transform _transform;

	[SerializeField]
	private Transform _camProxy;

	[SerializeField]
	private CapsuleCollider _col;

	[SerializeField]
	private Transform _otherPlayerCanvasHolder;

	[SerializeField]
	private TextMeshProUGUI _nameText;

	[SerializeField]
	private float _groundCheckDist;

	private readonly Queue<Sample> _velSamples = new Queue<Sample>();

	private Vector3 _boatLastPos;

	private Vector3 _receivedPos;

	private Vector2 _receivedRot;

	private Vector3 _lastStepPos;

	private Vector3 _lastPos;

	private float _origColHeight;

	private float _curCrouchPercent;

	private bool _grounded;

	private bool _isSwimming;

	private bool NetworkInitialize___EarlyOtherPlayerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateOtherPlayerAssembly_002DCSharp_002Edll_Excuted;

	public Transform Transform => _transform;

	public Transform CamProxy => _camProxy;

	public Vector3 Velocity { get; private set; }

	public Vector3 FlatVelocity { get; private set; }

	public Vector3 FlatLocalVelocity { get; private set; }

	public float VelMag { get; private set; }

	public bool Grounded => _grounded;

	public bool OnBoat { get; private set; }

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_OtherPlayer_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	private void Update()
	{
		if (!base.Owner.IsLocalClient)
		{
			ApplyReceivedPosRot();
			if ((bool)_otherPlayerCanvasHolder && (bool)Player.LocalPlayer && (bool)Player.LocalPlayer.CamObject)
			{
				_otherPlayerCanvasHolder.LookAt(Player.LocalPlayer.CamObject);
			}
		}
	}

	private void FixedUpdate()
	{
		SetCrouchHeight();
		GroundCheck();
		WaterCheck();
		CalculateVelocity();
	}

	private void ApplyReceivedPosRot()
	{
		float t = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
		if (OnBoat && (bool)BoatManager.Boat)
		{
			Vector3 position = Vector3.Lerp(_boatLastPos, _receivedPos, t);
			position = BoatManager.Boat.VisualBoat.TransformPoint(position);
			_transform.position = position;
			_boatLastPos = BoatManager.Boat.VisualBoat.InverseTransformPoint(_transform.position);
		}
		else
		{
			_transform.position = Vector3.Lerp(_transform.position, _receivedPos, t);
		}
		_transform.rotation = Quaternion.Slerp(_transform.rotation, Quaternion.Euler(0f, _receivedRot.y, 0f), t);
		_camProxy.localRotation = Quaternion.Slerp(_camProxy.localRotation, Quaternion.Euler(_receivedRot.x, 0f, 0f), t);
	}

	[ObserversRpc(ExcludeOwner = true)]
	public void SetReceivedPosRot(Vector3 pos, Vector2 rot, bool onBoat = false, bool teleport = false, Channel channel = Channel.Unreliable)
	{
		RpcWriter___SetReceivedPosRot___4106386375(pos, rot, onBoat, teleport, channel);
	}

	public void Teleport(Vector3 pos, Vector2 rot)
	{
		if (!base.Owner.IsLocalClient)
		{
			_transform.position = pos;
			_transform.rotation = Quaternion.Euler(0f, rot.y, 0f);
			_camProxy.localRotation = Quaternion.Euler(rot.x, 0f, 0f);
			Velocity = Vector3.zero;
			FlatVelocity = Vector3.zero;
			FlatLocalVelocity = Vector3.zero;
			VelMag = 0f;
			_velSamples.Clear();
			_player.Body.Reset();
			_player.Legs.Reset();
		}
	}

	public void SetPlayerName(ulong id)
	{
		string friendPersonaName = SteamFriends.GetFriendPersonaName(new CSteamID(id));
		_nameText.text = friendPersonaName;
	}

	private void GroundCheck()
	{
		Physics.SphereCast(_transform.position + Vector3.down * (_col.height / 2f - _col.radius - _col.center.y), _col.radius * 0.9f, Vector3.down, out var hitInfo, _groundCheckDist, -1, QueryTriggerInteraction.Ignore);
		float num = Vector3.Angle(Vector3.up, hitInfo.normal);
		_grounded = (bool)hitInfo.transform && num < _player.Movement.SlipAngle;
	}

	private void WaterCheck()
	{
		bool flag = _player.Transform.position.y <= WaterManager.WaterHeight;
		if ((!_isSwimming & flag) && !Grounded && !OnBoat)
		{
			ParticleManager.Play("WaterSplash", _transform.position);
			if (Velocity.y <= -10f)
			{
				AudioManager.PlayRandomClipAt("ItemHitWaterHeavy_V", 1, 3, _transform.position, variation: true, AudioDistance.Short, 1f, 0.5f);
			}
			else
			{
				AudioManager.PlayRandomClipAt("ItemHitWaterMedium_V", 1, 3, _transform.position, variation: true, AudioDistance.Short, 1f, 0.5f);
			}
		}
		_isSwimming = flag;
	}

	private void CalculateVelocity()
	{
		float time = Time.time;
		Vector3 position = (OnBoat ? BoatManager.Boat.VisualBoat.InverseTransformPoint(Transform.position) : Transform.position);
		_velSamples.Enqueue(new Sample
		{
			Position = position,
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
			Vector3 vector = (sample2.Position - sample.Position) / num;
			bool num2 = !_grounded && _transform.position.y - _col.height * 0.5f < WaterManager.WaterHeight;
			bool flag = Velocity.y < 0f && vector.y > 0f;
			if (num2 & flag)
			{
				_player.Legs.SwitchFoot();
			}
			Velocity = (OnBoat ? BoatManager.Boat.VisualBoat.TransformDirection(vector) : vector);
			VelMag = Velocity.magnitude;
			FlatVelocity = new Vector3(Velocity.x, 0f, Velocity.z);
			FlatLocalVelocity = Transform.InverseTransformDirection(FlatVelocity);
		}
	}

	private void SetCrouchHeight()
	{
		_curCrouchPercent = Mathf.MoveTowards(_curCrouchPercent, _player.IsCrouching ? 1 : 0, _player.Movement.CrouchSpeed * Time.fixedDeltaTime);
		float num = Mathf.Lerp(_origColHeight, _origColHeight * _player.Movement.CrouchHeightMulti, _player.Movement.CrouchSpeedCurve.Evaluate(_curCrouchPercent));
		if (_col.height != num)
		{
			float num2 = (_origColHeight - num) * 0.5f;
			_col.height = num;
			_col.center = Vector3.up * num2;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyOtherPlayerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyOtherPlayerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(0u, RpcReader___SetReceivedPosRot___4106386375);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateOtherPlayerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateOtherPlayerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___SetReceivedPosRot___4106386375(Vector3 pos, Vector2 rot, bool onBoat = false, bool teleport = false, Channel channel = Channel.Unreliable)
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
		pooledWriter.WriteVector2(rot);
		pooledWriter.WriteBoolean(onBoat);
		pooledWriter.WriteBoolean(teleport);
		SendObserversRpc(0u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: true);
		pooledWriter.Store();
	}

	public void RpcLogic___SetReceivedPosRot___4106386375(Vector3 P_0, Vector2 P_1, bool P_2, bool P_3, Channel P_4)
	{
		if (!base.Owner.IsLocalClient)
		{
			if (OnBoat != P_2)
			{
				_velSamples.Clear();
			}
			_receivedPos = P_0;
			_receivedRot = P_1;
			if (P_3)
			{
				Teleport(P_0, P_1);
			}
			if (!OnBoat & P_2)
			{
				_boatLastPos = BoatManager.Boat.VisualBoat.InverseTransformPoint(_transform.position);
			}
			OnBoat = P_2;
		}
	}

	private void RpcReader___SetReceivedPosRot___4106386375(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		Vector2 vector2 = PooledReader0.ReadVector2();
		bool flag = PooledReader0.ReadBoolean();
		bool flag2 = PooledReader0.ReadBoolean();
		if (base.IsClientInitialized)
		{
			RpcLogic___SetReceivedPosRot___4106386375(vector, vector2, flag, flag2, channel);
		}
	}

	private void Awake_UserLogic_OtherPlayer_Assembly_002DCSharp_002Edll()
	{
		_origColHeight = _col.height;
	}
}
