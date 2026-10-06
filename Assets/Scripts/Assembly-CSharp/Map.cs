using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Map : Item
{
	[Header("Map Settings")]
	[SerializeField]
	private RadarUI _radar;

	[Header("Rotation")]
	[SerializeField]
	private RectTransform _mapRotation;

	[FormerlySerializedAs("_holderDot")]
	[Header("Icons Settings")]
	[SerializeField]
	private RectTransform _localPlayerDot;

	[SerializeField]
	private MapDot[] _otherPlayerDots;

	[SerializeField]
	private MapDot[] _islandDots;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	private bool _isOn;

	private Vector2 _localPlayerPos;

	private Vector2 _radarSweepDir;

	private float _curPlayerRot;

	private float _curRadarRot;

	private float _curZoomSpeed;

	private float _curZoom;

	private float _zoomMultiplier;

	private bool _isZoomingIn;

	private bool _isZoomingOut;

	private bool NetworkInitialize___EarlyMapAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateMapAssembly_002DCSharp_002Edll_Excuted;

	public override void PrimaryInput(InputAction.CallbackContext context)
	{
		_radar.ToggleIsZoomingIn(to: true);
	}

	public override void PrimaryInputCancel(InputAction.CallbackContext context)
	{
		_radar.ToggleIsZoomingIn(to: false);
	}

	public override void SecondaryInput(InputAction.CallbackContext context)
	{
		_radar.ToggleIsZoomingOut(to: true);
	}

	public override void SecondaryInputCanceled(InputAction.CallbackContext context)
	{
		_radar.ToggleIsZoomingOut(to: false);
	}

	public override void OnPickUp()
	{
		base.OnPickUp();
		_radar.ToggleIsOn(to: true, base.IsInInventory);
	}

	public override void OnDrop()
	{
		base.OnDrop();
		_radar.ToggleIsOn(to: false, base.IsInInventory);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyMapAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyMapAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateMapAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateMapAssembly_002DCSharp_002Edll_Excuted = true;
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
