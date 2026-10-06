using System;
using UnityEngine;

[Serializable]
public class FloatInfo
{
	[SerializeField]
	[Tooltip("Apply force to this rig at center of mass")]
	private Rigidbody _rig;

	[SerializeField]
	[Tooltip("Apply force at this position to main rig")]
	private Transform _customPoint;

	public Rigidbody Rig => _rig;

	public Vector3 Pos
	{
		get
		{
			if (!_rig)
			{
				if (!_customPoint)
				{
					return Vector3.zero;
				}
				return _customPoint.position;
			}
			return _rig.worldCenterOfMass;
		}
	}
}
