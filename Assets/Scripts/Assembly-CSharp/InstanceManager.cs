using System;
using System.Collections.Generic;
using UnityEngine;

public class InstanceManager : MonoBehaviour
{
	[Serializable]
	public class InstanceType
	{
		[Tooltip("Name of the type of instance")]
		public string Name;

		[Tooltip("Mesh to be used for gpu instancing")]
		public Mesh Mesh;

		[Tooltip("Material to be used for gpu instancing")]
		public Material Material;

		public List<List<Matrix4x4>> Batches = new List<List<Matrix4x4>>();
	}

	private static InstanceManager _instance;

	private static readonly Dictionary<string, InstanceType> _instanceTypeDic = new Dictionary<string, InstanceType>();

	private static float _lastLevelPosY;

	[SerializeField]
	[NonReorderable]
	private InstanceType[] _instanceTypes;

	private void Awake()
	{
		Setter.SetSingleInstance(ref _instance, this);
		CreateDictionary();
	}

	private void Update()
	{
		RenderBatches();
	}

	public InstanceType GetInstanceType(string typeName)
	{
		InstanceType[] instanceTypes = _instanceTypes;
		foreach (InstanceType instanceType in instanceTypes)
		{
			if (instanceType.Name == typeName)
			{
				return instanceType;
			}
		}
		return null;
	}

	public static void AddInstance(string typeName, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		InstanceType instanceType = _instanceTypeDic[typeName];
		Matrix4x4 item = Matrix4x4.TRS(pos, rot, scale);
		if (instanceType.Batches.Count != 0)
		{
			List<List<Matrix4x4>> batches = instanceType.Batches;
			if (batches[batches.Count - 1].Count <= 999)
			{
				goto IL_0051;
			}
		}
		instanceType.Batches.Add(new List<Matrix4x4>());
		goto IL_0051;
		IL_0051:
		List<List<Matrix4x4>> batches2 = instanceType.Batches;
		batches2[batches2.Count - 1].Add(item);
	}

	public static void RemoveInstancesWithinBox(BoxCollider box)
	{
		foreach (KeyValuePair<string, InstanceType> item in _instanceTypeDic)
		{
			List<List<Matrix4x4>> list = new List<List<Matrix4x4>>(item.Value.Batches);
			bool flag = false;
			for (int num = list.Count - 1; num >= 0; num--)
			{
				for (int num2 = list[num].Count - 1; num2 >= 0; num2--)
				{
					Vector3 point = list[num][num2].MultiplyPoint3x4(Vector3.zero);
					if (box.bounds.Contains(Vector3.zero))
					{
						MonoBehaviour.print("test hit");
					}
					if (box.bounds.Contains(point))
					{
						list[num].RemoveAt(num2);
						flag = true;
					}
					else
					{
						MonoBehaviour.print("pair " + item.Key + " has was outside box");
					}
				}
			}
			if (flag)
			{
				ReplaceBatches(item.Key, list);
			}
		}
	}

	public static void ReplaceBatches(string typeName, List<List<Matrix4x4>> NewBatches)
	{
		_instanceTypeDic[typeName].Batches = NewBatches;
	}

	public static void Clear()
	{
		InstanceType[] instanceTypes = _instance._instanceTypes;
		foreach (InstanceType obj in instanceTypes)
		{
			obj.Batches.Clear();
			obj.Batches.Add(new List<Matrix4x4>());
		}
	}

	public static void OnNewLevelSpawned(float yPos)
	{
		_lastLevelPosY = yPos;
	}

	private static void CreateDictionary()
	{
		_instanceTypeDic.Clear();
		InstanceType[] instanceTypes = _instance._instanceTypes;
		foreach (InstanceType instanceType in instanceTypes)
		{
			_instanceTypeDic.Add(instanceType.Name, instanceType);
		}
	}

	private static void RenderBatches()
	{
		InstanceType[] instanceTypes = _instance._instanceTypes;
		foreach (InstanceType instanceType in instanceTypes)
		{
			foreach (List<Matrix4x4> batch in instanceType.Batches)
			{
				Graphics.DrawMeshInstanced(instanceType.Mesh, 0, instanceType.Material, batch);
			}
		}
	}
}
