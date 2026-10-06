using UnityEngine;

public class OutfitMeshInfo
{
	public Mesh Mesh { get; private set; }

	public bool Unlocked { get; private set; }

	public OutfitMeshInfo(Mesh mesh, bool unlocked)
	{
		Mesh = mesh;
		Unlocked = unlocked;
	}

	public void Unlock()
	{
		Unlocked = true;
	}
}
