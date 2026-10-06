using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerSkin : NetworkBehaviour
{
	[Header("Skin")]
	[SerializeField]
	private SkinnedMeshRenderer _localLeftHand;

	[SerializeField]
	private SkinnedMeshRenderer _localRightHand;

	[Space]
	[SerializeField]
	private SkinnedMeshRenderer _bodyRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _leftHand;

	[SerializeField]
	private SkinnedMeshRenderer _rightHand;

	[Space]
	[SerializeField]
	private SkinnedMeshRenderer _outfitRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _hatRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _accessoryRenderer;

	public readonly SyncVar<Vector3> _skinColor = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _hatColor = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _hatColor2 = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _hatColor3 = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _outfitColor = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _outfitColor2 = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _outfitColor3 = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _accessoryColor = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _accessoryColor2 = new SyncVar<Vector3>();

	public readonly SyncVar<Vector3> _accessoryColor3 = new SyncVar<Vector3>();

	public readonly SyncVar<byte> _hatMeshIndex = new SyncVar<byte>();

	public readonly SyncVar<byte> _outfitMeshIndex = new SyncVar<byte>();

	public readonly SyncVar<byte> _accessoryMeshIndex = new SyncVar<byte>();

	private bool NetworkInitialize___EarlyPlayerSkinAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerSkinAssembly_002DCSharp_002Edll_Excuted;

	public Vector3 SkinColor => _skinColor.Value;

	public Vector3 HatColor => _hatColor.Value;

	public Vector3 HatColor2 => _hatColor2.Value;

	public Vector3 HatColor3 => _hatColor3.Value;

	public Vector3 OutfitColor => _outfitColor.Value;

	public Vector3 OutfitColor2 => _outfitColor2.Value;

	public Vector3 OutfitColor3 => _outfitColor3.Value;

	public Vector3 AccessoryColor => _accessoryColor.Value;

	public Vector3 AccessoryColor2 => _accessoryColor2.Value;

	public Vector3 AccessoryColor3 => _accessoryColor3.Value;

	public byte HatMeshIndex => _hatMeshIndex.Value;

	public byte OutfitMeshIndex => _outfitMeshIndex.Value;

	public byte AccessoryMeshIndex => _accessoryMeshIndex.Value;

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			InitializeLocal();
		}
		else
		{
			InitializeOther();
		}
	}

	private void InitializeLocal()
	{
		if ((bool)_localLeftHand)
		{
			ShaderManager.SetPlayerColors(_localLeftHand, _skinColor.Value);
		}
		if ((bool)_localRightHand)
		{
			ShaderManager.SetPlayerColors(_localRightHand, _skinColor.Value);
		}
	}

	private void InitializeOther()
	{
		if ((bool)_bodyRenderer)
		{
			ShaderManager.SetPlayerColors(_bodyRenderer, _skinColor.Value);
		}
		if ((bool)_leftHand)
		{
			ShaderManager.SetPlayerColors(_leftHand, _skinColor.Value);
		}
		if ((bool)_rightHand)
		{
			ShaderManager.SetPlayerColors(_rightHand, _skinColor.Value);
		}
		if ((bool)_hatRenderer)
		{
			_hatRenderer.sharedMesh = SkinManager.GetHat(_hatMeshIndex.Value);
			ShaderManager.SetPlayerColors(_hatRenderer, _skinColor.Value, _hatColor.Value, _hatColor2.Value, _hatColor3.Value);
		}
		if ((bool)_accessoryRenderer)
		{
			_accessoryRenderer.sharedMesh = SkinManager.GetAccessory(_accessoryMeshIndex.Value);
			ShaderManager.SetPlayerColors(_accessoryRenderer, _skinColor.Value, _accessoryColor.Value, _accessoryColor2.Value, _accessoryColor3.Value);
		}
		if ((bool)_outfitRenderer)
		{
			_outfitRenderer.sharedMesh = SkinManager.GetOutfit(_outfitMeshIndex.Value);
			ShaderManager.SetPlayerColors(_outfitRenderer, _skinColor.Value, _outfitColor.Value, _outfitColor2.Value, _outfitColor3.Value);
		}
	}

	public void SetServerColors(Vector3 skinColor, Vector3 outfitColor, Vector3 outfitColor2, Vector3 outfitColor3, Vector3 hatColor, Vector3 hatColor2, Vector3 hatColor3, Vector3 accessoryColor, Vector3 accessoryColor2, Vector3 accessoryColor3, byte hatIndex, byte outfitIndex, byte accessoryIndex, bool isBean)
	{
		_skinColor.Value = skinColor;
		_outfitColor.Value = outfitColor;
		_outfitColor2.Value = outfitColor2;
		_outfitColor3.Value = outfitColor3;
		_hatColor.Value = hatColor;
		_hatColor2.Value = hatColor2;
		_hatColor3.Value = hatColor3;
		_accessoryColor.Value = accessoryColor;
		_accessoryColor2.Value = accessoryColor2;
		_accessoryColor3.Value = accessoryColor3;
		_hatMeshIndex.Value = hatIndex;
		_outfitMeshIndex.Value = outfitIndex;
		_accessoryMeshIndex.Value = accessoryIndex;
	}

	public void SendSkinToDeadPlayer(DeadPlayer deadPlayer)
	{
		deadPlayer.SetSkin(_skinColor.Value, _hatColor.Value, _hatColor2.Value, _hatColor3.Value, _outfitColor.Value, _outfitColor2.Value, _outfitColor3.Value, _accessoryColor.Value, _accessoryColor2.Value, _accessoryColor3.Value, _hatMeshIndex.Value, _outfitMeshIndex.Value, _accessoryMeshIndex.Value);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerSkinAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerSkinAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_accessoryMeshIndex.InitializeEarly(this, 12u, isSyncObject: false);
			_outfitMeshIndex.InitializeEarly(this, 11u, isSyncObject: false);
			_hatMeshIndex.InitializeEarly(this, 10u, isSyncObject: false);
			_accessoryColor3.InitializeEarly(this, 9u, isSyncObject: false);
			_accessoryColor2.InitializeEarly(this, 8u, isSyncObject: false);
			_accessoryColor.InitializeEarly(this, 7u, isSyncObject: false);
			_outfitColor3.InitializeEarly(this, 6u, isSyncObject: false);
			_outfitColor2.InitializeEarly(this, 5u, isSyncObject: false);
			_outfitColor.InitializeEarly(this, 4u, isSyncObject: false);
			_hatColor3.InitializeEarly(this, 3u, isSyncObject: false);
			_hatColor2.InitializeEarly(this, 2u, isSyncObject: false);
			_hatColor.InitializeEarly(this, 1u, isSyncObject: false);
			_skinColor.InitializeEarly(this, 0u, isSyncObject: false);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerSkinAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerSkinAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_accessoryMeshIndex.InitializeLate();
			_outfitMeshIndex.InitializeLate();
			_hatMeshIndex.InitializeLate();
			_accessoryColor3.InitializeLate();
			_accessoryColor2.InitializeLate();
			_accessoryColor.InitializeLate();
			_outfitColor3.InitializeLate();
			_outfitColor2.InitializeLate();
			_outfitColor.InitializeLate();
			_hatColor3.InitializeLate();
			_hatColor2.InitializeLate();
			_hatColor.InitializeLate();
			_skinColor.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}
}
