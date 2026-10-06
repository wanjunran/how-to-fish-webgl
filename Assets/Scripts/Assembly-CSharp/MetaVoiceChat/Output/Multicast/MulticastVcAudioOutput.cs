namespace MetaVoiceChat.Output.Multicast
{
	public class MulticastVcAudioOutput : VcAudioOutput
	{
		public VcAudioOutput[] multicastOutputs;

		protected override void ReceiveFrame(int index, float[] samples, float targetLatency)
		{
			if (multicastOutputs == null)
			{
				return;
			}
			VcAudioOutput[] array = multicastOutputs;
			foreach (VcAudioOutput vcAudioOutput in array)
			{
				if (vcAudioOutput != null)
				{
					vcAudioOutput.ReceiveAndFilterFrame(index, samples, targetLatency);
				}
			}
		}
	}
}
