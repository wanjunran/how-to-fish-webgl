using TMPro;
using UnityEngine;

public class DripText : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _dripText;

	[SerializeField]
	private float _hoverSpeed = 3f;

	[SerializeField]
	private float _hoverOffset = 0.005f;

	[SerializeField]
	private float _hoverScale = 1.5f;

	[Space]
	[SerializeField]
	private float _dripSpeed = 1f;

	[SerializeField]
	private float _dripOffset = 0.0025f;

	[SerializeField]
	private bool _isDrip;

	private void Update()
	{
		_dripText.ForceMeshUpdate();
		TMP_TextInfo textInfo = _dripText.textInfo;
		for (int i = 0; i < textInfo.characterCount; i++)
		{
			TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[i];
			if (!tMP_CharacterInfo.isVisible)
			{
				continue;
			}
			Vector3[] vertices = textInfo.meshInfo[tMP_CharacterInfo.materialReferenceIndex].vertices;
			for (int j = 0; j < 4; j++)
			{
				Vector3 vector = vertices[tMP_CharacterInfo.vertexIndex + j];
				vertices[tMP_CharacterInfo.vertexIndex + j] = vector + new Vector3(0f, Mathf.Sin(Time.time * _hoverSpeed + vector.x * _hoverOffset) * _hoverScale, 0f);
			}
			if (_isDrip)
			{
				Color32[] colors = textInfo.meshInfo[tMP_CharacterInfo.materialReferenceIndex].colors32;
				for (int k = 0; k < 4; k++)
				{
					float num = vertices[tMP_CharacterInfo.vertexIndex + k].x + vertices[tMP_CharacterInfo.vertexIndex + k].y;
					float time = Mathf.Repeat(Time.time * _dripSpeed + num * _dripOffset, 1f);
					colors[tMP_CharacterInfo.vertexIndex + k] = GameInfo.DripGradient.Evaluate(time);
				}
			}
		}
		for (int l = 0; l < textInfo.meshInfo.Length; l++)
		{
			TMP_MeshInfo tMP_MeshInfo = textInfo.meshInfo[l];
			if (tMP_MeshInfo.vertexCount != 0)
			{
				tMP_MeshInfo.mesh.vertices = tMP_MeshInfo.vertices;
				tMP_MeshInfo.mesh.colors32 = tMP_MeshInfo.colors32;
				_dripText.UpdateGeometry(tMP_MeshInfo.mesh, l);
			}
		}
	}

	public void SetDrip(bool to)
	{
		_isDrip = to;
	}
}
