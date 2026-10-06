using UnityEngine.Pool;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class Bullet : MonoBehaviour
	{
		[Tooltip("The bullet velocity")]
		public float speed = 1f;

		[Tooltip("The bullet movement direction vector")]
		public Vector3 direction = Vector3.forward;

		private IObjectPool<Bullet> m_Pool;

		private GameplayManager m_Manager;

		private bool m_Destroyed;

		public void Initialize(GameplayManager manager, IObjectPool<Bullet> pool)
		{
			m_Manager = manager;
			m_Pool = pool;
		}

		private void Update()
		{
			base.transform.position += direction * (speed * Time.deltaTime);
			if (!m_Manager.IsInsideGameplayArea(base.transform.position))
			{
				DestroyBullet();
			}
		}

		private void OnEnable()
		{
			m_Destroyed = false;
		}

		private void OnCollisionEnter(Collision other)
		{
			DestroyBullet();
		}

		private void DestroyBullet()
		{
			if (!m_Destroyed)
			{
				m_Pool.Release(this);
				m_Destroyed = true;
			}
		}
	}
}
