using UnityEngine.Pool;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class Player : MonoBehaviour
	{
		[HideInInspector]
		[Tooltip("The gameplay manager")]
		public GameplayManager manager;

		[Tooltip("The fire object")]
		public GameObject fireObject;

		[Tooltip("The omni-fire object")]
		public GameObject omniFireObject;

		[Tooltip("The bullet/particle object")]
		public GameObject particle;

		[Tooltip("The cannon belt")]
		public GameObject belt;

		[Tooltip("The cannon barrel")]
		public GameObject barrel;

		[Tooltip("The regular fire rate")]
		public float fireRate = 0.25f;

		[Tooltip("The omni-fire rate")]
		public float omniFireRate = 1f;

		[Tooltip("The change rate")]
		public float changeRate = 1f;

		[Tooltip("List of color animation targets")]
		public Renderer[] animatedRenderers;

		private static readonly int Color1 = Shader.PropertyToID("_Color");

		private Material m_Material;

		private Vector3 m_TargetEulerAngles;

		private Color m_TargetColor;

		private Color m_Color;

		private float m_TargetScale;

		private int m_ColorIndex;

		private float m_TimeUntilNextFire;

		private float m_TimeUntilNextChange;

		private bool m_OmniFire;

		private bool m_ChangeRequested;

		private float m_TargetBeltAngle;

		private float m_BeltAngle;

		private float m_BarrelPosition;

		private float m_RotationAngle;

		private ObjectPool<Bullet> m_ObjectPool;

		private Rigidbody m_Rigidbody;

		public bool firing { get; set; }

		public Vector2 move { get; set; }

		public Color GetColor()
		{
			return GetColor(m_OmniFire);
		}

		public void Change()
		{
			m_ChangeRequested = true;
		}

		public void Rotate(float angle)
		{
			m_RotationAngle += angle;
		}

		private void Awake()
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			m_BarrelPosition = barrel.transform.localPosition.y;
			fireObject.transform.localScale = (m_OmniFire ? Vector3.zero : Vector3.one);
			omniFireObject.transform.localScale = (m_OmniFire ? Vector3.one : Vector3.zero);
			m_TargetColor = GetColor(m_OmniFire);
		}

		private void Start()
		{
			m_Material = animatedRenderers[0].sharedMaterial;
			m_ObjectPool = new ObjectPool<Bullet>(() =>
			{
				Bullet component = Object.Instantiate(particle).GetComponent<Bullet>();
				component.Initialize(manager, m_ObjectPool);
				return component;
			}, (Bullet bullet) =>
			{
				bullet.gameObject.SetActive(value: true);
			}, (Bullet bullet) =>
			{
				bullet.gameObject.SetActive(value: false);
			}, (Bullet bullet) =>
			{
				Object.Destroy(bullet.gameObject);
			});
		}

		private void OnEnable()
		{
			m_TimeUntilNextFire = 0f;
			m_TimeUntilNextChange = 0f;
		}

		private void UpdateFire(float deltaTime)
		{
			if (Throttle(ref m_TimeUntilNextFire, firing, deltaTime, m_OmniFire ? omniFireRate : fireRate))
			{
				return;
			}
			if (m_OmniFire)
			{
				for (int i = 0; i < 8; i++)
				{
					FireBullet(Quaternion.AngleAxis((float)i * 45f, Vector3.forward) * base.transform.up);
				}
			}
			else
			{
				FireBullet(base.transform.up);
			}
		}

		private static bool Throttle(ref float remainingTime, bool condition, float deltaTime, float timeUntilNextEvent)
		{
			remainingTime -= deltaTime;
			if (remainingTime > 0f)
			{
				return true;
			}
			if (condition)
			{
				remainingTime += timeUntilNextEvent;
			}
			if (remainingTime < 0f)
			{
				remainingTime = 0f;
			}
			return !condition;
		}

		private void FireBullet(Vector3 direction)
		{
			Bullet bullet = m_ObjectPool.Get();
			bullet.direction = direction;
			bullet.transform.position = base.transform.position + direction.normalized * (1.6f * base.transform.lossyScale.y);
			Vector3 localPosition = barrel.transform.localPosition;
			barrel.transform.localPosition = new Vector3(localPosition.x, m_BarrelPosition - 0.2f, localPosition.z);
			m_BeltAngle += 45f;
		}

		private void UpdateChangeWeapon(float deltaTime)
		{
			if (!Throttle(ref m_TimeUntilNextChange, m_ChangeRequested, deltaTime, changeRate))
			{
				m_ChangeRequested = false;
				m_OmniFire = !m_OmniFire;
				m_TargetScale = (m_OmniFire ? 1f : 0f);
				m_BeltAngle += 360f;
				m_TargetColor = GetColor(m_OmniFire);
			}
		}

		private void UpdateRotate()
		{
			base.transform.Rotate(Vector3.forward, m_RotationAngle, Space.World);
			m_RotationAngle = 0f;
		}

		private void OnCollisionEnter(Collision other)
		{
			if ((bool)other.gameObject.GetComponent<Enemy>())
			{
				Color.RGBToHSV(GetColor(), out var H, out var S, out var V);
				Color color = Color.HSVToRGB(H, S * 0.5f, V);
				manager.Explosion(base.transform, other.GetContact(0).point, 0.5f, color, m_Material);
				manager.GameOver();
			}
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			UpdateFire(deltaTime);
			UpdateChangeWeapon(deltaTime);
			UpdateRotate();
			if (manager.TryTeleportOrthographicExtents(base.transform.position, out var result))
			{
				base.transform.position = result;
			}
			AnimateChangeWeapon(deltaTime);
			AnimateFireWeapon(deltaTime);
			AnimateColors(deltaTime);
		}

		private void FixedUpdate()
		{
			float num = move.y;
			if (num < 0f)
			{
				num *= 0.33f;
			}
			if (m_Rigidbody.linearVelocity.magnitude < 10f)
			{
				m_Rigidbody.AddRelativeForce(Vector3.up * (10f * num) + Vector3.right * (5f * move.x), ForceMode.Acceleration);
			}
		}

		private void AnimateChangeWeapon(float deltaTime)
		{
			float num = Mathf.Lerp(omniFireObject.transform.localScale.x, m_TargetScale, deltaTime * 10f);
			fireObject.transform.localScale = new Vector3(1f - num, 1f - num, 1f - num);
			omniFireObject.transform.localScale = new Vector3(num, num, num);
		}

		private void AnimateFireWeapon(float deltaTime)
		{
			m_BeltAngle = Mathf.Lerp(m_BeltAngle, m_TargetBeltAngle, deltaTime * 10f);
			belt.transform.localEulerAngles = new Vector3(0f, m_BeltAngle, 0f);
			Vector3 localPosition = barrel.transform.localPosition;
			barrel.transform.localPosition = new Vector3(localPosition.x, Mathf.Lerp(localPosition.y, m_BarrelPosition, deltaTime * 10f), localPosition.z);
		}

		private void AnimateColors(float deltaTime)
		{
			Color color = Color.Lerp(m_Material.color, m_TargetColor, deltaTime * 2f);
			if (color != GetColor())
			{
				m_Material.SetColor(Color1, color);
			}
		}

		private static Color GetColor(bool omniFire)
		{
			if (!omniFire)
			{
				return Color.red;
			}
			return Color.yellow;
		}
	}
}
