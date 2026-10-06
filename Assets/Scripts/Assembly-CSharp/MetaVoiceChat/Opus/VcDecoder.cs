using System;
using Concentus;

namespace MetaVoiceChat.Opus
{
	public class VcDecoder : IDisposable
	{
		private readonly IOpusDecoder opusDecoder;

		private readonly float[] buffer;

		public string Version => opusDecoder.GetVersionString();

		public bool HasDecodedYet { get; private set; }

		public VcDecoder(VcConfig config)
		{
			opusDecoder = OpusCodecFactory.CreateDecoder(48000, 1);
			buffer = new float[config.samplesPerFrame];
		}

		public ReadOnlySpan<float> DecodeFrame(ReadOnlySpan<byte> data, bool decodeFec = false)
		{
			HasDecodedYet = true;
			int length = opusDecoder.Decode(data, buffer, buffer.Length, decodeFec);
			return buffer.AsSpan(0, length);
		}

		public void ResetState()
		{
			opusDecoder.ResetState();
		}

		public void Dispose()
		{
			opusDecoder.Dispose();
		}
	}
}
