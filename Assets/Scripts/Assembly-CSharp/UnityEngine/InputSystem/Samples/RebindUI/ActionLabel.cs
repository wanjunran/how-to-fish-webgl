using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class ActionLabel : MonoBehaviour
	{
		[Tooltip("Reference to action that is to be rebound from the UI.")]
		[SerializeField]
		private InputActionReference m_Action;

		[SerializeField]
		private string m_BindingId;

		[SerializeField]
		private InputBinding.DisplayStringOptions m_DisplayStringOptions;

		[Tooltip("Text label that will receive the current, formatted binding string.")]
		[SerializeField]
		private Text m_BindingText;

		[Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying bindings in custom ways, e.g. using images instead of text.")]
		[SerializeField]
		private UpdateBindingUIEvent m_UpdateBindingUIEvent;

		private static List<ActionLabel> s_InputActionUIs;

		public InputActionReference actionReference
		{
			get
			{
				return m_Action;
			}
			set
			{
				m_Action = value;
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

		public Text bindingText
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
					text = inputAction.GetBindingDisplayString(num, out deviceLayoutName, out controlPath, displayStringOptions);
				}
			}
			if (m_BindingText != null)
			{
				m_BindingText.text = text;
			}
			m_UpdateBindingUIEvent?.Invoke(this, text, deviceLayoutName, controlPath);
		}

		protected void OnEnable()
		{
			if (s_InputActionUIs == null)
			{
				s_InputActionUIs = new List<ActionLabel>();
			}
			s_InputActionUIs.Add(this);
			if (s_InputActionUIs.Count == 1)
			{
				InputSystem.onActionChange += OnActionChange;
			}
			UpdateBindingDisplay();
		}

		protected void OnDisable()
		{
			s_InputActionUIs.Remove(this);
			if (s_InputActionUIs.Count == 0)
			{
				s_InputActionUIs = null;
				InputSystem.onActionChange -= OnActionChange;
			}
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
			for (int i = 0; i < s_InputActionUIs.Count; i++)
			{
				ActionLabel actionLabel = s_InputActionUIs[i];
				InputAction inputAction2 = actionLabel.actionReference?.action;
				if (inputAction2 != null && (inputAction2 == inputAction || inputAction2.actionMap == inputActionMap || inputAction2.actionMap?.asset == inputActionAsset))
				{
					actionLabel.UpdateBindingDisplay();
				}
			}
		}
	}
}
