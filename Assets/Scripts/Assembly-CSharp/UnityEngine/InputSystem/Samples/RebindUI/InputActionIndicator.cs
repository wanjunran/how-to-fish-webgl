using System;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	[RequireComponent(typeof(Image))]
	public class InputActionIndicator : MonoBehaviour
	{
		[Tooltip("Reference to the associated action to be visualized.")]
		public InputActionReference action;

		[Tooltip("The color to show when the associated action is performed.")]
		public Color activeColor = Color.green;

		[Tooltip("The color to show when the associated action has not been performed for the specified duration.")]
		public Color inactiveColor = Color.black;

		[Tooltip("The color to show when the associated action is disabled")]
		public Color disabledColor = Color.red;

		[Tooltip("The duration for which the indicator should be lit before becoming completely inactive.")]
		public float duration = 1f;

		public Image performedIndicator;

		public Image pressedIndicator;

		public Text label;

		private double m_RealTimeLastPerformed;

		private void OnEnable()
		{
			if (action != null && action.action != null)
			{
				action.action.performed += OnPerformed;
			}
		}

		private void OnDisable()
		{
			if (action != null && action.action != null)
			{
				action.action.performed -= OnPerformed;
			}
		}

		private void OnPerformed(InputAction.CallbackContext obj)
		{
			m_RealTimeLastPerformed = Time.realtimeSinceStartupAsDouble;
		}

		private void Update()
		{
			if (action.action.enabled)
			{
				double num = Time.realtimeSinceStartupAsDouble - m_RealTimeLastPerformed;
				if ((bool)performedIndicator)
				{
					performedIndicator.color = ((duration <= 0f) ? inactiveColor : Color.Lerp(inactiveColor, activeColor, (float)Math.Max(0.0, 1.0 - num / (double)duration)));
				}
				if ((bool)pressedIndicator)
				{
					pressedIndicator.color = (action.action.IsPressed() ? activeColor : inactiveColor);
				}
			}
			else
			{
				if ((bool)performedIndicator && performedIndicator.color != disabledColor)
				{
					performedIndicator.color = disabledColor;
				}
				if ((bool)pressedIndicator && pressedIndicator.color != disabledColor)
				{
					pressedIndicator.color = disabledColor;
				}
			}
		}

		private void UpdateActionLabel()
		{
			if (!(label == null))
			{
				if (action != null && action.action != null)
				{
					label.text = action.action.name;
				}
				else
				{
					label.text = string.Empty;
				}
			}
		}
	}
}
