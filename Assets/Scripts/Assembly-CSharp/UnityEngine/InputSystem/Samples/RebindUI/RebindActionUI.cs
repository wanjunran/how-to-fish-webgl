using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class RebindActionUI : MonoBehaviour
	{
		[Serializable]
		public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
		{
		}

		[Serializable]
		public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
		{
		}

		[Tooltip("Reference to action that is to be rebound from the UI.")]
		[SerializeField]
		private InputActionReference m_Action;

		[SerializeField]
		private string m_BindingId;

		[SerializeField]
		private InputBinding.DisplayStringOptions m_DisplayStringOptions;

		[Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the rebind UI not show a label for the action.")]
		[SerializeField]
		private TextMeshProUGUI m_ActionLabel;

		[Tooltip("Text label that will receive the current, formatted binding string.")]
		[SerializeField]
		private TextMeshProUGUI m_BindingText;

		[Tooltip("Optional UI that will be shown while a rebind is in progress.")]
		[SerializeField]
		private GameObject m_RebindOverlay;

		[Tooltip("Optional text label that will be updated with prompt for user input.")]
		[SerializeField]
		private TextMeshProUGUI m_RebindText;

		[Tooltip("Optional text label that will be updated with relevant information during rebinding.")]
		[SerializeField]
		private TextMeshProUGUI m_RebindInfo;

		[Tooltip("Optional cancellation UI button for rebinding overlay.")]
		[SerializeField]
		private Button m_RebindCancelButton;

		[Tooltip("Optional rebinding timeout in seconds. If zero, no timeout will be used.")]
		[SerializeField]
		private float m_RebindTimeout;

		[Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying bindings in custom ways, e.g. using images instead of text.")]
		[SerializeField]
		private UpdateBindingUIEvent m_UpdateBindingUIEvent;

		[Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, to implement custom UI behavior while a rebind is in progress. It can also be used to further customize the rebind.")]
		[SerializeField]
		private InteractiveRebindEvent m_RebindStartEvent;

		[Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
		[SerializeField]
		private InteractiveRebindEvent m_RebindStopEvent;

		private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

		private InputAction m_DisabledUISubmitAction;

		private static List<RebindActionUI> s_RebindActionUIs;

		private double m_RebindStartTime = -1.0;

		private int m_LastRemainingTimeoutSeconds;

		public InputActionReference actionReference
		{
			get
			{
				return m_Action;
			}
			set
			{
				m_Action = value;
				UpdateActionLabel();
				UpdateBindingDisplay();
			}
		}

		public string bindingId
		{
			get
			{
				return m_BindingId;
			}
			set
			{
				m_BindingId = value;
				UpdateBindingDisplay();
			}
		}

		public InputBinding.DisplayStringOptions displayStringOptions
		{
			get
			{
				return m_DisplayStringOptions;
			}
			set
			{
				m_DisplayStringOptions = value;
				UpdateBindingDisplay();
			}
		}

		public TextMeshProUGUI actionLabel
		{
			get
			{
				return m_ActionLabel;
			}
			set
			{
				m_ActionLabel = value;
				UpdateActionLabel();
			}
		}

		public TextMeshProUGUI bindingText
		{
			get
			{
				return m_BindingText;
			}
			set
			{
				m_BindingText = value;
				UpdateBindingDisplay();
			}
		}

		public TextMeshProUGUI rebindPrompt
		{
			get
			{
				return m_RebindText;
			}
			set
			{
				m_RebindText = value;
			}
		}

		public TextMeshProUGUI rebindInfo
		{
			get
			{
				return m_RebindInfo;
			}
			set
			{
				m_RebindInfo = value;
			}
		}

		public Button rebindCancelButton
		{
			get
			{
				return m_RebindCancelButton;
			}
			set
			{
				m_RebindCancelButton = value;
			}
		}

		public GameObject rebindOverlay
		{
			get
			{
				return m_RebindOverlay;
			}
			set
			{
				m_RebindOverlay = value;
			}
		}

		public UpdateBindingUIEvent updateBindingUIEvent
		{
			get
			{
				if (m_UpdateBindingUIEvent == null)
				{
					m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
				}
				return m_UpdateBindingUIEvent;
			}
		}

		public InteractiveRebindEvent startRebindEvent
		{
			get
			{
				if (m_RebindStartEvent == null)
				{
					m_RebindStartEvent = new InteractiveRebindEvent();
				}
				return m_RebindStartEvent;
			}
		}

		public InteractiveRebindEvent stopRebindEvent
		{
			get
			{
				if (m_RebindStopEvent == null)
				{
					m_RebindStopEvent = new InteractiveRebindEvent();
				}
				return m_RebindStopEvent;
			}
		}

		public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

		public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
		{
			action = m_Action?.action;
			bindingIndex = action.FindBindingById(m_BindingId);
			if (bindingIndex >= 0)
			{
				return true;
			}
			if (action != null && !string.IsNullOrEmpty(m_BindingId))
			{
				Debug.LogError($"Cannot find binding with ID '{m_BindingId}' on '{action}'", this);
			}
			return false;
		}

		public void UpdateBindingDisplay()
		{
			string text = string.Empty;
			string deviceLayoutName = null;
			string controlPath = null;
			InputAction inputAction = m_Action?.action;
			if (inputAction != null)
			{
				int num = inputAction.bindings.IndexOf((InputBinding x) => x.id.ToString() == m_BindingId);
				if (num != -1)
				{
					if (Application.isPlaying)
					{
						InputBinding binding = inputAction.bindings[num];
						InputSchemeSelection inputScheme = ((!InputBinding.MaskByGroup("Controller").Matches(binding)) ? InputSchemeSelection.ForceKeyboard : InputSchemeSelection.ForceController);
						text = SpriteManager.InputToSpriteName(inputAction, inputScheme, num);
					}
					else
					{
						text = inputAction.GetBindingDisplayString(num, out deviceLayoutName, out controlPath, displayStringOptions);
					}
				}
			}
			if (m_BindingText != null)
			{
				m_BindingText.text = text;
			}
			m_UpdateBindingUIEvent?.Invoke(this, text, deviceLayoutName, controlPath);
		}

		public void ResetToDefault()
		{
			if (!ResolveActionAndBinding(out var action, out var bindingIndex))
			{
				return;
			}
			if (action.bindings[bindingIndex].isComposite)
			{
				for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
				{
					action.RemoveBindingOverride(i);
				}
			}
			else
			{
				action.RemoveBindingOverride(bindingIndex);
			}
			UpdateBindingDisplay();
		}

		public void SwapBinding(RebindActionUI other)
		{
			if (!(this == other))
			{
				if (ongoingRebind != null || other.ongoingRebind != null)
				{
					throw new Exception("Cannot swap bindings when interactive rebinding is ongoing");
				}
				if (!ResolveActionAndBinding(out var action, out var bindingIndex))
				{
					throw new Exception("Failed to resolve action and binding index");
				}
				if (!other.ResolveActionAndBinding(out var action2, out var bindingIndex2))
				{
					throw new Exception("Failed to resolve action and binding index");
				}
				string effectivePath = action.bindings[bindingIndex].effectivePath;
				string effectivePath2 = action2.bindings[bindingIndex2].effectivePath;
				action.ApplyBindingOverride(bindingIndex, effectivePath2);
				action2.ApplyBindingOverride(bindingIndex2, effectivePath);
			}
		}

		public void StartInteractiveRebind()
		{
			if (!ResolveActionAndBinding(out var action, out var bindingIndex))
			{
				return;
			}
			if (action.bindings[bindingIndex].isComposite)
			{
				int num = bindingIndex + 1;
				if (num < action.bindings.Count && action.bindings[num].isPartOfComposite)
				{
					PerformInteractiveRebind(action, num, allCompositeParts: true);
				}
			}
			else
			{
				PerformInteractiveRebind(action, bindingIndex);
			}
		}

		private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
		{
			m_RebindOperation?.Cancel();
			bool actionWasEnabledPriorToRebind = action.enabled;
			if (actionWasEnabledPriorToRebind)
			{
				action.actionMap.Disable();
			}
			m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex).OnCancel((InputActionRebindingExtensions.RebindingOperation operation) =>
			{
				m_RebindStopEvent?.Invoke(this, operation);
				if (m_RebindOverlay != null)
				{
					m_RebindOverlay.SetActive(value: false);
				}
				UpdateBindingDisplay();
				CleanUp();
			}).WithActionEventNotificationsBeingSuppressed()
				.WithTimeout(m_RebindTimeout)
				.OnComplete((InputActionRebindingExtensions.RebindingOperation operation) =>
				{
					if (m_RebindOverlay != null)
					{
						m_RebindOverlay.SetActive(value: false);
					}
					m_RebindStopEvent?.Invoke(this, operation);
					UpdateBindingDisplay();
					CleanUp();
					if (allCompositeParts)
					{
						int num = bindingIndex + 1;
						if (num < action.bindings.Count && action.bindings[num].isPartOfComposite)
						{
							PerformInteractiveRebind(action, num, allCompositeParts: true);
						}
					}
				});
			string text = null;
			if (action.bindings[bindingIndex].isPartOfComposite)
			{
				text = LocalizationManager.BindingLocalized.GetLocalizedString() + " '" + SpriteManager.GetDirectionName(action.bindings[bindingIndex].name) + "'. ";
			}
			m_RebindOverlay?.SetActive(value: true);
			if (m_RebindText != null)
			{
				string text2 = text + LocalizationManager.WaitingForInputLocalized.GetLocalizedString();
				m_RebindText.text = text2;
			}
			if (m_RebindCancelButton != null)
			{
				m_RebindCancelButton.onClick.AddListener(CancelRebind);
			}
			if (m_RebindInfo != null)
			{
				m_RebindStartTime = Time.realtimeSinceStartup;
				UpdateRebindInfo(m_RebindStartTime);
			}
			m_RebindStartEvent?.Invoke(this, m_RebindOperation);
			DisableUISubmitAction();
			m_RebindOperation.Start();
			void CleanUp()
			{
				if (m_RebindCancelButton != null)
				{
					m_RebindCancelButton.onClick.RemoveListener(CancelRebind);
				}
				m_RebindOperation?.Dispose();
				m_RebindOperation = null;
				RestoreUISubmitAction();
				if (actionWasEnabledPriorToRebind)
				{
					action.actionMap.Enable();
				}
			}
		}

		private void DisableUISubmitAction()
		{
			if (EventSystem.current?.currentInputModule is InputSystemUIInputModule inputSystemUIInputModule)
			{
				InputAction inputAction = inputSystemUIInputModule.submit?.action;
				if (inputAction != null && inputAction.enabled)
				{
					m_DisabledUISubmitAction = inputAction;
					m_DisabledUISubmitAction.Disable();
				}
			}
		}

		private void RestoreUISubmitAction()
		{
			if (m_DisabledUISubmitAction != null)
			{
				m_DisabledUISubmitAction.Enable();
				m_DisabledUISubmitAction = null;
			}
		}

		private bool CheckForDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts = false)
		{
			InputBinding inputBinding = action.bindings[bindingIndex];
			foreach (InputBinding binding in action.actionMap.bindings)
			{
				if (!(binding.action == inputBinding.action) && binding.effectivePath == inputBinding.effectivePath)
				{
					Debug.Log("Duplicate binding found: " + inputBinding.effectivePath);
					return true;
				}
			}
			if (allCompositeParts)
			{
				for (int i = 0; i < bindingIndex; i++)
				{
					if (action.bindings[i].effectivePath == inputBinding.overridePath)
					{
						Debug.Log("Duplicate binding found: " + inputBinding.effectivePath);
						return true;
					}
				}
			}
			return false;
		}

		private void UpdateRebindInfo(double now)
		{
			if (m_RebindOperation != null)
			{
				double num = now - m_RebindStartTime;
				int num2 = (int)Math.Floor((double)m_RebindOperation.timeout - num);
				if (num2 != m_LastRemainingTimeoutSeconds)
				{
					string text = ((m_RebindOperation.timeout > 0f) ? $"Cancels in <b>{num2}</b> seconds if no matching input is provided." : string.Empty);
					m_RebindInfo.text = text;
					m_LastRemainingTimeoutSeconds = num2;
				}
			}
		}

		private void CancelRebind()
		{
			m_RebindOperation?.Cancel();
		}

		protected void Update()
		{
			if (m_RebindInfo != null)
			{
				UpdateRebindInfo(Time.realtimeSinceStartupAsDouble);
			}
		}

		protected void OnEnable()
		{
			if (s_RebindActionUIs == null)
			{
				s_RebindActionUIs = new List<RebindActionUI>();
			}
			s_RebindActionUIs.Add(this);
			if (s_RebindActionUIs.Count == 1)
			{
				InputSystem.onActionChange += OnActionChange;
			}
			UpdateBindingDisplay();
		}

		protected void OnDisable()
		{
			m_RebindOperation?.Dispose();
			m_RebindOperation = null;
			RestoreUISubmitAction();
			s_RebindActionUIs.Remove(this);
			if (s_RebindActionUIs.Count == 0)
			{
				s_RebindActionUIs = null;
				InputSystem.onActionChange -= OnActionChange;
			}
			UpdateBindingDisplay();
		}

		private static void OnActionChange(object obj, InputActionChange change)
		{
			if (change != InputActionChange.BoundControlsChanged)
			{
				return;
			}
			InputAction inputAction = obj as InputAction;
			InputActionMap inputActionMap = inputAction?.actionMap ?? (obj as InputActionMap);
			InputActionAsset inputActionAsset = inputActionMap?.asset ?? (obj as InputActionAsset);
			for (int i = 0; i < s_RebindActionUIs.Count; i++)
			{
				RebindActionUI rebindActionUI = s_RebindActionUIs[i];
				InputAction inputAction2 = rebindActionUI.actionReference?.action;
				if (inputAction2 != null && (inputAction2 == inputAction || inputAction2.actionMap == inputActionMap || inputAction2.actionMap?.asset == inputActionAsset))
				{
					rebindActionUI.UpdateBindingDisplay();
				}
			}
		}

		private void UpdateActionLabel()
		{
			if (m_ActionLabel != null)
			{
				InputAction inputAction = m_Action?.action;
				m_ActionLabel.text = ((inputAction != null) ? inputAction.name : string.Empty);
			}
		}
	}
}
