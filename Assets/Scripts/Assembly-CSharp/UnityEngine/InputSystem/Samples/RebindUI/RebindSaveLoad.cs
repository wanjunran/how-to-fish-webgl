namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class RebindSaveLoad : MonoBehaviour
	{
		[Tooltip("The associated input action asset to be serialized to player preferences (Required).")]
		public InputActionAsset actions;

		[Tooltip("The player preference key to be used when serializing binding overrides to player preferences (Required).")]
		public string playerPreferenceKey;

		[Tooltip("Specifies whether to load and apply binding overrides when the component is enabled")]
		public bool loadOnEnable = true;

		[Tooltip("Specifies whether to save binding overrides when the component is disabled")]
		public bool saveOnDisable = true;

		public void Load()
		{
			if (IsValidConfiguration())
			{
				string text = PlayerPrefs.GetString(playerPreferenceKey);
				if (!string.IsNullOrEmpty(text))
				{
					actions.LoadBindingOverridesFromJson(text);
				}
			}
		}

		public void Save()
		{
			if (IsValidConfiguration())
			{
				string value = actions.SaveBindingOverridesAsJson();
				PlayerPrefs.SetString(playerPreferenceKey, value);
			}
		}

		private void OnEnable()
		{
			if (loadOnEnable)
			{
				Load();
			}
		}

		private void OnDisable()
		{
			if (saveOnDisable)
			{
				Save();
			}
		}

		private bool IsValidConfiguration()
		{
			if (actions == null)
			{
				Debug.LogWarning("Unable to apply binding overrides from player preferences without an associated action asset.");
				return false;
			}
			if (string.IsNullOrEmpty(playerPreferenceKey))
			{
				Debug.LogWarning("Unable to load binding overrides from player preferences without a non-empty preference key.");
				return false;
			}
			return true;
		}
	}
}
