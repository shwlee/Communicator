using System;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common.Communication;
using Common.Interfaces;
using Communication.AsyncResponse;
using Communication.Hydrations;
using Communication.Packets;
using Communication.Sockets;
using Mediator;

namespace Communication
{
    public class Communicator
    {
        private static InstanceMediator _mediator = new InstanceMediator();

        private ServiceSocket _service;

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
            this._service = new ServiceSocket();
            this._service.StartService(port, backlog);
        }

        public void ConnectToService(string ip, int port)
        {
            this._outgoing = new OutgoingSocket();
            this._outgoing.Connect(ip, port);
        }

		// if you want to synchronized send, call Result property by this return instance.
        public async Task<TResult> SendAsync<TInterface, TResult>(Expression<Func<TInterface, TResult>> method, Guid clientId = default(Guid), PacketDirection direction = PacketDirection.Outgoing)
            where TInterface : class
            where TResult : class
        {
			// get protocol hash and TaskCompletionSource save.
			var tcs = new TaskCompletionSource<byte[]>();
			var hash = ProtocolHash.GetProtocolHash();

			ResponseAwaits.Insert(hash, tcs); // TODO : consider synchronization problem.

			// hydration			
			var packet = HydrateExpression.Get(method);
			packet.RequestHash = hash;

			// get Packet.
			var sendBytes = PacketGenerator.GeneratePacket(packet);

			// check sender
			// if passed client id, send to service; not passed client id, service response or push to client.
			var sender = direction == PacketDirection.Outgoing ? (ISocketSender)this._outgoing : this._service;
			if (sender == null)
			{
				// disconnected or not connect yet.
				return null; // throw exception?
			}
			
			var sendPacketLength = await sender.SendAsync(sendBytes, clientId);
			BufferPool.Instance.ReturnBuffer(sendBytes);

			if (sendPacketLength == 0)
			{
				// disconnected.
				return null; // throw exception?
			}
			
			// await receive response.
			var response = await tcs.Task;

			var args = PacketGenerator.ParseArgument<TResult>(response);

			BufferPool.Instance.ReturnBuffer(response);

			return args;
		}

        internal static void ReceiveServiceCallback(IAsyncResult ar)
        {
            InternalCallback(ar, PacketDirection.Incomming);
        }

        internal static void ReceiveResponseCallback(IAsyncResult ar)
        {
            InternalCallback(ar, PacketDirection.Outgoing);
        }

        private static void InternalCallback(IAsyncResult ar, PacketDirection direction)
        {
	        Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~ Thread Id : " + Thread.CurrentThread.ManagedThreadId + " , IsPool : " + Thread.CurrentThread.IsThreadPoolThread);
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
                    // TODO : need packet logging.

                    Console.WriteLine("[Received] Read Packets : {0}", read);
                    
                    if (stateObject.PacketHandler == null)
                    {
                        stateObject.PacketHandler = new PacketHandler(socket, _mediator);
                    }

                    stateObject.PacketHandler.HandlePackets(stateObject.Buffer, read);
                }

				BufferPool.Instance.ReturnBuffer(stateObject.Buffer);

                stateObject.Buffer = null;

                var callback = direction == PacketDirection.Incomming ? 
                    (AsyncCallback)ReceiveServiceCallback : ReceiveResponseCallback;

				stateObject.Buffer = BufferPool.Instance.GetBuffer(BufferPool.Buffer1024Size);

				socket.BeginReceive(stateObject.Buffer, 0, BufferPool.Buffer1024Size, SocketFlags.None, callback, stateObject);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("{0} Socket Disposed! {1}", direction, stateObject.ClientId);
                stateObject.Dispose();
                Console.WriteLine();
            }
            catch (SocketException se)
            {
                if (se.ErrorCode == 10054)
                {
                    // TODO : add to connection close message and handle to socket management
                    Console.WriteLine("{0} Socket Closed! {1}", direction, stateObject.ClientId);
                    stateObject.Dispose();
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                socket.Close();
                Console.WriteLine();
            }
        }
        
        public void Dispose()
        {
	        this._service?.Dispose();

	        this._outgoing?.Dispose();
        }
    }
}
