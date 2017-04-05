using System;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Threading;
using Communication.AsyncResponse;
using Communication.Hydrations;
using Communication.Sockets;
using Mediator;

namespace Communication
{
    public class Communicator
    {
        private static StaSynchronizationContext _socketSycnContext = new StaSynchronizationContext("SocketSynchronizationContext");

        private static InstanceMediator _mediator = new InstanceMediator();

        private ServiceSocket _service = new ServiceSocket();

        // allow only 1 connect
        private OutgoingSocket _outgoing;

        public Guid ClientId
        {
            get
            {
                if (this._outgoing == null)
                {
                    return default(Guid);
                }

                return this._outgoing.ClientId;
            }
        }

        public void Initialize(params object[] instances)
        {
            _mediator.SetInstance(instances);
        }

        public void StartService(int port, int backlog)
        {
            this._service.StartService(port, backlog);
        }

        public void ConnectToService(string ip, int port)
        {
            this._outgoing = new OutgoingSocket();
            this._outgoing.Connect(ip, port);
        }

        public async Task<TResult> Send<TInterface, TResult>(Expression<Func<TInterface, TResult>> method, Guid clientId = default(Guid))
            where TInterface : class
            where TResult : class
        {
            return await Task.Run(async () =>
            {
                // get protocol hash and TaskCompletionSource save.
                var tcs = new TaskCompletionSource<byte[]>();
                var hash = ProtocolHash.GetProtocolHash();
                _socketSycnContext.Send(d =>
                {
                    ResponseAwaits.Insert(hash, tcs); // TODO : consider synchronization problem.
                });

                // hydration			
                var packet = HydrateExpression.Get(method);
                packet.RequestHash = hash;

                // get Packet.
                var sendBytes = PacketGenerator.GeneratePacket(packet);

                // check sender
                // if passed client id, send to service; not passed client id, service response or push to client.
                var sender = clientId == default(Guid) ? (ISocketSender)this._service : this._outgoing;
                if (sender == null)
                {
                    // disconnected or not connect yet.
                    return null; // throw exception?
                }

                var sendPacketLength = await sender.Send(sendBytes, clientId);
                if (sendPacketLength == 0)
                {
                    // disconnected.
                    return null; // throw exception?
                }

                // await receive response.
                var response = await tcs.Task;

                return _mediator.ParseArgument<TResult>(response);
            });
        }

        internal static async void ReceiveServiceCallback(IAsyncResult ar)
        {
            var stateObject = ar.AsyncState as StateObject;
            if (stateObject == null)
            {
                return;
            }

            var socket = stateObject.WorkSocket;
            try
            {
                var read = socket.EndReceive(ar);
                if (read > 0)
                {
                    await Task.Run(async () =>
                    {
                        var readBuffer = new byte[read];
                        Array.Copy(stateObject.Buffer, readBuffer, read);

                        var tcs = new TaskCompletionSource<bool>();
                        _socketSycnContext.Post(d =>
                        {
                            tcs.SetResult(ResponseAwaits.MatchResponse(readBuffer));
                        });

                        var isServiceCall = await tcs.Task;
                        if (isServiceCall == false)
                        {
                            return;
                        }

                        var result = _mediator.Execute(readBuffer);
                        await SendResponse(socket, result);
                    });
                }

                socket.BeginReceive(stateObject.Buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None,
                    ReceiveServiceCallback, stateObject);
            }
            catch (ObjectDisposedException dex)
            {
                Console.WriteLine("Socket Closed! {0}", stateObject.ClientId);
            }
            catch (SocketException se)
            {
                if (se.ErrorCode == 10054)
                {
                    // TODO : add to connection close message and handle to socket management
                    Console.WriteLine("Socket Closed! {0}", stateObject.ClientId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                socket.Close();
            }
        }

        private static async Task SendResponse(Socket clientSocket, byte[] packet)
        {
            await clientSocket.SendTaskAsync(packet);
        }

        public void Dispose()
        {
            if (this._service != null)
            {
                this._service.Dispose();
            }

            if (this._outgoing != null)
            {
                this._outgoing.Dispose();
            }

            _socketSycnContext.Dispose();
        }
    }
}
