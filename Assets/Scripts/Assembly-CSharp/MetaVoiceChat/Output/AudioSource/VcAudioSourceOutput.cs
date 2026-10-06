using System.Diagnostics;
using UnityEngine;

namespace MetaVoiceChat.Output.AudioSource
{
	public class VcAudioSourceOutput : VcAudioOutput
	{
		[Tooltip("The output audio source.")]
		public UnityEngine.AudioSource audioSource;

		[Tooltip("The time a frame lives in the buffer for before being cleared out. The units are seconds.")]
		[Range(0.1f, 0.75f)]
		public float frameLifetime = 0.5f;

		[Tooltip("The largest magnitude latency considered negative before wrapping around to positive values. The units are seconds.")]
		[Range(0.1f, 0.5f)]
		public float maxNegativeLatency = 0.25f;

		[Tooltip("The proportional gain of the pitch P-controller. The units are percent per second of latency error.")]
		[Range(0f, 10f)]
		public float pitchProportionalGain = 1f;

		[Tooltip("The maximum increase or decrease in pitch allowed for corrections. The units are percent.")]
		[Range(0f, 0.5f)]
		public float pitchMaxCorrection = 0.2f;

		[SerializeField]
		private Player _player;

		private int framesPerSecond;

		private float secondsPerFrame;

		private VcAudioClip vcAudioClip;

		private int[] clipFrameIndicies;

		private int firstFrameIndex = -1;

		private int greatestFrameIndex = -1;

		private readonly Stopwatch frameStopwatch = new Stopwatch();

		private bool isInit;

		private float targetLatency;

		private float TimeSincePreviousFrame => (float)frameStopwatch.Elapsed.TotalSeconds;

		private void Start()
		{
			audioSource.dopplerLevel = 0f;
			VcConfig config = metaVc.config;
			framesPerSecond = config.framesPerSecond;
			secondsPerFrame = config.secondsPerFrame;
			vcAudioClip = new VcAudioClip(config.samplesPerFrame, config.framesPerClip, audioSource);
			clipFrameIndicies = new int[config.framesPerClip];
			for (int i = 0; i < clipFrameIndicies.Length; i++)
			{
				clipFrameIndicies[i] = -1;
			}
		}

		private void Update()
		{
			if (isInit && TimeSincePreviousFrame > frameLifetime)
			{
				vcAudioClip.Clear();
			}
			if (!isInit)
			{
				int num = ((greatestFrameIndex != -1) ? (greatestFrameIndex - firstFrameIndex + 1) : 0);
				if (num != 0 && (float)num / (float)framesPerSecond + TimeSincePreviousFrame >= targetLatency)
				{
					audioSource.time = GetWrappedTime(firstFrameIndex);
					audioSource.Play();
					isInit = true;
				}
				if (!isInit)
				{
					return;
				}
			}
			float latency = GetLatency();
			float value = (0f - (targetLatency - latency)) * pitchProportionalGain;
			value = Mathf.Clamp(value, 0f - pitchMaxCorrection, pitchMaxCorrection);
			audioSource.pitch = 1f + value;
			ClearOldFrames();
		}

		private void ClearOldFrames()
		{
			for (int i = 0; i < clipFrameIndicies.Length; i++)
			{
				int num = clipFrameIndicies[i];
				if (num != -1 && (float)(greatestFrameIndex - num) * secondsPerFrame > frameLifetime)
				{
					vcAudioClip.ClearFrame(i);
					clipFrameIndicies[i] = -1;
				}
			}
		}

		private float GetLatency()
		{
			return GetRawLatency() + TimeSincePreviousFrame;
		}

		private float GetRawLatency()
		{
			float wrappedTime = GetWrappedTime(greatestFrameIndex);
			float time = audioSource.time;
			float num = wrappedTime - time;
			float length = vcAudioClip.Length;
			if (num < 0f)
			{
				num = length + num;
			}
			if (length - maxNegativeLatency < num)
			{
				num -= length;
			}
			return num;
		}

		private float GetWrappedTime(int frameIndex)
		{
			return (float)vcAudioClip.GetOffsetFrames(frameIndex) * secondsPerFrame;
		}

		protected override void ReceiveFrame(int index, float[] samples, float targetLatency)
		{
			if (vcAudioClip != null)
			{
				this.targetLatency = targetLatency;
				int offsetFrames = vcAudioClip.GetOffsetFrames(index);
				vcAudioClip.WriteFrame(offsetFrames, samples);
				if (!_player.Owner.IsLocalClient)
				{
					_player.Mouth.SetSamples(samples);
				}
				clipFrameIndicies[offsetFrames] = index;
				if (firstFrameIndex == -1)
				{
					firstFrameIndex = index;
				}
				if (index > greatestFrameIndex)
				{
					greatestFrameIndex = index;
				}
				frameStopwatch.Restart();
			}
		}

		private void OnDestroy()
		{
			vcAudioClip.Dispose();
		}
	}
}
