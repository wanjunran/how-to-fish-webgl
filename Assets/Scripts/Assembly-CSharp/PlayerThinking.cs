using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThinking : MonoBehaviour
{
	private Player _player;

	private bool _isLocal;

	private bool _isScrolling;

	public static bool IsThinking { get; private set; }

	public static event Action OnThinking;

	public void InitializeLocal(Player player)
	{
		_player = player;
		_isLocal = _player.Owner.IsLocalClient;
		ToggleThinking(to: false);
		BindInputs();
	}

	private void OnDestroy()
	{
		UnbindInputs();
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["PlayerTab"].performed += ThinkingInput;
		input.actions["Pause"].performed += ExitThinkingInput;
		input.actions["PlayerMove"].performed += ScrollJournalInput;
		input.actions["PlayerMove"].canceled += ScrollJournalInputCanceled;
	}

	private void UnbindInputs()
	{
		if ((bool)_player && _isLocal)
		{
			PlayerInput input = GameInfo.Input;
			if ((bool)input)
			{
				input.actions["PlayerTab"].performed -= ThinkingInput;
				input.actions["PlayerMove"].performed -= ScrollJournalInput;
				input.actions["PlayerMove"].canceled -= ScrollJournalInputCanceled;
			}
		}
	}

	private void ScrollJournalInput(InputAction.CallbackContext context)
	{
		if (!IsThinking || PauseManager.IsPaused)
		{
			_isScrolling = false;
			return;
		}
		Vector2 vector = context.ReadValue<Vector2>();
		if (Mathf.Abs(vector.x) < 0.5f)
		{
			_isScrolling = false;
		}
		else if (!_isScrolling)
		{
			_isScrolling = true;
			if (vector.x != 0f)
			{
				PlayerUI.ScrollThinkingPage((vector.x > 0f) ? 1 : (-1));
			}
		}
	}

	private void ScrollJournalInputCanceled(InputAction.CallbackContext context)
	{
		_isScrolling = false;
	}

	private void ThinkingInput(InputAction.CallbackContext context)
	{
		if (!PauseManager.IsPaused)
		{
			ToggleThinking();
		}
	}

	private void ExitThinkingInput(InputAction.CallbackContext context)
	{
		ToggleThinking(to: false);
	}

	private void ToggleThinking()
	{
		ToggleThinking(!IsThinking);
	}

	private void ToggleThinking(bool to)
	{
		IsThinking = to;
		PlayerCamera.ToggleMouse(IsThinking);
		PlayerUI.OnToggleThinking();
		if (to)
		{
			OnThinking?.Invoke();
		}
	}
}
