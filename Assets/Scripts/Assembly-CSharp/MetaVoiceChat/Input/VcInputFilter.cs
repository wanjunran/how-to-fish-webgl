using UnityEngine;
using UnityEngine.Serialization;

namespace MetaVoiceChat.Input
{
	public abstract class VcInputFilter : MonoBehaviour
	{
		[Tooltip("The next audio input filter in the pipeline. This can be null.")]
		[FormerlySerializedAs("nextInputFilter")]
		public VcInputFilter optionalNextInputFilter;

		protected abstract void Filter(int index, ref float[] samples);

		public void FilterRecursively(int index, ref float[] samples)
		{
			VcInputFilter vcInputFilter = this;
			while (vcInputFilter != null && samples != null)
			{
				if (vcInputFilter.isActiveAndEnabled)
				{
					vcInputFilter.Filter(index, ref samples);
				}
				vcInputFilter = vcInputFilter.optionalNextInputFilter;
			}
		}

		private void OnValidate()
		{
			if (optionalNextInputFilter == this)
			{
				optionalNextInputFilter = null;
				Debug.LogWarning("Next input filter cannot be set to itself. Resetting to null.", this);
			}
		}
	}
}
