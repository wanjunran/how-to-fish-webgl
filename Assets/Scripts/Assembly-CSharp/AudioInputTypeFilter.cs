using MetaVoiceChat.Input;

public class AudioInputTypeFilter : VcInputFilter
{
	protected override void Filter(int index, ref float[] samples)
	{
		if (samples == null || samples.Length == 0)
		{
			PlayerUI.SetVoiceVolume(-80f);
			return;
		}
		float decibel = AudioManager.GetDecibel(samples);
		if (AudioManager.VoiceInputType == VoiceInputType.Off || (AudioManager.VoiceInputType == VoiceInputType.PushToTalk && !AudioManager.IsHoldingPushToTalk))
		{
			samples = null;
			PlayerUI.SetVoiceVolume(-80f);
		}
		else
		{
			PlayerUI.SetVoiceVolume(decibel);
		}
		PlayerMouth.SetLocalSamples(samples);
	}
}
