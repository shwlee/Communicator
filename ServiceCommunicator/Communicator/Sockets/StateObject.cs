using System;
using System.Net.Sockets;
using Communication.Common.Buffers;
using Communication.Common.Interfaces;
using Communication.Core.Packets;

namespace Communication.Core.Sockets
{
	class StateObject : IStateObject
	{
        public Guid ClientId { get; set; }

        internal Socket WorkSocket { get; set; }
        
        internal byte[] Buffer { get; set; }

        internal PacketHandler PacketHandler { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
	        this.WorkSocket?.Dispose();
	        this.PacketHandler?.Dispose();

	        if (this.Buffer != null)
            {
                BufferPool.Instance.ReturnBuffer(this.Buffer);
            }

            this.Buffer = null;
			this.WorkSocket = null;
	        this.PacketHandler = null;
        }

		#endregion
	}
}
