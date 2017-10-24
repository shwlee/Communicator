using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Communication.Common.Buffers;
using Communication.Common.Interfaces;

namespace Communication.Core.Sockets
{
	public class ServiceSocket : ISocketSender
    {
        private readonly List<StateObject> _connectedClients = new List<StateObject>();
		
		private Socket _socket;

		public List<Guid> ConnectedClients => this._connectedClients.Select(c => c.ClientId).ToList();

		public Action AuthenticationMethod { get; set; }

		public void StartService(int port, int backlog)
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, port);

            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._socket.Bind(ipEndpoint);
            this._socket.Listen(backlog);
            this._socket.BeginAccept(this.OnAccept, this._socket);
        }

        private void OnAccept(IAsyncResult ar)
        {
            var serviceSocket = ar.AsyncState as Socket;
            if (serviceSocket == null)
            {
                // TODO : need logging.
                return;
            }

            try
            {
	            lock (this)
	            {
					var clientSocket = serviceSocket.EndAccept(ar);

					// clear old connection.
					this._connectedClients.RemoveAll(s =>
					{
						try
						{
							return s.WorkSocket.Connected == false;
						}
						catch
						{
							return true;
						}
					});

					var alreadyConnected = this._connectedClients.FirstOrDefault(s => s.WorkSocket.RemoteEndPoint.Equals(clientSocket.RemoteEndPoint));
					if (alreadyConnected != null)
					{
						alreadyConnected.WorkSocket.Disconnect(false);
						alreadyConnected.WorkSocket.Dispose();
						alreadyConnected.Buffer = null;
						this._connectedClients.Remove(alreadyConnected);
					}

					var state = new StateObject
					{
						ClientId = Guid.NewGuid(),
						WorkSocket = clientSocket,
						Buffer = BufferPool.Instance.GetBuffer(BufferPool.Buffer1024Size)
					};

					this._connectedClients.Add(state);
					
					// connection management in communication module.
					Task.Run(() =>
					{
						// TODO : need verification proeccess for client auth.
						var clientIdPacket = state.ClientId.ToByteArray();
						clientSocket.Send(clientIdPacket);

						Console.WriteLine("[Connect client] Process thread : {0}, ClientId : {1}", Thread.CurrentThread.ManagedThreadId, state.ClientId);

						this.AuthenticationMethod?.Invoke();

						clientSocket.BeginReceive(state.Buffer, 0, BufferPool.Buffer1024Size, SocketFlags.None, Communicator.ReceiveCallback, state);
					});
				}

                serviceSocket.BeginAccept(this.OnAccept, serviceSocket);
            }
            catch (ObjectDisposedException dex)
            {
                Console.WriteLine("Socket Closed!");
                Console.WriteLine();
            }
            catch (SocketException se)
            {
                if (se.ErrorCode == 10054)
                {
                    // TODO : add to connection close message and handle to socket management
                    Console.WriteLine("Socket Closed! ");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<int> SendAsync(byte[] packet, Guid clientId = default(Guid))
        {
            var connectedClient = this._connectedClients.FirstOrDefault(s => s.ClientId.Equals(clientId));
            if (connectedClient == null)
            {
                // TODO : need logging.
                return 0;
            }

            var clientSocket = connectedClient.WorkSocket;
            return await clientSocket.SendPacketAsync(packet);
        }

	    public int Send(byte[] packet, Guid clientId = default(Guid))
	    {
			var connectedClient = this._connectedClients.FirstOrDefault(s => s.ClientId.Equals(clientId));
			if (connectedClient == null)
			{
				// TODO : need logging.
				return 0;
			}

			var clientSocket = connectedClient.WorkSocket;
			return clientSocket.Send(packet, 0, packet.Length, SocketFlags.None);
		}

		public void Disconnect(IStateObject state)
		{
			var client = this._connectedClients.FirstOrDefault(s => s.ClientId == state.ClientId);
			if (client == null)
			{
				return;
			}

			client.Dispose();
			this._connectedClients.Remove(client);
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
