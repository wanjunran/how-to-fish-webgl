using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaVoiceChat.Utils
{
	public class MicrophoneDevicesListener
	{
		private readonly HashSet<string> devices = new HashSet<string>();

		private readonly Action onDevicesChanged;

		public MicrophoneDevicesListener(Action onDevicesChanged)
		{
			this.onDevicesChanged = onDevicesChanged;
		}

		public void Poll()
		{
			string[] array = Microphone.devices;
			if (HasChanged(array))
			{
				devices.Clear();
				string[] array2 = array;
				foreach (string item in array2)
				{
					devices.Add(item);
				}
				onDevicesChanged?.Invoke();
			}
		}

		private bool HasChanged(string[] actualDevices)
		{
			if (actualDevices.Length != devices.Count)
			{
				return true;
			}
			foreach (string item in actualDevices)
			{
				if (!devices.Contains(item))
				{
					return true;
				}
			}
			return false;
		}
	}
}
