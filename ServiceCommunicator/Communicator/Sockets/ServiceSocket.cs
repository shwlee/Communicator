using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common.Interfaces;
using Communication.Packets;

namespace Communication.Sockets
{
    public class ServiceSocket : ISocketSender
    {
        private List<StateObject> _connectedClients = new List<StateObject>();
        private Socket _socket;

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
                var clientSocket = serviceSocket.EndAccept(ar);

                // clear old connection.
                this._connectedClients.RemoveAll(s =>
                {
                    try
                    {
                        if (s.WorkSocket.Connected == false)
                        {
                            return true;
                        }

                        return false;
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
                    Buffer = BufferPool.Instance.GetBuffer()
                };

                this._connectedClients.Add(state);

                Task.Factory.StartNew(() =>
                {
                    var clientIdPacket = state.ClientId.ToByteArray();
                    clientSocket.Send(clientIdPacket);
                    
                    Console.WriteLine("[Connect client] {0}", state.ClientId);

                    clientSocket.BeginReceive(state.Buffer, 0, BufferPool.BUFFER_SIZE, SocketFlags.None, Communicator.ReceiveServiceCallback, state);
                }, TaskCreationOptions.LongRunning);

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

        public async Task<int> Send(byte[] packet, Guid clientId = default(Guid))
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
