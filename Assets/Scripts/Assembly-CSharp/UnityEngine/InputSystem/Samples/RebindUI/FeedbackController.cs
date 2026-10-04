using System;
using System.Collections.Generic;
using UnityEngine.InputSystem.DualShock;

namespace UnityEngine.InputSystem.Samples.RebindUI
{
	public class FeedbackController : MonoBehaviour
	{
		private const float kDefaultOutputFrequency = 10f;

		private const float kDefaultOutputThrottleDelay = 0.1f;

		[Header("Color Output")]
		[Tooltip("The device color output frequency (Hz)")]
		public float colorOutputFrequency = 10f;

		[Header("Force Feedback Output")]
		[Tooltip("The device rumble output frequency (Hz)")]
		public float rumbleOutputFrequency = 10f;

		private const double kRecentThresholdSeconds = 3.0;

		private static readonly Dictionary<InputDevice, double> s_RecentlyUsedDevices = new Dictionary<InputDevice, double>();

		private static InputDevice s_MostRecentInputDevice;

		private bool m_InvalidateLight;

		private bool m_InvalidateRumble;

		private static readonly Color NoLight = Color.black;

		private const float kNoRumble = 0f;

		private double m_NextLightUpdateTime;

		private Color m_DeviceColor = NoLight;

		private double m_NextRumbleUpdateTime;

		private float m_DeviceRumble;

		private float m_Rumble;

		public Color color { get; set; }

		public float rumble
		{
			get
			{
				return m_Rumble;
			}
			set
			{
				m_Rumble = Mathf.Clamp01(value);
			}
		}

		public void RecordRecentDeviceFromAction(InputAction action)
		{
			InputControl activeControl = action.activeControl;
			if (activeControl != null)
			{
				InputDevice device = activeControl.device;
				double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
				if (!s_RecentlyUsedDevices.ContainsKey(device) || realtimeSinceStartupAsDouble - s_RecentlyUsedDevices[device] >= 3.0)
				{
					m_InvalidateLight = true;
					m_InvalidateRumble = true;
				}
				s_RecentlyUsedDevices[device] = realtimeSinceStartupAsDouble;
				s_MostRecentInputDevice = device;
			}
		}

		private void Awake()
		{
			m_NextRumbleUpdateTime = (m_NextLightUpdateTime = Time.realtimeSinceStartupAsDouble);
		}

		private void OnEnable()
		{
			m_InvalidateLight = true;
			m_InvalidateRumble = true;
		}

		private void OnDisable()
		{
			ApplyRumble(0f);
			ApplyLight(NoLight);
		}

		private void Update()
		{
			double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
			if (DetectAbandonedDevices(realtimeSinceStartupAsDouble))
			{
				m_InvalidateLight = (m_InvalidateRumble = true);
			}
			if (realtimeSinceStartupAsDouble >= m_NextLightUpdateTime && (m_InvalidateLight || m_DeviceColor != color))
			{
				m_InvalidateLight = false;
				m_NextLightUpdateTime = ComputeNextUpdateTime(realtimeSinceStartupAsDouble, colorOutputFrequency);
				ApplyLight(color);
			}
			if (realtimeSinceStartupAsDouble >= m_NextRumbleUpdateTime && (m_InvalidateRumble || !Mathf.Approximately(m_DeviceRumble, rumble)))
			{
				m_InvalidateRumble = false;
				m_NextRumbleUpdateTime = ComputeNextUpdateTime(realtimeSinceStartupAsDouble, rumbleOutputFrequency);
				ApplyRumble(rumble);
			}
		}

		private void ApplyLight(Color colorValue)
		{
			m_DeviceColor = colorValue;
			double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
			foreach (Gamepad item in Gamepad.all)
			{
				if (IsRecentlyUsed(item, realtimeSinceStartupAsDouble))
				{
					ApplyLightToDevice(item, colorValue);
				}
				else
				{
					ApplyLightToDevice(item, NoLight);
				}
			}
		}

		private void ApplyLightToDevice(Gamepad device, Color value)
		{
			(device as DualShockGamepad)?.SetLightBarColor(value);
		}

		private void ApplyRumble(float value)
		{
			m_DeviceRumble = value;
			double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
			foreach (Gamepad item in Gamepad.all)
			{
				if (IsRecentlyUsed(item, realtimeSinceStartupAsDouble))
				{
					ApplyRumbleToDevice(item, value);
				}
				else
				{
					ApplyRumbleToDevice(item, 0f);
				}
			}
		}

		private void ApplyRumbleToDevice(Gamepad device, float value)
		{
			device.SetMotorSpeeds(value, 0f);
		}

		private static bool IsRecentlyUsed(InputDevice device, double realtimeSinceStartup, double thresholdSeconds = 3.0)
		{
			if (s_MostRecentInputDevice != device)
			{
				if (s_RecentlyUsedDevices.ContainsKey(device))
				{
					return realtimeSinceStartup - s_RecentlyUsedDevices[device] < thresholdSeconds;
				}
				return false;
			}
			return true;
		}

		private static bool DetectAbandonedDevices(double realTimeSinceStartup)
		{
			bool result = false;
			bool flag;
			do
			{
				flag = false;
				foreach (KeyValuePair<InputDevice, double> s_RecentlyUsedDevice in s_RecentlyUsedDevices)
				{
					if (!(realTimeSinceStartup - s_RecentlyUsedDevice.Value < 3.0) && s_MostRecentInputDevice != s_RecentlyUsedDevice.Key)
					{
						s_RecentlyUsedDevices.Remove(s_RecentlyUsedDevice.Key);
						result = true;
						flag = true;
						break;
					}
				}
			}
			while (flag);
			return result;
		}

		private static double ComputeNextUpdateTime(double now, float frequency)
		{
			float num = (((double)frequency > 0.0) ? (1f / frequency) : 0.1f);
			return Math.Ceiling(now / (double)num) * (double)num;
		}
	}
}
