using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Threading;

namespace Communication.Sockets
{
	public class ServiceSocket
	{
		private List<StateObject> _connectedClients = new List<StateObject>();
		private Socket _socket;

		public void StartService(int port, int backlog)
		{
			var ipEndpoint = new IPEndPoint(IPAddress.Any, port);

			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.Bind(ipEndpoint);
			_socket.Listen(backlog);
			_socket.BeginAccept(OnAccept, _socket);
		}

		private void OnAccept(IAsyncResult ar)
		{
			var serviceSocket = ar.AsyncState as Socket;
			if (serviceSocket == null)
			{
				// TODO : need logging.
				return;
			}

			var clientSocket = serviceSocket.EndAccept(ar);

			// clear old connection.
			var alreadyConnected = this._connectedClients.FirstOrDefault(s => s.WorkSocket.LocalEndPoint.Equals(clientSocket.LocalEndPoint));
			if (alreadyConnected != null)
			{
				alreadyConnected.WorkSocket.Disconnect(false);
				alreadyConnected.WorkSocket.Dispose();
				alreadyConnected.buffer = null;
				this._connectedClients.Remove(alreadyConnected);
			}

			var state = new StateObject { WorkSocket = clientSocket };
			this._connectedClients.Add(state);

			clientSocket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, Communicator.ReceiveServiceCallback, state);

			serviceSocket.BeginAccept(OnAccept, serviceSocket);
		}		

		public void Send(Guid clientId)
		{

		}

		public void StopService()
		{
			if (this._socket == null)
			{
				// TODO : need logging.
				return;
			}

			this._socket.Dispose();
		}

		public void Dispose()
		{
			this.StopService();
			this._socket = null;
		}
	}
}
