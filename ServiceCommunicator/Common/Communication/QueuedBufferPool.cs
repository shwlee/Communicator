using Define.Interfaces;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Common.Communication
{
	class QueuedBufferPool : IBufferPool
	{
		private const int Inital64PoolSize = 512;
		private const int Inital128PoolSize = 256;
		private const int Inital512PoolSize = 128;
		private const int Inital1024PoolSize = 64;
		private const int Inital2048PoolSize = 32;
		private const int Inital4096PoolSize = 32;
		private const int Inital1MPoolSize = 16;

		public const int Buffer64Size = 64;
		public const int Buffer128Size = 128;
		public const int Buffer512Size = 512;
		public const int Buffer1024Size = 1024;
		public const int Buffer2048Size = 2048;
		public const int Buffer4096Size = 4096;
		public const int Buffer1MSize = 1048576;
		
		// pool of buffers
		private Queue<byte[]> _free64Buffers;
		private Queue<byte[]> _free128Buffers;
		private Queue<byte[]> _free512Buffers;
		private Queue<byte[]> _free1024Buffers;
		private Queue<byte[]> _free2048Buffers;
		private Queue<byte[]> _free4096Buffers;
		private Queue<byte[]> _free1MBuffers;

		internal QueuedBufferPool()
		{
			this.InitBuffers();
		}

		private void InitBuffers()
		{
			this._free64Buffers = new Queue<byte[]>(Inital64PoolSize);
			for (var i = 0; i < Inital64PoolSize; i++)
			{
				this._free64Buffers.Enqueue(new byte[Buffer64Size]);
			}

			this._free128Buffers = new Queue<byte[]>(Inital128PoolSize);
			for (var i = 0; i < Inital128PoolSize; i++)
			{
				this._free128Buffers.Enqueue(new byte[Buffer128Size]);
			}

			this._free512Buffers = new Queue<byte[]>(Inital512PoolSize);
			for (var i = 0; i < Inital512PoolSize; i++)
			{
				this._free512Buffers.Enqueue(new byte[Buffer512Size]);
			}

			this._free1024Buffers = new Queue<byte[]>(Inital1024PoolSize);
			for (var i = 0; i < Inital1024PoolSize; i++)
			{
				this._free1024Buffers.Enqueue(new byte[Buffer1024Size]);
			}

			this._free2048Buffers = new Queue<byte[]>(Inital2048PoolSize);
			for (var i = 0; i < Inital2048PoolSize; i++)
			{
				this._free2048Buffers.Enqueue(new byte[Buffer2048Size]);
			}

			this._free4096Buffers = new Queue<byte[]>(Inital4096PoolSize);
			for (var i = 0; i < Inital4096PoolSize; i++)
			{
				this._free4096Buffers.Enqueue(new byte[Buffer4096Size]);
			}

			this._free1MBuffers = new Queue<byte[]>(Inital1MPoolSize);
			for (var i = 0; i < Inital1MPoolSize; i++)
			{
				this._free1MBuffers.Enqueue(new byte[Buffer1MSize]);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public byte[] GetBuffer(int size)
		{
			var freeBuffer = this.GetFreeBuffer(size);

			if (freeBuffer.Count > 0)
			{
				return freeBuffer.Dequeue();
			}

			// instead of creating new buffer,
			// blocking waiting or refusing request may be better
			var bufferSize = this.GetBufferSize(size);
			var extensionBuffer = new byte[bufferSize];
			freeBuffer.Enqueue(extensionBuffer);

			return extensionBuffer;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ReturnBuffer(byte[] buffer)
		{
			var returnBuffer = this.GetFreeBuffer(buffer.Length);
			returnBuffer.Enqueue(buffer);
		}

		private Queue<byte[]> GetFreeBuffer(int size)
		{
			Queue<byte[]> buffer = null;

			if (size <= Buffer64Size)
			{
				buffer = this._free64Buffers;
			}
			else if (size <= Buffer128Size)
			{
				buffer = this._free128Buffers;
			}
			else if (size <= Buffer512Size)
			{
				buffer = this._free512Buffers;
			}
			else if (size <= Buffer1024Size)
			{
				buffer = this._free1024Buffers;
			}
			else if (size <= Buffer2048Size)
			{
				buffer = this._free2048Buffers;
			}
			else if (size <= Buffer4096Size)
			{
				buffer = this._free4096Buffers;
			}
			else
			{
				buffer = this._free1MBuffers;
			}

			return buffer;
		}

		private int GetBufferSize(int size)
		{
			var bufferSize = 0;

			if (size <= Buffer64Size)
			{
				bufferSize = Buffer64Size;
			}
			else if (size <= Buffer128Size)
			{
				bufferSize = Buffer128Size;
			}
			else if (size <= Buffer512Size)
			{
				bufferSize = Buffer512Size;
			}
			else if (size <= Buffer1024Size)
			{
				bufferSize = Buffer1024Size;
			}
			else if (size <= Buffer2048Size)
			{
				bufferSize = Buffer2048Size;
			}
			else if (size <= Buffer4096Size)
			{
				bufferSize = Buffer4096Size;
			}
			else
			{
				bufferSize = Buffer1MSize;
			}

			return bufferSize;
		}
	}
}
