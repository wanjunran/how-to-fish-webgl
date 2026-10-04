using UnityEngine;

public class HitmarkerManager : MonoBehaviour
{
	public static HitmarkerManager Instance;

	[SerializeField]
	private int _poolSize;

	[SerializeField]
	private float _despawnDelay;

	[SerializeField]
	private float _size;

	[SerializeField]
	private AnimationCurve _sizeCurve;

	[SerializeField]
	private Transform _hitMarkerHolder;

	[SerializeField]
	private Mesh _hitMarkerModel;

	[SerializeField]
	private Mesh _hitMarkerKillModel;

	[SerializeField]
	private GameObject _hitMarkerPrefab;

	private HitMarker[] _hitMarkers;

	private int _curHitMarker;

	private void Start()
	{
		Instance = this;
		_hitMarkers = new HitMarker[_poolSize];
		for (int i = 0; i < _poolSize; i++)
		{
			GameObject gameObject = Object.Instantiate(_hitMarkerPrefab, Vector3.up * 10000f, Quaternion.identity, _hitMarkerHolder);
			HitMarker hitMarker = new HitMarker
			{
				Transform = gameObject.transform,
				Mesh = gameObject.GetComponent<MeshFilter>()
			};
			_hitMarkers[i] = hitMarker;
		}
	}

	private void Update()
	{
		HitMarker[] hitMarkers = _hitMarkers;
		foreach (HitMarker hitMarker in hitMarkers)
		{
			if (!(hitMarker.Timer <= 0f))
			{
				hitMarker.Transform.LookAt(GameInfo.CurCamera.transform);
				hitMarker.Timer -= Time.deltaTime / _despawnDelay;
				hitMarker.Transform.localScale = Vector3.one * (_size * _sizeCurve.Evaluate((hitMarker.Mesh.mesh == _hitMarkerModel) ? hitMarker.Timer : (hitMarker.Timer / 2f)));
			}
		}
	}

	public void AddHitMarker(Vector3 pos, bool kill)
	{
		_hitMarkers[_curHitMarker].Transform.position = pos;
		_hitMarkers[_curHitMarker].Mesh.mesh = (kill ? _hitMarkerKillModel : _hitMarkerModel);
		_hitMarkers[_curHitMarker].Timer = ((!kill) ? 1 : 2);
		_curHitMarker++;
		if (_curHitMarker >= _poolSize)
		{
			_curHitMarker = 0;
		}
	}
}
