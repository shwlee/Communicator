using System;

namespace Common.Communication
{
	public class RawMessage
	{
		public int RequestHash { get; set; }

		public string InterfaceName { get; set; }

		public string MethodName { get; set; }
	}

	public class Message : RawMessage
	{
		public object Argument { get; set; }
	}

	public class Packet : IDisposable
	{
		public RawMessage Context { get; private set; }

		public byte[] ArgumentBuffer { get; private set; }

		public byte[] InBuffer { get; private set; }

		public Packet(RawMessage context, byte[] buffer, int startIndex, int size, byte[] argBytes)
		{
			this.Context = context;
			var inBuffer = BufferPool.Instance.GetBuffer(buffer.Length);
			Buffer.BlockCopy(buffer, startIndex, inBuffer, 0, size);

			this.InBuffer = inBuffer;
			this.ArgumentBuffer = argBytes;
		}

		public void Dispose()
		{
			if (this.InBuffer != null)
			{
				Array.Clear(this.InBuffer, 0, this.InBuffer.Length);
				BufferPool.Instance.ReturnBuffer(this.InBuffer);

				this.InBuffer = null;
			}

			this.Context = null;
		}
	}
}
