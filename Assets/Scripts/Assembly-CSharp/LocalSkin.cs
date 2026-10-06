using UnityEngine;

public class LocalSkin : MonoBehaviour
{
	private static LocalSkin _instance;

	[SerializeField]
	private GameObject _previewHolder;

	[SerializeField]
	private GameObject _beanGuy;

	[SerializeField]
	private Transform _hatTarget;

	[SerializeField]
	private Transform _accessoryTarget;

	[SerializeField]
	private Transform _outfitTarget;

	[Header("Local Skin")]
	[SerializeField]
	private SkinnedMeshRenderer _bodyRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _leftHand;

	[SerializeField]
	private SkinnedMeshRenderer _rightHand;

	[Header("Local Hat/Hair")]
	[SerializeField]
	private SkinnedMeshRenderer _hatRenderer;

	[Header("Local Clothes")]
	[SerializeField]
	private SkinnedMeshRenderer _accessoryRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _outfitRenderer;

	private static Vector3 _localSkinColorPreview;

	private static Vector3 _localHatColorPreview;

	private static Vector3 _localHatColor2Preview;

	private static Vector3 _localHatColor3Preview;

	private static Vector3 _localOutfitColorPreview;

	private static Vector3 _localOutfitColor2Preview;

	private static Vector3 _localOutfitColor3Preview;

	private static Vector3 _localAccessoryColorPreview;

	private static Vector3 _localAccessoryColor2Preview;

	private static Vector3 _localAccessoryColor3Preview;

	public static Transform HatTarget => _instance._hatTarget;

	public static Transform AccessoryTarget => _instance._accessoryTarget;

	public static Transform OutfitTarget => _instance._outfitTarget;

	public static Vector3 LocalSkinColor { get; private set; }

	public static Vector3 LocalHatColor { get; private set; }

	public static Vector3 LocalHatColor2 { get; private set; }

	public static Vector3 LocalHatColor3 { get; private set; }

	public static Vector3 LocalOutfitColor { get; private set; }

	public static Vector3 LocalOutfitColor2 { get; private set; }

	public static Vector3 LocalOutfitColor3 { get; private set; }

	public static Vector3 LocalAccessoryColor { get; private set; }

	public static Vector3 LocalAccessoryColor2 { get; private set; }

	public static Vector3 LocalAccessoryColor3 { get; private set; }

	public static byte LocalHatIndex { get; private set; }

	public static byte LocalOutfitIndex { get; private set; }

	public static byte LocalAccessoryIndex { get; private set; }

	public static bool IsBean { get; private set; }

