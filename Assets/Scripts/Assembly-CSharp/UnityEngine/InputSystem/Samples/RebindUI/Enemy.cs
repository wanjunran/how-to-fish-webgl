using UnityEngine.Pool;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class Enemy : MonoBehaviour
	{
		[Tooltip("The rotation animation target")]
		public GameObject animationTarget;

		[Tooltip("The target that the enemy will seek, e.g. player transform")]
		public Transform target;

		[Tooltip("The rotation speed around the X-axis")]
		public float rotationSpeedX = 130f;

		[Tooltip("The rotation speed around the Y-axis")]
		public float rotationSpeedY = 100f;

		[Tooltip("The movement speed")]
		public float speed = 1f;

		[Tooltip("The explosion color")]
		public Color explosionColor = new Color(0.8711135f, 0.5424528f, 1f);

		public IObjectPool<Enemy> pool;

		[HideInInspector]
		public GameplayManager manager;

		private void OnCollisionEnter(Collision other)
		{
			if ((bool)other.gameObject.GetComponent<Bullet>())
			{
				manager.KillEnemy();
				manager.Explosion(animationTarget.transform, other.GetContact(0).point, 0.1f, explosionColor);
				pool.Release(this);
			}
		}

		private void Update()
		{
			if ((bool)animationTarget)
			{
				animationTarget.transform.Rotate(Vector3.up, rotationSpeedX * Time.deltaTime, Space.World);
				animationTarget.transform.Rotate(Vector3.right, rotationSpeedY * Time.deltaTime, Space.World);
			}
			if ((bool)target)
			{
				base.transform.position += (target.position - base.transform.position).normalized * (Time.deltaTime * speed);
			}
			if (manager.TryTeleportOrthographicExtents(base.transform.position, out var result))
			{
				base.transform.position = result;
			}
		}
	}
}
