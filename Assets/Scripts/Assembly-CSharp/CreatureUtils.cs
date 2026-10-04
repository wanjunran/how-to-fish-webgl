using System;
using UnityEngine;

public class CreatureUtils
{
	public static float GetWeightWithStandardDeviation(int seed)
	{
		return Mathf.Clamp(NextGaussian(new System.Random(GameInfo.Seed + seed)), 0.25f, 1.75f);
	}

	private static float NextGaussian(System.Random rng, float mean = 1f, float stdDev = 0.15f)
	{
		float x = 1f - (float)rng.NextDouble();
		float num = 1f - (float)rng.NextDouble();
		float num2 = MathF.Sqrt(-2f * MathF.Log(x)) * MathF.Sin(MathF.PI * 2f * num);
		return mean + stdDev * num2;
	}

	public static Vector3 GetFlyDirection(Bird bird)
	{
		if ((bool)bird.AttackingItem)
		{
			Vector3 direction = bird.AttackingItem.transform.position - bird.transform.position;
			if (!bird.AttackingItem.Holder && !bird.AttackingItem.IsDeinitializing && !bird.AttackingItem.BirdHolder && bird.AttackingItem.IsInteractable && !Physics.Raycast(bird.transform.position, direction, direction.magnitude - 0.15f, GameInfo.LevelLayer) && !Physics.Raycast(bird.AttackingItem.transform.position, Vector3.up, 10f, GameInfo.LevelLayer))
			{
				return bird.AttackingItem.transform.position - bird.transform.position;
			}
			bird.SetAttackingFood(null);
		}
		return GetFlyDirection(bird.transform, bird.FlyHeigth, BirdManager.Instance.HomeRadiusSqr, BirdManager.Instance.HomeWeight, bird.IsIdlingRight);
	}

	public static Vector3 GetFlyDirection(Albatross albatross)
	{
		if ((bool)albatross.Target)
		{
			return albatross.Target.position - albatross.transform.position;
		}
		return GetFlyDirection(albatross.transform, albatross.FlyHeight, albatross.HomeRadius * albatross.HomeRadius, albatross.HomeWeight, albatross.IsIdlingRight);
	}

	private static Vector3 GetFlyDirection(Transform birdTrans, float homeHeigth, float homeRadiusSqr, float homeWeight, bool isIdlingRight)
	{
		Vector3 vector = (Player.LocalPlayer ? Player.LocalPlayer.Transform.position : birdTrans.position);
		vector.y = homeHeigth;
		Vector3 vector2 = vector - birdTrans.position;
		float sqrMagnitude = vector2.sqrMagnitude;
		Vector3 vector3 = Vector3.zero;
		if (sqrMagnitude > homeRadiusSqr)
		{
			vector3 = vector2.normalized;
		}
		Vector3 zero = Vector3.zero;
		Vector3 forward = birdTrans.forward;
		Vector3 position = birdTrans.position;
		Vector3 vector4 = (isIdlingRight ? birdTrans.right : (-birdTrans.right));
		if (Physics.Raycast(birdTrans.position, birdTrans.forward, out var hitInfo, BirdManager.Instance.LevelAvoidDistance, GameInfo.LevelLayer))
		{
			Vector3 normal = hitInfo.normal;
			normal.y = 0f;
			zero += normal.normalized;
		}
		for (int i = 0; i < BirdManager.Instance.DirectionalLevelRays; i++)
		{
			Vector3 direction = Quaternion.Euler(0f, (0f - BirdManager.Instance.RayAngleOffset) * (float)(i + 1), 0f) * forward;
			if (Physics.Raycast(position, direction, out var hitInfo2, BirdManager.Instance.LevelAvoidDistance, GameInfo.LevelLayer))
			{
				Vector3 normal2 = hitInfo2.normal;
				normal2.y = 0f;
				zero += normal2.normalized;
			}
			Vector3 direction2 = Quaternion.Euler(0f, BirdManager.Instance.RayAngleOffset * (float)(i + 1), 0f) * forward;
			if (Physics.Raycast(position, direction2, out var hitInfo3, BirdManager.Instance.LevelAvoidDistance, GameInfo.LevelLayer))
			{
				Vector3 normal3 = hitInfo3.normal;
				normal3.y = 0f;
				zero += normal3.normalized;
			}
		}
		Vector3 result = vector3 * homeWeight + forward * BirdManager.Instance.ForwardWeight + vector4 * BirdManager.Instance.IdleWeight + zero * BirdManager.Instance.LevelAvoidanceWeight;
		result.y = 0f;
		return result;
	}
}
