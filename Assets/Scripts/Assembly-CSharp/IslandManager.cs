using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IslandManager : MonoBehaviour
{
	private static IslandManager _instance;

	[SerializeField]
	private float _nearPlayerRange = 15f;

	[SerializeField]
	private IslandInfo[] _islandInfos;

	private List<string> _scenes = new List<string>();

	public static int TotalIslands;

	private byte _curIsland;

	private bool _hasQueuedIsland;

	private byte _queuedIsland = byte.MaxValue;

	private const int IslandBuildOffset = 1;

	public static float NearPlayerRange => _instance._nearPlayerRange;

	public static bool IsLoading { get; private set; }

	private void Awake()
	{
		_instance = this;
		TotalIslands = SceneManager.sceneCountInBuildSettings - 1;
	}

	private void Start()
	{
		Init();
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	private void Init()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChange;
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
			_scenes.Add(fileNameWithoutExtension);
		}
		UnloadIslands();
	}

	private void OnActiveSceneChange(Scene arg0, Scene arg1)
	{
	}

	public static void LoadIsland(byte islandId)
	{
		if ((bool)_instance)
		{
			_instance.QueueRequest(islandId);
		}
	}

	public static void UnloadIslands()
	{
		if ((bool)_instance)
		{
			_instance.QueueRequest(byte.MaxValue);
		}
	}

	private void QueueRequest(byte islandId)
	{
		if (IsLoading)
		{
			_hasQueuedIsland = true;
			_queuedIsland = islandId;
		}
		else
		{
			StartCoroutine(ProcessRequest(islandId));
		}
	}

	private IEnumerator ProcessRequest(byte islandId)
	{
		IsLoading = true;
		yield return UnloadAllIslandsRoutine();
		DecalManager.ClearDecals();
		if (islandId != byte.MaxValue)
		{
			yield return SceneManager.LoadSceneAsync(islandId + 1, LoadSceneMode.Additive);
		}
		IsLoading = false;
		if (!Application.isPlaying)
		{
			yield break;
		}
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized && OnlineIslandManager.TeleportPlayers)
		{
			foreach (Player alivePlayer in PlayerManager.AlivePlayers)
			{
				Server.Instance.TeleportPlayer(alivePlayer, SpawnManager.PlayerSpawnPos, SpawnManager.PlayerSpawnRot);
			}
		}
		if (OnlineIslandManager.TeleportPlayers)
		{
			BoatManager.Instance.TryMoveBoat(SpawnManager.BoatSpawnPos, SpawnManager.BoatSpawnRot);
		}
		OnlineIslandManager.ToggleTeleportPlayers(to: false);
		if (_hasQueuedIsland)
		{
			_hasQueuedIsland = false;
			StartCoroutine(ProcessRequest(_queuedIsland));
		}
	}

	private IEnumerator UnloadAllIslandsRoutine()
	{
		Scene mainScene = base.gameObject.scene;
		for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (!(sceneAt == mainScene))
			{
				AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneAt);
				if (asyncOperation != null)
				{
					yield return asyncOperation;
				}
			}
		}
		if ((bool)Server.Instance && Server.Instance.IsServerInitialized)
		{
			ItemManager.OnIslandUnloaded();
		}
	}

	public static void OnIslandChange(byte index)
	{
		for (int i = 0; i < _instance._islandInfos.Length; i++)
		{
			_instance._islandInfos[i].Toggle(i != index);
		}
	}

	public static IslandInfo GetIslandInfo(int index)
	{
		return _instance._islandInfos[index];
	}
}
