using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

public class CrabArms : NetworkBehaviour
{
	[SerializeField]
	private Spidercrab _spiderCrab;

	[Header("Arms")]
	[SerializeField]
	private Transform _leftArmTarget;

	[SerializeField]
	private Transform _rightArmTarget;

	[SerializeField]
	private Transform _leftRestingPos;

	[SerializeField]
	private Transform _rightRestingPos;

	[Space]
	[SerializeField]
	private float _idleArmForce = 5f;

	[SerializeField]
	private float _idleArmDamping = 2f;

	[Header("Stunned Pose")]
	[SerializeField]
	private float _stunnedArmDrop = 0.35f;

	[SerializeField]
	private float _stunnedArmBacked = 0.35f;

	[SerializeField]
	[Min(0f)]
	private float _stunRecoveryTime = 0.5f;

	[Header("Snipping")]
	[SerializeField]
	private Transform _leftFinger;

	[SerializeField]
	private Transform _rightFinger;

	[SerializeField]
	private float _snipRot = 34f;

	[SerializeField]
	private float _snipTime = 0.1f;

	[Header("Attack Settings")]
	[SerializeField]
	private int _damage = 15;

	[SerializeField]
	private float _attackMoveTime = 0.2f;

	[SerializeField]
	private Vector2 _attackDelay = new Vector2(0.2f, 0.3f);

	private Player _target;

	private bool _curArmRight;

	private bool _isAttacking;

	private float _lastAttackTime;

	private float _randomizedAttackDelay;

	private Vector3 _orgLeftFingerRot;

	private Vector3 _orgRightFingerRot;

	private Vector3 _curLeftArmForce;

	private Vector3 _curRightArmForce;

	private Vector3 _curLeftArmPos;

	private Vector3 _curRightArmPos;

	private bool NetworkInitialize___EarlyCrabArmsAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateCrabArmsAssembly_002DCSharp_002Edll_Excuted;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_CrabArms_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	private void Update()
	{
		if (!_isAttacking || !_curArmRight)
		{
			SetIdleArmPos(right: true);
		}
		if (!_isAttacking || _curArmRight)
		{
			SetIdleArmPos(right: false);
		}
	}

	private void SetIdleArmPos(bool right)
	{
		Vector3 vector = (right ? _rightRestingPos.position : _leftRestingPos.position);
		if (_spiderCrab.CurState == CrabState.Stunned || Time.time < _spiderCrab.TimeOfLastStun)
		{
			vector += Vector3.down * _stunnedArmDrop - base.transform.forward * _stunnedArmBacked;
		}
		Vector3 vector2 = (right ? _curRightArmPos : _curLeftArmPos);
		Vector3 vector3 = (vector - vector2) * _idleArmForce;
		Vector3 vector4 = (right ? _curRightArmForce : _curLeftArmForce);
		Vector3 vector5 = vector4;
		vector4 += vector3 * Time.deltaTime;
		vector4 -= vector5 * (_idleArmDamping * Time.deltaTime);
		if (right)
		{
			_curRightArmPos += vector4;
			_rightArmTarget.position = _curRightArmPos;
		}
		else
		{
			_curLeftArmPos += vector4;
			_leftArmTarget.position = _curLeftArmPos;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (base.IsServerInitialized && !(Time.time - _lastAttackTime < _randomizedAttackDelay) && !_isAttacking && !_spiderCrab.IsDead && !_spiderCrab.AttachedRod && _spiderCrab.CurState != CrabState.Stunned && !(Time.time < _spiderCrab.TimeOfLastStun + 0.5f))
		{
			Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(other.transform);
			if ((bool)playerFromBodyPart && !playerFromBodyPart.Dying.IsDead)
			{
				AttackTarget(playerFromBodyPart);
				ObserverAttackTarget(playerFromBodyPart);
			}
		}
	}

	[ObserversRpc(ExcludeServer = true)]
	private void ObserverAttackTarget(Player player)
	{
		RpcWriter___ObserverAttackTarget___3849956746(player);
	}

	private void AttackTarget(Player player)
	{
		_isAttacking = true;
		_curArmRight = !_curArmRight;
		_target = player;
		Transform obj = (_curArmRight ? _rightArmTarget : _leftArmTarget);
		LeanTween.cancel(obj.gameObject);
		LeanTween.move(obj.gameObject, player.Transform.position, _attackMoveTime).setEase(LeanTweenType.easeOutQuad).setOnComplete(OnAttackFinished);
		LeanTween.value(obj.gameObject, 0f, 1f, _attackMoveTime * 0.5f).setOnComplete(Snip);
	}

	private void Snip()
	{
		Transform obj = (_curArmRight ? _rightFinger : _leftFinger);
		Vector3 vector = (_curArmRight ? _orgRightFingerRot : _orgLeftFingerRot);
		float num = (_curArmRight ? (0f - _snipRot) : _snipRot);
		LeanTween.cancel(obj.gameObject);
		LeanTween.rotateLocal(obj.gameObject, vector + Vector3.forward * num, _snipTime).setEase(LeanTweenType.easeInQuad).setOnComplete(ResetFinger);
	}

	private void ResetFinger()
	{
		Transform obj = (_curArmRight ? _rightFinger : _leftFinger);
		Vector3 to = (_curArmRight ? _orgRightFingerRot : _orgLeftFingerRot);
		LeanTween.cancel(obj.gameObject);
		LeanTween.rotateLocal(obj.gameObject, to, _snipTime).setEase(LeanTweenType.easeInQuad);
	}

	private void OnAttackFinished()
	{
		if (base.IsServerInitialized && (bool)_target && !_target.Dying.IsDead)
		{
			_target.Vitals.TakeDamage(BossManager.GetBossDamage(_damage));
		}
		_lastAttackTime = Time.time;
		_randomizedAttackDelay = Random.Range(_attackDelay.x, _attackDelay.y);
		_isAttacking = false;
		if (_curArmRight)
		{
			_curRightArmPos = _rightArmTarget.position;
			_curRightArmForce = Vector3.zero;
		}
		else
		{
			_curLeftArmForce = Vector3.zero;
			_curLeftArmPos = _leftArmTarget.position;
		}
		_target = null;
		AudioManager.PlayRandomClipAt("SpiderCrabSnip_V", 1, 2, _curArmRight ? _rightArmTarget.position : _leftArmTarget.position, variation: true, AudioDistance.Short);
		ParticleManager.Play("Blood", _curArmRight ? _rightArmTarget.position : _leftArmTarget.position, Vector3.up);
		ResetArms();
	}

	private void ResetArms()
	{
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyCrabArmsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyCrabArmsAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(0u, RpcReader___ObserverAttackTarget___3849956746);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateCrabArmsAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateCrabArmsAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverAttackTarget___3849956746(Player player)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GeneratedWriters___Internal.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, player);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	private void RpcLogic___ObserverAttackTarget___3849956746(Player P_0)
	{
		AttackTarget(P_0);
	}

	private void RpcReader___ObserverAttackTarget___3849956746(PooledReader PooledReader0, Channel channel)
	{
		Player player = GeneratedReaders___Internal.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverAttackTarget___3849956746(player);
		}
	}

	private void Awake_UserLogic_CrabArms_Assembly_002DCSharp_002Edll()
	{
		_orgLeftFingerRot = _leftFinger.localEulerAngles;
		_orgRightFingerRot = _rightFinger.localEulerAngles;
		_curRightArmPos = _rightArmTarget.position;
		_curLeftArmPos = _leftArmTarget.position;
	}
}
