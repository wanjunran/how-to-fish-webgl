using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	private static ThirdPersonCamera _instance;

	[Header("Target")]
	public Transform target;

	[Header("Distance")]
	public float distance = 1f;

	public float minDistance = 0.5f;

	public float maxDistance = 12f;

	public float zoomSpeed = 4f;

	[Header("Orbit Settings")]
	public float sensitivityX = 1f;

	public float sensitivityY = 1f;

	public float minY = -90f;

	public float maxY = 90f;

	private float rotationX;

	private float rotationY;

	private void Start()
	{
		_instance = this;
		Vector3 eulerAngles = base.transform.eulerAngles;
		rotationX = eulerAngles.y;
		rotationY = eulerAngles.x;
		base.gameObject.SetActive(value: false);
	}

	private void SetTarget(Item item)
	{
		if ((bool)item.Fish)
		{
			target = item.Fish._joints[0].transform;
		}
	}

	public static void Toggle(bool to)
	{
		if ((bool)_instance)
		{
			_instance.gameObject.SetActive(to);
		}
	}

	private void LateUpdate()
	{
		if ((bool)target)
		{
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY -= Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minY, maxY);
			float axis = Input.GetAxis("Mouse ScrollWheel");
			distance -= axis * zoomSpeed;
			distance = Mathf.Clamp(distance, minDistance, maxDistance);
			Quaternion quaternion = Quaternion.Euler(rotationY, rotationX, 0f);
			Vector3 position = quaternion * new Vector3(0f, 0f, 0f - distance) + target.position;
			base.transform.rotation = quaternion;
			base.transform.position = position;
		}
	}
}
