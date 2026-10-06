using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class TutorialBox : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private LocalizeStringEvent _titleEvent;

	[SerializeField]
	private LocalizeStringEvent _descriptionEvent;

	[SerializeField]
	private TextMeshProUGUI _titleText;

	[SerializeField]
	private TextMeshProUGUI _descriptionText;

	[SerializeField]
	private Color _finishedColor;

	[SerializeField]
	private float _extraSizeY = 35f;

	private Tutorial _tutorial;

	private float _targetSize;

	private RectTransform _rectParent;

	private bool _isFinished;

	public void Initialize(Tutorial tutorial, RectTransform parent)
	{
		_tutorial = tutorial;
		_rectParent = parent;
		RefreshText();
	}

	private void RefreshText()
	{
		_titleEvent.StringReference = _tutorial.TutorialTitle;
		_titleEvent.RefreshString();
		_descriptionEvent.StringReference = _tutorial.TutorialDescription;
		if (_tutorial.InputActions != null)
		{
			object[] array = new object[_tutorial.InputActions.Length];
			for (int i = 0; i < _tutorial.InputActions.Length; i++)
			{
				string text = SpriteManager.InputToSpriteName(_tutorial.InputActions[i].action) ?? "";
				array[i] = text;
			}
			_descriptionEvent.StringReference.Arguments = array;
		}
		if (_descriptionEvent.StringReference != null)
		{
			_descriptionEvent.RefreshString();
		}
	}

	private void OnEnable()
	{
		if (_isFinished)
		{
			Object.Destroy(base.gameObject);
		}
		_targetSize = _descriptionText.rectTransform.sizeDelta.y + _extraSizeY;
		LeanTween.cancel(_descriptionText.gameObject);
		LeanTween.value(_descriptionText.gameObject, 0f, _descriptionText.rectTransform.sizeDelta.y + _extraSizeY, 0.5f).setOnUpdate(OnSizeExpand);
	}

	private void OnSizeExpand(float sizeY)
	{
		if (!Mathf.Approximately(_targetSize, _descriptionText.rectTransform.sizeDelta.y + _extraSizeY))
		{
			LeanTween.cancel(_descriptionText.gameObject);
			LeanTween.value(_descriptionText.gameObject, _image.rectTransform.sizeDelta.y, _descriptionText.rectTransform.sizeDelta.y + _extraSizeY, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(OnSizeExpand);
			_targetSize = _descriptionText.rectTransform.sizeDelta.y + _extraSizeY;
		}
		else
		{
			_image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x, sizeY);
		}
	}

	public void Finish()
	{
		_isFinished = true;
		base.gameObject.SetActive(value: true);
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(FinishAnimation());
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator FinishAnimation()
	{
		bool flag = false;
		if ((bool)_tutorial)
		{
			if (_tutorial.OnFinishedTitle != null)
			{
				_titleEvent.StringReference = _tutorial.OnFinishedTitle;
				_titleEvent.RefreshString();
				flag = true;
			}
			if (_tutorial.OnFinishedDescription != null)
			{
				_descriptionEvent.StringReference = _tutorial.OnFinishedDescription;
				_descriptionEvent.RefreshString();
				flag = true;
			}
		}
		_image.color = _finishedColor;
		if (flag)
		{
			yield return new WaitForSeconds(3f);
		}
		LeanTween.cancel(base.gameObject);
		LeanTween.value(base.gameObject, _image.rectTransform.sizeDelta.y, 0f, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(OnSizeShrink);
		yield return new WaitForSeconds(0.5f);
		Object.Destroy(base.gameObject);
	}

	private void OnSizeShrink(float sizeY)
	{
		_image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x, sizeY);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectParent);
	}
}
