using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Communication.Common.Buffers;
using Communication.Common.Interfaces;

namespace Communication.Core.Sockets
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

            var state = new StateObject
            {
                WorkSocket = this._socket,
				Buffer = BufferPool.Instance.GetBuffer(BufferPool.Buffer1024Size)
			};
            this.StartReceive(state);
        }

	    public async Task ConnectAsync(string ip, int port)
	    {
			this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			await this._socket.ConnectToAsync(ip, port);

			var state = new StateObject
			{
				WorkSocket = this._socket,
				Buffer = BufferPool.Instance.GetBuffer(BufferPool.Buffer1024Size)
			};
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

                serviceSocket.BeginReceive(state.Buffer, 0, BufferPool.Buffer1024Size, SocketFlags.None, Communicator.ReceiveCallback, state);
            }
            catch (Exception ex)
            {
                // TODO : need logging.
                Console.WriteLine(ex);
				state.Dispose();
			}
        }

        public async Task<int> SendAsync(byte[] packet, Guid clientId = default(Guid)) // no need client id in client side.
        {
            return await this._socket.SendPacketAsync(packet);
        }

	    public int Send(byte[] packet, Guid clientId = new Guid()) // no need client id in client side.
		{
			return this._socket.Send(packet, 0, packet.Length, SocketFlags.None);
	    }

		public void Disconnect(IStateObject state)
		{
			// send disconnect message.

			this.Dispose();
		}

		public void Dispose()
        {
            if (this._socket == null)
            {
                // TODO : need logging.
                return;
            }

			try
			{
				this._socket.Dispose();
				this._socket = null;
			}
			catch (ObjectDisposedException dex)
			{
				Console.WriteLine("Already disposed.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
        }
    }
}
