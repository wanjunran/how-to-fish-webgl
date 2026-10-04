using UnityEngine;

public class Island : MonoBehaviour
{
	[SerializeField]
	private float _islandSize = 55f;

	public static float IslandSize = 55f;

	public static Vector3 IslandPos;

	public static Island CurIsland;

	private void Awake()
	{
		IslandSize = _islandSize;
		IslandPos = base.transform.position;
	}

	private void Start()
	{
		CurIsland = this;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(base.transform.position, _islandSize);
	}
}
