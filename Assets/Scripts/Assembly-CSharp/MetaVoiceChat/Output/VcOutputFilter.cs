using UnityEngine;
using UnityEngine.Serialization;

namespace MetaVoiceChat.Output
{
	public abstract class VcOutputFilter : MonoBehaviour
	{
		[Tooltip("The next audio output filter in the pipeline. This can be null.")]
		[FormerlySerializedAs("nextOutputFilter")]
		public VcOutputFilter optionalNextOutputFilter;

		protected abstract void Filter(int index, float[] samples, float targetLatency);

		public void FilterRecursively(int index, float[] samples, float targetLatency)
		{
			VcOutputFilter vcOutputFilter = this;
			while (vcOutputFilter != null && samples != null)
			{
				if (vcOutputFilter.isActiveAndEnabled)
				{
					vcOutputFilter.Filter(index, samples, targetLatency);
				}
				vcOutputFilter = vcOutputFilter.optionalNextOutputFilter;
			}
		}

		private void OnValidate()
		{
			if (optionalNextOutputFilter == this)
			{
				optionalNextOutputFilter = null;
				Debug.LogWarning("Next output filter cannot be set to itself. Resetting to null.", this);
			}
		}
	}
}
