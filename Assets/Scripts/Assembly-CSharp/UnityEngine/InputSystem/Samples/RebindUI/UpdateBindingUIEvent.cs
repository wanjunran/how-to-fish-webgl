using System;
using UnityEngine.Events;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	[Serializable]
	public class UpdateBindingUIEvent : UnityEvent<ActionLabel, string, string, string>
	{
	}
}
