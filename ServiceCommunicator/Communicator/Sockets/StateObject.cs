using System;
using System.Net.Sockets;

namespace Communication.Sockets
{
	class StateObject : IDisposable
	{
        internal Guid ClientId { get; set; }

        internal Socket WorkSocket { get; set; }

        internal const int BUFFER_SIZE = 2048;

        internal byte[] Buffer = new byte[BUFFER_SIZE];

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

            this.Buffer = null;
        }

        #endregion
    }
}
