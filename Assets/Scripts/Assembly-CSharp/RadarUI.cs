using System;
using UnityEngine;

public class RadarUI : MonoBehaviour
{
	[Header("Zoom")]
	[SerializeField]
	private float _defaultZoom = 15f;

	[SerializeField]
	private float _zoomSpeed = 15f;

	[SerializeField]
	private float _zoomAcc = 1.5f;

	[SerializeField]
	private float _maxZoomAccMultiplier = 3f;

	[SerializeField]
	private Vector2 _zoomMinMax = new Vector2(5f, 50f);

	[Header("Scale")]
	[SerializeField]
	private float _mapScale = 0.01f;

	[Header("Rotation")]
	[SerializeField]
	private RectTransform _mapRotation;

	[SerializeField]
	[Tooltip("Use this if the radar should rotate around the boat instead of the player")]
	private bool _useBoatRotation;

	[SerializeField]
	private float _radarRotSpeed = 0.25f;

	[Header("Icons Settings")]
	[SerializeField]
	private RectTransform _localPlayerDot;

	[SerializeField]
	private float _maxPosDist = 0.107f;

	[SerializeField]
	private float _angleForPing = 1f;

	[SerializeField]
	private MapDot[] _otherPlayerDots;

	[SerializeField]
	private MapDot[] _islandDots;

	[SerializeField]
	private MapDot _lastDeathDot;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	private bool _isOn;

	private Vector2 _localPlayerPos;

	private Vector2 _radarSweepDir;

	private float _curRot;

	private float _curRadarRot;

	private float _curZoomSpeed;

	private float _curZoomTickTime;

	private float _curZoom;

	private float _zoomMultiplier;

	private bool _isZoomingIn;

	private bool _isZoomingOut;

	private static Vector3 _lastDeathPos;

	private void Awake()
	{
		HideDots(_otherPlayerDots);
		HideDots(_islandDots);
		_lastDeathDot.Hide();
		_curZoom = Mathf.Clamp(_defaultZoom, _zoomMinMax.x, _zoomMinMax.y);
		_zoomMultiplier = 1f / _curZoom;
		ShaderManager.UpdateMapZoom(_curZoom);
	}

	public static void SetLastDeathPos(Vector3 pos)
	{
		_lastDeathPos = pos;
	}

	public void ToggleIsZoomingIn(bool to)
	{
		_isZoomingIn = to;
		if (!Player.LocalPlayer || Player.LocalPlayer.BlockInputs)
		{
			_isZoomingIn = false;
		}
	}

	public void ToggleIsZoomingOut(bool to)
	{
		_isZoomingOut = to;
		if (!Player.LocalPlayer || Player.LocalPlayer.BlockInputs)
		{
			_isZoomingOut = false;
		}
	}

	private void Update()
	{
		if (_isOn && (_useBoatRotation || (bool)Player.LocalPlayer) && (!_useBoatRotation || (bool)BoatManager.Boat))
		{
			Zoom();
			Move();
			Rotate();
			UpdateAllDots();
		}
	}

	private void Zoom()
	{
		float num = _curZoom;
		if (!_isZoomingIn && !_isZoomingOut)
		{
			_curZoomSpeed = 1f;
		}
		else
		{
			if (_curZoomSpeed < _maxZoomAccMultiplier)
			{
				_curZoomSpeed += _curZoomSpeed * _zoomAcc * Time.deltaTime;
			}
			if (_isZoomingIn)
			{
				num -= _zoomSpeed * _curZoomSpeed * Time.deltaTime;
			}
			if (_isZoomingOut)
			{
				num += _zoomSpeed * _curZoomSpeed * Time.deltaTime;
			}
		}
		num = Mathf.Clamp(num, _zoomMinMax.x, _zoomMinMax.y);
		if (num != _curZoom)
		{
			_zoomMultiplier = 1f / num;
			ShaderManager.UpdateMapZoom(num);
			_curZoomTickTime += _curZoomSpeed * Time.deltaTime;
			if (_curZoomTickTime >= 0.2f)
			{
				_curZoomTickTime = 0f;
				AudioManager.PlayGlobalClip(_isZoomingIn ? "MapZoomIn" : "MapZoomOut", variation: false, 0.5f, 0.025f);
			}
		}
		_curZoom = num;
	}

