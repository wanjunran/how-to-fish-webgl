using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MetaVoiceChat.Input.Mic
{
	public class VcMic : IDisposable
	{
		private readonly MonoBehaviour coroutineProvider;

		private readonly int samplesPerFrame;

		private int nextFrameIndex;

		private Coroutine recordCoroutine;

		public bool IsRecording { get; private set; }

		public AudioClip AudioClip { get; private set; }

		public string[] Devices => Microphone.devices;

		public string SelectedDevice { get; private set; }

		public string ActiveDevice { get; private set; }

		private int NextFrameIndex => nextFrameIndex++;

		public event Action<int, float[]> OnFrameReady;

		public event Action<string> OnActiveDeviceChanged;

		public VcMic(MonoBehaviour coroutineProvider, int samplesPerFrame)
		{
			this.coroutineProvider = coroutineProvider;
			this.samplesPerFrame = samplesPerFrame;
		}

		public void SetSelectedDevice(string device)
		{
			if (!(device == SelectedDevice))
			{
				SelectedDevice = device;
				if (IsRecording)
				{
					StartRecording();
				}
			}
		}

		public bool StartRecording()
		{
			StopRecording();
			if (Devices.Length == 0)
			{
				Debug.LogWarning("No microphone detected for voice chat!");
				return false;
			}
			if (!Devices.Contains(SelectedDevice))
			{
				ActiveDevice = Devices[0];
			}
			else
			{
				ActiveDevice = SelectedDevice;
			}
			OnActiveDeviceChanged?.Invoke(ActiveDevice);
			AudioClip = Microphone.Start(ActiveDevice, loop: true, 1, 48000);
			if (AudioClip == null)
			{
				Debug.LogWarning("Microphone failed to start recording for voice chat!");
				StopRecording();
				return false;
			}
			if (AudioClip.channels != 1)
			{
				Debug.LogWarning("Microphone must have exactly one channel for voice chat!");
				StopRecording();
				return false;
			}
			recordCoroutine = coroutineProvider.StartCoroutine(CoRecord());
			IsRecording = true;
			return true;
		}

		public void StopRecording()
		{
			if (recordCoroutine != null)
			{
				coroutineProvider.StopCoroutine(recordCoroutine);
				recordCoroutine = null;
			}
			IsRecording = false;
			if (Microphone.IsRecording(ActiveDevice))
			{
				Microphone.End(ActiveDevice);
			}
			UnityEngine.Object.Destroy(AudioClip);
			AudioClip = null;
			if (ActiveDevice != null)
			{
				ActiveDevice = null;
				OnActiveDeviceChanged?.Invoke(ActiveDevice);
			}
		}

		private IEnumerator CoRecord()
		{
			int i = 0;
			int readAbsPos = 0;
			int prevPos = 0;
			float[] samples = new float[samplesPerFrame];
			while (AudioClip != null && Microphone.IsRecording(ActiveDevice))
			{
				bool flag = true;
				while (flag)
				{
					int position = Microphone.GetPosition(ActiveDevice);
					if (position < prevPos)
					{
						i++;
					}
					prevPos = position;
					int num = i * AudioClip.samples + position;
					int num2 = readAbsPos + samples.Length;
					if (num2 < num)
					{
						int offsetSamples = readAbsPos % AudioClip.samples;
						AudioClip.GetData(samples, offsetSamples);
						int arg = NextFrameIndex;
						OnFrameReady?.Invoke(arg, samples);
						readAbsPos = num2;
						flag = true;
					}
					else
					{
						flag = false;
					}
				}
				yield return null;
			}
			StopRecording();
		}

		public void Dispose()
		{
			StopRecording();
		}
	}
}
