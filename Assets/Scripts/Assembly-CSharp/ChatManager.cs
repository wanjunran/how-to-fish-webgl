using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
	private static ChatManager _instance;

	[SerializeField]
	private CanvasGroup _chatGroup;

	[SerializeField]
	private TextMeshProUGUI _messageText;

	[SerializeField]
	private TMP_InputField _chatInputField;

	[SerializeField]
	private RectTransform _chatMask;

	[SerializeField]
	private int _messagesWhenOpen = 5;

	private static bool _isDisabled;

	private static bool _showingFullChat;

	private static float _curChatHeight;

	private const float _messageHeight = 25f;

	private static GameObject _prevSelectedObject;

	private bool _canceledTyping;

	private static Queue<float> _heightsToRemove = new Queue<float>();

	public static bool IsTyping { get; private set; }

	public static bool IsTypingDelayed { get; private set; }

	private void Awake()
	{
		_instance = this;
		_messageText.text = " ";
		_instance._chatMask.sizeDelta = new Vector2(_instance._chatMask.sizeDelta.x, 0f);
		ToggleChat(to: false, openedManually: false);
	}

	public static void ToggleChatCanvas(bool to)
	{
		if ((bool)_instance)
		{
			_isDisabled = to;
			_instance._chatGroup.alpha = (to ? 1 : 0);
		}
	}

	private void Update()
	{
		IsTypingDelayed = IsTyping;
		if (Input.GetKeyDown(KeyCode.Return) && !PauseManager.IsPaused && !PlayerThinking.IsThinking && (bool)Server.Instance)
		{
			ToggleChat(!IsTyping);
		}
	}

	public static void OnPause()
	{
		if ((bool)_instance)
		{
			_instance._chatInputField.text = "";
		}
		if (IsTyping)
		{
			ToggleChat(to: false);
		}
	}

	public void SendTypedMessage()
	{
		if (_chatInputField.text.Replace(" ", "") != "" && (bool)Server.Instance)
		{
			string text = _chatInputField.text;
			if (!DazedCommands.IsServerCommand(text))
			{
				Server.Instance.SendChatMessage(SteamUser.GetSteamID().m_SteamID, text);
			}
		}
		_chatInputField.text = "";
	}

	public static void ToggleChat(bool to, bool openedManually = true)
	{
		if (IsTyping == to)
		{
			return;
		}
		IsTyping = to;
		_instance._chatInputField.gameObject.SetActive(IsTyping);
		if (IsTyping)
		{
			_prevSelectedObject = EventSystem.current.currentSelectedGameObject;
			LeanTween.cancel(_instance._chatInputField.gameObject);
			EventSystem.current.SetSelectedGameObject(_instance._chatInputField.gameObject);
		}
		else if (EventSystem.current.currentSelectedGameObject == _instance._chatInputField.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(_prevSelectedObject);
		}
		LeanTween.cancel(_instance._chatMask);
		LeanTween.value(_instance._chatMask.gameObject, SetChatHeight, _instance._chatMask.sizeDelta.y, (float)_instance._messagesWhenOpen * 25f, 0.1f).setEase(LeanTweenType.easeOutQuad);
		if (!IsTyping)
		{
			if (!openedManually)
			{
				OnlyShowRecentMessages();
				return;
			}
			_showingFullChat = true;
			LeanTween.cancel(_instance._chatInputField.gameObject);
			LeanTween.value(_instance._chatInputField.gameObject, 0f, 1f, 5f).setOnComplete(OnlyShowRecentMessages);
		}
	}

	private static void OnlyShowRecentMessages()
	{
		_showingFullChat = false;
		LeanTween.cancel(_instance._chatMask);
		LeanTween.value(_instance._chatMask.gameObject, SetChatHeight, _instance._chatMask.sizeDelta.y, _curChatHeight, 0.1f).setEase(LeanTweenType.easeOutQuad);
	}

	public static void ChatMessage(CSteamID from, string message)
	{
		if (from == SteamManager.CurrentLobbyID)
		{
			ChatMessage("[<color=#" + ColorUtility.ToHtmlStringRGB(GameInfo.MiniBossColor) + ">Server</color>] " + message);
			return;
		}
		ChatMessage("[<color=#" + ColorUtility.ToHtmlStringRGB(GameInfo.RedColor) + ">" + SteamFriends.GetFriendPersonaName(from) + "</color>] " + message);
	}

	public static void ChatMessage(string message)
	{
		if ((bool)_instance)
		{
			float y = _instance._messageText.rectTransform.sizeDelta.y;
			TextMeshProUGUI messageText = _instance._messageText;
			messageText.text = messageText.text + "\n</color></b></i></u></size></material>" + message;
			LayoutRebuilder.ForceRebuildLayoutImmediate(_instance._messageText.rectTransform);
			float num = _instance._messageText.rectTransform.sizeDelta.y - y;
			_curChatHeight += num;
			_heightsToRemove.Enqueue(num);
			_instance.StartCoroutine(LowerChatHeight());
			if (!IsTyping && !_showingFullChat)
			{
				LeanTween.cancel(_instance._chatMask);
				LeanTween.value(_instance._chatMask.gameObject, SetChatHeight, _instance._chatMask.sizeDelta.y, _curChatHeight, 0.1f).setEase(LeanTweenType.easeOutQuad);
			}
		}
	}

	private static IEnumerator LowerChatHeight()
	{
		yield return new WaitForSeconds(5f);
		float num = _heightsToRemove.Dequeue();
		_curChatHeight -= num;
		if (!IsTyping && !_showingFullChat)
		{
			LeanTween.cancel(_instance._chatMask);
			LeanTween.value(_instance._chatMask.gameObject, SetChatHeight, _instance._chatMask.sizeDelta.y, _curChatHeight, 0.1f).setEase(LeanTweenType.easeOutQuad);
		}
	}

	private static void SetChatHeight(float to)
	{
		_instance._chatMask.sizeDelta = new Vector2(_instance._chatMask.sizeDelta.x, to);
	}

	private void OnLog(string condition, string stacktrace, LogType type)
	{
		string text = "";
		switch (type)
		{
		case LogType.Assert:
			text = $"[<color=green>{type}</color>]";
			break;
		case LogType.Error:
			text = $"[<color=red>{type}</color>]";
			break;
		case LogType.Exception:
			text = $"[<color=orange>{type}</color>]";
			break;
		case LogType.Log:
			text = $"[<color=grey>{type}</color>]";
			break;
		case LogType.Warning:
			text = $"[<color=yellow>{type}</color>]";
			break;
		}
		ChatMessage(text + ": " + condition + ", " + stacktrace);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void HandleLog(string condition, string stacktrace, LogType type)
	{
		if (type == LogType.Error && !condition.Contains("Failed to load the Steam App List"))
		{
			ChatMessage($"<color=red>{type}:</color> {condition}, {stacktrace}");
		}
	}
}
