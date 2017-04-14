using System;
using System.Net.Sockets;
using Common.Communication;
using Communication.Packets;

namespace Communication.Sockets
{
	class StateObject : IDisposable
	{
        internal Guid ClientId { get; set; }

        internal Socket WorkSocket { get; set; }
        
        internal byte[] Buffer { get; set; }

        internal PacketHandler PacketHandler { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.WorkSocket != null)
            {
                this.WorkSocket.Dispose();
                this.WorkSocket = null;
            }

            if (this.PacketHandler != null)
            {
                this.PacketHandler.Dispose();
            }

            if (this.Buffer != null)
            {
                BufferPool.Instance.ReturnBuffer(this.Buffer);
            }

            this.Buffer = null;
        }

        #endregion
    }
}
