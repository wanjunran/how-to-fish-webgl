using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LaserSight : Attachment
{
	[SerializeField]
	private float _laserLength = 5f;

	[SerializeField]
	private LineRenderer _line;

	[Header("DECAL SETTINGS")]
	[SerializeField]
	private DecalProjector _decal;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		SetLine();
	}

	private void SetLine()
	{
		_line.SetPosition(0, _line.transform.position);
		if (Physics.Raycast(_line.transform.position, _line.transform.forward, out var hitInfo, _laserLength, GameInfo.ProjectileHitLayer))
		{
			_line.SetPosition(1, hitInfo.point);
			_decal.transform.position = hitInfo.point;
			_decal.transform.forward = -_line.transform.forward;
			_decal.enabled = true;
		}
		else
		{
			_line.SetPosition(1, _line.transform.position + _line.transform.forward * _laserLength);
			_decal.enabled = false;
		}
	}

	public void Toggle(bool to)
	{
		base.enabled = to;
		_decal.enabled = to;
		_line.enabled = to;
	}
}
