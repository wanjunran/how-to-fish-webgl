using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
	[SerializeField]
	private bool _selectOnShow;

	[SerializeField]
	private bool _selectOnPointerEnter = true;

	[FormerlySerializedAs("_toScale")]
	[FormerlySerializedAs("_effectOn")]
	[SerializeField]
	[HideInInspector]
	private RectTransform _legacyEffectOn;

	[SerializeField]
	[InspectorName("Effect On")]
	private Graphic _effectOnGraphic;

	[SerializeField]
	[HideInInspector]
	private bool _effectOnMigrated;

	[SerializeField]
	private GameObject _selectOnSubmit;

	private void OnValidate()
	{
		if (!_effectOnMigrated)
		{
			if ((bool)_legacyEffectOn)
			{
				_effectOnGraphic = _legacyEffectOn.GetComponent<Graphic>();
			}
			_effectOnMigrated = true;
		}
	}

	private void OnEnable()
	{
		if (_selectOnShow)
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
		else if ((bool)_effectOnGraphic)
		{
			_effectOnGraphic.color = new Color(1f, 1f, 1f, 0.75f);
		}
	}

	private void OnDisable()
	{
		if ((bool)_effectOnGraphic)
		{
			LeanTween.cancel(_effectOnGraphic.gameObject);
			_effectOnGraphic.transform.localScale = Vector3.one;
		}
	}

	private void Hover()
	{
		AudioManager.PlayGlobalClip("Hover", variation: true, 1.25f, 0.05f);
		if ((bool)_effectOnGraphic)
		{
			_effectOnGraphic.color = new Color(1f, 1f, 1f, 1f);
			LeanTween.cancel(_effectOnGraphic.gameObject);
			LeanTween.scale(_effectOnGraphic.gameObject, Vector3.one * 1.1f, 0.25f).setEase(LeanTweenType.easeOutBack);
		}
	}

	private void UnHover()
	{
		if ((bool)_effectOnGraphic)
		{
			_effectOnGraphic.color = new Color(1f, 1f, 1f, 0.75f);
			LeanTween.cancel(_effectOnGraphic.gameObject);
			LeanTween.scale(_effectOnGraphic.gameObject, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutQuart);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Hover();
		if (_selectOnPointerEnter)
		{
			EventSystem.current.SetSelectedGameObject(base.gameObject);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UnHover();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		AudioManager.PlayGlobalClip("Click", variation: true, 1.15f, 0.05f);
	}

	public void OnSelect(BaseEventData eventData)
	{
		Hover();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		UnHover();
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if ((bool)_selectOnSubmit)
		{
			EventSystem.current.SetSelectedGameObject(_selectOnSubmit);
		}
		AudioManager.PlayGlobalClip("Click", variation: true, 1.15f, 0.05f);
	}
}
