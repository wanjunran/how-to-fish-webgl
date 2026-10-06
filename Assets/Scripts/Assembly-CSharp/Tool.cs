using System.Collections;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Serialization;

public class Tool : Item
{
	[SerializeField]
	[Header("Tool")]
	protected Renderer _handsMesh;

	[SerializeField]
	private Transform _swayTransform;

	[SerializeField]
	private Transform _swayRotAroundTransform;

	[FormerlySerializedAs("_3rdPersonOffset")]
	[SerializeField]
	protected float _thirdPersonOffset;

	[SerializeField]
	[Header("Movement")]
	private float _tiltAmount;

	[SerializeField]
	[Header("Looking around")]
	private bool _canLookAround;

	[SerializeField]
	private Vector3 _maxLookAmount;

	[SerializeField]
	private float _lookSpeed;

	[SerializeField]
	[Header("Sway")]
	private float _swayPosForce;

	[SerializeField]
	private Vector3 _swayRotForce;

	[SerializeField]
	private float _fallForce;

	[SerializeField]
	private float _maxSwayPos;

	[SerializeField]
	private float _maxSwayRot;

	[SerializeField]
	private Vector3 _lookOffset;

	[SerializeField]
	[Header("Inspect Animation")]
	private bool _hasInspectAnim;

	[SerializeField]
	private AudioSequence _inspectAudioSeq;

	[SerializeField]
	protected Animation _anim;

	private bool NetworkInitialize___EarlyToolAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateToolAssembly_002DCSharp_002Edll_Excuted;

	public Transform SwayTransform => _swayTransform;

	public Transform SwayRotAroundTransform => _swayRotAroundTransform;

	public Renderer HandsMesh => _handsMesh;

	public float TiltAmount => _tiltAmount;

	public bool CanLookAround => _canLookAround;

	public Vector3 MaxLookAmount => _maxLookAmount;

	public float LookSpeed => _lookSpeed;

	public float SwayPosForce => _swayPosForce;

	public Vector3 SwayRotForce => _swayRotForce;

	public float FallForce => _fallForce;

	public float MaxSwayPos => _maxSwayPos;

	public float MaxSwayRot => _maxSwayRot;

	public Vector3 LookOffset => _lookOffset;

	public float ThirdPersonOffset => _thirdPersonOffset;

	public Vector3 SwayTransformOffset { get; private set; }

	public void SetLookOffset()
	{
		_lookOffset = base.transform.rotation * (_swayRotAroundTransform.position - _swayTransform.position);
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Tool_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnPickUp()
	{
		base.OnPickUp();
		ToggleWorldColliders(toEnabled: false);
		_holder.ToolMovement.SetTool(this);
		ShaderManager.SetPlayerColors(_handsMesh, _holder.Skin.SkinColor);
	}

	public override void OnDrop()
	{
		if ((bool)_holder)
		{
			_holder.ToolMovement.ClearTool(this);
		}
		base.OnDrop();
		if ((bool)_handsMesh)
		{
			_handsMesh.enabled = false;
		}
		ToggleWorldColliders(toEnabled: true);
	}

	public bool TryActivateAnimatedHands(Player expectedHolder)
	{
		if (!HandsMesh || !_holder || _holder != expectedHolder || !_holder.Arms || !_handModelRight || !_handModelLeft)
		{
			return false;
		}
		_handsMesh.enabled = true;
		_holder.Arms.SetIKTarget(_handModelRight, _handModelLeft);
		return true;
	}

	[ObserversRpc]
	public void ObserverPlayInspectAnim()
	{
		RpcWriter___ObserverPlayInspectAnim___2166136261();
	}

	protected override void LateUpdate()
	{
		if (!_holder)
		{
			_swayTransform.localPosition = Vector3.Lerp(_swayTransform.localPosition, SwayTransformOffset, 2f * Time.deltaTime);
		}
	}

	protected void PlayInspectAnim(bool calledFromLocal)
	{
		if (_hasInspectAnim && (bool)_holder && (!_holder.Owner.IsLocalClient || calledFromLocal))
		{
			if (calledFromLocal)
			{
				Server.Instance.InspectTool(this);
			}
			AudioSequenceManager.PlayPlayerSequence(_inspectAudioSeq, _holder, base.gameObject);
			_anim.Stop();
			_anim.Play("Inspect");
		}
	}

	protected void PlayIdleAnim()
	{
		_anim.Stop();
		_anim.Play("Idle");
		AudioSequenceManager.CancelAllActiveSequencesFromOwner(base.gameObject);
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(StopIdleAnim());
		}
	}

	private IEnumerator StopIdleAnim()
	{
		yield return new WaitForFixedUpdate();
		if (!_anim.isPlaying)
		{
			_anim.Stop();
			_anim.Play("Idle");
		}
	}

	public override void LoadFromSave(SavedItem savedItem)
	{
		base.LoadFromSave(savedItem);
		_curSkin.Value = savedItem.SkinIndex;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyToolAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyToolAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(2u, RpcReader___ObserverPlayInspectAnim___2166136261);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateToolAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateToolAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverPlayInspectAnim___2166136261()
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		SendObserversRpc(2u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverPlayInspectAnim___2166136261()
	{
		PlayInspectAnim(calledFromLocal: false);
	}

	private void RpcReader___ObserverPlayInspectAnim___2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverPlayInspectAnim___2166136261();
		}
	}

	protected virtual void Awake_UserLogic_Tool_Assembly_002DCSharp_002Edll()
	{
		_tool = this;
		SwayTransformOffset = _swayTransform.localPosition;
		_handsMesh.enabled = false;
		base.Awake();
	}
}
