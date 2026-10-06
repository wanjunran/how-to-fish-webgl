using UnityEngine;

public sealed class SettingsInstance : MonoBehaviour
{
	private static SettingsInstance _instance;

	[SerializeField]
	private SettingsPreset _settings;

	public static SettingsPreset Settings => _instance._settings;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		Object.DontDestroyOnLoad(this);
	}

	public void AddClipsFromPathButton()
	{
		if (_settings == null)
		{
			Debug.Log("SettingsPreset not set, cant set clips");
		}
		else
		{
			_settings.AddClipsFromPath();
		}
	}
}
