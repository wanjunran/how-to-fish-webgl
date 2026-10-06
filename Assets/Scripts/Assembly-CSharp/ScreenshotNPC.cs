using UnityEngine;

[ExecuteInEditMode]
public class ScreenshotNPC : MonoBehaviour
{
	[SerializeField]
	private Vector3 _skinColor;

	[SerializeField]
	private SkinnedMeshRenderer _head;

	[SerializeField]
	private SkinnedMeshRenderer _outfit;

	[SerializeField]
	private SkinnedMeshRenderer _handLeft;

	[SerializeField]
	private SkinnedMeshRenderer _handRight;

	private Vector3 _shaderSkinColor;

	private void Update()
	{
		if (_skinColor != _shaderSkinColor)
		{
			SetSkinColor();
		}
	}

	private void SetSkinColor()
	{
		_shaderSkinColor = _skinColor;
		if ((bool)_head)
		{
			ShaderManager.SetPlayerColorsEditMode(_head, _skinColor);
		}
		if ((bool)_outfit)
		{
			ShaderManager.SetPlayerColorsEditMode(_outfit, _skinColor);
		}
		if ((bool)_handLeft)
		{
			ShaderManager.SetPlayerColorsEditMode(_handLeft, _skinColor);
		}
		if ((bool)_handRight)
		{
			ShaderManager.SetPlayerColorsEditMode(_handRight, _skinColor);
		}
	}
}
