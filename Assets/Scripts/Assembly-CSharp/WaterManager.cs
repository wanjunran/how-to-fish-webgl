using System.Collections.Generic;
using FishNet;
using UnityEngine;
using UnityEngine.Rendering;

public class WaterManager : MonoBehaviour
{
	private static WaterManager _instance;

	[SerializeField]
	private float _itemMinForce;

	[SerializeField]
	private float _itemAngularVelocity = 3f;

	[SerializeField]
	private float _itemWaterMaxVelocity = 1f;

	[SerializeField]
	private float _itemBouyancy = 25f;

	[SerializeField]
	private float _boatWaterForce = 800f;

	[SerializeField]
	private float _itemWaterDrag = 1.5f;

	[SerializeField]
	private float _itemAngularWaterDrag = 2f;

	[Space]
	[SerializeField]
	private Material _waterMat;

	[SerializeField]
	private Mesh _waterMesh;

	[SerializeField]
	private Transform _waterHolder;

	[SerializeField]
	private Transform _mainWater;

	[SerializeField]
	private Renderer[] _waterTiles;

	[SerializeField]
	private MeshFilter[] _waterFilters;

	[SerializeField]
	private int _tileSize = 200;

	private static readonly int WaveFrequency1ID = Shader.PropertyToID("_Wave_Frequency_1");

	private static readonly int WaveFrequency2ID = Shader.PropertyToID("_Wave_Frequency_2");

	private static readonly int GradientSpeed1ID = Shader.PropertyToID("_Gradient_Speed_1");

	private static readonly int GradientSpeed2ID = Shader.PropertyToID("_Gradient_Speed_2");

	private static readonly int WaveDirection1ID = Shader.PropertyToID("_Wave_Direction_1");

	private static readonly int WaveDirection2ID = Shader.PropertyToID("_Wave_Direction_2");

	private static readonly int WaveHeight1ID = Shader.PropertyToID("_Wave_Height_1");

	private static readonly int WaveHeight2ID = Shader.PropertyToID("_Wave_Height_2");

	private static readonly int Peak1ID = Shader.PropertyToID("_Peak_1");

	private static readonly int Peak2ID = Shader.PropertyToID("_Peak_2");

	private static readonly int FakeTimeID = Shader.PropertyToID("_Fake_Time");

	private static readonly int WaveOffsetID = Shader.PropertyToID("_Wave_Offset");

	private static float _waveFrequency1;

	private static float _waveFrequency2;

	private static float _gradientSpeed1;

	private static float _gradientSpeed2;

	private static Vector3 _waveDirection1;

	private static Vector3 _waveDirection2;

	private static float _waveHeight1;

	private static float _waveHeight2;

	private static float _peak1;

	private static float _peak2;

	private static float _modelScale;

	private static float _waveOffset;

	private static float _waterLevel;

	private Vector2Int _prevTargetTile;

	private bool _waterTilesInitialized;

	private float _curDelay;

	private bool _isUnderwater;

	private Transform _customTarget;

	public static float ItemAngularVelocty => _instance._itemAngularVelocity;

	public static float ItemMinForce => _instance._itemMinForce;

	public static float ItemBouyancy => _instance._itemBouyancy;

	public static float WaterHeight => _instance._mainWater.position.y;

	public static float ItemWaterMaxVelocity => _instance._itemWaterMaxVelocity;

	public static float ItemWaterDrag => _instance._itemWaterDrag;

	public static float ItemAngularDrag => _instance._itemAngularWaterDrag;

	public static float BoatWaterForce => _instance._boatWaterForce;

