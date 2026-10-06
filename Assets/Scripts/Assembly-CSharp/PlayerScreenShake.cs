using System.Collections;
using UnityEngine;

public class PlayerScreenShake : MonoBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	[Tooltip("Joint for camera to follow")]
	private Rigidbody2D _jointedRig;

	[SerializeField]
	[Tooltip("Time between adding force to shake")]
	private float _timeBetweenReps;

	[SerializeField]
	[Tooltip("Screenshake position multiplier")]
	private float _posAmount;

	[SerializeField]
	[Tooltip("Screenshake rotation multiplier")]
	private float _rotateAmount;

	[SerializeField]
	[Tooltip("Force applied when camera is constantly shaking")]
	private float _constantShakeAmount;

	private bool _isConstShaking;

	private float _shakeMultiplier = 1f;

	private void Update()
	{
		_player.Camera.SetShakePos(_jointedRig.transform.localPosition);
		_player.ToolMovement.SetShakePos(_jointedRig.transform.localPosition);
	}

	public void SetShakeMultiplier(float to)
	{
		_shakeMultiplier = to;
	}

	public void ConstantShake(bool enabled)
	{
		if (!(_isConstShaking & enabled))
		{
			_isConstShaking = enabled;
			if (_isConstShaking)
			{
				StartCoroutine(ShakeAgain(_constantShakeAmount, 0));
			}
		}
	}

	public void ShakeAt(Vector3 pos, int repetitions = 1, float amount = 1000f, float minRange = 10f, float maxRange = 100f, Vector2 direction = default(Vector2))
	{
		float num = Vector3.Distance(Player.LocalPlayer.Transform.position, pos);
		if (num <= maxRange)
		{
			float num2 = Mathf.InverseLerp(minRange, maxRange, num);
			num2 = 1f - num2;
			num2 *= amount;
			Shake(num2, repetitions, direction);
		}
	}

	public void Shake(float force, int repetitions, Vector2 direction = default(Vector2))
	{
		if (!_isConstShaking)
		{
			if (direction == Vector2.zero)
			{
				direction = Random.insideUnitCircle.normalized;
			}
			AddShakeForce(direction * force);
			if (repetitions > 0)
			{
				StartCoroutine(ShakeAgain(force, repetitions));
			}
		}
	}

	private IEnumerator ShakeAgain(float force, int reps)
	{
		while (_isConstShaking)
		{
			yield return new WaitForSeconds(_timeBetweenReps);
			AddShakeForce(Random.insideUnitCircle.normalized * force);
		}
		for (int i = 0; i < reps; i++)
		{
			yield return new WaitForSeconds(_timeBetweenReps);
			AddShakeForce(Random.insideUnitCircle.normalized * force / (i + 1));
		}
	}

	private void AddShakeForce(Vector3 force)
	{
		_jointedRig.AddForce(force * _shakeMultiplier);
	}
}
