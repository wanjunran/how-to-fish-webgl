using UnityEngine;

public class PlayerBody : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private Transform _lowerBody;

	[SerializeField]
	private Transform _head;

	[SerializeField]
	private Transform[] _eyes;

	[SerializeField]
	private GameObject _newCharacter;

	[SerializeField]
	private GameObject _oldCharacter;

	[SerializeField]
	private GameObject _oldHead;

	[SerializeField]
	private Transform _nameTextCanvasHolder;

	[Header("Head movement")]
	[SerializeField]
	private float _headSpeed;

	[SerializeField]
	private float _headDamping;

	[Header("Bending towards move direction")]
	[SerializeField]
	private float _bendAmount;

	[SerializeField]
	private float _bodyPosOffsetAmount;

	[SerializeField]
	private float _bendSpeed;

	[Header("Bobbing with movement")]
	[SerializeField]
	private float _bodyBobAmount;

	[SerializeField]
	private float _bodyBobSpeed;

	[Header("Turning body towards player rotation")]
	[SerializeField]
	private float _bodyTurnSpeed;

	[SerializeField]
	private float _bodyTurnDamping;

	[Header("Breathing")]
	[SerializeField]
	private float _breathSize;

	private Quaternion[] _origEyeRots = new Quaternion[2];

	private Quaternion _headRot = Quaternion.identity;

	private Quaternion _bendRot;

	private Quaternion _swayRot = Quaternion.identity;

	private Vector3 _headAngVel;

	private Vector3 _origBodyPos;

	private Vector3 _bobPos;

	private Vector3 _bendPos;

	private Vector3 _swayAngVel;

	private Vector3 _breathePos;

	private bool _isOldModel;

	public Transform LowerBody => _lowerBody;

	public Transform Head => _head;

	private void Awake()
	{
		_origBodyPos = _lowerBody.localPosition;
		for (int i = 0; i < 2; i++)
		{
			_origEyeRots[i] = _eyes[i].localRotation;
		}
	}

	private void LateUpdate()
	{
		SetAndApplyPosRots();
	}

	public void Reset()
	{
		_headRot = Quaternion.Euler(_player.CamObject.eulerAngles.x, _player.CamObject.eulerAngles.y, 0f);
		_bendRot = Quaternion.identity;
		_swayRot = _player.CurPlayerRot;
		_headAngVel = Vector3.zero;
		_bobPos = Vector3.zero;
		_bendPos = Vector3.zero;
		_swayAngVel = Vector3.zero;
		_breathePos = Vector3.zero;
		SetAndApplyPosRots();
	}

	public void ToggleOldModel(bool to)
	{
		_isOldModel = to;
		_newCharacter.SetActive(!to);
		_oldCharacter.SetActive(to);
		_oldHead.SetActive(to);
		if (to)
		{
			_head = _oldHead.transform;
			_nameTextCanvasHolder.SetParent(_oldCharacter.transform);
			_nameTextCanvasHolder.localPosition = Vector3.up * 1.8f;
			_nameTextCanvasHolder.localEulerAngles = Vector3.up * -180f;
		}
	}

	private void SetAndApplyPosRots()
	{
		SetBreathing();
		SetBendPosRot();
		SetBobPos();
		SetSwayRot();
		_lowerBody.localPosition = _origBodyPos + _bendPos + _bobPos + _breathePos;
		_lowerBody.rotation = _swayRot;
		HeadMovement();
		EyeMovement();
	}

	private void SetBendPosRot()
	{
		Quaternion b = Quaternion.Euler(new Vector3(_player.Other.FlatLocalVelocity.z, 0f, 0f - _player.Other.FlatLocalVelocity.x) * _bendAmount);
		_bendRot = Quaternion.Slerp(_bendRot, b, _bendSpeed * Time.deltaTime);
		Vector3 b2 = new Vector3(0f - _player.Other.FlatLocalVelocity.x, _player.Other.FlatLocalVelocity.z, 0f) * _bodyPosOffsetAmount;
		_bendPos = Vector3.Lerp(_bendPos, b2, _bendSpeed * Time.deltaTime);
	}

	private void SetBobPos()
	{
		Vector3 b = Vector3.back * (_player.Legs.BodyBobDownPercent() * _bodyBobAmount);
		_bobPos = Vector3.Lerp(_bobPos, b, _bodyBobSpeed * Time.deltaTime);
	}

	private void SetSwayRot()
	{
		Quaternion curPlayerRot = _player.CurPlayerRot;
		curPlayerRot *= _bendRot;
		DazedUtils.SimulateSpringRotation(ref _swayRot, ref _swayAngVel, curPlayerRot, _bodyTurnSpeed, _bodyTurnDamping);
	}

	private void HeadMovement()
	{
		Quaternion targetRot = Quaternion.Euler(_player.CamObject.eulerAngles.x, _player.CamObject.eulerAngles.y, 0f);
		DazedUtils.SimulateSpringRotation(ref _headRot, ref _headAngVel, targetRot, _headSpeed, _headDamping);
		_head.rotation = _headRot;
	}

	private void EyeMovement()
	{
		for (int i = 0; i < 2; i++)
		{
			_eyes[i].rotation = Quaternion.LookRotation(_player.CamObject.forward, Vector3.up) * _origEyeRots[i];
		}
	}

	private void SetBreathing()
	{
		_breathePos.z = _player.ToolMovement.CurBreatheAmount * _breathSize;
	}
}