	private void Awake()
	{
		Renderer[] waterTiles = _waterTiles;
		for (int i = 0; i < waterTiles.Length; i++)
		{
			waterTiles[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		_waterHolder.position = Vector3.zero;
		_modelScale = _mainWater.localScale.x;
		_waterLevel = _mainWater.position.y;
		Setter.SetSingleInstance(ref _instance, this);
		_waveFrequency1 = _waterMat.GetFloat(WaveFrequency1ID);
		_waveFrequency2 = _waterMat.GetFloat(WaveFrequency2ID);
		_gradientSpeed1 = _waterMat.GetFloat(GradientSpeed1ID);
		_gradientSpeed2 = _waterMat.GetFloat(GradientSpeed2ID);
		_waveDirection1 = _waterMat.GetVector(WaveDirection1ID);
		_waveDirection2 = _waterMat.GetVector(WaveDirection2ID);
		_waveHeight1 = _waterMat.GetFloat(WaveHeight1ID);
		_waveHeight2 = _waterMat.GetFloat(WaveHeight2ID);
		_peak1 = _waterMat.GetFloat(Peak1ID);
		_peak2 = _waterMat.GetFloat(Peak2ID);
		ToggleUnderwater(to: false);
	}

	private void OnDestroy()
	{
		ToggleUnderwater(to: false);
	}

	public static void OnTickUpdate()
	{
		if ((bool)_instance)
		{
			double num = InstanceFinder.TimeManager.TicksToTime(InstanceFinder.TimeManager.Tick);
			_instance.SetWaterOffset((float)num);
		}
	}

	private void LateUpdate()
	{
		_waterMat.SetFloat(FakeTimeID, Time.time);
		CheckUnderwater();
		CheckPlayerPos();
	}

	private void CheckUnderwater()
	{
		if (!Player.LocalPlayer)
		{
			ToggleUnderwater(to: false);
			return;
		}
		Transform transform = Player.LocalPlayer.CurCam.transform;
		if (transform.position.y < GetWaterHeight(transform.position) + 0.1f)
		{
			ToggleUnderwater(to: true);
		}
		else
		{
			ToggleUnderwater(to: false);
		}
	}

	private void ToggleUnderwater(bool to, bool forced = false)
	{
		if (_isUnderwater != to || forced)
		{
			_isUnderwater = to;
			Renderer[] waterTiles = _waterTiles;
			for (int i = 0; i < waterTiles.Length; i++)
			{
				waterTiles[i].enabled = !to;
			}
			ShaderManager.SetFog(to ? FogState.Underwater : FogState.Default);
			Player.LocalPlayer.Underwater.SetSoundsPlaying(to);
			CanvasManager.ToggleUnderwaterImage(to);
		}
	}

	public static void SetCustomWaterTarget(Transform target)
	{
		if ((bool)_instance)
		{
			_instance._customTarget = target;
		}
	}

	private void CheckPlayerPos()
	{
		if (!_customTarget && !Player.LocalPlayer)
		{
			if (_waterTilesInitialized || _prevTargetTile != Vector2Int.zero)
			{
				SnapAllWaterTiles();
				AssignTilesToGrid(Vector2Int.zero);
			}
			_prevTargetTile = Vector2Int.zero;
			_waterTilesInitialized = false;
			return;
		}
		Transform transform = (_customTarget ? _customTarget : Player.LocalPlayer.Transform);
		if (!_customTarget && (bool)Player.LocalPlayer.Dying.DeadPlayer)
		{
			transform = Player.LocalPlayer.Dying.DeadPlayer.transform;
		}
		Vector2Int vector2Int = WorldPosToTileIndex(transform.position);
		if (!_waterTilesInitialized)
		{
			_prevTargetTile = vector2Int;
			_waterTilesInitialized = true;
			SnapAllWaterTiles();
			AssignTilesToGrid(vector2Int);
		}
		else
		{
			if (vector2Int != _prevTargetTile)
			{
				MoveWater(vector2Int);
			}
			_prevTargetTile = vector2Int;
		}
	}

	private Vector2Int WorldPosToTileIndex(Vector3 worldPos)
	{
		float num = (float)_tileSize * 0.5f;
		return new Vector2Int(Mathf.FloorToInt((worldPos.x + num) / (float)_tileSize), Mathf.FloorToInt((worldPos.z + num) / (float)_tileSize));
	}

	private Vector2Int LocalPosToTileIndex(Vector3 localPos)
	{
		float num = (float)_tileSize * 0.5f;
		return new Vector2Int(Mathf.FloorToInt((localPos.x + num) / (float)_tileSize), Mathf.FloorToInt((localPos.z + num) / (float)_tileSize));
	}

	private Vector3 TileIndexToLocalPos(Vector2Int tileIndex)
	{
		return new Vector3(tileIndex.x * _tileSize, _waterLevel, tileIndex.y * _tileSize);
	}

	private void SnapAllWaterTiles()
	{
		Renderer[] waterTiles = _waterTiles;
		foreach (Renderer renderer in waterTiles)
		{
			Vector2Int tileIndex = LocalPosToTileIndex(renderer.transform.localPosition);
			renderer.transform.localPosition = TileIndexToLocalPos(tileIndex);
		}
	}

	private void MoveWater(Vector2Int newTile)
	{
		Vector2Int vector2Int = newTile - _prevTargetTile;
		if (vector2Int == Vector2Int.zero)
		{
			return;
		}
		if (Mathf.Abs(vector2Int.x) > 1 || Mathf.Abs(vector2Int.y) > 1)
		{
			AssignTilesToGrid(newTile);
			return;
		}
		Renderer[] waterTiles = _waterTiles;
		foreach (Renderer renderer in waterTiles)
		{
			Vector2Int tileIndex = LocalPosToTileIndex(renderer.transform.localPosition);
			Vector2Int zero = Vector2Int.zero;
			if (vector2Int.x != 0 && Mathf.Abs(newTile.x - tileIndex.x) >= 2)
			{
				zero.x = vector2Int.x * 3;
			}
			if (vector2Int.y != 0 && Mathf.Abs(newTile.y - tileIndex.y) >= 2)
			{
				zero.y = vector2Int.y * 3;
			}
			if (!(zero == Vector2Int.zero))
			{
				tileIndex += zero;
				renderer.transform.localPosition = TileIndexToLocalPos(tileIndex);
			}
		}
	}

	private void AssignTilesToGrid(Vector2Int centerTile)
	{
		List<Renderer> list = new List<Renderer>(_waterTiles);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Vector2Int tileIndex = new Vector2Int(centerTile.x + j, centerTile.y + i);
				int index = 0;
				int num = int.MaxValue;
				for (int k = 0; k < list.Count; k++)
				{
					Vector2Int vector2Int = LocalPosToTileIndex(list[k].transform.localPosition);
					int num2 = vector2Int.x - tileIndex.x;
					int num3 = vector2Int.y - tileIndex.y;
					int num4 = num2 * num2 + num3 * num3;
					if (num4 < num)
					{
						num = num4;
						index = k;
						if (num4 == 0)
						{
							break;
						}
					}
				}
				Renderer renderer = list[index];
				list.RemoveAt(index);
				renderer.transform.localPosition = TileIndexToLocalPos(tileIndex);
			}
		}
	}

