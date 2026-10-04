using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemInfoWeight
{
	[FormerlySerializedAs("_itemInfo")]
	[SerializeField]
	private Fishable fishable;

	[SerializeField]
	private float _weight;

	public Fishable Fishable => fishable;

	public float Weight => _weight;
}
