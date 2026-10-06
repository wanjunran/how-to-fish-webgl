using UnityEngine;
using UnityEngine.Serialization;

public class AttackingFish : Fish
{
	[Header("Attack Stats")]
	[SerializeField]
	protected int _onHitDamage = 25;

	[SerializeField]
	private float _timeBetweenDamage = 0.5f;

	[SerializeField]
	private float _towardsPlayerForce = 5f;

	[SerializeField]
	private bool _attackInAir = true;

	[SerializeField]
	private float _airTowardsPlayerForce = 2f;

	[SerializeField]
	private float _setNewTargetDelay = 5f;

	[SerializeField]
	private float _underwaterAttackForceMulti = 2f;

	[FormerlySerializedAs("_oldVelAttackMulti")]
	[SerializeField]
	[Range(0f, 1f)]
	private float _keepOldVelOnAttackPercent = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("0 = no random extra dir, 1 = Can go 45 degrees in any direction of the player dir")]
	private float _randomTowardsPlayerDir = 0.1f;

	protected Player _target;

	private float _curCheckTarget;

	protected float _lastDamageTime;

	private bool NetworkInitialize___EarlyAttackingFishAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateAttackingFishAssembly_002DCSharp_002Edll_Excuted;

	public override void OnStartServer()
	{
		PlayerManager.OnPlayerAmountChange += OnPlayerAmountChange;
	}

	private void OnPlayerAmountChange()
	{
		if (PlayerManager.AlivePlayers.Count <= 0 && !base.IsDead)
		{
			DestroyItem(0);
		}
	}

	protected override void UpdateMovement()
	{
		base.UpdateMovement();
		FindTarget();
	}

	protected override void InitializeMovement()
	{
		base.InitializeMovement();
		AttackTarget();
	}

	protected void AttackTarget()
	{
		if (!_target || _rig.isKinematic)
		{
			return;
		}
		float num = 1f;
		if (_isUnderwater)
		{
			_isGrounded = true;
			num = _underwaterAttackForceMulti;
		}
		else
		{
			_isGrounded = false;
		}
		Vector3 normalized = (_target.Transform.position - _rig.worldCenterOfMass).normalized;
		bool flag = base.IsColliding || _isGrounded || _rigSync.IsStationary;
		float num2 = (flag ? (_towardsPlayerForce * num) : (_airTowardsPlayerForce * num));
		Vector3 vector = Random.insideUnitSphere * _randomTowardsPlayerDir;
		vector.y = 0f;
		Vector3 vector2 = (normalized + vector).normalized;
		if (Vector3.Dot(vector2, normalized) <= 0f)
		{
			vector2 = normalized;
		}
		if (!flag && !_attackInAir)
		{
			return;
		}
		if (_jumpOnWater && _isUnderwater)
		{
			_rig.linearVelocity = vector2 * num2 + Vector3.up * (_jumpForce * num);
			return;
		}
		_rig.linearVelocity *= _keepOldVelOnAttackPercent;
		_rig.linearVelocity += vector2 * num2;
		if (_isUnderwater | flag)
		{
			_rig.linearVelocity += Vector3.up * (_jumpForce * num);
		}
	}

	private void FindTarget()
	{
		if (!_target)
		{
			SetNewTarget();
		}
		_curCheckTarget += Time.fixedDeltaTime;
		if (_curCheckTarget >= _setNewTargetDelay)
		{
			SetNewTarget();
		}
	}

	protected void SetNewTarget(bool random = false)
	{
		_curCheckTarget = 0f;
		_target = (random ? PlayerManager.GetRandomAlivePlayer() : PlayerManager.GetNearestAlivePlayer(_rig.worldCenterOfMass));
	}

	public override void OnCollision(Collision other)
	{
		if (_rigSync.IsSimulatedLocal && !base.IsDead && !(Time.time - _lastDamageTime < _timeBetweenDamage) && other.transform.CompareTag("Player"))
		{
			Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(other.transform);
			if ((bool)playerFromBodyPart && other.contactCount != 0)
			{
				DamageOnCollision(playerFromBodyPart, other.contacts[0].point);
			}
		}
	}

	protected virtual void DamageOnCollision(Player player, Vector3 pos)
	{
		int num = _onHitDamage;
		if (base.BossType != BossType.None)
		{
			num = BossManager.GetBossDamage(num);
		}
		Server.Instance.HitPlayer(player, num, _rig.linearVelocity * GameInfo.PlayerDeathForceMultiFromCreature, pos, 3);
		_lastDamageTime = Time.time;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyAttackingFishAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyAttackingFishAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateAttackingFishAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateAttackingFishAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize___Late();
	}
}