	private void SetWaterOffset(float offset)
	{
		if ((bool)_instance)
		{
			float value = (_waveOffset = offset - Time.time);
			_instance._waterMat.SetFloat(WaveOffsetID, value);
		}
	}

	public static bool IsUnderWater(Vector3 worldPos)
	{
		return GetWaterHeight(worldPos) >= worldPos.y;
	}

	public static KeyValuePair<bool, float> GetWaterInfo(Vector3 worldPos)
	{
		float waterHeight = GetWaterHeight(worldPos);
		return new KeyValuePair<bool, float>(value: waterHeight - worldPos.y, key: waterHeight >= worldPos.y);
	}

	public static float GetWaterHeight(Vector3 worldPos)
	{
		Vector3 vector = GerstnerWave(worldPos, _waveFrequency1, _gradientSpeed1, _waveDirection1, _waveHeight1, _peak1);
		Vector3 vector2 = GerstnerWave(worldPos, _waveFrequency2, _gradientSpeed2, _waveDirection2, _waveHeight2, _peak2);
		return _waterLevel + (vector.y + vector2.y) * _modelScale;
	}

	private static Vector3 GerstnerWave(Vector3 worldPos, float frequency, float gradientSpeed, Vector3 direction, float height, float peak)
	{
		direction.Normalize();
		float num = Vector3.Dot(direction, worldPos) * frequency;
		float num2 = (Time.time + _waveOffset) * gradientSpeed;
		float num3 = Mathf.Sin(num + num2);
		num3 *= height;
		return Vector3.up * num3;
	}
}
