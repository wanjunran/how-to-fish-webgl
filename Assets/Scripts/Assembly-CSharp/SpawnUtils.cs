using UnityEngine;

public class SpawnUtils
{
	public static Vector3 GetRandomSpawnPoint(BoxCollider spawnBox, bool inAir)
	{
		Vector3 center = spawnBox.center;
		Vector3 size = spawnBox.size;
		Vector3 vector = spawnBox.transform.TransformPoint(center);
		Vector3 vector2 = Vector3.Scale(size * 0.5f, spawnBox.transform.lossyScale);
		float x = Random.Range(0f - vector2.x, vector2.x);
		float y = Random.Range(0f - vector2.y, vector2.y);
		float z = Random.Range(0f - vector2.z, vector2.z);
		Vector3 vector3 = new Vector3(x, y, z);
		Vector3 vector4 = vector + spawnBox.transform.rotation * vector3;
		if (inAir)
		{
			return vector4;
		}
		if (Physics.Raycast(vector4, Vector3.down, out var hitInfo, float.PositiveInfinity, GameInfo.LevelLayer))
		{
			return hitInfo.point;
		}
		Debug.Log("spawner raycast missed level, check box col pos");
		return Vector3.zero;
	}
}
