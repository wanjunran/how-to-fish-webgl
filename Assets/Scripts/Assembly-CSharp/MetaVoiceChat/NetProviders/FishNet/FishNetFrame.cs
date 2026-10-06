using System;

namespace MetaVoiceChat.NetProviders.FishNet
{
	public readonly struct FishNetFrame
	{
		public readonly int index;

		public readonly double timestamp;

		public readonly float additionalLatency;

		public readonly ArraySegment<byte> data;

		public ushort Length => (ushort)data.Count;

		public FishNetFrame(int index, double timestamp, float additionalLatency, ArraySegment<byte> data)
		{
			this.index = index;
			this.timestamp = timestamp;
			this.additionalLatency = additionalLatency;
			this.data = data;
		}

		public FishNetFrame(int index, double timestamp, float additionalLatency)
		{
			this.index = index;
			this.timestamp = timestamp;
			this.additionalLatency = additionalLatency;
			data = ArraySegment<byte>.Empty;
		}
	}
}
