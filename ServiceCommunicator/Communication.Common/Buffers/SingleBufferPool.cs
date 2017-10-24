using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Communication.Common.Interfaces;

namespace Communication.Common.Buffers
{
	class SingleBufferPool : IBufferPool
	{
		// pool of buffers
		private readonly object _bufferSyncBlock = new object();

		private readonly List<InPoolBuffer> _bufferPool = new List<InPoolBuffer>();
		
		private readonly Timer _idleTimer = new	Timer();

		private readonly List<InPoolBuffer> _removingBuffers = new List<InPoolBuffer>();

		internal SingleBufferPool()
		{
			this._idleTimer.Interval = 1200000;
			this._idleTimer.Elapsed += this.IdleTimerOnElapsed;
			this._idleTimer.Start();
		}

		private void IdleTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			var timer = sender as Timer;
			timer?.Stop();

			lock (this._bufferSyncBlock)
			{
				this._removingBuffers.Clear();

				foreach (var inPoolBuffer in this._bufferPool)
				{
					if (inPoolBuffer.Mark)
					{
						this._removingBuffers.Add(inPoolBuffer);
						continue;
					}

					if (inPoolBuffer.IsUsing)
					{
						continue;
					}

					inPoolBuffer.Mark = true;
				}

				foreach (var removingBuffer in this._removingBuffers)
				{
					this._bufferPool.Remove(removingBuffer);
					removingBuffer.Dispose();
				}
			}

			timer?.Start();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public byte[] GetBuffer(int size)
		{
			lock (this._bufferSyncBlock)
			{
				byte[] buffer = null;
				foreach (var inPoolBuffer in this._bufferPool)
				{
					if (inPoolBuffer.IsUsing)
					{
						continue;
					}

					if (inPoolBuffer.Size != size)
					{
						continue;
					}

					buffer = inPoolBuffer.GetBuffer();
					break;
				}

				if (buffer == null)
				{
					var newInPoolBuffer = new InPoolBuffer(size);
					this._bufferPool.Add(newInPoolBuffer);
					buffer = newInPoolBuffer.GetBuffer();
				}

				return buffer;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void ReturnBuffer(byte[] buffer)
		{
			lock (this._bufferSyncBlock)
			{
				var inPoolBuffer = this._bufferPool.FirstOrDefault(b => b.Size == buffer.Length);
				if (inPoolBuffer == null)
				{
					// confuse!
					var newInPoolBuffer = new InPoolBuffer(buffer);
					this._bufferPool.Add(newInPoolBuffer);

					Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! size : " + buffer.Length);
					return;
				}

				inPoolBuffer.ReturnBuffer(buffer);
			}
		}
	}
}
