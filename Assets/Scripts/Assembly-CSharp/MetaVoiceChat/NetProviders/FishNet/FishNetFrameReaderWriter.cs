using System;
using FishNet.Serializing;
using UnityEngine;

namespace MetaVoiceChat.NetProviders.FishNet
{
	public static class FishNetFrameReaderWriter
	{
		private const float MaxAdditionalLatency = 0.2f;

		public static void WriteFishNetFrame(this Writer writer, FishNetFrame value)
		{
			writer.WriteInt32(value.index);
			writer.WriteDouble(value.timestamp);
			float num = Mathf.Clamp(value.additionalLatency, 0f, 0.2f) / 0.2f;
			writer.WriteUInt8Unpacked((byte)(num * 255f));
			writer.WriteUInt16(value.Length);
			if (value.Length != 0)
			{
				writer.WriteUInt8Array(value.data.Array, value.data.Offset, value.Length);
			}
		}

		public static FishNetFrame ReadFishNetFrame(this Reader reader)
		{
			int index = reader.ReadInt32();
			double timestamp = reader.ReadDouble();
			float additionalLatency = (float)(int)reader.ReadUInt8Unpacked() / 255f * 0.2f;
			ushort num = reader.ReadUInt16();
			if (num != 0)
			{
				ArraySegment<byte> data = reader.ReadArraySegment(num);
				return new FishNetFrame(index, timestamp, additionalLatency, data);
			}
			return new FishNetFrame(index, timestamp, additionalLatency);
		}
	}
}
