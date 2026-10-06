using System;
using TMPro;
using UnityEngine.UI;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class GamepadIconsExample : MonoBehaviour
	{
		[Serializable]
		public struct GamepadIcons
		{
			public Sprite buttonSouth;

			public Sprite buttonNorth;

			public Sprite buttonEast;

			public Sprite buttonWest;

			public Sprite startButton;

			public Sprite selectButton;

			public Sprite leftTrigger;

			public Sprite rightTrigger;

			public Sprite leftShoulder;

			public Sprite rightShoulder;

			public Sprite dpad;

			public Sprite dpadUp;

			public Sprite dpadDown;

			public Sprite dpadLeft;

			public Sprite dpadRight;

			public Sprite leftStick;

			public Sprite rightStick;

			public Sprite leftStickPress;

			public Sprite rightStickPress;

			public Sprite GetSprite(string controlPath)
			{
				return controlPath switch
				{
					"buttonSouth" => buttonSouth, 
					"buttonNorth" => buttonNorth, 
					"buttonEast" => buttonEast, 
					"buttonWest" => buttonWest, 
					"start" => startButton, 
					"select" => selectButton, 
					"leftTrigger" => leftTrigger, 
					"leftTriggerButton" => leftTrigger, 
					"rightTrigger" => rightTrigger, 
					"rightTriggerButton" => rightTrigger, 
					"leftShoulder" => leftShoulder, 
					"rightShoulder" => rightShoulder, 
					"dpad" => dpad, 
					"dpad/up" => dpadUp, 
					"dpad/down" => dpadDown, 
					"dpad/left" => dpadLeft, 
					"dpad/right" => dpadRight, 
					"leftStick" => leftStick, 
					"rightStick" => rightStick, 
					"leftStickPress" => leftStickPress, 
					"rightStickPress" => rightStickPress, 
					_ => null, 
				};
			}
		}

		public GamepadIcons xbox;

		public GamepadIcons ps4;

		protected void OnEnable()
		{
			RebindActionUI[] componentsInChildren = base.transform.GetComponentsInChildren<RebindActionUI>();
			foreach (RebindActionUI obj in componentsInChildren)
			{
				obj.updateBindingUIEvent.AddListener(OnUpdateBindingDisplay);
				obj.UpdateBindingDisplay();
			}
		}

		protected void OnUpdateBindingDisplay(RebindActionUI component, string bindingDisplayString, string deviceLayoutName, string controlPath)
		{
			if (!string.IsNullOrEmpty(deviceLayoutName) && !string.IsNullOrEmpty(controlPath))
			{
				Sprite sprite = null;
				if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
				{
					sprite = ps4.GetSprite(controlPath);
				}
				else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
				{
					sprite = xbox.GetSprite(controlPath);
				}
				TextMeshProUGUI bindingText = component.bindingText;
				Image component2 = bindingText.transform.parent.Find("ActionBindingIcon").GetComponent<Image>();
				if (sprite != null)
				{
					bindingText.gameObject.SetActive(value: false);
					component2.sprite = sprite;
					component2.gameObject.SetActive(value: true);
				}
				else
				{
					bindingText.gameObject.SetActive(value: true);
					component2.gameObject.SetActive(value: false);
				}
			}
		}
	}
}
