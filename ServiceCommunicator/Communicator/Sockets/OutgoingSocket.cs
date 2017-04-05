using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common.Interfaces;

namespace Communication.Sockets
{
    /// <summary>
    /// Socket to connect service.
    /// </summary>
    public class OutgoingSocket : ISocketSender
    {
        private Socket _socket;

        public Guid ClientId { get; private set; }

        public void Connect(string ip, int port)
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._socket.Connect(ip, port);

            var state = new StateObject { WorkSocket = this._socket };
            this.StartReceive(state);
        }

        private void StartReceive(StateObject state)
        {
            try
            {
                // first call is sync action for receive ClientId.
                var serviceSocket = state.WorkSocket;
                var readSize = serviceSocket.Receive(state.Buffer);
                if (readSize != 16)
                {
                    throw new InvalidOperationException("Received packet size is invalid Client Id size.(Guid)");
                }

                var readBuffer = new byte[readSize];
                Array.Copy(state.Buffer, readBuffer, readSize);

                this.ClientId = new Guid(readBuffer);
                state.ClientId = this.ClientId;

                serviceSocket.BeginReceive(state.Buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, Communicator.ReceiveServiceCallback, state);
            }
            catch (Exception ex)
            {
                // TODO : need logging.
                Console.WriteLine(ex);
            }
        }

        public async Task<int> Send(byte[] packet, Guid clientId = default(Guid))
        {
            if (clientId == default(Guid))
            {
                // TODO : not connected. need logging.
                return 0;
            }

            return await this._socket.SendTaskAsync(packet);
        }

        public void Dispose()
        {
            if (this._socket == null)
            {
                // TODO : need logging.
                return;
            }

            this._socket.Dispose();
        }
    }
}
