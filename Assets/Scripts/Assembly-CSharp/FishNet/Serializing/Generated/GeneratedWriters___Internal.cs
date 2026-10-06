using System.Runtime.InteropServices;
using MetaVoiceChat.NetProviders.FishNet;
using Unity.Mathematics;
using UnityEngine;

namespace FishNet.Serializing.Generated
{
	[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
	public static class GeneratedWriters___Internal
	{
		[RuntimeInitializeOnLoadMethod]
		private static void InitializeOnce()
		{
			GenericWriter<FishNetFrame>.SetWrite(FishNetFrameReaderWriter.WriteFishNetFrame);
			GenericWriter<FishingRod>.SetWrite(GWrite___FishingRodFishNet_002ESerializing_002EGenerated);
			GenericWriter<Player>.SetWrite(GWrite___PlayerFishNet_002ESerializing_002EGenerated);
			GenericWriter<Bird>.SetWrite(GWrite___BirdFishNet_002ESerializing_002EGenerated);
			GenericWriter<Item>.SetWrite(GWrite___ItemFishNet_002ESerializing_002EGenerated);
			GenericWriter<WeaponInfo>.SetWrite(GWrite___WeaponInfoFishNet_002ESerializing_002EGenerated);
			GenericWriter<Weapon>.SetWrite(GWrite___WeaponFishNet_002ESerializing_002EGenerated);
			GenericWriter<Vector3[]>.SetWrite(GWrite___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerated);
			GenericWriter<byte[]>.SetWrite(GWrite___System_002EByte_005B_005DFishNet_002ESerializing_002EGenerated);
			GenericWriter<float[]>.SetWrite(GWrite___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated);
			GenericWriter<Quaternion[]>.SetWrite(GWrite___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerated);
			GenericWriter<DeadPlayer>.SetWrite(GWrite___DeadPlayerFishNet_002ESerializing_002EGenerated);
			GenericWriter<Tool>.SetWrite(GWrite___ToolFishNet_002ESerializing_002EGenerated);
			GenericWriter<Creature>.SetWrite(GWrite___CreatureFishNet_002ESerializing_002EGenerated);
			GenericWriter<Melee>.SetWrite(GWrite___MeleeFishNet_002ESerializing_002EGenerated);
			GenericWriter<Explosive>.SetWrite(GWrite___ExplosiveFishNet_002ESerializing_002EGenerated);
			GenericWriter<Radio>.SetWrite(GWrite___RadioFishNet_002ESerializing_002EGenerated);
			GenericWriter<half>.SetWrite(GWrite___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerated);
			GenericWriter<DamageType>.SetWrite(GWrite___DamageTypeFishNet_002ESerializing_002EGenerated);
		}

		public static void GWrite___FishingRodFishNet_002ESerializing_002EGenerated(this Writer writer, FishingRod value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___PlayerFishNet_002ESerializing_002EGenerated(this Writer writer, Player value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___BirdFishNet_002ESerializing_002EGenerated(this Writer writer, Bird value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___ItemFishNet_002ESerializing_002EGenerated(this Writer writer, Item value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___WeaponInfoFishNet_002ESerializing_002EGenerated(this Writer writer, WeaponInfo value)
		{
			if (value == null)
			{
				writer.WriteBoolean(value: true);
				return;
			}
			writer.WriteBoolean(value: false);
			GWrite___WeaponFishNet_002ESerializing_002EGenerated(writer, value.Weapon);
			writer.WriteUInt8Unpacked(value.ProjectileType);
			writer.WriteInt32(value.ProjectileDamage);
			writer.WriteSingle(value.ProjectileForce);
			writer.WriteSingle(value.ProjectileGravity);
			writer.WriteString(value.ShootVFX);
			writer.WriteSingle(value.BoatForceOverride);
		}

		public static void GWrite___WeaponFishNet_002ESerializing_002EGenerated(this Writer writer, Weapon value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, Vector3[] value)
		{
			writer.WriteArray(value);
		}

		public static void GWrite___System_002EByte_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, byte[] value)
		{
			writer.WriteArray(value);
		}

		public static void GWrite___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, float[] value)
		{
			writer.WriteArray(value);
		}

		public static void GWrite___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, Quaternion[] value)
		{
			writer.WriteArray(value);
		}

		public static void GWrite___DeadPlayerFishNet_002ESerializing_002EGenerated(this Writer writer, DeadPlayer value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___ToolFishNet_002ESerializing_002EGenerated(this Writer writer, Tool value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___CreatureFishNet_002ESerializing_002EGenerated(this Writer writer, Creature value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___MeleeFishNet_002ESerializing_002EGenerated(this Writer writer, Melee value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___ExplosiveFishNet_002ESerializing_002EGenerated(this Writer writer, Explosive value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___RadioFishNet_002ESerializing_002EGenerated(this Writer writer, Radio value)
		{
			writer.WriteNetworkBehaviour(value);
		}

		public static void GWrite___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerated(this Writer writer, half value)
		{
			writer.WriteUInt16(value.value);
		}

		public static void GWrite___DamageTypeFishNet_002ESerializing_002EGenerated(this Writer writer, DamageType value)
		{
			writer.WriteUInt8Unpacked((byte)value);
		}
	}
}
