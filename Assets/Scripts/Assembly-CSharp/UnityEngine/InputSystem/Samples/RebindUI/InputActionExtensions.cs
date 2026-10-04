using System;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public static class InputActionExtensions
	{
		public static int FindBindingById(this InputAction action, string bindingId)
		{
			if (action == null || string.IsNullOrEmpty(bindingId))
			{
				return -1;
			}
			Guid id = new Guid(bindingId);
			return action.bindings.IndexOf((InputBinding x) => x.id == id);
		}
	}
}
