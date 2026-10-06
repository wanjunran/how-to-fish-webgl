using UnityEngine.EventSystems;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class CanvasGroupModifier : MonoBehaviour
	{
		[Tooltip("The Canvas Group to be modified while this component is active")]
		public CanvasGroup canvasGroup;

		[Tooltip("The interactable setting to use for the Canvas Group while this component is active")]
		public bool interactable;

		private bool m_SavedInteractable;

		private GameObject m_SelectedObject;

		private void OnEnable()
		{
			if (canvasGroup != null)
			{
				m_SelectedObject = EventSystem.current.currentSelectedGameObject;
				m_SavedInteractable = canvasGroup.interactable;
				canvasGroup.interactable = interactable;
			}
		}

		private void OnDisable()
		{
			if (!(canvasGroup != null))
			{
				return;
			}
			canvasGroup.interactable = m_SavedInteractable;
			EventSystem current = EventSystem.current;
			if (current != null)
			{
				if (m_SelectedObject != null)
				{
					current.SetSelectedGameObject(m_SelectedObject);
				}
				else if (EventSystem.current.currentSelectedGameObject == null)
				{
					current.SetSelectedGameObject(current.firstSelectedGameObject);
				}
			}
		}
	}
}
