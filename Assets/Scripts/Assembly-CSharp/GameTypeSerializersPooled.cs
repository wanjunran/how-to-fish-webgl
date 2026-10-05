// FishNet RPC 序列化器：Pooled 变体。
//
// 为什么需要这个文件
// ------------------
// AssetRipper 反编译 IL 时跳过了 FishNet 的 ILPostProcessor 构建步骤，所以
// GeneratedWriters___Internal / GeneratedReaders___Internal 里那些针对游戏类型
// （Player/Creature/Item/WeaponInfo...）的 GWrite___/GRead___ 方法，在本工程里
// 一个都不存在。GameTypeSerializers.cs 手写补回了它们，方法名保持与 FishNet
// 编码的完全一致。
//
// 为什么要再写一个 Pooled 变体
// --------------------------
// 原来只补 Writer/Reader 签名就够了，但会在运行时崩：
//
//   RuntimeError: function signature mismatch
//     at wasm-function[103867]:0x1c91f10
//
// 当时的判断是：调用点传的是 PooledWriter（子类），C# 按基类重载解析
// 通过、编译零报错，但 IL2CPP 生成的间接调用签名与表项对不上。
// 于是为每个方法补了参数类型精确为 PooledWriter/PooledReader 的重载。
//
// 【这个判断后来被证伪了】—— 本地抓到完整 JS 栈 + 反汇编字节码之后，
// 真实调用链是：
//
//   func[1374] -> func[162584] -> invoke_iiii (JS)
//     -> func[162600] -> func[157585]   <- IL2CPP 委托 thunk
//       -> call_indirect (type 2 = (i32,i32))
//         -> func[103867]              <- 2 参
//           -> call_indirect (type 1 = (i32,i32,i32))
//
// func[157585] 整个函数只有三条 local.get 加一条 call_indirect，
// 是 IL2CPP 生成的委托 Invoke 桩，不是业务代码。整条链都在
// FishNet 内部的委托机制上，跟这 33 个方法的签名无关。
//
// 另外踩过一个坑值得记下来：最初以为两次构建的报错索引
// "103863 vs 103867 只差 4"说明签名修复往前走了一步——错的，
// IL2CPP 每次构建重排函数编号，跨构建比索引毫无意义。
// 旧产物的 func[103863] 是个静态字段访问器，压根没有 call_indirect。
//
// 所以本文件现在**不是**修复手段，只是让声明与调用点的
// 静态类型严格一致（本身是更干净的做法，保留）。
// 加载崩溃的真正原因还在 FishNet 的委托链上，未解决。
//
// 参数与调用点一一对应，不要改成基类，也不要动方法名——方法名里编码了
// 序列化器所属的命名空间。

using System;
using FishNet.Object;
using FishNet.Serializing;
using Unity.Mathematics;
using UnityEngine;

namespace FishNet.Serializing.Generated
{
	internal static class GameTypeSerializersPooled
	{
		// ---- Write side:PooledWriter -----------------------------------------

		public static void GWrite___DamageTypeFishNet_002ESerializing_002EGenerated(PooledWriter writer, DamageType value) =>
			writer.WriteInt32((int)value);

		public static void GWrite___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerated(PooledWriter writer, Vector3[] value) =>
			writer.WriteArray(value);

		public static void GWrite___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerated(PooledWriter writer, Quaternion[] value) =>
			writer.WriteArray(value);

		public static void GWrite___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(PooledWriter writer, float[] value) =>
			writer.WriteArray(value);

		public static void GWrite___System_002EByte_005B_005DFishNet_002ESerializing_002EGenerated(PooledWriter writer, byte[] value) =>
			writer.WriteArray(value);

		public static void GWrite___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerated(PooledWriter writer, half value) =>
			writer.Writehalf(value);

		// NetworkBehaviour 派生的类型在 FishNet 里按 spawn 引用序列化，
		// 不是逐字段写。
		public static void GWrite___PlayerFishNet_002ESerializing_002EGenerated(PooledWriter writer, Player value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___CreatureFishNet_002ESerializing_002EGenerated(PooledWriter writer, Creature value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___ItemFishNet_002ESerializing_002EGenerated(PooledWriter writer, Item value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___DeadPlayerFishNet_002ESerializing_002EGenerated(PooledWriter writer, DeadPlayer value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___ToolFishNet_002ESerializing_002EGenerated(PooledWriter writer, Tool value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___WeaponFishNet_002ESerializing_002EGenerated(PooledWriter writer, Weapon value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___MeleeFishNet_002ESerializing_002EGenerated(PooledWriter writer, Melee value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___FishingRodFishNet_002ESerializing_002EGenerated(PooledWriter writer, FishingRod value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___RadioFishNet_002ESerializing_002EGenerated(PooledWriter writer, Radio value) =>
			writer.WriteNetworkBehaviour(value);

		public static void GWrite___ExplosiveFishNet_002ESerializing_002EGenerated(PooledWriter writer, Explosive value) =>
			writer.WriteNetworkBehaviour(value);

		// WeaponInfo 是普通类，逐字段写。字段顺序必须与下面的 reader 一致。
		//
		// 开头的 bool 是 FishNet 的空引用标记，极性和看上去相反：
		// true 表示"这个引用是 null"。取自 LoadQueueData 的真实 IL——
		// 写侧 brtrue 到写 true 然后 return，读侧 brfalse -> ldnull。
		public static void GWrite___WeaponInfoFishNet_002ESerializing_002EGenerated(PooledWriter writer, WeaponInfo value)
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

		// ---- Read side:PooledReader ------------------------------------------
		// 与写侧逐字段镜像。

		public static DamageType GRead___DamageTypeFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			(DamageType)reader.ReadInt32();

		public static Vector3[] GRead___UnityEngine_002EVector3_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadArrayAllocated<Vector3>();

		public static Quaternion[] GRead___UnityEngine_002EQuaternion_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadArrayAllocated<Quaternion>();

		public static float[] GRead___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadArrayAllocated<float>();

		public static half GRead___Unity_002EMathematics_002EhalfFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.Readhalf();

		public static Player GRead___PlayerFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as Player;

		public static Creature GRead___CreatureFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as Creature;

		public static Item GRead___ItemFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as Item;

		public static DeadPlayer GRead___DeadPlayerFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as DeadPlayer;

		public static Tool GRead___ToolFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as Tool;

		public static Weapon GRead___WeaponFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as Weapon;

		public static Melee GRead___MeleeFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as Melee;

		public static FishingRod GRead___FishingRodFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as FishingRod;

		public static Radio GRead___RadioFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as Radio;

		public static Explosive GRead___ExplosiveFishNet_002ESerializing_002EGenerateds(PooledReader reader) =>
			reader.ReadNetworkBehaviour() as Explosive;

		public static WeaponInfo GRead___WeaponInfoFishNet_002ESerializing_002EGenerateds(PooledReader reader)
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
