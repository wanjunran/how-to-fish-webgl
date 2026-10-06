using System;
using Adrenak.RNNoise4Unity;
using MetaVoiceChat.Input;
using UnityEngine;

namespace MetaVoiceChat.Rnnoise
{
	public class RnnoiseVcInputFilter : VcInputFilter
	{
		public MetaVc metaVc;

		private const int DenoiserFramesize = 480;

		private Denoiser denoiser;

		private int multiples;

		private readonly float[] buffer = new float[480];

		private void OnEnable()
		{
			if (metaVc == null)
			{
				Debug.LogError("MetaVc is not assigned. Please assign it in the inspector.");
				return;
			}
			VcConfig config = metaVc.config;
			if (config.samplesPerFrame % 480 != 0)
			{
				Debug.LogError($"RnnoiseVcInputFilter requires samplesPerFrame to be a multiple of {480}. Please adjust the configuration.");
				return;
			}
			multiples = config.samplesPerFrame / 480;
			denoiser = new Denoiser();
		}

		private void OnDisable()
		{
			denoiser?.Dispose();
			denoiser = null;
			multiples = 0;
		}

		protected override void Filter(int index, ref float[] samples)
		{
			if (denoiser == null || multiples == 0 || samples == null || samples.Length == 0)
			{
				return;
			}
			if (samples.Length != multiples * 480)
			{
				Debug.LogWarning($"RnnoiseVcInputFilter requires samples to be of length {multiples * 480}. Please adjust the configuration.");
				return;
			}
			for (int i = 0; i < multiples; i++)
			{
				Array.Copy(samples, i * 480, buffer, 0, 480);
				denoiser.Denoise(buffer);
				Array.Copy(buffer, 0, samples, i * 480, 480);
			}
		}
	}
}
