// Wire-exact replacements for the convenience serializers that existed only as
// instance methods on the prebuilt FishNet.Runtime.dll's Writer/Reader.
//
// Why this file exists
// --------------------
// We ported FishNet to source (Assets/FishNet) to fix the wasm
// function-signature crash, but the source version (4.1.0 lineage) lacks a
// handful of Writer/Reader methods that the DLL-era version had natively.
// Every body below was transcribed from the backup DLL's IL
// (_fishnet_backup/FishNet.Runtime.dll, dumped with dncil), so the bytes on
// the wire are identical to the original Windows build:
//
//   Writer.WriteUInt8Unpacked(byte)            == WriteByte (IL: buffer[Position++]=v)
//   Reader.ReadUInt8Unpacked()                 == ReadByte  (IL: buffer[Position++])
//   Writer.WriteQuaternion32(Quaternion)       == WriteQuaternion(Packed)   (4-byte smallest-three)
//   Reader.ReadQuaternion32()                  == ReadQuaternion(Packed)
//   Writer.Writehalf(half)                     == WriteUInt16(value.value)  (raw 2 bytes)
//   Reader.Readhalf()                          == ReadUInt16 -> half{value}
//   Reader.ReadStringAllocated()               == ReadString (same size/-1/0/attack-check flow)
//   Reader.ReadUInt8ArrayAndSizeAllocated()    == ReadBytesAndSizeAllocated
//
// Put in namespace FishNet.Serializing (not ...Generated) so call sites that
// merely have `using FishNet.Serializing;` resolve them without extra usings.
using Unity.Mathematics;
using UnityEngine;

namespace FishNet.Serializing
{
	internal static class LegacyWriterSerializers
	{
		public static void WriteUInt8Unpacked(this Writer writer, byte value) =>
			writer.WriteByte(value);

		public static void WriteQuaternion32(this Writer writer, Quaternion value) =>
			writer.WriteQuaternion(value, AutoPackType.Packed);

		public static void Writehalf(this Writer writer, half value) =>
			writer.WriteUInt16(value.value);
	}

	internal static class LegacyReaderSerializers
	{
		public static byte ReadUInt8Unpacked(this Reader reader) =>
			reader.ReadByte();

		public static Quaternion ReadQuaternion32(this Reader reader) =>
			reader.ReadQuaternion(AutoPackType.Packed);

		public static half Readhalf(this Reader reader)
		{
			half h = default;
			h.value = reader.ReadUInt16();
			return h;
		}

		public static string ReadStringAllocated(this Reader reader) =>
			reader.ReadString();

		public static byte[] ReadUInt8ArrayAndSizeAllocated(this Reader reader) =>
			reader.ReadBytesAndSizeAllocated();
	}
}
