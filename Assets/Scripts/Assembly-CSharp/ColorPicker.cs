using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPicker : Selectable, IPointerMoveHandler, IEventSystemHandler, IPointerClickHandler, ISubmitHandler, ICancelHandler
{
	private static ColorPicker _instance;

	[SerializeField]
	private RectTransform _rect;

	[SerializeField]
	private Image _image;

	[SerializeField]
	[Range(0f, 1f)]
	private float _idleAlpha = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _selectedAlpha = 0.75f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _pickingAlpha = 1f;

	[SerializeField]
	private RectTransform _hoverPicker;

	[SerializeField]
	private RectTransform _selectPicker;

	[SerializeField]
	private GameObject _colorLayout;

	[Header("Text Buttons")]
	[SerializeField]
	private CanvasGroup _skinButton;

	[SerializeField]
	private CanvasGroup _hatButton;

	[SerializeField]
	private CanvasGroup _outfitButton;

	[SerializeField]
	private CanvasGroup _accessoryButton;

	[SerializeField]
	private CanvasGroup _color1Button;

	[SerializeField]
	private CanvasGroup _color2Button;

	[SerializeField]
	private CanvasGroup _color3Button;

	[SerializeField]
	private float _pickerSpeed = 5f;

	[SerializeField]
	private GameObject _hatNotification;

	[SerializeField]
	private GameObject _outfitNotification;

	[SerializeField]
	private GameObject _accessoryNotification;

	private const int _pixels = 32;

	private static float _pixelSize;

	private static float _halfPixel;

	private static Vector2 _startPos;

	private Vector2 _uv;

	private bool _isHovering;

	private bool _isSelected;

	private bool _controllerPicking;

	private static int _colorIndex;

	private static ColoringPart _coloringPart;

	private Vector3 _prevUv;

	private static float[] randomYValues = new float[8] { 0.2f, 0.23f, 0.45f, 0.48f, 0.7f, 0.73f, 0.95f, 0.98f };

	protected override void Awake()
	{
		base.Awake();
		_instance = this;
		_startPos = Vector2.one * (0f - _rect.sizeDelta.x) / 2f;
		_pixelSize = 1f / 32f;
		_halfPixel = _pixelSize * 0.5f;
		_selectPicker.gameObject.SetActive(value: false);
		_hoverPicker.gameObject.SetActive(value: false);
		UpdateImageAlpha();
		SetColoringPart(ColoringPart.Skin);
	}

	protected override void Start()
	{
		base.Start();
		CheckNotifications();
	}

	public static void CheckNotifications()
	{
		if ((bool)_instance)
		{
			_instance._hatNotification.SetActive(SkinManager.HasUnSeenSpecificClothing(PlayerSkinType.Hat));
			_instance._accessoryNotification.SetActive(SkinManager.HasUnSeenSpecificClothing(PlayerSkinType.Accessory));
			_instance._outfitNotification.SetActive(SkinManager.HasUnSeenSpecificClothing(PlayerSkinType.Outfit));
		}
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out var localPoint))
		{
			_uv = GetUVStepped(localPoint);
			if (_prevUv != new Vector3(_uv.x, _uv.y, 1f) && _isHovering)
			{
				PreviewColor();
			}
			_prevUv = _uv;
		}
	}

	private void PreviewColor()
	{
		LocalSkin.SetLocalColor(new Vector3(_uv.x, _uv.y, 1f), GetTypeFromColoringPart(), isPreview: true);
		Vector2 vector = _startPos + _uv * _rect.sizeDelta.x;
		_hoverPicker.gameObject.SetActive(value: true);
		LeanTween.cancel(_hoverPicker.gameObject);
		LeanTween.moveLocal(_hoverPicker.gameObject, vector, _pickerSpeed).setEase(LeanTweenType.easeOutBack);
	}

	private Vector2 GetUVStepped(Vector2 localPos)
	{
		Rect rect = _rect.rect;
		float value = (localPos.x - rect.x) / rect.width;
		float value2 = (localPos.y - rect.y) / rect.height;
		float num = Mathf.Clamp(value, 0f, 1f - _pixelSize);
		value2 = Mathf.Clamp(value2, 0f, 1f - _pixelSize);
		float f = num * 32f;
		value2 *= 32f;
		float num2 = Mathf.FloorToInt(f);
		value2 = Mathf.FloorToInt(value2);
		float num3 = num2 / 32f;
		value2 /= 32f;
		float x = num3 + _halfPixel;
		value2 += _halfPixel;
		return new Vector2(x, value2);
	}

	public static void UpdateSelectedPicker()
	{
		if ((bool)_instance)
		{
			Vector3 uV = LocalSkin.GetUV(GetTypeFromColoringPart());
			_instance._selectPicker.gameObject.SetActive(uV.z == 1f);
			Vector2 vector = _startPos + new Vector2(uV.x, uV.y) * _instance._rect.sizeDelta.x;
			_instance._selectPicker.localPosition = vector;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		SelectColor();
	}

	private void SelectColor()
	{
		Vector2 vector = _startPos + _uv * _rect.sizeDelta.x;
		_selectPicker.localPosition = vector;
		_selectPicker.gameObject.SetActive(value: true);
		LocalSkin.SetLocalColor(new Vector3(_uv.x, _uv.y, 1f), GetTypeFromColoringPart());
		UpdateSelectedPicker();
	}

	public override void OnMove(AxisEventData eventData)
	{
		if (!_controllerPicking)
		{
			base.OnMove(eventData);
			return;
		}
		Vector2 vector = eventData.moveDir switch
		{
			MoveDirection.Left => Vector2.left, 
			MoveDirection.Right => Vector2.right, 
			MoveDirection.Up => Vector2.up, 
			MoveDirection.Down => Vector2.down, 
			_ => Vector2.zero, 
		};
		if (!(vector == Vector2.zero))
		{
			_uv += vector * _pixelSize;
			_uv.x = Mathf.Clamp(_uv.x, _halfPixel, 1f - _halfPixel);
			_uv.y = Mathf.Clamp(_uv.y, _halfPixel, 1f - _halfPixel);
			PreviewColor();
			_prevUv = new Vector3(_uv.x, _uv.y, 1f);
			eventData.Use();
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (!_controllerPicking)
		{
			Vector3 uV = LocalSkin.GetUV(GetTypeFromColoringPart());
			_uv = ((uV.z == 1f) ? new Vector2(uV.x, uV.y) : GetUVStepped(Vector2.zero));
			_controllerPicking = true;
			UpdateImageAlpha();
			PreviewColor();
		}
		else
		{
			SelectColor();
			StopControllerPicking(resetPreview: false);
		}
		eventData.Use();
	}

	public void OnCancel(BaseEventData eventData)
	{
		if (_controllerPicking)
		{
			StopControllerPicking(resetPreview: true);
			eventData.Use();
		}
	}

	private void StopControllerPicking(bool resetPreview)
	{
		_controllerPicking = false;
		UpdateImageAlpha();
		if (resetPreview)
		{
			LocalSkin.SetLocalColor(Vector3.zero, GetTypeFromColoringPart(), isPreview: true);
		}
		LeanTween.cancel(_hoverPicker.gameObject);
		_hoverPicker.gameObject.SetActive(value: false);
	}

	private static PlayerSkinType GetTypeFromColoringPart()
	{
		return _coloringPart switch
		{
			ColoringPart.Skin => PlayerSkinType.Skin, 
			ColoringPart.Hat => _colorIndex switch
			{
				0 => PlayerSkinType.Hat, 
				1 => PlayerSkinType.Hat2, 
				_ => PlayerSkinType.Hat3, 
			}, 
			ColoringPart.Outfit => _colorIndex switch
			{
				0 => PlayerSkinType.Outfit, 
				1 => PlayerSkinType.Outfit2, 
				_ => PlayerSkinType.Outfit3, 
			}, 
			ColoringPart.Accessory => _colorIndex switch
			{
				0 => PlayerSkinType.Accessory, 
				1 => PlayerSkinType.Accessory2, 
				_ => PlayerSkinType.Accessory3, 
			}, 
			_ => PlayerSkinType.Skin, 
		};
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		_isHovering = true;
		UpdateImageAlpha();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		_isHovering = false;
		UpdateImageAlpha();
		if (!_controllerPicking)
		{
			StopControllerPicking(resetPreview: true);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		_isSelected = true;
		UpdateImageAlpha();
		base.OnSelect(eventData);
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		_isSelected = false;
		UpdateImageAlpha();
		if (_controllerPicking)
		{
			StopControllerPicking(resetPreview: true);
		}
		base.OnDeselect(eventData);
	}

	private void UpdateImageAlpha()
	{
		if ((bool)_image)
		{
			Color color = _image.color;
			color.a = (_isHovering ? 1f : (_controllerPicking ? _pickingAlpha : (_isSelected ? _selectedAlpha : _idleAlpha)));
			_image.color = color;
		}
	}

	public static void SetColorIndex(int to)
	{
		_colorIndex = to;
		if ((bool)_instance)
		{
			_instance._color1Button.alpha = 0.5f;
			_instance._color2Button.alpha = 0.5f;
			_instance._color3Button.alpha = 0.5f;
			switch (to)
			{
			case 0:
				_instance._color1Button.alpha = 1f;
				break;
			case 1:
				_instance._color2Button.alpha = 1f;
				break;
			case 2:
				_instance._color3Button.alpha = 1f;
				break;
			}
			Vector3 uV = LocalSkin.GetUV(GetTypeFromColoringPart());
			if (uV != Vector3.zero)
			{
				_instance._selectPicker.gameObject.SetActive(value: true);
				_instance._selectPicker.localPosition = _startPos + new Vector2(uV.x, uV.y) * _instance._rect.sizeDelta.x;
			}
			else
			{
				_instance._selectPicker.gameObject.SetActive(value: false);
			}
		}
	}

	public static void SetColoringPart(ColoringPart part)
	{
		_coloringPart = part;
		SetColorIndex(0);
		if ((bool)_instance)
		{
			_instance._skinButton.alpha = 0.5f;
			_instance._hatButton.alpha = 0.5f;
			_instance._outfitButton.alpha = 0.5f;
			_instance._accessoryButton.alpha = 0.5f;
			switch (part)
			{
			case ColoringPart.Skin:
				_instance._skinButton.alpha = 1f;
				break;
			case ColoringPart.Hat:
				_instance._hatButton.alpha = 1f;
				break;
			case ColoringPart.Outfit:
				_instance._outfitButton.alpha = 1f;
				break;
			case ColoringPart.Accessory:
				_instance._accessoryButton.alpha = 1f;
				break;
			}
			_instance._colorLayout.SetActive(part != ColoringPart.Skin);
			UpdateSelectedPicker();
		}
	}

	public static void RandomizeSelected()
	{
		LocalSkin.SetLocalColor(GetRandomUV(), GetTypeFromColoringPart());
		UpdateSelectedPicker();
	}

	public static void ResetSelected()
	{
		LocalSkin.SetLocalColor(Vector3.zero, GetTypeFromColoringPart());
		UpdateSelectedPicker();
	}

	public static void RandomizeSkin()
	{
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Skin, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Hat, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Hat2, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Hat3, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Outfit, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Outfit2, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Outfit3, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Accessory, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Accessory2, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Accessory3);
		UpdateSelectedPicker();
	}

	public static void ResetSkin()
	{
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Skin, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Hat, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Hat2, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Hat3, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Outfit, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Outfit2, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Outfit3, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Accessory, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(Vector3.zero, PlayerSkinType.Accessory2, isPreview: false, updatePreview: false);
		LocalSkin.SetLocalColor(GetRandomUV(), PlayerSkinType.Accessory3);
		UpdateSelectedPicker();
	}

	private static Vector3 GetRandomUV()
	{
		float x = Random.Range(0f, 1f - _pixelSize);
		float y = randomYValues[Random.Range(0, randomYValues.Length)];
		return new Vector3(x, y, 1f);
	}
}
