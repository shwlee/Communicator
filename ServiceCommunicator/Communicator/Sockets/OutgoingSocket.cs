using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common.Interfaces;

namespace Communication.Sockets
{
	public class OutgoingSocket : ISocketSender
	{
		private Socket _socket;

		public void Connect(string ip, int port)
		{
			this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this._socket.Connect(ip, port);

			var state = new StateObject { WorkSocket = this._socket };
			this._socket.BeginReceive(state.Buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, Communicator.ReceiveServiceCallback, state);
		}

		public async Task<int> Send(byte[] packet, Guid clientId = default(Guid))
		{
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
