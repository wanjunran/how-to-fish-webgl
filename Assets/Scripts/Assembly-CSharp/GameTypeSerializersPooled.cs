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
// 但只补 Writer/Reader 签名是不够的，会在运行时崩：
//
//   RuntimeError: function signature mismatch
//     at wasm-function[103867]
//     at wasm-function[157585]
//
// 机制（已从产物的 wasm 字节码确认，不是推测）：
//   func[103867] 开头是 20 00 28 02 14 —— local.get 0 后 call 4354，
//   也就是 call_indirect。IL2CPP 把委托调用编译成经函数表的间接调用，
//   表里放的是按**声明时精确签名**生成的包装器。Emscripten 的 invoke_*
//   在运行时比对调用点期望的签名与表项的实际签名，不一致就抛
//   "function signature mismatch"，而且这个检查发生在加载场景时，
//   所以页面表现是进度条卡在 90% 不动。
//
// 调用点实际传进来的类型统计（grep 全Assets/Scripts 得到）：
//   pooledWriter  85 处   -> PooledWriter
//   PooledReader0 83 处   -> PooledReader
//   Writer/Reader  33 处-> 只出现在 GameTypeSerializers.cs 的方法声明里
//
// 也就是说走 RPC 的路径**全部**传的是 Pooled 子类实例。PooledWriter 继承
// Writer，所以 C# 编译器按基类重载解析通过、编译零报错，但 IL2CPP 生成的
// 间接调用签名与表项对不上——这就是那个崩溃。
//
// 做法：为每个方法补一个参数类型精确为 PooledWriter/PooledReader 的重载。
// 保留原有的 Writer/Reader 版本，两者共存由重载决议按实参静态类型选择，
// 保证函数表里一定存在与调用点一致的签名。
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
