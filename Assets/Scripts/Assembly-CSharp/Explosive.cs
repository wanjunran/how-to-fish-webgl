using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Explosive : Item
{
	[Header("Explosive Variables")]
	[SerializeField]
	[Tooltip("Can this item be activated?")]
	private bool _activatable = true;

	[SerializeField]
	[Tooltip("When item is forced to explode, how long should the delay be?")]
	private float _forcedExplosionDelay;

	[FormerlySerializedAs("_activatedExplosiondelay")]
	[FormerlySerializedAs("_delay")]
	[SerializeField]
	[Tooltip("When item is activated, how long should the delay be until explosion?")]
	private float _activatedExplosionDelay;

	[FormerlySerializedAs("_disableOnPin")]
	[SerializeField]
	[Tooltip("Object that will be disabled when upon activation")]
	[Header("Effects")]
	private GameObject _disableOnActivate;

	[FormerlySerializedAs("_enableOnPin")]
	[SerializeField]
	[Tooltip("Object that will be enabled upon activation")]
	private GameObject _enableOnActivate;

	[FormerlySerializedAs("_debrisOnPin")]
	[SerializeField]
	[Tooltip("Rigidbody that will be enabled upon activation")]
	private Rigidbody _debrisOnActivate;

	[SerializeField]
	[Tooltip("Name of sound effect that will play upon activation")]
	private string _activateSoundName;

	[SerializeField]
	[Tooltip("Volume of sound effect that will play upon activation")]
	private float _activateSoundVol = 1f;

	[SerializeField]
	[Tooltip("Fuse particle that will move downwards while the timer is timing")]
	private ParticleSystem _fuse;

	[SerializeField]
	[Tooltip("Fuse particle that will move downwards while the timer is timing")]
	private ParticleSystem _fuseLight;

	[SerializeField]
	[Tooltip("How far down the fuse particle will move during timer")]
	private float _fuseMoveDist;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Where in the timer will the fuse be lit?")]
	private float _fuseLightPercent;

	[SerializeField]
	[Tooltip("Lighter that will be animated to go light the fuse")]
	private Transform _lighter;

	[SerializeField]
	[Tooltip("Move speed curve for the lighter")]
	private AnimationCurve _lighterMoveSpeedCurve;

	[SerializeField]
	[Tooltip("Pick up speed curve for the lighter")]
	private AnimationCurve _lighterPickUpSpeedCurve;

	[SerializeField]
	[Tooltip("Move speed for the lighter from off screen to default pos")]
	private float _lighterEquipSpeed;

	[SerializeField]
	[Tooltip("Position off screen where lighter will spawn. In cameras local space")]
	private Vector3 _lighterStartPos;

	[SerializeField]
	[Tooltip("Rotation that the lighter will be at when reaching the fuse")]
	private Vector3 _lighterFuseRot;

	[SerializeField]
	private ExplosionInfo _explosionInfo;

	private bool _isActivated;

	private bool _hasExploded;

	private float _timeUntilExplosion;

	private float _lighterPickupPercent;

	private Quaternion _lighterOrigRot;

	private Vector3 _lighterOrigPos;

	private Vector3 _fuseOrigPos;

	private Vector3 _curLighterPos;

	private bool NetworkInitialize___EarlyExplosiveAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateExplosiveAssembly_002DCSharp_002Edll_Excuted;

	public Player PlayerWhoForcedExplosion { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Explosive_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	protected override void Update()
	{
		base.Update();
		if (_isActivated)
		{
			_timeUntilExplosion -= Time.deltaTime;
			AnimateLighterAndFuse();
		}
		else if ((bool)_holder)
		{
			AnimateLighterPickUp();
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.IsServerInitialized && _isActivated && _timeUntilExplosion <= 0f)
		{
			ServerExplode();
		}
	}

	public override void OnPickUp()
	{
		base.OnPickUp();
		_lighterPickupPercent = 0f;
		_curLighterPos = _lighterStartPos;
		AnimateLighterPickUp();
	}

	public override void OnDrop()
	{
		_lighter.gameObject.SetActive(value: false);
		base.OnDrop();
	}

	public override void InspectInput(InputAction.CallbackContext context)
	{
		string description = $"Damage: {_explosionInfo.Damage}";
		PlayerUI.ShowInspectInfo(this, description);
	}

	public void ForceExplode(Player playerWhoForced, bool instant)
	{
		if ((!_isActivated || !(_timeUntilExplosion < _forcedExplosionDelay)) && !base.IsDeinitializing)
		{
			PlayerWhoForcedExplosion = playerWhoForced;
			_timeUntilExplosion = _forcedExplosionDelay;
			if (base.IsServerInitialized)
			{
				ObserverActivate(InstanceFinder.TimeManager.Tick, forced: true, instant);
			}
			else
			{
				Server.Instance.ActivateExplosive(this, InstanceFinder.TimeManager.Tick, forced: true, instant, playerWhoForced);
			}
		}
	}

	public override void PrimaryInput(InputAction.CallbackContext context)
	{
		if (_activatable)
		{
			LocalActivate();
		}
	}

	[ObserversRpc]
	public void ObserverActivate(uint tick, bool forced = false, bool instant = false, Player playerWhoForced = null)
	{
		RpcWriter___ObserverActivate___1538889672(tick, forced, instant, playerWhoForced);
	}

	private void LocalActivate()
	{
		if (!_isActivated)
		{
			if (base.IsServerInitialized)
			{
				ObserverActivate(InstanceFinder.TimeManager.Tick);
			}
			else
			{
				Server.Instance.ActivateExplosive(this, InstanceFinder.TimeManager.Tick);
			}
			ActivateEffects();
		}
	}

	private void ServerExplode()
	{
		if (!_hasExploded && !base.IsDeinitializing)
		{
			_hasExploded = true;
			ExplosionManager.ServerExplode(this, _explosionInfo);
		}
	}

	private void ActivateEffects()
	{
		if ((bool)_disableOnActivate)
		{
			_disableOnActivate.SetActive(value: false);
		}
		if ((bool)_enableOnActivate)
		{
			_enableOnActivate.SetActive(value: true);
		}
	}

	private void DecreaseTimer(uint times = 1u)
	{
		_timeUntilExplosion -= Time.fixedDeltaTime * (float)times;
	}

	private void AnimateLighterAndFuse()
	{
		float num = 1f - _timeUntilExplosion / _activatedExplosionDelay;
		float time = ((!(num < _fuseLightPercent)) ? (1f - (num - _fuseLightPercent) / _fuseLightPercent) : (num / _fuseLightPercent));
		float t = (num - _fuseLightPercent) / (1f - _fuseLightPercent);
		if (!_fuse.isPlaying && num > _fuseLightPercent)
		{
			_fuse.Play();
			_fuseLight.Play();
			if ((bool)_holder)
			{
				AudioManager.PlayPlayerClip(_activateSoundName, _holder, variation: true, AudioDistance.VeryShort, _activateSoundVol);
			}
			else
			{
				AudioManager.PlayClipAt(_activateSoundName, base.transform.position, variation: true, AudioDistance.VeryShort, _activateSoundVol);
			}
		}
		if ((bool)_holder)
		{
			_lighter.localPosition = Vector3.Lerp(_lighterOrigPos, _fuseOrigPos, _lighterMoveSpeedCurve.Evaluate(time));
			_lighter.localRotation = Quaternion.Slerp(_lighterOrigRot, Quaternion.Euler(_lighterFuseRot), _lighterMoveSpeedCurve.Evaluate(time));
		}
		float a = 0.0001f;
		float b = 0.04f;
		ParticleSystem.ShapeModule shape = _fuse.shape;
		shape.radius = Mathf.Lerp(a, b, t);
		_fuse.transform.localPosition = Vector3.Lerp(_fuseOrigPos, _fuseOrigPos + Vector3.down * _fuseMoveDist, t);
	}

	private void AnimateLighterPickUp()
	{
		if (!(_lighterPickupPercent >= 1f))
		{
			_lighterPickupPercent = Mathf.MoveTowards(_lighterPickupPercent, 1f, _lighterEquipSpeed * Time.deltaTime);
			_curLighterPos = Vector3.Lerp(base.transform.InverseTransformPoint(_holder.CamObject.TransformPoint(_lighterStartPos)), _lighterOrigPos, _lighterPickUpSpeedCurve.Evaluate(_lighterPickupPercent));
			_lighter.localPosition = _curLighterPos;
		}
	}

	public override ExplosionInfo GetExplosionInfo()
	{
		return _explosionInfo;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyExplosiveAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyExplosiveAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(2u, RpcReader___ObserverActivate___1538889672);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateExplosiveAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateExplosiveAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverActivate___1538889672(uint tick, bool forced = false, bool instant = false, Player playerWhoForced = null)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteUInt32(tick);
		pooledWriter.WriteBoolean(forced);
		pooledWriter.WriteBoolean(instant);
		GeneratedWriters___Internal.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, playerWhoForced);
		SendObserversRpc(2u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverActivate___1538889672(uint P_0, bool P_1, bool P_2, Player P_3)
	{
		PlayerWhoForcedExplosion = P_3;
		if (!_isActivated || (P_1 && _timeUntilExplosion > _forcedExplosionDelay))
		{
			_timeUntilExplosion = _activatedExplosionDelay;
			_isActivated = true;
			if (P_1)
			{
				_timeUntilExplosion = (P_2 ? 0f : _forcedExplosionDelay);
				uint times = (uint)((float)(InstanceFinder.TimeManager.Tick - P_0) * GameInfo.TickMulti);
				DecreaseTimer(times);
			}
			ActivateEffects();
		}
	}

	private void RpcReader___ObserverActivate___1538889672(PooledReader PooledReader0, Channel channel)
	{
		uint num = PooledReader0.ReadUInt32();
		bool flag = PooledReader0.ReadBoolean();
		bool flag2 = PooledReader0.ReadBoolean();
		Player player = GeneratedReaders___Internal.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverActivate___1538889672(num, flag, flag2, player);
		}
	}

	protected virtual void Awake_UserLogic_Explosive_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_explosive = this;
		_fuseOrigPos = _fuse.transform.localPosition;
		_lighterOrigRot = _lighter.localRotation;
		_lighterOrigPos = _lighter.localPosition;
	}
}
