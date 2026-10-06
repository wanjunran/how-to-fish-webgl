using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace MetaVoiceChat.Output
{
	public class VcJitter
	{
		private readonly struct Entry
		{
			public readonly double timestamp;

			public readonly double localTimestamp;

			public Entry(double timestamp, double localTimestamp)
			{
				this.timestamp = timestamp;
				this.localTimestamp = localTimestamp;
			}

			public float GetAge(double localTimestamp)
			{
				return (float)(localTimestamp - this.localTimestamp);
			}
		}

		private readonly double timeWindow;

		private readonly int meanOffsetWindow;

		private readonly Stopwatch stopwatch = new Stopwatch();

		private readonly Queue<Entry> entries = new Queue<Entry>();

		private readonly Queue<double> offsets = new Queue<double>();

		private double LocalTimestamp => stopwatch.Elapsed.TotalSeconds;

		public VcJitter(VcConfig config)
		{
			timeWindow = config.jitterTimeWindow;
			meanOffsetWindow = config.jitterMeanOffsetWindow;
		}

		public float Update(double timestamp)
		{
			if (!stopwatch.IsRunning)
			{
				stopwatch.Restart();
				return 0f;
			}
			double localTimestamp = LocalTimestamp;
			entries.Enqueue(new Entry(timestamp, localTimestamp));
			Entry result;
			while (entries.TryPeek(out result) && (double)result.GetAge(localTimestamp) > timeWindow)
			{
				entries.Dequeue();
			}
			offsets.Enqueue(localTimestamp - timestamp);
			if (offsets.Count > meanOffsetWindow)
			{
				offsets.Dequeue();
			}
			double meanOffset = offsets.Average();
			if (entries.Count > 0)
			{
				return Mathf.Sqrt(((IEnumerable<Entry>)entries).Average((Func<Entry, float>)SquareDeviation));
			}
			return 0f;
			float SquareDeviation(Entry e)
			{
				double num = meanOffset + e.timestamp - e.localTimestamp;
				return (float)(num * num);
			}
		}
	}
}