	public static bool IsInSkinCustomization { get; private set; }

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		Server.OnServerStarted += DisablePreview;
	}

	private void OnDestroy()
	{
		Server.OnServerStarted -= DisablePreview;
	}

	public static void EnablePreview()
	{
		if ((bool)_instance)
		{
			IsInSkinCustomization = true;
		}
	}

	public static void DisablePreview()
	{
		if ((bool)_instance)
		{
			IsInSkinCustomization = false;
		}
	}

	private void UpdatePreview()
	{
		Vector3 skinColor = ((_localSkinColorPreview != Vector3.zero) ? _localSkinColorPreview : LocalSkinColor);
		Vector3 primaryColor = ((_localHatColorPreview != Vector3.zero) ? _localHatColorPreview : LocalHatColor);
		Vector3 secondColor = ((_localHatColor2Preview != Vector3.zero) ? _localHatColor2Preview : LocalHatColor2);
		Vector3 thirdColor = ((_localHatColor3Preview != Vector3.zero) ? _localHatColor3Preview : LocalHatColor3);
		Vector3 primaryColor2 = ((_localAccessoryColorPreview != Vector3.zero) ? _localAccessoryColorPreview : LocalAccessoryColor);
		Vector3 secondColor2 = ((_localAccessoryColor2Preview != Vector3.zero) ? _localAccessoryColor2Preview : LocalAccessoryColor2);
		Vector3 thirdColor2 = ((_localAccessoryColor3Preview != Vector3.zero) ? _localAccessoryColor3Preview : LocalAccessoryColor3);
		Vector3 primaryColor3 = ((_localOutfitColorPreview != Vector3.zero) ? _localOutfitColorPreview : LocalOutfitColor);
		Vector3 secondColor3 = ((_localOutfitColor2Preview != Vector3.zero) ? _localOutfitColor2Preview : LocalOutfitColor2);
		Vector3 thirdColor3 = ((_localOutfitColor3Preview != Vector3.zero) ? _localOutfitColor3Preview : LocalOutfitColor3);
		if ((bool)_bodyRenderer)
		{
			ShaderManager.SetPlayerColors(_bodyRenderer, skinColor);
		}
		if ((bool)_leftHand)
		{
			ShaderManager.SetPlayerColors(_leftHand, skinColor);
		}
		if ((bool)_rightHand)
		{
			ShaderManager.SetPlayerColors(_rightHand, skinColor);
		}
		if ((bool)_hatRenderer)
		{
			ShaderManager.SetPlayerColors(_hatRenderer, skinColor, primaryColor, secondColor, thirdColor);
		}
		if ((bool)_accessoryRenderer)
		{
			ShaderManager.SetPlayerColors(_accessoryRenderer, skinColor, primaryColor2, secondColor2, thirdColor2);
		}
		if ((bool)_outfitRenderer)
		{
			ShaderManager.SetPlayerColors(_outfitRenderer, skinColor, primaryColor3, secondColor3, thirdColor3);
		}
	}

	public static void SetLocalColor(Vector3 to, PlayerSkinType type, bool isPreview = false, bool updatePreview = true)
	{
		switch (type)
		{
		case PlayerSkinType.Skin:
			if (!isPreview)
			{
				LocalSkinColor = to;
			}
			else
			{
				_localSkinColorPreview = to;
			}
			break;
		case PlayerSkinType.Hat:
			if (!isPreview)
			{
				LocalHatColor = to;
			}
			else
			{
				_localHatColorPreview = to;
			}
			break;
		case PlayerSkinType.Hat2:
			if (!isPreview)
			{
				LocalHatColor2 = to;
			}
			else
			{
				_localHatColor2Preview = to;
			}
			break;
		case PlayerSkinType.Hat3:
			if (!isPreview)
			{
				LocalHatColor3 = to;
			}
			else
			{
				_localHatColor3Preview = to;
			}
			break;
		case PlayerSkinType.Outfit:
			if (!isPreview)
			{
				LocalOutfitColor = to;
			}
			else
			{
				_localOutfitColorPreview = to;
			}
			break;
		case PlayerSkinType.Outfit2:
			if (!isPreview)
			{
				LocalOutfitColor2 = to;
			}
			else
			{
				_localOutfitColor2Preview = to;
			}
			break;
		case PlayerSkinType.Outfit3:
			if (!isPreview)
			{
				LocalOutfitColor3 = to;
			}
			else
			{
				_localOutfitColor3Preview = to;
			}
			break;
		case PlayerSkinType.Accessory:
			if (!isPreview)
			{
				LocalAccessoryColor = to;
			}
			else
			{
				_localAccessoryColorPreview = to;
			}
			break;
		case PlayerSkinType.Accessory2:
			if (!isPreview)
			{
				LocalAccessoryColor2 = to;
			}
			else
			{
				_localAccessoryColor2Preview = to;
			}
			break;
		case PlayerSkinType.Accessory3:
			if (!isPreview)
			{
				LocalAccessoryColor3 = to;
			}
			else
			{
				_localAccessoryColor3Preview = to;
			}
			break;
		}
		if ((bool)_instance & updatePreview)
		{
			_instance.UpdatePreview();
		}
	}

	public static Vector3 GetUV(PlayerSkinType type)
	{
		return type switch
		{
			PlayerSkinType.Skin => LocalSkinColor, 
			PlayerSkinType.Hat => LocalHatColor, 
			PlayerSkinType.Hat2 => LocalHatColor2, 
			PlayerSkinType.Hat3 => LocalHatColor3, 
			PlayerSkinType.Outfit => LocalOutfitColor, 
			PlayerSkinType.Outfit2 => LocalOutfitColor2, 
			PlayerSkinType.Outfit3 => LocalOutfitColor3, 
			PlayerSkinType.Accessory => LocalAccessoryColor, 
			PlayerSkinType.Accessory2 => LocalAccessoryColor2, 
			PlayerSkinType.Accessory3 => LocalAccessoryColor3, 
			_ => Vector3.zero, 
		};
	}

	public static void ChangeMesh(int toChange, PlayerSkinType type)
	{
		switch (type)
		{
		case PlayerSkinType.Outfit:
		case PlayerSkinType.Outfit2:
		case PlayerSkinType.Outfit3:
			LocalOutfitIndex = (byte)SkinManager.GetMeshIndex(type, LocalOutfitIndex, toChange);
			SkinManager.OnNewOutfitSelected(LocalOutfitIndex, type);
			if ((bool)_instance)
			{
				_instance._outfitRenderer.sharedMesh = SkinManager.GetOutfit(LocalOutfitIndex);
			}
			SetLocalColor(Vector3.zero, PlayerSkinType.Outfit, isPreview: false, updatePreview: false);
			SetLocalColor(Vector3.zero, PlayerSkinType.Outfit2, isPreview: false, updatePreview: false);
			SetLocalColor(Vector3.zero, PlayerSkinType.Outfit3);
			break;
		case PlayerSkinType.Hat:
		case PlayerSkinType.Hat2:
		case PlayerSkinType.Hat3:
			LocalHatIndex = (byte)SkinManager.GetMeshIndex(type, LocalHatIndex, toChange);
			SkinManager.OnNewOutfitSelected(LocalHatIndex, type);
			if ((bool)_instance)
			{
				_instance._hatRenderer.sharedMesh = SkinManager.GetHat(LocalHatIndex);
			}
			SetLocalColor(Vector3.zero, PlayerSkinType.Hat, isPreview: false, updatePreview: false);
			SetLocalColor(Vector3.zero, PlayerSkinType.Hat2, isPreview: false, updatePreview: false);
			SetLocalColor(Vector3.zero, PlayerSkinType.Hat3);
			break;
		case PlayerSkinType.Accessory:
		case PlayerSkinType.Accessory2:
		case PlayerSkinType.Accessory3:
			LocalAccessoryIndex = (byte)SkinManager.GetMeshIndex(type, LocalAccessoryIndex, toChange);
			SkinManager.OnNewOutfitSelected(LocalAccessoryIndex, type);
			if ((bool)_instance)
			{
				_instance._accessoryRenderer.sharedMesh = SkinManager.GetAccessory(LocalAccessoryIndex);
			}
			SetLocalColor(Vector3.zero, PlayerSkinType.Accessory, isPreview: false, updatePreview: false);
			SetLocalColor(Vector3.zero, PlayerSkinType.Accessory2, isPreview: false, updatePreview: false);
			SetLocalColor(Vector3.zero, PlayerSkinType.Accessory3);
			break;
		}
		ColorPicker.CheckNotifications();
		CanvasManager.UpdateCharacterNotification();
	}

	public static void SetIsBean(bool toBean)
	{
		IsBean = toBean;
		_instance._beanGuy.SetActive(toBean);
		_instance._previewHolder.SetActive(!toBean);
	}
}
