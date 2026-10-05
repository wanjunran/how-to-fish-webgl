// Hand-written replacements for the FishNet codegen output that AssetRipper
// never exported.
//
// Background
// ----------
// FishNet's ILPostProcessor generates a GWrite___<Type> / GRead___<Type>s pair
// for every type crossing an RPC boundary, into
// FishNet.Serializing.Generated.GeneratedWriters___Internal /
// GeneratedReaders___Internal. AssetRipper decompiles IL but skips that build
// step, so those 33 methods exist in NO assembly in this project - which is why
// the build reported 1008x CS0117 "type does not contain a definition".
//
// We cannot simply re-declare the original class names: FishNet.Runtime.dll
// already defines both types (61 internal base-type serializers each), so a
// same-named class here would be CS0433 all over again. Instead this file
// provides the same method bodies under a distinct class name, and the call
// sites are rewritten by tools/rewrite_generated_calls.py to match.
//
// The method names must stay byte-for-byte identical, because they encode the
// namespace they were generated for. Do not rename them.
//
// Verified against the real FishNet IL: enum -> WriteInt32/ReadInt32,
// array -> Writer.WriteArray<T>/Reader.ReadArrayAllocated<T>, and every
// NetworkBehaviour-derived type -> WriteNetworkBehaviour/ReadNetworkBehaviour,
// because FishNet serializes those as a spawn reference, not field by field.

using System;
using FishNet.Object;
using FishNet.Serializing;
using Unity.Mathematics;
using UnityEngine;

namespace FishNet.Serializing.Generated
{
	internal static class GameTypeSerializers
	{
		// ---- Write side ----------------------------------------------------

		public static void GWrite___DamageTypeFishNet_002ESerializing_002EGenerated(Writer writer, DamageType value) =>
			writer.WriteInt32((int)value);

		public static void GWrite___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerated(Writer writer, Vector3[] value) =>
			writer.WriteArray(value);

		public static void GWrite___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerated(Writer writer, Quaternion[] value) =>
			writer.WriteArray(value);

		public static void GWrite___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(Writer writer, float[] value) =>
			writer.WriteArray(value);

		public static void GWrite___System_002EByte_005B_005DFishNet_002ESerializing_002EGenerated(Writer writer, byte[] value) =>
			writer.WriteArray(value);

		public static void GWrite___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerated(Writer writer, half value) =>
			writer.Writehalf(value);

		// Every one of these derives from NetworkBehaviour, so FishNet writes a
		// spawn reference rather than the object's fields.
		public static void GWrite___PlayerFishNet_002ESerializing_002EGenerated(Writer writer, Player value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___CreatureFishNet_002ESerializing_002EGenerated(Writer writer, Creature value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___ItemFishNet_002ESerializing_002EGenerated(Writer writer, Item value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___DeadPlayerFishNet_002ESerializing_002EGenerated(Writer writer, DeadPlayer value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___ToolFishNet_002ESerializing_002EGenerated(Writer writer, Tool value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___WeaponFishNet_002ESerializing_002EGenerated(Writer writer, Weapon value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___MeleeFishNet_002ESerializing_002EGenerated(Writer writer, Melee value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___FishingRodFishNet_002ESerializing_002EGenerated(Writer writer, FishingRod value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___RadioFishNet_002ESerializing_002EGenerated(Writer writer, Radio value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___ExplosiveFishNet_002ESerializing_002EGenerated(Writer writer, Explosive value) =>
			writer.WriteNetworkBehaviour(value);

		// WeaponInfo is a plain class, not a NetworkBehaviour, so its fields are
		// written out one by one. Field order must match the reader below.
		//
		// The leading bool is FishNet's null marker, and its polarity is the
		// opposite of what it looks like: true means "this reference is null".
		// Confirmed from the generated IL for LoadQueueData, which branches on
		// brtrue to write `true` and return, and whose reader does brfalse ->
		// ldnull.
		public static void GWrite___WeaponInfoFishNet_002ESerializing_002EGenerated(Writer writer, WeaponInfo value)
		{
			if (value == null)
			{
				writer.WriteBoolean(true);
				return;
			}

			writer.WriteBoolean(false);
			writer.WriteNetworkBehaviour(value.Weapon);
			writer.WriteByte(value.ProjectileType);
			writer.WriteInt32(value.ProjectileDamage);
			writer.WriteSingle(value.ProjectileForce);
			writer.WriteSingle(value.ProjectileGravity);
			writer.WriteString(value.ShootVFX);
			writer.WriteSingle(value.BoatForceOverride);
		}

		// ---- Read side -----------------------------------------------------

		public static DamageType GRead___DamageTypeFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			(DamageType)reader.ReadInt32();

		public static Vector3[] GRead___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadArrayAllocated<Vector3>();

		public static Quaternion[] GRead___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadArrayAllocated<Quaternion>();

		public static float[] GRead___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadArrayAllocated<float>();

		public static half GRead___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.Readhalf();

		public static Player GRead___PlayerFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as Player;

		public static Creature GRead___CreatureFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as Creature;

		public static Item GRead___ItemFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as Item;

		public static DeadPlayer GRead___DeadPlayerFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as DeadPlayer;

		public static Tool GRead___ToolFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as Tool;

		public static Weapon GRead___WeaponFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as Weapon;

		public static Melee GRead___MeleeFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as Melee;

		public static FishingRod GRead___FishingRodFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as FishingRod;

		public static Radio GRead___RadioFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as Radio;

		public static Explosive GRead___ExplosiveFishNet_002ESerializing_002EGenerateds(Reader reader) =>
			reader.ReadNetworkBehaviour() as Explosive;

		// Mirror of the writer: a leading true means the reference was null.
		public static WeaponInfo GRead___WeaponInfoFishNet_002ESerializing_002EGenerateds(Reader reader)
		{
			if (reader.ReadBoolean())
				return null;

			return new WeaponInfo
			{
				Weapon = reader.ReadNetworkBehaviour() as Weapon,
				ProjectileType = reader.ReadByte(),
				ProjectileDamage = reader.ReadInt32(),
				ProjectileForce = reader.ReadSingle(),
				ProjectileGravity = reader.ReadSingle(),
				ShootVFX = reader.ReadString(),
				BoatForceOverride = reader.ReadSingle(),
			};
		}
	}
}
