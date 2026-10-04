using UnityEngine;

[CreateAssetMenu(fileName = "New Settings Preset", menuName = "Bord/Settings Preset")]
public class SettingsPreset : ScriptableObject
{
	[Header("Audio")]
	[SerializeField]
	[Tooltip("File to look for audio clips under Assets/Resources")]
	private string _audioClipPath;

	[SerializeField]
	private AudioClip[] _clipsInPath;

	public AudioClip[] ClipsInPath => _clipsInPath;

	public void AddClipsFromPath()
	{
		_clipsInPath = Resources.LoadAll<AudioClip>(_audioClipPath);
		Debug.Log("Added " + _clipsInPath.Length + " clips from Resources/" + _audioClipPath);
	}
}
