using System;
using UnityEngine.Events;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class InvokeUnityEvent : MonoBehaviour
	{
		[Tooltip("The input action that triggers the Unity event when performed.")]
		[SerializeField]
		private InputActionReference m_Action;

		[Tooltip("The Unity event to be invoked when action is performed.")]
		[SerializeField]
		private UnityEvent m_OnPerformed = new UnityEvent();

		private Action<InputAction.CallbackContext> m_OnActionPerformed;

		private bool m_HaveRegisteredCallback;

		public InputActionReference action
		{
			get
			{
				return m_Action;
			}
			set
			{
				if (!(m_Action == value))
				{
					Unregister();
					m_Action = value;
					Register();
				}
			}
		}

		public UnityEvent onPerformed
		{
			get
			{
				return m_OnPerformed;
			}
			set
			{
				if (m_OnPerformed == value)
				{
					return;
				}
				int num;
				if (m_OnPerformed != null)
				{
					num = ((value == null) ? 1 : 0);
					if (num == 0)
					{
						goto IL_0022;
					}
				}
				else
				{
					num = 1;
				}
				Unregister();
				goto IL_0022;
				IL_0022:
				m_OnPerformed = value;
				if (num != 0)
				{
					Register();
				}
			}
		}

		private void Awake()
		{
			m_OnActionPerformed = OnActionPerformed;
		}

		private void OnEnable()
		{
			Register();
		}

		private void OnDisable()
		{
			Unregister();
		}

		private void Register()
		{
			if (!m_HaveRegisteredCallback && m_OnPerformed != null && m_Action != null && m_Action.action != null)
			{
				action.action.performed += m_OnActionPerformed;
				m_HaveRegisteredCallback = true;
			}
		}

		private void Unregister()
		{
			if (m_HaveRegisteredCallback && action != null && action.action != null)
			{
				action.action.performed -= m_OnActionPerformed;
				m_HaveRegisteredCallback = false;
			}
		}

		private void OnActionPerformed(InputAction.CallbackContext context)
		{
			onPerformed?.Invoke();
		}
	}
}
