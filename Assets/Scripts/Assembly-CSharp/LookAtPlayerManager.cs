using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayerManager : MonoBehaviour
{
	[SerializeField]
	private List<Transform> _lookAtPlayerObjects;

	private void LateUpdate()
	{
		if (!GameInfo.CurCamera)
		{
			return;
		}
		Vector3 forward = GameInfo.CurCamera.transform.forward;
		forward.y = 0f;
		if (forward.sqrMagnitude < 0.0001f)
		{
			return;
		}
		forward.Normalize();
		foreach (Transform lookAtPlayerObject in _lookAtPlayerObjects)
		{
			if ((bool)lookAtPlayerObject)
			{
				lookAtPlayerObject.forward = forward;
			}
		}
	}
}
