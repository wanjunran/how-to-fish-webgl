using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Transporting;
using UnityEngine;

public class Bird : Creature
{
	[SerializeField]
	private Transform _foodTarget;

	[SerializeField]
	private Animator _anim;

	[SerializeField]
	private GameObject _disableOnDeath;

	[SerializeField]
	private GameObject _enableOnDeath;

	private bool NetworkInitialize___EarlyBirdAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateBirdAssembly_002DCSharp_002Edll_Excuted;

	public Transform FoodTarget => _foodTarget;

	public Vector3 ServerPos { get; private set; }

	public bool IsIdlingRight { get; private set; }

	public Item AttackingItem { get; private set; }

	public Item CaughtItem { get; private set; }

	public float Speed { get; private set; }

	public byte AnimState { get; private set; }

	public float FlyHeigth { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_Bird_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		BirdManager.Instance.AddFlyingBird(this);
		if (!base.IsDead)
		{
			_rigSync.Freeze(frozen: true);
		}
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		BirdManager.Instance.RemoveFlyingBird(this);
	}

	[ObserversRpc(ExcludeServer = true)]
	public void ObserverSetPos(Vector3 pos, byte animState, Channel channel = Channel.Unreliable)
	{
		RpcWriter___ObserverSetPos___74395448(pos, animState, channel);
	}

	public void InvertIdlingRight()
	{
		IsIdlingRight = !IsIdlingRight;
	}

	public void SetSpeed(float to)
	{
		Speed = to;
	}

	public void SetAttackingFood(Item item)
	{
		AttackingItem = item;
	}

	public void SetCaughtFood()
	{
		if ((bool)AttackingItem)
		{
			CaughtItem = AttackingItem;
			CaughtItem.RigidbodySync.StartSimulateLocal();
			CaughtItem.CaughtByBird(this);
			AttackingItem = null;
		}
	}

	public void RemoveFood()
	{
		AttackingItem = null;
		CaughtItem = null;
	}

	public void SetAnimState(byte to)
	{
		if (!base.IsDead && AnimState != to && (bool)_anim)
		{
			string trigger = "";
			switch (to)
			{
			case 1:
				trigger = "Searching";
				break;
			case 2:
				trigger = "Diving";
				break;
			case 3:
				trigger = "Flapping";
				break;
			case 4:
				trigger = "FlappingUp";
				break;
			}
			_anim.SetTrigger(trigger);
			AnimState = to;
		}
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		if ((bool)_disableOnDeath)
		{
			_disableOnDeath.SetActive(value: false);
		}
		if ((bool)_enableOnDeath)
		{
			_enableOnDeath.SetActive(value: true);
		}
		if ((bool)CaughtItem)
		{
			CaughtItem.ReleasedByBird();
		}
		CaughtItem = null;
		BirdManager.Instance.RemoveFlyingBird(this);
		_rigSync.Freeze(frozen: false);
		AudioManager.PlayRandomClipAt("Seagull_V", 1, 9, base.transform.position, variation: false, AudioDistance.Medium, 0.4f);
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyBirdAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyBirdAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(4u, RpcReader___ObserverSetPos___74395448);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateBirdAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateBirdAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverSetPos___74395448(Vector3 pos, byte animState, Channel channel = Channel.Unreliable)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel2 = channel;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteUInt8Unpacked(animState);
		SendObserversRpc(4u, pooledWriter, channel2, DataOrderType.Default, bufferLast: false, excludeServer: true, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverSetPos___74395448(Vector3 P_0, byte P_1, Channel P_2)
	{
		ServerPos = P_0;
		SetAnimState(P_1);
	}

	private void RpcReader___ObserverSetPos___74395448(PooledReader PooledReader0, Channel channel)
	{
		Vector3 vector = PooledReader0.ReadVector3();
		byte b = PooledReader0.ReadUInt8Unpacked();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverSetPos___74395448(vector, b, channel);
		}
	}

	protected virtual void Awake_UserLogic_Bird_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		FlyHeigth = base.transform.position.y;
		_canPickUp = false;
		_bird = this;
	}
}
