using System;
using System.Collections.Generic;

namespace MetaVoiceChat.Utils
{
	public static class FixedLengthArrayPool<T>
	{
		private static readonly Dictionary<int, Stack<T[]>> pool = new Dictionary<int, Stack<T[]>>();

		private static readonly object poolLock = new object();

		public static T[] Rent(int length)
		{
			if (length == 0)
			{
				return Array.Empty<T>();
			}
			lock (poolLock)
			{
				if (!pool.TryGetValue(length, out var value))
				{
					value = new Stack<T[]>();
					pool.Add(length, value);
				}
				if (value.Count > 0)
				{
					return value.Pop();
				}
			}
			return new T[length];
		}

		public static void Return(T[] array)
		{
			if (array.Length == 0)
			{
				return;
			}
			lock (poolLock)
			{
				if (!pool.TryGetValue(array.Length, out var value))
				{
					value = new Stack<T[]>();
					pool.Add(array.Length, value);
				}
				value.Push(array);
			}
		}
	}
}
