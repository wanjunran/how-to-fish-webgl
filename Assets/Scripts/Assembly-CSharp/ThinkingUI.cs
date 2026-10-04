using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;

public class ThinkingUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _thinkingCanvas;

	[SerializeField]
	private TextMeshProUGUI _progressText;

	[SerializeField]
	private TextMeshProUGUI _dripProgressText;

	[SerializeField]
	private CanvasGroup _thinkingHolderGroup;

	[SerializeField]
	private Camera _journalCamera;

	[SerializeField]
	private List<JournalSlot> _slots;

	[SerializeField]
	private float _slotAnimDelay = 0.1f;

	[SerializeField]
	private LocalizeStringEvent _closeEvent;

	[SerializeField]
	private InputActionReference _closeInput;

	[Header("Cloudy Animation")]
	[SerializeField]
	private List<Transform> _cloudlyTransforms;

	[SerializeField]
	private AnimationCurve _cloudyCurveX;

	[SerializeField]
	private AnimationCurve _cloudyCurveY;

	[SerializeField]
	private float _animSpeed = 1f;

	[SerializeField]
	private float _offsetStrength = 1f;

	[SerializeField]
	private float _animStrength = 1f;

	private int _curPage;

	private float _animTime;

	private float _thinkingAlpha;

	private byte _animID;

	private void Awake()
	{
		_journalCamera.clearFlags = CameraClearFlags.Color;
		_journalCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		_journalCamera.enabled = false;
		_thinkingHolderGroup.alpha = 0f;
	}

	public void InitializeLocal()
	{
		_journalCamera.targetTexture = GameInfo.JournalTexture;
		GameInfo.Input.onControlsChanged += OnControlsChanged;
		SetupCloseInputText();
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		if ((bool)GameInfo.Input)
		{
			GameInfo.Input.onControlsChanged -= OnControlsChanged;
		}
	}

	private void OnControlsChanged(PlayerInput playerInput)
	{
		SetupCloseInputText();
	}

	private void SetupCloseInputText()
	{
		object[] arguments = new object[1] { SpriteManager.InputToSpriteName(_closeInput.action) };
		_closeEvent.StringReference = LocalizationManager.CloseLocalized;
		_closeEvent.StringReference.Arguments = arguments;
		_closeEvent.RefreshString();
	}

	private void Update()
	{
		if (PlayerThinking.IsThinking)
		{
			CloudyAnimations();
		}
	}

	private void CloudyAnimations()
	{
		foreach (Transform cloudlyTransform in _cloudlyTransforms)
		{
			Vector3 position = cloudlyTransform.parent.transform.position;
			float num = position.x + position.y;
			float time = Mathf.Repeat(_animTime + num * _offsetStrength, 1f);
			Vector3 vector = new Vector3(_cloudyCurveX.Evaluate(time), _cloudyCurveY.Evaluate(time), 0f);
			cloudlyTransform.localPosition = vector * _animStrength;
		}
		_animTime += Time.deltaTime * _animSpeed;
	}

	private IEnumerator UpdateJournal()
	{
		_animID++;
		byte id = _animID;
		for (int i = 0; i < _slots.Count; i++)
		{
			Creature creature = GameInfo.GetCreature(_curPage * _slots.Count + i);
			_slots[i].SetCreature(creature);
		}
		UpdateJournalCamera();
		for (int j = 0; j < _slots.Count; j++)
		{
			_slots[j].ShowSlot();
			yield return new WaitForSeconds(_slotAnimDelay);
			if (_animID != id)
			{
				break;
			}
		}
	}

	private void UpdateJournalCamera()
	{
		_journalCamera.Render();
	}

	public void ToggleThinking()
	{
		float to = (PlayerThinking.IsThinking ? 1 : 0);
		_thinkingCanvas.alpha = 1f;
		if (PlayerThinking.IsThinking)
		{
			StartCoroutine(UpdateJournal());
		}
		if (PlayerThinking.IsThinking)
		{
			SetProgressTexts();
		}
		LeanTweenType ease = (PlayerThinking.IsThinking ? LeanTweenType.easeOutSine : LeanTweenType.easeInSine);
		LeanTween.cancel(_thinkingCanvas.gameObject);
		LeanTween.value(_thinkingCanvas.gameObject, _thinkingAlpha, to, 0.5f).setEase(ease).setOnUpdate(SetThinkingAlpha)
			.setOnComplete(OnThinkingComplete);
	}

	private void SetProgressTexts()
	{
		int num = GameInfo.KilledCreaturesCount(checkDrip: false);
		int num2 = GameInfo.KilledCreaturesCount(checkDrip: true);
		_progressText.text = $"{num} / {GameInfo.AllCreatureCount} {LocalizationManager.CreaturesLocalized.GetLocalizedString()}";
		_dripProgressText.text = $"{num2} / {GameInfo.AllCreatureCount} {LocalizationManager.DripCreaturesLocalized.GetLocalizedString()}";
		bool flag = num == GameInfo.AllCreatureCount;
		bool flag2 = num2 == GameInfo.AllCreatureCount;
		_progressText.color = (flag ? GameInfo.OrangeColor : Color.white);
		_dripProgressText.color = (flag2 ? GameInfo.OrangeColor : Color.white);
	}

	private void SetThinkingAlpha(float to)
	{
		_thinkingAlpha = to;
		_thinkingHolderGroup.alpha = to;
		ShaderManager.SetThinkingAlpha(to);
	}

	private void OnThinkingComplete()
	{
		if (!PlayerThinking.IsThinking)
		{
			_thinkingCanvas.alpha = 0f;
		}
	}

	public void ScrollThinkingPage(int input)
	{
		_curPage += input;
		if (_curPage * _slots.Count >= GameInfo.AllCreatureCount)
		{
			_curPage = 0;
		}
		else if (_curPage < 0)
		{
			_curPage = (GameInfo.AllCreatureCount - 1) / 10;
		}
		StartCoroutine(UpdateJournal());
	}
}
