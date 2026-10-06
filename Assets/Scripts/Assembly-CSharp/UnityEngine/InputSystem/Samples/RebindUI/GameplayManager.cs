using System;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class GameplayManager : MonoBehaviour
	{
		public enum GameplayState
		{
			None = 0,
			StartLevel = 1,
			Playing = 2,
			CompleteLevel = 3,
			GameOver = 4,
			ResetGame = 5
		}

		[Tooltip("The game camera")]
		public Camera gameCamera;

		[Tooltip("The enemy spawn rate")]
		public float enemySpawnRate = 1f;

		[Tooltip("The enemy spawn distance from center")]
		public float spawnDistance = 10f;

		[Tooltip("The enemy prefab for the mini game")]
		public GameObject enemy;

		[Tooltip("The explosion prefab for the mini game")]
		public GameObject enemyExplosion;

		[Tooltip("The player prefab for the mini game")]
		public GameObject player;

		private double m_TimeToNextSpawn;

		private GameObject m_Player;

		private ObjectPool<Enemy> m_EnemyPool;

		private float m_ShakeForce;

		private float m_ShakeMaxForce;

		private float m_ShakeDuration;

		private double m_ShakeTime;

		private Vector3 m_CameraPosition;

		private int m_RemainingEnemiesOnThisLevel;

		private int m_EnemySpawnCount;

		private FeedbackController m_FeedbackController;

		private GameplayState m_GameplayState;

		private double m_EarliestTimeToChangeState;

		private GameplayState m_NextGameplayState = GameplayState.StartLevel;

		public int level { get; private set; }

		public GameplayState state => m_GameplayState;

		public bool paused
		{
			get
			{
				return Time.timeScale == 0f;
			}
			set
			{
				if ((!value || Time.timeScale != 0f) && (value || Time.timeScale == 0f))
				{
					Time.timeScale = (value ? 0f : 1f);
					UpdateCursor();
					PauseChanged?.Invoke(value);
				}
			}
		}

		public event Action<GameplayState> GameplayStateChanged;

		public event Action<bool> PauseChanged;

		public void KillEnemy()
		{
			m_RemainingEnemiesOnThisLevel--;
		}

		public void GameOver()
		{
			m_Player.SetActive(value: false);
			m_NextGameplayState = GameplayState.GameOver;
		}

		private void Shake(float duration, float amplitude)
		{
			m_ShakeMaxForce = amplitude;
			m_ShakeForce = amplitude;
			m_ShakeDuration = duration;
			m_ShakeTime = Time.timeAsDouble;
		}

		public void Explosion(Transform target, Vector3 position, float amplitude, Color color, Material material = null)
		{
			GameObject gameObject = Object.Instantiate(enemyExplosion);
			gameObject.transform.position = target.position;
			gameObject.transform.rotation = target.rotation;
			if (material != null)
			{
				MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].sharedMaterial = material;
				}
			}
			Explosion component = gameObject.GetComponent<Explosion>();
			component.explosionPosition = position;
			ParticleSystem.MainModule main = component.GetComponent<ParticleSystem>().main;
			main.startColor = color;
			Shake(0.4f, amplitude);
		}

		private static void WrapAround(ref float x, float min, float max)
		{
			if (x <= min)
			{
				x = max;
			}
			else if (x >= max)
			{
				x = min;
			}
		}

		internal bool IsInsideGameplayArea(Vector3 position, float margin = 0.8f)
		{
			if (!gameCamera || !gameCamera.orthographic)
			{
				return true;
			}
			float orthographicSize = gameCamera.orthographicSize;
			float num = orthographicSize * gameCamera.aspect;
			if (position.x >= 0f - num - margin && position.x <= num + margin && position.y >= 0f - orthographicSize - margin)
			{
				return position.y <= orthographicSize + margin;
			}
			return false;
		}

		private static bool TryTeleportOrthographicExtents(Camera camera, Vector3 position, out Vector3 result, float margin = 0.8f)
		{
			if ((bool)camera && camera.orthographic)
			{
				float orthographicSize = camera.orthographicSize;
				float num = orthographicSize * camera.aspect;
				Vector3 vector = position;
				WrapAround(ref vector.x, 0f - num - margin, num + margin);
				WrapAround(ref vector.y, 0f - orthographicSize - margin, orthographicSize + margin);
				if (vector != position)
				{
					result = vector;
					return true;
				}
			}
			result = position;
			return false;
		}

		internal bool TryTeleportOrthographicExtents(Vector3 position, out Vector3 result, float margin = 0.8f)
		{
			return TryTeleportOrthographicExtents(gameCamera, position, out result, margin);
		}

		private void Awake()
		{
			Screen.orientation = ScreenOrientation.LandscapeLeft;
			m_FeedbackController = GetComponent<FeedbackController>();
			m_EnemyPool = new ObjectPool<Enemy>(() =>
			{
				Enemy component = Object.Instantiate(enemy).GetComponent<Enemy>();
				component.pool = m_EnemyPool;
				component.target = m_Player.transform;
				component.manager = this;
				return component;
			}, (Enemy obj) =>
			{
				obj.gameObject.SetActive(value: true);
			}, (Enemy obj) =>
			{
				obj.gameObject.SetActive(value: false);
			}, (Enemy obj) =>
			{
				Object.Destroy(obj.gameObject);
			});
			m_CameraPosition = gameCamera.transform.position;
			m_EarliestTimeToChangeState = Time.timeAsDouble;
		}

		private void Start()
		{
			m_Player = Object.Instantiate(player, base.transform, worldPositionStays: true);
			m_Player.GetComponent<Player>().manager = this;
			m_Player.GetComponent<PlayerController>().feedbackController = m_FeedbackController;
			m_TimeToNextSpawn = 3.0;
		}

		private void OnEnable()
		{
			Application.focusChanged += OnApplicationFocusChanged;
			paused = !Application.isFocused;
		}

		private void OnDisable()
		{
			Application.focusChanged -= OnApplicationFocusChanged;
			paused = true;
		}

		private void OnApplicationFocusChanged(bool focus)
		{
			paused = !focus;
		}

		private void SpawnEnemy()
		{
			if (m_EnemySpawnCount == 0)
			{
				return;
			}
			m_TimeToNextSpawn -= Time.deltaTime;
			if (!(m_TimeToNextSpawn > 0.0))
			{
				m_TimeToNextSpawn += enemySpawnRate;
				m_EnemySpawnCount--;
				Enemy enemy = m_EnemyPool.Get();
				float orthographicSize = gameCamera.orthographicSize;
				float num = orthographicSize * gameCamera.aspect;
				float num2 = Random.Range(-1f, 1f);
				float num3 = 0.5f;
				switch (Random.Range(0, 4))
				{
				case 0:
					enemy.transform.position = new Vector3(num2 * num, orthographicSize + num3, 0f);
					break;
				case 1:
					enemy.transform.position = new Vector3(num2 * num, 0f - orthographicSize - num3, 0f);
					break;
				case 2:
					enemy.transform.position = new Vector3(0f - num - num3, num2 * orthographicSize, 0f);
					break;
				case 3:
					enemy.transform.position = new Vector3(num + num3, num2 * orthographicSize, 0f);
					break;
				}
			}
		}

		private void AnimateCameraShake()
		{
			double timeAsDouble = Time.timeAsDouble;
			double num = timeAsDouble - m_ShakeTime;
			double num2 = ((m_ShakeDuration <= 0f) ? 1.0 : (num / (double)m_ShakeDuration));
			m_ShakeForce = Mathf.Lerp(m_ShakeMaxForce, 0f, (float)num2);
			Vector3 vector = new Vector3(m_ShakeForce * Mathf.Sin((float)timeAsDouble * 71f), m_ShakeForce * Mathf.Sin((float)timeAsDouble * 53f + MathF.PI / 3f), 0f);
			gameCamera.transform.position = m_CameraPosition + vector;
			if (m_FeedbackController != null)
			{
				m_FeedbackController.rumble = m_ShakeForce;
			}
		}

		private void Update()
		{
			float time = Time.time;
			while ((double)time >= m_EarliestTimeToChangeState && m_NextGameplayState != m_GameplayState)
			{
				m_EarliestTimeToChangeState = time;
				switch (m_GameplayState)
				{
				}
				m_GameplayState = m_NextGameplayState;
				switch (m_NextGameplayState)
				{
				case GameplayState.None:
					m_NextGameplayState = GameplayState.StartLevel;
					break;
				case GameplayState.StartLevel:
					m_EnemySpawnCount = 5 + ++level * 2;
					m_RemainingEnemiesOnThisLevel = m_EnemySpawnCount;
					enemySpawnRate *= 0.9f;
					m_EarliestTimeToChangeState += 2.0;
					m_NextGameplayState = GameplayState.Playing;
					break;
				case GameplayState.CompleteLevel:
					m_NextGameplayState = GameplayState.StartLevel;
					break;
				case GameplayState.GameOver:
					m_EarliestTimeToChangeState += 3.0;
					m_NextGameplayState = GameplayState.ResetGame;
					break;
				case GameplayState.ResetGame:
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
					break;
				}
				GameplayStateChanged?.Invoke(m_GameplayState);
			}
			if (state == GameplayState.Playing)
			{
				if (m_RemainingEnemiesOnThisLevel == 0)
				{
					m_NextGameplayState = GameplayState.CompleteLevel;
				}
				else
				{
					SpawnEnemy();
				}
			}
			AnimateCameraShake();
		}

		private void UpdateCursor()
		{
			if (paused)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}
}
