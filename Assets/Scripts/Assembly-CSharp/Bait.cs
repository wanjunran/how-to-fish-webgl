using System.Collections.Generic;
using UnityEngine;

public class Bait : MonoBehaviour
{
	private struct Sample
	{
		public Vector3 Position;

		public float Time;
	}

	[SerializeField]
	private FishingRod _fishingRod;

	[SerializeField]
	private MeshFilter _meshFilter;

	[SerializeField]
	private Rigidbody _rig;

	[SerializeField]
	private Transform _rodTip;

	[SerializeField]
	private float _minVelForExtraDrag;

	[SerializeField]
	private float _underWaterDrag;

	[SerializeField]
	private Transform _hookTarget;

	[SerializeField]
	private SpringJoint _joint;

	public static HashSet<Bait> BaitsUnderWater = new HashSet<Bait>();

	private bool _disableDrag;

	private float _timeAboveWater;

	private float _defaultDrag;

	private Vector3 _velocity;

	private float? _rodDrag;

	private float _flatVelMagSqr;

	private const float _minSqrVelForReeling = 5f;

	private readonly Queue<Sample> _velSamples = new Queue<Sample>();

	public Rigidbody Rig => _rig;

	public FishingRod FishingRod => _fishingRod;

	public Item ItemOnBait { get; private set; }

	public float LineLength { get; private set; }

	public Transform HookTarget => _hookTarget;

	public Item ServerItemOnBait { get; private set; }

	public Vector3 JointForce
	{
		get
		{
			if (!_joint)
			{
				return Vector3.zero;
			}
			return _joint.currentForce;
		}
	}

	public float JointForceMagnitude => JointForce.magnitude;

	public BaitInfo Info { get; private set; }

	public float TimeUnderWater { get; private set; }

	public float RandomizedCatchTime { get; private set; }

	private void Awake()
	{
		_defaultDrag = _rig.linearDamping;
	}

	private void FixedUpdate()
	{
		CalculateVelocity();
		UnderwaterCheck();
		if (!_rig.isKinematic)
		{
			SetJointLength();
			CustomDrag();
		}
	}

	private void OnDisable()
	{
		if (BaitsUnderWater.Contains(this))
		{
			BaitsUnderWater.Remove(this);
		}
	}

	public void ResetTimeUnderWater()
	{
		TimeUnderWater = 0f;
	}

	public void AddItemOnBait(Item item, bool asServer = false)
	{
		if (asServer)
		{
			ServerItemOnBait = item;
		}
		else if (!ItemOnBait || ItemOnBait.IsDeinitializing)
		{
			ServerItemOnBait = item;
			ItemOnBait = item;
			ToggleDrag(enable: true);
			if ((bool)FishingRod.Holder && FishingRod.Holder.Owner.IsLocalClient)
			{
				_rig.linearVelocity = Vector3.zero;
				_rig.angularVelocity = Vector3.zero;
			}
		}
	}

	public void OnRemoveBaitJoint(Item item)
	{
		if (!(ItemOnBait != item))
		{
			ItemOnBait = null;
			ServerItemOnBait = null;
			if (!_rig.isKinematic)
			{
				_rig.linearVelocity = Vector3.zero;
				_rig.angularVelocity = Vector3.zero;
			}
			ToggleDrag(enable: true);
		}
	}

	public void ToggleDrag(bool enable)
	{
		_disableDrag = !enable;
		_rodDrag = null;
	}

	public void SetRodDrag(float drag)
	{
		_disableDrag = false;
		_rodDrag = Mathf.Max(0f, drag);
	}

	public void SetLineLength(float length)
	{
		LineLength = length;
	}

	public void SetBaitInfo(BaitInfo baitInfo)
	{
		Info = baitInfo;
		_meshFilter.mesh = Info.Mesh;
		_hookTarget.localPosition = Info.HookPoint;
	}

