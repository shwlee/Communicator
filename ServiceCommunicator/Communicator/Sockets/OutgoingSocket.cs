using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Sockets
{
	public class OutgoingSocket
	{
		private Socket _socket;

		public void Connect(string ip, int port)
		{
			this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this._socket.Connect(ip, port);

			var state = new StateObject { WorkSocket = this._socket };
			this._socket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, Communicator.ReceiveServiceCallback, state);
		}

		public void Send()
		{

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
