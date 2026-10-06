using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DecorationManager : MonoBehaviour
{
	[Serializable]
	private class DecorationType
	{
		public string InstanceName;

		public float MeshSize;

		public float Density;

		[Range(0f, 1f)]
		public float PositionRandomness;

		[Range(0f, 360f)]
		public float RotationRandomness;

		public Vector2 ScaleRange;

		public List<Matrix4x4> SavedBatch = new List<Matrix4x4>();

		public List<List<Matrix4x4>> SplitBatches = new List<List<Matrix4x4>>();

		[HideInInspector]
		public Mesh Mesh;

		[HideInInspector]
		public Material Material;
	}

	private static DecorationManager _instance;

	[SerializeField]
	[NonReorderable]
	private DecorationType[] _decorationTypes;

	[SerializeField]
	[Tooltip("No decorations will spawn below this height")]
	private float _minHeight;

	[SerializeField]
	private bool _spawnDecorations;

	[SerializeField]
	private bool _renderDecorations = true;

	private bool _batchesSplit;

	private static bool _staticRenderDecorations;

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		if (!_batchesSplit)
		{
			SplitSavedBatches();
		}
		if (Application.isPlaying)
		{
			SendBatchesToInstanceManager(enable: true);
		}
	}

	public static void ToggleLevelDecorations(bool to)
	{
		_staticRenderDecorations = to;
		if ((bool)_instance && _instance._batchesSplit && Application.isPlaying)
		{
			_instance.SendBatchesToInstanceManager(to);
		}
	}

	private void OnEnable()
	{
		if (_batchesSplit && Application.isPlaying)
		{
			SendBatchesToInstanceManager(enable: true);
		}
	}

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			_instance.SendBatchesToInstanceManager(enable: false);
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}
		if (_spawnDecorations)
		{
			_spawnDecorations = false;
			SpawnDecorations();
		}
		if (_renderDecorations)
		{
			if (!_batchesSplit || (_decorationTypes.Length != 0 && _decorationTypes[0].SavedBatch.Count > 0 && _decorationTypes[0].SplitBatches.Count == 0))
			{
				SplitSavedBatches();
			}
			RenderBatches();
		}
	}

	private void SpawnDecorations()
	{
		DecorationType[] decorationTypes = _decorationTypes;
		foreach (DecorationType decorationType in decorationTypes)
		{
			decorationType.SplitBatches.Clear();
			decorationType.SavedBatch.Clear();
			InstanceManager.InstanceType instanceType = UnityEngine.Object.FindAnyObjectByType<InstanceManager>().GetInstanceType(decorationType.InstanceName);
			decorationType.Material = instanceType.Material;
			decorationType.Mesh = instanceType.Mesh;
			int num = Mathf.RoundToInt(150f / decorationType.MeshSize * decorationType.Density);
			float num2 = (float)num * 0.5f;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num; k++)
				{
					float z = UnityEngine.Random.Range(0f - decorationType.RotationRandomness, decorationType.RotationRandomness);
					Vector3 pos = base.transform.position + new Vector3((float)j - num2, 0f, (float)k - num2) * decorationType.MeshSize / decorationType.Density;
					pos += new Vector3(UnityEngine.Random.Range(0f - decorationType.PositionRandomness, decorationType.PositionRandomness), 0f, UnityEngine.Random.Range(0f - decorationType.PositionRandomness, decorationType.PositionRandomness)) * (decorationType.MeshSize * 0.5f);
					float num3 = UnityEngine.Random.Range(decorationType.ScaleRange.x, decorationType.ScaleRange.y);
					if (GroundCheck(decorationType, pos))
					{
						RaycastHit groundHit = GetGroundHit(pos);
						pos.y = groundHit.point.y;
						Quaternion rot = Quaternion.LookRotation(groundHit.normal) * Quaternion.Euler(0f, 0f, z);
						AddInstance(decorationType, pos, rot, Vector3.one * num3);
					}
				}
			}
		}
	}

	private bool GroundCheck(DecorationType type, Vector3 pos)
	{
		int num = 0;
		float num2 = type.MeshSize * 0.5f;
		for (int i = 0; i < 5; i++)
		{
			Vector3 vector = i switch
			{
				0 => new Vector3(0f, 0f, 0f), 
				1 => new Vector3(num2, 0f, 0f - num2), 
				2 => new Vector3(0f - num2, 0f, num2), 
				3 => new Vector3(num2, 0f, num2), 
				4 => new Vector3(0f - num2, 0f, 0f - num2), 
				_ => new Vector3(1337f, 420f, 69f), 
			};
			bool flag = false;
			RaycastHit[] array = Physics.RaycastAll(pos + vector + Vector3.up * 100f, Vector3.down, 100f - _minHeight, LayerMask.GetMask("Level"));
			int num3 = 0;
			if (num3 < array.Length)
			{
				_ = ref array[num3];
				flag = true;
			}
			if (Physics.RaycastAll(pos + vector + Vector3.up * 100f, Vector3.down, 200f, LayerMask.GetMask("DecorationBlock")).Length != 0)
			{
				return false;
			}
			if (flag)
			{
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	private RaycastHit GetGroundHit(Vector3 pos)
	{
		Physics.Raycast(pos + Vector3.up * 100f, Vector3.down, out var hitInfo, 100f - _minHeight, LayerMask.GetMask("Level"));
		return hitInfo;
	}

	private void AddInstance(DecorationType type, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		Matrix4x4 item = Matrix4x4.TRS(pos, rot, scale);
		if (type.SplitBatches.Count != 0)
		{
			List<List<Matrix4x4>> splitBatches = type.SplitBatches;
			if (splitBatches[splitBatches.Count - 1].Count <= 999)
			{
				goto IL_0046;
			}
		}
		type.SplitBatches.Add(new List<Matrix4x4>());
		goto IL_0046;
		IL_0046:
		List<List<Matrix4x4>> splitBatches2 = type.SplitBatches;
		splitBatches2[splitBatches2.Count - 1].Add(item);
		type.SavedBatch.Add(item);
	}

	private void SplitSavedBatches()
	{
		DecorationType[] decorationTypes = _decorationTypes;
		foreach (DecorationType decorationType in decorationTypes)
		{
			using List<Matrix4x4>.Enumerator enumerator = decorationType.SavedBatch.GetEnumerator();
			List<List<Matrix4x4>> splitBatches2;
			Matrix4x4 current;
			for (; enumerator.MoveNext(); splitBatches2 = decorationType.SplitBatches, splitBatches2[splitBatches2.Count - 1].Add(current))
			{
				current = enumerator.Current;
				if (decorationType.SplitBatches.Count != 0)
				{
					List<List<Matrix4x4>> splitBatches = decorationType.SplitBatches;
					if (splitBatches[splitBatches.Count - 1].Count <= 999)
					{
						continue;
					}
				}
				decorationType.SplitBatches.Add(new List<Matrix4x4>());
			}
		}
		_batchesSplit = true;
	}

	private void SendBatchesToInstanceManager(bool enable)
	{
		InstanceManager.OnNewLevelSpawned(base.transform.position.y);
		DecorationType[] decorationTypes = _decorationTypes;
		foreach (DecorationType decorationType in decorationTypes)
		{
			List<List<Matrix4x4>> list = new List<List<Matrix4x4>>();
			if (_staticRenderDecorations & enable)
			{
				foreach (List<Matrix4x4> splitBatch in decorationType.SplitBatches)
				{
					List<Matrix4x4> list2 = new List<Matrix4x4>();
					foreach (Matrix4x4 item in splitBatch)
					{
						list2.Add(item);
					}
					list.Add(list2);
				}
			}
			InstanceManager.ReplaceBatches(decorationType.InstanceName, list);
		}
	}

	private void RenderBatches()
	{
		DecorationType[] decorationTypes = _decorationTypes;
		foreach (DecorationType decorationType in decorationTypes)
		{
			foreach (List<Matrix4x4> splitBatch in decorationType.SplitBatches)
			{
				Graphics.DrawMeshInstanced(decorationType.Mesh, 0, decorationType.Material, splitBatch);
			}
		}
	}
}
