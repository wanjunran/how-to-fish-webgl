using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace MetaVoiceChat.Input.Mic
{
	public class VcMicAudioInput : VcAudioInput
	{
		public string ActiveDevice => Mic?.ActiveDevice ?? null;

		public VcMic Mic { get; private set; }

		public bool IsInitialized => Mic != null;

		public event Action<string> OnActiveDeviceChanged;

		public override void StartLocalPlayer()
		{
			int samplesPerFrame = metaVc.config.samplesPerFrame;
			Mic = new VcMic(this, samplesPerFrame);
			Mic.OnFrameReady += base.SendAndFilterFrame;
			Mic.OnActiveDeviceChanged += Mic_OnActiveDeviceChanged;
			ButtonManager.Micro.OnValueChanged += SetSelectedDevice;
			if (ButtonManager.Micro.Value == "ah")
			{
				SetSelectedDevice(ButtonManager.Micro.Value);
			}
			if (Mic.Devices.Length != 0)
			{
				Mic.StartRecording();
			}
			StartCoroutine(CoReconnect());
		}

		private void OnDestroy()
		{
			if (Mic != null)
			{
				Mic.OnFrameReady -= base.SendAndFilterFrame;
				Mic.OnActiveDeviceChanged -= Mic_OnActiveDeviceChanged;
				Mic.Dispose();
				Mic = null;
				ButtonManager.Micro.OnValueChanged -= SetSelectedDevice;
				StopAllCoroutines();
			}
		}

		private IEnumerator CoReconnect()
		{
			yield return new WaitForSecondsRealtime(1f);
			while (Mic != null)
			{
				while (!ShouldReconnect())
				{
					yield return null;
				}
				if (Mic == null)
				{
					break;
				}
				Mic.StopRecording();
				yield return null;
				yield return null;
				if (Mic == null)
				{
					break;
				}
				if (Mic.Devices.Length != 0)
				{
					if (!Mic.StartRecording())
					{
						yield return new WaitForSecondsRealtime(4f);
					}
				}
				else
				{
					yield return new WaitForSecondsRealtime(1f);
				}
				yield return null;
				yield return null;
			}
		}

		private void Mic_OnActiveDeviceChanged(string device)
		{
			OnActiveDeviceChanged?.Invoke(device);
		}

		public void SetSelectedDevice(string device)
		{
			if (Mic != null)
			{
				Mic.SetSelectedDevice(device);
			}
		}

		private bool ShouldReconnect()
		{
			if (Mic == null)
			{
				return true;
			}
			if (!Mic.IsRecording || !Mic.Devices.Contains(Mic.ActiveDevice))
			{
				return true;
			}
			if (Mic.SelectedDevice != Mic.ActiveDevice && Mic.Devices.Contains(Mic.SelectedDevice))
			{
				return true;
			}
			return false;
		}
	}
}
