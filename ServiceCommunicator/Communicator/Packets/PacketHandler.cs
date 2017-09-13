using Common.Communication;
using Mediator;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Communication.Packets
{
	/// <summary>
	/// Packet tokenizing and response from received Packet.
	/// </summary>
	class PacketHandler
    {
        private Socket _responseSocket;

        private InstanceMediator _mediator;

        private byte[] _buffer = new byte[BufferPool.Buffer1024Size * 10];

		private readonly byte[] _headerBuffer = new byte[PacketGenerator.HeaderUnitSize];

		private int _readPosition;

        private int _lastPosition;

        internal PacketHandler(Socket resposeSocket, InstanceMediator mediator)
        {
            this._responseSocket = resposeSocket;
            this._mediator = mediator;
        }

		[MethodImpl(MethodImplOptions.Synchronized)]
        public void HandlePackets(byte[] packet, int readBytes)
        {
            this._readPosition = 0;
			
            this.AddBuffer(packet, readBytes);

            // the last position has to larger than packet header size.
            if (this._lastPosition < PacketGenerator.HeaderSize)
            {
                // continue read packets from socket.
                return;
            }

            var needCompaction = false;

			while (this._readPosition < this._lastPosition)
			{
				try
				{
					Array.Clear(this._headerBuffer, 0, PacketGenerator.HeaderUnitSize);
					var needMorePacket = PacketGenerator.ParseAndExecute(
						ref this._readPosition, 
						this._buffer, 
						this._lastPosition, 
						this._headerBuffer,
						this._mediator, 
						this._responseSocket);

					if (needMorePacket)
					{
						break;
					}

					needCompaction = true;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					Array.Clear(this._buffer, 0, this._buffer.Length);
					throw;
				}
			}

			if (needCompaction)
            {
                this.CompactBuffer();
            }
        }
		
		private void AddBuffer(byte[] packet, int readBytes)
        {
            Buffer.BlockCopy(packet, 0, this._buffer, this._lastPosition, readBytes);
            this._lastPosition += readBytes;
        }

        private void CompactBuffer()
        {
            var remainSize = this._lastPosition - this._readPosition;
            var readPosition = this._readPosition;
            var lastPosition = this._lastPosition;

            this._readPosition = 0;
            this._lastPosition = remainSize;
            
            // read completed all packets.
            if (remainSize == 0)
            {
                Array.Clear(this._buffer, 0, lastPosition);
                return;
            }

            // buffer compaction.
            Buffer.BlockCopy(this._buffer, readPosition, this._buffer, 0, remainSize);
        }
		
		public void Dispose()
        {
            Array.Clear(this._buffer, 0, this._lastPosition);
            this._readPosition = 0;
            this._lastPosition = 0;

            this._buffer = null;

            // don't dispose, just set null;
            this._responseSocket = null;
            this._mediator = null;
        }
    }
}