	private void Move()
	{
		Transform transform = ((!_useBoatRotation) ? Player.LocalPlayer.Transform : BoatManager.Boat.VisualBoat);
		Vector2 vector = new Vector2(transform.position.x, transform.position.z);
		if (vector != _localPlayerPos)
		{
			ShaderManager.UpdatePlayerOffset(vector * _mapScale);
		}
		_localPlayerPos = vector;
	}

	private void Rotate()
	{
		float y = ((!_useBoatRotation) ? Player.LocalPlayer.Camera.CamTransform : BoatManager.Boat.VisualPhysicsRig.transform).rotation.eulerAngles.y;
		if (y != _curRot)
		{
			ShaderManager.UpdatePlayerRotation(y);
			_mapRotation.localRotation = Quaternion.Euler(0f, 0f, y);
		}
		_curRot = y;
		_curRadarRot += _radarRotSpeed * Time.deltaTime;
		ShaderManager.UpdateRadarRotation(_curRadarRot);
		float f = (0f - _curRadarRot) * 2f * MathF.PI;
		_radarSweepDir = new Vector2(0f - Mathf.Sin(f), Mathf.Cos(f));
	}

	private void UpdateAllDots()
	{
		UpdatePlayerDots();
		UpdateIslandDots();
		UpdateLastDeathDot();
	}

	private void UpdateLastDeathDot()
	{
		if (!(_lastDeathPos == Vector3.zero))
		{
			UpdateDot(_lastDeathDot, _lastDeathPos);
		}
	}

	private void UpdatePlayerDots()
	{
		int num = 0;
		for (int i = 0; i < PlayerManager.OtherPlayers.Count; i++)
		{
			if (num >= _otherPlayerDots.Length)
			{
				break;
			}
			Player player = PlayerManager.OtherPlayers[i];
			UpdateDot(_otherPlayerDots[num], player.Transform.position);
			num++;
		}
	}

	private void UpdateIslandDots()
	{
		int num = 0;
		for (int i = 0; i < OnlineIslandManager.MaxIslandUnlocked; i++)
		{
			if (num >= _islandDots.Length)
			{
				break;
			}
			IslandInfo islandInfo = IslandManager.GetIslandInfo(i);
			UpdateDot(_islandDots[num], islandInfo.IslandPosition);
			num++;
		}
	}

	private void UpdateDot(MapDot dot, Vector3 position)
	{
		MoveDot(dot, position);
		if (!dot.RecentlyPinged && Vector2.Angle(dot.RealtimeLocalPos, _radarSweepDir) <= _angleForPing)
		{
			dot.TriggerPing();
			AudioManager.PlayClipAt("RadarPing", base.transform.position, variation: false, AudioDistance.Short, 0.1f);
		}
	}

	private void MoveDot(MapDot dot, Vector3 worldPos)
	{
		Vector2 realtimeWorldPos = new Vector2(worldPos.x, worldPos.z);
		dot.Move(realtimeWorldPos, _localPlayerPos, _mapScale, _zoomMultiplier, _maxPosDist);
	}

	private void HideDots(MapDot[] dots)
	{
		for (int i = 0; i < dots.Length; i++)
		{
			dots[i].Hide();
		}
	}

	public void ToggleIsOn(bool to, bool isInInventory)
	{
		_isOn = to;
		float to2 = (to ? 1 : 0);
		if (!to)
		{
			_isZoomingIn = false;
			_isZoomingOut = false;
		}
		if (!isInInventory)
		{
			LeanTween.cancel(_canvasGroup.gameObject);
			LeanTween.value(_canvasGroup.gameObject, _canvasGroup.alpha, to2, 0.5f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(UpdateCanvasAlpha);
		}
	}

	private void UpdateCanvasAlpha(float to)
	{
		_canvasGroup.alpha = to;
	}
}
