using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class SpriteManager : MonoBehaviour
{
	private static SpriteManager _instance;

	[SerializeField]
	private InputActionReference _pickUpReference;

	private void Awake()
	{
		_instance = this;
	}

	public static string GetPickUpInput()
	{
		return InputToSpriteName(_instance._pickUpReference);
	}

	public static string InputToSpriteName(InputAction input, InputSchemeSelection inputScheme = InputSchemeSelection.Current, int bindingIndex = -1)
	{
		string text = "";
		switch (inputScheme)
		{
		case InputSchemeSelection.Current:
			text = GameInfo.Input.currentControlScheme;
			break;
		case InputSchemeSelection.ForceController:
			text = "Controller";
			break;
		case InputSchemeSelection.ForceKeyboard:
			text = "Keyboard";
			break;
		}
		if (!(text == "Keyboard"))
		{
			if (text == "Controller")
			{
				ControllerType controller = ControllerType.Controller;
				Gamepad current = Gamepad.current;
				if (!(current is DualShockGamepad))
				{
					if (current is XInputController)
					{
						controller = ControllerType.Xbox;
					}
				}
				else
				{
					controller = ControllerType.Playstation;
				}
				if (bindingIndex >= 0 && bindingIndex < input.bindings.Count)
				{
					if (ControllerInputToSpriteName(input.bindings[bindingIndex], controller, out var result))
					{
						return result;
					}
					return input.GetBindingDisplayString(bindingIndex);
				}
				foreach (InputBinding binding in input.bindings)
				{
					if (binding.groups.Contains(text) && ControllerInputToSpriteName(binding, controller, out var result2))
					{
						return result2;
					}
				}
			}
		}
		else
		{
			string keyboardBindingDisplay = GetKeyboardBindingDisplay(input, text, bindingIndex);
			if (!string.IsNullOrEmpty(keyboardBindingDisplay))
			{
				return keyboardBindingDisplay;
			}
		}
		return input.GetBindingDisplayString((InputBinding.DisplayStringOptions)0, text);
	}

	private static string GetKeyboardBindingDisplay(InputAction input, string scheme, int bindingIndex)
	{
		if (bindingIndex >= 0 && bindingIndex < input.bindings.Count)
		{
			return GetKeyboardMouseBindingDisplay(input, bindingIndex);
		}
		List<string> list = new List<string>();
		InputBinding inputBinding = InputBinding.MaskByGroup(scheme);
		for (int i = 0; i < input.bindings.Count; i++)
		{
			InputBinding binding = input.bindings[i];
			if (binding.isComposite)
			{
				bool flag = false;
				for (int j = i + 1; j < input.bindings.Count && input.bindings[j].isPartOfComposite; j++)
				{
					if (inputBinding.Matches(input.bindings[j]))
					{
						flag = true;
						break;
					}
				}
				if (flag || inputBinding.Matches(binding))
				{
					list.Add(GetKeyboardMouseBindingDisplay(input, i));
				}
			}
			else if (!binding.isPartOfComposite && inputBinding.Matches(binding))
			{
				list.Add(GetKeyboardMouseBindingDisplay(input, i));
			}
		}
		return string.Join(" | ", list);
	}

	private static string GetKeyboardMouseBindingDisplay(InputAction input, int bindingIndex)
	{
		if (KeyboardMouseInputToSpriteName(input.bindings[bindingIndex], out var result))
		{
			return result;
		}
		string bindingDisplayString = input.GetBindingDisplayString(bindingIndex);
		if (!IsKeyboardBinding(input, bindingIndex))
		{
			return bindingDisplayString;
		}
		return "[" + bindingDisplayString + "]";
	}

	private static bool IsKeyboardBinding(InputAction input, int bindingIndex)
	{
		InputBinding inputBinding = input.bindings[bindingIndex];
		if (!inputBinding.isComposite)
		{
			return inputBinding.effectivePath.StartsWith("<Keyboard>");
		}
		for (int i = bindingIndex + 1; i < input.bindings.Count && input.bindings[i].isPartOfComposite; i++)
		{
			if (input.bindings[i].effectivePath.StartsWith("<Keyboard>"))
			{
				return true;
			}
		}
		return false;
	}

	private static bool KeyboardMouseInputToSpriteName(InputBinding binding, out string name)
	{
		if (!_instance)
		{
			name = "";
			return false;
		}
		switch (binding.effectivePath)
		{
		case "<Mouse>/leftButton":
			name = "<sprite name=\"Keyboard & Mouse-Light-Mouse_Left_Key_Light\">";
			return true;
		case "<Mouse>/rightButton":
			name = "<sprite name=\"Keyboard & Mouse-Light-Mouse_Right_Key_Light\">";
			return true;
		case "<Mouse>/delta":
			name = "<sprite name=\"Keyboard & Mouse-Light-Mouse_Simple_Key_Light\">";
			return true;
		case "<Mouse>/scroll/up":
			name = LocalizationManager.ScrollUpLocalized.GetLocalizedString() ?? "";
			return true;
		case "<Mouse>/scroll/down":
			name = LocalizationManager.ScrollDownLocalized.GetLocalizedString() ?? "";
			return true;
		case "<Mouse>/scroll/y":
			name = LocalizationManager.ScrollLocalized.GetLocalizedString() ?? "";
			return true;
		default:
			name = "";
			return false;
		}
	}

	private static bool ControllerInputToSpriteName(InputBinding binding, ControllerType controller, out string name)
	{
		name = "";
		switch (binding.effectivePath)
		{
		case "<Gamepad>/leftStick":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Left_Stick\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Left_Stick\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Left_Stick\">";
				break;
			}
			return true;
		case "<Gamepad>/rightStick":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Right_Stick\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Right_Stick\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Right_Stick\">";
				break;
			}
			return true;
		case "<Gamepad>/buttonWest":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_X\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Square\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_X\">";
				break;
			}
			return true;
		case "<Gamepad>/buttonSouth":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_A\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Cross\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_A\">";
				break;
			}
			return true;
		case "<Gamepad>/buttonEast":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_B\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Circle\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_B\">";
				break;
			}
			return true;
		case "<Gamepad>/buttonNorth":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Y\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Triangle\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Y\">";
				break;
			}
			return true;
		case "<Gamepad>/leftTrigger":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_LT\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_L2\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_LT\">";
				break;
			}
			return true;
		case "<Gamepad>/rightTrigger":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_RT\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_R2\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_RT\">";
				break;
			}
			return true;
		case "<Gamepad>/leftShoulder":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_LB\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_L1\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_LB\">";
				break;
			}
			return true;
		case "<Gamepad>/rightShoulder":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_RB\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_R1\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_RB\">";
				break;
			}
			return true;
		case "<Gamepad>/dpad/down":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Dpad_Down\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Dpad_Down\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Dpad_Down\">";
				break;
			}
			return true;
		case "<Gamepad>/dpad/up":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Dpad_Up\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Dpad_Up\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Dpad_Up\">";
				break;
			}
			return true;
		case "<Gamepad>/dpad/left":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Dpad_Left\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Dpad_Left\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Dpad_Left\">";
				break;
			}
			return true;
		case "<Gamepad>/dpad/right":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Dpad_Right\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Dpad_Right\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Dpad_Right\">";
				break;
			}
			return true;
		case "<Gamepad>/select":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_View\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Share_Alt\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_View\">";
				break;
			}
			return true;
		case "<Gamepad>/leftStickPress":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Left_Stick_Click\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Left_Stick_Click\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Left_Stick_Click\">";
				break;
			}
			return true;
		case "<Gamepad>/rightStickPress":
			switch (controller)
			{
			case ControllerType.Xbox:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Right_Stick_Click\">";
				break;
			case ControllerType.Playstation:
				name = "<sprite name=\"PS5-PS5_Right_Stick_Click\">";
				break;
			default:
				name = "<sprite name=\"Xbox Series-XboxSeriesX_Right_Stick_Click\">";
				break;
			}
			return true;
		default:
			return false;
		}
	}

	public static string GetDirectionName(string direction)
	{
		return direction.ToLower() switch
		{
			"up" => LocalizationManager.UpLocalized.GetLocalizedString(), 
			"left" => LocalizationManager.LeftLocalized.GetLocalizedString(), 
			"right" => LocalizationManager.RightLocalized.GetLocalizedString(), 
			"down" => LocalizationManager.DownLocalized.GetLocalizedString(), 
			"negative" => LocalizationManager.NegativeLocalized.GetLocalizedString(), 
			"positive" => LocalizationManager.PositiveLocalized.GetLocalizedString(), 
			_ => direction, 
		};
	}
}
