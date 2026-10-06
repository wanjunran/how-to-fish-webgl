using UnityEngine.EventSystems;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class RebindUIGameManager : MonoBehaviour
	{
		private enum GameState
		{
			Initializing = 0,
			Playing = 1,
			RebindingMenu = 2
		}

		[Tooltip("The in-game menu object to be activated and deactivated when menu is toggled (Required).")]
		public GameObject menu;

		[Tooltip("The actions asset that holds Gameplay, Common and UI action maps to be used. (Required).")]
		public InputActionAsset actions;

		[Tooltip("Whether UI actions should be disabled during gameplay.")]
		public bool enableUIActionsDuringGameplay = true;

		[Tooltip("The gameplay manager responsible for managing gameplay.")]
		public GameplayManager gameplayManager;

		[Tooltip("The gameplay UI")]
		public GameObject gameUI;

		private GameState m_CurrentState;

		private GameState m_NextState = GameState.Playing;

		private InputActionMap gameplayActions;

		private InputActionMap uiActions;

		private InputAction toggleMenuAction;

		private void Awake()
		{
			gameplayActions = actions.FindActionMap("Gameplay");
			uiActions = actions.FindActionMap("UI");
			toggleMenuAction = actions.FindAction("Common/Menu");
		}

		public void ToggleMenu()
		{
			switch (m_CurrentState)
			{
			case GameState.Playing:
				m_NextState = GameState.RebindingMenu;
				break;
			case GameState.RebindingMenu:
				if (menu.GetComponent<CanvasGroup>().interactable)
				{
					m_NextState = GameState.Playing;
				}
				break;
			}
		}

		private void OnToggleMenu(InputAction.CallbackContext obj)
		{
			ToggleMenu();
		}

		private void OnEnable()
		{
			toggleMenuAction.performed += OnToggleMenu;
			toggleMenuAction.Enable();
		}

		private void OnDisable()
		{
			toggleMenuAction.performed -= OnToggleMenu;
			toggleMenuAction.Disable();
		}

		private void Update()
		{
			if (m_CurrentState == m_NextState)
			{
				return;
			}
			m_CurrentState = m_NextState;
			switch (m_NextState)
			{
			case GameState.Playing:
				gameplayActions.Enable();
				gameplayManager.enabled = true;
				if (enableUIActionsDuringGameplay)
				{
					uiActions.Enable();
				}
				else
				{
					uiActions.Disable();
				}
				gameUI.SetActive(value: true);
				menu.SetActive(value: false);
				break;
			case GameState.RebindingMenu:
			{
				gameplayActions.Disable();
				gameplayManager.enabled = false;
				if (!enableUIActionsDuringGameplay)
				{
					uiActions.Enable();
				}
				gameUI.SetActive(value: false);
				menu.SetActive(value: true);
				EventSystem current = EventSystem.current;
				if (current.currentSelectedGameObject == null)
				{
					current.SetSelectedGameObject(current.firstSelectedGameObject);
				}
				break;
			}
			}
		}
	}
}
