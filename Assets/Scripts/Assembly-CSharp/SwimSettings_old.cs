using UnityEngine;

[CreateAssetMenu(menuName = "Fish Settings", fileName = "New Fish Settings")]
public class SwimSettings_old : ScriptableObject
{
	[Header("Defaults")]
	[SerializeField]
	private float _startSpeed = 3f;

	[SerializeField]
	private float _defaultSpeed = 1.5f;

	[SerializeField]
	private float _acceleration = 2f;

	[SerializeField]
	private float _retardation = 0.75f;

	[SerializeField]
	private float _defaultHorSpeed = 1f;

	[SerializeField]
	private float _defaultVertSpeed = 3f;

	[Header("Bait")]
	[SerializeField]
	[Space]
	private float _baitSpeed = 3f;

	[SerializeField]
	private float _baitRotSpeed = 2f;

	[SerializeField]
	private float _seeBaitDist = 25f;

	[SerializeField]
	private float _hookDist = 0.25f;

	[Header("Move Vector & Weights")]
	[SerializeField]
	[Space]
	private float _swimYLevel = -2f;

	[SerializeField]
	private float _verticalWeight = 1f;

	[SerializeField]
	[Space]
	private float _separationDistance = 6f;

	[SerializeField]
	private float _separationWeight = 1.25f;

	[SerializeField]
	[Space]
	private float _alignmentDistance = 15f;

	[SerializeField]
	private float _alignmentWeight = 0.8f;

	[SerializeField]
	[Space]
	private float _cohesionWeight = 0.6f;

	[SerializeField]
	[Space]
	private float _forwardWeight = 0.5f;

	[SerializeField]
	[Space]
	private float _homeRadius = 12f;

	[SerializeField]
	private float _homeWeight = 1f;

	[SerializeField]
	[Space]
	private float _levelAvoidDistance = 4f;

	[SerializeField]
	private float _levelAvoidWeight = 5f;

	[SerializeField]
	[Range(0f, 5f)]
	private int _directionalLevelRays = 1;

	[SerializeField]
	private float _rayAngleOffset = 35f;

	public float StartSpeed => _startSpeed;

	public float DefaultSpeed => _defaultSpeed;

	public float Acceleration => _acceleration;

	public float Retardation => _retardation;

	public float DefaultHorSpeed => _defaultHorSpeed;

	public float DefaultVertSpeed => _defaultVertSpeed;

	public float BaitSpeed => _baitSpeed;

	public float BaitRotSpeed => _baitRotSpeed;

	public float SeeBaitDist => _seeBaitDist;

	public float HookDist => _hookDist;

	public float SwimYLevel => _swimYLevel;

	public float VerticalWeight => _verticalWeight;

	public float HomeRadius => _homeRadius;

	public float HomeWeight => _homeWeight;

	public float LevelAvoidDistance => _levelAvoidDistance;

	public float LevelAvoidWeight => _levelAvoidWeight;

	public int DirectionalLevelRays => _directionalLevelRays;

	public float RayAngleOffset => _rayAngleOffset;

	public float SeparationDistance => _separationDistance;

	public float SeparationWeight => _separationWeight;

	public float AlignmentDistance => _alignmentDistance;

	public float AlignmentWeight => _alignmentWeight;

	public float CohesionWeight => _cohesionWeight;

	public float ForwardWeight => _forwardWeight;
}
