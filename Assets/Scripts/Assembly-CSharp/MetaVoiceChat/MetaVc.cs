using System;
using System.Diagnostics;
using MetaVoiceChat.Input;
using MetaVoiceChat.NetProviders;
using MetaVoiceChat.Opus;
using MetaVoiceChat.Output;
using MetaVoiceChat.Utils;
using UnityEngine;

namespace MetaVoiceChat
{
	public class MetaVc : MonoBehaviour
	{
		private const string CodecTimeOverrunMessage = "Opus codec took too long this frame. It is recommended to decrease Opus complexity until this message is rare as long as you have a sensible max codec ms value chosen to maintain your desired fps.";

		[Header("General")]
		public VcAudioInput audioInput;

		public VcAudioOutput audioOutput;

		public VcConfig config;

		[Header("Testing")]
		[Tooltip("This plays back the voice of the local player.")]
		public bool isEchoEnabled;

		[Tooltip("This overwrites the audio input with a 200 Hz sine wave at 20% volume.")]
		public bool isSineOverrideEnabled;

		[Tooltip("The maximum time allowed per frame in milliseconds for all Opus encoding and decoding before giving a warning. This helps you ensure that the Opus codec is not limiting your fps. Disable the warnings by increasing this to its max.")]
		[Range(0f, 100f)]
		public float maxCodecMilliseconds = 50f;

		[Tooltip("This allows multiple codec time overrun warnings per frame.")]
		public bool allowMultipleCodecWarningsPerFrame;

		[Header("Serializable Reactive Properties")]
		[Tooltip("This is the local player and they don't want to hear anyone else.")]
		public MetaSerializableReactiveProperty<bool> isDeafened;

		[Tooltip("This is the local player and they don't want anyone to hear them.")]
		public MetaSerializableReactiveProperty<bool> isInputMuted;

		[Tooltip("This is a remote player that the local player doesn't want to hear.")]
		public MetaSerializableReactiveProperty<bool> isOutputMuted;

		[Tooltip("This player is speaking or trying to speak.")]
		public MetaSerializableReactiveProperty<bool> isSpeaking;

		private INetProvider netProvider;

		private bool isLocalPlayer;

		private VcEncoder encoder;

		private VcDecoder decoder;

		private VcJitter jitter;

		private readonly Stopwatch stopwatch = new Stopwatch();

		private static readonly FrameStopwatch codecStopwatch = new FrameStopwatch();

		private double Timestamp => stopwatch.Elapsed.TotalSeconds;

		private bool CannotSpeak
		{
			get
			{
				if (!netProvider.IsLocalPlayerDeafened)
				{
					return isOutputMuted;
				}
				return true;
			}
		}

		private bool ShouldLocalEcho
		{
			get
			{
				if (isLocalPlayer)
				{
					return isEchoEnabled;
				}
				return false;
			}
		}

		private void Awake()
		{
			config.Init();
			codecStopwatch.Reset();
		}

		public void StartClient(INetProvider netProvider, bool isLocalPlayer, int maxDataBytesPerPacket)
		{
			if (this.netProvider == netProvider)
			{
				MonoBehaviour.print("netprovider already set");
				return;
			}
			this.netProvider = netProvider;
			this.isLocalPlayer = isLocalPlayer;
			if (isLocalPlayer)
			{
				encoder = new VcEncoder(config, maxDataBytesPerPacket);
				audioInput.OnFrameReady += SendFrame;
				audioInput.StartLocalPlayer();
			}
			decoder = new VcDecoder(config);
			jitter = new VcJitter(config);
			stopwatch.Start();
		}

		private void SendFrame(int index, float[] samples)
		{
			if (samples != null && isSineOverrideEnabled)
			{
				float num = MathF.PI / 40f;
				for (int i = 0; i < samples.Length; i++)
				{
					samples[i] = 0.2f * Mathf.Sin((float)i * num);
				}
			}
			bool flag = samples != null;
			isSpeaking.Value = flag;
			if ((!isEchoEnabled) ? (!flag || (bool)isDeafened || (bool)isInputMuted) : (!flag))
			{
				ReceiveFrame(index, Timestamp, 0f, ReadOnlySpan<byte>.Empty);
				netProvider.RelayFrame(index, Timestamp, ReadOnlySpan<byte>.Empty);
				return;
			}
			bool hasEncodedYet = encoder.HasEncodedYet;
			codecStopwatch.Start();
			ReadOnlySpan<byte> data = encoder.EncodeFrame(samples.AsSpan());
			codecStopwatch.Stop(maxCodecMilliseconds, "Opus codec took too long this frame. It is recommended to decrease Opus complexity until this message is rare as long as you have a sensible max codec ms value chosen to maintain your desired fps.", !hasEncodedYet, allowMultipleCodecWarningsPerFrame);
			if (isEchoEnabled)
			{
				ReceiveFrame(index, Timestamp, 0f, data);
			}
			else
			{
				ReceiveFrame(index, Timestamp, 0f, ReadOnlySpan<byte>.Empty);
			}
			if ((bool)isDeafened || (bool)isInputMuted)
			{
				netProvider.RelayFrame(index, Timestamp, ReadOnlySpan<byte>.Empty);
			}
			else
			{
				netProvider.RelayFrame(index, Timestamp, data);
			}
		}

		public void ReceiveFrame(int index, double timestamp, float additionalLatency, ReadOnlySpan<byte> data)
		{
			float num = config.secondsPerFrame * config.outputMinBufferSize + Time.deltaTime + additionalLatency;
			if (!isLocalPlayer)
			{
				float num2 = jitter.Update(timestamp);
				num += num2;
			}
			if (data.Length == 0)
			{
				SetIsSpeaking(value: false);
				audioOutput.ReceiveAndFilterFrame(index, null, num);
				return;
			}
			SetIsSpeaking(value: true);
			if (CannotSpeak && !ShouldLocalEcho)
			{
				audioOutput.ReceiveAndFilterFrame(index, null, num);
				return;
			}
			bool hasDecodedYet = decoder.HasDecodedYet;
			codecStopwatch.Start();
			ReadOnlySpan<float> readOnlySpan = decoder.DecodeFrame(data);
			codecStopwatch.Stop(maxCodecMilliseconds, "Opus codec took too long this frame. It is recommended to decrease Opus complexity until this message is rare as long as you have a sensible max codec ms value chosen to maintain your desired fps.", !hasDecodedYet, allowMultipleCodecWarningsPerFrame);
			if (readOnlySpan.Length == config.samplesPerFrame)
			{
				float[] array = FixedLengthArrayPool<float>.Rent(readOnlySpan.Length);
				readOnlySpan.CopyTo(array);
				audioOutput.ReceiveAndFilterFrame(index, array, num);
				FixedLengthArrayPool<float>.Return(array);
			}
			else
			{
				audioOutput.ReceiveAndFilterFrame(index, null, num);
			}
		}

		public void StopClient()
		{
			if (isLocalPlayer)
			{
				encoder.Dispose();
				audioInput.OnFrameReady -= SendFrame;
			}
			decoder.Dispose();
		}

		private void SetIsSpeaking(bool value)
		{
			if (!isLocalPlayer)
			{
				isSpeaking.Value = value;
			}
		}
	}
}
