using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Threading;
using Communication.Hydrations;
using Communication.Sockets;
using Mediator;

namespace Communication
{
	public class Communicator
	{
		private InstanceMediator _mediator = new InstanceMediator();

		private ServiceSocket _service = new ServiceSocket();

		// allow only 1 connect
		public OutgoingSocket _outgoing;

		public void Initialize(params object[] instances)
		{
			this._mediator.SetInstance(instances);
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
				var hash =ProtocolHash.GetProtocolHash();
				ResponseAwaits.Insert(hash, tcs); // TODO : consider synchronization problem.
				
				// hydration			
				var packet = HydrateExpression.Get(method);
			
				// get Packet.
				var sendBytes = PacketHelper.GeneratePacket(packet.InterfaceName, packet.MethodName, packet.Argument, hash);

				// send
				if (clientId == default(Guid))
				{
					this._outgoing.Send();
				}
				else
				{
					this._service.Send(clientId);
				}

				// await receive response.
				var response = await tcs.Task;
				
				return PacketHelper.ParseArgument<TResult>(response, packet.GetType());
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
					Array.Copy(stateObject.buffer, readBuffer, read);
					MainSynchronizationContext.Post(d =>
					{
						ResponseAwaits.MatchResponse(readBuffer);
					});
				}

				socket.BeginReceive(stateObject.buffer, 0, StateObject.BUFFER_SIZE, SocketFlags.None, ReceiveServiceCallback, stateObject);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				socket.Close();
			}
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
		}
	}
}
