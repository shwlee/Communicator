using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;
using Communication.Common.Buffers;
using Communication.Common.Interfaces;
using Communication.Common.Packets;
using Communication.Core.AsyncResponse;
using Communication.Core.Hydrations;
using Communication.Core.Packets;

namespace Communication.Core.Proxy
{
	public class AsyncServiceProxy<T> : RealProxy, IAsyncProxy<T>, IDisposable
	{
		private T _proxy;

		private ISocketSender _socket;

		private Message _message;

		public AsyncServiceProxy(ISocketSender socket) : base(typeof(T))
		{
			this._socket = socket;
		}

		public override IMessage Invoke(IMessage msg)
		{
			var message = msg as IMethodCallMessage;
			if (message?.MethodBase.DeclaringType == null)
			{
				return null;
			}

			try
			{
				this._message = null;

				if (message.MethodBase.DeclaringType == null)
				{
					throw new NullReferenceException("Service interface type is null.");
				}

				var packet = new Message
				{
					InterfaceName = message.MethodBase.DeclaringType.Name,
					MethodName = message.MethodName,
					Argument = message.InArgs[0]
				};

				this._message = packet;

				return new ReturnMessage(null, null, 0, message.LogicalCallContext, message);
			}
			catch (Exception ex)
			{
				return new ReturnMessage(ex, message);
			}
		}

		public async Task<TResult> CallAsync<TResult>(Func<T, TResult> func, Guid clientId = default(Guid)) 
			where TResult : class
		{
			Message message = null;

			lock (this)
			{
				if (this._proxy == null)
				{
					this._proxy = (T)this.GetTransparentProxy();
				}

				func(this._proxy);
				message = this._message;
			}

			if (message == null)
			{
				throw new NullReferenceException();
			}

			var tcs = new TaskCompletionSource<byte[]>();
			var hash = ProtocolHash.GetProtocolHash();

			ResponseAwaits.Insert(hash, tcs); // TODO : consider synchronization problem.

			message.RequestHash = hash;

			// get Packet.
			var sendBytes = PacketGenerator.GeneratePacket(message);

			// check sender
			if (this._socket == null)
			{
				// disconnected or not connect yet.
				throw new NullReferenceException("Socket sender is null");
			}

			Debug.WriteLine("[shwlee] name : " + typeof(TResult));

			var sendPacketLength = await this._socket.SendAsync(sendBytes, clientId);
			BufferPool.Instance.ReturnBuffer(sendBytes);
			
			if (sendPacketLength == 0)
			{
				// disconnected.
				throw new ObjectDisposedException("Connection is disposed or disconnected.");
			}

			// receive response.
			var response = await tcs.Task;
			
			var args = PacketGenerator.ParseArgument<TResult>(response);

			BufferPool.Instance.ReturnBuffer(response);

			return args;
		}

		public bool IsMatchType<TResult>()
		{
			return typeof (T) == typeof (TResult);
		}

		public void Dispose()
		{
			this._socket = null;
		}
	}
}
