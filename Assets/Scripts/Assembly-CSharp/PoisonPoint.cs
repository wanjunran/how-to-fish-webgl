using UnityEngine;

public class PoisonPoint
{
	private Vector3 _pos;

	private float _lifeTime;

	public Vector3 Pos => _pos;

	public float LifeTime => _lifeTime;

	public PoisonPoint(Vector3 pos)
	{
		_pos = pos;
		_lifeTime = 0f;
	}

	public void IncreaseLifetime()
	{
		_lifeTime += Time.fixedDeltaTime;
	}
}
