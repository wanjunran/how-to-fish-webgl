using System.Runtime.InteropServices;
using MetaVoiceChat.NetProviders.FishNet;
using Unity.Mathematics;
using UnityEngine;

namespace FishNet.Serializing.Generated
{
	[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
	public static class GeneratedReaders___Internal
	{
		[RuntimeInitializeOnLoadMethod]
		private static void InitializeOnce()
		{
			GenericReader<FishNetFrame>.SetRead(FishNetFrameReaderWriter.ReadFishNetFrame);
			GenericReader<FishingRod>.SetRead(GRead___FishingRodFishNet_002ESerializing_002EGenerateds);
			GenericReader<Player>.SetRead(GRead___PlayerFishNet_002ESerializing_002EGenerateds);
			GenericReader<Bird>.SetRead(GRead___BirdFishNet_002ESerializing_002EGenerateds);
			GenericReader<Item>.SetRead(GRead___ItemFishNet_002ESerializing_002EGenerateds);
			GenericReader<WeaponInfo>.SetRead(GRead___WeaponInfoFishNet_002ESerializing_002EGenerateds);
			GenericReader<Weapon>.SetRead(GRead___WeaponFishNet_002ESerializing_002EGenerateds);
			GenericReader<Vector3[]>.SetRead(GRead___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerateds);
			GenericReader<float[]>.SetRead(GRead___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds);
			GenericReader<Quaternion[]>.SetRead(GRead___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerateds);
			GenericReader<DeadPlayer>.SetRead(GRead___DeadPlayerFishNet_002ESerializing_002EGenerateds);
			GenericReader<Tool>.SetRead(GRead___ToolFishNet_002ESerializing_002EGenerateds);
			GenericReader<Creature>.SetRead(GRead___CreatureFishNet_002ESerializing_002EGenerateds);
			GenericReader<Melee>.SetRead(GRead___MeleeFishNet_002ESerializing_002EGenerateds);
			GenericReader<Explosive>.SetRead(GRead___ExplosiveFishNet_002ESerializing_002EGenerateds);
			GenericReader<Radio>.SetRead(GRead___RadioFishNet_002ESerializing_002EGenerateds);
			GenericReader<half>.SetRead(GRead___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerateds);
			GenericReader<DamageType>.SetRead(GRead___DamageTypeFishNet_002ESerializing_002EGenerateds);
		}

		public static FishingRod GRead___FishingRodFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (FishingRod)reader.ReadNetworkBehaviour();
		}

		public static Player GRead___PlayerFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Player)reader.ReadNetworkBehaviour();
		}

		public static Bird GRead___BirdFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Bird)reader.ReadNetworkBehaviour();
		}

		public static Item GRead___ItemFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Item)reader.ReadNetworkBehaviour();
		}

		public static WeaponInfo GRead___WeaponInfoFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
			{
				return null;
			}
			WeaponInfo weaponInfo = new WeaponInfo();
			weaponInfo.Weapon = GRead___WeaponFishNet_002ESerializing_002EGenerateds(reader);
			weaponInfo.ProjectileType = reader.ReadUInt8Unpacked();
			weaponInfo.ProjectileDamage = reader.ReadInt32();
			weaponInfo.ProjectileForce = reader.ReadSingle();
			weaponInfo.ProjectileGravity = reader.ReadSingle();
			weaponInfo.ShootVFX = reader.ReadStringAllocated();
			weaponInfo.BoatForceOverride = reader.ReadSingle();
			return weaponInfo;
		}

		public static Weapon GRead___WeaponFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Weapon)reader.ReadNetworkBehaviour();
		}

		public static Vector3[] GRead___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<Vector3>();
		}

		public static float[] GRead___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<float>();
		}

		public static Quaternion[] GRead___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return reader.ReadArrayAllocated<Quaternion>();
		}

		public static DeadPlayer GRead___DeadPlayerFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (DeadPlayer)reader.ReadNetworkBehaviour();
		}

		public static Tool GRead___ToolFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Tool)reader.ReadNetworkBehaviour();
		}

		public static Creature GRead___CreatureFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Creature)reader.ReadNetworkBehaviour();
		}

		public static Melee GRead___MeleeFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Melee)reader.ReadNetworkBehaviour();
		}

		public static Explosive GRead___ExplosiveFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Explosive)reader.ReadNetworkBehaviour();
		}

		public static Radio GRead___RadioFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (Radio)reader.ReadNetworkBehaviour();
		}

		public static half GRead___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return new half
			{
				value = reader.ReadUInt16()
			};
		}

		public static DamageType GRead___DamageTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			return (DamageType)reader.ReadUInt8Unpacked();
		}
	}
}
