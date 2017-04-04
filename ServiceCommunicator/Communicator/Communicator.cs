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
		public OutgoingSocket _outgoing;

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

				// send
				var sender = clientId == default(Guid) ? (ISocketSender)this._outgoing : (ISocketSender)this._service;				
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

		internal static void ReceiveServiceCallback(IAsyncResult ar)
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
					var readBuffer = new byte[read];
					Array.Copy(stateObject.Buffer, readBuffer, read);

					var tcs = new TaskCompletionSource<bool>();
					_socketSycnContext.Post(d =>
					{
						tcs.SetResult(ResponseAwaits.MatchResponse(readBuffer));
					});

					tcs.Task.ContinueWith( t => 
					{
						var isServieCall = t.Result;
						if (isServieCall == false)
						{
							return;
						}

						var result = _mediator.Execute(readBuffer);
						SendResponse(socket, result);
					});
				}

				socket.BeginReceive(stateObject.Buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, ReceiveServiceCallback, stateObject);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				socket.Close();
			}
		}

		private static void SendResponse(Socket clientSocket, byte[] packet)
		{
			clientSocket.SendTaskAsync(packet);
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
