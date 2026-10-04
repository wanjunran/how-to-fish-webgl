using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEating : NetworkBehaviour
{
	[SerializeField]
	private Player _player;

	[SerializeField]
	private float _eatSpeed;

	[SerializeField]
	[Tooltip("Percent that EatPercent should go to instantly when you stop eating")]
	private float _stopEatPercent;

	[SerializeField]
	[Tooltip("EatPercent at which eat sound should start playing (when food reaches mouth)")]
	private float _eatSoundPercent;

	[SerializeField]
	private float _holdForceMulti;

	[SerializeField]
	private Vector3 _mouthPos;

	[SerializeField]
	private Vector3 _observerMouthPos;

	[SerializeField]
	private Vector3 _observerEatPosOffset;

	[SerializeField]
	private AnimationCurve _moveCurve;

	[SerializeField]
	private AudioSource _eatSoundSource;

	[SerializeField]
	private AudioClip _eatSound;

	[SerializeField]
	[Range(0f, 1f)]
	private float _eatSoundVolume = 0.1f;

	[SerializeField]
	private AudioClip _drinkSound;

	[SerializeField]
	[Range(0f, 1f)]
	private float _drinkSoundVolume = 0.1f;

	public readonly SyncVar<bool> _isEating = new SyncVar<bool>(new SyncTypeSettings(0f, Channel.Reliable));

	private Creature _curFood;

	private bool _localIsEating;

	private bool NetworkInitialize___EarlyPlayerEatingAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LatePlayerEatingAssembly_002DCSharp_002Edll_Excuted;

	public float EatPercent { get; private set; }

	public AnimationCurve MoveCurve => _moveCurve;

	public Vector3 MouthPos => _mouthPos;

	public Vector3 ObserverMouthPos => _observerMouthPos;

	public float HoldForceMulti => _holdForceMulti;

	public static event Action OnCreatureEaten;

	public override void OnStartClient()
	{
		if (base.Owner.IsLocalClient)
		{
			BindInputs();
		}
		else
		{
			_isEating.OnChange += OnEatingChange;
		}
	}

	public override void OnStopClient()
	{
		if (base.Owner.IsLocalClient)
		{
			UnbindInputs();
		}
	}

	public void ServerSetEating(bool isEating)
	{
		_isEating.Value = isEating;
	}

	private void BindInputs()
	{
		PlayerInput input = GameInfo.Input;
		input.actions["PlayerLeftClick"].performed += StartEatInput;
		input.actions["PlayerLeftClick"].canceled += StopEatInput;
	}

	private void UnbindInputs()
	{
		PlayerInput input = GameInfo.Input;
		if ((bool)input)
		{
			input.actions["PlayerLeftClick"].performed -= StartEatInput;
			input.actions["PlayerLeftClick"].canceled -= StopEatInput;
		}
	}

	private void StartEatInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs && (bool)_player.Holding.HeldItem && (bool)_player.Holding.HeldItem.Creature)
		{
			StartEating();
		}
	}

	private void StopEatInput(InputAction.CallbackContext context)
	{
		if (!_player.BlockInputs)
		{
			StopEating();
		}
	}

	private void StartEating()
	{
		if (!_localIsEating)
		{
			_localIsEating = true;
			_curFood = _player.Holding.HeldItem.Creature;
			Server.Instance.ToggleEatCreature(_player, isEating: true);
		}
	}

	private void StopEating()
	{
		if (_localIsEating)
		{
			_localIsEating = false;
			_curFood = null;
			if (EatPercent > _stopEatPercent)
			{
				EatPercent = _stopEatPercent;
			}
			_eatSoundSource.Stop();
			Server.Instance.ToggleEatCreature(_player, isEating: false);
		}
	}

	private void OnEatingChange(bool prev, bool next, bool asServer)
	{
		if (asServer)
		{
			return;
		}
		_localIsEating = next;
		if (next && (bool)_player.Holding.HeldItem && (bool)_player.Holding.HeldItem.Creature)
		{
			_curFood = _player.Holding.HeldItem.Creature;
		}
		if (!next)
		{
			if (EatPercent > _stopEatPercent)
			{
				EatPercent = _stopEatPercent;
			}
			_eatSoundSource.Stop();
		}
	}

	private void Update()
	{
		CheckCurFood();
		UpdateEating();
	}

	private void CheckCurFood()
	{
		if (!_curFood || _player.Holding.HeldItem != _curFood)
		{
			if (base.Owner.IsLocalClient)
			{
				StopEating();
			}
			else
			{
				_curFood = null;
			}
		}
	}

	private void UpdateEating()
	{
		if (_localIsEating)
		{
			if (EatPercent >= 1f)
			{
				return;
			}
			EatPercent += Time.deltaTime * _eatSpeed;
			if (EatPercent >= _eatSoundPercent && !_eatSoundSource.isPlaying)
			{
				bool flag = (bool)_curFood && _curFood.UseDrinkSound;
				_eatSoundSource.clip = (flag ? _drinkSound : _eatSound);
				_eatSoundSource.volume = (flag ? _drinkSoundVolume : _eatSoundVolume);
				_eatSoundSource.Play();
				Vector3 position = (base.Owner.IsLocalClient ? _player.CamObject.TransformPoint(_mouthPos) : _player.Body.Head.TransformPoint(_observerMouthPos));
				ParticleManager.Play("Spit", position, _player.CamObject.forward);
			}
			if (base.Owner.IsLocalClient && EatPercent >= 1f)
			{
				FinishEating();
			}
		}
		else
		{
			EatPercent = Mathf.MoveTowards(EatPercent, 0f, Time.deltaTime * _eatSpeed);
		}
		if (!base.Owner.IsLocalClient && (bool)_curFood)
		{
			Vector3 pos = _player.Body.Head.TransformPoint(_curFood.Creature.EatPos + _observerEatPosOffset);
			_curFood.RigidbodySync.ObserverSetOverride(_moveCurve.Evaluate(EatPercent), pos);
		}
	}

	private void FinishEating()
	{
		if ((bool)_curFood)
		{
			OnCreatureEaten?.Invoke();
			Server.Instance.FinishEatingCreature(_curFood, _player);
		}
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyPlayerEatingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyPlayerEatingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			_isEating.InitializeEarly(this, 0u, isSyncObject: false);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LatePlayerEatingAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LatePlayerEatingAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
			_isEating.InitializeLate();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}
}
