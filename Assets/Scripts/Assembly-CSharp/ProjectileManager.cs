using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using UnityEngine;

public class ProjectileManager : NetworkBehaviour
{
	public static ProjectileManager Instance;

	private readonly Dictionary<NetworkConnection, Dictionary<uint, Projectile>> _playerProjectiles = new Dictionary<NetworkConnection, Dictionary<uint, Projectile>>();

	private uint _nextId;

	private float _sqrMaxProjRange;

	private float _invisibleDistSqr;

	[SerializeField]
	[NonReorderable]
	private ProjectileType[] _types;

	[SerializeField]
	[Range(0f, 1f)]
	private float _catchUpSpeed;

	[SerializeField]
	private float _maxProjectileRange;

	[SerializeField]
	[Tooltip("Distance from projectile spawn point until projectile becomes visible")]
	private float _invisibleDist;

	private bool NetworkInitialize___EarlyProjectileManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize___LateProjectileManagerAssembly_002DCSharp_002Edll_Excuted;

	public virtual void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ProjectileManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize___Late();
	}

	private void FixedUpdate()
	{
		for (byte b = 0; b < _types.Length; b++)
		{
			ProjectileType type = GetType(b);
			foreach (Projectile projectile in type.Projectiles)
			{
				UpdateProjectileScan(projectile, type);
			}
			RemoveQueued(type);
			foreach (Projectile projectile2 in type.Projectiles)
			{
				UpdateProjectilePos(projectile2);
			}
		}
	}

	private void Update()
	{
		ProjectileType[] types = _types;
		foreach (ProjectileType type in types)
		{
			UpdateMatrices(type);
		}
	}

	[ObserversRpc]
	public void ObserverAddProjectile(Player owner, WeaponInfo weaponInfo, uint tick, uint id, Vector3 pos, Vector3 vel)
	{
		RpcWriter___ObserverAddProjectile___3746676724(owner, weaponInfo, tick, id, pos, vel);
	}

	[ObserversRpc]
	public void ObserverAddProjectiles(Player owner, WeaponInfo weaponInfo, uint tick, uint id, Vector3 pos, Vector3[] velocities)
	{
		RpcWriter___ObserverAddProjectiles___1894401688(owner, weaponInfo, tick, id, pos, velocities);
	}

	private void ReceiveAddProjectiles(Player owner, WeaponInfo weaponInfo, uint tick, uint id, Vector3 pos, Vector3[] velocities)
	{
		if ((!owner || !owner.IsOwner) && velocities != null && velocities.Length != 0)
		{
			uint num = InstanceFinder.TimeManager.Tick - tick;
			num = (uint)((float)num * GameInfo.TickMulti);
			AddProjectiles(owner, weaponInfo, isLocal: false, pos, velocities, (uint)((float)num * GameInfo.TickMulti), id);
		}
	}

	[ObserversRpc]
	public void ObserverProjectileHitDynamic(NetworkConnection netCon, uint id)
	{
		RpcWriter___ObserverProjectileHitDynamic___353006558(netCon, id);
	}

	public void AddProjectile(Player owner, WeaponInfo weaponInfo, bool isLocal, Vector3 pos, Vector3 velocity, uint catchingUpToDo = 0u, uint id = 0u, bool fromNpc = false)
	{
		if (isLocal)
		{
			id = _nextId;
		}
		ProjectileType type = GetType(weaponInfo.ProjectileType);
		if (isLocal)
		{
			Server.Instance.AddProjectile(owner, weaponInfo, InstanceFinder.TimeManager.Tick, id, pos, velocity);
			_nextId++;
		}
		else if ((bool)weaponInfo.Weapon)
		{
			weaponInfo.Weapon.ShootEffects();
		}
		Projectile projectile = new Projectile
		{
			TypeId = weaponInfo.ProjectileType,
			Owner = owner,
			IsLocal = isLocal,
			CatchingUpToDo = catchingUpToDo,
			Id = id,
			PreviousPosition = pos,
			Position = pos,
			SpawnPos = pos,
			PreviousVelocity = velocity,
			Velocity = velocity,
			Force = weaponInfo.ProjectileForce,
			BoatForceOverride = weaponInfo.BoatForceOverride,
			Damage = (ServerSettings.OneShotEnabled ? 99999 : (weaponInfo.Weapon ? weaponInfo.Weapon.Damage : ((!fromNpc) ? weaponInfo.ProjectileDamage : (weaponInfo.ProjectileDamage + (PlayerManager.Players.Count - 1) * 2)))),
			GravityForce = weaponInfo.ProjectileGravity,
			FromNpc = fromNpc
		};
		VFXManager.Play(weaponInfo.ShootVFX, pos, Quaternion.LookRotation(velocity.normalized).eulerAngles);
		if (type.IsHitScan)
		{
			Vector3 meshScale = type.MeshScale;
			meshScale.z = HitScan(projectile, type);
			type.InstanceMatrices.Add(Matrix4x4.TRS(pos, Quaternion.LookRotation(velocity), meshScale));
		}
		else
		{
			type.Projectiles.Add(projectile);
		}
		if ((bool)owner && !owner.IsDeinitializing)
		{
			NetworkConnection owner2 = owner.Owner;
			if (!_playerProjectiles.ContainsKey(owner2))
			{
				_playerProjectiles.Add(owner2, new Dictionary<uint, Projectile>());
			}
			_playerProjectiles[owner2].Add(id, projectile);
		}
	}

	public void AddProjectiles(Player owner, WeaponInfo weaponInfo, bool isLocal, Vector3 pos, Vector3[] velocities, uint catchingUpToDo = 0u, uint id = 0u, bool canHitOwner = false)
	{
		if (velocities == null || velocities.Length == 0)
		{
			return;
		}
		if (isLocal)
		{
			id = _nextId;
		}
		ProjectileType type = GetType(weaponInfo.ProjectileType);
		if (isLocal)
		{
			Server.Instance.AddProjectiles(owner, weaponInfo, InstanceFinder.TimeManager.Tick, id, pos, velocities);
			_nextId += (uint)velocities.Length;
		}
		else if ((bool)weaponInfo.Weapon)
		{
			weaponInfo.Weapon.ShootEffects();
		}
		for (int i = 0; i < velocities.Length; i++)
		{
			Vector3 vector = velocities[i];
			uint num = id + (uint)i;
			Projectile projectile = new Projectile
			{
				TypeId = weaponInfo.ProjectileType,
				Owner = owner,
				IsLocal = isLocal,
				CatchingUpToDo = catchingUpToDo,
				Id = num,
				PreviousPosition = pos,
				Position = pos,
				SpawnPos = pos,
				PreviousVelocity = vector,
				Velocity = vector,
				Force = weaponInfo.ProjectileForce,
				BoatForceOverride = weaponInfo.BoatForceOverride,
				Damage = (ServerSettings.OneShotEnabled ? 99999 : (weaponInfo.Weapon ? weaponInfo.Weapon.Damage : weaponInfo.ProjectileDamage)),
				GravityForce = weaponInfo.ProjectileGravity,
				FromNpc = canHitOwner
			};
			VFXManager.Play(weaponInfo.ShootVFX, pos, Quaternion.LookRotation(vector.normalized).eulerAngles);
			if (type.IsHitScan)
			{
				Vector3 meshScale = type.MeshScale;
				meshScale.z = HitScan(projectile, type);
				type.InstanceMatrices.Add(Matrix4x4.TRS(pos, Quaternion.LookRotation(vector), meshScale));
			}
			else
			{
				type.Projectiles.Add(projectile);
			}
			if ((bool)owner && !owner.IsDeinitializing)
			{
				NetworkConnection owner2 = owner.Owner;
				if (!_playerProjectiles.ContainsKey(owner2))
				{
					_playerProjectiles.Add(owner2, new Dictionary<uint, Projectile>());
				}
				_playerProjectiles[owner2].Add(num, projectile);
			}
		}
	}

	public void DuplicateProjectile(Projectile projToDuplicate)
	{
		uint nextId = _nextId;
		ProjectileType type = GetType(projToDuplicate.TypeId);
		if (projToDuplicate.IsLocal && !type.IsHitScan)
		{
			projToDuplicate.Position -= projToDuplicate.Velocity * Time.fixedDeltaTime;
		}
		Projectile projectile = new Projectile
		{
			TypeId = projToDuplicate.TypeId,
			Owner = projToDuplicate.Owner,
			IsLocal = projToDuplicate.IsLocal,
			CatchingUpToDo = 0u,
			Id = nextId,
			PreviousPosition = projToDuplicate.Position,
			Position = projToDuplicate.Position,
			PreviousVelocity = projToDuplicate.Velocity,
			Velocity = projToDuplicate.Velocity,
			Force = projToDuplicate.Force,
			BoatForceOverride = projToDuplicate.BoatForceOverride,
			Damage = projToDuplicate.Damage,
			GravityForce = projToDuplicate.GravityForce,
			CanHurtSelf = projToDuplicate.CanHurtSelf
		};
		Vector3 meshScale = type.MeshScale;
		if (type.IsHitScan)
		{
			meshScale.z = HitScan(projectile, type);
			type.InstanceMatrices.Add(Matrix4x4.TRS(projToDuplicate.Position, Quaternion.LookRotation(projToDuplicate.Velocity), meshScale));
		}
		else
		{
			type.Projectiles.Add(projectile);
		}
		if ((bool)projToDuplicate.Owner && !projToDuplicate.Owner.IsDeinitializing)
		{
			NetworkConnection owner = projToDuplicate.Owner.Owner;
			if (!_playerProjectiles.ContainsKey(owner))
			{
				_playerProjectiles.Add(owner, new Dictionary<uint, Projectile>());
			}
			_playerProjectiles[owner].Add(nextId, projectile);
		}
		_nextId++;
	}

	public void Clear()
	{
		ProjectileType[] types = Instance._types;
		foreach (ProjectileType obj in types)
		{
			obj.Projectiles.Clear();
			obj.InstanceMatrices.Clear();
		}
	}

	private void UpdateMatrices(ProjectileType type)
	{
		if (type.IsHitScan)
		{
			for (int num = type.InstanceMatrices.Count - 1; num >= 0; num--)
			{
				Matrix4x4 matrix4x = type.InstanceMatrices[num];
				if (matrix4x.lossyScale.x < 0.1f)
				{
					type.InstanceMatrices.RemoveAt(num);
				}
				else
				{
					Matrix4x4 item = Matrix4x4.TRS(matrix4x.GetPosition(), matrix4x.rotation, new Vector3(matrix4x.lossyScale.x - type.HitScanLineFadeSpeed * Time.deltaTime, 1f, matrix4x.lossyScale.z));
					type.InstanceMatrices.Add(item);
					type.InstanceMatrices.RemoveAt(num);
				}
			}
			List<List<Matrix4x4>> newBatches = new List<List<Matrix4x4>> { type.InstanceMatrices.ToList() };
			InstanceManager.ReplaceBatches(type.MeshInstance, newBatches);
		}
		else
		{
			Matrix4x4[] array = new Matrix4x4[type.Projectiles.Count];
			for (int i = 0; i < type.Projectiles.Count; i++)
			{
				array[i] = MatrixFromProjectile(type.Projectiles[i], GetFixedInterpolationFactor());
			}
			List<List<Matrix4x4>> newBatches2 = new List<List<Matrix4x4>> { array.ToList() };
			InstanceManager.ReplaceBatches(type.MeshInstance, newBatches2);
		}
	}

	private void UpdateProjectileScan(Projectile projectile, ProjectileType type)
	{
		float num = 1f + (float)projectile.CatchingUpToDo * Instance._catchUpSpeed;
		if (!projectile.Owner || (projectile.Position - projectile.Owner.Transform.position).sqrMagnitude > _sqrMaxProjRange)
		{
			AddToRemoveQueue(projectile);
		}
		else if (projectile.Position.y < WaterManager.WaterHeight - 0.5f)
		{
			AddToRemoveQueue(projectile);
			HitWater(projectile);
		}
		LayerMask layerMask = (projectile.FromNpc ? GameInfo.NpcProjectileHitLayer : GameInfo.ProjectileHitLayer);
		if (Physics.SphereCast(projectile.Position, type.WidthRadius, projectile.Velocity, out var hitInfo, projectile.Velocity.magnitude * num * Time.fixedDeltaTime, layerMask))
		{
			Hit(projectile, type, hitInfo);
		}
	}

	private void UpdateProjectilePos(Projectile projectile)
	{
		float num = 1f + (float)projectile.CatchingUpToDo * Instance._catchUpSpeed;
		if (projectile.CatchingUpToDo != 0)
		{
			projectile.CatchingUpToDo -= (uint)num;
		}
		else
		{
			projectile.CatchingUpToDo = 0u;
		}
		projectile.PreviousPosition = projectile.Position;
		projectile.PreviousVelocity = projectile.Velocity;
		projectile.Position += projectile.Velocity * (Time.fixedDeltaTime * num);
		projectile.Velocity += Vector3.down * (projectile.GravityForce * Time.fixedDeltaTime * num);
	}

	private float HitScan(Projectile projectile, ProjectileType type)
	{
		if (Physics.SphereCast(projectile.Position, type.WidthRadius, projectile.Velocity, out var hitInfo, float.PositiveInfinity, GameInfo.ProjectileHitLayer) && !type.ProjectilesToRemove.Contains(projectile))
		{
			Hit(projectile, type, hitInfo);
		}
		return 0f;
	}

	private void LandProjectile(Projectile projectile, ProjectileType type, Vector3 pos, Vector3 dir)
	{
		if (projectile.TypeId == 2)
		{
			BowheadWhale.AddLavaPool(pos, dir);
			return;
		}
		if (!string.IsNullOrEmpty(type.LandedMeshInstance))
		{
			Quaternion quaternion = Quaternion.LookRotation(dir);
			quaternion = Quaternion.AngleAxis(Random.Range(0f, 360f), dir) * quaternion;
			InstanceManager.AddInstance(type.LandedMeshInstance, pos, quaternion, type.LandedMeshScale);
			if (type.RandomSounds == 0)
			{
				AudioManager.PlayClipAt(type.HitSound, pos, variation: true, AudioDistance.VeryShort, type.HitSoundVolume);
			}
			else
			{
				AudioManager.PlayRandomClipAt(type.HitSound, 1, type.RandomSounds, pos, variation: true, AudioDistance.VeryShort, type.HitSoundVolume);
			}
			ParticleManager.Play(type.HitParticle, pos, dir);
		}
		if (!string.IsNullOrEmpty(type.LandedDecal))
		{
			DecalManager.SpawnDecal(type.LandedDecal, pos, dir, type.LandedDecalSize);
			if (type.RandomSounds == 0)
			{
				AudioManager.PlayClipAt(type.HitSound, pos, variation: true, AudioDistance.VeryShort, type.HitSoundVolume);
			}
			else
			{
				AudioManager.PlayRandomClipAt(type.HitSound, 1, type.RandomSounds, pos, variation: true, AudioDistance.VeryShort, type.HitSoundVolume);
			}
			ParticleManager.Play(type.HitParticle, pos, dir);
		}
	}

	private void Hit(Projectile projectile, ProjectileType type, RaycastHit hit)
	{
		if (projectile.IsLocal && !hit.transform.CompareTag("Level"))
		{
			Server.Instance.ProjectileHitDynamic(projectile.Owner.Owner, projectile.Id);
		}
		AddToRemoveQueue(projectile);
		Item item = ItemManager.Get(hit.collider);
		if ((bool)item)
		{
			item.LocalHit(hit.transform, hit.point, projectile.Velocity.normalized, projectile.Owner, projectile.Damage, rangedHit: true, projectile.Velocity.normalized * projectile.Force);
		}
		else if (hit.transform.CompareTag("NPC"))
		{
			DazedUtils.PlayDeadPlayerHitEffects(hit.point, projectile.Velocity, projectile.Damage, projectile.Owner, noDecals: true);
		}
		else if (hit.transform.CompareTag("Level") || hit.transform.CompareTag("Boat"))
		{
			if (hit.point.y < WaterManager.WaterHeight && !hit.transform.CompareTag("Boat"))
			{
				HitWater(projectile);
			}
			else if (projectile.TypeId != 2 || !hit.transform.CompareTag("Boat"))
			{
				LandProjectile(projectile, type, hit.point, type.LandedMeshFaceNormal ? (-hit.normal) : projectile.Velocity);
			}
			if (base.IsServerInitialized && hit.transform.CompareTag("Boat"))
			{
				float num = ((projectile.BoatForceOverride == 0f) ? GameInfo.BoatProjectileForce : projectile.BoatForceOverride);
				BoatManager.Boat.HiddenPhysicsRig.AddForceAtPosition(projectile.Velocity.normalized * num, hit.point);
			}
		}
		if (projectile.IsLocal)
		{
			Player playerFromBodyPart = PlayerManager.GetPlayerFromBodyPart(hit.transform);
			if ((bool)playerFromBodyPart)
			{
				playerFromBodyPart.Vitals.LocalHit(hit.point, projectile.Velocity.normalized, projectile.Owner, projectile.Damage, rangedHit: true, projectile.Velocity.normalized * GameInfo.PlayerKillForce, projectile.FromNpc);
			}
			if ((bool)item && (bool)item.Explosive)
			{
				item.Explosive.ForceExplode(projectile.Owner, instant: true);
			}
		}
	}

	private void HitWater(Projectile projectile)
	{
		if ((bool)projectile.Owner && (bool)projectile.Owner.CamObject)
		{
			Vector3 position = projectile.Owner.CamObject.position;
			if ((bool)projectile.Owner.Holding.HeldItem)
			{
				position = projectile.Owner.Holding.HeldItem.transform.position;
			}
			Vector3 normalized = projectile.Velocity.normalized;
			float num = (WaterManager.WaterHeight - position.y) / normalized.y;
			Vector3 vector = position + normalized * num;
			Vector3 position2 = vector;
			position2.y = WaterManager.GetWaterHeight(vector);
			VFXManager.Play("WaterSplash", position2, Vector3.zero);
			AudioManager.PlayRandomClipAt("ItemHitWaterMedium_V", 1, 3, vector, variation: true, AudioDistance.Short, 2f);
		}
	}

	private void RemoveQueued(ProjectileType type)
	{
		foreach (Projectile item in type.ProjectilesToRemove)
		{
			type.Projectiles.Remove(item);
		}
		type.ProjectilesToRemove.Clear();
	}

	private void AddToRemoveQueue(Projectile projectile)
	{
		if ((bool)projectile.Owner && _playerProjectiles.ContainsKey(projectile.Owner.Owner) && _playerProjectiles[projectile.Owner.Owner].ContainsKey(projectile.Id))
		{
			_playerProjectiles[projectile.Owner.Owner].Remove(projectile.Id);
		}
		GetType(projectile.TypeId).ProjectilesToRemove.Add(projectile);
	}

	private float GetFixedInterpolationFactor()
	{
		if (Time.fixedDeltaTime <= 0f)
		{
			return 1f;
		}
		return Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
	}

	private Matrix4x4 MatrixFromProjectile(Projectile projectile, float interpolationFactor)
	{
		Vector3 vector = Vector3.Lerp(projectile.PreviousPosition, projectile.Position, interpolationFactor);
		Vector3 forward = Vector3.Lerp(projectile.PreviousVelocity, projectile.Velocity, interpolationFactor);
		if ((projectile.SpawnPos - vector).sqrMagnitude < _invisibleDistSqr)
		{
			vector = Vector3.down * 100f;
		}
		return Matrix4x4.TRS(vector, Quaternion.LookRotation(forward), GetType(projectile.TypeId).MeshScale);
	}

	public ProjectileType GetType(byte typeId)
	{
		return Instance._types[typeId];
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyProjectileManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyProjectileManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			RegisterObserversRpc(0u, RpcReader___ObserverAddProjectile___3746676724);
			RegisterObserversRpc(1u, RpcReader___ObserverAddProjectiles___1894401688);
			RegisterObserversRpc(2u, RpcReader___ObserverProjectileHitDynamic___353006558);
		}
	}

	public override void NetworkInitialize___Late()
	{
		if (!NetworkInitialize___LateProjectileManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___LateProjectileManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize___Late();
	}

	private void RpcWriter___ObserverAddProjectile___3746676724(Player owner, WeaponInfo weaponInfo, uint tick, uint id, Vector3 pos, Vector3 vel)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GeneratedWriters___Internal.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, owner);
		GeneratedWriters___Internal.GWrite___WeaponInfoFishNet_002ESerializing_002EGenerated(pooledWriter, weaponInfo);
		pooledWriter.WriteUInt32(tick);
		pooledWriter.WriteUInt32(id);
		pooledWriter.WriteVector3(pos);
		pooledWriter.WriteVector3(vel);
		SendObserversRpc(0u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverAddProjectile___3746676724(Player P_0, WeaponInfo P_1, uint P_2, uint P_3, Vector3 P_4, Vector3 P_5)
	{
		ReceiveAddProjectiles(P_0, P_1, P_2, P_3, P_4, new Vector3[1] { P_5 });
	}

	private void RpcReader___ObserverAddProjectile___3746676724(PooledReader PooledReader0, Channel channel)
	{
		Player player = GeneratedReaders___Internal.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		WeaponInfo weaponInfo = GeneratedReaders___Internal.GRead___WeaponInfoFishNet_002ESerializing_002EGenerateds(PooledReader0);
		uint num = PooledReader0.ReadUInt32();
		uint num2 = PooledReader0.ReadUInt32();
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3 vector2 = PooledReader0.ReadVector3();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverAddProjectile___3746676724(player, weaponInfo, num, num2, vector, vector2);
		}
	}

	private void RpcWriter___ObserverAddProjectiles___1894401688(Player owner, WeaponInfo weaponInfo, uint tick, uint id, Vector3 pos, Vector3[] velocities)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		GeneratedWriters___Internal.GWrite___PlayerFishNet_002ESerializing_002EGenerated(pooledWriter, owner);
		GeneratedWriters___Internal.GWrite___WeaponInfoFishNet_002ESerializing_002EGenerated(pooledWriter, weaponInfo);
		pooledWriter.WriteUInt32(tick);
		pooledWriter.WriteUInt32(id);
		pooledWriter.WriteVector3(pos);
		GeneratedWriters___Internal.GWrite___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerated(pooledWriter, velocities);
		SendObserversRpc(1u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverAddProjectiles___1894401688(Player P_0, WeaponInfo P_1, uint P_2, uint P_3, Vector3 P_4, Vector3[] P_5)
	{
		ReceiveAddProjectiles(P_0, P_1, P_2, P_3, P_4, P_5);
	}

	private void RpcReader___ObserverAddProjectiles___1894401688(PooledReader PooledReader0, Channel channel)
	{
		Player player = GeneratedReaders___Internal.GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader0);
		WeaponInfo weaponInfo = GeneratedReaders___Internal.GRead___WeaponInfoFishNet_002ESerializing_002EGenerateds(PooledReader0);
		uint num = PooledReader0.ReadUInt32();
		uint num2 = PooledReader0.ReadUInt32();
		Vector3 vector = PooledReader0.ReadVector3();
		Vector3[] array = GeneratedReaders___Internal.GRead___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader0);
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverAddProjectiles___1894401688(player, weaponInfo, num, num2, vector, array);
		}
	}

	private void RpcWriter___ObserverProjectileHitDynamic___353006558(NetworkConnection netCon, uint id)
	{
		if (!base.IsServerInitialized)
		{
			NetworkManager networkManager = base.NetworkManager;
			networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			return;
		}
		Channel channel = Channel.Reliable;
		PooledWriter pooledWriter = WriterPool.Retrieve();
		pooledWriter.WriteNetworkConnection(netCon);
		pooledWriter.WriteUInt32(id);
		SendObserversRpc(2u, pooledWriter, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
		pooledWriter.Store();
	}

	public void RpcLogic___ObserverProjectileHitDynamic___353006558(NetworkConnection P_0, uint P_1)
	{
		if (!P_0.IsLocalClient && _playerProjectiles.ContainsKey(P_0) && _playerProjectiles[P_0].ContainsKey(P_1))
		{
			Projectile projectile = _playerProjectiles[P_0][P_1];
			AddToRemoveQueue(projectile);
		}
	}

	private void RpcReader___ObserverProjectileHitDynamic___353006558(PooledReader PooledReader0, Channel channel)
	{
		NetworkConnection networkConnection = PooledReader0.ReadNetworkConnection();
		uint num = PooledReader0.ReadUInt32();
		if (base.IsClientInitialized)
		{
			RpcLogic___ObserverProjectileHitDynamic___353006558(networkConnection, num);
		}
	}

	private void Awake_UserLogic_ProjectileManager_Assembly_002DCSharp_002Edll()
	{
		Setter.SetSingleInstance(ref Instance, this);
		_sqrMaxProjRange = Mathf.Pow(_maxProjectileRange, 2f);
		_invisibleDistSqr = _invisibleDist * _invisibleDist;
	}
}
