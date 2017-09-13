using Define.Interfaces;
using System;

namespace Common.Communication
{
	public class BufferPool
	{
		private static readonly Lazy<IBufferPool> _singletonInstance = new Lazy<IBufferPool>(() => new SingleBufferPool());

		// Singleton attribute
		public static IBufferPool Instance => _singletonInstance.Value;

	    public const int Buffer1024Size = 1024;
	}
}
