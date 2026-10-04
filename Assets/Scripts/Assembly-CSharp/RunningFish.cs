using System.Collections.Generic;
using UnityEngine;

public class RunningFish : Fish
{
	[Header("Running Fih")]
	[SerializeField]
	private float _runSpeed = 5f;

	[SerializeField]
	private float _rotSpeed = 5f;

	[SerializeField]
	private float _maxRandomRot = 45f;

	[SerializeField]
	private Vector2 _randomRotTime = new Vector2(0.5f, 1f);

	[SerializeField]
	private bool _chasePlayer;

	[SerializeField]
	private Animation _anim;

	[SerializeField]
	private List<ParticleSystem> _particles;

	[SerializeField]
	private Collider[] _feetCols;

	[SerializeField]
	private Collider[] _worldColsToDisable;

	private Vector3 _curDir;

	private Vector3 _targetDir;

	private RaycastHit _hit;

	private float _curRotAmount;

	private float _curRotTime;

	private float _targetRotTime;

	private bool NetworkInitialize___EarlyRunningFishAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateRunningFishAssembly_002DCSharp_002Edll_Excuted;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.IsDead)
		{
			if (Physics.Raycast(base.transform.position, Vector3.down, out _hit, _groundCheckDist, GameInfo.LevelLayer) && !base.AttachedRod && !_holder && !base.IsDead)
			{
				ToggleGrounded(to: true);
			}
			else
			{
				ToggleGrounded(to: false);
			}
		}
	}

	public override void OnPickUp()
	{
		base.OnPickUp();
	}

	public override void OnDrop()
	{
		base.OnDrop();
	}

	private void ToggleGrounded(bool to)
	{
		if (to && !_isGrounded)
		{
			if ((bool)_anim)
			{
				_anim.Stop();
				_anim.Play("DripWalk");
			}
		}
		else if (!to && _isGrounded && (bool)_anim)
		{
			_anim.Stop();
			_anim.Play("DripIdle");
		}
		_isGrounded = to;
	}

	protected override void UpdateMovement()
	{
		if ((bool)base.AttachedRod || (bool)_holder || !_isGrounded || _rig.isKinematic)
		{
			return;
		}
		if (_chasePlayer)
		{
			Vector3 vector = Vector3.zero;
			if ((bool)Player.LocalPlayer && (bool)Player.LocalPlayer.Transform)
			{
				vector = Player.LocalPlayer.Transform.position;
			}
			Vector3 forward = base.transform.position - vector;
			forward.y = 0f;
			forward.Normalize();
			base.transform.forward = forward;
			Vector3 linearVelocity = base.transform.forward * _runSpeed;
			linearVelocity.y = _rig.linearVelocity.y;
			_rig.linearVelocity = linearVelocity;
			_rig.angularVelocity = Vector3.zero;
		}
		else
		{
			_curRotTime += Time.fixedDeltaTime;
			if (_curRotTime >= _targetRotTime)
			{
				InitializeMovement();
				_curRotTime = 0f;
				_targetRotTime = Random.Range(_randomRotTime.x, _randomRotTime.y);
			}
			_targetDir = RotateAround(_targetDir, Vector3.up, _curRotAmount);
			_curDir = Vector3.Slerp(_curDir, _targetDir, _rotSpeed * Time.fixedDeltaTime);
			Vector3 vector2 = _curDir * _runSpeed;
			_rig.linearVelocity = new Vector3(vector2.x, _rig.linearVelocity.y, vector2.z);
			Vector3 b = _curDir - Vector3.Dot(_curDir, _hit.normal) * _hit.normal;
			base.transform.forward = Vector3.Slerp(base.transform.forward, b, _rotSpeed);
		}
	}

	protected override void InitializeMovement()
	{
		Vector3 vector = Vector3.zero;
		if ((bool)Player.LocalPlayer && (bool)Player.LocalPlayer.Transform)
		{
			vector = Player.LocalPlayer.Transform.position;
		}
		Vector3 targetDir = base.transform.position - vector;
		targetDir.y = 0f;
		targetDir.Normalize();
		_curRotAmount = Random.Range(0f - _maxRandomRot, _maxRandomRot);
		_targetDir = targetDir;
		_targetDir.Normalize();
	}

	private Vector3 RotateAround(Vector3 vector, Vector3 axis, float angle)
	{
		axis.Normalize();
		float num = Mathf.Cos(angle);
		float num2 = Mathf.Sin(angle);
		return vector * num + Vector3.Cross(axis, vector) * num2 + axis * (Vector3.Dot(axis, vector) * (1f - num));
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		foreach (ParticleSystem particle in _particles)
		{
			particle.Stop();
		}
		Object.Destroy(_anim);
		ToggleWorldColliders(toEnabled: false);
		Collider[] feetCols = _feetCols;
		for (int i = 0; i < feetCols.Length; i++)
		{
			feetCols[i].enabled = true;
		}
		feetCols = _worldColsToDisable;
		for (int i = 0; i < feetCols.Length; i++)
		{
			feetCols[i].enabled = false;
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyRunningFishAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyRunningFishAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateRunningFishAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateRunningFishAssembly_002DCSharp_002Edll_Excuted = true;
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