	private void UnderwaterCheck()
	{
		if (base.transform.position.y < WaterManager.WaterHeight && (bool)BoatManager.Boat && !BoatManager.Boat.BoatTrigger.BaitsOnBoat.Contains(base.transform))
		{
			IncreaseUnderwaterTime();
			_timeAboveWater = 0f;
			if (BaitsUnderWater.Contains(this))
			{
				return;
			}
			BaitsUnderWater.Add(this);
			RandomizedCatchTime = Random.Range(Info.CatchTimeMinMax.x, Info.CatchTimeMinMax.y);
			if (Time.time - _fishingRod.TimeOfEquip > 0.25f)
			{
				if (_velocity.y < -10f)
				{
					AudioManager.PlayRandomClipAt("BaitHitWater_V", 1, 3, base.transform.position, variation: false, AudioDistance.Medium, 0.25f, 0.25f);
				}
				else
				{
					AudioManager.PlayRandomClipAt("ItemHitWaterLight_V", 1, 3, base.transform.position, variation: false, AudioDistance.Medium, 3f, 0.25f);
				}
			}
			VFXManager.Play("WaterSplash", base.transform.position, Vector3.zero);
			return;
		}
		TimeUnderWater = 0f;
		if ((bool)ItemOnBait)
		{
			IncreaseAboveWaterTime();
		}
		if (BaitsUnderWater.Contains(this))
		{
			BaitsUnderWater.Remove(this);
			if (Time.time - _fishingRod.TimeOfEquip > 0.25f)
			{
				AudioManager.PlayRandomClipAt(ItemOnBait ? "BaitLeaveWater_V" : "BaitLeaveWater_noitem_V", volume: ItemOnBait ? 0.25f : 0.15f, min: 1, max: ItemOnBait ? 3 : 4, position: base.transform.position, variation: false, distance: AudioDistance.Medium, minDelay: 0.25f);
			}
		}
	}

	private void IncreaseUnderwaterTime()
	{
		bool flag = _flatVelMagSqr > 5f;
		if (!Info.RequireReelingToCatch | flag)
		{
			TimeUnderWater += Time.fixedDeltaTime;
		}
	}

	private void IncreaseAboveWaterTime()
	{
		_timeAboveWater += Time.fixedDeltaTime;
		if (_timeAboveWater >= 5f)
		{
			_fishingRod.ReleaseItem(ItemOnBait);
			AudioManager.PlayClipAt("BaitInWater", base.transform.position, variation: true, AudioDistance.Short, 0.5f);
			_timeAboveWater = 0f;
		}
	}

	private void CalculateVelocity()
	{
		float time = Time.time;
		_velSamples.Enqueue(new Sample
		{
			Position = base.transform.position,
			Time = time
		});
		while (_velSamples.Count > 2 && time - _velSamples.Peek().Time > 0.1f)
		{
			_velSamples.Dequeue();
		}
		if (_velSamples.Count < 2)
		{
			return;
		}
		Sample sample = _velSamples.Peek();
		Sample sample2 = default;
		foreach (Sample velSample in _velSamples)
		{
			sample2 = velSample;
		}
		float num = sample2.Time - sample.Time;
		if (num > Mathf.Epsilon)
		{
			_velocity = (sample2.Position - sample.Position) / num;
			Vector3 velocity = _velocity;
			velocity.y = 0f;
			_flatVelMagSqr = velocity.sqrMagnitude;
		}
	}

	private void SetJointLength()
	{
		_joint.maxDistance = LineLength;
	}

	private void CustomDrag()
	{
		if (base.transform.position.y > WaterManager.WaterHeight)
		{
			if (_disableDrag)
			{
				_rig.linearDamping = 0f;
			}
			else if (_rodDrag.HasValue)
			{
				_rig.linearDamping = _rodDrag.Value;
			}
			else if (_rig.linearVelocity.magnitude > _minVelForExtraDrag)
			{
				_rig.linearDamping = 25f;
			}
			else
			{
				_rig.linearDamping = _defaultDrag;
			}
		}
		else
		{
			if (!Mathf.Approximately(_rig.linearDamping, _underWaterDrag))
			{
				_rig.linearVelocity = Vector3.zero;
			}
			_rig.linearDamping = _underWaterDrag;
		}
	}
}
