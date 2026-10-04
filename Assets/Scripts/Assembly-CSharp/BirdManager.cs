using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class BirdManager : NetworkBehaviour
{
	public static BirdManager Instance;

	[Header("Base Movement")]
	[SerializeField]
	private float _defaultFlySpeed = 12f;

	[SerializeField]
	private float _horizontalSpeed = 8f;

	[SerializeField]
	private float _verticalSpeed = 16f;

	[SerializeField]
	private float _retardation = 15f;

	[SerializeField]
	private float _acceleration = 15f;

	[SerializeField]
	private float _minUpDirForFlap = 0.2f;

	[Header("Attacking Movement")]
	[SerializeField]
	private float _attackingFoodSpeed = 40f;

	[SerializeField]
	private float _attackingHorizontalSpeed = 10f;

	[SerializeField]
	private float _closeToFoodSpeed = 15f;

	[SerializeField]
	private float _closeToFoodHorizontalSpeed = 25f;

	[SerializeField]
	private float _catchFoodDist = 0.4f;

	[SerializeField]
	private float _closeToFoodDist = 1.5f;

	[Header("Settings")]
	[SerializeField]
	private float _homeRadius = 12f;

	[SerializeField]
	private float _levelAvoidDistance = 3f;

	[SerializeField]
	private int _directionalLevelRays = 2;

	[SerializeField]
	private float _rayAngleOffset = 30f;

	[SerializeField]
	private int _ticksBetweenDiveChecks = 1000;

	[Header("Weights")]
	[SerializeField]
	private float _levelAvoidanceWeight = 1f;

	[SerializeField]
	private float _homeWeight = 1f;

	[SerializeField]
	private float _forwardWeight = 1f;

	[SerializeField]
	private float _idleWeight = 1f;

	[SerializeField]
	private float _verticalWeight = 0.5f;

	[Header("Audio")]
	[SerializeField]
	private Vector2 _timeBetweenAudioMinMax;

	private readonly List<Bird> _flyingBirds = new List<Bird>();

	private readonly List<Vector2> _audioTimes = new List<Vector2>();

	private int _curFlyingBird;

	private int _curDiveCheckBird;

	private int _curDiveTick;

	private bool NetworkInitialize___EarlyBirdManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateBirdManagerAssembly_002DCSharp_002Edll_Excuted;

	public float HomeRadiusSqr { get; private set; }

	public float HomeRadius => _homeRadius;

	public float LevelAvoidDistance => _levelAvoidDistance;

	public int DirectionalLevelRays => _directionalLevelRays;

	public float RayAngleOffset => _rayAngleOffset;

	public float LevelAvoidanceWeight => _levelAvoidanceWeight;

	public float HomeWeight => _homeWeight;

	public float ForwardWeight => _forwardWeight;

	public float IdleWeight => _idleWeight;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_BirdManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartServer()
	{
		InstanceFinder.TimeManager.OnTick += ServerTickUpdate;
	}

	public override void OnStopServer()
	{
		InstanceFinder.TimeManager.OnTick -= ServerTickUpdate;
	}

	private void Update()
	{
		if (base.IsServerInitialized)
		{
			SimulateAllBirds();
		}
		else
		{
			UpdateAllBirds();
		}
		PlayAudio();
	}

	private void ServerTickUpdate()
	{
		if (_flyingBirds.Count != 0)
		{
			SendBirdPos();
			LookForFood();
		}
	}

	private void SendBirdPos()
	{
		foreach (Bird flyingBird in _flyingBirds)
		{
			flyingBird.ObserverSetPos(flyingBird.transform.position, flyingBird.AnimState);
		}
		_curFlyingBird++;
		if (_curFlyingBird >= _flyingBirds.Count)
		{
			_curFlyingBird = 0;
		}
		Bird bird = _flyingBirds[_curFlyingBird];
		if (Random.Range(0f, 1f) <= 0.0025f)
		{
			bird.InvertIdlingRight();
		}
	}

	private void LookForFood()
	{
		_curDiveTick++;
		if (_curDiveTick < _ticksBetweenDiveChecks)
		{
			return;
		}
		_curDiveTick = 0;
		_curDiveCheckBird++;
		if (_curDiveCheckBird >= _flyingBirds.Count)
		{
			_curDiveCheckBird = 0;
		}
		Bird bird = _flyingBirds[_curDiveCheckBird];
		if (!bird.CaughtItem)
		{
			Item item = ItemManager.AvailableItemForBird(bird);
			if ((bool)item)
			{
				bird.SetAttackingFood(item);
			}
		}
	}

	private void SimulateAllBirds()
	{
		foreach (Bird flyingBird in _flyingBirds)
		{
			SimulateBird(flyingBird);
		}
	}

	private void SimulateBird(Bird bird)
	{
		float num = 0f;
		if ((bool)bird.AttackingItem)
		{
			Vector3 vector = bird.AttackingItem.transform.position - bird.transform.position;
			num = vector.sqrMagnitude;
			if (vector.sqrMagnitude < _catchFoodDist * _catchFoodDist)
			{
				bird.SetCaughtFood();
			}
		}
		Vector3 flyDirection = CreatureUtils.GetFlyDirection(bird);
		float num2 = ((!bird.AttackingItem) ? _horizontalSpeed : ((num < _closeToFoodDist * _closeToFoodDist) ? _closeToFoodHorizontalSpeed : _attackingHorizontalSpeed));
		Vector3 vector2 = Vector3.Lerp(bird.transform.forward, flyDirection, Time.deltaTime * num2);
		float num3 = bird.FlyHeigth - bird.transform.position.y;
		num3 *= _verticalWeight;
		float magnitude = flyDirection.magnitude;
		num3 = Mathf.Clamp(num3, 0f - magnitude, magnitude);
		float y = Mathf.Lerp(bird.transform.forward.y, num3, Time.deltaTime * _verticalSpeed);
		if (!bird.AttackingItem)
		{
			vector2.y = y;
		}
		vector2.Normalize();
		Vector3 vector3 = vector2;
		bird.transform.forward = vector3;
		float speed = bird.Speed;
		float num4 = ((!bird.AttackingItem) ? _defaultFlySpeed : ((num < _closeToFoodDist * _closeToFoodDist) ? _closeToFoodSpeed : _attackingFoodSpeed));
		bool flag = num4 <= speed;
		speed = Mathf.MoveTowards(speed, num4, flag ? (Time.deltaTime * _retardation) : (Time.deltaTime * _acceleration));
		bird.SetSpeed(speed);
		bird.transform.position += vector3 * (speed * Time.deltaTime);
		if ((bool)bird.AttackingItem)
		{
			bird.SetAnimState((byte)(((num < _closeToFoodDist * _closeToFoodDist) & flag) ? 3 : 2));
		}
		else
		{
			bird.SetAnimState((byte)((!(vector3.y > _minUpDirForFlap)) ? 1 : 4));
		}
	}

	private void UpdateAllBirds()
	{
		float t = (float)(int)InstanceFinder.TimeManager.TickRate * Time.deltaTime;
		foreach (Bird flyingBird in _flyingBirds)
		{
			flyingBird.transform.position = Vector3.Lerp(flyingBird.transform.position, flyingBird.ServerPos, t);
			Vector3 vector = flyingBird.ServerPos - flyingBird.transform.position;
			if (vector != Vector3.zero)
			{
				Quaternion b = Quaternion.LookRotation(vector);
				flyingBird.transform.rotation = Quaternion.Slerp(flyingBird.transform.rotation, b, t);
			}
		}
	}

	private void PlayAudio()
	{
		if (_flyingBirds.Count == 0)
		{
			return;
		}
		for (int num = _audioTimes.Count - 1; num >= 0; num--)
		{
			if (_flyingBirds.Count < num)
			{
				_audioTimes.RemoveAt(num);
			}
			else
			{
				_audioTimes[num] = new Vector2(_audioTimes[num].x + Time.deltaTime, _audioTimes[num].y);
				if (_audioTimes[num].x > _audioTimes[num].y)
				{
					Transform transform = _flyingBirds[num].transform;
					float num2 = ((transform.position.y < _flyingBirds[num].FlyHeigth - 4f) ? 0.25f : 1f);
					float num3 = ((transform.position.y < _flyingBirds[num].FlyHeigth - 4f) ? 0.15f : 1f);
					_audioTimes[num] = new Vector2(0f, Random.Range(_timeBetweenAudioMinMax.x * num2, _timeBetweenAudioMinMax.y * num3));
					AudioManager.PlayRandomClipAt("Seagull_V", 1, 11, transform.position, variation: false, AudioDistance.Short, 0.4f);
				}
			}
		}
	}

	public void AddFlyingBird(Bird bird)
	{
		if (!_flyingBirds.Contains(bird))
		{
			_flyingBirds.Add(bird);
			_audioTimes.Add(new Vector2(0f, Random.Range(_timeBetweenAudioMinMax.x, _timeBetweenAudioMinMax.y)));
		}
	}

	public void RemoveFlyingBird(Bird bird)
	{
		if (_flyingBirds.Contains(bird))
		{
			_flyingBirds.Remove(bird);
			_audioTimes.RemoveAt(0);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyBirdManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyBirdManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateBirdManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateBirdManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void Awake_UserLogic_BirdManager_Assembly_002DCSharp_002Edll()
	{
		Instance = this;
		HomeRadiusSqr = _homeRadius * _homeRadius;
	}
}
