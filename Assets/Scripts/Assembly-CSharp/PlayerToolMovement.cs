using System;
using UnityEngine;

public class PlayerToolMovement : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private Rigidbody _toolSwayRig;

	[SerializeField]
	private Transform _toolSwayTarget;

	[SerializeField]
	private Rigidbody _toolRecoilRig;

	[Header("Breathing")]
	[SerializeField]
	private float _breatheSpeed;

	[SerializeField]
	private AnimationCurve _breatheCurve;

	[Header("Movement")]
	[SerializeField]
	private float _bobMulti;

	[SerializeField]
	private float _sprintBobMulti;

	[SerializeField]
	private float _horizontalBobMulti;

	[SerializeField]
	private float _sprintBobTransitionSpeed;

	[SerializeField]
	private float _tiltSpeed;

	[SerializeField]
	private float _screenShakeMulti;

	[Header("Pick up / equip")]
	[SerializeField]
	private float _equipSpeed;

	[SerializeField]
	private AnimationCurve _pickUpSpeedCurve;

	[SerializeField]
	private float _pickUpSpeed;

	private Tool _tool;

	private Vector3 _equipRot;

	private Vector3 _equipPos;

	private Vector3 _origEquipRot;

	private Vector3 _origEquipPos;

	private Vector3 _lookRot;

	private Vector2 _bobPos;

	private Vector3 _swayPos;

	private Vector3 _swayRot;

	private Vector2 _tarPos;

	private Vector2 _refPos;

	private Vector3 _fallPos;

	private Vector3 _breathePos;

	private Vector3 _aimPos;

	private Vector3 _sprintPos;

	private Vector3 _sprintRot;

	private Vector3 _shakePos;

	private Vector3 _pickUpPos;

	private Vector3 _pickUpRot;

	private Vector3 _finalTargetPos;

	private Vector3 _finalTargetRot;

	private Vector3 _fakeItemAngVel;

	private Vector3 _fakeItemVel;

	private Vector3 _lastUpdatePos;

	private Quaternion _lastUpdateRot;

	private float _curAimMulti;

	private float _curSprintPercent;

	private float _holdPercent;

	private float _tiltRot;

	private float _tarTilt;

	private float _refTilt;

	private float _breathePercent;

	private int _stage;

	private bool _breatheIn;

	private bool _hasNewTool;

	private bool _spawnedFromInv;

	private Tool _droppedTool;

	public Vector3 AimPos => _aimPos;

	public Vector3 SprintPos => _sprintPos;

	public Vector3 SprintRot => _sprintRot;

	public Vector3 FakeItemVel => _fakeItemVel;

	public Vector3 FakeItemAngVel => _fakeItemAngVel;

	public float HoldPercent => _holdPercent;

	public float CurBreatheAmount => _breathePos.y;

	public Tool CurrentTool => _tool;

	public ConfigurableJoint ToolRecoilRigJoint { get; private set; }

	private void Awake()
	{
		ToolRecoilRigJoint = _toolRecoilRig.GetComponent<ConfigurableJoint>();
	}

	public void Recoil(Vector2 recoil)
	{
		_lookRot += new Vector3(0f - recoil.y, recoil.x);
	}

	public void SetTool(Tool tool)
	{
		if (_tool == tool)
		{
			return;
		}
		if (!tool)
		{
			_droppedTool = _tool;
		}
		_tool = tool;
		if ((bool)tool)
		{
			_hasNewTool = true;
			if ((bool)tool.HandsMesh)
			{
				tool.HandsMesh.enabled = false;
			}
			ResetToolPosRot();
			ResetToolVariables();
			ResetEquipVariables();
			ResetSwayRecoilPosRotVel();
		}
	}

	public void ReconcileHeldTool(Tool heldTool)
	{
		if (_tool != heldTool)
		{
			SetTool(heldTool);
		}
	}

	public void ClearTool(Tool tool)
	{
		if ((bool)tool && !(_tool != tool))
		{
			SetTool(null);
		}
	}

	public void SpawnFromInventory(bool fromFixedUpdate = false)
	{
		_spawnedFromInv = !fromFixedUpdate;
		if ((bool)_tool)
		{
			_hasNewTool = false;
			ResetToolPosRot();
			ResetToolVariables();
			ResetEquipVariables();
			ResetSwayRecoilPosRotVel();
			SetSwayRigToInvPosRot();
			if (!_player.Owner.IsLocalClient)
			{
				_tool.RigidbodySync.TeleportHeldToolPosRot(_tool.DrawAnimStartPos, Quaternion.Euler(_tool.DrawAnimStartRot));
			}
			_holdPercent = 1f;
			if (fromFixedUpdate)
			{
				LeanTween.value(base.gameObject, 0f, 1f, _equipSpeed).setEaseOutSine().setOnUpdate(EquipEase);
			}
		}
	}

	public void SetAimPos(Vector3 aimPos)
	{
		_aimPos = aimPos;
	}

	public void SetSprintPos(Vector3 sprintPos)
	{
		_sprintPos = sprintPos;
	}

	public void SetSprintRot(Vector3 sprintRot)
	{
		_sprintRot = sprintRot;
	}

	public void SetShakePos(Vector3 shakePos)
	{
		_shakePos = shakePos * _screenShakeMulti;
	}

	public void AddPosToRecoilRig(Vector3 posForce)
	{
		_toolRecoilRig.position += posForce;
	}

	public void AddRotToRecoilRig(Vector3 rotForce)
	{
		_toolRecoilRig.rotation *= Quaternion.Euler(rotForce);
	}

	private void LateUpdate()
	{
		Breathe();
		if (!_tool)
		{
			return;
		}
		if (_holdPercent < 1f)
		{
			_holdPercent += _pickUpSpeed * Time.deltaTime;
		}
		else
		{
			_holdPercent = 1f;
		}
		GlueToCamera();
		if (_player.Owner.IsLocalClient)
		{
			if ((bool)_tool.Weapon)
			{
				_curAimMulti = Mathf.Lerp(_curAimMulti, _tool.Weapon.IsAds ? _tool.Weapon.Attachments.AimSwayMulti : 1f, 50f * Time.deltaTime);
			}
			else
			{
				_curAimMulti = 1f;
			}
			Sway();
			Bob();
			Fall();
			LookAround();
			ApplyPosRotToModel();
		}
	}

	private void FixedUpdate()
	{
		if ((bool)_droppedTool)
		{
			SetToolDropPos(_droppedTool);
			_droppedTool = null;
		}
		if (_spawnedFromInv)
		{
			SpawnFromInventory(fromFixedUpdate: true);
		}
		if (!_tool)
		{
			return;
		}
		GlueToCamera();
		if (!_player.Owner.IsLocalClient)
		{
			return;
		}
		ApplySwayToTarget();
		CalculateFakeVel();
		if (_hasNewTool)
		{
			_hasNewTool = false;
			if ((bool)_tool)
			{
				ResetSwayRecoilPosRotVel();
			}
		}
	}

	private void Sway()
	{
		Vector2 lookInput = _player.Camera.LookInput;
		Vector2 vector = new Vector2(0f - lookInput.y, lookInput.x) * 0.0001f / Time.deltaTime;
		_swayPos = vector * (_tool.SwayPosForce * _curAimMulti);
		_swayRot = new Vector3((0f - vector.y) * _tool.SwayRotForce.x, vector.x * _tool.SwayRotForce.y, (0f - vector.x) * _tool.SwayRotForce.z) * _curAimMulti;
		_swayPos = Vector3.ClampMagnitude(_swayPos, _tool.MaxSwayPos);
		_swayRot = Vector3.ClampMagnitude(_swayRot, _tool.MaxSwayRot);
	}

	private void Bob()
	{
		_curSprintPercent = Mathf.Lerp(_curSprintPercent, _player.Movement.Sprinting ? 1 : 0, _sprintBobTransitionSpeed * Time.deltaTime);
		float num = Mathf.Lerp(_bobMulti, _sprintBobMulti, _curSprintPercent);
		_bobPos = new Vector2(_player.Camera.BobPos.x * num * _horizontalBobMulti, _player.Camera.BobPos.y * num) * _curAimMulti;
		_tarTilt = _player.Movement.Input.x * _tool.TiltAmount;
		_tiltRot = Mathf.SmoothDamp(_tiltRot, _tarTilt * _curAimMulti, ref _refTilt, (_tarTilt == 0f) ? _player.Camera.TiltSpeed : _tiltSpeed);
	}

	private void Fall()
	{
		if (_player.Movement.Grounded)
		{
			_fallPos.y = 0f;
		}
		else
		{
			_fallPos.y = (0f - _player.Camera.VelY) * _tool.FallForce / Time.deltaTime;
		}
	}

	private void Breathe()
	{
		_breathePercent = Mathf.MoveTowards(_breathePercent, _breatheIn ? 1 : 0, _breatheSpeed * Time.deltaTime);
		if (_breathePercent >= 1f)
		{
			_breathePercent = 1f;
			_breatheIn = false;
		}
		else if (_breathePercent <= 0f)
		{
			_breathePercent = 0f;
			_breatheIn = true;
		}
		_breathePos.y = _breatheCurve.Evaluate(_breathePercent) * (((bool)_tool && _player.Owner.IsLocalClient) ? _curAimMulti : 1f);
	}

	private void ApplySwayToTarget()
	{
		_toolSwayTarget.localPosition = new Vector3(_bobPos.x, _bobPos.y, 0f) + _toolRecoilRig.rotation * _toolRecoilRig.position + _swayPos + _sprintPos + _shakePos + _aimPos + _fallPos + _breathePos + _equipPos + _tool.LookOffset;
		_toolSwayTarget.localEulerAngles = new Vector3(0f, 0f, _tiltRot) + _toolRecoilRig.rotation.eulerAngles + _swayRot + _sprintRot + _equipRot;
	}

	private void ApplyPosRotToModel()
	{
		if (_holdPercent >= 1f)
		{
			_tool.SwayTransform.localPosition = _toolSwayRig.transform.position - _tool.LookOffset;
		}
		else
		{
			Vector3 vector = Vector3.Lerp(Vector3.zero, _toolSwayRig.transform.position - _tool.LookOffset, _holdPercent);
			_tool.SwayTransform.localPosition = _tool.SwayTransformOffset + vector;
		}
		_tool.SwayTransform.localEulerAngles = Vector3.zero;
		Vector3 eulerAngles = _toolSwayRig.transform.eulerAngles;
		_tool.SwayTransform.RotateAround(_tool.SwayRotAroundTransform.position, _tool.SwayTransform.right, eulerAngles.x);
		_tool.SwayTransform.RotateAround(_tool.SwayRotAroundTransform.position, _tool.SwayTransform.up, eulerAngles.y);
		_tool.SwayTransform.RotateAround(_tool.SwayRotAroundTransform.position, _tool.SwayTransform.forward, eulerAngles.z);
	}

	private void LookAround()
	{
		_lookRot += (Vector3)_player.Camera.LookInput * _tool.LookSpeed;
		_lookRot = new Vector2(Mathf.Clamp(_lookRot.x, 0f - _tool.MaxLookAmount.x, _tool.MaxLookAmount.x), Mathf.Clamp(_lookRot.y, 0f - _tool.MaxLookAmount.y, _tool.MaxLookAmount.y));
		_lookRot.z = _lookRot.y / _tool.MaxLookAmount.y * (0f - _tool.MaxLookAmount.z);
		if (!_tool.CanLookAround || ((bool)_tool.Weapon && _tool.Weapon.IsAds))
		{
			_lookRot = Vector3.zero;
		}
		_toolSwayTarget.parent.localEulerAngles = _lookRot;
	}

	private void GlueToCamera()
	{
		Vector3 position = _player.CamObject.position;
		if (_holdPercent < 1f)
		{
			if (_player.Owner.IsLocalClient)
			{
				position -= _player.CamObject.rotation * _tool.SwayTransformOffset;
			}
			else
			{
				position += _player.CamObject.forward * (_tool.ThirdPersonOffset * _holdPercent);
			}
			_tool.transform.position = Vector3.LerpUnclamped(_pickUpPos, position, _pickUpSpeedCurve.Evaluate(_holdPercent));
			_tool.transform.rotation = Quaternion.SlerpUnclamped(Quaternion.Euler(_pickUpRot), _player.CamObject.rotation, _pickUpSpeedCurve.Evaluate(_holdPercent));
		}
		else
		{
			if (!_player.Owner.IsLocalClient)
			{
				position += _player.CamObject.forward * _tool.ThirdPersonOffset;
			}
			_tool.transform.position = position;
			_tool.transform.rotation = _player.CamObject.rotation;
		}
	}

	private void EquipEase(float t)
	{
		_equipRot = Vector3.Lerp(_origEquipRot, Vector3.zero, t);
		_equipPos = Vector3.Lerp(_origEquipPos, Vector3.zero, t);
	}

	private void ResetToolVariables()
	{
		_sprintPos = Vector3.zero;
		_sprintRot = Vector3.zero;
		_bobPos = Vector3.zero;
		_bobPos = Vector3.zero;
		_swayPos = Vector3.zero;
		_sprintPos = Vector3.zero;
		_aimPos = Vector3.zero;
		_fallPos = Vector3.zero;
		_breathePos = Vector3.zero;
		_tiltRot = 0f;
		_swayRot = Vector3.zero;
		_sprintRot = Vector3.zero;
	}

	private void ResetEquipVariables()
	{
		_origEquipRot = Vector3.zero;
		_equipRot = Vector3.zero;
		_origEquipPos = Vector3.zero;
		_equipPos = Vector3.zero;
		_holdPercent = 0f;
		if ((bool)_tool)
		{
			_pickUpPos = _tool.transform.position;
			_pickUpRot = _tool.transform.eulerAngles;
		}
	}

	private void ResetSwayRecoilPosRotVel()
	{
		if (_player.Owner.IsLocalClient)
		{
			_toolSwayRig.transform.localEulerAngles = _origEquipRot;
			_toolSwayTarget.localEulerAngles = _origEquipRot;
			_toolSwayRig.transform.localPosition = _tool.LookOffset + _origEquipPos;
			_toolSwayTarget.localPosition = _tool.LookOffset + _origEquipPos;
			_toolSwayRig.linearVelocity = Vector3.zero;
			_toolSwayRig.angularVelocity = Vector3.zero;
			_toolRecoilRig.position = Vector3.zero;
			_toolRecoilRig.rotation = Quaternion.identity;
			_toolRecoilRig.linearVelocity = Vector3.zero;
			_toolRecoilRig.angularVelocity = Vector3.zero;
		}
	}

	private void SetSwayRigToInvPosRot()
	{
		if (_player.Owner.IsLocalClient)
		{
			_origEquipPos = _tool.DrawAnimStartPos;
			_equipPos = _origEquipPos;
			_toolSwayRig.transform.localPosition = _origEquipPos;
			_toolSwayTarget.localPosition = _origEquipPos;
			_origEquipRot = _tool.DrawAnimStartRot;
			_equipRot = _origEquipRot;
			_toolSwayRig.transform.localEulerAngles = _origEquipRot;
			_toolSwayTarget.localEulerAngles = _origEquipRot;
		}
	}

	private void ResetToolPosRot()
	{
		if ((bool)_tool)
		{
			_tool.SwayTransform.localPosition = Vector3.zero;
			_tool.SwayTransform.localRotation = Quaternion.identity;
		}
	}

	public void OnForcedToolDrop(Tool tool)
	{
		SetToolDropPos(tool);
	}

	private void SetToolDropPos(Tool tool)
	{
		Vector3 swayTransformOffset = tool.SwayTransformOffset;
		if (!_player.Owner.IsLocalClient)
		{
			swayTransformOffset.z += tool.ThirdPersonOffset;
		}
		tool.transform.rotation = tool.SwayTransform.rotation;
		tool.transform.position = tool.transform.TransformPoint(tool.SwayTransform.localPosition - swayTransformOffset);
		tool.SwayTransform.localRotation = Quaternion.identity;
		tool.SwayTransform.localPosition = swayTransformOffset;
	}

	private void CalculateFakeVel()
	{
		_fakeItemVel = (_tool.SwayRotAroundTransform.position - _lastUpdatePos) / Time.fixedDeltaTime;
		_lastUpdatePos = _tool.SwayRotAroundTransform.position;
		(_tool.SwayRotAroundTransform.rotation * Quaternion.Inverse(_lastUpdateRot)).ToAngleAxis(out var angle, out var axis);
		if (angle > 180f)
		{
			angle -= 360f;
		}
		_fakeItemAngVel = axis * (angle * (MathF.PI / 180f)) / Time.fixedDeltaTime;
		_lastUpdateRot = _tool.SwayRotAroundTransform.rotation;
	}
}
