using UnityEngine;

public static class Setter
{
	public static void SetSingleInstance<T>(ref T instance, T setTo) where T : Component
	{
		if (instance != null)
		{
			Debug.LogWarning($"Instance was already set on {instance.name}, destroying {instance.GetType()}, located on {instance.name}");
			Object.Destroy(instance);
		}
		instance = setTo;
	}
}
