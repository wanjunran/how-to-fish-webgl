using System;
using UnityEngine;

[Serializable]
public class HandTransforms
{
	[SerializeField]
	private bool _exists;

	[SerializeField]
	private Vector3 _handPos;

	[SerializeField]
	private Quaternion _handRot;

	[SerializeField]
	private Quaternion[] _fingerRots;

	[SerializeField]
	private Transform _parent;

	public bool Exists => _exists;

	public Vector3 HandPos => _handPos;

	public Quaternion HandRot => _handRot;

	public Quaternion[] FingerRots => _fingerRots;

	public Transform Parent => _parent;

	public HandTransforms(bool exists, Vector3 handPos, Quaternion handRot, Quaternion[] fingerRots, Transform parent)
	{
		_exists = exists;
		_handPos = handPos;
		_handRot = handRot;
		_fingerRots = fingerRots;
		_parent = parent;
	}

	public HandTransforms(bool exists)
	{
		_exists = exists;
	}
}
