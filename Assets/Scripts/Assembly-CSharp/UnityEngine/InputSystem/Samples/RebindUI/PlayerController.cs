using System;
using UnityEngine.InputSystem.Controls;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	[RequireComponent(typeof(Player))]
	[DefaultExecutionOrder(-1)]
	public class PlayerController : MonoBehaviour
	{
		[Tooltip("The move action, must generate Vector2")]
		public InputActionReference move;

		[Tooltip("The move action, must generate Vector2")]
		public InputActionReference look;

		[Tooltip("The move action, must generate Button value")]
		public InputActionReference fire;

		[Tooltip("The move action, must generate Button value")]
		public InputActionReference change;

		[Tooltip("Feedback controller handling device feedback")]
		public FeedbackController feedbackController;

		private Action<InputAction.CallbackContext> m_OnFire;

		private Action<InputAction.CallbackContext> m_OnChange;

		private Player m_Player;

		private const float kMouseSensitivity = 0.4f;

		private const float kGamepadSensitivity = 1f;

		private void Awake()
		{
			m_Player = GetComponent<Player>();
			m_OnFire = OnFire;
			m_OnChange = OnChange;
		}

		private void OnEnable()
		{
			fire.action.performed += m_OnFire;
			change.action.performed += m_OnChange;
		}

		private void OnDisable()
		{
			fire.action.performed -= m_OnFire;
			change.action.performed -= m_OnChange;
			feedbackController.color = Color.black;
		}

		private void OnFire(InputAction.CallbackContext context)
		{
			bool flag = context.action.IsPressed();
			if (flag)
			{
				feedbackController?.RecordRecentDeviceFromAction(context.action);
			}
			m_Player.firing = flag;
		}

		private void OnChange(InputAction.CallbackContext context)
		{
			feedbackController?.RecordRecentDeviceFromAction(context.action);
			m_Player.Change();
		}

		private void Update()
		{
			Vector2 vector = move.action.ReadValue<Vector2>();
			m_Player.move = vector;
			if (vector.sqrMagnitude > 0.05f)
			{
				feedbackController?.RecordRecentDeviceFromAction(move);
			}
			if (look != null && look.action != null)
			{
				Vector2 vector2 = look.action.ReadValue<Vector2>();
				if (vector2.sqrMagnitude > 0.05f)
				{
					feedbackController?.RecordRecentDeviceFromAction(look);
				}
				float num = ((look.action.activeControl is DeltaControl) ? 0.4f : (Time.deltaTime * 300f * 1f));
				float angle = vector2.x * -1f * num;
				m_Player.Rotate(angle);
			}
			if (feedbackController != null)
			{
				feedbackController.color = m_Player.GetColor();
			}
		}
	}
}
