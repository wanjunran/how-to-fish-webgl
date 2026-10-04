using UnityEngine;

public class Sight : Attachment
{
	[SerializeField]
	private Vector3 _adsPos;

	[SerializeField]
	private float _adsSpeedDamping;

	[SerializeField]
	private float _adsFov;

	[SerializeField]
	[Range(0f, 1f)]
	private float _aimSwayMulti;

	[SerializeField]
	private bool _useSniperUI;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sniperUiAimPercent;

	[SerializeField]
	private AnimationCurve _sniperUiPosCurve;

	[SerializeField]
	private float _sniperUiScale;

	[SerializeField]
	private Transform _sniperUiPos;

	[SerializeField]
	private GameObject[] _disableWhenSniperUi;

	public Vector3 AdsPos => _adsPos;

	public float AdsSpeedDamping => _adsSpeedDamping;

	public float AdsFov => _adsFov;

	public float AimSwayMulti => _aimSwayMulti;

	public bool UseSniperUi => _useSniperUI;

	public float SniperUiAimPercent => _sniperUiAimPercent;

	public AnimationCurve SniperUiPosCurve => _sniperUiPosCurve;

	public float SniperUiScale => _sniperUiScale;

	public Transform SniperUiPos => _sniperUiPos;

	public GameObject[] DisableWhenSniperUi => _disableWhenSniperUi;
}
