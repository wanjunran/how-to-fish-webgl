using System;
using UnityEngine;

[Serializable]
public class SharpnessUpgrade
{
	[SerializeField]
	private int _damage;

	[SerializeField]
	private int _cost;

	public int Damage => _damage;

	public int Cost => _cost;
}
