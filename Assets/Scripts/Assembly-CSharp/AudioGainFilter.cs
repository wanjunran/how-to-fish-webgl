using MetaVoiceChat.Input;

public class AudioGainFilter : VcInputFilter
{
	private const float _defaultGain = 5f;

	protected override void Filter(int index, ref float[] samples)
	{
		if (samples != null && samples.Length != 0)
		{
			for (int i = 0; i < samples.Length; i++)
			{
				samples[i] *= 5f * AudioManager.MicrophoneGain;
			}
		}
	}
}
