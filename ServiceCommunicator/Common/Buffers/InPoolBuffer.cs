using System;

namespace Communication.Common.Buffers
{
	class InPoolBuffer : IDisposable
	{
		private bool _isDisposed;

		private byte[] _buffer;

		private bool _isUsing;

		public bool Mark { get; set; }

		public int Size { get; }

		public bool IsUsing
		{
			get { return this._isUsing; }
			private set
			{
				if (value)
				{
					this.Mark = false;
				}

				this._isUsing = value;
			}
		}

		public InPoolBuffer(byte[] buffer)
		{
			this._buffer = buffer;
			this.Size = buffer.Length;
		}

		public InPoolBuffer(int size)
		{
			this._buffer = new byte[size];
			this.Size = size;
		}
		
		private byte[] GetBufferInner()
		{
			if (this._isDisposed)
			{
				throw new ObjectDisposedException(this.ToString());
			}

			var returnBuffer = this._buffer;

			this._buffer = null;

			return returnBuffer;
		}

		public byte[] GetBuffer()
		{
			lock (this)
			{
				var returnBuffer = this.GetBufferInner();

				this.IsUsing = true;

				return returnBuffer;
			}
		}

		public void ReturnBuffer(byte[] buffer)
		{
			lock (this)
			{
				if (this._isDisposed)
				{
					throw new ObjectDisposedException(this.ToString());
				}

				this._buffer = buffer;

				this.IsUsing = false;
			}
		}

		public void Dispose()
		{
			lock (this)
			{
				if (this.Mark == false)
				{
					throw new ObjectDisposedException(this.ToString(), "There was tried dispoing the keeped buffer.");
				}
				
				this._buffer = null;

				this._isDisposed = true;
			}
		}
	}
}
