using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class RebindActionParameterUI : MonoBehaviour
	{
		[Tooltip("Reference to action that holds the parameter to be configurable via this behaviour.")]
		[SerializeField]
		private InputActionReference m_Action;

		[Tooltip("Optional binding ID of the binding processor parameter to override.")]
		[SerializeField]
		private string m_BindingId;

		[Tooltip("The player preference key to be used for persistence.")]
		[SerializeField]
		private string m_PreferenceKey;

		[Tooltip("The default value to be be used when no preference exists or when resetting")]
		[SerializeField]
		private float m_DefaultValue;

		[Tooltip("The associated slider UI component used to change the value.")]
		[SerializeField]
		private Slider m_Slider;

		[SerializeField]
		private string[] m_ParameterOverrides;

		private float m_Value;

		public InputActionReference actionReference
		{
			get
			{
				return m_Action;
			}
			set
			{
				m_Action = value;
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
			}
		}

		public string preferenceKey
		{
			get
			{
				return m_PreferenceKey;
			}
			set
			{
				m_PreferenceKey = value;
			}
		}

		public Slider slider
		{
			get
			{
				return m_Slider;
			}
			set
			{
				if (m_Slider != null)
				{
					m_Slider.onValueChanged.RemoveListener(SetParameterValue);
				}
				m_Slider = value;
				if (value != null)
				{
					value.onValueChanged.AddListener(SetParameterValue);
				}
			}
		}

		public float defaultValue
		{
			get
			{
				return m_DefaultValue;
			}
			set
			{
				m_DefaultValue = value;
			}
		}

		public void ResetToDefault()
		{
			PlayerPrefs.SetFloat(m_PreferenceKey, m_DefaultValue);
			SetParameterValue(m_DefaultValue);
		}

		private void Awake()
		{
			if (m_Slider == null)
			{
				m_Slider = GetComponent<Slider>();
			}
		}

		private void OnEnable()
		{
			if (!string.IsNullOrEmpty(m_PreferenceKey))
			{
				SetParameterValue(PlayerPrefs.GetFloat(m_PreferenceKey, m_DefaultValue));
			}
			if (m_Slider != null)
			{
				m_Slider.onValueChanged.AddListener(SetParameterValue);
			}
		}

		private void OnDisable()
		{
			if (m_Slider != null)
			{
				m_Slider.onValueChanged.RemoveListener(SetParameterValue);
			}
			if (!string.IsNullOrEmpty(m_PreferenceKey))
			{
				PlayerPrefs.SetFloat(m_PreferenceKey, m_Value);
			}
		}

		private void SetParameterValue(float value)
		{
			if (m_Action != null && m_Action.action != null)
			{
				InputAction action = m_Action.action;
				int num = action.FindBindingById(m_BindingId);
				InputBinding bindingMask = ((num >= 0) ? action.bindings[num] : default(InputBinding));
				string[] parameterOverrides = m_ParameterOverrides;
				foreach (string text in parameterOverrides)
				{
					action.ApplyParameterOverride(text, value, bindingMask);
				}
			}
			m_Value = value;
			UpdateDisplayValue(value);
		}

		private void UpdateDisplayValue(float value)
		{
			if (m_Slider != null)
			{
				m_Slider.value = Mathf.Clamp(value, m_Slider.minValue, m_Slider.maxValue);
			}
		}
	}
}
