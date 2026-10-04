using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

[Serializable]
public class HeldInfo
{
	[SerializeField]
	private LocalizedString _infoLocalized;

	[SerializeField]
	private InputActionReference[] _inputActions;

	public LocalizedString InfoLocalized => _infoLocalized;

	public InputActionReference[] InputActions => _inputActions;

	private string GetInfo()
	{
		string text = "";
		if (_inputActions != null)
		{
			for (int i = 0; i < _inputActions.Length; i++)
			{
				string text2 = SpriteManager.InputToSpriteName(_inputActions[i].action) ?? "";
				text += text2;
			}
		}
		return text;
	}
}
