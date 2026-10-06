using System;
using Concentus;
using Concentus.Enums;

namespace MetaVoiceChat.Opus
{
	public class VcEncoder : IDisposable
	{
		private readonly IOpusEncoder opusEncoder;

		private readonly byte[] buffer;

		private readonly int frameSize;

		public string Version => opusEncoder.GetVersionString();

		public bool HasEncodedYet { get; private set; }

		public VcEncoder(VcConfig config, int maxDataBytesPerPacket)
		{
			maxDataBytesPerPacket = Math.Min(maxDataBytesPerPacket, 1275);
			opusEncoder = OpusCodecFactory.CreateEncoder(48000, 1, config.application);
			opusEncoder.Bandwidth = OpusBandwidth.OPUS_BANDWIDTH_WIDEBAND;
			opusEncoder.Complexity = config.complexity;
			opusEncoder.ForceMode = OpusMode.MODE_SILK_ONLY;
			opusEncoder.LSBDepth = 16;
			opusEncoder.MaxBandwidth = OpusBandwidth.OPUS_BANDWIDTH_WIDEBAND;
			opusEncoder.SignalType = config.signal;
			buffer = new byte[maxDataBytesPerPacket];
			frameSize = config.samplesPerFrame;
		}

		public ReadOnlySpan<byte> EncodeFrame(ReadOnlySpan<float> samples)
		{
			HasEncodedYet = true;
			int length = opusEncoder.Encode(samples, frameSize, buffer, buffer.Length);
			return buffer.AsSpan(0, length);
		}

		public void ResetState()
		{
			opusEncoder.ResetState();
		}

		public void Dispose()
		{
			opusEncoder.Dispose();
		}
	}
}
