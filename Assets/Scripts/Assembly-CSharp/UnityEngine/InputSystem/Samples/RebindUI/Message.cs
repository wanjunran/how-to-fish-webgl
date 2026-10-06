using System;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class Message : MonoBehaviour
	{
		[Tooltip("The associated gameplay manager.")]
		public GameplayManager gameplayManager;

		[Tooltip("The associated UI root to hide/show.")]
		public GameObject root;

		[Tooltip("The associated UI text to be altered to show messages.")]
		public Text text;

		private Action m_TimeoutCallback;

		private void OnEnable()
		{
			gameplayManager.GameplayStateChanged += OnGameplayStateChanged;
			gameplayManager.PauseChanged += OnPauseChanged;
			OnGameplayStateChanged(gameplayManager.state);
		}

		private void OnDisable()
		{
			gameplayManager.GameplayStateChanged += OnGameplayStateChanged;
			gameplayManager.PauseChanged -= OnPauseChanged;
		}

		private void OnPauseChanged(bool paused)
		{
			OnGameplayStateChanged(gameplayManager.state);
		}

		private void Hide()
		{
			root.SetActive(value: false);
		}

		private void Show(string message)
		{
			text.text = message;
			root.SetActive(value: true);
		}

		private void Show(string message, float duration)
		{
			Show(message);
		}

		private void OnGameplayStateChanged(GameplayManager.GameplayState state)
		{
			if (gameplayManager.paused)
			{
				Show("PAUSED");
				return;
			}
			switch (state)
			{
			case GameplayManager.GameplayState.StartLevel:
				Show($"ROUND {gameplayManager.level}");
				break;
			case GameplayManager.GameplayState.Playing:
				Hide();
				break;
			case GameplayManager.GameplayState.GameOver:
				Show("GAME OVER");
				break;
			case GameplayManager.GameplayState.None:
			case GameplayManager.GameplayState.CompleteLevel:
				break;
			}
		}
	}
}
