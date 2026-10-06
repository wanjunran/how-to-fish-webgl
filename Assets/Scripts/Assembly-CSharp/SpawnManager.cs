using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	[SerializeField]
	private string _testPrint;

	[SerializeField]
	private Transform _playerSpawnPoint;

	[Header("Boat")]
	[SerializeField]
	private Boat _boatPrefab;

	[SerializeField]
	private Transform _boatSpawnPoint;

	public static Vector3 PlayerSpawnPos = Vector3.zero;

	public static float PlayerSpawnRot = 0f;

	public static Vector3 BoatSpawnPos = Vector3.zero;

	public static Quaternion BoatSpawnRot = Quaternion.identity;

	private void Awake()
	{
		PlayerSpawnPos = (_playerSpawnPoint ? _playerSpawnPoint.position : Vector3.zero);
		PlayerSpawnRot = (_playerSpawnPoint ? _playerSpawnPoint.eulerAngles.y : 0f);
		BoatSpawnPos = (_boatSpawnPoint ? _boatSpawnPoint.position : Vector3.zero);
		BoatSpawnRot = (_boatSpawnPoint ? _boatSpawnPoint.rotation : Quaternion.identity);
	}

	private void Start()
	{
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			InitializeIsland();
		}
	}

	private void InitializeIsland()
	{
		if ((bool)BoatManager.Instance && (bool)_boatSpawnPoint && (bool)_boatPrefab)
		{
			BoatManager.Instance.TrySpawnBoat(_boatPrefab, _boatSpawnPoint.position, _boatSpawnPoint.rotation);
		}
	}
}
